using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Orders.Business
{
	public class ContainerLoadListProcessTask : ProcessTask
	{
		public ContainerLoadListProcessTask(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Parent

		public override ZArchitecture.Modules.ControllerID ParentControllerID => Enterprise.ZArchitecture.Modules.ControllerIDs.ContainerLoadList;

		protected override Type ParentType => typeof(CYContainerLoadList);

		public new CYContainerLoadList Parent
		{
			get { return (CYContainerLoadList)base.Parent; }
		}

		#endregion
	}
}
