
namespace Enterprise.Customs.GUI
{
	partial class PackableItemSplitPartsForm
	{
		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfo1 = new Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.SplitterPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.TopPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ItemPartsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.BottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.OKButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SplitterPanel.SuspendLayout();
			this.TopPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ItemPartsGrid)).BeginInit();
			this.ItemPartsGrid.SuspendLayout();
			this.BottomPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 462, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(618, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Business.PackableItemsSplitter);
			// 
			// SplitterPanel
			// 
			this.SplitterPanel.Controls.Add(this.TopPanel);
			this.SplitterPanel.Controls.Add(this.BottomPanel);
			this.SplitterPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SplitterPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SplitterPanel.Name = "SplitterPanel";
			this.SplitterPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(618, 462, true);
			this.SplitterPanel.TabIndex = 1;
			// 
			// TopPanel
			// 
			this.TopPanel.Controls.Add(this.ItemPartsGrid);
			this.TopPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TopPanel.Name = "TopPanel";
			this.TopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(618, 426, true);
			this.TopPanel.TabIndex = 0;
			// 
			// ItemPartsGrid
			// 
			this.ItemPartsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ItemPartsGrid, "PackableItemParts");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.PackableItemsSplitter)(null)).PackableItemParts)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.Business.NonPersistentSplitPackItem)(((System.Collections.IList)(((Enterprise.Customs.Business.PackableItemsSplitter)(null)).PackableItemParts)).SyncRoot)).Sequence)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.NonPersistentSplitPackItem)(((System.Collections.IList)(((Enterprise.Customs.Business.PackableItemsSplitter)(null)).PackableItemParts)).SyncRoot)).GoodsDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.Business.NonPersistentSplitPackItem)(((System.Collections.IList)(((Enterprise.Customs.Business.PackableItemsSplitter)(null)).PackableItemParts)).SyncRoot)).PackableQuantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.NonPersistentSplitPackItem)(((System.Collections.IList)(((Enterprise.Customs.Business.PackableItemsSplitter)(null)).PackableItemParts)).SyncRoot)).PackableUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.Business.NonPersistentSplitPackItem)(((System.Collections.IList)(((Enterprise.Customs.Business.PackableItemsSplitter)(null)).PackableItemParts)).SyncRoot)).NetWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.NonPersistentSplitPackItem)(((System.Collections.IList)(((Enterprise.Customs.Business.PackableItemsSplitter)(null)).PackableItemParts)).SyncRoot)).NetWeightUQ)));
			this.ItemPartsGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "Sequence";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zMultiLineTextBoxColumnInfo1.ColumnName = "GoodsDescription";
			zMultiLineTextBoxColumnInfo1.MinimumEditControlWidth = 300;
			zMultiLineTextBoxColumnInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(300);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "PackableQuantity";
			zCalcEditColumnStyleInfo2.Decimals = 3;
			zCalcEditColumnStyleInfo2.GroupName = Enterprise.Customs.GUI.Res.GetData("56f6f856-b8ef-4bcd-b4ad-f6e5778daa9e", "Packable Quantity");
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zDropEditColumnStyleInfo1.ColumnName = "PackableUQ";
			zDropEditColumnStyleInfo1.GroupName = Enterprise.Customs.GUI.Res.GetData("56f6f856-b8ef-4bcd-b4ad-f6e5778daa9e", "Packable Quantity");
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.ColumnName = "NetWeight";
			zCalcEditColumnStyleInfo3.Decimals = 3;
			zCalcEditColumnStyleInfo3.GroupName = Enterprise.Customs.GUI.Res.GetData("d0a5b519-09e6-4f2f-ac2e-f7fedd8abf45", "Net Weight");
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zDropEditColumnStyleInfo2.ColumnName = "NetWeightUQ";
			zDropEditColumnStyleInfo2.GroupName = Enterprise.Customs.GUI.Res.GetData("d0a5b519-09e6-4f2f-ac2e-f7fedd8abf45", "Net Weight");
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.ItemPartsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.ItemPartsGrid.ColumnStyles.Add(zMultiLineTextBoxColumnInfo1);
			this.ItemPartsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.ItemPartsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.ItemPartsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.ItemPartsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.ItemPartsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ItemPartsGrid.GridId = "0efecaff-bb20-4976-9cde-710b22a9af7c";
			this.ItemPartsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ItemPartsGrid.LayoutKey = "zGrid1";
			this.ItemPartsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ItemPartsGrid.Name = "ItemPartsGrid";
			this.ItemPartsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(618, 426, true);
			this.ItemPartsGrid.TabIndex = 0;
			// 
			// BottomPanel
			// 
			this.BottomPanel.Controls.Add(this.OKButton);
			this.BottomPanel.Controls.Add(this.CancelButton);
			this.BottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 426, true);
			this.BottomPanel.Name = "BottomPanel";
			this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(618, 36, true);
			this.BottomPanel.TabIndex = 1;
			// 
			// OKButton
			// 
			this.OKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OKButton.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("9f4b9db2-f6ef-43d8-bfd6-40d4904bb492", "OK");
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(437, 6, true);
			this.OKButton.Name = "OKButton";
			this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.OKButton.TabIndex = 1;
			this.OKButton.ToolTipCaption = null;
			this.OKButton.UseVisualStyleBackColor = true;
			this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
			// 
			// CancelButton
			// 
			this.CancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelButton.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("37871403-c7d7-46b6-9d30-61629db8ad10", "Cancel");
			this.CancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(531, 6, true);
			this.CancelButton.Name = "CancelButton";
			this.CancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CancelButton.TabIndex = 2;
			this.CancelButton.ToolTipCaption = null;
			this.CancelButton.UseVisualStyleBackColor = true;
			// 
			// PackableItemSplitPartsForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("99636077-1658-47d3-93ea-b91192ecd8a1", "Split Pack Item");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(618, 486, true);
			this.Controls.Add(this.SplitterPanel);
			this.DataSourceType = typeof(Enterprise.Customs.Business.PackableItemsSplitter);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(634, 525, true);
			this.Name = "PackableItemSplitPartsForm";
			this.Text = "Split Pack Item";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.SplitterPanel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SplitterPanel.ResumeLayout(false);
			this.SplitterPanel.PerformLayout();
			this.TopPanel.ResumeLayout(false);
			this.TopPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ItemPartsGrid)).EndInit();
			this.ItemPartsGrid.ResumeLayout(false);
			this.ItemPartsGrid.PerformLayout();
			this.BottomPanel.ResumeLayout(false);
			this.BottomPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZPanel SplitterPanel;
		private ZArchitecture.GUI.ZPanel TopPanel;
		private ZArchitecture.ZGrid ItemPartsGrid;
		private ZArchitecture.GUI.ZPanel BottomPanel;
		private ZArchitecture.GUI.ZButton OKButton;
		private new ZArchitecture.GUI.ZButton CancelButton;
	}
}
