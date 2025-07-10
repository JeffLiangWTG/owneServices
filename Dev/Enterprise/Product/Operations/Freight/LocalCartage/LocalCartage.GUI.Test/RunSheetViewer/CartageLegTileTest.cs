using System.Data;
using System.Drawing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Freight.LocalCartage.GUI.Testing
{
	public class CartageLegTileTest : TestCaseWithFactory
	{
		public void TestPadding()
		{
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLExportPack;
			var container = cartage.ContainerBookedMoves.AddNew().Container;
			var move = cartage.LooseBookedMoves.AddNew();
			var leg = move.CartageLegs.AddNew();
			using (ZForm form = new ZForm())
			{
				var tile = new CartageLegTile();
				form.Controls.Add(tile);
				AssertEquals(0, tile.RoundCorners);
				Assert(!tile.ShowTitleBar);
				Assert(!tile.ShowTileShadow);
				var expectedPadding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
				AssertEquals(expectedPadding.Top, tile.DockPadding.Top);
				AssertEquals(expectedPadding.Bottom, tile.DockPadding.Bottom);
				AssertEquals(expectedPadding.Right, tile.DockPadding.Right);
				AssertEquals(expectedPadding.Left, tile.DockPadding.Left);
			}
		}

		public void TestTileColors()
		{
			var aqua = Color.FromArgb(192, 255, 255);
			var yellow = Color.FromArgb(255, 255, 192);
			var green = Color.FromArgb(215, 255, 215);
			var red = Color.FromArgb(255, 192, 192);
			var defaultColor = Color.FromArgb(192, 192, 255);
			var defaultGradient = Color.FromArgb(192, 255, 192);
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLExportPack;
			var container = cartage.ContainerBookedMoves.AddNew().Container;
			var move = cartage.LooseBookedMoves.AddNew();
			var leg = move.CartageLegs.AddNew();
			var runSheet = Factory.New<CommonWorkSheet>();
			runSheet.CartageLegs.Add(leg);
			Factory.Save();
			using (ZForm form = new ZForm())
			{
				var dashboardFactory = new BusinessObjectFactory();
				var dashboardRunSheet = dashboardFactory.Load<CommonWorkSheet>(runSheet.PK);
				var legDashboardFactory = dashboardRunSheet.CartageLegs[0];
				var tile = new CartageLegTile();
				AssertEquals("No Legs", 2, tile.TileColors.Length);
				form.Controls.Add(tile);
				tile.SetDataBinding(legDashboardFactory, "");
				AssertEquals("Leg has 2 addresses", 2, tile.TileColors.Length);
				AssertEquals(Color.White, tile.TileColors[0]);
				AssertEquals(Color.White, tile.TileColors[1]);
				AssertEquals(1, tile.TileColorPositions.Length);
				AssertEquals(0.67f, tile.TileColorPositions[0]);
				leg.JU_PickupTimeIn = ZDateTime.Now;
				Factory.Save();
				AssertEquals("Leg has 2 addresses", 2, tile.TileColors.Length);
				AssertEquals("Pick up has time-in so should be Aqua", aqua, tile.TileColors[0]);
				AssertEquals("Delivery should be White", Color.White, tile.TileColors[1]);
				leg.JU_PickupTimeOut = ZDateTime.Now;
				Factory.Save();
				AssertEquals("Leg has 2 addresses", 2, tile.TileColors.Length);
				AssertEquals("Pickup Complete so should be Green", green, tile.TileColors[0]);
				AssertEquals("Delivery should be White", Color.White, tile.TileColors[1]);
				leg.JU_DeliverTimeIn = ZDateTime.Now;
				Factory.Save();
				AssertEquals("Leg has 2 addresses", 2, tile.TileColors.Length);
				AssertEquals("Pickup Complete so should be Green", green, tile.TileColors[0]);
				AssertEquals("Deliver has Time-In so should be Aqua", aqua, tile.TileColors[1]);
				leg.JU_MessageStatus = Core.Constants.CartageLegDispatchStatusList.Codes.Rejected;
				Factory.Save();
				AssertEquals("Leg has 2 addresses", 2, tile.TileColors.Length);
				AssertEquals("Leg Rejected so Pickup should be Red", red, tile.TileColors[0]);
				AssertEquals("Leg Rejected so Delivery should be Red", red, tile.TileColors[1]);
				leg.JU_DeliverTimeOut = ZDateTime.Now;
				Factory.Save();
				AssertEquals("Leg has 2 addresses", 2, tile.TileColors.Length);
				AssertEquals("Pickup Complete so should be Green", green, tile.TileColors[0]);
				AssertEquals("Delivery Complete so should be Green", green, tile.TileColors[1]);
				leg.Delete();
				Factory.Save();
				AssertEquals("Leg has 2 addresses", 2, tile.TileColors.Length);
				AssertEquals("Leg Deleted so should be default color", defaultColor, tile.TileColors[0]);
				AssertEquals("Leg Deleted so should be default color", defaultGradient, tile.TileColors[1]);
				AssertEquals(1, tile.TileColorPositions.Length);
				AssertEquals("Default because deleted", 0.5f, tile.TileColorPositions[0]);
			}
		}

		public void TestTileColors_WithWaitPoint()
		{
			var aqua = Color.FromArgb(192, 255, 255);
			var yellow = Color.FromArgb(255, 255, 192);
			var green = Color.FromArgb(215, 255, 215);
			var red = Color.FromArgb(255, 192, 192);
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLExportPack;
			var container = cartage.ContainerBookedMoves.AddNew().Container;
			var move = cartage.LooseBookedMoves.AddNew();
			var leg = move.CartageLegs.AddNew();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			leg.JU_E2WaitPointAddressID = org.MainAddress.PK;
			var runSheet = Factory.New<CommonWorkSheet>();
			runSheet.CartageLegs.Add(leg);
			Factory.Save();
			using (ZForm form = new ZForm())
			{
				var dashboardFactory = new BusinessObjectFactory();
				var dashboardRunSheet = dashboardFactory.Load<CommonWorkSheet>(runSheet.PK);
				var legDashboardFactory = dashboardRunSheet.CartageLegs[0];
				var tile = new CartageLegTile();
				AssertEquals("No Legs", 2, tile.TileColors.Length);
				form.Controls.Add(tile);
				tile.SetDataBinding(legDashboardFactory, "");
				AssertEquals("Leg has 3 addresses", 3, tile.TileColors.Length);
				AssertEquals(Color.White, tile.TileColors[0]);
				AssertEquals(Color.White, tile.TileColors[1]);
				AssertEquals(Color.White, tile.TileColors[2]);
				leg.JU_PickupTimeIn = ZDateTime.Now;
				Factory.Save();
				AssertEquals("Leg has 3 addresses", 3, tile.TileColors.Length);
				AssertEquals("Pick up has time-in so should be Aqua", aqua, tile.TileColors[0]);
				AssertEquals("Waitpoint should be White", Color.White, tile.TileColors[1]);
				AssertEquals("Delivery should be White", Color.White, tile.TileColors[2]);
				leg.JU_PickupTimeOut = ZDateTime.Now;
				Factory.Save();
				AssertEquals("Leg has 3 addresses", 3, tile.TileColors.Length);
				AssertEquals("Pickup Complete so should be Green", green, tile.TileColors[0]);
				AssertEquals("Waitpoint should be White", Color.White, tile.TileColors[1]);
				AssertEquals("Delivery should be White", Color.White, tile.TileColors[2]);
				leg.JU_WaitPointTimeIn = ZDateTime.Now;
				Factory.Save();
				AssertEquals("Leg has 3 addresses", 3, tile.TileColors.Length);
				AssertEquals("Pickup Complete so should be Green", green, tile.TileColors[0]);
				AssertEquals("Waitpoint has Time-in so should be Aqua", aqua, tile.TileColors[1]);
				AssertEquals("Delivery should be White", Color.White, tile.TileColors[2]);
				leg.JU_WaitPointTimeOut = ZDateTime.Now;
				Factory.Save();
				AssertEquals("Leg has 3 addresses", 3, tile.TileColors.Length);
				AssertEquals("Pickup Complete so should be Green", green, tile.TileColors[0]);
				AssertEquals("Waitpoint Complete so should be Green", green, tile.TileColors[1]);
				AssertEquals("Delivery should be White", Color.White, tile.TileColors[2]);
				leg.JU_DeliverTimeIn = ZDateTime.Now;
				Factory.Save();
				AssertEquals("Leg has 3 addresses", 3, tile.TileColors.Length);
				AssertEquals("Pickup Complete so should be Green", green, tile.TileColors[0]);
				AssertEquals("Waitpoint Complete so should be Green", green, tile.TileColors[1]);
				AssertEquals("Deliver has Time-In so should be Aqua", aqua, tile.TileColors[2]);
				leg.JU_DeliverTimeOut = ZDateTime.Now;
				Factory.Save();
				AssertEquals("Leg has 3 addresses", 3, tile.TileColors.Length);
				AssertEquals("Pickup Complete so should be Green", green, tile.TileColors[0]);
				AssertEquals("Waitpoint Complete so should be Green", green, tile.TileColors[1]);
				AssertEquals("Delivery Complete so should be Green", green, tile.TileColors[2]);
				leg.JU_MessageStatus = Core.Constants.CartageLegDispatchStatusList.Codes.Rejected;
				Factory.Save();
				AssertEquals("Leg has 3 addresses", 3, tile.TileColors.Length);
				AssertEquals("Leg Rejected so Pickup should be Red", red, tile.TileColors[0]);
				AssertEquals("Leg Rejected so Waitpoint should be Red", red, tile.TileColors[1]);
				AssertEquals("Leg Rejected so Delivery should be Red", red, tile.TileColors[2]);
			}
		}

		public void TestNoExceptionWhenStatusIsDispatched()
		{
			var yellow = Color.FromArgb(255, 255, 192);
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLExportPack;
			var move = cartage.LooseBookedMoves.AddNew();
			var leg = move.CartageLegs.AddNew();
			var runSheet = Factory.New<CommonWorkSheet>();
			runSheet.CartageLegs.Add(leg);
			Factory.Save();
			using (var form = new ZForm())
			{
				var dashboardFactory = new BusinessObjectFactory();
				var dashboardRunSheet = dashboardFactory.Load<CommonWorkSheet>(runSheet.PK);
				var legDashboardFactory = dashboardRunSheet.CartageLegs[0];
				var tile = new CartageLegTile();
				AssertEquals("No Legs", 2, tile.TileColors.Length);
				form.Controls.Add(tile);
				tile.SetDataBinding(legDashboardFactory, "");
				leg.JU_MessageStatus = Core.Constants.CartageLegDispatchStatusList.Codes.NotStarted;
				Factory.Save();
				AssertEquals("Leg has 2 addresses", 2, tile.TileColors.Length);
				AssertEquals("Leg is dispatched so Pickup should be Yellow", yellow, tile.TileColors[0]);
				AssertEquals("Leg is dispatched so Delivery should be Yellow", yellow, tile.TileColors[1]);
			}
		}

		public void TestTileColorPositions()
		{
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLExportPack;
			var container = cartage.ContainerBookedMoves.AddNew().Container;
			var move = cartage.LooseBookedMoves.AddNew();
			var leg = move.CartageLegs.AddNew();
			var runSheet = Factory.New<CommonWorkSheet>();
			runSheet.CartageLegs.Add(leg);
			Factory.Save();
			using (ZForm form = new ZForm())
			{
				var dashboardFactory = new BusinessObjectFactory();
				var dashboardRunSheet = dashboardFactory.Load<CommonWorkSheet>(runSheet.PK);
				var legDashboardFactory = dashboardRunSheet.CartageLegs[0];
				var tile = new CartageLegTile();
				AssertEquals("No Legs", 1, tile.TileColorPositions.Length);
				form.Controls.Add(tile);
				tile.SetDataBinding(legDashboardFactory, "");
				AssertEquals(1, tile.TileColorPositions.Length);
				AssertEquals(0.67f, tile.TileColorPositions[0]);
			}
		}

		public void TestTileColorPositions_WithWaitPoint()
		{
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLExportPack;
			var container = cartage.ContainerBookedMoves.AddNew().Container;
			var move = cartage.LooseBookedMoves.AddNew();
			var leg = move.CartageLegs.AddNew();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			leg.JU_E2WaitPointAddressID = org.MainAddress.PK;
			var runSheet = Factory.New<CommonWorkSheet>();
			runSheet.CartageLegs.Add(leg);
			Factory.Save();
			using (ZForm form = new ZForm())
			{
				var dashboardFactory = new BusinessObjectFactory();
				var dashboardRunSheet = dashboardFactory.Load<CommonWorkSheet>(runSheet.PK);
				var legDashboardFactory = dashboardRunSheet.CartageLegs[0];
				var tile = new CartageLegTile();
				AssertEquals("No Legs", 1, tile.TileColorPositions.Length);
				form.Controls.Add(tile);
				tile.SetDataBinding(legDashboardFactory, "");
				AssertEquals(2, tile.TileColorPositions.Length);
				AssertEquals(0.5f, tile.TileColorPositions[0]);
				AssertEquals(0.75f, tile.TileColorPositions[1]);
			}
		}

		[ExpectNoExceptions()]
		public void TestClientLegs()
		{
			var leg = Factory.New<ClientLeg>();
			using (var form = new ZForm())
			{
				var tile = new CartageLegTile();
				form.Controls.Add(tile);
				tile.SetDataBinding(leg, "");
			}
		}

		class ClientLeg : CommonCartageLeg
		{
			public ClientLeg(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}
		}

		public void TestBindWithNewFactory()
		{
			var containerType20GP = new RefContainer.Loader(Factory).LoadFromCode("20GP").PK;
			var containerType40GP = new RefContainer.Loader(Factory).LoadFromCode("40GP").PK;
			// leg w/ 20GP
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLExportPack;
			var move = cartage.ContainerBookedMoves.AddNew();
			var container = move.Container;
			container.JC_RC = containerType20GP;
			var leg = move.CartageLegs.AddNew();
			Factory.Save();
			// same Leg in another Enterprise instance, but w/ 40GP
			var factory2 = new BusinessObjectFactory();
			factory2.RefreshEnabled = false;
			var container_Factory2 = factory2.Load<CommonContainer>(container.PK);
			container_Factory2.JC_RC = containerType40GP;
			var leg_Factory2 = factory2.Load<CommonCartageLeg>(leg.PK);
			factory2.Save();
			using (var form = new ZForm())
			{
				form.Show();
				var tile = new CartageLegTile();
				form.Controls.Add(tile);
				tile.SetDataBinding(leg, "");
				AssertEquals("  20GP  T00001000/A", tile.WhatLabel.Text);
				tile.SetDataBinding(leg_Factory2, "");
				AssertEquals("  40GP  T00001000/A", tile.WhatLabel.Text);
			}
		}
	}
}
