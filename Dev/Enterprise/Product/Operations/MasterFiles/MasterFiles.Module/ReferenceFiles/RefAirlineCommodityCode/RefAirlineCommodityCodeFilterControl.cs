using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.Module
{
	public partial class RefAirlineCommodityCodeFilterControl : ZFilterStripControl
	{
		public RefAirlineCommodityCodeFilterControl(IBusinessObjectCollection gridCollection, RefAirlineCommodityCodeFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}

		protected override ZFilterStrip NewZFilterStrip()
		{
			return new ZFilterStrip();
		}
	}
}
