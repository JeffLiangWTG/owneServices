namespace Enterprise.Customs.TR.GUI
{
	public partial class TariffQuestionsUserControl
	{
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.TariffQuestionsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.TariffQuestionsGrid)).BeginInit();
			this.TariffQuestionsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.TR.Business.Declaration.ICusEntryCPDecParent);
			// 
			// TariffQuestionsGrid
			// 
			this.TariffQuestionsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.TariffQuestionsGrid, "CPDecCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.TR.Business.Declaration.ICusEntryCPDecParent)(null)).CPDecCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.Business.Declaration.CusEntryCPDec)(((System.Collections.IList)(((Enterprise.Customs.TR.Business.Declaration.ICusEntryCPDecParent)(null)).CPDecCollection)).SyncRoot)).ON_QuestionType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.Business.Declaration.CusEntryCPDec)(((System.Collections.IList)(((Enterprise.Customs.TR.Business.Declaration.ICusEntryCPDecParent)(null)).CPDecCollection)).SyncRoot)).QuestionTypeDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TR.Business.Declaration.CusEntryCPDec)(((System.Collections.IList)(((Enterprise.Customs.TR.Business.Declaration.ICusEntryCPDecParent)(null)).CPDecCollection)).SyncRoot)).ON_CPDecNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.Business.Declaration.CusEntryCPDec)(((System.Collections.IList)(((Enterprise.Customs.TR.Business.Declaration.ICusEntryCPDecParent)(null)).CPDecCollection)).SyncRoot)).ON_AnswerCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.Business.Declaration.CusEntryCPDec)(((System.Collections.IList)(((Enterprise.Customs.TR.Business.Declaration.ICusEntryCPDecParent)(null)).CPDecCollection)).SyncRoot)).Description)));
			this.TariffQuestionsGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.TR.GUI.Res.GetData("4bb98720-207f-4444-9d2a-fc03c134987c", "Type");
			zDropEditColumnStyleInfo1.ColumnName = "ON_QuestionType";
			zDropEditColumnStyleInfo1.GroupName = Enterprise.Customs.TR.GUI.Res.GetData("62237269-8be7-4671-9703-481f7b47a074", "Question Type");
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo1.ColumnName = "QuestionTypeDescription";
			zTextBoxColumnStyleInfo1.GroupName = Enterprise.Customs.TR.GUI.Res.GetData("62237269-8be7-4671-9703-481f7b47a074", "Question Type");
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(75);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.TR.GUI.Res.GetData("0ac4eebd-6680-4da3-aef7-7be734dfb960", "Code");
			zCalcEditColumnStyleInfo1.ColumnName = "ON_CPDecNum";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.MaxValue = 99999;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.TR.GUI.Res.GetData("9dc522fa-c266-4e6b-ae83-b71d14d94fa7", "Answer");
			zDropEditColumnStyleInfo2.ColumnName = "ON_AnswerCode";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.TR.GUI.Res.GetData("5934ba83-a406-4ff6-a558-ac7b7e6a7670", "Description");
			zTextBoxColumnStyleInfo2.ColumnName = "Description";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(500);
			this.TariffQuestionsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.TariffQuestionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.TariffQuestionsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.TariffQuestionsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.TariffQuestionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.TariffQuestionsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TariffQuestionsGrid.GridId = "65dfe8f0-5f77-4f2b-92c9-13c660e1fa51";
			this.TariffQuestionsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.TariffQuestionsGrid.LayoutKey = "TariffQuestionsGrid";
			this.TariffQuestionsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TariffQuestionsGrid.Name = "TariffQuestionsGrid";
			this.TariffQuestionsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(538, 262, true);
			this.TariffQuestionsGrid.TabIndex = 0;
			// 
			// TariffQuestionsUserControl
			//
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.TariffQuestionsGrid);
			this.Name = "TariffQuestionsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(538, 262, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.TariffQuestionsGrid)).EndInit();
			this.TariffQuestionsGrid.ResumeLayout(false);
			this.TariffQuestionsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		protected ZArchitecture.ZGrid TariffQuestionsGrid;
	}
}
