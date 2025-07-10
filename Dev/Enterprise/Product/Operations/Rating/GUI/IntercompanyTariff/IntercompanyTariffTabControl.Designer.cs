using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.GUI
{
	public partial class IntercompanyTariffTabControl
	{
		private ZTemplateTabControl TabControl;
		private ZTabPage SummaryTabPage;
		private ZLogsTabPage zEventTabPage;
		private ZTabPage TemplateTabPage;
		private ZStmNoteTabPage zStmNoteTabPage;
		private SummaryEntryPanel summaryEntryPanel;
		private RateEntryPanelWithRelatedLines rateEntryPanel;

		private void InitializeComponent()
		{
			this.TabControl = new ZTemplateTabControl();
			this.TemplateTabPage = new ZTabPage();
			this.rateEntryPanel = new RateEntryPanelWithRelatedLines();
			this.SummaryTabPage = new ZTabPage();
			this.summaryEntryPanel = new SummaryEntryPanel();
			this.zStmNoteTabPage = new ZStmNoteTabPage();
			this.zEventTabPage = new ZLogsTabPage();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.TabControl.SuspendLayout();
			this.TemplateTabPage.SuspendLayout();
			this.SummaryTabPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(IntercompanyTariff);
			// 
			// TabControl
			// 
			this.TabControl.Controls.Add(this.TemplateTabPage);
			this.TabControl.Controls.Add(this.SummaryTabPage);
			this.TabControl.Controls.Add(this.zStmNoteTabPage);
			this.TabControl.Controls.Add(this.zEventTabPage);
			this.TabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TabControl.Name = "TabControl";
			this.TabControl.SelectedIndex = 0;
			this.TabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 582, true);
			this.TabControl.TabIndex = 0;
			// 
			// TemplateTabPage
			// 
			this.TemplateTabPage.Controls.Add(this.rateEntryPanel);
			this.TemplateTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TemplateTabPage.Name = "TemplateTabPage";
			this.TemplateTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 582, true);
			this.TemplateTabPage.TabIndex = 0;
			this.TemplateTabPage.Text = "Template";
			// 
			// rateEntryPanel
			// 
			this.rateEntryPanel.AllowDrop = true;
			this.rateEntryPanel.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom
			| System.Windows.Forms.AnchorStyles.Left
			| System.Windows.Forms.AnchorStyles.Right;
			this.BindingSource.SetBindingMember(this.rateEntryPanel, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(null);
			this.rateEntryPanel.Category = "";
			this.rateEntryPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.rateEntryPanel.Name = "rateEntryPanel";
			this.rateEntryPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 582, true);
			this.rateEntryPanel.TabIndex = 0;
			// 
			// SummaryTabPage
			// 
			this.SummaryTabPage.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("IntercompanyTariffTabControl|4d907782-c53d-48f5-9abd-a29216f7027c", "Rate Summary");
			this.SummaryTabPage.Controls.Add(this.summaryEntryPanel);
			this.SummaryTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SummaryTabPage.Name = "SummaryTabPage";
			this.SummaryTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 582, true);
			this.SummaryTabPage.TabIndex = 5;
			// 
			// summaryEntryPanel
			// 
			this.summaryEntryPanel.AllowDrop = true;
			this.summaryEntryPanel.Anchor = (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right);
			this.BindingSource.SetBindingMember(this.summaryEntryPanel, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(null);
			this.summaryEntryPanel.Category = "";
			this.summaryEntryPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.summaryEntryPanel.Name = "summaryEntryPanel";
			this.summaryEntryPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 582, true);
			this.summaryEntryPanel.TabIndex = 0;
			this.summaryEntryPanel.Tag = "SMR";
			// 
			// zStmNoteTabPage
			// 
			this.zStmNoteTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zStmNoteTabPage.Name = "zStmNoteTabPage";
			this.zStmNoteTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 582, true);
			this.zStmNoteTabPage.TabIndex = 8;
			// 
			// zEventTabPage
			// 
			this.zEventTabPage.ExcludeFromBindingOnSave = true;
			this.zEventTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zEventTabPage.Name = "zEventTabPage";
			this.zEventTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 582, true);
			this.zEventTabPage.TabIndex = 7;
			// 
			// IntercompanyTariffTabControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.TabControl);
			this.Name = "IntercompanyTariffTabControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 582, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.TabControl.ResumeLayout(false);
			this.TemplateTabPage.ResumeLayout(false);
			this.SummaryTabPage.ResumeLayout(false);
			this.ResumeLayout(false);
		}
	}
}
