using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.Accounting;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	[ModuleID(ModuleId.AccTaxOverrideGroup)]
	public class AccTaxOverrideGroupCollectionCompany : BusinessObjectCollection<AccTaxOverrideGroup>
	{
		public AccTaxOverrideGroupCollectionCompany(BusinessObjectFactory factory)
			: this(factory, GlbCompany.CurrentCompany)
		{
		}

		public AccTaxOverrideGroupCollectionCompany(BusinessObjectFactory factory, GlbCompany company)
			: base(factory)
		{
			Argument.NotNull(company, "company");

			this.company = company;
		}
		readonly GlbCompany company;

		protected override ZQuery CreateRelationshipFilter()
		{
			ZQuery taxOverrideGroupQuery = base.CreateRelationshipFilter();

			return DependencyFactory.GetTaxFrameworkConfigurationHelper().GetCompanyTaxOverrideGroup(taxOverrideGroupQuery, company);
		}

		IAccountingMasterFilesDependencyFactory DependencyFactory => dependencyFactory ?? (dependencyFactory = ObjectFactory.Get<IAccountingMasterFilesDependencyFactory>());
		IAccountingMasterFilesDependencyFactory dependencyFactory;
	}
}
