namespace Enterprise.MasterFiles.GUI
{
	partial class AddressSourceAndJobNumberFilterControl
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
			this.AddressSourceDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.JobNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.AddressSourceDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.GUI.AddressSourceAndJobNumberModuleFilter);
			// 
			// AddressSourceDropEdit
			// 
			this.AddressSourceDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AddressSourceDropEdit, "AddressSourceCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.GUI.AddressSourceAndJobNumberModuleFilter)(null)).AddressSourceCode)));
			this.AddressSourceDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("0325451e-9db3-4da2-b0d6-39ad73d2fd7a", "Address Source");
			this.AddressSourceDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(396, 0, true);
			this.AddressSourceDropEdit.Name = "AddressSourceDropEdit";
			this.AddressSourceDropEdit.PreBoundMaxLength = 3;
			this.AddressSourceDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.AddressSourceDropEdit.TabIndex = 3;
			// 
			// JobNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.JobNumberTextBox, "JobNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.GUI.AddressSourceAndJobNumberModuleFilter)(null)).JobNumber)));
			this.JobNumberTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("51ef3568-d97e-4c9f-9759-8cb0a045e9f7", "Job Number");
			this.JobNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(396, 23, true);
			this.JobNumberTextBox.Name = "JobNumberTextBox";
			this.JobNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.JobNumberTextBox.TabIndex = 4;
			// 
			// AddressSourceAndJobNumberFilterControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.JobNumberTextBox);
			this.Controls.Add(this.AddressSourceDropEdit);
			this.Name = "AddressSourceAndJobNumberFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(596, 43, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.AddressSourceDropEdit.ResumeLayout(true);
			this.AddressSourceDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZDropEdit AddressSourceDropEdit;
		private Enterprise.ZArchitecture.ZTextBox JobNumberTextBox;
	}
}
