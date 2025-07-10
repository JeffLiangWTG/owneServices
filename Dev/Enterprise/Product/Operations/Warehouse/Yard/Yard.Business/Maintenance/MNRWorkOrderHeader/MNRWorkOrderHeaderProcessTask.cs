using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Yard.Business
{
	public class MNRWorkOrderHeaderProcessTask : ProcessTask
	{
		public MNRWorkOrderHeaderProcessTask(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}
		protected override Type ParentType => typeof(MNRWorkOrderHeader);

		#region ParentControllerID

		public override ControllerID ParentControllerID => ControllerIDs.MNRWorkOrder;

		#endregion
	}
}
