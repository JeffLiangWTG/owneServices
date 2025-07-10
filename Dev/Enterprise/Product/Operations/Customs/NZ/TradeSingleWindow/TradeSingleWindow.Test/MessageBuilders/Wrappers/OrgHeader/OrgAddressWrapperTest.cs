using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders.Testing
{
	class OrgAddressWrapperTest : TestCaseWithFactory
	{
		public void TestStaticNew()
		{
			AssertNull(OrgAddressWrapper.New(null));
			AssertNotNull(OrgAddressWrapper.New(Factory.New<OrgAddress>()));
		}

		public void TestOrgAddressWrapper()
		{
			var organisation = Factory.New<OrgHeader>();
			organisation.OH_FullName = "Test OrgHeader";
			organisation.MainAddress.OA_Address1 = "100 Main St.";
			organisation.MainAddress.OA_City = "Sydney";
			organisation.MainAddress.OA_State = "NSW";
			organisation.MainAddress.OA_PostCode = "2000";
			organisation.MainAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			organisation.MainAddress.OA_Phone = "+61 2 80012201";
			organisation.MainAddress.OA_Fax = "+61 2 99999999";
			organisation.MainAddress.OA_Email = "admin@Organisation.com";
			var tswCode = organisation.CustomsCodes.AddNew();
			tswCode.OK_CodeType = OrgCusCode.CodeTypes.CustomsClientCode;
			tswCode.OK_CustomsRegNo = "372845965J";
			IOrganisation orgAddressWrapper = OrgAddressWrapper.New(organisation.MainAddress);
			Assert("CustomsClientCode", orgAddressWrapper.CustomsClientCode.IsEmpty);
			Assert("CustomsSupplierCode", orgAddressWrapper.CustomsSupplierCode.IsEmpty);
			AssertEquals("Name", "Test OrgHeader", orgAddressWrapper.Name);
			AssertEquals("ItemValue", "Sydney", orgAddressWrapper.City);
			AssertEquals("CountryCode", "AU", orgAddressWrapper.CountryCode);
			AssertEquals("CountryRegion", "NSW", orgAddressWrapper.CountryRegion);
			AssertEquals("PostCode", "2000", orgAddressWrapper.PostCode);
			AssertEquals("Street", "100 Main St.", orgAddressWrapper.Address);
			AssertEquals("Communications", Enumerable.Empty<ICommunication>(), orgAddressWrapper.Communications);
			AssertEquals("Contacts", Enumerable.Empty<IContact>(), orgAddressWrapper.Contacts);
		}

		public void TestAlternativeOrgAddress()
		{
			var organisation = Factory.New<OrgHeader>();
			organisation.OH_FullName = "Test OrgHeader";
			organisation.MainAddress.OA_Address1 = "100 Main St.";
			organisation.MainAddress.OA_City = "Sydney";
			organisation.MainAddress.OA_State = "NSW";
			organisation.MainAddress.OA_PostCode = "2000";
			organisation.MainAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			organisation.MainAddress.OA_Phone = "+61 2 80012201";
			organisation.MainAddress.OA_Fax = "+61 2 99999999";
			organisation.MainAddress.OA_Email = "admin@Organisation.com";
			var tswCode = organisation.CustomsCodes.AddNew();
			tswCode.OK_CodeType = OrgCusCode.CodeTypes.CustomsClientCode;
			tswCode.OK_CustomsRegNo = "372845965J";
			var altAddress = organisation.Addresses.AddNew();
			altAddress.OA_Address1 = "NZ Distribution Warehouse";
			altAddress.OA_City = "Aukland";
			altAddress.OA_RN_NKCountryCode = "NZ";
			IOrganisation wrapper = OrgAddressWrapper.New(altAddress);
			AssertNotNull("OrgAddressWrapper", wrapper);
			AssertEquals("Name", "Test OrgHeader", wrapper.Name);
			AssertEquals("ItemValue", "Aukland", wrapper.City);
			AssertEquals("CountryCode", "NZ", wrapper.CountryCode);
		}

		public void TestOrgAddressWrapperWithNull()
		{
			AssertNull(OrgAddressWrapper.New(null));
		}

		public void TestAddressTruncates()
		{
			var organisation = Factory.New<OrgHeader>();
			organisation.OH_FullName = "Test OrgHeader";
			organisation.MainAddress.OA_Address1 = "Unit 2A, Building 15, Campus East,";
			organisation.MainAddress.OA_Address2 = "University of New South Wales, Randwick";
			organisation.MainAddress.OA_City = "Sydney";
			organisation.MainAddress.OA_State = "NSW";
			organisation.MainAddress.OA_PostCode = "2000";
			organisation.MainAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			IOrganisation orgAddressWrapper = OrgAddressWrapper.New(organisation.MainAddress);
			AssertEquals("Address truncates to maximum message capability", "Unit 2A, Building 15, Campus East, University of New South Wales, Rand", orgAddressWrapper.Address);
		}

		public void TestOrgAddressWrapperStripsPostCodeOfInvalidCharacters()
		{
			var organisation = Factory.New<OrgHeader>();
			organisation.OH_FullName = "JAT Systems Inc.";
			organisation.MainAddress.OA_Address1 = "13555 Back Valley Rd.";
			organisation.MainAddress.OA_City = "Sale Creek";
			organisation.MainAddress.OA_State = "CHI";
			organisation.MainAddress.OA_PostCode = "37373-7780";
			organisation.MainAddress.OA_RL_NKRelatedPortCode = "USCHI";
			organisation.MainAddress.OA_Phone = "+1 412 80012201";
			organisation.MainAddress.OA_Fax = "+1 412 99999999";
			organisation.MainAddress.OA_Email = "admin@Organisation.com";
			IOrganisation orgAddressWrapper = OrgAddressWrapper.New(organisation.MainAddress);
			AssertEquals("PostCode should have had invalid character removed & if PostCode length is greater than allowed in message should be trimmed", "373737780", orgAddressWrapper.PostCode);

			organisation.MainAddress.OA_PostCode = "45001SWIFT";
			AssertEquals("Long PostCode should have been truncated to the message maximum length of 9 characters", "45001SWIF", orgAddressWrapper.PostCode);
		}
	}
}
