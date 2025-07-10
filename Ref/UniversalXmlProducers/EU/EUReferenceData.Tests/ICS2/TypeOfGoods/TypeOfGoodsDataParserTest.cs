using System.IO;
using CargoWise.RefDbRepo.EUReferenceData.Business;
using CargoWise.RefDbRepo.EUReferenceData.Business.ICS2;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests
{
	[TestFixture]
	sealed class TypeOfGoodsDataParserTest : CommonXmlDataParserTest<TypeOfGoodsDataParser>
	{
		protected override Stream GetXMLFile() => TestHelper.ReadManifestResourceContentAsStream("CargoWise.RefDbRepo.EUReferenceData.Tests.ICS2.TypeOfGoods.TestFiles.Input.RD_ICS2_TypeoOfGoods.xml");

		protected override Stream GetInvalidXMLFile() => TestHelper.ReadManifestResourceContentAsStream("CargoWise.RefDbRepo.EUReferenceData.Tests.ICS2.TypeOfGoods.TestFiles.Input.RD_ICS2_TypeoOfGoods_Invalid.xml");

		protected override Stream GetRDEntityNotFoundErrorFile() => TestHelper.ReadManifestResourceContentAsStream("CargoWise.RefDbRepo.EUReferenceData.Tests.ICS2.TypeOfGoods.TestFiles.Input.RD_ICS2_TypeoOfGoods_EntityNotFound.xml");

		protected override string RDEntityAttributeValue => Constants.TypeOfGoods.RDEntityAttributeValue;

		protected override bool IsAdditionalTranslationSupporter => true;
	}
}
