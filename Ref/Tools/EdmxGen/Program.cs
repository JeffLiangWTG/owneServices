using System;
using System.Linq;
using CargoWise.RefDbRepo.Common.Utils;
using CargoWise.RefDbRepo.Tools.Common;
using Microsoft.Data.SqlClient;

namespace CargoWise.RefDbRepo.EdmxGen
{
	class Program
	{
		const string ScaffoldPathSafe = @"..\..\..\Service\SchemaManagement\Schema_0_9_New";
		const string ScaffoldPathStaging = @"..\..\..\Staging\Schema\Schema_New";

		static void Main(string[] args)
		{
			var bothSafeAndStaging = true;
			var safeOnly = false;
			var stagingOnly = false;

			if (args.Contains("safe"))
			{
				safeOnly = true;
				bothSafeAndStaging = false;
				DBUpgrader.UpgradeDatabase(true, false);
			}
			else if (args.Contains("staging"))
			{
				stagingOnly = true;
				bothSafeAndStaging = false;
				DBUpgrader.UpgradeDatabase(false, true);
			}
			else
			{
				DBUpgrader.UpgradeDatabase(false, false);
			}

			Console.WriteLine(string.Empty);
			
			if (bothSafeAndStaging || safeOnly)
			{
				CreateSafeTriggers();
				RegenSafeWithEFCore();
			}
			if (bothSafeAndStaging || stagingOnly)
			{
				RegenStagingWithEFCore();
			}
		}

		static void CreateSafeTriggers()
		{
			using (var connection = new SqlConnection(Application.SafeConnectionString))
			{
				connection.Open();
				var dataSetHelper = new DataSetHelper(connection);
				var dataSetsList = DataSetStructureProvider.StructuredDataSets;
				var triggerGenerator = new TriggerGenerator(dataSetsList, dataSetHelper);
				triggerGenerator.CreateTriggers(Application.SafeDbTriggersPath);
			}
		}

		static void RegenSafeWithEFCore()
		{
			ScaffoldRunner.GenerateDbContextAndEntities(ScaffoldPathSafe, Application.SafeConnectionString, "SafeDbContext", "CargoWise.RefDbRepo.Service.Schema_0_9_New");
		}

		static void RegenStagingWithEFCore()
		{
			ScaffoldRunner.GenerateDbContextAndEntities(ScaffoldPathStaging, Application.StagingConnectionString, "StagingDbContext", "CargoWise.RefDbRepo.Staging.Schema_New");
		}
	}
}
