using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.Customs.Module;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Module
{
	public class USCountryCodeModuleFilter : CountryModuleNkFilter
	{
		public USCountryCodeModuleFilter(SchemaStringColumn schemaColumn, BusinessObjectFactory factory)
			: base("Country Code", schemaColumn, ModuleIDs.Customs.US.Country, new USCCountryCollection(factory), USCCountrySchema.UC_Code)
		{
		}
	}
}
