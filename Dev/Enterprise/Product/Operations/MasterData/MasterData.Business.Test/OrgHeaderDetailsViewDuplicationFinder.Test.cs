using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Glow.Model.Interfaces;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterData.Business.Tests
{
	public class OrgHeaderDetailsViewDuplicationFinderTest : TestCaseWithFactory
	{
		public void TestNotThrowExceptionWhenNKClosestPortIsInvalid()
		{
			var duplicateFinder = new OrgHeaderDetailsViewDuplicationFinderForTest(null, null);
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_RL_NKClosestPort = "T";
			orgHeader.OH_FullName = "TEST";

			(string, string) result = (null, null);
			AssertNoExceptionThrown(() => result = duplicateFinder.GenerateCacheSubkeyForTest(new DeduplicationOrgHeader(orgHeader)));
			AssertEquals("TEST", result.Item1);
			AssertEquals(string.Empty, result.Item2);

			orgHeader.OH_RL_NKClosestPort = null;
			AssertNoExceptionThrown(() => result = duplicateFinder.GenerateCacheSubkeyForTest(new DeduplicationOrgHeader(orgHeader)));
			AssertEquals("TEST", result.Item1);
			AssertEquals(string.Empty, result.Item2);

			orgHeader.OH_RL_NKClosestPort = "AUSYD";
			result = duplicateFinder.GenerateCacheSubkeyForTest(new DeduplicationOrgHeader(orgHeader));
			AssertEquals("TEST", result.Item1);
			AssertEquals("AU", result.Item2);
		}
	}

	#region Implementation

	public class OrgHeaderDetailsViewDuplicationFinderForTest : OrgHeaderDetailsViewDuplicationFinder
	{
		public OrgHeaderDetailsViewDuplicationFinderForTest(DeduplicationOrgHeader header, IEnumerable<DeduplicationOrgHeader> targetGlows, bool shouldUseCache = true) : base(header, targetGlows, shouldUseCache)
		{
		}

		public (string Part1, string Part2) GenerateCacheSubkeyForTest(IOrgHeader targetGlow)
		{
			return GenerateCacheSubkey(targetGlow);
		}
	}

	#endregion
}
