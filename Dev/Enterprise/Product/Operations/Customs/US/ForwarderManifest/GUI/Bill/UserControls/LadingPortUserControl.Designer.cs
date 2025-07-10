namespace Enterprise.Customs.US.ForwarderManifest.GUI
{
	partial class LadingPortUserControl
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
			this.LadingPortScheduleDTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.LadingPortCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.LadingPortScheduleDDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.LadingPortCodeFindBox.SuspendLayout();
			this.LadingPortScheduleDDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.ForwarderManifest.Business.USExportAsycudaBill);
			// 
			// LadingPortScheduleDTextBox
			// 
			this.BindingSource.SetBindingMember(this.LadingPortScheduleDTextBox, "ABL_CustomsLoadPort");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.ForwarderManifest.Business.USExportAsycudaBill)(null)).ABL_CustomsLoadPort)));
			this.LadingPortScheduleDTextBox.CaptionResourceString = Enterprise.Customs.US.ForwarderManifest.GUI.Res.GetData("427D0CFE-5E28-4C90-A196-BFE33CA8D100", "Schedule D");
			this.LadingPortScheduleDTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(150, 0, true);
			this.LadingPortScheduleDTextBox.Name = "LadingPortScheduleDTextBox";
			this.LadingPortScheduleDTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(90, 20, true);
			this.LadingPortScheduleDTextBox.TabIndex = 1;
			// 
			// LadingPortScheduleDDropEdit
			// 
			this.LadingPortScheduleDDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.LadingPortScheduleDDropEdit, "ABL_CustomsLoadPort");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.ForwarderManifest.Business.USExportAsycudaBill)(null)).ABL_CustomsLoadPort)));
			this.LadingPortScheduleDDropEdit.CaptionResourceString = Enterprise.Customs.US.ForwarderManifest.GUI.Res.GetData("427D0CFE-5E28-4C90-A196-BFE33CA8D100", "Schedule D");
			this.LadingPortScheduleDDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(150, 0, true);
			this.LadingPortScheduleDDropEdit.Name = "LadingPortScheduleDDropEdit";
			this.LadingPortScheduleDDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(90, 20, true);
			this.LadingPortScheduleDDropEdit.Visible = false;
			this.LadingPortScheduleDDropEdit.TabIndex = 1;
			// 
			// LadingPortCodeFindBox
			// 
			this.LadingPortCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.LadingPortCodeFindBox, "ABL_RL_NKPortOfLoading");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.ForwarderManifest.Business.USExportAsycudaBill)(null)).ABL_RL_NKPortOfLoading)));
			this.LadingPortCodeFindBox.CaptionResourceString = Enterprise.Customs.US.ForwarderManifest.GUI.Res.GetData("842561EE-CC67-462F-B2AB-BFE33CA8D101", "Port of Lading UNLOCO");
			this.LadingPortCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.LadingPortCodeFindBox.Name = "LadingPortCodeFindBox";
			this.LadingPortCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.LadingPortCodeFindBox.ParentType = null;
			this.LadingPortCodeFindBox.PreBoundMaxLength = 5;
			this.LadingPortCodeFindBox.ShowDescriptionBox = false;
			this.LadingPortCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 20, true);
			this.LadingPortCodeFindBox.TabIndex = 0;
			// 
			// LadingPortUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.LadingPortCodeFindBox);
			this.Controls.Add(this.LadingPortScheduleDTextBox);
			this.Controls.Add(this.LadingPortScheduleDDropEdit);
			this.Name = "LadingPortUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(338, 23, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.LadingPortCodeFindBox.ResumeLayout(true);
			this.LadingPortCodeFindBox.PerformLayout();
			this.LadingPortScheduleDDropEdit.ResumeLayout(true);
			this.LadingPortScheduleDDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal Enterprise.ZArchitecture.GUI.ZCodeFindBox LadingPortCodeFindBox;
		internal Enterprise.ZArchitecture.ZTextBox LadingPortScheduleDTextBox;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit LadingPortScheduleDDropEdit;
	}
}
