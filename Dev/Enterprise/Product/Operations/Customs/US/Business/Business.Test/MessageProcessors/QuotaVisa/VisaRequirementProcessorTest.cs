using System;
using System.Drawing;
using System.Linq;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Common;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Moq.Protected;

namespace Enterprise.Customs.US.Business.MessageProcessors.Testing
{
	sealed class VisaRequirementProcessorTest : ABIProcessorTest<VisaRequirementProcessor, APLA, APLB, APLY>
	{
		protected override void EndToEndCore()
		{
			var image1 = new Bitmap(1, 2);
			SystemDataRegistry.Instance.HtmlEmailBannerImage.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, image1);

			incomingMessage.EM_MessageText = "B018888XJ5UR                                               ~150000              U098191124           X20071014                                                  Y  8888XJ5UR00001";
			Factory.Save();

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			new USRIncomingMessageProcessor().ExecuteBatch();
			incomingMessage.Reload();
			AssertEquals(declaration, incomingMessage.EM_LinkedObject);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>((EmailDef emailToMatched) => emailToMatched.Subject == "Query for Visa Requirement"));
			AssertNotNull("An email with subject 'Query for Visa Requirement' should have been sent.", email);
			var banner = email.Attachments.Cast<AttachmentDef>().FirstOrDefault(x => x.DisplayName == "Banner.jpg");
			var image = new Bitmap(new System.IO.MemoryStream(banner.Data));
			AssertEquals(1, image.Width);
		}

		MQEDIMessage outgoingMessage;
		MQEDIMessage incomingMessage;
		JobDeclaration declaration;

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.New<JobDeclaration>();

			var mock = Factory.NewMoq<MQEDIMessage>();
			mock.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");

			outgoingMessage = mock.Object;
			outgoingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_MessageNum = "~150000";
			outgoingMessage.EM_MessageText = "B018888XJ5UI                                               ~150000              U198191124  6204624021ZAD                                                       Y  8888XJ5UI00001";
			declaration.Messages.Add(outgoingMessage);

			incomingMessage = Factory.New<MQEDIMessage>();
			incomingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_Status = "QUE";
			incomingMessage.EM_MessageType = ApplicationIdentifierCodeList.Codes.QueryQuotaResponse;
			incomingMessage.EM_MessageNum = "~150000";
		}
	}
}
