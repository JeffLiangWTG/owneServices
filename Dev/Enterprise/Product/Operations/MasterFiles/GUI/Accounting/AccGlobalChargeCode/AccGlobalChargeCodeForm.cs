using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class AccGlobalChargeCodeForm : AccChargeCodeForm, IZForm
	{
		ZArchitecture.ZGrid LocalChargeCodesGrid;
		ZTabPage LocalChargeCodesTab;

		public AccGlobalChargeCodeForm(AccChargeCode chargeCode) : base(chargeCode) { }

		public AccGlobalChargeCodeForm() : base() { }

		protected override AccChargeCodeFormDisplayPolicy DisplayPolicy
		{
			get
			{
				return new AccChargeCodeFormDisplayPolicy
				{
					HideBranchOverridesTab = true,
					HideSellComplianceDescriptionTab = true,
					HideGSTTaxID = true,
					HideGSTTaxOverridesTab = true,
					HideWithholdingTaxID = true,
					HideGovtChargeCode = true,
					HidePlaceOfSupplyConfigurationTab = true,
					HideCreditorOverrideTab_CreditorColumns = true,
					HideSupplyTypeOverrideTab = true
				};
			}
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();

			if (!ChargeCode.IsInDatabase)
			{
				LocalChargeCodesTab.TabVisible = false;
			}
		}

		protected override void Save(CargoWise.Integration.ITransactionParticipant[] factories)
		{
			base.Save(factories);

			if (!LocalChargeCodesTab.TabVisible && ChargeCode.IsInDatabase)
			{
				LocalChargeCodesTab.TabVisible = true;
			}
		}

		protected override ContinueWithSave ShowPreSaveDialogs()
		{
			var businessObject = (AccChargeCode)BusinessEntity;

			if (!businessObject.IsInDatabase)
			{
				var result = Globals.Message.Show(
					Res.GetString("AccGlobalChargeCodeForm|b2687fff-9552-4bd6-9726-f13d63dd0ad3", "This action will create a charge code called '{0}' for every company in the system. Continue?", businessObject.AC_Code),
					Res.GetString("AccGlobalChargeCodeForm|aea98f55-082f-4b3f-8d1f-7fb544a049f2", "Global Charge Code"),
					System.Windows.Forms.MessageBoxButtons.YesNo,
					MessageBoxIcon.Question);

				if (result == System.Windows.Forms.DialogResult.No)
				{
					return ContinueWithSave.No;
				}
			}

			return ContinueWithSave.Yes;
		}

		void LocalChargeCodesGrid_Initialized(object sender, EventArgs e)
		{
			LocalChargeCodesGrid_SetupContextMenu();
		}

		override protected bool LocalLanguageDescriptionTextBoxShouldBeVisible
		{
			get { return false; }
		}

		#region SetupContextMenu and Handlers

		void LocalChargeCodesGrid_SetupContextMenu()
		{
			LocalChargeCodesGrid.ContextMenu.MenuItems.Add(
				new ZMenuItem(ResString.GetMultilingualString("AccGlobalChargeCodeForm|8c60c8b1-c441-49d6-8e99-a4d14b8aef68", "Edit"),
					new EventHandler(OnEditChargeCodeClicked)));

			LocalChargeCodesGrid.RowsDeleting += LocalChargeCodesGrid_RowsDeleting;
			LocalChargeCodesGrid.DoubleClick += OnEditChargeCodeClicked;
		}

		void LocalChargeCodesGrid_RowsDeleting(object sender, ZArchitecture.RowsDeletingEventArgs e)
		{
			var count = e.Objects.Count();

			var result = Globals.Message.Show(
				count == 1 ?
					Res.GetString("AccGlobalChargeCodeForm|86abe2ae-f1ab-437f-90be-096a39b9be0a", "Are you sure you want to delete this charge code?") :
					Res.GetString("AccGlobalChargeCodeForm|20dcb854-7bc8-47a4-ad3c-053ac2cb906b", "Are you sure you want to delete these charge codes?"),
				Res.GetString("AccGlobalChargeCodeForm|cc4dc4a5-dda2-4486-ad5d-5f54c90ee409", "Delete Charge Code"),
				 MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, DialogResult.No);
			if (result == DialogResult.No)
			{
				e.Cancel = true;
			}
		}

		void OnEditChargeCodeClicked(object sender, EventArgs e)
		{
			if (BusinessEntity.HasChanges)
			{
				Globals.Message.Show(Res.GetString("AccGlobalChargeCodeForm|21e639f9-8ef4-4f19-ad1e-340f80c998a4", "Please save changes to the Global Charge Code before editing local Charge Codes"));
				return;
			}
			if (LocalChargeCodesGrid.SelectedElements.Length == 0)
			{
				Globals.Message.Show(Res.GetString("AccGlobalChargeCodeForm|7f316d46-48fe-4280-b08a-e02b2402fc26", "Please select a Charge Code to edit"));
				return;
			}
			if (LocalChargeCodesGrid.SelectedElements.Length > 1)
			{
				Globals.Message.Show(Res.GetString("AccGlobalChargeCodeForm|2a66bfbc-d439-4998-a2f9-c126db5b512e", "Please select just one Charge Code to edit"));
				return;
			}
			var chargeCodeToEdit = (AccChargeCode)LocalChargeCodesGrid.SelectedElements.First();
			if (chargeCodeToEdit.HasChanges) // Defensive coding. The charge codes shouldn't be changed directly or indirectly as a result of anything you do in the global charge code form, until you save.
			{
				Globals.Message.Show(Res.GetString("AccGlobalChargeCodeForm|21e639f9-8ef4-4f19-ad1e-340f80c998a4", "Please save changes to the Global Charge Code before editing local Charge Codes"));
				return;
			}

			if (chargeCodeToEdit.Company.FirstActiveBranch == null)
			{
				Globals.Message.Show(Res.GetString("AccGlobalChargeCodeForm|FC90BF7E-5B9D-4D57-96F7-1CB46E5BCB50", "Please select a company which has active branches"));
				return;
			}

			using (Env.SetTemporaryUserContext(Env.CurrentUser.PK, chargeCodeToEdit.Company.FirstActiveBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var newFactoryToEditCode = new BusinessObjectFactory();
				var chargeCodeToEditInNewFactory = newFactoryToEditCode.Load<AccChargeCode>(chargeCodeToEdit.PK);

				using (var form = new AccChargeCodeForm(chargeCodeToEditInNewFactory))
				{
					form.ShowDifferenceWarnings = true;
					ZFormModaliser.ShowDialogAndDispose(form);
#if DEBUG
					if (Globals.IsTest)
					{
						if (AssertOnChildForm != null)
						{
							AssertOnChildForm(form);
						}
					}
#endif
				}
			}

			chargeCodeToEdit.Reload();
		}

#if DEBUG
		public Action<AccChargeCodeForm> AssertOnChildForm;
#endif

		#endregion
	}
}
