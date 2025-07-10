using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.MetaDataGenerator.Test;

[TestFixture]
[Property("DAT:CapabilityRequirements", "SQL2019+")]
class RemoteDbMetaDataGeneratorFixture
{
	[Test]
	public void Run_GeneratesMetaDataFiles()
	{
		var generator =
			new TestableRemoteDbMetaDataGenerator(connectionString!, sqlObjectsInputFolder!, metaDataOutputFolder!);
		Assert.DoesNotThrow(() => generator.Run());

		var files = Directory.GetFiles(metaDataOutputFolder!, "*.metadata.json");
		Assert.That(files, Has.Length.GreaterThan(0), "No metadata files were generated.");
	}

	[SetUp]
	public void SetUp()
	{
		connectionString = TestConnectionString.GetAdmin(null);
		sqlObjectsInputFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
		metaDataOutputFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

		Directory.CreateDirectory(sqlObjectsInputFolder);
		Directory.CreateDirectory(metaDataOutputFolder);

		var testSqlFilePath = Path.Combine(sqlObjectsInputFolder, "TestView_V1.sql");
		File.WriteAllText(testSqlFilePath, "CREATE VIEW [dbo].[TestView_V1] AS SELECT 1 AS Dummy;");
	}

	[TearDown]
	public void TearDown()
	{
		if (Directory.Exists(sqlObjectsInputFolder))
		{
			Directory.Delete(sqlObjectsInputFolder, true);
		}

		if (Directory.Exists(metaDataOutputFolder))
		{
			Directory.Delete(metaDataOutputFolder, true);
		}
	}

	string? connectionString;
	string? sqlObjectsInputFolder;
	string? metaDataOutputFolder;

	class TestableRemoteDbMetaDataGenerator(
		string connectionString,
		string sqlObjectsInputFolder,
		string metaDataOutputFolder)
		: RemoteDbMetaDataGenerator(connectionString)
	{
		protected override string SqlObjectsInputFolder { get; } = sqlObjectsInputFolder;
		protected override string MetaDataOutputFolder { get; } = metaDataOutputFolder;
	}
}
