using System;
using CargoWise.Types;
using Enterprise.MarketingManager.Business;
using Enterprise.MarketingManager.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.Recruiter.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

#pragma warning disable IDE0001 // Simplify names. Designer requires fully qualified names to correctly deserialize properties

namespace Enterprise.Recruiter.GUI
{
	public class HRContentDesignerControl : ContentDesignerControl
	{
		protected new HRCampaignEmailTemplateEditor TemplateEditor => (HRCampaignEmailTemplateEditor)base.TemplateEditor;

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			AddDataSourceDropEdit();
			ReadjustPanel2Components();
		}

		void ReadjustPanel2Components()
		{
			GetPanel2().Controls.Remove(ContactGuidFindBox);
			dataSourceGroupBox.Controls.Add(ContactGuidFindBox);

			ContactGuidFindBox.CaptionResourceString = null;
			ContactGuidFindBox.Visible = false;
			ContactGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 16);
			Panel2_Resize(null, null);
			InfoTextBox_ResizeAndRelocate();
		}

		void AddDataSourceDropEdit()
		{
			dataSourceGroupBox = new ZGroupBox();
			DataSourceDropEdit = new ZDropEdit();
			dataSourceGroupBox.SuspendLayout();
			DataSourceDropEdit.SuspendLayout();
			AddPanel2Control(dataSourceGroupBox);

			// 
			// publishedListGroupBox
			// 
			dataSourceGroupBox.BackColor = System.Drawing.Color.Transparent;
			dataSourceGroupBox.CaptionResourceString = Enterprise.Recruiter.GUI.Res.GetData("12cd20b8-91b3-43d2-a3ee-f0ade99f6a26", "Simulation Data Source and Recipient");
			dataSourceGroupBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			dataSourceGroupBox.Controls.Add(DataSourceDropEdit);
			dataSourceGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 1, true);
			dataSourceGroupBox.Name = "dataSourceGroupBox";
			CargoWise.Windows.UI.ControlDpiScalingHelper.SetWidth(dataSourceGroupBox, GetDesiredDataSourceGroupBoxWidth(), false);
			CargoWise.Windows.UI.ControlDpiScalingHelper.SetHeight(dataSourceGroupBox, 40, true);
			dataSourceGroupBox.TabIndex = 4;
			dataSourceGroupBox.TabStop = false;
			// 
			// DataSourceDropEdit
			// 
			DataSourceDropEdit.AllowDrop = true;
			BindingSource.SetBindingMember(this.DataSourceDropEdit, "SimulationContactDataSource");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Recruiter.Business.HRGlbCompanyCampaign)(null)).SimulationContactDataSource);
			DataSourceDropEdit.CaptionResourceString = Enterprise.Recruiter.GUI.Res.GetData("c35e0385-a3ed-4319-b35d-23afedbb95cc", "Data Source");
			DataSourceDropEdit.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left;
			DataSourceDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			DataSourceDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 16, true);
			DataSourceDropEdit.Name = "DataSourceDropEdit";
			DataSourceDropEdit.ShowDescriptionBox = false;
			DataSourceDropEdit.ShowInDropDown = ZDropEdit.ShowInDropDownList.OnlyShowCode;
			DataSourceDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 18, true);
			DataSourceDropEdit.TabIndex = 0;
			DataSourceDropEdit.TextChanged += DataSourceDropEdit_TextChanged;

			dataSourceGroupBox.ResumeLayout(true);
			dataSourceGroupBox.PerformLayout();
			DataSourceDropEdit.ResumeLayout(true);
			DataSourceDropEdit.PerformLayout();
		}

		ZGroupBox dataSourceGroupBox;
		protected ZDropEdit DataSourceDropEdit;

		#region Resize

		protected override void Panel2_Resize(object sender, EventArgs e)
		{
			var width = GetPanel2().Width - CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			CargoWise.Windows.UI.ControlDpiScalingHelper.SetWidth(ContactGuidFindBox.CodeBox, width, false);
		}

		protected int GetDesiredDataSourceGroupBoxWidth()
		{
			return GetPanel2().Width - CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(4);
		}

		#endregion

		void DataSourceDropEdit_TextChanged(object sender, EventArgs e)
		{
			var dropEdit = (ZDropEdit)sender;
			if (CurrentDataItem.Lookups.HRSimulationContactDataSourceList.ContainsCode(dropEdit.Text))
			{
				ContactGuidFindBox.ModuleID = GetModuleID(dropEdit.Text);
				SetContactBindingMember(dropEdit.Text);
				ContactGuidFindBox.Visible = true;
			}
			else
			{
				ContactGuidFindBox.Visible = false;
			}
		}

		public new HRGlbCompanyCampaign CurrentDataItem => (HRGlbCompanyCampaign)base.CurrentDataItem;

		protected override ModuleIdentifier GetModuleID()
		{
			return GetModuleID(CurrentDataItem.SimulationContactDataSource);
		}

		protected ModuleIdentifier GetModuleID(ZString dataSource)
		{
			return dataSource == HRContactDataSourceList.Codes.JobApplicant ? ModuleIDs.HRJobApplicant : ModuleIDs.GlbStaff;
		}

		protected override GlbCompanyCampaignItem SetupCampaignItem(GlbCompanyCampaignItem campaignItem)
		{
			campaignItem = base.SetupCampaignItem(campaignItem);

			var staffRecipient = campaignItem.Factory.Load<GlbStaff>(campaignItem.G8_RecipientID);

			if (staffRecipient != null)
			{
				campaignItem.G8_RecipientTableCode = GlbStaffSchema.Constants.Prefix;
			}
			else
			{
				var applicantRecipient = campaignItem.Factory.Load<HRJobApplicant>(campaignItem.G8_RecipientID);

				if (applicantRecipient != null)
				{
					campaignItem.G8_RecipientTableCode = HRJobApplicantSchema.Constants.Prefix;
				}
			}

			return campaignItem;
		}

		protected override void SetContactBindingMember()
		{
			SetContactBindingMember(CurrentDataItem.SimulationContactDataSource);
		}

		protected void SetContactBindingMember(ZString dataSource)
		{
			if (dataSource == HRContactDataSourceList.Codes.Staff)
			{
				BindingSource.SetBindingMember(ContactGuidFindBox, "TemplateEditor.StaffSimulationContactPK");
			}
			else
			{
				BindingSource.SetBindingMember(ContactGuidFindBox, "TemplateEditor.ApplicantSimulationContactPK");
			}
		}
	}
}
