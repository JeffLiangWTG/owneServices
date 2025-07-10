using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class CertaintyLikertItemHelperTest : TestCase
	{
		#region ToItem

		public void TestToItem()
		{
			CombineAssertions(() =>
			{
				AssertToItemCode(CertaintyLikertItemList.Codes._1ExtremelyUnlikely, 0);
				AssertToItemCode(CertaintyLikertItemList.Codes._1ExtremelyUnlikely, 19);

				AssertToItemCode(CertaintyLikertItemList.Codes._2Unlikely, 20);
				AssertToItemCode(CertaintyLikertItemList.Codes._2Unlikely, 25);
				AssertToItemCode(CertaintyLikertItemList.Codes._2Unlikely, 39);

				AssertToItemCode(CertaintyLikertItemList.Codes._3Neutral, 40);
				AssertToItemCode(CertaintyLikertItemList.Codes._3Neutral, 50);
				AssertToItemCode(CertaintyLikertItemList.Codes._3Neutral, 59);

				AssertToItemCode(CertaintyLikertItemList.Codes._4Likely, 60);
				AssertToItemCode(CertaintyLikertItemList.Codes._4Likely, 75);
				AssertToItemCode(CertaintyLikertItemList.Codes._4Likely, 79);

				AssertToItemCode(CertaintyLikertItemList.Codes._5ExtremelyLikely, 80);
				AssertToItemCode(CertaintyLikertItemList.Codes._5ExtremelyLikely, 100);
				AssertToItemCode(CertaintyLikertItemList.Codes._5ExtremelyLikely, 110);
			});
		}

		void AssertToItemCode(string expectedItemCode, ZByte percentage)
		{
			AssertEquals(percentage.ToString(), expectedItemCode, CertaintyLikertItemHelper.ToItem(percentage).Code);
		}

		#endregion

		#region ToPercentage

		public void TestToPercentage()
		{
			CombineAssertions(() =>
			{
				AssertToPercentage(0, CertaintyLikertItemList.Codes._1ExtremelyUnlikely);
				AssertToPercentage(25, CertaintyLikertItemList.Codes._2Unlikely);
				AssertToPercentage(50, CertaintyLikertItemList.Codes._3Neutral);
				AssertToPercentage(75, CertaintyLikertItemList.Codes._4Likely);
				AssertToPercentage(100, CertaintyLikertItemList.Codes._5ExtremelyLikely);

				AssertToPercentage(0, "XXX");
			});
		}

		void AssertToPercentage(ZByte expectedPercentage, ZString itemCode)
		{
			AssertEquals(itemCode, expectedPercentage, CertaintyLikertItemHelper.ToPercentage(itemCode));
		}

		#endregion

		#region Implementation

		CertaintyLikertItemHelper CertaintyLikertItemHelper
		{
			get { return certaintyLikertItemHelper ?? (certaintyLikertItemHelper = new CertaintyLikertItemHelper()); }
		}
		CertaintyLikertItemHelper certaintyLikertItemHelper;

		#endregion
	}
}
