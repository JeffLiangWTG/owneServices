namespace Enterprise.MasterFiles.GUI
{
	partial class DistanceCalculationProviderConfigurationRegistryControl
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
			this.ProviderDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.VersionDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.MethodDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.DistanceCalculationProviderConfiguration);
			// 
			// ProviderDropEdit
			// 
			this.BindingSource.SetBindingMember(this.ProviderDropEdit, "Provider");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.DistanceCalculationProviderConfiguration)(null)).Provider)));
			this.ProviderDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("DistanceCalculationProviderConfigurationRegistryControl|cf3b56f7-fd31-464e-ac67-523231c2803d", "Provider");
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.ProviderDropEdit, CargoWise.Windows.UI.LabelCaptionAlignment.Auto);
			this.ProviderDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(64, 19, true);
			this.ProviderDropEdit.Name = "ProviderDropEdit";
			this.ProviderDropEdit.PreBoundMaxLength = 3;
			this.ProviderDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(368, 20, true);
			this.ProviderDropEdit.TabIndex = 0;
			// 
			// VersionDropEdit
			// 
			this.BindingSource.SetBindingMember(this.VersionDropEdit, "Version");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.DistanceCalculationProviderConfiguration)(null)).Version)));
			this.VersionDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("DistanceCalculationProviderConfigurationRegistryControl|11853ea2-129a-40df-963b-399e3c21c0a9", "Version");
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.VersionDropEdit, CargoWise.Windows.UI.LabelCaptionAlignment.Auto);
			this.VersionDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(64, 60, true);
			this.VersionDropEdit.Name = "VersionDropEdit";
			this.VersionDropEdit.PreBoundMaxLength = 3;
			this.VersionDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(205, 20, true);
			this.VersionDropEdit.TabIndex = 1;
			// 
			// MethodDropEdit
			// 
			this.BindingSource.SetBindingMember(this.MethodDropEdit, "CalculationMethod");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.DistanceCalculationProviderConfiguration)(null)).CalculationMethod)));
			this.MethodDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("DistanceCalculationProviderConfigurationRegistryControl|8cb7e662-8a01-4786-8e08-fceb68e09116", "Method");
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.MethodDropEdit, CargoWise.Windows.UI.LabelCaptionAlignment.Auto);
			this.MethodDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(64, 101, true);
			this.MethodDropEdit.Name = "MethodDropEdit";
			this.MethodDropEdit.PreBoundMaxLength = 3;
			this.MethodDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(205, 20, true);
			this.MethodDropEdit.TabIndex = 2;
			// 
			// DistanceCalculationProviderConfigurationRegistryControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.MethodDropEdit);
			this.Controls.Add(this.VersionDropEdit);
			this.Controls.Add(this.ProviderDropEdit);
			this.Name = "DistanceCalculationProviderConfigurationRegistryControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(439, 140, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZDropEdit ProviderDropEdit;
		private Enterprise.ZArchitecture.GUI.ZDropEdit VersionDropEdit;
		private Enterprise.ZArchitecture.GUI.ZDropEdit MethodDropEdit;
	}
}
