using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Transactions.Module
{
	public partial class AdHocServiceJobFilterControl : ZFilterStripControl
	{
		#region Constructors

		public AdHocServiceJobFilterControl()
		{
			InitializeComponent();
		}

		public AdHocServiceJobFilterControl(WhsAdHocServiceJobCollection collection, AdHocServiceJobFilterBusinessObject filterBusinessObject)
			: base(collection, filterBusinessObject)
		{
			InitializeComponent();
		}

		#endregion

		protected override ZFilterStrip NewZFilterStrip()
		{
			return new WhsWorkflowFilterStrip();
		}
	}
}
