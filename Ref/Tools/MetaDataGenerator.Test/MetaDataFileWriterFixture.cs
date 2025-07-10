using CargoWise.RefDbRepo.Common.Utils;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.MetaDataGenerator.Test;

[TestFixture]
class MetaDataFileWriterFixture
{
	[SetUp]
	public void SetUp()
	{
		outptuDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
		Directory.CreateDirectory(outptuDir);
	}

	[TearDown]
	public void TearDown()
	{
		if (Directory.Exists(outptuDir))
		{
			Directory.Delete(outptuDir, true);
		}
	}

	[Test]
	public void WriteMetaData_CreatesMetadataFilesWithHeaderAndJson()
	{
		var fakeSqlFiles = new[] { "Test_V1.sql", "Test_V2.sql" };
		var mockFileFinder = new Mock<IFileFinder>();
		mockFileFinder.Setup(f => f.GetFiles()).Returns(fakeSqlFiles);

		var mockMetaDataProvider = new Mock<IColumnMetaDataProvider>();
		mockMetaDataProvider.Setup(p => p.GetMetaData("Test_V1")).Returns([
			new ColumnMetaData
			{
				ColumnName = "Id",
				ColumnSize = 2,
				DataType = "int",
				IsNullable = false,
				NumericPrecision = 2,
				NumericScale = 1
			}
		]);
		mockMetaDataProvider.Setup(p => p.GetMetaData("Test_V2")).Returns([
			new ColumnMetaData
			{
				ColumnName = "Type",
				ColumnSize = 3,
				DataType = "varchar",
				IsNullable = false,
				NumericPrecision = 0,
				NumericScale = 0
			}
		]);

		var writer = new MetaDataFileWriter(outptuDir!, mockFileFinder.Object, mockMetaDataProvider.Object);

		writer.Write();

		foreach (var file in fakeSqlFiles)
		{
			var objectName = Path.GetFileNameWithoutExtension(file);
			var expectedPath = Path.Combine(outptuDir!, $"{objectName}.metadata.json");
			Assert.That(File.Exists(expectedPath), Is.True, $"File {expectedPath} should exist.");

			var content = File.ReadAllText(expectedPath);
			Assert.That(
				content.StartsWith("//------------------------------------------------------------------------------"),
				Is.True, "File should start with header comment.");
			Assert.That(content.Contains($"\"ColumnName\": \"{(objectName == "Test_V1" ? "Id" : "Type")}\""), Is.True,
				"File should contain correct JSON content.");
		}
	}

	string? outptuDir;
}
