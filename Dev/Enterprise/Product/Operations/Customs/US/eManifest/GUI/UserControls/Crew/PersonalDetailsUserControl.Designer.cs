namespace Enterprise.Customs.US.eManifest.GUI
{
	partial class PersonalDetailsUserControl
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
		void InitializeComponent()
		{
			this.CrewMemberGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CrewTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CitizenshipCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.DateOfBirthDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.GenderDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.FullNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CrewMemberGroupBox.SuspendLayout();
			this.CrewTypeDropEdit.SuspendLayout();
			this.CitizenshipCodeFindBox.SuspendLayout();
			this.DateOfBirthDateEdit.SuspendLayout();
			this.GenderDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.eManifest.Business.CrewMember);
			// 
			// CrewMemberGroupBox
			// 
			this.CrewMemberGroupBox.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("PersonalDetailsUserControl|ee77acd1-fd38-45da-b7b9-990900bf83d9", "Personal Details");
			this.CrewMemberGroupBox.Controls.Add(this.CrewTypeDropEdit);
			this.CrewMemberGroupBox.Controls.Add(this.CitizenshipCodeFindBox);
			this.CrewMemberGroupBox.Controls.Add(this.DateOfBirthDateEdit);
			this.CrewMemberGroupBox.Controls.Add(this.GenderDropEdit);
			this.CrewMemberGroupBox.Controls.Add(this.FullNameTextBox);
			this.CrewMemberGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CrewMemberGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CrewMemberGroupBox.Name = "CrewMemberGroupBox";
			this.CrewMemberGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(322, 150, true);
			this.CrewMemberGroupBox.TabIndex = 0;
			this.CrewMemberGroupBox.TabStop = false;
			// 
			// CrewTypeDropEdit
			// 
			this.CrewTypeDropEdit.AllowDrop = true;
			this.CrewTypeDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.CrewTypeDropEdit, "CP_Type");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.eManifest.Business.CrewMember)(null)).CP_Type)));
			this.CrewTypeDropEdit.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("PersonalDetailsUserControl|e882101d-9e35-41e0-9250-a52b00ea34c9", "Crew Type");
			this.CrewTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(86, 19, true);
			this.CrewTypeDropEdit.Name = "CrewTypeDropEdit";
			this.CrewTypeDropEdit.PreBoundMaxLength = 2;
			this.CrewTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(233, 20, true);
			this.CrewTypeDropEdit.TabIndex = 0;
			// 
			// CitizenshipCodeFindBox
			// 
			this.CitizenshipCodeFindBox.AllowDrop = true;
			this.CitizenshipCodeFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.CitizenshipCodeFindBox, "CP_RN_NKNationality");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.eManifest.Business.CrewMember)(null)).CP_RN_NKNationality)));
			this.CitizenshipCodeFindBox.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("PersonalDetailsUserControl|f347fbcd-0ea7-4820-8ee0-63c14b681bf9", "Citizenship");
			this.CitizenshipCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(86, 123, true);
			this.CitizenshipCodeFindBox.Name = "CitizenshipCodeFindBox";
			this.CitizenshipCodeFindBox.PreBoundMaxLength = 2;
			this.CitizenshipCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(233, 20, true);
			this.CitizenshipCodeFindBox.TabIndex = 4;
			// 
			// DateOfBirthDateEdit
			// 
			this.DateOfBirthDateEdit.AllowDrop = true;
			this.DateOfBirthDateEdit.AutoCompleteMonthThreshold = 1;
			this.DateOfBirthDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.DateOfBirthDateEdit, "CP_DateOfBirth");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.eManifest.Business.CrewMember)(null)).CP_DateOfBirth)));
			this.DateOfBirthDateEdit.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("PersonalDetailsUserControl|dd6716a9-5c00-4565-8dd4-1c7f17e51a3e", "Date Of Birth");
			this.DateOfBirthDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(86, 97, true);
			this.DateOfBirthDateEdit.Name = "DateOfBirthDateEdit";
			this.DateOfBirthDateEdit.TabIndex = 3;
			// 
			// GenderDropEdit
			// 
			this.GenderDropEdit.AllowDrop = true;
			this.GenderDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.GenderDropEdit, "CP_Gender");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.eManifest.Business.CrewMember)(null)).CP_Gender)));
			this.GenderDropEdit.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("PersonalDetailsUserControl|a70314c5-97a4-434a-86e3-324274765e7e", "Gender");
			this.GenderDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(86, 45, true);
			this.GenderDropEdit.Name = "GenderDropEdit";
			this.GenderDropEdit.PreBoundMaxLength = 1;
			this.GenderDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(233, 20, true);
			this.GenderDropEdit.TabIndex = 1;
			// 
			// FullNameTextBox
			// 
			this.FullNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.FullNameTextBox, "CP_FullName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.eManifest.Business.CrewMember)(null)).CP_FullName)));
			this.FullNameTextBox.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("PersonalDetailsUserControl|095698d4-fd4a-494e-9c68-5f1556d0465e", "Full Name");
			this.FullNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(86, 71, true);
			this.FullNameTextBox.Name = "FullNameTextBox";
			this.FullNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(233, 20, true);
			this.FullNameTextBox.TabIndex = 2;
			// 
			// PersonalDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.CrewMemberGroupBox);
			this.Name = "PersonalDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(322, 150, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CrewMemberGroupBox.ResumeLayout(false);
			this.CrewMemberGroupBox.PerformLayout();
			this.CrewTypeDropEdit.ResumeLayout(true);
			this.CrewTypeDropEdit.PerformLayout();
			this.CitizenshipCodeFindBox.ResumeLayout(true);
			this.CitizenshipCodeFindBox.PerformLayout();
			this.DateOfBirthDateEdit.ResumeLayout(true);
			this.DateOfBirthDateEdit.PerformLayout();
			this.GenderDropEdit.ResumeLayout(true);
			this.GenderDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox CrewMemberGroupBox;
		private ZArchitecture.GUI.ZDropEdit CrewTypeDropEdit;
		private ZArchitecture.GUI.ZCodeFindBox CitizenshipCodeFindBox;
		private ZArchitecture.GUI.ZDateEdit DateOfBirthDateEdit;
		private ZArchitecture.GUI.ZDropEdit GenderDropEdit;
		private ZArchitecture.ZTextBox FullNameTextBox;
	}
}
