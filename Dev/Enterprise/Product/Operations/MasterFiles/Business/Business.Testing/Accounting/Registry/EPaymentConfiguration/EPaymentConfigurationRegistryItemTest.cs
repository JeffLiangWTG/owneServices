using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(EPaymentConfigurationRegistryItem))]
	sealed class EPaymentConfigurationRegistryItemTest : StronglyTypedRegistryItemTestCase<EPaymentConfigurationCollection>
	{
		#region Implementation

		protected override StronglyTypedRegistryItem<EPaymentConfigurationCollection, EPaymentConfigurationCollection> GetNewRegistryItem()
		{
			return new EPaymentConfigurationRegistryItem("", null, null, null, RegistryStorageFlags.Company, RegistryOptions.IsOnlyForSupport);
		}

		#endregion
	}
}
