using System;
using System.Windows.Forms;
using CargoWise.Common;
using Enterprise.Customs.GUI;
using Enterprise.Customs.NZ.Business.MAFeBACCa;
using Enterprise.Customs.NZ.Business.TradeSingleWindow;
using Enterprise.Customs.NZ.GUI.TradeSingleWindow;
using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NZ.GUI.MAFeBACCa
{
	public class MAFeBACCaMenu : MenuItem
	{
		public MAFeBACCaMenu(MAFMessagingBO mafMessaging)
			: base(MenuNames.MenuTitle)
		{
			Argument.NotNull(mafMessaging, nameof(mafMessaging));
			this.mafMessaging = mafMessaging;

			MenuItems.Add(new ZMenuItem(MenuNames.SendToMpi, SendMenuItem_Click));
			MenuItems.Add(new ZMenuItem(MenuNames.ResetToOriginal, ResetToOriginalMenuItem_Click));
		}

		public static class MenuNames
		{
			public static string MenuTitle => Res.GetString("Enterprise.Customs.NZ.GUI.MAFeBACCa.MAFeBACCaMenu|MenuTitle", "eBACCa/IPI");
			public static ResourceString SendToMpi => ResString.GetMultilingualString("Enterprise.Customs.NZ.GUI.MAFeBACCa.MAFeBACCaMenu|SendToMpi", "Send to MPI");
			public static ResourceString ResetToOriginal => ResString.GetMultilingualString("Enterprise.Customs.NZ.GUI.MAFeBACCa.MAFeBACCaMenu|ResetToOriginal", "Reset to Original");
		}

		#region Send to MPI

		void SendMenuItem_Click(object sender, EventArgs eventArgs)
		{
			if (!Env.Security.NZCustomsSendeBACCa.IsAllowed)
			{
				Env.Security.ShowError(Env.Security.NZCustomsSendeBACCa);
			}
			else
			{
				bool proceed = false;
				if (!mafMessaging.PlugInSupport.Master.HasChanges || Globals.Message.Show("There are changes on this form. Do you want to save changes first?", "Save", MessageBoxButtons.YesNo, DialogResult.Yes) == DialogResult.Yes)
				{
					proceed = true;

					var mainManu = GetMainMenu();
					var parentForm = (ZForm)(mainManu != null ? mainManu.GetForm() : null);

					if (parentForm != null)
					{
						proceed = parentForm.FireSaveButton() == CargoWise.EntityFramework.ContinueWithSave.Yes;
					}
				}

				if (proceed)
				{
					SendIPIMenuItem();
				}
			}
		}

		void SendIPIMenuItem()
		{
			var transactionType = mafMessaging.ZX_ConsignmentNumber.IsEmpty ? TSWTransactionTypes.Original : TSWTransactionTypes.Replace;
			var sender = new SendsMessagesToCustomsGUI();

			var alleDocs = mafMessaging.Consol?.DocManagerInfo.AllEDocs;
			var eDocsForSelection = alleDocs != null ? new[] { alleDocs } : Array.Empty<IStorageDocsBaseCollection>();

			var additionalMessageInformation = new AdditionalMessageInformation(null, eDocsForSelection, transactionType, mafMessaging.Factory, MessageTypeList.Codes.IPI);
			additionalMessageInformation.GatherPotentialSupportingDocuments();

			var ipiSender = new SendIPIFromConsol(mafMessaging, additionalMessageInformation, transactionType);
			if (ipiSender.ErrorCount == 0)
			{
				if (!ipiSender.CheckWarningsBeforeGeneratingMessage() || sender.ContinueWithSend(ipiSender.MessageWarnings))
				{
					if (transactionType == TSWTransactionTypes.Original)
					{
						using (var sendingForm = new TSWSendFormWithAttachments(additionalMessageInformation))
						{
							if (ZFormModaliser.ShowDialogWithoutDispose(sendingForm) == DialogResult.OK)
							{
								var messageErrorsOnConsol = ipiSender.GetBOValidationMessageErrors();
								if (string.IsNullOrEmpty(messageErrorsOnConsol) || sender.ContinueWithAction(messageErrorsOnConsol, "Continue to Send"))
								{
									var msgSentSuccesfully = ipiSender.SendMessage();
									if (msgSentSuccesfully)
									{
										sender.NotifyUserOfASuccessfulSend("IPI message queued for sending");
									}
								}
							}
						}
					}
					else
					{
						using (var tswForm = new TSWReplaceForm(additionalMessageInformation))
						{
							if (ZFormModaliser.ShowDialogWithoutDispose(tswForm) == DialogResult.OK)
							{
								var messageErrorsOnConsol = ipiSender.GetBOValidationMessageErrors();
								if (string.IsNullOrEmpty(messageErrorsOnConsol) || sender.ContinueWithAction(messageErrorsOnConsol, "Continue to Send"))
								{
									var msgSentSuccesfully = ipiSender.SendMessage();
									if (msgSentSuccesfully)
									{
										sender.NotifyUserOfASuccessfulSend("IPI replacement message queued for sending");
									}
								}
							}
						}
					}
				}
			}
			else
			{
				sender.NotifyUserOfAnInvalidOperation((Res.GetString("1BEF98D0-D031-4AF3-ADC7-AE80CC98B7E4", "Unable to send IPI due to the following errors:")) + "\r\n\r\n" + ipiSender.Errors);
			}
		}

		#endregion

		#region Reset To Original

		void ResetToOriginalMenuItem_Click(object sender, EventArgs eventArgs)
		{
			if (!Env.Security.CustomsResetToOriginal.IsAllowed)
			{
				Env.Security.ShowError(Env.Security.CustomsResetToOriginal);
			}
			else
			{
				if (Globals.Message.ShowConfirmation(MessageTextResetToOriginal, MessageCaptionResetToOriginal,
					Res.GetString("Enterprise.Customs.NZ.GUI.MAFeBACCa.MAFeBACCaMenu|ConfirmationPrompt", "If you are absolutely sure, please type: "),
					"Reset To Original",
					MessageBoxIcon.Exclamation) == DialogResult.OK)
				{
					mafMessaging.ResetToOriginal();
					Globals.Message.ShowInformation(MessageSuccessResetToOriginal, MessageCaptionResetToOriginal);
				}
			}
		}

		internal static string MessageCaptionResetToOriginal => Res.GetString("Enterprise.Customs.NZ.GUI.MAFeBACCa.MAFeBACCaMenu|MessageCaptionResetToOriginal", "Reset eBACCa to Original");
		public static string MessageTextResetToOriginal => Res.GetString("Enterprise.Customs.NZ.GUI.MAFeBACCa.MAFeBACCaMenu|MessageTextResetToOriginal", @"Are you sure you want to Reset the eBACCa to Original?
This action is intended as a 'last resort' that should only be used under instruction from WiseTech Global staff.
There is no way to undo this operation.");
		public static string MessageSuccessResetToOriginal => Res.GetString("Enterprise.Customs.NZ.GUI.MAFeBACCa.MAFeBACCaMenu|MessageSuccessResetToOriginal", "eBACCa has been reset to original.");

		#endregion

		readonly MAFMessagingBO mafMessaging;
	}
}
