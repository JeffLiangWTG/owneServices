using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class LineDutyFeeCalculatorTest : TestCaseWithFactory
	{
		[TestDate(2019, 04, 28)]
		public void TestXVVSetsOnReconJob()
		{
			var testHelper = new Chapter98HelperTest();

			var declaration = Factory.New<JobDeclaration>();
			var reconDec = new ReconDeclaration(declaration);
			reconDec.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			var originalEntry = reconDec.OriginalEntries.AddNew();

			originalEntry = reconDec.OriginalEntries.AddNew();
			originalEntry.US_R_DateForMPFCalc = ZDateTime.Today;
			originalEntry.US_R_DutyRateDate = ZDateTime.Today;

			var invoice = originalEntry.Invoice;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.US_CH_ReconEntry = originalEntry.CH_PK;

			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "8409.99.9190";
			invoiceLine1.US_SupTariff = testHelper.Test99038801Tariff.UE_Tariff; // Rate = 40%
			invoiceLine1.US_SecondarySPI = "X";
			invoiceLine1.JI_LinePrice = 3000m;

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "8409.99.9190";
			invoiceLine2.US_SupTariff = testHelper.Test99038801Tariff.UE_Tariff;
			invoiceLine2.JI_ParentID = invoiceLine1.JI_ParentID;
			invoiceLine2.US_SecondarySPI = "V";
			invoiceLine2.JI_LinePrice = 2000m;

			var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_Tariff = "7318.22.0000";
			invoiceLine3.US_SupTariff = testHelper.Test99038801Tariff.UE_Tariff;
			invoiceLine3.JI_ParentID = invoiceLine1.JI_ParentID;
			invoiceLine3.US_SecondarySPI = "V";
			invoiceLine3.JI_LinePrice = 1000m;
			reconDec.CalculateDutyFeesForAllEntries();

			AssertEquals("3000 * 2.5%", 75m, invoiceLine1.US_Duty);
			AssertEquals("3000 * 40%", 1200m, invoiceLine1.US_SupDuty);
			AssertEquals(0m, invoiceLine2.US_Duty);
			AssertEquals(0m, invoiceLine2.US_SupDuty);
			AssertEquals(0m, invoiceLine3.US_Duty);
			AssertEquals(0m, invoiceLine3.US_SupDuty);

			invoiceLine1.JI_LinePrice = 10000m;
			reconDec.CalculateDutyFeesForAllEntries();
			AssertEquals("Duty should be changed, 1000 * 2.5%", 250m, invoiceLine1.US_Duty);
			AssertEquals("Prov/Prog duty should be changed, 10000 * 40%", 4000m, invoiceLine1.US_SupDuty);
			AssertEquals(0m, invoiceLine2.US_Duty);
			AssertEquals(0m, invoiceLine2.US_SupDuty);
			AssertEquals(0m, invoiceLine3.US_Duty);
			AssertEquals(0m, invoiceLine3.US_SupDuty);
		}

		[TestDate(2018, 12, 19)]
		public void TestCalculateDutyForSTNTariffInSets()
		{
			var testHelper = new Chapter98HelperTest();
			var tariff99038801 = testHelper.Test99038801Tariff;
			testHelper.SetTestTariffCodesForA99();

			var job = Factory.NewWithValidTestData<JobDeclaration>();
			job.JE_MessageType = JobMessageTypeList.Codes.Import;
			job.US_EntryFilerCode = "XJ5";
			job.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			job.US_EnableENS = true;
			var invoice = job.Invoices.AddNew();
			var parentLine = invoice.JobComInvoiceLines.AddNew();
			parentLine.US_UC_NKCountryOfExport = "CA";
			parentLine.US_UC_NKCountryOfOrigin = "CN";
			parentLine.JI_LinePrice = 5000m;
			parentLine.JI_Tariff = "9106905510";
			Factory.Save();

			AssertEquals(1, parentLine.SecondaryTariffLines.Count());
			var childLine = parentLine.SecondaryTariffLines.FirstOrDefault();
			AssertEquals(ZString.Empty, childLine.US_SupTariff);
			childLine.JI_LinePrice = 1000m;
			Factory.Save();
			job.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			AssertEquals("40% * (5000 + 1000)", 2400m, parentLine.US_SupDuty);
			AssertEquals("3.9% * 5000", 195m, parentLine.US_Duty);
			AssertEquals(0m, childLine.US_SupDuty);
			AssertEquals("5.3% * 1000", 53m, childLine.US_Duty);
		}

		[TestDate(2025, 04, 24)]
		public void TestCalculateDutyForSTNWithAdditionalTariffsInSets()
		{
			#region Setup Tariffs

			USCTariffTest.CreateNewTariffIfNotExist(Factory, "9102912010", "7", 0.039m, "NO");
			USCTariffTest.CreateNewTariffIfNotExist(Factory, "9102912020", "7", 0.053m, "NO");
			USCTariffTest.CreateNewTariffIfNotExist(Factory, "99030124", "7", 0.2m, ZString.Empty);
			USCTariffTest.CreateNewTariffIfNotExist(Factory, "99030128", "0", 0, ZString.Empty);
			USCTariffTest.CreateNewTariffIfNotExist(Factory, "99038815", "7", 0.075m, ZString.Empty);

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var hsnTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.TariffTypes.HarmonizedSystem);
			Factory.Save();
			var tariffView99030124 = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, hsnTariffType.PK, "99030124", new ZDateTime(2025, 03, 04), new ZDateTime(2079, 06, 06), "ALL IMPORTS OF ARTICLES THAT ARE PRODUCTS OF CHINA AND HONG KONG", 1);
			helper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.RULE, TariffRuleList.Codes.AdditionalTariffs, tariffView99030124);
			var tariffView99030128 = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, hsnTariffType.PK, "99030128", new ZDateTime(2025, 04, 24), new ZDateTime(2079, 06, 06), "ARTICLES THE PRODUCT OF ANY COUNTRY THAT (1) WERE LOADED ONTO A VESSEL AT THE PORT OF LOADING AND IN TRANSIT ON THE FINAL MODE OF TRANSIT PRIOR TO ENTRY INTO THE UNITED STATES", 1);
			helper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.RULE, TariffRuleList.Codes.AdditionalTariffs, tariffView99030128);
			var tariffView99038815 = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, hsnTariffType.PK, "99038815", new ZDateTime(2025, 01, 01), new ZDateTime(2079, 06, 06), "ARTICLES THE PRODUCT OF CHINA", 1);
			helper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.RULE, TariffRuleList.Codes.AdditionalTariffs, tariffView99038815);

			#endregion

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			var parentLine = invoice.JobComInvoiceLines.AddNew();
			parentLine.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.China;
			parentLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.China;
			parentLine.JI_FormattedTariff = "9102.91.2010";
			parentLine.SupTariffFormatted = "9903.88.15";
			parentLine.SupFormattedAdditionalTariff1 = "9903.01.24";
			parentLine.SupFormattedAdditionalTariff2 = "9903.01.28";
			parentLine.JI_LinePrice = 10000m;
			parentLine.JI_CustomsQuantity = 100m;

			var childLine = parentLine.ChildLines.ElementAt(0);
			childLine.JI_FormattedTariff = "9102.91.2020";
			childLine.JI_LinePrice = 5000m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			CombineAssertions("Duty for each tariff", () =>
			{
				AssertEquals("Duty for tariff 99030124 = ($10000+$5000) * 0.2", 3000m, parentLine.US_SupAdditionalTariff1Duty);
				AssertEquals("Duty for tariff 99030128 = ($10000+$5000) * 0", 0m, parentLine.US_SupAdditionalTariff2Duty);
				AssertEquals("Duty for tariff 99038815 = ($10000+$5000) * 0.075", 1125m, parentLine.US_SupDuty);
				AssertEquals("Duty for tariff 9102912010 = $10000 * 0.039", 390m, parentLine.US_Duty);
				AssertEquals("Duty for tariff 9102912020 = $5000 * 0.053", 265m, childLine.US_Duty);
			});
		}

		public void TestDerivedDutyCalculatorWith9903()
		{
			var testHelper = new Chapter98HelperTest();

			var job = Factory.NewWithValidTestData<JobDeclaration>();
			job.JE_MessageType = JobMessageTypeList.Codes.Import;
			job.US_EntryFilerCode = "XJ5";
			job.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			job.US_EnableENS = true;
			var invoice = job.Invoices.AddNew();
			var parentLine = invoice.JobComInvoiceLines.AddNew();
			parentLine.JI_Tariff = "8215.20.0000";
			parentLine.US_SupTariff = testHelper.Test99038801Tariff.UE_Tariff;

			var childLine1 = invoice.JobComInvoiceLines.AddNew();
			childLine1.JI_ParentID = parentLine.PK;
			childLine1.JI_Tariff = "8215.99.3500"; //6.8%
			childLine1.JI_LinePrice = 2000;
			childLine1.US_SupTariff = ZString.Empty;
			Factory.Save();
			job.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			AssertEquals("40% * 2000", 800m, parentLine.US_SupDuty);
			AssertEquals("6.8% * 2000", 136m, parentLine.US_Duty);
			AssertEquals(0m, childLine1.US_Duty);

			var ensEntry = job.ActiveEntryHeaders.EntrySummaryEntry;
			AssertEquals(3, ensEntry.AllEntryLines.Count);

			var childLine2 = invoice.JobComInvoiceLines.AddNew();
			childLine2.JI_ParentID = parentLine.PK;
			childLine2.JI_Tariff = "8215.91.6000"; //4.2%
			childLine2.JI_LinePrice = 8000;
			childLine2.US_SupTariff = ZString.Empty;
			Factory.Save();
			job.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			AssertEquals("40% * (8000 + 2000)", 4000m, parentLine.US_SupDuty);
			AssertEquals("6.8% * (8000 + 2000)", 680m, parentLine.US_Duty);
			AssertEquals(0m, childLine1.US_Duty);
			AssertEquals(0m, childLine2.US_Duty);

			ensEntry = job.ActiveEntryHeaders.EntrySummaryEntry;
			AssertEquals(3, ensEntry.AllEntryLines.Count);
		}

		public void TestShouldCalculateDutyFor9903CombinedLines()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);

			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, "HSN");
			Factory.Save();

			var zzTariff = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, tariffType.PK, "99030121", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateTariffAttribute("RULE", "A99", zzTariff);

			var testHelper = new Chapter98HelperTest();

			var job = Factory.NewWithValidTestData<JobDeclaration>();
			job.JE_MessageType = JobMessageTypeList.Codes.Import;
			job.US_EntryFilerCode = "XJ5";
			job.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			job.US_EnableENS = true;
			var invoice = job.Invoices.AddNew();
			var parentLine = invoice.JobComInvoiceLines.AddNew();
			parentLine.US_SupTariff = testHelper.Test99038801Tariff.UE_Tariff;

			var childLine1 = invoice.JobComInvoiceLines.AddNew();
			childLine1.JI_ParentID = parentLine.PK;
			childLine1.JI_Tariff = testHelper.TestCTariff.UE_Tariff;
			childLine1.JI_LinePrice = 2000;
			childLine1.US_SupTariff = testHelper.Test99030121Tariff.UE_Tariff;
			Factory.Save();
			job.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			AssertEquals("40% * 2000", 800m, parentLine.US_SupDuty);
			AssertEquals(0m, parentLine.US_Duty);
			AssertEquals("70% * 2000", 1400m, 0m, childLine1.US_Duty);
			AssertEquals(0m, childLine1.US_SupDuty);
		}

		public void TestShouldCalculateDutyFor9903WhenDutyComputationCodeIsZero()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);

			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, "HSN");
			Factory.Save();

			var zzTariff = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, tariffType.PK, "99030121", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateTariffAttribute("RULE", "A99", zzTariff);

			var testHelper = new Chapter98HelperTest();

			var job = Factory.NewWithValidTestData<JobDeclaration>();
			job.JE_MessageType = JobMessageTypeList.Codes.Import;
			job.US_EntryFilerCode = "XJ5";
			job.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			job.US_EnableENS = true;
			var invoice = job.Invoices.AddNew();

			var line = invoice.JobComInvoiceLines.AddNew();
			line.JI_Tariff = testHelper.Test8483509040Tariff.UE_Tariff;
			line.JI_LinePrice = 2000;
			line.US_SupTariff = testHelper.Test99030121Tariff.UE_Tariff;
			Factory.Save();
			job.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			AssertEquals("15% * 2000", 300m, 0m, line.US_Duty);
			AssertEquals(0m, line.US_SupDuty);
		}

		public void TestShouldNotCalculateDutyForTIBChapter98()
		{
			var testHelper = new Chapter98HelperTest();
			testHelper.ParentLine.US_SupTariff = testHelper.Test99038001Tariff.UE_Tariff;
			testHelper.ChildLine.US_SupTariff = testHelper.Test9813Tariff.UE_Tariff;
			testHelper.ParentLine.JI_LinePrice = 1000m;
			testHelper.Charpter98Job.US_EntryType = EntryTypeList.Codes.TemporaryImportationBond;
			testHelper.Charpter98Job.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals(0m, testHelper.ParentLine.US_Duty);
			AssertEquals(0m, testHelper.ParentLine.US_SupDuty);
			AssertEquals(0m, testHelper.ChildLine.US_Duty);
			AssertEquals(0m, testHelper.ChildLine.US_SupDuty);
		}

		public void TestCalculateDutyForXVVCombined()
		{
			var testHelper = new Chapter98HelperTest();

			testHelper.ParentLine.US_SupTariff = testHelper.Test99038801Tariff.UE_Tariff;
			testHelper.ParentLine.US_SetInd = "X";

			testHelper.ChildLine.US_SupTariff = testHelper.Test99038801Tariff.UE_Tariff;
			testHelper.ChildLine.JI_Tariff = testHelper.TestCTariff.UE_Tariff;
			testHelper.ChildLine.US_SetInd = "V";
			testHelper.ChildLine.JI_LinePrice = 100m;

			var incoineHeader = testHelper.ChildLine.InvoiceHeader;
			var invoiceLine3 = incoineHeader.InvoiceLines.AddNew();
			invoiceLine3.JI_ParentID = testHelper.ParentLine.PK;
			invoiceLine3.US_SetInd = "V";
			invoiceLine3.JI_Tariff = "8204200000";
			invoiceLine3.JI_LinePrice = 200m;

			var invoiceLine4 = incoineHeader.InvoiceLines.AddNew();
			invoiceLine4.JI_ParentID = testHelper.ParentLine.PK;
			invoiceLine4.US_SetInd = "V";
			invoiceLine4.JI_Tariff = "8204200000";
			invoiceLine4.JI_LinePrice = 300m;
			testHelper.Charpter98Job.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			Factory.Save();

			var dutyData = new EntryLineIEntryLineOrInvoiceLineDutyData(testHelper.ParentLine.CusEntryLine);
			Assert(CalculateDutyForSetsHelper.IsCombinedXLine(dutyData));

			AssertEquals(600m, CalculateDutyForSetsHelper.GetSetsCustomsValueForXVLine(dutyData));
			AssertEquals("600 * 40%", 240m, testHelper.ParentLine.US_SupDuty);
			AssertEquals("600 * 70%", 420m, testHelper.ParentLine.US_Duty);

			AssertEquals(0m, testHelper.ChildLine.US_SupDuty);
			AssertEquals(0m, testHelper.ChildLine.US_Duty);

			AssertEquals(0m, invoiceLine3.US_SupDuty);
			AssertEquals(0m, invoiceLine3.US_Duty);

			AssertEquals(0m, invoiceLine4.US_SupDuty);
			AssertEquals(0m, invoiceLine4.US_Duty);
		}

		public void TestCalculateADDDutyForChapter98()
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

			var testHelper = new Chapter98HelperTest();
			testHelper.Charpter98Job.US_EntryType = EntryTypeList.Codes.ConsumptionADDCVD;
			testHelper.Charpter98Job.US_EnableENS = true;
			testHelper.ParentLine.US_UC_NKCountryOfExport = "KR";
			testHelper.ParentLine.US_UC_NKCountryOfOrigin = "KR";
			testHelper.ParentLine.JI_Tariff = "7222.11.00 50";
			testHelper.ParentLine.US_SupTariff = testHelper.Test99038501Tariff.UE_Tariff;//99038501
			testHelper.ChildLine.US_SupTariff = testHelper.Test9817002000Tariff.UE_Tariff;

			testHelper.ParentLine.US_ADDCaseNo = "A9085290";
			testHelper.ParentLine.US_ADDDepositRateIndicator = "A";
			testHelper.ParentLine.JI_CustomsQuantity = 70m;
			testHelper.ParentLine.JI_LinePrice = 8000m;
			testHelper.ChildLine.US_98GoodsValue = 0m;
			testHelper.Charpter98Job.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			var addDuty = testHelper.ParentLine.CusEntryLine.Fees.GetAmount(Core.Constants.USCustoms.FeeCodes.AntidumpingDuty);
			AssertEquals("AntiDumping Duty", 4160m, addDuty);

			testHelper.ChildLine.US_98GoodsValue = 8000m;
			testHelper.Charpter98Job.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();
			addDuty = testHelper.ParentLine.CusEntryLine.Fees.GetAmount(Core.Constants.USCustoms.FeeCodes.AntidumpingDuty);
			AssertEquals("AntiDumping Duty", 8320m, addDuty);

			testHelper.ParentLine.US_ADDDepositValue = 1000m;
			testHelper.Charpter98Job.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();
			addDuty = testHelper.ParentLine.CusEntryLine.Fees.GetAmount(Core.Constants.USCustoms.FeeCodes.AntidumpingDuty);
			AssertEquals("AntiDumping Duty", 520m, addDuty);
		}

		public void TestShouldNotCalculateDutyForChapter98()
		{
			var testHelper = new Chapter98HelperTest();
			testHelper.ParentLine.US_SupTariff = testHelper.Test99038501Tariff.UE_Tariff;//99038501
			testHelper.ChildLine.US_SupTariff = testHelper.Test9802005060Tariff.UE_Tariff;//9802005060
			testHelper.ParentLine.JI_LinePrice = 1000m;
			testHelper.ChildLine.US_98GoodsValue = 1000m;
			testHelper.Charpter98Job.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals(700m, testHelper.ParentLine.US_Duty);
			AssertEquals(100m, testHelper.ParentLine.US_SupDuty);
			AssertEquals(0m, testHelper.ChildLine.US_Duty);
			AssertEquals(0m, testHelper.ChildLine.US_SupDuty);

			testHelper.ParentLine.US_SupTariff = testHelper.Test99038801Tariff.UE_Tariff;//99038801
			testHelper.ChildLine.US_SupTariff = testHelper.Test9802005060Tariff.UE_Tariff;//98
			testHelper.ParentLine.JI_LinePrice = 1000m;
			testHelper.ChildLine.US_98GoodsValue = 0m;
			testHelper.Charpter98Job.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals(700m, testHelper.ParentLine.US_Duty);
			AssertEquals(400m, testHelper.ParentLine.US_SupDuty);
			AssertEquals(0m, testHelper.ChildLine.US_Duty);
			AssertEquals(0m, testHelper.ChildLine.US_SupDuty);

			testHelper.ChildLine.US_98GoodsValue = 1000m;
			testHelper.Charpter98Job.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals(700m, testHelper.ParentLine.US_Duty);
			AssertEquals(400m, testHelper.ParentLine.US_SupDuty);
			AssertEquals(0m, testHelper.ChildLine.US_Duty);
			AssertEquals(0m, testHelper.ChildLine.US_SupDuty);
		}

		public void TestShouldCombineDutyForChapter98()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, "HSN");
			var zzTariff = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.UnitedStates, tariffType.PK, "99038501", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.TYPE, UniversalReferenceConstants.TariffAttributeTypes.Values._232, zzTariff);

			var testHelper = new Chapter98HelperTest();
			testHelper.ParentLine.US_SupTariff = testHelper.Test99038801Tariff.UE_Tariff;//99038801
			testHelper.ChildLine.US_SupTariff = testHelper.Test9802005060Tariff.UE_Tariff;//98
			testHelper.ParentLine.JI_LinePrice = 1000m;
			testHelper.ChildLine.US_98GoodsValue = 0m;
			testHelper.Charpter98Job.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals(700m, testHelper.ParentLine.US_Duty);
			AssertEquals(400m, testHelper.ParentLine.US_SupDuty);

			testHelper.ChildLine.US_98GoodsValue = 1000m;
			testHelper.Charpter98Job.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals(700m, testHelper.ParentLine.US_Duty);
			AssertEquals(400m, testHelper.ParentLine.US_SupDuty);

			testHelper.ParentLine.US_SupTariff = testHelper.Test99038501Tariff.UE_Tariff;//99038501
			testHelper.ChildLine.US_SupTariff = testHelper.Test9802006000Tariff.UE_Tariff;//9802006000
			testHelper.ParentLine.JI_LinePrice = 1000m;
			testHelper.ChildLine.US_98GoodsValue = 0m;
			testHelper.Charpter98Job.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals(700m, testHelper.ParentLine.US_Duty);
			AssertEquals(100m, testHelper.ParentLine.US_SupDuty);

			testHelper.ChildLine.US_98GoodsValue = 1000m;
			testHelper.Charpter98Job.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals(700m, testHelper.ParentLine.US_Duty);
			AssertEquals("(parent value + child value) * rate", 200m, testHelper.ParentLine.US_SupDuty);
		}

		[TestDate(2009, 1, 1)]
		public void TestCalculateWithEnteredValueDeclaredAtParent()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000.00m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_SupTariff = "99990050";
			invoiceLine.JI_LinePrice = 10000m;
			invoiceLine.JI_Tariff = "6106100010";
			invoiceLine.JI_CustomsQuantity = 1000m;
			invoiceLine.US_SPI = SPICompleteList.MoreCodes.NotApplicable;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			CusEntryHeader entry = declaration.CustomsEntryHeaders[0];

			AssertEquals("Total Customs duty", 1970m, invoiceLine.CusEntryLine.DutyAmount);
			AssertEquals("Total Customs duty", 1970m, entry.TotalDutyAmount);

			invoiceLine.US_SPI = SpecialProgramList.Codes.CA;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Total Customs duty", 0m, invoiceLine.CusEntryLine.DutyAmount);
			AssertEquals("Total Customs duty", 0m, entry.TotalDutyAmount);
		}

		public void Test9810008500WhichRequiresSecondaryTariffDutyRate()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000.00m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "3912120000";// 5.6%
			invoiceLine.JI_LinePrice = 10000.00m;
			invoiceLine.JI_CustomsQuantity = 10000.00000m;
			invoiceLine.US_SupTariff = "9810008500";// free

			SetTariffRuleIfNotApplicable(invoiceLine.ImportSupTariff, TariffRuleList.Codes.AdditionalTariffs, "9810008500", ZDateTime.Today);
			Assert(invoiceLine.ImportSupTariff.Applies(TariffRuleList.Codes.AdditionalTariffs, ZDateTime.Today));

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			CusEntryHeader entry = declaration.CustomsEntryHeaders[0];
			AssertEquals("Total Customs duty", 560m, entry.TotalDutyAmount);
		}

		void SetTariffRuleIfNotApplicable(USCTariff importTariff, ZString tariffRule, ZString tariffFrom, ZDateTime date)
		{
			USCTariffRule rule = importTariff.GetTariffRuleIfApplies(tariffRule, date);
			if (rule == null)
			{
				rule = importTariff.Factory.New<USCTariffRule>();
				rule.U1_RuleCode = tariffRule;
				rule.U1_Tariff = tariffFrom;
				rule.U1_DateFrom = date;

				importTariff.Factory.Save();

				importTariff.TariffRules.Load(importTariff.UE_Tariff);
			}
		}

		[TestDate(2009, 6, 1)]
		public void TestCalculateForDomesticMerchandise()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 1798.00m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "8708.80.6590";
			invoiceLine.JI_LinePrice = 1798.00m;
			invoiceLine.JI_CustomsQuantity = 10000.00000m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			CusEntryHeader entry = declaration.CustomsEntryHeaders[0];
			AssertEquals("Total Customs duty", 44.95m, entry.TotalDutyAmount);
			AssertEquals("MPF", 25m, entry.MPFAmountForEntry);

			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			invoiceLine.US_ZoneStatus = ZoneStatusList.Codes.Domestic;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			entry = declaration.CustomsEntryHeaders[0];

			AssertEquals("Total Customs duty", 0m, entry.TotalDutyAmount);
			AssertEquals("MPF", 0m, entry.MPFAmountForEntry);
		}

		public void TestCumulativeDutyCalculation()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000.00m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_SupTariff = "9904.17.11";
			invoiceLine.JI_LinePrice = 10000.00m;
			invoiceLine.JI_Tariff = "1701.12.5000";
			invoiceLine.JI_CustomsQuantity = 58000.00000m;
			invoiceLine.JI_CustomsSecondQuantity = 20m;
			invoiceLine.US_SupQty1 = 58000m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			CusEntryHeader entry = declaration.CustomsEntryHeaders[0];
			AssertEquals("Total Customs duty", 26297.20m, entry.TotalDutyAmount);

			CusEntryLine supLine = invoiceLine.GetEntryLineFor(CusEntryHeaderMessageTypeList.Codes.EntrySummary, true);
			AssertEquals(5568m, supLine.DutyAmount);
			AssertEquals(20729.20m, invoiceLine.CusEntryLine.DutyAmount);
		}

		[TestDate(2008, 3, 25)]
		public void TestStoreDutyRate()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 3000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.US_UC_NKCountryOfExport = "SG";
			invoice.US_UC_NKCountryOfOrigin = "SG";

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "3201.90.1000";
			invoiceLine.JI_CustomsQuantity = 50m;
			invoiceLine.JI_LinePrice = 3000m;
			invoiceLine.US_SPI = "";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertEquals(45m, invoiceLine.CusEntryLine.DutyAmount);
			AssertEquals(1.5m, invoiceLine.CusEntryLine.CL_DutyPercent);
		}

		[TestDate(2008, 1, 9)]
		public void TestCottonFeeThresholdAndEntryTotal()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.US_EnableENS = true;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 63650m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "6103.43.1570";
			invoiceLine.JI_CustomsQuantity = 2361.00000m;
			invoiceLine.JI_LinePrice = 58574.00m;
			invoiceLine.JI_CustomsSecondQuantity = 6197.0000m;

			JobComInvoiceLine invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "6111.30.5020";
			invoiceLine2.JI_CustomsQuantity = 141m;
			invoiceLine2.JI_LinePrice = 5076.00m;
			invoiceLine2.JI_CustomsSecondQuantity = 321.0000m;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			ZDecimal cottonFeeEntryLine1 = invoiceLine.CusEntryLine.Fees.GetAmount(Core.Constants.USCustoms.FeeCodes.Cotton);
			AssertNotEquals("EntryLine level CottonFee", 0m, cottonFeeEntryLine1);
			AssertEquals("EntryLine level CottonFee exempt as it is less than threhold", 0m, invoiceLine2.CusEntryLine.Fees.GetAmount(Core.Constants.USCustoms.FeeCodes.Cotton));

			AssertEquals("Entry level cottonFee should not have the exempted amount included", cottonFeeEntryLine1, declaration.CustomsEntryHeaders[0].CottonFee);
		}

		[TestDate(2006, 9, 6)]
		public void TestADD_CVDCalculationDoneWhileMerging()
		{
			USCACCase addCase = Factory.New<USCACCase>();
			addCase.U5_CaseNumber = "A580844004";
			addCase.U5_ISOCountryCode = "KR";
			addCase.U5_CaseStatus = ACCaseStatusList.Codes.IO;
			addCase.U5_CaseStatusDate = ZDateTime.Today;
			addCase.CaseTariffs.AddNew().U9_TariffNumber = "72142000";
			addCase.CaseTariffs.AddNew().U9_TariffNumber = "7222110050";
			var addRate = addCase.CaseRates.AddNew();
			addRate.U6_AdValoremRate = 1.02m;
			addRate.U6_EffectiveDate = ZDateTime.Today;

			USCACCase cvdCase = Factory.New<USCACCase>();
			cvdCase.U5_CaseNumber = "C122839025";
			cvdCase.U5_ISOCountryCode = "CA";
			cvdCase.U5_CaseStatus = ACCaseStatusList.Codes.IO;
			cvdCase.U5_CaseStatusDate = ZDateTime.Today;
			cvdCase.CaseTariffs.AddNew().U9_TariffNumber = "44091020";
			cvdCase.CaseTariffs.AddNew().U9_TariffNumber = "4421907040";
			var cvdRate = cvdCase.CaseRates.AddNew();
			cvdRate.U6_AdValoremRate = 0.09m;
			cvdRate.U6_EffectiveDate = ZDateTime.Today;

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionADDCVD;
			declaration.US_EnableENS = true;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.US_UC_NKCountryOfExport = "KR";
			invoice.US_UC_NKCountryOfOrigin = "KR";

			JobComInvoiceLine invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "7222.11.00 50";
			invoiceLine1.JI_CustomsQuantity = 70m;
			invoiceLine1.US_ADDCaseNo = "A580844004";
			invoiceLine1.JI_LinePrice = 4000m;

			JobComInvoiceLine invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "7222.11.00 50";
			invoiceLine2.JI_CustomsQuantity = 70m;
			invoiceLine2.US_CVDCaseNo = "C122839025";
			invoiceLine2.JI_LinePrice = 6000m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertEquals("AntiDumping Duty persisted into EntryLine.Fees", 4080.00m, invoiceLine1.CusEntryLine.Fees.GetAmount(Core.Constants.USCustoms.FeeCodes.AntidumpingDuty));
			AssertEquals("Countervailing Duty persisted into EntryLine.Fees", 540.00m, invoiceLine2.CusEntryLine.Fees.GetAmount(Core.Constants.USCustoms.FeeCodes.CountervailingDuty));

			declaration.US_EntryType = EntryTypeList.Codes.TemporaryImportationBond;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertEquals("For TIB, ADD should not be calculated now", 4080.00m, invoiceLine1.CusEntryLine.Fees.GetAmount(Core.Constants.USCustoms.FeeCodes.AntidumpingDuty));
			AssertEquals("For TIB, CVD should not be calculated now", 540.00m, invoiceLine2.CusEntryLine.Fees.GetAmount(Core.Constants.USCustoms.FeeCodes.CountervailingDuty));
		}

		public void TestADD_CVDCalculationWhenManual()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionADDCVD;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.US_UC_NKCountryOfExport = "KR";
			invoice.US_UC_NKCountryOfOrigin = "KR";

			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "7222.11.00 50";
			invoiceLine1.JI_CustomsQuantity = 70m;
			invoiceLine1.US_ADDCaseNo = "A580844004";
			invoiceLine1.JI_LinePrice = 4000m;
			invoiceLine1.US_ADDDepositRateIndicator = DepositRateIndicatorList.Codes.Specific;
			invoiceLine1.US_ADDuty = 100m;

			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "7222.11.00 50";
			invoiceLine2.JI_CustomsQuantity = 70m;
			invoiceLine2.US_CVDCaseNo = "C122839025";
			invoiceLine2.JI_LinePrice = 6000m;
			invoiceLine2.US_CVDDepositRateIndicator = DepositRateIndicatorList.Codes.Specific;
			invoiceLine2.US_CVDuty = 200m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertEquals("AD Duty against entry Line", 100m, invoiceLine1.CusEntryLine.Fees.GetAmount(Core.Constants.USCustoms.FeeCodes.AntidumpingDuty));
			AssertEquals("CV Duty against entry Line", 0m, invoiceLine1.CusEntryLine.Fees.GetAmount(Core.Constants.USCustoms.FeeCodes.CountervailingDuty));

			AssertEquals("AD Duty against entry Line", 0m, invoiceLine2.CusEntryLine.Fees.GetAmount(Core.Constants.USCustoms.FeeCodes.AntidumpingDuty));
			AssertEquals("CV Duty against entry Line", 200m, invoiceLine2.CusEntryLine.Fees.GetAmount(Core.Constants.USCustoms.FeeCodes.CountervailingDuty));
		}

		public void Test9802005010Calculation()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000.00m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_SupTariff = "9802.00.50 10";
			invoiceLine.JI_LinePrice = 0m;
			invoiceLine.US_98GoodsValue = 15000m;
			invoiceLine.US_UC_NKCountryOfExport = "PA";
			invoiceLine.US_UC_NKCountryOfOrigin = "PA";
			AssertEquals("Duty computation Code is not X", false, invoiceLine.ImportSupTariff.UE_DutyComputationCode == ComputationCodeList.Codes.NoComputationFormulaAvailable);

			invoiceLine.JI_Tariff = "8407.34.18 00";
			invoiceLine.JI_LinePrice = 25000.00m;
			invoiceLine.JI_CustomsQuantity = 10m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals(0m, invoiceLine.CusEntryLine.Header.TotalDutyAmount);
		}

		public void Test9802008042Calculation()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000.00m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_SupTariff = "9802008042";
			invoiceLine.US_98GoodsValue = 15000m;
			invoiceLine.US_UC_NKCountryOfExport = "ZA";
			invoiceLine.US_UC_NKCountryOfOrigin = "ZA";
			AssertEquals("Duty computation Code is not X", false, invoiceLine.ImportSupTariff.UE_DutyComputationCode == ComputationCodeList.Codes.NoComputationFormulaAvailable);

			invoiceLine.JI_Tariff = "6101.20.00 10";
			invoiceLine.JI_CustomsQuantity = 10000m;
			invoiceLine.JI_CustomsSecondQuantity = 10000m;
			invoiceLine.JI_LinePrice = 25000m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals(0m, invoiceLine.CusEntryLine.Header.TotalDutyAmount);
			AssertNull("Cotton fee is exempt", invoiceLine.FeeCusCodes.GetFirstElementHaving(Core.Constants.USCustoms.FeeCodes.Cotton));
		}

		[TestDate(2009, 12, 12)]
		public void TestAdditionalDutyTariffCalculation()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000.00m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 10000.00m;
			invoiceLine.US_SupTariff = "9904.02.37";//8.8% 
			invoiceLine.JI_Tariff = "0201.30.80 10";//26.4% 
			invoiceLine.JI_CustomsQuantity = 1500m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals(3520m, invoiceLine.CusEntryLine.Header.TotalDutyAmount);
		}

		public void TestAssemblyOfUSProducts_98220510()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000.00m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.Honduras;
			invoice.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Honduras;

			JobComInvoiceLine parentLine = invoice.JobComInvoiceLines.AddNew();
			parentLine.US_SupTariff = "9822.05.10";

			parentLine.US_98GoodsValue = 4000m;
			parentLine.JI_Tariff = "5101.19.60 30";//6.5% for general
			parentLine.JI_CustomsQuantity = 1500m;
			parentLine.JI_CustomsSecondQuantity = 100m;
			parentLine.JI_LinePrice = 6000m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertEquals(168.30m, declaration.CustomsEntryHeaders[0].TotalDutyAmount);
		}

		[TestDate(2009, 12, 12)]
		public void TestInLieuTariffDutyCalculation_9902()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000.00m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceline = invoice.JobComInvoiceLines.AddNew();
			invoiceline.US_SupTariff = "9902.01.21";//6%
			invoiceline.JI_Tariff = "2933.19.23 00";//6.5% for general
			invoiceline.JI_CustomsQuantity = 1500m;
			invoiceline.JI_LinePrice = 10000m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertEquals(600m, declaration.CustomsEntryHeaders[0].TotalDutyAmount);
			AssertEquals(25m, declaration.CustomsEntryHeaders[0].Charges.GetAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));
			AssertEquals(21m, invoiceline.GetEntryLineFor("ENS", true).Fees.GetAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));
		}

		[TestDate(2008, 9, 11)]
		public void TestCalculateCottonFeeForXAndVLines()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 5000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine xLine = invoice.JobComInvoiceLines.AddNew();
			xLine.JI_LinePrice = 5000m;
			xLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.X;
			xLine.JI_Tariff = "6206900040";
			xLine.JI_CustomsQuantity = 250m;
			xLine.JI_CustomsSecondQuantity = 10000m;

			JobComInvoiceLine vLine1 = invoice.JobComInvoiceLines.AddNew();
			vLine1.JI_LinePrice = 4500m;
			vLine1.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;
			vLine1.JI_Tariff = "6206900040";
			vLine1.JI_CustomsQuantity = 250m;
			vLine1.JI_CustomsSecondQuantity = 1000m;

			JobComInvoiceLine vLine2 = invoice.JobComInvoiceLines.AddNew();
			vLine2.JI_LinePrice = 500m;
			vLine2.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;
			vLine2.JI_Tariff = "6206302000";
			vLine2.JI_CustomsQuantity = 250m;
			vLine2.JI_CustomsSecondQuantity = 100m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertEquals("1 entry generated", 1, declaration.CustomsEntryHeaders.Count);
			AssertEquals("3 lines generated", 3, declaration.CustomsEntryHeaders[0].MergedLines.Count);

			AssertEquals("Cotton Fee for X line", 0m, xLine.CusEntryLine.Fees.GetAmount(Core.Constants.USCustoms.FeeCodes.Cotton));
			AssertEquals("MPF fee for X line", 10.50m, xLine.CusEntryLine.Fees.GetAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));
			AssertEquals("Cotton Fee for V line", 2.06m, vLine1.CusEntryLine.Fees.GetAmount(Core.Constants.USCustoms.FeeCodes.Cotton));
			AssertEquals("MPF fee for V line", 0m, vLine1.CusEntryLine.Fees.GetAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));
			AssertEquals("MPF fee for V line", 0m, vLine2.CusEntryLine.Fees.GetAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));
		}

		[TestDate(2009, 6, 1)]
		public void TestMinimumMerchandiseProcessingFee()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = "USD";
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 1000m;

			LineMerger merger = new LineMerger(declaration);
			merger.DoMerge();
			CusEntryLineFee fee = invoiceLine.CusEntryLine.Fees.GetOrAddFeeByFeeType(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing);
			AssertEquals(2.1m, fee.CF_ChargeAmount);
			CusEntryHeaderCharges charge = declaration.CustomsEntryHeaders[0].Charges[Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing];
			AssertEquals(25m, charge.C1_ChargeAmount);
		}

		[TestDate(2009, 6, 1)]
		public void TestMaximumMerchandiseProcessingFee()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = "USD";
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 1000000m;

			LineMerger merger = new LineMerger(declaration);
			merger.DoMerge();
			CusEntryLineFee fee = invoiceLine.CusEntryLine.Fees.GetOrAddFeeByFeeType(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing);
			AssertEquals(2100m, fee.CF_ChargeAmount);
			CusEntryHeaderCharges charge = declaration.CustomsEntryHeaders[0].Charges[Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing];
			AssertEquals(485m, charge.C1_ChargeAmount);
		}

		[TestDate(2009, 6, 1)]
		public void TestInRangeMerchandiseProcessingFee()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = "USD";
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 100000m;

			LineMerger merger = new LineMerger(declaration);
			merger.DoMerge();
			CusEntryLineFee fee = invoiceLine.CusEntryLine.Fees.GetOrAddFeeByFeeType(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing);
			AssertEquals(210m, fee.CF_ChargeAmount);
			CusEntryHeaderCharges charge = declaration.CustomsEntryHeaders[0].Charges[Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing];
			AssertEquals(210m, charge.C1_ChargeAmount);
		}

		[TestDate(2006, 12, 31)]
		public void TestMangoFee()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = "USD";
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "0804504040";
			invoiceLine.JI_CustomsQuantity = 1000m;
			invoiceLine.JI_LinePrice = 100000m;
			LineMerger merger = new LineMerger(declaration);
			merger.DoMerge();
			CusEntryLineFee fee = invoiceLine.CusEntryLine.Fees.GetOrAddFeeByFeeType(Core.Constants.USCustoms.FeeCodes.Mango);
			AssertEquals(11.02m, fee.CF_ChargeAmount);
			CusEntryHeaderCharges headerFee = invoiceLine.CusEntryLine.Header.Charges[Core.Constants.USCustoms.FeeCodes.Mango];
			AssertEquals(11.02m, headerFee.C1_ChargeAmount);
		}

		public void TestRepairTariffWithMX_SPI()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			invoice.US_UC_NKCountryOfExport = "MX";
			invoice.US_UC_NKCountryOfOrigin = "MX";

			JobComInvoiceLine invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.US_SupTariff = "9802004020";
			invoiceLine1.US_98GoodsValue = 8000m;
			invoiceLine1.US_SPI = "MX";
			invoiceLine1.JI_Tariff = "8407341800";
			invoiceLine1.JI_LinePrice = 2000m;
			invoiceLine1.JI_CustomsQuantity = 20m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertEquals("Entries generated", 1, declaration.CustomsEntryHeaders.Count);
			AssertEquals("Duty calculated", 0m, declaration.CustomsEntryHeaders[0].TotalDutyAmount);
		}

		[TestDate(2025, 05, 12)]
		public void TestCalculateSupAdditionalDutyForSingleWatchLine()
		{
			#region Setup Tariffs

			USCTariffTest.CreateNewTariffIfNotExist(Factory, "9102124000", "7", 0m, "NO");
			USCTariffTest.CreateNewTariffIfNotExist(Factory, "99030124", "7", 0.2m, "");
			USCTariffTest.CreateNewTariffIfNotExist(Factory, "99030125", "7", 0.1m, "");

			#endregion

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.China;
			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.China;
			invoiceLine.JI_FormattedTariff = "9102.12.4000";
			invoiceLine.SupTariffFormatted = "9903.01.25";
			invoiceLine.SupFormattedAdditionalTariff1 = "9903.01.24";
			invoiceLine.JI_LinePrice = 10000m;
			invoiceLine.ChildLines.DeleteAll();

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("US_Duty", 0m, invoiceLine.US_Duty);
			AssertEquals("US_SupDuty", 1000m, invoiceLine.US_SupDuty);
			AssertEquals("US_SupAdditionalTariff1Duty", 2000m, invoiceLine.US_SupAdditionalTariff1Duty);
		}

		[TestDate(2019, 12, 31)]
		public void TestCalculateDutyWhenOverriden()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.US_EnableENS = true;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.US_UC_NKCountryOfExport = "SV";
			invoice.US_UC_NKCountryOfOrigin = "SV";

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_SupTariff = "9915.04.90";
			invoiceLine.US_SPI = SpecialProgramList.Codes.PPlus;
			invoiceLine.US_SupQty1 = 70.00000m;
			invoiceLine.JI_Tariff = "0406.10.08 00";
			invoiceLine.JI_CustomsQuantity = 10000.00000m;
			invoiceLine.JI_LinePrice = 10000.00m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			CusEntryLine supLine = invoiceLine.GetEntryLineFor(CusEntryHeaderMessageTypeList.Codes.EntrySummary, true);
			AssertEquals("Calculated Duty Amount", 105.63m, invoiceLine.JI_Calc_DutyAmount);
			AssertEquals("Calculated Duty Amount", 105.63m, supLine.DutyAmount);

			invoiceLine.US_OverrideSupDuty = true;
			invoiceLine.US_SupDuty = 0m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			supLine = invoiceLine.GetEntryLineFor(CusEntryHeaderMessageTypeList.Codes.EntrySummary, true);
			AssertEquals("Overriden Duty Amount", 0m, invoiceLine.JI_Calc_DutyAmount);
			AssertEquals("Overriden Duty Amount", 0m, supLine.DutyAmount);
		}

		[TestDate(2009, 12, 12)]
		public void TestAdjustEntryLineCottonFee()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_SupTariff = "9821.11.19";
			invoiceLine.JI_Tariff = "6212.10.9020";
			invoiceLine.JI_CustomsQuantity = 10m;
			invoiceLine.JI_CustomsSecondQuantity = 100m;
			invoiceLine.JI_LinePrice = 10500m;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			CusEntryLine supLine = invoiceLine.GetEntryLineFor(CusEntryHeaderMessageTypeList.Codes.EntrySummary, true);

			AssertEquals("However no entry line should have this fee as it is less than threshold", 0m, invoiceLine.CusEntryLine.Fees.GetAmount(Core.Constants.USCustoms.FeeCodes.Cotton));
			AssertEquals("However no entry line should have this fee as it is less than threshold", 0m, supLine.Fees.GetAmount(Core.Constants.USCustoms.FeeCodes.Cotton));
			AssertEquals("And entry should not have this fee as it is less than threshold", 0m, declaration.CustomsEntryHeaders[0].Charges.GetAmount(Core.Constants.USCustoms.FeeCodes.Cotton));
		}

		[TestDate(2018, 10, 10)]
		public void TestCalculateDutyForI99InCombinedLines()
		{
			#region Setup

			var tariff2930904310 = new USCTariff.Loader(Factory).LoadBestMatch("2930904310", ZDateTime.Today);
			if (tariff2930904310 == null)
			{
				tariff2930904310 = Factory.New<USCTariff>();
				tariff2930904310.UE_Tariff = "2930904310";
				tariff2930904310.UE_DateFrom = ZDateTime.BrettsBirthday;
				tariff2930904310.UE_DateTo = ZDateTime.Today.AddYears(1);
				tariff2930904310.UE_DutyComputationCode = "7";
				tariff2930904310.UE_Column1RateAdValorem = 0.065m;
			}

			var tariff99020518 = new USCTariff.Loader(Factory).LoadBestMatch("99020518", ZDateTime.Today);
			if (tariff99020518 == null)
			{
				tariff99020518 = Factory.New<USCTariff>();
				tariff99020518.UE_Tariff = "99020518";
				tariff99020518.UE_DateFrom = ZDateTime.BrettsBirthday;
				tariff99020518.UE_DateTo = ZDateTime.Today.AddYears(1);
				tariff99020518.UE_DutyComputationCode = "7";
				tariff99020518.UE_Column1RateAdValorem = 0.054m;
			}

			var tariff99038803 = new USCTariff.Loader(Factory).LoadBestMatch("99038803", ZDateTime.Today);
			if (tariff99038803 == null)
			{
				tariff99038803 = Factory.New<USCTariff>();
				tariff99038803.UE_Tariff = "99038803";
				tariff99038803.UE_DateFrom = ZDateTime.BrettsBirthday;
				tariff99038803.UE_DateTo = ZDateTime.Today.AddYears(1);
				tariff99038803.UE_DutyComputationCode = "7";
				tariff99038803.UE_Column1RateAdValorem = 0.1m;
			}

			Factory.Save();

			#endregion

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 30000m;
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.US_UC_NKCountryOfExport = "HK";
			invoiceLine1.US_UC_NKCountryOfOrigin = "HK";
			invoiceLine1.JI_Tariff = "2930904310";
			invoiceLine1.US_SupTariff = "";
			invoiceLine1.JI_LinePrice = 10000m;
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.US_UC_NKCountryOfExport = "HK";
			invoiceLine2.US_UC_NKCountryOfOrigin = "HK";
			invoiceLine2.JI_Tariff = "2930904310";
			invoiceLine2.US_SupTariff = "99020518";
			invoiceLine2.JI_LinePrice = 10000m;
			var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine3.US_UC_NKCountryOfExport = "CN";
			invoiceLine3.US_UC_NKCountryOfOrigin = "CN";
			invoiceLine3.US_SupTariff = "99038803";
			var invoiceLine4 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine4.JI_ParentID = invoiceLine3.PK;
			invoiceLine4.JI_Tariff = "2930904310";
			invoiceLine4.US_SupTariff = "99020518";
			invoiceLine4.JI_LinePrice = 10000m;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			CombineAssertions(() =>
			{
				AssertEquals("Regular duty for invoice line 1 should be 650", 650m, invoiceLine1.US_Duty);
				AssertEquals("Prov duty for invoice line 1 should be zero", 0m, invoiceLine1.US_SupDuty);
				AssertEquals("Regular duty for invoice line 2 should be zero", 0m, invoiceLine2.US_Duty);
				AssertEquals("Prov duty for invoice line 2 should be 540", 540m, invoiceLine2.US_SupDuty);
				AssertEquals("Regular duty for invoice line 3 should be zero", 0m, invoiceLine3.US_Duty);
				AssertEquals("Prov duty for invoice line 3 should be 1000", 1000m, invoiceLine3.US_SupDuty);
				AssertEquals("Regular duty for invoice line 4 should be zero", 0m, invoiceLine4.US_Duty);
				AssertEquals("Prov duty for invoice line 4 should be 540", 540m, invoiceLine4.US_SupDuty);
			});
		}

		[TestDate(2020, 03, 03)]
		public void TestCalculateDutyForEmbroideryTariffOnRecon()
		{
			#region Setup Tariffs

			var helper = new UniversalReferenceTestDataHelper(Factory);

			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, "HSN");
			Factory.Save();

			var zzTariff = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, tariffType.PK, "5810929080", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateTariffAttribute("RULE", "EMB", zzTariff);

			var tariff5810929080 = Factory.Load<USCTariff>(new ZQuery(USCTariffSchema.UE_Tariff, "5810929080")).LastOrDefault();
			if (tariff5810929080 == null)
			{
				tariff5810929080 = Factory.New<USCTariff>();
				tariff5810929080.UE_Tariff = "5810929080";
				tariff5810929080.UE_DutyComputationCode = "7";
				tariff5810929080.UE_Column1RateAdValorem = 0.074m;
			}
			tariff5810929080.UE_DateFrom = new ZDateTime(2020, 01, 01);
			tariff5810929080.UE_DateTo = new ZDateTime(2021, 01, 01);

			var tariff5407532060 = Factory.Load<USCTariff>(new ZQuery(USCTariffSchema.UE_Tariff, "5407532060")).LastOrDefault();
			if (tariff5407532060 == null)
			{
				tariff5407532060 = Factory.New<USCTariff>();
				tariff5407532060.UE_Tariff = "5407532060";
				tariff5407532060.UE_DutyComputationCode = "7";
				tariff5407532060.UE_Column1RateAdValorem = 0.12m;
			}
			tariff5407532060.UE_DateFrom = new ZDateTime(2020, 01, 01);
			tariff5407532060.UE_DateTo = new ZDateTime(2021, 01, 01);

			var tariff99038803 = Factory.Load<USCTariff>(new ZQuery(USCTariffSchema.UE_Tariff, "99038803")).LastOrDefault();
			if (tariff99038803 == null)
			{
				tariff99038803 = Factory.New<USCTariff>();
				tariff99038803.UE_Tariff = "99038803";
				tariff99038803.UE_DutyComputationCode = "7";
				tariff99038803.UE_Column1RateAdValorem = 0.25m;
			}
			tariff99038803.UE_DateFrom = new ZDateTime(2020, 01, 01);
			tariff99038803.UE_DateTo = new ZDateTime(2021, 01, 01);

			var tariff99038824 = Factory.Load<USCTariff>(new ZQuery(USCTariffSchema.UE_Tariff, "99038824")).LastOrDefault();
			if (tariff99038824 == null)
			{
				tariff99038824 = Factory.New<USCTariff>();
				tariff99038824.UE_Tariff = "99038824";
				tariff99038824.UE_DutyComputationCode = "7";
				tariff99038824.UE_Column1RateAdValorem = 0.15m;
			}
			tariff99038824.UE_DateFrom = new ZDateTime(2020, 01, 01);
			tariff99038824.UE_DateTo = new ZDateTime(2021, 01, 01);

			Factory.Save();

			#endregion

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.ImportEntryNumber = "~9342838";

			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLineOne = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLineOne.JI_Tariff = "5810929080";
			invoiceLineOne.JI_LinePrice = 5000;
			invoiceLineOne.US_UC_NKCountryOfOrigin = "CN";
			invoiceLineOne.US_UC_NKCountryOfExport = "CN";
			invoiceLineOne.JI_CustomsQuantity = 220m;
			invoiceLineOne.JI_CustomsUnitQty = "KG";

			var invoiceLineTwo = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLineTwo.JI_ParentID = invoiceLineOne.PK;
			invoiceLineTwo.JI_Tariff = "5407532060";
			invoiceLineTwo.JI_LinePrice = 0;
			invoiceLineTwo.US_UC_NKCountryOfOrigin = "CN";
			invoiceLineTwo.US_UC_NKCountryOfExport = "CN";
			invoiceLineTwo.JI_CustomsQuantity = 579m;
			invoiceLineTwo.JI_CustomsUnitQty = "M2";
			invoiceLineTwo.JI_CustomsSecondQuantity = 220m;
			invoiceLineTwo.JI_CustomsSecondUnitQty = "KG";

			var invoiceLineThree = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLineThree.JI_Tariff = "5810929080";
			invoiceLineThree.US_SupTariff = "99038824";
			invoiceLineThree.JI_LinePrice = 5000;
			invoiceLineThree.US_UC_NKCountryOfOrigin = "CN";
			invoiceLineThree.US_UC_NKCountryOfExport = "CN";
			invoiceLineThree.JI_CustomsQuantity = 220m;
			invoiceLineThree.JI_CustomsUnitQty = "KG";

			var invoiceLineFour = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLineFour.JI_ParentID = invoiceLineThree.PK;
			invoiceLineFour.JI_Tariff = "5407532060";
			invoiceLineFour.US_SupTariff = "99038803";
			invoiceLineFour.JI_LinePrice = 0;
			invoiceLineFour.US_UC_NKCountryOfOrigin = "CN";
			invoiceLineFour.US_UC_NKCountryOfExport = "CN";
			invoiceLineFour.JI_CustomsQuantity = 579m;
			invoiceLineFour.JI_CustomsUnitQty = "M2";
			invoiceLineFour.JI_CustomsSecondQuantity = 220m;
			invoiceLineFour.JI_CustomsSecondUnitQty = "KG";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			var reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			var reconEntry = reconDec.OriginalEntries.AddNew();
			reconEntry.CH_OrigEntryReference = "XJ5~9342838";
			reconEntry.US_R_ReleaseDate = ZDateTime.Today;
			reconEntry.US_R_DutyRateDate = ZDateTime.Today;
			reconEntry.US_R_CalcOrigDuty = true;
			new ReconImportEntryRetriever(reconDec).ImportLines();
			reconDec.CalculateDutyFeesForAllEntries();
			var reconLines = reconDec.InvoiceLines.Cast<JobComInvoiceLine>().OrderBy(x => x.JI_LineNo).ToArray();
			AssertEquals(4, reconLines.Length);
			CombineAssertions(() =>
			{
				AssertEquals("Recon original duty on 1st line", 600m, reconLines[0].US_R_OrigDuty);
				AssertEquals("Recon original prov duty on 1st line", 0m, reconLines[0].US_R_OrigSupDuty);
				AssertEquals("Recon normal duty on 1st line", 600m, reconLines[0].US_Duty);
				AssertEquals("Recon prov duty on 1st line", 0m, reconLines[0].US_SupDuty);
				AssertEquals("Recon original duty on 2nd line", 0m, reconLines[1].US_R_OrigDuty);
				AssertEquals("Recon original prov duty on 2nd line", 0m, reconLines[1].US_R_OrigSupDuty);
				AssertEquals("Recon normal duty on 2nd line", 0m, reconLines[1].US_Duty);
				AssertEquals("Recon prov duty on 2nd line", 0m, reconLines[1].US_SupDuty);
				AssertEquals("Recon original duty on 3rd line", 600m, reconLines[2].US_R_OrigDuty);
				AssertEquals("Recon original prov duty on 3rd line", 750m, reconLines[2].US_R_OrigSupDuty);
				AssertEquals("Recon normal duty on 3rd line", 600m, reconLines[2].US_Duty);
				AssertEquals("Recon prov duty on 3rd line", 750m, reconLines[2].US_SupDuty);
				AssertEquals("Recon original duty on 4th line", 0m, reconLines[3].US_R_OrigDuty);
				AssertEquals("Recon original prov duty 4th 1st line", 0m, reconLines[3].US_R_OrigSupDuty);
				AssertEquals("Recon normal duty on 4th line", 0m, reconLines[3].US_Duty);
				AssertEquals("Recon prov duty on 4th line", 0m, reconLines[3].US_SupDuty);
			});
		}

		public void TestCalculateADCVDForLineWithAdditionalSupTariffsOnNormalLine()
		{
			#region Setup Tariffs

			var tariff99030120 = USCTariffTest.CreateNewTariffIfNotExist(Factory, "99030120", "7", 0.1m, ZString.Empty);
			var tariff99038802 = USCTariffTest.CreateNewTariffIfNotExist(Factory, "99038802", "7", 0.25m, ZString.Empty);
			var tariff4823690040 = USCTariffTest.CreateNewTariffIfNotExist(Factory, "4823690040", "7", 0m, "KG");

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var hsnTariffType = helper.CreateNewOrGetExistingTariffType("US", "HSN");
			var dutyRateType = helper.CreateNewOrGetExistingRateType("US", "DTY", "Duty");
			var rateCode = helper.LoadOrCreateNewCusRateCode(Factory, "DTY", dutyRateType.PK);
			var tradeGroup = helper.CreateTradeGroup("CN", "CN", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);

			var tariffView99030120 = helper.CreateTariff("US", hsnTariffType.PK, "99030120", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			var rate99030120 = helper.CreateRate(tariffView99030120, rateCode.PK, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date, "0");
			helper.CreateCusApplicability(rate99030120, tradeGroup, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateTariffAttribute("RULE", "A99", tariffView99030120);

			var tariffView99038802 = helper.CreateTariff("US", hsnTariffType.PK, "99038802", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			var rate99038802 = helper.CreateRate(tariffView99038802, rateCode.PK, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date, "0");
			helper.CreateCusApplicability(rate99038802, tradeGroup, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateTariffAttribute("RULE", "A99", tariffView99038802);

			var addCase = Factory.New<USCACCase>();
			addCase.U5_CaseNumber = "A570164003";
			addCase.U5_ISOCountryCode = "CN";
			addCase.U5_CaseStatus = ACCaseStatusList.Codes.AC;
			addCase.U5_CaseStatusDate = ZDateTime.Today;
			addCase.CaseTariffs.AddNew().U9_TariffNumber = "4823690040";
			var addRate = addCase.CaseRates.AddNew();
			addRate.U6_AdValoremRate = 2.6763m;
			addRate.U6_EffectiveDate = ZDateTime.Today;

			var cvdCase = Factory.New<USCACCase>();
			cvdCase.U5_CaseNumber = "C570165004";
			cvdCase.U5_ISOCountryCode = "CN";
			cvdCase.U5_CaseStatus = ACCaseStatusList.Codes.AC;
			cvdCase.U5_CaseStatusDate = ZDateTime.Today;
			cvdCase.CaseTariffs.AddNew().U9_TariffNumber = "4823690040";
			var cvdRate = cvdCase.CaseRates.AddNew();
			cvdRate.U6_AdValoremRate = 3.1314m;
			cvdRate.U6_EffectiveDate = ZDateTime.Today;

			#endregion

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionADDCVD;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_FormattedTariff = "4823.69.0040";
			invoiceLine.SupTariffFormatted = "9903.01.20";
			invoiceLine.SupFormattedAdditionalTariff1 = "9903.88.02";
			invoiceLine.JI_LinePrice = 250m;
			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.China;
			invoiceLine.US_ADDCaseNo = "A570164003";
			invoiceLine.US_CVDCaseNo = "C570165004";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var entry = declaration.FormalEntry;
			var entryLine = entry.MergedLines.Find(x => x.CL_AdValoremTariff == "4823690040").FirstOrDefault();
			AssertEquals(669.08m, entryLine.Fees.GetAmount(Core.Constants.USCustoms.FeeCodes.AntidumpingDuty));
			AssertEquals(782.85m, entryLine.Fees.GetAmount(Core.Constants.USCustoms.FeeCodes.CountervailingDuty));
		}

		public void TestCalculateADCVDForLineWithAdditionalSupTariffsOnXVVLine()
		{
			#region Setup Tariffs

			var tariff99030120 = USCTariffTest.CreateNewTariffIfNotExist(Factory, "99030120", "7", 0.1m, ZString.Empty);
			var tariff99038802 = USCTariffTest.CreateNewTariffIfNotExist(Factory, "99038802", "7", 0.25m, ZString.Empty);
			var tariff4823690040 = USCTariffTest.CreateNewTariffIfNotExist(Factory, "4823690040", "7", 0m, "KG");
			var tariff9503000013 = USCTariffTest.CreateNewTariffIfNotExist(Factory, "9503000013", "7", 0m, "KG");

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var hsnTariffType = helper.CreateNewOrGetExistingTariffType("US", "HSN");
			var dutyRateType = helper.CreateNewOrGetExistingRateType("US", "DTY", "Duty");
			var rateCode = helper.LoadOrCreateNewCusRateCode(Factory, "DTY", dutyRateType.PK);
			var tradeGroup = helper.CreateTradeGroup("CN", "CN", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);

			var tariffView99030120 = helper.CreateTariff("US", hsnTariffType.PK, "99030120", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			var rate99030120 = helper.CreateRate(tariffView99030120, rateCode.PK, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date, "0");
			helper.CreateCusApplicability(rate99030120, tradeGroup, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateTariffAttribute("RULE", "A99", tariffView99030120);

			var tariffView99038802 = helper.CreateTariff("US", hsnTariffType.PK, "99038802", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			var rate99038802 = helper.CreateRate(tariffView99038802, rateCode.PK, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date, "0");
			helper.CreateCusApplicability(rate99038802, tradeGroup, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateTariffAttribute("RULE", "A99", tariffView99038802);

			var addCase = Factory.New<USCACCase>();
			addCase.U5_CaseNumber = "A570164003";
			addCase.U5_ISOCountryCode = "CN";
			addCase.U5_CaseStatus = ACCaseStatusList.Codes.AC;
			addCase.U5_CaseStatusDate = ZDateTime.Today;
			addCase.CaseTariffs.AddNew().U9_TariffNumber = "4823690040";
			var addRate = addCase.CaseRates.AddNew();
			addRate.U6_AdValoremRate = 2.6763m;
			addRate.U6_EffectiveDate = ZDateTime.Today;

			var cvdCase = Factory.New<USCACCase>();
			cvdCase.U5_CaseNumber = "C570165004";
			cvdCase.U5_ISOCountryCode = "CN";
			cvdCase.U5_CaseStatus = ACCaseStatusList.Codes.AC;
			cvdCase.U5_CaseStatusDate = ZDateTime.Today;
			cvdCase.CaseTariffs.AddNew().U9_TariffNumber = "4823690040";
			var cvdRate = cvdCase.CaseRates.AddNew();
			cvdRate.U6_AdValoremRate = 3.1314m;
			cvdRate.U6_EffectiveDate = ZDateTime.Today;

			#endregion

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionADDCVD;

			var invoice = declaration.Invoices.AddNew();
			var xInvoiceLine = invoice.JobComInvoiceLines.AddNew();
			xInvoiceLine.US_SetInd = "X";
			xInvoiceLine.JI_FormattedTariff = "9503.00.0013";
			xInvoiceLine.SupTariffFormatted = "9903.01.20";
			xInvoiceLine.US_UC_NKCountryOfOrigin = "CN";
			var vParentLine = invoice.JobComInvoiceLines.AddNew();
			vParentLine.JI_ParentID = xInvoiceLine.PK;
			vParentLine.US_SetInd = "V";
			vParentLine.JI_FormattedTariff = "9503.00.0013";
			vParentLine.SupTariffFormatted = "9903.01.20";
			vParentLine.US_UC_NKCountryOfOrigin = "CN";
			vParentLine.JI_LinePrice = 2500m;
			var vChildLine = invoice.JobComInvoiceLines.AddNew();
			vChildLine.JI_ParentID = xInvoiceLine.PK;
			vChildLine.US_SetInd = "V";
			vChildLine.JI_FormattedTariff = "4823.69.0040";
			vChildLine.SupTariffFormatted = "9903.01.20";
			vChildLine.SupFormattedAdditionalTariff1 = "9903.88.02";
			vChildLine.US_UC_NKCountryOfOrigin = "CN";
			vChildLine.US_ADDCaseNo = "A570164003";
			vChildLine.US_CVDCaseNo = "C570165004";
			vChildLine.JI_LinePrice = 250m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var entry = declaration.FormalEntry;
			var entryLine = entry.MergedLines.Find(x => x.CL_LineNumber == 3 && x.CL_AdValoremTariff == "99038802").FirstOrDefault();
			AssertEquals(669.08m, entryLine.Fees.GetAmount(Core.Constants.USCustoms.FeeCodes.AntidumpingDuty));
			AssertEquals(782.85m, entryLine.Fees.GetAmount(Core.Constants.USCustoms.FeeCodes.CountervailingDuty));
		}

		public void TestCalculateDutyForDerivedSetsWithAdditionalTariffs()
		{
			#region Setup Tariffs

			var tariff99030120 = USCTariffTest.CreateNewTariffIfNotExist(Factory, "99030120", "7", 0.1m, ZString.Empty);
			var tariff99038802 = USCTariffTest.CreateNewTariffIfNotExist(Factory, "99038802", "7", 0.25m, ZString.Empty);
			var tariff8206000000 = USCTariffTest.CreateNewTariffIfNotExist(Factory, "8206000000", "9", 1m, "PCS");
			var tariff4823690040 = USCTariffTest.CreateNewTariffIfNotExist(Factory, "8203204000", "7", 0.12m, "DOZ");

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var hsnTariffType = helper.CreateNewOrGetExistingTariffType("US", "HSN");
			var dutyRateType = helper.CreateNewOrGetExistingRateType("US", "DTY", "Duty");
			var rateCode = helper.LoadOrCreateNewCusRateCode(Factory, "DTY", dutyRateType.PK);
			var tradeGroup = helper.CreateTradeGroup("CN", "CN", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);

			var tariffView99030120 = helper.CreateTariff("US", hsnTariffType.PK, "99030120", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			var rate99030120 = helper.CreateRate(tariffView99030120, rateCode.PK, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date, "0");
			helper.CreateCusApplicability(rate99030120, tradeGroup, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateTariffAttribute("RULE", "A99", tariffView99030120);

			var tariffView99038802 = helper.CreateTariff("US", hsnTariffType.PK, "99038802", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			var rate99038802 = helper.CreateRate(tariffView99038802, rateCode.PK, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date, "0");
			helper.CreateCusApplicability(rate99038802, tradeGroup, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateTariffAttribute("RULE", "A99", tariffView99038802);

			#endregion

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_FormattedTariff = "8206.00.0000";
			invoiceLine1.SupTariffFormatted = "9903.01.20";
			invoiceLine1.SupFormattedAdditionalTariff1 = "9903.88.02";
			invoiceLine1.JI_LinePrice = 0m;
			invoiceLine1.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.China;
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_ParentID = invoiceLine1.PK;
			invoiceLine2.JI_FormattedTariff = "8203.20.4000";
			invoiceLine2.JI_LinePrice = 10000m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			CombineAssertions(() =>
			{
				AssertEquals("Duty for tariff 8206000000, duty rate comes from child line", 1200m, invoiceLine1.US_Duty);
				AssertEquals("Duty for tariff 99030120", 1000m, invoiceLine1.US_SupDuty);
				AssertEquals("Duty for tariff 99038802", 2500m, invoiceLine1.US_SupAdditionalTariff1Duty);
				AssertEquals("Duty for tariff 8203204000", 0m, invoiceLine2.US_Duty);
			});
		}

		public void TestCalculateDutyForAdditionalSupTariffsWhenSupGoodsValueIsProvided()
		{
			#region Setup Tariffs

			var tariff99030120 = USCTariffTest.CreateNewTariffIfNotExist(Factory, "99030120", "7", 0.1m, ZString.Empty);
			var tariff99038802 = USCTariffTest.CreateNewTariffIfNotExist(Factory, "99038802", "7", 0.25m, ZString.Empty);
			var tariff4823690040 = USCTariffTest.CreateNewTariffIfNotExist(Factory, "4823690040", "7", 0m, "KG");

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var hsnTariffType = helper.CreateNewOrGetExistingTariffType("US", "HSN");
			var dutyRateType = helper.CreateNewOrGetExistingRateType("US", "DTY", "Duty");
			var rateCode = helper.LoadOrCreateNewCusRateCode(Factory, "DTY", dutyRateType.PK);
			var tradeGroup = helper.CreateTradeGroup("CN", "CN", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);

			var tariffView99030120 = helper.CreateTariff("US", hsnTariffType.PK, "99030120", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			var rate99030120 = helper.CreateRate(tariffView99030120, rateCode.PK, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date, "0");
			helper.CreateCusApplicability(rate99030120, tradeGroup, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateTariffAttribute("RULE", "A99", tariffView99030120);

			var tariffView99038802 = helper.CreateTariff("US", hsnTariffType.PK, "99038802", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			var rate99038802 = helper.CreateRate(tariffView99038802, rateCode.PK, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date, "0");
			helper.CreateCusApplicability(rate99038802, tradeGroup, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateTariffAttribute("RULE", "A99", tariffView99038802);

			#endregion

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionADDCVD;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_FormattedTariff = "4823.69.0040";
			invoiceLine.SupTariffFormatted = "9903.01.20";
			invoiceLine.SupFormattedAdditionalTariff1 = "9903.88.02";
			invoiceLine.JI_LinePrice = 2500m;
			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.China;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Duty for tariff 9903.01.20 = 2500 * 0.1", 250m, invoiceLine.US_SupDuty);
			AssertEquals("Duty for tariff 9903.88.02 = 2500 * 0.25", 625m, invoiceLine.US_SupAdditionalTariff1Duty);

			invoiceLine.US_SupGoodsValue = 1000m;
			invoiceLine.US_SupAdditionalTariff1GoodsValue = 500m;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Duty for tariff 9903.01.20 = 1000 * 0.1", 100m, invoiceLine.US_SupDuty);
			AssertEquals("Duty for tariff 9903.88.02 = 500 * 0.25", 125m, invoiceLine.US_SupAdditionalTariff1Duty);
		}

		public void TestCalculateDutyForAdditionalSupTariffsWithSpecificADCVDRate()
		{
			var acCase = Factory.New<USCACCase>();
			acCase.U5_CaseNumber = "A570863000";
			acCase.U5_ISOCountryCode = "CN";
			acCase.U5_CaseStatusDate = ZDateTime.BrettsBirthday;

			var caseRate = acCase.CaseRates.AddNew();
			caseRate.U6_EffectiveDate = ZDateTime.BrettsBirthday;
			caseRate.U6_SpecificRate = 2.63m;
			caseRate.U6_Unit = "KG";

			var caseTariff = acCase.CaseTariffs.AddNew();
			caseTariff.U9_TariffNumber = "0409000065";
			caseTariff.U9_CaseNumber = "A570863000";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_UC_NKCountryOfExport = "CN";
			invoiceLine.US_UC_NKCountryOfOrigin = "CN";
			invoiceLine.JI_LinePrice = 149280m;
			invoiceLine.JI_CustomsQuantity = 120m;
			invoiceLine.JI_Tariff = "0409.00.0065";
			invoiceLine.US_SupTariff = "9903.88.03";
			invoiceLine.US_ADCVDStat = ADDCVDNonReimbursementList.Codes.Declared;
			invoiceLine.US_ADDCaseNo = "A570863000";
			invoiceLine.US_ADDQty = 120m;
			invoiceLine.US_ADDDepositRateIndicator = DepositRateIndicatorList.Codes.Specific;
			invoiceLine.US_ADDDecID = "0000546787";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var entry = declaration.FormalEntry;
			var entryLine1 = entry.MergedLines.Find(x => x.CL_AdValoremTariff == "0409000065").FirstOrDefault();
			AssertEquals(0m, entryLine1.Fees.GetAmount(Core.Constants.USCustoms.FeeCodes.AntidumpingDuty));
			var entryLine2 = entry.MergedLines.Find(x => x.CL_AdValoremTariff == "99038803").FirstOrDefault();
			AssertEquals(315.6m, entryLine2.Fees.GetAmount(Core.Constants.USCustoms.FeeCodes.AntidumpingDuty));

			invoiceLine.SupFormattedAdditionalTariff1 = "99030124";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			entry = declaration.FormalEntry;
			entryLine1 = entry.MergedLines.Find(x => x.CL_AdValoremTariff == "0409000065").FirstOrDefault();
			AssertEquals(315.6m, entryLine1.Fees.GetAmount(Core.Constants.USCustoms.FeeCodes.AntidumpingDuty));
			entryLine2 = entry.MergedLines.Find(x => x.CL_AdValoremTariff == "99038803").FirstOrDefault();
			AssertEquals(0m, entryLine2.Fees.GetAmount(Core.Constants.USCustoms.FeeCodes.AntidumpingDuty));
			var entryLine3 = entry.MergedLines.Find(x => x.CL_AdValoremTariff == "99030124").FirstOrDefault();
			AssertEquals(0m, entryLine3.Fees.GetAmount(Core.Constants.USCustoms.FeeCodes.AntidumpingDuty));
		}

		public void TestCalculateDutyForAdditionalSupTariffsWith98Tariff()
		{
			var acCase = Factory.New<USCACCase>();
			acCase.U5_CaseNumber = "A570827004";
			acCase.U5_ISOCountryCode = "CN";
			acCase.U5_CaseStatusDate = ZDateTime.BrettsBirthday;

			var caseRate = acCase.CaseRates.AddNew();
			caseRate.U6_EffectiveDate = ZDateTime.BrettsBirthday;
			caseRate.U6_AdValoremRate = 0.01;

			var caseTariff = acCase.CaseTariffs.AddNew();
			caseTariff.U9_TariffNumber = "9609100000";
			caseTariff.U9_CaseNumber = "A570827004";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_UC_NKCountryOfExport = "MX";
			invoiceLine.US_UC_NKCountryOfOrigin = "CN";
			invoiceLine.JI_LinePrice = 500m;
			invoiceLine.JI_Tariff = "9609.10.0000";
			invoiceLine.US_SupTariff = "9903.01.24";
			invoiceLine.SupFormattedAdditionalTariff1 = "9802005060";
			invoiceLine.SupFormattedAdditionalTariff2 = "99038815";
			invoiceLine.JI_CustomsQuantity = 1000m;
			invoiceLine.US_98ValueInvCurr = 2000m;
			invoiceLine.US_ADCVDStat = ADDCVDNonReimbursementList.Codes.OnceOff;
			invoiceLine.US_ADDCaseNo = "A570827004";
			invoiceLine.US_ADDDepositRateIndicator = DepositRateIndicatorList.Codes.AdValorem;
			Factory.Save();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var entry = declaration.FormalEntry;
			var entryLine1 = entry.MergedLines.Find(x => x.CL_AdValoremTariff == "9609100000").FirstOrDefault();
			AssertEquals(25m, entryLine1.Fees.GetAmount(Core.Constants.USCustoms.FeeCodes.AntidumpingDuty));
			var entryLine2 = entry.MergedLines.Find(x => x.CL_AdValoremTariff == "99030124").FirstOrDefault();
			AssertEquals(0m, entryLine2.Fees.GetAmount(Core.Constants.USCustoms.FeeCodes.AntidumpingDuty));
			var entryLine3 = entry.MergedLines.Find(x => x.CL_AdValoremTariff == "9802005060").FirstOrDefault();
			AssertEquals(0m, entryLine3.Fees.GetAmount(Core.Constants.USCustoms.FeeCodes.AntidumpingDuty));
			var entryLine4 = entry.MergedLines.Find(x => x.CL_AdValoremTariff == "99038815").FirstOrDefault();
			AssertEquals(0m, entryLine4.Fees.GetAmount(Core.Constants.USCustoms.FeeCodes.AntidumpingDuty));
		}

		protected override void SetUp()
		{
			base.SetUp();
			CusFeeCodeConstantsTestHelper.CreateCusFeeCodeDescriptionPairListForTest();
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			DeclarationTestHelper.SetProcessingDistrictPortCode("8888");
			new FeeCalculationHelperTest().PrepareFeeAndTexData();
		}
	}
}
