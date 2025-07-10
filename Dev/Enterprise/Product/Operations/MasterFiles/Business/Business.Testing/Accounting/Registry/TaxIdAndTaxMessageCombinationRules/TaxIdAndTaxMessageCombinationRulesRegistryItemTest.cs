using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(TaxIdAndTaxMessageCombinationRulesRegistryItem))]
	sealed class TaxIdAndTaxMessageCombinationRulesRegistryItemTest : StronglyTypedRegistryItemTestCase<TaxIdAndTaxMessageCombinationRulesConfiguration>
	{
		protected override StronglyTypedRegistryItem<TaxIdAndTaxMessageCombinationRulesConfiguration, TaxIdAndTaxMessageCombinationRulesConfiguration> GetNewRegistryItem()
		{
			return new TaxIdAndTaxMessageCombinationRulesRegistryItem("", null, null, null, RegistryStorageFlags.Company, RegistryOptions.Default);
		}
	}
}
