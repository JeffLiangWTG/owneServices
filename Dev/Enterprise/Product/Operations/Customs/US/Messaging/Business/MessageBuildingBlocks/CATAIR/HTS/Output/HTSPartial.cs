using CargoWise.Types;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output
{
	#region HTSV

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ExtractReferenceFilesResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.HarmonizedSystemUpdate)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.HarmonizedTariffScheduleExtractReferenceQueryResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.HarmonizedTariffScheduleQueryTransactionQueryResponse)]
	public sealed partial class HTSV1 : MessageBlock, IHTS1
	{
		#region IHTS1 Members

		ZString IHTS1.TariffNumber
		{
			get { return TariffNumber; }
		}

		ZString IHTS1.TransactionCode
		{
			get { return TransactionCode; }
		}

		ZDate IHTS1.RecordBeginEffectiveDate
		{
			get { return RecordBeginEffectiveDate; }
		}

		ZDate IHTS1.RecordEndEffectiveDate
		{
			get { return RecordEndEffectiveDate; }
		}

		ZInt IHTS1.NumberOfReportingUnits
		{
			get { return NumberOfReportingUnits; }
		}

		ZString IHTS1.Unit1
		{
			get { return Unit1; }
		}

		ZString IHTS1.Unit2
		{
			get { return Unit2; }
		}

		ZString IHTS1.Unit3
		{
			get { return Unit3; }
		}

		ZString IHTS1.DutyComputationCode
		{
			get { return DutyComputationCode; }
		}

		ZString IHTS1.CommodityDescription
		{
			get { return CommodityDescription; }
		}

		ZDecimal IHTS1.Column1SpecificRate
		{
			get { return Column1SpecificRate; }
		}

		ZString IHTS1.BaseRateIndicator
		{
			get { return BaseRateIndicator; }
		}

		#endregion

		protected override void AdjustEndDates()
		{
			RecordEndEffectiveDate = AdjustedEndDate(RecordBeginEffectiveDate, RecordEndEffectiveDate);
		}
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ExtractReferenceFilesResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.HarmonizedSystemUpdate)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.HarmonizedTariffScheduleExtractReferenceQueryResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.HarmonizedTariffScheduleQueryTransactionQueryResponse)]
	public sealed partial class HTSV2 : MessageBlock, IHTS2
	{
		#region IHTS2 Members

		ZString IHTS2.TariffNumber
		{
			get { return TariffNumber; }
		}

		ZDecimal IHTS2.Column1RateAdValorem
		{
			get { return Column1RateAdValorem; }
		}

		ZDecimal IHTS2.Column1RateOther
		{
			get { return Column1RateOther; }
		}

		ZDecimal IHTS2.Column2RateSpecific
		{
			get { return Column2RateSpecific; }
		}

		ZDecimal IHTS2.Column2RateAdValorem
		{
			get { return Column2RateAdValorem; }
		}

		ZDecimal IHTS2.Column2RateOther
		{
			get { return Column2RateOther; }
		}

		ZString IHTS2.CountervailingDutyFlag
		{
			get { return CountervailingDutyFlag; }
		}

		ZString IHTS2.AdditionalTariffNumberIndicator
		{
			get { return AdditionalTariffNumberIndicator; }
		}

		ZString IHTS2.MiscellaneousPermitLicenseIndicator
		{
			get { return MiscellaneousPermitLicenseIndicator; }
		}

		#endregion
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ExtractReferenceFilesResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.HarmonizedSystemUpdate)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.HarmonizedTariffScheduleExtractReferenceQueryResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.HarmonizedTariffScheduleQueryTransactionQueryResponse)]
	public sealed partial class HTSV3 : MessageBlock, IHTS3
	{
		#region IHTS3 Members

		ZString IHTS3.TariffNumber
		{
			get { return TariffNumber; }
		}

		ZString IHTS3.GeneralizedSystemOfPreferencesGSPExcludedCountries
		{
			get { return GeneralizedSystemOfPreferencesGSPExcludedCountries; }
		}

		ZString IHTS3.AntidumpingDutyFlag
		{
			get { return AntidumpingDutyFlag; }
		}

		ZString IHTS3.QuotaIndicator
		{
			get { return QuotaIndicator; }
		}

		ZString IHTS3.CategoryNumber
		{
			get { return CategoryNumber; }
		}

		ZString IHTS3.SpecialProgramsIndicatorSPICode
		{
			get { return SpecialProgramsIndicatorSPICode; }
		}

		#endregion
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ExtractReferenceFilesResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.HarmonizedSystemUpdate)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.HarmonizedTariffScheduleExtractReferenceQueryResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.HarmonizedTariffScheduleQueryTransactionQueryResponse)]
	public sealed partial class HTSV4 : MessageBlock, IHTS4
	{
		#region IHTS4 Members

		ZString IHTS4.TariffNumber
		{
			get { return TariffNumber; }
		}

		ZString IHTS4.ValueEditCode
		{
			get { return ValueEditCode; }
		}

		ZDecimal IHTS4.ValueLowBounds
		{
			get { return ValueLowBounds; }
		}

		ZDecimal IHTS4.ValueHighBounds
		{
			get { return ValueHighBounds; }
		}

		ZString IHTS4.EntryDateRestrictionCode1
		{
			get { return EntryDateRestrictionCode1; }
		}

		ZShort IHTS4.BeginRestrictionDate1
		{
			get { return BeginRestrictionDate1; }
		}

		ZShort IHTS4.EndRestrictionDate1
		{
			get { return EndRestrictionDate1; }
		}

		ZString IHTS4.EntryDateRestrictionCode2
		{
			get { return EntryDateRestrictionCode2; }
		}

		ZShort IHTS4.BeginRestrictionDate2
		{
			get { return BeginRestrictionDate2; }
		}

		ZShort IHTS4.EndRestrictionDate2
		{
			get { return EndRestrictionDate2; }
		}

		ZString IHTS4.ISOCountryOfOriginEditCode
		{
			get { return ISOCountryOfOriginEditCode; }
		}

		ZString IHTS4.QuantityEditCode
		{
			get { return QuantityEditCode; }
		}

		ZDecimal IHTS4.QuantityEditLowerBound
		{
			get { return QuantityEditLowerBound; }
		}

		ZDecimal IHTS4.QuantityEditUpperBound
		{
			get { return QuantityEditUpperBound; }
		}

		#endregion
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ExtractReferenceFilesResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.HarmonizedSystemUpdate)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.HarmonizedTariffScheduleExtractReferenceQueryResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.HarmonizedTariffScheduleQueryTransactionQueryResponse)]
	public sealed partial class HTSV56789ABCEFGHIJK : MessageBlock, IHTS56789ABCEFGHIJK
	{
		#region IHTS56789ABC Members

		ZString IHTS56789ABCEFGHIJK.DutyElement
		{
			get { return DutyElement; }
		}

		ZString IHTS56789ABCEFGHIJK.TariffNumber
		{
			get { return TariffNumber; }
		}

		ZString IHTS56789ABCEFGHIJK.InternationalOrganizationForStandardizationISOCountryCode
		{
			get { return InternationalOrganizationForStandardizationISOCountryCode; }
		}

		ZDecimal IHTS56789ABCEFGHIJK.SpecificSpecialRate
		{
			get { return SpecificSpecialRate; }
		}

		ZDecimal IHTS56789ABCEFGHIJK.AdValoremSpecialRate
		{
			get { return AdValoremSpecialRate; }
		}

		ZDecimal IHTS56789ABCEFGHIJK.OtherSpecialRate
		{
			get { return OtherSpecialRate; }
		}

		ZString IHTS56789ABCEFGHIJK.TaxFeeClassCode
		{
			get { return TaxFeeClassCode; }
		}

		ZString IHTS56789ABCEFGHIJK.TaxFeeComputationCode
		{
			get { return TaxFeeComputationCode; }
		}

		ZString IHTS56789ABCEFGHIJK.TaxFeeFlag
		{
			get { return TaxFeeFlag; }
		}

		ZDecimal IHTS56789ABCEFGHIJK.TaxFeeSpecificRate
		{
			get { return TaxFeeSpecificRate; }
		}

		ZDecimal IHTS56789ABCEFGHIJK.TaxFeeAdValorem
		{
			get { return TaxFeeAdValorem; }
		}

		#endregion
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ExtractReferenceFilesResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.HarmonizedSystemUpdate)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.HarmonizedTariffScheduleExtractReferenceQueryResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.HarmonizedTariffScheduleQueryTransactionQueryResponse)]
	public sealed partial class HTSVD : MessageBlock, IHTSD
	{
		#region IHTSD Members

		ZString IHTSD.TariffNumber
		{
			get { return TariffNumber; }
		}

		ZString IHTSD.SpecialProgramsIndicatorSPICode
		{
			get { return SpecialProgramsIndicatorSPICode; }
		}

		#endregion
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ExtractReferenceFilesResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.HarmonizedSystemUpdate)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.HarmonizedTariffScheduleExtractReferenceQueryResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.HarmonizedTariffScheduleQueryTransactionQueryResponse)]
	public sealed partial class HTSVL : MessageBlock, IHTSL
	{
		#region IHTSL Members

		ZString IHTSL.TariffNumber
		{
			get { return TariffNumber; }
		}

		ZString IHTSL.ParticipatingGovernmentAgencies
		{
			get { return ParticipatingGovernmentAgencies; }
		}

		#endregion
	}

	#endregion

	#region HTSW

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.QueryHarmonizedSystem)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.HarmonizedTariffScheduleQueryTransactionQueryResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.HarmonizedTariffScheduleExtractReferenceQueryResponse)]
	public sealed partial class HTSW0 : MessageBlock, IHTS0, IStatusesAndErrors
	{
		#region IHTS0 Members

		ZString IHTS0.FromTariffNumber
		{
			get { return FromTariffNumber; }
		}

		ZDate IHTS0.AsOfDate
		{
			get { return AsOfDate; }
		}

		ZString IHTS0.ToTariffNumber
		{
			get { return ToTariffNumber; }
		}

		ZString IHTS0.NarrativeMessage
		{
			get { return NarrativeMessage; }
		}

		#endregion

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

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.QueryHarmonizedSystem)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.HarmonizedTariffScheduleQueryTransactionQueryResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.HarmonizedTariffScheduleExtractReferenceQueryResponse)]
	public sealed partial class HTSW1 : MessageBlock, IHTS1
	{
		#region IHTS1 Members

		ZString IHTS1.TariffNumber
		{
			get { return TariffNumber; }
		}

		ZString IHTS1.TransactionCode
		{
			get { return ZString.Empty; }
		}

		ZDate IHTS1.RecordBeginEffectiveDate
		{
			get { return RecordBeginEffectiveDate; }
		}

		ZDate IHTS1.RecordEndEffectiveDate
		{
			get { return RecordEndEffectiveDate; }
		}

		ZInt IHTS1.NumberOfReportingUnits
		{
			get { return NumberOfReportingUnits; }
		}

		ZString IHTS1.Unit1
		{
			get { return Unit1; }
		}

		ZString IHTS1.Unit2
		{
			get { return Unit2; }
		}

		ZString IHTS1.Unit3
		{
			get { return Unit3; }
		}

		ZString IHTS1.DutyComputationCode
		{
			get { return DutyComputationCode; }
		}

		ZString IHTS1.CommodityDescription
		{
			get { return CommodityDescription; }
		}

		ZDecimal IHTS1.Column1SpecificRate
		{
			get { return Column1SpecificRate; }
		}

		ZString IHTS1.BaseRateIndicator
		{
			get { return BaseRateIndicator; }
		}

		#endregion

		protected override void AdjustEndDates()
		{
			RecordEndEffectiveDate = AdjustedEndDate(RecordBeginEffectiveDate, RecordEndEffectiveDate);
		}
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.QueryHarmonizedSystem)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.HarmonizedTariffScheduleQueryTransactionQueryResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.HarmonizedTariffScheduleExtractReferenceQueryResponse)]
	public sealed partial class HTSW2 : MessageBlock, IHTS2
	{
		#region IHTS2 Members

		ZString IHTS2.TariffNumber
		{
			get { return TariffNumber; }
		}

		ZDecimal IHTS2.Column1RateAdValorem
		{
			get { return Column1RateAdValorem; }
		}

		ZDecimal IHTS2.Column1RateOther
		{
			get { return Column1RateOther; }
		}

		ZDecimal IHTS2.Column2RateSpecific
		{
			get { return Column2RateSpecific; }
		}

		ZDecimal IHTS2.Column2RateAdValorem
		{
			get { return Column2RateAdValorem; }
		}

		ZDecimal IHTS2.Column2RateOther
		{
			get { return Column2RateOther; }
		}

		ZString IHTS2.CountervailingDutyFlag
		{
			get { return CountervailingDutyFlag; }
		}

		ZString IHTS2.AdditionalTariffNumberIndicator
		{
			get { return AdditionalTariffNumberIndicator; }
		}

		ZString IHTS2.MiscellaneousPermitLicenseIndicator
		{
			get { return MiscellaneousPermitLicenseIndicator; }
		}

		#endregion
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.QueryHarmonizedSystem)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.HarmonizedTariffScheduleQueryTransactionQueryResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.HarmonizedTariffScheduleExtractReferenceQueryResponse)]
	public sealed partial class HTSW3 : MessageBlock, IHTS3
	{
		#region IHTS3 Members

		ZString IHTS3.TariffNumber
		{
			get { return TariffNumber; }
		}

		ZString IHTS3.GeneralizedSystemOfPreferencesGSPExcludedCountries
		{
			get { return GeneralizedSystemOfPreferencesGSPExcludedCountries; }
		}

		ZString IHTS3.AntidumpingDutyFlag
		{
			get { return AntidumpingDutyFlag; }
		}

		ZString IHTS3.QuotaIndicator
		{
			get { return QuotaIndicator; }
		}

		ZString IHTS3.CategoryNumber
		{
			get { return CategoryNumber; }
		}

		ZString IHTS3.SpecialProgramsIndicatorSPICode
		{
			get { return SpecialProgramIndicatorSPICode; }
		}

		#endregion
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.QueryHarmonizedSystem)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.HarmonizedTariffScheduleQueryTransactionQueryResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.HarmonizedTariffScheduleExtractReferenceQueryResponse)]
	public sealed partial class HTSW4 : MessageBlock, IHTS4
	{
		#region IHTS4 Members

		ZString IHTS4.TariffNumber
		{
			get { return TariffNumber; }
		}

		ZString IHTS4.ValueEditCode
		{
			get { return ValueEditCode; }
		}

		ZDecimal IHTS4.ValueLowBounds
		{
			get { return ValueLowBounds; }
		}

		ZDecimal IHTS4.ValueHighBounds
		{
			get { return ValueHighBounds; }
		}

		ZString IHTS4.EntryDateRestrictionCode1
		{
			get { return EntryDateRestrictionCode1; }
		}

		ZShort IHTS4.BeginRestrictionDate1
		{
			get { return BeginRestrictionDate1; }
		}

		ZShort IHTS4.EndRestrictionDate1
		{
			get { return EndRestrictionDate1; }
		}

		ZString IHTS4.EntryDateRestrictionCode2
		{
			get { return EntryDateRestrictionCode2; }
		}

		ZShort IHTS4.BeginRestrictionDate2
		{
			get { return BeginRestrictionDate2; }
		}

		ZShort IHTS4.EndRestrictionDate2
		{
			get { return EndRestrictionDate2; }
		}

		ZString IHTS4.ISOCountryOfOriginEditCode
		{
			get { return ISOCountryOfOriginEditCode; }
		}

		ZString IHTS4.QuantityEditCode
		{
			get { return QuantityEditCode; }
		}

		ZDecimal IHTS4.QuantityEditLowerBound
		{
			get { return QuantityEditLowBounds; }
		}

		ZDecimal IHTS4.QuantityEditUpperBound
		{
			get { return QuantityEditHighBounds; }
		}

		#endregion
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.QueryHarmonizedSystem)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.HarmonizedTariffScheduleQueryTransactionQueryResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.HarmonizedTariffScheduleExtractReferenceQueryResponse)]
	public sealed partial class HTSW56789ABCEFGHIJK : MessageBlock, IHTS56789ABCEFGHIJK
	{
		#region IHTS56789ABC Members

		ZString IHTS56789ABCEFGHIJK.DutyElement
		{
			get { return DutyElement; }
		}

		ZString IHTS56789ABCEFGHIJK.TariffNumber
		{
			get { return TariffNumber; }
		}

		ZString IHTS56789ABCEFGHIJK.InternationalOrganizationForStandardizationISOCountryCode
		{
			get { return InternationalOrganizationForStandardizationISOCountryCode1; }
		}

		ZDecimal IHTS56789ABCEFGHIJK.SpecificSpecialRate
		{
			get { return SpecificSpecialRate1; }
		}

		ZDecimal IHTS56789ABCEFGHIJK.AdValoremSpecialRate
		{
			get { return AdValoremSpecialRate1; }
		}

		ZDecimal IHTS56789ABCEFGHIJK.OtherSpecialRate
		{
			get { return OtherSpecialRate1; }
		}

		ZString IHTS56789ABCEFGHIJK.TaxFeeClassCode
		{
			get { return TaxFeeClassCode1; }
		}

		ZString IHTS56789ABCEFGHIJK.TaxFeeComputationCode
		{
			get { return TaxFeeComputationCode1; }
		}

		ZString IHTS56789ABCEFGHIJK.TaxFeeFlag
		{
			get { return TaxFeeFlag1; }
		}

		ZDecimal IHTS56789ABCEFGHIJK.TaxFeeSpecificRate
		{
			get { return TaxFeeSpecificRate1; }
		}

		ZDecimal IHTS56789ABCEFGHIJK.TaxFeeAdValorem
		{
			get { return TaxFeeAdValorem1; }
		}

		#endregion
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.QueryHarmonizedSystem)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.HarmonizedTariffScheduleQueryTransactionQueryResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.HarmonizedTariffScheduleExtractReferenceQueryResponse)]
	public sealed partial class HTSWD : MessageBlock, IHTSD
	{
		#region IHTSD Members

		ZString IHTSD.TariffNumber
		{
			get { return TariffNumber; }
		}

		ZString IHTSD.SpecialProgramsIndicatorSPICode
		{
			get { return SpecialProgramsIndicatorSPICode; }
		}

		#endregion
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.QueryHarmonizedSystem)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.HarmonizedTariffScheduleQueryTransactionQueryResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.HarmonizedTariffScheduleExtractReferenceQueryResponse)]
	public sealed partial class HTSWL : MessageBlock, IHTSL
	{
		#region IHTSL Members

		ZString IHTSL.TariffNumber
		{
			get { return TariffNumber; }
		}

		ZString IHTSL.ParticipatingGovernmentAgencies
		{
			get { return ParticipatingGovernmentAgencies; }
		}

		#endregion
	}

	#endregion
}
