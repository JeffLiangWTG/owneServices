using CargoWise.Types;
namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output
{
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.StatusNotification, Constants.ACE)]
	public partial class STSB04 : MessageBlock, IINPB04
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

	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.StatusNotification, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.StatusNotification, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	public partial class STSC01 : MessageBlock, IAMSContainer
	{
		#region IAMSContainer Members

		ZString IAMSContainer.ContainerNumber
		{
			get { return ContainerEquipmentNo; }
		}

		ZString IAMSContainer.SealNumber1
		{
			get { return SealNumber1; }
		}

		ZString IAMSContainer.SealNumber2
		{
			get { return SealNumber2; }
		}

		#endregion
	}

	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.StatusNotification, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.StatusNotification, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	public partial class STSC02 : MessageBlock, IAMSContainerDetail
	{
		#region IAMSContainerDetail Members

		ZString IAMSContainerDetail.VIN
		{
			get { return VIN; }
		}

		ZString IAMSContainerDetail.ContainerOperatorLine
		{
			get { return ContainerOperatorLine; }
		}

		ZString IAMSContainerDetail.ForeignPort
		{
			get { return ForeignPort; }
		}

		ZString IAMSContainerDetail.FactoryCarOrderNumber
		{
			get { return FactoryCarOrderNumber; }
		}

		#endregion
	}

	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.StatusNotification, Constants.ACE)]
	public partial class STSJ01 : MessageBlock, IINPJ01
	{
		#region IINPJ01 Members

		ZString IINPJ01.IssuerCode
		{
			get { return IssuerCode; }
		}

		#endregion
	}

	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.StatusNotification, Constants.ACE)]
	public partial class STSM01 : MessageBlock, IINPM01
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

	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.StatusNotification, Constants.ACE)]
	public partial class STSR01 : MessageBlock, IOUTR01
	{
		#region IOUTR01 Members

		ZString IOUTR01.CarrierCode
		{
			get { return CarrierCode; }
		}

		ZString IOUTR01.CBPDistrictPort
		{
			get { return CBPPort; }
		}

		ZString IOUTR01.VesselName
		{
			get { return VesselName; }
		}

		ZString IOUTR01.VoyageNumber
		{
			get { return VoyageNumber; }
		}

		ZString IOUTR01.ManifestSequenceNumber
		{
			get { return ManifestSequenceNumber; }
		}

		ZDate IOUTR01.EstimatedDate
		{
			get { return EstimatedDate; }
		}

		#endregion
	}

	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.StatusNotification, Constants.ACE)]
	public partial class STSR02Combine : MessageBlock, IOUTR02Combine
	{
		#region IOUTR02Combine Members

		ZString IOUTR02Combine.Data
		{
			get { return Data; }
		}

		#endregion
	}

	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.StatusNotification, Constants.ACE)]
	public partial class STSR03 : MessageBlock, IOUTR03
	{
		#region IOUTR03 Members

		ZString IOUTR03.Remarks
		{
			get { return Remarks; }
		}

		#endregion
	}

	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.StatusNotification, Constants.ACE)]
	public partial class STSR05 : MessageBlock, IAMSContainer
	{
		#region IAMSContainer Members

		ZString IAMSContainer.ContainerNumber
		{
			get { return ContainerNumber; }
		}

		ZString IAMSContainer.SealNumber1
		{
			get { return SealNumber1; }
		}

		ZString IAMSContainer.SealNumber2
		{
			get { return SealNumber2; }
		}

		#endregion
	}

	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.StatusNotification, Constants.ACE)]
	public partial class STSR06 : MessageBlock, IOUTR06
	{
		#region IOUTR06 Members

		ZString IOUTR06.EventCode
		{
			get { return EventCode; }
		}

		ZDate IOUTR06.ActionDate
		{
			get { return ActionDate; }
		}

		ZString IOUTR06.ActionTime
		{
			get { return ActionTime; }
		}

		#endregion
	}
}
