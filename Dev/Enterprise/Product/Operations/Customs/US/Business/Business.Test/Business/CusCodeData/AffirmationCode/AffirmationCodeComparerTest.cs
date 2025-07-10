using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class AffirmationCodeComparerTest : TestCaseWithFactory
	{
		public void TestPNCComesFirst()
		{
			List<KeyValuePair<ZString, ZString>> affirmationCodes = new List<KeyValuePair<ZString, ZString>>();
			affirmationCodes.Add(new KeyValuePair<ZString, ZString>("CCC", ""));
			affirmationCodes.Add(new KeyValuePair<ZString, ZString>(AffirmationCodeConstants.Codes.PNC, ""));
			affirmationCodes.Add(new KeyValuePair<ZString, ZString>("AAA", ""));
			affirmationCodes.Sort(new AffirmationCodeComparer());
			AssertEquals("First element should be pnc", AffirmationCodeConstants.Codes.PNC, affirmationCodes[0].Key);
			AssertEquals("First element should be aaa", "AAA", affirmationCodes[1].Key);
			AssertEquals("First element should be ccc", "CCC", affirmationCodes[2].Key);
		}

		public void TestSLNComesFirst()
		{
			List<KeyValuePair<ZString, ZString>> affirmationCodes = new List<KeyValuePair<ZString, ZString>>();
			affirmationCodes.Add(new KeyValuePair<ZString, ZString>("CCC", ""));
			affirmationCodes.Add(new KeyValuePair<ZString, ZString>(AffirmationCodeConstants.Codes.SLN, ""));
			affirmationCodes.Add(new KeyValuePair<ZString, ZString>("AAA", ""));
			affirmationCodes.Sort(new AffirmationCodeComparer());
			AssertEquals("First element should be pnc", AffirmationCodeConstants.Codes.SLN, affirmationCodes[0].Key);
			AssertEquals("First element should be aaa", "AAA", affirmationCodes[1].Key);
			AssertEquals("First element should be ccc", "CCC", affirmationCodes[2].Key);
		}

		public void TestPNDComesFirst()
		{
			List<KeyValuePair<ZString, ZString>> affirmationCodes = new List<KeyValuePair<ZString, ZString>>();
			affirmationCodes.Add(new KeyValuePair<ZString, ZString>("CCC", ""));
			affirmationCodes.Add(new KeyValuePair<ZString, ZString>(AffirmationCodeConstants.Codes.PND, ""));
			affirmationCodes.Add(new KeyValuePair<ZString, ZString>("AAA", ""));
			affirmationCodes.Sort(new AffirmationCodeComparer());
			AssertEquals("First element should be pnc", AffirmationCodeConstants.Codes.PND, affirmationCodes[0].Key);
			AssertEquals("First element should be aaa", "AAA", affirmationCodes[1].Key);
			AssertEquals("First element should be ccc", "CCC", affirmationCodes[2].Key);
		}
	}
}
