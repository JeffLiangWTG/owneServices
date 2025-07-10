using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Transactions.Module
{
	public partial class VASOrderFilterControl : ZFilterStripControl<InvoicingFilterStrip>
	{
		#region Constructors

		public VASOrderFilterControl()
		{
			InitializeComponent();
		}

		public VASOrderFilterControl(WhsVASOrderCollection collection, VASOrderFilterBusinessObject filterBusinessObject)
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
