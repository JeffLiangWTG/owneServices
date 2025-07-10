using System;
using System.Drawing;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;

namespace Enterprise.Customs.US.Business.MessageProcessors.Testing
{
	class CensusWarningMessageProcessorTest : ABIProcessorTest<CensusWarningMessageProcessor, AABIOutputA, AABIOutputB, AABIOutputY>
	{
		protected override void EndToEndCore()
		{
			var newBranch = Factory.New<GlbBranch>();
			newBranch.FillWithValidTestData();
			newBranch.GB_GC = GlbCompany.CurrentCompany.PK;

			var image1 = new Bitmap(1, 2);
			var image2 = new Bitmap(2, 1);
			SystemDataRegistry.Instance.HtmlEmailBannerImage.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, image1);
			SystemDataRegistry.Instance.HtmlEmailBannerImage.SetValue(Guid.Empty, newBranch.PK.ToGuid(), Guid.Empty, image2);

			JobDeclaration declaration = GetMergedDeclaration("00000063");
			declaration.JE_GB = newBranch.PK;

			MQEDIMessage outgoingmessage = GetMessageToProcess();
			outgoingmessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CensusWarningOverride;
			outgoingmessage.EM_ReceiveTransmit = MQEDIMessage.Direction.Transmit;
			outgoingmessage.EM_MessageText = MQEDIMessage.USEntryNumberPlaceHolder + " " + MQEDIMessage.MessageNumberPlaceHolder + " B";
			outgoingmessage.EM_LinkedObject = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			Factory.Save();

			outgoingmessage.EM_MessageNum = "~15000";

			var invoiceLine1 = declaration.InvoiceLines[0];
			invoiceLine1.CusEntryLine.US_CWOs = "27D";

			var invoiceLine2 = declaration.InvoiceLines[1];
			invoiceLine2.CusEntryLine.US_CWOs = "27C";

			MQEDIMessage message = GetMessageToProcess();
			message.EM_MessageNum = "~15000";
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CensusWarningOverrideResponse;
			message.EM_MessageText =
"B003902SV9CO                                               ~15000               " +
"CW03SV9  10000915  00227C12C01CENSUS WARN OVERRIDE ACCPTD                       " + // Acknowledging Line 2 CWO
"Y  3902SV9CO00001";

			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			new ABIIncomingMessageProcessor().ExecuteBatch();
			message.Reload();
			AssertEquals(MQEDIMessage.Status.Received, message.EM_Status);
			AssertEquals(declaration.ActiveEntryHeaders.EntrySummaryEntry, message.EM_LinkedObject);

			var factory = new CargoWise.EntityFramework.BusinessObjectFactory();
			var entryLine = factory.Load<CusEntryLine>(invoiceLine1.JI_CL);
			var entryLine2 = factory.Load<CusEntryLine>(invoiceLine2.JI_CL);

			AssertEquals(ImportMessageStatusList.Codes.ClearCensusWarningOverride, entryLine.Header.US_CWOStatus);
			AssertEquals("No more outstanding CWs", "", entryLine2.US_CWOs);
			AssertEquals("Accepted CWOs for missed out entry lines should be untouched", "27D", entryLine.US_CWOs);
			AssertNotNull("Subject should contain " + declaration.DeclarationReferenceAppendedByFormattedEntryNumber, Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject.Contains(declaration.DeclarationReferenceAppendedByFormattedEntryNumber)));

			var email = Enterprise.Environment.Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var banner = email.Attachments.Cast<ZArchitecture.Environment.AttachmentDef>().FirstOrDefault(x => x.DisplayName == "Banner.jpg");
			var image = new Bitmap(new System.IO.MemoryStream(banner.Data));
			AssertEquals(2, image.Width);
		}

		public void TestUpdateEntrySummaryStatus()
		{
			var declaration = GetMergedDeclaration("00000063");

			var outgoingmessage = GetMessageToProcess();
			outgoingmessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CensusWarningOverride;
			outgoingmessage.EM_ReceiveTransmit = MQEDIMessage.Direction.Transmit;
			outgoingmessage.EM_MessageText = MQEDIMessage.USEntryNumberPlaceHolder + " " + MQEDIMessage.MessageNumberPlaceHolder + " B";
			outgoingmessage.EM_LinkedObject = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			Factory.Save();

			outgoingmessage.EM_MessageNum = "~15000";

			var invoiceLine1 = declaration.InvoiceLines[0];
			invoiceLine1.CusEntryLine.US_CWOs = "27D";

			var invoiceLine2 = declaration.InvoiceLines[1];
			invoiceLine2.CusEntryLine.US_CWOs = "27C";

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			entry.CH_Status = ImportMessageStatusList.Codes.EntrySummaryReplaceAcceptedWithCensusWarnings;

			var message = GetMessageToProcess();
			message.EM_MessageNum = "~15000";
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CensusWarningOverrideResponse;
			message.EM_MessageText =
"B003902SV9CO                                               ~15000               " +
"CW03SV9  10000915  00227C12C01CENSUS WARN OVERRIDE ACCPTD                       " + // Acknowledging Line 2 CWO
"Y  3902SV9CO00001";

			Factory.Save();

			new ABIIncomingMessageProcessor().ExecuteBatch();
			message.Reload();

			var factory = new CargoWise.EntityFramework.BusinessObjectFactory();
			var entryLoaded = factory.Load<CusEntryHeader>(entry.PK);
			AssertEquals("As there is an entry line that still has a CW", ImportMessageStatusList.Codes.EntrySummaryReplaceAcceptedWithCensusWarnings, entryLoaded.CH_Status);

			outgoingmessage = GetMessageToProcess();
			outgoingmessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CensusWarningOverride;
			outgoingmessage.EM_ReceiveTransmit = MQEDIMessage.Direction.Transmit;
			outgoingmessage.EM_MessageText = MQEDIMessage.USEntryNumberPlaceHolder + " " + MQEDIMessage.MessageNumberPlaceHolder + " B";
			outgoingmessage.EM_LinkedObject = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			Factory.Save();

			outgoingmessage.EM_MessageNum = "~15001";
			message = GetMessageToProcess();
			message.EM_MessageNum = "~15001";
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CensusWarningOverrideResponse;
			message.EM_MessageText =
"B003902SV9CO                                               ~15000               " +
"CW03SV9  10000915  00127D12C01CENSUS WARN OVERRIDE ACCPTD                       " + // Acknowledging Line 2 CWO
"Y  3902SV9CO00001";

			Factory.Save();

			new ABIIncomingMessageProcessor().ExecuteBatch();
			message.Reload();

			factory = new CargoWise.EntityFramework.BusinessObjectFactory();
			entryLoaded = factory.Load<CusEntryHeader>(entry.PK);
			AssertEquals("All the CWs are cleared", ImportMessageStatusList.Codes.ClearEntrySummaryReplace, entryLoaded.CH_Status);
		}

		public void TestProcessWhenThereIsSecondaryLine()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec.US_EnableENS = true;
			dec.US_EntryFilerCode = "XJ5";
			dec.JE_MergeBy = MasterFiles.Business.OrgConstants.MergeInvoiceLines.NotMerge;

			dec.Invoices.AddNew();
			var invoiceLine = dec.InvoiceLines.AddNew();
			invoiceLine.US_SupTariff = "9802.00.6000";
			invoiceLine.JI_Tariff = "9032.89.6015";

			var invoiceLine2 = dec.InvoiceLines.AddNew();
			invoiceLine2.US_SupTariff = "9802.00.6000";
			invoiceLine2.JI_Tariff = "9032.89.6015";

			dec.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			invoiceLine.CusEntryLine.US_CWOs = "27C";
			invoiceLine2.CusEntryLine.US_CWOs = "27C";

			MQEDIMessage outgoingmessage = GetMessageToProcess();
			outgoingmessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CensusWarningOverride;
			outgoingmessage.EM_ReceiveTransmit = MQEDIMessage.Direction.Transmit;
			outgoingmessage.EM_MessageText = MQEDIMessage.USEntryNumberPlaceHolder + " " + MQEDIMessage.MessageNumberPlaceHolder + " B";
			outgoingmessage.EM_LinkedObject = dec.ActiveEntryHeaders.EntrySummaryEntry;
			Factory.Save();

			outgoingmessage.EM_MessageNum = "~15000";

			MQEDIMessage message = GetMessageToProcess();
			message.EM_MessageNum = "~15000";
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CensusWarningOverrideResponse;
			message.EM_MessageText =
"B003902SV9CO                                               ~15000               " +
"CW03SV9  10000949  00227C13C01CENSUS WARN OVERRIDE ACCPTD                       " +
"Y  3902SV9CO00001";

			Factory.Save();

			new ABIIncomingMessageProcessor().ExecuteBatch();
			message.Reload();
			AssertEquals(MQEDIMessage.Status.Received, message.EM_Status);

			var factory = new CargoWise.EntityFramework.BusinessObjectFactory();
			var entryLoaded = factory.Load<CusEntryHeader>(dec.ActiveEntryHeaders.EntrySummaryEntry.PK);
			var entryLine1 = factory.Load<CusEntryLine>(invoiceLine.JI_CL);
			var entryLine2 = factory.Load<CusEntryLine>(invoiceLine2.JI_CL);
			AssertEquals("Outstanding CWs", "27C", entryLine1.US_CWOs);
			AssertEquals("No more outstanding CWs", "", entryLine2.US_CWOs);
		}

		JobDeclaration GetMergedDeclaration(ZString entryNumber)
		{
			JobDeclaration dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec.US_EnableENS = true;
			dec.US_EntryFilerCode = "XJ5";
			dec.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;
			dec.JE_MergeBy = MasterFiles.Business.OrgConstants.MergeInvoiceLines.NotMerge;
			dec.ImportEntryNumber = entryNumber;

			dec.Invoices.AddNew();
			dec.InvoiceLines.AddNew();
			dec.InvoiceLines.AddNew();

			dec.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			Factory.Save();

			return dec;
		}

		MQEDIMessage GetMessageToProcess()
		{
			MQEDIMessage message = Factory.New<MQEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = EDIMessage.Status.Queued;
			return message;
		}

		protected override ZInt EmailsExpectedAtCompletionOfEndToEndTest
		{
			get { return 1; }
		}
	}
}
