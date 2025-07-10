using System;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.GUI
{
	public partial class BulkUpdateActionsPage : WizardPageStep
	{
		public BulkUpdateActionsPage()
		{
			InitializeComponent();

			AddChargeRadioButton.CheckedChanged += new EventHandler(OnSelectedActionCheckedChanged);
			DeleteRadioButton.CheckedChanged += new EventHandler(OnSelectedActionCheckedChanged);
			IncreaseChargeRadioButton.CheckedChanged += new EventHandler(OnSelectedActionCheckedChanged);
			ReplaceRadioButton.CheckedChanged += new EventHandler(OnSelectedActionCheckedChanged);
			CreateNewEntryCheckBox.CheckedChanged += OnSelectedNewEntryCheckedChanged;
		}

		#region Implementation

		public override void NotifyActivated(WizardForm wizard)
		{
			base.NotifyActivated(wizard);

			Updater.ClearAllNotifications();

			wizard.PageHeaderTitle = Res.GetString("0272d4dc-2693-49ea-82cc-9802e9964f00", "Actions");
			wizard.PageHeaderDescription = Res.GetString("eb548bb8-48ab-45a4-8997-c6ae019b1c07", "State the charge code that will be added, replaced, increased/decreased or deleted");
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);

			if (dataSource != null)
			{
				rateLineControl.DataBindings.Add(nameof(rateLineControl.BindingDataSource), Updater, nameof(Updater.ActionsLine), false, DataSourceUpdateMode.OnPropertyChanged);
			}
		}

		public override void NotifyLeaving(WizardSteppingEventArgs args)
		{
			base.NotifyLeaving(args);

			if (args.MovementDirection != WizardSteppingEventArgs.Direction.Forward)
			{
				return;
			}

			if (!ValidatePage())
			{
				args.Cancel = true;

				if (Updater.ActionsLine.HasErrors())
				{
					// M.K: SuppressMessage CA2000 does not work - have to dispose manually.
					using (var errorMessageBox = new ZErrorMessageBox(Updater.ActionsLine,
						Res.GetString("ca052213-aa4b-4e77-8ade-9816eeb246c2", "action"),
						Res.GetString("c7dbdb3d-4222-4708-bb90-9eba59e2c048", "continue"),
						Res.GetString("42307b48-637c-4b9a-b23b-a9d6fc57ac4c", "continued")))
					{
						ZFormModaliser.ShowDialogWithoutDispose(errorMessageBox);
					}
				}
				else
				{
					Globals.Message.ShowError(Res.GetString("8d36ead4-2d6d-4bb5-a6fd-9d04ed015797", "You have provided incorrect actions criteria"), UnableToProceedMessage);
				}
			}
		}

		bool ValidatePage()
		{
			Updater.PreviewEntries.RemoveAll(); // invalid preview entries mark Updater invalid

			Updater.ClearAllNotifications();
			Updater.RunPreSaveValidation();
			Updater.ActionsLine.RunPreSaveValidation();

			return !Updater.HasErrors && (!Updater.ActionsLine.HasErrors() || (Updater.DeleteCharge && !Updater.ActionsLine.TL_ACInfo.HasErrors()));
		}

		#endregion

		#region Event Handlers

		void OnSelectedActionCheckedChanged(object sender, EventArgs e)
		{
			ZRadioButton rb = sender as ZRadioButton;
			if (rb != null && rb.Checked)
			{
				rateLineControl.CalculatorPanelVisible = rb != DeleteRadioButton;
				rateLineControl.CalculatorPanelCalculatorDropEditVisible = rb != IncreaseChargeRadioButton;
				rateLineControl.CalculatorPanelAgentRatesCheckBoxVisible = rb != IncreaseChargeRadioButton;
				StandardRatesRadioButton.Enabled = rb == IncreaseChargeRadioButton;
				AgentRatesRadioButton.Enabled = rb == IncreaseChargeRadioButton;
				BothRatesRadioButton.Enabled = rb == IncreaseChargeRadioButton;
			}
		}

		void OnSelectedNewEntryCheckedChanged(object sender, EventArgs e)
		{
			var applyNewCheck = sender as ZCheckBox;
			if (applyNewCheck != null)
			{
				NewEntryStartDateEdit.Visible = applyNewCheck.Checked;
				NewEntryEndDateEdit.Visible = applyNewCheck.Checked;
			}
		}

		#endregion
	}
}

