using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Recruiter.Module
{
	public partial class HRJobOpeningsFilterControl
	{
		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#endregion

		System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo1 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// grid
			// 
			zTextBoxColumnStyleInfo1.Caption = null;
			zTextBoxColumnStyleInfo1.CaptionResourceString = null;
			zTextBoxColumnStyleInfo1.ColumnName = "HV_AdTitle";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zGuidFindBoxColumnStyleInfo1.BindToList = "Lookups+JobRoles";
			zGuidFindBoxColumnStyleInfo1.Caption = null;
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = null;
			zGuidFindBoxColumnStyleInfo1.ColumnName = "HV_HJ_JobRole";
			zGuidFindBoxColumnStyleInfo1.ModuleID = ZArchitecture.Modules.ModuleIDs.HRJobRole;
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zOrganisationFindBoxColumnStyleInfo1.BindToList = "Lookups+ClientAccounts";
			zOrganisationFindBoxColumnStyleInfo1.Caption = null;
			zOrganisationFindBoxColumnStyleInfo1.CaptionResourceString = null;
			zOrganisationFindBoxColumnStyleInfo1.ColumnName = "HV_OH_ClientAccount";
			zDateEditColumnStyleInfo1.Caption = null;
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Recruiter.Module.Res.GetData("HRJobCampaignFilterControl|0de503be-61a2-47f6-a8b3-3c1d12c689c7", "Start Date");
			zDateEditColumnStyleInfo1.ColumnName = "HV_CampaignStartDateLocal";
			zDateEditColumnStyleInfo1.DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDateEditColumnStyleInfo2.Caption = null;
			zDateEditColumnStyleInfo2.CaptionResourceString = Enterprise.Recruiter.Module.Res.GetData("HRJobCampaignFilterControl|51b00de0-6b72-4424-a9e1-b9d2ca6ec7f8", "End Date");
			zDateEditColumnStyleInfo2.ColumnName = "HV_CampaignEndDateLocal";
			zDateEditColumnStyleInfo2.DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo2.Caption = null;
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Recruiter.Module.Res.GetData("HRJobCampaignFilterControl|d2a4662f-0d3a-4557-94c4-588414346b20", "Campaign Leader");
			zTextBoxColumnStyleInfo2.ColumnName = "ControlledBy+GS_FullName";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.Caption = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Recruiter.Module.Res.GetData("HRJobCampaignFilterControl|72df7058-57c3-4597-856e-556fc6ada692", "Avail.");
			zCalcEditColumnStyleInfo1.ColumnName = "HV_NumberOfPositionsAvailable";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(35);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.Caption = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Recruiter.Module.Res.GetData("HRJobCampaignFilterControl|4216abcb-400d-4b41-8fb4-16fd7d198a4e", "Filled");
			zCalcEditColumnStyleInfo2.ColumnName = "HV_NumberOfPositionsFilled";
			zCalcEditColumnStyleInfo2.Decimals = 0;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(35);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.Caption = null;
			zCalcEditColumnStyleInfo3.CaptionResourceString = Enterprise.Recruiter.Module.Res.GetData("HRJobCampaignFilterControl|123e0a67-e10f-4529-867d-fb9772fef184", "Max Salary");
			zCalcEditColumnStyleInfo3.ColumnName = "HV_WageHigh";
			zCalcEditColumnStyleInfo3.Decimals = 0;
			zCalcEditColumnStyleInfo3.GroupName = Enterprise.Recruiter.Module.Res.GetData("HRJobCampaignFilterControl|115180e1-4908-4174-a7ae-1a1848f53c14", "Salary");
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.Caption = null;
			zCalcEditColumnStyleInfo4.CaptionResourceString = Enterprise.Recruiter.Module.Res.GetData("HRJobCampaignFilterControl|49c56aeb-f76b-44a5-851a-612075f718a5", "Min Salary");
			zCalcEditColumnStyleInfo4.ColumnName = "HV_WageLow";
			zCalcEditColumnStyleInfo4.Decimals = 0;
			zCalcEditColumnStyleInfo4.GroupName = Enterprise.Recruiter.Module.Res.GetData("HRJobCampaignFilterControl|115180e1-4908-4174-a7ae-1a1848f53c14", "Salary");
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zTextBoxColumnStyleInfo3.Caption = null;
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Recruiter.Module.Res.GetData("HRJobCampaignFilterControl|9eeacb7b-b45f-4637-b7b4-7535f0a43194", "Location");
			zTextBoxColumnStyleInfo3.ColumnName = "CampaignLocation";
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.grid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.grid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 136, true);
			this.grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(758, 38, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Recruiter.Business.HRRecruitmentJobCampaign);
			// 
			// HRJobCampaignFilterControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Name = "HRJobCampaignFilterControl";
			((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
