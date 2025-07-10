using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(RevenueRecognitionRegistryItem))]
	sealed class RevenueRecognitionRegistryItemTest : StronglyTypedRegistryItemTestCase<RevenueRecognitionCollection>
	{
		protected override StronglyTypedRegistryItem<RevenueRecognitionCollection, RevenueRecognitionCollection> GetNewRegistryItem()
		{
			return new RevenueRecognitionRegistryItem("", null, null, null, RegistryStorageFlags.System, new RevenueRecognitionCollection());
		}
	}
}
