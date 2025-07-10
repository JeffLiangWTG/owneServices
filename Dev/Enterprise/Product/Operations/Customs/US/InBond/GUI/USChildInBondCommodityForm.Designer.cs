namespace Enterprise.Customs.US.InBond.GUI
{
	partial class USChildInBondCommodityForm
	{
		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo1 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo6 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.BottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.closeButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.MainPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.CommodityRelationShipSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.ClassificationGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ClassificationGrid = new Enterprise.ZArchitecture.ZGrid();
			this.PartClassificationGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.PartClassificationGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.BottomPanel.SuspendLayout();
			this.MainPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.CommodityRelationShipSplitContainer)).BeginInit();
			this.CommodityRelationShipSplitContainer.Panel1.SuspendLayout();
			this.CommodityRelationShipSplitContainer.Panel2.SuspendLayout();
			this.CommodityRelationShipSplitContainer.SuspendLayout();
			this.ClassificationGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ClassificationGrid)).BeginInit();
			this.ClassificationGrid.SuspendLayout();
			this.PartClassificationGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.PartClassificationGrid)).BeginInit();
			this.PartClassificationGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 426, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(859, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.InBond.Business.CusInBondCargoDesc);
			// 
			// BottomPanel
			// 
			this.BottomPanel.Controls.Add(this.closeButton);
			this.BottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 390, true);
			this.BottomPanel.Name = "BottomPanel";
			this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(859, 36, true);
			this.BottomPanel.TabIndex = 4;
			// 
			// closeButton
			// 
			this.closeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.closeButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.closeButton.IsCaptionOverridden = true;
			this.closeButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(779, 6, true);
			this.closeButton.Name = "closeButton";
			this.closeButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.closeButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.closeButton.TabIndex = 0;
			this.closeButton.Text = "&Close";
			this.closeButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.closeButton.ToolTipCaption = null;
			this.closeButton.UseVisualStyleBackColor = true;
			// 
			// MainPanel
			// 
			this.MainPanel.Controls.Add(this.CommodityRelationShipSplitContainer);
			this.MainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainPanel.Name = "MainPanel";
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(859, 390, true);
			this.MainPanel.TabIndex = 5;
			// 
			// CommodityRelationShipSplitContainer
			// 
			this.CommodityRelationShipSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CommodityRelationShipSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CommodityRelationShipSplitContainer.Name = "CommodityRelationShipSplitContainer";
			this.CommodityRelationShipSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// CommodityRelationShipSplitContainer.Panel1
			// 
			this.CommodityRelationShipSplitContainer.Panel1.Controls.Add(this.ClassificationGroupBox);
			this.CommodityRelationShipSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(859, 390, true);
			this.CommodityRelationShipSplitContainer.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(150);
			// 
			// CommodityRelationShipSplitContainer.Panel2
			// 
			this.CommodityRelationShipSplitContainer.Panel2.Controls.Add(this.PartClassificationGroupBox);
			this.CommodityRelationShipSplitContainer.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(150);
			this.CommodityRelationShipSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(208);
			this.CommodityRelationShipSplitContainer.TabIndex = 2;
			// 
			// ClassificationGroupBox
			// 
			this.ClassificationGroupBox.Controls.Add(this.ClassificationGrid);
			this.ClassificationGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ClassificationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ClassificationGroupBox.Name = "ClassificationGroupBox";
			this.ClassificationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(859, 208, true);
			this.ClassificationGroupBox.TabIndex = 2;
			this.ClassificationGroupBox.TabStop = false;
			this.ClassificationGroupBox.Text = "Classification";
			// 
			// ClassificationGrid
			// 
			this.ClassificationGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ClassificationGrid, "ChildCommodities");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.InBond.Business.CusInBondCargoDesc)(null)).ChildCommodities)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.InBond.Business.CusInBondCargoDesc)(((System.Collections.IList)(((Enterprise.Customs.US.InBond.Business.CusInBondCargoDesc)(null)).ChildCommodities)).SyncRoot)).BY_OH_Supplier)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.InBond.Business.CusInBondCargoDesc)(((System.Collections.IList)(((Enterprise.Customs.US.InBond.Business.CusInBondCargoDesc)(null)).ChildCommodities)).SyncRoot)).BY_PartNumberForBinding)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.InBond.Business.CusInBondCargoDesc)(((System.Collections.IList)(((Enterprise.Customs.US.InBond.Business.CusInBondCargoDesc)(null)).ChildCommodities)).SyncRoot)).BY_FormattedHarmonisedTariffForBinding)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.InBond.Business.CusInBondCargoDesc)(((System.Collections.IList)(((Enterprise.Customs.US.InBond.Business.CusInBondCargoDesc)(null)).ChildCommodities)).SyncRoot)).BY_MonetaryValue)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.InBond.Business.CusInBondCargoDesc)(((System.Collections.IList)(((Enterprise.Customs.US.InBond.Business.CusInBondCargoDesc)(null)).ChildCommodities)).SyncRoot)).BY_GrossWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.InBond.Business.CusInBondCargoDesc)(((System.Collections.IList)(((Enterprise.Customs.US.InBond.Business.CusInBondCargoDesc)(null)).ChildCommodities)).SyncRoot)).BY_GrossWeightUnit)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.InBond.Business.CusInBondCargoDesc)(((System.Collections.IList)(((Enterprise.Customs.US.InBond.Business.CusInBondCargoDesc)(null)).ChildCommodities)).SyncRoot)).BY_InvoiceQuantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.InBond.Business.CusInBondCargoDesc)(((System.Collections.IList)(((Enterprise.Customs.US.InBond.Business.CusInBondCargoDesc)(null)).ChildCommodities)).SyncRoot)).BY_WarehouseEntryNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.InBond.Business.CusInBondCargoDesc)(((System.Collections.IList)(((Enterprise.Customs.US.InBond.Business.CusInBondCargoDesc)(null)).ChildCommodities)).SyncRoot)).BY_WarehouseEntryLineNo)));
			this.ClassificationGrid.CaptionVisible = false;
			zOrganisationFindBoxColumnStyleInfo1.ColumnName = "BY_OH_Supplier";
			zOrganisationFindBoxColumnStyleInfo1.IsVisible = false;
			zOrganisationFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCodeFindBoxColumnStyleInfo1.ColumnName = "BY_PartNumberForBinding";
			zCodeFindBoxColumnStyleInfo1.IsVisible = false;
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCodeFindBoxColumnStyleInfo2.ColumnName = "BY_FormattedHarmonisedTariffForBinding";
			zCodeFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "BY_MonetaryValue";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(102);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "BY_GrossWeight";
			zCalcEditColumnStyleInfo2.GroupName = Enterprise.Customs.US.InBond.GUI.Res.GetData("172ec269-6796-4878-9330-529c6e3a153b", "Weight");
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "BY_GrossWeightUnit";
			zDropEditColumnStyleInfo1.GroupName = Enterprise.Customs.US.InBond.GUI.Res.GetData("172ec269-6796-4878-9330-529c6e3a153b", "Weight");
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(38);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.ColumnName = "BY_InvoiceQuantity";
			zCalcEditColumnStyleInfo3.IsVisible = false;
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.ColumnName = "BY_WarehouseEntryNumber";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.ColumnName = "BY_WarehouseEntryLineNo";
			zCalcEditColumnStyleInfo4.IsVisible = false;
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(95);
			this.ClassificationGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo1);
			this.ClassificationGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.ClassificationGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo2);
			this.ClassificationGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.ClassificationGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.ClassificationGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.ClassificationGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.ClassificationGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ClassificationGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.ClassificationGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ClassificationGrid.GridId = "c43f20d7-7367-4275-875a-d69680353ac3";
			this.ClassificationGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ClassificationGrid.LayoutKey = "groupBox1";
			this.ClassificationGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.ClassificationGrid.Name = "ClassificationGrid";
			this.ClassificationGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(853, 189, true);
			this.ClassificationGrid.TabIndex = 1;
			this.ClassificationGrid.TabStop = false;
			// 
			// PartClassificationGroupBox
			// 
			this.PartClassificationGroupBox.Controls.Add(this.PartClassificationGrid);
			this.PartClassificationGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PartClassificationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PartClassificationGroupBox.Name = "PartClassificationGroupBox";
			this.PartClassificationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(859, 178, true);
			this.PartClassificationGroupBox.TabIndex = 3;
			this.PartClassificationGroupBox.TabStop = false;
			this.PartClassificationGroupBox.Text = "Part Classification - Multiple Tariffs associated to a Part (e.g. Sets, Assemblie" +
    "s, Watches)";
			// 
			// PartClassificationGrid
			// 
			this.PartClassificationGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.PartClassificationGrid, "ChildCommodities.ChildCommodities");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.InBond.Business.CusInBondCargoDesc)(((System.Collections.IList)(((Enterprise.Customs.US.InBond.Business.CusInBondCargoDesc)(null)).ChildCommodities)).SyncRoot)).ChildCommodities)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.InBond.Business.CusInBondCargoDesc)(((System.Collections.IList)(((Enterprise.Customs.US.InBond.Business.CusInBondCargoDesc)(((System.Collections.IList)(((Enterprise.Customs.US.InBond.Business.CusInBondCargoDesc)(null)).ChildCommodities)).SyncRoot)).ChildCommodities)).SyncRoot)).BY_FormattedHarmonisedTariff)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.InBond.Business.CusInBondCargoDesc)(((System.Collections.IList)(((Enterprise.Customs.US.InBond.Business.CusInBondCargoDesc)(((System.Collections.IList)(((Enterprise.Customs.US.InBond.Business.CusInBondCargoDesc)(null)).ChildCommodities)).SyncRoot)).ChildCommodities)).SyncRoot)).BY_MonetaryValue)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.InBond.Business.CusInBondCargoDesc)(((System.Collections.IList)(((Enterprise.Customs.US.InBond.Business.CusInBondCargoDesc)(((System.Collections.IList)(((Enterprise.Customs.US.InBond.Business.CusInBondCargoDesc)(null)).ChildCommodities)).SyncRoot)).ChildCommodities)).SyncRoot)).BY_GrossWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.InBond.Business.CusInBondCargoDesc)(((System.Collections.IList)(((Enterprise.Customs.US.InBond.Business.CusInBondCargoDesc)(((System.Collections.IList)(((Enterprise.Customs.US.InBond.Business.CusInBondCargoDesc)(null)).ChildCommodities)).SyncRoot)).ChildCommodities)).SyncRoot)).BY_GrossWeightUnit)));
			this.PartClassificationGrid.CaptionVisible = false;
			zCodeFindBoxColumnStyleInfo3.ColumnName = "BY_FormattedHarmonisedTariff";
			zCodeFindBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo5.ColumnName = "BY_MonetaryValue";
			zCalcEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(102);
			zCalcEditColumnStyleInfo6.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo6.ColumnName = "BY_GrossWeight";
			zCalcEditColumnStyleInfo6.GroupName = Enterprise.Customs.US.InBond.GUI.Res.GetData("d931e225-d6ea-48eb-b897-8ff888a23e63", "Weight");
			zCalcEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zDropEditColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo2.ColumnName = "BY_GrossWeightUnit";
			zDropEditColumnStyleInfo2.GroupName = Enterprise.Customs.US.InBond.GUI.Res.GetData("d931e225-d6ea-48eb-b897-8ff888a23e63", "Weight");
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(38);
			this.PartClassificationGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo3);
			this.PartClassificationGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.PartClassificationGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo6);
			this.PartClassificationGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.PartClassificationGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PartClassificationGrid.GridId = "c43f20d7-7367-4275-875a-d69680353ac3";
			this.PartClassificationGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.PartClassificationGrid.LayoutKey = "groupBox1";
			this.PartClassificationGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.PartClassificationGrid.Name = "PartClassificationGrid";
			this.PartClassificationGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(853, 159, true);
			this.PartClassificationGrid.TabIndex = 1;
			this.PartClassificationGrid.TabStop = false;
			// 
			// USChildInBondCommodityForm
			// 
			this.CancelButton = this.closeButton;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(859, 450, true);
			this.Controls.Add(this.MainPanel);
			this.Controls.Add(this.BottomPanel);
			this.DataSourceType = typeof(Enterprise.Customs.US.InBond.Business.CusInBondCargoDesc);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(700, 400, true);
			this.Name = "USChildInBondCommodityForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.BottomPanel, 0);
			this.Controls.SetChildIndex(this.MainPanel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.BottomPanel.ResumeLayout(false);
			this.BottomPanel.PerformLayout();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			this.CommodityRelationShipSplitContainer.Panel1.ResumeLayout(false);
			this.CommodityRelationShipSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.CommodityRelationShipSplitContainer)).EndInit();
			this.CommodityRelationShipSplitContainer.ResumeLayout(false);
			this.CommodityRelationShipSplitContainer.PerformLayout();
			this.ClassificationGroupBox.ResumeLayout(false);
			this.ClassificationGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ClassificationGrid)).EndInit();
			this.ClassificationGrid.ResumeLayout(false);
			this.ClassificationGrid.PerformLayout();
			this.PartClassificationGroupBox.ResumeLayout(false);
			this.PartClassificationGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.PartClassificationGrid)).EndInit();
			this.PartClassificationGrid.ResumeLayout(false);
			this.PartClassificationGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZPanel BottomPanel;
		private ZArchitecture.GUI.ZButton closeButton;
		private ZArchitecture.GUI.ZPanel MainPanel;
		private CargoWise.Windows.UI.KSplitContainer CommodityRelationShipSplitContainer;
		private ZArchitecture.GUI.ZGroupBox ClassificationGroupBox;
		private ZArchitecture.ZGrid ClassificationGrid;
		private ZArchitecture.GUI.ZGroupBox PartClassificationGroupBox;
		private ZArchitecture.ZGrid PartClassificationGrid;

	}
}
