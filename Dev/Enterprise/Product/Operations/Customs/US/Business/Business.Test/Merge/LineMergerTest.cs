using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Customs.US;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class LineMergerTest : TestCaseWithFactory
	{
		public void TestComparerToSortInvoiceLinesAndEntryLines()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			AssertEquals("invoice line 2.JI_LineNo > invoiceLine.JI_LineNo", (short)1, invoiceLine.JI_LineNo);
			AssertEquals("invoice line 2.JI_LineNo > invoiceLine.JI_LineNo", (short)2, invoiceLine2.JI_LineNo);

			invoiceLine2.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.X;
			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;
			invoiceLine.JI_ParentID = invoiceLine2.PK;

			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();

			AssertNoExceptionThrown(delegate
			{ declaration.DoMerge(); });

			AssertEquals("parent entry line's CL_LineNumber", (short)1, invoiceLine2.CusEntryLine.CL_LineNumber);
			AssertEquals("child entry line's CL_LineNumber", (short)2, invoiceLine.CusEntryLine.CL_LineNumber);
		}

		public void TestEntryCreationStrategy()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = "";
			declaration.US_EnableINB = true;
			AssertEquals(true, declaration.IsInBond);

			var merger = new TestLineMerger(declaration);
			var result = merger.GetEntryCreationStrategies();
			AssertEquals("Even though ENS is disabled, it should be part of the array as Strategy.IsActive will be taken into account", 23, result.Length);

			declaration.US_EnableINB = false;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			AssertEquals(false, declaration.IsInBond);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			result = merger.GetEntryCreationStrategies();
			AssertEquals("Even though ENS is disabled, it should be part of the array as Strategy.IsActive will be taken into account", 23, result.Length);
			AssertEquals("for export", typeof(AESMergeStrategy), result[22].GetType());
		}

		public void TestClearCalculateExceptionOnMerging()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_IncoTerm = TermsOfDeliveryList.Codes.DDP;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.JZ_InvoiceAmount = 10000m;

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "98220105";
			invoiceLine.JI_LinePrice = 99999999m;
			invoiceLine.TariffCalculateExceptionMessage = "Calculate error";

			AssertEquals("InvoiceLine has calculate error", "Calculate error", invoiceLine.TariffCalculateExceptionMessage);

			var merger = new LineMerger(declaration);
			merger.DoMerge();

			Assert("Calculate error cleared", invoiceLine.TariffCalculateExceptionMessage.IsEmpty);
		}

		public void TestDoMergeWillRecalculateTotalEnteredValue()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 12580m;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals(12580m, declaration.US_TotalEnteredValue);
			AssertEquals(12580m, declaration.ActiveEntryHeaders.EntrySummaryEntry.TotalEnteredValue);
		}

		[TestDate(2017, 12, 1)]
		public void TestUS_PayableMPF()
		{
			new FeeCalculationHelperTest().PrepareFeeAndTexData();
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 1m;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertEquals(25m, invoiceLine.US_PayableMPF);
		}

		public void TestMergeDoesNotDeleteMandatoryFeesWithZeroAmount()
		{
			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "1010101010";
			tariff.UE_DateFrom = ZDateTime.Today.AddYears(-1);
			tariff.UE_DateTo = ZDateTime.Today.AddYears(1);
			USCTariffDutyRate dutyRate = tariff.DutyRates.AddNew();
			dutyRate.UD_TaxFeeClassCode = Core.Constants.USCustoms.FeeCodes.OtherExcise;
			dutyRate.UD_TaxFeeComputationCode = ComputationCodeList.Codes.NoComputationFormulaAvailable;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			invoiceLine.JI_CustomsQuantity = 1m;
			invoiceLine.US_TaxApply = TaxApplyList.Codes.Yes;
			invoiceLine.US_TaxCode = Core.Constants.USCustoms.FeeCodes.OtherExcise;
			invoiceLine.US_TaxRateS = AppendixBTaxRateList.Codes.Other_4;
			invoiceLine.US_TaxQty = 0m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertEquals(1, invoiceLine.FeeCusCodes.Count);
			AssertEquals(ZBool.False, invoiceLine.FeeCusCodes.GetFirstElementHaving(invoiceLine.US_TaxCode).CY_IsOverridden);
			AssertEquals(ZDecimal.Zero, invoiceLine.FeeCusCodes.GetFirstElementHaving(invoiceLine.US_TaxCode).CY_FeeAmount);
		}

		public void TestMergeDoesNotChangeIndicator()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "1020304010";
			tariff.UE_DateFrom = ZDateTime.Today.AddYears(-1);
			tariff.UE_DateTo = ZDateTime.Today.AddYears(1);
			tariff.UE_PGACodes = "   AQ2";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_CertifyCargoRelease = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "1020304010";
			invoiceLine.JI_LinePrice = 1m;
			AssertEquals("invoiceLine.DoesRequireAPHIS", true, invoiceLine.PGARequirementIndicator.RequireAPHIS);
			invoiceLine.US_APHISInd = OGAIndicatorList.Codes.Disclaimed;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("invoiceLine.US_APHISInd should not be changed", OGAIndicatorList.Codes.Disclaimed, invoiceLine.US_APHISInd);
		}

		public void TestMergeWhenUsersChangeMessageTypes()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_SupTariff = "98";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var ftzEntry = declaration.ActiveEntryHeaders.FTZEntry;
			AssertNotNull(ftzEntry);

			AssertEquals(2, ftzEntry.MergedLines.Count);

			var supLine = invoiceLine.GetEntryLineFor(CusEntryHeaderMessageTypeList.Codes.ForeignTradeZone, true);
			var normalLine = invoiceLine.CusEntryLine;
			AssertEquals(true, supLine.US_SupLine);
			AssertEquals(supLine, normalLine.ParentLine);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertNull(declaration.ActiveEntryHeaders.FTZEntry);
			Assert(ftzEntry.IsDeleted);

			AssertNotNull(declaration.ActiveEntryHeaders.EntrySummaryEntry);
		}

		public void TestDeletingPGADataOnFirstMerging()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = ZString.Empty;
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			AssertEquals(false, declaration.IsACECargoCertificationMode);
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var dot = invoiceLine.DOTs.AddNew();
			var atf = invoiceLine.ATFLines.AddNew();

			declaration.DoMerge();
			AssertEquals(0, declaration.CustomsEntryHeaders.Count);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACS;
			AssertEquals(false, declaration.IsACECargoCertificationMode);
			AssertEquals(false, dot.IsDeleted);
			AssertEquals(true, atf.IsDeleted);
			atf = invoiceLine.ATFLines.AddNew();

			declaration.DoMerge();
			AssertEquals(1, declaration.CustomsEntryHeaders.Count);
			AssertEquals(1, invoiceLine.DOTs.Count);
			AssertEquals(false, dot.IsDeleted);
			AssertEquals(0, invoiceLine.ATFLines.Count);
			AssertEquals(true, atf.IsDeleted);

			atf = invoiceLine.ATFLines.AddNew();
			declaration.DoMerge();
			AssertEquals(1, declaration.CustomsEntryHeaders.Count);
			AssertEquals(1, invoiceLine.DOTs.Count);
			AssertEquals(false, dot.IsDeleted);
			AssertEquals(0, invoiceLine.ATFLines.Count);
			AssertEquals(true, atf.IsDeleted);

			declaration.CustomsEntryHeaders.RemoveAndDeleteAll();
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			AssertEquals(true, declaration.IsACECargoCertificationMode);
			AssertEquals(0, invoiceLine.DOTs.Count);
			AssertEquals(true, dot.IsDeleted);
			atf = invoiceLine.ATFLines.AddNew();
			dot = invoiceLine.DOTs.AddNew();

			declaration.DoMerge();
			AssertEquals(2, declaration.CustomsEntryHeaders.Count);
			AssertEquals(0, invoiceLine.DOTs.Count);
			AssertEquals(true, dot.IsDeleted);
			AssertEquals(1, invoiceLine.ATFLines.Count);
			AssertEquals(false, atf.IsDeleted);
		}

		[TestDate(2017, 12, 1)]
		public void TestUS_PayableMPFWith98Values()
		{
			new FeeCalculationHelperTest().PrepareFeeAndTexData();
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 1m;

			JobComInvoiceLine invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.US_98ValueInvCurr = 1m;
			invoiceLine2.US_98GoodsValue = 3m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertEquals(25m, invoiceLine.US_PayableMPF);
			AssertEquals(0m, invoiceLine2.US_PayableMPF);
		}

		public void TestUS_CustomsValueExcludes98GoodsValue()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_SupTariff = "9902";
			invoiceLine.US_98GoodsValue = 2300m;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals(0m, invoiceLine.US_CustomsValue);

			invoiceLine.JI_LinePrice = 10000m;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals(10000m, invoiceLine.US_CustomsValue);

			invoiceLine.US_98GoodsValue = 20m;
			invoiceLine.US_98ValueInvCurr = 10m;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals(10000m, invoiceLine.US_CustomsValue);
		}

		public void TestMergeWhenMessageTypeChangesFromIMPToEXP()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";

			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			CusEntryHeader ensEntry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			AssertNotNull(ensEntry);

			Factory.Save();

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.Invoices[0].RunPreSaveValidation();
			AssertNoErrors("PreCondition", declaration);

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("ENS entry is deleted while merging", true, ensEntry.IsDeleted);

			AssertEquals("EXP entry should have been created", 1, declaration.ActiveEntryHeaders.Count);
			AssertEquals("EXP entry should have been created", CusEntryHeaderMessageTypeList.Codes.Export, declaration.ActiveEntryHeaders[0].CH_MessageType);
		}

		public void TestMergeWhenStrategyBecomesDeactivated()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertEquals("One ENS entry is created", 1, declaration.CustomsEntryHeaders.Count);

			declaration.US_EnableCRL = true;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertEquals("Two entries (ENS and CRL) created", 2, declaration.CustomsEntryHeaders.Count);
			AssertEquals("Two entries (ENS and CRL) created", 2, declaration.ActiveEntryHeaders.Count);
			AssertNotNull(declaration.ActiveEntryHeaders.GetEntryWithType(ImportMessageStatusList.MessageType.EntrySummary));
			AssertNotNull(declaration.ActiveEntryHeaders.GetEntryWithType(ImportMessageStatusList.MessageType.CargoRelease));

			declaration.US_EnableENS = false;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("One entry created", 1, declaration.CustomsEntryHeaders.Count);
			AssertNotNull(declaration.ActiveEntryHeaders.GetEntryWithType(ImportMessageStatusList.MessageType.CargoRelease));
		}

		public void TestMergeWithMultiStrategies()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EnableCRL = true;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "789345879";

			JobComInvoiceLine invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "789345879";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertEquals("merged into the same line", invoiceLine1.CusEntryLine, invoiceLine2.CusEntryLine);
			AssertEquals("Two invoice lines", 2, invoiceLine1.CusEntryLine.InvoiceLines.Count);
			AssertEquals("contains invoiceline1", true, invoiceLine1.CusEntryLine.InvoiceLines.Contains(invoiceLine1));
			AssertEquals("contains invoiceline2", true, invoiceLine1.CusEntryLine.InvoiceLines.Contains(invoiceLine2));

			CusEntryLine crlLine1 = invoiceLine1.GetEntryLineFor(CusEntryHeaderMessageTypeList.Codes.CargoRelease, false);
			CusEntryLine crlLine2 = invoiceLine2.GetEntryLineFor(CusEntryHeaderMessageTypeList.Codes.CargoRelease, false);
			AssertEquals("Mergd into the same line for CRL", crlLine1, crlLine2);
			AssertEquals(true, crlLine1.InvoiceLines.Contains(invoiceLine1));
			AssertEquals(true, crlLine1.InvoiceLines.Contains(invoiceLine2));
		}

		public void TestMergeWithDifferentIncoTerms()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_IncoTerm = TermsOfDeliveryList.Codes.FOB;
			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_IncoTerm = TermsOfDeliveryList.Codes.DDP;

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "4202228100";
			invoiceLine.JI_LinePrice = 803.4m;

			var invoiceLine2 = invoice2.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "6211431091";
			invoiceLine2.JI_LinePrice = 315m;

			var merger = new LineMerger(declaration);
			merger.DoMerge();

			var entryHeader = declaration.CustomsEntryHeaders[0];
			var entryLine = entryHeader.MergedLines[0];
			AssertEquals("should be rounded", 803m, entryLine.CL_CustomsValue);
		}

		public void TestEntryNumberIsReAssignedToENSEntry()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = false;
			declaration.US_EnableCRL = true;
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000m;

			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 1000m;
			declaration.DoMerge();
			CusEntryHeader crlEntry = declaration.ActiveEntryHeaders[0];
			AssertEquals(true, crlEntry.IsRelatedToENSEntry);
			crlEntry.EntryNumber = "01000100";
			AssertEquals(declaration.PK, crlEntry.CusEntryNumber.CE_ParentID);
			AssertEquals(declaration.ENSEntryNumber, crlEntry.CusEntryNumber);

			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.DoMerge();
			AssertEquals(2, declaration.ActiveEntryHeaders.Count);
			CusEntryHeader ensEntry = declaration.ActiveEntryHeaders.GetEntryWithType(ImportMessageStatusList.MessageType.EntrySummary)[0];
			AssertEquals(false, ensEntry.IsRelatedToENSEntry);
			AssertEquals(true, crlEntry.IsRelatedToENSEntry);
			AssertEquals("01000100", ensEntry.EntryNumber);
			AssertEquals(ensEntry.EntryNumber, crlEntry.EntryNumber);
			AssertEquals(declaration.PK, crlEntry.CusEntryNumber.CE_ParentID);
			AssertEquals(declaration.ENSEntryNumber, crlEntry.CusEntryNumber);
		}

		[TestDate(2010, 07, 20, 10, 20, 30)]
		public void TestUS_XTNIsSetForExport()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.JE_DeclarationReference = "BEA000017592CHI2010";

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000m;

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 1000m;

			var notifier = new Customs.Business.SendsMessagesToCustomsShutterUpperer();

			var filer = new ExportEntryFilerID();
			filer.EntryFilerID = "356-76-2387";
			filer.EntryFilerIDType = "S";
			USCustomsDataRegistry.Instance.ExportEntryFilerID.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, filer);

			var filerOnBranchLevel = new ExportEntryFilerID();
			filerOnBranchLevel.EntryFilerID = "865-48-6268";
			filerOnBranchLevel.EntryFilerIDType = "S";
			USCustomsDataRegistry.Instance.ExportEntryFilerID.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, filerOnBranchLevel);

			declaration.DoMerge(notifier);
			AssertEquals(1, declaration.CustomsEntryHeaders.Count);
			var entry = declaration.CustomsEntryHeaders[0];
			AssertEquals("XTN", "865486268-E0010072010203000", entry.US_XTN);

			entry.US_XTN = ZString.Empty;
			((IRegistryItemInternals)USCustomsDataRegistry.Instance.ExportEntryFilerID).DeleteValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty);
			declaration.DoMerge(notifier);
			AssertEquals(1, declaration.CustomsEntryHeaders.Count);
			entry = declaration.CustomsEntryHeaders[0];
			AssertEquals("XTN", "356762387-E0010072010203000", entry.US_XTN);

			declaration.CustomsEntryHeaders.RemoveAndDeleteAll();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.DoMerge(notifier);
			AssertEquals(1, declaration.CustomsEntryHeaders.Count);
			entry = declaration.CustomsEntryHeaders[0];
			AssertEquals("XTN", ZString.Empty, entry.US_XTN);
		}

		[TestDate(2010, 07, 20, 10, 20, 30)]
		public void TestUS_XTNIsSetForExportStdJobBGMReference()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.JE_DeclarationReference = "B00123222";

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000m;

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 1000m;

			Customs.Business.SendsMessagesToCustomsShutterUpperer notifier = new Customs.Business.SendsMessagesToCustomsShutterUpperer();

			var filer = new ExportEntryFilerID();
			filer.EntryFilerID = "356-76-2387";
			filer.EntryFilerIDType = "S";
			USCustomsDataRegistry.Instance.ExportEntryFilerID.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, filer);

			var filerOnBranchLevel = new ExportEntryFilerID();
			filerOnBranchLevel.EntryFilerID = "865-48-6268";
			filerOnBranchLevel.EntryFilerIDType = "S";
			USCustomsDataRegistry.Instance.ExportEntryFilerID.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, filerOnBranchLevel);

			declaration.DoMerge(notifier);
			AssertEquals(1, declaration.CustomsEntryHeaders.Count);
			var entry = declaration.CustomsEntryHeaders[0];
			AssertEquals("XTN", "865486268-B00123222", entry.US_XTN);

			entry.US_XTN = ZString.Empty;
			((IRegistryItemInternals)USCustomsDataRegistry.Instance.ExportEntryFilerID).DeleteValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty);
			declaration.DoMerge(notifier);
			AssertEquals(1, declaration.CustomsEntryHeaders.Count);
			entry = declaration.CustomsEntryHeaders[0];
			AssertEquals("XTN", "356762387-B00123222", entry.US_XTN);

			declaration.CustomsEntryHeaders.RemoveAndDeleteAll();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EnableENS = true;
			declaration.DoMerge(notifier);
			AssertEquals(1, declaration.CustomsEntryHeaders.Count);
			entry = declaration.CustomsEntryHeaders[0];
			AssertEquals("XTN", ZString.Empty, entry.US_XTN);
		}

		public void TestCalculateFinalDestinationFromLineWithMaxValue()
		{
			DeclarationTestHelper.SetupBranchSpecificInBondNumberRanges();
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;

			declaration.US_EnableINB = true;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 2000m;
			invoiceLine1.US_DestinationState = "IL";

			JobComInvoiceLine invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 8000m;
			invoiceLine2.US_DestinationState = "AL";

			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();

			AssertEquals("PreCondition:Two entries", 2, declaration.CustomsEntryHeaders.Count);

			CusEntryHeader ensEntry = invoiceLine1.CusEntryLine.Header;
			AssertEquals("PreCondition:ENS entry", CusEntryHeaderMessageTypeList.Codes.EntrySummary, ensEntry.CH_MessageType);

			CusEntryHeader inbEntry = invoiceLine1.GetEntryLineFor(CusEntryHeaderMessageTypeList.Codes.InBond, false).Header;
			AssertEquals("PreCondition:INB entry", CusEntryHeaderMessageTypeList.Codes.InBond, inbEntry.CH_MessageType);

			AssertEquals("State is set for ens entry", "AL", ensEntry.US_DestinationState);
			AssertEquals("State is not set for inb entry", "", inbEntry.US_DestinationState);

			invoiceLine1.JI_LinePrice = 12000m;
			declaration.DoMerge();
			AssertEquals("State is updated", "IL", ensEntry.US_DestinationState);
		}

		public void TestRemoveAdditionalInvoiceLineLink()
		{
			DeclarationTestHelper.SetupBranchSpecificInBondNumberRanges();
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			var declaration = new DeclarationTestHelper().CreateSimpleImportDeclaration(Factory);
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EnableINB = true;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.DoMerge();
			AssertNoExceptionThrown(delegate
			{ Factory.Save(); });

			CusEntryHeader ensEntry = declaration.CustomsEntryHeaders.Find(new ZQuery(CusEntryHeaderSchema.CH_MessageType, CusEntryHeaderMessageTypeList.Codes.EntrySummary))[0];
			CusEntryHeader inbEntry = declaration.CustomsEntryHeaders.Find(new ZQuery(CusEntryHeaderSchema.CH_MessageType, CusEntryHeaderMessageTypeList.Codes.InBond))[0];

			AssertNotNull(ensEntry);
			AssertNotNull(inbEntry);

			inbEntry.CH_Status = ImportMessageStatusList.Codes.ClearDepartureOriginal;
			MQEDIMessage message = Factory.New<MQEDIMessage>();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			inbEntry.Messages.Add(message);
			AssertNoExceptionThrown(delegate
			{ Factory.Save(); });

			AssertEquals("PreCondition:HasBeenLodged", true, inbEntry.HasBeenLodgedAtCustoms);
			AssertNoExceptionThrown(delegate
			{ Factory.Save(); });

			CusEntryLine existingENSentryLine = ensEntry.MergedLines[0];

			declaration.US_EnableINB = false;
			bool remerged = declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer(false));

			Assert(!remerged);

			declaration.US_EnableINB = true;
			declaration.DoMerge();

			AssertEquals("Two entries still", 2, declaration.ActiveEntryHeaders.Count);
			AssertEquals(true, declaration.ActiveEntryHeaders.Contains(inbEntry));
			AssertEquals(true, declaration.ActiveEntryHeaders.Contains(ensEntry));

			inbEntry.CH_Status = ImportMessageStatusList.Codes.AwaitingDepartureWithdraw;
			AssertNoExceptionThrown(delegate
			{ Factory.Save(); });
			inbEntry.CH_Status = ImportMessageStatusList.Codes.ClearDepartureWithdraw;
			AssertNoExceptionThrown(delegate
			{ Factory.Save(); });
			AssertEquals("HasBeenWithdrwan", true, inbEntry.HasBeenWithdrawn);

			declaration.US_EnableINB = false;
			remerged = declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer(false));
			Assert(!remerged);
		}

		public void TestMergeWithMultipleImportMessagingsEnabled()
		{
			DeclarationTestHelper.SetupBranchSpecificInBondNumberRanges();
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			var declaration = new DeclarationTestHelper().CreateSimpleImportDeclaration(Factory);
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EnableINB = true;
			declaration.DoMerge();
			AssertNoExceptionThrown(delegate
			{ Factory.Save(); });

			ZQuery inBondQuery = new ZQuery(CusEntryHeaderSchema.CH_MessageType, CusEntryHeaderMessageTypeList.Codes.InBond);
			ZQuery entrySummaryQuery = new ZQuery(CusEntryHeaderSchema.CH_MessageType, CusEntryHeaderMessageTypeList.Codes.EntrySummary);

			AssertEquals("There should be three entries created", 2, declaration.ActiveEntryHeaders.Count);
			AssertEquals("CH_MessageType is set", 1, declaration.ActiveEntryHeaders.Find(inBondQuery).Length);
			AssertEquals("CH_MessageType is set", 1, declaration.ActiveEntryHeaders.Find(entrySummaryQuery).Length);

			JobComInvoiceLine line = declaration.InvoiceLines[0];
			AssertEquals("JI_CL should point to an entry line of entry summary", CusEntryHeaderMessageTypeList.Codes.EntrySummary, line.CusEntryLine.Header.CH_MessageType);
			AssertEquals("additional entry line link", 1, line.AdditionalEntryLineLinks.Count);
			AssertNotNull(line.AdditionalEntryLineLinks.GetEntryLineFor(CusEntryHeaderMessageTypeList.Codes.InBond));

			declaration.US_EnableENS = false;
			AssertEquals("US_EnableENS change should make another merge required", true, declaration.MergeManager.RequiresMergeBeforeSave);
			declaration.DoMerge();
			AssertEquals("ENS entry is deleted after merge is done", 1, declaration.ActiveEntryHeaders.Count);
			AssertEquals("ENS entry is deleted after merge is done", 1, declaration.CustomsEntryHeaders.Count);
			AssertEquals("CH_MessageType", 1, declaration.ActiveEntryHeaders.Find(inBondQuery).Length);
			AssertNoExceptionThrown(delegate
			{ Factory.Save(); });

			line = declaration.InvoiceLines[0];
			AssertEquals("JI_CL should point to an entry line of InBond", CusEntryHeaderMessageTypeList.Codes.InBond, line.CusEntryLine.Header.CH_MessageType);
			AssertEquals("line.AdditionalLink is cleared", 0, line.AdditionalEntryLineLinks.Count);

			CusEntryLine inbEntryLine = line.CusEntryLine;
			declaration.DoMerge();
			AssertEquals("Existing entry line is recycled", inbEntryLine, line.CusEntryLine);
		}

		public void TestMergeWithOneMessagingModeThenChangeTheMode()
		{
			DeclarationTestHelper.SetupBranchSpecificInBondNumberRanges();
			var declaration = new DeclarationTestHelper().CreateSimpleImportDeclaration(Factory);
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EnableENS = false;
			declaration.US_EntryType = "";
			declaration.US_EnableINB = true;
			declaration.DoMerge();

			AssertEquals("There should be one entry created", 1, declaration.ActiveEntryHeaders.Count);
			AssertEquals("CH_MessageType is set", 1, declaration.ActiveEntryHeaders.Find(new ZQuery(CusEntryHeaderSchema.CH_MessageType, CusEntryHeaderMessageTypeList.Codes.InBond)).Length);

			JobComInvoiceLine line = declaration.InvoiceLines[0];
			AssertEquals("JI_CL should point to an entry line of entry summary", CusEntryHeaderMessageTypeList.Codes.InBond, line.CusEntryLine.Header.CH_MessageType);
			AssertEquals("additional entry line link", 0, line.AdditionalEntryLineLinks.Count);

			declaration.US_EnableINB = false;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.DoMerge();
			AssertEquals("There should be one entry created", 1, declaration.ActiveEntryHeaders.Count);
			AssertEquals("CH_MessageType is set", 1, declaration.ActiveEntryHeaders.Find(new ZQuery(CusEntryHeaderSchema.CH_MessageType, CusEntryHeaderMessageTypeList.Codes.EntrySummary)).Length);
			line = declaration.InvoiceLines[0];
			AssertEquals("JI_CL should point to an entry line of entry summary", CusEntryHeaderMessageTypeList.Codes.EntrySummary, line.CusEntryLine.Header.CH_MessageType);
			AssertEquals("line.AdditionalLink is cleared", 0, line.AdditionalEntryLineLinks.Count);
		}

		[TestDate(2010, 07, 20)]
		public void TestUpdateShipmentNoAndStatusForExport()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "BEA000017592CHI2010";
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			var invoice = declaration.Invoices.AddNew();
			AssertNotEquals("PreCondition: invoice should not be hazardous", YesNoDefaultList.Codes.Yes, invoice.US_HazardousCargo);
			invoice.US_ImportEntryNo = "12345";
			invoice.JobComInvoiceLines.AddNew();

			invoice = declaration.Invoices.AddNew();
			AssertNotEquals("PreCondition: invoice should not be hazardous", YesNoDefaultList.Codes.Yes, invoice.US_HazardousCargo);
			invoice.US_ImportEntryNo = "12346";
			invoice.JobComInvoiceLines.AddNew();

			invoice = declaration.Invoices.AddNew();
			AssertNotEquals("PreCondition: invoice should not be hazardous", YesNoDefaultList.Codes.Yes, invoice.US_HazardousCargo);
			invoice.US_ImportEntryNo = "12347";
			invoice.JobComInvoiceLines.AddNew();
			Factory.Save();

			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			Factory.Save();

			var newFactory = new BusinessObjectFactory();

			AssertNull("There should be no Entry Header containing ref E0010072000000000", CusEntryHeader.LoadForBGMReference(newFactory, "E0010072000000000"));
			AssertNull("There should be no Entry Header containing ref E0010072000000001", CusEntryHeader.LoadForBGMReference(newFactory, "E0010072000000001"));
			AssertNull("There should be no Entry Header containing ref E0010072000000002", CusEntryHeader.LoadForBGMReference(newFactory, "E0010072000000002"));

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.DoMerge();
			Factory.Save();

			AssertUpdateShipmentNoAndStatus("E0010072000000000", AESDirectCustomsEntryStatus.Codes.NotSent, true);
			AssertUpdateShipmentNoAndStatus("E0010072000000001", AESDirectCustomsEntryStatus.Codes.NotSent, true);
			AssertUpdateShipmentNoAndStatus("E0010072000000002", AESDirectCustomsEntryStatus.Codes.NotSent, true);

			var entryHeader = (CusEntryHeader)CusEntryHeader.LoadForBGMReference(newFactory, "E0010072000000002");
			entryHeader.CH_Status = AESDirectCustomsEntryStatus.Codes.OriginalSEDClear;
			entryHeader.EntryNumber = "ITN432";
			newFactory.Save();
			declaration.US_HazardousCargo = YesNoDefaultList.Codes.Yes;
			declaration.DoMerge();
			AssertUpdateShipmentNoAndStatus("E0010072000000000", AESDirectCustomsEntryStatus.Codes.NotSent, true);
			AssertUpdateShipmentNoAndStatus("E0010072000000001", AESDirectCustomsEntryStatus.Codes.NotSent, true);
			AssertUpdateShipmentNoAndStatus("E0010072000000002", AESDirectCustomsEntryStatus.Codes.ReplacementSEDRequired, true);

			entryHeader = (CusEntryHeader)CusEntryHeader.LoadForBGMReference(Factory, "E0010072000000002");
			entryHeader.CH_Status = AESDirectCustomsEntryStatus.Codes.OriginalSEDClear;
			declaration.US_HazardousCargo = YesNoDefaultList.Codes.No;
			declaration.DoMerge();
			AssertUpdateShipmentNoAndStatus("E0010072000000000", AESDirectCustomsEntryStatus.Codes.NotSent, true);
			AssertUpdateShipmentNoAndStatus("E0010072000000001", AESDirectCustomsEntryStatus.Codes.NotSent, true);
			AssertUpdateShipmentNoAndStatus("E0010072000000002", AESDirectCustomsEntryStatus.Codes.OriginalSEDClear, false);
		}

		[TestDate(2010, 07, 20)]
		public void TestUpdateShipmentNoAndStatusForExportWithStdJobNumber()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "B000017592";
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			var invoice = declaration.Invoices.AddNew();
			AssertNotEquals("PreCondition: invoice should not be hazardous", YesNoDefaultList.Codes.Yes, invoice.US_HazardousCargo);
			invoice.US_ImportEntryNo = "12345";
			invoice.JobComInvoiceLines.AddNew();

			invoice = declaration.Invoices.AddNew();
			AssertNotEquals("PreCondition: invoice should not be hazardous", YesNoDefaultList.Codes.Yes, invoice.US_HazardousCargo);
			invoice.US_ImportEntryNo = "12346";
			invoice.JobComInvoiceLines.AddNew();

			invoice = declaration.Invoices.AddNew();
			AssertNotEquals("PreCondition: invoice should not be hazardous", YesNoDefaultList.Codes.Yes, invoice.US_HazardousCargo);
			invoice.US_ImportEntryNo = "12347";
			invoice.JobComInvoiceLines.AddNew();
			Factory.Save();

			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			Factory.Save();

			var newFactory = new BusinessObjectFactory();

			AssertNull("There should be no Entry Header containing ref B000017592", CusEntryHeader.LoadForBGMReference(newFactory, "B000017592"));
			AssertNull("There should be no Entry Header containing ref B00001759201", CusEntryHeader.LoadForBGMReference(newFactory, "B00001759201"));
			AssertNull("There should be no Entry Header containing ref B00001759202", CusEntryHeader.LoadForBGMReference(newFactory, "B00001759202"));

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.DoMerge();
			Factory.Save();

			AssertUpdateShipmentNoAndStatus("B000017592", AESDirectCustomsEntryStatus.Codes.NotSent, true);
			AssertUpdateShipmentNoAndStatus("B00001759201", AESDirectCustomsEntryStatus.Codes.NotSent, true);
			AssertUpdateShipmentNoAndStatus("B00001759202", AESDirectCustomsEntryStatus.Codes.NotSent, true);

			var entryHeader = (CusEntryHeader)CusEntryHeader.LoadForBGMReference(newFactory, "B00001759202");
			entryHeader.CH_Status = AESDirectCustomsEntryStatus.Codes.OriginalSEDClear;
			entryHeader.EntryNumber = "ITN432";
			newFactory.Save();
			declaration.US_HazardousCargo = YesNoDefaultList.Codes.Yes;
			declaration.DoMerge();
			AssertUpdateShipmentNoAndStatus("B000017592", AESDirectCustomsEntryStatus.Codes.NotSent, true);
			AssertUpdateShipmentNoAndStatus("B00001759201", AESDirectCustomsEntryStatus.Codes.NotSent, true);
			AssertUpdateShipmentNoAndStatus("B00001759202", AESDirectCustomsEntryStatus.Codes.ReplacementSEDRequired, true);

			entryHeader = (CusEntryHeader)CusEntryHeader.LoadForBGMReference(Factory, "B00001759202");
			entryHeader.CH_Status = AESDirectCustomsEntryStatus.Codes.OriginalSEDClear;
			declaration.US_HazardousCargo = YesNoDefaultList.Codes.No;
			declaration.DoMerge();
			AssertUpdateShipmentNoAndStatus("B000017592", AESDirectCustomsEntryStatus.Codes.NotSent, true);
			AssertUpdateShipmentNoAndStatus("B00001759201", AESDirectCustomsEntryStatus.Codes.NotSent, true);
			AssertUpdateShipmentNoAndStatus("B00001759202", AESDirectCustomsEntryStatus.Codes.OriginalSEDClear, false);
		}

		[TestDate(2010, 07, 20)]
		public void TestUpdateShipmentNoAndStatusIsCalledBeforeDeclarationIsSaved()
		{
			var filer = new ExportEntryFilerID();
			filer.EntryFilerID = "123546870";
			filer.EntryFilerIDType = "D";
			USCustomsDataRegistry.Instance.ExportEntryFilerID.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, filer);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var invoice1 = declaration.Invoices.AddNew();
			invoice1.US_InbondType = InbondTypeList.Codes.IEForeignTradeZoneWithdrawal;
			var invoiceLin1 = invoice1.JobComInvoiceLines.AddNew();
			var invoice2 = declaration.Invoices.AddNew();
			invoice2.US_InbondType = InbondTypeList.Codes.MerchandiseNOTShippedInbond;
			var invoiceLin2 = invoice2.JobComInvoiceLines.AddNew();

			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			AssertEquals(ZString.Empty, declaration.JobNumber);
			declaration.DoMerge();
			AssertEquals(2, declaration.CustomsEntryHeaders.Count);
			var entry1 = declaration.CustomsEntryHeaders[0];
			var entry2 = declaration.CustomsEntryHeaders[1];
			AssertEquals(ZString.Empty, entry1.CH_BGMReference);
			AssertEquals(ZString.Empty, entry1.US_XTN);
			AssertEquals(ZString.Empty, entry2.CH_BGMReference);
			AssertEquals(ZString.Empty, entry2.US_XTN);
			Factory.Save();
			var jobNumber = declaration.JobNumber;
			AssertNotEquals(ZString.Empty, jobNumber);
			if (declaration.CustomsEntryHeaders[1].CH_BGMReference == jobNumber)
			{
				entry1 = declaration.CustomsEntryHeaders[1];
				entry2 = declaration.CustomsEntryHeaders[0];
			}
			AssertEquals(jobNumber, entry1.CH_BGMReference);
			AssertEquals("123546870-" + jobNumber, entry1.US_XTN);
			AssertEquals(jobNumber + "01", entry2.CH_BGMReference);
			AssertEquals("123546870-" + jobNumber + "01", entry2.US_XTN);
		}

		public void TestNoIrrelevantExceptionThrownForInBond()
		{
			DeclarationTestHelper.SetupBranchSpecificInBondNumberRanges();
			DeclarationTestHelper testHeper = new DeclarationTestHelper();
			JobDeclaration declaration = testHeper.FillInTestDataForInBondMessaging(Factory);
			declaration.US_EnableINB = true;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "1210200020";
			invoiceLine.JI_CustomsQuantity = 1200m;

			AssertNoExceptionThrown(delegate
			{ declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer(true)); });
		}

		public void TestMergeWatchRepair()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceAmount = 3406m;

			using (declaration.SuspendDefaultingSecondaryTariffLines())
			{
				JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
				invoiceLine.US_SupTariff = "9802004040";    // repairs
				invoiceLine.US_98GoodsValue = 3406m;

				invoiceLine.JI_Tariff = "9102111010";
				invoiceLine.JI_LinePrice = 5258m;
				invoiceLine.JI_CustomsQuantity = 1000m;

				JobComInvoiceLine secondRepairLine = invoiceLine.AddSecondaryInvoiceLine();
				secondRepairLine.US_SupTariff = "9802004040";   // repairs
				secondRepairLine.JI_Tariff = "9102111020";
				secondRepairLine.JI_LinePrice = 2619m;
				secondRepairLine.JI_CustomsQuantity = 1000m;

				JobComInvoiceLine thirdRepairLine = invoiceLine.AddSecondaryInvoiceLine();
				thirdRepairLine.US_SupTariff = "9802004040";    // repairs
				thirdRepairLine.JI_Tariff = "9102111030";
				thirdRepairLine.JI_LinePrice = 1344m;
				thirdRepairLine.JI_CustomsQuantity = 1000m;

				JobComInvoiceLine fourthRepairLine = invoiceLine.AddSecondaryInvoiceLine();
				fourthRepairLine.US_SupTariff = "9802004040";   // repairs
				fourthRepairLine.JI_Tariff = "9102111040";
				fourthRepairLine.JI_LinePrice = 204m;
				fourthRepairLine.JI_CustomsQuantity = 1000m;
			}

			LineMerger merger = new LineMerger(declaration);
			merger.DoMerge();

			AssertEquals(1, declaration.CustomsEntryHeaders.Count);
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders[0];
			AssertEquals(8, entryHeader.MergedLines.Count);

			string[] tariffsInOrder = { "9802004040", "9102111010", "9802004040", "9102111020", "9802004040", "9102111030", "9802004040", "9102111040" };
			for (int i = 0; i < 8; i++)
			{
				CusEntryLine entryLine = entryHeader.MergedLines[i];
				AssertEquals("Tariff Line : " + (i + 1).ToString(), tariffsInOrder[i], entryLine.CL_AdValoremTariff.ToString());
			}
		}

		public void TestNoExceptionIsThrownWhenNoMergeStategy()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = "";
			declaration.US_EnableINB = false;
			declaration.US_EnableCRL = false;
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceAmount = 1000m;
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "1210200020";
			invoiceLine.JI_LinePrice = 1000m;
			LineMerger merger = new LineMerger(declaration);
			AssertNoExceptionThrown("Should be ok to call merge when there is no merge stategy selected", delegate
			{ merger.DoMerge(); });
		}

		public void TestDutyCalculationForWatchRepair()
		{
			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "69696969";
			tariff.UE_DateFrom = ZDateTime.Today.AddYears(-1);
			tariff.UE_DateTo = ZDateTime.Today.AddYears(1);
			tariff.UE_Column1RateAdValorem = .25m;
			tariff.UE_Column1RateSpecific = .25m;
			tariff.UE_Unit1 = "KG";
			tariff.UE_DutyComputationCode = "1";

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceAmount = 3406m;
			invoiceHeader.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.US_SupTariff = "9802004040";    // repairs
			invoiceLine.US_98GoodsValue = 10000m;

			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			invoiceLine.JI_LinePrice = 2000m;
			invoiceLine.JI_CustomsQuantity = 1000m;

			LineMerger merger = new LineMerger(declaration);
			merger.DoMerge();

			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders[0];
			CusEntryLine entryLine = entryHeader.MergedLines[0];
			ICusEntryLine iEntryLine = entryLine;
			AssertEquals("Duty on line1", 0m, entryLine.DutyAmount);
			IEnumerator<ISecondaryTariffLine> enumerator = iEntryLine.SecondaryTariffLines.GetEnumerator();
			Assert("No secondary tariff lines", enumerator.MoveNext());
			ISecondaryTariffLine secondaryTariffLine = enumerator.Current;
			AssertEquals(41.66m, secondaryTariffLine.Duty);
		}

		public void TestBGMReferenceUpdatesWhenEntryCancelledAndResent()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_DeclarationReference = "B00001759";

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "Inv-01";
			invoice.US_ImportEntryNo = invoice.JZ_InvoiceNumber;
			JobComInvoiceLine line = invoice.InvoiceLines.AddNew();
			line.US_MarksAndNumbers = invoice.JZ_InvoiceNumber;

			Factory.Save();
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();

			CusEntryHeader entry1 = declaration.ActiveEntryHeaders[0];
			AssertEquals("entry1 CH_BGMReference", "B00001759", entry1.CH_BGMReference);    // First reference will not include suffix

			entry1.CH_Status = AESDirectCustomsEntryStatus.Codes.DeleteSEDClear;
			Factory.Save();
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			entry1 = declaration.ActiveEntryHeaders[0];
			AssertEquals("entry1 CH_BGMReference should have been incremented on resend after withdrawal", "B0000175901", entry1.CH_BGMReference);
		}

		public void TestBGMReferenceGreaterThan1296EntriesWithLength15OfJobNo()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_DeclarationReference = "B000017592CHI20";

			for (int i = 0; i < 1297; i++)
			{
				JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
				invoice.JZ_InvoiceNumber = "Inv-" + i.ToString();
				invoice.US_ImportEntryNo = invoice.JZ_InvoiceNumber;  // make merge key different
				JobComInvoiceLine line = invoice.InvoiceLines.AddNew();
				line.US_MarksAndNumbers = invoice.JZ_InvoiceNumber;
			}

			Factory.Save();
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();

			CusEntryHeader entry1 = declaration.ActiveEntryHeaders[0];
			CusEntryHeader entry2 = declaration.ActiveEntryHeaders[1];
			CusEntryHeader entry1296 = declaration.ActiveEntryHeaders[1295];
			AssertEquals("entry1 CH_BGMReference", "B000017592CHI20", entry1.CH_BGMReference);
			AssertEquals("entry2 CH_BGMReference", "B000017592CHI2001", entry2.CH_BGMReference);
			AssertEquals("entry1296 CH_BGMReference", "B000017592CHI20ZZ", entry1296.CH_BGMReference);

			CusEntryHeader entry1297 = declaration.ActiveEntryHeaders[1296];
			Assert("The prefix of entry1297's CH_BGMReference should not be job number", !entry1297.CH_BGMReference.StartsWith(declaration.JE_DeclarationReference));
			AssertEquals("The suffix of entry1297's CH_BGMReference should be 00", "00", entry1297.CH_BGMReference.Right(2));
		}

		public void TestBGMReferenceGreaterThan36EntriesWithLength16OfJobNo()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_DeclarationReference = "B000017592CHI201";

			for (int i = 0; i < 37; i++)    //create 37 entries
			{
				JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
				invoice.JZ_InvoiceNumber = "Inv-" + i.ToString();
				invoice.US_ImportEntryNo = invoice.JZ_InvoiceNumber;  // make merge key different
				JobComInvoiceLine line = invoice.InvoiceLines.AddNew();
				line.US_MarksAndNumbers = invoice.JZ_InvoiceNumber;
			}

			Factory.Save();
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();

			CusEntryHeader entry1 = declaration.ActiveEntryHeaders[0];
			CusEntryHeader entry36 = declaration.ActiveEntryHeaders[35];
			AssertEquals("entry1 CH_BGMReference", "B000017592CHI201", entry1.CH_BGMReference);
			AssertEquals("entry36 CH_BGMReference", "B000017592CHI201Z", entry36.CH_BGMReference);

			CusEntryHeader entry37 = declaration.ActiveEntryHeaders[36];
			Assert("The prefix of entry37's CH_BGMReference should not be the job number", !entry37.CH_BGMReference.StartsWith(declaration.JE_DeclarationReference));
			AssertEquals("The suffix of entry37's CH_BGMReference should be 00", "00", entry37.CH_BGMReference.Right(2));
		}

		[TestDate(2010, 07, 20, 13, 07, 12)]
		public void TestBGMReferenceGreaterThan1296EntriesWithLength17OfJobNo()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_DeclarationReference = "B000017592CHI2010";

			for (int i = 0; i < 11; i++)    //create 11 entries
			{
				JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
				invoice.JZ_InvoiceNumber = "Inv-" + i.ToString();
				invoice.US_ImportEntryNo = invoice.JZ_InvoiceNumber;  // make merge key different
				JobComInvoiceLine line = invoice.InvoiceLines.AddNew();
				line.US_MarksAndNumbers = invoice.JZ_InvoiceNumber;
			}

			Factory.Save();
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();

			CusEntryHeader entry1 = declaration.ActiveEntryHeaders[0];
			CusEntryHeader entry2 = declaration.ActiveEntryHeaders[1];
			CusEntryHeader entry3 = declaration.ActiveEntryHeaders[2];
			CusEntryHeader entry4 = declaration.ActiveEntryHeaders[3];
			CusEntryHeader entry5 = declaration.ActiveEntryHeaders[4];
			CusEntryHeader entry6 = declaration.ActiveEntryHeaders[5];
			CusEntryHeader entry7 = declaration.ActiveEntryHeaders[6];
			CusEntryHeader entry8 = declaration.ActiveEntryHeaders[7];
			CusEntryHeader entry9 = declaration.ActiveEntryHeaders[8];
			CusEntryHeader entry10 = declaration.ActiveEntryHeaders[9];
			CusEntryHeader entry11 = declaration.ActiveEntryHeaders[10];

			AssertEquals("entry1 CH_BGMReference", "B000017592CHI2010", entry1.CH_BGMReference);
			AssertEquals("entry2 CH_BGMReference", "E0010072013071200", entry2.CH_BGMReference);
			AssertEquals("entry3 CH_BGMReference", "E0010072013071201", entry3.CH_BGMReference);
			AssertEquals("entry4 CH_BGMReference", "E0010072013071202", entry4.CH_BGMReference);
			AssertEquals("entry5 CH_BGMReference", "E0010072013071203", entry5.CH_BGMReference);
			AssertEquals("entry6 CH_BGMReference", "E0010072013071204", entry6.CH_BGMReference);
			AssertEquals("entry7 CH_BGMReference", "E0010072013071205", entry7.CH_BGMReference);
			AssertEquals("entry8 CH_BGMReference", "E0010072013071206", entry8.CH_BGMReference);
			AssertEquals("entry9 CH_BGMReference", "E0010072013071207", entry9.CH_BGMReference);
			AssertEquals("entry10 CH_BGMReference", "E0010072013071208", entry10.CH_BGMReference);
			AssertEquals("entry11 CH_BGMReference", "E0010072013071209", entry11.CH_BGMReference);

			JobComInvoiceHeader invoice11 = declaration.Invoices.AddNew();
			invoice11.JZ_InvoiceNumber = "Inv-11";
			invoice11.US_ImportEntryNo = invoice11.JZ_InvoiceNumber;  // make merge key different
			JobComInvoiceLine invoice11line = invoice11.InvoiceLines.AddNew();
			invoice11line.US_MarksAndNumbers = invoice11.JZ_InvoiceNumber;
			Factory.Save();
			declaration.DoMerge();

			CusEntryHeader entry12 = declaration.ActiveEntryHeaders[11];
			AssertEquals("entry12 CH_BGMReference should generate into Alpa suffix range", "E001007201307120A", entry12.CH_BGMReference);

			for (int i = 12; i < 112; i++)      //create another 100 entries
			{
				JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
				invoice.JZ_InvoiceNumber = "Inv-" + i.ToString();
				invoice.US_ImportEntryNo = invoice.JZ_InvoiceNumber;  // make merge key different
				JobComInvoiceLine line = invoice.InvoiceLines.AddNew();
				line.US_MarksAndNumbers = invoice.JZ_InvoiceNumber;
			}

			Factory.Save();
			declaration.DoMerge();

			CusEntryHeader entry13 = declaration.ActiveEntryHeaders[12];
			CusEntryHeader entry14 = declaration.ActiveEntryHeaders[13];
			AssertEquals("entry13 CH_BGMReference should generate into Alpa suffix range", "E001007201307120B", entry13.CH_BGMReference);
			AssertEquals("entry14 CH_BGMReference should generate into Alpa suffix range", "E001007201307120C", entry14.CH_BGMReference);

			CusEntryHeader entry108 = declaration.ActiveEntryHeaders[107];
			CusEntryHeader entry109 = declaration.ActiveEntryHeaders[108];
			CusEntryHeader entry110 = declaration.ActiveEntryHeaders[109];
			CusEntryHeader entry111 = declaration.ActiveEntryHeaders[110];
			AssertEquals("entry108 CH_BGMReference should generate into Alpa suffix range", "E001007201307122Y", entry108.CH_BGMReference);
			AssertEquals("entry109 CH_BGMReference should generate into Alpa suffix range", "E001007201307122Z", entry109.CH_BGMReference);
			AssertEquals("entry110 CH_BGMReference should generate into Alpa suffix range", "E0010072013071230", entry110.CH_BGMReference);
			AssertEquals("entry111 CH_BGMReference should generate into Alpa suffix range", "E0010072013071231", entry111.CH_BGMReference);

			for (int i = 112; i < 1112; i++)    //create another 1000 entries
			{
				JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
				invoice.JZ_InvoiceNumber = "Inv-" + i.ToString();
				invoice.US_ImportEntryNo = invoice.JZ_InvoiceNumber;  // make merge key different
				JobComInvoiceLine line = invoice.InvoiceLines.AddNew();
				line.US_MarksAndNumbers = invoice.JZ_InvoiceNumber;
			}

			Factory.Save();
			declaration.DoMerge();

			CusEntryHeader entry361 = declaration.ActiveEntryHeaders[360];
			CusEntryHeader entry362 = declaration.ActiveEntryHeaders[361];
			AssertEquals("entry361 CH_BGMReference Suffix 1 should be last numeric suffix1 range", "E001007201307129Z", entry361.CH_BGMReference);
			AssertEquals("entry362 CH_BGMReference Suffix 1 should generate from numeric into Alpa suffix range", "E00100720130712A0", entry362.CH_BGMReference);

			CusEntryHeader entry1008 = declaration.ActiveEntryHeaders[1007];
			CusEntryHeader entry1009 = declaration.ActiveEntryHeaders[1008];
			CusEntryHeader entry1010 = declaration.ActiveEntryHeaders[1009];
			CusEntryHeader entry1011 = declaration.ActiveEntryHeaders[1010];
			AssertEquals("entry1008 CH_BGMReference should generate into Alpa suffix range", "E00100720130712RY", entry1008.CH_BGMReference);
			AssertEquals("entry1009 CH_BGMReference should generate into Alpa suffix range", "E00100720130712RZ", entry1009.CH_BGMReference);
			AssertEquals("entry1010 CH_BGMReference should generate into Alpa suffix range", "E00100720130712S0", entry1010.CH_BGMReference);
			AssertEquals("entry1011 CH_BGMReference should generate into Alpa suffix range", "E00100720130712S1", entry1011.CH_BGMReference);

			for (int i = 1112; i < 1297; i++)    //create another 185 entries
			{
				JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
				invoice.JZ_InvoiceNumber = "Inv-" + i.ToString();
				invoice.US_ImportEntryNo = invoice.JZ_InvoiceNumber;  // make merge key different
				JobComInvoiceLine line = invoice.InvoiceLines.AddNew();
				line.US_MarksAndNumbers = invoice.JZ_InvoiceNumber;
			}

			Factory.Save();
			declaration.DoMerge();

			CusEntryHeader entry1297 = declaration.ActiveEntryHeaders[1296];
			AssertEquals("entry1297 CH_BGMReference should generate into Alpa suffix range", "E00100720130712ZZ", entry1297.CH_BGMReference);
		}

		public void TestBGMReferenceEqual1296EntriesWithLength18OfJobNo()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_DeclarationReference = "B000017592CHI20100";

			for (int i = 0; i < 1297; i++)
			{
				JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
				invoice.JZ_InvoiceNumber = "Inv-" + i.ToString();
				invoice.US_ImportEntryNo = invoice.JZ_InvoiceNumber;  // make merge key different
				JobComInvoiceLine line = invoice.InvoiceLines.AddNew();
				line.US_MarksAndNumbers = invoice.JZ_InvoiceNumber;
			}

			Factory.Save();
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();

			CusEntryHeader entry1 = declaration.ActiveEntryHeaders[0];
			Assert("The prefix of entry1's CH_BGMReference should not be job number", !entry1.CH_BGMReference.StartsWith(declaration.JE_DeclarationReference));
			AssertEquals("The suffix of entry1's CH_BGMReference should be 00", "00", entry1.CH_BGMReference.Right(2));

			CusEntryHeader entry1296 = declaration.ActiveEntryHeaders[1295];
			Assert("The prefix of entry1296's CH_BGMReference should be equal to entry1's", entry1296.CH_BGMReference.Left(15) == entry1.CH_BGMReference.Left(15));
			AssertEquals("The suffix of entry1296's CH_BGMReference should be ZZ", "ZZ", entry1296.CH_BGMReference.Right(2));

			CusEntryHeader entry1297 = declaration.ActiveEntryHeaders[1296];
			Assert("The prefix of entry1297's CH_BGMReference should not be equal to entry1296's", entry1297.CH_BGMReference.Left(15) != entry1296.CH_BGMReference.Left(15));
			AssertEquals("The suffix of entry1297's CH_BGMReference should be 00", "00", entry1297.CH_BGMReference.Right(2));
		}

		protected override void SetUp()
		{
			base.SetUp();
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			DeclarationTestHelper.SetProcessingDistrictPortCode("8888");
		}

		void AssertUpdateShipmentNoAndStatus(string shipmentRef, string status, bool shouldBeReportToCustoms)
		{
			CusEntryHeader entryHeader = (CusEntryHeader)CusEntryHeader.LoadForBGMReference(Factory, shipmentRef);
			AssertNotNull("There should be an Entry Header containing " + shipmentRef, entryHeader);
			AssertEquals("CH_Status", status, entryHeader.CH_Status);
			AssertEquals("ShouldBeReportToCustoms", shouldBeReportToCustoms, entryHeader.US_ShouldBeReportToCustoms);
		}

		sealed class TestLineMerger : LineMerger
		{
			public TestLineMerger(JobDeclaration declaration)
				: base(declaration)
			{
			}

			public new Customs.Business.EntryCreationStrategy[] GetEntryCreationStrategies() => base.GetEntryCreationStrategies();
		}
	}
}
