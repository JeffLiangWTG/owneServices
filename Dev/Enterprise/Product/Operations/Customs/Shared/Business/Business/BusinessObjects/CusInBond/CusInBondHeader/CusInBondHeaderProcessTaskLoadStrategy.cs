using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.Business
{
	public class CusInBondHeaderProcessTaskLoadStrategy : IProcessTaskLoadStrategy
	{
		public Type GetTypeForLoad(string parentTablePrefix, ZGuid parentID, BusinessObjectFactory factory)
		{
			object header = parentID.IsValid && parentTablePrefix == CusInBondHeaderSchema.Constants.Prefix ? factory.Load<Integration.Customs.ICusInBondHeader>(parentID) : null;
			return MapCusInBondHeaderToProcessTaskType(header);
		}

		public void AddAdditionalParentFilters(WorkflowDescriptor workflowDescriptor, ZDBOnlySubQuery subQuery)
		{
			switch (workflowDescriptor.Code)
			{
				case WorkflowDescriptors.USAMSWorkflowDescriptorCode:
					subQuery.AddToFilter(CusInBondHeaderSchema.BH_ApplicationCode, CusInBondApplicationCodeList.Codes.AMS);
					break;
				case WorkflowDescriptors.eManifestWorkflowDescriptorCode:
					subQuery.AddToFilter(CusInBondHeaderSchema.BH_ApplicationCode, CusInBondApplicationCodeList.Codes.eManifest);
					AddCompanyQuery(subQuery, CountryCodes.UnitedStates);
					break;
				case WorkflowDescriptors.CusInBondHeaderWorkflowDescriptorCode:
					subQuery.AddToFilter(CusInBondHeaderSchema.BH_ApplicationCode, CusInBondApplicationCodeList.Codes.InBond);
					AddCompanyQuery(subQuery, CountryCodes.UnitedStates);
					break;
				case WorkflowDescriptors.NctsHeaderWorkflowDescriptorCode:
					subQuery.AddToFilter(CusInBondHeaderSchema.BH_ApplicationCode, CusInBondApplicationCodeList.Codes.NCTS4);
					break;
			}
		}

		Type MapCusInBondHeaderToProcessTaskType(object header)
		{
			Type result = null;
			if (header != null)
			{
				if (header is Integration.Customs.US.eManifest.ICusInBondHeader)
				{
					result = ObjectFactory.GetType<Integration.Customs.US.eManifest.ICusInBondHeaderProcessTask>();
				}
				else if (header is Integration.Customs.US.InBond.ICusInBondHeader)
				{
					result = ObjectFactory.GetType<Integration.Customs.US.InBond.ICusInBondHeaderProcessTask>();
				}
				else if (header is Integration.Customs.EU.NCTS.ICusInBondHeader)
				{
					result = ObjectFactory.GetType<Integration.Customs.EU.NCTS.INctsHeaderProcessTask>();
				}
				else if (header is Integration.Customs.US.USAMS.ICusInBondHeader)
				{
					result = ObjectFactory.GetType<Integration.Customs.US.USAMS.ICusInBondHeaderProcessTask>();
				}
			}
			return result;
		}

		protected void AddCompanyQuery(ZDBOnlySubQuery subQuery, string countryCode)
		{
			var branchQuery = new ZDBOnlySubQuery(typeof(GlbBranch), CusInBondHeaderSchema.BH_GB);
			var companyQuery = new ZDBOnlySubQuery(typeof(GlbCompany), GlbBranchSchema.GB_GC);
			companyQuery.AddToFilter(GlbCompanySchema.GC_RN_NKCountryCode, countryCode);
			branchQuery.AddSubQuery(companyQuery, JoinCondition.And);
			subQuery.AddSubQuery(branchQuery, JoinCondition.And);
		}
	}
}
