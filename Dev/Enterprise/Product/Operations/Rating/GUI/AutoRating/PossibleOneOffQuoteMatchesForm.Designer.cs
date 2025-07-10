using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.GUI
{
	public partial class PossibleOneOffQuoteMatchesForm
	{
		private ZButton UpdateButton;
		private ZButton CancelButtonX;
		private ZArchitecture.ZGrid zGrid1;
		private ZArchitecture.ZLabel Text1;
		private ZArchitecture.ZLabel zLabel1;
		private OneOffQuoteDetailsControl oneOffQuoteDetailsControl1;
		private System.ComponentModel.Container components = null;

		new void InitializeComponent()
		{
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new ZArchitecture.ZDateEditColumnStyleInfo();
			this.UpdateButton = new ZButton();
			this.CancelButtonX = new ZButton();
			this.zGrid1 = new ZArchitecture.ZGrid();
			this.Text1 = new ZArchitecture.ZLabel();
			this.zLabel1 = new ZArchitecture.ZLabel();
			this.oneOffQuoteDetailsControl1 = new OneOffQuoteDetailsControl();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.zGrid1)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 378, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(954, 22, true);
			this.MainStatusBar.SizingGrip = false;
			this.MainStatusBar.TabIndex = 6;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(477);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(477);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(SimpleOneOffQuoteCollectionWrapper);
			// 
			// UpdateButton
			// 
			this.UpdateButton.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("PossibleOneOffQuoteMatchesForm|23d88aa3-44b0-4642-adc6-bc03cb757f4b", "Use Selected Quote");
			this.UpdateButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(676, 344, true);
			this.UpdateButton.Name = "UpdateButton";
			this.UpdateButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 23, true);
			this.UpdateButton.TabIndex = 4;
			this.UpdateButton.Click += new System.EventHandler(this.UpdateButton_Click);
			// 
			// CancelButtonX
			// 
			this.CancelButtonX.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("PossibleOneOffQuoteMatchesForm|adb77aed-9e7c-4879-b818-29136daa3b9f", "Use Standard Rates");
			this.CancelButtonX.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelButtonX.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(810, 344, true);
			this.CancelButtonX.Name = "CancelButtonX";
			this.CancelButtonX.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 23, true);
			this.CancelButtonX.TabIndex = 5;
			this.CancelButtonX.Click += new System.EventHandler(this.CancelButtonX_Click);
			// 
			// zGrid1
			// 
			this.zGrid1.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.zGrid1, "PossibleMatches");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((SimpleOneOffQuoteCollectionWrapper)(null)).PossibleMatches);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Quote)((System.Collections.IList)((SimpleOneOffQuoteCollectionWrapper)null).PossibleMatches).SyncRoot).TH_QuoteNumber);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Quote)((System.Collections.IList)((SimpleOneOffQuoteCollectionWrapper)null).PossibleMatches).SyncRoot).TH_StatusDate);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check((CargoWise.Types.ZDateTime)((Quote)((System.Collections.IList)((SimpleOneOffQuoteCollectionWrapper)null).PossibleMatches).SyncRoot).TH_QuoteDate);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check((CargoWise.Types.ZDateTime)((Quote)((System.Collections.IList)((SimpleOneOffQuoteCollectionWrapper)null).PossibleMatches).SyncRoot).TH_QuoteEndDate);
			this.zGrid1.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "TH_QuoteNumber";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("PossibleOneOffQuoteMatchesForm|7ae6f7d4-6572-43a7-9a30-65ed84394320", "Last Updated");
			zDateEditColumnStyleInfo1.ColumnName = "TH_StatusDate";
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDateEditColumnStyleInfo2.ColumnName = "TH_QuoteDate";
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDateEditColumnStyleInfo3.ColumnName = "TH_QuoteEndDate";
			zDateEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.zGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.zGrid1.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.zGrid1.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.zGrid1.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.zGrid1.GridId = "67808131-3013-4595-a630-2c55c0d3381d";
			this.zGrid1.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.zGrid1.IsWholeRowSelectedOnClick = true;
			this.zGrid1.LayoutKey = "zGrid1";
			this.zGrid1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 45, true);
			this.zGrid1.Name = "zGrid1";
			this.zGrid1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(935, 104, true);
			this.zGrid1.TabIndex = 1;
			// 
			// Text1
			// 
			this.Text1.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("PossibleOneOffQuoteMatchesForm|AC3960DF-5877-4712-B617-6198495964D0", "", "The following One Off Quotes are possible matches for this job. Please choose the correct match to bring through charges from the selected Quote in addition to Standard Rates. Or you can choose to not use any One Off Quotation by choosing \'Use Standard Rates\'.");
			this.Text1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 3, true);
			this.Text1.Name = "Text1";
			this.Text1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(928, 37, true);
			this.Text1.TabIndex = 0;
			// 
			// zLabel1
			// 
			this.zLabel1.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("PossibleOneOffQuoteMatchesForm|c0bdefdf-47dc-4b97-a845-e753d9537231", "", "If you use one of the above One Off Quotations for this job, you will not be able to use it on any further jobs.");
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 319, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(920, 23, true);
			this.zLabel1.TabIndex = 3;
			// 
			// oneOffQuoteDetailsControl1
			// 
			this.oneOffQuoteDetailsControl1.BindTo = "PossibleMatches.OneOffQuote";
			this.oneOffQuoteDetailsControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 155, true);
			this.oneOffQuoteDetailsControl1.Name = "oneOffQuoteDetailsControl1";
			this.oneOffQuoteDetailsControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(935, 162, true);
			this.oneOffQuoteDetailsControl1.TabIndex = 2;
			// 
			// PossibleOneOffQuoteMatchesForm
			// 

			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(954, 400, true);
			this.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("PossibleOneOffQuoteMatchesForm|7d3f7754-3f43-4a85-95dd-929dd3abd9c3", "One Off Quote Matches");
			this.Controls.Add(this.oneOffQuoteDetailsControl1);
			this.Controls.Add(this.zLabel1);
			this.Controls.Add(this.Text1);
			this.Controls.Add(this.zGrid1);
			this.Controls.Add(this.CancelButtonX);
			this.Controls.Add(this.UpdateButton);
			this.DataSourceAssemblyName = "Enterprise.Rating.Business";
			this.DataSourceType = typeof(SimpleOneOffQuoteCollectionWrapper);
			this.DataSourceTypeName = "Enterprise.Rating.Business.SimpleOneOffQuoteCollectionWrapper";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MinimizeBox = false;
			this.Name = "PossibleOneOffQuoteMatchesForm";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Controls.SetChildIndex(this.UpdateButton, 0);
			this.Controls.SetChildIndex(this.CancelButtonX, 0);
			this.Controls.SetChildIndex(this.zGrid1, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.Text1, 0);
			this.Controls.SetChildIndex(this.zLabel1, 0);
			this.Controls.SetChildIndex(this.oneOffQuoteDetailsControl1, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.zGrid1)).EndInit();
			this.ResumeLayout(false);
		}
	}
}
