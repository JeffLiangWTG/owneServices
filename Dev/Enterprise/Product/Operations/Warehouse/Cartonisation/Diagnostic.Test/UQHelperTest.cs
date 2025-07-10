using System.Linq;
using CargoWise.Common;
using Enterprise.Core;
using NUnit.Framework;

namespace Enterprise.Warehouse.Cartonisation.Diagnostic.Testing
{
	public class UQHelperTest : TestCase
	{
		public void TestVolumeUQList()
		{
			AssertEquals(Constants.Volume.Codes.Length, UQHelper.VolumeUQList.Count);
			Assert(UQHelper.VolumeUQList.Select(x => x.Code).ContainsSameElementsInAnyOrder(Constants.Volume.Codes));
			Assert(UQHelper.VolumeUQList.Select(x => x.Description).ContainsSameElementsInAnyOrder(Constants.Volume.Codes));
		}

		public void TestWeightUQList()
		{
			AssertEquals(Constants.Weight.Codes.Length, UQHelper.WeightUQList.Count);
			Assert(UQHelper.WeightUQList.Select(x => x.Code).ContainsSameElementsInAnyOrder(Constants.Weight.Codes));
			Assert(UQHelper.WeightUQList.Select(x => x.Description).ContainsSameElementsInAnyOrder(Constants.Weight.Codes));
		}

		public void TestDimensionUQList()
		{
			AssertEquals(Constants.Length.Codes.Length, UQHelper.DimensionUQList.Count);
			Assert(UQHelper.DimensionUQList.Select(x => x.Code).ContainsSameElementsInAnyOrder(Constants.Length.Codes));
			Assert(UQHelper.DimensionUQList.Select(x => x.Description).ContainsSameElementsInAnyOrder(Constants.Length.Codes));
		}
	}
}
