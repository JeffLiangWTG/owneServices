using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Recruiter.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Recruiter.Module
{
	public partial class LearningCentreCampaignFilterControl
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			//
			// grid
			//
			zTextBoxColumnStyleInfo1.Caption = null;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Res.GetData("LearningCentreCampaignFilterControl|f3abe606-588c-454f-9cb9-e330d7762bd1", "Exam ID");
			zTextBoxColumnStyleInfo1.ColumnName = "G0_CampaignID";
			zTextBoxColumnStyleInfo2.Caption = null;
			zTextBoxColumnStyleInfo2.CaptionResourceString = Res.GetData("LearningCentreCampaignFilterControl|d81e50d3-b53e-4661-8618-26e21f2da39a", "Type");
			zTextBoxColumnStyleInfo2.ColumnName = "G0_Type";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo3.Caption = null;
			zTextBoxColumnStyleInfo3.CaptionResourceString = Res.GetData("LearningCentreCampaignFilterControl|76065077-3be3-4a6d-83a0-103541703784", "Exam Name");
			zTextBoxColumnStyleInfo3.ColumnName = "G0_CampaignNameMultilingual";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(300);
			zTextBoxColumnStyleInfo4.Caption = null;
			zTextBoxColumnStyleInfo4.CaptionResourceString = null;
			zTextBoxColumnStyleInfo4.ColumnName = "G0_GS_NKCampaignCoordinator";
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.grid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 136, true);
			this.grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(758, 38, true);
			//
			// BindingSource
			//
			this.BindingSource.DataSourceType = typeof(Enterprise.Recruiter.Business.LearningCentreCampaign);
			//
			// LearningCentreCampaignFilterControl
			//
			this.CaptionRenderingEnabled = true;
			this.Name = "LearningCentreCampaignFilterControl";
			((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
