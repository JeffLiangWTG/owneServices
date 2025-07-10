using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Confirmations.Module
{
	public partial class ConsolidatedTransportBookingFilterControl : ZFilterStripControl
	{
		public ConsolidatedTransportBookingFilterControl()
		{
			InitializeComponent();
		}

		public ConsolidatedTransportBookingFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterStripBusinessObject)
			: base(gridCollection, filterStripBusinessObject)
		{
			InitializeComponent();
		}
	}
}

