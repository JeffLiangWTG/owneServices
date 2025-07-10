using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ReconOrigNormalSPILineTest : TestCaseWithFactory
	{
		[TestDate(2008, 7, 1)]
		public void TestSPILineForReconOriginal()
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
			var reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			var originalEntry = reconDeclaration.OriginalEntries.AddNew();
			originalEntry.US_R_DutyRateDate = new ZDateTime(2007, 12, 31);
			var invoice = originalEntry.Invoice;
			var invoiceLine = reconDeclaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "0000000000";
			invoiceLine.US_R_OrigTariff = "0000000000";
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";
			var spiLine = new ReconOrigNormalSPILine(invoiceLine);
			AssertEquals("EffectiveDate", new ZDateTime(2007, 12, 31), spiLine.EffectiveDate);
			AssertEquals("ImportTariff", tariff, spiLine.ImportTariff);
			AssertEquals("AU", spiLine.CountryOfOrigin.UC_Code);
			AssertEquals("HasSecondaryTariffLines", false, spiLine.HasSecondaryChildrenLines);
			AssertEquals("ParentTariffLine", null, spiLine.ParentTariffLine);
			var secondaryTariffLine = invoiceLine.AddSecondaryInvoiceLine();
			secondaryTariffLine.JI_Tariff = "1111111111";
			secondaryTariffLine.US_R_OrigTariff = "1111111111";
			AssertEquals("HasSecondaryTariffLines", true, spiLine.HasSecondaryChildrenLines);
			AssertEquals("ParentTariffLine", null, spiLine.ParentTariffLine);
			int countOfSecondaryLine = 0;
			foreach (ISPILine secondarySPILine in spiLine.SecondaryTariffLines)
			{
				countOfSecondaryLine++;
				AssertEquals(new ZDateTime(2007, 12, 31), secondarySPILine.EffectiveDate);
				AssertEquals(typeof(ReconOrigNormalSPILine), secondarySPILine.ParentTariffLine.GetType());
				AssertEquals(tariff3, secondarySPILine.ImportTariff);
			}

			AssertEquals(1, countOfSecondaryLine);
			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.X;
			secondaryTariffLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;
			AssertEquals("HasSecondaryTariffLines", false, spiLine.HasSecondaryChildrenLines);
			AssertEquals("V lines should not be considered as secondary lines", 0, new List<ISPILine>(spiLine.SecondaryTariffLines).Count);
		}
	}
}
