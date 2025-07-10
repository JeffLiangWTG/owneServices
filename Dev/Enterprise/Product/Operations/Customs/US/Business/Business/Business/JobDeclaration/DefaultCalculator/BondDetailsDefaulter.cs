using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public interface IBondDetailsDefault
	{
		//Informations needed for decision making
		IDisposable SuspendAddInfoPropertySetting { get; }
		ZBool IsReconMessageType { get; }
		OrgHeaderWrapper IORWrapper { get; }
		ZString ActivityCode { get; }
		ZDateTime EffectiveDate { get; }
		ZString EntryType { get; }

		//Fields to default on Declaration or ReconJob
		ZDecimal US_BondAmount { set; }
		ZString US_BondType { get; set; }
		ZString US_BondProducerAccNo { set; }
		ZString US_SuretyCode { set; }
	}

	public class BondDetailsDefaulter
	{
		public void DefaultWhenBondTypeChanges(IBondDetailsDefault declaration, ZString bondType)
		{
			DefaultCore(declaration, bondType, false);
		}

		public void Default(IBondDetailsDefault declaration, ZString bondType)
		{
			using (declaration.IsReconMessageType ? declaration.SuspendAddInfoPropertySetting : DisposableAction.NoAction)
			{
				DefaultCore(declaration, bondType, true);
			}
		}

		void DefaultCore(IBondDetailsDefault declaration, ZString bondType, bool defaultBondType)
		{
			if (defaultBondType && declaration.EntryType == EntryTypeList.Codes.LowValue)
			{
				declaration.US_BondType = BondTypeList.Codes.NoBondRequired;
			}
			else
			{
				var bondData = GetDefaultBondDetails(declaration, bondType);

				if (bondData == null && defaultBondType && (bondType == BondTypeList.Codes.ContinuousBond || bondType == BondTypeList.Codes.SingleTransactionBond))
				{
					bondData = GetDefaultBondDetails(declaration, ZString.Empty);
				}

				if (bondData != null)
				{
					if (defaultBondType)
					{
						declaration.US_BondType = bondData.PW_BondType;
					}

					if (BondTypeList.IsRelevantFor(declaration.US_BondType, USAddInfoSchema.Constants.US_BondAmount))
					{
						declaration.US_BondAmount = bondData.PW_BondAmount;
					}

					if (BondTypeList.IsRelevantFor(declaration.US_BondType, USAddInfoSchema.Constants.US_BondProducerAccNo))
					{
						declaration.US_BondProducerAccNo = bondData.PW_BondNumber.Left(AddInfo.Schema.US_BondProducerAccNoMaxLength);
					}

					if (BondTypeList.IsRelevantFor(declaration.US_BondType, USAddInfoSchema.Constants.US_SuretyCode))
					{
						declaration.US_SuretyCode = bondData.PW_SuretyCode;
					}
				}
			}
		}

		CusBondDetail GetDefaultBondDetails(IBondDetailsDefault declaration, ZString bondType)
		{
			CusBondDetail result = null;

			if (declaration.IORWrapper != null)
			{
				ZString activityCode = declaration.ActivityCode;
				List<ZString> activityCodes = new List<ZString>(new ZString[] { activityCode, ActivityCodeList.Codes._1a1 });

				result = declaration.IORWrapper.BondDetails.GetActiveBondDetailDataFor(activityCodes, bondType, declaration.EffectiveDate);
			}

			return result;
		}

		public CusBondDetail GetBondDetailsForAccountNo(IBondDetailsDefault declaration, ZString accountNo)
		{
			CusBondDetail bondData = null;

			if (declaration.IORWrapper != null)
			{
				bondData = declaration.IORWrapper.BondDetails.GetBondDetailForAccountNo(accountNo);
			}
			return bondData;
		}

		public ZDecimal DetermineSEBBondAmount(CusEntryHeader ensEntry, ZString bondCalcCode)
		{
			ZDecimal result = 0;
			ZDecimal minimumAmount = MinBondAmount;

			switch (bondCalcCode)
			{
				case SEBCalculationList.Codes.DEF:
					result = AutoCalcBondAmount(ensEntry);
					break;
				case SEBCalculationList.Codes.EXH:
					if (ensEntry.ExhibitionEstimatedDutiesIfEntryHadBeenForConsumption > 0)
					{
						result = ensEntry.ExhibitionEstimatedDutiesIfEntryHadBeenForConsumption;
					}
					else
					{
						result = CalculateTotalFeesIfWereNotForTIBOrExhibition(ensEntry);
					}
					break;
				case SEBCalculationList.Codes.MAN:
					result = ensEntry.Declaration.US_BondAmount;
					minimumAmount = 0m;
					break;
				case SEBCalculationList.Codes.MSC:
					result = ensEntry.CustomsValue * 3;
					break;
				case SEBCalculationList.Codes.TIB:
					result = AutoCalcEstimatedTIB(ensEntry, null);
					break;
				case SEBCalculationList.Codes.UFM:
					result = ensEntry.CustomsValue * 0.1m;
					break;
			}

			result = result.Round(2);
			result = result > minimumAmount ? result : minimumAmount;
			return result;
		}

		public TIBCalculations TIBCalculationsForEntrySummaryPrinting(CusEntryHeader ensEntry)
		{
			TIBCalculations tibCalcs = new TIBCalculations();
			AutoCalcEstimatedTIB(ensEntry, tibCalcs);
			return tibCalcs;
		}

		#region Implementation

		ZDecimal AutoCalcBondAmount(CusEntryHeader ensEntry)
		{
			ZDecimal result = 0;

			if (ensEntry.IsExWarehouseEntryType)  // for ex-warehouse - ADD & CVD only applicable in these cases - Joo has confirmed with testing
			{
				result = ((ensEntry.TotalDutyAmount + ensEntry.TotalAntidumpingDuty + ensEntry.TotalCountervailingDuty) * 2) + (ensEntry.TotalAmountForBondCalculationUse - ensEntry.TotalDutyAmount - ensEntry.TotalAntidumpingDuty - ensEntry.TotalCountervailingDuty);   //twice the duty amount plus all applicable taxes & fees
			}
			else if (ensEntry.EntryType == EntryTypeList.Codes.TemporaryImportationBond)
			{
				result = (ensEntry.TotalDutyAmount * 2) + (ensEntry.TotalAmountForBondCalculationUse - ensEntry.TotalDutyAmount);   //twice the duty amount plus all applicable taxes & fees that would be payable if not for a warehouse entry
			}
			else if (ensEntry.HasPGALinesRequireThreeTimesCustomsValue)
			{
				result = ensEntry.CustomsValue * 3;
			}
			else if (ensEntry.IsQuotaOrVisaEntryType)
			{
				result = (ensEntry.VisaOrQuotaLinesMerchandiseValue * 3) + ensEntry.NonVisaOrQuotaLinesValuePlusDutiesTaxesAndFees; //total entered value of merchandise which falls into the specified categories * 3, plus the total entered value and all duties, taxes and fees which apply for the remainder of the merchandise.
			}
			else if (ensEntry.EntryType == EntryTypeList.Codes.TradeFair)
			{
				return result;
			}
			else if (ensEntry.EntryType == EntryTypeList.Codes.PermanentExhibition)
			{
				result = ensEntry.TotalDutyAmount > 0m ? ensEntry.TotalDutyAmount : ensEntry.MPFAmountForEntry;
			}
			else
			{
				result = ensEntry.CustomsValue + ensEntry.TotalAmountForBondCalculationUse; //total customs value plus all applicable duties, taxes & fees
			}

			return result;
		}

		ZDecimal CalculateTotalFeesIfWereNotForTIBOrExhibition(CusEntryHeader ensEntry)
		{
			ZDecimal mpfAmount = 0;
			ZDecimal otherFees = 0m;

			MPFCalculator mpfCalculator = new MPFCalculator(ensEntry.Factory, true);
			FeeCalculator feeCalculator = new FeeCalculator(ensEntry.Factory, true);
			foreach (CusEntryLine line in ensEntry.MergedLines)
			{
				mpfAmount += mpfCalculator.CalculateFee(line).Amount;
				otherFees += feeCalculator.CalculateAllNonMPFAndHMFFeesForTIBBondCharge(line);
			}

			// adjust for Entry Minimum or Maximum MPF value
			var declaration = ensEntry.Declaration;
			var currentMPFRate = new FeeCalculationHelper(declaration.Factory, declaration.DateForMPFCalculation).GetCurrentRate(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing);
			if (currentMPFRate != null)
			{
				if (mpfAmount < currentMPFRate.ZZF_Minimum)
				{
					mpfAmount = currentMPFRate.ZZF_Minimum;
				}
				else if (mpfAmount > currentMPFRate.ZZF_Maximum)
				{
					mpfAmount = currentMPFRate.ZZF_Maximum;
				}
			}

			return mpfAmount.Round(2) + otherFees.Round(2);
		}

		ZString GetTotalFeesDescIfWereNotForTIBOrExhibition(CusEntryHeader ensEntry)
		{
			var result = ZString.Empty;
			ZDecimal mpfAmount = 0m;
			ZDecimal otherFees = 0m;
			MPFCalculator mpfCalculator = new MPFCalculator(ensEntry.Factory, true);
			FeeCalculator feeCalculator = new FeeCalculator(ensEntry.Factory, true);

			foreach (CusEntryLine line in ensEntry.MergedLines)
			{
				mpfAmount += mpfCalculator.CalculateFee(line).Amount;
				otherFees += feeCalculator.CalculateAllNonMPFAndHMFFeesForTIBBondCharge(line);
			}

			var mpfDesc = mpfAmount > 0 ? "MPF: " + mpfAmount.Round(2).ToString(2) : "";

			var otherDesc = ZString.Empty;
			Dictionary<ZString, ZDecimal> feesToPrint = new Dictionary<ZString, ZDecimal>();
			if (otherFees > 0)
			{
				foreach (CusEntryLine line in ensEntry.MergedLines)
				{
					feeCalculator.GetAllNonMPFAndHMFFeeDescForTIBBondCharge(line, feesToPrint);
				}
			}

			if (feesToPrint.Count > 0)
			{
				ZStringBuilder otherDescBuilder = new ZStringBuilder();
				foreach (KeyValuePair<ZString, ZDecimal> keyValuePair in feesToPrint)
				{
					if (otherDescBuilder.Length == 0)
					{
						otherDescBuilder.Append(keyValuePair.Key + ": " + keyValuePair.Value.ToString(2));
					}
					else
					{
						otherDescBuilder.Append("; " + keyValuePair.Key + ": " + keyValuePair.Value.ToString(2));
					}
				}

				otherDesc = otherDescBuilder.ToString();
			}

			// adjust for Entry Minimum or Maximum MPF value
			var declaration = ensEntry.Declaration;
			var currentMPFRate = new FeeCalculationHelper(declaration.Factory, declaration.DateForMPFCalculation).GetCurrentRate(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing);
			if (currentMPFRate != null)
			{
				if (mpfAmount < currentMPFRate.ZZF_Minimum)
				{
					mpfDesc = "MPF - Minimum: " + currentMPFRate.ZZF_Minimum.Round(2).ToString(2);
				}
				else if (mpfAmount > currentMPFRate.ZZF_Maximum)
				{
					mpfDesc = "MPF - Maximum: " + currentMPFRate.ZZF_Maximum.Round(2).ToString(2);
				}
			}

			result = mpfDesc;
			if (!otherDesc.IsEmpty)
			{
				result = result.IsEmpty ? otherDesc.ToString() : result + "; " + otherDesc;
			}

			if (!result.IsEmpty)
			{
				result = "(" + result + ")";
			}

			return result;
		}

		#region TemporaryImportBond

		public class TIBCalculations
		{
			public ZDecimal TIBADDAmount;
			public ZDecimal TIBCVDAmount;
			public ZDecimal TIBCharges;
			public ZString TIBChargesDesc;
			public ZDecimal TIBDuty;
			public ZDecimal TIBBondCharge;
		}

		ZDecimal AutoCalcEstimatedTIB(CusEntryHeader ensEntry, TIBCalculations print)
		{
			var totalFeesForEntry = CalculateTotalFeesIfWereNotForTIBOrExhibition(ensEntry);
			var feesDesc = GetTotalFeesDescIfWereNotForTIBOrExhibition(ensEntry);

			if (print != null)
			{
				print.TIBCharges = totalFeesForEntry;
				print.TIBChargesDesc = feesDesc;
			}

			///
			///	Estimated Bond for TIB is:
			///	a) equal to double the duties, including fees, which it is estimated would accrue
			///	b) for certain (motion picture goods) the bond required to be given shall be in an amount equal to 110 percent of the estimated duties, including fees, determined at the time of entry
			///	
			/// If any of the lines attract duty as in a) we will apply the doubling calculation for the whole entry (ie the higher of the values)
			/// If there is no duty just fees, the bond amount will only be 1 times the fee or $100, whichever is greater.
			/// 

			ZDecimal tibTotalDutyAndTaxesStandardTariffs = TIBTotalDutyAndTaxesStandardTariffsIfEntryHadBeenForConsumption(ensEntry, print);
			ZDecimal tibTotalDutyAndTaxesExceptionTariffs = TIBTotalDutyAndTaxesExceptionTariffsIfEntryHadBeenForConsumption(ensEntry, print);

			ZDecimal result = totalFeesForEntry * 2;
			if (tibTotalDutyAndTaxesStandardTariffs > ZDecimal.Zero)
			{
				result = (totalFeesForEntry + tibTotalDutyAndTaxesStandardTariffs + tibTotalDutyAndTaxesExceptionTariffs) * 2;
			}
			else if (tibTotalDutyAndTaxesExceptionTariffs > ZDecimal.Zero)
			{
				result = (totalFeesForEntry + tibTotalDutyAndTaxesExceptionTariffs) * 1.1m;
			}

			if (result < MinBondAmount)
			{
				result = MinBondAmount;
			}

			if (print != null)
			{
				print.TIBBondCharge = result;
			}

			return result;
		}

		ZDecimal GetTIBTotalDutyForCombineLines(CusEntryLine line, TIBCalculations print, Func<USCTariff, bool> shouldCalculate)
		{
			var result = ZDecimal.Zero;
			var dutyData = new EntryLineIEntryLineOrInvoiceLineDutyData(line);

			var chapter98TariffLine = dutyData.GetCombineLines().FirstOrDefault(x => Chapter98Helper.Is98Tariff(x.Tariff));
			var tariff = chapter98TariffLine.ImportTariff;
			if (tariff != null && shouldCalculate(tariff))
			{
				if (!dutyData.Tariff.IsEmpty && !Chapter98Helper.Is98Tariff(dutyData.Tariff))
				{
					var dutyAmount = ZDecimal.Zero;

					if (Chapter98Helper.Is99Tariff(dutyData.Tariff))
					{
						var normalTariffLine = dutyData.GetCombineLines().FirstOrDefault(x => x.IsNormalTariffLine());
						if (normalTariffLine != null)
						{
							var adjuestLine = new LineDutyData(dutyData, normalTariffLine.CustomsValue);
							var calculator = AppendixFDutyCalculator.NewWithCombinedCustomsValue(adjuestLine, null);
							dutyAmount = calculator.DutyResult.TotalAmount.Amount;
						}
					}
					else
					{
						var calculator = AppendixFDutyCalculator.NewWithCombinedCustomsValue(line, null);
						dutyAmount = calculator.DutyResult.TotalAmount.Amount;
					}

					result = dutyAmount;

					if (print != null)
					{
						print.TIBDuty += dutyAmount;
					}

					result += AddOtherDuties(line, print);
				}
			}
			return result;
		}

		protected ZDecimal TIBTotalDutyAndTaxesStandardTariffsIfEntryHadBeenForConsumption(CusEntryHeader entry, TIBCalculations print)
		{
			var result = ZDecimal.Zero;

			foreach (CusEntryLine line in entry.MergedLines)
			{
				MessageBuilders.ICusEntryLine entryLine = line;

				if (Chapter98Helper.HaveTIB9813TariffLine(new EntryLineIEntryLineOrInvoiceLineDutyData(line)))
				{
					result += GetTIBTotalDutyForCombineLines(line, print, (tariff) => !tariff.IsTIB110PercentTariff);
				}
				else if (line.ParentLine != null)
				{
					var tariff = line.ParentLine.ImportTariff;
					if (tariff != null && !tariff.IsTIB110PercentTariff)
					{
						if (!line.IsSetVLine)
						{
							result += line.ParentChildLineDuty;
							if (print != null)
							{
								print.TIBDuty += line.ParentChildLineDuty;
							}
						}

						result += AddOtherDuties(line, print);
					}
				}
				else
				{
					USCTariff tariff = line.ImportTariff;
					if (tariff != null && !tariff.IsTIB110PercentTariff)
					{
						if (!line.IsSetVLine)
						{
							result = result + line.DutyAmount;
							if (print != null)
							{
								print.TIBDuty += line.DutyAmount;
							}
						}

						result += AddOtherDuties(line, print);
					}
				}
			}

			return result;
		}

		protected ZDecimal TIBTotalDutyAndTaxesExceptionTariffsIfEntryHadBeenForConsumption(CusEntryHeader entry, TIBCalculations print)
		{
			ZDecimal result = ZDecimal.Zero;

			foreach (CusEntryLine line in entry.MergedLines)
			{
				if (Chapter98Helper.HaveTIB9813TariffLine(new EntryLineIEntryLineOrInvoiceLineDutyData(line)))
				{
					result += GetTIBTotalDutyForCombineLines(line, print, (tariff) => tariff.IsTIB110PercentTariff);
				}
				else if (line.ParentLine != null)
				{
					USCTariff tariff = line.ParentLine.ImportTariff;
					if (tariff != null && tariff.IsTIB110PercentTariff)
					{
						if (!line.IsSetVLine)
						{
							result += line.ParentChildLineDuty;
							if (print != null)
							{
								print.TIBDuty += line.ParentChildLineDuty;
							}
						}

						result += AddOtherDuties(line, print);
					}
				}
				else
				{
					USCTariff tariff = line.ImportTariff;
					if (tariff != null && tariff.IsTIB110PercentTariff)
					{
						if (!line.IsSetVLine)
						{
							result += line.DutyAmount;
							if (print != null)
							{
								print.TIBDuty += line.DutyAmount;
							}
						}

						result += AddOtherDuties(line, print);
					}
				}
			}

			return result;
		}

		ZDecimal AddOtherDuties(CusEntryLine line, TIBCalculations print)
		{
			ZDecimal result = ZDecimal.Zero;
			if (!line.RandomLine.US_ADDCaseNo.IsEmpty)
			{
				ZDecimal antiDumpingDuty = AddAnyADDThatWouldBeDueIfForConsumption(line);
				result += antiDumpingDuty;
				if (print != null)
				{
					print.TIBADDAmount += antiDumpingDuty;
				}
			}
			if (!line.RandomLine.US_CVDCaseNo.IsEmpty)
			{
				ZDecimal countervailingDuty = AddAnyCVDThatWouldBeDueIfForConsumption(line);
				result += countervailingDuty;
				if (print != null)
				{
					print.TIBCVDAmount += countervailingDuty;
				}
			}

			return result;
		}

		ZDecimal AddAnyADDThatWouldBeDueIfForConsumption(CusEntryLine line)
		{
			return new LineDutyFeeCalculator.ADD_CVDCalculator().CalculateADDDuty(line, new EntryLineIEntryLineOrInvoiceLineDutyData(line).GetADDAdjustValueForCombineLines(), out var addpercentOfValue);
		}

		ZDecimal AddAnyCVDThatWouldBeDueIfForConsumption(CusEntryLine line)
		{
			return new LineDutyFeeCalculator.ADD_CVDCalculator().CalculateCVDDuty(line, new EntryLineIEntryLineOrInvoiceLineDutyData(line).GetCVDAdjustValueForCombineLines(), out var cvdpercentOfValue);
		}

		#endregion

		const decimal MinBondAmount = 100;

		#endregion
	}
}
