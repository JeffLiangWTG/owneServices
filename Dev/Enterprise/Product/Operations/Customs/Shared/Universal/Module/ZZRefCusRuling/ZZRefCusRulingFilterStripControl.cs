using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.Universal.Module
{
	public partial class ZZRefCusRulingFilterStripControl : ZFilterStripControl
	{
		public ZZRefCusRulingFilterStripControl()
		{
			InitializeComponent();
		}

		public ZZRefCusRulingFilterStripControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}
