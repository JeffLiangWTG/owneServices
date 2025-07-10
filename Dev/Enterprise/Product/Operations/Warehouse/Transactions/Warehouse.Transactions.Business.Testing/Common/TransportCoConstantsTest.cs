using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class TransportCoConstantsTest : TestCase
	{
		public void TestAddressType()
		{
			AssertEquals(DocAddressType.TransportCompanyDocumentaryAddress, TransportCoConstants.AddressType);
		}

		public void TestAddressTypeCode()
		{
			AssertEquals(DocAddressTypes.Codes.TransportCompanyDocumentaryAddress,
				TransportCoConstants.AddressTypeCode);
		}

		public void TestGetSupportedAddressTypes()
		{
			AssertContainsExactElementsInAnyOrder(new[] { TransportCoConstants.AddressType },
				TransportCoConstants.GetSupportedAddressTypes());
		}

		public void TestRequirement()
		{
			var requirement = TransportCoConstants.Requirement;
			AssertEquals(TransportCoConstants.AddressType, requirement.DefaultDocAddressType);
			AssertEquals(ContactType.TransportServices, requirement.DefaultContactType);
		}
	}
}
