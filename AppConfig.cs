using System.IO;
using Microsoft.Extensions.Configuration;

namespace MySqlCockroachBench
{
	public static class AppConfig
	{
		public static IConfigurationRoot Config = new ConfigurationBuilder()
			.SetBasePath(Directory.GetCurrentDirectory())
			.AddJsonFile("config.json")
			.Build();
	}
}
