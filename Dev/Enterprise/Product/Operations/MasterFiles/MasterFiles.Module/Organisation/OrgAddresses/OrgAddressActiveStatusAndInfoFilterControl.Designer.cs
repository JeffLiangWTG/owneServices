namespace Enterprise.MasterFiles.Module
{
	partial class OrgAddressActiveStatusAndInfoFilterControl
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
			this.ActiveStatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.AddressTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ActiveStatusDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Module.OrgAddressWithActiveStatusModuleTextFilter);
			// 
			// AddressTextBox
			// 
			this.BindingSource.SetBindingMember(this.AddressTextBox, "Property");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Module.OrgAddressWithActiveStatusModuleTextFilter)(null)).Property)));
			this.AddressTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(308, 0, true);
			this.AddressTextBox.Name = "AddressTextBox";
			this.AddressTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 17, true);
			this.AddressTextBox.TabIndex = 3;
			// 
			// ActiveStatusDropEdit
			// 
			this.ActiveStatusDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ActiveStatusDropEdit, "ActiveStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Module.OrgAddressWithActiveStatusModuleTextFilter)(null)).ActiveStatus)));
			this.ActiveStatusDropEdit.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("90497268-a0d9-429b-99ac-a5fa90346e30", "Address Status");
			this.ActiveStatusDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ActiveStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(308, 23, true);
			this.ActiveStatusDropEdit.Name = "ActiveStatusDropEdit";
			this.ActiveStatusDropEdit.PreBoundMaxLength = 3;
			this.ActiveStatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 17, true);
			this.ActiveStatusDropEdit.TabIndex = 4;
			// 
			// OrgAddressActiveStatusAndInfoFilterControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.AddressTextBox);
			this.Controls.Add(this.ActiveStatusDropEdit);
			this.Name = "OrgAddressActiveStatusAndInfoFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(609, 43, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ActiveStatusDropEdit.ResumeLayout(true);
			this.ActiveStatusDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZDropEdit ActiveStatusDropEdit;
		private Enterprise.ZArchitecture.ZTextBox AddressTextBox;
	}
}
