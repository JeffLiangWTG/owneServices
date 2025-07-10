using System;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.MasterFiles.Business.Accounting.Netting;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI.Accounting.Netting
{
	public partial class NettingImportInitiatorForm : ZChildForm, IButtonPostTextOverride, IPostingButtonsProvider
	{
		public NettingImportInitiatorForm()
		{
			InitializeComponent();
		}

		public NettingImportInitiatorForm(NettingImportInitiator bizO) : base(bizO)
		{
			InitializeComponent();
			ZFormPostingButtonsStrategy.SetupPosting(this, ButtonsUserControl);
		}

		public override string FormVerb
		{
			get { return string.Empty; }
		}

		protected override string FormClosingQuestion => Res.GetString("6E45E6A1-136E-442F-B5EA-FFC3AA6F90C6", "Do you still want to queue transactions for Netting with these filters?");

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			nettingCentreDropEdit.SelectedIndexChanged += NettingCentreDropEdit_SelectedIndexChanged;
		}

		string IButtonPostTextOverride.PostButtonText => Res.GetString("F426AAB7-BF29-43FD-8DE9-E96CDF72444A", "Queue");

		void NettingCentreDropEdit_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (ImportInitiator != null)
			{
				activityTrailTextBox.AppendText(ImportInitiator.TrailLog);
			}
		}

		NettingImportInitiator ImportInitiator => (NettingImportInitiator)BusinessEntity;

		bool IPostingButtonsProvider.IsPostOnly
		{
			get { return true; }
			set { }
		}

		protected override ContinueWithSave ValidateAndSave()
		{
			var saveResult = base.ValidateAndSave();

			if (saveResult == ContinueWithSave.Yes)
			{
				Globals.Message.ShowInformation(Res.GetString("61019276-42B1-4A52-9E47-38153D9A4C78", @"Total: {0} transactions were queued to the Netting System.
Please allow sometime for the transactions to be processed.
The progress of the import can be tracked by checking the log for 'UMI' service task."
					, ImportInitiator.NumberOfQueuedTransactions.ToString()));
			}

			return saveResult;
		}

		protected override void Save(ITransactionParticipant[] factories)
		{
			base.Save(factories);
			ImportInitiator.QueueTransactionsForImport();
		}
	}
}
