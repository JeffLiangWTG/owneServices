using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public class CusOutturnHeaderProcessTaskLoadStrategy : IProcessTaskLoadStrategy
	{
		public Type GetTypeForLoad(string parentTablePrefix, ZGuid parentID, BusinessObjectFactory factory)
		{
			object outturnHeader = parentID.IsValid && parentTablePrefix == CusOutturnHeaderSchema.Constants.Prefix ? factory.Load<CusOutturnHeader>(parentID) : null;
			return MapCusOutturnHeaderToProcessTaskType(outturnHeader);
		}

		public void AddAdditionalParentFilters(WorkflowDescriptor workflowDescriptor, ZDBOnlySubQuery subQuery)
		{
		}

		Type MapCusOutturnHeaderToProcessTaskType(object outturnHeader)
		{
			Type result = null;

			if (outturnHeader != null)
			{
				result = ObjectFactory.GetType<Integration.Customs.AU.ICusOutturnHeaderProcessTask>();
			}

			return result;
		}
	}
}
