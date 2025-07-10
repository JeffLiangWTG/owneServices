using NUnit.Framework;

namespace CargoWise.RefDbRepo.TaiwanReferenceData.Test
{
	[TestFixture]
	public class TariffDescriptionDataRowTest
	{
		[Test]
		public void TestTariffDescriptionDataRow()
		{
			var tariffDescriptionDataRow = new TariffDescriptionDataRow("0101210000 馬，純種繁殖用");
			Assert.AreEqual("0101210000", tariffDescriptionDataRow.TariffCode);
			Assert.AreEqual("馬，純種繁殖用", tariffDescriptionDataRow.TariffDescription);

			tariffDescriptionDataRow = new TariffDescriptionDataRow("01012100001XXXXXXXXXXXXXXXXXXXXXXXXXX");
			Assert.AreEqual("01012100001", tariffDescriptionDataRow.TariffCode);
			Assert.AreEqual("XXXXXXXXXXXXXXXXXXXXXXXXXX", tariffDescriptionDataRow.TariffDescription);
		}
	}
}
