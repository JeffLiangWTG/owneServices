namespace Enterprise.MarketingManager.GUI
{
	partial class DeliveryDetailsPopupForm
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
			this.bottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.postingButtonsUserControl = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			this.DeliveryDetailsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.DeliveryDetailsLabel = new Enterprise.ZArchitecture.ZLabel();
			this.DeliveryStatusTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ContactEmailTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ContactNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DeliveryDetailsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.bottomPanel.SuspendLayout();
			this.postingButtonsUserControl.SuspendLayout();
			this.DeliveryDetailsPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 417, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(634, 24, true);
			this.MainStatusBar.Visible = false;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MarketingManager.Business.EmailDeliveryDetailsProvider);
			// 
			// bottomPanel
			// 
			this.bottomPanel.Controls.Add(this.postingButtonsUserControl);
			this.bottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.bottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 389, true);
			this.bottomPanel.Name = "bottomPanel";
			this.bottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(634, 28, true);
			this.bottomPanel.TabIndex = 1;
			// 
			// postingButtonsUserControl
			// 
			this.postingButtonsUserControl.AllowDrop = true;
			this.postingButtonsUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.postingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(390, 1, true);
			this.postingButtonsUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.postingButtonsUserControl.Name = "postingButtonsUserControl";
			this.postingButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.postingButtonsUserControl.TabIndex = 0;
			// 
			// DeliveryDetailsPanel
			// 
			this.DeliveryDetailsPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.DeliveryDetailsPanel.Controls.Add(this.DeliveryDetailsLabel);
			this.DeliveryDetailsPanel.Controls.Add(this.DeliveryStatusTextBox);
			this.DeliveryDetailsPanel.Controls.Add(this.ContactEmailTextBox);
			this.DeliveryDetailsPanel.Controls.Add(this.ContactNameTextBox);
			this.DeliveryDetailsPanel.Controls.Add(this.DeliveryDetailsTextBox);
			this.DeliveryDetailsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DeliveryDetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DeliveryDetailsPanel.Name = "DeliveryDetailsPanel";
			this.DeliveryDetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(634, 389, true);
			this.DeliveryDetailsPanel.TabIndex = 2;
			// 
			// DeliveryDetailsLabel
			// 
			this.DeliveryDetailsLabel.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("a2d670f5-7b3b-4e49-870c-47276834ff5b", "Delivery Details:");
			this.DeliveryDetailsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 60, true);
			this.DeliveryDetailsLabel.Name = "DeliveryDetailsLabel";
			this.DeliveryDetailsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 23, true);
			this.DeliveryDetailsLabel.TabIndex = 7;
			// 
			// DeliveryStatusTextBox
			// 
			this.BindingSource.SetBindingMember(this.DeliveryStatusTextBox, "TrackingStatusDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.EmailDeliveryDetailsProvider)(null)).TrackingStatusDescription)));
			this.DeliveryStatusTextBox.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("062b630f-baf4-463f-8cd1-bd0a0f4c4b84", "Delivery Status");
			this.DeliveryStatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 37, true);
			this.DeliveryStatusTextBox.Name = "DeliveryStatusTextBox";
			this.DeliveryStatusTextBox.ReadOnly = true;
			this.DeliveryStatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(182, 20, true);
			this.DeliveryStatusTextBox.TabIndex = 5;
			// 
			// ContactEmailTextBox
			// 
			this.BindingSource.SetBindingMember(this.ContactEmailTextBox, "EmailAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.EmailDeliveryDetailsProvider)(null)).EmailAddress)));
			this.ContactEmailTextBox.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("4598f7f5-ce76-45ad-813a-ca1fd1cf54aa", "Contact Email");
			this.ContactEmailTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(371, 11, true);
			this.ContactEmailTextBox.Name = "ContactEmailTextBox";
			this.ContactEmailTextBox.ReadOnly = true;
			this.ContactEmailTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(257, 20, true);
			this.ContactEmailTextBox.TabIndex = 4;
			// 
			// ContactNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.ContactNameTextBox, "ContactName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.EmailDeliveryDetailsProvider)(null)).ContactName)));
			this.ContactNameTextBox.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("70468434-26dd-42fb-9a78-b71b5d1e6481", "Contact Name");
			this.ContactNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 11, true);
			this.ContactNameTextBox.Name = "ContactNameTextBox";
			this.ContactNameTextBox.ReadOnly = true;
			this.ContactNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(182, 20, true);
			this.ContactNameTextBox.TabIndex = 3;
			// 
			// DeliveryDetailsTextBox
			// 
			this.DeliveryDetailsTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.DeliveryDetailsTextBox, "BounceBackEmail");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.EmailDeliveryDetailsProvider)(null)).BounceBackEmail)));
			this.DeliveryDetailsTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.DeliveryDetailsTextBox, false);
			this.DeliveryDetailsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 86, true);
			this.DeliveryDetailsTextBox.Multiline = true;
			this.DeliveryDetailsTextBox.Name = "DeliveryDetailsTextBox";
			this.DeliveryDetailsTextBox.ReadOnly = true;
			this.DeliveryDetailsTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.DeliveryDetailsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(625, 301, true);
			this.DeliveryDetailsTextBox.TabIndex = 6;
			// 
			// DeliveryDetailsPopupForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(634, 441, true);
			this.Controls.Add(this.DeliveryDetailsPanel);
			this.Controls.Add(this.bottomPanel);
			this.DataSourceAssemblyName = "Enterprise.MarketingManager.Business";
			this.DataSourceType = typeof(Enterprise.MarketingManager.Business.EmailDeliveryDetailsProvider);
			this.DataSourceTypeName = "Enterprise.MarketingManager.Business.EmailDeliveryDetailsProvider";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MinimizeBox = false;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 350, true);
			this.Name = "DeliveryDetailsPopupForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.bottomPanel, 0);
			this.Controls.SetChildIndex(this.DeliveryDetailsPanel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.bottomPanel.ResumeLayout(false);
			this.bottomPanel.PerformLayout();
			this.postingButtonsUserControl.ResumeLayout(true);
			this.postingButtonsUserControl.PerformLayout();
			this.DeliveryDetailsPanel.ResumeLayout(false);
			this.DeliveryDetailsPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZPanel bottomPanel;
		private ZArchitecture.GUI.ZPanel DeliveryDetailsPanel;
		private Enterprise.ZArchitecture.ZTextBox DeliveryDetailsTextBox;
		private Core.Forms.ZPostingButtonsUserControl postingButtonsUserControl;
		private ZArchitecture.ZTextBox DeliveryStatusTextBox;
		private ZArchitecture.ZTextBox ContactEmailTextBox;
		private ZArchitecture.ZTextBox ContactNameTextBox;
		private ZArchitecture.ZLabel DeliveryDetailsLabel;
	}
}