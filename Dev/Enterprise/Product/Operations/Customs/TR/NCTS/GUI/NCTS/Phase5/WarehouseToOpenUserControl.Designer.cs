using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TR.NCTS.GUI
{
	partial class WarehouseToOpenUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo zMultiControlColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new ZCodeFindBoxColumnStyleInfo();
			this.WarehouseToOpenGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.WarehouseToOpenGrid = new Enterprise.ZArchitecture.ZGrid();
			this.ReferenceTextBox = new ZArchitecture.ZTextBox();
			this.PaymentTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.IncotermDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.AmountCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.CountryCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.NatureOfBusinessDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DeclarationItemNoTextBox = new Enterprise.ZArchitecture.ZCalcEdit();
			this.NctsLineNoTextBox = new Enterprise.ZArchitecture.ZCalcEdit();
			this.BoxQuantityCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.ExplanationTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.TopPanel = new ZArchitecture.GUI.ZPanel();
			this.BottomPanel = new ZArchitecture.GUI.ZPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.TopPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.WarehouseToOpenGrid)).BeginInit();
			this.WarehouseToOpenGrid.SuspendLayout();
			this.BottomPanel.SuspendLayout();
			this.WarehouseToOpenGroupBox.SuspendLayout();
			this.PaymentTypeDropEdit.SuspendLayout();
			this.IncotermDropEdit.SuspendLayout();
			this.AmountCalcDropEdit.SuspendLayout();
			this.CountryCodeFindBox.SuspendLayout();
			this.NatureOfBusinessDropEdit.SuspendLayout();
			this.DeclarationItemNoTextBox.SuspendLayout();
			this.NctsLineNoTextBox.SuspendLayout();
			this.BoxQuantityCalcDropEdit.SuspendLayout();
			this.ExplanationTextBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			//this.BindingSource.DataSourceType = typeof(Enterprise.Customs.TR.NCTS.Business.NctsDepartureMovementHeader);
			// 
			// TopPanel
			// 
			this.TopPanel.Controls.Add(this.WarehouseToOpenGrid);
			this.TopPanel.Dock = DockStyle.Fill;
			this.TopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TopPanel.Name = "TopPanel";
			this.TopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(909, 206, true);
			this.TopPanel.TabIndex = 0;
			// 
			// WarehouseToOpenGrid
			//
			this.WarehouseToOpenGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.WarehouseToOpenGrid, "MovementHeader.WarehouseToOpenList");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.TR.NCTS.Business.NctsDepartureMovementHeader)(null)).WarehouseToOpenList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.NCTS.Business.NctsWarehouseToOpen)(((System.Collections.IList)(((Enterprise.Customs.TR.NCTS.Business.NctsDepartureMovementHeader)(null)).WarehouseToOpenList)).SyncRoot)).CSI_ReferenceNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TR.NCTS.Business.NctsWarehouseToOpen)(((System.Collections.IList)(((Enterprise.Customs.TR.NCTS.Business.NctsDepartureMovementHeader)(null)).WarehouseToOpenList)).SyncRoot)).Incoterm)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TR.NCTS.Business.NctsWarehouseToOpen)(((System.Collections.IList)(((Enterprise.Customs.TR.NCTS.Business.NctsDepartureMovementHeader)(null)).WarehouseToOpenList)).SyncRoot)).CSI_SubType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TR.NCTS.Business.NctsWarehouseToOpen)(((System.Collections.IList)(((Enterprise.Customs.TR.NCTS.Business.NctsDepartureMovementHeader)(null)).WarehouseToOpenList)).SyncRoot)).CSI_Value)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.NCTS.Business.NctsWarehouseToOpen)(((System.Collections.IList)(((Enterprise.Customs.TR.NCTS.Business.NctsDepartureMovementHeader)(null)).WarehouseToOpenList)).SyncRoot)).CSI_RX_NKCurrency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.NCTS.Business.NctsWarehouseToOpen)(((System.Collections.IList)(((Enterprise.Customs.TR.NCTS.Business.NctsDepartureMovementHeader)(null)).WarehouseToOpenList)).SyncRoot)).CSI_RN_NKCountryCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TR.NCTS.Business.NctsWarehouseToOpen)(((System.Collections.IList)(((Enterprise.Customs.TR.NCTS.Business.NctsDepartureMovementHeader)(null)).WarehouseToOpenList)).SyncRoot)).CSI_Procedure)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TR.NCTS.Business.NctsWarehouseToOpen)(((System.Collections.IList)(((Enterprise.Customs.TR.NCTS.Business.NctsDepartureMovementHeader)(null)).WarehouseToOpenList)).SyncRoot)).CSI_LineNo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TR.NCTS.Business.NctsWarehouseToOpen)(((System.Collections.IList)(((Enterprise.Customs.TR.NCTS.Business.NctsDepartureMovementHeader)(null)).WarehouseToOpenList)).SyncRoot)).CSI_ItemNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TR.NCTS.Business.NctsWarehouseToOpen)(((System.Collections.IList)(((Enterprise.Customs.TR.NCTS.Business.NctsDepartureMovementHeader)(null)).WarehouseToOpenList)).SyncRoot)).CSI_Quantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.NCTS.Business.NctsWarehouseToOpen)(((System.Collections.IList)(((Enterprise.Customs.TR.NCTS.Business.NctsDepartureMovementHeader)(null)).WarehouseToOpenList)).SyncRoot)).CSI_UnitOfQuantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.NCTS.Business.NctsWarehouseToOpen)(((System.Collections.IList)(((Enterprise.Customs.TR.NCTS.Business.NctsDepartureMovementHeader)(null)).WarehouseToOpenList)).SyncRoot)).CSI_Description)));
			this.WarehouseToOpenGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(909, 206, true);
			this.WarehouseToOpenGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CharacterCasing = CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "CSI_ReferenceNumber";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "CSI_LineNo";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zCalcEditColumnStyleInfo2.ColumnName = "CSI_ItemNumber";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zCalcEditColumnStyleInfo3.ColumnName = "CSI_Value";
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo1.CharacterCasing = CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "CSI_RX_NKCurrency";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo4.ColumnName = "CSI_Quantity";
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo2.CharacterCasing = CharacterCasing.Upper;
			zDropEditColumnStyleInfo2.ColumnName = "CSI_UnitOfQuantity";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CharacterCasing = CharacterCasing.Upper;
			zTextBoxColumnStyleInfo2.ColumnName = "CSI_Description";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDropEditColumnStyleInfo3.CharacterCasing = CharacterCasing.Upper;
			zDropEditColumnStyleInfo3.ColumnName = "Incoterm";
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo4.CharacterCasing = CharacterCasing.Upper;
			zDropEditColumnStyleInfo4.ColumnName = "CSI_SubType";
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo5.ColumnName = "CSI_Procedure";
			zDropEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCodeFindBoxColumnStyleInfo1.ColumnName = "CSI_RN_NKCountryCode";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.WarehouseToOpenGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.WarehouseToOpenGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.WarehouseToOpenGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.WarehouseToOpenGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.WarehouseToOpenGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.WarehouseToOpenGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.WarehouseToOpenGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.WarehouseToOpenGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.WarehouseToOpenGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.WarehouseToOpenGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.WarehouseToOpenGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.WarehouseToOpenGrid.ColumnStyles.Add(zDropEditColumnStyleInfo5);
			this.WarehouseToOpenGrid.Dock = DockStyle.Fill;
			this.WarehouseToOpenGrid.GridId = "2880BCF7-3434-4812-A6A1-A08243A3BF0F";
			this.WarehouseToOpenGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.WarehouseToOpenGrid.LayoutKey = "WarehouseToOpenGrid";
			this.WarehouseToOpenGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.WarehouseToOpenGrid.Name = "WarehouseToOpenGrid";
			this.WarehouseToOpenGrid.TabIndex = 0;
			// 
			// WarehouseToOpenGroupBox
			//
			this.WarehouseToOpenGroupBox.Controls.Add(this.ReferenceTextBox);
			this.WarehouseToOpenGroupBox.Controls.Add(this.DeclarationItemNoTextBox);
			this.WarehouseToOpenGroupBox.Controls.Add(this.NctsLineNoTextBox);
			this.WarehouseToOpenGroupBox.Controls.Add(this.BoxQuantityCalcDropEdit);
			this.WarehouseToOpenGroupBox.Controls.Add(this.ExplanationTextBox);
			this.WarehouseToOpenGroupBox.Controls.Add(this.AmountCalcDropEdit);
			this.WarehouseToOpenGroupBox.Controls.Add(this.CountryCodeFindBox);
			this.WarehouseToOpenGroupBox.Controls.Add(this.IncotermDropEdit);
			this.WarehouseToOpenGroupBox.Controls.Add(this.PaymentTypeDropEdit);
			this.WarehouseToOpenGroupBox.Controls.Add(this.NatureOfBusinessDropEdit);
			this.WarehouseToOpenGroupBox.Dock = DockStyle.Fill;
			this.WarehouseToOpenGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.WarehouseToOpenGroupBox.Name = "WarehouseToOpenGroupBox";
			this.WarehouseToOpenGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(909, 379, true);
			this.WarehouseToOpenGroupBox.TabIndex = 0;
			this.WarehouseToOpenGroupBox.TabStop = false;
			this.WarehouseToOpenGroupBox.Text = Enterprise.Customs.TR.NCTS.GUI.Res.GetString("2EB16B51-29CC-42B3-8806-FC11974E12E1", "Warehouse To Open Details");
			// 
			// ReferenceTextBox
			// 
			this.ReferenceTextBox.Anchor = ((AnchorStyles)((AnchorStyles.Top | AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.ReferenceTextBox, "MovementHeader.WarehouseToOpenList.CSI_ReferenceNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.NCTS.Business.NctsWarehouseToOpen)(((System.Collections.IList)(((Enterprise.Customs.TR.NCTS.Business.NctsDepartureMovementHeader)(null)).WarehouseToOpenList)).SyncRoot)).CSI_ReferenceNumber)));
			this.ReferenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 36, true);
			this.ReferenceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(163, 20, true);
			this.ReferenceTextBox.Name = "ReferenceTextBox";
			// 
			// IncotermDropEdit
			// 
			this.IncotermDropEdit.Anchor = ((AnchorStyles)((AnchorStyles.Top | AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.IncotermDropEdit, "MovementHeader.WarehouseToOpenList.Incoterm");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TR.NCTS.Business.NctsWarehouseToOpen)(((System.Collections.IList)(((Enterprise.Customs.TR.NCTS.Business.NctsDepartureMovementHeader)(null)).WarehouseToOpenList)).SyncRoot)).Incoterm)));
			this.IncotermDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(384, 62, true);
			this.IncotermDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(163, 20, true);
			this.IncotermDropEdit.Name = "IncotermDropEdit";
			this.IncotermDropEdit.TabIndex = 7;
			// 
			// BottomPanel
			//
			this.BottomPanel.Controls.Add(this.WarehouseToOpenGroupBox);
			this.BottomPanel.Dock = DockStyle.Bottom;
			this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 253, true);
			this.BottomPanel.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(213, 95, true);
			this.BottomPanel.Name = "BottomPanel";
			this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(679, 235, true);
			this.BottomPanel.TabIndex = 1;
			// 
			// PaymentTypeDropEdit
			// 
			this.PaymentTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PaymentTypeDropEdit, "MovementHeader.WarehouseToOpenList.CSI_SubType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TR.NCTS.Business.NctsWarehouseToOpen)(((System.Collections.IList)(((Enterprise.Customs.TR.NCTS.Business.NctsDepartureMovementHeader)(null)).WarehouseToOpenList)).SyncRoot)).CSI_SubType)));
			this.PaymentTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(384, 88, true);
			this.PaymentTypeDropEdit.Name = "PaymentTypeDropEdit";
			this.PaymentTypeDropEdit.PreBoundMaxLength = 4;
			this.PaymentTypeDropEdit.ShouldResizeByMaxLength = true;
			this.PaymentTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(163, 20, true);
			this.PaymentTypeDropEdit.TabIndex = 8;
			// 
			// AmountCalcDropEdit
			// 
			this.AmountCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AmountCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TR.NCTS.Business.NctsWarehouseToOpen)(((System.Collections.IList)(((Enterprise.Customs.TR.NCTS.Business.NctsDepartureMovementHeader)(null)).WarehouseToOpenList)).SyncRoot)).CSI_Value)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.NCTS.Business.NctsWarehouseToOpen)(((System.Collections.IList)(((Enterprise.Customs.TR.NCTS.Business.NctsDepartureMovementHeader)(null)).WarehouseToOpenList)).SyncRoot)).CSI_RX_NKCurrency)));
			this.AmountCalcDropEdit.BindToAmount = "MovementHeader.WarehouseToOpenList.CSI_Value";
			this.AmountCalcDropEdit.BindToUnit = "MovementHeader.WarehouseToOpenList.CSI_RX_NKCurrency";
			this.AmountCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 166, true);
			this.AmountCalcDropEdit.Name = "AmountCalcDropEdit";
			this.AmountCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(163, 20, true);
			this.AmountCalcDropEdit.TabIndex = 5;
			// 
			// CountryCodeFindBox
			// 
			this.CountryCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CountryCodeFindBox, "MovementHeader.WarehouseToOpenList.CSI_RN_NKCountryCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.NCTS.Business.NctsWarehouseToOpen)(((System.Collections.IList)(((Enterprise.Customs.TR.NCTS.Business.NctsDepartureMovementHeader)(null)).WarehouseToOpenList)).SyncRoot)).CSI_RN_NKCountryCode)));
			this.CountryCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(384, 36, true);
			this.CountryCodeFindBox.Name = "CountryCodeFindBox";
			this.CountryCodeFindBox.PreBoundMaxLength = 4;
			this.CountryCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(163, 20, true);
			this.CountryCodeFindBox.TabIndex = 6;
			// 
			// NatureOfBusinessDropEdit
			// 
			this.NatureOfBusinessDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.NatureOfBusinessDropEdit, "MovementHeader.WarehouseToOpenList.CSI_Procedure");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TR.NCTS.Business.NctsWarehouseToOpen)(((System.Collections.IList)(((Enterprise.Customs.TR.NCTS.Business.NctsDepartureMovementHeader)(null)).WarehouseToOpenList)).SyncRoot)).CSI_Procedure)));
			this.NatureOfBusinessDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(384, 114, true);
			this.NatureOfBusinessDropEdit.Name = "NatureOfBusinessDropEdit";
			this.NatureOfBusinessDropEdit.PreBoundMaxLength = 4;
			this.NatureOfBusinessDropEdit.ShouldResizeByMaxLength = true;
			this.NatureOfBusinessDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(163, 20, true);
			this.NatureOfBusinessDropEdit.TabIndex = 9;
			// 
			// DeclarationItemNoTextBox
			// 
			this.BindingSource.SetBindingMember(this.DeclarationItemNoTextBox, "MovementHeader.WarehouseToOpenList.CSI_LineNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TR.NCTS.Business.NctsWarehouseToOpen)(((System.Collections.IList)(((Enterprise.Customs.TR.NCTS.Business.NctsDepartureMovementHeader)(null)).WarehouseToOpenList)).SyncRoot)).CSI_LineNo)));
			this.DeclarationItemNoTextBox.CaptionResourceString = null;
			this.DeclarationItemNoTextBox.DecimalPlaces = 2;
			this.DeclarationItemNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 62, true);
			this.DeclarationItemNoTextBox.Name = "DeclarationItemNoTextBox";
			this.DeclarationItemNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(163, 20, true);
			this.DeclarationItemNoTextBox.TabIndex = 2;
			this.DeclarationItemNoTextBox.TextAlign = HorizontalAlignment.Right;
			// 
			// NctsLineNoTextBox
			// 
			this.BindingSource.SetBindingMember(this.NctsLineNoTextBox, "MovementHeader.WarehouseToOpenList.CSI_ItemNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TR.NCTS.Business.NctsWarehouseToOpen)(((System.Collections.IList)(((Enterprise.Customs.TR.NCTS.Business.NctsDepartureMovementHeader)(null)).WarehouseToOpenList)).SyncRoot)).CSI_ItemNumber)));
			this.NctsLineNoTextBox.CaptionResourceString = null;
			this.NctsLineNoTextBox.DecimalPlaces = 2;
			this.NctsLineNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 88, true);
			this.NctsLineNoTextBox.Name = "NctsLineNoTextBox";
			this.NctsLineNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(163, 20, true);
			this.NctsLineNoTextBox.TabIndex = 3;
			this.NctsLineNoTextBox.TextAlign = HorizontalAlignment.Right;
			// 
			// BoxQuantityCalcDropEdit
			// 
			this.BoxQuantityCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BoxQuantityCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TR.NCTS.Business.NctsWarehouseToOpen)(((System.Collections.IList)(((Enterprise.Customs.TR.NCTS.Business.NctsDepartureMovementHeader)(null)).WarehouseToOpenList)).SyncRoot)).CSI_Quantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.NCTS.Business.NctsWarehouseToOpen)(((System.Collections.IList)(((Enterprise.Customs.TR.NCTS.Business.NctsDepartureMovementHeader)(null)).WarehouseToOpenList)).SyncRoot)).CSI_UnitOfQuantity)));
			this.BoxQuantityCalcDropEdit.BindToAmount = "MovementHeader.WarehouseToOpenList.CSI_Quantity";
			this.BoxQuantityCalcDropEdit.BindToUnit = "MovementHeader.WarehouseToOpenList.CSI_UnitOfQuantity";
			this.BoxQuantityCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 114, true);
			this.BoxQuantityCalcDropEdit.Name = "BoxQuantityCalcDropEdit";
			this.BoxQuantityCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(163, 20, true);
			this.BoxQuantityCalcDropEdit.TabIndex = 4;
			// 
			// ExplanationTextBox
			// 
			this.BindingSource.SetBindingMember(this.ExplanationTextBox, "MovementHeader.WarehouseToOpenList.CSI_Description");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.NCTS.Business.NctsWarehouseToOpen)(((System.Collections.IList)(((Enterprise.Customs.TR.NCTS.Business.NctsDepartureMovementHeader)(null)).WarehouseToOpenList)).SyncRoot)).CSI_Description)));
			this.ExplanationTextBox.CaptionResourceString = null;
			this.ExplanationTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 140, true);
			this.ExplanationTextBox.Name = "ExplanationTextBox";
			this.ExplanationTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(163, 20, true);
			this.ExplanationTextBox.TabIndex = 11;
			// 
			// NctsWarehouseToOpenUserControl
			// 
			this.AutoScaleMode = AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.TopPanel);
			this.Controls.Add(this.BottomPanel);
			this.Name = "NctsWarehouseToOpenUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(679, 488, true);
			this.Controls.SetChildIndex(this.BottomPanel, 0);
			this.Controls.SetChildIndex(this.TopPanel, 0);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.TopPanel.ResumeLayout(false);
			this.TopPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.WarehouseToOpenGrid)).EndInit();
			this.BottomPanel.ResumeLayout(false);
			this.BottomPanel.PerformLayout();
			this.WarehouseToOpenGroupBox.ResumeLayout(false);
			this.WarehouseToOpenGroupBox.PerformLayout();
			this.WarehouseToOpenGrid.ResumeLayout(false);
			this.WarehouseToOpenGrid.PerformLayout();
			this.IncotermDropEdit.ResumeLayout(true);
			this.IncotermDropEdit.PerformLayout();
			this.ReferenceTextBox.ResumeLayout(false);
			this.ReferenceTextBox.PerformLayout();
			this.PaymentTypeDropEdit.ResumeLayout(true);
			this.PaymentTypeDropEdit.PerformLayout();
			this.AmountCalcDropEdit.ResumeLayout(true);
			this.AmountCalcDropEdit.PerformLayout();
			this.CountryCodeFindBox.ResumeLayout(true);
			this.CountryCodeFindBox.PerformLayout();
			this.NatureOfBusinessDropEdit.ResumeLayout(true);
			this.NatureOfBusinessDropEdit.PerformLayout();
			this.DeclarationItemNoTextBox.ResumeLayout(true);
			this.DeclarationItemNoTextBox.PerformLayout();
			this.NctsLineNoTextBox.ResumeLayout(true);
			this.NctsLineNoTextBox.PerformLayout();
			this.BoxQuantityCalcDropEdit.ResumeLayout(true);
			this.BoxQuantityCalcDropEdit.PerformLayout();
			this.ExplanationTextBox.ResumeLayout(true);
			this.ExplanationTextBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
		#endregion

		protected ZArchitecture.GUI.ZGroupBox WarehouseToOpenGroupBox;
		private ZArchitecture.ZGrid WarehouseToOpenGrid;
		private ZArchitecture.ZTextBox ReferenceTextBox;
		private ZArchitecture.GUI.ZDropEdit PaymentTypeDropEdit;
		private ZArchitecture.GUI.ZDropEdit IncotermDropEdit;
		private ZArchitecture.GUI.ZCalcDropEdit AmountCalcDropEdit;
		private ZArchitecture.GUI.ZCodeFindBox CountryCodeFindBox;
		private ZArchitecture.GUI.ZDropEdit NatureOfBusinessDropEdit;
		private ZArchitecture.ZCalcEdit DeclarationItemNoTextBox;
		private ZArchitecture.ZCalcEdit NctsLineNoTextBox;
		private ZArchitecture.ZTextBox ExplanationTextBox;
		private ZArchitecture.GUI.ZCalcDropEdit BoxQuantityCalcDropEdit;
		private ZArchitecture.GUI.ZPanel TopPanel;
		private ZArchitecture.GUI.ZPanel BottomPanel;
	}
}
