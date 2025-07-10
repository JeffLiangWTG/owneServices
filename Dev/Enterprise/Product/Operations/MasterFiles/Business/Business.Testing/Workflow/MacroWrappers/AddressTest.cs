using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Macros.Testing
{
	sealed class AddressTest : TestCaseWithFactory
	{
		#region TestAddress

		public void TestAddress()
		{
			var orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_Address1 = "line 1";
			orgAddress.OA_Address2 = "line 2";
			orgAddress.OA_State = "NSW";
			orgAddress.OA_PostCode = "2015";
			orgAddress.OA_City = "Sydney";
			orgAddress.OA_RN_NKCountryCode = "AU";
			orgAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			orgAddress.OA_Email = "test@test.com";

			var orgHeader = Factory.New<OrgHeader>();
			orgAddress.OA_OH = orgHeader.PK;

			var address = new Address(orgAddress);

			CombineAssertions(() =>
			{
				AssertEquals("AddressIdentifier", orgAddress.PK, address.AddressIdentifier);
				AssertEquals("HeaderIdentifier", orgAddress.Header.PK, address.HeaderIdentifier);
				AssertEquals("AddressLine1", "line 1", address.AddressLine1);
				AssertEquals("AddressLine2", "line 2", address.AddressLine2);
				AssertEquals("State", "NSW", address.State);
				AssertEquals("Postcode", "2015", address.Postcode);
				AssertEquals("City", "Sydney", address.City);
				AssertEquals("Country.Code", "AU", address.Country.Code);
				AssertEquals("Unloco.Code", "AUSYD", address.Unloco.Code);
				AssertEquals("Email", "test@test.com", address.Email);
				AssertMultilineASCIIEquals("AddressFormatted",
@"LINE 1
LINE 2
SYDNEY NSW 2015
AUSTRALIA", address.AddressFormatted);
			});
		}

		#endregion

		#region TestResetAddressFormatted

		public void TestResetAddressFormatted()
		{
			var orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_Address1 = "line 1";
			orgAddress.OA_Address2 = "line 2";
			orgAddress.OA_State = "NSW";
			orgAddress.OA_PostCode = "2015";
			orgAddress.OA_City = "Sydney";
			orgAddress.OA_RN_NKCountryCode = "AU";
			orgAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			orgAddress.OA_Email = "test@test.com";

			var address = new Address(orgAddress);

			AssertMultilineASCIIEquals("AddressFormatted",
@"LINE 1
LINE 2
SYDNEY NSW 2015
AUSTRALIA", address.AddressFormatted);

			address.AddressLine1 = "line 11";

			AssertMultilineASCIIEquals("AddressFormatted",
@"LINE 11
LINE 2
SYDNEY NSW 2015
AUSTRALIA", address.AddressFormatted);

			address.AddressLine2 = "line 22";

			AssertMultilineASCIIEquals("AddressFormatted",
@"LINE 11
LINE 22
SYDNEY NSW 2015
AUSTRALIA", address.AddressFormatted);

			address.State = "QLD";

			AssertMultilineASCIIEquals("AddressFormatted",
@"LINE 11
LINE 22
SYDNEY QLD 2015
AUSTRALIA", address.AddressFormatted);

			address.Postcode = "4000";

			AssertMultilineASCIIEquals("AddressFormatted",
@"LINE 11
LINE 22
SYDNEY QLD 4000
AUSTRALIA", address.AddressFormatted);

			address.City = "Brisbane";

			AssertMultilineASCIIEquals("AddressFormatted",
@"LINE 11
LINE 22
BRISBANE QLD 4000
AUSTRALIA", address.AddressFormatted);
		}

		#endregion

		#region TestNullAddress

		public void TestNullAddress()
		{
			var address = new Address(null);

			CombineAssertions(() =>
			{
				AssertEquals("AddressIdentifier", ZGuid.Empty, address.AddressIdentifier);
				AssertEquals("HeaderIdentifier", ZGuid.Empty, address.HeaderIdentifier);
				AssertNullOrEmpty("AddressLine1", address.AddressLine1);
				AssertNullOrEmpty("AddressLine2", address.AddressLine2);
				AssertNullOrEmpty("State", address.State);
				AssertNullOrEmpty("Postcode", address.Postcode);
				AssertNullOrEmpty("City", address.City);
				AssertNotNull("Country", address.Country);
				AssertNotNull("Unloco", address.Unloco);
				AssertNotNull("Email", address.Email);
				AssertNotNull("AddressFormatted", address.AddressFormatted);
			});
		}

		#endregion
	}
}
