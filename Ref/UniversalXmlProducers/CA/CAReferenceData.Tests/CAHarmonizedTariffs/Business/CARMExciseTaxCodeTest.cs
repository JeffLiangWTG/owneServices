using System.Collections.Generic;
using CargoWise.RefDbRepo.CAReferenceData.Business;
using CargoWise.RefDbRepo.CAReferenceData.Business.CAHarmonizedTariffs;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.CAReferenceData.Tests
{
	[TestFixture]
	internal class CARMExciseTaxCodeTest : CARMBusinessObjectTest<CARMExciseTaxCode>
	{
		[Test]
		public void TestParseToRefCusRate()
		{
			var exciseTaxCode = new CARMExciseTaxCode
			{
				ExciseTaxCode = "C01",
				ExciseTaxRateTypeCode = "1",
				ExciseTaxRateValue = 10d,
				UnitOfMeasureCode = "KGM"
			};
			var rate = exciseTaxCode.ParseToRefCusRate();
			Assert.AreEqual("10*[KGM]", rate.ZZ2_RateFormula);
			Assert.AreEqual("EXS", rate.ZZ2_ZY1_ZZR_NKRateType);
			Assert.AreEqual("C01", rate.ZZ2_ZY1_NKRateCode);
			Assert.AreEqual("CA", rate.ZZ2_ZZZ_NKDataGrouping);

			exciseTaxCode.ExciseTaxCode = string.Empty;
			Assert.Null(exciseTaxCode.ParseToRefCusRate());
		}

		public void TestParseToRefCusRate_RateFormula()
		{
			var exciseTaxCode = new CARMExciseTaxCode
			{
				ExciseTaxRateTypeCode = "1",
				ExciseTaxRateValue = 10d,
				UnitOfMeasureCode = "KGM"
			};
			var rate = exciseTaxCode.ParseToRefCusRate();
			Assert.AreEqual("10*[KGM]", rate.ZZ2_RateFormula);

			exciseTaxCode.ExciseTaxRateTypeCode = "2";
			exciseTaxCode.ExciseTaxRateValue = 20d;
			rate = exciseTaxCode.ParseToRefCusRate();
			Assert.AreEqual("20*[KGM]", rate.ZZ2_RateFormula);

			exciseTaxCode.ExciseTaxRateTypeCode = "3";
			exciseTaxCode.ExciseTaxRateValue = 30d;
			rate = exciseTaxCode.ParseToRefCusRate();
			Assert.AreEqual("0.300*VFD", rate.ZZ2_RateFormula);
		}

		protected override void AssertDeserializedEntry(CARMExciseTaxCode entryProperties, int caseID)
		{
			switch (caseID)
			{
				case 1:
					Assert.NotNull(entryProperties);
					Assert.AreEqual("C01", entryProperties.ExciseTaxCode);
					Assert.AreEqual("1", entryProperties.ExciseTaxRateTypeCode);
					Assert.AreEqual(0.25000d, entryProperties.ExciseTaxRateValue);
					Assert.AreEqual("GRM", entryProperties.UnitOfMeasureCode);
					break;
				case 2:
					Assert.AreEqual("C01", entryProperties.ExciseTaxCode);
					break;
			}
		}

		protected override IEnumerable<(int caseID, string xml)> TestCasesForDeserializeXML
		{
			get
			{
				yield return (1, @"
<feed xmlns=""http://www.w3.org/2005/Atom"" xmlns:m=""http://schemas.microsoft.com/ado/2007/08/dataservices/metadata"" xmlns:d=""http://schemas.microsoft.com/ado/2007/08/dataservices"" xml:base=""https://ccapi-ipacc.cbsa-asfc.cloud-nuage.canada.ca/v1/tariff-srv/"">
    <entry>
        <content type=""application/xml"">
            <m:properties xmlns:m=""http://schemas.microsoft.com/ado/2007/08/dataservices/metadata"" xmlns:d=""http://schemas.microsoft.com/ado/2007/08/dataservices"">
                <d:ExciseTaxCode>C01</d:ExciseTaxCode>
                <d:AsOfDate>2024-09-10T00:00:00</d:AsOfDate>
                <d:Language>EN</d:Language>
                <d:ExciseTaxRateValidStartDate>1000-05-13T00:00:00</d:ExciseTaxRateValidStartDate>
                <d:ExciseTaxRateValidEndDate>9999-10-20T00:00:00</d:ExciseTaxRateValidEndDate>
                <d:ExciseTaxRateTypeCode>1</d:ExciseTaxRateTypeCode>
                <d:ExciseTaxRateValue>0.25000</d:ExciseTaxRateValue>
                <d:UnitOfMeasureCode>GRM</d:UnitOfMeasureCode>
                <d:InactiveIndicator>false</d:InactiveIndicator>
                <d:Description>Federal specified rate - Dried, fresh, plants and plant seeds - flowering material</d:Description>
                <d:UpdateOn>2024-04-09T00:00:00</d:UpdateOn>
            </m:properties>
        </content>
    </entry>
</feed>
");
				yield return (2, @"
<feed xmlns=""http://www.w3.org/2005/Atom"" xmlns:m=""http://schemas.microsoft.com/ado/2007/08/dataservices/metadata"" xmlns:d=""http://schemas.microsoft.com/ado/2007/08/dataservices"" xml:base=""https://ccapi-ipacc.cbsa-asfc.cloud-nuage.canada.ca/v1/tariff-srv/"">
    <entry>
        <content type=""application/xml"">
            <m:properties xmlns:m=""http://schemas.microsoft.com/ado/2007/08/dataservices/metadata"" xmlns:d=""http://schemas.microsoft.com/ado/2007/08/dataservices"">
                <d:ExciseTaxCode>C01</d:ExciseTaxCode>
                <d:ExciseTaxRateValidStartDate>1998-01-01T00:00:00</d:ExciseTaxRateValidStartDate>
                <d:ExciseTaxRateValidEndDate>2024-12-31T00:00:00</d:ExciseTaxRateValidEndDate>
            </m:properties>
        </content>
    </entry>
</feed>
");
			}
		}
	}
}
