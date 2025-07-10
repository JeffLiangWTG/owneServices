using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.Shared;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.Business
{
	public class CusPermitHeaderProcessTaskLoadStrategy : IProcessTaskLoadStrategy
	{
		public Type GetTypeForLoad(string parentTablePrefix, ZGuid parentID, BusinessObjectFactory factory)
		{
			object header = parentID.IsValid && parentTablePrefix == CusPermitHeaderSchema.Constants.Prefix ? factory.Load<Integration.Customs.ICommonCusPermitHeader>(parentID) : null;
			return MapCusPermitHeaderToProcessTaskType(header);
		}

		public void AddAdditionalParentFilters(WorkflowDescriptor workflowDescriptor, ZDBOnlySubQuery subQuery)
		{
			switch (workflowDescriptor.Code)
			{
				case WorkflowDescriptors.CusBRLPCOHeaderWorkflowDescriptorCode:
					subQuery.AddToFilter(CusPermitHeaderSchema.CPH_ApplicationCode, CusPermitHeaderApplicationCodeList.Codes.Operational);
					AddCompanyQuery(subQuery, CountryCodes.Brazil);
					break;
			}
		}

		Type MapCusPermitHeaderToProcessTaskType(object header)
		{
			Type result = null;
			if (header is Integration.Customs.BR.ICusLPCOHeader)
			{
				result = ObjectFactory.GetType<Integration.Customs.BR.ICusLPCOHeaderProcessTask>();
			}
			return result;
		}

		protected void AddCompanyQuery(ZDBOnlySubQuery subQuery, string countryCode)
		{
			var companyQuery = new ZDBOnlySubQuery(typeof(GlbCompany), CusPermitHeaderSchema.CPH_GC_Company);
			companyQuery.AddToFilter(GlbCompanySchema.GC_RN_NKCountryCode, countryCode);
			subQuery.AddSubQuery(companyQuery, JoinCondition.And);
		}
	}
}
