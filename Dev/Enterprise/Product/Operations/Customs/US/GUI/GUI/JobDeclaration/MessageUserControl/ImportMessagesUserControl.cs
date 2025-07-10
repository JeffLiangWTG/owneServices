using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.ZArchitecture.GUI;
using ErrorsRecord = Enterprise.Customs.US.Business.ErrorsRecord;

namespace Enterprise.Customs.US.GUI
{
	public partial class ImportMessagesUserControl : Customs.GUI.MessageUserControl
	{
		public ImportMessagesUserControl()
		{
			InitializeComponent();
			messageInterpretationBox.Font = new System.Drawing.Font("Courier New", 8F);
			messageDetailsTextBox.Font = new System.Drawing.Font("Courier New", 8F);
			this.MessagesTabControl.SelectedTab = messageDetailsTabPage;

			InitializeMessagesGrid();

			StatusesErrorsLabel.AllowOverlap(messagesStatusErrorsPanel);
		}

		void InitializeMessagesGrid()
		{
			MessagesGrid.ReOrderColumns(ColumnNamesInSortOrder);
			MessagesGrid.SetAllColumnsVisible(false);
			MessagesGrid.SetColumnVisible(true, DefaultColumnsForGrid);
			MessagesGrid.SetColumnWidth(EDIMessage.Schema.EM_MessageDateTime, 100);
			MessagesGrid.SetColumnWidth(EDIMessage.Schema.EM_User, 100);
		}

		void OnCurrentChanged(object sender, EventArgs e)
		{
			var message = MessagesGrid.ListManager?.GetCurrent() as BaseEDIMessage;
			var isVisible = message?.IsInterpretationInHtmlFormat ?? false;

			messageInterpretationBox.Visible = isVisible;
			messageDetailsTextBox.Visible = !isVisible;
			statusErrorsTabPage.TabVisible = message?.HasStatusOrErrors ?? true;
		}

		void RemoveUnnecessaryColumns()
		{
			if (DataSource is JobDeclaration declaration && declaration != null && (declaration.IsFTZAdmission || declaration.IsDrawback))
			{
				InBondHeadersGrid.RemoveFromAvailableColumns(MessageActionRelatedRecordWrapper.Schema.EntryStatus,
					MessageActionRelatedRecordWrapper.Schema.EntryStatusDesc,
					MessageActionRelatedRecordWrapper.Schema.ReleaseDate,
					MessageActionRelatedRecordWrapper.Schema.TIBExpiryDate,
					MessageActionRelatedRecordWrapper.Schema.TIBExpiryDate,
					MessageActionRelatedRecordWrapper.Schema.TIBNumOfExtensions);

				InBondHeadersGrid.SetAllColumnsVisible(true);
			}
		}

		void SetStatusErrorsGridColumnsVisibility()
		{
			if (DataSource is JobDeclaration declaration && declaration != null && !declaration.IsACE)
			{
				this.messagesStatusErrorsUserControl.StatusesAndErrorsGrid.RemoveFromAvailableColumns(ErrorsRecord.Schema.TariffNumber);
			}
		}

		string[] ColumnNamesInSortOrder
		{
			get
			{
				if (columnNamesInSortOrder == null)
				{
					columnNamesInSortOrder =
					[
						EDIMessage.Schema.EM_MessageNum,
						EDIMessage.Schema.MessageTypeDescription,
						EDIMessage.Schema.EM_MessageDateTime,
						EDIMessage.Schema.EM_DateTimeInterchangeSent,
						EDIMessage.Schema.EM_User,
						EDIMessage.Schema.EM_SystemCreateTimeUtc,
						EDIMessage.Schema.EM_InterchangeNumber,
						EDIMessage.Schema.EM_MessageSubType
					];
				}
				return columnNamesInSortOrder;
			}
		}
		string[] columnNamesInSortOrder;

		public bool ShowMessageHeaderGrid
		{
			get => relatedRecordsGroupBox.Visible;
			set => relatedRecordsGroupBox.Visible = value;
		}

		string[] DefaultColumnsForGrid
		{
			get
			{
				if (defaultColumnsForGrid == null)
				{
					List<string> columns = new List<string>();
					columns.Add(EDIMessage.Schema.EM_MessageNum);
					columns.Add(EDIMessage.Schema.MessageTypeDescription);
					columns.Add(EDIMessage.Schema.EM_MessageDateTime);
					columns.Add(EDIMessage.Schema.EM_DateTimeInterchangeSent);
					columns.Add(EDIMessage.Schema.EM_User);
					defaultColumnsForGrid = columns.ToArray();
				}

				return defaultColumnsForGrid;
			}
		}
		string[] defaultColumnsForGrid;

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);

			if (dataSource != null)
			{
				SetStatusErrorsGridColumnsVisibility();
				RemoveUnnecessaryColumns();
				if (StatusesErrorsLabel.DataBindings[StatusLableVisibilityForBinding] == null)
				{
					var bindingPath = string.IsNullOrEmpty(dataMember)
						? "InBondRelatedRecords.MessagesToShow.StatusesAndErrorsVisible"
						: dataMember + ".InBondRelatedRecords.MessagesToShow.StatusesAndErrorsVisible";
					StatusesErrorsLabel.DataBindings.Add(new KBinding(StatusLableVisibilityForBinding, BindingSource.DataSource, bindingPath, true, DataSourceUpdateMode.Never));
				}

				var listManager = MessagesGrid.ListManager;
				if (listManager != null)
				{
					listManager.CurrentChanged += OnCurrentChanged;
					OnCurrentChanged(null, EventArgs.Empty);
				}
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

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				var listManager = MessagesGrid?.ListManager;

				if (listManager != null)
				{
					listManager.CurrentChanged -= OnCurrentChanged;
				}
			}

			base.Dispose(disposing);
		}
	}
}
