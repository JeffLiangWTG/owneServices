using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ActiveOrAllAddressesCollection))]
	sealed class ActiveOrAllAddressesCollectionTest : BusinessObjectCollectionViewTestCase<ActiveOrAllAddressesCollection>
	{
		protected override ActiveOrAllAddressesCollection GetCollectionToTest()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			org.Addresses.AddNew();
			return org.ActiveOrAllAddresses;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<OrgAddress>();
		}
	}
}
