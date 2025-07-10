using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.Module
{
	public partial class RefContainerFilterControl : ZFilterStripControl
	{
		public RefContainerFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
						: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}
