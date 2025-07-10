using CargoWise.Types;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output
{
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PriorNoticeStatusNotification)]
	public partial class SESNPO10
	{
	}

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PriorNoticeStatusNotification)]
	public partial class SESNPO70 : IStatusesAndErrors, IPGADispositionProvider
	{
		#region IStatusesAndErrors Members

		ZString IStatusesAndErrors.LineNumber
		{
			get { return PGALine; }
		}

		ZString IStatusesAndErrors.NarrativeMessage
		{
			get { return ZString.Empty; }
		}

		ZString IStatusesAndErrors.Code
		{
			get { return PGALineLevelStatusCode; }
		}

		ZString IStatusesAndErrors.ReferenceNumber
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region IPGADispositionProvider
		ZString IPGADispositionProvider.GovernmentAgencyProgramCode
		{
			get { return GovernmentAgencyProgramCode; }
		}

		ZString IPGADispositionProvider.BeginningCBPLineNo
		{
			get { return ZString.Empty; }
		}

		ZString IPGADispositionProvider.BeginningOGALineNo
		{
			get { return PGALine; }
		}

		ZString IPGADispositionProvider.PGALineDispositionCode
		{
			get { return PGALineLevelStatusCode; }
		}

		ZString IPGADispositionProvider.ReviewReasonCode
		{
			get { return StatusReasonCode; }
		}

		ZDateTime IPGADispositionProvider.DispositionDateTime
		{
			get
			{
				return ZDateTime.Empty;
			}
		}

		ZString IPGADispositionProvider.EntryDispositionCode
		{
			get { return ZString.Empty; }
		}

		ZString IPGADispositionProvider.EntryLineDispositionCode
		{
			get { return ZString.Empty; }
		}

		ZString IPGADispositionProvider.EndingCBPLineNo
		{
			get { return ZString.Empty; }
		}

		ZString IPGADispositionProvider.EndingOGALineNo
		{
			get { return ZString.Empty; }
		}

		ZString IPGADispositionProvider.NarrativeMessage
		{
			get { return ZString.Empty; }
		}

		ZString IPGADispositionProvider.OtherAgencyQuotaIdentifier
		{
			get { return GovernmentAgencyCode; }
		}

		ZString IPGADispositionProvider.RangeIndicator
		{
			get { return ZString.Empty; }
		}

		ZString IPGADispositionProvider.DocumentTypeCode
		{
			get { return ZString.Empty; }
		}

		ZString IPGADispositionProvider.PGAEntryHoldType
		{
			get { return ZString.Empty; }
		}

		ZString IPGADispositionProvider.BeginningTariffPosition
		{
			get { return ZString.Empty; }
		}

		ZString IPGADispositionProvider.EndingTariffPosition
		{
			get { return ZString.Empty; }
		}

		ZString IPGADispositionProvider.PGAProcessingGroupVersion
		{
			get { return ZString.Empty; }
		}

		#endregion
	}

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PriorNoticeStatusNotification)]
	public partial class SESNPO72
	{
	}
}
