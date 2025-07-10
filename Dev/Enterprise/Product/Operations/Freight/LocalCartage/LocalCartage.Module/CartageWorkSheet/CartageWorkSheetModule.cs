using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.LocalCartage.Module
{
	public class CartageWorkSheetModule : ZFilterGridModule
	{
		public CartageWorkSheetModule()
		{
		}

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.CartageWorkSheet; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.CartageWorkSheet);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new CartageWorkSheetFilterControl(GridCollection, FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new ModuleCartageRunSheetCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new CartageWorkSheetFilterStripBusinessObject();
		}

		public override bool AllowDelete
		{
			get { return false; }
		}

		protected override MenuItem[] GetNewStandardMenuItems()
		{
			var menuItems = base.GetNewStandardMenuItems();

			if (NewMenuItem != null)
			{
				NewMenuItem.Popup += new EventHandler(newMenu_Popup);

				weeklyMenus = new List<MenuItem>();
				for (var i = 0; i < 7; i++)
				{
					var localI = i;
					var dayMenu = new ZMenuItem(GetMenuText(i), delegate
					{ MenuClick(localI); });
#if DEBUG
					TypeDescriptor.AddAttributes(dayMenu, new SuppressFormsLocalizedTestAttribute());
#endif

					weeklyMenus.Add(dayMenu);
					NewMenuItem.MenuItems.Add(dayMenu);
				}
			}
			return menuItems;
		}
		List<MenuItem> weeklyMenus;

		void newMenu_Popup(object sender, EventArgs e)
		{
			for (int i = 0; i < weeklyMenus.Count; i++)
			{
				weeklyMenus[i].Text = GetMenuText(i);
			}
		}

		MultilingualString GetMenuText(int i)
		{
			if (i == 0)
			{
				return ResString.GetMultilingualString("ca96ed7e-aa2e-4ae7-ae7c-b89f832dc730", "Today");
			}
			else if (i == 1)
			{
				return ResString.GetMultilingualString("be3a12d6-e772-476c-874c-1ff9af229501", "Tomorrow");
			}
			else
			{
				return (NoResString)ZDateTime.Today.AddDays(i).ToString("dddd", CultureInfo.CurrentCulture);
			}
		}

		void MenuClick(int i)
		{
			new CartageWorkSheetController(ZDateTime.Today.AddDays(i)).ShowNewForm();
		}

		protected override MenuItem[] GetNewAdditionalMenuItems()
		{
			List<MenuItem> result = new List<MenuItem>(base.GetNewAdditionalMenuItems());
			MenuItem runSheetDashboardItem = new ZMenuItem(ResString.GetMultilingualString("c50a3950-d1af-4791-9662-c7b8bd759d9f", "Run Sheet Dashboard"), delegate
			{ RunSheetDashboardMenuClick(); });
			result.Add(runSheetDashboardItem);
			return result.ToArray();
		}

		void RunSheetDashboardMenuClick()
		{
#if DEBUG
			lastRunSheetDashboardForm =
#endif
				ZControllerFactory.Create(ControllerIDs.CartageRunSheetDashboard).ShowNewForm();
		}

#if DEBUG
		internal IZForm lastRunSheetDashboardForm;
#endif

		protected override BusinessObjectFactory GetNewFactory()
		{
			BusinessObjectFactory moduleFactory = base.GetNewFactory();
			CommonCartageBehaviorStrategyProvider.SetProvider(moduleFactory, new CartageBehaviorStrategyProvider()); //DocumentSupport
			return moduleFactory;
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.LocalTransport; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.LocalTransportRunSheet; }
		}
	}
}
