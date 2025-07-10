namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output
{
	using CargoWise.Types;

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ProtestInitialFiling)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ProtestAmendment)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ProtestAddenda)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ProtestServiceRequest)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ProtestInitialFilingResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ProtestAmendmentResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ProtestAddendaResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ProtestServiceRequestResponse)]
	partial class PROP01 : MessageBlock, IStatusesAndErrors
	{
		#region IStatusesAndErrors

		ZString IStatusesAndErrors.LineNumber
		{
			get { return ZString.Empty; }
		}

		ZString IStatusesAndErrors.Code
		{
			get { return ResponseCode; }
		}

		ZString IStatusesAndErrors.NarrativeMessage
		{
			get { return ResponseMessage; }
		}

		ZString IStatusesAndErrors.ReferenceNumber
		{
			get { return ZString.Empty; }
		}

		#endregion
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ProtestInitialFiling)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ProtestAmendment)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ProtestAddenda)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ProtestServiceRequest)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ProtestInitialFilingResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ProtestAmendmentResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ProtestAddendaResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ProtestServiceRequestResponse)]
	partial class PROP99 : MessageBlock, IStatusesAndErrors
	{
		#region IStatusesAndErrors

		ZString IStatusesAndErrors.LineNumber
		{
			get { return ZString.Empty; }
		}

		ZString IStatusesAndErrors.Code
		{
			get { return ResponseCode; }
		}

		ZString IStatusesAndErrors.NarrativeMessage
		{
			get { return ResponseMessage; }
		}

		ZString IStatusesAndErrors.ReferenceNumber
		{
			get { return ZString.Empty; }
		}

		#endregion
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ProtestInitialFiling)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ProtestAmendment)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ProtestAddenda)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ProtestServiceRequest)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ProtestInitialFilingResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ProtestAmendmentResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ProtestAddendaResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ProtestServiceRequestResponse)]
	partial class PROPER : MessageBlock, IStatusesAndErrors
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
			get { return ErrorMessage; }
		}

		ZString IStatusesAndErrors.ReferenceNumber
		{
			get { return ZString.Empty; }
		}

		#endregion
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ProtestAutomaticNotificationandResponsetoFilerQuery)]
	partial class PROP12SSSS : MessageBlock, IStatusesAndErrors
	{
		#region IStatusesAndErrors

		ZString IStatusesAndErrors.LineNumber
		{
			get { return ZString.Empty; }
		}

		ZString IStatusesAndErrors.Code
		{
			get { return ProcessingStatusCode; }
		}

		ZString IStatusesAndErrors.NarrativeMessage
		{
			get { return ProcessingStatusDescription; }
		}

		ZString IStatusesAndErrors.ReferenceNumber
		{
			get { return ZString.Empty; }
		}

		#endregion
	}
}
