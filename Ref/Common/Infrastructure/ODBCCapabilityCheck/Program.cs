using System;
using System.Data.Odbc;
using CargoWise.RefDbRepo.Common.Utils;

namespace CargoWise.RefDbRepo.Common.ODBCCapabilityCheck
{
	class Program
	{
		static int Main()
		{
#pragma warning disable CA1031 // Do not catch general exception types
			try
			{
				var driver = new OdbcDriverHelper().GetMicrosoftAccessDriver();
				if (string.IsNullOrEmpty(driver))
				{
					throw new InvalidOperationException("Driver not found");
				}
				var executing = System.Reflection.Assembly.GetExecutingAssembly();
				var emptyMdbFilePath = executing.Location.Replace(executing.ManifestModule.Name, "Empty.mdb");

				var connectionString = $"Driver={{{driver}}};Dbq={emptyMdbFilePath};";
				using (var conn = new OdbcConnection(connectionString))
				{
					conn.Open();
					conn.Close();
				}
				return 0;
			}
			catch
			{
				return 1;
			}
#pragma warning restore CA1031 // Do not catch general exception types
		}
	}
}
