using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.TW.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using ResString = Enterprise.Customs.TW.Module.ResString;

namespace Enterprise.Customs.TW.Transhipment.Module
{
	public class CusInBondHeaderFilterStripBusinessObject : FilterStripBusinessObject
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter constant")]
		public static class FilterConstants
		{
			public const string Importer = "Importer";
			public const string TransportMode = "TransportMode";
			public const string LoadingPort = "Loading Port";
			public const string Vessel = "Vessel";
			public const string VesselREG = "Vessel REG";
			public const string Voyage = "Voyage";
			public const string ETA = "ETA";
			public const string Carrier = "Carrier";
			public const string ImportMasterBill = "Import Master Bill";
			public const string ImportHouseBill = "Import House Bill";
			public const string EntryNumber = "Entry #";
			public const string BH_JobReference = "Job Reference";
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();
			AddFilters(result);
			return result;
		}

		protected override List<IFilterStripsHelper> GetCustomFilterStripsHelpersCore()
		{
			var helpers = base.GetCustomFilterStripsHelpersCore();
			var workflowHelper = new WorkflowFilterStripsHelperCusInBond(typeof(CusInBondHeader), WorkflowDescriptors.CusInBondHeaderWorkflowDescriptorCode, Factory);
			workflowHelper.SetShouldAddWorkflowCustomFieldsFilters(true);
			helpers.Add(workflowHelper);
			return helpers;
		}

		public override ZQuery Filter
		{
			get
			{
				var result = base.Filter;
				result.AddToFilter(GetCompanyQuery());
				return result;
			}
		}

		ZQuery GetCompanyQuery()
		{
			var result = new ZDBOnlyQuery(typeof(CusInBondHeader));
			var companySubQuery = new ZDBOnlySubQuery(typeof(GlbBranch), CusInBondHeaderSchema.BH_GB);
			companySubQuery.AddToFilter(GlbBranchSchema.GB_GC, GlbCompany.CurrentCompany.PK);

			result.AddSubQuery(companySubQuery, JoinCondition.And);
			return result;
		}

		void AddFilters(ModuleFilterCollection filters)
		{
			var transportModeFilter = filters.AddTextFilter(FilterConstants.TransportMode, CusInBondHeaderSchema.BH_ImportTransportMode, TransportModeCodes);
			transportModeFilter.Category = FilterCategories.ModesAndTypes;
			transportModeFilter.MultilingualDescription = ResString.GetMultilingualString("CusInBondHeaderFilterStripBusinessObject|TransportMode", "Transport Mode");

			var loadingSchedK = filters.AddTextFilter(FilterConstants.LoadingPort, CusInBondHeaderSchema.BH_RL_NKImportLoadPort);
			loadingSchedK.Category = FilterCategories.Locations;
			loadingSchedK.MultilingualDescription = ResString.GetMultilingualString("CusInBondHeaderFilterStripBusinessObject|LoadingPort", FilterConstants.LoadingPort);

			var importerFilter = filters.AddGuidFilter(FilterConstants.Importer, ModuleIDs.Organisation, GetImporterQuery, ImporterList);
			importerFilter.Category = FilterCategories.Organisations;
			importerFilter.MultilingualDescription = ResString.GetMultilingualString("CusInBondHeaderFilterStripBusinessObject|Importer", FilterConstants.Importer);

			var carrierFilter = filters.AddGuidFilter(FilterConstants.Carrier, ModuleIDs.Organisation, CusInBondHeaderSchema.BH_OH_Carrier, Carriers);
			carrierFilter.Category = FilterCategories.Organisations;
			carrierFilter.MultilingualDescription = ResString.GetMultilingualString("CusInBondHeaderFilterStripBusinessObject|Carrier", FilterConstants.Carrier);

			var etaFilter = filters.AddDateFilter(FilterConstants.ETA, CusInBondHeaderSchema.BH_ETA);
			etaFilter.Category = FilterCategories.Dates;
			etaFilter.MultilingualDescription = ResString.GetMultilingualString("CusInBondHeaderFilterStripBusinessObject|ETA", FilterConstants.ETA);

			var voyageFilter = filters.AddTextFilter(FilterConstants.Voyage, CusInBondHeaderSchema.BH_UniqueVoyageIdentifier);
			voyageFilter.Category = FilterCategories.TextSearch;
			voyageFilter.MultilingualDescription = ResString.GetMultilingualString("CusInBondHeaderFilterStripBusinessObject|Voyage", FilterConstants.Voyage);

			var vesselFilter = filters.AddTextFilter(FilterConstants.Vessel, CusInBondHeaderSchema.BH_ImportConveyanceName);
			vesselFilter.Category = FilterCategories.TextSearch;
			vesselFilter.MultilingualDescription = ResString.GetMultilingualString("CusInBondHeaderFilterStripBusinessObject|Vessel", FilterConstants.Vessel);

			var vesselREGFilter = filters.AddTextFilter(FilterConstants.VesselREG, CusInBondHeaderSchema.BH_VoyageNumber);
			vesselREGFilter.Category = FilterCategories.TextSearch;
			vesselREGFilter.MaxLength = 6;
			vesselREGFilter.MultilingualDescription = ResString.GetMultilingualString("CusInBondHeaderFilterStripBusinessObject|VesselREG", FilterConstants.VesselREG);

			var entryNumberFilter = filters.AddNumberFilter(FilterConstants.EntryNumber, (op, value) => GetEntryNumberQuery(new ZDBOnlyQuery(typeof(CusInBondHeader)), value, op));
			entryNumberFilter.Category = FilterCategories.NumbersAndReferences;
			entryNumberFilter.MaxLength = CusInBondHeader.Schema.EntryNumberMaxLength;
			entryNumberFilter.MultilingualDescription = ResString.GetMultilingualString("CusInBondHeaderFilterStripBusinessObject|EntryNumber", FilterConstants.EntryNumber);

			var cusInBondBillSubGroup = new CusInBondBillSubGroup();
			var importMasterBillFilter = filters.AddNumberFilter(FilterConstants.ImportMasterBill, CusInBondBillSchema.B0_MasterBillNumber);
			importMasterBillFilter.Category = FilterCategories.NumbersAndReferences;
			importMasterBillFilter.SubGroup = cusInBondBillSubGroup;
			importMasterBillFilter.MultilingualDescription = ResString.GetMultilingualString("CusInBondHeaderFilterStripBusinessObject|MasterBill", FilterConstants.ImportMasterBill);

			var importHouseBillFilter = filters.AddNumberFilter(FilterConstants.ImportHouseBill, CusInBondBillSchema.B0_HouseBillNumber);
			importHouseBillFilter.Category = FilterCategories.NumbersAndReferences;
			importHouseBillFilter.SubGroup = cusInBondBillSubGroup;
			importHouseBillFilter.MultilingualDescription = ResString.GetMultilingualString("CusInBondHeaderFilterStripBusinessObject|HouseBill", FilterConstants.ImportHouseBill);

			var jobReferenceFilter = filters.AddTextFilter(FilterConstants.BH_JobReference, CusInBondHeaderSchema.BH_JobReference);
			jobReferenceFilter.Category = FilterCategories.NumbersAndReferences;
			jobReferenceFilter.MaxLength = CusInBondHeader.Schema.BH_JobReferenceMaxLength;
			jobReferenceFilter.MultilingualDescription = ResString.GetMultilingualString("CusInBondHeaderFilterStripBusinessObject|BH_JobReference", FilterConstants.BH_JobReference);
		}

		ZQuery GetImporterQuery(ZGuid importerPK)
		{
			var addressQuery = new ZDBOnlySubQuery(typeof(OrgAddress), OrgAddressSchema.PK);
			addressQuery.AddToFilter(OrgAddressSchema.OA_OH, importerPK);

			var result = new ZDBOnlyQuery(typeof(CusInBondHeader));
			result.AddSubQuery(CusInBondHeaderSchema.BH_OA_Importer, addressQuery, JoinCondition.And);

			return result;
		}

		ZDBOnlyQuery GetEntryNumberQuery(ZDBOnlyQuery query, ZString value, SQLComparisonOperator comparison)
		{
			var isBlank = comparison == SpecialComparisonOperator.IsBlank;
			var addOnColumnQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID, isBlank);
			addOnColumnQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, Enterprise.Customs.Common.CusEntryNumberTypes.Taiwan.Transhipment);
			addOnColumnQuery.AddToFilter(CusEntryNumSchema.CE_Category, CusEntryNumber.Categories.CustomsPermitClearanceNumber);
			addOnColumnQuery.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, Core.Constants.CountryCodes.Taiwan);
			addOnColumnQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, isBlank ? SQLComparisonOperator.IsNotBlank : comparison, value);
			query.AddSubQuery(CusInBondHeaderSchema.PK, addOnColumnQuery, JoinCondition.And);
			return query;
		}

		class CusInBondBillSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var subQuery = new ZDBOnlySubQuery(typeof(CusInBondBill), CusInBondBillSchema.B0_BH);
				subQuery.AddToFilter(filter);
				subQuery.AddToFilter(CusInBondBillSchema.B0_ShipmentType, Constants.CusInBondBill.ShipmentType.Import);

				var dbOnlyResult = new ZDBOnlyQuery(typeof(CusInBondHeader));
				dbOnlyResult.AddSubQuery(subQuery, JoinCondition.And);

				return dbOnlyResult;
			}
		}

		#region Lookups
		public ConsigneeCollection ImporterList => new ConsigneeCollection(Factory);

		public CodeDescriptionPairList TransportModeCodes => Factory.GetCachedValue<InBondTransportModeCodes>();

		public ShippingProviderCollection Carriers => new ShippingProviderCollection(Factory);

		public CusInBondHeaderFilterLookups Lookups => new CusInBondHeaderFilterLookups(this);
		#endregion
	}
}
