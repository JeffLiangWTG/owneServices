using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business.Accounting.EInvoicing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.MasterFiles.Business.GermanyOrgCusCodeInfo;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public class GermanyComplianceInfo : CountryComplianceInfo,
										IComplianceInfoElectronicInvoicing,
										IFiscalTaxCodeProvider
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.Germany;
		protected override string GetConsumptionTaxCode() => OrgCusCodes.MST;
		protected override string GetConsumptionTaxRegistrationCode() => OrgCusCodes.UST;
		protected override string GetLocalBusinessRegNoCodeType() => GermanyOrgCusCodeInfo.OrgCusCodes.Handelsregister;
		protected override bool? GetIsReciprocal() => false;
		protected override bool? GetDefaultValueForDisplayRecipientTaxIDRegistry() => true;

		#endregion

		#region IComplianceInfoElectronicInvoicing

		ZDate IComplianceInfoElectronicInvoicing.GetEInvoicingComplianceDate() => new ZDate(2020, 11, 27);

		ZDate IComplianceInfoElectronicInvoicing.GetEInvoicingComplianceDateForPayables() => ZDate.Empty;

		string IComplianceInfoElectronicInvoicing.GetDefaultEInvoicingSubmitPivotStatus() => Constants.EInvoicingPivotState.Queued;

		string IComplianceInfoElectronicInvoicing.GetDefaultEInvoicingSubmitPivotStatusForPayables(GlbCompany currenCompany) => string.Empty;

		MultilingualString IComplianceInfoElectronicInvoicing.GetDefaultEInvoicingPivotPendingStatusDescription() => IEInvoicingTransactionConstants.DefaultEInvoicingPivotPendingStatusDescription;

		string IComplianceInfoElectronicInvoicing.GetGovernmentAllocatedNumberColumnName() => AccEInvoicingBatchSchema.Constants.AIB_GovernmentAllocatedNumber;

		string IComplianceInfoElectronicInvoicing.GetAccTransactionHeaderAuthorisationRecordType() => string.Empty;

		#endregion

		#region IFiscalTaxCodeProvider
		CodeDescriptionPairList IFiscalTaxCodeProvider.GetFiscalTaxCodeForOutputNetAmount()
		{
			return new FiscalOutputNetCodeList();
		}

		CodeDescriptionPairList IFiscalTaxCodeProvider.GetFiscalTaxCodeForOutputTaxAmount()
		{
			return new FiscalOutputTaxCodeList();
		}

		CodeDescriptionPairList IFiscalTaxCodeProvider.GetFiscalTaxCodeForInputTaxAmount()
		{
			return new FiscalInputTaxCodeList();
		}

		CodeDescriptionWithThreeGroupsCollection IFiscalTaxCodeProvider.GetValidFiscalTaxCodeCombinations(ReadOnlyCodeDescriptionPairList ont, ReadOnlyCodeDescriptionPairList otx, ReadOnlyCodeDescriptionPairList itx)
		{
			var result = new CodeDescriptionWithThreeGroupsCollection(groupLookup: ont,
																							group2Lookup: otx,
																							group3Lookup: itx,
																							codeMaxLength: 17);
			// The code for a valid combination indicates a combination of valid fiscal tax codes as well as the sort order the single fiscal tax codes are displayed in the new USTVA pop-up form, e.g. 46:40_47:41_67:68, a colon separates the fiscal tax code from the sort order number.
			result.Add("81:01_NA_NA", FiscalOutputNetCodeList.Codes.ONT81, FiscalOutputTaxCodeList.Codes.OTXNA, FiscalInputTaxCodeList.Codes.ITXNA);
			result.Add("86:02_NA_NA", FiscalOutputNetCodeList.Codes.ONT86, FiscalOutputTaxCodeList.Codes.OTXNA, FiscalInputTaxCodeList.Codes.ITXNA);
			result.Add("35:03_36:04_NA", FiscalOutputNetCodeList.Codes.ONT35, FiscalOutputTaxCodeList.Codes.OTX36, FiscalInputTaxCodeList.Codes.ITXNA);
			result.Add("77:05_NA_NA", FiscalOutputNetCodeList.Codes.ONT77, FiscalOutputTaxCodeList.Codes.OTXNA, FiscalInputTaxCodeList.Codes.ITXNA);
			result.Add("76:06_80:07_NA", FiscalOutputNetCodeList.Codes.ONT76, FiscalOutputTaxCodeList.Codes.OTX80, FiscalInputTaxCodeList.Codes.ITXNA);
			result.Add("41:15_NA_NA", FiscalOutputNetCodeList.Codes.ONT41, FiscalOutputTaxCodeList.Codes.OTXNA, FiscalInputTaxCodeList.Codes.ITXNA);
			result.Add("44:16_NA_NA", FiscalOutputNetCodeList.Codes.ONT44, FiscalOutputTaxCodeList.Codes.OTXNA, FiscalInputTaxCodeList.Codes.ITXNA);
			result.Add("49:17_NA_NA", FiscalOutputNetCodeList.Codes.ONT49, FiscalOutputTaxCodeList.Codes.OTXNA, FiscalInputTaxCodeList.Codes.ITXNA);
			result.Add("43:18_NA_NA", FiscalOutputNetCodeList.Codes.ONT43, FiscalOutputTaxCodeList.Codes.OTXNA, FiscalInputTaxCodeList.Codes.ITXNA);
			result.Add("48:19_NA_NA", FiscalOutputNetCodeList.Codes.ONT48, FiscalOutputTaxCodeList.Codes.OTXNA, FiscalInputTaxCodeList.Codes.ITXNA);
			result.Add("91:25_NA_NA", FiscalOutputNetCodeList.Codes.ONT91, FiscalOutputTaxCodeList.Codes.OTXNA, FiscalInputTaxCodeList.Codes.ITXNA);
			result.Add("89:26_NA_NA", FiscalOutputNetCodeList.Codes.ONT89, FiscalOutputTaxCodeList.Codes.OTXNA, FiscalInputTaxCodeList.Codes.ITXNA);
			result.Add("93:27_NA_NA", FiscalOutputNetCodeList.Codes.ONT93, FiscalOutputTaxCodeList.Codes.OTXNA, FiscalInputTaxCodeList.Codes.ITXNA);
			result.Add("95:28_98:29_NA", FiscalOutputNetCodeList.Codes.ONT95, FiscalOutputTaxCodeList.Codes.OTX98, FiscalInputTaxCodeList.Codes.ITXNA);
			result.Add("94:30_96:31_NA", FiscalOutputNetCodeList.Codes.ONT94, FiscalOutputTaxCodeList.Codes.OTX96, FiscalInputTaxCodeList.Codes.ITXNA);
			result.Add("46:40_47:41_67:68", FiscalOutputNetCodeList.Codes.ONT46, FiscalOutputTaxCodeList.Codes.OTX47, FiscalInputTaxCodeList.Codes.ITX67);
			result.Add("73:43_74:44_67:68", FiscalOutputNetCodeList.Codes.ONT73, FiscalOutputTaxCodeList.Codes.OTX74, FiscalInputTaxCodeList.Codes.ITX67);
			result.Add("84:46_85:47_67:68", FiscalOutputNetCodeList.Codes.ONT84, FiscalOutputTaxCodeList.Codes.OTX85, FiscalInputTaxCodeList.Codes.ITX67);
			result.Add("42:55_NA_NA", FiscalOutputNetCodeList.Codes.ONT42, FiscalOutputTaxCodeList.Codes.OTXNA, FiscalInputTaxCodeList.Codes.ITXNA);
			result.Add("60:56_NA_NA", FiscalOutputNetCodeList.Codes.ONT60, FiscalOutputTaxCodeList.Codes.OTXNA, FiscalInputTaxCodeList.Codes.ITXNA);
			result.Add("21:57_NA_NA", FiscalOutputNetCodeList.Codes.ONT21, FiscalOutputTaxCodeList.Codes.OTXNA, FiscalInputTaxCodeList.Codes.ITXNA);
			result.Add("45:58_NA_NA", FiscalOutputNetCodeList.Codes.ONT45, FiscalOutputTaxCodeList.Codes.OTXNA, FiscalInputTaxCodeList.Codes.ITXNA);
			result.Add("NA_NA_66:65", FiscalOutputNetCodeList.Codes.ONTNA, FiscalOutputTaxCodeList.Codes.OTXNA, FiscalInputTaxCodeList.Codes.ITX66);
			result.Add("NA_NA_61:66", FiscalOutputNetCodeList.Codes.ONTNA, FiscalOutputTaxCodeList.Codes.OTXNA, FiscalInputTaxCodeList.Codes.ITX61);
			result.Add("NA_NA_62:67", FiscalOutputNetCodeList.Codes.ONTNA, FiscalOutputTaxCodeList.Codes.OTXNA, FiscalInputTaxCodeList.Codes.ITX62);
			result.Add("NA_NA_63:69", FiscalOutputNetCodeList.Codes.ONTNA, FiscalOutputTaxCodeList.Codes.OTXNA, FiscalInputTaxCodeList.Codes.ITX63);
			result.Add("NA_NA_59:70", FiscalOutputNetCodeList.Codes.ONTNA, FiscalOutputTaxCodeList.Codes.OTXNA, FiscalInputTaxCodeList.Codes.ITX59);
			result.Add("NA_NA_64:71", FiscalOutputNetCodeList.Codes.ONTNA, FiscalOutputTaxCodeList.Codes.OTXNA, FiscalInputTaxCodeList.Codes.ITX64);
			result.Add("NA_NA_65:72", FiscalOutputNetCodeList.Codes.ONTNA, FiscalOutputTaxCodeList.Codes.OTXNA, FiscalInputTaxCodeList.Codes.ITX65);
			result.Add("NA_NA_69:73", FiscalOutputNetCodeList.Codes.ONTNA, FiscalOutputTaxCodeList.Codes.OTXNA, FiscalInputTaxCodeList.Codes.ITX69);
			result.Add("NA_NA_NA", FiscalOutputNetCodeList.Codes.ONTNA, FiscalOutputTaxCodeList.Codes.OTXNA, FiscalInputTaxCodeList.Codes.ITXNA);

			return result;
		}

		#endregion
	}
}
