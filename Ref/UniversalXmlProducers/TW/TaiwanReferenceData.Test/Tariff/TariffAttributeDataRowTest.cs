using NUnit.Framework;

namespace CargoWise.RefDbRepo.TaiwanReferenceData.Test
{
	[TestFixture]
	public class TariffAttributeDataRowTest
	{
		[Test]
		public void TestTariffAttributeDataRow()
		{
			var tariffAttributeDataRow = new TariffAttributeDataRow("87120010109,F5FTZDestination,AT;BE;BG;CY;CZ;DE;DK;EE;ES;FI;FR;GR;HR;HU;IE;IT;LT;LU;LV;MT;NL;PL;PT;RO;SE;SI;SK;GB;US");
			Assert.AreEqual("87120010109", tariffAttributeDataRow.TariffCode);
			Assert.AreEqual("F5FTZDestination", tariffAttributeDataRow.TariffAttributeName);
			Assert.AreEqual("AT;BE;BG;CY;CZ;DE;DK;EE;ES;FI;FR;GR;HR;HU;IE;IT;LT;LU;LV;MT;NL;PL;PT;RO;SE;SI;SK;GB;US", tariffAttributeDataRow.TariffAttributeValue);
		}
	}
}
