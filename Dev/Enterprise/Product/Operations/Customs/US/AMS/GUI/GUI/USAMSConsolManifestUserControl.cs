using System;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.US.AMS.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.AMS.GUI
{
	public partial class USAMSConsolManifestUserControl : ZUserControl
	{
		public USAMSConsolManifestUserControl()
		{
			InitializeComponent();
		}

		public USAMSConsolManifestUserControl(CusInBondHeader header)
			: this()
		{
			Argument.NotNull(header, "header");
			this.header = header;
			header.OnOverrideFreightDefaultsUnchecking += new System.ComponentModel.CancelEventHandler(header_OnOverrideFreightDefaultsChanging);
			header.OnOverrideFreightDefaultsChanging += RefreshOverrideCheckBoxOnCommoditiesContextMenu;
			var isNVOCCHeader = header.IsNVOCCHeader;
			PTTTabPage.TabVisible = !isNVOCCHeader;
			AMSInBondUserControl.Visible = !isNVOCCHeader;
			AMSNVOCCInBondUserControl.Visible = isNVOCCHeader;
		}

		public void SelectAndShowBill(ZGuid bilPK)
		{
			if (header != null && BillsTabPage.TabVisible)
			{
				var bill = (CusInBondBill)header.Bills.FindByPK(bilPK);
				if (bill != null)
				{
					MainTabControl.SelectedTab = BillsTabPage;
					var manager = (CurrencyManager)BillsDetailsUserControl.BindingContext[header, "Bills"];
					var index = manager.List.IndexOf(bill);
					if (index >= 0)
					{
						manager.Position = index;
					}
				}
				else
				{
					var oceanBill = header.OceanBill;
					if (oceanBill != null && oceanBill.PK == bilPK)
					{
						MainTabControl.SelectedTab = MainTabPage;
					}
				}
			}
		}

		void RefreshOverrideCheckBoxOnCommoditiesContextMenu(object sender, EventArgs e)
		{
			var overrideMenu = BillsDetailsUserControl.BillContainerCommoditiesGrid.ContextMenu.MenuItems.FindByName("OverrideDefaultValuesMenuItem");
			if (overrideMenu != null)
			{
				overrideMenu.Checked = usamsMainUserControl1.OverrideFreightDefaultsCheckBox.Checked;
			}
		}

		void header_OnOverrideFreightDefaultsChanging(object sender, System.ComponentModel.CancelEventArgs e)
		{
			var result = Globals.Message.Show(Res.GetString("USAMSConsolManifestUserControl|F6F109D3-446B-471F-B8B8-72C0AA53ED3E", "Removing the override will reset your AMS data.\r\nYou will lose changes that you have made to the AMS data.\r\n\r\nProceed?"), Res.GetString("USAMSConsolManifestUserControl|253FB2BA-CBB0-4848-886A-839724972A6E", "Confirm"), MessageBoxButtons.YesNo, MessageBoxIcon.Question, DialogResult.Yes);
			e.Cancel = result == DialogResult.No;
		}

		readonly CusInBondHeader header;

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (header != null)
				{
					header.OnOverrideFreightDefaultsUnchecking -= new System.ComponentModel.CancelEventHandler(header_OnOverrideFreightDefaultsChanging);
					header.OnOverrideFreightDefaultsChanging -= RefreshOverrideCheckBoxOnCommoditiesContextMenu;
				}
			}

			base.Dispose(disposing);
		}
	}
}
