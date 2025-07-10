using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.TRReferenceData.Services.Loaders;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.TRReferenceData.Tests.Services
{
	[TestFixture]
	public class ExportUnionTariffLoaderTest
	{
		[Test]
		public void LoadData()
		{
			var records = ExportUnionTariffLoader.LoadData(DataFilePath);
			Assert.AreEqual(12157, records.Count());

			var pairs = records.Where(r => r.Code == "200580000000001");
			Assert.AreEqual(1, pairs.Count());

			var pair = pairs.First();
			Assert.AreEqual("TATLI MISIR (ZEA MAYS VAR. SACCHARATA)-SİRKESİZ,  KONSERVE EDİLMİŞ,DONDURULMAMIŞ  NET<=10", pair.Description);
		}

		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			TempFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			Directory.CreateDirectory(TempFolder);
			DataFilePath = Path.Combine(TempFolder, "TR Export Union Export Tariff Additional Codes.xlsx");
			TestHelper.SimulateDownload(DataFilePath, "CargoWise.RefDbRepo.TRReferenceData.Tests.Tariffs.TestFiles.Input.TR Export Union Export Tariff Additional Codes.xlsx");
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
		string DataFilePath;
	}
}
