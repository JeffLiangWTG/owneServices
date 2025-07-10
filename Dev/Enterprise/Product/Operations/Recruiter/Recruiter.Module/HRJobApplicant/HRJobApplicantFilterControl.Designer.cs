using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Recruiter.Module
{
	public partial class HRJobApplicantFilterControl
	{
		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing && components != null)
			{
				components.Dispose();
			}

			base.Dispose(disposing);
		}
		#endregion

		private System.ComponentModel.Container components = null;

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();

			((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// grid
			// 
			zTextBoxColumnStyleInfo1.Caption = null;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Recruiter.Module.Res.GetData("HRJobApplicantFilterControl|24a056f4-4443-40d9-9570-9cec53eae44f", "Title");
			zTextBoxColumnStyleInfo1.ColumnName = "HA_Title";
			zTextBoxColumnStyleInfo1.IsVisible = false;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo2.Caption = null;
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Recruiter.Module.Res.GetData("HRJobApplicantFilterControl|e350bac1-cf18-44c5-a588-987e7a9ab41a", "Full Name");
			zTextBoxColumnStyleInfo2.ColumnName = "HA_FullName";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo3.Caption = null;
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Recruiter.Module.Res.GetData("HRJobApplicantFilterControl|755cbc90-f885-4510-84fd-552fa5d2b890", "Email Address");
			zTextBoxColumnStyleInfo3.ColumnName = "HA_EmailAddress";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zDateEditColumnStyleInfo1.Caption = null;
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Recruiter.Module.Res.GetData("HRJobApplicantFilterControl|f1a8c17f-1083-4f35-aa72-0177b760b567", "Birthdate");
			zDateEditColumnStyleInfo1.ColumnName = "HA_Birthdate";
			zDateEditColumnStyleInfo1.DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zTextBoxColumnStyleInfo4.Caption = null;
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Recruiter.Module.Res.GetData("HRJobApplicantFilterControl|d073f455-6523-4b33-b93a-3bf0eff4d739", "Availability");
			zTextBoxColumnStyleInfo4.ColumnName = "HA_Availability";
			zTextBoxColumnStyleInfo4.IsVisible = false;
			zTextBoxColumnStyleInfo5.Caption = null;
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Recruiter.Module.Res.GetData("HRJobApplicantFilterControl|a217dc6d-6abb-4658-8f0f-80fe9df698d1", "City");
			zTextBoxColumnStyleInfo5.ColumnName = "HA_City";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo6.Caption = null;
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Recruiter.Module.Res.GetData("HRJobApplicantFilterControl|4a901527-2557-496a-86c1-29d09d080297", "State");
			zTextBoxColumnStyleInfo6.ColumnName = "HA_State";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo7.Caption = null;
			zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.Recruiter.Module.Res.GetData("HRJobApplicantFilterControl|1da05070-a253-46a4-b455-921e6cd03047", "Mobile");
			zTextBoxColumnStyleInfo7.ColumnName = "HA_MobilePhone";
			zTextBoxColumnStyleInfo8.Caption = null;
			zTextBoxColumnStyleInfo8.CaptionResourceString = Enterprise.Recruiter.Module.Res.GetData("HRJobApplicantFilterControl|7202db64-caee-49e5-b704-ee2bd036cd38", "Home Phone");
			zTextBoxColumnStyleInfo8.ColumnName = "HA_HomePhone";
			zTextBoxColumnStyleInfo9.Caption = null;
			zTextBoxColumnStyleInfo9.CaptionResourceString = Enterprise.Recruiter.Module.Res.GetData("HRJobApplicantFilterControl|7a6fac8f-0228-421c-a25b-d2958111a064", "Fax Number");
			zTextBoxColumnStyleInfo9.ColumnName = "HA_FaxNum";
			zTextBoxColumnStyleInfo10.Caption = null;
			zTextBoxColumnStyleInfo10.CaptionResourceString = Enterprise.Recruiter.Module.Res.GetData("HRJobApplicantFilterControl|d1c280fe-5cbb-4372-aa9e-efb357ead131", "Gender");
			zTextBoxColumnStyleInfo10.ColumnName = "HA_Gender";
			zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.Caption = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Recruiter.Module.Res.GetData("HRJobApplicantFilterControl|b7000d1f-1005-442e-b43c-355d9e1e2858", "Current Wage");
			zCalcEditColumnStyleInfo1.ColumnName = "HA_CurrentWage";
			zCalcEditColumnStyleInfo1.GroupName = Enterprise.Recruiter.Module.Res.GetData("HRJobApplicantFilterControl|dc5c3175-368b-4af0-93e9-91000e30e01a", "Current Wages");
			zCalcEditColumnStyleInfo1.IsVisible = false;
			zCodeFindBoxColumnStyleInfo1.Caption = null;
			zCodeFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Recruiter.Module.Res.GetData("HRJobApplicantFilterControl|fac697a6-8248-4ef8-9cce-43665396c8a1", "Currency");
			zCodeFindBoxColumnStyleInfo1.ColumnName = "HA_RX_NKCurrentWageCurrency";
			zCodeFindBoxColumnStyleInfo1.GroupName = Enterprise.Recruiter.Module.Res.GetData("HRJobApplicantFilterControl|dc5c3175-368b-4af0-93e9-91000e30e01a", "Current Wages");
			zCodeFindBoxColumnStyleInfo1.IsVisible = false;
			zCodeFindBoxColumnStyleInfo1.ModuleID = ZArchitecture.Modules.ModuleIDs.RefCurrency;
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.Caption = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Recruiter.Module.Res.GetData("HRJobApplicantFilterControl|e252e4b2-8498-4d1a-a148-28cf2beea539", "Wage Expectation");
			zCalcEditColumnStyleInfo2.ColumnName = "HA_WageExpectation";
			zCalcEditColumnStyleInfo2.GroupName = Enterprise.Recruiter.Module.Res.GetData("HRJobApplicantFilterControl|986fd20f-b7a8-49ae-9dbe-1a96a723c07c", "Wage Expectation");
			zCalcEditColumnStyleInfo2.IsVisible = false;
			zCodeFindBoxColumnStyleInfo2.Caption = null;
			zCodeFindBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Recruiter.Module.Res.GetData("HRJobApplicantFilterControl|5833311a-d1af-40d6-8e57-028bea39ebb3", "Currency");
			zCodeFindBoxColumnStyleInfo2.ColumnName = "HA_RX_NKWageExpectationCurrency";
			zCodeFindBoxColumnStyleInfo2.GroupName = Enterprise.Recruiter.Module.Res.GetData("HRJobApplicantFilterControl|986fd20f-b7a8-49ae-9dbe-1a96a723c07c", "Wage Expectation");
			zCodeFindBoxColumnStyleInfo2.IsVisible = false;
			zCodeFindBoxColumnStyleInfo2.ModuleID = ZArchitecture.Modules.ModuleIDs.RefCurrency;
			zCodeFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo11.Caption = null;
			zTextBoxColumnStyleInfo11.CaptionResourceString = Enterprise.Recruiter.Module.Res.GetData("HRJobApplicantFilterControl|80c2f2f5-04b8-46cf-9489-e5506b63148c", "Drivers License Number");
			zTextBoxColumnStyleInfo11.ColumnName = "HA_DriversLicenseNumber";
			zTextBoxColumnStyleInfo11.IsVisible = false;
			zTextBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo12.Caption = null;
			zTextBoxColumnStyleInfo12.CaptionResourceString = Enterprise.Recruiter.Module.Res.GetData("HRJobApplicantFilterControl|2e3389a4-0b99-4498-a727-6e4987972372", "Work Permit Status");
			zTextBoxColumnStyleInfo12.ColumnName = "HA_WorkPermitStatus";
			zTextBoxColumnStyleInfo12.IsVisible = false;
			zTextBoxColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Recruiter.Module.Res.GetData("HRJobApplicantFilterControl|08d63ce3-a9b6-46eb-bf6f-1c497d0663ca", "Learning Center User");
			zCheckBoxColumnStyleInfo1.ColumnName = "IsLearningCenterUser";
			zCheckBoxColumnStyleInfo1.IsVisible = false;
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDateEditColumnStyleInfo2.Caption = null;
			zDateEditColumnStyleInfo2.CaptionResourceString = Enterprise.Recruiter.Module.Res.GetData("HRJobApplicantFilterControl|0ceb23d8-c32d-4481-a4b9-99c2e950b530", "Last Application Date");
			zDateEditColumnStyleInfo2.ColumnName = "LastSubmittedJobApplicationSubmissionTime";
			zDateEditColumnStyleInfo2.DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.grid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
			this.grid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.grid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 192, true);
			this.grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(640, 224, true);
			this.grid.TabIndex = 9;
			// 
			// AddStripButton
			// 
			this.AddStripButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(590, 28, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Recruiter.Business.HRJobApplicant);
			// 
			// HRJobApplicantFilterControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Name = "HRJobApplicantFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(640, 416, true);
			((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
