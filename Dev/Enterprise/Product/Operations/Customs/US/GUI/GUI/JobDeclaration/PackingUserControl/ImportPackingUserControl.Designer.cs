using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.GUI
{
	public partial class ImportPackingUserControl
	{
		void InitializeComponent()
		{
			ZCheckBoxColumnStyleInfo splitCheckBoxColumn = new ZCheckBoxColumnStyleInfo();
			ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new ZCodeFindBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new ZTextBoxColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new ZDropEditColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new ZDropEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new ZCalcEditColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new ZDropEditColumnStyleInfo();
			ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfo1 = new ZMultiLineTextBoxColumnInfo();
			ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfo2 = new ZMultiLineTextBoxColumnInfo();
			ZCheckBoxColumnStyleInfo expressTrackingCheckBoxColumn = new ZCheckBoxColumnStyleInfo();
			this.bottomPanel = new ZPanel();
			this.BillOfLadingTabControl = new ZTabControl();
			this.packagesTabPage = new ZTabPage();
			this.PackagesGrid = new ZGrid();
			this.ITNosSplitTabPage = new ZTabPage();
			this.itAndSplitDetailsUserControl = new ITDetailsUserControl();
			this.FTZTabPage = new ZTabPage();
			this.pTTGroupBox = new ZGroupBox();
			this.remarksTextBox = new ZTextBox();
			this.importStatusGroupBox = new ZGroupBox();
			this.pTTCarrierGuidFindBox = new ZGuidFindBox();
			this.uS_UC_NKCountryOfExportCodeFindBox = new ZCodeFindBox();
			this.portOfLoadingSchDFindBox = new ZCodeFindBox();
			this.locationOfGoodsCodeFindBox = new ZCodeFindBox();
			this.fZ10GroupBox = new ZGroupBox();
			this.fZ10RemarksTextBox = new ZTextBox();
			this.pTTUniqueIDTextBox = new ZTextBox();
			this.horizontalSplitter = new CargoWise.Windows.UI.KSplitter();
			this.HouseBillPanel.SuspendLayout();
			this.BillFilterByAndGridPanel.SuspendLayout();
			this.FilterByPanel.SuspendLayout();
			this.BillGroupBoxPanel.SuspendLayout();
			this.HouseBillsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.HouseBillsGrid)).BeginInit();
			this.HouseBillsGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.bottomPanel.SuspendLayout();
			this.BillOfLadingTabControl.SuspendLayout();
			this.packagesTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.PackagesGrid)).BeginInit();
			this.PackagesGrid.SuspendLayout();
			this.ITNosSplitTabPage.SuspendLayout();
			this.itAndSplitDetailsUserControl.SuspendLayout();
			this.FTZTabPage.SuspendLayout();
			this.pTTGroupBox.SuspendLayout();
			this.importStatusGroupBox.SuspendLayout();
			this.pTTCarrierGuidFindBox.SuspendLayout();
			this.uS_UC_NKCountryOfExportCodeFindBox.SuspendLayout();
			this.portOfLoadingSchDFindBox.SuspendLayout();
			this.locationOfGoodsCodeFindBox.SuspendLayout();
			this.fZ10GroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// HouseBillPanel
			// 
			this.HouseBillPanel.Controls.Add(this.bottomPanel);
			this.HouseBillPanel.Controls.Add(this.horizontalSplitter);
			this.HouseBillPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1006, 582, true);
			this.HouseBillPanel.Controls.SetChildIndex(this.BillFilterByAndGridPanel, 0);
			this.HouseBillPanel.Controls.SetChildIndex(this.horizontalSplitter, 0);
			this.HouseBillPanel.Controls.SetChildIndex(this.bottomPanel, 0);
			// 
			// BillFilterByAndGridPanel
			// 
			this.BillFilterByAndGridPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.BillFilterByAndGridPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1006, 185, true);
			// 
			// FilterByPanel
			// 
			this.FilterByPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1006, 45, true);
			// 
			// BillGroupBoxPanel
			// 
			this.BillGroupBoxPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1006, 140, true);
			// 
			// HouseBillsGroupBox
			// 
			this.HouseBillsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1006, 140, true);
			this.HouseBillsGroupBox.Text = "Bills of Lading";
			// 
			// HouseBillsGrid
			// 
			zCodeFindBoxColumnStyleInfo1.BindToList = "Lookups+USCarrierList";
			zCodeFindBoxColumnStyleInfo1.Caption = "Bill Issuer SCAC";
			zCodeFindBoxColumnStyleInfo1.CaptionResourceString = null;
			zCodeFindBoxColumnStyleInfo1.ColumnName = "US_UI_NKBillIssuerSCAC";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo1.Caption = "ISF Bill Status";
			zTextBoxColumnStyleInfo1.CaptionResourceString = null;
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "ISFBillStatus";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.IsVisible = false;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo2.Caption = "ISF Bill Status Description";
			zTextBoxColumnStyleInfo2.CaptionResourceString = null;
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo2.ColumnName = "ISFBillStatusDescription";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.IsVisible = false;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo7.Caption = "IT Number";
			zTextBoxColumnStyleInfo7.CaptionResourceString = null;
			zTextBoxColumnStyleInfo7.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo7.ColumnName = "ITNumber";
			splitCheckBoxColumn.Caption = "Is Split";
			splitCheckBoxColumn.ColumnName = "US_SESplitShip";
			expressTrackingCheckBoxColumn.ColumnName = "US_ExpressTracking";
			expressTrackingCheckBoxColumn.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			this.HouseBillsGrid.ColumnStyles.Add(splitCheckBoxColumn);
			this.HouseBillsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.HouseBillsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.HouseBillsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.HouseBillsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.HouseBillsGrid.ColumnStyles.Add(expressTrackingCheckBoxColumn);
			this.HouseBillsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 121, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(JobDeclaration);
			// 
			// BottomPanel
			// 
			this.bottomPanel.Controls.Add(this.BillOfLadingTabControl);
			this.bottomPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.bottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 188, true);
			this.bottomPanel.Name = "BottomPanel";
			this.bottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1006, 394, true);
			this.bottomPanel.TabIndex = 1;
			// 
			// BillOfLadingTabControl
			// 
			this.BillOfLadingTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.BillOfLadingTabControl.Controls.Add(this.packagesTabPage);
			this.BillOfLadingTabControl.Controls.Add(this.ITNosSplitTabPage);
			this.BillOfLadingTabControl.Controls.Add(this.FTZTabPage);
			this.BillOfLadingTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BillOfLadingTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.BillOfLadingTabControl.Name = "BillOfLadingTabControl";
			this.BillOfLadingTabControl.SelectedIndex = 0;
			this.BillOfLadingTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1006, 394, true);
			this.BillOfLadingTabControl.TabIndex = 0;
			// 
			// PackagesTabPage
			// 
			this.packagesTabPage.Controls.Add(this.PackagesGrid);
			this.packagesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.packagesTabPage.Name = "PackagesTabPage";
			this.packagesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.packagesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(998, 367, true);
			this.packagesTabPage.TabIndex = 0;
			this.packagesTabPage.Text = "Packages Details";
			// 
			// PackagesGrid
			// 
			this.PackagesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.PackagesGrid, "Packages");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((JobDeclaration)(null)).Packages)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Package)(((System.Collections.IList)(((JobDeclaration)(null)).Packages)).SyncRoot)).CW_HouseBill)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Package)(((System.Collections.IList)(((JobDeclaration)(null)).Packages)).SyncRoot)).Lookups.BillList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Package)(((System.Collections.IList)(((JobDeclaration)(null)).Packages)).SyncRoot)).CW_ContainerNoOrEquipmentNo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Package)(((System.Collections.IList)(((JobDeclaration)(null)).Packages)).SyncRoot)).Lookups.ContainerList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Package)(((System.Collections.IList)(((JobDeclaration)(null)).Packages)).SyncRoot)).CW_PackQty)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Package)(((System.Collections.IList)(((JobDeclaration)(null)).Packages)).SyncRoot)).CW_PackType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Package)(((System.Collections.IList)(((JobDeclaration)(null)).Packages)).SyncRoot)).Lookups.PackTypeList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Package)(((System.Collections.IList)(((JobDeclaration)(null)).Packages)).SyncRoot)).CW_MarksAndNos)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Package)(((System.Collections.IList)(((JobDeclaration)(null)).Packages)).SyncRoot)).CW_ShippingSymbol)));
			this.PackagesGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.BindToList = "Lookups.BillList";
			zDropEditColumnStyleInfo1.Caption = "Bill Number";
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "CW_HouseBill";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo2.BindToList = "Lookups.ContainerList";
			zDropEditColumnStyleInfo2.Caption = "Container Number";
			zDropEditColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo2.ColumnName = "CW_ContainerNoOrEquipmentNo";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(98);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.Caption = "Pack Qty";
			zCalcEditColumnStyleInfo1.ColumnName = "CW_PackQty";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.GroupName = Enterprise.Customs.US.GUI.Res.GetData("13b99100-e243-48e1-8dee-5678a2192ba8", "Pack Qty/UQ");
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo3.BindToList = "Lookups+PackTypeList";
			zDropEditColumnStyleInfo3.Caption = "Pack Type";
			zDropEditColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo3.ColumnName = "CW_PackType";
			zDropEditColumnStyleInfo3.GroupName = Enterprise.Customs.US.GUI.Res.GetData("13b99100-e243-48e1-8dee-5678a2192ba8", "Pack Qty/UQ");
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zMultiLineTextBoxColumnInfo1.Caption = "Marks And Numbers";
			zMultiLineTextBoxColumnInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zMultiLineTextBoxColumnInfo1.ColumnName = "CW_MarksAndNos";
			zMultiLineTextBoxColumnInfo1.MinimumEditControlWidth = 250;
			zMultiLineTextBoxColumnInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(250);
			zMultiLineTextBoxColumnInfo2.Caption = "Shipping Symbol";
			zMultiLineTextBoxColumnInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zMultiLineTextBoxColumnInfo2.ColumnName = "CW_ShippingSymbol";
			zMultiLineTextBoxColumnInfo2.MinimumEditControlWidth = 250;
			zMultiLineTextBoxColumnInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(250);
			this.PackagesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.PackagesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.PackagesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.PackagesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.PackagesGrid.ColumnStyles.Add(zMultiLineTextBoxColumnInfo1);
			this.PackagesGrid.ColumnStyles.Add(zMultiLineTextBoxColumnInfo2);
			this.PackagesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PackagesGrid.GridId = "74550151-3c21-4e94-8b93-6740444d7780";
			this.PackagesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.PackagesGrid.LayoutKey = "ContainersGrid";
			this.PackagesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.PackagesGrid.Name = "PackagesGrid";
			this.PackagesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(992, 361, true);
			this.PackagesGrid.TabIndex = 1;
			// 
			// ITNosSplitTabPage
			// 
			this.ITNosSplitTabPage.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("75af7b84-e5c1-427e-92ba-ff409b78c4d7", "IT Numbers And Qty/Split Shipment Details");
			this.ITNosSplitTabPage.Controls.Add(this.itAndSplitDetailsUserControl);
			this.ITNosSplitTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ITNosSplitTabPage.Name = "ITNosSplitTabPage";
			this.ITNosSplitTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(998, 367, true);
			this.ITNosSplitTabPage.TabIndex = 2;
			// 
			// itAndSplitDetailsUserControl
			// 
			this.itAndSplitDetailsUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.itAndSplitDetailsUserControl, "FilteredBills.ITAndSplitDetails");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ITAndSplitDetailsCollection)(((Bill)(((System.Collections.IList)(((JobDeclaration)(null)).FilteredBills)).SyncRoot)).ITAndSplitDetails)));
			this.itAndSplitDetailsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.itAndSplitDetailsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.itAndSplitDetailsUserControl.Name = "itAndSplitDetailsUserControl";
			this.itAndSplitDetailsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(998, 367, true);
			this.itAndSplitDetailsUserControl.TabIndex = 0;
			// 
			// FTZTabPage
			// 
			this.FTZTabPage.Controls.Add(this.pTTGroupBox);
			this.FTZTabPage.Controls.Add(this.importStatusGroupBox);
			this.FTZTabPage.Controls.Add(this.fZ10GroupBox);
			this.FTZTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.FTZTabPage.Name = "FTZTabPage";
			this.FTZTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(998, 367, true);
			this.FTZTabPage.TabIndex = 3;
			this.FTZTabPage.Text = "FTZ";
			// 
			// PTTGroupBox
			// 
			this.pTTGroupBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("a22fd6e7-a05b-4dbb-a539-79353d5ee517", "Permit To Transfer Details");
			this.pTTGroupBox.Controls.Add(this.pTTUniqueIDTextBox);
			this.pTTGroupBox.Controls.Add(this.remarksTextBox);
			this.pTTGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 132, true);
			this.pTTGroupBox.Name = "PTTGroupBox";
			this.pTTGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(370, 228, true);
			this.pTTGroupBox.TabIndex = 8;
			this.pTTGroupBox.TabStop = false;
			// 
			// PTTUniqueIDTextBox
			// 
			this.pTTUniqueIDTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.pTTUniqueIDTextBox, "FilteredBills.USB_PermitToTransferID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Bill)(((System.Collections.IList)(((JobDeclaration)(null)).FilteredBills)).SyncRoot)).USB_PermitToTransferID)));
			this.pTTUniqueIDTextBox.CaptionResourceString = null;
			this.pTTUniqueIDTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.pTTUniqueIDTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(132, 22, true);
			this.pTTUniqueIDTextBox.Name = "PTTUniqueIDTextBox";
			this.pTTUniqueIDTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.pTTUniqueIDTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(227, 17, true);
			this.pTTUniqueIDTextBox.TabIndex = 2;
			// 
			// RemarksTextBox
			// 
			this.remarksTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.remarksTextBox, "FilteredBills.US_F_Remarks");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Bill)(((System.Collections.IList)(((JobDeclaration)(null)).FilteredBills)).SyncRoot)).US_F_Remarks)));
			this.remarksTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("f69d26ca-dee9-4429-a3e8-e8542cc7ba73", "Remarks");
			this.remarksTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.remarksTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(132, 42, true);
			this.remarksTextBox.Multiline = true;
			this.remarksTextBox.Name = "RemarksTextBox";
			this.remarksTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(227, 179, true);
			this.remarksTextBox.TabIndex = 3;
			// 
			// ImportStatusGroupBox
			// 
			this.importStatusGroupBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("505e984d-08c4-4277-a291-d0ad3819c425", "Shipment Details");
			this.importStatusGroupBox.Controls.Add(this.pTTCarrierGuidFindBox);
			this.importStatusGroupBox.Controls.Add(this.uS_UC_NKCountryOfExportCodeFindBox);
			this.importStatusGroupBox.Controls.Add(this.portOfLoadingSchDFindBox);
			this.importStatusGroupBox.Controls.Add(this.locationOfGoodsCodeFindBox);
			this.importStatusGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.importStatusGroupBox.Name = "ImportStatusGroupBox";
			this.importStatusGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(370, 124, true);
			this.importStatusGroupBox.TabIndex = 7;
			this.importStatusGroupBox.TabStop = false;
			// 
			// PTTCarrierGuidFindBox
			// 
			this.pTTCarrierGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.pTTCarrierGuidFindBox, "FilteredBills.US_F_OH_PTTCarrier");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Bill)(((System.Collections.IList)(((JobDeclaration)(null)).FilteredBills)).SyncRoot)).US_F_OH_PTTCarrier)));
			this.pTTCarrierGuidFindBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("547c564e-b283-4dfc-a70b-4f3759ab05e1", "PTT Carrier");
			this.pTTCarrierGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(132, 97, true);
			this.pTTCarrierGuidFindBox.Name = "PTTCarrierGuidFindBox";
			this.pTTCarrierGuidFindBox.PreBoundMaxLength = 12;
			this.pTTCarrierGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(227, 20, true);
			this.pTTCarrierGuidFindBox.TabIndex = 3;
			// 
			// US_UC_NKCountryOfExportCodeFindBox
			// 
			this.uS_UC_NKCountryOfExportCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.uS_UC_NKCountryOfExportCodeFindBox, "FilteredBills.US_UC_NKCountryOfExport");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Bill)(((System.Collections.IList)(((JobDeclaration)(null)).FilteredBills)).SyncRoot)).US_UC_NKCountryOfExport)));
			this.uS_UC_NKCountryOfExportCodeFindBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("e803ec8c-0c2e-4174-a299-128048fcce28", "Country Of Export");
			this.uS_UC_NKCountryOfExportCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(132, 45, true);
			this.uS_UC_NKCountryOfExportCodeFindBox.Name = "US_UC_NKCountryOfExportCodeFindBox";
			this.uS_UC_NKCountryOfExportCodeFindBox.PreBoundMaxLength = 2;
			this.uS_UC_NKCountryOfExportCodeFindBox.ShowDescriptionBox = false;
			this.uS_UC_NKCountryOfExportCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(47, 20, true);
			this.uS_UC_NKCountryOfExportCodeFindBox.TabIndex = 1;
			// 
			// PortOfLoadingSchDFindBox
			// 
			this.portOfLoadingSchDFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.portOfLoadingSchDFindBox, "FilteredBills.US_SchDLoading");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Bill)(((System.Collections.IList)(((JobDeclaration)(null)).FilteredBills)).SyncRoot)).US_SchDLoading)));
			this.portOfLoadingSchDFindBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("8f9cb59e-2a8d-4607-b649-98a071d95337", "Foreign Port Of Loading");
			this.portOfLoadingSchDFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(132, 19, true);
			this.portOfLoadingSchDFindBox.Name = "PortOfLoadingSchDFindBox";
			this.portOfLoadingSchDFindBox.PopupCaption = null;
			this.portOfLoadingSchDFindBox.PreBoundMaxLength = 5;
			this.portOfLoadingSchDFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(161, 20, true);
			this.portOfLoadingSchDFindBox.TabIndex = 0;
			// 
			// LocationOfGoodsCodeFindBox
			// 
			this.locationOfGoodsCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.locationOfGoodsCodeFindBox, "FilteredBills.US_US_NKLocationOfGoods");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Bill)(((System.Collections.IList)(((JobDeclaration)(null)).FilteredBills)).SyncRoot)).US_US_NKLocationOfGoods)));
			this.locationOfGoodsCodeFindBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("ae68c96d-e440-43a0-bb41-1f2aea8c2b6c", "FIRMS");
			this.locationOfGoodsCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(132, 71, true);
			this.locationOfGoodsCodeFindBox.Name = "LocationOfGoodsCodeFindBox";
			this.locationOfGoodsCodeFindBox.PopupCaption = null;
			this.locationOfGoodsCodeFindBox.PreBoundMaxLength = 4;
			this.locationOfGoodsCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(227, 20, true);
			this.locationOfGoodsCodeFindBox.TabIndex = 2;
			// 
			// FZ10GroupBox
			// 
			this.fZ10GroupBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("a22fd6e7-a05b-f149-a539-79353d5ee517", "Concurrence Details");
			this.fZ10GroupBox.Controls.Add(this.fZ10RemarksTextBox);
			this.fZ10GroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(379, 3, true);
			this.fZ10GroupBox.Name = "FZ10GroupBox";
			this.fZ10GroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(370, 135, true);
			this.fZ10GroupBox.TabIndex = 9;
			this.fZ10GroupBox.TabStop = false;
			// 
			// FZ10RemarksTextBox
			// 
			this.fZ10RemarksTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.fZ10RemarksTextBox, "FilteredBills.US_F_FZ10Remarks");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Bill)(((System.Collections.IList)(((JobDeclaration)(null)).FilteredBills)).SyncRoot)).US_F_FZ10Remarks)));
			this.fZ10RemarksTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("a99d26ca-daf9-4429-a3e8-e8519ce7ba73", "Remarks Merchandise Received");
			this.fZ10RemarksTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.fZ10RemarksTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(137, 19, true);
			this.fZ10RemarksTextBox.Multiline = true;
			this.fZ10RemarksTextBox.Name = "FZ10RemarksTextBox";
			this.fZ10RemarksTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(227, 105, true);
			this.fZ10RemarksTextBox.TabIndex = 2;
			// 
			// HorizontalSplitter
			// 
			this.horizontalSplitter.BackColor = System.Drawing.SystemColors.ButtonShadow;
			this.horizontalSplitter.Dock = System.Windows.Forms.DockStyle.Top;
			this.horizontalSplitter.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 185, true);
			this.horizontalSplitter.Name = "HorizontalSplitter";
			this.horizontalSplitter.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1006, 3, true);
			this.horizontalSplitter.TabIndex = 1;
			this.horizontalSplitter.TabStop = false;
			// 
			// ImportPackingUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Name = "ImportPackingUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1006, 582, true);
			this.HouseBillPanel.ResumeLayout(false);
			this.HouseBillPanel.PerformLayout();
			this.BillFilterByAndGridPanel.ResumeLayout(false);
			this.BillFilterByAndGridPanel.PerformLayout();
			this.FilterByPanel.ResumeLayout(false);
			this.FilterByPanel.PerformLayout();
			this.BillGroupBoxPanel.ResumeLayout(false);
			this.BillGroupBoxPanel.PerformLayout();
			this.HouseBillsGroupBox.ResumeLayout(false);
			this.HouseBillsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.HouseBillsGrid)).EndInit();
			this.HouseBillsGrid.ResumeLayout(false);
			this.HouseBillsGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.bottomPanel.ResumeLayout(false);
			this.bottomPanel.PerformLayout();
			this.BillOfLadingTabControl.ResumeLayout(false);
			this.BillOfLadingTabControl.PerformLayout();
			this.packagesTabPage.ResumeLayout(false);
			this.packagesTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.PackagesGrid)).EndInit();
			this.PackagesGrid.ResumeLayout(false);
			this.PackagesGrid.PerformLayout();
			this.ITNosSplitTabPage.ResumeLayout(false);
			this.ITNosSplitTabPage.PerformLayout();
			this.itAndSplitDetailsUserControl.ResumeLayout(true);
			this.itAndSplitDetailsUserControl.PerformLayout();
			this.FTZTabPage.ResumeLayout(false);
			this.FTZTabPage.PerformLayout();
			this.pTTGroupBox.ResumeLayout(false);
			this.pTTGroupBox.PerformLayout();
			this.importStatusGroupBox.ResumeLayout(false);
			this.importStatusGroupBox.PerformLayout();
			this.pTTCarrierGuidFindBox.ResumeLayout(true);
			this.pTTCarrierGuidFindBox.PerformLayout();
			this.uS_UC_NKCountryOfExportCodeFindBox.ResumeLayout(true);
			this.uS_UC_NKCountryOfExportCodeFindBox.PerformLayout();
			this.portOfLoadingSchDFindBox.ResumeLayout(true);
			this.portOfLoadingSchDFindBox.PerformLayout();
			this.locationOfGoodsCodeFindBox.ResumeLayout(true);
			this.locationOfGoodsCodeFindBox.PerformLayout();
			this.fZ10GroupBox.ResumeLayout(false);
			this.fZ10GroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		ZPanel bottomPanel;
		internal ZTabControl BillOfLadingTabControl;
		ZTabPage packagesTabPage;
		CargoWise.Windows.UI.KSplitter horizontalSplitter;
		public ZGrid PackagesGrid;
		internal ZTabPage ITNosSplitTabPage;
		internal ITDetailsUserControl itAndSplitDetailsUserControl;
		internal ZTabPage FTZTabPage;
		ZGroupBox importStatusGroupBox;
		ZCodeFindBox locationOfGoodsCodeFindBox;
		protected ZCodeFindBox portOfLoadingSchDFindBox;
		ZCodeFindBox uS_UC_NKCountryOfExportCodeFindBox;
		ZGroupBox pTTGroupBox;
		ZGroupBox fZ10GroupBox;
		ZTextBox remarksTextBox;
		ZTextBox fZ10RemarksTextBox;
		ZTextBox pTTUniqueIDTextBox;
		ZGuidFindBox pTTCarrierGuidFindBox;
	}
}
