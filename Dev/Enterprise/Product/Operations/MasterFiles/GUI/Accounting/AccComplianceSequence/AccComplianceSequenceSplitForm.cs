using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class AccComplianceSequenceSplitForm : ZForm, IButtonPostTextOverride
	{
		protected AccComplianceSequenceSplitForm()
		{
		}

		public AccComplianceSequenceSplitForm(AccComplianceSequenceSplitViewModel model) : base(model)
		{
			DisplayMode = ODisplayMode.New;
			ZFormPostingButtonsStrategy.SetupPosting(this, PostingButtonsUserControl, true);
			PostingButtonsUserControl.SaveButton.Visible = false;
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		public override string FormVerb
		{
			get { return string.Empty; }
		}

		protected override bool AllowNew
		{
			get { return false; }
		}

		string IButtonPostTextOverride.PostButtonText => Res.GetString("3F2A7B13-625B-49EB-8497-04BB59A44D79", "Split");

		protected override ContinueWithSave ValidateAndSave()
		{
			var result = base.ValidateAndSave();

			if (result == ContinueWithSave.Yes)
			{
				Globals.Message.Show(Res.GetString("F38F6E28-1A3F-447D-BFD0-2EB8DFBFE363", "Compliance Sequence book split successfully."));
			}

			return result;
		}
	}
}
