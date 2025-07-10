using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class Chapter98HelperTest : TestCaseWithFactory
	{
		[TestDate(2018, 12, 19)]
		public void TestSTNTariffInSets()
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
			childLine.JI_LinePrice = 1000m;
			job.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			var entryHeader = job.ActiveEntryHeaders.EntrySummaryEntry;
			var cusEntryLine = entryHeader.AllEntryLines.Cast<CusEntryLine>().FirstOrDefault();
			var dutyData = new EntryLineIEntryLineOrInvoiceLineDutyData(cusEntryLine);
			Assert(CalculateDutyForSetsHelper.HasSTNTariffInSetsWith9903(dutyData));
			var cusEntryLine2 = entryHeader.AllEntryLines.Cast<CusEntryLine>().FirstOrDefault(x => x.CL_AdValoremTariff == "99038801");
			Assert(CalculateDutyForSetsHelper.ShouldCalculateProvDutyForTariffRuleSTN(new EntryLineIEntryLineOrInvoiceLineDutyData(cusEntryLine2)));
			Assert(CalculateDutyForSetsHelper.IsSTNTariff(parentLine.ImportTariff, parentLine.EffectiveDateForDutyRate));
		}

		public void SetTestTariffCodesForA99()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var startDate = new ZDate(2016, 01, 01);
			var endDate = new ZDate(2079, 01, 01);

			var hsnTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, "HSN");
			Factory.Save();
			var tariff1 = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, hsnTariffType.PK, "91069055", startDate, endDate);
			var progTariff1 = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, hsnTariffType.PK, "99038801", startDate, endDate);
			var dutyRateType = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.RateTypes.Duty, "Duty");
			var rateCode = helper.LoadOrCreateNewCusRateCode(Factory, "DTY", dutyRateType.PK);
			Factory.Save();
			var rate1 = helper.CreateRate(progTariff1, rateCode.PK, startDate, endDate, "0");
			var tradeGroup = helper.CreateTradeGroup(Core.Constants.CountryCodes.China, "CN", startDate, endDate);
			var tradeGroupCountry = helper.AddCountry(tradeGroup, Core.Constants.CountryCodes.China, startDate, endDate);
			var applicability1 = helper.CreateCusApplicability(rate1, tradeGroup, startDate, endDate);
			var relationship1 = helper.CreateTariffRelationship(progTariff1.PK, hsnTariffType.PK, "91069055");
			var tariffAttribute1 = helper.CreateTariffAttribute("RULE", "A99", progTariff1);
			Factory.Save();
		}

		public void TestIsCombinedXLineAndGetCombinedCustomsValue()
		{
			ParentLine.US_SupTariff = Test99038801Tariff.UE_Tariff;
			ParentLine.US_SetInd = "X";

			ChildLine.US_SupTariff = Test99038801Tariff.UE_Tariff;
			ChildLine.JI_Tariff = TestCTariff.UE_Tariff;
			ChildLine.US_SetInd = "V";
			ChildLine.JI_LinePrice = 100m;

			var incoineHeader = ChildLine.InvoiceHeader;
			var invoiceLine3 = incoineHeader.InvoiceLines.AddNew();
			invoiceLine3.JI_ParentID = ParentLine.PK;
			invoiceLine3.US_SetInd = "V";
			invoiceLine3.JI_Tariff = "8204200000";
			invoiceLine3.JI_LinePrice = 200m;

			var invoiceLine4 = incoineHeader.InvoiceLines.AddNew();
			invoiceLine4.JI_ParentID = ParentLine.PK;
			invoiceLine4.US_SetInd = "V";
			invoiceLine4.JI_Tariff = "8204200000";
			invoiceLine4.JI_LinePrice = 300m;

			Charpter98Job.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			Factory.Save();

			var dutyData = new EntryLineIEntryLineOrInvoiceLineDutyData(ParentLine.CusEntryLine);
			Assert(CalculateDutyForSetsHelper.IsCombinedXLine(dutyData));
			AssertEquals(600m, CalculateDutyForSetsHelper.GetSetsCustomsValueForXVLine(dutyData));

			var invoiceLine5 = incoineHeader.InvoiceLines.AddNew();
			invoiceLine5.US_SupTariff = Test99038801Tariff.UE_Tariff;
			invoiceLine5.JI_Tariff = TestCTariff.UE_Tariff;
			invoiceLine5.US_SetInd = "X";

			var invoiceLine6 = incoineHeader.InvoiceLines.AddNew();
			invoiceLine6.US_SupTariff = Test99038801Tariff.UE_Tariff;
			invoiceLine6.JI_ParentID = invoiceLine5.PK;
			invoiceLine6.US_SetInd = "V";

			var invoiceLine7 = incoineHeader.InvoiceLines.AddNew();
			invoiceLine7.US_SupTariff = Test99038801Tariff.UE_Tariff;
			invoiceLine7.JI_ParentID = invoiceLine5.PK;
			invoiceLine7.US_SetInd = "V";
			invoiceLine7.JI_Tariff = "8204200000";
			invoiceLine7.JI_LinePrice = 300m;

			Charpter98Job.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			dutyData = new EntryLineIEntryLineOrInvoiceLineDutyData(invoiceLine5.CusEntryLine);
			Assert(CalculateDutyForSetsHelper.IsCombinedXLine(dutyData));
			AssertEquals(300m, CalculateDutyForSetsHelper.GetSetsCustomsValueForXVLine(dutyData));
		}

		public void TestShouldNotExemptMPFFor98()
		{
			Assert(Chapter98Helper.ShouldNotExemptMPFFor98("98020060"));
			Assert(Chapter98Helper.ShouldNotExemptMPFFor98("980200800"));
			Assert(!Chapter98Helper.ShouldNotExemptMPFFor98("9817"));
		}

		public void TestShouldCalculateDutyOnParentLine()
		{
			var tariff1 = TestCTariff; //7601103000
			Assert(Chapter98Helper.ShouldCalculateDutyOnParentLine(tariff1, tariff1.UE_Tariff, ZDate.Today));

			var tariff2 = Test9802006000Tariff; //9802006000
			Assert(!Chapter98Helper.ShouldCalculateDutyOnParentLine(tariff2, tariff2.UE_Tariff, ZDate.Today));

			var tariffRule1 = Factory.New<USCTariffRule>();
			tariffRule1.U1_RuleCode = TariffRuleList.Codes.AdditionalTariffs;
			tariffRule1.U1_Tariff = tariff2.UE_Tariff;
			tariffRule1.U1_DateFrom = ZDateTime.Today;
			Factory.Save();
			Assert(!Chapter98Helper.ShouldCalculateDutyOnParentLine(tariff2, tariff2.UE_Tariff, ZDate.Today));

			var tariffRule2 = Factory.New<USCTariffRule>();
			tariffRule2.U1_RuleCode = TariffRuleList.Codes.InLieuTariffs;
			tariffRule2.U1_Tariff = tariff1.UE_Tariff;
			tariffRule2.U1_DateFrom = ZDateTime.Today;
			Factory.Save();
			Assert(Chapter98Helper.ShouldCalculateDutyOnParentLine(tariff1, tariff1.UE_Tariff, ZDate.Today));
		}

		[TestDate(2020, 03, 03)]
		public void TestHasEmbroideryTariffLine()
		{
			#region Setup Tariffs

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

			#endregion

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLineOne = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLineOne.JI_Tariff = "5810929080";
			invoiceLineOne.JI_LinePrice = 5000;
			invoiceLineOne.US_UC_NKCountryOfOrigin = "CN";
			invoiceLineOne.US_UC_NKCountryOfExport = "CN";
			invoiceLineOne.JI_CustomsQuantity = 220m;
			invoiceLineOne.JI_CustomsUnitQty = "KG";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var entryLineDutyData = new EntryLineIEntryLineOrInvoiceLineDutyData(declaration.FormalEntry.EntryLines.FirstOrDefault());
			AssertEquals(false, CalculateDutyForEmbroidery.IsParentLineEmbroideryTariff(entryLineDutyData));

			invoiceLineOne.US_SupTariff = "99038824";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			entryLineDutyData = new EntryLineIEntryLineOrInvoiceLineDutyData(declaration.FormalEntry.EntryLines.FirstOrDefault());
			AssertEquals(false, CalculateDutyForEmbroidery.IsParentLineEmbroideryTariff(entryLineDutyData));

			invoiceLineOne.US_SupTariff = ZString.Empty;
			var invoiceLineTwo = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLineTwo.JI_ParentID = invoiceLineOne.PK;
			invoiceLineTwo.JI_Tariff = "5407532060";
			invoiceLineTwo.US_SupTariff = "99038803";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			entryLineDutyData = new EntryLineIEntryLineOrInvoiceLineDutyData(declaration.FormalEntry.EntryLines.FirstOrDefault());
			AssertEquals(true, CalculateDutyForEmbroidery.IsParentLineEmbroideryTariff(entryLineDutyData));

			invoiceLineOne.US_SupTariff = "99038824";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			entryLineDutyData = new EntryLineIEntryLineOrInvoiceLineDutyData(declaration.FormalEntry.EntryLines.FirstOrDefault());
			AssertEquals(true, CalculateDutyForEmbroidery.IsParentLineEmbroideryTariff(entryLineDutyData));
		}

		public void TestIsDutyComputationCodeValid()
		{
			var tariff1 = TestCTariff; //7601103000
			tariff1.UE_DutyComputationCode = ComputationCodeList.Codes.Derived;
			Factory.Save();
			Assert(!Chapter98Helper.IsDutyComputationCodeValid(tariff1));

			tariff1.UE_DutyComputationCode = ComputationCodeList.Codes.Free;
			Factory.Save();
			Assert(!Chapter98Helper.IsDutyComputationCodeValid(tariff1));

			tariff1.UE_DutyComputationCode = ComputationCodeList.Codes.AdValorem;
			Factory.Save();
			Assert(Chapter98Helper.IsDutyComputationCodeValid(tariff1));
		}

		public void TestShouldCalculateProvDutyOnChapter98ChildLineForSection232_WhenNotStartWith98020060()
		{
			CreateTestDataForSection232();
			var tariff9808003000 = new USCTariff.Loader(Factory).LoadBestMatch("9808003000", ZDateTime.Today);
			if (tariff9808003000 == null)
			{
				tariff9808003000 = Factory.New<USCTariff>();
				tariff9808003000.UE_Tariff = "9808003000";
			}
			tariff9808003000.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff9808003000.UE_DateTo = ZDateTime.Today.AddYears(1);
			tariff9808003000.UE_DutyComputationCode = "0";
			tariff9808003000.UE_Column1RateAdValorem = 0m;
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.US_SupTariff = "9808003000";
			invoiceLine1.US_UC_NKCountryOfExport = "HK";
			invoiceLine1.US_UC_NKCountryOfOrigin = "HK";
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_ParentID = invoiceLine1.PK;
			invoiceLine2.US_SupTariff = "99038001";
			invoiceLine2.JI_Tariff = "3920992000";
			invoiceLine2.JI_LinePrice = 5000m;
			invoiceLine2.US_UC_NKCountryOfExport = "HK";
			invoiceLine2.US_UC_NKCountryOfOrigin = "HK";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertEquals(1250m, invoiceLine2.US_SupDuty);
		}

		public void TestShouldCalculateProvDutyOnChapter98ChildLineForSection232_WhenStartWith98020060()
		{
			CreateTestDataForSection232();
			var tariff980200600 = new USCTariff.Loader(Factory).LoadBestMatch("980200600", ZDateTime.Today);
			if (tariff980200600 == null)
			{
				tariff980200600 = Factory.New<USCTariff>();
				tariff980200600.UE_Tariff = "980200600";
			}
			tariff980200600.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff980200600.UE_DateTo = ZDateTime.Today.AddYears(1);
			tariff980200600.UE_Column1RateAdValorem = 0m;
			tariff980200600.UE_DutyComputationCode = "X";
			var rule = tariff980200600.TariffRules.AddNew();
			rule.U1_RuleCode = TariffRuleList.Codes.RepairTariffs;
			rule.U1_DateFrom = ZDateTime.BrettsBirthday;
			rule.U1_DateTo = ZDateTime.Today.AddYears(1);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.US_SupTariff = "980200600";
			invoiceLine1.US_UC_NKCountryOfExport = "HK";
			invoiceLine1.US_UC_NKCountryOfOrigin = "HK";
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_ParentID = invoiceLine1.PK;
			invoiceLine2.US_SupTariff = "99038001";
			invoiceLine2.JI_Tariff = "3920992000";
			invoiceLine2.JI_LinePrice = 5000m;
			invoiceLine2.US_UC_NKCountryOfExport = "HK";
			invoiceLine2.US_UC_NKCountryOfOrigin = "HK";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertEquals(1250m, invoiceLine2.US_SupDuty);

			var startDate = ZDateTime.Today.AddMonths(-1);
			var endDate = ZDateTime.Today.AddMonths(1);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var hsn = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, Customs.Universal.Constants.TariffTypes.HarmonizedSystem);
			var tariff = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.UnitedStates, hsn.PK, "99038001", startDate, endDate);
			var ruleNmae = helper.CreateNewOrGetExistingRefCusTariffAttributeName(Factory, UniversalReferenceConstants.TariffAttributeTypes.Codes.RULE, "RULE", "RULE", Core.Constants.CountryCodes.UnitedStates, hsn.ZZI_TariffType);
			var a99 = helper.CreateNewOrGetExistingTariffAttribute(ruleNmae.ZY6_Name, TariffRuleList.Codes.AdditionalTariffs, tariff);

			tariff980200600.UE_DutyComputationCode = ComputationCodeList.Codes.Free;
			invoiceLine2.US_SupTariff = "99038501";
			invoiceLine2.US_SupTariff = "99038001";
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var newDeclaration = newFactory.Load<JobDeclaration>(declaration.PK);
			newDeclaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var newLine2 = newDeclaration.FilteredInvoiceLines.FirstOrDefault(x => x.JI_Tariff == "3920992000");

			AssertEquals(0m, newLine2.US_SupDuty);
		}

		[TestDate(2025, 03, 26)]
		public void TestCalculateProvDutyOnChapter98ForTariffWith232Attribute()
		{
			#region Setup Tariffs
			USCTariffTest.CreateNewTariffIfNotExist(Factory, "7308909590", "7", 0m, "KG");
			USCTariffTest.CreateNewTariffIfNotExist(Factory, "9817005000", "7", 0m, ZString.Empty);
			USCTariffTest.CreateNewTariffIfNotExist(Factory, "99038802", "7", 0.25m, ZString.Empty);
			USCTariffTest.CreateNewTariffIfNotExist(Factory, "99030124", "7", 0.2m, ZString.Empty);
			USCTariffTest.CreateNewTariffIfNotExist(Factory, "99038190", "7", 0.25m, ZString.Empty);

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var hsnTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.TariffTypes.HarmonizedSystem);
			Factory.Save();
			var tariffView99038802 = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, hsnTariffType.PK, "99038802", new ZDateTime(2025, 01, 01), new ZDateTime(2079, 06, 06), "ATRICLE OF CHINA", 1);
			helper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.RULE, TariffRuleList.Codes.AdditionalTariffs, tariffView99038802);
			helper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.TYPE, UniversalReferenceConstants.TariffAttributeTypes.Values._301, tariffView99038802);
			var tariffView99030124 = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, hsnTariffType.PK, "99030124", new ZDateTime(2025, 03, 04), new ZDateTime(2079, 06, 06), "TEST 99030124", 1);
			helper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.RULE, TariffRuleList.Codes.AdditionalTariffs, tariffView99030124);
			helper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.TYPE, UniversalReferenceConstants.TariffAttributeTypes.Values.IEEPA, tariffView99030124);
			var tariffView99038190 = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, hsnTariffType.PK, "99038190", new ZDateTime(2025, 03, 12), new ZDateTime(2079, 06, 06), "TEST 99038190", 1);
			helper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.RULE, TariffRuleList.Codes.AdditionalTariffs, tariffView99038190);
			helper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.TYPE, UniversalReferenceConstants.TariffAttributeTypes.Values._232, tariffView99038190);

			#endregion

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.China;
			invoiceLine.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.China;
			invoiceLine.JI_FormattedTariff = "7308.90.9590";
			invoiceLine.SupTariffFormatted = "9903.81.90";
			invoiceLine.SupFormattedAdditionalTariff1 = "9817.00.5000";
			invoiceLine.SupFormattedAdditionalTariff2 = "9903.88.02";
			invoiceLine.SupFormattedAdditionalTariff3 = "9903.01.24";
			invoiceLine.JI_LinePrice = 10000m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("US_Duty should be zero", 0m, invoiceLine.US_Duty);
			AssertEquals("US_SupDuty = 10000 * 0.25", 2500m, invoiceLine.US_SupDuty);
			AssertEquals("US_SupAdditionalTariff1Duty should be zero", 0m, invoiceLine.US_SupAdditionalTariff1Duty);
			AssertEquals("US_SupAdditionalTariff2Duty should be zero", 0m, invoiceLine.US_SupAdditionalTariff2Duty);
			AssertEquals("US_SupAdditionalTariff3Duty should be zero", 0m, invoiceLine.US_SupAdditionalTariff3Duty);
		}

		[TestDate(2025, 05, 29)]
		public void TestCalculateProvDutyOnChapter98ForTariff_EligibleForAGOATextileClaims()
		{
			USCTariffTest.CreateNewTariffIfNotExist(Factory, "6205202026", "7", 0.1m, ZString.Empty);
			USCTariffTest.CreateNewTariffIfNotExist(Factory, "99030125", "7", 0.2m, ZString.Empty);
			USCTariffTest.CreateNewTariffIfNotExist(Factory, "98191103", "7", 0.3m, ZString.Empty);
			USCTariffTest.CreateNewTariffIfNotExist(Factory, "98191222", "7", 0.4m, ZString.Empty);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var hsnTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.TariffTypes.HarmonizedSystem);
			Factory.Save();
			var tariffView99030125 = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, hsnTariffType.PK, "99030125", new ZDateTime(2025, 01, 01), new ZDateTime(2079, 06, 06), "A99", 1);
			helper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.RULE, TariffRuleList.Codes.AdditionalTariffs, tariffView99030125);
			var tariffView98191103 = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, hsnTariffType.PK, "98191103", new ZDateTime(2025, 01, 01), new ZDateTime(2079, 06, 06), "98 AGO", 1);
			helper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.RULE, TariffRuleList.Codes.EligibleForAGOATextileClaims, tariffView98191103);
			var tariffView98191222 = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, hsnTariffType.PK, "98191222", new ZDateTime(2025, 01, 01), new ZDateTime(2079, 06, 06), "98 HTH", 1);
			helper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.RULE, TariffRuleList.Codes.HaitiTariffHope, tariffView98191222);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.China;
			invoiceLine.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.China;
			invoiceLine.JI_FormattedTariff = "6205.20.2026";
			invoiceLine.SupTariffFormatted = "9903.01.25";
			invoiceLine.SupFormattedAdditionalTariff1 = "9819.11.03";
			invoiceLine.JI_LinePrice = 10000m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("US_Duty should be zero", 0m, invoiceLine.US_Duty);
			AssertEquals("US_SupDuty = 10000 * 0.2", 2000m, invoiceLine.US_SupDuty);
			AssertEquals("US_SupAdditionalTariff1Duty should be zero", 0m, invoiceLine.US_SupAdditionalTariff1Duty);

			invoiceLine.SupFormattedAdditionalTariff1 = "9819.12.22";
			invoiceLine.JI_LinePrice = 20000m;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("US_Duty should be zero", 0m, invoiceLine.US_Duty);
			AssertEquals("US_SupDuty = 20000 * 0.2", 4000m, invoiceLine.US_SupDuty);
			AssertEquals("US_SupAdditionalTariff1Duty should be zero", 0m, invoiceLine.US_SupAdditionalTariff1Duty);
		}

		void CreateTestDataForSection232()
		{
			var tariff3920992000 = new USCTariff.Loader(Factory).LoadBestMatch("3920992000", ZDateTime.Today);
			if (tariff3920992000 == null)
			{
				tariff3920992000 = Factory.New<USCTariff>();
				tariff3920992000.UE_Tariff = "3920992000";
			}
			tariff3920992000.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff3920992000.UE_DateTo = ZDateTime.Today.AddYears(1);
			tariff3920992000.UE_DutyComputationCode = "7";
			tariff3920992000.UE_Column1RateAdValorem = 0.042m;

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, "HSN");
			var zzTariff = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.UnitedStates, tariffType.PK, "99038001", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.TYPE, UniversalReferenceConstants.TariffAttributeTypes.Values._232, zzTariff);

			var tariff99038001_Section232 = new USCTariff.Loader(Factory).LoadBestMatch("99038001", ZDateTime.Today);
			if (tariff99038001_Section232 == null)
			{
				tariff99038001_Section232 = Factory.New<USCTariff>();
				tariff99038001_Section232.UE_Tariff = "99038001";
			}
			tariff99038001_Section232.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff99038001_Section232.UE_DateTo = ZDateTime.Today.AddYears(1);
			tariff99038001_Section232.UE_DutyComputationCode = "7";
			tariff99038001_Section232.UE_Column1RateAdValorem = 0.25m;
			Factory.Save();
		}

		public JobDeclaration Charpter98Job
		{
			get
			{
				if (charpter98Job == null)
				{
					charpter98Job = Factory.NewWithValidTestData<JobDeclaration>();
					charpter98Job.JE_MessageType = JobMessageTypeList.Codes.Import;
					charpter98Job.US_EntryFilerCode = "XJ5";
					charpter98Job.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
					charpter98Job.US_EnableENS = true;
					var invoice = charpter98Job.Invoices.AddNew();
					parentLine = invoice.JobComInvoiceLines.AddNew();
					parentLine.JI_Tariff = TestCTariff.UE_Tariff;
					parentLine.JI_LinePrice = 5000m;

					childLine = invoice.JobComInvoiceLines.AddNew();
					childLine.JI_ParentID = parentLine.PK;
					Factory.Save();
				}
				return charpter98Job;
			}
		}
		JobDeclaration charpter98Job;

		public JobComInvoiceLine ParentLine
		{
			get
			{
				var job = Charpter98Job;
				return parentLine;
			}
		}
		JobComInvoiceLine parentLine;

		public JobComInvoiceLine ChildLine
		{
			get
			{
				var job = Charpter98Job;
				return childLine;
			}
		}
		JobComInvoiceLine childLine;

		public USCTariff Test98191112Tariff
		{
			get
			{
				if (test98191112Tariff == null)
				{
					test98191112Tariff = CreateOrLoadExistTariff("98191112", 0.1);
				}
				return test98191112Tariff;
			}
		}
		USCTariff test98191112Tariff;

		public USCTariff Test9813Tariff
		{
			get
			{
				if (test9813Tariff == null)
				{
					test9813Tariff = CreateOrLoadExistTariff("9813000520", 0.4);
				}
				return test9813Tariff;
			}
		}
		USCTariff test9813Tariff;

		public USCTariff Test9802005060Tariff
		{
			get
			{
				if (test9802Tariff == null)
				{
					test9802Tariff = CreateOrLoadExistTariff("9802005060", 0.5);
				}
				return test9802Tariff;
			}
		}
		USCTariff test9802Tariff;

		public USCTariff Test99038001Tariff
		{
			get
			{
				if (test99038001Tariff == null)
				{
					test99038001Tariff = CreateOrLoadExistTariff("99038001", 0.4);
				}
				return test99038001Tariff;
			}
		}
		USCTariff test99038001Tariff;

		public USCTariff Test99038801Tariff
		{
			get
			{
				if (test99038801Tariff == null)
				{
					test99038801Tariff = CreateOrLoadExistTariff("99038801", 0.4);
				}
				return test99038801Tariff;
			}
		}
		USCTariff test99038801Tariff;

		public USCTariff Test98020080Tariff
		{
			get
			{
				if (test98020080Tariff == null)
				{
					test98020080Tariff = CreateOrLoadExistTariff("9802008015", 0.4);
				}
				return test98020080Tariff;
			}
		}
		USCTariff test98020080Tariff;

		public USCTariff Test99038802Tariff
		{
			get
			{
				if (test99038802Tariff == null)
				{
					test99038802Tariff = CreateOrLoadExistTariff("99038802", 0.5);
				}
				return test99038802Tariff;
			}
		}
		USCTariff test99038802Tariff;

		public USCTariff Test99038501Tariff
		{
			get
			{
				if (test99038501Tariff == null)
				{
					test99038501Tariff = CreateOrLoadExistTariff("99038501", 0.1);
				}
				return test99038501Tariff;
			}
		}
		USCTariff test99038501Tariff;

		public USCTariff Test9817002000Tariff
		{
			get
			{
				if (test9817002000Tariff == null)
				{
					test9817002000Tariff = CreateOrLoadExistTariff("9817002000", 0.1);
				}
				return test9817002000Tariff;
			}
		}
		USCTariff test9817002000Tariff;

		public USCTariff Test9802006000Tariff
		{
			get
			{
				if (test9802006000Tariff == null)
				{
					test9802006000Tariff = CreateOrLoadExistTariff("9802006000", 0.1);
				}
				return test9802006000Tariff;
			}
		}
		USCTariff test9802006000Tariff;

		public USCTariff Test9802004000Tariff
		{
			get
			{
				if (test9802004000Tariff == null)
				{
					test9802004000Tariff = CreateOrLoadExistTariff("9802004000", 0.1);
				}
				return test9802004000Tariff;
			}
		}
		USCTariff test9802004000Tariff;

		public USCTariff Test9802005000Tariff
		{
			get
			{
				if (test9802005000Tariff == null)
				{
					test9802005000Tariff = CreateOrLoadExistTariff("9802005000", 0.1);
				}
				return test9802005000Tariff;
			}
		}
		USCTariff test9802005000Tariff;

		public USCTariff Test9802008000Tariff
		{
			get
			{
				if (test9802008000Tariff == null)
				{
					test9802008000Tariff = CreateOrLoadExistTariff("9802008000", 0.1);
				}
				return test9802008000Tariff;
			}
		}
		USCTariff test9802008000Tariff;

		public USCTariff Test99038815Tariff
		{
			get
			{
				if (test99038815Tariff == null)
				{
					test99038815Tariff = CreateOrLoadExistTariff("99038815", 0.15);
				}

				return test99038815Tariff;
			}
		}
		USCTariff test99038815Tariff;

		public USCTariff Test99030121Tariff
		{
			get
			{
				if (test99030121Tariff == null)
				{
					test99030121Tariff = CreateOrLoadExistTariff("99030121", 0.15, "0");
				}

				return test99030121Tariff;
			}
		}
		USCTariff test99030121Tariff;

		public USCTariff Test8483509040Tariff
		{
			get
			{
				if (test8483509040Tariff == null)
				{
					test8483509040Tariff = CreateOrLoadExistTariff("8483509040", 0.15);
				}

				return test8483509040Tariff;
			}
		}
		USCTariff test8483509040Tariff;

		public USCTariff TestCTariff
		{
			get
			{
				if (testCTariff == null)
				{
					testCTariff = CreateOrLoadExistTariff("7601103000", 0.7);
				}
				return testCTariff;
			}
		}
		USCTariff testCTariff;

		USCTariff CreateOrLoadExistTariff(ZString tariffCode, ZDecimal column1RateAdValorem, string dutyComputationCode = "7")
		{
			var result = Factory.LoadTop1<USCTariff>(new ZQuery(USCTariffSchema.UE_Tariff, SQLComparisonOperator.StartsWith, tariffCode));
			if (result == null)
			{
				result = Factory.New<USCTariff>();
				result.UE_Tariff = tariffCode;
			}
			if (column1RateAdValorem > 0)
			{
				result.UE_Column1RateAdValorem = column1RateAdValorem;
			}

			if (result.UE_DutyComputationCode.IsEmpty || dutyComputationCode != "7")
			{
				result.UE_DutyComputationCode = dutyComputationCode;
			}

			result.UE_DateFrom = ZDateTime.Today.AddDays(-10);
			result.UE_DateTo = ZDateTime.Today.AddDays(10);
			Factory.Save();

			return result;
		}
	}
}
