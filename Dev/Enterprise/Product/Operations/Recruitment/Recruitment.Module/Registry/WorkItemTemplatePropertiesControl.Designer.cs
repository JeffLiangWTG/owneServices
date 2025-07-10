namespace Enterprise.Recruitment.Registry
{
	partial class WorkItemTemplatePropertiesControl
	{
		#region Component Designer generated code

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


		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.WorkItemTemplatePropertiesGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.WorkItemTemplatePropertiesGrid)).BeginInit();
			this.WorkItemTemplatePropertiesGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Recruitment.Registry.WorkItemTemplatePropertiesCollection);
			// 
			// WorkItemTemplatePropertiesGrid
			// 
			this.WorkItemTemplatePropertiesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.WorkItemTemplatePropertiesGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Recruitment.Registry.WorkItemTemplateProperties)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Recruitment.Registry.WorkItemTemplateProperties)(null)).FriendlyName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Recruitment.Registry.WorkItemTemplateProperties)(null)).WKI_PK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Recruitment.Registry.WorkItemTemplateProperties)(null)).Type)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Recruitment.Registry.WorkItemTemplateProperties)(null)).Area)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Recruitment.Registry.WorkItemTemplateProperties)(null)).ActivityType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Recruitment.Registry.WorkItemTemplateProperties)(null)).ActivitySubType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Recruitment.Registry.WorkItemTemplateProperties)(null)).Priority)));
			this.WorkItemTemplatePropertiesGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Recruitment.Module.Res.GetData("b192df79-c80a-4c4d-8bc6-451f3bf85b2b", "Friendly Name");
			zTextBoxColumnStyleInfo1.ColumnName = "FriendlyName";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Recruitment.Module.Res.GetData("3e66d4d3-b8b7-49c1-8cfd-6446760b6a58", "Template");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "WKI_PK";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo1.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.WorkItem;
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Recruitment.Module.Res.GetData("6f7e9f24-f125-4ed8-97b7-82de0c5aec59", "Type");
			zTextBoxColumnStyleInfo2.ColumnName = "Type";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Recruitment.Module.Res.GetData("41e7139b-366d-4721-a115-bc106f36d2f7", "Area");
			zTextBoxColumnStyleInfo3.ColumnName = "Area";
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Recruitment.Module.Res.GetData("764c566c-82d7-47c4-ac8e-f0de5ed6869d", "Activity Type");
			zTextBoxColumnStyleInfo4.ColumnName = "ActivityType";
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Recruitment.Module.Res.GetData("67695f40-b123-4a42-9588-bafdcccb1232", "Activity Subtype");
			zTextBoxColumnStyleInfo5.ColumnName = "ActivitySubType";
			zTextBoxColumnStyleInfo5.IsReadOnly = true;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(88);
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Recruitment.Module.Res.GetData("33ef1c43-5abd-4b85-8a7a-9e8a8bc4db09", "Priority");
			zTextBoxColumnStyleInfo6.ColumnName = "Priority";
			zTextBoxColumnStyleInfo6.IsReadOnly = true;
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.WorkItemTemplatePropertiesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.WorkItemTemplatePropertiesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.WorkItemTemplatePropertiesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.WorkItemTemplatePropertiesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.WorkItemTemplatePropertiesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.WorkItemTemplatePropertiesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.WorkItemTemplatePropertiesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.WorkItemTemplatePropertiesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.WorkItemTemplatePropertiesGrid.GridId = "500ab525-71cb-421c-afa1-fb0c6785b3f3";
			this.WorkItemTemplatePropertiesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.WorkItemTemplatePropertiesGrid.LayoutKey = "WorkItemTemplatePropertiesGrid";
			this.WorkItemTemplatePropertiesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.WorkItemTemplatePropertiesGrid.Name = "WorkItemTemplatePropertiesGrid";
			this.WorkItemTemplatePropertiesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(720, 514, true);
			this.WorkItemTemplatePropertiesGrid.TabIndex = 0;
			// 
			// WorkItemTemplatePropertiesControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.WorkItemTemplatePropertiesGrid);
			this.Name = "WorkItemTemplatePropertiesControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(720, 514, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.WorkItemTemplatePropertiesGrid)).EndInit();
			this.WorkItemTemplatePropertiesGrid.ResumeLayout(false);
			this.WorkItemTemplatePropertiesGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion


		Enterprise.ZArchitecture.ZGrid WorkItemTemplatePropertiesGrid;
	}
}
