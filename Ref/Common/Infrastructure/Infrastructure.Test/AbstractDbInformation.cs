using System;
using System.IO;
using Microsoft.Data.SqlClient;

namespace CargoWise.RefDbRepo.Common.Infrastructure.Test
{
	abstract class AbstractDbInformation
	{
		protected string DbName
		{
			get { return fDbName ?? (fDbName = DbNamePrefix + UniqueDbName); }
		}

		string fDbName;
		protected string UniqueDbName { get; set; }

		protected string TmpPath
		{
			get { return fTmpPath ?? (fTmpPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString())); }
		}

		string fTmpPath;

		public void CreateDatabase(string collation = "")
		{
			using (var connection = new SqlConnection(TestConnectionString.GetAdmin(null)))
			{
				DBHelper.Setup(connection, DbName, DbNamePrefix.Substring(0, DbNamePrefix.Length - 1));
				Deploy(connection, collation);
				DBHelper.CreateLogin(connection, DbName, TestConnectionString.Reader,
					TestConnectionString.ReaderPassword, AbstractTestDbInitializer.ReaderMemeberships);
				DBHelper.CreateLogin(connection, DbName, TestConnectionString.WriterUsername,
					TestConnectionString.WriterPassword, MemberShips);
			}
		}

		protected string[] MemberShips { get; set; }

		public void TearDown()
		{
			try
			{
				using (var connection = new SqlConnection(TestConnectionString.GetAdmin(null)))
				{
					DBHelper.TearDown(connection, TmpPath, DbName);
				}
			}
			catch (SqlException)
			{
			}
		}

		protected abstract void Deploy(SqlConnection connection, string collation);

		public const string DbNamePrefix = "RefDbRepo9D461089D8AC41FD920A6C60742A0070_";
	}
}
