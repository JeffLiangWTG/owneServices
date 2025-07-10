namespace Enterprise.MasterFiles.GUI
{
	partial class OSMGBulkUpdateForm
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
		protected override void InitializeComponent()
		{
			this.OSMGGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.UpdateButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CancelFormButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.OSMGGuidFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 79, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(319, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.OSMGBulkUpdater);
			// 
			// OSMGGuidFindBox
			// 
			this.OSMGGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OSMGGuidFindBox, "BulkOrgSecurityGroup");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OSMGBulkUpdater)(null)).BulkOrgSecurityGroup)));
			this.OSMGGuidFindBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("2f6947e7-cabd-44ed-bd40-e454156f7b48", "OSMG");
			this.OSMGGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(62, 15, true);
			this.OSMGGuidFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.GlbGroup;
			this.OSMGGuidFindBox.Name = "OSMGGuidFindBox";
			this.OSMGGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 17, true);
			this.OSMGGuidFindBox.TabIndex = 1;
			// 
			// UpdateButton
			// 
			this.UpdateButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("c4e1d671-bdc5-4ef3-91e5-d0bd6c79429a", "Update");
			this.UpdateButton.Enabled = false;
			this.UpdateButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(142, 44, true);
			this.UpdateButton.Name = "UpdateButton";
			this.UpdateButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.UpdateButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.UpdateButton.TabIndex = 2;
			this.UpdateButton.ToolTipCaption = null;
			this.UpdateButton.UseVisualStyleBackColor = true;
			this.UpdateButton.Click += new System.EventHandler(this.UpdateButton_Click);
			// 
			// CancelFormButton
			// 
			this.CancelFormButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("e2d50cdc-9a21-4da8-99f6-eb3b75221e67", "Cancel");
			this.CancelFormButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(227, 44, true);
			this.CancelFormButton.Name = "CancelFormButton";
			this.CancelFormButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.CancelFormButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CancelFormButton.TabIndex = 3;
			this.CancelFormButton.ToolTipCaption = null;
			this.CancelFormButton.UseVisualStyleBackColor = true;
			this.CancelFormButton.Click += new System.EventHandler(this.CancelFormButton_Click);
			// 
			// OSMGBulkUpdateForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("65b440d9-662a-435c-b661-df9cbfbd9092", "OSMG Bulk Update");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(319, 103, true);
			this.Controls.Add(this.CancelFormButton);
			this.Controls.Add(this.UpdateButton);
			this.Controls.Add(this.OSMGGuidFindBox);
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.DefaultOSMG);
			this.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(333, 140, true);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(333, 140, true);
			this.Name = "OSMGBulkUpdateForm";
			this.Controls.SetChildIndex(this.OSMGGuidFindBox, 0);
			this.Controls.SetChildIndex(this.UpdateButton, 0);
			this.Controls.SetChildIndex(this.CancelFormButton, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.OSMGGuidFindBox.ResumeLayout(true);
			this.OSMGGuidFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGuidFindBox OSMGGuidFindBox;
		private ZArchitecture.GUI.ZButton UpdateButton;
		private ZArchitecture.GUI.ZButton CancelFormButton;
	}
}
