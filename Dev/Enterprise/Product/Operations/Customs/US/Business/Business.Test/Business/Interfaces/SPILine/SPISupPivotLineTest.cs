using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class SPISupPivotLineTest : TestCaseWithFactory
	{
		[TestDate(2008, 7, 1)]
		public void TestSPILine()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "0000000000";
			tariff.UE_SPICode = "AUSG";
			tariff.UE_DateFrom = new ZDateTime(2007, 1, 1);
			tariff.UE_DateTo = new ZDateTime(2007, 12, 31);
			var tariff2 = Factory.New<USCTariff>();
			tariff2.UE_Tariff = "0000000000";
			tariff2.UE_SPICode = "AU";
			tariff2.UE_DateFrom = new ZDateTime(2008, 1, 1);
			tariff2.UE_DateTo = new ZDateTime(2008, 12, 31);
			var tariff3 = Factory.New<USCTariff>();
			tariff3.UE_Tariff = "1111111111";
			tariff3.UE_SPICode = "AUSG";
			tariff3.UE_DateFrom = new ZDateTime(2007, 1, 1);
			tariff3.UE_DateTo = new ZDateTime(2007, 12, 31);
			var tariff4 = Factory.New<USCTariff>();
			tariff4.UE_Tariff = "1111111111";
			tariff4.UE_SPICode = "AU";
			tariff4.UE_DateFrom = new ZDateTime(2008, 1, 1);
			tariff4.UE_DateTo = new ZDateTime(2008, 12, 31);
			var product = Factory.New<OrgSupplierPart>();
			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_TariffNum = "0000000000";
			pivot.CD_UC_NKCountryOfOrigin = "AU";
			pivot.CI_SupplementalTariff = "9802009000";
			var spiLine = SPILine.New(pivot);
			AssertEquals("EffectiveDate:Today for pivot", new ZDateTime(2008, 7, 1), spiLine.EffectiveDate);
			AssertEquals("ImportTariff", "9802009000", spiLine.ImportTariff.UE_Tariff);
			AssertEquals("AU", spiLine.CountryOfOrigin.UC_Code);
			AssertEquals("HasSecondaryTariffLines", true, spiLine.HasSecondaryChildrenLines);
			AssertEquals("ParentTariffLine", null, spiLine.ParentTariffLine);
			AssertEquals("SecondaryLine1.Tariff", "0000000000", spiLine.SecondaryTariffLines.ElementAt(0).ImportTariff.UE_Tariff);
			var secondaryTariffLine = pivot.Children.AddNew();
			secondaryTariffLine.CI_TariffNum = "1111111111";
			secondaryTariffLine.CI_ChildType = ClassificationChildTypeList.Codes.COMPONENT;
			AssertEquals("HasSecondaryTariffLines", true, spiLine.HasSecondaryChildrenLines);
			AssertEquals("ParentTariffLine", null, spiLine.ParentTariffLine);
			AssertEquals("SecondaryLine2.Tariff", "1111111111", spiLine.SecondaryTariffLines.ElementAt(1).ImportTariff.UE_Tariff);
			int countOfSecondaryLine = 0;
			foreach (ISPILine secondarySPILine in spiLine.SecondaryTariffLines)
			{
				countOfSecondaryLine++;
				AssertEquals(new ZDateTime(2008, 7, 1), secondarySPILine.EffectiveDate);
			}

			AssertEquals(2, countOfSecondaryLine);
		}

		[TestDate(2008, 07, 01)]
		public void TestSPILineWhenEffectiveDateValid()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "0000000000";
			tariff.UE_SPICode = "AUSG";
			tariff.UE_DateFrom = new ZDateTime(2007, 1, 1);
			tariff.UE_DateTo = new ZDateTime(2007, 12, 31);
			var tariff2 = Factory.New<USCTariff>();
			tariff2.UE_Tariff = "0000000000";
			tariff2.UE_SPICode = "AU";
			tariff2.UE_DateFrom = new ZDateTime(2008, 1, 1);
			tariff2.UE_DateTo = new ZDateTime(2008, 12, 31);
			var tariff3 = Factory.New<USCTariff>();
			tariff3.UE_Tariff = "1111111111";
			tariff3.UE_SPICode = "AUSG";
			tariff3.UE_DateFrom = new ZDateTime(2007, 1, 1);
			tariff3.UE_DateTo = new ZDateTime(2007, 12, 31);
			var tariff4 = Factory.New<USCTariff>();
			tariff4.UE_Tariff = "1111111111";
			tariff4.UE_SPICode = "AU";
			tariff4.UE_DateFrom = new ZDateTime(2008, 1, 1);
			tariff4.UE_DateTo = new ZDateTime(2008, 12, 31);
			var product = Factory.New<OrgSupplierPart>();
			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_TariffNum = "0000000000";
			pivot.CD_UC_NKCountryOfOrigin = "AU";
			pivot.CI_SupplementalTariff = "9802009000";
			pivot.CI_DateStart = new ZDateTime(2008, 08, 01);
			var spiLine = SPILine.New(pivot);
			AssertEquals("EffectiveDate:Today for pivot", new ZDateTime(2008, 8, 1), spiLine.EffectiveDate);
			AssertEquals("ImportTariff", "9802009000", spiLine.ImportTariff.UE_Tariff);
			AssertEquals("AU", spiLine.CountryOfOrigin.UC_Code);
			AssertEquals("HasSecondaryTariffLines", true, spiLine.HasSecondaryChildrenLines);
			AssertEquals("ParentTariffLine", null, spiLine.ParentTariffLine);
			AssertEquals("SecondaryLine1.Tariff", "0000000000", spiLine.SecondaryTariffLines.ElementAt(0).ImportTariff.UE_Tariff);
			var secondaryTariffLine = pivot.Children.AddNew();
			secondaryTariffLine.CI_TariffNum = "1111111111";
			secondaryTariffLine.CI_ChildType = ClassificationChildTypeList.Codes.COMPONENT;
			AssertEquals("HasSecondaryTariffLines", true, spiLine.HasSecondaryChildrenLines);
			AssertEquals("ParentTariffLine", null, spiLine.ParentTariffLine);
			AssertEquals("SecondaryLine2.Tariff", "1111111111", spiLine.SecondaryTariffLines.ElementAt(1).ImportTariff.UE_Tariff);
			AssertEquals(2, spiLine.SecondaryTariffLines.Count());
			var line1 = spiLine.SecondaryTariffLines.FirstOrDefault(x => x.ImportTariffCode == "0000000000");
			var line2 = spiLine.SecondaryTariffLines.FirstOrDefault(x => x.ImportTariffCode == "1111111111");
			AssertEquals(new ZDateTime(2008, 8, 1), line1.EffectiveDate);
			AssertEquals(new ZDateTime(2008, 7, 1), line2.EffectiveDate);
		}
	}
}
