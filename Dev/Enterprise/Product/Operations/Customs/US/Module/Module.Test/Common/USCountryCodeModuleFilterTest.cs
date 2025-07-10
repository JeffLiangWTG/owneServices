using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Module.Testing
{
	[TestedType(typeof(USCountryCodeModuleFilter))]
	sealed class USCountryCodeModuleFilterTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject() => new USCountryCodeModuleFilter(DummyBizoSchema.Z0_Code, Factory);
	}
}
