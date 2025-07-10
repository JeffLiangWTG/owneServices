using CargoWise.Types;
using Enterprise.Customs.GUI;

namespace Enterprise.Customs.TW.GUI
{
	public partial class CustomsBrokerageUserControl : BaseCustomsBrokerageUserControl
	{
		public CustomsBrokerageUserControl()
		{
			InitializeComponent();
			RenameTabPageCaptionResourceString();
			ReorderMainTabControlTabs();
		}

		void RenameTabPageCaptionResourceString()
		{
			EntryInstructionDetailsTabPage.CaptionResourceString = Res.GetData("6A7DC5C6-A597-45D3-BC7D-84089B49277E", "Licensing");
			MessagesTabPage.CaptionResourceString = Res.GetData("7C0567B8-441F-4BA3-B558-DE0043F61D1E", "Entry");
			PackingTabPage.CaptionResourceString = Res.GetData("8AA1C660-CEA3-4EED-AFB0-FA28297B4713", "Bills");
		}

		void ReorderMainTabControlTabs()
		{
			MainTabControl.SuspendLayout();
			var mainTabPages = MainTabControl.TabPages;
			var messagesTabPageIndex = mainTabPages.IndexOf(MessagesTabPage);
			mainTabPages.Remove(TWMessagesTabPage);
			mainTabPages.Insert(TWMessagesTabPage, messagesTabPageIndex + 1);
			MainTabControl.ResumeLayout(false);
			MainTabControl.PerformLayout();
		}

		protected override BaseCustomsEntryUserControl GetEntryInstructionUserControl()
		{
			return new ControllingMessageUserControl();
		}

		protected override BaseCustomsEntryUserControl GetDeclarationUserControl()
		{
			return new JobDeclarationUserControl();
		}

		protected override BaseCustomsCusContainersUserControl GetContainerUserControl()
		{
			return new ContainerUserControl();
		}

		protected override IBasePackingControl GetPackingUserControl()
		{
			return new CustomsBillsUserControl();
		}

		protected override void RemoveUserControlOfEachTabPage()
		{
			base.RemoveUserControlOfEachTabPage();
			RemoveControl(EntryInstructionDetailsTabPage);
		}

		protected override void ShowOrHidePackingTabPage()
		{
		}

		#region Create New User Controls for each tab

		protected override Customs.GUI.BaseCustomsSupplierHeaderUserControl GetSupplierHeaderUserControl()
		{
			Customs.GUI.BaseCustomsSupplierHeaderUserControl result;

			if (JobDeclaration.IsImport)
			{
				result = new ImportSupplierHeaderUserControl();
			}
			else
			{
				result = new ExportSupplierHeaderUserControl();
			}

			return result;
		}

		protected override Customs.GUI.BaseInvoiceLineUserControl GetInvoiceLinesUserControl()
		{
			Customs.GUI.BaseInvoiceLineUserControl result;

			if (JobDeclaration.IsImport)
			{
				result = new ImportInvoiceLineUserControl();
			}
			else
			{
				result = new ExportInvoiceLineUserControl();
			}

			return result;
		}

		protected override BaseMiscOptionsUserControl GetMiscOptionsUserControl()
		{
			return new MiscOptionsUserControl();
		}

		protected override BaseCustomsEntryUserControl GetMessageUserControl()
		{
			BaseCustomsEntryUserControl result;
			if (JobDeclaration?.IsImport ?? ZBool.False)
			{
				result = new ImportCustomsEntriesAndEntryLinesUserControl();
			}
			else if (JobDeclaration?.IsExport ?? ZBool.False)
			{
				result = new ExportCustomsEntriesAndEntryLinesUserControl();
			}
			else
			{
				result = new CustomsEntriesAndEntryLinesUserControl();
			}
			return result;
		}

		public override bool EntryInstructionsTabVisibleForCountry => true;

		#endregion

		protected override void OtherRegisters()
		{
			if (LockManager != null && fBaseCustomsEntryUserControl is JobDeclarationUserControl declarationUserControl)
			{
				LockManager.Register(Core.Constants.Customs.DeclarationTabPages.Codes.EntryDetails, declarationUserControl.EntryDetailsTabPage);
				LockManager.Register(Core.Constants.Customs.DeclarationTabPages.Codes.BondedDetails, declarationUserControl.BondedDetailsTabPage);
			}
		}
	}
}
