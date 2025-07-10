using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	partial class CompetitorDetailsUserControl
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
			this.CompetitorGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.OM_CICompetitiveRankingDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.OM_CICompetitorCategoryBoundDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.OM_CIEstimatedStaffThisCountryBoundCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.OM_CIEstimatedStaffThisLocationBoundCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.OM_CISellingStyleBoundDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.OM_CITypeOfServiceBoundDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CIFincanialDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.OM_CICapitalEmployedBoundCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.OM_CIProfitBoundCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.OM_CITurnoverBoundCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.OM_CIFinancialDetailsApplicableFromDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.OM_CIFinancialDetailsApplicableToDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.FinancialDetailsApplicableFromDateLabel = new Enterprise.ZArchitecture.ZLabel();
			this.CISWOTGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.OM_CIThreatsBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.OM_CIOpportunitiesBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.OM_CIWeaknessesBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.OM_CIStrengthBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CompetitorGroupBox.SuspendLayout();
			this.CIFincanialDetailsGroupBox.SuspendLayout();
			this.CISWOTGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgHeader);
			// 
			// CompetitorGroupBox
			// 
			this.CompetitorGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CompetitorDetailsUserControl|e5a20918-c590-4901-bf87-d39544e577ca", "Competitor Details");
			this.CompetitorGroupBox.Controls.Add(this.OM_CICompetitiveRankingDropEdit);
			this.CompetitorGroupBox.Controls.Add(this.OM_CICompetitorCategoryBoundDropEdit);
			this.CompetitorGroupBox.Controls.Add(this.OM_CIEstimatedStaffThisCountryBoundCalcEdit);
			this.CompetitorGroupBox.Controls.Add(this.OM_CIEstimatedStaffThisLocationBoundCalcEdit);
			this.CompetitorGroupBox.Controls.Add(this.OM_CISellingStyleBoundDropEdit);
			this.CompetitorGroupBox.Controls.Add(this.OM_CITypeOfServiceBoundDropEdit);
			this.CompetitorGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CompetitorGroupBox.Name = "CompetitorGroupBox";
			this.CompetitorGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(504, 188, true);
			this.CompetitorGroupBox.TabIndex = 3;
			this.CompetitorGroupBox.TabStop = false;
			// 
			// OM_CITypeOfServiceBoundDropEdit
			// 
			this.BindingSource.SetBindingMember(this.OM_CITypeOfServiceBoundDropEdit, "MiscServ.OM_CITypeOfService");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_CITypeOfService)));
			this.OM_CITypeOfServiceBoundDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(184, 33, true);
			this.OM_CITypeOfServiceBoundDropEdit.Name = "OM_CITypeOfServiceBoundDropEdit";
			this.OM_CITypeOfServiceBoundDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(296, 20, true);
			this.OM_CITypeOfServiceBoundDropEdit.TabIndex = 1;
			// 
			// OM_CISellingStyleBoundDropEdit
			// 
			this.BindingSource.SetBindingMember(this.OM_CISellingStyleBoundDropEdit, "MiscServ.OM_CISellingStyle");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_CISellingStyle)));
			this.OM_CISellingStyleBoundDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(184, 57, true);
			this.OM_CISellingStyleBoundDropEdit.Name = "OM_CISellingStyleBoundDropEdit";
			this.OM_CISellingStyleBoundDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(296, 20, true);
			this.OM_CISellingStyleBoundDropEdit.TabIndex = 2;
			// 
			// OM_CICompetitorCategoryBoundDropEdit
			// 
			this.BindingSource.SetBindingMember(this.OM_CICompetitorCategoryBoundDropEdit, "MiscServ.OM_CICompetitorCategory");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_CICompetitorCategory)));
			this.OM_CICompetitorCategoryBoundDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(184, 81, true);
			this.OM_CICompetitorCategoryBoundDropEdit.Name = "OM_CICompetitorCategoryBoundDropEdit";
			this.OM_CICompetitorCategoryBoundDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(296, 20, true);
			this.OM_CICompetitorCategoryBoundDropEdit.TabIndex = 3;
			// 
			// OM_CICompetitiveRankingDropEdit
			// 
			this.BindingSource.SetBindingMember(this.OM_CICompetitiveRankingDropEdit, "MiscServ.OM_CICompetitiveRanking");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_CICompetitiveRanking)));
			this.OM_CICompetitiveRankingDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CompetitorDetailsUserControl|15b87ca4-ee28-4b7f-9e3d-0da0e499c603", "Competitive Ranking", "Competitive Ranking Text.");
			this.OM_CICompetitiveRankingDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(184, 105, true);
			this.OM_CICompetitiveRankingDropEdit.MaxItemsToShowInDropDown = 11;
			this.OM_CICompetitiveRankingDropEdit.Name = "OM_CICompetitiveRankingDropEdit";
			this.OM_CICompetitiveRankingDropEdit.PreBoundMaxLength = 2;
			this.OM_CICompetitiveRankingDropEdit.ShowDescriptionBox = false;
			this.OM_CICompetitiveRankingDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.OM_CICompetitiveRankingDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(43, 20, true);
			this.OM_CICompetitiveRankingDropEdit.TabIndex = 4;
			// 
			// OM_CIEstimatedStaffThisLocationBoundCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.OM_CIEstimatedStaffThisLocationBoundCalcEdit, "MiscServ.OM_CIEstimatedStaffThisLocation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_CIEstimatedStaffThisLocation)));
			this.OM_CIEstimatedStaffThisLocationBoundCalcEdit.Decimals = 0;
			this.OM_CIEstimatedStaffThisLocationBoundCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(184, 129, true);
			this.OM_CIEstimatedStaffThisLocationBoundCalcEdit.Name = "OM_CIEstimatedStaffThisLocationBoundCalcEdit";
			this.OM_CIEstimatedStaffThisLocationBoundCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 20, true);
			this.OM_CIEstimatedStaffThisLocationBoundCalcEdit.TabIndex = 5;
			this.OM_CIEstimatedStaffThisLocationBoundCalcEdit.Text = "0";
			this.OM_CIEstimatedStaffThisLocationBoundCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// OM_CIEstimatedStaffThisCountryBoundCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.OM_CIEstimatedStaffThisCountryBoundCalcEdit, "MiscServ.OM_CIEstimatedStaffThisCountry");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_CIEstimatedStaffThisCountry)));
			this.OM_CIEstimatedStaffThisCountryBoundCalcEdit.Decimals = 0;
			this.OM_CIEstimatedStaffThisCountryBoundCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(184, 153, true);
			this.OM_CIEstimatedStaffThisCountryBoundCalcEdit.Name = "OM_CIEstimatedStaffThisCountryBoundCalcEdit";
			this.OM_CIEstimatedStaffThisCountryBoundCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 20, true);
			this.OM_CIEstimatedStaffThisCountryBoundCalcEdit.TabIndex = 6;
			this.OM_CIEstimatedStaffThisCountryBoundCalcEdit.Text = "0";
			this.OM_CIEstimatedStaffThisCountryBoundCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// CIFincanialDetailsGroupBox
			// 
			this.CIFincanialDetailsGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CompetitorDetailsUserControl|6a5d0e01-222d-4641-9676-733f7b4633ce", "Financial Details");
			this.CIFincanialDetailsGroupBox.Controls.Add(this.OM_CICapitalEmployedBoundCalcEdit);
			this.CIFincanialDetailsGroupBox.Controls.Add(this.OM_CIProfitBoundCalcEdit);
			this.CIFincanialDetailsGroupBox.Controls.Add(this.OM_CITurnoverBoundCalcEdit);
			this.CIFincanialDetailsGroupBox.Controls.Add(this.OM_CIFinancialDetailsApplicableFromDateDateEdit);
			this.CIFincanialDetailsGroupBox.Controls.Add(this.OM_CIFinancialDetailsApplicableToDateDateEdit);
			this.CIFincanialDetailsGroupBox.Controls.Add(this.FinancialDetailsApplicableFromDateLabel);
			this.CIFincanialDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(510, 0, true);
			this.CIFincanialDetailsGroupBox.Name = "CIFincanialDetailsGroupBox";
			this.CIFincanialDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(330, 188, true);
			this.CIFincanialDetailsGroupBox.TabIndex = 4;
			this.CIFincanialDetailsGroupBox.TabStop = false;
			// 
			// OM_CICapitalEmployedBoundCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.OM_CICapitalEmployedBoundCalcEdit, "MiscServ.OM_CICapitalEmployed");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_CICapitalEmployed)));
			this.OM_CICapitalEmployedBoundCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(132, 66, true);
			this.OM_CICapitalEmployedBoundCalcEdit.Name = "OM_CICapitalEmployedBoundCalcEdit";
			this.OM_CICapitalEmployedBoundCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 20, true);
			this.OM_CICapitalEmployedBoundCalcEdit.TabIndex = 2;
			this.OM_CICapitalEmployedBoundCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// OM_CIProfitBoundCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.OM_CIProfitBoundCalcEdit, "MiscServ.OM_CIProfit");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_CIProfit)));
			this.OM_CIProfitBoundCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(132, 41, true);
			this.OM_CIProfitBoundCalcEdit.Name = "OM_CIProfitBoundCalcEdit";
			this.OM_CIProfitBoundCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 20, true);
			this.OM_CIProfitBoundCalcEdit.TabIndex = 1;
			this.OM_CIProfitBoundCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// OM_CITurnoverBoundCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.OM_CITurnoverBoundCalcEdit, "MiscServ.OM_CITurnover");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_CITurnover)));
			this.OM_CITurnoverBoundCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(132, 16, true);
			this.OM_CITurnoverBoundCalcEdit.Name = "OM_CITurnoverBoundCalcEdit";
			this.OM_CITurnoverBoundCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 20, true);
			this.OM_CITurnoverBoundCalcEdit.TabIndex = 0;
			this.OM_CITurnoverBoundCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// FinancialDetailsApplicableFromDateLabel
			//
			this.FinancialDetailsApplicableFromDateLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 89, true);
			this.FinancialDetailsApplicableFromDateLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("9EDA2925-A588-4CE6-BDF0-49D5749B622E", "Applicable From");
			this.FinancialDetailsApplicableFromDateLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.FinancialDetailsApplicableFromDateLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(125, 23, true);
			//
			// OM_CIFinancialDetailsApplicableFromDateDateEdit
			// 
			this.OM_CIFinancialDetailsApplicableFromDateDateEdit.AllowDrop = true;
			this.OM_CIFinancialDetailsApplicableFromDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.OM_CIFinancialDetailsApplicableFromDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.OM_CIFinancialDetailsApplicableFromDateDateEdit, "MiscServ.OM_CIFinancialDetailsApplicableFromDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDate)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_CIFinancialDetailsApplicableFromDate)));
			this.OM_CIFinancialDetailsApplicableFromDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(132, 91, true);
			this.OM_CIFinancialDetailsApplicableFromDateDateEdit.Name = "OM_CIFinancialDetailsApplicableFromDateDateEdit";
			this.OM_CIFinancialDetailsApplicableFromDateDateEdit.TabIndex = 3;
			// 
			// OM_CIFinancialDetailsApplicableToDateDateEdit
			// 
			this.OM_CIFinancialDetailsApplicableToDateDateEdit.AllowDrop = true;
			this.OM_CIFinancialDetailsApplicableToDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.OM_CIFinancialDetailsApplicableToDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.OM_CIFinancialDetailsApplicableToDateDateEdit, "MiscServ.OM_CIFinancialDetailsApplicableToDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDate)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_CIFinancialDetailsApplicableToDate)));
			this.OM_CIFinancialDetailsApplicableToDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(132, 116, true);
			this.OM_CIFinancialDetailsApplicableToDateDateEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("E9219317-8772-4996-8FFE-5A68AA9456C5", "Applicable To");
			this.OM_CIFinancialDetailsApplicableToDateDateEdit.Name = "OM_CIFinancialDetailsApplicableToDateDateEdit";
			this.OM_CIFinancialDetailsApplicableToDateDateEdit.TabIndex = 4;
			// 
			// CISWOTGroupBox
			// 
			this.CISWOTGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CompetitorDetailsUserControl|8a87f88b-9c9f-41e8-ac37-dbe81d1f61a5", "SWOT Analysis");
			this.CISWOTGroupBox.Controls.Add(this.OM_CIThreatsBoundTextBox);
			this.CISWOTGroupBox.Controls.Add(this.OM_CIOpportunitiesBoundTextBox);
			this.CISWOTGroupBox.Controls.Add(this.OM_CIWeaknessesBoundTextBox);
			this.CISWOTGroupBox.Controls.Add(this.OM_CIStrengthBoundTextBox);
			this.CISWOTGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 194, true);
			this.CISWOTGroupBox.Name = "CISWOTGroupBox";
			this.CISWOTGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(840, 324, true);
			this.CISWOTGroupBox.TabIndex = 5;
			this.CISWOTGroupBox.TabStop = false;
			// 
			// OM_CIThreatsBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.OM_CIThreatsBoundTextBox, "MiscServ.OM_CIThreats");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_CIThreats)));
			this.OM_CIThreatsBoundTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.OM_CIThreatsBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(504, 178, true);
			this.OM_CIThreatsBoundTextBox.Multiline = true;
			this.OM_CIThreatsBoundTextBox.Name = "OM_CIThreatsBoundTextBox";
			this.OM_CIThreatsBoundTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.OM_CIThreatsBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(312, 128, true);
			this.OM_CIThreatsBoundTextBox.TabIndex = 4;
			// 
			// OM_CIOpportunitiesBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.OM_CIOpportunitiesBoundTextBox, "MiscServ.OM_CIOpportunities");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_CIOpportunities)));
			this.OM_CIOpportunitiesBoundTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.OM_CIOpportunitiesBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(504, 34, true);
			this.OM_CIOpportunitiesBoundTextBox.Multiline = true;
			this.OM_CIOpportunitiesBoundTextBox.Name = "OM_CIOpportunitiesBoundTextBox";
			this.OM_CIOpportunitiesBoundTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.OM_CIOpportunitiesBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(312, 128, true);
			this.OM_CIOpportunitiesBoundTextBox.TabIndex = 3;
			// 
			// OM_CIWeaknessesBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.OM_CIWeaknessesBoundTextBox, "MiscServ.OM_CIWeaknesses");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_CIWeaknesses)));
			this.OM_CIWeaknessesBoundTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.OM_CIWeaknessesBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 178, true);
			this.OM_CIWeaknessesBoundTextBox.Multiline = true;
			this.OM_CIWeaknessesBoundTextBox.Name = "OM_CIWeaknessesBoundTextBox";
			this.OM_CIWeaknessesBoundTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.OM_CIWeaknessesBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(312, 128, true);
			this.OM_CIWeaknessesBoundTextBox.TabIndex = 2;
			// 
			// OM_CIStrengthBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.OM_CIStrengthBoundTextBox, "MiscServ.OM_CIStrength");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_CIStrength)));
			this.OM_CIStrengthBoundTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.OM_CIStrengthBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 34, true);
			this.OM_CIStrengthBoundTextBox.Multiline = true;
			this.OM_CIStrengthBoundTextBox.Name = "OM_CIStrengthBoundTextBox";
			this.OM_CIStrengthBoundTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.OM_CIStrengthBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(312, 128, true);
			this.OM_CIStrengthBoundTextBox.TabIndex = 1;
			// 
			// CompetitorDetailsUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.CompetitorGroupBox);
			this.Controls.Add(this.CIFincanialDetailsGroupBox);
			this.Controls.Add(this.CISWOTGroupBox);
			this.Name = "CompetitorDetailsUserControl";
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CompetitorGroupBox.ResumeLayout(false);
			this.CompetitorGroupBox.PerformLayout();
			this.CIFincanialDetailsGroupBox.ResumeLayout(false);
			this.CIFincanialDetailsGroupBox.PerformLayout();
			this.CISWOTGroupBox.ResumeLayout(false);
			this.CISWOTGroupBox.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private ZGroupBox CompetitorGroupBox;
		protected Enterprise.ZArchitecture.GUI.ZDropEdit OM_CICompetitiveRankingDropEdit;
		private Enterprise.ZArchitecture.GUI.ZDropEdit OM_CICompetitorCategoryBoundDropEdit;
		private Enterprise.ZArchitecture.ZCalcEdit OM_CIEstimatedStaffThisCountryBoundCalcEdit;
		private Enterprise.ZArchitecture.ZCalcEdit OM_CIEstimatedStaffThisLocationBoundCalcEdit;
		private Enterprise.ZArchitecture.GUI.ZDropEdit OM_CISellingStyleBoundDropEdit;
		private Enterprise.ZArchitecture.GUI.ZDropEdit OM_CITypeOfServiceBoundDropEdit;
		private ZGroupBox CIFincanialDetailsGroupBox;
		private Enterprise.ZArchitecture.ZCalcEdit OM_CICapitalEmployedBoundCalcEdit;
		private Enterprise.ZArchitecture.ZCalcEdit OM_CIProfitBoundCalcEdit;
		private Enterprise.ZArchitecture.ZCalcEdit OM_CITurnoverBoundCalcEdit;
		private Enterprise.ZArchitecture.GUI.ZDateEdit OM_CIFinancialDetailsApplicableFromDateDateEdit;
		private Enterprise.ZArchitecture.GUI.ZDateEdit OM_CIFinancialDetailsApplicableToDateDateEdit;
		private Enterprise.ZArchitecture.ZLabel FinancialDetailsApplicableFromDateLabel;
		private ZGroupBox CISWOTGroupBox;
		private Enterprise.ZArchitecture.ZTextBox OM_CIThreatsBoundTextBox;
		private Enterprise.ZArchitecture.ZTextBox OM_CIOpportunitiesBoundTextBox;
		private Enterprise.ZArchitecture.ZTextBox OM_CIWeaknessesBoundTextBox;
		private Enterprise.ZArchitecture.ZTextBox OM_CIStrengthBoundTextBox;
	}
}
