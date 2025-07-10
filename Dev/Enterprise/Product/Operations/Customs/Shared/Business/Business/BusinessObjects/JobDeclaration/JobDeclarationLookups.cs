using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Business.CustomsLists;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Customs.Business
{
	public class JobDeclarationLookups : AutoJobDeclarationLookups, IInvoicesProviderLookups
	{
		public JobDeclarationLookups(AutoJobDeclaration parent)
			: base(parent)
		{
		}

		BaseJobDeclaration Declaration => (BaseJobDeclaration)Parent;

		public virtual OrgHeaderCollection Organisations => new OrgHeaderCollection(Factory);

		public ConsigneeCollection ConsigneeOrganisations => new ConsigneeCollection(Factory);

		public virtual OrganisationsFindBoxCollection ConsigneeList => new OrganisationsFindBoxCollection(Factory);

		public virtual System.Collections.ICollection GoodsOrigin => new RefCountryCollection(Factory);

		public virtual System.Collections.ICollection GoodsDestination => new RefCountryCollection(Factory);

		#region InvoicesToAttach

		public InvoiceHeaderWithNoDeclarationCollection InvoicesToAttach
		{
			get { return GetInvoicesToAttachCore(); }
		}

		protected virtual InvoiceHeaderWithNoDeclarationCollection GetInvoicesToAttachCore()
		{
			return new AttachInvoiceCollection(Declaration);
		}

		#endregion

		#region OrgHeader Lists

		protected OrgHeaderCollection fExporters;

		public override OrgHeaderCollection Exporters
		{
			get
			{
				if (fExporters == null)
				{
					fExporters = new OrgHeaderCollection(Factory);
				}
				return fExporters;
			}
		}

		protected OrgHeaderCollection fSellingAgents;

		public override OrgHeaderCollection SellingAgents
		{
			get
			{
				if (fSellingAgents == null)
				{
					fSellingAgents = new OrgHeaderCollection(Factory);
				}
				return fSellingAgents;
			}
		}

		protected ShippingProviderCollection fCarrierOrganisations;

		public ShippingProviderCollection CarrierOrganisations
		{
			get
			{
				if (fCarrierOrganisations == null)
				{
					fCarrierOrganisations = new ShippingProviderCollection(Factory);
				}
				return fCarrierOrganisations;
			}
		}

		protected ConsignorCollection fSuppliersList;

		[OrganisationDefaultProvider(OrganisationType = OrganisationTypes.Consignor)]
		public ConsignorCollection SuppliersList
		{
			get
			{
				if (fSuppliersList == null)
				{
					fSuppliersList = new ConsignorCollection(Factory);
				}
				return fSuppliersList;
			}
		}

		protected ConsigneeCollection fImportersList;

		[OrganisationDefaultProvider(OrganisationType = OrganisationTypes.Consignee)]
		public virtual ConsigneeCollection ImportersList
		{
			get
			{
				if (fImportersList == null)
				{
					fImportersList = new ConsigneeCollection(Factory);

					fImportersList.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(
				"Consignee - Related Consignor", "Property",
				delegate
				{
					return (Declaration.Supplier != null && Declaration.Supplier.BuyerLinks.Count > 0) ? Declaration.Supplier.PK : ZGuid.Empty;
				}));
				}

				return fImportersList;
			}
		}

		public ShippingProviderCollection ShippingLineList
		{
			get
			{
				if (Declaration.IsAir)
				{
					return AirShippingLineList;
				}
				else if (Declaration.IsSea)
				{
					return SeaShippingLineList;
				}
				else
				{
					return AirOrSeaShippingLineList;
				}
			}
		}

		protected ShippingProviderCollection fAirOrSeaShippingLineList;

		public virtual OrgHeaderCollection ShipToParties
		{
			get { return new ConsigneeCollection(Factory); }
		}

		public virtual OrgHeaderCollection SoldToParties
		{
			get { return new ConsigneeCollection(Factory); }
		}

		public virtual OrgHeaderCollection SellerConsignors
		{
			get { return new ConsignorCollection(Factory); }
		}

		[OrganisationDefaultProvider(OrganisationType = OrganisationTypes.Carrier)]
		public ShippingProviderCollection AirOrSeaShippingLineList
		{
			get
			{
				if (fAirOrSeaShippingLineList == null)
				{
					fAirOrSeaShippingLineList = new ShippingProviderCollection(Factory);
				}
				return fAirOrSeaShippingLineList;
			}
		}

		protected AirShippingProviderCollection fAirShippingLineList;

		[OrganisationDefaultProvider(OrganisationType = OrganisationTypes.Carrier)]
		public AirShippingProviderCollection AirShippingLineList
		{
			get
			{
				if (fAirShippingLineList == null)
				{
					fAirShippingLineList = new AirShippingProviderCollection(Factory);
				}
				return fAirShippingLineList;
			}
		}

		protected SeaShippingProviderCollection fSeaShippingLineList;

		[OrganisationDefaultProvider(OrganisationType = OrganisationTypes.Carrier)]
		public SeaShippingProviderCollection SeaShippingLineList
		{
			get
			{
				if (fSeaShippingLineList == null)
				{
					fSeaShippingLineList = new SeaShippingProviderCollection(Factory);
				}
				return fSeaShippingLineList;
			}
		}

		protected ForwarderCollection fForwarderList;

		[OrganisationDefaultProvider(OrganisationType = OrganisationTypes.Forwarder)]
		public ForwarderCollection ForwarderList
		{
			get
			{
				if (fForwarderList == null)
				{
					fForwarderList = new ForwarderCollection(Factory);
				}
				return fForwarderList;
			}
		}

		protected LocalTransportCollection fCartageList;
		public LocalTransportCollection CartageList
		{
			get
			{
				if (fCartageList == null)
				{
					fCartageList = new LocalTransportCollection(Factory);
				}
				return fCartageList;
			}
		}

		protected DepotCollection fDepotCollection;
		public DepotCollection DepotCollection
		{
			get
			{
				if (fDepotCollection == null)
				{
					fDepotCollection = new DepotCollection(Factory);
				}
				return fDepotCollection;
			}
		}

		protected BondedWarehouseCollection fBondedWarehouseCollection;
		public BondedWarehouseCollection BondedWarehouseCollection
		{
			get
			{
				if (fBondedWarehouseCollection == null)
				{
					fBondedWarehouseCollection = new BondedWarehouseCollection(Factory);
				}
				return fBondedWarehouseCollection;
			}
		}

		protected ContainerYardCollection fContainerYardCollection;
		public ContainerYardCollection ContainerYardCollection
		{
			get
			{
				if (fContainerYardCollection == null)
				{
					fContainerYardCollection = new ContainerYardCollection(Factory);
				}
				return fContainerYardCollection;
			}
		}

		public OrgHeaderCollection ContainerTerminalOperatorCollection
		{
			get
			{
				OrgHeaderCollection result;

				if (Declaration.IsAir)
				{
					result = new AirCTOCollection(Declaration.Factory);
				}
				else if (Declaration.IsSea)
				{
					result = new SeaCTOCollection(Declaration.Factory);
				}
				else
				{
					result = new CTOCollection(Declaration.Factory);
				}

				return result;
			}
		}

		OrgHeaderCollection fPickupDeliveryOrganisations;
		public OrgHeaderCollection PickupDeliveryOrganisations
		{
			get
			{
				if (fPickupDeliveryOrganisations == null)
				{
					fPickupDeliveryOrganisations = new OrgHeaderCollection(Factory);
				}
				return fPickupDeliveryOrganisations;
			}
		}

		public override OrgHeaderCollection ExternalBrokers
		{
			get { return externalBrokers ?? (externalBrokers = new BrokerCollection(Factory)); }
		}
		BrokerCollection externalBrokers;

		public override OrgHeaderCollection ControllingAgents
		{
			get { return controllingAgents ?? (controllingAgents = new ControllingAgentCollection(Factory)); }
		}
		ControllingAgentCollection controllingAgents;

		public override OrgHeaderCollection ControllingCustomers
		{
			get { return controllingCustomers ?? (controllingCustomers = new ControllingCustomerCollection(Factory)); }
		}
		ControllingCustomerCollection controllingCustomers;

		public virtual OrgHeaderCollection Distributors
		{
			get { return new OrgHeaderCollection(Factory); }
		}

		public virtual OrgHeaderCollection Packagers
		{
			get { return new OrgHeaderCollection(Factory); }
		}

		public virtual OrgHeaderCollection Shippers
		{
			get { return new OrgHeaderCollection(Factory); }
		}

		public override OrgHeaderCollection NotifyParties
		{
			get { return notifyParties ?? (notifyParties = new OrgHeaderCollection(Factory)); }
		}
		protected OrgHeaderCollection notifyParties;

		public override OrgHeaderCollection Buyers
		{
			get { return buyers ?? (buyers = new OrgHeaderCollection(Factory)); }
		}
		protected OrgHeaderCollection buyers;

		#endregion

		#region RefUnLoco lists

		#region Origins

		public override RefUNLOCOCollection Origins
		{
			get
			{
				RefUNLOCOCollection result = new RefUNLOCOCollection(Declaration.Factory, OriginPortFilter(), LocoMapSystemUsage);
				result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Code", "Property", Declaration.JE_RL_NKOrigin));
				return result;
			}
		}

		protected virtual ZQuery OriginPortFilter()
		{
			PortLocation portLocation = PortLocation.All;
			if (Declaration.IsExport && !Declaration.IsImport)
			{
				portLocation = PortLocation.Local;
			}
			else if (Declaration.IsImport && !Declaration.IsExport)
			{
				portLocation = PortLocation.Foreign;
			}
			return PortQuery("", portLocation);
		}

		protected virtual ZQuery LoadingPortFilter()
		{
			return OriginPortFilter();
		}

		#endregion

		#region Destination

		public RefUNLOCOCollection DestinationList
		{
			get { return DestinationListCore; }
		}

		protected virtual RefUNLOCOCollection DestinationListCore
		{
			get { return new RefUNLOCOCollection(Declaration.Factory, DestinationPortFilter(), LocoMapSystemUsage); }
		}

		protected virtual ZQuery DestinationPortFilter()
		{
			PortLocation portLocation = PortLocation.All;
			if (Declaration.IsImport && !Declaration.IsExport)
			{
				portLocation = PortLocation.Local;
			}
			else if (Declaration.IsExport && !Declaration.IsImport)
			{
				portLocation = PortLocation.Foreign;
			}
			return PortQuery("", portLocation);
		}

		#endregion

		#region Final Destination

		public override RefUNLOCOCollection FinalDestinations
		{
			get
			{
				RefUNLOCOCollection result = new RefUNLOCOCollection(Declaration.Factory, FinalDestinationPortFilter(), LocoMapSystemUsage);
				result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Code", "Property", Declaration.JE_RL_NKFinalDestination));
				return result;
			}
		}

		protected virtual ZQuery FinalDestinationPortFilter()
		{
			ZQuery result = null;
			if (Declaration.IsImport)
			{
				var origin = Declaration.JE_RL_NKOrigin;
				if (!origin.IsEmpty)
				{
					result = new ZQuery(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.DoesNotStartWith, origin.Left(2));
				}
			}
			else if (Declaration.IsExport)
			{
				result = PortQuery(string.Empty, PortLocation.Foreign);
			}

			return result ?? PortQuery(string.Empty, PortLocation.All);
		}

		#endregion

		#region PortOfArrivals

		public override RefUNLOCOCollection PortOfArrivals
		{
			get
			{
				RefUNLOCOCollection result = new RefUNLOCOCollection(Declaration.Factory, DischargePortFilter(), LocoMapSystemUsage);
				result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Code", "Property", Declaration.JE_RL_NKPortOfArrival));
				return result;
			}
		}

		protected virtual ZQuery DischargePortFilter()
		{
			PortLocation portLocation = PortLocation.All;
			if (Declaration.IsImport && !Declaration.IsExport)
			{
				portLocation = PortLocation.Local;
			}
			else if (Declaration.IsExport && !Declaration.IsImport)
			{
				portLocation = PortLocation.Foreign;
			}
			return PortQuery(Declaration.JE_TransportMode, portLocation);
		}

		public enum PortLocation { Local, Foreign, All }

		protected virtual ZQuery PortQuery(string transportMode, PortLocation localForeign)
		{
			ZQuery result = new ZQuery();

			if (!string.IsNullOrEmpty(transportMode))
			{
				SchemaBoolColumn filterColumn = null;
				switch (transportMode)
				{
					case Core.Constants.TransportModes.Air:
						filterColumn = RefUNLOCOSchema.RL_HasAirport;
						break;
					case Core.Constants.TransportModes.Sea:
						filterColumn = RefUNLOCOSchema.RL_HasSeaport;
						break;
					case Core.Constants.TransportModes.Rail:
						filterColumn = RefUNLOCOSchema.RL_HasRail;
						break;
					case Core.Constants.TransportModes.Mail:
						filterColumn = RefUNLOCOSchema.RL_HasPost;
						break;
					default:
						break;
				}
				if (filterColumn != null)
				{
					result.AddToFilter(filterColumn, true);
				}
			}
			if (localForeign != PortLocation.All)
			{
				var oper = localForeign == PortLocation.Local ? SQLComparisonOperator.StartsWith : SQLComparisonOperator.DoesNotStartWith;
				var joinCondition = oper == SQLComparisonOperator.StartsWith ? JoinCondition.Or : JoinCondition.And;
				var subQuery = new ZQuery();
				foreach (var country in CountriesUnderTheSameCustomsJurisdiction)
				{
					subQuery.AddToFilter(joinCondition, RefUNLOCOSchema.RL_Code, oper, country);
				}
				result.AddToFilter(subQuery);
			}
			return result;
		}

		protected virtual IEnumerable<ZString> CountriesUnderTheSameCustomsJurisdiction
		{
			get { yield return Declaration.CountryCode; }
		}

		#endregion

		protected virtual ZString LocoMapSystemUsage
		{
			get { return ZString.Empty; }
		}

		#endregion

		public static string BranchesAdditionalFilterNotMatchedError
		{
			get { return Res.GetString("4dae2343-91e1-4c2b-b368-0ec830ac7bc5", "This branch belongs to a different company and you cannot transfer this job to this branch."); }
		}
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter Name, should not be translated")]
		public const string ImporterSupplierFilterName = "Importer / Supplier";

		protected GlbBranchCollection fBranchCollection;
		public GlbBranchCollection BranchCollection
		{
			get
			{
				if (fBranchCollection == null)
				{
					ZQuery filter = new ZQuery(GlbBranchSchema.GB_GC, GlbCompany.CurrentCompany.PK);
					fBranchCollection = new GlbBranchCollection(Factory, filter);
					fBranchCollection.SetOverrideNotificationWhenAdditionalFilterNotMet(BranchesAdditionalFilterNotMatchedError);
				}
				return fBranchCollection;
			}
		}

		public RefCurrencyCollection CurrencyList
		{
			get { return new RefCurrencyCollection(Factory); }
		}

		public CodeDescriptionPairList SortedInvoiceList
		{
			get { return Declaration.SortedInvoiceList; }
		}

		public BusinessObjectCollection ExportDeclarations
		{
			get
			{
				return new ExportDeclarationCollection(Factory, Declaration);
			}
		}

		#region CodeDescriptionPairList

		public virtual CodeDescriptionPairList WarehouseTransactionStatusList => Factory.GetCachedValue<WarehouseTransactionStatusList>();

		public virtual CodeDescriptionPairList ConsolidatedCargoStatusList => new CodeDescriptionPairList();

		public virtual CodeDescriptionPairList MessageStatusList => Factory.GetCachedValue<Common.Shared.MessageStatusList>();

		public virtual CodeDescriptionPairList MessageStatusCategoryList => Factory.GetCachedValue<Common.Shared.MessageStatusCategoryList>();

		public virtual CodeDescriptionPairList OperationalStatusList => new CodeDescriptionPairList();

		public virtual CodeDescriptionPairList EFTModeList => new CodeDescriptionPairList();

		public virtual CodeDescriptionPairList PaymentPartyList => Factory.GetCachedValue<PaymentPartyCodeDescriptionList>();

		public CodeDescriptionPairList PaidByList => Factory.GetCachedValue<MasterFiles.Business.Customs.PaidByCodeList>();

		public virtual CodeDescriptionPairList IncoTermList => Factory.GetCachedValue("Declaration|IncoTermList", () => new IncoTermsCodeDescriptionPairList(IncoTermsListType.ActiveIncoTerms));

		public virtual CodeDescriptionPairList JE_TotalNoOfPacksPackType_List => RefPackTypeCollection.GetAsCodeDescriptionPairWithStandardUnits(Factory);

		public virtual CodeDescriptionPairList CargoIdTypeList
		{
			get
			{
				return Factory.GetCachedValue("BaseDeclarationCargoIdTypeList", () =>
					{
						var result = new CodeDescriptionPairList();
						result.AddPair(Enterprise.Core.Constants.ContainerModes.Containerised, Enterprise.Core.Constants.ContainerModeDescriptions.Containerised);
						result.AddPair(Enterprise.Core.Constants.ContainerModes.BreakBulk, Enterprise.Core.Constants.ContainerModeDescriptions.BreakBulk);
						result.AddPair(Enterprise.Core.Constants.ContainerModes.Bulk, Enterprise.Core.Constants.ContainerModeDescriptions.Bulk);
						result.AddPair(Enterprise.Core.Constants.ContainerModes.Liquid, Enterprise.Core.Constants.ContainerModeDescriptions.Liquid);
						return result;
					});
			}
		}

		public virtual CodeDescriptionPairList TransportTypeList => Factory.GetCachedValue<TransportTypeList>();

		public virtual CodeDescriptionPairList TransportMeansList => Factory.GetCachedValue<TransportMeansList>();

		/// <summary>
		/// This is a generic list used for Documentation and all areas where the locally defined Customs modes are not what is required. Should never be overridden.
		/// </summary>
		public TransportTypeGenericList TransportTypeGenericList => Factory.GetCachedValue<TransportTypeGenericList>();

		public CodeDescriptionPairList MessageTypeList
		{
			get
			{
				var nonSupportedMessageTypeCodes = GetNonSupportedMessageTypeCodes().ToArray();
				var key = GetMessageTypeListKey(nonSupportedMessageTypeCodes);
				return Factory.GetCachedValue("BaseDeclarationMessageTypeList" + key, () =>
					{
						var fMessageTypeList = JobMessageTypeList.GetNewListFor(Declaration.CountryCode);
						foreach (var code in nonSupportedMessageTypeCodes)
						{
							fMessageTypeList.RemoveCode(code);
						}
						return fMessageTypeList;
					});
			}
		}

		string GetMessageTypeListKey(IEnumerable<ZString> nonSupportedMessageTypeCodes)
		{
			var keyBuilder = new ZStringBuilder(Declaration.CountryCode);
			foreach (var code in nonSupportedMessageTypeCodes)
			{
				keyBuilder.AppendIfNotEmpty(code);
			}
			return keyBuilder.ToStringWithDelimiterBetweenAppends("-");
		}

		protected virtual IEnumerable<ZString> GetNonSupportedMessageTypeCodes()
		{
			return new ZString[3] { EUJobMessageTypeList.Codes.NctsArrivalNotification,
									EUJobMessageTypeList.Codes.NctsDeparture,
									EUJobMessageTypeList.Codes.NctsArrivalUnloadingRemarks };
		}

		public virtual CodeDescriptionPairList MessageSubTypeList
		{
			get { return new CodeDescriptionPairList(); }
		}

		public virtual CodeDescriptionPairList EntryStatusList
		{
			get
			{
				if (Declaration.IsDeclarationIntegrated && Declaration.IsABMInterfaceActivated)
				{
					return EntryStatusListForCustomsWare;
				}
				else
				{
					var statusListInZZ = Declaration.IsInterface
						? GetEntryStatusListInZZ(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatusForInterface, EntryStatusListCodeType)
						: GetEntryStatusListInZZ(EntryStatusListCodeType, ZString.Empty);
					return statusListInZZ.Count == 0 ? EntryStatusListForDefaultFallBack : statusListInZZ;
				}
			}
		}

		protected virtual CodeDescriptionPairList GetEntryStatusListInZZ(ZString codeType, ZString fallback)
		{
			var dataGrouping = Declaration.GetDefaultDataGroupingCode();
			var statusListInZZ = RefCusCodeListTypes.GetCachedList(Factory, dataGrouping, codeType, ZDateTime.Today);

			if (statusListInZZ.Count == 0 && !fallback.IsEmpty)
			{
				statusListInZZ = RefCusCodeListTypes.GetCachedList(Factory, dataGrouping, fallback, ZDateTime.Today);
			}

			return statusListInZZ;
		}

		protected virtual ZString EntryStatusListCodeType => Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus;

		protected virtual CodeDescriptionPairList EntryStatusListForCustomsWare => new CodeDescriptionPairList();

		protected virtual CodeDescriptionPairList EntryStatusListForDefaultFallBack => new CodeDescriptionPairList();

		public CodeDescriptionPairList JE_ExportGoodsType_List
		{
			get { return GetJE_ExportGoodsType_List(); }
		}

		protected virtual CodeDescriptionPairList GetJE_ExportGoodsType_List()
		{
			return Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.ExportGoodsType);
		}

		public virtual CodeDescriptionPairList WeightUnitList
		{
			get { return Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Weight); }
		}

		public virtual CodeDescriptionPairList VolumeUnitList
		{
			get { return Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Volume); }
		}

		public virtual CodeDescriptionPairList MergeByList
		{
			get { return Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.CommercialInvoiceMergeMethod); }
		}

		public CodeDescriptionPairList BillFilterByList
		{
			get { return Factory.GetCachedValue<BillFilterByList>(); }
		}

		public CodeDescriptionPairList ScreeningStatusesList
		{
			get { return Factory.GetCachedValue<ScreeningStatusesList>(); }
		}

		public virtual CodeDescriptionPairList ApplicationCodeList
		{
			get
			{
				return IntegratedCountryHelper.CountryHasBuiltInDeclaration(Declaration.CountryCode) ||
					IntegratedCountryHelper.CountryHasDeclarationInDevelopment(Declaration.CountryCode) ?
					Factory.GetCachedValue<DeclarationApplicationCodeList>() :
					ITFOnlyApplicationCodeList;
			}
		}

		public CodeDescriptionPairList ITFOnlyApplicationCodeList => Factory.GetCachedValue("DeclarationApplicationCodeListITFOnly", () =>
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(DeclarationApplicationCodeList.Codes.Interfaced, DeclarationApplicationCodeList.Descriptions.Interfaced);
			return result;
		});

		public virtual ICodeDescriptionPairList CustomsOfficeList
		{
			get { return new CodeDescriptionPairList(); }
		}

		public virtual ICodeDescriptionPairList DeclarantTypeList
		{
			get { return new CodeDescriptionPairList(); }
		}

		public virtual IBusinessObjectCollection CarrierCodeCollection
		{
			get { return null; }
		}

		public virtual IBusinessObjectCollection LocationOfGoodsCollection
		{
			get { return null; }
		}

		public virtual IBusinessObjectCollection SubLocationOfGoodsCollection
		{
			get { return null; }
		}

		#endregion

		#region IInvoicesProviderLookups members

		IBusinessObjectCollection IInvoicesProviderLookups.InvoicesToAttach
		{
			get { return InvoicesToAttach; }
		}

		#endregion

#if DEBUG
		public virtual CodeDescriptionPairList GetEffectiveMessageSubTypeList(ZString messageType)
		{
			return Declaration.Lookups.MessageSubTypeList;
		}

		public virtual CodeDescriptionPairList GetEffectiveTransportTypeList(ZString messageType)
		{
			return Declaration.Lookups.TransportTypeList;
		}
#endif

		public BusinessObjectCollection IATALoadPorts
		{
			get { return IATALoadPortsCore; }
		}

		protected virtual BusinessObjectCollection IATALoadPortsCore
		{
			get { return null; }
		}

		public CodeDescriptionPairList PackingUnitTypesList
		{
			get { return PackingUnitTypesListCore; }
		}

		protected virtual CodeDescriptionPairList PackingUnitTypesListCore => RefCusCodeListTypes.GetCachedList(Factory, Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, Declaration.DateOfValuation);

		public RefVesselCollection InlandVesselNamesOrLloyds => GetInlandVesselNamesOrLloydsCore();
		protected virtual RefVesselCollection GetInlandVesselNamesOrLloydsCore() => new RefVesselCollection(Factory);

		public RefVesselCollection Vessels => GetVesselCollectionCore();

		protected virtual RefVesselCollection GetVesselCollectionCore() => new RefVesselCollection(Factory);

		public OrganisationsFindBoxCollection DeclarantOfficeList => new OrganisationsFindBoxCollection(Factory);

		public virtual CodeDescriptionPairList DeclarationLanguageList => new CodeDescriptionPairList();

		public CodeDescriptionPairList EntryPhaseStatusList => GetEntryPhaseStatusListCore;

		protected virtual CodeDescriptionPairList GetEntryPhaseStatusListCore => new CodeDescriptionPairList();
	}
}
