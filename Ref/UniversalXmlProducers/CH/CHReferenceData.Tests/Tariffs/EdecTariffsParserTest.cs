using CargoWise.RefDbRepo.CHReferenceData.Business.Tariffs;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.CHReferenceData.Tests.Tariffs
{
	[TestFixture]
	class EdecTariffsParserTest
	{
		[TestCase]
		public void TestMapQuantityCode() => Assert.Multiple(() =>
		{
			Assert.AreEqual(null, EdecTariffsParser.MapQuantityCode(10), "10");
			Assert.AreEqual("LTR", EdecTariffsParser.MapQuantityCode(11), "11");
			Assert.AreEqual("MTR", EdecTariffsParser.MapQuantityCode(12), "12");
			Assert.AreEqual("MTK", EdecTariffsParser.MapQuantityCode(13), "13");
			Assert.AreEqual("MTQ", EdecTariffsParser.MapQuantityCode(14), "14");
			Assert.AreEqual("MWH", EdecTariffsParser.MapQuantityCode(15), "15");
			Assert.AreEqual("NCR", EdecTariffsParser.MapQuantityCode(16), "16");
			Assert.AreEqual("NAR", EdecTariffsParser.MapQuantityCode(17), "17");
			Assert.AreEqual("NAR", EdecTariffsParser.MapQuantityCode(18), "18");
			Assert.AreEqual("NAR", EdecTariffsParser.MapQuantityCode(19), "19");
			Assert.AreEqual("NPR", EdecTariffsParser.MapQuantityCode(20), "20");
			Assert.AreEqual("NAR", EdecTariffsParser.MapQuantityCode(22), "22");
			Assert.AreEqual("LTR", EdecTariffsParser.MapQuantityCode(24), "24");
			Assert.AreEqual("MTQ", EdecTariffsParser.MapQuantityCode(25), "25");
			Assert.AreEqual("NAR", EdecTariffsParser.MapQuantityCode(26), "26");
		});
	}
}
