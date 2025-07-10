using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Confirmations.Module
{
	public partial class PickupDeliveryConfirmFilterControl : ZFilterStripControl
	{
		public PickupDeliveryConfirmFilterControl()
		{
			InitializeComponent();
		}

		public PickupDeliveryConfirmFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterStripBusinessObject)
			: base(gridCollection, filterStripBusinessObject)
		{
			InitializeComponent();
		}
	}
}

