using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.TRReferenceData.Services.Loaders;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.TRReferenceData.Tests.Tariffs
{
	[TestFixture]
	public class TariffVatCodeLoaderTest
	{
		[Test]
		public void LoadAllTariffVatCodes()
		{
			var tempFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			Directory.CreateDirectory(tempFolder);
			var dataFilePath = Path.Combine(tempFolder, "TRTariffCodesVATCodes.xlsx");
			TestHelper.SimulateDownload(dataFilePath, "CargoWise.RefDbRepo.TRReferenceData.Tests.Tariffs.TestFiles.Input.TRTariffCodesVATCodes.xlsx");

			var data = TariffVatCodeLoader.Load(dataFilePath);

			Assert.That(data, Is.Not.Null.And.Not.Empty);
			Assert.That(data.Count, Is.EqualTo(18143));
		}
	}
}
