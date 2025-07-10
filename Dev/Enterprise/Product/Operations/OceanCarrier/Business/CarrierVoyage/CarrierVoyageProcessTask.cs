using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.OceanCarrier.Business
{
	public sealed class CarrierVoyageProcessTask : ProcessTask
	{
		public CarrierVoyageProcessTask(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
		{
		}

		protected override Type ParentType => typeof(CarrierVoyage);
		public new CarrierVoyage Parent => (CarrierVoyage)base.Parent;
	}
}
