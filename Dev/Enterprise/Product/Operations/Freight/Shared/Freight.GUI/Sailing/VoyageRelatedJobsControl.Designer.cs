namespace Enterprise.Freight.GUI
{
	partial class VoyageRelatedJobsControl
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.RelatedJobsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.OpenJobButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.RelatedJobsGrid)).BeginInit();
			this.RelatedJobsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Business.JobVoyage);
			// 
			// RelatedJobsGrid
			// 
			this.RelatedJobsGrid.AllowNavigation = false;
			this.RelatedJobsGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.RelatedJobsGrid, "RelatedJobs");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.Business.JobVoyage)(null)).RelatedJobs)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.VoyageRelatedJob)(((System.Collections.IList)(((Enterprise.Freight.Business.JobVoyage)(null)).RelatedJobs)).SyncRoot)).VJV_JobNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.VoyageRelatedJob)(((System.Collections.IList)(((Enterprise.Freight.Business.JobVoyage)(null)).RelatedJobs)).SyncRoot)).VJV_JobTypeDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.Business.VoyageRelatedJob)(((System.Collections.IList)(((Enterprise.Freight.Business.JobVoyage)(null)).RelatedJobs)).SyncRoot)).VJV_GC)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.Business.VoyageRelatedJob)(((System.Collections.IList)(((Enterprise.Freight.Business.JobVoyage)(null)).RelatedJobs)).SyncRoot)).Lookups.Companies)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Business.VoyageRelatedJob)(((System.Collections.IList)(((Enterprise.Freight.Business.JobVoyage)(null)).RelatedJobs)).SyncRoot)).VJV_IsCancelled)));
			this.RelatedJobsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "VJV_JobNumber";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo2.ColumnName = "VJV_JobTypeDescription";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zGuidFindBoxColumnStyleInfo1.BindToList = "Lookups.Companies";
			zGuidFindBoxColumnStyleInfo1.ColumnName = "VJV_GC";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCheckBoxColumnStyleInfo1.ColumnName = "VJV_IsCancelled";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.RelatedJobsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.RelatedJobsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.RelatedJobsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.RelatedJobsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.RelatedJobsGrid.GridId = "5439f38f-1a27-469c-8b12-9e5240439f07";
			this.RelatedJobsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.RelatedJobsGrid.IsWholeRowSelectedOnClick = true;
			this.RelatedJobsGrid.LayoutKey = "relatedJobsGrid1";
			this.RelatedJobsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.RelatedJobsGrid.Name = "RelatedJobsGrid";
			this.RelatedJobsGrid.ReadOnly = true;
			this.RelatedJobsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(682, 378, true);
			this.RelatedJobsGrid.TabIndex = 1;
			this.RelatedJobsGrid.DoubleClick += new System.EventHandler(this.RelatedJobsGrid_DoubleClick);
			// 
			// OpenJobButton
			// 
			this.OpenJobButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OpenJobButton.IsCaptionOverridden = true;
			this.OpenJobButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(580, 387, true);
			this.OpenJobButton.Name = "OpenJobButton";
			this.OpenJobButton.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("41671c96-b292-4d42-9d0d-637ac1c87d07", "Open Job");
			this.OpenJobButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.OpenJobButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(95, 23, true);
			this.OpenJobButton.TabIndex = 2;
			this.OpenJobButton.ToolTipCaption = null;
			this.OpenJobButton.UseVisualStyleBackColor = true;
			this.OpenJobButton.Text = Enterprise.Freight.GUI.Res.GetString("afb5a4a2-583d-49d7-a136-150c5bbab311", "Open Job");
			this.OpenJobButton.Click += new System.EventHandler(this.OpenJobButton_Click);
			// 
			// SailingRelatedJobsControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.OpenJobButton);
			this.Controls.Add(this.RelatedJobsGrid);
			this.Name = "SailingRelatedJobsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(688, 413, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.RelatedJobsGrid)).EndInit();
			this.RelatedJobsGrid.ResumeLayout(false);
			this.RelatedJobsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.ZGrid RelatedJobsGrid;
		internal ZArchitecture.GUI.ZButton OpenJobButton;
	}
}
