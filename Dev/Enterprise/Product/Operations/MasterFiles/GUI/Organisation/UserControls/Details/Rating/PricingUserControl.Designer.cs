using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI.Organisation.UserControls
{
	public partial class PricingUserControl
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
			this.components = new System.ComponentModel.Container();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new ZDropEditColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new ZDropEditColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new ZDropEditColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new ZDropEditColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo5 = new ZDropEditColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo6 = new ZDropEditColumnStyleInfo();
			ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new ZCodeFindBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new ZGuidFindBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo7 = new ZDropEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new ZCalcEditColumnStyleInfo();
			this.PricingDetailsGroupBox = new ZGroupBox();
			this.GroupChargesBox = new ZGroupBox();
			this.ModuleDropEdit = new ZDropEdit();
			this.JobTypeDropEdit = new ZDropEdit();
			this.ModeDropEdit = new ZDropEdit();
			this.DisplayDropEdit = new ZDropEdit();
			this.StyleDropEdit = new ZDropEdit();
			this.GroupChargesGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PricingDetailsGroupBox.SuspendLayout();
			this.GroupChargesBox.SuspendLayout();
			this.ModuleDropEdit.SuspendLayout();
			this.JobTypeDropEdit.SuspendLayout();
			this.ModeDropEdit.SuspendLayout();
			this.DisplayDropEdit.SuspendLayout();
			this.StyleDropEdit.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.GroupChargesGrid)).BeginInit();
			this.GroupChargesGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgHeader);
			// 
			// PricingDetailsGroupBox
			// 
			this.PricingDetailsGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("0B2EEE58-E3F9-4D66-A1EE-E1194A8FA206", "Pricing Details");
			this.PricingDetailsGroupBox.Controls.Add(this.GroupChargesBox);
			this.PricingDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 2, true);
			this.PricingDetailsGroupBox.Name = "PricingDetailsGroupBox";
			this.PricingDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(750, 410, true);
			this.PricingDetailsGroupBox.TabIndex = 0;
			this.PricingDetailsGroupBox.TabStop = false;
			this.PricingDetailsGroupBox.Text = "Pricing Details";
			// 
			// GroupChargesBox
			// 
			this.GroupChargesBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
				| System.Windows.Forms.AnchorStyles.Right)));
			this.GroupChargesBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("D12E8101-CAFA-4902-8071-3C0684579BE5", "Charge Grouping && Roll Up");
			this.GroupChargesBox.Controls.Add(this.ModuleDropEdit);
			this.GroupChargesBox.Controls.Add(this.JobTypeDropEdit);
			this.GroupChargesBox.Controls.Add(this.ModeDropEdit);
			this.GroupChargesBox.Controls.Add(this.DisplayDropEdit);
			this.GroupChargesBox.Controls.Add(this.StyleDropEdit);
			this.GroupChargesBox.Controls.Add(this.GroupChargesGrid);
			this.GroupChargesBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 17, true);
			this.GroupChargesBox.Name = "GroupChargesBox";
			this.GroupChargesBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(730, 390, true);
			this.GroupChargesBox.TabIndex = 0;
			this.GroupChargesBox.TabStop = false;
			this.GroupChargesBox.Text = "Charge Grouping && Roll Up";
			// 
			// ModuleDropEdit
			// 
			this.ModuleDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ModuleDropEdit, "CompanyData+RatingDocRollupOrGroups.RCG_Module");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.Rating.RatingDocumentsChargeGroupingOrRollup)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.RatingDocRollupOrGroups)).SyncRoot)).RCG_Module)));
			this.ModuleDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("067771BF-DC9A-4DA3-80E1-7693BF0AF2ED", "Module");
			this.ModuleDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(90, 160, true);
			this.ModuleDropEdit.Name = "ModuleDropEdit";
			this.ModuleDropEdit.PreBoundMaxLength = 3;
			this.ModuleDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 18, true);
			this.ModuleDropEdit.TabIndex = 1;
			// 
			// JobTypeDropEdit
			// 
			this.JobTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JobTypeDropEdit, "CompanyData+RatingDocRollupOrGroups.RCG_JobType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.Rating.RatingDocumentsChargeGroupingOrRollup)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.RatingDocRollupOrGroups)).SyncRoot)).RCG_JobType)));
			this.JobTypeDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("0FE15FE6-EC03-4942-84C3-297AFD59F45A", "Job Type");
			this.JobTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(90, 190, true);
			this.JobTypeDropEdit.Name = "JobTypeDropEdit";
			this.JobTypeDropEdit.PreBoundMaxLength = 3;
			this.JobTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 18, true);
			this.JobTypeDropEdit.TabIndex = 2;
			// 
			// ModeDropEdit
			// 
			this.ModeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ModeDropEdit, "CompanyData+RatingDocRollupOrGroups.RCG_TransportMode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.Rating.RatingDocumentsChargeGroupingOrRollup)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.RatingDocRollupOrGroups)).SyncRoot)).RCG_TransportMode)));
			this.ModeDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("2E08C315-9D05-457F-B533-766CA9007C98", "Mode");
			this.ModeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(90, 220, true);
			this.ModeDropEdit.Name = "ModeDropEdit";
			this.ModeDropEdit.PreBoundMaxLength = 3;
			this.ModeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 18, true);
			this.ModeDropEdit.TabIndex = 3;
			// 
			// DisplayDropEdit
			// 
			this.DisplayDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DisplayDropEdit, "CompanyData+RatingDocRollupOrGroups.RCG_Display");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.Rating.RatingDocumentsChargeGroupingOrRollup)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.RatingDocRollupOrGroups)).SyncRoot)).RCG_Display)));
			this.DisplayDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("B9FDDD21-1C7B-4D6D-8E8D-9C702288B3A7", "Display");
			this.DisplayDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(90, 250, true);
			this.DisplayDropEdit.Name = "DisplayDropEdit";
			this.DisplayDropEdit.PreBoundMaxLength = 3;
			this.DisplayDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 18, true);
			this.DisplayDropEdit.TabIndex = 4;
			// 
			// StyleDropEdit
			// 
			this.StyleDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.StyleDropEdit, "CompanyData+RatingDocRollupOrGroups.RCG_Style");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.Rating.RatingDocumentsChargeGroupingOrRollup)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.RatingDocRollupOrGroups)).SyncRoot)).RCG_Style)));
			this.StyleDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("B580FE79-AD4B-48AC-9148-F82592DB0B6D", "Style");
			this.StyleDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(90, 280, true);
			this.StyleDropEdit.Name = "StyleDropEdit";
			this.StyleDropEdit.PreBoundMaxLength = 3;
			this.StyleDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 18, true);
			this.StyleDropEdit.TabIndex = 5;
			// 
			// GroupChargesGrid
			// 
			this.GroupChargesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.GroupChargesGrid, "CompanyData+RatingDocRollupOrGroups");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.RatingDocRollupOrGroups)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.Rating.RatingDocumentsChargeGroupingOrRollup)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.RatingDocRollupOrGroups)).SyncRoot)).RCG_Module)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.Rating.RatingDocumentsChargeGroupingOrRollup)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.RatingDocRollupOrGroups)).SyncRoot)).RCG_JobType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.Rating.RatingDocumentsChargeGroupingOrRollup)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.RatingDocRollupOrGroups)).SyncRoot)).RCG_TransportMode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.Rating.RatingDocumentsChargeGroupingOrRollup)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.RatingDocRollupOrGroups)).SyncRoot)).RCG_Display)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.Rating.RatingDocumentsChargeGroupingOrRollup)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.RatingDocRollupOrGroups)).SyncRoot)).RCG_Style)));
			this.GroupChargesGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.ColumnName = "RCG_Module";
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("0013F783-092F-4EA7-BCE6-E1CB3AC1BFC9", "Module");
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDropEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo2.ColumnName = "RCG_JobType";
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("D3E2B9C3-8A66-4549-B020-B7DDCA8CC698", "Job Type");
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDropEditColumnStyleInfo2.DefaultCollectionIndex = 1;
			zDropEditColumnStyleInfo3.ColumnName = "RCG_TransportMode";
			zDropEditColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("4FE6EAC9-B96D-4277-9FA2-EF8B3CCB2546", "Mode");
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDropEditColumnStyleInfo3.DefaultCollectionIndex = 2;
			zDropEditColumnStyleInfo4.ColumnName = "RCG_Display";
			zDropEditColumnStyleInfo4.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("BEAD84B2-60DF-4CB3-887E-B465CE8525D5", "Display");
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDropEditColumnStyleInfo4.DefaultCollectionIndex = 3;
			zDropEditColumnStyleInfo5.ColumnName = "RCG_Style";
			zDropEditColumnStyleInfo5.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("27A50471-68F5-446C-9B49-9CEDDAD3A0AE", "Style");
			zDropEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDropEditColumnStyleInfo5.DefaultCollectionIndex = 4;
			this.GroupChargesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.GroupChargesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.GroupChargesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.GroupChargesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.GroupChargesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo5);
			this.GroupChargesGrid.Dock = System.Windows.Forms.DockStyle.Top;
			this.GroupChargesGrid.GridId = "338332EC-C5D2-4E41-B7AE-758B95208F10";
			this.GroupChargesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.GroupChargesGrid.LayoutKey = "GroupChargesGrid";
			this.GroupChargesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 14, true);
			this.GroupChargesGrid.Name = "GroupChargesGrid";
			this.GroupChargesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(618, 125, true);
			this.GroupChargesGrid.TabIndex = 0;
			//
			// PricingUserControl
			//
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.PricingDetailsGroupBox);
			this.Name = "PricingUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(890, 463, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PricingDetailsGroupBox.ResumeLayout(false);
			this.PricingDetailsGroupBox.PerformLayout();
			this.GroupChargesBox.ResumeLayout(false);
			this.GroupChargesBox.PerformLayout();
			this.ModuleDropEdit.ResumeLayout(false);
			this.ModuleDropEdit.PerformLayout();
			this.JobTypeDropEdit.ResumeLayout(false);
			this.JobTypeDropEdit.PerformLayout();
			this.ModeDropEdit.ResumeLayout(false);
			this.ModeDropEdit.PerformLayout();
			this.DisplayDropEdit.ResumeLayout(false);
			this.DisplayDropEdit.PerformLayout();
			this.StyleDropEdit.ResumeLayout(false);
			this.StyleDropEdit.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.GroupChargesGrid)).EndInit();
			this.GroupChargesGrid.ResumeLayout(false);
			this.GroupChargesGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		ZGroupBox PricingDetailsGroupBox;
		ZGroupBox GroupChargesBox;
		ZDropEdit ModuleDropEdit;
		ZDropEdit JobTypeDropEdit;
		ZDropEdit ModeDropEdit;
		ZDropEdit DisplayDropEdit;
		ZDropEdit StyleDropEdit;
		ZGrid GroupChargesGrid;
	}
}
