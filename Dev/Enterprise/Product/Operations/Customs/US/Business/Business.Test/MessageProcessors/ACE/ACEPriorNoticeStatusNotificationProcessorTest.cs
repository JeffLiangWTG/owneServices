using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Moq.Protected;

namespace Enterprise.Customs.US.Business.MessageProcessors.Testing
{
	public class ACEPriorNoticeStatusNotificationProcessorTest : ABIProcessorTest<ACEPriorNoticeStatusNotificationProcessor, AABIOutputA, AABIOutputB, AABIOutputY>
	{
		protected override void EndToEndCore()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			declaration.US_EnableENS = true;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.Messages.Add(GetPNMessage("HYEDUSCMT_165024"));
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();

			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			var fdaLine1 = invoiceLine2.ACE_FDALines.AddNew();
			var fdaLine2 = invoiceLine2.ACE_FDALines.AddNew();
			fdaLine2.US_PNC = "051456566";

			var fdaLine3 = invoiceLine2.ACE_FDALines.AddNew();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			var incomingMessage = Factory.New<MQEDIMessage>();
			incomingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_Status = "QUE";
			incomingMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.PriorNoticeStatusNotification;

			incomingMessage.EM_MessageText =
"B  3901SV9PO                                                                    " +
"PO10 AAWBAY  00145012314                                    APLU0140            " +
"PO60111715122480NO INBOND NUMBER SUBMITTED                                      " +
"PO70FDAFOO                                          0414002003                  " +
"PO71013004005012331116151231109115                                              " +
"PO72SOME ADDITIONAL INFORMATION FOR PN CONFIRMATION AND REVIEW                  " +
"Y  3901SV9PO";
			Factory.Save();

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();

			incomingMessage.Reload();
			AssertEquals("Should be processed successfully", "RCV", incomingMessage.EM_Status);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertContains("No Declaration Job found for Bill Issuer Code 'AY' and Master Bill Number '00145012314'", email.Body);

			declaration.JE_MasterBill = "00145012314";
			declaration.PrimaryMasterBill.US_UI_NKBillIssuerSCAC = "AY";

			incomingMessage = Factory.New<MQEDIMessage>();
			incomingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_Status = "QUE";
			incomingMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.PriorNoticeStatusNotification;

			incomingMessage.EM_MessageText =
"B  3901SV9PO                                                                    " +
"PO10 AAWBAY  00145012314                                    APLU0140            " +
"PO60111715123001PN CONFIRMATION NUMBER CANCELLED                                " +
"PO60111715122480NO INBOND NUMBER SUBMITTED                                      " +
"PO70FDAFOO                                          0414002003                  " +
"PO7101300400501233111615123110119127                                            " +
"PO72SOME ADDITIONAL INFORMATION FOR PN CONFIRMATION AND REVIEW                  " +
"Y  3901SV9PO";

			Factory.Save();

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();

			email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertContains("<b>Comments to Trade from PGA:</b><br />SOME ADDITIONAL INFORMATION FOR PN CONFIRMATION AND REVIEW<br />", email.Body);

			incomingMessage.Reload();
			AssertEquals(EM_MessageSubTypeList.Codes.FDAPriorNoticeStatus, incomingMessage.EM_MessageSubType);

			fdaLine2.Reload();
			AssertEquals(ZString.Empty, fdaLine2.US_PNC);
		}

		public void TestPGAStatusDetailWithOutEntryHeader()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;

			declaration.JE_MasterBill = "0021210";
			declaration.PrimaryMasterBill.US_UI_NKBillIssuerSCAC = "GPHQ";
			Factory.Save();

			AssertEquals(0, declaration.ActiveEntryHeaders.Count);

			var incomingMessage = Factory.New<MQEDIMessage>();
			incomingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_Status = "QUE";
			incomingMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.PriorNoticeStatusNotification;

			incomingMessage.EM_MessageText =
"B003910SV9PO                                               HYEDUSCMT_176600     " +
"PO10 ABOLGPHQ0021210                                       RGPHQ0130A00000074523" +
"PO70FDAFOO                                          0414001                     " +
"PO71                          110                                               " +
"Y  3910SV9PO00000";

			Factory.Save();

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();

			var reLoadJob = Factory.Load<JobDeclaration>(declaration.PK);
			Assert(reLoadJob.OGADispositionCodes.Count > 0);
			var dispositionCode = reLoadJob.OGADispositionCodes[0];
			Assert(dispositionCode.OGADispositionDetails.Count > 0);

			var dispositionDetails = dispositionCode.OGADispositionDetails[0];
			AssertEquals(1, dispositionDetails.SubReasonCodes.Count());
			AssertEquals("110", dispositionDetails.SubReasonCodes.FirstOrDefault());
		}

		public void TestDeclarationBasedMessageNum()
		{
			var declaration = Factory.New<JobDeclaration>();

			var mock = Factory.NewMoq<MQEDIMessage>();
			mock.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");

			var outgoing = mock.Object;
			outgoing.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			outgoing.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoing.EM_Status = "SNT";
			outgoing.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.PriorNotice;
			declaration.US_EnableENS = true;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.JE_MasterBill = "17661525623";
			declaration.PrimaryMasterBill.US_UI_NKBillIssuerSCAC = "AY";
			outgoing.EM_MessageNum = "HYEDUSCMT_165024";
			declaration.Messages.Add(outgoing);

			var incomingMessage = CreateIncomingMessage("HYEDUSCMT_165024");

			Factory.Save();

			incomingMessage.EM_MessageText =
"B004701D17PO                                               HYEDUSCMT_165024     " +
"PO10 AAWB    17661525623                                   REK  0140            " +
"PO70FDAFOO                                           01 001                     " +
"Y  4701D17PO00000";

			Factory.Save();

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();

			incomingMessage.Reload();
			AssertEquals("Should be processed successfully", "RCV", incomingMessage.EM_Status);
			AssertEquals(declaration, incomingMessage.EM_LinkedObject);
			incomingMessage.ClearAllNotifications();
		}

		MQEDIMessage CreateIncomingMessage(ZString messageNum)
		{
			var message = Factory.New<MQEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = "QUE";
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.PriorNoticeStatusNotification;

			message.EM_MessageNum = messageNum;
			Factory.Save();

			return message;
		}

		public void TestSetPNCNumberWithPGALinenumber()
		{
			//declaration 1
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.JE_MasterBill = "00145012314";
			declaration.PrimaryMasterBill.US_UI_NKBillIssuerSCAC = "AY";
			declaration.Messages.Add(GetPNMessage("HYEDUSCMT_165024"));

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			var fdaLine11 = invoiceLine1.ACE_FDALines.AddNew();
			fdaLine11.US_LineNo = 1;
			fdaLine11.US_ProgramCode = "FOO";
			var fdaLine21 = invoiceLine1.ACE_FDALines.AddNew();
			fdaLine21.US_ProgramCode = "FOO";
			fdaLine21.US_LineNo = 2;
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			var fdaLine31 = invoiceLine2.ACE_FDALines.AddNew();
			fdaLine31.US_ProgramCode = "FOO";
			fdaLine31.US_LineNo = 3;

			//PN status notification message
			var incomingMessage = Factory.New<MQEDIMessage>();
			incomingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_Status = "QUE";
			incomingMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.PriorNoticeStatusNotification;

			incomingMessage.EM_MessageText =
"B002704906PO                                               HYEDUSCMT_165024     " +
"PO10 AAWBAY  00145012314                                    APLU0140            " +
"PO70FDAFOO                                          01  001                     " +
"PO7101166531159874                                                              " +
"PO70FDAFOO                                          01  002                     " +
"PO7101166531159885                                                              " +
"PO70FDAFOO                                          01  003                     " +
"PO7101166531159896                                                              " +
"Y  2704906PO00000";

			Factory.Save();

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();

			incomingMessage.Reload();
			AssertEquals("Should be processed successfully", "RCV", incomingMessage.EM_Status);
			AssertEquals(declaration, incomingMessage.EM_LinkedObject);

			fdaLine11.Reload();
			AssertEquals(declaration.InvoiceLines[0].ACE_FDALines[0].US_PNC, "166531159874");
			fdaLine21.Reload();
			AssertEquals(declaration.InvoiceLines[0].ACE_FDALines[1].US_PNC, "166531159885");
			fdaLine31.Reload();
			AssertEquals(declaration.InvoiceLines[1].ACE_FDALines[0].US_PNC, "166531159896");
		}

		public void TestLoadMatchingBill()
		{
			//declaration 1
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.JE_MasterBill = "00145012314";
			declaration.PrimaryMasterBill.US_UI_NKBillIssuerSCAC = "AY";

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();

			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			var fdaLine1 = invoiceLine2.ACE_FDALines.AddNew();
			var fdaLine2 = invoiceLine2.ACE_FDALines.AddNew();
			fdaLine2.US_PNC = "051456566";

			var fdaLine3 = invoiceLine2.ACE_FDALines.AddNew();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			declaration.Messages.Add(GetPNMessage("HYEDUSCMT_165024"));

			//declaration 2
			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration2.US_EnableENS = true;
			declaration2.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration2.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration2.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration2.JE_MasterBill = "00145012314";
			declaration2.PrimaryMasterBill.US_UI_NKBillIssuerSCAC = "AY";
			declaration2.Messages.Add(GetPNMessage("HYEDUSCMT_165025"));

			//declaration 3 Recycled
			FreightDataRegistry.Instance.MAWBRecyclePeriod.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 2);
			var declaration3 = Factory.New<JobDeclaration>();
			declaration3.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration3.US_EnableENS = true;
			declaration3.JE_TransportMode = Core.Constants.TransportModes.Air;
			declaration3.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration3.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration3.JE_MasterBill = "00145012314";
			declaration3.PrimaryMasterBill.US_UI_NKBillIssuerSCAC = "AY";
			declaration3.JE_SystemCreateTimeUtc = ZDateTime.Today.AddMonths(-4);
			declaration3.Messages.Add(GetPNMessage("HYEDUSCMT_165026"));

			//declaration 4
			var declaration4 = Factory.New<JobDeclaration>();
			declaration4.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration4.US_EnableENS = true;
			declaration4.JE_TransportMode = Core.Constants.TransportModes.Air;
			declaration4.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration4.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration4.JE_MasterBill = "00145012314";
			declaration4.PrimaryMasterBill.US_UI_NKBillIssuerSCAC = "BB";
			declaration4.Messages.Add(GetPNMessage("HYEDUSCMT_165027"));

			//PN status notification message
			var incomingMessage = Factory.New<MQEDIMessage>();
			incomingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_Status = "QUE";
			incomingMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.PriorNoticeStatusNotification;

			incomingMessage.EM_MessageText =
"B  3901SV9PO                                                                    " +
"PO10 AAWBAY  00145012314                                    APLU0140            " +
"PO60111715122480NO INBOND NUMBER SUBMITTED                                      " +
"PO70FDAFOO                                          0414002003                  " +
"PO71013004005012331116151231109115                                              " +
"PO72SOME ADDITIONAL INFORMATION FOR PN CONFIRMATION AND REVIEW                  " +
"Y  3901SV9PO";
			Factory.Save();

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();

			incomingMessage.Reload();
			AssertEquals("Should be processed successfully", "RCV", incomingMessage.EM_Status);
			AssertEquals(declaration, incomingMessage.EM_LinkedObject);
		}

		public void TestDISDocumentTypeDesc()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USDISFormList, "DIS Form List");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USDISFormList, "AMS01", "AMS_FOREIGN_GOVT_EXPORT", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			declaration.US_EnableENS = true;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.JE_MasterBill = "00145012314";
			declaration.PrimaryMasterBill.US_UI_NKBillIssuerSCAC = "AY";
			declaration.Messages.Add(GetPNMessage("HYEDUSCMT_165024"));
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var fdaLine = invoiceLine.ACE_FDALines.AddNew();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			var incomingMessage = Factory.New<MQEDIMessage>();
			incomingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_Status = "QUE";
			incomingMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.PriorNoticeStatusNotification;

			incomingMessage.EM_MessageText =
"B  3901SV9PO                                                                    " +
"PO10 AAWBAY  00145012314                                    APLU0140            " +
"PO60111715123001PN CONFIRMATION NUMBER CANCELLED                AMS01           " +
"PO70FDAFOO                                          0414002003                  " +
"PO7101300400501233111615123110119127                                            " +
"PO72SOME ADDITIONAL INFORMATION FOR PN CONFIRMATION AND REVIEW                  " +
"Y  3901SV9PO";

			Factory.Save();

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();

			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertContains("<tr><td>Document Type</td><td>AMS01 - AMS_FOREIGN_GOVT_EXPORT</td></tr>", email.Body);
		}

		public void TestSetPNCNumberForENTStandAlonePriorNotice()
		{
			DeclarationTestHelper.SetEntryFilerCode("SV9");
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableSPN = true;
			declaration.US_SPNIDType = StandAlonePriorNoticeIDTypeList.Codes.ENT;
			declaration.ImportEntryNumber = "70043367";
			declaration.Messages.Add(GetPNMessage("HYEDUSCMT_165024"));

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			var fdaLine11 = invoiceLine1.ACE_FDALines.AddNew();
			fdaLine11.US_LineNo = 1;
			fdaLine11.US_ProgramCode = "FOO";
			var fdaLine21 = invoiceLine1.ACE_FDALines.AddNew();
			fdaLine21.US_ProgramCode = "FOO";
			fdaLine21.US_LineNo = 2;
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			var fdaLine31 = invoiceLine2.ACE_FDALines.AddNew();
			fdaLine31.US_ProgramCode = "FOO";
			fdaLine31.US_LineNo = 3;

			var incomingMessage = Factory.New<MQEDIMessage>();
			incomingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_Status = "QUE";
			incomingMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.PriorNoticeStatusNotification;

			incomingMessage.EM_MessageText =
"B002704906PO                                               HYEDUSCMT_XXXXXX     " +
"PO10 AENTSV9 70043367                                       APLU0140A00000089005" +
"PO70FDAFOO                                          01  001                     " +
"PO7101166531159874                                                              " +
"PO70FDAFOO                                          01  002                     " +
"PO7101166531159885                                                              " +
"PO70FDAFOO                                          01  003                     " +
"PO7101166531159896                                                              " +
"Y  2704906PO00000";

			Factory.Save();

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();

			incomingMessage.Reload();
			AssertEquals("Should be processed successfully", "RCV", incomingMessage.EM_Status);
			AssertEquals(declaration, incomingMessage.EM_LinkedObject);
			AssertEquals(3, declaration.OGADispositionCodes.Count);
			AssertEquals("A00000089005", declaration.OGADispositionCodes[0].US_EnvelopeNumber);
			AssertEquals("001", declaration.OGADispositionCodes[0].US_OGADispositionBeginningOGALine);
			AssertEquals("A00000089005", declaration.OGADispositionCodes[1].US_EnvelopeNumber);
			AssertEquals("002", declaration.OGADispositionCodes[1].US_OGADispositionBeginningOGALine);
			AssertEquals("A00000089005", declaration.OGADispositionCodes[2].US_EnvelopeNumber);
			AssertEquals("003", declaration.OGADispositionCodes[2].US_OGADispositionBeginningOGALine);

			fdaLine11.Reload();
			AssertEquals(declaration.InvoiceLines[0].ACE_FDALines[0].US_PNC, "166531159874");
			fdaLine21.Reload();
			AssertEquals(declaration.InvoiceLines[0].ACE_FDALines[1].US_PNC, "166531159885");
			fdaLine31.Reload();
			AssertEquals(declaration.InvoiceLines[1].ACE_FDALines[0].US_PNC, "166531159896");
		}

		public void TestSetPNCNumberForMultipleBill()
		{
			//declaration 1
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.JE_MasterBill = "00145012314";
			declaration.PrimaryMasterBill.US_UI_NKBillIssuerSCAC = "AY";
			declaration.Messages.Add(GetPNMessage("HYEDUSCMT_165024"));

			var masterBill = declaration.Bills.CreatePrimaryBill(BillTypeList.Codes.MasterBill);
			masterBill.CU_BillNum = "MB1234567";

			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_CU_RelatedHouseBill = declaration.PrimaryMasterBill.PK;
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			var fdaLine11 = invoiceLine1.ACE_FDALines.AddNew();
			fdaLine11.US_LineNo = 1;
			fdaLine11.US_ProgramCode = "FOO";
			var fdaLine21 = invoiceLine1.ACE_FDALines.AddNew();
			fdaLine21.US_ProgramCode = "FOO";
			fdaLine21.US_LineNo = 2;

			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_CU_RelatedHouseBill = masterBill.PK;
			var invoiceLine2 = invoice2.InvoiceLines.AddNew();
			var fdaLine31 = invoiceLine2.ACE_FDALines.AddNew();
			fdaLine31.US_ProgramCode = "FOO";
			fdaLine31.US_LineNo = 1;

			//PN status notification message
			var incomingMessage1 = Factory.New<MQEDIMessage>();
			incomingMessage1.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			incomingMessage1.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage1.EM_Status = "QUE";
			incomingMessage1.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.PriorNoticeStatusNotification;

			incomingMessage1.EM_MessageText =
"B002704906PO                                               HYEDUSCMT_XXXXXX     " +
"PO10 AAWBAY  00145012314                                    APLU0140A00000089004" +
"PO70FDAFOO                                          01  001                     " +
"PO7101166531159874                                                              " +
"PO70FDAFOO                                          01  002                     " +
"PO7101166531159885                                                              " +
"Y  2704906PO00000";

			Factory.Save();

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();

			incomingMessage1.Reload();
			AssertEquals("Should be processed successfully", "RCV", incomingMessage1.EM_Status);
			AssertEquals(declaration, incomingMessage1.EM_LinkedObject);
			AssertEquals(2, declaration.OGADispositionCodes.Count);
			AssertEquals("A00000089004", declaration.OGADispositionCodes[0].US_EnvelopeNumber);
			AssertEquals("001", declaration.OGADispositionCodes[0].US_OGADispositionBeginningOGALine);
			AssertEquals("A00000089004", declaration.OGADispositionCodes[1].US_EnvelopeNumber);
			AssertEquals("002", declaration.OGADispositionCodes[1].US_OGADispositionBeginningOGALine);

			fdaLine11.Reload();
			AssertEquals("PNC should be updated", declaration.InvoiceLines[0].ACE_FDALines[0].US_PNC, "166531159874");
			fdaLine21.Reload();
			AssertEquals("PNC should be updated", declaration.InvoiceLines[0].ACE_FDALines[1].US_PNC, "166531159885");
			fdaLine31.Reload();
			AssertEquals("PNC shouldn't be updated", declaration.InvoiceLines[1].ACE_FDALines[0].US_PNC, ZString.Empty);

			declaration.Messages.Add(GetPNMessage("HYEDUSCMT_165025"));

			var incomingMessage2 = Factory.New<MQEDIMessage>();
			incomingMessage2.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			incomingMessage2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage2.EM_Status = "QUE";
			incomingMessage2.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.PriorNoticeStatusNotification;

			incomingMessage2.EM_MessageText =
"B002704906PO                                               HYEDUSCMT_XXXXXX     " +
"PO10 AAWB    MB1234567                                      APLU0140A00000089005" +
"PO70FDAFOO                                          01  001                     " +
"PO7101166531159896                                                              " +
"Y  2704906PO00000";

			Factory.Save();

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();

			incomingMessage2.Reload();
			AssertEquals("Should be processed successfully", "RCV", incomingMessage2.EM_Status);
			AssertEquals(declaration, incomingMessage2.EM_LinkedObject);

			declaration.OGADispositionCodes.Load();
			AssertEquals(3, declaration.OGADispositionCodes.Count);
			AssertEquals("A00000089004", declaration.OGADispositionCodes[0].US_EnvelopeNumber);
			AssertEquals("001", declaration.OGADispositionCodes[0].US_OGADispositionBeginningOGALine);
			AssertEquals("A00000089004", declaration.OGADispositionCodes[1].US_EnvelopeNumber);
			AssertEquals("002", declaration.OGADispositionCodes[1].US_OGADispositionBeginningOGALine);
			AssertEquals("A00000089005", declaration.OGADispositionCodes[2].US_EnvelopeNumber);
			AssertEquals("001", declaration.OGADispositionCodes[2].US_OGADispositionBeginningOGALine);

			fdaLine11.Reload();
			AssertEquals("PNC shouldn't be updated", declaration.InvoiceLines[0].ACE_FDALines[0].US_PNC, "166531159874");
			fdaLine21.Reload();
			AssertEquals("PNC shouldn't be updated", declaration.InvoiceLines[0].ACE_FDALines[1].US_PNC, "166531159885");
			fdaLine31.Reload();
			AssertEquals("PNC should be updated", declaration.InvoiceLines[1].ACE_FDALines[0].US_PNC, "166531159896");
		}

		public void TestSetPNCNumberForFTZStandAlonePriorNotice()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = US.Business.JobMessageTypeList.Codes.FTZ;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			declaration.US_F_AdmissionType = FTZAdmissionTypeCodeList.Codes.RegularAdmission;
			declaration.US_EnableSPN = true;
			declaration.US_F_PNMode = PriorNoticeModeCodeList.Codes.P;
			declaration.FTZAdmissionNumber = "2140000|17|00000001";
			declaration.US_SPNIDType = StandAlonePriorNoticeIDTypeList.Codes.FTZ;
			declaration.Messages.Add(GetPNMessage("HYEDUSCMT_165024"));

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			var fdaLine11 = invoiceLine1.ACE_FDALines.AddNew();
			fdaLine11.US_LineNo = 1;
			fdaLine11.US_ProgramCode = "FOO";
			var fdaLine21 = invoiceLine1.ACE_FDALines.AddNew();
			fdaLine21.US_ProgramCode = "FOO";
			fdaLine21.US_LineNo = 2;
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			var fdaLine31 = invoiceLine2.ACE_FDALines.AddNew();
			fdaLine31.US_ProgramCode = "FOO";
			fdaLine31.US_LineNo = 3;

			var incomingMessage = Factory.New<MQEDIMessage>();
			incomingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_Status = "QUE";
			incomingMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.PriorNoticeStatusNotification;

			incomingMessage.EM_MessageText =
"B002704906PO                                               HYEDUSCMT_XXXXXX     " +
"PO10 AFTZ    21400001700000001                              APLU0140A00000089005" +
"PO70FDAFOO                                          01  001                     " +
"PO7101166531159874                                                              " +
"PO70FDAFOO                                          01  002                     " +
"PO7101166531159885                                                              " +
"PO70FDAFOO                                          01  003                     " +
"PO7101166531159896                                                              " +
"Y  2704906PO00000";

			Factory.Save();

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();

			incomingMessage.Reload();
			AssertEquals("Should be processed successfully", "RCV", incomingMessage.EM_Status);
			AssertEquals(declaration, incomingMessage.EM_LinkedObject);
			AssertEquals(3, declaration.OGADispositionCodes.Count);
			AssertEquals("A00000089005", declaration.OGADispositionCodes[0].US_EnvelopeNumber);
			AssertEquals("001", declaration.OGADispositionCodes[0].US_OGADispositionBeginningOGALine);
			AssertEquals("A00000089005", declaration.OGADispositionCodes[1].US_EnvelopeNumber);
			AssertEquals("002", declaration.OGADispositionCodes[1].US_OGADispositionBeginningOGALine);
			AssertEquals("A00000089005", declaration.OGADispositionCodes[2].US_EnvelopeNumber);
			AssertEquals("003", declaration.OGADispositionCodes[2].US_OGADispositionBeginningOGALine);

			fdaLine11.Reload();
			AssertEquals(declaration.InvoiceLines[0].ACE_FDALines[0].US_PNC, "166531159874");
			fdaLine21.Reload();
			AssertEquals(declaration.InvoiceLines[0].ACE_FDALines[1].US_PNC, "166531159885");
			fdaLine31.Reload();
			AssertEquals(declaration.InvoiceLines[1].ACE_FDALines[0].US_PNC, "166531159896");
		}

		MQEDIMessage GetPNMessage(ZString messageNum)
		{
			var mock = Factory.NewMoq<MQEDIMessage>();
			mock.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");

			var outgoing = mock.Object;
			outgoing.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			outgoing.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoing.EM_Status = "SNT";
			outgoing.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.PriorNotice;
			outgoing.EM_MessageNum = messageNum;
			return outgoing;
		}

		protected override ZInt EmailsExpectedAtCompletionOfEndToEndTest
		{
			get { return 1; }
		}
	}
}
