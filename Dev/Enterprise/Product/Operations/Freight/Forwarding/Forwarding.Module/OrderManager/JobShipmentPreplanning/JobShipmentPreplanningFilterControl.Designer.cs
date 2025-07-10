using CargoWise.EntityFramework;
using Enterprise.Freight.Module;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.Orders.Module
{
	public partial class JobShipmentPreplanningFilterControl : ZFilterStripControl
	{
		void InitializeComponent()
		{
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo1 = new MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo2 = new MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo3 = new MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo4 = new MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new ZGuidFindBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new ZGuidFindBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZDateTimeOffsetEditColumnStyleInfo zDateEditColumnStyleInfo1 = new ZArchitecture.ZDateTimeOffsetEditColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZDateTimeOffsetEditColumnStyleInfo zDateEditColumnStyleInfo2 = new ZArchitecture.ZDateTimeOffsetEditColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.FilteredGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// FilteredGrid
			// 
			zTextBoxColumnStyleInfo1.ColumnName = "EF_PreshipID";
			zOrganisationFindBoxColumnStyleInfo1.ColumnName = "BuyerPK";
			zTextBoxColumnStyleInfo2.ColumnName = "EF_RL_NKPortLoad";
			zTextBoxColumnStyleInfo3.ColumnName = "EF_RL_NKPortDisch";
			zOrganisationFindBoxColumnStyleInfo2.ColumnName = "EF_OH_SendingAgent";
			zOrganisationFindBoxColumnStyleInfo3.ColumnName = "EF_OH_ReceivingAgent";
			zOrganisationFindBoxColumnStyleInfo4.ColumnName = "EF_OH_Carrier";
			zTextBoxColumnStyleInfo4.ColumnName = "EF_MasterBill";
			zTextBoxColumnStyleInfo5.ColumnName = "EF_HouseBill";
			zGuidFindBoxColumnStyleInfo1.ColumnName = "EF_JS";
			zGuidFindBoxColumnStyleInfo2.ColumnName = "EF_JE";
			zTextBoxColumnStyleInfo6.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Freight.Forwarding.Module.Res.GetData("JobShipmentPreplanningFilterControl|d011e00f-27b7-44db-a87f-1ebdce22bbf0", "Last Milestone Event");
			zTextBoxColumnStyleInfo6.ColumnName = "WorkflowItems+Milestones+LastMilestone+P9_SE_NKMilestoneEvent";
			zTextBoxColumnStyleInfo6.IsVisible = false;
			zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.Freight.Forwarding.Module.Res.GetData("JobShipmentPreplanningFilterControl|368e8e6b-8c20-4feb-8e7d-707478b445cb", "Last Milestone Desc.", "Last Milestone Description");
			zTextBoxColumnStyleInfo7.ColumnName = "WorkflowItems+Milestones+LastMilestone+P9_Description";
			zTextBoxColumnStyleInfo7.IsVisible = false;
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.Forwarding.Module.Res.GetData("JobShipmentPreplanningFilterControl|2f3e4e73-dfe2-41f5-9700-6d12bdd4b4da", "Last Milestone ATD");
			zDateEditColumnStyleInfo1.ColumnName = "WorkflowItems+Milestones+LastMilestone+P9_ActualDateForBinding";
			zDateEditColumnStyleInfo1.IsVisible = false;
			zTextBoxColumnStyleInfo8.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo8.CaptionResourceString = Enterprise.Freight.Forwarding.Module.Res.GetData("JobShipmentPreplanningFilterControl|315120b4-1d49-47ee-bfd0-319908e37568", "Next Milestone Event");
			zTextBoxColumnStyleInfo8.ColumnName = "WorkflowItems+Milestones+NextMilestone+P9_SE_NKMilestoneEvent";
			zTextBoxColumnStyleInfo8.IsVisible = false;
			zTextBoxColumnStyleInfo9.CaptionResourceString = Enterprise.Freight.Forwarding.Module.Res.GetData("JobShipmentPreplanningFilterControl|482a1322-176a-4e50-8e6a-dda3db248ec9", "Next Milestone Desc.", "Next Milestone Description");
			zTextBoxColumnStyleInfo9.ColumnName = "WorkflowItems+Milestones+NextMilestone+P9_Description";
			zTextBoxColumnStyleInfo9.IsVisible = false;
			zDateEditColumnStyleInfo2.CaptionResourceString = Enterprise.Freight.Forwarding.Module.Res.GetData("JobShipmentPreplanningFilterControl|00752229-7daa-4d4c-8632-9263e3f450d3", "Next Milestone ETD");
			zDateEditColumnStyleInfo2.ColumnName = "WorkflowItems+Milestones+NextMilestone+P9_ScheduledDateForBinding";
			zDateEditColumnStyleInfo2.IsVisible = false;
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.FilteredGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo1);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.FilteredGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo2);
			this.FilteredGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo3);
			this.FilteredGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo4);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.FilteredGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.FilteredGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.FilteredGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.FilteredGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.FilteredGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 12, true);
			this.FilteredGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(744, 492, true);
			this.FilteredGrid.TabIndex = 28;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Business.JobShipmentPreplanning);
			// 
			// JobShipmentPreplanningFilterControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Name = "JobShipmentPreplanningFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(744, 504, true);
			((System.ComponentModel.ISupportInitialize)(this.FilteredGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		System.ComponentModel.IContainer components = null;
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
	}
}
