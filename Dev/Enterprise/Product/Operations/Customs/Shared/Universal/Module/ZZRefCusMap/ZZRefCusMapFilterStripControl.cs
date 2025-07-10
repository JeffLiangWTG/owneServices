using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.Universal.Module
{
	partial class ZZRefCusMapFilterStripControl : ZFilterStripControl
	{
		public ZZRefCusMapFilterStripControl()
		{
			InitializeComponent();
		}

		public ZZRefCusMapFilterStripControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}

		protected override ZFilterStrip NewZFilterStrip()
		{
			return new ZZRefCusMapFilterStrip();
		}
	}
}
