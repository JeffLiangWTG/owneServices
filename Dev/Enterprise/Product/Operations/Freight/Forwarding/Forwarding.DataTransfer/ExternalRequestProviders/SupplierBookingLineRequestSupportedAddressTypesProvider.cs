using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public class SupplierBookingLineRequestSupportedAddressTypesProvider : IExternalRequestSupportedAddressTypesProvider
	{
		public CodeDescriptionPairList SupportedAssigneeAddressTypes
		{
			get
			{
				return supportedAssigneeAddressTypes ?? (supportedAssigneeAddressTypes = new CodeDescriptionPairList()
				{
					new CodeDescriptionPair(DocAddressTypes.Codes.BuyerDocumentaryAddress, DocAddressTypes.Descriptions.BuyerDocumentaryAddress),
					new CodeDescriptionPair(DocAddressTypes.Codes.SupplierDocumentaryAddress, DocAddressTypes.Descriptions.SupplierDocumentaryAddress),
					new CodeDescriptionPair(DocAddressTypes.Codes.ControllingCustomer, DocAddressTypes.Descriptions.ControllingCustomer),
					new CodeDescriptionPair(DocAddressTypes.Codes.Manufacturer, DocAddressTypes.Descriptions.Manufacturer),
					new CodeDescriptionPair(DocAddressTypes.Codes.BookingPartyDocumentaryAddress, DocAddressTypes.Descriptions.BookingPartyDocumentaryAddress)
				});
			}
		}
		CodeDescriptionPairList supportedAssigneeAddressTypes;

		public CodeDescriptionPairList SupportedReviewerAddressTypes
		{
			get
			{
				return supportedReviewerAddressTypes ?? (supportedReviewerAddressTypes = new CodeDescriptionPairList()
				{
					new CodeDescriptionPair(DocAddressTypes.Codes.BuyerDocumentaryAddress, DocAddressTypes.Descriptions.BuyerDocumentaryAddress),
					new CodeDescriptionPair(DocAddressTypes.Codes.SupplierDocumentaryAddress, DocAddressTypes.Descriptions.SupplierDocumentaryAddress),
					new CodeDescriptionPair(DocAddressTypes.Codes.ControllingCustomer, DocAddressTypes.Descriptions.ControllingCustomer),
					new CodeDescriptionPair(DocAddressTypes.Codes.Manufacturer, DocAddressTypes.Descriptions.Manufacturer),
					new CodeDescriptionPair(DocAddressTypes.Codes.BookingPartyDocumentaryAddress, DocAddressTypes.Descriptions.BookingPartyDocumentaryAddress)
				});
			}
		}
		CodeDescriptionPairList supportedReviewerAddressTypes;
	}
}
