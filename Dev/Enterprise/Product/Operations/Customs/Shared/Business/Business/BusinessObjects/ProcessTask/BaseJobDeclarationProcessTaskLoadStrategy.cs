using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.Business
{
	public class BaseJobDeclarationProcessTaskLoadStrategy : IProcessTaskLoadStrategy
	{
		public Type GetTypeForLoad(string parentTablePrefix, ZGuid parentID, BusinessObjectFactory factory)
		{
			object jobDeclaration = parentID.IsValid && parentTablePrefix == JobDeclarationSchema.Constants.Prefix ? factory.Load<IBaseJobDeclaration>(parentID) : null;
			return MapJobDeclarationToProcessTaskType(jobDeclaration);
		}

		public void AddAdditionalParentFilters(WorkflowDescriptor workflowDescriptor, ZDBOnlySubQuery subQuery)
		{
			switch (workflowDescriptor.Code)
			{
				case WorkflowDescriptors.DrawBackWorkflowDescriptorCode:
					subQuery.AddToFilter(JobDeclarationSchema.JE_MessageType, Enterprise.Customs.Common.US.USJobMessageTypeList.Codes.Drawback);
					AddCompanyQuery(subQuery, CountryCodes.UnitedStates);
					break;
				case WorkflowDescriptors.ProtestWorkflowDescriptorCode:
					subQuery.AddToFilter(JobDeclarationSchema.JE_MessageType, Enterprise.Customs.Common.US.USJobMessageTypeList.MoreCodes.Protest);
					AddCompanyQuery(subQuery, CountryCodes.UnitedStates);
					break;
				case WorkflowDescriptors.ReconWorkflowDescriptorCode:
					subQuery.AddToFilter(JobDeclarationSchema.JE_MessageType, Enterprise.Customs.Common.US.USJobMessageTypeList.Codes.Recon);
					AddCompanyQuery(subQuery, CountryCodes.UnitedStates);
					break;
			}
		}

		Type MapJobDeclarationToProcessTaskType(object jobDeclaration)
		{
			Type result = null;
			if (jobDeclaration is IBaseJobDeclaration)
			{
				result = typeof(BaseJobDeclarationProcessTask<>).MakeGenericType(jobDeclaration.GetType());
			}
			return result;
		}

		static void AddCompanyQuery(ZDBOnlySubQuery subQuery, string countryCode)
		{
			var branchQuery = new ZDBOnlySubQuery(typeof(GlbBranch), JobDeclarationSchema.JE_GB);
			var companyQuery = new ZDBOnlySubQuery(typeof(GlbCompany), GlbBranchSchema.GB_GC);
			companyQuery.AddToFilter(GlbCompanySchema.GC_RN_NKCountryCode, countryCode);
			branchQuery.AddSubQuery(companyQuery, JoinCondition.And);
			subQuery.AddSubQuery(branchQuery, JoinCondition.And);
		}
	}
}
