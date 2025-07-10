namespace Enterprise.MasterFiles.GUI
{
	partial class OpportunityStageProgressControl
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
			Enterprise.ZArchitecture.ZDateTimeOffsetEditColumnStyleInfo zDateTimeOffsetEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateTimeOffsetEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateTimeOffsetEditColumnStyleInfo zDateTimeOffsetEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateTimeOffsetEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.ProgressGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ProgressGrid)).BeginInit();
			this.ProgressGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgOpportunity);
			// 
			// ProgressGrid
			// 
			this.ProgressGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ProgressGrid, "StageProgressCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgOpportunity)(null)).StageProgressCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgOpportunityStageProgress)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgOpportunity)(null)).StageProgressCollection)).SyncRoot)).OSP_Stage)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgOpportunityStageProgress)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgOpportunity)(null)).StageProgressCollection)).SyncRoot)).OSP_StageDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTimeOffset)(((Enterprise.MasterFiles.Business.OrgOpportunityStageProgress)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgOpportunity)(null)).StageProgressCollection)).SyncRoot)).OSP_DateStarted)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTimeOffset)(((Enterprise.MasterFiles.Business.OrgOpportunityStageProgress)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgOpportunity)(null)).StageProgressCollection)).SyncRoot)).OSP_DateCompleted)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgOpportunityStageProgress)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgOpportunity)(null)).StageProgressCollection)).SyncRoot)).DaysInStage)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgOpportunityStageProgress)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgOpportunity)(null)).StageProgressCollection)).SyncRoot)).OSP_SystemLastEditUser)));
			this.ProgressGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("e2d415f4-9112-42c4-8b2c-e4d21202cdd0", "Stage");
			zTextBoxColumnStyleInfo1.ColumnName = "OSP_Stage";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("3e867f17-5a9c-4c74-90a1-86c6ea661db5", "Stage Description");
			zTextBoxColumnStyleInfo2.ColumnName = "OSP_StageDescription";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zDateTimeOffsetEditColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("24292608-82b6-4a62-ad1f-6433090e1a80", "Date Started");
			zDateTimeOffsetEditColumnStyleInfo1.ColumnName = "OSP_DateStarted";
			zDateTimeOffsetEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDateTimeOffsetEditColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ffe1e0f8-1de8-4fc0-bf94-87d13e5e4ba7", "Date Completed");
			zDateTimeOffsetEditColumnStyleInfo2.ColumnName = "OSP_DateCompleted";
			zDateTimeOffsetEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("538c18bd-98e5-4d1e-998d-8478eb23a4b8", "Days In Stage");
			zCalcEditColumnStyleInfo1.ColumnName = "DaysInStage";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("a759cb28-48e0-4f81-8b0a-a0cdbfead277", "Last Edit User");
			zTextBoxColumnStyleInfo3.ColumnName = "OSP_SystemLastEditUser";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.ProgressGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ProgressGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ProgressGrid.ColumnStyles.Add(zDateTimeOffsetEditColumnStyleInfo1);
			this.ProgressGrid.ColumnStyles.Add(zDateTimeOffsetEditColumnStyleInfo2);
			this.ProgressGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.ProgressGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.ProgressGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ProgressGrid.GridId = "ed3160d8-0dbc-466a-8beb-621a9db0cb0b";
			this.ProgressGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ProgressGrid.LayoutKey = "ProductsGrid";
			this.ProgressGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ProgressGrid.Name = "ProgressGrid";
			this.ProgressGrid.ReadOnly = true;
			this.ProgressGrid.RowHeadersVisible = false;
			this.ProgressGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(818, 309, true);
			this.ProgressGrid.TabIndex = 1;
			// 
			// OpportunityStageProgressControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.ProgressGrid);
			this.Name = "OpportunityStageProgressControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(818, 309, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ProgressGrid)).EndInit();
			this.ProgressGrid.ResumeLayout(false);
			this.ProgressGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZGrid ProgressGrid;
	}
}
