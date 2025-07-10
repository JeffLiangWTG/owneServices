using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class SPINormalPivotLineTest : TestCaseWithFactory
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
			var spiLine = SPILine.New(pivot);
			AssertEquals("EffectiveDate:Today for pivot", new ZDateTime(2008, 7, 1), spiLine.EffectiveDate);
			AssertEquals("ImportTariff", "0000000000", spiLine.ImportTariff.UE_Tariff);
			AssertEquals("AU", spiLine.CountryOfOrigin.UC_Code);
			AssertEquals("HasSecondaryTariffLines", false, spiLine.HasSecondaryChildrenLines);
			AssertEquals("ParentTariffLine", null, spiLine.ParentTariffLine);
			var secondaryTariffLine = pivot.Children.AddNew();
			secondaryTariffLine.CI_TariffNum = "1111111111";
			secondaryTariffLine.CI_ChildType = ClassificationChildTypeList.Codes.COMPONENT;
			AssertEquals("HasSecondaryTariffLines", true, spiLine.HasSecondaryChildrenLines);
			AssertEquals("ParentTariffLine", null, spiLine.ParentTariffLine);
			int countOfSecondaryLine = 0;
			foreach (ISPILine secondarySPILine in spiLine.SecondaryTariffLines)
			{
				countOfSecondaryLine++;
				AssertEquals(new ZDateTime(2008, 7, 1), secondarySPILine.EffectiveDate);
				AssertEquals(typeof(SPINormalPivotLine), secondarySPILine.ParentTariffLine.GetType());
			}

			AssertEquals(1, countOfSecondaryLine);
			pivot.CD_ProductClaim = SecondarySpecProgIndicatorList.Codes.X;
			secondaryTariffLine.CD_ProductClaim = SecondarySpecProgIndicatorList.Codes.V;
			AssertEquals("HasSecondaryTariffLines", false, spiLine.HasSecondaryChildrenLines);
			AssertEquals("V lines should not be considered as secondary lines", 0, new List<ISPILine>(spiLine.SecondaryTariffLines).Count);
		}

		[TestDate(2008, 7, 1)]
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
			pivot.CI_DateStart = new ZDateTime(2008, 08, 01);
			var spiLine = SPILine.New(pivot);
			AssertEquals("EffectiveDate:Today for pivot", new ZDateTime(2008, 8, 1), spiLine.EffectiveDate);
			AssertEquals("ImportTariff", "0000000000", spiLine.ImportTariff.UE_Tariff);
			AssertEquals("AU", spiLine.CountryOfOrigin.UC_Code);
			AssertEquals("HasSecondaryTariffLines", false, spiLine.HasSecondaryChildrenLines);
			AssertEquals("ParentTariffLine", null, spiLine.ParentTariffLine);
			var secondaryTariffLine = pivot.Children.AddNew();
			secondaryTariffLine.CI_TariffNum = "1111111111";
			secondaryTariffLine.CI_ChildType = ClassificationChildTypeList.Codes.COMPONENT;
			AssertEquals("HasSecondaryTariffLines", true, spiLine.HasSecondaryChildrenLines);
			AssertEquals("ParentTariffLine", null, spiLine.ParentTariffLine);
			int countOfSecondaryLine = 0;
			foreach (ISPILine secondarySPILine in spiLine.SecondaryTariffLines)
			{
				countOfSecondaryLine++;
				AssertEquals(new ZDateTime(2008, 7, 1), secondarySPILine.EffectiveDate);
				AssertEquals(typeof(SPINormalPivotLine), secondarySPILine.ParentTariffLine.GetType());
			}

			AssertEquals(1, countOfSecondaryLine);
			pivot.CD_ProductClaim = SecondarySpecProgIndicatorList.Codes.X;
			secondaryTariffLine.CD_ProductClaim = SecondarySpecProgIndicatorList.Codes.V;
			AssertEquals("HasSecondaryTariffLines", false, spiLine.HasSecondaryChildrenLines);
			AssertEquals("V lines should not be considered as secondary lines", 0, new List<ISPILine>(spiLine.SecondaryTariffLines).Count);
		}
	}
}
