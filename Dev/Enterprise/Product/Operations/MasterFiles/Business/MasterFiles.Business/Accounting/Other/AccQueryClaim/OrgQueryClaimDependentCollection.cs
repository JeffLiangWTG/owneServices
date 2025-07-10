using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public abstract class OrgQueryClaimDependentCollection : DependentBusinessObjectCollection<AccQueryClaim, OrgHeader>
	{
		public OrgQueryClaimDependentCollection(OrgHeader parent, BusinessObjectFactory factory)
			: base(parent, factory)
		{
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			ZQuery additionalFilter = base.CreateRelationshipFilter();
			ZDBOnlyQuery dBOnlyQuery = new ZDBOnlyQuery(typeof(AccQueryClaim));
			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(GlbBranch), GlbBranchSchema.PK);
			subQuery.AddToFilter(GlbBranchSchema.GB_GC, GlbCompany.CurrentCompany.PK);
			dBOnlyQuery.AddSubQuery(AccQueryClaimSchema.AY_GB, subQuery, JoinCondition.And);
			ZDBOnlyQuery ledgerDbOnlyQuery = new ZDBOnlyQuery(typeof(AccQueryClaim));
			AddLedgerFilter(ledgerDbOnlyQuery);
			additionalFilter.AddToFilter(dBOnlyQuery);
			additionalFilter.AddToFilter(ledgerDbOnlyQuery);
			return additionalFilter;
		}

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			((AccQueryClaim)bizOAdded).SetDebtorReadOnly(true);
			base.OnAdded(bizOAdded);
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			((AccQueryClaim)child).SetDebtorReadOnly(true);
		}

		protected override bool AllowNewCore
		{
			get { return true; }
		}

		protected override SchemaGuidColumn FKSchemaColumnInDependent
		{
			get { return AccQueryClaimSchema.AY_OH_Debtor; }
		}

		protected abstract void AddLedgerFilter(ZDBOnlyQuery relatedTablesQuery);
	}
}
