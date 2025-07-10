using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using CargoWiseOne.ResourceStrings;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.GUI
{
	public partial class CompanyTariffAndCostingRateLineAndItemsControl
	{
		private ZCheckBox ClientRatesCheckBox;
		private ZCheckBox CostingCheckBox;
		private ZCheckBox CompanyTariffCheckBox;
		private Container components = null;

		private void InitializeComponent()
		{
			ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new ZCodeFindBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new ZGuidFindBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new ZGuidFindBoxColumnStyleInfo();
			ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfo1 = new ZMultiLineTextBoxColumnInfo();
			ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfo2 = new ZMultiLineTextBoxColumnInfo();
			ZDropEditColumnStyleInfo ZDropEditColumnStyleInfo1 = new ZDropEditColumnStyleInfo();
			this.ClientRatesCheckBox = new ZCheckBox();
			this.CostingCheckBox = new ZCheckBox();
			this.CompanyTariffCheckBox = new ZCheckBox();
			this.splitContainer.Panel1.SuspendLayout();
			this.splitContainer.Panel2.SuspendLayout();
			this.splitContainer.SuspendLayout();
			this.zPanel1.SuspendLayout();
			((ISupportInitialize)(this.RateLinesGrid)).BeginInit();
			((ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// splitContainer
			// 
			this.splitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(952, 176, true);
			// 
			// zPanel1
			// 
			this.zPanel1.Controls.Add(this.ClientRatesCheckBox);
			this.zPanel1.Controls.Add(this.CostingCheckBox);
			this.zPanel1.Controls.Add(this.CompanyTariffCheckBox);
			this.zPanel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(492, 176, true);
			this.zPanel1.TabIndex = 0;
			this.zPanel1.Controls.SetChildIndex(this.RateLinesGrid, 0);
			this.zPanel1.Controls.SetChildIndex(this.CompanyTariffCheckBox, 0);
			this.zPanel1.Controls.SetChildIndex(this.CostingCheckBox, 0);
			this.zPanel1.Controls.SetChildIndex(this.ClientRatesCheckBox, 0);
			// 
			// calculatorPanel1
			// 
			this.calculatorPanel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(456, 176, true);
			this.calculatorPanel1.TabIndex = 2;
			// 
			// RateLinesGrid
			// 
			zCodeFindBoxColumnStyleInfo1.ColumnName = "TL_RX_NKCurrency";
			zCodeFindBoxColumnStyleInfo1.IsVisible = false;
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("CompanyTariffAndCostingRateLineAndItemsControl|dd41db75-32bc-419d-9001-76e543f36af6", "Consignee");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "Consignee";
			zGuidFindBoxColumnStyleInfo1.IsVisible = false;
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zGuidFindBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("CompanyTariffAndCostingRateLineAndItemsControl|d380f28a-e7d4-46ab-8f55-6020ee21341b", "Consignor");
			zGuidFindBoxColumnStyleInfo2.ColumnName = "Consignor";
			zGuidFindBoxColumnStyleInfo2.IsVisible = false;
			zGuidFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zMultiLineTextBoxColumnInfo1.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("CompanyTariffAndCostingRateLineAndItemsControl|ad88436d-6981-4ed0-98c7-f505e44c23ea", "Public Note", "Trade Lane Charge Information Note.");
			zMultiLineTextBoxColumnInfo1.ColumnName = "ChargeInformationNoteText";
			zMultiLineTextBoxColumnInfo1.IsVisible = false;
			zMultiLineTextBoxColumnInfo1.MinimumEditControlWidth = 300;
			zMultiLineTextBoxColumnInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(300);
			zMultiLineTextBoxColumnInfo2.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("CompanyTariffAndCostingRateLineAndItemsControl|8caf3fe8-a2d5-4fff-969c-457478a33962", "Internal Note", "Trade Lane Charge Internal Note.");
			zMultiLineTextBoxColumnInfo2.ColumnName = "ChargeInternalNoteText";
			zMultiLineTextBoxColumnInfo2.IsVisible = false;
			zMultiLineTextBoxColumnInfo2.MinimumEditControlWidth = 300;
			zMultiLineTextBoxColumnInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(300);
			ZDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("CompanyTariffAndCostingRateLineAndItemsControl|2a78c6b2-99ab-46bd-a936-757e6d803d7b", "Is Non-Op. Reefer", "Is Non-Operating Reefer");
			ZDropEditColumnStyleInfo1.ColumnName = "IsNonOperatedReefer";
			ZDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			ZDropEditColumnStyleInfo1.CharacterCasing = CharacterCasing.Upper;
			this.RateLinesGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.RateLinesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.RateLinesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
			this.RateLinesGrid.ColumnStyles.Add(zMultiLineTextBoxColumnInfo1);
			this.RateLinesGrid.ColumnStyles.Add(zMultiLineTextBoxColumnInfo2);
			this.RateLinesGrid.ColumnStyles.Add(ZDropEditColumnStyleInfo1);
			this.RateLinesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 23, true);
			this.RateLinesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(492, 153, true);
			this.RateLinesGrid.TabIndex = 3;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(RatingHeader);
			// 
			// ClientRatesCheckBox
			// 
			this.ClientRatesCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ClientRatesCheckBox, "ShowClientRates");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RatingHeader)(null)).ShowClientRates);
			this.ClientRatesCheckBox.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("CompanyTariffAndCostingRateLineAndItemsControl|114fda15-a070-4b14-9098-cd6e0acb11cf", "Client Rates");
			this.ClientRatesCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ClientRatesCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(200, 3, true);
			this.ClientRatesCheckBox.Name = "ClientRatesCheckBox";
			this.ClientRatesCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.ClientRatesCheckBox.TabIndex = 2;
			this.ClientRatesCheckBox.ReadOnlyChanged += new EventHandler(this.CheckBox_ReadOnlyChanged);
			this.ClientRatesCheckBox.CheckStateChanged += new EventHandler(this.CheckBox_CheckStateChanged);
			// 
			// CostingCheckBox
			// 
			this.CostingCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.CostingCheckBox, "ShowCosting");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RatingHeader)(null)).ShowCosting);
			this.CostingCheckBox.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("CompanyTariffAndCostingRateLineAndItemsControl|a63877bf-7c37-4989-a456-d679fb9d55c0", "Costing");
			this.CostingCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.CostingCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(119, 3, true);
			this.CostingCheckBox.Name = "CostingCheckBox";
			this.CostingCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.CostingCheckBox.TabIndex = 1;
			this.CostingCheckBox.ReadOnlyChanged += new EventHandler(this.CheckBox_ReadOnlyChanged);
			this.CostingCheckBox.CheckStateChanged += new EventHandler(this.CheckBox_CheckStateChanged);
			// 
			// CompanyTariffCheckBox
			// 
			this.CompanyTariffCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.CompanyTariffCheckBox, "ShowCompanyTariff");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RatingHeader)(null)).ShowCompanyTariff);
			this.CompanyTariffCheckBox.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("CompanyTariffAndCostingRateLineAndItemsControl|c0632e9d-a546-45a2-8e2a-d9ad5ef323b5", "Tariff", "Company Tariff", "");
			this.CompanyTariffCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.CompanyTariffCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.CompanyTariffCheckBox.Name = "CompanyTariffCheckBox";
			this.CompanyTariffCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.CompanyTariffCheckBox.TabIndex = 0;
			this.CompanyTariffCheckBox.ReadOnlyChanged += new EventHandler(this.CheckBox_ReadOnlyChanged);
			this.CompanyTariffCheckBox.CheckStateChanged += new EventHandler(this.CheckBox_CheckStateChanged);
			// 
			// CompanyTariffAndCostingRateLineAndItemsControl
			// 
			this.Name = "CompanyTariffAndCostingRateLineAndItemsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(952, 176, true);
			this.splitContainer.Panel1.ResumeLayout(false);
			this.splitContainer.Panel2.ResumeLayout(false);
			this.splitContainer.ResumeLayout(false);
			this.zPanel1.ResumeLayout(false);
			this.zPanel1.PerformLayout();
			((ISupportInitialize)(this.RateLinesGrid)).EndInit();
			((ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
		}
	}

	public partial class CompanyTariffAndCostingRateLineAndItemsLazyControl
	{
		internal CompanyTariffAndCostingRateLineAndItemsControl InnerControl;
		internal ZLinkLabel ShowControlLabel;

		void InitializeComponent()
		{
			InnerControl = new CompanyTariffAndCostingRateLineAndItemsControl();
			ShowControlLabel = new ZLinkLabel();
			SuspendLayout();

			InnerControl.Name = "CompanyTariffAndCostingRateLineAndItemsControl";
			InnerControl.Visible = false;
			InnerControl.Dock = DockStyle.Fill;
			InnerControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			InnerControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(952, 176, true);
			BindingSource.SetBindingMember(InnerControl, ".");

			ShowControlLabel.Name = "ShowControlLabel";
			ShowControlLabel.Visible = true;
			ShowControlLabel.Dock = DockStyle.Fill;
			ShowControlLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			ShowControlLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(952, 176, true);
			ShowControlLabel.TextAlign = ContentAlignment.MiddleCenter;
			ShowControlLabel.Text = Res.GetString("4e586cd0-a7dc-481a-a08b-c02fc5649b20", "Click here to show related Rate Line and Items.");
			ShowControlLabel.Click += showControlLabel_Click;

			Controls.Add(InnerControl);
			Controls.Add(ShowControlLabel);

			Name = "CompanyTariffAndCostingRateLineAndItemsLazyControl";
			CaptionRenderingEnabled = true;
			Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(952, 176, true);
			ResumeLayout(false);
		}
	}
}
