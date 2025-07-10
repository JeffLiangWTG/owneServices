using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public class GuaranteeMessagingMenuItem : ZMenuItem
	{
		public GuaranteeMessagingMenuItem(BaseCusGuaranteeHeader header)
		{
			Argument.NotNull(header, nameof(header));
			CaptionResourceString = Res.GetData("8BB7D9F6-3A99-4A07-A324-8FB3D93F992F", "&Messaging");
			AddMenuItems(header);
		}
		GuaranteeMessagingMenuProvider messagingMenuProvider;

		protected override void OnPopup(EventArgs e)
		{
			base.OnPopup(e);
			messagingMenuProvider.RefreshMenu();
		}

		void AddMenuItems(BaseCusGuaranteeHeader header)
		{
			messagingMenuProvider = GuaranteeMessagingMenuProvider.GetProvider(header);
			MenuItems.AddRange(messagingMenuProvider.CreateMenuItems().ToArray<MenuItem>());
		}
	}
}
