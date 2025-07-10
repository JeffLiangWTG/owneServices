using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.GUI
{
	public partial class RatingTabControl
	{
		private ZTabPage TemplateTabPage;
		private ZTabPage SummaryTabPage;
		private ZLogsTabPage zEventTabPage1;
		private ZTemplateTabControl TabControl;
		private ZStmNoteTabPage zStmNoteTabPage1;
		private ZTabPage DocumentFormatTabPage;
		private ZCheckBox PrintDestinationCheckBox;
		private ZCheckBox PrintOriginCheckBox;
		private SummaryEntryPanel summaryEntryPanel1;
		private RateEntryPanelWithRelatedLines rateEntryPanelWithRelatedLines1;
		public ZWorkflowTabPage WorkflowTabPage;

		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.TemplateTabPage = new ZTabPage();
			this.rateEntryPanelWithRelatedLines1 = new RateEntryPanelWithRelatedLines();
			this.SummaryTabPage = new ZTabPage();
			this.summaryEntryPanel1 = new SummaryEntryPanel();
			this.zEventTabPage1 = new ZLogsTabPage();
			this.TabControl = new ZTemplateTabControl();
			this.DocumentFormatTabPage = new ZTabPage();
			this.PrintDestinationCheckBox = new ZCheckBox();
			this.PrintOriginCheckBox = new ZCheckBox();
			this.WorkflowTabPage = new ZWorkflowTabPage();
			this.zStmNoteTabPage1 = new ZStmNoteTabPage();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.TemplateTabPage.SuspendLayout();
			this.SummaryTabPage.SuspendLayout();
			this.TabControl.SuspendLayout();
			this.DocumentFormatTabPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Business.ClientRate);
			// 
			// TemplateTabPage
			// 
			this.TemplateTabPage.Controls.Add(this.rateEntryPanelWithRelatedLines1);
			this.TemplateTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TemplateTabPage.Name = "TemplateTabPage";
			this.TemplateTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 582, true);
			this.TemplateTabPage.TabIndex = 0;
			this.TemplateTabPage.Text = "Template";
			// 
			// rateEntryPanelWithRelatedLines1
			// 
			this.rateEntryPanelWithRelatedLines1.AllowDrop = true;
			this.rateEntryPanelWithRelatedLines1.Anchor = (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right);
			this.BindingSource.SetBindingMember(this.rateEntryPanelWithRelatedLines1, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(null);
			this.rateEntryPanelWithRelatedLines1.Category = "";
			this.rateEntryPanelWithRelatedLines1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.rateEntryPanelWithRelatedLines1.Name = "rateEntryPanelWithRelatedLines1";
			this.rateEntryPanelWithRelatedLines1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 582, true);
			this.rateEntryPanelWithRelatedLines1.TabIndex = 0;
			// 
			// SummaryTabPage
			// 
			this.SummaryTabPage.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("RatingTabControl|eb4dba1b-2456-4f27-bf15-07c8f84a8e70", "Rate Summary");
			this.SummaryTabPage.Controls.Add(this.summaryEntryPanel1);
			this.SummaryTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SummaryTabPage.Name = "SummaryTabPage";
			this.SummaryTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 582, true);
			this.SummaryTabPage.TabIndex = 5;
			// 
			// summaryEntryPanel1
			// 
			this.summaryEntryPanel1.AllowDrop = true;
			this.summaryEntryPanel1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom
			| System.Windows.Forms.AnchorStyles.Left
			| System.Windows.Forms.AnchorStyles.Right;
			this.BindingSource.SetBindingMember(this.summaryEntryPanel1, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(null);
			this.summaryEntryPanel1.Category = "";
			this.summaryEntryPanel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.summaryEntryPanel1.Name = "summaryEntryPanel1";
			this.summaryEntryPanel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 582, true);
			this.summaryEntryPanel1.TabIndex = 0;
			this.summaryEntryPanel1.Tag = "SMR";
			// 
			// zEventTabPage1
			// 
			this.zEventTabPage1.ExcludeFromBindingOnSave = true;
			this.zEventTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zEventTabPage1.Name = "zEventTabPage1";
			this.zEventTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 582, true);
			this.zEventTabPage1.TabIndex = 7;
			// 
			// TabControl
			// 
			this.TabControl.Controls.Add(this.TemplateTabPage);
			this.TabControl.Controls.Add(this.SummaryTabPage);
			this.TabControl.Controls.Add(this.DocumentFormatTabPage);
			this.TabControl.Controls.Add(this.WorkflowTabPage);
			this.TabControl.Controls.Add(this.zStmNoteTabPage1);
			this.TabControl.Controls.Add(this.zEventTabPage1);
			this.TabControl.ItemSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 19, true);
			this.TabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TabControl.Name = "TabControl";
			this.TabControl.SelectedIndex = 0;
			this.TabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 582, true);
			this.TabControl.TabIndex = 0;
			// 
			// DocumentFormatTabPage
			// 
			this.DocumentFormatTabPage.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("RatingTabControl|8c3b77c5-7260-4284-b958-f248ed0c9cab", "Document Format");
			this.DocumentFormatTabPage.Controls.Add(this.PrintDestinationCheckBox);
			this.DocumentFormatTabPage.Controls.Add(this.PrintOriginCheckBox);
			this.DocumentFormatTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DocumentFormatTabPage.Name = "DocumentFormatTabPage";
			this.DocumentFormatTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 582, true);
			this.DocumentFormatTabPage.TabIndex = 9;
			// 
			// PrintDestinationCheckBox
			// 
			this.PrintDestinationCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.PrintDestinationCheckBox, "TH_PrintRateLevelDestinationCharges");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Business.ClientRate)(null)).TH_PrintRateLevelDestinationCharges);
			this.PrintDestinationCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.PrintDestinationCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 48, true);
			this.PrintDestinationCheckBox.Name = "PrintDestinationCheckBox";
			this.PrintDestinationCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.PrintDestinationCheckBox.TabIndex = 10;
			// 
			// PrintOriginCheckBox
			// 
			this.PrintOriginCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.PrintOriginCheckBox, "TH_PrintRateLevelOriginCharges");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Business.ClientRate)(null)).TH_PrintRateLevelOriginCharges);
			this.PrintOriginCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.PrintOriginCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 16, true);
			this.PrintOriginCheckBox.Name = "PrintOriginCheckBox";
			this.PrintOriginCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.PrintOriginCheckBox.TabIndex = 9;
			// 
			// WorkflowTabPage
			// 
			this.WorkflowTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.WorkflowTabPage.Name = "WorkflowTabPage";
			this.WorkflowTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 582, true);
			this.WorkflowTabPage.TabIndex = 10;
			// 
			// zStmNoteTabPage1
			// 
			this.zStmNoteTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zStmNoteTabPage1.Name = "zStmNoteTabPage1";
			this.zStmNoteTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 582, true);
			this.zStmNoteTabPage1.TabIndex = 8;
			// 
			// RatingTabControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.TabControl);
			this.Name = "RatingTabControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 582, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.TemplateTabPage.ResumeLayout(false);
			this.SummaryTabPage.ResumeLayout(false);
			this.TabControl.ResumeLayout(false);
			this.DocumentFormatTabPage.ResumeLayout(false);
			this.DocumentFormatTabPage.PerformLayout();
			this.ResumeLayout(false);
		}
	}
}
