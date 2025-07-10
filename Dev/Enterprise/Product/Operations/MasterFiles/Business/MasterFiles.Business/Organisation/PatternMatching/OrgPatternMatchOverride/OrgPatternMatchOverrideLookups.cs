using System;
using System.Diagnostics.CodeAnalysis;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgPatternMatchOverrideLookups : AutoOrgPatternMatchOverrideLookups
	{
		public OrgPatternMatchOverrideLookups(AutoOrgPatternMatchOverride parent)
			: base(parent) { }

		#region Relationships

		public CodeDescriptionPairList OO_Relationship_List
		{
			get { return Factory.GetCachedValue("OrgPatternMatchOverrideLookups|Relationships", () => OO_Relationship_ListCore); }
		}

		protected virtual CodeDescriptionPairList OO_Relationship_ListCore
		{
			get { return GetRelationshipCodeDescriptionList(); }
		}

		public static CodeDescriptionPairList GetRelationshipCodeDescriptionList()
		{
			var list = new CodeDescriptionPairList();
			list.AddPair(Constants.OrgPatternMatchOverrideRelationships.ChargeCodes, Res.GetString("92c60a80-34a4-40b8-9ee2-f38c6733b262", "Charge Code"));
			list.AddPair(Constants.OrgPatternMatchOverrideRelationships.Commodities, Res.GetString("6e222262-88ef-401a-9c05-6a56705ab0a2", "Commodity"));
			list.AddPair(Constants.OrgPatternMatchOverrideRelationships.ContainerType, Res.GetString("325d08e0-5940-4e66-b3cb-6d2820bfc0a2", "Container Type"));
			list.AddPair(Constants.OrgPatternMatchOverrideRelationships.Country, Res.GetString("ad37008b-a67d-4f93-a352-e4ea91eb88fa", "Country/Region"));
			list.AddPair(Constants.OrgPatternMatchOverrideRelationships.Currency, Res.GetString("732333b4-20df-4d87-bd69-f146305f5ceb", "Currency"));
			list.AddPair(Constants.OrgPatternMatchOverrideRelationships.DropMode, Res.GetString("f8a8810f-93c2-42eb-9859-ac54584ad26f", "Drop Mode"));
			list.AddPair(Constants.OrgPatternMatchOverrideRelationships.Equipment, Res.GetString("3c388535-2132-44d0-aeff-26bb5b5c6bd7", "Equipment"));
			list.AddPair(Constants.OrgPatternMatchOverrideRelationships.IncoTerm, Res.GetString("45951354-9cd0-9fbf-4132-4db7657e8912", "Incoterm"));
			list.AddPair(Constants.OrgPatternMatchOverrideRelationships.Organisation, Res.GetString("fc217d8d-be6f-422c-8bf4-975f15077702", "Organization"));
			list.AddPair(Constants.OrgPatternMatchOverrideRelationships.Port, Res.GetString("bdfa9f5a-ee47-47bc-920e-a8cc0a52592f", "Port"));
			list.AddPair(Constants.OrgPatternMatchOverrideRelationships.PackageType, Res.GetString("f7e198fd-e1ae-4d21-b927-0e2a838573cc", "Package Type"));
			list.AddPair(Constants.OrgPatternMatchOverrideRelationships.EventCode, Res.GetString("3757e772-d8e5-40cc-8a06-e9814242a34d", "Event Code"));
			list.AddPair(Constants.OrgPatternMatchOverrideRelationships.Warehouse, Res.GetString("0cf47d63-ff38-46c0-836e-3f79dbdc153d", "Warehouse"));
			list.AddPair(Constants.OrgPatternMatchOverrideRelationships.ServiceLevel, Res.GetString("481d8d71-54ec-4b99-b009-2a59f4991f3a", "Service Level"));
			list.AddPair(Constants.OrgPatternMatchOverrideRelationships.IntZone, Res.GetString("28a6b7a0-d4cb-4a46-a5bd-301037739cc9", "International Zone"));
			list.AddPair(Constants.OrgPatternMatchOverrideRelationships.CarrierServiceLevel, Res.GetString("38d01a7c-51d0-4cb2-8e27-f7ec52dcb69a", "Carrier Service Level"));
			list.AddPair(Constants.OrgPatternMatchOverrideRelationships.DocumentType, Res.GetString("b3f15ec9-84ef-4b09-91c7-5913705a9932", "Document Type"));

			return list;
		}

		#endregion

		#region Context

		public CodeDescriptionPairList OO_Context_List
		{
			get
			{
				var isPTCRelationship = ((OrgPatternMatchOverride)Parent).OO_Relationship == Constants.OrgPatternMatchOverrideRelationships.Port;
				var cacheKey = string.Format("OrgPatternMatchOverrideLookups_IsPTC_{0}|OO_Context_List", isPTCRelationship ? "Y" : "N");

				return Factory.GetCachedValue(cacheKey, () =>
				{
					var result = new CodeDescriptionPairList(OrganisationsDataRegistry.Instance.UserDefinedContext.Value);
					if (isPTCRelationship && !result.ContainsCode(Constants.OrgPatternMatchOverrideContexts.Codes.OceanCarrierMessage))
					{
						result.AddPair(Constants.OrgPatternMatchOverrideContexts.Codes.OceanCarrierMessage, Constants.OrgPatternMatchOverrideContexts.Descriptions.OceanCarrierMessage);
					}

					return result;
				});
			}
		}

		#endregion

		#region OrgCoNames
		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public object OrgCoNames
		{
			get
			{
				switch (((OrgPatternMatchOverride)Parent).OO_Relationship)
				{
					case Constants.OrgPatternMatchOverrideRelationships.ContainerType:
						return RefContainers;
					case Constants.OrgPatternMatchOverrideRelationships.Country:
						return RefCountries;
					case Constants.OrgPatternMatchOverrideRelationships.Currency:
						return RefCurrencies;
					case Constants.OrgPatternMatchOverrideRelationships.Commodities:
						return Commodities;
					case Constants.OrgPatternMatchOverrideRelationships.Equipment:
						return Equipment;
					case Constants.OrgPatternMatchOverrideRelationships.Organisation:
						return Organisations;
					case Constants.OrgPatternMatchOverrideRelationships.Port:
						return RefUNLOCOs;
					case Constants.OrgPatternMatchOverrideRelationships.Warehouse:
						return WhsWarehouses;
					case Constants.OrgPatternMatchOverrideRelationships.ServiceLevel:
						return RefServiceLevels;
					case Constants.OrgPatternMatchOverrideRelationships.IntZone:
						return IntZones;
					case Constants.OrgPatternMatchOverrideRelationships.DocumentType:
						return DocTypes;

					case Constants.OrgPatternMatchOverrideRelationships.ChargeCodes:
						return ChargeCodes;
					case Constants.OrgPatternMatchOverrideRelationships.DropMode:
						return DropModes;
					case Constants.OrgPatternMatchOverrideRelationships.IncoTerm:
						return OO_IncoTerm_List;
					case Constants.OrgPatternMatchOverrideRelationships.PackageType:
						return PackageTypes;
					case Constants.OrgPatternMatchOverrideRelationships.EventCode:
						return EventTypes;
					case Constants.OrgPatternMatchOverrideRelationships.CarrierServiceLevel:
						return CarrierServiceLevels;
					default:
						return Organisations;
				}
			}
		}
		#endregion

		#region INCOTERMS

		public CodeDescriptionPairList OO_IncoTerm_List
			=> Factory.GetCachedValue("OrgPatternMatchOverrideLookups|IncoTermList", () => new IncoTermsCodeDescriptionPairList(IncoTermsListType.ActiveIncludingDomesticTerms));

		#endregion

		#region Organisations

		public OrganisationsFindBoxCollection Organisations
		{
			get { return Factory.GetCachedValue("OrgPatternMatchOverrideLookups|Organisations", () => new OrganisationsFindBoxCollection(Factory)); }
		}

		#endregion

		#region UNLOCOs

		public RefUNLOCOCollection RefUNLOCOs
		{
			get { return Factory.GetCachedValue("OrgPatternMatchOverrideLookups|RefUNLOCO", () => new RefUNLOCOCollection(Factory)); }
		}

		#endregion

		#region Currencies

		public RefCurrencyCollection RefCurrencies
		{
			get { return Factory.GetCachedValue("OrgPatternMatchOverrideLookups|RefCurrencies", () => new RefCurrencyCollection(Factory)); }
		}

		#endregion

		#region Countries

		public RefCountryCollection RefCountries
		{
			get { return Factory.GetCachedValue("OrgPatternMatchOverrideLookups|RefCountries", () => new RefCountryCollection(Factory)); }
		}

		#endregion

		#region Commodities

		public RefCommodityCodeCollection Commodities
		{
			get { return Factory.GetCachedValue("OrgPatternMatchOverrideLookups|Commodities", () => new RefCommodityCodeCollection(Factory)); }
		}

		#endregion

		#region DropModes

		public CombinedEquipmentNeededList DropModes
		{
			get { return Factory.GetCachedValue("OrgPatternMatchOverrideLookups|DropModes", () => new CombinedEquipmentNeededList()); }
		}

		#endregion

		#region Equipment

		public RefEquipmentCollection Equipment
		{
			get { return Factory.GetCachedValue("OrgPatternMatchOverrideLookups|Equipment", () => new RefEquipmentCollection(Factory)); }
		}

		#endregion

		#region RefContainers

		public RefContainerCollection RefContainers
		{
			get { return Factory.GetCachedValue("OrgPatternMatchOverrideLookups|RefContainers", () => new RefContainerCollection(Factory)); }
		}

		#endregion

		#region Warehouses

		public BusinessObjectCollection WhsWarehouses
		{
			get { return Factory.GetCachedValue("OrgPatternMatchOverrideLookups|WhsWarehouses", () => (BusinessObjectCollection)Activator.CreateInstance(ObjectFactory.GetType<Warehouse.Integration.IWhsWarehouseCollection>(), Factory)); }
		}

		#endregion

		#region ChargeCode

		public AccChargeCodeCollection ChargeCodes
		{
			get { return Factory.GetCachedValue("OrgPatternMatchOverrideLookups|ChargeCodes", () => new AccChargeCodeCollection(Factory, new ZQuery(), GlbCompany.CurrentCompany.PK.ToGuid())); }
		}

		#endregion

		#region PackageType

		public RefPackTypeCollection PackageTypes
		{
			get { return Factory.GetCachedValue("OrgPatternMatchOverrideLookups|PackageTypes", () => new RefPackTypeCollection(Factory)); }
		}

		#endregion

		#region Event

		public CodeDescriptionPairList EventTypes
		{
			get { return Factory.GetCachedValue("OrgPatternMatchOverrideLookups|EventTypes", GetEventTypes); }
		}

		static CodeDescriptionPairList GetEventTypes()
		{
			var eventTypes = new CodeDescriptionPairList();
			foreach (Event evt in Events.All)
			{
				eventTypes.Add(evt);
			}

			return eventTypes;
		}

		#endregion

		#region Service Levels

		public RefServiceLevelCollection RefServiceLevels
		{
			get { return Factory.GetCachedValue("OrgPatternMatchOverrideLookups|RefServiceLevels", () => new RefServiceLevelCollection(Factory)); }
		}

		#endregion

		#region Zones

		public RefZoneHeaderCollection IntZones
		{
			get { return Factory.GetCachedValue("OrgPatternMatchOverrideLookups|IntZones", () => new RefZoneHeaderCollection(Factory)); }
		}

		#endregion

		#region CarrierServiceLevel

		public OrgCarrierServiceLevelCollection CarrierServiceLevels
		{
			get { return Factory.GetCachedValue("OrgPatternMatchOverrideLookups|CarrierServiceLevels", GetCarrierServiceLevels); }
		}

		OrgCarrierServiceLevelCollection GetCarrierServiceLevels()
		{
			var orgMiscServQuery = new ZQuery(OrgMiscServSchema.OM_OH, ((OrgPatternMatchOverride)Parent).OO_OH);
			var orgMiscServ = Factory.LoadTop1<OrgMiscServ>(orgMiscServQuery);
			var carrierServiceLevelCollection = orgMiscServ != null
				? new OrgCarrierServiceLevelCollection(orgMiscServ)
				: new OrgCarrierServiceLevelCollection(Factory);
			carrierServiceLevelCollection.Load();

			return carrierServiceLevelCollection;
		}

		#endregion

		#region DocumentType

		public RefDocTypeCollection DocTypes
		{
			get { return Factory.GetCachedValue("OrgPatternMatchOverrideLookups|DocumentTypes", () => new RefDocTypeCollection(Factory)); }
		}

		#endregion

		#region OO_ForeignCode_List

		public CodeDescriptionPairList OO_ForeignCode_List
		{
			get
			{
				var parent = (OrgPatternMatchOverride)Parent;

				if (parent.IsComPayMapping)
				{
					return parent.eNettGenericChargeCodes;
				}

				return null;
			}
		}

		#endregion
	}
}
