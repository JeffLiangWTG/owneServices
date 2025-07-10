using System;
using System.Globalization;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.Business;
using Enterprise.Customs.GUI;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.DataMapping;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.DataMapping;
using Enterprise.ZArchitecture.Modules;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.GUI
{
	public partial class ReconDeclarationForm : ZTemplateForm
	{
		public ReconDeclarationForm(ReconDeclaration reconDeclaration)
			: base(reconDeclaration)
		{
			this.reconDeclaration = reconDeclaration;
			AddMenuItems();

			PlugIns.Add(ControllerIDs.DocDataPlugIn);
			PlugIns.AddJobInvoicing(reconDeclaration.InvoicingSupporter);
			WorkflowTabPage.Initialize(reconDeclaration);

			StatementPaymentDateDateEdit.ReadOnly = true;

			reconDeclaration.US_IsAggregateInfo.ValueChanged += US_IsAggregateInfo_ValueChanged;
			reconDeclaration.JE_ApplicationCodeInfo.ValueChanged += JE_ApplicationCodeInfo_ValueChanged;
			reconDeclaration.US_IssueCodeInfo.ValueChanged += US_IssueCodeInfo_ValueChanged;
			ChangeControlsVisibilityForAggregateRecon();
			ChangeControlsVisibilityForApplicationCode();
			ChangeCalculateCustomValuesMenuItemVisible();

			OriginalEntriesGrid.SetAvailability(ZZCustomsFunctionality.USFTAReconIndIsValid, AddInfo.Schema.US_NAFTAReconIndicator);
			OriginalEntriesGrid.RefreshTableStyles();

			StatusesErrorsLabel.AllowOverlap(messagesStatusErrorsUserControl);
		}
		readonly ReconDeclaration reconDeclaration;

		void AddMenuItems()
		{
			BrokerageMenuItem = new ZMenuItem("Brokerage");
			this.Menu.MenuItems.Add(Menu.MenuItems.IndexOfKey(ZFormMenuStrategy.ActionsMenuItemName) + 1, BrokerageMenuItem);

			CalculateDutyFeeMenuItem = new ZMenuItem(CalculateDutyFee, new EventHandler(CalculateDuty_Click));
			BrokerageMenuItem.MenuItems.Add(CalculateDutyFeeMenuItem);

			ImportInvoicesMenuItem = new ZMenuItem(ImportInvoices, new EventHandler(ImportLinesDetails_Click));
			BrokerageMenuItem.MenuItems.Add(ImportInvoicesMenuItem);

			CalculateCustomValuesMenuItem = new ZMenuItem(CalculateCustomValues, new EventHandler(CalculateCustomsValues_Click));
			BrokerageMenuItem.MenuItems.Add(CalculateCustomValuesMenuItem);

			BrokerageMenuItem.MenuItems.Add("-");

			ImportBulkDeclarationsMenuItem = new ZMenuItem(ImportBulkDeclarations, new EventHandler(ImportBulkDeclarations_Click));
			BrokerageMenuItem.MenuItems.Add(ImportBulkDeclarationsMenuItem);

			#region Data Transfer Menu

			MenuItem dataTransferMenuItem = BrokerageMenuItem.MenuItems.Add("DataTransfer");

			ExportEntriesReconDataMenuItem = new ZMenuItem(ExportEntriesReconData, new EventHandler(ExportEntriesReconData_Click));
			dataTransferMenuItem.MenuItems.Add(ExportEntriesReconDataMenuItem);

			ExportInvoiceLinesReconDataMenuItem = new ZMenuItem(ExportInvoiceLinesReconData, new EventHandler(ExportInvoiceLinesReconData_Click));
			dataTransferMenuItem.MenuItems.Add(ExportInvoiceLinesReconDataMenuItem);

			dataTransferMenuItem.MenuItems.Add("-");

			UpdateEntriesReconDataMenuItem = new ZMenuItem(UpdateEntriesReconData, new EventHandler(UpdateEntriesReconData_Click));
			dataTransferMenuItem.MenuItems.Add(UpdateEntriesReconDataMenuItem);

			UpdateInvoiceLinesReconDataMenuItem = new ZMenuItem(UpdateInvoiceLinesReconData, new EventHandler(UpdateInvoiceLinesReconData_Click));
			dataTransferMenuItem.MenuItems.Add(UpdateInvoiceLinesReconDataMenuItem);

			#endregion

			BrokerageMenuItem.MenuItems.Add("-");

			SendAddMessageMenuItem = new ZMenuItem(SendAddMessage, new EventHandler(SendAddMessage_Click));
			SendReplaceMessageMenuItem = new ZMenuItem(SendReplaceMessage, new EventHandler(SendReplaceMessage_Click));
			SendDeleteMessageMenuItem = new ZMenuItem(SendDeleteMessage, new EventHandler(SendDeleteMessage_Click));

			BrokerageMenuItem.MenuItems.Add(SendAddMessageMenuItem);
			BrokerageMenuItem.MenuItems.Add(SendReplaceMessageMenuItem);
			BrokerageMenuItem.MenuItems.Add(SendDeleteMessageMenuItem);

			BrokerageMenuItem.MenuItems.Add(new ZMenuItem("-"));

			StatementDeleteAddMessageMenuItem = new ZMenuItem(StatementDeleteAddMessage, new EventHandler(StatementDeleteAddMessage_Click));
			BrokerageMenuItem.MenuItems.Add(StatementDeleteAddMessageMenuItem);

			BrokerageMenuItem.MenuItems.Add(new ZMenuItem("-"));

			QueryEntrySummaryMenuItem = new ZMenuItem(QueryEntrySummary, new EventHandler(QueryEntrySummary_Click));
			BrokerageMenuItem.MenuItems.Add(QueryEntrySummaryMenuItem);

			RefreshTariffDetailsMenuItem = new ZMenuItem(RefreshTariffDetails, new EventHandler(RefreshTariffDetails_Click));
			BrokerageMenuItem.MenuItems.Add(RefreshTariffDetailsMenuItem);

			RefreshNotificationDispositionActionsMenuItem = new ZMenuItem(RefreshNotificationDispositionActions, new EventHandler(RefreshNotificationDispositionActions_Click));
			BrokerageMenuItem.MenuItems.Add(RefreshNotificationDispositionActionsMenuItem);

			RequestTariffUpdateMenuItem = new ZMenuItem(RequestTariffUpdate, new EventHandler(RequestTariffUpdate_Click));
			BrokerageMenuItem.MenuItems.Add(RequestTariffUpdateMenuItem);
		}

		#region Menu Items

		public MenuItem BrokerageMenuItem;

		public MenuItem CalculateDutyFeeMenuItem;
		public const string CalculateDutyFee = "Calculate Duty && Fees";

		public MenuItem ImportInvoicesMenuItem;
		public const string ImportInvoices = "Retrieve Invoices for Original Entries";

		public MenuItem CalculateCustomValuesMenuItem;
		public const string CalculateCustomValues = "Calculate Recon Customs Values";

		public MenuItem SendAddMessageMenuItem;
		public const string SendAddMessage = "Send Add Message";

		public MenuItem SendReplaceMessageMenuItem;
		public const string SendReplaceMessage = "Send Replace Message";

		public MenuItem SendDeleteMessageMenuItem;
		public const string SendDeleteMessage = "Send Delete Message";

		public MenuItem ImportBulkDeclarationsMenuItem;
		public const string ImportBulkDeclarations = "Import Declarations (Bulk)";

		public MenuItem RefreshTariffDetailsMenuItem;
		public const string RefreshTariffDetails = "Refresh Tariff Details";

		public MenuItem RefreshNotificationDispositionActionsMenuItem;
		public const string RefreshNotificationDispositionActions = "Refresh Notification Disposition Actions";

		public MenuItem RequestTariffUpdateMenuItem;
		public const string RequestTariffUpdate = "Request Tariff Update for entered tariffs";

		public MenuItem UpdateInvoiceLinesReconDataMenuItem;
		public const string UpdateInvoiceLinesReconData = "Import Entry Lines From File";

		public MenuItem ExportInvoiceLinesReconDataMenuItem;
		public const string ExportInvoiceLinesReconData = "Export Entry Lines To File";

		public MenuItem UpdateEntriesReconDataMenuItem;
		public const string UpdateEntriesReconData = "Import Entries From File";

		public MenuItem ExportEntriesReconDataMenuItem;
		public const string ExportEntriesReconData = "Export Entries To File";

		public MenuItem StatementDeleteAddMessageMenuItem;
		public const string StatementDeleteAddMessage = "Send Statement Delete/Add Message";

		public MenuItem QueryEntrySummaryMenuItem;
		public const string QueryEntrySummary = "Query Entry Summary";

		#endregion

		#region Implementation

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);

			if (StatusesErrorsLabel.DataBindings[StatusLableVisibilityForBinding] == null && dataSource != null)
			{
				StatusesErrorsLabel.DataBindings.Add(new KBinding(StatusLableVisibilityForBinding, BindingSource.DataSource, "Messages.StatusesAndErrorsVisible", true, DataSourceUpdateMode.Never));
			}
		}
		const string StatusLableVisibilityForBinding = "IsVisibleForBinding";

		void StatusesErrorsLabel_VisibleChanged(object sender, EventArgs e)
		{
			if (this.StatusesErrorsLabel.Visible || (this.MessagesGrid.ListManager != null && this.MessagesGrid.ListManager.Count <= 0))
			{
				this.StatusesErrorsLabel.BringToFront();
			}
		}

		public new ReconDeclaration BusinessEntity
		{
			get { return reconDeclaration; }
		}

		public override string FormCaption
		{
			get
			{
				string result = "Recon Declaration";

				if (!this.IsDesignMode() && !BusinessEntity.JE_DeclarationReference.IsEmpty)
				{
					result += " - " + BusinessEntity.JE_DeclarationReference;
				}

				return result;
			}
		}

		protected override ContinueWithSave ShowPreSaveDialogs()
		{
			ContinueWithSave result = base.ShowPreSaveDialogs();

			if (result == ContinueWithSave.Yes)
			{
				//If there is an original entry without any invoices attached, but belongs to the current broker and not aggregate recon with no duty, fees input
				if (BusinessEntity.OriginalEntries.HasImportableEntriesWithoutAnyLinesAttached && !BusinessEntity.US_R_IsNoChangeAgg)
				{
					if (Globals.Message.Show("There are original entries that do not have any lines attached. Do you want system to import lines for such original entries?", "Import Lines", MessageBoxButtons.YesNo, MessageBoxIcon.Question, DialogResult.Yes) == DialogResult.Yes)
					{
						new ReconImportEntryRetriever(BusinessEntity).ImportLines();
					}
				}

				if (BusinessEntity.ApportionmentDirty)
				{
					using (ApportionmentProgressForm progressForm = new ApportionmentProgressForm(BusinessEntity))
					{
						ZFormModaliser.Show(progressForm, this);
						BusinessEntity.ResumeApportionment();
						progressForm.Close();
					}
				}

				BusinessEntity.CalculateDutyFeesForChangedEntries();
			}

			return result;
		}

		public override bool IsResizableByTabPageAllowed => true;

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();

			MessagesTabPage.RunWhenBindingOrFirstShown(delegate
			{
				MessageDetailsTabPage.RunWhenBindingOrFirstShown(delegate
				{
					MessageDetailsTextBox.Font = new System.Drawing.Font("Courier New", 8F);
				});
			});
		}

		public ZTemplateTabControl MainTabControlExposed
		{
			get { return this.MainTabControl; }
		}

		#endregion

		#region Menu Event Handlers

		void ImportBulkDeclarations_Click(object sender, EventArgs e)
		{
			ZFilterModule module = (ZFilterModule)ZModuleFactory.Instance.Create(ModuleIDs.Customs.JobDeclaration, Core.Constants.CountryCodes.UnitedStates);
			module.FilterBusinessObject.SetExternalDefaults(GetFilterDefaults());

			var popup = new CustomsEmbeddedModulePopup(module);
			var strategy = new ReconBulkImportPopupOKButtonStrategy(popup, BusinessEntity);
			popup.EmbeddedModulePopupOKButtonStrategy = strategy;
			module.OverrideModuleDecisionProvider(strategy.ModuleDecisionProvider);
			ZFormModaliser.Show(popup, this);
		}

		FilterBusinessObjectDefaults GetFilterDefaults()
		{
			FilterBusinessObjectDefaults result = new FilterBusinessObjectDefaults();
			result.Add(new FilterBusinessObjectDefault("Shipment Type", "Property", new ZString(Enterprise.Customs.US.Business.JobMessageTypeList.Codes.Import)));
			if (BusinessEntity.IOROrgPK.IsValid)
			{
				result.Add(new FilterBusinessObjectDefault("Importer of Record", "Property", BusinessEntity.IOROrgPK));
			}

			if (BusinessEntity.US_IssueCode == ReconIssueCodeList.Codes.FTA)
			{
				result.Add(new FilterBusinessObjectDefault("FTA Recon", "Property0", ZBool.True));
			}
			else
			{
				result.Add(new FilterBusinessObjectDefault("Recon Issue", "Property", BusinessEntity.US_IssueCode));
			}

			result.Add(new FilterBusinessObjectDefault("Exclude IOR Filing Their Own Recon", "Property0", ZBool.True));
			result.Add(new FilterBusinessObjectDefault("Surety Code", "Property", BusinessEntity.US_SuretyCode));
			result.Add(new FilterBusinessObjectDefault("Not Yet Reconciled", "Property", BusinessEntity.US_IssueCode));
			return result;
		}

		void ImportLinesDetails_Click(object sernder, EventArgs e)
		{
			int countOfInvoices = BusinessEntity.InvoiceLines.Count;

			new ReconImportEntryRetriever(BusinessEntity).ImportLines();

			Globals.Message.Show(BusinessEntity.InvoiceLines.Count - countOfInvoices + " line(s) have been imported.", "Importing Lines", MessageBoxButtons.OK, MessageBoxIcon.Information);
		}

		void CalculateDuty_Click(object sender, EventArgs e)
		{
			if (BusinessEntity.US_R_IsNoChangeAgg)
			{
				Globals.Message.Show(NoCalculationForAggregate, "Duty & Fee Calculation", MessageBoxButtons.OK, MessageBoxIcon.Information);
				return;
			}

			BusinessEntity.CalculateDutyFeesForAllEntries();
			Globals.Message.Show("Calculation finished. Please check 'Recon Charges & Fees' for each original entry.", "Duty & Fee Calculation", MessageBoxButtons.OK, MessageBoxIcon.Information);
		}
		internal const string NoCalculationForAggregate = "You have indicated that this is a 'No-Change' aggregate recon. No duty & fees calculation will occur.";

		void CalculateCustomsValues_Click(object sender, EventArgs e)
		{
			if (CheckHasChangesAndGetConfirmationFromUsers())
			{
				using (var form = new ReconCalculateCustomsValueForm(BusinessEntity.ReconCustomsValueCalculationManager))
				{
					ZFormModaliser.ShowDialogWithoutDispose(form);
				}
			}
		}

		void SendAddMessage_Click(object sender, EventArgs e)
		{
			SendMessage(UpdateActionCode.Add);
		}

		void SendReplaceMessage_Click(object sender, EventArgs e)
		{
			SendMessage(UpdateActionCode.Replace);
		}

		void SendDeleteMessage_Click(object sender, EventArgs e)
		{
			SendMessage(UpdateActionCode.Delete);
		}

		void SendMessage(UpdateActionCode actionCode)
		{
			using (ZFormPostingButtonsStrategy.DeferredUpdateSaveButtonsBasedOnHasChanges(this))
			{
				if (!Env.Security.USReconMessaging.IsAllowed)
				{
					Env.Security.USReconMessaging.ShowError();
				}
				else
				{
					var declaration = BusinessEntity.ReconWrappedJobDeclaration;
					declaration.MessageInitiator = new SendsMessagesToCustomsGUI();

					var messageSender = new ReconMessageSender(BusinessEntity, actionCode);
					messageSender.OnPrepare += (action =>
					{
						var isOkToContinue = true;
						if (action != null)
						{
							try
							{
								if (declaration.LockSendCustomsMessageMutex)
								{
									isOkToContinue = ZFormModaliser.ShowDialogAndDispose(new ReconMessageSendingForm(action)) == DialogResult.OK;
								}
								else
								{
									isOkToContinue = false;
									Globals.Message.ShowInformation(Res.GetString("ccdf35a0-40c0-4cfc-a662-be09c86a21a7", "{0} is in the process of sending reconciliation message.\r\nPlease wait until the sending process is finished before trying again.\r\nPlease ensure that you re-open the form to pickup the latest changes.", declaration.GetSendCustomsMessageMutexInfo()));
								}
							}
							finally
							{
								declaration.UnlockSendCustomsMessageMutex();
							}
						}
						return isOkToContinue;
					});
					messageSender.OnAllowNotifications += new Customs.Business.MessageSender.AllowNotificationsEventHandler(ContinueWithNotifications);
					messageSender.OnSave += new Customs.Business.MessageSender.SaveEventHandler(() => FireSaveButton());
					messageSender.SendMessage();
				}
			}
		}

		bool ContinueWithNotifications(MessageSendingNotificationCollection notifications)
		{
			return StatementMessagesHandler.ContinueWithNotifications(notifications);
		}

		bool CheckHasChangesAndGetConfirmationFromUsers()
		{
			bool result = false;

			if (BusinessEntity.HasChanges)
			{
				if (Globals.Message.Show("Changes have not been saved. Do you want system to save and proceed?", "Save", MessageBoxButtons.YesNo, MessageBoxIcon.Question, DialogResult.Yes) == DialogResult.Yes)
				{
					result = FireSaveButton() == ContinueWithSave.Yes;
				}
			}
			else
			{
				result = true;
			}

			return result;
		}

		#region RefreshTariffDetails

		void RefreshTariffDetails_Click(object sender, EventArgs e)
		{
			reconDeclaration.RefreshTariff();
			Globals.Message.Show("Refreshing Tariffs complete.");
		}

		#endregion

		void RefreshNotificationDispositionActions_Click(object sender, EventArgs e)
		{
			reconDeclaration.ReconWrappedJobDeclaration.ReCalculateENSAction();
			Globals.Message.ShowInformation("Declaration's notification disposition actions have been updated.");
		}

		void RequestTariffUpdate_Click(object sender, EventArgs e)
		{
			//To stop sending two requests on the same tariff number
			foreach (JobComInvoiceLine invoiceLine in BusinessEntity.InvoiceLines)
			{
				invoiceLine.TariffMarkedForReferenceFileRequest = false;
				invoiceLine.ReconOrigTariffMarkedForReferenceFileRequest = false;
			}

			if (CheckHasChangesAndGetConfirmationFromUsers())
			{
				new ReferenceFileRequester().RequestTariffs(BusinessEntity, true);
				Globals.Message.ShowInformation(TariffUpdateRequestConfirmation);
			}
		}

		#region Query Entry Summary

		void QueryEntrySummary_Click(object sender, EventArgs e)
		{
			if (CheckHasChangesAndGetConfirmationFromUsers())
			{
				if (!CheckEntrySummaryQueryCannotBeSent())
				{
					new ACEEntrySummaryQueryMessageBuilder(BusinessEntity.ReconEntry.GetEntry()).PopulateMessage();

					try
					{
						if (FireSaveButton() == ContinueWithSave.Yes)
						{
							Globals.Message.ShowInformation(string.Format(CultureInfo.CurrentCulture, "{0} Query Message Sent", "Recon entry"));
						}
					}
					catch (ZSaveException ex)
					{
						ZExceptionReporting.HandleSaveException(ex);
					}
				}
			}
		}

		bool CheckEntrySummaryQueryCannotBeSent()
		{
			var entry = BusinessEntity.ReconEntry.GetEntry();
			var result = entry == null || entry.EntryNumber.IsEmpty;
			if (result)
			{
				Globals.Message.ShowInformation("Recon entry does not exists for this job.");
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

		#endregion

		#region Statement Delete/Add

		void StatementDeleteAddMessage_Click(object sender, EventArgs e)
		{
			StatementDeleteAndSendingActionCollection messageSendingActions = GetActionCollection(BusinessEntity);

			MessageSendingNotificationCollection notifications = new StatementMessageSendingValidator().GetNotificationsForStatementDeleteAdd(BusinessEntity);

			StatementMessagesHandler.SendDeleteAddMessage(messageSendingActions, notifications, BusinessEntity.Factory);
		}

		StatementDeleteAndSendingActionCollection GetActionCollection(ReconDeclaration reconDeclaration)
		{
			return new StatementDeleteAndSendingActionCollection(reconDeclaration);
		}

		StatementMessagesHandler StatementMessagesHandler
		{
			get { return statementMessagesHandler ?? (statementMessagesHandler = new StatementMessagesHandler()); }
		}
		StatementMessagesHandler statementMessagesHandler;

		#endregion

		public const string TariffUpdateRequestConfirmation = "Tariff information has been requested from ABI - please close the job, wait a couple of minutes and validate the tariff information is now correct. The message is attached to this job.";

		#region Data Transfer

		void ExportInvoiceLinesReconData_Click(object sender, EventArgs e)
		{
			if (CheckHasChangesAndGetConfirmationFromUsers())
			{
				try
				{
					SaveInternal();
					ReconFlattenedDataLineCollection collection = GetReconFlattenedDataLineCollection();

					ReconInvoiceLineFlatFileDataTransferProcessor processor = new ReconInvoiceLineFlatFileDataTransferProcessor();
					ProcessWhileShowingProgress(processor, () => processor.ExportReconDataToCollection(collection, BusinessEntity));

					ImportCollectionInfoImpl impl = new ImportCollectionInfoImpl(collection);
					impl.AddFlattenedDataPropertiesForLineExport();

					RunExportWizardForm(impl, ReconLineDataSettingKey);
				}
				catch (ZSaveException ex)
				{
					ZExceptionReporting.HandleSaveException(ex);
				}
			}
		}

		void RunExportWizardForm(ImportCollectionInfoImpl impl, string contextKey)
		{
			CultureInfo originalCulture = Culture.Current;
			try
			{
				Culture.Set(Culture.GetCulture(Core.Constants.CountryCodes.UnitedStates));
				using (Form form = new DataExportWizardForm(impl, contextKey))
				{
					form.ShowDialog();
				}
			}
			finally
			{
				Culture.Set(originalCulture);
			}
		}

		void UpdateInvoiceLinesReconData_Click(object sender, EventArgs e)
		{
			ReconFlattenedDataLineCollection collection = GetReconFlattenedDataLineCollection();
			ImportCollectionInfoImpl impl = new ImportCollectionInfoImpl(collection);
			impl.AddFlattenedDataPropertiesForLineUpdate();

			if (RunImportWizardForm(impl, ReconLineDataSettingKey) != DialogResult.Cancel)
			{
				var importer = new ReconInvoiceLineFlatFileDataTransferProcessor();
				ProcessWhileShowingProgress(importer, () => importer.ImportReconDataFromCollection(collection, BusinessEntity));
			}
		}

		const string ReconLineDataSettingKey = "5F71BC3D-F466-4EE9-B33A-002C2177F45A";

		void ExportEntriesReconData_Click(object sender, EventArgs e)
		{
			if (CheckHasChangesAndGetConfirmationFromUsers())
			{
				try
				{
					SaveInternal();

					ReconFlattenedDataLineCollection collection = GetReconFlattenedDataLineCollection();

					ReconEntryFlatFileDataTransferProcessor processor = new ReconEntryFlatFileDataTransferProcessor();
					ProcessWhileShowingProgress(processor, () => processor.ExportReconDataToCollection(collection, BusinessEntity));

					ImportCollectionInfoImpl impl = new ImportCollectionInfoImpl(collection);
					impl.AddFlattenedDataPropertiesForEntryExport();

					RunExportWizardForm(impl, ReconEntryDataSettingKey);
				}
				catch (ZSaveException ex)
				{
					ZExceptionReporting.HandleSaveException(ex);
				}
			}
		}

		void UpdateEntriesReconData_Click(object sender, EventArgs e)
		{
			ReconFlattenedDataLineCollection collection = GetReconFlattenedDataLineCollection();
			ImportCollectionInfoImpl impl = new ImportCollectionInfoImpl(collection);
			impl.AddFlattenedDataPropertiesForEntryUpdate();

			if (RunImportWizardForm(impl, ReconEntryDataSettingKey) != DialogResult.Cancel)
			{
				var importer = new ReconEntryFlatFileDataTransferProcessor();
				ProcessWhileShowingProgress(importer, () => importer.ImportReconDataFromCollection(collection, BusinessEntity));
			}
		}

		const string ReconEntryDataSettingKey = "7E9B2CEF-E0AF-4c2d-BB9E-FB19FC8115B5";

		DialogResult RunImportWizardForm(ImportCollectionInfoImpl impl, string contextKey)
		{
			DialogResult result = DialogResult.Cancel;
			CultureInfo originalCulture = Culture.Current;
			try
			{
				Culture.Set(Culture.GetCulture(Core.Constants.CountryCodes.UnitedStates));
				using (Form form = new DataImportWizardForm(impl, contextKey))
				{
					result = form.ShowDialog();
				}
			}
			finally
			{
				Culture.Set(originalCulture);
			}
			return result;
		}

		#endregion

		void ProcessWhileShowingProgress(ReconFlatFileDataTransferProcessor processor, ProcessReconDataTransferEventHander doProcess)
		{
			Cursor cursorBefore = Cursor;
			try
			{
				Cursor = Cursors.WaitCursor;
				processor.ProgressChanged += Importer_ProgressChanged;

				doProcess();
			}
			finally
			{
				processor.ProgressChanged -= Importer_ProgressChanged;
				if (importProgressForm != null)
				{
					importProgressForm.Cancelled -= ImportProgressForm_Cancelled;
					importProgressForm.Dispose();
					importProgressForm = null;
				}
				Cursor = cursorBefore;
			}
		}

		delegate void ProcessReconDataTransferEventHander();

		protected virtual ReconFlattenedDataLineCollection GetReconFlattenedDataLineCollection()
		{
			return new ReconFlattenedDataLineCollection(BusinessEntity.Factory);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule")]
		bool Importer_ProgressChanged(int percentComplete, string status)
		{
			if (importProgressForm == null)
			{
				isCancelled = false;
				importProgressForm = new ProgressForm();
				importProgressForm.TopMost = true;
				importProgressForm.Cancelled += ImportProgressForm_Cancelled;
				importProgressForm.Show();
				Application.DoEvents();
			}

			if (importProgressForm.PercentComplete != percentComplete)
			{
				importProgressForm.SetStatusAndPercentComplete(status, percentComplete);
				Application.DoEvents();
			}

			return !isCancelled;
		}
		void ImportProgressForm_Cancelled(object sender, EventArgs e)
		{
			isCancelled = true;
		}

		bool isCancelled;
		ProgressForm importProgressForm;

		#endregion

		#region Visibility

		void US_IsAggregateInfo_ValueChanged(object sender, EventArgs e)
		{
			ChangeControlsVisibilityForAggregateRecon();
		}
		void US_IssueCodeInfo_ValueChanged(object sender, EventArgs e)
		{
			ChangeCalculateCustomValuesMenuItemVisible();
		}

		void ChangeCalculateCustomValuesMenuItemVisible()
		{
			CalculateCustomValuesMenuItem.Visible = ReconIssueCodeList.IsAllowedChangeCustomsValues(reconDeclaration.US_IssueCode);
		}

		void ChangeControlsVisibilityForAggregateRecon()
		{
			if (reconDeclaration != null && !reconDeclaration.IsACE)
			{
				RefundedFeesTabPage.TabVisible = !reconDeclaration.US_IsAggregate;
			}
		}

		void JE_ApplicationCodeInfo_ValueChanged(object sender, EventArgs e)
		{
			ChangeControlsVisibilityForApplicationCode();
		}
		void ChangeControlsVisibilityForApplicationCode()
		{
			if (reconDeclaration.IsACE)
			{
				OriginalEntriesGrid.SetColumnCaption(AddInfo.Schema.US_R_NoLineDetails, "No Change");
				StatusNotificationsTabPage.TabVisible = true;
			}
			else
			{
				OriginalEntriesGrid.SetColumnCaption(AddInfo.Schema.US_R_NoLineDetails, "No Change/No Lines");
				StatusNotificationsTabPage.TabVisible = false;
			}
			string[] columns = new string[] { AddInfo.Schema.US_PriorDisclosure, AddInfo.Schema.US_NAFTAClaimStat,
				AddInfo.Schema.US_ProtestStat, AddInfo.Schema.US_ProtestID, AddInfo.Schema.US_PendingActionID, AddInfo.Schema.US_PendingActionIDType };
			OriginalEntriesGrid.SetAvailability(reconDeclaration.IsACE, columns);
			OriginalEntriesGrid.RefreshTableStyles();
			RefundedFeesTabPage.TabVisible = !reconDeclaration.IsACE && !reconDeclaration.US_IsAggregate;
			StatementsTabPage.TabVisible = reconDeclaration.IsACE;
			EntryLinesReconInvoiceLineUserControl.ChangeVisibilityForACE(reconDeclaration.IsACE);
			HeaderDetailsReconHeaderUserControl.ChangeVisibilityForACE(reconDeclaration.IsACE);
		}

		#endregion
	}
}
