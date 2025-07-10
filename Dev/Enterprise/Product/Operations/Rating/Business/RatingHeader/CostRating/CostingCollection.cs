using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business
{
	public class CostingCollection : RatingHeaderCollection
	{
		public CostingCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public CostingCollection(BusinessObjectFactory factory, GlbCompany company)
			: base(factory, company)
		{
		}

		public new Costing this[int index]
		{
			get { return (Costing)Elements[index]; }
		}

		public new Costing AddNew()
		{
			return (Costing)base.AddNew();
		}

		protected override IFindBoxListProvider FindBoxListProvider
		{
			get { return new RatingHeaderFindBoxListProviderForOrganisationCode(this, RelationshipFilter); }
		}

		protected override ZQuery GetCurrentCompanyFilter()
		{
			var companyFilter = base.GetCurrentCompanyFilter();
			companyFilter.AddToFilter(JoinCondition.Or, RatingHeaderSchema.TH_GC, null);

			return companyFilter;
		}
	}
}

