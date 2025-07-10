using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.Business.JobDeclarationExtensions;
using Enterprise.Customs.Business.WarehouseExtensions;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Environment;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using MessageSendingNotificationCollection = Enterprise.Customs.Business.MessageSendingNotificationCollection;
using MessageSendingValidation = Enterprise.Customs.Business.MessageSendingValidation;
using WarehouseTransactionStatusList = Enterprise.Customs.Business.WarehouseTransactionStatusList;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.GUI
{
	public partial class EDIMenu : Customs.GUI.EDIMenu
	{
		public EDIMenu()
		{
			this.Popup += Menu_Popup;
		}
		public new JobDeclaration Declaration
		{
			get { return (JobDeclaration)base.Declaration; }
			set { base.Declaration = value; }
		}

		internal static class Constants
		{
			public const string SendOriginalMessages = "Send Original Messages";
			public const string SendAmendmentMessages = "Send Replacement / Update Messages";
			public const string SendWithdrawalMessages = "Send Deletion Messages";

			public const string SendEntrySummaryMessages = "Send Entry Summary";
			public const string SendCargoReleaseMessages = "Send Cargo Release";

			public const string CopyPreviousFDALine = "Copy Previous FDA Line";
			public const string SendFDACorrectionMessages = "Send FDA Correction (CP)";
			public const string SendStandalonePriorNotice = "Send Standalone Prior Notice";

			public const string CopyPreviousPGALine = "Copy Previous PGA Line";

			public const string RequestToExtendTIB = "Send TIB Extension Request";
			public const string RequestToClosureTIB = "Send TIB Closure Request";

			public const string QueryADDCVD = "Query ADD/CVD cases";
			public const string QueryADDCVDByTariff = "Query ADD/CVD for tariffs used";
			public const string QueryImporterBond = "Query Importer Bond";
			public const string SendQuotaQuery = "Query Quota for tariffs used";
			public const string SendVisaQuery = "Query Visa for tariffs used";
			public const string CargoManifestStatusQuery = "Query Cargo/Manifest/Entry Status";

			public const string SendBillOfLadingUpdate = "Send Bill Of Lading Update";
			public const string SendConsigneeNameAddressAddMessages = "Send Consignee Name/Address Add";
			public const string SendEntryDateUpdate = "Send Entry Date Update";
			public const string SendEntrySummaryQuery = "Query Entry Summary";
			public const string StatementDeleteAddMessage = "Send Statement Delete/Add Message";

			public const string SendTariffUpdateQuery = "Quota Tariff Update for tariffs used";
			public const string RequestTariffUpdate = "Request Tariff Update for tariffs used";
			public const string RefreshTariffDetails = "Refresh Tariff Details";
			public const string RefreshNotificationDispositionActions = "Refresh Notification Disposition Actions";

			public const string GeneratingMessagesStatusMessage = "Generating Messages...";
			public const string SavingMessagesStatusMessage = "Saving Messages...";
			public const string ShowDiscardedAndInactiveMessages = "Show Inactive Messages";

			public const string GenerateAIILines = "Generate AII Lines";
			public const string RefreshExRates = "Refresh Ex-Rates";

			public const string SendCensusWarningOverrideMessage = "Send Census Warning Override";
			public const string SendCensusWarningQueryMessage = "Query Census Warning";

			public const string ResetToOriginal = "Reset To Original";

			public const string SendPGACorrection = "Send PGA Correction";

			public const int GeneratingMessagesStatusMessageProgress = 60;
			public const int SavingMessagesStatusMessageProgress = 80;

			public const string ResetDutyCalculationDate = "Reset Duty Calculation Date";

			//BIRD
			public const string ExportDeclarationTo7501 = "Export Declaration to 7501";
			public const string ExportDeclarationTo3461 = "Export Declaration to 3461";
			public const string ExportQueryBIRD = "Export Entry Summary Query Request";
			public const string ExportLiquidationBIRD = "Export Liquidation";
			public const string ExportCargoReleaseProcessingResultBIRD = "Export Cargo Release Processing Result";
			public const string ExportEntrySummaryQueryResponse = "Export Entry Summary Query Response";
			public const string ExportToEN = "Export Acknowledgement of 7501/3461 Receipt";
			public const string ExportToDT = "Export Significant Dates";

			//FTZ
			public const string SendFTZOriginalMessages = "Send Original Message";
			public const string SendFTZAmendmentMessages = "Send Replacement / Amendment Message";
			public const string SendFTZWithdrawalMessages = "Send Deletion Message";
			public const string SendArrival = "Send Arrival Message";
			public const string SendConcurrence = "Send Concurrence Message";
			public const string SendPostAdmissionCorrection = "Send Post Admission Correction Message";
			public const string SendPTTMessages = "Send Permit To Transfer Message";
			public const string CancelPTTMessage = "Cancel Permit To Transfer Message";
			public const string SendPTTArrival = "Send Permit To Transfer Arrival";
			public const string SendPTTUnArrival = "Send Permit To Transfer Un-Arrival";
			public const string SendDeliveryOfGoods = "Send Delivery Of Goods Message";
			public const string FTZCargoManifestStatusQuery = "Query Cargo Manifest Status";

			//Drawback
			public const string DrawbackBulkImportEntryLines = "Import Entry Lines (Bulk)";

			public const string ImportBulkDeclarations = "Import Declarations (Bulk)";
		}

		protected override string TermName => Declaration.TermNameForBondedWarehouse;

		void Menu_Popup(object sender, EventArgs e)
		{
			if (Declaration != null && Declaration.TermNameForBondedWarehouse != "Inventory Management")
			{
				var termNameForBondedWarehouse = Declaration.TermNameForBondedWarehouse == "Inventory Management" ? "Inventory" : Declaration.TermNameForBondedWarehouse;
				bondedWarehouseMenuItem.Text = termNameForBondedWarehouse;
				updateBondedWarehouseMenuItem.Text = "Update " + termNameForBondedWarehouse;
				cancelUpdateBondedWarehouseInwardMenuItem.Text = "Cancel " + termNameForBondedWarehouse;
				synchronizeWithBondedWarehouseMenuItem.Text = "&Synchronize with " + termNameForBondedWarehouse;
				cancelBondedWarehouseMenuItem.Text = "&Cancel " + termNameForBondedWarehouse + " Stock Release";
				finaliseStockWithBondedWarehouseMenuItem.Text = "&Finalize Stock with " + termNameForBondedWarehouse;
			}
		}

		void SendAESMessageMenuItem_Click(object sender, EventArgs e)
		{
			if (Declaration.LockSendCustomsMessageMutex)
			{
				try
				{
					AESMessageHandler.SendMessage(Declaration, new Customs.Business.MessageSender.SaveEventHandler(() => MainForm.FireSaveButton()));
				}
				finally
				{
					Declaration.UnlockSendCustomsMessageMutex();
				}
			}
			else
			{
				Globals.Message.ShowInformation(Res.GetString("60902317-5F70-4BEB-BBE4-E3D40F7FBAF2", "Unable to file AES message; {0} is in the process of filing AES message.\r\nPlease wait until the sending process is finished before trying again.\r\nPlease ensure that you re-open the form to pickup the latest changes.", Declaration.GetSendCustomsMessageMutexInfo()));
			}
		}

		void SendStandalonePriorNotice_Click(object sender, EventArgs e)
		{
			if (!Declaration.CanHavePGAFDA)
			{
				Globals.Message.ShowInformation(NotSupportedByCBP);
				return;
			}
			var messageSender = CreateMessageSender<StandAlonePriorNoticeMessageSender>(Declaration);
			messageSender.OnPrepare += new StandAlonePriorNoticeMessageSender.PrepareEventHandler(ShowImportMessageSendingActionForm);
			messageSender.OnAllowNotifications += new Customs.Business.MessageSender.AllowNotificationsEventHandler(ContinueWithNotifications);
			messageSender.SendMessage();
		}

		void SendDrawbackSummaryMessageMenuItem_Click(object sender, EventArgs e)
		{
			SendDrawbackSummaryMessage(UpdateActionCode.Add);
		}

		void SendDrawbackSummaryReplaceMessageMenuItem_Click(object sender, EventArgs e)
		{
			SendDrawbackSummaryMessage(UpdateActionCode.Replace);
		}

		void SendDrawbackSummaryDeleteMessageMenuItem_Click(object sender, EventArgs e)
		{
			SendDrawbackSummaryMessage(UpdateActionCode.Delete);
		}

		void SendDrawbackSummaryMessage(UpdateActionCode actionCode)
		{
			if (Declaration.IsACEDrawback)
			{
				if (CheckHasChanges() && ValidateForACEDrawback())
				{
					var actionDescription = ZString.Empty;
					if (actionCode == UpdateActionCode.Add)
					{
						actionDescription = "Original";
					}
					else if (actionCode == UpdateActionCode.Replace)
					{
						actionDescription = "Replacement";
					}

					if (!actionDescription.IsEmpty)
					{
						var isWaitingForResponse = DrawbackSummaryStatusList.IsACEDrawbackAwaitingForResponse(Declaration.JE_MessageStatus);
						if (!isWaitingForResponse || Globals.Message.Show(Res.GetString("350C39F9-5F06-4B31-8560-C2A81AF42940", "This Drawback is waiting for Customs response.\r\nAre you sure you want to resend to Customs?"), Res.GetString("839262E3-299F-46A9-A7B1-C684B5A72059", "Warning"), MessageBoxButtons.YesNo, MessageBoxIcon.Warning, DialogResult.Yes) == DialogResult.Yes)
						{
							var sendingAction = new ACEDrawbackAcknowledgeAndSign(Declaration, actionCode);
							ZFormModaliser.ShowDialogAndDispose(new ACEDrawbackMessageSendingForm(sendingAction));

							if (sendingAction.ShouldSendMessage)
							{
								try
								{
									sendingAction.GenerateMessages();
									if (MainForm.FireSaveButton() == ContinueWithSave.Yes)
									{
										Globals.Message.ShowInformation("ACE Drawback Summary " + actionDescription + " Message Sent.");
									}
								}
								catch (ZSaveException ex)
								{
									ZExceptionReporting.HandleSaveException(ex);
								}
							}
						}
					}
				}
			}
			else
			{
				var messageSender = CreateMessageSender<DrawbackSummaryMessageSender>(Declaration, actionCode);
				messageSender.OnAllowNotifications += new Customs.Business.MessageSender.AllowNotificationsEventHandler(ContinueWithNotifications);
				messageSender.SendMessage();
			}
		}

		bool ValidateForACEDrawback()
		{
			var validation = MessageSendingValidation.New(Declaration, null);
			var notifications = validation.CheckBusinessObjectLevelValidation(MessageSendingValidation.ErrorExistHeaderText, "There are following message errors.", "Do you want to proceed despite these errors?");

			return ContinueWithNotifications(notifications);
		}

		void ResetDutyCalculationDate_Click(object sender, EventArgs e)
		{
			var message = Declaration?.ResetDutyCalculationDate();
			if (!string.IsNullOrEmpty(message))
			{
				Globals.Message.Show(message);
			}
		}

		void RefreshTariffDetails_Click(object sender, EventArgs e)
		{
			Declaration.RefreshTariff();
			Globals.Message.Show("Refreshing Tariffs complete.");
		}

		void RequestTariffUpdates_Click(object sender, EventArgs e)
		{
			if (Declaration.InvoiceLines.Count > 0)
			{
				var messageSender = CreateMessageSender<RequestTariffUpdatesMessageSender>(Declaration);
				messageSender.SendMessage();
			}
			else
			{
				Globals.Message.Show("This Declaration does not have Invoice Lines.");
			}
		}

		void RefreshNotificationDispositionActions_Click(object sender, EventArgs e)
		{
			Declaration.ReCalculateCRLAction();
			Declaration.ReCalculateENSAction();
			Globals.Message.ShowInformation("Declaration's notification disposition actions have been updated.");
		}

		void RefreshExRates_Click(object sender, EventArgs e)
		{
			Declaration.RefreshExchangeRates();
		}

		void ExportToBIRD7501File_Click(object sender, EventArgs e)
		{
			ExportToBIRD7501(delegate
			{
				new BIRDFileOutputHelper().BuildAMessage(
						Declaration,
						new BuildMessage(new BIRDEntrySummaryMessageBuilder(Declaration.ActiveEntryHeaders.EntrySummaryEntry).PopulateMessage),
						"7501");
			});
		}

		void ExportToBIRD7501(Action doDataTransfer)
		{
			if (!Declaration.US_EnableENS)
			{
				Globals.Message.ShowError("This Declaration does not have 7501 enabled.", "Export to BIRD 7501");
			}
			else
			{
				if (CheckHasChangesAndMergeIfRelatedEntryDoesNotExist(new string[] { CusEntryHeaderMessageTypeList.Codes.EntrySummary }))
				{
					if (ValidateForBIRDExport(ValidationModes.EntrySummary))
					{
						doDataTransfer();
					}

					Declaration.RecalculateValidationModesOnDeclaration();
				}
			}
		}

		void ExportToBIRD3461File_Click(object sender, EventArgs e)
		{
			ExportToBIRD3461(delegate
				{
					new BIRDFileOutputHelper().BuildAMessage(
						Declaration,
						new BuildMessage(delegate
						{
							if (Declaration.IsBorderMovement)
							{
								return new BIRDBorderCargoReleaseMessageBuilder(Declaration.ActiveEntryHeaders.CargoReleaseEntry).PopulateMessage();
							}
							else
							{
								var entry = Declaration.IsACECargoRelease ? Declaration.ActiveEntryHeaders.SimplifiedEntry :
										Declaration.ActiveEntryHeaders.CargoReleaseEntry;
								return new BIRDCargoReleaseMessageBuilder(entry).PopulateMessage();
							}
						}),
						"3461");
				});
		}

		void ExportToBIRD3461(Action doDataTransfer)
		{
			if (!Declaration.US_EnableCRL)
			{
				Globals.Message.ShowError("This Declaration does not have 3461 enabled.", "Export to BIRD 3461");
			}
			else
			{
				if (CheckHasChangesAndMergeIfRelatedEntryDoesNotExist(
						new string[] {
										CusEntryHeaderMessageTypeList.Codes.CargoRelease,
										CusEntryHeaderMessageTypeList.Codes.BorderCargoRelease,
										CusEntryHeaderMessageTypeList.Codes.ACECargoRelease
									}))
				{
					if (ValidateForBIRDExport(ValidationModes.CargoRelease))
					{
						doDataTransfer();
					}

					Declaration.RecalculateValidationModesOnDeclaration();
				}
			}
		}

		void SendEntryQueryBIRDFile_Click(object sender, EventArgs e)
		{
			SendEntryQueryBIRD(delegate
			{
				new BIRDFileOutputHelper().BuildAMessage(
					Declaration,
					new BuildMessage(delegate
					{
						return new BIRDEntrySummaryQueryMessageBuilder(Declaration.ActiveEntryHeaders.EntrySummaryEntry).PopulateMessage();
					}),
					"JI");
			});
		}

		void SendEntryQueryBIRD(Action send)
		{
			if (!Declaration.US_EnableENS)
			{
				Globals.Message.ShowError("This Declaration does not have 7501 enabled.", "Entry Summary Query");
			}
			else
			{
				if (CheckHasChangesAndMergeIfRelatedEntryDoesNotExist(new string[] { CusEntryHeaderMessageTypeList.Codes.EntrySummary }))
				{
					send();
				}
			}
		}

		void ExportBIRDLiquidationFile_Click(object sender, EventArgs e)
		{
			if (Declaration.Liquidations.Count == 0)
			{
				Globals.Message.ShowError(string.Format(NoMessageToExportAsBIRD, "Liquidation Notices"), "BIRD Liquidation");
			}
			else
			{
				if (CheckHasChangesAndMergeIfRelatedEntryDoesNotExist(Array.Empty<string>()))
				{
					new BIRDFileOutputHelper().BuildAMessage(
					Declaration,
					new BuildMessage(delegate
					{
						BIRDLiquidationMessageBuilder builder = new BIRDLiquidationMessageBuilder(Declaration);
						return builder.PopulateMessage();
					}),
					"NR");
				}
			}
		}

		public const string NoMessageToExportAsBIRD = "This Declaration does not have any {0} from Customs.";

		void ExportBIRDCargoReleaseProcessingResultFile_Click(object sender, EventArgs e)
		{
			MQEDIMessage message = (MQEDIMessage)Declaration.Messages.GetLastMessage(MQEDIMessage.ApplicationCodes.USCustomsImport, ApplicationIdentifierCodeList.Codes.CargoReleaseProcessingResults);

			if (message == null)
			{
				Globals.Message.ShowError(string.Format(NoMessageToExportAsBIRD, "Cargo Release Processing Results"), "BIRD Cargo Release Processing Results");
			}
			else
			{
				if (CheckHasChangesAndMergeIfRelatedEntryDoesNotExist(Array.Empty<string>()))
				{
					new BIRDFileOutputHelper().BuildAMessage(
					Declaration,
					new BuildMessage(delegate
					{
						BIRDStatusMessageBuilder builder = new BIRDStatusMessageBuilder(Declaration);
						return builder.BuildTheLatestCargoProcessingResult();
					}),
					"STAT");
				}
			}
		}

		void ExportBIRDEntrySummaryQueryResponseFile_Click(object sender, EventArgs e)
		{
			MQEDIMessage message = Declaration.ActiveEntryHeaders.EntrySummaryEntry == null ? null :
				(MQEDIMessage)Declaration.ActiveEntryHeaders.EntrySummaryEntry.Messages.GetLastMessage(MQEDIMessage.ApplicationCodes.USCustomsImport, ApplicationIdentifierCodeList.Codes.QueryEntrySummaryResponse);

			if (message == null)
			{
				Globals.Message.ShowError(string.Format(NoMessageToExportAsBIRD, "Entry Summary Query response"), "BIRD Entry Summary Query Response");
			}
			else
			{
				if (CheckHasChangesAndMergeIfRelatedEntryDoesNotExist(Array.Empty<string>()))
				{
					new BIRDFileOutputHelper().BuildAMessage(
					Declaration,
					new BuildMessage(delegate
					{
						BIRDEntrySummaryQueryResponseMessageBuilder builder = new BIRDEntrySummaryQueryResponseMessageBuilder(Declaration.ActiveEntryHeaders.EntrySummaryEntry);
						return builder.PopulateMessage();
					}),
					"JR");
				}
			}
		}

		bool ValidateForBIRDExport(ValidationModes validationMode)
		{
			Declaration.ValidationModes = validationMode;

			Declaration.RunPreSaveValidation();

			MessageSendingValidation validation = MessageSendingValidation.New(Declaration, null);
			MessageSendingNotificationCollection notifications = validation.CheckBusinessObjectLevelValidation(MessageSendingValidation.ErrorExistHeaderText, "There are following message errors.", "Do you want to proceed despite these errors?");

			return ContinueWithNotifications(notifications);
		}

		void ExportToBIRDEN_Click(object sender, EventArgs e)
		{
			if (!Declaration.US_EnableENS && !Declaration.US_EnableCRL)
			{
				Globals.Message.ShowError("This Declaration does not have 7501 or 3461 enabled.", "BIRD EN");
			}
			else
			{
				if (CheckHasChangesAndMergeIfRelatedEntryDoesNotExist(
						new string[] {
										CusEntryHeaderMessageTypeList.Codes.EntrySummary,
										CusEntryHeaderMessageTypeList.Codes.CargoRelease,
										CusEntryHeaderMessageTypeList.Codes.BorderCargoRelease,
										CusEntryHeaderMessageTypeList.Codes.ACECargoRelease
									}))
				{
					new BIRDFileOutputHelper().BuildAMessage(
						Declaration,
						new BuildMessage(delegate
							{
								var entry = Declaration.ActiveEntryHeaders.EntrySummaryEntry ?? Declaration.ActiveEntryHeaders.CargoReleaseEntry ?? Declaration.ActiveEntryHeaders.SimplifiedEntry;

								var builder = new BIRDStatusMessageBuilder(entry);
								return builder.BuildENRecord();
							}),
						"STAT");
				}
			}
		}

		void ExportToBIRDDT_Click(object sender, EventArgs e)
		{
			if (CheckHasChangesAndMergeIfRelatedEntryDoesNotExist(
						new string[] {
										CusEntryHeaderMessageTypeList.Codes.EntrySummary,
										CusEntryHeaderMessageTypeList.Codes.CargoRelease,
										CusEntryHeaderMessageTypeList.Codes.BorderCargoRelease,
										CusEntryHeaderMessageTypeList.Codes.ACECargoRelease
									}))
			{
				new BIRDFileOutputHelper().BuildAMessage(
					Declaration,
					new BuildMessage(delegate
						{
							var entry = Declaration.ActiveEntryHeaders.EntrySummaryEntry ?? Declaration.ActiveEntryHeaders.CargoReleaseEntry ?? Declaration.ActiveEntryHeaders.SimplifiedEntry;

							var builder = new BIRDStatusMessageBuilder(entry);
							return builder.BuildDTRecords();
						}),
					"STAT");
			}
		}

		void SendOriginalMessages_Click(object sender, EventArgs eventArg)
		{
			SendMessages(ImportMessageSendingMessageType.Original);
		}

		void SendAmendmentMessages_Click(object sender, EventArgs eventArg)
		{
			SendMessages(ImportMessageSendingMessageType.Replacement);
		}

		void SendWithdrawalMessages_Click(object sender, EventArgs eventArg)
		{
			SendMessages(ImportMessageSendingMessageType.Deletion);
		}

		void SendEntrySummaryOriginalMessages_Click(object sender, EventArgs eventArg)
		{
			SendMessages(ImportMessageSendingMessageType.Original, ImportMessageSendingMessageTypeAdditionalFilter.EntrySummary);
		}

		void SendEntrySummaryAmendmentMessages_Click(object sender, EventArgs eventArg)
		{
			SendMessages(ImportMessageSendingMessageType.Replacement, ImportMessageSendingMessageTypeAdditionalFilter.EntrySummary);
		}

		void SendEntrySummaryWithdrawalMessages_Click(object sender, EventArgs eventArg)
		{
			SendMessages(ImportMessageSendingMessageType.Deletion, ImportMessageSendingMessageTypeAdditionalFilter.EntrySummary);
		}

		void SendCargoReleaseOriginalMessages_Click(object sender, EventArgs eventArg)
		{
			SendMessages(ImportMessageSendingMessageType.Original, ImportMessageSendingMessageTypeAdditionalFilter.CargoRelease);
		}

		void SendCargoReleaseAmendmentMessages_Click(object sender, EventArgs eventArg)
		{
			SendMessages(ImportMessageSendingMessageType.Replacement, ImportMessageSendingMessageTypeAdditionalFilter.CargoRelease);
		}

		void SendCargoReleaseWithdrawalMessages_Click(object sender, EventArgs eventArg)
		{
			SendMessages(ImportMessageSendingMessageType.Deletion, ImportMessageSendingMessageTypeAdditionalFilter.CargoRelease);
		}

		void SendMessages(ImportMessageSendingMessageType originalAmendmentOrWithdraw, ImportMessageSendingMessageTypeAdditionalFilter additionalFilter = ImportMessageSendingMessageTypeAdditionalFilter.None)
		{
			SenderController.SendMessages<MainMessageSender>(originalAmendmentOrWithdraw, additionalFilter);
		}

		void QueryACEQuota_Click(object sender, EventArgs e)
		{
			var messageSender = CreateMessageSender<ACEQuotaQueryMessageSender>(Declaration);
			messageSender.OnPrepare += new ACEQuotaQueryMessageSender.PrepareEventHandler(ShowImportMessageSendingActionForm);
			messageSender.SendMessage();
		}

		void SendQuotaVisaOrADDCVDQuery(QueryTypeForQuotaVisaADDCVD queryType)
		{
			var messageSender = CreateMessageSender<QuotaVisaOrADDCVDQueryMessageSender>(Declaration, queryType);
			messageSender.OnPrepare += new QuotaVisaOrADDCVDQueryMessageSender.PrepareEventHandler(ShowImportMessageSendingActionForm);
			messageSender.SendMessage();
		}

		#region Send Antidumping and Countervailing Request Messages

		void QueryADDCVD_Click(object sender, EventArgs e)
		{
			var messageSender = CreateMessageSender<QueryADDCVDMessageSender>(Declaration);
			messageSender.SendMessage();
		}

		void QueryADDCVDByTariff_Click(object sender, EventArgs e)
		{
			SendQuotaVisaOrADDCVDQuery(QueryTypeForQuotaVisaADDCVD.ADDCVD);
		}

		#endregion

		void BillOfLadingUpdate_Click(object sender, EventArgs e)
		{
			Globals.Message.ShowInformation(NotSupportedByCBP);
		}

		void EntryDateUpdate_Click(object sender, EventArgs e)
		{
			Globals.Message.ShowInformation(NotSupportedByCBP);
		}

		internal const string NotSupportedByCBP = "This message is no longer supported by CBP.";

		void CensusWarningOverrideMessage_Click(object sender, EventArgs e)
		{
			if (CheckHasChangesAndMergeIfRelatedEntryDoesNotExist(new string[] { CusEntryHeaderMessageTypeList.Codes.EntrySummary }))
			{
				var entry = Declaration.ActiveEntryHeaders.EntrySummaryEntry;

				if (entry != null)
				{
					var coll = new EntryCensusWarningOverrideCollection(entry);
					CWOForm.Action action = CWOForm.Action.None;
					using (CWOForm form = new CWOForm(coll, CusEntryHeader.GetFormmattedFilerCodeAndEntryNumber(entry.EntryFilerCode, entry.EntryNumber)))
					{
						ZFormModaliser.ShowDialogWithoutDispose(form);
						action = form.ActionChosenByUsers;
					}

					if (action != CWOForm.Action.None)
					{
						PerformRequiredCWOAction(action, entry, coll);
					}
				}
			}
		}

		internal void PerformRequiredCWOAction(CWOForm.Action action, CusEntryHeader entry, EntryCensusWarningOverrideCollection coll)
		{
			var manager = new CensusWarningOverrideMessageManager();
			var notifications = action == CWOForm.Action.Save ? manager.GetNotificationsForSavingCWO(entry) : manager.GetNotificationsForSendingCWO(entry);

			if (notifications != null && ContinueWithNotifications(notifications))
			{
				coll.CopyToEntryLines();

				if (MainForm.FireSaveButton() == ContinueWithSave.Yes)
				{
					if (action == CWOForm.Action.Send)
					{
						manager.SendMessage(entry);
					}

					try
					{
						entry.Factory.Save();
						if (action == CWOForm.Action.Send)
						{
							Globals.Message.ShowInformation("CWO Message Sent");
						}
					}
					catch (ZSaveException e)
					{
						ZExceptionReporting.HandleSaveException(e);
					}
				}
			}
		}

		void CensusWarningQueryMessage_Click(object sender, EventArgs e)
		{
			if (CheckEntryCannotBeSent(CensusWarning))
			{
				return;
			}

			new CensusWarningQueryMessageBuilder(Declaration.ActiveEntryHeaders.EntrySummaryEntry).PopulateMessage();
			DoSave(CensusWarning);
		}

		void ResetToOriginal_Click(object sender, EventArgs e)
		{
			if (Declaration != null)
			{
				Declaration.ResetToOriginal();
			}
		}

		internal const string EntryNotAccepted = "Entry has not been accepted by Customs. ";
		internal const string NoFormalEntries = "Entry does not exist. ";
		internal const string QryNotRequired = "{0} Query message cannot be sent.";
		internal const string CensusWarning = "Census Warning";
		internal const string EntrySummary = "Entry Summary";

		#region Statement Delete/Add

		void StatementDeleteAddMessage_Click(object sender, EventArgs e)
		{
			StatementDeleteAndSendingActionCollection messageSendingActions = GetActionCollection(Declaration);
			MessageSendingNotificationCollection notifications = new StatementMessageSendingValidator().GetNotificationsForStatementDeleteAdd(Declaration);

			var continueWithSubmit = true;
			if (Declaration.HasChanges)
			{
				if (Globals.Message.Show(Res.GetString("462D3996-E096-40C9-B857-55055F630871", "The Job has not yet been saved. Do you want to save and proceed?"),
					Res.GetString("2E5D46BC-498F-4B1E-AAB9-8728D38D0C68", "Save Job"), MessageBoxButtons.YesNo, MessageBoxIcon.Warning, DialogResult.Yes) == DialogResult.Yes)
				{
					continueWithSubmit = Form.FireSaveButton() == ContinueWithSave.Yes;
				}
			}
			if (continueWithSubmit)
			{
				StatementMessagesHandler.SendDeleteAddMessage(messageSendingActions, notifications, Declaration.Factory);
			}
		}

		StatementDeleteAndSendingActionCollection GetActionCollection(JobDeclaration declaration)
		{
			return new StatementDeleteAndSendingActionCollection(declaration);
		}

		StatementMessagesHandler StatementMessagesHandler
		{
			get { return statementMessagesHandler ?? (statementMessagesHandler = new StatementMessagesHandler()); }
		}
		StatementMessagesHandler statementMessagesHandler;

		#endregion

		void EntrySummaryQuery_Click(object sender, EventArgs e)
		{
			var continueWithSubmit = true;
			if (Declaration.HasChanges)
			{
				if (Globals.Message.Show(Res.GetString("8336EB96-B30C-4D68-B1FE-269C3DFA5EBB", "The Job has not yet been saved. Do you want to save and proceed?"),
					Res.GetString("9B4E5756-8D85-4C61-B282-C45E684A869E", "Save Job"), MessageBoxButtons.YesNo, MessageBoxIcon.Warning, DialogResult.Yes) == DialogResult.Yes)
				{
					continueWithSubmit = Form.FireSaveButton() == ContinueWithSave.Yes;
				}
				else
				{
					continueWithSubmit = false;
				}
			}

			if (continueWithSubmit)
			{
				if (CheckEntrySummaryQueryCannotBeSent())
				{
					return;
				}

				new ACEEntrySummaryQueryMessageBuilder(Declaration.ActiveEntryHeaders.EntrySummaryEntry).PopulateMessage();
				DoSave(EntrySummary);
			}
		}

		void DoSave(string messageText)
		{
			try
			{
				if (MainForm.FireSaveButton() == ContinueWithSave.Yes)
				{
					Globals.Message.ShowInformation(string.Format("{0} Query Message Sent", messageText));
				}
			}
			catch (ZSaveException ex)
			{
				ZExceptionReporting.HandleSaveException(ex);
			}
		}

		bool CheckEntryCannotBeSent(string messageText)
		{
			var entry = Declaration.ActiveEntryHeaders.EntrySummaryEntry;
			if (entry == null || !entry.HasBeenLodgedAtCustoms)
			{
				Globals.Message.ShowError((entry == null ? NoFormalEntries : EntryNotAccepted)
					+ string.Format(QryNotRequired, messageText), messageText + " Query");

				return true;
			}
			return false;
		}

		bool CheckEntrySummaryQueryCannotBeSent()
		{
			var entry = Declaration.ActiveEntryHeaders.EntrySummaryEntry;
			bool result = entry == null;
			if (result)
			{
				Globals.Message.ShowInformation("Entry Summary entry does not exists for this job.");
			}

			if (!result && !entry.HasBeenLodgedAtCustoms && !entry.HasBeenWithdrawn)
			{
				if (Globals.Message.Show("The entry has not been lodged at Customs. Are you sure you wish to continue?", "Query", MessageBoxButtons.YesNo, DialogResult.No) == DialogResult.No)
				{
					result = true;
				}
			}

			return result;
		}

		void SendFDACorrectionMessages_Click(object sender, EventArgs e)
		{
			Globals.Message.ShowInformation(NotSupportedByCBP);
		}

		void RequestToExtendTIB_Click(object sender, EventArgs e)
		{
			var messageSender = CreateMessageSender<RequestToExtensionOrClosureTIBMessageSender>(Declaration, ImportMessageSendingMessageType.ExtendTIB);
			messageSender.OnPrepare += new RequestToExtensionOrClosureTIBMessageSender.PrepareEventHandler(ShowImportMessageSendingActionForm);
			messageSender.SendMessage();
		}

		void RequestToClosureTIB_Click(object sender, EventArgs e)
		{
			var messageSender = CreateMessageSender<RequestToExtensionOrClosureTIBMessageSender>(Declaration, ImportMessageSendingMessageType.ClosureTIB);
			messageSender.OnPrepare += new RequestToExtensionOrClosureTIBMessageSender.PrepareEventHandler(ShowImportMessageSendingActionForm);
			messageSender.SendMessage();
		}

		void CargoManifestStatusQuery_Click(object sender, EventArgs e)
		{
			if (PreSaveDeclaration(Declaration))
			{
				var sendingHeader = new CargoManifestStatusQueryHeaderObject(Declaration);
				if (sendingHeader.SendingObjects.Count > 0)
				{
					var cargoManifestStatusQueryForm = new CargoManifestStatusQueryActionForm(sendingHeader);
					ZFormModaliser.ShowDialogAndDispose(cargoManifestStatusQueryForm);

					if (sendingHeader.ShouldSendMessage)
					{
						var messagesCount = sendingHeader.SendQueryMessage();
						if (messagesCount > 0)
						{
							DoSave("Cargo Manifest Status");
						}
					}
				}
				else
				{
					Globals.Message.ShowInformation(ResString.GetMultilingualString("DBEB3753-7369-4E4B-AEE3-9AD9B50F5D61", "There is no Entry/Bills available for sending ACE Cargo/Manifest query message to Customs."), ResString.GetMultilingualString("F02DA033-A476-4BF1-8D28-9262CC5E46C5", "No Entry/Bills"));
				}
			}
		}

		void ConsigneeNameAddressAdd_Click(object sender, EventArgs e)
		{
			Globals.Message.ShowInformation(NotSupportedByCBP);
		}

		void ImportInvoicesMenuItem_Click(object sender, EventArgs e)
		{
			var module = (ZFilterModule)ZModuleFactory.Instance.Create(ModuleIDs.Customs.JobDeclaration, Core.Constants.CountryCodes.UnitedStates);
			module.FilterBusinessObject.SetExternalDefaults(GetFilterDefaults());

			var popup = new CustomsEmbeddedModulePopup(module);
			var strategy = new JobDeclarationBulkImportPopupOKButtonStrategy(popup, Declaration);
			popup.EmbeddedModulePopupOKButtonStrategy = strategy;
			module.OverrideModuleDecisionProvider(strategy.ModuleDecisionProvider);
			ZFormModaliser.Show(popup, MainForm);
		}

		FilterBusinessObjectDefaults GetFilterDefaults()
		{
			var result = new FilterBusinessObjectDefaults();
			result.Add(new FilterBusinessObjectDefault(JobDeclaration.Constants.USFilterConstants.ShipmentType, "Property", new ZString(JobMessageTypeList.Codes.Import)));
			result.Add(new FilterBusinessObjectDefault(JobDeclaration.Constants.USFilterConstants.ReleaseStatus, "Property", new ZString(CRLReleaseStatusList.Codes.REL)));
			if (Declaration.IOROrgPK.IsValid)
			{
				result.Add(new FilterBusinessObjectDefault(JobDeclaration.Constants.USFilterConstants.ImporterOfRecord, "Property", Declaration.IOROrgPK));
			}
			if (Declaration.Consignee != null && Declaration.Consignee.PK.IsValid)
			{
				result.Add(new FilterBusinessObjectDefault(JobDeclaration.Constants.USFilterConstants.UltinateConsignee, "Property", Declaration.Consignee.PK));
			}
			if (Declaration.US_SchDEntry.IsValid)
			{
				result.Add(new FilterBusinessObjectDefault(JobDeclaration.Constants.USFilterConstants.PortOfEntry, "Property", Declaration.US_SchDEntry));
			}
			result.Add(new FilterBusinessObjectDefault(JobDeclaration.Constants.USFilterConstants.ConsolidatedJobNo, "ComparisonOperator", new ZString("is blank"), true));

			var validReleaseDate = Declaration.GetValidReleaseDate();
			result.Add(new FilterBusinessObjectDefault(JobDeclaration.Constants.USFilterConstants.ReleaseDate, "PropertySearch", new ZString("Date Range")));
			result.Add(new FilterBusinessObjectDefault(JobDeclaration.Constants.USFilterConstants.ReleaseDate, "Property1", validReleaseDate));
			result.Add(new FilterBusinessObjectDefault(JobDeclaration.Constants.USFilterConstants.ReleaseDate, "Property2", ZDateTime.Today));
			return result;
		}

		void ShowDiscardedAndInactiveMessages_Clicked(object sender, EventArgs e)
		{
			showDiscardedAndInactiveMessagesMenu.Checked = !showDiscardedAndInactiveMessagesMenu.Checked;

			Declaration.InBondRelatedRecords.ReBuild(showDiscardedAndInactiveMessagesMenu.Checked ? MessagesToShowCollection.MessagesStatus.InactiveOnly : MessagesToShowCollection.MessagesStatus.ActiveOnly);
		}

		void CopyPreviousFDALine_Click(object sender, EventArgs e)
		{
			copyPreviousFDALineMenu.Checked = !copyPreviousFDALineMenu.Checked;
			Declaration.CopyLastFDADetailsToNewLine = copyPreviousFDALineMenu.Checked;
		}

		void CopyPreviousPGALine_Click(object sender, EventArgs e)
		{
			copyPreviousPGALineMenu.Checked = !copyPreviousPGALineMenu.Checked;
			Declaration.CopyLastPGADetailsToNewLine = copyPreviousPGALineMenu.Checked;
		}

		#region Send FTZ

		void FTZOriginalMessage_Click(object sender, EventArgs e)
		{
			SendFTZMessage(UpdateActionCode.Add);
		}

		void FTZAmendmentMessage_Click(object sender, EventArgs e)
		{
			SendFTZMessage(UpdateActionCode.Replace);
		}

		void FTZWithdrawalMessage_Click(object sender, EventArgs e)
		{
			SendFTZMessage(UpdateActionCode.Delete);
		}

		void SendFTZMessage(UpdateActionCode actionCode)
		{
			var ftzMessageSendingObject = new FTZMessageSendingObject(Declaration, actionCode);

			if (actionCode == UpdateActionCode.Replace)
			{
				ZFormModaliser.ShowDialogAndDispose(new FTZMessageSendingForm(ftzMessageSendingObject));
			}
			else
			{
				ftzMessageSendingObject.ShouldSendMessage = true;
			}

			if (ftzMessageSendingObject.ShouldSendMessage)
			{
				SendFTZMessageCore(ftzMessageSendingObject);
			}
		}

		void SendFTZMessageCore(FTZMessageSendingObject ftzMessageSendingObject)
		{
			var needsToRestoreToPreMessagingState = false;
			Action restoreToPreMessagingState = null;
			var actionCode = ftzMessageSendingObject.ActionCode;

			try
			{
				Func<PublishToUniversalResult> preMessagingAction = null;
				var isBondedWarehouse = !Declaration.IsBondedWarehousingDisabled && Declaration.IsWHSUniversalXMLActive && (Declaration.HasWHSTransaction || Declaration.HasLineGoingIntoAnAutomatedBondedWarehouse);
				if (isBondedWarehouse)
				{
					switch (actionCode)
					{
						case UpdateActionCode.Delete:
							if (Declaration.HasWHSTransaction && Declaration.WarehouseTransactionStatus != WarehouseTransactionStatusList.Codes.InwardCanceledPendingWithdrawal)
							{
								preMessagingAction = () => Declaration.PublishCancelEventForWHSInwardAndSaveIfNeeded(true, WarehouseTransactionStatusList.Codes.InwardCanceledPendingWithdrawal);
								restoreToPreMessagingState = Declaration.RestoreLatestClearedBondedWarehouseInwardInADifferentFactory;
							}
							break;
						case UpdateActionCode.Add:
						case UpdateActionCode.Replace:
							preMessagingAction = () => Declaration.PublishShipmentForWHSInward(true);
							restoreToPreMessagingState = Declaration.RestoreBondedWarehouseInwardInADifferentFactory;
							break;
					}
				}
				var messageSender = CreateMessageSender<FTZMessageSender>(ftzMessageSendingObject);
				messageSender.OnPrepare += new FTZMessageSender.PrepareEventHandler(() =>
				{
					var isOKToContinue = true;
					if (isBondedWarehouse)
					{
						isOKToContinue = CheckRequiredFieldsForBondedWarehousingAreEntered(checkProduct: actionCode == UpdateActionCode.Add || actionCode == UpdateActionCode.Replace || actionCode == UpdateActionCode.Update,
										checkQuantity: actionCode == UpdateActionCode.Add || actionCode == UpdateActionCode.Replace || actionCode == UpdateActionCode.Update,
										checkEntryDetails: actionCode == UpdateActionCode.Add || actionCode == UpdateActionCode.Replace || actionCode == UpdateActionCode.Update);
						if (isOKToContinue && preMessagingAction != null)
						{
							isOKToContinue = Declaration.MessageInitiator.IsPublishToUniversalTransactionOK(preMessagingAction());
							needsToRestoreToPreMessagingState = isOKToContinue;
						}
					}
					return isOKToContinue;
				});
				messageSender.OnAllowNotifications += new Customs.Business.MessageSender.AllowNotificationsEventHandler(ContinueWithNotifications);
				if (messageSender.SendMessage())
				{
					needsToRestoreToPreMessagingState = false;
				}
			}
			finally
			{
				if (needsToRestoreToPreMessagingState && restoreToPreMessagingState != null)
				{
					restoreToPreMessagingState();
				}
			}
		}

		void PTTMessage_Click(object sender, EventArgs e)
		{
			SendOrCancelPermitToTransferMessage(PTTSendingOption.SendPTTMessage);
		}

		void CancelPTTMessage_Click(object sender, EventArgs e)
		{
			SendOrCancelPermitToTransferMessage(PTTSendingOption.CancellPTTMessage);
		}

		void PTTArrival_Click(object sender, EventArgs e)
		{
			SendOrCancelPermitToTransferMessage(PTTSendingOption.SendPTTArrival);
		}

		void SendPTTUnArrival_Click(object sender, EventArgs e)
		{
			SendOrCancelPermitToTransferMessage(PTTSendingOption.SendPTTUnArrival);
		}

		void SendOrCancelPermitToTransferMessage(PTTSendingOption sendingOption)
		{
			var messageSender = CreateMessageSender<PTTMessageSender>(Declaration, sendingOption);
			messageSender.OnAllowNotifications += new Customs.Business.MessageSender.AllowNotificationsEventHandler(ContinueWithNotifications);
			messageSender.SendMessage();
		}

		void SendConcurrence_Click(object sender, EventArgs e)
		{
			var messageSender = CreateMessageSender<ConcurrenceDeliveryMessageSender>(Declaration, FZEventType.Concur);
			messageSender.OnPrepare += new ConcurrenceDeliveryMessageSender.PrepareEventHandler(ShowFZEventMessageSendingForm);
			messageSender.OnAllowNotifications += new Customs.Business.MessageSender.AllowNotificationsEventHandler(ContinueWithNotifications);
			messageSender.SendMessage();
		}

		void SendPostAdmissionCorrection_Click(object sender, EventArgs e)
		{
			var messageSender = CreateMessageSender<ConcurrenceDeliveryMessageSender>(Declaration, FZEventType.Unconcur);
			messageSender.OnPrepare += new ConcurrenceDeliveryMessageSender.PrepareEventHandler(ShowFZEventMessageSendingForm);
			messageSender.OnAllowNotifications += new Customs.Business.MessageSender.AllowNotificationsEventHandler(ContinueWithNotifications);
			messageSender.SendMessage();
		}

		bool ShowFZEventMessageSendingForm(FZEventAction action)
		{
			ZFormModaliser.ShowDialogAndDispose(new FZEventMessageSendingForm(action));
			return !action.IsCancelled;
		}

		void SendArrival_Click(object sender, EventArgs e)
		{
			var messageSender = CreateMessageSender<FZArrivalMessageSender>(Declaration);
			messageSender.OnAllowNotifications += new Customs.Business.MessageSender.AllowNotificationsEventHandler(ContinueWithNotifications);
			messageSender.SendMessage();
		}

		void SendDeliveryOfGoods_Click(object sender, EventArgs e)
		{
			var messageSender = CreateMessageSender<ConcurrenceDeliveryMessageSender>(Declaration, FZEventType.Delivery);
			messageSender.OnPrepare += new ConcurrenceDeliveryMessageSender.PrepareEventHandler(ShowFZEventMessageSendingForm);
			messageSender.OnAllowNotifications += new Customs.Business.MessageSender.AllowNotificationsEventHandler(ContinueWithNotifications);
			messageSender.SendMessage();
		}

		void FTZCargoManifestStatusQuery_Click(object sender, EventArgs e)
		{
			if (PreSaveDeclaration(Declaration))
			{
				var sendingHeader = new CargoManifestStatusQueryHeaderObject(Declaration);
				if (sendingHeader.SendingObjects.Count > 0)
				{
					var cargoManifestStatusQueryForm = new CargoManifestStatusQueryActionForm(sendingHeader);
					ZFormModaliser.ShowDialogAndDispose(cargoManifestStatusQueryForm);

					if (sendingHeader.ShouldSendMessage)
					{
						var messagesCount = sendingHeader.SendQueryMessage();
						if (messagesCount > 0)
						{
							DoSave("FTZ Cargo Manifest Status");
						}
					}
				}
				else
				{
					Globals.Message.ShowInformation(ResString.GetMultilingualString("1a614015-2216-4b3a-a7c9-ea05d9992c28", "There is no Entry/Bills available for sending Cargo Manifest Status query message to Customs."), ResString.GetMultilingualString("F02DA033-A476-4BF1-8D28-9262CC5E46C5", "No Entry/Bills"));
				}
			}
		}

		#endregion

		#region implementation

		protected override Customs.GUI.BondedWarehouseOperationDeterminer GetNewBondedWarehouseOperationDeterminer(IWarehouseIntegrationSupporter supporter)
		{
			var declaration = supporter as JobDeclaration;
			if (declaration == null)
			{
				ErrorReporter.ReportOnce("For US, supporter must be JobDeclaration");
				return null;
			}
			else
			{
				return new BondedWarehouseOperationDeterminer(declaration);
			}
		}

		T CreateMessageSender<T>(params object[] args)
			where T : MessageSender
		{
			T result = (T)Activator.CreateInstance(typeof(T), args);
			result.OnSave += new Customs.Business.MessageSender.SaveEventHandler(PerformSave);
			return result;
		}

		ZForm MainForm
		{
			get { return (ZForm)GetMainMenu().GetForm(); }
		}

		void PerformSave()
		{
			MainForm.FireSaveButton();
		}

#if DEBUG
		protected virtual // don`t override! For DynamicMock only!
#endif
		bool ShowImportMessageSendingActionForm(ImportMessageSendingActionCollection actions)
		{
			ZFormModaliser.ShowDialogAndDispose(new ImportMessageSendingActionForm(actions));
			return !actions.IsCancelled;
		}

		#endregion

		#region overrides

		protected override void Dispose(bool disposing)
		{
			try
			{
				if (Declaration != null)
				{
					Declaration.UnlockImportEntryNumberAllocationMutex();
					Declaration.UnlockFTZAdmissionNumberAllocationMutex();
				}
			}
			finally
			{
				base.Dispose(disposing);
			}
		}

		protected override bool PerformMerge()
		{
			return SenderController.PerformMerge();
		}

		#region SenderController

		MessageSenderController SenderController
		{
			get => senderController ?? (senderController = new MessageSenderController(Declaration, new Lazy<ZForm>(() => MainForm), ShowImportMessageSendingActionForm));
		}
		MessageSenderController senderController;

		internal bool ContinueWithNotifications(MessageSendingNotificationCollection notifications)
		{
			return SenderController.ContinueWithNotifications(notifications);
		}

		bool CheckHasChangesAndMergeIfRelatedEntryDoesNotExist(string[] messageTypes)
		{
			return SenderController.CheckHasChangesAndMergeIfRelatedEntryDoesNotExist(messageTypes);
		}

		#endregion

		#region MenuItems

		MenuItem resetDutyCalculationDateMenuItem;
		internal MenuItem aESMessagingMenuItem;
		internal MenuItem drawbackSummaryMenuItem;
		internal MenuItem drawbackSummaryReplaceMenuItem;
		internal MenuItem drawbackSummaryDeleteMenuItem;
		MenuItem drawbackBulkImportEntryLinesMenuItem;
		internal List<MenuItem> importCommonMenus;
		internal MenuItem importInvoicesMenuItem;
		internal MenuItem sendOriginalMenu;
		internal MenuItem sendAmendmentMenu;
		internal MenuItem sendWithdrawalMenu;
		internal MenuItem sendEntrySummaryMenu;
		internal MenuItem sendEntrySummaryOriginalMenu;
		internal MenuItem sendEntrySummaryAmendmentMenu;
		internal MenuItem sendEntrySummaryWithdrawalMenu;
		internal MenuItem sendCargoReleaseMenu;
		internal MenuItem sendCargoReleaseOriginalMenu;
		internal MenuItem sendCargoReleaseAmendmentMenu;
		internal MenuItem sendCargoReleaseWithdrawalMenu;
		MenuItem sendStandalonePriorNoticeMenu;
		internal MenuItem sendFDACorrectionMessagesMenu;
		internal MenuItem sendFTZCargoManifestStatusQueryMenu;
		internal MenuItem cargoManifestStatusQueryMenu;
		MenuItem sendConsigneeNameAddressAddMenu;
		MenuItem sendEntryDateUpdateMenu;
		internal MenuItem sendBillOfLadingUpdateMenu;
		internal MenuItem requestToExtendTIBMenu;
		internal MenuItem requestToClosureTIBMenu;
		internal MenuItem queryQuotaMenu;
		internal MenuItem sendEntrySummaryQueryMenu;
		internal MenuItem queryADDCVDMenu;
		internal MenuItem queryADDCVDByTariffMenu;
		internal MenuItem statementDeleteAddMessageMenu;
		internal MenuItem otherMenuItems;
		internal MenuItem queryMenuItems;
		MenuItem dividerMenuBetweenMainMessagingAndAncillaryMessaging;
		MenuItem showDiscardedAndInactiveMessagesMenu;

		internal MenuItem bIRDExportMenu;
		internal MenuItem exportToBIRD7501Menu;
		internal MenuItem exportToBIRD3461Menu;
		internal MenuItem exportToBIRDEntryQueryMenu;
		internal MenuItem exportToBIRDLiquidationMenu;
		internal MenuItem exportToBIRDCargoReleaseProcessingResultMenu;
		internal MenuItem exportToBIRDEntrySummaryQueryResponseMenu;
		internal MenuItem exportToBIRDENMenu;
		internal MenuItem exportToBIRDDTMenu;
		internal MenuItem requestTariffUpdateMenu;
		internal MenuItem refreshNotificationDispositionActions;

		internal MenuItem moreAuditsMenu;
		internal MenuItem resetToOriginalMenu;
		internal MenuItem copyPreviousFDALineMenu;

		MenuItem copyPreviousPGALineMenu;
		MenuItem copyPreviousPGALineMenuDivider;

		MenuItem censusWarningOverrideMessageMenu;
		internal MenuItem censusWarningQueryMessageMenu;

		MenuItem fDAMenuItem;

		internal MenuItem pGACorrectionMessageMenu;

		internal MenuItem sendPTTArrivalMenuItem;
		internal MenuItem sendPTTUnArrivalMenuItem;

		internal List<MenuItem> fTZMenus;

		#endregion

		protected override void SetupTopLevelMenu()
		{
			base.SetupTopLevelMenu();

			#region PGA

			importCommonMenus = new List<MenuItem>();

			copyPreviousPGALineMenuDivider = new ZMenuItem("-");
			importCommonMenus.Add(copyPreviousPGALineMenuDivider);

			copyPreviousPGALineMenu = new ZMenuItem(Constants.CopyPreviousPGALine, new EventHandler(CopyPreviousPGALine_Click));
			importCommonMenus.Add(copyPreviousPGALineMenu);

			#endregion

			aESMessagingMenuItem = new ZMenuItem("FILE AES Message", new EventHandler(SendAESMessageMenuItem_Click));
			this.MenuItems.Add(1, aESMessagingMenuItem);

			drawbackSummaryMenuItem = new ZMenuItem("Send Drawback Summary Add Message", new EventHandler(SendDrawbackSummaryMessageMenuItem_Click));
			this.MenuItems.Add(drawbackSummaryMenuItem);

			drawbackSummaryReplaceMenuItem = new ZMenuItem("Send Drawback Summary Replacement Message", new EventHandler(SendDrawbackSummaryReplaceMessageMenuItem_Click));
			this.MenuItems.Add(drawbackSummaryReplaceMenuItem);

			drawbackSummaryDeleteMenuItem = new ZMenuItem("Send Delete Drawback Summary Message", new EventHandler(SendDrawbackSummaryDeleteMessageMenuItem_Click));
			this.MenuItems.Add(drawbackSummaryDeleteMenuItem);

			importCommonMenus.Add(new ZMenuItem("-"));

			#region Send

			sendOriginalMenu = new ZMenuItem(Constants.SendOriginalMessages, new EventHandler(SendOriginalMessages_Click));
			importCommonMenus.Add(sendOriginalMenu);

			sendAmendmentMenu = new ZMenuItem(Constants.SendAmendmentMessages, new EventHandler(SendAmendmentMessages_Click));
			importCommonMenus.Add(sendAmendmentMenu);

			sendWithdrawalMenu = new ZMenuItem(Constants.SendWithdrawalMessages, new EventHandler(SendWithdrawalMessages_Click));
			importCommonMenus.Add(sendWithdrawalMenu);

			sendEntrySummaryOriginalMenu = new ZMenuItem(Constants.SendOriginalMessages, new EventHandler(SendEntrySummaryOriginalMessages_Click));
			sendEntrySummaryAmendmentMenu = new ZMenuItem(Constants.SendAmendmentMessages, new EventHandler(SendEntrySummaryAmendmentMessages_Click));
			sendEntrySummaryWithdrawalMenu = new ZMenuItem(Constants.SendWithdrawalMessages, new EventHandler(SendEntrySummaryWithdrawalMessages_Click));
			sendEntrySummaryMenu = new ZMenuItem(Constants.SendEntrySummaryMessages, new[] { sendEntrySummaryOriginalMenu, sendEntrySummaryAmendmentMenu, sendEntrySummaryWithdrawalMenu });
			importCommonMenus.Add(sendEntrySummaryMenu);

			sendCargoReleaseOriginalMenu = new ZMenuItem(Constants.SendOriginalMessages, new EventHandler(SendCargoReleaseOriginalMessages_Click));
			sendCargoReleaseAmendmentMenu = new ZMenuItem(Constants.SendAmendmentMessages, new EventHandler(SendCargoReleaseAmendmentMessages_Click));
			sendCargoReleaseWithdrawalMenu = new ZMenuItem(Constants.SendWithdrawalMessages, new EventHandler(SendCargoReleaseWithdrawalMessages_Click));
			sendCargoReleaseMenu = new ZMenuItem(Constants.SendCargoReleaseMessages, new[] { sendCargoReleaseOriginalMenu, sendCargoReleaseAmendmentMenu, sendCargoReleaseWithdrawalMenu });
			importCommonMenus.Add(sendCargoReleaseMenu);

			#endregion

			#region FTZ

			fTZMenus = new List<MenuItem>();
			fTZMenus.Add(new ZMenuItem("-"));
			var sendFTZOriginalMenuItem = new ZMenuItem(Constants.SendFTZOriginalMessages, new EventHandler(FTZOriginalMessage_Click));
			fTZMenus.Add(sendFTZOriginalMenuItem);

			var sendFTZAmendmentMenuItem = new ZMenuItem(Constants.SendFTZAmendmentMessages, new EventHandler(FTZAmendmentMessage_Click));
			fTZMenus.Add(sendFTZAmendmentMenuItem);

			var sendFTZWithdrawalMenuItem = new ZMenuItem(Constants.SendFTZWithdrawalMessages, new EventHandler(FTZWithdrawalMessage_Click));
			fTZMenus.Add(sendFTZWithdrawalMenuItem);

			fTZMenus.Add(new ZMenuItem("-"));
			var sendArrivalMenuItem = new ZMenuItem(Constants.SendArrival, new EventHandler(SendArrival_Click));
			fTZMenus.Add(sendArrivalMenuItem);

			var sendConcurrenceMenuItem = new ZMenuItem(Constants.SendConcurrence, new EventHandler(SendConcurrence_Click));
			fTZMenus.Add(sendConcurrenceMenuItem);

			var sendPostAdmissionCorrectionMenuItem = new ZMenuItem(Constants.SendPostAdmissionCorrection, new EventHandler(SendPostAdmissionCorrection_Click));
			fTZMenus.Add(sendPostAdmissionCorrectionMenuItem);

			var sendPTTMessagesMenuItem = new ZMenuItem(Constants.SendPTTMessages, new EventHandler(PTTMessage_Click));
			fTZMenus.Add(sendPTTMessagesMenuItem);

			var cancelPTTMessagesMenuItem = new ZMenuItem(Constants.CancelPTTMessage, new EventHandler(CancelPTTMessage_Click));
			fTZMenus.Add(cancelPTTMessagesMenuItem);

			sendPTTArrivalMenuItem = new ZMenuItem(Constants.SendPTTArrival, new EventHandler(PTTArrival_Click));
			fTZMenus.Add(sendPTTArrivalMenuItem);

			sendPTTUnArrivalMenuItem = new ZMenuItem(Constants.SendPTTUnArrival, new EventHandler(SendPTTUnArrival_Click));
			fTZMenus.Add(sendPTTUnArrivalMenuItem);

			var sendDeliveryMenuItem = new ZMenuItem(Constants.SendDeliveryOfGoods, new EventHandler(SendDeliveryOfGoods_Click));
			fTZMenus.Add(sendDeliveryMenuItem);

			fTZMenus.Add(new ZMenuItem("-"));
			sendFTZCargoManifestStatusQueryMenu = new ZMenuItem(Constants.FTZCargoManifestStatusQuery, FTZCargoManifestStatusQuery_Click);
			fTZMenus.Add(sendFTZCargoManifestStatusQueryMenu);

			MenuItems.AddRange(fTZMenus.ToArray());

			#endregion

			dividerMenuBetweenMainMessagingAndAncillaryMessaging = new ZMenuItem("-");
			importCommonMenus.Add(dividerMenuBetweenMainMessagingAndAncillaryMessaging);

			#region FDA

			fDAMenuItem = new ZMenuItem("FDA");
			importCommonMenus.Add(fDAMenuItem);

			copyPreviousFDALineMenu = new ZMenuItem(Constants.CopyPreviousFDALine, new EventHandler(CopyPreviousFDALine_Click));
			fDAMenuItem.MenuItems.Add(copyPreviousFDALineMenu);

			sendFDACorrectionMessagesMenu = new ZMenuItem(Constants.SendFDACorrectionMessages, new EventHandler(SendFDACorrectionMessages_Click));
			fDAMenuItem.MenuItems.Add(sendFDACorrectionMessagesMenu);

			sendStandalonePriorNoticeMenu = new ZMenuItem(Constants.SendStandalonePriorNotice, new EventHandler(SendStandalonePriorNotice_Click));
			fDAMenuItem.MenuItems.Add(sendStandalonePriorNoticeMenu);

			#endregion

			#region Query

			queryMenuItems = new ZMenuItem("Query");
			importCommonMenus.Add(queryMenuItems);

			queryADDCVDMenu = new ZMenuItem(Constants.QueryADDCVD, new EventHandler(QueryADDCVD_Click));
			queryMenuItems.MenuItems.Add(queryADDCVDMenu);

			queryADDCVDByTariffMenu = new ZMenuItem(Constants.QueryADDCVDByTariff, new EventHandler(QueryADDCVDByTariff_Click));
			queryMenuItems.MenuItems.Add(queryADDCVDByTariffMenu);

			cargoManifestStatusQueryMenu = new ZMenuItem(Constants.CargoManifestStatusQuery, new EventHandler(CargoManifestStatusQuery_Click));
			queryMenuItems.MenuItems.Add(cargoManifestStatusQueryMenu);

			sendEntrySummaryQueryMenu = new ZMenuItem(Constants.SendEntrySummaryQuery, new EventHandler(EntrySummaryQuery_Click));
			queryMenuItems.MenuItems.Add(sendEntrySummaryQueryMenu);

			queryQuotaMenu = new ZMenuItem(Constants.SendQuotaQuery, new EventHandler(QueryACEQuota_Click));
			queryMenuItems.MenuItems.Add(queryQuotaMenu);

			censusWarningQueryMessageMenu = new ZMenuItem(Constants.SendCensusWarningQueryMessage, new EventHandler(CensusWarningQueryMessage_Click));
			queryMenuItems.MenuItems.Add(censusWarningQueryMessageMenu);

			#endregion

			#region Other

			otherMenuItems = new ZMenuItem("Other");
			importCommonMenus.Add(otherMenuItems);

			requestToExtendTIBMenu = new ZMenuItem(Constants.RequestToExtendTIB, new EventHandler(RequestToExtendTIB_Click));
			otherMenuItems.MenuItems.Add(requestToExtendTIBMenu);

			requestToClosureTIBMenu = new ZMenuItem(Constants.RequestToClosureTIB, new EventHandler(RequestToClosureTIB_Click));
			otherMenuItems.MenuItems.Add(requestToClosureTIBMenu);

			otherMenuItems.MenuItems.Add(new ZMenuItem("-"));

			sendBillOfLadingUpdateMenu = new ZMenuItem(Constants.SendBillOfLadingUpdate, new EventHandler(BillOfLadingUpdate_Click));
			otherMenuItems.MenuItems.Add(sendBillOfLadingUpdateMenu);

			censusWarningOverrideMessageMenu = new ZMenuItem(Constants.SendCensusWarningOverrideMessage, new EventHandler(CensusWarningOverrideMessage_Click));
			otherMenuItems.MenuItems.Add(censusWarningOverrideMessageMenu);

			sendConsigneeNameAddressAddMenu = new ZMenuItem(Constants.SendConsigneeNameAddressAddMessages, new EventHandler(ConsigneeNameAddressAdd_Click));
			otherMenuItems.MenuItems.Add(sendConsigneeNameAddressAddMenu);

			sendEntryDateUpdateMenu = new ZMenuItem(Constants.SendEntryDateUpdate, new EventHandler(EntryDateUpdate_Click));
			otherMenuItems.MenuItems.Add(sendEntryDateUpdateMenu);

			pGACorrectionMessageMenu = new ZMenuItem(Constants.SendPGACorrection, new EventHandler(SendPGACorrectionMessage_Click));
			otherMenuItems.MenuItems.Add(pGACorrectionMessageMenu);

			statementDeleteAddMessageMenu = new ZMenuItem(Constants.StatementDeleteAddMessage, new EventHandler(StatementDeleteAddMessage_Click));
			otherMenuItems.MenuItems.Add(statementDeleteAddMessageMenu);

			#endregion

			#region BIRD

			bIRDExportMenu = new ZMenuItem("BIRD");
			dataMenuItem.MenuItems.Add(bIRDExportMenu);

			exportToBIRD7501Menu = new ZMenuItem(Constants.ExportDeclarationTo7501, new EventHandler(ExportToBIRD7501File_Click));
			bIRDExportMenu.MenuItems.Add(exportToBIRD7501Menu);

			exportToBIRD3461Menu = new ZMenuItem(Constants.ExportDeclarationTo3461, new EventHandler(ExportToBIRD3461File_Click));
			bIRDExportMenu.MenuItems.Add(exportToBIRD3461Menu);

			exportToBIRDEntryQueryMenu = new ZMenuItem(Constants.ExportQueryBIRD, new EventHandler(SendEntryQueryBIRDFile_Click));
			bIRDExportMenu.MenuItems.Add(exportToBIRDEntryQueryMenu);

			bIRDExportMenu.MenuItems.Add(new ZMenuItem("-"));

			exportToBIRDENMenu = new ZMenuItem(Constants.ExportToEN, new EventHandler(ExportToBIRDEN_Click));
			bIRDExportMenu.MenuItems.Add(exportToBIRDENMenu);

			exportToBIRDCargoReleaseProcessingResultMenu = new ZMenuItem(Constants.ExportCargoReleaseProcessingResultBIRD, new EventHandler(ExportBIRDCargoReleaseProcessingResultFile_Click));
			bIRDExportMenu.MenuItems.Add(exportToBIRDCargoReleaseProcessingResultMenu);

			exportToBIRDEntrySummaryQueryResponseMenu = new ZMenuItem(Constants.ExportEntrySummaryQueryResponse, new EventHandler(ExportBIRDEntrySummaryQueryResponseFile_Click));
			bIRDExportMenu.MenuItems.Add(exportToBIRDEntrySummaryQueryResponseMenu);

			exportToBIRDLiquidationMenu = new ZMenuItem(Constants.ExportLiquidationBIRD, new EventHandler(ExportBIRDLiquidationFile_Click));
			bIRDExportMenu.MenuItems.Add(exportToBIRDLiquidationMenu);

			exportToBIRDDTMenu = new ZMenuItem(Constants.ExportToDT, new EventHandler(ExportToBIRDDT_Click));
			bIRDExportMenu.MenuItems.Add(exportToBIRDDTMenu);

			#endregion

			MenuItems.AddRange(importCommonMenus.ToArray());

			MenuItems.Add(new ZMenuItem("-"));

			var refreshExRates = new ZMenuItem(Constants.RefreshExRates, new EventHandler(RefreshExRates_Click));
			MenuItems.Add(refreshExRates);

			var refreshTariffDetails = new ZMenuItem(Constants.RefreshTariffDetails, new EventHandler(RefreshTariffDetails_Click));
			MenuItems.Add(refreshTariffDetails);

			refreshNotificationDispositionActions = new ZMenuItem(Constants.RefreshNotificationDispositionActions, new EventHandler(RefreshNotificationDispositionActions_Click));
			MenuItems.Add(refreshNotificationDispositionActions);

			requestTariffUpdateMenu = new ZMenuItem(Constants.RequestTariffUpdate, new EventHandler(RequestTariffUpdates_Click));
			MenuItems.Add(requestTariffUpdateMenu);

			resetDutyCalculationDateMenuItem = new ZMenuItem(Constants.ResetDutyCalculationDate, new EventHandler(ResetDutyCalculationDate_Click));
			MenuItems.Add(resetDutyCalculationDateMenuItem);

			showDiscardedAndInactiveMessagesMenu = new ZMenuItem(Constants.ShowDiscardedAndInactiveMessages, new EventHandler(ShowDiscardedAndInactiveMessages_Clicked));
			int indexOfPerformApportionment = MenuItems.IndexOf(apportionmentMenuItem);
			MenuItems.Add(indexOfPerformApportionment + 1, showDiscardedAndInactiveMessagesMenu);

			importInvoicesMenuItem = new ZMenuItem(Constants.ImportBulkDeclarations, new EventHandler(ImportInvoicesMenuItem_Click));
			MenuItems.Add(indexOfPerformApportionment + 2, importInvoicesMenuItem);

			#region Drawback

			drawbackBulkImportEntryLinesMenuItem = new ZMenuItem(Constants.DrawbackBulkImportEntryLines, new EventHandler(BulkImportMenuItem_Click));
			dataMenuItem.MenuItems.Add(drawbackBulkImportEntryLinesMenuItem);

			#endregion
		}

		void SendPGACorrectionMessage_Click(object sender, EventArgs e)
		{
			if (CheckHasChanges())
			{
				var entry = Declaration.GetEntryHeaderWithPGADetail();
				if (entry != null)
				{
					GeneratePGACorrection(entry);
				}
				else
				{
					entry = Declaration.GetEntryHeaderForPGACorrection();
					if (entry != null)
					{
						GeneratePGACorrection(entry);
					}
					else
					{
						Globals.Message.ShowInformation(ResString.GetMultilingualString("824AC434-111B-4AF6-A158-C6E03FB1E40E", "There is no Entry available for sending PGA Correction message to Customs and no new PGA lines available for sending."), ResString.GetMultilingualString("B4A66707-5E0B-4124-B835-2C70F85B608F", "No Entry"));
					}
				}
			}
		}

		void GeneratePGACorrection(CusEntryHeader entry)
		{
			var entryLineWithPGA = entry.GetEntryLinesWithPGAToSend();
			if (entryLineWithPGA.Any())
			{
				var idHasChanged = entry.PGADataCorrections.Any(x => x.HasTrackingIDChanged());
				if (!idHasChanged || Globals.Message.Show("PGA Line ID, i.e. entry line numbers and/or tariffs have changed and as such, you should send an ACE Cargo release replacement instead of a PGA Correction message to update PGA details. Are you sure you wish to continue?", "Continue?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
				{
					var sendingAction = new PGACorrectionMessageSendingAction(entry);
					if (ShowPGACorrectionSendingForm(sendingAction))
					{
						try
						{
							sendingAction.GeneratePGACorrectionMessages();
							if (MainForm.FireSaveButton() == ContinueWithSave.Yes)
							{
								Globals.Message.ShowInformation(Res.GetString("5E9F8959-1DEC-4C12-8E0A-9CF245E78283", "PGA Correction Message Sent."));
							}
						}
						catch (ZSaveException ex)
						{
							ZExceptionReporting.HandleSaveException(ex);
						}
					}
				}
			}
			else
			{
				Globals.Message.ShowInformation(ResString.GetMultilingualString("0541F5C6-ABFB-4871-A00A-DE2F345BC9FA", "There are no PGA lines available for sending PGA Correction. No messages generated."), ResString.GetMultilingualString("BF283DBB-B684-4A87-AAB2-BB04BC8A0317", "No PGA Lines"));
			}
		}

#if DEBUG
		protected virtual
#endif
		bool ShowPGACorrectionSendingForm(PGACorrectionMessageSendingAction sendingAction)
		{
			ZFormModaliser.ShowDialogAndDispose(new PGACorrectionSendingForm(sendingAction));
			return sendingAction.ShouldSendMessage;
		}

		void BulkImportMenuItem_Click(object sender, EventArgs e)
		{
			var module = (ZFilterModule)ZModuleFactory.Instance.Create(ModuleIDs.Customs.US.EntryLine, Core.Constants.CountryCodes.UnitedStates);
			var popup = new EmbeddedModulePopup(module);
			var strategy = new DrawbackBulkImportPopupOKButtonStrategy(Declaration);
			var claimant = Declaration.Importer;
			if (claimant != null)
			{
				var importerFilter = (ModuleGuidFilter)module.FilterBusinessObject.AlwaysVisibleModuleFilters.FirstOrDefault(x => x.Description == "Importer");
				if (importerFilter != null)
				{
					importerFilter.DefaultProperty = claimant.PK;
				}
			}
			strategy.OKButtonClick += (o, ev) => popup.Close();
			popup.EmbeddedModulePopupOKButtonStrategy = strategy;
			module.OverrideModuleDecisionProvider(strategy.ModuleDecisionProvider);
			ZFormModaliser.Show(popup, this.MainForm);
		}

		protected override void AddAuditMenuItems()
		{
			base.AddAuditMenuItems();
			AddStatusTibWriteToLogMenuItem(MenuItems, 3);

			moreAuditsMenu = new ZMenuItem("More Audits");
			MenuItems.Add(moreAuditsMenu);
			AddCW1SuportMenuItem();

			AddAuditSpiWriteToLogMenuItem(moreAuditsMenu.MenuItems);
			AddAuditFdaWriteToLogMenuItem(moreAuditsMenu.MenuItems);
			AddAuditCensusWarningWriteToLogMenuItem(moreAuditsMenu.MenuItems);
		}

		void AddStatusTibWriteToLogMenuItem(MenuItemCollection menuItems, int index)
		{
			var loggerOptions = new BusinessObjectLoggerOptions(
				AuditFieldsList.Codes.TIB, GetLastLogFieldPrefix(USAddInfoSchema.US_StatusTibLastLogDate), Enterprise.ZArchitecture.Business.Events.StatusUpdated);

			var tIBAuditMenuItem = new WriteToLogMenuItem(
				(IStmALogParent)Declaration.Shipment ?? Declaration,
				() => Declaration,
				Env.Security.CustomsDeclarationAudit,
				"Close " + AuditFieldsList.Codes.TIB,
				loggerOptions);

			tIBAuditMenuItem.Visible = Declaration.IsTemporaryImportationBond;
			menuItems.Add(index, tIBAuditMenuItem);
		}

		void AddAuditSpiWriteToLogMenuItem(MenuItemCollection menuItems)
		{
			var loggerOptions = new BusinessObjectLoggerOptions(
				AuditFieldsList.Codes.SPI, GetLastLogFieldPrefix(USAddInfoSchema.US_AuditSpiLastLogDate), Enterprise.ZArchitecture.Business.Events.RecordAudited);

			menuItems.Add(
				new WriteToLogMenuItem(
					(IStmALogParent)Declaration.Shipment ?? Declaration,
					() => Declaration,
					Env.Security.CustomsDeclarationAudit,
					"Audit " + AuditFieldsList.Codes.SPI,
					loggerOptions));
		}

		void AddAuditFdaWriteToLogMenuItem(MenuItemCollection menuItems)
		{
			var loggerOptions = new BusinessObjectLoggerOptions(
				AuditFieldsList.Codes.FDA, GetLastLogFieldPrefix(USAddInfoSchema.US_AuditFdaLastLogDate), Enterprise.ZArchitecture.Business.Events.RecordAudited);

			menuItems.Add(
				new WriteToLogMenuItem(
					(IStmALogParent)Declaration.Shipment ?? Declaration,
					() => Declaration,
					Env.Security.CustomsDeclarationAudit,
					"Audit " + AuditFieldsList.Codes.FDA,
					loggerOptions));
		}

		void AddAuditCensusWarningWriteToLogMenuItem(MenuItemCollection menuItems)
		{
			var cwoMenuItem = new WriteToLogMenuItem(
				(IStmALogParent)Declaration.Shipment ?? Declaration,
				delegate
				{ return Declaration; },
				Env.Security.CustomsDeclarationAudit,
				"Audit " + AuditFieldsList.Descriptions.CensusWarning,
				new BusinessObjectLoggerOptions(AuditFieldsList.Codes.CensusWarning, false, Enterprise.ZArchitecture.Business.Events.RecordAudited));

			var status = (Declaration != null && Declaration.ActiveEntryHeaders.EntrySummaryEntry != null) ?
				Declaration.ActiveEntryHeaders.EntrySummaryEntry.CH_Status :
				ZString.Empty;

			cwoMenuItem.Visible = (
				status == ImportMessageStatusList.Codes.EntrySummaryOriginalAcceptedWithCensusWarnings ||
				status == ImportMessageStatusList.Codes.EntrySummaryReplaceAcceptedWithCensusWarnings);

			menuItems.Add(cwoMenuItem);
		}

		void AddCW1SuportMenuItem()
		{
			resetToOriginalMenu = new ZMenuItem(Constants.ResetToOriginal, new EventHandler(ResetToOriginal_Click));
			MenuItems.Add(resetToOriginalMenu);
		}

		string GetLastLogFieldPrefix(CargoWise.Schema.SchemaDateTimeColumn lastLogDateColumn)
		{
			return lastLogDateColumn.Name.Replace(BusinessObjectLogger.LastLogDatePropertySuffix, "");
		}

		public override void RefreshMenu()
		{
			base.RefreshMenu();

			bool isImport = false;
			bool isExport = false;
			bool isMiscellaneous = false;
			bool isDrawback = false;
			bool is7552 = false;
			bool isImportByExternalBroker = false;
			bool isStandAlonePriorNotice = false;
			var isFTZ = false;
			var isACEDrawback = false;
			var isACE = false;
			var isITF = false;

			if (Declaration != null)
			{
				isImport = Declaration.IsImport;
				isExport = Declaration.IsExport;
				isMiscellaneous = Declaration.IsMiscellaneous;
				isDrawback = Declaration.IsDrawback;
				is7552 = Declaration.Is7552;
				isImportByExternalBroker = Declaration.IsImportByExternalBroker;
				isACEDrawback = Declaration.IsACEDrawback;
				isACE = Declaration.IsACE;
				isStandAlonePriorNotice = isImport && !isImportByExternalBroker && Declaration.US_EnableSPN;
				isFTZ = Declaration.IsFTZAdmission;
				isITF = Declaration.IsInterface;

				sendFDACorrectionMessagesMenu.Visible = !(isMiscellaneous || isImportByExternalBroker || isExport || Declaration.IsACECargoCertificationMode);
				otherMenuItems.Visible = !(isMiscellaneous || isImportByExternalBroker) && !isITF;
				sendBillOfLadingUpdateMenu.Visible = isImport && !Declaration.IsACECargoCertificationMode;
				bIRDExportMenu.Visible = isImport && !isACE && !isFTZ;
				importInvoicesMenuItem.Visible = Declaration.US_ConsolACE;
			}

			requestTariffUpdateMenu.Visible = !isExport;
			aESMessagingMenuItem.Visible = isExport && !isITF;
			drawbackSummaryMenuItem.Visible = isDrawback && (!is7552 || isACEDrawback);
			drawbackSummaryReplaceMenuItem.Visible = isACEDrawback;
			drawbackSummaryDeleteMenuItem.Visible = isDrawback && !is7552 && !isACEDrawback;
			drawbackBulkImportEntryLinesMenuItem.Visible = isDrawback;
			var isImportCommonVisible = isImport && !isImportByExternalBroker && !isFTZ && !isMiscellaneous && !isITF;
			importCommonMenus.SetAllVisible(isImportCommonVisible);
			var sendEntrySummaryMenus = sendEntrySummaryMenu.MenuItems.OfType<MenuItem>().ToArray();
			sendEntrySummaryMenus.SetAllVisible(isImportCommonVisible);
			var sendCargoReleaseMenus = sendCargoReleaseMenu.MenuItems.OfType<MenuItem>().ToArray();
			sendCargoReleaseMenus.SetAllVisible(isImportCommonVisible);
			resetDutyCalculationDateMenuItem.Visible = isImportCommonVisible && isACE && Declaration.ActiveEntryHeaders.EntrySummaryEntry != null;
			if (isImportCommonVisible)
			{
				if (isACE)
				{
					sendOriginalMenu.Visible = false;
					sendAmendmentMenu.Visible = false;
					sendWithdrawalMenu.Visible = false;
					var enableENS = Declaration.US_EnableENS;
					sendEntrySummaryMenu.Visible = enableENS;
					sendEntrySummaryMenus.SetAllVisible(enableENS);
					var shouldCargoReleaseBeVisible = Declaration.US_EnableCRL || Declaration.IsACECargoCertificationMode;
					sendCargoReleaseMenu.Visible = shouldCargoReleaseBeVisible;
					sendCargoReleaseMenus.SetAllVisible(shouldCargoReleaseBeVisible);
					resetDutyCalculationDateMenuItem.Visible = (Declaration.ActiveEntryHeaders.EntrySummaryEntry?.US_DutyCalcDate ?? ZDateTime.Empty).IsValid;
				}
				else
				{
					sendEntrySummaryMenu.Visible = false;
					sendEntrySummaryMenus.SetAllVisible(false);
					sendCargoReleaseMenu.Visible = false;
					sendCargoReleaseMenus.SetAllVisible(false);
				}
			}
			dividerMenuBetweenMainMessagingAndAncillaryMessaging.Visible = (isImport || isFTZ) && !isITF;
			queryMenuItems.Visible = (isImport || isFTZ) && !isITF;
			cargoManifestStatusQueryMenu.Visible = isImport && !isFTZ;
			sendEntrySummaryQueryMenu.Visible = isImport && !isFTZ;
			queryQuotaMenu.Visible = isImport && !isFTZ;
			censusWarningQueryMessageMenu.Visible = isImport && !isFTZ;

			sendEntrySummaryQueryMenu.Visible = isImport && !isImportByExternalBroker && !isFTZ;

			fTZMenus.SetAllVisible(isFTZ && !isITF);

			sendConsigneeNameAddressAddMenu.Visible = isImport && Declaration.US_EnableCRL && !isACE && !isFTZ;

			showDiscardedAndInactiveMessagesMenu.Visible = isImport && !isImportByExternalBroker && Declaration.HasInactiveMessages;
			censusWarningOverrideMessageMenu.Visible = isImport && isACE;
			censusWarningQueryMessageMenu.Visible = isImport && isACE;
			pGACorrectionMessageMenu.Visible = isImport && Declaration.IsACECargoCertificationMode;

			if (moreAuditsMenu != null)
			{
				moreAuditsMenu.Visible = isImport;
			}

			if (resetToOriginalMenu != null)
			{
				resetToOriginalMenu.Visible = (Env.CurrentUser.IsSupportUser && isImport);
			}

			if (isFTZ)
			{
				sendFDACorrectionMessagesMenu.Visible = false;
				copyPreviousFDALineMenu.Visible = !isITF;
				sendPTTArrivalMenuItem.Visible = sendPTTUnArrivalMenuItem.Visible = ZZCustomsFunctionality.IsAMSHBREffective && (Declaration.IsSea || Declaration.IsRail) && !isITF;
			}
			else if (isImport)
			{
				copyPreviousPGALineMenu.Visible = isACE || isITF;
				copyPreviousPGALineMenuDivider.Visible = isACE || isITF;

				copyPreviousFDALineMenu.Visible = !isACE && !isITF;
			}
			sendStandalonePriorNoticeMenu.Visible = isStandAlonePriorNotice;

			fDAMenuItem.Visible = isImport && (copyPreviousFDALineMenu.Visible || sendFDACorrectionMessagesMenu.Visible || sendStandalonePriorNoticeMenu.Visible) && !isITF;
			refreshNotificationDispositionActions.Visible = isImport || isDrawback;
		}

		protected override bool DisplayGenerateEntriesMenuOption
		{
			get { return !(Declaration != null && Declaration.IsDrawback); }
		}

		protected override string GenerateEntriesMenuOptionText
		{
			get { return "MERGE (Generate Entries)"; }
		}

		protected override void SynchronizeWithOrdersMenuItem_Click(object sender, EventArgs e)
		{
			if (Declaration != null)
			{
				var permits = Declaration.FindRelatedPermits();
				if (permits.Any(p => p.HasPendingTransaction()))
				{
					Globals.Message.Show(
						ResString.GetMultilingualString("c8f65a4f-31d5-4d65-a9b0-bde419aa67ca", "Cannot synchronize. Please check that all orders are finalized."),
						ResString.GetMultilingualString("a6ba6544-d0c7-42ed-b088-872a5ceb5521", "Synchronize With Orders"),
						MessageBoxButtons.OK,
						MessageBoxIcon.Error
					);
					return;
				}
			}

			base.SynchronizeWithOrdersMenuItem_Click(sender, e);
		}

		#endregion
		internal MenuItem BondedWarehouseMenuItemInternal => bondedWarehouseMenuItem;

		internal MenuItem SynchronizeWithOrdersMenuItemInternal => synchronizeWithOrdersMenuItem;
	}
}
