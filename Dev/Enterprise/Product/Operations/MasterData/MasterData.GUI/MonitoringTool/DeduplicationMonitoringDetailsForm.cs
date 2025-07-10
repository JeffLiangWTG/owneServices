using Enterprise.MasterData.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.MasterData.GUI
{
	[TestExcludeZWinFormsAllHaveFormBashers]
	public sealed partial class DeduplicationMonitoringDetailsForm : ZChildForm
	{
		public DeduplicationMonitoringDetailsForm(DeduplicationDebugReporter reporter)
			: base(reporter)
		{
			InitializeComponent();

			detailsTextBox.Text = reporter.DetailsText;
		}

		public override string FormVerb
		{
			get { return string.Empty; }
		}

		protected override bool AllowNew
		{
			get { return false; }
		}

		#region Form Caption

		public override string FormCaption
		{
			get { return Res.GetString("524c425a-6585-44e7-ac95-4a2b006a1d17", "Debug Report Details"); }
		}

		#endregion
	}
}
