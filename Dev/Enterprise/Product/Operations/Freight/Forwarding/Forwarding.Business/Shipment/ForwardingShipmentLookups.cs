using System.Collections;
using System.Linq;
using CargoWise.Common.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ContractManagement.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Registry;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Freight.Forwarding.Business
{
	public class ForwardingShipmentLookups : BaseJobShipmentLookups
	{
		public ForwardingShipmentLookups(ForwardingShipment parent)
			: base(parent)
		{
		}

		public new ForwardingShipment Shipment
		{
			get { return Parent as ForwardingShipment; }
		}

		public CodeDescriptionPairList JS_PaymentTermAutoratingOverride_List
		{
			get { return Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.PaymentType); }
		}

		public CodeDescriptionPairList Phases
		{
			get
			{
				return Factory.GetCachedValue("ForwardingShipmentLookups.Phases", () =>
				{
					var list = PhaseConstants.GetCommonPhaseList();
					list.AddRange(Shipment.PhaseResolver.GetPhaseList());
					return list;
				});
			}
		}

		[OrganisationDefaultProvider(OrganisationType = OrganisationTypes.ControllingCustomer)]
		public ControllingCustomerCollection ControllingCustomerList
		{
			get { return BindingLists.OrgControllingCustomerList; }
		}

		[OrganisationDefaultProvider(OrganisationType = OrganisationTypes.ControllingAgent)]
		public ControllingAgentCollection ControllingAgentList
		{
			get { return BindingLists.OrgControllingAgentList; }
		}

		public CodeDescriptionPairList CompanyTariffLevelOverrideList
		{
			get
			{
				if (companyTariffLevelList == null)
				{
					companyTariffLevelList = new CompanyTariffLevelList(Factory);
				}
				return companyTariffLevelList.CompanyTariffLevelOverrideList;
			}
		}

		CompanyTariffLevelList companyTariffLevelList;

		#region Consol_List

		protected override MainFormConsolCollection GetNewMainFormConsolCollection()
		{
			return new MainFormForwardingConsolCollection(Factory);
		}

		protected override void SetFiltersOnMainFormConsolCollection(MainFormConsolCollection collection)
		{
			var provider = new ForwardingConsolDefaultFilterProvider
			{
				TransportMode = Shipment.JS_TransportMode,
				ContainerMode = Shipment.JS_PackingMode,
				LoadPort = Shipment.JS_RL_NKOrigin,
				DischargePort = Shipment.JS_RL_NKDestination,
				ETDFrom = Shipment.JS_E_DEP,
				ETATo = Shipment.JS_E_ARV
			};
			provider.SetDefaultFilters(collection);
		}

		#endregion

		#region JS_ShipmentStatus_List

		public CodeDescriptionPairList JS_ShipmentStatus_List
		{
			get
			{
				var isBooking = Shipment.JS_IsBooking && !Shipment.JS_IsForwardRegistered;

				if (isBooking)
				{
					return Factory.GetCachedValue("ForwardingShipmentLookups.JS_ShipmentStatus_List|Booking", delegate
					{
						var list = new CodeDescriptionPairList();
						list.Add(new CodeDescriptionPair(ShipmentStatusList.Codes.ElectronicBooking, Res.GetString("54008f04-c807-4b80-a2ed-74d126c50611", "eBooking Request Received")));
						list.Add(new CodeDescriptionPair(ShipmentStatusList.Codes.Booked, Res.GetString("0601ca5f-8044-4d5e-a268-4ba8c66f6d7d", "Booking Confirmed")));
						list.Add(new CodeDescriptionPair(ShipmentStatusList.Codes.BookingRejected, ShipmentStatusList.Descriptions.BookingRejected));

						if (Shipment.JS_ShipmentStatus != ShipmentStatusList.Codes.ElectronicBooking)
						{
							list.Add(new CodeDescriptionPair(ShipmentStatusList.Codes.EBookingCancellationRequest, ShipmentStatusList.Descriptions.EBookingCancellationRequest));
							list.Add(new CodeDescriptionPair(ShipmentStatusList.Codes.BookingCancelled, ShipmentStatusList.Descriptions.BookingCancelled));
						}

						return list;
					});
				}
				else if (Shipment.JS_ShipmentStatus == ShipmentStatusList.Codes.Amendment
					|| Shipment.JS_ShipmentStatus == ShipmentStatusList.Codes.Booked
					|| (Shipment.JS_ShipmentStatusInfo.HasChanges && (ZString)Shipment.JS_ShipmentStatusInfo.OriginalValue == ShipmentStatusList.Codes.Amendment))
				{
					return Factory.GetCachedValue("ForwardingShipmentLookups.JS_ShipmentStatus_List|AmendmentOrBookingConfirmed", delegate
					{
						var list = new CodeDescriptionPairList();
						list.Add(new CodeDescriptionPair(ShipmentStatusList.Codes.Amendment, Res.GetString("ace597c7-4d63-4592-860c-c46aedb58287", "Amendment")));
						list.Add(new CodeDescriptionPair(ShipmentStatusList.Codes.Booked, Res.GetString("ac7460ae-12e3-4db2-8059-1719c64badc9", "Booking Confirmed")));

						return list;
					});
				}
				else
				{
					return Factory.GetCachedValue("ForwardingShipmentLookups.JS_ShipmentStatus_List|Forwarding", delegate
					{
						var list = new CodeDescriptionPairList();
						list.Add(new CodeDescriptionPair(ShipmentStatusList.Codes.ElectronicShippingInstruction, Res.GetString("2fcbac9c-5e27-4264-a139-8301938c936b", "eSI Received")));
						list.Add(new CodeDescriptionPair(ShipmentStatusList.Codes.Confirmed, Res.GetString("c253342a-9e1c-4257-8c9d-c8ad2788cc3e", "SI Confirmed")));
						list.Add(new CodeDescriptionPair(ShipmentStatusList.Codes.SIRejected, Res.GetString("5f9b012e-e609-4cd6-9f5e-63e8dae6e08e", "SI Rejected")));

						return list;
					});
				}
			}
		}

		#endregion

		#region RelatedShipmentsCollection

		protected override IRelatedShipmentsCollection GetRelatedShipmentsCollection(bool isMasterShipment)
		{
			return new RelatedForwardingShipmentsCollection(Shipment, isMasterShipment);
		}

		#endregion

		#region ServiceLevelCollection

		[SuppressWeaklyTypedCollectionMessage]
		public IList ServiceLevelOrTransitTimeCollection => GetServiceLevelOrTransitTimeCollection(Shipment, RefServiceLevel_List, Factory);

		[SuppressWeaklyTypedCollectionMessage]
		public static IList GetServiceLevelOrTransitTimeCollection(ForwardingShipment shipment, IList defaultServiceLevels, BusinessObjectFactory defaultFactory)
		{
			var deliveryDueDateCanBeCalculated = shipment != null && DeliveryDueDateCalculator.SupportedDeliveryModes.Contains(shipment.JS_HBLContainerPackModeOverride.ToString());

			if (!FreightDataRegistry.Instance.CalculateDeliveryDueDateByTransportMode.Value.IsTransportModeActive(shipment?.JS_TransportMode ?? ZString.Empty) || !deliveryDueDateCanBeCalculated)
			{
				return defaultServiceLevels;
			}

			var exportReceivingDepot = shipment?.ExportReceivingDepot?.Header;
			var importReleaseDepot = shipment?.ImportReleaseDepot?.Header;

			var destinationZoneOwner = importReleaseDepot ?? shipment.DeliveryAgent;

			return new TransitTimeServiceLevelCombinationCollection(
				shipment?.Factory ?? defaultFactory,
				exportReceivingDepot,
				destinationZoneOwner,
				exportReceivingDepot == null ? shipment?.ConsignorPickupAddress : null,
				destinationZoneOwner == null ? shipment?.ConsigneeDeliveryAddress : null,
				rateModesList.ContainsCode(shipment?.JS_PackingMode) ? shipment?.JS_PackingMode : shipment?.JS_TransportMode ?? Core.Constants.TransportModes.All);
		}

		#endregion

		#region AllocationRouteCollection

		public RatingContractAllocationLineCollection AllocationRouteCollection
		{
			get
			{
				return Factory.GetCachedValue("ForwardingShipmentLookups|AllocationRouteCollection", () =>
				{
					return new RatingContractAllocationLineCollection(Factory);
				});
			}
		}

		#endregion

		#region CarrierContractCollection

		public CarrierContractCollection CarrierContractCollection
		{
			get
			{
				return Factory.GetCachedValue("ForwardingShipmentLookups|CarrierContractCollection", () =>
				{
					return new CarrierContractCollection(Factory);
				});
			}
		}

		#endregion

		#region JS_ElectronicBillOfLadingBillStatus_List

		public CodeDescriptionPairList JS_ElectronicBillOfLadingBillStatus_List => Factory.GetCachedValue("FreightCodePairLists.HouseBillOfLadingBillStatusList", () => FreightCodePairLists.HouseBillOfLadingBillStatusList());

		#endregion

		#region Holder Lists

		public OrganisationsFindBoxCollection Holder_List
		{
			get { return orgHolder_List ?? (orgHolder_List = new HolderCollection(Shipment)); }
		}
		HolderCollection orgHolder_List;

		#endregion

		#region SurrenderParty Lists

		public OrganisationsFindBoxCollection SurrenderParty_List
		{
			get { return orgSurrenderParty_List ?? (orgSurrenderParty_List = new SurrenderPartyCollection(Factory)); }
		}
		SurrenderPartyCollection orgSurrenderParty_List;

		#endregion

		#region ToOrder Lists

		public OrganisationsFindBoxCollection ToOrder_List
		{
			get { return toOrder_List ?? (toOrder_List = new ToOrderCollection(Factory)); }
		}

		ToOrderCollection toOrder_List;

		#endregion

		[ThreadSafe]
		static readonly CodeDescriptionPairList rateModesList = new CodeDescriptionPairList(OLookUpEditType.RateModes);

		public CodeDescriptionPairList JS_ElectronicBillOfLadingTerms_List => Factory.GetCachedValue("FreightCodePairLists.BillOfLadingBillTermsList", () => FreightCodePairLists.BillOfLadingBillTermsList());

		public CodeDescriptionPairList JS_ElectronicBillOfLadingType_List => Factory.GetCachedValue("FreightCodePairLists.BillOfLadingBillTypeList", () => FreightCodePairLists.BillOfLadingBillTypeList());
	}
}
