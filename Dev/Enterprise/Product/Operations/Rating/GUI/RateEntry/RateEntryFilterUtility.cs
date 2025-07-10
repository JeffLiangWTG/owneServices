using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.GUI
{
	public static class RateEntryFilterUtility
	{
		#region SuppressResourceStringsCheckRegion

		public static class Constants
		{
			public static class Codes
			{
				public const string CGReference = "CG Reference";
				public const string IncludeTACTRates = "Include TACT Rates";
				public const string OriginIATARegion = "Origin IATA Region";
				public const string DestinationIATARegion = "Destination IATA Region";
				public const string ViaIATARegion = "Via IATA Region";
				public const string ShowExpired = "Show Expired";
				public const string ShowPublished = "Show Published";
				public const string ControllingCustomer = "Controlling Customer";
				public const string Consignee = "Consignee";
				public const string Consignor = "Consignor";
				public const string CarrierTransportProvider = "Carrier / Transport Provider";
				public const string ServiceProvider = "Service Provider";
				public const string StartDate = "Start Date";
				public const string EndDate = "End Date";
				public const string EffectiveOn = "Effective On";
				public const string OriginDestination = "Origin / Destination";
				public const string FromLocationType = "From Location Type";
				public const string ToLocationType = "To Location Type";
				public const string Via = "Via";
				public const string FirstLoad = "FirstLoad";
				public const string LastDischarge = "LastDischarge";
				public const string FirstRouteSetLoad = "FirstRouteSetLoad";
				public const string LastRouteSetDischarge = "LastRouteSetDischarge";
				public const string CommodityCode = "Commodity Code";
				public const string FMCTariffID = "FMC Tariff ID";
				public const string ContractNumberLinked = "Contract Number Linked";
				public const string CarrierContractNumber = "Carrier Contract Number";
				public const string ClientContractNumber = "Client Contract Number";
				public const string CarrierContract = "Carrier Contract";
				public const string SCACCode = "SCAC Code";
				public const string IATACode = "IATA Code";
				public const string C1Code = "C1 Code";
				public const string TransportMode = "Transport Mode";
				public const string ContainerMode = "Container Mode";
				public const string CrossTrade = "Cross Trade";
				public const string TransitTime = "Transit Time";
				public const string ProductWarehouse = "Product Warehouse";
				public const string TransitWarehouse = "Transit Warehouse";
				public const string ContainerType = "Container Type";
				public const string ServiceLevel = "Service Level";
				public const string CarrierServiceLevel = "Carrier Service Level";
				public const string GatewayServiceLevel = "Gateway Service Level";
				public const string ShipmentGatewayServiceLevel = "Shipment Gateway Service Level";
				public const string FromToZone = "From / To Zone";
				public const string FromToSuburb = "From / To Suburb";
				public const string FromPostcode = "From Postcode";
				public const string ToPostcode = "To Postcode";
				public const string FromToOrganization = "From / To Organization";
				public const string FromLocationDescription = "From Location Description";
				public const string ToLocationDescription = "To Location Description";
				public const string NamedAccount = "Named Account";
				public const string Publisher = "Publisher";
				public const string PaymentTerm = "Payment Term";
				public const string UniversalCommodityGroup = "Universal Commodity Group";
				public const string AircraftType = "Aircraft Type";
				public const string GatewayAgentType = "Gateway Agent Type";
				public const string Currency = "Currency - Rate Entry";
				public const string ShipmentConsolidationStatus = "Shipment Consolidation Status";
				public const string HBLDeliveryMode = "HBL Delivery Mode";
				public const string IsNonOperatingReefer = "Is Non-Operating Reefer";
				public const string ShowAllRateLines = "Show All Rate Lines";
			}

			public static class Description
			{
				public static readonly ResourceString IncludeTACTRates = ResString.GetMultilingualString("124DEE2E-BCB7-4584-A445-A825583E0181", "Include TACT Rates");
				public static readonly ResourceString OriginIATARegion = ResString.GetMultilingualString("EDB21F98-04A4-4809-BACE-49CA99EFCFB8", "Origin IATA Region");
				public static readonly ResourceString DestinationIATARegion = ResString.GetMultilingualString("5AE49AD4-58F0-4C6F-BE85-1D9BD7F65B56", "Destination IATA Region");
				public static readonly ResourceString ViaIATARegion = ResString.GetMultilingualString("4CADE434-C904-4FB5-97BC-2A7407D4FC99", "Via IATA Region");
				public static readonly ResourceString ShowExpired = ResString.GetMultilingualString("A6014484-414E-4E09-AB66-BD94611E3E38", "Show Expired - Rate Entry");
				public static readonly ResourceString ShowPublished = ResString.GetMultilingualString("fbf4d4e7-f58c-4198-843f-7e5ac8d4e860", "Show Published");
				public static readonly ResourceString ControllingCustomer = ResString.GetMultilingualString("c09fed0a-d031-4c4a-b12d-82960b22f88d", "Controlling Customer");
				public static readonly ResourceString Consignee = ResString.GetMultilingualString("74a7b987-8276-4af4-98ce-6f5170d85ae0", "Consignee");
				public static readonly ResourceString Consignor = ResString.GetMultilingualString("2b6a6926-cad3-4ae3-ad54-95169e03c891", "Consignor");
				public static readonly ResourceString CarrierTransportProvider = ResString.GetMultilingualString("a81f8489-14f9-4ca8-923b-0069eb258a35", "Carrier / Transport Provider");
				public static readonly ResourceString ServiceProvider = ResString.GetMultilingualString("020EB26B-8531-4C8B-BD02-F15F873B1E6B", "Service Provider");
				public static readonly ResourceString StartDate = ResString.GetMultilingualString("714a8c31-e38d-48ef-ba9a-ee7ac4802811", "Start Date - Rate Entry");
				public static readonly ResourceString EndDate = ResString.GetMultilingualString("ae9bceb3-c7b8-46fb-b60d-8fce11bc4e8b", "Expiry Date - Rate Entry");
				public static readonly ResourceString EffectiveOn = ResString.GetMultilingualString("a1669edb-164b-4bf7-9ddc-9de3b3770d12", "Effective On - Rate Entry");
				public static readonly ResourceString OriginDestination = ResString.GetMultilingualString("b1c36246-7952-470b-b2fe-dd6039297ea2", "Origin / Destination");
				public static readonly ResourceString FromLocationType = ResString.GetMultilingualString("1197f9b4-9b20-4c8c-966f-4a84acb21fd7", "From Location Type");
				public static readonly ResourceString ToLocationType = ResString.GetMultilingualString("1abe3d27-2d7d-4751-b101-857fc3dfe4c2", "To Location Type");
				public static readonly ResourceString Via = ResString.GetMultilingualString("c3f059ce-8019-4282-b20e-94853aa54a5a", "Via");
				public static readonly ResourceString FirstLoad = ResString.GetMultilingualString("EEFC037C-855B-444A-AF33-12FE5A635122", "First Load");
				public static readonly ResourceString LastDischarge = ResString.GetMultilingualString("75FFCB7B-1266-49F8-8504-296EC12DE990", "Last Discharge");
				public static readonly ResourceString FirstRouteSetLoad = ResString.GetMultilingualString("279C4500-3592-4B80-B800-2C2939A0EC64", "First Mode & Route Set Load");
				public static readonly ResourceString LastRouteSetDischarge = ResString.GetMultilingualString("B33F91AD-26E4-4C78-8764-C04009A8B7D3", "Last Mode & Route Set Discharge");
				public static readonly ResourceString CommodityCode = ResString.GetMultilingualString("b666e4fe-90fa-4bc0-8348-60bdce40bb53", "Commodity Code");
				public static readonly ResourceString FMCTariffID = ResString.GetMultilingualString("e9b0e1e0-f0ac-4d47-ae7f-691ddac89ffd", "FMC Tariff ID");
				public static readonly ResourceString ContractNumberLinked = ResString.GetMultilingualString("eded2ead-fef5-4045-9c91-1bf9d49ae373", "Contract Number Linked");
				public static readonly ResourceString CarrierContractNumber = ResString.GetMultilingualString("1ead4602-4f33-4a45-98f3-1f4f4ff527ee", "Carrier Contract Number");
				public static readonly ResourceString ClientContractNumber = ResString.GetMultilingualString("1d8f836f-7f69-4f4f-8ce0-a45035780b1f", "Client Contract Number");
				public static readonly ResourceString CarrierContract = ResString.GetMultilingualString("C8C684D1-DE88-4747-9A68-079FA22A57BF", "Carrier Contract");
				public static readonly ResourceString SCACCode = ResString.GetMultilingualString("cdcff92b-e904-4fc7-9bf9-299c71b0aecc", "SCAC Code");
				public static readonly ResourceString IATACode = ResString.GetMultilingualString("6861c4d3-8f53-4467-be90-acbec8a9d229", "IATA Code");
				public static readonly ResourceString C1Code = ResString.GetMultilingualString("ec39336b-47b9-4013-a062-acefaf1a950c", "C1 Code");
				public static readonly ResourceString TransportMode = ResString.GetMultilingualString("053b9866-70b4-432b-8d70-4719ab505372", "Transport Mode");
				public static readonly ResourceString CGReference = ResString.GetMultilingualString("751582fa-4f2d-4e5e-941c-00be90d362a9", "CG Reference");
				public static readonly ResourceString CrossTrade = ResString.GetMultilingualString("1d78459a-e94b-4d23-9d7a-5cf9ecb39319", "Cross Trade");
				public static readonly ResourceString TransitTime = ResString.GetMultilingualString("f9025ee7-f064-491f-bbde-8947af5180aa", "Transit Time");
				public static readonly ResourceString ProductWarehouse = ResString.GetMultilingualString("543320aa-8609-41d2-8b8d-7421370ff3e9", "Product Warehouse");
				public static readonly ResourceString TransitWarehouse = ResString.GetMultilingualString("35e8cdc2-6c58-44b4-9e3a-3addcd7baaa8", "Transit Warehouse");
				public static readonly ResourceString ContainerType = ResString.GetMultilingualString("de18fa34-976d-48c5-9587-713b83ac7ed4", "Container Type");
				public static readonly ResourceString ContainerMode = ResString.GetMultilingualString("0724af6b-ab69-43ba-884c-9547ce0a76e5", "Container Mode");
				public static readonly ResourceString ServiceLevel = ResString.GetMultilingualString("69f2e9eb-9ee0-4dd2-901d-b55a4ddfec68", "Service Level");
				public static readonly ResourceString CarrierServiceLevel = ResString.GetMultilingualString("4ffaabf8-da16-4815-affe-5826ca324859", "Carrier Service Level");
				public static readonly ResourceString GatewayServiceLevel = ResString.GetMultilingualString("EA7892A4-ABA7-4AE0-9C62-2512891C04FD", "Gateway Service Level");
				public static readonly ResourceString ShipmentGatewayServiceLevel = ResString.GetMultilingualString("3ABFCE41-5BA1-4916-BA27-E9BBC8A11D9F", "Shipment Gateway Service Level");
				public static readonly ResourceString FromToZone = ResString.GetMultilingualString("bcb10881-688d-46c2-9fe0-6c0f5ea60f83", "From / To Zone");
				public static readonly ResourceString FromToSuburb = ResString.GetMultilingualString("e9100081-474d-4427-867c-d9270756ad12", "From / To Suburb");
				public static readonly ResourceString FromPostcode = ResString.GetMultilingualString("83c8c742-4cba-489e-a6f1-d732bfa03e4f", "From Postcode");
				public static readonly ResourceString ToPostcode = ResString.GetMultilingualString("ce2f035e-ca5a-4b55-9705-24ae525a86f7", "To Postcode");
				public static readonly ResourceString FromToOrganization = ResString.GetMultilingualString("566c3245-9565-45b8-98ae-2156b241af95", "From / To Organization");
				public static readonly ResourceString FromLocationDescription = ResString.GetMultilingualString("f2ad40ab-e4e4-4b4f-bf9f-d29599f63d1b", "From Location Description");
				public static readonly ResourceString ToLocationDescription = ResString.GetMultilingualString("187de0b1-ca44-483d-a73a-327d4cda91f1", "To Location Description");
				public static readonly ResourceString NamedAccount = ResString.GetMultilingualString("e22a22df-064b-434e-a924-e452695a8864", "Named Account");
				public static readonly ResourceString Publisher = ResString.GetMultilingualString("d8197331-7da9-4d5e-9684-3798762dbeb5", "Publisher");
				public static readonly ResourceString PaymentTerm = ResString.GetMultilingualString("bdfe1216-70c0-46d6-b4d5-0f02dfa0df0b", "Payment Terms");
				public static readonly ResourceString UniversalCommodityGroup = ResString.GetMultilingualString("5e36a0f5-6734-4509-8cd3-97e0008103b1", "Universal Commodity Group");
				public static readonly ResourceString AircraftType = ResString.GetMultilingualString("b4b87ea0-a1cb-4e08-a2bf-b88c9c1ac3ce", "Aircraft Type");
				public static readonly ResourceString GatewayAgentType = ResString.GetMultilingualString("b54e4bf7-9aef-403d-a760-919f9fd6faf0", "Gateway Agent Type");
				public static readonly ResourceString Currency = ResString.GetMultilingualString("1a3b9798-4762-4a08-b3ae-f993e31d777c", "Currency - Rate Entry");
				public static readonly ResourceString ShipmentConsolidationStatus = ResString.GetMultilingualString("BE041223-CEFB-40EF-BB6F-853F125B4C08", "Shipment Consolidation Status");
				public static readonly ResourceString HBLDeliveryMode = ResString.GetMultilingualString("FAC1BC48-2D91-4101-A01A-4F96361EE80F", "HBL Delivery Mode");
				public static readonly ResourceString IsNonOperatingReefer = ResString.GetMultilingualString("JENF837D-22F1-4101-M932-HWNF77FWF923", "Is Non-Operating Reefer");
				public static readonly ResourceString ShowAllRateLines = ResString.GetMultilingualString("3799215a-2423-45a6-9168-366a455d1c8a", "Show All Rate Lines");
			}
		}

		public static class TransportModes
		{
			public const string SEF = nameof(SEF);
			public const string AIF = nameof(AIF);
			public const string ROF = nameof(ROF);
			public const string RAF = nameof(RAF);
		}

		#endregion
		public static ModuleDateFilter AddStartDateFilter(ModuleFilterCollection filterCollection)
		{
			var filter = filterCollection.AddDateFilter(Constants.Codes.StartDate, RateEntrySchema.TI_RateStartDate);
			filter.MultilingualDescription = Constants.Description.StartDate;
			filter.Category = FilterCategories.Dates;
			return filter;
		}

		public static ModuleDateFilter AddEndDateFilter(ModuleFilterCollection filterCollection)
		{
			var filter = filterCollection.AddDateFilter(Constants.Codes.EndDate, RateEntrySchema.TI_RateEndDate);
			filter.MultilingualDescription = Constants.Description.EndDate;
			filter.Category = FilterCategories.Dates;
			return filter;
		}

		public static ModuleSingleDateFilter AddEffectiveOnFilter(ModuleFilterCollection filterCollection)
		{
			var filter =
				filterCollection
				.AddSingleDateFilter(
					Constants.Codes.EffectiveOn,
					(value) =>
					{
						var startDateQuery = new ZQuery();
						startDateQuery.AddToFilter(JoinCondition.Or, RateEntrySchema.TI_RateStartDate, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, value);

						var endDateQuery = new ZQuery(RateEntrySchema.TI_RateEndDate, null);
						endDateQuery.AddToFilter(JoinCondition.Or, RateEntrySchema.TI_RateEndDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, value);

						return new ZQuery(startDateQuery, JoinCondition.And, endDateQuery);
					});

			filter.MultilingualDescription = Constants.Description.EffectiveOn;
			filter.Category = FilterCategories.Dates;

			return filter;
		}

		public static ModuleLocationFilter AddOriginDestinationFilter(ModuleFilterCollection filterCollection, IBusinessObjectCollection locationsList)
		{
			var filter = filterCollection.AddLocationFilter(Constants.Codes.OriginDestination, RateEntrySchema.TI_OriginLRC, locationsList, RateEntrySchema.TI_DestinationLRC, locationsList, true);
			filter.Category = FilterCategories.Locations;
			filter.MultilingualDescription = Constants.Description.OriginDestination;
			filter.SetItemDescriptions(Res.GetData("1cfd8d8e-3114-4f51-8f26-19ab406ba792", "Origin"), Res.GetData("d92a0d70-b307-4155-8849-566e36d16629", "Destination"));
			return filter;
		}

		public static ModuleNkFilter AddViaFilter(ModuleFilterCollection filterCollection, IBusinessObjectCollection locationsList)
		{
			var filter = filterCollection.AddNkFilter(Constants.Codes.Via, RateEntrySchema.TI_ViaLRC, ModuleIDs.Location, locationsList);
			filter.MultilingualDescription = Constants.Description.Via;
			filter.Category = FilterCategories.Locations;
			filter.SupportsFiltersMatchComparisonOperator = false; // Can't determine which module to show since it can be RefUNLOCO, RefCountry, or RefZoneHeader.
			return filter;
		}

		public static ModuleNkFilter AddFirstLoadFilter(ModuleFilterCollection filterCollection, IBusinessObjectCollection locationList)
		{
			var filter = new ModuleSingleLocationFilter(
				Constants.Codes.FirstLoad,
				Constants.Description.FirstLoad,
				RateEntrySchema.TI_FirstLoadLRC,
				locationList);

			filterCollection.AddFilter(filter);

			return filter;
		}

		public static ModuleNkFilter AddFirstLoadFilter(ModuleFilterCollection filterCollection, GetNkQuery query, IBusinessObjectCollection locationList)
		{
			var filter = new ModuleSingleLocationFilter(
				Constants.Codes.FirstLoad,
				Constants.Description.FirstLoad,
				query,
				locationList);

			filterCollection.AddFilter(filter);

			return filter;
		}

		public static ModuleNkFilter AddLastDischargeFilter(ModuleFilterCollection filterCollection, IBusinessObjectCollection locationList)
		{
			var filter = new ModuleSingleLocationFilter(
				Constants.Codes.LastDischarge,
				Constants.Description.LastDischarge,
				RateEntrySchema.TI_LastDischargeLRC,
				locationList);

			filterCollection.AddFilter(filter);

			return filter;
		}

		public static ModuleNkFilter AddLastDischargeFilter(ModuleFilterCollection filterCollection, GetNkQuery query, IBusinessObjectCollection locationList)
		{
			var filter = new ModuleSingleLocationFilter(
				Constants.Codes.LastDischarge,
				Constants.Description.LastDischarge,
				query,
				locationList);

			filterCollection.AddFilter(filter);

			return filter;
		}

		public static ModuleNkFilter AddFirstRouteSetLoadFilter(ModuleFilterCollection filterCollection, IBusinessObjectCollection locationList)
		{
			var filter = new ModuleSingleLocationFilter(
				Constants.Codes.FirstRouteSetLoad,
				Constants.Description.FirstRouteSetLoad,
				RateEntrySchema.TI_FirstRouteSetLoadPortLRC,
				locationList);

			filterCollection.AddFilter(filter);

			return filter;
		}

		public static ModuleNkFilter AddFirstRouteSetLoadFilter(ModuleFilterCollection filterCollection, GetNkQuery query, IBusinessObjectCollection locationList)
		{
			var filter = new ModuleSingleLocationFilter(
				Constants.Codes.FirstRouteSetLoad,
				Constants.Description.FirstRouteSetLoad,
				query,
				locationList);

			filterCollection.AddFilter(filter);

			return filter;
		}

		public static ModuleNkFilter AddLastRouteSetDischargeFilter(ModuleFilterCollection filterCollection, IBusinessObjectCollection locationList)
		{
			var filter = new ModuleSingleLocationFilter(
				Constants.Codes.LastRouteSetDischarge,
				Constants.Description.LastRouteSetDischarge,
				RateEntrySchema.TI_LastRouteSetDischargePortLRC,
				locationList);

			filterCollection.AddFilter(filter);

			return filter;
		}

		public static ModuleNkFilter AddLastRouteSetDischargeFilter(ModuleFilterCollection filterCollection, GetNkQuery query, IBusinessObjectCollection locationList)
		{
			var filter = new ModuleSingleLocationFilter(
				Constants.Codes.LastRouteSetDischarge,
				Constants.Description.LastRouteSetDischarge,
				query,
				locationList);

			filterCollection.AddFilter(filter);

			return filter;
		}

		public static ModuleFlagsFilter AddCrossTradeFilter(ModuleFilterCollection filterCollection)
		{
			var filter =
				filterCollection
				.AddFlagsFilter
					(
						Constants.Codes.CrossTrade
						, new string[] { Constants.Description.CrossTrade }
						, new GetFlagsQuery[]
							{
								(isCrossTrade) =>
								{
									const string sql = @"
TI_IsCrossTrade = {0}
{1}
(
	(
		ISNULL(TI_OriginLRC, '') = '' AND ISNULL(TI_DestinationLRC, '') = ''
	)
	OR
	(
		LEN(TI_OriginLRC) IN (2, 5) AND LEFT(TI_OriginLRC, 2) = '{2}'
	)
	OR
	(
		LEN(TI_DestinationLRC) IN (2, 5) AND LEFT(TI_DestinationLRC, 2) = '{2}'
	)
	OR
	(
		LEN(TI_OriginLRC) = 4 AND '{2}' IN
		(
			SELECT DISTINCT COALESCE(RL_RN_NKCountryCode, RN_Code)
			FROM dbo.RefZonePivot
			LEFT JOIN dbo.RefZoneHeader ON F2_FZ = FZ_PK
			LEFT JOIN dbo.RefUNLOCO ON F2_ParentTableCode = 'RL' AND F2_ParentID = RL_PK
			LEFT JOIN dbo.RefCountry ON F2_ParentTableCode = 'RN' AND F2_ParentID = RN_PK
			WHERE FZ_Code = TI_OriginLRC
		)
	)
	OR
	(
		LEN(TI_DestinationLRC) = 4 AND '{2}' IN
		(
			SELECT DISTINCT COALESCE(RL_RN_NKCountryCode, RN_Code)
			FROM dbo.RefZonePivot
			LEFT JOIN dbo.RefZoneHeader ON F2_FZ = FZ_PK
			LEFT JOIN dbo.RefUNLOCO ON F2_ParentTableCode = 'RL' AND F2_ParentID = RL_PK
			LEFT JOIN dbo.RefCountry ON F2_ParentTableCode = 'RN' AND F2_ParentID = RN_PK
			WHERE FZ_Code = TI_DestinationLRC
		)
	)
)";
									var sqlFormatted = string.Format(sql, isCrossTrade ? 1 : 0, isCrossTrade ? (NoResString)"OR NOT" : "AND", GlbCompany.CurrentCompany.GC_RN_NKCountryCode); // SQL
									return new ZDBOnlyQuery(typeof(RateEntry)).AddFilterAndZSQLParameterCollection(sqlFormatted, new ZSqlParameterCollection());
								}
							});

			filter.MultilingualDescription = Constants.Description.CrossTrade;
			filter.Category = FilterCategories.Locations;

			return filter;
		}

		public static ModuleTextFilter AddTransitTimeFilter(ModuleFilterCollection filterCollection)
		{
			//TODO: FR hint , I used another overload, not to use query delegate and use schema column instead.
			var filter =
				filterCollection
				.AddTextFilter(
					Constants.Codes.TransitTime
					, RateEntrySchema.TI_TransitTime
					, RateEntryLookups.GetTransitTimeList(true));

			filter.MultilingualDescription = Constants.Description.TransitTime;
			filter.Category = FilterCategories.Locations;

			return filter;
		}

		public static ModuleGuidsFilter AddFromToSuburbFilter(ModuleFilterCollection filterCollection, IBusinessObjectCollection fromCollection, IBusinessObjectCollection toCollection)
		{
			var filter = filterCollection.AddGuidFilter
			(
				Constants.Codes.FromToSuburb,
				ModuleIDs.RefCityTown,
				(fromZonePK, toZonePK) =>
				{
					var result = new ZQuery();

					if (fromZonePK.IsValid)
					{
						var fromZoneSubQuery = new ZQuery(RateEntrySchema.TI_R9_FromSuburb, fromZonePK);
						result.AddToFilter(fromZoneSubQuery);
					}

					if (toZonePK.IsValid)
					{
						var toZoneSubQuery = new ZQuery(RateEntrySchema.TI_R9_ToSuburb, toZonePK);
						result.AddToFilter(toZoneSubQuery);
					}

					return result;
				},
				fromCollection,
				toCollection
			);
			filter.MultilingualDescription = Constants.Description.FromToSuburb;
			filter.Category = FilterCategories.Locations;
			filter.SetItemDescriptions(Res.GetData("068a8ae6-12d8-4a44-98e9-c78c79332b58", "From"), Res.GetData("8b7b0d6f-8e43-4ace-b47b-6cf9bdf9f40d", "To"));
			return filter;
		}

		public static ModuleTextFilter AddFromPostCodeFilter(ModuleFilterCollection filterCollection)
		{
			var filter = filterCollection.AddTextFilter(Constants.Codes.FromPostcode, RateEntrySchema.TI_CartagePickupAddressPostCode);
			filter.MultilingualDescription = Constants.Description.FromPostcode;
			filter.Category = FilterCategories.Locations;
			return filter;
		}

		public static ModuleTextFilter AddToPostCodeFilter(ModuleFilterCollection filterCollection)
		{
			var filter = filterCollection.AddTextFilter(Constants.Codes.ToPostcode, RateEntrySchema.TI_CartageDeliveryAddressPostCode);
			filter.MultilingualDescription = Constants.Description.ToPostcode;
			filter.Category = FilterCategories.Locations;
			return filter;
		}

		public static ModuleGuidsFilter AddFromToZoneFilter(ModuleFilterCollection filterCollection, IBusinessObjectCollection fromCollection, IBusinessObjectCollection toCollection)
		{
			var filter = filterCollection.AddGuidFilter(Constants.Codes.FromToZone, ModuleIDs.RateTransportZone, RateEntrySchema.TI_TZ_OriginZone, fromCollection, RateEntrySchema.TI_TZ_DestinationZone, toCollection);
			filter.MultilingualDescription = Constants.Description.FromToZone;
			filter.Category = FilterCategories.Locations;
			filter.SetItemDescriptions(Res.GetData("d22567ec-e251-41bb-b462-a9eab8241b3f", "From"), Res.GetData("5a3ba444-835b-4d37-a2be-1848634c8804", "To"));
			return filter;
		}

		public static ModuleTextFilter AddFromLocationDescriptionFilter(ModuleFilterCollection filterCollection)
		{
			var filter = filterCollection.AddTextFilter(Constants.Codes.FromLocationDescription, (comparisonOperator, value) => GetLocationDescriptionQuery(comparisonOperator, value, RateEntrySchema.TI_OriginLRC))
				.WithMaxLengthOf<ModuleTextFilter>(RefCountrySchema.RN_Desc)
				.WithMaxLengthOf<ModuleTextFilter>(RefUNLOCOSchema.RL_PortName)
				.WithMaxLengthOf<ModuleTextFilter>(RefZoneHeaderSchema.FZ_Description);
			filter.MultilingualDescription = Constants.Description.FromLocationDescription;
			filter.Category = FilterCategories.Locations;

			return filter;
		}

		public static ModuleTextFilter AddToLocationDescriptionFilter(ModuleFilterCollection filterCollection)
		{
			var filter = filterCollection.AddTextFilter(Constants.Codes.ToLocationDescription, (comparisonOperator, value) => GetLocationDescriptionQuery(comparisonOperator, value, RateEntrySchema.TI_DestinationLRC))
				.WithMaxLengthOf<ModuleTextFilter>(RefCountrySchema.RN_Desc)
				.WithMaxLengthOf<ModuleTextFilter>(RefUNLOCOSchema.RL_PortName)
				.WithMaxLengthOf<ModuleTextFilter>(RefZoneHeaderSchema.FZ_Description);
			filter.MultilingualDescription = Constants.Description.ToLocationDescription;
			filter.Category = FilterCategories.Locations;

			return filter;
		}

		public static ModuleTextFilter AddClientContractNumberFilter(ModuleFilterCollection filterCollection)
		{
			var filter = filterCollection.AddTextFilter(Constants.Codes.ClientContractNumber, RateEntrySchema.TI_ContractNumber);
			filter.MultilingualDescription = Constants.Description.ClientContractNumber;
			filter.Category = FilterCategories.TextSearch;
			return filter;
		}

		public static ModuleTextFilter AddCarrierContractNumberFilter(ModuleFilterCollection filterCollection)
		{
			var filter = filterCollection.AddTextFilter(Constants.Codes.CarrierContractNumber, RateEntrySchema.TI_ContractNumber);
			filter.MultilingualDescription = Constants.Description.CarrierContractNumber;
			filter.Category = FilterCategories.TextSearch;
			return filter;
		}

		public static ModuleFlagsFilter AddContractNumberLinkedFilter(ModuleFilterCollection filterCollection)
		{
			var filter = filterCollection.AddFlagFilter(Constants.Codes.ContractNumberLinked, Constants.Description.ContractNumberLinked, RateEntrySchema.TI_ContractNumberLinked, ModuleFilterSubGroup.Default);
			filter.MultilingualDescription = Constants.Description.ContractNumberLinked;
			filter.Category = FilterCategories.StatusAndFlags;

			return filter;
		}

		public static ModuleNkFilter AddCommodityCodeFilter(ModuleFilterCollection filterCollection, IBusinessObjectCollection commodityCodeList)
		{
			var filter = filterCollection.AddNkFilter(Constants.Codes.CommodityCode, RateEntrySchema.TI_RH_NKCommodityCode, ModuleIDs.RefCommodityCode, commodityCodeList);
			filter.MultilingualDescription = Constants.Description.CommodityCode;
			filter.Category = FilterCategories.TextSearch;
			return filter;
		}

		public static ModuleTextFilter AddFMCTariffIDFilter(ModuleFilterCollection filterCollection)
		{
			var filter = filterCollection.AddTextFilter(Constants.Codes.FMCTariffID, RateEntrySchema.TI_FMCTariffID);
			filter.MultilingualDescription = Constants.Description.FMCTariffID;
			filter.Category = FilterCategories.TextSearch;
			return filter;
		}

		public static ModuleTextFilter AddCarrierServiceLevelFilter(ModuleFilterCollection filterCollection)
		{
			var filter = filterCollection.AddTextFilter(Constants.Codes.CarrierServiceLevel, RateEntrySchema.TI_PL_NKCarrierServiceLevel);
			filter.MultilingualDescription = Constants.Description.CarrierServiceLevel;
			filter.Category = FilterCategories.TextSearch;
			return filter;
		}

		public static ModuleTextFilter AddGatewayServiceLevelFilter(ModuleFilterCollection filterCollection, IBusinessObjectCollection serviceLevelList)
		{
			var filter = filterCollection.AddTextFilter
				(
					Constants.Codes.GatewayServiceLevel,
					(serviceLevel) => new ZQuery(RateEntrySchema.TI_RS_NKGatewayServiceLevel, serviceLevel),
					serviceLevelList
				);

			filter.MultilingualDescription = Constants.Description.GatewayServiceLevel;
			filter.Category = FilterCategories.ModesAndTypes;

			return filter;
		}

		public static ModuleTextFilter AddShipmentGatewayServiceLevelFilter(ModuleFilterCollection filterCollection, IBusinessObjectCollection serviceLevelList)
		{
			var filter = filterCollection.AddTextFilter
			(
				Constants.Codes.ShipmentGatewayServiceLevel,
				(serviceLevel) => new ZQuery(RateEntrySchema.TI_RS_NKShipmentGatewayServiceLevel, serviceLevel),
				serviceLevelList
			);

			filter.MultilingualDescription = Constants.Description.ShipmentGatewayServiceLevel;
			filter.Category = FilterCategories.ModesAndTypes;

			return filter;
		}

		public static ModuleTextFilter AddTransportModeFilter(ModuleFilterCollection filterCollection, CodeDescriptionPairList transportModeList)
		{
			var filter =
				filterCollection
				.AddTextFilter(
					Constants.Codes.TransportMode
					, (value) =>
						{
							var query = new ZQuery();

							switch (value)
							{
								case TransportModes.AIF:
									query.AddToFilter(RateEntrySchema.TI_Mode, Core.Constants.RateMode.AIR);
									break;

								case TransportModes.SEF:
									query.AddToFilter(RateEntrySchema.TI_Mode, Core.Constants.RateMode.SEA);
									break;

								case TransportModes.ROF:
									query.AddToFilter(RateEntrySchema.TI_Mode, Core.Constants.RateMode.ROA);
									break;

								case TransportModes.RAF:
									query.AddToFilter(RateEntrySchema.TI_Mode, Core.Constants.RateMode.RAI);
									break;

								case Core.Constants.RateMode.BLK:
								case Core.Constants.RateMode.BBK:
								case Core.Constants.RateMode.ROR:
								case Core.Constants.RateMode.BCN:
								case Core.Constants.RateMode.COU:
								case Core.Constants.RateMode.OBC:
								case Core.Constants.RateMode.UNA:
									query.AddToFilter(RateEntrySchema.TI_Mode, value);
									break;

								default:
									query.AddToFilter(RatingHelper.GetFreightModeAndFCL_LCLInclusiveFilter(value, rateType: null));
									break;
							}

							var nonFreightFilter = new ZQuery();
							nonFreightFilter.AddToFilter(JoinCondition.Or, RateEntrySchema.TI_RateCategory, SQLComparisonOperator.Equal,
								new[]
								{
									// Forwarding > Origin/Destination Charges
									RatingConstants.RateCategory.ORG,
									RatingConstants.RateCategory.DST,
									// Customs > Origin/Destination Charges
									RatingConstants.RateCategory.COR,
									RatingConstants.RateCategory.CDS,
									// Shipping > Origin/Destination Charges
									RatingConstants.RateCategory.SOR,
									RatingConstants.RateCategory.SDE,
									// CFS
									RatingConstants.RateCategory.PAC,
									RatingConstants.RateCategory.UNP,
									RatingConstants.RateCategory.CST,
									// Transport
									RatingConstants.RateCategory.TBC,
									RatingConstants.RateCategory.TRN,
								});

							query.AddToFilter(new ZQuery(nonFreightFilter, RatingHelper.GetNonFreightModeInclusiveFilter(value)), JoinCondition.Or);

							return query;
						}
					, transportModeList);
			filter.MultilingualDescription = RateEntryFilterUtility.Constants.Description.TransportMode;
			filter.Category = FilterCategories.ModesAndTypes;
			return filter;
		}

		public static ModuleGuidFilter AddContainterTypeFilter(ModuleFilterCollection filterCollection, IBusinessObjectCollection containerTypes)
		{
			var filter =
				filterCollection
				.AddGuidFilter
					(
						Constants.Codes.ContainerType
						, ModuleIDs.RefContainer
						, RateEntrySchema.TI_RC
						, containerTypes
					);

			filter.MultilingualDescription = Constants.Description.ContainerType;
			filter.Category = FilterCategories.ModesAndTypes;
			return filter;
		}

		public static ModuleTextFilter AddServiceLevelFilter(ModuleFilterCollection filterCollection, IBusinessObjectCollection serviceLevelList)
		{
			var filter =
				filterCollection
				.AddTextFilter
					(
						Constants.Codes.ServiceLevel
						, (serviceLevel) => new ZQuery(RateEntrySchema.TI_RS_NKServiceLevel_NI, serviceLevel)
						, serviceLevelList
					);
			filter.MultilingualDescription = Constants.Description.ServiceLevel;
			filter.Category = FilterCategories.ModesAndTypes;
			return filter;
		}

		public static ModuleTextFilter AddAircraftTypeFilter(ModuleFilterCollection filterCollection, CodeDescriptionPairList aircraftTypeList)
		{
			var filter =
				filterCollection
				.AddTextFilter
					(
						Constants.Codes.AircraftType
						, (aircraftType) => new ZQuery(RateEntrySchema.TI_AircraftType, aircraftType)
						, aircraftTypeList
					);
			filter.MultilingualDescription = Constants.Description.AircraftType;
			filter.Category = FilterCategories.ModesAndTypes;
			return filter;
		}

		public static ModuleTextFilter AddGatewayAgentTypeTypeFilter(ModuleFilterCollection filterCollection, CodeDescriptionPairList gatewayAgentTypeList)
		{
			var filter =
				filterCollection
				.AddTextFilter
					(
						Constants.Codes.GatewayAgentType
						, (gatewayAgentType) => new ZQuery(RateEntrySchema.TI_GatewayAgentType, gatewayAgentType)
						, gatewayAgentTypeList
					);
			filter.MultilingualDescription = Constants.Description.GatewayAgentType;
			filter.Category = FilterCategories.ModesAndTypes;
			return filter;
		}

		public static ModuleGuidFilter AddControllingCustomerFilter(ModuleFilterCollection filterCollection, IBusinessObjectCollection orgHeaderList)
		{
			var filter = filterCollection.AddGuidFilter(Constants.Codes.ControllingCustomer, ModuleIDs.Organisation, RateEntrySchema.TI_OH_ControllingCustomer, orgHeaderList);
			filter.MultilingualDescription = Constants.Description.ControllingCustomer;
			filter.Category = FilterCategories.Organisations;
			return filter;
		}

		public static ModuleGuidFilter AddConsigneeFilter(ModuleFilterCollection filterCollection, IBusinessObjectCollection orgHeaderList)
		{
			var filter = filterCollection.AddGuidFilter(Constants.Codes.Consignee, ModuleIDs.Organisation, RateEntrySchema.TI_OH_Consignee, orgHeaderList);
			filter.MultilingualDescription = Constants.Description.Consignee;
			filter.Category = FilterCategories.Organisations;
			return filter;
		}

		public static ModuleGuidFilter AddConsignorFilter(ModuleFilterCollection filterCollection, IBusinessObjectCollection orgHeaderList)
		{
			var filter = filterCollection.AddGuidFilter(Constants.Codes.Consignor, ModuleIDs.Organisation, RateEntrySchema.TI_OH_Consignor, orgHeaderList);
			filter.MultilingualDescription = Constants.Description.Consignor;
			filter.Category = FilterCategories.Organisations;
			return filter;
		}

		public static ModuleGuidFilter AddCarrierTransportProviderFilter(ModuleFilterCollection filterCollection, IBusinessObjectCollection orgHeaderList)
		{
			var filter = filterCollection.AddGuidFilter(Constants.Codes.CarrierTransportProvider, ModuleIDs.Organisation, RateEntrySchema.TI_OH_TransportProvider, orgHeaderList);
			filter.MultilingualDescription = Constants.Description.CarrierTransportProvider;
			filter.Category = FilterCategories.Organisations;
			return filter;
		}

		public static ModuleGuidFilter AddSupplierFilter(ModuleFilterCollection filterCollection, IBusinessObjectCollection orgHeaderList)
		{
			var filter = filterCollection.AddGuidFilter(Constants.Codes.ServiceProvider, ModuleIDs.Organisation, RateEntrySchema.TI_OH_Supplier, orgHeaderList);
			filter.MultilingualDescription = Constants.Description.ServiceProvider;
			filter.Category = FilterCategories.Organisations;
			return filter;
		}

		public static ModuleGuidFilter AddProductWarehouseFilter(ModuleFilterCollection filterCollection, IBusinessObjectCollection warehouses)
		{
			var filter =
				filterCollection
				.AddGuidFilter
					(
						Constants.Codes.ProductWarehouse
						, ModuleIDs.WhsConfigWarehouse
						, (warehouse) => new ZQuery(RateEntrySchema.TI_ParentID, warehouse)
						, warehouses
					);
			filter.MultilingualDescription = Constants.Description.ProductWarehouse;
			filter.Category = FilterCategories.Organisations;
			return filter;
		}

		public static ModuleGuidFilter AddTransitWarehouseFilter(ModuleFilterCollection filterCollection, IBusinessObjectCollection warehouses)
		{
			var filter =
				filterCollection
				.AddGuidFilter
					(
						Constants.Codes.TransitWarehouse
						, ModuleIDs.WhsConfigWarehouse
						, (warehouse) => new ZQuery(RateEntrySchema.TI_ParentID, warehouse)
						, warehouses
					);
			filter.MultilingualDescription = Constants.Description.TransitWarehouse;
			filter.Category = FilterCategories.Organisations;
			return filter;
		}

		public static ModuleGuidsFilter AddFromToOrganizationFilter(ModuleFilterCollection filterCollection, IBusinessObjectCollection fromOrgList, IBusinessObjectCollection toOrgList)
		{
			var fromToOrganizationFilter = filterCollection.AddGuidFilter(Constants.Codes.FromToOrganization, ModuleIDs.Organisation, RateEntrySchema.TI_OH_Consignor, fromOrgList, RateEntrySchema.TI_OH_Consignee, toOrgList);
			fromToOrganizationFilter.MultilingualDescription = Constants.Description.FromToOrganization;
			fromToOrganizationFilter.Category = FilterCategories.Organisations;
			fromToOrganizationFilter.SetItemDescriptions(Res.GetData("8c5b2e6f-f16e-49b6-b5f6-419999fd4dc5", "From"), Res.GetData("681cb82b-e73e-433e-979f-238e43a7fd0f", "To"));
			return fromToOrganizationFilter;
		}

		public static ModuleNkFilter AddCurrencyFilter(ModuleFilterCollection filters, RefCurrencyCollection refCurrencyCollection)
		{
			var filter = filters.AddNkFilter(Constants.Codes.Currency, RateEntrySchema.TI_RX_NKCurrency, ModuleIDs.RefCurrency, refCurrencyCollection);
			filter.Category = FilterCategories.Other;
			filter.MultilingualDescription = Constants.Description.Currency;
			return filter;
		}

		public static ModuleTextFilter AddShipmentConsolidationStatusFilter(ModuleFilterCollection filterCollection, CodeDescriptionPairList shipmentConsolidationStatusList)
		{
			var filter =
				filterCollection
				.AddTextFilter
					(
						Constants.Codes.ShipmentConsolidationStatus
						, (shipmentConsolidationStatus) => new ZQuery(RateEntrySchema.TI_ShipmentConsolidationStatus, shipmentConsolidationStatus)
						, shipmentConsolidationStatusList
					);
			filter.MultilingualDescription = Constants.Description.ShipmentConsolidationStatus;
			filter.Category = FilterCategories.ModesAndTypes;
			return filter;
		}

		public static ModuleTextFilter AddHBLDeliveryModeFilter(ModuleFilterCollection filterCollection, CodeDescriptionPairList hblDeliveryModeList)
		{
			var filter =
				filterCollection
				.AddTextFilter
					(
						Constants.Codes.HBLDeliveryMode
						, (hblDeliveryMode) => new ZQuery(RateEntrySchema.TI_HBLDeliveryMode, hblDeliveryMode)
						, hblDeliveryModeList
					);
			filter.MultilingualDescription = Constants.Description.HBLDeliveryMode;
			filter.Category = FilterCategories.ModesAndTypes;
			return filter;
		}

		public static ModuleTextFilter AddIsNonOperatingReeferFilter(ModuleFilterCollection filterCollection, CodeDescriptionPairList isNonOperatedReeferList)
		{
			var filter =
				filterCollection
				.AddTextFilter
				(
					Constants.Codes.IsNonOperatingReefer
					, (isNonOperatedReefer) => new ZQuery(RateEntrySchema.TI_IsNonOperatedReefer, isNonOperatedReefer)
					, isNonOperatedReeferList
				);
			filter.MultilingualDescription = Constants.Description.IsNonOperatingReefer;
			filter.Category = FilterCategories.ModesAndTypes;
			return filter;
		}

		public static CodeDescriptionPairList GetTransportModeList()
		{
			var result = new CodeDescriptionPairList(OLookUpEditType.CustomType);
			result.AddPair(TransportModes.AIF, Res.GetString("68f07040-8131-497b-a21f-4f74ab3f7685", "Air Freight"));
			result.AddPair(Core.Constants.RateMode.AIR, Res.GetString("8a50b161-83a7-11e6-b363-fcaa14295823", "Air Freight (LSE and ULD)"));
			result.AddPair(Core.Constants.RateMode.LSE, Res.GetString("8de87e1e-83a7-11e6-8401-fcaa14295823", "Air Freight (LSE)"));
			result.AddPair(Core.Constants.RateMode.ULD, Res.GetString("69a57221-d05e-4f86-b357-02427e2aede8", "Air Freight (ULD)"));
			result.AddPair(TransportModes.SEF, Res.GetString("8c2945a3-82cd-492b-bdee-01e5c2eba2d3", "Sea Freight"));
			result.AddPair(Core.Constants.RateMode.SEA, Res.GetString("5d59947a-c141-4be5-b2cc-d5594f7f9877", "Sea Freight (LCL and FCL)"));
			result.AddPair(Core.Constants.RateMode.LCL, Res.GetString("d571949c-065c-4f25-9a81-835212ac262f", "Sea Freight (LCL)"));
			result.AddPair(Core.Constants.RateMode.FCL, Res.GetString("97abb17b-2de0-4428-8eeb-7c505c7cef53", "Sea Freight (FCL)"));
			result.AddPair(TransportModes.ROF, Res.GetString("a51445bb-5828-41b5-bf3a-beae985c849f", "Road Freight"));
			result.AddPair(Core.Constants.RateMode.ROA, Res.GetString("d6c80ebf-2356-40c0-ad2c-3360c4173a02", "Road Freight (LCL, FCL and FTL)"));
			result.AddPair(Core.Constants.RateMode.LRO, Res.GetString("f291691e-9976-4fb0-97ae-5ff98abbfe56", "Road Freight (LCL)"));
			result.AddPair(Core.Constants.RateMode.FRO, Res.GetString("e5051f0c-c0f6-4a24-8adb-15061fff9ac9", "Road Freight (FCL)"));
			result.AddPair(Core.Constants.RateMode.FTL, Res.GetString("916f0cd1-83a7-11e6-a886-fcaa14295823", "Road Freight (FTL)"));
			result.AddPair(TransportModes.RAF, Res.GetString("5a9c5aba-0efa-41fd-952c-9d8e70a58c01", "Rail Freight"));
			result.AddPair(Core.Constants.RateMode.RAI, Res.GetString("cfd00369-9885-4480-8e50-1e0ce7298f00", "Rail Freight (LCL, FCL and FWL)"));
			result.AddPair(Core.Constants.RateMode.LRA, Res.GetString("2872709c-6e3b-4ef4-9661-c35235c2b9d9", "Rail Freight (LCL)"));
			result.AddPair(Core.Constants.RateMode.FRA, Res.GetString("4fd4a3c1-18fb-4164-983f-99ef536939b5", "Rail Freight (FCL)"));
			result.AddPair(Core.Constants.RateMode.FWL, Res.GetString("94e76aae-83a7-11e6-add3-fcaa14295823", "Rail Freight (FWL)"));
			result.AddPair(Core.Constants.RateMode.MAI, Res.GetString("dba431dd-1090-483a-b40b-b7e5e685e70c", "Post"));
			result.AddPair(Core.Constants.RateMode.BBK, Res.GetString("93c3482d-0556-4986-af24-30833329dcf3", "Break Bulk"));
			result.AddPair(Core.Constants.RateMode.BLK, Res.GetString("5c85ab21-7c49-40cf-a599-885ba7387262", "Bulk"));
			result.AddPair(Core.Constants.RateMode.ROR, Res.GetString("d66621dc-3707-4147-bf6c-c003ce71c6a1", "Roll On / Roll Off"));
			result.AddPair(Core.Constants.RateMode.BCN, Res.GetString("69c1c9f5-8d17-455e-892e-6085a3661537", "Buyer's Consol"));
			result.AddPair(Core.Constants.RateMode.COU, Res.GetString("951ab2db-861f-40d8-8faa-e7712a7f0045", "Courier"));
			result.AddPair(Core.Constants.RateMode.UNA, Res.GetString("3997b03b-492e-436e-beef-e2b2139209b2", "Unaccompanied"));
			result.AddPair(Core.Constants.RateMode.OBC, Res.GetString("bb728914-ac94-43d1-a396-fbf167a70570", "On Board Courier"));

			return result;
		}

		public static CodeDescriptionPairList GetAircraftTypeList()
		{
			var aircraftTypes = new CodeDescriptionPairList(OLookUpEditType.CustomType);
			aircraftTypes.AddPair(Core.Constants.AircraftType.CAO, Core.Constants.AircraftTypeDescriptions.CAO);
			aircraftTypes.AddPair(Core.Constants.AircraftType.PAX, Core.Constants.AircraftTypeDescriptions.PAX);

			return aircraftTypes;
		}

		public static CodeDescriptionPairList GetGatewayAgentTypeList()
		{
			var gatewayAgentTypes = new CodeDescriptionPairList(OLookUpEditType.CustomType);
			gatewayAgentTypes.AddPair(Core.Constants.GatewayAgentType.Codes.FirstSendingAgent, Core.Constants.GatewayAgentType.Descriptions.FirstSendingAgent);
			gatewayAgentTypes.AddPair(Core.Constants.GatewayAgentType.Codes.ReceivingAgent, Core.Constants.GatewayAgentType.Descriptions.ReceivingAgent);
			gatewayAgentTypes.AddPair(Core.Constants.GatewayAgentType.Codes.ReceivingAgentForImport, Core.Constants.GatewayAgentType.Descriptions.ReceivingAgentForImport);
			gatewayAgentTypes.AddPair(Core.Constants.GatewayAgentType.Codes.SendingAgent, Core.Constants.GatewayAgentType.Descriptions.SendingAgent);
			gatewayAgentTypes.AddPair(Core.Constants.GatewayAgentType.Codes.SucceedingSendingAgent, Core.Constants.GatewayAgentType.Descriptions.SucceedingSendingAgent);

			return gatewayAgentTypes;
		}

		#region Internals

		static ZQuery GetLocationDescriptionQuery(SQLComparisonOperator comparisonOperator, object value, SchemaColumn column)
		{
			var result = new ZQuery();
			var textValue = (ZString)value;
			if (textValue.IsEmpty)
			{
				return result;
			}

			var rateEntryQuery = new ZDBOnlyQuery(typeof(RateEntry));

			var countrySubQuery = new ZDBOnlySubQuery(typeof(RefCountry), RefCountrySchema.RN_Code);
			countrySubQuery.AddToFilter(RefCountrySchema.RN_Desc, comparisonOperator, textValue);
			countrySubQuery.AddToFilter(RefCountrySchema.RN_IsActive, true);
			rateEntryQuery.AddSubQuery(column, countrySubQuery, JoinCondition.Or);

			var unlocoSubQuery = new ZDBOnlySubQuery(typeof(RefUNLOCO), RefUNLOCOSchema.RL_Code);
			unlocoSubQuery.AddToFilter(RefUNLOCOSchema.RL_PortName, comparisonOperator, textValue);
			unlocoSubQuery.AddToFilter(RefUNLOCOSchema.RL_IsActive, true);
			rateEntryQuery.AddSubQuery(column, unlocoSubQuery, JoinCondition.Or);

			var iataRegionSubQuery = new ZDBOnlySubQuery(typeof(RefUNLOCO), RefUNLOCOSchema.RL_IATARegionCode);
			iataRegionSubQuery.AddToFilter(RefUNLOCOSchema.RL_PortName, comparisonOperator, textValue);
			iataRegionSubQuery.AddToFilter(RefUNLOCOSchema.RL_IsActive, true);
			rateEntryQuery.AddSubQuery(column, iataRegionSubQuery, JoinCondition.Or);

			var zoneHeaderSubQuery = new ZDBOnlySubQuery(typeof(RefZoneHeader), RefZoneHeaderSchema.FZ_Code);
			zoneHeaderSubQuery.AddToFilter(RefZoneHeaderSchema.FZ_Description, comparisonOperator, textValue);
			rateEntryQuery.AddSubQuery(column, zoneHeaderSubQuery, JoinCondition.Or);

			return result.AddToFilter(rateEntryQuery);
		}

		#endregion
	}
}
