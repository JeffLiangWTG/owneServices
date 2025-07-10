namespace Enterprise.Customs.US.ForwarderManifest.GUI
{
	partial class OriginPortUserControl
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
			this.OriginScheduleDTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.OriginCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.OriginScheduleDDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.OriginCodeFindBox.SuspendLayout();
			this.OriginScheduleDDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.ForwarderManifest.Business.USExportAsycudaBill);
			// 
			// OriginScheduleDTextBox
			// 
			this.BindingSource.SetBindingMember(this.OriginScheduleDTextBox, "ABL_CustomsOriginPort");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.ForwarderManifest.Business.USExportAsycudaBill)(null)).ABL_CustomsOriginPort)));
			this.OriginScheduleDTextBox.CaptionResourceString = Enterprise.Customs.US.ForwarderManifest.GUI.Res.GetData("842561CC-CC67-462F-B2AB-03CC101BF417", "Schedule D");
			this.OriginScheduleDTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(150, 0, true);
			this.OriginScheduleDTextBox.Name = "OriginScheduleDTextBox";
			this.OriginScheduleDTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(90, 20, true);
			this.OriginScheduleDTextBox.TabIndex = 1;
			// 
			// OriginScheduleDDropEdit
			// 
			this.OriginScheduleDDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OriginScheduleDDropEdit, "ABL_CustomsOriginPort");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.ForwarderManifest.Business.USExportAsycudaBill)(null)).ABL_CustomsOriginPort)));
			this.OriginScheduleDDropEdit.CaptionResourceString = Enterprise.Customs.US.ForwarderManifest.GUI.Res.GetData("842561CC-CC67-462F-B2AB-03CC101BF417", "Schedule D");
			this.OriginScheduleDDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(150, 0, true);
			this.OriginScheduleDDropEdit.Name = "OriginScheduleDDropEdit";
			this.OriginScheduleDDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(90, 20, true);
			this.OriginScheduleDDropEdit.Visible = false;
			this.OriginScheduleDDropEdit.TabIndex = 1;
			// 
			// OriginCodeFindBox
			// 
			this.OriginCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OriginCodeFindBox, "ABL_RL_NKOrigin");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.ForwarderManifest.Business.USExportAsycudaBill)(null)).ABL_RL_NKOrigin)));
			this.OriginCodeFindBox.CaptionResourceString = Enterprise.Customs.US.ForwarderManifest.GUI.Res.GetData("842561CC-CC67-462F-B2AB-BFE33CA8D336", "Origin UNLOCO");
			this.OriginCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.OriginCodeFindBox.Name = "OriginCodeFindBox";
			this.OriginCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.OriginCodeFindBox.ParentType = null;
			this.OriginCodeFindBox.PreBoundMaxLength = 5;
			this.OriginCodeFindBox.ShowDescriptionBox = false;
			this.OriginCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 20, true);
			this.OriginCodeFindBox.TabIndex = 0;
			// 
			// OriginPortUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.OriginCodeFindBox);
			this.Controls.Add(this.OriginScheduleDTextBox);
			this.Controls.Add(this.OriginScheduleDDropEdit);
			this.Name = "OriginPortUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(338, 23, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.OriginCodeFindBox.ResumeLayout(true);
			this.OriginCodeFindBox.PerformLayout();
			this.OriginScheduleDDropEdit.ResumeLayout(true);
			this.OriginScheduleDDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal Enterprise.ZArchitecture.GUI.ZCodeFindBox OriginCodeFindBox;
		internal Enterprise.ZArchitecture.ZTextBox OriginScheduleDTextBox;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit OriginScheduleDDropEdit;
	}
}
