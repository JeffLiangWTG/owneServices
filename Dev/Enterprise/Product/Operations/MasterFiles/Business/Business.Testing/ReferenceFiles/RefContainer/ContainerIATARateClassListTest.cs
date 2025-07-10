using System.Linq;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class ContainerIATARateClassListTest : TestCase
	{
		public void TestGetFullList()
		{
			var list = new ContainerIATARateClassList();
			AssertEquals("List.Count", 37, list.Count);

			AssertEquals(@"1: Minimum Chargeable 5540 KG per ULD
1P: Minimum Chargeable 4935 KG per ULD
1S: 
2: Minimum Chargeable 2860 KG per ULD
2A: Minimum Chargeable 2610 KG per ULD
2AA: Minimum Chargeable 2390 KG per ULD
2B: Minimum Chargeable 2130 KG per ULD
2BG: Minimum Chargeable 1895 KG per ULD
2C: Minimum Chargeable 3220 KG per ULD
2D: Minimum Chargeable 2330 KG per ULD
2H: Minimum Chargeable 3525 KG per ULD
2Q: Minimum Chargeable 2750 KG per ULD
2R: Minimum Chargeable 2685 KG per ULD
2W: Minimum Chargeable 2265 KG per ULD
2WA: Minimum Chargeable 2500 KG per ULD
3: Minimum Chargeable 2100 KG per ULD
3A: Minimum Chargeable 2240 KG per ULD
4: Minimum Chargeable 1845 KG per ULD
4A: Minimum Chargeable 1700 KG per ULD
5: Minimum Chargeable 1630 KG per ULD
5A: Minimum Chargeable 1720 KG per ULD
5W: Minimum Chargeable 2170 KG per ULD
5WA: Minimum Chargeable 2125 KG per ULD
6: Minimum Chargeable 1155 KG per ULD
6A: Minimum Chargeable 1110 KG per ULD
6B: 
6W: Minimum Chargeable 1460 KG per ULD
7: Minimum Chargeable 1015 KG per ULD
7A: Minimum Chargeable 950 KG per ULD
8: Minimum Chargeable 710 KG per ULD
8A: Minimum Chargeable 590 KG per ULD
8B: Minimum Chargeable 800 KG per ULD
8C: Minimum Chargeable 550 KG per ULD
8D: Minimum Chargeable 565 KG per ULD
8F: Minimum Chargeable 765 KG per ULD
8G: Minimum Chargeable 565 KG per ULD
9: Minimum Chargeable 885 KG per ULD"
				, string.Join(System.Environment.NewLine, list.ToArray().Select(x => $"{x.Code}: {x.Description}")));
		}

		public void TestGetListForAWB()
		{
			var list = ContainerIATARateClassList.GetListForAWB();
			AssertEquals("List.Count", 35, list.Count);

			AssertEquals(@"1: Minimum Chargeable 5540 KG per ULD
1P: Minimum Chargeable 4935 KG per ULD
2: Minimum Chargeable 2860 KG per ULD
2A: Minimum Chargeable 2610 KG per ULD
2AA: Minimum Chargeable 2390 KG per ULD
2B: Minimum Chargeable 2130 KG per ULD
2BG: Minimum Chargeable 1895 KG per ULD
2C: Minimum Chargeable 3220 KG per ULD
2D: Minimum Chargeable 2330 KG per ULD
2H: Minimum Chargeable 3525 KG per ULD
2Q: Minimum Chargeable 2750 KG per ULD
2R: Minimum Chargeable 2685 KG per ULD
2W: Minimum Chargeable 2265 KG per ULD
2WA: Minimum Chargeable 2500 KG per ULD
3: Minimum Chargeable 2100 KG per ULD
3A: Minimum Chargeable 2240 KG per ULD
4: Minimum Chargeable 1845 KG per ULD
4A: Minimum Chargeable 1700 KG per ULD
5: Minimum Chargeable 1630 KG per ULD
5A: Minimum Chargeable 1720 KG per ULD
5W: Minimum Chargeable 2170 KG per ULD
5WA: Minimum Chargeable 2125 KG per ULD
6: Minimum Chargeable 1155 KG per ULD
6A: Minimum Chargeable 1110 KG per ULD
6W: Minimum Chargeable 1460 KG per ULD
7: Minimum Chargeable 1015 KG per ULD
7A: Minimum Chargeable 950 KG per ULD
8: Minimum Chargeable 710 KG per ULD
8A: Minimum Chargeable 590 KG per ULD
8B: Minimum Chargeable 800 KG per ULD
8C: Minimum Chargeable 550 KG per ULD
8D: Minimum Chargeable 565 KG per ULD
8F: Minimum Chargeable 765 KG per ULD
8G: Minimum Chargeable 565 KG per ULD
9: Minimum Chargeable 885 KG per ULD"
				, string.Join(System.Environment.NewLine, list.ToArray().Select(x => $"{x.Code}: {x.Description}")));
		}
	}
}
