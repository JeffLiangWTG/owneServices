using Enterprise.ZArchitecture;

namespace Enterprise.MasterFiles.GUI
{
	partial class TemplateFileUserControl
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.TemplateFileGrid = new Enterprise.ZArchitecture.ZGrid();
			this.pnlGeneral = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.pnlButtons = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.btnDelete = new Enterprise.ZArchitecture.GUI.ZButton();
			this.btnEditFile = new Enterprise.ZArchitecture.GUI.ZButton();
			this.btnNew = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.TemplateFileGrid)).BeginInit();
			this.TemplateFileGrid.SuspendLayout();
			this.pnlGeneral.SuspendLayout();
			this.pnlButtons.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.GlbCompany);
			// 
			// TemplateFileGrid
			// 
			this.TemplateFileGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.TemplateFileGrid, "TemplateFiles");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbCompany)(null)).TemplateFiles)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccTemplateFileStorage)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbCompany)(null)).TemplateFiles)).SyncRoot)).TFS_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccTemplateFileStorage)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbCompany)(null)).TemplateFiles)).SyncRoot)).TFS_Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.AccTemplateFileStorage)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbCompany)(null)).TemplateFiles)).SyncRoot)).TFS_IsActive)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccTemplateFileStorage)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbCompany)(null)).TemplateFiles)).SyncRoot)).TFS_FileName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccTemplateFileStorage)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbCompany)(null)).TemplateFiles)).SyncRoot)).TFS_ExternalReference_ForBinding)));
			this.TemplateFileGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("4649bb17-6733-42bc-86f2-91d2c552ae36", "Code");
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "TFS_Code";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(53);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("db72238e-2562-4f7f-9f07-9f35d112edbb", "Description");
			zTextBoxColumnStyleInfo2.ColumnName = "TFS_Description";
			zTextBoxColumnStyleInfo2.IsMandatory = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("27bb3eec-da47-4469-8cd1-2929d3e2afec", "Active");
			zCheckBoxColumnStyleInfo1.ColumnName = "TFS_IsActive";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo3.AllowMultipleMacroses = false;
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("12587e13-de62-43ce-be9c-2f1070da2ef4", "File Name");
			zTextBoxColumnStyleInfo3.ColumnName = "TFS_FileName";
			zTextBoxColumnStyleInfo3.IsMandatory = false;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("EA5FA05D-A35F-4F81-ABB5-91A1C22D177B", "External Reference");
			zTextBoxColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo4.ColumnName = "TFS_ExternalReference_ForBinding";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			this.TemplateFileGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.TemplateFileGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.TemplateFileGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.TemplateFileGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.TemplateFileGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.TemplateFileGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TemplateFileGrid.GridId = "8B1CF0B2-793C-43E8-B91E-30781220F90F";
			this.TemplateFileGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.TemplateFileGrid.LayoutKey = "TemplateFileGrid";
			this.TemplateFileGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TemplateFileGrid.Name = "TemplateFileGrid";
			this.TemplateFileGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.TemplateFileGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(360, 300, true);
			this.TemplateFileGrid.TabIndex = 3;
			// 
			// pnlGeneral
			// 
			this.pnlGeneral.Controls.Add(this.TemplateFileGrid);
			this.pnlGeneral.Controls.Add(this.pnlButtons);
			this.pnlGeneral.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pnlGeneral.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.pnlGeneral.Name = "pnlGeneral";
			this.pnlGeneral.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(450, 300, true);
			this.pnlGeneral.TabIndex = 4;
			// 
			// pnlButtons
			// 
			this.pnlButtons.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.pnlButtons.Controls.Add(this.btnDelete);
			this.pnlButtons.Controls.Add(this.btnEditFile);
			this.pnlButtons.Controls.Add(this.btnNew);
			this.pnlButtons.Dock = System.Windows.Forms.DockStyle.Right;
			this.pnlButtons.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(360, 0, true);
			this.pnlButtons.Name = "pnlButtons";
			this.pnlButtons.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(90, 300, true);
			this.pnlButtons.TabIndex = 0;
			// 
			// btnDelete
			// 
			this.btnDelete.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("9b82c9a1-6bb3-4d92-84f8-224329281f06", "Delete");
			this.btnDelete.IsCaptionOverridden = true;
			this.btnDelete.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 91, true);
			this.btnDelete.Name = "btnDelete";
			this.btnDelete.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.btnDelete.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(70, 32, true);
			this.btnDelete.TabIndex = 2;
			this.btnDelete.Text = "Delete";
			this.btnDelete.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.btnDelete.ToolTipCaption = null;
			this.btnDelete.UseVisualStyleBackColor = true;
			this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
			// 
			// btnChange
			// 
			this.btnEditFile.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("b7b14ca2-c530-4437-813b-cef2d034566f", "Edit File");
			this.btnEditFile.IsCaptionOverridden = true;
			this.btnEditFile.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 53, true);
			this.btnEditFile.Name = "btnEditFile";
			this.btnEditFile.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.btnEditFile.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(70, 32, true);
			this.btnEditFile.TabIndex = 1;
			this.btnEditFile.Text = "Edit File";
			this.btnEditFile.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.btnEditFile.ToolTipCaption = null;
			this.btnEditFile.UseVisualStyleBackColor = true;
			this.btnEditFile.Click += new System.EventHandler(this.btnEditFile_Click);
			// 
			// btnNew
			// 
			this.btnNew.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("acb02987-0e26-4049-89d8-a1ea78e23401", "New");
			this.btnNew.IsCaptionOverridden = true;
			this.btnNew.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 15, true);
			this.btnNew.Name = "btnNew";
			this.btnNew.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.btnNew.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(70, 32, true);
			this.btnNew.TabIndex = 0;
			this.btnNew.Text = "New";
			this.btnNew.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.btnNew.ToolTipCaption = null;
			this.btnNew.UseVisualStyleBackColor = true;
			this.btnNew.Click += new System.EventHandler(this.btnNew_Click);
			// 
			// TemplateFileUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.pnlGeneral);
			this.Name = "TemplateFileUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(450, 300, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.TemplateFileGrid)).EndInit();
			this.TemplateFileGrid.ResumeLayout(false);
			this.TemplateFileGrid.PerformLayout();
			this.pnlGeneral.ResumeLayout(false);
			this.pnlGeneral.PerformLayout();
			this.pnlButtons.ResumeLayout(false);
			this.pnlButtons.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZGrid TemplateFileGrid;
		private ZArchitecture.GUI.ZPanel pnlButtons;
		private ZArchitecture.GUI.ZPanel pnlGeneral;
		private ZArchitecture.GUI.ZButton btnNew;
		private ZArchitecture.GUI.ZButton btnEditFile;
		private ZArchitecture.GUI.ZButton btnDelete;
	}
}
