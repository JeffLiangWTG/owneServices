using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class MPFAndInformalFeeExemptConditionCheckerTest : TestCaseWithFactory
	{
		public void TestIsExemptForChapter98()
		{
			var checker = new MPFAndInformalFeeExemptConditionChecker(false);
			var testHelper = new CombinedLinesHelperTest();
			var testJob = testHelper.CombinedJob;
			var invoiceLine1 = testHelper.InvoiceHeaderForCombined.InvoiceLines.AddNew();
			var invoiceLine2 = testHelper.InvoiceHeaderForCombined.InvoiceLines.AddNew();
			invoiceLine1.US_SupTariff = testHelper.Chapter98TestingHelper.Test99038801Tariff.UE_Tariff;
			invoiceLine2.JI_ParentID = invoiceLine1.PK;
			invoiceLine2.US_SupTariff = testHelper.Chapter98TestingHelper.Test99038802Tariff.UE_Tariff;
			invoiceLine2.JI_Tariff = testHelper.Chapter98TestingHelper.TestCTariff.UE_Tariff;
			invoiceLine2.JI_LinePrice = 1000m;
			testJob.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			var ensEntryHeader = testJob.ActiveEntryHeaders.EntrySummaryEntry;
			var entryline1 = ensEntryHeader.MergedLines.Cast<CusEntryLine>().FirstOrDefault(x => x.CL_AdValoremTariff == invoiceLine1.US_SupTariff);
			var entryline2 = ensEntryHeader.MergedLines.Cast<CusEntryLine>().FirstOrDefault(x => x.CL_AdValoremTariff == invoiceLine2.US_SupTariff);
			var entryline3 = ensEntryHeader.MergedLines.Cast<CusEntryLine>().FirstOrDefault(x => x.CL_AdValoremTariff == invoiceLine2.JI_Tariff);

			Assert(!checker.IsExempt(entryline1));
			Assert(!checker.IsExempt(entryline2));
			Assert(!checker.IsExempt(entryline3));

			invoiceLine2.US_SupTariff = testHelper.Chapter98TestingHelper.Test9817002000Tariff.UE_Tariff;
			testJob.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			entryline1 = ensEntryHeader.MergedLines.Cast<CusEntryLine>().FirstOrDefault(x => x.CL_AdValoremTariff == invoiceLine1.US_SupTariff);
			entryline2 = ensEntryHeader.MergedLines.Cast<CusEntryLine>().FirstOrDefault(x => x.CL_AdValoremTariff == invoiceLine2.US_SupTariff);
			entryline3 = ensEntryHeader.MergedLines.Cast<CusEntryLine>().FirstOrDefault(x => x.CL_AdValoremTariff == invoiceLine2.JI_Tariff);

			Assert(checker.IsExempt(entryline1));
			Assert(checker.IsExempt(entryline2));
			Assert(checker.IsExempt(entryline3));

			invoiceLine2.US_SupTariff = testHelper.Chapter98TestingHelper.Test9802006000Tariff.UE_Tariff;
			testJob.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			entryline1 = ensEntryHeader.MergedLines.Cast<CusEntryLine>().FirstOrDefault(x => x.CL_AdValoremTariff == invoiceLine1.US_SupTariff);
			entryline2 = ensEntryHeader.MergedLines.Cast<CusEntryLine>().FirstOrDefault(x => x.CL_AdValoremTariff == invoiceLine2.US_SupTariff);
			entryline3 = ensEntryHeader.MergedLines.Cast<CusEntryLine>().FirstOrDefault(x => x.CL_AdValoremTariff == invoiceLine2.JI_Tariff);
			Assert(checker.IsExempt(entryline1));
			Assert(checker.IsExempt(entryline2));
			Assert(!checker.IsExempt(entryline3));
		}

		public void TestIsExemptForCBICountry()
		{
			new FeeCalculationHelperTest().PrepareFeeAndTexData();
			invoiceLine.US_UC_NKCountryOfOrigin = "KR";
			invoiceLine.JI_LinePrice = 1100m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertNotEquals("MPF is not exempt", 0m, invoiceLine.FeeCusCodes.GetFirstElementHaving(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing).CY_FeeAmount);

			invoiceLine.US_UC_NKCountryOfOrigin = "TT";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertNull("MPF should be exempt", invoiceLine.FeeCusCodes.GetFirstElementHaving(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));
		}

		public void TestIsExemptFor98TariffNumbers()
		{
			invoiceLine.JI_Tariff = "9801001035";
			AssertEquals("should be exempt as it is a 98 tariff number", true, new MPFAndInformalFeeExemptConditionChecker(false).IsExempt(invoiceLine));

			invoiceLine.US_SupTariff = "98020060";
			AssertEquals("one of the 98 number that is not exempt", true, new MPFAndInformalFeeExemptConditionChecker(false).IsExempt(((IDutyData)invoiceLine).ParentTariffLine));

			AssertEquals("it was told to ignore TIB exemption condition", false, new MPFAndInformalFeeExemptConditionChecker(true).IsExempt(invoiceLine));
		}

		//CMR Declaration B00152401
		public void TestIsExemptFor99TariffNumbers()
		{
			invoiceLine.US_SupTariff = "9999.00.84";
			AssertEquals("should be exempt", true, new MPFAndInformalFeeExemptConditionChecker(false).IsExempt(((IDutyData)invoiceLine).ParentTariffLine));

			invoiceLine.US_SupTariff = "99070125";
			AssertEquals(false, new MPFAndInformalFeeExemptConditionChecker(false).IsExempt(((IDutyData)invoiceLine).ParentTariffLine));
		}

		public void TestIsExemptForDomesticMerchandise()
		{
			invoiceLine.US_ZoneStatus = ZoneStatusList.Codes.Domestic;
			invoiceLine.Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;

			AssertEquals(true, new MPFAndInformalFeeExemptConditionChecker(false).IsExempt(invoiceLine));

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals(true, new MPFAndInformalFeeExemptConditionChecker(false).IsExempt(invoiceLine.CusEntryLine));
		}

		public void TestIsExemptForPrimarySPI()
		{
			invoiceLine.US_SPI = PrimarySpecProgramIndicatorList.Codes.E;
			AssertEquals("line's primary SPI should be E", PrimarySpecProgramIndicatorList.Codes.E, invoiceLine.US_SPI);
			AssertEquals(true, new MPFAndInformalFeeExemptConditionChecker(false).IsExempt(invoiceLine));

			invoiceLine.US_SPI = PrimarySpecProgramIndicatorList.Codes.A;
			AssertEquals(false, new MPFAndInformalFeeExemptConditionChecker(false).IsExempt(invoiceLine));

			invoiceLine.US_SPI = PrimarySpecProgramIndicatorList.Codes.P;
			AssertEquals(true, new MPFAndInformalFeeExemptConditionChecker(false).IsExempt(invoiceLine));

			invoiceLine.US_SPI = PrimarySpecProgramIndicatorList.Codes.B;
			AssertEquals(false, new MPFAndInformalFeeExemptConditionChecker(false).IsExempt(invoiceLine));

			invoiceLine.US_SPI = PrimarySpecProgramIndicatorList.Codes.R;
			AssertEquals(true, new MPFAndInformalFeeExemptConditionChecker(false).IsExempt(invoiceLine));

			invoiceLine.US_SPI = PrimarySpecProgramIndicatorList.Codes.L;
			AssertEquals(false, new MPFAndInformalFeeExemptConditionChecker(false).IsExempt(invoiceLine));

			invoiceLine.US_SPI = PrimarySpecProgramIndicatorList.Codes.Y;
			AssertEquals(true, new MPFAndInformalFeeExemptConditionChecker(false).IsExempt(invoiceLine));

			invoiceLine.US_SPI = PrimarySpecProgramIndicatorList.Codes.K;
			AssertEquals(false, new MPFAndInformalFeeExemptConditionChecker(false).IsExempt(invoiceLine));

			invoiceLine.US_SPI = PrimarySpecProgramIndicatorList.Codes.W;
			AssertEquals(true, new MPFAndInformalFeeExemptConditionChecker(false).IsExempt(invoiceLine));
		}

		[TestDate(2011, 11, 1)]
		public void TestIsExemptForSPICountry()
		{
			invoiceLine.US_SPI = SpecialProgramList.Codes.BH;
			AssertEquals("line's primary SPI should be BH", SpecialProgramList.Codes.BH, invoiceLine.US_SPI);
			AssertEquals(true, new MPFAndInformalFeeExemptConditionChecker(false).IsExempt(invoiceLine));

			invoiceLine.US_SupTariff = "9914.99.20";
			AssertEquals(false, new MPFAndInformalFeeExemptConditionChecker(false).IsExempt(((IDutyData)invoiceLine).ParentTariffLine));

			invoiceLine.US_SPI = SpecialProgramList.Codes.S;
			AssertEquals(true, new MPFAndInformalFeeExemptConditionChecker(false).IsExempt(invoiceLine));

			invoiceLine.US_SPI = SpecialProgramList.Codes.SPlus;
			AssertEquals(true, new MPFAndInformalFeeExemptConditionChecker(false).IsExempt(invoiceLine));

			invoiceLine.US_SupTariff = "9999.00.50";
			AssertEquals(false, new MPFAndInformalFeeExemptConditionChecker(false).IsExempt(((IDutyData)invoiceLine).ParentTariffLine));

			invoiceLine.US_SPI = SpecialProgramList.Codes.S;
			AssertEquals(false, new MPFAndInformalFeeExemptConditionChecker(false).IsExempt(((IDutyData)invoiceLine).ParentTariffLine));
			AssertEquals(true, new MPFAndInformalFeeExemptConditionChecker(false).IsExempt(invoiceLine));

			invoiceLine.US_SPI = SpecialProgramList.Codes.SPlus;
			AssertEquals(true, new MPFAndInformalFeeExemptConditionChecker(false).IsExempt(invoiceLine));

			invoiceLine.US_SupTariff = "9999.00.60";
			AssertEquals(false, new MPFAndInformalFeeExemptConditionChecker(false).IsExempt(((IDutyData)invoiceLine).ParentTariffLine));

			invoiceLine.US_SPI = SpecialProgramList.Codes.S;
			AssertEquals(false, new MPFAndInformalFeeExemptConditionChecker(false).IsExempt(((IDutyData)invoiceLine).ParentTariffLine));

			invoiceLine.US_SPI = SpecialProgramList.Codes.IL;
			AssertEquals(false, new MPFAndInformalFeeExemptConditionChecker(false).IsExempt(invoiceLine));

			invoiceLine.US_SPI = SpecialProgramList.Codes.CA;
			AssertEquals(true, new MPFAndInformalFeeExemptConditionChecker(false).IsExempt(invoiceLine));

			invoiceLine.US_SupTariff = "9999.00.50";
			AssertEquals(false, new MPFAndInformalFeeExemptConditionChecker(false).IsExempt(((IDutyData)invoiceLine).ParentTariffLine));

			invoiceLine.US_SPI = SpecialProgramList.Codes.CL;
			AssertEquals(true, new MPFAndInformalFeeExemptConditionChecker(false).IsExempt(invoiceLine));

			invoiceLine.US_SupTariff = "9911.99.20";
			AssertEquals(false, new MPFAndInformalFeeExemptConditionChecker(false).IsExempt(((IDutyData)invoiceLine).ParentTariffLine));

			invoiceLine.US_SPI = SpecialProgramList.Codes.AU;
			AssertEquals(true, new MPFAndInformalFeeExemptConditionChecker(false).IsExempt(invoiceLine));

			invoiceLine.US_SPI = SpecialProgramList.Codes.KR;
			AssertEquals(true, new MPFAndInformalFeeExemptConditionChecker(false).IsExempt(invoiceLine));

			invoiceLine.US_SPI = SpecialProgramList.Codes.CO;
			AssertEquals(true, new MPFAndInformalFeeExemptConditionChecker(false).IsExempt(invoiceLine));

			invoiceLine.US_SPI = SpecialProgramList.Codes.PA;
			AssertEquals(true, new MPFAndInformalFeeExemptConditionChecker(false).IsExempt(invoiceLine));

			invoiceLine.US_SPI = SpecialProgramList.Codes.MX;
			AssertEquals(true, new MPFAndInformalFeeExemptConditionChecker(false).IsExempt(invoiceLine));

			invoiceLine.US_SupTariff = "9999.00.60";
			AssertEquals(false, new MPFAndInformalFeeExemptConditionChecker(false).IsExempt(((IDutyData)invoiceLine).ParentTariffLine));

			invoiceLine.US_SPI = SpecialProgramList.Codes.OM;
			AssertEquals(true, new MPFAndInformalFeeExemptConditionChecker(false).IsExempt(invoiceLine));

			invoiceLine.US_SupTariff = "9916.99.20";
			AssertEquals(false, new MPFAndInformalFeeExemptConditionChecker(false).IsExempt(((IDutyData)invoiceLine).ParentTariffLine));

			invoiceLine.US_SPI = SpecialProgramList.Codes.PE;
			AssertEquals(true, new MPFAndInformalFeeExemptConditionChecker(false).IsExempt(invoiceLine));

			invoiceLine.US_SPI = SpecialProgramList.Codes.PPlus;
			AssertEquals(true, new MPFAndInformalFeeExemptConditionChecker(false).IsExempt(invoiceLine));

			invoiceLine.US_SPI = SpecialProgramList.Codes.SG;
			AssertEquals(true, new MPFAndInformalFeeExemptConditionChecker(false).IsExempt(invoiceLine));

			invoiceLine.US_SupTariff = "9910.61.09";
			AssertEquals(false, new MPFAndInformalFeeExemptConditionChecker(false).IsExempt(((IDutyData)invoiceLine).ParentTariffLine));

			invoiceLine.US_SPI = SpecialProgramList.Codes.BSharp;
			AssertEquals(true, new MPFAndInformalFeeExemptConditionChecker(false).IsExempt(invoiceLine));

			invoiceLine.US_SPI = SpecialProgramList.Codes.CSharp;
			AssertEquals(true, new MPFAndInformalFeeExemptConditionChecker(false).IsExempt(invoiceLine));

			invoiceLine.US_SPI = SpecialProgramList.Codes.KSharp;
			AssertEquals(true, new MPFAndInformalFeeExemptConditionChecker(false).IsExempt(invoiceLine));

			invoiceLine.US_SPI = SpecialProgramList.Codes.LSharp;
			AssertEquals(true, new MPFAndInformalFeeExemptConditionChecker(false).IsExempt(invoiceLine));
		}

		public void TestIsExemptForCountryOfOrigin()
		{
			invoice.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Cambodia;
			AssertEquals("preCondition", Core.Constants.CountryCodes.Cambodia, invoiceLine.US_UC_NKCountryOfOrigin);
			AssertEquals("preCondition", true, invoiceLine.CountryOfOrigin_US.IsLeastDevelopedCountry(ZDateTime.Today));
			AssertEquals(true, new MPFAndInformalFeeExemptConditionChecker(false).IsExempt(invoiceLine));

			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.KoreaSouth;
			AssertEquals(false, new MPFAndInformalFeeExemptConditionChecker(false).IsExempt(invoiceLine));

			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Israel;
			AssertEquals(true, new MPFAndInformalFeeExemptConditionChecker(false).IsExempt(invoiceLine));

			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.SaintKittsAndNevis;
			AssertEquals(true, new MPFAndInformalFeeExemptConditionChecker(false).IsExempt(invoiceLine));
		}

		//Customs MPF exemption depends on the SPI indicator rather than tariff number. Should follow the suit...
		//B00151455 in CMR
		[TestDate(2009, 6, 1)]
		public void TestMPFCalculationFor99WithoutSPI()
		{
			new FeeCalculationHelperTest().PrepareFeeAndTexData();
			invoiceLine.US_SupTariff = "9911.77.04";
			invoiceLine.JI_LinePrice = 10000m;
			invoiceLine.JI_Tariff = "0811.10.0050";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("MPF payable due to no SPI entered", 25m, declaration.CustomsEntryHeaders[0].MPFAmountForEntry);

			invoiceLine.US_SPI = "CL";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("MPF exempt due to SPI", 0m, declaration.CustomsEntryHeaders[0].MPFAmountForEntry);
		}

		public void TestMPFAndInformalFeeExemptConditionCheckerWithCalculationFlagWhenS()
		{
			var checker = new MPFAndInformalFeeExemptConditionChecker(false);

			var reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			var entry = reconDeclaration.OriginalEntries.AddNew();
			entry.US_PaymentDate = new ZDateTime(2020, 9, 10);
			entry.US_R_DateForMPFCalc = new ZDateTime(2020, 9, 12);
			entry.US_R_DutyRateDate = new ZDateTime(2020, 9, 2);

			var line = entry.Invoice.JobComInvoiceLines.AddNew();
			line.JI_Tariff = "3920992000";
			line.JI_LinePrice = 10000m;
			line.US_R_OrigCV = 10000m;
			line.US_UC_NKCountryOfOrigin = "XC";
			line.US_UC_NKCountryOfExport = "CA";
			line.US_R_OrigSPI = "N/A";
			line.US_SPI = "S";
			AssertEquals(true, checker.IsExempt(line));
		}

		public void TestIsExemptForInsularPossessions()
		{
			new FeeCalculationHelperTest().PrepareFeeAndTexData();
			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Guam;
			invoiceLine.JI_LinePrice = 1100m;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertNull("MPF should be exempt", invoiceLine.FeeCusCodes.GetFirstElementHaving(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));

			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.VirginIslands;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertNull("MPF should be exempt", invoiceLine.FeeCusCodes.GetFirstElementHaving(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));

			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.AmericanSamoa;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertNull("MPF should be exempt", invoiceLine.FeeCusCodes.GetFirstElementHaving(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));

			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.NorthernMarianaIslands;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertNull("MPF should be exempt", invoiceLine.FeeCusCodes.GetFirstElementHaving(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));

			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Honduras;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertNotEquals("MPF is not exempt", 0m, invoiceLine.FeeCusCodes.GetFirstElementHaving(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing).CY_FeeAmount);
		}

		protected override void SetUp()
		{
			base.SetUp();
			CusFeeCodeConstantsTestHelper.CreateCusFeeCodeDescriptionPairListForTest();

			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";

			invoice = declaration.Invoices.AddNew();
			invoiceLine = declaration.InvoiceLines.AddNew();
		}
		JobDeclaration declaration;
		JobComInvoiceHeader invoice;
		JobComInvoiceLine invoiceLine;
	}
}
