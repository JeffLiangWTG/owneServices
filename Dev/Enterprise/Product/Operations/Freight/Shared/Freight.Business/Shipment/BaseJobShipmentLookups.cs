using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Freight.Common.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business
{
	public class BaseJobShipmentLookups : JobShipmentLookups
	{
		public BaseJobShipmentLookups(AutoJobShipment parent)
			: base(parent)
		{
		}

		public CodeDescriptionPairList CommunityTransitStatusCodes => CommunityTransitStatusCodesGetter.GetList();

		protected virtual CommunityTransitStatusCodes CommunityTransitStatusCodesGetter
			=> communityTransitStatusCodes ?? (communityTransitStatusCodes = new CommunityTransitStatusCodes(Factory));

		CommunityTransitStatusCodes communityTransitStatusCodes;

		public CommonShipment Shipment
		{
			get { return Parent as CommonShipment; }
		}

		#region Binding Lists

		protected internal BindToLists BindingLists
		{
			get { return BindToLists.GetCachedLists(Factory); }
		}

		public virtual RefUNLOCOCollection RefUNLOCO_List
		{
			get { return BindingLists.RefUNLOCO_List; }
		}

		public virtual OrgHeaderCollection OrgHeader_List
		{
			get { return BindingLists.OrgHeader_List; }
		}

		public virtual OrgContactCollection OrgContact_List
		{
			get { return BindingLists.OrgContact_List; }
		}

		public virtual RefServiceLevelCollection RefServiceLevel_List
		{
			get { return BindingLists.RefServiceLevel_List; }
		}

		public virtual RefCommodityCodeCollection RefCommodity_List
		{
			get { return BindingLists.RefCommodityCode_List; }
		}

		public virtual RefCurrencyCollection RefCurrency_List
		{
			get { return BindingLists.RefCurrency_List; }
		}

		public virtual GlbBranchCollection GlbBranch_List
		{
			get { return BindingLists.GlbBranch_List; }
		}

		public virtual ShippingProviderCollection ShippingProvider_List
		{
			get { return BindingLists.ShippingProvider_List; }
		}

		public virtual PackDepotCollection PackDepot_List
		{
			get { return BindingLists.PackDepot_List; }
		}

		public virtual UnpackDepotCollection UnpackDepot_List
		{
			get { return BindingLists.UnpackDepot_List; }
		}

		public virtual ForwarderCollection ForwarderList
		{
			get { return BindingLists.OrgForwarder_List; }
		}

		[OrganisationDefaultProvider(OrganisationType = OrganisationTypes.Forwarder)]
		public ForwarderCollection Forwarder_List
		{
			get { return ForwarderList; }
		}

		[OrganisationDefaultProvider(DocAddressType = DocAddressTypes.Codes.PickupAgent)]
		public ForwarderCollection PickupAgent_List
		{
			get { return ForwarderList; }
		}

		public virtual ForwarderCollection ForwarderDefaultOnly_List
		{
			get { return BindingLists.OrgForwarderDefaultOnly_List; }
		}

		[OrganisationDefaultProvider(OrganisationType = OrganisationTypes.Broker, OrganisationSubType = OrganisationsSubTypeList.Codes.ImportBroker)]
		public virtual BrokerCollection Broker_List
		{
			get { return BindingLists.OrgMiscServBroker_List; }
		}

		[OrganisationDefaultProvider(OrganisationType = OrganisationTypes.Broker, OrganisationSubType = OrganisationsSubTypeList.Codes.ExportBroker)]
		public BrokerCollection ExportBroker_List
		{
			get { return Broker_List; }
		}

		[OrganisationDefaultProvider(OrganisationType = OrganisationTypes.Carrier)]
		public virtual LocalTransportCollection LocalTransport_List
		{
			get { return BindingLists.OrgMiscServLocalTransport_List; }
		}

		public virtual UNDGSubstanceCollection UNDGSubstances_List
		{
			get { return BindingLists.UNDGSubstance_List; }
		}

		public virtual TransportShippingProviderCollection ShippingLine_List
		{
			get { return BindingLists.ShippingLine_List; }
		}

		#endregion

		#region Consol_List

		public MainFormConsolCollection Consols_List
		{
			get
			{
				if (fConsols_List == null)
				{
					fConsols_List = GetNewMainFormConsolCollection();
					fConsols_List.ParentShipment = Shipment;
					fConsols_List.Sort(JobConsolSchema.Constants.JK_UniqueConsignRef, System.ComponentModel.ListSortDirection.Descending);
				}
				if (!Shipment.IsDeleted)
				{
					SetFiltersOnMainFormConsolCollection(fConsols_List);
				}
				return fConsols_List;
			}
		}
		MainFormConsolCollection fConsols_List;

		protected virtual MainFormConsolCollection GetNewMainFormConsolCollection()
		{
			return new MainFormConsolCollection(Factory);
		}

		protected virtual void SetFiltersOnMainFormConsolCollection(MainFormConsolCollection collection)
		{
		}

		#endregion

		public CodeDescriptionPairList JS_DiscrepancyReason_List
		{
			get { return Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.DiscrepancyReason); }
		}

		public CodeDescriptionPairList AWBDimsCodeDescriptionPairList
		{
			get { return Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.AWBDimensions); }
		}

		public virtual LocalTransportCollection LocalTransportAtOrigin_List
		{
			get
			{
				fLocalTransportAtOrigin_List = new LocalTransportCollection(Factory);

				if (!Shipment.JS_RL_NKOrigin.IsEmpty)
				{
					fLocalTransportAtOrigin_List.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault((ZString)OrgConstants.FilterControl.UNLOCOType.OrgPort, "Property", Shipment.JS_RL_NKOrigin));
				}
				return fLocalTransportAtOrigin_List;
			}
		}
		LocalTransportCollection fLocalTransportAtOrigin_List;

		public virtual LocalTransportCollection LocalTransportAtDestination_List
		{
			get
			{
				fLocalTransportAtDestination_List = new LocalTransportCollection(Factory);

				if (!Shipment.JS_RL_NKDestination.IsEmpty)
				{
					fLocalTransportAtDestination_List.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault((ZString)OrgConstants.FilterControl.UNLOCOType.OrgPort, "Property", Shipment.JS_RL_NKDestination));
				}
				return fLocalTransportAtDestination_List;
			}
		}
		LocalTransportCollection fLocalTransportAtDestination_List;

		#region CoLoad Sub Shipments

		public IRelatedShipmentsCollection CoLoadShipment_List
		{
			get
			{
				var relatedShipmentsCollection = GetRelatedShipmentsCollection(false);
				Shipment.AddRelatedShipmentCollectionFilterBusinessObjectDefaults(relatedShipmentsCollection);

				return relatedShipmentsCollection;
			}
		}

		protected virtual IRelatedShipmentsCollection GetRelatedShipmentsCollection(bool isMasterShipment)
		{
			return new RelatedCommonShipmentsCollection(Shipment, isMasterShipment);
		}

		#endregion

		#region Code Master List

		public IRelatedShipmentsCollection CoLoadMaster_List
		{
			get
			{
				var coloadMasterCollection = GetRelatedShipmentsCollection(true);
				Shipment.AdjustCoLoadMasterListFilterAndFilterBusinessObjectDefaults(coloadMasterCollection);

				return coloadMasterCollection;
			}
		}

		#endregion

		#region Consignee Lists

		[OrganisationDefaultProvider(OrganisationType = OrganisationTypes.Consignee)]
		public virtual OrganisationsFindBoxCollection Consignee_List
		{
			get { return AddToConsigneeOrgFilter(OrgConsignee_List); }
		}

		ConsigneeCollection OrgConsignee_List
		{
			get { return orgConsignee_List ?? (orgConsignee_List = new ConsigneeCollection(Factory)); }
		}
		ConsigneeCollection orgConsignee_List;

		[OrganisationDefaultProvider(OrganisationType = OrganisationTypes.Consignee)]
		public virtual OrganisationsFindBoxCollection ConsigneeDefaultOnly_List
		{
			get { return AddToConsigneeOrgFilter(OrgConsigneeDefaultOnly_List); }
		}

		ConsigneeCollection OrgConsigneeDefaultOnly_List
		{
			get
			{
				return orgConsigneeDefaultOnly_List ?? (orgConsigneeDefaultOnly_List = new ConsigneeCollection(Factory)
				{
					AllowOtherOrgTypes = true
				});
			}
		}
		ConsigneeCollection orgConsigneeDefaultOnly_List;

		protected OrganisationsFindBoxCollection AddToConsigneeOrgFilter(OrganisationsFindBoxCollection orgCollection)
		{
			orgCollection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(
				OrgConstants.FilterControl.UNLOCOType.OrgPort, "Property",
				() => Shipment.JS_RL_NKDestination));

			orgCollection.DefaultsForNewChild.Add(OrgHeader.Schema.OH_RL_NKClosestPort, Shipment.JS_RL_NKDestination, ZBool.False);

			orgCollection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(
				"Consignee - Related Consignor", "Property",
				() => (Shipment.Consignor != null && Shipment.Consignor.BuyerLinks.Count > 0)
					? Shipment.ConsignorPK
					: ZGuid.Empty
				));

			// IndexSearch
			orgCollection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(
				"CLOSESTPORT", "Property",
				() => Shipment.JS_RL_NKDestination, SearchType.Index));
			orgCollection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(
				"CONSIGNEERELATEDCONSIGNOR", "Property",
				() => (Shipment.Consignor != null && Shipment.Consignor.BuyerLinks.Count > 0)
					? Shipment.Consignor.OH_Code
					: ZString.Empty
				, SearchType.Index));

			return orgCollection;
		}

		#endregion

		#region Consignor Lists

		[OrganisationDefaultProvider(OrganisationType = OrganisationTypes.Consignor)]
		public virtual OrganisationsFindBoxCollection Consignor_List
		{
			get { return AddToConsignorOrgFilter(OrgConsignor_List); }
		}

		ConsignorCollection OrgConsignor_List
		{
			get { return orgConsignor_List ?? (orgConsignor_List = new ConsignorCollection(Factory)); }
		}
		ConsignorCollection orgConsignor_List;

		[OrganisationDefaultProvider(OrganisationType = OrganisationTypes.Consignor)]
		public virtual OrganisationsFindBoxCollection ConsignorDefaultOnly_List
		{
			get { return AddToConsignorOrgFilter(OrgConsignorDefaultOnly_List); }
		}

		ConsignorCollection OrgConsignorDefaultOnly_List
		{
			get
			{
				return orgConsignorDefaultOnly_List ?? (orgConsignorDefaultOnly_List = new ConsignorCollection(Factory)
				{
					AllowOtherOrgTypes = true
				});
			}
		}
		ConsignorCollection orgConsignorDefaultOnly_List;

		protected OrganisationsFindBoxCollection AddToConsignorOrgFilter(OrganisationsFindBoxCollection orgCollection)
		{
			orgCollection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(
				OrgConstants.FilterControl.UNLOCOType.OrgPort, "Property",
				() => Shipment.JS_RL_NKOrigin));

			orgCollection.DefaultsForNewChild.Add(OrgHeader.Schema.OH_RL_NKClosestPort, Shipment.JS_RL_NKOrigin, ZBool.False);

			orgCollection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(
				"Consignor - Related Consignee", "Property",
				() => (Shipment.Consignee != null && Shipment.Consignee.SupplierLinks.Count > 0)
					? Shipment.ConsigneePK
					: ZGuid.Empty
				));

			// IndexSearch
			orgCollection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(
				"CLOSESTPORT", "Property",
				() => Shipment.JS_RL_NKOrigin, SearchType.Index));
			orgCollection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(
				"CONSIGNORRELATEDCONSIGNEE", "Property",
				() => (Shipment.Consignee != null && Shipment.Consignee.SupplierLinks.Count > 0)
					? Shipment.Consignee.OH_Code
					: ZString.Empty,
				SearchType.Index));
			return orgCollection;
		}

		#endregion

		#region Consignee Forwarders Lists

		/// <summary>
		/// Returns either a ConsigneeCollection or ForwarderCollection depending on IsCoLoadMaster
		/// </summary>

		[OrganisationDefaultProvider(OrganisationType = OrganisationTypes.Consignee)]
		public OrganisationsFindBoxCollection ConsigneeForwarder_List
		{
			get
			{
				return Shipment.IsCoLoadMaster || Shipment.IsBlindCoLoadMaster
					? Forwarder_List
					: Consignee_List;
			}
		}

		public OrganisationsFindBoxCollection ConsigneeForwarderDelivery_List
		{
			get
			{
				return Shipment.IsCoLoadMaster || Shipment.IsBlindCoLoadMaster
					? ForwarderDefaultOnly_List
					: ConsigneeDefaultOnly_List;
			}
		}

		#endregion

		#region Consignor Forwarder Lists

		/// <summary>
		/// Returns either a ConsignorCollection or ForwarderCollection depending on IsCoLoadMaster
		/// </summary>

		[OrganisationDefaultProvider(OrganisationType = OrganisationTypes.Consignor)]
		public OrganisationsFindBoxCollection ConsignorForwarder_List
		{
			get
			{
				return Shipment.IsCoLoadMaster || Shipment.IsBlindCoLoadMaster
					? Forwarder_List
					: Consignor_List;
			}
		}

		public OrganisationsFindBoxCollection ConsignorForwarderPickup_List
		{
			get
			{
				return Shipment.IsCoLoadMaster || Shipment.IsBlindCoLoadMaster
					? ForwarderDefaultOnly_List
					: ConsignorDefaultOnly_List;
			}
		}

		#endregion

		#region Release Type List

		public virtual ReadOnlyCodeDescriptionPairList JS_ReleaseType_List
		{
			get { return Registry.Business.FreightDataRegistry.Instance.ReleaseTypes.Value.GetCodeDescriptionPairList(); }
		}

		#endregion

		#region HBLAWBChargesDisplay_List

		public virtual CodeDescriptionPairList JS_HBLAWBChargesDisplay_List
		{
			get { return DocumentsDataRegistry.Instance.HBLChargesDefaultDisplayTypesPairList; }
		}

		#endregion

		#region Shipped On Board / HouseBill Of Lading Types Lists

		public CodeDescriptionPairList JS_ShippedOnBoard_List
		{
			get
			{
				return Factory.GetCachedValue(
					"Shipment.Lookups.JS_ShippedOnBoard_List",
					delegate
					{
						var list = new CodeDescriptionPairList();
						list.AddPair(FreightConstants.ShippedOnBoardType.Shipped, ResString.GetMultilingualString("ea1e3519-8e70-4f11-bb2b-c7debf1f451f", "Shipped"));
						list.AddPair(FreightConstants.ShippedOnBoardType.Clean, ResString.GetMultilingualString("25150c71-ae3b-4cd1-b483-3de2699af9f7", "Clean"));
						list.AddPair(FreightConstants.ShippedOnBoardType.Laden, ResString.GetMultilingualString("95881fa2-da3c-4995-aa77-457a4d9f331e", "Laden"));
						list.AddPair(FreightConstants.ShippedOnBoardType.Received, ResString.GetMultilingualString("60e4673c-74a1-4f5a-afd3-92876561e4ac", "Received For Shipment"));
						return list;
					});
			}
		}

		public CodeDescriptionPairList JS_HouseBillOfLadingType_List
		{
			get
			{
				if (Shipment.IsSea)
				{
					return HouseBillOfLadingTypesForSea_List.GetCodeDescriptionPairList();
				}
				if (Shipment.IsRoad)
				{
					return HouseBillOfLadingTypesForRoad_List.GetCodeDescriptionPairList();
				}

				if (Shipment.IsRail)
				{
					return HouseBillOfLadingTypesForRail_List.GetCodeDescriptionPairList();
				}

				return new CodeDescriptionPairList();
			}
		}

		public string JS_HouseBillOfLadingTypeDefault
		{
			get
			{
				var defaultCode = string.Empty;

				if (Shipment.IsSea)
				{
					defaultCode = HouseBillOfLadingTypesForSea_List.DefaultCode;
				}

				if (Shipment.IsRoad)
				{
					defaultCode = HouseBillOfLadingTypesForRoad_List.DefaultCode;
				}

				if (Shipment.IsRail)
				{
					defaultCode = HouseBillOfLadingTypesForRail_List.DefaultCode;
				}

				if (string.IsNullOrWhiteSpace(defaultCode))
				{
					defaultCode = JS_HouseBillOfLadingType_List.Count > 0 ? JS_HouseBillOfLadingType_List[0].Code : string.Empty;
				}

				return defaultCode;
			}
		}

		ICodeDescriptionPairListWithDefaultCode HouseBillOfLadingTypesForSea_List
		{
			get
			{
				return Factory.GetCachedValue(
					"Shipment.Lookups.HouseBillOfLadingTypesForSea_List",
					delegate
					{
						var result = new SystemDefinableCodeDescriptionBoolCollection();
						result.AddRange(FreightDataRegistry.Instance.HouseBillOfLadingTypesForSea.Value);
						result.SetDefaultCode(FreightDataRegistry.Instance.HouseBillOfLadingTypesForSea.Value.DefaultCode, false);
						var additionalTypes = FreightDataRegistry.Instance.AddtionalHouseBillOfLadingTypes.Value.Cast<AdditionalHouseBillOfLadingType>().Where(pair => pair.Enable);
						additionalTypes.ForEach(t => result.Add(t));

						return result;
					});
			}
		}

		ICodeDescriptionPairListWithDefaultCode HouseBillOfLadingTypesForRoad_List
		{
			get { return Factory.GetCachedValue("HouseBillOfLadingTypesForRoad", () => FreightDataRegistry.Instance.HouseBillOfLadingTypesForRoad.Value); }
		}

		ICodeDescriptionPairListWithDefaultCode HouseBillOfLadingTypesForRail_List
		{
			get { return Factory.GetCachedValue("HouseBillOfLadingTypesForRail", () => FreightDataRegistry.Instance.HouseBillOfLadingTypesForRail.Value); }
		}

		#endregion

		#region Transport Mode / Shipment Type List

		public virtual CodeDescriptionPairList JS_TransportMode_List
		{
			get
			{
				return Factory.GetCachedValue("FreightCodePairLists.JS_TransportModeList",
					FreightCodePairLists.JS_TransportModeList);
			}
		}

		public virtual CodeDescriptionPairList JS_ShipmentType_List
		{
			get
			{
				var list = FreightCodePairLists.JS_ShipmentTypeList();

				if (!HVLVDataRegistry.HasHVLVClearance)
				{
					list.RemoveCode(Constants.ShipmentTypes.HighVolumeLowValueLegacy);
				}

				if (Shipment.JS_ShipmentType != Constants.ShipmentTypes.HighVolumeLowValueMaster)
				{
					list.RemoveCode(Constants.ShipmentTypes.HighVolumeLowValueMaster);
				}

				return list;
			}
		}

		#endregion

		#region Pack Mode / Type List

		public virtual CodeDescriptionPairList JS_PackingMode_List
		{
			get
			{
				return Factory.GetCachedValue("FreightCodePairLists.JS_PackingModeList_" + Shipment.JS_TransportMode,
					() => FreightCodePairLists.JS_PackingModeList(Shipment.JS_TransportMode));
			}
		}

		public RefPackTypeCollection JS_PackType_List
		{
			get { return new RefPackTypeCollection(Factory); }
		}

		#endregion

		#region HBL Delivery Mode List

		public virtual CodeDescriptionPairList JS_HBLContainerPackModeOverride_List
		{
			get
			{
				var list = new CodeDescriptionPairList();
				var hBLDeliveryModes = FreightUtilities.ShipmentHBLDeliveryMode(Shipment.JS_PackingMode);
				foreach (HBLDeliveryMode item in hBLDeliveryModes.Modes)
				{
					if (item.ShowInList)
					{
						list.AddPair(item.Code, item.Description);
					}
				}

				return list;
			}
		}

		#endregion

		#region INCO List

		public virtual CodeDescriptionPairList JS_INCO_List
		{
			get { return Shipment.IsDomesticFreight ? DomesticPaymentTermsList : InternationalPaymentTermsList; }
		}

		CodeDescriptionPairList DomesticPaymentTermsList
		{
			get { return Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.DomesticPaymentTerms); }
		}

		CodeDescriptionPairList InternationalPaymentTermsList
		{
			get
			{
				return Factory.GetCachedValue("BaseJobShipmentLookups.InternationalPaymentTermsList", () => new IncoTermsCodeDescriptionPairList(IncoTermsListType.ActiveIncoTerms));
			}
		}

		#endregion

		#region Inspection Types

		public CodeDescriptionPairList InspectionTypes
		{
			get
			{
				var key = "Shipment.InspectionTypes." + GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				return Factory.GetCachedValue(key, () =>
				{
					var list = new CodeDescriptionPairList();
					list.AddPair(InspectionType_Approved, FreightUtilities.InspectionType_Approved_Description);
					list.AddPair(InspectionType_Screened, ResString.GetMultilingualString("0700f6c3-9c70-469e-87c0-bf80cc0a41f5", "Screened"));

					foreach (CodeDescriptionPair item in FreightUtilities.SupplyChainSecurityConfiguration.InspectionTypeList)
					{
						list.Add(item);
					}

					list.AddPair(FreightDataRegistry.AviationSecurity_Unknown_Code, FreightDataRegistry.AviationSecurity_Unknown_Description);

					return list;
				});
			}
		}

		public CodeDescriptionPairList AdditionalInspectionTypes
		{
			get
			{
				var key = "Shipment.AdditionalInspectionTypes." + GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				return Factory.GetCachedValue(key, () =>
				{
					var list = new CodeDescriptionPairList();

					foreach (CodeDescriptionPair item in FreightUtilities.SupplyChainSecurityConfiguration.AdditionalInspectionTypeList)
					{
						list.Add(item);
					}

					list.AddPair(FreightDataRegistry.AviationSecurity_Unknown_Code, FreightDataRegistry.AviationSecurity_Unknown_Description);
					list.AddPair(InspectionType_Screened, ResString.GetMultilingualString("5d0d3d99-8d5f-4721-87bd-fe8d276ae792", "Screened"));

					return list;
				});
			}
		}

		public const string InspectionType_Approved = "APP";
		public const string InspectionType_Web = "WEB";
		public const string InspectionType_Transshipment = "TRN";
		public const string InspectionType_Screened = "SCR";

		#endregion

		#region Shipper COD Payment Types

		public ReadOnlyCodeDescriptionPairList ShipperCODPaymentTypes
		{
			get { return Factory.GetCachedValue("Shipment.ShipperCODPaymentTypes", () => FreightDataRegistry.Instance.ShipperCODPaymentTypes.Value); }
		}

		#endregion

		#region WareHouses List

		public IWhsWarehouseCollection Warehouses
		{
			get
			{
				var result = ObjectFactory.Get<IWhsWarehouseCollection>("IWhsWarehouseCollection", Factory);
				result.Load();
				return result;
			}
		}

		#endregion

		#region Unit Of Weight / Volume List

		public CodeDescriptionPairList JS_UnitOfWeight_List
		{
			get { return Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Weight); }
		}

		public CodeDescriptionPairList JS_UnitOfVolume_List
		{
			get { return Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Volume); }
		}

		public CodeDescriptionPairList JS_Calc_ActualVolumeWeightUnit_List => Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.WeightAndVolumeDisplayTypes);

		#endregion

		#region Freight Rate Autorating Mode List

		public CodeDescriptionPairList FreightRateAutoratingMode_List
		{
			get
			{
				return Factory.GetCachedValue("FreightRateAutoratingMode_List",
					() => new CodeDescriptionPairList(OLookUpEditType.FreightRateAutoratingMode));
			}
		}

		#endregion

		#region Notify Party List

		[OrganisationDefaultProvider(OrganisationSubType = OrganisationsSubTypeList.Codes.NotifyParty)]
		public OrganisationsFindBoxCollection NotifyParty_List
		{
			get { return ConsigneeDefaultOnly_List; }
		}

		#endregion

		#region EFreightStatus_List

		public CodeDescriptionPairList EFreightStatus_List
		{
			get
			{
				return Factory.GetCachedValue("EFreightStatus_List",
					() => new CodeDescriptionPairList(OLookUpEditType.EFreightStatus));
			}
		}

		#endregion

		#region ScreeningStatusesList

		public CodeDescriptionPairList ScreeningStatusesList => Factory.GetCachedValue<ScreeningStatusesList>();

		#endregion

		#region GatewayServiceLevels

		public override RefServiceLevelCollection GatewayServiceLevels =>
			new GatewayServiceLevelCollection(Factory);

		#endregion
	}
}
