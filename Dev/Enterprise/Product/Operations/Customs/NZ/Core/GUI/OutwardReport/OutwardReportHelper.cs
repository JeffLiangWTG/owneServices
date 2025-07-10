using System.Globalization;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.GUI;
using Enterprise.Customs.NZ.Business.Declaration.OutwardReport;
using Enterprise.Customs.NZ.Business.TradeSingleWindow;
using Enterprise.Customs.NZ.GUI.TradeSingleWindow;
using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NZ.GUI
{
	public static class OutwardReportHelper
	{
		public static void TryToSendOCR(OutwardReportManifestStatus manifestStatus, SendOCR ocrSender, AdditionalMessageInformation additionalMessageInformation, TSWTransactionTypes type, bool canSendOCR, bool sendWithAttachments, bool verbose = false)
		{
			if (canSendOCR)
			{
				var notifier = new SendsMessagesToCustomsGUI();

				if (ocrSender.ErrorCount == 0)
				{
					if (!ocrSender.CheckWarningsBeforeGeneratingMessage() || notifier.ContinueWithSend(ocrSender.MessageWarnings))
					{
						var messageErrorsOnSendingObject = ocrSender.GetBOValidationMessageErrors();
						if (string.IsNullOrEmpty(messageErrorsOnSendingObject) || notifier.ContinueWithAction(messageErrorsOnSendingObject, "Continue to Send"))
						{
							var collectInfoAndSend = sendWithAttachments ? collectCommentsAndAttachments(type, additionalMessageInformation) : collectAdditionalMessageInformation(type, additionalMessageInformation, verbose);
							if (collectInfoAndSend)
							{
								if (ocrSender.SendMessage())
								{
									manifestStatus?.MessagesIncludingInterchangeRejections.Load();
									notifier.NotifyUserOfASuccessfulSend(string.Format(CultureInfo.CurrentCulture, "{0} message queued for sending.", type.ToString()));
								}
								else
								{
									notifier.NotifyUserOfAnInvalidOperation((Res.GetString("6D9CB6E6-F2C6-4CD4-86AE-52D21EABF1EA", "Unable to send OCR due to the following errors: \r\n\r\n{0}", ocrSender.Errors)));
								}
							}
						}
					}
				}
				else
				{
					notifier.NotifyUserOfAnInvalidOperation((Res.GetString("6D79D7A4-C6A5-4DA3-8BF9-77870C7CA2DC", "Unable to send {0} due to the following errors:", type.ToString())) + "\r\n\r\n" + ocrSender.Errors);
				}
			}
		}

		public static AdditionalMessageInformation GetAdditionalMessageInformation(BusinessObject hostEntity, TSWTransactionTypes type, bool sendWithAttachments)
		{
			var docManagerSuppoter = hostEntity as IDocManagerSupport;
			var eDocsForSelection = new IStorageDocsBaseCollection[] { docManagerSuppoter.DocManagerInfo.AllEDocs };
			var additionalMessageInformation = sendWithAttachments ? new AdditionalMessageInformation(null, eDocsForSelection, type, hostEntity.Factory, MessageTypeList.Codes.OCR) : new AdditionalMessageInformation(type, hostEntity.Factory);
			additionalMessageInformation.GatherPotentialSupportingDocuments();
			return additionalMessageInformation;
		}

		static bool collectAdditionalMessageInformation(TSWTransactionTypes type, AdditionalMessageInformation additionalMessageInformation, bool verbose)
		{
			bool result = false;

			TSWOriginalForm form = null;
			try
			{
				if (type == TSWTransactionTypes.Replace)
				{
					form = verbose ? new OCRReplaceWithCommentsForm(additionalMessageInformation) : new OCRReplaceForm(additionalMessageInformation);
				}
				else if (type == TSWTransactionTypes.Original)
				{
					if (verbose)
					{
						form = new OCROriginalForm(additionalMessageInformation);
					}
				}
				else if (type == TSWTransactionTypes.Cancel)
				{
					form = new OCRCancelForm(additionalMessageInformation);
				}

				result = form == null || ZFormModaliser.ShowDialogWithoutDispose(form) == DialogResult.OK;
			}
			finally
			{
				if (form != null)
				{
					form.Dispose();
				}
			}

			return result;
		}

		static bool collectCommentsAndAttachments(TSWTransactionTypes type, AdditionalMessageInformation additionalMessageInformation)
		{
			bool result = false;
			TSWSendFormWithAttachments sendingForm = type == TSWTransactionTypes.Replace ? new TSWReplaceForm(additionalMessageInformation) : new TSWSendFormWithAttachments(additionalMessageInformation);

			try
			{
				result = ZFormModaliser.ShowDialogWithoutDispose(sendingForm) == DialogResult.OK;
			}
			finally
			{
				if (sendingForm != null)
				{
					sendingForm.Dispose();
				}
			}

			return result;
		}
	}
}
