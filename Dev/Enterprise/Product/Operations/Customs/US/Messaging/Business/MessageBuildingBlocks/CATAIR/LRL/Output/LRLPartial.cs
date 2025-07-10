namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output
{
	using CargoWise.Types;

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.LineRelease)]
	partial class LRLX10 : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.LineRelease)]
	partial class LRLX20 : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.LineRelease)]
	partial class LRLX25 : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.LineRelease)]
	partial class LRLX30 : MessageBlock, IStatusesAndErrors
	{
		#region IStatusesAndErrors

		ZString IStatusesAndErrors.LineNumber
		{
			get { return ZString.Empty; }
		}

		ZString IStatusesAndErrors.Code
		{
			get { return ErrorType; }
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

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.LineRelease)]
	partial class LRLX40 : MessageBlock { }
}
