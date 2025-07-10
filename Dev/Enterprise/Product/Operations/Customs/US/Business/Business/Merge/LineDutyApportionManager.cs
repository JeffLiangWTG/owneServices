using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class LineDutyApportionManager
	{
		public void Apportion(IDeclarationDutyDataProvider declaration)
		{
			ClearInvoiceLineAmounts(declaration);

			var entry = declaration.EntrySummaryEntry ?? declaration.FTZEntry;

			if (entry != null)
			{
				var totalCVs = GetTotalCV(entry);

				foreach (var entryLine in totalCVs.Keys)
				{
					ZDecimal totalCV;
					totalCVs.TryGetValue(entryLine, out totalCV);
					if (totalCV > 0 || entryLine.InvoiceLines.Length == 1)
					{
						ApportionDuty(entryLine, totalCV);

						var duty = entryLine as IDutyData;
						if (duty != null && (!duty.IsCombinedLine() || duty.IsNormalTariffLine() || CalculateDutyForSetsHelper.IsCombinedXLine(duty)))
						{
							ApportionFees(entryLine, totalCV);
						}
					}
				}
			}
		}

		#region Apportion Duty

		void ApportionDuty(IEntryLineDutyDataProvider entryLine, ZDecimal totalCV)
		{
			var dutyAmount = entryLine.DutyAmount;
			if (dutyAmount > 0m)
			{
				var dutyFieldName = JobComInvoiceLine.Schema.US_Duty;
				var dutyOverrideFieldName = JobComInvoiceLine.Schema.US_OverrideDuty;
				if (entryLine.US_SupLine)
				{
					if (entryLine.US_SupAdditionalLine)
					{
						dutyFieldName = JobComInvoiceLine.Schema.US_SupAdditionalTariff1Duty;
						dutyOverrideFieldName = JobComInvoiceLine.Schema.US_OverrideSupAdditionalTariff1Duty;
					}
					else if (entryLine.US_SupAdditionalLine2)
					{
						dutyFieldName = JobComInvoiceLine.Schema.US_SupAdditionalTariff2Duty;
						dutyOverrideFieldName = JobComInvoiceLine.Schema.US_OverrideSupAdditionalTariff2Duty;
					}
					else if (entryLine.US_SupAdditionalLine3)
					{
						dutyFieldName = JobComInvoiceLine.Schema.US_SupAdditionalTariff3Duty;
						dutyOverrideFieldName = JobComInvoiceLine.Schema.US_OverrideSupAdditionalTariff3Duty;
					}
					else if (entryLine.US_SupAdditionalLine4)
					{
						dutyFieldName = JobComInvoiceLine.Schema.US_SupAdditionalTariff4Duty;
						dutyOverrideFieldName = JobComInvoiceLine.Schema.US_OverrideSupAdditionalTariff4Duty;
					}
					else if (entryLine.US_SupAdditionalLine5)
					{
						dutyFieldName = JobComInvoiceLine.Schema.US_SupAdditionalTariff5Duty;
						dutyOverrideFieldName = JobComInvoiceLine.Schema.US_OverrideSupAdditionalTariff5Duty;
					}
					else
					{
						dutyFieldName = JobComInvoiceLine.Schema.US_SupDuty;
						dutyOverrideFieldName = JobComInvoiceLine.Schema.US_OverrideSupDuty;
					}
				}

				var invoiceLineWithHighestCV = ApportionDutyToInvoiceLines(entryLine, totalCV, dutyAmount, dutyFieldName,
					invoiceLine => !((ZBool)invoiceLine[dutyOverrideFieldName] || (dutyFieldName == JobComInvoiceLine.Schema.US_SupDuty && invoiceLine.IsQuotaProductExclusion)));
				BackRoundDuty(entryLine, invoiceLineWithHighestCV, dutyAmount, dutyFieldName);
			}

			var addDutyAmount = entryLine.AntidumpingDuty;
			if (addDutyAmount > 0m)
			{
				var invoiceLineWithHighestCV = ApportionDutyToInvoiceLines(entryLine, totalCV, addDutyAmount, JobComInvoiceLine.Schema.US_ADDuty, invoiceLine => !invoiceLine.IsADDManual && !invoiceLine.HasSupTariffOnly);
				BackRoundDuty(entryLine, invoiceLineWithHighestCV, addDutyAmount, JobComInvoiceLine.Schema.US_ADDuty);
			}

			//CV duty
			var cvdDutyAmount = entryLine.CountervailingDuty;
			if (entryLine.CountervailingDuty > 0m)
			{
				var invoiceLineWithHighestCV = ApportionDutyToInvoiceLines(entryLine, totalCV, cvdDutyAmount, JobComInvoiceLine.Schema.US_CVDuty, invoiceLine => !invoiceLine.IsCVDManual && !invoiceLine.HasSupTariffOnly);
				BackRoundDuty(entryLine, invoiceLineWithHighestCV, cvdDutyAmount, JobComInvoiceLine.Schema.US_CVDuty);
			}
		}

		IInvoiceLineDutyDataProvider ApportionDutyToInvoiceLines(IEntryLineDutyDataProvider entryLine, ZDecimal totalCV, ZDecimal dutyAmount, string dutyFieldName, Func<IInvoiceLineDutyDataProvider, bool> isDutyApportionable)
		{
			IInvoiceLineDutyDataProvider lineWithHighestCV = null;

			foreach (IInvoiceLineDutyDataProvider invoiceLine in entryLine.InvoiceLines)
			{
				if (isDutyApportionable(invoiceLine))
				{
					if (lineWithHighestCV == null || lineWithHighestCV.JI_CustomsValue < invoiceLine.JI_CustomsValue)
					{
						lineWithHighestCV = invoiceLine;
					}

					var ratio = totalCV > 0 ? invoiceLine.JI_CustomsValue / totalCV : entryLine.InvoiceLines.Length == 1 ? 1 : 0;
					var apportionedDuty = new ZDecimal(dutyAmount * ratio).Round(2);
					invoiceLine[dutyFieldName] = new ZDecimal(((ZDecimal)invoiceLine[dutyFieldName]) + apportionedDuty);
				}
			}
			return lineWithHighestCV;
		}

		void BackRoundDuty(IEntryLineDutyDataProvider entryLine, IInvoiceLineDutyDataProvider lineWithHighestCV, ZDecimal dutyAmount, string dutyFieldName)
		{
			if (dutyAmount > 0m && lineWithHighestCV != null)
			{
				var totalApportionedDuty = ZDecimal.Zero;

				foreach (IInvoiceLineDutyDataProvider invoiceLine in entryLine.InvoiceLines)
				{
					totalApportionedDuty += (ZDecimal)invoiceLine[dutyFieldName];
				}

				var difference = dutyAmount - totalApportionedDuty;
				if (difference != 0)
				{
					lineWithHighestCV[dutyFieldName] = new ZDecimal((ZDecimal)lineWithHighestCV[dutyFieldName] + difference);
				}
			}
		}

		#endregion

		#region Apportion Fees

		void ApportionFees(IEntryLineDutyDataProvider entryLine, ZDecimal totalCV)
		{
			var feeCodelist = CusFeeCodeConstants.GetAccountingClassFeeCodeList(entryLine.Factory);
			foreach (IFee entryLineFee in entryLine.Fees)
			{
				if (feeCodelist.ContainsCode(entryLineFee.Code) && entryLineFee.Amount > ZDecimal.Zero)
				{
					var lineWithHighestCV = ApportionFeesToInvoiceLines(entryLine, totalCV, entryLineFee.Amount, entryLineFee.Code);
					BackRoundFees(entryLine, lineWithHighestCV, entryLineFee.Amount, entryLineFee.Code);
				}
			}
		}

		IInvoiceLineDutyDataProvider ApportionFeesToInvoiceLines(IEntryLineDutyDataProvider entryLine, ZDecimal totalCV, ZDecimal feeAmount, ZString feeCode)
		{
			IInvoiceLineDutyDataProvider lineWithHighestCV = null;

			foreach (var invoiceLine in entryLine.InvoiceLines)
			{
				var feeCodeData = invoiceLine.FeeCusCodes.GetFeeFor(feeCode);

				if (feeCodeData == null || !feeCodeData.IsOverridden)
				{
					if (lineWithHighestCV == null || lineWithHighestCV.JI_CustomsValue < invoiceLine.JI_CustomsValue)
					{
						lineWithHighestCV = invoiceLine;
					}

					var ratio = totalCV > 0 ? invoiceLine.JI_CustomsValue / totalCV : entryLine.InvoiceLines.Length == 1 ? 1 : 0;
					var apportionedAmount = new ZDecimal(feeAmount * ratio).Round(2);

					if (apportionedAmount > ZDecimal.Zero)
					{
						var existingAmount = invoiceLine.FeeCusCodes.GetFeeOrChargeAmount(feeCode);
						invoiceLine.FeeCusCodes.UpdateOrAddCharge(feeCode, apportionedAmount + existingAmount);
					}
				}
			}
			return lineWithHighestCV;
		}

		void BackRoundFees(IEntryLineDutyDataProvider entryLine, IInvoiceLineDutyDataProvider lineWithHighestCV, ZDecimal feeAmount, ZString feeCode)
		{
			if (feeAmount > ZDecimal.Zero && lineWithHighestCV != null)
			{
				var totalApportionedAmounts = ZDecimal.Zero;

				foreach (var invoiceLine in entryLine.InvoiceLines)
				{
					totalApportionedAmounts += invoiceLine.FeeCusCodes.GetFeeOrChargeAmount(feeCode);
				}

				var difference = feeAmount - totalApportionedAmounts;
				if (difference != 0)
				{
					var existingAmount = lineWithHighestCV.FeeCusCodes.GetFeeOrChargeAmount(feeCode);
					if (existingAmount + difference > 0)
					{
						lineWithHighestCV.FeeCusCodes.UpdateOrAddCharge(feeCode, existingAmount + difference);
					}
					else
					{
						SpreadRoundingDifferenceBetweenLines(entryLine, difference, feeCode);
					}
				}
			}
		}

		void SpreadRoundingDifferenceBetweenLines(IEntryLineDutyDataProvider entryLine, ZDecimal difference, ZString feeCode)
		{
			var linesSortedByCustomsValue = entryLine.InvoiceLines.ToList();
			linesSortedByCustomsValue.Sort(new Comparer());
			var isNegativeDiff = difference < ZDecimal.Zero;

			var diffFraction = new ZDecimal(difference / entryLine.InvoiceLines.Length);
			var roundUp = isNegativeDiff ? -0.01m : 0.01m;
			diffFraction = diffFraction.DecimalPlaces == 2 ? diffFraction : new ZDecimal(diffFraction.Truncate(2) + roundUp);
			var isRemainingDiffNotValid = isNegativeDiff ? new Func<ZDecimal, bool>((d) => d >= 0) : new Func<ZDecimal, bool>((d) => d <= 0);

			var remainingDiff = difference;

			foreach (var invoiceLine in linesSortedByCustomsValue)
			{
				var existingAmount = invoiceLine.FeeCusCodes.GetFeeOrChargeAmount(feeCode);
				invoiceLine.FeeCusCodes.UpdateOrAddCharge(feeCode, existingAmount + diffFraction);

				remainingDiff -= diffFraction;
				if (isRemainingDiffNotValid(remainingDiff))
				{
					return;
				}
			}
		}

		class Comparer : IComparer<IInvoiceLineDutyDataProvider>
		{
			public int Compare(IInvoiceLineDutyDataProvider x, IInvoiceLineDutyDataProvider y) => y.JI_CustomsValue.CompareTo(x.JI_CustomsValue);
		}

		#endregion

		#region Customs Value Totalling

		void ClearInvoiceLineAmounts(IDeclarationDutyDataProvider declaration)
		{
			foreach (IInvoiceLineDutyDataProvider invoiceLine in declaration.InvoiceLines)
			{
				if (!invoiceLine.US_OverrideDuty)
				{
					invoiceLine.US_Duty = ZDecimal.Zero;
				}

				if (!invoiceLine.US_OverrideSupDuty)
				{
					invoiceLine.US_SupDuty = ZDecimal.Zero;
				}

				if (!invoiceLine.US_OverrideSupAdditionalTariff1Duty)
				{
					invoiceLine.US_SupAdditionalTariff1Duty = ZDecimal.Zero;
				}

				if (!invoiceLine.US_OverrideSupAdditionalTariff2Duty)
				{
					invoiceLine.US_SupAdditionalTariff2Duty = ZDecimal.Zero;
				}

				if (!invoiceLine.US_OverrideSupAdditionalTariff3Duty)
				{
					invoiceLine.US_SupAdditionalTariff3Duty = ZDecimal.Zero;
				}

				if (!invoiceLine.US_OverrideSupAdditionalTariff4Duty)
				{
					invoiceLine.US_SupAdditionalTariff4Duty = ZDecimal.Zero;
				}

				if (!invoiceLine.US_OverrideSupAdditionalTariff5Duty)
				{
					invoiceLine.US_SupAdditionalTariff5Duty = ZDecimal.Zero;
				}

				if (!invoiceLine.IsADDManual)
				{
					invoiceLine.US_ADDuty = ZDecimal.Zero;
				}

				if (!invoiceLine.IsCVDManual)
				{
					invoiceLine.US_CVDuty = ZDecimal.Zero;
				}

				foreach (IFee fee in invoiceLine.FeeCusCodes)
				{
					if (!fee.IsOverridden)
					{
						fee.Amount = ZDecimal.Zero;
					}
				}
			}
		}

		Dictionary<IEntryLineDutyDataProvider, ZDecimal> GetTotalCV(IEntryHeaderDutyDataProvider entry)
		{
			var result = new Dictionary<IEntryLineDutyDataProvider, ZDecimal>();

			foreach (IEntryLineDutyDataProvider entryLine in entry.EntryLines)
			{
				result.Add(entryLine, GetTotalCustomsValueFromInvoiceLines(entryLine));
			}

			return result;
		}

		ZDecimal GetTotalCustomsValueFromInvoiceLines(IEntryLineDutyDataProvider entryLine)
		{
			ZDecimal result = 0m;
			foreach (IInvoiceLineDutyDataProvider invoiceLine in entryLine.InvoiceLines)
			{
				result += invoiceLine.JI_CustomsValue;
			}
			return result;
		}

		#endregion
	}
}
