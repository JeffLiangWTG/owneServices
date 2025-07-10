using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Telematics.Module.Filters
{
	public partial class TelPreDriveChecklistTemplateHeaderFilterControl : ZFilterStripControl
	{
		public TelPreDriveChecklistTemplateHeaderFilterControl(IBusinessObjectCollection gridCollection, TelPreDriveChecklistTemplateHeaderFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}
