using CargoWise.RefDbRepo.Common.DbUpgrade;
using CargoWise.RefDbRepo.Common.Utils;
using Microsoft.Data.SqlClient;

namespace CargoWise.RefDbRepo.MetaDataGenerator;

abstract class MetaDataGenerator(string connectionString)
{
	public void Run()
	{
		using var connection = new SqlConnection(connectionString);
		var dbCreator = new DbCreator(connection);
		dbCreator.Deploy(DacPacFilePath, DbName, connectionString);
		GenerateMetaDataFiles();
		dbCreator.DropDatabase(DbName);
	}

	void GenerateMetaDataFiles()
	{
		var fileFinder = new ExposedObjectSqlFileFinder(SqlObjectsInputFolder);
		using var columnMetaDataProvider = new ColumnMetaDataProvider(connectionString, DbName);
		var metaDataFileWriter = new MetaDataFileWriter(MetaDataOutputFolder, fileFinder, columnMetaDataProvider);
		metaDataFileWriter.Write();

		Console.WriteLine("Generated MetaData successfully.");
	}

	protected abstract string DbName { get; }
	protected abstract string DacPacFilePath { get; }
	protected abstract string SqlObjectsInputFolder { get; }
	protected abstract string MetaDataOutputFolder { get; }
}
