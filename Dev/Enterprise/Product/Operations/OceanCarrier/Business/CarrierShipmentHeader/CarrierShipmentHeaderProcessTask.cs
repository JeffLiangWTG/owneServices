using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.OceanCarrier.Business
{
	public sealed class CarrierShipmentHeaderProcessTask : ProcessTask
	{
		public CarrierShipmentHeaderProcessTask(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
		{
		}

		protected override Type ParentType => typeof(CarrierShipmentHeader);
		public override ControllerID ParentControllerID => ControllerIDs.CarrierShipmentHeader;
	}
}
