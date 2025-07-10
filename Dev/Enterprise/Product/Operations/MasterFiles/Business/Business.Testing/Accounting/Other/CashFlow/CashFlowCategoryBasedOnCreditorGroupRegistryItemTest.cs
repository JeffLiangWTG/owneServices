using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(CashFlowCategoryBasedOnCreditorGroupRegistryItem))]
	sealed class CashFlowCategoryBasedOnCreditorGroupRegistryItemTest : StronglyTypedRegistryItemTestCase<CashFlowCategoryBasedOnCreditorGroupCollection>
	{
		protected override StronglyTypedRegistryItem<CashFlowCategoryBasedOnCreditorGroupCollection, CashFlowCategoryBasedOnCreditorGroupCollection> GetNewRegistryItem()
		{
			return new CashFlowCategoryBasedOnCreditorGroupRegistryItem("", null, null, null, RegistryStorageFlags.Company, RegistryOptions.IsOnlyForSupport);
		}
	}
}
