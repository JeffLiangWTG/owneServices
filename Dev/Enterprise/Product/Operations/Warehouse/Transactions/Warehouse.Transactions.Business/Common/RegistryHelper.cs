using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;

namespace Enterprise.Warehouse.Transactions.Business
{
	public static class RegistryHelper
	{
		public static GlbBranch GetBillingOperationsBranch(WhsWarehouse warehouse, OrgHeader organisation)
		{
			GlbBranch result = null;

			var orderRulesForDefaulting = AccountingConfigurationRegistry.Instance.JobBranchDefaultOrderRule.Value;

			for (int i = 1; i <= 4 && (result == null || !result.GB_IsActive); i++)
			{
				if (i == orderRulesForDefaulting.DefaultToBlank)
				{
					result = null;
					break;
				}
				else if (i == orderRulesForDefaulting.DefaultToBranchRelatedToPortOrWarehouseBranch)
				{
					result = (warehouse != null) ? warehouse.RelatedCompanyBranch : null;
				}
				else if (i == orderRulesForDefaulting.DefaultToBranchOfOrganisation)
				{
					result = (organisation != null) ? organisation.CompanyData.ControllingBranch : null;
				}
				else if (i == orderRulesForDefaulting.DefaultToLoginUserDefault)
				{
					result = GlbBranch.CurrentBranch;
				}
			}

			return result;
		}
	}
}
