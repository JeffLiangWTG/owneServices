using CargoWise.Types;

namespace Enterprise.MasterFiles.Integration
{
	public interface IDocAddress
	{
		ZString E2_AddressType { get; }
		ZBool E2_AddressOverride { get; }
		ZGuid E2_OA_Address { get; }
		ZString E2_CompanyName { get; }
		ZString E2_CompanyNameTruncated { get; }
		ZString E2_AdditionalAddressInformation { get; }
		ZString E2_Address1 { get; }
		ZString E2_Address2 { get; }
		ZString E2_City { get; }
		ZString E2_State { get; }
		ZString E2_Postcode { get; }
		ZString E2_GovRegNum { get; }
		ZString E2_GovRegNumType { get; }
		ZString E2_PortCode { get; }
		ZString CountryCode { get; }
		ZString AddressCaption { get; }
		ZString ParentDescription { get; }
		ZString E2_PassportCountryOfIssue { get; }
		ZDateTime E2_PassportDateOfBirth { get; }
		ZString E2_PassportID { get; }
		IOrgHeader Organisation { get; }
		ZString E2_Phone { get; }
		ZString E2_Fax { get; }
		ZString E2_RN_NKCountryCode { get; }

		ZString AddressFull { get; }

		ZString CountryDescription { get; }
	}
}
