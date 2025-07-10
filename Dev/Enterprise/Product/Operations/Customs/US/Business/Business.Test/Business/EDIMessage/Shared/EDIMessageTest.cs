using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(EDIMessage))]
	class EDIMessageTest : EnterpriseBusinessObjectTestCase
	{
		public void TestResetToQueue()
		{
			var message = Factory.New<AESTIREDIMessage>();
			message.EM_ReceiveTransmit = EDIMessage.Status.Received;
			message.ResetToQueuedStatus();
			Factory.Save();
			var messageLoaded = new BusinessObjectFactory().Load<EDIMessage>(message.PK);
			AssertEquals(typeof(AESTIREDIMessage), messageLoaded.GetType());
		}

		public void TestTypeDecider()
		{
			AssertEquals(typeof(EDIMessageTypeDecider), EDIMessage.TypeDecider.GetType());
		}

		public void TestMessageTypeDescription()
		{
			DeclarationTestHelper.SetupForSendMessage();
			message.EM_MessageText = CustomsResponseMsg;
			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.EntrySummaryAdd;
			AssertEquals("Add Entry Summary", message.EM_MessageSubTypeDescription);
			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			message.Reload();
			Enterprise.Messaging.Business.EDIInterchange interchange = Factory.New<Enterprise.Messaging.Business.EDIInterchange>();
			interchange.EI_From = "Customs";
			message.EM_EI = interchange.PK;
			AssertEquals("FAL", message.EM_Status);
			AssertEquals("RCV", message.EM_ReceiveTransmit);
			AssertEquals("CUSTOMS", message.EM_User.ToUpper());
			AssertEquals("Entry Summary Response", message.MessageTypeDescription);
		}

		public void TestEM_MessageSubTypeDescription()
		{
			message.EM_MessageSubType = "XXX";
			AssertEquals("Unknown Message Sub Type", message.EM_MessageSubTypeDescription);
		}

		public void TestResetStatusToQueued()
		{
			message.EM_LinkUniqueID = Guid.NewGuid();
			message.EM_LinkTable = "BLAH";
			message.EM_MessageType = "BAR";
			message.EM_MessageSubType = "ETC";
			message.EM_Status = "FOO";
			message.ResetToQueuedStatus();
			AssertEquals("EM_LinkUniqueID should be empty", ZGuid.Empty, message.EM_LinkUniqueID);
			AssertEquals("EM_LinkTable should be empty", "", message.EM_LinkTable);
			AssertEquals("EM_MessageType should not be empty", "BAR", message.EM_MessageType);
			AssertEquals("EM_MessageSubType should not be empty", "ETC", message.EM_MessageSubType);
			AssertEquals("EM_Status should be Queued", EDIMessage.Status.Queued, message.EM_Status);
		}

		protected override BusinessObject GetNewBusinessObject() => Factory.New<EDIMessage>();

		JobDeclaration dec;
		CusEntryHeader entryHeader;
		MQEDIMessage message;
		protected override void SetUp()
		{
			base.SetUp();
			dec = Factory.New<JobDeclaration>();
			entryHeader = dec.CustomsEntryHeaders.AddNew();
			entryHeader.CH_Status = ImportMessageStatusList.Codes.AwaitingEntrySummaryOriginal;
			message = Factory.New<MQEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = ApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
			message.EM_MessageNum = "~15000";
			Factory.Save();
		}

		const string CustomsResponseMsg = "B018888XJ5EI                                               6824                 "
			+ "10A888891-01319900091-013199000                 8         XJ5 7000238701891  MI "
			+ "20     CAECILIA SCHULTE    112704042008B00150464            *****042008J797     "
			+ "22            843024321   SIN0021198              00000010CT         APLUHDMU   "
			+ "30                                  0               1                   APLU    "
			+ "40001SG00000100000000007532                    000000000155976                  "
			+ "50 39169050000000058000000000700000KG                               SG032308N   "
			+ "51                                                                              "
			+ "60                                        AUABCEXP6390ALE                       "
			+ "62391690500050100001250                                                         "
			+ "62          49900002100                                                         "
			+ "895010000000125049900000002500                                                  "
			+ "9000000058000           0                       0000000375000000010000          "
			+ "Y  8888XJ5EI00012000000058000";
	}
}
