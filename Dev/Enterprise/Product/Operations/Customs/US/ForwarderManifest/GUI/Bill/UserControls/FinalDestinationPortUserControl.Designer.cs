namespace Enterprise.Customs.US.ForwarderManifest.GUI
{
	partial class FinalDestinationPortUserControl
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
			this.FinalDestinationScheduleKTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.FinalDestinationCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.FinalDestinationScheduleKDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.FinalDestinationCodeFindBox.SuspendLayout();
			this.FinalDestinationScheduleKDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.ForwarderManifest.Business.USExportAsycudaBill);
			// 
			// FinalDestinationScheduleKTextBox
			// 
			this.BindingSource.SetBindingMember(this.FinalDestinationScheduleKTextBox, "ABL_CustomsFinalDestinationPort");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.ForwarderManifest.Business.USExportAsycudaBill)(null)).ABL_CustomsFinalDestinationPort)));
			this.FinalDestinationScheduleKTextBox.CaptionResourceString = Enterprise.Customs.US.ForwarderManifest.GUI.Res.GetData("842561EE-CC67-462F-B2AB-03CC333BF418", "Schedule K");
			this.FinalDestinationScheduleKTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(150, 0, true);
			this.FinalDestinationScheduleKTextBox.Name = "FinalDestinationScheduleKTextBox";
			this.FinalDestinationScheduleKTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(90, 20, true);
			this.FinalDestinationScheduleKTextBox.TabIndex = 1;
			// 
			// FinalDestinationScheduleKDropEdit
			// 
			this.FinalDestinationScheduleKDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.FinalDestinationScheduleKDropEdit, "ABL_CustomsFinalDestinationPort");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.ForwarderManifest.Business.USExportAsycudaBill)(null)).ABL_CustomsFinalDestinationPort)));
			this.FinalDestinationScheduleKDropEdit.CaptionResourceString = Enterprise.Customs.US.ForwarderManifest.GUI.Res.GetData("842561EE-CC67-462F-B2AB-03CC333BF418", "Schedule K");
			this.FinalDestinationScheduleKDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(150, 0, true);
			this.FinalDestinationScheduleKDropEdit.Name = "FinalDestinationScheduleKDropEdit";
			this.FinalDestinationScheduleKDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(90, 20, true);
			this.FinalDestinationScheduleKDropEdit.Visible = false;
			this.FinalDestinationScheduleKDropEdit.TabIndex = 1;
			// 
			// FinalDestinationCodeFindBox
			// 
			this.FinalDestinationCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.FinalDestinationCodeFindBox, "ABL_RL_NKFinalDestination");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.ForwarderManifest.Business.USExportAsycudaBill)(null)).ABL_RL_NKFinalDestination)));
			this.FinalDestinationCodeFindBox.CaptionResourceString = Enterprise.Customs.US.ForwarderManifest.GUI.Res.GetData("842561EE-CC67-462F-B2AB-03CC101BC418", "Final Destination UNLOCO");
			this.FinalDestinationCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.FinalDestinationCodeFindBox.Name = "FinalDestinationCodeFindBox";
			this.FinalDestinationCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.FinalDestinationCodeFindBox.ParentType = null;
			this.FinalDestinationCodeFindBox.PreBoundMaxLength = 5;
			this.FinalDestinationCodeFindBox.ShowDescriptionBox = false;
			this.FinalDestinationCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 20, true);
			this.FinalDestinationCodeFindBox.TabIndex = 0;
			// 
			// FinalDestinationPortUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.FinalDestinationCodeFindBox);
			this.Controls.Add(this.FinalDestinationScheduleKTextBox);
			this.Controls.Add(this.FinalDestinationScheduleKDropEdit);
			this.Name = "FinalDestinationPortUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(338, 23, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.FinalDestinationCodeFindBox.ResumeLayout(true);
			this.FinalDestinationCodeFindBox.PerformLayout();
			this.FinalDestinationScheduleKDropEdit.ResumeLayout(true);
			this.FinalDestinationScheduleKDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal Enterprise.ZArchitecture.GUI.ZCodeFindBox FinalDestinationCodeFindBox;
		internal Enterprise.ZArchitecture.ZTextBox FinalDestinationScheduleKTextBox;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit FinalDestinationScheduleKDropEdit;
	}
}
