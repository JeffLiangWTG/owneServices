using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgInvTypeDeferredChargesCollection : DependentBusinessObjectCollection<OrgInvTypeDeferredCharges, OrgInvoiceType>
	{
		public OrgInvTypeDeferredChargesCollection(OrgInvoiceType parent)
			: base(parent)
		{
			this.Parent = parent;
		}

		public OrgInvTypeDeferredChargesCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		readonly OrgInvoiceType Parent;

		protected override void OnRemoved(BusinessObject bizO)
		{
			base.OnRemoved(bizO);
			if (this.Count == 0 && Parent.PI_Calc_IsInclude != InvoiceTypeChargeInclusionTypeList.Codes.ALL)
			{
				Parent.PI_Calc_IsInclude = InvoiceTypeChargeInclusionTypeList.Codes.ALL;
			}
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			ZQuery filter = base.CreateRelationshipFilter();

			var chargeQuery = new ZDBOnlySubQuery(typeof(AccChargeCode), OrgInvTypeDeferredChargesSchema.PO_AC);
			chargeQuery.AddToFilter(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK);

			var additionalFilter = new ZDBOnlyQuery(typeof(OrgInvTypeDeferredCharges));
			additionalFilter.AddSubQuery(chargeQuery, JoinCondition.And);
			additionalFilter.AddToFilter(new ZQuery(OrgInvTypeDeferredChargesSchema.PO_AC, null), JoinCondition.Or);
			filter.AddToFilter(additionalFilter);

			return filter;
		}
	}
}
