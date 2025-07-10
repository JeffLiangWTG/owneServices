using System.ComponentModel;
using CargoWise.ComponentModel;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(ACEFDAJobDocAddress))]
	public sealed class ACEFDAJobDocAddressTest : EnterpriseBusinessObjectTestCase
	{
		public void TestE2_AddressType()
		{
			AssertHasCustomAttribute<ListAttribute>(typeof(ACEFDAJobDocAddress), nameof(ACEFDAJobDocAddress.E2_AddressType), false, attribute => attribute.ListDataSourceMember == "Lookups.AddressTypeList");

			var newDocAddress = Factory.New<ACEFDA>().DocAddresses.AddNew();
			AssertEquals("", newDocAddress.E2_AddressType);

			newDocAddress.E2_AddressType = DocAddressTypes.Codes.Shipper;
			AssertEquals(DocAddressTypes.Codes.Shipper, newDocAddress.E2_AddressType);
		}

		public void TestAddressDescription()
		{
			AssertHasCustomAttribute<ReadOnlyAttribute>(typeof(ACEFDAJobDocAddress), nameof(ACEFDAJobDocAddress.AddressDescription), false, attribute => attribute.IsReadOnly);

			docAddress.E2_AddressType = DocAddressTypes.Codes.Shipper;
			AssertEquals(DocAddressTypes.Descriptions.Shipper, docAddress.AddressDescription);

			docAddress.E2_AddressType = DocAddressTypes.Codes.Laboratory;
			AssertEquals(DocAddressTypes.Descriptions.Laboratory, docAddress.AddressDescription);

			docAddress.E2_AddressType = "";
			AssertEquals(DocAddressTypes.Unspecified, docAddress.AddressDescription);
		}

		public void TestOverrideRequirement()
		{
			AssertType<ACEFDAJobDocAddressRequirement>(docAddress.OverrideRequirement);
		}

		public void TestLookups()
		{
			AssertType<ACEFDAJobDocAddressLookups>(docAddress.Lookups);
		}

		public void TestPGAContactDetailsAndAddressDetails()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "AAA";
			org.OH_FullName = "Company Name";

			var contact = Factory.New<OrgContact>();
			contact.OC_OH = org.PK;
			contact.OC_ContactName = "Contact Name";
			contact.OC_Phone = "10086";
			contact.OC_Fax = "1234567890";
			contact.OC_Email = "d@w.com";

			var address = Factory.New<OrgAddress>();
			address.OA_OH = org.PK;
			address.Address1 = "Address1";
			address.Address2 = "Address2";
			address.City = "City";
			address.State = "ST";
			address.Postcode = "12345";
			address.OA_RN_NKCountryCode = "AU";
			Factory.Save();
			docAddress.E2_OA_Address = address.PK;
			docAddress.OrganisationPK = org.PK;
			docAddress.ContactPK = contact.PK;

			var pgaDetails = (IPGAContactDetails)docAddress;
			CombineAssertions(() =>
			{
				AssertEquals("Contact Name", pgaDetails.Name);
				AssertEquals("10086", pgaDetails.PhoneNumber);
				AssertEquals("d@w.com", pgaDetails.EmailAddress);
				AssertEquals("1234567890", pgaDetails.Fax);
				AssertEquals("Address1", pgaDetails.CompanyAddress.AddressLine1);
			});

			var addressDetails = (IAddressDetails)docAddress;
			CombineAssertions(() =>
			{
				AssertEquals("Company Name", addressDetails.CompanyName);
				AssertEquals("Contact Name", addressDetails.ContactName);
				AssertEquals("10086", addressDetails.Phone);
				AssertEquals("1234567890", addressDetails.Fax);
				AssertEquals("d@w.com", addressDetails.Email);
				AssertEquals("Address1", addressDetails.AddressLine1);
				AssertEquals("Address2", addressDetails.AddressLine2);
				AssertEquals("City", addressDetails.City);
				AssertEquals("ST", addressDetails.State);
				AssertEquals("12345", addressDetails.PostCode);
				AssertEquals("AU", addressDetails.Country);
			});

			docAddress.E2_AddressOverride = true;
			docAddress.E2_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			docAddress.E2_Phone = "+1 201-555-8888";
			AssertEquals("2015558888", pgaDetails.PhoneNumber);
		}

		public void TestIsFSVPImporter()
		{
			docAddress.E2_AddressType = DocAddressTypes.Codes.FSVPImporter;
			Assert(docAddress.IsFSVPImporter);
			docAddress.E2_AddressType = DocAddressTypes.Codes.Shipper;
			Assert(!docAddress.IsFSVPImporter);
		}

		protected override void SetUp()
		{
			base.SetUp();
			docAddress = Factory.New<ACEFDAJobDocAddress>();
		}

		ACEFDAJobDocAddress docAddress;
	}
}
