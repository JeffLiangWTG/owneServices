using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.GUI
{
	public partial class CommodityGroupForm
	{
		ZGrid CommodityGroupsGrid;
		ZButton OKButton;

		new void InitializeComponent()
		{
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new ZTextBoxColumnStyleInfo();

			this.CommodityGroupsGrid = new ZGrid();
			this.OKButton = new ZButton();

			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.CommodityGroupsGrid)).BeginInit();
			this.CommodityGroupsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 407, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(548, 22, true);
			this.BindingSource.DataSourceType = typeof(CommodityGroupViewModelCollection);

			// 
			// commodityGroupsGrid
			// 
			this.CommodityGroupsGrid.AllowNavigation = false;
			this.CommodityGroupsGrid.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left;
			this.CommodityGroupsGrid.Dock = System.Windows.Forms.DockStyle.Top;

			this.BindingSource.SetBindingMember(this.CommodityGroupsGrid, ".");

			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(null);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CommodityGroupViewModel)(null)).UniversalGroup);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CommodityGroupViewModel)(null)).UniversalGroupDescription);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CommodityGroupViewModel)(null)).CommodityCode);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CommodityGroupViewModel)(null)).CommodityDescription);

			zTextBoxColumnStyleInfo1.ColumnName = "UniversalGroup";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);

			zTextBoxColumnStyleInfo2.ColumnName = "UniversalGroupDescription";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);

			zTextBoxColumnStyleInfo3.ColumnName = "CommodityCode";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);

			zTextBoxColumnStyleInfo4.ColumnName = "CommodityDescription";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);

			this.CommodityGroupsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.CommodityGroupsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.CommodityGroupsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.CommodityGroupsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);

			this.CommodityGroupsGrid.GridId = "D277FE49-78FA-4229-92D9-FB8F4351BB68";
			this.CommodityGroupsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.CommodityGroupsGrid.LayoutKey = "commodityGroupsGrid";
			this.CommodityGroupsGrid.Name = "commodityGroupsGrid";
			this.CommodityGroupsGrid.ReadOnly = true;
			this.CommodityGroupsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(550, 200, true);
			this.CommodityGroupsGrid.TabIndex = 1;
			// 
			// OKButton
			// 
			this.OKButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
			this.OKButton.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("CommodityGroupsForm|FFF4E147-6846-4080-92A3-F615B4DF2721", "&OK");
			this.OKButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(520, 205, true);
			this.OKButton.Name = "OKButton";
			this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 20, true);
			this.OKButton.TabIndex = 2;
			// 
			// CommodityGroupsForm
			//
			this.AcceptButton = this.OKButton;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("CommodityGroupsForm|99A2BE5E-310F-4483-81E9-A14578F63BF4", "Commodity Groups");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 250, true);
			this.Controls.Add(this.OKButton);
			this.Controls.Add(this.CommodityGroupsGrid);
			this.DataSourceAssemblyName = "Enterprise.Rating.Business";
			this.DataSourceType = typeof(CommodityGroupViewModelCollection);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "CommodityGroupsForm";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.Controls.SetChildIndex(this.CommodityGroupsGrid, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.CommodityGroupsGrid)).EndInit();
			this.CommodityGroupsGrid.ResumeLayout(false);
			this.CommodityGroupsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
