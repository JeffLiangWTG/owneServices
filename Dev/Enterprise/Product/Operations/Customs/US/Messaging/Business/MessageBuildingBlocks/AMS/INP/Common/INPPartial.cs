
using CargoWise.Types;
namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.AMS.Common
{
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestCreate, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestCreateTransmissionResponse, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestEdit, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestEditResponse, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestAmendment, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestAmendmentResponse, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	public partial class INPB01 : MessageBlock
	{
	}

	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestCreate, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestCreateTransmissionResponse, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestEdit, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestEditResponse, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestAmendment, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestAmendmentResponse, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	public partial class INPB02 : MessageBlock
	{
	}

	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestCreate, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestCreateTransmissionResponse, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestEdit, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestEditResponse, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestAmendment, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestAmendmentResponse, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	public partial class INPB04 : MessageBlock, IINPB04
	{
		#region IINPB04 Members

		ZString IINPB04.ReferenceIdentifierQualifier
		{
			get { return Qualifier; }
		}

		ZString IINPB04.ReferenceIdentifier
		{
			get { return ReferenceIdentifier; }
		}

		#endregion
	}

	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestCreate, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestCreateTransmissionResponse, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestEdit, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestEditResponse, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestAmendment, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestAmendmentResponse, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	public partial class INPB05 : MessageBlock
	{
	}

	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestCreate, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestCreateTransmissionResponse, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestEdit, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestEditResponse, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestAmendment, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestAmendmentResponse, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	public partial class INPC01 : MessageBlock
	{
	}

	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestCreate, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestCreateTransmissionResponse, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestEdit, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestEditResponse, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestAmendment, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestAmendmentResponse, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	public partial class INPC02 : MessageBlock
	{
	}

	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestCreate, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestCreateTransmissionResponse, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestEdit, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestEditResponse, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestAmendment, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestAmendmentResponse, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	public partial class INPD00 : MessageBlock
	{
	}

	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestCreate, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestCreateTransmissionResponse, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestEdit, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestEditResponse, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestAmendment, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestAmendmentResponse, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	public partial class INPD01 : MessageBlock
	{
	}

	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestCreate, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestCreateTransmissionResponse, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestEdit, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestEditResponse, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestAmendment, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestAmendmentResponse, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	public partial class INPD02 : MessageBlock
	{
	}

	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestCreate, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestCreateTransmissionResponse, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestEdit, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestEditResponse, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestAmendment, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestAmendmentResponse, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	public partial class INPI01 : MessageBlock
	{
	}

	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestCreate, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestCreateTransmissionResponse, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestEdit, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestEditResponse, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestAmendment, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestAmendmentResponse, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	public partial class INPI02 : MessageBlock
	{
	}

	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestCreate, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestCreateTransmissionResponse, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestEdit, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestEditResponse, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestAmendment, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestAmendmentResponse, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.StatusNotification, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.PermitToTransfer, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.PermitToTransferResponse, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	public partial class INPJ01 : MessageBlock, IINPJ01
	{
		#region IINPJ01 Members

		ZString IINPJ01.IssuerCode
		{
			get { return IssuerCode; }
		}

		#endregion
	}

	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestCreate, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestCreateTransmissionResponse, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestEditResponse, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestAmendment, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestAmendmentResponse, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestEdit, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.StatusNotification, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.PermitToTransfer, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.PermitToTransferResponse, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	public partial class INPM01 : MessageBlock, IINPM01
	{
		ZString IINPM01.CarrierCode
		{
			get { return CarrierCode; }
		}

		ZString IINPM01.ManifestSequenceNumber
		{
			get { return ManifestSequenceNumber; }
		}

		ZString IINPM01.VesselName
		{
			get { return ConveyanceName; }
		}

		ZString IINPM01.VoyageNumber
		{
			get { return VoyageNumber; }
		}

		ZString IINPM01.ModeOfTransportation
		{
			get { return ModeOfTransportationCode; }
		}

		ZString IINPM01.VesselCountry
		{
			get { return ConveyanceCountryCode; }
		}
	}

	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestCreate, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestCreateTransmissionResponse, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestEdit, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestEditResponse, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestAmendment, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestAmendmentResponse, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.PermitToTransfer, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.PermitToTransferResponse, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	public partial class INPM02 : MessageBlock, IINPM02
	{
		ZString IINPM02.CarrierAssignedBatchNumber
		{
			get { return CarrierAssignedBatchNumber; }
		}
	}

	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestCreate, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestCreateTransmissionResponse, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestEdit, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestEditResponse, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestAmendment, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestAmendmentResponse, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	public partial class INPN00 : MessageBlock
	{
	}

	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestCreate, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestCreateTransmissionResponse, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestEdit, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestEditResponse, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestAmendment, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestAmendmentResponse, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	public partial class INPN01 : MessageBlock
	{
	}

	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestCreate, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestCreateTransmissionResponse, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestEdit, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestEditResponse, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestAmendment, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestAmendmentResponse, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	public partial class INPN02 : MessageBlock
	{
	}

	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestCreate, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestCreateTransmissionResponse, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestEdit, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestEditResponse, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestAmendment, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestAmendmentResponse, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	public partial class INPN03 : MessageBlock
	{
	}

	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestCreate, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestCreateTransmissionResponse, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestEdit, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestEditResponse, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestAmendment, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestAmendmentResponse, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	public partial class INPN04 : MessageBlock
	{
	}

	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestCreate, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestCreateTransmissionResponse, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestEdit, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestEditResponse, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestAmendment, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestAmendmentResponse, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.PermitToTransfer, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.PermitToTransferResponse, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	public partial class INPP01 : MessageBlock, IINPP01
	{
		#region IINPP01 Members

		ZString IINPP01.PortOfUnlading
		{
			get { return DistrictPortOfUnladingCode; }
		}

		public ZDate EstimatedDate
		{
			get { return OriginalEstimatedDate; }
		}

		#endregion
	}

	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestCreate, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestCreateTransmissionResponse, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestEdit, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestEditResponse, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestAmendment, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestAmendmentResponse, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	public partial class INPS01 : MessageBlock
	{
	}

	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestCreate, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestCreateTransmissionResponse, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestEdit, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestEditResponse, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestAmendment, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestAmendmentResponse, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	public partial class INPS02 : MessageBlock
	{
	}

	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestCreate, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestCreateTransmissionResponse, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestEdit, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestEditResponse, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestAmendment, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestAmendmentResponse, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	public partial class INPS03 : MessageBlock
	{
	}

	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestCreate, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestCreateTransmissionResponse, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestEdit, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestEditResponse, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestAmendment, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestAmendmentResponse, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	public partial class INPU01 : MessageBlock
	{
	}

	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestCreate, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestCreateTransmissionResponse, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestEdit, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestEditResponse, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestAmendment, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestAmendmentResponse, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	public partial class INPU02 : MessageBlock
	{
	}

	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestCreate, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestCreateTransmissionResponse, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestEdit, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestEditResponse, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestAmendment, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestAmendmentResponse, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	public partial class INPU03 : MessageBlock
	{
	}

	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestCreate, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestCreateTransmissionResponse, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestEdit, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestEditResponse, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestAmendment, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestAmendmentResponse, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	public partial class INPV01 : MessageBlock
	{
	}

	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestCreate, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestCreateTransmissionResponse, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestEdit, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestEditResponse, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestAmendment, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestAmendmentResponse, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	public partial class INPV02 : MessageBlock
	{
	}

	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestCreate, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestCreateTransmissionResponse, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestEdit, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestEditResponse, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestAmendment, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestAmendmentResponse, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	public partial class INPV03 : MessageBlock
	{
	}
}