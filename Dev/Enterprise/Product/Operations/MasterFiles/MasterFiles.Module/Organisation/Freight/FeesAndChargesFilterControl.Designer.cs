namespace Enterprise.MasterFiles.Module
{
	partial class FeesAndChargesFilterControl
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
			this.ServiceLevelDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ServiceTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ServiceLevelDropEdit.SuspendLayout();
			this.ServiceTypeDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Module.FeesAndChargesFilter);
			// 
			// ServiceLevelDropEdit
			// 
			this.ServiceLevelDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ServiceLevelDropEdit, "ServiceLevel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Module.FeesAndChargesFilter)(null)).ServiceLevel)));
			this.ServiceLevelDropEdit.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("FeesAndChargesFilterControl|afbd0d43-6ddf-4d30-b3e5-7129998f60b0", "Level");
			this.ServiceLevelDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(468, 1, true);
			this.ServiceLevelDropEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(6, true);
			this.ServiceLevelDropEdit.Name = "ServiceLevelDropEdit";
			this.ServiceLevelDropEdit.PreBoundMaxLength = 2;
			this.ServiceLevelDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 29, true);
			this.ServiceLevelDropEdit.TabIndex = 0;
			// 
			// ServiceTypeDropEdit
			// 
			this.ServiceTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ServiceTypeDropEdit, "ServiceType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Module.FeesAndChargesFilter)(null)).ServiceType)));
			this.ServiceTypeDropEdit.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("FeesAndChargesFilterControl|be1ee9b0-31c5-43ea-8e39-429249d7e6f4", "Type");
			this.ServiceTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(257, 1, true);
			this.ServiceTypeDropEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(6, true);
			this.ServiceTypeDropEdit.Name = "ServiceTypeDropEdit";
			this.ServiceTypeDropEdit.PreBoundMaxLength = 2;
			this.ServiceTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(172, 29, true);
			this.ServiceTypeDropEdit.TabIndex = 1;
			// 
			// FeesAndChargesFilterControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.ServiceLevelDropEdit);
			this.Controls.Add(this.ServiceTypeDropEdit);
			this.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(6, true);
			this.Name = "FeesAndChargesFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1100, 44, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ServiceLevelDropEdit.ResumeLayout(true);
			this.ServiceLevelDropEdit.PerformLayout();
			this.ServiceTypeDropEdit.ResumeLayout(true);
			this.ServiceTypeDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZDropEdit ServiceLevelDropEdit;
		private ZArchitecture.GUI.ZDropEdit ServiceTypeDropEdit;
	}
}
