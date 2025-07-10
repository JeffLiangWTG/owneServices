using System;
using System.Collections.Generic;
using System.Globalization;
using CargoWise.RefDbRepo.CAReferenceData.Business;
using CargoWise.RefDbRepo.CAReferenceData.Business.CAHarmonizedTariffs;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.CAReferenceData.Tests
{
	[TestFixture]
	internal class CARMCustomsDutyTest : CARMBusinessObjectTest<CARMCustomsDuty>
	{
		[Test]
		[TestCaseSource(nameof(TestCasesForGenerateFormula))]
		public void TestGenerateFormula(int caseID, string[] valuesAndCodes)
		{
			var customsDuty = new CARMCustomsDuty
			{
				TariffItemNumber = "0704.10.11",
				SpecificRateMinValue = double.Parse(valuesAndCodes[0], CultureInfo.InvariantCulture),
				SpecificRateMinQualifierCode = valuesAndCodes[1],
				SpecificRateRegValue = double.Parse(valuesAndCodes[2], CultureInfo.InvariantCulture),
				SpecificRateRegQualifierCode = valuesAndCodes[3],
				SpecificRateMaxValue = double.Parse(valuesAndCodes[4], CultureInfo.InvariantCulture),
				SpecificRateMaxQualifierCode = valuesAndCodes[5],
				AdValoremRateMinValue = double.Parse(valuesAndCodes[6], CultureInfo.InvariantCulture),
				AdValoremRateMinQualifierCode = valuesAndCodes[7],
				AdValoremRateRegValue = double.Parse(valuesAndCodes[8], CultureInfo.InvariantCulture),
				AdValoremRateRegQualifierCode = valuesAndCodes[9],
				AdValoremRateMaxValue = double.Parse(valuesAndCodes[10], CultureInfo.InvariantCulture),
				AdValoremRateMaxQualifierCode = valuesAndCodes[11],
				UnitOfMeasureCode = valuesAndCodes[12]
			};
			Assert.AreEqual(valuesAndCodes[13], customsDuty.ParseToRefCusRate().ZZ2_RateFormula);

			customsDuty.TariffItemNumber = string.Empty;
			Assert.Null(customsDuty.ParseToRefCusRate());
		}

		static object[] TestCasesForGenerateFormula =
		{
				new object[] { 1, new string[] { "10", "2", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "UNT", "0.1*[UNT]" }},
				new object[] { 2, new string[] { "0", "2", "20", "1", "0", "0", "0", "0", "0", "0", "0", "0", "KGM", "20*[KGM]" } },
				new object[] { 3, new string[] { "0", "2", "0", "1", "30", "2", "0", "0", "0", "0", "0", "0", "KGM", "0.3*[KGM]" } },
				new object[] { 4, new string[] { "10", "2", "20", "1", "0", "0", "0", "0", "0", "0", "0", "0", "KGM", "20*[KGM] + 0.1*[KGM]" } },
				new object[] { 5, new string[] { "10", "2", "20", "1", "30", "2", "0", "0", "0", "0", "0", "0", "KGM", "20*[KGM] + MIN(0.3*[KGM], 0.1*[KGM])" } },
				new object[] { 6, new string[] { "10", "2", "20", "1", "30", "2", "40", "3", "0", "0", "0", "0", "KGM", "MAX(20*[KGM],0.4*VFD) + MIN(0.3*[KGM], 0.1*[KGM])" } },
				new object[] { 7, new string[] { "10", "2", "20", "1", "30", "2", "40", "3", "50", "3", "0", "0", "KGM", "MAX(20*[KGM],0.4*VFD) + MIN(0.3*[KGM], MAX(0.5*VFD,0.1*[KGM]))" } },
				new object[] { 8, new string[] { "10", "2", "20", "1", "30", "2", "40", "3", "50", "3", "60", "3", "KGM", "MIN(0.6*VFD, MAX(20*[KGM],0.4*VFD)) + MIN(0.3*[KGM], MAX(0.5*VFD,0.1*[KGM]))" } },
				new object[] { 9, new string[] { "10", "2", "20", "1", "0", "2", "40", "3", "50", "3", "60", "3", "KGM", "MIN(0.6*VFD, MAX(20*[KGM],0.4*VFD)) + MAX(0.5*VFD,0.1*[KGM])" } },
				new object[] { 10, new string[] { "10", "2", "0", "1", "0", "2", "40", "3", "50", "3", "60", "3", "KGM", "MIN(0.6*VFD, 0.4*VFD) + MAX(0.5*VFD,0.1*[KGM])" } },
				new object[] { 11, new string[] { "0", "2", "0", "1", "0", "2", "40", "3", "50", "3", "60", "3", "KGM", "MIN(0.6*VFD, 0.4*VFD) + 0.5*VFD" } },
				new object[] { 12, new string[] { "0", "2", "0", "1", "0", "2", "0", "3", "50", "3", "60", "3", "KGM", "0.6*VFD + 0.5*VFD" } },
				new object[] { 13, new string[] { "0", "2", "0", "1", "0", "2", "0", "3", "0", "3", "60", "3", "KGM", "0.6*VFD" } },
				new object[] { 14, new string[] { "0", "2", "0", "1", "0", "2", "0", "3", "50", "3", "0", "3", "KGM", "0.5*VFD" } },
				new object[] { 15, new string[] { "0", "2", "20", "1", "0", "0", "0", "0", "50", "3", "0", "0", "KGM", "20*[KGM] + 0.5*VFD" } }
		};

		[Test]
		public void TestParseToRefCusRate()
		{
			var customsDuty = new CARMCustomsDuty
			{
				TariffItemNumber = "0704.10.11",
				SpecificRateMinValue = 10,
				SpecificRateMinQualifierCode = "2",
				CustomsDutyValidStartDate = Constants.DefaultValues.MinDateTime,
				CustomsDutyValidEndDate = Constants.DefaultValues.MaxDateTime,
				TariffTreatmentCode = "02",
				UnitOfMeasureCode = "KGM"
			};
			var rate = customsDuty.ParseToRefCusRate();
			Assert.AreEqual("0.1*[KGM]", rate.ZZ2_RateFormula);
			Assert.AreEqual(Constants.DefaultValues.MinDateTime, rate.ZZ2_StartDate);
			Assert.AreEqual(Constants.DefaultValues.MaxDateTime, rate.ZZ2_EndDate);
			Assert.AreEqual("DTY", rate.ZZ2_ZY1_NKRateCode);
			Assert.AreEqual("DTY", rate.ZZ2_ZY1_ZZR_NKRateType);
			Assert.AreEqual("02", rate.ZZ2_ZZS_NKPreference);
			Assert.AreEqual("CA", rate.ZZ2_ZZZ_NKDataGrouping);
			Assert.AreEqual(1, rate.RefCusApplicabilities.Length);
			Assert.AreEqual(Constants.DefaultValues.MinDateTime, rate.RefCusApplicabilities[0].ZZT_StartDate);
			Assert.AreEqual(Constants.DefaultValues.MaxDateTime, rate.RefCusApplicabilities[0].ZZT_EndDate);
			Assert.AreEqual("02", rate.RefCusApplicabilities[0].ZZT_ZZA_NKTradeGroup);
		}

		protected override void AssertDeserializedEntry(CARMCustomsDuty entryProperties, int caseID)
		{
			switch (caseID)
			{
				case 1:
					Assert.NotNull(entryProperties);
					Assert.AreEqual(Constants.DefaultValues.MinDateTime, entryProperties.CustomsDutyValidStartDate);
					Assert.AreEqual(Constants.DefaultValues.MaxDateTime, entryProperties.CustomsDutyValidEndDate);
					Assert.AreEqual("07041011", entryProperties.TariffItemNumber);
					Assert.AreEqual("03", entryProperties.TariffTreatmentCode);
					Assert.AreEqual(0, entryProperties.SpecificRateRegValue);
					Assert.AreEqual("", entryProperties.SpecificRateRegQualifierCode);
					Assert.AreEqual(35, entryProperties.AdValoremRateRegValue);
					Assert.AreEqual("3", entryProperties.AdValoremRateRegQualifierCode);
					Assert.AreEqual("KGM", entryProperties.UnitOfMeasureCode);
					break;
				case 2:
					Assert.AreEqual(DateTime.Parse("1998-01-01T00:00:00", CultureInfo.InvariantCulture), entryProperties.CustomsDutyValidStartDate);
					Assert.AreEqual(DateTime.Parse("2024-12-31T23:59:00", CultureInfo.InvariantCulture), entryProperties.CustomsDutyValidEndDate);
					Assert.AreEqual("0704101100", entryProperties.TariffItemNumber);
					Assert.AreEqual("03", entryProperties.TariffTreatmentCode);
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
                <d:TariffItemNumber>0704.10.11</d:TariffItemNumber>
                <d:TariffTreatmentCode>003</d:TariffTreatmentCode>
                <d:CustomsDutyValidStartDate>1000-01-01T00:00:00</d:CustomsDutyValidStartDate>
                <d:CustomsDutyValidEndDate>9999-12-31T00:00:00</d:CustomsDutyValidEndDate>
                <d:SpecificRateRegValue>0.00000</d:SpecificRateRegValue>
                <d:SpecificRateRegQualifierCode></d:SpecificRateRegQualifierCode>
                <d:AdValoremRateRegValue>35.00000</d:AdValoremRateRegValue>
                <d:AdValoremRateRegQualifierCode>3</d:AdValoremRateRegQualifierCode>
                <d:UnitOfMeasureCode>KGM</d:UnitOfMeasureCode>
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
                <d:TariffItemNumber>0704.10.11.00</d:TariffItemNumber>
                <d:TariffTreatmentCode>03</d:TariffTreatmentCode>
                <d:CustomsDutyValidStartDate>1998-01-01T00:00:00</d:CustomsDutyValidStartDate>
                <d:CustomsDutyValidEndDate>2024-12-31T00:00:00</d:CustomsDutyValidEndDate>
            </m:properties>
        </content>
    </entry>
</feed>
");
			}
		}
	}
}
