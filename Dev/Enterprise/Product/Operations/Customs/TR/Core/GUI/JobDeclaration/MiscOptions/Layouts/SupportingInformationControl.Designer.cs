namespace Enterprise.Customs.TR.GUI
{
	partial class SupportingInformationControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.TariffQuestionsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.TariffQuestionsUserControl = new Enterprise.Customs.TR.GUI.TariffQuestionsUserControl();
			this.SupportingInformationTabControl.SuspendLayout();
			this.SupportingDocumentTabPage.SuspendLayout();
			this.AdditionalInfoTabPage.SuspendLayout();
			this.PreviousDocumentTabPage.SuspendLayout();
			this.GuaranteesTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.TariffQuestionsTabPage.SuspendLayout();
			this.TariffQuestionsUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// SupportingInformationTabControl
			// 
			this.SupportingInformationTabControl.Controls.Add(this.TariffQuestionsTabPage);
			this.SupportingInformationTabControl.Controls.SetChildIndex(this.GuaranteesTabPage, 0);
			this.SupportingInformationTabControl.Controls.SetChildIndex(this.PreviousDocumentTabPage, 0);
			this.SupportingInformationTabControl.Controls.SetChildIndex(this.AdditionalInfoTabPage, 0);
			this.SupportingInformationTabControl.Controls.SetChildIndex(this.SupportingDocumentTabPage, 0);
			this.SupportingInformationTabControl.Controls.SetChildIndex(this.TariffQuestionsTabPage, 0);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.TR.Business.Declaration.JobDeclaration);
			// 
			// TariffQuestionsTabPage
			// 
			this.TariffQuestionsTabPage.CaptionResourceString = Enterprise.Customs.TR.GUI.Res.GetData("A78F61AB-8882-4DD2-BCC1-55DB3BC2B05C", "Entry Questions");
			this.TariffQuestionsTabPage.Controls.Add(this.TariffQuestionsUserControl);
			this.TariffQuestionsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.TariffQuestionsTabPage.Name = "TariffQuestionsTabPage";
			this.TariffQuestionsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.TariffQuestionsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(377, 278, true);
			this.TariffQuestionsTabPage.TabIndex = 3;
			this.TariffQuestionsTabPage.UseVisualStyleBackColor = true;
			// 
			// TariffQuestionsUserControl
			// 
			this.TariffQuestionsUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TariffQuestionsUserControl, "CusEntryHeader");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.TR.Business.Declaration.ICusEntryCPDecParent)(((Enterprise.Customs.TR.Business.Declaration.JobDeclaration)(null)).CusEntryHeader)));
			this.TariffQuestionsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TariffQuestionsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.TariffQuestionsUserControl.Name = "TariffQuestionsUserControl";
			this.TariffQuestionsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(371, 272, true);
			this.TariffQuestionsUserControl.TabIndex = 0;
			// 
			// SupportingInformationControl
			// 
			this.Name = "SupportingInformationControl";
			this.SupportingInformationTabControl.ResumeLayout(false);
			this.SupportingInformationTabControl.PerformLayout();
			this.SupportingDocumentTabPage.ResumeLayout(false);
			this.SupportingDocumentTabPage.PerformLayout();
			this.AdditionalInfoTabPage.ResumeLayout(false);
			this.AdditionalInfoTabPage.PerformLayout();
			this.PreviousDocumentTabPage.ResumeLayout(false);
			this.PreviousDocumentTabPage.PerformLayout();
			this.GuaranteesTabPage.ResumeLayout(false);
			this.GuaranteesTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.TariffQuestionsTabPage.ResumeLayout(false);
			this.TariffQuestionsTabPage.PerformLayout();
			this.TariffQuestionsUserControl.ResumeLayout(true);
			this.TariffQuestionsUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		protected ZArchitecture.GUI.ZTabPage TariffQuestionsTabPage;
		private TR.GUI.TariffQuestionsUserControl TariffQuestionsUserControl;
	}
}
