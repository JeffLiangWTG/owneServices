using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Module
{
	public class CountryModuleNkFilter : ModuleNkFilter
	{
		public CountryModuleNkFilter(SchemaStringColumn schemaColumn, BusinessObjectFactory factory)
			: base("Country/Region", schemaColumn, ModuleIDs.RefCountry, new RefCountryCollection(factory))
		{
			ForeignCodeColumnOverride = RefCountrySchema.RN_Code;
		}

		protected CountryModuleNkFilter(string description, SchemaStringColumn schemaColumn, ModuleIdentifier moduleId, IBusinessObjectCollection list, SchemaStringColumn foreignCodeColumn)
			: base(description, schemaColumn, moduleId, list)
		{
			ForeignCodeColumnOverride = foreignCodeColumn;
		}
	}
}
