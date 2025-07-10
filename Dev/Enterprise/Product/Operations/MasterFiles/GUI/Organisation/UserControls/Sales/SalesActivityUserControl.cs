using System;
using System.Windows.Forms;
using Enterprise.MarketingManager.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.GUI
{
	public partial class SalesActivityUserControl : ZUserControl
	{
		public SalesActivityUserControl()
		{
			InitializeComponent();
		}

		internal ZFilterGridModule OrgSalesDashboardModule;
		internal ZFilterStripCommonControl FilterStripControl;

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (!filterGridAdded)
			{
				var org = dataSource as OrgHeader;
				if (org != null)
				{
					AddFilterGrid(org);
				}
			}

			base.SetDataBinding(dataSource, dataMember);
		}

		void AddFilterGrid(OrgHeader org)
		{
			filterGridAdded = true;

			OrgSalesDashboardModule = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.OrgSalesDashboard);
			((IOrgSalesDashboardModule)OrgSalesDashboardModule).Org = org;

			FilterStripControl = (ZFilterStripCommonControl)OrgSalesDashboardModule.EmbeddedControl;
			FilterStripControl.Dock = DockStyle.Fill;
			FilterPanel.Controls.Add(FilterStripControl);

			FilterStripControl.SetDataBinding(FilterStripControl.GridCollection, "");
		}
		bool filterGridAdded;

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (!DesignModeFinder.IsDesigning)
			{
				if (OrgSalesDashboardModule != null)
				{
					foreach (var meunItem in OrgSalesDashboardModule.FormActionMenu)
					{
						ToolStrip.Items.Add(MenuItemToToolStripItemConverter.ConvertMenuItemToToolStripItem(meunItem, OrgSalesDashboardModule));
					}
				}
			}
		}

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (OrgSalesDashboardModule != null)
			{
				OrgSalesDashboardModule.Dispose();
			}

			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}
	}
}
