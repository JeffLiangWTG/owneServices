using CargoWise.EntityFramework;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Module
{
	public class CourtesyNoticesOfLiquidationFilterStripBusinessObject : FilterStripBusinessObject
	{
		public static class Constants
		{
			internal const string EntryNumberFilterName = "Entry #";
			internal const string EntryFiler = "Entry Filer";
			internal const string EntryType = "Entry Type";
			internal const string ImportOfRecordNo = "Importer Of Record #";
			internal const string ChangeLiquidationReasonCode = "Change Liquidation Reason Code";
			internal const string BrokerReferenceNo = "Job #";
			internal const string CustomsDocumentFilingLocation = "Customs Document Filing Location";
			internal const string LiquidationType = "Liquidation Type";
			internal const string ExtensionSuspensionCode = "Extension Suspension Code";
			internal const string EntryDate = "Entry Date";
			internal const string ExtensionSuspensionDate = "Extension Suspension Date";
			internal const string LiquidationDate = "Liquidation Date";
		}

		public CodeDescriptionPairList EntryTypeList => CusLiquidationLookups.GetEntryTypeList(Factory);

		public override ZQuery Filter
		{
			get
			{
				var result = base.Filter;
				result.AddToFilter(JoinCondition.And, CusLiquidationSchema.B8_GC, GlbCompany.CurrentCompany.PK);
				return result;
			}
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();

			result.AddNumberFilter(Constants.EntryNumberFilterName, CusLiquidationSchema.B8_EntryNumber);
			result.AddNumberFilter(Constants.EntryFiler, CusLiquidationSchema.B8_EntryFilerCode);
			result.AddNumberFilter(Constants.ImportOfRecordNo, CusLiquidationSchema.B8_ImportOfRecordNo);
			result.AddNumberFilter(Constants.BrokerReferenceNo, CusLiquidationSchema.B8_BrokerReferenceNo);
			result.AddNumberFilter(Constants.CustomsDocumentFilingLocation, CusLiquidationSchema.B8_CustomsDocumentFilingLocation);

			result.AddDateFilter(Constants.EntryDate, CusLiquidationSchema.B8_EntryDate);
			result.AddDateFilter(Constants.ExtensionSuspensionDate, CusLiquidationSchema.B8_ExtensionSuspensionDate);
			result.AddDateFilter(Constants.LiquidationDate, CusLiquidationSchema.B8_LiquidationDate);

			var entryTypeFilter = result.AddTextFilter(Constants.EntryType, CusLiquidationSchema.B8_EntryType, EntryTypeList);
			entryTypeFilter.Category = FilterCategories.NumbersAndReferences;

			var changeLiquidationReasonCodeFilter = result.AddTextFilter(Constants.ChangeLiquidationReasonCode, CusLiquidationSchema.B8_ChangeLiquidationReasonCode, new ChangeLiquidationReasonCodeList());
			changeLiquidationReasonCodeFilter.Category = FilterCategories.StatusAndFlags;

			var extensionSuspensionCodeFilter = result.AddTextFilter(Constants.ExtensionSuspensionCode, CusLiquidationSchema.B8_ExtensionSuspensionCode, new ExtensionSuspensionCodeList(Factory));
			extensionSuspensionCodeFilter.Category = FilterCategories.StatusAndFlags;

			var liquidationTypeFilter = result.AddTextFilter(Constants.LiquidationType, CusLiquidationSchema.B8_LiquidationType, new LiquidationTypeCodeList());
			liquidationTypeFilter.Category = FilterCategories.StatusAndFlags;

			return result;
		}
	}
}
