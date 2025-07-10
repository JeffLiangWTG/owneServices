namespace CargoWise.RefDbRepo.Common.Infrastructure.Test
{
	public static class TestConnectionString
	{
		public static string GetAdmin(string dbName)
		{
			return Get(dbName);
		}

		public static string GetWriter(string dbName)
		{
			return Get(dbName, WriterUsername, WriterPassword);
		}

		static string Get(string dbName, string userId = "", string password = "")
		{
			var builder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder()
			{
				DataSource = DataSource,
				ApplicationName = "RefDbRepo Testing",
				MultipleActiveResultSets = true,
				TrustServerCertificate = true
			};
			if (!string.IsNullOrEmpty(dbName))
			{
				builder.InitialCatalog = dbName;
			}
			if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(password))
			{
				builder.IntegratedSecurity = true;
			}
			else
			{
				builder.UserID = userId;
				builder.Password = password;
			}
			return builder.ToString();
		}

		public static string WriterUsername { get { return "refdbrepowriterfortest"; } }
		// This is a secure password that is used for testing purposes only. It is not used in production.
		public static string WriterPassword { get { return "FTuA_6.WXa@ZW#v"; } }
		public static string Reader { get { return "refdbreporeaderfortest"; } }
		// This is a secure password that is used for testing purposes only. It is not used in production.
		public static string ReaderPassword { get { return "5(q<YA=2:^^ZeB"; } }

		public static string DataSource { get { return "localhost"; } }
	}
}
