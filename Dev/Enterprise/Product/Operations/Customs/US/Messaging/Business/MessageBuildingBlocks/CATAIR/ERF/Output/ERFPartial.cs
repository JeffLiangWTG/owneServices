namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output
{
	using CargoWise.Types;

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ExtractADDCVDCaseFileResponse)]
	public partial class ERFC2 : MessageBlock
	{
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ExtractADDCVDCaseFileResponse)]
	public partial class ERFC3 : MessageBlock, IAntiDumpingC3
	{
		#region IAntiDumpingC3 Members

		ZString IAntiDumpingC3.ISOCountryCode
		{
			get { return ISOCountryCode; }
			set { ISOCountryCode = value; }
		}

		ZString IAntiDumpingC3.CaseNumber
		{
			get { return CaseNumber; }
			set { CaseNumber = value; }
		}

		ZString IAntiDumpingC3.RelatedCaseNumber
		{
			get { return RelatedCaseNumber; }
			set { RelatedCaseNumber = value; }
		}

		ZString IAntiDumpingC3.ManufacturerIDCode
		{
			get { return ManufacturerIDCode; }
			set { ManufacturerIDCode = value; }
		}

		ZString IAntiDumpingC3.ShipperID
		{
			get { return ShipperID; }
			set { ShipperID = value; }
		}

		ZString IAntiDumpingC3.CaseStatus
		{
			get { return CaseStatus; }
			set { CaseStatus = value; }
		}

		ZDate IAntiDumpingC3.CaseStatusDate
		{
			get { return CaseStatusDate; }
			set { CaseStatusDate = value; }
		}

		ZString IAntiDumpingC3.BondCashIndicator
		{
			get { return BondCashIndicator; }
			set { BondCashIndicator = value; }
		}

		#endregion
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ExtractADDCVDCaseFileResponse)]
	public partial class ERFC4 : MessageBlock, IAntiDumpingC4
	{
		#region IAntiDumpingC4 Members

		ZString IAntiDumpingC4.CaseNumber
		{
			get { return CaseNumber; }
			set { CaseNumber = value; }
		}

		ZString IAntiDumpingC4.Contact
		{
			get { return Contact; }
			set { Contact = value; }
		}

		ZString IAntiDumpingC4.Phone
		{
			get { return Phone; }
			set { Phone = value; }
		}

		ZDate IAntiDumpingC4.LiquidationSuspensionDate
		{
			get { return LiquidationSuspensionDate; }
			set { LiquidationSuspensionDate = value; }
		}

		ZString IAntiDumpingC4.ShortDescription
		{
			get { return ShortDescription; }
			set { ShortDescription = value; }
		}

		#endregion
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ExtractADDCVDCaseFileResponse)]
	public partial class ERFC5 : MessageBlock, IAntiDumpingC5
	{
		#region IAntiDumpingC5 Members

		ZString IAntiDumpingC5.CaseNumber
		{
			get { return CaseNumber; }
			set { CaseNumber = value; }
		}

		ZString IAntiDumpingC5.RateIndicator
		{
			get { return RateIndicator; }
			set { RateIndicator = value; }
		}

		ZString IAntiDumpingC5.DepositRate1
		{
			get { return DepositRate1; }
			set { DepositRate1 = value; }
		}

		ZDecimal IAntiDumpingC5.DepositRate2
		{
			get { return DepositRate2; }
			set { DepositRate2 = value; }
		}

		ZDecimal IAntiDumpingC5.DepositRate3
		{
			get { return DepositRate3; }
			set { DepositRate3 = value; }
		}

		ZDate IAntiDumpingC5.EffectiveEntryDate
		{
			get { return EffectiveEntryDate; }
			set { EffectiveEntryDate = value; }
		}

		ZDate IAntiDumpingC5.EffectiveExportDate
		{
			get { return EffectiveExportDate; }
			set { EffectiveExportDate = value; }
		}

		ZString IAntiDumpingC5.BondCashIndicator1
		{
			get { return BondCashIndicator1; }
			set { BondCashIndicator1 = value; }
		}

		ZDate IAntiDumpingC5.BondCashDate1
		{
			get { return BondCashDate1; }
			set { BondCashDate1 = value; }
		}

		ZString IAntiDumpingC5.BondCashIndicator2
		{
			get { return BondCashIndicator2; }
			set { BondCashIndicator2 = value; }
		}

		ZDate IAntiDumpingC5.BondCashDate2
		{
			get { return BondCashDate2; }
			set { BondCashDate2 = value; }
		}

		ZString IAntiDumpingC5.BondCashIndicator3
		{
			get { return BondCashIndicator3; }
			set { BondCashIndicator3 = value; }
		}

		ZDate IAntiDumpingC5.BondCashDate3
		{
			get { return BondCashDate3; }
			set { BondCashDate3 = value; }
		}

		ZString IAntiDumpingC5.BondCashIndicator4
		{
			get { return BondCashIndicator4; }
			set { BondCashIndicator4 = value; }
		}

		ZDate IAntiDumpingC5.BondCashDate4
		{
			get { return BondCashDate4; }
			set { BondCashDate4 = value; }
		}

		ZString IAntiDumpingC5.BondCashIndicator5
		{
			get { return BondCashIndicator5; }
			set { BondCashIndicator5 = value; }
		}

		ZDate IAntiDumpingC5.BondCashDate5
		{
			get { return BondCashDate5; }
			set { BondCashDate5 = value; }
		}

		#endregion
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ExtractADDCVDCaseFileResponse)]
	public partial class ERFC6 : MessageBlock, IAntiDumpingC6
	{
		#region IAntiDumpingC6 Members

		ZString IAntiDumpingC6.TariffNumber
		{
			get { return TariffNumber; }
			set { TariffNumber = value; }
		}

		ZString IAntiDumpingC6.TariffNumber1
		{
			get { return TariffNumber1; }
			set { TariffNumber1 = value; }
		}

		ZString IAntiDumpingC6.TariffNumber2
		{
			get { return TariffNumber2; }
			set { TariffNumber2 = value; }
		}

		ZString IAntiDumpingC6.TariffNumber3
		{
			get { return TariffNumber3; }
			set { TariffNumber3 = value; }
		}

		ZString IAntiDumpingC6.TariffNumber4
		{
			get { return TariffNumber4; }
			set { TariffNumber4 = value; }
		}

		ZString IAntiDumpingC6.TariffNumber5
		{
			get { return TariffNumber5; }
			set { TariffNumber5 = value; }
		}

		ZString IAntiDumpingC6.TariffNumber6
		{
			get { return TariffNumber6; }
			set { TariffNumber6 = value; }
		}

		#endregion
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ExtractADDCVDCaseFileResponse)]
	public partial class ERFC7 : MessageBlock, IAntiDumpingC7
	{
		#region IAntiDumpingC7 Members

		ZString IAntiDumpingC7.CaseNumber
		{
			get { return CaseNumber; }
			set { CaseNumber = value; }
		}

		ZString IAntiDumpingC7.ManufacturerName
		{
			get { return ManufacturerName; }
			set { ManufacturerName = value; }
		}

		#endregion
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ExtractADDCVDCaseFileResponse)]
	public partial class ERFC8 : MessageBlock, IAntiDumpingC8
	{
		#region IAntiDumpingC8 Members

		ZString IAntiDumpingC8.CaseNumber
		{
			get { return CaseNumber; }
			set { CaseNumber = value; }
		}

		ZString IAntiDumpingC8.Shipper
		{
			get { return Shipper; }
			set { Shipper = value; }
		}

		#endregion
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ExtractADDCVDCaseFileResponse)]
	public partial class ERFC9 : MessageBlock, IAntiDumpingC9, IStatusesAndErrors
	{
		#region IAntiDumpingC9 Members

		ZString IAntiDumpingC9.NarrativeMessage
		{
			get { return NarrativeMessage; }
			set { NarrativeMessage = value; }
		}

		ZString IAntiDumpingC9.MessageIDCode
		{
			get { return MessageIdentificationCode; }
			set { MessageIdentificationCode = value; }
		}

		#endregion

		#region IStatusesAndErrors

		ZString IStatusesAndErrors.LineNumber
		{
			get { return ZString.Empty; }
		}

		ZString IStatusesAndErrors.Code
		{
			get { return MessageIdentificationCode; }
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

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ExtractReferenceFilesResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.ExtractReferenceResponse)]
	public partial class ERFF101 : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ExtractReferenceFilesResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.ExtractReferenceResponse)]
	public partial class ERFF201 : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ExtractReferenceFilesResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.ExtractReferenceResponse)]
	public partial class ERFF202 : MessageBlock
	{
		protected override void AdjustEndDates()
		{
			RateColumnEndDate = AdjustedEndDate(RateColumnBeginDate, RateColumnEndDate);
			RestrictionIndicatorEndDate = AdjustedEndDate(RestrictionIndicatorBeginDate, RestrictionIndicatorEndDate);
			SpecialProgramsIndicatorEndDate = AdjustedEndDate(SpecialProgramsIndicatorBeginDate, SpecialProgramsIndicatorEndDate);
			AGOACBTPAATPDEASpecialProgramsIndicatorEndDate = AdjustedEndDate(AGOACBTPAATPDEASpecialProgramsIndicatorBeginDate, AGOACBTPAATPDEASpecialProgramsIndicatorEndDate);
			MiscellaneousSPIEndDate = AdjustedEndDate(MiscellaneousSPIBeginDate, MiscellaneousSPIEndDate);
		}
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ExtractReferenceFilesResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.ExtractReferenceResponse)]
	public partial class ERFF301 : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ExtractReferenceFilesResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.ExtractReferenceResponse)]
	public partial class ERFF401 : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ExtractReferenceFilesResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.ExtractReferenceResponse)]
	public partial class ERFF102 : MessageBlock
	{
		protected override void AdjustEndDates()
		{
			GeneralizedSystemOfPreferencesGSPEndDate = AdjustedEndDate(GeneralizedSystemOfPreferencesGSPBeginDate, GeneralizedSystemOfPreferencesGSPEndDate);
		}
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ExtractReferenceFilesResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.ExtractReferenceResponse)]
	public partial class ERFF104 : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ExtractReferenceFilesResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.ExtractReferenceResponse)]
	public partial class ERFF105 : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ExtractReferenceFilesResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.ExtractReferenceResponse)]
	public partial class ERFF106 : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ExtractReferenceFilesResponse)]
	public partial class ERFF107 : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ExtractReferenceFilesResponse)]
	public partial class ERFF108 : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ExtractReferenceFilesResponse)]
	public partial class ERFF109 : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ExtractReferenceFilesResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.HarmonizedTariffScheduleExtractReferenceQueryResponse)]
	public partial class ERFF110 : MessageBlock, IStatusesAndErrors
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

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ExtractReferenceFilesResponse)]
	public partial class ERFEError : MessageBlock, IStatusesAndErrors
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

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ExtractReferenceFilesResponse)]
	public partial class ERFEF106 : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ExtractReferenceFilesResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.ExtractReferenceResponse)]
	public partial class ERFF211 : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ExtractReferenceFilesResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.ExtractReferenceResponse)]
	public partial class ERFF311 : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ExtractReferenceFilesResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.ExtractReferenceResponse)]
	public partial class ERFF411 : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ExtractReferenceFilesResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.ExtractReferenceResponse)]
	public partial class ERFF112 : MessageBlock { }
}