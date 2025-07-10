using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business.MessagingProcess;
using Enterprise.Customs.GUI.MessagingProcess;
using Enterprise.Customs.TR.Business;
using Enterprise.Customs.TR.Business.MessagingProcess;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TR.GUI.MessagingProcess
{
	public sealed class TRCustomsMessagingGui : ICustomsMessagingGui, ISupportPreviewDialog
	{
		internal TRCustomsMessagingGui(BusinessObject topLevelBusinessObject, ICustomsMessagingProviderFactory customsMessagingProviderFactory, ZForm mainForm)
		{
			customsMessagingSupporter = new CustomsMessagingSupporter(topLevelBusinessObject, customsMessagingProviderFactory);
			this.mainForm = mainForm;
		}
		readonly ICustomsMessagingSupporter customsMessagingSupporter;
		readonly ZForm mainForm;

		ICustomsMessagingSupporter ICustomsMessagingGui.MessagingSupporter => customsMessagingSupporter;

		ZForm ICustomsMessagingGui.TopLevelBusinessObjectForm => mainForm;

		string ISupportPreviewDialog.PreviewDialogCancelledMessage => Res.GetString("TR|SendProcess|PreviewDialog", "Message signing canceled");

		IDialog ISupportPreviewDialog.GetPreviewDialog(ActionResult previousResult) => GetPreviewDialog(previousResult);
		IDialog GetPreviewDialog(ActionResult previousResult)
		{
			IDialog preview = null;
			var msg = previousResult.EDIMessages?.FirstOrDefault();
			var provider = customsMessagingSupporter.Provider;

			if (msg != null && provider is ITRCustomsMessagingProvider trProvider && trProvider.TRMessengers.Any(x => x.IsMessageSigningRequired))
			{
				var msgXml = msg.EM_FormattedMessageText;
				var userInfo = trProvider.TRBPassword;

				var ackAndSign = new MessageSendAcknowledgeAndSign(msgXml, userInfo, msg.EM_MessageType);
				return new Dialog(ackAndSign, typeof(ESignatureForm));
			}

			return preview;
		}

		public static ActionResult SendMessages(BusinessObject topLevelBusinessObject, ICustomsMessagingProviderFactory customsMessagingProviderFactor, ZForm mainForm)
		{
			var gui = new TRCustomsMessagingGui(topLevelBusinessObject, customsMessagingProviderFactor, mainForm);

			var result = gui.SendMessages();

			return result;
		}
	}
}
