using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(RevenueRecognitionByChargeGroupRegistryItem))]
	sealed class RevenueRecognitionByChargeGroupRegistryItemTest : StronglyTypedRegistryItemTestCase<RevenueRecognitionByChargeGroupCollection>
	{
		protected override StronglyTypedRegistryItem<RevenueRecognitionByChargeGroupCollection, RevenueRecognitionByChargeGroupCollection> GetNewRegistryItem()
		{
			return new RevenueRecognitionByChargeGroupRegistryItem("", null, null, null, RegistryStorageFlags.System, new RevenueRecognitionByChargeGroupCollection());
		}
	}
}
