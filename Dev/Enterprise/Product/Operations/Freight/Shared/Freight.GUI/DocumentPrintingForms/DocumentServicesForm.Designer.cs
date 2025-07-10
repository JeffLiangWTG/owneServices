using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Enterprise.Freight.GUI
{
	public partial class DocumentServicesForm
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

		new void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.ServicesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.PrintButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CancelPrintButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.SelectContainerLegsLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ServicesGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 320, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(464, 22, true);
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(244);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(245);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.DocumentServices);
			// 
			// ServicesGrid
			// 
			this.ServicesGrid.AllowNavigation = false;
			this.ServicesGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ServicesGrid, "ServiceToSelectFrom");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.DocumentServices)(null)).ServiceToSelectFrom)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ServiceToSelectFromForPrinting)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.DocumentServices)(null)).ServiceToSelectFrom)).SyncRoot)).Service.ES_ServiceCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ServiceToSelectFromForPrinting)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.DocumentServices)(null)).ServiceToSelectFrom)).SyncRoot)).Service.ES_Calc_Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.ServiceToSelectFromForPrinting)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.DocumentServices)(null)).ServiceToSelectFrom)).SyncRoot)).Service.ES_Booked)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ServiceToSelectFromForPrinting)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.DocumentServices)(null)).ServiceToSelectFrom)).SyncRoot)).Service.ES_References)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.ServiceToSelectFromForPrinting)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.DocumentServices)(null)).ServiceToSelectFrom)).SyncRoot)).ES_Calc_PrintDocumentForService)));
			this.ServicesGrid.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.ServicesGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("DocumentServicesForm|cc0e136c-1f61-4e28-b617-c81917283476", "Type");
			zDropEditColumnStyleInfo1.ColumnName = "Service+ES_ServiceCode";
			zDropEditColumnStyleInfo1.IsReadOnly = true;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("DocumentServicesForm|6527f640-a60d-4315-919c-806f3db92933", "Description");
			zTextBoxColumnStyleInfo1.ColumnName = "Service+ES_Calc_Description";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("DocumentServicesForm|ac8af331-f6c8-4e25-b682-18ac61df285e", "Booked Date");
			zDateEditColumnStyleInfo1.ColumnName = "Service+ES_Booked";
			zDateEditColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("DocumentServicesForm|24032e50-acef-41c6-bc1d-8d813f327332", "Ref No.");
			zTextBoxColumnStyleInfo2.ColumnName = "Service+ES_References";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("DocumentServicesForm|c90a5436-977e-4203-9448-920b27a30711", "Print");
			zCheckBoxColumnStyleInfo1.ColumnName = "ES_Calc_PrintDocumentForService";
			this.ServicesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.ServicesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ServicesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.ServicesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ServicesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.ServicesGrid.GridId = "9056f7f4-e354-4c43-a03b-aa1c4a9fcf42";
			this.ServicesGrid.CopySelectedRowsAllowed = true;
			this.ServicesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ServicesGrid.LayoutKey = "ServicesGrid";
			this.ServicesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 30, true);
			this.ServicesGrid.Name = "ServicesGrid";
			this.ServicesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(450, 250, true);
			this.ServicesGrid.TabIndex = 1;
			// 
			// PrintButton
			// 
			this.PrintButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.PrintButton.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("DocumentServicesForm|5cd2f1d1-adaf-483d-9912-7dabd20e294f", "Print");
			this.PrintButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(301, 293, true);
			this.PrintButton.Name = "PrintButton";
			this.PrintButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.PrintButton.TabIndex = 2;
			// 
			// CancelPrintButton
			// 
			this.CancelPrintButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelPrintButton.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("DocumentServicesForm|8067732e-3e34-4bb4-a9e2-ad6ecaaf323a", "Cancel");
			this.CancelPrintButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelPrintButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(382, 293, true);
			this.CancelPrintButton.Name = "CancelPrintButton";
			this.CancelPrintButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.CancelPrintButton.TabIndex = 3;
			// 
			// SelectContainerLegsLabel
			// 
			this.SelectContainerLegsLabel.AutoSize = true;
			this.SelectContainerLegsLabel.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("DocumentServicesForm|4295aebd-41b1-40d5-a94b-4d9709f10988", "Select Services To Print");
			this.SelectContainerLegsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 9, true);
			this.SelectContainerLegsLabel.Name = "SelectContainerLegsLabel";
			this.SelectContainerLegsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(119, 13, true);
			this.SelectContainerLegsLabel.TabIndex = 264;
			// 
			// DocumentServicesForm
			// 
			this.AcceptButton = this.PrintButton;
			this.CancelButton = this.CancelPrintButton;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(464, 342, true);
			this.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("DocumentServicesForm|6e83d000-764e-423a-a5b9-95484a44f1c2", "Services");
			this.Controls.Add(this.SelectContainerLegsLabel);
			this.Controls.Add(this.PrintButton);
			this.Controls.Add(this.CancelPrintButton);
			this.Controls.Add(this.ServicesGrid);
			this.DataSourceAssemblyName = "Enterprise.Freight";
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.DocumentServices);
			this.DataSourceTypeName = "Enterprise.MasterFiles.Business.DocumentServices";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(470, 370, true);
			this.MinimizeBox = false;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(470, 370, true);
			this.Name = "DocumentServicesForm";
			this.RememberFormPosition = false;
			this.RememberFormSize = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.ServicesGrid, 0);
			this.Controls.SetChildIndex(this.CancelPrintButton, 0);
			this.Controls.SetChildIndex(this.PrintButton, 0);
			this.Controls.SetChildIndex(this.SelectContainerLegsLabel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ServicesGrid)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		Enterprise.ZArchitecture.ZGrid ServicesGrid;
		Enterprise.ZArchitecture.GUI.ZButton PrintButton;
		Enterprise.ZArchitecture.GUI.ZButton CancelPrintButton;
		Enterprise.ZArchitecture.ZLabel SelectContainerLegsLabel;
	}
}
