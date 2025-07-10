using System;
using System.IO;
using System.Reflection;
using Dat.Integration;
using Dat.Integration.Deployment;

namespace CargoWise.RefDbRepo.Deployment.Cmd
{
	class Program
	{
		static void Main(string[] args)
		{
			var config = args.Length > 0 ? args[0] : "Test";
			var sourcePath = Path.GetFullPath(Path.Combine(Assembly.GetExecutingAssembly().Location, @"..\..\..\.."));

			if (!string.IsNullOrEmpty(TaskCommentsAsTestRig))
			{
				var taskInfo = new TaskInfo("SH0Local", "Local", TaskCommentsAsTestRig);
				//new DATDeployer(new Logger()).AutoDeployTestedShelf("Local", config, sourcePath, Path.Combine(sourcePath, "bin"), taskInfo);
				((IBuildDeployer2)new DATDeployer(new Logger())).TeardownTestedShelf("Local", config, sourcePath, Path.Combine(sourcePath, "bin"), taskInfo);
			}
			else
			{
				new DATDeployer(new Logger()).DeployOnDemand(null, config, sourcePath, Path.Combine(sourcePath, "bin"));
			}

			Console.Out.WriteLine("Press any key to exit..");
			Console.ReadKey();
		}

		static string TaskCommentsAsTestRig => @"
TestRigWebSiteName:WILocalBuild
TestRigWebServer:
TestRigSqlServer:SYDSP-SSQL-6.sand.wtg.zone\INSTANCE1
TestRigDeployUsername:
TestRigDeployPassword:
TestRigRefDbRepoSafeRestoreFromBackup:\\SYDSP-SWEB-1.sand.wtg.zone\SQLBackups\ProdForTestRig\RefDbRepoSafe.bak
TestRigRefDbRepoStagingRestoreFromBackup:\\SYDSP-SWEB-1.sand.wtg.zone\SQLBackups\ProdForTestRig\RefDbRepoStage.bak
";
	}

	class Logger : ITaskLogger
	{
		public void RecordInfo(string message)
		{
			Console.Out.WriteLine(message);
		}

		public IDisposable RecordTask(string taskInfo)
		{
			throw new NotImplementedException();
		}
	}
}
