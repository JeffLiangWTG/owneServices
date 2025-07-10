using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.BatchProcessor;
using Enterprise.Customs.NZ.Business;
using Enterprise.Customs.NZ.Business.MessageProcessors;
using Enterprise.Customs.NZ.Registry;
using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.NZ.Manifest.Business.Testing
{
	sealed class WriteOffResponseAsycudaTest : TestCaseWithFactory
	{
		public void TestICRWriteOffMessageResponse()
		{
			using (NZCustomsDataRegistry.Instance.NZBrokerageID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "00009917B"))
			{
				var bill1 = header.Bills.AddNew();
				var bill2 = header.Bills.AddNew();
				SetupICRWriteOffResponse(bill1, bill2);
				var processor = new MessageProcessorFactory(new LoggingInformation());
				processor.ProcessMessage(outboundMessage);
				AssertEquals(EDIMessage.Status.Received, outboundMessage.EM_Status);
				AssertEquals(LowValueManifestStatusList.Codes.Acknowledgement, header.AMA_MessageStatus);
				AssertEquals(1, header.Messages.Count);
				AssertEquals("", header.RegistrationNumber);
				var email = Env.OutgoingMailManager.EmailsCreated.Find(emailToMatch => emailToMatch.Subject == "[ICR/CRE Rejected] Response for Manifest: MAN0000026");
				AssertNotNull("Should have found the email generated from processing this Depot Response message", email);
				AssertEquals("JohnTester@TestingCompany.com", email.Recipients[0].Email);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ManifestType = "ICR";
			header.AMA_JobReference = "MAN0000026";
			var unsolicitedDOGroup = Factory.New<GlbGroup>();
			unsolicitedDOGroup.GG_Code = "USR";
			unsolicitedDOGroup.GG_Desc = "Unsolicited Delivery Order Responses";
			var staff = unsolicitedDOGroup.Staff.AddNew();
			staff.GS_Code = "TST";
			staff.GS_FullName = "John Tester";
			staff.GS_LoginName = "JT";
			staff.GS_EmailAddress = "JohnTester@TestingCompany.com";
			outboundMessage = Factory.New<TSWMessage>();
			outboundMessage.EM_ApplicationCode = EDIInterchange.ApplicationCodes.NewZealandCustoms;
			outboundMessage.EM_MessageType = MessageTypeList.Codes.ICR;
			outboundMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outboundMessage.EM_ApplicationReference = "MAN0000026";
			outboundMessage.EM_LinkedObject = header;
			outboundMessage.EM_SystemCreateUser = "TST";
			outboundMessage.EM_MessageText = new EmbeddedResourceRetriever().GetString("Enterprise.Customs.NZ.Manifest.Business.Testing.Messaging.TestFiles.ICRAsycudaWriterOffResponse.txt");
		}

		AsycudaManifestHeader header;
		TSWMessage outboundMessage;

		void SetupICRWriteOffResponse(AsycudaBill bill1, AsycudaBill bill2)
		{
			bill1.ABL_BillNumber = "1";
			bill1.ABL_MessageStatus = LowValueConsignmentStatusList.Codes.SentToCustoms;
			bill2.ABL_BillNumber = "2";
			bill2.ABL_MessageStatus = LowValueConsignmentStatusList.Codes.SentToCustoms;
			Factory.Save();
		}
	}
}
