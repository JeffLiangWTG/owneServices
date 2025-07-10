using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public abstract class AccQueryClaimCollection : BusinessObjectCollection<AccQueryClaim>
	{
		public AccQueryClaimCollection(BusinessObjectFactory factory)
			: this(factory, new ZQuery())
		{
		}

		public AccQueryClaimCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		public AccQueryClaimCollection(BusinessObjectFactory factory, ZQuery filter, GlbCompany company)
			: this(factory, filter)
		{
			fCompany = company;
		}

		protected internal readonly GlbCompany fCompany;

		protected override ZQuery CreateRelationshipFilter()
		{
			ZQuery additionalFilter = base.CreateRelationshipFilter();
			ZDBOnlyQuery dbOnlyQuery = new ZDBOnlyQuery(TypeOfElements);

			AddAdditionalFilters(dbOnlyQuery);
			if (!dbOnlyQuery.IsEmpty)
			{
				additionalFilter.AddToFilter(dbOnlyQuery);
			}

			return additionalFilter;
		}

		protected abstract void AddAdditionalFilters(ZDBOnlyQuery relatedTablesQuery);

		protected static ZDBOnlySubQuery GetTransactionQuery(ZString ledger, bool notInThisCompany, ZGuid companyPK)
		{
			ZDBOnlySubQuery transactionQuery = new ZDBOnlySubQuery(typeof(AccTransactionHeader), AccQueryClaimSchema.AY_AH);
			transactionQuery.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, ledger);
			transactionQuery.AddToFilter(AccTransactionHeaderSchema.AH_GC, notInThisCompany ? SQLComparisonOperator.NotEqual : SQLComparisonOperator.Equal, companyPK);

			return transactionQuery;
		}
	}
}
