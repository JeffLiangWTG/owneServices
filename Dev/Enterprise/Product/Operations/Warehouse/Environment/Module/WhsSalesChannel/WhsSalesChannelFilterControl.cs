using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Environment.Module
{
	public partial class WhsSalesChannelFilterControl : ZFilterStripControl
	{
		public WhsSalesChannelFilterControl()
			: this(null, null)
		{
		}

		public WhsSalesChannelFilterControl(IBusinessObjectCollection collection, FilterStripBusinessObject strip)
			: base(collection, strip)
		{
			InitializeComponent();
		}
	}
}
