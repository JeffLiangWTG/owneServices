using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.US.Business.DDPDisbursementCalculation;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business
{
	/// <summary>
	/// For DDP invoices, it calculates Disbursement cost including duty & fees and adds it as a line level charge
	/// </summary>
	class DDPDisbursementChargeCalculator
	{
		public DDPDisbursementChargeCalculator(JobDeclaration declaration)
		{
			this.declaration = declaration;
			dutyFeeCalculationResult = new Dictionary<JobComInvoiceLine, Dictionary<string, DDPCalculationResultData>>(declaration.InvoiceLines.Count * 2);
			customsValues = new Dictionary<JobComInvoiceLine, CustomsValues>(declaration.InvoiceLines.Count);
		}

		readonly JobDeclaration declaration;

		//instead of writing duty & fee calculation result back to JobComInvoiceLine, the calculation result is written to this dictionary
		readonly Dictionary<JobComInvoiceLine, Dictionary<string, DDPCalculationResultData>> dutyFeeCalculationResult;
		readonly Dictionary<JobComInvoiceLine, CustomsValues> customsValues;

		public void Calculate()
		{
			if (declaration.IsFormalImport || declaration.IsFTZAdmission)
			{
				dutyFeeCalculationResult.Clear();
				customsValues.Clear();

				using (declaration.SuspendMarkApportionmentDirty())
				{
					bool hasInvoicesDutyPaid = false;

					foreach (JobComInvoiceHeader invoice in declaration.Invoices)
					{
						ClearExistingSystemGeneratedDisbursementCharge(invoice);

						if (DDPCalculationHelper.IsDisbursementChargeToBeAutoCalculated(invoice))
						{
							hasInvoicesDutyPaid = true;
						}
					}

					foreach (JobComInvoiceLine invoiceLine in declaration.InvoiceLines)
					{
						customsValues[invoiceLine] = DDPCustomsValuesExtensionMethods.GetCustomsValues(invoiceLine);
					}

					if (hasInvoicesDutyPaid && declaration.IsFormalImport)
					{
						CalculateEntryLevelFeesAndApportionToEachLine();

						ZDecimal totalCustomsValueSubjectToMPF = CalculateDutyFeeAndGetTotalCustomsValueSubjectToMPFForDDPInvoice();
						ApportionMinimumMaximumMPFIfRequired(totalCustomsValueSubjectToMPF);
					}

					CalculateForInvoices();

					if (hasInvoicesDutyPaid)
					{
						RefreshCusEntryLineCustomsValue(declaration);
					}
				}
			}
		}

		public void ReCalculateDDD()
		{
			foreach (JobComInvoiceHeader invoice in declaration.Invoices)
			{
				if (DDPCalculationHelper.IsDisbursementChargeToBeAutoCalculated(invoice))
				{
					ClearExistingSystemGeneratedDisbursementCharge(invoice);
					List<JobComInvoiceLine> lines = new List<JobComInvoiceLine>(new TypedEnumerable<JobComInvoiceLine>(invoice.JobComInvoiceLines));
					lines.Sort(new InvoiceLineComparer());

					JobComInvCharge[] userEnteredDDDCharges = invoice.Charges.GetCharge(USCustomsChargeTypeList.Codes.DisbursementCharge);
					var totalDisbursementAmount = ApportionParentLineDDD(lines, userEnteredDDDCharges);

					if (totalDisbursementAmount > 0m)
					{
						SetDisbursementCharge(invoice, totalDisbursementAmount, userEnteredDDDCharges);
					}
					DeleteEmptySystemGeneratedDisbursementCharge(invoice);
				}
			}
			RemergedEntryTotals();
		}

		ZDecimal ApportionParentLineDDD(List<JobComInvoiceLine> invoiceLines, JobComInvCharge[] userEnteredDDDCharges)
		{
			var totalValueForInvoice = ZDecimal.Zero;
			foreach (JobComInvoiceLine invoiceLine in invoiceLines)
			{
				if (!CalculatedWhenParentIsCalculated(invoiceLine) && !DDPCalculationHelper.HasUserEnteredDisbursementCharge(invoiceLine))
				{
					var result = ZDecimal.Zero;
					result = GetDisbursementChargeAmountWithoutCalculation(invoiceLine);
					IEnumerable<JobComInvoiceLine> secondaryTariffLines = null;
					var hasSetDDD = false;
					if (ShouldCalculateSecondaryWhenParentIsCalculated(invoiceLine))
					{
						secondaryTariffLines = invoiceLine.SecondaryTariffLines;
						if (secondaryTariffLines != null && secondaryTariffLines.Any())
						{
							foreach (JobComInvoiceLine secondaryLine in secondaryTariffLines)
							{
								result += GetDDDAmount(secondaryLine);
							}
							var allInvoiceLines = new List<JobComInvoiceLine>();
							allInvoiceLines.AddRange(secondaryTariffLines);
							allInvoiceLines.Add(invoiceLine);
							if (!ApportionDisbursementChargeIfRequiredWithoutCalculation(allInvoiceLines, result))
							{
								foreach (var line in allInvoiceLines)
								{
									SetDisbursementCharge(line, GetDDDAmount(line), userEnteredDDDCharges);
								}
							}
							hasSetDDD = true;
						}
					}
					if (!hasSetDDD)
					{
						SetDisbursementCharge(invoiceLine, result, userEnteredDDDCharges);
					}
					totalValueForInvoice += result;
				}
			}
			return totalValueForInvoice;
		}

		void RemergedEntryTotals()
		{
			var hasInvoicesDutyPaid = declaration.Invoices.OfType<JobComInvoiceHeader>().Any(invoice => DDPCalculationHelper.IsDisbursementChargeToBeAutoCalculated(invoice));
			if (hasInvoicesDutyPaid)
			{
				foreach (CusEntryHeader entry in declaration.ActiveEntryHeaders)
				{
					entry.CH_TotalPaid = 0;
					entry.ResetIsCustomsValueCalculated();

					foreach (CusEntryLine entryLine in entry.AllEntryLines)
					{
						if (DDPCalculationHelper.IsDisbursementChargeToBeAutoCalculated(entryLine.RandomLine.InvoiceHeader))
						{
							entryLine.CL_CustomsValue = 0;

							foreach (JobComInvoiceLine invoiceLine in entryLine.InvoiceLines)
							{
								entryLine.MergeInvoiceLine(invoiceLine);
							}
						}
					}
				}
			}
		}

		void CalculateEntryLevelFeesAndApportionToEachLine()
		{
			var formalEntry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			if (formalEntry != null)
			{
				var entryFees = new Dictionary<string, decimal>();
				formalEntry.CalculateAndStoreEntryLevelFees((feeType, feeAmount) => entryFees[feeType] = feeAmount);

				if (entryFees.Count > 0)
				{
					JobComInvoiceLine invoiceLineWithMostCV = null;
					var totalCV = GetTotalCustomsValueAndLineWithHighestCustomsValue(out invoiceLineWithMostCV);

					if (totalCV > 0)
					{
						var apportionedEntryFees = ApportionEntryLevelFeesToInvoiceLines(entryFees, totalCV);

						if (invoiceLineWithMostCV != null)
						{
							foreach (string entryFeeType in entryFees.Keys)
							{
								if (entryFees[entryFeeType] != apportionedEntryFees[entryFeeType])
								{
									var apportioned = dutyFeeCalculationResult.GetChargeOrFeeAmount(invoiceLineWithMostCV, entryFeeType);

									var amount = apportioned + (entryFees[entryFeeType] - apportionedEntryFees[entryFeeType]);

									dutyFeeCalculationResult.SetDutyFeeCalculationResult(invoiceLineWithMostCV, entryFeeType, new DDPCalculationResultData(amount, new FeeCalculationInternalData(amount, 0m)));
								}
							}

							AdjustCustomsValue(new JobComInvoiceLine[] { invoiceLineWithMostCV });
						}
					}
				}
			}
		}

		Dictionary<string, decimal> ApportionEntryLevelFeesToInvoiceLines(Dictionary<string, decimal> entryFees, ZDecimal totalCV)
		{
			var apportionedEntryFees = new Dictionary<string, decimal>();
			foreach (JobComInvoiceLine invoiceLine in declaration.InvoiceLines)
			{
				var customsValue = customsValues.GetTotalCustomsValue(invoiceLine);
				foreach (string entryFeeType in entryFees.Keys)
				{
					var entryAmount = entryFees[entryFeeType];
					var apportionedAmount = new ZDecimal(entryAmount * customsValue / totalCV).Round(2);
					dutyFeeCalculationResult.SetDutyFeeCalculationResult(invoiceLine, entryFeeType, new DDPCalculationResultData(apportionedAmount, new FeeCalculationInternalData(apportionedAmount, 0m)));

					decimal existingAmount;
					apportionedEntryFees.TryGetValue(entryFeeType, out existingAmount);
					apportionedEntryFees[entryFeeType] = (existingAmount + apportionedAmount);
				}

				AdjustCustomsValue(new JobComInvoiceLine[] { invoiceLine });
			}
			return apportionedEntryFees;
		}

		ZDecimal GetTotalCustomsValueAndLineWithHighestCustomsValue(out JobComInvoiceLine invoiceLineWithMostCV)
		{
			var totalCV = ZDecimal.Zero;
			invoiceLineWithMostCV = null;
			foreach (JobComInvoiceLine invoiceLine in declaration.InvoiceLines)
			{
				var customsValue = customsValues.GetTotalCustomsValue(invoiceLine);
				totalCV += customsValue;

				if (invoiceLineWithMostCV == null || customsValues.GetTotalCustomsValue(invoiceLineWithMostCV) < customsValue)
				{
					invoiceLineWithMostCV = invoiceLine;
				}
			}

			return totalCV;
		}

		void CalculateForInvoices()
		{
			bool hasMinMaxMPFBeenApportioned = dutyFeeCalculationResult.GetTotalAmounts(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing) > 0m;

			foreach (JobComInvoiceHeader invoice in declaration.Invoices)
			{
				ClearExistingSystemGeneratedDisbursementCharge(invoice);

				if (DDPCalculationHelper.IsDisbursementChargeToBeAutoCalculated(invoice))
				{
					List<JobComInvoiceLine> lines = new List<JobComInvoiceLine>(new TypedEnumerable<JobComInvoiceLine>(invoice.JobComInvoiceLines));
					lines.Sort(new InvoiceLineComparer());

					JobComInvCharge[] userEnteredDDDCharges = invoice.Charges.GetCharge(USCustomsChargeTypeList.Codes.DisbursementCharge);
					ZDecimal totalDisbursementAmount = 0m;

					foreach (JobComInvoiceLine invoiceLine in lines)
					{
						if (!DDPCalculationHelper.HasUserEnteredDisbursementCharge(invoiceLine))
						{
							if (!CalculatedWhenParentIsCalculated(invoiceLine))
							{
								var isCombinedLine = invoiceLine.IsCombinedLine();
								var shouldCalculateMPF = !hasMinMaxMPFBeenApportioned && (!isCombinedLine || ShouldHaveMPFFeeOnInvoiceLine(invoiceLine));
								CalculateDutyAndFee(invoiceLine, shouldCalculateMPF, false);
							}

							ZDecimal disbursementCharge = GetDisbursementChargeAmount(invoiceLine);
							disbursementCharge = disbursementCharge.Round(2);

							SetDisbursementCharge(invoiceLine, disbursementCharge, userEnteredDDDCharges);

							totalDisbursementAmount += disbursementCharge;
						}
					}

					if (totalDisbursementAmount > 0m)
					{
						SetDisbursementCharge(invoice, totalDisbursementAmount, userEnteredDDDCharges);
					}
				}

				DeleteEmptySystemGeneratedDisbursementCharge(invoice);
			}
		}

		ZDecimal GetDisbursementChargeAmount(JobComInvoiceLine invoiceLine)
		{
			var result = ZDecimal.Zero;
			if (invoiceLine.IsCombinedLine())
			{
				if (ShouldHaveDDDChargeInCombinedLinesSet(invoiceLine)) //In combined lines the 'DDD' charge should be on normal tariff line
				{
					var combindeLines = invoiceLine.GetCombinedLines();
					result = combindeLines.Sum(x => dutyFeeCalculationResult.GetTotalAmountsInCombinedLines(x));
				}
			}
			else
			{
				result = dutyFeeCalculationResult.GetTotalAmounts(invoiceLine);
			}
			return result;
		}

		ZDecimal GetDisbursementChargeAmountWithoutCalculation(JobComInvoiceLine invoiceLine)
		{
			var result = ZDecimal.Zero;
			if (invoiceLine.IsCombinedLine())
			{
				if (ShouldHaveDDDChargeInCombinedLinesSet(invoiceLine)) //In combined lines the 'DDD' charge should be on normal tariff line
				{
					var combindeLines = invoiceLine.GetCombinedLines();
					result = combindeLines.Sum(x => GetDDDAmount(x));
				}
			}
			else
			{
				result = GetDDDAmount(invoiceLine);
			}
			return result;
		}

		ZDecimal GetDDDAmount(JobComInvoiceLine invoiceLine)
		{
			ZDecimal result = invoiceLine.JI_Calc_DutyAmount;
			foreach (FeeCusCodeData fee in invoiceLine.FeeCusCodes)
			{
				if (fee.CY_Code == Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing)
				{
					result += invoiceLine.US_PayableMPF;
				}
				else
				{
					result += fee.CY_FeeAmount;
				}
			}
			if (invoiceLine.FeeCusCodes.Count == 0)
			{
				result += invoiceLine.US_PayableMPF;
			}
			if (invoiceLine.IsADDManual || invoiceLine.InvoiceHeader.US_DeductADDCVDDuty == YesNoDefaultList.Codes.Yes)
			{
				result += invoiceLine.US_ADDuty;
			}
			if (invoiceLine.IsCVDManual || invoiceLine.InvoiceHeader.US_DeductADDCVDDuty == YesNoDefaultList.Codes.Yes)
			{
				result += invoiceLine.US_CVDuty;
			}
			return result;
		}

		bool ShouldHaveDDDChargeInCombinedLinesSet(JobComInvoiceLine invoiceLine)
		{
			return invoiceLine.IsNormalTariffLine() && !invoiceLine.IsParentLine;
		}

		void ClearExistingSystemGeneratedDisbursementCharge(JobComInvoiceHeader invoice)
		{
			ClearExistingSystemGeneratedDisbursementChargeCore(invoice);

			foreach (JobComInvoiceLine invoiceLine in invoice.JobComInvoiceLines)
			{
				ClearExistingSystemGeneratedDisbursementChargeCore(invoiceLine);
			}
		}

		void ClearExistingSystemGeneratedDisbursementChargeCore(IChargeApportionee apportionee)
		{
			foreach (BaseJobComInvHeaderCharge apportionedCharge in apportionee.ApportionedCharges)
			{
				if (apportionedCharge.J7_ChargeType == USCustomsChargeTypeList.Codes.DisbursementCharge && apportionedCharge.J7_IsSystem)
				{
					apportionedCharge.J7_Amount = 0m;
					apportionedCharge.J7_RX_NKCurrency = ZString.Empty;
				}
			}
		}

		void DeleteEmptySystemGeneratedDisbursementCharge(JobComInvoiceHeader invoice)
		{
			DeleteEmptySystemGeneratedDisbursementChargeCore(invoice);

			foreach (JobComInvoiceLine invoiceLine in invoice.JobComInvoiceLines)
			{
				DeleteEmptySystemGeneratedDisbursementChargeCore(invoiceLine);
			}
		}

		void DeleteEmptySystemGeneratedDisbursementChargeCore(IChargeApportionee apportionee)
		{
			foreach (BaseJobComInvHeaderCharge apportionedCharge in apportionee.ApportionedCharges.ToArray())
			{
				if (apportionedCharge.IsEmpty && apportionedCharge.J7_ChargeType == USCustomsChargeTypeList.Codes.DisbursementCharge && apportionedCharge.J7_IsSystem)
				{
					apportionedCharge.Delete();
				}
			}
		}

		ZDecimal CalculateDutyFeeAndGetTotalCustomsValueSubjectToMPFForDDPInvoice()
		{
			ZDecimal result = 0m;

			foreach (JobComInvoiceHeader invoice in declaration.Invoices)
			{
				List<JobComInvoiceLine> lines = new List<JobComInvoiceLine>(new TypedEnumerable<JobComInvoiceLine>(invoice.JobComInvoiceLines));
				lines.Sort(new InvoiceLineComparer());

				foreach (JobComInvoiceLine invoiceLine in lines)
				{
					if (DDPCalculationHelper.IsDisbursementChargeToBeAutoCalculated(invoice))
					{
						if (!CalculatedWhenParentIsCalculated(invoiceLine))
						{
							//Do not calculate MPF at this stage. Needs to determine whether it is going to be adjusted according to min/max limits first.
							CalculateDutyAndFee(invoiceLine, false, false);
						}
					}

					if (InvoiceLineSubjectForMPF(invoiceLine))
					{
						// no MPF is payable on 98 Goods value
						result += customsValues.GetTotalCustomsValue(invoiceLine) - invoiceLine.TotalOriginalGoodsValueInUSD;
					}
				}
			}

			return result;
		}

		bool CalculatedWhenParentIsCalculated(JobComInvoiceLine invoiceLine)
		{
			bool result = false;

			var isCombinedLine = invoiceLine.IsCombinedLine();
			if (isCombinedLine)
			{
				result = !invoiceLine.IsNormalTariffLine(); // In Combined lines, for one sets we only calaulate once, the normal line is key.
			}
			else if (invoiceLine.IsSecondaryTariffLine)
			{
				IDutyData parentDutyData = ((IDutyData)invoiceLine).ParentTariffLine;
				result = parentDutyData != null && IsRepairOrDerivedCalculation(parentDutyData);
			}

			return result;
		}

		bool ShouldCalculateSecondaryWhenParentIsCalculated(JobComInvoiceLine invoiceLine)
		{
			var lineDutyData = (IDutyData)invoiceLine;
			var parentLineDutyData = lineDutyData.ParentTariffLine;
			return ShouldCalculateSecondaryWhenParentIsCalculated(lineDutyData) || ShouldCalculateSecondaryWhenParentIsCalculated(parentLineDutyData);
		}

		bool ShouldCalculateSecondaryWhenParentIsCalculated(IDutyData dutyData)
		{
			return dutyData != null && (IsRepairOrDerivedCalculation(dutyData) || IsInLieuTariff(dutyData)) && !dutyData.IsCombinedLine();
		}

		bool IsRepairOrDerivedCalculation(IDutyData invoiceLine)
		{
			return IsRepairOrAssemblyLine(invoiceLine) ||
				invoiceLine != null && invoiceLine.ImportTariff != null && invoiceLine.ImportTariff.UE_DutyComputationCode == ComputationCodeList.Codes.Derived;
		}

		void CalculateDutyAndFee(JobComInvoiceLine invoiceLine, bool shouldCalculateMPF, bool useOriginalCustomsValue)
		{
			try
			{
				FeeCalculator feeCalculator = new FeeCalculator(invoiceLine.Factory, declaration.IsHMFApplicable, SetFeeResult);
				ZDecimal previousAmountToDeduct = 0m;
				ZDecimal amountToDeduct = CalculateDisbursementCharge(invoiceLine, feeCalculator, shouldCalculateMPF);

				var hasRateGreaterThanOne = dutyFeeCalculationResult.HasRateGreaterThanOne(invoiceLine);
				if (hasRateGreaterThanOne)
				{
					CalculateWhenRateGreaterThanOne(invoiceLine, useOriginalCustomsValue);
				}
				else
				{
					ZDecimal disbursementAfterDeduction = amountToDeduct;
					do
					{
						previousAmountToDeduct = amountToDeduct;

						amountToDeduct = disbursementAfterDeduction;

						disbursementAfterDeduction = CalculateDisbursementCharge(invoiceLine, feeCalculator, shouldCalculateMPF);
					}
					while (previousAmountToDeduct != disbursementAfterDeduction || disbursementAfterDeduction - amountToDeduct > 0.01m);
				}
			}
			catch (CustomsMergeException cex)
			{
				invoiceLine.TariffCalculateExceptionMessage = cex.Message;
			}
		}

		void CalculateWhenRateGreaterThanOne(JobComInvoiceLine invoiceLine, bool useOriginalCustomsValue)
		{
			if (customsValues.TryGetValue(invoiceLine, out var mappingCustomValue) && dutyFeeCalculationResult.TryGetValue(invoiceLine, out var dutyAndCharge))
			{
				var customsValueData = useOriginalCustomsValue ? DDPCustomsValuesExtensionMethods.GetCustomsValues(invoiceLine) : mappingCustomValue;
				var customsValueUsedToCal = invoiceLine.HasEmptySupTariff ? customsValueData.CustomsValue : customsValueData.SupCustomsValue;

				var resultDatas = dutyAndCharge.Values.Where(x => x != null);
				var rate = resultDatas.Sum(x => x.InternalData.PercentOfRate / 100);
				var noneRateFeeAmount = resultDatas.Sum(x => x.InternalData.NoneCustomsValueAmount);

				var diffCustomValue = customsValueUsedToCal - noneRateFeeAmount;
				var unitRateFeeAmount = diffCustomValue / (1m + rate);

				foreach (var ddpData in dutyAndCharge.Values)
				{
					var internalData = ddpData.InternalData;
					if (internalData.PercentOfRate != 0m)
					{
						var percentOfRate = internalData.PercentOfRate / 100;
						ddpData.Amount = (percentOfRate * unitRateFeeAmount) + internalData.NoneCustomsValueAmount;
						internalData.PercentOfRate = 0m;
					}
				}

				var newCusomsValue = new CustomsValues();
				if (invoiceLine.HasEmptySupTariff)
				{
					newCusomsValue.CustomsValue = unitRateFeeAmount * 1;
				}
				else
				{
					newCusomsValue.SupCustomsValue = unitRateFeeAmount * 1;
				}
				customsValues[invoiceLine] = newCusomsValue;
			}
		}

		void SetFeeResult(FeeResult feeResult, string feeCode, IFeeCalculationDataProvider invoiceLine)
		{
			if (!CusFeeCodeConstants.IsHeaderLevelFee(feeCode))
			{
				ZDecimal amount = feeResult.IsRequired ? feeResult.Amount : ZDecimal.Zero;

				invoiceLine.SetFeeResult(feeCode, amount, new FeeCalculationInternalData(feeResult.NoneCustomsValueAmount, feeResult.PercentOfRate));
			}
		}

		/// <summary>
		/// Calculate Duty and Fee including secondary lines if required and return the sum
		/// </summary>
		ZDecimal CalculateDisbursementCharge(JobComInvoiceLine invoiceLine, FeeCalculator feeCalculator, bool shouldCalculateMPF)
		{
			var result = ZDecimal.Zero;
			var isCombinedLine = invoiceLine.IsCombinedLine();
			var dutyCalculator = invoiceLine.Declaration.GetLineCalculator();

			if (isCombinedLine)
			{
				ClearDutyFeeCalculationResult(invoiceLine, shouldCalculateMPF, isCombinedLine);
				SetOverriddenDutiesAndFees(invoiceLine);

				var dutyDataList = invoiceLine.GetCombinedDutyDataListForDDPCalculation(dutyFeeCalculationResult, customsValues, shouldCalculateMPF);
				foreach (var dutyData in dutyDataList)
				{
					dutyCalculator.Calculate(dutyData, feeCalculator);
				}

				result = dutyFeeCalculationResult.GetTotalAmounts(invoiceLine);
				AdjustCustomsValueOnLine(invoiceLine);
			}
			else
			{
				var dutyData = invoiceLine.GetDDPDisbursementDutyDataForCalculation(dutyFeeCalculationResult, customsValues, shouldCalculateMPF);

				ClearDutyFeeCalculationResult(invoiceLine, shouldCalculateMPF, isCombinedLine);

				SetOverriddenDutiesAndFees(invoiceLine);

				dutyCalculator.Calculate(dutyData, feeCalculator);//calculate duty including secondary if required
				dutyData.CalculateNormalDutyIfRequired(dutyCalculator);

				result = dutyFeeCalculationResult.GetTotalAmounts(invoiceLine);

				if (ShouldCalculateSecondaryWhenParentIsCalculated(invoiceLine))
				{
					foreach (JobComInvoiceLine secondaryLine in invoiceLine.SecondaryTariffLines)
					{
						result += CalculateDisbursementCharge(secondaryLine, feeCalculator, shouldCalculateMPF);//calculate fees and ADD/CVD duty 
					}
				}

				if (!invoiceLine.IsSecondaryTariffLine)
				{
					var invoiceLines = new List<JobComInvoiceLine>(invoiceLine.SecondaryTariffLines);
					invoiceLines.Add(invoiceLine);

					ApportionDisbursementChargeIfRequired(invoiceLines, result);

					AdjustCustomsValue(invoiceLines);
				}
			}

			return result.Round(5);
		}

		void ClearDutyFeeCalculationResult(JobComInvoiceLine invoiceLine, bool shouldCalculateMPF, bool isCombinedLine)
		{
			var chargesToKeep = EntryLevelFeeCalculator.GetEntryLevelFeeCodes();//entry level fees are calculated at the start and should be kept. It is not calculated as part of this routine
			if (!shouldCalculateMPF)
			{
				chargesToKeep = chargesToKeep.Concat(new string[] { Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing });
			}

			if (isCombinedLine)
			{
				var combinedLines = invoiceLine.GetCombinedLines();
				foreach (var combinedLine in combinedLines)
				{
					dutyFeeCalculationResult.RemoveChargesExcept(combinedLine, chargesToKeep);
				}
			}
			else
			{
				var parentTariffLine = invoiceLine.ParentTariffLine;

				if (parentTariffLine == null || !ShouldCalculateSecondaryWhenParentIsCalculated(parentTariffLine))
				{
					dutyFeeCalculationResult.RemoveChargesExcept(invoiceLine, chargesToKeep);
				}

				//Repair and in-lieu tariff duty calculation calculuates duty and apportions them to all of its secondary lines when their parent line is calculated. Clearing duty & Fee should happen when duty is calculated at the parent level
				if (ShouldCalculateSecondaryWhenParentIsCalculated(invoiceLine))
				{
					foreach (JobComInvoiceLine secondaryLine in invoiceLine.SecondaryTariffLines)
					{
						dutyFeeCalculationResult.RemoveChargesExcept(secondaryLine, chargesToKeep);
					}
				}
			}
		}
		void SetOverriddenDutiesAndFees(JobComInvoiceLine invoiceLine)
		{
			if (invoiceLine.US_OverrideDuty || invoiceLine.US_OverrideSupDuty || invoiceLine.US_OverrideSupAdditionalTariff1Duty || invoiceLine.US_OverrideSupAdditionalTariff2Duty || invoiceLine.US_OverrideSupAdditionalTariff3Duty || invoiceLine.US_OverrideSupAdditionalTariff4Duty || invoiceLine.US_OverrideSupAdditionalTariff5Duty)
			{
				var totalDutyOverridden = (invoiceLine.US_OverrideDuty ? invoiceLine.US_Duty : ZDecimal.Zero) + (invoiceLine.US_OverrideSupDuty ? invoiceLine.US_SupDuty : ZDecimal.Zero) + (invoiceLine.US_OverrideSupAdditionalTariff1Duty ? invoiceLine.US_SupAdditionalTariff1Duty : ZDecimal.Zero)
											+ (invoiceLine.US_OverrideSupAdditionalTariff2Duty ? invoiceLine.US_SupAdditionalTariff2Duty : ZDecimal.Zero) + (invoiceLine.US_OverrideSupAdditionalTariff3Duty ? invoiceLine.US_SupAdditionalTariff3Duty : ZDecimal.Zero)
											+ (invoiceLine.US_OverrideSupAdditionalTariff4Duty ? invoiceLine.US_SupAdditionalTariff4Duty : ZDecimal.Zero) + (invoiceLine.US_OverrideSupAdditionalTariff5Duty ? invoiceLine.US_SupAdditionalTariff5Duty : ZDecimal.Zero);
				dutyFeeCalculationResult.SetDutyFeeCalculationResult(invoiceLine, Core.Constants.Customs.CusEntryFeeTypes.DutyAmount, new DDPCalculationResultData(totalDutyOverridden, new FeeCalculationInternalData(totalDutyOverridden, 0m)));
			}

			if (invoiceLine.IsADDManual)
			{
				dutyFeeCalculationResult.SetDutyFeeCalculationResult(invoiceLine, Core.Constants.USCustoms.FeeCodes.AntidumpingDuty, new DDPCalculationResultData(invoiceLine.US_ADDuty, new FeeCalculationInternalData(invoiceLine.US_ADDuty, 0m)));
			}

			if (invoiceLine.IsCVDManual)
			{
				dutyFeeCalculationResult.SetDutyFeeCalculationResult(invoiceLine, Core.Constants.USCustoms.FeeCodes.CountervailingDuty, new DDPCalculationResultData(invoiceLine.US_CVDuty, new FeeCalculationInternalData(invoiceLine.US_CVDuty, 0m)));
			}

			foreach (FeeCusCodeData feeData in invoiceLine.FeeCusCodes)
			{
				if (feeData.CY_IsOverridden)
				{
					dutyFeeCalculationResult.SetDutyFeeCalculationResult(invoiceLine, feeData.CY_Code, new DDPCalculationResultData(feeData.CY_FeeAmount, new FeeCalculationInternalData(feeData.CY_FeeAmount, 0m)));
				}
			}
		}

		void AdjustCustomsValue(IEnumerable<JobComInvoiceLine> invoiceLines)
		{
			foreach (JobComInvoiceLine line in invoiceLines)
			{
				AdjustCustomsValueOnLine(line);
			}
		}

		void AdjustCustomsValueOnLine(JobComInvoiceLine line)
		{
			ZDecimal totalOriginalValue = line.TotalOriginalGoodsValueInUSD;
			ZDecimal newCustomsValueExcluding98GoodsValue = line.JI_CustomsValue - totalOriginalValue;
			if (newCustomsValueExcluding98GoodsValue > 0 && !dutyFeeCalculationResult.HasRateGreaterThanOne(line))
			{
				newCustomsValueExcluding98GoodsValue -= GetDisbursementChargeAmount(line);
			}

			var result = new CustomsValues();

			result.SupCustomsValue = CustomsValueDeciderForInvoiceLine.GetCustomsValueForDDP(line, true, newCustomsValueExcluding98GoodsValue);
			result.CustomsValue = CustomsValueDeciderForInvoiceLine.GetCustomsValueForDDP(line, false, newCustomsValueExcluding98GoodsValue);

			customsValues[line] = result;
		}

		//If a parent line price is zero and system does not do the following, having a deduction would lead to a negative customs value.
		void ApportionDisbursementChargeIfRequired(List<JobComInvoiceLine> invoiceLines, ZDecimal totalDisbursementAmount)
		{
			ZDecimal totalLinePrice = ZDecimal.Zero;
			bool hasLinesWithZeroPriceButWithDDDCost = false;

			foreach (JobComInvoiceLine line in invoiceLines)
			{
				totalLinePrice += line.JI_LinePrice;

				hasLinesWithZeroPriceButWithDDDCost |= (line.JI_LinePrice == 0m && dutyFeeCalculationResult.GetTotalAmounts(line) > 0m);
			}

			if (hasLinesWithZeroPriceButWithDDDCost && totalLinePrice != 0)
			{
				JobComInvoiceLine lineWithHighestPrice = null;

				ZDecimal totalApportioned = 0m;

				foreach (JobComInvoiceLine line in invoiceLines)
				{
					if (lineWithHighestPrice == null || lineWithHighestPrice.JI_LinePrice < line.JI_LinePrice)
					{
						if (line.JI_LinePrice > 0)
						{
							lineWithHighestPrice = line;
						}
					}

					ZDecimal apportioned = totalDisbursementAmount * line.JI_LinePrice / totalLinePrice;
					apportioned = apportioned.Round(2);
					totalApportioned += apportioned;

					dutyFeeCalculationResult.SetDutyFeeCalculationResult(line, USCustomsChargeTypeList.Codes.DisbursementCharge, new DDPCalculationResultData(apportioned, new FeeCalculationInternalData(apportioned, 0m)));
				}

				if (totalApportioned != totalDisbursementAmount && lineWithHighestPrice != null)
				{
					ZDecimal existingAmount = dutyFeeCalculationResult.GetChargeOrFeeAmount(lineWithHighestPrice, USCustomsChargeTypeList.Codes.DisbursementCharge);
					var amount = existingAmount + (totalDisbursementAmount - totalApportioned);

					dutyFeeCalculationResult.SetDutyFeeCalculationResult(lineWithHighestPrice, USCustomsChargeTypeList.Codes.DisbursementCharge, new DDPCalculationResultData(amount, new FeeCalculationInternalData(amount, 0m)));
				}
			}
		}

		bool ApportionDisbursementChargeIfRequiredWithoutCalculation(List<JobComInvoiceLine> invoiceLines, ZDecimal totalDisbursementAmount)
		{
			var hasApportioned = false;
			ZDecimal totalLinePrice = ZDecimal.Zero;
			bool hasLinesWithZeroPriceButWithDDDCost = false;

			foreach (JobComInvoiceLine line in invoiceLines)
			{
				totalLinePrice += line.JI_LinePrice;

				hasLinesWithZeroPriceButWithDDDCost |= (line.JI_LinePrice == 0m && GetDDDAmount(line) > 0m);
			}

			if (hasLinesWithZeroPriceButWithDDDCost && totalLinePrice != 0)
			{
				JobComInvoiceLine lineWithHighestPrice = null;

				ZDecimal totalApportioned = 0m;

				foreach (JobComInvoiceLine line in invoiceLines)
				{
					if (lineWithHighestPrice == null || lineWithHighestPrice.JI_LinePrice < line.JI_LinePrice)
					{
						if (line.JI_LinePrice > 0)
						{
							lineWithHighestPrice = line;
						}
					}

					ZDecimal apportioned = totalDisbursementAmount * line.JI_LinePrice / totalLinePrice;
					apportioned = apportioned.Round(2);
					totalApportioned += apportioned;

					SetDisbursementCharge(line, apportioned, System.Array.Empty<JobComInvCharge>());
				}

				if (totalApportioned != totalDisbursementAmount && lineWithHighestPrice != null)
				{
					ZDecimal existingAmount = lineWithHighestPrice.ApportionedCharges.GetCharge(USCustomsChargeTypeList.Codes.DisbursementCharge)[0].J7_Amount;

					SetDisbursementCharge(lineWithHighestPrice, existingAmount + (totalDisbursementAmount - totalApportioned), System.Array.Empty<JobComInvCharge>());
				}
				hasApportioned = true;
			}
			return hasApportioned;
		}

		bool IsRepairOrAssemblyLine(IDutyData parentLine)
		{
			return parentLine != null && parentLine.ImportTariff != null &&
				(
					parentLine.ImportTariff.Applies(TariffRuleList.Codes.RepairTariffs, parentLine.DateForDutyCalculation) ||
					parentLine.ImportTariff.Applies(TariffRuleList.Codes.AssembledAbroadOfUSProducts, parentLine.DateForDutyCalculation)
				);
		}

		bool IsInLieuTariff(IDutyData invoiceLine)
		{
			return invoiceLine != null && invoiceLine.ImportTariff != null &&
				invoiceLine.ImportTariff.Applies(TariffRuleList.Codes.InLieuTariffs, invoiceLine.DateForDutyCalculation) &&
				!invoiceLine.ImportTariff.Applies(TariffRuleList.Codes.AdditionalTariffs, invoiceLine.DateForDutyCalculation);
		}

		void SetDisbursementCharge(IChargeApportionee chargeApportionee, ZDecimal disbursementAmount, JobComInvCharge[] dddInvoiceCharges)
		{
			JobComInvCharge lineDisbursementCharge = null;

			foreach (BaseJobComInvHeaderCharge lineCharge in chargeApportionee.ApportionedCharges)
			{
				if (lineCharge.J7_ChargeType == USCustomsChargeTypeList.Codes.DisbursementCharge && lineCharge.J7_IsSystem)
				{
					lineDisbursementCharge = lineCharge;
					break;
				}
			}

			if (lineDisbursementCharge == null)
			{
				lineDisbursementCharge = chargeApportionee.ApportionedCharges.AddNew();
				lineDisbursementCharge.J7_IsSystem = true;
				lineDisbursementCharge.J7_ChargeType = USCustomsChargeTypeList.Codes.DisbursementCharge;
			}

			if (lineDisbursementCharge != null)
			{
				lineDisbursementCharge.J7_IsDutiable = false;
				lineDisbursementCharge.J7_IsGSTApplicable = false;
				lineDisbursementCharge.J7_IsNotIncludedInInvoice = false;
				lineDisbursementCharge.J7_AdjustedCharge = dddInvoiceCharges.Length > 0;
				lineDisbursementCharge.J7_IsIncludedInITOT = dddInvoiceCharges.Length == 0 || dddInvoiceCharges[0].J7_IsIncludedInITOT;
				lineDisbursementCharge.J7_Amount = disbursementAmount.Round(2);
				lineDisbursementCharge.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			}
		}

		void ApportionMinimumMaximumMPFIfRequired(ZDecimal totalCustomsValueSubjectToMPF)
		{
			if (totalCustomsValueSubjectToMPF > 0m)
			{
				var currentMPFRate = new FeeCalculationHelper(declaration.Factory, declaration.DateForMPFCalculation).GetCurrentRate(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing);
				if (currentMPFRate != null)
				{
					var mPFPercent = currentMPFRate.ZZF_Value;
					var customsValue = totalCustomsValueSubjectToMPF / (1 + (mPFPercent / 100m));
					var totalMPF = customsValue * mPFPercent / 100m;

					var mPFMinimumAmount = currentMPFRate.ZZF_Minimum;
					var mPFMaximumAmount = currentMPFRate.ZZF_Maximum;
					if (totalMPF > 0m && totalMPF < mPFMinimumAmount || totalMPF > mPFMaximumAmount)
					{
						totalMPF = totalMPF > mPFMaximumAmount ? mPFMaximumAmount : mPFMinimumAmount;

						JobComInvoiceLine invoiceLineWithMaxValue = null;

						foreach (JobComInvoiceLine invoiceLine in declaration.InvoiceLines)
						{
							if (InvoiceLineSubjectForMPF(invoiceLine))
							{
								if (invoiceLineWithMaxValue == null || invoiceLineWithMaxValue.JI_CustomsValue < invoiceLine.JI_CustomsValue)
								{
									invoiceLineWithMaxValue = invoiceLine;
								}

								var apportionedMPF = totalMPF * (customsValues.GetTotalCustomsValue(invoiceLine) - invoiceLine.TotalOriginalGoodsValueInUSD) / totalCustomsValueSubjectToMPF;

								dutyFeeCalculationResult.SetDutyFeeCalculationResult(invoiceLine, Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, new DDPCalculationResultData(apportionedMPF, new FeeCalculationInternalData(apportionedMPF, 0m)));
							}
						}

						var totalApportioned = dutyFeeCalculationResult.GetTotalAmounts(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing);

						if (totalApportioned != totalMPF && invoiceLineWithMaxValue != null)
						{
							decimal amountToSet = dutyFeeCalculationResult.GetChargeOrFeeAmount(invoiceLineWithMaxValue, Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing) + (totalMPF - totalApportioned);
							dutyFeeCalculationResult.SetDutyFeeCalculationResult(invoiceLineWithMaxValue, Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, new DDPCalculationResultData(amountToSet, new FeeCalculationInternalData(amountToSet, 0m)));
						}
					}
				}
			}
		}

		bool InvoiceLineSubjectForMPF(JobComInvoiceLine invoiceLine)
		{
			bool result = false;

			if (invoiceLine.JI_CustomsValue > 0m)
			{
				FeeCusCodeData mpfData = invoiceLine.FeeCusCodes.GetFirstElementHaving(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing);

				if (mpfData != null && mpfData.CY_IsOverridden && mpfData.CY_FeeAmount > 0m || ShouldHaveMPFFeeOnInvoiceLine(invoiceLine))
				{
					result = true;
				}
			}

			return result;
		}

		bool ShouldHaveMPFFeeOnInvoiceLine(JobComInvoiceLine invoiceLine)
		{
			var calculator = new MPFCalculator(invoiceLine.Factory);
			var checkLinesList = new List<IDutyData>();
			if (invoiceLine.IsCombinedLine())
			{
				checkLinesList = invoiceLine.AdditionalEntryLineLinks.AdditionalEntryLines.Cast<IDutyData>().Where(x => x != null).Distinct().ToList();
			}
			else
			{
				checkLinesList.Add(invoiceLine);
			}
			return checkLinesList.Any(x => calculator.ShouldHaveFee(x));
		}

		void RefreshCusEntryLineCustomsValue(JobDeclaration declaration)
		{
			CusEntryHeader entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;

			if (entry != null)
			{
				foreach (CusEntryLine entryLine in entry.MergedLines)
				{
					entryLine.ResetTotalsAndCachedValues();

					foreach (JobComInvoiceLine invoiceLine in entryLine.InvoiceLines)
					{
						entryLine.MergeInvoiceLine(invoiceLine);
					}

					entryLine.RoundCustomsValue();
				}
			}
		}
	}
}
