using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.TRReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.TRReferenceData.Tests.Services
{
	[TestFixture]
	class CsvRepositoryTest
	{
		[Test]
		public void GetAllFrom()
		{
			var path = Path.Combine(TempFolder, "CsvHelperTestData.csv");
			TestHelper.SimulateDownload(path, "CargoWise.RefDbRepo.TRReferenceData.Tests.Tariffs.TestFiles.Input.CsvHelperTestData.csv");
			var records = CsvRepository.GetAllFrom<Foo>(path);
			Assert.That(records.Count(), Is.EqualTo(2));
			Assert.That(records.First().Name, Is.EqualTo("Windows"));
			Assert.That(records.Last().Id, Is.EqualTo(2));
		}

		class Foo
		{
			public int Id { get; set; }
			public string Name { get; set; }
		}

		#region Setup

		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			TempFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			Directory.CreateDirectory(TempFolder);
		}

		[OneTimeTearDown]
		public void TearDown()
		{
			if (Directory.Exists(TempFolder))
			{
				Directory.Delete(TempFolder, true);
			}
		}

		string TempFolder;

		#endregion
	}
}
