using System;
using System.Collections.ObjectModel;
using CargoWise.Types;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common
{
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestCreate, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestCreateTransmissionResponse, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestEdit, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestEditResponse, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestAmendment, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestAmendmentResponse, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.SubsequentInBond, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.SubsequentInBondResponse, Constants.ACE)]
	public partial class INPA01 : MessageBlock
	{
	}

	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestCreate, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestCreateTransmissionResponse, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestEdit, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestEditResponse, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestAmendment, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestAmendmentResponse, Constants.ACE)]
	public partial class INPB01 : MessageBlock
	{
	}

	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestCreate, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestCreateTransmissionResponse, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestEdit, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestEditResponse, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestAmendment, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestAmendmentResponse, Constants.ACE)]
	public partial class INPB02 : MessageBlock
	{
	}

	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestCreate, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestCreateTransmissionResponse, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestEdit, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestEditResponse, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestAmendment, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestAmendmentResponse, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.SubsequentInBond, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.SubsequentInBondResponse, Constants.ACE)]
	public partial class INPB04 : MessageBlock, IINPB04
	{
		#region IINPB04 Members

		ZString IINPB04.ReferenceIdentifierQualifier
		{
			get { return ReferenceIdentifierQualifier; }
		}

		ZString IINPB04.ReferenceIdentifier
		{
			get { return ReferenceIdentifier; }
		}

		#endregion
	}

	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestCreate, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestCreateTransmissionResponse, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestEdit, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestEditResponse, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestAmendment, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestAmendmentResponse, Constants.ACE)]
	public partial class INPC01 : MessageBlock
	{
	}

	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestCreate, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestCreateTransmissionResponse, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestEdit, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestEditResponse, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestAmendment, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestAmendmentResponse, Constants.ACE)]
	public partial class INPC02 : MessageBlock
	{
	}

	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestCreate, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestCreateTransmissionResponse, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestEdit, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestEditResponse, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestAmendment, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestAmendmentResponse, Constants.ACE)]
	public partial class INPD00 : MessageBlock
	{
	}

	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestCreate, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestCreateTransmissionResponse, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestEdit, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestEditResponse, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestAmendment, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestAmendmentResponse, Constants.ACE)]
	public partial class INPD01 : MessageBlock
	{
	}

	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestCreate, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestCreateTransmissionResponse, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestEdit, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestEditResponse, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestAmendment, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestAmendmentResponse, Constants.ACE)]
	public partial class INPD02 : MessageBlock
	{
	}

	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestCreate, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestCreateTransmissionResponse, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestEdit, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestEditResponse, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestAmendment, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestAmendmentResponse, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.SubsequentInBond, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.SubsequentInBondResponse, Constants.ACE)]
	public partial class INPI01 : MessageBlock
	{
	}

	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestCreate, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestCreateTransmissionResponse, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestEdit, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestEditResponse, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestAmendment, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestAmendmentResponse, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.SubsequentInBond, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.SubsequentInBondResponse, Constants.ACE)]
	public partial class INPI02 : MessageBlock
	{
	}

	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestCreate, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestCreateTransmissionResponse, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestEdit, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestEditResponse, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestAmendment, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestAmendmentResponse, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.PermitToTransfer, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.PermitToTransferResponse, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.PaperlessInBondOrVesselArrival, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.PaperlessInBondOrVesselArrivalResponse, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.SubsequentInBond, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.SubsequentInBondResponse, Constants.ACE)]
	public partial class INPJ01 : MessageBlock, IINPJ01
	{
		#region IINPJ01 Members

		ZString IINPJ01.IssuerCode
		{
			get { return IssuerCode; }
		}

		#endregion
	}

	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestCreate, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestCreateTransmissionResponse, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestEditResponse, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestAmendment, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestAmendmentResponse, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestEdit, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.PermitToTransfer, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.PermitToTransferResponse, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.PaperlessInBondOrVesselArrival, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.PaperlessInBondOrVesselArrivalResponse, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.SubsequentInBond, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.SubsequentInBondResponse, Constants.ACE)]
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
			get { return VesselName; }
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
			get { return VesselCountryCode; }
		}
	}

	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestCreate, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestCreateTransmissionResponse, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestEdit, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestEditResponse, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestAmendment, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestAmendmentResponse, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.PermitToTransfer, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.PermitToTransferResponse, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.PaperlessInBondOrVesselArrival, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.PaperlessInBondOrVesselArrivalResponse, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.SubsequentInBond, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.SubsequentInBondResponse, Constants.ACE)]
	public partial class INPM02 : MessageBlock, IINPM02
	{
		ZString IINPM02.CarrierAssignedBatchNumber
		{
			get { return CarrierAssignedBatchNumber; }
		}
	}

	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestCreate, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestCreateTransmissionResponse, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestEdit, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestEditResponse, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestAmendment, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestAmendmentResponse, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.SubsequentInBond, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.SubsequentInBondResponse, Constants.ACE)]
	public partial class INPN00 : MessageBlock
	{
	}

	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestCreate, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestCreateTransmissionResponse, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestEdit, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestEditResponse, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestAmendment, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestAmendmentResponse, Constants.ACE)]
	public partial class INPN01 : MessageBlock
	{
	}

	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestCreate, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestCreateTransmissionResponse, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestEdit, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestEditResponse, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestAmendment, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestAmendmentResponse, Constants.ACE)]
	public partial class INPN02 : MessageBlock
	{
	}

	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestCreate, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestCreateTransmissionResponse, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestEdit, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestEditResponse, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestAmendment, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestAmendmentResponse, Constants.ACE)]
	public partial class INPN03 : MessageBlock, ISerialiserSupporter
	{
		#region ISerialiserWithOrigAppID Members

		string ISerialiserSupporter.Serialise(bool humanFriendly, string outgoingApplicationIdentifier, ReadOnlyCollection<MessageBlock> messageBlocks)
		{
			MessageBlock messageBlock = this;
			var index = messageBlocks.IndexOf(this);
			if (index > 0)
			{
				for (int i = index - 1; i >= Math.Max(0, index - 2); i--)
				{
					var previousMessageBlock = messageBlocks[i];
					if (previousMessageBlock is INPN01)
					{
						messageBlock = new INPN03ForN01();
						messageBlock.Deserialise(this.Serialise());
						break;
					}
				}
			}
			return messageBlock.Serialise(humanFriendly);
		}

		#endregion
	}

	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestCreate, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestCreateTransmissionResponse, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestEdit, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestEditResponse, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestAmendment, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestAmendmentResponse, Constants.ACE)]
	public partial class INPN04 : MessageBlock
	{
	}

	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestCreate, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestCreateTransmissionResponse, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestEdit, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestEditResponse, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestAmendment, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestAmendmentResponse, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.PermitToTransfer, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.PermitToTransferResponse, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.PaperlessInBondOrVesselArrival, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.PaperlessInBondOrVesselArrivalResponse, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.SubsequentInBond, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.SubsequentInBondResponse, Constants.ACE)]
	public partial class INPP01 : MessageBlock, IINPP01
	{
		#region IINPP01 Members

		ZString IINPP01.PortOfUnlading
		{
			get { return PortOfUnladingCode; }
		}

		public ZDate EstimatedDate
		{
			get { return OriginalEstimatedDate; }
		}

		#endregion
	}

	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestCreate, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestCreateTransmissionResponse, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestEdit, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestEditResponse, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestAmendment, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestAmendmentResponse, Constants.ACE)]
	public partial class INPS01 : MessageBlock
	{
	}

	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestCreate, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestCreateTransmissionResponse, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestEdit, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestEditResponse, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestAmendment, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestAmendmentResponse, Constants.ACE)]
	public partial class INPS02 : MessageBlock
	{
	}

	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestCreate, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestCreateTransmissionResponse, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestEdit, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestEditResponse, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestAmendment, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestAmendmentResponse, Constants.ACE)]
	public partial class INPS03 : MessageBlock
	{
	}

	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestCreate, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestCreateTransmissionResponse, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestEdit, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestEditResponse, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestAmendment, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestAmendmentResponse, Constants.ACE)]
	public partial class INPU01 : MessageBlock
	{
	}

	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestCreate, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestCreateTransmissionResponse, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestEdit, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestEditResponse, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestAmendment, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestAmendmentResponse, Constants.ACE)]
	public partial class INPU02 : MessageBlock
	{
	}

	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestCreate, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestCreateTransmissionResponse, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestEdit, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestEditResponse, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestAmendment, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestAmendmentResponse, Constants.ACE)]
	public partial class INPU03 : MessageBlock
	{
	}

	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestCreate, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestCreateTransmissionResponse, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestEdit, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestEditResponse, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestAmendment, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestAmendmentResponse, Constants.ACE)]
	public partial class INPV01 : MessageBlock
	{
	}

	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestCreate, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestCreateTransmissionResponse, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestEdit, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestEditResponse, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestAmendment, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestAmendmentResponse, Constants.ACE)]
	public partial class INPV02 : MessageBlock
	{
	}

	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestCreate, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestCreateTransmissionResponse, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestEdit, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestEditResponse, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestAmendment, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestAmendmentResponse, Constants.ACE)]
	public partial class INPV03 : MessageBlock
	{
	}
}
