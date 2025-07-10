using System.Collections.Generic;
using System.Data;
using System.Globalization;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.Shared;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Tracking.Business
{
	public class LinerAndAgencyContainer : AgencyShipmentContainer,
		IWebDocumentsWithUploadSupport,
		IBizOChangesEmailNotification,
		IWebUserEditableNoteSupport,
		IUpdatableMilestoneEventsProvider,
		IMilestonesProvider,
		ITrackingEventsProvider,
		IEventReferenceProvider
	{
		#region Schema

		public new abstract class Schema : AgencyShipmentContainer.Schema
		{
			public const string ShipmentNumbers = "ShipmentNumbers";
			public const string ContainerNumber = "ContainerNumber";
			public const string TypeDescription = "TypeDescription";
			public const string Mode = "Mode";
			public const string Packs = "Packs";
			public const string EmptyReturnRequired = "EmptyReturnRequired";
			public const string SlotDate = "SlotDate";
			public const string RequiredDelivery = "RequiredDelivery";
			public const string RequiredDeliveryStatus = "RequiredDeliveryStatus";
			public const string ConfirmedDelivery = "ConfirmedDelivery";
			public const string ConfirmedDeliveryStatus = "ConfirmedDeliveryStatus";
			public const string ActualDelivery = "ActualDelivery";
			public const string ActualDeliveryStatus = "ActualDeliveryStatus";
			public const string ConsigneesExtended = "ConsigneesExtended";
			public const string ConsignorsExtended = "ConsignorsExtended";
			public const string EmptyReady = "EmptyReady";
			public const string ActualDehire = "ActualDehire";
			public const string ActualDehireStatus = "ActualDehireStatus";
			public const string DynamicNumber = "DynamicNumber";
			public const string Weight = "Weight";
			public const string WeightUQ = "WeightUQ";
			public const string TareWeight = "TareWeight";
			public const string TareWeightUQ = "TareWeightUQ";
			public const string VesselName = "VesselName";
			public const string Voyage = "Voyage";
			public const string PortOfDischarge = "PortOfDischarge";
			public const string PortOfLoading = "PortOfLoading";
			public const string ContainerStatus = "ContainerStatus";
			public const string WeightWithUnits = "WeightWithUnits";
			public const string GoodsDescription = "GoodsDescription";
			public const string ServiceLevel = "ServiceLevel";
			public const string PaymentTerm = "PaymentTerm";
			public const string ShippersRef = "ShippersRef";
			public const string OrderRefs = "OrderRefs";
			public const string PortOfDestination = "PortOfDestination";
			public const string PortOfOrigin = "PortOfOrigin";
			public const string SetPointTempUQ = "SetPointTempUQ";
			public const string AirVentFlowUQ = "AirVentFlowUQ";
		}

		#endregion

		public LinerAndAgencyContainer(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			if (SiteUser != null)
			{
				Logs.AutoCreatedLogDefaultSL_Reference = SiteUser.ContactAndCompanyReference;
			}
		}

		public static LinerAndAgencyContainer FromPKFilteredBySiteUser(BusinessObjectFactory factory, ZGuid containerPK, TrackingSiteUser siteUser)
		{
			LinerAndAgencyContainer container = null;

			if (siteUser != null && siteUser.LoggedInOrganisation != null)
			{
				var filter = new ZDBOnlyQuery(typeof(LinerAndAgencyContainer));
				if (!siteUser.IsShipmentQuickViewUser) // We use it to enable direct view of Container Details by Container Number
				{
					filter.AddToFilter(OrgRestrictionFilterFactory.Instance.GetFilter<LinerAndAgencyContainer>());
				}
				filter.AddToFilter(JobContainerSchema.PK, containerPK);
				filter.IgnoreActiveFilter = true;
				container = factory.LoadTop1<LinerAndAgencyContainer>(filter);

				if (container != null)
				{
					container.SiteUser = siteUser;
				}
			}

			return container;
		}

		#region Properties

		#region Container Number

		public ZString ContainerNumber
		{
			get { return JC_ContainerNum; }
		}

		public ZPropertyInfo ContainerNumberInfo
		{
			get { return GetReadOnlyZPropertyInfo(Schema.ContainerNumber); }
		}

		#endregion

		#region Shipments

		public TrackingShipmentCollection Shipments
		{
			get
			{
				if (shipments == null)
				{
					shipments = new TrackingShipmentCollection(Factory);
					var shipmentPKs = GetShipmentPKsFromPackLines();

					if (shipmentPKs.Count > 0 && !CurrentOrg.IsEmpty)
					{
						var filter = new ZQuery(JobShipmentSchema.PK, shipmentPKs);
						filter.AddToFilter(OrgRestrictionFilterFactory.Instance.GetFilter<TrackingShipment>());
						shipments.Load(filter);
					}
				}

				return shipments;
			}
		}
		TrackingShipmentCollection shipments;

#if DEBUG
		virtual
#endif
 protected List<ZGuid> GetShipmentPKsFromPackLines()
		{
			var shipmentPKs = new List<ZGuid>();

			foreach (PackLine pack in PackLines)
			{
				if (pack.Shipment != null)
				{
					shipmentPKs.Add(pack.Shipment.PK);
				}
			}

			return shipmentPKs;
		}

		#endregion

		#region Type

		public ZString TypeDescription
		{
			get { return Container != null ? Container.RC_DescriptionMultilingual : ZString.Empty; }
		}

		public ZPropertyInfo TypeDescriptionInfo
		{
			get { return GetReadOnlyZPropertyInfo(Schema.TypeDescription); }
		}

		#endregion

		#region Mode

		public ZString Mode
		{
			get { return ContainerModeForBinding; }
		}

		public ZString ModeDescription
		{
			get { return Container != null ? Container.RC_DescriptionMultilingual : ZString.Empty; }
		}

		public ZPropertyInfo ModeInfo
		{
			get { return GetReadOnlyZPropertyInfo(Schema.Mode); }
		}

		#endregion

		#region Packs

		public ZInt Packs
		{
			get { return JC_Calc_TotalPackages; }
		}

		public ZPropertyInfo PacksInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.Packs, x => JC_Calc_TotalPackagesInfo); }
		}

		#endregion

		#region Port Of Discharge

		public ZString PortOfDischarge
		{
			get
			{
				var result = ZString.Empty;
				if (Booking != null && Booking.CalcDischargePort != null)
				{
					result = Booking.CalcDischargePort.Description;
				}
				return result;
			}
		}

		public ZPropertyInfo PortOfDischargeInfo
		{
			get { return GetZPropertyInfo(Schema.PortOfDischarge); }
		}

		#endregion

		#region Port Of Discharge

		public ZString PortOfDestination
		{
			get { return (ZString)(Booking?.JS_RL_NKDestination); }
		}

		public ZPropertyInfo PortOfDestinationInfo
		{
			get { return GetZPropertyInfo(Schema.PortOfDestination); }
		}

		#endregion

		#region Port Of Origin

		public ZString PortOfOrigin
		{
			get { return (ZString)(Booking?.JS_RL_NKOrigin); }
		}

		public ZPropertyInfo PortOfOriginInfo
		{
			get { return GetZPropertyInfo(Schema.PortOfOrigin); }
		}

		#endregion

		#region Goods Description

		public ZString GoodsDescription
		{
			get { return (ZString)(Booking?.JS_GoodsDescription); }
		}

		public ZPropertyInfo GoodsDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.GoodsDescription); }
		}

		#endregion

		#region Service Level

		public ZString ServiceLevel
		{
			get { return (ZString)(Booking?.JS_RS_NKServiceLevel); }
		}

		public ZPropertyInfo ServiceLevelInfo
		{
			get { return GetZPropertyInfo(Schema.ServiceLevel); }
		}

		#endregion

		#region Payment Term

		public ZString PaymentTerm
		{
			get { return (ZString)(Booking?.JS_INCO); }
		}

		public ZPropertyInfo PaymentTermInfo
		{
			get { return GetZPropertyInfo(Schema.PaymentTerm); }
		}

		#endregion

		#region Shippers Ref

		public ZString ShippersRef
		{
			get { return (ZString)(Booking?.JS_BookingReference); }
		}

		public ZPropertyInfo ShippersRefInfo
		{
			get { return GetZPropertyInfo(Schema.ShippersRef); }
		}

		#endregion

		#region Shippers Ref

		public ZString OrderRefs
		{
			get
			{
				var result = ZString.Empty;
				if (Booking != null && Booking.DocsAndCartage != null)
				{
					result = Booking.DocsAndCartage.JP_OrderItemsAsString;
				}
				return result;
			}
		}

		public ZPropertyInfo OrderRefsInfo
		{
			get { return GetZPropertyInfo(Schema.OrderRefs); }
		}

		#endregion

		#region Slot Date

		/// <summary>
		/// Date & Time slot container is scheduled for uplift from the wharf
		/// </summary>
		public ZDateTime SlotDate
		{
			get { return JC_ArrivalSlotDateTime; }
		}

		public ZPropertyInfo SlotDateInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.SlotDate, x => JC_ArrivalSlotDateTimeInfo); }
		}

		#endregion

		#region Required Delivery

		/// <summary>
		/// Required Delivery Date - date container required 
		/// to be delivered into appropriate McPherson's warehouse.
		/// This field is modifiable to allow the warehouses to schedule deliveries the carriers.
		/// </summary>
		public ZDateTime RequiredDelivery
		{
			get { return JC_ArrivalEstimatedDelivery; }
			set { JC_ArrivalEstimatedDelivery = value; } // Updated by XML Import
		}

		public ZPropertyInfo RequiredDeliveryInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.RequiredDelivery, x => JC_ArrivalEstimatedDeliveryInfo); }
		}

		public ZString RequiredDeliveryStatus
		{
			get
			{
				var result = ZString.Empty;
				if (!JC_LastFreeDay.IsEmpty)
				{
					if (JC_LastFreeDay <= ZDateTime.Now && RequiredDelivery.IsEmpty)
					{
						result = Constants.DateTimeStatus.Overdue;
					}
					else if (JC_LastFreeDay < RequiredDelivery)
					{
						result = Constants.DateTimeStatus.Late;
					}
					else if (JC_LastFreeDay >= RequiredDelivery)
					{
						result = Constants.DateTimeStatus.OnTime;
					}
				}
				return result;
			}
		}

		public ZPropertyInfo RequiredDeliveryStatusInfo
		{
			get { return GetZPropertyInfo(Schema.RequiredDeliveryStatus); }
		}

		#endregion

		#region Confirmed Delivery

		/// <summary>
		/// Confirmed delivery date - date the carrier has confirmed 
		/// they will deliver the container into appropriate McPherson's warehouse.
		/// This field is updateable to allow carriers to confirm delivery times.
		/// </summary>
		public ZDateTime ConfirmedDelivery
		{
			get { return JC_ArrivalCartageAdvised; }
			set { JC_ArrivalCartageAdvised = value; }
		}

		public ZPropertyInfo ConfirmedDeliveryInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ConfirmedDelivery, x => JC_ArrivalCartageAdvisedInfo); }
		}

		public ZString ConfirmedDeliveryStatus
		{
			get
			{
				var result = ZString.Empty;
				if (!RequiredDelivery.IsEmpty)
				{
					if (ConfirmedDelivery.IsEmpty)
					{
						result = Constants.DateTimeStatus.Overdue;
					}
					else if (RequiredDelivery.Date != ConfirmedDelivery.Date)
					{
						result = Constants.DateTimeStatus.Late;
					}
					else if (RequiredDelivery.Date == ConfirmedDelivery.Date)
					{
						result = Constants.DateTimeStatus.OnTime;
					}
				}
				return result;
			}
		}

		public ZPropertyInfo ConfirmedDeliveryStatusInfo
		{
			get { return GetZPropertyInfo(Schema.ConfirmedDeliveryStatus); }
		}

		#endregion

		#region Actual Delivery

		/// <summary>
		/// Actual delivery date - date the container actually arrived 
		/// into McPherson's warehouse.
		/// This field is updateable by McPherson's to confirm delivery times.
		/// </summary>
		public ZDateTime ActualDelivery
		{
			get { return JC_ArrivalCartageComplete; }
			set { JC_ArrivalCartageComplete = value; }
		}

		public ZPropertyInfo ActualDeliveryInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ActualDelivery, x => JC_ArrivalCartageCompleteInfo); }
		}

		public ZString ActualDeliveryStatus
		{
			get
			{
				var result = ZString.Empty;
				if (!ConfirmedDelivery.IsEmpty)
				{
					if (ActualDelivery.IsEmpty)
					{
						result = Constants.DateTimeStatus.Overdue;
					}
					else if (ActualDelivery > ConfirmedDelivery)
					{
						result = Constants.DateTimeStatus.Late;
					}
					else if (ActualDelivery <= ConfirmedDelivery)
					{
						result = Constants.DateTimeStatus.OnTime;
					}
				}
				return result;
			}
		}

		public ZPropertyInfo ActualDeliveryStatusInfo
		{
			get { return GetZPropertyInfo(Schema.ActualDeliveryStatus); }
		}

		#endregion

		public ZString ContainerStatus
		{
			get { return Lookups.ContainerStatuses.GetDescriptionFromCode(JC_ContainerStatus); }
		}

		public ZPropertyInfo ContainerStatusInfo
		{
			get { return GetReadOnlyZPropertyInfo(Schema.ContainerStatus); }
		}

		#region ShipmentNumbers

		/// <summary>
		/// Shipment Numbers
		/// </summary>
		public ZString ShipmentNumbers
		{
			get
			{
				if (!shipmentNumbersHasBeenCalculated)
				{
					var listofNumbers = new List<ZString>();

					foreach (TrackingShipment shipment in Shipments)
					{
						listofNumbers.Add(shipment.JS_UniqueConsignRef);
					}
					shipmentNumbers = ZString.Join(", ", listofNumbers.ToArray());
					shipmentNumbersHasBeenCalculated = true;
				}

				return shipmentNumbers;
			}
		}

		ZString shipmentNumbers;

		bool shipmentNumbersHasBeenCalculated;

		public ZPropertyInfo ShipmentNumbersInfo
		{
			get { return GetReadOnlyZPropertyInfo(Schema.ShipmentNumbers); }
		}

		#endregion

		#region Consignees Extended

		public ZString ConsigneesExtended
		{
			get { return FormatAddress(DestinationConfirm.ConfirmAddress); }
		}

		public ZPropertyInfo ConsigneesExtendedInfo
		{
			get { return GetReadOnlyZPropertyInfo(Schema.ConsigneesExtended); }
		}

		#endregion

		#region Consignors Extended

		public ZString ConsignorsExtended
		{
			get { return FormatAddress(OriginConfirm.ConfirmAddress); }
		}

		ZString FormatAddress(JobDocAddress address)
		{
			return address.E2_CompanyNameTruncated + System.Environment.NewLine +
			address.AddressSummary + System.Environment.NewLine;
		}

		public ZPropertyInfo ConsignorsExtendedInfo
		{
			get { return GetReadOnlyZPropertyInfo(Schema.ConsignorsExtended); }
		}

		#endregion

		#region Detention

		/// <summary>
		/// Date and time container goes on detention
		/// </summary>
		public ZDateTime EmptyReturnRequired
		{
			get { return JC_EmptyReturnedBy; }
		}

		public ZPropertyInfo EmptyReturnRequiredInfo
		{
			get { return GetReadOnlyZPropertyInfo(Schema.EmptyReturnRequired); }
		}

		#endregion

		#region Empty Ready

		/// <summary>
		/// This field is modifiable to allow the warehouses to notify carriers to pickup empties.
		/// </summary>
		public ZDateTime EmptyReady
		{
			get { return JC_EmptyReadyForReturn; }
			set { JC_EmptyReadyForReturn = value; }
		}

		public ZPropertyInfo EmptyReadyInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.EmptyReady, x => JC_EmptyReadyForReturnInfo); }
		}

		#endregion

		#region Actual Dehire

		/// <summary>
		/// Actual date and time container was dehired with the shipping line
		/// </summary>
		public ZDateTime ActualDehire
		{
			get { return JC_ContainerYardEmptyReturnGateIn; }
			set { JC_ContainerYardEmptyReturnGateIn = value; }
		}

		public ZPropertyInfo ActualDehireInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ActualDehire, x => JC_ContainerYardEmptyReturnGateInInfo); }
		}

		public ZString ActualDehireStatus
		{
			get
			{
				ZString result = ZString.Empty;
				if (!EmptyReturnRequired.IsEmpty)
				{
					if (EmptyReturnRequired <= ZDateTime.Now && JC_FCLWharfGateIn.IsEmpty && ActualDehire.IsEmpty)
					{
						result = Constants.DateTimeStatus.Overdue;
					}
					else if (EmptyReturnRequired < JC_FCLWharfGateIn || EmptyReturnRequired < ActualDehire)
					{
						result = Constants.DateTimeStatus.Late;
					}
					else if (EmptyReturnRequired >= JC_FCLWharfGateIn || EmptyReturnRequired >= ActualDehire)
					{
						result = Constants.DateTimeStatus.OnTime;
					}
				}
				return result;
			}
		}

		public ZPropertyInfo ActualDehireStatusInfo
		{
			get { return GetZPropertyInfo(Schema.ActualDehireStatus); }
		}

		#endregion

		#region Dynamic Number

		public ZString DynamicNumber
		{
			get
			{
				if (Booking != null)
				{
					if (JC_Purpose == "REL")
					{
						return Booking.JS_HouseBill;
					}
					else if (JC_Purpose == "BKD")
					{
						return Booking.JS_CFSReference;
					}
					else
					{
						return ZString.Empty;
					}
				}
				else
				{
					return ZString.Empty;
				}
			}
		}

		public ZPropertyInfo DynamicNumberInfo
		{
			get { return GetReadOnlyZPropertyInfo(Schema.DynamicNumber); }
		}

		#endregion

		#region Weight

		/// <summary>
		/// Total Weight of Container
		/// </summary>
		public ZDecimal Weight
		{
			get { return JC_GrossWeight; }
		}

		public ZPropertyInfo WeightInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.Weight, x => JC_GrossWeightInfo); }
		}

		public ZString WeightUQ
		{
			get { return JC_GrossWeightUQ; }
		}

		public ZPropertyInfo WeightUQInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.WeightUQ, x => JC_GrossWeightUQInfo); }
		}

		#endregion

		public ZString SetPointTempUQ
		{
			get { return JC_SetPointTemp + JC_SetPointTempUnit; }
		}

		public ZPropertyInfo SetPointTempUQInfo
		{
			get { return GetReadOnlyZPropertyInfo(Schema.SetPointTempUQ); }
		}

		public ZString AirVentFlowUQ
		{
			get { return JC_AirVentFlow + JC_AirVentFlowRateUnit; }
		}

		public ZPropertyInfo AirVentFlowUQInfo
		{
			get { return GetReadOnlyZPropertyInfo(Schema.AirVentFlowUQ); }
		}

		#region WeightWithUnits
		public ZString WeightWithUnits
		{
			get
			{
				var roundedDecimal = this.GetRoundedValue(WeightInfo, Weight);
				var formattedDecimal = FormatNumber(roundedDecimal, DefaultNumberOfDecimalsSupporterHelperForFreight.GetDefaultNumberOfDecimalsMetaDataProperty(this, WeightInfo.PropertyDescriptor));
				return string.Format(CultureInfo.InvariantCulture, "{0} {1}", formattedDecimal, WeightUQ);
			}
		}

		public ZPropertyInfo WeightWithUnitsInfo
		{
			get { return GetZPropertyInfo(Schema.WeightWithUnits); }
		}

		protected string FormatNumber(ZDecimal number, int decimalsToShow)
		{
			return Utilities.FormatNumber(number, decimalsToShow, WebEnvShared.ClientCulture);
		}

		#endregion

		#region Tare Weight

		/// <summary>
		/// Total Weight of Container
		/// </summary>
		public ZDecimal TareWeight
		{
			get { return JC_TareWeight; }
		}

		public ZPropertyInfo TareWeightInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.TareWeight, x => JC_TareWeightInfo); }
		}

		#endregion

		#region TareWeightUQ

		public ZString TareWeightUQ
		{
			get
			{
				var roundedDecimal = this.GetRoundedValue(TareWeightInfo, TareWeight);
				var formattedDecimal = FormatNumber(roundedDecimal, DefaultNumberOfDecimalsSupporterHelperForFreight.GetDefaultNumberOfDecimalsMetaDataProperty(this, TareWeightInfo.PropertyDescriptor));
				return string.Format(CultureInfo.InvariantCulture, "{0} {1}", formattedDecimal, WeightUQ);
			}
		}

		public ZPropertyInfo TareWeightUQInfo
		{
			get { return GetZPropertyInfo(Schema.TareWeightUQ); }
		}

		#endregion

		#region Port Of Loading

		public ZString PortOfLoading
		{
			get
			{
				var result = ZString.Empty;
				if (Booking != null && Booking.CalcLoadPort != null)
				{
					result = Booking.CalcLoadPort.Description;
				}
				return result;
			}
		}

		public ZPropertyInfo PortOfLoadingInfo
		{
			get { return GetZPropertyInfo(Schema.PortOfLoading); }
		}

		#endregion

		#region Vessel Name

		/// <summary>
		/// Vessel goods are being shipped on
		/// </summary>
		public ZString VesselName
		{
			get
			{
				var result = ZString.Empty;
				if (Booking != null && Booking.Sailing != null)
				{
					result = Booking.Sailing.JX_JV_NKVessel;
				}

				return result;
			}
		}

		public ZPropertyInfo VesselNameInfo
		{
			get { return GetReadOnlyZPropertyInfo(Schema.VesselName); }
		}

		#endregion

		#region Voyage

		/// <summary>
		/// Voyage goods are being shipped on
		/// </summary>
		public ZString Voyage
		{
			get
			{
				var result = ZString.Empty;
				if (Booking != null && Booking.Sailing != null)
				{
					result = Booking.Sailing.JX_JV_VoyageFlight;
				}
				return result;
			}
		}

		public ZPropertyInfo VoyageInfo
		{
			get { return GetReadOnlyZPropertyInfo(Schema.Voyage); }
		}

		#endregion
		#endregion

		#region UserEditableNote

		public IStmNoteParent NotesParentBO
		{
			get { return this; }
		}

		public WebUserEditableNote UserEditableNoteHelper
		{
			get
			{
				if (userEditableNoteHelper == null)
				{
					userEditableNoteHelper = GetNewUserEditableNoteHelper();
				}
				return userEditableNoteHelper;
			}
		}
		WebUserEditableNote userEditableNoteHelper;

		protected WebUserEditableNote GetNewUserEditableNoteHelper()
		{
			return new WebUserEditableNote(this, PredefinedNoteTypes.Instance.SpecialInstructions);
		}

		#endregion

		#region Implementation

		ZPropertyInfo GetReadOnlyZPropertyInfo(string propertyname)
		{
			ZPropertyInfo info = GetZPropertyInfo(propertyname);
			((IZPropertyInfoObsolete)info).ReadOnly = true;
			return info;
		}

		public TrackingSiteUser SiteUser
		{
			get
			{
				if (siteUser == null)
				{
					if (WebEnv.AppInstance != null && WebEnv.AppInstance.SiteUser != null)
					{
						siteUser = (WebEnv.AppInstance.SiteUser as TrackingSiteUser);
					}
				}
				return siteUser;
			}
			set
			{
				siteUser = value;

				if (SiteUser != null &&
				!(Logs.AutoCreatedLogDefaultSL_Reference == SiteUser.ContactAndCompanyReference))
				{
					Logs.AutoCreatedLogDefaultSL_Reference = SiteUser.ContactAndCompanyReference;
				}
			}
		}
		TrackingSiteUser siteUser;

		public ZGuid CurrentOrg
		{
			get { return SiteUser != null && SiteUser.IsLoggedIn ? SiteUser.LoggedInOrganisation.PK : ZGuid.Empty; }
		}

		#endregion

		#region IWebDocumentsSupport Members

		public ZGuid DocParentPK
		{
			get { return PK; }
		}

		public List<ZGuid> DocRelatedPKs
		{
			get { return new List<ZGuid>(); }
		}

		public OrgContact LoggedInContact
		{
			get { return SiteUser != null ? SiteUser.LoggedInUser : null; }
		}

		public DocumentSupport DocumentHelper
		{
			get
			{
				return documentHelper ?? (documentHelper = new DocumentSupport(this));
			}
		}
		DocumentSupport documentHelper;

		#endregion

		#region IWebDocumentsWithUploadSupport

		public new DocManagerInfo DocManagerInfo
		{
			get { return ((IDocManagerSupport)this).DocManagerInfo; }
		}

		public DocumentUploadSupport DocumentUploadHelper
		{
			get
			{
				if (documentUploadHelper == null)
				{
					documentUploadHelper = new DocumentUploadSupport(Factory);
				}
				return documentUploadHelper;
			}
		}

		DocumentUploadSupport documentUploadHelper;

		public void ResetDocumentHelper()
		{
		}

		#endregion

		#region TrackingEvents

		public StmALogCollection TrackingEvents
		{
			get { return this.GetTrackingEvents(SiteUser); }
		}

		public bool CanViewTrackingEvents
		{
			get { return SiteUser?.CanViewEvents ?? false; }
		}

		#endregion

		#region Milestones

		public void ReloadMilestones()
		{
			milestones = null;
		}

		public TrackingMilestoneCollection Milestones
		{
			get { return milestones ?? (milestones = new TrackingMilestoneCollection(this)); }
		}
		TrackingMilestoneCollection milestones;

		public TrackingMilestoneCollection EditableMilestones
		{
			get { return editableMilestones ?? (editableMilestones = new TrackingMilestoneCollection(this, true)); }
		}
		TrackingMilestoneCollection editableMilestones;

		#endregion

		#region IUpdatableMilestoneEventsProvider

		public List<string> UpdatableMilestoneEventCodes
		{
			get
			{
				return (new UpdateableMilestoneEventsHelper(SiteUser)).GetUpdateableMilestoneEvents(WebDataRegistry.Instance.LinerAndAgencyContainerMilestoneEventUpdates.Value, WebParties);
			}
		}

		#endregion

		#region IEventReferenceProvider

		public string EventReference
		{
			get
			{
				if (WebEnv.AppInstance != null && WebEnv.AppInstance.SiteUser != null)
				{
					return (new EventReferenceHelper(WebEnv.AppInstance.SiteUser as TrackingSiteUser)).GetEventReferences(WebParties);
				}
				return string.Empty;
			}
		}

		#endregion

		#region WebParties

		WebPartyTypeOrgPairCollection WebParties
		{
			get
			{
				if (webParties == null)
				{
					webParties = new WebPartyTypeOrgPairCollection();

					if (Booking != null)
					{
						webParties.Add(WebPartyType.DeliveryAgent, Booking.DeliveryAgent);
						webParties.Add(WebPartyType.Shipper, Booking.Consignor);
						webParties.Add(WebPartyType.Consignee, Booking.Consignee);

						if (Booking.Job != null)
						{
							webParties.Add(WebPartyType.LocalClient, Booking.Job.LocalCharges);
						}
					}
				}

				return webParties;
			}
		}
		WebPartyTypeOrgPairCollection webParties;

		#endregion

		#region IEmailNotification Members

		ZString IBizOChangesEmailNotification.Number
		{
			get { return ContainerNumber; }
		}

		ZBool IBizOChangesEmailNotification.IsCancelled
		{
			get { return false; }
		}

		GlbBranch IBizOChangesEmailNotification.EventBranch
		{
			get { return GlbBranch.FindControllingBranchWithFallBackToAnyCompany(((IBizOChangesEmailNotification)this).RelatedOrg); }
		}

		GuidRegistryItem IBizOChangesEmailNotification.EmailGroupRegistryItem
		{
			get { return WebDataRegistry.Instance.LinerAndAgencyContainerNotificationEmailGroup; }
		}

		ControllerID IBizOChangesEmailNotification.ControllerForEnterpriseUrl
		{
			get { return ControllerIDs.Containers; }
		}

		OrgHeader IBizOChangesEmailNotification.RelatedOrg
		{
			get { return Booking != null ? Booking.Consignee : null; }
		}

		ZGuid IBizOChangesEmailNotification.GetStaffGuid(OrgStaffAssignmentsCollection staffAssignments, CodeDescriptionBool role)
		{
			var staffNK = staffAssignments.GetStaffAssignment(role.Code, OrgStaffAssignmentsLookups.ContainerYardServices);
			var staff = staffAssignments.Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, staffNK);
			if (staff != null)
			{
				return staff.PK;
			}
			else
			{
				return ZGuid.Empty;
			}
		}

		CodeDescriptionBoolRegistryItem IBizOChangesEmailNotification.StaffRolesToNotify
		{
			get { return WebDataRegistry.Instance.LinerAndAgencyContainerNotificationStaffRoles; }
		}

		CodePairRegistryItem IBizOChangesEmailNotification.NotificationSendingRule
		{
			get { return WebDataRegistry.Instance.LinerAndAgencyContainerNotificationOptions; }
		}

		#region Email Reporting

		void IBizOChangesEmailNotification.AddPropertiesForEmailReporting(DataState state)
		{
			PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("E03FD96A-33A2-4DAD-BE04-327A9E3D8467", "Estimated Full Delivery"), RequiredDelivery);
			PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("f6aa1288-214f-454e-906c-68d34badb8c1", "Actual Delivery"), ActualDelivery);
			PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("B11E943C-735C-44F3-99A2-4EBCDBEDFF8A", "Empty Ready for Return"), EmptyReady);
			PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("1449E74C-B6C9-4EAD-933E-ACF841048312", "Empty Return Req. By"), EmptyReturnRequired);
			PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("29E14A20-7BA3-464C-ADE0-D67F6602B70B", "Empty Returned On"), ActualDehire);
			DocumentHelper.AddDocumentsForEmailReporting(state, PropertiesForEmailReporting);
			Milestones.AddForEmailReporting(state, PropertiesForEmailReporting);
		}

		PropertyChangeInfo[] IBizOChangesEmailNotification.GetPropertiesForEmailReporting()
		{
			return PropertiesForEmailReporting.GetValuesAsArray();
		}

		PropertyChangeInfoCollection PropertiesForEmailReporting
		{
			get
			{
				if (propertiesForEmailReporting == null)
				{
					propertiesForEmailReporting = new PropertyChangeInfoCollection();
				}

				return propertiesForEmailReporting;
			}
		}
		PropertyChangeInfoCollection propertiesForEmailReporting;

		#endregion

		#endregion
	}
}
