using System;
using System.Collections.Generic;
using System.Globalization;
using CargoWise.RefDbRepo.CAReferenceData.Business;
using CargoWise.RefDbRepo.CAReferenceData.Business.CAHarmonizedTariffs;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.CAReferenceData.Tests
{
	[TestFixture]
	internal class CARMExciseDutyTest : CARMBusinessObjectTest<CARMExciseDuty>
	{
		[Test]
		public void TestParseToRefCusRate()
		{
			var exciseDuty = new CARMExciseDuty
			{
				TariffNumber = "0704.10.11.00",
				ExciseDutyValidStartDate = Constants.DefaultValues.MinDateTime,
				ExciseDutyValidEndDate = Constants.DefaultValues.MaxDateTime,
				ExciseDutyValueType = "S",
				ExciseDutyRateValue = 10d,
				UnitOfMeasureCode = "KGM"
			};
			var rate = exciseDuty.ParseToRefCusRate();
			Assert.AreEqual("10*[KGM]", rate.ZZ2_RateFormula);
			Assert.AreEqual(Constants.DefaultValues.MinDateTime, rate.ZZ2_StartDate);
			Assert.AreEqual(Constants.DefaultValues.MaxDateTime, rate.ZZ2_EndDate);
			Assert.AreEqual("EXC", rate.ZZ2_ZY1_ZZR_NKRateType);
			Assert.AreEqual("EXS", rate.ZZ2_ZY1_NKRateCode);
			Assert.AreEqual("CA", rate.ZZ2_ZZZ_NKDataGrouping);

			exciseDuty.TariffNumber = string.Empty;
			Assert.Null(exciseDuty.ParseToRefCusRate());
		}

		protected override void AssertDeserializedEntry(CARMExciseDuty entryProperties, int caseID)
		{
			switch (caseID)
			{
				case 1:
					Assert.NotNull(entryProperties);
					Assert.AreEqual(Constants.DefaultValues.MinDateTime, entryProperties.ExciseDutyValidStartDate);
					Assert.AreEqual(Constants.DefaultValues.MaxDateTime, entryProperties.ExciseDutyValidEndDate);
					Assert.AreEqual("2402100010", entryProperties.TariffNumber);
					Assert.AreEqual("S", entryProperties.ExciseDutyValueType);
					Assert.AreEqual(40.43121d, entryProperties.ExciseDutyRateValue);
					Assert.AreEqual("MIL", entryProperties.UnitOfMeasureCode);
					break;
				case 2:
					Assert.AreEqual("2402100000", entryProperties.TariffNumber);
					Assert.AreEqual(DateTime.Parse("1998-01-01T00:00:00", CultureInfo.InvariantCulture), entryProperties.ExciseDutyValidStartDate);
					Assert.AreEqual(DateTime.Parse("2024-12-31T23:59:00", CultureInfo.InvariantCulture), entryProperties.ExciseDutyValidEndDate);
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
                <d:TariffNumber>2402.10.00.10</d:TariffNumber>
                <d:AsOfDate>2024-09-10T00:00:00</d:AsOfDate>
                <d:ExciseDutyValidStartDate>1000-04-17T00:00:00</d:ExciseDutyValidStartDate>
                <d:ExciseDutyValidEndDate>9999-12-31T00:00:00</d:ExciseDutyValidEndDate>
                <d:UnitOfMeasureCode>MIL</d:UnitOfMeasureCode>
                <d:ExciseDutyValueType>S</d:ExciseDutyValueType>
                <d:ExciseDutyRateValue>40.43121</d:ExciseDutyRateValue>
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
                <d:TariffNumber>2402.10.00</d:TariffNumber>
                <d:ExciseDutyValidStartDate>1998-01-01T00:00:00</d:ExciseDutyValidStartDate>
                <d:ExciseDutyValidEndDate>2024-12-31T00:00:00</d:ExciseDutyValidEndDate>
            </m:properties>
        </content>
    </entry>
</feed>
");
			}
		}
	}
}
