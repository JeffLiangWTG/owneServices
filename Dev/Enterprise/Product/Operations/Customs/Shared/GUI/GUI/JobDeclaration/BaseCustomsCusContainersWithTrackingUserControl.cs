using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.Customs.GUI
{
	public partial class BaseCustomsCusContainersWithTrackingUserControl /*SuppressCodeSmell Reason=The base class has the data source type name for bind to checking*/ : BaseCustomsCusContainersUserControl
	{
		#region Controls

		public Freight.GUI.ContainersUserControl containersUserControl1;

		#endregion

		public BaseCustomsCusContainersWithTrackingUserControl()
		{
			InitializeComponent();
			InitializeContainerTrackingUserControl();

			this.CusContainersBoundGrid.RowDeleteSecurityRightCheckStrategy = new CusContainerRowDeleteSecurityRightCheckStrategy();
			CusContainersBoundGrid.InnerGrid.GridId = "GridLayoutV6/wYdjdj5Cz4Nc7hGpbKA==";
		}

		class CusContainerRowDeleteSecurityRightCheckStrategy : GridRowDeleteSecurityRightCheckDefaultStrategy
		{
			protected override bool ShouldCheckSecurityRight(RowsDeletingEventArgs args)
			{
				return false;
			}
		}

		void InitializeContainerTrackingUserControl()
		{
			this.containersUserControl1 = GetContainerTrackingUserControl();
			// 
			// containersUserControl1
			// 
			this.containersUserControl1.CurrentContainer = null;
			this.containersUserControl1.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.containersUserControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 187);
			this.containersUserControl1.Name = "containersUserControl1";
			this.containersUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 415);
			this.containersUserControl1.TabIndex = 1;

			this.BaseAllPanel.Controls.Add(this.containersUserControl1);
		}

		protected virtual Freight.GUI.ContainersUserControl GetContainerTrackingUserControl()
		{
			return new Freight.GUI.ContainersUserControl();
		}

		#region OnLoad

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			if (!DesignModeFinder.IsDesigning)
			{
				AddContextMenuOptionsForAutoAssignContainerToInvoiceLinesToGrid();
			}
		}

		#endregion

		#region Auto Assign Container to Invoice lines - Additions for context menu

		MenuItem autoAssignContainersToInvoiceLinesSeperatorMenuItem;
		MenuItem autoAssignContainersToInvoiceLinesActionMenuItem;

		private void AddContextMenuOptionsForAutoAssignContainerToInvoiceLinesToGrid()
		{
			autoAssignContainersToInvoiceLinesSeperatorMenuItem = new ZMenuItem("-");
			autoAssignContainersToInvoiceLinesActionMenuItem = new ZMenuItem(ResString.GetMultilingualString("f0ffd1ef-d74e-4646-b57b-bb20f0e76794", "&Assign container to invoice lines"), AutoAssignContainerToInvoiceLines_Click);

			ContextMenu mnu = CusContainersBoundGrid.InnerGrid.ContextMenu;
			mnu.MenuItems.Add(autoAssignContainersToInvoiceLinesSeperatorMenuItem);
			mnu.MenuItems.Add(autoAssignContainersToInvoiceLinesActionMenuItem);
			mnu.Popup += new EventHandler(AutoAssignContainerToInvoiceLines_Popup);
		}

		private void AutoAssignContainerToInvoiceLines_Popup(object sender, EventArgs e)
		{
			if (autoAssignContainersToInvoiceLinesSeperatorMenuItem != null && autoAssignContainersToInvoiceLinesActionMenuItem != null)
			{
				bool anyInvoiceLinesCreated = JobDeclaration.InvoiceLines.Count > 0;
				bool singleContainerToAssign = CusContainersBoundGrid.InnerGrid.SelectedElements.Length == 1;
				var isMenuItemVisible = anyInvoiceLinesCreated && singleContainerToAssign && JobDeclaration.IsContainerInvoiceLinkRelevant;

				autoAssignContainersToInvoiceLinesSeperatorMenuItem.Visible = isMenuItemVisible;
				autoAssignContainersToInvoiceLinesActionMenuItem.Visible = isMenuItemVisible;
			}
		}

		private void AutoAssignContainerToInvoiceLines_Click(object sender, EventArgs e)
		{
			AutoAssignContainerToInvoiceLines();
		}

		protected void AutoAssignContainerToInvoiceLines()
		{
			CusContainersBoundGrid.SelectFirstRowIfOnlyRowInGrid();
			BusinessObject[] selected = CusContainersBoundGrid.InnerGrid.SelectedElements;
			if (selected.Length == 0)
			{
				CusContainersBoundGrid.ShowNotSelectedMessage();
			}
			else
			{
				foreach (BaseCusContainer container in selected)
				{
					foreach (BaseJobComInvoiceHeader invoice in JobDeclaration.Invoices)
					{
						invoice.AssignContainerToInvoiceLines(container.CO_ContainerNumber);
					}
				}
			}
		}
		#endregion

		#region LoadPlugin

		protected virtual void LoadPluginsCore()
		{
			containersUserControl1.SetupPlugIn(CusContainersBoundGrid.InnerGrid);
		}

		internal void LoadPlugins()
		{
			LoadPluginsCore();
		}

		#endregion

		#region HandleDeclarationControlVisibilityChangedCore

		protected override void HandleDeclarationControlVisibilityChangedCore()
		{
			base.HandleDeclarationControlVisibilityChangedCore();

			if (JobDeclaration.IsImport)
			{
				containersUserControl1.SelectImportTab();
			}
			else if (JobDeclaration.IsExport)
			{
				containersUserControl1.SelectExportTab();
			}

			if (JobDeclaration.Shipment == null && containersUserControl1.DetailTabControl.TabPages.Contains(containersUserControl1.OutturnTabPage))
			{
				containersUserControl1.DetailTabControl.TabPages.Remove(containersUserControl1.OutturnTabPage);
			}

			if (JobDeclaration.Shipment != null && containersUserControl1.DetailTabControl.TabPages.Contains(containersUserControl1.VGMTabPage))
			{
				containersUserControl1.DetailTabControl.TabPages.Remove(containersUserControl1.VGMTabPage);
			}

			if (containersUserControl1.DetailTabControl.TabPages.Contains(containersUserControl1.FreightRatesTabPage)) //We don't show SpotRates on Customs Declaration
			{
				containersUserControl1.DetailTabControl.TabPages.Remove(containersUserControl1.FreightRatesTabPage);
			}
		}

		#endregion

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (!containersUserControl1.DetailTabControl.TabPages.Contains(containersUserControl1.OutturnTabPage))
				{
					containersUserControl1.DetailTabControl.TabPages.Add(containersUserControl1.OutturnTabPage);
				}

				if (!containersUserControl1.DetailTabControl.TabPages.Contains(containersUserControl1.VGMTabPage))
				{
					containersUserControl1.DetailTabControl.TabPages.Add(containersUserControl1.VGMTabPage);
				}

				if (!containersUserControl1.DetailTabControl.TabPages.Contains(containersUserControl1.FreightRatesTabPage))
				{
					containersUserControl1.DetailTabControl.TabPages.Add(containersUserControl1.FreightRatesTabPage);
				}
			}

			base.Dispose(disposing);
		}

		#endregion

		#region Binding

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			BaseJobDeclaration declaration = (BaseJobDeclaration)dataSource;
			base.SetDataBinding(declaration == null ? null : declaration.CusContainers, "");
		}

		protected override BaseJobDeclaration DeclarationFromDataSource(IBusiness dataSource)
		{
			return
				base.DeclarationFromDataSource(dataSource) ??
				((ICusContainerCollection<BaseCusContainer>)dataSource).Declaration;
		}

		#endregion

	}
}
