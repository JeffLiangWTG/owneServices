namespace Enterprise.MasterFiles.GUI
{
	public partial class ExporterSchemeControl
	{

		#region Component Designer generated code

		protected Enterprise.ZArchitecture.GUI.ZGroupBox MajorExporterGroupBox;
		protected Enterprise.ZArchitecture.ZTextBox ExportPermissionsTextBox;
		protected Enterprise.ZArchitecture.GUI.ZDropEdit OV_IMApprovedOrMajorExporterDropEdit;
		protected Enterprise.ZArchitecture.GUI.ZDateEdit LastReviewedOnDateEdit;
		private Enterprise.ZArchitecture.GUI.ZPanel ExporterSchemePanel;

		private void InitializeComponent()
		{
			this.MajorExporterGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.LastReviewedOnDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ExportPermissionsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.OV_IMApprovedOrMajorExporterDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ExporterSchemePanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MajorExporterGroupBox.SuspendLayout();
			this.ExporterSchemePanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgHeader);
			// 
			// MajorExporterGroupBox
			// 
			this.MajorExporterGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ExporterSchemeControl|2517b193-17fc-41bd-bd8f-cc41e01c1bb1", "Aviation Security Scheme Details", "Provides details as to whether this organization is approved for the current country's aviation security scheme.");
			this.MajorExporterGroupBox.Controls.Add(this.LastReviewedOnDateEdit);
			this.MajorExporterGroupBox.Controls.Add(this.ExportPermissionsTextBox);
			this.MajorExporterGroupBox.Controls.Add(this.OV_IMApprovedOrMajorExporterDropEdit);
			this.MajorExporterGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MajorExporterGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 5, true);
			this.MajorExporterGroupBox.Name = "MajorExporterGroupBox";
			this.MajorExporterGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(447, 121, true);
			this.MajorExporterGroupBox.TabIndex = 5;
			this.MajorExporterGroupBox.TabStop = false;
			// 
			// LastReviewedOnDateEdit
			// 
			this.LastReviewedOnDateEdit.AutoCompleteMonthThreshold = 1;
			this.LastReviewedOnDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.LastReviewedOnDateEdit, "CountryData+OV_LastReviewedOn");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CountryData.OV_LastReviewedOn)));
			this.LastReviewedOnDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(144, 69, true);
			this.LastReviewedOnDateEdit.Name = "LastReviewedOnDateEdit";
			this.LastReviewedOnDateEdit.TabIndex = 4;
			// 
			// ExportPermissionsTextBox
			// 
			this.ExportPermissionsTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ExportPermissionsTextBox, "CountryData.OV_EXExportPermissionDetails");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CountryData.OV_EXExportPermissionDetails)));
			this.ExportPermissionsTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ExportPermissionsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(144, 43, true);
			this.ExportPermissionsTextBox.Name = "ExportPermissionsTextBox";
			this.ExportPermissionsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(270, 20, true);
			this.ExportPermissionsTextBox.TabIndex = 1;
			// 
			// OV_IMApprovedOrMajorExporterDropEdit
			// 
			this.BindingSource.SetBindingMember(this.OV_IMApprovedOrMajorExporterDropEdit, "CountryData.OV_EXApprovedOrMajorExporter");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CountryData.OV_EXApprovedOrMajorExporter)));
			this.OV_IMApprovedOrMajorExporterDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.OV_IMApprovedOrMajorExporterDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(144, 17, true);
			this.OV_IMApprovedOrMajorExporterDropEdit.Name = "OV_IMApprovedOrMajorExporterDropEdit";
			this.OV_IMApprovedOrMajorExporterDropEdit.PreBoundMaxLength = 3;
			this.OV_IMApprovedOrMajorExporterDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(270, 20, true);
			this.OV_IMApprovedOrMajorExporterDropEdit.TabIndex = 0;
			// 
			// ExporterSchemePanel
			// 
			this.ExporterSchemePanel.Controls.Add(this.MajorExporterGroupBox);
			this.ExporterSchemePanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ExporterSchemePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ExporterSchemePanel.Name = "ExporterSchemePanel";
			this.ExporterSchemePanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, true);
			this.ExporterSchemePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(457, 131, true);
			this.ExporterSchemePanel.TabIndex = 6;
			// 
			// ExporterSchemeControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ExporterSchemePanel);
			this.Name = "ExporterSchemeControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(457, 131, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MajorExporterGroupBox.ResumeLayout(false);
			this.MajorExporterGroupBox.PerformLayout();
			this.ExporterSchemePanel.ResumeLayout(false);
			this.ResumeLayout(false);
		}
		#endregion

	}
}
