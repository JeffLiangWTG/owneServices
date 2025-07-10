using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class ChargeableAmountCalculatorTest : TestCaseWithFactory
	{
		#region Chargeable Units

		public void TestGetChargeableUnit()
		{
			List<string> listWeightUnits = new List<string>(Constants.Weight.Codes);
			List<string> listVolumeUnits = new List<string>(Constants.Volume.Codes);

			foreach (var transportMode in FreightDataRegistry.Instance.WeightChargableTransportModes)
			{
				foreach (var weightUnit in listWeightUnits)
				{
					foreach (var volumeUnit in listVolumeUnits)
					{
						bool isWeightImperial = Constants.Weight.IsImperial(weightUnit);
						bool isVolumeImperial = Constants.Volume.IsImperial(volumeUnit);

						string toTest = ChargeableAmountCalculator.GetChargeableUnit(transportMode, weightUnit, volumeUnit);

						string expected = !isWeightImperial || !isVolumeImperial ? Constants.Weight.Kilograms : Constants.Weight.Pounds;

						AssertEquals(string.Format("[{0}]; for weight in [{1}] and volume in [{2}] expected chargeable unit [{3}], received [{4}]",
							transportMode, weightUnit, volumeUnit, expected, toTest), toTest, expected);
					}
				}
			}

			foreach (var transportMode in FreightDataRegistry.Instance.VolumeChargableTransportModes)
			{
				foreach (var weightUnit in listWeightUnits)
				{
					foreach (var volumeUnit in listVolumeUnits)
					{
						bool isWeightImperial = Constants.Weight.IsImperial(weightUnit);
						bool isVolumeImperial = Constants.Volume.IsImperial(volumeUnit);

						string toTest = ChargeableAmountCalculator.GetChargeableUnit(transportMode, weightUnit, volumeUnit);

						string expected = !isWeightImperial || !isVolumeImperial ? Constants.Volume.CubicMetres : Constants.Volume.CubicFeet;

						AssertEquals(string.Format("[{0}]; for weight in [{1}] and volume in [{2}] expected chargeable unit [{3}], received [{4}]",
							transportMode, weightUnit, volumeUnit, expected, toTest), toTest, expected);
					}
				}
			}
		}

		public void TestSQL_IsUnitMetric()
		{
			List<string> errors = new List<string>();

			foreach (FieldInfo weightInfo in typeof(Weight).GetFields(BindingFlags.Public | BindingFlags.Static))
			{
				if (weightInfo.FieldType != typeof(string))
				{
					continue;
				}

				string unitOfWeight = (string)weightInfo.GetValue(null);

				foreach (FieldInfo volumeInfo in typeof(Volume).GetFields(BindingFlags.Public | BindingFlags.Static))
				{
					if (volumeInfo.FieldType != typeof(string))
					{
						continue;
					}

					string unitOfVolume = (string)volumeInfo.GetValue(null);

					string expected = ChargeableAmountCalculator.ChargeableUnitIsMetric(unitOfWeight, unitOfVolume) ? "Metric" : "Imperial";
					string actual = SQL_IsUnitMetric(unitOfWeight, unitOfVolume);

					if (actual != expected)
					{
						errors.Add(string.Format("Weight: {0} ({1}), Volume: {2} ({3}) - In code: {4}, but in sql: {5}",
							unitOfWeight, Constants.Weight.GetDescription(unitOfWeight, Constants.PluralState.Plural),
							unitOfVolume, Constants.Volume.GetDescription(unitOfVolume, Constants.PluralState.Plural),
							expected,
							actual));
					}
				}
			}

			Dictionary<string, IList<string>> results = new Dictionary<string, IList<string>>();
			if (errors.Count > 0)
			{
				results.Add("Missmatches", errors);
			}

			AssertGroupedErrorList(results);
		}

		public void TestSQL_GetChargeableUnit()
		{
			List<string> errors = new List<string>();

			foreach (FieldInfo transportInfo in typeof(TransportModes).GetFields(BindingFlags.Public | BindingFlags.Static))
			{
				if (transportInfo.FieldType != typeof(string))
				{
					continue;
				}

				string transportMode = (string)transportInfo.GetValue(null);

				foreach (FieldInfo weightInfo in typeof(Weight).GetFields(BindingFlags.Public | BindingFlags.Static))
				{
					if (weightInfo.FieldType != typeof(string))
					{
						continue;
					}

					string unitOfWeight = (string)weightInfo.GetValue(null);

					foreach (FieldInfo volumeInfo in typeof(Volume).GetFields(BindingFlags.Public | BindingFlags.Static))
					{
						if (volumeInfo.FieldType != typeof(string))
						{
							continue;
						}

						string unitOfVolume = (string)volumeInfo.GetValue(null);

						string expected = ChargeableAmountCalculator.GetChargeableUnit(transportMode, unitOfWeight, unitOfVolume);
						string actual = SQL_GetChargeableUnit(transportMode, unitOfWeight, unitOfVolume);

						if (actual != expected)
						{
							errors.Add(string.Format("Transport Mode: {0} ({1}), Weight: {2} ({3}), Volume: {4} ({5}) - In code: {6}, but in sql: {7}",
								transportMode, transportInfo,
								unitOfWeight, Constants.Weight.GetDescription(unitOfWeight, Constants.PluralState.Plural),
								unitOfVolume, Constants.Volume.GetDescription(unitOfVolume, Constants.PluralState.Plural),
								expected,
								actual));
						}
					}
				}
			}

			Dictionary<string, IList<string>> results = new Dictionary<string, IList<string>>();
			if (errors.Count > 0)
			{
				results.Add("Missmatches", errors);
			}

			AssertGroupedErrorList(results);
		}

		#endregion

		#region GetDefaultConversionFactors

		public void TestGetDefaultConversionFactors_ReturnDefaultFromTheRegistry()
		{
			using (FreightDataRegistry.Instance.InternationalChargeableFactorAir.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new ChargeableFactor(
				new ConversionFactor(150, Weight.Kilograms, Volume.CubicMetres),
				new ConversionFactor(100, Weight.Pounds, Volume.CubicInches))))
			{
				var factors = ChargeableAmountCalculator.GetDefaultConversionFactors(false, TransportModes.Air, Weight.Kilograms);
				AssertContainsExactElementsInAnyOrder(new[] { new ConversionFactor(150, Weight.Kilograms, Volume.CubicMetres) }, factors);

				factors = ChargeableAmountCalculator.GetDefaultConversionFactors(false, TransportModes.Air, Weight.Pounds);
				AssertContainsExactElementsInAnyOrder(new[] { new ConversionFactor(100, Weight.Pounds, Volume.CubicInches) }, factors);
			}
		}

		public void TestGetDefaultConversionFactors_TransportIsRoadAndRoadLoadingMetersDisabled_ReturnStandardFactorForLoadingMeters()
		{
			using (FreightDataRegistry.Instance.InternationalChargeableFactorRoad.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new ChargeableFactor(
				new ConversionFactor(150, Weight.Kilograms, Volume.CubicMetres),
				new ConversionFactor(100, Weight.Pounds, Volume.CubicInches))))
			{
				var factors = ChargeableAmountCalculator.GetDefaultConversionFactors(false, TransportModes.Road, Weight.Kilograms);
				AssertContainsExactElementsInAnyOrder(
					new[]
					{
						new ConversionFactor(150, Weight.Kilograms, Volume.CubicMetres),
						ConversionFactor.Standard.Metric.LoadingMeters
					},
					factors);

				factors = ChargeableAmountCalculator.GetDefaultConversionFactors(false, TransportModes.Road, Weight.Pounds);
				AssertContainsExactElementsInAnyOrder(
					new[]
					{
						new ConversionFactor(100, Weight.Pounds, Volume.CubicInches),
						ConversionFactor.Standard.Metric.LoadingMeters
					},
					factors);
			}
		}

		public void TestGetDefaultConversionFactors_TransportIsRoadAndRoadLoadingMetersEnabled_ReturnFactorFromTheRegistry()
		{
			using (FreightDataRegistry.Instance.EnableRoadLoadingMeters.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightDataRegistry.Instance.RoadLoadingMetersWeightPerLDM.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 1000m))
			using (FreightDataRegistry.Instance.InternationalChargeableFactorRoad.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new ChargeableFactor(
				new ConversionFactor(150, Weight.Kilograms, Volume.CubicMetres),
				new ConversionFactor(100, Weight.Pounds, Volume.CubicInches))))
			{
				var factors = ChargeableAmountCalculator.GetDefaultConversionFactors(false, TransportModes.Road, Weight.Kilograms);
				AssertContainsExactElementsInAnyOrder(
					new[]
					{
						new ConversionFactor(150, Weight.Kilograms, Volume.CubicMetres),
						new ConversionFactor(1000, Weight.Kilograms, LoadingLength.LoadingMeters),
					},
					factors);

				factors = ChargeableAmountCalculator.GetDefaultConversionFactors(false, TransportModes.Road, Weight.Pounds);
				AssertContainsExactElementsInAnyOrder(
					new[]
					{
						new ConversionFactor(100, Weight.Pounds, Volume.CubicInches),
						new ConversionFactor(1000, Weight.Kilograms, LoadingLength.LoadingMeters),
					},
					factors);
			}
		}

		#endregion

		#region CalculateChargeable

		public void TestCalculateChargeable()
		{
			var chargeableParams = new ChargeableParameters
			{
				TargetUnit = Volume.CubicMetres,
				ConversionFactors = new[]
				{
					new ConversionFactor(100, Weight.Kilograms, Volume.CubicMetres),
					new ConversionFactor(1000, Weight.Kilograms, LoadingLength.LoadingMeters),
					new ConversionFactor(10, Weight.Kilograms, Area.SquareCentimetre),
					new ConversionFactor(5, Weight.Kilograms, Length.Centimetres),
				},
			};

			chargeableParams.Weight = new Quantity(200 * 1000, Weight.Grams);
			var actualChargeable = ChargeableAmountCalculator.CalculateChargeable(chargeableParams);
			var expectedChargeable = new ChargeableCalculationResult(
				new Quantity(2, Volume.CubicMetres),    // 200 000 G -> 200 KG -> 2 M3 (100 KG/M3)
				new Quantity(0, Volume.CubicMetres),
				new Quantity(0, Volume.CubicMetres),
				new Quantity(2, Volume.CubicMetres));   // Weight chosen
			AssertEquals(expectedChargeable, actualChargeable);

			chargeableParams.Volume = new Quantity(3 * 1000 * 1000, Volume.CubicCentimeters);
			actualChargeable = ChargeableAmountCalculator.CalculateChargeable(chargeableParams);
			expectedChargeable = new ChargeableCalculationResult(
				new Quantity(2, Volume.CubicMetres),
				new Quantity(3, Volume.CubicMetres),    // 3 000 000 CC -> 3 M3
				new Quantity(0, Volume.CubicMetres),
				new Quantity(3, Volume.CubicMetres));   // Volume chosen
			AssertEquals(expectedChargeable, actualChargeable);

			chargeableParams.LoadingLength = new Quantity(0.5, LoadingLength.LoadingMeters);
			actualChargeable = ChargeableAmountCalculator.CalculateChargeable(chargeableParams);
			expectedChargeable = new ChargeableCalculationResult(
				new Quantity(2, Volume.CubicMetres),
				new Quantity(3, Volume.CubicMetres),
				new Quantity(5, Volume.CubicMetres),    // 5 LM -> 500 KG (100 KG/LM) -> 5 M3 (100 KG/M3)
				new Quantity(5, Volume.CubicMetres));   // Loading Length chosen
			AssertEquals(expectedChargeable, actualChargeable);

			chargeableParams.TargetUnit = Weight.Pounds;
			actualChargeable = ChargeableAmountCalculator.CalculateChargeable(chargeableParams);
			expectedChargeable = new ChargeableCalculationResult(
				new Quantity(440.924524369755m, Weight.Pounds),    // 200 000 G -> 440.92 LB (2.20 KG/LB)
				new Quantity(661.386786554633, Weight.Pounds),  // 3 000 000 CC -> 3 M3 (100 KG/M3) -> 300 KG -> 661.387 LB (2.20 KG/LB)
				new Quantity(1102.31131092439, Weight.Pounds),   // 5 LM -> 500 KG (100 KG/LM) -> 1102.31 LB (2.20 KG/LB)
				new Quantity(1102.311, Weight.Pounds));     // Loading Length chosen
			AssertEquals(expectedChargeable, actualChargeable);
		}

		public void TestCalculateChargeable_EnableRoadLoadingMeter_RoadLoadingMeterWeightPerLDMIsZero_LMChargeableShouldBeIgnore()
		{
			using (FreightDataRegistry.Instance.EnableRoadLoadingMeters.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightDataRegistry.Instance.RoadLoadingMetersWeightPerLDM.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0m))
			{
				var chargeableParams = new ChargeableParameters
				{
					ConversionFactors = ChargeableAmountCalculator.GetDefaultConversionFactors(false, TransportModes.Road, Weight.Kilograms),
					LoadingLength = new Quantity(3m, LoadingLength.LoadingMeters),
					TargetUnit = Weight.Kilograms,
					Volume = new ZVolume(0, Volume.CubicMetres),
					Weight = new ZWeight(10000, Weight.Kilograms)
				};

				var actualChargeable = ChargeableAmountCalculator.CalculateChargeable(chargeableParams);

				var expectedChargeable = new ChargeableCalculationResult(
					weight: new Quantity(10000, Weight.Kilograms),
					volume: new Quantity(0, Weight.Kilograms),
					loadingLength: new Quantity(0, Weight.Kilograms),
					chargeable: new Quantity(10000, Weight.Kilograms));
				AssertEquals("GIVEN  conversion factor for LM is 0, WHEN converting from other units to LM, THEN LM cost should be ignore", expectedChargeable, actualChargeable);
			}
		}

		#endregion

		#region GetActualFromChargeable

		public void TestGetActualFromChargeable_Volume_Domestic_Road_LM()
		{
			var chargeableFactor = new ChargeableFactor(ConversionFactor.Standard.Metric.LoadingMeters, ConversionFactor.Standard.Imperial.Sea);

			using (FreightDataRegistry.Instance.DomesticChargeableFactorRoad.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, chargeableFactor))
			{
				var volume = ChargeableAmountCalculator.GetActualFromChargeable(
					transportMode: TransportModes.Road,
					isDomesticFreight: true,
					chargeableWeight: new ZWeight(2000, Weight.Kilograms),
					actualWeight: new ZWeight(300, Weight.Kilograms),
					actualVolume: new ZVolume(0, Volume.CubicFeet));

				Assert("Cannot calculate actual volume from a factor containing Loading Meters", !volume.HasValue);
			}
		}

		public void TestGetActualFromChargeable_Volume_Domestic_Air()
		{
			var chargeableFactor = new ChargeableFactor(ConversionFactor.Standard.Metric.Air, ConversionFactor.Standard.Imperial.Air);

			using (FreightDataRegistry.Instance.DomesticChargeableFactorRoad.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, chargeableFactor))
			{
				var volume = ChargeableAmountCalculator.GetActualFromChargeable(
					transportMode: TransportModes.Air,
					isDomesticFreight: false,
					chargeableWeight: new ZWeight(2000, Weight.Pounds),
					actualWeight: new ZWeight(300, Weight.Kilograms),
					actualVolume: new ZVolume(0, Volume.CubicFeet));

				Assert("Actual volume was calculated", volume.HasValue);
				AssertEquals("Actual volume amount", 192.130m, volume.Value.Amount);
				AssertEquals("Actual volume amount", Volume.CubicFeet, volume.Value.Unit);
			}
		}

		public void TestGetActualFromChargeable_Weight_Domestic_Road_LM()
		{
			var chargeableFactor = new ChargeableFactor(ConversionFactor.Standard.Metric.LoadingMeters, ConversionFactor.Standard.Imperial.Sea);

			using (FreightDataRegistry.Instance.DomesticChargeableFactorRoad.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, chargeableFactor))
			{
				var weight = ChargeableAmountCalculator.GetActualFromChargeable(
					transportMode: TransportModes.Road,
					isDomesticFreight: true,
					chargeableVolume: new ZVolume(2, Volume.CubicMetres),
					actualWeight: new ZWeight(300, Weight.Kilograms),
					actualVolume: new ZVolume(0, Volume.CubicFeet));

				Assert(@"Cannot calculate actual weight from a factor containing Loading Meters.
Nb. this should not happen because ROA is weight chargeable and therefore it's chargeable should be a weight not volume", !weight.HasValue);
			}
		}

		public void TestGetActualFromChargeable_Weight_International_Sea()
		{
			var chargeableFactor = new ChargeableFactor(ConversionFactor.Standard.Metric.Sea, ConversionFactor.Standard.Imperial.Sea);

			using (FreightDataRegistry.Instance.DomesticChargeableFactorRoad.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, chargeableFactor))
			{
				var weight = ChargeableAmountCalculator.GetActualFromChargeable(
					transportMode: TransportModes.Sea,
					isDomesticFreight: false,
					chargeableVolume: new ZVolume(2, Volume.CubicFeet),
					actualWeight: new ZWeight(300, Weight.Kilograms),
					actualVolume: new ZVolume(1, Volume.CubicFeet));

				Assert("Actual weight was calculated", weight.HasValue);
				AssertEquals("Actual weight amount", 90.718m, weight.Value.Amount);
				AssertEquals("Actual weight amount", Weight.Kilograms, weight.Value.Unit);
			}
		}

		#endregion

		#region Implementation

		string SQL_IsUnitMetric(string unitOfWeight, string unitOfVolume)
		{
			const string sql = "select value from dbo.IsUnitMetric(@unitOfWeight, @unitOfVolume)";
			using (DbCommand command = Db.Connection.Command(sql)) // SQL function should repeat code logic
			{
				command.AddParameter("@unitOfWeight", SqlDbType.VarChar, unitOfWeight);
				command.AddParameter("@unitOfVolume", SqlDbType.VarChar, unitOfVolume);
				return (int)command.ExecuteScalar() == 0 ? "Imperial" : "Metric";
			}
		}

		string SQL_GetChargeableUnit(string transportMode, string unitOfWeight, string unitOfVolume)
		{
			const string sql = "select value from dbo.GetChargeableUnit(@transportMode, (select value from dbo.IsUnitMetric(@unitOfWeight, @unitOfVolume)))";
			using (DbCommand command = Db.Connection.Command(sql)) // SQL function should repeat code logic
			{
				command.AddParameter("@transportMode", SqlDbType.VarChar, transportMode);
				command.AddParameter("@unitOfWeight", SqlDbType.VarChar, unitOfWeight);
				command.AddParameter("@unitOfVolume", SqlDbType.VarChar, unitOfVolume);
				return (string)command.ExecuteScalar();
			}
		}

		#endregion
	}
}
