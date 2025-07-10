namespace Enterprise.MarketingManager.GUI
{
	partial class VoteResultsByQuestionUserControl
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
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo zMultiControlColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo();
			this.ResultsByItemsSplitContainer.Panel1.SuspendLayout();
			this.ResultsByItemsSplitContainer.Panel2.SuspendLayout();
			this.ResultsByItemsSplitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ResultItemsGrid)).BeginInit();
			this.AnswersTabControl.SuspendLayout();
			this.AnswersTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ResultItemAnswersGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// ResultsByItemsSplitContainer
			// 
			// 
			// AnswersTabPage
			// 
			this.AnswersTabPage.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("VoteResultsByQuestionUserControl|ae76baa6-39c8-4363-a0af-15505d6465d4", "Voters");
			// 
			// ResultItemsGrid
			// 
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("VoteResultsByQuestionUserControl|e111d2a9-2f94-41a6-9879-32b51ce1ce61", "Vote#");
			zCalcEditColumnStyleInfo1.ColumnName = "SubmittedAnswers+PopulatedAnswersCount";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			this.ResultItemsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			// 
			// ResultItemAnswersGrid
			// 
			zTextBoxColumnStyleInfo1.ColumnName = "CampaignItem+Recipient+Organisation+OH_Code";
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("VoteResultsByQuestionUserControl|38af19a1-bafb-40b4-bdf1-b894d8eb7ec7", "Phone");
			zTextBoxColumnStyleInfo2.ColumnName = "CampaignItem+WorkPhone";
			zTextBoxColumnStyleInfo3.ColumnName = "CampaignItem+Recipient+Organisation+ClosestPort+CountryCode+RN_Code";
			zTextBoxColumnStyleInfo3.IsVisible = false;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zMultiControlColumnStyleInfo1.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("VoteResultsByQuestionUserControl|c4179d12-7f67-4966-a566-cc883e1257c0", "Voted");
			zMultiControlColumnStyleInfo1.ColumnName = "Answer";
			zMultiControlColumnStyleInfo1.FieldTypeColumnName = "AnswerFieldType";
			zMultiControlColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			this.ResultItemAnswersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ResultItemAnswersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ResultItemAnswersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.ResultItemAnswersGrid.ColumnStyles.Add(zMultiControlColumnStyleInfo1);
			// 
			// VoteResultsByQuestionUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Name = "VoteResultsByQuestionUserControl";
			this.ResultsByItemsSplitContainer.Panel1.ResumeLayout(false);
			this.ResultsByItemsSplitContainer.Panel2.ResumeLayout(false);
			this.ResultsByItemsSplitContainer.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.ResultItemsGrid)).EndInit();
			this.AnswersTabControl.ResumeLayout(false);
			this.AnswersTabPage.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.ResultItemAnswersGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion
	}
}
