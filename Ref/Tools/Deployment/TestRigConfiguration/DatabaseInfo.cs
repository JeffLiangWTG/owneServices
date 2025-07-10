namespace CargoWise.RefDbRepo.Deployment.TestRigConfiguration
{
	public class DatabaseInfo
	{
		public string DatabaseName { get; private set; }
		public string BackupFullFileName { get; set; }

		public DatabaseInfo(string databaseName, string backupFullFileName)
		{
			DatabaseName = databaseName;
			BackupFullFileName = backupFullFileName;
		}
	}
}
