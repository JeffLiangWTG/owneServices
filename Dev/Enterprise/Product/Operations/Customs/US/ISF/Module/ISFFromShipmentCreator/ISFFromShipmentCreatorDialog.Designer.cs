namespace Enterprise.Customs.US.ISF.Module
{
	partial class ISFFromShipmentCreatorDialog
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
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ShipmentGuidFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 83, true);
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
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.ISF.Business.ISFFromShipmentCreator);
			// 
			// ShipmentGuidFindBox
			// 
			this.ShipmentGuidFindBox.AllowDrop = true;
			this.ShipmentGuidFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.ShipmentGuidFindBox, "ShipmentPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.ISF.Business.ISFFromShipmentCreator)(null)).ShipmentPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.ISF.Business.ISFFromShipmentCreator)(null)).Shipments)));
			this.ShipmentGuidFindBox.BindToList = "Shipments";
			this.ShipmentGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 47, true);
			this.ShipmentGuidFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.JobShipment;
			this.ShipmentGuidFindBox.Name = "ShipmentGuidFindBox";
			this.ShipmentGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.ShipmentGuidFindBox.ParentType = null;
			this.ShipmentGuidFindBox.ShowDescriptionBox = false;
			this.ShipmentGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.ShipmentGuidFindBox.TabIndex = 0;
			// 
			// CreateButton
			// 
			this.CreateButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.CreateButton.IsCaptionOverridden = true;
			this.CreateButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(162, 45, true);
			this.CreateButton.Name = "CreateButton";
			this.CreateButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(90, 23, true);
			this.CreateButton.TabIndex = 2;
			this.CreateButton.Text = "Create ISF Job";
			this.CreateButton.ToolTipCaption = null;
			this.CreateButton.Click += new System.EventHandler(this.CreateButton_Click);
			// 
			// DescriptionLabel
			// 
			this.DescriptionLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.DescriptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 7, true);
			this.DescriptionLabel.Name = "DescriptionLabel";
			this.DescriptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(338, 31, true);
			this.DescriptionLabel.TabIndex = 3;
			this.DescriptionLabel.Text = "Select the shipment that you want to create an Importer Security Filing job for";
			// 
			// CancelButton
			// 
			this.CancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.CancelButton.IsCaptionOverridden = true;
			this.CancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(259, 45, true);
			this.CancelButton.Name = "CancelButton";
			this.CancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CancelButton.TabIndex = 3;
			this.CancelButton.Text = "Cancel";
			this.CancelButton.ToolTipCaption = null;
			this.CancelButton.Click += new System.EventHandler(this.CancelButton_Click);
			// 
			// ISFFromShipmentCreatorDialog
			// 
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(346, 105, true);
			this.Controls.Add(this.DescriptionLabel);
			this.Controls.Add(this.ShipmentGuidFindBox);
			this.Controls.Add(this.CancelButton);
			this.Controls.Add(this.CreateButton);
			this.DataSourceAssemblyName = "Enterprise.Customs.US.ISF.Business";
			this.DataSourceType = typeof(Enterprise.Customs.US.ISF.Business.ISFFromShipmentCreator);
			this.DataSourceTypeName = "Enterprise.Customs.US.ISF.Business.ISFFromShipmentCreator";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MinimizeBox = false;
			this.Name = "ISFFromShipmentCreatorDialog";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Select Shipment";
			this.Controls.SetChildIndex(this.CreateButton, 0);
			this.Controls.SetChildIndex(this.CancelButton, 0);
			this.Controls.SetChildIndex(this.ShipmentGuidFindBox, 0);
			this.Controls.SetChildIndex(this.DescriptionLabel, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ShipmentGuidFindBox.ResumeLayout(true);
			this.ShipmentGuidFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZGuidFindBox ShipmentGuidFindBox;
		private Enterprise.ZArchitecture.ZLabel DescriptionLabel;
		public new Enterprise.ZArchitecture.GUI.ZButton CancelButton;
		public Enterprise.ZArchitecture.GUI.ZButton CreateButton;
	}
}
