using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
namespace Enterprise.Customs.US.Module
{
	public partial class CourtesyNoticesOfLiquidationFilterControl : ZFilterStripControl
	{
		public CourtesyNoticesOfLiquidationFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterStripBusinessObject)
			: base(gridCollection, filterStripBusinessObject)
		{
			InitializeComponent();
		}
	}
}
