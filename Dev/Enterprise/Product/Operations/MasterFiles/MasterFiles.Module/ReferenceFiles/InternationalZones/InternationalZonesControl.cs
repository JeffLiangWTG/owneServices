using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.Module
{
	public partial class InternationalZonesControl : ZFilterStripControl
	{
		public InternationalZonesControl(IBusinessObjectCollection gridCollection, InternationalZonesFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}
