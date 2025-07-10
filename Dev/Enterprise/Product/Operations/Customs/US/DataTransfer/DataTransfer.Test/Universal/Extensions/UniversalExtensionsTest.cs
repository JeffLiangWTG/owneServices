using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Business;

namespace Enterprise.Customs.US.DataTransfer.Universal.Extensions.Testing
{
	sealed class UniversalExtensionsTest : TestCaseWithFactory
	{
		public void TestGetInvoiceLineAddInfosApplicableForInwardWarehousing()
		{
			var list = Factory.GetInvoiceLineAddInfosApplicableForInwardWarehousing();
			AssertEquals("List should be cached to the factory", list, Factory.GetInvoiceLineAddInfosApplicableForInwardWarehousing());
			var checkedList = new List<string>(list);
			var missingFields = new List<string>();
			foreach (var expectedField in ExpectedList.Select(x => x.Substring(3)))
			{
				if (checkedList.Contains(expectedField))
				{
					checkedList.Remove(expectedField);
				}
				else
				{
					missingFields.Add(expectedField);
				}
			}
			var failedMessage = new ZStringBuilder();
			if (missingFields.Count > 0)
			{
				failedMessage.Append("The following expected fields are missing from the list; if they are no longer valid then please remove them from the expected list:");
				missingFields.ForEach(x => failedMessage.Append(x));
				failedMessage.AppendLine();
			}
			if (checkedList.Count > 0)
			{
				failedMessage.Append("The following new fields are not in the expected list; if they are valid then please add them to the expected list and also if they are numeric fields that need to be apportioned correctly then add them to Enterprise.Customs.US.Busines.InventorySelectionHeader.FieldsNeedToApplyRatio:");
				checkedList.ForEach(x => failedMessage.Append(x));
			}
			Assert(failedMessage.ToStringWithNewLineBetweenAppends(), failedMessage.IsEmpty);
		}

		internal static string[] ExpectedList
		{
			get
			{
				return new[]
				{
					JobComInvoiceLine.Schema.US_9802PerUnit,
					JobComInvoiceLine.Schema.US_98GoodsValue,
					JobComInvoiceLine.Schema.US_98InvCurrPerUnit,
					JobComInvoiceLine.Schema.US_98ValueInvCurr,
					JobComInvoiceLine.Schema.US_ADCVDStat,
					JobComInvoiceLine.Schema.US_ADDCaseNo,
					JobComInvoiceLine.Schema.US_ADDDecID,
					JobComInvoiceLine.Schema.US_ADDDepositRateIndicator,
					JobComInvoiceLine.Schema.US_ADDDepositValue,
					JobComInvoiceLine.Schema.US_ADDDepositRateOverride,
					JobComInvoiceLine.Schema.US_ADDQty,
					JobComInvoiceLine.Schema.US_ADD_NA,
					JobComInvoiceLine.Schema.US_ADDuty,
					JobComInvoiceLine.Schema.US_ADD_Cert,
					JobComInvoiceLine.Schema.US_AESOriginIndicator,
					JobComInvoiceLine.Schema.US_AMMVPerUnit,
					JobComInvoiceLine.Schema.US_AMMVPercentage,
					JobComInvoiceLine.Schema.US_AllocationQuantity,
					JobComInvoiceLine.Schema.US_AgricultureLicNo,
					JobComInvoiceLine.Schema.US_ArticleNoA,
					JobComInvoiceLine.Schema.US_ArticleNoB,
					JobComInvoiceLine.Schema.US_BOMLineExpanded,
					JobComInvoiceLine.Schema.US_BOMParentLine,
					JobComInvoiceLine.Schema.US_CAExportCertificate,
					JobComInvoiceLine.Schema.US_CBMADefaultTaxAmount,
					JobComInvoiceLine.Schema.US_CBMADefaultTaxRate,
					JobComInvoiceLine.Schema.US_CBTPACertificateNo,
					JobComInvoiceLine.Schema.US_CI_PreviousPivot,
					JobComInvoiceLine.Schema.US_CVDCaseNo,
					JobComInvoiceLine.Schema.US_CVDDepositRateIndicator,
					JobComInvoiceLine.Schema.US_CVDDepositValue,
					JobComInvoiceLine.Schema.US_CVDDepositRateOverride,
					JobComInvoiceLine.Schema.US_CVDQty,
					JobComInvoiceLine.Schema.US_CVD_NA,
					JobComInvoiceLine.Schema.US_CVDuty,
					JobComInvoiceLine.Schema.US_CVD_Cert,
					JobComInvoiceLine.Schema.US_ControlledGroupName,
					JobComInvoiceLine.Schema.US_CottonCertificateNo,
					JobComInvoiceLine.Schema.US_CottonFeeExempt,
					JobComInvoiceLine.Schema.US_CustomsValue,
					JobComInvoiceLine.Schema.US_DDTCArrivalDate,
					JobComInvoiceLine.Schema.US_DDTCExemptionCode,
					JobComInvoiceLine.Schema.US_DDTCInd,
					JobComInvoiceLine.Schema.US_DDTCLicenseNo,
					JobComInvoiceLine.Schema.US_DDTCLicenseType,
					JobComInvoiceLine.Schema.US_DDTCITARExemptionNo,
					JobComInvoiceLine.Schema.US_DDTCMilitaryEquipmentIndicator,
					JobComInvoiceLine.Schema.US_DDTCPartyCertificationIndicator,
					JobComInvoiceLine.Schema.US_DDTCQuantity,
					JobComInvoiceLine.Schema.US_DDTCRegistrationNo,
					JobComInvoiceLine.Schema.US_DDTCUSMLCategoryCode,
					JobComInvoiceLine.Schema.US_DDTCUnit,
					JobComInvoiceLine.Schema.US_DOTIndicator,
					JobComInvoiceLine.Schema.US_DateOfExport,
					JobComInvoiceLine.Schema.US_DateOfExportFromCountryOfOrigin,
					JobComInvoiceLine.Schema.US_DerivedCV,
					JobComInvoiceLine.Schema.US_DestinationState,
					JobComInvoiceLine.Schema.US_DisclaimSanctions,
					JobComInvoiceLine.Schema.US_Duty,
					JobComInvoiceLine.Schema.US_ECCN,
					JobComInvoiceLine.Schema.US_EPAConsentNumber,
					JobComInvoiceLine.Schema.US_ExportCode,
					JobComInvoiceLine.Schema.US_ExportTariff,
					JobComInvoiceLine.Schema.US_FCCIndicator,
					JobComInvoiceLine.Schema.US_FDAIndicator,
					JobComInvoiceLine.Schema.US_FSISInd,
					JobComInvoiceLine.Schema.US_F_PNDisclaimer,
					JobComInvoiceLine.Schema.US_FirstSale,
					JobComInvoiceLine.Schema.US_FlavorContentCreditInd,
					JobComInvoiceLine.Schema.US_FPI,
					JobComInvoiceLine.Schema.US_HasMPF,
					JobComInvoiceLine.Schema.US_HazMatClassDesc,
					JobComInvoiceLine.Schema.US_HazMatDesc,
					JobComInvoiceLine.Schema.US_HazWasteTrackingNo,
					JobComInvoiceLine.Schema.US_HFCInd,
					JobComInvoiceLine.Schema.US_HFCDisclaimReason,
					JobComInvoiceLine.Schema.US_ImportEntryNo,
					JobComInvoiceLine.Schema.US_IsBondedADD,
					JobComInvoiceLine.Schema.US_IsBondedCVD,
					JobComInvoiceLine.Schema.US_IsExcludedFromAII,
					JobComInvoiceLine.Schema.US_IsNAFTANet,
					JobComInvoiceLine.Schema.US_IsParent,
					JobComInvoiceLine.Schema.US_IsUsedVehicle,
					JobComInvoiceLine.Schema.US_JI_ParentProduct,
					JobComInvoiceLine.Schema.US_JurisdictionNumber,
					JobComInvoiceLine.Schema.US_LicenseNo,
					JobComInvoiceLine.Schema.US_LicenseType,
					JobComInvoiceLine.Schema.US_LicenseValue,
					JobComInvoiceLine.Schema.US_LumberExportCharges,
					JobComInvoiceLine.Schema.US_LumberExportPrice,
					JobComInvoiceLine.Schema.US_LumberImporterDeclaration,
					JobComInvoiceLine.Schema.US_ManifestQty,
					JobComInvoiceLine.Schema.US_MarksAndNumbers,
					JobComInvoiceLine.Schema.US_MiscPermitNo,
					JobComInvoiceLine.Schema.US_NAFTADutyFGN,
					JobComInvoiceLine.Schema.US_NAFTADutyRate,
					JobComInvoiceLine.Schema.US_NAFTADutyUS,
					JobComInvoiceLine.Schema.US_NAFTATariff,
					JobComInvoiceLine.Schema.US_NMFS370Ind,
					JobComInvoiceLine.Schema.US_NMFS370DisclaimReason,
					JobComInvoiceLine.Schema.US_NMFSAMRInd,
					JobComInvoiceLine.Schema.US_NMFSAMRDisclaimReason,
					JobComInvoiceLine.Schema.US_NMFSCOAInd,
					JobComInvoiceLine.Schema.US_NMFSHMSInd,
					JobComInvoiceLine.Schema.US_NMFSHMSDisclaimReason,
					JobComInvoiceLine.Schema.US_NMFSSIMPInd,
					JobComInvoiceLine.Schema.US_ODSInd,
					JobComInvoiceLine.Schema.US_UI_NKCarrierSCAC,
					JobComInvoiceLine.Schema.US_OverrideDuty,
					JobComInvoiceLine.Schema.US_OverrideSupDuty,
					JobComInvoiceLine.Schema.US_PIRPRulingNo,
					JobComInvoiceLine.Schema.US_PIRPRulingType,
					JobComInvoiceLine.Schema.US_PayableMPF,
					JobComInvoiceLine.Schema.US_PrivilegedStatusDate,
					JobComInvoiceLine.Schema.US_SPI,
					JobComInvoiceLine.Schema.US_SWPMIndicator,
					JobComInvoiceLine.Schema.US_SchDLoading,
					JobComInvoiceLine.Schema.US_SecondarySPI,
					JobComInvoiceLine.Schema.US_SelectedRateType,
					JobComInvoiceLine.Schema.US_SetInd,
					JobComInvoiceLine.Schema.US_SupDuty,
					JobComInvoiceLine.Schema.US_SupGoodsValue,
					JobComInvoiceLine.Schema.US_SupQty1,
					JobComInvoiceLine.Schema.US_SupQty2,
					JobComInvoiceLine.Schema.US_SupQty3,
					JobComInvoiceLine.Schema.US_SupTariff,
					JobComInvoiceLine.Schema.US_SupUQ1,
					JobComInvoiceLine.Schema.US_SupUQ2,
					JobComInvoiceLine.Schema.US_SupUQ3,
					JobComInvoiceLine.Schema.US_TSCAIndicator,
					JobComInvoiceLine.Schema.US_TSCAName,
					JobComInvoiceLine.Schema.US_TariffType,
					JobComInvoiceLine.Schema.US_TaxApply,
					JobComInvoiceLine.Schema.US_TaxCode,
					JobComInvoiceLine.Schema.US_TaxQty,
					JobComInvoiceLine.Schema.US_TaxRate,
					JobComInvoiceLine.Schema.US_TaxRateS,
					JobComInvoiceLine.Schema.US_TaxRateT,
					JobComInvoiceLine.Schema.US_TextileCategoryNo,
					JobComInvoiceLine.Schema.US_OMCInd,
					JobComInvoiceLine.Schema.US_OMCDisclaimReason,
					JobComInvoiceLine.Schema.US_TransactionsRelated,
					JobComInvoiceLine.Schema.US_APHISInd,
					JobComInvoiceLine.Schema.US_APHISDisclaimReason,
					JobComInvoiceLine.Schema.US_CPSCInd,
					JobComInvoiceLine.Schema.US_CPSCDisclaimReason,
					JobComInvoiceLine.Schema.US_FWSInd,
					JobComInvoiceLine.Schema.US_FWSDisclaimReason,
					JobComInvoiceLine.Schema.US_TTBDisclaimReason,
					JobComInvoiceLine.Schema.US_TTBInd,
					JobComInvoiceLine.Schema.US_UC_NKCountryOfExport,
					JobComInvoiceLine.Schema.US_UC_NKCountryOfOrigin,
					JobComInvoiceLine.Schema.US_VNEInd,
					JobComInvoiceLine.Schema.US_VehicleID,
					JobComInvoiceLine.Schema.US_VehicleIDType,
					JobComInvoiceLine.Schema.US_VehicleTitleNo,
					JobComInvoiceLine.Schema.US_VehicleTitleState,
					JobComInvoiceLine.Schema.US_VisaNo,
					JobComInvoiceLine.Schema.US_VisaQty,
					JobComInvoiceLine.Schema.US_VisaUQ,
					JobComInvoiceLine.Schema.US_WHSEntryLineNo,
					JobComInvoiceLine.Schema.US_WHSEntryNumber,
					JobComInvoiceLine.Schema.US_GrossWeight,
					JobComInvoiceLine.Schema.US_WeightNET,
					JobComInvoiceLine.Schema.US_WoolLicenceNo,
					JobComInvoiceLine.Schema.US_ZoneStatus,
					JobComInvoiceLine.Schema.US_ATFInd,
					JobComInvoiceLine.Schema.US_PSTIndicator,
					JobComInvoiceLine.Schema.US_LaceyIndicator,
					JobComInvoiceLine.Schema.US_NHTSAIndicator,
					JobComInvoiceLine.Schema.US_TSCAInd,
					JobComInvoiceLine.Schema.US_AMSInd,
					JobComInvoiceLine.Schema.US_NOPInd,
					JobComInvoiceLine.Schema.US_DEAInd,
					JobComInvoiceLine.Schema.US_TSCAODSCertIndividual,
					JobComInvoiceLine.Schema.US_TTBRateDesignationCode,
					JobComInvoiceLine.Schema.US_FTZCurrentTariff,
					JobComInvoiceLine.Schema.US_EPANetQty,
					JobComInvoiceLine.Schema.US_EPANetQtyUQ,
					JobComInvoiceLine.Schema.US_ExportCertificateNo,
					JobComInvoiceLine.Schema.US_FDAContactEmail,
					JobComInvoiceLine.Schema.US_FDAContactName,
					JobComInvoiceLine.Schema.US_FDAContactPhoneNo,
					JobComInvoiceLine.Schema.US_R_HTSChanged4ValueInd,
					JobComInvoiceLine.Schema.US_R_OrigEntryLineNo,
					JobComInvoiceLine.Schema.US_R_ReconReasonText,
					JobComInvoiceLine.Schema.US_Prim_NA,
					JobComInvoiceLine.Schema.US_RN_NKPrimCtry,
					JobComInvoiceLine.Schema.US_Sec_NA,
					JobComInvoiceLine.Schema.US_RN_NKSecCtry,
					JobComInvoiceLine.Schema.US_RN_NKCastCtry,
					JobComInvoiceLine.Schema.US_RN_NKCertOrigin,
					JobComInvoiceLine.Schema.US_RN_NKMeltCtry
				};
			}
		}
	}
}
