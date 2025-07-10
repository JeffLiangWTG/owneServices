using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.Business
{
	#region Value Calculators - Apply To List

	public class ValueApplyToListCodeDescriptionPairList : CodeDescriptionPairList
	{
		protected ValueApplyToListCodeDescriptionPairList()
		{
		}

		public static ValueApplyToListCodeDescriptionPairList NewValueApplyToList(ZGuid countryPK)
		{
			var result = new ValueApplyToListCodeDescriptionPairList();

			result.AddPair(CalculatorConstants.Text.ApplyTo.Value.ValueOfGoods, CalculatorConstants.Text.ApplyTo.Value.Descriptions.ValueOfGoods);
			result.AddPair(CalculatorConstants.Text.ApplyTo.Value.InsuranceValue, CalculatorConstants.Text.ApplyTo.Value.Descriptions.InsuranceValue);
			result.AddPair(CalculatorConstants.Text.ApplyTo.Value.CustomsValue, CalculatorConstants.Text.ApplyTo.Value.Descriptions.CustomsValue);
			result.AddPair(CalculatorConstants.Text.ApplyTo.Value.InvoiceValue, CalculatorConstants.Text.ApplyTo.Value.Descriptions.InvoiceValue);

			if (countryPK == Core.CountryGuids.Instance.UnitedStates)
			{
				result.AddPair(CalculatorConstants.Text.ApplyTo.Value.SingleTransactionBondAmount, CalculatorConstants.Text.ApplyTo.Value.Descriptions.BondAmount);
			}

			return result;
		}

		public static ValueApplyToListCodeDescriptionPairList NewPercentageApplyToList(ZGuid countryPK, bool includeLoadingAndCustomsBrokerageCharges = false)
		{
			var result = new ValueApplyToListCodeDescriptionPairList();

			result.AddPair(CalculatorConstants.Text.ChargeCode, Calculator.Items.Value.ChargesApplyToTypesDescription.ChargeCodeDescription);
			result.AddPair(CalculatorConstants.Text.AllCharges, Calculator.Items.Value.ChargesApplyToTypesDescription.AllChargesDescription);
			result.AddPair(CalculatorConstants.Text.FreightCharges, Calculator.Items.Value.ChargesApplyToTypesDescription.FreightChargesDescription);
			result.AddPair(CalculatorConstants.Text.OriginCharges, Calculator.Items.Value.ChargesApplyToTypesDescription.OriginChargesDescription);
			result.AddPair(CalculatorConstants.Text.DestinationCharges, Calculator.Items.Value.ChargesApplyToTypesDescription.DestinationChargesDescription);

			if (includeLoadingAndCustomsBrokerageCharges)
			{
				result.AddPair(CalculatorConstants.Text.LoadingCharges, Calculator.Items.Value.ChargesApplyToTypesDescription.LoadingChargesDescription);
				result.AddPair(CalculatorConstants.Text.OriginCustomsBrokerageCharges, Calculator.Items.Value.ChargesApplyToTypesDescription.OriginCustomsBrokerageChargesDescription);
				result.AddPair(CalculatorConstants.Text.CustomsBrokerageCharges, Calculator.Items.Value.ChargesApplyToTypesDescription.CustomsBrokerageChargesDescription);
				result.AddPair(CalculatorConstants.Text.UnloadingCharges, Calculator.Items.Value.ChargesApplyToTypesDescription.UnloadingChargesDescription);
			}

			result.AddPair(Calculator.Items.Value.DisbursementApplyToTypes.Disbursements, Calculator.Items.Value.DisbursementApplyToTypesDescription.DisbursementsDescription);
			result.AddPair(Calculator.Items.Value.DisbursementApplyToTypes.CustomsDisbursement, Calculator.Items.Value.DisbursementApplyToTypesDescription.CustomsDisbursementDescription);
			//result.AddPair(Calculator.Items.Value.ChargesApplyToTypes.DestinationCharges, Calculator.Items.Value.ChargesApplyToTypesDescription.DestinationChargesDescription);

			result.AddRange(NewValueApplyToList(countryPK));

			result.AddPair(Calculator.Items.Value.CalculationOrder, Calculator.Items.Value.CalculationOrderDesc);

			return result;
		}

		public static ZString GetDescriptionFromCode(ZString applyTo, ZGuid countryPK)
		{
			var applyToList = ValueApplyToListCodeDescriptionPairList.NewPercentageApplyToList(countryPK, true);
			return applyToList.GetDescriptionFromCode(applyTo);
		}
	}

	#endregion

	#region Conversion Factors

	#region Conversion Factor List

	/// <summary>
	/// A reference list containing standard industry Volumetric Conversion Factor information.
	/// This includes both a value (conversion factor) and a description.
	/// </summary>
	public class ConversionFactorList : CodeDescriptionPairList
	{
		public static class Codes
		{
			public const string Custom = "CUSTOM";
		}

		public static class Descriptions
		{
			public static string Custom
			{
				get { return Res.GetString("d2c9f8df-de32-4a73-9fa7-4e77f21eabf8", "Custom"); }
			}
		}

		public ConversionFactorList(string[] targetUnits, UnitsSystem unitsSystem)
		{
			if (targetUnits.Length > 0)
			{
				var factors = ConversionFactor.Standard.All
					.Where(factor => targetUnits.Contains(factor.DenominatorUnit) || targetUnits.Contains(factor.NumeratorUnit))
					.Where(factor => factor.UnitsSystem == unitsSystem);

				foreach (var factor in factors)
				{
					AddPair(factor.ToString(), factor.Description);
				}
			}

			AddPair(Codes.Custom, Descriptions.Custom);
		}
	}

	#endregion

	#region Custom Conversion Factor

	public class CustomConversionFactor : NonPersistentBusinessObject, IObsoleteValidation
	{
		#region Constructor

		public CustomConversionFactor(RateLine line)
		{
			if (line != null)
			{
				Initialise(line, line.ConversionFactor.Factor);
			}
		}

		void Initialise(RateLine rateLine, ZDecimal conversionFactor)
		{
			if (rateLine == null)
			{
				return;
			}

			if (rateLine.TL_WeightVolume.IsEmpty)
			{
				return;
			}

			TargetUnit = rateLine.TL_WeightVolume;

			var items = rateLine.RateLineItems.Cast<RateLineItem>().ToArray();
			var unitItem = items.FirstOrDefault(x => x.RateOperatorIsUNT())
				?? items.FirstOrDefault(x => x.IsLowestBreak())
				?? items.FirstOrDefault(x => x.RateOperatorIsFirst());

			if (unitItem == null)
			{
				return;
			}

			var perUnit = new Quantity(unitItem.TM_RelevantValue, rateLine.TL_WeightVolume);
			if (perUnit.IsWeight())
			{
				weightPrice = perUnit.Amount;
				weightUnit = perUnit.Unit;
				var weightK = new Quantity(1, WeightUnit);
				ZDecimal weightPriceInKG = perUnit.Amount / weightK.AmountFor(RatingConstants.Units.KG).Amount; //TODO replace with operator overload
				volumePrice = weightPriceInKG * conversionFactor;
			}
			else if (perUnit.IsVolume())
			{
				volumePrice = perUnit.Amount;
				volumeUnit = perUnit.Unit;
				if (!conversionFactor.IsEmpty)
				{
					var volumeK = new Quantity(1, VolumeUnit);
					ZDecimal volumePriceInM3 = perUnit.Amount / volumeK.AmountFor(RatingConstants.Units.M3).Amount; //TODO replace with operator overload
					weightPrice = volumePriceInM3 / conversionFactor;
				}
			}
		}

		#endregion

		#region Properties

		string TargetUnit { get; set; }

		#region Weight Price

		public ZDecimal WeightPrice
		{
			get { return weightPrice; }
			set
			{
				SetNonPersistentPropertyValue(WeightPriceInfo, ref weightPrice, value);
				ConversionFactorInfo.RefreshBinding();
			}
		}

		ZDecimal weightPrice;

		public ZPropertyInfo WeightPriceInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(nameof(WeightPrice)); }
		}

		#endregion

		#region Weight Unit
		[List("Weights")]
		[MaxLength(2)]
		public ZString WeightUnit
		{
			get { return weightUnit; }
			set
			{
				CheckMaximumLength(WeightUnitInfo, value);
				SetNonPersistentPropertyValue(WeightUnitInfo, ref weightUnit, value);
				ConversionFactorInfo.RefreshBinding();
			}
		}

		ZString weightUnit = RatingConstants.Units.KG;

		public ZPropertyInfo WeightUnitInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(nameof(WeightUnit)); }
		}

		#endregion

		#region Volume Price

		public ZDecimal VolumePrice
		{
			get { return volumePrice; }
			set
			{
				SetNonPersistentPropertyValue(VolumePriceInfo, ref volumePrice, value);
				ConversionFactorInfo.RefreshBinding();
			}
		}

		ZDecimal volumePrice;

		public ZPropertyInfo VolumePriceInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(nameof(VolumePrice)); }
		}

		#endregion

		#region Volume Unit

		[List("Volumes")]
		[MaxLength(2)]
		public ZString VolumeUnit
		{
			get { return volumeUnit; }
			set
			{
				CheckMaximumLength(VolumeUnitInfo, value);
				SetNonPersistentPropertyValue(VolumeUnitInfo, ref volumeUnit, value);
				ConversionFactorInfo.RefreshBinding();
			}
		}

		ZString volumeUnit = RatingConstants.Units.M3;

		public ZPropertyInfo VolumeUnitInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(nameof(VolumeUnit)); }
		}

		#endregion

		#region Conversion Factor

		public ZString ConversionFactor
		{
			get
			{
				if (WeightPrice.IsEmpty || VolumePrice.IsEmpty)
				{
					return ZString.Empty;
				}

				ConversionFactor normal, flip;
				if (Constants.Volume.Codes.Contains(TargetUnit))
				{
					normal = new ConversionFactor(VolumePrice / WeightPrice, WeightUnit, VolumeUnit);
					flip = new ConversionFactor(WeightPrice / VolumePrice, VolumeUnit, WeightUnit);
				}
				else
				{
					normal = new ConversionFactor(WeightPrice / VolumePrice, VolumeUnit, WeightUnit);
					flip = new ConversionFactor(VolumePrice / WeightPrice, WeightUnit, VolumeUnit);
				}

				if (((ZDecimal)normal.Factor).DecimalPlaces > 2)
				{
					if (((ZDecimal)flip.Factor).DecimalPlaces < 3)
					{
						return flip.ToShortString();
					}

					return normal.Factor > flip.Factor ? normal.ToShortString() : flip.ToShortString();
				}

				return normal.ToShortString();
			}
		}

		public ZPropertyInfo ConversionFactorInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(nameof(ConversionFactor)); }
		}

		#endregion

		#endregion

		#region Lookups

		#region Weights

		public CodeDescriptionPairList Weights
		{
			get { return fWeights ?? (fWeights = new CodeDescriptionPairList(OLookUpEditType.Weight)); }
		}

		CodeDescriptionPairList fWeights;

		#endregion

		#region Volumes

		public CodeDescriptionPairList Volumes
		{
			get { return fVolumes ?? (fVolumes = new CodeDescriptionPairList(OLookUpEditType.Volume)); }
		}

		CodeDescriptionPairList fVolumes;

		#endregion

		#endregion
	}

	#endregion

	#endregion
}

