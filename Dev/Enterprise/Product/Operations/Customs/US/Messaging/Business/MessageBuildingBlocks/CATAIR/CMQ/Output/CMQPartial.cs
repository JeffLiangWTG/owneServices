using CargoWise.Types;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output.Abstract
{
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.QueryCurrentEntryStatusResponse)]
	public partial class CMQR0 : IStatusesAndErrors
	{
		ZString IStatusesAndErrors.LineNumber
		{
			get { return ZString.Empty; }
		}

		ZString IStatusesAndErrors.NarrativeMessage
		{
			get { return NarrativeMessage; }
		}

		ZString IStatusesAndErrors.Code
		{
			get { return ErrorMessageIdentifier; }
		}

		ZString IStatusesAndErrors.ReferenceNumber
		{
			get { return EntryFilerCode + EntryNumber; }
		}
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.QueryCurrentEntryStatusResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.CargoReleaseProcessingResults)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BIRDTransaction)]
	[InputBlock("R5")]
	public partial class CMQR5 : MessageBlock, IReleaseDetailProvider, IStatusesAndErrors, I7501Status
	{
		#region IDispositionDetailProvider Members

		ZString IDispositionDetailProvider.DispositionCode
		{
			get { return DispositionActionCode; }
		}

		ZDateTime IDispositionDetailProvider.DispositionDateTime
		{
			get { return DateTimeParser.GetDateTimeFromZDateAndStringTime(DispositionActionDate, DispositionActionTime); }
		}

		ZString IDispositionDetailProvider.NarrativeMessage
		{
			get { return NarrativeMessage; }
		}

		ZString IDispositionDetailProvider.ReleaseOrigin
		{
			get { return !ReleaseOrigin.IsEmpty ? ReleaseOrigin.ToString().PadLeft(2, '0') : ReleaseOrigin.ToString(); }
		}

		ZDateTime IDispositionDetailProvider.ReleaseDateTime
		{
			get { return ReleaseDate; }
		}

		ZString IDispositionDetailProvider.DocumentType
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region IStatusesAndErrors

		ZString IStatusesAndErrors.LineNumber
		{
			get { return ZString.Empty; }
		}

		ZString IStatusesAndErrors.NarrativeMessage
		{
			get { return NarrativeMessage; }
		}

		ZString IStatusesAndErrors.Code
		{
			get { return DispositionActionCode; }
		}

		ZString IStatusesAndErrors.ReferenceNumber
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region IReleaseDetailProvider Members

		ZInt IReleaseDetailProvider.Sequence
		{
			get { return Sequence; }
		}

		ZInt IReleaseDetailProvider.Quantity
		{
			get { return Quantity; }
		}

		#endregion

		#region I7501Status Members

		ZString I7501Status.Code
		{
			get { return DispositionActionCode; }
		}

		ZString I7501Status.NarrativeMessage
		{
			get { return NarrativeMessage; }
		}

		bool I7501Status.IsMessageStatus
		{
			get { return true; }
		}

		ZDateTime I7501Status.StatusDate
		{
			get { return DispositionActionDate; }
		}

		#endregion
	}
}

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output
{
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.QueryCurrentEntryStatusResponse)]
	public partial class CMQSA : IStatusesAndErrors
	{
		ZString IStatusesAndErrors.LineNumber
		{
			get { return ZString.Empty; }
		}

		ZString IStatusesAndErrors.ReferenceNumber
		{
			get
			{
				ZStringBuilder result = new ZStringBuilder();

				if (!InbondNumber.IsEmpty)
				{
					result.Append("IT Number: " + InbondNumber);
				}

				if (!MasterBillNumber.IsEmpty)
				{
					result.Append("Master Bill Number: " + MasterBillNumber);
				}

				if (!IssuerCodeOfMasterBillNumber.IsEmpty)
				{
					result.Append("Issued By: " + IssuerCodeOfMasterBillNumber);
				}

				return result.ToStringWithDelimiterBetweenAppends(", ");
			}
		}

		ZString IStatusesAndErrors.NarrativeMessage
		{
			get { return NarrativeMessage; }
		}

		ZString IStatusesAndErrors.Code
		{
			get { return ErrorMessageIdentifier; }
		}
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.QueryCurrentEntryStatusResponse)]
	public partial class CMQSB : IStatusesAndErrors
	{
		ZString IStatusesAndErrors.LineNumber
		{
			get { return ZString.Empty; }
		}

		ZString IStatusesAndErrors.ReferenceNumber
		{
			get
			{
				ZStringBuilder result = new ZStringBuilder();

				if (!AirWaybillNumber.IsEmpty)
				{
					result.Append("AWB: " + AirWaybillNumber);
				}

				if (!HouseAirWaybillNumber.IsEmpty)
				{
					result.Append("HAWB: " + HouseAirWaybillNumber);
				}

				return result.ToStringWithDelimiterBetweenAppends(", ");
			}
		}

		ZString IStatusesAndErrors.NarrativeMessage
		{
			get { return NarrativeMessage; }
		}

		ZString IStatusesAndErrors.Code
		{
			get { return ErrorMessageIdentifier; }
		}
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.QueryCurrentEntryStatusResponse)]
	public partial class CMQR1 : MessageBlock
	{
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.QueryCurrentEntryStatusResponse)]
	public partial class CMQR3 : MessageBlock, ICountryOfOriginTariffDetailsBlock
	{
		ZInt ICountryOfOriginTariffDetailsBlock.RecordControlNumber { get { return RecordControlNumber; } }
		ZString ICountryOfOriginTariffDetailsBlock.CountryOfOrigin { get { return CountryOfOrigin; } }
		ZString ICountryOfOriginTariffDetailsBlock.TariffNumber { get { return TariffNumber; } }
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.QueryCurrentEntryStatusResponse)]
	public partial class CMQS4 : IInBondStatusProvider
	{
		ZString IInBondStatusProvider.InBondStatus
		{
			get { return InbondStatus.ToString(); }
		}

		ZDate IInBondStatusProvider.InBondArrivalDate
		{
			get { return InbondArrivalDate; }
		}

		ZDate IInBondStatusProvider.InBondExportDate
		{
			get { return InbondExportDate; }
		}

		ZString IInBondStatusProvider.InbondEntryType
		{
			get { return ZString.Empty; }
		}
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.QueryCurrentEntryStatusResponse)]
	public partial class CMQR4 : MessageBlock
	{
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.QueryCurrentEntryStatusResponse)]
	public partial class CMQS5 : IInBondStatusProvider
	{
		ZString IInBondStatusProvider.InBondStatus
		{
			get { return InbondStatus.ToString(); }
		}

		ZDate IInBondStatusProvider.InBondArrivalDate
		{
			get { return InbondArrivalDate; }
		}

		ZDate IInBondStatusProvider.InBondExportDate
		{
			get { return InbondExportDate; }
		}

		ZString IInBondStatusProvider.InbondEntryType
		{
			get { return ZString.Empty; }
		}
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.QueryCurrentEntryStatusResponse)]
	public partial class CMQSC
	{
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.QueryCurrentEntryStatusResponse)]
	public partial class CMQSD : MessageBlock, IDispositionDetailProvider, IStatusesAndErrors
	{
		#region IDispositionDetailProvider

		ZString IDispositionDetailProvider.DispositionCode
		{
			get { return DispositionCode; }
		}

		ZDateTime IDispositionDetailProvider.DispositionDateTime
		{
			get { return DateTimeParser.GetDateTimeFromZDateAndStringTime(DispositionActionDate, DispositionActionTime); }
		}

		ZString IDispositionDetailProvider.NarrativeMessage
		{
			get { return NarrativeMessage; }
		}

		ZDateTime IDispositionDetailProvider.ReleaseDateTime
		{
			get { return ZDateTime.Empty; }
		}

		ZString IDispositionDetailProvider.ReleaseOrigin
		{
			get { return ZString.Empty; }
		}

		ZString IDispositionDetailProvider.DocumentType
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region IStatusesAndErrors

		ZString IStatusesAndErrors.LineNumber
		{
			get { return ZString.Empty; }
		}

		ZString IStatusesAndErrors.NarrativeMessage
		{
			get { return NarrativeMessage; }
		}

		ZString IStatusesAndErrors.Code
		{
			get { return DispositionCode; }
		}

		ZString IStatusesAndErrors.ReferenceNumber
		{
			get { return ZString.Empty; }
		}

		#endregion
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.QueryCurrentEntryStatusResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.CargoReleaseTransactionsResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.CargoReleaseProcessingResults)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BIRDTransaction)]
	[InputBlock("R6")]
	public partial class CMQR6 : MessageBlock, IStatusesAndErrors, IPGADispositionProvider
	{
		#region IStatusesAndErrors

		ZString IStatusesAndErrors.LineNumber
		{
			get { return ZString.Empty; }
		}

		ZString IStatusesAndErrors.NarrativeMessage
		{
			get { return DispositionQuotaStatusMessage; }
		}

		ZString IStatusesAndErrors.Code
		{
			get { return DispositionQuotaStatusCode; }
		}

		ZString IStatusesAndErrors.ReferenceNumber
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region IPGADispositionProvider
		ZString IPGADispositionProvider.GovernmentAgencyProgramCode
		{
			get { return ZString.Empty; }
		}

		ZString IPGADispositionProvider.BeginningCBPLineNo
		{
			get { return BeginningCBPLine.ToString(); }
		}

		ZString IPGADispositionProvider.BeginningOGALineNo
		{
			get { return BeginningOGALine.ToString(); }
		}

		ZString IPGADispositionProvider.PGALineDispositionCode
		{
			get { return DispositionCodelineLevel; }
		}

		ZDateTime IPGADispositionProvider.DispositionDateTime
		{
			get { return DateTimeParser.GetDateTimeFromZDateAndStringTime(DispositionDate, DispositionTime); }
		}

		ZString IPGADispositionProvider.EntryDispositionCode
		{
			get { return DispositionQuotaStatusCode; }
		}

		ZString IPGADispositionProvider.EndingCBPLineNo
		{
			get { return EndingCBPLine.ToString(); }
		}

		ZString IPGADispositionProvider.EndingOGALineNo
		{
			get { return EndingOGALine.ToString(); }
		}

		ZString IPGADispositionProvider.NarrativeMessage
		{
			get { return DispositionQuotaStatusMessage; }
		}

		ZString IPGADispositionProvider.OtherAgencyQuotaIdentifier
		{
			get { return OtherAgencyQuotaIdentifier; }
		}

		ZString IPGADispositionProvider.RangeIndicator
		{
			get { return RangeIndicator; }
		}

		ZString IPGADispositionProvider.EntryLineDispositionCode
		{
			get { return ZString.Empty; }
		}

		ZString IPGADispositionProvider.ReviewReasonCode
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

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.QueryCurrentEntryStatusResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.CargoReleaseTransactionsResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.CargoReleaseProcessingResults)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BIRDTransaction)]
	[InputBlock("R7")]
	public partial class CMQR7 : MessageBlock, IDispositionDetailProvider
	{
		ZString IDispositionDetailProvider.DispositionCode
		{
			get { return DispositionActionCode; }
		}

		ZDateTime IDispositionDetailProvider.DispositionDateTime
		{
			get { return DateTimeParser.GetDateTimeFromZDateAndStringTime(DispositionActionDate, DispositionActionTime); }
		}

		ZString IDispositionDetailProvider.NarrativeMessage
		{
			get { return NarrativeMessage; }
		}

		ZDateTime IDispositionDetailProvider.ReleaseDateTime
		{
			get { return ZDateTime.Empty; }
		}

		ZString IDispositionDetailProvider.ReleaseOrigin
		{
			get { return ZString.Empty; }
		}

		ZString IDispositionDetailProvider.DocumentType
		{
			get { return ZString.Empty; }
		}
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.QueryEntryStatusResponse)]
	public partial class CMQI1 : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.QueryEntryStatusResponse)]
	public partial class CMQIO : MessageBlock, IStatusesAndErrors
	{
		#region IStatusesAndErrors

		ZString IStatusesAndErrors.LineNumber
		{
			get { return ZString.Empty; }
		}

		ZString IStatusesAndErrors.NarrativeMessage
		{
			get { return NarrativeMessage; }
		}

		ZString IStatusesAndErrors.Code
		{
			get { return ZString.Empty; }
		}

		ZString IStatusesAndErrors.ReferenceNumber
		{
			get { return ZString.Empty; }
		}

		#endregion
	}
}
