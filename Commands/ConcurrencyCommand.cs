using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MySqlCockroachBench.Db;
using MySqlCockroachBench.Models;

namespace MySqlCockroachBench.Commands
{
	public static class ConcurrencyCommand
	{
		private static Random Rng = new Random();

		public static void Run(IAppDb appDb, int iterations, int concurrency, int ops)
		{
			var recordNum = 0;
			async Task InsertOne(IAppDb db)
			{
				var blog = new BlogPost(db)
				{
					Title = "Title " + Interlocked.Increment(ref recordNum),
					Content = "Content " + recordNum
				};
				await blog.InsertAsync();
			}

			var selected = new ConcurrentQueue<BlogPost>();
			async Task SelectTen(IAppDb db)
			{
				var blogPosts = await (new BlogPostQuery(db)).Random10Async(recordNum);
				selected.Enqueue(blogPosts.FirstOrDefault());
			}

			var updatedNum = 0;
			async Task UpdateOne(IAppDb db)
			{
				BlogPost blogPost;
				if (selected.TryDequeue(out blogPost) && blogPost != null)
				{
					blogPost.Db = db;
					blogPost.Content = "Content Updated " + Interlocked.Increment(ref updatedNum);
					await blogPost.UpdateAsync();
				}
			}

			using (var db = appDb.New())
			{
				db.Connection.Open();
				using (var cmd = db.Connection.CreateCommand())
				{
					cmd.CommandText = "DELETE FROM BlogPost";
					cmd.ExecuteNonQuery();
				}
			}

			PerfTest(appDb, InsertOne, "Insert One", iterations, concurrency, ops).GetAwaiter().GetResult();
			using (var db = appDb.New())
			{
				db.Connection.Open();
				using (var cmd = db.Connection.CreateCommand())
				{
					cmd.CommandText = "SELECT COUNT(*) FROM BlogPost";
					Console.WriteLine("Records Inserted: " + cmd.ExecuteScalar());
					Console.WriteLine();
				}
			}

			PerfTest(appDb, SelectTen, "Select Ten", iterations, concurrency, ops).GetAwaiter().GetResult();
			Console.WriteLine("Records Selected: " + selected.Count * 10);
			BlogPost firstRecord;
			if (selected.TryPeek(out firstRecord))
				Console.WriteLine("First Record: " + firstRecord.Content);
			Console.WriteLine();

			PerfTest(appDb, UpdateOne, "Update One", iterations, concurrency, ops).GetAwaiter().GetResult();
			Console.WriteLine("Records Updated: " + updatedNum);
			using (var db = appDb.New())
			{
				db.Connection.Open();
				var firstRecordUpdated = new BlogPostQuery(db).FindOne(firstRecord.Id);
				if (firstRecordUpdated != null)
					Console.WriteLine("First Record: " + firstRecordUpdated.Content);
			}
		}

		public static async Task PerfTest(IAppDb appDb, Func<IAppDb, Task> test, string testName, int iterations, int concurrency, int ops)
		{
			var timers = new List<TimeSpan>();
			for (var iteration = 0; iteration < iterations; iteration++)
			{
				var tasks = new List<Task>();
				var start = DateTime.UtcNow;
				for (var connection = 0; connection < concurrency; connection++)
				{
					tasks.Add(ConnectionTask(appDb, test, ops));
				}
				await Task.WhenAll(tasks);
				timers.Add(DateTime.UtcNow - start);
			}
			Console.WriteLine("Test                     " + testName);
			Console.WriteLine("Iterations:              " + iterations);
			Console.WriteLine("Concurrency:             " + concurrency);
			Console.WriteLine("Operations:              " + ops);
			Console.WriteLine("Times (Min, Average, Max) "
							  + timers.Min() + ", "
							  + TimeSpan.FromTicks(timers.Sum(timer => timer.Ticks) / timers.Count) + ", "
							  + timers.Max());
			Console.WriteLine();
		}

		private static async Task ConnectionTask(IAppDb appDb, Func<IAppDb, Task> cb, int ops)
		{
			using (var db = appDb.New())
			{
				await db.Connection.OpenAsync();
				for (var op = 0; op < ops; op++)
				{
					await cb(db);
				}
			}
		}

	}
}
