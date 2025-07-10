using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Orders.Business
{
	public class ContainerLoadPlanProcessTask : ProcessTask
	{
		public ContainerLoadPlanProcessTask(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Parent

		public override ZArchitecture.Modules.ControllerID ParentControllerID => Enterprise.ZArchitecture.Modules.ControllerIDs.ContainerLoadPlan;

		protected override Type ParentType => typeof(CFSContainerLoadList);

		public new CFSContainerLoadList Parent
		{
			get { return (CFSContainerLoadList)base.Parent; }
		}

		#endregion
	}
}
