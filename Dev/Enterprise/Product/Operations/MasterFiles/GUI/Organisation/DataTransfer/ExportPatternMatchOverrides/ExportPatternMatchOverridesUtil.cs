using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.DataTransfer.Native.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public class ExportPatternMatchOverridesUtil
	{
		public ExportPatternMatchOverridesUtil(Func<IEnumerable<OrgHeader>> getOrgHeaders)
		{
			GetOrgHeaders = getOrgHeaders;
		}

		internal Func<IEnumerable<OrgHeader>> GetOrgHeaders;

		public Func<IEnumerable<OrgHeader>> GetOrgHeadersExposedForTesting => GetOrgHeaders;

		public void AddExportMenuItem(MenuItem menuItemParent, string previousMenuItemName)
		{
			menuItemParent.Popup += delegate
			{
				var newMenuName = "ExportPatternMatchOverrideMenuItem";
				if (menuItemParent.MenuItems.FindByName(newMenuName) == null)
				{
					var caption = ResString.GetMultilingualString("EE24544F-ADDE-41FE-82FF-9496904AD0F5", "Export Pattern Match Overrides as Native XML");
					var exportPatternMatchOverrideMenItem = new ZMenuItem(caption, ExportEventHandler) { Name = newMenuName };
					if (!string.IsNullOrEmpty(previousMenuItemName))
					{
						var previousMenuItem = menuItemParent.MenuItems.FindByName(previousMenuItemName);
						menuItemParent.MenuItems.Add(previousMenuItem.Index + 1, exportPatternMatchOverrideMenItem);
					}
					else
					{
						menuItemParent.MenuItems.Add(exportPatternMatchOverrideMenItem);
					}
				}
			};
		}

		void ExportEventHandler(object sender, EventArgs e)
		{
			var orgHeaders = GetOrgHeaders();

			if (!orgHeaders.Any())
			{
				GetNativeXmlExportService().Export(Array.Empty<BusinessObject>());
			}
			else
			{
				var overrides = orgHeaders.SelectMany(x => x.PatternMatchOverrides_ForBinding.ToList()).ToList();

				if (overrides.Any())
				{
					GetNativeXmlExportService().Export(overrides);
				}
				else if (orgHeaders.Count() > 1)
				{
					Globals.Message.ShowError(ResString.GetMultilingualString("49CE12FF-1A6E-48AB-AEA6-86AA7C8BF7F7", "No Pattern Match Overrides found on Orgs."));
				}
				else
				{
					Globals.Message.ShowError(ResString.GetMultilingualString("EFF933ED-5268-4310-A6AC-7AAB97933F73", "No Pattern Match Overrides found on Org."));
				}
			}
		}

		protected virtual IExportService GetNativeXmlExportService()
		{
			return ObjectFactory.GetDesignerSafe<IExportService>("NativeXmlExportService");
		}
	}
}
