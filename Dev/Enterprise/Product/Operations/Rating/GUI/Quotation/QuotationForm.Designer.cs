using System;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.GUI
{
	public partial class QuotationForm
	{
		CFXUserControl cfxUserControl1;
		internal QuoteTabControl QuotationTabControl1;
		ClientInformationUserControl clientInformationUserControl1;
		internal Core.Forms.ZPostingButtonsUserControl PostingButtonsUserControl;
		QuotationDateAndNumberControl quotationDateControl1;
		internal ZButton PrintQuoteButton;
		private ZDateEdit FollowUpDateEdit;
		internal ZButton ApproveQuoteButton;
		internal RateEntryFilterStripControl rateEntryFilterStripControl;
		private ZPanel zPanel1;
		private ZPanel zPanel3;
		private ZPanel zPanel2;

		new void InitializeComponent()
		{
			this.cfxUserControl1 = new CFXUserControl();
			this.PostingButtonsUserControl = new Core.Forms.ZPostingButtonsUserControl();
			this.QuotationTabControl1 = new QuoteTabControl();
			this.clientInformationUserControl1 = new ClientInformationUserControl();
			this.quotationDateControl1 = new QuotationDateAndNumberControl();
			this.PrintQuoteButton = new ZButton();
			this.FollowUpDateEdit = new ZDateEdit();
			this.ApproveQuoteButton = new ZButton();
			this.rateEntryFilterStripControl = new RateEntryFilterStripControl();
			this.zPanel1 = new ZPanel();
			this.zPanel2 = new ZPanel();
			this.zPanel3 = new ZPanel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PostingButtonsUserControl.SuspendLayout();
			this.zPanel1.SuspendLayout();
			this.zPanel2.SuspendLayout();
			this.zPanel3.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 701, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1024, 24, true);
			this.MainStatusBar.TabIndex = 10;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(492);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(493);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Quote);
			// 
			// cfxUserControl1
			// 
			this.cfxUserControl1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.cfxUserControl1, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(null);
			this.cfxUserControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 42, true);
			this.cfxUserControl1.Name = "cfxUserControl1";
			this.cfxUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 56, true);
			this.cfxUserControl1.TabIndex = 1;
			// 
			// PostingButtonsUserControl
			// 
			this.PostingButtonsUserControl.AllowDrop = true;
			this.PostingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 0, true);
			this.PostingButtonsUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.PostingButtonsUserControl.Name = "PostingButtonsUserControl";
			this.PostingButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.PostingButtonsUserControl.TabIndex = 9;
			// 
			// QuotationTabControl1
			// 
			this.QuotationTabControl1.AllowDrop = true;
			this.QuotationTabControl1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom
			| System.Windows.Forms.AnchorStyles.Left
			| System.Windows.Forms.AnchorStyles.Right;
			this.BindingSource.SetBindingMember(this.QuotationTabControl1, ".");
			this.QuotationTabControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 137, true);
			this.QuotationTabControl1.Name = "QuotationTabControl1";
			this.QuotationTabControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1024, 537, true);
			this.QuotationTabControl1.TabIndex = 4;
			// 
			// clientInformationUserControl1
			// 
			this.clientInformationUserControl1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.clientInformationUserControl1, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(null);
			this.clientInformationUserControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 0, true);
			this.clientInformationUserControl1.Name = "clientInformationUserControl1";
			this.clientInformationUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 39, true);
			this.clientInformationUserControl1.TabIndex = 0;
			// 
			// quotationDateControl1
			// 
			this.quotationDateControl1.AllowDrop = true;
			this.quotationDateControl1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left
			| System.Windows.Forms.AnchorStyles.Right;
			this.quotationDateControl1.AutoSize = true;
			this.BindingSource.SetBindingMember(this.quotationDateControl1, ".");
			this.quotationDateControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 104, true);
			this.quotationDateControl1.Name = "quotationDateControl1";
			this.quotationDateControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 22, true);
			this.quotationDateControl1.TabIndex = 2;
			// 
			// PrintQuoteButton
			// 
			this.PrintQuoteButton.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("QuotationForm|84982807-2a5f-46cf-b1f6-d10db84ffb1c", "Print");
			this.PrintQuoteButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(258, 2, true);
			this.PrintQuoteButton.Name = "PrintQuoteButton";
			this.PrintQuoteButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.PrintQuoteButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(76, 23, true);
			this.PrintQuoteButton.TabIndex = 8;
			this.PrintQuoteButton.ToolTipCaption = null;
			this.PrintQuoteButton.Click += new EventHandler(this.PrintQuoteButton_Click);
			// 
			// FollowUpDateEdit
			// 
			this.FollowUpDateEdit.AllowDrop = true;
			this.FollowUpDateEdit.AutoCompleteMonthThreshold = 1;
			this.FollowUpDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.FollowUpDateEdit, "TH_FollowUpDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Quote)(null)).TH_FollowUpDate);
			this.FollowUpDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(87, 2, true);
			this.FollowUpDateEdit.Name = "FollowUpDateEdit";
			this.FollowUpDateEdit.TabIndex = 6;
			// 
			// ApproveQuoteButton
			// 
			this.ApproveQuoteButton.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("QuotationForm|426f6bb0-3edd-4836-bb1e-238685bc8014", "Approve");
			this.ApproveQuoteButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(176, 2, true);
			this.ApproveQuoteButton.Name = "ApproveQuoteButton";
			this.ApproveQuoteButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.ApproveQuoteButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(76, 23, true);
			this.ApproveQuoteButton.TabIndex = 7;
			this.ApproveQuoteButton.ToolTipCaption = null;
			this.ApproveQuoteButton.Click += new EventHandler(this.ApproveQuoteButton_Click);
			// 
			// rateEntryFilterStripControl
			// 
			this.rateEntryFilterStripControl.AllowDrop = true;
			this.rateEntryFilterStripControl.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left
			| System.Windows.Forms.AnchorStyles.Right;
			this.rateEntryFilterStripControl.BackColor = System.Drawing.Color.Transparent;
			this.BindingSource.SetBindingMember(this.rateEntryFilterStripControl, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(null);
			this.rateEntryFilterStripControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(314, 0, true);
			this.rateEntryFilterStripControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(710, 136, true);
			this.rateEntryFilterStripControl.Name = "rateEntryFilterStripControl";
			this.rateEntryFilterStripControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(710, 136, true);
			this.rateEntryFilterStripControl.TabIndex = 3;
			this.rateEntryFilterStripControl.CaptionRenderingEnabled = true;
			// 
			// zPanel1
			// 
			this.zPanel1.Controls.Add(this.zPanel3);
			this.zPanel1.Controls.Add(this.zPanel2);
			this.zPanel1.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.zPanel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 637, true);
			this.zPanel1.Name = "zPanel1";
			this.zPanel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1024, 25, true);
			this.zPanel1.TabIndex = 11;
			// 
			// zPanel2
			// 
			this.zPanel2.AutoSize = true;
			this.zPanel2.Controls.Add(this.PostingButtonsUserControl);
			this.zPanel2.Dock = System.Windows.Forms.DockStyle.Right;
			this.zPanel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(828, 0, true);
			this.zPanel2.Name = "zPanel2";
			this.zPanel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(256, 25, true);
			this.zPanel2.TabIndex = 12;
			// 
			// zPanel3
			// 
			this.zPanel3.Controls.Add(this.FollowUpDateEdit);
			this.zPanel3.Controls.Add(this.PrintQuoteButton);
			this.zPanel3.Controls.Add(this.ApproveQuoteButton);
			this.zPanel3.Dock = System.Windows.Forms.DockStyle.Right;
			this.zPanel3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(422, 0, true);
			this.zPanel3.Name = "zPanel3";
			this.zPanel3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(406, 25, true);
			this.zPanel3.TabIndex = 12;
			// 
			// QuotationForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1024, 725, true);
			this.Controls.Add(this.zPanel1);
			this.Controls.Add(this.rateEntryFilterStripControl);
			this.Controls.Add(this.cfxUserControl1);
			this.Controls.Add(this.quotationDateControl1);
			this.Controls.Add(this.clientInformationUserControl1);
			this.Controls.Add(this.QuotationTabControl1);
			this.DataSourceAssemblyName = "Enterprise.Rating.Business";
			this.DataSourceType = typeof(Quote);
			this.DataSourceTypeName = "Enterprise.Rating.Business.Quote";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1024, 725, true);
			this.Name = "QuotationForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Text = "QuotationForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.QuotationTabControl1, 0);
			this.Controls.SetChildIndex(this.clientInformationUserControl1, 0);
			this.Controls.SetChildIndex(this.quotationDateControl1, 0);
			this.Controls.SetChildIndex(this.cfxUserControl1, 0);
			this.Controls.SetChildIndex(this.rateEntryFilterStripControl, 0);
			this.Controls.SetChildIndex(this.zPanel1, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PostingButtonsUserControl.ResumeLayout(true);
			this.PostingButtonsUserControl.PerformLayout();
			this.zPanel1.ResumeLayout(false);
			this.zPanel1.PerformLayout();
			this.zPanel2.ResumeLayout(false);
			this.zPanel2.PerformLayout();
			this.zPanel3.ResumeLayout(false);
			this.zPanel3.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
