using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class ConsigneeDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestControlsReadOnlyAsPerOrgConsigneeModifyDetailsSecurityRight()
		{
			var oldValue = Env.Security.OrgConsigneeModifyDetails.IsAllowed;

			try
			{
				var org = Factory.New<OrgHeader>();
				Env.Security.OrgConsigneeModifyDetails.IsAllowed = false;

				using (var form1 = new ZForm(org))
				using (var control1 = new DetailsControlForTest())
				{
					form1.Controls.Add(control1);
					form1.Show();
					CombineAssertions("Assert controls read-only if OrgConsigneeModifyDetails.IsAllowed is false", () =>
					{
						AssertEquals("ASNDefaultFieldTypeGrid", true, control1.ASNDefaultFieldTypeGrid.ReadOnly);
						AssertEquals("OM_IMDefaultINCOTermDropEdit", true, control1.OM_IMDefaultINCOTermDropEdit.ReadOnly);
						AssertEquals("OM_IMImporterRequiresOrderNumbersOnDocsCheckBox", true, control1.OM_IMImporterRequiresOrderNumbersOnDocsCheckBox.ReadOnly);
						AssertEquals("OM_RS_NKIMDefaultServiceLevelBoundCodeFindBox", true, control1.OM_RS_NKIMDefaultServiceLevelBoundCodeFindBox.ReadOnly);
						AssertEquals("OM_IMSeaDepotFreeDaysBoundCalcEdit", true, control1.OM_IMSeaDepotFreeDaysBoundCalcEdit.ReadOnly);
						AssertEquals("OM_IMAirDepotFreeDaysBoundCalcEdit", true, control1.OM_IMAirDepotFreeDaysBoundCalcEdit.ReadOnly);
						AssertEquals("OM_IMCopySeaBillsBoundCalcEdit", true, control1.OM_IMCopySeaBillsBoundCalcEdit.ReadOnly);
						AssertEquals("OM_IMImporterCategoryBoundDropEdit", true, control1.OM_IMImporterCategoryBoundDropEdit.ReadOnly);
						AssertEquals("OM_IMJobRequireOrderTrackLinkBoundCheckEdit", true, control1.OM_IMJobRequireOrderTrackLinkBoundCheckEdit.ReadOnly);
						AssertEquals("OM_IMMergeCustomsInvoiceLinesByBoundDropEdit", true, control1.OM_IMMergeCustomsInvoiceLinesByBoundDropEdit.ReadOnly);
						AssertEquals("OM_IMOriginalSeaBillsBoundCalcEdit", true, control1.OM_IMOriginalSeaBillsBoundCalcEdit.ReadOnly);
						AssertEquals("OM_IMPaymentMethodDropEdit", true, control1.OM_IMPaymentMethodDropEdit.ReadOnly);
						AssertEquals("OwnersRefInitialNumberButton", true, control1.OwnersRefInitialNumberButton.ReadOnly);
						AssertEquals("AutoImporterJobRefCheckBox", true, control1.AutoImporterJobRefCheckBox.ReadOnly);
						AssertEquals("ProductImportAuditDropEdit", true, control1.ProductImportAuditDropEdit.ReadOnly);
						AssertEquals("OM_IMAutoPopulateOwnerRefWithOrderNumsDropEdit", true, control1.OM_IMAutoPopulateOwnerRefWithOrderNumsDropEdit.ReadOnly);
						AssertEquals("OM_IMEnablePromptToCreateProductsDropEdit", true, control1.OM_IMEnablePromptToCreateProductsDropEdit.ReadOnly);
						AssertEquals("OM_IMDocumentAddressPreferenceDropEdit", true, control1.OM_IMDocumentAddressPreferenceDropEdit.ReadOnly);
						AssertEquals("OM_IMDefaultToNewOrdersToNextOrderNumCheckBox", true, control1.OM_IMDefaultToNewOrdersToNextOrderNumCheckBox.ReadOnly);
						AssertEquals("OM_IMAllowAttachedOrderXMLUpdateCheckBox", true, control1.OM_IMAllowAttachedOrderXMLUpdateCheckBox.ReadOnly);
						AssertEquals("OM_IMLastOrderReferenceTextBox", true, control1.OM_IMLastOrderReferenceTextBox.ReadOnly);
						AssertEquals("RelatedPartiesGrid", true, control1.RelatedPartiesGrid.ReadOnly);
						AssertEquals("ContainerDetentionGrid", true, control1.ContainerDetentionGrid.ReadOnly);
						AssertEquals("OM_IMShowDutyOnWarehouseEntriesCheckBox", true, control1.OM_IMShowDutyOnWarehouseEntriesCheckBox.ReadOnly);
						AssertEquals("OM_IMEftQuarantineFromImportCheckBox", true, control1.OM_IMEftQuarantineFromImportCheckBox.ReadOnly);
						AssertEquals("OM_IMEFTBankAccountBoundTextBox", true, control1.OM_IMEFTBankAccountBoundTextBox.ReadOnly);
						AssertEquals("OM_IMEFTBankBSBBoundTextBox", true, control1.OM_IMEFTBankBSBBoundTextBox.ReadOnly);
						AssertEquals("OM_IMMinEFTAmountBoundCalcEdit", true, control1.OM_IMMinEFTAmountBoundCalcEdit.ReadOnly);
						AssertEquals("OM_IMMaxEFTAmountBoundCalcEdit", true, control1.OM_IMMaxEFTAmountBoundCalcEdit.ReadOnly);
						AssertEquals("OM_IMEftCustomsFromImportBoundCheckEdit", true, control1.OM_IMEftCustomsFromImportBoundCheckEdit.ReadOnly);
						AssertEquals("OM_IMEftHoldUntilPayAuthorisedBoundCheckEdit", true, control1.OM_IMEftHoldUntilPayAuthorisedBoundCheckEdit.ReadOnly);
						AssertEquals("OM_IMIsGSTDeferredBoundCheckBox", true, control1.OM_IMIsGSTDeferredBoundCheckBox.ReadOnly);
						AssertEquals("exportBillAgentChargesDirectCheckBox", true, control1.exportBillAgentChargesDirectCheckBox.ReadOnly);
						AssertEquals("OM_ConsigneeAuthorityToLeaveDropEdit", true, control1.OM_ConsigneeAuthorityToLeaveDropEdit.ReadOnly);
						AssertEquals("OM_IMOwnsProductsCheckBox", true, control1.OM_IMOwnsProductsCheckBox.ReadOnly);
						AssertEquals("OM_IMDisallowOrdersCheckBox", true, control1.OM_IMDisallowOrdersCheckBox.ReadOnly);
						AssertEquals("OM_IMBalanceInvoicePackageCheckBox", true, control1.OM_IMBalanceInvoicePackageCheckBox.ReadOnly);
						AssertEquals("ImporterOverrideCheckBox", true, control1.ImporterOverrideCheckBox.ReadOnly);
						AssertEquals("OM_IMSendImportDocsDropEdit", true, control1.OM_IMSendImportDocsDropEdit.ReadOnly);
						AssertEquals("OM_IMSendSeaImportDocsDropEdit", true, control1.OM_IMSendSeaImportDocsDropEdit.ReadOnly);
						AssertEquals("IsDutyDeferredCheckBox", true, control1.IsDutyDeferredCheckBox.ReadOnly);
						AssertEquals("OM_IMAdvanceCargoReportingSelfFilerCheckBox", true, control1.OM_IMAdvanceCargoReportingSelfFilerCheckBox.ReadOnly);
					});
				}

				Env.Security.OrgConsigneeModifyDetails.IsAllowed = true;

				using (var form2 = new ZForm(org))
				using (var control2 = new DetailsControlForTest())
				{
					form2.Controls.Add(control2);
					form2.Show();
					CombineAssertions("Assert controls editable if OrgConsigneeModifyDetails.IsAllowed is true", () =>
					{
						AssertEquals("OM_IMDefaultINCOTermDropEdit", false, control2.OM_IMDefaultINCOTermDropEdit.ReadOnly);
						AssertEquals("OM_IMImporterRequiresOrderNumbersOnDocsCheckBox", false, control2.OM_IMImporterRequiresOrderNumbersOnDocsCheckBox.ReadOnly);
						AssertEquals("OM_RS_NKIMDefaultServiceLevelBoundCodeFindBox", false, control2.OM_RS_NKIMDefaultServiceLevelBoundCodeFindBox.ReadOnly);
						AssertEquals("OM_IMSeaDepotFreeDaysBoundCalcEdit", false, control2.OM_IMSeaDepotFreeDaysBoundCalcEdit.ReadOnly);
						AssertEquals("OM_IMAirDepotFreeDaysBoundCalcEdit", false, control2.OM_IMAirDepotFreeDaysBoundCalcEdit.ReadOnly);
						AssertEquals("OM_IMCopySeaBillsBoundCalcEdit", false, control2.OM_IMCopySeaBillsBoundCalcEdit.ReadOnly);
						AssertEquals("OM_IMImporterCategoryBoundDropEdit", false, control2.OM_IMImporterCategoryBoundDropEdit.ReadOnly);
						AssertEquals("OM_IMJobRequireOrderTrackLinkBoundCheckEdit", false, control2.OM_IMJobRequireOrderTrackLinkBoundCheckEdit.ReadOnly);
						AssertEquals("OM_IMMergeCustomsInvoiceLinesByBoundDropEdit", false, control2.OM_IMMergeCustomsInvoiceLinesByBoundDropEdit.ReadOnly);
						AssertEquals("OM_IMOriginalSeaBillsBoundCalcEdit", false, control2.OM_IMOriginalSeaBillsBoundCalcEdit.ReadOnly);
						AssertEquals("OM_IMPaymentMethodDropEdit", false, control2.OM_IMPaymentMethodDropEdit.ReadOnly);
						AssertEquals("OwnersRefInitialNumberButton", false, control2.OwnersRefInitialNumberButton.ReadOnly);
						AssertEquals("AutoImporterJobRefCheckBox", false, control2.AutoImporterJobRefCheckBox.ReadOnly);
						AssertEquals("ProductImportAuditDropEdit", false, control2.ProductImportAuditDropEdit.ReadOnly);
						AssertEquals("OM_IMAutoPopulateOwnerRefWithOrderNumsDropEdit", false, control2.OM_IMAutoPopulateOwnerRefWithOrderNumsDropEdit.ReadOnly);
						AssertEquals("OM_IMEnablePromptToCreateProductsDropEdit", false, control2.OM_IMEnablePromptToCreateProductsDropEdit.ReadOnly);
						AssertEquals("OM_IMDocumentAddressPreferenceDropEdit", false, control2.OM_IMDocumentAddressPreferenceDropEdit.ReadOnly);
						AssertEquals("OM_IMDefaultToNewOrdersToNextOrderNumCheckBox", false, control2.OM_IMDefaultToNewOrdersToNextOrderNumCheckBox.ReadOnly);
						AssertEquals("OM_IMAllowAttachedOrderXMLUpdateCheckBox", false, control2.OM_IMAllowAttachedOrderXMLUpdateCheckBox.ReadOnly);
						AssertEquals("OM_IMLastOrderReferenceTextBox", false, control2.OM_IMLastOrderReferenceTextBox.ReadOnly);
						AssertEquals("RelatedPartiesGrid", false, control2.RelatedPartiesGrid.ReadOnly);
						AssertEquals("ContainerDetentionGrid", false, control2.ContainerDetentionGrid.ReadOnly);
						AssertEquals("OM_IMShowDutyOnWarehouseEntriesCheckBox", false, control2.OM_IMShowDutyOnWarehouseEntriesCheckBox.ReadOnly);
						AssertEquals("OM_IMEftQuarantineFromImportCheckBox", false, control2.OM_IMEftQuarantineFromImportCheckBox.ReadOnly);
						AssertEquals("OM_IMEFTBankAccountBoundTextBox", false, control2.OM_IMEFTBankAccountBoundTextBox.ReadOnly);
						AssertEquals("OM_IMEFTBankBSBBoundTextBox", false, control2.OM_IMEFTBankBSBBoundTextBox.ReadOnly);
						AssertEquals("OM_IMMinEFTAmountBoundCalcEdit", false, control2.OM_IMMinEFTAmountBoundCalcEdit.ReadOnly);
						AssertEquals("OM_IMMaxEFTAmountBoundCalcEdit", false, control2.OM_IMMaxEFTAmountBoundCalcEdit.ReadOnly);
						AssertEquals("OM_IMEftCustomsFromImportBoundCheckEdit", false, control2.OM_IMEftCustomsFromImportBoundCheckEdit.ReadOnly);
						AssertEquals("OM_IMEftHoldUntilPayAuthorisedBoundCheckEdit", false, control2.OM_IMEftHoldUntilPayAuthorisedBoundCheckEdit.ReadOnly);
						AssertEquals("OM_IMIsGSTDeferredBoundCheckBox", false, control2.OM_IMIsGSTDeferredBoundCheckBox.ReadOnly);
						AssertEquals("exportBillAgentChargesDirectCheckBox", false, control2.exportBillAgentChargesDirectCheckBox.ReadOnly);
						AssertEquals("OM_ConsigneeAuthorityToLeaveDropEdit", false, control2.OM_ConsigneeAuthorityToLeaveDropEdit.ReadOnly);
						AssertEquals("OM_IMOwnsProductsCheckBox", false, control2.OM_IMOwnsProductsCheckBox.ReadOnly);
						AssertEquals("OM_IMDisallowOrdersCheckBox", false, control2.OM_IMDisallowOrdersCheckBox.ReadOnly);
						AssertEquals("OM_IMBalanceInvoicePackageCheckBox", false, control2.OM_IMBalanceInvoicePackageCheckBox.ReadOnly);
						AssertEquals("ImporterOverrideCheckBox", false, control2.ImporterOverrideCheckBox.ReadOnly);
						AssertEquals("OM_IMSendImportDocsDropEdit", false, control2.OM_IMSendImportDocsDropEdit.ReadOnly);
						AssertEquals("OM_IMSendSeaImportDocsDropEdit", false, control2.OM_IMSendSeaImportDocsDropEdit.ReadOnly);
						AssertEquals("IsDutyDeferredCheckBox", false, control2.IsDutyDeferredCheckBox.ReadOnly);
						AssertEquals("OM_IMAdvanceCargoReportingSelfFilerCheckBox", false, control2.OM_IMAdvanceCargoReportingSelfFilerCheckBox.ReadOnly);
					});
				}
			}
			finally
			{
				Env.Security.OrgConsigneeModifyDetails.IsAllowed = oldValue;
			}
		}

		public void TestControlVisibility()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "XYZ";

			AssertDetailsControl(Constants.CountryCodes.NewZealand, org, control =>
			{
				Assert("OM_IMPaymentMethodDropEdit is visible in NewZealand", control.IsOM_IMPaymentMethodDropEditVisible);
				Assert("ProductImportAuditDropEdit is visible in NewZealand", control.IsProductImportAuditDropEditVisible);
				Assert("OB_CusPaidByDropEdit is not visible in NewZealand", !control.IsOB_CusPaidByDropEdit);
				Assert("AUPaymentDetailsTabPage is not visible in NewZealand", !control.IsAUPaymentDetailsTabPageVisible);
				Assert("ACRConsigneeDefaultDropEdit is not visible in NewZealand", !control.IsACRConsigneeDefaultDropEditVisible);
				Assert("SCRConsigneeDefaultDropEdit is not visible in NewZealand", !control.IsSCRConsigneeDefaultDropEditVisible);
			});
			AssertDetailsControl(Constants.CountryCodes.SouthAfrica, org, control =>
			{
				Assert("OM_IMPaymentMethodDropEdit is visible in SouthAfrica", control.IsOM_IMPaymentMethodDropEditVisible);
				Assert("ProductImportAuditDropEdit is visible in SouthAfrica", control.IsProductImportAuditDropEditVisible);
				Assert("OB_CusPaidByDropEdit is not visible in SouthAfrica", !control.IsOB_CusPaidByDropEdit);
				Assert("AUPaymentDetailsTabPage is not visible in SouthAfrica", !control.IsAUPaymentDetailsTabPageVisible);
				Assert("ACRConsigneeDefaultDropEdit is not visible in SouthAfrica", !control.IsACRConsigneeDefaultDropEditVisible);
				Assert("SCRConsigneeDefaultDropEdit is not visible in SouthAfrica", !control.IsSCRConsigneeDefaultDropEditVisible);
			});
			AssertDetailsControl(Constants.CountryCodes.Singapore, org, control =>
			{
				Assert("OM_IMPaymentMethodDropEdit is visible in Singapore", control.IsOM_IMPaymentMethodDropEditVisible);
				Assert("ProductImportAuditDropEdit is visible in Singapore", control.IsProductImportAuditDropEditVisible);
				Assert("OB_CusPaidByDropEdit is not visible in Singapore", !control.IsOB_CusPaidByDropEdit);
				Assert("AUPaymentDetailsTabPage is not visible in Singapore", !control.IsAUPaymentDetailsTabPageVisible);
				Assert("ACRConsigneeDefaultDropEdit is not visible in Singapore", !control.IsACRConsigneeDefaultDropEditVisible);
				Assert("SCRConsigneeDefaultDropEdit is not visible in Singapore", !control.IsSCRConsigneeDefaultDropEditVisible);
			});
			AssertDetailsControl(Constants.CountryCodes.Canada, org, control =>
			{
				Assert("OM_IMPaymentMethodDropEdit is not visible in Canada", !control.IsOM_IMPaymentMethodDropEditVisible);
				Assert("ProductImportAuditDropEdit is not visible in Canada", !control.IsProductImportAuditDropEditVisible);
				Assert("OB_CusPaidByDropEdit is not visible in Canada", !control.IsOB_CusPaidByDropEdit);
				Assert("AUPaymentDetailsTabPage is not visible in Canada", !control.IsAUPaymentDetailsTabPageVisible);
				Assert("ACRConsigneeDefaultDropEdit is not visible in Canada", !control.IsACRConsigneeDefaultDropEditVisible);
				Assert("SCRConsigneeDefaultDropEdit is not visible in Canada", !control.IsSCRConsigneeDefaultDropEditVisible);
			});
			AssertDetailsControl(Constants.CountryCodes.Taiwan, org, control =>
			{
				Assert("OM_IMPaymentMethodDropEdit is not visible in Taiwan", !control.IsOM_IMPaymentMethodDropEditVisible);
				Assert("ProductImportAuditDropEdit is visible in Taiwan", control.IsProductImportAuditDropEditVisible);
				Assert("OB_CusPaidByDropEdit is visible in Taiwan", control.IsOB_CusPaidByDropEdit);
				Assert("AUPaymentDetailsTabPage is not visible in Taiwan", !control.IsAUPaymentDetailsTabPageVisible);
				Assert("ACRConsigneeDefaultDropEdit is not visible in Taiwan", !control.IsACRConsigneeDefaultDropEditVisible);
				Assert("SCRConsigneeDefaultDropEdit is not visible in Taiwan", !control.IsSCRConsigneeDefaultDropEditVisible);
			});
			AssertDetailsControl(Constants.CountryCodes.Australia, org, control =>
			{
				Assert("OM_IMPaymentMethodDropEdit is not visible in Australia", !control.IsOM_IMPaymentMethodDropEditVisible);
				Assert("ProductImportAuditDropEdit is visible in Australia", control.IsProductImportAuditDropEditVisible);
				Assert("OB_CusPaidByDropEdit is not visible in Australia", !control.IsOB_CusPaidByDropEdit);
				Assert("AUPaymentDetailsTabPage is visible in Australia", control.IsAUPaymentDetailsTabPageVisible);
				Assert("ACRConsigneeDefaultDropEdit is visible in Australia", control.IsACRConsigneeDefaultDropEditVisible);
				Assert("SCRConsigneeDefaultDropEdit is visible in Australia", control.IsSCRConsigneeDefaultDropEditVisible);
			});
			AssertDetailsControl(Constants.CountryCodes.Afghanistan, org, control =>
			{
				Assert("OM_IMPaymentMethodDropEdit is not visible by default", !control.IsOM_IMPaymentMethodDropEditVisible);
				Assert("ProductImportAuditDropEdit is visible by default", control.IsProductImportAuditDropEditVisible);
				Assert("OB_CusPaidByDropEdit is not visible by default", !control.IsOB_CusPaidByDropEdit);
				Assert("AUPaymentDetailsTabPage is not visible by default", !control.IsAUPaymentDetailsTabPageVisible);
				Assert("ACRConsigneeDefaultDropEdit is not visible by default", !control.IsACRConsigneeDefaultDropEditVisible);
				Assert("SCRConsigneeDefaultDropEdit is not visible by default", !control.IsSCRConsigneeDefaultDropEditVisible);
			});
		}

		public void TestRelevantCountryTabIsShown()
		{
			AssertDetailsControl(Constants.CountryCodes.Australia, control =>
			{
				Assert("AU consignee EFT tab is visible", control.PaymentDetailsTabControl.Visible);
				AssertNull(control.PaymentDetailsTabControl.PlugIns.GetPlugIn(ControllerIDs.Customs.CA.OrganisationConsigneePlugIn));
				AssertNull(control.CustomsDefaultsTabControl.PlugIns.GetPlugIn(ControllerIDs.Customs.GB.OrganisationConsigneePlugIn));
				AssertNull(control.CustomsDefaultsTabControl.PlugIns.GetPlugIn(ControllerIDs.Customs.DE.OrganisationConsigneePlugIn));
				AssertNull(control.CustomsDefaultsTabControl.PlugIns.GetPlugIn(ControllerIDs.Customs.FR.OrganisationConsigneePlugIn));
				AssertNull(control.CustomsDefaultsTabControl.PlugIns.GetPlugIn(ControllerIDs.Customs.NL.OrganisationConsigneePlugIn));
				AssertNull(control.CustomsDefaultsTabControl.PlugIns.GetPlugIn(ControllerIDs.Customs.BR.OrganisationConsigneePlugIn));
				AssertNull(control.CustomsDefaultsTabControl.PlugIns.GetPlugIn(ControllerIDs.Customs.IN.OrganisationConsigneePlugIn));
			});

			AssertDetailsControl(Constants.CountryCodes.Canada, control =>
			{
				Assert("PaymentDetailsTabControl is visible", control.PaymentDetailsTabControl.Visible);
				AssertNotNull("CA consignee plugIn added", control.PaymentDetailsTabControl.PlugIns.GetPlugIn(ControllerIDs.Customs.CA.OrganisationConsigneePlugIn));
				AssertNull(control.CustomsDefaultsTabControl.PlugIns.GetPlugIn(ControllerIDs.Customs.GB.OrganisationConsigneePlugIn));
				AssertNull(control.CustomsDefaultsTabControl.PlugIns.GetPlugIn(ControllerIDs.Customs.DE.OrganisationConsigneePlugIn));
				AssertNull(control.CustomsDefaultsTabControl.PlugIns.GetPlugIn(ControllerIDs.Customs.FR.OrganisationConsigneePlugIn));
				AssertNull(control.CustomsDefaultsTabControl.PlugIns.GetPlugIn(ControllerIDs.Customs.NL.OrganisationConsigneePlugIn));
				AssertNull(control.CustomsDefaultsTabControl.PlugIns.GetPlugIn(ControllerIDs.Customs.BR.OrganisationConsigneePlugIn));
			});

			AssertDetailsControl(Constants.CountryCodes.UnitedKingdom, control =>
			{
				AssertEquals("Should only be 2 Tab Pages visible", 2, control.CustomsDefaultsTabControl.TabPages.Count);
				AssertNull(control.PaymentDetailsTabControl.PlugIns.GetPlugIn(ControllerIDs.Customs.CA.OrganisationConsigneePlugIn));
				AssertNotNull("UK consignee plugIn added", control.CustomsDefaultsTabControl.PlugIns.GetPlugIn(ControllerIDs.Customs.GB.OrganisationConsigneePlugIn));
				AssertNull(control.CustomsDefaultsTabControl.PlugIns.GetPlugIn(ControllerIDs.Customs.DE.OrganisationConsigneePlugIn));
				AssertNull(control.CustomsDefaultsTabControl.PlugIns.GetPlugIn(ControllerIDs.Customs.FR.OrganisationConsigneePlugIn));
				AssertNull(control.CustomsDefaultsTabControl.PlugIns.GetPlugIn(ControllerIDs.Customs.NL.OrganisationConsigneePlugIn));
				AssertNull(control.CustomsDefaultsTabControl.PlugIns.GetPlugIn(ControllerIDs.Customs.BR.OrganisationConsigneePlugIn));
				AssertNull(control.CustomsDefaultsTabControl.PlugIns.GetPlugIn(ControllerIDs.Customs.IN.OrganisationConsigneePlugIn));
			});

			AssertDetailsControl(Constants.CountryCodes.Germany, control =>
			{
				AssertEquals("Should only be 2 Tab Pages visible", 2, control.CustomsDefaultsTabControl.TabPages.Count);
				AssertNull(control.PaymentDetailsTabControl.PlugIns.GetPlugIn(ControllerIDs.Customs.CA.OrganisationConsigneePlugIn));
				AssertNull(control.CustomsDefaultsTabControl.PlugIns.GetPlugIn(ControllerIDs.Customs.GB.OrganisationConsigneePlugIn));
				AssertNotNull("DE consignee plugIn added", control.CustomsDefaultsTabControl.PlugIns.GetPlugIn(ControllerIDs.Customs.DE.OrganisationConsigneePlugIn));
				AssertNull(control.CustomsDefaultsTabControl.PlugIns.GetPlugIn(ControllerIDs.Customs.FR.OrganisationConsigneePlugIn));
				AssertNull(control.CustomsDefaultsTabControl.PlugIns.GetPlugIn(ControllerIDs.Customs.NL.OrganisationConsigneePlugIn));
				AssertNull(control.CustomsDefaultsTabControl.PlugIns.GetPlugIn(ControllerIDs.Customs.BR.OrganisationConsigneePlugIn));
				AssertNull(control.CustomsDefaultsTabControl.PlugIns.GetPlugIn(ControllerIDs.Customs.IN.OrganisationConsigneePlugIn));
			});

			foreach (var country in Constants.CountryCodes.FranceAndOverseasDepartmentsUnderItsCustomsJurisdiction)
			{
				AssertDetailsControl(country, control =>
				{
					AssertEquals("Should only be 2 Tab Pages visible", 2, control.CustomsDefaultsTabControl.TabPages.Count);
					AssertNull(control.PaymentDetailsTabControl.PlugIns.GetPlugIn(ControllerIDs.Customs.CA.OrganisationConsigneePlugIn));
					AssertNull(control.CustomsDefaultsTabControl.PlugIns.GetPlugIn(ControllerIDs.Customs.GB.OrganisationConsigneePlugIn));
					AssertNull(control.CustomsDefaultsTabControl.PlugIns.GetPlugIn(ControllerIDs.Customs.DE.OrganisationConsigneePlugIn));
					AssertNotNull("FR consignee plugIn added", control.CustomsDefaultsTabControl.PlugIns.GetPlugIn(ControllerIDs.Customs.FR.OrganisationConsigneePlugIn));
					AssertNull(control.CustomsDefaultsTabControl.PlugIns.GetPlugIn(ControllerIDs.Customs.NL.OrganisationConsigneePlugIn));
					AssertNull(control.CustomsDefaultsTabControl.PlugIns.GetPlugIn(ControllerIDs.Customs.BR.OrganisationConsigneePlugIn));
					AssertNull(control.CustomsDefaultsTabControl.PlugIns.GetPlugIn(ControllerIDs.Customs.IN.OrganisationConsigneePlugIn));
				});
			}

			AssertDetailsControl(Constants.CountryCodes.Netherlands, control =>
			{
				AssertEquals("Should only be 2 Tab Pages visible", 2, control.CustomsDefaultsTabControl.TabPages.Count);
				AssertNull(control.PaymentDetailsTabControl.PlugIns.GetPlugIn(ControllerIDs.Customs.CA.OrganisationConsigneePlugIn));
				AssertNull(control.CustomsDefaultsTabControl.PlugIns.GetPlugIn(ControllerIDs.Customs.GB.OrganisationConsigneePlugIn));
				AssertNull(control.CustomsDefaultsTabControl.PlugIns.GetPlugIn(ControllerIDs.Customs.DE.OrganisationConsigneePlugIn));
				AssertNull(control.CustomsDefaultsTabControl.PlugIns.GetPlugIn(ControllerIDs.Customs.FR.OrganisationConsigneePlugIn));
				AssertNull(control.CustomsDefaultsTabControl.PlugIns.GetPlugIn(ControllerIDs.Customs.BR.OrganisationConsigneePlugIn));
				AssertNull(control.CustomsDefaultsTabControl.PlugIns.GetPlugIn(ControllerIDs.Customs.IN.OrganisationConsigneePlugIn));
				AssertNotNull("NL consignee plugIn added", control.CustomsDefaultsTabControl.PlugIns.GetPlugIn(ControllerIDs.Customs.NL.OrganisationConsigneePlugIn));
			});

			AssertDetailsControl(Constants.CountryCodes.Brazil, control =>
			{
				AssertEquals("Should only be 1 Tab Pages visible", 1, control.CustomsDefaultsTabControl.TabPages.Count);
				AssertNull(control.PaymentDetailsTabControl.PlugIns.GetPlugIn(ControllerIDs.Customs.CA.OrganisationConsigneePlugIn));
				AssertNull(control.CustomsDefaultsTabControl.PlugIns.GetPlugIn(ControllerIDs.Customs.GB.OrganisationConsigneePlugIn));
				AssertNull(control.CustomsDefaultsTabControl.PlugIns.GetPlugIn(ControllerIDs.Customs.DE.OrganisationConsigneePlugIn));
				AssertNull(control.CustomsDefaultsTabControl.PlugIns.GetPlugIn(ControllerIDs.Customs.FR.OrganisationConsigneePlugIn));
				AssertNull(control.CustomsDefaultsTabControl.PlugIns.GetPlugIn(ControllerIDs.Customs.IN.OrganisationConsigneePlugIn));
				AssertNotNull("BR consignee plugIn added", control.CustomsDefaultsTabControl.PlugIns.GetPlugIn(ControllerIDs.Customs.BR.OrganisationConsigneePlugIn));
			});

			AssertDetailsControl(Constants.CountryCodes.India, control =>
			{
				AssertEquals("Should only be 1 Tab Pages visible", 1, control.CustomsDefaultsTabControl.TabPages.Count);
				AssertNull(control.PaymentDetailsTabControl.PlugIns.GetPlugIn(ControllerIDs.Customs.CA.OrganisationConsigneePlugIn));
				AssertNull(control.CustomsDefaultsTabControl.PlugIns.GetPlugIn(ControllerIDs.Customs.GB.OrganisationConsigneePlugIn));
				AssertNull(control.CustomsDefaultsTabControl.PlugIns.GetPlugIn(ControllerIDs.Customs.DE.OrganisationConsigneePlugIn));
				AssertNull(control.CustomsDefaultsTabControl.PlugIns.GetPlugIn(ControllerIDs.Customs.FR.OrganisationConsigneePlugIn));
				AssertNull(control.CustomsDefaultsTabControl.PlugIns.GetPlugIn(ControllerIDs.Customs.BR.OrganisationConsigneePlugIn));
				AssertNotNull("IN consignee plugIn added", control.CustomsDefaultsTabControl.PlugIns.GetPlugIn(ControllerIDs.Customs.IN.OrganisationConsigneePlugIn));
			});
		}

		public void TestRelevantCountryTabIsShown_IfGbHasLeftEu()
		{
			RemoveGbFromEu();

			AssertDetailsControl(Constants.CountryCodes.UnitedKingdom, control =>
			{
				AssertEquals("Should only be 2 Tab Pages visible", 2, control.CustomsDefaultsTabControl.TabPages.Count);
				AssertNotNull("UK consignee plugIn added", control.CustomsDefaultsTabControl.PlugIns.GetPlugIn(ControllerIDs.Customs.GB.OrganisationConsigneePlugIn));
				AssertNull(control.CustomsDefaultsTabControl.PlugIns.GetPlugIn(ControllerIDs.Customs.DE.OrganisationConsigneePlugIn));
				AssertNull(control.CustomsDefaultsTabControl.PlugIns.GetPlugIn(ControllerIDs.Customs.FR.OrganisationConsigneePlugIn));
				AssertNull(control.CustomsDefaultsTabControl.PlugIns.GetPlugIn(ControllerIDs.Customs.NL.OrganisationConsigneePlugIn));
			});

			AssertDetailsControl(Constants.CountryCodes.Germany, control =>
			{
				AssertEquals("Should only be 2 Tab Pages visible", 2, control.CustomsDefaultsTabControl.TabPages.Count);
				AssertNull(control.CustomsDefaultsTabControl.PlugIns.GetPlugIn(ControllerIDs.Customs.GB.OrganisationConsigneePlugIn));
				AssertNotNull("DE consignee plugIn added", control.CustomsDefaultsTabControl.PlugIns.GetPlugIn(ControllerIDs.Customs.DE.OrganisationConsigneePlugIn));
				AssertNull(control.CustomsDefaultsTabControl.PlugIns.GetPlugIn(ControllerIDs.Customs.FR.OrganisationConsigneePlugIn));
				AssertNull(control.CustomsDefaultsTabControl.PlugIns.GetPlugIn(ControllerIDs.Customs.NL.OrganisationConsigneePlugIn));
			});

			AssertDetailsControl(Constants.CountryCodes.France, control =>
			{
				AssertEquals("Should only be 2 Tab Pages visible", 2, control.CustomsDefaultsTabControl.TabPages.Count);
				AssertNull(control.CustomsDefaultsTabControl.PlugIns.GetPlugIn(ControllerIDs.Customs.GB.OrganisationConsigneePlugIn));
				AssertNull(control.CustomsDefaultsTabControl.PlugIns.GetPlugIn(ControllerIDs.Customs.DE.OrganisationConsigneePlugIn));
				AssertNotNull("FR consignee plugIn added", control.CustomsDefaultsTabControl.PlugIns.GetPlugIn(ControllerIDs.Customs.FR.OrganisationConsigneePlugIn));
				AssertNull(control.CustomsDefaultsTabControl.PlugIns.GetPlugIn(ControllerIDs.Customs.NL.OrganisationConsigneePlugIn));
			});

			AssertDetailsControl(Constants.CountryCodes.Netherlands, control =>
			{
				AssertEquals("Should only be 2 Tab Pages visible", 2, control.CustomsDefaultsTabControl.TabPages.Count);
				AssertNull(control.CustomsDefaultsTabControl.PlugIns.GetPlugIn(ControllerIDs.Customs.GB.OrganisationConsigneePlugIn));
				AssertNull(control.CustomsDefaultsTabControl.PlugIns.GetPlugIn(ControllerIDs.Customs.DE.OrganisationConsigneePlugIn));
				AssertNull(control.CustomsDefaultsTabControl.PlugIns.GetPlugIn(ControllerIDs.Customs.FR.OrganisationConsigneePlugIn));
				AssertNotNull("NL consignee plugIn added", control.CustomsDefaultsTabControl.PlugIns.GetPlugIn(ControllerIDs.Customs.NL.OrganisationConsigneePlugIn));
			});

			void RemoveGbFromEu()
			{
				var sql = $@"  
  --RefDataGrouping //Adding EUN and GB
  declare @RefDataGroupingParentPK uniqueidentifier = NEWID();
  INSERT RefDatabase_RefDataGrouping (ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description, ZZZ_ZZZ_Grouping) VALUES (@RefDataGroupingParentPK, 'EUN', 'Europe', NULL)
  INSERT RefDatabase_RefDataGrouping (ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description, ZZZ_ZZZ_Grouping) VALUES (NEWID(), 'GB', 'United Kingdom', @RefDataGroupingParentPK)

  --RefCusTradeGroup //Adding EUC
  declare @RefCusTradeGroupParentPK uniqueidentifier = NEWID();
  INSERT RefDatabase_RefCusTradeGroup (ZZA_PK, ZZA_TradeGroup, ZZA_Description, ZZA_StartDate, ZZA_EndDate, ZZA_ZZZ_NKDataGrouping) VALUES (@RefCusTradeGroupParentPK, 'EUC', 'European Trade Group', '1900-01-01 12:00:00', '2079-06-06 23:59:00', 'EUN')

  --RefCusTradeGroupCountry //Adding GB and DE and FR and NL
  INSERT RefDatabase_RefCusTradeGroupCountry (ZZB_PK, ZZB_ZZA_TradeGroup, ZZB_RN_NKTradeGroupCountryCode, ZZB_StartDate, ZZB_EndDate) VALUES (NEWID(), @RefCusTradeGroupParentPK, 'GB', '1900-01-01 12:00:00', '2019-05-13 00:00:00')
  INSERT RefDatabase_RefCusTradeGroupCountry (ZZB_PK, ZZB_ZZA_TradeGroup, ZZB_RN_NKTradeGroupCountryCode, ZZB_StartDate, ZZB_EndDate) VALUES (NEWID(), @RefCusTradeGroupParentPK, 'DE', '1900-01-01 12:00:00', '2079-06-06 23:59:00')
  INSERT RefDatabase_RefCusTradeGroupCountry (ZZB_PK, ZZB_ZZA_TradeGroup, ZZB_RN_NKTradeGroupCountryCode, ZZB_StartDate, ZZB_EndDate) VALUES (NEWID(), @RefCusTradeGroupParentPK, 'FR', '1900-01-01 12:00:00', '2079-06-06 23:59:00')
  INSERT RefDatabase_RefCusTradeGroupCountry (ZZB_PK, ZZB_ZZA_TradeGroup, ZZB_RN_NKTradeGroupCountryCode, ZZB_StartDate, ZZB_EndDate) VALUES (NEWID(), @RefCusTradeGroupParentPK, 'NL', '1900-01-01 12:00:00', '2079-06-06 23:59:00')";

				((IDbConnected)Factory).Connection.ExecuteNonQuery(sql);
				Factory.Save();
			}
		}

		public void TestASNDefaultFieldTypeGridReadOnly()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "XYZ";

			AssertDetailsControl("", org, control =>
			{
				AssertEquals(true, control.ASNDefaultFieldTypeGrid.ReadOnly);

				org.CompanyData.ImporterOverride = true;
				AssertEquals(false, control.ASNDefaultFieldTypeGrid.ReadOnly);
			});
		}

		[RequiresSTA]
		public void TestNoPlugIsAddedWhenDisposing()
		{
			ErrorReporter.Clear();

			var org = Factory.New<OrgHeader>();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.UnitedKingdom))
			{
				using (var form = new ZForm(org))
				{
					var control = new DetailsControlForTest();
					form.Controls.Add(control);
					control.SetDataBinding(org, "");
					form.Show();
				}
			}

			AssertNull(ErrorReporter.LastExceptionReported);
		}

		public void TestOwnersRefInitialNumberButton_Click()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "XYZ";

			var fountain = Env.NumberFountains.ImporterJobReference(org.PK.ToGuid());
			AssertEquals("New Org, Importer job reference number fountain should start at 1", 1L, fountain.PeekPreliminary(Factory));

			AssertDetailsControl("", org, control =>
			{
				control.OwnersRefInitialNumberButton_Click(null, EventArgs.Empty);
				AssertEquals(typeof(NewNumberFountainDialog), ZFormModaliser.ActiveForm.GetType());
				SetFountainInTransaction(fountain, 5);
				ZFormModaliser.ActiveForm.Close();

				ZQuery query = new ZQuery(StmALogSchema.SL_Parent, org.PK);
				query.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.EditedARecordCode);
				query.AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.StartsWith, "Importer Job Reference Next No:");
				query.ReLoadExistingRows = true;
				StmALog[] logs = Factory.Load<StmALog>(query);
				Assert("No job reference change logs initially", logs == null || logs.Length == 0);

				control.OwnersRefInitialNumberButton_Click(null, EventArgs.Empty);
				AssertEquals(typeof(NewNumberFountainDialog), ZFormModaliser.ActiveForm.GetType());
				SetFountainInTransaction(fountain, 10);
				ZFormModaliser.ActiveForm.DialogResult = DialogResult.OK;
				ZFormModaliser.ActiveForm.Close();

				logs = Factory.Load<StmALog>(query);
				Assert("No job reference change logs as bizo is not in database", logs == null || logs.Length == 0);

				org.Factory.Save();

				control.OwnersRefInitialNumberButton_Click(null, EventArgs.Empty);
				AssertEquals(typeof(NewNumberFountainDialog), ZFormModaliser.ActiveForm.GetType());
				SetFountainInTransaction(fountain, 15);
				ZFormModaliser.ActiveForm.DialogResult = DialogResult.Cancel;
				ZFormModaliser.ActiveForm.Close();

				logs = Factory.Load<StmALog>(query);
				Assert("No job reference change logs as Cancel was clicked", logs == null || logs.Length == 0);

				control.OwnersRefInitialNumberButton_Click(null, EventArgs.Empty);
				AssertEquals(typeof(NewNumberFountainDialog), ZFormModaliser.ActiveForm.GetType());
				SetFountainInTransaction(fountain, 15);
				ZFormModaliser.ActiveForm.DialogResult = DialogResult.OK;
				ZFormModaliser.ActiveForm.Close();

				logs = Factory.Load<StmALog>(query);
				Assert("No job reference change logs as fountain was not changed", logs == null || logs.Length == 0);

				control.OwnersRefInitialNumberButton_Click(null, EventArgs.Empty);
				AssertEquals(typeof(NewNumberFountainDialog), ZFormModaliser.ActiveForm.GetType());
				SetFountainInTransaction(fountain, 20);
				ZFormModaliser.ActiveForm.DialogResult = DialogResult.OK;
				ZFormModaliser.ActiveForm.Close();

				logs = Factory.Load<StmALog>(query);
				Assert("Job reference change logshould be added", logs.Length == 1);
			});
		}

		void SetFountainInTransaction(INumberFountainProxy fountain, int next)
		{
			((IDbConnected)Factory).Connection.BeginTransaction();
			fountain.SetNext(Factory, next);
			((IDbConnected)Factory).Connection.CommitTransaction();
		}

		public void TestOwnersRegInitialNumberButtonSecurity()
		{
			bool oldCNEDetailsValue = Env.Security.OrgConsigneeModifyDetails.IsAllowed;

			try
			{
				Env.Security.OrgConsigneeModifyDetails.IsAllowed = false;
				OrgHeader org = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "DEMORG");

				using (ZForm form = new ZForm(org))
				{
					using (DetailsControlForTest control = new DetailsControlForTest())
					{
						form.Controls.Add(control);
						form.Width = control.Width + 10;
						form.Height = control.Height + 10;
						control.Dock = DockStyle.Fill;
						form.Show();
						AssertEquals("The button should be disabled", false, control.OwnersRefInitialNumberButtonEnabled);
					}
				}
			}
			finally
			{
				Env.Security.OrgConsigneeModifyDetails.IsAllowed = oldCNEDetailsValue;
			}
		}

		public void TestContainerFillingRatioTolerancesTabPage()
		{
			var org = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "DEMORG");

			AdvOrmFeatureHelper.RunTestWith(false, action: () =>
			{
				using (var form = new ZForm(org))
				using (var control = new DetailsControlForTest())
				{
					form.Controls.Add(control);
					form.Width = control.Width + 10;
					form.Height = control.Height + 10;
					control.Dock = DockStyle.Fill;
					form.Show();

					AssertNull(form.Controls.Find("ContainerFillingRatioTolerancesTabPage", true).OfType<ZTabPage>().FirstOrDefault());
				}
			});

			AdvOrmFeatureHelper.RunTestWith(true, action: () =>
			{
				using (var form = new ZForm(org))
				using (var control = new DetailsControlForTest())
				{
					form.Controls.Add(control);
					form.Width = control.Width + 10;
					form.Height = control.Height + 10;
					control.Dock = DockStyle.Fill;
					form.Show();

					AssertNotNull(form.Controls.Find("ContainerFillingRatioTolerancesTabPage", true).OfType<ZTabPage>().FirstOrDefault());
				}
			});
		}

		#region Implementation

		void AssertDetailsControl(string countryCode, AssertDetailsControlCoreDelegate assertDetailsControlCore)
		{
			AssertDetailsControl(countryCode, Factory.New<OrgHeader>(), assertDetailsControlCore);
		}

		void AssertDetailsControl(ZString countryCode, OrgHeader org, AssertDetailsControlCoreDelegate assertDetailsControlCore)
		{
			using (countryCode.IsEmpty ? null : GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
			{
				using (var form = new ZForm(org))
				{
					using (var control = new DetailsControlForTest())
					{
						form.Controls.Add(control);
						form.Show();
						control.SetDataBinding(org, "");
						assertDetailsControlCore(control);
					}
				}
			}
		}

		delegate void AssertDetailsControlCoreDelegate(DetailsControlForTest control);

		#region Setup

		protected override void SetUp()
		{
			base.SetUp();
			oldCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
		}

		protected override void TearDown()
		{
			GlbCompany.CurrentCompany.SetCountry(oldCountry);
			base.TearDown();
		}

		string oldCountry;

		#endregion

		#region DetailsControlForTest

		public class DetailsControlForTest : ConsigneeDetailsUserControl
		{
			public new ZGrid ASNDefaultFieldTypeGrid => base.ASNDefaultFieldTypeGrid;

			public bool IsACRConsigneeDefaultDropEditVisible
			{
				get { return ACRConsigneeDefaultDropEdit.Visible; }
			}

			public bool IsSCRConsigneeDefaultDropEditVisible
			{
				get { return SCRConsigneeDefaultDropEdit.Visible; }
			}

			public bool IsOM_IMPaymentMethodDropEditVisible
			{
				get { return OM_IMPaymentMethodDropEdit.Visible; }
			}

			public bool IsProductImportAuditDropEditVisible
			{
				get { return ProductImportAuditDropEdit.Visible; }
			}

			public bool IsOB_CusPaidByDropEdit
			{
				get { return OB_CusPaidByDropEdit.Visible; }
			}

			public bool IsAUPaymentDetailsTabPageVisible => AUPaymentDetailsTabPage.TabVisible;

			public new void OwnersRefInitialNumberButton_Click(object sender, EventArgs e)
			{
				base.OwnersRefInitialNumberButton_Click(sender, e);
			}

			public bool OwnersRefInitialNumberButtonEnabled
			{
				get { return base.OwnersRefInitialNumberButton.Enabled; }
			}

			public new ZTabControl PaymentDetailsTabControl
			{
				get { return base.PaymentDetailsTabControl; }
			}

			public new ZTabControl CustomsDefaultsTabControl
			{
				get { return base.CustomsDefaultsTabControl; }
			}

			internal new ZDropEdit ACRConsigneeDefaultDropEdit => base.ACRConsigneeDefaultDropEdit;
			internal new ZDropEdit SCRConsigneeDefaultDropEdit => base.SCRConsigneeDefaultDropEdit;
			internal new ZDropEdit OM_IMDefaultINCOTermDropEdit => base.OM_IMDefaultINCOTermDropEdit;
			internal new ZCheckBox OM_IMImporterRequiresOrderNumbersOnDocsCheckBox => base.OM_IMImporterRequiresOrderNumbersOnDocsCheckBox;
			internal new ZCodeFindBox OM_RS_NKIMDefaultServiceLevelBoundCodeFindBox => base.OM_RS_NKIMDefaultServiceLevelBoundCodeFindBox;
			internal new ZCalcEdit OM_IMSeaDepotFreeDaysBoundCalcEdit => base.OM_IMSeaDepotFreeDaysBoundCalcEdit;
			internal new ZCalcEdit OM_IMAirDepotFreeDaysBoundCalcEdit => base.OM_IMAirDepotFreeDaysBoundCalcEdit;
			internal new ZCalcEdit OM_IMCopySeaBillsBoundCalcEdit => base.OM_IMCopySeaBillsBoundCalcEdit;
			internal new ZDropEdit OM_IMImporterCategoryBoundDropEdit => base.OM_IMImporterCategoryBoundDropEdit;
			internal new ZCheckBox OM_IMJobRequireOrderTrackLinkBoundCheckEdit => base.OM_IMJobRequireOrderTrackLinkBoundCheckEdit;
			internal new ZDropEdit OM_IMMergeCustomsInvoiceLinesByBoundDropEdit => base.OM_IMMergeCustomsInvoiceLinesByBoundDropEdit;
			internal new ZCalcEdit OM_IMOriginalSeaBillsBoundCalcEdit => base.OM_IMOriginalSeaBillsBoundCalcEdit;
			internal new ZDropEdit OM_IMPaymentMethodDropEdit => base.OM_IMPaymentMethodDropEdit;
			internal new ZButton OwnersRefInitialNumberButton => base.OwnersRefInitialNumberButton;
			internal new ZCheckBox AutoImporterJobRefCheckBox => base.AutoImporterJobRefCheckBox;
			internal new ZDropEdit ProductImportAuditDropEdit => base.ProductImportAuditDropEdit;
			internal new ZDropEdit OM_IMAutoPopulateOwnerRefWithOrderNumsDropEdit => base.OM_IMAutoPopulateOwnerRefWithOrderNumsDropEdit;
			internal new ZDropEdit OM_IMEnablePromptToCreateProductsDropEdit => base.OM_IMEnablePromptToCreateProductsDropEdit;
			internal new ZDropEdit OM_IMDocumentAddressPreferenceDropEdit => base.OM_IMDocumentAddressPreferenceDropEdit;
			internal new ZCheckBox OM_IMDefaultToNewOrdersToNextOrderNumCheckBox => base.OM_IMDefaultToNewOrdersToNextOrderNumCheckBox;
			internal new ZCheckBox OM_IMAllowAttachedOrderXMLUpdateCheckBox => base.OM_IMAllowAttachedOrderXMLUpdateCheckBox;
			internal new ZTextBox OM_IMLastOrderReferenceTextBox => base.OM_IMLastOrderReferenceTextBox;
			internal new ZGrid RelatedPartiesGrid => base.RelatedPartiesGrid;
			internal new ZGrid ContainerDetentionGrid => base.ContainerDetentionGrid;
			internal new ZCheckBox OM_IMShowDutyOnWarehouseEntriesCheckBox => base.OM_IMShowDutyOnWarehouseEntriesCheckBox;
			internal new ZCheckBox OM_IMEftQuarantineFromImportCheckBox => base.OM_IMEftQuarantineFromImportCheckBox;
			internal new ZTextBox OM_IMEFTBankAccountBoundTextBox => base.OM_IMEFTBankAccountBoundTextBox;
			internal new ZTextBox OM_IMEFTBankBSBBoundTextBox => base.OM_IMEFTBankBSBBoundTextBox;
			internal new ZCalcEdit OM_IMMinEFTAmountBoundCalcEdit => base.OM_IMMinEFTAmountBoundCalcEdit;
			internal new ZCalcEdit OM_IMMaxEFTAmountBoundCalcEdit => base.OM_IMMaxEFTAmountBoundCalcEdit;
			internal new ZCheckBox OM_IMEftCustomsFromImportBoundCheckEdit => base.OM_IMEftCustomsFromImportBoundCheckEdit;
			internal new ZCheckBox OM_IMEftHoldUntilPayAuthorisedBoundCheckEdit => base.OM_IMEftHoldUntilPayAuthorisedBoundCheckEdit;
			internal new ZCheckBox OM_IMIsGSTDeferredBoundCheckBox => base.OM_IMIsGSTDeferredBoundCheckBox;
			internal new ZCheckBox exportBillAgentChargesDirectCheckBox => base.exportBillAgentChargesDirectCheckBox;
			internal new ZDropEdit OM_ConsigneeAuthorityToLeaveDropEdit => base.OM_ConsigneeAuthorityToLeaveDropEdit;
			internal new ZCheckBox OM_IMOwnsProductsCheckBox => base.OM_IMOwnsProductsCheckBox;
			internal new ZCheckBox OM_IMDisallowOrdersCheckBox => base.OM_IMDisallowOrdersCheckBox;
			internal new ZCheckBox OM_IMBalanceInvoicePackageCheckBox => base.OM_IMBalanceInvoicePackageCheckBox;
			internal new ZCheckBox ImporterOverrideCheckBox => base.ImporterOverrideCheckBox;
			internal new ZDropEdit OM_IMSendImportDocsDropEdit => base.OM_IMSendImportDocsDropEdit;
			internal new ZDropEdit OM_IMSendSeaImportDocsDropEdit => base.OM_IMSendSeaImportDocsDropEdit;
			internal new ZCheckBox IsDutyDeferredCheckBox => base.IsDutyDeferredCheckBox;
			internal new ZCheckBox OM_IMAdvanceCargoReportingSelfFilerCheckBox => base.OM_IMAdvanceCargoReportingSelfFilerCheckBox;
		}

		#endregion

		#endregion
	}
}
