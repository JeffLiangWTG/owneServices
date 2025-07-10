namespace Enterprise.Customs.DataRegistry.GUI
{
	partial class AutoBillingGroupNotificationUserControl
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.SendGroupGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.SuspendUnpostARNotificationCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.NotificationOptionsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SendGroupGuidFindBox.SuspendLayout();
			this.NotificationOptionsGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.DataRegistry.Business.AutoBillingGroupNotification);
			// 
			// SendGroupGuidFindBox
			// 
			this.SendGroupGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SendGroupGuidFindBox, "SendGroupPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.DataRegistry.Business.AutoBillingGroupNotification)(null)).SendGroupPK)));
			this.SendGroupGuidFindBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("7589f3bf-b1e7-4da0-819f-24f4a64a418b", "Send Group");
			this.SendGroupGuidFindBox.IsPrimaryKeyFromCodeRequired = false;
			this.SendGroupGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(69, 23, true);
			this.SendGroupGuidFindBox.Name = "SendGroupGuidFindBox";
			this.SendGroupGuidFindBox.ShouldResize = true;
			this.SendGroupGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(356, 17, true);
			this.SendGroupGuidFindBox.TabIndex = 0;
			// 
			// SuspendUnpostARNotificationCheckBox
			// 
			this.SuspendUnpostARNotificationCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.SuspendUnpostARNotificationCheckBox, "SuppressUnpostARNotificaiton");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.DataRegistry.Business.AutoBillingGroupNotification)(null)).SuppressUnpostARNotificaiton)));
			this.SuspendUnpostARNotificationCheckBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("1305f7aa-2f13-4f12-948b-9ec1accb9733", "Suppress unposted AR notification when AP posted");
			this.SuspendUnpostARNotificationCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.SuspendUnpostARNotificationCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.SuspendUnpostARNotificationCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(411, 54, true);
			this.SuspendUnpostARNotificationCheckBox.Name = "SuspendUnpostARNotificationCheckBox";
			this.SuspendUnpostARNotificationCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.SuspendUnpostARNotificationCheckBox.TabIndex = 1;
			this.SuspendUnpostARNotificationCheckBox.UseVisualStyleBackColor = true;
			// 
			// NotificationOptionsGroupBox
			// 
			this.NotificationOptionsGroupBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("94947fc2-7d34-4acf-86b7-3df843ca3a8b", "Notification Options");
			this.NotificationOptionsGroupBox.Controls.Add(this.SendGroupGuidFindBox);
			this.NotificationOptionsGroupBox.Controls.Add(this.SuspendUnpostARNotificationCheckBox);
			this.NotificationOptionsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.NotificationOptionsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.NotificationOptionsGroupBox.Name = "NotificationOptionsGroupBox";
			this.NotificationOptionsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(431, 80, true);
			this.NotificationOptionsGroupBox.TabIndex = 2;
			this.NotificationOptionsGroupBox.TabStop = false;
			// 
			// AutoBillingGroupNotificationUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.NotificationOptionsGroupBox);
			this.Name = "AutoBillingGroupNotificationUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(431, 80, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SendGroupGuidFindBox.ResumeLayout(true);
			this.SendGroupGuidFindBox.PerformLayout();
			this.NotificationOptionsGroupBox.ResumeLayout(false);
			this.NotificationOptionsGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZGuidFindBox SendGroupGuidFindBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox SuspendUnpostARNotificationCheckBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox NotificationOptionsGroupBox;
	}
}
