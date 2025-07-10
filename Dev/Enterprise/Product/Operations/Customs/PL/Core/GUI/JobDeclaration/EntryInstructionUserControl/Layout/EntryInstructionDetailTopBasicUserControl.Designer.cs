namespace Enterprise.Customs.PL.GUI
{
	partial class EntryInstructionDetailTopBasicUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.ExportManifestCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.PostExportTransitCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.EADPrintOutDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.OfficeOfExitArrivalTimeLimitDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.TemporaryLocationDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.TemporaryLocationTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DeclarationDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.EADPrintOutDropEdit.SuspendLayout();
			this.OfficeOfExitArrivalTimeLimitDateEdit.SuspendLayout();
			this.TemporaryLocationDropEdit.SuspendLayout();
			this.DeclarationDateDateEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.PL.Business.Declaration.CusEntryInstruction);
			// 
			// ExportManifestCheckBox
			// 
			this.ExportManifestCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ExportManifestCheckBox, "ZG_ExportManifest");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.PL.Business.Declaration.CusEntryInstruction)(null)).ZG_ExportManifest)));
			this.ExportManifestCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.ExportManifestCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ExportManifestCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(481, 18, true);
			this.ExportManifestCheckBox.Name = "ExportManifestCheckBox";
			this.ExportManifestCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(14, 14, true);
			this.ExportManifestCheckBox.TabIndex = 1;
			// 
			// PostExportTransitCheckBox
			// 
			this.PostExportTransitCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.PostExportTransitCheckBox, "ZG_PostExportTransit");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.PL.Business.Declaration.CusEntryInstruction)(null)).ZG_PostExportTransit)));
			this.PostExportTransitCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.PostExportTransitCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.PostExportTransitCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(481, 41, true);
			this.PostExportTransitCheckBox.Name = "PostExportTransitCheckBox";
			this.PostExportTransitCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(14, 14, true);
			this.PostExportTransitCheckBox.TabIndex = 5;
			// 
			// EADPrintOutDropEdit
			// 
			this.EADPrintOutDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.EADPrintOutDropEdit, "ZG_EADPrintOut");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.PL.Business.Declaration.CusEntryInstruction)(null)).ZG_EADPrintOut)));
			this.EADPrintOutDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 38, true);
			this.EADPrintOutDropEdit.Name = "EADPrintOutDropEdit";
			this.EADPrintOutDropEdit.ShouldResizeByMaxLength = true;
			this.EADPrintOutDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 18, true);
			this.EADPrintOutDropEdit.TabIndex = 4;
			// 
			// OfficeOfExitArrivalTimeLimitDateEdit
			// 
			this.OfficeOfExitArrivalTimeLimitDateEdit.AllowDrop = true;
			this.OfficeOfExitArrivalTimeLimitDateEdit.AutoCompleteMonthThreshold = 1;
			this.OfficeOfExitArrivalTimeLimitDateEdit.AutoCompleteYear = true;
			this.OfficeOfExitArrivalTimeLimitDateEdit.AutoSize = true;
			this.BindingSource.SetBindingMember(this.OfficeOfExitArrivalTimeLimitDateEdit, "ZG_OfficeOfExitArrivalTimeLimit");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.PL.Business.Declaration.CusEntryInstruction)(null)).ZG_OfficeOfExitArrivalTimeLimit)));
			this.OfficeOfExitArrivalTimeLimitDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(637, 62, true);
			this.OfficeOfExitArrivalTimeLimitDateEdit.Name = "OfficeOfExitArrivalTimeLimitDateEdit";
			this.OfficeOfExitArrivalTimeLimitDateEdit.TabIndex = 7;
			// 
			// TemporaryLocationDropEdit
			// 
			this.TemporaryLocationDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TemporaryLocationDropEdit, "ZG_TemporaryLocationCodeType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.PL.Business.Declaration.CusEntryInstruction)(null)).ZG_TemporaryLocationCodeType)));
			this.TemporaryLocationDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(637, 18, true);
			this.TemporaryLocationDropEdit.Name = "TemporaryLocationDropEdit";
			this.TemporaryLocationDropEdit.PreBoundMaxLength = 4;
			this.TemporaryLocationDropEdit.ShouldResizeByMaxLength = true;
			this.TemporaryLocationDropEdit.ShowDescriptionBox = false;
			this.TemporaryLocationDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(62, 18, true);
			this.TemporaryLocationDropEdit.TabIndex = 2;
			// 
			// TemporaryLocationTextBox
			// 
			this.BindingSource.SetBindingMember(this.TemporaryLocationTextBox, "ZG_TemporaryLocation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.PL.Business.Declaration.CusEntryInstruction)(null)).ZG_TemporaryLocation)));
			this.TemporaryLocationTextBox.CaptionResourceString = null;
			this.TemporaryLocationTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(711, 18, true);
			this.TemporaryLocationTextBox.Name = "TemporaryLocationTextBox";
			this.TemporaryLocationTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(334, 18, true);
			this.TemporaryLocationTextBox.TabIndex = 3;
			// 
			// DeclarationDateDateEdit
			// 
			this.DeclarationDateDateEdit.AllowDrop = true;
			this.DeclarationDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.DeclarationDateDateEdit.AutoCompleteYear = true;
			this.DeclarationDateDateEdit.AutoSize = true;
			this.BindingSource.SetBindingMember(this.DeclarationDateDateEdit, "CEI_DateForDuty");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.PL.Business.Declaration.CusEntryInstruction)(null)).CEI_DateForDuty)));
			this.DeclarationDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 62, true);
			this.DeclarationDateDateEdit.Name = "DeclarationDateDateEdit";
			this.DeclarationDateDateEdit.TabIndex = 6;
			// 
			// EntryInstructionDetailExportBasicUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ExportManifestCheckBox);
			this.Controls.Add(this.PostExportTransitCheckBox);
			this.Controls.Add(this.EADPrintOutDropEdit);
			this.Controls.Add(this.OfficeOfExitArrivalTimeLimitDateEdit);
			this.Controls.Add(this.TemporaryLocationDropEdit);
			this.Controls.Add(this.TemporaryLocationTextBox);
			this.Controls.Add(this.DeclarationDateDateEdit);
			this.Name = "EntryInstructionDetailExportBasicUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1097, 113, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.EADPrintOutDropEdit.ResumeLayout(true);
			this.EADPrintOutDropEdit.PerformLayout();
			this.OfficeOfExitArrivalTimeLimitDateEdit.ResumeLayout(true);
			this.OfficeOfExitArrivalTimeLimitDateEdit.PerformLayout();
			this.TemporaryLocationDropEdit.ResumeLayout(true);
			this.TemporaryLocationDropEdit.PerformLayout();
			this.DeclarationDateDateEdit.ResumeLayout(true);
			this.DeclarationDateDateEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		internal ZArchitecture.GUI.ZCheckBox ExportManifestCheckBox;
		internal ZArchitecture.GUI.ZCheckBox PostExportTransitCheckBox;
		internal ZArchitecture.GUI.ZDropEdit EADPrintOutDropEdit;
		internal ZArchitecture.GUI.ZDropEdit TemporaryLocationDropEdit;
		internal ZArchitecture.ZTextBox TemporaryLocationTextBox;
		internal ZArchitecture.GUI.ZDateEdit OfficeOfExitArrivalTimeLimitDateEdit;
		internal ZArchitecture.GUI.ZDateEdit DeclarationDateDateEdit;

		#endregion
	}
}
