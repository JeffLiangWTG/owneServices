using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.BatchProcessor;
using Enterprise.Customs.TR.Messaging;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business.Customs;
using AsycudaManifestHeader = Enterprise.Integration.Customs.ASYCUDA.TRManifest.IAsycudaManifestHeader;

namespace Enterprise.Customs.TR.Business.Testing
{
	public class CustomsServiceErrorMessageProcessorTest : TestCaseWithFactory
	{
		public void TestApplicationCode()
		{
			var processor = new CustomsServiceErrorMessageProcessor(new LoggingInformation());
			AssertEquals(EDIMessage.ApplicationCodes.TRCustoms, processor.ApplicationCode);
		}

		public void TestProcessMessage()
		{
			var group = Factory.New<GlbGroup>();
			group.GG_Code = "ULU";

			var staff = group.Staff.AddNew();
			staff.GS_Code = "TST";
			staff.GS_LoginName = "TST";
			staff.GS_FullName = "Test Staff";
			staff.GS_EmailAddress = "test@mail.com";
			var messageText = @"{""custom.ErrorType"":""ERR"",""custom.NotificationType"":""Failure"",""custom.ErrorDescription"":""Error Message\r\nContract: xt-contract:/Customs/TR/Contract TRE-TRD-TCD-TST\r\nReference Object: xt-httpclientaddress:{91cc0a89-2a03-46a2-81d5-b84cae232c0b}\r\nReference Object: xt-node:{fd428655-a4d6-48d0-8ad0-0fd3d61fccbb}\r\n\r\nError description: Message transmission to https://wstest.gtb.gov.tr:8443/EXT/Gumruk/EGE/Provider/ETicaretWS rejected by peer: result code 400 not accepted\r\n""}";
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_JobReference = "M00002345";

			var message = Factory.New<TRBaseMessage>();
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = TRMessageTypes.Codes.XER;
			message.EM_MessageText = messageText;
			message.EM_LinkedObject = header as BusinessObject;
			Factory.Save();

			using (TRCustomsDataRegistry.Instance.TRMANGroupNotification.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new ManifestGroupNotification("ENG", group.PK, false)))
			{
				var processor = new CustomsServiceErrorMessageProcessor(new LoggingInformation());
				processor.ProcessMessage(message);
			}

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == $"TR Customs Service Error Message Response (Failure) for {header.AMA_JobReference}");
			var bodyText = email.Body;

			var expectedMessageInterpretation = "Message for job M00002345 has been rejected. For details please follow the Link to the job<br /><br /><table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\"><tr><td>Error Type:</td><td>ERR</td></tr><tr><td>Notification Time:</td><td>&nbsp;</td></tr><tr><td>Notification Type:</td><td>Failure</td></tr><tr><td>Error Description:</td><td>Error Message<br>Contract: xt-contract:/Customs/TR/Contract TRE-TRD-TCD-TST<br>Reference Object: xt-httpclientaddress:{91cc0a89-2a03-46a2-81d5-b84cae232c0b}<br>Reference Object: xt-node:{fd428655-a4d6-48d0-8ad0-0fd3d61fccbb}<br><br>Error description: Message transmission to https://wstest.gtb.gov.tr:8443/EXT/Gumruk/EGE/Provider/ETicaretWS rejected by peer: result code 400 not accepted<br></td></tr></table>";

			CombineAssertions(() =>
			{
				AssertNotNull(email);

				AssertEquals("One recipient", 1, email.Recipients.Count);
				AssertEquals("Email Address", "test@mail.com", email.Recipients[0].Email);
				Assert("Contains Error Type Title", bodyText.Contains("Error Type"));
				Assert("Contains Error Description Title", bodyText.Contains("Error Description"));
				Assert("Contains Error Type Content", bodyText.Contains("ERR"));
				Assert("Contains Error Description Content", bodyText.Contains("Message transmission to https://wstest.gtb.gov.tr:8443/EXT/Gumruk/EGE/Provider/ETicaretWS rejected by peer: result code 400 not accepted"));
				AssertEquals("AMA_MessageStatus", TRMessageStatusCodeList.Codes.Error, header.AMA_MessageStatus);
				AssertEquals("expectedMessageInterpretation", expectedMessageInterpretation, message.EM_MessageInterpretation);
			});
		}
	}
}
