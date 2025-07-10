using System.Collections.Generic;
using CargoWise.Types;
using ACEAppCode = Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACEApplicationIdentifierCodeList.Codes;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common.Abstract
{
	[ApplicationIdentifier(ACEAppCode.EntrySummary)]
	[ApplicationIdentifier(ACEAppCode.EntrySummaryResponse)]
	[ApplicationIdentifier(ACEAppCode.EntrySummaryQueryResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.ACEBIRDTransaction)]
	partial class AENS10 : IENS10, IENSDeferredTaxIndicator
	{
		ZString IENS10.EntryType
		{
			get { return EntryTypeCode; }
		}

		ZString IENSDeferredTaxIndicator.DeferredTaxIndicator
		{
			get { return DeferredTaxPaymentCode; }
		}
	}

	[ApplicationIdentifier(ACEAppCode.EntrySummary)]
	[ApplicationIdentifier(ACEAppCode.EntrySummaryResponse)]
	[ApplicationIdentifier(ACEAppCode.EntrySummaryQueryResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.ACEBIRDTransaction)]
	partial class AENS11
	{
	}

	[ApplicationIdentifier(ACEAppCode.EntrySummary)]
	[ApplicationIdentifier(ACEAppCode.EntrySummaryResponse)]
	[ApplicationIdentifier(ACEAppCode.EntrySummaryQueryResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.ACEBIRDTransaction)]
	partial class AENS20
	{
	}

	[ApplicationIdentifier(ACEAppCode.EntrySummary)]
	[ApplicationIdentifier(ACEAppCode.EntrySummaryResponse)]
	[ApplicationIdentifier(ACEAppCode.EntrySummaryQueryResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.ACEBIRDTransaction)]
	partial class AENS21
	{
	}

	[ApplicationIdentifier(ACEAppCode.EntrySummary)]
	[ApplicationIdentifier(ACEAppCode.EntrySummaryResponse)]
	[ApplicationIdentifier(ACEAppCode.EntrySummaryQueryResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.ACEBIRDTransaction)]
	partial class AENS30
	{
	}

	[ApplicationIdentifier(ACEAppCode.EntrySummary)]
	[ApplicationIdentifier(ACEAppCode.EntrySummaryResponse)]
	[ApplicationIdentifier(ACEAppCode.EntrySummaryQueryResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.ACEBIRDTransaction)]
	partial class AENS31
	{
	}

	[ApplicationIdentifier(ACEAppCode.EntrySummary)]
	[ApplicationIdentifier(ACEAppCode.EntrySummaryResponse)]
	[ApplicationIdentifier(ACEAppCode.EntrySummaryQueryResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.ACEBIRDTransaction)]
	partial class AENS32
	{
	}

	[ApplicationIdentifier(ACEAppCode.EntrySummary)]
	[ApplicationIdentifier(ACEAppCode.EntrySummaryResponse)]
	[ApplicationIdentifier(ACEAppCode.EntrySummaryQueryResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.ACEBIRDTransaction)]
	partial class AENS33
	{
	}

	[ApplicationIdentifier(ACEAppCode.EntrySummary)]
	[ApplicationIdentifier(ACEAppCode.EntrySummaryResponse)]
	[ApplicationIdentifier(ACEAppCode.EntrySummaryQueryResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.ACEBIRDTransaction)]
	partial class AENS34
	{
	}

	[ApplicationIdentifier(ACEAppCode.EntrySummary)]
	[ApplicationIdentifier(ACEAppCode.EntrySummaryResponse)]
	[ApplicationIdentifier(ACEAppCode.EntrySummaryQueryResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.ACEBIRDTransaction)]
	partial class AENS40
	{
	}

	[ApplicationIdentifier(ACEAppCode.EntrySummary)]
	[ApplicationIdentifier(ACEAppCode.EntrySummaryResponse)]
	[ApplicationIdentifier(ACEAppCode.EntrySummaryQueryResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.ACEBIRDTransaction)]
	partial class AENS41
	{
	}

	[ApplicationIdentifier(ACEAppCode.EntrySummary)]
	[ApplicationIdentifier(ACEAppCode.EntrySummaryResponse)]
	[ApplicationIdentifier(ACEAppCode.EntrySummaryQueryResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.ACEBIRDTransaction)]
	partial class AENS43
	{
	}

	[ApplicationIdentifier(ACEAppCode.EntrySummary)]
	[ApplicationIdentifier(ACEAppCode.EntrySummaryResponse)]
	[ApplicationIdentifier(ACEAppCode.EntrySummaryQueryResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.ACEBIRDTransaction)]
	partial class AENS47
	{
	}

	[ApplicationIdentifier(ACEAppCode.EntrySummary)]
	[ApplicationIdentifier(ACEAppCode.EntrySummaryResponse)]
	[ApplicationIdentifier(ACEAppCode.EntrySummaryQueryResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.ACEBIRDTransaction)]
	partial class AENS50
	{
	}

	[ApplicationIdentifier(ACEAppCode.EntrySummary)]
	[ApplicationIdentifier(ACEAppCode.EntrySummaryResponse)]
	[ApplicationIdentifier(ACEAppCode.EntrySummaryQueryResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.ACEBIRDTransaction)]
	partial class AENS51
	{
	}

	[ApplicationIdentifier(ACEAppCode.EntrySummary)]
	[ApplicationIdentifier(ACEAppCode.EntrySummaryResponse)]
	[ApplicationIdentifier(ACEAppCode.EntrySummaryQueryResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.ACEBIRDTransaction)]
	partial class AENS52 : IACELicenceAndPermit
	{
		ZString IACELicenceAndPermit.LicenseCertificatePermitTypeCode
		{
			get { return LicenseCertificatePermitTypeCode; }
		}

		ZString IACELicenceAndPermit.LicenseNumberCertificateNumberPermitNumber
		{
			get { return LicenseNumberCertificateNumberPermitNumber; }
		}
	}

	[ApplicationIdentifier(ACEAppCode.EntrySummary)]
	[ApplicationIdentifier(ACEAppCode.EntrySummaryResponse)]
	[ApplicationIdentifier(ACEAppCode.EntrySummaryQueryResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.ACEBIRDTransaction)]
	partial class AENS53
	{
	}

	[ApplicationIdentifier(ACEAppCode.EntrySummary)]
	[ApplicationIdentifier(ACEAppCode.EntrySummaryResponse)]
	[ApplicationIdentifier(ACEAppCode.EntrySummaryQueryResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.ACEBIRDTransaction)]
	partial class AENS54 : IExtendedFieldsHumanFriendlySerialiserSupporter
	{
		ZString IExtendedFieldsHumanFriendlySerialiserSupporter.ExtendedFieldNameToSerialise
		{
			get { return nameof(ImportersAdditionalDeclarationInformation); }
		}

		Dictionary<ZString, Dictionary<ZString, MessageBlockAttribute>> IExtendedFieldsHumanFriendlySerialiserSupporter.GetExtendedFieldsMappings(ZString extendedFieldName)
		{
			var result = new Dictionary<ZString, Dictionary<ZString, MessageBlockAttribute>>();
			switch (ImportersAdditionalDeclarationTypeCode)
			{
				case AdditionalDeclarationTypeCodeList.Codes._01:
					result.Add(extendedFieldName, GetExtendedFieldsMapping_01());
					break;
				case AdditionalDeclarationTypeCodeList.Codes._02:
					result.Add(extendedFieldName, GetExtendedFieldsMapping_02());
					break;
				case AdditionalDeclarationTypeCodeList.Codes._03:
					result.Add(extendedFieldName, GetExtendedFieldsMapping_03());
					break;
				case AdditionalDeclarationTypeCodeList.Codes._04:
					result.Add(extendedFieldName, GetExtendedFieldsMapping_04());
					break;
				case AdditionalDeclarationTypeCodeList.Codes._05:
					result.Add(extendedFieldName, GetExtendedFieldsMapping_05());
					break;
				case AdditionalDeclarationTypeCodeList.Codes._06:
					result.Add(extendedFieldName, GetExtendedFieldsMapping_06());
					break;
				case AdditionalDeclarationTypeCodeList.Codes._07:
					result.Add(extendedFieldName, GetExtendedFieldsMapping_07());
					break;
				case AdditionalDeclarationTypeCodeList.Codes._08:
					result.Add(extendedFieldName, GetExtendedFieldsMapping_08());
					break;
				case AdditionalDeclarationTypeCodeList.Codes._09:
					result.Add(extendedFieldName, GetExtendedFieldsMapping_09());
					break;
				case AdditionalDeclarationTypeCodeList.Codes._10:
					result.Add(extendedFieldName, GetExtendedFieldsMapping_10());
					break;
			}

			return result;
		}

		Dictionary<ZString, MessageBlockAttribute> GetExtendedFieldsMapping_01()
		{
			var result = new Dictionary<ZString, MessageBlockAttribute>
			{
				{ "SoftwoodLumberDeclarationIndicator", new MessageBlockStringAttribute(1, 5, "C") },
				{ "SoftwoodLumberExportPrice", new MessageBlockDecimalAttribute(10, 6, "C", 0) },
				{ "SoftwoodLumberExportCharges", new MessageBlockDecimalAttribute(10, 16, "C", 0) }
			};

			return result;
		}

		Dictionary<ZString, MessageBlockAttribute> GetExtendedFieldsMapping_02()
		{
			return new Dictionary<ZString, MessageBlockAttribute> { { "ProductExclusionIdentifierSteelProduct", new MessageBlockStringAttribute(9, 5, "C") } };
		}

		Dictionary<ZString, MessageBlockAttribute> GetExtendedFieldsMapping_03()
		{
			return new Dictionary<ZString, MessageBlockAttribute> { { "ProductExclusionIdentifierAluminumProduct", new MessageBlockStringAttribute(9, 5, "C") } };
		}

		Dictionary<ZString, MessageBlockAttribute> GetExtendedFieldsMapping_04()
		{
			return new Dictionary<ZString, MessageBlockAttribute> { { "SouthKoreanExportSteelCertificate", new MessageBlockStringAttribute(9, 5, "C") } };
		}

		Dictionary<ZString, MessageBlockAttribute> GetExtendedFieldsMapping_05()
		{
			return new Dictionary<ZString, MessageBlockAttribute>
			{
				{ "ControlledGroupName", new MessageBlockStringAttribute(10, 5, "C") },
				{ "ForeignProducerIdentifier", new MessageBlockStringAttribute(18, 15, "C") },
				{ "ForeignProducerName", new MessageBlockStringAttribute(21, 33, "C") },
				{ "AllocationQuantity", new MessageBlockDecimalAttribute(12, 54, "C", 4) },
				{ "FlavorContentCreditIndicator", new MessageBlockStringAttribute(1, 66, "C") },
				{ "CBMARateDesignationCode", new MessageBlockStringAttribute(6, 67, "C") },
				{ "TTBTaxRateConfirmation", new MessageBlockStringAttribute(8, 73, "C") }
			};
		}

		Dictionary<ZString, MessageBlockAttribute> GetExtendedFieldsMapping_06()
		{
			return new Dictionary<ZString, MessageBlockAttribute> { { "ADCVDCertificationDesignation", new MessageBlockStringAttribute(10, 5, "C") } };
		}

		Dictionary<ZString, MessageBlockAttribute> GetExtendedFieldsMapping_07()
		{
			return new Dictionary<ZString, MessageBlockAttribute>
			{
				{ "PrimaryCountryOfSmeltApplicabilityCode", new MessageBlockStringAttribute(3, 5, "C") },
				{ "PrimaryCountryOfSmeltCode", new MessageBlockStringAttribute(2, 8, "C") },
				{ "SecondaryCountryOfSmeltApplicabilityCode", new MessageBlockStringAttribute(3, 10, "C") },
				{ "SecondaryCountryOfSmeltCode", new MessageBlockStringAttribute(2, 13, "C") },
				{ "CountryOfCastCode", new MessageBlockStringAttribute(2, 15, "C") }
			};
		}

		Dictionary<ZString, MessageBlockAttribute> GetExtendedFieldsMapping_08()
		{
			var result = new Dictionary<ZString, MessageBlockAttribute>
			{
				{ "CountryOfMeltAndPourCode", new MessageBlockStringAttribute(2, 5, "C") },
				{ "CountryOfMeltAndPourApplicabilityCode", new MessageBlockStringAttribute(3, 7, "C") }
			};

			return result;
		}

		Dictionary<ZString, MessageBlockAttribute> GetExtendedFieldsMapping_09()
		{
			return new Dictionary<ZString, MessageBlockAttribute> { { "201BifacialCertificationDesignation", new MessageBlockStringAttribute(13, 5, "C") } };
		}

		Dictionary<ZString, MessageBlockAttribute> GetExtendedFieldsMapping_10()
		{
			return new Dictionary<ZString, MessageBlockAttribute> { { "301Ship-to-ShoreCraneCertificationDesignation", new MessageBlockStringAttribute(11, 5, "C") } };
		}
	}

	[ApplicationIdentifier(ACEAppCode.EntrySummary)]
	[ApplicationIdentifier(ACEAppCode.EntrySummaryResponse)]
	[ApplicationIdentifier(ACEAppCode.EntrySummaryQueryResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.ACEBIRDTransaction)]
	partial class AENS60
	{
	}

	[ApplicationIdentifier(ACEAppCode.EntrySummary)]
	[ApplicationIdentifier(ACEAppCode.EntrySummaryResponse)]
	[ApplicationIdentifier(ACEAppCode.EntrySummaryQueryResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.ACEBIRDTransaction)]
	partial class AENS61 : IChargeBlock
	{
		ZString IChargeBlock.AccountingClassCode
		{
			get { return this.AccountingClassCode; }
			set { this.AccountingClassCode = value; }
		}

		ZDecimal IChargeBlock.UserFeeAmount
		{
			get { return this.UserFeeAmount; }
			set { this.UserFeeAmount = value; }
		}
	}

	[ApplicationIdentifier(ACEAppCode.EntrySummary)]
	[ApplicationIdentifier(ACEAppCode.EntrySummaryResponse)]
	[ApplicationIdentifier(ACEAppCode.EntrySummaryQueryResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.ACEBIRDTransaction)]
	partial class AENS62 : IChargeBlock
	{
		ZString IChargeBlock.AccountingClassCode
		{
			get { return this.AccountingClassCode; }
			set { this.AccountingClassCode = value; }
		}

		ZDecimal IChargeBlock.UserFeeAmount
		{
			get { return this.UserFeeAmount; }
			set { this.UserFeeAmount = value; }
		}
	}
}

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common
{
	[ApplicationIdentifier(ACEAppCode.EntrySummary)]
	[ApplicationIdentifier(ACEAppCode.EntrySummaryResponse)]
	[ApplicationIdentifier(ACEAppCode.EntrySummaryQueryResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.ACEBIRDTransaction)]
	partial class AENS22 : IBlock22BillDetails
	{
		ZString IBlock22BillDetails.ITNo { get { return ZString.Empty; } }
		ZDate IBlock22BillDetails.ITDate { get { return ZDate.Empty; } }
		ZString IBlock22BillDetails.MasterBillNumber { get { return ZString.Empty; } }
		ZString IBlock22BillDetails.HouseBillNumber { get { return ZString.Empty; } }
		ZString IBlock22BillDetails.SubHouseBillNumber { get { return ZString.Empty; } }
		ZInt IBlock22BillDetails.Quantity { get { return ManifestedQuantity; } }
		ZString IBlock22BillDetails.Unit { get { return ManifestedQuantityUnitOfMeasureCode; } }
		ZString IBlock22BillDetails.IssuerCodeOfMasterBillNumber { get { return ZString.Empty; } }
		ZString IBlock22BillDetails.IssuerCodeOfHouseBillNumber { get { return ZString.Empty; } }
		ZString IBlock22BillDetails.IssuerCodeOfSubHouseBillNumber { get { return ZString.Empty; } }
	}

	[ApplicationIdentifier(ACEAppCode.EntrySummary)]
	[ApplicationIdentifier(ACEAppCode.EntrySummaryResponse)]
	[ApplicationIdentifier(ACEAppCode.EntrySummaryQueryResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.ACEBIRDTransaction)]
	partial class AENS23
	{
	}

	[ApplicationIdentifier(ACEAppCode.EntrySummary)]
	[ApplicationIdentifier(ACEAppCode.EntrySummaryResponse)]
	[ApplicationIdentifier(ACEAppCode.EntrySummaryQueryResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.ACEBIRDTransaction)]
	partial class AENS35
	{
	}

	[ApplicationIdentifier(ACEAppCode.EntrySummary)]
	[ApplicationIdentifier(ACEAppCode.EntrySummaryResponse)]
	[ApplicationIdentifier(ACEAppCode.EntrySummaryQueryResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.ACEBIRDTransaction)]
	partial class AENS36
	{
	}

	[ApplicationIdentifier(ACEAppCode.EntrySummary)]
	[ApplicationIdentifier(ACEAppCode.EntrySummaryResponse)]
	[ApplicationIdentifier(ACEAppCode.EntrySummaryQueryResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.ACEBIRDTransaction)]
	partial class AENS42
	{
	}

	[ApplicationIdentifier(ACEAppCode.EntrySummary)]
	[ApplicationIdentifier(ACEAppCode.EntrySummaryResponse)]
	[ApplicationIdentifier(ACEAppCode.EntrySummaryQueryResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.ACEBIRDTransaction)]
	partial class AENS44
	{
	}

	[ApplicationIdentifier(ACEAppCode.EntrySummary)]
	[ApplicationIdentifier(ACEAppCode.EntrySummaryResponse)]
	[ApplicationIdentifier(ACEAppCode.EntrySummaryQueryResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.ACEBIRDTransaction)]
	partial class AENS63
	{
	}

	[ApplicationIdentifier(ACEAppCode.EntrySummary)]
	[ApplicationIdentifier(ACEAppCode.EntrySummaryResponse)]
	[ApplicationIdentifier(ACEAppCode.EntrySummaryQueryResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.ACEBIRDTransaction)]
	partial class AENS88
	{
	}

	[ApplicationIdentifier(ACEAppCode.EntrySummary)]
	[ApplicationIdentifier(ACEAppCode.EntrySummaryResponse)]
	[ApplicationIdentifier(ACEAppCode.EntrySummaryQueryResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.ACEBIRDTransaction)]
	partial class AENS89
	{
		public ZDecimal TotalFeeAmount1
		{
			get { return ParseTotalAmount(TotalFeeAmount1String); }
			set { TotalFeeAmount1String = ToStringTotalAmount(value); }
		}

		public ZDecimal TotalFeeAmount2
		{
			get { return ParseTotalAmount(TotalFeeAmount2String); }
			set { TotalFeeAmount2String = ToStringTotalAmount(value); }
		}

		public ZDecimal TotalFeeAmount3
		{
			get { return ParseTotalAmount(TotalFeeAmount3String); }
			set { TotalFeeAmount3String = ToStringTotalAmount(value); }
		}

		public ZDecimal TotalFeeAmount4
		{
			get { return ParseTotalAmount(TotalFeeAmount4String); }
			set { TotalFeeAmount4String = ToStringTotalAmount(value); }
		}

		public ZDecimal TotalFeeAmount5
		{
			get { return ParseTotalAmount(TotalFeeAmount5String); }
			set { TotalFeeAmount5String = ToStringTotalAmount(value); }
		}

		ZDecimal ParseTotalAmount(ZString amountTwoDecimalsImplied)
		{
			var result = ZDecimal.ParseSafe(amountTwoDecimalsImplied, 0);

			return result / 100m;
		}

		ZString ToStringTotalAmount(ZDecimal value)
		{
			return value.ToString(2).Replace(".", "").PadLeft(11, '0');
		}
	}

	[ApplicationIdentifier(ACEAppCode.EntrySummary)]
	[ApplicationIdentifier(ACEAppCode.EntrySummaryResponse)]
	[ApplicationIdentifier(ACEAppCode.EntrySummaryQueryResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.ACEBIRDTransaction)]
	partial class AENS90 : IENS90
	{
		ZDecimal IENS90.GetTotal(bool deferred)
		{
			return (deferred ? ZDecimal.Zero : GrandTotalIRTaxAmount) +
					GrandTotalDutyAmount +
					GrandTotalUserFeeAmount +
					GrandTotalADDutyAmount +
					GrandTotalCVDutyAmount;
		}
	}

	[ApplicationIdentifier(ACEAppCode.EntrySummary)]
	[ApplicationIdentifier(ACEAppCode.EntrySummaryResponse)]
	[ApplicationIdentifier(ACEAppCode.EntrySummaryQueryResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.ACEBIRDTransaction)]
	partial class AENSCW02
	{
	}

	[ApplicationIdentifier(ACEAppCode.EntrySummary)]
	[ApplicationIdentifier(ACEAppCode.EntrySummaryResponse)]
	[ApplicationIdentifier(ACEAppCode.EntrySummaryQueryResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.ACEBIRDTransaction)]
	partial class AENSE0
	{
	}

	[ApplicationIdentifier(ACEAppCode.EntrySummary)]
	[ApplicationIdentifier(ACEAppCode.EntrySummaryResponse)]
	[ApplicationIdentifier(ACEAppCode.EntrySummaryQueryResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.ACEBIRDTransaction)]
	partial class AENSE1 : IStatusesAndErrors, ITariffNumberStatusAndErrors, I7501Status, I7501Errors
	{
		ZString IStatusesAndErrors.LineNumber
		{
			get { return LineNumber == 0 ? "" : LineNumber.ToString(); }
		}

		ZString IStatusesAndErrors.NarrativeMessage
		{
			get { return this.NarrativeText; }
		}

		ZString IStatusesAndErrors.Code
		{
			get { return this.ConditionCode; }
		}

		ZString IStatusesAndErrors.ReferenceNumber
		{
			get { return this.BrokerReferenceNumber; }
		}

		ZString ITariffNumberStatusAndErrors.TariffNumber
		{
			get { return TariffNumber; }
		}
		public ZString TariffNumber;

		ZString ITariffNumberStatusAndErrors.PGAAgencyCode
		{
			get { return PGAAgencyCode; }
		}
		public ZString PGAAgencyCode;

		ZString ITariffNumberStatusAndErrors.PGALineNo
		{
			get { return PGALineNo; }
		}
		public ZString PGALineNo;

		ZString I7501Status.Code
		{
			get { return this.ConditionCode; }
		}

		ZString I7501Status.NarrativeMessage
		{
			get { return this.NarrativeText; }
		}

		bool I7501Status.IsMessageStatus
		{
			get { return IsMessageStatus; }
		}

		ZDateTime I7501Status.StatusDate
		{
			get { return ZDateTime.Empty; }
		}

		ZString I7501Errors.LineNumber
		{
			get { return LineNumber == 0 ? "" : LineNumber.ToString(); }
		}
		public ZShort LineNumber;

		ZString I7501Errors.Code
		{
			get { return ConditionCode; }
		}

		ZString I7501Errors.NarrativeMessage
		{
			get { return NarrativeText; }
		}

		bool I7501Errors.IsError
		{
			get { return IsError; }
		}

		bool IsMessageStatus
		{
			get { return !DispositionTypeCode.IsEmpty; }
		}

		bool IsError
		{
			get { return DispositionTypeCode.IsEmpty && (SeverityCode == "F" || SeverityCode == "W" || SeverityCode == "P" || SeverityCode == "I"); }
		}
	}

	[ApplicationIdentifier(ACEAppCode.EntrySummary)]
	[ApplicationIdentifier(ACEAppCode.EntrySummaryResponse)]
	[ApplicationIdentifier(ACEAppCode.EntrySummaryQueryResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.ACEBIRDTransaction)]
	partial class AENSFC01
	{
	}

	[ApplicationIdentifier(ACEAppCode.EntrySummary)]
	[ApplicationIdentifier(ACEAppCode.EntrySummaryResponse)]
	[ApplicationIdentifier(ACEAppCode.EntrySummaryQueryResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.ACEBIRDTransaction)]
	partial class AENSFC02
	{
	}

	[ApplicationIdentifier(ACEAppCode.EntrySummary)]
	[ApplicationIdentifier(ACEAppCode.EntrySummaryQueryResponse)]
	[ApplicationIdentifier(ACEAppCode.EntrySummaryResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.ACEBIRDTransaction)]
	partial class AENSOA
	{
	}

	namespace Abstract
	{
		[ApplicationIdentifier(ACEAppCode.EntrySummary)]
		[ApplicationIdentifier(ACEAppCode.EntrySummaryResponse)]
		[ApplicationIdentifier(ACEAppCode.EntrySummaryQueryResponse)]
		[ApplicationIdentifier(ACEAppCode.CargoRelease)]
		[ApplicationIdentifier(ACEAppCode.CargoReleaseResponse)]
		[ApplicationIdentifier(ACEAppCode.PriorNotice)]
		[ApplicationIdentifier(ACEAppCode.PriorNoticeResponse)]
		[ApplicationIdentifier(ACEAppCode.PGADataCorrection)]
		[ApplicationIdentifier(ACEAppCode.PGADataCorrectionResponse)]
		[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.InbondUpdateTransferofLiability)]
		[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.InbondUpdateTransferofLiabilityResponse)]
		[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.OtherGovernmentAgencyMessage)]
		[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.CargoReleaseTransactions)]
		[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.CargoReleaseTransactionsResponse)]
		[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BorderCargoRelease)]
		[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BorderCargoReleaseResponse)]
		[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.EntrySummaryResponse)]
		[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.EntrySummary)]
		[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.OtherAgencyEntryDataUpdate)]
		[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ParticipatingGovernmentAgencies)]
		[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BIRDTransaction)]
		[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.ACEBIRDTransaction)]
		partial class AENSOI
		{
		}
	}
}
