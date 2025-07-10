using CargoWise.Types;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output
{
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.AirInbond)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.AirInbondResponse)]
	public partial class AIRXT95 : MessageBlock, IINBQT95
	{
		#region IINBQT95

		ZString IStatusesAndErrors.LineNumber
		{
			get { return ZString.Empty; }
		}

		ZString IStatusesAndErrors.Code
		{
			get { return NarrativeMessageIdentifier; }
		}

		ZString IStatusesAndErrors.NarrativeMessage
		{
			get { return NarrativeMessage; }
		}

		ZString IStatusesAndErrors.ReferenceNumber
		{
			get { return ZString.Empty; }
		}

		ZString IINBQT95.NarrativeMessageTypeCode
		{
			get { return NarrativeMessageTypeCode; }
		}

		#endregion
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.AirInbondUpdateTransferOfLiabilityPriorNotice)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.AirInbondUpdateTransferOfLiabilityPriorNoticeResponse)]
	public partial class AIRYT95 : MessageBlock { }
}