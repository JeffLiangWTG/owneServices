using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business
{
	[ModuleID(ModuleId.ClientRates)]
	public class RateCollection : RatingHeaderCollection
	{
		public RateCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public RateCollection(BusinessObjectFactory factory, GlbCompany company)
			: base(factory, company)
		{
		}

		public new ClientRate this[int index]
		{
			get { return (ClientRate)Elements[index]; }
		}

		public new ClientRate AddNew()
		{
			return (ClientRate)base.AddNew();
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

