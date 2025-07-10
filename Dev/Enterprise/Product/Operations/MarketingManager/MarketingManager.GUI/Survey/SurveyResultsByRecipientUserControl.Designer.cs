namespace Enterprise.MarketingManager.GUI
{
	partial class SurveyResultsByRecipientUserControl
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo zMultiControlColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo();
			this.ResultsByRecipientsSplitContainer.Panel1.SuspendLayout();
			this.ResultsByRecipientsSplitContainer.Panel2.SuspendLayout();
			this.ResultsByRecipientsSplitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.RecipientsGrid)).BeginInit();
			this.AnswersTabControl.SuspendLayout();
			this.AnswersTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.RecipientAnswersGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// ResultsByRecipientsSplitContainer
			// 
			// 
			// RecipientsGrid
			// 
			zTextBoxColumnStyleInfo1.ColumnName = "Recipient+Organisation+OH_Code";
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("49ba9cc5-697e-4ac3-a113-8dcd633cf1de", "Phone");
			zTextBoxColumnStyleInfo2.ColumnName = "WorkPhone";
			zTextBoxColumnStyleInfo3.ColumnName = "Recipient+Organisation+ClosestPort+CountryCode+RN_Code";
			zTextBoxColumnStyleInfo3.IsVisible = false;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zDateEditColumnStyleInfo1.ColumnName = "G8_SystemCreateTimeUtc";
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCodeFindBoxColumnStyleInfo1.BindToList = "Lookups+FollowedUpBys";
			zCodeFindBoxColumnStyleInfo1.ColumnName = "G8_SystemCreateUser";
			zCodeFindBoxColumnStyleInfo1.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.GlbStaff;
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("SurveyResultsByRecipientUserControl|45742817-5066-4103-a8dd-622a43739126", "Staff", "Staff Name", "");
			zTextBoxColumnStyleInfo4.ColumnName = "SenderStaffName";
			zTextBoxColumnStyleInfo4.IsVisible = false;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDateEditColumnStyleInfo2.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("SurveyResultsByRecipientUserControl|405745dd-7767-4354-82ab-a08903faec78", "Closed Date (UTC)", "Closed Date (UTC)", "");
			zDateEditColumnStyleInfo2.ColumnName = "G8_ClosedDateUtc";
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo5.ColumnName = "Recipient+Organisation+OH_FullName";
			zTextBoxColumnStyleInfo5.IsVisible = false;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.RecipientsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.RecipientsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.RecipientsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.RecipientsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.RecipientsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.RecipientsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.RecipientsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.RecipientsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			// 
			// RecipientAnswersGrid
			// 
			zMultiControlColumnStyleInfo1.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("ResultsByRecipientUserControl|9d85a8c9-589c-4bbc-af19-740551caa681", "Answer");
			zMultiControlColumnStyleInfo1.ColumnName = "Answer";
			zMultiControlColumnStyleInfo1.FieldTypeColumnName = "AnswerFieldType";
			this.RecipientAnswersGrid.ColumnStyles.Add(zMultiControlColumnStyleInfo1);
			// 
			// SurveyResultsByRecipientUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Name = "SurveyResultsByRecipientUserControl";
			this.ResultsByRecipientsSplitContainer.Panel1.ResumeLayout(false);
			this.ResultsByRecipientsSplitContainer.Panel2.ResumeLayout(false);
			this.ResultsByRecipientsSplitContainer.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.RecipientsGrid)).EndInit();
			this.AnswersTabControl.ResumeLayout(false);
			this.AnswersTabPage.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.RecipientAnswersGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion
	}
}
