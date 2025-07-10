using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.Module
{
	public partial class RefContainerISOTypesFilterControl : ZFilterStripControl
	{
		public RefContainerISOTypesFilterControl()
		{
			InitializeComponent();
		}

		public RefContainerISOTypesFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
						: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}
