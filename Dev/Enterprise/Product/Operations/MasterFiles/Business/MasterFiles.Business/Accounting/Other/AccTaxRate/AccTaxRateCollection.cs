using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[ModuleID(ModuleId.AccTaxRate)]
	public class AccTaxRateCollection : BusinessObjectCollection<AccTaxRate>
	{
		public AccTaxRateCollection(BusinessObjectFactory factory)
			: this(factory, new ZQuery())
		{
		}

		public AccTaxRateCollection(BusinessObjectFactory factory, ZQuery filter)
			: this(factory, filter, GlbCompany.CurrentCompany)
		{
		}

		public AccTaxRateCollection(BusinessObjectFactory factory, ZQuery filter, GlbCompany company)
			: base(factory, company != null ? CombineStaticFilter(filter, company.GC_RN_NKCountryCode) : filter)
		{
			if (company != null)
			{
				countryCode = company.GC_RN_NKCountryCode;
			}
		}

		public AccTaxRateCollection(BusinessObjectFactory factory, ZString countryCode)
			: base(factory, !countryCode.IsEmpty ? CombineStaticFilter(new ZQuery(), countryCode) : new ZQuery())
		{
			this.countryCode = countryCode;
		}

		public AccTaxRateCollection(BusinessObjectFactory factory, ZQuery filter, ZString countryCode)
			: base(factory, CombineStaticFilter(filter, countryCode))
		{
			this.countryCode = countryCode;
		}

		readonly ZString countryCode;

		protected static ZQuery CombineStaticFilter(ZQuery query, ZString countryCode)
		{
			ZQuery basicFilter = new ZQuery(AccTaxRateSchema.AT_RN_NKCountry, countryCode);
			basicFilter.AddToFilter(query, JoinCondition.And);

			return basicFilter;
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			return !countryCode.IsEmpty ? CombineStaticFilter(new ZQuery(), countryCode) : new ZQuery();
		}
	}
}
