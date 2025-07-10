namespace Enterprise.Customs.Module
{
	partial class DeclarationFromShipmentPullerDialog
	{
		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			this.ShipmentGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.CreateButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.DescriptionLabel = new Enterprise.ZArchitecture.ZLabel();
			this.CancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 74, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(346, 22, true);
			this.MainStatusBar.SizingGrip = false;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(146);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(146);
			// 
			// ShipmentGuidFindBox
			// 
			this.ShipmentGuidFindBox.BindTo = "ShipmentPK";
			this.ShipmentGuidFindBox.BindToList = "Shipments";
			this.ShipmentGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 38, true);
			this.ShipmentGuidFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.JobShipment;
			this.ShipmentGuidFindBox.Name = "ShipmentGuidFindBox";
			this.ShipmentGuidFindBox.ShowDescriptionBox = false;
			this.ShipmentGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 21, true);
			this.ShipmentGuidFindBox.TabIndex = 0;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ShipmentGuidFindBox, false);
			// 
			// CreateButton
			// 
			this.CreateButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 37, true);
			this.CreateButton.Name = "CreateButton";
			this.CreateButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 21, true);
			this.CreateButton.TabIndex = 2;
			this.CreateButton.CaptionResourceString = Enterprise.Customs.Module.Res.GetData("2D14CF40-FE75-4A3D-B5F3-E9C92DFF6024", "Create Brokerage Job");
			this.CreateButton.Click += new System.EventHandler(this.CreateButton_Click);
			// 
			// DescriptionLabel
			// 
			this.DescriptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 7, true);
			this.DescriptionLabel.Name = "DescriptionLabel";
			this.DescriptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 22, true);
			this.DescriptionLabel.TabIndex = 3;
			this.DescriptionLabel.CaptionResourceString = Enterprise.Customs.Module.Res.GetData("3F71B48F-F668-4026-8C35-A87915E5B35B", "Select the shipment that you want to start a brokerage entry for");
			// 
			// CancelButton
			// 
			this.CancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(256, 37, true);
			this.CancelButton.Name = "CancelButton";
			this.CancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.CancelButton.TabIndex = 3;
			this.CancelButton.CaptionResourceString = Enterprise.Customs.Module.Res.GetData("965B4910-F439-448B-BCFF-EE1D4F720E9A", "Cancel");
			this.CancelButton.Click += new System.EventHandler(this.CancelButton_Click);
			// 
			// DeclarationFromShipmentPullerDialog
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(346, 96, true);
			this.Controls.Add(this.CancelButton);
			this.Controls.Add(this.DescriptionLabel);
			this.Controls.Add(this.ShipmentGuidFindBox);
			this.Controls.Add(this.CreateButton);
			this.DataSourceAssemblyName = "Enterprise.Customs.Business";
			this.DataSourceTypeName = "Enterprise.Customs.Business.DeclarationFromShipmentPuller";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MinimizeBox = false;
			this.Name = "DeclarationFromShipmentPullerDialog";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.CaptionResourceString = Enterprise.Customs.Module.Res.GetData("19C384A4-7E72-410B-92C5-A0A81B968E12", "Select Shipment");
			this.Controls.SetChildIndex(this.CreateButton, 0);
			this.Controls.SetChildIndex(this.ShipmentGuidFindBox, 0);
			this.Controls.SetChildIndex(this.DescriptionLabel, 0);
			this.Controls.SetChildIndex(this.CancelButton, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			this.ResumeLayout(false);
		}
		#endregion

		private Enterprise.ZArchitecture.GUI.ZGuidFindBox ShipmentGuidFindBox;
		private Enterprise.ZArchitecture.ZLabel DescriptionLabel;
		public new Enterprise.ZArchitecture.GUI.ZButton CancelButton;
		public Enterprise.ZArchitecture.GUI.ZButton CreateButton;
	}
}
