using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Integration.DocumentEngine;
using Enterprise.Integration.Packing;
using Enterprise.Integration.TransitWarehouse;
using Enterprise.Integration.Warehouse;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Integration.CodeLists;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.MasterFiles.Business.UNDGSubstanceLookups;

namespace Enterprise.Warehouse.Environment.Business.Testing
{
	//  IF YOU CHANGE ANY OF THIS CODE YOU MUST RUN ALL WAREHOUSE TESTS !!!!!
	//  MOST WAREHOUSE TESTS RELY ON THE EXACT DETAILS OF THESE TEST HELPERS.
	//  SPECIFICALLY THE DATA SETUPS, THE COMPILER WILL NOT FIND PROBLEMS.
	//  RUNNING LOCALISED TESTS AFTER CHANGING THIS CODE IS NOT GOOD ENOUGH!!
	public class WhsTestHelperFunctionsEnv : Assertion
	{
		public WhsTestHelperFunctionsEnv(BusinessObjectFactory factory)
		{
			Factory = factory;
		}

		public readonly BusinessObjectFactory Factory;

		#region CreateCommodityCode

		public RefCommodityCode CreateCommodityCode(string code)
		{
			var commodityCode = Factory.NewWithValidTestData<RefCommodityCode>();
			commodityCode.RH_Code = code;
			return commodityCode;
		}

		#endregion

		#region CreateProduct

		public OrgSupplierPart CreateProduct(OrgHeader owner, string code, OrgPartCategory category = null)
		{
			return CreateProduct(owner, code, OrgPartRelation.RelationshipTypes.Owner, category);
		}

		public OrgSupplierPart CreateProduct(OrgHeader owner, string code, ZByte numberOfDecimalPlaces, OrgPartCategory category = null)
		{
			return CreateProduct(owner, code, OrgPartRelation.RelationshipTypes.Owner, numberOfDecimalPlaces, category);
		}

		public OrgSupplierPart CreateProduct(OrgHeader owner, string code, string relationship, OrgPartCategory category = null)
		{
			return CreateProduct(owner, code, relationship, 0, category);
		}

		public OrgSupplierPart CreateProduct(OrgHeader owner, string code, string relationship, ZByte numberOfDecimalPlaces, OrgPartCategory category = null)
		{
			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = code;
			part.OP_Desc = code;
			part.OP_StockKeepingUnit = Constants.PkgUnit.Unit;
			part.OP_CountDecimalPlaces = numberOfDecimalPlaces;
			part.OP_Weight = 2.0m;
			part.OP_Cubic = 0.02m;
			part.OP_WeightUQ = Constants.Weight.Kilograms;
			part.OP_CubicUQ = Constants.Volume.CubicMetres;

			var relation = CreateProductClientRelationShip(owner, part, relationship);
			if (category != null)
			{
				relation.OU_OPC_Category = category.PK;
			}
			CreateProductUnit(part, Constants.PkgUnit.Carton, 12);

			return part;
		}

		#endregion

		#region CreateProductPickFace

		public WhsPickFace CreateProductPickFace(OrgSupplierPart part, OrgHeader org, WhsWarehouse whs, string location)
		{
			return CreateProductPickFace(part, org, whs.FindLocation(location));
		}

		public WhsPickFace CreateProductPickFace(OrgSupplierPart part, OrgHeader org, WhsLocation location, int replenishMin)
		{
			return CreateProductPickFace(part, org, location, (ZDecimal)replenishMin, (ZDecimal)replenishMin + 1);
		}

		public WhsPickFace CreateProductPickFace(OrgSupplierPart part, OrgHeader org, WhsLocation location, decimal replenishMin = 0m, decimal replenishMax = 1m, decimal replenishMultiple = 1m)
		{
			if (location.LocationType.WLT_LocationClass != LocationClasses.Codes.FIX)
			{
				var locationType = Factory.LoadTop1<WhsLocationType>(new ZQuery(WhsLocationTypeSchema.WLT_Code, "PFC"))
					// this can be removed when the system Location Type PFC is checked in
					?? CreateLocationType("PFC", "PFC Test", true, 1, LocationClasses.Codes.FIX);

				location.WLV_WLT_LocationType = locationType.PK;
				// Max capacity of PickFace Location should be 0.
				location.WLV_MaxWeight = 0m;
				location.WLV_MaxCubic = 0m;
				location.WLV_MaxQuantity = 0m;
			}

			var collection = new WhsPickFaceCollection(part, Factory);
			var pickface = collection.AddNew();
			pickface.WF_WL = location.PK;
			pickface.WF_OH_Client = org.PK;
			pickface.WF_ReplenishMinimum = replenishMin;
			pickface.WF_ReplenishMaximum = replenishMax;
			pickface.WF_ReplenishmentMultiple = replenishMultiple;
			return pickface;
		}

		#endregion

		#region CreateDynamicPF

		public WhsArea CreateDynamicPF(WhsWarehouse whs, WhsLocation location, string areaName = "DYNAMIC")
		{
			var dynamicLocationType = Factory.LoadTop1<WhsLocationType>(new ZQuery(WhsLocationTypeSchema.WLT_Code, "DLC")) ?? CreateLocationType("DLC", LocationClasses.Codes.DPF);
			var dynamicArea = CreateArea(whs, areaName, AreaTypes.Codes.DynamicPickFace, isPickingArea: true, isPutawayArea: false);
			location.WLV_WLT_LocationType = dynamicLocationType.PK;
			location.WLV_WA_PickingArea = dynamicArea.PK;

			return dynamicArea;
		}

		#endregion

		#region CreateProductUnit

		public OrgPartUnit CreateProductUnit(OrgSupplierPart part, string partUnitType, ZDecimal partUnitSize)
		{
			return CreateProductUnit(part, part.OP_StockKeepingUnit, partUnitType, partUnitSize);
		}

		public OrgPartUnit CreateProductUnit(OrgSupplierPart part, string partUnitType, string parentPartUnitType, ZDecimal partUnitSize)
		{
			OrgPartUnit partUnit = part.PartUnits.AddNew();
			partUnit.OF_PackType = partUnitType;
			partUnit.OF_ParentPackType = parentPartUnitType;
			partUnit.OF_QuantityInParent = partUnitSize;
			return partUnit;
		}

		#endregion

		#region CreateProductClientRelationShip

		public OrgPartRelation CreateProductClientRelationShip(OrgHeader owner, OrgSupplierPart part)
		{
			return CreateProductClientRelationShip(owner, part, OrgPartRelation.RelationshipTypes.Owner);
		}

		public OrgPartRelation CreateProductClientRelationShip(OrgHeader owner, OrgSupplierPart part, ZString relationship)
		{
			OrgPartRelationCollection productRelationshipList = part.RelatedOrganisations;
			OrgPartRelation result = productRelationshipList.FindByOrganisationPKAndRelationship(owner.PK, relationship);
			if (result == null)
			{
				result = productRelationshipList.AddNew();
				result.OU_OH = owner.PK;
				result.OU_OP = part.PK;
				result.OU_Relationship = relationship;
			}
			return result;
		}

		#endregion

		#region Client

		public OrgHeader CreateClient()
		{
			return CreateClient("WHTEST");
		}

		public OrgHeader CreateClient(string code)
		{
			return CreateClient(code, code);
		}

		// overriden for US specifics.
		public virtual OrgHeader CreateClient(string code, string name)
		{
			return CreateClient(code, name, "", "");
		}

		public OrgHeader CreateClient(string code, string name, string city, string state)
		{
			var org = Factory.New<OrgHeader>();
			org.OH_FullName = name;
			org.OH_Code = code;
			org.OH_IsWarehouseClient = true;

			var address = org.MainAddress;
			if (!string.IsNullOrEmpty(city))
			{
				address.OA_City = city;
			}

			if (!string.IsNullOrEmpty(state))
			{
				address.OA_State = state;
			}

			return org;
		}

		#endregion

		#region CreatePickPackParameter

		public WhsClientPickPackParamsByWhs CreatePickPackParameter(OrgHeader client)
		{
			return CreatePickPackParameter(client, null);
		}

		public WhsClientPickPackParamsByWhs CreatePickPackParameter(OrgHeader client, WhsWarehouse warehouse)
		{
			var pickPackParameter = Factory.New<WhsClientPickPackParamsByWhs>();
			pickPackParameter.WPP_OH_Client = client.PK;

			if (warehouse != null)
			{
				pickPackParameter.WPP_WW_Warehouse = warehouse.PK;
			}

			return pickPackParameter;
		}

		#endregion

		#region GlbBranch

		public GlbBranch CreateGlbBranch(ZString code)
		{
			var branch = Factory.New<GlbBranch>();
			branch.GB_Code = code;
			branch.GB_GC = GlbCompany.CurrentCompany.PK;
			branch.GB_RL_NKHomePort = Env.CurrentBranch.NKUNLOCO;

			return branch;
		}

		#endregion

		#region GlbStaff

		public GlbStaff CreateGlbStaff(ZString gsCode, ZString loginName)
		{
			return CreateGlbStaff(gsCode, loginName, true);
		}

		public GlbStaff CreateGlbStaff(ZString gsCode, ZString loginName, bool isActive)
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = gsCode;
			staff.GS_LoginName = loginName;
			staff.GS_IsActive = isActive;
			staff.StaffPlainTextPassword = "P@ssw0rd" + loginName;
			return staff;
		}

		#endregion

		#region PrintQueue

		public IStmPrintQueue CreatePrintQueue(ZString name, ZString displayName)
		{
			var printQueue = CreatePrintQueue(name);
			printQueue.SQ_DisplayName = displayName;

			return printQueue;
		}

		public IStmPrintQueue CreatePrintQueue(ZString name)
		{
			var printQueue = Factory.New<IStmPrintQueue>();
			printQueue.QueueName = name;
			printQueue.SQ_AllowPrinting = true;

			return printQueue;
		}

		#endregion

		#region Product

		public OrgSupplierPart CreateProduct(ZString partNumber, OrgHeader client)
		{
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = partNumber;
			product.RelatedOrganisations.AddOwner(client);

			return product;
		}

		#endregion

		#region CreateReleaseGroup

		public GlbGroup CreateReleaseGroup(ZString code, ZString description)
		{
			var releaseGroup = Factory.New<GlbGroup>();
			releaseGroup.GG_Code = code;
			releaseGroup.GG_Desc = description;
			return releaseGroup;
		}

		#endregion

		#region Warehouse

		public WhsWarehouse CreateWarehouse(ZString code, OrgAddress address, GlbBranch branch, bool shouldPreGenerateDDL = true)
		{
			return CreateWarehouse(code, code, address, branch, shouldPreGenerateDDL);
		}

		public WhsWarehouse CreateWarehouse(ZString code, ZString name, OrgAddress address, GlbBranch branch, bool shouldPreGenerateDDL = true)
		{
			if (branch == null)
			{
				branch = Factory.LoadFromNaturalKey<GlbBranch>(GlbBranchSchema.GB_Code, code.PadRight(GlbBranchSchema.GB_Code.MaxLength, ' '));
				branch = branch ?? CreateGlbBranch(code);
			}

			if (address == null)
			{
				address = CreateClient(code + "111").MainAddress;
				address.OA_RL_NKRelatedPortCode = Env.CurrentBranch.NKUNLOCO;
			}

			var whs = Factory.New<WhsWarehouse>();
			whs.WW_WarehouseCode = code;
			whs.WW_WarehouseName = name;
			whs.WW_OA_WarehouseAddress = address.PK;
			whs.WW_GB_RelatedCompanyBranch = branch.PK;
			whs.WW_WLT_DefaultLocationType = Factory.LoadTop1<WhsLocationType>(new ZQuery(WhsLocationTypeSchema.WLT_Code, WhsLocationType.SystemWideDefaultLocationTypeCode)).PK;
			whs.WW_AutoPrintPickingSlip = false;
			whs.WW_AutoPrintPackingSlip = false;
			((IWhsWarehouseInternals)whs).CreateDefaultArea();
			if (shouldPreGenerateDDL)
			{
				whs.CreateDefaultDockDoorLocationIfNeeded_ForTest();
			}

			return whs;
		}

		public WhsWarehouse CreateWarehouse(ZString name, string rowName)
		{
			return CreateWarehouse(name, rowName, 1, 1);
		}

		public WhsWarehouse CreateWarehouse(ZString name, string rowName, short columns, short levels, bool shouldPreGenerateDDL = true)
		{
			var whs = CreateWarehouse(name, shouldPreGenerateDDL);
			CreateRowAndGenerateLocations(whs, rowName, columns, levels);

			return whs;
		}

		public virtual WhsWarehouse CreateWarehouse(ZString name, bool shouldPreGenerateDDL = true)
		{
			var code = GetWareHouseNameSafe(name, WhsWarehouseSchema.WW_WarehouseCode.MaxLength);
			var whs = CreateWarehouse(code, name, null, null, shouldPreGenerateDDL);

			return whs;
		}

		public WhsWarehouse CreateFixedWidthLocationWarehouse(ZString name, byte fixedColumnWidth, byte fixedLevelWidth, byte fixedTraysWidth, bool shouldPreGenerateDDL = true)
		{
			var code = GetWareHouseNameSafe(name, WhsWarehouseSchema.WW_WarehouseCode.MaxLength);
			var whs = CreateWarehouse(code, name, null, null, shouldPreGenerateDDL);

			whs.IsFixedWidthLocation = true;
			whs.WW_LocationColumnsFixedWidth = fixedColumnWidth;
			whs.WW_LocationLevelsFixedWidth = fixedLevelWidth;
			whs.WW_LocationTraysFixedWidth = fixedTraysWidth;
			whs.WW_LocationsHaveLeadingZeros = true;

			return whs;
		}

		WhsWarehouse CreateWarehouse(string warehouseType, string warehouseCode, string rowName, short columns, short levels, string countrycode)
		{
			var branch = CreateGlbBranch(warehouseCode);
			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_RN_NKCountryCode = countrycode;
			var warehouse = CreateWarehouse(warehouseCode, address, branch);
			warehouse.WW_WarehouseType = warehouseType;
			CreateRowAndGenerateLocations(warehouse, rowName, columns, levels);

			return warehouse;
		}

		public WhsWarehouse CreateFTZWarehouse(string warehouseCode = "WHS", string rowName = "A", short columns = 1, short levels = 1, bool isDetailedTrackingEnabled = true, bool enableBonded = true, string countrycode = Constants.CountryCodes.Australia)
		{
			var warehouse = CreateWarehouse(WarehouseTypes.Codes.FreeTradeZone, warehouseCode, rowName, columns, levels, countrycode);
			warehouse.WW_FTZIsDetailedTrackingEnabled = isDetailedTrackingEnabled;
			EnableWarehouseForBond(warehouse, enableBonded);

			return warehouse;
		}

		public WhsWarehouse CreateCYDWarehouse(string warehouseCode = "WHS", string rowName = "A", short columns = 1, short levels = 1, string countrycode = Constants.CountryCodes.Australia)
		{
			return CreateWarehouse(WarehouseTypes.Codes.ContainerYard, warehouseCode, rowName, columns, levels, countrycode);
		}

		public WhsWarehouse CreateTRWWarehouse(string warehouseCode = "WHS", string rowName = "A", short columns = 1, short levels = 1, string countrycode = Constants.CountryCodes.Australia)
		{
			return CreateWarehouse(WarehouseTypes.Codes.Transit, warehouseCode, rowName, columns, levels, countrycode);
		}

		public WhsWarehouse CreateFTZWarehouseInUS(string warehouseCode = "WHS", string rowName = "A", short columns = 1, short levels = 1, bool isDetailedTrackingEnabled = true, bool enableBonded = true)
		{
			return CreateFTZWarehouse(warehouseCode, rowName, columns, levels, isDetailedTrackingEnabled, enableBonded, Constants.CountryCodes.UnitedStates);
		}

		protected ZString GetWareHouseNameSafe(ZString name, int maxLength)
		{
			var newName = name.SubstringSafe(name.Length - maxLength, maxLength);

			if (!NameList.Contains(newName))
			{
				NameList.Add(newName);
			}
			else
			{
				ZString tmpName = newName + NameList.Count.ToString();
				newName = tmpName.SubstringSafe(tmpName.Length - maxLength, maxLength);
				NameList.Add(newName);
			}

			return newName;
		}

		List<ZString> NameList
		{
			get { return nameList ?? (nameList = new List<ZString>()); }
		}
		List<ZString> nameList;

		public WhsWarehouse CreateWarehouse(ZString name, ZString code, string rowName)
		{
			var result = CreateWarehouse(name, rowName);
			result.WW_WarehouseCode = code;
			return result;
		}

		public void EnableWarehouseForBond(WhsWarehouse whs, bool enable)
		{
			whs.WW_IsBondedWarehouse = enable;
			if (enable)
			{
				WhsArea area = whs.Areas.AddNew();
				area.WA_Name = "BOND";
				area.WA_AreaType = CodeLists.AreaTypes.Codes.Bonded;
				var row = CreateRow(whs, "RR1");
				((WhsRowInternals)row).GenerateLocations();
				row.Locations[0].WLV_WA_PickingArea = area.PK;
				row.Locations[0].WLV_WA_PutawayArea = area.PK;
			}
			else
			{
				RemoveAreasOfTypeFromWarehouse(whs, CodeLists.AreaTypes.Codes.Bonded);
			}
		}

		public void EnableWarehouseForDockDoor(WhsWarehouse whs, bool enable)
		{
			if (enable)
			{
				var area = whs.Areas.AddNew();
				area.WA_Name = WhsWarehouse.DefaultDockDoorAreaName;
				area.WA_AreaType = CodeLists.AreaTypes.Codes.DockDoor;
				var row = CreateRow(whs, WhsWarehouse.DefaultDockDoorRowName);
				((WhsRowInternals)row).GenerateLocations();
				row.Locations[0].WLV_WA_PickingArea = area.PK;
				row.Locations[0].WLV_WA_PutawayArea = area.PK;
			}
			else
			{
				RemoveAreasOfTypeFromWarehouse(whs, CodeLists.AreaTypes.Codes.DockDoor);
			}
		}

		public void EnableWarehouseForExcise(WhsWarehouse whs, bool enable)
			=> EnableWarehouseWithAreaType(whs, AreaTypes.Codes.Excise, enable);

		public void EnableWarehouseForFreeStore(WhsWarehouse whs, bool enable)
			=> EnableWarehouseWithAreaType(whs, AreaTypes.Codes.FreeStore, enable);

		public void EnableWarehouseForInwardProcessing(WhsWarehouse whs, bool enable)
			=> EnableWarehouseWithAreaType(whs, AreaTypes.Codes.InwardProcessing, enable);

		void EnableWarehouseWithAreaType(WhsWarehouse whs, string areaType, bool enable)
		{
			if (enable)
			{
				var area = whs.Areas.AddNew();
				area.WA_Name = areaType;
				area.WA_AreaType = areaType;
			}
			else
			{
				RemoveAreasOfTypeFromWarehouse(whs, areaType);
			}
		}

		public void RemoveAreasOfTypeFromWarehouse(WhsWarehouse whs, string areaType)
		{
			for (int a = whs.Areas.Count - 1; a >= 0; a--)
			{
				var area = whs.Areas[a];
				if (area.WA_AreaType == areaType)
				{
					whs.Areas.RemoveFromRelationship(area);
					area.PickLocations.DeleteAll();
					for (int i = whs.Rows.Count - 1; i >= 0; i--)
					{
						if (whs.Rows[i].Locations.Count == 0)
						{
							whs.Rows.Delete(whs.Rows[i]);
						}
					}

					area.Delete();
				}
			}
		}

		public void ProhibitWarehouseAccessForOrgContact(WhsWarehouse warehouse, OrgContact contact)
		{
			var pivot = Factory.New<GenPivot>();
			pivot.XX_Relation1ID = contact.PK;
			pivot.XX_Relation2ID = warehouse.PK;
			pivot.XX_Relation1TableCode = OrgContactSchema.Constants.Prefix;
			pivot.XX_Relation2TableCode = WhsWarehouseSchema.Constants.Prefix;
			pivot.XX_RelationType = Constants.GenPivotTypes.OrgContactDeniedWarehouse;
		}

		#endregion

		#region Areas

		public WhsArea CreateArea(WhsWarehouse whs, ZString name)
		{
			return CreateArea(whs, name, AreaTypes.Codes.FreeStore);
		}

		public WhsArea CreateArea(WhsWarehouse whs, ZString name, ZString type, bool isPickingArea = true, bool isPutawayArea = true)
		{
			WhsArea area = whs.Areas.AddNew();
			area.WA_WW_Whs = whs.PK;
			area.WA_Name = name;
			area.WA_AreaType = type;
			area.WA_IsPickingArea = isPickingArea;
			area.WA_IsPutawayArea = isPutawayArea;
			return area;
		}

		#endregion

		#region Rows / Locations

		public WhsRow CreateRow(IWhsWarehouse whs, string code)
		{
			var row = (WhsRow)whs.Rows.AddNew();
			row.WR_WW_Whs = whs.PK;
			row.WR_Name = code;
			return row;
		}

		public WhsRow CreateRow(IWhsWarehouse whs, string code, short cols, short levels)
		{
			return CreateRow(whs, code, cols, levels, 1);
		}

		public WhsRow CreateRow(IWhsWarehouse whs, string code, short cols, short levels, short trays, short rowSequence = 0)
		{
			var row = CreateRow(whs, code);
			row.WR_Columns = cols;
			row.WR_Levels = levels;
			row.WR_Trays = trays;
			row.WR_PickPathSequence = rowSequence;
			return row;
		}

		public WhsRow CreateRowAndGenerateLocations(IWhsWarehouse whs, string code, short cols = 1, short levels = 1)
		{
			return CreateRowAndGenerateLocations(whs, code, cols, levels, 1);
		}

		public WhsRow CreateRowAndGenerateLocations(IWhsWarehouse whs, string code, short cols, short levels, short trays, short rowSequence = 0)
		{
			var row = CreateRow(whs, code, cols, levels, trays, rowSequence);
			((IWhsWarehouseInternals)whs).GenerateLocations();
			return row;
		}

		public void SetLocationMaxWeightAndVolume(WhsLocation location, ZDecimal maxWeight, ZString maxWeightUQ, ZDecimal maxVolume, ZString maxVolumeUQ)
		{
			location.WLV_MaxWeight = maxWeight;
			location.WLV_MaxWeightUnit = maxWeightUQ;
			location.WLV_MaxCubic = maxVolume;
			location.WLV_MaxCubicUnit = maxVolumeUQ;
		}

		#region SetLocationArea

		public void SetLocationArea(WhsLocation location, WhsArea pickingArea, WhsArea putawayArea)
		{
			location.WLV_WA_PickingArea = pickingArea.PK;
			location.WLV_WA_PutawayArea = putawayArea.PK;
		}

		#endregion

		#endregion

		#region WhsClientParameterByWarehouse

		public WhsClientParameterByWarehouse CreateWhsClientParameterByWarehouse(OrgHeader client, WhsWarehouse whs)
		{
			return CreateWhsClientParameterByWarehouse(client.PK, whs.PK);
		}

		public WhsClientParameterByWarehouse CreateWhsClientParameterByWarehouse(ZGuid clientPK, ZGuid whsPK)
		{
			var clientParameter = Factory.New<WhsClientParameterByWarehouse>();
			clientParameter.WY_OH_Client = clientPK;
			clientParameter.WY_WW_Whs = whsPK;

			return clientParameter;
		}

		#endregion

		#region Country / Transit Warehouse Zones / UNLOCO

		public RefUNLOCO GetCountryUNLOCO(ZString countryCode)
		{
			RefUNLOCO uNLOCO = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, countryCode) { OrderBy = RefUNLOCOSchema.Constants.RL_Code });
			return uNLOCO;
		}

		public RefUNLOCO CreateUNLOCO(string portCode, string countryCode = "AU")
		{
			var port = Factory.New<RefUNLOCO>();
			port.RL_Code = portCode;
			port.RL_RN_NKCountryCode = countryCode;
			port.RL_IsActive = true;

			return port;
		}

		public RefZoneHeader CreateTransitWarehouseZone(string zoneCode, string zoneDescription)
		{
			var zone = Factory.New<RefZoneHeader>();
			zone.FZ_Code = zoneCode;
			zone.FZ_Description = zoneDescription;
			zone.FZ_ZoneType = RefZoneHeaderLookups.ZoneTypeCodes.TransitWarehouse;
			zone.FZ_IsActive = true;

			return zone;
		}

		#endregion

		#region CreateLocationType

		public WhsLocationType CreateLocationType(ZString code)
		{
			return CreateLocationType(code, LocationClasses.Codes.NOR);
		}

		public WhsLocationType CreateLocationType(ZString code, ZString locationClass)
		{
			return CreateLocationType(code, $"{code} description", true, 0, locationClass);
		}

		public WhsLocationType CreateLocationType(ZString code, ZString description, bool isPalletIDNeutral, int maxNumberOfProducts, ZString locationClasss)
		{
			var locationType = Factory.New<WhsLocationType>();

			locationType.WLT_Code = code;
			locationType.WLT_Description = description;
			locationType.WLT_IsPalletIDNeutral = isPalletIDNeutral;
			locationType.WLT_MaximumNumberOfProducts = maxNumberOfProducts;
			locationType.WLT_LocationClass = locationClasss;

			if (locationClasss == LocationClasses.Codes.PST)
			{
				locationType.WLT_MaximumNumberOfProducts = 0;
				locationType.WLT_MinimumTemperature = 0;
				locationType.WLT_MaximumTemperature = 0;
				locationType.WLT_TemperatureUnit = "";
				locationType.WLT_IsPalletIDNeutral = false;
				locationType.WLT_RetainPalletIDsInFixedPickFaces = false;
			}

			return locationType;
		}

		#endregion

		#region CreateCartonGroup

		public WhsCartonGroup CreateWhsCartonGroup(string code, string description)
		{
			var cartonGroup = Factory.New<WhsCartonGroup>();
			cartonGroup.WCG_Code = code;
			cartonGroup.WCG_Description = description;
			return cartonGroup;
		}

		#endregion

		#region CreateCartonSize

		public WhsCartonSize CreateWhsCartonSize(string name)
		{
			return CreateWhsCartonSize(name, 1m, 1m, 1m, 0.1m, 2m, 1, 85, Constants.Length.Metres, Constants.Weight.Kilograms);
		}

		public WhsCartonSize CreateWhsCartonSize(string name, decimal length, decimal width, decimal height, decimal emptyWeight, decimal maxWeight, int maxUnits, byte maxFillPercent, string dimensionUQ, string weightUQ)
		{
			return CreateWhsCartonSize(name, length, width, height, emptyWeight, maxWeight, maxUnits, maxFillPercent, dimensionUQ, weightUQ, Constants.Volume.CubicMetres);
		}

		public WhsCartonSize CreateWhsCartonSize(string name, decimal length, decimal width, decimal height, decimal emptyWeight, decimal maxWeight, int maxUnits, byte maxFillPercent, string dimensionUQ, string weightUQ, string volumeUQ)
		{
			var cartonSize = Factory.New<WhsCartonSize>();
			cartonSize.WCS_Code = name;
			cartonSize.WCS_VolumeUQ = volumeUQ;
			cartonSize.WCS_DimensionUQ = dimensionUQ;
			cartonSize.WCS_WeightUQ = weightUQ;
			cartonSize.WCS_Length = length;
			cartonSize.WCS_Width = width;
			cartonSize.WCS_Height = height;
			cartonSize.WCS_EmptyWeight = emptyWeight;
			cartonSize.WCS_MaxWeight = maxWeight;
			cartonSize.WCS_MaxUnits = maxUnits;
			cartonSize.WCS_MaxFillPercent = maxFillPercent;
			return cartonSize;
		}

		#endregion

		#region CreateWhsCartonGroupSizeLink

		public WhsCartonGroupSizeLink CreateWhsCartonGroupSizeLink(WhsCartonGroup cartonGroup, WhsCartonSize cartonSize)
		{
			var link = Factory.New<WhsCartonGroupSizeLink>();
			link.WCV_WCG = cartonGroup.PK;
			link.WCV_WCS = cartonSize.PK;
			return link;
		}

		#endregion

		#region CreateWhsInvoice

		public IWhsInvoice CreateWhsInvoice(ZGuid warehousePK, ZGuid clientPK, ZDateTime storageFromDate, ZDateTime storageToDate)
		{
			var invoice = Factory.New<IWhsInvoice>();
			invoice.ET_WW = warehousePK;
			invoice.ET_OH_Client = clientPK;
			invoice.ET_StorageToDate = storageToDate;
			invoice.ET_StorageFromDate = storageFromDate;
			return invoice;
		}

		#endregion

		#region CreateProductStyle

		public WhsProductStyle CreateProductStyle(string code = "", string description = "", ZGuid? clientPK = null)
		{
			var productStyle = Factory.New<WhsProductStyle>();
			productStyle.WST_Code = code;
			productStyle.WST_Description = description;
			if (clientPK.HasValue)
			{
				productStyle.WST_OH_Owner = clientPK.Value;
			}
			return productStyle;
		}

		public WhsProductStyleColour CreateProductStyleColour(WhsProductStyle productStyle, string code, string description)
		{
			var colour = productStyle.Colours.AddNew();
			colour.WSC_Code = code;
			colour.WSC_Description = description;

			return colour;
		}

		public WhsProductStyleClassification CreateProductStyleClassification(WhsProductStyle productStyle, string code, string description)
		{
			var classification = productStyle.Classifications.AddNew();
			classification.WSS_Code = code;
			classification.WSS_Description = description;

			return classification;
		}

		public WhsProductStyleSize CreateProductStyleSize(WhsProductStyle productStyle, ZByte sequence, string sizeCode)
		{
			var size = productStyle.Sizes.AddNew();
			size.WSZ_Sequence = sequence;
			size.WSZ_Size = sizeCode;

			return size;
		}

		#endregion

		#region CreateServiceLevel

		public RefServiceLevel CreateServiceLevel(ZString code, ZString description, bool isActive = true)
		{
			var result = Factory.New<RefServiceLevel>();
			result.RS_Code = code;
			result.RS_Description = description;
			result.RS_IsActive = isActive;

			return result;
		}

		#endregion

		#region CreateReceiveConsignment

		public ITransitReceiveConsignment CreateReceiveConsignment(string consignmentID, ZGuid warehousePK)
		{
			var receiveConsignment = (BusinessObject)Factory.New<ITransitReceiveConsignment>();
			receiveConsignment[WhsItemReceiveConsignmentSchema.WRC_ConsignmentID] = consignmentID;
			receiveConsignment[WhsItemReceiveConsignmentSchema.WRC_JobID] = consignmentID;
			receiveConsignment[WhsItemReceiveConsignmentSchema.WRC_WW_IntendedWarehouse] = warehousePK;
			return (ITransitReceiveConsignment)receiveConsignment;
		}

		#endregion

		#region CreateReceiveHeader

		public IWhsItemReceiveTransportationUnit CreateReceiveTransportationUnit(ZString reference, ZGuid warehousePK, ZGuid locationPK)
		{
			var receiveUnit = (BusinessObject)Factory.New<IWhsItemReceiveTransportationUnit>();
			receiveUnit[WhsItemReceiveTransportationUnitSchema.WRH_ReferenceNumber] = reference;
			receiveUnit[WhsItemReceiveTransportationUnitSchema.WRH_WL_StagingLocation] = locationPK;
			receiveUnit[WhsItemReceiveTransportationUnitSchema.WRH_WW_Warehouse] = warehousePK;
			return (IWhsItemReceiveTransportationUnit)receiveUnit;
		}

		#endregion

		#region CreateDispatchConsignment

		public ITransitDispatchConsignment CreateDispatchConsignment(ZString consignmentID, ZGuid warehousePK)
		{
			var dispatchConsignment = (BusinessObject)Factory.New<ITransitDispatchConsignment>();
			dispatchConsignment[WhsItemDispatchConsignmentSchema.WDC_ConsignmentID] = consignmentID;
			dispatchConsignment[WhsItemDispatchConsignmentSchema.WDC_WW_Warehouse] = warehousePK;
			dispatchConsignment[WhsItemDispatchConsignmentSchema.WDC_JobID] = consignmentID;

			return (ITransitDispatchConsignment)dispatchConsignment;
		}

		#endregion

		#region CreateDispatchHeader

		public IWhsItemDispatchTransportationUnit CreateDispatchTransportationUnit(ZString reference, ZGuid warehousePK)
		{
			var dispatchUnit = (BusinessObject)Factory.New<IWhsItemDispatchTransportationUnit>();
			dispatchUnit[WhsItemDispatchTransportationUnitSchema.WDH_ReferenceNumber] = reference;
			dispatchUnit[WhsItemDispatchTransportationUnitSchema.WDH_WW_Warehouse] = warehousePK;
			return (IWhsItemDispatchTransportationUnit)dispatchUnit;
		}

		#endregion

		#region CreateDispatchLoadList

		public IWhsItemDispatchLoadList CreateDispatchLoadList(ZString reference, ZGuid warehousePK)
		{
			var dispatchLoadList = (BusinessObject)Factory.New<IWhsItemDispatchLoadList>();
			dispatchLoadList[WhsItemDispatchLoadListSchema.WDL_WW_Warehouse] = warehousePK;
			dispatchLoadList[WhsItemDispatchLoadListSchema.WDL_JobID] = reference;
			dispatchLoadList[WhsItemDispatchLoadListSchema.WDL_ReferenceNumber] = reference;
			return (IWhsItemDispatchLoadList)dispatchLoadList;
		}

		#endregion

		#region CreatePackageJob

		public IPkgPackageJob CreatePackageJob(BusinessObject parent)
		{
			var packageJob = (BusinessObject)Factory.New<IPkgPackageJob>();
			packageJob[PkgPackageJobSchema.KJ_ParentID] = parent.PK;
			packageJob[PkgPackageJobSchema.KJ_ParentTableCode] = parent.TablePrefix;
			packageJob[PkgPackageJobSchema.KJ_IsFinalized] = false;
			packageJob[PkgPackageJobSchema.KJ_ReleasedTimeUtc] = ZDateTime.Empty;
			return (IPkgPackageJob)packageJob;
		}

		#endregion

		#region CreatePackage

		public IPkgPackage CreatePackage(IPkgPackageJob packageJob)
		{
			var pj = (BusinessObject)packageJob;
			var package = (BusinessObject)Factory.New<IPkgPackage>();
			package[PkgPackageSchema.KP_KJ_ParentPackageJob] = pj[PkgPackageJobSchema.PK];
			package[PkgPackageSchema.KP_MarksAndNumbers] = "Marks";
			package[PkgPackageSchema.KP_F3_NKPackType] = Constants.PkgUnit.Carton;
			package[PkgPackageSchema.KP_Weight] = 10m;
			package[PkgPackageSchema.KP_Length] = 20m;
			package[PkgPackageSchema.KP_Width] = 30m;
			package[PkgPackageSchema.KP_Height] = 40m;
			package[PkgPackageSchema.KP_Volume] = 50m;

			return (IPkgPackage)package;
		}

		#endregion

		#region CreateWhsItemPackageState

		public BusinessObject CreateWhsItemPackageState(string status, WhsLocation location, BusinessObject rtu, BusinessObject package)
		{
			var packageState = (BusinessObject)Factory.New<IWhsItemPackageState>();
			packageState[WhsItemPackageStateSchema.WPS_Status] = status;
			packageState[WhsItemPackageStateSchema.WPS_WL_LastLocation] = location.PK;
			packageState[WhsItemPackageStateSchema.WPS_WRH_TransitReceiveHeader] = rtu.PK;
			packageState[WhsItemPackageStateSchema.WPS_KP_Package] = package.PK;
			packageState[WhsItemPackageStateSchema.WPS_WW_Warehouse] = location.WLV_WW_Whs;
			packageState[WhsItemPackageStateSchema.WPS_UnloadedTime] = ZDateTimeOffset.Now;
			return packageState;
		}

		#endregion

		#region CreateTransferHeader
		public IWhsItemTransferHeader CreateTransferHeader(string referenceNumber, IWhsWarehouse warehouse, bool isFinalised)
		{
			var transfer = (BusinessObject)Factory.New<IWhsItemTransferHeader>();
			transfer[WhsItemTransferHeaderSchema.WTH_ReferenceNumber] = referenceNumber;
			transfer[WhsItemTransferHeaderSchema.WTH_WW_Warehouse] = warehouse.PK;
			transfer[WhsItemTransferHeaderSchema.WTH_IsFinalised] = isFinalised;
			transfer[WhsItemTransferHeaderSchema.WTH_TransferType] = "TRF";

			return (IWhsItemTransferHeader)transfer;
		}

		#endregion

		#region CreateTransferLine
		public IWhsItemTransferLine CreateTransferLine(IWhsItemTransferHeader transfer, IWhsLocation from, IWhsLocation to, IWhsItemPackageState packageState, ZDateTime? pickTime = null, string pickUser = "")
		{
			var transferHeader = (BusinessObject)transfer;
			var ps = (BusinessObject)packageState;
			var transferLine = (BusinessObject)Factory.New<IWhsItemTransferLine>();
			transferLine[WhsItemTransferLineSchema.WTF_WTH_TransitTransferHeader] = transferHeader[WhsItemTransferHeaderSchema.PK];
			transferLine[WhsItemTransferLineSchema.WTF_WL_From] = from.PK;
			transferLine[WhsItemTransferLineSchema.WTF_WL_To] = to.PK;
			transferLine[WhsItemTransferLineSchema.WTF_WPS_PackageState] = ps[WhsItemPackageStateSchema.PK];

			if (pickTime.HasValue)
			{
				transferLine[WhsItemTransferLineSchema.WTF_PickTime] = pickTime.Value;
				transferLine[WhsItemTransferLineSchema.WTF_GS_NKPickUser] = pickUser;
			}

			return (IWhsItemTransferLine)transferLine;
		}

		#endregion

		#region CreatePutawayGroup

		public WhsPutawayGroup CreatePutawayGroup(string code, string description)
		{
			var putawayGroup = Factory.New<WhsPutawayGroup>();
			putawayGroup.WPG_Code = code;
			putawayGroup.WPG_Description = description;

			return putawayGroup;
		}

		#endregion

		#region CreateWhsUNDGLimit

		public WhsUNDGLimit CreateWhsUNDGLimit(
			WhsWarehouse warehouse,
			string undgSubstanceCode = "",
			string undgClass = "",
			UNDGCountryReference countryReference = null,
			decimal totalWeightLimit = 10m,
			string totalWeightLimitUQ = Constants.Weight.Kilograms,
			decimal totalVolumeLimit = 10m,
			string totalVolumeLimitUQ = Constants.Volume.CubicMetres)
		{
			UNDGSubstance substance = null;
			if (!string.IsNullOrEmpty(undgSubstanceCode))
			{
				var unno = ((ZString)undgSubstanceCode).SubstringSafe(0, 4);
				var variant = ((ZString)undgSubstanceCode).SubstringSafe(4, 2);
				var standard = UNDGSubstanceStandardTypes.IMO;
				substance = UNDGSubstanceLoader.LoadSubstances(Factory, unno, variant, standard).First();
			}

			return CreateWhsUNDGLimit(warehouse, substance, undgClass, countryReference, totalWeightLimit, totalWeightLimitUQ, totalVolumeLimit, totalVolumeLimitUQ);
		}

		public WhsUNDGLimit CreateWhsUNDGLimit(
			WhsWarehouse warehouse,
			UNDGSubstance substance,
			string undgClass = "",
			UNDGCountryReference countryReference = null,
			decimal totalWeightLimit = 10m,
			string totalWeightLimitUQ = Constants.Weight.Kilograms,
			decimal totalVolumeLimit = 10m,
			string totalVolumeLimitUQ = Constants.Volume.CubicMetres)
		{
			var whsUNDGLimit = Factory.New<WhsUNDGLimit>();
			whsUNDGLimit.WWD_WW_Warehouse = warehouse.PK;

			if (substance != null)
			{
				whsUNDGLimit.WWD_DG = substance.PK;
			}

			if (countryReference != null)
			{
				whsUNDGLimit.WWD_DCR_UNDGCountryReference = countryReference.PK;
			}

			if (!string.IsNullOrEmpty(undgClass))
			{
				whsUNDGLimit.WWD_UNDGClass = undgClass;
			}

			whsUNDGLimit.WWD_TotalWeightLimit = totalWeightLimit;
			whsUNDGLimit.WWD_TotalVolumeLimit = totalVolumeLimit;
			whsUNDGLimit.WWD_TotalWeightLimitUQ = totalWeightLimitUQ;
			whsUNDGLimit.WWD_TotalVolumeLimitUQ = totalVolumeLimitUQ;

			return whsUNDGLimit;
		}

		#endregion

		#region CreateUNDGSubstance

		public UNDGSubstance CreateUNDGSubstance(string undgUNNOCode, string undgClass, string undgCode, string undgPSN = "", decimal lqMaxAmt = 0m, string lqMaxAmtUQ = Constants.Weight.Kilograms, string undgStandard = "IMO", string undgMode = "")
		{
			var undgSubstance = Factory.NewWithValidTestData<UNDGSubstance>();
			undgSubstance.DG_UNNO = undgUNNOCode;
			undgSubstance.DG_Class = undgClass;
			undgSubstance.DG_PSN = undgPSN;
			undgSubstance.DG_LQMaxAmt = lqMaxAmt;
			undgSubstance.DG_Code = undgCode;
			undgSubstance.DG_Standard = undgStandard;
			undgSubstance.DG_Mode = undgMode;

			return undgSubstance;
		}

		#endregion

		#region CreateUNDGDataItem

		public UNDGDataItem CreateUNDGDataItem(ZGuid parentID, string parentTableCode, UNDGSubstance undgSubstance, decimal weight, decimal volume, string unitOfWeight = Constants.Weight.Kilograms, string unitOfVolume = Constants.Volume.CubicMetres)
		{
			var undgDataItem = Factory.NewWithValidTestData<UNDGDataItem>();
			undgDataItem.DI_ParentID = parentID;
			undgDataItem.DI_ParentTableCode = parentTableCode;
			undgDataItem.DI_DG = undgSubstance.PK;
			undgDataItem.DI_IMOClass = undgSubstance.DG_Class;
			undgDataItem.DI_DGWeight = weight;
			undgDataItem.DI_UnitOfWeight = unitOfWeight;
			undgDataItem.DI_DGVolume = volume;
			undgDataItem.DI_UnitOfVolume = unitOfVolume;

			return undgDataItem;
		}

		#endregion

		#region CreateCountryReference

		public UNDGCountryReference CreateCountryReference(string referenceCode = "1234", string country = "FR", string type = "ICPE")
		{
			var countryReference = Factory.New<UNDGCountryReference>();
			countryReference.DCR_Code = referenceCode;
			countryReference.DCR_RN_NKCountry = country;
			countryReference.DCR_Type = type;
			countryReference.DCR_Description = referenceCode;

			return countryReference;
		}

		#endregion

		#region CreateCountryReferencePivot

		public UNDGCountryReferencePivot CreateCountryReferencePivot(UNDGCountryReference countryReference, string dgCode)
		{
			var countryReferencePivot = Factory.New<UNDGCountryReferencePivot>();
			var unno = ((ZString)dgCode).SubstringSafe(0, 4);
			var variant = ((ZString)dgCode).SubstringSafe(4, 2);
			var standard = UNDGSubstanceStandardTypes.IMO;
			var substance = UNDGSubstanceLoader.LoadSubstances(Factory, unno, variant, standard).First();
			countryReferencePivot.DCP_DCR = countryReference.PK;
			countryReferencePivot.DCP_UNNO = substance.DG_UNNO;
			countryReferencePivot.DCP_Variant = substance.DG_Variant;
			countryReferencePivot.DCP_Standard = substance.DG_Standard;

			return countryReferencePivot;
		}

		public UNDGCountryReferencePivot CreateCountryReferencePivot(UNDGCountryReference countryReference, UNDGSubstance substance)
		{
			var countryReferencePivot = Factory.New<UNDGCountryReferencePivot>();
			countryReferencePivot.DCP_DCR = countryReference.PK;
			countryReferencePivot.DCP_UNNO = substance.DG_UNNO;
			countryReferencePivot.DCP_Variant = substance.DG_Variant;
			countryReferencePivot.DCP_Standard = substance.DG_Standard;

			return countryReferencePivot;
		}

		#endregion

		#region CreateWhsSalesChannel

		public WhsSalesChannel CreateWhsSalesChannel(string code, string description)
		{
			var whsSalesChannel = Factory.New<WhsSalesChannel>();
			whsSalesChannel.WSH_Code = code;
			whsSalesChannel.WSH_Description = description;

			return whsSalesChannel;
		}

		#endregion

		#region CreateWhsRFRegistry

		public WhsRFRegistry CreateWhsRFRegistry(GlbStaff staff, WhsWarehouse warehouse)
		{
			var rfRegistry = Factory.New<WhsRFRegistry>();
			rfRegistry.WRR_GS_NKAssignedTo = staff.GS_Code;
			rfRegistry.WRR_WW_Whs = warehouse.PK;

			return rfRegistry;
		}

		#endregion

		#region Implementation

		protected string LocnToString(ZGuid location)
		{
			return Factory.Load<WhsLocation>(location).ToLocationString();
		}

		#endregion

		#region Property

		TestNotificationBuffer fNotify;
		public TestNotificationBuffer Notify
		{
			get
			{
				if (fNotify == null)
				{
					fNotify = new TestNotificationBuffer();
				}
				return fNotify;
			}
		}

		#endregion
	}
}
