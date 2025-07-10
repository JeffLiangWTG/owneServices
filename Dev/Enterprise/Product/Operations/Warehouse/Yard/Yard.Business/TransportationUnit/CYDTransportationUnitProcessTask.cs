using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Yard.Business
{
	public class CYDTransportationUnitProcessTask : ProcessTask
	{
		public CYDTransportationUnitProcessTask(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override Type ParentType => typeof(CYDTransportationUnit);

		#region ParentControllerID

		public override ControllerID ParentControllerID => ControllerIDs.CYDTransportationUnit;

		#endregion
	}
}
