using System.IO;
using CargoWise.RefDbRepo.EUReferenceData.Business.ICS2;
using CargoWise.RefDbRepo.EUReferenceData.Services;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests
{
	sealed class TypeOfGoodsXMLProducerTest : CommonXmlProducerTest<TypeOfGoodsXMLProducer>
	{
		protected override string DownloadUrl => ApplicationConfig.Instance.TypeOfGoodsUrl;
		protected override string ExpectedXML => TestHelper.ReadManifestResourceContentAsString("CargoWise.RefDbRepo.EUReferenceData.Tests.ICS2.TypeOfGoods.TestFiles.Output.EUICS2_TypeOfGoods.xml");
		protected override string OutputFileName => "EUICS2_TypeOfGoods.xml";
		protected override Stream ZipFileForTest => TestHelper.ReadManifestResourceContentAsStream("CargoWise.RefDbRepo.EUReferenceData.Tests.ICS2.TypeOfGoods.TestFiles.Input.RD_ICS2_TypeoOfGoods.zip");
	}
}
