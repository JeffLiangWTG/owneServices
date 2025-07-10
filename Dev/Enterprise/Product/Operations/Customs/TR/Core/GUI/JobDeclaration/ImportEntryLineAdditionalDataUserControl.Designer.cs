namespace Enterprise.Customs.TR.GUI
{
	public partial class ImportEntryLineAdditionalDataUserControl
	{
		void InitializeComponent()
		{
			this.TariffQuestionsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.TariffQuestionsUserControl = new Enterprise.Customs.TR.GUI.TariffQuestionsUserControl();
			this.ExtendInfoTabControl.SuspendLayout();
			this.SupportingDocumentsTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.EntryLineSupportingDocumentsGrid)).BeginInit();
			this.EntryLineSupportingDocumentsGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.TariffQuestionsTabPage.SuspendLayout();
			this.TariffQuestionsUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// ExtendInfoTabControl
			// 
			this.ExtendInfoTabControl.Controls.Add(this.TariffQuestionsTabPage);
			this.ExtendInfoTabControl.Controls.SetChildIndex(this.TariffQuestionsTabPage, 0);
			this.ExtendInfoTabControl.Controls.SetChildIndex(this.SupportingDocumentsTabPage, 0);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.TR.Business.Declaration.JobDeclaration);
			// 
			// TariffQuestionsUserControl
			// 
			this.TariffQuestionsUserControl.AllowDrop = true;
			//this.BindingSource.SetBindingMember(this.TariffQuestionsUserControl, "CusEntryLine");
			this.BindingSource.SetBindingMember(this.TariffQuestionsUserControl, "CustomsEntryHeaders.AllEntryLines");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(((Enterprise.Customs.TR.Business.Declaration.JobDeclaration)(null)))));
			this.TariffQuestionsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.TariffQuestionsUserControl.Name = "TariffQuestionsUserControl";
			this.TariffQuestionsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(391, 311, true);
			this.TariffQuestionsUserControl.TabIndex = 0;
			this.TariffQuestionsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;	
			// 
			// TariffQuestionsTabPage
			// 
			this.TariffQuestionsTabPage.CaptionResourceString = Enterprise.Customs.TR.GUI.Res.GetData("1ccc3b55-690e-4598-851b-759e090e162f", "Tariff Questions");
			this.TariffQuestionsTabPage.Controls.Add(this.TariffQuestionsUserControl);
			this.TariffQuestionsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.TariffQuestionsTabPage.Name = "TariffQuestionsTabPage";
			this.TariffQuestionsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.TariffQuestionsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(828, 184, true);
			this.TariffQuestionsTabPage.TabIndex = 3;
			this.TariffQuestionsTabPage.Text = Enterprise.Customs.TR.GUI.Res.GetString("678045FF-459D-4EB7-BB83-0E65C48D271A", "Tariff Questions");
			this.TariffQuestionsTabPage.UseVisualStyleBackColor = true;
			// 
			// ImportEntryLineAdditionalDataUserControl
			//
			this.CaptionRenderingEnabled = true;
			this.Name = "ImportEntryLineAdditionalDataUserControl";
			this.ExtendInfoTabControl.ResumeLayout(false);
			this.ExtendInfoTabControl.PerformLayout();
			this.SupportingDocumentsTabPage.ResumeLayout(false);
			this.SupportingDocumentsTabPage.PerformLayout();
			this.TariffQuestionsUserControl.ResumeLayout(true);
			this.TariffQuestionsUserControl.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.EntryLineSupportingDocumentsGrid)).EndInit();
			this.EntryLineSupportingDocumentsGrid.ResumeLayout(false);
			this.EntryLineSupportingDocumentsGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.TariffQuestionsTabPage.ResumeLayout(false);
			this.TariffQuestionsTabPage.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		private ZArchitecture.GUI.ZTabPage TariffQuestionsTabPage;
		internal Enterprise.Customs.TR.GUI.TariffQuestionsUserControl TariffQuestionsUserControl;
	}
}
