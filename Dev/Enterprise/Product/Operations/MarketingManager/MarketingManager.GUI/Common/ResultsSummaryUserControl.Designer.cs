namespace Enterprise.MarketingManager.GUI
{
	partial class ResultsSummaryUserControl
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
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo13 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo14 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo15 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo16 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo17 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo18 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo19 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo20 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo21 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo22 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo23 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo24 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo25 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo26 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo27 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo28 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.ResultsSummarySplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.SummaryGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ResultsSummarySplitContainer.Panel1.SuspendLayout();
			this.ResultsSummarySplitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SummaryGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MarketingManager.Business.GlbCompanyCampaign);
			// 
			// ResultsSummarySplitContainer
			// 
			this.ResultsSummarySplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ResultsSummarySplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ResultsSummarySplitContainer.Name = "ResultsSummarySplitContainer";
			this.ResultsSummarySplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// ResultsSummarySplitContainer.Panel1
			// 
			this.ResultsSummarySplitContainer.Panel1.Controls.Add(this.SummaryGrid);
			this.ResultsSummarySplitContainer.Panel2Collapsed = true;
			this.ResultsSummarySplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(648, 426, true);
			this.ResultsSummarySplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(228);
			this.ResultsSummarySplitContainer.TabIndex = 1;
			// 
			// SummaryGrid
			// 
			this.SummaryGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.SummaryGrid, "VoteExamSurveySummaries");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).VoteExamSurveySummaries)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MarketingManager.Business.VoteExamSurveySummary)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).VoteExamSurveySummaries)).SyncRoot)).Number)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.VoteExamSurveySummary)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).VoteExamSurveySummaries)).SyncRoot)).QuestionText)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.VoteExamSurveySummary)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).VoteExamSurveySummaries)).SyncRoot)).NumberOfRecipientAsText)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.VoteExamSurveySummary)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).VoteExamSurveySummaries)).SyncRoot)).NumberOfRecipientRepliedAsText)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.VoteExamSurveySummary)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).VoteExamSurveySummaries)).SyncRoot)).RecipientAnsweredAsText)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.VoteExamSurveySummary)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).VoteExamSurveySummaries)).SyncRoot)).RecipientSkippedAsText)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.VoteExamSurveySummary)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).VoteExamSurveySummaries)).SyncRoot)).AverageAsText)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.VoteExamSurveySummary)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).VoteExamSurveySummaries)).SyncRoot)).AverageAsPercentageAsText)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.VoteExamSurveySummary)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).VoteExamSurveySummaries)).SyncRoot)).NumberOfAnswersAsText)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.VoteExamSurveySummary)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).VoteExamSurveySummaries)).SyncRoot)).Answer1)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.VoteExamSurveySummary)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).VoteExamSurveySummaries)).SyncRoot)).Answer1CountAsText)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.VoteExamSurveySummary)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).VoteExamSurveySummaries)).SyncRoot)).Answer2)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.VoteExamSurveySummary)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).VoteExamSurveySummaries)).SyncRoot)).Answer2CountAsText)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.VoteExamSurveySummary)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).VoteExamSurveySummaries)).SyncRoot)).Answer3)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.VoteExamSurveySummary)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).VoteExamSurveySummaries)).SyncRoot)).Answer3CountAsText)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.VoteExamSurveySummary)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).VoteExamSurveySummaries)).SyncRoot)).Answer4)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.VoteExamSurveySummary)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).VoteExamSurveySummaries)).SyncRoot)).Answer4CountAsText)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.VoteExamSurveySummary)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).VoteExamSurveySummaries)).SyncRoot)).Answer5)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.VoteExamSurveySummary)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).VoteExamSurveySummaries)).SyncRoot)).Answer5CountAsText)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.VoteExamSurveySummary)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).VoteExamSurveySummaries)).SyncRoot)).Answer6)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.VoteExamSurveySummary)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).VoteExamSurveySummaries)).SyncRoot)).Answer6CountAsText)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.VoteExamSurveySummary)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).VoteExamSurveySummaries)).SyncRoot)).Answer7)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.VoteExamSurveySummary)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).VoteExamSurveySummaries)).SyncRoot)).Answer7CountAsText)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.VoteExamSurveySummary)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).VoteExamSurveySummaries)).SyncRoot)).Answer8)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.VoteExamSurveySummary)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).VoteExamSurveySummaries)).SyncRoot)).Answer8CountAsText)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.VoteExamSurveySummary)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).VoteExamSurveySummaries)).SyncRoot)).Answer9)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.VoteExamSurveySummary)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).VoteExamSurveySummaries)).SyncRoot)).Answer9CountAsText)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.VoteExamSurveySummary)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).VoteExamSurveySummaries)).SyncRoot)).Answer10)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.VoteExamSurveySummary)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).VoteExamSurveySummaries)).SyncRoot)).Answer10CountAsText)));
			this.SummaryGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "Number";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo1.ColumnName = "QuestionText";
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("ResultsSummaryUserControl|6425bf63-f0f5-45e1-b0a2-fa20f550bd2c", "Recipients");
			zTextBoxColumnStyleInfo2.ColumnName = "NumberOfRecipientAsText";
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("ResultsSummaryUserControl|df79e3c0-937e-4907-b3c3-b7f0a9ca9e44", "Replied");
			zTextBoxColumnStyleInfo3.ColumnName = "NumberOfRecipientRepliedAsText";
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("ResultsSummaryUserControl|4687e9bc-22e3-4953-b968-c97bde99415e", "Answered");
			zTextBoxColumnStyleInfo4.ColumnName = "RecipientAnsweredAsText";
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("ResultsSummaryUserControl|c55e386d-ca5a-48f3-b92c-3c99b760ccc3", "Skipped");
			zTextBoxColumnStyleInfo5.ColumnName = "RecipientSkippedAsText";
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("ResultsSummaryUserControl|b04d0617-8fb5-4cd7-bcd1-da398f74b846", "Average");
			zTextBoxColumnStyleInfo6.ColumnName = "AverageAsText";
			zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("ResultsSummaryUserControl|476f9c13-7951-40bc-80a2-001f80216008", "Average %");
			zTextBoxColumnStyleInfo7.ColumnName = "AverageAsPercentageAsText";
			zTextBoxColumnStyleInfo8.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("ResultsSummaryUserControl|ac8f0267-fc46-4118-810a-71ddb907adec", "# Options");
			zTextBoxColumnStyleInfo8.ColumnName = "NumberOfAnswersAsText";
			zTextBoxColumnStyleInfo9.ColumnName = "Answer1";
			zTextBoxColumnStyleInfo10.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("ResultsSummaryUserControl|cee6b663-84e3-4805-98b6-e2e9446be0ca", "1#");
			zTextBoxColumnStyleInfo10.ColumnName = "Answer1CountAsText";
			zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			zTextBoxColumnStyleInfo11.ColumnName = "Answer2";
			zTextBoxColumnStyleInfo12.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("ResultsSummaryUserControl|dce7b7ca-538e-43c6-af3c-1b2f4fb7e443", "2#");
			zTextBoxColumnStyleInfo12.ColumnName = "Answer2CountAsText";
			zTextBoxColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			zTextBoxColumnStyleInfo13.ColumnName = "Answer3";
			zTextBoxColumnStyleInfo14.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("ResultsSummaryUserControl|1f333da9-dd13-4d3d-8f8b-98dc2c9836e3", "3#");
			zTextBoxColumnStyleInfo14.ColumnName = "Answer3CountAsText";
			zTextBoxColumnStyleInfo14.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			zTextBoxColumnStyleInfo15.ColumnName = "Answer4";
			zTextBoxColumnStyleInfo16.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("ResultsSummaryUserControl|817f06a5-d22f-4f00-9943-3c993679276a", "4#");
			zTextBoxColumnStyleInfo16.ColumnName = "Answer4CountAsText";
			zTextBoxColumnStyleInfo16.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			zTextBoxColumnStyleInfo17.ColumnName = "Answer5";
			zTextBoxColumnStyleInfo18.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("ResultsSummaryUserControl|a2e72c06-5a13-48df-8b9b-c8e408650bdd", "5#");
			zTextBoxColumnStyleInfo18.ColumnName = "Answer5CountAsText";
			zTextBoxColumnStyleInfo18.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			zTextBoxColumnStyleInfo19.ColumnName = "Answer6";
			zTextBoxColumnStyleInfo20.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("ResultsSummaryUserControl|8a436f07-7ebb-4a6c-8e13-a1e72d02ee13", "6#");
			zTextBoxColumnStyleInfo20.ColumnName = "Answer6CountAsText";
			zTextBoxColumnStyleInfo20.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			zTextBoxColumnStyleInfo21.ColumnName = "Answer7";
			zTextBoxColumnStyleInfo22.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("ResultsSummaryUserControl|95915b58-f419-41a2-b1f1-7c857330765f", "7#");
			zTextBoxColumnStyleInfo22.ColumnName = "Answer7CountAsText";
			zTextBoxColumnStyleInfo22.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			zTextBoxColumnStyleInfo23.ColumnName = "Answer8";
			zTextBoxColumnStyleInfo24.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("ResultsSummaryUserControl|d8746fb5-423e-448d-b420-3f8650fb2216", "8#");
			zTextBoxColumnStyleInfo24.ColumnName = "Answer8CountAsText";
			zTextBoxColumnStyleInfo24.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			zTextBoxColumnStyleInfo25.ColumnName = "Answer9";
			zTextBoxColumnStyleInfo26.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("ResultsSummaryUserControl|90ece58b-b2c2-4051-89ee-7594243dfe98", "9#");
			zTextBoxColumnStyleInfo26.ColumnName = "Answer9CountAsText";
			zTextBoxColumnStyleInfo26.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			zTextBoxColumnStyleInfo27.ColumnName = "Answer10";
			zTextBoxColumnStyleInfo28.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("ResultsSummaryUserControl|4b6ce8f7-c20b-4553-a104-63fe7e784d4b", "10#");
			zTextBoxColumnStyleInfo28.ColumnName = "Answer10CountAsText";
			zTextBoxColumnStyleInfo28.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			this.SummaryGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.SummaryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.SummaryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.SummaryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.SummaryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.SummaryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.SummaryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.SummaryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.SummaryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.SummaryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.SummaryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.SummaryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.SummaryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
			this.SummaryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo13);
			this.SummaryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo14);
			this.SummaryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo15);
			this.SummaryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo16);
			this.SummaryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo17);
			this.SummaryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo18);
			this.SummaryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo19);
			this.SummaryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo20);
			this.SummaryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo21);
			this.SummaryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo22);
			this.SummaryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo23);
			this.SummaryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo24);
			this.SummaryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo25);
			this.SummaryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo26);
			this.SummaryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo27);
			this.SummaryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo28);
			this.SummaryGrid.GridId = "2cb7eb93-e473-48da-8c06-a221eabd23c2";
			this.SummaryGrid.CopySelectedRowsAllowed = true;
			this.SummaryGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SummaryGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.SummaryGrid.IsWholeRowSelectedOnClick = true;
			this.SummaryGrid.LayoutKey = "zGrid1";
			this.SummaryGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SummaryGrid.Name = "SummaryGrid";
			this.SummaryGrid.ReadOnly = true;
			this.SummaryGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.SummaryGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(648, 426, true);
			this.SummaryGrid.TabIndex = 3;
			// 
			// ResultsSummaryUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ResultsSummarySplitContainer);
			this.Name = "ResultsSummaryUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(648, 426, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResultsSummarySplitContainer.Panel1.ResumeLayout(false);
			this.ResultsSummarySplitContainer.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.SummaryGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		protected CargoWise.Windows.UI.KSplitContainer ResultsSummarySplitContainer;
		protected Enterprise.ZArchitecture.ZGrid SummaryGrid;
	}
}
