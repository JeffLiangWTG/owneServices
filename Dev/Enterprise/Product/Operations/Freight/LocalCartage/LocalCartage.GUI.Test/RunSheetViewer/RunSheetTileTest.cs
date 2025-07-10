using System.Drawing;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.LocalCartage.GUI.Testing
{
	public class RunSheetTileTest : TestCaseWithFactory
	{
		public void TestPadding()
		{
			CommonCartage cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLExportPack;
			CommonContainer container = cartage.ContainerBookedMoves.AddNew().Container;
			CommonBookedCtgMove move = cartage.LooseBookedMoves.AddNew();
			CommonCartageLeg leg = move.CartageLegs.AddNew();
			CommonWorkSheet runSheet = Factory.New<CommonWorkSheet>();
			runSheet.CartageLegs.Add(leg);
			using (ZForm form = new ZForm())
			{
				RunSheetTile tile = new RunSheetTile();
				AssertEquals(10, tile.RoundCorners);
				Assert(tile.ShowTitleBar);
				Assert(tile.ShowTileShadow);
				form.Controls.Add(tile);
				AssertEquals(10, tile.RoundCorners);
				Assert(tile.ShowTitleBar);
				Assert(tile.ShowTileShadow);
				AssertEquals(22, tile.DockPadding.Top);
				AssertEquals(11, tile.DockPadding.Bottom);
				AssertEquals(11, tile.DockPadding.Right);
				AssertEquals(8, tile.DockPadding.Left);
			}
		}

		public void TestTileColor()
		{
			CommonCartage cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLExportPack;
			CommonContainer container = cartage.ContainerBookedMoves.AddNew().Container;
			CommonBookedCtgMove move = cartage.LooseBookedMoves.AddNew();
			CommonCartageLeg leg = move.CartageLegs.AddNew();
			CommonWorkSheet runSheet = Factory.New<CommonWorkSheet>();
			Factory.Save();
			using (ZForm form = new ZForm())
			{
				BusinessObjectFactory dashboardFactory = new BusinessObjectFactory();
				CommonWorkSheet dashboardRunSheet = dashboardFactory.Load<CommonWorkSheet>(runSheet.PK);
				RunSheetTile tile = new RunSheetTile();
				AssertEquals("No run sheet - Default (White) TC: 192, 0, 192", Color.FromArgb(192, 0, 192), tile.TileColor);
				AssertEquals("No run sheet - Default TCG: 255, 192, 255", Color.FromArgb(255, 192, 255), tile.TileColorGradient);
				form.Controls.Add(tile);
				tile.SetDataBinding(dashboardRunSheet, "");
				AssertEquals("None (Grey) TC: 117, 117, 117", Color.FromArgb(117, 117, 117), tile.TileColor);
				AssertEquals("None (Grey) TCG: 255, 255, 255", Color.FromArgb(255, 255, 255), tile.TileColorGradient);
				runSheet.CartageLegs.Add(leg);
				Factory.Save();
				AssertEquals("Last Leg (Aqua) TC: 0, 180, 180", Color.FromArgb(0, 180, 180), tile.TileColor);
				AssertEquals("Last Leg (Aqua) TCG: 200, 255, 255", Color.FromArgb(200, 255, 255), tile.TileColorGradient);
				leg.JU_PickupTimeIn = ZDateTime.Now;
				Factory.Save();
				AssertEquals("Still Last Leg (Aqua) TC: 0, 180, 180", Color.FromArgb(0, 180, 180), tile.TileColor);
				AssertEquals("Still Last Leg (Aqua) TCG: 200, 255, 255", Color.FromArgb(200, 255, 255), tile.TileColorGradient);
				leg.JU_PickupTimeOut = ZDateTime.Now;
				leg.JU_DeliverTimeIn = ZDateTime.Now;
				leg.JU_DeliverTimeOut = ZDateTime.Now;
				Factory.Save();
				AssertEquals("Complete (Green) TC: 0, 180, 0", Color.FromArgb(0, 180, 0), tile.TileColor);
				AssertEquals("Complete (Green) TCG: 200, 255, 200", Color.FromArgb(200, 255, 200), tile.TileColorGradient);
				CommonCartageLeg leg2 = move.CartageLegs.AddNew();
				CommonCartageLeg leg3 = move.CartageLegs.AddNew();
				runSheet.CartageLegs.Add(leg2);
				leg2.JU_RunSheetSequence = 1;
				runSheet.CartageLegs.Add(leg3);
				leg3.JU_RunSheetSequence = 2;
				Factory.Save();
				AssertEquals("Normal (Blue) TC: 0, 0, 180", Color.FromArgb(0, 0, 180), tile.TileColor);
				AssertEquals("Normal (Blue) TCG: 200, 200, 255", Color.FromArgb(200, 200, 255), tile.TileColorGradient);
				leg2.JU_MessageStatus = Core.Constants.CartageLegDispatchStatusList.Codes.Rejected;
				Factory.Save();
				AssertEquals("Error (Red) TC: 180, 0, 0", Color.FromArgb(180, 0, 0), tile.TileColor);
				AssertEquals("Error (Red) TCG: 255, 200, 200", Color.FromArgb(255, 200, 200), tile.TileColorGradient);
				runSheet.ErrorStatus = CommonWorkSheet.ErrorStatuses.Canceled;
				Factory.Save();
				((ILogsInternals)dashboardRunSheet.Logs).ReloadFromDB();
				dashboardRunSheet.ResetErrorStatus();
				dashboardRunSheet.StatusInfo.RefreshBinding();
				AssertEquals("Error (Red) TC: 180, 0, 0", Color.FromArgb(180, 0, 0), tile.TileColor);
				AssertEquals("Error (Red) TCG: 255, 200, 200", Color.FromArgb(255, 200, 200), tile.TileColorGradient);
			}
		}

		public void TestLegTiles()
		{
			CommonCartage cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLExportPack;
			CommonContainer container = cartage.ContainerBookedMoves.AddNew().Container;
			CommonBookedCtgMove move = cartage.LooseBookedMoves.AddNew();
			CommonCartageLeg leg1 = move.CartageLegs.AddNew();
			CommonCartageLeg leg2 = move.CartageLegs.AddNew();
			CommonWorkSheet runSheet = Factory.New<CommonWorkSheet>();
			runSheet.CartageLegs.Add(leg1);
			runSheet.CartageLegs.Add(leg2);
			leg1.JU_RunSheetSequence = 2;
			leg2.JU_RunSheetSequence = 1;
			using (ZForm form = new ZForm())
			{
				RunSheetTile tile = new RunSheetTile();
				form.Controls.Add(tile);
				AssertEquals(0, tile.BodyPanel.Controls.Count);
				tile.SetDataBinding(runSheet, "");
				AssertEquals("Controls should contain 2 leg tiles", 2, tile.BodyPanel.Controls.Count);
				AssertEquals(typeof(CartageLegTile), tile.BodyPanel.Controls[0].GetType());
				AssertEquals(typeof(CartageLegTile), tile.BodyPanel.Controls[1].GetType());
				AssertEquals("CartageLegTiles should contain 2 leg tiles", 2, tile.CartageLegTiles.Count);
				Assert(tile.CartageLegTiles.ContainsKey(leg1.PK));
				Assert(tile.CartageLegTiles.ContainsKey(leg2.PK));
				AssertEquals("Should be ordered by index, leg 1 should be lower", 0, tile.BodyPanel.Controls.GetChildIndex(tile.CartageLegTiles[leg1.PK], true));
				AssertEquals("Should be ordered by index, leg 2 should be higher", 1, tile.BodyPanel.Controls.GetChildIndex(tile.CartageLegTiles[leg2.PK], true));
			}
		}

		public void TestBindWithNewFactory()
		{
			var containerType20GP = new RefContainer.Loader(Factory).LoadFromCode("20GP").PK;
			// RunSheet w/ 3 legs
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLExportPack;
			var move = cartage.ContainerBookedMoves.AddNew();
			var container = move.Container;
			container.JC_RC = containerType20GP;
			var leg1 = move.CartageLegs.AddNew();
			var leg2 = move.CartageLegs.AddNew();
			var leg3 = move.CartageLegs.AddNew();
			var runSheet = Factory.New<CommonWorkSheet>();
			runSheet.CartageLegs.Add(leg1);
			runSheet.CartageLegs.Add(leg2);
			leg1.JU_RunSheetSequence = 1;
			leg2.JU_RunSheetSequence = 2;
			Factory.Save();
			// same RunSheet in another Factory, but add the 3rd leg
			var factory2 = new BusinessObjectFactory();
			factory2.RefreshEnabled = false;
			var leg3_Factory2 = factory2.Load<CommonCartageLeg>(leg3.PK);
			var runSheet_Factory2 = factory2.Load<CommonWorkSheet>(runSheet.PK);
			runSheet_Factory2.CartageLegs.Add(leg3_Factory2);
			factory2.Save();
			using (var form = new ZForm())
			{
				form.Show();
				var tile = new RunSheetTile();
				form.Controls.Add(tile);
				AssertEquals("Precondition: No Legs yet.", 0, tile.BodyPanel.Controls.Count);
				tile.SetDataBinding(runSheet, "");
				var cartageLegTiles = tile.BodyPanel.Controls.Cast<CartageLegTile>();
				AssertEquals("Controls should contain 2 Leg Tiles.", 2, cartageLegTiles.Count());
				AssertEquals("CartageLegTiles Dictionary should contain 2 leg tiles", 2, tile.CartageLegTiles.Count);
				Assert(tile.CartageLegTiles.ContainsKey(leg1.PK));
				Assert(tile.CartageLegTiles.ContainsKey(leg2.PK));
				tile.SetDataBinding(runSheet_Factory2, "");
				cartageLegTiles = tile.BodyPanel.Controls.Cast<CartageLegTile>();
				AssertEquals("Controls should contain 3 Leg Tiles.", 3, cartageLegTiles.Count());
				AssertEquals("CartageLegTiles should contain 3 leg tiles", 3, tile.CartageLegTiles.Count);
				Assert(tile.CartageLegTiles.ContainsKey(leg1.PK));
				Assert(tile.CartageLegTiles.ContainsKey(leg2.PK));
				Assert(tile.CartageLegTiles.ContainsKey(leg3.PK));
			}
		}

		public void TestBindWithNewFactory_LegRemovedAndAdded()
		{
			var containerType20GP = new RefContainer.Loader(Factory).LoadFromCode("20GP").PK;
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLExportPack;
			var move = cartage.ContainerBookedMoves.AddNew();
			var container = move.Container;
			container.JC_RC = containerType20GP;
			var leg1 = move.CartageLegs.AddNew();
			var runSheet = Factory.New<CommonWorkSheet>();
			runSheet.CartageLegs.Add(leg1);
			leg1.JU_RunSheetSequence = 1;
			Factory.Save();
			using (var form = new ZForm())
			{
				form.Show();
				var tile = new RunSheetTile();
				form.Controls.Add(tile);
				AssertEquals("Precondition: No Legs yet.", 0, tile.BodyPanel.Controls.Count);
				tile.SetDataBinding(runSheet, "");
				var cartageLegTiles = tile.BodyPanel.Controls.Cast<CartageLegTile>();
				AssertEquals("Controls should contain 1 Leg Tiles.", 1, cartageLegTiles.Count());
				AssertEquals("CartageLegTiles Dictionary should contain 1 leg tiles", 1, tile.CartageLegTiles.Count);
				Assert(tile.CartageLegTiles.ContainsKey(leg1.PK));
				var factory2 = new BusinessObjectFactory();
				var runSheet_Factory2 = factory2.Load<CommonWorkSheet>(runSheet.PK);
				var leg1_Factory2 = factory2.Load<CommonCartageLeg>(leg1.PK);
				leg1_Factory2.Delete();
				AssertEquals(true, leg1_Factory2.IsDeleted);
				var legNew_Factory2 = runSheet_Factory2.CartageLegs.AddNew();
				runSheet_Factory2.CartageLegs.Add(legNew_Factory2);
				factory2.Save();
				AssertEquals("Controls should contain 1 Leg Tiles.", 1, cartageLegTiles.Count());
				AssertEquals("CartageLegTiles Dictionary should contain 1 leg tiles", 1, tile.CartageLegTiles.Count);
				Assert(!tile.CartageLegTiles.ContainsKey(leg1.PK));
				Assert(tile.CartageLegTiles.ContainsKey(legNew_Factory2.PK));
			}
		}
	}
}
