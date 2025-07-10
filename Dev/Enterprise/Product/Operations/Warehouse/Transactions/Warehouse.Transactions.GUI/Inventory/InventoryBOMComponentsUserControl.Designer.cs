using System;
using System.Windows.Forms;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Transactions.GUI
{
	partial class InventoryBOMComponentsUserControl
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
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			this.ComponentDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ComponentDetailsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ComponentDetailsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ComponentDetailsGrid)).BeginInit();
			this.ComponentDetailsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Warehouse.Transactions.Business.WhsDocketLine);
			// 
			// ComponentDetailsGroupBox
			// 
			this.ComponentDetailsGroupBox.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("InventoryComponentUserControl|6e9145c1-f21a-47cc-a2fc-ec169e1a5d0e", "Components");
			this.ComponentDetailsGroupBox.Controls.Add(this.ComponentDetailsGrid);
			this.ComponentDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ComponentDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ComponentDetailsGroupBox.Name = "ComponentDetailsGroupBox";
			this.ComponentDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(747, 563, true);
			this.ComponentDetailsGroupBox.TabIndex = 0;
			this.ComponentDetailsGroupBox.TabStop = false;
			// 
			// ComponentDetailsGrid
			// 
			this.ComponentDetailsGrid.AllowDrop = true;
			this.ComponentDetailsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ComponentDetailsGrid, "ComponentInventoryLines");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsDocketLine)(null)).ComponentInventoryLines)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Warehouse.Transactions.Business.WhsDocketLine)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsDocketLine)(null)).ComponentInventoryLines)).SyncRoot)).WE_OP)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Transactions.Business.WhsDocketLine)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsDocketLine)(null)).ComponentInventoryLines)).SyncRoot)).WE_PartAttrib1)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Transactions.Business.WhsDocketLine)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsDocketLine)(null)).ComponentInventoryLines)).SyncRoot)).WE_PartAttrib2)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Transactions.Business.WhsDocketLine)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsDocketLine)(null)).ComponentInventoryLines)).SyncRoot)).WE_PartAttrib3)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Transactions.Business.WhsDocketLine)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsDocketLine)(null)).ComponentInventoryLines)).SyncRoot)).WE_SerialNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDate)(((Enterprise.Warehouse.Transactions.Business.WhsDocketLine)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsDocketLine)(null)).ComponentInventoryLines)).SyncRoot)).WE_ExpiryDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDate)(((Enterprise.Warehouse.Transactions.Business.WhsDocketLine)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsDocketLine)(null)).ComponentInventoryLines)).SyncRoot)).WE_PackingDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Transactions.Business.WhsDocketLine)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsDocketLine)(null)).ComponentInventoryLines)).SyncRoot)).CustomsData.WB_EntryKey)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Transactions.Business.WhsDocketLine)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsDocketLine)(null)).ComponentInventoryLines)).SyncRoot)).CustomsData.WB_DeclarationReference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Transactions.Business.WhsDocketLine)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsDocketLine)(null)).ComponentInventoryLines)).SyncRoot)).CustomsData.WB_InwardStyle)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Transactions.Business.WhsDocketLine)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsDocketLine)(null)).ComponentInventoryLines)).SyncRoot)).CustomsData.WB_InwardProcedure)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Transactions.Business.WhsDocketLine)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsDocketLine)(null)).ComponentInventoryLines)).SyncRoot)).CustomsData.WB_CustomsUnitOfQty)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Transactions.Business.WhsDocketLine)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsDocketLine)(null)).ComponentInventoryLines)).SyncRoot)).CustomsData.WB_AddInfo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Warehouse.Transactions.Business.WhsDocketLine)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsDocketLine)(null)).ComponentInventoryLines)).SyncRoot)).CustomsData.WB_EntryLineNo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Warehouse.Transactions.Business.WhsDocketLine)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsDocketLine)(null)).ComponentInventoryLines)).SyncRoot)).CustomsData.WB_CustomsQty)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Warehouse.Transactions.Business.WhsDocketLine)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsDocketLine)(null)).ComponentInventoryLines)).SyncRoot)).CustomsData.WB_ValueForDuty)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Warehouse.Transactions.Business.WhsDocketLine)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsDocketLine)(null)).ComponentInventoryLines)).SyncRoot)).CustomsData.WB_TILV)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Warehouse.Transactions.Business.WhsDocketLine)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsDocketLine)(null)).ComponentInventoryLines)).SyncRoot)).CustomsData.WB_EntryDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDate)(((Enterprise.Warehouse.Transactions.Business.WhsDocketLine)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsDocketLine)(null)).ComponentInventoryLines)).SyncRoot)).CustomsData.WB_CustomsDeadline)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Transactions.Business.WhsDocketLine)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsDocketLine)(null)).ComponentInventoryLines)).SyncRoot)).CustomsData.WB_RN_NKCountryOfOrigin)));
			this.ComponentDetailsGrid.CaptionVisible = false;
			zGuidFindBoxColumnStyleInfo1.ColumnName = "WE_OP";
			zGuidFindBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.ColumnName = "WE_PartAttrib1";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.ColumnName = "WE_PartAttrib2";
			zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.ColumnName = "WE_PartAttrib3";
			zTextBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo4.ColumnName = "WE_SerialNumber";
			zTextBoxColumnStyleInfo4.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo5.ColumnName = "WE_ExpiryDate";
			zTextBoxColumnStyleInfo5.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo6.ColumnName = "WE_PackingDate";
			zTextBoxColumnStyleInfo6.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo7.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo7.ColumnName = "CustomsData+WB_EntryKey";
			zTextBoxColumnStyleInfo7.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo7.GroupName = Enterprise.Warehouse.Transactions.GUI.Res.GetData("OrderDocketLinesGridUserControl|85fed39d-ec7e-42fa-ba38-28da686f59b4", "Customs Data");
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo8.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo8.ColumnName = "CustomsData+WB_DeclarationReference";
			zTextBoxColumnStyleInfo8.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo8.GroupName = Enterprise.Warehouse.Transactions.GUI.Res.GetData("OrderDocketLinesGridUserControl|85fed39d-ec7e-42fa-ba38-28da686f59b4", "Customs Data");
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo9.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo9.ColumnName = "CustomsData+WB_InwardStyle";
			zTextBoxColumnStyleInfo9.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo9.GroupName = Enterprise.Warehouse.Transactions.GUI.Res.GetData("OrderDocketLinesGridUserControl|85fed39d-ec7e-42fa-ba38-28da686f59b4", "Customs Data");
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo10.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo10.ColumnName = "CustomsData+WB_InwardProcedure";
			zTextBoxColumnStyleInfo10.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo10.GroupName = Enterprise.Warehouse.Transactions.GUI.Res.GetData("OrderDocketLinesGridUserControl|85fed39d-ec7e-42fa-ba38-28da686f59b4", "Customs Data");
			zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo11.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo11.ColumnName = "CustomsData+WB_CustomsUnitOfQty";
			zTextBoxColumnStyleInfo11.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo11.GroupName = Enterprise.Warehouse.Transactions.GUI.Res.GetData("OrderDocketLinesGridUserControl|85fed39d-ec7e-42fa-ba38-28da686f59b4", "Customs Data");
			zTextBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo12.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo12.ColumnName = "CustomsData+WB_AddInfo";
			zTextBoxColumnStyleInfo12.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo12.GroupName = Enterprise.Warehouse.Transactions.GUI.Res.GetData("OrderDocketLinesGridUserControl|85fed39d-ec7e-42fa-ba38-28da686f59b4", "Customs Data");
			zTextBoxColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zCalcEditColumnStyleInfo1.ColumnName = "CustomsData+WB_EntryLineNo";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo1.GroupName = Enterprise.Warehouse.Transactions.GUI.Res.GetData("OrderDocketLinesGridUserControl|85fed39d-ec7e-42fa-ba38-28da686f59b4", "Customs Data");
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zCalcEditColumnStyleInfo2.ColumnName = "CustomsData+WB_CustomsQty";
			zCalcEditColumnStyleInfo2.Decimals = 4;
			zCalcEditColumnStyleInfo2.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo2.GroupName = Enterprise.Warehouse.Transactions.GUI.Res.GetData("OrderDocketLinesGridUserControl|85fed39d-ec7e-42fa-ba38-28da686f59b4", "Customs Data");
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zCalcEditColumnStyleInfo3.ColumnName = "CustomsData+WB_ValueForDuty";
			zCalcEditColumnStyleInfo3.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo3.GroupName = Enterprise.Warehouse.Transactions.GUI.Res.GetData("OrderDocketLinesGridUserControl|85fed39d-ec7e-42fa-ba38-28da686f59b4", "Customs Data");
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.ColumnName = "CustomsData+WB_TILV";
			zCalcEditColumnStyleInfo4.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo4.GroupName = Enterprise.Warehouse.Transactions.GUI.Res.GetData("OrderDocketLinesGridUserControl|85fed39d-ec7e-42fa-ba38-28da686f59b4", "Customs Data");
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			zDateEditColumnStyleInfo1.ColumnName = "CustomsData+WB_EntryDate";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zDateEditColumnStyleInfo1.GroupName = Enterprise.Warehouse.Transactions.GUI.Res.GetData("OrderDocketLinesGridUserControl|85fed39d-ec7e-42fa-ba38-28da686f59b4", "Customs Data");
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			zDateEditColumnStyleInfo2.ColumnName = "CustomsData+WB_CustomsDeadline";
			zDateEditColumnStyleInfo2.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo2.DefaultCollectionIndex = 0;
			zDateEditColumnStyleInfo2.GroupName = Enterprise.Warehouse.Transactions.GUI.Res.GetData("OrderDocketLinesGridUserControl|85fed39d-ec7e-42fa-ba38-28da686f59b4", "Customs Data");
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCodeFindBoxColumnStyleInfo1.ColumnName = "CustomsData+WB_RN_NKCountryOfOrigin";
			zCodeFindBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCodeFindBoxColumnStyleInfo1.GroupName = Enterprise.Warehouse.Transactions.GUI.Res.GetData("OrderDocketLinesGridUserControl|85fed39d-ec7e-42fa-ba38-28da686f59b4", "Customs Data");
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.ComponentDetailsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.ComponentDetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ComponentDetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ComponentDetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.ComponentDetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.ComponentDetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.ComponentDetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.ComponentDetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.ComponentDetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.ComponentDetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.ComponentDetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.ComponentDetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.ComponentDetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
			this.ComponentDetailsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.ComponentDetailsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.ComponentDetailsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.ComponentDetailsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.ComponentDetailsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.ComponentDetailsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.ComponentDetailsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.ComponentDetailsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ComponentDetailsGrid.GridId = "0d18222f-0f1a-49b0-839b-fdf73397824b";
			this.ComponentDetailsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ComponentDetailsGrid.LayoutKey = "ComponentDetailsGrid";
			this.ComponentDetailsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 11, true);
			this.ComponentDetailsGrid.Name = "ComponentDetailsGrid";
			this.ComponentDetailsGrid.ReadOnly = true;
			this.ComponentDetailsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(743, 550, true);
			this.ComponentDetailsGrid.TabIndex = 0;
			this.ComponentDetailsGrid.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.ComponentInfoGrid_MouseDoubleClick);
			// 
			// InventoryBOMComponentsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ComponentDetailsGroupBox);
			this.Name = "InventoryBOMComponentsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(747, 563, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ComponentDetailsGroupBox.ResumeLayout(false);
			this.ComponentDetailsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ComponentDetailsGrid)).EndInit();
			this.ComponentDetailsGrid.ResumeLayout(false);
			this.ComponentDetailsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		private ZArchitecture.GUI.ZGroupBox ComponentDetailsGroupBox;
		private ZArchitecture.ZGrid ComponentDetailsGrid;

		#endregion
	}
}
