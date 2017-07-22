using System;
using MySqlCockroachBench.Commands;
using MySqlCockroachBench.Db;

namespace MySqlCockroachBench
{
	public class Program
	{
		public static void Main(string[] args)
		{
			using (var mySqlDb = new MySqlDb())
            {
                mySqlDb.Connection.Open();
                mySqlDb.Initialize();
            }
            using (var cockroachDb = new CockroachDb())
            {
                cockroachDb.Connection.Open();
                cockroachDb.Initialize();
            }
			
			Environment.Exit(CommandRunner.Run(args));
		}
	}
}
