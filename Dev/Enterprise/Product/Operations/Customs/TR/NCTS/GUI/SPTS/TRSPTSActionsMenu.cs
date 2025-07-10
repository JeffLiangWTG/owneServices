using System;
using System.Collections.Generic;
using Enterprise.Customs.TR.Business;
using Enterprise.Customs.TR.GUI;
using Enterprise.Customs.TR.NCTS.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TR.NCTS.GUI
{
	public class TRSPTSActionsMenu : ZMenuItem
	{
		public TRSPTSActionsMenu(SPTSHeader sptsheader, ZMenuItem actionMenuItem) : base((NoResString)"Actions")
		{
			this.header = sptsheader;
			this.actionMenuItem = actionMenuItem;

			if (header != null)
			{
				BuildMenus();
			}
		}
		readonly SPTSHeader header;
		readonly ZMenuItem actionMenuItem;

		void BuildMenus()
		{
			var menuItems = new List<ZMenuItem>();
			AddMenuItem(header, menuItems, Captions.ManualRegistration, () => ManualRegistrationNoEntry());
			MenuItems.AddRange(menuItems.ToArray());
		}

		ZMenuItem AddMenuItem(SPTSHeader header, List<ZMenuItem> menuItems, ResourceString caption, Action pop)
		{
			var menuItem = new ZMenuItem(caption);
			menuItem.Click += delegate
			{
				pop();
			};
			menuItems.Add(menuItem);

			return menuItem;
		}

		void ManualRegistrationNoEntry()
		{
			var mainForm = actionMenuItem.GetMainMenu().GetForm() as ZForm;
			var header = mainForm.BusinessEntity as IRegistrationNoEntryProvider;
			ManualRegistrationHelper.ManualRegistrationNoEntry(header, mainForm);
		}

		static class Captions
		{
			public static ResourceString ManualRegistration => ResString.GetMultilingualString("1C2F321C-0F8B-44B0-B5A4-C76948CECCFA", "Manual Registration No Entry");
		}
	}
}
