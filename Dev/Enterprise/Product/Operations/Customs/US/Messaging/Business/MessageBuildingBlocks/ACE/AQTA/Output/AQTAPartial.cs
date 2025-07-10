namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output
{
	using CargoWise.Types;

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.QuotaQueryResponse)]
	public partial class AQTAQ2 : MessageBlock
	{
		protected override void AdjustEndDates()
		{
			EndingPeriodDate = AdjustedEndDate(BeginningPeriodDate, EndingPeriodDate);
		}
	}

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.QuotaQueryResponse)]
	public partial class AQTAQ3 : MessageBlock { }

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.QuotaQueryResponse)]
	public partial class AQTAQ4 : MessageBlock { }

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.QuotaQueryResponse)]
	public partial class AQTAQ5 : MessageBlock, IStatusesAndErrors
	{
		#region IStatusesAndErrors

		ZString IStatusesAndErrors.LineNumber
		{
			get { return ZString.Empty; }
		}

		ZString IStatusesAndErrors.Code
		{
			get { return ConditionCode; }
		}

		ZString IStatusesAndErrors.NarrativeMessage
		{
			get { return NarrativeText; }
		}

		ZString IStatusesAndErrors.ReferenceNumber
		{
			get { return ZString.Empty; }
		}

		#endregion
	}
}
