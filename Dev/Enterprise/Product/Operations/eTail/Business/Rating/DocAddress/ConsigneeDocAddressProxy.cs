using CargoWise.Types;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.eTail.Business.Rating
{
	class ConsigneeDocAddressProxy : IDocAddress
	{
		public ConsigneeDocAddressProxy(HVLVConsignment consignment)
		{
			inner = consignment;
		}

		readonly HVLVConsignment inner;

		public ZString E2_AddressType { get; }
		public ZBool E2_AddressOverride { get; }
		public ZGuid E2_OA_Address { get; }
		public ZString E2_CompanyName { get; }
		public ZString E2_CompanyNameTruncated { get; }
		public ZString E2_AdditionalAddressInformation { get; }
		public ZString E2_Address1 { get; }
		public ZString E2_Address2 { get; }
		public ZString E2_City => inner.HVC_ConsigneeCity;
		public ZString E2_State => inner.HVC_ConsigneeState;
		public ZString E2_Postcode => inner.HVC_ConsigneePostcode;
		public ZString E2_GovRegNum { get; }
		public ZString E2_GovRegNumType { get; }
		public ZString E2_PortCode { get; }
		public ZString CountryCode => inner.HVC_RN_NKConsigneeCountryCode;
		public ZString AddressCaption { get; }
		public ZString ParentDescription { get; }
		public ZString E2_PassportCountryOfIssue { get; }
		public ZDateTime E2_PassportDateOfBirth { get; }
		public ZString E2_PassportID { get; }
		public IOrgHeader Organisation { get; }
		public ZString E2_Phone { get; }
		public ZString E2_Fax { get; }
		ZString IDocAddress.E2_RN_NKCountryCode { get; }

		public ZString AddressFull { get; }

		public ZString CountryDescription { get; }
	}
}
