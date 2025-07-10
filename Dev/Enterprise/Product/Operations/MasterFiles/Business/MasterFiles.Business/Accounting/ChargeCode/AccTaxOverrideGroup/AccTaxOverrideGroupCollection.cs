using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[ModuleID(ModuleId.AccTaxOverrideGroup)]
	public class AccTaxOverrideGroupCollection : BusinessObjectCollection<AccTaxOverrideGroup>
	{
		public AccTaxOverrideGroupCollection(BusinessObjectFactory factory)
			: this(factory, GlbCompany.CurrentCompany)
		{
		}

		public AccTaxOverrideGroupCollection(BusinessObjectFactory factory, GlbCompany company)
			: this(factory, company, null)
		{
		}

		public AccTaxOverrideGroupCollection(BusinessObjectFactory factory, GlbCompany company, ZQuery filter)
			: base(factory)
		{
			Argument.NotNull(company, "company");

			this.company = company;
			this.extraFilter = filter;
		}

		readonly GlbCompany company;
		readonly ZQuery extraFilter;

		protected override ZQuery CreateRelationshipFilter()
		{
			var filter = new ZQuery(AccTaxOverrideGroupSchema.AX_RN_NKCountry, company.GC_RN_NKCountryCode);
			if (extraFilter != null)
			{
				filter.AddToFilter(extraFilter);
			}
			return filter;
		}
	}
}
