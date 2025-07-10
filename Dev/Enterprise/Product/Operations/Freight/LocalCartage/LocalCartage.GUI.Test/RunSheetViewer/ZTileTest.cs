using System.Drawing;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Freight.LocalCartage.GUI.Testing
{
	public class ZTileTest : TestCaseWithFactory
	{
		public void TestPadding()
		{
			var commandExecuteCount = DbCommandExecuteTracker.Instance.ExecuteCount;
			using (ZForm form = new ZForm())
			{
				ZGroupBoxTile tile = new ZGroupBoxTile();
				form.Controls.Add(tile);
				AssertEquals(10, tile.RoundCorners);
				Assert(!tile.ShowTitleBar);
				Assert(!tile.ShowTileShadow);
				AssertEquals(8, tile.DockPadding.Top);
				AssertEquals(8, tile.DockPadding.Bottom);
				AssertEquals(8, tile.DockPadding.Right);
				AssertEquals(8, tile.DockPadding.Left);
				tile.ShowTitleBar = true;
				AssertEquals(22, tile.DockPadding.Top);
				AssertEquals(8, tile.DockPadding.Bottom);
				AssertEquals(8, tile.DockPadding.Right);
				AssertEquals(8, tile.DockPadding.Left);
				tile.ShowTileShadow = true;
				AssertEquals(3, tile.ShadowThickness);
				AssertEquals(22, tile.DockPadding.Top);
				AssertEquals(11, tile.DockPadding.Bottom);
				AssertEquals(11, tile.DockPadding.Right);
				AssertEquals(8, tile.DockPadding.Left);
				tile.RoundCorners = 6;
				AssertEquals(22, tile.DockPadding.Top);
				AssertEquals(9, tile.DockPadding.Bottom);
				AssertEquals(9, tile.DockPadding.Right);
				AssertEquals(6, tile.DockPadding.Left);
			}
			AssertEquals("Should not be any DB Hits here", DbCommandExecuteTracker.Instance.ExecuteCount, commandExecuteCount);
		}

		[ExpectNoExceptions]
		public void TestTile()
		{
			var commandExecuteCount = DbCommandExecuteTracker.Instance.ExecuteCount;
			using (ZForm form = new ZForm())
			{
				ZGroupBoxTile tile = new ZGroupBoxTile();
				form.Controls.Add(tile);
				tile.BackColor = Color.White;
				tile.ShowTitleBar = true;
				tile.TitleBarText = "Hello";
				tile.TileColor = Color.White;
				tile.TileColorGradient = Color.White;
				tile.TileGradient = ZGroupBoxTile.TilesGradient.Horizontal;
				tile.RoundCorners = 6;
				tile.BorderColor = Color.White;
				tile.BorderThickness = 2;
				tile.ShadowColor = Color.White;
				tile.ShadowThickness = 4;
				tile.ShowTileShadow = true;
				tile.PanelColor = Color.White;
				tile.PanelColorGradient = Color.White;
				tile.PanelBorderColor = Color.White;
				tile.BackColor = Color.Turquoise;
				tile.ShowTitleBar = false;
				tile.TitleBarText = "Tile";
				tile.TileColor = Color.Turquoise;
				tile.TileColorGradient = Color.Turquoise;
				tile.TileGradient = ZGroupBoxTile.TilesGradient.Vertical;
				tile.RoundCorners = 7;
				tile.BorderColor = Color.Turquoise;
				tile.BorderThickness = 6;
				tile.ShadowColor = Color.Turquoise;
				tile.ShadowThickness = 2;
				tile.ShowTileShadow = false;
				tile.PanelColor = Color.Turquoise;
				tile.PanelColorGradient = Color.Turquoise;
				tile.PanelBorderColor = Color.Turquoise;
			}
			AssertEquals("Should not be any DB Hits here", DbCommandExecuteTracker.Instance.ExecuteCount, commandExecuteCount);
		}
	}
}
