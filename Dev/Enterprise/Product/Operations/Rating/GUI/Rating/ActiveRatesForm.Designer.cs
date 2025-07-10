using System;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.GUI
{
	public sealed partial class ActiveRatesForm
	{
		internal RateEntryFilterStripControl rateEntryFilterStripControl;
		private ClientInformationUserControl clientInformationUserControl1;
		private CFXUserControl cfxUserControl1;
		private Core.Forms.ZPostingButtonsUserControl PostingButtonsUserControl;
		private RatingTabControl ratingTabControl1;
		private ZDateEdit RateEndDateEdit;
		private ZButton UnacceptedQuotesButton;
		private ZArchitecture.ZLabel UnacceptedQuotesWarningLabel;

		new void InitializeComponent()
		{
			this.PostingButtonsUserControl = new Core.Forms.ZPostingButtonsUserControl();
			this.clientInformationUserControl1 = new ClientInformationUserControl();
			this.cfxUserControl1 = new CFXUserControl();
			this.ratingTabControl1 = new RatingTabControl();
			this.RateEndDateEdit = new ZDateEdit();
			this.UnacceptedQuotesButton = new ZButton();
			this.UnacceptedQuotesWarningLabel = new ZArchitecture.ZLabel();
			this.rateEntryFilterStripControl = new RateEntryFilterStripControl();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
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
			this.BindingSource.DataSourceType = typeof(ClientRate);
			// 
			// PostingButtonsUserControl
			// 
			this.PostingButtonsUserControl.AllowDrop = true;
			this.PostingButtonsUserControl.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
			this.PostingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(760, 675, true);
			this.PostingButtonsUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.PostingButtonsUserControl.Name = "PostingButtonsUserControl";
			this.PostingButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.PostingButtonsUserControl.TabIndex = 9;
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
			// ratingTabControl1
			// 
			this.ratingTabControl1.AllowDrop = true;
			this.ratingTabControl1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom
			| System.Windows.Forms.AnchorStyles.Left
			| System.Windows.Forms.AnchorStyles.Right;
			this.BindingSource.SetBindingMember(this.ratingTabControl1, ".");
			this.ratingTabControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 137, true);
			this.ratingTabControl1.Name = "ratingTabControl1";
			this.ratingTabControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1024, 537, true);
			this.ratingTabControl1.TabIndex = 6;
			// 
			// RateEndDateEdit
			// 
			this.RateEndDateEdit.AllowDrop = true;
			this.RateEndDateEdit.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left;
			this.RateEndDateEdit.AutoCompleteMonthThreshold = 1;
			this.RateEndDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.RateEndDateEdit, "TH_NewRateEndDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ClientRate)(null)).TH_NewRateEndDate);
			this.RateEndDateEdit.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("ActiveRatesForm|3e59f328-b669-482a-a9d0-00f611e8d87c", "Rate End Date");
			this.RateEndDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 104, true);
			this.RateEndDateEdit.Name = "RateEndDateEdit";
			this.RateEndDateEdit.TabIndex = 5;
			this.RateEndDateEdit.Visible = false;
			// 
			// UnacceptedQuotesButton
			// 
			this.UnacceptedQuotesButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
			this.UnacceptedQuotesButton.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("ActiveRatesForm|e8c98ddd-53e4-431e-b060-a019e7320448", "Unaccepted Quotes");
			this.UnacceptedQuotesButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(459, 675, true);
			this.UnacceptedQuotesButton.Name = "UnacceptedQuotesButton";
			this.UnacceptedQuotesButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(148, 23, true);
			this.UnacceptedQuotesButton.TabIndex = 7;
			this.UnacceptedQuotesButton.Click += new EventHandler(this.UnacceptedQuotesButton_Click);
			// 
			// UnacceptedQuotesWarningLabel
			// 
			this.UnacceptedQuotesWarningLabel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
			this.UnacceptedQuotesWarningLabel.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("ActiveRatesForm|9c68cd25-5fe0-4ee2-abd2-df03337db8d6", "Unaccepted quotes exist for this client.");
			this.UnacceptedQuotesWarningLabel.ForeColor = System.Drawing.Color.Red;
			this.UnacceptedQuotesWarningLabel.IsFontBold = true;
			this.UnacceptedQuotesWarningLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(579, 675, true);
			this.UnacceptedQuotesWarningLabel.Name = "UnacceptedQuotesWarningLabel";
			this.UnacceptedQuotesWarningLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 24, true);
			this.UnacceptedQuotesWarningLabel.TabIndex = 8;
			// 
			// rateEntryFilterStripOnPanel1
			// 
			this.rateEntryFilterStripControl.AllowDrop = true;
			this.rateEntryFilterStripControl.CaptionRenderingEnabled = true;
			this.rateEntryFilterStripControl.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left
			| System.Windows.Forms.AnchorStyles.Right;
			this.rateEntryFilterStripControl.BackColor = System.Drawing.Color.Transparent;
			this.BindingSource.SetBindingMember(this.rateEntryFilterStripControl, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(null);
			this.rateEntryFilterStripControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(314, 0, true);
			this.rateEntryFilterStripControl.Name = "rateEntryFilterStripControl";
			this.rateEntryFilterStripControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(710, 136, true);
			this.rateEntryFilterStripControl.TabIndex = 2;
			// 
			// ActiveRatesForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1024, 725, true);
			this.Controls.Add(this.rateEntryFilterStripControl);
			this.Controls.Add(this.RateEndDateEdit);
			this.Controls.Add(this.UnacceptedQuotesWarningLabel);
			this.Controls.Add(this.UnacceptedQuotesButton);
			this.Controls.Add(this.ratingTabControl1);
			this.Controls.Add(this.cfxUserControl1);
			this.Controls.Add(this.clientInformationUserControl1);
			this.Controls.Add(this.PostingButtonsUserControl);
			this.DataSourceAssemblyName = "Enterprise.Rating.Business";
			this.DataSourceType = typeof(ClientRate);
			this.DataSourceTypeName = "Enterprise.Rating.Business.ClientRate";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1024, 725, true);
			this.Name = "ActiveRatesForm";
			this.Text = "ActiveRatesForm";
			this.Controls.SetChildIndex(this.PostingButtonsUserControl, 0);
			this.Controls.SetChildIndex(this.clientInformationUserControl1, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.cfxUserControl1, 0);
			this.Controls.SetChildIndex(this.ratingTabControl1, 0);
			this.Controls.SetChildIndex(this.UnacceptedQuotesButton, 0);
			this.Controls.SetChildIndex(this.UnacceptedQuotesWarningLabel, 0);
			this.Controls.SetChildIndex(this.RateEndDateEdit, 0);
			this.Controls.SetChildIndex(this.rateEntryFilterStripControl, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
		}
	}
}
