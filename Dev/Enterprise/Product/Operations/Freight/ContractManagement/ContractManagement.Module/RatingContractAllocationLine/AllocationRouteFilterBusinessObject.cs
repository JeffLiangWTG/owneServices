using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ContractManagement.Business;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ContractManagement.Module
{
	public class AllocationRouteFilterBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();

			ModuleTextFilter allocationIDFilter = filters.AddTextFilter(AllocationRouteFilterConstants.AllocationRouteID, RatingContractAllocationLineSchema.RCA_AllocationLineID);
			allocationIDFilter.MultilingualDescription = ResString.GetMultilingualString("RatingContractAllocationLine|AllocationRouteFilter|AllocationID", "Allocation ID");
			allocationIDFilter.IsExclusiveHelper = true;

			var startDateFilter = filters.AddDateFilter(AllocationRouteFilterConstants.StartDate, RatingContractAllocationLineSchema.RCA_StartDate);
			startDateFilter.MultilingualDescription = ResString.GetMultilingualString("RatingContractAllocationLine|AllocationRouteFilter|StartDate", "Start Date");

			var expiryDateFilter = filters.AddDateFilter(AllocationRouteFilterConstants.ExpiryDate, RatingContractAllocationLineSchema.RCA_ExpiryDate);
			expiryDateFilter.MultilingualDescription = ResString.GetMultilingualString("RatingContractAllocationLine|AllocationRouteFilter|ExpiryDate", "Expiry Date");

			if (FreightConfigurationRegistry.Instance.EnableContainerWeightLimitSupportOnAllocationRoutes.Value)
			{
				var containerWeightLimitFilter =
					new AllocationContainerWeightLimitWithTypeFilter(
						ResString.GetMultilingualString("6524e081-015c-4e4b-9fd9-2d6203a5a64f", "Container Weight Limit"),
						GetContainerWeightLimitQuery,
						ContainerWeightLimitUnit_List);
				filters.AddFilter(containerWeightLimitFilter);
			}

			var bookingLimitFilter = filters.AddFlagFilter(AllocationRouteFilterConstants.HasBookingLimit,
				Res.GetString("d9655a0f-8a32-dc81-4757-18d99c22189a", "Has Booking Limit"),
				RatingContractAllocationLineSchema.RCA_HasBookingLimit, ModuleFilterSubGroup.Default);
			bookingLimitFilter.MultilingualDescription = ResString.GetMultilingualString("RatingContractAllocationLine|AllocationRouteFilter|BookingLimit", "Has Booking Limit");

			var gatewayConsolFilter = filters.AddFlagFilter(AllocationRouteFilterConstants.GatewayConsol,
				Res.GetString("32983b27-0297-7691-4121-4eb7d60af481", "Gateway Consol"),
				RatingContractAllocationLineSchema.RCA_AllowGatewayConsolOnly, ModuleFilterSubGroup.Default);
			gatewayConsolFilter.MultilingualDescription = ResString.GetMultilingualString("RatingContractAllocationLine|AllocationRouteFilter|GatewayConsol", "Gateway Consol");

			var shipperOwnedContainerFilter = filters.AddTextFilter(AllocationRouteFilterConstants.ShipperOwnedContainer, RatingContractAllocationLineSchema.RCA_ContainerOwner, ContainerOwner_List);
			shipperOwnedContainerFilter.MultilingualDescription = ResString.GetMultilingualString("RatingContractAllocationLine|AllocationRouteFilter|Shipper Owned Container", "Shipper Owned Container");

			var groupageContainerModeFilter = filters.AddFlagFilter(AllocationRouteFilterConstants.GroupageContainerMode,
			Res.GetString("fe6d30cd-bf4c-410d-9274-0fe48f367540", "Groupage Container Mode"),
			RatingContractAllocationLineSchema.RCA_AllowGroupageOnly, ModuleFilterSubGroup.Default);
			groupageContainerModeFilter.MultilingualDescription = ResString.GetMultilingualString("RatingContractAllocationLine|AllocationRouteFilter|GroupageContainerMode", "Groupage Container Mode");

			if (FreightConfigurationRegistry.Instance.EnableFreightSpotRateOnAllocationRoutes.Value)
			{
				var freightSpotRateFilter = filters.AddFlagFilter(AllocationRouteFilterConstants.AllowFreightSpotRate,
					Res.GetString("eed4e62e-4b24-5b9d-4350-2f12686441c7", "Allow Freight Spot Rate"),
					RatingContractAllocationLineSchema.RCA_AllowFreightSpotRate, ModuleFilterSubGroup.Default);
				freightSpotRateFilter.MultilingualDescription = ResString.GetMultilingualString(
					"RatingContractAllocationLine|AllocationRouteFilter|AllowFreightSpotRate",
					"Allow Freight Spot Rate");
			}

			var bookingVarianceFilter = filters.AddNumberRangeFilter(AllocationRouteFilterConstants.BookingVariance, RatingContractAllocationLineSchema.RCA_BookingVariance);
			bookingVarianceFilter.MultilingualDescription = ResString.GetMultilingualString("RatingContractAllocationLine|AllocationRouteFilter|BookingVariance", "Booking Variance");

			var allocatedQuantityFilter = filters.AddNumberRangeFilter(AllocationRouteFilterConstants.AllocatedQuantity, RatingContractAllocationLineSchema.RCA_AllocatedQuantity);
			allocatedQuantityFilter.MultilingualDescription = ResString.GetMultilingualString("RatingContractAllocationLine|AllocationRouteFilter|AllocatedQuantity", "Allocated Quantity");

			var relatedNamedAccountsFilter = new NamedAccountsOfContractFilter(AllocationRouteFilterConstants.NamedAccountClients, OrgHeaders, typeof(RatingContractAllocationLine), RatingContractAllocationLineSchema.Constants.Prefix, RatingContractAllocationLineSchema.Constants.PK);
			relatedNamedAccountsFilter.MultilingualDescription = ResString.GetMultilingualString("RatingContractAllocationLine|AllocationRouteFilter|RelatedNamedAccounts", "Named Account Clients");
			filters.AddFilter(relatedNamedAccountsFilter);

			var agentsFilter = new AgentsOfAllocationRouteFilter(AllocationRouteFilterConstants.Agents, OrgHeaders, typeof(RatingContractAllocationLine), RatingContractAllocationLineSchema.Constants.PK);
			agentsFilter.MultilingualDescription = ResString.GetMultilingualString("RatingContractAllocationLine|AllocationRouteFilter|Agents", "Agents");
			filters.AddFilter(agentsFilter);

			var loadAndDischargeFilter = new AllocationRouteCoveringLocationFilter(AllocationRouteFilterConstants.LoadDischargePort, Locations);
			loadAndDischargeFilter.SetItemDescriptions(Res.GetData("RatingContractAllocationLine|AllocationRouteFilter|LoadPort", "Load"), Res.GetData("RatingContractAllocationLine|AllocationRouteFilter|DischargePort", "Discharge"));
			loadAndDischargeFilter.MultilingualDescription = ResString.GetMultilingualString("RatingContractAllocationLine|AllocationRouteFilter|LoadAndDischarge", "Load / Discharge");
			filters.AddFilter(loadAndDischargeFilter);

			if (FreightConfigurationRegistry.Instance.EnablePlaceOfReceiptAndDeliverySupportOnAllocationRoutes.Value)
			{
				var receiptAndDeliveryFilter = new PlaceOfReceiptDeliveryFilter(AllocationRouteFilterConstants.PlaceOfReceiptDelivery, Locations);
				receiptAndDeliveryFilter.SetItemDescriptions(Res.GetData("RatingContractAllocationLine|AllocationRouteFilter|PlaceOfReceipt", "Receipt"), Res.GetData("RatingContractAllocationLine|AllocationRouteFilter|PlaceOfDelivery", "Delivery"));
				receiptAndDeliveryFilter.MultilingualDescription = ResString.GetMultilingualString("RatingContractAllocationLine|AllocationRouteFilter|ReceiptAndDelivery", "Receipt / Delivery");
				filters.AddFilter(receiptAndDeliveryFilter);
			}

			var vesselFilter = new SoftModuleNkFilter(AllocationRouteFilterConstants.Vessel, GetVesselQuery, ModuleIDs.RefVessel, Vessels);
			vesselFilter.MultilingualDescription = ResString.GetMultilingualString("RatingContractAllocationLine|AllocationRouteFilter|Vessel", "Vessel");
			filters.AddFilter(vesselFilter);

			var voyageNumberFilter = filters.AddTextFilter(AllocationRouteFilterConstants.VoyageNumber, GetVoyageNumberQuery);
			voyageNumberFilter.MultilingualDescription = ResString.GetMultilingualString("RatingContractAllocationLine|AllocationRouteFilter|Voyage", "Voyage Number");
			voyageNumberFilter.WithMaxLengthOf<ModuleTextFilter>(RatingContractAllocationLineSchema.RCA_VoyageNumber);

			var containerTypeFilter = filters.AddGuidFilter(AllocationRouteFilterConstants.ContainerType, ModuleIDs.RefContainer, RatingContractAllocationLineSchema.RCA_RC_ContainerType, ContainerType_List);
			containerTypeFilter.MultilingualDescription = ResString.GetMultilingualString("RatingContractAllocationLine|AllocationRouteFilter|ContainerType", "Container Type");

			var storageOrFreightRateClassFilter = filters.AddTextFilter(AllocationRouteFilterConstants.StorageOrFreightRateClass, RatingContractAllocationLineSchema.RCA_StorageOrFreightRateClass, StorageOrFreightRateClass_List);
			storageOrFreightRateClassFilter.MultilingualDescription = ResString.GetMultilingualString("RatingContractAllocationLine|AllocationRouteFilter|StorageOrFreightRateClass", "Storage or Freight Rate Class");

			var serviceStringFilter = filters.AddTextFilter(AllocationRouteFilterConstants.ServiceString, GetServiceStringQuery);
			serviceStringFilter.MultilingualDescription = ResString.GetMultilingualString("RatingContractAllocationLine|AllocationRouteFilter|ServiceString", "Service String");
			serviceStringFilter.WithMaxLengthOf<ModuleTextFilter>(RatingContractAllocationLineSchema.RCA_ServiceLoop);

			var allowRelatedUNLOCOsFilter = filters.AddFlagFilter(AllocationRouteFilterConstants.AllowRelatedUNLOCOs,
				Res.GetString("538f009c-16c3-2596-4ff5-79a5480043ae", "Allow Related UNLOCOs"),
				RatingContractAllocationLineSchema.RCA_AllowRelatedPorts, ModuleFilterSubGroup.Default);
			allowRelatedUNLOCOsFilter.MultilingualDescription = ResString.GetMultilingualString("RatingContractAllocationLine|AllocationRouteFilter|AllowRelatedUNLOCOs", "Allow Related UNLOCOs");

			var tradeLaneFilter = filters.AddGuidFilter(AllocationRouteFilterConstants.TradeLane, ModuleIDs.TradeLane, RatingContractAllocationLineSchema.RCA_EJ_TradeLane, TradeLanes);
			tradeLaneFilter.MultilingualDescription = ResString.GetMultilingualString("RatingContractAllocationLine|AllocationRouteFilter|TradeLane", "Trade Lane");

			if (FreightConfigurationRegistry.Instance.EnablePrioritySupportOnAllocationRoutes.Value)
			{
				var priorityFilter = filters.AddNumberRangeFilter(AllocationRouteFilterConstants.Priority, RatingContractAllocationLineSchema.RCA_Priority);
				priorityFilter.MultilingualDescription = ResString.GetMultilingualString("RatingContractAllocationLine|AllocationRouteFilter|Priority", "Priority");
			}

			return filters;
		}

		ZQuery GetVesselQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var query = new ZDBOnlyQuery(typeof(RatingContractAllocationLine));
			query.AddToFilter(RatingContractAllocationLineSchema.RCA_RV_NKVessel, comparisonOperator, value);

			if (comparisonOperator == SQLComparisonOperator.IsBlank)
			{
				return query;
			}

			var voyageSubQuery = new ZDBOnlySubQuery(typeof(JobVoyage), JobVoyOriginSchema.JA_JV);
			voyageSubQuery.AddToFilter(JobVoyageSchema.JV_RV_NKVessel, comparisonOperator, value);

			var originSubQuery = new ZDBOnlySubQuery(typeof(VoyageOrigin), JobSailingSchema.JX_JA);
			originSubQuery.AddSubQuery(voyageSubQuery, JoinCondition.And);

			var jobSailingSubQuery = new ZDBOnlySubQuery(typeof(JobSailing), RatingContractAllocationLineSchema.RCA_JX_SailingSchedule);
			jobSailingSubQuery.AddSubQuery(originSubQuery, JoinCondition.And);

			query.AddSubQuery(jobSailingSubQuery, JoinCondition.Or);

			return query;
		}

		ZQuery GetServiceStringQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var query = new ZDBOnlyQuery(typeof(RatingContractAllocationLine));
			query.AddToFilter(RatingContractAllocationLineSchema.RCA_ServiceLoop, comparisonOperator, value);

			if (comparisonOperator == SQLComparisonOperator.IsBlank)
			{
				return query;
			}

			var jobSailingSubQuery = new ZDBOnlySubQuery(typeof(JobSailing), RatingContractAllocationLineSchema.RCA_JX_SailingSchedule);
			jobSailingSubQuery.AddToFilter(JobSailingSchema.JX_ServiceString, comparisonOperator, value);
			query.AddSubQuery(jobSailingSubQuery, JoinCondition.Or);

			return query;
		}

		ZQuery GetVoyageNumberQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var query = new ZDBOnlyQuery(typeof(RatingContractAllocationLine));
			query.AddToFilter(RatingContractAllocationLineSchema.RCA_VoyageNumber, comparisonOperator, value);

			if (comparisonOperator == SQLComparisonOperator.IsBlank)
			{
				return query;
			}

			var voyageSubQuery = new ZDBOnlySubQuery(typeof(JobVoyage), JobVoyOriginSchema.JA_JV);
			voyageSubQuery.AddToFilter(JobVoyageSchema.JV_VoyageFlight, comparisonOperator, value);

			var voyageOriginSubQuery = new ZDBOnlySubQuery(typeof(VoyageOrigin), JobSailingSchema.JX_JA);
			voyageOriginSubQuery.AddSubQuery(voyageSubQuery, JoinCondition.And);

			var jobSailingSubQuery = new ZDBOnlySubQuery(typeof(JobSailing), RatingContractAllocationLineSchema.RCA_JX_SailingSchedule);
			jobSailingSubQuery.AddSubQuery(voyageOriginSubQuery, JoinCondition.And);

			query.AddSubQuery(jobSailingSubQuery, JoinCondition.Or);

			return query;
		}

		protected static ZQuery GetContainerWeightLimitQuery(INumericZType value1, INumericZType value2, ZString unit)
		{
			var result = new ZDBOnlyQuery(typeof(RatingContractAllocationLine));
			var unitColumnName = RatingContractAllocationLineSchema.RCA_ContainerWeightLimitUQ.Name;
			var weightColumnName = RatingContractAllocationLineSchema.RCA_ContainerWeightLimit.Name;

			var sqlFilter = string.Format(CultureInfo.InvariantCulture, @" 
						{1} IN
						(
							SELECT
								{1}
							FROM 
								{0}
								CROSS APPLY dbo.ConvertWeight({2}, {3}, '{4}') AS ConvertedTotal
							WHERE ConvertedTotal.Value BETWEEN {5} AND {6}							
						)
						", RatingContractAllocationLineSchema.Constants.TableName, RatingContractAllocationLineSchema.PK.Name, weightColumnName, unitColumnName,
				unit, value1.ToString(), value2.ToString());

			var sqlFilterParameters = new ZSqlParameterCollection();
			result.AddFilterAndZSQLParameterCollection(sqlFilter, sqlFilterParameters);
			return result;
		}

		RefVesselCollection Vessels => vessels ??= new RefVesselCollection(Factory);
		RefVesselCollection vessels;

		LocationCollection Locations => locations ??= new LocationCollection(Factory);
		LocationCollection locations;

		OrgHeaderCollection OrgHeaders => orgHeaders ??= new OrgHeaderCollection(Factory);
		OrgHeaderCollection orgHeaders;

		JobTradeLaneCollection TradeLanes => tradeLanes ??= new JobTradeLaneCollection(Factory);
		JobTradeLaneCollection tradeLanes;

		#region ContainerType_List

		RefContainerCollection ContainerType_List => fContainerType_List ??= new RefContainerCollection(Factory, RefContainerLookups.ShippingModes.Sea);
		RefContainerCollection fContainerType_List;

		#endregion

		#region Freight Classes

		public ContainerFreightRateClassList FreightRateClassList
		{
			get
			{
				return new ContainerFreightRateClassList();
			}
		}

		#endregion

		#region StorageOrFreightRateClass_List

		CodeDescriptionPairList StorageOrFreightRateClass_List
		{
			get
			{
				var storageOrFreightRateClass_List = new CodeDescriptionPairList(OLookUpEditType.ContainerStorageClass);
				storageOrFreightRateClass_List.AddRange(FreightRateClassList);
				return storageOrFreightRateClass_List;
			}
		}

		#endregion

		#region ContainerOwner_List

		CodeDescriptionPairList ContainerOwner_List
		{
			get
			{
				if (containerOwner_List == null)
				{
					containerOwner_List = new CodeDescriptionPairList();
					containerOwner_List.AddPair(Constants.ContainerOwnership.Codes.ShipperOwned, Constants.ContainerOwnership.Descriptions.ShipperOwned);
					containerOwner_List.AddPair(Constants.ContainerOwnership.Codes.CarrierOwned, Constants.ContainerOwnership.Descriptions.CarrierOwned);
				}
				return containerOwner_List;
			}
		}
		CodeDescriptionPairList containerOwner_List;

		#endregion

		#region ContainerWeightLimitUnit_List

		CodeDescriptionPairList ContainerWeightLimitUnit_List
		{
			get
			{
				if (containerWeightLimitUnit_List == null)
				{
					containerWeightLimitUnit_List = Factory.GetCachedValue("RatingContractAllocationLine|WeightUnits", () => new CodeDescriptionPairList(OLookUpEditType.Weight));
				}
				return containerWeightLimitUnit_List;
			}
		}

		CodeDescriptionPairList containerWeightLimitUnit_List;

		#endregion
	}
}
