using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business
{
	public class CompanyTariffCollection : RatingHeaderCollection
	{
		public CompanyTariffCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public CompanyTariffCollection(BusinessObjectFactory factory, GlbCompany company)
			: base(factory, company)
		{
		}

		public new CompanyTariff this[int index]
		{
			get { return (CompanyTariff)Elements[index]; }
		}

		public new CompanyTariff AddNew()
		{
			return (CompanyTariff)base.AddNew();
		}

		protected override IFindBoxListProvider FindBoxListProvider
		{
			get { return new RatingHeaderFindBoxListProviderForRateLevel(this, RelationshipFilter); }
		}

		protected override ZQuery GetCurrentCompanyFilter()
		{
			var companyFilter = base.GetCurrentCompanyFilter();
			companyFilter.AddToFilter(JoinCondition.Or, RatingHeaderSchema.TH_GC, null);

			return companyFilter;
		}
	}
}

