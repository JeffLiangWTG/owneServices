using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.ZA.Business.MessageBuilders;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class CusEntryLine_MessageDataProviderTest : TestCaseWithFactory
	{
		[TestDate(2016, 08, 02)]
		public void TestILineLevelInformationTariffCode()
		{
			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory);
			testHelper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "", "45", "00", "", "", "EXW", group: UniversalReferenceConstants.RefCusProcedureGroup.Excise);
			var tariffType1P1 = universalReferenceDataHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, UniversalReferenceConstants.CusTariffCode.Schedule1Part1);
			var tariffType12A = universalReferenceDataHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, UniversalReferenceConstants.CusTariffCode.Schedule1Part2A);
			Factory.Save();
			var tariff1P1 = universalReferenceDataHelper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType1P1.PK, "992001", new ZDateTime(2016, 08, 02), new ZDateTime(2079, 06, 06));
			var tariff12A = universalReferenceDataHelper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType12A.PK, "992002", new ZDateTime(2016, 08, 02), new ZDateTime(2079, 06, 06));
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var testInstruction1 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInstruction1.CEI_Style = "45";
			var invHeader = declaration.Invoices.AddNew();
			var invLine = invHeader.InvoiceLines.AddNew();
			invLine.JI_CEI = testInstruction1.PK;
			invLine.JI_Procedure = "4500";
			var lineTariff = invLine.CusLineTariffDetails.AddNew("12A", "992002");
			var entry = declaration.ActiveEntryHeaders.AddNew();
			var entryLine = entry.AllEntryLines.AddNew();
			invLine.JI_CL = entryLine.PK;
			var provider = (ILineLevelInformation)entryLine;
			AssertEquals("992002000", provider.TariffCode);
			invLine.JI_Procedure = ZString.Empty;
			invLine.JI_Tariff = "992001";
			AssertEquals("992001", provider.TariffCode);
		}

		public void TestRebateUser()
		{
			var tariffType1P1 = universalReferenceDataHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, UniversalReferenceConstants.CusTariffCode.Schedule1Part1);
			var tariffType6P1 = universalReferenceDataHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "6P1");
			var tariffType3P1 = universalReferenceDataHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "3P1");
			var tariffType4P1 = universalReferenceDataHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "4P1");
			Factory.Save();
			var startDate = new ZDateTime(1990, 1, 1);
			var endDate = new ZDateTime(2075, 1, 1);
			var universalHelper = new UniversalReferenceTestDataHelper(Factory);
			universalHelper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "ZA", "64", "00", "6", "", "EXP", "");
			universalHelper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "ZA", "85", "00", "3", "", "IMP", "");
			universalHelper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "ZA", "13", "00", "4", "", "IMP", "");
			var tariff6 = universalReferenceDataHelper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType6P1.PK, "6###", startDate, endDate);
			universalReferenceDataHelper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributes.CheckDigit, "11", tariff6);
			var tariff3 = universalReferenceDataHelper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType3P1.PK, "3###", startDate, endDate);
			universalReferenceDataHelper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributes.CheckDigit, "22", tariff3);
			var tariff4 = universalReferenceDataHelper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType4P1.PK, "4###", startDate, endDate);
			universalReferenceDataHelper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributes.CheckDigit, "33", tariff4);
			var s1p1TariffType = universalHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "1P1");
			universalHelper.CreateTariffRelationship(tariff6.PK, s1p1TariffType.PK, ZString.Empty);
			universalHelper.CreateTariffRelationship(tariff3.PK, s1p1TariffType.PK, ZString.Empty);
			universalHelper.CreateTariffRelationship(tariff4.PK, s1p1TariffType.PK, ZString.Empty);
			Factory.Save();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			var ccd = importer.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CustomsClientCode, "0110110", Core.Constants.CountryCodes.SouthAfrica);
			var reb = importer.CustomsCodes.AddNew(OrgCusCode.CodeTypes.RebateUserCode, "0110111", Core.Constants.CountryCodes.SouthAfrica);
			declaration.JE_OH_Importer = importer.PK;
			var ceInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			ceInstruction.CEI_Style = UniversalReferenceConstants.ProcedureCodes._64;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			var line1 = invoice.JobComInvoiceLines.AddNew();
			line1.JI_Description = "abcde";
			line1.CusLineTariffDetails.RemoveAll();
			var clTariffDetails = line1.CusLineTariffDetails.AddNew("6P1", "6###");
			new LineMerger(declaration).DoMerge();
			var entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];
			ILineLevelInformation lineLevel = entryLine;
			AssertEquals("pre-condition", "6###11", lineLevel.ProcedureMeasure);
			AssertEquals("no rebate user output", "", lineLevel.RebateUserCode);
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			ceInstruction.CEI_Style = "85";
			line1.CusLineTariffDetails.RemoveAll();
			clTariffDetails = line1.CusLineTariffDetails.AddNew("3P1", "3###");
			new LineMerger(declaration).DoMerge();
			entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];
			lineLevel = entryLine;
			AssertEquals("pre-condition", "3###22", lineLevel.ProcedureMeasure);
			AssertEquals("rebate user output", "0110111", lineLevel.RebateUserCode);
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			ceInstruction.CEI_Style = "13";
			line1.CusLineTariffDetails.RemoveAll();
			clTariffDetails = line1.CusLineTariffDetails.AddNew("4P1", "4###");
			new LineMerger(declaration).DoMerge();
			entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];
			lineLevel = entryLine;
			CombineAssertions(() =>
			{
				AssertEquals("pre-condition", "4###33", lineLevel.ProcedureMeasure);
				AssertEquals("importer rebate user output", "0110111", lineLevel.RebateUserCode);
			});
			importer.CustomsCodes.Remove(reb);
			new LineMerger(declaration).DoMerge();
			entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];
			lineLevel = entryLine;
			CombineAssertions(() =>
			{
				AssertEquals("pre-condition", "4###33", lineLevel.ProcedureMeasure);
				AssertEquals("importer rebate user output", "0110110", lineLevel.RebateUserCode);
			});
		}

		public void TestProcedureMeasure()
		{
			var tariffType6P1 = universalReferenceDataHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "6P1");
			var rateType_ZA_REF = universalReferenceDataHelper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.SouthAfrica, "REF", "Refd");
			var rateCode_ZA_REF_D = universalReferenceDataHelper.LoadOrCreateNewCusRateCode(Factory, "6P1", rateType_ZA_REF.PK);
			Factory.Save();
			var startDate = new ZDateTime(1990, 1, 1);
			var endDate = new ZDateTime(2075, 1, 1);
			var procedure = universalReferenceDataHelper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "", "64", "00", "6", "", "EXP", "");
			var tariff6 = universalReferenceDataHelper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType6P1.PK, "6###", startDate, endDate);
			universalReferenceDataHelper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributes.CheckDigit, "11", tariff6);
			Factory.Save();
			var ceInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			ceInstruction.CEI_Style = UniversalReferenceConstants.ProcedureCodes._64;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			var line1 = invoice.JobComInvoiceLines.AddNew();
			line1.JI_Description = "abcde";
			line1.CusLineTariffDetails.RemoveAll();
			var clTariffDetails = line1.CusLineTariffDetails.AddNew("6P1", "6###");
			new LineMerger(declaration).DoMerge();
			var entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];
			ILineLevelInformation lineLevel = entryLine;
			var entry = entryLine.Header;
			AssertEquals("6###11", lineLevel.ProcedureMeasure);
			var fee = entryLine.Fees.GetOrAddFeeByFeeType("6P1");
			fee.CF_IsLandedCostOnly = true;
			AssertEquals(ZString.Empty, lineLevel.ProcedureMeasure);
			fee.Delete();
			AssertEquals("6###11", lineLevel.ProcedureMeasure);
		}

		public void TestLandedCostData()
		{
			//1P1
			var tariff1 = universalReferenceDataHelper.CreateRateViewWithTariffTypeAndRateType(Core.Constants.CountryCodes.SouthAfrica, UniversalReferenceConstants.CusTariffCode.Schedule1Part1, "DTY", "991001", Universal.Constants.RateTypes.AntiDumping);
			tariff1.Rates.FirstOrDefault().CusRateType.ZZR_IsPayable = true;
			universalReferenceDataHelper.CreateTariffUOM(tariff1, UniversalReferenceConstants.RefCusTariffUOMTypes.StatisticalUOMType, "LI");
			universalReferenceDataHelper.CreateTariffUOM(tariff1, UniversalReferenceConstants.RefCusTariffUOMTypes.ClassificationUOMType, "NO");
			//12A
			var tariff2 = universalReferenceDataHelper.CreateRateViewWithTariffTypeAndRateType(Core.Constants.CountryCodes.SouthAfrica, "12A", "DTY", "991012", Universal.Constants.RateTypes.Duty);
			tariff2.Rates.FirstOrDefault().CusRateType.ZZR_IsPayable = true;
			universalReferenceDataHelper.CreateTariffUOM(tariff2, UniversalReferenceConstants.RefCusTariffUOMTypes.StatisticalUOMType, "LA");
			//Relationship
			universalReferenceDataHelper.CreateTariffRelationship(tariff2.PK, tariff1.ZZ1_ZZI_TariffType, tariff1.ZZ1_TariffCode);
			Factory.Save();
			var ceInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CEI = ceInstruction.PK;
			new LineMerger(declaration).DoMerge();
			var entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];
			var entry = entryLine.Header;
			entry.CH_CEI_Instruction = ceInstruction.PK; // To be remove when we use CH_CEI_Instruction in merging
			ILineLevelInformation lineLevel = entryLine;
			var fee1 = entryLine.Fees.AddOrUpdate("1P1", 10m);
			var fee2 = entryLine.Fees.AddOrUpdate("2P1", 20m);
			var fee3 = entryLine.Fees.AddOrUpdate("2P2", 30m);
			invoiceLine.JI_Tariff = "991001";
			invoiceLine.CusLineTariffDetails.RemoveAll();
			var tariffDetail = invoiceLine.CusLineTariffDetails.AddNew();
			tariffDetail.BZ_Type = UniversalReferenceConstants.CusTariffCode.Schedule1Part2A;
			tariffDetail.BZ_Tariff = "991012";
			AssertEquals("Customs Additional Unit", "LA", invoiceLine.JI_CustomsSecondUnitQty);
			invoiceLine.JI_CustomsSecondQuantity = 10m;
			AssertEquals("AdditionalUnitQty", "LA", lineLevel.AdditionalUnitQty);
			AssertEquals("AdditionalQuantity", 10m, lineLevel.AdditionalQuantity);
			var dutiesAndFees = lineLevel.DutiesAndFees.ToArray();
			AssertEquals(3, dutiesAndFees.Length);
			fee1.CF_IsLandedCostOnly = true;
			fee2.CF_IsLandedCostOnly = true;
			fee3.CF_IsLandedCostOnly = true;
			dutiesAndFees = lineLevel.DutiesAndFees.ToArray();
			AssertEquals(0, dutiesAndFees.Length);
		}

		public void TestActualPriceAndCustomsValue()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = "IMP";
			var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = "11";
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 500;
			invoice.JZ_IncoTerm = "FOB";
			invoice.JZ_RX_NKInvoice_Currency = "ZAR";
			JobComInvoiceLine line = invoice.JobComInvoiceLines.AddNew();
			line.JI_CEI = instruction.PK;
			line.JI_Procedure = line.EntryInstruction.CEI_Style + "00";
			line.JI_LinePrice = 500;
			Factory.Save();
			CombineAssertions(() =>
			{
				{
					new LineMerger(declaration).DoMerge();
					var target = declaration.CustomsEntryHeaders[0].MergedLines[0] as ILineLevelInformation;
					AssertEquals("ActualPrice: None", 500m, target.ActualPrice);
					AssertEquals("CustomsValue: none", 500m, target.CustomsValue);
				}

				{
					line.Charges.RemoveAndDeleteAll();
					line.Charges.AddNew("INT", 50m);
					new LineMerger(declaration).DoMerge();
					var target = declaration.CustomsEntryHeaders[0].MergedLines[0] as ILineLevelInformation;
					AssertEquals("ActualPrice: INT", 500m, target.ActualPrice);
					AssertEquals("CustomsValue: INT", 450m, target.CustomsValue);
				}
			});
		}

		public void TestActualPrice_ShouldFirstRoundTo4DecimalPlaces_ThenApplyRoundingRules()
		{
			var helper = new TestHelper(Factory);
			helper.SetExchangeRate(helper.USDCurrency, 0.070771, ZDateTime.Today);
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MasterBillIssuedDate = ZDateTime.Today;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = "IMP";
			var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = "11";
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 100000m;
			invoice.JZ_RX_NKInvoice_Currency = "USD";
			var line = invoice.JobComInvoiceLines.AddNew();
			line.JI_CEI = instruction.PK;
			line.JI_LinePrice = 10974m;
			Factory.Save();
			AssertEquals(155063.50999140m, line.JI_Calc_ActualPrice);
			new LineMerger(declaration).DoMerge();
			var target = declaration.CustomsEntryHeaders[0].MergedLines[0] as ILineLevelInformation;
			AssertEquals(155064m, target.ActualPrice);
			AssertEquals(155064m, target.CustomsValue);
		}

		public void TestDutiesAndFees()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			var fee1 = entryLine.Fees.AddNew();
			fee1.CF_ChargeType = "1P1";
			var fee2 = entryLine.Fees.AddNew();
			fee2.CF_ChargeType = "VAT";
			var fee3 = entryLine.Fees.AddNew();
			fee3.CF_ChargeType = "3P1";
			var dutiesAndFees = ((ILineLevelInformation)entryLine).DutiesAndFees;
			foreach (CodeDescriptionPair declarationTypePair in new DeclarationTypeList())
			{
				entryLine.MessageKeyFactor.DeclarationType = declarationTypePair.Code;
				switch (declarationTypePair.Code)
				{
					case DeclarationTypeList.Codes.RegularIncompleteDeclaration:
					case DeclarationTypeList.Codes.RegularProvisionalDeclaration:
						AssertEquals(0, dutiesAndFees.Count());
						break;
					case DeclarationTypeList.Codes.RegularCompleteDeclarationDefault:
					case DeclarationTypeList.Codes.RegularSupplementaryDeclaration:
						AssertEquals(2, dutiesAndFees.Count());
						AssertContainsExactElementsInAnyOrder(new List<CusEntryLineFee> { fee1, fee2 }, dutiesAndFees);
						break;
				}
			}
		}

		public void TestProvisionalPayments()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_ProvisionalPaymentType = HeaderLevelProvisionalPayments.Codes.PPE;
			instruction.CEI_ProvisionalPaymentAmount = 66.11m;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			CombineAssertions("Miscellaneous", () =>
			{
				declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Miscellaneous;
				var entryLine = entryHeader.MergedLines.AddNew();
				entryLine.InvoiceLines.Add(invoiceLine);
				entryLine.CL_LineNumber = 2;
				var pp1 = entryLine.ProvisionalPayments.AddNew("PEN", 0);
				var pp2 = entryLine.ProvisionalPayments.AddNew("PEN", 11.11);
				var pp3 = entryLine.ProvisionalPayments.AddNew("XXT", 22.11);
				var pp4 = entryLine.ProvisionalPayments.AddNew("", 33.11);
				var pp5 = entryLine.ProvisionalPayments.AddNew("PPA", 44.11);
				var pp6 = entryLine.ProvisionalPayments.AddNew("PPA", 55.11);
				var pps = ((ILineLevelInformation)entryLine).ProvisionalPayments;
				AssertEquals(0, pps.Count());
			});
			foreach (var messageType in new[] { ZAJobMessageTypeList.Codes.Import, ZAJobMessageTypeList.Codes.ExBond, ZAJobMessageTypeList.Codes.Export })
			{
				declaration.JE_MessageType = messageType;
				var entryLine = entryHeader.MergedLines.AddNew();
				entryLine.InvoiceLines.Add(invoiceLine);
				entryLine.CL_LineNumber = 2;
				var pp1 = entryLine.ProvisionalPayments.AddNew("PEN", 0);
				var pp2 = entryLine.ProvisionalPayments.AddNew("PEN", 11.11);
				var pp3 = entryLine.ProvisionalPayments.AddNew("XXT", 22.11);
				var pp4 = entryLine.ProvisionalPayments.AddNew("", 33.11);
				var pp5 = entryLine.ProvisionalPayments.AddNew("PPA", 44.11);
				var pp6 = entryLine.ProvisionalPayments.AddNew("PPA", 55.11);
				var pps = ((ILineLevelInformation)entryLine).ProvisionalPayments;
				foreach (CodeDescriptionPair declarationTypePair in new DeclarationTypeList())
				{
					entryLine.MessageKeyFactor.DeclarationType = declarationTypePair.Code;
					switch (declarationTypePair.Code)
					{
						case DeclarationTypeList.Codes.RegularIncompleteDeclaration:
						case DeclarationTypeList.Codes.RegularProvisionalDeclaration:
							AssertEquals(messageType, 0, pps.Count());
							break;
						case DeclarationTypeList.Codes.RegularCompleteDeclarationDefault:
						case DeclarationTypeList.Codes.RegularSupplementaryDeclaration:
							CombineAssertions(messageType, () =>
							{
								AssertEquals(4, pps.Count());
								AssertContainsExactElementsInAnyOrder(new List<ProvisionalPaymentAmountCodeData> { pp2, pp3, pp5, pp6 }, pps);
							});
							break;
					}
				}
			}
		}

		public void TestProvisionalPayments_ForLine1()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_ProvisionalPaymentType = HeaderLevelProvisionalPayments.Codes.PPE;
			instruction.CEI_ProvisionalPaymentAmount = 66.11m;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			CombineAssertions("Export", () =>
			{
				declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
				var entryLine = entryHeader.MergedLines.AddNew();
				entryLine.InvoiceLines.Add(invoiceLine);
				entryLine.CL_LineNumber = 1;
				var pp1 = entryLine.ProvisionalPayments.AddNew("PEN", 0);
				var pp2 = entryLine.ProvisionalPayments.AddNew("PEN", 11.11);
				var pp3 = entryLine.ProvisionalPayments.AddNew("XXT", 22.11);
				var pp4 = entryLine.ProvisionalPayments.AddNew("", 33.11);
				var pp5 = entryLine.ProvisionalPayments.AddNew("PPA", 44.11);
				var pp6 = entryLine.ProvisionalPayments.AddNew("PPA", 55.11);
				var pps = ((ILineLevelInformation)entryLine).ProvisionalPayments;
				AssertEquals(4, pps.Count());
				Assert(pps.Any(x => x.Code == "PEN" && x.Value == 11.11m));
				Assert(pps.Any(x => x.Code == "XXT" && x.Value == 22.11m));
				Assert(pps.Any(x => x.Code == "PPA" && x.Value == 44.11m));
				Assert(pps.Any(x => x.Code == "PPA" && x.Value == 55.11m));
			});
			CombineAssertions("Import", () =>
			{
				declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
				var entryLine = entryHeader.MergedLines.AddNew();
				entryLine.InvoiceLines.Add(invoiceLine);
				entryLine.CL_LineNumber = 1;
				var pp1 = entryLine.ProvisionalPayments.AddNew("PEN", 0);
				var pp2 = entryLine.ProvisionalPayments.AddNew("PEN", 11.11);
				var pp3 = entryLine.ProvisionalPayments.AddNew("XXT", 22.11);
				var pp4 = entryLine.ProvisionalPayments.AddNew("", 33.11);
				var pp5 = entryLine.ProvisionalPayments.AddNew("PPA", 44.11);
				var pp6 = entryLine.ProvisionalPayments.AddNew("PPA", 55.11);
				var pp7 = entryLine.ProvisionalPayments.AddNew("PPE", 66.11);
				var pps = ((ILineLevelInformation)entryLine).ProvisionalPayments;
				AssertEquals(6, pps.Count());
				Assert(pps.Any(x => x.Code == "PEN" && x.Value == 11.11m));
				Assert(pps.Any(x => x.Code == "XXT" && x.Value == 22.11m));
				Assert(pps.Any(x => x.Code == "PPA" && x.Value == 44.11m));
				Assert(pps.Any(x => x.Code == "PPA" && x.Value == 55.11m));
				Assert(pps.Any(x => x.Code == "PPE" && x.Value == 66.11m));
			});
			CombineAssertions("Exbond", () =>
			{
				declaration.JE_MessageType = ZAJobMessageTypeList.Codes.ExBond;
				var entryLine = entryHeader.MergedLines.AddNew();
				entryLine.InvoiceLines.Add(invoiceLine);
				entryLine.CL_LineNumber = 1;
				var pp1 = entryLine.ProvisionalPayments.AddNew("PEN", 0);
				var pp2 = entryLine.ProvisionalPayments.AddNew("PEN", 11.11);
				var pp3 = entryLine.ProvisionalPayments.AddNew("XXT", 22.11);
				var pp4 = entryLine.ProvisionalPayments.AddNew("", 33.11);
				var pp5 = entryLine.ProvisionalPayments.AddNew("PPA", 44.11);
				var pp6 = entryLine.ProvisionalPayments.AddNew("PPA", 55.11);
				var pp7 = entryLine.ProvisionalPayments.AddNew("PPE", 66.11);
				var pps = ((ILineLevelInformation)entryLine).ProvisionalPayments;
				AssertEquals(6, pps.Count());
				Assert(pps.Any(x => x.Code == "PEN" && x.Value == 11.11m));
				Assert(pps.Any(x => x.Code == "XXT" && x.Value == 22.11m));
				Assert(pps.Any(x => x.Code == "PPA" && x.Value == 44.11m));
				Assert(pps.Any(x => x.Code == "PPA" && x.Value == 55.11m));
				Assert(pps.Any(x => x.Code == "PPE" && x.Value == 66.11m));
			});
		}

		public void TestProvisionalPayments_NotIncludingClosedPPCase()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_ProvisionalPaymentType = HeaderLevelProvisionalPayments.Codes.PPE;
			instruction.CEI_ProvisionalPaymentAmount = 66m;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			invoiceLine.JI_Description = "DESC1";
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_CEI = instruction.PK;
			invoiceLine2.JI_Description = "DESC2";
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var case1 = entryHeader.EntryPayInfos.AddNewEntryPayInfo(ZDateTime.Now, "C", HeaderLevelProvisionalPayments.Codes.PPE, 0m, "1", "REF1");
			var case2 = entryHeader.EntryPayInfos.AddNewEntryPayInfo(ZDateTime.Now, "C", LineLevelProvisionalPayments.Codes.PPA, 0m, "1", "REF2");
			var case3 = entryHeader.EntryPayInfos.AddNewEntryPayInfo(ZDateTime.Now, "C", LineLevelProvisionalPayments.Codes.PPC, 0m, "2", "REF3");
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var entryLine = entryHeader.MergedLines.AddNew();
			entryLine.InvoiceLines.Add(invoiceLine);
			entryLine.CL_LineNumber = 1;
			var pp11 = entryLine.ProvisionalPayments.AddNew("PEN", 11.11);
			var pp12 = entryLine.ProvisionalPayments.AddNew("PPE", 22.11);
			var pp13 = entryLine.ProvisionalPayments.AddNew("PPA", 33.11);
			var pp14 = entryLine.ProvisionalPayments.AddNew("PPA", 44.11);
			var pp15 = entryLine.ProvisionalPayments.AddNew("PPC", 55.11);
			var entryLine2 = entryHeader.MergedLines.AddNew();
			entryLine2.InvoiceLines.Add(invoiceLine2);
			entryLine2.CL_LineNumber = 2;
			var pp21 = entryLine2.ProvisionalPayments.AddNew("PEN", 11.11);
			var pp22 = entryLine2.ProvisionalPayments.AddNew("PPE", 22.11);
			var pp23 = entryLine2.ProvisionalPayments.AddNew("PPA", 33.11);
			var pp24 = entryLine2.ProvisionalPayments.AddNew("PPA", 44.11);
			var pp25 = entryLine2.ProvisionalPayments.AddNew("PPC", 55.11);
			CombineAssertions("No Case Closed", () =>
			{
				var pps1 = ((ILineLevelInformation)entryLine).ProvisionalPayments;
				var pps2 = ((ILineLevelInformation)entryLine2).ProvisionalPayments;
				AssertEquals(6, pps1.Count());
				AssertEquals(5, pps2.Count());
				AssertContainsExactElementsInAnyOrder(new List<string> { "PPE:66", "PEN:11.11", "PPE:22.11", "PPA:33.11", "PPA:44.11", "PPC:55.11", }, pps1.Select(x => string.Format("{0}:{1}", x.Code, x.Value)));
				AssertContainsExactElementsInAnyOrder(new List<ProvisionalPaymentAmountCodeData> { pp21, pp22, pp23, pp24, pp25 }, pps2);
			});
			CombineAssertions("Case Closed", () =>
			{
				case1.C9_RemAdvReceived = true;
				case2.C9_RemAdvReceived = true;
				case3.C9_RemAdvReceived = true;
				var pps1 = ((ILineLevelInformation)entryLine).ProvisionalPayments;
				var pps2 = ((ILineLevelInformation)entryLine2).ProvisionalPayments;
				AssertEquals(2, pps1.Count());
				AssertEquals(4, pps2.Count());
				AssertContainsExactElementsInAnyOrder(new List<ProvisionalPaymentAmountCodeData> { pp11, pp15 }, pps1);
				AssertContainsExactElementsInAnyOrder(new List<ProvisionalPaymentAmountCodeData> { pp21, pp22, pp23, pp24 }, pps2);
			});
		}

		public void TestQuantitySumAndRound()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = "40";
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_CEI = instruction.PK;
			invoiceLine1.JI_CustomsQuantity = 1;
			invoiceLine1.JI_CustomsSecondQuantity = 2;
			invoiceLine1.JI_CustomsThirdQuantity = 3;
			invoiceLine1.JI_BondedWhsQuantity = 4;
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_CEI = instruction.PK;
			invoiceLine2.JI_CustomsQuantity = 1.1;
			invoiceLine2.JI_CustomsSecondQuantity = 2.2;
			invoiceLine2.JI_CustomsThirdQuantity = 3.3;
			invoiceLine2.JI_BondedWhsQuantity = 4;
			var invoiceLine3 = invoice.InvoiceLines.AddNew();
			invoiceLine3.JI_CEI = instruction.PK;
			invoiceLine3.JI_CustomsQuantity = 1.015;
			invoiceLine3.JI_CustomsSecondQuantity = 2.025;
			invoiceLine3.JI_CustomsThirdQuantity = 3.035;
			invoiceLine3.JI_BondedWhsQuantity = 4.5;
			declaration.DoMerge();
			var tester = declaration.ActiveEntryHeaders[0].MergedLines[0] as ILineLevelInformation;
			CombineAssertions(() =>
			{
				AssertEquals("CustomsQuantity", 3.12m, tester.CustomsQuantity);
				AssertEquals("AdditionalQuantity", 6.23m, tester.AdditionalQuantity);
				AssertEquals("ClassificationQuantity", 9.34m, tester.ClassificationQuantity);
				AssertEquals("WarehouseCountableQuantity", 13.0m, tester.WarehouseCountableQuantity);
			});
		}

		ZAUniversalReferenceTestDataHelper universalReferenceDataHelper;
		protected override void SetUp()
		{
			universalReferenceDataHelper = new ZAUniversalReferenceTestDataHelper(Factory);
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
		}

		JobDeclaration declaration;
	}
}
