namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output
{
	using CargoWise.Types;

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.QueryQuotaResponse)]
	public partial class QTAU0 : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.QueryQuotaResponse)]
	public partial class QTAU2 : MessageBlock
	{
		protected override void AdjustEndDates()
		{
			EndingPeriodDate = AdjustedEndDate(BeginningPeriodDate, EndingPeriodDate);
		}
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.QueryQuotaResponse)]
	public partial class QTAU3 : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.QueryQuotaResponse)]
	public partial class QTAU4 : MessageBlock, IStatusesAndErrors
	{
		#region IStatusesAndErrors

		ZString IStatusesAndErrors.LineNumber
		{
			get { return ZString.Empty; }
		}

		ZString IStatusesAndErrors.Code
		{
			get { return ZString.Empty; }
		}

		ZString IStatusesAndErrors.NarrativeMessage
		{
			get { return ErrorMessage; }
		}

		ZString IStatusesAndErrors.ReferenceNumber
		{
			get { return ZString.Empty; }
		}

		#endregion
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.QueryQuotaResponse)]
	public partial class QTAU5 : MessageBlock
	{
		protected override void AdjustEndDates()
		{
			ExpirationDate = AdjustedEndDate(EffectiveDate, ExpirationDate);
		}
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.QueryQuotaResponse)]
	public partial class QTAU6 : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.QueryQuotaResponse)]
	public partial class QTAU7 : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.QueryQuotaResponse)]
	public partial class QTAU8 : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.QueryQuotaResponse)]
	public partial class QTAU9 : MessageBlock, IStatusesAndErrors
	{
		#region IStatusesAndErrors

		ZString IStatusesAndErrors.LineNumber
		{
			get { return ZString.Empty; }
		}

		ZString IStatusesAndErrors.Code
		{
			get { return ZString.Empty; }
		}

		ZString IStatusesAndErrors.NarrativeMessage
		{
			get { return ErrorMessage; }
		}

		ZString IStatusesAndErrors.ReferenceNumber
		{
			get { return ZString.Empty; }
		}

		#endregion
	}
}