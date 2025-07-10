using System;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.Freight.LocalCartage.GUI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.LocalCartage.Module.Testing
{
	[TestedType(typeof(CartageWorkSheetModule))]
	public class CartageWorkSheetModuleTest : ZModuleBasherTest
	{
		public void TestModuleFilterPerformance_ActiveBizo()
		{
			for (var c = 0; c < 50; c++)
			{
				var cartage = Factory.New<CommonCartage>();
				cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLCFStoCTO;
				cartage.JJ_ConsignmentID = "T000" + c;
				var move = cartage.ContainerBookedMoves.AddNew();
				var container = move.Container;
				container.JC_ContainerNum = "C00" + c;
				var leg = move.CartageLegs.AddNew();
				var runSheet = Factory.New<CommonWorkSheet>();
				runSheet.CartageLegs.Add(leg);
			}

			Factory.Save();
			using (var module = new CartageWorkSheetModule())
			{
				var iModule = (IFilterModuleInternalsForTesting)module;
				AssertEquals(0, iModule.GridCollection.Count);
				using (var form = module.ShowPopup())
				{
					var filters = (CartageWorkSheetFilterStripBusinessObject)iModule.FilterBusinessObject;
					var filter = ((ModuleTextFilter)filters["Container #"]);
					filter.Property = "C";
					filter.IsActive = true;
					var factory_Initial = iModule.GridCollection.Factory;
					factory_Initial.ResetDatabaseLoadCount();
					iModule.PerformSearch();
					AssertEquals(50, iModule.GridCollection.Count);
					var factory_1stSearch = iModule.GridCollection.Factory;
					// GenCustomColumnDefinition: 1
					// Hits: 1/0
					AssertMaxDbHits(1, factory_Initial);
					// JobWorkSheet: 1
					// JobBookedCtgMove: 1
					// JobCartage: 1
					// JobContainerLegs: 1
					// Hits: 4/0
					AssertMaxDbHits(4, factory_1stSearch);
					factory_Initial.ResetDatabaseLoadCount();
					factory_1stSearch.ResetDatabaseLoadCount();
					filter.Property = "BLA";
					iModule.PerformSearch();
					AssertEquals(0, iModule.GridCollection.Count);
					var factory_2ndSearch = iModule.GridCollection.Factory;
					AssertMaxDbHits(0, factory_Initial);
					AssertMaxDbHits(0, factory_1stSearch);
					// JobWorkSheet: 1
					// Hits: 1/0
					AssertMaxDbHits("Should not be 55", 1, factory_2ndSearch);
				}
			}
		}

		public void TestStrategyProviderSetForInModule()
		{
			using (CartageWorkSheetModule module = new CartageWorkSheetModule())
			{
				Assert(CommonCartageBehaviorStrategyProvider.GetProvider(module.GridCollection.Factory).GetType().IsAssignableFrom(typeof(CartageBehaviorStrategyProvider)));
			}
		}

		[TestDate]
		public void TestWeeklyMenus()
		{
			using (CartageWorkSheetModule module = new CartageWorkSheetModule())
			{
				ZDateTime now = ZDateTime.Now;
				MenuItem newMenu = module.FormActionMenu.FindByText("&New");
				AssertEquals(7, newMenu.MenuItems.Count);
				AssertEquals("Today", newMenu.MenuItems[0].Text);
				AssertEquals("Tomorrow", newMenu.MenuItems[1].Text);
				AssertEquals(now.AddDays(2).ToString("dddd"), newMenu.MenuItems[2].Text);
				AssertEquals(now.AddDays(3).ToString("dddd"), newMenu.MenuItems[3].Text);
				AssertEquals(now.AddDays(4).ToString("dddd"), newMenu.MenuItems[4].Text);
				AssertEquals(now.AddDays(5).ToString("dddd"), newMenu.MenuItems[5].Text);
				AssertEquals(now.AddDays(6).ToString("dddd"), newMenu.MenuItems[6].Text);
				TestDateAttribute.Date = now.AddDays(1).ToDateTime();
				typeof(MenuItem).InvokeMember("OnPopup", BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.NonPublic, null, newMenu, new object[] { EventArgs.Empty });
				AssertEquals(7, newMenu.MenuItems.Count);
				AssertEquals("Today", newMenu.MenuItems[0].Text);
				AssertEquals("Tomorrow", newMenu.MenuItems[1].Text);
				AssertEquals(now.AddDays(3).ToString("dddd"), newMenu.MenuItems[2].Text);
				AssertEquals(now.AddDays(4).ToString("dddd"), newMenu.MenuItems[3].Text);
				AssertEquals(now.AddDays(5).ToString("dddd"), newMenu.MenuItems[4].Text);
				AssertEquals(now.AddDays(6).ToString("dddd"), newMenu.MenuItems[5].Text);
				AssertEquals(now.AddDays(7).ToString("dddd"), newMenu.MenuItems[6].Text);
			}
		}

		public void TestRunSheetDashboardMenu()
		{
			using (CartageWorkSheetModule module = new CartageWorkSheetModule())
			{
				ZDateTime now = ZDateTime.Now;
				MenuItem runSheetMenuMenu = module.FormActionMenu.FindByText("Run Sheet Dashboard");
				AssertNotNull(runSheetMenuMenu);
				runSheetMenuMenu.PerformClick();
				AssertNotNull(module.lastRunSheetDashboardForm);
				AssertEquals(typeof(RunSheetDashboardForm), module.lastRunSheetDashboardForm.GetType());
				module.lastRunSheetDashboardForm.Dispose();
			}
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.CartageWorkSheet;
		}
	}
}
