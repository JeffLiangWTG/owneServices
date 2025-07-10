namespace Enterprise.MasterFiles.GUI
{
	partial class DefaultOSMGControl
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
			this.OSMGGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.BulkUpdateButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.DefaultGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.OSMGLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ActionGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.BulkUpdateInfoLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.OSMGGuidFindBox.SuspendLayout();
			this.DefaultGroupBox.SuspendLayout();
			this.ActionGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.DefaultOSMG);
			// 
			// OSMGGuidFindBox
			// 
			this.OSMGGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OSMGGuidFindBox, "OrgSecurityGroup");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.DefaultOSMG)(null)).OrgSecurityGroup)));
			this.OSMGGuidFindBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("dff1a08b-24cb-4b9e-ac60-f45b776959c5", "Find OSMG");
			this.OSMGGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(55, 18, true);
			this.OSMGGuidFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.GlbGroup;
			this.OSMGGuidFindBox.Name = "OSMGGuidFindBox";
			this.OSMGGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.OSMGGuidFindBox.TabIndex = 0;
			// 
			// BulkUpdateButton
			// 
			this.BulkUpdateButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("cdf08eb4-b2c2-4e16-8c6e-d363a0d74dfd", "Bulk Update");
			this.BulkUpdateButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 52, true);
			this.BulkUpdateButton.Name = "BulkUpdateButton";
			this.BulkUpdateButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.BulkUpdateButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.BulkUpdateButton.TabIndex = 1;
			this.BulkUpdateButton.ToolTipCaption = null;
			this.BulkUpdateButton.UseVisualStyleBackColor = true;
			this.BulkUpdateButton.Click += new System.EventHandler(this.BulkUpdateButton_Click);
			// 
			// DefaultGroupBox
			// 
			this.DefaultGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("de819575-24f2-4060-99e2-4682aca82093", "Default");
			this.DefaultGroupBox.Controls.Add(this.OSMGLabel);
			this.DefaultGroupBox.Controls.Add(this.OSMGGuidFindBox);
			this.DefaultGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.DefaultGroupBox.Name = "DefaultGroupBox";
			this.DefaultGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(312, 56, true);
			this.DefaultGroupBox.TabIndex = 2;
			this.DefaultGroupBox.TabStop = false;
			// 
			// OSMGLabel
			// 
			this.OSMGLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("6c892267-fdb6-49ab-803e-af49e1b996bd", "OSMG");
			this.OSMGLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.OSMGLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 15, true);
			this.OSMGLabel.Name = "OSMGLabel";
			this.OSMGLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(42, 23, true);
			this.OSMGLabel.TabIndex = 1;
			// 
			// ActionGroupBox
			// 
			this.ActionGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("a8cca280-1395-4910-8b0c-3b593aa1f581", "Action");
			this.ActionGroupBox.Controls.Add(this.BulkUpdateInfoLabel);
			this.ActionGroupBox.Controls.Add(this.BulkUpdateButton);
			this.ActionGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 66, true);
			this.ActionGroupBox.Name = "ActionGroupBox";
			this.ActionGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(311, 88, true);
			this.ActionGroupBox.TabIndex = 3;
			this.ActionGroupBox.TabStop = false;
			// 
			// BulkUpdateInfoLabel
			// 
			this.BulkUpdateInfoLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("08f256c3-5dcb-4a66-8cdc-71c6e2c2b43c", "Attach all Unrestricted Organizations to an Organization Security Management Group.");
			this.BulkUpdateInfoLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.BulkUpdateInfoLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 20, true);
			this.BulkUpdateInfoLabel.Name = "BulkUpdateInfoLabel";
			this.BulkUpdateInfoLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(287, 29, true);
			this.BulkUpdateInfoLabel.TabIndex = 0;
			// 
			// DefaultOSMGControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ActionGroupBox);
			this.Controls.Add(this.DefaultGroupBox);
			this.Name = "DefaultOSMGControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(324, 161, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.OSMGGuidFindBox.ResumeLayout(true);
			this.OSMGGuidFindBox.PerformLayout();
			this.DefaultGroupBox.ResumeLayout(false);
			this.DefaultGroupBox.PerformLayout();
			this.ActionGroupBox.ResumeLayout(false);
			this.ActionGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZGuidFindBox OSMGGuidFindBox;
		internal ZArchitecture.GUI.ZButton BulkUpdateButton;
		private ZArchitecture.GUI.ZGroupBox DefaultGroupBox;
		private ZArchitecture.ZLabel OSMGLabel;
		private ZArchitecture.GUI.ZGroupBox ActionGroupBox;
		private ZArchitecture.ZLabel BulkUpdateInfoLabel;
	}
}
