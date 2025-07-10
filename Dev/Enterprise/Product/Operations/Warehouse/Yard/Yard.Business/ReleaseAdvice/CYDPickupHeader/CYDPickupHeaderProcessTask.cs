using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Yard.Business
{
	public class CYDPickupHeaderProcessTask : ProcessTask
	{
		public CYDPickupHeaderProcessTask(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override Type ParentType => typeof(CYDPickupHeader);
	}
}
