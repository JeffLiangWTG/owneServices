using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class PhoneNumberUpdaterTest : TestCaseWithFactory
	{
		public void TestNormalize()
		{
			// Prepare test data
			var unlocoFromAustralia = GetFirstUnlocoByCountryCode("AU");
			var unlocoFromChina = GetFirstUnlocoByCountryCode("CN");

			var organization1 = CreateOrganization("AUSTRALIA JOHNTONESS", "OH1", unlocoFromAustralia);
			var organization2 = CreateOrganization("CHINA AIRLINES", "OH2", unlocoFromChina);

			var organizationAddresses = new[]
			{
				CreateOrganizationAddressWithPhone(organization1, "OA11", "Organization 1 Address 1", "+61 426 829 924"),
				CreateOrganizationAddressWithPhone(organization1, "OA12", "Organization 1 Address 2", "Crappy Phone Number"),
				CreateOrganizationAddressWithPhone(organization2, "OA21", "Organization 2 Address 1", "+61 (2) 8210 6666"),
				CreateOrganizationAddressWithPhone(organization2, "OA22", "Organization 2 Address 2", "15601131981")
			};

			IPhoneNumberUpdater phoneNumberUpdater = ObjectFactory.Get<IPhoneNumberUpdater>();
			var logger = new LoggerForTest();

			// Normalize
			var phoneNumberUpdateResults = phoneNumberUpdater.Normalize(logger, OrgAddressSchema.OA_Phone.Name, organizationAddresses
				.Select(oa => new Tuple<ZGuid, ZString>(oa.PK, oa.Header.UNLOCO.RL_RN_NKCountryCode))
				.ToArray());

			// Assert
			AssertEquals(4, phoneNumberUpdateResults.Length);

			Assert(phoneNumberUpdateResults[0].IsSuccessful);
			AssertEquals(organizationAddresses[0].PK, phoneNumberUpdateResults[0].RowPk);
			AssertEquals("+61 426 829 924", phoneNumberUpdateResults[0].OriginalNumber);
			AssertEquals("+61426829924", phoneNumberUpdateResults[0].NewNumber);
			AssertEquals(organizationAddresses[0].OA_Phone, phoneNumberUpdateResults[0].NewNumber);

			Assert(!phoneNumberUpdateResults[1].IsSuccessful);
			AssertEquals(organizationAddresses[1].PK, phoneNumberUpdateResults[1].RowPk);
			AssertEquals(organizationAddresses[1].OA_Phone, phoneNumberUpdateResults[1].OriginalNumber);
			Assert(phoneNumberUpdateResults[1].NewNumber.IsEmpty);

			Assert(phoneNumberUpdateResults[2].IsSuccessful);
			AssertEquals(organizationAddresses[2].PK, phoneNumberUpdateResults[2].RowPk);
			AssertEquals("+61 (2) 8210 6666", phoneNumberUpdateResults[2].OriginalNumber);
			AssertEquals("+61282106666", phoneNumberUpdateResults[2].NewNumber);
			AssertEquals(organizationAddresses[2].OA_Phone, phoneNumberUpdateResults[2].NewNumber);

			Assert(phoneNumberUpdateResults[3].IsSuccessful);
			AssertEquals(organizationAddresses[3].PK, phoneNumberUpdateResults[3].RowPk);
			AssertEquals("15601131981", phoneNumberUpdateResults[3].OriginalNumber);
			AssertEquals(organizationAddresses[3].OA_Phone, phoneNumberUpdateResults[3].NewNumber);
			AssertEquals("+8615601131981", phoneNumberUpdateResults[3].NewNumber);
		}

		#region Implementations

		RefUNLOCO GetFirstUnlocoByCountryCode(string countryCode)
		{
			var query = new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, countryCode);
			return Factory.LoadTop1<RefUNLOCO>(query);
		}

		OrgHeader CreateOrganization(ZString fullName, ZString code, RefUNLOCO unloco)
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_FullName = fullName;
			orgHeader.OH_Code = code;
			orgHeader.OH_RL_NKClosestPort = unloco.RL_Code;
			Factory.Save();
			return orgHeader;
		}

		OrgAddress CreateOrganizationAddressWithPhone(OrgHeader organization, ZString code, ZString address, ZString phone)
		{
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			orgAddress.OA_OH = organization.PK;
			orgAddress.OA_Code = code;
			orgAddress.OA_Address1 = address;
			orgAddress.OA_Phone = phone;
			Factory.Save();
			return orgAddress;
		}

		#endregion
	}
}
