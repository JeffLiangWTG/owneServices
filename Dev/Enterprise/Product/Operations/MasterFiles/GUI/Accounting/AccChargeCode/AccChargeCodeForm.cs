using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.GUI
{
	public partial class AccChargeCodeForm : ZForm
	{
		public AccChargeCodeForm()
		{
		}

		public AccChargeCodeForm(AccChargeCode chargeCode)
			: base(chargeCode)
		{
			ZFormPostingButtonsStrategy.SetupPosting(this, ButtonsUserControl);
			PlugIns.Add(ControllerIDs.Audit);
			PlugIns.Add(ControllerIDs.eDocsPlugIn);
			SetupCountrySpecificValues();
			SetupActionsMenu();
		}

		#region Implementation

		ZMenuItem showDifferenceWarningsMenuItem;

		void SetupActionsMenu()
		{
			if (ChargeCode.IsGlobal || ChargeCode.IsLinkedToGlobalChargeCode)
			{
				showDifferenceWarningsMenuItem = new ZMenuItem(ResString.GetMultilingualString("33cb3ce8-b732-47f6-966a-98d720181443", "Show All Global Differences"),
							new EventHandler((s, e) => { ShowDifferenceWarnings = !ShowDifferenceWarnings; }));
				ActionsMenuItem.MenuItems.Add(showDifferenceWarningsMenuItem);
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public ZBool ShowDifferenceWarnings
		{
			get
			{
				return ChargeCode.ShowDifferenceWarnings;
			}
			set
			{
				ChargeCode.ShowDifferenceWarnings = value;
				showDifferenceWarningsMenuItem.Checked = ChargeCode.ShowDifferenceWarnings;
				ChargeCode.RunPreSaveValidation();
			}
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();

			DelayedControlInitializationToDisplayWithFullWidth();

			if (!DesignModeFinder.IsDesigning)
			{
				if (DisplayPolicy.HideGovtChargeCode)
				{
					GovtChargeCodeTextBox.Visible = false;
					GovtChargeCodeOverrideTabPage.TabVisible = false;
				}

				if (DisplayPolicy.HidePlaceOfSupplyConfigurationTab)
				{
					PlaceOfSupplyConfigurationTabPage.TabVisible = false;
				}

				if (DisplayPolicy.HideSupplyTypeOverrideTab)
				{
					SupplyTypeOverrideTabPage.TabVisible = false;
				}

				if (DisplayPolicy.HideGSTTaxOverridesTab)
				{
					TaxOverridesTabPage.TabVisible = false;
				}

				if (DisplayPolicy.HideBranchOverridesTab)
				{
					BranchOverridesTab.TabVisible = false;
				}

				if (DisplayPolicy.HideSellComplianceDescriptionTab)
				{
					SellComplianceDescriptionTabPage.TabVisible = false;
				}

				if (DisplayPolicy.HideAirlineIATACodeTab)
				{
					AirlineIATACodeTab.TabVisible = false;
				}

				if (DisplayPolicy.HideCreditorOverrideTab)
				{
					CreditorOverridesTab.TabVisible = false;
				}

				if (!DisplayPolicy.HideGSTTaxID)
				{
					DetailsTabPage.RunWhenBindingOrFirstShown(
						delegate
						{
							AC_AT_GSTRateBoundGuidFindBox.GetExtension<ILabelCaptionRenderer>().Caption = GlbCompany.CurrentCompany.Country.ConsumptionTaxDescription + " " + Res.GetString("AccChargeCodeForm|eb1eebc0-65eb-49dc-825c-179944973801", "Tax ID");
							FormToolTip.SetToolTip(AC_AT_GSTRateBoundGuidFindBox,
								Res.GetString("Accounting|AccChargeCodeForm|FormToolTip", "If the Current Company is {0}/Tax Registered, this field will be enabled.", GlbCompany.CurrentCompany.Country.ConsumptionTaxDescription));

							InputGSTVATRecoverableCalcEdit.GetExtension<ILabelCaptionRenderer>().Caption = Res.GetString("0c3c7a84-2376-4040-96d7-59245f063b38", "{0} Recoverable %", GlbCompany.CurrentCompany.Country.ConsumptionTaxDescription);
							FormToolTip.SetToolTip(InputGSTVATRecoverableCalcEdit,
								Res.GetString("Accounting|AccChargeCodeForm|InputGSTVATRecoverableCalcEditFormToolTip", "If the Current Company is {0}/Tax Registered, this field will be enabled.", GlbCompany.CurrentCompany.Country.ConsumptionTaxDescription));
						});
				}

				if (!DisplayPolicy.HideGSTTaxOverridesTab)
				{
					TaxOverridesTabPage.RunWhenBindingOrFirstShown(
						delegate
						{
							if (!GlbCompany.CurrentCompany.Country.IsPartOfEuropeanUnion)
							{
								foreach (ZGridColumnInfo column in TaxOverridesGrid.ColumnStyles)
								{
									if (column.ColumnName == AccChargeTaxOverride.Schema.AO_CustomsStatus)
									{
										TaxOverridesGrid.ColumnStyles.Remove(column);
										break;
									}
								}
							}

							if (GlbCompany.CurrentCompany.Country.Code != Core.Constants.CountryCodes.Italy)
							{
								foreach (ZGridColumnInfo column in TaxOverridesGrid.ColumnStyles)
								{
									if (column.ColumnName == AccChargeTaxOverride.Schema.AO_SplitPaymentVATOrganisation)
									{
										TaxOverridesGrid.ColumnStyles.Remove(column);
										break;
									}
								}
							}

							if (!AccountingMasterFilesRegistry.Instance.EnableBranchLevelTaxOverrideRuleConfigurations.Value)
							{
								foreach (ZGridColumnInfo column in TaxOverridesGrid.ColumnStyles)
								{
									if (column.ColumnName == AccChargeTaxOverride.Schema.AO_GB)
									{
										TaxOverridesGrid.ColumnStyles.Remove(column);
										break;
									}
								}
							}

							if (!AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.Value)
							{
								foreach (ZGridColumnInfo column in TaxOverridesGrid.ColumnStyles)
								{
									if (column.ColumnName == AccChargeTaxOverride.Schema.AO_SupplyType)
									{
										TaxOverridesGrid.ColumnStyles.Remove(column);
										break;
									}
								}
							}
						});
				}
			}
		}

		void ChangeVisibleAccordingToRegistry()
		{
			if (!ObjectFactory.Get<IAccounting>().EnableBulkDisbursementJobsClosure)
			{
				DetailsDisbursementSurplusAccountGuidFindBox.Visible = false;
				DetailsDisbursementShortfallAccountGuidFindBox.Visible = false;
				GLAccountSetupGroupBox.Size = ControlDpiScalingHelper.NewScaledSize(GLAccountSetupGroupBox.Size.Width, GLAccountSetupGroupBox.Size.Height - ControlDpiScalingHelper.ScaleToCurrentDpiY(50), false);
				AutoRatingGroupBox.Location = ControlDpiScalingHelper.NewScaledPoint(AutoRatingGroupBox.Location.X, AutoRatingGroupBox.Location.Y - ControlDpiScalingHelper.ScaleToCurrentDpiY(50), false);
			}
		}

		protected AccChargeCode ChargeCode
		{
			get { return (AccChargeCode)BusinessEntity; }
		}

		protected override ContinueWithSave ShowPreSaveDialogs()
		{
			ContinueWithSave result = base.ShowPreSaveDialogs();
			if (result == ContinueWithSave.Yes && !ChargeCode.AC_IsActive)
			{
				string warningText = Res.GetString("8858f31b-72d0-4bcb-a804-f7140736e92b", "This charge code will be removed from all existing charge code sequence setups. Proceed?");
				DialogResult messageResult = Globals.Message.Show(warningText, Res.GetString("32104c3c-4af5-47b8-8bb6-14936291c9a4", "Remove charge code sequence setup"), MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
				result = messageResult == DialogResult.OK ? ContinueWithSave.Yes : ContinueWithSave.No;
			}
			return result;
		}

		protected virtual bool LocalLanguageDescriptionTextBoxShouldBeVisible
		{
			get { return ObjectFactory.Get<IAccounting>().EnableLocalChargeCodeDescriptionDefault; }
		}

		void SetupCountrySpecificValues()
		{
			LocalLanguageDescriptionTextBox.Visible = LocalLanguageDescriptionTextBoxShouldBeVisible;

			if (!LocalLanguageDescriptionTextBoxShouldBeVisible)
			{
				ControlDpiScalingHelper.SetTop(ref AC_DepartmentFilterListTextBox, descriptionZTranslatableTextControl.Bottom + ControlDpiScalingHelper.ScaleToCurrentDpiY(8), false);

				ControlDpiScalingHelper.SetTop(ref PrintSequenceEdit, AC_DepartmentFilterListTextBox.Bottom + ControlDpiScalingHelper.ScaleToCurrentDpiY(8), false);
				ControlDpiScalingHelper.SetTop(ref GoodServiceTypeDropEdit, AC_DepartmentFilterListTextBox.Bottom + ControlDpiScalingHelper.ScaleToCurrentDpiY(8), false);
				ControlDpiScalingHelper.SetTop(ref GovtChargeCodeTextBox, AC_DepartmentFilterListTextBox.Bottom + ControlDpiScalingHelper.ScaleToCurrentDpiY(8), false);

				ControlDpiScalingHelper.SetTop(ref ChargeCodeTabControl, PrintSequenceEdit.Bottom + ControlDpiScalingHelper.ScaleToCurrentDpiY(16), false);
				ControlDpiScalingHelper.SetHeight(this, this.Height - ControlDpiScalingHelper.ScaleToCurrentDpiY(20), false);
			}

			if (!DisplayPolicy.HideGSTTaxOverridesTab)
			{
				TaxOverridesTabPage.Text = GlbCompany.CurrentCompany.Country.ConsumptionTaxDescription + " " + Res.GetString("Accounting|AccChargeCodeForm|TaxOverridesTabPageCaptionSuffix", "Tax Overrides");
			}
		}

		void SetupIATAChargeCodeMapping()
		{
			AC_IATA_ChargeCodeMapDropEdit.Visible = Env.Registry.Freight.AirWaybill.ShowChargeCodeForOtherChargesInHAWBScreen;
		}

		protected override void OnLoad(EventArgs e)
		{
			if (DesignModeFinder.IsDesigning)
			{
				return;
			}

			base.OnLoad(e);
			SetupIATAChargeCodeMapping();
			ChargeCode.OnLoadedGUIValidation();
			ChangeVisibleAccordingToRegistry();
		}

		protected override void Save(ITransactionParticipant[] factories)
		{
			WarnIfGlAccountSetupChanged();
			try
			{
				base.Save(factories);
			}
			catch (AccChargeCode.ValidationOnLocalChargeCodeException ex)
			{
				Globals.Message.ShowError(Res.GetString("a303f1e0-f9b6-491b-8338-41927dca23c3", @"{0}

This form will now close.", ex.Message), ex.Heading);

				if (!ex.OnTrialRun)
				{
					// Exception happened during the factory save on this Factory. Factory will be dirty with child record changes, 
					// but we don't know which changes were made by user vs. which were made by saving code, so
					// we have no choice but to close the form.
					Close();
				}
				return;
			}

			if (!IsViewOrDeleteMode)
			{
				ValidateAll(ValidationType.Full);
			}
		}

		void WarnIfGlAccountSetupChanged()
		{
			if (!ChargeCode.IsDeleted &&
				((ZGuid)ChargeCode.AC_AG_AccrualAccountInfo.OriginalValue != ChargeCode.AC_AG_AccrualAccount
				|| (ZGuid)ChargeCode.AC_AG_CostAccountInfo.OriginalValue != ChargeCode.AC_AG_CostAccount
				|| (ZGuid)ChargeCode.AC_AG_RevenueAccountInfo.OriginalValue != ChargeCode.AC_AG_RevenueAccount
				|| (ZGuid)ChargeCode.AC_AG_WIPAccountInfo.OriginalValue != ChargeCode.AC_AG_WIPAccount
				|| ChargeCode.GLPostingOverrides.HasChanges))
			{
				Globals.Message.ShowWarning(Res.GetString("8448d545-4ee4-4284-b76b-8270ef336fc0", @"This GL Account change will be used when posting new & future transactions.  
Transactions already posted under the previous configuration will remain unchanged. Those transactions will remain posted against the previous GL account configuration."));
			}
		}

		public override string FormCaption
		{
			get
			{
				if (!ChargeCode.IsGlobal && Env.CurrentCompany.PK != ChargeCode.AC_GC.ToGuid())
				{
					return Res.GetString("AccChargeCodeForm|c133f4b3-bd52-4e5d-9186-a474e7bed531", "Charge Code for company {0} ({1})", ChargeCode.Company.GC_Code, ChargeCode.Company.GC_Name);
				}
				else
				{
					return base.FormCaption;
				}
			}
		}

		protected override void SaveToRecentItems()
		{
			if (ChargeCode?.AC_GC == GlbCompany.CurrentCompany.PK)
			{
				base.SaveToRecentItems();
			}
		}

		#endregion

		#region Display Policy
		AccChargeCodeFormDisplayPolicy displayPolicy;

		protected virtual AccChargeCodeFormDisplayPolicy DisplayPolicy
		{
			get
			{
				if (displayPolicy == null)
				{
					displayPolicy = new AccChargeCodeFormDisplayPolicy();
					if (!DesignModeFinder.IsDesigning)
					{
						displayPolicy.HideGovtChargeCode = !AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.Value;
						displayPolicy.HideAirlineIATACodeTab = !Env.Registry.Freight.AirWaybill.ShowChargeCodeForOtherChargesInHAWBScreen;
						displayPolicy.HidePlaceOfSupplyConfigurationTab = !PlaceOfSupplyHelper.IsPlaceOfSupplyEnabled(ChargeCode.Company);
						displayPolicy.HideSellComplianceDescriptionTab = !(AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.Value && AccountingMasterFilesRegistry.Instance.EnableSellComplianceDescription.Value);
						displayPolicy.HideSupplyTypeOverrideTab = !AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.Value;
					}
				}
				return displayPolicy;
			}
		}

		public class AccChargeCodeFormDisplayPolicy
		{
			public bool HideGSTTaxID { get; set; }
			public bool HideWithholdingTaxID { get; set; }
			public bool HideGSTTaxOverridesTab { get; set; }
			public bool HideBranchOverridesTab { get; set; }
			public bool HideSellComplianceDescriptionTab { get; set; }
			public bool HideGovtChargeCode { get; set; }
			public bool HideAirlineIATACodeTab { get; set; }
			public bool HideCreditorOverrideTab { get; set; }
			public bool HidePlaceOfSupplyConfigurationTab { get; set; }
			public bool HideCreditorOverrideTab_CreditorColumns { get; set; }
			public bool HideSupplyTypeOverrideTab { get; set; }
		}

		void AC_AT_GSTRateBoundGuidFindBox_Load(object sender, EventArgs e)
		{
			if (DisplayPolicy.HideGSTTaxID)
			{
				AC_AT_GSTRateBoundGuidFindBox.Visible = false;
				InputGSTVATRecoverableCalcEdit.Visible = false;
			}
		}

		void AC_AW_WithholdingTaxRateBoundGuidFindBox_Load(object sender, EventArgs e)
		{
			if (DisplayPolicy.HideWithholdingTaxID)
			{
				AC_AW_WithholdingTaxRateBoundGuidFindBox.Visible = false;
			}
		}

		void CreditorOverridesTab_TabInitialized(object sender, EventArgs e)
		{
			if (DisplayPolicy.HideCreditorOverrideTab_CreditorColumns)
			{
				CreditorOverridesGrid.SetColumnVisible(false, new[] { "ACC_OH_Creditor", "CreditorName" });
			}
		}

		#endregion
	}
}
