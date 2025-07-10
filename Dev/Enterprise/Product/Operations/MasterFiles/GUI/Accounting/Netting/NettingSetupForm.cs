using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.MasterFiles.Business.Accounting.Netting;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI.Accounting.Netting
{
	public partial class NettingSetupForm : ZChildForm, IButtonPostTextOverride, IPostingButtonsProvider
	{
		public NettingSetupForm()
		{
			InitializeComponent();
		}

		public NettingSetupForm(NettingSetupManager bizO) : base(bizO)
		{
			InitializeComponent();
			ZFormPostingButtonsStrategy.SetupPosting(this, ButtonsUserControl);
		}

		public override string FormVerb
		{
			get { return string.Empty; }
		}

		protected override string FormClosingQuestion => Res.GetString("4F8B09BB-A04B-4CEB-ABF5-5209017C4345", "Do you want to continue with the Netting Setup?");

		string IButtonPostTextOverride.PostButtonText => Res.GetString("E8F0DBF4-E65B-4B9B-8637-741C56ABDDA5", "Setup");

		#region IPostingButtonsProvider Members

		bool IPostingButtonsProvider.IsPostOnly
		{
			get { return true; }
			set { }
		}

		#endregion

		NettingSetupManager SetupManager => (NettingSetupManager)BusinessEntity;

		protected override ContinueWithSave ValidateAndSave()
		{
			var saveResult = base.ValidateAndSave();
			if (saveResult == ContinueWithSave.Yes && SetupManager.IsSetupSuccessful)
			{
				Globals.Message.ShowInformation(Res.GetString("8EB0820A-73CE-454B-9F47-0E1B8E95D1FA", "Netting setup successfully completed."));
			}
			else if (saveResult == ContinueWithSave.Yes && !SetupManager.IsSetupSuccessful)
			{
				Globals.Message.ShowError(Res.GetString("7B7896E4-F36E-4551-818C-7503F7FFBFD2", @"Netting Setup was not successful.
Please make sure a Netting System already does not exist with the same Code or a Netting system is not already set up with '{0}' as Netting company", SetupManager.NettingSystemCompanyCode));
			}

			return saveResult;
		}

		protected override void Save(ITransactionParticipant[] factories)
		{
			base.Save(factories);
			try
			{
				SetupManager.SetupNetting();
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				Globals.Message.Show(ex.Message);
			}
		}
	}
}
