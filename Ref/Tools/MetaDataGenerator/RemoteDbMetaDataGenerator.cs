namespace CargoWise.RefDbRepo.MetaDataGenerator;

class RemoteDbMetaDataGenerator(string connectionString) : MetaDataGenerator(connectionString)
{
	protected override string DbName => "RefDbRepoRemote_MetaData";
	protected override string DacPacFilePath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "RemoteDb.dacpac");
	protected override string SqlObjectsInputFolder => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\Service\SchemaManagement\RemoteDb");
	protected override string MetaDataOutputFolder => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\Service\SchemaManagement\RemoteDbManager.Test\MetaData\");
}
