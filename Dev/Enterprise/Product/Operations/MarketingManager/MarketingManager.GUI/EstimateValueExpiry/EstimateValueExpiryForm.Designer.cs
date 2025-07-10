namespace Enterprise.MarketingManager.GUI
{
	partial class EstimateValueExpiryForm
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
			this.SetExpiryButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ExpiryDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ExpiryReasonDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ExpiryDateEdit.SuspendLayout();
			this.ExpiryReasonDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 85, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(274, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MarketingManager.GUI.EstimateValueExpiryAction);
			// 
			// SetExpiryButton
			// 
			this.SetExpiryButton.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("615efc10-bae4-44e1-b842-75fbd2e358cf", "Set Expiry");
			this.SetExpiryButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(171, 59, true);
			this.SetExpiryButton.Name = "SetExpiryButton";
			this.SetExpiryButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.SetExpiryButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(89, 22, true);
			this.SetExpiryButton.TabIndex = 3;
			this.SetExpiryButton.ToolTipCaption = null;
			this.SetExpiryButton.Click += SetExpiryButton_Click;
			// 
			// ExpiryDateEdit
			// 
			this.ExpiryDateEdit.AllowDrop = true;
			this.ExpiryDateEdit.AutoCompleteMonthThreshold = 1;
			this.ExpiryDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ExpiryDateEdit, "ExpiryDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MarketingManager.GUI.EstimateValueExpiryAction)(null)).ExpiryDate)));
			this.ExpiryDateEdit.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("6e3073ca-65e5-4139-9a9b-ae0a0fa4742d", "Expire From");
			this.ExpiryDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(73, 10, true);
			this.ExpiryDateEdit.Name = "ExpiryDateEdit";
			this.ExpiryDateEdit.TabIndex = 1;
			// 
			// ExpiryReasonDropEdit
			// 
			this.ExpiryReasonDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ExpiryReasonDropEdit, "ExpiryReason");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MarketingManager.GUI.EstimateValueExpiryAction)(null)).ExpiryReason)));
			this.ExpiryReasonDropEdit.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("c6613ffd-8dce-4ec7-ae78-f1681b5b3d01", "Reason");
			this.ExpiryReasonDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(73, 35, true);
			this.ExpiryReasonDropEdit.Name = "ExpiryReasonDropEdit";
			this.ExpiryReasonDropEdit.PreBoundMaxLength = 3;
			this.ExpiryReasonDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(187, 17, true);
			this.ExpiryReasonDropEdit.TabIndex = 2;
			// 
			// EstimateValueExpiryForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("3b8722a1-13d1-42ba-b626-6dbf7010634c", "Set Estimated Value Expiry");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(274, 109, true);
			this.Controls.Add(this.ExpiryReasonDropEdit);
			this.Controls.Add(this.ExpiryDateEdit);
			this.Controls.Add(this.SetExpiryButton);
			this.DataSourceType = typeof(Enterprise.MarketingManager.GUI.EstimateValueExpiryAction);
			this.Name = "EstimateValueExpiryForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.SetExpiryButton, 0);
			this.Controls.SetChildIndex(this.ExpiryDateEdit, 0);
			this.Controls.SetChildIndex(this.ExpiryReasonDropEdit, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ExpiryDateEdit.ResumeLayout(true);
			this.ExpiryDateEdit.PerformLayout();
			this.ExpiryReasonDropEdit.ResumeLayout(true);
			this.ExpiryReasonDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZButton SetExpiryButton;
		private Enterprise.ZArchitecture.GUI.ZDateEdit ExpiryDateEdit;
		private Enterprise.ZArchitecture.GUI.ZDropEdit ExpiryReasonDropEdit;
	}
}