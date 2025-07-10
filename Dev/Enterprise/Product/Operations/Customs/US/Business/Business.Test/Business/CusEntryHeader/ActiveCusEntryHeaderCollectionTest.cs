using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.US;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(ActiveCusEntryHeaderCollection))]
	sealed class ActiveCusEntryHeaderCollectionTest : Customs.Business.Testing.ActiveCusEntryHeaderCollectionTest<ActiveCusEntryHeaderCollection>
	{
		public void TestGetEntriesAboutToBeDiscarded()
		{
			DeclarationTestHelper.SetupBranchSpecificInBondNumberRanges();
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.US_EnableCRL = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.CR;
			declaration.US_InbondType = EntryTypeList.Codes.ImmediateExportation;
			declaration.US_EnableINB = true;
			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals(3, declaration.ActiveEntryHeaders.Count);
			declaration.US_InbondType = ZString.Empty;
			declaration.US_EnableINB = false;
			List<CusEntryHeader> entries = new List<CusEntryHeader>();
			entries.AddRange(declaration.ActiveEntryHeaders.GetEntriesAboutToBeDiscarded());
			AssertEquals("One entry about to be discarded", 1, entries.Count);
			AssertEquals(CusEntryHeaderMessageTypeList.Codes.InBond, entries[0].CH_MessageType);
			declaration.US_EnableENS = false;
			entries.Clear();
			entries.AddRange(declaration.ActiveEntryHeaders.GetEntriesAboutToBeDiscarded());
			AssertEquals("Two entries about to be discarded", 2, entries.Count);
			AssertEquals(CusEntryHeaderMessageTypeList.Codes.EntrySummary, entries[0].CH_MessageType);
			AssertEquals(CusEntryHeaderMessageTypeList.Codes.InBond, entries[1].CH_MessageType);
			declaration.JE_TransportMode = TransportTypeList.Codes.Rail;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.BCR;
			entries.Clear();
			entries.AddRange(declaration.ActiveEntryHeaders.GetEntriesAboutToBeDiscarded());
			AssertEquals("Three entries about to be discarded", 3, entries.Count);
			AssertEquals(CusEntryHeaderMessageTypeList.Codes.EntrySummary, entries[0].CH_MessageType);
			AssertEquals(CusEntryHeaderMessageTypeList.Codes.CargoRelease, entries[1].CH_MessageType);
			AssertEquals(CusEntryHeaderMessageTypeList.Codes.InBond, entries[2].CH_MessageType);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals(1, declaration.ActiveEntryHeaders.Count);
			declaration.JE_MessageType = JobMessageTypeList.Codes.ImportByExternalBroker;
			entries.Clear();
			entries.AddRange(declaration.ActiveEntryHeaders.GetEntriesAboutToBeDiscarded());
			AssertEquals("One entry about to be discarded", 1, entries.Count);
			AssertEquals(CusEntryHeaderMessageTypeList.Codes.Export, entries[0].CH_MessageType);
		}

		public void TestCargoReleaseEntry()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			CusEntryHeader ensEntry = declaration.CustomsEntryHeaders.AddNew();
			ensEntry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			CusEntryHeader crlEntry = declaration.CustomsEntryHeaders.AddNew();
			crlEntry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.BorderCargoRelease;
			AssertEquals(crlEntry, declaration.ActiveEntryHeaders.CargoReleaseEntry);
		}

		public void TestEntryNumbersAsCommaDelimitedStringNotThrowException()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			CusEntryHeader entry1 = declaration.ActiveEntryHeaders.AddNew();
			entry1.CH_BGMReference = "BGM123";
			entry1.EntryNumber = "ENS123";
			CusEntryHeader entry2 = declaration.ActiveEntryHeaders.AddNew();
			entry2.CH_BGMReference = "BGM456";
			entry2.EntryNumber = "ENS456";
			CusEntryNumber entryNum = Factory.New<CusEntryNumber>();
			entryNum.CE_EntryType = "ZZZ";
			entryNum.CE_ParentID = entry2.CusEntryNumber.CE_ParentID;
			entryNum.CE_ParentTable = entry2.CusEntryNumber.CE_ParentTable;
			Factory.Save();
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			JobDeclaration declarationReloaded = newFactory.Load<JobDeclaration>(declaration.PK);
			declarationReloaded.ActiveEntryHeaders.Sort(CusEntryHeader.Schema.CH_BGMReference);
			AssertEquals("DeclarationNumber", "ENS123, ENS456", declarationReloaded.ActiveEntryHeaders.EntryNumbersAsCommaDelimitedString);
		}

		public void TestHeaderHasEntryNumbersOrderByLineNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "FTZ";
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			var bill = declaration.Bills.AddNew();
			bill.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			bill.CU_BillNum = "0012365489";
			bill.US_UI_NKBillIssuerSCAC = "ABC";
			var houseBill = declaration.Bills.AddNew();
			houseBill.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			houseBill.CU_BillNum = "HOUSEBILL1";
			houseBill.US_UI_NKBillIssuerSCAC = "ABC";
			houseBill.CU_CU_ParentBill = bill.PK;
			var houseBill2 = declaration.Bills.AddNew();
			houseBill2.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			houseBill2.CU_BillNum = "HOUSEBILL2";
			houseBill2.US_UI_NKBillIssuerSCAC = "DEF";
			houseBill2.CU_CU_ParentBill = bill.PK;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoice.JZ_CU_RelatedHouseBill = bill.PK;
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 20.51m;
			invoiceLine.JI_BondedWhsQuantity = 5m;
			invoiceLine.JI_InvoiceUQ = "PCS";
			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_BondedWhsQuantity = 7m;
			invoiceLine2.JI_LinePrice = 33m;
			invoiceLine2.JI_InvoiceUQ = "NNM";
			var invoiceLine3 = declaration.InvoiceLines.AddNew();
			invoiceLine3.JI_LinePrice = 50.61m;
			invoiceLine3.JI_InvoiceUQ = "NO";
			invoiceLine3.JI_BondedWhsQuantity = 9m;
			var container1 = declaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "OON1111111";
			var container2 = declaration.CusContainers.AddNew();
			container2.CO_ContainerNumber = "OON2222222";
			var container3 = declaration.CusContainers.AddNew();
			container3.CO_ContainerNumber = "OON3333333";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			JobDeclaration declarationReloaded = newFactory.Load<JobDeclaration>(declaration.PK);
			AssertEquals(3, declarationReloaded.ActiveEntryHeaders.FTZEntry.MergedLines.Count);
			var entry = declarationReloaded.ActiveEntryHeaders.FTZEntry;
			AssertEquals("LineNumber 1.", (ZShort)1, entry.MergedLines[0].CL_LineNumber);
			AssertEquals("LineNumber 2.", (ZShort)2, entry.MergedLines[1].CL_LineNumber);
			AssertEquals("LineNumber 3.", (ZShort)3, entry.MergedLines[2].CL_LineNumber);
			entry.MergedLines[0].CL_LineNumber = 2;
			entry.MergedLines[1].CL_LineNumber = 3;
			entry.MergedLines[2].CL_LineNumber = 1;
			var sortedMergedLines = declarationReloaded.ActiveEntryHeaders.FindEntryLinesByMasterBill(bill.PK);
			ZShort num = 1;
			foreach (CusEntryLine oneLine in sortedMergedLines)
			{
				AssertEquals("FindEntryLinesByMasterBill is ordered by LineNumber.", num++, oneLine.CL_LineNumber);
			}
		}

		public void TestEntryHeaderWithENSEntryNumber()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = false;
			declaration.US_EnableCRL = true;
			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertNotNull(declaration.ActiveEntryHeaders.EntryHeaderWithENSEntryNumber);
			AssertEquals(CusEntryHeaderMessageTypeList.Codes.CargoRelease, declaration.ActiveEntryHeaders.EntryHeaderWithENSEntryNumber.CH_MessageType);
			declaration.US_EnableENS = true;
			declaration.US_EnableCRL = true;
			//Haven't re-merged again
			AssertEquals(CusEntryHeaderMessageTypeList.Codes.CargoRelease, declaration.ActiveEntryHeaders.EntryHeaderWithENSEntryNumber.CH_MessageType);
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals(CusEntryHeaderMessageTypeList.Codes.EntrySummary, declaration.ActiveEntryHeaders.EntryHeaderWithENSEntryNumber.CH_MessageType);
		}

		public void TestGetPKs()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			AssertEquals(0, declaration.ActiveEntryHeaders.GetPKs(CusEntryHeaderMessageTypeList.Codes.EntrySummary).Count);
			CusEntryHeader cargoRelease = declaration.ActiveEntryHeaders.AddNew();
			cargoRelease.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.CargoRelease;
			AssertEquals(0, declaration.ActiveEntryHeaders.GetPKs(CusEntryHeaderMessageTypeList.Codes.EntrySummary).Count);
			CusEntryHeader entrySummary = declaration.ActiveEntryHeaders.AddNew();
			entrySummary.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			AssertEquals(1, declaration.ActiveEntryHeaders.GetPKs(CusEntryHeaderMessageTypeList.Codes.EntrySummary).Count);
		}

		public void TestEntrySummaryEntry()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			AssertNull(declaration.ActiveEntryHeaders.EntrySummaryEntry);
			CusEntryHeader cRLentry = declaration.CustomsEntryHeaders.AddNew();
			cRLentry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.CargoRelease;
			AssertNull(declaration.ActiveEntryHeaders.EntrySummaryEntry);
			CusEntryHeader eNSentry = declaration.CustomsEntryHeaders.AddNew();
			eNSentry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			AssertNotNull(declaration.ActiveEntryHeaders.EntrySummaryEntry);
			AssertEquals("EntrySummaryEntry", eNSentry, declaration.ActiveEntryHeaders.EntrySummaryEntry);
		}

		public void TestInBondEntry()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			AssertNull(declaration.ActiveEntryHeaders.InBondEntry);
			CusEntryHeader crlEntry = declaration.CustomsEntryHeaders.AddNew();
			crlEntry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.CargoRelease;
			AssertNull(declaration.ActiveEntryHeaders.InBondEntry);
			CusEntryHeader inbondEntry = declaration.CustomsEntryHeaders.AddNew();
			inbondEntry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.InBond;
			AssertNotNull(declaration.ActiveEntryHeaders.InBondEntry);
			AssertEquals(inbondEntry, declaration.ActiveEntryHeaders.InBondEntry);
		}

		public void TestHasTemporaryImportationBond()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			AssertEquals(false, declaration.ActiveEntryHeaders.HasTemporaryImportationBond);
			CusEntryHeader ensEntry = declaration.CustomsEntryHeaders.AddNew();
			ensEntry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			declaration.US_EntryType = EntryTypeList.Codes.TemporaryImportationBond;
			AssertEquals(true, declaration.ActiveEntryHeaders.HasTemporaryImportationBond);
		}

		public void TestHasEntriesCargoReleaseBeingCertified()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			CusEntryHeader ensEntry = declaration.CustomsEntryHeaders.AddNew();
			ensEntry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			CusEntryHeader inbEntry = declaration.CustomsEntryHeaders.AddNew();
			inbEntry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.InBond;
			AssertEquals("HasEntriesCargoReleaseBeingCertified", false, declaration.ActiveEntryHeaders.HasEntriesCargoReleaseBeingCertified);
			ensEntry.US_CRLCertStatus = CargoReleaseCertificationStatusList.Codes.CertificationSentAckPending;
			AssertEquals("HasEntriesCargoReleaseBeingCertified", true, declaration.ActiveEntryHeaders.HasEntriesCargoReleaseBeingCertified);
			ensEntry.IsActive = false;
			declaration.ActiveEntryHeaders.Rebuild();
			AssertEquals("HasEntriesCargoReleaseBeingCertified", false, declaration.ActiveEntryHeaders.HasEntriesCargoReleaseBeingCertified);
		}

		public override void TestAreAllEntriesCleared()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			CusEntryHeader entry1 = declaration.CustomsEntryHeaders.AddNew();
			entry1.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.Export;
			Factory.Save();
			AssertEquals(false, declaration.ActiveEntryHeaders.AreAllEntriesCleared);
			entry1.CH_Status = AESDirectCustomsEntryStatus.Codes.OriginalSEDClear;
			Factory.Save();
			AssertEquals(true, declaration.ActiveEntryHeaders.AreAllEntriesCleared);
			CusEntryHeader entry2 = declaration.CustomsEntryHeaders.AddNew();
			entry2.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.Export;
			Factory.Save();
			AssertEquals(false, declaration.ActiveEntryHeaders.AreAllEntriesCleared);
			entry2.CH_Status = AESDirectCustomsEntryStatus.Codes.OriginalSEDClear;
			Factory.Save();
			AssertEquals(true, declaration.ActiveEntryHeaders.AreAllEntriesCleared);
			entry1.CH_Status = AESDirectCustomsEntryStatus.Codes.AwaitingDeleteResponse;
			Factory.Save();
			AssertEquals(false, declaration.ActiveEntryHeaders.AreAllEntriesCleared);
			entry1.CH_Status = AESDirectCustomsEntryStatus.Codes.DeleteSEDClear;
			Factory.Save();
			AssertEquals(false, declaration.ActiveEntryHeaders.AreAllEntriesCleared);
			entry1.CH_Status = AESDirectCustomsEntryStatus.Codes.AwaitingOriginalResponse;
			Factory.Save();
			AssertEquals(false, declaration.ActiveEntryHeaders.AreAllEntriesCleared);
			entry1.CH_Status = AESDirectCustomsEntryStatus.Codes.OriginalSEDClear;
			Factory.Save();
			AssertEquals(true, declaration.ActiveEntryHeaders.AreAllEntriesCleared);
		}

		public void TestHasAtLeastOneEntryWithActiveMessages()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableINB = true;
			Factory.Save();
			AssertEquals(false, declaration.ActiveEntryHeaders.HasAtLeastOneEntryWithActiveMessages);
			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			Factory.Save();
			AssertEquals(false, declaration.ActiveEntryHeaders.HasAtLeastOneEntryWithActiveMessages);
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.InBond;
			AssertEquals(false, declaration.ActiveEntryHeaders.HasAtLeastOneEntryWithActiveMessages);
			SetStatusAndSave(entry, ImportMessageStatusList.Codes.ClearDepartureOriginal);
			AssertEquals(true, declaration.ActiveEntryHeaders.HasAtLeastOneEntryWithActiveMessages);
			CusEntryHeader entry2 = declaration.CustomsEntryHeaders.AddNew();
			entry2.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.InBond;
			AssertEquals(true, declaration.ActiveEntryHeaders.HasAtLeastOneEntryWithActiveMessages);
		}

		public void TestHasAtLeastOneEntryWaitingForResponse()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableINB = true;
			Factory.Save();
			AssertEquals(false, declaration.ActiveEntryHeaders.HaveAtLeastOneEntryWaitingForResponse);
			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			Factory.Save();
			AssertEquals(false, declaration.ActiveEntryHeaders.HaveAtLeastOneEntryWaitingForResponse);
			SetStatusAndSave(entry, ImportMessageStatusList.Codes.AwaitingDepartureAmendment);
			AssertEquals(true, declaration.ActiveEntryHeaders.HaveAtLeastOneEntryWaitingForResponse);
		}

		public void TestInBondNumber()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableINB = true;
			AssertEquals(true, declaration.IsInBond);
			AssertEquals("UniqueInBondNumber", "", declaration.ActiveEntryHeaders.InBondNumber);
			CusEntryHeader entry = declaration.ActiveEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.InBond;
			entry.EntryNumber = "1";
			AssertEquals("UniqueInBondNumber", "1", declaration.ActiveEntryHeaders.InBondNumber);
			CusEntryHeader entry2 = declaration.ActiveEntryHeaders.AddNew();
			entry2.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entry2.EntryNumber = "2";
			AssertEquals("UniqueInBondNumber", "1", declaration.ActiveEntryHeaders.InBondNumber);
		}

		public void TestFTZEntry()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "FTZ";
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertNotNull(declaration.ActiveEntryHeaders.FTZEntry);
		}

		public void TestEntySummaryEnteredValue()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			Factory.Save();
			AssertEquals(0m, declaration.ActiveEntryHeaders.EntySummaryEnteredValue);
			CusEntryHeader cargoRelease = declaration.ActiveEntryHeaders.AddNew();
			cargoRelease.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.CargoRelease;
			AssertEquals(0m, declaration.ActiveEntryHeaders.EntySummaryEnteredValue);
			CusEntryHeader entrySummary = declaration.ActiveEntryHeaders.AddNew();
			entrySummary.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "TST-INV1";
			invoiceHeader.JZ_InvoiceAmount = 24055.10m;
			invoiceHeader.JZ_RX_NKInvoice_Currency = "USD";
			invoiceHeader.JZ_RN_NKDefaultOrigin = "CN";
			invoiceHeader.JZ_IncoTerm = "FOB";
			JobComInvoiceLine invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.US_SupTariff = "98130030";
			invoiceLine1.JI_InvoiceQuantity = 6100m;
			invoiceLine1.JI_InvoiceUQ = "KG";
			invoiceLine1.JI_LinePrice = 24055m;
			invoiceLine1.JI_Tariff = "7326908587";
			invoiceLine1.JI_CustomsQuantity = 6100m;
			invoiceLine1.JI_CustomsUnitQty = "KG";
			Factory.Save();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("EntySummaryEnteredValue", 24055m, declaration.ActiveEntryHeaders.EntySummaryEnteredValue);
		}

		public void TestReconciliationEntry()
		{
			var reconDeclaration = new DeclarationTestHelper().GetDutiableReconDeclaration(Factory);
			AssertNotNull(reconDeclaration.ReconWrappedJobDeclaration.ActiveEntryHeaders.ReconciliationEntry);
		}

		public void TestSimplifiedEntry()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertNull(declaration.ActiveEntryHeaders.SimplifiedEntry);
			var entry = declaration.CustomsEntryHeaders.AddNew();
			AssertNull(declaration.ActiveEntryHeaders.SimplifiedEntry);
			var entry2 = declaration.CustomsEntryHeaders.AddNew();
			entry2.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.ACECargoRelease;
			AssertNotNull(declaration.ActiveEntryHeaders.SimplifiedEntry);
			AssertEquals("Simplified Entry", entry2, declaration.ActiveEntryHeaders.SimplifiedEntry);
		}

		protected override ActiveCusEntryHeaderCollection GetCollectionToTest() => new ActiveCusEntryHeaderCollection(Factory.New<JobDeclaration>());

		void SetStatusAndSave(CusEntryHeader entry, ZString status)
		{
			entry.CH_Status = status;
			entry.Factory.Save();
		}
	}
}
