using CargoWise.EntityFramework;
using Enterprise.Freight.Module;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.Orders.Module
{
	public partial class JobShipmentPreplanningFilterControl : ZFilterStripControl
	{
		public JobShipmentPreplanningFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}

		protected override ZFilterStrip NewZFilterStrip()
		{
			return new WorkflowFilterStripWithRoutingSupport();
		}
	}
}
