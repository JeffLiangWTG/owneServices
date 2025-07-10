using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.GUI
{
	public partial class RateCommodityFMCSelectorForm
	{
		private ZButton noButton;
		private ZArchitecture.ZGrid zGrid1;
		private ZArchitecture.ZLabel Text1;
		private ZButton yesButton;

		private System.ComponentModel.Container components = null;

		new void InitializeComponent()
		{
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			this.noButton = new ZButton();
			this.zGrid1 = new ZArchitecture.ZGrid();
			this.Text1 = new ZArchitecture.ZLabel();
			this.yesButton = new ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.zGrid1)).BeginInit();
			this.zGrid1.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 323, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(627, 22, true);
			this.MainStatusBar.SizingGrip = false;
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
			this.BindingSource.DataSourceType = typeof(RateCommodityFMCSelectorViewModel);
			// 
			// noButton
			// 
			this.noButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
			this.noButton.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("CompanyTariffRateSelectorForm|AFDCFFE1-F306-47BB-AF63-E7DD24D736F4", "No");
			this.noButton.DialogResult = System.Windows.Forms.DialogResult.No;
			this.noButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(487, 294, true);
			this.noButton.Name = "noButton";
			this.noButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 23, true);
			this.noButton.TabIndex = 3;
			this.noButton.ToolTipCaption = null;
			this.noButton.Click += new System.EventHandler(this.NoButton_Click);
			// 
			// zGrid1
			// 
			this.zGrid1.AllowNavigation = false;
			this.zGrid1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom
			| System.Windows.Forms.AnchorStyles.Left
			| System.Windows.Forms.AnchorStyles.Right;
			this.BindingSource.SetBindingMember(this.zGrid1, "CompanyTariffs");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RateCommodityFMCSelectorViewModel)(null)).CompanyTariffs);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RateCommodityFMCViewModel)(((System.Collections.IList)(((RateCommodityFMCSelectorViewModel)(null)).CompanyTariffs)).SyncRoot)).Commodity);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RateCommodityFMCViewModel)(((System.Collections.IList)(((RateCommodityFMCSelectorViewModel)(null)).CompanyTariffs)).SyncRoot)).LocalCode);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RateCommodityFMCViewModel)(((System.Collections.IList)(((RateCommodityFMCSelectorViewModel)(null)).CompanyTariffs)).SyncRoot)).FMCTariffID);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RateCommodityFMCViewModel)(((System.Collections.IList)(((RateCommodityFMCSelectorViewModel)(null)).CompanyTariffs)).SyncRoot)).Description);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RateCommodityFMCViewModel)(((System.Collections.IList)(((RateCommodityFMCSelectorViewModel)(null)).CompanyTariffs)).SyncRoot)).RateSource);
			this.zGrid1.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("CompanyTariffRateSelectorForm|44C70FF5-F708-4AD4-A62E-EACF231172FC", "Commodity");
			zTextBoxColumnStyleInfo1.ColumnName = "Commodity";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("CompanyTariffRateSelectorForm|84A34918-F141-4ABF-9DC5-DB6814AA8F90", "Comm. LC", "Commodity Local Code");
			zTextBoxColumnStyleInfo2.ColumnName = "LocalCode";
			zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("CompanyTariffRateSelectorForm|8DB170F2-6DAE-4DD2-977F-654824B0D237", "FMC Tariff ID");
			zTextBoxColumnStyleInfo3.ColumnName = "FMCTariffID";
			zTextBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("CompanyTariffRateSelectorForm|0C16E272-9FF0-40A8-B471-89A1188E6ADB", "Commodity Description");
			zTextBoxColumnStyleInfo4.ColumnName = "Description";
			zTextBoxColumnStyleInfo4.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(400);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("CompanyTariffRateSelectorForm|17f20312-9dda-4624-99c2-66ccbc8fd5ed", "Rate Source");
			zTextBoxColumnStyleInfo5.ColumnName = "RateSource";
			zTextBoxColumnStyleInfo5.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.zGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.zGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.zGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.zGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.zGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.zGrid1.GridId = "F50144B1-A069-4B4B-9B65-BD5F9537B95D";
			this.zGrid1.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.zGrid1.IsWholeRowSelectedOnClick = true;
			this.zGrid1.LayoutKey = "zGrid1";
			this.zGrid1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 85, true);
			this.zGrid1.Name = "zGrid1";
			this.zGrid1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(606, 194, true);
			this.zGrid1.TabIndex = 1;

			// 
			// Text1
			//
			this.Text1.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("CompanyTariffRateSelectorForm|AC3960DF-5877-4712-B617-6198495964D0", "", @"Following combinations of Commodity and FMC Tariff ID in Company Tariffs and Client Rates are found, would you like to choose one of them for populating to the job for Autorating purposes?

Note: Please note that these combination are found regardless of Direction, Payment Term and Incoterm. The charges under these combination may not be applicable during autorating revenue.");
			this.Text1.FontType = Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif;
			this.Text1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 3, true);
			this.Text1.Name = "Text1";
			this.Text1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(599, 78, true);
			this.Text1.TabIndex = 0;
			this.Text1.UseMnemonic = false;
			// 
			// yesButton
			// 
			this.yesButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
			this.yesButton.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("CompanyTariffRateSelectorForm|605875FE-23A4-4C4F-9DB4-10554385EC38", "Yes");
			this.yesButton.DialogResult = System.Windows.Forms.DialogResult.Yes;
			this.yesButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(349, 294, true);
			this.yesButton.Name = "yesButton";
			this.yesButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 23, true);
			this.yesButton.TabIndex = 2;
			this.yesButton.ToolTipCaption = null;
			this.yesButton.Click += new System.EventHandler(this.YesButton_Click);
			// 
			// CompanyTariffRateSelectorForm
			// 
			this.AcceptButton = this.yesButton;
			this.CancelButton = this.noButton;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("CompanyTariffRateSelectorForm|D0D9238E-7B24-4194-AF59-1406799C6B14", "Company Tariff and Client Rate Selection");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(627, 345, true);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(627, 345, true);
			this.Controls.Add(this.yesButton);
			this.Controls.Add(this.Text1);
			this.Controls.Add(this.zGrid1);
			this.Controls.Add(this.noButton);
			this.DataSourceAssemblyName = "Enterprise.Rating.Business";
			this.DataSourceType = typeof(RateCommodityFMCSelectorViewModel);
			this.DataSourceTypeName = "Enterprise.Rating.GUI.CompanyTariffRateSelectorViewModel";
			this.MinimizeBox = false;
			this.Name = "CompanyTariffRateSelectorForm";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Controls.SetChildIndex(this.noButton, 0);
			this.Controls.SetChildIndex(this.zGrid1, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.Text1, 0);
			this.Controls.SetChildIndex(this.yesButton, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.zGrid1)).EndInit();
			this.zGrid1.ResumeLayout(false);
			this.zGrid1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
