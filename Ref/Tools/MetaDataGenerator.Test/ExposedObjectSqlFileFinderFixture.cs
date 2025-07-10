using NUnit.Framework;

namespace CargoWise.RefDbRepo.MetaDataGenerator.Test;

[TestFixture]
class ExposedObjectSqlFileFinderFixture
{
	[Test]
	public void GetFiles_ReturnsOnlyMatchingFiles()
	{
		var validFile = Path.Combine(tempDir!, "TestView_V1.sql");
		var invalidFile = Path.Combine(tempDir!, "Invalid.sql");
		File.WriteAllText(validFile, "-- sql");
		File.WriteAllText(invalidFile, "-- sql");

		var finder = new ExposedObjectSqlFileFinder(tempDir!);

		var result = finder.GetFiles().ToList();

		Assert.That(result, Has.Count.EqualTo(1));
		Assert.That(result[0], Is.EqualTo(validFile));
	}

	[SetUp]
	public void SetUp()
	{
		tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
		Directory.CreateDirectory(tempDir);
	}

	[TearDown]
	public void TearDown()
	{
		Directory.Delete(tempDir!, true);
	}

	string? tempDir;
}
