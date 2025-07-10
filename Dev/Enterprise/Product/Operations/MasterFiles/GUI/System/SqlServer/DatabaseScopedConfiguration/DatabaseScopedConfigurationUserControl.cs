using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	sealed partial class DatabaseScopedConfigurationUserControl : ZUserControl, ITabPageContentHolder
	{
		public DatabaseScopedConfigurationUserControl()
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

			using (new ZWaitCursorChanger(ParentForm))
			using (var connection = Db.NewAdminConnection())
			{
				viewModel = new DatabaseScopedConfigurationViewModel();
				viewModel.SelectedDatabase = Db.DatabaseName;
				viewModel.PropertyChanged += ViewModel_PropertyChanged;
				OnDatabaseContainerSelected();

				SetDataBinding(viewModel, string.Empty);
				RefreshButtonStates();
			}
		}

		DatabaseScopedConfigurationContainer lastSelectedDatabaseContainerInfo;
		void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(viewModel.SelectedDatabaseContainer))
			{
				OnDatabaseContainerSelected();
			}
		}

		void OnDatabaseContainerSelected()
		{
			StopListeningToLastSelectedContainer(lastSelectedDatabaseContainerInfo);

			var selectedDatabase = viewModel.SelectedDatabaseContainer;
			ListenToSelectedContainer(selectedDatabase);
			lastSelectedDatabaseContainerInfo = selectedDatabase;

			configurationsGrid.RefreshBinding(selectedDatabase.Configurations);
		}

		void ListenToSelectedContainer(DatabaseScopedConfigurationContainer database)
		{
			if (database == null)
			{
				return;
			}

			foreach (var configuration in database.Configurations)
			{
				configuration.ProposedValueInfo.ValueChanged -= OnConfigurationValueChanged;
				configuration.ProposedValueInfo.ValueChanged += OnConfigurationValueChanged;
			}
		}

		void StopListeningToLastSelectedContainer(DatabaseScopedConfigurationContainer database)
		{
			if (database == null)
			{
				return;
			}

			foreach (var configuration in database.Configurations)
			{
				configuration.ProposedValueInfo.ValueChanged -= OnConfigurationValueChanged;
			}
		}

		void OnConfigurationValueChanged(object sender, EventArgs e)
		{
			RefreshButtonStates();
		}

		void ITabPageContentHolder.OnParentFormClosing(Form parent, FormClosingEventArgs e)
		{
			if (!RefreshButtonStates())
			{
				return;
			}

			var userConfirmation = Globals.Message.Show(UnappliedChangesBeforeClosingMessage, Text, MessageBoxButtons.YesNoCancel, DialogResult.Cancel);
			if (userConfirmation == DialogResult.Yes)
			{
				Apply();
				return;
			}

			if (userConfirmation == DialogResult.Cancel)
			{
				e.Cancel = true;
				return;
			}
		}

		void ITabPageContentHolder.OnParentTabControlSwitchingToOtherTab()
		{
			if (!RefreshButtonStates())
			{
				return;
			}

			var userConfirmation = Globals.Message.Show(UnappliedChangesBeforeSwitchingMessage, Text, MessageBoxButtons.YesNo, DialogResult.Yes);
			if (userConfirmation == DialogResult.Yes)
			{
				Apply();
			}
		}

		void OnApplyButtonClick(object sender, EventArgs e)
		{
			if (!RefreshButtonStates())
			{
				return;
			}

			Apply();
			RefreshButtonStates();
		}

		void Apply()
		{
			try
			{
				using (new ZWaitCursorChanger(ParentForm))
				{
					viewModel?.Save();
				}

				Globals.Message.ShowInformation(SuccessfullyAppliedMessage, Text);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				Globals.Message.ShowError(FailedToApplyProposedConfigs, MessageCaption);
			}
		}

		bool RefreshButtonStates()
		{
			var needToSave = viewModel?.CanBeSavedPotentially ?? false;
			applyButton.Enabled = needToSave;
			cancelButton.Text = needToSave ? CancelCaption : CloseCaption;
			return needToSave;
		}

		static readonly MultilingualString SuccessfullyAppliedMessage = ResString.GetMultilingualString("620B8E3C-56B3-40F1-A048-0389645FF130", "You have successfully applied proposed database scoped configurations");
		static readonly MultilingualString UnappliedChangesBeforeClosingMessage = ResString.GetMultilingualString("F31385DA-CAF8-4716-910B-76DBD6081A0A", "You have some un-applied changes in 'Database Configurations', would you like to apply the changes before exit?");
		static readonly MultilingualString UnappliedChangesBeforeSwitchingMessage = ResString.GetMultilingualString("9B8C0237-6898-42C6-B01D-4F638AD7328D", "You have some un-applied changes in 'Database Configurations', would you like to apply the changes before switching tab?");
		static readonly MultilingualString MessageCaption = ResString.GetMultilingualString("11D1BDF6-E9EE-4354-A9FD-5F63B8ED1281", "Apply proposed database scoped configurations");
		static readonly MultilingualString CancelCaption = ResString.GetMultilingualString("10A48444-E89A-4E4D-9FD3-CDB53015FF6D", "Cancel");
		static readonly MultilingualString CloseCaption = ResString.GetMultilingualString("C23798EC-EC56-4987-A2F8-E9F9120ECAD1", "Close");
		static readonly MultilingualString FailedToApplyProposedConfigs = ResString.GetMultilingualString("9C345E37-EDBE-479B-BE7F-B7BB3DBCB711", "Failed to apply one or more proposed configurations to database, please see details in grid row notifications");

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (viewModel != null)
				{
					StopListeningToLastSelectedContainer(lastSelectedDatabaseContainerInfo);
					viewModel.Dispose();
					viewModel = null;
				}

				components?.Dispose();
			}

			base.Dispose(disposing);
		}
	}
}
