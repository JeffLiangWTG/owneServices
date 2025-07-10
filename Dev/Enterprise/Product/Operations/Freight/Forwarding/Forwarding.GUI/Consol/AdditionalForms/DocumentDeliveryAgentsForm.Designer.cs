using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Enterprise.Freight.Forwarding.GUI
{
	public partial class DocumentDeliveryAgentsForm
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.SelectDeliveryAgentsLabel = new Enterprise.ZArchitecture.ZLabel();
			this.DeliveryAgentsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.PrintButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CancelPrintButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.DeliveryAgentsGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 320, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(429, 22, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Forwarding.Business.DocumentDeliveryAgents);
			// 
			// SelectDeliveryAgentsLabel
			// 
			this.SelectDeliveryAgentsLabel.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("DocumentDeliveryAgentsForm|3d5735ef-115d-472c-ab83-adb9ac585e90", "Select Delivery Agents");
			this.SelectDeliveryAgentsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 6, true);
			this.SelectDeliveryAgentsLabel.Name = "SelectDeliveryAgentsLabel";
			this.SelectDeliveryAgentsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 21, true);
			this.SelectDeliveryAgentsLabel.TabIndex = 1;
			// 
			// DeliveryAgentsGrid
			// 
			this.DeliveryAgentsGrid.AllowNavigation = false;
			this.DeliveryAgentsGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.DeliveryAgentsGrid, "DeliveryAgentsToSelectFrom");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.DocumentDeliveryAgents)(null)).DeliveryAgentsToSelectFrom)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.DeliveryAgentToSelectFromForPrinting)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.DocumentDeliveryAgents)(null)).DeliveryAgentsToSelectFrom)).SyncRoot)).OH_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.DeliveryAgentToSelectFromForPrinting)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.DocumentDeliveryAgents)(null)).DeliveryAgentsToSelectFrom)).SyncRoot)).OH_FullName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Forwarding.Business.DeliveryAgentToSelectFromForPrinting)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.DocumentDeliveryAgents)(null)).DeliveryAgentsToSelectFrom)).SyncRoot)).OH_Calc_PrintDocumentForDeliveryAgent)));
			this.DeliveryAgentsGrid.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.DeliveryAgentsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo3.ColumnName = "OH_Code";
			zTextBoxColumnStyleInfo3.IsMandatory = true;
			zTextBoxColumnStyleInfo4.ColumnName = "OH_FullName";
			zTextBoxColumnStyleInfo4.IsMandatory = true;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zCheckBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("DocumentDeliveryAgentsForm|82751d6e-0056-4b1f-b5d3-56c3f5790c76", "Print Document");
			zCheckBoxColumnStyleInfo2.ColumnName = "OH_Calc_PrintDocumentForDeliveryAgent";
			zCheckBoxColumnStyleInfo2.IsMandatory = true;
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(85);
			this.DeliveryAgentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.DeliveryAgentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.DeliveryAgentsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.DeliveryAgentsGrid.GridId = "c1d7498f-ba71-4708-b690-bf66ff4405c0";
			this.DeliveryAgentsGrid.CopySelectedRowsAllowed = true;
			this.DeliveryAgentsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.DeliveryAgentsGrid.LayoutKey = "DeliveryAgentsGrid";
			this.DeliveryAgentsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 30, true);
			this.DeliveryAgentsGrid.Name = "DeliveryAgentsGrid";
			this.DeliveryAgentsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(415, 257, true);
			this.DeliveryAgentsGrid.TabIndex = 2;
			// 
			// PrintButton
			// 
			this.PrintButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.PrintButton.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("DocumentDeliveryAgentsForm|94c27001-6322-468f-8898-f9c351048001", "Print");
			this.PrintButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(266, 293, true);
			this.PrintButton.Name = "PrintButton";
			this.PrintButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.PrintButton.TabIndex = 3;
			// 
			// CancelPrintButton
			// 
			this.CancelPrintButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelPrintButton.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("DocumentDeliveryAgentsForm|013641f5-56de-4c8a-8f89-ee39f29921ed", "Cancel");
			this.CancelPrintButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelPrintButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(347, 293, true);
			this.CancelPrintButton.Name = "CancelPrintButton";
			this.CancelPrintButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.CancelPrintButton.TabIndex = 4;
			// 
			// DocumentDeliveryAgentsForm
			// 
			this.AcceptButton = this.PrintButton;
			
			this.CancelButton = this.CancelPrintButton;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(429, 342, true);
			this.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("DocumentDeliveryAgentsForm|b03ebc1b-9b53-4774-b745-89b1c6940961", "Delivery Agents Document Pack");
			this.Controls.Add(this.CancelPrintButton);
			this.Controls.Add(this.DeliveryAgentsGrid);
			this.Controls.Add(this.PrintButton);
			this.Controls.Add(this.SelectDeliveryAgentsLabel);
			this.DataSourceAssemblyName = "Enterprise.Freight.Forwarding.Business";
			this.DataSourceType = typeof(Enterprise.Freight.Forwarding.Business.DocumentDeliveryAgents);
			this.DataSourceTypeName = "Enterprise.Freight.Forwarding.Business.DocumentDeliveryAgents";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(435, 370, true);
			this.MinimizeBox = false;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(435, 370, true);
			this.Name = "DocumentDeliveryAgentsForm";
			this.RememberFormPosition = false;
			this.RememberFormSize = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Controls.SetChildIndex(this.SelectDeliveryAgentsLabel, 0);
			this.Controls.SetChildIndex(this.PrintButton, 0);
			this.Controls.SetChildIndex(this.DeliveryAgentsGrid, 0);
			this.Controls.SetChildIndex(this.CancelPrintButton, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.DeliveryAgentsGrid)).EndInit();
			this.ResumeLayout(false);

		}

		Enterprise.ZArchitecture.ZLabel SelectDeliveryAgentsLabel;
		Enterprise.ZArchitecture.ZGrid DeliveryAgentsGrid;
		Enterprise.ZArchitecture.GUI.ZButton PrintButton;
		Enterprise.ZArchitecture.GUI.ZButton CancelPrintButton;
	}
}
