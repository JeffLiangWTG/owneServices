namespace Enterprise.MarketingManager.GUI
{
	partial class UnsubscribeContactsForm
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();

			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfoSub1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfoSub1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfoSub2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfoSub2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfoSub1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();

			this.ContactsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.SubscriptionsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.postingButtonsUserControl = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			this.bottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.zGroupBox1 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.zLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ContactsGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.SubscriptionsGrid)).BeginInit();
			this.ContactsGrid.SuspendLayout();
			this.SubscriptionsGrid.SuspendLayout();
			this.postingButtonsUserControl.SuspendLayout();
			this.bottomPanel.SuspendLayout();
			this.zGroupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 441, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1106, 24, true);
			this.MainStatusBar.TabIndex = 2;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MarketingManager.GUI.UnsubscribeContactsBusinessObject);
			// 
			// ContactsGrid
			// 
			this.ContactsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ContactsGrid, "ContactsCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MarketingManager.GUI.UnsubscribeContactsBusinessObject)(null)).ContactsCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.GUI.UnsubscribeContactsItem)(((System.Collections.IList)(((Enterprise.MarketingManager.GUI.UnsubscribeContactsBusinessObject)(null)).ContactsCollection)).SyncRoot)).ClientName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.GUI.UnsubscribeContactsItem)(((System.Collections.IList)(((Enterprise.MarketingManager.GUI.UnsubscribeContactsBusinessObject)(null)).ContactsCollection)).SyncRoot)).PrimaryContact)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.GUI.UnsubscribeContactsItem)(((System.Collections.IList)(((Enterprise.MarketingManager.GUI.UnsubscribeContactsBusinessObject)(null)).ContactsCollection)).SyncRoot)).Email)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.GUI.UnsubscribeContactsItem)(((System.Collections.IList)(((Enterprise.MarketingManager.GUI.UnsubscribeContactsBusinessObject)(null)).ContactsCollection)).SyncRoot)).UnsubscribeResult)));
			this.ContactsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("140ffa89-e89f-4e91-989c-cb3feceb494b", "Client Name");
			zTextBoxColumnStyleInfo1.ColumnName = "ClientName";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("fe6ad882-6956-4ce0-805d-4924ab72443d", "Primary Contact");
			zTextBoxColumnStyleInfo2.ColumnName = "PrimaryContact";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("49a65ab5-b8d7-40e9-a6e5-9ad9ebfe4d25", "Email");
			zTextBoxColumnStyleInfo3.ColumnName = "Email";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(33);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("d1a6f970-8c26-4513-b8a9-a9cece813788", "Unsubscribe Result");
			zTextBoxColumnStyleInfo4.ColumnName = "UnsubscribeResult";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(333);
			this.ContactsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ContactsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ContactsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.ContactsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.ContactsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ContactsGrid.GridId = "30bf9fba-6002-4f45-99f1-a1f9a599c58b";
			this.ContactsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ContactsGrid.LayoutKey = "ContactsGrid";
			this.ContactsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 145, true);
			this.ContactsGrid.Name = "ContactsGrid";
			this.ContactsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1106, 296, true);
			this.ContactsGrid.TabIndex = 1;
			// 
			// SubscriptionsGrid
			// 
			this.SubscriptionsGrid.AllowNavigation = false;
			this.SubscriptionsGrid.AllowReadOnlyRowsToBeDeleted = true;
			this.BindingSource.SetBindingMember(this.SubscriptionsGrid, "SubscriptionsList");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MarketingManager.GUI.UnsubscribeContactsBusinessObject)(null)).SubscriptionsList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.SubscriptionProperties)(((System.Collections.IList)(((Enterprise.MarketingManager.GUI.UnsubscribeContactsBusinessObject)(null)).SubscriptionsList)).SyncRoot)).IsSubscribed)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.SubscriptionProperties)(((System.Collections.IList)(((Enterprise.MarketingManager.GUI.UnsubscribeContactsBusinessObject)(null)).SubscriptionsList)).SyncRoot)).MediaCategoryWithAll)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.SubscriptionProperties)(((System.Collections.IList)(((Enterprise.MarketingManager.GUI.UnsubscribeContactsBusinessObject)(null)).SubscriptionsList)).SyncRoot)).MediaTypeWithAll)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Registry.Business.SubscriptionProperties)(((System.Collections.IList)(((Enterprise.MarketingManager.GUI.UnsubscribeContactsBusinessObject)(null)).SubscriptionsList)).SyncRoot)).CampaignPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.SubscriptionProperties)(((System.Collections.IList)(((Enterprise.MarketingManager.GUI.UnsubscribeContactsBusinessObject)(null)).SubscriptionsList)).SyncRoot)).IsOrgLevel)));
			this.SubscriptionsGrid.CaptionVisible = false;
			zCheckBoxColumnStyleInfoSub1.ColumnName = "IsSubscribed";
			zCheckBoxColumnStyleInfoSub1.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("880dd165-5b0e-4c1f-ae47-d18cc9989501", "Subscribed");
			zCheckBoxColumnStyleInfoSub1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDropEditColumnStyleInfoSub1.ColumnName = "MediaCategoryWithAll";
			zDropEditColumnStyleInfoSub1.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("c571f074-10d4-408e-a6a7-5b2b86e72d51", "Media Category");
			zDropEditColumnStyleInfoSub1.ShowHorizontalScrollBar = false;
			zDropEditColumnStyleInfoSub1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDropEditColumnStyleInfoSub2.ColumnName = "MediaTypeWithAll";
			zDropEditColumnStyleInfoSub2.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("d0c880d2-dec0-4b40-910c-feee95dd4495", "Media Type");
			zDropEditColumnStyleInfoSub2.ShowHorizontalScrollBar = false;
			zDropEditColumnStyleInfoSub2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCheckBoxColumnStyleInfoSub2.ColumnName = "IsOrgLevel";
			zCheckBoxColumnStyleInfoSub2.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("b3303c2f-bcfb-4425-9263-6b2df00c40fc", "Org. Level");
			zCheckBoxColumnStyleInfoSub2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfoSub1.ColumnName = "CampaignPK";
			zTextBoxColumnStyleInfoSub1.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("dd2a4193-dd21-4037-b52a-eb8b40c66bba", "Campaign");
			zTextBoxColumnStyleInfoSub1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfoSub1.IsVisible = false;
			zTextBoxColumnStyleInfoSub1.IsReadOnly = true;

			this.SubscriptionsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfoSub1);
			this.SubscriptionsGrid.ColumnStyles.Add(zDropEditColumnStyleInfoSub1);
			this.SubscriptionsGrid.ColumnStyles.Add(zDropEditColumnStyleInfoSub2);
			this.SubscriptionsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfoSub2);
			this.SubscriptionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfoSub1);

			this.SubscriptionsGrid.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.SubscriptionsGrid.GridId = "47436305-a75e-48e8-b0cc-433a5e5d3fa2";
			this.SubscriptionsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.SubscriptionsGrid.LayoutKey = "SubscriptionsGrid";
			this.SubscriptionsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 60, true);
			this.SubscriptionsGrid.Name = "SubscriptionsGrid";
			this.SubscriptionsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1100, 180, true);
			this.SubscriptionsGrid.TabIndex = 0;

			// 
			// postingButtonsUserControl
			// 
			this.postingButtonsUserControl.AllowDrop = true;
			this.postingButtonsUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.postingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(864, 1, true);
			this.postingButtonsUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.postingButtonsUserControl.Name = "postingButtonsUserControl";
			this.postingButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.postingButtonsUserControl.TabIndex = 0;
			// 
			// bottomPanel
			// 
			this.bottomPanel.Controls.Add(this.postingButtonsUserControl);
			this.bottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.bottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 465, true);
			this.bottomPanel.Name = "bottomPanel";
			this.bottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1106, 28, true);
			this.bottomPanel.TabIndex = 3;
			// 
			// zGroupBox1
			// 
			this.zGroupBox1.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("197c46fc-69f1-4b58-ad45-3a2ab2fff1fc", "Subscription Action:");
			this.zGroupBox1.Controls.Add(this.zLabel);
			this.zGroupBox1.Controls.Add(this.SubscriptionsGrid);
			this.zGroupBox1.Dock = System.Windows.Forms.DockStyle.Top;
			this.zGroupBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 7, true);
			this.zGroupBox1.Name = "zGroupBox1";
			this.zGroupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1106, 220, true);
			this.zGroupBox1.TabIndex = 0;
			this.zGroupBox1.TabStop = false;

			// 
			// zLabel
			// 
			this.zLabel.AutoSize = true;
			this.zLabel.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("88cb637b-ae6a-4d80-8b6a-5f8ca42d356d", "Choose the subscription preference you wish to apply to the following contact(s):");
			this.zLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 16, true);
			this.zLabel.Name = "zLabel";
			this.zLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(374, 13, true);
			this.zLabel.TabIndex = 0;
			// 
			// UnsubscribeContactsForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("570a6a61-c102-4c4b-8312-91470efb557d", "Subscription Preference");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1120, 500, true);
			this.Controls.Add(this.ContactsGrid);
			this.Controls.Add(this.zGroupBox1);
			this.Controls.Add(this.bottomPanel);
			this.DataSourceType = typeof(Enterprise.MarketingManager.GUI.UnsubscribeContactsBusinessObject);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1120, 500, true);
			this.Name = "UnsubscribeContactsForm";
			this.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(7, true);
			this.Controls.SetChildIndex(this.bottomPanel, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.zGroupBox1, 0);
			this.Controls.SetChildIndex(this.ContactsGrid, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ContactsGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.SubscriptionsGrid)).EndInit();
			this.ContactsGrid.ResumeLayout(false);
			this.ContactsGrid.PerformLayout();
			this.SubscriptionsGrid.ResumeLayout(false);
			this.SubscriptionsGrid.PerformLayout();
			this.postingButtonsUserControl.ResumeLayout(true);
			this.postingButtonsUserControl.PerformLayout();
			this.bottomPanel.ResumeLayout(false);
			this.bottomPanel.PerformLayout();
			this.zGroupBox1.ResumeLayout(false);
			this.zGroupBox1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Core.Forms.ZPostingButtonsUserControl postingButtonsUserControl;
		private ZArchitecture.GUI.ZPanel bottomPanel;
		private ZArchitecture.ZGrid ContactsGrid;
		private Enterprise.ZArchitecture.GUI.ZGroupBox zGroupBox1;
		private Enterprise.ZArchitecture.ZLabel zLabel;
		private ZArchitecture.ZGrid SubscriptionsGrid;
	}
}