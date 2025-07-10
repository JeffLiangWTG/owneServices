using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.GateManagement.Business
{
	public class GteGateMovementProcessTask : ProcessTask
	{
		public GteGateMovementProcessTask(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override Type ParentType => typeof(GteGateMovement);
	}
}
