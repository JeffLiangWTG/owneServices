using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.OceanCarrier.Business
{
	public sealed class CarrierVoyagePortCallProcessTask : ProcessTask
	{
		public CarrierVoyagePortCallProcessTask(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
		{
		}

		protected override Type ParentType => typeof(CarrierVoyagePortCall);
		public new CarrierVoyagePortCall Parent => (CarrierVoyagePortCall)base.Parent;
	}
}
