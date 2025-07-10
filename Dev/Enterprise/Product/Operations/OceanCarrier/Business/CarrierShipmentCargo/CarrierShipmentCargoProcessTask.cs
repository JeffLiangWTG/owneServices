using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.OceanCarrier.Business
{
	public sealed class CarrierShipmentCargoProcessTask : ProcessTask
	{
		public CarrierShipmentCargoProcessTask(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
		{
		}

		protected override Type ParentType => typeof(CarrierShipmentCargo);
		public new CarrierShipmentCargo Parent => (CarrierShipmentCargo)base.Parent;
	}
}
