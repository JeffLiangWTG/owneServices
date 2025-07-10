using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business.Testing
{
	public static class BusinessObjectFactoryExtensions
	{
		public static OrgAddress CreateValidOrgAddress(this BusinessObjectFactory factory)
		{
			var address = factory.New<OrgAddress>();

			address.OA_Address1 = "[_MOCK_ADDRESS_1_]";
			address.OA_Address2 = "[_MOCK_ADDRESS_2_]";
			address.OA_City = "[_MOCK_CITY_]";
			address.OA_PostCode = "2000";
			address.OA_State = "NSW";
			address.OA_RN_NKCountryCode = "AU";

			return address;
		}

		public static JobDocAddress CreateValidOverriddenJobDocAddress(this BusinessObjectFactory factory)
		{
			var docAddress = factory.New<JobDocAddress>();

			docAddress.E2_Address1 = "[_MOCK_ADDRESS_1_]";
			docAddress.E2_Address2 = "[_MOCK_ADDRESS_2_]";
			docAddress.E2_City = "[_MOCK_CITY_]";
			docAddress.E2_Postcode = "2000";
			docAddress.E2_State = "NSW";
			docAddress.E2_RN_NKCountryCode = "AU";

			docAddress.E2_AddressOverride = true;

			return docAddress;
		}
	}
}
