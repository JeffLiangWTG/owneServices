using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.Universal.GUI
{
	public partial class RefHarbourRateForm : ZTemplateForm
	{
		public RefHarbourRateForm(RefHarbourRate dataSource) : base(dataSource)
		{
			ControllerID = ControllerIDs.Customs.Universal.RefHarbourRate;
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		protected override bool SupportsEDocs => false;

		protected override bool ShowNotesTab => false;

		public override string FormCaption => BusinessEntity?.HumanReadableName ?? Res.GetString("bb5dc52e-618e-44f0-a558-1223d445e2fd", "Global Harbor Rate");

		protected override void SaveToRecentItems()
		{
		}
	}
}
