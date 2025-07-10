using System;
using System.Drawing;
using System.Linq;
using System.Net;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.MessageProcessors.Testing
{
	public class AESCommodityShipmentProcessorTest : TestCaseWithFactory
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
			AssertEquals("Shipment Reference Number is same as Declaration Number", declaration.JE_DeclarationReference, entry.CH_BGMReference);
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
			EmailDef email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate(EmailDef emailToMatched)
			{ return emailToMatched.Subject.Contains(AESCommodityShipmentProcessor.ResponseEmailSubject); }));
			AssertEquals(1, email.Recipients.Count);
			AssertEquals(currentUser.GS_EmailAddress, email.Recipients[0].Email);
			AssertEquals(AESCommodityShipmentProcessor.ResponseEmailSubject + " (USPPI: ABC Exporters) Response (Failure) for B00001000", email.Subject);
			string errorRowText = @"<td>{0}</td><td>{1}</td><td>{2}</td>";
			AssertContains(string.Format(errorRowText, "066", AESSeverityIndicatorList.Descriptions.Fatally, HtmlEncode("FILING OPT IND MUST BE 2 OR 4")), email.Body);
			AssertContains(string.Format(errorRowText, "970", AESSeverityIndicatorList.Descriptions.Fatally, HtmlEncode("SHIPMENT REJECTED; RESOLVE & RETRANSMIT")), email.Body);
			AssertEquals("Entry Status", AESDirectCustomsEntryStatus.Codes.Error, entry.CH_EntryStatus);

			entry.CH_BGMReference = declaration.JE_DeclarationReference + "01";
			incomingMessage.EM_Status = EDIMessage.Status.Queued;
			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			processor.ExecuteBatch();
			incomingMessage.Reload();
			entry.Reload();
			entry.Messages.Load();
			AssertEquals(EDIMessage.Status.Received, incomingMessage.EM_Status);
			AssertEquals(EM_MessageSubTypeList.Codes.SEDAdd, incomingMessage.EM_MessageSubType);
			AssertEquals(2, entry.Messages.Count);
			AssertEquals(true, entry.Messages.Contains(incomingMessage));
			email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate(EmailDef emailToMatched)
			{ return emailToMatched.Subject.Contains(AESCommodityShipmentProcessor.ResponseEmailSubject); }));
			AssertEquals(1, email.Recipients.Count);
			AssertEquals(currentUser.GS_EmailAddress, email.Recipients[0].Email);
			AssertEquals(AESCommodityShipmentProcessor.ResponseEmailSubject + " (USPPI: ABC Exporters) Response (Failure) for B00001000 / B0000100001", email.Subject);
			AssertContains(string.Format(errorRowText, "066", AESSeverityIndicatorList.Descriptions.Fatally, HtmlEncode("FILING OPT IND MUST BE 2 OR 4")), email.Body);
			AssertContains(string.Format(errorRowText, "970", AESSeverityIndicatorList.Descriptions.Fatally, HtmlEncode("SHIPMENT REJECTED; RESOLVE & RETRANSMIT")), email.Body);
			AssertEquals("Entry Status", AESDirectCustomsEntryStatus.Codes.Error, entry.CH_EntryStatus);

			USCustomsDataRegistry.Instance.AESSendNotificationsItem.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Core.Constants.EmailTo.NominatedGroup);
			incomingMessage.EM_Status = EDIMessage.Status.Queued;
			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			processor.ExecuteBatch();
			incomingMessage.Reload();
			AssertEquals(EDIMessage.Status.Received, incomingMessage.EM_Status);
			email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate(EmailDef emailToMatched)
			{ return emailToMatched.Subject.Contains(AESCommodityShipmentProcessor.ResponseEmailSubject); }));
			AssertEquals(2, email.Recipients.Count);
			AssertEquals(true, email.Recipients.Contains(staffZ1.GS_EmailAddress));
			AssertEquals(true, email.Recipients.Contains(staffZ2.GS_EmailAddress));
			AssertEquals(AESCommodityShipmentProcessor.ResponseEmailSubject + " (USPPI: ABC Exporters) Response (Failure) for B00001000 / B0000100001", email.Subject);

			USCustomsDataRegistry.Instance.AESSendNotificationsItem.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Core.Constants.EmailTo.StaffMemberAndNominatedGroup);
			incomingMessage.EM_Status = EDIMessage.Status.Queued;
			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			processor.ExecuteBatch();
			incomingMessage.Reload();
			AssertEquals(EDIMessage.Status.Received, incomingMessage.EM_Status);
			email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate(EmailDef emailToMatched)
			{ return emailToMatched.Subject.Contains(AESCommodityShipmentProcessor.ResponseEmailSubject); }));
			AssertEquals(3, email.Recipients.Count);
			AssertEquals(true, email.Recipients.Contains(staffZ1.GS_EmailAddress));
			AssertEquals(true, email.Recipients.Contains(currentUser.GS_EmailAddress));
			AssertEquals(true, email.Recipients.Contains(staffZ2.GS_EmailAddress));
			AssertEquals(AESCommodityShipmentProcessor.ResponseEmailSubject + " (USPPI: ABC Exporters) Response (Failure) for B00001000 / B0000100001", email.Subject);

			USCustomsDataRegistry.Instance.AESSendNotificationsItem.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Core.Constants.EmailTo.NoEmails);
			incomingMessage.EM_Status = EDIMessage.Status.Queued;
			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			processor.ExecuteBatch();
			incomingMessage.Reload();
			AssertEquals(EDIMessage.Status.Received, incomingMessage.EM_Status);
			email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate(EmailDef emailToMatched)
			{ return emailToMatched.Subject.Contains(AESCommodityShipmentProcessor.ResponseEmailSubject); }));
			AssertNull(email);
		}

		public void TestLinkToEntryAndUpdateMessageByITNumberFirstThenShipment()
		{
			entry.EntryNumber = "X20230504639873";
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.Export;
			outgoingMessage.EM_MessageSubType = EM_MessageSubTypeList.Codes.SEDAdd;

			USCustomsDataRegistry.Instance.AESSendNotificationsItem.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Core.Constants.EmailTo.NominatedGroup);
			incomingMessage.EM_MessageText =
"B  58123456789E          ACE TEST IMPORTER 1                                    " +
"SC1N11HKPAAPLUS300055935        TITANIC                2 58201110120230508 N    " +
"ES197H AI SHIPMENT ON HOLD.CONTACT 5555555555     X20230504639873               " +
"Y  58123456789E          ACE TEST IMPORTER 1";
			incomingMessage.EM_MessageNum = "";
			Factory.Save();

			var processor = new AESTIRIncomingMessageProcessor();
			processor.Logger = new BatchProcessor.LoggingInformation();
			processor.ExecuteBatch();
			incomingMessage.Reload();
			AssertEquals(EDIMessage.Status.Received, incomingMessage.EM_Status);
			AssertEquals(entry, incomingMessage.EM_LinkedObject);
			AssertEquals(EM_MessageSubTypeList.Codes.SEDAdd, incomingMessage.EM_MessageSubType);

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = "CLD";
			consol.JK_TransportMode = "SEA";
			consol.JK_RL_NKLoadPort = "USLAX";
			consol.JK_RL_NKDischargePort = "JPYOK";
			consol.JK_MasterBillNum = "MASTERBILL";

			var transport = consol.MostInterestingTransportForBinding[0];
			transport.JW_VoyageFlight = "94";
			transport.JW_Vessel = "BOSTON EXPRESS";
			var shipment = consol.Shipments.AddNew();
			shipment.JS_ConsolReference = "S300055935";
			shipment.JS_HouseBill = "HOUSEBILL";
			declaration.JE_JS = shipment.PK;

			Factory.Save();

			incomingMessage.EM_Status = EDIMessage.Status.Queued;
			incomingMessage.EM_LinkedObject = null;
			incomingMessage.EM_MessageText =
"B  58123456789E          ACE TEST IMPORTER 1                                    " +
"SC1N11HKPAAPLUS300055935        TITANIC                2 58201110120230508 N    " +
"ES197H AI SHIPMENT ON HOLD.CONTACT 5555555555     X20230504639877               " +
"Y  58123456789E          ACE TEST IMPORTER 1";

			processor.ExecuteBatch();
			incomingMessage.Reload();
			AssertEquals(EDIMessage.Status.Received, incomingMessage.EM_Status);
			AssertEquals(entry, incomingMessage.EM_LinkedObject);
			AssertEquals("", incomingMessage.EM_MessageNum);
		}

		public void TestOverriddenCompanyName()
		{
			USCustomsDataRegistry.Instance.AESSendNotificationsItem.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Core.Constants.EmailTo.StaffMember);
			invoice.USPPIDocAddress.E2_OA_Address = address2.PK;
			outgoingMessage.EM_SendWithMessageErrors = true;
			incomingMessage.EM_MessageText =
"B  60612456712E          US EXPORTER NAME                                       " +
"SC1N11AUCAUNKNS40002509        ABAI YUN HE               60267270420081101 N    " +
"ES1066  F FILING OPT IND MUST BE 2 OR 4                                         " +
"ES1970 RF SHIPMENT REJECTED; RESOLVE & RETRANSMIT                               " +
"Y  60612456712E          US EXPORTER NAME";

			Factory.Save();
			AssertEquals("Shipment Reference Number is same as Declaration Number", declaration.JE_DeclarationReference, entry.CH_BGMReference);
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
			EmailDef email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate(EmailDef emailToMatched)
			{ return emailToMatched.Subject.Contains(AESCommodityShipmentProcessor.ResponseEmailSubject); }));
			AssertEquals(1, email.Recipients.Count);
			AssertEquals(currentUser.GS_EmailAddress, email.Recipients[0].Email);
			AssertEquals(AESCommodityShipmentProcessor.ResponseEmailSubject + " (USPPI: Overridden Company) Response (Failure) for B00001000", email.Subject);
			string errorRowText = @"<td>{0}</td><td>{1}</td><td>{2}</td>";
			AssertContains(string.Format(errorRowText, "066", AESSeverityIndicatorList.Descriptions.Fatally, HtmlEncode("FILING OPT IND MUST BE 2 OR 4")), email.Body);
			AssertContains(string.Format(errorRowText, "970", AESSeverityIndicatorList.Descriptions.Fatally, HtmlEncode("SHIPMENT REJECTED; RESOLVE & RETRANSMIT")), email.Body);
			AssertEquals("Entry Status", AESDirectCustomsEntryStatus.Codes.Error, entry.CH_EntryStatus);
		}

		public void TestOverriddenUSPPIDocAddress()
		{
			var miscAddress = Factory.Load<OrgAddress>(OrganisationsDataRegistry.Instance.MiscOrganisation.Value.PrimaryOfficeAddress);
			miscAddress.OA_CompanyNameOverride = "MISC Company Name Override";
			USCustomsDataRegistry.Instance.AESSendNotificationsItem.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Core.Constants.EmailTo.StaffMember);
			invoice.USPPIDocAddress.E2_AddressOverride = true;
			invoice.USPPIDocAddress.E2_CompanyName = "DocAddress Override Company";
			outgoingMessage.EM_SendWithMessageErrors = true;
			incomingMessage.EM_MessageText =
"B  60612456712E          US EXPORTER NAME                                       " +
"SC1N11AUCAUNKNS40002509        ABAI YUN HE               60267270420081101 N    " +
"ES1066  F FILING OPT IND MUST BE 2 OR 4                                         " +
"ES1970 RF SHIPMENT REJECTED; RESOLVE & RETRANSMIT                               " +
"Y  60612456712E          US EXPORTER NAME";

			Factory.Save();
			AssertEquals("Shipment Reference Number is same as Declaration Number", declaration.JE_DeclarationReference, entry.CH_BGMReference);
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
			EmailDef email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate (EmailDef emailToMatched)
			{ return emailToMatched.Subject.Contains(AESCommodityShipmentProcessor.ResponseEmailSubject); }));
			AssertEquals(1, email.Recipients.Count);
			AssertEquals(currentUser.GS_EmailAddress, email.Recipients[0].Email);
			AssertEquals(AESCommodityShipmentProcessor.ResponseEmailSubject + " (USPPI: DocAddress Override Company) Response (Failure) for B00001000", email.Subject);
		}

		public void TestWhenShipmentDeletedOrCancelledMessage()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceDate = ZDateTime.Today.AddDays(-5);
			invoice.JZ_InvoiceNumber = "INV1232";

			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Description = "LINE778";
			declaration.JE_OH_Forwarder = uSPPI.PK;
			declaration.JE_OH_ShippingLine = uSPPI.PK;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.NonContainerised;
			declaration.JE_VoyageFlightNo = "510";
			declaration.JE_RL_NKFinalDestination = "AUSYD";
			declaration.US_DateOfExport = ZDate.Today;
			declaration.US_SchDExport = "8888";
			declaration.US_RL_NKPortOfExport = "USLBH";
			declaration.US_TransportReference = "081-56783210";
			entry.CH_Status = AESDirectCustomsEntryStatus.Codes.OriginalSEDClear;
			declaration.JE_MessageStatus = AESDirectCustomsEntryStatus.Codes.AwaitingDeleteResponse;

			USCustomsDataRegistry.Instance.AESSendNotificationsItem.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Core.Constants.EmailTo.StaffMember);
			incomingMessage.EM_MessageText =
"B  91013199000E          ABC EXPORTS USA                                        " +
"SC1N41CAILQFA B00152888        AQANTAS AIRWAYS LIMITED 2 60210300120091221 N    " +
"ES1983 A  SHIPMENT CANCELLED                      X20230704648267               " +
"Y  91013199000E          ABC EXPORTS USA";
			incomingMessage.EM_MessageNum = "";
			Factory.Save();

			AESTIRIncomingMessageProcessor processor = new AESTIRIncomingMessageProcessor();
			processor.Logger = new BatchProcessor.LoggingInformation();
			processor.ExecuteBatch();

			incomingMessage.Reload();
			entry.Reload();
			AssertEquals("ITN Number is empty", string.Empty, declaration.DeclarationNumber);
			var log = declaration.Logs.Find(new ZQuery(Enterprise.ZArchitecture.Schema.StmALogSchema.SL_Reference, "SED Delete: [X20230704648267]"));
			AssertNotNull(log);
			AssertEquals("", incomingMessage.EM_MessageNum);
		}

		public void TestPublishUniversalEvent_NoConcurrency_CS00244963()
		{
			var newBranch = Factory.New<GlbBranch>();
			newBranch.FillWithValidTestData();
			newBranch.GB_GC = GlbCompany.CurrentCompany.PK;

			var image1 = new Bitmap(1, 2);
			var image2 = new Bitmap(2, 1);
			SystemDataRegistry.Instance.HtmlEmailBannerImage.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, image1);
			SystemDataRegistry.Instance.HtmlEmailBannerImage.SetValue(Guid.Empty, newBranch.PK.ToGuid(), Guid.Empty, image2);

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = "CLD";
			consol.JK_TransportMode = "SEA";
			consol.JK_RL_NKLoadPort = "USLAX";
			consol.JK_RL_NKDischargePort = "JPYOK";
			consol.JK_MasterBillNum = "MASTERBILL";

			var transport = consol.MostInterestingTransportForBinding[0];
			transport.JW_VoyageFlight = "94";
			transport.JW_Vessel = "BOSTON EXPRESS";
			transport.JW_ETD = ZDate.Today;
			transport.JW_ETA = ZDate.Today.AddDays(10);
			transport.JW_Vessel = "BOSTON EXPRESS";

			var shipment = consol.Shipments.AddNew();
			shipment.JS_HouseBill = "HOUSEBILL";
			var trigger = shipment.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = Events.ExportCustomsClearedCode;
			trigger.TemplateConditions.TemplateCondition1 = "BRK";

			declaration.JE_JS = shipment.PK;
			declaration.JE_MasterBill = "MASTERBILL";
			declaration.JE_HouseBill = "HOUSEBILL";
			declaration.JE_GB = newBranch.PK;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			incomingMessage.EM_MessageText =
					"B  60612456712E          US EXPORTER NAME                                       " +
					"SC1N11AUCAUNKNS40002509        ABAI YUN HE               60267270420081101 N    " +
					"ES1974 A  SHIPMENT ADDED                          X20130924012062               " +
					"Y  60612456712E          US EXPORTER NAME";
			Factory.Save();

			var logs = shipment.GetLogs();
			var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.ExportCustomsClearedCode);
			var exportCustomsClearedLogs = logs.Find(query);
			AssertEquals(0, exportCustomsClearedLogs.Length);
			Assert(trigger.P9_ActualDate.IsEmpty);

			var processor = new AESTIRIncomingMessageProcessor();
			processor.Logger = new BatchProcessor.LoggingInformation();
			processor.ExecuteBatch();

			var log = processor.Logger.UserLogStrings.Cast<string>();
			AssertNull(log.FirstOrDefault(x => x.Contains("CONCURRENCY")));
			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var banner = email.Attachments.Cast<AttachmentDef>().FirstOrDefault(x => x.DisplayName == "Banner.jpg");
			var image = new Bitmap(new System.IO.MemoryStream(banner.Data));
			AssertEquals(1, image.Width);
		}

		public void TestShipmentAddedResponse()
		{
			OrgHeader forwarder = Factory.LoadTop1<OrgHeader>(new ZQuery());
			forwarder.MainAddress.OA_Phone = "+1 (273) 5495200";
			forwarder.MainAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, "123456789");
			forwarder.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "APLU");
			OrgContact forwarderContact = forwarder.Contacts.AddNew();
			forwarderContact.OC_ContactName = "John Smith";
			OrgDocument contactDocs = forwarderContact.Documents.AddNew();
			contactDocs.OD_DocumentGroup = "ALL";

			OrgHeader carrier = Factory.New<OrgHeader>();
			carrier.OH_FullName = "Air Carrier";
			carrier.OH_IsShippingLine = true;
			carrier.OH_RL_NKClosestPort = "AUSYD";
			carrier.OH_IsAirLine = true;
			carrier.MiscServ.OM_RM_Airline = RefAirline.LoadFromAirlinePrefix(Factory, "081").PK;

			declaration.JE_OH_Forwarder = forwarder.PK;
			declaration.JE_OH_ShippingLine = carrier.PK;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.NonContainerised;
			declaration.JE_VoyageFlightNo = "510";
			declaration.JE_RL_NKFinalDestination = "AUSYD";
			declaration.US_DateOfExport = ZDate.Today;
			declaration.US_SchDExport = "8888";
			declaration.US_RL_NKPortOfExport = "USLBH";
			declaration.US_TransportReference = "081-56783210";

			USCustomsDataRegistry.Instance.AESSendNotificationsItem.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Core.Constants.EmailTo.StaffMember);
			incomingMessage.EM_MessageText =
"B  91013199000E          ABC EXPORTS USA                                        " +
"SC1N41CAILQFA B00152888        AQANTAS AIRWAYS LIMITED 2 60210300120091221 N    " +
"ES1974 A  SHIPMENT ADDED                          X20091221000053               " +
"Y  91013199000E          ABC EXPORTS USA";
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
			EmailDef email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate(EmailDef emailToMatched)
			{ return emailToMatched.Subject.Contains(AESCommodityShipmentProcessor.ResponseEmailSubject); }));
			AssertEquals(1, email.Recipients.Count);
			AssertEquals(currentUser.GS_EmailAddress, email.Recipients[0].Email);
			AssertEquals(AESCommodityShipmentProcessor.ResponseEmailSubject + " (USPPI: ABC Exporters) Response for B00001000", email.Subject);
			string rowText = @"<td>{0}</td><td>&nbsp;</td><td>{1}</td>";
			string valueExpected = string.Format(rowText, "974", HtmlEncode("SHIPMENT ADDED"));
			AssertContains(valueExpected, email.Body);
			valueExpected = string.Format(rowText, "ITN", HtmlEncode("X20091221000053"));
			AssertContains(valueExpected, email.Body);

			AssertEquals("Entry should be updated with ITN", "X20091221000053", entry.EntryNumber);
			AssertEquals("Declaration should reflect this ITN", "X20091221000053", declaration.DeclarationNumber);
			AssertEquals("Entry Status", AESDirectCustomsEntryStatus.Codes.OriginalSEDClear, entry.CH_EntryStatus);

			var log = declaration.Logs.Find(new ZQuery(Enterprise.ZArchitecture.Schema.StmALogSchema.SL_Reference, "Public UniversalEvent"));
			AssertNotNull(log);
		}

		public void TestUserGetsEmailResponseWithUnsolicitedMessage()
		{
			USCustomsDataRegistry.Instance.AESSendNotificationsItem.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Core.Constants.EmailTo.StaffMember);

			var staff = Factory.New<GlbStaff>();
			staff.GS_LoginName = "TEST1";
			staff.GS_Code = "TS1";
			staff.GS_IsSystemAccount = false;
			staff.GS_IsActive = true;
			staff.GS_EmailAddress = "test1@wisetechglobal.com";

			var staff2 = Factory.New<GlbStaff>();
			staff2.GS_LoginName = "TEST2";
			staff2.GS_Code = "TS2";
			staff2.GS_IsSystemAccount = true;
			staff2.GS_IsActive = true;
			staff2.GS_EmailAddress = "test2@wisetechglobal.com";

			var staff3 = Factory.New<GlbStaff>();
			staff3.GS_LoginName = "TEST3";
			staff3.GS_Code = "TS3";
			staff3.GS_IsSystemAccount = false;
			staff3.GS_IsActive = false;
			staff3.GS_EmailAddress = "test3@wisetechglobal.com";

			var staff4 = Factory.New<GlbStaff>();
			staff4.GS_LoginName = "TEST4";
			staff4.GS_Code = "TS4";
			staff4.GS_IsSystemAccount = false;
			staff4.GS_IsActive = true;
			staff4.GS_EmailAddress = "test4@wisetechglobal.com";
			Factory.Save();

			var mock = Factory.NewMoq<AESTIREDIMessage>();
			mock.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");
			var outgoingMessage2 = mock.Object;
			outgoingMessage2.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsExport;
			outgoingMessage2.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage2.EM_Status = EDIMessage.Status.Sent;
			outgoingMessage2.EM_MessageNum = "2147483648";
			outgoingMessage2.EM_MessageType = ApplicationIdentifierCodeList.AES.CommodityShipment;
			outgoingMessage2.EM_MessageSubType = EM_MessageSubTypeList.Codes.SEDAdd;
			outgoingMessage2.EM_LinkedObject = entry;
			outgoingMessage2.EM_SystemCreateUser = staff4.GS_Code;
			outgoingMessage2.EM_SystemCreateTimeUtc = ZDateTime.UtcNow.AddDays(-1);

			outgoingMessage.EM_SystemCreateUser = staff.GS_Code;
			incomingMessage.EM_MessageNum = ZString.Empty;
			incomingMessage.EM_MessageText =
					"B  60612456712E          US EXPORTER NAME                                       " +
					"SC1N11AUCAUNKNB00001000        ABAI YUN HE               60267270420081101 N    " +
					"ES197H A  SHIPMENT ON HOLD.CONTACT 5555555555     X20130924012062               " +
					"Y  60612456712E          US EXPORTER NAME";
			incomingMessage.EM_SystemCreateTimeUtc = ZDateTime.UtcNow;
			Factory.Save();

			AssertEquals("Shipment Reference Number is same as Declaration Number", declaration.JE_DeclarationReference, entry.CH_BGMReference);
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			var processor = new AESTIRIncomingMessageProcessor();
			processor.Logger = new BatchProcessor.LoggingInformation();
			processor.ExecuteBatch();

			incomingMessage.Reload();
			entry.Reload();
			entry.Messages.Load();
			AssertEquals(EDIMessage.Status.Received, incomingMessage.EM_Status);
			AssertEquals(3, entry.Messages.Count);
			AssertEquals(true, entry.Messages.Contains(incomingMessage));

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate(EmailDef emailToMatched)
			{ return emailToMatched.Subject.Contains(AESCommodityShipmentProcessor.ResponseEmailSubject); }));
			var recipients = email.Recipients.ToList<RecipientDef>();
			AssertEquals(2, recipients.Count);
			AssertEquals(true, recipients.Exists(x => x.Email == staff.GS_EmailAddress));
			AssertEquals(true, recipients.Exists(x => x.Email == staff4.GS_EmailAddress));

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			outgoingMessage.EM_SystemCreateUser = staff2.GS_Code;
			outgoingMessage2.EM_SystemCreateTimeUtc = ZDateTime.UtcNow.AddDays(1);
			incomingMessage.EM_Status = "QUE";
			incomingMessage.EM_LinkTable = "";
			incomingMessage.EM_LinkUniqueID = ZGuid.Empty;
			Factory.Save();

			processor = new AESTIRIncomingMessageProcessor();
			processor.Logger = new BatchProcessor.LoggingInformation();
			processor.ExecuteBatch();

			incomingMessage.Reload();
			entry.Reload();
			entry.Messages.Load();
			AssertEquals(EDIMessage.Status.Received, incomingMessage.EM_Status);
			AssertEquals(3, entry.Messages.Count);
			AssertEquals(true, entry.Messages.Contains(incomingMessage));

			email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate(EmailDef emailToMatched)
			{ return emailToMatched.Subject.Contains(AESCommodityShipmentProcessor.ResponseEmailSubject); }));
			AssertNull(email);

			outgoingMessage.EM_SystemCreateUser = staff3.GS_Code;
			incomingMessage.EM_Status = "QUE";
			incomingMessage.EM_LinkTable = "";
			incomingMessage.EM_LinkUniqueID = ZGuid.Empty;
			Factory.Save();

			processor = new AESTIRIncomingMessageProcessor();
			processor.Logger = new BatchProcessor.LoggingInformation();
			processor.ExecuteBatch();

			incomingMessage.Reload();
			entry.Reload();
			entry.Messages.Load();
			AssertEquals(EDIMessage.Status.Received, incomingMessage.EM_Status);
			AssertEquals(3, entry.Messages.Count);
			AssertEquals(true, entry.Messages.Contains(incomingMessage));

			email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate(EmailDef emailToMatched)
			{ return emailToMatched.Subject.Contains(AESCommodityShipmentProcessor.ResponseEmailSubject); }));
			AssertNull(email);
		}

		public void TestUserGetsEmailResponseWhenGroupIsEmpty()
		{
			OrgHeader forwarder = Factory.LoadTop1<OrgHeader>(new ZQuery());
			forwarder.MainAddress.OA_Phone = "+1 (273) 5495200";
			forwarder.MainAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, "123456789");
			forwarder.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "APLU");
			OrgContact forwarderContact = forwarder.Contacts.AddNew();
			forwarderContact.OC_ContactName = "John Smith";
			OrgDocument contactDocs = forwarderContact.Documents.AddNew();
			contactDocs.OD_DocumentGroup = "ALL";

			OrgHeader carrier = Factory.New<OrgHeader>();
			carrier.OH_FullName = "Air Carrier";
			carrier.OH_IsShippingLine = true;
			carrier.OH_RL_NKClosestPort = "AUSYD";
			carrier.OH_IsAirLine = true;
			carrier.MiscServ.OM_RM_Airline = RefAirline.LoadFromAirlinePrefix(Factory, "081").PK;

			declaration.JE_OH_Forwarder = forwarder.PK;
			declaration.JE_OH_ShippingLine = carrier.PK;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.NonContainerised;
			declaration.JE_VoyageFlightNo = "510";
			declaration.JE_RL_NKFinalDestination = "AUSYD";
			declaration.US_DateOfExport = ZDate.Today;
			declaration.US_SchDExport = "8888";
			declaration.US_RL_NKPortOfExport = "USLBH";
			declaration.US_TransportReference = "081-56783210";

			GlbGroup emptyNotificationGroup = Factory.New<GlbGroup>();
			emptyNotificationGroup.GG_Code = "TST";
			GlbStaff staff = Factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
			staff.GS_EmailAddress = "dummy@testcompany.com";
			emptyNotificationGroup.Staff.Add(staff);
			Factory.Save();

			USCustomsDataRegistry.Instance.AESSendNotificationsToGroup = emptyNotificationGroup.PK.ToGuid();
			USCustomsDataRegistry.Instance.AESSendNotificationsItem.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Core.Constants.EmailTo.StaffMemberAndNominatedGroup);
			incomingMessage.EM_MessageText =
"B  91013199000E          ABC EXPORTS USA                                        " +
"SC1N41CAILQFA B00152888        AQANTAS AIRWAYS LIMITED 2 60210300120091221 N    " +
"ES1974 A  SHIPMENT ADDED                          X20091221000053               " +
"Y  91013199000E          ABC EXPORTS USA";
			Factory.Save();

			emptyNotificationGroup.Staff.RemoveAll();
			Factory.Save();

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			AESTIRIncomingMessageProcessor processor = new AESTIRIncomingMessageProcessor();
			processor.Logger = new BatchProcessor.LoggingInformation();
			processor.ExecuteBatch();

			incomingMessage.Reload();
			entry.Reload();
			entry.Messages.Load();

			EmailDef email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate(EmailDef emailToMatched)
			{ return emailToMatched.Subject.Contains(AESCommodityShipmentProcessor.ResponseEmailSubject); }));
			AssertEquals(1, email.Recipients.Count);
			AssertEquals(currentUser.GS_EmailAddress, email.Recipients[0].Email);
			AssertEquals(AESCommodityShipmentProcessor.ResponseEmailSubject + " (USPPI: ABC Exporters) Response for B00001000", email.Subject);

			var body = @"For more information on each code see <a href=""http://www.cbp.gov/document/guidance/aestir-appendix-commodity-filing-response-messages"">Appendix A</a>.";
			AssertContains("Body should contain", body, email.Body);

			string rowText = @"<td>{0}</td><td>&nbsp;</td><td>{1}</td>";
			string valueExpected = string.Format(rowText, "974", HtmlEncode("SHIPMENT ADDED"));
			AssertContains(valueExpected, email.Body);
			valueExpected = string.Format(rowText, "ITN", HtmlEncode("X20091221000053"));
			AssertContains(valueExpected, email.Body);
			AssertEquals("Entry Status", AESDirectCustomsEntryStatus.Codes.OriginalSEDClear, entry.CH_EntryStatus);
		}

		public void TestNoRecipientException()
		{
			var forwarder = Factory.LoadTop1<OrgHeader>(new ZQuery());
			forwarder.MainAddress.OA_Phone = "+1 (273) 5495200";
			forwarder.MainAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, "123456789");
			forwarder.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "APLU");
			var forwarderContact = forwarder.Contacts.AddNew();
			forwarderContact.OC_ContactName = "John Smith";
			var contactDocs = forwarderContact.Documents.AddNew();
			contactDocs.OD_DocumentGroup = "ALL";

			var carrier = Factory.New<OrgHeader>();
			carrier.OH_FullName = "Air Carrier";
			carrier.OH_IsShippingLine = true;
			carrier.OH_RL_NKClosestPort = "AUSYD";
			carrier.OH_IsAirLine = true;
			carrier.MiscServ.OM_RM_Airline = RefAirline.LoadFromAirlinePrefix(Factory, "081").PK;

			declaration.JE_OH_Forwarder = forwarder.PK;
			declaration.JE_OH_ShippingLine = carrier.PK;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.NonContainerised;
			declaration.JE_VoyageFlightNo = "510";
			declaration.JE_RL_NKFinalDestination = "AUSYD";
			declaration.US_DateOfExport = ZDate.Today;
			declaration.US_SchDExport = "8888";
			declaration.US_RL_NKPortOfExport = "USLBH";
			declaration.US_TransportReference = "081-56783210";

			var emptyNotificationGroup = Factory.New<GlbGroup>();
			emptyNotificationGroup.GG_Code = "TST";
			var staff = Factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
			staff.GS_EmailAddress = "dummy@testcompany.com";
			emptyNotificationGroup.Staff.Add(staff);
			Factory.Save();

			USCustomsDataRegistry.Instance.AESSendNotificationsToGroup = emptyNotificationGroup.PK.ToGuid();
			USCustomsDataRegistry.Instance.AESSendNotificationsItem.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Core.Constants.EmailTo.StaffMember);
			incomingMessage.EM_MessageText =
"B  91013199000E          ABC EXPORTS USA                                        " +
"SC1N41CAILQFA B00152888        AQANTAS AIRWAYS LIMITED 2 60210300120091221 N    " +
"ES1974 A  SHIPMENT ADDED                          X20091221000053               " +
"Y  91013199000E          ABC EXPORTS USA";
			Factory.Save();

			staff.GS_EmailAddress = "";
			Factory.Save();

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var processor = new AESTIRIncomingMessageProcessor();
			processor.ExecuteBatch();
			AssertEquals(0, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
		}

		[TestDate(2010, 07, 20, 10, 20, 30)]
		public void TestProcessingUnknownEntry()
		{
			USCustomsDataRegistry.Instance.AESSendNotificationsItem.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Core.Constants.EmailTo.NominatedGroup);
			incomingMessage.EM_MessageText =
"B  91013199000E          ABC EXPORTS USA                                        " +
"SC1N40CAILSIA B0015395300      ASINGAPORE AIRLINES     2 01535300120100812 N    " +
"ES1974 A  SHIPMENT ADDED                          X20100812003438               " +
"Y  91013199000E          ABC EXPORTS USA";
			incomingMessage.EM_MessageNum = "~123123432";
			Factory.Save();

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			AESTIRIncomingMessageProcessor processor = new AESTIRIncomingMessageProcessor();
			processor.Logger = new BatchProcessor.LoggingInformation();
			processor.ExecuteBatch();
			incomingMessage.Reload();
			AssertEquals(EDIMessage.Status.Received, incomingMessage.EM_Status);

			EmailDef email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate(EmailDef emailToMatched)
			{ return emailToMatched.Subject == "Shippers Export Declaration Response for Unknown"; }));
			AssertEquals(2, email.Recipients.Count);
			AssertEquals(true, email.Recipients.Contains(staffZ1.GS_EmailAddress));
			AssertEquals(true, email.Recipients.Contains(staffZ2.GS_EmailAddress));
			string rowText = @"<td>{0}</td><td>&nbsp;</td><td>{1}</td>";
			string valueExpected = string.Format(rowText, "974", HtmlEncode("SHIPMENT ADDED"));
			AssertContains(valueExpected, email.Body);
			valueExpected = string.Format(rowText, "ITN", HtmlEncode("X20100812003438"));
			AssertContains(valueExpected, email.Body);
		}

		[TestDate(2010, 07, 20, 10, 20, 30)]
		public void TestProcessingDeletedITN()
		{
			USCustomsDataRegistry.Instance.AESSendNotificationsItem.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Core.Constants.EmailTo.NominatedGroup);
			incomingMessage.EM_MessageText =
"B  91013199000E          ABC EXPORTS USA                                        " +
"SC1N40CAILSIA B0015395300      ASINGAPORE AIRLINES     2 01535300120100812 N    " +
"ES1983 A  SHIPMENT CANCELLED                      X20100812003438               " +
"Y  91013199000E          ABC EXPORTS USA";
			incomingMessage.EM_MessageNum = "~123123432";
			Factory.Save();

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			AESTIRIncomingMessageProcessor processor = new AESTIRIncomingMessageProcessor();
			processor.Logger = new BatchProcessor.LoggingInformation();
			processor.ExecuteBatch();
			incomingMessage.Reload();
			AssertEquals(EDIMessage.Status.Received, incomingMessage.EM_Status);

			EmailDef email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate(EmailDef emailToMatched)
			{ return emailToMatched.Subject == "Shippers Export Declaration Response for Unknown"; }));
			AssertEquals(2, email.Recipients.Count);
			AssertEquals(true, email.Recipients.Contains(staffZ1.GS_EmailAddress));
			AssertEquals(true, email.Recipients.Contains(staffZ2.GS_EmailAddress));
			string rowText = @"<td>{0}</td><td>&nbsp;</td><td>{1}</td>";
			string valueExpected = string.Format(rowText, "983", HtmlEncode("SHIPMENT CANCELLED"));
			AssertContains(valueExpected, email.Body);
			valueExpected = string.Format(rowText, "ITN", HtmlEncode("X20100812003438"));
			AssertContains(valueExpected, email.Body);
			AssertEquals(entry.EntryNumber, ZString.Empty);

			var log = entry.Logs.Find(new ZQuery(Enterprise.ZArchitecture.Schema.StmALogSchema.SL_Reference, "SED Delete: [X20100812003438]"));
			AssertNotNull(log);
		}

		[TestDate(2010, 07, 20, 10, 20, 30)]
		public void TestProcessingDeletedITNWehnNoMessageNumber()
		{
			USCustomsDataRegistry.Instance.AESSendNotificationsItem.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Core.Constants.EmailTo.NominatedGroup);

			entry.CH_BGMReference = "S0015395300";
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.Export;
			outgoingMessage.EM_MessageSubType = EM_MessageSubTypeList.Codes.SEDDelete;

			incomingMessage.EM_MessageText =
"B  91013199000E          ABC EXPORTS USA                                        " +
"SC1N40CAILSIA S0015395300      ASINGAPORE AIRLINES     2 01535300120100812 N    " +
"ES1983 A  SHIPMENT CANCELLED                      X20100812003438               " +
"Y  91013199000E          ABC EXPORTS USA";
			incomingMessage.EM_MessageNum = "";
			Factory.Save();

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			AESTIRIncomingMessageProcessor processor = new AESTIRIncomingMessageProcessor();
			processor.Logger = new BatchProcessor.LoggingInformation();
			processor.ExecuteBatch();

			incomingMessage.Reload();
			AssertEquals(EDIMessage.Status.Received, incomingMessage.EM_Status);

			EmailDef email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate(EmailDef emailToMatched)
			{ return emailToMatched.Subject.Contains("Shippers Export Declaration (USPPI: ABC Exporters)"); }));

			AssertEquals(2, email.Recipients.Count);
			AssertEquals(true, email.Recipients.Contains(staffZ1.GS_EmailAddress));
			AssertEquals(true, email.Recipients.Contains(staffZ2.GS_EmailAddress));
			string rowText = @"<td>{0}</td><td>&nbsp;</td><td>{1}</td>";
			string valueExpected = string.Format(rowText, "983", HtmlEncode("SHIPMENT CANCELLED"));
			AssertContains(valueExpected, email.Body);
			valueExpected = string.Format(rowText, "ITN", HtmlEncode("X20100812003438"));

			AssertContains(valueExpected, email.Body);
			AssertEquals(entry.EntryNumber, ZString.Empty);
			AssertEquals(entry.Declaration.DeclarationNumber, ZString.Empty);

			var log = entry.Logs.Find(new ZQuery(Enterprise.ZArchitecture.Schema.StmALogSchema.SL_Reference, "SED Delete: [X20100812003438]"));
			AssertNotNull(log);
		}

		public void TestProcessingDeletedITNWithWarning()
		{
			entry.CH_Status = AESDirectCustomsEntryStatus.Codes.OriginalSEDClear;
			USCustomsDataRegistry.Instance.AESSendNotificationsItem.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Core.Constants.EmailTo.NominatedGroup);
			incomingMessage.EM_MessageText =
"B  33088557700E          VMS AIRCRAFT CO                                        " +
"SC1N40CNCACA  S00002094        XAIR CHINA INTERNATIONAL2      272020140325 N    " +
"ES1700  C SHIPMENT REPORTED LATE; OPT 2                                         " +
"ES1983 A  SHIPMENT CANCELLED                      X20140324994618               " +
"Y  33088557700E          VMS AIRCRAFT CO";
			incomingMessage.EM_MessageNum = "2147483647";
			outgoingMessage.EM_MessageSubType = EM_MessageSubTypeList.Codes.SEDDelete;
			Factory.Save();

			var processor = new AESTIRIncomingMessageProcessor();
			processor.Logger = new BatchProcessor.LoggingInformation();
			processor.ExecuteBatch();
			incomingMessage.Reload();
			AssertEquals(EDIMessage.Status.Received, incomingMessage.EM_Status);
			AssertEquals(entry, incomingMessage.EM_LinkedObject);

			entry.Reload();
			AssertEquals(AESDirectCustomsEntryStatus.Codes.DeleteSEDClear, entry.CH_Status);

			var entryLoaded = new BusinessObjectFactory().Load<CusEntryHeader>(entry.PK);
			AssertEquals(UpdateActionCode.Add, entryLoaded.MessageAction);
			AssertEquals(EM_MessageSubTypeList.Codes.SEDDelete, incomingMessage.EM_MessageSubType);
		}

		public void TestProcessingResponseCode97H()
		{
			entry.CH_Status = AESDirectCustomsEntryStatus.Codes.AwaitingOriginalResponse;
			USCustomsDataRegistry.Instance.AESSendNotificationsItem.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Core.Constants.EmailTo.NominatedGroup);
			incomingMessage.EM_MessageText =
"B  58123456789E          ACE TEST IMPORTER 1                                    " +
"SC1N11HKPAAPLUB00178952        RTITANIC                2 58201110120200508 N    " +
"CL1OS 0001OTHER BREATHING APPLIANCES AND GAS MASKS AND 0000000000 AC33D        1" +
"CL29020008000NO 00000100000000010000   00000000000000001000     NLR             " +
"ES197H AI SHIPMENT OH HOLD                        X20200504399473               " +
"Y  58123456789E          ACE TEST IMPORTER 1";
			incomingMessage.EM_MessageNum = "2147483647";
			Factory.Save();

			var processor = new AESTIRIncomingMessageProcessor();
			processor.Logger = new BatchProcessor.LoggingInformation();
			processor.ExecuteBatch();
			incomingMessage.Reload();
			AssertEquals(EDIMessage.Status.Received, incomingMessage.EM_Status);
			AssertEquals(entry, incomingMessage.EM_LinkedObject);

			entry.Reload();
			AssertEquals(AESDirectCustomsEntryStatus.Codes.Hold, entry.CH_Status);
			AssertEquals(AESDirectCustomsEntryStatus.Codes.Hold, entry.CH_EntryStatus);
		}

		public void TestProcessingResponseCode97R()
		{
			entry.CH_Status = AESDirectCustomsEntryStatus.Codes.AwaitingOriginalResponse;
			USCustomsDataRegistry.Instance.AESSendNotificationsItem.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Core.Constants.EmailTo.NominatedGroup);
			incomingMessage.EM_MessageText =
"B  58123456789E          ACE TEST IMPORTER 1                                    " +
"SC1N11HKPAAPLUB00178952        RTITANIC                2 58201110120200508 N    " +
"CL1OS 0001OTHER BREATHING APPLIANCES AND GAS MASKS AND 0000000000 AC33D        1" +
"CL29020008000NO 00000100000000010000   00000000000000001000     NLR             " +
"ES197R AI SHIPMENT OH HOLD                        X20200504399473               " +
"Y  58123456789E          ACE TEST IMPORTER 1";
			incomingMessage.EM_MessageNum = "2147483647";
			Factory.Save();

			var processor = new AESTIRIncomingMessageProcessor();
			processor.Logger = new BatchProcessor.LoggingInformation();
			processor.ExecuteBatch();
			incomingMessage.Reload();
			AssertEquals(EDIMessage.Status.Received, incomingMessage.EM_Status);
			AssertEquals(entry, incomingMessage.EM_LinkedObject);

			entry.Reload();
			AssertEquals(AESDirectCustomsEntryStatus.Codes.Released, entry.CH_Status);
			AssertEquals(AESDirectCustomsEntryStatus.Codes.Released, entry.CH_EntryStatus);
		}

		public void TestProcessingHoldMessageMatchingByITN()
		{
			entry.EntryNumber = "X20230504639873";
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.Export;
			outgoingMessage.EM_MessageSubType = EM_MessageSubTypeList.Codes.SEDAdd;

			USCustomsDataRegistry.Instance.AESSendNotificationsItem.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Core.Constants.EmailTo.NominatedGroup);
			incomingMessage.EM_MessageText =
"B  58123456789E          ACE TEST IMPORTER 1                                    " +
"SC1N11HKPAAPLUS300055934        TITANIC                2 58201110120230508 N    " +
"ES197H AI SHIPMENT ON HOLD.CONTACT 5555555555     X20230504639873               " +
"Y  58123456789E          ACE TEST IMPORTER 1";
			incomingMessage.EM_MessageNum = "26";
			Factory.Save();

			var processor = new AESTIRIncomingMessageProcessor();
			processor.Logger = new BatchProcessor.LoggingInformation();
			processor.ExecuteBatch();
			incomingMessage.Reload();
			AssertEquals(EDIMessage.Status.Received, incomingMessage.EM_Status);
			AssertEquals(entry, incomingMessage.EM_LinkedObject);
			AssertEquals(EM_MessageSubTypeList.Codes.SEDAdd, incomingMessage.EM_MessageSubType);
		}

		public void TestUpdateDispositionCodes()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

			var entry01 = declaration.ActiveEntryHeaders.AddNew();
			entry01.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.Export;
			entry01.EntryNumber = "S40002510";

			var cusDisposition = entry01.AESCusDispositions.AddNew();
			cusDisposition.CDI_Sequence = 1;
			cusDisposition.CDI_StatusKey = "F";
			cusDisposition.CDI_StatusDate = ZDateTime.Today;
			cusDisposition.CDI_Status = "123";

			var entryLine11 = entry01.AllEntryLines.AddNew();
			entryLine11.CL_LineNumber = 3;

			cusDisposition = entryLine11.AESCusDispositions.AddNew();
			cusDisposition.CDI_Sequence = 1;
			cusDisposition.CDI_StatusKey = "F";
			cusDisposition.CDI_StatusDate = ZDateTime.Today;
			cusDisposition.CDI_Status = "123";

			var entryLine12 = entry01.AllEntryLines.AddNew();
			entryLine12.CL_LineNumber = 2;

			var entryLine13 = entry01.AllEntryLines.AddNew();
			entryLine13.CL_LineNumber = 1;

			USCustomsDataRegistry.Instance.AESSendNotificationsItem.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Core.Constants.EmailTo.StaffMember);
			outgoingMessage.EM_LinkedObject = entry01;
			incomingMessage.EM_MessageText =
"B  60612456712E          US EXPORTER NAME                                       " +
"SC1N11AUCAUNKNB00001001        ABAI YUN HE               60267270420081101 N    " +
"ES1066    FILING OPT IND MUST BE 2 OR 4                                         " +
"SC2N11AUCAUNKNS40002510        ABAI YUN HE               60267270420081101 N    " +
"ES1970 RF SHIPMENT REJECTED; RESOLVE & RETRANSMIT                               " +
"CL1OS 0001OTHER BREATHING APPLIANCES AND GAS MASKS AND 0000000000 AC33D        1" +
"ES197H  I SHIPMENT OH HOLD                        X20200504399473               " +
"CL29020002000NO 00000100000000010000   00000000000000001000     NLR             " +
"ES198H  I SHIPMENT OH HOLD                        X20200504399473               " +
"ES199H AI SHIPMENT OH HOLD                        X20200504399473               " +
"Y  60612456712E          US EXPORTER NAME";
			Factory.Save();

			AssertEquals(1, entry01.AESCusDispositions.Count);
			AssertEquals(1, entryLine11.AESCusDispositions.Count);

			var processor = new AESTIRIncomingMessageProcessor();
			processor.Logger = new BatchProcessor.LoggingInformation();
			processor.ExecuteBatch();

			var newFactory = new BusinessObjectFactory();
			var reLoadEntry = newFactory.Load<CusEntryHeader>(entry01.PK);
			var reLoadEntryLine01 = newFactory.Load<CusEntryLine>(entryLine11.PK);
			var reLoadEntryLine02 = newFactory.Load<CusEntryLine>(entryLine12.PK);
			var reLoadEntryLine03 = newFactory.Load<CusEntryLine>(entryLine13.PK);

			var messageTime = incomingMessage.EM_MessageDateTime;
			AssertEquals(3, reLoadEntry.AESCusDispositions.Count);
			var dispositionCode = reLoadEntry.AESCusDispositions.Where(x => x.CDI_Status == "066").FirstOrDefault();
			AssertEquals("Sequence", (ZShort)1, dispositionCode.CDI_Sequence);
			AssertEquals("Severity Indicator", "Space", dispositionCode.CDI_StatusKey);
			AssertEquals("Message Time", messageTime, dispositionCode.CDI_StatusDate);
			AssertEquals("Parent Table Code", CusEntryHeaderSchema.Constants.Prefix, dispositionCode.CDI_ParentTableCode);
			dispositionCode = reLoadEntry.AESCusDispositions.Where(x => x.CDI_Status == "970").FirstOrDefault();
			AssertEquals("Sequence", (ZShort)2, dispositionCode.CDI_Sequence);
			AssertEquals("Severity Indicator", "F", dispositionCode.CDI_StatusKey);
			AssertEquals("Message Time", messageTime, dispositionCode.CDI_StatusDate);
			AssertEquals("Parent Table Code", CusEntryHeaderSchema.Constants.Prefix, dispositionCode.CDI_ParentTableCode);
			dispositionCode = reLoadEntry.AESCusDispositions.Where(x => x.CDI_Status == "99H").FirstOrDefault();
			AssertEquals("Sequence", (ZShort)3, dispositionCode.CDI_Sequence);
			AssertEquals("Severity Indicator", "I", dispositionCode.CDI_StatusKey);
			AssertEquals("Message Time", messageTime, dispositionCode.CDI_StatusDate);
			AssertEquals("Parent Table Code", CusEntryHeaderSchema.Constants.Prefix, dispositionCode.CDI_ParentTableCode);

			AssertEquals(0, reLoadEntryLine01.AESCusDispositions.Count);
			AssertEquals(0, reLoadEntryLine02.AESCusDispositions.Count);

			AssertEquals(2, reLoadEntryLine03.AESCusDispositions.Count);
			dispositionCode = reLoadEntryLine03.AESCusDispositions.Where(x => x.CDI_Status == "97H").FirstOrDefault();
			AssertEquals("Sequence", (ZShort)1, dispositionCode.CDI_Sequence);
			AssertEquals("Severity Indicator", "I", dispositionCode.CDI_StatusKey);
			AssertEquals("Message Time", messageTime, dispositionCode.CDI_StatusDate);
			AssertEquals("Parent Table Code", CusEntryLineSchema.Constants.Prefix, dispositionCode.CDI_ParentTableCode);
			dispositionCode = reLoadEntryLine03.AESCusDispositions.Where(x => x.CDI_Status == "98H").FirstOrDefault();
			AssertEquals("Sequence", (ZShort)2, dispositionCode.CDI_Sequence);
			AssertEquals("Severity Indicator", "I", dispositionCode.CDI_StatusKey);
			AssertEquals("Message Time", messageTime, dispositionCode.CDI_StatusDate);
			AssertEquals("Parent Table Code", CusEntryLineSchema.Constants.Prefix, dispositionCode.CDI_ParentTableCode);
		}

		#region Implementation

		ZString HtmlEncode(ZString data)
		{
			return WebUtility.HtmlEncode(data).Replace("\r\n", "<br>").Replace("\n", "<br>");
		}

		JobDeclaration declaration;
		JobComInvoiceHeader invoice;
		CusEntryHeader entry;
		AESTIREDIMessage outgoingMessage;
		AESTIREDIMessage incomingMessage;
		GlbStaff currentUser;
		GlbGroup groupZZ1;
		GlbStaff staffZ1;
		GlbStaff staffZ2;
		OrgAddress address2;
		OrgHeader uSPPI;

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

			uSPPI = Factory.NewWithValidTestData<OrgHeader>();
			uSPPI.OH_FullName = "ABC Exporters";
			uSPPI.OH_RL_NKClosestPort = "USNYK";
			uSPPI.Addresses.AddNewMainAddress();
			uSPPI.MainAddress.OA_Address1 = "1592 Main St.";
			uSPPI.MainAddress.OA_Address2 = "Queens";
			uSPPI.MainAddress.OA_City = "New York";
			uSPPI.MainAddress.OA_Phone = "+1 (273) 5495200";
			uSPPI.MainAddress.OA_State = "NY";
			uSPPI.MainAddress.OA_PostCode = "17453";

			address2 = uSPPI.Addresses.AddNew();
			address2.OA_Address1 = "1593 Main St.";
			address2.OA_Address2 = "Kings";
			address2.OA_City = "Chicago";
			address2.OA_Phone = "+1 (273) 5495201";
			address2.OA_State = "IL";
			address2.OA_PostCode = "17454";
			address2.OA_CompanyNameOverride = "Overridden Company";

			Factory.Save();
			USCustomsDataRegistry.Instance.AESSendNotificationsToGroupItem.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, groupZZ1.PK.ToGuid());

			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

			invoice = declaration.Invoices.AddNew();
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

			var mock = Factory.NewMoq<AESTIREDIMessage>();
			mock.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");

			outgoingMessage = mock.Object;
			outgoingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsExport;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_Status = EDIMessage.Status.Sent;
			outgoingMessage.EM_MessageNum = "2147483647";
			outgoingMessage.EM_MessageType = ApplicationIdentifierCodeList.AES.CommodityShipment;
			outgoingMessage.EM_MessageSubType = EM_MessageSubTypeList.Codes.SEDAdd;
			outgoingMessage.EM_LinkedObject = entry;
			outgoingMessage.EM_EI = interchange.PK;

			incomingMessage = Factory.New<AESTIREDIMessage>();
			incomingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsExport;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_MessageType = ApplicationIdentifierCodeList.AES.CommodityShipmentResponse;
			incomingMessage.EM_MessageNum = "2147483647";

			Factory.Save();
		}

		#endregion
	}
}
