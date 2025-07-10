using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Recruiter.Module
{
	public partial class HRJobApplicationFilterControl
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

		System.ComponentModel.Container components = null;

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
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
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
			this.grid.SuspendLayout();
			this.AddStripButton.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// grid
			// 
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Recruiter.Module.Res.GetData("HRJobApplicationFilterControl|93afebc1-d8e8-46d2-9d42-79de5f3139c5", "Submission Time");
			zDateEditColumnStyleInfo1.ColumnName = "SubmissionTimeLocal";
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Recruiter.Module.Res.GetData("HRJobApplicationFilterControl|380bc8d0-ed7e-4d35-a88c-59d9292b7213", "Full Name");
			zTextBoxColumnStyleInfo1.ColumnName = "Applicant+HA_FullName";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Recruiter.Module.Res.GetData("HRJobApplicationFilterControl|5de36502-82b6-4f47-af62-f40d4e7807f0", "Mobile");
			zTextBoxColumnStyleInfo2.ColumnName = "Applicant+HA_MobilePhone";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Recruiter.Module.Res.GetData("HRJobApplicationFilterControl|ed693b3e-77ac-4bf3-8cf0-a5e858e84fe3", "Email");
			zTextBoxColumnStyleInfo3.ColumnName = "Applicant+HA_EmailAddress";
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo20.CaptionResourceString = Enterprise.Recruiter.Module.Res.GetData("HRJobApplicationFilterControl|059DCD70-88B6-4DA0-94D9-89A760D03B6A", "Application #");
			zTextBoxColumnStyleInfo20.ColumnName = "HP_ApplicationNumber";
			zTextBoxColumnStyleInfo20.IsReadOnly = true;
			zTextBoxColumnStyleInfo20.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Recruiter.Module.Res.GetData("HRJobApplicationFilterControl|3448da92-b87b-4d39-984d-751026bc1c65", "City");
			zTextBoxColumnStyleInfo4.ColumnName = "Applicant+HA_City";
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Recruiter.Module.Res.GetData("HRJobApplicationFilterControl|e90121bb-b5d8-473f-899e-222e8729140d", "Country/Region");
			zTextBoxColumnStyleInfo5.ColumnName = "Applicant+HA_RN_NKCountry";
			zTextBoxColumnStyleInfo5.IsReadOnly = true;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Recruiter.Module.Res.GetData("HRJobApplicationFilterControl|fde68de4-9c9c-4393-a338-14190c41c5c0", "Overall Rating");
			zTextBoxColumnStyleInfo6.ColumnName = "ApplicationOverallRatingDescription";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.Recruiter.Module.Res.GetData("HRJobApplicationFilterControl|fb1699e9-dfe1-431a-a99d-e7513b8258bd", "Status");
			zTextBoxColumnStyleInfo7.ColumnName = "HP_CurrentStatus";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo8.CaptionResourceString = Enterprise.Recruiter.Module.Res.GetData("HRJobApplicationFilterControl|164209e1-793a-4129-b98a-d74c72b24f40", "Assigned To");
			zTextBoxColumnStyleInfo8.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			zTextBoxColumnStyleInfo8.ColumnName = "HP_GS_NKAssignedTo";
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Recruiter.Module.Res.GetData("HRJobApplicationFilterControl|c61f48fe-c4e0-41eb-8782-c01434978ebc", "Yrs Experience");
			zCalcEditColumnStyleInfo1.ColumnName = "HP_JobExperienceYears";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo9.CaptionResourceString = Enterprise.Recruiter.Module.Res.GetData("HRJobApplicationFilterControl|d2a4662f-0d3a-4557-94c4-588414346b21", "Recruitment Coordinator");
			zTextBoxColumnStyleInfo9.ColumnName = "JobOpening+ControlledBy+GS_FullName";
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zTextBoxColumnStyleInfo10.ColumnName = "JobOpening+HV_AdTitle";
			zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo11.CaptionResourceString = Enterprise.Recruiter.Module.Res.GetData("HRJobApplicationFilterControl|082881e4-f5c2-4f63-b64d-f86f7793985f", "Job Role");
			zTextBoxColumnStyleInfo11.ColumnName = "JobOpening+JobRole+HJ_JobTitle";
			zTextBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo12.CaptionResourceString = Enterprise.Recruiter.Module.Res.GetData("HRJobApplicationFilterControl|884cf3d1-50eb-4adf-b4cb-77366dfbd6f3", "Office Location");
			zTextBoxColumnStyleInfo12.ColumnName = "JobOpening+ClientAccount+OH_Code";
			zTextBoxColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo13.CaptionResourceString = Enterprise.Recruiter.Module.Res.GetData("HRJobApplicationFilterControl|e0143fca-e11e-4f81-9d76-75a0eb730e6e", "Location Contact");
			zTextBoxColumnStyleInfo13.ColumnName = "JobOpening+ClientContact+OC_ContactName";
			zTextBoxColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo14.CaptionResourceString = Enterprise.Recruiter.Module.Res.GetData("HRJobApplicationFilterControl|1f462d79-484b-46a2-bb77-e630f0063cdb", "Source");
			zTextBoxColumnStyleInfo14.ColumnName = "HP_SourceType";
			zTextBoxColumnStyleInfo14.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo15.CaptionResourceString = Enterprise.Recruiter.Module.Res.GetData("HRJobApplicationFilterControl|d3e66090-eb6e-419e-8871-a65738bacb78", "Source Details");
			zTextBoxColumnStyleInfo15.ColumnName = "HP_SourceDetails";
			zTextBoxColumnStyleInfo15.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo15.IsVisible = false;
			zTextBoxColumnStyleInfo16.CaptionResourceString = Enterprise.Recruiter.Module.Res.GetData("HRJobApplicationFilterControl|57bc7803-0cb0-47c5-a2d3-7e63b7912926", "Referring Org. Code");
			zTextBoxColumnStyleInfo16.ColumnName = "ReferringOrganisation+OH_Code";
			zTextBoxColumnStyleInfo16.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo17.CaptionResourceString = Enterprise.Recruiter.Module.Res.GetData("HRJobApplicationFilterControl|eb9fefdb-600d-4c5c-968e-eba5dc5a6a64", "Referring Org. Name");
			zTextBoxColumnStyleInfo17.ColumnName = "ReferringOrganisation+OH_FullName";
			zTextBoxColumnStyleInfo17.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo17.IsVisible = false;
			zTextBoxColumnStyleInfo18.CaptionResourceString = Enterprise.Recruiter.Module.Res.GetData("HRJobApplicationFilterControl|e43dc1ad-5883-4126-b1f5-a89f0a594127", "Referring Person");
			zTextBoxColumnStyleInfo18.ColumnName = "ReferringPerson+PER_FullName";
			zTextBoxColumnStyleInfo18.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo19.CaptionResourceString = Enterprise.Recruiter.Module.Res.GetData("HRJobApplicationFilterControl|5a9c2e6c-adc6-4086-8584-1369d84de089", "Referring Staff");
			zTextBoxColumnStyleInfo19.ColumnName = "ReferringStaffCode";
			zTextBoxColumnStyleInfo19.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo19.IsVisible = false;
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Recruiter.Module.Res.GetData("HRJobApplicationFilterControl|e1eb0ebe-ed7b-4d7f-b696-718dd7d2cb7e", "Parsed Documents");
			zCheckBoxColumnStyleInfo1.ColumnName = "ParsedDocuments";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo20);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.grid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo13);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo14);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo15);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo16);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo17);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo18);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo19);
			this.grid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1100, 197, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Recruiter.Business.HRJobApplication);
			// 
			// HRJobApplicationFilterControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Name = "HRJobApplicationFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1100, 349, true);
			((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
			this.grid.ResumeLayout(false);
			this.grid.PerformLayout();
			this.AddStripButton.ResumeLayout(true);
			this.AddStripButton.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
