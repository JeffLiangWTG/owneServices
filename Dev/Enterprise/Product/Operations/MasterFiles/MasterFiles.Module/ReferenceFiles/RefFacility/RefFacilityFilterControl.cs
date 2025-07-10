using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.Module
{
	public partial class RefFacilityFilterControl : ZFilterStripControl
	{
		public RefFacilityFilterControl()
		{
			InitializeComponent();
		}
		public RefFacilityFilterControl(IBusinessObjectCollection gridCollection, RefFacilityFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}
