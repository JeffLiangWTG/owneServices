using CargoWiseOne.ResourceStrings;
using Enterprise.Registry.Business;

namespace Enterprise.MarketingManager.GUI
{
	sealed partial class SelectStaffPoolOpportunityAssignmentForm
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
			this.StaffAssignmentsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DescriptionLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ContactsGrid)).BeginInit();
			this.ContactsGrid.SuspendLayout();
			this.StaffAssignmentsGroupBox.SuspendLayout();
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
			| System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
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
			zCalcEditColumnStyleInfo1.CaptionResourceString = Res.GetData("175FC4B9-E475-4F28-A32E-53CC045B8837", "Opp Ratio");
			zCalcEditColumnStyleInfo1.ColumnName = "GCP_SendRatio";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Res.GetData("41905208-2579-4119-ACE9-7A98A5C3B237", "Opp Ratio %");
			zTextBoxColumnStyleInfo1.ColumnName = "SendRatioPercentage";
			zTextBoxColumnStyleInfo1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.ContactsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.ContactsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.ContactsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ContactsGrid.GridId = "37B5E86D-F791-48DC-BADB-5572940505D0";
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
			this.CloseButton.CaptionResourceString = Res.GetData("1A9FE13A-3031-4580-B42D-DE026FDAE868", "Close");
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(489, 412, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CloseButton.TabIndex = 1;
			this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
			// 
			// StaffAssignmentsGroupBox
			// 
			this.StaffAssignmentsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
			this.StaffAssignmentsGroupBox.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("2C0F8FCB-66BC-4796-B947-91EDF4FB3C02", "Staff Assignments");
			this.StaffAssignmentsGroupBox.Controls.Add(this.DescriptionLabel);
			this.StaffAssignmentsGroupBox.Controls.Add(this.ContactsGrid);
			this.StaffAssignmentsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 8, true);
			this.StaffAssignmentsGroupBox.Name = "StaffAssignmentsGroupBox";
			this.StaffAssignmentsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(555, 400, true);
			this.StaffAssignmentsGroupBox.TabIndex = 0;
			this.StaffAssignmentsGroupBox.TabStop = false;
			// 
			// DescriptionLabel
			// 
			this.DescriptionLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
			this.BackColor = SystemDataRegistry.Instance.ColorTheme.TabBackgroundColor;
			this.DescriptionLabel.Text = Res.GetString("E2333DD2-988C-4AFE-93B8-8BB78EDA86C6", "Opportunities will be created randomly from the following Staff Pool. Enter the preferred Staff Assignment and the creation ratio to control the amount of opportunities created for each Staff member.");
			this.DescriptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.DescriptionLabel.Name = "DescriptionLabel";
			this.DescriptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(551, 53, true);
			this.DescriptionLabel.TabIndex = 0;
			this.DescriptionLabel.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			// 
			// SelectStaffPoolOpportunityAssignmentForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("4935C628-5318-4563-9498-8035F123DD8E", "Staff Pool Assignments");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(572, 463, true);
			this.Controls.Add(this.StaffAssignmentsGroupBox);
			this.Controls.Add(this.CloseButton);
			this.DataSourceType = typeof(Enterprise.MarketingManager.Business.GlbCompanyCampaignSenderPool);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(587, 500, true);
			this.Name = "SelectStaffPoolOpportunityAssignmentForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.CloseButton, 0);
			this.Controls.SetChildIndex(this.StaffAssignmentsGroupBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ContactsGrid)).EndInit();
			this.ContactsGrid.ResumeLayout(false);
			this.ContactsGrid.PerformLayout();
			this.StaffAssignmentsGroupBox.ResumeLayout(false);
			this.StaffAssignmentsGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		ZArchitecture.ZGrid ContactsGrid;
		ZArchitecture.GUI.ZButton CloseButton;
		ZArchitecture.GUI.ZGroupBox StaffAssignmentsGroupBox;
		ZArchitecture.ZLabel DescriptionLabel;

		#endregion
	}
}