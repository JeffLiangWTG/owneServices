using System.Linq;
using Enterprise.Customs.Business.MessagingProcess;
using Enterprise.Customs.GUI.MessagingProcess;
using Enterprise.Customs.ZA.Business;
using Enterprise.Customs.ZA.Business.MessagingProcess;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ZA.GUI.MessagingProcess
{
	public sealed class ZACustomsMessagingGui : ICustomsMessagingGui, ISupportConfigureProcess, ISupportSendDialog
	{
		public static void SendToCustoms(JobDeclaration jobDeclaration, ZForm form)
		{
			var gui = new ZACustomsMessagingGui(jobDeclaration, form);

			_ = gui.SendMessages();
		}

		internal ZACustomsMessagingGui(JobDeclaration jobDeclaration, ZForm topLevelBusinessObjectForm)
		{
			CustomsMessagingSupporter = new CustomsMessagingSupporter(jobDeclaration, new ZACustomsMessagingProviderFactory());
			this.topLevelBusinessObjectForm = topLevelBusinessObjectForm;
		}
		internal readonly ICustomsMessagingSupporter CustomsMessagingSupporter;
		internal readonly ZForm topLevelBusinessObjectForm;

		ZACustomsMessagingProvider Provider => provider ??= CustomsMessagingSupporter.Provider as ZACustomsMessagingProvider;
		ZACustomsMessagingProvider provider;

		IDialog GetSendDialog()
		{
			IDialog sendDialog = null;

			if (CustomsMessagingSupporter.Messengers.Any())
			{
				sendDialog = new Dialog(Provider.DeclarationWrapper, typeof(MessageSendingForm));
			}

			return sendDialog;
		}

		internal void ConfigureProcess(ActionChain subChain)
		{
			subChain.FindAction(SendMessagesProcess.SendMessageActions.ShowSendDialog)
				.InsertActionAfter("ZAShowDefermentDialog", ShowDefermentDialog, ActionLink.Success);
		}

		ActionResult ShowDefermentDialog(ActionResult previousResult)
		{
			var result = SendMessagesGuiActionProvider.ShowOkCancelDialog(GetDeferredSubmissionDialog(previousResult), previousResult, Res.GetString("SendProcess|ZADeferredDialogCancellation", "User canceled, deferment aborted"));

			return result;
		}

		IDialog GetDeferredSubmissionDialog(ActionResult previousResult)
		{
			IDialog deferredDialog = null;

			if (previousResult.Success && previousResult.DataSource is JobDeclarationMessageSendingObjectParent && Provider.DeferrredIntegrator.ShouldShowDialog())
			{
				deferredDialog = new Dialog(Provider.DeferrredIntegrator.Submission, typeof(DeferredSubmissionForm));
			}

			return deferredDialog;
		}

		ICustomsMessagingSupporter ICustomsMessagingGui.MessagingSupporter => CustomsMessagingSupporter;

		ZForm ICustomsMessagingGui.TopLevelBusinessObjectForm => topLevelBusinessObjectForm;

		string ISupportSendDialog.SendDialogCancelledMessage => null;

		IDialog ISupportSendDialog.GetSendDialog(ActionResult previousResult) => GetSendDialog();

		void ISupportConfigureProcess.ConfigureProcess(ActionChain sendChain) => ConfigureProcess(sendChain);
	}
}
