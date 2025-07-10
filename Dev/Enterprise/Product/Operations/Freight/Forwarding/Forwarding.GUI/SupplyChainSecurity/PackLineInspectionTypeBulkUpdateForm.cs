using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI
{
	public partial class PackLineInspectionTypeBulkUpdateForm : ZForm
	{
		public PackLineInspectionTypeBulkUpdateForm(PackLineBulkUpdateDataSource dataSource)
			: base(dataSource)
		{
			InitializeComponent();
			ZFormPostingButtonsStrategy.SetupPosting(this, PostingButtonsUserControl);
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
