using System;
using System.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.ProcessManagement.Business
{
	[ImmutableObject(true)]
	public class WorkItemTypeDecider : TypeDecider
	{
		public override Type GetTypeForBinding()
		{
			return typeof(WorkItemCommon);
		}

		public override Type GetTypeForLoad(System.Data.DataRow row, BusinessObjectFactory factory)
		{
			return typeof(WorkItem);
		}

		public override Type GetTypeForNew()
		{
			throw new NotSupportedException("Abstract type.");
		}
	}
}



