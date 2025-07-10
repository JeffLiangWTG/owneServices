using System;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Interop;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.AutoRefresh;
using NUnit.Framework;

namespace Enterprise.Freight.LocalCartage.GUI.Testing
{
	public class RunSheetDashboardControlTest : TestCaseWithFactory
	{
		[TestDate(2009, 4, 1)]
		public void TestRunSheetTiles()
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
			Factory.Save();
			var dashboards = RunSheetDashboardCollection.New(Factory);
			var dashboard = dashboards[0];
			using (ZForm form = new ZForm())
			{
				RunSheetDashboardControl viewerControl = new RunSheetDashboardControl();
				form.Controls.Add(viewerControl);
				AssertEquals("Expected one ZLabel control", 1, viewerControl.BodyPanel.Controls.Count);
				AssertEquals("Expected one ZLabel control", typeof(ZLabel), viewerControl.BodyPanel.Controls[0].GetType());
				viewerControl.SetDataBinding(dashboards, "");
				runSheet.EY_StartTime = ZDateTime.Today;
				runSheet.EY_EndTime = ZDateTime.Today.AddDays(1);
				Factory.Save();
				dashboard.DateRangeFilter = "Today";
				AssertEquals("Controls should contain ZLabel + 1 RunSheet Tile", 2, viewerControl.BodyPanel.Controls.Count);
				AssertEquals(typeof(RunSheetTile), viewerControl.BodyPanel.Controls[1].GetType());
				dashboard.DateRangeFilter = "Tomorrow";
				AssertEquals("Controls should not contain any RunSheet Tiles only 1 Zlabel", 1, viewerControl.BodyPanel.Controls.Count);
				AssertEquals("Controls should not contain any RunSheet Tiles only 1 Zlabel", typeof(ZLabel), viewerControl.BodyPanel.Controls[0].GetType());
			}
		}

		[TestDate(2009, 4, 1)]
		public void TestRunSheetRefreshBinding()
		{
			CommonCartage cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLExportPack;
			var move = cartage.ContainerBookedMoves.AddNew();
			var container = move.Container;
			container.JC_RC = new RefContainer.Loader(Factory).LoadFromCode("20GP").PK;
			var leg1 = move.CartageLegs.AddNew();
			var runSheet = Factory.New<CommonWorkSheet>();
			runSheet.CartageLegs.Add(leg1);
			leg1.JU_RunSheetSequence = 1;
			runSheet.EY_StartTime = ZDateTime.Today;
			runSheet.EY_EndTime = ZDateTime.Today.AddDays(1);
			Factory.Save();
			var dashboardFactory = new BusinessObjectFactory();
			var dashboards = RunSheetDashboardCollection.New(dashboardFactory);
			using (ZForm form = new ZForm())
			{
				RunSheetDashboardControl viewerControl = new RunSheetDashboardControl();
				form.Controls.Add(viewerControl);
				AssertEquals("Expected one ZLabel control", 1, viewerControl.BodyPanel.Controls.Count);
				AssertEquals("Expected one ZLabel control", typeof(ZLabel), viewerControl.BodyPanel.Controls[0].GetType());
				viewerControl.SetDataBinding(dashboards, "");
				dashboards[0].DateRangeFilter = "Today";
				AssertEquals("Controls should contain ZLabel andd 1 RunSheet Tile", 2, viewerControl.BodyPanel.Controls.Count);
				AssertEquals("Expected ZLabel control", typeof(ZLabel), viewerControl.BodyPanel.Controls[0].GetType());
				AssertEquals("Expected RunSheetTile control", typeof(RunSheetTile), viewerControl.BodyPanel.Controls[1].GetType());
				RunSheetTile tile = viewerControl.BodyPanel.Controls[1] as RunSheetTile;
				Assert("Expected control with index 1 on body panel to be RunSheetTile control", tile != null);
				AssertEquals("Controls should contain 1 Cartage Leg Tile", 1, tile.BodyPanel.Controls.Count);
				AssertEquals(typeof(CartageLegTile), tile.BodyPanel.Controls[0].GetType());
				CartageLegTile legTile = (CartageLegTile)tile.BodyPanel.Controls[0];
				form.Show();
				AssertEquals("  20GP  T00001000/A", legTile.WhatLabel.Text);
				// change to 40GP in another Factory
				container.JC_RC = new RefContainer.Loader(Factory).LoadFromCode("40GP").PK;
				Factory.Save();
				AssertEquals("  20GP  T00001000/A", legTile.WhatLabel.Text);
				viewerControl.RefreshButton.PerformClick();
				AssertEquals("Controls should contain ZLabel andd 1 RunSheet Tile", 2, viewerControl.BodyPanel.Controls.Count);
				AssertEquals("Expected ZLabel control", typeof(ZLabel), viewerControl.BodyPanel.Controls[0].GetType());
				AssertEquals("Expected RunSheetTile control", typeof(RunSheetTile), viewerControl.BodyPanel.Controls[1].GetType());
				tile = (RunSheetTile)viewerControl.BodyPanel.Controls[1];
				AssertEquals("Controls should contain 1 Cartage Leg Tile", 1, tile.BodyPanel.Controls.Count);
				legTile = (CartageLegTile)tile.BodyPanel.Controls[0];
				AssertEquals("  40GP  T00001000/A", legTile.WhatLabel.Text);
			}
		}

		[TestDate(2009, 4, 1)]
		public void TestRunSheetRefreshBinding_WhileAnotherWindowHasAWindowLock()
		{
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLExportPack;
			var move = cartage.ContainerBookedMoves.AddNew();
			var container = move.Container;
			container.JC_RC = new RefContainer.Loader(Factory).LoadFromCode("20GP").PK;
			var leg1 = move.CartageLegs.AddNew();
			var runSheet = Factory.New<CommonWorkSheet>();
			runSheet.CartageLegs.Add(leg1);
			leg1.JU_RunSheetSequence = 1;
			runSheet.EY_StartTime = ZDateTime.Today;
			runSheet.EY_EndTime = ZDateTime.Today.AddDays(1);
			Factory.Save();
			var dashboardFactory = new BusinessObjectFactory();
			var dashboards = RunSheetDashboardCollection.New(dashboardFactory);
			var dashboard = dashboards[0];
			using (ZForm someOtherForm = new ZForm())
			{
				try
				{
#if !WINZOR
					SafeNativeMethods.LockWindowUpdate(someOtherForm.Handle);
#endif
					using (ZForm form = new ZForm())
					{
						var viewerControl = new RunSheetDashboardControl();
						form.Controls.Add(viewerControl);
						AssertEquals("Expected one ZLabel control", 1, viewerControl.BodyPanel.Controls.Count);
						AssertEquals("Expected one ZLabel control", typeof(ZLabel), viewerControl.BodyPanel.Controls[0].GetType());
						viewerControl.SetDataBinding(dashboards, "");
						dashboard.DateRangeFilter = "Today";
						AssertEquals("Controls should contain ZLabel andd 1 RunSheet Tile", 2, viewerControl.BodyPanel.Controls.Count);
						AssertEquals("Expected ZLabel control", typeof(ZLabel), viewerControl.BodyPanel.Controls[0].GetType());
						AssertEquals("Expected RunSheetTile control", typeof(RunSheetTile), viewerControl.BodyPanel.Controls[1].GetType());
						var tile = viewerControl.BodyPanel.Controls[1] as RunSheetTile;
						Assert("Expected control with index 1 on body panel to be RunSheetTile control", tile != null);
						AssertEquals("Controls should contain 1 Cartage Leg Tile", 1, tile.BodyPanel.Controls.Count);
						AssertEquals(typeof(CartageLegTile), tile.BodyPanel.Controls[0].GetType());
						var legTile = (CartageLegTile)tile.BodyPanel.Controls[0];
						form.Show();
						AssertEquals("  20GP  T00001000/A", legTile.WhatLabel.Text);
						container.JC_RC = new RefContainer.Loader(Factory).LoadFromCode("40GP").PK;
						Factory.Save();
						AssertEquals("  20GP  T00001000/A", legTile.WhatLabel.Text);
						viewerControl.RefreshButton.PerformClick();
						tile = (RunSheetTile)viewerControl.BodyPanel.Controls[1];
						legTile = (CartageLegTile)tile.BodyPanel.Controls[0];
						AssertEquals("  40GP  T00001000/A", legTile.WhatLabel.Text);
					}
				}
				finally
				{
#if !WINZOR
					SafeNativeMethods.UnlockWindowUpdate(someOtherForm.Handle); // be sure to unlock here, otherwise we may get an exception in the above code and all other tests after this will fail
#endif
				}
			}
		}

		public void TestRefreshPerformClickForDifferentWorksheetStatus()
		{
			var runSheet = Factory.New<CommonWorkSheet>();
			runSheet.EY_StartTime = ZDateTime.Today;
			runSheet.EY_EndTime = ZDateTime.Today.AddDays(1);
			Factory.Save();
			var dashboardFactory = new BusinessObjectFactory();
			var dashboards = RunSheetDashboardCollection.New(dashboardFactory);
			var dashboard = dashboards[0];
			using (var form = new ZForm())
			{
				var dashboardControl = new RunSheetDashboardControl();
				form.Controls.Add(dashboardControl);
				dashboardControl.SetDataBinding(dashboards, "");
				dashboard.DateRangeFilter = "Today";
				var tile = dashboardControl.BodyPanel.Controls[1] as RunSheetTile;
				Assert("precondition : Tile should be created.", tile != null);
				dashboardControl.PerformSearchCore();
				tile = (RunSheetTile)dashboardControl.BodyPanel.Controls[1];
				AssertEquals("Precondition", CommonWorkSheet.ErrorStatuses.Working, runSheet.ErrorStatus);
				AssertEquals("Since worksheet is in working state tile color should be grey.", Color.FromArgb(117, 117, 117), tile.TileColor);
				runSheet.ErrorStatus = CommonWorkSheet.ErrorStatuses.Canceled;
				Factory.Save();
				dashboardControl.PerformSearchCore();
				tile = (RunSheetTile)dashboardControl.BodyPanel.Controls[1];
				AssertEquals("Since worksheet is cancelled it should be red.", Color.FromArgb(180, 0, 0), tile.TileColor);
				runSheet.ErrorStatus = CommonWorkSheet.ErrorStatuses.Working;
				Factory.Save();
				dashboardControl.PerformSearchCore();
				tile = (RunSheetTile)dashboardControl.BodyPanel.Controls[1];
				AssertEquals("Since worksheet is in working state tile color should be grey.", Color.FromArgb(117, 117, 117), tile.TileColor);
			}
		}

		public void TestAutoRefreshTimer()
		{
			var viewerFactory = new BusinessObjectFactory();
			var dashboards = RunSheetDashboardCollection.New(viewerFactory);
			var viewer = dashboards[0];
			viewer.DateRangeFilter = "Today";
			AutoRefreshManager.Instance.SetAutoRefreshTimeOut(ModuleIDs.CartageRunSheetDashboard, true, 0);
			using (var form = new RunSheetDashboardForm(dashboards))
			{
				var viewerControl = form.RunSheetDashboardControl;
				form.Show();
				viewerControl.AutoRefreshStatus = RunSheetDashboardControl.AutoRefreshStatusType.Running;
				Thread.Sleep(1200);
				Application.DoEvents();
				AssertGreaterThan(viewerControl.StopwatchForRefresh.Elapsed, TimeSpan.Zero);
				AssertLessThan(viewerControl.StopwatchForRefresh.Elapsed, TimeSpan.FromSeconds(1));
			}
		}

		public void TestAutoRefreshTimerShouldBeDisabledDuringDbUpgrade()
		{
			var viewerFactory = new BusinessObjectFactory();
			var dashboards = RunSheetDashboardCollection.New(viewerFactory);
			var viewer = dashboards[0];
			viewer.DateRangeFilter = "Today";
			AutoRefreshManager.Instance.SetAutoRefreshTimeOut(ModuleIDs.CartageRunSheetDashboard, true, 0);
			using (DbEnv.Instance.DisableTimerDuringDbUpgrade())
			using (var form = new RunSheetDashboardForm(dashboards))
			{
				var viewerControl = form.RunSheetDashboardControl;
				form.Show();
				viewerControl.AutoRefreshStatus = RunSheetDashboardControl.AutoRefreshStatusType.Running;
				Thread.Sleep(1200);
				Application.DoEvents();
				AssertGreaterThan(viewerControl.StopwatchForRefresh.Elapsed, TimeSpan.FromSeconds(1));
			}
		}

		// used for debugging
		//[StressTestAttribute]
		//public void TestRunSheetRefreshPerformance()
		//{
		//	CreatePerformanceData();
		//	Factory.Save();
		//	var viewerFactory = new BusinessObjectFactory();
		//	var dashboards = RunSheetDashboardCollection.New(viewerFactory);
		//	var viewer = dashboards[0];
		//	viewer.DateRangeFilter = "Today";
		//	// enable refresh - sets status label
		//	AutoRefreshManager.Instance.SetAutoRefreshTimeOut(ModuleIDs.CartageRunSheetDashboard, true, 5);
		//	using (var form = new RunSheetDashboardForm(dashboards))
		//	{
		//		var viewerControl = form.RunSheetDashboardControl;
		//		form.Show();
		//		var runSheetTiles = viewerControl.BodyPanel.Controls.Cast<Control>().Where(c => c.GetType() == typeof(RunSheetTile)).Count();
		//		AssertEquals(50, runSheetTiles);
		//		var start = ZDateTime.Now;
		//		viewerControl.PerformSearchCore();
		//		viewerControl.PerformSearchCore();
		//		var end = ZDateTime.Now;
		//		AssertEquals(string.Format("Took longer than 10 seconds to refresh. '{0}' seconds for 2 refreshes. Status '{1}'", (end - start).TotalSeconds, viewerControl.WorkSheetTileForm.AutoRefreshWarningLabel.Text),
		//			true, viewerControl.WorkSheetTileForm.AutoRefreshWarningLabel.Text.StartsWith("Refreshing in"));
		//	}
		//}
		//// Create 50 RunSheets each with 10 Legs
		//void CreatePerformanceData()
		//{
		//	var containerTypePK = new RefContainer.Loader(Factory).LoadFromCode("20GP").PK;
		//	for (int i = 0; i < 50; i++)
		//	{
		//		var runSheet = Factory.New<CommonWorkSheet>();
		//		runSheet.EY_StartTime = ZDateTime.Today;
		//		runSheet.EY_EndTime = ZDateTime.Today.AddDays(1);
		//		for (int x = 0; x < 10; x++)
		//		{
		//			var cartage = Factory.New<CommonCartage>();
		//			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLExportPack;
		//			var container = Factory.NewWithValidTestData<CommonContainer>();
		//			container.JC_RC = containerTypePK;
		//			cartage.Containers.Append(container);
		//			var move = Factory.NewWithValidTestData<CommonBookedCtgMove>();
		//			cartage.GetBookedMoves(container).Append(move);
		//			var leg = move.CartageLegs.AddNew();
		//			runSheet.CartageLegs.Add(leg);
		//			leg.JU_RunSheetSequence = 1;
		//		}
		//	}
		//}

		public void TestOneLeg()
		{
			// 1 runsheet, 1 leg
			var runSheet = Factory.New<CommonWorkSheet>();
			runSheet.EY_StartTime = ZDateTime.Today;
			runSheet.EY_EndTime = ZDateTime.Today.AddDays(1);
			var containerTypePK = new RefContainer.Loader(Factory).LoadFromCode("20GP").PK;
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLExportPack;
			var move = cartage.ContainerBookedMoves.AddNew();
			var container = move.Container;
			container.JC_RC = containerTypePK;
			var leg = move.CartageLegs.AddNew();
			runSheet.CartageLegs.Add(leg);
			leg.JU_RunSheetSequence = 1;
			Factory.Save();
			var viewerFactory = new BusinessObjectFactory();
			var dashboards = RunSheetDashboardCollection.New(viewerFactory);
			var viewer = dashboards[0];
			viewer.DateRangeFilter = "Today";
			// enable refresh - sets status label
			AutoRefreshManager.Instance.SetAutoRefreshTimeOut(ModuleIDs.CartageRunSheetDashboard, true, 5);
			using (var form = new RunSheetDashboardForm(dashboards))
			{
				var viewerControl = form.RunSheetDashboardControl;
				form.Show();
				var runSheetTiles = viewerControl.BodyPanel.Controls.Cast<Control>().Count(c => c.GetType() == typeof(RunSheetTile));
				AssertEquals(1, runSheetTiles);
				var start = ZDateTime.Now;
				viewerControl.PerformSearchCore();
				viewerControl.PerformSearchCore();
				var end = ZDateTime.Now;
				AssertEquals(string.Format("Took longer than 10 seconds to refresh. '{0}' seconds for 2 refreshes. Status '{1}'", (end - start).TotalSeconds, viewerControl.WorkSheetTileForm.AutoRefreshWarningLabel.Text), true, viewerControl.WorkSheetTileForm.AutoRefreshWarningLabel.Text.StartsWith("Refreshing in"));
			}
		}

		public void TestBindToNewFactory()
		{
			var containerType20GP = new RefContainer.Loader(Factory).LoadFromCode("20GP").PK;
			// RunSheet w/ 2 legs
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLExportPack;
			var move = cartage.ContainerBookedMoves.AddNew();
			var container = move.Container;
			container.JC_RC = containerType20GP;
			var leg1 = move.CartageLegs.AddNew();
			var leg2 = move.CartageLegs.AddNew();
			var leg3 = move.CartageLegs.AddNew();
			var runSheet = Factory.New<CommonWorkSheet>();
			var runSheet2 = Factory.New<CommonWorkSheet>();
			runSheet.EY_StartTime = ZDateTime.Today;
			runSheet.EY_EndTime = ZDateTime.Today.AddDays(1);
			runSheet.CartageLegs.Add(leg1);
			runSheet2.EY_StartTime = ZDateTime.Today;
			runSheet2.EY_EndTime = ZDateTime.Today.AddDays(1);
			runSheet2.CartageLegs.Add(leg2);
			var viewers = RunSheetDashboardCollection.New(Factory);
			var viewer = viewers[0];
			viewer.DateRangeFilter = "Today";
			var poke = viewer.RunSheets.Count; // load and cache
			Factory.Save();
			// same RunSheet in another Factory, but add the 3rd leg
			var factory2 = new BusinessObjectFactory();
			factory2.RefreshEnabled = false;
			var leg3_Factory2 = factory2.Load<CommonCartageLeg>(leg3.PK);
			var runSheet3_Factory2 = factory2.New<CommonWorkSheet>();
			runSheet3_Factory2.EY_StartTime = ZDateTime.Today;
			runSheet3_Factory2.EY_EndTime = ZDateTime.Today.AddDays(1);
			runSheet3_Factory2.CartageLegs.Add(leg3_Factory2);
			var viewers_Factory2 = RunSheetDashboardCollection.New(factory2);
			var viewer_Factory2 = viewers[0];
			viewer_Factory2.DateRangeFilter = "Today";
			poke = viewer_Factory2.RunSheets.Count;
			factory2.Save();
			using (var form = new ZForm())
			{
				form.Show();
				var dashBoard = new RunSheetDashboardControl();
				form.Controls.Add(dashBoard);
				AssertEquals("Precondition: Expected one ZLabel control.", 1, dashBoard.BodyPanel.Controls.Count);
				AssertEquals("Precondition: Expected one ZLabel control.", typeof(ZLabel), dashBoard.BodyPanel.Controls[0].GetType());
				dashBoard.SetDataBinding(viewers, "");
				var runSheetTiles = dashBoard.BodyPanel.Controls.Cast<Control>().Where(c => c is RunSheetTile).Cast<RunSheetTile>();
				AssertEquals("Should contain 2 RunSheet Tiles.", 2, runSheetTiles.Count());
				viewers.SwapFactoryRemoveAllAndAddNew();
				runSheetTiles = dashBoard.BodyPanel.Controls.Cast<Control>().Where(c => c is RunSheetTile).Cast<RunSheetTile>();
				AssertEquals("Should contain 3 RunSheet Tiles, as Factory2 knows about the 3rd RunSheet.", 3, runSheetTiles.Count());
			}
		}
	}
}
