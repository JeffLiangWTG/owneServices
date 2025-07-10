namespace Enterprise.MasterFiles.Module
{
	partial class UtcOffsetFilterControl
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
			this.UtcOffsetToDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.UtcOffsetFromDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.UtcOffsetToDropEdit.SuspendLayout();
			this.UtcOffsetFromDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Module.UtcOffsetFilter);
			// 
			// UtcOffsetToDropEdit
			// 
			this.UtcOffsetToDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.UtcOffsetToDropEdit, "UtcOffsetTo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Module.UtcOffsetFilter)(null)).UtcOffsetTo)));
			this.UtcOffsetToDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(404, 1, true);
			this.UtcOffsetToDropEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.UtcOffsetToDropEdit.Name = "UtcOffsetToDropEdit";
			this.UtcOffsetToDropEdit.PreBoundMaxLength = 3;
			this.UtcOffsetToDropEdit.ShouldResizeByMaxLength = true;
			this.UtcOffsetToDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 17, true);
			this.UtcOffsetToDropEdit.TabIndex = 0;
			// 
			// UtcOffsetFromDropEdit
			// 
			this.UtcOffsetFromDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.UtcOffsetFromDropEdit, "UtcOffsetFrom");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Module.UtcOffsetFilter)(null)).UtcOffsetFrom)));
			this.UtcOffsetFromDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(218, 1, true);
			this.UtcOffsetFromDropEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.UtcOffsetFromDropEdit.Name = "UtcOffsetFromDropEdit";
			this.UtcOffsetFromDropEdit.PreBoundMaxLength = 3;
			this.UtcOffsetFromDropEdit.ShouldResizeByMaxLength = true;
			this.UtcOffsetFromDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 17, true);
			this.UtcOffsetFromDropEdit.TabIndex = 1;
			// 
			// UtcOffsetFilterControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.UtcOffsetToDropEdit);
			this.Controls.Add(this.UtcOffsetFromDropEdit);
			this.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.Name = "UtcOffsetFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(626, 26, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.UtcOffsetToDropEdit.ResumeLayout(true);
			this.UtcOffsetToDropEdit.PerformLayout();
			this.UtcOffsetFromDropEdit.ResumeLayout(true);
			this.UtcOffsetFromDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZDropEdit UtcOffsetToDropEdit;
		private ZArchitecture.GUI.ZDropEdit UtcOffsetFromDropEdit;
	}
}
