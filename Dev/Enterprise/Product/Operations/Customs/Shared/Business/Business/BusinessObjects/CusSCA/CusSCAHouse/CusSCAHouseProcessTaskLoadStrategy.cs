using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public class CusSCAHouseProcessTaskLoadStrategy : IProcessTaskLoadStrategy
	{
		public void AddAdditionalParentFilters(WorkflowDescriptor workflowDescriptor, ZDBOnlySubQuery subQuery)
		{
		}

		public Type GetTypeForLoad(string parentTablePrefix, ZGuid parentID, BusinessObjectFactory factory)
		{
			object house = parentID.IsValid && parentTablePrefix == CusSCAHouseSchema.Constants.Prefix ? factory.Load<Integration.Customs.Shared.IBaseCusSCAHouse>(parentID) : null;
			return MapCusSCAHouseToProcessTaskType(house);
		}

		Type MapCusSCAHouseToProcessTaskType(object house)
		{
			if (house != null)
			{
				if (house is Integration.Customs.AU.ICusSCAHouse)
				{
					return ObjectFactory.GetType<Integration.Customs.AU.ICusSCAHouseProcessTask>();
				}
				else
				{
					return typeof(CusSCAHouseProcessTask);
				}
			}
			return null;
		}
	}
}
