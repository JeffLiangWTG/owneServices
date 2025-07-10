using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.Universal.Module
{
	public partial class RefHarbourRateFilterStripControl : ZFilterStripControl
	{
		public RefHarbourRateFilterStripControl()
		{
			InitializeComponent();
		}

		public RefHarbourRateFilterStripControl(IBusinessObjectCollection gridCollection,
			FilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}
