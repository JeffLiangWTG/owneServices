using System;
using Enterprise.Customs.Business.MessagingProcess;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI.MessagingProcess.Testing
{
	class CustomsMessagingGuiImplForTest : ICustomsMessagingGui
	{
		public CustomsMessagingGuiImplForTest(ICustomsMessagingSupporter messagingSupporter, ZForm mainForm)
		{
			this.messagingSupporter = messagingSupporter;
			this.mainForm = mainForm;
		}
		readonly ICustomsMessagingSupporter messagingSupporter;
		readonly ZForm mainForm;

		ICustomsMessagingSupporter ICustomsMessagingGui.MessagingSupporter => messagingSupporter;
		ZForm ICustomsMessagingGui.TopLevelBusinessObjectForm => mainForm;
	}

	class CustomsMessagingGuiWithConfigureProcessSupportImplForTest : CustomsMessagingGuiImplForTest, ISupportConfigureProcess
	{
		public CustomsMessagingGuiWithConfigureProcessSupportImplForTest(ICustomsMessagingSupporter messagingSupporter, ZForm mainForm) : base(messagingSupporter, mainForm)
		{
		}

		public Action<ActionChain> ConfigProcessForTesting { get; set; }
		void ISupportConfigureProcess.ConfigureProcess(ActionChain sendChain)
		{
			ConfigProcessForTesting?.Invoke(sendChain);
		}
	}

	class CustomsMessagingGuiWithPreviewDialogSupportImplForTest : CustomsMessagingGuiImplForTest, ISupportPreviewDialog
	{
		public CustomsMessagingGuiWithPreviewDialogSupportImplForTest(ICustomsMessagingSupporter messagingSupporter, ZForm mainForm) : base(messagingSupporter, mainForm)
		{
		}

		public string PreviewDialogCancelledMessage { get; set; } = "Preview cancelled";
		string ISupportPreviewDialog.PreviewDialogCancelledMessage => PreviewDialogCancelledMessage;

		public IDialog PreviewDialog { get; set; }
		IDialog ISupportPreviewDialog.GetPreviewDialog(ActionResult previousResult) => PreviewDialog;
	}

	class CustomsMessagingGuiWithSendDialogSupportImplForTest : CustomsMessagingGuiImplForTest, ISupportSendDialog
	{
		public CustomsMessagingGuiWithSendDialogSupportImplForTest(ICustomsMessagingSupporter messagingSupporter, ZForm mainForm) : base(messagingSupporter, mainForm)
		{
		}

		public string SendDialogCancelledMessage { get; set; } = "Send cancelled";
		string ISupportSendDialog.SendDialogCancelledMessage => SendDialogCancelledMessage;

		public IDialog SendDialog { get; set; }
		IDialog ISupportSendDialog.GetSendDialog(ActionResult previousResult) => SendDialog;
	}
}
