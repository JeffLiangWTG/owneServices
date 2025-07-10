using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Module
{
	public class JobSeaVoyageFilterStrip : FilterStripBusinessObject
	{
		#region Descriptions

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter related")]
		public static class Descriptions
		{
			public const string VoyageVessel = "Voyage / Vessel";
			public const string Carrier = "Carrier";
			public const string TradeLane = "TradeLane";
		}

		#endregion

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection collection = new ModuleFilterCollection();
			AddVoyageVesselFilters(collection);
			AddOrganisationsAndStaffFilters(collection);
			AddLocationFilters(collection);
			return collection;
		}

		public override ZQuery Filter
		{
			get
			{
				ZQuery result = new ZQuery();
				result.AddToFilter(base.Filter);
				result.AddToFilter(JobVoyageSchema.JV_AirSeaRoad, Constants.TransportModes.Sea);
				return result;
			}
		}

		#region AddTextFilters

		void AddVoyageVesselFilters(ModuleFilterCollection filters)
		{
			var voyageVesselFilter = new VoyageVesselModuleFilter(Descriptions.VoyageVessel, GetVoyageVesselFilter, new RefVesselCollection(Factory))
				.WithMaxLengthOf(JobVoyageSchema.JV_VoyageFlight, JobVoyageSchema.JV_RV_NKVessel);
			voyageVesselFilter.MultilingualDescription = ResString.GetMultilingualString("d9453e3a-e74e-4f73-bb26-d88a70bfd42a", "Voyage / Vessel");
			voyageVesselFilter.Category = FilterCategories.NumbersAndReferences;

			filters.AddCustomFilter(voyageVesselFilter);
		}

		ZQuery GetVoyageVesselFilter(SQLComparisonOperator opp, ZString voyageNumber, ZString vessel, ZBool includeArchived)
		{
			return VoyageVesselModuleFilterHelper.GetBasicVoyageVesselQuery(opp, voyageNumber, vessel, false, JobVoyageSchema.JV_VoyageFlight, JobVoyageSchema.JV_RV_NKVessel);
		}

		#endregion

		#region AddOrganisationsAndStaffFilters

		void AddOrganisationsAndStaffFilters(ModuleFilterCollection collection)
		{
			var filter = collection.AddGuidFilter(Descriptions.Carrier, ModuleIDs.Organisation, JobVoyageSchema.JV_OH_Line, new ShipsAgencyPrincipalCollectionWithSecurityCheck(Factory));
			filter.MultilingualDescription = ResString.GetMultilingualString("3663530b-b30d-40b7-86b5-e193f5efd0a0", "Carrier");
			filter.Category = FilterCategories.Organisations;
		}

		#endregion

		#region AddLocationFilters

		void AddLocationFilters(ModuleFilterCollection filters)
		{
			var tradeLaneFilter = filters.AddGuidFilter(Descriptions.TradeLane, ModuleIDs.TradeLane, JobTradeLaneVoyageSchema.NB_EJ, new JobTradeLaneCollection(Factory));
			tradeLaneFilter.SubGroup = new TradeLaneSubGroup();
			tradeLaneFilter.MultilingualDescription = ResString.GetMultilingualString("438008b0-e5c9-4c4f-bcb3-f3535497484a", "Trade Lane");
			tradeLaneFilter.Category = FilterCategories.Locations;
		}

		class TradeLaneSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var result = new ZDBOnlyQuery(typeof(JobVoyage));

				var jobTradeLaneVoyageSubQuery = new ZDBOnlySubQuery(typeof(JobTradeLaneVoyage), JobTradeLaneVoyageSchema.NB_JV);
				jobTradeLaneVoyageSubQuery.AddToFilter(filter);
				result.AddSubQuery(jobTradeLaneVoyageSubQuery, JoinCondition.And);

				return result;
			}
		}

		#endregion
	}
}
