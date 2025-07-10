using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CargoReportWorkflowHelper : CargoReportWorkflow
	{
		public CargoReportWorkflowHelper(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public bool ForceConcurrencyError;

		protected override ZDateTime ScheduledCargoReportDate(ForwardingShipment shipment)
		{
			if (ForceConcurrencyError)
			{
				BusinessObjectFactory anotherFactory = new BusinessObjectFactory();
				anotherFactory.RefreshEnabled = false;
				ForwardingShipment shipmentInAnotherFactory = anotherFactory.Load<ForwardingShipment>(shipment.PK);
				shipmentInAnotherFactory.JS_ActualWeight = 1m;
				anotherFactory.Save();
				shipment.JS_ActualWeight = 2m;
				ForceConcurrencyError = false;
			}
			return base.ScheduledCargoReportDate(shipment);
		}
	}
}
