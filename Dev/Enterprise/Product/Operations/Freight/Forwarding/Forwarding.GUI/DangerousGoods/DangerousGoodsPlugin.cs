using System;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Licensing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Freight.Forwarding.GUI.DangerousGoods
{
	public class DangerousGoodsPlugin : ZPlugIn
	{
		readonly IUrlGenerator _urlGenerator;

		public DangerousGoodsPlugin(IBusiness hostBusinessEntity, IUrlGenerator urlGenerator) : base(hostBusinessEntity)
		{
			Argument.NotNull(hostBusinessEntity, nameof(hostBusinessEntity));
			Argument.NotNull(urlGenerator, nameof(urlGenerator));
			_urlGenerator = urlGenerator;
		}

		public override string Name => Res.GetString("675f188c-fa45-40d2-a416-daa593f6c666", "Dangerous Goods Portal");

		protected override ZBool HasUserControl => false;

		protected override LicenceCheckpoint LicenceCheckPoint => Env.Licence.AlwaysAllow;

		protected override MenuItem GetNewTopLevelMenu()
		{
			var actionLevelMenu = Form?.Menu?.MenuItems[ZFormMenuStrategy.ActionsMenuItemName];
			if (actionLevelMenu == null)
			{
				return null;
			}
			var openDangerousGoodsPortalMenuItem = new ZMenuItem(ResString.GetMultilingualString("729f3a50-bf87-472e-ab3f-ad0fe5c86d2a", "Dangerous Goods Portal"));
			openDangerousGoodsPortalMenuItem.Click += OpenDangerousGoodsPortalMenuItem_Click;
			actionLevelMenu.MenuItems.Add(openDangerousGoodsPortalMenuItem);
			return actionLevelMenu;
		}

		void OpenDangerousGoodsPortalMenuItem_Click(object sender, EventArgs e)
		{
			if (HostBusinessEntity is IDGPortalLaunchErrorProvider errorProvider && !errorProvider.DGPortalLaunchError.IsEmpty)
			{
				Globals.Message.ShowError(errorProvider.DGPortalLaunchError);
				return;
			}

			var url = _urlGenerator.GenerateUrl(HostBusinessEntity);
			if (url != null)
			{
				WebUrlLauncher.Launch(url);
			}
		}
	}
}
