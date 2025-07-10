using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public class RefVesselZZFilterBusinessObject : FilterStripBusinessObject
	{
		#region Descriptions

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter related description")]
		public static class Descriptions
		{
			public const string LloydsNumber = "Lloyds Number";
			public const string RadioCallSign = "Radio Call Sign";
			public const string VesselName = "Vessel Name";
			public const string CountryOfRegistration = "Country of Registration";
			public const string VesselType = "Vessel Type";
			public const string CarrierCodes = "Carrier Code(s)";
			public const string CarrierNames = "Carrier Name(s)";
		}

		#endregion

		public IList<string> ApplicableModuleFilters;

		bool ApplicableModuleFilter(string filterColumn)
		{
			return ApplicableModuleFilters == null || ApplicableModuleFilters.Contains(filterColumn);
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = new ModuleFilterCollection();
			AddTextFilters(filters);
			AddNumbersAndReferencesFilters(filters);
			AddLocationFilters(filters);
			AddModesAndTypesFilters(filters);
			return filters;
		}

		#region AddTextFilters

		void AddTextFilters(ModuleFilterCollection filters)
		{
			if (ApplicableModuleFilter(RefVesselZZFilterBusinessObject.Descriptions.CarrierCodes))
			{
				var carrierCodesFilter = filters.AddTextFilter(Descriptions.CarrierCodes, GetCarrierCodesQuery);
				carrierCodesFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|RefVesselZZFilter|CarrierCodes", "Carrier Code(s)");
				carrierCodesFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.Exact);
				carrierCodesFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotEqual);
				carrierCodesFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsBlank);
				carrierCodesFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsNotBlank);
				carrierCodesFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.StartsWith);
				carrierCodesFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotStartsWith);
				carrierCodesFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotContain);
			}

			if (ApplicableModuleFilter(RefVesselZZFilterBusinessObject.Descriptions.CarrierNames))
			{
				var carrierNamesFilter = filters.AddTextFilter(Descriptions.CarrierNames, GetCarrierNamesQuery);
				carrierNamesFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|RefVesselZZFilter|CarrierNames", "Carrier Name(s)");
				carrierNamesFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.Exact);
				carrierNamesFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotEqual);
				carrierNamesFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsBlank);
				carrierNamesFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsNotBlank);
				carrierNamesFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.StartsWith);
				carrierNamesFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotStartsWith);
				carrierNamesFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotContain);
			}
		}

			ZQuery GetCarrierCodesQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var query = new ZDBOnlyQuery(typeof(RefVesselZZ));
			query.AddToFilter(RefVesselZZSchema.ZZO_ZZZ_NKDataGrouping, Core.Constants.CountryCodes.SouthAfrica);
			var refCarrierVesselPivotSubQuery = new ZDBOnlySubQuery(typeof(RefCarrierVesselPivot), RefCarrierVesselPivotSchema.ZZQ_ZZO);
			var refCarrierCodeSubQuery = new ZDBOnlySubQuery(typeof(RefCarrierCode), RefCarrierCodeSchema.PK);
			refCarrierCodeSubQuery.AddToFilter(RefCarrierCodeSchema.ZZ4_ZZZ_NKDataGrouping, Core.Constants.CountryCodes.SouthAfrica);
			refCarrierCodeSubQuery.AddToFilter(RefCarrierCodeSchema.ZZ4_Code, SQLComparisonOperator.Contains, value);
			refCarrierVesselPivotSubQuery.AddSubQuery(RefCarrierVesselPivotSchema.ZZQ_ZZ4, refCarrierCodeSubQuery, JoinCondition.And);
			query.AddSubQuery(RefVesselZZSchema.PK, refCarrierVesselPivotSubQuery, JoinCondition.And);
			return query;
		}

		ZQuery GetCarrierNamesQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var query = new ZDBOnlyQuery(typeof(RefVesselZZ));
			query.AddToFilter(RefVesselZZSchema.ZZO_ZZZ_NKDataGrouping, Core.Constants.CountryCodes.SouthAfrica);
			var refCarrierVesselPivotSubQuery = new ZDBOnlySubQuery(typeof(RefCarrierVesselPivot), RefCarrierVesselPivotSchema.ZZQ_ZZO);
			var refCarrierCodeSubQuery = new ZDBOnlySubQuery(typeof(RefCarrierCode), RefCarrierCodeSchema.PK);
			refCarrierCodeSubQuery.AddToFilter(RefCarrierCodeSchema.ZZ4_ZZZ_NKDataGrouping, Core.Constants.CountryCodes.SouthAfrica);
			refCarrierCodeSubQuery.AddToFilter(RefCarrierCodeSchema.ZZ4_Description, SQLComparisonOperator.Contains, value);
			refCarrierVesselPivotSubQuery.AddSubQuery(RefCarrierVesselPivotSchema.ZZQ_ZZ4, refCarrierCodeSubQuery, JoinCondition.And);
			query.AddSubQuery(RefVesselZZSchema.PK, refCarrierVesselPivotSubQuery, JoinCondition.And);
			return query;
		}

		#endregion

		#region AddNumberFilters

		void AddNumbersAndReferencesFilters(ModuleFilterCollection filters)
		{
			if (ApplicableModuleFilter(RefVesselZZSchema.Constants.ZZO_LloydsNumber))
			{
				filters.AddNumberFilter(Descriptions.LloydsNumber, RefVesselZZSchema.ZZO_LloydsNumber).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|RefVesselZZFilter|LloydsNumber", "Lloyds/IMO Number");
			}

			if (ApplicableModuleFilter(RefVesselZZSchema.Constants.ZZO_RadioCallSign))
			{
				filters.AddNumberFilter(Descriptions.RadioCallSign, RefVesselZZSchema.ZZO_RadioCallSign).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|RefVesselZZFilter|RadioCallSign", "Radio Call Sign");
			}

			if (ApplicableModuleFilter(RefVesselZZSchema.Constants.ZZO_Code))
			{
				filters.AddNumberFilter(Descriptions.VesselName, RefVesselZZSchema.ZZO_Code).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|RefVesselZZFilter|VesselName", "Vessel Name");
			}
		}

		#endregion

		#region AddLocationFilters

		void AddLocationFilters(ModuleFilterCollection filters)
		{
			if (ApplicableModuleFilter(RefVesselZZSchema.Constants.ZZO_RN_NKCountryOfReg))
			{
				ModuleNkFilter filter = filters.AddNkFilter(Descriptions.CountryOfRegistration, RefVesselZZSchema.ZZO_RN_NKCountryOfReg, ModuleIDs.RefCountry, new RefCountryCollection(Factory));
				filter.Category = FilterCategories.Locations;
				filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|RefVesselZZFilter|CountryOfRegistration", "Country/Region of Registration");
			}
		}

		#endregion

		#region AddModesAndTypesFilters

		void AddModesAndTypesFilters(ModuleFilterCollection filters)
		{
			if (ApplicableModuleFilter(RefVesselZZSchema.Constants.ZZO_VesselType))
			{
				ModuleFilter filter = filters.AddTextFilter(Descriptions.VesselType, RefVesselZZSchema.ZZO_VesselType, new CodeDescriptionPairList(OLookUpEditType.VesselType));
				filter.Category = FilterCategories.ModesAndTypes;
				filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|RefVesselZZFilter|VesselType", "Vessel Type");
			}
		}

		#endregion
	}
}
