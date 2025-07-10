using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TR.NCTS.Business.Testing
{
	[TestedType(typeof(NctsCompanyValidationHelper))]
	sealed class NctsCompanyValidationHelperTest : TestCaseWithFactory
	{
		public void TestCheckAddressLength()
		{
			var orgHeaderLong = Factory.New<OrgHeader>();
			orgHeaderLong.OH_Code = "XYZAAZ";
			orgHeaderLong.OH_FullName = "1234567890123456789012345678901234567890";

			var addressLong = orgHeaderLong.MainAddress;
			addressLong.OA_OH = orgHeaderLong.PK;
			addressLong.CompanyName = "Company Name";
			addressLong.Address1 = "12345678901234567890";
			addressLong.Address2 = "12345678901234567890";
			addressLong.City = "IST";
			addressLong.Postcode = "340300";
			addressLong.OA_RN_NKCountryCode = "TR";

			var docAddress = Factory.New<JobDocAddress>();
			docAddress.E2_OA_Address = addressLong.PK;
			var warningMessage = NctsCompanyValidationHelper.CheckAddressLength(docAddress);
			CombineAssertions("Warning Message", () =>
			{
				AssertEquals("Has Warning Message", 2, warningMessage.Length);
				AssertEquals("Company Name", "Company Name is 40 characters. Only the first 35 characters will be sent in the message", warningMessage[0]);
				AssertEquals("Company Address", "Company Address is 41 characters. Only the first 35 characters will be sent in the message", warningMessage[1]);
			});

			var orgHeaderNotLong = Factory.New<OrgHeader>();
			orgHeaderNotLong.OH_Code = "XYZAAY";
			orgHeaderNotLong.OH_FullName = "12345678901234567890123456789012345";
			var addressNotLong = orgHeaderNotLong.MainAddress;
			addressNotLong.OA_OH = orgHeaderNotLong.PK;
			addressNotLong.CompanyName = "Company Name 2";
			addressNotLong.Address1 = "12345678901234567";
			addressNotLong.Address2 = "12345678901234567";
			addressNotLong.City = "IST";
			addressNotLong.Postcode = "340300";
			addressNotLong.OA_RN_NKCountryCode = "TR";

			docAddress.E2_OA_Address = addressNotLong.PK;
			warningMessage = NctsCompanyValidationHelper.CheckAddressLength(docAddress);
			AssertEquals("No Warning Message", ZInt.Zero, warningMessage.Length);
		}

		public void TestCheckOrganizationLength()
		{
			var orgHeaderLong = Factory.New<OrgHeader>();
			orgHeaderLong.OH_Code = "XYZAAZ";
			orgHeaderLong.OH_FullName = "1234567890123456789012345678901234567890";

			var addressLong = orgHeaderLong.MainAddress;
			addressLong.OA_OH = orgHeaderLong.PK;
			addressLong.CompanyName = "Company Name";
			addressLong.Address1 = "12345678901234567890";
			addressLong.Address2 = "12345678901234567890";
			addressLong.City = "IST";
			addressLong.Postcode = "340300";
			addressLong.OA_RN_NKCountryCode = "TR";

			var warningMessage = NctsCompanyValidationHelper.CheckOrganizationLength(orgHeaderLong, addressLong);
			CombineAssertions("Warning Message", () =>
			{
				AssertEquals("Has Warning Message", 2, warningMessage.Length);
				AssertEquals("Company Name", "Company Name is 40 characters. Only the first 35 characters will be sent in the message", warningMessage[0]);
				AssertEquals("Company Address", "Company Address is 41 characters. Only the first 35 characters will be sent in the message", warningMessage[1]);
			});

			var orgHeaderNotLong = Factory.New<OrgHeader>();
			orgHeaderNotLong.OH_Code = "XYZAAY";
			orgHeaderNotLong.OH_FullName = "12345678901234567890123456789012345";
			var addressNotLong = orgHeaderNotLong.MainAddress;
			addressNotLong.OA_OH = orgHeaderNotLong.PK;
			addressNotLong.CompanyName = "Company Name 2";
			addressNotLong.Address1 = "12345678901234567";
			addressNotLong.Address2 = "12345678901234567";
			addressNotLong.City = "IST";
			addressNotLong.Postcode = "340300";
			addressNotLong.OA_RN_NKCountryCode = "TR";

			warningMessage = NctsCompanyValidationHelper.CheckOrganizationLength(orgHeaderNotLong, addressNotLong);
			AssertEquals("No Warning Message", ZInt.Zero, warningMessage.Length);
		}
	}
}
