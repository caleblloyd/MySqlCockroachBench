using System;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using MySqlCockroachBench.Db;
using Npgsql;

namespace MySqlCockroachBench.Models
{
	public class BlogPost
	{
		public long Id { get; set; }
		public string Title { get; set; }
		public string Content { get; set; }
		public IAppDb Db { get; set; }

		public BlogPost(IAppDb db=null)
		{
			Db = db;
		}

		public void Insert()
		{
			var cmd = InsertCmd();
			Id = Convert.ToInt64(cmd.ExecuteScalar());
		}

		public async Task InsertAsync()
		{
			var cmd = InsertCmd();
			Id = Convert.ToInt64(await cmd.ExecuteScalarAsync());
		}

		public void Update()
		{
			var cmd = UpdateCmd();
			cmd.ExecuteNonQuery();
		}

		public async Task UpdateAsync()
		{
			var cmd = UpdateCmd();
			await cmd.ExecuteNonQueryAsync();
		}

		public void Delete()
		{
			var cmd = DeleteCmd();
			cmd.ExecuteNonQuery();
		}

		public async Task DeleteAsync()
		{
			var cmd = DeleteCmd();
			await cmd.ExecuteNonQueryAsync();
		}

		private void BindId(DbCommand cmd)
		{
			var parameter = cmd.CreateParameter();
            parameter.ParameterName = "@id";
            parameter.Value = Id;
			cmd.Parameters.Add(parameter);
		}

		private void BindParams(DbCommand cmd)
		{
			var parameter = cmd.CreateParameter();
			parameter.ParameterName = "@title";
            parameter.Value = Title;
			cmd.Parameters.Add(parameter);
			
			parameter = cmd.CreateParameter();
			parameter.ParameterName = "@content";
            parameter.Value = Content;
			cmd.Parameters.Add(parameter);
		}

		private DbCommand InsertCmd()
		{
			var cmd = Db.Connection.CreateCommand();
			// ReSharper disable once PossibleNullReferenceException
			cmd.CommandText = @"INSERT INTO BlogPost (Title, Content) VALUES (@title, @content)";
			BindParams(cmd);
			if (cmd as MySqlCommand != null)
				cmd.CommandText += "; SELECT LAST_INSERT_ID();";
			if (cmd as NpgsqlCommand != null)
				cmd.CommandText += " RETURNING id;";
			return cmd;
		}

		private DbCommand UpdateCmd()
		{
			var cmd = Db.Connection.CreateCommand();
			// ReSharper disable once PossibleNullReferenceException
			cmd.CommandText = @"UPDATE BlogPost SET Title = @title, Content = @content WHERE Id = @id;";
			BindParams(cmd);
			BindId(cmd);
			return cmd;
		}

		private DbCommand DeleteCmd()
		{
			var cmd = Db.Connection.CreateCommand();
			// ReSharper disable once PossibleNullReferenceException
			cmd.CommandText = @"DELETE FROM BlogPost WHERE Id = @id;";
			BindId(cmd);
			return cmd;
		}

	}
}
