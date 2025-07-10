using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.AWB.Business.Testing
{
	sealed class ChargeableWeightRoundingHelperTest : TestCaseWithFactory
	{
		public void TestGetAWBRoundingValueUpToWholeNumber()
		{
			AWBRoundingCollection collection = FreightDataRegistry.Instance.AWBRoundings.Value;
			collection[AWBRounding.Keys.House].RoundingMode = nameof(ChargeableWeightRoundingType.Up);
			collection[AWBRounding.Keys.House].RoundingScale = ChargeableWeightRoundingScales.Scale10;
			FreightDataRegistry.Instance.AWBRoundings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			AssertEquals(13M, AWBRoundingHelper.GetAWBRoundedValue(ExportAWBHeader.TypeOfAWB.House, 12.65M));
			AssertEquals(13M, AWBRoundingHelper.GetAWBRoundedValue(ExportAWBHeader.TypeOfAWB.House, 12.01M));
			AssertEquals(13M, AWBRoundingHelper.GetAWBRoundedValue(ExportAWBHeader.TypeOfAWB.House, 12.32M));
			AssertEquals(12M, AWBRoundingHelper.GetAWBRoundedValue(ExportAWBHeader.TypeOfAWB.House, 12M));
		}

		public void TestGetAWBRoundingValueUpToNextHalf()
		{
			AWBRoundingCollection collection = FreightDataRegistry.Instance.AWBRoundings.Value;
			collection[AWBRounding.Keys.MasterHouse].RoundingMode = nameof(ChargeableWeightRoundingType.Up);
			collection[AWBRounding.Keys.MasterHouse].RoundingScale = ChargeableWeightRoundingScales.Scale05;
			FreightDataRegistry.Instance.AWBRoundings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			AssertEquals(12.5M, AWBRoundingHelper.GetAWBRoundedValue(ExportAWBHeader.TypeOfAWB.MasterHouse, 12.32M));
			AssertEquals(12.5M, AWBRoundingHelper.GetAWBRoundedValue(ExportAWBHeader.TypeOfAWB.MasterHouse, 12.5M));
			AssertEquals(13M, AWBRoundingHelper.GetAWBRoundedValue(ExportAWBHeader.TypeOfAWB.MasterHouse, 12.64M));
		}

		public void TestGetAWBRoundingValueDownToWholeNumber()
		{
			AWBRoundingCollection collection = FreightDataRegistry.Instance.AWBRoundings.Value;
			collection[AWBRounding.Keys.House].RoundingMode = nameof(ChargeableWeightRoundingType.Down);
			collection[AWBRounding.Keys.House].RoundingScale = ChargeableWeightRoundingScales.Scale10;
			FreightDataRegistry.Instance.AWBRoundings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			AssertEquals(12M, AWBRoundingHelper.GetAWBRoundedValue(ExportAWBHeader.TypeOfAWB.House, 12.32M));
			AssertEquals(12M, AWBRoundingHelper.GetAWBRoundedValue(ExportAWBHeader.TypeOfAWB.House, 12.79M));
			AssertEquals(12M, AWBRoundingHelper.GetAWBRoundedValue(ExportAWBHeader.TypeOfAWB.House, 12M));
		}

		public void TestGetAWBRoundingValueDownToNextHalf()
		{
			AWBRoundingCollection collection = FreightDataRegistry.Instance.AWBRoundings.Value;
			collection[AWBRounding.Keys.MasterHouse].RoundingMode = nameof(ChargeableWeightRoundingType.Down);
			collection[AWBRounding.Keys.MasterHouse].RoundingScale = ChargeableWeightRoundingScales.Scale05;
			FreightDataRegistry.Instance.AWBRoundings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			AssertEquals(12M, AWBRoundingHelper.GetAWBRoundedValue(ExportAWBHeader.TypeOfAWB.MasterHouse, 12.32M));
			AssertEquals(12.5M, AWBRoundingHelper.GetAWBRoundedValue(ExportAWBHeader.TypeOfAWB.MasterHouse, 12.67M));
			AssertEquals(12.5M, AWBRoundingHelper.GetAWBRoundedValue(ExportAWBHeader.TypeOfAWB.MasterHouse, 12.5M));
			AssertEquals(12.5M, AWBRoundingHelper.GetAWBRoundedValue(ExportAWBHeader.TypeOfAWB.MasterHouse, 12.99M));
			AssertEquals(13M, AWBRoundingHelper.GetAWBRoundedValue(ExportAWBHeader.TypeOfAWB.MasterHouse, 13M));
		}
	}
}
