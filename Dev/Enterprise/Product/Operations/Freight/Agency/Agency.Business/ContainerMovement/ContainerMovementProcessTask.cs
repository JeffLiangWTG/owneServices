using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Agency.Business
{
	public class ContainerMovementProcessTask : ProcessTask
	{
		public ContainerMovementProcessTask(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override Type ParentType
		{
			get { return typeof(ContainerMovement); }
		}

		public new ContainerMovement Parent
		{
			get { return (ContainerMovement)base.Parent; }
		}

		public override ControllerID ParentControllerID
		{
			get { return ControllerIDs.AgencyContainerManager; }
		}
	}
}


