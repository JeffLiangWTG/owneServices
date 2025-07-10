using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ProcessManagement.GUI
{
	partial class ProjectLogControl : ZUserControl
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
			this.ProjectTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.LogTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.LogTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SalesRelationsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.salesRelationControl = new Enterprise.MasterFiles.GUI.SalesRelationControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ProjectTabControl.SuspendLayout();
			this.LogTabPage.SuspendLayout();
			this.SalesRelationsTabPage.SuspendLayout();
			this.salesRelationControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.ProcessManagement.Business.Project);

			// 
			// ProjectTabControl
			// 
			this.ProjectTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)));
			this.ProjectTabControl.Controls.Add(this.LogTabPage);
			this.ProjectTabControl.Controls.Add(this.SalesRelationsTabPage);
			this.ProjectTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ProjectTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ProjectTabControl.Name = "ProjectTabControl";
			this.ProjectTabControl.SelectedIndex = 0;
			this.ProjectTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(762, 328, true);
			this.ProjectTabControl.TabIndex = 5;
			this.ProjectTabControl.TabStop = false;
			// 
			// LogTabPage
			// 
			this.LogTabPage.CaptionResourceString = Enterprise.ProcessManagement.GUI.Res.GetData("ProjectForm|886ped34-e6ba-4eaf-9136-f2fa1453fd5f", "Project Log");
			this.LogTabPage.Controls.Add(this.LogTextBox);
			this.LogTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.LogTabPage.Name = "LogTabPage";
			this.LogTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(762, 241, true);
			this.LogTabPage.TabIndex = 0;
			// 
			// 
			// LogTextBox
			// 
			this.BindingSource.SetBindingMember(this.LogTextBox, "LogText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ProcessManagement.Business.Project)(null)).LogText)));
			this.LogTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LogTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.LogTextBox, false);
			this.LogTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 14, true);
			this.LogTextBox.Multiline = true;
			this.LogTextBox.Name = "LogTextBox";
			this.LogTextBox.ReadOnly = true;
			this.LogTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(453, 312, true);
			this.LogTextBox.TabIndex = 0;
			// 
			// SalesRelationsTabPage
			// 
			this.SalesRelationsTabPage.CaptionResourceString = Enterprise.ProcessManagement.GUI.Res.GetData("b8970ae9-bd00-4112-a059-8af3a94237c2", "Sales Relations");
			this.SalesRelationsTabPage.Controls.Add(this.salesRelationControl);
			this.SalesRelationsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.SalesRelationsTabPage.Name = "SalesRelationsTabPage";
			this.SalesRelationsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(762, 241, true);
			this.SalesRelationsTabPage.TabIndex = 7;
			// 
			// salesRelationControl
			// 
			this.salesRelationControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.salesRelationControl, "SalesRelationModel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.SalesRelationModel)(((Enterprise.MasterFiles.Business.OrgOpportunity)(null)).SalesRelationModel)));
			this.salesRelationControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.salesRelationControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.salesRelationControl.Name = "salesRelationControl";
			this.salesRelationControl.NameOfATreeElement = Enterprise.ProcessManagement.GUI.Res.GetData("e0pdcf6a-9135-4f4d-b8a1-f1asad65448c", "Relatable Activity");
			this.salesRelationControl.NameOfTreeElementsPlural = Enterprise.ProcessManagement.GUI.Res.GetData("d5f80a0a-7663-4373-9277-cqb79a2c9f65", "Relatable Activities");
			this.salesRelationControl.ShowPopupButton = false;
			this.salesRelationControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(762, 241, true);
			this.salesRelationControl.TabIndex = 0;
			// 
			// ProjectLogControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ProjectTabControl);
			this.Name = "ProjectLogControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(456, 328, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ProjectTabControl.ResumeLayout(false);
			this.ProjectTabControl.PerformLayout();
			this.LogTabPage.ResumeLayout(false);
			this.LogTabPage.PerformLayout();
			this.SalesRelationsTabPage.ResumeLayout(false);
			this.SalesRelationsTabPage.PerformLayout();
			this.salesRelationControl.ResumeLayout(true);
			this.salesRelationControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.ZTextBox LogTextBox;
		internal Enterprise.ZArchitecture.GUI.ZTemplateTabControl ProjectTabControl;
		private Enterprise.ZArchitecture.GUI.ZTabPage LogTabPage;
		private ZTabPage SalesRelationsTabPage;
		private SalesRelationControl salesRelationControl;
	}
}
