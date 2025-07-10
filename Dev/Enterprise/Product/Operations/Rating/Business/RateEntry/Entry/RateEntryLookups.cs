using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static System.FormattableString;
using static Enterprise.Core.Constants;

namespace Enterprise.Rating.Business
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
	public class RateEntryLookups : AutoRateEntryLookups
	{
		public RateEntryLookups(AutoRateEntry parent)
			: base(parent)
		{
			RateEntry = (IRateEntry)parent;
			Factory = parent.Factory;
		}

		public RateEntryLookups(IRateEntry parent, BusinessObjectFactory factory) : base(null)
		{
			RateEntry = parent;
			Factory = factory;
		}

		protected override BusinessObjectFactory Factory { get; }
		IRateEntry RateEntry { get; }

		#region Agent Overrides

		public new ForwarderCollection AgentOverrides
		{
			get { return Factory.GetCachedValue("ForwarderCollection", delegate { return new ForwarderCollection(Factory); }); }
		}

		#endregion

		#region INCO Terms

		public CodeDescriptionPairList IncoTerms
		{
			get
			{
				return Factory.GetCachedValue("RateEntryLookups.IncoTerms", () => new IncoTermsCodeDescriptionPairList(IncoTermsListType.ActiveIncoTerms));
			}
		}

		#endregion

		#region Gateway Agent types

		public CodeDescriptionPairList GatewayAgentTypes
		{
			get
			{
				return Factory.GetCachedValue(string.Concat("GatewayAgentTypes", "ef878176-fd35-4f44-b9f4-92288d162a51"), () =>   // Cache key, not related to GUI
				{
					var agentTypes = new CodeDescriptionPairList();
					agentTypes.AddPair(GatewayAgentType.Codes.ReceivingAgent, GatewayAgentType.Descriptions.ReceivingAgent);
					agentTypes.AddPair(GatewayAgentType.Codes.SendingAgent, GatewayAgentType.Descriptions.SendingAgent);
					agentTypes.AddPair(GatewayAgentType.Codes.FirstSendingAgent, GatewayAgentType.Descriptions.FirstSendingAgent);
					agentTypes.AddPair(GatewayAgentType.Codes.SucceedingSendingAgent, GatewayAgentType.Descriptions.SucceedingSendingAgent);
					agentTypes.AddPair(GatewayAgentType.Codes.ReceivingAgentForImport, GatewayAgentType.Descriptions.ReceivingAgentForImport);

					return agentTypes;
				});
			}
		}

		public CodeDescriptionPairList NotObsoleteGatewayAgentTypes
		{
			get
			{
				return Factory.GetCachedValue(string.Concat("NotObsoleteGatewayAgentTypes", "3513cea8-dbb4-43bd-ba03-ed55f66f5eb4"), () =>   // Cache key, not related to GUI
				{
					var agentTypes = new CodeDescriptionPairList();
					agentTypes.AddPair(GatewayAgentType.Codes.ReceivingAgent, GatewayAgentType.Descriptions.ReceivingAgent);
					agentTypes.AddPair(GatewayAgentType.Codes.SendingAgent, GatewayAgentType.Descriptions.SendingAgent);
					agentTypes.AddPair(GatewayAgentType.Codes.ReceivingAgentForImport, GatewayAgentType.Descriptions.ReceivingAgentForImport);

					return agentTypes;
				});
			}
		}

		#endregion

		#region ShipmentGatewayServiceLevels

		public override RefServiceLevelCollection ShipmentGatewayServiceLevels =>
			new GatewayServiceLevelCollection(Factory);

		#endregion

		#region Shipment Consolidation Status

		public CodeDescriptionPairList ShipmentConsolidationStatusList
		{
			get
			{
				return Factory.GetCachedValue(string.Concat("ShipmentConsolidationStatusList", "2E692561-8F64-4A67-B672-8F844C17A892"), () =>   // Cache key, not related to GUI
				{
					return GetShipmentConsolidationStatusList();
				});
			}
		}

		#endregion

		#region Aircraft types

		public CodeDescriptionPairList AircraftTypes
		{
			get
			{
				return Factory.GetCachedValue(string.Concat("AircraftTypes", "b0fc4f61-7fc1-44c5-9a06-200554d93ea4"), () =>   // Cache key, not related to GUI
				{
					var aircraftTypes = new CodeDescriptionPairList(OLookUpEditType.CustomType);
					aircraftTypes.AddPair(AircraftType.CAO, AircraftTypeDescriptions.CAO);
					aircraftTypes.AddPair(AircraftType.PAX, AircraftTypeDescriptions.PAX);

					return aircraftTypes;
				});
			}
		}

		#endregion

		#region Locations

		public RatingLocationCollection Locations
		{
			get
			{
				if (RateEntry.IsAirFreight() && RateEntry.ParentRatingHeader.IsStandardCostRate())
				{
					return Factory.GetCachedValue(nameof(RatingIATASupportedLocationCollection),
								delegate
								{ return new RatingIATASupportedLocationCollection(Factory); });
				}
				else
				{
					return Factory.GetCachedValue("LocationCollection", delegate
					{ return new RatingLocationCollection(Factory); });
				}
			}
		}

		public CodeDescriptionPairList LocationSourceOptions
			=> Factory.GetCachedValue(nameof(LocationSourceOptions), () => new CodeDescriptionPairList
					{
						LocationSourceOption.FirstLoad,
						LocationSourceOption.LastDischarge,
						LocationSourceOption.FirstRouteSetLoad,
						LocationSourceOption.LastRouteSetDischarge,
					});

		public sealed class LocationSourceOption
		{
			public sealed class Code
			{
				public const string FirstLoad = "1LD";
				public const string LastDischarge = "LDC";
				public const string FirstRouteSetLoad = "MLD";
				public const string LastRouteSetDischarge = "MDC";
			}

			public static CodeDescriptionPair FirstLoad =>
				new CodeDescriptionPair(
					Code.FirstLoad,
					ResString.GetMultilingualString("8F5B5CFC-700C-429B-A357-94564FF301F7", "First Load"));

			public static CodeDescriptionPair LastDischarge =>
				new CodeDescriptionPair(
					Code.LastDischarge,
					ResString.GetMultilingualString("38A1CDCC-325D-41B9-A96A-BC3B19E8DB82", "Last Discharge"));

			public static CodeDescriptionPair FirstRouteSetLoad =>
				new CodeDescriptionPair(
					Code.FirstRouteSetLoad,
					ResString.GetMultilingualString("A65950CA-6259-4B54-8037-BAE3E8D5F799", "First Mode & Route Set Load"));

			public static CodeDescriptionPair LastRouteSetDischarge =>
				new CodeDescriptionPair(
					Code.LastRouteSetDischarge,
					ResString.GetMultilingualString("C22B90A8-92E2-467E-A9D4-BF8F03913BA3", "Last Mode & Route Set Discharge"));
		}

		#endregion

		#region Transport Zones

		public RateTransportZonesCollection TransportZones
		{
			get
			{
				return Factory.GetCachedValue("TransportZones" + string.Join(string.Empty, TransportZoneOwnerPKs) + RateEntry.TI_Mode, GetTransportZones);
			}
		}

		RateTransportZonesCollection GetTransportZones()
		{
			var zonesQuery = new ZDBOnlyQuery(typeof(RateTransportZone));

			var zoneOwnerSubQuery = new ZDBOnlySubQuery(typeof(RateTransportProvider), RateTransportProviderSchema.PK);
			zoneOwnerSubQuery.AddToFilter(RateTransportProviderSchema.TP_OH_RelatedParty, TransportZoneOwnerPKs);
			zoneOwnerSubQuery.AddToFilter(JoinCondition.Or, RateTransportProviderSchema.TP_OH_RelatedParty, null);

			var zoneTypeAndModeSubQuery = new ZDBOnlySubQuery(typeof(RateTransportProvider), RateTransportProviderSchema.PK);
			zoneTypeAndModeSubQuery.AddToFilter(RateTransportProviderSchema.TP_ZoneType, RatingApplicableZoneTypes);
			zoneTypeAndModeSubQuery.AddToFilter(RateTransportProviderSchema.TP_ZoneMode, RateTransportZoneHelper.GetPossibleZoneModes(RateEntry.TI_Mode));

			zonesQuery.AddSubQuery(RateTransportZonesSchema.TZ_TP, zoneTypeAndModeSubQuery, JoinCondition.And);
			zonesQuery.AddSubQuery(RateTransportZonesSchema.TZ_TP, zoneOwnerSubQuery, JoinCondition.And);
			zonesQuery.AddToFilter(JoinCondition.And, RateTransportZonesSchema.TZ_IsActive, true);

			var result = new RateTransportZonesCollection(Factory);

			var allZoneTypeFlagDefault = new FilterBusinessObjectDefault("Zone Type", "Property0", ZBool.True);
			var ratingZoneTypeFlagDefault = new FilterBusinessObjectDefault("Zone Type", "Property2", ZBool.True);
			result.FilterBusinessObjectDefaults.Add(allZoneTypeFlagDefault);
			result.FilterBusinessObjectDefaults.Add(ratingZoneTypeFlagDefault);

			result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Zone Mode", "Property", RateEntry.TI_Mode));

			result.AddNotificationWhenAdditionalFilterNotMetOverride = ZonesNotificationDelegate;
			result.AdditionalFilter = zonesQuery;

			return result;
		}

		void ZonesNotificationDelegate(StringCollectionX errors, BusinessObject bizObj)
		{
			var zone = (RateTransportZone)bizObj;
			var provider = zone.TransportProvider;

			if (!RatingApplicableZoneTypes.Contains(provider.TP_ZoneType))
			{
				errors.Add(Res.GetString("10da7ce5-75a8-4a0d-a589-6968839aa063", "This Transport Zone cannot be chosen as the Zone Type is not compatible with Rating."));
			}

			if (!provider.TP_OH_RelatedParty.IsEmpty && !TransportZoneOwnerPKs.Contains(provider.TP_OH_RelatedParty))
			{
				errors.Add(Res.GetString("7eb9c398-214e-4f7a-b056-f7ac96a40877",
					"This Transport Zone cannot be chosen because it is not related to any organization on this rate header or rate entry. Please choose a Transport Zone that is generic or related to the Service Provider, From/To Organizations on the rate entry."));
			}
		}

		static List<string> RatingApplicableZoneTypes
		{
			get
			{
				var result = new List<string>();

				result.Add(RatingConstants.RatingZoneTypes.All);
				result.Add(RatingConstants.RatingZoneTypes.Rating);

				return result;
			}
		}

		static List<string> RatingApplicableZoneModes => new CodeDescriptionPairList(OLookUpEditType.RateModes).GetAllCodes().ToList();

		internal IEnumerable<ZGuid> TransportZoneOwnerPKs
		{
			get
			{
				yield return RateEntry.ParentRatingHeader?.Header?.PK ?? ZGuid.Empty;
				yield return RateEntry.TI_OH_Supplier;
				yield return RateEntry.TI_OH_Consignee;
				yield return RateEntry.TI_OH_Consignor;
			}
		}

		#endregion

		#region Suburbs

		public RefCityTownCollection Suburbs
		{
			get { return Factory.GetCachedValue((NoResString)"Suburbs" + RateEntry.OriginCountryCode(), GetSuburbsCollection); } // This is a cache key
		}

		RefCityTownCollection GetSuburbsCollection()
		{
			var countryFilter = RateEntry.OriginCountryCode().IsEmpty
				? ZQuery.NoResultQuery
				: new ZQuery(RefCityTownSchema.R9_RN_NKCountry, RateEntry.OriginCountryCode());

			return new RefCityTownCollection(Factory, countryFilter);
		}

		#endregion

		#region Cartage Addresses

		public override OrgAddressCollection CartagePickupAddressOverrides
		{
			get
			{
				if (fCartagePickupAddresses == null)
				{
					fCartagePickupAddresses = LoadCartageAddressCollection(RateEntrySchema.TI_OH_Consignor);
				}

				return fCartagePickupAddresses;
			}
		}

		OrgAddressCollection fCartagePickupAddresses;

		public override OrgAddressCollection CartageDeliveryAddressOverrides
		{
			get
			{
				if (fCartageDeliveryAddresses == null)
				{
					fCartageDeliveryAddresses = LoadCartageAddressCollection(RateEntrySchema.TI_OH_Consignee);
				}

				return fCartageDeliveryAddresses;
			}
		}
		OrgAddressCollection fCartageDeliveryAddresses;

		public void InvalidateCartageAddresses(SchemaColumn orgColumn)
		{
			if (orgColumn == RateEntrySchema.TI_OH_Consignor)
			{
				fCartagePickupAddresses = null;
			}

			if (orgColumn == RateEntrySchema.TI_OH_Consignee)
			{
				fCartageDeliveryAddresses = null;
			}
		}

		OrgAddressCollection LoadCartageAddressCollection(SchemaColumn orgColumn)
		{
			var filter = new ZQuery(OrgAddressSchema.OA_OH, Parent[orgColumn.Name]);
			filter.AddToFilter(new ZQuery(OrgAddressSchema.OA_IsActive, true));

			var result = new OrgAddressCollection(Factory, filter);
			var resultToReturn = new OrgAddressCollection(Factory);
			result.Load();

			foreach (OrgAddress address in result)
			{
				if ((address.AddressCapability.GetCapabilityEnabled(OrgConstants.AddressType.PickupAndDelivery) ||
					address.AddressCapability.GetCapabilityEnabled(OrgConstants.AddressType.Office)) ||
					(orgColumn == RateEntrySchema.TI_OH_Consignor &&
					address.AddressCapability.GetCapabilityEnabled(OrgConstants.AddressType.Pickup)) ||
					((orgColumn == RateEntrySchema.TI_OH_Consignee) &&
					address.AddressCapability.GetCapabilityEnabled(OrgConstants.AddressType.Delivery))
					)
				{
					resultToReturn.Add(address);
				}
			}

			return resultToReturn;
		}

		#endregion

		#region Shipping Providers

		public ShippingProviderCollection ShippingProviders
		{
			get
			{
				if (RateEntry.IsAir())
				{
					return Factory.GetCachedValue("AirShippingProviderCollection", () => new AirShippingProviderCollection(Factory));
				}
				else if (RateEntry.IsSea())
				{
					return Factory.GetCachedValue("SeaShippingProviderCollection", () => new SeaShippingProviderCollection(Factory));
				}
				else if (RateEntry.IsRoad())
				{
					return Factory.GetCachedValue("LocalTransportCollection", () => new LocalTransportCollection(Factory));
				}
				else if (RateEntry.IsRail())
				{
					return Factory.GetCachedValue("RailShippingProviderCollection", () => new RailShippingProviderCollection(Factory));
				}
				else
				{
					return Factory.GetCachedValue("ShippingProviderCollection", () => new ShippingProviderCollection(Factory));
				}
			}
		}

		public ShipsAgencyPrincipalCollection ShippingPrincipals
		{
			get { return Factory.GetCachedValue("ShipsAgencyPrincipalCollection", delegate { return new ShipsAgencyPrincipalCollection(Factory); }); }
		}

		#endregion

		#region Suppliers

		public override OrgHeaderCollection Suppliers
		{
			get
			{
				if (RateEntry.IsCosting())
				{
					return Factory.GetCachedValue("ServiceProviderCollection", delegate
					{ return new ServiceProviderCollection(Factory); });
				}
				else
				{
					return Factory.GetCachedValue("CreditorCollection", delegate
					{ return new CreditorCollection(Factory); });
				}
			}
		}

		#endregion

		#region General Organizations

		OrgHeaderCollection GeneralOrganizations
		{
			get { return Factory.GetCachedValue<OrgHeaderCollection>("GeneralOrganizations", delegate { return new OrganisationsFindBoxCollection(Factory); }); }
		}

		#endregion

		#region Consignees

		ConsigneeCollection ConsigneesRaw
		{
			get { return Factory.GetCachedValue("ConsigneeCollection", delegate { return new ConsigneeCollection(Factory); }); }
		}

		public override OrgHeaderCollection Consignees
		{
			get { return RateEntry.IsPortTransport() || RateEntry.IsDomesticTransport() ? GeneralOrganizations : ConsigneesRaw; }
		}

		#endregion

		#region Consignors

		ConsignorCollection ConsignorsRaw
		{
			get { return Factory.GetCachedValue("ConsignorCollection", delegate { return new ConsignorCollection(Factory); }); }
		}

		public override OrgHeaderCollection Consignors
		{
			get { return RateEntry.IsPortTransport() || RateEntry.IsDomesticTransport() || RateEntry.IsContainerYardTPU() ? GeneralOrganizations : ConsignorsRaw; }
		}

		#endregion

		#region Transport Modes

		public CodeDescriptionPairList TransportModes
		{
			get
			{
				if (Parent != null)
				{
					var rateType = RateEntry.ParentRatingHeader?.RateTypeSafe() ?? default;
					return Factory.GetCachedValue("TransportModes" + RateEntry.TI_RateCategory + rateType, delegate
					{ return GetTransportModesByRateCategory(RateEntry.TI_RateCategory); });
				}
				else
				{
					return Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.CustomType);
				}
			}
		}

		public static CodeDescriptionPairList GetApplicableDirections(string tariffType)
		{
			var result = new CodeDescriptionPairList();
			result.AddPair("ALL", Res.GetString("f2cbd575-0cd5-447d-8769-86cca7507e76", "All Directions"));
			var foundDirectionApplicableCategory = true;

			var companyTariffCodes = new CompanyTariffCodes();
			foreach (ZString category in companyTariffCodes.GetRateCategories(tariffType))
			{
				foundDirectionApplicableCategory = false;
				if (!companyTariffCodes.HasNoServiceDirection(category))
				{
					foundDirectionApplicableCategory = true;
					break;
				}
			}

			if (foundDirectionApplicableCategory)
			{
				result.AddPair("EXP", Res.GetString("3730ab1a-cd38-4cab-b2b8-7746fdd4f584", "Export"));
				result.AddPair("IMP", Res.GetString("087a4608-5bed-40c0-9fad-f1b2d5b9f3b9", "Import"));
			}

			return result;
		}

		public static CodeDescriptionPairList GetTransportModesByTariffType(string tariffType)
		{
			var list = new CodeDescriptionPairList();

			foreach (ZString category in new CompanyTariffCodes().GetRateCategories(tariffType))
			{
				foreach (CodeDescriptionPair pair in GetTransportModesByRateCategory(category))
				{
					list.AddPairIfNotExist(pair.Code, pair.Description);
				}
			}

			return list.Count > 1
					? list
					: new CodeDescriptionPairList();
		}

		internal static CodeDescriptionPairList GetTransportModesByRateCategory(string rateCategory)
		{
			var list = new CodeDescriptionPairList();
			switch (rateCategory)
			{
				case RatingConstants.RateCategory.AIR:
				case RatingConstants.RateCategory.CAI:
					list.AddPair(RateMode.LSE, Res.GetString("3d3b438f-83a7-11e6-b362-fcaa14295823", "Air Freight (LSE)"));
					list.AddPair(RateMode.ULD, Res.GetString("661aa636-bb0a-4b24-9d2f-150ebe5f8f9c", "Air Freight (ULD)"));
					list.AddPair(RateMode.BCN, Res.GetString("C9369D5F-4094-44C1-B872-419CA31408FB", "Buyer's Consol"));
					list.AddPair(RateMode.SCN, Res.GetString("635408b4-d28b-4a74-a537-f5da88863ed4", "Shipper's Consol"));
					break;

				case RatingConstants.RateCategory.FCL:
				case RatingConstants.RateCategory.CFC:
					list.AddPair(RateMode.SEA, Res.GetString("5fd63bc9-f1c3-45b4-b842-b6c177d686a3", "Sea Freight (FCL)"));
					list.AddPair(RateMode.ROA, Res.GetString("3559a506-d66d-4013-b8fe-17002d4ba3a1", "Road Freight (FCL)"));
					list.AddPair(RateMode.RAI, Res.GetString("73fd7772-92bd-4547-8d6f-72d3d83f419c", "Rail Freight (FCL)"));
					list.AddPair(RateMode.BCN, Res.GetString("C9369D5F-4094-44C1-B872-419CA31408FB", "Buyer's Consol"));
					list.AddPair(RateMode.SCN, Res.GetString("635408b4-d28b-4a74-a537-f5da88863ed4", "Shipper's Consol"));
					break;

				case RatingConstants.RateCategory.LCL:
				case RatingConstants.RateCategory.CLC:
					list.AddPair(RateMode.LCL, Res.GetString("0997d74f-9e89-40bf-b3b9-5460353e3eb1", "Sea Freight (LCL)"));
					list.AddPair(RateMode.LRO, Res.GetString("4659811D-D5F9-4B15-827E-D7DE7B673C0E", "Road Freight (LTL)"));
					list.AddPair(RateMode.FTL, Res.GetString("41a97321-83a7-11e6-872a-fcaa14295823", "Road Freight (FTL)"));
					list.AddPair(RateMode.LRA, Res.GetString("61f377c4-b89c-485f-b4e3-dc923542ae8e", "Rail Freight (LWL)"));
					list.AddPair(RateMode.FWL, Res.GetString("45a44770-83a7-11e6-b75c-fcaa14295823", "Rail Freight (FWL)"));
					list.AddPair(RateMode.BBK, Res.GetString("0563B6DD-0392-4F82-AABC-D557D93D1E06", "Break Bulk"));
					list.AddPair(RateMode.BLK, Res.GetString("7A179F95-6421-4F96-875E-E2D634AAB31C", "Bulk"));
					list.AddPair(RateMode.ROR, Res.GetString("DDA76067-F5E6-4E50-A5AA-C6D0FEC1C4A7", "Roll On/Roll Off"));
					list.AddPair(RateMode.BCN, Res.GetString("C9369D5F-4094-44C1-B872-419CA31408FB", "Buyer's Consol"));
					list.AddPair(RateMode.SCN, Res.GetString("635408b4-d28b-4a74-a537-f5da88863ed4", "Shipper's Consol"));

					if (rateCategory == RatingConstants.RateCategory.LCL)
					{
						list.AddPair(RateMode.UNA, Res.GetString("eb6cebd3-18b6-4c15-81c9-6392c30417a6", "Unaccompanied"));
						list.AddPair(RateMode.OBC, Res.GetString("d2e69461-12e0-469c-88a5-0ae2e369027c", "On Board Courier"));
					}

					break;

				case RatingConstants.RateCategory.ORG:
				case RatingConstants.RateCategory.DST:
				case RatingConstants.RateCategory.COR:
				case RatingConstants.RateCategory.CDS:
					list.AddRange(new CodeDescriptionPairList(OLookUpEditType.RateModes));
					list.AddPair(RateMode.BBK, Res.GetString("0563B6DD-0392-4F82-AABC-D557D93D1E06", "Break Bulk"));
					list.AddPair(RateMode.BLK, Res.GetString("7A179F95-6421-4F96-875E-E2D634AAB31C", "Bulk"));
					list.AddPair(RateMode.ROR, Res.GetString("DDA76067-F5E6-4E50-A5AA-C6D0FEC1C4A7", "Roll On/Roll Off"));
					list.AddPair(RateMode.BCN, Res.GetString("C9369D5F-4094-44C1-B872-419CA31408FB", "Buyer's Consol"));
					list.AddPair(RateMode.SCN, Res.GetString("635408b4-d28b-4a74-a537-f5da88863ed4", "Shipper's Consol"));

					if (rateCategory == RatingConstants.RateCategory.ORG || rateCategory == RatingConstants.RateCategory.DST)
					{
						list.AddPair(RateMode.COU, Res.GetString("9d7bc174-a6f1-4bc1-a813-aaf6a74a4e10", "Courier"));
						list.AddPair(RateMode.UNA, Res.GetString("eb6cebd3-18b6-4c15-81c9-6392c30417a6", "Unaccompanied"));
						list.AddPair(RateMode.OBC, Res.GetString("d2e69461-12e0-469c-88a5-0ae2e369027c", "On Board Courier"));
					}

					break;

				case RatingConstants.RateCategory.PAC:
				case RatingConstants.RateCategory.UNP:
					list.AddPair(RateMode.ALL, Res.GetString("93d6a95f-61fd-4581-a57f-4b6076a65d37", "All Freight Modes"));
					list.AddPair(RateMode.AIR, Res.GetString("5b645d21-83a7-11e6-9619-fcaa14295823", "Air Freight (ULD and LSE)"));
					list.AddPair(RateMode.ULD, Res.GetString("661aa636-bb0a-4b24-9d2f-150ebe5f8f9c", "Air Freight (ULD)"));
					list.AddPair(RateMode.LSE, Res.GetString("6034d0f0-83a7-11e6-942f-fcaa14295823", "Air Freight (LSE)"));
					list.AddPair(RateMode.SEA, Res.GetString("60a7d6e6-7905-4e7e-ac41-24ae7c155b48", "Sea Freight (LCL, FCL and GRP)"));
					list.AddPair(RateMode.LCL, Res.GetString("0997d74f-9e89-40bf-b3b9-5460353e3eb1", "Sea Freight (LCL)"));
					list.AddPair(RateMode.FCL, Res.GetString("5fd63bc9-f1c3-45b4-b842-b6c177d686a3", "Sea Freight (FCL)"));
					list.AddPair(RateMode.GRP, Res.GetString("7209dbbf-2c69-4ca2-a851-74f36f413b20", "Sea Freight (GRP)"));
					list.AddPair(RateMode.ROA, Res.GetString("56167a2f-d8ba-4961-b507-a9dfa2b2b687", "Road Freight (LCL, FCL and FTL)"));
					list.AddPair(RateMode.LRO, Res.GetString("b5b6a9f0-e0a2-48d3-a724-b977be56e81c", "Road Freight (LCL)"));
					list.AddPair(RateMode.FRO, Res.GetString("3559a506-d66d-4013-b8fe-17002d4ba3a1", "Road Freight (FCL)"));
					list.AddPair(RateMode.FTL, Res.GetString("63f73021-83a7-11e6-a81c-fcaa14295823", "Road Freight (FTL)"));
					list.AddPair(RateMode.RAI, Res.GetString("8dca4061-6e81-4edc-ab16-3469434eca0d", "Rail Freight (LCL, FCL and FWL)"));
					list.AddPair(RateMode.LRA, Res.GetString("61f377c4-b89c-485f-b4e3-dc923542ae8e", "Rail Freight (LWL)"));
					list.AddPair(RateMode.FRA, Res.GetString("73fd7772-92bd-4547-8d6f-72d3d83f419c", "Rail Freight (FCL)"));
					list.AddPair(RateMode.FWL, Res.GetString("672a6eb0-83a7-11e6-9e00-fcaa14295823", "Rail Freight (FWL)"));
					break;

				case RatingConstants.RateCategory.CST:
					list.AddPair(RateMode.ALL, Res.GetString("93d6a95f-61fd-4581-a57f-4b6076a65d37", "All Freight Modes"));
					list.AddPair(RateMode.AIR, Res.GetString("dfa5be07-6128-4279-8b64-afae0e4a884c", "Air Freight"));
					list.AddPair(RateMode.SEA, Res.GetString("4d1e712c-f131-40de-95f7-c22031278dd9", "Sea Freight"));
					list.AddPair(RateMode.ROA, Res.GetString("d968a358-5ef5-422a-8cf5-7b4c595d4c1e", "Road Freight"));
					list.AddPair(RateMode.RAI, Res.GetString("4242a75a-0d88-4327-ba45-b5445a56bc29", "Rail Freight"));
					break;

				case RatingConstants.RateCategory.WHS:
				case RatingConstants.RateCategory.TRW:
					list.AddPair(RateMode.ALL, Res.GetString("93d6a95f-61fd-4581-a57f-4b6076a65d37", "All Freight Modes"));
					break;

				case RatingConstants.RateCategory.TWU:
					list.AddPair(RateMode.ALL, Res.GetString("93d6a95f-61fd-4581-a57f-4b6076a65d37", "All Freight Modes"));
					list.AddPair(RateMode.AIR, Res.GetString("dfa5be07-6128-4279-8b64-afae0e4a884c", "Air Freight"));
					list.AddPair(RateMode.SEA, Res.GetString("4d1e712c-f131-40de-95f7-c22031278dd9", "Sea Freight"));
					list.AddPair(RateMode.ROA, Res.GetString("d968a358-5ef5-422a-8cf5-7b4c595d4c1e", "Road Freight"));
					break;

				case RatingConstants.RateCategory.CYU:
				case RatingConstants.RateCategory.CYD:
				case RatingConstants.RateCategory.CYM:
					list.AddPair(RateMode.ALL, Res.GetString("93d6a95f-61fd-4581-a57f-4b6076a65d37", "All Freight Modes"));
					list.AddPair(RateMode.ROA, Res.GetString("d968a358-5ef5-422a-8cf5-7b4c595d4c1e", "Road Freight"));
					break;

				case RatingConstants.RateCategory.TRN:
					list.AddPair(RateMode.ALL, Res.GetString("93d6a95f-61fd-4581-a57f-4b6076a65d37", "All Freight Modes"));
					list.AddPair(RateMode.AIR, Res.GetString("dfa5be07-6128-4279-8b64-afae0e4a884c", "Air Freight"));
					list.AddPair(RateMode.ROA, Res.GetString("56167a2f-d8ba-4961-b507-a9dfa2b2b687", "Road Freight (LCL, FCL and FTL)"));
					list.AddPair(RateMode.LRO, Res.GetString("b5b6a9f0-e0a2-48d3-a724-b977be56e81c", "Road Freight (LCL)"));
					list.AddPair(RateMode.FRO, Res.GetString("7b8faef8-b153-433d-9e5c-ea167e30848a", "Road Freight (FCL and FTL)"));
					break;

				case RatingConstants.RateCategory.TBC:
					list.AddPair(RateMode.ALL, Res.GetString("93d6a95f-61fd-4581-a57f-4b6076a65d37", "All Freight Modes"));
					list.AddPair(RateMode.ROA, Res.GetString("56167a2f-d8ba-4961-b507-a9dfa2b2b687", "Road Freight (LCL, FCL and FTL)"));
					list.AddPair(RateMode.LRO, Res.GetString("4659811D-D5F9-4B15-827E-D7DE7B673C0E", "Road Freight (LTL)"));
					list.AddPair(RateMode.FRO, Res.GetString("3559a506-d66d-4013-b8fe-17002d4ba3a1", "Road Freight (FCL)"));
					list.AddPair(RateMode.FTL, Res.GetString("523c00de-83a7-11e6-9028-fcaa14295823", "Road Freight (FTL)"));
					list.AddPair(RateMode.RAI, Res.GetString("8dca4061-6e81-4edc-ab16-3469434eca0d", "Rail Freight (LCL, FCL and FWL)"));
					list.AddPair(RateMode.LRA, Res.GetString("61f377c4-b89c-485f-b4e3-dc923542ae8e", "Rail Freight (LWL)"));
					list.AddPair(RateMode.FRA, Res.GetString("73fd7772-92bd-4547-8d6f-72d3d83f419c", "Rail Freight (FCL)"));
					list.AddPair(RateMode.FWL, Res.GetString("55d10e80-83a7-11e6-99a4-fcaa14295823", "Rail Freight (FWL)"));
					break;

				case RatingConstants.RateCategory.SOR:
				case RatingConstants.RateCategory.SDE:
					list.AddPair(RateMode.ALL, Res.GetString("93d6a95f-61fd-4581-a57f-4b6076a65d37", "All Freight Modes"));
					list.AddPair(RateMode.LCL, Res.GetString("376666d7-412c-4190-b018-ad01b5e698b4", "Non-Containerized"));
					list.AddPair(RateMode.FCL, Res.GetString("79810e28-c84b-458f-9e44-bda15d54c1ac", "Containerized"));
					break;

				case RatingConstants.RateCategory.SNC:
					list.AddPair(RateMode.LCL, Res.GetString("658763e1-768e-4d6a-9fc5-8e854284e77a", "Non Containerized"));
					break;

				case RatingConstants.RateCategory.SCO:
					list.AddPair(RateMode.SEA, Res.GetString("79810e28-c84b-458f-9e44-bda15d54c1ac", "Containerized"));
					break;

				case RatingConstants.RateCategory.SED:
				case RatingConstants.RateCategory.SID:
					list.AddPair(RateMode.SEA, Res.GetString("4d1e712c-f131-40de-95f7-c22031278dd9", "Sea Freight"));
					break;
			}

			return list;
		}

		public static CodeDescriptionPairList GetShipmentConsolidationStatusList()
		{
			var list = new CodeDescriptionPairList(OLookUpEditType.CustomType);
			list.AddPair(Core.Constants.ShipmentConsolidationStatus.Codes.StandaloneShipment, Core.Constants.ShipmentConsolidationStatus.Descriptions.StandaloneShipment);
			list.AddPair(Core.Constants.ShipmentConsolidationStatus.Codes.ConsolidatedShipment, Core.Constants.ShipmentConsolidationStatus.Descriptions.ConsolidatedShipment);

			return list;
		}

		#endregion

		#region Weight Volumes

		public CodeDescriptionPairList Units
		{
			get { return UnitHelper.GetUnits(RateEntry, RateEntry.Country().RN_Code, Factory); }
		}

		#endregion

		#region Transit Times

		public CodeDescriptionPairList AirTransitTimes
		{
			get { return Factory.GetCachedValue("AirTransitTimes", delegate { return GetTransitTimeList(true); }); }
		}

		public CodeDescriptionPairList SeaTransitTimes
		{
			get { return Factory.GetCachedValue("SeaTransitTimes", delegate { return GetTransitTimeList(false); }); }
		}

		public static CodeDescriptionPairList GetTransitTimeList(ZBool isAirTransitTimeList)
		{
			var result = new CodeDescriptionPairList();
			if (isAirTransitTimeList)
			{
				result.AddPair(RatingConstants.TransitTimes.SameDay, Res.GetString("772d5f58-18ee-4112-b3c0-8fd8fed76737", "Same Day"));
				result.AddPair(RatingConstants.TransitTimes.Overnight, Res.GetString("a79a5fb6-5828-49cf-802c-7ed100c608ff", "Overnight"));
			}

			result.AddPair("1", Res.GetString("5f8a39a9-8842-4daf-9ec4-02b0cd4f9a3f", "1 day"));
			for (int index = 2; index <= 120; index++)
			{
				var daysCount = index.ToString(System.Globalization.CultureInfo.InvariantCulture);
				result.AddPair(daysCount, Res.GetString("06c96fc4-4eef-4e40-8928-fecfe0d5b07c", "{0} days", daysCount));
			}

			return result;
		}

		#endregion

		#region Frequency Units

		public FrequencyList FrequencyUnits
		{
			get { return Factory.GetCachedValue<FrequencyList>(); }
		}

		#endregion

		#region Warehouses

		public BusinessObjectCollection Warehouses
		{
			get
			{
				if (RateEntry.IsTRW() || RateEntry.IsTWU())
				{
					return GetWarehouses(Factory, WarehouseCollectionType.TransitWarehouse);
				}
				else if (RateEntry.IsContainerYard() || RateEntry.IsContainerYardTPU())
				{
					return GetWarehouses(Factory, WarehouseCollectionType.CYDWarehouse);
				}
				else
				{
					return GetWarehouses(Factory, WarehouseCollectionType.ProductWarehouse);
				}
			}
		}

		public static BusinessObjectCollection GetWarehouses(BusinessObjectFactory factory, WarehouseCollectionType collectionType = WarehouseCollectionType.ProductWarehouse)
		{
			return factory.GetCachedValue(Invariant($"Warehouses - {collectionType}"), delegate // Factory Cache Key
			{
				var whsWarehouseCollectionType = ObjectFactory.GetType<IWhsWarehouseCollection>();
				var result = (BusinessObjectCollection)Activator.CreateInstance(whsWarehouseCollectionType, factory);
				((IWhsWarehouseCollection)result).WarehouseCollectionType = collectionType;
				result.Load();
				return result;
			});
		}

		#endregion

		#region Yards

		public BusinessObjectCollection Yards
		{
			get { return GetWarehouses(Factory, WarehouseCollectionType.CYDWarehouse); }
		}

		public CodeDescriptionPairList YardUnitTypes
		{
			get
			{
				var list = new CodeDescriptionPairList();
				list.AddPair(ContainerYardConstants.YardUnitType.Codes.CNT, ContainerYardConstants.YardUnitType.Descriptions.CNT);
				list.AddPair(ContainerYardConstants.YardUnitType.Codes.CHS, ContainerYardConstants.YardUnitType.Descriptions.CHS);
				list.AddPair(ContainerYardConstants.YardUnitType.Codes.GEN, ContainerYardConstants.YardUnitType.Descriptions.GEN);
				list.AddPair(ContainerYardConstants.YardUnitType.Codes.BLK, ContainerYardConstants.YardUnitType.Descriptions.BLK);
				return list;
			}
		}

		public CodeDescriptionPairList ContainerUnitSections
		{
			get
			{
				return Factory.GetCachedValue("ContainerUnitSections", () =>
				{
					var unitSectionsPairList = new CodeDescriptionPairList();
					var unitSections = GetContainerUnitSections();
					unitSections.ForEach(u =>  unitSectionsPairList.AddPair(u, u));
					return unitSectionsPairList;
				});
			}
		}

		List<ZString> GetContainerUnitSections()
		{
			var unitSections = Factory.Load<RefUnitSection>(new ZQuery());
			return unitSections.Select(unitSection => unitSection.GroupCode).Distinct().OrderBy(code => code).ToList();
		}

		public CodeDescriptionPairList YardUnitLoads
		{
			get
			{
				var list = new CodeDescriptionPairList();
				list.AddPair(ContainerYardConstants.YardUnitLoad.Codes.EMP, ContainerYardConstants.YardUnitLoad.Descriptions.EMP);
				list.AddPair(ContainerYardConstants.YardUnitLoad.Codes.LAD, ContainerYardConstants.YardUnitLoad.Descriptions.LAD);
				return list;
			}
		}

		#endregion

		#region Containers

		public override RefContainerCollection Containers
		{
			get
			{
				RatingRefContainerCollection result = null;

				if (RateEntry.IsAir())
				{
					result = Factory.GetCachedValue("ContainersAir", delegate
					{ return new RatingRefContainerCollection(Factory, RefContainerLookups.ShippingModes.Air); });
				}
				else if (RateEntry.IsSea() || RateEntry.IsRail())
				{
					result = Factory.GetCachedValue("ContainersSea", delegate
					{ return new RatingRefContainerCollection(Factory, RefContainerLookups.ShippingModes.Sea); });
				}
				else if (RateEntry.IsRoad())
				{
					result = Factory.GetCachedValue("ContainersRoad", delegate
					{ return new RatingRefContainerCollection(Factory, RefContainerLookups.ShippingModes.Road); });
				}
				else
				{
					result = Factory.GetCachedValue((NoResString)"Containers", delegate
					{ return new RatingRefContainerCollection(Factory); }); // Factory Cache Key
				}

				return result;
			}
		}

		#endregion

		#region Carrier Service Levels

		public override OrgCarrierServiceLevelCollection CarrierServiceLevels
		{
			get
			{
				if (fCarrierServiceLevels == null || fCarrierServiceLevels.Master != RateEntry.CarrierServiceLevelParent())
				{
					if (RateEntry.CarrierServiceLevelParent() == null)
					{
						fCarrierServiceLevels = Factory.GetCachedValue("CarrierServiceLevels_Empty", delegate
						{ return new OrgCarrierServiceLevelCollection(Factory); });
					}
					else
					{
						fCarrierServiceLevels = Factory.GetCachedValue("CarrierServiceLevels_" + RateEntry.CarrierServiceLevelParent().MiscServ.PK, delegate
						{ return RateEntry.CarrierServiceLevelParent().MiscServ.CarrierServiceLevels; });
					}
					fCarrierServiceLevels.Load();
				}

				return fCarrierServiceLevels;
			}
		}

		OrgCarrierServiceLevelCollection fCarrierServiceLevels;

		#endregion

		#region Payment Terms

		public CodeDescriptionPairList PaymentTerms
		{
			get { return Factory.GetCachedValue("PaymentTerms", delegate { return GetPaymentTerms(); }); }
		}

		public static CodeDescriptionPairList GetPaymentTerms()
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(RatingConstants.PaymentTerms.Prepaid, Res.GetString("9b026fc0-9edf-4ad8-897b-4208e1bce4c2", "Prepaid"));
			result.AddPair(RatingConstants.PaymentTerms.Collect, Res.GetString("47ae3a2f-03eb-49ce-8127-ddb18992e86b", "Collect"));
			return result;
		}

		#endregion

		#region Is NonOperationalReefer

		public CodeDescriptionPairList IsNonOperationalReefer
		{
			get { return Factory.GetCachedValue("IsNonOperationalReefer", delegate { return GetIsNonOperatingReeferList(); }); }
		}

		public static CodeDescriptionPairList GetIsNonOperatingReeferList(bool includeAny = true)
		{
			var result = new CodeDescriptionPairList();
			if (includeAny)
			{
				result.AddPair("", Res.GetString("991643f5-1592-4762-8579-5c922eea7c2e", "Any Reefer"));
			}
			result.AddPair("Y", Res.GetString("57ffa308-e6c4-4743-995d-90f411bed0ac", "Non-Operating Reefer"));
			result.AddPair("N", Res.GetString("a55cb933-be4c-41af-87e2-5a8436ef18a8", "Operating Reefer"));
			return result;
		}

		#endregion

		public CodeDescriptionPairList HBLDeliveryModeList
		{
			get
			{
				return Factory.GetCachedValue("HBLDeliveryModeList" + RateEntry.TI_RateCategory + RateEntry.TI_Mode, () =>
				{
					return GetHBLDeliveryModeList(RateEntry.TI_RateCategory, RateEntry.TI_Mode);
				});
			}
		}

		static List<string> ConvertRateModeForHBLDelivery(string rateCategory, string rateMode)
		{
			if (rateCategory == RatingConstants.RateCategory.FCL)
			{
				switch (rateMode)
				{
					case RateMode.SEA:
					case RateMode.ROA:
					case RateMode.RAI:
						return new List<string>() { RateMode.FCL };
					default:
						return new List<string>() { rateMode };
				}
			}

			if (rateCategory == RatingConstants.RateCategory.ORG || rateCategory == RatingConstants.RateCategory.DST)
			{
				switch (rateMode)
				{
					case RateMode.AIR:
						return new List<string>() { RateMode.ULD, RateMode.LSE };
					case RateMode.SEA:
					case RateMode.ROA:
					case RateMode.RAI:
						return new List<string>() { RateMode.FCL, RateMode.LCL };
					case RateMode.FRO:
					case RateMode.FRA:
						return new List<string>() { RateMode.FCL };
					case RateMode.LRA:
					case RateMode.LRO:
						return new List<string>() { RateMode.LCL };
					default:
						return new List<string>() { rateMode };
				}
			}

			return new List<string>() { rateMode };
		}
		public static CodeDescriptionPairList GetHBLDeliveryModeList(string rateCategory, string rateMode)
		{
			if (string.IsNullOrWhiteSpace(rateMode))
			{
				// Use these values for the filter’s drop-down. We did not use reflection or union different registries, because it could be expensive.
				// Added a test (TestValidate_AllHBLDeliveryModesInFilter), so that if we add a new HBL Delivery Mode, we update this function.
				var result = new CodeDescriptionPairList();

				result.AddPair(Core.Constants.HBLDeliveryModes.Codes.ARPT_ARPT, Core.Constants.HBLDeliveryModes.Descriptions.ARPT_ARPT);
				result.AddPair(Core.Constants.HBLDeliveryModes.Codes.ARPT_CFS, Core.Constants.HBLDeliveryModes.Descriptions.ARPT_CFS);
				result.AddPair(Core.Constants.HBLDeliveryModes.Codes.ARPT_DOOR, Core.Constants.HBLDeliveryModes.Descriptions.ARPT_DOOR);
				result.AddPair(Core.Constants.HBLDeliveryModes.Codes.CFS_ARPT, Core.Constants.HBLDeliveryModes.Descriptions.CFS_ARPT);
				result.AddPair(Core.Constants.HBLDeliveryModes.Codes.CFS_CFS, Core.Constants.HBLDeliveryModes.Descriptions.CFS_CFS);
				result.AddPair(Core.Constants.HBLDeliveryModes.Codes.CFS_CY, Core.Constants.HBLDeliveryModes.Descriptions.CFS_CY);
				result.AddPair(Core.Constants.HBLDeliveryModes.Codes.CFS_DOOR, Core.Constants.HBLDeliveryModes.Descriptions.CFS_DOOR);
				result.AddPair(Core.Constants.HBLDeliveryModes.Codes.CY_CY, Core.Constants.HBLDeliveryModes.Descriptions.CY_CY);
				result.AddPair(Core.Constants.HBLDeliveryModes.Codes.CY_CFS, Core.Constants.HBLDeliveryModes.Descriptions.CY_CFS);
				result.AddPair(Core.Constants.HBLDeliveryModes.Codes.CY_DOOR, Core.Constants.HBLDeliveryModes.Descriptions.CY_DOOR);
				result.AddPair(Core.Constants.HBLDeliveryModes.Codes.DOOR_ARPT, Core.Constants.HBLDeliveryModes.Descriptions.DOOR_ARPT);
				result.AddPair(Core.Constants.HBLDeliveryModes.Codes.DOOR_CFS, Core.Constants.HBLDeliveryModes.Descriptions.DOOR_CFS);
				result.AddPair(Core.Constants.HBLDeliveryModes.Codes.DOOR_CY, Core.Constants.HBLDeliveryModes.Descriptions.DOOR_CY);
				result.AddPair(Core.Constants.HBLDeliveryModes.Codes.DOOR_DOOR, Core.Constants.HBLDeliveryModes.Descriptions.DOOR_DOOR);
				result.AddPair(Core.Constants.HBLDeliveryModes.Codes.DOOR_PORT, Core.Constants.HBLDeliveryModes.Descriptions.DOOR_PORT);
				result.AddPair(Core.Constants.HBLDeliveryModes.Codes.PORT_DOOR, Core.Constants.HBLDeliveryModes.Descriptions.PORT_DOOR);
				result.AddPair(Core.Constants.HBLDeliveryModes.Codes.PORT_PORT, Core.Constants.HBLDeliveryModes.Descriptions.PORT_PORT);

				return result;
			}

			var list = new CodeDescriptionPairList();
			var hblDeliveryRateModes = ConvertRateModeForHBLDelivery(rateCategory, rateMode);

			foreach (var hdrm in hblDeliveryRateModes)
			{
				var hblDeliveryModes = Freight.Business.FreightUtilities.ShipmentHBLDeliveryMode(hdrm);

				foreach (HBLDeliveryMode item in hblDeliveryModes.Modes)
				{
					if (item.ShowInList && !list.ContainsCode(item.Code))
					{
						list.AddPair(item.Code, item.Description);
					}
				}
			}

			list.Sort();

			return list;
		}
	}
}
