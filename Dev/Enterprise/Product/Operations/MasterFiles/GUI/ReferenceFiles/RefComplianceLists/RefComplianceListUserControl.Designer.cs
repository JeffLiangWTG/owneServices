namespace Enterprise.MasterFiles.GUI
{
	partial class RefComplianceListUserControl
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
			this.DetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ModificationDateTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.IntegrationDateTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SecondarySourceTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.MainSourceTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.IsSystemCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.IsActiveCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.PublisherDetailsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PublisherNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PublisherJurisdictionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ListDescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ListNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ListTypeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ListCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ConfigurationGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.IsExcludedCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.DetailsGroupBox.SuspendLayout();
			this.ConfigurationGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.RefComplianceList);
			// 
			// DetailsGroupBox
			// 
			this.DetailsGroupBox.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.DetailsGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("26ae5ebb-9abd-4687-9476-c09cf690ea1f", "Details");
			this.DetailsGroupBox.Controls.Add(this.ModificationDateTextBox);
			this.DetailsGroupBox.Controls.Add(this.IntegrationDateTextBox);
			this.DetailsGroupBox.Controls.Add(this.SecondarySourceTextBox);
			this.DetailsGroupBox.Controls.Add(this.MainSourceTextBox);
			this.DetailsGroupBox.Controls.Add(this.IsSystemCheckBox);
			this.DetailsGroupBox.Controls.Add(this.IsActiveCheckBox);
			this.DetailsGroupBox.Controls.Add(this.PublisherDetailsTextBox);
			this.DetailsGroupBox.Controls.Add(this.PublisherNameTextBox);
			this.DetailsGroupBox.Controls.Add(this.PublisherJurisdictionTextBox);
			this.DetailsGroupBox.Controls.Add(this.ListDescriptionTextBox);
			this.DetailsGroupBox.Controls.Add(this.ListNameTextBox);
			this.DetailsGroupBox.Controls.Add(this.ListTypeTextBox);
			this.DetailsGroupBox.Controls.Add(this.ListCodeTextBox);
			this.DetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 9, true);
			this.DetailsGroupBox.Name = "DetailsGroupBox";
			this.DetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1132, 345, true);
			this.DetailsGroupBox.TabIndex = 2;
			this.DetailsGroupBox.TabStop = false;
			// 
			// ModificationDateTextBox
			// 
			this.BindingSource.SetBindingMember(this.ModificationDateTextBox, "RCL_LastUpdatedDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDate)(((Enterprise.MasterFiles.Business.RefComplianceList)(null)).RCL_LastUpdatedDate)));
			this.ModificationDateTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("0bcb8b22-83a8-434f-b6d7-361c1f4422f4", "Modification Date");
			this.ModificationDateTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ModificationDateTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(803, 54, true);
			this.ModificationDateTextBox.Name = "ModificationDateTextBox";
			this.ModificationDateTextBox.ReadOnly = true;
			this.ModificationDateTextBox.ShouldEscapeAllSpecialCharacters = true;
			this.ModificationDateTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 17, true);
			this.ModificationDateTextBox.TabIndex = 8;
			// 
			// IntegrationDateTextBox
			// 
			this.BindingSource.SetBindingMember(this.IntegrationDateTextBox, "RCL_IntegrationDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDate)(((Enterprise.MasterFiles.Business.RefComplianceList)(null)).RCL_IntegrationDate)));
			this.IntegrationDateTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("a0197abe-7719-49f4-bad7-9257286b983c", "Integration Date");
			this.IntegrationDateTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.IntegrationDateTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(803, 28, true);
			this.IntegrationDateTextBox.Name = "IntegrationDateTextBox";
			this.IntegrationDateTextBox.ReadOnly = true;
			this.IntegrationDateTextBox.ShouldEscapeAllSpecialCharacters = true;
			this.IntegrationDateTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 17, true);
			this.IntegrationDateTextBox.TabIndex = 7;
			// 
			// SecondarySourceTextBox
			// 
			this.BindingSource.SetBindingMember(this.SecondarySourceTextBox, "RCL_SecondarySourceURL");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefComplianceList)(null)).RCL_SecondarySourceURL)));
			this.SecondarySourceTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("f9aa8762-3ea7-448e-8c18-d1a707e49e02", "Secondary Source");
			this.SecondarySourceTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.SecondarySourceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(395, 105, true);
			this.SecondarySourceTextBox.Name = "SecondarySourceTextBox";
			this.SecondarySourceTextBox.ReadOnly = true;
			this.SecondarySourceTextBox.ShouldEscapeAllSpecialCharacters = true;
			this.SecondarySourceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(242, 17, true);
			this.SecondarySourceTextBox.TabIndex = 6;
			// 
			// MainSourceTextBox
			// 
			this.BindingSource.SetBindingMember(this.MainSourceTextBox, "RCL_MainSourceURL");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefComplianceList)(null)).RCL_MainSourceURL)));
			this.MainSourceTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("8292ae9a-9f04-4081-bc82-c0142939f471", "Main Source");
			this.MainSourceTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.MainSourceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(395, 79, true);
			this.MainSourceTextBox.Name = "MainSourceTextBox";
			this.MainSourceTextBox.ReadOnly = true;
			this.MainSourceTextBox.ShouldEscapeAllSpecialCharacters = true;
			this.MainSourceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(242, 17, true);
			this.MainSourceTextBox.TabIndex = 5;
			// 
			// IsSystemCheckBox
			// 
			this.BindingSource.SetBindingMember(this.IsSystemCheckBox, "RCL_IsSystem");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.RefComplianceList)(null)).RCL_IsSystem)));
			this.IsSystemCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("12a5b114-e92c-4266-a68b-154b28e3be29", "Is System");
			this.IsSystemCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsSystemCheckBox.ForeColor = System.Drawing.SystemColors.GrayText;
			this.IsSystemCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1024, 48, true);
			this.IsSystemCheckBox.Name = "IsSystemCheckBox";
			this.IsSystemCheckBox.ReadOnly = true;
			this.IsSystemCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(83, 18, true);
			this.IsSystemCheckBox.TabIndex = 10;
			this.IsSystemCheckBox.UseVisualStyleBackColor = false;
			// 
			// IsActiveCheckBox
			// 
			this.BindingSource.SetBindingMember(this.IsActiveCheckBox, "RCL_IsActive");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.RefComplianceList)(null)).RCL_IsActive)));
			this.IsActiveCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("8bc9eec6-0475-418a-a689-5f039542ac23", "Is Active");
			this.IsActiveCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsActiveCheckBox.ForeColor = System.Drawing.SystemColors.GrayText;
			this.IsActiveCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1024, 27, true);
			this.IsActiveCheckBox.Name = "IsActiveCheckBox";
			this.IsActiveCheckBox.ReadOnly = true;
			this.IsActiveCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(83, 18, true);
			this.IsActiveCheckBox.TabIndex = 9;
			this.IsActiveCheckBox.UseVisualStyleBackColor = false;
			// 
			// PublisherDetailsTextBox
			// 
			this.BindingSource.SetBindingMember(this.PublisherDetailsTextBox, "RCL_PublisherDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefComplianceList)(null)).RCL_PublisherDescription)));
			this.PublisherDetailsTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("80a6285b-e611-48b7-8ef6-7fbf25b62014", "Publisher Description");
			this.PublisherDetailsTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelTop(this.PublisherDetailsTextBox, 0);
			this.PublisherDetailsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 244, true);
			this.PublisherDetailsTextBox.Multiline = true;
			this.PublisherDetailsTextBox.Name = "PublisherDetailsTextBox";
			this.PublisherDetailsTextBox.ReadOnly = true;
			this.PublisherDetailsTextBox.ShouldEscapeAllSpecialCharacters = true;
			this.PublisherDetailsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(822, 66, true);
			this.PublisherDetailsTextBox.TabIndex = 12;
			// 
			// PublisherNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.PublisherNameTextBox, "RCL_ListPublisher");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefComplianceList)(null)).RCL_ListPublisher)));
			this.PublisherNameTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("b7859037-a6f4-4498-ae32-23de8d58b8f0", "Publisher");
			this.PublisherNameTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.PublisherNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(395, 54, true);
			this.PublisherNameTextBox.Name = "PublisherNameTextBox";
			this.PublisherNameTextBox.ReadOnly = true;
			this.PublisherNameTextBox.ShouldEscapeAllSpecialCharacters = true;
			this.PublisherNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(242, 17, true);
			this.PublisherNameTextBox.TabIndex = 4;
			// 
			// PublisherJurisdictionTextBox
			// 
			this.BindingSource.SetBindingMember(this.PublisherJurisdictionTextBox, "RCL_PublisherJurisdiction");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefComplianceList)(null)).RCL_PublisherJurisdiction)));
			this.PublisherJurisdictionTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("732ba8c1-4483-49e2-9102-3b2a244c28df", "Publisher Jurisdiction");
			this.PublisherJurisdictionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.PublisherJurisdictionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 79, true);
			this.PublisherJurisdictionTextBox.Name = "PublisherJurisdictionTextBox";
			this.PublisherJurisdictionTextBox.ReadOnly = true;
			this.PublisherJurisdictionTextBox.ShouldEscapeAllSpecialCharacters = true;
			this.PublisherJurisdictionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 17, true);
			this.PublisherJurisdictionTextBox.TabIndex = 2;
			// 
			// ListDescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.ListDescriptionTextBox, "RCL_ListDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefComplianceList)(null)).RCL_ListDescription)));
			this.ListDescriptionTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("90e22444-1151-4218-b286-7e7db51c28d8", "Description");
			this.ListDescriptionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelTop(this.ListDescriptionTextBox, 0);
			this.ListDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 145, true);
			this.ListDescriptionTextBox.Multiline = true;
			this.ListDescriptionTextBox.Name = "ListDescriptionTextBox";
			this.ListDescriptionTextBox.ReadOnly = true;
			this.ListDescriptionTextBox.ShouldEscapeAllSpecialCharacters = true;
			this.ListDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(822, 87, true);
			this.ListDescriptionTextBox.TabIndex = 11;
			// 
			// ListNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.ListNameTextBox, "RCL_ListName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefComplianceList)(null)).RCL_ListName)));
			this.ListNameTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ef8f5ad9-bd49-4968-91d0-59367d669f24", "Name");
			this.ListNameTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ListNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(395, 28, true);
			this.ListNameTextBox.Name = "ListNameTextBox";
			this.ListNameTextBox.ReadOnly = true;
			this.ListNameTextBox.ShouldEscapeAllSpecialCharacters = true;
			this.ListNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(242, 17, true);
			this.ListNameTextBox.TabIndex = 3;
			// 
			// ListTypeTextBox
			// 
			this.BindingSource.SetBindingMember(this.ListTypeTextBox, "RCL_ListType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefComplianceList)(null)).RCL_ListType)));
			this.ListTypeTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("84331f1a-c6e3-4e16-a106-58f944773d21", "Type");
			this.ListTypeTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ListTypeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 54, true);
			this.ListTypeTextBox.Name = "ListTypeTextBox";
			this.ListTypeTextBox.ReadOnly = true;
			this.ListTypeTextBox.ShouldEscapeAllSpecialCharacters = true;
			this.ListTypeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 17, true);
			this.ListTypeTextBox.TabIndex = 1;
			// 
			// ListCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.ListCodeTextBox, "RCL_ListCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefComplianceList)(null)).RCL_ListCode)));
			this.ListCodeTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("2581994f-2a7f-46a3-9b4d-abf3bb607c14", "Code");
			this.ListCodeTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ListCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 28, true);
			this.ListCodeTextBox.Name = "ListCodeTextBox";
			this.ListCodeTextBox.ReadOnly = true;
			this.ListCodeTextBox.ShouldEscapeAllSpecialCharacters = true;
			this.ListCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 17, true);
			this.ListCodeTextBox.TabIndex = 0;
			// 
			// ConfigurationGroupBox
			// 
			this.ConfigurationGroupBox.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.ConfigurationGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("4b047991-bc74-4319-9682-74679eaf39f3", "Configuration");
			this.ConfigurationGroupBox.Controls.Add(this.IsExcludedCheckBox);
			this.ConfigurationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 359, true);
			this.ConfigurationGroupBox.Name = "ConfigurationGroupBox";
			this.ConfigurationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(517, 72, true);
			this.ConfigurationGroupBox.TabIndex = 8;
			this.ConfigurationGroupBox.TabStop = false;
			// 
			// IsExcludedCheckBox
			// 
			this.BindingSource.SetBindingMember(this.IsExcludedCheckBox, "RCL_IsExcluded");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.RefComplianceList)(null)).RCL_IsExcluded)));
			this.IsExcludedCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("d63e65db-206d-4f30-8932-a4f1d567524f", "Exclude from Denied Party Screening");
			this.IsExcludedCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsExcludedCheckBox.ForeColor = System.Drawing.Color.Black;
			this.IsExcludedCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 35, true);
			this.IsExcludedCheckBox.Name = "IsExcludedCheckBox";
			this.IsExcludedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(229, 18, true);
			this.IsExcludedCheckBox.TabIndex = 13;
			this.IsExcludedCheckBox.UseVisualStyleBackColor = false;
			// 
			// RefComplianceListUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ConfigurationGroupBox);
			this.Controls.Add(this.DetailsGroupBox);
			this.Name = "RefComplianceListUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1157, 440, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.DetailsGroupBox.ResumeLayout(false);
			this.DetailsGroupBox.PerformLayout();
			this.ConfigurationGroupBox.ResumeLayout(false);
			this.ConfigurationGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		protected ZArchitecture.GUI.ZGroupBox DetailsGroupBox;
		protected ZArchitecture.ZTextBox ListDescriptionTextBox;
		protected ZArchitecture.ZTextBox ListTypeTextBox;
		protected ZArchitecture.ZTextBox ListCodeTextBox;
		protected ZArchitecture.ZTextBox PublisherJurisdictionTextBox;
		protected ZArchitecture.ZTextBox PublisherDetailsTextBox;
		protected ZArchitecture.ZTextBox PublisherNameTextBox;
		protected ZArchitecture.ZTextBox ListNameTextBox;
		protected ZArchitecture.GUI.ZGroupBox ConfigurationGroupBox;
		protected ZArchitecture.GUI.ZCheckBox IsSystemCheckBox;
		protected ZArchitecture.GUI.ZCheckBox IsActiveCheckBox;
		protected ZArchitecture.GUI.ZCheckBox IsExcludedCheckBox;
		protected ZArchitecture.ZTextBox ModificationDateTextBox;
		protected ZArchitecture.ZTextBox IntegrationDateTextBox;
		protected ZArchitecture.ZTextBox SecondarySourceTextBox;
		protected ZArchitecture.ZTextBox MainSourceTextBox;
	}
}
