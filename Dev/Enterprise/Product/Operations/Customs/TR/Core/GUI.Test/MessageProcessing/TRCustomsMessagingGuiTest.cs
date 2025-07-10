using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.MessagingProcess;
using Enterprise.Customs.Business.MessagingProcess.Testing;
using Enterprise.Customs.GUI.MessagingProcess;
using Enterprise.Customs.TR.Business;
using Enterprise.Customs.TR.Business.MessagingProcess;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.GUI;
using Moq;

namespace Enterprise.Customs.TR.GUI.MessagingProcess.Testing
{
	class TRCustomsMessagingGuiTest : TestCaseWithFactory
	{
		public void TestGetPreviewDialog()
		{
			var mockMsg = Factory.New<EDIMessage>();
			mockMsg.EM_MessageInterpretation = "Message preview";

			var msgFactory = CreateTRCustomsMessagingFactory(true);

			var gui = new TRCustomsMessagingGui(msgFactory.TopLevelBusinessObject, msgFactory, mainForm);
			var prevResult = new ActionResult(true) { EDIMessages = new List<EDIMessage> { mockMsg } };
			var dlg = (gui as ISupportPreviewDialog).GetPreviewDialog(prevResult);

			AssertNotNull("IDialog not null", dlg);
			AssertType<MessageSendAcknowledgeAndSign>("DataSource", dlg.DataSource);
			AssertEquals("ESignatureForm", typeof(ESignatureForm), dlg.TypeOfForm);
		}

		public void TestGetPreviewDialogNoMsgs()
		{
			var gui = new TRCustomsMessagingGui(trMessagingFactory.TopLevelBusinessObject, trMessagingFactory, mainForm);

			var prevResult = new ActionResult(true);

			var dlg = (gui as ISupportPreviewDialog).GetPreviewDialog(prevResult);

			AssertNull("IDialog null", dlg);
		}

		public void TestNew()
		{
			ICustomsMessagingGui gui = new TRCustomsMessagingGui(trMessagingFactory.TopLevelBusinessObject, trMessagingFactory, mainForm);

			AssertType<CustomsMessagingSupporter>("Supporter", gui.MessagingSupporter);
			AssertSame("Form", mainForm, gui.TopLevelBusinessObjectForm);
		}

		public void TestPreviewCancelledMessage()
		{
			var gui = new TRCustomsMessagingGui(trMessagingFactory.TopLevelBusinessObject, trMessagingFactory, mainForm) as ISupportPreviewDialog;
			AssertEquals("Message signing canceled", gui.PreviewDialogCancelledMessage);
		}

		public void TestMessageSigningRequiredTrue()
		{
			var msg = Factory.New<EDIMessage>();
			msg.EM_MessageInterpretation = "Message preview";

			var msgFactory = CreateTRCustomsMessagingFactory(true);

			var gui = new TRCustomsMessagingGui(msgFactory.TopLevelBusinessObject, msgFactory, mainForm);
			var prevResult = new ActionResult(true) { EDIMessages = new List<EDIMessage> { msg } };
			var dlg = (gui as ISupportPreviewDialog).GetPreviewDialog(prevResult);

			AssertNotNull("IDialog not null", dlg);
			AssertType<MessageSendAcknowledgeAndSign>("DataSource", dlg.DataSource);
			AssertEquals("ESignatureForm", typeof(ESignatureForm), dlg.TypeOfForm);
		}

		public void TestMessageSigningRequiredFalse()
		{
			var msg = Factory.New<EDIMessage>();
			msg.EM_MessageInterpretation = "Message preview";

			var msgFactory = CreateTRCustomsMessagingFactory(false);

			var gui = new TRCustomsMessagingGui(msgFactory.TopLevelBusinessObject, msgFactory, mainForm);
			var prevResult = new ActionResult(true) { EDIMessages = new List<EDIMessage> { msg } };
			var dlg = (gui as ISupportPreviewDialog).GetPreviewDialog(prevResult);

			AssertNull("IDialog null", dlg);
		}

		public void TestSendMessages()
		{
			using (var form = new ZForm())
			{
				var bizObj = Factory.New<DummyBizObjWithMessages>();

				var msg = Factory.New<TRManifestMessage>();
				msg.EM_MessageText = "Msg Created";

				var mockMessageGenerator = new Mock<ITRCustomsMessageGenerator>();
				mockMessageGenerator.Setup(m => m.IsMessageSigningRequired).Returns(false);
				mockMessageGenerator.Setup(m => m.GenerateMessage()).Returns(msg);

				var messenger = new TRCustomsMessenger(bizObj, mockMessageGenerator.Object);
				var provider = new TRCustomsMessagingProviderForTest(new[] { messenger });

				var msgFactory = new TRMessagingTestFactory(bizObj, provider);

				AssertEquals("Pre-req: No msgs", 0, bizObj.Messages.Count);

				TRCustomsMessagingGui.SendMessages(bizObj, msgFactory, form);

				AssertEquals("1 Message created", 1, bizObj.Messages.Count);
				AssertEquals("BizObj Saved", true, bizObj.IsInDatabase);
			}
		}

		public void TestICustomsMessagingGui()
		{
			var gui = new TRCustomsMessagingGui(trMessagingFactory.TopLevelBusinessObject, trMessagingFactory, mainForm) as ICustomsMessagingGui;

			CombineAssertions(() =>
			{
				AssertNotNull("Supporter", gui.MessagingSupporter);
				AssertSame("Form", mainForm, gui.TopLevelBusinessObjectForm);
			});
		}

		TRMessagingTestFactory CreateTRCustomsMessagingFactory(bool requireSigning)
		{
			var dummy = Factory.New<DummyBusinessObject>();
			var child = Factory.New<DummyBizObjWithMessages>();
			var msg = Factory.New<EDIMessage>();
			msg.EM_MessageInterpretation = "Message preview";
			child.Messages.Add(msg);

			var mockMessageGenerator = new Mock<ITRCustomsMessageGenerator>();
			mockMessageGenerator.Setup(m => m.IsMessageSigningRequired).Returns(requireSigning);
			var messenger = new TRCustomsMessenger(child, mockMessageGenerator.Object);
			var provider = new TRCustomsMessagingProviderForTest(new[] { messenger });

			return new TRMessagingTestFactory(dummy, provider);
		}

		class TRMessagingTestFactory : ICustomsMessagingProviderFactory
		{
			public TRMessagingTestFactory(BusinessObject topLevelBusinessObject, ICustomsMessagingProvider provider)
			{
				TopLevelBusinessObject = topLevelBusinessObject;
				this.provider = provider;
			}
			readonly ICustomsMessagingProvider provider;
			public BusinessObject TopLevelBusinessObject { get; set; }

			public ICustomsMessagingProvider CreateProvider(BusinessObject businessObject) => provider;
		}

		class TRCustomsMessagingProviderForTest : ITRCustomsMessagingProvider, ICustomsMessagingProvider
		{
			public TRCustomsMessagingProviderForTest(IReadOnlyCollection<ITRCustomsMessenger> testMessengers)
			{
				commonProvider = new TRCustomsMessagingCommonProvider(testMessengers, null);
			}
			readonly TRCustomsMessagingCommonProvider commonProvider;

			IReadOnlyCollection<ITRCustomsMessenger> ITRCustomsMessagingProvider.TRMessengers => commonProvider.TRMessengers;
			GlbExternalPassword_TR ITRCustomsMessagingProvider.TRBPassword => commonProvider.TRBPassword;

			bool ICustomsMessagingProvider.IsInTestMode => commonProvider.IsInTestMode;
			bool ICustomsMessagingProvider.EnableTestModeValidation => commonProvider.EnableTestModeValidation;
			IReadOnlyCollection<ICustomsMessenger> ICustomsMessagingProvider.GetMessengers() => commonProvider.GetMessengers();
		}

		protected override void SetUp()
		{
			base.SetUp();

			var dummy = Factory.New<DummyBusinessObject>();
			var provider = new TRCustomsMessagingProviderForTest(Array.Empty<TRCustomsMessenger>());
			trMessagingFactory = new TRMessagingTestFactory(dummy, provider);
			mainForm = new ZForm(dummy);
		}

		protected override void TearDown()
		{
			base.FinalTearDown();
			mainForm?.Dispose();
		}

		TRMessagingTestFactory trMessagingFactory;
		ZForm mainForm;
	}
}
