namespace Enterprise.Customs.Module
{
	partial class ReferenceFilterControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		//private System.ComponentModel.IContainer components = null;

		///// <summary> 
		///// Clean up any resources being used.
		///// </summary>
		///// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		//protected override void Dispose(bool disposing)
		//{
		//    if (disposing && (components != null))
		//    {
		//        components.Dispose();
		//    }
		//    base.Dispose(disposing);
		//}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.RefNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.RefTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.RefTypeDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Module.ReferenceModuleFilter);
			// 
			// RefNoTextBox
			// 
			this.BindingSource.SetBindingMember(this.RefNoTextBox, "Property");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Module.ReferenceModuleFilter)(null)).Property)));
			this.RefNoTextBox.CaptionResourceString = null;
			this.RefNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(308, 1, true);
			this.RefNoTextBox.Name = "RefNoTextBox";
			this.RefNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(94, 20, true);
			this.RefNoTextBox.TabIndex = 2;
			// 
			// RefTypeDropEdit
			// 
			this.RefTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RefTypeDropEdit, "ReferenceType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Module.ReferenceModuleFilter)(null)).ReferenceType)));
			this.RefTypeDropEdit.CaptionResourceString = Enterprise.Customs.Module.Res.GetData("ReferenceFilterControl|e2cfd0e3-ef42-4021-ab5e-67c945fb7cbd", "Ref. Type", "Ref. Type", "");
			this.RefTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(481, 1, true);
			this.RefTypeDropEdit.Name = "RefTypeDropEdit";
			this.RefTypeDropEdit.PreBoundMaxLength = 3;
			this.RefTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(117, 20, true);
			this.RefTypeDropEdit.TabIndex = 3;
			// 
			// ReferenceFilterControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.RefTypeDropEdit);
			this.Controls.Add(this.RefNoTextBox);
			this.Name = "ReferenceFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(609, 21, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.RefTypeDropEdit.ResumeLayout(true);
			this.RefTypeDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal Enterprise.ZArchitecture.ZTextBox RefNoTextBox;
		private Enterprise.ZArchitecture.GUI.ZDropEdit RefTypeDropEdit;
	}
}
