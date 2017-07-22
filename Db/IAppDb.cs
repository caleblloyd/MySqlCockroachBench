using System;
using System.Data.Common;

namespace MySqlCockroachBench.Db
{
	public interface IAppDb : IDisposable
	{
		void Initialize();

		IAppDb New();

		DbConnection Connection { get; }
	}
}
