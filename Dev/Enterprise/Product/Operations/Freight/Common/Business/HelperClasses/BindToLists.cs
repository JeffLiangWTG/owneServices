using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Common.Business
{
	/// <summary>
	/// Commonly used collections for findbox BindToLists.
	/// </summary>
	public class BindToLists
	{
		public static BindToLists GetCachedLists(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("Enterprise.Freight.Common.Business.BindToLists", () => new BindToLists(factory));
		}

		protected BindToLists(BusinessObjectFactory factory)
		{
			this.Factory = factory;
		}

		protected readonly BusinessObjectFactory Factory;

		#region OLD

		#region Ref Lists

		LocationCollection fRefLocation_List;
		public LocationCollection RefLocation_List
		{
			get
			{
				if (fRefLocation_List == null)
				{
					fRefLocation_List = new LocationCollection(Factory);
				}
				return fRefLocation_List;
			}
		}

		RefCountryCollection fRefCountry_List;
		public RefCountryCollection RefCountry_List
		{
			get
			{
				if (fRefCountry_List == null)
				{
					fRefCountry_List = new RefCountryCollection(Factory);
				}
				return fRefCountry_List;
			}
		}

		RefUNLOCOCollection fRefUNLOCO_List;
		public RefUNLOCOCollection RefUNLOCO_List
		{
			get
			{
				if (fRefUNLOCO_List == null)
				{
					fRefUNLOCO_List = new RefUNLOCOCollection(Factory);
				}
				return fRefUNLOCO_List;
			}
		}

		RefVesselCollection fRefVessel_List;
		public RefVesselCollection RefVessel_List
		{
			get
			{
				if (fRefVessel_List == null)
				{
					fRefVessel_List = new RefVesselCollection(Factory);
				}
				return fRefVessel_List;
			}
		}

		ActiveServiceLevelCollection fRefServiceLevel_List;
		public ActiveServiceLevelCollection RefServiceLevel_List
		{
			get
			{
				if (fRefServiceLevel_List == null)
				{
					fRefServiceLevel_List = new ActiveServiceLevelCollection(Factory);
				}
				return fRefServiceLevel_List;
			}
		}

		RefExchangeRateCollection fRefExchangeRate_List;
		public RefExchangeRateCollection RefExchangeRate_List
		{
			get
			{
				if (fRefExchangeRate_List == null)
				{
					fRefExchangeRate_List = new RefExchangeRateCollection(Factory);
				}
				return fRefExchangeRate_List;
			}
		}

		RefCurrencyCollection fRefCurrency_List;
		public RefCurrencyCollection RefCurrency_List
		{
			get
			{
				if (fRefCurrency_List == null)
				{
					fRefCurrency_List = new RefCurrencyCollection(Factory);
				}
				return fRefCurrency_List;
			}
		}

		RefCommodityCodeCollection fRefCommodityCode_List;
		public RefCommodityCodeCollection RefCommodityCode_List
		{
			get
			{
				if (fRefCommodityCode_List == null)
				{
					fRefCommodityCode_List = new RefCommodityCodeCollection(Factory);
				}
				return fRefCommodityCode_List;
			}
		}

		RefContainerCollection fRefContainer_List;
		public RefContainerCollection RefContainer_List
		{
			get
			{
				if (fRefContainer_List == null)
				{
					fRefContainer_List = new RefContainerCollection(Factory);
				}
				return fRefContainer_List;
			}
		}

		RefCommodityCodeCollection fRefCommodity_List;
		public RefCommodityCodeCollection RefCommodity_List
		{
			get
			{
				if (fRefCommodity_List == null)
				{
					fRefCommodity_List = new RefCommodityCodeCollection(Factory);
				}
				return fRefCommodity_List;
			}
		}

		#endregion

		#region Org Lists

		OrgHeaderCollection fOrgHeader_List;
		public OrgHeaderCollection OrgHeader_List
		{
			get
			{
				if (fOrgHeader_List == null)
				{
					fOrgHeader_List = new OrgHeaderCollection(Factory);
				}
				return fOrgHeader_List;
			}
		}

		OrgContactCollection fOrgContact_List;
		public OrgContactCollection OrgContact_List
		{
			get
			{
				if (fOrgContact_List == null)
				{
					fOrgContact_List = new OrgContactCollection(Factory);
				}
				return fOrgContact_List;
			}
		}

		OrgAddressCollection fOrgAddress_List;
		public OrgAddressCollection OrgAddress_List
		{
			get
			{
				if (fOrgAddress_List == null)
				{
					fOrgAddress_List = new OrgAddressCollection(Factory);
				}
				return fOrgAddress_List;
			}
		}

		OrgSupplierPartCollection fOrgSupplierPart_List;
		public OrgSupplierPartCollection OrgSupplierPart_List
		{
			get
			{
				if (fOrgSupplierPart_List == null)
				{
					fOrgSupplierPart_List = new OrgSupplierPartCollection(Factory);
				}
				return fOrgSupplierPart_List;
			}
		}

		#region Shipping Providers

		ShippingProviderCollection fShippingProvider_List;
		public ShippingProviderCollection ShippingProvider_List
		{
			get
			{
				if (fShippingProvider_List == null)
				{
					fShippingProvider_List = new ShippingProviderCollection(Factory);
				}
				return fShippingProvider_List;
			}
		}

		ShippingProviderCollection fShippingProvider_FilterList;
		public ShippingProviderCollection ShippingProvider_FilterList
		{
			get
			{
				if (fShippingProvider_FilterList == null)
				{
					fShippingProvider_FilterList = new ShippingProviderCollection(Factory)
					{
						ShouldApplyActiveFilter = false
					};
				}
				return fShippingProvider_FilterList;
			}
		}

		AirShippingProviderCollection fAirShippingProvider_List;
		public AirShippingProviderCollection AirShippingProvider_List
		{
			get
			{
				if (fAirShippingProvider_List == null)
				{
					fAirShippingProvider_List = new AirShippingProviderCollection(Factory);
				}
				return fAirShippingProvider_List;
			}
		}

		LineHaulShippingProviderCollection fLineHaulShippingProvider_List;
		public LineHaulShippingProviderCollection LineHaulShippingProvider_List
		{
			get
			{
				if (fLineHaulShippingProvider_List == null)
				{
					fLineHaulShippingProvider_List = new LineHaulShippingProviderCollection(Factory);
				}
				return fLineHaulShippingProvider_List;
			}
		}

		SeaShippingProviderCollection fSeaShippingProvider_List;
		public SeaShippingProviderCollection SeaShippingProvider_List
		{
			get
			{
				if (fSeaShippingProvider_List == null)
				{
					fSeaShippingProvider_List = new SeaShippingProviderCollection(Factory);
				}
				return fSeaShippingProvider_List;
			}
		}

		RailShippingProviderCollection fRailShippingProvider_List;
		public RailShippingProviderCollection RailShippingProvider_List
		{
			get
			{
				if (fRailShippingProvider_List == null)
				{
					fRailShippingProvider_List = new RailShippingProviderCollection(Factory);
				}
				return fRailShippingProvider_List;
			}
		}

		#endregion

		ForwarderCollection fOrgForwarder_List;
		public ForwarderCollection OrgForwarder_List
		{
			get
			{
				if (fOrgForwarder_List == null)
				{
					fOrgForwarder_List = new ForwarderCollection(Factory);
				}
				return fOrgForwarder_List;
			}
		}

		ForwarderCollection fOrgForwarder_FilterList;
		public ForwarderCollection OrgForwarder_FilterList
		{
			get
			{
				if (fOrgForwarder_FilterList == null)
				{
					fOrgForwarder_FilterList = new ForwarderCollection(Factory)
					{
						ShouldApplyActiveFilter = false
					};
				}
				return fOrgForwarder_FilterList;
			}
		}

		ForwarderCollection fOrgForwarderDefaultOnly_List;
		public ForwarderCollection OrgForwarderDefaultOnly_List
		{
			get
			{
				if (fOrgForwarderDefaultOnly_List == null)
				{
					fOrgForwarderDefaultOnly_List = new ForwarderCollection(Factory);
					fOrgForwarderDefaultOnly_List.AllowOtherOrgTypes = true;
				}
				return fOrgForwarderDefaultOnly_List;
			}
		}
		ConsignorCollection fOrgConsignor_List;
		public ConsignorCollection OrgConsignor_List
		{
			get
			{
				if (fOrgConsignor_List == null)
				{
					fOrgConsignor_List = new ConsignorCollection(Factory);
				}
				return fOrgConsignor_List;
			}
		}

		ConsignorCollection fOrgConsignor_FilterList;
		public ConsignorCollection OrgConsignor_FilterList
		{
			get
			{
				if (fOrgConsignor_FilterList == null)
				{
					fOrgConsignor_FilterList = new ConsignorCollection(Factory)
					{
						ShouldApplyActiveFilter = false
					};
				}
				return fOrgConsignor_FilterList;
			}
		}

		public ControllingCustomerCollection OrgControllingCustomerList
		{
			get
			{
				return orgControllingCustomerList ?? (orgControllingCustomerList = new ControllingCustomerCollection(Factory));
			}
		}
		ControllingCustomerCollection orgControllingCustomerList;

		public ControllingCustomerCollection OrgControllingCustomerFilterList
		{
			get
			{
				return orgControllingCustomerFilterList ?? (orgControllingCustomerFilterList = new ControllingCustomerCollection(Factory)
				{
					ShouldApplyActiveFilter = false
				});
			}
		}
		ControllingCustomerCollection orgControllingCustomerFilterList;

		public ControllingAgentCollection OrgControllingAgentList
		{
			get { return orgControllingAgentList ?? (orgControllingAgentList = new ControllingAgentCollection(Factory)); }
		}
		ControllingAgentCollection orgControllingAgentList;

		public ControllingAgentCollection OrgControllingAgentFilterList
		{
			get
			{
				return orgControllingAgentFilterList ?? (orgControllingAgentFilterList = new ControllingAgentCollection(Factory)
				{
					ShouldApplyActiveFilter = false
				});
			}
		}
		ControllingAgentCollection orgControllingAgentFilterList;

		ConsignorCollection fOrgConsignorDefaultOnly_List;
		public ConsignorCollection OrgConsignorDefaultOnly_List
		{
			get
			{
				if (fOrgConsignorDefaultOnly_List == null)
				{
					fOrgConsignorDefaultOnly_List = new ConsignorCollection(Factory);
					fOrgConsignorDefaultOnly_List.AllowOtherOrgTypes = true;
				}
				return fOrgConsignorDefaultOnly_List;
			}
		}

		ConsigneeCollection fOrgConsignee_List;
		public ConsigneeCollection OrgConsignee_List
		{
			get
			{
				if (fOrgConsignee_List == null)
				{
					fOrgConsignee_List = new ConsigneeCollection(Factory);
				}
				return fOrgConsignee_List;
			}
		}

		ConsigneeCollection fOrgConsignee_FilterList;
		public ConsigneeCollection OrgConsignee_FilterList
		{
			get
			{
				if (fOrgConsignee_FilterList == null)
				{
					fOrgConsignee_FilterList = new ConsigneeCollection(Factory)
					{
						ShouldApplyActiveFilter = false
					};
				}
				return fOrgConsignee_FilterList;
			}
		}

		ConsigneeCollection fOrgConsigneeDefaultOnly_List;
		public ConsigneeCollection OrgConsigneeDefaultOnly_List
		{
			get
			{
				if (fOrgConsigneeDefaultOnly_List == null)
				{
					fOrgConsigneeDefaultOnly_List = new ConsigneeCollection(Factory);
					fOrgConsigneeDefaultOnly_List.AllowOtherOrgTypes = true;
				}
				return fOrgConsigneeDefaultOnly_List;
			}
		}

		public ConsignorCollection OrgSupplier_List
		{
			get { return OrgConsignor_List; }
		}

		public ConsignorCollection OrgSupplier_FilterList
		{
			get { return OrgConsignor_FilterList; }
		}

		public ConsigneeCollection OrgBuyer_List
		{
			get { return OrgConsignee_List; }
		}

		public ConsigneeCollection OrgBuyer_FilterList
		{
			get { return OrgConsignee_FilterList; }
		}

		DepotCollection fOrgDepot_List;
		public DepotCollection OrgDepot_List
		{
			get
			{
				if (fOrgDepot_List == null)
				{
					fOrgDepot_List = new DepotCollection(Factory);
				}
				return fOrgDepot_List;
			}
		}

		PackDepotCollection fPackDepot_List;
		public PackDepotCollection PackDepot_List
		{
			get
			{
				if (fPackDepot_List == null)
				{
					fPackDepot_List = new PackDepotCollection(Factory);
				}
				return fPackDepot_List;
			}
		}

		PackDepotCollection fPackDepot_FilterList;
		public PackDepotCollection PackDepot_FilterList
		{
			get
			{
				if (fPackDepot_FilterList == null)
				{
					fPackDepot_FilterList = new PackDepotCollection(Factory)
					{
						ShouldApplyActiveFilter = false
					};
				}
				return fPackDepot_FilterList;
			}
		}

		UnpackDepotCollection fUnpackDepot_List;
		public UnpackDepotCollection UnpackDepot_List
		{
			get
			{
				if (fUnpackDepot_List == null)
				{
					fUnpackDepot_List = new UnpackDepotCollection(Factory);
				}
				return fUnpackDepot_List;
			}
		}

		UnpackDepotCollection fUnpackDepot_FilterList;
		public UnpackDepotCollection UnpackDepot_FilterList
		{
			get
			{
				if (fUnpackDepot_FilterList == null)
				{
					fUnpackDepot_FilterList = new UnpackDepotCollection(Factory)
					{
						ShouldApplyActiveFilter = false
					};
				}
				return fUnpackDepot_FilterList;
			}
		}

		CTOCollection fOrgCTO_List;
		public CTOCollection OrgCTO_List
		{
			get
			{
				if (fOrgCTO_List == null)
				{
					fOrgCTO_List = new CTOCollection(Factory);
				}
				return fOrgCTO_List;
			}
		}

		CTOCollection fOrgCTO_FilterList;
		public CTOCollection OrgCTO_FilterList
		{
			get
			{
				if (fOrgCTO_FilterList == null)
				{
					fOrgCTO_FilterList = new CTOCollection(Factory)
					{
						ShouldApplyActiveFilter = false
					};
				}
				return fOrgCTO_FilterList;
			}
		}

		AirCTOCollection fAirCTO_List;
		public AirCTOCollection AirCTO_List
		{
			get
			{
				if (fAirCTO_List == null)
				{
					fAirCTO_List = new AirCTOCollection(Factory);
				}
				return fAirCTO_List;
			}
		}

		SeaCTOCollection fSeaCTO_List;
		public SeaCTOCollection SeaCTO_List
		{
			get
			{
				if (fSeaCTO_List == null)
				{
					fSeaCTO_List = new SeaCTOCollection(Factory);
				}
				return fSeaCTO_List;
			}
		}

		ContainerYardCollection fOrgContainerYard_List;
		public ContainerYardCollection OrgContainerYard_List
		{
			get
			{
				if (fOrgContainerYard_List == null)
				{
					fOrgContainerYard_List = new ContainerYardCollection(Factory);
				}
				return fOrgContainerYard_List;
			}
		}

		ContainerYardCollection fOrgContainerYard_FilterList;
		public ContainerYardCollection OrgContainerYard_FilterList
		{
			get
			{
				if (fOrgContainerYard_FilterList == null)
				{
					fOrgContainerYard_FilterList = new ContainerYardCollection(Factory)
					{
						ShouldApplyActiveFilter = false
					};
				}
				return fOrgContainerYard_FilterList;
			}
		}

		#region OrgMiscServBroker_List

		BrokerCollection fOrgMiscServBroker_List;
		public BrokerCollection OrgMiscServBroker_List
		{
			get
			{
				if (fOrgMiscServBroker_List == null)
				{
					fOrgMiscServBroker_List = new BrokerCollection(Factory);
				}
				return fOrgMiscServBroker_List;
			}
		}

		#endregion

		BrokerCollection fOrgMiscServBroker_FilterList;
		public BrokerCollection OrgMiscServBroker_FilterList
		{
			get
			{
				if (fOrgMiscServBroker_FilterList == null)
				{
					fOrgMiscServBroker_FilterList = new BrokerCollection(Factory)
					{
						ShouldApplyActiveFilter = false
					};
				}
				return fOrgMiscServBroker_FilterList;
			}
		}

		#region OrgMiscServLocalTransport_List

		LocalTransportCollection fOrgMiscServLocalTransport_List;
		public LocalTransportCollection OrgMiscServLocalTransport_List
		{
			get
			{
				if (fOrgMiscServLocalTransport_List == null)
				{
					fOrgMiscServLocalTransport_List = new LocalTransportCollection(Factory);
				}
				return fOrgMiscServLocalTransport_List;
			}
		}

		LocalTransportCollection fOrgMiscServLocalTransport_FilterList;
		public LocalTransportCollection OrgMiscServLocalTransport_FilterList
		{
			get
			{
				if (fOrgMiscServLocalTransport_FilterList == null)
				{
					fOrgMiscServLocalTransport_FilterList = new LocalTransportCollection(Factory)
					{
						ShouldApplyActiveFilter = false
					};
				}
				return fOrgMiscServLocalTransport_FilterList;
			}
		}

		#endregion

		#region UNDGSubstance_List

		UNDGSubstanceCollection fUNDGSubstance_List;
		public UNDGSubstanceCollection UNDGSubstance_List
		{
			get
			{
				if (fUNDGSubstance_List == null)
				{
					fUNDGSubstance_List = new UNDGSubstanceCollection(Factory);
				}
				return fUNDGSubstance_List;
			}
		}

		#endregion

		#region AirCTOAndDepot_List

		AirCTOAndDepotCollection fAirCTOAndDepot_List;
		public AirCTOAndDepotCollection AirCTOAndDepot_List
		{
			get
			{
				if (fAirCTOAndDepot_List == null)
				{
					fAirCTOAndDepot_List = new AirCTOAndDepotCollection(Factory);
				}
				return fAirCTOAndDepot_List;
			}
		}

		#endregion

		#region SeaCTOAndDepot_List

		SeaCTOAndDepotCollection fSeaCTOAndDepot_List;
		public SeaCTOAndDepotCollection SeaCTOAndDepot_List
		{
			get
			{
				if (fSeaCTOAndDepot_List == null)
				{
					fSeaCTOAndDepot_List = new SeaCTOAndDepotCollection(Factory);
				}
				return fSeaCTOAndDepot_List;
			}
		}

		#endregion

		#region ShippingLine_List

		TransportShippingProviderCollection fShippingLine_List;
		public TransportShippingProviderCollection ShippingLine_List
		{
			get
			{
				if (fShippingLine_List == null)
				{
					fShippingLine_List = new TransportShippingProviderCollection(Factory);
				}
				return fShippingLine_List;
			}
		}

		#endregion

		#region OrgReceivables_List

		DebtorCollection fOrgDebtor_List;
		public DebtorCollection OrgDebtor_List
		{
			get
			{
				if (fOrgDebtor_List == null)
				{
					fOrgDebtor_List = new DebtorCollection(new BusinessObjectFactory());
				}

				return fOrgDebtor_List;
			}
		}

		#endregion

		#endregion

		#region Glb Lists

		GlbBranchCollection fGlbBranch_List;
		public GlbBranchCollection GlbBranch_List
		{
			get
			{
				if (fGlbBranch_List == null)
				{
					fGlbBranch_List = new GlbBranchCollection(Factory);
				}
				return fGlbBranch_List;
			}
		}

		#endregion

		#endregion

		#region TransportProviders

		public LocalTransportCollection TransportProviders
		{
			get { return transportProviders ?? (transportProviders = new LocalTransportCollection(Factory)); }
		}
		LocalTransportCollection transportProviders;

		#endregion

		#region StaffDrivers

		//May not need it's own collection, implement like Vehicles - Shouldn't be creating new from this
		public StaffDriverCollection StaffDrivers
		{
			get { return staffDrivers ?? (staffDrivers = new StaffDriverCollection(Factory)); }
		}
		StaffDriverCollection staffDrivers;

		#endregion

		#region Equipments

		public RefEquipmentCollection Equipments
		{
			get { return equipments ?? (equipments = new RefEquipmentCollection(Factory)); }
		}
		RefEquipmentCollection equipments;

		#endregion

		#region Vehicles

		public RefEquipmentCollection Vehicles
		{
			get
			{
				if (vehicles == null)
				{
					vehicles = new RefEquipmentCollection(Factory, new ZQuery(RefEquipmentSchema.RQ_IsVehicle, true));
					vehicles.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Vehicle Status", "Property", (ZString)(NoResString)"Is a Vehicle"));
				}
				return vehicles;
			}
		}

		RefEquipmentCollection vehicles;

		#endregion

		#region Organisations

		public OrgHeaderCollection Organisations
		{
			get { return organisations ?? (organisations = new OrgHeaderCollection(Factory)); }
		}
		OrgHeaderCollection organisations;

		#endregion

		#region RefUNLOCOs

		public RefUNLOCOCollection RefUNLOCOs
		{
			get { return refUNLOCOs ?? (refUNLOCOs = new RefUNLOCOCollection(Factory)); }
		}
		RefUNLOCOCollection refUNLOCOs;

		#endregion

		#region RefLocations

		public LocationCollection RefLocations
		{
			get { return refLocations ?? (refLocations = new LocationCollection(Factory)); }
		}
		LocationCollection refLocations;

		#endregion

		#region RefVessels

		public RefVesselCollection RefVessels
		{
			get { return refVessels ?? (refVessels = new RefVesselCollection(Factory)); }
		}
		RefVesselCollection refVessels;

		#endregion

		#region ServiceLevels

		public RefServiceLevelCollection ServiceLevels
		{
			get { return serviceLevels ?? (serviceLevels = new RefServiceLevelCollection(Factory)); }
		}
		RefServiceLevelCollection serviceLevels;

		#endregion

		#region Substances

		public UNDGSubstanceCollection Substances
		{
			get { return new UNDGSubstanceCollection(Factory); }
		}

		#endregion

		#region DGCOntacts

		public OrgContactCollection DGCOntacts
		{
			get { return new OrgContactCollection(Factory); }
		}

		#endregion

		#region ContainerTypes

		public RefContainerCollection ContainerTypes
		{
			get { return new RefContainerCollection(Factory); }
		}

		#endregion

		#region Branches

		public GlbBranchCollection Branches
		{
			get { return branches ?? (branches = new GlbBranchCollection(Factory)); }
		}
		GlbBranchCollection branches;

		#endregion

		#region CartageJobTypes

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Untranslatable reason")]
		public CodeDescriptionPairList NewCartageJobTypes
		{
			get
			{
				if (cartageJobTypes == null)
				{
					cartageJobTypes = new UntranslatableCodeDescriptionPairList("Descriptions are stored in the database");

					ZQuery filter = new ZQuery(LocalCartageJobTypeSchema.E3_IsHidden, false);
					filter.AddToFilter(JoinCondition.And, LocalCartageJobTypeSchema.E3_JobType, SQLComparisonOperator.NotEqual, "");

					CommonCartageTypeCollection cartageTypes = new CommonCartageTypeCollection(Factory, filter);
					foreach (CommonCartageType type in cartageTypes)
					{
						cartageJobTypes.AddPair(type.E3_JobType, type.E3_DescriptionMultilingual);
					}
					cartageJobTypes.SortByDescription();
				}
				return cartageJobTypes;
			}
		}
		CodeDescriptionPairList cartageJobTypes;

		#endregion

		#region AllCartageJobTypes

		public CodeDescriptionPairList NewAllCartageJobTypes
		{
			get
			{
				if (allCartageJobTypes == null)
				{
					allCartageJobTypes = new CodeDescriptionPairList();

					ZQuery filter = new ZQuery(LocalCartageJobTypeSchema.E3_JobType, SQLComparisonOperator.NotEqual, "");
					CommonCartageTypeCollection cartageTypes = new CommonCartageTypeCollection(Factory, filter);
					foreach (CommonCartageType type in cartageTypes)
					{
						allCartageJobTypes.AddPair(type.E3_JobType, type.E3_DescriptionMultilingual);
					}
					allCartageJobTypes.SortByDescription();
				}
				return allCartageJobTypes;
			}
		}
		CodeDescriptionPairList allCartageJobTypes;

		#endregion

		#region ServiceLevelsCodePairList

		public CodeDescriptionPairList ServiceLevelsCodePairList
		{
			get
			{
				if (serviceLevelsCodePairList == null)
				{
					serviceLevelsCodePairList = new CodeDescriptionPairList();
					RefServiceLevelCollection services = new RefServiceLevelCollection(Factory);
					serviceLevelsCodePairList.AddRange(services);
					serviceLevelsCodePairList.Sort();
				}
				return serviceLevelsCodePairList;
			}
		}
		CodeDescriptionPairList serviceLevelsCodePairList;

		#endregion

		#region WeightUnits

		public CodeDescriptionPairList WeightUnits
		{
			get { return weightUnits ?? (weightUnits = new CodeDescriptionPairList(OLookUpEditType.Weight)); }
		}
		CodeDescriptionPairList weightUnits;

		#endregion

		#region VolumeUnits

		public CodeDescriptionPairList VolumeUnits
		{
			get { return volumeUnits ?? (volumeUnits = new CodeDescriptionPairList(OLookUpEditType.Volume)); }
		}
		CodeDescriptionPairList volumeUnits;

		#endregion

		#region OuterPackTypes

		public RefPackTypeCollection OuterPackTypes
		{
			get { return outerPackTypes ?? (outerPackTypes = new RefPackTypeCollection(Factory)); }
		}
		RefPackTypeCollection outerPackTypes;

		#endregion

		#region DimensionUnits

		public CodeDescriptionPairList DimensionUnits
		{
			get { return dimensionUnits ?? (dimensionUnits = new CodeDescriptionPairList(OLookUpEditType.Length)); }
		}
		CodeDescriptionPairList dimensionUnits;

		#endregion

		#region DistanceUnits

		public CodeDescriptionPairList DistanceUnits
		{
			get
			{
				if (distanceUnits == null)
				{
					distanceUnits = new DistanceUnitList();
				}

				return distanceUnits;
			}
		}
		CodeDescriptionPairList distanceUnits;

		#endregion

		#region DropModes

		public CodeDescriptionPairList DropModes()
		{
			return dropModes ?? (dropModes = new CombinedEquipmentNeededList());
		}
		CodeDescriptionPairList dropModes;

		public CodeDescriptionPairList DropModes(bool isContainerized)
		{
			return isContainerized ? FCLEquipmentNeededList : LCLAIREquipmentNeededList;
		}

		FCLEquipmentNeededList FCLEquipmentNeededList
		{
			get { return fclEquipmentNeededList ?? (fclEquipmentNeededList = new FCLEquipmentNeededList()); }
		}
		FCLEquipmentNeededList fclEquipmentNeededList;

		LCLAIREquipmentNeededList LCLAIREquipmentNeededList
		{
			get { return lclAIREquipmentNeededList ?? (lclAIREquipmentNeededList = new LCLAIREquipmentNeededList()); }
		}
		LCLAIREquipmentNeededList lclAIREquipmentNeededList;

		#endregion

		#region Message status list

		public CodeDescriptionPairList MessageStatuses
		{
			get
			{
				if (messageStatuses == null)
				{
					messageStatuses = new CodeDescriptionPairList();
					messageStatuses.AddPair(Constants.CartageLegDispatchStatusList.Codes.NotStarted, Constants.CartageLegDispatchStatusList.Descriptions.NotStarted);
					messageStatuses.AddPair(Constants.CartageLegDispatchStatusList.Codes.Delivered, Constants.CartageLegDispatchStatusList.Descriptions.Delivered);
					messageStatuses.AddPair(Constants.CartageLegDispatchStatusList.Codes.Futile, Constants.CartageLegDispatchStatusList.Descriptions.Futile);
					messageStatuses.AddPair(Constants.CartageLegDispatchStatusList.Codes.PickedUp, Constants.CartageLegDispatchStatusList.Descriptions.PickedUp);
					messageStatuses.AddPair(Constants.CartageLegDispatchStatusList.Codes.Rejected, Constants.CartageLegDispatchStatusList.Descriptions.Rejected);
					messageStatuses.AddPair(Constants.CartageLegDispatchStatusList.Codes.Runsheet, Constants.CartageLegDispatchStatusList.Descriptions.Runsheet);
					messageStatuses.AddPair(Constants.CartageLegDispatchStatusList.Codes.WIP, Constants.CartageLegDispatchStatusList.Descriptions.WIP);
				}
				return messageStatuses;
			}
		}
		CodeDescriptionPairList messageStatuses;

		#endregion

		#region AirVentFlowRateUnits

		public CodeDescriptionPairList AirVentFlowRateUnits
		{
			get
			{
				if (airVentFlowRateUnits == null)
				{
					airVentFlowRateUnits = new CodeDescriptionPairList();
					airVentFlowRateUnits.AddPair("2L", Res.GetString("813e5e0b-3577-4223-9af7-59c11a4c5fb2", "Cubic feet per minute"));
					airVentFlowRateUnits.AddPair("MQH", Res.GetString("a3742e3c-6ff0-43f1-96cd-3ca793b701e4", "Cubic meters per hour"));
					airVentFlowRateUnits.AddPair("P1", Res.GetString("996cc0a2-a02f-4a6a-a9b0-50391f823e96", "Percent"));
				}
				return airVentFlowRateUnits;
			}
		}
		CodeDescriptionPairList airVentFlowRateUnits;

		#endregion

		#region TemperatureUnits

		public CodeDescriptionPairList TemperatureUnits
		{
			get { return temperatureUnits ?? (temperatureUnits = new CodeDescriptionPairList(OLookUpEditType.TemperatureTypes)); }
		}
		CodeDescriptionPairList temperatureUnits;

		#endregion

		#region RadioactiveUnits

		public CodeDescriptionPairList RadioactiveUnits
		{
			get { return radioactiveUnits ?? (radioactiveUnits = new CodeDescriptionPairList(OLookUpEditType.RadioactiveTypes)); }
		}
		CodeDescriptionPairList radioactiveUnits;

		#endregion

		#region JobTypeList

		public CodeDescriptionPairList LoadListTypeList
		{
			get
			{
				if (fLoadListTypeList == null)
				{
					fLoadListTypeList = new CodeDescriptionPairList();
					fLoadListTypeList.AddPair(LoadListTypeList_All, Res.GetString("000de56e-98a3-4fee-9062-9f502c90d6cd", "All Jobs"));
					fLoadListTypeList.AddPair(LoadListTypeList_CFS, Res.GetString("54d61e95-7f81-46a8-ac0a-9a291bd83f6c", "CFS Jobs"));
					fLoadListTypeList.AddPair(LoadListTypeList_TRS, Res.GetString("3af2d82a-1c75-41b0-8f00-27a4a2385e7b", "Transport Jobs"));
				}
				return fLoadListTypeList;
			}
		}

		CodeDescriptionPairList fLoadListTypeList;
		public ZString LoadListTypeList_All = "ALL";
		public ZString LoadListTypeList_CFS = "CFS";
		public ZString LoadListTypeList_TRS = "TRS";

		#endregion

		#region Directions

		public CodeDescriptionPairList OneCharDirections
		{
			get
			{
				if (oneCharDirections == null)
				{
					oneCharDirections = new CodeDescriptionPairList();
					oneCharDirections.AddPair(Constants.CartageDirectionChar.Import, Constants.CartageDirection.Import);
					oneCharDirections.AddPair(Constants.CartageDirectionChar.Export, Constants.CartageDirection.Export);
					oneCharDirections.AddPair(Constants.CartageDirectionChar.Destination, Constants.CartageDirection.Destination);
					oneCharDirections.AddPair(Constants.CartageDirectionChar.Origin, Constants.CartageDirection.Origin);
					oneCharDirections.AddPair(Constants.CartageDirectionChar.Local, Constants.CartageDirection.Local);
					oneCharDirections.AddPair(Constants.CartageDirectionChar.LineHaul, Constants.CartageDirection.LineHaul);
				}
				return oneCharDirections;
			}
		}
		CodeDescriptionPairList oneCharDirections;

		public CodeDescriptionPairList Directions
		{
			get
			{
				if (directions == null)
				{
					directions = new CodeDescriptionPairList();
					directions.AddPair(Constants.CartageDirection.Import, Constants.CartageDirectionDescription.Import);
					directions.AddPair(Constants.CartageDirection.Export, Constants.CartageDirectionDescription.Export);
					directions.AddPair(Constants.CartageDirection.Destination, Constants.CartageDirectionDescription.Destination);
					directions.AddPair(Constants.CartageDirection.Origin, Constants.CartageDirectionDescription.Origin);
					directions.AddPair(Constants.CartageDirection.Local, Constants.CartageDirectionDescription.Local);
					directions.AddPair(Constants.CartageDirection.LineHaul, Constants.CartageDirectionDescription.LineHaul);
				}
				return directions;
			}
		}
		CodeDescriptionPairList directions;

		#endregion

		#region ContainerModes

		public CodeDescriptionPairList OneCharContainerModes
		{
			get
			{
				if (oneCharContainerModes == null)
				{
					oneCharContainerModes = new CodeDescriptionPairList();
					oneCharContainerModes.AddPair(Constants.CartageContainerModeChar.FCL, Constants.CartageContainerMode.FCL);
					oneCharContainerModes.AddPair(Constants.CartageContainerModeChar.EmptyContainer, Constants.CartageContainerMode.EmptyContainer);
					oneCharContainerModes.AddPair(Constants.CartageContainerModeChar.Loose, Constants.CartageContainerMode.Loose);
					oneCharContainerModes.AddPair(Constants.CartageContainerModeChar.FTL, Constants.CartageContainerMode.FTL);
					oneCharContainerModes.AddPair(Constants.CartageContainerModeChar.Mixed, Constants.CartageContainerMode.Mixed);
					oneCharContainerModes.AddPair(Constants.CartageContainerModeChar.Containerized, Constants.CartageContainerMode.Containerized);
				}
				return oneCharContainerModes;
			}
		}
		CodeDescriptionPairList oneCharContainerModes;

		public CodeDescriptionPairList ContainerModes
		{
			get
			{
				if (containerModes == null)
				{
					containerModes = new CodeDescriptionPairList();
					containerModes.AddPair(Constants.CartageContainerMode.FCL, Constants.CartageContainerModeDescription.FCL);
					containerModes.AddPair(Constants.CartageContainerMode.EmptyContainer, Constants.CartageContainerModeDescription.EmptyContainer);
					containerModes.AddPair(Constants.CartageContainerMode.Loose, Constants.CartageContainerModeDescription.Loose);
					containerModes.AddPair(Constants.CartageContainerMode.FTL, Constants.CartageContainerModeDescription.FTL);
					containerModes.AddPair(Constants.CartageContainerMode.Mixed, Constants.CartageContainerModeDescription.Mixed);
					containerModes.AddPair(Constants.CartageContainerMode.Containerized, Constants.CartageContainerModeDescription.Containerized);
				}
				return containerModes;
			}
		}
		CodeDescriptionPairList containerModes;

		#endregion

		#region ShippingTransportModeList

		public CodeDescriptionPairList OneCharConnectingFreightModes
		{
			get
			{
				if (oneCharConnectingFreightModes == null)
				{
					oneCharConnectingFreightModes = new CodeDescriptionPairList();
					oneCharConnectingFreightModes.AddPair("A", Constants.TransportModes.Air);
					oneCharConnectingFreightModes.AddPair("S", Constants.TransportModes.Sea);
					oneCharConnectingFreightModes.AddPair("R", Constants.TransportModes.Road);
					oneCharConnectingFreightModes.AddPair("L", Constants.TransportModes.Rail);
				}
				return oneCharConnectingFreightModes;
			}
		}
		CodeDescriptionPairList oneCharConnectingFreightModes;

		public CodeDescriptionPairList ShippingTransportModeList
		{
			get { return new CodeDescriptionPairList(OLookUpEditType.LocalCartageTransportModes); }
		}

		#endregion

		#region AdditionalServices

		public CodeDescriptionPairList AdditionalServices
		{
			get
			{
				if (additional == null)
				{
					additional = new CodeDescriptionPairList();
					additional.AddPair(Constants.CartageAdditional.Futile, Constants.CartageAdditionalDescritpion.Futile);
					additional.AddPair(Constants.CartageAdditional.AdditionalService, Constants.CartageAdditionalDescritpion.AdditionalService);
				}
				return additional;
			}
		}
		CodeDescriptionPairList additional;

		#endregion

		#region ForwardingPickupDeliveryConfirmTypes

		public CodeDescriptionPairList ForwardingPickupDeliveryConfirmTypes
		{
			get
			{
				if (forwardingPickupDeliveryConfirmTypes == null)
				{
					forwardingPickupDeliveryConfirmTypes = new CodeDescriptionPairList();
					forwardingPickupDeliveryConfirmTypes.AddPair(Core.Constants.PickupDeliveryConfirmTypes.OriginPickup, Res.GetString("62e213eb-6c27-42e2-b174-1cea36e8be8d", "Origin Pickup"));
					forwardingPickupDeliveryConfirmTypes.AddPair(Core.Constants.PickupDeliveryConfirmTypes.DestinationDelivery, Res.GetString("8819c865-aab6-4e57-b3cb-865c56a01773", "Destination Delivery"));
				}
				return forwardingPickupDeliveryConfirmTypes;
			}
		}
		CodeDescriptionPairList forwardingPickupDeliveryConfirmTypes;

		#endregion

		#region PickupDeliveryConfirmTypes

		public CodeDescriptionPairList PickupDeliveryConfirmTypes
		{
			get
			{
				if (pickupDeliveryConfirmTypes == null)
				{
					pickupDeliveryConfirmTypes = new CodeDescriptionPairList();
					pickupDeliveryConfirmTypes.AddPair(Core.Constants.PickupDeliveryConfirmTypes.OriginPickup, Res.GetString("62e213eb-6c27-42e2-b174-1cea36e8be8d", "Origin Pickup"));
					pickupDeliveryConfirmTypes.AddPair(Core.Constants.PickupDeliveryConfirmTypes.DestinationDelivery, Res.GetString("8819c865-aab6-4e57-b3cb-865c56a01773", "Destination Delivery"));
					pickupDeliveryConfirmTypes.AddPair(Core.Constants.PickupDeliveryConfirmTypes.OriginCFSArrival, Res.GetString("bb737f8a-9194-49b6-be79-9df4a6269356", "Origin CFS Arrival"));
					pickupDeliveryConfirmTypes.AddPair(Core.Constants.PickupDeliveryConfirmTypes.OriginCFSDeparture, Res.GetString("04033996-1bf8-4824-a05f-421e9942b425", "Origin CFS Departure"));
					pickupDeliveryConfirmTypes.AddPair(Core.Constants.PickupDeliveryConfirmTypes.DestinationCFSArrival, Res.GetString("47a8482f-8806-4179-a45f-cfae50ea6fa8", "Destination CFS Arrival"));
					pickupDeliveryConfirmTypes.AddPair(Core.Constants.PickupDeliveryConfirmTypes.DestinationCFSDeparture, Res.GetString("3146e55c-ff16-4e3a-839b-5937b97c1e28", "Destination CFS Departure"));
				}
				return pickupDeliveryConfirmTypes;
			}
		}
		CodeDescriptionPairList pickupDeliveryConfirmTypes;

		#endregion
	}
}
