using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AddressMapper))]
	sealed class AddressMapperTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var address = Factory.NewWithValidTestData<OrgAddress>();
			var translatedAddress = address.TranslatedAddresses.AddNew();
			return new AddressMapper(address, translatedAddress);
		}
	}
}
