namespace Enterprise.Customs.US.eManifest.GUI
{
	partial class SendingActionForm
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.CancellButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.SendButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ShipmentsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ShipmentsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ShipmentsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ShipmentsGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 238, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(434, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.eManifest.Business.IShipmentActionsProvider);
			// 
			// CancellButton
			// 
			this.CancellButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CancellButton.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("07a3a399-d5e6-40c0-b93c-0a10b78b9bb7", "Cancel");
			this.CancellButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancellButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(347, 209, true);
			this.CancellButton.Name = "CancellButton";
			this.CancellButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CancellButton.TabIndex = 1;
			this.CancellButton.UseVisualStyleBackColor = true;
			// 
			// SendButton
			// 
			this.SendButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.SendButton.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("a74ae382-d9d0-4200-9e1d-6c0a96c7e3dc", "Send");
			this.SendButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(266, 209, true);
			this.SendButton.Name = "SendButton";
			this.SendButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.SendButton.TabIndex = 1;
			this.SendButton.UseVisualStyleBackColor = true;
			this.SendButton.Click += new System.EventHandler(this.SendButton_Click);
			// 
			// ShipmentsGroupBox
			// 
			this.ShipmentsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.ShipmentsGroupBox.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("031df05d-ccc0-41ec-91e0-9a5fd444de14", "Shipments");
			this.ShipmentsGroupBox.Controls.Add(this.ShipmentsGrid);
			this.ShipmentsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ShipmentsGroupBox.Name = "ShipmentsGroupBox";
			this.ShipmentsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(434, 203, true);
			this.ShipmentsGroupBox.TabIndex = 2;
			this.ShipmentsGroupBox.TabStop = false;
			// 
			// ShipmentsGrid
			// 
			this.ShipmentsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ShipmentsGrid, "ShipmentsActions");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.eManifest.Business.IShipmentActionsProvider)(null)).ShipmentsActions)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.eManifest.Business.ShipmentAction)(((System.Collections.IList)(((Enterprise.Customs.US.eManifest.Business.IShipmentActionsProvider)(null)).ShipmentsActions)).SyncRoot)).B0_ShipmentControlNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.eManifest.Business.ShipmentAction)(((System.Collections.IList)(((Enterprise.Customs.US.eManifest.Business.IShipmentActionsProvider)(null)).ShipmentsActions)).SyncRoot)).B0_ActionCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.eManifest.Business.ShipmentAction)(((System.Collections.IList)(((Enterprise.Customs.US.eManifest.Business.IShipmentActionsProvider)(null)).ShipmentsActions)).SyncRoot)).B0_ActionCodeDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.eManifest.Business.ShipmentAction)(((System.Collections.IList)(((Enterprise.Customs.US.eManifest.Business.IShipmentActionsProvider)(null)).ShipmentsActions)).SyncRoot)).B0_AmendmentReason)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.eManifest.Business.ShipmentAction)(((System.Collections.IList)(((Enterprise.Customs.US.eManifest.Business.IShipmentActionsProvider)(null)).ShipmentsActions)).SyncRoot)).Shipment.B0_ReleaseStatusCodeDescription)));
			this.ShipmentsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.Caption = null;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("e59910aa-9eed-437e-8be7-7e8bdad5d820", "SCN", "Shipment Control Number", "");
			zTextBoxColumnStyleInfo1.ColumnName = "B0_ShipmentControlNumber";
			zTextBoxColumnStyleInfo1.GroupName = null;
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			zDropEditColumnStyleInfo1.Caption = null;
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("dc221cac-a707-48d7-bbf1-4522bc3aea1d", "Action", "Action Code", "");
			zDropEditColumnStyleInfo1.ColumnName = "B0_ActionCode";
			zDropEditColumnStyleInfo1.GroupName = null;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo2.Caption = null;
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("929edc26-e083-4dfe-b2bf-e892846634e9", "Description", "Action Description", "");
			zTextBoxColumnStyleInfo2.ColumnName = "B0_ActionCodeDescription";
			zTextBoxColumnStyleInfo2.GroupName = null;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDropEditColumnStyleInfo2.Caption = null;
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("01cd2e12-8923-4c61-b876-993b3490c771", "Amend.", "Amendment Reason", "");
			zDropEditColumnStyleInfo2.ColumnName = "B0_AmendmentReason";
			zDropEditColumnStyleInfo2.GroupName = null;
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo3.Caption = null;
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("4b157ed6-a937-49ab-a0ce-1c91f745e725", "Status", "Release Status", "");
			zTextBoxColumnStyleInfo3.ColumnName = "Shipment+B0_ReleaseStatusCodeDescription";
			zTextBoxColumnStyleInfo3.GroupName = null;
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			this.ShipmentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ShipmentsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.ShipmentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ShipmentsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.ShipmentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.ShipmentsGrid.CopySelectedRowsAllowed = true;
			this.ShipmentsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ShipmentsGrid.GridId = "a39f89a8-5ec8-4b2c-a235-fac26bbc5b35";
			this.ShipmentsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ShipmentsGrid.LayoutKey = "ShipmentsGrid";
			this.ShipmentsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.ShipmentsGrid.Name = "ShipmentsGrid";
			this.ShipmentsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(428, 184, true);
			this.ShipmentsGrid.TabIndex = 0;
			// 
			// SendingActionForm
			// 
			this.AutoAddPreviousNextButtons = false;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(434, 262, true);
			this.Controls.Add(this.ShipmentsGroupBox);
			this.Controls.Add(this.SendButton);
			this.Controls.Add(this.CancellButton);
			this.DataSourceType = typeof(Enterprise.Customs.US.eManifest.Business.IShipmentActionsProvider);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(450, 300, true);
			this.Name = "SendingActionForm";
			this.Text = "SendingActionForm";
			this.Controls.SetChildIndex(this.CancellButton, 0);
			this.Controls.SetChildIndex(this.SendButton, 0);
			this.Controls.SetChildIndex(this.ShipmentsGroupBox, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ShipmentsGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.ShipmentsGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private ZArchitecture.GUI.ZButton CancellButton;
		private ZArchitecture.GUI.ZButton SendButton;
		private ZArchitecture.GUI.ZGroupBox ShipmentsGroupBox;
		private ZArchitecture.ZGrid ShipmentsGrid;
	}
}