using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business.CountrySpecificRegistryDefaultValue;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.CountryCompliance.CountryComplianceInfoDisplay.Testing
{
	[TestedType(typeof(CountrySpecificRegistryDefaultValueCollection))]
	sealed class CountrySpecificRegistryDefaultValueCollectionTest : NonPersistentBusinessObjectCollectionTestCase<CountrySpecificRegistryDefaultValueCollection>
	{
		protected override CountrySpecificRegistryDefaultValueCollection GetCollectionToTest() => new CountrySpecificRegistryDefaultValueCollection();

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new DefaultRegistryValueDisplay("Caption", "DefaultValue");
		}

		public void TestAllowNew()
		{
			var headerCollection = GetCollectionToTest();
			Assert(!headerCollection.AllowNew);
		}
	}
}
