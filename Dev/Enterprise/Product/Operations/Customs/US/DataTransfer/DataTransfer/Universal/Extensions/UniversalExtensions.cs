using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.DataTransfer.Universal.AddInfoExtensions;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.DataTransfer.Universal.Extensions
{
	public static class UniversalExtensions
	{
		public static IEnumerable<string> GetInvoiceLineAddInfosApplicableForInwardWarehousing(this BusinessObjectFactory factory)
		{
			IEnumerable<string> result = null;
			if (factory == null)
			{
				result = Enumerable.Empty<string>();
			}
			else
			{
				result = factory.GetCachedValue<IEnumerable<string>>("USInvoiceLineAddInfosApplicableForInwardWarehousing", () =>
					{
						var invoiceLineSchemaDictionary = factory.GetAddInfoSchemaDictionary<JobComInvoiceLine>(USAddInfoSchema.Instance);
						var list = new List<string>(invoiceLineSchemaDictionary.Keys.OrderBy(o => o));
						foreach (var keyToRemove in InvoiceLineAddInfoFieldsNotApplicableToInwardWarehousing.Select(x => x.Substring(3)))
						{
							list.Remove(keyToRemove);
						}
						return list;
					});
			}
			return result;
		}

		static string[] InvoiceLineAddInfoFieldsNotApplicableToInwardWarehousing
		{
			get
			{
				return new[]
				{
					JobComInvoiceLine.Schema.US_CH_ReconEntry,
					JobComInvoiceLine.Schema.US_DRWCDInd,
					JobComInvoiceLine.Schema.US_DRWCDUse,
					JobComInvoiceLine.Schema.US_DRWCertOfManufacture,
					JobComInvoiceLine.Schema.US_DRWClaimAmountOverriden_New,
					JobComInvoiceLine.Schema.US_DRWCMCDIndicator,
					JobComInvoiceLine.Schema.US_DRWDateDelFrom,
					JobComInvoiceLine.Schema.US_DRWDateDelTo,
					JobComInvoiceLine.Schema.US_DRWDateRcvFrom,
					JobComInvoiceLine.Schema.US_DRWDateRcvTo,
					JobComInvoiceLine.Schema.US_DRWDateUsedFrom,
					JobComInvoiceLine.Schema.US_DRWDateUsedTo,
					JobComInvoiceLine.Schema.US_DRWDeclaredHMF,
					JobComInvoiceLine.Schema.US_DRWDeclaredMPF,
					JobComInvoiceLine.Schema.US_DRWDeclaredOtherFees,
					JobComInvoiceLine.Schema.US_DRWDeclaredTax,
					JobComInvoiceLine.Schema.US_DRWDeclaredVFD,
					JobComInvoiceLine.Schema.US_DRWDutyRate_New,
					JobComInvoiceLine.Schema.US_DRWEntryDate,
					JobComInvoiceLine.Schema.US_DRWExportAction,
					JobComInvoiceLine.Schema.US_DRWExportDate,
					JobComInvoiceLine.Schema.US_DRWExportDest,
					JobComInvoiceLine.Schema.US_DRWExportID,
					JobComInvoiceLine.Schema.US_DRWExportQuantity,
					JobComInvoiceLine.Schema.US_DRWImportEntryLine,
					JobComInvoiceLine.Schema.US_DRWImportQuantity,
					JobComInvoiceLine.Schema.US_DRWImportUQ,
					JobComInvoiceLine.Schema.US_DRWIntendedPortOfExport,
					JobComInvoiceLine.Schema.US_DRWIsForExportSection,
					JobComInvoiceLine.Schema.US_DRWIsForImportSection,
					JobComInvoiceLine.Schema.US_DRWMethodOfDestruction,
					JobComInvoiceLine.Schema.US_DRWNoticeOfIntentLastPrint,
					JobComInvoiceLine.Schema.US_DRWPort,
					JobComInvoiceLine.Schema.US_DRWTENo,
					JobComInvoiceLine.Schema.US_DRWWeightedRatio,
					JobComInvoiceLine.Schema.US_DRWMPFWeightedRatio,
					JobComInvoiceLine.Schema.US_DRWLineDuty,
					JobComInvoiceLine.Schema.US_DRWLineDutyRateDesc,
					JobComInvoiceLine.Schema.US_DRWOldData,
					JobComInvoiceLine.Schema.US_ImpDecInvoiceNum,
					JobComInvoiceLine.Schema.US_R_Orig98Value,
					JobComInvoiceLine.Schema.US_R_OrigCottonFeeExempt,
					JobComInvoiceLine.Schema.US_R_OrigCV,
					JobComInvoiceLine.Schema.US_R_OrigDuty,
					JobComInvoiceLine.Schema.US_R_OrigFirstQty,
					JobComInvoiceLine.Schema.US_R_OrigFirstUQ,
					JobComInvoiceLine.Schema.US_R_OrigHasMPF,
					JobComInvoiceLine.Schema.US_R_OrigOverrideDuty,
					JobComInvoiceLine.Schema.US_R_OrigOverrideSupDuty,
					JobComInvoiceLine.Schema.US_R_OrigRateType,
					JobComInvoiceLine.Schema.US_R_OrigSecondQty,
					JobComInvoiceLine.Schema.US_R_OrigSecondUQ,
					JobComInvoiceLine.Schema.US_R_OrigSPI,
					JobComInvoiceLine.Schema.US_R_OrigSupDuty,
					JobComInvoiceLine.Schema.US_R_OrigSupQty1,
					JobComInvoiceLine.Schema.US_R_OrigSupQty2,
					JobComInvoiceLine.Schema.US_R_OrigSupQty3,
					JobComInvoiceLine.Schema.US_R_OrigSupTariff,
					JobComInvoiceLine.Schema.US_R_OrigSupUQ1,
					JobComInvoiceLine.Schema.US_R_OrigSupUQ2,
					JobComInvoiceLine.Schema.US_R_OrigSupUQ3,
					JobComInvoiceLine.Schema.US_R_OrigTariff,
					JobComInvoiceLine.Schema.US_R_OrigTaxApply,
					JobComInvoiceLine.Schema.US_R_OrigTaxCode,
					JobComInvoiceLine.Schema.US_R_OrigTaxQty,
					JobComInvoiceLine.Schema.US_R_OrigTaxRate,
					JobComInvoiceLine.Schema.US_R_OrigTaxRateS,
					JobComInvoiceLine.Schema.US_R_OrigTaxRateT,
					JobComInvoiceLine.Schema.US_R_OrigThirdQty,
					JobComInvoiceLine.Schema.US_R_OrigThirdUQ,
					JobComInvoiceLine.Schema.US_R_Textile,
					JobComInvoiceLine.Schema.US_LaceyDisclaimReason,
					JobComInvoiceLine.Schema.US_FSISDisclaimReason,
					JobComInvoiceLine.Schema.US_ODSDisclaimReason,
					JobComInvoiceLine.Schema.US_PSTDisclaimProgram,
					JobComInvoiceLine.Schema.US_PSTDisclaimReason,
					JobComInvoiceLine.Schema.US_TSCACertification,
					JobComInvoiceLine.Schema.US_TSCADisclaimReason,
					JobComInvoiceLine.Schema.US_VNEDisclaimReason,
					JobComInvoiceLine.Schema.US_AMSDisclaimProgram,
					JobComInvoiceLine.Schema.US_AMSDisclaimReason,
					JobComInvoiceLine.Schema.US_NOPDisclaimReason,
					JobComInvoiceLine.Schema.US_NHTDisclaimReason,
					JobComInvoiceLine.Schema.US_FDADisclaimReason,
					JobComInvoiceLine.Schema.US_DRWDateOfManufacture,
					JobComInvoiceLine.Schema.US_DRWDescrManufactured,
					JobComInvoiceLine.Schema.US_DRWDescrUsed,
					JobComInvoiceLine.Schema.US_DRWFactoryLocation,
					JobComInvoiceLine.Schema.US_DRWIsForManufacturerSection,
					JobComInvoiceLine.Schema.US_DRWQuantityUsed,
					JobComInvoiceLine.Schema.US_DRWUQUsed,
					JobComInvoiceLine.Schema.US_DRWExportUQ,
					JobComInvoiceLine.Schema.US_DRWAdValoremRate,
					JobComInvoiceLine.Schema.US_DRWCalcDutyWithAdValoremRate,
					JobComInvoiceLine.Schema.US_DDTCTrackingStatus,
					JobComInvoiceLine.Schema.US_ODSTrackingStatus,
					JobComInvoiceLine.Schema.US_TSCATrackingStatus,
					JobComInvoiceLine.Schema.US_PGATrackingID,
					JobComInvoiceLine.Schema.US_DRWImpActInd,
					JobComInvoiceLine.Schema.US_DEADisclaimReason,
					JobComInvoiceLine.Schema.US_DRWImpManufRuleNo,
					JobComInvoiceLine.Schema.US_DRWClaimBasis,
					JobComInvoiceLine.Schema.US_DRWAllowQty,
					JobComInvoiceLine.Schema.US_DRWValuePerUQ,
					JobComInvoiceLine.Schema.US_DRWSubstituted,
					JobComInvoiceLine.Schema.US_DRWImportQuantity2,
					JobComInvoiceLine.Schema.US_DRWImportUQ2,
					JobComInvoiceLine.Schema.US_DRWAllowQty2,
					JobComInvoiceLine.Schema.US_DRWValuePerUQ2,
					JobComInvoiceLine.Schema.US_DRWSubstituted2,
					JobComInvoiceLine.Schema.US_DRWImportQuantity3,
					JobComInvoiceLine.Schema.US_DRWImportUQ3,
					JobComInvoiceLine.Schema.US_DRWAllowQty3,
					JobComInvoiceLine.Schema.US_DRWValuePerUQ3,
					JobComInvoiceLine.Schema.US_DRWSubstituted3,
					JobComInvoiceLine.Schema.US_DRWManufRuleNo,
					JobComInvoiceLine.Schema.US_DRWExpNoticeInd,
					JobComInvoiceLine.Schema.US_DRWExpWavInd,
					JobComInvoiceLine.Schema.US_DRWExpBOLInd,
					JobComInvoiceLine.Schema.US_DRWExpBOLCarrier,
					JobComInvoiceLine.Schema.US_DRWCalcDuty,
					JobComInvoiceLine.Schema.US_DRWCalcHMF,
					JobComInvoiceLine.Schema.US_DRWQuarterlyHMF,
					JobComInvoiceLine.Schema.US_DRWCalcMPF,
					JobComInvoiceLine.Schema.US_DRWCalcTax,
					JobComInvoiceLine.Schema.US_DRWAccMethod,
					JobComInvoiceLine.Schema.US_DRWImpTrkID,
					JobComInvoiceLine.Schema.US_DRWMafTrkID,
					JobComInvoiceLine.Schema.US_DRWMafActInd,
					JobComInvoiceLine.Schema.US_DRWSubstituted,
					JobComInvoiceLine.Schema.US_TSCALineNumber,
					JobComInvoiceLine.Schema.US_ODSLineNumber,
					JobComInvoiceLine.Schema.US_ManifestUQ,
					JobComInvoiceLine.Schema.US_ProductExclusion,
					JobComInvoiceLine.Schema.US_ExclusionNumber,
					JobComInvoiceLine.Schema.US_FTADuty,
					JobComInvoiceLine.Schema.US_FTAPayableMPF,
					JobComInvoiceLine.Schema.US_FTASPI,
					JobComInvoiceLine.Schema.US_NonFTADuty,
					JobComInvoiceLine.Schema.US_NonFTAPayableMPF,
					JobComInvoiceLine.Schema.US_DRWAdjClaimDuty,
					JobComInvoiceLine.Schema.US_DRWAdjClaimHMF,
					JobComInvoiceLine.Schema.US_DRWAdjClaimMPF,
					JobComInvoiceLine.Schema.US_DRWAdjClaimTax
				};
			}
		}
	}
}
