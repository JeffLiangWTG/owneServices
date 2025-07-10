using CargoWiseOne.ResourceStrings;
using Enterprise.Registry.Business;

namespace Enterprise.MasterFiles.GUI.Organisation.UserControls
{
	partial class OrganisationRatingUserControl
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
			this.components = new System.ComponentModel.Container();
			this.OrganisationRatingTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.CompanyTariffTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.CompanyTariffUserControl = new Enterprise.MasterFiles.GUI.CompanyTariffsUserControl();
			this.FeesAndChargesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.feesAndChargesUserControl1 = new Enterprise.MasterFiles.GUI.Organisation.UserControls.FeesAndChargesUserControl();
			this.AutoratingDateFilteringTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.AutoratingDateFilteringUserControl = new Enterprise.MasterFiles.GUI.AutoratingDateFilteringUserControl();
			this.RateCommodityCodeDefaultingRuleTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.RateCommodityDefaultingRuleControl = new RateCommodityDefaultingRuleControl();
			this.PricingTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.PricingUserControl = new Enterprise.MasterFiles.GUI.Organisation.UserControls.PricingUserControl();
			this.ChargeCodePrintSequenceTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ChargeCodePrintSequenceUserControl = new Enterprise.MasterFiles.GUI.Organisation.UserControls.ChargeCodePrintSequenceUserControl();
			this.CaptionRenderingEnabled = true;
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.OrganisationRatingTabControl.SuspendLayout();
			this.CompanyTariffTabPage.SuspendLayout();
			this.FeesAndChargesTabPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgHeader);
			// 
			// OrganisationRatingTabControl
			// 
			this.OrganisationRatingTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.OrganisationRatingTabControl.Controls.Add(this.CompanyTariffTabPage);
			this.OrganisationRatingTabControl.Controls.Add(this.FeesAndChargesTabPage);
			this.OrganisationRatingTabControl.Controls.Add(this.AutoratingDateFilteringTabPage);
			this.OrganisationRatingTabControl.Controls.Add(this.RateCommodityCodeDefaultingRuleTabPage);
			this.OrganisationRatingTabControl.Controls.Add(this.PricingTabPage);
			this.OrganisationRatingTabControl.Controls.Add(this.ChargeCodePrintSequenceTabPage);
			this.OrganisationRatingTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.OrganisationRatingTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.OrganisationRatingTabControl.Name = "OrganisationRatingTabControl";
			this.OrganisationRatingTabControl.SelectedIndex = 0;
			this.OrganisationRatingTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(781, 558, true);
			this.OrganisationRatingTabControl.TabIndex = 0;
			// 
			// CompanyTariffTabPage
			// 
			this.CompanyTariffTabPage.Text = Res.GetString("eaeea794-969f-436e-acd5-ca19a7133b81", "Configuration");
			this.CompanyTariffTabPage.Controls.Add(this.CompanyTariffUserControl);
			this.CompanyTariffTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.CompanyTariffTabPage.Name = "CompanyTariffTabPage";
			this.CompanyTariffTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.CompanyTariffTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(773, 531, true);
			this.CompanyTariffTabPage.TabIndex = 0;
			this.CompanyTariffTabPage.UseVisualStyleBackColor = true;
			// 
			// CompanyTariffUserControl
			// 
			this.CompanyTariffUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CompanyTariffUserControl, ".");
			this.CompanyTariffUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CompanyTariffUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.CompanyTariffUserControl.Name = "CompanyTariffUserControl";
			this.CompanyTariffUserControl.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, true);
			this.CompanyTariffUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(767, 525, true);
			this.CompanyTariffUserControl.TabIndex = 1;
			// 
			// FeesAndChargesTabPage
			// 
			this.FeesAndChargesTabPage.Controls.Add(this.feesAndChargesUserControl1);
			this.FeesAndChargesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.FeesAndChargesTabPage.Name = "FeesAndChargesTabPage";
			this.FeesAndChargesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.FeesAndChargesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(773, 531, true);
			this.FeesAndChargesTabPage.TabIndex = 1;
			this.FeesAndChargesTabPage.Text = Res.GetString("a583429c-18a3-4b2f-8702-76925dbd3ffa", "Fees And Charges");
			this.FeesAndChargesTabPage.UseVisualStyleBackColor = true;
			// 
			// feesAndChargesUserControl1
			// 
			this.feesAndChargesUserControl1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.feesAndChargesUserControl1, "CompanyData.RateFeeChargeLevels");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.OrgRateFeeChargeLevelCollection)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.RateFeeChargeLevels)));
			this.feesAndChargesUserControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.feesAndChargesUserControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.feesAndChargesUserControl1.Name = "feesAndChargesUserControl1";
			this.feesAndChargesUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(767, 525, true);
			this.feesAndChargesUserControl1.TabIndex = 0;
			// 
			// AutoratingDateFilteringTabPage
			// 
			this.AutoratingDateFilteringTabPage.Text = Res.GetString("7939E522-AB29-4CDB-8B84-5F30D0A5E00F", "Autorating Date Filtering");
			this.AutoratingDateFilteringTabPage.Controls.Add(this.AutoratingDateFilteringUserControl);
			this.AutoratingDateFilteringTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.AutoratingDateFilteringTabPage.Name = "AutoratingDateFilteringTabPage";
			this.AutoratingDateFilteringTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.AutoratingDateFilteringTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(773, 531, true);
			this.AutoratingDateFilteringTabPage.TabIndex = 2;
			this.AutoratingDateFilteringTabPage.UseVisualStyleBackColor = true;
			// 
			// AutoratingDateFilteringUserControl
			// 
			this.AutoratingDateFilteringUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AutoratingDateFilteringUserControl, ".");
			this.AutoratingDateFilteringUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AutoratingDateFilteringUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.AutoratingDateFilteringUserControl.Name = "AutoratingDateFilteringUserControl";
			this.AutoratingDateFilteringUserControl.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, true);
			this.AutoratingDateFilteringUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(767, 525, true);
			this.AutoratingDateFilteringUserControl.TabIndex = 1;
			// 
			// RateCommodityCodeDefaultingRuleTabPage
			// 
			this.RateCommodityCodeDefaultingRuleTabPage.Text = Res.GetString("c1fd996c-5957-4bde-a1b5-0ea1f6f9c4dd", "Rate Commodity");
			this.RateCommodityCodeDefaultingRuleTabPage.Controls.Add(this.RateCommodityDefaultingRuleControl);
			this.RateCommodityCodeDefaultingRuleTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.RateCommodityCodeDefaultingRuleTabPage.Name = "RateCommodityCodeDefaultingRuleTabPage";
			this.RateCommodityCodeDefaultingRuleTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.RateCommodityCodeDefaultingRuleTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(773, 531, true);
			this.RateCommodityCodeDefaultingRuleTabPage.TabIndex = 3;
			this.RateCommodityCodeDefaultingRuleTabPage.UseVisualStyleBackColor = true;
			// 
			// RateCommodityDefaultingRuleControl
			//
			this.RateCommodityDefaultingRuleControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RateCommodityDefaultingRuleControl, "CompanyData.RateCommodityDefaultingRules");
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.OrgRateCommodityDefaultingRuleCollection)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.RateCommodityDefaultingRules)));
			this.RateCommodityDefaultingRuleControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RateCommodityDefaultingRuleControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.RateCommodityDefaultingRuleControl.Name = "RateCommodityDefaultingRuleControl";
			this.RateCommodityDefaultingRuleControl.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, true);
			this.RateCommodityDefaultingRuleControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(767, 525, true);
			this.RateCommodityDefaultingRuleControl.TabIndex = 1;
			//
			// PricingTabPage
			//
			this.PricingTabPage.Text = "Pricing";
			this.PricingTabPage.Controls.Add(this.PricingUserControl);
			this.PricingTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 20, true);
			this.PricingTabPage.Name = "PricingTab";
			this.PricingTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(881, 437, true);
			this.PricingTabPage.TabIndex = 0;
			this.PricingTabPage.UseVisualStyleBackColor = true;
			this.PricingTabPage.TabVisible = RatingDataRegistry.Instance.EnableQuotationDocumentsChargeGroupingSequencingAndRollup.Value;
			// 
			// PricingUserControl
			//
			this.PricingUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PricingUserControl, ".");
			this.PricingUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PricingUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 2, true);
			this.PricingUserControl.Name = "PricingDetailsGroupBox";
			this.PricingUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(750, 410, true);
			this.PricingUserControl.TabIndex = 0;
			this.PricingUserControl.TabStop = false;
			// 
			// ChargeCodePrintSequenceTabPage
			//
			this.ChargeCodePrintSequenceTabPage.Text = Res.GetString("ChargeCodePrintSequenceUserControl|3E6C1AA1-2FFF-4CE6-9BBD-DDD0AB991AD0", "Charge Code Print Sequence");
			this.ChargeCodePrintSequenceTabPage.Controls.Add(this.ChargeCodePrintSequenceUserControl);
			this.ChargeCodePrintSequenceTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 20, true);
			this.ChargeCodePrintSequenceTabPage.Name = "SequenceTab";
			this.ChargeCodePrintSequenceTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.ChargeCodePrintSequenceTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(881, 437, true);
			this.ChargeCodePrintSequenceTabPage.TabIndex = 1;
			this.ChargeCodePrintSequenceTabPage.TabVisible = RatingDataRegistry.Instance.EnableQuotationDocumentsChargeGroupingSequencingAndRollup.Value;
			//
			// ChargeCodePrintSequenceUserControl
			//
			this.ChargeCodePrintSequenceUserControl.AllowDrop = true;
			this.ChargeCodePrintSequenceUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ChargeCodePrintSequenceUserControl, ".");
			this.ChargeCodePrintSequenceUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.ChargeCodePrintSequenceUserControl.Name = "ChargeCodePrintSequenceUserControl";
			this.ChargeCodePrintSequenceUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(850, 419, true);
			this.ChargeCodePrintSequenceUserControl.TabIndex = 0;
			this.ChargeCodePrintSequenceUserControl.TabStop = false;
			// 
			// OrganisationRatingUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.OrganisationRatingTabControl);
			this.Name = "OrganisationRatingUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(781, 558, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.OrganisationRatingTabControl.ResumeLayout(false);
			this.CompanyTariffTabPage.ResumeLayout(false);
			this.FeesAndChargesTabPage.ResumeLayout(false);
			this.AutoratingDateFilteringTabPage.ResumeLayout(false);
			this.ResumeLayout(false);
		}

		#endregion

		private ZArchitecture.GUI.ZTemplateTabControl OrganisationRatingTabControl;
		private ZArchitecture.GUI.ZTabPage CompanyTariffTabPage;
		private ZArchitecture.GUI.ZTabPage FeesAndChargesTabPage;
		private ZArchitecture.GUI.ZTabPage AutoratingDateFilteringTabPage;
		private ZArchitecture.GUI.ZTabPage RateCommodityCodeDefaultingRuleTabPage;
		private ZArchitecture.GUI.ZTabPage PricingTabPage;
		private ZArchitecture.GUI.ZTabPage ChargeCodePrintSequenceTabPage;
		internal CompanyTariffsUserControl CompanyTariffUserControl;
		private FeesAndChargesUserControl feesAndChargesUserControl1;
		AutoratingDateFilteringUserControl AutoratingDateFilteringUserControl;
		RateCommodityDefaultingRuleControl RateCommodityDefaultingRuleControl;
		PricingUserControl PricingUserControl;
		ChargeCodePrintSequenceUserControl ChargeCodePrintSequenceUserControl;
	}
}
