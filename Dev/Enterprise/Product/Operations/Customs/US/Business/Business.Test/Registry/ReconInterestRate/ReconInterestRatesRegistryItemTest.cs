using Enterprise.Integration;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.US.DataRegistry.Business.Testing
{
	[TestedType(typeof(ReconInterestRatesRegistryItem))]
	sealed class ReconInterestRatesRegistryItemTest : StronglyTypedRegistryItemTestCaseWithFactory<ReconInterestRateCollection>
	{
		protected override StronglyTypedRegistryItem<ReconInterestRateCollection, ReconInterestRateCollection> GetNewRegistryItem()
		{
			return new ReconInterestRatesRegistryItem("", null, null, null, RegistryStorageFlags.System, new ReconInterestRateCollection());
		}
	}
}
