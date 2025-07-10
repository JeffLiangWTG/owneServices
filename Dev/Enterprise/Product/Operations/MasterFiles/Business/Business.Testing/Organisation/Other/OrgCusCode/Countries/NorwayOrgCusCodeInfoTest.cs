using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(NorwayOrgCusCodeInfo))]
	sealed class NorwayOrgCusCodeInfoTest : TestCaseWithFactory
	{
		public void TestOrgCusCodeInfoListContainsCodes()
		{
			var expectedCodes = new[] { "CTR", "EMD", "MVA", "ORG", "SSN" };
			var list = new OrgCodeLists().CustomsCodes_List(Enterprise.Core.Constants.CountryCodes.Norway);
			CombineAssertions(() =>
			{
				foreach (var code in expectedCodes)
				{
					Assert($"Code: {code} should be present", list.ContainsCode(code));
				}
			});
		}
	}
}
