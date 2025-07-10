using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ContractManagement.Module
{
	public partial class AllocationRouteFilterControl : ZFilterStripControl<AllocationRouteModuleStrip>
	{
		public AllocationRouteFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
		: base(gridCollection, filterBusinessObject)
		{
			if (!DesignModeFinder.IsDesigning)
			{
				InitializeComponent();
			}
		}
	}
}
