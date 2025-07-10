namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output
{
	using CargoWise.Types;

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BorderCargoReleaseResponse)]
	partial class BCRE0102 : MessageBlock, IStatusesAndErrors, ICargoReleaseStatus
	{
		#region IStatusesAndErrors

		ZString IStatusesAndErrors.LineNumber
		{
			get { return ZString.Empty; }
		}

		ZString IStatusesAndErrors.Code
		{
			get { return MessageCode; }
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

		ZString ICargoReleaseStatus.Status
		{
			get { return MessageCode; }
		}

		ZString ICargoReleaseStatus.NarrativeMessage
		{
			get { return NarrativeMessage; }
		}

		ZString ICargoReleaseStatus.ErrorIdentifierCode
		{
			get { return ZString.Empty; }
		}
	}
}
