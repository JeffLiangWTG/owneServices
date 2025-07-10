using System;
using System.Data;
using System.Net;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.US.Business.MessageProcessors.Testing
{
	sealed class AESCommodityShipmentWarningReminderProcessorTest : TestCaseWithFactory
	{
		public void TestEndToEnd()
		{
			USCustomsDataRegistry.Instance.AESSendNotificationsItem.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Core.Constants.EmailTo.StaffMember);
			outgoingMessage.EM_SendWithMessageErrors = true;
			incomingMessage.EM_MessageText =
"B  60612456712E          US EXPORTER NAME                                       " +
"SC1N11AUCAUNKNS40002509        ABAI YUN HE               60267270420081101 N    " +
"ES1066  F FILING OPT IND MUST BE 2 OR 4                                         " +
"ES1970 RF SHIPMENT REJECTED; RESOLVE & RETRANSMIT                               " +
"Y  60612456712E          US EXPORTER NAME";
			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			AESTIRIncomingMessageProcessor processor = new AESTIRIncomingMessageProcessor();
			processor.Logger = new BatchProcessor.LoggingInformation();
			processor.ExecuteBatch();

			incomingMessage.Reload();
			entry.Reload();
			entry.Messages.Load();
			AssertEquals(EDIMessage.Status.Received, incomingMessage.EM_Status);
			AssertEquals(EM_MessageSubTypeList.Codes.SEDAdd, incomingMessage.EM_MessageSubType);
			AssertEquals(2, entry.Messages.Count);
			AssertEquals(true, entry.Messages.Contains(incomingMessage));
			EmailDef email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>((EmailDef emailToMatched) => emailToMatched.Subject.Contains(AESCommodityShipmentProcessor.ResponseEmailSubject)));
			AssertEquals(1, email.Recipients.Count);
			AssertEquals(currentUser.GS_EmailAddress, email.Recipients[0].Email);
			AssertEquals(AESCommodityShipmentProcessor.ResponseEmailSubject + " (USPPI: John Williams Pty. LTD.) Response (Failure) for B00001000", email.Subject);
			string errorRowText = @"<td>{0}</td><td>{1}</td><td>{2}</td>";
			AssertContains(string.Format(errorRowText, "066", AESSeverityIndicatorList.Descriptions.Fatally, HtmlEncode("FILING OPT IND MUST BE 2 OR 4")), email.Body);
			AssertContains(string.Format(errorRowText, "970", AESSeverityIndicatorList.Descriptions.Fatally, HtmlEncode("SHIPMENT REJECTED; RESOLVE & RETRANSMIT")), email.Body);

			USCustomsDataRegistry.Instance.AESSendNotificationsItem.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Core.Constants.EmailTo.NominatedGroup);
			incomingMessage.EM_Status = EDIMessage.Status.Queued;
			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			processor.ExecuteBatch();
			incomingMessage.Reload();
			AssertEquals(EDIMessage.Status.Received, incomingMessage.EM_Status);
			email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>((EmailDef emailToMatched) => emailToMatched.Subject.Contains(AESCommodityShipmentProcessor.ResponseEmailSubject)));
			AssertEquals(2, email.Recipients.Count);
			AssertEquals(true, email.Recipients.Contains(staffZ1.GS_EmailAddress));
			AssertEquals(true, email.Recipients.Contains(staffZ2.GS_EmailAddress));
			AssertEquals(AESCommodityShipmentProcessor.ResponseEmailSubject + " (USPPI: John Williams Pty. LTD.) Response (Failure) for B00001000", email.Subject);

			USCustomsDataRegistry.Instance.AESSendNotificationsItem.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Core.Constants.EmailTo.StaffMemberAndNominatedGroup);
			incomingMessage.EM_Status = EDIMessage.Status.Queued;
			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			processor.ExecuteBatch();
			incomingMessage.Reload();
			AssertEquals(EDIMessage.Status.Received, incomingMessage.EM_Status);
			email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>((EmailDef emailToMatched) => emailToMatched.Subject.Contains(AESCommodityShipmentProcessor.ResponseEmailSubject)));
			AssertEquals(3, email.Recipients.Count);
			AssertEquals(true, email.Recipients.Contains(staffZ1.GS_EmailAddress));
			AssertEquals(true, email.Recipients.Contains(currentUser.GS_EmailAddress));
			AssertEquals(true, email.Recipients.Contains(staffZ2.GS_EmailAddress));
			AssertEquals(AESCommodityShipmentProcessor.ResponseEmailSubject + " (USPPI: John Williams Pty. LTD.) Response (Failure) for B00001000", email.Subject);

			USCustomsDataRegistry.Instance.AESSendNotificationsItem.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Core.Constants.EmailTo.NoEmails);
			incomingMessage.EM_Status = EDIMessage.Status.Queued;
			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			processor.ExecuteBatch();
			incomingMessage.Reload();
			AssertEquals(EDIMessage.Status.Received, incomingMessage.EM_Status);
			email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>((EmailDef emailToMatched) => emailToMatched.Subject.Contains(AESCommodityShipmentProcessor.ResponseEmailSubject)));
			AssertNull(email);
		}

		#region Implementation

		ZString HtmlEncode(ZString data)
		{
			return WebUtility.HtmlEncode(data).Replace("\r\n", "<br>").Replace("\n", "<br>");
		}

		JobDeclaration declaration;
		CusEntryHeader entry;
		AESTIREDIMessage outgoingMessage;
		AESTIREDIMessage incomingMessage;
		GlbStaff currentUser;
		GlbGroup groupZZ1;
		GlbStaff staffZ1;
		GlbStaff staffZ2;

		protected override void SetUp()
		{
			base.SetUp();

			currentUser = Factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
			currentUser.GS_EmailAddress = "dong@pretend.email.com";

			groupZZ1 = Factory.New<GlbGroup>();
			groupZZ1.GG_Code = "ZZ1";
			staffZ1 = groupZZ1.Staff.AddNew();
			staffZ1.GS_Code = "Z1";
			staffZ1.GS_LoginName = "~2";
			staffZ1.GS_EmailAddress = "postmaster@pretendemail.com";

			staffZ2 = groupZZ1.Staff.AddNew();
			staffZ2.GS_Code = "Z2";
			staffZ2.GS_LoginName = "z2";
			staffZ2.GS_EmailAddress = "dong2@pretend.email.com";

			Factory.Save();
			USCustomsDataRegistry.Instance.AESSendNotificationsToGroupItem.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, groupZZ1.PK.ToGuid());

			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

			OrgHeader uSPPI = Factory.NewWithValidTestData<OrgHeader>();
			uSPPI.OH_FullName = "John Williams Pty. LTD.";
			uSPPI.Addresses.AddNewMainAddress();
			uSPPI.MainAddress.OA_Address1 = "200 Highway 1";
			uSPPI.MainAddress.OA_Address2 = "Johnstown";
			uSPPI.MainAddress.OA_City = "Alberqueque";

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceDate = ZDateTime.Today.AddDays(-5);
			invoice.JZ_InvoiceNumber = "INV1232";
			invoice.JZ_InvoiceAmount = 6000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoice.JZ_OH_Supplier = uSPPI.PK;

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "3506990000";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.JI_LinePrice = 6000m;
			invoiceLine.JI_CustomsQuantity = 600m;
			invoiceLine.JI_InvoiceQuantity = 600m;
			invoiceLine.JI_InvoiceUQ = Core.Constants.Weight.Kilograms;
			invoiceLine.JI_Weight = 10m;
			invoiceLine.JI_WeightUQ = Core.Constants.Weight.Kilograms;
			Factory.Save();

			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			AssertEquals(1, declaration.ActiveEntryHeaders.Count);
			entry = declaration.ActiveEntryHeaders[0];
			AssertEquals("B00001000", entry.CH_BGMReference);

			ZString encodedMessageNum = AESTIRMessageNumberEncoder.Encode(2147483647);
			var interchange = Factory.New<CBPEDIInterchange>();
			interchange.EI_ApplicationCode = CBPEDIInterchange.ApplicationCodes.USCustomsExport;
			interchange.EI_From = "861161674";
			interchange.EI_To = "USC";
			interchange.EI_HeaderText = "A    861161674CAREDIEXT20091207" + encodedMessageNum + "N861161674";
			interchange.EI_FooterText = "Z    861161674      EXT20091207" + encodedMessageNum + " 861161674";

			outgoingMessage = Factory.New<AESTIREDIMessageForTesting>();

			outgoingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsExport;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_MessageNum = "2147483647";
			outgoingMessage.EM_MessageType = ApplicationIdentifierCodeList.AES.CommodityShipment;
			outgoingMessage.EM_MessageSubType = EM_MessageSubTypeList.Codes.SEDAdd;
			outgoingMessage.EM_LinkedObject = entry;
			outgoingMessage.EM_EI = interchange.PK;

			incomingMessage = Factory.New<AESTIREDIMessage>();
			incomingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsExport;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_MessageType = ApplicationIdentifierCodeList.AES.CommodityShipmentWarningReminder;
			incomingMessage.EM_MessageNum = "2147483647";

			Factory.Save();
		}
		#endregion
	}

	sealed class AESTIREDIMessageForTesting : AESTIREDIMessage
	{
		public AESTIREDIMessageForTesting(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override void GetNumberFountainNumbersAndFillInPlaceHolders()
		{
		}
	}
}
