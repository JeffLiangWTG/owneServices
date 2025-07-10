using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class UtcOffsetUtilsTest : TestCaseWithFactory
	{
		public void TestGetUtcOffsets()
		{
			var utcOffsetList = GetUtcOffsetList();
			var utcOffsetUtils = new UtcOffsetUtils(utcOffsetList);
			void AssertUtcOffsets(ZShort actualMin, ZShort actualMax, ZShort expectedMin, ZShort expectedMax, int expectedLength, ZShort[] expectedWrappedUtcOffsets = null)
			{
				var actual = utcOffsetUtils.GetUtcOffsets(actualMin, actualMax);
				AssertEquals(expectedMin, actual.Min());
				AssertEquals(expectedMax, actual.Max());
				AssertEquals(expectedLength, actual.Length);

				if (expectedWrappedUtcOffsets != null)
				{
					var actualWithWrappedUtcOffets = actual.Intersect(expectedWrappedUtcOffsets);
					AssertContainsExactElementsInAnyOrder(expectedWrappedUtcOffsets, actualWithWrappedUtcOffets);

					var actualWithoutWrappedUtcOffets = actual.Except(expectedWrappedUtcOffsets);
					AssertEquals(actualMin, actualWithoutWrappedUtcOffets.Min());
					AssertEquals(actualMax, actualWithoutWrappedUtcOffets.Max());
				}
			}

			AssertUtcOffsets(-300, 300, -300, 300, 41);
			AssertUtcOffsets(-660, 840, -660, 840, 101);
			AssertUtcOffsets(0, 840, -660, 840, 62, new ZShort[] { -660, -645, -630, -615, -600 });
			AssertUtcOffsets(-660, 0, -660, 840, 50, new ZShort[] { 780, 795, 810, 825, 840 });
		}

		ZShort[] GetUtcOffsetList()
		{
			var utcOffsetList = new List<ZShort>();
			for (short i = -660; i <= 840; i += 15)
			{
				utcOffsetList.Add(i);
			}
			return utcOffsetList.ToArray();
		}
	}
}
