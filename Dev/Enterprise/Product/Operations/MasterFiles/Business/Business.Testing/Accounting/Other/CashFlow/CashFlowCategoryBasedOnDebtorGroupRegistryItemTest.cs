using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(CashFlowCategoryBasedOnDebtorGroupRegistryItem))]
	sealed class CashFlowCategoryBasedOnDebtorGroupRegistryItemTest : StronglyTypedRegistryItemTestCase<CashFlowCategoryBasedOnDebtorGroupCollection>
	{
		protected override StronglyTypedRegistryItem<CashFlowCategoryBasedOnDebtorGroupCollection, CashFlowCategoryBasedOnDebtorGroupCollection> GetNewRegistryItem()
		{
			return new CashFlowCategoryBasedOnDebtorGroupRegistryItem("", null, null, null, RegistryStorageFlags.Company, RegistryOptions.IsOnlyForSupport);
		}
	}
}
