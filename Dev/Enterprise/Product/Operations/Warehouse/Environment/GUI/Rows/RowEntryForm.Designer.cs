using System;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Environment.GUI
{
	public partial class RowEntryForm
	{
		ZButton LocationsButton;
		ZTemplateTabControl TabControl;
		ZTabPage TabPage;
		ZStmNoteTabPage zStmNoteTabPage1;
		ZLogsTabPage zEventTabPage1;
		ZGroupBox DetailsGroupBox;
		ZTextBox RowNameTextBox;
		ZCalcEdit ColumnsCalcEdit;
		ZCalcEdit zCalcEdit1;
		ZCalcEdit zCalcEdit2;
		ZLabel labelTips;
		ZGuidFindBox WarehouseGuidFindBox;
		ZCalcEdit RowPathSequenceCalcEdit;
		Enterprise.Core.Forms.ZPostingButtonsUserControl PostingButtonsUserControl;

		new void InitializeComponent()
		{
			this.PostingButtonsUserControl = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			this.LocationsButton = new ZButton();
			this.TabControl = new ZTemplateTabControl();
			this.TabPage = new ZTabPage();
			this.zStmNoteTabPage1 = new ZStmNoteTabPage();
			this.zEventTabPage1 = new ZLogsTabPage();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PostingButtonsUserControl.SuspendLayout();
			this.TabControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 356, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(830, 24, true);
			this.MainStatusBar.SizingGrip = false;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(517);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(517);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(WhsRow);
			// 
			// PostingButtonsUserControl
			// 
			this.PostingButtonsUserControl.AllowDrop = true;
			this.PostingButtonsUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.PostingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(495, 330, true);
			this.PostingButtonsUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.PostingButtonsUserControl.Name = "PostingButtonsUserControl";
			this.PostingButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(302, 25, true);
			this.PostingButtonsUserControl.TabIndex = 2;
			// 
			// LocationsButton
			// 
			this.LocationsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.LocationsButton.CaptionResourceString = Enterprise.Warehouse.Environment.GUI.Res.GetData("RowEntryForm|78ebfef4-c7ac-4e10-9905-c52f02274ccc", "Locations");
			this.LocationsButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(310, 330, true);
			this.LocationsButton.Name = "LocationsButton";
			this.LocationsButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.LocationsButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(125, 23, true);
			this.LocationsButton.TabIndex = 1;
			this.LocationsButton.ToolTipCaption = null;
			this.LocationsButton.Click += new EventHandler(this.LocationsButton_Click);
			// 
			// TabControl
			// 
			this.TabControl.Controls.Add(this.TabPage);
			this.TabControl.Controls.Add(this.zStmNoteTabPage1);
			this.TabControl.Controls.Add(this.zEventTabPage1);
			this.TabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TabControl.Name = "TabControl";
			this.TabControl.SelectedIndex = 0;
			this.TabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(810, 320, true);
			this.TabControl.TabIndex = 0;
			// 
			// TabPage
			// 
			this.TabPage.CaptionResourceString = Enterprise.Warehouse.Environment.GUI.Res.GetData("RowEntryForm|cc6b4018-c845-4929-8c7a-0a53a8235568", "Entry");
			this.TabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.TabPage.Name = "TabPage";
			this.TabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(802, 293, true);
			this.TabPage.TabIndex = 0;
			this.TabPage.RunWhenBindingOrFirstShown(new EventHandler(this.TabPage_InitializeTab));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((WhsRow)(null)).WR_Trays)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((WhsRow)(null)).WR_Levels)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((WhsRow)(null)).WR_Columns)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((WhsRow)(null)).WR_Name)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((WhsRow)(null)).WR_WW_Whs)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((WhsRow)(null)).WR_PickPathSequence)));
			// 
			// zStmNoteTabPage1
			// 
			this.zStmNoteTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.zStmNoteTabPage1.Name = "zStmNoteTabPage1";
			this.zStmNoteTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(802, 293, true);
			this.zStmNoteTabPage1.TabIndex = 1;
			// 
			// zEventTabPage1
			// 
			this.zEventTabPage1.ExcludeFromBindingOnSave = true;
			this.zEventTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.zEventTabPage1.Name = "zEventTabPage1";
			this.zEventTabPage1.ShouldBeReadOnlyInViewMode = false;
			this.zEventTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(802, 293, true);
			this.zEventTabPage1.TabIndex = 2;
			// 
			// RowEntryForm
			// 
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Warehouse.Environment.GUI.Res.GetData("RowEntryForm|ba16331a-71ef-4dd1-82b9-18db2b7167be", "Row");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(830, 380, true);
			this.Controls.Add(this.TabControl);
			this.Controls.Add(this.LocationsButton);
			this.Controls.Add(this.PostingButtonsUserControl);
			this.DataSourceAssemblyName = "Enterprise.Warehouse.Environment.Business";
			this.DataSourceType = typeof(WhsRow);
			this.DataSourceTypeName = "Enterprise.Warehouse.Environment.Business.WhsRow";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(830, 380, true);
			this.Name = "RowEntryForm";
			this.ShouldSerializeTabPageMethods = true;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.PostingButtonsUserControl, 0);
			this.Controls.SetChildIndex(this.LocationsButton, 0);
			this.Controls.SetChildIndex(this.TabControl, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PostingButtonsUserControl.ResumeLayout(true);
			this.PostingButtonsUserControl.PerformLayout();
			this.TabControl.ResumeLayout(false);
			this.TabControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		// This event handler isn't automatically generated, but appears to contain contents of an auto-generated function, and so has been placed in this file to avoid linter rules mangling it
		void TabPage_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.labelTips = new ZLabel();
			this.DetailsGroupBox = new ZGroupBox();
			this.zCalcEdit2 = new ZCalcEdit();
			this.zCalcEdit1 = new ZCalcEdit();
			this.ColumnsCalcEdit = new ZCalcEdit();
			this.RowNameTextBox = new ZTextBox();
			this.WarehouseGuidFindBox = new ZGuidFindBox();
			this.RowPathSequenceCalcEdit = new ZCalcEdit();
			this.TabPage.SuspendLayout();
			this.DetailsGroupBox.SuspendLayout();
			this.WarehouseGuidFindBox.SuspendLayout();
			this.TabPage.Controls.Add(this.labelTips);
			this.TabPage.Controls.Add(this.DetailsGroupBox);
			// 
			// labelTips
			// 
			this.labelTips.FontType = ((ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.labelTips.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(411, 18, true);
			this.labelTips.Name = "labelTips";
			this.labelTips.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(371, 215, true);
			this.labelTips.TabIndex = 1;
			this.labelTips.Text = "Tip: To define a single bulk location, enter the Location Identifier as the Row N" +
								  "ame and specify 1 (one) for Columns, Levels and Trays.";
			this.labelTips.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			// 
			// DetailsGroupBox
			// 
			this.DetailsGroupBox.CaptionResourceString = Enterprise.Warehouse.Environment.GUI.Res.GetData("RowEntryForm|76ac61fd-c605-4018-94bd-29de8fa4f487", "Details");
			this.DetailsGroupBox.Controls.Add(this.RowPathSequenceCalcEdit);
			this.DetailsGroupBox.Controls.Add(this.WarehouseGuidFindBox);
			this.DetailsGroupBox.Controls.Add(this.zCalcEdit2);
			this.DetailsGroupBox.Controls.Add(this.zCalcEdit1);
			this.DetailsGroupBox.Controls.Add(this.ColumnsCalcEdit);
			this.DetailsGroupBox.Controls.Add(this.RowNameTextBox);
			this.DetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 12, true);
			this.DetailsGroupBox.Name = "DetailsGroupBox";
			this.DetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(390, 221, true);
			this.DetailsGroupBox.TabIndex = 1;
			this.DetailsGroupBox.TabStop = false;
			// 
			// zCalcEdit2
			// 
			this.BindingSource.SetBindingMember(this.zCalcEdit2, "WR_Trays");
			this.zCalcEdit2.CaptionResourceString = Enterprise.Warehouse.Environment.GUI.Res.GetData("RowEntryForm|149c9abf-778b-4225-bb28-b51accd4d236", "Trays in Location");
			this.zCalcEdit2.DecimalPlaces = 0;
			this.zCalcEdit2.Decimals = 0;
			this.zCalcEdit2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(126, 152, true);
			this.zCalcEdit2.Name = "zCalcEdit2";
			this.zCalcEdit2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(76, 20, true);
			this.zCalcEdit2.TabIndex = 4;
			this.zCalcEdit2.Text = "0";
			this.zCalcEdit2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// zCalcEdit1
			// 
			this.BindingSource.SetBindingMember(this.zCalcEdit1, "WR_Levels");
			this.zCalcEdit1.CaptionResourceString = Enterprise.Warehouse.Environment.GUI.Res.GetData("RowEntryForm|4e978555-2845-409d-8a91-4b50f83abc38", "Levels in Row");
			this.zCalcEdit1.DecimalPlaces = 0;
			this.zCalcEdit1.Decimals = 0;
			this.zCalcEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(126, 120, true);
			this.zCalcEdit1.Name = "zCalcEdit1";
			this.zCalcEdit1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(76, 20, true);
			this.zCalcEdit1.TabIndex = 3;
			this.zCalcEdit1.Text = "0";
			this.zCalcEdit1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ColumnsCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.ColumnsCalcEdit, "WR_Columns");
			this.ColumnsCalcEdit.CaptionResourceString = Enterprise.Warehouse.Environment.GUI.Res.GetData("RowEntryForm|a16a907d-c191-47d0-9bcf-9fb053416306", "Columns in Row");
			this.ColumnsCalcEdit.DecimalPlaces = 0;
			this.ColumnsCalcEdit.Decimals = 0;
			this.ColumnsCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(126, 88, true);
			this.ColumnsCalcEdit.Name = "ColumnsCalcEdit";
			this.ColumnsCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(76, 20, true);
			this.ColumnsCalcEdit.TabIndex = 2;
			this.ColumnsCalcEdit.Text = "0";
			this.ColumnsCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// RowNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.RowNameTextBox, "WR_Name");
			this.RowNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(126, 56, true);
			this.RowNameTextBox.Name = "RowNameTextBox";
			this.RowNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.RowNameTextBox.TabIndex = 1;
			// 
			// WarehouseGuidFindBox
			// 
			this.WarehouseGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.WarehouseGuidFindBox, "WR_WW_Whs");
			this.WarehouseGuidFindBox.IsPrimaryKeyFromCodeRequired = false;
			this.WarehouseGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(126, 24, true);
			this.WarehouseGuidFindBox.Name = "WarehouseGuidFindBox";
			this.WarehouseGuidFindBox.PreBoundMaxLength = 3;
			this.WarehouseGuidFindBox.ShouldResize = true;
			this.WarehouseGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.WarehouseGuidFindBox.TabIndex = 0;
			// 
			// RowPathSequenceCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.RowPathSequenceCalcEdit, "WR_PickPathSequence");
			this.RowPathSequenceCalcEdit.DecimalPlaces = 0;
			this.RowPathSequenceCalcEdit.Decimals = 0;
			this.RowPathSequenceCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(126, 184, true);
			this.RowPathSequenceCalcEdit.Name = "RowPathSequenceCalcEdit";
			this.RowPathSequenceCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(76, 20, true);
			this.RowPathSequenceCalcEdit.TabIndex = 5;
			this.RowPathSequenceCalcEdit.Text = "0";
			this.RowPathSequenceCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.TabPage.PerformLayout();
			this.DetailsGroupBox.ResumeLayout(false);
			this.DetailsGroupBox.PerformLayout();
			this.WarehouseGuidFindBox.ResumeLayout(true);
			this.WarehouseGuidFindBox.PerformLayout();
			this.TabPage.ResumeLayout(true);
		}
	}
}
