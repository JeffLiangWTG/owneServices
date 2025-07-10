namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output
{
	using CargoWise.Types;

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BillofLadingUpdateResponse)]
	public partial class BOLL7 : MessageBlock, IStatusesAndErrors
	{
		#region IStatusesAndErrors
		ZString IStatusesAndErrors.LineNumber
		{
			get { return ZString.Empty; }
		}

		ZString IStatusesAndErrors.Code
		{
			get { return ErrorMessageIdentifier; }
		}

		ZString IStatusesAndErrors.NarrativeMessage
		{
			get { return NarrativeMessage; }
		}

		ZString IStatusesAndErrors.ReferenceNumber
		{
			get { return ZString.Empty; }
		}

		#endregion
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BillofLadingProcessingResults)]
	public partial class BOLP1 : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BillofLadingProcessingResults)]
	public partial class BOLP3 : MessageBlock { }
}