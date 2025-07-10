namespace Enterprise.MarketingManager.GUI
{
	sealed partial class SelectStaffForSenderPoolForm
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
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.ContactsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.groupBox1 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DescriptionLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ContactsGrid)).BeginInit();
			this.ContactsGrid.SuspendLayout();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 439, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(572, 24, true);
			this.MainStatusBar.TabIndex = 2;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MarketingManager.Business.GlbCompanyCampaignSenderPool);
			// 
			// ContactsGrid
			// 
			this.ContactsGrid.AllowNavigation = false;
			this.ContactsGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ContactsGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MarketingManager.Business.GlbCompanyCampaignSenderPoolItem)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.GlbCompanyCampaignSenderPoolItem)(null)).GCP_GS_NKSender)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MarketingManager.Business.GlbCompanyCampaignSenderPoolItem)(null)).GCP_SendRatio)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.GlbCompanyCampaignSenderPoolItem)(null)).SendRatioPercentage)));
			this.ContactsGrid.CaptionVisible = false;
			zCodeFindBoxColumnStyleInfo1.ColumnName = "GCP_GS_NKSender";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(167);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "GCP_SendRatio";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.ColumnName = "SendRatioPercentage";
			zTextBoxColumnStyleInfo1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.ContactsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.ContactsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.ContactsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ContactsGrid.GridId = "30bf9fba-6002-4f45-99f1-a1f9a599c58b";
			this.ContactsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ContactsGrid.LayoutKey = "ContactsGrid";
			this.ContactsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 69, true);
			this.ContactsGrid.Name = "ContactsGrid";
			this.ContactsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(547, 327, true);
			this.ContactsGrid.TabIndex = 1;
			this.ContactsGrid.TabStop = false;
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("aeb5c97f-9d20-4d9b-ba45-9c0cbb0d2f77", "Close");
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(489, 412, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CloseButton.TabIndex = 1;
			this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
			// 
			// groupBox1
			// 
			this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox1.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("e017dfdb-c9ef-414b-8cfc-36318e78a503", "Email Senders");
			this.groupBox1.Controls.Add(this.DescriptionLabel);
			this.groupBox1.Controls.Add(this.ContactsGrid);
			this.groupBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 8, true);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(555, 400, true);
			this.groupBox1.TabIndex = 0;
			this.groupBox1.TabStop = false;
			// 
			// DescriptionLabel
			// 
			this.DescriptionLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.DescriptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.DescriptionLabel.Name = "DescriptionLabel";
			this.DescriptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(551, 53, true);
			this.DescriptionLabel.TabIndex = 0;
			this.DescriptionLabel.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			// 
			// SelectStaffForSenderPoolForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("825de6df-7bfe-4586-9023-78758b7d5013", "Staff Pool of Senders");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(572, 463, true);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.CloseButton);
			this.DataSourceType = typeof(Enterprise.MarketingManager.Business.GlbCompanyCampaignSenderPool);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(587, 500, true);
			this.Name = "SelectStaffForSenderPoolForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.CloseButton, 0);
			this.Controls.SetChildIndex(this.groupBox1, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ContactsGrid)).EndInit();
			this.ContactsGrid.ResumeLayout(false);
			this.ContactsGrid.PerformLayout();
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		ZArchitecture.ZGrid ContactsGrid;
		ZArchitecture.GUI.ZButton CloseButton;
		private ZArchitecture.GUI.ZGroupBox groupBox1;
		private ZArchitecture.ZLabel DescriptionLabel;
	}
}