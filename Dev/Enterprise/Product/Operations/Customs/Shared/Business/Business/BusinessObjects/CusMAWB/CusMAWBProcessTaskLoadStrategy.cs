using System;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public class CusMAWBProcessTaskLoadStrategy : IProcessTaskLoadStrategy
	{
		public Type GetTypeForLoad(string parentTablePrefix, ZGuid parentID, BusinessObjectFactory factory)
		{
			object mawb = parentID.IsValid && parentTablePrefix == CusMAWBSchema.Constants.Prefix ? factory.Load<CusMAWB>(parentID) : null;
			return MapCusMAWBToProcessTaskType(mawb);
		}

		public void AddAdditionalParentFilters(WorkflowDescriptor workflowDescriptor, ZDBOnlySubQuery subQuery)
		{
		}

		Type MapCusMAWBToProcessTaskType(object mawb)
		{
			Type result = null;
			if (mawb != null)
			{
				if (mawb is Integration.Customs.AU.ICusMAWB)
				{
					result = ObjectFactory.GetType<Integration.Customs.AU.ICusMAWBProcessTask>();
				}
				else if (mawb is Integration.Customs.NZ.ICusMAWB)
				{
					result = ObjectFactory.GetType<Integration.Customs.NZ.ICusMAWBProcessTask>();
				}
				else if (mawb is Integration.Customs.GB.CCSUK.ICusMAWB)
				{
					result = ObjectFactory.GetType<Integration.Customs.GB.CCSUK.ICusMAWBProcessTask>();
				}
				else
				{
					ErrorReporter.ReportOnce("CUS-MAWB-ProcessTaskLoadUnknownCountry", "CusMAWBProcessTaskLoadStrategy cannot load a CusMAWBProcessTask for this MAWB as the country is not recognised. Add a case for your country");
					result = typeof(CusMAWBProcessTask);
				}
			}
			return result;
		}
	}
}
