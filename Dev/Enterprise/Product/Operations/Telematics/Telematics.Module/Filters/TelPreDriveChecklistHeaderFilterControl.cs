using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Telematics.Module.Filters
{
	public partial class TelPreDriveChecklistHeaderFilterControl : ZFilterStripControl
	{
		public TelPreDriveChecklistHeaderFilterControl(IBusinessObjectCollection gridCollection, TelPreDriveChecklistHeaderFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}
