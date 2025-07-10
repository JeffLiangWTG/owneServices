using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public class StatementProcessTaskLoadStrategy : IProcessTaskLoadStrategy
	{
		public Type GetTypeForLoad(string parentTablePrefix, ZGuid parentID, BusinessObjectFactory factory)
		{
			var job = parentID.IsValid && parentTablePrefix == CusStatementHeaderSchema.Constants.Prefix ? factory.Load<BaseCusStatementHeader>(parentID) : null;

			return job?.ProcessTaskType;
		}

		public void AddAdditionalParentFilters(WorkflowDescriptor workflowDescriptor, ZDBOnlySubQuery subQuery)
		{
			if (workflowDescriptor.Code == WorkflowDescriptors.CADailyNotice)
			{
				var countryQuery = new ZDBOnlySubQuery(typeof(GlbCompany), GlbCompanySchema.PK);
				countryQuery.AddToFilter(GlbCompanySchema.GC_RN_NKCountryCode, Core.Constants.CountryCodes.Canada);
				subQuery.AddSubQuery(countryQuery, JoinCondition.And);
			}
		}
	}
}
