using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business;

sealed class CusEntryHeaderProcessTaskLoadStrategy : IProcessTaskLoadStrategy
{
	Type IProcessTaskLoadStrategy.GetTypeForLoad(string parentTablePrefix, ZGuid parentID, BusinessObjectFactory factory)
	{
		object entryHeader = parentID.IsValid && parentTablePrefix == CusEntryHeaderSchema.Constants.Prefix ? factory.Load<CusEntryHeader>(parentID) : null;
		return MapEntryHeaderToProcessorType(entryHeader);
	}

	void IProcessTaskLoadStrategy.AddAdditionalParentFilters(WorkflowDescriptor workflowDescriptor, ZDBOnlySubQuery subQuery)
	{
		if (workflowDescriptor is { Code: WorkflowDescriptors.CusNOEmmaMessageGeneratorWorkflowDescriptorCode })
		{
			AddCompanyQuery(subQuery, Constants.CountryCodes.Norway);
		}
	}

	static Type MapEntryHeaderToProcessorType(object entryHeader)
	{
		if (entryHeader is Integration.Customs.ICusEntryHeader cusEntryHeader)
		{
			return typeof(CusEntryHeaderProcessTask<>).MakeGenericType(cusEntryHeader.GetType());
		}

		return null;
	}

	static void AddCompanyQuery(ZDBOnlySubQuery subQuery, string countryCode)
	{
		var declarationQuery = new ZDBOnlySubQuery(typeof(BaseJobDeclaration), CusEntryHeaderSchema.CH_JE);
		var branchQuery = new ZDBOnlySubQuery(typeof(GlbBranch), JobDeclarationSchema.JE_GB);
		var companyQuery = new ZDBOnlySubQuery(typeof(GlbCompany), GlbBranchSchema.GB_GC);
		companyQuery.AddToFilter(GlbCompanySchema.GC_RN_NKCountryCode, countryCode);
		branchQuery.AddSubQuery(companyQuery, JoinCondition.And);
		declarationQuery.AddSubQuery(branchQuery, JoinCondition.And);
		subQuery.AddSubQuery(declarationQuery, JoinCondition.And);
	}
}
