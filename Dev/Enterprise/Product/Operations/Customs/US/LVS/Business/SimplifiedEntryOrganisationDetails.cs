using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.US.LVS.Business
{
	enum OrganisationType
	{
		Seller,
		Consignee,
		Buyer,
		Manufacturer,
	}

	class SimplifiedEntryOrganisationDetails : ISimplifiedEntryOrganisationDetails
	{
		public SimplifiedEntryOrganisationDetails(CusUSLVConsignment consignment, OrganisationType organisationType)
		{
			this.consignment = consignment;
			this.organisationType = organisationType;
		}
		readonly CusUSLVConsignment consignment;
		readonly OrganisationType organisationType;

		ZString ISimplifiedEntryOrganisationDetails.EntityCode
		{
			get
			{
				var result = ZString.Empty;
				switch (organisationType)
				{
					case OrganisationType.Seller:
						result = EntityCodeList.Codes.SellingParty;
						break;
					case OrganisationType.Consignee:
						result = EntityCodeList.Codes.Consignee;
						break;
					case OrganisationType.Buyer:
						result = EntityCodeList.Codes.BuyingParty;
						break;
					case OrganisationType.Manufacturer:
						result = EntityCodeList.Codes.ManufacturerSupplier;
						break;
					default:
						break;
				}

				return result;
			}
			set { }
		}

		bool IsSellerOrManufacturer => organisationType == OrganisationType.Seller || organisationType == OrganisationType.Manufacturer;

		ZString ISimplifiedEntryOrganisationDetails.EntityIdentifierQualifier => IsSellerOrManufacturer ? ZString.Empty : consignment.ULB_ConsigneeQualifier;

		ZString ISimplifiedEntryOrganisationDetails.EntityIdentifier => IsSellerOrManufacturer ? ZString.Empty : consignment.ULB_ConsigneeIdentifier;

		ZString IAddressDetails.CompanyName => IsSellerOrManufacturer ? consignment.ULB_SellerName : consignment.ULB_ConsigneeName;

		ZString IAddressDetails.ContactName => ZString.Empty;

		ZString IAddressDetails.Phone => ZString.Empty;

		ZString IAddressDetails.Fax => ZString.Empty;

		ZString IAddressDetails.Email => ZString.Empty;

		ZString IAddressDetails.AddressLine1 => IsSellerOrManufacturer ? consignment.ULB_SellerAddress1 : consignment.ULB_ConsigneeAddress1;

		ZString IAddressDetails.AddressLine2 => IsSellerOrManufacturer ? consignment.ULB_SellerAddress2 : consignment.ULB_ConsigneeAddress2;

		ZString IAddressDetails.City => IsSellerOrManufacturer ? consignment.ULB_SellerCity : consignment.ULB_ConsigneeCity;

		ZString IAddressDetails.State => IsSellerOrManufacturer ? consignment.ULB_SellerState : consignment.ULB_ConsigneeState;

		ZString IAddressDetails.PostCode => IsSellerOrManufacturer ? consignment.ULB_SellerPostCode : consignment.ULB_ConsigneePostCode;

		ZString IAddressDetails.Country => IsSellerOrManufacturer ? consignment.ULB_RN_NKSellerCountry : consignment.ULB_RN_NKConsigneeCountry;

		IEnumerable<(ZString IdentifierType, ZString Identifier)> ISimplifiedEntryOrganisationDetails.GlobalBusinessIdentifiers => Enumerable.Empty<(ZString, ZString)>();
	}
}
