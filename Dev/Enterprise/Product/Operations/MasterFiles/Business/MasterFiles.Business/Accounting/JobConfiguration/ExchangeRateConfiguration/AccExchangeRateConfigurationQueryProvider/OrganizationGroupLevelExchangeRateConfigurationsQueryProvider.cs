using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrganizationGroupLevelExchangeRateConfigurationsQueryProvider : IAccExchangeRateConfigurationsQueryProvider
	{
		public OrganizationGroupLevelExchangeRateConfigurationsQueryProvider(ZGuid companyPK, ZGuid orgGroupPK, ZString ledger, ZQuery additionalCompanySubQuery)
		{
			this.companyPK = companyPK;
			this.orgGroupPK = orgGroupPK;
			this.ledger = ledger;
			this.additionalCompanySubQuery = additionalCompanySubQuery;
		}

		public ZQuery GetQuery()
		{
			var query = new ZQuery(AccExchangeRateConfigurationViewSchema.JCE_ParentTableCode, GetParentGroupTablePrefix(ledger));
			query.AddToFilter(AccExchangeRateConfigurationViewSchema.JCE_ParentID, orgGroupPK);

			var companySubQuery = new ZQuery();
			companySubQuery.AddToFilter(AccExchangeRateConfigurationViewSchema.JCE_GC, companyPK);
			companySubQuery.AddToFilter(additionalCompanySubQuery, JoinCondition.Or);
			query.AddToFilter(companySubQuery);

			return query;
		}

		ZString GetParentGroupTablePrefix(ZString ledger)
		{
			return ledger == LedgerTypes.AccountsReceivable ? OrgDebtorGroupSchema.Constants.Prefix : OrgCreditorGroupSchema.Constants.Prefix;
		}

		readonly ZGuid companyPK;
		readonly ZGuid orgGroupPK;
		readonly ZString ledger;
		readonly ZQuery additionalCompanySubQuery;
	}
}
