using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Enterprise.Freight.Forwarding.GUI
{
	public partial class DocumentChargeSheet
	{
		System.ComponentModel.Container components = null;

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		protected new void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.DebtorsSelectLabel = new Enterprise.ZArchitecture.ZLabel();
			this.DebtorsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.PrintButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CancelPrintButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.DebtorsGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 248, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(364, 24, true);
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(133);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(133);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Business.DocumentShipment);
			// 
			// DebtorsSelectLabel
			// 
			this.DebtorsSelectLabel.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("DocumentChargeSheet|c8f1dd92-7048-40cb-ac54-4cf7dcb67c23", "Select Debtors To Print Document For");
			this.DebtorsSelectLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 4, true);
			this.DebtorsSelectLabel.Name = "DebtorsSelectLabel";
			this.DebtorsSelectLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(234, 23, true);
			this.DebtorsSelectLabel.TabIndex = 265;
			// 
			// DebtorsGrid
			// 
			this.DebtorsGrid.AllowNavigation = false;
			this.DebtorsGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.DebtorsGrid, "DebtorsToPrint");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.Business.DocumentShipment)(null)).DebtorsToPrint)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.DebtorToSelectFromForPrinting)(((System.Collections.IList)(((Enterprise.Freight.Business.DocumentShipment)(null)).DebtorsToPrint)).SyncRoot)).Debtor.OH_FullName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Business.DebtorToSelectFromForPrinting)(((System.Collections.IList)(((Enterprise.Freight.Business.DocumentShipment)(null)).DebtorsToPrint)).SyncRoot)).OH_Calc_PrintDebtor)));
			this.DebtorsGrid.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.DebtorsGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("DocumentChargeSheet|d4c86e23-f2e1-4a35-80c3-978fad667749", "Name");
			zDropEditColumnStyleInfo1.ColumnName = "Debtor+OH_FullName";
			zDropEditColumnStyleInfo1.IsReadOnly = true;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(250);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("DocumentChargeSheet|8bd12154-c88d-49c3-acf7-c43ff816b2b9", "Print");
			zCheckBoxColumnStyleInfo1.ColumnName = "OH_Calc_PrintDebtor";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			this.DebtorsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.DebtorsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.DebtorsGrid.GridId = "c785a34c-2197-43ac-af2c-9f503cace583";
			this.DebtorsGrid.CopySelectedRowsAllowed = true;
			this.DebtorsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.DebtorsGrid.LayoutKey = "ServicesGrid";
			this.DebtorsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 30, true);
			this.DebtorsGrid.Name = "DebtorsGrid";
			this.DebtorsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 181, true);
			this.DebtorsGrid.TabIndex = 266;
			// 
			// PrintButton
			// 
			this.PrintButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.PrintButton.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("DocumentChargeSheet|659f6442-bafa-439a-854d-b1fcda8314d3", "Print");
			this.PrintButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(201, 219, true);
			this.PrintButton.Name = "PrintButton";
			this.PrintButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.PrintButton.TabIndex = 267;
			// 
			// CancelPrintButton
			// 
			this.CancelPrintButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelPrintButton.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("DocumentChargeSheet|57a489b1-ff14-4bbe-85aa-ed19817122ac", "Cancel");
			this.CancelPrintButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelPrintButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(282, 219, true);
			this.CancelPrintButton.Name = "CancelPrintButton";
			this.CancelPrintButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CancelPrintButton.TabIndex = 268;
			// 
			// DocumentChargeSheet
			// 
			this.AcceptButton = this.PrintButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.CancelPrintButton;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(364, 272, true);
			this.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("DocumentChargeSheet|e0eec9cf-951e-4202-916b-92e4bacbf502", "Debtors");
			this.Controls.Add(this.DebtorsGrid);
			this.Controls.Add(this.DebtorsSelectLabel);
			this.Controls.Add(this.PrintButton);
			this.Controls.Add(this.CancelPrintButton);
			this.DataSourceAssemblyName = "Enterprise.Freight";
			this.DataSourceType = typeof(Enterprise.Freight.Business.DocumentShipment);
			this.DataSourceTypeName = "Enterprise.Freight.Business.DocumentShipment";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(370, 300, true);
			this.MinimizeBox = false;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(370, 300, true);
			this.Name = "DocumentChargeSheet";
			this.RememberFormPosition = false;
			this.RememberFormSize = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Controls.SetChildIndex(this.CancelPrintButton, 0);
			this.Controls.SetChildIndex(this.PrintButton, 0);
			this.Controls.SetChildIndex(this.DebtorsSelectLabel, 0);
			this.Controls.SetChildIndex(this.DebtorsGrid, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.DebtorsGrid)).EndInit();
			this.ResumeLayout(false);

		}

		Enterprise.ZArchitecture.ZGrid DebtorsGrid;
		Enterprise.ZArchitecture.GUI.ZButton PrintButton;
		Enterprise.ZArchitecture.GUI.ZButton CancelPrintButton;
		Enterprise.ZArchitecture.ZLabel DebtorsSelectLabel;
	}
}
