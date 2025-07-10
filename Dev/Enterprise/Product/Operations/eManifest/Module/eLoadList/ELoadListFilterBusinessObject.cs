namespace Enterprise.eManifest.Module
{
	using CargoWise.EntityFramework;
	using CargoWise.Schema;
	using CargoWise.Types;
	using Enterprise.Core;
	using Enterprise.eManifest.Business;
	using Enterprise.Freight.Common.Business;
	using Enterprise.MasterFiles.Business;
	using Enterprise.ZArchitecture.Business;
	using Enterprise.ZArchitecture.Core;
	using Enterprise.ZArchitecture.Modules;
	using Enterprise.ZArchitecture.Schema;

	public class ELoadListFilterBusinessObject : FilterStripBusinessObject
	{
		public static class Descriptions
		{
			#region SuppressResourceStringsCheckRegion

			public const string UniqueReference = "Unique Reference #";
			public const string MasterBill = "Master Bill";
			public const string FlightVoyageNumAndVessel = "Flight/Voyage # and Vessel";
			public const string ETA = "ETA";
			public const string ETD = "ETD";
			public const string ContainerNum = "Container #";
			public const string OriginDepot = "Origin Depot";
			public const string DestinationDepot = "Destination Depot";
			public const string Status = "Status";

			#endregion
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();

			var masterBillFilter = result.AddNumberFilter(Descriptions.MasterBill, GetMasterBillQuery)
				.WithMaxLengthOf<ModuleNumberFilter>(ELoadListSchema.DO_MasterBillNumber);
			masterBillFilter.MultilingualDescription = ResString.GetMultilingualString("eManifest|eLoadListFilterControl|MasterBill", "Master Bill");
			masterBillFilter.Prefix = "M";

			var uniqueReferenceFilter = result.AddNumberFilter(Descriptions.UniqueReference, ELoadListSchema.DO_UniqueReference);
			uniqueReferenceFilter.MultilingualDescription = ResString.GetMultilingualString("eManifest|eLoadListFilterControl|eLoadListID", "eLoadList ID");
			uniqueReferenceFilter.Prefix = "I";

			var containerNumberFilter = result.AddNumberFilter(Descriptions.ContainerNum, ELoadListSchema.DO_ContainerNumber);
			containerNumberFilter.MultilingualDescription = ResString.GetMultilingualString("eManifest|eLoadListFilterControl|ContainerNumber", "Container #");
			containerNumberFilter.Prefix = "C";

			var flightVoyageNoFilter = result.AddTextAndNkFilter(Descriptions.FlightVoyageNumAndVessel, GetFlightVoyageNumberAndVesselQuery, ModuleIDs.RefVessel, BindingLists.RefVessel_List)
				.WithMaxLengthOf<ModuleTextAndNkFilter>(ELoadListSchema.DO_VoyageFlight);
			flightVoyageNoFilter.MultilingualDescription = ResString.GetMultilingualString("eManifest|eLoadListFilterControl|FlightVoyageAndVessel", "Flight/Voyage # and Vessel");
			flightVoyageNoFilter.Prefix = "V";

			etaFilter = result.AddDateFilter(Descriptions.ETA, ELoadListSchema.DO_E_ARV, true);
			etaFilter.MultilingualDescription = ResString.GetMultilingualString("eManifest|eLoadListFilterControl|ETA", "ETA");

			etdFilter = result.AddDateFilter(Descriptions.ETD, ELoadListSchema.DO_E_DEP, true);
			etdFilter.MultilingualDescription = ResString.GetMultilingualString("eManifest|eLoadListFilterControl|ETD", "ETD");

			var originDepotFilter = result.AddGuidFilter(Descriptions.OriginDepot, ModuleIDs.Organisation, OrgAddressSchema.OA_OH, BindingLists.OrgDepot_List);
			originDepotFilter.SubGroup = new OrgAddressSubGroup(ELoadListSchema.DO_OA_OriginDepot);
			originDepotFilter.MultilingualDescription = ResString.GetMultilingualString("eManifest|eLoadListFilterControl|OriginDepot", "Origin Depot");
			originDepotFilter.Prefix = "O";

			var destinationDepotFilter = result.AddGuidFilter(Descriptions.DestinationDepot, ModuleIDs.Organisation, OrgAddressSchema.OA_OH, BindingLists.OrgDepot_List);
			destinationDepotFilter.SubGroup = new OrgAddressSubGroup(ELoadListSchema.DO_OA_DestinationDepot);
			destinationDepotFilter.MultilingualDescription = ResString.GetMultilingualString("eManifest|eLoadListFilterControl|DestinationDepot", "Destination Depot");
			destinationDepotFilter.Prefix = "D";

			var statusFilter = result.AddTextFilter(Descriptions.Status, ELoadListSchema.DO_Status, ELoadListStatusList);
			statusFilter.MultilingualDescription = ResString.GetMultilingualString("eManifest|eLoadListFilterControl|Status", "Status");
			statusFilter.Prefix = "S";

			return result;
		}

		#region MasterBill Query

		ZQuery GetMasterBillQuery(SQLComparisonOperator comparisonOperator, ZString masterBill)
		{
			var result = new ZQuery();

			var alteredMasterBill = masterBill.Replace(" ", "").Replace("-", "");
			result.AddToFilter_PossiblyCommaSeparated(ELoadListSchema.DO_MasterBillNumber, comparisonOperator, masterBill);
			result.AddToFilter(JoinCondition.Or, ELoadListSchema.DO_MasterBillNumber, comparisonOperator, alteredMasterBill);

			return result;
		}

		#endregion

		#region FlightVoyageNumberAndVessel Query

		ZQuery GetFlightVoyageNumberAndVesselQuery(SQLComparisonOperator flightOrVoyageNoComparisonOperator, ZString flightOrVoyageNo, ZString vesselNK)
		{
			var result = new ZQuery();

			if (!vesselNK.IsEmpty || flightOrVoyageNoComparisonOperator == SpecialComparisonOperator.IsBlank || flightOrVoyageNoComparisonOperator == SpecialComparisonOperator.IsNotBlank)
			{
				result.AddToFilter(ELoadListSchema.DO_RV_NKVessel, flightOrVoyageNoComparisonOperator, vesselNK);
			}

			if (!flightOrVoyageNo.IsEmpty || flightOrVoyageNoComparisonOperator == SpecialComparisonOperator.IsBlank || flightOrVoyageNoComparisonOperator == SpecialComparisonOperator.IsNotBlank)
			{
				result.AddToFilter(ELoadListSchema.DO_VoyageFlight, flightOrVoyageNoComparisonOperator, flightOrVoyageNo);
			}

			return result;
		}

		#endregion

		#region Date Queries
		ModuleDateFilter etaFilter;

		ModuleDateFilter etdFilter;

		#endregion

		#region Depots Queries

		class OrgAddressSubGroup : ModuleFilterSubGroup
		{
			readonly SchemaColumn consolAddressForeignKeyColumn;

			public OrgAddressSubGroup(SchemaColumn consolAddressForeignKeyColumn)
			{
				this.consolAddressForeignKeyColumn = consolAddressForeignKeyColumn;
			}

			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var result = new ZDBOnlyQuery(typeof(ELoadList));
				var orgAddressSubQuery = new ZDBOnlySubQuery(typeof(OrgAddress), consolAddressForeignKeyColumn);
				orgAddressSubQuery.AddToFilter(filter);
				result.AddSubQuery(orgAddressSubQuery, JoinCondition.And);

				return result;
			}
		}

		#endregion

		BindToLists BindingLists
		{
			get { return BindToLists.GetCachedLists(Factory); }
		}

		#region ELoadLists Statuses List

		CodeDescriptionPairList ELoadListStatusList
		{
			get
			{
				if (eLoadListStatusList == null)
				{
					eLoadListStatusList = new CodeDescriptionPairList();
					eLoadListStatusList.AddPair(Constants.ELoadListStatuses.Open, Constants.ELoadListStatusDescription.Open);
					eLoadListStatusList.AddPair(Constants.ELoadListStatuses.Lodged, Constants.ELoadListStatusDescription.Lodged);
					eLoadListStatusList.AddPair(Constants.ELoadListStatuses.Consolidated, Constants.ELoadListStatusDescription.Consolidated);
					eLoadListStatusList.AddPair(Constants.ELoadListStatuses.Closed, Constants.ELoadListStatusDescription.Closed);
				}

				return eLoadListStatusList;
			}
		}

		CodeDescriptionPairList eLoadListStatusList;

		#endregion
	}
}
