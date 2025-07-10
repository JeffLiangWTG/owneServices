using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Module.Testing
{
	[TestedType(typeof(TariffInvalidModuleFilter))]
	sealed class TariffInvalidModuleFilterTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject() => new TariffInvalidModuleFilter("TestDescription", (expiredDate) => new ZQuery());
	}
}
