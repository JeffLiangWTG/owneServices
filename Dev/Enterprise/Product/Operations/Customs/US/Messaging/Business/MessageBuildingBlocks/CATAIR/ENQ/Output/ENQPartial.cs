namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output.Abstract
{
	using CargoWise.Types;

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.QueryEntrySummaryResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BIRDTransaction)]
	[InputBlock("J0")]
	public partial class ENQJ0 : MessageBlock, IStatusesAndErrors
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
			get { return NarrativeMessage; }
		}

		ZString IStatusesAndErrors.ReferenceNumber
		{
			get { return ZString.Empty; }
		}

		#endregion
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.QueryEntrySummaryResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BIRDTransaction)]
	[InputBlock("J1")]
	public partial class ENQJ1 : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.QueryEntrySummaryResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BIRDTransaction)]
	[InputBlock("J2")]
	public partial class ENQJ2 : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.QueryEntrySummaryResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BIRDTransaction)]
	[InputBlock("J3")]
	public partial class ENQJ3 : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.QueryEntrySummaryResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BIRDTransaction)]
	[InputBlock("J4")]
	public partial class ENQJ4 : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.QueryEntrySummaryResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BIRDTransaction)]
	[InputBlock("J5")]
	public partial class ENQJ5 : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.QueryEntrySummaryResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BIRDTransaction)]
	[InputBlock("J6")]
	public partial class ENQJ6 : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.QueryEntrySummaryResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BIRDTransaction)]
	[InputBlock("J7")]
	public partial class ENQJ7 : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.QueryEntrySummaryResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BIRDTransaction)]
	[InputBlock("J8")]
	public partial class ENQJ8 : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.QueryEntrySummaryResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BIRDTransaction)]
	[InputBlock("J9")]
	public partial class ENQJ9 : MessageBlock, IStatusesAndErrors
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
			get { return NarrativeMessage; }
		}

		ZString IStatusesAndErrors.ReferenceNumber
		{
			get { return ZString.Empty; }
		}

		#endregion
	}
}
