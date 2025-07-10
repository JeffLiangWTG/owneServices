using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.GUI
{
	public partial class CostingTabControl
	{
		private ZTemplateTabControl TabControl;
		private ZTabPage SummaryTabPage;
		private ZLogsTabPage zEventTabPage1;
		private ZTabPage TemplateTabPage;
		private ZStmNoteTabPage zStmNoteTabPage1;
		private SummaryEntryPanel summaryEntryPanel1;
		private RateEntryPanelWithRelatedLines rateEntryPanel1;

		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.TabControl = new ZTemplateTabControl();
			this.TemplateTabPage = new ZTabPage();
			this.rateEntryPanel1 = new RateEntryPanelWithRelatedLines();
			this.SummaryTabPage = new ZTabPage();
			this.summaryEntryPanel1 = new SummaryEntryPanel();
			this.zStmNoteTabPage1 = new ZStmNoteTabPage();
			this.zEventTabPage1 = new ZLogsTabPage();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.TabControl.SuspendLayout();
			this.TemplateTabPage.SuspendLayout();
			this.SummaryTabPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Business.Costing);
			// 
			// TabControl
			// 
			this.TabControl.Controls.Add(this.TemplateTabPage);
			this.TabControl.Controls.Add(this.SummaryTabPage);
			this.TabControl.Controls.Add(this.zStmNoteTabPage1);
			this.TabControl.Controls.Add(this.zEventTabPage1);
			this.TabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TabControl.Name = "TabControl";
			this.TabControl.SelectedIndex = 0;
			this.TabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 582, true);
			this.TabControl.TabIndex = 0;
			// 
			// TemplateTabPage
			// 
			this.TemplateTabPage.Controls.Add(this.rateEntryPanel1);
			this.TemplateTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TemplateTabPage.Name = "TemplateTabPage";
			this.TemplateTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 582, true);
			this.TemplateTabPage.TabIndex = 0;
			this.TemplateTabPage.Text = "Template";
			// 
			// rateEntryPanel1
			// 
			this.rateEntryPanel1.AllowDrop = true;
			this.rateEntryPanel1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom
			| System.Windows.Forms.AnchorStyles.Left
			| System.Windows.Forms.AnchorStyles.Right;
			this.BindingSource.SetBindingMember(this.rateEntryPanel1, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(null);
			this.rateEntryPanel1.Category = "";
			this.rateEntryPanel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.rateEntryPanel1.Name = "rateEntryPanel1";
			this.rateEntryPanel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 582, true);
			this.rateEntryPanel1.TabIndex = 0;
			// 
			// SummaryTabPage
			// 
			this.SummaryTabPage.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("CostingTabControl|4d907782-c53d-48f5-9abd-a29216f7027c", "Rate Summary");
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
			// zStmNoteTabPage1
			// 
			this.zStmNoteTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zStmNoteTabPage1.Name = "zStmNoteTabPage1";
			this.zStmNoteTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 582, true);
			this.zStmNoteTabPage1.TabIndex = 8;
			// 
			// zEventTabPage1
			// 
			this.zEventTabPage1.ExcludeFromBindingOnSave = true;
			this.zEventTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zEventTabPage1.Name = "zEventTabPage1";
			this.zEventTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 582, true);
			this.zEventTabPage1.TabIndex = 7;
			// 
			// CostingTabControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.TabControl);
			this.Name = "CostingTabControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 582, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.TabControl.ResumeLayout(false);
			this.TemplateTabPage.ResumeLayout(false);
			this.SummaryTabPage.ResumeLayout(false);
			this.ResumeLayout(false);
		}
	}
}
