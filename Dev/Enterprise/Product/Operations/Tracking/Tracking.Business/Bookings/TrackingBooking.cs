using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.BufferManagement.Integration;
using Enterprise.Customs.Common;
using Enterprise.DocumentEngine.GUI;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Freight.Forwarding.Orders.Module;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Business;
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
	public class TrackingBooking : NonPersistentBusinessObjectWithLogsAndNotes,
				IBizOChangesEmailNotification,
				IWebUserEditableNoteSupport,
				IDocumentSupportable,
				IWebDocumentsWithUploadSupport,
				IWorkflowProvider,
				IDocumentsMenuProvider,
				IModuleFilterProvider,
				ITemplateCopyable,
				ITemplateReversible,
				IContainerListProvider,
				IUpdatableMilestoneEventsProvider,
				IMilestonesProvider,
				ITrackingEventsProvider,
				IEventReferenceProvider
	{
		#region Constructors

		public TrackingBooking(BusinessObjectFactory factory, TrackingSiteUser siteUser)
			: this(QuotedBooking.CreateNewBooking(factory), factory, siteUser)
		{
		}

		public TrackingBooking(ForwardingShipment booking, BusinessObjectFactory factory, TrackingSiteUser siteUser)
			: this(booking.PK, factory, siteUser)
		{
			if (siteUser != null && siteUser.LoggedInOrganisation != null)
			{
				if (Booking != null)
				{
					var settings = Env.Registry.GetWebUserDefaultSettingsForDocAddress(siteUser.LoggedInUser.PK.ToGuid());
					var defaultOrgIsSet = false;

					if (!settings.IsEmpty && siteUser.LoggedInOrganisation.PK == settings.OrganisationPK)
					{
						if (siteUser.LoggedInOrganisation.OH_IsConsignee &&
							settings.DocAddressType == Booking.ConsigneeDeliveryAddress.DocAddressType.ToString())
						{
							Booking.ConsigneeDeliveryAddress.OrganisationPK = settings.OrganisationPK;
							Booking.ConsigneeDeliveryAddress.E2_OA_Address = settings.AddressPK;
							Booking.ConsigneeDeliveryAddress.ContactPK = settings.ContactPK;
							defaultOrgIsSet = true;
						}
						else if (siteUser.LoggedInOrganisation.OH_IsConsignor &&
							settings.DocAddressType == Booking.ConsignorPickupAddress.DocAddressType.ToString())
						{
							Booking.ConsignorPickupAddress.OrganisationPK = settings.OrganisationPK;
							Booking.ConsignorPickupAddress.E2_OA_Address = settings.AddressPK;
							Booking.ConsignorPickupAddress.ContactPK = settings.ContactPK;
							defaultOrgIsSet = true;
						}
					}

					if (!defaultOrgIsSet)
					{
						if (siteUser.LoggedInOrganisation.OH_IsConsignor)
						{
							Booking.ConsignorPickupAddress.OrganisationPK = siteUser.LoggedInOrganisation.PK;
							Booking.ConsignorPickupAddress.ContactPK = siteUser.LoggedInUser.PK;
						}
						else if (siteUser.LoggedInOrganisation.OH_IsConsignee)
						{
							Booking.ConsigneeDeliveryAddress.OrganisationPK = siteUser.LoggedInOrganisation.PK;
							Booking.ConsigneeDeliveryAddress.ContactPK = siteUser.LoggedInUser.PK;
						}
					}

					if (Booking.ConsigneeDeliveryAddress.IsEmpty && !QuotedBooking.ConsigneeDocumentaryAddress.IsEmpty)
					{
						Booking.ConsigneeDeliveryAddress.OrganisationPK = QuotedBooking.ConsigneeDocumentaryAddress.OrganisationPK;
						Booking.ConsigneeDeliveryAddress.E2_OA_Address = QuotedBooking.ConsigneeDocumentaryAddress.E2_OA_Address;
						Booking.ConsigneeDeliveryAddress.ContactPK = QuotedBooking.ConsigneeDocumentaryAddress.ContactPK;
					}

					if (Booking.ConsignorPickupAddress.IsEmpty && !QuotedBooking.ConsignorDocumentaryAddress.IsEmpty)
					{
						Booking.ConsignorPickupAddress.OrganisationPK = QuotedBooking.ConsignorDocumentaryAddress.OrganisationPK;
						Booking.ConsignorPickupAddress.E2_OA_Address = QuotedBooking.ConsignorDocumentaryAddress.E2_OA_Address;
						Booking.ConsignorPickupAddress.ContactPK = QuotedBooking.ConsignorDocumentaryAddress.ContactPK;
					}
				}

				QuotedBooking.Logs.AutoCreatedLogDefaultSL_Reference = siteUser.ContactAndCompanyReference;

				SetModeAndBillDefaults();
			}
		}

		public TrackingBooking(ZGuid bookingPK, BusinessObjectFactory factory, TrackingSiteUser siteUser)
			: this(bookingPK, ZGuid.Empty, factory, siteUser)
		{
		}

		public TrackingBooking(ZGuid bookingPK, ZGuid quotePK, BusinessObjectFactory factory, TrackingSiteUser siteUser)
			: this(QuotedBooking.New(quotePK, bookingPK, factory, new TrackingScheduleChooserCreator(siteUser)), siteUser != null ? siteUser.LoggedInUser : null)
		{
		}

		public TrackingBooking(QuotedBooking quotedBooking, OrgContact webUser)
			: base(quotedBooking.Factory)
		{
			this.quotedBooking = quotedBooking;
			quotedBooking.WebServiceLevelCollection = () => new WebServiceLevelCollection(Factory, GetTemporaryPublishedServiceLevelQuery(), JoinCondition.Or);

			RegisterEditableChildObject(QuotedBookingContainers);

			quotedBooking.IsDomesticFreightInfo.ValueChanged += OnIsDomesticFreightInternalValueChanged;

			LoggedInContact = webUser;
			if (Booking != null)
			{
				Booking.NewBillOfLadingGeneratorContextOverride = GetBillOfLadingCustomisation;
				RegisterEditableChildObject(Booking);
				RegisterEditableChildObject(ConsignorPickupAddress);
				RegisterEditableChildObject(ConsigneeDeliveryAddress);
				RegisterEditableChildObject(OuterPackLines);
				Booking.JS_INCOInfo.ValueChanged += new EventHandler(JS_INCOInfo_ValueChanged);
				Booking.ConsigneeChanged += new EventHandler(ConsigneePKChanged);
			}
			quotedBooking.ModeInfo.ValueChanged += new EventHandler(ModeChanged);

			SetModeAndBillDefaults();
			SetIsDomesticFreight();
		}

		#endregion

		#region BookingPartyDocumentaryAddress

		public JobDocAddress BookingPartyDocumentaryAddress
		{
			get { return Booking != null ? Booking.BookingPartyDocumentaryAddress : null; }
		}

		#endregion

		void SetModeAndBillDefaults()
		{
			if (QuotedBooking != null && QuotedBooking.Mode.IsEmpty)
			{
				QuotedBooking.Mode = Constants.RateMode.LSE;
			}

			if (Booking != null && Booking.JS_HouseBillOfLadingType.IsEmpty)
			{
				Booking.JS_HouseBillOfLadingType = WebDataRegistry.Instance.BookingDefaultHouseBillType.Value;
			}
		}

		internal ZQuery GetTemporaryPublishedServiceLevelQuery()
		{
			ZQuery result = new ZQuery();
			if (!TemporaryServiceLevelPK.IsEmpty)
			{
				result.AddToFilter(RefServiceLevelSchema.PK, TemporaryServiceLevelPK);
			}
			return result;
		}

		public ZGuid TemporaryServiceLevelPK = ZGuid.Empty;

		#region GetFromRefPK

		public static TrackingBooking GetFromRefPK(BusinessObjectFactory factory, ZGuid refPK, TrackingSiteUser siteUser)
		{
			var trackingBooking = GetFromBookingOrQuotePK(factory, refPK, siteUser);

			return trackingBooking?.Booking != null ? trackingBooking : null;
		}

		static TrackingBooking GetFromBookingOrQuotePK(BusinessObjectFactory factory, ZGuid refPK, TrackingSiteUser siteUser)
		{
			if (siteUser?.LoggedInOrganisation == null)
			{
				return null;
			}

			if (siteUser.IsShipmentQuickViewUser)
			{
				if (factory.Load<ForwardingShipment>(refPK) != null)
				{
					return new TrackingBooking(refPK, factory, siteUser);
				}

				if (factory.Load<Quote>(refPK) != null)
				{
					return new TrackingBooking(ZGuid.Empty, refPK, factory, siteUser);
				}
			}

			var filter = OrgRestrictionFilterFactory.Instance.GetFilter<TrackingBooking>();
			var pkFilter = new ZQuery(ViewQuotedBookingSchema.VB_JS, refPK);
			pkFilter.AddToFilter(new ZQuery(ViewQuotedBookingSchema.VB_TH, refPK), JoinCondition.Or);
			filter.AddToFilter(pkFilter);
			var result = factory.LoadTop1<ViewTrackingBooking>(filter);

			return result?.TrackingBooking;
		}

		#endregion

		public QuotedBooking QuotedBooking
		{
			get { return quotedBooking; }
		}

		readonly QuotedBooking quotedBooking;

		internal ForwardingShipment Booking
		{
			get { return QuotedBooking != null ? QuotedBooking.Booking : null; }
		}

		#region Schema

		public abstract class Schema
		{
			public const string Origin = "Origin";
			public const string Destination = "Destination";
			public const string Mode = "Mode";
			public const string IsDomesticFreight = "IsDomesticFreight";
			public const string ServiceLevel = "ServiceLevel";

			public const string ThirdPartyAddressPK = "ThirdPartyAddressPK";

			public const string CustomsEntryNumber = "CustomsEntryNumber";
			public const string UniqueConsignRef = "UniqueConsignRef";
			public const string BookingReference = "BookingReference";
			public const string CFSReference = "CFSReference";
			public const string GoodsDescription = "GoodsDescription";
			public const string OuterPacks = "OuterPacks";
			public const string OuterPacksPackType = "OuterPacksPackType";
			public const string ActualWeight = "ActualWeight";
			public const string UnitOfWeight = "UnitOfWeight";
			public const string ActualVolume = "ActualVolume";
			public const string UnitOfVolume = "UnitOfVolume";
			public const string GoodsValue = "GoodsValue";
			public const string GoodsValueCurr = "GoodsValueCurr";
			public const string InsuranceValue = "InsuranceValue";
			public const string InsuranceCurrency = "InsuranceCurrency";
			public const string ShipperCODAmount = "ShipperCODAmount";
			public const string ShipperCODPayMethod = "ShipperCODPayMethod";
			public const string A_RCV = "A_RCV";
			public const string A_BKD = "A_BKD";
			public const string INCO = "INCO";
			public const string MarksAndNumbers = "MarksAndNumbers";
			public const string IsCancelled = "IsCancelled";
			public const string AdditionalTerms = "AdditionalTerms";

			public const string OrderItemsAsString = "OrderItemsAsString";
			public const string EstimatedPickup = "EstimatedPickup";
			public const string PickupRequiredBy = "PickupRequiredBy";
			public const string FCLPickupEquipmentNeeded = "FCLPickupEquipmentNeeded";
			public const string EstimatedDelivery = "EstimatedDelivery";
			public const string DeliveryRequiredBy = "DeliveryRequiredBy";
			public const string FCLDeliveryEquipmentNeeded = "FCLDeliveryEquipmentNeeded";
			public const string DeliveryCartageCompleted = "DeliveryCartageCompleted";

			public const string Vessel = "Vessel";
			public const string VoyageFlightWithSuppression = "VoyageFlightWithSuppression";
			public const string ETDWithSuppression = "ETDWithSuppression";
			public const string ETAWithSuppression = "ETAWithSuppression";
			public const string MAWBNumber = "MAWBNumber";
			public const string ConsigneeFullName = "ConsigneeFullName";
			public const string ConsignorFullName = "ConsignorFullName";
			public const string DepotCutOff = "DepotCutOff";

			public const string BookingPK = "BookingPK";
			public const string ChargesApply = "ChargesApply";
			public const string ReleaseType = "ReleaseType";
			public const string OnBoard = "OnBoard";

			public const string DeliveryAgentFullName = "DeliveryAgentFullName";
			public const string PickupAgentFullName = "PickupAgentFullName";

			public const string VolumeWithUnits = "VolumeWithUnits";
			public const string WeightWithUnits = "WeightWithUnits";
		}

		#endregion

		#region Wrapped

		#region Wrapped QuotedBooking properties

		#region Origin

		[List("ReceivalLocations")]
		public RefUNLOCO OriginUNLOCO
		{
			get { return QuotedBooking.OriginUNLOCO; }
		}

		[List("ReceivalLocations")]
		public ZString Origin
		{
			get
			{
				if (QuotedBooking.Origin.EndsWith("ZZZ"))
				{
					QuotedBooking.Origin = ZString.Empty;
				}
				return QuotedBooking.Origin;
			}
			set
			{
				QuotedBooking.Origin = value;
				SetIsDomesticFreight();
			}
		}

		public ZString GetClosestPortOrCountryCode(JobDocAddress docAddress)
		{
			ZString result = (new PortLoader()).GetClosestPortCode(docAddress);
			if (result.IsEmpty)
			{
				if (docAddress.Country != null)
				{
					result = docAddress.Country.RN_Code;
				}
			}
			return result;
		}

		public ZPropertyInfo OriginInfo
		{
			get { return QuotedBooking == null ? null : GetWrappedZPropertyInfo(Schema.Origin, x => QuotedBooking.OriginInfo); }
		}

		public RefUNLOCOCollection ReceivalLocations
		{
			get { return QuotedBooking.ReceivalLocations; }
		}

		#endregion

		#region Destination

		[List("DeliveryLocations")]
		public RefUNLOCO DestinationUNLOCO
		{
			get { return QuotedBooking.DestinationUNLOCO; }
		}

		[List("DeliveryLocations")]
		public ZString Destination
		{
			get { return QuotedBooking.Destination; }
			set
			{
				QuotedBooking.Destination = value;
				SetIsDomesticFreight();
			}
		}

		public ZPropertyInfo DestinationInfo
		{
			get { return QuotedBooking == null ? null : GetWrappedZPropertyInfo(Schema.Destination, x => QuotedBooking.DestinationInfo); }
		}

		public RefUNLOCOCollection DeliveryLocations
		{
			get { return QuotedBooking.DeliveryLocations; }
		}

		#endregion

		#region Mode

		[BusinessObjectTestExclude]
		[List("Modes")]
		public ZString Mode
		{
			get { return QuotedBooking.Mode; }
			set { QuotedBooking.Mode = value; }
		}

		public ZPropertyInfo ModeInfo
		{
			get { return QuotedBooking == null ? null : GetWrappedZPropertyInfo(Schema.Mode, x => QuotedBooking.ModeInfo); }
		}

		#endregion

		#region IsDomesticFreight

		public ZBool IsDomesticFreight
		{
			get { return QuotedBooking.IsDomesticFreight; }
			set { QuotedBooking.IsDomesticFreight = value; }
		}

		public ZPropertyInfo IsDomesticFreightInfo
		{
			get { return QuotedBooking == null ? null : GetWrappedZPropertyInfo(Schema.IsDomesticFreight, x => QuotedBooking.IsDomesticFreightInfo); }
		}

		#endregion

		#region ServiceLevel

		[List("ServiceLevels")]
		public ZString ServiceLevel
		{
			get { return QuotedBooking.ServiceLevel; }
			set { QuotedBooking.ServiceLevel = value; }
		}

		public ZPropertyInfo ServiceLevelInfo
		{
			get { return QuotedBooking == null ? null : GetWrappedZPropertyInfo(Schema.ServiceLevel, x => QuotedBooking.ServiceLevelInfo); }
		}

		#endregion

		#region ChargesApply

		[List("ChargesApply_List")]
		public ZString ChargesApply
		{
			get { return QuotedBooking.HBLAWBChargesDisplay; }
			set { QuotedBooking.HBLAWBChargesDisplay = value; }
		}

		public ZPropertyInfo ChargesApplyInfo
		{
			get { return QuotedBooking == null ? null : GetWrappedZPropertyInfo(Schema.ChargesApply, x => QuotedBooking.HBLAWBChargesDisplayInfo); }
		}

		#endregion

		#endregion

		#region Wrapped Booking properties

		#region AttachedOrders

		public OrderCollection AttachedOrders
		{
			get
			{
				if (Booking != null)
				{
					return Booking.AttachedOrders;
				}
				ZQuery noResultQuery = new ZQuery();
				noResultQuery.IsNoResultQuery = true;
				return new OrderCollection(Factory, noResultQuery);
			}
		}

		#endregion

		#region AvailableOrders

		public OrderCollection AvailableOrders
		{
			get
			{
				OrderCollection result = null;
				if (Booking != null)
				{
					result = TrackingOrder.GetOrdersToBeAttachedForWebModule(Factory, Booking);
					var additionalFilter = GetAvailableOrdersAdditionalFilter();
					ApplyAdditionalLoggedInUserFilter(typeof(Order), additionalFilter);
					result.AdditionalFilter.AddToFilter(additionalFilter);
					if (CachedAvailableOrdersFilter == null)
					{
						CachedAvailableOrdersFilter = result.CompleteFilter;
					}
					if (result.CompleteFilter.LiteralTextSqlFormatted != CachedAvailableOrdersFilter.LiteralTextSqlFormatted)
					{
						CachedAvailableOrdersFilter = result.CompleteFilter;
						result.RefreshFromDb();
					}
				}
				return result;
			}
		}

		ZQuery GetAvailableOrdersAdditionalFilter()
		{
			var filter = OrgRestrictionFilterFactory.Instance.GetFilter(typeof(TrackingOrder));
			filter.AddToFilter(JobOrderHeaderSchema.JD_OrderStatus, SQLComparisonOperator.NotEqual, Constants.OrderStatus.Cancelled);

			return filter;
		}

		ZQuery CachedAvailableOrdersFilter;

		#endregion

		#region CustomsEntryNumber

		public ZString CustomsEntryNumber
		{
			get { return Booking.CustomsEntryNumber; }
			set { Booking.CustomsEntryNumber = value; }
		}

		public ZPropertyInfo CustomsEntryNumberInfo
		{
			get { return Booking == null ? null : GetWrappedZPropertyInfo(Schema.CustomsEntryNumber, x => Booking.CustomsEntryNumberInfo); }
		}

		#endregion

		#region UniqueConsignRef

		public ZString UniqueConsignRef
		{
			get { return Booking.JS_UniqueConsignRef; }
			set { Booking.JS_UniqueConsignRef = value; }
		}

		public ZPropertyInfo UniqueConsignRefInfo
		{
			get { return Booking == null ? null : GetWrappedZPropertyInfo(Schema.UniqueConsignRef, x => Booking.JS_UniqueConsignRefInfo); }
		}

		#endregion

		#region BookingReference

		public ZString BookingReference
		{
			get { return Booking.JS_BookingReference; }
			set { Booking.JS_BookingReference = value; }
		}

		public ZPropertyInfo BookingReferenceInfo
		{
			get { return Booking == null ? null : GetWrappedZPropertyInfo(Schema.BookingReference, x => Booking.JS_BookingReferenceInfo); }
		}

		#endregion

		#region CFSReference

		public ZString CFSReference
		{
			get { return Booking.JS_CFSReference; }
			set { Booking.JS_CFSReference = value; }
		}

		public ZPropertyInfo CFSReferenceInfo
		{
			get { return Booking == null ? null : GetWrappedZPropertyInfo(Schema.CFSReference, x => Booking.JS_CFSReferenceInfo); }
		}

		#endregion

		#region GoodsDescription

		public ZString GoodsDescription
		{
			get { return Booking.JS_GoodsDescription; }
			set { Booking.JS_GoodsDescription = value; }
		}

		public ZPropertyInfo GoodsDescriptionInfo
		{
			get { return Booking == null ? null : GetWrappedZPropertyInfo(Schema.GoodsDescription, x => Booking.JS_GoodsDescriptionInfo); }
		}

		#endregion

		#region AttachedOrderLinks

		public TrackingBookingOrderLinkCollection AttachedOrderLinks
		{
			get
			{
				if (attachedOrderLinks == null)
				{
					attachedOrderLinks = new TrackingBookingOrderLinkCollection(this);
					foreach (Order attachedOrder in AttachedOrders)
					{
						attachedOrderLinks.Add(new TrackingBookingOrderLink(this, attachedOrder.PK));
					}
					RegisterEditableChildObject(attachedOrderLinks);
				}
				return attachedOrderLinks;
			}
		}
		TrackingBookingOrderLinkCollection attachedOrderLinks;

		#endregion

		#region OuterPacks

		public ZInt OuterPacks
		{
			get { return Booking.JS_OuterPacks; }
			set { Booking.JS_OuterPacks = value; }
		}

		public ZPropertyInfo OuterPacksInfo
		{
			get { return Booking == null ? null : GetWrappedZPropertyInfo(Schema.OuterPacks, x => Booking.JS_OuterPacksInfo); }
		}

		#endregion

		#region OuterPacksPackType

		[List("PackType_List")]
		public ZString OuterPacksPackType
		{
			get { return Booking.JS_F3_NKPackType; }
			set { Booking.JS_F3_NKPackType = value; }
		}

		public ZPropertyInfo OuterPacksPackTypeInfo
		{
			get { return Booking == null ? null : GetWrappedZPropertyInfo(Schema.OuterPacksPackType, x => Booking.JS_F3_NKPackTypeInfo); }
		}

		#endregion

		#region ActualWeight

		public ZDecimal ActualWeight
		{
			get { return Booking.JS_ActualWeight; }
			set { Booking.JS_ActualWeight = value; }
		}

		public ZPropertyInfo ActualWeightInfo
		{
			get { return Booking == null ? null : GetWrappedZPropertyInfo(Schema.ActualWeight, x => Booking.JS_ActualWeightInfo); }
		}

		#endregion

		#region UnitOfWeight

		[List("UnitOfWeightList")]
		public ZString UnitOfWeight
		{
			get { return Booking.JS_UnitOfWeight; }
			set { Booking.JS_UnitOfWeight = value; }
		}

		public ZPropertyInfo UnitOfWeightInfo
		{
			get { return Booking == null ? null : GetWrappedZPropertyInfo(Schema.UnitOfWeight, x => Booking.JS_UnitOfWeightInfo); }
		}

		#endregion

		#region ActualVolume

		public ZDecimal ActualVolume
		{
			get { return Booking.JS_ActualVolume; }
			set { Booking.JS_ActualVolume = value; }
		}

		public ZPropertyInfo ActualVolumeInfo
		{
			get { return Booking == null ? null : GetWrappedZPropertyInfo(Schema.ActualVolume, x => Booking.JS_ActualVolumeInfo); }
		}

		#endregion

		#region UnitOfVolume

		[List("UnitOfVolumeList")]
		public ZString UnitOfVolume
		{
			get { return Booking.JS_UnitOfVolume; }
			set { Booking.JS_UnitOfVolume = value; }
		}

		public ZPropertyInfo UnitOfVolumeInfo
		{
			get { return Booking == null ? null : GetWrappedZPropertyInfo(Schema.UnitOfVolume, x => Booking.JS_UnitOfVolumeInfo); }
		}

		#endregion

		#region VolumeWithUnits

		public ZString VolumeWithUnits
		{
			get
			{
				if (Booking != null)
				{
					var roundedDecimal = Booking.GetRoundedValue(JobShipmentSchema.JS_ActualVolume, Booking.JS_ActualVolumeInfo, Booking.JS_ActualVolume);
					var formattedDecimal = FormatNumber(roundedDecimal, DefaultNumberOfDecimalsSupporterHelperForFreight.GetDefaultNumberOfDecimalsMetaDataProperty(Booking, Booking.JS_ActualVolumeInfo.PropertyDescriptor));
					return string.Format("{0} {1}", formattedDecimal, UnitOfVolume);
				}
				return ZString.Empty;
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
				if (Booking != null)
				{
					var roundedDecimal = Booking.GetRoundedValue(JobShipmentSchema.JS_ActualWeight,	Booking.JS_ActualWeightInfo, Booking.JS_ActualWeight);
					var formattedDecimal = FormatNumber(roundedDecimal, DefaultNumberOfDecimalsSupporterHelperForFreight.GetDefaultNumberOfDecimalsMetaDataProperty(Booking, Booking.JS_ActualWeightInfo.PropertyDescriptor));
					return string.Format("{0} {1}", formattedDecimal, UnitOfWeight);
				}
				return ZString.Empty;
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

		#region GoodsValue

		public ZDecimal GoodsValue
		{
			get { return Booking.JS_GoodsValue; }
			set { Booking.JS_GoodsValue = value; }
		}

		public ZPropertyInfo GoodsValueInfo
		{
			get { return Booking == null ? null : GetWrappedZPropertyInfo(Schema.GoodsValue, x => Booking.JS_GoodsValueInfo); }
		}

		#endregion

		#region GoodsValueCurr

		[List("RefCurrency_List")]
		public ZString GoodsValueCurr
		{
			get { return Booking.JS_RX_NKGoodsValueCurr; }
			set { Booking.JS_RX_NKGoodsValueCurr = value; }
		}

		public ZPropertyInfo GoodsValueCurrInfo
		{
			get { return Booking == null ? null : GetWrappedZPropertyInfo(Schema.GoodsValueCurr, x => Booking.JS_RX_NKGoodsValueCurrInfo); }
		}

		#endregion

		#region ReleaseType

		[List("ReleaseType_List")]
		public ZString ReleaseType
		{
			get { return Booking.JS_ReleaseType; }
			set { Booking.JS_ReleaseType = value; }
		}

		public ZPropertyInfo ReleaseTypeInfo
		{
			get { return Booking == null ? null : GetWrappedZPropertyInfo(Schema.ReleaseType, x => Booking.JS_ReleaseTypeInfo); }
		}

		#endregion

		#region OnBoard

		[List("OnBoard_List")]
		public ZString OnBoard
		{
			get { return Booking.JS_ShippedOnBoard; }
			set { Booking.JS_ShippedOnBoard = value; }
		}

		public ZPropertyInfo OnBoardInfo
		{
			get { return Booking == null ? null : GetWrappedZPropertyInfo(Schema.OnBoard, x => Booking.JS_ShippedOnBoardInfo); }
		}

		#endregion

		#region InsuranceValue

		public ZDecimal InsuranceValue
		{
			get { return Booking.JS_InsuranceValue; }
			set { Booking.JS_InsuranceValue = value; }
		}

		public ZPropertyInfo InsuranceValueInfo
		{
			get { return Booking == null ? null : GetWrappedZPropertyInfo(Schema.InsuranceValue, x => Booking.JS_InsuranceValueInfo); }
		}

		#endregion

		#region InsuranceCurrency

		[List("RefCurrency_List")]
		public ZString InsuranceCurrency
		{
			get { return Booking.JS_RX_NKInsuranceCurrency; }
			set { Booking.JS_RX_NKInsuranceCurrency = value; }
		}

		public ZPropertyInfo InsuranceCurrencyInfo
		{
			get { return Booking == null ? null : GetWrappedZPropertyInfo(Schema.InsuranceCurrency, x => Booking.JS_RX_NKInsuranceCurrencyInfo); }
		}

		#endregion

		#region ShipperCODAmount

		public ZDecimal ShipperCODAmount
		{
			get { return Booking.JS_ShipperCODAmount; }
			set { Booking.JS_ShipperCODAmount = value; }
		}

		public ZPropertyInfo ShipperCODAmountInfo
		{
			get { return Booking == null ? null : GetWrappedZPropertyInfo(Schema.ShipperCODAmount, x => Booking.JS_ShipperCODAmountInfo); }
		}

		#endregion

		#region ShipperCODPayMethod

		[List("ShipperCODPaymentTypes")]
		public ZString ShipperCODPayMethod
		{
			get { return Booking.JS_ShipperCODPayMethod; }
			set { Booking.JS_ShipperCODPayMethod = value; }
		}

		public ZPropertyInfo ShipperCODPayMethodInfo
		{
			get { return Booking == null ? null : GetWrappedZPropertyInfo(Schema.ShipperCODPayMethod, x => Booking.JS_ShipperCODPayMethodInfo); }
		}

		#endregion

		#region A_RCV

		public ZDateTime A_RCV
		{
			get { return Booking.JS_A_RCV; }
			set { Booking.JS_A_RCV = value; }
		}

		public ZPropertyInfo A_RCVInfo
		{
			get { return Booking == null ? null : GetWrappedZPropertyInfo(Schema.A_RCV, x => Booking.JS_A_RCVInfo); }
		}

		#endregion

		#region A_RCV

		public ZDateTime A_BKD
		{
			get { return Booking.JS_A_BKD; }
			set { Booking.JS_A_BKD = value; }
		}

		public ZPropertyInfo A_BKDInfo
		{
			get { return Booking == null ? null : GetWrappedZPropertyInfo(Schema.A_BKD, x => Booking.JS_A_BKDInfo); }
		}

		#endregion

		#region INCO

		[List("IncoTerms")]
		public ZString INCO
		{
			get { return Booking.JS_INCO; }
			set { Booking.JS_INCO = value; }
		}

		public ZPropertyInfo INCOInfo
		{
			get { return Booking == null ? null : GetWrappedZPropertyInfo(Schema.INCO, x => Booking.JS_INCOInfo); }
		}

		#endregion

		#region Additional Terms

		public ZString AdditionalTerms
		{
			get { return Booking.JS_AdditionalTerms; }
			set { Booking.JS_AdditionalTerms = value; }
		}

		public ZPropertyInfo AdditionalTermsInfo
		{
			get { return Booking == null ? null : GetWrappedZPropertyInfo(Schema.AdditionalTerms, x => Booking.JS_AdditionalTermsInfo); }
		}

		#endregion

		#region MarksAndNumbers

		public ZString MarksAndNumbers
		{
			get { return Booking.JS_MarksAndNumbers; }
			set { Booking.JS_MarksAndNumbers = value; }
		}

		public ZPropertyInfo MarksAndNumbersInfo
		{
			get { return Booking == null ? null : GetWrappedZPropertyInfo(Schema.MarksAndNumbers, x => Booking.JS_MarksAndNumbersInfo); }
		}

		#endregion

		#region IsCancelled

		public ZBool IsCancelled
		{
			get { return Booking.JS_IsCancelled; }
			set { Booking.JS_IsCancelled = value; }
		}

		public ZPropertyInfo IsCancelledInfo
		{
			get { return Booking == null ? null : GetWrappedZPropertyInfo(Schema.IsCancelled, x => Booking.JS_IsCancelledInfo); }
		}

		#endregion

		#endregion

		#region Wrapped properties of Booking.DocsAndCartage

		#region OrderItemsAsString

		[BusinessObjectTestExclude]
		public ZString OrderItemsAsString
		{
			get
			{
				ZString result = Booking.DocsAndCartage.JP_OrderItemsAsString;

				if (result.IsEmpty && Booking.AttachedOrders.Count > 0)
				{
					foreach (Order attachedOrder in Booking.AttachedOrders)
					{
						if (!result.IsEmpty && !attachedOrder.JD_OrderNumberAndSplit.IsEmpty)
						{
							result += ",";
						}
						result += attachedOrder.JD_OrderNumberAndSplit;
					}
				}
				if (!result.IsEmpty)
				{
					result = result.Replace(",", ", ");
					result = result.Replace("  ", " ");
				}
				return result;
			}
			set { Booking.DocsAndCartage.JP_OrderItemsAsString = value; }
		}

		public ZPropertyInfo OrderItemsAsStringInfo
		{
			get { return Booking == null ? null : GetWrappedZPropertyInfo(Schema.OrderItemsAsString, x => Booking.DocsAndCartage.JP_OrderItemsAsStringInfo); }
		}

		#endregion

		#region EstimatedPickup

		public ZDateTime EstimatedPickup
		{
			get { return Booking.DocsAndCartage.JP_EstimatedPickup; }
			set { Booking.DocsAndCartage.JP_EstimatedPickup = value; }
		}

		public ZPropertyInfo EstimatedPickupInfo
		{
			get { return Booking == null ? null : GetWrappedZPropertyInfo(Schema.EstimatedPickup, x => Booking.DocsAndCartage.JP_EstimatedPickupInfo); }
		}

		#endregion

		#region PickupRequiredBy

		public ZDateTime PickupRequiredBy
		{
			get { return Booking.DocsAndCartage.JP_PickupRequiredBy; }
			set { Booking.DocsAndCartage.JP_PickupRequiredBy = value; }
		}

		public ZPropertyInfo PickupRequiredByInfo
		{
			get { return Booking == null ? null : GetWrappedZPropertyInfo(Schema.PickupRequiredBy, x => Booking.DocsAndCartage.JP_PickupRequiredByInfo); }
		}

		#endregion

		#region FCLPickupEquipmentNeeded

		[List("PickupEquipmentNeededList")]
		public ZString FCLPickupEquipmentNeeded
		{
			get { return Booking.DocsAndCartage.JP_FCLPickupEquipmentNeeded; }
			set { Booking.DocsAndCartage.JP_FCLPickupEquipmentNeeded = value; }
		}

		public ZPropertyInfo FCLPickupEquipmentNeededInfo
		{
			get { return Booking == null ? null : GetWrappedZPropertyInfo(Schema.FCLPickupEquipmentNeeded, x => Booking.DocsAndCartage.JP_FCLPickupEquipmentNeededInfo); }
		}

		#endregion

		#region EstimatedDelivery

		public ZDateTime EstimatedDelivery
		{
			get { return Booking.DocsAndCartage.JP_EstimatedDelivery; }
			set { Booking.DocsAndCartage.JP_EstimatedDelivery = value; }
		}

		public ZPropertyInfo EstimatedDeliveryInfo
		{
			get { return Booking == null ? null : GetWrappedZPropertyInfo(Schema.EstimatedDelivery, x => Booking.DocsAndCartage.JP_EstimatedDeliveryInfo); }
		}

		#endregion

		#region DeliveryRequiredBy

		public ZDateTime DeliveryRequiredBy
		{
			get { return Booking.DocsAndCartage.JP_DeliveryRequiredBy; }
			set { Booking.DocsAndCartage.JP_DeliveryRequiredBy = value; }
		}

		public ZPropertyInfo DeliveryRequiredByInfo
		{
			get { return Booking == null ? null : GetWrappedZPropertyInfo(Schema.DeliveryRequiredBy, x => Booking.DocsAndCartage.JP_DeliveryRequiredByInfo); }
		}

		#endregion

		#region FCLDeliveryEquipmentNeeded

		[List("DeliveryEquipmentNeededList")]
		public ZString FCLDeliveryEquipmentNeeded
		{
			get { return Booking.DocsAndCartage.JP_FCLDeliveryEquipmentNeeded; }
			set { Booking.DocsAndCartage.JP_FCLDeliveryEquipmentNeeded = value; }
		}

		public ZPropertyInfo FCLDeliveryEquipmentNeededInfo
		{
			get { return Booking == null ? null : GetWrappedZPropertyInfo(Schema.FCLDeliveryEquipmentNeeded, x => Booking.DocsAndCartage.JP_FCLDeliveryEquipmentNeededInfo); }
		}

		#endregion

		#region DeliveryCartageCompleted

		public ZDateTime DeliveryCartageCompleted
		{
			get { return Booking.DocsAndCartage.JP_DeliveryCartageCompleted; }
			set { Booking.DocsAndCartage.JP_DeliveryCartageCompleted = value; }
		}

		public ZPropertyInfo DeliveryCartageCompletedInfo
		{
			get { return Booking == null ? null : GetWrappedZPropertyInfo(Schema.DeliveryCartageCompleted, x => Booking.DocsAndCartage.JP_DeliveryCartageCompletedInfo); }
		}

		#endregion

		#region MAWBNumber

		public ZString MAWBNumber
		{
			get
			{
				ZString result = ZString.Empty;
				if (Booking != null)
				{
					if (Booking.JS_IsDirectBooking)
					{
						result = Booking.JS_HouseBill;
					}
				}
				return result;
			}
		}

		public ZPropertyInfo MAWBNumberInfo
		{
			get { return GetZPropertyInfo(Schema.MAWBNumber); }
		}

		#endregion

		#region ConsigneeFullName

		public ZString ConsigneeFullName
		{
			get
			{
				ZString result = ZString.Empty;
				if (Booking != null && Booking.Consignee != null)
				{
					result = Booking.Consignee.OH_FullNameTruncated;
				}
				return result;
			}
		}

		public ZPropertyInfo ConsigneeFullNameInfo
		{
			get { return GetZPropertyInfo(Schema.ConsigneeFullName); }
		}

		#endregion

		#region ConsignorFullName

		public ZString ConsignorFullName
		{
			get
			{
				ZString result = ZString.Empty;
				if (Booking != null && Booking.Consignor != null)
				{
					result = Booking.Consignor.OH_FullNameTruncated;
				}
				return result;
			}
		}

		public ZPropertyInfo ConsignorFullNameInfo
		{
			get { return GetZPropertyInfo(Schema.ConsignorFullName); }
		}

		#endregion

		#region DepotCutOff

		public ZDateTime DepotCutOff
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;
				if (Booking != null && Booking.Sailing != null)
				{
					result = Booking.Sailing.JX_DepotCutOff;
				}
				return result;
			}
		}

		public ZPropertyInfo DepotCutOffInfo
		{
			get { return GetZPropertyInfo(Schema.DepotCutOff); }
		}

		#endregion

		#region DeliveryAgentFullName

		public ZString DeliveryAgentFullName
		{
			get
			{
				ZString result = ZString.Empty;
				if (Booking != null && Booking.DeliveryAgent != null)
				{
					result = Booking.DeliveryAgent.OH_FullNameTruncated;
				}
				return result;
			}
		}

		public ZPropertyInfo DeliveryAgentFullNameInfo
		{
			get { return GetZPropertyInfo(Schema.DeliveryAgentFullName); }
		}

		#endregion

		#region PickupAgentFullName

		public ZString PickupAgentFullName
		{
			get
			{
				ZString result = ZString.Empty;
				if (Booking != null && Booking.PickupAgent != null)
				{
					result = Booking.PickupAgent.OH_FullNameTruncated;
				}
				return result;
			}
		}

		public ZPropertyInfo PickupAgentFullNameInfo
		{
			get { return GetZPropertyInfo(Schema.PickupAgentFullName); }
		}

		#endregion

		#endregion

		#endregion

		#region Properties With Suppression

		#region Vessel

		public ZString Vessel
		{
			get { return Booking.JS_Calc_CurrentVessel; }
		}

		public ZPropertyInfo VesselInfo
		{
			get { return GetZPropertyInfo(Schema.Vessel); }
		}

		#endregion

		#region VoyageFlightWithSuppression

		public ZString VoyageFlightWithSuppression
		{
			get { return Suppression.GetWebValue(Booking.JS_Calc_CurrentVoyageFlight, Booking, SuppressFields.FlightNumber); }
		}

		public ZPropertyInfo VoyageFlightWithSuppressionInfo
		{
			get { return GetZPropertyInfo(Schema.VoyageFlightWithSuppression); }
		}

		#endregion

		#region ETDWithSuppression

		public ZDateTime ETDWithSuppression
		{
			get { return Suppression.GetWebValue(Booking.JS_E_DEP, Booking, SuppressFields.ETD); }
		}

		public ZPropertyInfo ETDWithSuppressionInfo
		{
			get { return GetZPropertyInfo(Schema.ETDWithSuppression); }
		}

		#endregion

		#region ETAWithSuppression

		public ZDateTime ETAWithSuppression
		{
			get { return Suppression.GetWebValue(Booking.JS_E_ARV, Booking, SuppressFields.ETA); }
		}

		public ZPropertyInfo ETAWithSuppressionInfo
		{
			get { return GetZPropertyInfo(Schema.ETAWithSuppression); }
		}

		#endregion

		#endregion

		#region Events

		void SetIsDomesticFreight()
		{
			bool cachedValue = IsDomesticFreight;
			if (Booking != null)
			{
				IsDomesticFreight = (!Destination.IsEmpty && !Origin.IsEmpty && Destination.SubstringSafe(0, 2) == Origin.SubstringSafe(0, 2));
			}
			IsDomesticFreightInfo.RefreshBinding();
			RaiseIsDomesticFreightValueChanged();
		}

		public event EventHandler IsDomesticFreightValueChanged;

		void RaiseIsDomesticFreightValueChanged()
		{
			if (IsDomesticFreightValueChanged != null)
			{
				IsDomesticFreightValueChanged(this, EventArgs.Empty);
			}
		}

		protected void OnIsDomesticFreightInternalValueChanged(object sender, EventArgs e)
		{
			RaiseIsDomesticFreightValueChanged();
		}

		#endregion

		#region Overrides

		protected override ZString HumanReadableNameCore
		{
			get { return Res.GetString("d12f43d8-918c-4ae7-8344-d91f966cf2d7", "Booking {0}", Number); }
		}

		public override bool IsInDatabase
		{
			get { return Booking.IsInDatabase; }
		}

		public override bool ReadOnly
		{
			get { return base.ReadOnly; }
			set
			{
				base.ReadOnly = value;
				Booking.ReadOnly = value;
			}
		}

		protected override BusinessObject LogsAndNotesTarget
		{
			get { return QuotedBooking.Booking ?? (BusinessObject)QuotedBooking.Quote; }
		}

		protected override StmNoteContexts NoteContextsForRelatedNotes
		{
			get
			{
				StmNoteContexts result = new StmNoteContexts();

				result = StmNoteContextUtils.GetContextFromTransportModeImportExport(Constants.GlobalModuleNamesConstants.Forwarding, "", QuotedBooking.TransportMode, "");
				result.Module |= base.NoteContextsForRelatedNotes.Module;
				result.Direction |= base.NoteContextsForRelatedNotes.Direction;
				result.FreightMode |= base.NoteContextsForRelatedNotes.FreightMode;

				if (QuotedBooking.IsImport())
				{
					result.Direction |= StmNoteContextDirection.I;
				}

				if (QuotedBooking.IsExport())
				{
					result.Direction |= StmNoteContextDirection.E;
				}

				if (QuotedBooking.IsDomesticFreight)
				{
					result.Direction |= StmNoteContextDirection.D;
				}

				if (QuotedBooking.IsImport() || QuotedBooking.IsExport())
				{
					result.Direction |= StmNoteContextDirection.B;
				}

				StmNoteContexts baseResult = base.NoteContextsForRelatedNotes;
				result.Module = result.Module | baseResult.Module;
				result.Direction = result.Direction | baseResult.Direction;
				result.FreightMode = result.FreightMode | baseResult.FreightMode;

				return result;
			}
		}

		#endregion

		public ZGuid BookingPK
		{
			get { return Booking.PK; }
		}

		public bool IsAir
		{
			get { return Booking.IsAir; }
		}

		public bool IsFCL
		{
			get { return Booking.JS_PackingMode == Constants.ContainerModes.FCL; }
		}

		public ZString TransportMode
		{
			get { return Booking == null ? ZString.Empty : Booking.JS_TransportMode; }
		}

		public ZBool UseFormBuilderBillsOfLading => false;

		#region Related BusinessObjects

		public JobDocAddress ConsignorPickupAddress
		{
			get
			{
				if (!Booking.ConsignorPickupAddress.ShouldAlwaysUpdateSecondary)
				{
					Booking.ConsignorPickupAddress.ShouldAlwaysUpdateSecondary = true;
				}
				return Booking.ConsignorPickupAddress;
			}
		}

		public JobDocAddress ConsigneeDeliveryAddress
		{
			get
			{
				if (!Booking.ConsigneeDeliveryAddress.ShouldAlwaysUpdateSecondary)
				{
					Booking.ConsigneeDeliveryAddress.ShouldAlwaysUpdateSecondary = true;
				}
				return Booking.ConsigneeDeliveryAddress;
			}
		}

		#endregion

		#region Cnor

		public ZGuid ConsignorOrganisationPK
		{
			get { return Booking.ConsignorPickupAddress.OrganisationPK; }
			set { Booking.ConsignorPickupAddress.OrganisationPK = value; }
		}

		public ZPropertyInfo ConsignorOrganisationPKInfo
		{
			get { return Booking == null ? null : GetWrappedZPropertyInfo(nameof(ConsignorOrganisationPK), x => Booking.ConsignorPickupAddress.OrganisationPKInfo); }
		}

		#endregion

		#region Cnee

		public ZGuid ConsigneeOrganisationPK
		{
			get { return Booking.ConsigneeDeliveryAddress.OrganisationPK; }
			set { Booking.ConsigneeDeliveryAddress.OrganisationPK = value; }
		}

		public ZPropertyInfo ConsigneeOrganisationPKInfo
		{
			get { return Booking == null ? null : GetWrappedZPropertyInfo(nameof(ConsigneeOrganisationPK), x => Booking.ConsigneeDeliveryAddress.OrganisationPKInfo); }
		}

		#endregion

		#region ThirdParty

		#region Address

		public ZGuid ThirdPartyAddressPK
		{
			get
			{
				return ThirdPartyDocAddress != null ? ThirdPartyDocAddress.E2_OA_Address : fThirdPartyAddressPK;
			}
			set
			{
				if (fThirdPartyAddressPK != value || (ThirdPartyDocAddress != null && ThirdPartyDocAddress.E2_OA_Address != value))
				{
					var docAddressToRemove = Booking.DocAddresses.FindByDocAddressType(DocAddressType.ClientRequestedBillingParty);
					if (docAddressToRemove != null)
					{
						Booking.DocAddresses.RemoveAndDelete(docAddressToRemove);
					}
					fThirdPartyAddressPK = value;
					if (ThirdPartyAddress != null)
					{
						Booking.DocAddresses.AddNew(ThirdPartyAddress, DocAddressType.ClientRequestedBillingParty);
					}
					ThirdPartyAddressPKInfo.RefreshBinding();
				}
			}
		}
		ZGuid fThirdPartyAddressPK;

		public ZPropertyInfo ThirdPartyAddressPKInfo
		{
			get { return GetZPropertyInfo(Schema.ThirdPartyAddressPK); }
		}

		public OrgAddress ThirdPartyAddress
		{
			get { return Factory.Load<OrgAddress>(ThirdPartyAddressPK); }
		}

		public JobDocAddress ThirdPartyDocAddress
		{
			get
			{
				return Booking.DocAddresses.FindByDocAddressType(DocAddressType.ClientRequestedBillingParty);
			}
		}

		void ModeChanged(object sender, EventArgs e)
		{
			AttachedOrderLinks.Validate();
		}

		void ConsigneePKChanged(object sender, EventArgs e)
		{
			AttachedOrderLinks.Validate();
		}

		void JS_INCOInfo_ValueChanged(object sender, EventArgs e)
		{
			if (Booking.IsDomesticFreight && INCO != Constants.DomesticPaymentTerms.CollectThirdParty)
			{
				ThirdPartyAddressPK = ZGuid.Empty;
			}
		}

		#endregion

		#endregion

		#region Related Collections

		public TrackingBookingContainerDependentCollection Containers
		{
			get
			{
				if (containers == null)
				{
					containers = new TrackingBookingContainerDependentCollection(this);
					containers.Load();
					RegisterEditableChildObject(containers);
				}

				return containers;
			}
		}
		TrackingBookingContainerDependentCollection containers;

		public CusEntryNumAdditionalReferenceCollection AdditionalReferenceNumbers
		{
			get { return Booking.Numbers; }
		}

		public QuotedBookingContainerDependentCollection QuotedBookingContainers
		{
			get { return QuotedBooking.QuotedBookingContainers; }
		}

		public ForwardingPackLineCollection OuterPackLines
		{
			get { return Booking.OuterPackLines; }
		}

		#endregion

		#region Lookup Collections

		public virtual RefServiceLevelCollection ServiceLevels
		{
			get { return QuotedBooking.ServiceLevels; }
		}

		public virtual CodeDescriptionPairList ChargesApply_List
		{
			get { return QuotedBooking.HBLAWBChargesDisplay_List; }
		}

		public virtual ReadOnlyCodeDescriptionPairList ReleaseType_List
		{
			get { return this.Booking.Lookups.JS_ReleaseType_List; }
		}

		public virtual CodeDescriptionPairList OnBoard_List
		{
			get { return this.Booking.Lookups.JS_ShippedOnBoard_List; }
		}

		public CodeDescriptionPairList IncoTerms
		{
			get { return QuotedBooking.IncoTerms; }
		}

		public CodeDescriptionPairList Modes
		{
			get { return QuotedBooking.ModesForWebTracker; }
		}

		public CodeDescriptionPairList PackType_List
		{
			get { return Booking.Lookups.JS_PackType_List.GetAsCodeDescriptionPair(); }
		}

		public CodeDescriptionPairList UnitOfWeightList
		{
			get { return QuotedBooking.UnitOfWeightList; }
		}

		public CodeDescriptionPairList UnitOfVolumeList
		{
			get { return QuotedBooking.UnitOfVolumeList; }
		}

		public RefUNLOCOCollection RefUNLOCO_List
		{
			get { return this.Booking.Lookups.RefUNLOCO_List; }
		}

		public RefCurrencyCollection RefCurrency_List
		{
			get { return this.Booking.Lookups.RefCurrency_List; }
		}

		public ReadOnlyCodeDescriptionPairList ShipperCODPaymentTypes
		{
			get { return Booking.Lookups.ShipperCODPaymentTypes; }
		}

		public CodeDescriptionPairList PickupEquipmentNeededList
		{
			get { return Booking.DocsAndCartage.Lookups.PickupEquipmentNeededList; }
		}

		public CodeDescriptionPairList DeliveryEquipmentNeededList
		{
			get { return Booking.DocsAndCartage.Lookups.DeliveryEquipmentNeededList; }
		}

		public OrgHeaderCollection Consignee_List
		{
			get { return Booking.Lookups.Consignee_List; }
		}

		public OrgHeaderCollection Consignor_List
		{
			get { return Booking.Lookups.Consignor_List; }
		}

		#endregion

		#region LoggedInContact

		public OrgContact LoggedInContact
		{
			get { return fLoggedInContact; }
			set { fLoggedInContact = value; }
		}
		protected OrgContact fLoggedInContact;

		#endregion

		#region UserEditableNote

		public IStmNoteParent NotesParentBO
		{
			get { return QuotedBooking.Booking ?? (IStmNoteParent)QuotedBooking.Quote; }
		}

		public WebUserEditableNote UserEditableNoteHelper
		{
			get
			{
				if (fUserEditableNoteHelper == null)
				{
					fUserEditableNoteHelper = GetNewUserEditableNoteHelper();
					RegisterEditableChildObject(fUserEditableNoteHelper);
				}
				return fUserEditableNoteHelper;
			}
		}
		WebUserEditableNote fUserEditableNoteHelper;

		protected WebUserEditableNote GetNewUserEditableNoteHelper()
		{
			return new WebUserEditableNote(this, PredefinedNoteTypes.Instance.SpecialInstructions);
		}

		public WebUserEditableNote DetailedGoodsDescriptionNoteHelper
		{
			get
			{
				if (detailedGoodsDescriptionNoteHelper == null)
				{
					detailedGoodsDescriptionNoteHelper = GetNewDetailedGoodsDescriptionNoteHelper();
					RegisterEditableChildObject(detailedGoodsDescriptionNoteHelper);
				}
				return detailedGoodsDescriptionNoteHelper;
			}
		}
		WebUserEditableNote detailedGoodsDescriptionNoteHelper;

		protected WebUserEditableNote GetNewDetailedGoodsDescriptionNoteHelper()
		{
			return new WebUserEditableNote(this, PredefinedNoteTypes.Instance.DetailedGoodsDescription);
		}

		#endregion

		#region IContainerListProvider

		public RefContainerCollection Container_List
		{
			get { return new ContainerHelper(Factory).List(TransportMode); }
		}

		#endregion

		#region IEmailNotification Members

		public ZString Number
		{
			get { return Booking.JS_UniqueConsignRef; }
		}

		ZBool IBizOChangesEmailNotification.IsCancelled
		{
			get { return Booking.JS_IsCancelled; }
		}

		GlbBranch IBizOChangesEmailNotification.EventBranch
		{
			get
			{
				OrgHeader loggedInOrg = LoggedInContact != null ? LoggedInContact.ParentOrg : null;
				OrgHeader relatedOrg = ((IBizOChangesEmailNotification)this).RelatedOrg ?? loggedInOrg;

				RefUNLOCO closestPort = relatedOrg != null ? relatedOrg.ClosestPort : null;

				return
					GlbBranch.FindControllingBranchWithFallBackToAnyCompany(relatedOrg) ??
					GlbBranch.FindByHomePortWithFallBackToRelatedPort(Factory, closestPort);
			}
		}

		GuidRegistryItem IBizOChangesEmailNotification.EmailGroupRegistryItem
		{
			get { return WebDataRegistry.Instance.BookingNotificationEmailGroup; }
		}

		OrgHeader IBizOChangesEmailNotification.RelatedOrg
		{
			get
			{
				if (Booking.IsImport() && Booking.ConsigneeDeliveryAddress != null && !Booking.ConsigneeDeliveryAddress.E2_AddressOverride)
				{
					return Booking.ConsigneeDeliveryAddress.Organisation;
				}

				else if (Booking.ConsignorPickupAddress != null && !Booking.ConsignorPickupAddress.E2_AddressOverride)
				{
					return Booking.ConsignorPickupAddress.Organisation;
				}

				return null;
			}
		}

		CodeDescriptionBoolRegistryItem IBizOChangesEmailNotification.StaffRolesToNotify
		{
			get { return WebDataRegistry.Instance.BookingsNotificationStaffRoles; }
		}

		CodePairRegistryItem IBizOChangesEmailNotification.NotificationSendingRule
		{
			get { return WebDataRegistry.Instance.BookingNotificationOptions; }
		}

		ZGuid IBizOChangesEmailNotification.GetStaffGuid(OrgStaffAssignmentsCollection staffAssignments, CodeDescriptionBool role)
		{
			var mode = OrgStaffAssignmentsCollection.AirSea.None;

			if (Booking.IsAir)
			{
				mode = OrgStaffAssignmentsCollection.AirSea.Air;
			}
			else if (Booking.IsSea)
			{
				mode = OrgStaffAssignmentsCollection.AirSea.Sea;
			}
			else if (Booking.IsRail)
			{
				mode = OrgStaffAssignmentsCollection.AirSea.Rail;
			}
			else if (Booking.IsRoad)
			{
				mode = OrgStaffAssignmentsCollection.AirSea.Road;
			}
			else if (Booking.IsCourier)
			{
				mode = OrgStaffAssignmentsCollection.AirSea.Post;
			}

			var direction = OrgStaffAssignmentsCollection.Direction.None;
			direction = Booking.IsImport() ? OrgStaffAssignmentsCollection.Direction.Import : OrgStaffAssignmentsCollection.Direction.Export;

			ZString staffNK = staffAssignments.GetStaffAssignment(role.Code, direction, mode);

			GlbStaff staff = staffAssignments.Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, staffNK);
			if (staff != null)
			{
				return staff.PK;
			}
			else
			{
				return ZGuid.Empty;
			}
		}

		ControllerID IBizOChangesEmailNotification.ControllerForEnterpriseUrl
		{
			get { return ControllerIDs.QuotedBookings; }
		}

		ZGuid IBizOChangesEmailNotification.PK
		{
			get { return BookingPK; }
		}

		#region Email Reporting

		void IBizOChangesEmailNotification.AddPropertiesForEmailReporting(DataState state)
		{
			PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("143c609d-2e97-4372-bb35-b20311e23646", "Pickup"), Booking.ConsignorPickupAddress.E2_CompanyNameTruncated);
			PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("d1509dee-e84b-46e1-ab93-28f4d0f792b0", "Pickup Address"), Booking.ConsignorPickupAddress.AddressAsASingleLine);
			PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("4a7effef-0996-48cd-945b-5918a5c390ce", "Pickup Contact"), Booking.ConsignorPickupAddress.E2_Contact);
			PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("fa8f0543-4791-4d9e-b965-625447cd2102", "Origin"), Booking.JS_RL_NKOrigin);

			PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("1a4798b1-f5d1-474b-8afe-4133c3355843", "Delivery"), Booking.ConsigneeDeliveryAddress.E2_CompanyNameTruncated);
			PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("679dbbf4-87ae-46ae-98c4-7c80efc07b60", "Delivery Address"), Booking.ConsigneeDeliveryAddress.AddressAsASingleLine);
			PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("8987e652-cf4d-4c14-b0a0-b4b42da4a5b0", "Delivery Contact"), Booking.ConsigneeDeliveryAddress.E2_Contact);
			PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("970aa21d-dee8-4441-a179-7e8c4c45f518", "Destination"), Booking.JS_RL_NKDestination);

			PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("6c61f4ab-4a8f-4229-9bbb-bf3ee64e0652", "Mode"), Mode, Modes);
			PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("27BEED77-5820-4FC8-A954-6A99B3E3440F", "Shipper's Ref#"), Booking.JS_BookingReference);
			PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("23A50605-F753-47B9-A26E-F6E1F78A3CFE", "Order Ref#"), Booking.DocsAndCartage.JP_OrderItemsAsString);
			PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("dc6b2af5-f1c3-4a45-a805-a96c5d2a2f8f", "Description"), Booking.JS_GoodsDescription);

			PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("b750d8dc-ec2a-456c-a03c-d31ecd3f8530", "Packs"), Booking.JS_OuterPacks, Booking.JS_F3_NKPackType, Booking.Lookups.JS_PackType_List.GetAsCodeDescriptionPair());
			PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("beec5843-ff0d-47b6-9645-1189af663766", "Weight"), Booking.JS_ActualWeight, Booking.JS_UnitOfWeight, Booking.Lookups.JS_UnitOfWeight_List);
			PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("53d24810-cc8e-4f7b-b53e-d4971ec886c4", "Volume"), Booking.JS_ActualVolume, Booking.JS_UnitOfVolume, Booking.Lookups.JS_UnitOfVolume_List);

			PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("7b43961e-ad47-43d5-8314-c6584ebe5b62", "Shipper COD Amount"), Booking.JS_ShipperCODAmount, Booking.JS_ShipperCODPayMethod, Booking.Lookups.ShipperCODPaymentTypes);
			PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("c7f18a28-3945-44cf-b8cc-4423c4ffd6dd", "Goods Value"), ZString.Format("{0} {1}", Booking.JS_GoodsValue, Booking.JS_RX_NKGoodsValueCurr));
			PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("da2dd83d-7e51-4585-ad4f-697d37264133", "Insurance Value"), ZString.Format("{0} {1}", Booking.JS_InsuranceValue, Booking.JS_RX_NKInsuranceCurrency));

			PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("b4201e27-bdf6-4a13-a463-b2dca2b751f4", "Warehouse Rec."), Booking.JS_A_RCV);

			PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("69149b31-b705-4c02-9099-e53187cec62f", "Estimated Pickup"), Booking.DocsAndCartage.JP_EstimatedPickup);
			PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("4fddec4a-5c1d-4100-b55a-5b82d1231153", "Pickup Required By"), Booking.DocsAndCartage.JP_PickupRequiredBy);
			PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("91dbe810-c8bb-4860-b98e-cc79e5324a19", "Pickup Equipment"), Booking.DocsAndCartage.JP_FCLPickupEquipmentNeeded);

			PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("66b6a33f-0c1a-490b-a4e8-cae9b36e8751", "Estimated Delivery"), Booking.DocsAndCartage.JP_EstimatedDelivery);
			PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("cc931afe-f44b-4d77-94a2-0fa1a8b90e0e", "Delivery Required By"), Booking.DocsAndCartage.JP_DeliveryRequiredBy);
			PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("4d2f2ad7-a187-4796-ac54-8bd3a3b56047", "Port Transport Drop Mode"), Booking.DocsAndCartage.JP_FCLDeliveryEquipmentNeeded);

			PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("1819b8a9-7eed-4458-9e5c-d906379c6c57", "Service Level"), Booking.JS_RS_NKServiceLevel);
			if (Booking.IsDomesticFreight)
			{
				PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("e558863b-8514-49e2-b199-e7f068551998", "Payment Term"), Booking.JS_INCO);
			}
			else
			{
				PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("304e4b76-fa2c-9e85-4657-9b992c51edd3", "Incoterm"), Booking.JS_INCO);
				PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("7d825433-5db5-464a-800f-3355aa7c1735", "Additional Terms"), Booking.JS_AdditionalTerms);
			}

			PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("ce333fe3-07a7-404c-aa57-bdf7622e184c", "Customs #"), Booking.CustomsEntryNumber);
			PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("cc47641e-7bd0-4648-a2a4-0eb21868f31a", "Marks & Numbers"), Booking.JS_MarksAndNumbers);

			if (ThirdPartyDocAddress != null)
			{
				PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("35cebb71-12f4-4c6a-b288-6f2a7f86c1fe", "Requested Billing Party"), ThirdPartyDocAddress.AddressAsASingleLine);
			}

			if (IsFCL)
			{
				PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("63a3f1f0-2d26-4279-a32a-0870ce7daa6e", "Carrier"), Booking.BookedShippingLine != null ? Booking.BookedShippingLine.OH_FullNameTruncated : ZString.Empty);
			}

			AddContainersForEmailReporting(state, Containers);
			AddPackLinesForEmailReporting(state, Booking.OuterPackLines);
			DocumentHelper.AddDocumentsForEmailReporting(state, PropertiesForEmailReporting);
			Milestones.AddForEmailReporting(state, PropertiesForEmailReporting);
		}

		public void AddContainersForEmailReporting(DataState state, TrackingBookingContainerDependentCollection collection)
		{
			int i = 1;
			foreach (ForwardingContainer container in collection)
			{
				var value = GenerateContainerDetailsForEmailReporting(container);

				var propertyName = ResString.GetMultilingualString("809d92c4-1375-4fe8-b140-f93082b289e1", "Container {0}", i++);

				PropertiesForEmailReporting.Add(state, propertyName, value);
			}
		}

		protected MultilingualString GenerateContainerDetailsForEmailReporting(ForwardingContainer container)
		{
			return MultilingualString.Join(System.Environment.NewLine,
				ResString.GetMultilingualString("9ce56572-92bf-4093-802f-9fde5a459e7d", "Container #: {0}", container.JC_ContainerNum),
				ResString.GetMultilingualString("7e9c0dc3-4ae8-49c2-87c9-ffe0cc3d668d", "Type: {0}", container.RefContainer == null ? UNKNOWN : container.RefContainer.RC_Code),
				ResString.GetMultilingualString("e371235e-3418-4b54-b91c-aace4a95978f", "Count: {0}", container.JC_ContainerCount));
		}

		ZString UNKNOWN
		{
			get { return Res.GetString("3c6757cc-a70f-4b36-adf6-a7e6065d263f", "UNKNOWN"); }
		}

		public void AddPackLinesForEmailReporting(DataState state, ForwardingPackLineCollection collection)
		{
			int i = 1;
			foreach (ForwardingPackLine line in collection)
			{
				var value = GeneratePackLineDetailsForEmailReporting(line);

				var propertyName = ResString.GetMultilingualString("c19a31f9-7a30-4b91-b9a4-334a928b6dd8", "Pack Line {0}", i++);

				PropertiesForEmailReporting.Add(state, propertyName, value);
			}
		}

		protected MultilingualString GeneratePackLineDetailsForEmailReporting(ForwardingPackLine packLine)
		{
			return MultilingualString.Join(System.Environment.NewLine,
				ResString.GetMultilingualString("60950dc1-c5f8-488c-8a0b-34576211edce", "Packs: {0}", packLine.JL_PackageCount),
				ResString.GetMultilingualString("00d4c1f5-8f9a-4a1c-b05c-b709051c8102", "Pack Type: {0}", packLine.JL_F3_NKPackType),
				ResString.GetMultilingualString("d90a3205-8744-45ab-84d2-4463f2448932", "Weight: {0} {1}", packLine.JL_ActualWeight, packLine.JL_ActualWeightUQ),
				ResString.GetMultilingualString("7ed9119b-72d9-4f71-ba72-3a670b7333a6", "Volume: {0} {1}", packLine.JL_ActualVolume, packLine.JL_ActualVolumeUQ),
				ResString.GetMultilingualString("a350cc48-f156-4199-bd97-2822804f5d04", "Length: {0} {1}", packLine.JL_Length, packLine.JL_UnitOfDimension),
				ResString.GetMultilingualString("4db17a47-4999-483c-ab5f-498879b1a793", "Width: {0} {1}", packLine.JL_Width, packLine.JL_UnitOfDimension),
				ResString.GetMultilingualString("17a9fd71-32d8-4231-8e32-5dbbe18f92ee", "Height: {0} {1}", packLine.JL_Height, packLine.JL_UnitOfDimension),
				ResString.GetMultilingualString("eab19d00-84ef-4780-ac17-33500d20660d", "Marks & Numbers: {0}", packLine.JL_MarksAndNumbers),
				ResString.GetMultilingualString("7fea47a4-299b-4f5b-9e37-111ce9756ffd", "Tariff #: {0}", packLine.JL_HarmonisedCode),
				ResString.GetMultilingualString("fda495af-9b6b-4bab-b105-aa5c96574a92", "Line Price: {0}", packLine.JL_LinePrice),
				ResString.GetMultilingualString("239f5208-ab7c-438a-ac23-e0692d6932fe", "Description: {0}", packLine.JL_Description));
		}

		public PropertyChangeInfo[] GetPropertiesForEmailReporting()
		{
			return PropertiesForEmailReporting.GetValuesAsArray();
		}

		PropertyChangeInfoCollection PropertiesForEmailReporting
		{
			get
			{
				if (fPropertiesForEmailReporting == null)
				{
					fPropertiesForEmailReporting = new PropertyChangeInfoCollection();
				}

				return fPropertiesForEmailReporting;
			}
		}
		PropertyChangeInfoCollection fPropertiesForEmailReporting;

		#endregion

		#endregion

		#region House Bill Number Customisation

		protected NumberGeneratorContext GetBillOfLadingCustomisation()
		{
			GlbBranch branch = GetBranchForBillOfLadingCustomisation();
			ZGuid departmentPK = GetDepartmentForBillOfLadingCustomisation();

			if (branch != null)
			{
				return new NumberGeneratorContext(branch.GB_GC, branch.PK, departmentPK);
			}
			else
			{
				return new NumberGeneratorContext();
			}
		}

		ZGuid GetDepartmentForBillOfLadingCustomisation()
		{
			DepartmentChooser deptChooser = DepartmentChooser.New(Factory);
			IJobInvoicingPlugIn pluginData = Booking;

			if (IsDomesticFreight)
			{
				return deptChooser.GetDepartment(pluginData, true, false, pluginData.InvoicingSupporter.ContainerMode);
			}
			else
			{
				return deptChooser.GetDepartment(pluginData);
			}
		}

		GlbBranch GetBranchForBillOfLadingCustomisation()
		{
			OrgHeader loggedInOrg = LoggedInContact != null ? LoggedInContact.Header : null;
			OrgHeader consignor = Booking.ConsignorDocumentaryAddress != null ? Booking.ConsignorDocumentaryAddress.Organisation : null;
			OrgHeader consignee = Booking.ConsigneeDocumentaryAddress != null ? Booking.ConsigneeDocumentaryAddress.Organisation : null;

			return GetBranchForOrganisation(loggedInOrg)
				?? GetBranchForOrganisation(consignor)
				?? GetBranchForOrganisation(consignee)
				?? GetBranchForUNLOCO(Booking.Origin)
				?? GetBranchForUNLOCO(Booking.Destination);
		}

		GlbBranch GetBranchForOrganisation(OrgHeader organisation)
		{
			GlbBranch result = null;

			if (organisation != null)
			{
				foreach (OrgCompanyData companyData in organisation.CompanyDataCollection)
				{
					if (companyData.ControllingBranch != null)
					{
						result = companyData.ControllingBranch;
						break;
					}
				}
			}

			return result;
		}

		GlbBranch GetBranchForUNLOCO(RefUNLOCO uNLOCO)
		{
			GlbBranch result = null;

			if (uNLOCO != null)
			{
				ZQuery branchQuery = new ZQuery(GlbBranchSchema.GB_RL_NKHomePort, uNLOCO.Code);
				result = Factory.LoadTop1<GlbBranch>(branchQuery);
			}

			return result;
		}

		#endregion

		#region Document Supporter

		public virtual DocumentSupporter DocumentSupporter
		{
			get { return new TrackingBookingDocumentSupporter(this); }
		}

		public class TrackingBookingDocumentSupporter : QuotedBookingDocumentSupporter
		{
			public TrackingBookingDocumentSupporter(TrackingBooking trackingBooking)
				: base(trackingBooking.QuotedBooking)
			{
				this.TrackingBooking = trackingBooking;
			}

			protected readonly TrackingBooking TrackingBooking;

			protected override DocumentWrapper[] GetDocumentWrappersInternal(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
			{
				DocumentWrapper[] result = null;

				if (dataContext == Constants.DataContext.FreightLabels)
				{
					DocumentShipment docShipmentBizObject = new DocumentShipment(TrackingBooking.Booking, dataContext);
					docShipmentBizObject.IncludeNone = true;

					if (WebDataRegistry.Instance.PrintAddressesOnFreightLabels.Value)
					{
						if (TrackingBooking.LoggedInContact != null && TrackingBooking.Booking.Consignor != null &&
							TrackingBooking.LoggedInContact.OC_OH == TrackingBooking.Booking.Consignor.PK)
						{
							docShipmentBizObject.IncludeConsignor = true;
							docShipmentBizObject.IncludeNone = false;
						}
						else
						{
							docShipmentBizObject.IncludeConsignee = true;
							docShipmentBizObject.IncludeNone = false;
						}
					}

					docShipmentBizObject.NumberOfLabelsToPrint = Shipment.JS_OuterPacks;

					result = new DocumentWrapper[] { DocumentWrapperFactory.CreateWrapper(Constants.DataContext.Shipment, docShipmentBizObject) };
				}
				else
				{
					result = base.GetDocumentWrappersInternal(dataContext, commandBeingRun);
				}

				return result;
			}
		}

		#endregion

		#region IWorkflowProvider Members

		IColumnValueRanker IWorkflowProviderCore.GetTemplateSelectionCriteria()
		{
			return ((IWorkflowProvider)QuotedBooking).GetTemplateSelectionCriteria();
		}

		IProcessHeaderCollection IWorkflowProvider.Workflows => ((IWorkflowProvider)QuotedBooking).Workflows;

		ProcessTaskCollection IWorkflowProvider.WorkflowItems
		{
			get { return ((IWorkflowProvider)QuotedBooking).WorkflowItems; }
		}

		ZString IWorkflowProviderCore.WorkflowType
		{
			get { return ((IWorkflowProvider)QuotedBooking).WorkflowType; }
		}

		IWorkflowInformationProvider IWorkflowProvider.GetWorkflowInformationProvider()
		{
			return null;
		}

		#endregion

		#region Override Delete for IWorkFlowProvider to delete WorkflowItems

		public override void Delete()
		{
			QuotedBooking.Delete();
			((IWorkflowProvider)this).WorkflowItems.RemoveAndDeleteAll();
			base.Delete();
		}

		#endregion

		#region OnFactorySaving override

		protected override void OnFactorySaving()
		{
			if (Booking != null)
			{
				if (LoggedInContact != null &&
						(Booking.Consignor == null || Booking.Consignor.PK != LoggedInContact.OC_OH) &&
						(Booking.Consignee == null || Booking.Consignee.PK != LoggedInContact.OC_OH))
				{
					Booking.NotifyPartyDocumentaryAddress.OrganisationPK = LoggedInContact.OC_OH; // Set up NotifyParty if both Consignor and Consignee are not the LoggedInOrganisation
				}

				if (OuterPacks.IsEmpty && ActualWeight.IsEmpty && ActualVolume.IsEmpty || IsFCL)
				{
					Booking.UpdateShipmentFromOuterPackLines();
				}
			}

			foreach (ForwardingPackLine line in OuterPackLines)
			{
				var productLinesToDelete = line.Products.Where(x => x != null && x.D2_JO.IsEmpty && x.D2_ProductCode.IsEmpty && x.D2_ProductQuantity.IsEmpty).ToArray();
				foreach (var productLine in productLinesToDelete)
				{
					line.Products.RemoveFromRelationship(productLine);
					productLine.Delete();
				}
			}

			FixHiddenFieldsValuesBeforeSaving();
			base.OnFactorySaving();
		}

		protected virtual void FixHiddenFieldsValuesBeforeSaving()
		{
			if (Booking != null)
			{
				if (Booking.JS_ActualChargeableInfo.HasErrors())
				{
					Booking.JS_ActualChargeable = ZDecimal.Zero;
				}
				if (Booking.JS_InspectionTypeCode.IsEmpty || (Booking.JS_InspectionTypeCodeInfo.HasErrors() && !Booking.IsInDatabase))
				{
					Booking.JS_InspectionTypeCode = BaseJobShipmentLookups.InspectionType_Web;
				}
			}
		}

		#endregion

		public OrgHeader LoggedInOrganisation
		{
			get { return (LoggedInContact == null) ? null : LoggedInContact.ParentOrg; }
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

		TrackingSiteUser SiteUser
		{
			get { return WebEnv.AppInstance != null ? WebEnv.AppInstance.SiteUser as TrackingSiteUser : null; }
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
			get
			{
				if (editableMilestones == null)
				{
					editableMilestones = new TrackingMilestoneCollection(this, true);
					RegisterEditableChildObject(editableMilestones);
				}

				return editableMilestones;
			}
		}
		TrackingMilestoneCollection editableMilestones;

		#endregion

		#region IUpdatableMilestoneEventsProvider

		public List<string> UpdatableMilestoneEventCodes
		{
			get
			{
				if (WebEnv.AppInstance != null && WebEnv.AppInstance.SiteUser != null)
				{
					fUpdatableMilestoneEventCodes = (new UpdateableMilestoneEventsHelper(WebEnv.AppInstance.SiteUser as TrackingSiteUser)).GetUpdateableMilestoneEvents(WebDataRegistry.Instance.BookingMilestoneEventUpdates.Value, WebParties);
				}
				return fUpdatableMilestoneEventCodes;
			}
#if DEBUG
			set
			{
				fUpdatableMilestoneEventCodes = value;
			}
#endif
		}

		List<string> fUpdatableMilestoneEventCodes = new List<string>();

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

		WebPartyTypeOrgPairCollection WebParties
		{
			get
			{
				if (webParties == null)
				{
					webParties = new WebPartyTypeOrgPairCollection();
					if (Booking != null)
					{
						webParties.Add(WebPartyType.ExportBroker, Booking.ExportBroker);
						webParties.Add(WebPartyType.ImportBroker, Booking.ImportBroker);
						webParties.Add(WebPartyType.DeliveryAgent, Booking.DeliveryAgent);
						foreach (CommonConsol consol in Booking.Consols)
						{
							webParties.Add(WebPartyType.SendingAgent, consol.SendingForwarder);
							webParties.Add(WebPartyType.ReceivingAgent, consol.ReceivingForwarder);
						}
						webParties.Add(WebPartyType.Shipper, Booking.Consignor);
						webParties.Add(WebPartyType.Consignee, Booking.Consignee);
						if (Booking.Job != null)
						{
							webParties.Add(WebPartyType.LocalClient, Booking.Job.LocalCharges);
						}
					}
					if (QuotedBooking?.Job != null)
					{
						webParties.Add(WebPartyType.LocalClient, QuotedBooking.Job.LocalCharges);
					}
				}

				return webParties;
			}
		}
		WebPartyTypeOrgPairCollection webParties;

		#region IWebDocumentsSupport Members

		public DocumentSupport DocumentHelper
		{
			get
			{
				if (fDocumentHelper == null)
				{
					fDocumentHelper = new DocumentSupport(this);
				}
				return fDocumentHelper;
			}
		}
		DocumentSupport fDocumentHelper;

		public ZGuid DocParentPK
		{
			get { return PK; }
		}

		public List<ZGuid> DocRelatedPKs
		{
			get
			{
				List<ZGuid> result = new List<ZGuid>();
				result.Add(Booking.PK);
				return result;
			}
		}

		#endregion

		#region IWebDocumentsWithUploadSupport

		public DocManagerInfo DocManagerInfo
		{
			get
			{
				return ((IDocManagerSupport)QuotedBooking).DocManagerInfo;
			}
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
			fDocumentHelper = null;
		}

		#endregion

		#region Validation

		public TrackingBookingValidation Validation
		{
			get
			{
				if (!Factory.HasDomainValidation ||
						!Factory.Validation.MainGroup.ContainsDomainValidation(typeof(JobDocAddress), typeof(TrackingPickupDeliveryAddressValidation)))
				{
					Factory.Validation.MainGroup.RegisterValidationType(typeof(JobDocAddress), typeof(TrackingPickupDeliveryAddressValidation));
				}

				return new TrackingBookingValidation(this);
			}
		}

		protected override void RunPreSaveValidationCore()
		{
			FixHiddenFieldsValuesBeforeSaving();
			Validation.ValidateAll();
			base.RunPreSaveValidationCore();
		}

		#endregion

		#region IDocumentsMenuProvider Members

		DocumentsMenuHelper IDocumentsMenuProvider.DocumentsMenuHelper
		{
			get
			{
				return new TrackingBookingDocumentsMenuHelper(this);
			}
		}

		#endregion
		#region IModuleFilterProvider Members

		public ZQuery FilterSubQuery(Type businessObjectTypeToFilter)
		{
			ZQuery result = new ZQuery();
			if (businessObjectTypeToFilter == typeof(OrderLine))
			{
				ZDBOnlyQuery orderLineQuery = new ZDBOnlyQuery(typeof(OrderLine));
				if (AttachedOrderLinks.Count == 0)
				{
					orderLineQuery.AddToFilter(JobOrderLineSchema.JO_JD, ZGuid.Empty);
				}
				else
				{
					orderLineQuery.AddToFilter(JobOrderLineSchema.JO_JD, Array.ConvertAll(AttachedOrderLinks.ToArray<TrackingBookingOrderLink>(), orderLink => orderLink.OrderPK));
				}
				result.AddToFilter(orderLineQuery, JoinCondition.And);
			}
			return result;
		}

		public void ApplyAdditionalLoggedInUserFilter(Type businessObjectTypeToFilter, ZQuery loggedInUserFilter)
		{
			if (businessObjectTypeToFilter == typeof(Order))
			{
				if (WebDataRegistry.Instance.BookingAttachOrdersWithoutSupplier.Value)
				{
					var supplierBuyerQuery = new ZQuery(JobOrderHeaderSchema.JD_OA_SupplierAddress, null);

					var consignee = Factory.Load<OrgHeader>(ConsigneeOrganisationPK);
					if (consignee != null)
					{
						supplierBuyerQuery.AddToFilter(JoinCondition.And, JobOrderHeaderSchema.JD_OA_BuyerAddress, consignee.Addresses.Select(x => x.PK));
					}

					loggedInUserFilter.AddToFilter(supplierBuyerQuery, JoinCondition.Or);
				}
			}
		}

		#endregion

		public void SetupDefaults(FilterBusinessObject filterBizO, FilterBusinessObjectDefaults filterBODetauls)
		{
			if (filterBizO is OrdersFilterBusinessObject)
			{
				IAttachOrders ordersConsumer = Booking;
				if (ordersConsumer != null)
				{
					filterBODetauls.Add(new FilterBusinessObjectDefault("Transport Mode", "Property", ordersConsumer.TransportMode));
				}
				filterBODetauls.Add(new FilterBusinessObjectDefault("Attached / Unattached Orders", "Property", (ZString)(NoResString)"Unattached"));
			}
		}

		#region ITemplateReversible Members

		void ITemplateReversible.Reverse()
		{
			((ITemplateReversible)QuotedBooking).Reverse();
		}

		#endregion

		#region ITemplateCopyable Members

		IBusiness ITemplateCopyable.TemplateCopy()
		{
			return new TrackingBooking((QuotedBooking)((ITemplateCopyable)QuotedBooking).TemplateCopy(), LoggedInContact);
		}

		#endregion
	}
}
