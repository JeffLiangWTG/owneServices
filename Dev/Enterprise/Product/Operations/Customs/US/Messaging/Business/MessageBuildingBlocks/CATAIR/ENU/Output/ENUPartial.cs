namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output
{
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.EntryDateUpdateTransactionResponse)]
	public partial class ENUDN02 : MessageBlock, IStatusesAndErrors
	{
		#region IStatusesAndErrors

		CargoWise.Types.ZString IStatusesAndErrors.LineNumber
		{
			get { return CargoWise.Types.ZString.Empty; }
		}

		CargoWise.Types.ZString IStatusesAndErrors.Code
		{
			get { return MessageCode; }
		}

		CargoWise.Types.ZString IStatusesAndErrors.NarrativeMessage
		{
			get { return NarrativeMessage; }
		}

		CargoWise.Types.ZString IStatusesAndErrors.ReferenceNumber
		{
			get { return CargoWise.Types.ZString.Empty; }
		}

		#endregion
	}
}