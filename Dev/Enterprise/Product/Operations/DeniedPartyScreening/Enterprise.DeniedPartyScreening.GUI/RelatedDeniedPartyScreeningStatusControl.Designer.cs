namespace Enterprise.DeniedPartyScreening.GUI
{
	partial class RelatedDeniedPartyScreeningStatusControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfo1 = new Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo();
			Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfo2 = new Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo();
			Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfo3 = new Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfo4 = new Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo();
			Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfo5 = new Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo();
			Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfo6 = new Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.MatchingResultsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.splitContainer1 = new CargoWise.Windows.UI.KSplitContainer();
			this.LogsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.LogsGrid)).BeginInit();
			this.LogsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterData.Common.IRelatedOrgPartyScreeningStatusCollection);
			// 
			// MatchingResultsTextBox
			// 
			this.MatchingResultsTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.MatchingResultsTextBox, "PJ_MatchingData");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterData.Common.IRelatedOrgPartyScreeningStatus)(null)).PJ_MatchingData)));
			this.MatchingResultsTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.MatchingResultsTextBox, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.MatchingResultsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 29, true);
			this.MatchingResultsTextBox.Multiline = true;
			this.MatchingResultsTextBox.Name = "MatchingResultsTextBox";
			this.MatchingResultsTextBox.ReadOnly = true;
			this.MatchingResultsTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.MatchingResultsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(673, 132, true);
			this.MatchingResultsTextBox.TabIndex = 1;
			// 
			// splitContainer1
			// 
			this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.splitContainer1.Name = "splitContainer1";
			this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.LogsGrid);
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.MatchingResultsTextBox);
			this.splitContainer1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(679, 347, true);
			this.splitContainer1.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(179);
			this.splitContainer1.TabIndex = 6;
			// 
			// LogsGrid
			// 
			this.LogsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.LogsGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterData.Common.IRelatedOrgPartyScreeningStatus)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterData.Common.IRelatedOrgPartyScreeningStatus)(null)).PJ_ScreenDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterData.Common.IRelatedOrgPartyScreeningStatus)(null)).PJ_SystemCreateTimeUtc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterData.Common.IRelatedOrgPartyScreeningStatus)(null)).ScreenedByFullName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterData.Common.IRelatedOrgPartyScreeningStatus)(null)).PJ_Status)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterData.Common.IRelatedOrgPartyScreeningStatus)(null)).StatusDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterData.Common.IRelatedOrgPartyScreeningStatus)(null)).PJ_MatchingData)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterData.Common.IRelatedOrgPartyScreeningStatus)(null)).PJ_HighConfidenceResults)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterData.Common.IRelatedOrgPartyScreeningStatus)(null)).PJ_MediumConfidenceResults)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZInt)(((Enterprise.MasterData.Common.IRelatedOrgPartyScreeningStatus)(null)).PJ_LowConfidenceResultsCount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterData.Common.IRelatedOrgPartyScreeningStatus)(null)).RelatedOrganization)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterData.Common.IRelatedOrgPartyScreeningStatus)(null)).PJ_IncludedLists)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterData.Common.IRelatedOrgPartyScreeningStatus)(null)).PJ_ExcludedLists)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterData.Common.IRelatedOrgPartyScreeningStatus)(null)).PJ_ClearedReason)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterData.Common.IRelatedOrgPartyScreeningStatus)(null)).SourceInformation)));
			this.LogsGrid.CaptionVisible = false;
			zDateEditColumnStyleInfo1.ColumnName = "PJ_ScreenDate";
			zDateEditColumnStyleInfo1.IsReadOnly = true;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDateEditColumnStyleInfo2.ColumnName = "PJ_SystemCreateTimeUtc";
			zDateEditColumnStyleInfo2.IsReadOnly = true;
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.DeniedPartyScreening.GUI.Res.GetData("RelatedDeniedPartyScreeningStatusControl|100c8e00-d49d-4a85-9c10-1649a6cdc9c0", "Screened By");
			zTextBoxColumnStyleInfo1.ColumnName = "ScreenedByFullName";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo2.ColumnName = "PJ_Status";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.IsVisible = false;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.DeniedPartyScreening.GUI.Res.GetData("RelatedDeniedPartyScreeningStatusControl|26105f75-be3d-4718-87b4-c8b7362b09c7", "Status Desc.");
			zTextBoxColumnStyleInfo3.ColumnName = "StatusDescription";
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			zMultiLineTextBoxColumnInfo1.ColumnName = "PJ_MatchingData";
			zMultiLineTextBoxColumnInfo1.IsReadOnly = true;
			zMultiLineTextBoxColumnInfo1.MinimumEditControlWidth = 500;
			zMultiLineTextBoxColumnInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(500);
			zMultiLineTextBoxColumnInfo2.CaptionResourceString = Enterprise.DeniedPartyScreening.GUI.Res.GetData("98e765e2-2bbc-466b-b4e8-719bbd0f3ef4", "High Confidence Suggested", "Suggested High Confidence Matches", "");
			zMultiLineTextBoxColumnInfo2.ColumnName = "PJ_HighConfidenceResults";
			zMultiLineTextBoxColumnInfo2.IsReadOnly = true;
			zMultiLineTextBoxColumnInfo2.MinimumEditControlWidth = 500;
			zMultiLineTextBoxColumnInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			zMultiLineTextBoxColumnInfo3.CaptionResourceString = Enterprise.DeniedPartyScreening.GUI.Res.GetData("b26ab7e5-b434-132c-8b1f-fa61e416ad6d", "Medium Confidence Suggested", "Suggested Medium Confidence Matches", "");
			zMultiLineTextBoxColumnInfo3.ColumnName = "PJ_MediumConfidenceResults";
			zMultiLineTextBoxColumnInfo3.IsReadOnly = true;
			zMultiLineTextBoxColumnInfo3.MinimumEditControlWidth = 500;
			zMultiLineTextBoxColumnInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.DeniedPartyScreening.GUI.Res.GetData("265671ad-aaa0-5ce9-93e8-db7e24851ee0", "Low Confidence Suggested", "Number of Suggested Low Confidence Matches", "");
			zTextBoxColumnStyleInfo4.ColumnName = "PJ_LowConfidenceResultsCount";
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.IsVisible = false;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.DeniedPartyScreening.GUI.Res.GetData("RelatedDeniedPartyScreeningStatusControl|36105f75-be3d-4718-87b1-c8b7362b09c7", "Related Party");
			zTextBoxColumnStyleInfo5.ColumnName = "RelatedOrganization";
			zTextBoxColumnStyleInfo5.IsReadOnly = true;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			zMultiLineTextBoxColumnInfo4.CaptionResourceString = Enterprise.DeniedPartyScreening.GUI.Res.GetData("b6fce846-16d8-4fa4-8983-0da06cdab785", "Included Lists");
			zMultiLineTextBoxColumnInfo4.ColumnName = "PJ_IncludedLists";
			zMultiLineTextBoxColumnInfo4.IsReadOnly = true;
			zMultiLineTextBoxColumnInfo4.MinimumEditControlWidth = 500;
			zMultiLineTextBoxColumnInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			zMultiLineTextBoxColumnInfo5.CaptionResourceString = Enterprise.DeniedPartyScreening.GUI.Res.GetData("a4f609d7-83f7-4d90-a8ca-cfe721456627", "Excluded Lists");
			zMultiLineTextBoxColumnInfo5.ColumnName = "PJ_ExcludedLists";
			zMultiLineTextBoxColumnInfo5.IsReadOnly = true;
			zMultiLineTextBoxColumnInfo5.MinimumEditControlWidth = 500;
			zMultiLineTextBoxColumnInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			zMultiLineTextBoxColumnInfo6.ColumnName = "PJ_ClearedReason";
			zMultiLineTextBoxColumnInfo6.IsReadOnly = true;
			zMultiLineTextBoxColumnInfo6.IsVisible = false;
			zMultiLineTextBoxColumnInfo6.MinimumEditControlWidth = 300;
			zMultiLineTextBoxColumnInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(114);
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.DeniedPartyScreening.GUI.Res.GetData("8f837a4f-53a7-4328-b4f4-6c59ace7cb30", "Source");
			zTextBoxColumnStyleInfo6.ColumnName = "SourceInformation";
			zTextBoxColumnStyleInfo6.IsReadOnly = true;
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			this.LogsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.LogsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.LogsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.LogsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.LogsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.LogsGrid.ColumnStyles.Add(zMultiLineTextBoxColumnInfo1);
			this.LogsGrid.ColumnStyles.Add(zMultiLineTextBoxColumnInfo2);
			this.LogsGrid.ColumnStyles.Add(zMultiLineTextBoxColumnInfo3);
			this.LogsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.LogsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.LogsGrid.ColumnStyles.Add(zMultiLineTextBoxColumnInfo4);
			this.LogsGrid.ColumnStyles.Add(zMultiLineTextBoxColumnInfo5);
			this.LogsGrid.ColumnStyles.Add(zMultiLineTextBoxColumnInfo6);
			this.LogsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.LogsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LogsGrid.GridId = "c31d2cc6-ac2c-4913-af0f-b9af4c7a282f";
			this.LogsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.LogsGrid.LayoutKey = "zGrid1";
			this.LogsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.LogsGrid.Name = "LogsGrid";
			this.LogsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(679, 179, true);
			this.LogsGrid.TabIndex = 6;
			// 
			// RelatedDeniedPartyScreeningStatusControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.splitContainer1);
			this.Name = "RelatedDeniedPartyScreeningStatusControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(679, 347, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel2.ResumeLayout(false);
			this.splitContainer1.Panel2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
			this.splitContainer1.ResumeLayout(false);
			this.splitContainer1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.LogsGrid)).EndInit();
			this.LogsGrid.ResumeLayout(false);
			this.LogsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.ZTextBox MatchingResultsTextBox;
		private CargoWise.Windows.UI.KSplitContainer splitContainer1;
		internal Enterprise.ZArchitecture.ZGrid LogsGrid;
	}
}
