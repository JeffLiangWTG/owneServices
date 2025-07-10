using System;
using System.Windows.Forms;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.GUI
{
	public sealed class CommercialInvoiceCustomsControlLockManager : BaseCustomsControlLockManager
	{
		public CommercialInvoiceCustomsControlLockManager(ICustomsFileParent fileParent, Control owner)
			: base(fileParent, owner) { }

		public static class TabPages
		{
			public static class Codes
			{
				public const string Lines = "LIN";
			}

			public static class Descriptions
			{
				public static MultilingualString Lines => ResString.GetMultilingualString("CEBF5C75-E2B1-4CC8-B1F2-FCCD4B9A5848", "Lines");
			}
		}

		protected override void OnDeclarationTypeChanged(object sender, EventArgs eventArgs) { }
		protected override string GetLockFunctionNotAvailableWhenParentIsReadOnly() => Res.GetString("D8FC2714-3BE9-400C-B388-2160F806B9CD", "The Commercial Invoice is Read-only, the function of lock is not available.");
		protected override string GetUnlockFunctionNotAvailableWhenParentIsReadOnly() => Res.GetString("FB85C0AE-FFDA-4074-B191-89AAC66F6F2C", "The Commercial Invoice is Read-only, the function of unlock is not available.");
		protected override DeclarationLockConfig GetConfigForDeclarationType()
		{
			var config = new DeclarationLockConfig();
			var tabInfos = config.TabInfos;
			AddTabPageInfo(tabInfos, TabPages.Codes.Lines, TabPages.Descriptions.Lines);
			return config;
		}

		void AddTabPageInfo(DeclarationTabLockInfoCollection tabInfos, string code, MultilingualString description)
		{
			var tabLockInfo = new DeclarationTabLockInfo();
			var tabPageList = tabLockInfo.Lookups.TabPageList;
			tabPageList.Clear();
			tabPageList.AddPair(code, description);
			tabInfos.Add(tabLockInfo);
			tabLockInfo.TabPage = code;
		}

		protected override string GetTabPagesLockedForEditMessage(string tabPageNames)
		{
			return Res.GetString("AD65FFDC-5F7C-42B6-B870-EE29640B6C1D",
				"These tab pages have been locked for edit.\r\n\r\n{0}\r\n\r\nYou can click the Brokerage - Unlock Commercial Invoice to unlock them.",
				tabPageNames);
		}
	}
}
