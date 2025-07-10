using System.Linq;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.PortMessaging.Business.Testing
{
	sealed class ExemptionReasonListTest : TestCase
	{
		public void TestList_EntryTypes()
		{
			var list = new ExemptionReasonList(EntryTypeList.Codes.Message);
			AssertContainsExactElementsInAnyOrder(new[] { "0", "2", "3" }, list.Cast<CodeDescriptionPair>().Select(x => x.Code));

			var list2 = new ObsoleteExemptionReasonsList(EntryTypeList.Codes.Message);
			AssertContainsExactElementsInAnyOrder(new[] { "4", "6" }, list2.Cast<CodeDescriptionPair>().Select(x => x.Code));

			list = new ExemptionReasonList(EntryTypeList.Codes.OtherExemptions);
			AssertContainsExactElementsInAnyOrder(new[] { "V", "L", "O" }, list.Cast<CodeDescriptionPair>().Select(x => x.Code));

			list = new ExemptionReasonList("XYZ");
			AssertEquals(0, list.Count);
		}
	}
}
