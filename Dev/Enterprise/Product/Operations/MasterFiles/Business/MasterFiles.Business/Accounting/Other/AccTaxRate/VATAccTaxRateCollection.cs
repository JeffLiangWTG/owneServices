using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class VATAccTaxRateCollection : AccTaxRateCollection
	{
		public VATAccTaxRateCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public VATAccTaxRateCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		public VATAccTaxRateCollection(BusinessObjectFactory factory, ZQuery filter, GlbCompany company)
			: base(factory, filter, company)
		{
		}

		public VATAccTaxRateCollection(BusinessObjectFactory factory, ZString countryCode)
			: base(factory, countryCode)
		{
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			var filter = base.CreateRelationshipFilter();
			filter.AddToFilter(JoinCondition.And, AccTaxRateSchema.AT_TaxSystemCode, "");
			return filter;
		}
	}
}
