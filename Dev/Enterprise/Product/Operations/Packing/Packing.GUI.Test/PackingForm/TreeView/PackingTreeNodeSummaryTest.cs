using System.Collections.Generic;
using System.Linq;
using Enterprise.Packing.Business.Testing;

namespace Enterprise.Packing.GUI.Testing
{
	class PackingTreeNodeSummaryTest : PackingTestCaseWithFactory
	{
		public void TestTokens()
		{
			var summary = new PackingTreeNodeSummary();
			AssertContainsExactElementsInExactOrder(
				new[]
				{
					(PackingTreeNodeSummary.Constants.PackageContentsID, PackingTreeNodeSummary.Constants.PackageContents, (byte)1, true),
					(PackingTreeNodeSummary.Constants.PackageIDID, PackingTreeNodeSummary.Constants.PackageID, (byte)2, true),
					(PackingTreeNodeSummary.Constants.WeightID, PackingTreeNodeSummary.Constants.Weight, (byte)3, true),
					(PackingTreeNodeSummary.Constants.VolumeID, PackingTreeNodeSummary.Constants.Volume, (byte)4, false),
					(PackingTreeNodeSummary.Constants.DimensionsID, PackingTreeNodeSummary.Constants.Dimensions, (byte)5, false),
					(PackingTreeNodeSummary.Constants.CommodityID, PackingTreeNodeSummary.Constants.Commodity, (byte)6, false),
					(PackingTreeNodeSummary.Constants.TemperatureID, PackingTreeNodeSummary.Constants.Temperature, (byte)7, false),
					(PackingTreeNodeSummary.Constants.PackageSequenceID, PackingTreeNodeSummary.Constants.PackageSequence, (byte)8, false),
					(PackingTreeNodeSummary.Constants.HeldID, PackingTreeNodeSummary.Constants.Held, (byte)9, false),
					(PackingTreeNodeSummary.Constants.HandlingUnitID, PackingTreeNodeSummary.Constants.HandlingUnit, (byte)10, false)
				},
				summary.Tokens.Cast<PackingTreeNodeSummaryToken>().Select(t => (t.ColumnName, t.ToString(), t.ColumnPosition, t.IsVisible)));
		}

		public void TestGetOrderedVisibleTokens()
		{
			var summary = new PackingTreeNodeSummary();
			AssertContainsExactElementsInExactOrder(
				new[]
				{
					(PackingTreeNodeSummary.Constants.PackageContentsID, PackingTreeNodeSummary.Constants.PackageContents, (byte)1),
					(PackingTreeNodeSummary.Constants.PackageIDID, PackingTreeNodeSummary.Constants.PackageID, (byte)2),
					(PackingTreeNodeSummary.Constants.WeightID, PackingTreeNodeSummary.Constants.Weight, (byte)3)
				},
				summary.GetOrderedVisibleTokens().Select(t => (t.ColumnName, t.ToString(), t.ColumnPosition)));
		}

		public void TestUpdateTokens()
		{
			var summary = new PackingTreeNodeSummary();
			var list = new List<PackingTreeNodeSummaryToken>()
			{
				new PackingTreeNodeSummaryToken(PackingTreeNodeSummary.Constants.VolumeID, PackingTreeNodeSummary.Constants.Volume, 1, p => "") { IsVisible = true },
				new PackingTreeNodeSummaryToken(PackingTreeNodeSummary.Constants.CommodityID, PackingTreeNodeSummary.Constants.Commodity, 2, p => "") { IsVisible = true },
				new PackingTreeNodeSummaryToken(PackingTreeNodeSummary.Constants.DimensionsID, PackingTreeNodeSummary.Constants.Dimensions, 3, p => "") { IsVisible = true },
				new PackingTreeNodeSummaryToken(PackingTreeNodeSummary.Constants.HandlingUnitID, PackingTreeNodeSummary.Constants.HandlingUnit, 4, p => "") { IsVisible = true },
			};

			summary.UpdateTokens(list);

			AssertContainsExactElementsInExactOrder(
				new[]
				{
					(PackingTreeNodeSummary.Constants.VolumeID, PackingTreeNodeSummary.Constants.Volume, (byte)0, true),
					(PackingTreeNodeSummary.Constants.PackageContentsID, PackingTreeNodeSummary.Constants.PackageContents, (byte)1, false),
					(PackingTreeNodeSummary.Constants.CommodityID, PackingTreeNodeSummary.Constants.Commodity, (byte)1, true),
					(PackingTreeNodeSummary.Constants.PackageIDID, PackingTreeNodeSummary.Constants.PackageID, (byte)2, false),
					(PackingTreeNodeSummary.Constants.DimensionsID, PackingTreeNodeSummary.Constants.Dimensions, (byte)2, true),
					(PackingTreeNodeSummary.Constants.WeightID, PackingTreeNodeSummary.Constants.Weight, (byte)3, false),
					(PackingTreeNodeSummary.Constants.HandlingUnitID, PackingTreeNodeSummary.Constants.HandlingUnit, (byte)3, true),
					(PackingTreeNodeSummary.Constants.TemperatureID, PackingTreeNodeSummary.Constants.Temperature, (byte)7, false),
					(PackingTreeNodeSummary.Constants.PackageSequenceID, PackingTreeNodeSummary.Constants.PackageSequence, (byte)8, false),
					(PackingTreeNodeSummary.Constants.HeldID, PackingTreeNodeSummary.Constants.Held, (byte)9, false)
				},
				summary.Tokens.Cast<PackingTreeNodeSummaryToken>().Select(t => (t.ColumnName, t.ToString(), t.ColumnPosition, t.IsVisible)));

			AssertContainsExactElementsInExactOrder(
				new[]
				{
					(PackingTreeNodeSummary.Constants.VolumeID, PackingTreeNodeSummary.Constants.Volume, (byte)0),
					(PackingTreeNodeSummary.Constants.CommodityID, PackingTreeNodeSummary.Constants.Commodity, (byte)1),
					(PackingTreeNodeSummary.Constants.DimensionsID, PackingTreeNodeSummary.Constants.Dimensions, (byte)2),
					(PackingTreeNodeSummary.Constants.HandlingUnitID, PackingTreeNodeSummary.Constants.HandlingUnit, (byte)3)
				},
				summary.GetOrderedVisibleTokens().Select(t => (t.ColumnName, t.ToString(), t.ColumnPosition)));
		}
	}
}
