using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.Module
{
	public partial class RefOrgPartCategoryFilterControl : ZFilterStripControl
	{
		public RefOrgPartCategoryFilterControl()
		{
			InitializeComponent();
		}

		public RefOrgPartCategoryFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}
