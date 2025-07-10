using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Environment.Module
{
	public partial class WhsPutawayGroupFilterControl : ZFilterStripControl
	{
		public WhsPutawayGroupFilterControl()
			: this(null, null)
		{
		}

		public WhsPutawayGroupFilterControl(IBusinessObjectCollection collection, FilterStripBusinessObject strip)
			: base(collection, strip)
		{
			InitializeComponent();
		}
	}
}
