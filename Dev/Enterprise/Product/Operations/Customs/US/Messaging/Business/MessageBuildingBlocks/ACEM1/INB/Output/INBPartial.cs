namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output
{
	using CargoWise.Types;

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.FDAPriorNoticeStatusMessage, Constants.ACE)]
	public partial class INBBN01 : MessageBlock, IINBBN01
	{
		#region IINBBN01 members

		ZString IINBBN01.HeaderIdentifier
		{
			get { return HeaderIdentifier; }
		}

		ZString IINBBN01.HeaderIdentifierKey
		{
			get { return HeaderIdentifierKey; }
		}

		#endregion
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.FDAPriorNoticeStatusMessage, Constants.ACE)]
	public partial class INBBN02 : MessageBlock, IStatusesAndErrors, IINBBN02
	{
		#region IStatusesAndErrors

		ZString IStatusesAndErrors.LineNumber
		{
			get { return USCBPLine.ToString(); }
		}

		ZString IStatusesAndErrors.Code
		{
			get { return PriorNoticeLineRejectCode.Trim().Left(3); }
		}

		ZString IStatusesAndErrors.NarrativeMessage
		{
			get { return PriorNoticeLineMessage; }
		}

		ZString IStatusesAndErrors.ReferenceNumber
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region IINBBN02 members

		ZInt IINBBN02.FDALine { get { return FDALine; } }
		ZInt IINBBN02.CBPLine { get { return USCBPLine; } }
		ZString IINBBN02.PriorNoticeLineRejectCode { get { return PriorNoticeLineRejectCode; } }
		ZString IINBBN02.PriorNoticeConfirmationNumber { get { return PriorNoticeConfirmationNumber; } }
		ZDate IINBBN02.PriorNoticeClockStartDate { get { return PriorNoticeClockStartDate; } }
		ZString IINBBN02.PriorNoticeClockStartTime { get { return PriorNoticeClockStartTime; } }

		#endregion
	}

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.InbondTransactionResponse, Constants.ACE)]
	public partial class INBQT95 : MessageBlock, IINBQT95
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

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.InbondUpdateTransferofLiabilityResponse, Constants.ACE)]
	public partial class INBWT95 : MessageBlock, IINBWT95
	{
		#region IINBWT95

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

		ZString IINBWT95.NarrativeMessageTypeCode
		{
			get { return NarrativeMessageTypeCode; }
		}

		#endregion
	}
}
