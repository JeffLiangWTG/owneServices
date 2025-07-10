using System.Windows.Forms;

namespace Enterprise.MasterFiles.GUI;

partial class ValidationToolTabPageUserControl
{
	internal ValidationToolUserControl ValidationToolUserControl;

	void InitializeComponent()
	{
			this.ValidationToolUserControl = new Enterprise.MasterFiles.GUI.ValidationToolUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ValidationToolUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.IWorkflowProvider);
			// 
			// validationToolUserControl
			// 
			this.ValidationToolUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ValidationToolUserControl, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.IValidationToolParent)(((Enterprise.MasterFiles.Business.IWorkflowProvider)(null)))));
			this.ValidationToolUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ValidationToolUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ValidationToolUserControl.Name = "ValidationToolUserControl";
			this.ValidationToolUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(644, 343, true);
			this.ValidationToolUserControl.TabIndex = 0;
			// 
			// ValidationToolTabPageUserControl
			// 
			this.Controls.Add(this.ValidationToolUserControl);
			this.Name = "ValidationToolTabPageUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(644, 343, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ValidationToolUserControl.ResumeLayout(true);
			this.ValidationToolUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

	}
}
