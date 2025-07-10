using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business.AWB
{
	public class CalculationLogsAnalyzer
	{
		public CalculationLogsAnalyzer(ExportAWBHeader exportAWBHeader)
		{
			Argument.NotNull(exportAWBHeader, "exportAWBHeader");
			Header = exportAWBHeader;
			Factory = new BusinessObjectFactory();
		}

		readonly ExportAWBHeader Header;
		readonly BusinessObjectFactory Factory;

		#region Logs Wrapper

		public CalculationLogsWrapper LogsWrapper
		{
			get
			{
				if (LogsWrapperCollection.IsNullOrEmpty())
				{
					return null;
				}

				return LogsWrapperCollection.First();
			}
		}

		public IEnumerable<CalculationLogsWrapper> LogsWrapperCollection
		{
			get
			{
				if (logsWrapperCollection.IsNullOrEmpty())
				{
					var bizoWithLogs = GetAllBusinessObjectsWithCalculationLogs()?.WhereNotNull();

					if (!bizoWithLogs.IsNullOrEmpty())
					{
						logsWrapperCollection = bizoWithLogs
							.Select(bizoWithLog => CalculationLogsLoader.Load(bizoWithLog))
							.ToList();
					}
				}

				return logsWrapperCollection;
			}
		}
		IEnumerable<CalculationLogsWrapper> logsWrapperCollection;

		protected virtual IEnumerable<BusinessObject> GetAllBusinessObjectsWithCalculationLogs()
		{
			return null;
		}

		#endregion

		#region Disable Logs

		public void DisableLogs()
		{
			if (LogsWrapperCollection != null)
			{
				var bizoWithLogs = GetAllBusinessObjectsWithCalculationLogs()?.WhereNotNull();
				foreach (var bizo in bizoWithLogs)
				{
					var logWrapper = CalculationLogsLoader.Load(bizo);
					if (logWrapper != null)
					{
						logWrapper.IsDisabled = true;
						CalculationLogsLoader.Save(bizo, logWrapper);
					}
				}
				logsWrapperCollection = null;
			}

			DisableLogsCore();
		}

		protected virtual void DisableLogsCore()
		{
		}

		protected virtual bool IsDisabled { get; }

		#endregion

		#region Populate Rate Lines

		public int PopulateRateLines()
		{
			if (LogsWrapperCollection != null && !IsDisabled)
			{
				return PopulateRateLinesCore();
			}

			return 0;
		}

		int PopulateRateLinesCore()
		{
			int ratelineNumber = 0;
			try
			{
				foreach (var logWrapper in LogsWrapperCollection)
				{
					if (logWrapper != null && !logWrapper.IsEmpty && !logWrapper.IsDisabled)
					{
						foreach (CalculationLog calculationLog in logWrapper.Logs)
						{
							bool success = PopulateRateLinesFromLog(calculationLog, ref ratelineNumber);
							if (!success)
							{
								ratelineNumber = 0;
								break;
							}
							else
							{
								ConvertMonetaryAmountsToAWBCurrency(ratelineNumber, calculationLog.Currency);
							}
						}
					}
				}
			}
			catch (MaximumRatelineCountExceededException)
			{
				ratelineNumber = 0;
			}

			return ratelineNumber;
		}

		bool PopulateRateLinesFromLog(CalculationLog calculationLog, ref int ratelineNumber)
		{
			bool result = false;
			if (calculationLog.RateMode == Constants.ContainerModes.ULD)
			{
				result = PopulateULD(calculationLog, ref ratelineNumber);
			}
			else if (Constants.Weight.ContainsCode(calculationLog.Unit))
			{
				result = PopulateNonULD(calculationLog, ref ratelineNumber);
			}

			return result;
		}

		#endregion

		#region Populate NonULD

		bool PopulateNonULD(CalculationLog calculationLog, ref int ratelineNumber)
		{
			bool result = false;

			var commodityItemNumber = GetCommodityItemNumber(calculationLog);
			var useSpecificCommodityRate = commodityItemNumber != ZString.Empty;

			ZString weightUnit = calculationLog.Unit;
			if (IsMixedAndLoose())
			{
				PopulateNonULD_Mixed(++ratelineNumber, calculationLog);
				result = true;
			}
			else if (calculationLog.HasMinimum && calculationLog.Result <= calculationLog.Minimum)
			{
				PopulateMainLineNonULD(++ratelineNumber, calculationLog.Weight, commodityItemNumber, Constants.AWB.RateClass.MinimumCharge, weightUnit, calculationLog.Chargeable, calculationLog.ChargeableUnit, calculationLog.Minimum);
				result = true;
			}
			else if (calculationLog.Steps.Count == 0 && calculationLog.HasBaseRate)
			{
				PopulateMainLineNonULD(++ratelineNumber, calculationLog.Weight, commodityItemNumber, Constants.AWB.RateClass.BasicCharge, weightUnit, calculationLog.Chargeable, calculationLog.ChargeableUnit, calculationLog.BaseRate);
				result = true;
			}
			else if (calculationLog.Steps.Count == 1)
			{
				CalculationStep calculationStep = calculationLog.Steps[0];
				ZDecimal baseRate = calculationLog.BaseRate + calculationStep.Flat;
				if (baseRate > 0m)
				{
					PopulateMainLineNonULD(++ratelineNumber, calculationLog.Weight, commodityItemNumber, Constants.AWB.RateClass.BasicCharge, weightUnit, calculationLog.Chargeable, calculationLog.ChargeableUnit, baseRate);
					if (useSpecificCommodityRate)
					{
						PopulateAdditionalLine(++ratelineNumber, Constants.AWB.RateClass.SpecificCommodityRate, weightUnit, calculationStep.UnitCount, calculationStep.UnitPrice);
					}
					else
					{
						PopulateAdditionalLine(++ratelineNumber, Constants.AWB.RateClass.RatePerKilogram, weightUnit, calculationStep.UnitCount, calculationStep.UnitPrice);
					}
					result = true;
				}
				else
				{
					if (useSpecificCommodityRate)
					{
						PopulateMainLineNonULD(++ratelineNumber, calculationLog.Weight, commodityItemNumber, Constants.AWB.RateClass.SpecificCommodityRate, weightUnit, calculationStep.UnitCount, weightUnit, calculationStep.UnitPrice);
					}
					else
					{
						ZDecimal weightInKilograms = Constants.Weight.Convert(calculationStep.UnitCount, weightUnit, Constants.Weight.Kilograms);
						ZString rateClass = weightInKilograms >= 45m ? Constants.AWB.RateClass.QuantityRate : Constants.AWB.RateClass.NormalCharge;
						PopulateMainLineNonULD(++ratelineNumber, calculationLog.Weight, ZString.Empty, rateClass, weightUnit, calculationStep.UnitCount, weightUnit, calculationStep.UnitPrice);
					}
					result = true;
				}
			}

			return result;
		}

		void PopulateNonULD_Mixed(int lineNumber, CalculationLog calculationLog)
		{
			var loosePackCount = GetNumberOfLoosePieces();
			var numberOfPieces = Math.Min(loosePackCount, 9999).ToString(CultureInfo.InvariantCulture);
			var calculationStep = calculationLog.Steps.FirstOrDefault();
			var unitPrice = calculationStep.UnitPrice;

			var rateLine = PopulateLineConverted(lineNumber, numberOfPieces, calculationLog.Weight, calculationLog.Unit, "", "", calculationLog.Chargeable, calculationLog.ChargeableUnit, unitPrice);

			using (rateLine.SuspendSettingHasChanges())
			using (rateLine.SuppressTotalAndRateChargeCalculation())
			{
				rateLine.ER_Total = calculationStep.Result;
			}

			Header.PopulateNatureAndQtyOfGoodsLine(lineNumber, loosePackCount + " " + ExportAWBHeader.Constants.SLAC);
			Header.PopulateNatureAndQtyOfGoodsType(lineNumber, Core.Constants.AWB.NatureAndQtyOfGoodsTypes.ShippersLoadAndCount);
		}

		#endregion

		#region Populate ULD

		bool PopulateULD(CalculationLog calculationLog, ref int ratelineNumber)
		{
			bool result = false;

			if (CheckULD_PerContainerPerWeightBased(calculationLog))
			{
				PopulateULD_PerContainerPerWeightBased(calculationLog, ref ratelineNumber);
				result = true;
			}
			else if (Constants.Weight.ContainsCode(calculationLog.Unit))
			{
				if (CheckULD_PivotBased(calculationLog))
				{
					PopulateULD_PivotBased(calculationLog, ref ratelineNumber);
					result = true;
				}
				else if (calculationLog.HasBaseRate)     //NOTE: Doubts about this logic, should be considered for discussion
				{
					PopulateULD_ContainerBased(calculationLog, ref ratelineNumber);
					result = true;
				}
				else if (calculationLog.Steps.Count > 0)
				{
					PopulateULD_WeightBased(calculationLog, ref ratelineNumber);
					result = true;
				}
			}
			else if (calculationLog.Unit == QuantityUnit.CN)
			{
				PopulateULD_PerContainer(calculationLog, ref ratelineNumber);
				result = true;
			}

			return result;
		}

		/// <summary>
		/// Identifies whether the calculation log contains appropriate Steps that
		/// would require pivot-based AWB lines.
		/// </summary>
		bool CheckULD_PivotBased(CalculationLog calculationLog)
		{
			var hasPivotBreak = false;
			var hasOverPivotPrice = false;
			var underPivotPriceCount = 0;

			foreach (var step in calculationLog.Steps)
			{
				if (step.UnitPrice == ZDecimal.Zero && step.UnitCount != ZDecimal.Zero)
				{
					hasPivotBreak = true;
				}

				if (step.UnitPrice != ZDecimal.Zero && step.UnitCount != ZDecimal.Zero)
				{
					hasOverPivotPrice = true;
				}

				if (step.UnitPrice == ZDecimal.Zero && step.Flat != ZDecimal.Zero)
				{
					underPivotPriceCount++;
				}
			}

			// Steps that are over (or at) pivot include a pivot-break step and an over-pivot price step
			// Steps that are under pivot include only a single step.
			return (hasPivotBreak && hasOverPivotPrice) || underPivotPriceCount == 1;
		}

		/// <summary>
		/// Handles under, at, or over pivot breaks.
		/// </summary>
		void PopulateULD_PivotBased(CalculationLog calculationLog, ref int ratelineNumber)
		{
			var containerCode = calculationLog.ContainerCode;
			var weightUnit = calculationLog.Unit;
			var commodityItemNumber = GetCommodityItemNumber(calculationLog);

			var baseRate = calculationLog.BaseRate + calculationLog.Steps.Sum(s => s.Flat);
			var pivotWeight = calculationLog.Steps.Where(s => s.UnitPrice == 0).Sum(s => s.UnitCount);

			PopulateMainLineULD(++ratelineNumber, containerCode, calculationLog.Weight, commodityItemNumber, Constants.AWB.RateClass.UnitLoadDeviceBasicCharge, weightUnit, pivotWeight, weightUnit, baseRate, total: baseRate);

			foreach (var additionalStep in calculationLog.Steps.Where(s => s.UnitPrice != 0))
			{
				PopulateAdditionalLine(++ratelineNumber, Constants.AWB.RateClass.UnitLoadDeviceAdditionalCharge, weightUnit, additionalStep.UnitCount, additionalStep.UnitPrice);
			}

			PopulateULD_ContainersInformation(containerCode, ref ratelineNumber);
		}

		#region ULD Per Container Per Weight (Used when doing CG rates)

		/// <summary>
		///		This is for ULD setup when the client is charged per container and then per additional kilogram over a pivot defined by a rate for the container.
		///		Also, a container has max payload weight, and if goods weight exceeds container max payload weight, additional container will be charged
		///		and the weight will be split between 2 containers.
		///
		///		For example, assume a job has AKE ULD container with max payload 1400 KG, the user packed 1600 KG, and the rate has pivot 700 KG, i.e.:
		///		$5000 per ULD
		///		Less than 700 KG: $0
		///		More than 700 KG: $2 per KG over 700 KG
		///
		///		Since 1600 KG exceeds 1400 KG max payload, 2 containers will be charged instead of 1. Which means, each of them will include 800 KG of goods.
		///		Since pivot per container is 700 KG, each container will charge 2$ per 100 KG over the pivot (800 KG goods - 700 KG pivot)
		///
		///		So, this setup will lead to the following calculations in calculation log:
		///		Calculation Log:
		///			Weight: 1600 KG
		///			ContainerCode: ULD
		///			Unit: CN (because it is ULD, but in this setup it should be ignored by AWB and use units on steps instead)
		///			Steps:
		///				Step1: UnitCount=1, Unit=CN, UnitPrice=$5000
		///				Step2: UnitCount=700, Unit=KG, UnitPrice=$0
		///				Step3: UnitCount=100, Unit=KG, UnitPrice=$2
		///				Step4: UnitCount=1, Unit=CN, UnitPrice=$5000
		///				Step5: UnitCount=700, Unit=KG, UnitPrice=$0
		///				Step6: UnitCount=100, Unit=KG, UnitPrice=$2
		///
		///		I.e. Step 4-6 repeat Step 1-3 as we have 2 containers and we have steps for each individual container.
		///		It will be merged here, and adjusted to the same weight unit if necessary (although it should have the same unit).
		/// </summary>
		bool CheckULD_PerContainerPerWeightBased(CalculationLog calculationLog)
		{
			var perUnitSteps = calculationLog.Steps.Where(s => !s.Unit.IsEmpty).ToList();
			var perContainerSteps = perUnitSteps.Where(s => s.Unit == QuantityUnit.CN).ToList();
			var perWeightSteps = perUnitSteps.Where(s => Constants.Weight.ContainsCode(s.Unit)).ToList();

			if (!perContainerSteps.Any() || !perWeightSteps.Any())
			{
				return false;
			}

			if (!perContainerSteps.AllSame(s => s.UnitPrice))
			{
				// In theory one price is expected. We don't support calculations with different per container prices.
				// But, if such case arises, we will think how to display it properly on AWB.
				return false;
			}

			return true;
		}

		void PopulateULD_PerContainerPerWeightBased(CalculationLog calculationLog, ref int ratelineNumber)
		{
			var perUnitSteps = calculationLog.Steps.Where(s => !s.Unit.IsEmpty).ToList();
			var perContainerSteps = perUnitSteps.Where(s => s.Unit == QuantityUnit.CN).ToList();
			var perWeightSteps = perUnitSteps.Where(s => Constants.Weight.ContainsCode(s.Unit)).ToList();

			var containerCode = calculationLog.ContainerCode;
			var weightUnit = Constants.Weight.IsImperial(perWeightSteps.First().Unit)
				? Constants.Weight.Pounds
				: Constants.Weight.Kilograms;
			var commodityItemNumber = GetCommodityItemNumber(calculationLog);

			var numberOfContainers = (int)perContainerSteps.Sum(s => s.UnitCount);
			var perContainerRate = perContainerSteps.First().UnitPrice;             // The price should be the same for all per container steps
			var perContainerResult = perContainerRate * numberOfContainers;

			// Basically pivot weight is the weight we charged with 0 rate, i.e. it is included in the container price
			var pivotWeight = perWeightSteps.Where(s => s.UnitPrice == 0).Sum(s => GetConvertedValue(s.UnitCount, s.Unit, weightUnit));

			// Per container results
			PopulateMainLineULD(++ratelineNumber, containerCode, calculationLog.Weight, commodityItemNumber, Constants.AWB.RateClass.UnitLoadDeviceBasicCharge, weightUnit, pivotWeight, weightUnit, perContainerRate, numberOfContainers, perContainerResult);

			// Align all weight rates to the same unit and grouping them by price. Each price will have its own line on the AWB.
			var perWeightStepsGroupedByPrice = perWeightSteps.Where(s => s.UnitPrice != 0).GroupBy(s => GetConvertedValue(s.UnitPrice, s.Unit, weightUnit));

			// Per weight results
			foreach (var group in perWeightStepsGroupedByPrice)
			{
				var weight = group.Sum(s => s.UnitCount);
				var price = group.Key;

				PopulateAdditionalLine(++ratelineNumber, Constants.AWB.RateClass.UnitLoadDeviceAdditionalCharge, weightUnit, weight, price);
			}

			PopulateULD_ContainersInformation(containerCode, ref ratelineNumber);
		}

		#endregion

		void PopulateULD_ContainerBased(CalculationLog calculationLog, ref int ratelineNumber)
		{
			ZString containerCode = calculationLog.ContainerCode;
			ZString weightUnit = calculationLog.Unit;
			var baseRate = calculationLog.BaseRate;
			var commodityItemNumber = GetCommodityItemNumber(calculationLog);

			PopulateMainLineULD(++ratelineNumber, containerCode, calculationLog.Weight, commodityItemNumber, Constants.AWB.RateClass.UnitLoadDeviceBasicCharge, weightUnit, calculationLog.Chargeable, calculationLog.ChargeableUnit, baseRate, total: baseRate);

			foreach (CalculationStep additionalStep in calculationLog.Steps)
			{
				PopulateAdditionalLine(++ratelineNumber, Constants.AWB.RateClass.UnitLoadDeviceAdditionalCharge, weightUnit, additionalStep.UnitCount, additionalStep.UnitPrice);
			}

			PopulateULD_ContainersInformation(containerCode, ref ratelineNumber);
		}

		void PopulateULD_WeightBased(CalculationLog calculationLog, ref int ratelineNumber)
		{
			ZString containerCode = calculationLog.ContainerCode;
			ZString weightUnit = calculationLog.Unit;
			var commodityItemNumber = GetCommodityItemNumber(calculationLog);

			CalculationStep firstCalculationStep = calculationLog.Steps[0];
			PopulateMainLineULD(++ratelineNumber, containerCode, calculationLog.Weight, commodityItemNumber, Constants.AWB.RateClass.UnitLoadDeviceBasicCharge, weightUnit, firstCalculationStep.UnitCount, weightUnit, firstCalculationStep.UnitPrice);

			foreach (CalculationStep additionalStep in calculationLog.Steps.Skip(1))
			{
				PopulateAdditionalLine(++ratelineNumber, Constants.AWB.RateClass.UnitLoadDeviceAdditionalCharge, weightUnit, additionalStep.UnitCount, additionalStep.UnitPrice);
			}

			PopulateULD_ContainersInformation(containerCode, ref ratelineNumber);
		}

		void PopulateULD_PerContainer(CalculationLog calculationLog, ref int ratelineNumber)
		{
			var isFirstLine = ratelineNumber == 0;

			var (numberOfPieces, grossWeight, weightUnit, _, commodityItemNumber, rate) = ULDRateLineValues(calculationLog);
			PopulateLine(++ratelineNumber, ((int)numberOfPieces).ToString(CultureInfo.InvariantCulture), grossWeight, GetLineWeightUnit(weightUnit), commodityItemNumber, Constants.AWB.RateClass.UnitLoadDeviceBasicCharge, 0m, rate);

			PopulateULD_ContainersInformation(calculationLog.ContainerCode, ref ratelineNumber, !isFirstLine);
		}

		(ZInt, ZDecimal, ZString, ZInt, ZString, ZDecimal) ULDRateLineValues(CalculationLog calculationLog)
		{
			var containers = GetULDContainers(calculationLog.ContainerCode);
			var numberOfPieces = containers.Sum(container => container.JC_Calc_ContainerCount);
			var packLines = containers.SelectMany(container => container.PackLines).Cast<PackLine>();
			var weightUnit = packLines.All(packLine => Constants.Weight.IsImperial(packLine.JL_ActualWeightUQ)) ? Constants.Weight.Pounds : Constants.Weight.Kilograms;
			var grossWeight = packLines.Sum(packLine => Constants.Weight.Convert(packLine.JL_ActualWeight, packLine.JL_ActualWeightUQ, weightUnit));
			var packageCount = containers.Sum(container => container.JC_Calc_TotalPackages);
			var commodityItemNumber = GetCommodityItemNumber(calculationLog);
			var rate = calculationLog.Steps.FirstOrDefault().UnitPrice;

			return (numberOfPieces, grossWeight, weightUnit, packageCount, commodityItemNumber, rate);
		}

		void PopulateULD_ContainersInformation(ZString containerCode, ref int ratelineNumber, bool isFirstSCACOnPreviousLine = false)
		{
			int currentRateLineNumber = ratelineNumber;
			var builder = new AWBCalculationLogRateLineBuilder(Header, containerCode, currentRateLineNumber, isFirstSCACOnPreviousLine);
			builder.BuildRatelines(Header.AWBRateLines);

			ratelineNumber = builder.RateLineNumber;
		}

		#endregion

		#region Populate Line

		protected ExportAWBRateLine PopulateLine(int lineNumber, ZString numberOfPieces, ZDecimal grossWeight, ZString ratelineWeightUnit, ZString commodityItemNumber, ZString rateClass, ZDecimal chargeableWeight, ZDecimal rateCharge)
		{
			if (lineNumber > ExportAWBHeader.Constants.NumberOfRateLines)
			{
				throw new MaximumRatelineCountExceededException();
			}

			ExportAWBRateLine rateLine = Header.AWBRateLines[ExportAWBRateLine.Schema.ER_LineCount, (ZByte)lineNumber];
			using (rateLine.SuspendSettingHasChanges())
			{
				rateLine.ER_NoOfPiecesOrRCP = numberOfPieces;
				rateLine.ER_WeightInLBsOrKGs = grossWeight == 0m ? ZString.Empty : ratelineWeightUnit;
				rateLine.ER_GrossWeight = grossWeight;
				rateLine.ER_CommodityItemNumber = commodityItemNumber;
				rateLine.ER_RateClass = rateClass;
				rateLine.ER_ChargeableWeight = chargeableWeight;
				rateLine.ER_RateChargeOrDiscount = rateCharge;
			}

			rateLine.MarkAsNeedingValidation();

			return rateLine;
		}

		protected ExportAWBRateLine PopulateLineConverted(int lineNumber, ZString numberOfPieces, ZDecimal grossWeight, ZString weightUnit, ZString commodityItemNumber, ZString rateClass, ZDecimal chargeable, ZString chargeableUnit, ZDecimal rateCharge)
		{
			ZString ratelineWeightUnit = GetLineWeightUnit(weightUnit);
			ZDecimal rateChargeConverted = GetConvertedValue(rateCharge, weightUnit);
			ZDecimal grossWeightConverted = GetConvertedValue(grossWeight, weightUnit);

			ZDecimal chargeableWeightConverted = grossWeightConverted;
			if (Constants.Weight.ContainsCode(chargeableUnit))
			{
				chargeableWeightConverted = GetConvertedValue(chargeable, chargeableUnit);
			}

			return PopulateLine(lineNumber, numberOfPieces, grossWeightConverted, ratelineWeightUnit, commodityItemNumber, rateClass, chargeableWeightConverted, rateChargeConverted);
		}

		protected ExportAWBRateLine PopulateMainLineNonULD(int lineNumber, ZDecimal grossWeight, ZString commodityItemNumber, ZString rateClass, ZString weightUnit, ZDecimal chargeable, ZString chargeableUnit, ZDecimal rateCharge)
		{
			var numberOfPieces = lineNumber == 1
				? NumberOfPieces
				: ZString.Empty;

			var lineGrossWeight = lineNumber == 1
				? grossWeight
				: ZDecimal.Zero;

			return PopulateLineConverted(lineNumber, numberOfPieces, lineGrossWeight, weightUnit, commodityItemNumber, rateClass, chargeable, chargeableUnit, rateCharge);
		}

		protected ExportAWBRateLine PopulateMainLineULD(int lineNumber, ZString containerCode, ZDecimal grossWeight, ZString commodityItemNumber, ZString rateClass, ZString weightUnit, ZDecimal chargeable, ZString chargeableUnit, ZDecimal rateCharge, int? numberOfPieces = null, ZDecimal? total = null)
		{
			if (!numberOfPieces.HasValue)
			{
				numberOfPieces = GetULDContainers(containerCode).Count();
			}

			ExportAWBRateLine line = PopulateLineConverted(lineNumber, numberOfPieces.Value.ToString(CultureInfo.InvariantCulture), grossWeight, weightUnit, commodityItemNumber, rateClass, chargeable, chargeableUnit, rateCharge);
			using (line.SuspendSettingHasChanges())
			using (line.SuppressTotalAndRateChargeCalculation())
			{
				line.ER_Total = total ?? ZArchitecture.Core.Utilities.Round(line.ER_ChargeableWeight * line.ER_RateChargeOrDiscount, line.CurrencyDecimals);
			}

			return line;
		}

		protected ExportAWBRateLine PopulateAdditionalLine(int lineNumber, ZString rateClass, ZString weightUnit, ZDecimal chargeableWeight, ZDecimal rateCharge)
		{
			ZString numberOfPieces = "";
			ZDecimal grossWeight = 0m;

			ExportAWBRateLine line = PopulateLineConverted(lineNumber, numberOfPieces, grossWeight, weightUnit, ZString.Empty, rateClass, chargeableWeight, weightUnit, rateCharge);
			using (line.SuspendSettingHasChanges())
			{
				line.ER_WeightInLBsOrKGs = "";
			}

			return line;
		}

		#endregion

		#region Conversion Methods

		protected ZString GetLineWeightUnit(ZString weightUnit)
		{
			return Constants.Weight.IsImperial(weightUnit) ? Constants.AWB.RateLineUQ.Pounds : Constants.AWB.RateLineUQ.Kilos;
		}

		protected ZString GetTargetWeightUnit(ZString sourceWeightUnit)
		{
			return Constants.Weight.IsImperial(sourceWeightUnit) ? Constants.Weight.Pounds : Constants.Weight.Kilograms;
		}

		protected ZDecimal GetConvertedValue(ZDecimal sourceValue, ZString sourceWeightUnit, ZString? targetWeightUnit = null)
		{
			if (!targetWeightUnit.HasValue)
			{
				targetWeightUnit = Constants.Weight.IsImperial(sourceWeightUnit) ? Constants.Weight.Pounds : Constants.Weight.Kilograms;
			}

			return Constants.Weight.Convert(sourceValue, sourceWeightUnit, targetWeightUnit);
		}

		#endregion

		#region ConvertMonetaryAmountsToAWBCurrency

		protected virtual void ConvertMonetaryAmountsToAWBCurrency(int numberOfLinesPopulated, ZString currencyCode)
		{
			for (int lineNumber = 1; lineNumber <= numberOfLinesPopulated; lineNumber++)
			{
				ExportAWBRateLine rateLine = Header.AWBRateLines[ExportAWBRateLine.Schema.ER_LineCount, (ZByte)lineNumber];
				if (rateLine.ER_Total > 0m)
				{
					using (rateLine.SuspendSettingHasChanges())
					using (rateLine.SuppressTotalAndRateChargeCalculation())
					{
						rateLine.ER_Total = GetAmountInAWBCurrency(rateLine.ER_Total, currencyCode);
						rateLine.ER_RateChargeOrDiscount = GetAmountInAWBCurrency(rateLine.ER_RateChargeOrDiscount, currencyCode);
					}
				}
			}
		}

		protected virtual ZDecimal GetAmountInAWBCurrency(ZDecimal originalAmount, ZString originalCurrencyCode)
		{
			return originalAmount;
		}

		#endregion

		#region Properties & Methods for override

		protected IEnumerable<CommonContainer> GetULDContainers(ZString containerCode)
		{
			return Header.ULDContainers != null ? Header.ULDContainers.Where(x => x.RefContainer != null && x.RefContainer.RC_Code == containerCode) : Enumerable.Empty<CommonContainer>();
		}

		protected ZString NumberOfPieces
		{
			get { return Math.Min(GetNumberOfPieces(), 9999).ToString(CultureInfo.InvariantCulture); }
		}

		protected virtual ZInt GetNumberOfPieces()
		{
			return 1;
		}

		protected virtual ZInt GetNumberOfLoosePieces()
		{
			return 0;
		}

		public ZString GetCommodityItemNumber(CalculationLog calculationLog)
		{
			var commodityCode = calculationLog.CommodityCode;
			if (commodityCode == ZString.Empty)
			{
				return ZString.Empty;
			}

			var refCommodityCode = Factory.Load<RefCommodityCode>(new ZQuery(RefCommodityCodeSchema.RH_Code, commodityCode)).FirstOrDefault();
			var commodityItemNumber = refCommodityCode?.RH_IATACommodityItem ?? ZString.Empty;

			return commodityItemNumber;
		}

		#endregion

		#region MaximumRatelineCountExceededException

		[Serializable]
		public class MaximumRatelineCountExceededException : Exception
		{
			public MaximumRatelineCountExceededException()
				: base()
			{
			}

#if NETFRAMEWORK
			protected MaximumRatelineCountExceededException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
				: base(info, context)
			{
			}
#endif
		}

		#endregion

		#region Mixed ULD/Loose

		bool IsMixedAndLoose()
		{
			var logs = LogsWrapperCollection
				.Where(wrapper => wrapper != null && !wrapper.IsEmpty && !wrapper.IsDisabled)
				.SelectMany(wrapper => wrapper.Logs);
			var uldLineExists = logs.Any(log => log.RateMode == Constants.ContainerModes.ULD);
			var looseLineExists = logs.Any(log => !(log.RateMode == Constants.ContainerModes.ULD) && Constants.Weight.ContainsCode(log.Unit));
			return uldLineExists && looseLineExists;
		}

		#endregion
	}
}