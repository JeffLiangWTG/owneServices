using System.Data;
using Microsoft.Data.SqlClient;

namespace CargoWise.RefDbRepo.Staging.DataPublishingProcessor.Test
{
	public static class DbTestHelper
	{
		public static void ExecuteNonQuery(IDbConnection conn, string cmdText, SqlTransaction trans = null)
		{
			var cmd = conn.CreateCommand();
			if (trans != null)
			{
				cmd.Transaction = trans;
			}
			cmd.CommandText = cmdText;
			cmd.ExecuteNonQuery();
		}

		public static object ExecuteScalar(IDbConnection conn, string cmdText)
		{
			var cmd = conn.CreateCommand();
			cmd.CommandText = cmdText;
			return cmd.ExecuteScalar();
		}

		public static IDbCommand CreateCommand(IDbConnection conn, SqlTransaction trans = null)
		{
			var cmd = conn.CreateCommand();
			if (trans != null)
			{
				cmd.Transaction = trans;
			}
			return cmd;
		}
	}
}
