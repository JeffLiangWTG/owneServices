using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.Universal.Module
{
	partial class ZZRefCarrierFilterStripControl : ZFilterStripControl
	{
		public ZZRefCarrierFilterStripControl()
		{
			InitializeComponent();
		}

		public ZZRefCarrierFilterStripControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}
