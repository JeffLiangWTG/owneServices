using System;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.NZ.Business.TradeSingleWindow.Testing
{
	using System.Threading;
	using CargoWise.Integration;
	using Enterprise.Customs.Common;
	using Enterprise.Customs.Common.NZ;
	using Enterprise.Customs.NZ.Registry;
	using NUnit.Framework;
	using CusEntryNumber = CusEntryNumber;

	[TestedType(typeof(ICRManifestStatus))]
	public class ICRManifestStatusTest : Customs.Business.Testing.CustomsManifestStatusTest
	{
		protected class TestManifestStatusClass : ICRManifestStatus
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

		public void TestEDITransmitDate()
		{
			ZDateTime first = Status.EDITransmitDate;
			Thread.Sleep(1000);
			ZDateTime second = Status.EDITransmitDate;
			AssertNotEquals("2 subsequernt pollings of EDITransmitDate should be different, using ZDateTime.Now", first, second);
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

		public void TestWithdrawManifest()
		{
			SendsMessagesToCustomsShutterUpperer sender = new SendsMessagesToCustomsShutterUpperer(false);
			AssertNull("PreCondition: No warning yet", sender.LastWarnings);

			CusEntryNumber entryNumber = Factory.New<CusEntryNumber>();
			entryNumber.CE_EntryType = CusEntryNumberTypeList.Codes.ICRNumber;
			entryNumber.CE_EntryNum = "123456";
			entryNumber.CE_ParentID = Consol.PK;
			entryNumber.CE_ParentTable = Consol.TableName;
			entryNumber.CE_EntryStatus = LowValueManifestStatusList.Codes.ManifestAccepted;

			AssertEquals("No message yet", 0, Consol.Messages.Count);
			sender.AnswerToContinueWithAction = true;
			SetUpValidAIRConsol();
			Status.WithdrawManifest(sender);
			AssertEquals("One message is generated", 1, Consol.Messages.Count);
		}

		public void TestSetCustomsEntryNumber()
		{
			CusEntryNumber entryNumber = Factory.New<CusEntryNumber>();
			entryNumber.CE_EntryType = CusEntryNumberTypeList.Codes.ICRNumber;
			entryNumber.CE_ParentID = Consol.PK;

			Status.SetCustomsEntryNumber("123456");
			AssertEquals("CustomsEntryNumber", "123456", entryNumber.CE_EntryNum);
		}

		public void TestUsersAreWarnedWhenTheyWithdraw()
		{
			SendsMessagesToCustomsShutterUpperer sender = new SendsMessagesToCustomsShutterUpperer(false);
			AssertNull("PreCondition: No warning yet", sender.LastWarnings);

			CusEntryNumber entryNumber = Factory.New<CusEntryNumber>();
			entryNumber.CE_EntryType = CusEntryNumberTypeList.Codes.ICRNumber;
			entryNumber.CE_EntryNum = "123456";
			entryNumber.CE_ParentID = Consol.PK;
			entryNumber.CE_EntryStatus = LowValueManifestStatusList.Codes.ManifestAccepted;

			Status.WithdrawManifest(sender);
			AssertEquals("Users are warned", 1, sender.PastMessages.Count);
		}

		public void TestMessagesIncludingInterchangeRejectionsReturnsNZCollection()
		{
			var collection = Status.MessagesIncludingInterchangeRejections;
			var message = collection.AddNew();
			AssertEquals("EDIMessage is typeof(NZCMessage)", typeof(ICRMessage), message.GetType());
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
			entryNumber.CE_EntryType = CusEntryNumberTypeList.Codes.ICRNumber;
			//EntryNumber.CE_EntryNum = ICRManifestStatus.InvalidReportNumber;
			entryNumber.CE_ParentID = Consol.PK;
			entryNumber.CE_EntryStatus = LowValueManifestStatusList.Codes.ManifestCancelled;

			SendsMessagesToCustomsShutterUpperer s = new SendsMessagesToCustomsShutterUpperer(false);
			Status.DeclareManifest(s);

			Assert("Cannot send a message as it is cancelled", s.InvalidOperationText == "This manifest has been withdrawn. You cannot send further messages on this consol.");
		}

		public void TestNewCreateOrReplaceMessageBuilderForInvalidReportNumber()
		{
			CusEntryNumber entryNumber = Factory.New<CusEntryNumber>();
			entryNumber.CE_EntryType = CusEntryNumberTypeList.Codes.ICRNumber;
			//EntryNumber.CE_EntryNum = ICRManifestStatus.InvalidReportNumber;
			entryNumber.CE_ParentID = Consol.PK;
			entryNumber.CE_EntryStatus = LowValueManifestStatusList.Codes.ManifestRejected;

			Customs.Business.MessageBuilders.IManifestMessageBuilder builder2 = Status.NewCreateOrReplaceMessageBuilder();
			MessageBuilder builder = builder2 as MessageBuilder;

			AssertEquals("Next message should be an original", "Original", builder.ManifestMessageTypeCode);
		}

		public void TestMessageStatusGetter()
		{
			AssertEquals("Status should be 'Not Sent'", LowValueManifestStatusList.Descriptions.NotSentToCustoms, Status.E2_MessageStatus);

			CusEntryNumber entryNumber = Factory.New<CusEntryNumber>();
			entryNumber.CE_EntryType = CusEntryNumberTypeList.Codes.ICRNumber;
			entryNumber.CE_EntryNum = "123456789";
			entryNumber.CE_ParentID = Consol.PK;
			entryNumber.CE_EntryStatus = LowValueManifestStatusList.Codes.ManifestAccepted;

			AssertEquals("Status should be 'Cleared'", LowValueManifestStatusList.Descriptions.ManifestAccepted, Status.E2_MessageStatus);
		}

		public void TestMessageStatusSetter()
		{
			Status.E2_MessageStatus = LowValueManifestStatusList.Codes.SentToCustoms;
			AssertEquals("Consol now has CusEntryNum attached", LowValueManifestStatusList.Descriptions.SentToCustoms, Status.E2_MessageStatus);
		}

		public void TestMessageStatusIsDiscardedIfTransactionFails()
		{
			Status.E2_MessageStatus = LowValueManifestStatusList.Codes.SentToCustoms;
			AssertNotNull("Pre-condition: CusEntryNumber is created", Factory.LoadTop1<CusEntryNumber>(CusEntryNumber.GetEntryNumberFilter(Consol, CusEntryNumberTypeList.Codes.ICRNumber)));

			var transactionParticipant = ((ITransactionParticipant)Factory);
			transactionParticipant.BeginTransactionWithManager().Dispose();
			transactionParticipant.OnAllTransactionsRolledBack();

			AssertEquals("Message status should be discarded", LowValueManifestStatusList.Descriptions.NotSentToCustoms, Status.E2_MessageStatus);
			AssertNull("CusEntryNumber is deleted", Factory.LoadTop1<CusEntryNumber>(CusEntryNumber.GetEntryNumberFilter(Consol, CusEntryNumberTypeList.Codes.ICRNumber)));

			Status.E2_MessageStatus = LowValueManifestStatusList.Codes.ManifestAccepted;
			Factory.Save();
			Status.E2_MessageStatus = LowValueManifestStatusList.Codes.SentToCustoms;

			transactionParticipant.BeginTransactionWithManager().Dispose();
			transactionParticipant.OnAllTransactionsRolledBack();

			AssertEquals("Message status should be discarded", LowValueManifestStatusList.Descriptions.ManifestAccepted, Status.E2_MessageStatus);
		}

		public void TestICRNumber()
		{
			CusEntryNumber entryNumber = Factory.New<CusEntryNumber>();
			entryNumber.CE_EntryType = CusEntryNumberTypeList.Codes.ICRNumber;
			entryNumber.CE_EntryNum = "123456789";
			entryNumber.CE_ParentID = Consol.PK;

			AssertEquals("ICR Number", entryNumber.CE_EntryNum, Status.E2_CustomsEntryNumber);
		}

		public void TestNewCreateOrReplaceMessageBuilder()
		{
			Customs.Business.MessageBuilders.IManifestMessageBuilder builder2 = Status.NewCreateOrReplaceMessageBuilder();
			MessageBuilder builder = builder2 as MessageBuilder;
			AssertEquals("Original Message", "Original", builder.ManifestMessageTypeCode);

			CusEntryNumber entryNumber = Factory.New<CusEntryNumber>();
			entryNumber.CE_EntryType = CusEntryNumberTypeList.Codes.ICRNumber;
			entryNumber.CE_EntryNum = ZString.Empty;
			entryNumber.CE_ParentID = Consol.PK;
			entryNumber.CE_EntryStatus = LowValueManifestStatusList.Codes.NotSentToCustoms;

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
			AssertEquals("Test manifest provider is waiting for response", LowValueManifestStatusList.Descriptions.SentToCustoms, Status.E2_MessageStatus);

			AddValidMessage(Consol);
			Assert("Test manifest provider is NOT waiting for response", LowValueManifestStatusList.Codes.SentToCustoms != Status.E2_MessageStatus);
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
			return new ICRManifestStatus(consol);
		}

		ForwardingConsol fConsol;
		ICRManifestStatus fStatus;

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

		ICRManifestStatus Status
		{
			get
			{
				if (fStatus == null)
				{
					fStatus = new ICRManifestStatus(Consol);
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
			Consol.JK_RL_NKLoadPort = "AUSYD";
			Consol.JK_RL_NKDischargePort = "NZAKL";

			Transport transport = Consol.Transports[0];
			transport.JW_ETD = new ZDateTime(2012, 12, 1);
			transport.JW_VoyageFlight = "QF23";
			Factory.Save();
		}

		protected override EDIMessage AddValidMessage(IManifestProvider manifestProvider)
		{
			var outgoingMessage = Factory.New<ICRMessage>();
			outgoingMessage.EM_LinkedObject = Consol;
			outgoingMessage.EM_ApplicationCode = EDIInterchange.ApplicationCodes.NewZealandCustoms;
			outgoingMessage.EM_ReceiveTransmit = "TRX";
			outgoingMessage.EM_MessageText = ICRMessage.MessageNumberPlaceHolder;
			EntryNum.CE_EntryStatus = LowValueManifestStatusList.Codes.SentToCustoms;
			EntryNum.CE_ParentTable = Consol.TableName;
			return outgoingMessage;
		}

		protected override EDIMessage AddValidResponse(IManifestProvider manifestProvider)
		{
			var incomingMessage = Factory.New<ICRMessage>();
			incomingMessage.EM_LinkedObject = Consol;
			incomingMessage.EM_ApplicationCode = EDIInterchange.ApplicationCodes.NewZealandCustoms;
			incomingMessage.EM_ReceiveTransmit = "RCV";
			incomingMessage.EM_SystemCreateTimeUtc = GetExpectedLastMessageDate(incomingMessage);

			EntryNum.CE_EntryStatus = LowValueManifestStatusList.Codes.ManifestAccepted;
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
					fEntryNum.CE_EntryNum = ExpectedCustomsEntryNumber;
					fEntryNum.CE_EntryType = CusEntryNumberTypeList.Codes.ICRNumber;
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
