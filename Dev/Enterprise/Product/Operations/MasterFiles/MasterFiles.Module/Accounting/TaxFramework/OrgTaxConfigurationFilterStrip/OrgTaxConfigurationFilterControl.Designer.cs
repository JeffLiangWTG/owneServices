namespace Enterprise.MasterFiles.Module
{
	partial class OrgTaxConfigurationFilterControl
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
			this.taxConfigurationGuidDropEdit = new Enterprise.ZArchitecture.GUI.ZGuidDropEdit();
			this.statusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.taxConfigurationGuidDropEdit.SuspendLayout();
			this.statusDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Module.OrgTaxConfigurationModuleFilter);
			// 
			// taxConfigurationGuidDropEdit
			// 
			this.taxConfigurationGuidDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.taxConfigurationGuidDropEdit, "TaxConfiguration");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Module.OrgTaxConfigurationModuleFilter)(null)).TaxConfiguration)));
			this.taxConfigurationGuidDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(309, 1, true);
			this.taxConfigurationGuidDropEdit.Name = "taxConfigurationGuidDropEdit";
			this.taxConfigurationGuidDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 15, true);
			this.taxConfigurationGuidDropEdit.TabIndex = 0;
			// 
			// statusDropEdit
			// 
			this.statusDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.statusDropEdit, "ConfigurationStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Module.OrgTaxConfigurationModuleFilter)(null)).ConfigurationStatus)));
			this.statusDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.statusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(309, 25, true);
			this.statusDropEdit.Name = "statusDropEdit";
			this.statusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 15, true);
			this.statusDropEdit.TabIndex = 1;
			// 
			// OrgTaxConfigurationFilterControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.statusDropEdit);
			this.Controls.Add(this.taxConfigurationGuidDropEdit);
			this.Name = "OrgTaxConfigurationFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(641, 50, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.taxConfigurationGuidDropEdit.ResumeLayout(true);
			this.taxConfigurationGuidDropEdit.PerformLayout();
			this.statusDropEdit.ResumeLayout(true);
			this.statusDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGuidDropEdit taxConfigurationGuidDropEdit;
		private ZArchitecture.GUI.ZDropEdit statusDropEdit;
	}
}
