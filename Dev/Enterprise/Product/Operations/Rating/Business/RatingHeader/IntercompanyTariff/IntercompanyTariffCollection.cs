using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business
{
	public class IntercompanyTariffCollection : RatingHeaderCollection
	{
		public IntercompanyTariffCollection(BusinessObjectFactory factory)
			: base(factory)
		{ }

		public new IntercompanyTariff this[int index] =>
			(IntercompanyTariff)Elements[index];

		public new IntercompanyTariff AddNew() =>
			(IntercompanyTariff)base.AddNew();

		protected override IFindBoxListProvider FindBoxListProvider =>
			new RatingHeaderFindBoxListProviderForOrganisationCode(this, RelationshipFilter);

		protected override ZQuery GetCurrentCompanyFilter() =>
			new ZQuery(RatingHeaderSchema.TH_GC, null);
	}
}
