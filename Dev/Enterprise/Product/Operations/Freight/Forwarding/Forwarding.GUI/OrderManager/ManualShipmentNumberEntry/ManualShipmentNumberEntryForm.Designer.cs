namespace Enterprise.Freight.Forwarding.Orders.GUI
{
	partial class ManualShipmentNumberEntryForm
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
		protected new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo1 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo2 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			this.ShipmentsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.OKButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.TheCancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ShipmentsGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 295, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(342, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Forwarding.Business.ShipmentNumberEntries);
			// 
			// ShipmentsGrid
			// 
			this.ShipmentsGrid.AllowBeginDrag = false;
			this.ShipmentsGrid.AllowCopyToNewRowMenuItem = false;
			this.ShipmentsGrid.AllowDragDropWithChanges = false;
			this.ShipmentsGrid.AllowNavigation = false;
			this.ShipmentsGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ShipmentsGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ShipmentNumberEntry)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.ShipmentNumberEntry)(null)).ShipmentNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.Forwarding.Business.ShipmentNumberEntry)(null)).ConsignorPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.Forwarding.Business.ShipmentNumberEntry)(null)).ConsigneePK)));
			this.ShipmentsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ManualShipmentNumberEntryForm|09d3c20d-ab03-4432-b1ae-26ed1c73a9ff", "Shipment#");
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "ShipmentNumber";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(95);
			zOrganisationFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ManualShipmentNumberEntryForm|3331d5e1-a21f-40db-ad4c-311c0749f885", "Consignor");
			zOrganisationFindBoxColumnStyleInfo1.ColumnName = "ConsignorPK";
			zOrganisationFindBoxColumnStyleInfo1.IsReadOnly = true;
			zOrganisationFindBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ManualShipmentNumberEntryForm|7a9607ba-2570-45d5-ab06-7640cb6ea1bb", "Consignee");
			zOrganisationFindBoxColumnStyleInfo2.ColumnName = "ConsigneePK";
			zOrganisationFindBoxColumnStyleInfo2.IsReadOnly = true;
			this.ShipmentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ShipmentsGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo1);
			this.ShipmentsGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo2);
			this.ShipmentsGrid.CopySelectedRowsAllowed = true;
			this.ShipmentsGrid.GridId = "1e1ae49e-a07e-4b34-ab78-686f3ed96e9d";
			this.ShipmentsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ShipmentsGrid.LayoutKey = "ShipmentsGrid";
			this.ShipmentsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 42, true);
			this.ShipmentsGrid.Name = "ShipmentsGrid";
			this.ShipmentsGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.ShipmentsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(318, 216, true);
			this.ShipmentsGrid.TabIndex = 1;
			// 
			// OKButton
			// 
			this.OKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OKButton.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ManualShipmentNumberEntryForm|ca324383-6348-448f-bfbc-6bcf37e68180", "OK");
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 264, true);
			this.OKButton.Name = "OKButton";
			this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(113, 25, true);
			this.OKButton.TabIndex = 2;
			this.OKButton.UseVisualStyleBackColor = true;
			this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
			// 
			// TheCancelButton
			// 
			this.TheCancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.TheCancelButton.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ManualShipmentNumberEntryForm|d69d8fec-ddb7-4e28-9f08-4b07ce3f9b81", "Cancel");
			this.TheCancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(217, 264, true);
			this.TheCancelButton.Name = "TheCancelButton";
			this.TheCancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(113, 25, true);
			this.TheCancelButton.TabIndex = 3;
			this.TheCancelButton.UseVisualStyleBackColor = true;
			this.TheCancelButton.Click += new System.EventHandler(this.CancelButton_Click);
			// 
			// zLabel1
			// 
			this.zLabel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.zLabel1.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ManualShipmentNumberEntryForm|d38f5db4-ef22-458c-be31-cc6f0831b226", "Please enter Shipment numbers or leave blank for the system to Auto-Generate.");
			this.zLabel1.ForeColor = System.Drawing.Color.Green;
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 9, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(318, 27, true);
			this.zLabel1.TabIndex = 4;
			this.zLabel1.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			// 
			// ManualShipmentNumberEntryForm
			// 
			this.AcceptButton = this.OKButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ManualShipmentNumberEntryForm|dd2d45e3-c3ac-47f3-b446-84d4eb210ae5", "Manual Shipment Entry");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(342, 319, true);
			this.ControlBox = false;
			this.Controls.Add(this.zLabel1);
			this.Controls.Add(this.TheCancelButton);
			this.Controls.Add(this.ShipmentsGrid);
			this.Controls.Add(this.OKButton);
			this.DataSourceAssemblyName = "Enterprise.Freight.Forwarding.Business";
			this.DataSourceType = typeof(Enterprise.Freight.Forwarding.Business.ShipmentNumberEntries);
			this.DataSourceTypeName = "Enterprise.Freight.Forwarding.Business.ShipmentNumberEntries";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(346, 272, true);
			this.Name = "ManualShipmentNumberEntryForm";
			this.Controls.SetChildIndex(this.OKButton, 0);
			this.Controls.SetChildIndex(this.ShipmentsGrid, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.TheCancelButton, 0);
			this.Controls.SetChildIndex(this.zLabel1, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ShipmentsGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private Enterprise.ZArchitecture.ZGrid ShipmentsGrid;
		private Enterprise.ZArchitecture.GUI.ZButton OKButton;
		private Enterprise.ZArchitecture.GUI.ZButton TheCancelButton;
		private Enterprise.ZArchitecture.ZLabel zLabel1;

	}
}
