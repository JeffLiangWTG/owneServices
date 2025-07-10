using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public class CusSCAOceanBillProcessTaskLoadStrategy : IProcessTaskLoadStrategy
	{
		public Type GetTypeForLoad(string parentTablePrefix, ZGuid parentID, BusinessObjectFactory factory)
		{
			object oceanBill = parentID.IsValid && parentTablePrefix == CusSCAOceanBillSchema.Constants.Prefix ? factory.Load<Integration.Customs.Shared.IBaseCusSCAOceanBill>(parentID) : null;
			return MapCusSCAOceanBillToProcessTaskType(oceanBill);
		}

		public void AddAdditionalParentFilters(WorkflowDescriptor workflowDescriptor, ZDBOnlySubQuery subQuery)
		{
			switch (workflowDescriptor.Code)
			{
				case WorkflowDescriptors.CusSCAOceanBillDescriptorCode:
					subQuery.AddToFilter(CusSCAOceanBillSchema.CB_ApplicationCode, Core.Constants.Customs.CusSCAOceanBillApplicationCodes.CanadaACISea);
					break;
			}
		}

		Type MapCusSCAOceanBillToProcessTaskType(object oceanBill)
		{
			Type result = null;
			if (oceanBill != null)
			{
				if (oceanBill is Integration.Customs.AU.ICusSCAOceanBill)
				{
					result = ObjectFactory.GetType<Integration.Customs.AU.ICusSCAOceanBillProcessTask>();
				}
				else if (oceanBill is Integration.Customs.CA.ICusSCAOceanBill)
				{
					result = ObjectFactory.GetType<Integration.Customs.CA.ICusSCAOceanBillProcessTask>();
				}
				else if (oceanBill is Integration.Customs.NZ.ICusSCAOceanBill)
				{
					result = ObjectFactory.GetType<Integration.Customs.NZ.ICusSCAOceanBillProcessTask>();
				}
			}
			return result;
		}
	}
}
