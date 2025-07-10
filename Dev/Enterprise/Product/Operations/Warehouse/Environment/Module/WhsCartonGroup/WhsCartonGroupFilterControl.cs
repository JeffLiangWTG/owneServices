using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Environment.Module
{
	public partial class WhsCartonGroupFilterControl : ZFilterStripControl
	{
		public WhsCartonGroupFilterControl()
			: this(null, null)
		{
		}

		public WhsCartonGroupFilterControl(IBusinessObjectCollection collection, FilterStripBusinessObject strip)
			: base(collection, strip)
		{
			InitializeComponent();
		}
	}
}
