using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.ComponentModel.Design;
using CargoWise.Types;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core.Forms;
using Enterprise.Freight.Integration;
#if WINZOR
using Enterprise.Freight.Business;
#endif
using Enterprise.Rating.Business;
using Enterprise.Rating.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Rating.GUI
{
	[CodeAlive("Type and derived types are loaded using reflection for use in GUI")]
	public abstract class RateEntryCollectionGUIInfo
	{
		#region Columns and Tab Heading

		public Tuple<string, ResourceStringData> GroupData
		{
			get { return new Tuple<string, ResourceStringData>(Groups.GetGroupNameByCategory(Category), Groups.GetGroupCaptionByCategory(Category)); }
		}

		public abstract List<ZGridColumnInfo> Columns { get; }
		public string Text { get { return RateCategoriesList.GetRateCategoriesList().GetDescriptionFromCode(Category) ?? ""; } }
		public abstract string Category { get; }

		internal static string[] CategoriesShowingCarrierContractNumberInRevenue => new[]
		{
			RatingConstants.RateCategory.SCO,
			RatingConstants.RateCategory.SNC,
			RatingConstants.RateCategory.SOR,
			RatingConstants.RateCategory.SDE,
			RatingConstants.RateCategory.SED,
			RatingConstants.RateCategory.SID
		};

		#endregion

		#region GetInfo

		#region SuppressResourceStringsCheckRegion

		internal static RateEntryCollectionGUIInfo GetInfo(string category, Type dataSourceType, bool isGlobal, bool isStandardCosting = false)
		{
			if (string.IsNullOrEmpty(category))
			{
				category = "Summary";
			}

			var type = Type.GetType("Enterprise.Rating.GUI." + category + "RateEntryCollectionGUIInfo");
			if (type == null)
			{
				return null;
			}

			var info = (RateEntryCollectionGUIInfo)Activator.CreateInstance(type);
			if (info != null)
			{
				info.dataSourceType = dataSourceType;
				info.isGlobal = isGlobal;
				info.IsStandardCosting = isStandardCosting;

				return info;
			}

			return null;
		}

		#endregion

		#endregion

		#region DataSource

		protected bool IsQuote
		{
			get { return typeof(Quote).IsAssignableFrom(dataSourceType); }
		}

		public bool IsCosting
		{
			get { return typeof(Costing).IsAssignableFrom(dataSourceType); }
		}

		public bool IsClientRate
		{
			get { return typeof(ClientRate).IsAssignableFrom(dataSourceType) && !IsCosting; }
		}

		public bool IsTariff
		{
			get { return typeof(CompanyTariff).IsAssignableFrom(dataSourceType); }
		}

		public bool IsIntercompanyTariff
		{
			get { return typeof(IntercompanyTariff).IsAssignableFrom(dataSourceType); }
		}

		public bool IsMultiModalRateSearch
		{
			get { return typeof(WiseRatingHeaderView).IsAssignableFrom(dataSourceType); }
		}

		public bool IsFMCTariffIDAllowed
		{
			get { return RatingHeader.IsFMCTariffAllowed(dataSourceType); }
		}

		public bool IsCustoms => RatingConstants.RateCategory.GetRateType(Category) == RateType.Customs;

		protected bool IsStandardCosting { get; private set; }

		Type dataSourceType;
		protected bool isGlobal;

		protected bool ShowIsPublishedColumn => !isGlobal && !IsQuote;

		#endregion

		#region Common Columns

		protected ZDropEditColumnStyleInfo AircraftType()
		{
			CompileTimeCheckBindingMember.Check(((RateEntry)(null)).TI_AircraftType);
			ZDropEditColumnStyleInfo aircraftType = new ZDropEditColumnStyleInfo();
			aircraftType.ColumnName = AutoRateEntry.Schema.TI_AircraftType;
			ControlDpiScalingHelper.SetWidth(ref aircraftType, 30, true);

			return aircraftType;
		}

		protected ZDropEditColumnStyleInfo GatewayAgentType()
		{
			CompileTimeCheckBindingMember.Check(((RateEntry)(null)).TI_GatewayAgentType);
			ZDropEditColumnStyleInfo gatewayAgent = new ZDropEditColumnStyleInfo();
			gatewayAgent.ColumnName = AutoRateEntry.Schema.TI_GatewayAgentType;
			ControlDpiScalingHelper.SetWidth(ref gatewayAgent, 45, true);

			return gatewayAgent;
		}

		protected ZDropEditColumnStyleInfo ShipmentConsolidationStatus()
		{
			CompileTimeCheckBindingMember.Check(((RateEntry)(null)).TI_ShipmentConsolidationStatus);
			ZDropEditColumnStyleInfo shipmentConsolidationStatus = new ZDropEditColumnStyleInfo();
			shipmentConsolidationStatus.ColumnName = AutoRateEntry.Schema.TI_ShipmentConsolidationStatus;
			ControlDpiScalingHelper.SetWidth(ref shipmentConsolidationStatus, 45, true);

			return shipmentConsolidationStatus;
		}

		protected ZDropEditColumnStyleInfo HBLDeliveryMode()
		{
			CompileTimeCheckBindingMember.Check(((RateEntry)(null)).TI_HBLDeliveryMode);
			var hblDeliveryMode = new ZDropEditColumnStyleInfo();
			hblDeliveryMode.ColumnName = AutoRateEntry.Schema.TI_HBLDeliveryMode;
			ControlDpiScalingHelper.SetWidth(ref hblDeliveryMode, 45, true);

			return hblDeliveryMode;
		}

		protected ZCodeFindBoxColumnStyleInfo Origin()
		{
			CompileTimeCheckBindingMember.Check(((RateEntry)(null)).TI_OriginLRC);
			var origin = new ZCodeFindBoxColumnStyleInfo();
			origin.ColumnName = AutoRateEntry.Schema.TI_OriginLRC;
			ControlDpiScalingHelper.SetWidth(ref origin, 60, true);

			return origin;
		}

		protected ZCodeFindBoxColumnStyleInfo RateOrigin()
		{
			CompileTimeCheckBindingMember.Check(((RateEntry)(null)).TI_RateOrigin);
			var rateOrigin = new ZCodeFindBoxColumnStyleInfo();
			rateOrigin.ColumnName = AutoRateEntry.Schema.TI_RateOrigin;
			ControlDpiScalingHelper.SetWidth(ref rateOrigin, 60, true);

			return rateOrigin;
		}

		protected ZGuidFindBoxColumnStyleInfo OriginZone()
		{
			CompileTimeCheckBindingMember.Check(((RateEntry)(null)).TI_TZ_OriginZone);
			var newStyleInfo = new ZGuidFindBoxColumnStyleInfo
			{
				ColumnName = RateEntry.Schema.TI_TZ_OriginZone,
				IsVisible = false
			};
			ControlDpiScalingHelper.SetWidth(newStyleInfo, 60, true);

			return newStyleInfo;
		}

		protected ZGuidFindBoxColumnStyleInfo OriginSuburb()
		{
			CompileTimeCheckBindingMember.Check(((RateEntry)(null)).OriginSuburbPK);
			var newStyleInfo = new ZGuidFindBoxColumnStyleInfo
			{
				ColumnName = RateEntry.Schema.OriginSuburbPK,
				IsVisible = false
			};
			ControlDpiScalingHelper.SetWidth(newStyleInfo, 60, true);

			return newStyleInfo;
		}

		protected ZCodeFindBoxColumnStyleInfo Destination()
		{
			CompileTimeCheckBindingMember.Check(((RateEntry)(null)).TI_DestinationLRC);
			ZCodeFindBoxColumnStyleInfo destination = new ZCodeFindBoxColumnStyleInfo();
			destination.ColumnName = AutoRateEntry.Schema.TI_DestinationLRC;
			ControlDpiScalingHelper.SetWidth(ref destination, 60, true);

			return destination;
		}

		protected ZCodeFindBoxColumnStyleInfo RateDestination()
		{
			CompileTimeCheckBindingMember.Check(((RateEntry)(null)).TI_RateDestination);
			ZCodeFindBoxColumnStyleInfo rateDestination = new ZCodeFindBoxColumnStyleInfo();
			rateDestination.ColumnName = AutoRateEntry.Schema.TI_RateDestination;
			ControlDpiScalingHelper.SetWidth(ref rateDestination, 60, true);

			return rateDestination;
		}

		protected ZGuidFindBoxColumnStyleInfo DestinationZone()
		{
			CompileTimeCheckBindingMember.Check(((RateEntry)(null)).TI_TZ_DestinationZone);
			var newStyleInfo = new ZGuidFindBoxColumnStyleInfo
			{
				ColumnName = RateEntry.Schema.TI_TZ_DestinationZone,
				IsVisible = false
			};
			ControlDpiScalingHelper.SetWidth(newStyleInfo, 60, true);

			return newStyleInfo;
		}

		protected ZGuidFindBoxColumnStyleInfo DestinationSuburb()
		{
			CompileTimeCheckBindingMember.Check(((RateEntry)(null)).DestinationSuburbPK);
			var newStyleInfo = new ZGuidFindBoxColumnStyleInfo
			{
				ColumnName = RateEntry.Schema.DestinationSuburbPK,
				IsVisible = false
			};
			ControlDpiScalingHelper.SetWidth(newStyleInfo, 60, true);

			return newStyleInfo;
		}

		protected ZCheckBoxColumnStyleInfo CrossTrade()
		{
			CompileTimeCheckBindingMember.Check(((RateEntry)(null)).TI_IsCrossTrade);
			ZCheckBoxColumnStyleInfo crossTrade = new ZCheckBoxColumnStyleInfo();
			crossTrade.ColumnName = AutoRateEntry.Schema.TI_IsCrossTrade;
			ControlDpiScalingHelper.SetWidth(ref crossTrade, 40, true);

			return crossTrade;
		}

		protected ZCodeFindBoxColumnStyleInfo Via()
		{
			CompileTimeCheckBindingMember.Check(((RateEntry)(null)).TI_ViaLRC);
			ZCodeFindBoxColumnStyleInfo via = new ZCodeFindBoxColumnStyleInfo();
			via.ColumnName = AutoRateEntry.Schema.TI_ViaLRC;
			ControlDpiScalingHelper.SetWidth(ref via, 45, true);

			return via;
		}

		protected ZCodeFindBoxColumnStyleInfo PlannedLoad()
		{
			CompileTimeCheckBindingMember.Check(((RateEntry)(null)).TI_PlannedLoadLRC);
			ZCodeFindBoxColumnStyleInfo plannedLoad = new ZCodeFindBoxColumnStyleInfo();
			plannedLoad.ColumnName = AutoRateEntry.Schema.TI_PlannedLoadLRC;
			ControlDpiScalingHelper.SetWidth(ref plannedLoad, 100, true);

			return plannedLoad;
		}

		protected ZCodeFindBoxColumnStyleInfo PlannedDischarge()
		{
			CompileTimeCheckBindingMember.Check(((RateEntry)(null)).TI_PlannedDischargeLRC);
			ZCodeFindBoxColumnStyleInfo plannedDischarge = new ZCodeFindBoxColumnStyleInfo();
			plannedDischarge.ColumnName = AutoRateEntry.Schema.TI_PlannedDischargeLRC;
			ControlDpiScalingHelper.SetWidth(ref plannedDischarge, 100, true);

			return plannedDischarge;
		}

		protected ZCodeFindBoxColumnStyleInfo Currency()
		{
			CompileTimeCheckBindingMember.Check(((RateEntry)(null)).TI_RX_NKCurrency);
			ZCodeFindBoxColumnStyleInfo currency = new ZCodeFindBoxColumnStyleInfo();
			currency.ColumnName = AutoRateEntry.Schema.TI_RX_NKCurrency;
			ControlDpiScalingHelper.SetWidth(ref currency, 45, true);
			return currency;
		}

		protected ZTextBoxColumnStyleInfo WiseCarrierCode()
		{
			CompileTimeCheckBindingMember.Check(((WiseEntryView)(null)).CarrierCode);
			var carrierInfo = new ZTextBoxColumnStyleInfo();
			carrierInfo.ColumnName = nameof(WiseEntryView.CarrierCode);
			ControlDpiScalingHelper.SetWidth(ref carrierInfo, 70, true);

			return carrierInfo;
		}

		protected ZGuidFindBoxColumnStyleInfo Carrier(string bindToList)
		{
			CompileTimeCheckBindingMember.Check(((RateEntry)(null)).TI_OH_TransportProvider);
			ZGuidFindBoxColumnStyleInfo carrierInfo = new ZGuidFindBoxColumnStyleInfo();
			carrierInfo.ColumnName = AutoRateEntry.Schema.TI_OH_TransportProvider;
			carrierInfo.BindToList = bindToList;
			ControlDpiScalingHelper.SetWidth(ref carrierInfo, 70, true);

			return carrierInfo;
		}

		protected ZTextBoxColumnStyleInfo AirlineCode()
		{
			CompileTimeCheckBindingMember.Check(((RateEntry)(null)).TransportProviderCarrierCode);
			ZTextBoxColumnStyleInfo airlineCode = new ZTextBoxColumnStyleInfo();
			airlineCode.ColumnName = RateEntry.Schema.TransportProviderCarrierCode;
			airlineCode.CharacterCasing = CharacterCasing.Upper;
			ControlDpiScalingHelper.SetWidth(ref airlineCode, 70, true);

			return airlineCode;
		}

		protected ZGuidDropEditColumnStyleInfo RefContainerComponent()
		{
			CompileTimeCheckBindingMember.Check(((RateEntry)(null)).TI_RCC_ComponentCode);
			var containerComponent = new ZGuidDropEditColumnStyleInfo();
			containerComponent.ColumnName = AutoRateEntry.Schema.TI_RCC_ComponentCode;
			ControlDpiScalingHelper.SetWidth(ref containerComponent, 70, true);

			return containerComponent;
		}

		protected ZDropEditColumnStyleInfo RefUnitSection()
		{
			CompileTimeCheckBindingMember.Check(((RateEntry)(null)).TI_ContainerUnitSection);
			var containerLocations = new ZDropEditColumnStyleInfo();
			containerLocations.ColumnName = AutoRateEntry.Schema.TI_ContainerUnitSection;
			containerLocations.ShowInDropDown = ZDropEdit.ShowInDropDownList.OnlyShowCode;
			ControlDpiScalingHelper.SetWidth(ref containerLocations, 70, true);

			return containerLocations;
		}

		protected ZGuidDropEditColumnStyleInfo RefContainerRepair()
		{
			CompileTimeCheckBindingMember.Check(((RateEntry)(null)).TI_RRC_RepairCode);
			var containerRepair = new ZGuidDropEditColumnStyleInfo();
			containerRepair.ColumnName = AutoRateEntry.Schema.TI_RRC_RepairCode;
			ControlDpiScalingHelper.SetWidth(ref containerRepair, 70, true);

			return containerRepair;
		}

		protected ZGuidDropEditColumnStyleInfo RefContainerMaterial()
		{
			CompileTimeCheckBindingMember.Check(((RateEntry)(null)).TI_RMC_Material);
			var containerMaterial = new ZGuidDropEditColumnStyleInfo();
			containerMaterial.ColumnName = AutoRateEntry.Schema.TI_RMC_Material;
			ControlDpiScalingHelper.SetWidth(ref containerMaterial, 70, true);

			return containerMaterial;
		}

		protected ZTextBoxColumnStyleInfo CreationSource()
		{
			CompileTimeCheckBindingMember.Check(((RateEntry)(null)).TI_CreationSource);
			ZTextBoxColumnStyleInfo creationSource = new ZTextBoxColumnStyleInfo();
			creationSource.ColumnName = RateEntry.Schema.TI_CreationSource;
			creationSource.CharacterCasing = CharacterCasing.Upper;
			creationSource.IsReadOnly = true;
			ControlDpiScalingHelper.SetWidth(ref creationSource, 40, true);

			return creationSource;
		}

		protected ZGuidFindBoxColumnStyleInfo Container()
		{
			CompileTimeCheckBindingMember.Check(((RateEntry)(null)).TI_RC);
			ZGuidFindBoxColumnStyleInfo container = new ZGuidFindBoxColumnStyleInfo();
			container.ColumnName = AutoRateEntry.Schema.TI_RC;
			ControlDpiScalingHelper.SetWidth(ref container, 75, true);

			return container;
		}

		protected ZCheckBoxColumnStyleInfo MatchContainerClass()
		{
			CompileTimeCheckBindingMember.Check(((RateEntry)(null)).TI_MatchContainerRateClass);
			ZCheckBoxColumnStyleInfo matchContainerClass = new ZCheckBoxColumnStyleInfo();
			matchContainerClass.ColumnName = AutoRateEntry.Schema.TI_MatchContainerRateClass;
			ControlDpiScalingHelper.SetWidth(ref matchContainerClass, 40, true);

			return matchContainerClass;
		}

		protected ZCodeFindBoxColumnStyleInfo ServiceLevel()
		{
			CompileTimeCheckBindingMember.Check(((RateEntry)(null)).TI_RS_NKServiceLevel_NI);
			ZCodeFindBoxColumnStyleInfo serviceLevel = new ZCodeFindBoxColumnStyleInfo();
			serviceLevel.ColumnName = AutoRateEntry.Schema.TI_RS_NKServiceLevel_NI;
			ControlDpiScalingHelper.SetWidth(ref serviceLevel, 45, true);

			return serviceLevel;
		}

		protected ZDropEditColumnStyleInfo CarrierServiceLevel()
		{
			CompileTimeCheckBindingMember.Check(((RateEntry)(null)).TI_PL_NKCarrierServiceLevel);
			ZDropEditColumnStyleInfo serviceLevel = new ZDropEditColumnStyleInfo();
			serviceLevel.ColumnName = AutoRateEntry.Schema.TI_PL_NKCarrierServiceLevel;
			ControlDpiScalingHelper.SetWidth(ref serviceLevel, 45, true);

			return serviceLevel;
		}

		protected ZCodeFindBoxColumnStyleInfo GatewayServiceLevel()
		{
			CompileTimeCheckBindingMember.Check(((RateEntry)(null)).TI_RS_NKGatewayServiceLevel);
			var serviceLevel = new ZCodeFindBoxColumnStyleInfo();
			serviceLevel.ColumnName = AutoRateEntry.Schema.TI_RS_NKGatewayServiceLevel;
			ControlDpiScalingHelper.SetWidth(ref serviceLevel, 45, true);

			return serviceLevel;
		}

		protected ZCodeFindBoxColumnStyleInfo ShipmentGatewayServiceLevel()
		{
			CompileTimeCheckBindingMember.Check(((RateEntry)(null)).TI_RS_NKShipmentGatewayServiceLevel);
			var serviceLevel = new ZCodeFindBoxColumnStyleInfo();
			serviceLevel.ColumnName = AutoRateEntry.Schema.TI_RS_NKShipmentGatewayServiceLevel;
			ControlDpiScalingHelper.SetWidth(ref serviceLevel, 45, true);

			return serviceLevel;
		}

		protected ZCodeFindBoxColumnStyleInfo CommodityCode()
		{
			CompileTimeCheckBindingMember.Check(((RateEntry)(null)).TI_RH_NKCommodityCode);
			ZCodeFindBoxColumnStyleInfo commodityCode = new ZCodeFindBoxColumnStyleInfo();
			commodityCode.ColumnName = AutoRateEntry.Schema.TI_RH_NKCommodityCode;
			ControlDpiScalingHelper.SetWidth(ref commodityCode, 45, true);

			return commodityCode;
		}

		protected ZTextBoxColumnStyleInfo CommodityDescription()
		{
			CompileTimeCheckBindingMember.Check(((RateEntry)(null)).CommodityCode.RH_DescriptionMultilingual);
			var commodityDescription = new ZTextBoxColumnStyleInfo(RateEntry.Schema.CommodityDescription, 120)
			{
				IsReadOnly = true,
				CharacterCasing = CharacterCasing.Normal
			};
			commodityDescription.CaptionResourceString = Res.GetData("88CA0691-8709-4CCE-8F50-26A70AD3B5CF", "Comm. Desc.", "Commodity Description", "Description of the commodity of this rate");
			return commodityDescription;
		}

		protected ZTextBoxColumnStyleInfo CommodityGroup()
		{
			CompileTimeCheckBindingMember.Check(((WiseEntryView)(null)).CommodityGroup);
			var commodityGroup = new ZTextBoxColumnStyleInfo(nameof(WiseEntryView.CommodityGroup), 80)
			{
				IsReadOnly = true,
				CharacterCasing = CharacterCasing.Normal
			};

			return commodityGroup;
		}

		protected ZTextBoxColumnStyleInfo CommodityLocalCode()
		{
			CompileTimeCheckBindingMember.Check(((RateEntry)(null)).CommodityCode.RatingLocalCode.LC_LocalCode);
			return new ZTextBoxColumnStyleInfo(RateEntry.Schema.CommodityLocalCode, 80)
			{
				CaptionResourceString = Res.GetData("65ac720d-e037-4e81-a719-a3a17f24824b", "Comm. Local", "Commodity Local Code", "The Rating Local Code of the commodity of this rate."),
				IsReadOnly = true
			};
		}

		protected ZTextBoxColumnStyleInfo FMCTariffID()
		{
			CompileTimeCheckBindingMember.Check(((RateEntry)(null)).TI_FMCTariffID);
			ZTextBoxColumnStyleInfo tariffLineItem = new ZTextBoxColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("51aeaaa3-cd63-4769-a3cd-de0dfea4fc1b", "FMC TID", "FMC Tariff ID", "The FMC Tariff ID of this rate."),
				ColumnName = RateEntry.Schema.TI_FMCTariffID,
				CharacterCasing = CharacterCasing.Upper
			};
			ControlDpiScalingHelper.SetWidth(ref tariffLineItem, 50, true);
			return tariffLineItem;
		}

		protected ZTextBoxColumnStyleInfo ProductName()
		{
			CompileTimeCheckBindingMember.Check(((WiseEntryView)(null)).ProductName);
			var productName = new ZTextBoxColumnStyleInfo(nameof(WiseEntryView.ProductName), 80)
			{
				IsReadOnly = true,
				CharacterCasing = CharacterCasing.Normal
			};

			return productName;
		}

		protected ZTextBoxColumnStyleInfo Commodities()
		{
			CompileTimeCheckBindingMember.Check(((WiseEntryView)(null)).Commodities);
			var commodities = new ZTextBoxColumnStyleInfo(nameof(WiseEntryView.Commodities), 80)
			{
				IsReadOnly = true,
				CharacterCasing = CharacterCasing.Normal
			};

			return commodities;
		}

		protected ZDropEditColumnStyleInfo Unit()
		{
			CompileTimeCheckBindingMember.Check(((RateEntry)(null)).Unit);
			ZDropEditColumnStyleInfo unit = new ZDropEditColumnStyleInfo();
			unit.ColumnName = RateEntry.Schema.Unit;
			ControlDpiScalingHelper.SetWidth(ref unit, 30, true);

			return unit;
		}

		protected ZDropEditColumnStyleInfo TransitTime(string bindToList)
		{
			CompileTimeCheckBindingMember.Check(((RateEntry)(null)).TI_TransitTime);
			ZDropEditColumnStyleInfo transitTime = new ZDropEditColumnStyleInfo();
			transitTime.ColumnName = AutoRateEntry.Schema.TI_TransitTime;
			transitTime.BindToList = bindToList;
			ControlDpiScalingHelper.SetWidth(ref transitTime, 45, true);

			return transitTime;
		}

		protected ZCalcEditColumnStyleInfo Frequency()
		{
			CompileTimeCheckBindingMember.Check(((RateEntry)(null)).TI_Frequency);
			ZCalcEditColumnStyleInfo frequency = new ZCalcEditColumnStyleInfo();
			frequency.ColumnName = AutoRateEntry.Schema.TI_Frequency;
			frequency.Decimals = 0;
			ControlDpiScalingHelper.SetWidth(ref frequency, 30, true);

			return frequency;
		}

		protected ZDropEditColumnStyleInfo FrequencyUnit()
		{
			CompileTimeCheckBindingMember.Check(((RateEntry)(null)).TI_FrequencyUnit);
			ZDropEditColumnStyleInfo frequencyUnit = new ZDropEditColumnStyleInfo();
			frequencyUnit.ColumnName = AutoRateEntry.Schema.TI_FrequencyUnit;
			ControlDpiScalingHelper.SetWidth(ref frequencyUnit, 60, true);

			return frequencyUnit;
		}

		protected ZGuidFindBoxColumnStyleInfo Consignor()
		{
			CompileTimeCheckBindingMember.Check(((RateEntry)(null)).TI_OH_Consignor);
			ZGuidFindBoxColumnStyleInfo consignor = new ZGuidFindBoxColumnStyleInfo();
			consignor.ColumnName = AutoRateEntry.Schema.TI_OH_Consignor;
			ControlDpiScalingHelper.SetWidth(ref consignor, 100, true);

			return consignor;
		}

		protected ZGuidFindBoxColumnStyleInfo Consignee()
		{
			CompileTimeCheckBindingMember.Check(((RateEntry)(null)).TI_OH_Consignee);
			ZGuidFindBoxColumnStyleInfo consignee = new ZGuidFindBoxColumnStyleInfo();
			consignee.ColumnName = AutoRateEntry.Schema.TI_OH_Consignee;

			ControlDpiScalingHelper.SetWidth(ref consignee, 100, true);
			return consignee;
		}

		protected ZGuidFindBoxColumnStyleInfo ControllingCustomer()
		{
			CompileTimeCheckBindingMember.Check(((RateEntry)(null)).TI_OH_ControllingCustomer);
			ZGuidFindBoxColumnStyleInfo controllingCustomer = new ZGuidFindBoxColumnStyleInfo();
			controllingCustomer.ColumnName = AutoRateEntry.Schema.TI_OH_ControllingCustomer;

			ControlDpiScalingHelper.SetWidth(ref controllingCustomer, 100, true);
			return controllingCustomer;
		}

		protected ZGuidDropEditColumnStyleInfo CartagePickupAddress()
		{
			CompileTimeCheckBindingMember.Check(((RateEntry)(null)).TI_OA_CartagePickupAddressOverride);
			ZGuidDropEditColumnStyleInfo pickupCartageAddress = new ZGuidDropEditColumnStyleInfo();
			pickupCartageAddress.ColumnName = AutoRateEntry.Schema.TI_OA_CartagePickupAddressOverride;
			ControlDpiScalingHelper.SetWidth(ref pickupCartageAddress, 150, true);

			return pickupCartageAddress;
		}

		protected ZGuidDropEditColumnStyleInfo CartageDeliveryAddress()
		{
			CompileTimeCheckBindingMember.Check(((RateEntry)(null)).TI_OA_CartageDeliveryAddressOverride);
			ZGuidDropEditColumnStyleInfo deliveryCartageAddress = new ZGuidDropEditColumnStyleInfo();
			deliveryCartageAddress.ColumnName = AutoRateEntry.Schema.TI_OA_CartageDeliveryAddressOverride;
			ControlDpiScalingHelper.SetWidth(ref deliveryCartageAddress, 150, true);

			return deliveryCartageAddress;
		}

		protected ZTextBoxColumnStyleInfo CartagePickupPostcode()
		{
			CompileTimeCheckBindingMember.Check(((RateEntry)(null)).TI_CartagePickupAddressPostCode);
			var pickupCartagePostcode = new ZTextBoxColumnStyleInfo();
			pickupCartagePostcode.ColumnName = AutoRateEntry.Schema.TI_CartagePickupAddressPostCode;
			pickupCartagePostcode.CharacterCasing = CharacterCasing.Upper;

			ControlDpiScalingHelper.SetWidth(ref pickupCartagePostcode, 70, true);

			return pickupCartagePostcode;
		}

		protected ZTextBoxColumnStyleInfo CartageDeliveryPostcode()
		{
			CompileTimeCheckBindingMember.Check(((RateEntry)(null)).TI_CartageDeliveryAddressPostCode);
			var deliveryCartagePostcode = new ZTextBoxColumnStyleInfo();
			deliveryCartagePostcode.ColumnName = AutoRateEntry.Schema.TI_CartageDeliveryAddressPostCode;
			deliveryCartagePostcode.CharacterCasing = CharacterCasing.Upper;
			ControlDpiScalingHelper.SetWidth(ref deliveryCartagePostcode, 70, true);

			return deliveryCartagePostcode;
		}

		protected ZGuidFindBoxColumnStyleInfo Supplier()
		{
			CompileTimeCheckBindingMember.Check(((RateEntry)(null)).TI_OH_Supplier);
			ZGuidFindBoxColumnStyleInfo supplier = new ZGuidFindBoxColumnStyleInfo();
			supplier.ColumnName = AutoRateEntry.Schema.TI_OH_Supplier;
			ControlDpiScalingHelper.SetWidth(ref supplier, 70, true);
			return supplier;
		}

		protected ZTextBoxColumnStyleInfo RateCategory()
		{
			CompileTimeCheckBindingMember.Check(((RateEntry)(null)).TI_RateCategory);
			ZTextBoxColumnStyleInfo rateCategory = new ZTextBoxColumnStyleInfo
			{
				ColumnName = AutoRateEntry.Schema.TI_RateCategory,
				CharacterCasing = CharacterCasing.Upper
			};
			ControlDpiScalingHelper.SetWidth(ref rateCategory, 45, true);

			return rateCategory;
		}

		protected ZDropEditColumnStyleInfo Mode()
		{
			CompileTimeCheckBindingMember.Check(((RateEntry)(null)).TI_Mode);
			ZDropEditColumnStyleInfo mode = new ZDropEditColumnStyleInfo();
			mode.ColumnName = AutoRateEntry.Schema.TI_Mode;
			mode.CharacterCasing = CharacterCasing.Upper;
			ControlDpiScalingHelper.SetWidth(ref mode, 45, true);

			return mode;
		}

		protected ZDropEditColumnStyleInfo IsNonOperatedReefer()
		{
			CompileTimeCheckBindingMember.Check(((RateEntry)(null)).TI_IsNonOperatedReefer);
			ZDropEditColumnStyleInfo isNonOperatedReefer = new ZDropEditColumnStyleInfo();
			isNonOperatedReefer.ColumnName = AutoRateEntry.Schema.TI_IsNonOperatedReefer;
			isNonOperatedReefer.CharacterCasing = CharacterCasing.Upper;
			ControlDpiScalingHelper.SetWidth(ref isNonOperatedReefer, 100, true);

			return isNonOperatedReefer;
		}

		protected ZDateEditColumnStyleInfo RateStartDate()
		{
			CompileTimeCheckBindingMember.Check(((ZDateTime)(((RateEntry)(null)).TI_RateStartDate)));
			ZDateEditColumnStyleInfo rateStartDate = new ZDateEditColumnStyleInfo();
			rateStartDate.ColumnName = AutoRateEntry.Schema.TI_RateStartDate;
			rateStartDate.DateTimeFormat = ZDateTimePickerFormat.Short;
			rateStartDate.CharacterCasing = CharacterCasing.Upper;
			ControlDpiScalingHelper.SetWidth(ref rateStartDate, 65, true);

			return rateStartDate;
		}

		protected ZDateEditColumnStyleInfo RateEndDate()
		{
			CompileTimeCheckBindingMember.Check(((ZDateTime)(((RateEntry)(null)).TI_RateEndDate)));
			ZDateEditColumnStyleInfo rateEndDate = new ZDateEditColumnStyleInfo();
			rateEndDate.ColumnName = AutoRateEntry.Schema.TI_RateEndDate;
			rateEndDate.DateTimeFormat = ZDateTimePickerFormat.Short;
			rateEndDate.CharacterCasing = CharacterCasing.Upper;
			ControlDpiScalingHelper.SetWidth(ref rateEndDate, 65, true);

			return rateEndDate;
		}

		protected ZCheckBoxColumnStyleInfo AllWarehouses()
		{
			CompileTimeCheckBindingMember.Check(((RateEntry)(null)).AllWarehouses);
			ZCheckBoxColumnStyleInfo allWarehouses = new ZCheckBoxColumnStyleInfo();
			allWarehouses.ColumnName = RateEntry.Schema.AllWarehouses;
			ControlDpiScalingHelper.SetWidth(ref allWarehouses, 50, true);

			return allWarehouses;
		}

		protected ZGuidDropEditColumnStyleInfo Warehouse()
		{
			CompileTimeCheckBindingMember.Check(((RateEntry)(null)).TI_WW_Warehouse);
			ZGuidDropEditColumnStyleInfo warehouse = new ZGuidDropEditColumnStyleInfo();
			warehouse.ColumnName = RateEntry.Schema.TI_WW_Warehouse;
			warehouse.CharacterCasing = CharacterCasing.Upper;
			ControlDpiScalingHelper.SetWidth(ref warehouse, 60, true);

			return warehouse;
		}

		protected ZGuidDropEditColumnStyleInfo Yard()
		{
			CompileTimeCheckBindingMember.Check(((RateEntry)(null)).TI_CYC_WW_Facility);
			ZGuidDropEditColumnStyleInfo yard = new ZGuidDropEditColumnStyleInfo();
			yard.ColumnName = RateEntry.Schema.TI_CYC_WW_Facility;
			yard.CharacterCasing = CharacterCasing.Upper;
			ControlDpiScalingHelper.SetWidth(ref yard, 60, true);

			return yard;
		}

		protected ZDropEditColumnStyleInfo YardUnitType()
		{
			CompileTimeCheckBindingMember.Check(((RateEntry)(null)).TI_YardUnitType);
			ZDropEditColumnStyleInfo column = new ZDropEditColumnStyleInfo();
			column.ColumnName = RateEntry.Schema.TI_YardUnitType;
			column.CharacterCasing = CharacterCasing.Upper;
			ControlDpiScalingHelper.SetWidth(ref column, 45, true);

			return column;
		}

		protected ZDropEditColumnStyleInfo YardUnitLoad()
		{
			CompileTimeCheckBindingMember.Check(((RateEntry)(null)).TI_YardUnitLoad);
			ZDropEditColumnStyleInfo column = new ZDropEditColumnStyleInfo();
			column.ColumnName = RateEntry.Schema.TI_YardUnitLoad;
			column.CharacterCasing = CharacterCasing.Upper;
			ControlDpiScalingHelper.SetWidth(ref column, 45, true);

			return column;
		}

		protected ZCheckBoxColumnStyleInfo DataChecked()
		{
			CompileTimeCheckBindingMember.Check(((RateEntry)(null)).TI_DataChecked);
			ZCheckBoxColumnStyleInfo dataChecked = new ZCheckBoxColumnStyleInfo();
			dataChecked.ColumnName = AutoRateEntry.Schema.TI_DataChecked;
			ControlDpiScalingHelper.SetWidth(ref dataChecked, 50, true);

			return dataChecked;
		}

		protected ZDropEditColumnStyleInfo IncoTerm()
		{
			CompileTimeCheckBindingMember.Check(((RateEntry)(null)).TI_QuotePageIncoTerm);
			ZDropEditColumnStyleInfo styleInfo = new ZDropEditColumnStyleInfo();
			styleInfo.ColumnName = AutoRateEntry.Schema.TI_QuotePageIncoTerm;
			styleInfo.CharacterCasing = CharacterCasing.Upper;
			ControlDpiScalingHelper.SetWidth(ref styleInfo, 45, true);

			return styleInfo;
		}

		protected ZCheckBoxColumnStyleInfo ContractNumberLinked()
		{
			CompileTimeCheckBindingMember.Check(((RateEntry)(null)).TI_ContractNumberLinked);
			ZCheckBoxColumnStyleInfo styleInfo = new ZCheckBoxColumnStyleInfo();
			styleInfo.ColumnName = AutoRateEntry.Schema.TI_ContractNumberLinked;
			ControlDpiScalingHelper.SetWidth(ref styleInfo, 65, true);

			return styleInfo;
		}

		protected ZGridColumnInfo ContractNumber()
		{
			CompileTimeCheckBindingMember.Check(((RateEntry)(null)).TI_ContractNumber);

			ZGridColumnInfo info;
			if (ShouldContractNumberUseFindBox)
			{
				info = new ContractNumberFindBoxColumnStyleInfo();
			}
			else
			{
				info = new ZTextBoxColumnStyleInfo();
			}

			info.ColumnName = AutoRateEntry.Schema.TI_ContractNumber;
			info.CharacterCasing = CharacterCasing.Normal;
			info.CaptionResourceString = IsCosting || IsMultiModalRateSearch || CategoriesShowingCarrierContractNumberInRevenue.Contains(Category)
				? Res.GetData("3e14077b-91c1-44be-97ed-2eaec39f00ae", "Contract No.", "Carrier Contract Number", "Contract Number for Carrier")
				: Res.GetData("1e2dca53-c9e7-4c57-94e3-78d06f283b8d", "Contract No.", "Client Contract Number", "Contract Number for Client");

			return info;
		}

		bool ShouldContractNumberUseFindBox
		{
			get
			{
#if WINZOR
				// When in Winzor, we can only do the CW1Popup when it's a costing
				return
					IsCosting
					&& RatingConstants.RateCategory.SupportsContractNumberLookup(Category)
					&& ObjectFactory.Get<IContractPermissions>().IsAllocationsVisible();
#else
				// No Winzor means GLOW and CW1 popup both work. No need to check for
				// CW1 Popup (ObjectFactory.Get<IContractPermissions>().IsCarrierAndClientContractModulesEnabled())
				// because the GLOW popup (ObjectFactory.Get<IContractPermissions>().IsCarrierAndClientContractModulesEnabled())
				// will be available if the CW1Popup isnt.
				return
					(IsCosting || IsClientRate)
					&& RatingConstants.RateCategory.SupportsContractNumberLookup(Category)
					&& ObjectFactory.Get<IContractPermissions>().IsCarrierAndClientContractModulesEnabled();
#endif
			}
		}

		protected ZTextBoxColumnStyleInfo AllInCost()
		{
			CompileTimeCheckBindingMember.Check(((RateEntry)(null)).AllInCost());
			var allInCost = new ZTextBoxColumnStyleInfo("AllInCost", 80);
			allInCost.IsReadOnly = true;
			allInCost.CharacterCasing = CharacterCasing.Normal;

			return allInCost;
		}

		protected ZTextBoxColumnStyleInfo FreightRatePerChargeableUnit()
		{
			CompileTimeCheckBindingMember.Check(((RateEntry)(null)).FreightRatePerChargeableUnit());
			var freightRatePerChargeableUnit = new ZTextBoxColumnStyleInfo("FreightRatePerChargeableUnit", 80);
			freightRatePerChargeableUnit.IsReadOnly = true;
			freightRatePerChargeableUnit.CharacterCasing = CharacterCasing.Normal;

			return freightRatePerChargeableUnit;
		}

		protected ZGuidFindBoxColumnStyleInfo Publisher()
		{
			CompileTimeCheckBindingMember.Check(((RateEntry)(null)).TI_GC_Publisher);
			var publisher = new ZGuidFindBoxColumnStyleInfo();
			publisher.ColumnName = AutoRateEntry.Schema.TI_GC_Publisher;
			publisher.IsReadOnly = true;
			publisher.IsVisible = true;

			ControlDpiScalingHelper.SetWidth(ref publisher, 40, true);

			return publisher;
		}

		protected ZCheckBoxColumnStyleInfo IsPublished()
		{
			CompileTimeCheckBindingMember.Check(((RateEntry)(null)).IsPublished);
			var isPublished = new ZCheckBoxColumnStyleInfo();
			isPublished.ColumnName = RateEntry.Schema.IsPublished;
			isPublished.IsVisible = false;
			ControlDpiScalingHelper.SetWidth(ref isPublished, 40, true);

			return isPublished;
		}

		protected ZCheckBoxColumnStyleInfo IsTact()
		{
			CompileTimeCheckBindingMember.Check(((RateEntry)(null)).TI_IsTact);
			var isTactRateColumn = new ZCheckBoxColumnStyleInfo();
			isTactRateColumn.ColumnName = RateEntry.Schema.TI_IsTact;
			isTactRateColumn.IsReadOnly = true;
			ControlDpiScalingHelper.SetWidth(ref isTactRateColumn, 50, true);

			return isTactRateColumn;
		}

		protected ZCheckBoxColumnStyleInfo IsExcludedFromAutoRating()
		{
			CompileTimeCheckBindingMember.Check(((RateEntry)(null)).TI_IsExcludedFromAutoRating);
			var isExcludedFromAutoRating = new ZCheckBoxColumnStyleInfo();
			isExcludedFromAutoRating.ColumnName = RateEntry.Schema.TI_IsExcludedFromAutoRating;
			ControlDpiScalingHelper.SetWidth(ref isExcludedFromAutoRating, 50, true);

			return isExcludedFromAutoRating;
		}

		protected ZTextBoxColumnStyleInfo RateProvider()
		{
			CompileTimeCheckBindingMember.Check(((RateEntry)(null)).RateProvider);
			var rateProviderColumn = new ZTextBoxColumnStyleInfo();
			rateProviderColumn.ColumnName = nameof(RateEntry.RateProvider);
			rateProviderColumn.CaptionResourceString = Res.GetData("dfe64d38-7766-447c-ac87-4cbcc04e0df7", "Provider", "Rate Provider", "Provider of this Rate Entry");
			rateProviderColumn.IsReadOnly = true;
			ControlDpiScalingHelper.SetWidth(ref rateProviderColumn, 50, true);

			return rateProviderColumn;
		}

		protected ZTextBoxColumnStyleInfo CGReference()
		{
			CompileTimeCheckBindingMember.Check(((WiseEntryView)(null)).CGReference);
			var cgReference = new ZTextBoxColumnStyleInfo(nameof(WiseEntryView.CGReference), 50)
			{
				IsReadOnly = true,
				CharacterCasing = CharacterCasing.Normal,
				CaptionResourceString = Res.GetData("18303cc7-d2de-42d6-956e-7a71fb79e2fe", "CG Ref.", "CG Reference", "Cargoguide Reference")
			};

			return cgReference;
		}

		protected ZDropEditColumnStyleInfo PaymentTermOverride()
		{
			CompileTimeCheckBindingMember.Check(((RateEntry)(null)).TI_PaymentTerm);
			ZDropEditColumnStyleInfo paymentTermOverride = new ZDropEditColumnStyleInfo();
			paymentTermOverride.ColumnName = AutoRateEntry.Schema.TI_PaymentTerm;
			paymentTermOverride.BindToList = "Lookups.PaymentTerms";
			ControlDpiScalingHelper.SetWidth(ref paymentTermOverride, 45, true);
			return paymentTermOverride;
		}

		#endregion

		protected ZTextBoxColumnStyleInfo UniversalCarrierServiceLevel()
		{
			CompileTimeCheckBindingMember.Check(((WiseEntryView)(null)).UniversalCarrierServiceLevel);
			return new ZTextBoxColumnStyleInfo(nameof(WiseEntryView.UniversalCarrierServiceLevel), 50)
			{
				IsReadOnly = true,
				CharacterCasing = CharacterCasing.Normal,
				CaptionResourceString = Res.GetData("e90870a9-0cda-4f30-bad0-50e18c28e6b6", "Universal Level", "Universal Carrier Service Level")
			};
		}

		protected ZTextBoxColumnStyleInfo ContainerQuality()
		{
			CompileTimeCheckBindingMember.Check(((WiseEntryView)(null)).ContainerQuality);
			return new ZTextBoxColumnStyleInfo(nameof(WiseEntryView.ContainerQuality), 50)
			{
				IsReadOnly = true,
				CharacterCasing = CharacterCasing.Normal,
				CaptionResourceString = Res.GetData("ADEFE7B7-1E9C-43FC-9D6E-99E4844EF58E", "Cont. Quality", "Container Quality")
			};
		}

		protected ZTextBoxColumnStyleInfo CargoguideProductCode()
		{
			CompileTimeCheckBindingMember.Check(((WiseEntryView)(null)).CargoguideProductCode);
			return new ZTextBoxColumnStyleInfo(nameof(WiseEntryView.CargoguideProductCode), 50)
			{
				IsReadOnly = true,
				CharacterCasing = CharacterCasing.Normal,
				CaptionResourceString = Res.GetData("221b8557-ff84-49ef-a6ce-e1626aa4a6b4", "Product Code")
			};
		}

		protected ZTextBoxColumnStyleInfo ContainerPayloadWeight()
		{
			CompileTimeCheckBindingMember.Check(((WiseEntryView)(null)).ContainerPayloadWeight);
			var column = new ZTextBoxColumnStyleInfo(nameof(WiseEntryView.ContainerPayloadWeightForBinding), 80)
			{
				IsReadOnly = true,
				CharacterCasing = CharacterCasing.Normal,
				CaptionResourceString = Res.GetData("8e5f9709-2f4d-4d13-b68f-6e2b756a2b2a", "Max Payload (Wgt.)", "Maximum Weight Payload of the container")
			};

			return column;
		}

		protected ZTextBoxColumnStyleInfo ContainerPayloadVolume()
		{
			CompileTimeCheckBindingMember.Check(((WiseEntryView)(null)).ContainerPayloadVolume);
			var column = new ZTextBoxColumnStyleInfo(nameof(WiseEntryView.ContainerPayloadVolumeForBinding), 80)
			{
				IsReadOnly = true,
				CharacterCasing = CharacterCasing.Normal,
				CaptionResourceString = Res.GetData("635b5388-f7e4-409c-a53f-2bcb20bf603d", "Max Payload (Vol.)", "Maximum Volume Payload of the container")
			};

			return column;
		}
	}

	static class RateEntryCollectionExtensions
	{
		public static void Add(this IList<ZGridColumnInfo> list, Func<ZGridColumnInfo> infoGetter, bool shouldAddInfoToList = true, string captionOverride = "", bool isVisible = true)
		{
			if (shouldAddInfoToList)
			{
				var info = infoGetter();

				if (!string.IsNullOrEmpty(captionOverride))
				{
					info.Caption = captionOverride;
				}

				info.IsVisible = isVisible;

				list.Add(info);
			}
		}

		public static void Add(this IList<ZGridColumnInfo> list, Func<ZGridColumnInfo> infoGetter, ResourceStringData captionOverride, bool isVisible = true)
		{
			var info = infoGetter();

			if (captionOverride != null)
			{
				info.CaptionResourceString = captionOverride;
			}

			info.IsVisible = isVisible;

			list.Add(info);
		}
	}
}

