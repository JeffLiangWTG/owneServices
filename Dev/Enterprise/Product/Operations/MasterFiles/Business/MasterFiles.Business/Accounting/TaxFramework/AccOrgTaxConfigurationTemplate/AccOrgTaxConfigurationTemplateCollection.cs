using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[ModuleID(ModuleId.AccOrgTaxConfigurationTemplate)]
	public class AccOrgTaxConfigurationTemplateCollection : ActiveBusinessObjectCollection<AccOrgTaxConfigurationTemplate>
	{
		public AccOrgTaxConfigurationTemplateCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public AccOrgTaxConfigurationTemplateCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			return new ZQuery(AccOrgTaxConfigurationTemplateSchema.OCT_GC_Company, GlbCompany.CurrentCompany.PK);
		}
	}
}
