using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Cryptoki.Signing.API;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.MessagingProcess;
using Enterprise.Customs.Common;
using Enterprise.Customs.TR.Business.Testing;
using Enterprise.Customs.TR.Messaging;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Moq;

namespace Enterprise.Customs.TR.Business.MessagingProcess.Testing
{
	class TRCustomsMessengerTest : TestCaseWithFactory
	{
		public void TestIsMessageSigningRequired()
		{
			mockGenerator.SetupSequence(m => m.IsMessageSigningRequired).Returns(true).Returns(false);
			AssertEquals("Signing required", true, ((ITRCustomsMessenger)customsMessenger).IsMessageSigningRequired);
			AssertEquals("Signing not required", false, ((ITRCustomsMessenger)customsMessenger).IsMessageSigningRequired);
			mockGenerator.VerifyAll();
		}

		public void TestSignMessages()
		{
			mockGenerator.Setup(m => m.IsMessageSigningRequired).Returns(true);

			using (Env.SetTemporaryUserContext(new UserContext(staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				var ackAndSign = new MessageSendAcknowledgeAndSign("<xmlMessage>", extPassword);
				ackAndSign.PINCode = "123456";

				var actionResult = new ActionResult(true) { DataSource = ackAndSign };

				var msgs = GetMessages().ToArray();
				var result = customsMessenger.SignMessages(msgs, actionResult);

				CombineAssertions(() =>
				{
					AssertEquals("Result Empty", string.Empty, result);
					AssertContains("Msg1", "Signed", msgs[0].EM_MessageData.ToUTF8());
					AssertContains("Msg2", "Signed", msgs[1].EM_MessageData.ToUTF8());
				});
			}

			mockGenerator.Verify(m => m.IsMessageSigningRequired);
		}

		public void TestSignMessages_NoNeedToSign()
		{
			mockGenerator.Setup(m => m.IsMessageSigningRequired).Returns(false);
			using (Env.SetTemporaryUserContext(new UserContext(staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				var actionResult = new ActionResult(true);

				var msgs = GetMessages().ToArray();
				var result = customsMessenger.SignMessages(msgs, actionResult);

				CombineAssertions(() =>
				{
					AssertEquals("Result Empty", string.Empty, result);
					AssertNotContains("Msg1", "Signed", msgs[0].EM_MessageData.ToUTF8());
					AssertNotContains("Msg2", "Signed", msgs[1].EM_MessageData.ToUTF8());
				});
			}

			mockGenerator.Verify(m => m.IsMessageSigningRequired);
		}

		public void TestSignMessages_MissingDataSource()
		{
			mockGenerator.Setup(m => m.IsMessageSigningRequired).Returns(true);

			using (Env.SetTemporaryUserContext(new UserContext(staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				var actionResult = new ActionResult(true);

				var msgs = GetMessages().ToArray();
				var result = customsMessenger.SignMessages(msgs, actionResult);

				CombineAssertions(() =>
				{
					AssertEquals("PIN info required", "PIN information is missing from Preview Result", result);
					AssertNotContains("Msg1", "Signed", msgs[0].EM_MessageData.ToUTF8());
					AssertNotContains("Msg2", "Signed", msgs[1].EM_MessageData.ToUTF8());
				});
			}

			mockGenerator.Verify(m => m.IsMessageSigningRequired);
		}

		public void TestSignMessages_SigningError()
		{
			var crashSign = new Mock<ISignatureBuilder>();
			crashSign
				.Setup(m => m.Sign(It.IsAny<byte[]>(), It.IsAny<ISignatureAlgorithm>()))
				.Throws(new Exception("Signing is not available at present"));

			var crashsigner = TRMessageSignerForTest.New(crashSign.Object);
			var crashmessenger = new TRCustomsMessenger(mockOwner.Object, mockGenerator.Object, crashsigner);

			mockGenerator.Setup(m => m.IsMessageSigningRequired).Returns(true);

			using (Env.SetTemporaryUserContext(new UserContext(staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				var ackAndSign = new MessageSendAcknowledgeAndSign("<xmlMessage>", extPassword);
				ackAndSign.PINCode = "123456";

				var actionResult = new ActionResult(true) { DataSource = ackAndSign };

				var msgs = GetMessages().ToArray();
				var result = crashmessenger.SignMessages(msgs, actionResult);

				CombineAssertions(() =>
				{
					AssertEquals("Result Empty", "The following error was encountered while signing the message: Signing is not available at present", result);
					AssertNotContains("Msg1", "Signed", msgs[0].EM_MessageData.ToUTF8());
					AssertNotContains("Msg2", "Signed", msgs[1].EM_MessageData.ToUTF8());
				});
			}
			crashSign.VerifyAll();
			mockGenerator.Verify(m => m.IsMessageSigningRequired);
		}

		public void TestUpdateStatus()
		{
			var header = Factory.New<OwnerAndAttacheeForTest>();
			var attachee = (IMessageAttachee)header;
			attachee.MessageStatus = ZString.Empty;

			var messenger = new TRCustomsMessenger(header, mockGenerator.Object, TRMessageSignerForTest.New());

			var result = (messenger as ICustomsMessenger).ProcessUpdates(new ActionResult(true));
			AssertEquals("Success", true, result);
			AssertEquals("Status", TRMessageStatusCodeList.Codes.Awaiting, attachee.MessageStatus);

			attachee.MessageStatus = ZString.Empty;
			result = (messenger as ICustomsMessenger).ProcessUpdates(new ActionResult(false));
			AssertEquals("Success", true, result);
			AssertEquals("Status", ZString.Empty, attachee.MessageStatus);

			var header2 = Factory.New<Customs.Business.MessagingProcess.Testing.DummyBizObjWithMessages>();
			messenger = new TRCustomsMessenger(header2, mockGenerator.Object, TRMessageSignerForTest.New());
			result = (messenger as ICustomsMessenger).ProcessUpdates(new ActionResult(true));
			AssertEquals("Success", true, result);
		}

		public void TestShouldCreateMessage()
		{
			var header = Factory.New<OwnerAndAttacheeForTest>();
			var attachee = (IMessageAttachee)header;
			attachee.MessageStatus = ZString.Empty;

			var messenger = new TRCustomsMessenger(header, mockGenerator.Object, TRMessageSignerForTest.New()) as ICustomsMessenger;

			AssertEquals("Always create", true, messenger.ShouldCreateMessage(new ActionResult()));
		}

		public void TestOwner()
		{
			var header = Factory.New<OwnerAndAttacheeForTest>();
			var attachee = (IMessageAttachee)header;
			attachee.MessageStatus = ZString.Empty;

			var messenger = new TRCustomsMessenger(header, mockGenerator.Object, TRMessageSignerForTest.New()) as ICustomsMessenger;
			AssertSame(header, messenger.Owner);
		}

		public void TestGenerator()
		{
			var header = Factory.New<OwnerAndAttacheeForTest>();
			var attachee = (IMessageAttachee)header;
			attachee.MessageStatus = ZString.Empty;

			var messenger = new TRCustomsMessenger(header, mockGenerator.Object, TRMessageSignerForTest.New()) as ICustomsMessenger;

			AssertSame(mockGenerator.Object, messenger.MessageGenerator);
		}

		IReadOnlyCollection<EDIMessage> GetMessages()
		{
			var mockMessage1 = Factory.NewMoq<EDIMessage>();
			mockMessage1.Object.EM_MessageInterpretation = "Hello World";
			var mockMessage2 = Factory.NewMoq<EDIMessage>();
			mockMessage2.Object.EM_MessageInterpretation = "Goodbye Universe";

			return new[] { mockMessage1.Object, mockMessage2.Object };
		}

		protected override void SetUp()
		{
			base.SetUp();

			staff = Factory.New<GlbStaff>();
			staff.GS_Code = "TSN";
			staff.GS_FullName = "TR Testing User";
			extPassword = TRGlbStaffWrapper.Get(staff).TRBPassword;
			extPassword.GP_UserID = "1234";
			extPassword.CurrentDecryptedPassword = "xxx";
			extPassword.GP_CertificateAuthority = "TÜBİTAK";
			extPassword.TR_Chipset = "WINDOWS";
			extPassword.GP_CertificateSerialNumber = "02b9572b9cad7250a906b3";

			Factory.Save();

			mockOwner = new Mock<IEDIMessageCollectionOwner>();
			mockGenerator = new Mock<ITRCustomsMessageGenerator>();
			customsMessenger = new TRCustomsMessenger(mockOwner.Object, mockGenerator.Object, TRMessageSignerForTest.New());
		}

		GlbStaff staff;
		GlbExternalPassword_TR extPassword;
		Mock<IEDIMessageCollectionOwner> mockOwner;
		Mock<ITRCustomsMessageGenerator> mockGenerator;
		TRCustomsMessenger customsMessenger;
	}

	class OwnerAndAttacheeForTest : Customs.Business.MessagingProcess.Testing.DummyBizObjWithMessages, IMessageAttachee
	{
		public OwnerAndAttacheeForTest(CargoWise.EntityFramework.BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public ZString MessageStatus { get; set; }

		public ZString JobReference => throw new NotImplementedException();

		public ZGuid GlobalBranchPK => throw new NotImplementedException();

		public ZString CustomsStatus { get; set; }

		CargoWise.EntityFramework.IBusinessObjectCollection IMessageAttachee.Messages => Messages;
	}
}
