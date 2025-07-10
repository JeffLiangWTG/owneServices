//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoHVLVConsignmentLookups
//
//    This class should be used for overriding collections in AutoHVLVConsignmentLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.ComponentModel;
using CargoWise.Definitions.Ecommerce;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.eTail.Business
{
	public class HVLVConsignmentLookups : AutoHVLVConsignmentLookups
	{
		public HVLVConsignmentLookups(AutoHVLVConsignment parent)
			: base(parent)
		{ }

		public BindToLists BindingLists
		{
			get { return BindToLists.GetCachedLists(Factory); }
		}

		public CodeDescriptionPairList HVC_ReleaseStatus_List
		{
			get
			{
				return Factory.GetCachedValue(
					"HVLVConsignmentLookups.HVC_ReleaseStatus_List",
					() => HVLVReleaseStatus.GetAll());
			}
		}

		public static CodeDescriptionPairList GetHVC_ImportCustomsStatus_List(BusinessObjectFactory factory)
		{
			var cacheKey = "HVLVConsignmentLookups.HVC_ImportCustomsStatus_List" + GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			return factory.GetCachedValue(cacheKey,
				() => HVLVCustomsStatusService.GetService(factory).GetAllRefCusCodeList(true));
		}

		public static CodeDescriptionPairList GetHVC_ExportCustomsStatus_List(BusinessObjectFactory factory)
		{
			var cacheKey = "HVLVConsignmentLookups.HVC_ExportCustomsStatus_List" + GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			return factory.GetCachedValue(cacheKey,
				() => HVLVCustomsStatusService.GetService(factory).GetAllRefCusCodeList(false));
		}

		public BusinessObjectCollection HVC_JS_ManifestedOnShipment_List
		{
			get
			{
				var filter = new ZQuery();
				filter.AddToFilter(JobShipmentSchema.JS_ShipmentType, ShipmentTypes.HighVolumeLowValue);
				filter.AddToFilter(JobShipmentSchema.JS_IsBooking, ZBool.False);
				return new ShipmentCollection(Factory, filter);
			}
		}

		public CodeDescriptionPairList HVC_VolumeUQ_List
		{
			get { return Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Volume); }
		}

		public CodeDescriptionPairList HVC_WeightUQ_List
		{
			get { return Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Weight); }
		}

		public override OrgCarrierServiceLevelCollection LastMileCarrierServiceLevels
		{
			get
			{
				var carrierServiceLevels = (Parent.LastMileCarrier != null && Parent.LastMileCarrier.OH_IsShippingProvider) ?
					new OrgCarrierServiceLevelCollection(Parent.LastMileCarrier.MiscServ) :
					new OrgCarrierServiceLevelCollection(Factory, false, false);

				carrierServiceLevels.Load();

				return carrierServiceLevels;
			}
		}

		public OrgCarrierAccountCollection HVC_CarrierAccountNumber_List
		{
			get
			{
				return (Parent.LastMileCarrier != null && Parent.LastMileCarrier.OH_IsShippingProvider) ?
					Parent.LastMileCarrier.CarrierAccounts :
					null;
			}
		}

		public CodeDescriptionPairList HVC_Status_List
		{
			get { return Factory.GetCachedValue("HVLVConsignmentLookups.HVC_Status_List", () => GetAllHVLVConsignmentStatuses()); }
		}

		public CodeDescriptionPairList HVC_UndgClass_List
		{
			get { return UNDGDataItemLookups.GetDGClassList(Factory); }
		}

		public UnpackDepotCollection DestinationDepotOrgCollection => new UnpackDepotCollection(Factory);

		public override OrgHeaderCollection LastMileCarriers => new CarrierCollection(Factory);

		public override OrgHeaderCollection LastMileCarrierBookingAgents => new ControllingAgentCollection(Factory);

		public BaseJobDeclarationCollection DeclarationCollection => new BaseJobDeclarationCollection(Factory);

		public CodeDescriptionPairList ConsigneeStateList
		{
			get
			{
				return Factory.GetStateList(Parent.HVC_RN_NKConsigneeCountryCode, Parent.HVC_RN_NKConsigneeCountryCodeInfo.HasErrors());
			}
		}

		public CodeDescriptionPairList ConsignorStateList
		{
			get
			{
				return Factory.GetStateList(Parent.HVC_RN_NKShipperCountryCode, Parent.HVC_RN_NKShipperCountryCodeInfo.HasErrors());
			}
		}

		public CodeDescriptionPairList ReturnAddrStateList
		{
			get
			{
				return Factory.GetStateList(Parent.HVC_RN_NKReturnCountryCode, Parent.HVC_RN_NKReturnCountryCodeInfo.HasErrors());
			}
		}

		public CodeDescriptionPairList INCOTermsList
		{
			get { return Factory.GetCachedValue("HVLVConsignmentLookups.INCOTermsList", () => new IncoTermsCodeDescriptionPairList(IncoTermsListType.ActiveIncoTerms)); }
		}

		public static CodeDescriptionPairList GetAllHVLVConsignmentStatuses()
		{
			var result = new CodeDescriptionPairList
			{
				new CodeDescriptionPair(HVLVConsignmentStatus.Codes.Booked, ResString.GetMultilingualString("721454d1-42f2-4b41-ab78-0266af06e534", HVLVConsignmentStatus.Descriptions.Booked)),
				new CodeDescriptionPair(HVLVConsignmentStatus.Codes.Confirmed, ResString.GetMultilingualString("9e83f6ac-3840-4924-8004-a0cfe27945b4", HVLVConsignmentStatus.Descriptions.Confirmed)),
				new CodeDescriptionPair(HVLVConsignmentStatus.Codes.DepartedFromOriginDepot, ResString.GetMultilingualString("2d031756-6efd-4e65-9ef5-63b79185c4cb", HVLVConsignmentStatus.Descriptions.DepartedFromOriginDepot)),
				new CodeDescriptionPair(HVLVConsignmentStatus.Codes.ArrivedAtDestination, ResString.GetMultilingualString("4a0f87a5-3642-4b84-a8a7-0b1b54b60101", HVLVConsignmentStatus.Descriptions.ArrivedAtDestination)),
				new CodeDescriptionPair(HVLVConsignmentStatus.Codes.CustomsClearedAtDestination, ResString.GetMultilingualString("8b32c8e7-cae3-40a5-9902-27f53746bf9f", HVLVConsignmentStatus.Descriptions.CustomsClearedAtDestination)),
				new CodeDescriptionPair(HVLVConsignmentStatus.Codes.CustomsHeldAtDestination, ResString.GetMultilingualString("d505ab21-4246-44d6-968c-ee7b3568fc3a", HVLVConsignmentStatus.Descriptions.CustomsHeldAtDestination)),
				new CodeDescriptionPair(HVLVConsignmentStatus.Codes.Delivered, ResString.GetMultilingualString("5297662f-0ac0-4571-aae6-9aee9cdecf59", HVLVConsignmentStatus.Descriptions.Delivered)),
				new CodeDescriptionPair(HVLVConsignmentStatus.Codes.Detached, ResString.GetMultilingualString("a003d22d-21b9-4579-8294-7e4eefe56640", HVLVConsignmentStatus.Descriptions.Detached))
			};
			return result;
		}

		public CodeDescriptionPairList HVC_PreScreeningStatus_List
		{
			get { return Factory.GetCachedValue("HVLVConsignmentLookups.PreScreeningStatusList", () => new HVLVConsignmentPreScreeningStatusCodes()); }
		}

		public CodeDescriptionPairList HVC_DeniedPartyScreeningStatus_List => Factory.GetCachedValue<ScreeningStatusesList>();

		public TransportBookings.Business.DtbBookingCollectionForFindBox HVC_KM_LastMileTransportBooking_List
		{
			get { return hvc_KM_LastMileTransportBooking_List ?? (hvc_KM_LastMileTransportBooking_List = new TransportBookings.Business.DtbBookingCollectionForFindBox(Parent.Factory)); }
		}

		TransportBookings.Business.DtbBookingCollectionForFindBox hvc_KM_LastMileTransportBooking_List;

		public OrganisationsFindBoxCollection ConsigneeOrganisation_List => new ConsigneeCollection(Factory);

		public OrgContactDependentCollection ConsigneeOrgContact_List => Parent.ConsigneeAddress?.Header?.Contacts ?? new OrgContactDependentCollection(Factory);

		public OrganisationsFindBoxCollection ShipperOrganisation_List => new ConsignorCollection(Factory);

		public OrgContactDependentCollection ShipperOrgContact_List => Parent.ShipperAddress?.Header?.Contacts ?? new OrgContactDependentCollection(Factory);

		public OrganisationsFindBoxCollection ReturnOrganisation_List => new StorageCTOCollection(Factory);

		public OrgContactDependentCollection ReturnContact_List => Parent.ReturnLocation?.Header?.Contacts ?? new OrgContactDependentCollection(Factory);

		public CodeDescriptionPairList ShipmentTransportMode_List => Factory.GetCachedValue("HVLVConsignmentLookups.ShipmentTransportModeList", () => FreightCodePairLists.JS_TransportModeList());

		public CodeDescriptionPairList ShipmentPackingMode_List => Factory.GetCachedValue("HVLVConsignmentLookups.ShipmentPackingModeList_" + Parent.ShipmentTransportMode, () => FreightCodePairLists.JS_PackingModeList(Parent.ShipmentTransportMode));

		public RefUNLOCOCollection ConsolDestination_List => new RefUNLOCOCollection(Factory);

		public RefUNLOCOCollection ConsolOrigin_List => new RefUNLOCOCollection(Factory);

		public CodeDescriptionPairList HVC_ACASStatus_List => GetHVC_ACASStatus_List(Factory);
		public static CodeDescriptionPairList GetHVC_ACASStatus_List(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("HVLVConsignmentLookups.ACASStatusList", () => new HVLVACASStatusList());
		}

		public CodeDescriptionPairList HVC_ACASMessageStatus_List => Factory.GetCachedValue("HVLVConsignmentLookups.HVLVACASMessageStatusList", () => new HVLVACASMessageStatusList());

		public CodeDescriptionPairList HVC_ACASInterchangeStatus_List => Factory.GetCachedValue("HVLVConsignmentLookups.HVLVACASInterchangeStatusList", () => new HVLVACASInterchangeStatusList());

		#region Implementation

		public new HVLVConsignment Parent
		{
			get { return (HVLVConsignment)base.Parent; }
		}

		#endregion
	}
}
