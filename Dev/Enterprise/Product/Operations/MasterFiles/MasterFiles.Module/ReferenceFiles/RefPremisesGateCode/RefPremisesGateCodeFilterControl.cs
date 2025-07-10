using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.Module
{
	public partial class RefPremisesGateCodeFilterControl : ZFilterStripControl
	{
		public RefPremisesGateCodeFilterControl()
		{
			InitializeComponent();
		}

		public RefPremisesGateCodeFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}
