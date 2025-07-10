using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.US;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class EntryLineInvoiceLineLinkCalculatorTest : TestCaseWithFactory
	{
		public void TestMergeForBorderCargoReleaseWithoutEnablingENS()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EnableENS = false;
			declaration.US_EnableCRL = true;
			declaration.JE_TransportMode = TransportTypeList.Codes.Truck;
			declaration.US_EntryFilerCode = "XJ5";

			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			CusEntryHeader crlEntry = declaration.ActiveEntryHeaders.CargoReleaseEntry;
			AssertNotNull(crlEntry);

			AssertEquals("Merged Lines", 1, crlEntry.MergedLines.Count);
			AssertEquals("Should contain one invoice line", 1, crlEntry.MergedLines[0].InvoiceLines.Count);
		}

		public void TestMergeForCargoReleaseAndHasTrasactionWIthoutEnablingENS()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EnableENS = false;
			declaration.US_EnableCRL = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			var invoiceLine2 = declaration.InvoiceLines.AddNew();

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var crlEntry = declaration.ActiveEntryHeaders.CargoReleaseEntry;
			AssertNotNull(crlEntry);

			AssertEquals("Merged Lines", 1, crlEntry.MergedLines.Count);
			AssertEquals("Should contain one invoice line", 2, crlEntry.MergedLines[0].InvoiceLines.Count);

			AssertEquals(0, invoiceLine.AdditionalEntryLineLinks.Count);
			AssertEquals(0, invoiceLine2.AdditionalEntryLineLinks.Count);

			crlEntry.CH_Status = ImportMessageStatusList.Codes.ClearCargoReleaseOriginal;
			Factory.Save();

			var declarationLoaded = new BusinessObjectFactory().Load<JobDeclaration>(declaration.PK);
			declarationLoaded.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			declarationLoaded.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var line = declarationLoaded.InvoiceLines[0];
			var line2 = declarationLoaded.InvoiceLines[1];

			var entryLine1 = line.CusEntryLine;
			var entryLine2 = line2.CusEntryLine;

			AssertNotEquals(entryLine1, entryLine2);

			AssertEquals(0, line.AdditionalEntryLineLinks.Count);
			AssertEquals(0, line2.AdditionalEntryLineLinks.Count);

			declarationLoaded.US_EnableENS = true;
			declarationLoaded.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertEquals("existing entry line is still used", entryLine1, line.GetEntryLineFor(CusEntryHeaderMessageTypeList.Codes.CargoRelease, false));
			AssertEquals("existing entry line is still used", entryLine2, line2.GetEntryLineFor(CusEntryHeaderMessageTypeList.Codes.CargoRelease, false));
		}

		public void TestStartWithInBondThenENSenabledWhereInBondEntryNumberAllocatedWithoutMessagingIsDone()
		{
			DeclarationTestHelper.SetupBranchSpecificInBondNumberRanges();
			var declaration = new DeclarationTestHelper().CreateSimpleImportDeclaration(Factory);
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EnableENS = false;
			declaration.US_EnableINB = true;
			declaration.US_EntryType = "";
			declaration.DoMerge();
			AssertEquals("one entry is generated", 1, declaration.CustomsEntryHeaders.Count);
			Factory.Save();

			CusEntryHeader inbEntry = declaration.CustomsEntryHeaders.Find(new ZQuery(CusEntryHeaderSchema.CH_MessageType, CusEntryHeaderMessageTypeList.Codes.InBond))[0];
			AssertEquals("EntryNumber allocated", false, inbEntry.EntryNumber.IsEmpty);

			CusEntryLine inbEntryLine = inbEntry.MergedLines[0];
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.DoMerge();
			AssertEquals("two entries are generated", 2, declaration.CustomsEntryHeaders.Count);
			AssertEquals("has inbEntry", true, declaration.CustomsEntryHeaders.Contains(inbEntry));
			AssertEquals("INB entry's CH_MessageType", CusEntryHeaderMessageTypeList.Codes.InBond, inbEntry.CH_MessageType);

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines[0];
			AssertEquals("JI_CL is a link for entry summary", CusEntryHeaderMessageTypeList.Codes.EntrySummary, invoiceLine.CusEntryLine.Header.CH_MessageType);
			AssertEquals("one additional link", inbEntryLine, invoiceLine.GetEntryLineFor(CusEntryHeaderMessageTypeList.Codes.InBond, false));
		}

		public void TestStartedWithInBondOnlyThenENS()
		{
			DeclarationTestHelper.SetupBranchSpecificInBondNumberRanges();
			var declaration = new DeclarationTestHelper().CreateSimpleImportDeclaration(Factory);
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EnableENS = false;
			declaration.US_EnableINB = true;
			declaration.US_EntryType = "";
			declaration.DoMerge();
			AssertEquals("one entry is generated", 1, declaration.CustomsEntryHeaders.Count);
			Factory.Save();

			CusEntryHeader inbEntry = declaration.CustomsEntryHeaders.Find(new ZQuery(CusEntryHeaderSchema.CH_MessageType, CusEntryHeaderMessageTypeList.Codes.InBond))[0];
			inbEntry.CH_Status = ImportMessageStatusList.Codes.ClearDepartureOriginal;
			MQEDIMessage message = Factory.New<MQEDIMessage>();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			inbEntry.Messages.Add(message);

			CusEntryLine inbEntryLine = inbEntry.MergedLines[0];
			AssertNoExceptionThrown(() => Factory.Save());
			AssertEquals("HasBeenLodged", true, inbEntry.HasBeenLodgedAtCustoms);

			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.DoMerge();
			AssertEquals("two entries are generated", 2, declaration.CustomsEntryHeaders.Count);
			AssertEquals("has inbEntry", true, declaration.CustomsEntryHeaders.Contains(inbEntry));
			AssertEquals("INB entry's CH_MessageType", CusEntryHeaderMessageTypeList.Codes.InBond, inbEntry.CH_MessageType);

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines[0];
			AssertEquals("JI_CL is a link for entry summary", CusEntryHeaderMessageTypeList.Codes.EntrySummary, invoiceLine.CusEntryLine.Header.CH_MessageType);
			AssertEquals("one additional link", inbEntryLine, invoiceLine.GetEntryLineFor(CusEntryHeaderMessageTypeList.Codes.InBond, false));
		}

		public void TestInBondEntryHasBeenLodgedAndENSDisabled()
		{
			DeclarationTestHelper.SetupBranchSpecificInBondNumberRanges();
			var declaration = new DeclarationTestHelper().CreateSimpleImportDeclaration(Factory);
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EnableINB = true;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.DoMerge();

			AssertEquals("two entries created", 2, declaration.CustomsEntryHeaders.Count);
			AssertNoExceptionThrown(() => Factory.Save());

			CusEntryHeader inbEntry = declaration.CustomsEntryHeaders.Find(new ZQuery(CusEntryHeaderSchema.CH_MessageType, CusEntryHeaderMessageTypeList.Codes.InBond))[0];
			inbEntry.CH_Status = ImportMessageStatusList.Codes.ClearDepartureOriginal;
			MQEDIMessage message = Factory.New<MQEDIMessage>();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			inbEntry.Messages.Add(message);

			AssertNoExceptionThrown(() => Factory.Save());
			AssertEquals("HasBeenLodged", true, inbEntry.HasBeenLodgedAtCustoms);

			CusEntryLine inbEntryLine = declaration.InvoiceLines[0].GetEntryLineFor(CusEntryHeaderMessageTypeList.Codes.InBond, false);

			declaration.US_EnableENS = false;
			declaration.DoMerge();

			AssertEquals("JI_CL points to the existing INB entry line", inbEntryLine, declaration.InvoiceLines[0].CusEntryLine);
			AssertEquals("No additional line link", 0, declaration.InvoiceLines[0].AdditionalEntryLineLinks.Count);
			AssertNoExceptionThrown(() => Factory.Save());
		}

		public void TestInBondEntryHasBeenLodgedAndENSWithdrawn()
		{
			DeclarationTestHelper.SetupBranchSpecificInBondNumberRanges();
			var declaration = new DeclarationTestHelper().CreateSimpleImportDeclaration(Factory);
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EnableINB = true;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.DoMerge();

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines[0];

			AssertEquals("two entries created", 2, declaration.CustomsEntryHeaders.Count);
			AssertNoExceptionThrown(() => Factory.Save());

			CusEntryHeader inbEntry = declaration.CustomsEntryHeaders.Find(new ZQuery(CusEntryHeaderSchema.CH_MessageType, CusEntryHeaderMessageTypeList.Codes.InBond))[0];
			inbEntry.CH_Status = ImportMessageStatusList.Codes.ClearDepartureOriginal;
			MQEDIMessage message = Factory.New<MQEDIMessage>();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			inbEntry.Messages.Add(message);

			CusEntryHeader ensEntry = declaration.CustomsEntryHeaders.Find(new ZQuery(CusEntryHeaderSchema.CH_MessageType, CusEntryHeaderMessageTypeList.Codes.EntrySummary))[0];
			ensEntry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
			message = Factory.New<MQEDIMessage>();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			ensEntry.Messages.Add(message);

			AssertNoExceptionThrown(() => Factory.Save());
			AssertEquals("HasBeenLodged", true, inbEntry.HasBeenLodgedAtCustoms);
			AssertEquals("HasBeenLodged", true, ensEntry.HasBeenLodgedAtCustoms);

			ensEntry.CH_Status = ImportMessageStatusList.Codes.AwaitingEntrySummaryDelete;
			ensEntry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryDelete;
			AssertEquals("HasBeenWithdrawn", true, ensEntry.HasBeenWithdrawn);

			declaration.US_EnableENS = false;

			declaration.MessageInitiator = null;
			bool successful = declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer(false));

			Assert("Should not be able to merge", !successful);
		}

		public void TestMergeForSimplifiedEntryWithoutEnablingENS()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = false;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableCRL = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.SE;

			declaration.Invoices.AddNew();
			var invoiceline = declaration.InvoiceLines.AddNew();
			var manufacturer = Factory.New<OrgHeader>();
			manufacturer.OH_FullName = "Manufacturer";
			manufacturer.OH_Code = "MAN" + new Random().Next(1000000).ToString();
			var address = manufacturer.Addresses.AddNew();
			address.OA_Address1 = "Test man for line";
			address.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "FR034FREQU6LBH");
			invoiceline.JI_OA_ManufacturerAddress = address.PK;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entry = declaration.ActiveEntryHeaders.SimplifiedEntry;
			AssertNotNull(entry);

			AssertEquals("Merged Lines", 1, entry.MergedLines.Count);
			AssertEquals("Should contain one invoice line", 1, entry.MergedLines[0].InvoiceLines.Count);
			AssertEquals("This should not be newly created empty Random Line", address.PK, entry.MergedLines[0].RandomLine.JI_OA_ManufacturerAddress);
		}

		protected override void SetUp()
		{
			base.SetUp();
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
		}
	}
}
