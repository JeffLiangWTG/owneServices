using CargoWise.EntityFramework.Testing;
using ISelectionItem = Enterprise.Customs.ASYCUDA.Business.ISelectionItem;

namespace Enterprise.Customs.SG.Access.Business.Testing
{
	sealed class MessageChooserLookupsTest : BusinessObjectValidationTestCase
	{
		public void TestCycleNumbers()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ManifestType = Constants.ManifestType.Import;
			var bill1 = header.Bills.AddNew();
			var bill2 = header.Bills.AddNew();
			var chooser = new MessageChooser(header, new ISelectionItem[] { bill1, bill2 }, true);
			AssertEquals(@"1 - 23:40
2 - 00:25
3 - 01:10
4 - 01:55
5 - 02:40
6 - 03:25
7 - 04:10
8 - 04:55
9 - 05:40
10 - 06:25
11 - 07:10
12 - 07:55
13 - 08:40
14 - 09:25
15 - 10:10
16 - 10:55
17 - 11:40
18 - 12:25
19 - 13:10
20 - 13:55
21 - 14:40
22 - 15:25
23 - 16:10
24 - 16:55
25 - 17:40
26 - 18:25
27 - 19:10
28 - 19:55
29 - 20:40
30 - 21:25
31 - 22:10
32 - 22:55", chooser.Lookups.CycleNumbers.ElementsAsString);
		}
	}
}
