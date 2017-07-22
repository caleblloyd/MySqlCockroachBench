using System;
using System.Data.Common;
using MySqlCockroachBench;
using Npgsql;

namespace MySqlCockroachBench.Db
{
	public class CockroachDb : IAppDb
	{
		public CockroachDb()
		{
			Connection = new NpgsqlConnection(AppConfig.Config["Data:CockroachDbConnectionString"]);
		}

		public IAppDb New(){
			return new CockroachDb();
		}

        public DbConnection Connection { get; private set; }

        public void Initialize()
		{
            using (var cmd = Connection.CreateCommand())
            {
                cmd.CommandText = @"
DROP TABLE IF EXISTS BlogPost;
CREATE TABLE IF NOT EXISTS BlogPost (
    Id SERIAL PRIMARY KEY,
    Content STRING,
    Title STRING
);
";
				cmd.ExecuteNonQuery();
            }
		}

		public void Dispose()
		{
			Connection.Close();
		}
	}
}
