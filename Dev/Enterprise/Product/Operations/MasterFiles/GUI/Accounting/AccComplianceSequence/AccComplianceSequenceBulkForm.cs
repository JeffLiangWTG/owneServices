using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class AccComplianceSequenceBulkForm : ZForm
	{
		public AccComplianceSequenceBulkForm(AccComplianceSequenceBulkCreator creator) : base(creator)
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
	}
}
