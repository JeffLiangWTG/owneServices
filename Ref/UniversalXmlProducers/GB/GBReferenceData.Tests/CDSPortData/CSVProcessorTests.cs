using CargoWise.RefDbRepo.GBReferenceData.Services.CDSPortData.Config;
using NUnit.Framework;
using static CargoWise.RefDbRepo.GBReferenceData.Tests.CDSPortData.TestHelperClasses;

namespace CargoWise.RefDbRepo.GBReferenceData.Tests.CDSPortData
{
	[TestFixture]
	class CSVProcessorTests
	{
		[Test]
		public void ExtractContent()
		{
			var source = new CDSPortSource { Code = "ABC", CodeColumn = 2, DescriptionColumns = new[] { 0 }, PageURL = "DoesNotMatter" };
			var content = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.GBReferenceData.Tests.CDSPortData.TestFiles.Input.SampleData.txt");

			var data = new CSVProcessorTester(source, Mocker.GetDownloadManagerForCSV(content)).ReadContentExposed();

			Assert.That(data, Is.Not.Null);
			Assert.That(data.Count, Is.EqualTo(7));

			Assert.That(data[1].Length, Is.EqualTo(5));
			Assert.That(string.Join("|", data[1]), Is.EqualTo("Location A|FAC 001|GBAA10000001|GBAA20000001|Something"));
			Assert.That(data[3].Length, Is.EqualTo(1));
			Assert.That(string.Join("|", data[3]), Is.EqualTo("some dodgy data"));
			Assert.That(data[6].Length, Is.EqualTo(5));
			Assert.That(string.Join("|", data[6]), Is.EqualTo("Location, C|FAC, 003|GBCC10000003|GBCC20000003|I-am-a-duplicate"));
		}

		[Test]
		public void ExtractContentWithNoHeaders()
		{
			var source = new CDSPortSource { Code = "ABC", CodeColumn = 2, DescriptionColumns = new[] { 0 }, PageURL = "DoesNotMatter" };
			var content = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.GBReferenceData.Tests.CDSPortData.TestFiles.Input.CSVSampleFileNoHeaders.txt");

			var data = new CSVProcessorTester(source, Mocker.GetDownloadManagerForCSV(content)).ReadContentExposed();

			Assert.That(data, Is.Not.Null);
			Assert.That(data.Count, Is.EqualTo(2));
		}
	}
}
