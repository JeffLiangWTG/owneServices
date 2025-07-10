using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.DataTransfer;

public class ShipmentRequestSupportedAddressTypesProvider : IExternalRequestSupportedAddressTypesProvider
{
	public CodeDescriptionPairList SupportedAssigneeAddressTypes
	{
		get
		{
			return supportedAssigneeAddressTypes ?? (supportedAssigneeAddressTypes = new CodeDescriptionPairList()
				{
					new CodeDescriptionPair(DocAddressTypes.Codes.ConsigneeDocumentaryAddress, DocAddressTypes.Descriptions.ConsigneeDocumentaryAddress),
					new CodeDescriptionPair(DocAddressTypes.Codes.ConsignorDocumentaryAddress, DocAddressTypes.Descriptions.ConsignorDocumentaryAddress),
					new CodeDescriptionPair(DocAddressTypes.Codes.ControllingCustomer, DocAddressTypes.Descriptions.ControllingCustomer),
					new CodeDescriptionPair(DocAddressTypes.Codes.LocalClient, DocAddressTypes.Descriptions.LocalClient),
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
					new CodeDescriptionPair(DocAddressTypes.Codes.ConsigneeDocumentaryAddress, DocAddressTypes.Descriptions.ConsigneeDocumentaryAddress),
					new CodeDescriptionPair(DocAddressTypes.Codes.ConsignorDocumentaryAddress, DocAddressTypes.Descriptions.ConsignorDocumentaryAddress),
					new CodeDescriptionPair(DocAddressTypes.Codes.ControllingCustomer, DocAddressTypes.Descriptions.ControllingCustomer),
					new CodeDescriptionPair(DocAddressTypes.Codes.LocalClient, DocAddressTypes.Descriptions.LocalClient),
				});
		}
	}
	CodeDescriptionPairList supportedReviewerAddressTypes;
}
