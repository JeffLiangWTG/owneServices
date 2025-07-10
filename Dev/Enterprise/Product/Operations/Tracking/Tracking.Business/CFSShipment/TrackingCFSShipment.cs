using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.CFS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.Shared;

namespace Enterprise.Tracking.Business
{
	/// <summary>
	/// Provides access to tracking related shipment details
	/// </summary>
	public partial class TrackingCFSShipment : CFSShipment,
		IWebDocumentsWithUploadSupport,
		ITransactionSupport,
		IWebUserVisibleNotesSupport,
		IEventReferenceProvider,
		ITrackingEventsProvider
	{
		#region Schema

		public abstract new class Schema : CFSShipment.Schema
		{
			public const string PickupAddressAsText = "PickupAddressAsText";
			public const string StorageDate = "StorageDate";

			public const string DeliveryAddressAsText = "DeliveryAddressAsText";
			public const string AvailableDate = "AvailableDate";

			public const string LoadETDWithSuppression = "LoadETDWithSuppression";
			public const string DischargeETAWithSuppression = "DischargeETAWithSuppression";
			public const string ETDWithSuppression = "ETDWithSuppression";
			public const string ETAWithSuppression = "ETAWithSuppression";

			public const string AvailableAtAddressAsText = "AvailableAtAddressAsText";
			public const string MasterShipmentNum = "MasterShipmentNum";
			public const string ShipmentType = "ShipmentType";
			public const string MasterBill = "MasterBill";

			public const string ConsignorName = "ConsignorName";
			public const string ConsignorFullAddress = "ConsignorFullAddress";
			public const string ConsignorAddress = "ConsignorAddress";
			public const string ConsignorCity = "ConsignorCity";
			public const string ConsignorState = "ConsignorState";
			public const string ConsignorPostCode = "ConsignorPostCode";

			public const string ConsigneeName = "ConsigneeName";
			public const string ConsigneeFullAddress = "ConsigneeFullAddress";
			public const string ConsigneeAddress = "ConsigneeAddress";
			public const string ConsigneeCity = "ConsigneeCity";
			public const string ConsigneeState = "ConsigneeState";
			public const string ConsigneePostCode = "ConsigneePostCode";

			public const string CurrentLoadPort = "CurrentLoadPort";
			public const string CurrentDischargePort = "CurrentDischargePort";
			public const string MainLoadPort = "MainLoadPort";
			public const string MainDischargePort = "MainDischargePort";

			public const string MainVessel = "MainVessel";
			public const string MainVoyageWithSuppression = "MainVoyageWithSuppression";
			public const string CurrentVessel = "CurrentVessel";
			public const string CurrentVoyageWithSuppression = "CurrentVoyageWithSuppression";

			public const string TransportMode = "TransportMode";

			public const string PacksWithUnits = "PacksWithUnits";
			public const string VolumeWithUnits = "VolumeWithUnits";
			public const string WeightWithUnits = "WeightWithUnits";

			public const string EstimatedPickupDate = "EstimatedPickupDate";
			public const string PickupDateRequiredBy = "PickupDateRequiredBy";
			public const string EstimatedDeliveryDate = "EstimatedDeliveryDate";
			public const string DeliveryDateRequiredBy = "DeliveryDateRequiredBy";
			public const string DeliveryDate = "DeliveryDate";
			public const string ActualPickupDate = "ActualPickupDate";

			public const string Charges = "Charges";
			public const string ReceivedDate = "ReceivedDate";
			public const string ReceivedBy = "ReceivedBy";
			public const string PiecesReceived = "PiecesReceived";
			public const string BookedOnline = "BookedOnline";

			public const string DeliveryAgentFullName = "DeliveryAgentFullName";
			public const string PickupAgentFullName = "PickupAgentFullName";
		}

		#endregion

		public TrackingCFSShipment(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Lookups

		public RefUNLOCOCollection Ports
		{
			get { return ports ?? (ports = new RefUNLOCOCollection(Factory)); }
		}
		RefUNLOCOCollection ports;

		public RefCurrencyCollection Currencies
		{
			get { return currencies ?? (currencies = new RefCurrencyCollection(Factory)); }
		}
		RefCurrencyCollection currencies;

		public RefServiceLevelCollection ServiceLevels
		{
			get { return serviceLevels ?? (serviceLevels = new WebServiceLevelCollection(Factory)); }
		}
		RefServiceLevelCollection serviceLevels;

		public TrackingMilestoneCollection Milestones
		{
			get { return milestones ?? (milestones = new TrackingMilestoneCollection(this)); }
		}
		TrackingMilestoneCollection milestones;

		#endregion

		#region TransportMode

		// TransportMode property is already defined in one of the parent classes

		public ZPropertyInfo TransportModeInfo
		{
			get { return GetZPropertyInfo(Schema.TransportMode); }
		}

		#endregion

		#region VoyageWithSuppression

		public ZString CurrentVoyageWithSuppression
		{
			get { return CurrentTransport != null ? Suppression.GetWebValue(CurrentTransport.JV_VoyageFlight, this, SuppressFields.FlightNumber) : ZString.Empty; }
		}

		public ZPropertyInfo CurrentVoyageWithSuppressionInfo
		{
			get { return GetZPropertyInfo(Schema.CurrentVoyageWithSuppression); }
		}

		public ZString MainVoyageWithSuppression
		{
			get { return MainTransport != null ? Suppression.GetWebValue(MainTransport.JV_VoyageFlight, this, SuppressFields.FlightNumber) : ZString.Empty; }
		}

		public ZPropertyInfo MainVoyageWithSuppressionInfo
		{
			get { return GetZPropertyInfo(Schema.MainVoyageWithSuppression); }
		}

		#endregion

		#region Charges

		public ZString Charges
		{
			get { return charges ?? (charges = InvoiceLoader.ChargesTotalsAsString); }
		}
		string charges;

		public ZPropertyInfo ChargesInfo
		{
			get { return GetZPropertyInfo(Schema.Charges); }
		}

		#endregion

		#region Current & Main Vessel

		public ZString CurrentVessel
		{
			get { return CurrentTransport != null ? CurrentTransport.JV_RV_NKVessel : ZString.Empty; }
		}

		public ZPropertyInfo CurrentVesselInfo
		{
			get { return GetZPropertyInfo(Schema.CurrentVessel); }
		}

		public ZString MainVessel
		{
			get { return MainTransport != null ? MainTransport.JV_RV_NKVessel : ZString.Empty; }
		}

		public ZPropertyInfo MainVesselInfo
		{
			get { return GetZPropertyInfo(Schema.MainVessel); }
		}

		#endregion

		#region Transport

		protected WebShipmentTransport MainTransport
		{
			get
			{
				foreach (WebShipmentTransport transport in ShipmentTransports)
				{
					if (transport.JW_TransportType == Core.Constants.TransportPlanningType.MainVessel)
					{
						return transport;
					}
				}
				return null;
			}
		}

		protected WebShipmentTransport CurrentTransport
		{
			get
			{
				WebShipmentTransport result = null;
				foreach (WebShipmentTransport transport in ShipmentTransports)
				{
					if (!transport.JW_ATD.IsEmpty && transport.JW_ATA.IsEmpty)
					{
						return transport;
					}

					if (result == null || !transport.JW_ATA.IsEmpty)
					{
						result = transport;
					}
				}
				return result;
			}
		}

		protected WebShipmentTransportCollection ShipmentTransports
		{
			get
			{
				return shipmentTransports ?? (shipmentTransports = GetShipmentTransports());
			}
		}

		protected WebShipmentTransportCollection shipmentTransports;

		protected WebShipmentTransportCollection GetShipmentTransports()
		{
			WebShipmentTransportCollection collection = new WebShipmentTransportCollection(Factory, PK);
			collection.Load();
			return collection;
		}

		#endregion

		#region Address

		ZString GetAddress1And2(JobDocAddress jobDocAddress)
		{
			List<string> lines = new List<string>();

			if (jobDocAddress != null)
			{
				if (jobDocAddress.E2_AddressOverride)
				{
					lines.Add(jobDocAddress.E2_Address1);
					if (!jobDocAddress.E2_Address2.IsEmpty)
					{
						lines.Add(jobDocAddress.E2_Address2);
					}
				}
				else if (jobDocAddress.Address != null)
				{
					lines.Add(jobDocAddress.Address.OA_Address1);
					if (!jobDocAddress.Address.OA_Address2.IsEmpty)
					{
						lines.Add(jobDocAddress.Address.OA_Address2);
					}
				}
			}

			return string.Join(", ", lines.ToArray());
		}

		ZString GetCity(JobDocAddress jobDocAddress)
		{
			if (jobDocAddress != null)
			{
				if (jobDocAddress.E2_AddressOverride)
				{
					return jobDocAddress.E2_City;
				}
				else if (jobDocAddress.Address != null)
				{
					return jobDocAddress.Address.OA_City;
				}
			}
			return ZString.Empty;
		}

		ZString GetState(JobDocAddress jobDocAddress)
		{
			if (jobDocAddress != null)
			{
				if (jobDocAddress.E2_AddressOverride)
				{
					return jobDocAddress.E2_State;
				}
				else if (jobDocAddress.Address != null)
				{
					return jobDocAddress.Address.OA_State;
				}
			}
			return ZString.Empty;
		}

		ZString GetPostCode(JobDocAddress jobDocAddress)
		{
			if (jobDocAddress != null)
			{
				if (jobDocAddress.E2_AddressOverride)
				{
					return jobDocAddress.E2_Postcode;
				}
				else if (jobDocAddress.Address != null)
				{
					return jobDocAddress.Address.OA_PostCode;
				}
			}
			return ZString.Empty;
		}

		#endregion

		#region CommonPickupDelivery

		CommonPickupDeliveryConfirm LastDeliveredLeg
		{
			get
			{
				if (lastDeliveredLeg == null)
				{
					foreach (CommonPickupDeliveryConfirm leg in DeliveryConfirms)
					{
						if (lastDeliveredLeg == null || lastDeliveredLeg.EU_PickupDeliveryTime < leg.EU_PickupDeliveryTime)
						{
							lastDeliveredLeg = leg;
						}
					}
				}

				return lastDeliveredLeg;
			}
		}

		CommonPickupDeliveryConfirm lastDeliveredLeg;

		#endregion

		#region Lists
		public ICodeDescriptionPairList ChargesApply_List
		{
			get { return Lookups.JS_HBLAWBChargesDisplay_List; }
		}

		public ICodeDescriptionPairList ReleaseType_List
		{
			get { return Lookups.JS_ReleaseType_List; }
		}

		public ICodeDescriptionPairList OnBoard_List
		{
			get { return Lookups.JS_ShippedOnBoard_List; }
		}

		public ICodeDescriptionPairList PaymentTerm_List
		{
			get { return Lookups.JS_INCO_List; }
		}

		#endregion

		#region DeliveryAgentFullName

		public ZString DeliveryAgentFullName
		{
			get { return DeliveryAgent != null ? DeliveryAgent.OH_FullNameTruncated : ZString.Empty; }
		}

		public ZPropertyInfo DeliveryAgentFullNameInfo
		{
			get { return GetZPropertyInfo(Schema.DeliveryAgentFullName); }
		}

		#endregion

		#region PickupAgentFullName

		public ZString PickupAgentFullName
		{
			get { return PickupAgent != null ? PickupAgent.OH_FullNameTruncated : ZString.Empty; }
		}

		public ZPropertyInfo PickupAgentFullNameInfo
		{
			get { return GetZPropertyInfo(Schema.PickupAgentFullName); }
		}

		#endregion

		#region VolumeWithUnits

		public ZString VolumeWithUnits
		{
			get
			{
				var roundedDecimal = this.GetRoundedValue(JobShipmentSchema.JS_ActualVolume, JS_ActualVolumeInfo, JS_ActualVolume);
				var formattedDecimal = FormatNumber(roundedDecimal, DefaultNumberOfDecimalsSupporterHelperForFreight.GetDefaultNumberOfDecimalsMetaDataProperty(this, JS_ActualVolumeInfo.PropertyDescriptor));
				return string.Format(CultureInfo.InvariantCulture, "{0} {1}", formattedDecimal, JS_UnitOfVolume);
			}
		}

		public ZPropertyInfo VolumeWithUnitsInfo
		{
			get { return GetZPropertyInfo(Schema.VolumeWithUnits); }
		}

		#endregion

		#region WeightWithUnits

		public ZString WeightWithUnits
		{
			get
			{
				var roundedDecimal = this.GetRoundedValue(JobShipmentSchema.JS_ActualWeight, JS_ActualWeightInfo, JS_ActualWeight);
				var formattedDecimal = FormatNumber(roundedDecimal, DefaultNumberOfDecimalsSupporterHelperForFreight.GetDefaultNumberOfDecimalsMetaDataProperty(this, JS_ActualWeightInfo.PropertyDescriptor));
				return string.Format(CultureInfo.InvariantCulture, "{0} {1}", formattedDecimal, JS_UnitOfWeight);
			}
		}

		protected string FormatNumber(ZDecimal number, int decimalsToShow)
		{
			return Utilities.FormatNumber(number, decimalsToShow, WebEnvShared.ClientCulture);
		}

		public ZPropertyInfo WeightWithUnitsInfo
		{
			get { return GetZPropertyInfo(Schema.WeightWithUnits); }
		}

		#endregion

		#region Ports

		#region Current & Main LoadPort

		[RequiresSuppression]
		public ZString CurrentLoadPort
		{
			get { return CurrentTransport != null ? CurrentTransport.JA_RL_NKPortOfLoading : ZString.Empty; }
		}

		public ZPropertyInfo CurrentLoadPortInfo
		{
			get { return GetZPropertyInfo(Schema.CurrentLoadPort); }
		}

		[RequiresSuppression]
		public ZString MainLoadPort
		{
			get { return MainTransport != null ? MainTransport.JA_RL_NKPortOfLoading : ZString.Empty; }
		}

		public ZPropertyInfo MainLoadPortInfo
		{
			get { return GetZPropertyInfo(Schema.MainLoadPort); }
		}

		#endregion

		#region Current & Main DischargePort

		[RequiresSuppression]
		public ZString CurrentDischargePort
		{
			get { return CurrentTransport != null ? CurrentTransport.JB_RL_NKPortOfDischarge : ZString.Empty; }
		}

		public ZPropertyInfo CurrentDischargePortInfo
		{
			get { return GetZPropertyInfo(Schema.CurrentDischargePort); }
		}

		[RequiresSuppression]
		public ZString MainDischargePort
		{
			get { return MainTransport != null ? MainTransport.JB_RL_NKPortOfDischarge : ZString.Empty; }
		}

		public ZPropertyInfo MainDischargePortInfo
		{
			get { return GetZPropertyInfo(Schema.MainDischargePort); }
		}

		#endregion

		#endregion

		#region Consignor

		#region ConsignorName

		public ZString ConsignorName
		{
			get { return Consignor != null ? Consignor.OH_FullNameTruncated : ConsignorDocumentaryAddress.E2_CompanyNameTruncated; }
		}

		public ZPropertyInfo ConsignorNameInfo
		{
			get { return GetZPropertyInfo(Schema.ConsignorName); }
		}

		#endregion

		#region ConsignorFullAddress

		public ZString ConsignorFullAddress
		{
			get
			{
				WebAddressFormatter formatter = new WebAddressFormatter(ConsignorDocumentaryAddress);
				return formatter.FormattedAddressWithCompanyName(ConsignorName);
			}
		}

		public ZPropertyInfo ConsignorFullAddressInfo
		{
			get { return GetZPropertyInfo(Schema.ConsignorFullAddress); }
		}

		#endregion

		#region ConsignorAddress

		public ZString ConsignorAddress
		{
			get { return GetAddress1And2(ConsignorDocumentaryAddress); }
		}

		public ZPropertyInfo ConsignorAddressInfo
		{
			get { return GetZPropertyInfo(Schema.ConsignorAddress); }
		}

		#endregion

		#region ConsignorCity

		public ZString ConsignorCity
		{
			get { return GetCity(ConsignorDocumentaryAddress); }
		}

		public ZPropertyInfo ConsignorCityInfo
		{
			get { return GetZPropertyInfo(Schema.ConsignorCity); }
		}

		#endregion

		#region ConsignorState

		public ZString ConsignorState
		{
			get { return GetState(ConsignorDocumentaryAddress); }
		}

		public ZPropertyInfo ConsignorStateInfo
		{
			get { return GetZPropertyInfo(Schema.ConsignorState); }
		}

		#endregion

		#region ConsignorPostCode

		public ZString ConsignorPostCode
		{
			get { return GetPostCode(ConsignorDocumentaryAddress); }
		}

		public ZPropertyInfo ConsignorPostCodeInfo
		{
			get { return GetZPropertyInfo(Schema.ConsignorPostCode); }
		}

		#endregion

		#endregion

		#region Consignee

		#region ConsigneeName

		public ZString ConsigneeName
		{
			get { return Consignee != null ? Consignee.OH_FullNameTruncated : ConsigneeDocumentaryAddress.E2_CompanyNameTruncated; }
		}

		public ZPropertyInfo ConsigneeNameInfo
		{
			get { return GetZPropertyInfo(Schema.ConsigneeName); }
		}

		#endregion

		#region ConsigneeFullAddress

		public ZString ConsigneeFullAddress
		{
			get
			{
				WebAddressFormatter formatter = new WebAddressFormatter(ConsigneeDocumentaryAddress);
				return formatter.FormattedAddressWithCompanyName(ConsigneeName);
			}
		}

		public ZPropertyInfo ConsigneeFullAddressInfo
		{
			get { return GetZPropertyInfo(Schema.ConsigneeFullAddress); }
		}

		#endregion

		#region ConsigneeAddress

		public ZString ConsigneeAddress
		{
			get { return GetAddress1And2(ConsigneeDocumentaryAddress); }
		}

		public ZPropertyInfo ConsigneeAddressInfo
		{
			get { return GetZPropertyInfo(Schema.ConsigneeAddress); }
		}

		#endregion

		#region ConsigneeCity

		public ZString ConsigneeCity
		{
			get { return GetCity(ConsigneeDocumentaryAddress); }
		}

		public ZPropertyInfo ConsigneeCityInfo
		{
			get { return GetZPropertyInfo(Schema.ConsigneeCity); }
		}

		#endregion

		#region ConsigneeState

		public ZString ConsigneeState
		{
			get { return GetState(ConsigneeDocumentaryAddress); }
		}

		public ZPropertyInfo ConsigneeStateInfo
		{
			get { return GetZPropertyInfo(Schema.ConsigneeState); }
		}

		#endregion

		#region ConsigneePostCode

		public ZString ConsigneePostCode
		{
			get { return GetPostCode(ConsigneeDocumentaryAddress); }
		}

		public ZPropertyInfo ConsigneePostCodeInfo
		{
			get { return GetZPropertyInfo(Schema.ConsigneePostCode); }
		}

		#endregion

		#endregion

		#region DeliverTo

		#region DeliverToName

		public ZString DeliverToName
		{
			get { return ConsigneeDeliveryAddress.E2_CompanyNameTruncated; }
		}

		#endregion

		#region DeliverToFullAddress

		public ZString DeliverToFullAddress
		{
			get
			{
				WebAddressFormatter formatter = new WebAddressFormatter(ConsigneeDeliveryAddress);
				return formatter.FormattedAddressWithCompanyName(DeliverToName);
			}
		}

		#endregion

		#endregion

		#region PickupFrom

		#region PickupFromName

		public ZString PickupFromName
		{
			get { return ConsignorPickupAddress.E2_CompanyNameTruncated; }
		}

		#endregion

		#region PickupFromFullAddress

		public ZString PickupFromFullAddress
		{
			get
			{
				WebAddressFormatter formatter = new WebAddressFormatter(ConsignorPickupAddress);
				return formatter.FormattedAddressWithCompanyName(PickupFromName);
			}
		}

		#endregion

		#endregion

		#region DocsAndCartage

		#region EstimatedPickupDate

		public ZDateTime EstimatedPickupDate
		{
			get { return IsDeleted ? ZDateTime.Empty : DocsAndCartage.JP_EstimatedPickup; }
		}

		public ZPropertyInfo EstimatedPickupDateInfo
		{
			get { return GetZPropertyInfo(Schema.EstimatedPickupDate); }
		}

		#endregion

		#region PickupDateRequiredBy

		public ZDateTime PickupDateRequiredBy
		{
			get { return IsDeleted ? ZDateTime.Empty : DocsAndCartage.JP_PickupRequiredBy; }
		}

		public ZPropertyInfo PickupDateRequiredByInfo
		{
			get { return GetZPropertyInfo(Schema.PickupDateRequiredBy); }
		}

		#endregion

		#region EstimatedDeliveryDate

		public ZDateTime EstimatedDeliveryDate
		{
			get { return IsDeleted ? ZDateTime.Empty : DocsAndCartage.JP_EstimatedDelivery; }
		}

		public ZPropertyInfo EstimatedDeliveryDateInfo
		{
			get { return GetZPropertyInfo(Schema.EstimatedDeliveryDate); }
		}

		#endregion

		#region DeliveryDateRequiredBy

		public ZDateTime DeliveryDateRequiredBy
		{
			get { return IsDeleted ? ZDateTime.Empty : DocsAndCartage.JP_DeliveryRequiredBy; }
		}

		public ZPropertyInfo DeliveryDateRequiredByInfo
		{
			get { return GetZPropertyInfo(Schema.DeliveryDateRequiredBy); }
		}

		#endregion

		#region DeliveryDate

		public ZDateTime DeliveryDate
		{
			get { return IsDeleted ? ZDateTime.Empty : DocsAndCartage.JP_DeliveryCartageCompleted; }
		}

		public ZPropertyInfo DeliveryDateInfo
		{
			get { return GetZPropertyInfo(Schema.DeliveryDate); }
		}

		#endregion

		#region ActualPickupDate

		public ZDateTime ActualPickupDate
		{
			get { return IsDeleted ? ZDateTime.Empty : DocsAndCartage.JP_PickupCartageCompleted; }
		}

		public ZPropertyInfo ActualPickupDateInfo
		{
			get { return GetZPropertyInfo(Schema.ActualPickupDate); }
		}

		#endregion

		#endregion

		#region MasterBill

		public ZString MasterBill
		{
			get { return CurrentTransport != null ? CurrentTransport.JK_MasterBillNum : ZString.Empty; }
		}

		public ZPropertyInfo MasterBillInfo
		{
			get { return GetZPropertyInfo(Schema.MasterBill); }
		}

		#endregion

		#region PacksWithUnits

		public ZString PacksWithUnits
		{
			get { return string.Format(CultureInfo.CurrentCulture, "{0} {1}", JS_OuterPacks, JS_F3_NKPackType); }
		}

		public ZPropertyInfo PacksWithUnitsInfo
		{
			get { return GetZPropertyInfo(Schema.PacksWithUnits); }
		}

		#endregion

		#region LastDeliveredLeg

		#region ReceivedDate

		public ZDateTime ReceivedDate
		{
			get { return LastDeliveredLeg != null ? LastDeliveredLeg.EU_PickupDeliveryTime : ZDateTime.Empty; }
		}

		public ZPropertyInfo ReceivedDateInfo
		{
			get { return GetZPropertyInfo(Schema.ReceivedDate); }
		}

		#endregion

		#region ReceivedBy

		public ZString ReceivedBy
		{
			get { return LastDeliveredLeg != null ? LastDeliveredLeg.EU_GoodsSignForBy : ZString.Empty; }
		}

		public ZPropertyInfo ReceivedByInfo
		{
			get { return GetZPropertyInfo(Schema.ReceivedBy); }
		}

		#endregion

		#region PiecesReceived

		public ZInt PiecesReceived
		{
			get { return LastDeliveredLeg != null ? LastDeliveredLeg.TotalDeliveredPackages : ZInt.Zero; }
		}

		public ZPropertyInfo PiecesReceivedInfo
		{
			get { return GetZPropertyInfo(Schema.PiecesReceived); }
		}

		#endregion

		#endregion

		#region BookedOnline

		public ZBool BookedOnline
		{
			get { return JS_SystemCreateUser == "ZZ"; }
		}

		public ZPropertyInfo BookedOnlineInfo
		{
			get { return GetZPropertyInfo(Schema.BookedOnline); }
		}

		#endregion

		#region FromNumber
		public static TrackingCFSShipment FromPKFilteredBySiteUser(BusinessObjectFactory factory, ZGuid shipmentPK, TrackingSiteUser siteUser)
		{
			TrackingCFSShipment result = null;
			if (siteUser != null && siteUser.LoggedInOrganisation != null)
			{
				ZQuery filter = new ZQuery(JobShipmentSchema.PK, shipmentPK);
				if (!siteUser.IsShipmentQuickViewUser) // We use it to enable direct view of SHipment Detalils by Housebill Number
				{
					filter.AddToFilter(OrgRestrictionFilterFactory.Instance.GetFilter<TrackingCFSShipment>());
				}
				filter.IgnoreActiveFilter = true;
				result = (TrackingCFSShipment)factory.LoadTop1(typeof(TrackingCFSShipment), filter);
				if (result != null)
				{
					result.SiteUser = siteUser;
				}
			}
			return result;
		}

		#endregion FromNumber

		#region Property wrappers

		public new ZString ShipmentType
		{
			get { return JS_ShipmentType; }
		}

		public ZPropertyInfo ShipmentTypeInfo
		{
			get { return GetZPropertyInfo(Schema.ShipmentType); }
		}

		#region LoggedInOrganisation

		public OrgHeader LoggedInOrganisation
		{
			get { return (LoggedInContact == null) ? null : LoggedInContact.ParentOrg; }
		}

		#endregion

		#region Related documents

		public DocumentSupport DocumentHelper
		{
			get
			{
				if (documentHelper == null)
				{
					documentHelper = new DocumentSupport(this);
				}
				return documentHelper;
			}
		}
		DocumentSupport documentHelper;

		public ZGuid DocParentPK
		{
			get { return PK; }
		}

		public List<ZGuid> DocRelatedPKs
		{
			get
			{
				List<ZGuid> result = new List<ZGuid>();
				foreach (BusinessObject declaration in Declarations)
				{
					result.Add(declaration.PK);

					var dec = declaration as BaseJobDeclaration; // declaration is BaseJobDeclaration ? (BaseJobDeclaration)declaration : null;
					if (dec != null)
					{
						result.AddRange(new TrackingDeclaration(dec, SiteUser).DocRelatedPKs);
					}
				}
				if (LocalConsol != null && LocalConsol.JK_AgentType == Core.Constants.AgentType.Direct)
				{
					result.Add(LocalConsol.PK);
				}
				return result.Distinct().ToList();
			}
		}

		public TrackingSiteUser SiteUser
		{
			get { return siteUser; }
			set { siteUser = value; }
		}
		protected TrackingSiteUser siteUser;

		public OrgContact LoggedInContact
		{
			get { return SiteUser != null ? SiteUser.LoggedInUser : null; }
		}

		#endregion

		#region VisibleNotes

		public IStmNoteParent NotesParentBO
		{
			get { return this; }
		}

		public WebUserVisibleNotes NotesHelper
		{
			get
			{
				if (notesHelper == null)
				{
					notesHelper = new WebUserVisibleNotes(this);
				}
				return notesHelper;
			}
		}

		WebUserVisibleNotes notesHelper;

		#endregion

		#endregion

		WebPartyTypeOrgPairCollection WebParties
		{
			get
			{
				webParties = new WebPartyTypeOrgPairCollection();
				webParties.Add(WebPartyType.Shipper, Consignor);
				webParties.Add(WebPartyType.Consignee, Consignee);
				if (Job != null)
				{
					webParties.Add(WebPartyType.LocalClient, Job.LocalCharges);
				}
				webParties.Add(WebPartyType.DeliveryAgent, DeliveryAgent);
				webParties.Add(WebPartyType.ImportBroker, ImportBroker);
				webParties.Add(WebPartyType.ExportBroker, ExportBroker);

				return webParties;
			}
		}
		WebPartyTypeOrgPairCollection webParties;

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

		#region New fields

		#region Transports

		public RoutingCollection RelatedTransportsInLegOrder
		{
			get
			{
				if (relatedTransportsInLegOrder == null)
				{
					relatedTransportsInLegOrder = TransportsIncludingRelated;
					relatedTransportsInLegOrder.Sort(MovementLegComparer.PortsAndDatesBased(relatedTransportsInLegOrder));
				}

				return relatedTransportsInLegOrder;
			}
		}

		RoutingCollection relatedTransportsInLegOrder;

		#endregion

		#region Delivery

		[ChildEditable(true)]
		public CommonPickupDeliveryConfirmCollection RelatedCommonPickupDeliveryConfirm
		{
			get
			{
				if (relatedCommonPickupDeliveryConfirm == null)
				{
					relatedCommonPickupDeliveryConfirm = DestinationCFSDepartures;
				}

				return relatedCommonPickupDeliveryConfirm;
			}
		}

		CommonPickupDeliveryConfirmCollection relatedCommonPickupDeliveryConfirm;

		#endregion

		#region Pickup Address

		public ZString PickupAddressAsText
		{
			get
			{
				return (ConsignorPickupAddress == null) ? ZString.Empty : ConsignorPickupAddress.AddressAsASingleLine;
			}
		}

		public ZPropertyInfo PickupAddressAsTextInfo
		{
			get { return GetZPropertyInfo(Schema.PickupAddressAsText); }
		}

		#endregion

		#region DeliveryAddressAsText

		public ZString DeliveryAddressAsText
		{
			get
			{
				return (ConsigneeDeliveryAddress == null) ? ZString.Empty : ConsigneeDeliveryAddress.AddressAsASingleLine;
			}
		}

		public ZPropertyInfo DeliveryAddressAsTextInfo
		{
			get { return GetZPropertyInfo(Schema.DeliveryAddressAsText); }
		}

		#endregion

		#region Available Date

		public ZDateTime AvailableDate
		{
			get
			{
				return Core.Constants.ContainerModes.IsLCLType(JS_PackingMode)
					? DocsAndCartage.JP_LCLAvailable
					: DocsAndCartage.JP_FCLAvailable;
			}
		}

		public ZPropertyInfo AvailableDateInfo
		{
			get { return GetZPropertyInfo(Schema.AvailableDate); }
		}

		#endregion

		#region AvailableAtAddressAsText

		public ZString AvailableAtAddressAsText
		{
			get
			{
				return ImportReleaseDepot != null ? ImportReleaseDepot.AddressAsASingleLine :
					ArrivalConsol != null && ArrivalConsol.UnpackDepotAddress != null ? ArrivalConsol.UnpackDepotAddress.AddressAsASingleLine : ZString.Empty;
			}
		}

		public ZPropertyInfo AvailableAtAddressAsTextInfo
		{
			get { return GetZPropertyInfo(Schema.AvailableAtAddressAsText); }
		}

		#endregion

		#region Storage Date

		public ZDateTime StorageDate
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;
				if (Core.Constants.ContainerModes.IsLCLType(JS_PackingMode))
				{
					result = DocsAndCartage.JP_LCLStorageCommences;
					if (result.IsEmpty && ArrivalConsol != null)
					{
						result = ArrivalConsol.JK_DepotStorageDate;
					}
				}
				else
				{
					result = DocsAndCartage.JP_FCLStorageCommences;
					if (result.IsEmpty && ArrivalConsol != null)
					{
						result = ArrivalConsol.JK_CTOStorageDate;
					}
				}
				return result;
			}
		}

		public ZPropertyInfo StorageDateInfo
		{
			get { return GetZPropertyInfo(Schema.StorageDate); }
		}

		#endregion

		#endregion

		#region Suppress Flight Details

		#region LoadETDWithSuppression

		public ZDateTime LoadETDWithSuppression
		{
			get
			{
				ZDateTime result = (DepartureConsol != null && DepartureConsol.Transports.Count > 0)
					? DepartureConsol.Transports.DepartureTransport.JW_ETD
					: ZDateTime.Empty;

				return Suppression.GetWebValue(result, this, SuppressFields.ETD);
			}
		}

		public ZPropertyInfo LoadETDWithSuppressionInfo
		{
			get { return GetZPropertyInfo(Schema.LoadETDWithSuppression); }
		}

		#endregion

		#region DischargeETAWithSuppression

		public ZDateTime DischargeETAWithSuppression
		{
			get
			{
				ZDateTime result = (ArrivalConsol != null && ArrivalConsol.Transports.Count > 0)
					? ArrivalConsol.Transports.ArrivalTransport.JW_ETA
					: ZDateTime.Empty;

				return Suppression.GetWebValue(result, this, SuppressFields.ETA);
			}
		}

		public ZPropertyInfo DischargeETAWithSuppressionInfo
		{
			get { return GetZPropertyInfo(Schema.DischargeETAWithSuppression); }
		}

		#endregion

		#region ETDWithSuppression

		public ZDateTime ETDWithSuppression
		{
			get { return Suppression.GetWebValue(JS_E_DEP, this, SuppressFields.ETD); }
		}

		public ZPropertyInfo ETDWithSuppressionInfo
		{
			get { return GetZPropertyInfo(Schema.ETDWithSuppression); }
		}

		#endregion

		#region ETAWithSuppression

		public ZDateTime ETAWithSuppression
		{
			get { return Suppression.GetWebValue(JS_E_ARV, this, SuppressFields.ETA); }
		}

		public ZPropertyInfo ETAWithSuppressionInfo
		{
			get { return GetZPropertyInfo(Schema.ETAWithSuppression); }
		}

		#endregion

		#region Property overrides

		[RequiresSuppression]
		public override ZDateTime JS_E_DEP
		{
			get { return base.JS_E_DEP; }
			set { base.JS_E_DEP = value; }
		}

		[RequiresSuppression]
		public override ZDateTime JS_E_ARV
		{
			get { return base.JS_E_ARV; }
			set { base.JS_E_ARV = value; }
		}

		#endregion

		#endregion

		#region ITransactionSupport Members

		public ZString Reference
		{
			get { return JS_UniqueConsignRef; }
		}

		public TrackingInvoiceLoader InvoiceLoader
		{
			get
			{
				if (invoiceLoader == null)
				{
					invoiceLoader = new TrackingInvoiceLoader(this);
				}

				return invoiceLoader;
			}
		}
		TrackingInvoiceLoader invoiceLoader;

		#endregion

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new TrackingShipmentFetchStrategy(this);
		}

		public override BusinessObject[] BusinessObjectsWithRelatedNotes
		{
			get
			{
				List<BusinessObject> result = new List<BusinessObject>();

				if (base.BusinessObjectsWithRelatedNotes.Length > 0)
				{
					result.AddRange(base.BusinessObjectsWithRelatedNotes);
				}

				return result.ToArray();
			}
		}

		public void EnableOnlyDeliveryConfirmationsValidationOnPreSave()
		{
			isOnlyDeliveryConfirmationsValidationEnabledOnPreSave = true;
		}
		bool isOnlyDeliveryConfirmationsValidationEnabledOnPreSave;

		protected override void RunPreSaveValidationCore()
		{
			if (isOnlyDeliveryConfirmationsValidationEnabledOnPreSave)
			{
				foreach (CommonPickupDeliveryConfirm confirm in DeliveryConfirms)
				{
					confirm.Validation.ValidateAll();
				}
			}
			else
			{
				base.RunPreSaveValidationCore();
			}
		}

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
			documentHelper = null;
		}

		#endregion

		#region IWebUserVisibleNotesSupport Members

		public bool ShowAgentNotes
		{
			get
			{
				return true;
			}
		}

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

		#endregion

	}
}
