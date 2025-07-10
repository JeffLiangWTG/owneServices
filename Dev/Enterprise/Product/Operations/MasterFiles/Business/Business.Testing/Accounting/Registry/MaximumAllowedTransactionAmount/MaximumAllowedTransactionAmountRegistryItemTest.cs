using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(MaximumAllowedTransactionAmountRegistryItem))]
	sealed class MaximumAllowedTransactionAmountRegistryItemTest : StronglyTypedRegistryItemTestCase<MaximumAllowedTransactionAmount>
	{
		protected override StronglyTypedRegistryItem<MaximumAllowedTransactionAmount, MaximumAllowedTransactionAmount> GetNewRegistryItem()
		{
			return new MaximumAllowedTransactionAmountRegistryItem("", null, null, null, RegistryStorageFlags.Company | RegistryStorageFlags.System, RegistryOptions.IsOnlyForSupport, new MaximumAllowedTransactionAmount() { MaximumAllowedHeaderAmount = 1, MaximumAllowedLineAmount = 2 });
		}
	}
}
