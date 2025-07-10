using System;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public class CusHAWBProcessTaskLoadStrategy : IProcessTaskLoadStrategy
	{
		public Type GetTypeForLoad(string parentTablePrefix, ZGuid parentID, BusinessObjectFactory factory)
		{
			object hawb = parentID.IsValid && parentTablePrefix == CusHAWBSchema.Constants.Prefix ? factory.Load<CusHAWB>(parentID) : null;
			return MapCusHAWBToProcessTaskType(hawb);
		}

		public void AddAdditionalParentFilters(WorkflowDescriptor workflowDescriptor, ZDBOnlySubQuery subQuery)
		{
		}

		Type MapCusHAWBToProcessTaskType(object hawb)
		{
			if (hawb != null)
			{
				if (hawb is Integration.Customs.AU.ICusHAWB || hawb is Integration.Customs.AU.ICTOCusHAWB)
				{
					return ObjectFactory.GetType<Integration.Customs.AU.ICusHAWBProcessTask>();
				}
				else if (hawb is Integration.Customs.NZ.ICusHAWB)
				{
					return ObjectFactory.GetType<Integration.Customs.NZ.ICusHAWBProcessTask>();
				}
				else if (hawb is Integration.Customs.GB.CCSUK.ICusHAWB)
				{
					return ObjectFactory.GetType<Integration.Customs.GB.CCSUK.ICusHAWBProcessTask>();
				}
				else
				{
					ErrorReporter.ReportOnce("CUS-HAWB-ProcessTaskLoadUnknownCountry", "CusHAWBProcessTaskLoadStrategy cannot load a CusHAWBProcessTask for this HAWB as the country is not recognised. Add a case for your country");
					return typeof(CusHAWBProcessTask);
				}
			}
			return null;
		}
	}
}
