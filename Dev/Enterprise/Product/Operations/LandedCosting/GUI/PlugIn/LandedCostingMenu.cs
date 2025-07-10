using System;
using System.Windows.Forms;
using Enterprise.LandedCosting.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.LandedCosting.GUI
{
	public delegate LandedCostHeader LandedCostHeaderDelegate();

	public class LandedCostingMenu : ZMenuItem
	{
		public LandedCostingMenu(Func<string> licenceLogIn)
			: base(ResString.GetMultilingualString("MenuItem.LandedCosting", "Landed Costing"))
		{
			this.licenceLogIn = licenceLogIn;
			ConstructSubMenus();
		}

		readonly Func<string> licenceLogIn;

		public LandedCostHeaderDelegate LCHeaderDelegate
		{
			get { return fLCHeaderDelegate; }
			set { fLCHeaderDelegate = value; }
		}
		LandedCostHeaderDelegate fLCHeaderDelegate;

		#region Implementation

		ZForm Form
		{
			get { return (ZForm)GetMainMenu().GetForm(); }
		}

		static string ConfirmationDespiteWarningsMessage
		{
			get { return "\r\n" + Res.GetString("69118888-7f95-4c7f-ae12-a69e1e068382", "Are you sure you wish to continue?"); }
		}
		static string LCLinesAndManualChargesWillBeDeletedWarningMessage
		{
			get { return Res.GetString("b93b9845-e7f3-4564-8075-2a7ac2b748c1", "Refreshing Landed Costing will delete existing LC lines including MANUALLY ENTERED CHARGES. Are you sure you wish to continue?"); }
		}
		static string LCLineWillBeDeletedWarningMessage
		{
			get { return Res.GetString("6c6ce29c-1ec8-488c-b55c-3eb7650e71e1", "Refreshing Landed Costing will delete existing LC lines and keep manually entered charges. Are you sure you wish to continue?"); }
		}

		internal MenuItem RunLCMenu;
		internal MenuItem RefreshLandedCostingData;
		internal MenuItem RefreshLandedCostingDataWithoutLosingUserEnteredData;

		LandedCostHeader LCHeader
		{
			get { return LCHeaderDelegate == null ? null : LCHeaderDelegate(); }
		}

		void ConstructSubMenus()
		{
			RunLCMenu = new ZMenuItem(ResString.GetMultilingualString("LandedCosting.RunLandedCosting", "Run Landed Costing"));
			RunLCMenu.Click += new EventHandler(RunLCMenu_Click);
			MenuItems.Add(RunLCMenu);

			RefreshLandedCostingData = new ZMenuItem(ResString.GetMultilingualString("LandedCosting.RefreshLandedCostingAfterDeleting", "Refresh Landed Costing (Re-importing charges after deleting LC lines and manually entered charges)"));
			RefreshLandedCostingData.Click += new EventHandler(RefreshLandedCostingData_Click);
			MenuItems.Add(RefreshLandedCostingData);

			RefreshLandedCostingDataWithoutLosingUserEnteredData = new ZMenuItem(ResString.GetMultilingualString("LandedCosting.RefreshLandedCostingWithoutLosing", "Refresh Landed Costing (Re-importing charges without losing manually entered charges)"));
			RefreshLandedCostingDataWithoutLosingUserEnteredData.Click += new EventHandler(RefreshLandedCostingDataWithoutLosingUserEnteredData_Click);
			MenuItems.Add(RefreshLandedCostingDataWithoutLosingUserEnteredData);
		}

		bool CheckPreRequisiteConditions()
		{
			bool conditionsAreSatisfied = false;

			if (LCHeader == null)
			{
				Globals.Message.ShowError(Res.GetString("35a723bf-e746-486a-9b16-cf02c99b6bd9", "No Landed Costing job exists. Please click the tab and create a Landed Costing job first."), Res.GetString("909f07c1-5b1b-4929-ac4f-f7e62cb47a58", "Landed Costing"));
			}
			else
			{
				var errorMessage = licenceLogIn();
				if (!string.IsNullOrEmpty(errorMessage))
				{
					Globals.Message.ShowError(errorMessage);
				}
				else
				{
					conditionsAreSatisfied = true;
				}
			}

			return conditionsAreSatisfied;
		}

		void RunLCMenu_Click(object sender, EventArgs e)
		{
			if (CheckPreRequisiteConditions() && LCHeader != null)
			{
				StatusCheckResult status = new LCDistributionStatusChecker(this.LCHeader).GetStatus();

				if (status.IsError)
				{
					Globals.Message.ShowError(status.Message, Res.GetString("909f07c1-5b1b-4929-ac4f-f7e62cb47a58", "Landed Costing"));
				}
				else if (!status.IsWarning || GetConfirmationFromUsers(status.Message + "\r\n" + ConfirmationDespiteWarningsMessage) == DialogResult.Yes)
				{
					LCDistributionManager manager = new LCDistributionManager(LCHeader);
					manager.RunLandedCosting();

					if (Globals.Message.Show(Res.GetString("2f76e1ba-bef3-43cc-80a2-361bbd2489b3", "Landed Costing finished running. Do you want to save the changes?"), Res.GetString("909f07c1-5b1b-4929-ac4f-f7e62cb47a58", "Landed Costing"), MessageBoxButtons.YesNo, DialogResult.No) == DialogResult.Yes)
					{
						Form.FireSaveButton();
					}
				}
			}
		}

		void RefreshLandedCostingData_Click(object sender, EventArgs e)
		{
			RefreshData(LCLinesAndManualChargesWillBeDeletedWarningMessage, true);
		}

		void RefreshLandedCostingDataWithoutLosingUserEnteredData_Click(object sender, EventArgs e)
		{
			RefreshData(LCLineWillBeDeletedWarningMessage, false);
		}

		void RefreshData(string confirmationQuestion, bool deleteUserEnteredCharges)
		{
			if (CheckPreRequisiteConditions() && LCHeader != null)
			{
				bool goForRefreshing = true;
				if (LCHeader.Histories.Count > 0 || LCHeader.CostInputs.Count > 0)
				{
					if (GetConfirmationFromUsers(confirmationQuestion) == DialogResult.No)
					{
						goForRefreshing = false;
					}
				}

				if (goForRefreshing)
				{
					LCHeader.ResetToOriginal();
					if (deleteUserEnteredCharges)
					{
						LCHeader.CostInputs.RemoveAndDeleteAll();
					}
					else
					{
						LCHeader.CostInputs.DeleteSystemDefaultedRowsOnly();
					}
					LCHeader.SynchroniseAll();
					Globals.Message.ShowInformation(Res.GetString("0536fb5e-686d-47e2-a2c1-87248d738bf1", "Landed Costing Data Refreshed"), Res.GetString("909f07c1-5b1b-4929-ac4f-f7e62cb47a58", "Landed Costing"));
				}
			}
		}

		DialogResult GetConfirmationFromUsers(string question)
		{
			return Globals.Message.Show(
				question,
				Res.GetString("909f07c1-5b1b-4929-ac4f-f7e62cb47a58", "Landed Costing"),
				MessageBoxButtons.YesNo,
				MessageBoxIcon.Question);
		}

		#endregion
	}
}
