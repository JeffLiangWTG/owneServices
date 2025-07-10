using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Environment.Module
{
	public partial class DynamicPickFacesFilterControl : ZFilterStripControl
	{
		public DynamicPickFacesFilterControl()
			: this(null, null)
		{
		}

		public DynamicPickFacesFilterControl(WhsDynamicPickFaceViewCollection collection, DynamicPickFacesFilterBusinessObject strip)
			: base(collection, strip)
		{
			InitializeComponent();
		}
	}
}

