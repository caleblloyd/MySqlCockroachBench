using System;
using System.Data.Common;
using MySql.Data.MySqlClient;
using MySqlCockroachBench;

namespace MySqlCockroachBench.Db
{
	public class MySqlDb : IAppDb
	{
		public MySqlDb()
		{
			Connection = new MySqlConnection(AppConfig.Config["Data:MySqlConnectionString"]);
		}

		public IAppDb New(){
			return new MySqlDb();
		}

        public DbConnection Connection { get; private set; }

        public void Initialize()
		{
            using (var cmd = Connection.CreateCommand())
            {
                cmd.CommandText = @"
DROP TABLE IF EXISTS `BlogPost`;
CREATE TABLE IF NOT EXISTS `BlogPost` (
`Id` bigint(20) NOT NULL AUTO_INCREMENT,
`Content` longtext,
`Title` longtext,
PRIMARY KEY (`Id`)
) ENGINE=InnoDB;
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
