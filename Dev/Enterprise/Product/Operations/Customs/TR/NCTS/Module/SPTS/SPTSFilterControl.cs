using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TR.NCTS.Module
{
	public partial class SPTSFilterControl : ZFilterStripControl
	{
		public SPTSFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject) : base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}
