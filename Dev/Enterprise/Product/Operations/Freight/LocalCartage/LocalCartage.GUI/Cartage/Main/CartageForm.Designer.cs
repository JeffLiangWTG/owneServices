using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.LocalCartage.GUI
{
	partial class CartageForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			this.LooseMovesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.looseMovesControl1 = new Enterprise.Freight.LocalCartage.GUI.LooseMovesControl();
			this.CartageLegsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.cartageLegsControl1 = new Enterprise.Freight.LocalCartage.GUI.CartageLegsControl();
			this.ContainersTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.containerDetailsControl1 = new Enterprise.Freight.LocalCartage.GUI.ContainerDetailsControl();
			this.WorkflowTabPage = new Enterprise.MasterFiles.GUI.ZWorkflowTabPage();
			this.CartageControl = new Enterprise.Freight.LocalCartage.GUI.CartageUserControl();
			this.ReferencesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ReferencesNumbersControl = new Enterprise.MasterFiles.GUI.NumbersControl();
			this.CustomFieldsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.cartageCustomFieldsControl1 = new Enterprise.Freight.LocalCartage.GUI.CartageCustomFieldsControl();
			this.relatedJobsTabPage = new Enterprise.ZArchitecture.GUI.RelatedJobsTabPage();
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			this.NotesTabPage.SuspendLayout();
			this.MainPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.LooseMovesTabPage.SuspendLayout();
			this.looseMovesControl1.SuspendLayout();
			this.CartageLegsTabPage.SuspendLayout();
			this.cartageLegsControl1.SuspendLayout();
			this.ContainersTabPage.SuspendLayout();
			this.containerDetailsControl1.SuspendLayout();
			this.WorkflowTabPage.SuspendLayout();
			this.CartageControl.SuspendLayout();
			this.ReferencesTabPage.SuspendLayout();
			this.ReferencesNumbersControl.SuspendLayout();
			this.CustomFieldsTabPage.SuspendLayout();
			this.cartageCustomFieldsControl1.SuspendLayout();
			this.relatedJobsTabPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Controls.Add(this.ContainersTabPage);
			this.MainTabControl.Controls.Add(this.LooseMovesTabPage);
			this.MainTabControl.Controls.Add(this.CartageLegsTabPage);
			this.MainTabControl.Controls.Add(this.ReferencesTabPage);
			this.MainTabControl.Controls.Add(this.WorkflowTabPage);
			this.MainTabControl.Controls.Add(this.CustomFieldsTabPage);
			this.MainTabControl.Controls.Add(this.relatedJobsTabPage);
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1057, 630, true);
			this.MainTabControl.Controls.SetChildIndex(this.LogsTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.NotesTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.relatedJobsTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.CustomFieldsTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.WorkflowTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.ReferencesTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.CartageLegsTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.LooseMovesTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.ContainersTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.MainTabPage, 0);
			// 
			// MainTabPage
			// 
			this.MainTabPage.Controls.Add(this.CartageControl);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1049, 582, true);
			// 
			// NotesTabPage
			// 
			this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1049, 582, true);
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1049, 582, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1057, 630, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1057, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.LocalCartage.Business.CommonCartage);
			// 
			// LooseMovesTabPage
			// 
			this.LooseMovesTabPage.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("CartageForm|13F8E166-1521-4a41-8968-DA372AD60F24", "Loose Moves");
			this.LooseMovesTabPage.Controls.Add(this.looseMovesControl1);
			this.LooseMovesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.LooseMovesTabPage.Name = "LooseMovesTabPage";
			this.LooseMovesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1049, 582, true);
			this.LooseMovesTabPage.TabIndex = 3;
			// 
			// looseMovesControl1
			// 
			this.looseMovesControl1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.looseMovesControl1, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Freight.LocalCartage.Business.CommonCartage)(((Enterprise.Freight.LocalCartage.Business.CommonCartage)(null)))));
			this.looseMovesControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.looseMovesControl1.Name = "looseMovesControl1";
			this.looseMovesControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(360, 587, true);
			this.looseMovesControl1.TabIndex = 0;
			// 
			// CartageLegsTabPage
			// 
			this.CartageLegsTabPage.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("CartageForm|A3972AC8-91B0-415b-AF1F-AE5A59A0DBA6", "Port Transport Legs");
			this.CartageLegsTabPage.Controls.Add(this.cartageLegsControl1);
			this.CartageLegsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.CartageLegsTabPage.Name = "CartageLegsTabPage";
			this.CartageLegsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1049, 582, true);
			this.CartageLegsTabPage.TabIndex = 4;
			// 
			// cartageLegsControl1
			// 
			this.cartageLegsControl1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.cartageLegsControl1, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Freight.LocalCartage.Business.CommonCartage)(((Enterprise.Freight.LocalCartage.Business.CommonCartage)(null)))));
			this.cartageLegsControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.cartageLegsControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.cartageLegsControl1.Name = "cartageLegsControl1";
			this.cartageLegsControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1049, 582, true);
			this.cartageLegsControl1.TabIndex = 0;
			// 
			// ContainersTabPage
			// 
			this.ContainersTabPage.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("CartageForm|B46E45C7-54C2-469c-A1F4-718E707CD87D", "Containers");
			this.ContainersTabPage.Controls.Add(this.containerDetailsControl1);
			this.ContainersTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ContainersTabPage.Name = "ContainersTabPage";
			this.ContainersTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1049, 582, true);
			this.ContainersTabPage.TabIndex = 5;
			// 
			// containerDetailsControl1
			// 
			this.containerDetailsControl1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.containerDetailsControl1, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Freight.LocalCartage.Business.CommonCartage)(((Enterprise.Freight.LocalCartage.Business.CommonCartage)(null)))));
			this.containerDetailsControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.containerDetailsControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.containerDetailsControl1.Name = "containerDetailsControl1";
			this.containerDetailsControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1049, 582, true);
			this.containerDetailsControl1.TabIndex = 0;
			// 
			// WorkflowTabPage
			// 
			this.WorkflowTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.WorkflowTabPage.Name = "WorkflowTabPage";
			this.WorkflowTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(842, 550, true);
			this.WorkflowTabPage.TabIndex = 6;
			// 
			// CartageControl
			// 
			this.CartageControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CartageControl, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Freight.LocalCartage.Business.CommonCartage)(((Enterprise.Freight.LocalCartage.Business.CommonCartage)(null)))));
			this.CartageControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CartageControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CartageControl.Name = "CartageControl";
			this.CartageControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1049, 582, true);
			this.CartageControl.TabIndex = 0;
			// 
			// ReferencesTabPage
			// 
			this.ReferencesTabPage.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("CartageForm|5562DEC0-C190-4195-BA09-1B4FC3197692", "References");
			this.ReferencesTabPage.Controls.Add(this.ReferencesNumbersControl);
			this.ReferencesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ReferencesTabPage.Name = "ReferencesTabPage";
			this.ReferencesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1049, 582, true);
			this.ReferencesTabPage.TabIndex = 7;
			// 
			// ReferencesNumbersControl
			// 
			this.ReferencesNumbersControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ReferencesNumbersControl, "AdditionalReferenceNumbers");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Integration.Customs.ICusEntryNumAdditionalReferenceCollection)(((Enterprise.Freight.LocalCartage.Business.CommonCartage)(null)).AdditionalReferenceNumbers)));
			this.ReferencesNumbersControl.DisplayDetailPanel = false;
			this.ReferencesNumbersControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ReferencesNumbersControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ReferencesNumbersControl.Name = "ReferencesNumbersControl";
			this.ReferencesNumbersControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1049, 582, true);
			this.ReferencesNumbersControl.TabIndex = 0;
			// 
			// CustomFieldsTabPage
			// 
			this.CustomFieldsTabPage.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("CartageForm|EFD67A0F-F9C8-457F-882F-8730D79295EE", "Additional Info");
			this.CustomFieldsTabPage.Controls.Add(this.cartageCustomFieldsControl1);
			this.CustomFieldsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.CustomFieldsTabPage.Name = "CustomFieldsTabPage";
			this.CustomFieldsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.CustomFieldsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1049, 603, true);
			this.CustomFieldsTabPage.TabIndex = 8;
			this.CustomFieldsTabPage.UseVisualStyleBackColor = true;
			// 
			// cartageCustomFieldsControl1
			// 
			this.cartageCustomFieldsControl1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.cartageCustomFieldsControl1, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Freight.LocalCartage.Business.CommonCartage)(((Enterprise.Freight.LocalCartage.Business.CommonCartage)(null)))));
			this.cartageCustomFieldsControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.cartageCustomFieldsControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.cartageCustomFieldsControl1.Name = "cartageCustomFieldsControl1";
			this.cartageCustomFieldsControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1043, 597, true);
			this.cartageCustomFieldsControl1.TabIndex = 0;
			// 
			// relatedJobsTabPage
			// 
			this.relatedJobsTabPage.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("CartageForm|131605d9-9b6b-485e-af6d-8c1b3b546bb3", "Related Jobs");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.ZArchitecture.Business.RelatedJobCollection)(((Enterprise.Freight.LocalCartage.Business.CommonCartage)(null)).RelatedJobs)));
			this.relatedJobsTabPage.ExcludeFromBindingOnSave = true;
			this.relatedJobsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.relatedJobsTabPage.Name = "relatedJobsTabPage";
			this.relatedJobsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(842, 550, true);
			this.relatedJobsTabPage.TabIndex = 9;
			// 
			// CartageForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1057, 686, true);
			this.DataSourceAssemblyName = "Enterprise.Freight.LocalCartage.Business";
			this.DataSourceType = typeof(Enterprise.Freight.LocalCartage.Business.CommonCartage);
			this.DataSourceTypeName = "Enterprise.Freight.LocalCartage.Business.CommonCartage";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1155, 725, true);
			this.Name = "CartageForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Text = "CartageLegForm";
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.MainTabPage.ResumeLayout(false);
			this.MainTabPage.PerformLayout();
			this.NotesTabPage.ResumeLayout(false);
			this.NotesTabPage.PerformLayout();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.LooseMovesTabPage.ResumeLayout(false);
			this.LooseMovesTabPage.PerformLayout();
			this.looseMovesControl1.ResumeLayout(true);
			this.looseMovesControl1.PerformLayout();
			this.CartageLegsTabPage.ResumeLayout(false);
			this.CartageLegsTabPage.PerformLayout();
			this.cartageLegsControl1.ResumeLayout(true);
			this.cartageLegsControl1.PerformLayout();
			this.ContainersTabPage.ResumeLayout(false);
			this.ContainersTabPage.PerformLayout();
			this.containerDetailsControl1.ResumeLayout(true);
			this.containerDetailsControl1.PerformLayout();
			this.WorkflowTabPage.ResumeLayout(false);
			this.WorkflowTabPage.PerformLayout();
			this.CartageControl.ResumeLayout(true);
			this.CartageControl.PerformLayout();
			this.ReferencesTabPage.ResumeLayout(false);
			this.ReferencesTabPage.PerformLayout();
			this.ReferencesNumbersControl.ResumeLayout(true);
			this.ReferencesNumbersControl.PerformLayout();
			this.CustomFieldsTabPage.ResumeLayout(false);
			this.CustomFieldsTabPage.PerformLayout();
			this.cartageCustomFieldsControl1.ResumeLayout(true);
			this.cartageCustomFieldsControl1.PerformLayout();
			this.relatedJobsTabPage.ResumeLayout(false);
			this.relatedJobsTabPage.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal Enterprise.ZArchitecture.GUI.ZTabPage LooseMovesTabPage;
		internal LooseMovesControl looseMovesControl1;
        internal Enterprise.ZArchitecture.GUI.ZTabPage ContainersTabPage;
        private Enterprise.ZArchitecture.GUI.ZTabPage CartageLegsTabPage;
        internal CartageLegsControl cartageLegsControl1;
        internal ContainerDetailsControl containerDetailsControl1;
		private Enterprise.MasterFiles.GUI.ZWorkflowTabPage WorkflowTabPage;
		internal CartageUserControl CartageControl;
		private ZArchitecture.GUI.ZTabPage ReferencesTabPage;
		private NumbersControl ReferencesNumbersControl;
		private ZArchitecture.GUI.ZTabPage CustomFieldsTabPage;
		private CartageCustomFieldsControl cartageCustomFieldsControl1;
		private RelatedJobsTabPage relatedJobsTabPage;
	}
}
