using NUnit.Framework;

namespace Enterprise.Customs._CustomsTemplate_.Module.Testing
{
	[TestedType(typeof(OrgSupplierPartModule))]
	class OrgSupplierPartModuleTest : Customs.Module.Testing.OrgSupplierPartModuleTest
	{
		protected override string CountryCode => Core.Constants.CountryCodes._TemplateCountryName_;
	}
}
