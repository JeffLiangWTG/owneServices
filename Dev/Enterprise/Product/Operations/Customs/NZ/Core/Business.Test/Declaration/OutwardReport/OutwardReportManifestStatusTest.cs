using System;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.NZ.Business.Express;
using Enterprise.Customs.NZ.Business.MessageBuilders.OutwardReport;
using Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.NZ.Business.Declaration.OutwardReport.Testing
{
	using System.Threading;
	using CargoWise.Integration;
	using Enterprise.Customs.Common;
	using Enterprise.Customs.Common.NZ;
	using Enterprise.Customs.NZ.Registry;
	using NUnit.Framework;
	using CusEntryNumber = CusEntryNumber;

	[TestedType(typeof(OutwardReportManifestStatus))]
	public class OutwardReportManifestStatusTest : Customs.Business.Testing.CustomsManifestStatusTest
	{
		protected class TestManifestStatusClass : OutwardReportManifestStatus
		{
			public TestManifestStatusClass(ForwardingConsol consol)
				: base(consol)
			{
			}

			protected override void OnMessageSent(ISendsMessagesToCustoms sender)
			{
				throw new ZSaveException(new ZDataException(new ApplicationException("I am testing"), ((INeedRow)Consol).Row, Db.Connection), Factory);
			}

			public bool SaveExceptionHandled;
			protected override void HandleSaveException(ZSaveException ex)
			{
				SaveExceptionHandled = true;
			}
		}

		public void TestTablePrefix()
		{
			var consol = Factory.New<ForwardingConsol>();
			var status = new OutwardReportManifestStatus(consol);
			AssertEquals("JK", status.TablePrefix);

			var seaCargo = Factory.New<CusSCAOceanBill>();
			status = new OutwardReportManifestStatus(seaCargo);
			AssertEquals("CB", status.TablePrefix);
		}

		public void TestMessagingMode()
		{
			AssertEquals(MsgTransportList.Codes.TSW, Status.E2_MessagingModeDesc);
			Status.E2_MessagingMode = MsgTransportList.Codes.TSW;
			AssertEquals(MsgTransportList.Codes.TSW, Status.GetSystemDefinedValue<ZString>(NZCMessage.MsgTransModeConstant));
		}

		public void TestEDITransmitDate()
		{
			ZDateTime first = Status.EDITransmitDate;
			Thread.Sleep(1000);
			ZDateTime second = Status.EDITransmitDate;
			AssertNotEquals("2 subsequent pollings of EDITransmitDate should be different, using ZDateTime.Now", first, second);
			EDIMessage message = Status.MessagesIncludingInterchangeRejections.AddNew();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			ZDateTime third = Status.EDITransmitDate;
			AssertEquals("Status.EDITransmitDate", message.EM_SystemCreateTimeUtc, third);
			Thread.Sleep(1000);
			AssertEquals("Status.EDITransmitDate", third, Status.EDITransmitDate);
		}

		public void TestDeclareManifestCatchSaveException()
		{
			SendsMessagesToCustomsShutterUpperer sender = new SendsMessagesToCustomsShutterUpperer(false);
			SetUpValidAIRConsol();
			TestManifestStatusClass testStatus = new TestManifestStatusClass(Consol);
			testStatus.DeclareManifest(sender);
			AssertEquals("Exception handled", true, testStatus.SaveExceptionHandled);
		}

		public void TestDeclareManifest()
		{
			SendsMessagesToCustomsShutterUpperer sender = new SendsMessagesToCustomsShutterUpperer(false);
			AssertEquals("No message yet", 0, Consol.Messages.Count);
			SetUpValidAIRConsol();
			Status.DeclareManifest(sender);
			AssertNull("Users have not been warned", sender.InvalidOperationText);
			AssertEquals("One message is generated", 1, Consol.Messages.Count);
		}

		public void TestOutwardReportSingularity()
		{
			SendsMessagesToCustomsShutterUpperer sender = new SendsMessagesToCustomsShutterUpperer(false);
			SetUpValidAIRConsol();

			CusEntryNumber anotherEntryNumber = null;
			if (Status.E2_CustomsEntryNumber.IsEmpty)
			{
				using (GetFactoryIsolater(Factory))
				{
					var anotherFactory = Factory.CreateNewFactory();
					using (GetFactoryIsolater(anotherFactory))
					{
						anotherEntryNumber = anotherFactory.New<CusEntryNumber>();
						anotherEntryNumber.CE_EntryStatus = "AWT";
						anotherEntryNumber.CE_EntryType = CusEntryNumberTypeList.Codes.OutwardReportNumber;
						anotherEntryNumber.CE_ParentID = Consol.PK;
						anotherEntryNumber.CE_ParentTable = ForwardingConsol.Schema.TableName;
						anotherEntryNumber.CE_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
						anotherFactory.Save();
					}
				}
			}

			Status.DeclareManifest(sender);
			var resultingCusEntryNumbers = Factory.Load(typeof(CusEntryNumber), CusEntryNumber.GetEntryNumberFilter(Consol));
			AssertEquals(1, resultingCusEntryNumbers.Length);
			AssertEquals(anotherEntryNumber?.PK, resultingCusEntryNumbers[0].PK);
		}

		public void TestWithdrawManifest()
		{
			SendsMessagesToCustomsShutterUpperer sender = new SendsMessagesToCustomsShutterUpperer(false);
			AssertNull("PreCondition: No warning yet", sender.LastWarnings);

			CusEntryNumber entryNumber = Factory.New<CusEntryNumber>();
			entryNumber.CE_EntryType = CusEntryNumberTypeList.Codes.OutwardReportNumber;
			entryNumber.CE_EntryNum = "123456";
			entryNumber.CE_ParentID = Consol.PK;
			entryNumber.CE_ParentTable = Consol.TableName;
			entryNumber.CE_EntryStatus = OutwardReportStatusList.Codes.Cleared;

			AssertEquals("No message yet", 0, Consol.Messages.Count);
			sender.AnswerToContinueWithAction = true;
			SetUpValidAIRConsol();
			Status.WithdrawManifest(sender);
			AssertEquals("One message is generated", 1, Consol.Messages.Count);
		}

		public void TestUsersAreWarnedWhenTheyWithdraw()
		{
			SendsMessagesToCustomsShutterUpperer sender = new SendsMessagesToCustomsShutterUpperer(false);
			AssertNull("PreCondition: No warning yet", sender.LastWarnings);

			CusEntryNumber entryNumber = Factory.New<CusEntryNumber>();
			entryNumber.CE_EntryType = CusEntryNumberTypeList.Codes.OutwardReportNumber;
			entryNumber.CE_EntryNum = "123456";
			entryNumber.CE_ParentID = Consol.PK;
			entryNumber.CE_EntryStatus = OutwardReportStatusList.Codes.Cleared;

			Status.WithdrawManifest(sender);
			AssertEquals("Users are warned", 1, sender.PastMessages.Count);
		}

		public void TestMessagesIncludingInterchangeRejectionsReturnsNZCollection()
		{
			var collection = Status.MessagesIncludingInterchangeRejections;
			var message = collection.AddNew();
			AssertEquals("EDIMessage is typeof(NZCMessage)", typeof(NZCMessage), message.GetType());
		}

		public void TestMessageApplicationCode()
		{
			EDIMessage message1 = Consol.Messages.AddNew();
			message1.EM_ApplicationCode = "XXX";

			EDIMessage message2 = Consol.Messages.AddNew();
			message2.EM_ApplicationCode = EDIInterchange.ApplicationCodes.NewZealandCustoms;

			AssertEquals("Ordered Message contains only one message with the right application code", 1, Status.OrderedMessages.Length);
		}

		public void TestValidateEnvironmentForSendingManifestsForCancelled()
		{
			CusEntryNumber entryNumber = Factory.New<CusEntryNumber>();
			entryNumber.CE_EntryType = CusEntryNumberTypeList.Codes.OutwardReportNumber;
			entryNumber.CE_EntryNum = OutwardReportManifestStatus.InvalidReportNumber;
			entryNumber.CE_ParentID = Consol.PK;
			entryNumber.CE_EntryStatus = OutwardReportStatusList.Codes.Cancelled;

			SendsMessagesToCustomsShutterUpperer s = new SendsMessagesToCustomsShutterUpperer(false);
			Status.DeclareManifest(s);

			Assert("Cannot send a message as it is cancelled", s.InvalidOperationText == "This manifest has been cancelled. You cannot send further messages on this consol.");
		}

		public void TestNewCreateOrReplaceMessageBuilderForInvalidReportNumber()
		{
			CusEntryNumber entryNumber = Factory.New<CusEntryNumber>();
			entryNumber.CE_EntryType = CusEntryNumberTypeList.Codes.OutwardReportNumber;
			entryNumber.CE_EntryNum = OutwardReportManifestStatus.InvalidReportNumber;
			entryNumber.CE_ParentID = Consol.PK;
			entryNumber.CE_EntryStatus = OutwardReportStatusList.Codes.Rejected;

			Customs.Business.MessageBuilders.IManifestMessageBuilder builder2 = Status.NewCreateOrReplaceMessageBuilder();
			MessageBuilder builder = builder2 as MessageBuilder;
			AssertEquals("Next message should be an original", "Original", builder.ManifestMessageTypeCode);
		}

		public void TestCreateOrReplaceMessageTypeForEmptyReportNumber()
		{
			CusEntryNumber entryNumber = Factory.New<CusEntryNumber>();
			entryNumber.CE_EntryType = CusEntryNumberTypeList.Codes.OutwardReportNumber;
			entryNumber.CE_EntryNum = ZString.Empty;
			entryNumber.CE_ParentID = Consol.PK;

			Customs.Business.MessageBuilders.IManifestMessageBuilder builder2 = Status.NewCreateOrReplaceMessageBuilder();
			MessageBuilder builder = builder2 as MessageBuilder;
			AssertEquals("Message type should be an original", "Original", builder.ManifestMessageTypeCode);
		}

		public void TestMessageStatusGetter()
		{
			AssertEquals("Status should be 'Not Sent'", OutwardReportStatusList.Descriptions.NotSent, Status.E2_MessageStatus);

			CusEntryNumber entryNumber = Factory.New<CusEntryNumber>();
			entryNumber.CE_EntryType = CusEntryNumberTypeList.Codes.OutwardReportNumber;
			entryNumber.CE_ParentID = Consol.PK;
			AssertEquals("Status should return 'Acknowledged' where an entry number object exists but has no entry number as yet", OutwardReportStatusList.Descriptions.Acknowledgement, Status.E2_MessageStatus);

			entryNumber.CE_EntryNum = "123456789";
			entryNumber.CE_EntryStatus = OutwardReportStatusList.Codes.Cleared;
			AssertEquals("Status should be 'Cleared'", OutwardReportStatusList.Descriptions.Cleared, Status.E2_MessageStatus);
		}

		public void TestMessageStatusSetter()
		{
			Status.E2_MessageStatus = OutwardReportStatusList.Codes.AwaitingResponse;
			AssertEquals("Consol now has CusEntryNum attached", OutwardReportStatusList.Descriptions.AwaitingResponse, Status.E2_MessageStatus);
		}

		public void TestMessageStatusIsDiscardedIfTransactionFails()
		{
			Status.E2_MessageStatus = OutwardReportStatusList.Codes.AwaitingResponse;
			AssertNotNull("Pre-condition: CusEntryNumber is created", Factory.LoadTop1<CusEntryNumber>(CusEntryNumber.GetEntryNumberFilter(Consol)));

			var transactionParticipant = ((ITransactionParticipant)Factory);
			transactionParticipant.BeginTransactionWithManager().Dispose();
			transactionParticipant.OnAllTransactionsRolledBack();

			AssertEquals("Message status should be discarded", OutwardReportStatusList.Descriptions.NotSent, Status.E2_MessageStatus);
			AssertNull("CusEntryNumber is deleted", Factory.LoadTop1<CusEntryNumber>(CusEntryNumber.GetEntryNumberFilter(Consol)));

			Status.E2_MessageStatus = OutwardReportStatusList.Codes.Cleared;
			Factory.Save();
			Status.E2_MessageStatus = OutwardReportStatusList.Codes.AwaitingResponse;

			transactionParticipant.BeginTransactionWithManager().Dispose();
			transactionParticipant.OnAllTransactionsRolledBack();

			AssertEquals("Message status should be discarded", OutwardReportStatusList.Descriptions.Cleared, Status.E2_MessageStatus);
		}

		public void TestOutwardReportNumber()
		{
			CusEntryNumber entryNumber = Factory.New<CusEntryNumber>();
			entryNumber.CE_EntryType = CusEntryNumberTypeList.Codes.OutwardReportNumber;
			entryNumber.CE_EntryNum = "123456789";
			entryNumber.CE_ParentID = Consol.PK;

			AssertEquals("Outward Report Number", entryNumber.CE_EntryNum, Status.E2_CustomsEntryNumber);
		}

		public void TestE2_CustomsEntryNumberHumanReadableName()
		{
			AssertEquals("Label has changed from using ORN code to using Outward Report No from description", "Outward Report No.:", Status.E2_CustomsEntryNumberHumanReadableName);
		}

		public void TestNewCreateOrReplaceMessageBuilder()
		{
			Customs.Business.MessageBuilders.IManifestMessageBuilder builder2 = Status.NewCreateOrReplaceMessageBuilder();
			MessageBuilder builder = builder2 as MessageBuilder;
			AssertEquals("Original Message", "Original", builder.ManifestMessageTypeCode);

			CusEntryNumber entryNumber = Factory.New<CusEntryNumber>();
			entryNumber.CE_EntryType = CusEntryNumberTypeList.Codes.OutwardReportNumber;
			entryNumber.CE_EntryNum = ZString.Empty;
			entryNumber.CE_ParentID = Consol.PK;
			entryNumber.CE_EntryStatus = OutwardReportStatusList.Codes.NotSent;

			builder2 = Status.NewCreateOrReplaceMessageBuilder();
			builder = builder2 as MessageBuilder;
			AssertEquals("Original Message", "Original", builder.ManifestMessageTypeCode);

			entryNumber.CE_EntryNum = "12345678";
			builder2 = Status.NewCreateOrReplaceMessageBuilder();
			builder = builder2 as MessageBuilder;
			AssertEquals("Original Message", "Replacement", builder.ManifestMessageTypeCode);
		}

		public override void TestIsWaitingForResponse()
		{
			AddValidMessage(Consol);
			AssertEquals("Test manifest provider is waiting for response", OutwardReportStatusList.Descriptions.AwaitingResponse, Status.E2_MessageStatus);

			AddValidMessage(Consol);
			Assert("Test manifest provider is NOT waiting for response", OutwardReportStatusList.Codes.AwaitingResponse != Status.E2_MessageStatus);
		}

		public void TestCreateOrReplaceTransaction()
		{
			var entryNumber = Factory.New<CusEntryNumber>();
			entryNumber.CE_EntryType = CusEntryNumberTypeList.Codes.OutwardReportNumber;
			entryNumber.CE_EntryNum = ZString.Empty;
			entryNumber.CE_ParentID = Consol.PK;
			entryNumber.CE_EntryStatus = OutwardReportStatusList.Codes.NotSent;
			AssertEquals("CreateOrReplaceTransaction", TSWTransactionTypes.Original, Status.CreateOrReplaceTransaction);

			Status.E2_MessageStatus = OutwardReportStatusList.Codes.Rejected;
			entryNumber.CE_EntryNum = ZString.Empty;
			AssertEquals("CreateOrReplaceTransaction", TSWTransactionTypes.Original, Status.CreateOrReplaceTransaction);

			Status.E2_MessageStatus = OutwardReportStatusList.Codes.Cleared;
			entryNumber.CE_EntryNum = "684839002";
			AssertEquals("CreateOrReplaceTransaction", TSWTransactionTypes.Replace, Status.CreateOrReplaceTransaction);
		}

		public override void TestMessageStatus()
		{
			Assert("MessageStatus is not used in NZ", condition: true);
		}

		protected override bool SupportsGetMessageStatusHistory()
		{
			return false;
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			return new OutwardReportManifestStatus(consol);
		}

		ForwardingConsol fConsol;
		OutwardReportManifestStatus fStatus;

		ForwardingConsol Consol
		{
			get
			{
				if (fConsol == null)
				{
					fConsol = Factory.New<ForwardingConsol>();
				}
				return fConsol;
			}
		}

		OutwardReportManifestStatus Status
		{
			get
			{
				if (fStatus == null)
				{
					fStatus = new OutwardReportManifestStatus(Consol);
				}
				return fStatus;
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "12345");
		}

		protected override void TearDown()
		{
			base.TearDown();
			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "");
		}

		protected override CustomsManifestStatus NewCustomsManifestStatus(IManifestProvider manifestProvider)
		{
			return Status;
		}

		protected void SetUpValidAIRConsol()
		{
			CommonShipment shipment = Consol.Shipments.AddNew();
			shipment.JS_HouseBill = "Shipment1212";
			shipment.CustomsEntryNumberType = "ENT";
			shipment.CustomsEntryNumber = "EntryNumber";

			var shippingLine = Factory.LoadTop1<OrgHeader>(new ZQuery());
			Consol.SetDefaultShippingLineAddress(shippingLine);
			Consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			Consol.JK_RL_NKLoadPort = "NZAKL";
			Consol.JK_RL_NKDischargePort = "AUSYD";

			Transport transport = Consol.Transports[0];
			transport.JW_ETD = new ZDateTime(2005, 1, 1);
			transport.JW_VoyageFlight = "QF23";
			Factory.Save();
		}

		protected override EDIMessage AddValidMessage(IManifestProvider manifestProvider)
		{
			var outgoingMessage = Factory.New<OutwardReportMessage>();
			outgoingMessage.EM_LinkedObject = Consol;

			outgoingMessage.EM_ApplicationCode = EDIInterchange.ApplicationCodes.NewZealandCustoms;
			outgoingMessage.EM_ReceiveTransmit = "TRX";
			outgoingMessage.EM_MessageText = EDIMessage.MessageNumberPlaceHolder;
			EntryNum.CE_EntryStatus = OutwardReportStatusList.Codes.AwaitingResponse;
			return outgoingMessage;
		}

		protected override EDIMessage AddValidResponse(IManifestProvider manifestProvider)
		{
			var incomingMessage = Factory.New<OutwardReportMessage>();
			incomingMessage.EM_LinkedObject = Consol;
			incomingMessage.EM_ApplicationCode = EDIInterchange.ApplicationCodes.NewZealandCustoms;
			incomingMessage.EM_ReceiveTransmit = "RCV";
			incomingMessage.EM_SystemCreateTimeUtc = GetExpectedLastMessageDate(incomingMessage);

			EntryNum.CE_EntryStatus = OutwardReportStatusList.Codes.Cleared;
			return incomingMessage;
		}

		CusEntryNumber fEntryNum;
		protected CusEntryNumber EntryNum
		{
			get
			{
				if (fEntryNum == null)
				{
					fEntryNum = Factory.New<CusEntryNumber>();
					fEntryNum.CE_ParentID = Consol.PK;
					fEntryNum.CE_ParentTable = Consol.TableName;
					fEntryNum.CE_EntryNum = ExpectedCustomsEntryNumber;
					fEntryNum.CE_EntryType = CusEntryNumberTypeList.Codes.OutwardReportNumber;
				}
				return fEntryNum;
			}
		}

		protected override ZString ExpectedCustomsEntryNumber
		{
			get { return "1234"; }
		}

		protected override IManifestProvider NewManifestProvider()
		{
			return Consol;
		}

		#endregion
	}
}
