namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output
{
	using CargoWise.Types;

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ReconciliationEntryFilingResponse)]
	public partial class RECR01 : MessageBlock, IStatusesAndErrors
	{
		#region IStatusesAndErrors

		ZString IStatusesAndErrors.LineNumber
		{
			get { return ZString.Empty; }
		}

		ZString IStatusesAndErrors.Code
		{
			get { return ErrorCode; }
		}

		ZString IStatusesAndErrors.NarrativeMessage
		{
			get { return ErrorOrFinalMessage; }
		}

		ZString IStatusesAndErrors.ReferenceNumber
		{
			get { return ZString.Empty; }
		}

		#endregion
	}
}
