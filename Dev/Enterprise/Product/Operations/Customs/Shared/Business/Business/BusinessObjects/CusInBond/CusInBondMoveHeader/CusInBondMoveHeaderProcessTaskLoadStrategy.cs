using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public class CusInBondMoveHeaderProcessTaskLoadStrategy : IProcessTaskLoadStrategy
	{
		public void AddAdditionalParentFilters(WorkflowDescriptor workflowDescriptor, ZDBOnlySubQuery subQuery)
		{
			switch (workflowDescriptor.Code)
			{
				case WorkflowDescriptors.NctsDepartureMovementHeaderWorkflowDescriptor:
					subQuery.AddToFilter(CusInBondMoveHeaderSchema.BM_SubApplicationCode, Enterprise.Customs.Common.EU.NctsMoveHeaderType.Codes.Departure);
					break;
				case WorkflowDescriptors.NctsArrivalMovementHeaderWorkflowDescriptor:
					subQuery.AddToFilter(CusInBondMoveHeaderSchema.BM_SubApplicationCode, Enterprise.Customs.Common.EU.NctsMoveHeaderType.Codes.Arrival);
					break;
			}
		}

		public Type GetTypeForLoad(string parentTablePrefix, ZGuid parentID, BusinessObjectFactory factory)
		{
			object header = parentID.IsValid && parentTablePrefix == CusInBondMoveHeaderSchema.Constants.Prefix ? factory.Load<Integration.Customs.ICusInBondMoveHeader>(parentID) : null;
			return MapCusInBondHeaderToProcessTaskType(header);
		}

		Type MapCusInBondHeaderToProcessTaskType(object header)
		{
			Type result = null;
			if (header != null)
			{
				if (header is Integration.Customs.EU.NCTS.IArrivalMovementHeader)
				{
					result = ObjectFactory.GetType<Integration.Customs.EU.NCTS.IArrivalMovementHeaderProcessTask>();
				}
				else if (header is Integration.Customs.EU.NCTS.IDepartureMovementHeader)
				{
					result = ObjectFactory.GetType<Integration.Customs.EU.NCTS.IDepartureMovementHeaderProcessTask>();
				}
			}
			return result;
		}
	}
}
