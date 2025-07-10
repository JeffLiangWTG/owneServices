using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using WTG.WebSecurityRight;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class WebSecurityRightsProviderTest : TestCase
	{
		public void TestAddAndRemove()
		{
			var right = new WebSecurityRight("First", (NoResString)"", WebSecurityApplication.EdiWebTracker);
			var rights = new WebSecurityRightsProviderForTest();

			AssertEquals("Nothing happened, Should be empty", 0, rights.Count);

			rights.Add(right);

			AssertEquals("Should have our added right", 1, rights.Count);
		}

		public void TestTryGetValue()
		{
			var right = new WebSecurityRight("First", (NoResString)"", WebSecurityApplication.EdiWebTracker);
			var rights = new WebSecurityRightsProviderForTest();

			rights.Add(right);

			WebSecurityRight otherRight;
			Assert("Key doesn't exist, should return false", !rights.TryGetValue("Second", out otherRight));
			AssertNull("Since key doesn't exist", otherRight);

			Assert("Key does exist, should find", rights.TryGetValue("First", out otherRight));
			AssertSame("Should find correct item", right, otherRight);
		}

		public void TestEnumerating()
		{
			var firstRightInDict = new WebSecurityRight("First", (NoResString)"", WebSecurityApplication.EdiWebTracker);
			var secondRightInDict = new WebSecurityRight("Second", (NoResString)"", WebSecurityApplication.EdiWebTracker);
			var notInDict = new WebSecurityRight("Third", (NoResString)"", WebSecurityApplication.EdiWebTracker);

			var rights = new WebSecurityRightsProviderForTest();
			rights.Add(firstRightInDict);
			rights.Add(secondRightInDict);

			// Use Linq so we actually test iteration
			AssertContainsExactElementsInAnyOrder(new[] { firstRightInDict, secondRightInDict }, rights);
		}
	}
}
