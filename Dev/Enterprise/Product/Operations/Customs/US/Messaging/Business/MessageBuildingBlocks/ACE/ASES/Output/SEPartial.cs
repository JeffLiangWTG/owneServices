using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output
{
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.CargoReleaseStatus)]
	public partial class ASESSE91
	{
	}

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.CargoReleaseStatus)]
	public partial class ASESSE92
	{
	}

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.CargoReleaseStatus)]
	public partial class ASESSE93
	{
	}

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.CargoReleaseStatus)]
	public partial class ASESSE94
	{
	}

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.CargoReleaseStatus)]
	public partial class ASESSE95 : IStatusesAndErrors, IDispositionDetailProvider
	{
		#region IStatusesAndErrors Members

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

		#region IDispositionDetailProvider Members

		ZString IDispositionDetailProvider.DispositionCode
		{
			get { return DispositionCode; }
		}

		ZDateTime IDispositionDetailProvider.DispositionDateTime
		{
			get { return DateTimeParser.GetDateTimeFromZDateAndStringTime(DispositionDate, DispositionTime); }
		}

		ZString IDispositionDetailProvider.NarrativeMessage
		{
			get { return NarrativeMessage; }
		}

		ZString IDispositionDetailProvider.ReleaseOrigin
		{
			get { return ZString.Empty; }
		}

		ZDateTime IDispositionDetailProvider.ReleaseDateTime
		{
			get { return ZDateTime.Empty; }
		}

		ZString IDispositionDetailProvider.DocumentType
		{
			get { return ZString.Empty; }
		}

		#endregion
	}

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.CargoReleaseStatus)]
	public partial class ASESSE96 : IStatusesAndErrors, IDispositionDetailProvider
	{
		#region IStatusesAndErrors Members

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
			get { return DispositionActionCode == DocumentRequired ? NarrativeMessage + " (" + new DocumentTypesList().GetDescriptionFromCode(DocumentType) + ")" : NarrativeMessage.ToString(); }
		}
		const string DocumentRequired = "96";

		ZString IDispositionDetailProvider.ReleaseOrigin
		{
			get { return ReleaseOrigin; }
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
	}

	public partial class ASESSO20Base : IReferenceDataProvider
	{
		ZString IReferenceDataProvider.ReferenceIdentifierQualifier
		{
			get { return ReferenceIdentifierQualifier; }
		}

		ZString IReferenceDataProvider.ReferenceIdentifier
		{
			get { return ReferenceIdentifier; }
		}
	}

	public partial class ASESSO30Base : ICountryOfOriginTariffDetailsBlock
	{
		ZInt ICountryOfOriginTariffDetailsBlock.RecordControlNumber { get { return LineItemIdentifier; } }
		ZString ICountryOfOriginTariffDetailsBlock.CountryOfOrigin { get { return CountryOfOrigin; } }
		ZString ICountryOfOriginTariffDetailsBlock.TariffNumber { get { return HTSNumber; } }
	}

	public partial class ASESSO50Base : IStatusesAndErrors
	{
		#region IStatusesAndErrors Members

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

	public partial class ASESSO60Base : IStatusesAndErrors, IDispositionDetailProvider
	{
		#region IStatusesAndErrors Members

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
			get { return DispositionActionCode == DocumentRequired ? NarrativeMessage + " (" + new DocumentTypesList().GetDescriptionFromCode(DocumentType) + ")" : NarrativeMessage.ToString(); }
		}
		const string DocumentRequired = "96";
		const string ReleaseSuspended = "99";

		ZString IDispositionDetailProvider.ReleaseOrigin
		{
			get { return !ReleaseOrigin.IsEmpty ? ReleaseOrigin : DispositionActionCode == ReleaseSuspended ? DispositionActionCode : ZString.Empty; }
		}

		ZDateTime IDispositionDetailProvider.ReleaseDateTime
		{
			get { return ReleaseDate; }
		}

		ZString IDispositionDetailProvider.DocumentType
		{
			get { return DocumentType; }
		}

		#endregion
	}

	public partial class ASESSO70Base : IStatusesAndErrors, IPGADispositionProvider
	{
		#region IStatusesAndErrors Members

		ZString IStatusesAndErrors.LineNumber
		{
			get { return BeginningCBPLine; }
		}

		ZString IStatusesAndErrors.NarrativeMessage
		{
			get { return EntryLevelStatusMessage; }
		}

		ZString IStatusesAndErrors.Code
		{
			get { return PGALineLevelStatusCode; }
		}

		ZString IStatusesAndErrors.ReferenceNumber
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region IPGADispositionProvider
		ZString IPGADispositionProvider.GovernmentAgencyProgramCode
		{
			get { return GovernmentAgencyProgramCode; }
		}

		ZString IPGADispositionProvider.BeginningCBPLineNo
		{
			get { return BeginningCBPLine; }
		}

		ZString IPGADispositionProvider.BeginningOGALineNo
		{
			get { return BeginningPGALine.ToString(); }
		}

		ZString IPGADispositionProvider.PGALineDispositionCode
		{
			get { return PGALineLevelStatusCode; }
		}

		ZString IPGADispositionProvider.ReviewReasonCode
		{
			get { return StatusReasonCode; }
		}

		ZDateTime IPGADispositionProvider.DispositionDateTime
		{
			get
			{
				ZDateTime actionDate;
				if (ZDateTime.TryParseExact(StatusActionDate, out actionDate, "MMddyy"))
				{
					return DateTimeParser.GetDateTimeFromZDateAndStringTime(actionDate.Date, StatusActionTime);
				}
				return ZDateTime.Empty;
			}
		}

		ZString IPGADispositionProvider.EntryDispositionCode
		{
			get { return EntryLevelStatusCode; }
		}

		ZString IPGADispositionProvider.EntryLineDispositionCode
		{
			get { return EntryLineLevelStatusCode; }
		}

		ZString IPGADispositionProvider.EndingCBPLineNo
		{
			get { return EndingCBPLine; }
		}

		ZString IPGADispositionProvider.EndingOGALineNo
		{
			get { return EndingPGALine.ToString(); }
		}

		ZString IPGADispositionProvider.NarrativeMessage
		{
			get { return EntryLevelStatusMessage; }
		}

		ZString IPGADispositionProvider.OtherAgencyQuotaIdentifier
		{
			get { return GovernmentAgencyCode; }
		}

		ZString IPGADispositionProvider.RangeIndicator
		{
			get { return RangeIndicator; }
		}

		ZString IPGADispositionProvider.DocumentTypeCode
		{
			get { return DocumentTypeCode; }
		}

		ZString IPGADispositionProvider.PGAEntryHoldType
		{
			get { return PGAEntryHoldType; }
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

	public partial class ASESSO71Base : IPGADispositionDetailProvider
	{
		#region IPGADispositionDetailProvider

		ZString IPGADispositionDetailProvider.ReferenceIDQualifier
		{
			get { return !PGAReferenceIdentificationNumberQualifier.IsEmpty ? PGAReferenceIdentificationNumberQualifier : PGAReferenceIdentificationNumberQualifier1; }
		}

		ZString IPGADispositionDetailProvider.ReferenceID
		{
			get { return !PGAReferenceIdentificationNumber.IsEmpty ? PGAReferenceIdentificationNumber : PGAReferenceIdentificationNumber1; }
		}

		ZDateTime IPGADispositionDetailProvider.ReceiptDateTime
		{
			get { return DateTimeParser.GetDateTimeFromZDateAndStringTime(PGAReferenceIdentificationNumberReceiptDate, PGAReferenceIdentificationNumberReceiptTime); }
		}

		IEnumerable<ZString> IPGADispositionDetailProvider.SubReasonCodes
		{
			get
			{
				if (!PGALineSubReasonCode.IsEmpty)
				{
					yield return PGALineSubReasonCode;
				}

				if (!PGALineSubReasonCode1.IsEmpty)
				{
					yield return PGALineSubReasonCode1;
				}

				if (!PGALineSubReasonCode2.IsEmpty)
				{
					yield return PGALineSubReasonCode2;
				}

				if (!PGALineSubReasonCode3.IsEmpty)
				{
					yield return PGALineSubReasonCode3;
				}

				if (!PGALineSubReasonCode4.IsEmpty)
				{
					yield return PGALineSubReasonCode4;
				}

				if (!PGALineSubReasonCode5.IsEmpty)
				{
					yield return PGALineSubReasonCode5;
				}

				if (!PGALineSubReasonCode6.IsEmpty)
				{
					yield return PGALineSubReasonCode6;
				}

				if (!PGALineSubReasonCode7.IsEmpty)
				{
					yield return PGALineSubReasonCode7;
				}

				if (!PGALineSubReasonCode8.IsEmpty)
				{
					yield return PGALineSubReasonCode8;
				}

				if (!PGALineSubReasonCode9.IsEmpty)
				{
					yield return PGALineSubReasonCode9;
				}
			}
		}
		#endregion
	}

	public partial class ASESSO72Base : IPGADispositionComments
	{
		#region IPGADispositionComments

		ZString IPGADispositionComments.CommentsToTradeFromPGA
		{
			get { return CommentsToTradeFromPGA; }
		}

		#endregion
	}

	public partial class ASESSO70_01Base : IStatusesAndErrors, IPGADispositionProvider
	{
		#region IStatusesAndErrors Members

		ZString IStatusesAndErrors.LineNumber
		{
			get { return BeginningCBPLine; }
		}

		ZString IStatusesAndErrors.NarrativeMessage
		{
			get { return PGAEntryLevelStatusMessage; }
		}

		ZString IStatusesAndErrors.Code
		{
			get { return PGALineLevelStatusCode; }
		}

		ZString IStatusesAndErrors.ReferenceNumber
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region IPGADispositionProvider
		ZString IPGADispositionProvider.GovernmentAgencyProgramCode
		{
			get { return GovernmentAgencyProgramCode; }
		}

		ZString IPGADispositionProvider.BeginningCBPLineNo
		{
			get { return BeginningCBPLine; }
		}

		ZString IPGADispositionProvider.BeginningOGALineNo
		{
			get { return BeginningPGALine.ToString(); }
		}

		ZString IPGADispositionProvider.PGALineDispositionCode
		{
			get { return PGALineLevelStatusCode; }
		}

		ZString IPGADispositionProvider.ReviewReasonCode
		{
			get { return StatusReasonCode; }
		}

		ZDateTime IPGADispositionProvider.DispositionDateTime
		{
			get
			{
				ZDateTime actionDate;
				if (ZDateTime.TryParseExact(StatusActionDate, out actionDate, "MMddyy"))
				{
					return DateTimeParser.GetDateTimeFromZDateAndStringTime(actionDate.Date, StatusActionTime);
				}
				return ZDateTime.Empty;
			}
		}

		ZString IPGADispositionProvider.EntryDispositionCode
		{
			get { return PGAEntryLevelStatusCode; }
		}

		ZString IPGADispositionProvider.EntryLineDispositionCode
		{
			get { return EntryLineLevelStatusCode; }
		}

		ZString IPGADispositionProvider.EndingCBPLineNo
		{
			get { return EndingCBPLine; }
		}

		ZString IPGADispositionProvider.EndingOGALineNo
		{
			get { return EndingPGALine.ToString(); }
		}

		ZString IPGADispositionProvider.NarrativeMessage
		{
			get { return PGAEntryLevelStatusMessage; }
		}

		ZString IPGADispositionProvider.OtherAgencyQuotaIdentifier
		{
			get { return GovernmentAgencyCode; }
		}

		ZString IPGADispositionProvider.RangeIndicator
		{
			get { return ZString.Empty; }
		}

		ZString IPGADispositionProvider.DocumentTypeCode
		{
			get { return DocumentTypeCode; }
		}

		ZString IPGADispositionProvider.PGAEntryHoldType
		{
			get { return PGAEntryHoldType; }
		}

		ZString IPGADispositionProvider.BeginningTariffPosition
		{
			get { return BeginningTariffPosition; }
		}

		ZString IPGADispositionProvider.EndingTariffPosition
		{
			get { return EndingTariffPosition; }
		}

		ZString IPGADispositionProvider.PGAProcessingGroupVersion
		{
			get { return PGAProcessingGroupVersion; }
		}

		#endregion
	}

	public partial class ASESSO70_02Base : IStatusesAndErrors, IPGADispositionProvider
	{
		#region IStatusesAndErrors Members

		ZString IStatusesAndErrors.LineNumber
		{
			get { return BeginningCBPLine; }
		}

		ZString IStatusesAndErrors.NarrativeMessage
		{
			get { return PGAEntryLevelStatusMessage; }
		}

		ZString IStatusesAndErrors.Code
		{
			get { return PGALineLevelStatusCode; }
		}

		ZString IStatusesAndErrors.ReferenceNumber
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region IPGADispositionProvider
		ZString IPGADispositionProvider.GovernmentAgencyProgramCode
		{
			get { return GovernmentAgencyProgramCode; }
		}

		ZString IPGADispositionProvider.BeginningCBPLineNo
		{
			get { return BeginningCBPLine; }
		}

		ZString IPGADispositionProvider.BeginningOGALineNo
		{
			get { return BeginningPGALine.ToString(); }
		}

		ZString IPGADispositionProvider.PGALineDispositionCode
		{
			get { return PGALineLevelStatusCode; }
		}

		ZString IPGADispositionProvider.ReviewReasonCode
		{
			get { return StatusReasonCode; }
		}

		ZDateTime IPGADispositionProvider.DispositionDateTime
		{
			get
			{
				ZDateTime actionDate;
				if (ZDateTime.TryParseExact(StatusActionDate, out actionDate, "MMddyy"))
				{
					return DateTimeParser.GetDateTimeFromZDateAndStringTime(actionDate.Date, StatusActionTime);
				}
				return ZDateTime.Empty;
			}
		}

		ZString IPGADispositionProvider.EntryDispositionCode
		{
			get { return PGAEntryLevelStatusCode; }
		}

		ZString IPGADispositionProvider.EntryLineDispositionCode
		{
			get { return EntryLineLevelStatusCode; }
		}

		ZString IPGADispositionProvider.EndingCBPLineNo
		{
			get { return EndingCBPLine; }
		}

		ZString IPGADispositionProvider.EndingOGALineNo
		{
			get { return EndingPGALine.ToString(); }
		}

		ZString IPGADispositionProvider.NarrativeMessage
		{
			get { return PGAEntryLevelStatusMessage; }
		}

		ZString IPGADispositionProvider.OtherAgencyQuotaIdentifier
		{
			get { return GovernmentAgencyCode; }
		}

		ZString IPGADispositionProvider.RangeIndicator
		{
			get { return ZString.Empty; }
		}

		ZString IPGADispositionProvider.DocumentTypeCode
		{
			get { return DocumentTypeCode; }
		}

		ZString IPGADispositionProvider.PGAEntryHoldType
		{
			get { return PGAEntryHoldType; }
		}

		ZString IPGADispositionProvider.BeginningTariffPosition
		{
			get { return BeginningTariffPosition; }
		}

		ZString IPGADispositionProvider.EndingTariffPosition
		{
			get { return EndingTariffPosition; }
		}

		ZString IPGADispositionProvider.PGAProcessingGroupVersion
		{
			get { return PGAProcessingGroupVersion; }
		}

		#endregion
	}

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.CargoReleaseStatus)]
	public partial class ASESSE50 : IStatusesAndErrors, IDispositionDetailProvider
	{
		#region IStatusesAndErrors Members

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

		#region IDispositionDetailProvider Members

		ZString IDispositionDetailProvider.DispositionCode
		{
			get { return DispositionCode; }
		}

		ZDateTime IDispositionDetailProvider.DispositionDateTime
		{
			get { return DateTimeParser.GetDateTimeFromZDateAndStringTime(DispositionDate, DispositionTime); }
		}

		ZString IDispositionDetailProvider.NarrativeMessage
		{
			get { return NarrativeMessage; }
		}

		ZString IDispositionDetailProvider.ReleaseOrigin
		{
			get { return ZString.Empty; }
		}

		ZDateTime IDispositionDetailProvider.ReleaseDateTime
		{
			get { return ZDateTime.Empty; }
		}

		ZString IDispositionDetailProvider.DocumentType
		{
			get { return ZString.Empty; }
		}

		#endregion
	}
}
