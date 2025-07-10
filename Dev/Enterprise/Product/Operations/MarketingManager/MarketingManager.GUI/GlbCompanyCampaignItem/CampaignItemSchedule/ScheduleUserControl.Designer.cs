using Enterprise.MarketingManager.Business;

namespace Enterprise.MarketingManager.GUI
{
	partial class ScheduleUserControl
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
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.zGroupBox1 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.zGroupBox2 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.NoTimeZoneLabel = new Enterprise.ZArchitecture.ZLabel();
			this.RecipientTimeZonesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.FallbackTimeZoneDescriptionLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ScheduleSendTimeDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
			this.zRadioButton2 = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.zRadioButton1 = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.SenderTimeZoneCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.zGroupBox1.SuspendLayout();
			this.zGroupBox2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.RecipientTimeZonesGrid)).BeginInit();
			this.RecipientTimeZonesGrid.SuspendLayout();
			this.ScheduleSendTimeDateEdit.SuspendLayout();
			this.SenderTimeZoneCodeFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MarketingManager.Business.GlbCompanyCampaignItemSchedule);
			// 
			// zGroupBox1
			// 
			this.zGroupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.zGroupBox1.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("38bbb8d6-845e-4111-a18c-cbc585bdbe22", "Campaign Send Schedule");
			this.zGroupBox1.Controls.Add(this.zGroupBox2);
			this.zGroupBox1.Controls.Add(this.FallbackTimeZoneDescriptionLabel);
			this.zGroupBox1.Controls.Add(this.ScheduleSendTimeDateEdit);
			this.zGroupBox1.Controls.Add(this.zLabel1);
			this.zGroupBox1.Controls.Add(this.zRadioButton2);
			this.zGroupBox1.Controls.Add(this.zRadioButton1);
			this.zGroupBox1.Controls.Add(this.SenderTimeZoneCodeFindBox);
			this.zGroupBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.zGroupBox1.Name = "zGroupBox1";
			this.zGroupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(840, 386, true);
			this.zGroupBox1.TabIndex = 0;
			this.zGroupBox1.TabStop = false;
			// 
			// zGroupBox2
			// 
			this.zGroupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.zGroupBox2.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("bbd05f78-d072-48dd-985e-d6c88e935686", "Time Zone Schedule");
			this.zGroupBox2.Controls.Add(this.NoTimeZoneLabel);
			this.zGroupBox2.Controls.Add(this.RecipientTimeZonesGrid);
			this.zGroupBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 99, true);
			this.zGroupBox2.Name = "zGroupBox2";
			this.zGroupBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(827, 281, true);
			this.zGroupBox2.TabIndex = 6;
			this.zGroupBox2.TabStop = false;
			// 
			// NoTimeZoneLabel
			// 
			this.NoTimeZoneLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.NoTimeZoneLabel, "NoTimeZoneDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.GlbCompanyCampaignItemSchedule)(null)).NoTimeZoneDescription)));
			this.NoTimeZoneLabel.ForeColor = System.Drawing.Color.Red;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.NoTimeZoneLabel, false);
			this.NoTimeZoneLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(352, 10, true);
			this.NoTimeZoneLabel.Name = "NoTimeZoneLabel";
			this.NoTimeZoneLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(469, 15, true);
			this.NoTimeZoneLabel.TabIndex = 0;
			// 
			// RecipientTimeZonesGrid
			// 
			this.RecipientTimeZonesGrid.AllowNavigation = false;
			this.RecipientTimeZonesGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.RecipientTimeZonesGrid, "ScheduleItemsCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MarketingManager.Business.GlbCompanyCampaignItemSchedule)(null)).ScheduleItemsCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MarketingManager.Business.ScheduleCampaignItems)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.GlbCompanyCampaignItemSchedule)(null)).ScheduleItemsCollection)).SyncRoot)).ScheduleSendTimeUTC)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.ScheduleCampaignItems)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.GlbCompanyCampaignItemSchedule)(null)).ScheduleItemsCollection)).SyncRoot)).UtcOffsetText)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.ScheduleCampaignItems)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.GlbCompanyCampaignItemSchedule)(null)).ScheduleItemsCollection)).SyncRoot)).TimeZone)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.ScheduleCampaignItems)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.GlbCompanyCampaignItemSchedule)(null)).ScheduleItemsCollection)).SyncRoot)).StandardTimeZoneCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MarketingManager.Business.ScheduleCampaignItems)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.GlbCompanyCampaignItemSchedule)(null)).ScheduleItemsCollection)).SyncRoot)).ContactsCount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MarketingManager.Business.ScheduleCampaignItems)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.GlbCompanyCampaignItemSchedule)(null)).ScheduleItemsCollection)).SyncRoot)).ScheduleSendTimeLocal)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.ScheduleCampaignItems)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.GlbCompanyCampaignItemSchedule)(null)).ScheduleItemsCollection)).SyncRoot)).StatusText)));
			this.RecipientTimeZonesGrid.CaptionVisible = false;
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("17fb1ce8-83f3-4133-a2b2-32041babad63", "Schedule Time (UTC)");
			zDateEditColumnStyleInfo1.ColumnName = "ScheduleSendTimeUTC";
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("6d578dab-b44a-42cf-8279-328d64669722", "UTC Offset");
			zTextBoxColumnStyleInfo1.ColumnName = "UtcOffsetText";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("fc291545-94c3-4acc-b066-be7ccd5e9d1a", "Time Zone Description");
			zTextBoxColumnStyleInfo2.ColumnName = "TimeZone";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(360);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("d9edc7d8-a1fc-45b3-81ba-e0fe97e62ca5", "Standard Code");
			zTextBoxColumnStyleInfo3.ColumnName = "StandardTimeZoneCode";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("7be04d14-5be6-4053-9cb4-11ea131c8426", "Number of Contacts");
			zCalcEditColumnStyleInfo1.ColumnName = "ContactsCount";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			zDateEditColumnStyleInfo2.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("8c99fe89-f0d5-49d1-a688-a97c9350d485", "Schedule Time (Time Zone)");
			zDateEditColumnStyleInfo2.ColumnName = "ScheduleSendTimeLocal";
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("6ff2ebb5-5c75-4b3e-9abe-c96801a24d1b", "Status");
			zTextBoxColumnStyleInfo4.ColumnName = "StatusText";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.RecipientTimeZonesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.RecipientTimeZonesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.RecipientTimeZonesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.RecipientTimeZonesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.RecipientTimeZonesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.RecipientTimeZonesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.RecipientTimeZonesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.RecipientTimeZonesGrid.CopySelectedRowsAllowed = true;
			this.RecipientTimeZonesGrid.GridId = "b74fbd1c-117e-4dd9-9d4d-90b69903434a";
			this.RecipientTimeZonesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.RecipientTimeZonesGrid.LayoutKey = "zGrid1";
			this.RecipientTimeZonesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 28, true);
			this.RecipientTimeZonesGrid.Name = "RecipientTimeZonesGrid";
			this.RecipientTimeZonesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(815, 247, true);
			this.RecipientTimeZonesGrid.TabIndex = 1;
			// 
			// FallbackTimeZoneDescriptionLabel
			// 
			this.FallbackTimeZoneDescriptionLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.FallbackTimeZoneDescriptionLabel, "SenderTimeZoneDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.GlbCompanyCampaignItemSchedule)(null)).SenderTimeZoneDescription)));
			this.FallbackTimeZoneDescriptionLabel.IsFontBold = true;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.FallbackTimeZoneDescriptionLabel, false);
			this.FallbackTimeZoneDescriptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(638, 71, true);
			this.FallbackTimeZoneDescriptionLabel.Name = "FallbackTimeZoneDescriptionLabel";
			this.FallbackTimeZoneDescriptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(195, 23, true);
			this.FallbackTimeZoneDescriptionLabel.TabIndex = 5;
			// 
			// ScheduleSendTimeDateEdit
			// 
			this.ScheduleSendTimeDateEdit.AllowDrop = true;
			this.ScheduleSendTimeDateEdit.AutoCompleteMonthThreshold = 1;
			this.ScheduleSendTimeDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ScheduleSendTimeDateEdit, "ScheduleSendTimeLocal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MarketingManager.Business.GlbCompanyCampaignItemSchedule)(null)).ScheduleSendTimeLocal)));
			this.ScheduleSendTimeDateEdit.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("1b98fff2-49d7-4762-8a14-04412dcf9a16", "Scheduled Send Time");
			this.ScheduleSendTimeDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.ScheduleSendTimeDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(162, 50, true);
			this.ScheduleSendTimeDateEdit.Name = "ScheduleSendTimeDateEdit";
			this.ScheduleSendTimeDateEdit.TabIndex = 1;
			// 
			// zLabel1
			// 
			this.zLabel1.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("465d0a25-0356-4bb7-bc2b-d70d09654c08", "Set a Schedule Send Time and preferred time zone for automatic delivery.");
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(811, 23, true);
			this.zLabel1.TabIndex = 0;
			// 
			// zRadioButton2
			// 
			this.zRadioButton2.AutoCheck = false;
			this.BindingSource.SetBindingMember(this.zRadioButton2, "IsRecipientsLocalTimeUsed");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MarketingManager.Business.GlbCompanyCampaignItemSchedule)(null)).IsRecipientsLocalTimeUsed)));
			this.zRadioButton2.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("365edc5b-ed37-42da-85d6-3fed57f10792", "Recipient Local Time");
			this.zRadioButton2.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.zRadioButton2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(296, 47, true);
			this.zRadioButton2.Name = "zRadioButton2";
			this.zRadioButton2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(239, 24, true);
			this.zRadioButton2.TabIndex = 2;
			this.zRadioButton2.UseVisualStyleBackColor = true;
			// 
			// zRadioButton1
			// 
			this.zRadioButton1.AutoCheck = false;
			this.BindingSource.SetBindingMember(this.zRadioButton1, "IsSendersLocalTimeUsed");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MarketingManager.Business.GlbCompanyCampaignItemSchedule)(null)).IsSendersLocalTimeUsed)));
			this.zRadioButton1.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("0c2e824f-c16e-487d-bf0f-eee40ede93e1", "Sender Local Time");
			this.zRadioButton1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.zRadioButton1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(296, 71, true);
			this.zRadioButton1.Name = "zRadioButton1";
			this.zRadioButton1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(148, 24, true);
			this.zRadioButton1.TabIndex = 3;
			this.zRadioButton1.TabStop = true;
			this.zRadioButton1.UseVisualStyleBackColor = true;
			// 
			// SenderTimeZoneCodeFindBox
			// 
			this.SenderTimeZoneCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SenderTimeZoneCodeFindBox, "SenderTimeZone");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.GlbCompanyCampaignItemSchedule)(null)).SenderTimeZone)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MarketingManager.Business.GlbCompanyCampaignItemSchedule)(null)).UNLOCOs)));
			this.SenderTimeZoneCodeFindBox.BindToList = "UNLOCOs";
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.SenderTimeZoneCodeFindBox, false);
			this.SenderTimeZoneCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(450, 73, true);
			this.SenderTimeZoneCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefUNLOCO;
			this.SenderTimeZoneCodeFindBox.Name = "SenderTimeZoneCodeFindBox";
			this.SenderTimeZoneCodeFindBox.PreBoundMaxLength = 7;
			this.SenderTimeZoneCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(182, 20, true);
			this.SenderTimeZoneCodeFindBox.TabIndex = 4;
			// 
			// ScheduleUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.zGroupBox1);
			this.Name = "ScheduleUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(846, 392, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.zGroupBox1.ResumeLayout(false);
			this.zGroupBox1.PerformLayout();
			this.zGroupBox2.ResumeLayout(false);
			this.zGroupBox2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.RecipientTimeZonesGrid)).EndInit();
			this.RecipientTimeZonesGrid.ResumeLayout(false);
			this.RecipientTimeZonesGrid.PerformLayout();
			this.ScheduleSendTimeDateEdit.ResumeLayout(true);
			this.ScheduleSendTimeDateEdit.PerformLayout();
			this.SenderTimeZoneCodeFindBox.ResumeLayout(true);
			this.SenderTimeZoneCodeFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox zGroupBox1;
		private ZArchitecture.ZLabel zLabel1;
		private ZArchitecture.GUI.ZGroupBox zGroupBox2;
		private ZArchitecture.ZLabel FallbackTimeZoneDescriptionLabel;
		private ZArchitecture.GUI.ZRadioButton zRadioButton2;
		private ZArchitecture.GUI.ZCodeFindBox SenderTimeZoneCodeFindBox;
		private ZArchitecture.GUI.ZRadioButton zRadioButton1;
		private ZArchitecture.GUI.ZDateEdit ScheduleSendTimeDateEdit;
		protected ZArchitecture.ZGrid RecipientTimeZonesGrid;
		private ZArchitecture.ZLabel NoTimeZoneLabel;
	}
}
