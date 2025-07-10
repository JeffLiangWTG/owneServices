using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class CombinedLinesHelperTest : TestCaseWithFactory
	{
		public void TestExemptMPFForCombineLinesUseSGFTATariff()
		{
			var testJob = CombinedJob;
			var invoiceLine1 = InvoiceHeaderForCombined.InvoiceLines.AddNew();
			var invoiceLine2 = InvoiceHeaderForCombined.InvoiceLines.AddNew();

			invoiceLine1.US_SupTariff = Chapter98TestingHelper.Test99038801Tariff.UE_Tariff;
			invoiceLine2.JI_ParentID = invoiceLine1.PK;
			invoiceLine2.US_SupTariff = "99990084"; //SGFTA Tariff
			invoiceLine2.JI_Tariff = "8473290000";
			invoiceLine2.JI_LinePrice = 1000m;
			testJob.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			AssertNull(invoiceLine1.FeeCusCodes.GetFirstElementHaving(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));
			AssertNull(invoiceLine2.FeeCusCodes.GetFirstElementHaving(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));
		}

		public void TestShouldCalculate990388DutyForSection301()
		{
			Chapter98TestingHelper.Test9802006000Tariff.UE_DutyComputationCode = ComputationCodeList.Codes.NoComputationFormulaAvailable;
			Chapter98TestingHelper.Test9802004000Tariff.UE_DutyComputationCode = ComputationCodeList.Codes.NoComputationFormulaAvailable;
			Chapter98TestingHelper.Test9802005000Tariff.UE_DutyComputationCode = ComputationCodeList.Codes.NoComputationFormulaAvailable;
			Chapter98TestingHelper.Test9802008000Tariff.UE_DutyComputationCode = ComputationCodeList.Codes.NoComputationFormulaAvailable;

			var testJob = CombinedJob;
			var invoiceLine1 = InvoiceHeaderForCombined.InvoiceLines.AddNew();
			var invoiceLine2 = InvoiceHeaderForCombined.InvoiceLines.AddNew();

			invoiceLine1.US_SupTariff = Chapter98TestingHelper.Test99038801Tariff.UE_Tariff;
			invoiceLine2.JI_ParentID = invoiceLine1.PK;
			invoiceLine2.US_SupTariff = Chapter98TestingHelper.Test9802006000Tariff.UE_Tariff;
			invoiceLine2.JI_Tariff = Chapter98TestingHelper.TestCTariff.UE_Tariff;
			invoiceLine2.JI_LinePrice = 107501.15m;
			invoiceLine2.US_98GoodsValue = 136029.35m;
			testJob.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			AssertEquals("107501.15 * 40%", 43000.40m, invoiceLine1.US_SupDuty);

			invoiceLine2.US_SupTariff = Chapter98TestingHelper.Test9802004000Tariff.UE_Tariff;
			testJob.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();
			AssertEquals("107501.15 * 40%", 43000.40m, invoiceLine1.US_SupDuty);

			invoiceLine2.US_SupTariff = Chapter98TestingHelper.Test9802005000Tariff.UE_Tariff;
			testJob.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();
			AssertEquals("107501.15 * 40%", 43000.40m, invoiceLine1.US_SupDuty);

			invoiceLine2.US_SupTariff = Chapter98TestingHelper.Test9802008000Tariff.UE_Tariff;
			invoiceLine2.US_98GoodsValue = 7501.15m;
			testJob.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();
			AssertEquals("107501.15 * 40%", 43000.4m, invoiceLine1.US_SupDuty);
		}

		public void TestTwo9903s()
		{
			var testJob = CombinedJob;
			var invoiceLine1 = InvoiceHeaderForCombined.InvoiceLines.AddNew();
			var invoiceLine2 = InvoiceHeaderForCombined.InvoiceLines.AddNew();
			invoiceLine1.US_SupTariff = Chapter98TestingHelper.Test99038801Tariff.UE_Tariff;
			invoiceLine2.JI_ParentID = invoiceLine1.PK;
			invoiceLine2.US_SupTariff = Chapter98TestingHelper.Test99038802Tariff.UE_Tariff;
			invoiceLine2.JI_Tariff = Chapter98TestingHelper.TestCTariff.UE_Tariff;
			invoiceLine2.JI_LinePrice = 8000m;
			testJob.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			AssertEquals(3200m, invoiceLine1.US_SupDuty);
			AssertEquals(0m, invoiceLine1.US_Duty);
			AssertEquals(4000m, invoiceLine2.US_SupDuty);
			AssertEquals(5600m, invoiceLine2.US_Duty);
		}

		public void Test99088501()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, "HSN");
			var zzTariff = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.UnitedStates, tariffType.PK, "99038501", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.TYPE, UniversalReferenceConstants.TariffAttributeTypes.Values._232, zzTariff);

			new FeeCalculationHelperTest().PrepareFeeAndTexData();

			var testJob = CombinedJob;
			var invoiceLine1 = InvoiceHeaderForCombined.InvoiceLines.AddNew();
			var invoiceLine2 = InvoiceHeaderForCombined.InvoiceLines.AddNew();
			invoiceLine1.US_SupTariff = Chapter98TestingHelper.Test99038501Tariff.UE_Tariff;
			invoiceLine2.JI_ParentID = invoiceLine1.PK;
			invoiceLine2.US_SupTariff = Chapter98TestingHelper.Test9802005060Tariff.UE_Tariff;
			invoiceLine2.JI_Tariff = Chapter98TestingHelper.TestCTariff.UE_Tariff;
			invoiceLine2.JI_LinePrice = 8000m;
			invoiceLine2.US_98GoodsValue = 2000m;

			var invoiceLine3 = InvoiceHeaderForCombined.InvoiceLines.AddNew();
			var invoiceLine4 = InvoiceHeaderForCombined.InvoiceLines.AddNew();
			invoiceLine3.US_SupTariff = Chapter98TestingHelper.Test99038501Tariff.UE_Tariff;
			invoiceLine4.JI_ParentID = invoiceLine3.PK;
			invoiceLine4.US_SupTariff = Chapter98TestingHelper.Test9802006000Tariff.UE_Tariff;
			invoiceLine4.JI_Tariff = Chapter98TestingHelper.TestCTariff.UE_Tariff;
			invoiceLine4.JI_LinePrice = 8000m;
			invoiceLine4.US_98GoodsValue = 2000m;

			testJob.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			AssertEquals(0m, invoiceLine1.US_Duty);
			AssertEquals("8000 * 10%", 800m, invoiceLine1.US_SupDuty);
			AssertNull(invoiceLine1.FeeCusCodes.GetFirstElementHaving(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));

			AssertEquals("8000 * 70%", 5600m, invoiceLine2.US_Duty);
			AssertEquals(0m, invoiceLine2.US_SupDuty);
			AssertNull(invoiceLine2.FeeCusCodes.GetFirstElementHaving(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));

			AssertEquals(0m, invoiceLine3.US_Duty);
			AssertEquals("(8000 + 2000) * 10% = 1000", 1000m, invoiceLine3.US_SupDuty);
			AssertNull(invoiceLine3.FeeCusCodes.GetFirstElementHaving(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));

			AssertEquals("8000 * 70%", 5600m, invoiceLine4.US_Duty);
			AssertEquals(0m, invoiceLine4.US_SupDuty);
			AssertNotNull(invoiceLine4.FeeCusCodes.GetFirstElementHaving(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));
		}

		public void Test99038801WithADD()
		{
			var case1 = Factory.New<USCACCase>();
			case1.U5_CaseNumber = "A9085290";
			case1.U5_ISOCountryCode = "KR";
			case1.U5_CaseStatus = ACCaseStatusList.Codes.AC;
			case1.U5_CaseStatusDate = ZDateTime.BrettsBirthday;
			var caseRate = case1.CaseRates.AddNew();
			caseRate.U6_EffectiveDate = ZDateTime.BrettsBirthday;
			caseRate.U6_AdValoremRate = 0.52m;
			caseRate.U6_SpecificRate = 0.62m;
			Factory.Save();

			new FeeCalculationHelperTest().PrepareFeeAndTexData();

			var testJob = CombinedJob;
			var invoiceLine1 = InvoiceHeaderForCombined.InvoiceLines.AddNew();
			var invoiceLine2 = InvoiceHeaderForCombined.InvoiceLines.AddNew();
			invoiceLine1.US_SupTariff = Chapter98TestingHelper.Test99038801Tariff.UE_Tariff;
			invoiceLine2.JI_ParentID = invoiceLine1.PK;
			invoiceLine2.US_SupTariff = Chapter98TestingHelper.Test9802005060Tariff.UE_Tariff;
			invoiceLine2.JI_Tariff = Chapter98TestingHelper.TestCTariff.UE_Tariff;
			invoiceLine2.JI_LinePrice = 8000m;
			invoiceLine2.US_98GoodsValue = 2000m;

			var invoiceLine3 = InvoiceHeaderForCombined.InvoiceLines.AddNew();
			var invoiceLine4 = InvoiceHeaderForCombined.InvoiceLines.AddNew();
			invoiceLine3.US_SupTariff = Chapter98TestingHelper.Test99038801Tariff.UE_Tariff;
			invoiceLine4.JI_ParentID = invoiceLine3.PK;
			invoiceLine4.US_SupTariff = Chapter98TestingHelper.Test9802006000Tariff.UE_Tariff;
			invoiceLine4.JI_Tariff = Chapter98TestingHelper.TestCTariff.UE_Tariff;
			invoiceLine4.JI_LinePrice = 8000m;
			invoiceLine4.US_98GoodsValue = 2000m;
			invoiceLine4.US_ADDCaseNo = "A9085290";
			invoiceLine4.US_ADDDepositRateIndicator = "A";
			invoiceLine3.US_UC_NKCountryOfExport = "KR";
			invoiceLine3.US_UC_NKCountryOfOrigin = "KR";
			testJob.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			AssertEquals(0m, invoiceLine1.US_Duty);
			AssertEquals(3200m, invoiceLine1.US_SupDuty);
			AssertNull(invoiceLine1.FeeCusCodes.GetFirstElementHaving(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));

			AssertEquals(5600m, invoiceLine2.US_Duty);
			AssertEquals(0m, invoiceLine2.US_SupDuty);
			AssertNull(invoiceLine2.FeeCusCodes.GetFirstElementHaving(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));

			AssertEquals(0m, invoiceLine3.US_Duty);
			AssertEquals(3200m, invoiceLine3.US_SupDuty);
			AssertNull(invoiceLine3.FeeCusCodes.GetFirstElementHaving(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));

			AssertEquals(5600m, invoiceLine4.US_Duty);
			AssertEquals(0m, invoiceLine4.US_SupDuty);
			AssertNotNull(invoiceLine4.FeeCusCodes.GetFirstElementHaving(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));
			var addDuty = invoiceLine4.CusEntryLine.Fees.GetAmount(Core.Constants.USCustoms.FeeCodes.AntidumpingDuty);
			AssertEquals("AntiDumping Duty", 5200m, addDuty);
		}

		public void Test98020080()
		{
			new FeeCalculationHelperTest().PrepareFeeAndTexData();

			var testJob = CombinedJob;
			var invoiceLine1 = InvoiceHeaderForCombined.InvoiceLines.AddNew();
			var invoiceLine2 = InvoiceHeaderForCombined.InvoiceLines.AddNew();
			invoiceLine1.US_SupTariff = Chapter98TestingHelper.Test99038801Tariff.UE_Tariff;
			invoiceLine2.JI_ParentID = invoiceLine1.PK;
			invoiceLine2.US_SupTariff = Chapter98TestingHelper.Test99038802Tariff.UE_Tariff;

			var invoiceLine3 = InvoiceHeaderForCombined.InvoiceLines.AddNew();
			invoiceLine3.JI_ParentID = invoiceLine1.PK;
			invoiceLine3.US_SupTariff = Chapter98TestingHelper.Test98020080Tariff.UE_Tariff;
			invoiceLine3.JI_Tariff = Chapter98TestingHelper.TestCTariff.UE_Tariff;
			invoiceLine3.JI_LinePrice = 8000m;
			invoiceLine3.US_98GoodsValue = 2000m;
			testJob.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			AssertEquals(0m, invoiceLine1.US_Duty);
			AssertEquals("8000 * 40%", 3200m, invoiceLine1.US_SupDuty);
			AssertNull(invoiceLine1.FeeCusCodes.GetFirstElementHaving(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));

			AssertEquals(0m, invoiceLine2.US_Duty);
			AssertEquals("8000 * 50%", 4000m, invoiceLine2.US_SupDuty);
			AssertNull(invoiceLine2.FeeCusCodes.GetFirstElementHaving(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));

			AssertEquals("8000 * 70%", 5600m, invoiceLine3.US_Duty);
			AssertEquals(0m, invoiceLine3.US_SupDuty);
			AssertNotNull(invoiceLine3.FeeCusCodes.GetFirstElementHaving(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));
		}

		public void Test9817()
		{
			new FeeCalculationHelperTest().PrepareFeeAndTexData();
			var testJob = CombinedJob;
			var invoiceLine1 = InvoiceHeaderForCombined.InvoiceLines.AddNew();
			var invoiceLine2 = InvoiceHeaderForCombined.InvoiceLines.AddNew();
			invoiceLine1.US_SupTariff = Chapter98TestingHelper.Test99038801Tariff.UE_Tariff;
			invoiceLine2.JI_ParentID = invoiceLine1.PK;
			invoiceLine2.US_SupTariff = Chapter98TestingHelper.Test9817002000Tariff.UE_Tariff;
			invoiceLine2.JI_Tariff = Chapter98TestingHelper.TestCTariff.UE_Tariff;
			testJob.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();
			AssertEquals(0m, invoiceLine1.US_Duty);
			AssertEquals(0m, invoiceLine1.US_SupDuty);
			AssertNull(invoiceLine1.FeeCusCodes.GetFirstElementHaving(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));

			AssertEquals(0m, invoiceLine2.US_Duty);
			AssertEquals(0m, invoiceLine2.US_SupDuty);
			AssertNull(invoiceLine2.FeeCusCodes.GetFirstElementHaving(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));
		}

		public void TestTIBWithTwo9903()
		{
			new FeeCalculationHelperTest().PrepareFeeAndTexData();
			var testJob = CombinedJob;
			testJob.US_EntryType = "23";
			var invoiceLine1 = InvoiceHeaderForCombined.InvoiceLines.AddNew();
			var invoiceLine2 = InvoiceHeaderForCombined.InvoiceLines.AddNew();
			var invoiceLine3 = InvoiceHeaderForCombined.InvoiceLines.AddNew();
			invoiceLine1.US_SupTariff = Chapter98TestingHelper.Test9813Tariff.UE_Tariff;
			invoiceLine2.JI_ParentID = invoiceLine1.PK;
			invoiceLine2.US_SupTariff = Chapter98TestingHelper.Test99038801Tariff.UE_Tariff;
			invoiceLine3.JI_ParentID = invoiceLine1.PK;
			invoiceLine3.US_SupTariff = Chapter98TestingHelper.Test99038802Tariff.UE_Tariff;
			invoiceLine3.JI_Tariff = Chapter98TestingHelper.TestCTariff.UE_Tariff;
			invoiceLine3.JI_LinePrice = 10000m;

			testJob.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			AssertEquals(0m, invoiceLine1.US_Duty);
			AssertEquals(0m, invoiceLine1.US_SupDuty);
			AssertNull(invoiceLine1.FeeCusCodes.GetFirstElementHaving(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));

			AssertEquals(0m, invoiceLine2.US_Duty);
			AssertEquals(0m, invoiceLine2.US_SupDuty);
			AssertNull(invoiceLine2.FeeCusCodes.GetFirstElementHaving(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));

			AssertEquals(0m, invoiceLine3.US_Duty);
			AssertEquals(0m, invoiceLine3.US_SupDuty);
			AssertNull(invoiceLine3.FeeCusCodes.GetFirstElementHaving(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));

			testJob.US_BondType = "9";
			testJob.US_BondCalcCode = "TIB";
			AssertEquals(32070m, testJob.US_BondAmount);
		}

		public void TestIsExemptMPFForCombinedLines()
		{
			var testJob = CombinedJob;
			var invoiceLine1 = InvoiceHeaderForCombined.InvoiceLines.AddNew();
			var invoiceLine2 = InvoiceHeaderForCombined.InvoiceLines.AddNew();
			invoiceLine1.US_SupTariff = Chapter98TestingHelper.Test99038801Tariff.UE_Tariff;
			invoiceLine2.JI_ParentID = invoiceLine1.PK;
			invoiceLine2.US_SupTariff = Chapter98TestingHelper.Test99038802Tariff.UE_Tariff;
			invoiceLine2.JI_Tariff = Chapter98TestingHelper.TestCTariff.UE_Tariff;
			invoiceLine2.JI_LinePrice = 1000m;
			testJob.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			var ensEntryHeader = testJob.ActiveEntryHeaders.EntrySummaryEntry;
			var entryline1 = ensEntryHeader.MergedLines.Cast<CusEntryLine>().FirstOrDefault(x => x.CL_AdValoremTariff == invoiceLine1.US_SupTariff);
			var entryline2 = ensEntryHeader.MergedLines.Cast<CusEntryLine>().FirstOrDefault(x => x.CL_AdValoremTariff == invoiceLine2.US_SupTariff);
			var entryline3 = ensEntryHeader.MergedLines.Cast<CusEntryLine>().FirstOrDefault(x => x.CL_AdValoremTariff == invoiceLine2.JI_Tariff);

			Assert(!Chapter98Helper.IsExemptMPFForCombineLines(entryline1));
			Assert(!Chapter98Helper.IsExemptMPFForCombineLines(entryline2));
			Assert(!Chapter98Helper.IsExemptMPFForCombineLines(entryline3));

			invoiceLine2.US_SupTariff = Chapter98TestingHelper.Test9817002000Tariff.UE_Tariff;
			testJob.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			entryline1 = ensEntryHeader.MergedLines.Cast<CusEntryLine>().FirstOrDefault(x => x.CL_AdValoremTariff == invoiceLine1.US_SupTariff);
			entryline2 = ensEntryHeader.MergedLines.Cast<CusEntryLine>().FirstOrDefault(x => x.CL_AdValoremTariff == invoiceLine2.US_SupTariff);
			entryline3 = ensEntryHeader.MergedLines.Cast<CusEntryLine>().FirstOrDefault(x => x.CL_AdValoremTariff == invoiceLine2.JI_Tariff);

			Assert(Chapter98Helper.IsExemptMPFForCombineLines(entryline1));
			Assert(Chapter98Helper.IsExemptMPFForCombineLines(entryline2));
			Assert(Chapter98Helper.IsExemptMPFForCombineLines(entryline3));

			invoiceLine2.US_SupTariff = Chapter98TestingHelper.Test9802006000Tariff.UE_Tariff;
			testJob.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			entryline1 = ensEntryHeader.MergedLines.Cast<CusEntryLine>().FirstOrDefault(x => x.CL_AdValoremTariff == invoiceLine1.US_SupTariff);
			entryline2 = ensEntryHeader.MergedLines.Cast<CusEntryLine>().FirstOrDefault(x => x.CL_AdValoremTariff == invoiceLine2.US_SupTariff);
			entryline3 = ensEntryHeader.MergedLines.Cast<CusEntryLine>().FirstOrDefault(x => x.CL_AdValoremTariff == invoiceLine2.JI_Tariff);
			Assert(Chapter98Helper.IsExemptMPFForCombineLines(entryline1));
			Assert(Chapter98Helper.IsExemptMPFForCombineLines(entryline2));
			Assert(!Chapter98Helper.IsExemptMPFForCombineLines(entryline3));
		}

		public void TestPropertiesForChapter98Helper()
		{
			var testJob = CombinedJob;
			var invoiceLine1 = InvoiceHeaderForCombined.InvoiceLines.AddNew();
			var invoiceLine2 = InvoiceHeaderForCombined.InvoiceLines.AddNew();
			invoiceLine1.US_SupTariff = Chapter98TestingHelper.Test99038801Tariff.UE_Tariff;
			invoiceLine2.JI_ParentID = invoiceLine1.PK;
			invoiceLine2.US_SupTariff = Chapter98TestingHelper.Test9817002000Tariff.UE_Tariff;
			invoiceLine2.JI_Tariff = Chapter98TestingHelper.TestCTariff.UE_Tariff;
			invoiceLine2.JI_LinePrice = 1000m;
			testJob.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			Assert(!Chapter98Helper.IsCombineSecondaryTariffLine(invoiceLine1));
			Assert(Chapter98Helper.IsCombineSecondaryTariffLine(invoiceLine2));
			Assert(Chapter98Helper.Is98SecondaryTariffLine(invoiceLine2));
			Assert(!Chapter98Helper.Is98SecondaryTariffLine(invoiceLine1));

			var combinedParentLine = Chapter98Helper.GetCombineParentLine(invoiceLine2);
			Assert(combinedParentLine.SupTariffs.Contains(invoiceLine1.US_SupTariff));

			var invoiceLine3 = InvoiceHeaderForCombined.InvoiceLines.AddNew();
			invoiceLine3.JI_ParentID = invoiceLine1.PK;
			invoiceLine3.US_SupTariff = Chapter98TestingHelper.Test99038501Tariff.UE_Tariff;
			testJob.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();
			Assert(Chapter98Helper.Is99SecondaryTariffLine(invoiceLine3));
		}

		[TestDate(2025, 02, 01)]
		public void TestDutyAndCustomsValueForMultipleProvTariffsOnOneInvoiceLine()
		{
			#region Setup Tariffs
			var tariff99038803 = USCTariffTest.CreateNewTariffIfNotExist(Factory, "99038803", "7", 0.25m, ZString.Empty);
			var tariff99030120 = USCTariffTest.CreateNewTariffIfNotExist(Factory, "99030120", "7", 0.1m, ZString.Empty);
			var tariff8424201000 = USCTariffTest.CreateNewTariffIfNotExist(Factory, "8424201000", "7", 0.029m, "X");

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var hsnTariffType = helper.CreateNewOrGetExistingTariffType("US", "HSN");
			var dutyRateType = helper.CreateNewOrGetExistingRateType("US", "DTY", "Duty");
			var rateCode = helper.LoadOrCreateNewCusRateCode(Factory, "DTY", dutyRateType.PK);
			var tradeGroup = helper.CreateTradeGroup("CN", "CN", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);

			var tariffView99030120 = helper.CreateTariff("US", hsnTariffType.PK, "99030120", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			var rate99030120 = helper.CreateRate(tariffView99030120, rateCode.PK, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date, "0.1");
			helper.CreateCusApplicability(rate99030120, tradeGroup, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateTariffAttribute("RULE", "A99", tariffView99030120);
			Factory.Save();

			#endregion

			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.Yes;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = tariff8424201000.UE_Tariff;
			invoiceLine.US_SupTariff = tariff99038803.UE_Tariff;
			invoiceLine.SupFormattedAdditionalTariff1 = tariff99030120.UE_Tariff;
			invoiceLine.JI_LinePrice = 12000m;
			invoiceLine.JI_CustomsQuantity = 10m;
			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.China;
			invoiceLine.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.China;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			CombineAssertions("Customs Value & Duty on invoice lines", () =>
			{
				AssertEquals("Duty on invoice line", 348m, invoiceLine.US_Duty);
				AssertEquals("Sup Duty 1 on invoice line", 3000m, invoiceLine.US_SupDuty);
				AssertEquals("Sup Duty 2 on invoice line", 1200m, invoiceLine.US_SupAdditionalTariff1Duty);
				AssertEquals("Customs value on invoice line", 12000m, invoiceLine.JI_CustomsValue);

				var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
				var entryLine8424201000 = entry.AllEntryLines.FirstOrDefault(x => x.CL_AdValoremTariff == tariff8424201000.UE_Tariff);
				AssertEquals("Customs value on entry line 8424201000", 12000m, entryLine8424201000.CL_CustomsValue);
				var entryLine99038803 = entry.AllEntryLines.FirstOrDefault(x => x.CL_AdValoremTariff == tariff99038803.UE_Tariff);
				AssertEquals("Customs value on entry line 99038803", 0m, entryLine99038803.CL_CustomsValue);
				var entryLine99030120 = entry.AllEntryLines.FirstOrDefault(x => x.CL_AdValoremTariff == tariff99030120.UE_Tariff);
				AssertEquals("Customs value on entry line 99030120", 0m, entryLine99030120.CL_CustomsValue);
			});
		}

		[TestDate(2025, 02, 01)]
		public void TestDutyAndCustomsValueForXVVSetsWithMultipleProvTariffs()
		{
			#region Setup Tariffs
			var tariff99038803 = USCTariffTest.CreateNewTariffIfNotExist(Factory, "99038803", "7", 0.25m, ZString.Empty);
			var tariff99030123 = USCTariffTest.CreateNewTariffIfNotExist(Factory, "99030123", "0", 0m, ZString.Empty);
			var tariff8424201000 = USCTariffTest.CreateNewTariffIfNotExist(Factory, "8424201000", "7", 0.029m, "X");
			var tariff9503000013 = USCTariffTest.CreateNewTariffIfNotExist(Factory, "9503000013", "7", 0m, "NO");

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var hsnTariffType = helper.CreateNewOrGetExistingTariffType("US", "HSN");
			var dutyRateType = helper.CreateNewOrGetExistingRateType("US", "DTY", "Duty");
			var rateCode = helper.LoadOrCreateNewCusRateCode(Factory, "DTY", dutyRateType.PK);
			var tradeGroup = helper.CreateTradeGroup("CN", "CN", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);

			var tariffView99030123 = helper.CreateTariff("US", hsnTariffType.PK, "99030123", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			var rate99030123 = helper.CreateRate(tariffView99030123, rateCode.PK, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date, "0");
			helper.CreateCusApplicability(rate99030123, tradeGroup, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateTariffAttribute("RULE", "A99", tariffView99030123);
			Factory.Save();

			#endregion

			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.Yes;
			var invoiceHeader = declaration.Invoices.AddNew();
			var xInvoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			xInvoiceLine.US_SetInd = "X";
			xInvoiceLine.JI_Tariff = tariff8424201000.UE_Tariff;
			xInvoiceLine.US_SupTariff = tariff99038803.UE_Tariff;
			xInvoiceLine.SupFormattedAdditionalTariff1 = tariff99030123.UE_Tariff;
			xInvoiceLine.JI_LinePrice = 0m;
			xInvoiceLine.JI_CustomsQuantity = 10m;
			xInvoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.China;
			xInvoiceLine.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.China;

			var vParentInvoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			vParentInvoiceLine.JI_ParentID = xInvoiceLine.PK;
			vParentInvoiceLine.US_SetInd = "V";
			vParentInvoiceLine.JI_Tariff = tariff8424201000.UE_Tariff;
			vParentInvoiceLine.US_SupTariff = tariff99038803.UE_Tariff;
			vParentInvoiceLine.SupFormattedAdditionalTariff1 = tariff99030123.UE_Tariff;
			vParentInvoiceLine.JI_LinePrice = 10000m;
			vParentInvoiceLine.JI_CustomsQuantity = 10m;
			vParentInvoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.China;
			vParentInvoiceLine.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.China;

			var vChildInvoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			vChildInvoiceLine.JI_ParentID = xInvoiceLine.PK;
			vChildInvoiceLine.US_SetInd = "V";
			vChildInvoiceLine.JI_Tariff = tariff9503000013.UE_Tariff;
			vChildInvoiceLine.US_SupTariff = tariff99030123.UE_Tariff;
			vChildInvoiceLine.JI_LinePrice = 2000m;
			vChildInvoiceLine.JI_CustomsQuantity = 10m;
			vChildInvoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.China;
			vChildInvoiceLine.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.China;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			CombineAssertions("Customs Value & Duty on invoice lines", () =>
			{
				var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
				AssertEquals("Duty on X invoice line", 348m, xInvoiceLine.US_Duty);
				AssertEquals("Sup Duty 1 on X invoice line", 3000m, xInvoiceLine.US_SupDuty);
				AssertEquals("Sup Duty 2 on X invoice line", 0m, xInvoiceLine.US_SupAdditionalTariff1Duty);
				AssertEquals("Customs value on X invoice line", 12000m, xInvoiceLine.JI_CustomsValue);

				var entryLine8424201000XInvoiceLine = entry.AllEntryLines.FirstOrDefault(x => x.CL_AdValoremTariff == tariff8424201000.UE_Tariff && x.RandomLine.PK == xInvoiceLine.PK);
				AssertEquals("Customs value on entry line 8424201000 for X invoice line", 0m, entryLine8424201000XInvoiceLine.CL_CustomsValue);
				var entryLine99038803XInvoiceLine = entry.AllEntryLines.FirstOrDefault(x => x.CL_AdValoremTariff == tariff99038803.UE_Tariff && x.RandomLine.PK == xInvoiceLine.PK);
				AssertEquals("Customs value on entry line 99038803 for X invoice line", 0m, entryLine99038803XInvoiceLine.CL_CustomsValue);
				var entryLine99030120XInvoiceLine = entry.AllEntryLines.FirstOrDefault(x => x.CL_AdValoremTariff == tariff99030123.UE_Tariff && x.RandomLine.PK == xInvoiceLine.PK);
				AssertEquals("Customs value on entry line 99030123 for X invoice line", 12000m, entryLine99030120XInvoiceLine.CL_CustomsValue);

				AssertEquals("Duty on first V invoice line", 0m, vParentInvoiceLine.US_Duty);
				AssertEquals("Sup Duty 1 on first V invoice line", 0m, vParentInvoiceLine.US_SupDuty);
				AssertEquals("Sup Duty 2 on first V invoice line", 0m, vParentInvoiceLine.US_SupAdditionalTariff1Duty);
				AssertEquals("Customs value on first V invoice line", 10000m, vParentInvoiceLine.JI_CustomsValue);

				var entryLine8424201000VParentInvoiceLine = entry.AllEntryLines.FirstOrDefault(x => x.CL_AdValoremTariff == tariff8424201000.UE_Tariff && x.RandomLine.PK == vParentInvoiceLine.PK);
				AssertEquals("Customs value on entry line 8424201000 for first V invoice line", 0m, entryLine8424201000VParentInvoiceLine.CL_CustomsValue);
				var entryLine99038803VParentInvoiceLine = entry.AllEntryLines.FirstOrDefault(x => x.CL_AdValoremTariff == tariff99038803.UE_Tariff && x.RandomLine.PK == vParentInvoiceLine.PK);
				AssertEquals("Customs value on entry line 99038803 for first V invoice line", 0m, entryLine99038803VParentInvoiceLine.CL_CustomsValue);
				var entryLine99030120VParentInvoiceLine = entry.AllEntryLines.FirstOrDefault(x => x.CL_AdValoremTariff == tariff99030123.UE_Tariff && x.RandomLine.PK == vParentInvoiceLine.PK);
				AssertEquals("Customs value on entry line 99030123 for first V invoice line", 10000m, entryLine99030120VParentInvoiceLine.CL_CustomsValue);

				AssertEquals("Duty on second V invoice line", 0m, vChildInvoiceLine.US_Duty);
				AssertEquals("Sup Duty 1 on second V invoice line", 0m, vChildInvoiceLine.US_SupDuty);
				AssertEquals("Sup Duty 2 on second V invoice line", 0m, vChildInvoiceLine.US_SupAdditionalTariff1Duty);
				AssertEquals("Customs value on second V invoice line", 2000m, vChildInvoiceLine.JI_CustomsValue);

				var entryLine9503000013VChildInvoiceLine = entry.AllEntryLines.FirstOrDefault(x => x.CL_AdValoremTariff == tariff9503000013.UE_Tariff && x.RandomLine.PK == vChildInvoiceLine.PK);
				AssertEquals("Customs value on entry line 8424201000 for first V invoice line", 0m, entryLine9503000013VChildInvoiceLine.CL_CustomsValue);
				var entryLine99030120VChildInvoiceLine = entry.AllEntryLines.FirstOrDefault(x => x.CL_AdValoremTariff == tariff99030123.UE_Tariff && x.RandomLine.PK == vChildInvoiceLine.PK);
				AssertEquals("Customs value on entry line 99030123 for first V invoice line", 2000m, entryLine99030120VChildInvoiceLine.CL_CustomsValue);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			CusFeeCodeConstantsTestHelper.CreateCusFeeCodeDescriptionPairListForTest();
		}

		public JobDeclaration CombinedJob
		{
			get
			{
				if (combinedJob == null)
				{
					combinedJob = Factory.NewWithValidTestData<JobDeclaration>();
					combinedJob.JE_MessageType = JobMessageTypeList.Codes.Import;
					combinedJob.US_EntryFilerCode = "XJ5";
					combinedJob.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
					combinedJob.US_EnableENS = true;
					invoiceHeader = combinedJob.Invoices.AddNew();
					Factory.Save();
				}
				return combinedJob;
			}
		}
		JobDeclaration combinedJob;
		JobComInvoiceHeader invoiceHeader;

		public JobComInvoiceHeader InvoiceHeaderForCombined
		{
			get { return invoiceHeader; }
		}

		public Chapter98HelperTest Chapter98TestingHelper
		{
			get
			{
				if (chapter98HelperTest == null)
				{
					chapter98HelperTest = new Chapter98HelperTest();
				}
				return chapter98HelperTest;
			}
		}
		Chapter98HelperTest chapter98HelperTest;
	}
}
