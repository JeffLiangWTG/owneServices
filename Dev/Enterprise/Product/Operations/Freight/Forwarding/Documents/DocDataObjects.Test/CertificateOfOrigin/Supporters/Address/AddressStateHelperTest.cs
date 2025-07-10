using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Supporters.Address;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing.CertificateOfOrigin.Supporters.Address
{
	sealed class AddressStateHelperTest : TestCaseWithFactory
	{
		public void Test_GetAddressStateString()
		{
			var addressState = AddressStateHelper.GetAddressStateString(sameAsExporter: true, unknown: false, excludeFromCertificate: true);
			var expectedState = "True\nFalse\nTrue";
			AssertEquals(expectedState, addressState);
		}

		public void Test_Test_GetAddressStateString_ThrowsException_When_IllegalArguemntsGiven()
		{
			AssertExceptionThrown<ArgumentException>("sameAsExporter and unknown cannot both be true at the same time",
				() => AddressStateHelper.GetAddressStateString(sameAsExporter: true, unknown: true, excludeFromCertificate: true));
		}

		public void Test_GetAddressStateObjectFromString()
		{
			var addressStateString = "true\nfalse\ntrue";
			var addressState = AddressStateHelper.GetAddressStateObjectFromString(addressStateString);

			AssertEquals(addressState.IsSameAsExporter, true);
			AssertEquals(addressState.IsUnknown, false);
			AssertEquals(addressState.ExcludeFromPDF, true);
		}

		public void Test_GetAddressStateObjectFromString_ReturnNull_Given_IncorrectString()
		{
			AssertNull(AddressStateHelper.GetAddressStateObjectFromString(null));
			AssertNull(AddressStateHelper.GetAddressStateObjectFromString(string.Empty));
			AssertNull(AddressStateHelper.GetAddressStateObjectFromString("ABCD"));
			AssertNull(AddressStateHelper.GetAddressStateObjectFromString("True\nFalse"));
			AssertNull(AddressStateHelper.GetAddressStateObjectFromString("True\nFalse\nTrue\nFalse"));
			AssertNull(AddressStateHelper.GetAddressStateObjectFromString("True\nFalse\nString"));
		}
	}
}
