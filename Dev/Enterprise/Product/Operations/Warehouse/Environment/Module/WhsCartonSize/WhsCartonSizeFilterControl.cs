using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Environment.Module
{
	public partial class WhsCartonSizeFilterControl : ZFilterStripControl
	{
		public WhsCartonSizeFilterControl()
			: this(null, null)
		{
		}

		public WhsCartonSizeFilterControl(IBusinessObjectCollection collection, FilterStripBusinessObject strip)
			: base(collection, strip)
		{
			InitializeComponent();
		}
	}
}
