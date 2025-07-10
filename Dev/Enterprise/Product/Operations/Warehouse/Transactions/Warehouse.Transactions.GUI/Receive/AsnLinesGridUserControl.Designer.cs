using System.Windows.Forms;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Warehouse.Transactions.GUI
{
	partial class AsnLinesGridUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		internal SerialNumberUserControl SerialNumberControl;

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			this.Grid = new Enterprise.ZArchitecture.ZGrid();
			this.SerialNumberControl = new SerialNumberUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.Grid)).BeginInit();
			this.Grid.SuspendLayout();
			this.SerialNumberControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Warehouse.Transactions.Business.WhsReceive);
			// 
			// Grid
			// 
			this.Grid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.Grid, "AsnLines");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsReceive)(null)).AsnLines)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Warehouse.Transactions.Business.WhsAsnLine)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsReceive)(null)).AsnLines)).SyncRoot)).WN_OP)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Transactions.Business.WhsAsnLine)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsReceive)(null)).AsnLines)).SyncRoot)).WN_OP_Desc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Warehouse.Transactions.Business.WhsAsnLine)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsReceive)(null)).AsnLines)).SyncRoot)).WN_Quantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Transactions.Business.WhsAsnLine)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsReceive)(null)).AsnLines)).SyncRoot)).WN_QuantityUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Transactions.Business.WhsAsnLine)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsReceive)(null)).AsnLines)).SyncRoot)).CommodityCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Transactions.Business.WhsAsnLine)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsReceive)(null)).AsnLines)).SyncRoot)).WN_PalletId)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Transactions.Business.WhsAsnLine)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsReceive)(null)).AsnLines)).SyncRoot)).WN_PartAttrib1)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Transactions.Business.WhsAsnLine)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsReceive)(null)).AsnLines)).SyncRoot)).WN_PartAttrib2)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Transactions.Business.WhsAsnLine)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsReceive)(null)).AsnLines)).SyncRoot)).WN_PartAttrib3)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Transactions.Business.WhsAsnLine)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsReceive)(null)).AsnLines)).SyncRoot)).WN_SerialNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDate)(((Enterprise.Warehouse.Transactions.Business.WhsAsnLine)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsReceive)(null)).AsnLines)).SyncRoot)).WN_PackingDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDate)(((Enterprise.Warehouse.Transactions.Business.WhsAsnLine)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsReceive)(null)).AsnLines)).SyncRoot)).WN_ExpiryDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Warehouse.Transactions.Business.WhsAsnLine)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsReceive)(null)).AsnLines)).SyncRoot)).WN_LineNo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Warehouse.Transactions.Business.WhsAsnLine)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsReceive)(null)).AsnLines)).SyncRoot)).WN_SubLineNo)));
			this.Grid.CaptionVisible = false;
			zGuidFindBoxColumnStyleInfo1.ColumnName = "WN_OP";
			zGuidFindBoxColumnStyleInfo1.ToolTip = "Enter or select the product code (SKU) . ";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("AsnLinesGridUserControl|ddd6d49b-92d9-445e-b3e8-083c2f7cbd51", "Description");
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "WN_OP_Desc";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "WN_Quantity";
			zCalcEditColumnStyleInfo1.GroupName = Enterprise.Warehouse.Transactions.GUI.Res.GetData("AsnLinesGridUserControl|9017cba7-ed72-4eb6-ab00-37d5bd4022af", "Quantity");
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "WN_QuantityUQ";
			zDropEditColumnStyleInfo1.GroupName = Enterprise.Warehouse.Transactions.GUI.Res.GetData("AsnLinesGridUserControl|9017cba7-ed72-4eb6-ab00-37d5bd4022af", "Quantity");
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("AsnLinesGridUserControl|43596f9f-a930-45b8-88f2-dc970c942633", "Commodity");
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo2.ColumnName = "CommodityCode";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.ColumnName = "WN_PalletId";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo4.ColumnName = "WN_PartAttrib1";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo5.ColumnName = "WN_PartAttrib2";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo6.ColumnName = "WN_PartAttrib3";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo7.ColumnName = "WN_SerialNumber";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo1.ColumnName = "WN_PackingDate";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo2.ColumnName = "WN_ExpiryDate";
			zDateEditColumnStyleInfo2.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "WN_LineNo";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.ColumnName = "WN_SubLineNo";
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.Grid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.Grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.Grid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.Grid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.Grid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.Grid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.Grid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.Grid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.Grid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.Grid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.Grid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.Grid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.Grid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.Grid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.Grid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.Grid.GridId = "f588a032-40fd-4a26-b3e9-a9aa3aec1128";
			this.Grid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.Grid.LayoutKey = "Grid";
			this.Grid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.Grid.Name = "Grid";
			this.Grid.ReadOnly = true;
			this.Grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(684, 150, true);
			this.Grid.TabIndex = 2;
			this.Grid.AfterBind += new System.EventHandler(this.Grid_AfterBind);
			// 
			// SerialNumberControl
			// 
			this.SerialNumberControl.AllowDrop = true;
			this.SerialNumberControl.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right | System.Windows.Forms.AnchorStyles.Bottom)));
			this.BindingSource.SetBindingMember(this.SerialNumberControl, "AsnLines.SerialNumbers");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Warehouse.Transactions.Business.WhsSerialNumberPivotCollection)(((WhsReceiveLine)(((System.Collections.IList)(((WhsReceive)(null)).Lines)).SyncRoot)).SerialNumbers)));
			this.SerialNumberControl.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("AsnLinesGridUserControl|SerialNumbers", "Serial Numbers");
			this.SerialNumberControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(684, 0, true);
			this.SerialNumberControl.Dock = System.Windows.Forms.DockStyle.Right;
			this.SerialNumberControl.Name = "AsnSerialNumberControl";
			this.SerialNumberControl.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(7, 0, 0, 0, true);
			this.SerialNumberControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 150, true);
			this.SerialNumberControl.TabIndex = 3;
			this.SerialNumberControl.Visible = false;
			// 
			// AsnLinesGridUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.SerialNumberControl);
			this.Controls.Add(this.Grid);
			this.Name = "AsnLinesGridUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(684, 150, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.Grid)).EndInit();
			this.Grid.ResumeLayout(false);
			this.Grid.PerformLayout();
			this.SerialNumberControl.ResumeLayout(true);
			this.SerialNumberControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal Enterprise.ZArchitecture.ZGrid Grid;
	}
}
