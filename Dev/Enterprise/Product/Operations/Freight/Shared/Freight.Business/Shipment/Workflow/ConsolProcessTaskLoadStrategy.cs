using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business
{
	public class ConsolProcessTaskLoadStrategy : IProcessTaskLoadStrategy
	{
		public Type GetTypeForLoad(string parentTablePrefix, ZGuid parentID, BusinessObjectFactory factory)
		{
			if (parentID.IsValid)
			{
				var cfsOnlyLoadList = factory.GetBizOsForPK(parentID.ToGuid()).FirstOrDefault(IsCFSOnlyLoadList)
					?? GetCFSOnlyLoadListFromDB(parentTablePrefix, parentID, factory);

				if (cfsOnlyLoadList != null)
				{
					return ObjectFactory.GetType<Integration.CFS.ICFSLoadListConsolProcessTask>();
				}
			}

			return ObjectFactory.GetType<Enterprise.Integration.Forwarding.IForwardingConsolProcessTask>();
		}

		bool IsCFSOnlyLoadList(BusinessObject bizO)
		{
			return bizO != null
				&& bizO is Integration.CFS.ICFSLoadListConsol
				&& !(ZBool)bizO[JobConsolSchema.JK_IsForwarding];
		}

		object GetCFSOnlyLoadListFromDB(string parentTablePrefix, ZGuid parentID, BusinessObjectFactory factory)
		{
			var query = new ZQuery(JobConsolSchema.PK, parentID);
			query.AddToFilter(JobConsolSchema.JK_IsForwarding, false);
			query.AddToFilter(JobConsolSchema.JK_IsCFS, true);
			query.IgnoreActiveFilter = true;

			return factory.LoadTop1<Integration.CFS.ICFSLoadListConsol>(query);
		}

		void IProcessTaskLoadStrategy.AddAdditionalParentFilters(WorkflowDescriptor workflowDescriptor, ZDBOnlySubQuery subQuery)
		{
			var workflowProviderType = workflowDescriptor.WorkflowProviderType;

			if (typeof(Integration.CFS.ICFSLoadListConsol).IsAssignableFrom(workflowProviderType))
			{
				subQuery.AddToFilter(JobConsolSchema.JK_IsCFS, true);
				subQuery.AddToFilter(JobConsolSchema.JK_IsForwarding, false);
			}
			else
			{
				subQuery.AddToFilter(JobConsolSchema.JK_IsForwarding, true);
			}
		}
	}
}
