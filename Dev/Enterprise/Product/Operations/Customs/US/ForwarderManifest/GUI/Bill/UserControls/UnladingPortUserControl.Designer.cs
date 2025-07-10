namespace Enterprise.Customs.US.ForwarderManifest.GUI
{
	partial class UnladingPortUserControl
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
			this.UnladingPortScheduleKTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.UnladingPortCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.UnladingPortScheduleKDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.UnladingPortCodeFindBox.SuspendLayout();
			this.UnladingPortScheduleKDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.ForwarderManifest.Business.USExportAsycudaBill);
			// 
			// UnladingPortScheduleKTextBox
			// 
			this.BindingSource.SetBindingMember(this.UnladingPortScheduleKTextBox, "ABL_CustomsDischargePort");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.ForwarderManifest.Business.USExportAsycudaBill)(null)).ABL_CustomsDischargePort)));
			this.UnladingPortScheduleKTextBox.CaptionResourceString = Enterprise.Customs.US.ForwarderManifest.GUI.Res.GetData("842561EE-CC67-462F-B2AB-BFE33CA8D333", "Schedule K");
			this.UnladingPortScheduleKTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(150, 0, true);
			this.UnladingPortScheduleKTextBox.Name = "UnladingPortScheduleKTextBox";
			this.UnladingPortScheduleKTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(90, 20, true);
			this.UnladingPortScheduleKTextBox.TabIndex = 1;
			// 
			// UnladingPortScheduleKDropEdit
			// 
			this.UnladingPortScheduleKDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.UnladingPortScheduleKDropEdit, "ABL_CustomsDischargePort");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.ForwarderManifest.Business.USExportAsycudaBill)(null)).ABL_CustomsDischargePort)));
			this.UnladingPortScheduleKDropEdit.CaptionResourceString = Enterprise.Customs.US.ForwarderManifest.GUI.Res.GetData("842561EE-CC67-462F-B2AB-BFE33CA8D333", "Schedule K");
			this.UnladingPortScheduleKDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(150, 0, true);
			this.UnladingPortScheduleKDropEdit.Name = "UnladingPortScheduleKDropEdit";
			this.UnladingPortScheduleKDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(90, 20, true);
			this.UnladingPortScheduleKDropEdit.Visible = false;
			this.UnladingPortScheduleKDropEdit.TabIndex = 1;
			// 
			// UnladingPortCodeFindBox
			// 
			this.UnladingPortCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.UnladingPortCodeFindBox, "ABL_RL_NKPortOfDischarge");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.ForwarderManifest.Business.USExportAsycudaBill)(null)).ABL_RL_NKPortOfDischarge)));
			this.UnladingPortCodeFindBox.CaptionResourceString = Enterprise.Customs.US.ForwarderManifest.GUI.Res.GetData("842561EE-CC67-462F-B2AB-BFE33CA8D336", "Port of Unlading UNLOCO");
			this.UnladingPortCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.UnladingPortCodeFindBox.Name = "UnladingPortCodeFindBox";
			this.UnladingPortCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.UnladingPortCodeFindBox.ParentType = null;
			this.UnladingPortCodeFindBox.PreBoundMaxLength = 5;
			this.UnladingPortCodeFindBox.ShowDescriptionBox = false;
			this.UnladingPortCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 20, true);
			this.UnladingPortCodeFindBox.TabIndex = 0;
			// 
			// UnladingPortUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.UnladingPortCodeFindBox);
			this.Controls.Add(this.UnladingPortScheduleKTextBox);
			this.Controls.Add(this.UnladingPortScheduleKDropEdit);
			this.Name = "UnladingPortUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(338, 23, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.UnladingPortCodeFindBox.ResumeLayout(true);
			this.UnladingPortCodeFindBox.PerformLayout();
			this.UnladingPortScheduleKDropEdit.ResumeLayout(true);
			this.UnladingPortScheduleKDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal Enterprise.ZArchitecture.GUI.ZCodeFindBox UnladingPortCodeFindBox;
		internal Enterprise.ZArchitecture.ZTextBox UnladingPortScheduleKTextBox;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit UnladingPortScheduleKDropEdit;
	}
}
