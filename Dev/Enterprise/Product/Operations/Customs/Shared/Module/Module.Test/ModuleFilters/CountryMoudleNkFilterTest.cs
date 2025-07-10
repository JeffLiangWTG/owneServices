using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Module.Testing
{
	[TestedType(typeof(CountryModuleNkFilter))]
	sealed class CountryMoudleNkFilterTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject() => new CountryModuleNkFilter(DummyBizoSchema.Z0_Code, Factory);
	}
}
