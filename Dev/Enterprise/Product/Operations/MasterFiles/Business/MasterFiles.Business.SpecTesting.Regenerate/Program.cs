#pragma warning disable CW1161 // There's no need for resource strings here
#pragma warning disable CW1106 // Console logs are ok here

using System;
using CargoWise.Data;
using Enterprise.Initialisation;
using Enterprise.Startup;
using WTG.TestHelpers.SpecTesting;

namespace Enterprise.MasterFiles.Business.SpecTesting.Regenerate
{
	public static class Program
	{
		public static void Main(string[] args)
		{
			var databaseName = "Odyssey";
			Console.WriteLine($"Using database {databaseName} for RefDB data");

			// This is necessary as the current implementation of the static spec is coupled to code that requires database access.
			// This should be removed/changed in the future.
			Initialiser.InitialiseWinForms();
			Db.InitializeDatabaseDetails(System.Environment.MachineName, databaseName);
			LoginDirector.Instance.Execute(new ApplicationArguments(Array.Empty<string>()));

			WriteSpec();
		}

		public static void WriteSpec()
		{
			SpecFileWriter.WriteSpecFiles(new OrgCusCodeSpecGenerator());
			SpecFileWriter.WriteSpecFiles(new ComplianceDataSpecGenerator());
		}
	}
}
