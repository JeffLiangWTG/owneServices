using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Transactions.Module
{
	public partial class WhsInventoryHeldCodeFilterControl : ZFilterStripControl
	{
		public WhsInventoryHeldCodeFilterControl()
			: this(null, null)
		{
		}

		public WhsInventoryHeldCodeFilterControl(IBusinessObjectCollection collection, FilterStripBusinessObject strip)
			: base(collection, strip)
		{
			InitializeComponent();
		}
	}
}
