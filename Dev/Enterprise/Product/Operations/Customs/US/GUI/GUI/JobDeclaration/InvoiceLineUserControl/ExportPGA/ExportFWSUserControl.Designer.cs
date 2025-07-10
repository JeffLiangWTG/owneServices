namespace Enterprise.Customs.US.GUI
{
	partial class ExportFWSUserControl
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
			this.FWSGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.StateOfOriginDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.WildLifeCategoryDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ExemCertCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.SourceCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.SpeciesOriginCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.DescriptionCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.PurposeCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.TaxonomicSerialTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ConfirmationNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.FWSGroupBox.SuspendLayout();
			this.StateOfOriginDropEdit.SuspendLayout();
			this.WildLifeCategoryDropEdit.SuspendLayout();
			this.ExemCertCodeDropEdit.SuspendLayout();
			this.SourceCodeDropEdit.SuspendLayout();
			this.SpeciesOriginCodeFindBox.SuspendLayout();
			this.DescriptionCodeDropEdit.SuspendLayout();
			this.PurposeCodeDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.Business.FWSHeader);
			// 
			// FWSGroupBox
			// 
			this.FWSGroupBox.Controls.Add(this.StateOfOriginDropEdit);
			this.FWSGroupBox.Controls.Add(this.WildLifeCategoryDropEdit);
			this.FWSGroupBox.Controls.Add(this.ExemCertCodeDropEdit);
			this.FWSGroupBox.Controls.Add(this.SourceCodeDropEdit);
			this.FWSGroupBox.Controls.Add(this.SpeciesOriginCodeFindBox);
			this.FWSGroupBox.Controls.Add(this.DescriptionCodeDropEdit);
			this.FWSGroupBox.Controls.Add(this.PurposeCodeDropEdit);
			this.FWSGroupBox.Controls.Add(this.TaxonomicSerialTextBox);
			this.FWSGroupBox.Controls.Add(this.ConfirmationNumberTextBox);
			this.FWSGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FWSGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.FWSGroupBox.Name = "FWSGroupBox";
			this.FWSGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(555, 174, true);
			this.FWSGroupBox.TabIndex = 1;
			this.FWSGroupBox.TabStop = false;
			this.FWSGroupBox.Text = "FWS - Fish & Wildlife Service";
			// 
			// StateOfOriginDropEdit
			// 
			this.StateOfOriginDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.StateOfOriginDropEdit, "US_USState");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.FWSHeader)(null)).US_USState)));
			this.StateOfOriginDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(385, 129, true);
			this.StateOfOriginDropEdit.Name = "StateOfOriginDropEdit";
			this.StateOfOriginDropEdit.PreBoundMaxLength = 2;
			this.StateOfOriginDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(121, 20, true);
			this.StateOfOriginDropEdit.TabIndex = 8;
			// 
			// WildLifeCategoryDropEdit
			// 
			this.WildLifeCategoryDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.WildLifeCategoryDropEdit, "US_WildlifeCategoryCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.FWSHeader)(null)).US_WildlifeCategoryCode)));
			this.WildLifeCategoryDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("7ca8a430-40ef-4ad7-a63d-6e31be1b98c9", "Wildlife Category Code");
			this.WildLifeCategoryDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(137, 103, true);
			this.WildLifeCategoryDropEdit.Name = "WildLifeCategoryDropEdit";
			this.WildLifeCategoryDropEdit.PreBoundMaxLength = 3;
			this.WildLifeCategoryDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
			this.WildLifeCategoryDropEdit.TabIndex = 5;
			// 
			// ExemCertCodeDropEdit
			// 
			this.ExemCertCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ExemCertCodeDropEdit, "US_CertificationCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.FWSHeader)(null)).US_CertificationCode)));
			this.ExemCertCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(385, 103, true);
			this.ExemCertCodeDropEdit.Name = "ExemCertCodeDropEdit";
			this.ExemCertCodeDropEdit.PreBoundMaxLength = 3;
			this.ExemCertCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(121, 20, true);
			this.ExemCertCodeDropEdit.TabIndex = 6;
			// 
			// SourceCodeDropEdit
			// 
			this.SourceCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SourceCodeDropEdit, "US_WildlifeSource");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.FWSHeader)(null)).US_WildlifeSource)));
			this.SourceCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(385, 77, true);
			this.SourceCodeDropEdit.Name = "SourceCodeDropEdit";
			this.SourceCodeDropEdit.PreBoundMaxLength = 3;
			this.SourceCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(121, 20, true);
			this.SourceCodeDropEdit.TabIndex = 4;
			// 
			// SpeciesOriginCodeFindBox
			// 
			this.SpeciesOriginCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SpeciesOriginCodeFindBox, "US_SpeciesOrigin");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.FWSHeader)(null)).US_SpeciesOrigin)));
			this.SpeciesOriginCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(137, 129, true);
			this.SpeciesOriginCodeFindBox.Name = "SpeciesOriginCodeFindBox";
			this.SpeciesOriginCodeFindBox.PreBoundMaxLength = 2;
			this.SpeciesOriginCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
			this.SpeciesOriginCodeFindBox.TabIndex = 7;
			// 
			// DescriptionCodeDropEdit
			// 
			this.DescriptionCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DescriptionCodeDropEdit, "US_WildlifeDescriptionCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.FWSHeader)(null)).US_WildlifeDescriptionCode)));
			this.DescriptionCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(137, 77, true);
			this.DescriptionCodeDropEdit.Name = "DescriptionCodeDropEdit";
			this.DescriptionCodeDropEdit.PreBoundMaxLength = 3;
			this.DescriptionCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
			this.DescriptionCodeDropEdit.TabIndex = 3;
			// 
			// PurposeCodeDropEdit
			// 
			this.PurposeCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PurposeCodeDropEdit, "US_PurposeCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.FWSHeader)(null)).US_PurposeCode)));
			this.PurposeCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(385, 51, true);
			this.PurposeCodeDropEdit.Name = "PurposeCodeDropEdit";
			this.PurposeCodeDropEdit.PreBoundMaxLength = 1;
			this.PurposeCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(121, 20, true);
			this.PurposeCodeDropEdit.TabIndex = 2;
			// 
			// TaxonomicSerialTextBox
			// 
			this.BindingSource.SetBindingMember(this.TaxonomicSerialTextBox, "US_TaxonomicSerialNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.FWSHeader)(null)).US_TaxonomicSerialNumber)));
			this.TaxonomicSerialTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(137, 51, true);
			this.TaxonomicSerialTextBox.Name = "TaxonomicSerialTextBox";
			this.TaxonomicSerialTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
			this.TaxonomicSerialTextBox.TabIndex = 1;
			// 
			// ConfirmationNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.ConfirmationNumberTextBox, "US_ConfirmationNum");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.FWSHeader)(null)).US_ConfirmationNum)));
			this.ConfirmationNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(137, 25, true);
			this.ConfirmationNumberTextBox.Name = "ConfirmationNumberTextBox";
			this.ConfirmationNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(369, 20, true);
			this.ConfirmationNumberTextBox.TabIndex = 0;
			// 
			// ExportFWSUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.FWSGroupBox);
			this.Name = "ExportFWSUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(555, 174, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.FWSGroupBox.ResumeLayout(false);
			this.FWSGroupBox.PerformLayout();
			this.StateOfOriginDropEdit.ResumeLayout(true);
			this.StateOfOriginDropEdit.PerformLayout();
			this.WildLifeCategoryDropEdit.ResumeLayout(true);
			this.WildLifeCategoryDropEdit.PerformLayout();
			this.ExemCertCodeDropEdit.ResumeLayout(true);
			this.ExemCertCodeDropEdit.PerformLayout();
			this.SourceCodeDropEdit.ResumeLayout(true);
			this.SourceCodeDropEdit.PerformLayout();
			this.SpeciesOriginCodeFindBox.ResumeLayout(true);
			this.SpeciesOriginCodeFindBox.PerformLayout();
			this.DescriptionCodeDropEdit.ResumeLayout(true);
			this.DescriptionCodeDropEdit.PerformLayout();
			this.PurposeCodeDropEdit.ResumeLayout(true);
			this.PurposeCodeDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox FWSGroupBox;
		private ZArchitecture.GUI.ZDropEdit StateOfOriginDropEdit;
		private ZArchitecture.GUI.ZDropEdit WildLifeCategoryDropEdit;
		private ZArchitecture.GUI.ZDropEdit ExemCertCodeDropEdit;
		private ZArchitecture.GUI.ZDropEdit SourceCodeDropEdit;
		private ZArchitecture.GUI.ZCodeFindBox SpeciesOriginCodeFindBox;
		private ZArchitecture.GUI.ZDropEdit DescriptionCodeDropEdit;
		private ZArchitecture.GUI.ZDropEdit PurposeCodeDropEdit;
		private ZArchitecture.ZTextBox TaxonomicSerialTextBox;
		private ZArchitecture.ZTextBox ConfirmationNumberTextBox;
	}
}
