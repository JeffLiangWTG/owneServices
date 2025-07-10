using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Telematics.Module.Filters
{
	public partial class GlbDeviceFilterControl : ZFilterStripControl
	{
		public GlbDeviceFilterControl(IBusinessObjectCollection gridCollection, GlbDeviceFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}
