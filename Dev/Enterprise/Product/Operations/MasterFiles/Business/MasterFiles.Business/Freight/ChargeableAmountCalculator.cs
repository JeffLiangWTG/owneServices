using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class ChargeableCalculationResult
	{
		public ChargeableCalculationResult(IQuantity weight, IQuantity volume, IQuantity loadingLength, IQuantity chargeable)
		{
			Weight = weight;
			Volume = volume;
			LoadingLength = loadingLength;
			Chargeable = chargeable;
		}

		/// <summary>
		///		Gets weight chargeable amount in <see cref="ChargeableParameters.TargetUnit"/>.
		/// </summary>
		public IQuantity Weight { get; private set; }

		/// <summary>
		///		Gets volume chargeable amount in <see cref="ChargeableParameters.TargetUnit"/>.
		/// </summary>
		public IQuantity Volume { get; private set; }

		/// <summary>
		///		Gets loading length chargeable amount in <see cref="ChargeableParameters.TargetUnit"/>.
		/// </summary>
		public IQuantity LoadingLength { get; private set; }

		/// <summary>
		///		Gets chargeable amount which is the highest amount among <see cref="Weight"/>, <see cref="Volume"/> and <see cref="LoadingLength"/>.
		/// </summary>
		public IQuantity Chargeable { get; private set; }

		public override bool Equals(object obj)
		{
			var other = obj as ChargeableCalculationResult;
			if (other == null)
			{
				return false;
			}

			return Weight.Amount == other.Weight.Amount && Weight.Unit == other.Weight.Unit
				&& Volume.Amount == other.Volume.Amount && Weight.Unit == other.Volume.Unit
				&& LoadingLength.Amount == other.LoadingLength.Amount && Weight.Unit == other.LoadingLength.Unit
				&& Chargeable.Amount == other.Chargeable.Amount && Weight.Unit == other.Chargeable.Unit;
		}

		public override int GetHashCode()
		{
			return Chargeable.GetHashCode();
		}

		public override string ToString()
		{
			return ZString.Format((NoResString)"Weight: {0}, Volume: {1}, LoadingLength: {2}, Chargeable: {5}", Weight, Volume, LoadingLength, Chargeable);
		}
	}

	public class ChargeableParameters
	{
		public ChargeableParameters()
		{
			RoundingScale = ChargeableAmountCalculator.DefaultRoundingScale;
		}

		public IQuantity Weight { get; set; }
		public IQuantity Volume { get; set; }
		public IQuantity LoadingLength { get; set; }

		public string TargetUnit { get; set; }
		public int RoundingScale { get; set; }
		public IEnumerable<ConversionFactor> ConversionFactors { get; set; }
	}

	/// <summary>
	/// Calculates the Chargeable amount based on an Actual Weight and Volume and Conversion Factor.
	/// Conversion Factor can either be supplied, or if not, retrieved from the system registry.
	/// </summary>
	public static class ChargeableAmountCalculator
	{
		public const int DefaultRoundingScale = 3;

		#region Public Conversion Methods

		public static ChargeableCalculationResult CalculateChargeable(ChargeableParameters parameters)
		{
			var weightInTargetUnit = Convert(parameters.Weight, parameters.TargetUnit, parameters.ConversionFactors);
			var volumeInTargetUnit = Convert(parameters.Volume, parameters.TargetUnit, parameters.ConversionFactors);
			var loadingMetersInTargetUnit = Convert(parameters.LoadingLength, parameters.TargetUnit, parameters.ConversionFactors);

			var chargeableAmount = Enumerable.Max(new[] { weightInTargetUnit.Amount, volumeInTargetUnit.Amount, loadingMetersInTargetUnit.Amount });
			var chargeableAmountRounded = Utilities.Round(chargeableAmount, parameters.RoundingScale);
			var chargeable = new Quantity(chargeableAmountRounded, parameters.TargetUnit);

			return new ChargeableCalculationResult(weightInTargetUnit, volumeInTargetUnit, loadingMetersInTargetUnit, chargeable);
		}

		public static IQuantity Convert(IQuantity amount, ZString targetUnit, IEnumerable<ConversionFactor> factors)
		{
			if (amount == null || amount.IsEmpty || !amount.IsValid || amount.Amount.IsEmpty)
			{
				return new Quantity(0m, targetUnit);
			}

			var convertedValue = amount.Convert(targetUnit, factors.ToArray());
			return convertedValue ?? new Quantity(0, targetUnit);
		}

		public static IEnumerable<ConversionFactor> GetDefaultConversionFactors(IJobInvoicingSupporter jobInvoicingSupporter, string targetUnit)
		{
			var chargeableSource = jobInvoicingSupporter is IJobInvoicingSupporterWithChargeableFactorSource jobInvoicingSupporterWithChargeableFactorSource
				? jobInvoicingSupporterWithChargeableFactorSource.ChargeableFactorSource
				: jobInvoicingSupporter.IsDomestic
					? ChargeableFactorSource.Domestic
					: ChargeableFactorSource.International;

			return GetDefaultConversionFactors(chargeableSource, jobInvoicingSupporter.TransportMode, targetUnit);
		}

		public static IEnumerable<ConversionFactor> GetDefaultConversionFactors(bool isDomestic, string transportMode, string targetUnit)
		{
			var chargeableSource = isDomestic
				? ChargeableFactorSource.Domestic
				: ChargeableFactorSource.International;
			return GetDefaultConversionFactors(chargeableSource, transportMode, targetUnit);
		}

		public static IEnumerable<ConversionFactor> GetDefaultConversionFactors(ChargeableFactorSource chargeableSource, string transportMode, string targetUnit)
		{
			var targetUnitSystem = Constants.Weight.IsImperial(targetUnit) || Constants.Volume.IsImperial(targetUnit)
				? UnitsSystem.Imperial
				: UnitsSystem.Metric;

			var factors = new List<ConversionFactor>();

			var chargeableFactor = ChargeableFactor.GetDefault(chargeableSource, transportMode);
			if (chargeableFactor != null)
			{
				var factor = targetUnitSystem == UnitsSystem.Imperial ? chargeableFactor.ImperialFactor : chargeableFactor.MetricFactor;
				if (!factor.IsEmpty)
				{
					factors.Add(factor);
				}
			}

			if (transportMode == Constants.TransportModes.Road)
			{
				if (FreightDataRegistry.Instance.EnableRoadLoadingMeters.Value)
				{
					factors.Add(new ConversionFactor(FreightDataRegistry.Instance.RoadLoadingMetersWeightPerLDM.Value, Constants.Weight.Kilograms, Constants.LoadingLength.LoadingMeters));
				}
				else
				{
					factors.Add(ConversionFactor.Standard.Metric.LoadingMeters);
				}
			}

			return factors;
		}

		#endregion

		#region Chargeable Units

		public static string GetChargeableUnit(string transportMode, string unitOfWeight, string unitOfVolume)
		{
			bool useMetricUnit = ChargeableUnitIsMetric(unitOfWeight, unitOfVolume);

			return FreightDataRegistry.Instance.WeightChargableTransportModes.Contains(transportMode) ?
				FreightDataRegistry.Instance.GetDefaultChargeableWeightUnit(useMetricUnit) :
				FreightDataRegistry.Instance.GetDefaultChargeableVolumeUnit(useMetricUnit);
		}

		public static bool ChargeableUnitIsMetric(string unitOfWeight, string unitOfVolume)
		{
			return !Constants.Weight.IsImperial(unitOfWeight) || !Constants.Volume.IsImperial(unitOfVolume);
		}

		#endregion

		#region Actual From Chargeable

		public static ZVolume? GetActualFromChargeable(ZString transportMode, bool isDomesticFreight, ZWeight chargeableWeight, ZWeight actualWeight, ZVolume actualVolume)
		{
			if (actualWeight >= chargeableWeight)
			{
				return null;
			}

			ChargeableFactor chargeableFactor = ChargeableFactor.GetDefault(isDomesticFreight ? ChargeableFactorSource.Domestic : ChargeableFactorSource.International, transportMode);

			if (chargeableFactor == null)
			{
				return null;
			}

			bool isImperial = Constants.Weight.IsImperial(chargeableWeight.Unit);

			var factor = isImperial
				? chargeableFactor.ImperialFactor
				: chargeableFactor.MetricFactor;

			if (factor.DenominatorUnit == Constants.LoadingLength.LoadingMeters)
			{
				return null;
			}

			var volume = factor.Convert(chargeableWeight);

			decimal convertedVolumeAmount = Utilities.Round(Constants.Volume.Convert(volume.Amount, volume.Unit, actualVolume.Unit, false), DefaultRoundingScale);

			return new ZVolume(convertedVolumeAmount, actualVolume.Unit);
		}

		public static ZWeight? GetActualFromChargeable(ZString transportMode, bool isDomesticFreight, ZVolume chargeableVolume, ZWeight actualWeight, ZVolume actualVolume)
		{
			if (actualVolume >= chargeableVolume)
			{
				return null;
			}

			ChargeableFactor chargeableFactor = ChargeableFactor.GetDefault(isDomesticFreight ? ChargeableFactorSource.Domestic : ChargeableFactorSource.International, transportMode);

			if (chargeableFactor == null)
			{
				return null;
			}

			bool isImperial = Constants.Volume.IsImperial(chargeableVolume.Unit);

			var factor = isImperial
				? chargeableFactor.ImperialFactor
				: chargeableFactor.MetricFactor;

			if (factor.DenominatorUnit == Constants.LoadingLength.LoadingMeters)
			{
				return null;
			}

			var weight = factor.Convert(chargeableVolume);
			decimal convertedweightAmount = Utilities.Round(Constants.Weight.Convert(weight.Amount, weight.Unit, actualWeight.Unit, false), DefaultRoundingScale);

			return new ZWeight(convertedweightAmount, actualWeight.Unit);
		}

		#endregion
	}
}
