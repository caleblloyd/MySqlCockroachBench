using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using MySqlCockroachBench.Db;

namespace MySqlCockroachBench.Models
{
	public class BlogPostQuery
	{

		public readonly IAppDb Db;

		private static Random Rng = new Random();
		
		public BlogPostQuery(IAppDb db)
		{
			Db = db;
		}

		public BlogPost FindOne(long id)
		{
			var result = ReadAll(FindOneCmd(id).ExecuteReader());
			return result.Count > 0 ? result[0] : null;
		}

		public async Task<BlogPost> FindOneAsync(long id)
		{
			var result = await ReadAllAsync(await FindOneCmd(id).ExecuteReaderAsync());
			return result.Count > 0 ? result[0] : null;
		}

		public List<BlogPost> Random10(int max)
		{
			return ReadAll(FindManyCommand(10, Rng.Next(0, max - 10)).ExecuteReader());
		}

		public async Task<List<BlogPost>> Random10Async(int max)
		{
			return await ReadAllAsync(await FindManyCommand(10, Rng.Next(0, max)).ExecuteReaderAsync());
		}

		public void DeleteAll()
		{
			var txn = Db.Connection.BeginTransaction();
			try
			{
				DeleteAllCmd().ExecuteNonQuery();
				txn.Commit();
			}
			catch
			{
				txn.Rollback();
				throw;
			}
		}

		private DbCommand FindOneCmd(long id)
		{
			var cmd = Db.Connection.CreateCommand();
			cmd.CommandText = @"SELECT Id, Title, Content FROM BlogPost WHERE Id = @id";
            var parameter = cmd.CreateParameter();
            parameter.ParameterName = "@id";
            parameter.Value = id;
			cmd.Parameters.Add(parameter);
			return cmd;
		}

		public DbCommand FindManyCommand(int limit, int offset)
		{
			var cmd = Db.Connection.CreateCommand();
			cmd.CommandText = @"SELECT Id, Title, Content FROM BlogPost ORDER BY Id DESC LIMIT @limit OFFSET @offset;";
			var parameter = cmd.CreateParameter();
            parameter.ParameterName = "@limit";
            parameter.Value = limit;
			cmd.Parameters.Add(parameter);
			parameter = cmd.CreateParameter();
            parameter.ParameterName = "@offset";
            parameter.Value = offset;
			cmd.Parameters.Add(parameter);
			return cmd;
		}

		private DbCommand DeleteAllCmd()
		{
			var cmd = Db.Connection.CreateCommand();
			cmd.CommandText = @"DELETE FROM BlogPost";
			return cmd;
		}

		private List<BlogPost> ReadAll(DbDataReader reader)
		{
			var posts = new List<BlogPost>();
			using (reader)
			{
				while (reader.Read())
				{
					var post = new BlogPost(Db)
					{
						Id = reader.GetFieldValue<long>(0),
						Title = reader.GetFieldValue<string>(1),
						Content = reader.GetFieldValue<string>(2)
					};
					posts.Add(post);
				}
			}
			return posts;
		}

		private async Task<List<BlogPost>> ReadAllAsync(DbDataReader reader)
		{
			var posts = new List<BlogPost>();
			using (reader)
			{
				while (await reader.ReadAsync())
				{
					var post = new BlogPost(Db)
					{
						Id = await reader.GetFieldValueAsync<long>(0),
						Title = await reader.GetFieldValueAsync<string>(1),
						Content = await reader.GetFieldValueAsync<string>(2)
					};
					posts.Add(post);
				}
			}
			return posts;
		}
	}
}
