using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Orders.Business
{
	public class ContainerLoadListLineProcessTask(BusinessObjectFactory factory, DataRow row)
		: ProcessTask(factory, row)
	{
		#region Parent

		protected override Type ParentType => typeof(ContainerLoadListLine);

		public new ContainerLoadListLine Parent => (ContainerLoadListLine)base.Parent;

		#endregion
	}
}
