using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using static System.FormattableString;

namespace Enterprise.MasterFiles.GUI
{
	public partial class SqlSystemConfigurationsUserControl : ZUserControl, ITabPageContentHolder
	{
		public SqlSystemConfigurationsUserControl()
		{
			InitializeComponent();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (IsDisposing)
			{
				return;
			}

			connectedServer = Invariant($"{Db.Connection.ServerName}.{Db.Connection.ServerDomain}");

			LoadSystemConfigurations();
			RefreshDataBinding();

			systemConfigurations.HasProposedChangesChanged += SystemConfigurationsHasChangesChanged;
			SystemConfigurationsHasChangesChanged(this, HasChangesChangedEventArgs.Create(true, this));

			if (SqlSystemConfigurationsHelper.DenyUserEdit)
			{
				saveButton.Enabled = false;
				applyButton.Enabled = false;
			}
		}

		void SystemConfigurationsHasChangesChanged(object sender, EventArgs e)
		{
			if (!IsDisposing && !SqlSystemConfigurationsHelper.DenyUserEdit)
			{
				saveButton.Enabled = systemConfigurations.HasChanges && !systemConfigurations.HasErrors();
				applyButton.Enabled = (systemConfigurations.HasChanges || systemConfigurations.HasProposedChanges) && !systemConfigurations.HasErrors();
				changedConfigsListBox.DataSource = systemConfigurations.ChangedConfigs.ToArray();

				cancelButton.Text = applyButton.Enabled ? CancelCaption : CloseCaption;
			}
		}

		void ITabPageContentHolder.OnParentFormClosing(Form parent, FormClosingEventArgs e)
		{
			if (saveButton.Enabled)
			{
				var userConfirmation = Globals.Message.Show(UnsavedChangesBeforeClosingMessage, Text, MessageBoxButtons.YesNoCancel, DialogResult.Cancel);
				if (userConfirmation == DialogResult.Yes)
				{
					OnSaveButtonClick(this, EventArgs.Empty);
					return;
				}

				if (userConfirmation == DialogResult.Cancel)
				{
					e.Cancel = true;
					return;
				}
			}

			systemConfigurations.HasChangesChanged -= SystemConfigurationsHasChangesChanged;
			systemConfigurations.HasChanges = false;
		}

		void ITabPageContentHolder.OnParentTabControlSwitchingToOtherTab()
		{
			if (!saveButton.Enabled)
			{
				return;
			}

			var userConfirmation = Globals.Message.Show(UnsavedChangesBeforeSwitchingMessage, Text, MessageBoxButtons.YesNo, DialogResult.Yes);
			if (userConfirmation == DialogResult.Yes)
			{
				OnSaveButtonClick(this, EventArgs.Empty);
			}
		}

		void OnApplyButtonClick(object sender, EventArgs e)
		{
			var applyHasExceptions = false;

			try
			{
				ApplyProposedConfigurations();
				SystemConfigurationsHasChangesChanged(this, HasChangesChangedEventArgs.Create(true, this));
				Globals.Message.ShowInformation(SuccessfullyAppliedMessage + connectedServer, Text);
			}
			catch (Exception ex)
			{
				applyHasExceptions = true;
				ErrorReporter.ReportOnce(FailedToApplyProposedConfigs, ex);
				Globals.Message.ShowError(FailedToApplyProposedConfigs, MessageCaption);
			}

			if (!applyHasExceptions)
			{
				OnSaveButtonClick(sender, e);
			}
		}

		void OnSaveButtonClick(object sender, EventArgs e)
		{
			try
			{
				SaveSystemConfigurations();
				SystemConfigurationsHasChangesChanged(this, HasChangesChangedEventArgs.Create(true, this));
			}
			catch (Exception ex)
			{
				Globals.Message.ShowError(ex.Message, MessageCaption);
			}
		}

		void RefreshDataBinding()
		{
			SetDataBinding(null, string.Empty);
			SetDataBinding(systemConfigurations, string.Empty);
		}

		void LoadSystemConfigurations()
		{
			using (new ZWaitCursorChanger(ParentForm))
			{
				using (var connection = Db.NewAdminConnection(Db.SqlMasterDb))
				{
					InitializeSystemConfigurations(GetSqlServerConfigurations(connection));
				}

				SqlSystemConfigurationsHelper.Instance.LoadPersistedValuesFromRegistry(systemConfigurations.Configurations);
				SqlSystemConfigurationsHelper.Instance.ComputeOptimalValues(systemConfigurations.Configurations);
			}
		}

		protected virtual void SaveSystemConfigurations()
		{
			SqlSystemConfigurationsHelper.Instance.SaveConfigurationsToRegistry(systemConfigurations.Configurations);
			systemConfigurations.HasChanges = false;
		}

		protected virtual void ApplyProposedConfigurations()
		{
			using (new ZWaitCursorChanger(ParentForm))
			{
				using (var connection = Db.NewAdminConnection(Db.SqlMasterDb))
				{
					SqlSystemConfigurationsHelper.Instance.ApplyProposedValues(connection, systemConfigurations.ChangedConfigs);
				}
			}
		}

		void InitializeSystemConfigurations(SqlSystemConfigurationsCollection collection)
		{
			systemConfigurations = collection;
			changedConfigsListBox.DataSource = systemConfigurations.ChangedConfigs.ToArray();
		}

		protected virtual SqlSystemConfigurationsCollection GetSqlServerConfigurations(DbConnection connection)
		{
			return SqlSystemConfigurationsHelper.Instance.LoadSqlServerConfigurations(connection);
		}

		#region Fields

		static readonly MultilingualString SuccessfullyAppliedMessage = ResString.GetMultilingualString("CE4F5759-EEAA-4690-AF5C-BFB477DD0F13", "You have successfully applied proposed configurations to server: ");
		static readonly MultilingualString UnsavedChangesBeforeClosingMessage = ResString.GetMultilingualString("8A0F4E16-4E76-467B-AE9A-79B94F01CBDB", "You have some unsaved changes in 'Server Configurations', would you like to save the changes before exit?");
		static readonly MultilingualString UnsavedChangesBeforeSwitchingMessage = ResString.GetMultilingualString("3D555478-BE95-42C5-82FB-05BAFEEF2F61", "You have some unsaved changes in 'Server Configurations', would you like to save the changes before switching tab?");
		static readonly MultilingualString MessageCaption = ResString.GetMultilingualString("C184986A-0C69-4F74-A5B7-68CD7CAFCAE4", "Apply proposed SQL system configurations");
		static readonly MultilingualString CancelCaption = ResString.GetMultilingualString("C8F57D50-BF4B-425C-B9EB-E150CCBCDDA1", "Cancel");
		static readonly MultilingualString CloseCaption = ResString.GetMultilingualString("3FE7D7FB-4166-49DF-9B5B-B671A288DB2B", "Close");
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "string resource key for the error reporter")]
		const string FailedToApplyProposedConfigs = "Failed to apply one or more proposed configurations to the sql system";
		string connectedServer = string.Empty;

		SqlSystemConfigurationsCollection systemConfigurations;

		#endregion

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (systemConfigurations != null)
				{
					systemConfigurations.HasProposedChangesChanged -= SystemConfigurationsHasChangesChanged;
					systemConfigurations = null;
				}

				components?.Dispose();
			}

			base.Dispose(disposing);
		}
	}
}
