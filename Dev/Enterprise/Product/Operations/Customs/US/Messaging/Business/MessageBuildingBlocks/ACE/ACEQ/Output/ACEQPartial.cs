using System;
using CargoWise.Types;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output
{
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQueryResponse)]
	public partial class ACEQWR0 : IStatusesAndErrors
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

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQueryResponse)]
	public partial class ACEQWSA : IStatusesAndErrors
	{
		ZString IStatusesAndErrors.LineNumber
		{
			get { return ZString.Empty; }
		}

		ZString IStatusesAndErrors.ReferenceNumber
		{
			get
			{
				var result = new ZStringBuilder();

				if (!InbondNumber.IsEmpty)
				{
					result.Append("IT Number: " + InbondNumber);
				}

				if (!MasterBillNumber.IsEmpty || !IssuerCodeOfMasterBillNumber.IsEmpty)
				{
					result.Append("Master Bill Number: " + IssuerCodeOfMasterBillNumber + MasterBillNumber);
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

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQueryResponse)]
	public partial class ACEQWSB : IStatusesAndErrors
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

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQueryResponse)]
	public partial class ACEQWR1 : MessageBlock
	{
	}

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQueryResponse)]
	public partial class ACEQWR2 : MessageBlock
	{
	}

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQueryResponse)]
	public partial class ACEQWR3 : MessageBlock, ICountryOfOriginTariffDetailsBlock
	{
		ZInt ICountryOfOriginTariffDetailsBlock.RecordControlNumber { get { return RecordControlNumber; } }
		ZString ICountryOfOriginTariffDetailsBlock.CountryOfOrigin { get { return CountryOfOrigin; } }
		ZString ICountryOfOriginTariffDetailsBlock.TariffNumber { get { return TariffNumber; } }
	}

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQueryResponse)]
	public partial class ACEQWS4 : MessageBlock, IInBondStatusProvider
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
			get { return InbondEntryType; }
		}
	}

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQueryResponse)]
	public partial class ACEQWR4 : MessageBlock, IManifestInfoProvider
	{
		ZString IManifestInfoProvider.InBondNumber => InBondNumber;
		ZString IManifestInfoProvider.MasterBillNumber => MasterBillNumber;
		ZString IManifestInfoProvider.HouseBillNumber => HouseBillNumber;
		ZString IManifestInfoProvider.SubHouseBillNumber => SubHouseBillNumber;
		ZDecimal IManifestInfoProvider.ManifestQuantity => Convert.ToDecimal(ManifestQuantity);
		ZString IManifestInfoProvider.Unit => Unit;
		ZString IManifestInfoProvider.IssuerCodeOfMasterBillNumber => IssuerCodeOfMasterBillNumber;
		ZString IManifestInfoProvider.IssuerCodeOfHouseBillNumber => IssuerCodeOfHouseBillNumber;
		ZString IManifestInfoProvider.BillOfLadingType => BillOfLadingType;
		ZString IManifestInfoProvider.ImporterSecurityFilingIndicator => ImporterSecurityFilingIndicator;
		ZString IManifestInfoProvider.ModeOfTransportationCode => ModeOfTransportationCode;
	}

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQueryResponse)]
	public partial class ACEQWR4_1 : MessageBlock, IManifestInfoProvider
	{
		ZString IManifestInfoProvider.InBondNumber => InBondNumber;
		ZString IManifestInfoProvider.MasterBillNumber => MasterBillNumber;
		ZString IManifestInfoProvider.HouseBillNumber => HouseBillNumber;
		ZString IManifestInfoProvider.SubHouseBillNumber => SubHouseBillNumber;
		ZDecimal IManifestInfoProvider.ManifestQuantity => ManifestQuantity;
		ZString IManifestInfoProvider.Unit => Unit;
		ZString IManifestInfoProvider.IssuerCodeOfMasterBillNumber => IssuerCodeOfMasterBillNumber;
		ZString IManifestInfoProvider.IssuerCodeOfHouseBillNumber => IssuerCodeOfHouseBillNumber;
		ZString IManifestInfoProvider.BillOfLadingType => BillOfLadingType;
		ZString IManifestInfoProvider.ImporterSecurityFilingIndicator => ImporterSecurityFilingIndicator;
		ZString IManifestInfoProvider.ModeOfTransportationCode => ModeOfTransportationCode;
	}

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQueryResponse)]
	public partial class ACEQWS5 : IInBondStatusProvider
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
			get { return InbondEntryType; }
		}
	}

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQueryResponse)]
	public partial class ACEQWSC : MessageBlock
	{
	}

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQueryResponse)]
	public partial class ACEQWSD : MessageBlock, IDispositionDetailProvider, IStatusesAndErrors
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

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQueryResponse)]
	public partial class ACEQWR5 : MessageBlock, IStatusesAndErrors
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
			get { return DispositionActionCode; }
		}

		ZString IStatusesAndErrors.ReferenceNumber
		{
			get { return ZString.Empty; }
		}

		#endregion
	}

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQueryResponse)]
	public partial class ACEQWN1 : MessageBlock
	{
	}

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQueryResponse)]
	public partial class ACEQWN0 : MessageBlock
	{
	}
}
