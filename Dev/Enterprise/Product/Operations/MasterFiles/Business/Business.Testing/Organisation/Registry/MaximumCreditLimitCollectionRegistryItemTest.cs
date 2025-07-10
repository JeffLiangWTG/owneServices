using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(MaximumCreditLimitCollectionRegistryItem))]
	public class MaximumCreditLimitCollectionRegistryItemTest : StronglyTypedRegistryItemTestCase<MaximumCreditLimitCollection>
	{
		protected override StronglyTypedRegistryItem<MaximumCreditLimitCollection, MaximumCreditLimitCollection> GetNewRegistryItem()
		{
			var collection = new MaximumCreditLimitCollection();
			return new MaximumCreditLimitCollectionRegistryItem("", null, null, null, RegistryStorageFlags.All, RegistryOptions.Default, collection);
		}
	}
}
