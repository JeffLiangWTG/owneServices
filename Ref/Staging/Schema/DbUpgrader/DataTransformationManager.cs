using System.Data;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Staging.DbUpgrader
{
	public class DataTransformationManager
	{
		readonly TransformationTasks tasks;
		readonly ISchemaVersionManager versionManager;
		readonly IDbConnection connection;
		readonly int targetVersion;

		public DataTransformationManager(TransformationTasks tasks, ISchemaVersionManager versionManager, IDbConnection connection, int targetVersion)
		{
			Argument.NotNull(versionManager, nameof(versionManager));
			Argument.NotNull(tasks, nameof(tasks));
			Argument.NotNull(connection, nameof(connection));

			this.tasks = tasks;
			this.versionManager = versionManager;
			this.connection = connection;
			this.targetVersion = targetVersion;
		}

		public void Execute()
		{
			if (versionManager.GetVersion(null) < targetVersion)
			{
				using (var trans = connection.BeginTransaction())
				{
					var currentVersion = versionManager.GetVersion(trans);
					if (currentVersion < targetVersion)
					{
						tasks.Run(currentVersion, targetVersion, trans);
						versionManager.UpdateVersion(targetVersion, trans);
						trans.Commit();
					}
				}
			}
		}
	}
}
