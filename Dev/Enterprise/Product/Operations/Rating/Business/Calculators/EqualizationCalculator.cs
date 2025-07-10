using System.Collections.Generic;
using System.Linq;
using CargoWise.DataTransfer.Ratings;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Integration;
using Enterprise.ZArchitecture;

namespace Enterprise.Rating.Business
{
	[CalculatorProperty(Items.Operator.Plus, RateLineItem.Schema.TM_Type, IsMandatory = true)]
	[CalculatorProperty(Items.UseInclusiveBreaks, RateLineItem.Schema.TM_Text, IsMandatory = true, MapTo = "Bool1", RelatedTo = "UseInclusiveBreaks")]
	[CalculatorProperty("ContractHint", IsCalculatedField = true, MapTo = "String1", RelatedTo = "ContractHint")]
	public class EqualizationCalculator : Calculator
	{
		public EqualizationCalculator(IRateLine master)
			: base(master)
		{ }

		public const string Code = RatingCalculatorCodes.Equalization;

		#region Initialisation

		protected override IEnumerable<IRateLineItem> CheckOrCreateItems()
		{
			var itemList = base.CheckOrCreateItems();

			if (ParentRateEntry != null && !(Line is WiseLine) && (ParentRateEntry.IsAir() || ParentRateEntry.IsFCL() && ParentRateEntry.IsForwarding()))
			{
				if (!ParentRateEntry.IsFCL())
				{
					RateLineBizO.Parent.TI_Mode = Core.Constants.RateMode.ULD;
					RateLineBizO.TL_WeightVolume = RatingConstants.Units.KG;
				}

				RateLineBizO.Parent.Validation.ValidateTI_ContractNumber();
				RateLineBizO.UseOnlyActualWeightMeasure = true;
			}

			this.RateLineItems.CountChanged += (s, e) => this.String1Info.RefreshBinding();

			return itemList;
		}

		#endregion

		#region Properties

		public override ZBool UseInclusiveBreaks
		{
			get { return (ZBool)this[EqualizationCalculator.Items.UseInclusiveBreaks]; }
			set { this[EqualizationCalculator.Items.UseInclusiveBreaks] = value; }
		}

		public override bool CanBePrintedUsing6StandardOperatorColumnHeaders
		{
			get { return true; }
		}

		#endregion

		#region Calculation

		protected override void CalculateInternal(CalculatorOutput calcOutput)
		{
			var parameters = calcOutput.Parameters;
			var criteria = parameters.Criteria;

			if (!RateLineBizO.ViewAgentRates && _Rating.LocalSession != null)
			{
				var (equalizationUnitsCount, containerCount) = GetEqualizationUnitsAndContainerCount(RateLineBizO, parameters);

				var unit = RateLineBizO.TL_WeightVolume;
				if (!Equals(criteria.ConsumerType, JobInvoicingConsumerTypes.eManifest))
				{
					if (equalizationUnitsCount == 0 && (unit == QuantityUnit.KG || unit == QuantityUnit.M3))
					{
						calcOutput.FailureMessage = Res.GetString("1e2eaeef-66f0-4e06-88dd-630298a0b7da", "Weight/Volume information was not specified for Pack Lines on this job. Volume Equalization cannot be performed.");
						return;
					}
				}

				if (!_Rating.EqualizationInfo.ConsolWasEqualizedForRateLine(RateLineBizO.PK))
				{
					var equalizationEntry = _Rating.EqualizationInfo.GetOrAddEqualizationRateLineInfo(RateLineBizO);

					if (equalizationEntry.PivotValue == 0m)
					{
						calcOutput.FailureMessage = Res.GetString("1c44b1b3-0756-4923-99c5-965ce7336b0f", "pivot value was not specified for this Rate. Volume Equalization cannot be performed.");
						return;
					}

					equalizationEntry.AddConsolEqualizationInfo(equalizationUnitsCount, containerCount);
					calcOutput.IsEmpty = true;
					return;
				}

				var calcLog = calcOutput.CalculationLog;
				var equalizedCostInfo = _Rating.EqualizationInfo.GetEqualizedCostInfoForConsolPerRateLine(RateLineBizO.PK);

				if (equalizedCostInfo != null)
				{
					var results = new List<PaymentBasis>();

					if (!equalizedCostInfo.FlatRate.IsEmpty)
					{
						var flatRateInfo = RateInfo.CreateFLT(equalizedCostInfo.FlatRate, Line.TL_RX_NKCurrency);
						var flatPaymentBasis = criteria.CreatePaymentBasis(flatRateInfo, default);
						results.Add(flatPaymentBasis);
					}

					var perUnitRateInfo = RateInfo.CreateUNT(equalizedCostInfo.PerUnitRate, unit, Line.TL_RX_NKCurrency, UnitDescription(false), unitMultiplier: UnitMultiplier);
					var chargeable = new Quantity(equalizedCostInfo.UnitsOnConsol, unit);
					var unitDescription = UnitDescriptionInternal(unit, addPlural: true, addContainerCode: true);
					var perUnitResult = criteria.CreatePaymentBasis(perUnitRateInfo, chargeable, unitDescription);
					results.Add(perUnitResult);

					if (!equalizedCostInfo.FlatRate.IsEmpty)
					{
						calcLog.AddFlatAmountToLastCalculation(equalizedCostInfo.FlatRate);
					}

					if (perUnitResult.Amount != 0)
					{
						var step = calcLog.AddCalculationStep();
						step.Result = perUnitResult.Amount;
						step.Flat = equalizedCostInfo.FlatRate;
					}

					calcOutput.Add(results);
					return;
				}
			}

			calcOutput.IsEmpty = true;
		}

		(ZDecimal EqualisationUnitsCount, ZDecimal ContainerCount) GetEqualizationUnitsAndContainerCount(IRateLine line, AutoRatingCalculatorParameters parameters)
		{
			var measures = parameters.Criteria.JobMeasures;
			if (!measures.HasContainerMeasure)
			{
				return (0, 0);
			}

			var equalizationUnits = 0m;
			var containerTypePK = line.ParentRateEntry.Container.PK;
			var containersCount = measures.GetContainerCountForContainerType(containerTypePK);

			if (line.TL_WeightVolume == QuantityUnit.KG || line.TL_WeightVolume == QuantityUnit.M3)
			{
				var measureType = QuantityUnit.IsWeight(line.TL_WeightVolume) ? MeasureType.Weight : MeasureType.Volume;
				equalizationUnits = parameters.GetTotalMeasureValueForLine(line, measureType);
			}
			else if (line.TL_WeightVolume == QuantityUnit.TU)
			{
				equalizationUnits = measures.GetTotalTEUForContainerType(containerTypePK);
			}

			return (equalizationUnits, containersCount);
		}

		internal override bool TM_BreakEditable(RateLineItem item)
		{
			// TM_Break column is used to specify the calculator Pivot Break weight. It is entered on the first item, so, it has to be editable.
			var lineItems = item.Parent.Calculator.RateLineItems.Cast<IRateLineItem>().ToArray();
			return lineItems.Any() && item == lineItems[0];
		}

		public ZString ContractHint => this.RateLineItems.Count == 2
											? Res.GetString("a4b611b9-738d-4f55-87d2-1e3a5796628f", "Volume Discount is achieved if Average {0} across selected jobs reaches the Pivot Break, then the second rate is applied to the actual {0}. Otherwise the first rate is applied to the actual {0}.", UnitDescription(false))
											: Res.GetString("55e73480-cbd1-4e9f-8473-3e675bc89469", "Volume Discount is achieved if Average {0} across selected jobs reaches the Pivot Break, then the rate is applied to the actual job {0}. Otherwise the rate is applied to the Pivot Break {0} per job.", UnitDescription(false));

		public ZPropertyInfo ContractHintInfo => ((IGetZPropertyInfo)RateLineBizO).GetZPropertyInfo("Calculator+String1");

		#endregion
	}
}
