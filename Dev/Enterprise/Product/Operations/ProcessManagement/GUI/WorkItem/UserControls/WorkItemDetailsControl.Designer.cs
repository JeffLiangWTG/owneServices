using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ProcessManagement.GUI
{
	partial class WorkItemDetailsControl : ZUserControl
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
			this.DetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.WorkItemDetailsPanel = new CargoWise.Windows.UI.Layout.RowLayoutPanel();
			this.CompanyFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.PortOrCountryFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.DepartmentFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.PriorityDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ActivitySubTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ActivityTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.WorkItemAreaDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.WorkItemTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			DetailsGroupBox.SuspendLayout();
			WorkItemDetailsPanel.SuspendLayout();
			CompanyFindBox.SuspendLayout();
			PortOrCountryFindBox.SuspendLayout();
			DepartmentFindBox.SuspendLayout();
			PriorityDropEdit.SuspendLayout();
			ActivitySubTypeDropEdit.SuspendLayout();
			ActivityTypeDropEdit.SuspendLayout();
			WorkItemAreaDropEdit.SuspendLayout();
			WorkItemTypeDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.ProcessManagement.Business.WorkItem);
			// 
			// DetailsGroupBox
			// 
			this.DetailsGroupBox.CaptionResourceString = Enterprise.ProcessManagement.GUI.Res.GetData("0a578308-c6d2-4fc3-8213-04e4cd6f4fd5", "Details");
			this.DetailsGroupBox.Controls.Add(this.WorkItemDetailsPanel);
			this.DetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DetailsGroupBox.Name = "DetailsGroupBox";
			this.DetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(369, 230, true);
			this.DetailsGroupBox.TabIndex = 0;
			this.DetailsGroupBox.TabStop = false;
			// 
			// WorkItemDetailsPanel
			// 
			this.WorkItemDetailsPanel.Alignment = System.Windows.Forms.VisualStyles.VerticalAlignment.Center;
			this.WorkItemDetailsPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.WorkItemDetailsPanel.Controls.Add(this.CompanyFindBox);
			this.WorkItemDetailsPanel.Controls.Add(this.PortOrCountryFindBox);
			this.WorkItemDetailsPanel.Controls.Add(this.DepartmentFindBox);
			this.WorkItemDetailsPanel.Controls.Add(this.PriorityDropEdit);
			this.WorkItemDetailsPanel.Controls.Add(this.ActivitySubTypeDropEdit);
			this.WorkItemDetailsPanel.Controls.Add(this.ActivityTypeDropEdit);
			this.WorkItemDetailsPanel.Controls.Add(this.WorkItemAreaDropEdit);
			this.WorkItemDetailsPanel.Controls.Add(this.WorkItemTypeDropEdit);
			this.VisibilityConfigurationProvider.SetIsVisibilityConfigured(this.WorkItemDetailsPanel, true);
			this.WorkItemDetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 16, true);
			this.WorkItemDetailsPanel.Name = "WorkItemDetailsPanel";
			this.WorkItemDetailsPanel.RowHeight = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(25);
			this.WorkItemDetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(227, 206, true);
			this.WorkItemDetailsPanel.TabIndex = 1;
			// 
			// CompanyFindBox
			// 
			this.CompanyFindBox.AllowDrop = true;
			this.CompanyFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.CompanyFindBox, "WKI_GC_AssignedCompany");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.ProcessManagement.Business.WorkItem)(null)).WKI_GC_AssignedCompany)));
			this.CompanyFindBox.CaptionResourceString = Enterprise.ProcessManagement.GUI.Res.GetData("540b1902-c096-4cf5-a48c-0025f073206a", "Company");
			this.CompanyFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 152, true);
			this.CompanyFindBox.Name = "CompanyFindBox";
			this.CompanyFindBox.PreBoundMaxLength = 5;
			this.WorkItemDetailsPanel.SetRow(this.CompanyFindBox, 6);
			this.CompanyFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(227, 20, true);
			this.CompanyFindBox.TabIndex = 7;
			// 
			// PortOrCountryFindBox
			// 
			this.PortOrCountryFindBox.AllowDrop = true;
			this.PortOrCountryFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.PortOrCountryFindBox, "WKI_PortOrCountry");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ProcessManagement.Business.WorkItem)(null)).WKI_PortOrCountry)));
			this.PortOrCountryFindBox.CaptionResourceString = Enterprise.ProcessManagement.GUI.Res.GetData("2083aad2-5f41-48ec-8517-fb45333938ff", "Country/Region/Port");
			this.PortOrCountryFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 177, true);
			this.PortOrCountryFindBox.Name = "PortOrCountryFindBox";
			this.WorkItemDetailsPanel.SetRow(this.PortOrCountryFindBox, 7);
			this.PortOrCountryFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(227, 20, true);
			this.PortOrCountryFindBox.TabIndex = 8;
			// 
			// DepartmentFindBox
			// 
			this.DepartmentFindBox.AllowDrop = true;
			this.DepartmentFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.DepartmentFindBox, "WKI_GE_AssignedDepartment");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.ProcessManagement.Business.WorkItem)(null)).WKI_GE_AssignedDepartment)));
			this.DepartmentFindBox.CaptionResourceString = Enterprise.ProcessManagement.GUI.Res.GetData("8b1a6057-bd99-4411-ab70-6f4121e06c7e", "Department");
			this.DepartmentFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 127, true);
			this.DepartmentFindBox.Name = "DepartmentFindBox";
			this.DepartmentFindBox.PreBoundMaxLength = 5;
			this.WorkItemDetailsPanel.SetRow(this.DepartmentFindBox, 5);
			this.DepartmentFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(227, 20, true);
			this.DepartmentFindBox.TabIndex = 6;
			// 
			// PriorityDropEdit
			// 
			this.PriorityDropEdit.AllowDrop = true;
			this.PriorityDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.PriorityDropEdit, "WKI_Priority");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.ProcessManagement.Business.WorkItem)(null)).WKI_Priority)));
			this.PriorityDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 102, true);
			this.PriorityDropEdit.Name = "PriorityDropEdit";
			this.PriorityDropEdit.PreBoundMaxLength = 3;
			this.WorkItemDetailsPanel.SetRow(this.PriorityDropEdit, 4);
			this.PriorityDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(227, 20, true);
			this.PriorityDropEdit.TabIndex = 5;
			// 
			// ActivitySubTypeDropEdit
			// 
			this.ActivitySubTypeDropEdit.AllowDrop = true;
			this.ActivitySubTypeDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ActivitySubTypeDropEdit, "WKI_ActivitySubtype");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.ProcessManagement.Business.WorkItem)(null)).WKI_ActivitySubtype)));
			this.ActivitySubTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 77, true);
			this.ActivitySubTypeDropEdit.Name = "ActivitySubTypeDropEdit";
			this.ActivitySubTypeDropEdit.PreBoundMaxLength = 3;
			this.WorkItemDetailsPanel.SetRow(this.ActivitySubTypeDropEdit, 3);
			this.ActivitySubTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(227, 20, true);
			this.ActivitySubTypeDropEdit.TabIndex = 4;
			// 
			// ActivityTypeDropEdit
			// 
			this.ActivityTypeDropEdit.AllowDrop = true;
			this.ActivityTypeDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ActivityTypeDropEdit, "WKI_ActivityType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.ProcessManagement.Business.WorkItem)(null)).WKI_ActivityType)));
			this.ActivityTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 52, true);
			this.ActivityTypeDropEdit.Name = "ActivityTypeDropEdit";
			this.ActivityTypeDropEdit.PreBoundMaxLength = 3;
			this.WorkItemDetailsPanel.SetRow(this.ActivityTypeDropEdit, 2);
			this.ActivityTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(227, 20, true);
			this.ActivityTypeDropEdit.TabIndex = 3;
			// 
			// WorkItemAreaDropEdit
			// 
			this.WorkItemAreaDropEdit.AllowDrop = true;
			this.WorkItemAreaDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.WorkItemAreaDropEdit, "WKI_WorkItemArea");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.ProcessManagement.Business.WorkItem)(null)).WKI_WorkItemArea)));
			this.WorkItemAreaDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 27, true);
			this.WorkItemAreaDropEdit.Name = "WorkItemAreaDropEdit";
			this.WorkItemAreaDropEdit.PreBoundMaxLength = 3;
			this.WorkItemDetailsPanel.SetRow(this.WorkItemAreaDropEdit, 1);
			this.WorkItemAreaDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(227, 20, true);
			this.WorkItemAreaDropEdit.TabIndex = 2;
			// 
			// WorkItemTypeDropEdit
			// 
			this.WorkItemTypeDropEdit.AllowDrop = true;
			this.WorkItemTypeDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.WorkItemTypeDropEdit, "WKI_WorkItemType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.ProcessManagement.Business.WorkItem)(null)).WKI_WorkItemType)));
			this.WorkItemTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 2, true);
			this.WorkItemTypeDropEdit.Name = "WorkItemTypeDropEdit";
			this.WorkItemTypeDropEdit.PreBoundMaxLength = 3;
			this.WorkItemDetailsPanel.SetRow(this.WorkItemTypeDropEdit, 0);
			this.WorkItemTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(227, 20, true);
			this.WorkItemTypeDropEdit.TabIndex = 1;
			// 
			// WorkItemDetailsControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.DetailsGroupBox);
			this.Name = "WorkItemDetailsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(369, 230, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			DetailsGroupBox.ResumeLayout(false);
			DetailsGroupBox.PerformLayout();
			WorkItemDetailsPanel.ResumeLayout(false);
			WorkItemDetailsPanel.PerformLayout();
			CompanyFindBox.ResumeLayout(false);
			CompanyFindBox.PerformLayout();
			PortOrCountryFindBox.ResumeLayout(false);
			PortOrCountryFindBox.PerformLayout();
			DepartmentFindBox.ResumeLayout(false);
			DepartmentFindBox.PerformLayout();
			PriorityDropEdit.ResumeLayout(false);
			PriorityDropEdit.PerformLayout();
			ActivitySubTypeDropEdit.ResumeLayout(false);
			ActivitySubTypeDropEdit.PerformLayout();
			ActivityTypeDropEdit.ResumeLayout(false);
			ActivityTypeDropEdit.PerformLayout();
			WorkItemAreaDropEdit.ResumeLayout(false);
			WorkItemAreaDropEdit.PerformLayout();
			WorkItemTypeDropEdit.ResumeLayout(false);
			WorkItemTypeDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox DetailsGroupBox;
		private ZArchitecture.GUI.ZDropEdit WorkItemTypeDropEdit;
		private ZArchitecture.GUI.ZDropEdit PriorityDropEdit;
		private ZArchitecture.GUI.ZDropEdit ActivitySubTypeDropEdit;
		private ZArchitecture.GUI.ZDropEdit ActivityTypeDropEdit;
		private ZArchitecture.GUI.ZDropEdit WorkItemAreaDropEdit;
		private CargoWise.Windows.UI.Layout.RowLayoutPanel WorkItemDetailsPanel;
		private ZGuidFindBox CompanyFindBox;
		private ZCodeFindBox PortOrCountryFindBox;
		private ZGuidFindBox DepartmentFindBox;
	}
}
