using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.eTail.Module
{
	public partial class HVLVOuterPackageFilterControl : ZFilterStripControl
	{
		public HVLVOuterPackageFilterControl(IBusinessObjectCollection gridCollection, HVLVOuterPackageFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}
