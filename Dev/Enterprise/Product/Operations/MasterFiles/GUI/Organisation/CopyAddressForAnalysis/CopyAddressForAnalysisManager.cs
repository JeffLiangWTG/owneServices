using System.Windows.Forms;
using CargoWise.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	class CopyAddressForAnalysisManager
	{
		MenuItem CopyAddressForAnalysis { get; set; }

		public void CreateOrgMenu(ZForm parentForm, OrgHeader organisation)
		{
			Argument.NotNull(parentForm, "Parent Form");
			Argument.NotNull(organisation, "Organisation");

			CopyAddressForAnalysis = new ZMenuItem(
				ResString.GetMultilingualString("B30CFBBC-6DB3-41DC-816E-F6005CF18C75", "Copy Address For Analysis"),
				delegate
				{
					var orgAddress = (parentForm as BaseOrganisationsForm)?.OrgAddressForAnalysis;
					SafeClipboard.SetText(OrgAddress.ConvertAddressToAnalysisText(orgAddress));
				})
			{
				Name = (NoResString)"Copy Address For Analysis",
				Shortcut = Shortcut.CtrlR
			};

			IFileMenuItemsProvider menuProvider = parentForm;

			int len = menuProvider.ActionsMenuItem.MenuItems.Count - 1;
			while (len > 0)
			{
				MenuItem item = menuProvider.ActionsMenuItem.MenuItems[len];
				if (item.Text.Equals("-"))
				{
					menuProvider.ActionsMenuItem.MenuItems.Remove(item);
					break;
				}
			}

			ZFormMenuStrategy.AddActionsMenuItem(parentForm, CopyAddressForAnalysis);
		}
	}
}
