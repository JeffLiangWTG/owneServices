using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Yard.Business
{
	public class CYDYardUnitStateProcessTask : ProcessTask
	{
		public CYDYardUnitStateProcessTask(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override Type ParentType => typeof(CYDYardUnitState);

		#region ParentControllerID

		public override ControllerID ParentControllerID => ControllerIDs.CYDYardUnitState;

		#endregion
	}
}
