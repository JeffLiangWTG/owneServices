using CargoWiseOne.ResourceStrings;

namespace Enterprise.Telematics.Module.Filters
{
	partial class TelPreDriveChecklistTemplateHeaderFilterControl
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
			this.grid.SuspendLayout();
			this.AddStripButton.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// grid
			// 
			this.BindingSource.SetBindingMember(this.grid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Telematics.Business.TelPreDriveChecklistTemplateHeader)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Telematics.Business.TelPreDriveChecklistTemplateHeader)(null)).TTH_Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Telematics.Business.TelPreDriveChecklistTemplateHeader)(null)).TTH_IsActive)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Telematics.Business.TelPreDriveChecklistTemplateHeader)(null)).TTH_SystemCreateUser)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Telematics.Business.TelPreDriveChecklistTemplateHeader)(null)).TTH_SystemCreateTimeUtc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Telematics.Business.TelPreDriveChecklistTemplateHeader)(null)).TTH_SystemLastEditUser)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Telematics.Business.TelPreDriveChecklistTemplateHeader)(null)).TTH_SystemLastEditTimeUtc)));
			zTextBoxColumnStyleInfo1.ColumnName = "TTH_Description";
			zTextBoxColumnStyleInfo1.CaptionResourceString = Res.GetData("C5862444-FB53-447D-9A88-9CF03C2B87C3", "Description", "Description", "");
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo1.IsVisible = true;
			zCheckBoxColumnStyleInfo1.ColumnName = "TTH_IsActive";
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Res.GetData("0F4DBD8B-1EB0-4204-82C9-9CF3ABB5AD0E", "Is Active", "Is Active", "");
			zCheckBoxColumnStyleInfo1.IsReadOnly = true;
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCheckBoxColumnStyleInfo1.IsVisible = true;
			zTextBoxColumnStyleInfo2.ColumnName = "TTH_SystemCreateUser";
			zTextBoxColumnStyleInfo2.CaptionResourceString = Res.GetData("56E7B9C4-E2CF-4EDD-972C-22FA22B4CCA7", "Creating User", "Creating User", "");
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			zTextBoxColumnStyleInfo2.IsVisible = true;
			zDateEditColumnStyleInfo1.ColumnName = "TTH_SystemCreateTimeUtc";
			zDateEditColumnStyleInfo1.CaptionResourceString = Res.GetData("33EA09CB-0DE4-4955-8360-6A7ADC4642ED", "Created Time", "Created Time", "");
			zDateEditColumnStyleInfo1.IsReadOnly = true;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zDateEditColumnStyleInfo1.IsVisible = true;
			zTextBoxColumnStyleInfo3.ColumnName = "TTH_SystemLastEditUser";
			zTextBoxColumnStyleInfo3.CaptionResourceString = Res.GetData("77DAF037-84A3-41EA-983F-95C816C96B3A", "Last Edit User", "Last Edit User", "");
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo3.IsVisible = true;
			zDateEditColumnStyleInfo2.ColumnName = "TTH_SystemLastEditTimeUtc";
			zDateEditColumnStyleInfo2.CaptionResourceString = Res.GetData("204AA738-6BCB-445E-8F23-3001B64651F8", "Last Edit Time", "Last Edit Time", "");
			zDateEditColumnStyleInfo2.IsReadOnly = true;
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zDateEditColumnStyleInfo2.IsVisible = true;
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(811, 150, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Telematics.Business.TelPreDriveChecklistTemplateHeader);
			// 
			// GlbDeviceFilterControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Name = "TelPreDriveChecklistTemplateHeaderFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(811, 302, true);
			((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
			this.grid.ResumeLayout(false);
			this.grid.PerformLayout();
			this.AddStripButton.ResumeLayout(true);
			this.AddStripButton.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
	}
}
