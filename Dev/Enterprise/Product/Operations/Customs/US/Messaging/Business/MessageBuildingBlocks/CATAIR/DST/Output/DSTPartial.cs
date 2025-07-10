namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output
{
	using System.Collections.Generic;
	using CargoWise.Types;

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.DailyStatement)]
	public partial class DSTQ1 : MessageBlock, IDailyStatementDutyAndTax
	{
		ZString IDailyStatementDutyAndTax.EntryFilerCode
		{
			get { return EntryFilerCode; }
		}

		ZString IDailyStatementDutyAndTax.EntryNumber
		{
			get { return EntryNumber; }
		}

		ZString IDailyStatementDutyAndTax.EntryType
		{
			get { return EntryType; }
		}

		ZString IDailyStatementDutyAndTax.DistrictPortOfEntrySummary
		{
			get { return DistrictPortOfEntrySummary; }
		}

		ZDecimal IDailyStatementDutyAndTax.EstimatedDutyAmount
		{
			get { return EstimatedDutyAmount; }
		}

		ZDecimal IDailyStatementDutyAndTax.EstimatedTaxAmount
		{
			get { return EstimatedTaxAmount; }
		}

		ZString IDailyStatementDutyAndTax.DeferredTaxIndicator
		{
			get { return DeferredTaxIndicator; }
		}

		ZDecimal IDailyStatementDutyAndTax.CountervailingDutyAmount
		{
			get { return CountervailingDutyAmount; }
		}

		ZDecimal IDailyStatementDutyAndTax.AntidumpingDutyAmount
		{
			get { return AntidumpingDutyAmount; }
		}

		ZString IDailyStatementDutyAndTax.BrokerReferenceNumber
		{
			get { return BrokerReferenceNumber; }
		}

		ZString IDailyStatementDutyAndTax.MessageType
		{
			get { return ApplicationIdentifierCodeList.Codes.DailyStatement; }
		}
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.DailyStatement)]
	public partial class DSTQ2 : MessageBlock, IStatementFees, IDailyStatementDutyAndTaxContinued
	{
		IEnumerable<KeyValuePair<ZString, ZDecimal>> IStatementFees.Fees
		{
			get
			{
				if (!SugarFee.IsEmpty)
				{
					yield return new KeyValuePair<ZString, ZDecimal>(Core.Constants.USCustoms.FeeCodes.Sugar, SugarFee);
				}
				if (!MailFeeAmount.IsEmpty)
				{
					yield return new KeyValuePair<ZString, ZDecimal>(Core.Constants.USCustoms.FeeCodes.DutiableMail, MailFeeAmount);
				}
			}
		}

		ZString IDailyStatementDutyAndTaxContinued.TeamNumber
		{
			get { return TeamNumber; }
		}

		ZString IDailyStatementDutyAndTaxContinued.CensusWarningIndicator
		{
			get { return CensusWarningIndicator; }
		}

		ZString IDailyStatementDutyAndTaxContinued.ACEIndicator
		{
			get { return ACEIndicator; }
		}

		ZString IDailyStatementDutyAndTaxContinued.PaperlessElectronicIndicator
		{
			get { return PaperlessElectronicIndicator; }
		}

		ZString IDailyStatementDutyAndTaxContinued.ElectronicInvoiceIndicator
		{
			get { return ElectronicInvoiceIndicator; }
		}

		ZString IDailyStatementDutyAndTaxContinued.PaymentTypeIndicator
		{
			get { return PaymentTypeIndicator; }
		}

		ZDecimal IDailyStatementDutyAndTaxContinued.CountervailingDutyAmount
		{
			get { return ZDecimal.Zero; }
		}

		ZDecimal IDailyStatementDutyAndTaxContinued.AntidumpingDutyAmount
		{
			get { return ZDecimal.Zero; }
		}

		ZString IDailyStatementDutyAndTaxContinued.CountervailingIndicator
		{
			get { return CountervailingIndicator; }
		}

		ZString IDailyStatementDutyAndTaxContinued.AntidumpingIndicator
		{
			get { return AntidumpingIndicator; }
		}
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.DailyStatement)]
	public partial class DSTQ3 : MessageBlock, IStatementFees
	{
		IEnumerable<KeyValuePair<ZString, ZDecimal>> IStatementFees.Fees
		{
			get
			{
				if (!TotalEstimatedDuty.IsEmpty)
				{
					yield return new KeyValuePair<ZString, ZDecimal>(Core.Constants.USCustoms.FeeCodes.Duty, TotalEstimatedDuty);
				}
				if (!TotalEstimatedTax.IsEmpty)
				{
					yield return new KeyValuePair<ZString, ZDecimal>(Core.Constants.USCustoms.FeeCodes.ExciseTaxPayable, TotalEstimatedTax);
				}
				if (!TotalDeferredTax.IsEmpty)
				{
					yield return new KeyValuePair<ZString, ZDecimal>(Core.Constants.USCustoms.FeeCodes.ExciseTaxDeferred, TotalDeferredTax);
				}
				if (!TotalAntidumpingDuty.IsEmpty)
				{
					yield return new KeyValuePair<ZString, ZDecimal>(Core.Constants.USCustoms.FeeCodes.AntidumpingDuty, TotalAntidumpingDuty);
				}
				if (!TotalCountervailingDuty.IsEmpty)
				{
					yield return new KeyValuePair<ZString, ZDecimal>(Core.Constants.USCustoms.FeeCodes.CountervailingDuty, TotalCountervailingDuty);
				}
			}
		}
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.DailyStatement)]
	public partial class DSTQ4 : MessageBlock, IStatementFees
	{
		IEnumerable<KeyValuePair<ZString, ZDecimal>> IStatementFees.Fees
		{
			get
			{
				if (!TotalCottonFee.IsEmpty)
				{
					yield return new KeyValuePair<ZString, ZDecimal>(Core.Constants.USCustoms.FeeCodes.Cotton, TotalCottonFee);
				}
				if (!TotalInterestAmountForReconciliationSummary.IsEmpty)
				{
					yield return new KeyValuePair<ZString, ZDecimal>(Core.Constants.USCustoms.FeeCodes.ReconciliationInterest, TotalInterestAmountForReconciliationSummary);
				}
				if (!TotalSugarFee.IsEmpty)
				{
					yield return new KeyValuePair<ZString, ZDecimal>(Core.Constants.USCustoms.FeeCodes.Sugar, TotalSugarFee);
				}
				if (!TotalAmountDue.IsEmpty)
				{
					yield return new KeyValuePair<ZString, ZDecimal>(Constants.TotalAmountDue, TotalAmountDue);
				}
				if (!TotalNumberRevenueProducingEntries.IsEmpty)
				{
					yield return new KeyValuePair<ZString, ZDecimal>(Constants.TotalNumberRevenueProducingEntries, (ZDecimal)TotalNumberRevenueProducingEntries);
				}
				if (!TotalNumberNonRevenueProducingEntries.IsEmpty)
				{
					yield return new KeyValuePair<ZString, ZDecimal>(Constants.TotalNumberNonRevenueProducingEntries, (ZDecimal)TotalNumberNonRevenueProducingEntries);
				}
			}
		}
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.DailyStatement)]
	public partial class DSTQ5 : MessageBlock, IStatementFees
	{
		IEnumerable<KeyValuePair<ZString, ZDecimal>> IStatementFees.Fees
		{
			get
			{
				if (!TotalEstimatedDuty.IsEmpty)
				{
					yield return new KeyValuePair<ZString, ZDecimal>(Core.Constants.USCustoms.FeeCodes.Duty, TotalEstimatedDuty);
				}
				if (!TotalEstimatedTax.IsEmpty)
				{
					yield return new KeyValuePair<ZString, ZDecimal>(Core.Constants.USCustoms.FeeCodes.ExciseTaxPayable, TotalEstimatedTax);
				}
				if (!TotalDeferredTax.IsEmpty)
				{
					yield return new KeyValuePair<ZString, ZDecimal>(Core.Constants.USCustoms.FeeCodes.ExciseTaxDeferred, TotalDeferredTax);
				}
				if (!TotalAntidumpingDuty.IsEmpty)
				{
					yield return new KeyValuePair<ZString, ZDecimal>(Core.Constants.USCustoms.FeeCodes.AntidumpingDuty, TotalAntidumpingDuty);
				}
				if (!TotalCountervailingDuty.IsEmpty)
				{
					yield return new KeyValuePair<ZString, ZDecimal>(Core.Constants.USCustoms.FeeCodes.CountervailingDuty, TotalCountervailingDuty);
				}
			}
		}
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.DailyStatement)]
	public partial class DSTQ6 : MessageBlock, IStatementFees
	{
		IEnumerable<KeyValuePair<ZString, ZDecimal>> IStatementFees.Fees
		{
			get
			{
				if (!TotalCottonFee.IsEmpty)
				{
					yield return new KeyValuePair<ZString, ZDecimal>(Core.Constants.USCustoms.FeeCodes.Cotton, TotalCottonFee);
				}
				if (!TotalInterestAmountForReconciliationSummary.IsEmpty)
				{
					yield return new KeyValuePair<ZString, ZDecimal>(Core.Constants.USCustoms.FeeCodes.ReconciliationInterest, TotalInterestAmountForReconciliationSummary);
				}
				if (!TotalSugarFee.IsEmpty)
				{
					yield return new KeyValuePair<ZString, ZDecimal>(Core.Constants.USCustoms.FeeCodes.Sugar, TotalSugarFee);
				}
				if (!TotalAmountDue.IsEmpty)
				{
					yield return new KeyValuePair<ZString, ZDecimal>(Constants.TotalAmountDue, TotalAmountDue);
				}
				if (!TotalNumberRevenueProducingEntries.IsEmpty)
				{
					yield return new KeyValuePair<ZString, ZDecimal>(Constants.TotalNumberRevenueProducingEntries, (ZDecimal)TotalNumberRevenueProducingEntries);
				}
				if (!TotalNumberNonRevenueProducingEntries.IsEmpty)
				{
					yield return new KeyValuePair<ZString, ZDecimal>(Constants.TotalNumberNonRevenueProducingEntries, (ZDecimal)TotalNumberNonRevenueProducingEntries);
				}
			}
		}
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.DailyStatement)]
	public partial class DSTQ7 : MessageBlock, IStatementDeletedEntries
	{
		ZString IStatementDeletedEntries.StatementNumber
		{
			get { return StatementNumber; }
		}

		IEnumerable<KeyValuePair<ZString, ZString>> IStatementDeletedEntries.DeletedEntries
		{
			get
			{
				if (!EntryNumber.IsEmpty)
				{
					yield return new KeyValuePair<ZString, ZString>(EntryNumber, DeleteSource);
				}

				if (!EntryNumber1.IsEmpty)
				{
					yield return new KeyValuePair<ZString, ZString>(EntryNumber1, DeleteSource1);
				}

				if (!EntryNumber2.IsEmpty)
				{
					yield return new KeyValuePair<ZString, ZString>(EntryNumber2, DeleteSource2);
				}

				if (!EntryNumber3.IsEmpty)
				{
					yield return new KeyValuePair<ZString, ZString>(EntryNumber3, DeleteSource3);
				}
			}
		}
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.DailyStatement)]
	public partial class DSTQA : MessageBlock, IStatementFees
	{
		public IEnumerable<KeyValuePair<ZString, ZDecimal>> Fees
		{
			get
			{
				if (!BeefFee.IsEmpty)
				{
					yield return new KeyValuePair<ZString, ZDecimal>(Core.Constants.USCustoms.FeeCodes.Beef, BeefFee);
				}
				if (!PorkFee.IsEmpty)
				{
					yield return new KeyValuePair<ZString, ZDecimal>(Core.Constants.USCustoms.FeeCodes.Pork, PorkFee);
				}
				if (!CottonFee.IsEmpty)
				{
					yield return new KeyValuePair<ZString, ZDecimal>(Core.Constants.USCustoms.FeeCodes.Cotton, CottonFee);
				}
				if (!MerchandiseProcessingFee.IsEmpty)
				{
					yield return new KeyValuePair<ZString, ZDecimal>(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, MerchandiseProcessingFee);
				}
				if (!HoneyFee.IsEmpty)
				{
					yield return new KeyValuePair<ZString, ZDecimal>(Core.Constants.USCustoms.FeeCodes.Honey, HoneyFee);
				}
				if (!HarborMaintenanceFee.IsEmpty)
				{
					yield return new KeyValuePair<ZString, ZDecimal>(Core.Constants.USCustoms.FeeCodes.HMF, HarborMaintenanceFee);
				}
				if (!InformalMerchandiseProcessingFee.IsEmpty)
				{
					yield return new KeyValuePair<ZString, ZDecimal>(Core.Constants.USCustoms.FeeCodes.MerchandiseInformal, InformalMerchandiseProcessingFee);
				}
				if (!ManualSurcharge.IsEmpty)
				{
					yield return new KeyValuePair<ZString, ZDecimal>(Core.Constants.USCustoms.FeeCodes.MerchandiseSurcharge, ManualSurcharge);
				}
			}
		}
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.DailyStatement)]
	public partial class DSTQB : MessageBlock, IStatementFees
	{
		public IEnumerable<KeyValuePair<ZString, ZDecimal>> Fees
		{
			get
			{
				if (!RaspberryFee.IsEmpty)
				{
					yield return new KeyValuePair<ZString, ZDecimal>(Core.Constants.USCustoms.FeeCodes.Raspberry, RaspberryFee);
				}
				if (!PotatoFee.IsEmpty)
				{
					yield return new KeyValuePair<ZString, ZDecimal>(Core.Constants.USCustoms.FeeCodes.Potato, PotatoFee);
				}
				if (!LimeFee.IsEmpty)
				{
					yield return new KeyValuePair<ZString, ZDecimal>(Core.Constants.USCustoms.FeeCodes.FreshLimes, LimeFee);
				}
				if (!MushroomFee.IsEmpty)
				{
					yield return new KeyValuePair<ZString, ZDecimal>(Core.Constants.USCustoms.FeeCodes.Mushroom, MushroomFee);
				}
				if (!WatermelonFee.IsEmpty)
				{
					yield return new KeyValuePair<ZString, ZDecimal>(Core.Constants.USCustoms.FeeCodes.Watermelon, WatermelonFee);
				}
				if (!InterestAmountForReconciliationSummary.IsEmpty)
				{
					yield return new KeyValuePair<ZString, ZDecimal>(Core.Constants.USCustoms.FeeCodes.ReconciliationInterest, InterestAmountForReconciliationSummary);
				}
			}
		}
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.DailyStatement)]
	public partial class DSTQC : MessageBlock, IStatementFees
	{
		IEnumerable<KeyValuePair<ZString, ZDecimal>> IStatementFees.Fees
		{
			get
			{
				if (!BlueberryFee.IsEmpty)
				{
					yield return new KeyValuePair<ZString, ZDecimal>(Core.Constants.USCustoms.FeeCodes.Blueberry, BlueberryFee);
				}
				if (!HassAvocadoFee.IsEmpty)
				{
					yield return new KeyValuePair<ZString, ZDecimal>(Core.Constants.USCustoms.FeeCodes.Avocado, HassAvocadoFee);
				}
				if (!MangoFee.IsEmpty)
				{
					yield return new KeyValuePair<ZString, ZDecimal>(Core.Constants.USCustoms.FeeCodes.Mango, MangoFee);
				}
				if (!SorghumFee.IsEmpty)
				{
					yield return new KeyValuePair<ZString, ZDecimal>(Core.Constants.USCustoms.FeeCodes.Sorghum, SorghumFee);
				}
				if (!DairyFee.IsEmpty)
				{
					yield return new KeyValuePair<ZString, ZDecimal>(Core.Constants.USCustoms.FeeCodes.DairyFee, DairyFee);
				}
			}
		}
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.DailyStatement)]
	public partial class DSTQE : MessageBlock, IStatementFees
	{
		IEnumerable<KeyValuePair<ZString, ZDecimal>> IStatementFees.Fees
		{
			get
			{
				if (!TotalMailFee.IsEmpty)
				{
					yield return new KeyValuePair<ZString, ZDecimal>(Core.Constants.USCustoms.FeeCodes.DutiableMail, TotalMailFee);
				}
				if (!TotalBeefFee.IsEmpty)
				{
					yield return new KeyValuePair<ZString, ZDecimal>(Core.Constants.USCustoms.FeeCodes.Beef, TotalBeefFee);
				}
				if (!TotalPorkFee.IsEmpty)
				{
					yield return new KeyValuePair<ZString, ZDecimal>(Core.Constants.USCustoms.FeeCodes.Pork, TotalPorkFee);
				}
				if (!TotalMerchandiseProcessingFee.IsEmpty)
				{
					yield return new KeyValuePair<ZString, ZDecimal>(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, TotalMerchandiseProcessingFee);
				}
				if (!TotalHoneyFee.IsEmpty)
				{
					yield return new KeyValuePair<ZString, ZDecimal>(Core.Constants.USCustoms.FeeCodes.Honey, TotalHoneyFee);
				}
				if (!TotalHarborMaintenanceFeeWaterways.IsEmpty)
				{
					yield return new KeyValuePair<ZString, ZDecimal>(Core.Constants.USCustoms.FeeCodes.HMF, TotalHarborMaintenanceFeeWaterways);
				}
				if (!TotalManualSurcharge.IsEmpty)
				{
					yield return new KeyValuePair<ZString, ZDecimal>(Core.Constants.USCustoms.FeeCodes.MerchandiseSurcharge, TotalManualSurcharge);
				}
				if (!TotalInformalMerchandiseProcessingFee.IsEmpty)
				{
					yield return new KeyValuePair<ZString, ZDecimal>(Core.Constants.USCustoms.FeeCodes.MerchandiseInformal, TotalInformalMerchandiseProcessingFee);
				}
			}
		}
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.DailyStatement)]
	public partial class DSTQJ : MessageBlock, IStatementFees
	{
		IEnumerable<KeyValuePair<ZString, ZDecimal>> IStatementFees.Fees
		{
			get
			{
				if (!TotalMailFee.IsEmpty)
				{
					yield return new KeyValuePair<ZString, ZDecimal>(Core.Constants.USCustoms.FeeCodes.DutiableMail, TotalMailFee);
				}
				if (!TotalBeefFee.IsEmpty)
				{
					yield return new KeyValuePair<ZString, ZDecimal>(Core.Constants.USCustoms.FeeCodes.Beef, TotalBeefFee);
				}
				if (!TotalPorkFee.IsEmpty)
				{
					yield return new KeyValuePair<ZString, ZDecimal>(Core.Constants.USCustoms.FeeCodes.Pork, TotalPorkFee);
				}
				if (!TotalMerchandiseProcessingFee.IsEmpty)
				{
					yield return new KeyValuePair<ZString, ZDecimal>(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, TotalMerchandiseProcessingFee);
				}
				if (!TotalHoneyFee.IsEmpty)
				{
					yield return new KeyValuePair<ZString, ZDecimal>(Core.Constants.USCustoms.FeeCodes.Honey, TotalHoneyFee);
				}
				if (!TotalHarborMaintenanceFeeWaterways.IsEmpty)
				{
					yield return new KeyValuePair<ZString, ZDecimal>(Core.Constants.USCustoms.FeeCodes.HMF, TotalHarborMaintenanceFeeWaterways);
				}
				if (!TotalManualSurcharge.IsEmpty)
				{
					yield return new KeyValuePair<ZString, ZDecimal>(Core.Constants.USCustoms.FeeCodes.MerchandiseSurcharge, TotalManualSurcharge);
				}
				if (!TotalInformalMerchandiseProcessingFee.IsEmpty)
				{
					yield return new KeyValuePair<ZString, ZDecimal>(Core.Constants.USCustoms.FeeCodes.MerchandiseInformal, TotalInformalMerchandiseProcessingFee);
				}
			}
		}
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ABIStatementACHPaymentRerouteResponse)]
	public partial class DSTQX : MessageBlock, IStatementRerouteResponse
	{
		#region IRerouteResponse Members

		ZString IStatementRerouteResponse.ErrorCode
		{
			get { return ErrorCode; }
		}

		ZString IStatementRerouteResponse.MessageText
		{
			get { return MessageText; }
		}

		ZInt IStatementRerouteResponse.TotalNumberOfReroutes
		{
			get { return TotalNumberOfReroutes; }
		}

		#endregion
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.DailyStatement)]
	public partial class DSTQF : MessageBlock, IStatementFees
	{
		IEnumerable<KeyValuePair<ZString, ZDecimal>> IStatementFees.Fees
		{
			get
			{
				if (!TotalRaspberryFee.IsEmpty)
				{
					yield return new KeyValuePair<ZString, ZDecimal>(Core.Constants.USCustoms.FeeCodes.Raspberry, TotalRaspberryFee);
				}
				if (!TotalPotatoFee.IsEmpty)
				{
					yield return new KeyValuePair<ZString, ZDecimal>(Core.Constants.USCustoms.FeeCodes.Potato, TotalPotatoFee);
				}
				if (!TotalLimeFee.IsEmpty)
				{
					yield return new KeyValuePair<ZString, ZDecimal>(Core.Constants.USCustoms.FeeCodes.FreshLimes, TotalLimeFee);
				}
				if (!TotalMushroomFee.IsEmpty)
				{
					yield return new KeyValuePair<ZString, ZDecimal>(Core.Constants.USCustoms.FeeCodes.Mushroom, TotalMushroomFee);
				}
				if (!TotalWatermelonFee.IsEmpty)
				{
					yield return new KeyValuePair<ZString, ZDecimal>(Core.Constants.USCustoms.FeeCodes.Watermelon, TotalWatermelonFee);
				}
				if (!TotalSheepFee.IsEmpty)
				{
					yield return new KeyValuePair<ZString, ZDecimal>(Core.Constants.USCustoms.FeeCodes.SoftwoodLumber, TotalSheepFee);
				}
				if (!TotalBlueberryFee.IsEmpty)
				{
					yield return new KeyValuePair<ZString, ZDecimal>(Core.Constants.USCustoms.FeeCodes.Blueberry, TotalBlueberryFee);
				}
			}
		}
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.DailyStatement)]
	public partial class DSTQK : MessageBlock, IStatementFees
	{
		IEnumerable<KeyValuePair<ZString, ZDecimal>> IStatementFees.Fees
		{
			get
			{
				if (!TotalRaspberryFee.IsEmpty)
				{
					yield return new KeyValuePair<ZString, ZDecimal>(Core.Constants.USCustoms.FeeCodes.Raspberry, TotalRaspberryFee);
				}
				if (!TotalPotatoFee.IsEmpty)
				{
					yield return new KeyValuePair<ZString, ZDecimal>(Core.Constants.USCustoms.FeeCodes.Potato, TotalPotatoFee);
				}
				if (!TotalLimeFee.IsEmpty)
				{
					yield return new KeyValuePair<ZString, ZDecimal>(Core.Constants.USCustoms.FeeCodes.FreshLimes, TotalLimeFee);
				}
				if (!TotalMushroomFee.IsEmpty)
				{
					yield return new KeyValuePair<ZString, ZDecimal>(Core.Constants.USCustoms.FeeCodes.Mushroom, TotalMushroomFee);
				}
				if (!TotalWatermelonFee.IsEmpty)
				{
					yield return new KeyValuePair<ZString, ZDecimal>(Core.Constants.USCustoms.FeeCodes.Watermelon, TotalWatermelonFee);
				}
				if (!TotalSheepFee.IsEmpty)
				{
					yield return new KeyValuePair<ZString, ZDecimal>(Core.Constants.USCustoms.FeeCodes.SoftwoodLumber, TotalSheepFee);
				}
				if (!TotalBlueberryFee.IsEmpty)
				{
					yield return new KeyValuePair<ZString, ZDecimal>(Core.Constants.USCustoms.FeeCodes.Blueberry, TotalBlueberryFee);
				}
			}
		}
	}

	public partial class DSTQL : MessageBlock, IStatementFees
	{
		IEnumerable<KeyValuePair<ZString, ZDecimal>> IStatementFees.Fees
		{
			get
			{
				if (!TotalHassAvocadoFee.IsEmpty)
				{
					yield return new KeyValuePair<ZString, ZDecimal>(Core.Constants.USCustoms.FeeCodes.Avocado, TotalHassAvocadoFee);
				}
				if (!TotalMangoFee.IsEmpty)
				{
					yield return new KeyValuePair<ZString, ZDecimal>(Core.Constants.USCustoms.FeeCodes.Mango, TotalMangoFee);
				}
				if (!TotalSorghumFee.IsEmpty)
				{
					yield return new KeyValuePair<ZString, ZDecimal>(Core.Constants.USCustoms.FeeCodes.Sorghum, TotalSorghumFee);
				}
				if (!TotalDairyFee.IsEmpty)
				{
					yield return new KeyValuePair<ZString, ZDecimal>(Core.Constants.USCustoms.FeeCodes.DairyFee, TotalDairyFee);
				}
			}
		}
	}

	public partial class DSTQG : MessageBlock, IStatementFees
	{
		IEnumerable<KeyValuePair<ZString, ZDecimal>> IStatementFees.Fees
		{
			get
			{
				if (!TotalHassAvocadoFee.IsEmpty)
				{
					yield return new KeyValuePair<ZString, ZDecimal>(Core.Constants.USCustoms.FeeCodes.Avocado, TotalHassAvocadoFee);
				}
				if (!TotalMangoFee.IsEmpty)
				{
					yield return new KeyValuePair<ZString, ZDecimal>(Core.Constants.USCustoms.FeeCodes.Mango, TotalMangoFee);
				}
				if (!TotalSorghumFee.IsEmpty)
				{
					yield return new KeyValuePair<ZString, ZDecimal>(Core.Constants.USCustoms.FeeCodes.Sorghum, TotalSorghumFee);
				}
				if (!TotalDairyFee.IsEmpty)
				{
					yield return new KeyValuePair<ZString, ZDecimal>(Core.Constants.USCustoms.FeeCodes.DairyFee, TotalDairyFee);
				}
			}
		}
	}
}
