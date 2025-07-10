using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Enterprise.Freight.Confirmations.GUI
{
	public partial class DocumentPickupDeliveryConfirmForm
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
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			this.PrintButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CancelPrintButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.PickupDeliveryConfirmGrid = new Enterprise.ZArchitecture.ZGrid();
			this.SelectPickupDeliveryConfirmLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ShipmentTransportCoLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.PickupDeliveryConfirmGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 296, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(681, 22, true);
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
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Business.DocumentPickupDeliveryConfirm);
			// 
			// PrintButton
			// 
			this.PrintButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.PrintButton.CaptionResourceString = Enterprise.Freight.Confirmations.GUI.Res.GetData("DocumentPickupDeliveryConfirmForm|51055ebf-0b8e-426e-8406-0c5219b50922", "Print");
			this.PrintButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(518, 269, true);
			this.PrintButton.Name = "PrintButton";
			this.PrintButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.PrintButton.TabIndex = 2;
			this.PrintButton.Click += new System.EventHandler(this.PrintButton_Click);
			// 
			// CancelPrintButton
			// 
			this.CancelPrintButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelPrintButton.CaptionResourceString = Enterprise.Freight.Confirmations.GUI.Res.GetData("DocumentPickupDeliveryConfirmForm|72e7825e-ca98-4ecd-bf04-305a23d68dd1", "Cancel");
			this.CancelPrintButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelPrintButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(598, 269, true);
			this.CancelPrintButton.Name = "CancelPrintButton";
			this.CancelPrintButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.CancelPrintButton.TabIndex = 3;
			this.CancelPrintButton.Click += new System.EventHandler(this.CancelPrintButton_Click);
			// 
			// PickupDeliveryConfirmGrid
			// 
			this.PickupDeliveryConfirmGrid.AllowNavigation = false;
			this.PickupDeliveryConfirmGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.PickupDeliveryConfirmGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.Business.DocumentPickupDeliveryConfirm)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Business.DocumentPickupDeliveryConfirm)(null)).PrintConfirm)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.DocumentPickupDeliveryConfirm)(null)).Identifier)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.Business.DocumentPickupDeliveryConfirm)(null)).Confirm.EU_PlannedPickupDeliveryTime)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.Business.DocumentPickupDeliveryConfirm)(null)).Confirm.EU_RequestedPickupDeliveryTime)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.DocumentPickupDeliveryConfirm)(null)).Confirm.EU_TransportCoName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.DocumentPickupDeliveryConfirm)(null)).Confirm.EU_DriversName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.DocumentPickupDeliveryConfirm)(null)).Confirm.EU_VehicleRegistration)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.DocumentPickupDeliveryConfirm)(null)).Confirm.UniqueID)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.Business.DocumentPickupDeliveryConfirm)(null)).Confirm.EU_PickupDeliveryTime)));
			this.PickupDeliveryConfirmGrid.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.PickupDeliveryConfirmGrid.CaptionVisible = false;
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.Confirmations.GUI.Res.GetData("DocumentPickupDeliveryConfirmForm|dd9bd2fe-2f89-4ac4-9917-9355e659541b", "Print Confirm");
			zCheckBoxColumnStyleInfo1.ColumnName = "PrintConfirm";
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.Confirmations.GUI.Res.GetData("DocumentPickupDeliveryConfirmForm|990d1ece-4f52-48fb-88a6-0ea8575b5dca", "Description");
			zTextBoxColumnStyleInfo1.ColumnName = "Identifier";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDateEditColumnStyleInfo1.ColumnName = "Confirm+EU_PlannedPickupDeliveryTime";
			zDateEditColumnStyleInfo1.IsReadOnly = true;
			zDateEditColumnStyleInfo2.ColumnName = "Confirm+EU_RequestedPickupDeliveryTime";
			zDateEditColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.ColumnName = "Confirm+EU_TransportCoName";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zTextBoxColumnStyleInfo3.ColumnName = "Confirm+EU_DriversName";
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			zTextBoxColumnStyleInfo4.ColumnName = "Confirm+EU_VehicleRegistration";
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Freight.Confirmations.GUI.Res.GetData("DocumentPickupDeliveryConfirmForm|7c14bc9d-17f1-4d14-88d9-b4527e42ecd6", "Confirm", "Confirm", "");
			zTextBoxColumnStyleInfo5.ColumnName = "Confirm+UniqueID";
			zTextBoxColumnStyleInfo5.IsReadOnly = true;
			zTextBoxColumnStyleInfo5.IsVisible = false;
			zDateEditColumnStyleInfo3.ColumnName = "Confirm+EU_PickupDeliveryTime";
			zDateEditColumnStyleInfo3.IsReadOnly = true;
			this.PickupDeliveryConfirmGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.PickupDeliveryConfirmGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.PickupDeliveryConfirmGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.PickupDeliveryConfirmGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.PickupDeliveryConfirmGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.PickupDeliveryConfirmGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.PickupDeliveryConfirmGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.PickupDeliveryConfirmGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.PickupDeliveryConfirmGrid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.PickupDeliveryConfirmGrid.GridId = "506c7e81-3132-4e04-81bb-eceee351af44";
			this.PickupDeliveryConfirmGrid.CopySelectedRowsAllowed = true;
			this.PickupDeliveryConfirmGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.PickupDeliveryConfirmGrid.LayoutKey = "zGrid1";
			this.PickupDeliveryConfirmGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 53, true);
			this.PickupDeliveryConfirmGrid.Name = "PickupDeliveryConfirmGrid";
			this.PickupDeliveryConfirmGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(665, 210, true);
			this.PickupDeliveryConfirmGrid.TabIndex = 1;
			// 
			// SelectPickupDeliveryConfirmLabel
			// 
			this.SelectPickupDeliveryConfirmLabel.AutoSize = true;
			this.SelectPickupDeliveryConfirmLabel.CaptionResourceString = Enterprise.Freight.Confirmations.GUI.Res.GetData("DocumentPickupDeliveryConfirmForm|9b468508-b367-4156-bf89-7e0a4fa67e59", "Select Confirms To Print");
			this.SelectPickupDeliveryConfirmLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 7, true);
			this.SelectPickupDeliveryConfirmLabel.Name = "SelectPickupDeliveryConfirmLabel";
			this.SelectPickupDeliveryConfirmLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(121, 13, true);
			this.SelectPickupDeliveryConfirmLabel.TabIndex = 5;
			// 
			// ShipmentTransportCoLabel
			// 
			this.ShipmentTransportCoLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 27, true);
			this.ShipmentTransportCoLabel.Name = "ShipmentTransportCoLabel";
			this.ShipmentTransportCoLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(500, 13, true);
			this.ShipmentTransportCoLabel.TabIndex = 6;
			this.ShipmentTransportCoLabel.Visible = false;
			// 
			// DocumentPickupDeliveryConfirmForm
			// 
			this.AcceptButton = this.PrintButton;

			this.CancelButton = this.CancelPrintButton;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(681, 318, true);
			this.CaptionResourceString = Enterprise.Freight.Confirmations.GUI.Res.GetData("DocumentPickupDeliveryConfirmForm|8355b7cd-4c98-4515-afda-9ce42df38171", "Select Confirms");
			this.Controls.Add(this.ShipmentTransportCoLabel);
			this.Controls.Add(this.PrintButton);
			this.Controls.Add(this.CancelPrintButton);
			this.Controls.Add(this.PickupDeliveryConfirmGrid);
			this.Controls.Add(this.SelectPickupDeliveryConfirmLabel);
			this.DataSourceAssemblyName = "Enterprise.Freight";
			this.DataSourceType = typeof(Enterprise.Freight.Business.DocumentPickupDeliveryConfirm);
			this.DataSourceTypeName = "Enterprise.Freight.Business.DocumentPickupDeliveryConfirm";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(687, 346, true);
			this.MinimizeBox = false;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(687, 346, true);
			this.Name = "DocumentPickupDeliveryConfirmForm";
			this.RememberFormPosition = false;
			this.RememberFormSize = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.SelectPickupDeliveryConfirmLabel, 0);
			this.Controls.SetChildIndex(this.PickupDeliveryConfirmGrid, 0);
			this.Controls.SetChildIndex(this.CancelPrintButton, 0);
			this.Controls.SetChildIndex(this.PrintButton, 0);
			this.Controls.SetChildIndex(this.ShipmentTransportCoLabel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.PickupDeliveryConfirmGrid)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		Enterprise.ZArchitecture.ZLabel ShipmentTransportCoLabel;
		Enterprise.ZArchitecture.GUI.ZButton PrintButton;
		Enterprise.ZArchitecture.GUI.ZButton CancelPrintButton;
		Enterprise.ZArchitecture.ZGrid PickupDeliveryConfirmGrid;
		Enterprise.ZArchitecture.ZLabel SelectPickupDeliveryConfirmLabel;
	}
}
