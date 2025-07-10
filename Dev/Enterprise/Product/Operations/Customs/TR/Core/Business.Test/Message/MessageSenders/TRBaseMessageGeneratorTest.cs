using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.MessagingProcess.Testing;
using Enterprise.Customs.TR.Business.MessagingProcess;
using Enterprise.Customs.TR.Messaging;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Moq;

namespace Enterprise.Customs.TR.Business.Testing
{
	abstract class TRBaseMessageGeneratorAbstractTest<T> : TestCaseWithFactory
		where T : TRBaseMessage
	{
		public void TestGenerateMessage()
		{
			var sender = Sender;
			var message = (T)((ITRCustomsMessageGenerator)Generator).GenerateMessage();

			CombineAssertions(() =>
			{
				AssertEquals("Message Type", typeof(T), message.GetType());

				AssertEquals("EM_ReceiveTransmit", EDIMessage.Direction.Transmit, message.EM_ReceiveTransmit);
				AssertEquals("EM_Status", EDIMessage.Status.Queued, message.EM_Status);

				AssertEquals("EM_MessageType", ExpectedMessageType, message.EM_MessageType);
				AssertEquals("EM_MessageSubType", ExpectedMessageSubType, message.EM_MessageSubType);
				AssertEquals("EM_IsTestMessage", ExpectedIsTestMessage, message.EM_IsTestMessage);
				AssertEquals("EM_MessageOwner", ExpectedMessageOwner, message.EM_MessageOwner);
				AssertEquals("EM_ApplicationReference", ExpectedApplicationReference, message.EM_ApplicationReference);
				AssertEquals("EM_LinkTable", sender.Parent.TableName, message.EM_LinkTable);
				AssertEquals("EM_LinkUniqueID", sender.Parent.PK, message.EM_LinkUniqueID);
				AssertEquals("EM_GP", ExpectedEMGP, message.EM_GP);
				AssertNotNullOrEmpty("EM_MessageInterpretation", message.EM_MessageInterpretation);

				if (!message.NeedToSignMessage)
				{
					AssertNotNullOrEmpty("EM_MessageText", message.EM_MessageText);
				}
				else
				{
					AssertEquals("EM_MessageText should be empty", true, message.EM_MessageText.IsEmpty);
				}
			});
		}

		protected virtual string ExpectedMessageSubType => string.Empty;
		protected virtual ZBool ExpectedIsTestMessage => TRCustomsDataRegistry.Instance.IsTRTestingSystem;
		protected virtual string ExpectedMessageOwner => "TRM";
		protected virtual ZGuid ExpectedEMGP => ZGuid.Empty;

		protected abstract string ExpectedMessageType { get; }
		protected abstract string ExpectedApplicationReference { get; }
		protected abstract IMessageSender Sender { get; }
		protected abstract TRBaseMessageGenerator<T> Generator { get; }

		protected override void SetUp()
		{
			base.SetUp();

			SetupUser();
			SetupData();
		}

		protected virtual void SetupUser()
		{
			currentUser = TRGlbStaffWrapper.Get(GlbStaff.CurrentUser).TRBPassword;
			GlbStaff.CurrentUser.GS_Code = "TRM";
			currentUser.GP_UserID = "12345678901";
			currentUser.GP_PasswordType = PasswordTypesList.Codes.TRK;
			currentUser.GP_GC = Env.CurrentBranch.Company.PK;
			currentUser.GP_GS = currentUser.PK;
			currentUser.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
			currentUser.GP_StatusReason = "";
			currentUser.CurrentDecryptedPassword = "12345678";
			currentUser.GP_CertificateAuthority = "TÜBİTAK";
			currentUser.TR_Chipset = "EKART";
			currentUser.GP_CertificateSerialNumber = "02b9572b9cad7250a906b3";
			currentUser.PINCode = "080621";
		}
		protected GlbExternalPassword_TR currentUser;

		protected virtual void SetupData() { }
	}

	class TRBaseMessageGeneratorTest : TRBaseMessageGeneratorAbstractTest<TRBaseMessage>
	{
		protected override string ExpectedMessageType => "EMT";

		protected override string ExpectedApplicationReference => sender.Object.JobReference;

		protected override IMessageSender Sender => sender.Object;

		protected override TRBaseMessageGenerator<TRBaseMessage> Generator => generator;

		protected override void SetupData()
		{
			var dummyBiz = Factory.New<DummyBizObjWithMessages>();
			sender = new Mock<IMessageSender>();
			sender.Setup(x => x.Parent).Returns(dummyBiz);
			sender.Setup(x => x.Messages).Returns(dummyBiz.Messages);
			sender.Setup(x => x.JobReference).Returns("Job12345678");

			generator = new DummyGenerator<TRBaseMessage>(sender.Object, ExpectedMessageType, ExpectedApplicationReference);
		}

		Mock<IMessageSender> sender;
		DummyGenerator<TRBaseMessage> generator;

		class DummyGenerator<T> : TRBaseMessageGenerator<T>
		where T : TRBaseMessage
		{
			public DummyGenerator(IMessageSender sender, ZString messageType, ZString appReference) : base(sender)
			{
				MessageType = messageType;
				ApplicationReference = appReference;
			}

			public override ZString MessageType { get; }

			protected override ZString MessageText => "TR Base Message Generator";

			protected override ZString ApplicationReference { get; }
		}
	}
}
