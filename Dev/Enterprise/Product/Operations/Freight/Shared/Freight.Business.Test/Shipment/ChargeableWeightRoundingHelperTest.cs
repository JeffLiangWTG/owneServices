using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business.Testing
{
	sealed class ChargeableWeightRoundingHelperTest : TestCaseWithFactory
	{
		#region Rounding Tests

		public void TestGetRounding()
		{
			ChargeableWeightRoundingCollection registryEntry = FreightDataRegistry.Instance.FreightChargeableWeightRoundings.Value;
			AssertEquals("PreCondition: Rounding off by default", false, registryEntry[0].RoundingEnabled);
			AssertEquals("PreCondition: No rounding applied", 123.45M, ChargeableWeightRoundingHelper.GetRoundedValueAir(JobConsolSchema.JK_ConsolChargeable, 123.45M));

			registryEntry[0].RoundingMode = nameof(ChargeableWeightRoundingType.Down);
			registryEntry[0].RoundingScale = ChargeableWeightRoundingScales.Scale05;
			FreightDataRegistry.Instance.FreightChargeableWeightRoundings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryEntry);
			AssertEquals(12M, ChargeableWeightRoundingHelper.GetRoundedValueAir(JobConsolSchema.JK_ConsolChargeable, 12.32M));
			AssertEquals(12.5M, ChargeableWeightRoundingHelper.GetRoundedValueAir(JobConsolSchema.JK_ConsolChargeable, 12.67M));
			AssertEquals(13M, ChargeableWeightRoundingHelper.GetRoundedValueAir(JobConsolSchema.JK_ConsolChargeable, 13.27M));

			registryEntry[0].RoundingScale = ChargeableWeightRoundingScales.Scale10;
			FreightDataRegistry.Instance.FreightChargeableWeightRoundings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryEntry);
			AssertEquals(12M, ChargeableWeightRoundingHelper.GetRoundedValueAir(JobConsolSchema.JK_ConsolChargeable, 12.32M));
			AssertEquals(12M, ChargeableWeightRoundingHelper.GetRoundedValueAir(JobConsolSchema.JK_ConsolChargeable, 12.67M));
			AssertEquals(13M, ChargeableWeightRoundingHelper.GetRoundedValueAir(JobConsolSchema.JK_ConsolChargeable, 13.27M));

			registryEntry[0].RoundingMode = nameof(ChargeableWeightRoundingType.Up);
			FreightDataRegistry.Instance.FreightChargeableWeightRoundings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryEntry);
			AssertEquals(13M, ChargeableWeightRoundingHelper.GetRoundedValueAir(JobConsolSchema.JK_ConsolChargeable, 12.32M));
			AssertEquals(13M, ChargeableWeightRoundingHelper.GetRoundedValueAir(JobConsolSchema.JK_ConsolChargeable, 12.67M));
			AssertEquals(14M, ChargeableWeightRoundingHelper.GetRoundedValueAir(JobConsolSchema.JK_ConsolChargeable, 13.27M));

			registryEntry[0].RoundingScale = ChargeableWeightRoundingScales.Scale05;
			FreightDataRegistry.Instance.FreightChargeableWeightRoundings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryEntry);
			AssertEquals(12.5M, ChargeableWeightRoundingHelper.GetRoundedValueAir(JobConsolSchema.JK_ConsolChargeable, 12.32M));
			AssertEquals(13M, ChargeableWeightRoundingHelper.GetRoundedValueAir(JobConsolSchema.JK_ConsolChargeable, 12.67M));
			AssertEquals(13.5M, ChargeableWeightRoundingHelper.GetRoundedValueAir(JobConsolSchema.JK_ConsolChargeable, 13.27M));

			registryEntry[0].RoundingMode = nameof(ChargeableWeightRoundingType.None);
			FreightDataRegistry.Instance.FreightChargeableWeightRoundings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryEntry);
			AssertEquals(12.32M, ChargeableWeightRoundingHelper.GetRoundedValueAir(JobConsolSchema.JK_ConsolChargeable, 12.32M));
			AssertEquals(12.67M, ChargeableWeightRoundingHelper.GetRoundedValueAir(JobConsolSchema.JK_ConsolChargeable, 12.67M));
			AssertEquals(13.27M, ChargeableWeightRoundingHelper.GetRoundedValueAir(JobConsolSchema.JK_ConsolChargeable, 13.27M));
		}

		#endregion
	}
}
