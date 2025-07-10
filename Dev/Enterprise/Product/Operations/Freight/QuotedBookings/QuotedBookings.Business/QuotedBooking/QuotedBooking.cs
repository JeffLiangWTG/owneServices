using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Common.Testing;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.IO;
using CargoWise.Macros;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWise.UniversalCopy;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Integration;
using Enterprise.BufferManagement.Integration;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.ContractManagement.Business;
using Enterprise.Customs.Common;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.EConversation.Business;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.CarbonEmissions.Business;
using Enterprise.Freight.CarbonEmissions.Integration;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Business.HelperClasses;
using Enterprise.Freight.Forwarding.Business.HelperClasses.FilteredRatingContractAllocationLine;
using Enterprise.Freight.Integration;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.Integration;
using Enterprise.Integration.TransportBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.CreditControl.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Metadata.Integration;
using Enterprise.Rating.Business;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.TransportCommon.Shared;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Workflow.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using static Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;
using static Enterprise.Freight.Integration.IGlobalCommercialInvoiceComplianceProcessor;
using static Enterprise.Integration.Forwarding;
using AdditionalReferenceNumbersCodes = Enterprise.Registry.Business.CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes;
using Constants = Enterprise.Core.Constants;
using OrgCodes = Enterprise.MasterFiles.Business.AccountingMasterFilesConstants.OrganisationTypeCodes;
using Params = CargoWise.EventReference.Constants.EventReferenceParameters.Codes;
using Shipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;
using TransportBooking = Enterprise.TransportBookings.Shared;
using WorkflowSelectionOrgTypeCodes = Enterprise.Registry.Business.ClientInTemplateSelectionOrgTypeList.Codes;

namespace Enterprise.Freight.QuotedBookings.Business
{
	[DebuggerDisplay("Quote: {Quote != null ? Quote.JobNumber : \"\"}, Booking: {Booking != null ? Booking.JobNumber : \"\"}")]
	[PreventDelete(true)]
	[UserDefinedValues]
	[UniversalDataContext(DataContextType.ForwardingBooking)]
	[UniversalCopyWithExtendedEntities(FinishCopyMethod = "FinishInitializationAfterUniversalCopy")]
	[UniversalCopyExtraCollection("CustomFields", "IAddOnValue", GenCustomAddOnValueSchema.Constants.TableName, GenCustomAddOnValueSchema.Constants.XV_ParentID, GenCustomAddOnValueSchema.Constants.XV_ParentTableCode)]
	[UniversalCopyElementsOrder("Booking", "CustomFields")]
	[ProvideMetaDataProperty("DefaultNumberOfDecimals", MetaDataTypes.DecimalPlaces)]
	[UniversalCopyElementsOrder("Quote", "CustomFields")]
	[UniversalCopyElementsOrder("ClientPK", "ClientAddrPK")]
	[UniversalCopyElementsOrder("OH_Carrier", "CarrierServiceLevel")]
	[UniversalCopyElementsOrder("Booking", "OH_Carrier")]
	[UniversalCopyElementsOrder("Quote", "OH_Carrier")]
	[UniversalCopyElementsOrder("OneOffQuoteContainerMode", "Mode")]
	[UniversalCopyInstanceType(InstanceType = typeof(QuotedBooking), CreationMethod = "NewForUniversalCopy", GetSourceMethod = "GetSourceForUniversalCopy", ShouldSyncTreeNodes = true)]
	[CodeProperty("QuotedBookingNumber"), DescriptionProperty("HumanReadableName")]
	[VisualizableDocumentsSupportable("QuotedBookingVisualizableDocumentSupporter")]
	[MetadataContext(MetadataContext.QuotedBooking)]
	[UniversalCopyIgnoreElement(nameof(Job))]
	public class QuotedBooking : NonPersistentBusinessObject,
								IQuotedBooking,
								IJobInvoicingPlugIn,
								IRatingSupporter,
								IImportExport,
								IEDocsPluginHostDecider,
								ISailingChooserParent,
								IDocumentSupportableOverrideType,
								IDocumentDeliveredLogSupporter,
								IEDocsProvider,
								ICancellable,
								IMAWBParent,
								ITemplateCopyable,
								ISometimesWorkflowProvider,
								ICustomFieldProvider,
								IAutoRatingFreightConditionsSupportable,
								ISupportDataImporting,
								IBuyerSupplierRelationshipConsumer,
								ICartageParent,
								ITemplateReversible,
								IProcessTaskTemplateUpdatable,
								IStmALogParent,
								IDefaultNumberOfDecimalsSupporterWithSchemaColumn,
								TransportBooking.IDtbBookingParent,
								ISalesRelationActivity,
								IImportParentRelatedActivityInfoOnNew,
								ISupportTradeDetailImporting,
								ISupportsPostingOverseasAgentCharge,
								IUniversalXMLNoteParent,
								ISendEmailSource,
								ITemplateRecordProvider,
								IParentDocManagerSupport,
								IDocAddresses,
								ICreditControlledDocumentDelivery,
								IConversationProvider,
								IUniversalCopyValidationStrategy,
								ICompliancePartyRiskStatusProvider,
								IComplianceLocationRiskStatusProvider,
								IComplianceCommodityRiskStatusProvider,
								IComplianceJobDirectionProvider,
								IScreeningPartyProvider,
								IScreeningPartyForVessel,
								ICO2eLegBasedSupporter,
								IAllocationRouteAssignable,
								IChargeCreditorDefaulting,
								IConfirmAddressParent,
								IChargeApplicableForCopy,
								IGlobalCommercialInvoiceJobProvider,
								IGlobalCommercialInvoiceProvider,
								ITriggerActionProvider,
								IAuditDetailsWithContext
	{
		#region Schema

		public static class Schema
		{
			public const string TableName = ViewQuotedBookingSchema.Constants.TableName;
			public const string PK = ViewQuotedBookingSchema.Constants.PK;

			public const string IsCompareMode = "IsCompareMode";
			public const string IsCompareServiceLevel = "IsCompareServiceLevel";
			public const string TEUCount = "TEUCount";
			public const string ContainerCountOther = "ContainerCountOther";
			public const string ContainerCount40RE = "ContainerCount40RE";
			public const string ContainerCount40GP = "ContainerCount40GP";
			public const string ContainerCount20RE = "ContainerCount20RE";
			public const string ContainerCount20GP = "ContainerCount20GP";
			public const string ContainerCount = "ContainerCount";
			public const string VesselName = "VesselName";
			public const string GoodsDescription = "GoodsDescription";
			public const string PacksType = "PacksType";
			public const string Packs = "Packs";
			public const string ClientPK = "ClientPK";
			public const string ConsignorContact = "ConsignorContact";
			public const string ConsigneeContact = "ConsigneeContact";
			public const string ClientContact = "ClientContact";
			public const string ConsignorFieldType = "ConsignorFieldType";
			public const string ConsigneeFieldType = "ConsigneeFieldType";
			public const string ConsignorNameOrPK = "ConsignorNameOrPK";
			public const string ConsigneeNameOrPK = "ConsigneeNameOrPK";
			public const string TransitTime = "TransitTime";
			public const string FrequencyUnit = "FrequencyUnit";
			public const string Frequency = "Frequency";
			public const string ClientAddrPK = "ClientAddrPK";
			public const string Mode = "Mode";
			public const string TransportMode = "TransportMode";
			public const string ContainerMode = "ContainerMode";
			public const string Weight = "Weight";
			public const string WeightUnit = "WeightUnit";
			public const string Volume = "Volume";
			public const string VolumeUnit = "VolumeUnit";
			public const string Chargeable = "Chargeable";
			public const string ChargeableUnit = "ChargeableUnit";
			public const string IsDomesticFreight = "IsDomesticFreight";
			public const string PickupReady = "PickupReady";
			public const string PickupClose = "PickupClose";
			public const string DeliveryOpen = "DeliveryOpen";
			public const string DeliveryClose = "DeliveryClose";
			public const string Origin = "Origin";
			public const string Destination = "Destination";
			public const string Via = "Via";
			public const string OH_Carrier = "OH_Carrier";
			public const string CarrierServiceLevel = "CarrierServiceLevel";
			public const string PaymentTerms = "PaymentTerms";
			public const string AdditionalTerms = "AdditionalTerms";
			public const string ContainerPackModeOverride = "ContainerPackModeOverride";
			public const string ServiceLevel = "ServiceLevel";
			public const string Commodity = "Commodity";
			public const string FMCTariffID = "FMCTariffID";
			public const string PickupEquipment = "PickupEquipment";
			public const string DeliveryEquipment = "DeliveryEquipment";
			public const string GoodsValue = "GoodsValue";
			public const string GoodsCurrency = "GoodsCurrency";
			public const string InsuranceValue = "InsuranceValue";
			public const string InsuranceCurrency = "InsuranceCurrency";
			public const string CompanyTariffLevel = "CompanyTariffLevel";
			public const string ExportReceivingDepot = "ExportReceivingDepot";
			public const string ImportReleaseDepot = "ImportReleaseDepot";
			public const string QuoteNumberOfEntries = "QuoteNumberOfEntries";
			public const string QuoteNumberOfEntryLines = "QuoteNumberOfEntryLines";
			public const string BookingOuterPacks = "BookingOuterPacks";
			public const string QuotedBookingTypeCode = "QuotedBookingTypeCode";
			public const string HBLAWBChargesDisplay = "HBLAWBChargesDisplay";
			public const string IsForwardRegistered = "IsForwardRegistered";
			public const string ShipmentStatus = "ShipmentStatus";
			public const string LoadPort = "LoadPort";
			public const string DischargePort = "DischargePort";
			public const string ETD = "ETD";
			public const string ETA = "ETA";
			public const string IsAviationSecurityApplicableForTransportMode = "IsAviationSecurityApplicableForTransportMode";
			public const string OneOffQuoteApprovalStatus = "OneOffQuoteApprovalStatus";
			public const string OneOffQuoteIsAmended = "OneOffQuoteIsAmended";
			public const string DeliveryDueDate = "DeliveryDueDate";
			public const string RevisedDeliveryDueDate = "RevisedDeliveryDueDate";
			public const string CO2ePerTonneInKg = "CO2ePerTonneInKg";
			public const string CO2ePerTEUInKg = "CO2ePerTEUInKg";
			public const string CO2eStatus = "CO2eStatus";
			public const string CO2eDistanceInKM = "CO2eDistanceInKM";
			public const string CarrierContractNumber = "CarrierContractNumber";
			public const string AllocationLinePK = "AllocationLinePK";
			public const string Creditor = "Creditor";
		}

		public override SchemaGuidColumn PKSchemaColumn
		{
			get { return ViewQuotedBookingSchema.PK; }
		}

		#endregion

		#region Construction

		[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters")]
#if DEBUG
		internal
#endif
		protected object NewForUniversalCopy(BusinessObjectFactory factory, object parentEntity)
		{
			return new QuotedBooking(factory);
		}

#if DEBUG
		internal
#endif
		protected BusinessObject GetSourceForUniversalCopy()
		{
			var newFactory = CreateNewFactory();
			if (IsInDatabase)
			{
				return newFactory.Load(typeof(QuotedBooking), PK);
			}

			if (Quote != null)
			{
				newFactory.ImportFromAnotherFactory(Quote);
				newFactory.ImportFromAnotherFactory(Quote.CurrentOneOffQuote);
			}

			if (bookingPK.IsValid)
			{
				newFactory.ImportFromAnotherFactory(Booking);
				newFactory.ImportFromAnotherFactory(Booking.DocsAndCartage);
			}

			return New(quotePK, bookingPK, newFactory);
		}

		public static QuotedBooking New(QuoteBookingType quotedBookingType, BusinessObjectFactory factory)
		{
			if (quotedBookingType == Enterprise.Freight.Integration.QuoteBookingType.SpotQuote)
			{
				Quote spotQuote = QuotedBooking.CreateNewQuote(factory, QuotedBooking.QuoteState.NotApprovedAndNotAccepted);
				return New(spotQuote.PK, ZGuid.Empty, false, factory);
			}
			else if (quotedBookingType == Enterprise.Freight.Integration.QuoteBookingType.QuickBooking)
			{
				using (ShipmentFieldStateChange.InitializingShipment(factory))
				{
					ForwardingShipment quoteBooking = QuotedBooking.CreateNewBooking(factory);
					return New(ZGuid.Empty, quoteBooking.PK, false, factory);
				}
			}
			else
			{
				Quote spotQuote = QuotedBooking.CreateNewQuote(factory, QuotedBooking.QuoteState.NotApprovedAndNotAccepted);
				using (ShipmentFieldStateChange.InitializingShipment(factory))
				{
					ForwardingShipment quoteBooking = QuotedBooking.CreateNewBooking(factory);
					return New(spotQuote.PK, quoteBooking.PK, false, factory);
				}
			}
		}

		public static QuotedBooking New(ZGuid quotePK, ZGuid quoteBookingPK, BusinessObjectFactory factory, IScheduleChooserCreator scheduleChooserCreator = null)
		{
			return New(quotePK, quoteBookingPK, true, factory, scheduleChooserCreator);
		}

		public static QuotedBooking New(ViewQuotedBooking viewQuotedBooking, BusinessObjectFactory factory)
		{
			return New(viewQuotedBooking.VB_TH, viewQuotedBooking.VB_JS, false, factory);
		}

		static QuotedBooking New(ZGuid quotePK, ZGuid quoteBookingPK, bool attemptToLoadFromOther, BusinessObjectFactory factory, IScheduleChooserCreator scheduleChooserCreator = null)
		{
			QuotedBooking result = null;

			var overridden = OverridableNewDelegate.Value;
			if (overridden != null)
			{
				result = overridden(quotePK, quoteBookingPK, attemptToLoadFromOther, factory);
			}
			else
			{
				result = new QuotedBooking(quotePK, quoteBookingPK, attemptToLoadFromOther, factory, scheduleChooserCreator);
			}

			return result.ObjectState != QuotedBookingState.None ? result : null;
		}

		public static QuotedBooking New(BusinessObjectFactory factory, ITemplateRecord localTemplateRecord)
		{
			QuotedBooking result = new QuotedBooking(factory, localTemplateRecord);

			return result.ObjectState != QuotedBookingState.None ? result : null;
		}

		[SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		protected QuotedBooking(BusinessObjectFactory factory, ITemplateRecord localTemplateRecord)
			: base(factory)
		{
			((ITemplateRecordProvider)this).LoadFromTemplateRecord(localTemplateRecord);

			Initialise(this.quotePK, this.bookingPK, true);
		}

		[SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		protected QuotedBooking(ZGuid quotePK, ZGuid quoteBookingPK, bool attemptToLoadFromOther, BusinessObjectFactory factory, IScheduleChooserCreator scheduleChooserCreator = null)
			: base(factory)
		{
			this.scheduleChooserCreator = scheduleChooserCreator;
			Initialise(quotePK, quoteBookingPK, attemptToLoadFromOther);
		}

		internal QuotedBooking(BusinessObjectFactory factory) : base(factory) { }

		protected delegate QuotedBooking NewDelegate(ZGuid quotePK, ZGuid bookingPK, bool attemptToLoadFromOther, BusinessObjectFactory factoryToWrap);
		protected static readonly Overridable<NewDelegate> OverridableNewDelegate = new Overridable<NewDelegate>();

		string creationCallStack;

		[SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals")]
		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "constant string")]
		void Initialise(ZGuid quotePK, ZGuid quoteBookingPK, bool attemptToLoadFromOther)
		{
			creationCallStack = wasCreatedByUniversalCopy
				? "created by Universal Copy"
				: new StackTrace(1).ToString();

			if (quotePK.IsEmpty && quoteBookingPK.IsEmpty)
			{
				throw new ArgumentException("Needs at least one valid Quote or Booking PK");
			}

			var isUpdatedQuote = this.quotePK != quotePK;
			var isUpdatedBooking = this.bookingPK != quoteBookingPK;

			this.quotePK = quotePK;
			this.bookingPK = quoteBookingPK;

			if (attemptToLoadFromOther)
			{
				if (Quote == null)
				{
					LoadQuoteByBookingNumber();
				}

				if (Booking == null)
				{
					LoadBookingByQuoteNumber();
				}
			}

			if (!quotePK.IsEmpty && Quote == null)
			{
				throw new ArgumentException("Invalid Quote PK.");
			}

			if (Quote != null && !Quote.TH_OneTimeQuote)
			{
				throw new ArgumentException("Invalid Quote - Should be a One Time Quote.");
			}

			if (Quote != null)
			{
				Quote.SetParentQuotedBooking(this);
				if (isUpdatedQuote)
				{
					this.BusinessObjectRelationChanged(Quote, BusinessObjectParentLocatorEvent.ParentAdded);
				}
			}

			TryAssignQuoteNumber();

			if (ObjectState == QuotedBookingState.QuoteOnly)
			{
				RegisterEditableChildObject(Quote);
			}

			if (Booking != null)
			{
				RegisterEditableChildObject(Booking);
				if (isUpdatedBooking)
				{
					this.BusinessObjectRelationChanged(Booking, BusinessObjectParentLocatorEvent.ParentAdded);
				}
			}

			HookQuoteEvents();
			HookBookingEvents();
			ModeInfo.ValueChanged += Mode_ValueChanged;

			if (ObjectState == QuotedBookingState.AcceptedBookingWithQuote)
			{
				SynchDocAddressesBookingWithQuote();
			}

			if (ObjectState != QuotedBookingState.None && ObjectState != QuotedBookingState.QuoteOnly)
			{
				ScheduleChooser touchScheduleChooser = ScheduleChooser;
			}

			base.AddToFactoryCache();

			BuyerSupplierLinksHelper = new BuyerSupplierLinksHelper<QuotedBooking>(this);
			if (Quote != null || Booking != null)
			{
				BuyerSupplierLinksHelper.Register();
			}

			if (((ICancellable)this).IsCancelled)
			{
				SetReadOnlyIncludingChildren(true);
			}

			UpdateCollectionsFromMode();

			ViewQuotedBooking.LoadOrCreate(this);

			oldRequiresTemperatureControl = (this as ICO2eLegBasedSupporter).RequiresTemperatureControl;
		}

		void TryAssignQuoteNumber()
		{
			if (!quotePK.IsValid
				|| Booking == null)
			{
				return;
			}

			var isThereAnyBookingLinkedToMyQuote = Factory.Exists(typeof(ForwardingShipment), new ZQuery(JobShipmentSchema.JS_TH_OneTimeQuote, quotePK));

			if (!isThereAnyBookingLinkedToMyQuote)
			{
				Booking.JS_TH_OneTimeQuote = quotePK;
			}
		}

		void LoadQuoteByBookingNumber()
		{
			if (quotePK.IsValid
				|| !bookingPK.IsValid)
			{
				return;
			}

			var quoteByBooking = Factory.Load<ForwardingShipment>(bookingPK);

			if (quoteByBooking != null)
			{
				quotePK = quoteByBooking.JS_TH_OneTimeQuote;
			}
		}

		void LoadBookingByQuoteNumber()
		{
			if (bookingPK.IsValid
				|| !quotePK.IsValid)
			{
				return;
			}

			var bookingByQuote = Factory.LoadTop1<ForwardingShipment>(new ZQuery(JobShipmentSchema.JS_TH_OneTimeQuote, quotePK));

			if (bookingByQuote != null)
			{
				bookingPK = bookingByQuote.PK;
			}
		}

		public enum QuoteState
		{
			NotApprovedAndNotAccepted = 0,
			ApprovedButNotAccepted = 1,
			ApprovedAndAccepted = 2,
		}

		public static Quote CreateNewQuote(BusinessObjectFactory factory, QuoteState quoteState)
		{
			Quote quote = factory.New<Quote>();
			quote.TH_OneTimeQuote = true;

			if (QuoteState.ApprovedAndAccepted == quoteState || QuoteState.ApprovedButNotAccepted == quoteState)
			{
				quote.CurrentOneOffQuote.TT_QuoteApprovedByManager = true;
			}

			if (QuoteState.ApprovedAndAccepted == quoteState)
			{
				quote.TH_Accepted = ZDateTime.Now;
			}

			return quote;
		}

#if DEBUG
		internal
#endif
		protected void CreateNewQuoteForUniversalCopy()
		{
			var quote = Factory.New<Quote>();
			quotePK = quote.PK;
			((INeedRow)quote).Row[RatingHeaderSchema.Constants.TH_OneTimeQuote] = true;
		}

		public static ForwardingShipment CreateNewBooking(BusinessObjectFactory factory)
		{
			using (ShipmentFieldStateChange.InitializingShipment(factory))
			{
				var booking = factory.New<ForwardingShipment>();
				booking.BuyerSupplierLinksHelper.Deregister();
				booking.BuyerSupplierLinksHelper = null;
				booking.JS_A_BKD = ZDateTime.Now;
				booking.JS_IsBooking = true;
				booking.JS_IsForwardRegistered = false;
				booking.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
				return booking;
			}
		}

		public void AddErrorInformation(NullReferenceException ex)
		{
			var jS_TH_OneTimeQuote = booking?.JS_TH_OneTimeQuote.ToString() ?? "null";
			var infoMessage = $@"BookingPK = '{bookingPK.ToString()}'
QuotePK = '{quotePK.ToString()}'
BusinessObjectPK = '{PK.ToString()}'
JS_TH_OneTimeQuote = '{jS_TH_OneTimeQuote}'
QuotedBookingState = '{ObjectState.ToString()}'";
			ex.Data.Add("QuotedBookingDebuggingInfo", infoMessage);
		}
#if DEBUG
		internal
#endif
		protected void CreateNewBookingForUniversalCopy()
		{
			bookingPK = CreateNewBooking(Factory).PK;
		}

#if DEBUG
		internal
#endif
		protected void FinishInitializationAfterUniversalCopy()
		{
			if (quotePK.IsEmpty && bookingPK.IsEmpty)
			{
				throw new UniversalCopyAbortException(Res.GetString("e243cc98-8946-4eda-85a2-63dbf065c7fc", "Neither Booking nor Quote were copied, but at least one of them is required. Check that the copy template is correctly configured."));
			}

			wasCreatedByUniversalCopy = true;
			Initialise(quotePK, bookingPK, false);
		}

#if DEBUG
		internal
#endif
		bool wasCreatedByUniversalCopy;

		#endregion

		#region Default Values

		public ZBool IsSettingDefaultValues
		{
			get
			{
				if (Booking != null && Booking.IsSettingDefaultValues)
				{
					return true;
				}
				else
				{
					return fIsSettingDefaultValues;
				}
			}
			set
			{
				fIsSettingDefaultValues = value;
			}
		}

		ZBool fIsSettingDefaultValues;

		protected override void SetDefaultValues()
		{
			IsSettingDefaultValues = true;
			try
			{
				base.SetDefaultValues();
			}
			finally
			{
				IsSettingDefaultValues = false;
			}
		}

		#endregion

		#region Business Object Overrides

		protected override ZGuid GetPK()
		{
			return Quote != null ? Quote.PK : Booking != null ? Booking.PK : base.GetPK();
		}

		protected override void AddToFactoryCache()
		{
			// Do not call base here. Called from factory methods, once Quote or Booking setup (Initialise).
		}

		protected override ZString HumanReadableNameCore
		{
			get
			{
				ZString result = HumanReadableNameWithoutID;

				if (Quote != null && Quote.IsInDatabase && !Quote.TH_QuoteNumber.IsEmpty)
				{
					result += " - " + Res.GetString("QuoteBookingName|QuoteNo", "Quote ({0})", Quote.TH_QuoteNumber);
				}
				if (Booking != null && Booking.IsInDatabase && !Booking.JS_UniqueConsignRef.IsEmpty)
				{
					result += " - " + Res.GetString("QuoteBookingName|BookingNo", "Booking ({0})", Booking.JS_UniqueConsignRef);
				}

				return result;
			}
		}

		public ZString HumanReadableNameWithoutID
		{
			get
			{
				var result = IsTemplateRecord ? Res.GetString("QuoteBookingName|Template", "Template") + " " : "";
				result += BusinessObjectName;
				return result;
			}
		}

		ZString BusinessObjectName
		{
			get
			{
				var result = string.Empty;

				switch (ObjectState)
				{
					case QuotedBookingState.QuoteOnly:
						result += Res.GetString("QuoteBookingName|QuoteOnly", "One Off Quote");
						break;
					case QuotedBookingState.BookingOnly:
						result += Res.GetString("QuoteBookingName|BookingOnly", "Quick Booking");
						break;
					default:
						result += Res.GetString("QuoteBookingName|BookingWithQuote", "Booking with Quote");
						break;
				}

				return result;
			}
		}

		protected override ZString HumanReadableShortcutNameCore
		{
			get
			{
				var result = string.Empty;

				if (Quote != null && Quote.IsInDatabase && !Quote.TH_QuoteNumber.IsEmpty)
				{
					result += Quote.TH_QuoteNumber;
				}

				if (Booking != null && Booking.IsInDatabase && !Booking.JS_UniqueConsignRef.IsEmpty)
				{
					result += (string.IsNullOrEmpty(result) ? "" : " - ") + Booking.JS_UniqueConsignRef;
				}

				return result;
			}
		}

		#region IsInComparisonMode

		public bool IsInComparisonMode
		{
			get { return fIsInComparisonMode || IsCompareMode; }
			set { fIsInComparisonMode = value; }
		}
		bool fIsInComparisonMode;

		#endregion

		public override bool IsInDatabase
		{
			get { return (Quote != null && Quote.IsInDatabase) || (Booking != null && Booking.IsInDatabase); }
		}

		public override bool ReadOnly
		{
			get { return base.ReadOnly; }
			set
			{
				base.ReadOnly = value;

				if (Booking != null)
				{
					Booking.ReadOnly = value;
				}

				if (Quote != null)
				{
					Quote.ReadOnly = value;
				}
			}
		}

		#endregion

		#region Related Objects

		#region Quote

		//WI00215268: Made unmandatory so you can copy template records lacking a Quote. Still exists in universal copy templates, because removing it broke some unit tests.
		[UniversalCopyRelatedEntity(CreationMethodName = "CreateNewQuoteForUniversalCopy", DisableCopyMethodLink = true)]
		[UniversalCopyExtraMetadata(Priority = -1, IsMandatory = false)]
		public Quote Quote
		{
			get
			{
				Quote result = Factory.Load<Quote>(quotePK);
				if (result != null)
				{
					result.IsInComparisonMode = IsInComparisonMode;
				}
				return result;
			}
		}
		ZGuid quotePK
		{
			get { return fQuotePK; }
			set { fQuotePK = value; }
		}
		ZGuid fQuotePK;

		#endregion

		public OneOffQuoteStatisticsWrapper OneOffQuoteStatistics
		{
			get
			{
				if (oneOffQuoteStatistics == null)
				{
					oneOffQuoteStatistics = new OneOffQuoteStatisticsWrapper(Quote?.CurrentOneOffQuote);
					if (ObjectState != QuotedBookingState.QuoteOnly && Quote != null)
					{
						RegisterEditableChildObject(OneOffQuoteStatistics);
					}
				}
				return oneOffQuoteStatistics;
			}
		}

		OneOffQuoteStatisticsWrapper oneOffQuoteStatistics;

		#region Booking

		[UniversalCopyRelatedEntity(CreationMethodName = "CreateNewBookingForUniversalCopy", DisableCopyMethodLink = true)]
		[UniversalCopyExtraMetadata(Priority = -1, IsMandatory = false)] //do not set this to true please - see CS00792809
		public ForwardingShipment Booking
		{
			get
			{
				if (!bookingPK.IsValid)
				{
					return null;
				}

				if (booking != null
					&& bookingPK == booking.PK)
				{
					return !booking.IsDeleted
						? booking
						: null;
				}

				booking = Factory.Load<ForwardingShipment>(bookingPK);

				if (booking?.BuyerSupplierLinksHelper != null)
				{
					booking.BuyerSupplierLinksHelper.Deregister();
					booking.BuyerSupplierLinksHelper = null;
				}
				QuotedBookingConcurrencyCheck.Register(Factory, bookingPK, creationCallStack);
				return booking;
			}
		}
		ZGuid bookingPK;
		ForwardingShipment booking;

		#endregion

		public bool CanCreateTransportBooking
		{
			get
			{
				return !IsForwardRegistered;
			}
		}

		public ZGuid BookingParentPK => PK;

		public string BookingParentTablePrefix => TablePrefix;

		public (bool isShouldShow, string caption, string message, string confirmation) GetExtendingConfirmMessageBeforeCreateTransportBooking() => (false, null, null, null);

		#region Job

		/// <summary>
		/// Loads the existing job, if one exists. Does NOT create a new job
		/// </summary>
		public JobHeader Job
		{
			get
			{
				string errorMessage;
				return LoadOrCreateJobHeaderWithMutex(false, out errorMessage);
			}
		}
		JobHeader jobHeader;

		public string TryLoadOrCreateJob()
		{
			string errorMessage;
			LoadOrCreateJobHeaderWithMutex(true, out errorMessage);

			return errorMessage;
		}

		void RemoveAllDefaultedCharges(JobHeader job)
		{
			if (job != null && !job.IsInDatabase)
			{
				ZQuery chargesQuery = new ZQuery(JobChargeSchema.JR_JH, job.PK);
				chargesQuery.FetchOnlyFromLocalCache = true;
				JobCharge[] charges = Factory.Load<JobCharge>(chargesQuery);
				for (int i = charges.Length - 1; i >= 0; i--)
				{
					charges[i].Delete();
				}
			}
		}

		JobHeader LoadOrCreateJobHeaderWithMutex(bool createWithMutex, out string errorMessage)
		{
			errorMessage = "";
			if (IsTemplate)
			{
				jobHeader = null;
				errorMessage = Res.GetString("9361df72-078d-4dbd-9a30-b055e6d9fc99", "You cannot set Client or Billing information on a template record.");
			}
			else if (createWithMutex)
			{
				var loader = new JobHeader.Loader(JobParent);
				jobHeader = loader.TryLoadOrCreateWithMutex();
				if (jobHeader == null)
				{
					errorMessage = loader.GetJobCreationError();
				}
				else if (UseJobFromBooking && !wasCreatedByUniversalCopy)
				{
					RemoveAllDefaultedCharges(jobHeader);
				}
			}
			else
			{
				// So far, QuotedBooking hasn't set Job's Parent by default.
				// However, in defect WI00701548, we noticed that triggers with FLD (Set Field) action need the Job's Parent to be set to run in the ServiceTasks.
				// The Rating team provided a solution for notifying the JobHeaderParent's BusinessObject to load the Job with the Parent only when necessary, but the Automation team rejected it.
				// After a long discussion, it's supposed that QuotedBooking sets the Job's Parent by default, like Shipment.
				jobHeader = new JobHeader.Loader(JobParent).Load(setParent: true, setJobDefaults: false);
			}

			if (jobHeader != null)
			{
				RegisterEditableChildObject(jobHeader);

				if (Booking != null)
				{
					Booking.RegisterEditableChildObject(jobHeader);
				}

				if (Quote != null)
				{
					Quote.RegisterEditableChildObject(jobHeader);
				}
			}
			return jobHeader;
		}

		#endregion

		#region QuotedBookingContainers

		[UniversalCopyCollectionEntity(JobContainerSchema.Constants.TableName, "")] // Do not specify item property name (FK) because it does not reference this QuotedBooking but associated Shipment
		public QuotedBookingContainerDependentCollection QuotedBookingContainers
		{
			get
			{
				if (quotedBookingContainers == null)
				{
					quotedBookingContainers = GetNewQuotedBookingContainers();
					quotedBookingContainers.CommodityCode = Commodity;
					quotedBookingContainers.Load();
					quotedBookingContainers.CountChanged += QuotedBookingContainers_CountChanged;
					RegisterEditableChildObject(quotedBookingContainers);
					OnQuotedBookingContainersCreated();
				}

				return quotedBookingContainers;
			}
		}

		QuotedBookingContainerDependentCollection quotedBookingContainers;

		QuotedBookingContainerDependentCollection GetNewQuotedBookingContainers()
		{
			return new QuotedBookingContainerDependentCollection(Booking, Factory);
		}

		void QuotedBookingContainers_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			Booking?.MarkAsNeedingValidationIncludingChildren();
		}

		bool originalRequireTEU;

		protected virtual void OnQuotedBookingContainersCreated()
		{
			HookBookingContainerEventForRequireTEU();
			HookRequiresTemperatureControlEvents(QuotedBookingContainers, container => [container.JC_RCInfo]);
		}

		void HookBookingContainerEventForRequireTEU()
		{
			originalRequireTEU = (this as ICO2eCalculationSupporter).RequireTEU;

			void ContainerTEUChanged(object s, EventArgs e)
			{
				if (!this.SkipCO2eStatusCheck())
				{
					var newRequireTEU = (this as ICO2eCalculationSupporter).RequireTEU;

					if ((newRequireTEU || originalRequireTEU) && s is ForwardingContainer && !IsDeleted && !IsDeleting)
					{
						UpdateQuotedBookingCO2eStatusToNotCurrent(new CO2eStatusChangedReason(e as ValueChangedEventArgs));
					}

					originalRequireTEU = newRequireTEU;
				}
			}

			foreach (var container in QuotedBookingContainers.Cast<ForwardingContainer>())
			{
				container.JC_ContainerCountInfo.ValueChanged += ContainerTEUChanged;
				container.JC_RCInfo.ValueChanged += ContainerTEUChanged;
			}

			QuotedBookingContainers.CountChanged += (sender, args) =>
			{
				if (!(args.BizObject is ForwardingContainer container))
				{
					return;
				}

				if (!this.SkipCO2eStatusCheck())
				{
					var newRequireTEU = (this as ICO2eCalculationSupporter).RequireTEU;

					if ((originalRequireTEU || newRequireTEU) && !IsDeleted && !IsDeleting)
					{
						UpdateQuotedBookingCO2eStatusToNotCurrent(new CO2eStatusChangedReason(args, (NoResString)"Container"));
					}
					originalRequireTEU = newRequireTEU;
				}

				if (args.ItemAdded)
				{
					container.JC_ContainerCountInfo.ValueChanged += ContainerTEUChanged;
					container.JC_RCInfo.ValueChanged += ContainerTEUChanged;
				}
				else if (args.ItemRemoved)
				{
					container.JC_ContainerCountInfo.ValueChanged -= ContainerTEUChanged;
					container.JC_RCInfo.ValueChanged -= ContainerTEUChanged;
				}
			};
		}

		#endregion

		#region DocumentaryOverrides

		[UniversalCopyRelatedEntity]
		public DocumentaryOverrides DocumentaryOverrides
		{
			get => documentaryOverrides ?? (documentaryOverrides = new DocumentaryOverrides(this));
			set
			{
				if (value?.Parent != this)
				{
					documentaryOverrides = new DocumentaryOverrides(this);
					documentaryOverrides.ImportOverrides(value.Parent);
				}
				else
				{
					documentaryOverrides = value;
				}
			}
		}

		DocumentaryOverrides documentaryOverrides;

		#endregion

		#region ScheduleChooser

		public ScheduleChooser ScheduleChooser
		{
			get
			{
				if (scheduleChooser == null || scheduleChooser.Parent is NullSailingChooserParent)
				{
					if (Booking == null && scheduleChooser == null)
					{
#pragma warning disable 0618
						//Needed for binding
						scheduleChooser = new ScheduleChooser(Factory);
#pragma warning restore 0618
					}
					else if (Booking != null)
					{
						scheduleChooser = scheduleChooserCreator != null ? scheduleChooserCreator.Create(this) : new ScheduleChooser(this);
						RegisterEditableChildObject(ScheduleChooser);
					}
				}

				return scheduleChooser;
			}
		}

		ScheduleChooser scheduleChooser;

		readonly IScheduleChooserCreator scheduleChooserCreator;

		#endregion

		#endregion

		#region State

		public QuotedBookingState ObjectState
		{
			get
			{
				QuotedBookingState state = QuotedBookingState.None;

				if (Quote != null && Booking != null)
				{
					state = (Quote.TH_Accepted.IsEmpty) ? QuotedBookingState.UnacceptedBookingWithQuote : QuotedBookingState.AcceptedBookingWithQuote;
				}
				else if (Quote != null)
				{
					state = QuotedBookingState.QuoteOnly;
				}
				else if (Booking != null)
				{
					state = QuotedBookingState.BookingOnly;
				}

				return state;
			}
		}

		internal ZBool GetFromBooking
		{
			get { return ObjectState == QuotedBookingState.BookingOnly || ObjectState == QuotedBookingState.AcceptedBookingWithQuote || ObjectState == QuotedBookingState.UnacceptedBookingWithQuote; }
		}

		ZBool SetOnQuote
		{
			get { return ObjectState == QuotedBookingState.QuoteOnly || ObjectState == QuotedBookingState.AcceptedBookingWithQuote; }
		}

		public ZBool ValidateQuoteProperty
		{
			get { return SetOnQuote; }
		}

		public ZBool ValidateBookingProperty
		{
			get { return GetFromBooking; }
		}

		#endregion

		#region Simple Properties

		public bool IsOneOffQuote => !quotePK.IsEmpty && bookingPK.IsEmpty;

		#region IsForwardRegistered

		public ZBool IsForwardRegistered
		{
			get { return Booking != null ? Booking.JS_IsForwardRegistered : ZBool.False; }
		}

		public ZPropertyInfo IsForwardRegisteredInfo
		{
			get { return GetZPropertyInfo(Schema.IsForwardRegistered); }
		}

		#endregion

		#region QuotedBookingNumber

		public ZString QuotedBookingNumber
		{
			get
			{
				switch (ObjectState)
				{
					case QuotedBookingState.QuoteOnly:
					case QuotedBookingState.UnacceptedBookingWithQuote:
					case QuotedBookingState.AcceptedBookingWithQuote:
						return Quote.TH_QuoteNumber;

					case QuotedBookingState.BookingOnly:
						return Booking.JS_UniqueConsignRef;
					default:
						return ZString.Empty;
				}
			}
			set
			{
				switch (ObjectState)
				{
					case QuotedBookingState.QuoteOnly:
					case QuotedBookingState.UnacceptedBookingWithQuote:
					case QuotedBookingState.AcceptedBookingWithQuote:
						Quote.TH_QuoteNumber = value;
						break;
					case QuotedBookingState.BookingOnly:
						Booking.JS_UniqueConsignRef = value;
						break;
					default:
						break;
				}
			}
		}

		#endregion

		#region ConsignorNameOrPK

		[MaxLength(3)]
		public ZString ConsignorFieldType
		{
			get
			{
				return ConsignorDocumentaryAddress.E2_AddressOverride
					? nameof(FieldType.Text)
					: nameof(FieldType.OrganisationGuid);
			}
		}

		public ZPropertyInfo ConsignorFieldTypeInfo
		{
			get { return GetZPropertyInfo(Schema.ConsignorFieldType); }
		}

		[BusinessObjectTestExclude]
		[RelatedBusinessObject("Consignor")]
		[List("Booking+Lookups+ConsignorForwarder_List")]
		public ZString ConsignorNameOrPK
		{
			get { return ConsignorDocumentaryAddress.OrganisationNameOrPK; }
		}

		public ZPropertyInfo ConsignorNameOrPKInfo
		{
			get { return GetZPropertyInfo(Schema.ConsignorNameOrPK); }
		}

		#endregion

		#region ConsigneeNameOrPK

		[MaxLength(3)]
		public ZString ConsigneeFieldType
		{
			get
			{
				return ConsigneeDocumentaryAddress.E2_AddressOverride
				? nameof(FieldType.Text)
				: nameof(FieldType.OrganisationGuid);
			}
		}

		public ZPropertyInfo ConsigneeFieldTypeInfo
		{
			get { return GetZPropertyInfo(Schema.ConsigneeFieldType); }
		}

		[BusinessObjectTestExclude]
		[RelatedBusinessObject("Consignee")]
		[List("Booking+Lookups+ConsigneeForwarder_List")]
		public ZString ConsigneeNameOrPK
		{
			get { return ConsigneeDocumentaryAddress.OrganisationNameOrPK; }
		}

		public ZPropertyInfo ConsigneeNameOrPKInfo
		{
			get { return GetZPropertyInfo(Schema.ConsigneeNameOrPK); }
		}

		#endregion

		#region ConsignorContact

		public ZString ConsignorContact
		{
			get { return ConsignorDocumentaryAddress.E2_Contact; }
		}

		public ZPropertyInfo ConsignorContactInfo
		{
			get { return GetZPropertyInfo(Schema.ConsignorContact); }
		}

		#endregion

		#region ConsigneeContact

		public ZString ConsigneeContact
		{
			get { return ConsigneeDocumentaryAddress.E2_Contact; }
		}

		public ZPropertyInfo ConsigneeContactInfo
		{
			get { return GetZPropertyInfo(Schema.ConsigneeContact); }
		}

		#endregion

		#region ClientContact

		public ZString ClientContact
		{
			get { return ClientDocAddress.E2_Contact; }
		}

		public ZPropertyInfo ClientContactInfo
		{
			get { return GetZPropertyInfo(Schema.ClientContact); }
		}

		#endregion

		#region Packs

		public ZInt Packs
		{
			get { return GetFromBooking ? Booking.JS_OuterPacks : GetQuoteTotalLoosePackCount(); }
		}

		public ZPropertyInfo PacksInfo
		{
			get { return GetZPropertyInfo(Schema.Packs); }
		}

		#endregion

		#region ContainerCount

		public ZInt ContainerCount
		{
			get { return GetFromBooking ? GetQuotedBookingContainersTotalContainerCount(cont => true) : GetQuoteContainerCount(); }
		}

		public ZPropertyInfo ContainerCountInfo
		{
			get { return GetZPropertyInfo(Schema.ContainerCount); }
		}

		#endregion

		#region ContainerCount20GP

		public ZInt ContainerCount20GP
		{
			get { return GetFromBooking ? GetQuotedBookingContainersTotalContainerCount(cont => cont.JC_Is20GP) : GetQuoteContainerCountByType(cont => cont.Container.Is20GP); }
		}

		public ZPropertyInfo ContainerCount20GPInfo
		{
			get { return GetZPropertyInfo(Schema.ContainerCount20GP); }
		}

		#endregion

		#region ContainerCount20RE

		public ZInt ContainerCount20RE
		{
			get { return GetFromBooking ? GetQuotedBookingContainersTotalContainerCount(cont => cont.JC_Is20RE) : GetQuoteContainerCountByType(cont => cont.Container.Is20RE); }
		}

		public ZPropertyInfo ContainerCount20REInfo
		{
			get { return GetZPropertyInfo(Schema.ContainerCount20RE); }
		}

		#endregion

		#region ContainerCount40GP

		public ZInt ContainerCount40GP
		{
			get { return GetFromBooking ? GetQuotedBookingContainersTotalContainerCount(cont => cont.JC_Is40GP) : GetQuoteContainerCountByType(cont => cont.Container.Is40GP); }
		}

		public ZPropertyInfo ContainerCount40GPInfo
		{
			get { return GetZPropertyInfo(Schema.ContainerCount40GP); }
		}

		#endregion

		#region ContainerCount40RE

		public ZInt ContainerCount40RE
		{
			get { return GetFromBooking ? GetQuotedBookingContainersTotalContainerCount(cont => cont.JC_Is40RE) : GetQuoteContainerCountByType(cont => cont.Container.Is40RE); }
		}

		public ZPropertyInfo ContainerCount40REInfo
		{
			get { return GetZPropertyInfo(Schema.ContainerCount40RE); }
		}

		#endregion

		#region ContainerCountOther

		public ZInt ContainerCountOther
		{
			get { return GetFromBooking ? GetQuotedBookingContainersTotalContainerCount(cont => cont.JC_IsOtherContainerType) : GetQuoteContainerCountByType(cont => cont.Container.IsOtherContainerType); }
		}

		public ZPropertyInfo ContainerCountOtherInfo
		{
			get { return GetZPropertyInfo(Schema.ContainerCountOther); }
		}

		#endregion

		#region GetQuotedBookingContainersTotalContainerCount

		ZInt GetQuotedBookingContainersTotalContainerCount(Func<ForwardingContainer, bool> appropriateContainerType)
		{
			return QuotedBookingContainers.Cast<ForwardingContainer>().Where(appropriateContainerType).Sum(cont => cont.JC_Calc_ContainerCount);
		}

		#endregion

		#region TEUCount

		[DecimalPlaces(2)]
		public ZDecimal TEUCount
		{
			get { return GetFromBooking ? GetQuotedBookingContainersTEUCount() : GetQuoteTEUCount(); }
		}

		public ZPropertyInfo TEUCountInfo
		{
			get { return GetZPropertyInfo(Schema.TEUCount); }
		}

		ZDecimal GetQuotedBookingContainersTEUCount()
		{
			return QuotedBookingContainers.Cast<ForwardingContainer>().Sum(cont => cont.JC_Calc_TEUCount);
		}

		#endregion

		#region PacksType

		public ZString PacksType
		{
			get { return GetFromBooking ? Booking.JS_F3_NKPackType : GetQuotePackType(); }
		}

		public ZPropertyInfo PacksTypeInfo
		{
			get { return GetZPropertyInfo(Schema.PacksType); }
		}

		#endregion

		#region GoodsDescription

		public ZString GoodsDescription
		{
			get { return (this as IBuyerSupplierRelationshipConsumer).GoodsDescription; }
		}

		public ZPropertyInfo GoodsDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.GoodsDescription); }
		}

		#endregion

		#region Weight

		[ResourceStringData("ViewQuotedBooking|Weight", Caption = "Weight", FullDescription = "The weight of this movement.")]
		[MeasureUnit(Schema.WeightUnit, MeasureUnitType.Weight)]
		public ZDecimal Weight
		{
			get { return new ZDecimal(GetThroughState(RateOneOffShipmentSchema.TT_ActualWeight, JobShipmentSchema.JS_ActualWeight)); }
			set
			{
				var roundedValue = this.GetRoundedValue(RateOneOffShipmentSchema.TT_ActualWeight, WeightInfo, value);

				SetThroughState(RateOneOffShipmentSchema.TT_ActualWeight, JobShipmentSchema.JS_ActualWeight, roundedValue);
				WeightInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo WeightInfo
		{
			get { return GetZPropertyInfo(Schema.Weight); }
		}

		#endregion

		#region WeightUnit

		[List("UnitOfWeightList")]
		[MaxLength(CommonShipment.Schema.JS_UnitOfWeightMaxLength)]
		public ZString WeightUnit
		{
			get { return new ZString(GetThroughState(RateOneOffShipmentSchema.TT_UnitOfWeight, JobShipmentSchema.JS_UnitOfWeight)); }
			set
			{
				SetThroughState(RateOneOffShipmentSchema.TT_UnitOfWeight, JobShipmentSchema.JS_UnitOfWeight, value);

				var roundedValue = this.GetRoundedValue(RateOneOffShipmentSchema.TT_ActualWeight, WeightInfo, Weight);
				SetThroughState(RateOneOffShipmentSchema.TT_ActualWeight, JobShipmentSchema.JS_ActualWeight, roundedValue);

				WeightUnitInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo WeightUnitInfo
		{
			get { return GetZPropertyInfo(Schema.WeightUnit); }
		}

		#endregion

		#region Volume

		[ResourceStringData("ViewQuotedBooking|Volume", Caption = "Volume", FullDescription = "The volume of this movement.")]
		[MeasureUnit(Schema.VolumeUnit, MeasureUnitType.Volume)]
		public ZDecimal Volume
		{
			get { return new ZDecimal(GetThroughState(RateOneOffShipmentSchema.TT_ActualVolume, JobShipmentSchema.JS_ActualVolume)); }
			set
			{
				var roundedValue = this.GetRoundedValue(RateOneOffShipmentSchema.TT_ActualVolume, VolumeInfo, value);

				SetThroughState(RateOneOffShipmentSchema.TT_ActualVolume, JobShipmentSchema.JS_ActualVolume, roundedValue);
				VolumeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo VolumeInfo
		{
			get { return GetZPropertyInfo(Schema.Volume); }
		}

		#endregion

		#region VolumeUnit

		[List("UnitOfVolumeList")]
		[MaxLength(CommonShipment.Schema.JS_UnitOfVolumeMaxLength)]
		public ZString VolumeUnit
		{
			get { return new ZString(GetThroughState(RateOneOffShipmentSchema.TT_UnitOfVolume, JobShipmentSchema.JS_UnitOfVolume)); }
			set
			{
				SetThroughState(RateOneOffShipmentSchema.TT_UnitOfVolume, JobShipmentSchema.JS_UnitOfVolume, value);

				var roundedValue = this.GetRoundedValue(RateOneOffShipmentSchema.TT_ActualVolume, VolumeInfo, Volume);
				SetThroughState(RateOneOffShipmentSchema.TT_ActualVolume, JobShipmentSchema.JS_ActualVolume, roundedValue);

				VolumeUnitInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo VolumeUnitInfo
		{
			get { return GetZPropertyInfo(Schema.VolumeUnit); }
		}

		#endregion

		#region Chargeable

		[ResourceStringData("ViewQuotedBooking|Chargeable", Caption = "Chargeable", FullDescription = "The Chargeable Weight/Volume for this movement.")]
		public ZDecimal Chargeable
		{
			get { return new ZDecimal(GetThroughState(RateOneOffShipmentSchema.TT_Chargeable, JobShipmentSchema.JS_ActualChargeable)); }
			set
			{
				var roundedValue = this.GetRoundedValue(RateOneOffShipmentSchema.TT_Chargeable, ChargeableInfo, value);

				SetThroughState(RateOneOffShipmentSchema.TT_Chargeable, JobShipmentSchema.JS_ActualChargeable, roundedValue);
				ChargeableInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ChargeableInfo
		{
			get { return GetZPropertyInfo(Schema.Chargeable); }
		}

		#endregion

		#region LoadPort

		[List("LoadPortLocations")]
		[MaxLength(CommonShipment.Schema.JS_RL_NKLoadPortMaxLength)]
		[ResourceStringData("ViewQuotedBooking|LoadPort", Caption = "Load Port", FullDescription = "The Loading Port of this booking.")]
		public ZString LoadPort
		{
			get { return new ZString(GetThroughState(null, JobShipmentSchema.JS_RL_NKLoadPort)); }
			set
			{
				var previousValue = LoadPort;

				SetThroughState(null, JobShipmentSchema.JS_RL_NKLoadPort, value);
				LoadPortInfo.RefreshBinding();
				if (value != previousValue)
				{
					UpdateQuotedBookingCO2eStatusToNotCurrent(new CO2eStatusChangedReason(LoadPortInfo, previousValue));
				}

				if (!LoadPort.IsEmpty &&
					LoadPort != Origin &&
					(Origin.IsEmpty || Origin.Length < 5))
				{
					Origin = LoadPort;
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateLoadPort();
					Validation.ValidateAllocationLinePK();
				}

				MarkContainersAsNeedingValidation();
			}
		}

		void MarkContainersAsNeedingValidation()
		{
			foreach (var container in QuotedBookingContainers.OfType<ForwardingContainer>())
			{
				container.MarkAsNeedingValidation();
			}
		}

		public ZPropertyInfo LoadPortInfo => GetZPropertyInfo(Schema.LoadPort);

		public RefUNLOCO LoadPortUNLOCO => Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, LoadPort);

		#endregion

		#region DischargePort

		[List("DischargePortLocations")]
		[MaxLength(CommonShipment.Schema.JS_RL_NKDischargePortMaxLength)]
		[ResourceStringData("ViewQuotedBooking|DischargePort", Caption = "Discharge Port", FullDescription = "The Discharge Port of this booking.")]
		public ZString DischargePort
		{
			get { return new ZString(GetThroughState(null, JobShipmentSchema.JS_RL_NKDischargePort)); }
			set
			{
				var previousValue = DischargePort;

				SetThroughState(null, JobShipmentSchema.JS_RL_NKDischargePort, value);
				DischargePortInfo.RefreshBinding();
				if (value != previousValue)
				{
					UpdateQuotedBookingCO2eStatusToNotCurrent(new CO2eStatusChangedReason(DischargePortInfo, previousValue));
				}

				if (!DischargePort.IsEmpty &&
					DischargePort != Destination &&
					(Destination.IsEmpty || Destination.Length < 5))
				{
					Destination = DischargePort;
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateDischargePort();
					Validation.ValidateAllocationLinePK();
				}

				MarkContainersAsNeedingValidation();
			}
		}

		public ZPropertyInfo DischargePortInfo => GetZPropertyInfo(Schema.DischargePort);

		public RefUNLOCO DischargePortUNLOCO => Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, DischargePort);

		#endregion

		#region Origin

		public bool PrevIsCrossTrade;

		public RefUNLOCO OriginUNLOCO
		{
			get { return Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, Origin); }
		}

		[List("ReceivalLocations")]
		[MaxLength(CommonShipment.Schema.JS_RL_NKOriginMaxLength)]
		[ResourceStringData("ViewQuotedBooking|Origin", Caption = "Origin", FullDescription = "The origin for this registration.")]
		public ZString Origin
		{
			get { return new ZString(GetThroughState(RateOneOffShipmentSchema.TT_RL_NKReceivalLocation, JobShipmentSchema.JS_RL_NKOrigin)); }
			set
			{
				var previousValue = Origin;

				SetThroughState(RateOneOffShipmentSchema.TT_RL_NKReceivalLocation, JobShipmentSchema.JS_RL_NKOrigin, value);
				OriginInfo.RefreshBinding();
				IsDomesticFreightInfo.RefreshBinding();
				if (value != previousValue)
				{
					UpdateQuotedBookingCO2eStatusToNotCurrent(new CO2eStatusChangedReason(OriginInfo, previousValue));
				}
			}
		}

		public ZPropertyInfo OriginInfo
		{
			get { return GetZPropertyInfo(Schema.Origin); }
		}

		public bool Origin_ReadOnly
		{
			get;
			set;
		}

		#endregion

		#region Creditor

		[List("Creditors")]
		public ZGuid Creditor
		{
			get { return new ZGuid(GetThroughState(RateOneOffShipmentSchema.TT_OH_Creditor, JobShipmentSchema.JS_OH_Creditor)); }
			set
			{
				SetThroughState(RateOneOffShipmentSchema.TT_OH_Creditor, JobShipmentSchema.JS_OH_Creditor, value);
				CreditorInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CreditorInfo
		{
			get { return GetZPropertyInfo(Schema.Creditor); }
		}

		public bool Creditor_ReadOnly { get; set; }

		#endregion

		#region Destination

		public RefUNLOCO DestinationUNLOCO
		{
			get { return Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, Destination); }
		}

		[List("DeliveryLocations")]
		[MaxLength(CommonShipment.Schema.JS_RL_NKDestinationMaxLength)]
		[ResourceStringData("ViewQuotedBooking|Destination", Caption = "Destination", FullDescription = "The destination for this registration.")]
		public ZString Destination
		{
			get { return new ZString(GetThroughState(RateOneOffShipmentSchema.TT_RL_NKDeliveryLocation, JobShipmentSchema.JS_RL_NKDestination)); }
			set
			{
				var previousValue = Destination;

				SetThroughState(RateOneOffShipmentSchema.TT_RL_NKDeliveryLocation, JobShipmentSchema.JS_RL_NKDestination, value);
				DestinationInfo.RefreshBinding();
				IsDomesticFreightInfo.RefreshBinding();

				if (value != previousValue)
				{
					UpdateQuotedBookingCO2eStatusToNotCurrent(new CO2eStatusChangedReason(DestinationInfo, previousValue));
				}
			}
		}

		public ZPropertyInfo DestinationInfo
		{
			get { return GetZPropertyInfo(Schema.Destination); }
		}

		public bool Destination_ReadOnly
		{
			get;
			set;
		}

		#endregion

		#region ETD

		[ResourceStringData("ViewQuotedBooking|ETD", ShortCaption = "ETD", Caption = "Estimated Time of Departure", FullDescription = "The estimated date of departure.")]
		public ZDateTime ETD
		{
			get => new ZDateTime(GetThroughState(null, JobShipmentSchema.JS_E_DEP));
			set
			{
				SetThroughState(null, JobShipmentSchema.JS_E_DEP, value);
				ETDInfo.RefreshBinding();

				if (!IsValidationSuspended)
				{
					Validation.ValidateETD();
					Validation.ValidateCarrierContractNumber();
					Validation.ValidateAllocationLinePK();
				}

				MarkContainersAsNeedingValidation();
			}
		}

		public ZPropertyInfo ETDInfo => GetZPropertyInfo(Schema.ETD);

		#endregion

		#region ETA

		[ResourceStringData("ViewQuotedBooking|ETA", ShortCaption = "ETA", Caption = "Estimated Time of Arrival", FullDescription = "The estimated date of arrival.")]
		public ZDateTime ETA
		{
			get => new ZDateTime(GetThroughState(null, JobShipmentSchema.JS_E_ARV));
			set
			{
				SetThroughState(null, JobShipmentSchema.JS_E_ARV, value);
				ETAInfo.RefreshBinding();

				if (!IsValidationSuspended)
				{
					Validation.ValidateETA();
				}
			}
		}

		public ZPropertyInfo ETAInfo => GetZPropertyInfo(Schema.ETA);

		#endregion

		#region Company Tariff Level

		[ReadOnlyMember(nameof(IsCompanyTariffLevelReadonly))]
		[List("CompanyTariffLevelOverrideList")]
		[ResourceStringData("ViewQuotedBooking|CompanyTariffLevel", ShortCaption = "Tariff", MediumCaption = "Comp Tariff", Caption = "Company Tariff Level Override", FullDescription = "Company Tariff Level to override.")]
		[BusinessObjectTestExclude]
		public ZString CompanyTariffLevel
		{
			get
			{
				var companyTariffLevelOverride = new ZByte(GetThroughState(RateOneOffShipmentSchema.TT_CompanyTariffLevelOverride, JobShipmentSchema.JS_CompanyTariffLevelOverride));
				if (companyTariffLevelOverride != 0)
				{
					return companyTariffLevelOverride.ToString();
				}
				return ZString.Empty;
			}
			set
			{
				if (CompanyTariffLevel != value)
				{
					ZByte.TryParse(value, out var companyTariffLevelOverride);

					SetThroughState(RateOneOffShipmentSchema.TT_CompanyTariffLevelOverride, JobShipmentSchema.JS_CompanyTariffLevelOverride, companyTariffLevelOverride);
					CompanyTariffLevelInfo.RefreshBinding();
					Validation.ValidateCompanyTariffLevel();
				}
			}
		}

		protected bool IsCompanyTariffLevelReadonly => !DataRegistryRating.Instance.AllowOverrideCompanyTariffLevel.Value && !(ClientDocAddress?.E2_AddressOverride ?? false);

		public ZPropertyInfo CompanyTariffLevelInfo
		{
			get { return GetZPropertyInfo(Schema.CompanyTariffLevel); }
		}

		#endregion

		#region ServiceLevel

		[List("ServiceLevelOrTransitTimeCollection")]
		[MaxLength(CommonShipment.Schema.JS_RS_NKServiceLevelMaxLength)]
		[ResourceStringData("ViewQuotedBooking|ServiceLevel", ShortCaption = "Svc. Lvl.", Caption = "Service Level", FullDescription = "The Service Level of this registration.")]
		public ZString ServiceLevel
		{
			get { return new ZString(GetThroughState(RateOneOffShipmentSchema.TT_RS_NKServiceLevel, JobShipmentSchema.JS_RS_NKServiceLevel)); }
			set
			{
				SetThroughState(RateOneOffShipmentSchema.TT_RS_NKServiceLevel, JobShipmentSchema.JS_RS_NKServiceLevel, value);

				if (!value.IsEmpty)
				{
					IsCompareServiceLevel = false;
				}

				ServiceLevelInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ServiceLevelInfo
		{
			get { return GetZPropertyInfo(Schema.ServiceLevel); }
		}

		public bool ServiceLevel_ReadOnly
		{
			get;
			set;
		}

		#endregion

		#region PaymentTerms

		public ZString PaymentTermLabel
		{
			get { return IsDomesticFreight ? Res.GetString("QuotedBooking|PaymentLabel|PaytTerm", "Payment Term") : Res.GetString("QuotedBooking|PaymentLabel|INCOTerm", "Incoterm"); }
		}

		public ZPropertyInfo PaymentTermLabelInfo
		{
			get { return GetZPropertyInfo(nameof(PaymentTermLabel)); }
		}

		[List("IncoTerms")]
		[MaxLength(CommonShipment.Schema.JS_INCOMaxLength)]
		public ZString PaymentTerms
		{
			get { return new ZString(GetThroughState(RateOneOffShipmentSchema.TT_IncoTerm, JobShipmentSchema.JS_INCO)); }
			set
			{
				if (PaymentTerms != value)
				{
					SetThroughState(RateOneOffShipmentSchema.TT_IncoTerm, JobShipmentSchema.JS_INCO, value);

					if (!IsValidationSuspended)
					{
						Validation.ValidatePaymentTerms();
					}
					PaymentTermsInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo PaymentTermsInfo
		{
			get { return GetZPropertyInfo(Schema.PaymentTerms); }
		}

		#endregion

		#region Container Pack Mode Override

		[List("HBLDeliveryModes")]
		[MaxLength(CommonShipment.Schema.JS_HBLContainerPackModeOverrideMaxLength)]
		[ResourceStringData("ViewQuotedBooking|ContainerPackModeOverride", ShortCaption = "HBL Dlv. Mode", Caption = "HBL Delivery Mode")]
		public ZString ContainerPackModeOverride
		{
			get { return Booking != null ? Booking.JS_HBLContainerPackModeOverride : Quote?.CurrentOneOffQuote?.TT_HBLDeliveryMode ?? ZString.Empty; }
			set
			{
				if (GetFromBooking && Booking != null && Booking.JS_HBLContainerPackModeOverride != value)
				{
					Booking.JS_HBLContainerPackModeOverride = value;
				}

				if (SetOnQuote && Quote != null && Quote.CurrentOneOffQuote != null && Quote.CurrentOneOffQuote.TT_HBLDeliveryMode != value)
				{
					Quote.CurrentOneOffQuote.TT_HBLDeliveryMode = value;
				}

				ContainerPackModeOverrideInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ContainerPackModeOverrideInfo
		{
			get
			{
				return GetWrappedZPropertyInfo(Schema.ContainerPackModeOverride, (x) =>
				{
					if (Booking != null)
					{
						return Booking.JS_HBLContainerPackModeOverrideInfo;
					}
					if (Quote != null)
					{
						return Quote.CurrentOneOffQuote.TT_HBLDeliveryModeInfo;
					}
					return GetZPropertyInfo(Schema.ContainerPackModeOverride);
				});
			}
		}

		#endregion

		#region Delivery Due Date

		[BusinessObjectTestExclude]
		[ResourceStringData("ViewQuotedBooking|DeliveryDueDate", Caption = "Delivery Due Date")]
		public ZDateTime DeliveryDueDate
		{
			get { return new ZDateTime(GetThroughState(null, JobShipmentSchema.JS_DeliveryDueDate)); }
			set
			{
				if (Booking != null)
				{
					SetThroughState(null, JobShipmentSchema.JS_DeliveryDueDate, value);
					DeliveryDueDateInfo.RefreshBinding();

					if (!IsValidationSuspended)
					{
						Validation.ValidateDeliveryDueDate();
					}
				}
			}
		}

		public ZPropertyInfo DeliveryDueDateInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.DeliveryDueDate, (x) => Booking != null ? Booking.JS_DeliveryDueDateInfo : GetZPropertyInfo(Schema.DeliveryDueDate)); }
		}

		public bool DeliveryDueDate_ReadOnly => !Env.Security.QuickBookingDeliveryDueDateOverride.IsAllowed;

		#endregion

		#region Revised Delivery Due Date

		[BusinessObjectTestExclude]
		[ResourceStringData("ViewQuotedBooking|RevisedDeliveryDueDate", Caption = "Revised Delivery Due Date")]
		public ZDateTimeOffset RevisedDeliveryDueDate
		{
			get { return new ZDateTimeOffset(GetThroughState(null, JobShipmentSchema.JS_RevisedDeliveryDueDate)); }
			set
			{
				if (Booking != null)
				{
					SetThroughState(null, JobShipmentSchema.JS_RevisedDeliveryDueDate, value);
					RevisedDeliveryDueDateInfo.RefreshBinding();

					if (!IsValidationSuspended)
					{
						Validation.ValidateDeliveryDueDate();
					}
				}
			}
		}

		public ZPropertyInfo RevisedDeliveryDueDateInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.RevisedDeliveryDueDate, (x) => Booking != null ? Booking.JS_RevisedDeliveryDueDateInfo : GetZPropertyInfo(Schema.RevisedDeliveryDueDate)); }
		}

		public bool RevisedDeliveryDueDate_ReadOnly => !Env.Security.QuickBookingDeliveryDueDateOverride.IsAllowed;

		#endregion

		#region AdditionalTerms

		[ResourceStringData("ViewQuotedBooking|AdditionalTerms", ShortCaption = "Add. Terms", Caption = "Additional Terms", FullDescription = "Additional Incoterms information (for example, agreed payment location, parties responsible etc.).")]
		public ZString AdditionalTerms
		{
			get { return new ZString(GetThroughState(RateOneOffShipmentSchema.TT_AdditionalTerms, JobShipmentSchema.JS_AdditionalTerms)); }
			set
			{
				SetThroughState(RateOneOffShipmentSchema.TT_AdditionalTerms, JobShipmentSchema.JS_AdditionalTerms, value);
				AdditionalTermsInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo AdditionalTermsInfo => GetWrappedZPropertyInfo(Schema.AdditionalTerms, (x) => GetAdditionalTermsPropertyInfo());

		ZPropertyInfo GetAdditionalTermsPropertyInfo()
		{
			ZPropertyInfo propertyInfo = null;

			switch (ObjectState)
			{
				case QuotedBookingState.QuoteOnly:
					if (Quote?.CurrentOneOffQuote != null)
					{
						propertyInfo = Quote.CurrentOneOffQuote.TT_AdditionalTermsInfo;
					}
					break;
				case QuotedBookingState.BookingOnly:
				case QuotedBookingState.UnacceptedBookingWithQuote:
				case QuotedBookingState.AcceptedBookingWithQuote:
					if (Booking != null)
					{
						propertyInfo = Booking.JS_AdditionalTermsInfo;
					}
					break;
			}

			if (propertyInfo == null)
			{
				propertyInfo = GetZPropertyInfo(Schema.AdditionalTerms);
			}

			return propertyInfo;
		}

		public bool AdditionalTerms_ReadOnly { get; set; }

		#endregion

		#region GoodsValue

		[ResourceStringData("ViewQuotedBooking|GoodsValue", ShortCaption = "Goods Val", Caption = "Goods Value", FullDescription = "The value of goods for this quotation/movement.")]
		[DecimalPlaces(2)]
		public ZDecimal GoodsValue
		{
			get { return new ZDecimal(GetThroughState(RateOneOffShipmentSchema.TT_ValueOfGoods, JobShipmentSchema.JS_GoodsValue)); }
			set
			{
				SetThroughState(RateOneOffShipmentSchema.TT_ValueOfGoods, JobShipmentSchema.JS_GoodsValue, value);
				GoodsValueInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo GoodsValueInfo
		{
			get { return GetZPropertyInfo(Schema.GoodsValue); }
		}

		#endregion

		#region GoodsCurrency

		public virtual RefCurrency GoodsValueCurr
		{
			get { return Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, GoodsCurrency); }
		}

		[List("Currencies")]
		[MaxLength(CommonShipment.Schema.JS_RX_NKGoodsValueCurrMaxLength)]
		public ZString GoodsCurrency
		{
			get { return new ZString(GetThroughState(RateOneOffShipmentSchema.TT_RX_NKGoodsCurrency, JobShipmentSchema.JS_RX_NKGoodsValueCurr)); }
			set
			{
				SetThroughState(RateOneOffShipmentSchema.TT_RX_NKGoodsCurrency, JobShipmentSchema.JS_RX_NKGoodsValueCurr, value);
				GoodsCurrencyInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo GoodsCurrencyInfo
		{
			get { return GetZPropertyInfo(Schema.GoodsCurrency); }
		}

		#endregion

		#region InsuranceValue

		[ResourceStringData("ViewQuotedBooking|InsuranceValue", ShortCaption = "Ins Value", Caption = "Insurance Value", FullDescription = "The value of goods for insurance purposes.")]
		[DecimalPlaces(2)]
		public ZDecimal InsuranceValue
		{
			get { return new ZDecimal(GetThroughState(RateOneOffShipmentSchema.TT_InsureVal, JobShipmentSchema.JS_InsuranceValue)); }
			set
			{
				SetThroughState(RateOneOffShipmentSchema.TT_InsureVal, JobShipmentSchema.JS_InsuranceValue, value);
				InsuranceValueInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo InsuranceValueInfo
		{
			get { return GetZPropertyInfo(Schema.InsuranceValue); }
		}

		#endregion

		#region InsuranceCurrency

		[List("Currencies")]
		[MaxLength(CommonShipment.Schema.JS_RX_NKInsuranceCurrencyMaxLength)]
		public ZString InsuranceCurrency
		{
			get { return new ZString(GetThroughState(RateOneOffShipmentSchema.TT_RX_NKInsureValCurr, JobShipmentSchema.JS_RX_NKInsuranceCurrency)); }
			set
			{
				SetThroughState(RateOneOffShipmentSchema.TT_RX_NKInsureValCurr, JobShipmentSchema.JS_RX_NKInsuranceCurrency, value);
				InsuranceCurrencyInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo InsuranceCurrencyInfo
		{
			get { return GetZPropertyInfo(Schema.InsuranceCurrency); }
		}

		#endregion

		#region PickupEquipment

		[List("Equipments")]
		[MaxLength(JobDocsAndCartage.Schema.JP_FCLPickupEquipmentNeededMaxLength)]
		[ResourceStringData("ViewQuotedBooking|PickupEquipment", ShortCaption = "Pic. Drop", Caption = "Pickup Drop Mode", FullDescription = "The drop mode / equipment to use for pickup purposes.")]
		public ZString PickupEquipment
		{
			get { return new ZString(GetThroughState(RateOneOffShipmentSchema.TT_PickupEquipment, JobDocsAndCartageSchema.JP_FCLPickupEquipmentNeeded)); }
			set
			{
				SetThroughState(RateOneOffShipmentSchema.TT_PickupEquipment, JobDocsAndCartageSchema.JP_FCLPickupEquipmentNeeded, value);
				PickupEquipmentInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo PickupEquipmentInfo
		{
			get { return GetZPropertyInfo(Schema.PickupEquipment); }
		}

		#endregion

		#region DeliveryEquipment

		[List("Equipments")]
		[MaxLength(JobDocsAndCartage.Schema.JP_FCLDeliveryEquipmentNeededMaxLength)]
		[ResourceStringData("ViewQuotedBooking|DeliveryEquipment", ShortCaption = "Dlv. Drop", Caption = "Delivery Drop Mode", FullDescription = "The drop mode / equipment to use for delivery purposes.")]
		public ZString DeliveryEquipment
		{
			get { return new ZString(GetThroughState(RateOneOffShipmentSchema.TT_DeliveryEquipment, JobDocsAndCartageSchema.JP_FCLDeliveryEquipmentNeeded)); }
			set
			{
				SetThroughState(RateOneOffShipmentSchema.TT_DeliveryEquipment, JobDocsAndCartageSchema.JP_FCLDeliveryEquipmentNeeded, value);
				DeliveryEquipmentInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo DeliveryEquipmentInfo
		{
			get { return GetZPropertyInfo(Schema.DeliveryEquipment); }
		}

		#endregion

		#region Implementation

		#region SetThroughState

		void SetThroughState(SchemaColumn quoteProperty, SchemaColumn bookingProperty, object value)
		{
			switch (ObjectState)
			{
				case QuotedBookingState.QuoteOnly:
					SetQuoteProperty(quoteProperty, value);
					break;

				case QuotedBookingState.BookingOnly:
					SetBookingProperty(bookingProperty, value);
					break;

				case QuotedBookingState.UnacceptedBookingWithQuote:
					if (bookingProperty != null)
					{
						SetBookingProperty(bookingProperty, value);
					}
					else
					{
						SetQuoteProperty(quoteProperty, value);
					}
					break;

				case QuotedBookingState.AcceptedBookingWithQuote:
					SetQuoteProperty(quoteProperty, value, bookingProperty == null);
					SetBookingProperty(bookingProperty, value);
					break;
			}
		}

		void SetQuoteProperty(SchemaColumn quoteProperty, object value, bool setHasChanges = false)
		{
			if (quoteProperty == null || Quote == null)
			{
				return;
			}

			if (quoteProperty.TableName == RatingHeader.Schema.TableName)
			{
				if (Quote[quoteProperty.Name] != value)
				{
					Quote[quoteProperty.Name] = value;
					if (setHasChanges)
					{
						HasChanges = true;
					}
				}
			}
			else
			{
				if (Quote.CurrentOneOffQuote[quoteProperty.Name] != value)
				{
					Quote.CurrentOneOffQuote[quoteProperty.Name] = value;
					if (setHasChanges)
					{
						HasChanges = true;
					}
				}
			}
		}

		void SetBookingProperty(SchemaColumn bookingProperty, object value)
		{
			if (bookingProperty == null)
			{
				return;
			}

			if (bookingProperty.TableName == JobDocsAndCartage.Schema.TableName)
			{
				Booking.DocsAndCartage[bookingProperty.Name] = value;
			}
			else
			{
				Booking[bookingProperty.Name] = value;
			}
		}

		#endregion

		#region GetThroughState

		object GetThroughState(SchemaColumn quoteProperty, SchemaColumn bookingProperty)
		{
			object result = null;
			switch (ObjectState)
			{
				case QuotedBookingState.QuoteOnly:
					result = GetQuoteProperty(quoteProperty);
					break;

				case QuotedBookingState.BookingOnly:
				case QuotedBookingState.UnacceptedBookingWithQuote:
				case QuotedBookingState.AcceptedBookingWithQuote:
					result = GetBookingProperty(bookingProperty);
					break;
			}
			return result;
		}

		object GetQuoteProperty(SchemaColumn quoteProperty)
		{
			if (quoteProperty == null || Quote == null)
			{
				return null;
			}

			if (quoteProperty.TableName == RatingHeader.Schema.TableName)
			{
				return Quote[quoteProperty.Name];
			}
			else
			{
				return Quote.CurrentOneOffQuote[quoteProperty.Name];
			}
		}

		object GetBookingProperty(SchemaColumn bookingProperty)
		{
			if (bookingProperty == null)
			{
				return null;
			}

			if (bookingProperty.TableName == JobDocsAndCartage.Schema.TableName)
			{
				return Booking.DocsAndCartage[bookingProperty.Name];
			}
			else
			{
				return Booking[bookingProperty.Name];
			}
		}

		#endregion

		#endregion

		#region ViewPK

		public ZGuid ViewPK
		{
			get { return PK; }
		}

		#endregion

		#region Client

		public OrgAddress ClientAddrForWorkFlow
		{
			get
			{
				var job = GetJobHeaderForWorkflow();
				ZGuid clientAddrPK = GetClientAddrPK(job);

				return Factory.Load<OrgAddress>(clientAddrPK);
			}
		}

		public OrgAddress ClientAddr
		{
			get { return Factory.Load<OrgAddress>(ClientAddrPK); }
		}

		[RelatedBusinessObject("ClientAddr")]
		[List("Clients")]
		public virtual ZGuid ClientAddrPK
		{
			get
			{
				return GetClientAddrPK(Job);
			}
			set
			{
				var address = Factory.Load<OrgAddress>(value);

				if (Job != null)
				{
					Job.JH_OA_LocalChargesAddr = value;
				}

				if (SetOnQuote)
				{
					Quote.QuotationClientAddress.E2_OA_Address = address != null ? address.PK : ZGuid.Empty;
				}

				if (address != null && ConsignorDocumentaryAddress != null && ConsignorDocumentaryAddress.Address == null)
				{
					ConsignorDocumentaryAddress.OrganisationPK = address.Header.PK;
				}

				if (ClientAddrPKInfo != null)
				{
					ClientAddrPKInfo.RefreshBinding();
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateCarrierContractNumber();
					Validation.ValidateAllocationLinePK();
				}

				MarkContainersAsNeedingValidation();
			}
		}

		ZGuid GetClientAddrPK(JobHeader job)
		{
			ZGuid result;
			if (GetFromBooking)
			{
				if (job != null)
				{
					if (OrgRole == Core.Constants.OrgRoles.OverseasAgent)
					{
						result = job.JH_OA_AgentCollectAddr;
					}
					else
					{
						result = job.JH_OA_LocalChargesAddr;
					}
				}
				else
				{
					var org = Factory.Load<OrgHeader>(clientPK);
					result = org != null ? org.MainAddress.PK : ZGuid.Empty;
				}
			}
			else
			{
				result = ClientDocAddress != null ? ClientDocAddress.E2_OA_Address : ZGuid.Empty;
			}

			return result;
		}

		#region Client ZAddress

		public ZAddress ClientAddrPK_ZAddress
		{
			get
			{
				if (clientAddrPK_ZAddress == null)
				{
					clientAddrPK_ZAddress = new ZAddress(ClientAddrPKInfo)
					{
						DefaultAddressType = AddressType.ARM,
						GetDefaultAddress = GetDefaultAddressForClient
					};
				}

				return clientAddrPK_ZAddress;
			}
		}

		ZAddress clientAddrPK_ZAddress;

		static ZGuid GetDefaultAddressForClient(IOrgHeader org)
		{
			OrgAddress result = null;
			var header = org as OrgHeader;
			if (header != null)
			{
				result = header.Addresses.DefaultAddressOfType(OrgAddressType.Receivables)
						?? header.Addresses.DefaultAddressOfType(OrgAddressType.Postal)
						?? header.Addresses.DefaultAddressOfType(OrgAddressType.Office)
						?? header.MainAddress;
			}

			return result == null ? ZGuid.Empty : result.PK;
		}

		#endregion

		public ZPropertyInfo ClientAddrPKInfo
		{
			get
			{
				ZPropertyInfo result;

				if (GetFromBooking)
				{
					result = GetZPropertyInfo(Schema.ClientAddrPK);
				}
				else
				{
					result = ClientDocAddress == null ? null : GetWrappedZPropertyInfo(Schema.ClientAddrPK, x => ClientDocAddress.E2_OA_AddressInfo);
				}

				return result;
			}
		}

		public bool ClientAddrPK_ReadOnly
		{
			get;
			set;
		}

		public ZString ClientFullName
		{
			get
			{
				ZString result = "";
				if (GetFromBooking)
				{
					result = Job != null && Job.LocalCharges != null ? Job.LocalCharges.OH_FullNameTruncated : ZString.Empty;
				}
				else
				{
					result = ClientDocAddress.E2_CompanyNameTruncated;
				}

				return result;
			}
		}

		public ZPropertyInfo ClientFullNameInfo
		{
			get { return GetZPropertyInfo(nameof(ClientFullName)); }
		}

		public OrgHeader Client
		{
			get { return ClientAddr?.Header; }
		}

		[RelatedBusinessObject("Client")]
		[List("Clients")]
		public ZGuid ClientPK
		{
			get { return Client != null ? Client.PK : clientPK; }
			set
			{
				OrgHeader org = Factory.Load<OrgHeader>(value);
				ClientAddrPK = org != null ? org.MainAddress.PK : ZGuid.Empty;
				clientPK = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateClientPK();
				}

				ClientPKInfo.RefreshBinding();
			}
		}

		ZGuid clientPK;

		public ZPropertyInfo ClientPKInfo
		{
			get { return GetZPropertyInfo(Schema.ClientPK); }
		}

		public ResourceStringData PrepaidBillToPartyCaption => Res.GetData("0FF977B3-E0A7-4D2F-A2BF-441C3EA8CD6D", "Prepaid Bill-To Party", "This field defaults the job's Consignor. If Cross Trade jobs are not configured to bill the job's Controlling Party, then the Prepaid Bill-To Party (or their IFT party if they have one) will default as the charge line debtor for prepaid charges on this job.");

		public ResourceStringData ClientCaption => Res.GetData("CF9E0142-76F7-41EF-91FC-C61131FFB03C", "Client");

		public MultilingualString PrepaidBillToPartyText => ResString.GetMultilingualString("EE8F22F5-D4E3-47B0-A4C0-BFC198356297", "Prepaid Bill-To Party");

		public MultilingualString ClientText => ResString.GetMultilingualString("5BF56856-0D1E-4200-B3CC-D38A9FB21573", "Client");

		#endregion

		#region ClientDocAddress

		public JobDocAddress ClientDocAddress
		{
			get
			{
				if (clientDocAddress == null)
				{
					clientDocAddress = Quote != null ? Quote.QuotationClientAddress : null;

					if (clientDocAddress != null)
					{
						clientDocAddress.OrganisationPKInfo.ValueChanged += new EventHandler(OrganisationPKInfo_ValueChanged);
						clientDocAddress.E2_AddressOverrideInfo.ValueChanged += new EventHandler(ClientDocAddressE2_AddressOverrideInfo_ValueChanged);
					}
				}

				return clientDocAddress;
			}
		}
		JobDocAddress clientDocAddress;

		void OrganisationPKInfo_ValueChanged(object sender, EventArgs e)
		{
			if (!ClientPK.IsEmpty
				&& ConsignorDocumentaryAddress.Organisation == null
				&& OrgRole == Core.Constants.OrgRoles.LocalClient)
			{
				ConsignorDocumentaryAddress.OrganisationPK = ClientPK;
			}
			else if (!ClientPK.IsEmpty
				&& Job != null
				&& Job.JH_OA_AgentCollectAddr == ZGuid.Empty
				&& OrgRole == Core.Constants.OrgRoles.OverseasAgent)
			{
				Job.JH_OA_AgentCollectAddr = ClientAddrPK;
			}
		}

		void ClientDocAddressE2_AddressOverrideInfo_ValueChanged(object sender, EventArgs e)
		{
			CompanyTariffLevelInfo.RefreshBinding();
		}

		#endregion

		#region Mode

		[BusinessObjectTestExclude]
		[List("Modes")]
		[MaxLength(RateOneOffShipment.Schema.TT_TransportModeMaxLength)]
		[ResourceStringData("ViewQuotedBooking|Mode", Caption = "Mode", FullDescription = "The Mode of this quotation / booking.")]
		public ZString Mode
		{
			get
			{
				return GetFromBooking
					? GetBookingMode(Booking.JS_TransportMode, Booking.JS_PackingMode)
					: Quote == null
						? ZString.Empty
						: RateOneOffShipment.FreightModeConverter.GetMode(Quote.CurrentOneOffQuote.TT_TransportMode, Quote.CurrentOneOffQuote.TT_ContainerMode);
			}
			set
			{
				if (Mode != value && ModesForWebTracker.CodesAsString.Contains(value))// The set method now is only supported for old Modes, which are in ModesForWebTracker.
				{
					var previousValue = Mode;
					if (GetFromBooking)
					{
						SetModeOnBooking(value);
					}

					if (SetOnQuote)
					{
						Quote.CurrentOneOffQuote.TT_TransportMode = RatingConstants.GetTransportModeFromMode(value);
						Quote.CurrentOneOffQuote.TT_ContainerMode = RatingConstants.GetOneOffQuoteContainerModeFromMode(value);
					}

					if (previousValue != value)
					{
						UpdateQuotedBookingCO2eStatusToNotCurrent(new CO2eStatusChangedReason(ModeInfo, previousValue));
					}
				}
				ModeInfo.RefreshBinding();
			}
		}

		[BusinessObjectTestExclude]
		[List("TransportModes")]
		[MaxLength(RateOneOffShipment.Schema.TT_TransportModeMaxLength)]
		[ResourceStringData("ViewQuotedBooking|TransportMode", Caption = "Transport Mode", ShortCaption = "Transport", FullDescription = "The Transport Mode of this quotation / booking.")]
		public ZString TransportMode
		{
			get
			{
				if (GetFromBooking)
				{
					return Booking?.JS_TransportMode ?? ZString.Empty;
				}

				return Quote?.CurrentOneOffQuote?.TT_TransportMode ?? ZString.Empty;
			}
			set
			{
				if (IsCompareMode)
				{
					return;
				}

				var previousValue = TransportMode;

				if (GetFromBooking && Booking.JS_TransportMode != value)
				{
					Booking.JS_TransportMode = value;
				}

				if (SetOnQuote && Quote.CurrentOneOffQuote.TT_TransportMode != value)
				{
					Quote.CurrentOneOffQuote.TT_TransportMode = value;
				}

				ContainerModeInfo.RefreshBinding();

				ModeInfo.RefreshBinding();

				if (previousValue != value)
				{
					UpdateQuotedBookingCO2eStatusToNotCurrent(new CO2eStatusChangedReason(TransportModeInfo, previousValue));
				}
			}
		}

		[BusinessObjectTestExclude]
		[List("OrgRoles")]
		[MaxLength(RateOneOffShipment.Schema.TT_OrgRoleMaxLength)]
		[ResourceStringData("ViewQuotedBooking|OrgRole", Caption = "Type", FullDescription = "The client role for this quotation / booking.")]
		public ZString OrgRole
		{
			get
			{
				return Quote?.CurrentOneOffQuote?.TT_OrgRole ?? Constants.OrgRoles.LocalClient;
			}
			set
			{
				if (OrgRole != value
					&& Quote?.CurrentOneOffQuote != null)
				{
					Quote.CurrentOneOffQuote.TT_OrgRole = value;
				}
			}
		}

		[BusinessObjectTestExclude]
		[List("ContainerModes")]
		[MaxLength(RateOneOffShipment.Schema.TT_ContainerModeMaxLength)]
		[ResourceStringData("ViewQuotedBooking|ContainerMode", Caption = "Container Mode", ShortCaption = "Container", FullDescription = "The Container Mode of this quotation / booking.")]
		public ZString ContainerMode
		{
			get
			{
				if (TransportMode.IsEmpty)
				{
					return ZString.Empty;
				}
				else if (GetFromBooking)
				{
					return Booking?.JS_PackingMode ?? ZString.Empty;
				}

				return Quote?.CurrentOneOffQuote?.TT_ContainerMode ?? ZString.Empty;
			}
			set
			{
				if (IsCompareMode)
				{
					return;
				}

				originalRequireTEU = (this as ICO2eCalculationSupporter).RequireTEU;
				var previousValue = ContainerMode;

				if (GetFromBooking && Booking.JS_PackingMode != value)
				{
					Booking.JS_PackingMode = value;
				}

				if (SetOnQuote && Quote.CurrentOneOffQuote.TT_ContainerMode != value)
				{
					Quote.CurrentOneOffQuote.TT_ContainerMode = value;
				}

				ModeInfo.RefreshBinding();
				ContainerModeInfo.RefreshBinding();

				if (!this.SkipCO2eStatusCheck())
				{
					var newRequireTEU = (this as ICO2eCalculationSupporter).RequireTEU;
					if (newRequireTEU != originalRequireTEU && previousValue != value)
					{
						UpdateQuotedBookingCO2eStatusToNotCurrent(new CO2eStatusChangedReason(ContainerModeInfo, previousValue));
						originalRequireTEU = newRequireTEU;
					}
				}
			}
		}

		public ZPropertyInfo ModeInfo
		{
			get { return GetZPropertyInfo(Schema.Mode); }
		}

		public bool Mode_ReadOnly
		{
			get;
			set;
		}

		public ZPropertyInfo TransportModeInfo
		{
			get { return GetZPropertyInfo(Schema.TransportMode); }
		}

		public ZPropertyInfo ContainerModeInfo
		{
			get { return GetZPropertyInfo(Schema.ContainerMode); }
		}

		public bool TransportMode_ReadOnly
		{
			get;
			set;
		}

		public bool ContainerMode_ReadOnly
		{
			get;
			set;
		}

		void UpdateCollectionsFromMode()
		{
			if (ReadOnly || IsConsolidated)
			{
				return;
			}

			if (Booking != null)
			{
				if (ContainersShouldBeEnabled)
				{
					QuotedBookingContainers.SetReadOnlyIncludingChildren(false);
				}
				else
				{
					QuotedBookingContainers.SetReadOnlyIncludingChildren(true);
					QuotedBookingContainers.RemoveAndDeleteAll();
				}
			}
		}

		bool ContainersShouldBeEnabled
		{
			get
			{
				string containerMode = ContainerMode;
				if (containerMode == Constants.ContainerModes.ULD
					|| containerMode == Constants.ContainerModes.FCL
					|| containerMode == Constants.ContainerModes.FTL
					|| containerMode == Constants.ContainerModes.ShippersConsol
					|| containerMode == Constants.ContainerModes.BuyersConsol)
				{
					return true;
				}
				else
				{
					return false;
				}
			}
		}

		void SetModeOnBooking(ZString value)
		{
			Booking.JS_TransportMode = RatingConstants.GetTransportModeFromMode(value);
			Booking.JS_PackingMode = RatingConstants.GetContainerModeFromMode(value);
		}

		#region Mode Converters

		ZString GetBookingMode(ZString transportMode, ZString containerMode)
		{
			switch (transportMode)
			{
				case Constants.TransportModes.Road:
					switch (containerMode)
					{
						case Constants.ContainerModes.FCL:
							return Constants.RateMode.FRO;
						case Constants.ContainerModes.LCL:
						case Constants.ContainerModes.LTL:
							return Constants.RateMode.LRO;
						default:
							return containerMode;
					}
				case Constants.TransportModes.Rail:
					switch (containerMode)
					{
						case Constants.ContainerModes.FCL:
							return Constants.RateMode.FRA;
						case Constants.ContainerModes.LCL:
							return Constants.RateMode.LRA;
						default:
							return containerMode;
					}
				case Constants.TransportModes.Courier:
					switch (containerMode)
					{
						case Constants.RateMode.OBC:
							return Constants.RateMode.COU;
						default:
							return containerMode;
					}
				case Constants.TransportModes.Air:
				case Constants.TransportModes.Sea:
				case Constants.TransportModes.SeaAir:
				case Constants.TransportModes.AirSea:
					return containerMode;
				default:
					return ZString.Empty;
			}
		}

		#endregion

		#endregion

		#region ShipmentStatus

		[List("Booking.Lookups+JS_ShipmentStatus_List")]
		[MaxLength(CommonShipment.Schema.JS_ShipmentStatusMaxLength)]
		public ZString ShipmentStatus
		{
			get { return new ZString(GetThroughState(null, JobShipmentSchema.JS_ShipmentStatus)); }
			set
			{
				if (ShipmentStatus != value)
				{
					switch (value)
					{
						case ShipmentStatusList.Codes.Booked:
							{
								if (ShipmentStatus != ShipmentStatusList.Codes.EBookingCancellationRequest)
								{
									LogStatusChangedEvent(value, (NoResString)"Booking Confirmed"); // Event Parameter Constant.
								}
								break;
							}
						case ShipmentStatusList.Codes.EBookingCancellationRequest:
							LogStatusChangedEvent(value, (NoResString)"eBooking Cancell Request"); // Event Parameter Constant.
							break;
						case ShipmentStatusList.Codes.BookingCancelled:
							LogStatusChangedEvent(value, (NoResString)"Booking Cancelled"); // Event Parameter Constant.
							break;
						case ShipmentStatusList.Codes.BookingRejected:
							{
								if (!isBookingBeingSetToInactive)
								{
									var reason = ZString.Empty;
									if (OnHBLBookingStatusUpdate != null)
									{
										var eventArgs = new HBLBookingStatusEventArgs(value);
										OnHBLBookingStatusUpdate(this, eventArgs);
										reason = eventArgs.StatusUpdatedReason;

										if (reason.IsEmpty)
										{
											return;
										}
									}

									LogStatusChangedEvent(value, reason);
								}
								break;
							}
					}

					SetThroughState(null, JobShipmentSchema.JS_ShipmentStatus, value);
					ShipmentStatusInfo.RefreshBinding();

					if (!IsValidationSuspended)
					{
						Validation.ValidateShipmentStatus();
					}
				}
			}
		}

		public void LogStatusChangedEvent(string newValue, string reason)
		{
			var parameters = new KeyValuePair<string, string>[]
				{
					new KeyValuePair<string, string>(Params.New, newValue),
					new KeyValuePair<string, string>(Params.Old, ShipmentStatus),
					new KeyValuePair<string, string>(Params.Reason, GetReasonWithPrefix(newValue, reason)),
					new KeyValuePair<string, string>(Params.Type, (NoResString)"Shipment Status") // Event Parameter Constant.
				};
			Logs.CreateOrRecreateEventLog(Events.StatusUpdated, EstimateActual.Actual, ZDateTimeOffset.Now, String.Empty, parameters);
		}

		ZString GetReasonWithPrefix(ZString newValue, ZString reason)
		{
			if (newValue == ShipmentStatusList.Codes.BookingRejected && !isBookingBeingSetToInactive)
			{
				return reason.IsEmpty ? (NoResString)"Booking Rejected" : (NoResString)"Booking Rejected, " + reason; // Event Parameter Constant.
			}

			return reason;
		}

		public IDisposable SuspendAutomaticCreationOfStatusChangedLog()
		{
			isBookingBeingSetToInactive = true;
			return new DisposableAction(() => isBookingBeingSetToInactive = false);
		}

		bool isBookingBeingSetToInactive;

		public event EventHandler<HBLBookingStatusEventArgs> OnHBLBookingStatusUpdate;

		public ZPropertyInfo ShipmentStatusInfo
		{
			get { return GetZPropertyInfo(Schema.ShipmentStatus); }
		}

		bool ShipmentStatus_ReadOnly
		{
			get => Booking == null
				|| Booking.JS_ShipmentStatus_ReadOnly
				|| ShipmentStatus == ShipmentStatusList.Codes.Booked
				|| ShipmentStatus == ShipmentStatusList.Codes.BookingRejected
				|| ShipmentStatus == ShipmentStatusList.Codes.EBookingCancellationRequest
				|| ShipmentStatus == ShipmentStatusList.Codes.BookingCancelled;
		}

		#endregion

		#region IsDomesticFreight

		[BusinessObjectTestExclude]
		[ResourceStringData("ViewQuotedBooking|IsDomesticFreight", ShortCaption = "Domestic", Caption = "Is Domestic", FullDescription = "Specifies whether this registration is for a domestic freight movement.")]
		public ZBool IsDomesticFreight
		{
			get { return (GetFromBooking) ? Booking.IsDomesticFreight : Quote.CurrentOneOffQuote.IsDomesticFreight; }
			set
			{
				if (GetFromBooking)
				{
					Booking.IsDomesticFreight = value;
				}

				if (SetOnQuote)
				{
					Quote.CurrentOneOffQuote.IsDomesticFreight = value;
				}

				IsDomesticFreightInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo IsDomesticFreightInfo
		{
			get { return GetZPropertyInfo(Schema.IsDomesticFreight); }
		}

		#endregion

		#region ConsignorDocumentaryAddress

		JobDocAddress fConsignorDocumentaryAddress;
		public JobDocAddress ConsignorDocumentaryAddress
		{
			get
			{
				if (fConsignorDocumentaryAddress == null || fConsignorDocumentaryAddress.IsDeleted)
				{
					if (GetFromBooking)
					{
						fConsignorDocumentaryAddress = Booking.ConsignorDocumentaryAddress;
						fConsignorDocumentaryAddress.OrgHeaderAfterChange -= ClientForCarrierContractChanged;
						fConsignorDocumentaryAddress.OrgHeaderAfterChange += ClientForCarrierContractChanged;
					}
					else
					{
						fConsignorDocumentaryAddress = Quote?.CurrentOneOffQuote?.PickUpDocAddress;
					}
				}

				return fConsignorDocumentaryAddress;
			}
		}

		protected bool IsBookingImportToAnyCountry => !Booking.JS_RL_NKOrigin.IsEmpty && !Booking.JS_RL_NKDestination.IsEmpty && Booking.JS_RL_NKOrigin.SubstringSafe(0, 2) != Booking.JS_RL_NKDestination.SubstringSafe(0, 2);

		protected virtual ZGuid? GetDefaultImportBrokerPK()
		{
			if (Booking.Consignee != null)
			{
				OrgHeader relatedParty = Booking.Consignee.GetRelatedParty(RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Delivery, Booking.JS_TransportMode, Booking.JS_PackingMode, Booking.JS_Calc_LastDischargePort.IsEmpty ? Booking.JS_RL_NKDestination : Booking.JS_Calc_LastDischargePort);
				return (relatedParty != null) ? relatedParty.PK : ZGuid.Empty;
			}

			return null;
		}

		public void RestoreImportBrokerFallback()
		{
			if (Booking != null)
			{
				var defaultImportBrokerPK = GetImportBrokerFallback();
				if (defaultImportBrokerPK.HasValue)
				{
					Booking.JS_OH_ImportBroker = defaultImportBrokerPK.Value;
				}
			}
		}

		ZGuid? GetImportBrokerFallback()
		{
			return ((!Booking.IsChangingConsigneeAddress || !Booking.IsInDatabase) && (!Booking.IsChangingConsignorAddress || !Booking.IsInDatabase)) ? GetDefaultImportBrokerPK() : null;
		}

		public OrgHeader Consignor
		{
			get { return (ConsignorDocumentaryAddress == null || !ConsignorDocumentaryAddress.HasRealOrganisation) ? null : ConsignorDocumentaryAddress.Organisation; }
		}

		#endregion

		#region ConsigneeDocumentaryAddress

		JobDocAddress fConsigneeDocumentaryAddress;
		public JobDocAddress ConsigneeDocumentaryAddress
		{
			get
			{
				if (fConsigneeDocumentaryAddress == null || fConsigneeDocumentaryAddress.IsDeleted)
				{
					if (GetFromBooking)
					{
						fConsigneeDocumentaryAddress = Booking.ConsigneeDocumentaryAddress;
						fConsigneeDocumentaryAddress.OrgHeaderAfterChange -= ClientForCarrierContractChanged;
						fConsigneeDocumentaryAddress.OrgHeaderAfterChange += ClientForCarrierContractChanged;
					}
					else
					{
						fConsigneeDocumentaryAddress = Quote.CurrentOneOffQuote.DeliveryDocAddress;
					}
				}
				return fConsigneeDocumentaryAddress;
			}
		}

		void ClientForCarrierContractChanged(object sender, EventArgs e)
		{
			if (!IsValidationSuspended)
			{
				Validation.ValidateCarrierContractNumber();
				Validation.ValidateAllocationLinePK();
			}

			MarkContainersAsNeedingValidation();
		}

		public OrgHeader Consignee
		{
			get { return (ConsigneeDocumentaryAddress == null || !ConsigneeDocumentaryAddress.HasRealOrganisation) ? null : ConsigneeDocumentaryAddress.Organisation; }
		}

		#endregion

		#region BookingPartyDocumentaryAddress

		public JobDocAddress BookingPartyDocumentaryAddress
		{
			get { return Booking != null ? Booking.BookingPartyDocumentaryAddress : null; }
		}

		#endregion

		public OrgHeader ControllingCustomer
		{
			get { return (ControllingCustomerDocumentaryAddress != null && ControllingCustomerDocumentaryAddress.HasRealOrganisation) ? ControllingCustomerDocumentaryAddress.Organisation : null; }
		}

		#region ControllingCustomerDocumentaryAddress

		public JobDocAddress ControllingCustomerDocumentaryAddress
		{
			get
			{
				if (Booking != null)
				{
					var controllingCustomerAddress = Booking.ControllingCustomerAddress;
					controllingCustomerAddress.OrgHeaderAfterChange -= ClientForCarrierContractChanged;
					controllingCustomerAddress.OrgHeaderAfterChange += ClientForCarrierContractChanged;

					return controllingCustomerAddress;
				}

				return null;
			}
		}

		public SecurityCheckpoint GetControllingCustomerSecurityCheckPoint()
		{
			if (Booking == null)
			{
				return null;
			}

			if (Booking.IsAir)
			{
				return Env.Security.QuickBookingAllowSaveWithoutControllingCustomerAir;
			}

			if (Booking.IsSea)
			{
				return Env.Security.QuickBookingAllowSaveWithoutControllingCustomerSea;
			}

			if (Booking.IsRoad)
			{
				return Env.Security.QuickBookingAllowSaveWithoutControllingCustomerRoad;
			}

			if (Booking.IsRail)
			{
				return Env.Security.QuickBookingAllowSaveWithoutControllingCustomerRail;
			}

			return Env.Security.QuickBookingAllowSaveWithoutControllingCustomer;
		}

		#endregion

		public OrgHeader ControllingAgent
		{
			get { return (ControllingAgentDocumentaryAddress != null && ControllingAgentDocumentaryAddress.HasRealOrganisation) ? ControllingAgentDocumentaryAddress.Organisation : null; }
		}

		#region ControllingAgentDocumentaryAddress

		public JobDocAddress ControllingAgentDocumentaryAddress
		{
			get { return Booking != null ? Booking.ControllingAgentDocumentaryAddress : null; }
		}

		public SecurityCheckpoint GetControllingAgentSecurityCheckPoint()
		{
			if (Booking == null)
			{
				return null;
			}

			if (Booking.IsAir)
			{
				return Env.Security.QuickBookingAllowSaveWithoutControllingAgentAir;
			}

			if (Booking.IsSea)
			{
				return Env.Security.QuickBookingAllowSaveWithoutControllingAgentSea;
			}

			if (Booking.IsRoad)
			{
				return Env.Security.QuickBookingAllowSaveWithoutControllingAgentRoad;
			}

			if (Booking.IsRail)
			{
				return Env.Security.QuickBookingAllowSaveWithoutControllingAgentRail;
			}

			return Env.Security.QuickBookingAllowSaveWithoutControllingAgent;
		}

		#endregion

		#region ChargeableUnit

		[List("UnitOfWeightList")]
		[MaxLength(2)]
		public ZString ChargeableUnit
		{
			get { return (GetFromBooking) ? Booking.JS_ChargeableUnit : Quote?.CurrentOneOffQuote?.TT_ChargeableUnit ?? ZString.Empty; }
		}

		public ZPropertyInfo ChargeableUnitInfo
		{
			get { return GetZPropertyInfo(Schema.ChargeableUnit); }
		}

		#endregion

		#region PickupReady

		[ResourceStringData("ViewQuotedBooking|PickupReady", ShortCaption = "Est Pickup", Caption = "Estimated Pickup", FullDescription = "The estimated pickup date for this movement.")]
		public ZDateTime PickupReady
		{
			get { return new ZDateTime(GetThroughState(RatingHeaderSchema.TH_QuoteDate, JobDocsAndCartageSchema.JP_EstimatedPickup)); }
			set
			{
				SetThroughState(RatingHeaderSchema.TH_QuoteDate, JobDocsAndCartageSchema.JP_EstimatedPickup, value);
				PickupReadyInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo PickupReadyInfo
		{
			get { return GetZPropertyInfo(Schema.PickupReady); }
		}

		#endregion

		#region Rating Frequency & Transit time

		#region Frequency & Unit

		#region Frequency

		[ResourceStringData("ViewQuotedBooking|Frequency", ShortCaption = "Freq", Caption = "Frequency", FullDescription = "The Frequency to print on this quotation.")]
		public ZInt Frequency
		{
			get
			{
				return new ZInt(GetQuoteProperty(RateOneOffShipmentSchema.TT_Frequency));
			}
			set
			{
				SetThroughState(RateOneOffShipmentSchema.TT_Frequency, null, value);
				FrequencyInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo FrequencyInfo
		{
			get
			{
				ZPropertyInfo info = GetZPropertyInfo(Schema.Frequency);
				return info;
			}
		}

		protected bool Frequency_ReadOnly
		{
			get { return Mode.IsEmpty; }
		}

		#endregion

		#region Unit

		// Max length in DB is 10 but was changed to 9 here because the GUI uses this MaxLength to change the size
		// of the control and 10 for the drop down side of calc edit drop on the gui was too long.
		[MaxLength(9)]
		[List("FrequencyUnits")]
		public ZString FrequencyUnit
		{
			get
			{
				return new ZString(GetQuoteProperty(RateOneOffShipmentSchema.TT_FrequencyUnit));
			}
			set
			{
				CheckMaximumLength(FrequencyUnitInfo, value);
				SetThroughState(RateOneOffShipmentSchema.TT_FrequencyUnit, null, value);
				FrequencyUnitInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo FrequencyUnitInfo
		{
			get
			{
				return GetZPropertyInfo(Schema.FrequencyUnit);
			}
		}

		protected bool FrequencyUnit_ReadOnly
		{
			get { return Mode.IsEmpty; }
		}

		#endregion

		#region Start Date

		[ResourceStringData("ViewQuotedBooking|StartDate", ShortCaption = "Start", Caption = "Start Date", FullDescription = "The starting date for the validity of the quotation.")]
		public ZDateTime StartDate
		{
			get
			{
				return Quote == null ? ZDateTime.Empty : Quote.TH_QuoteDate;
			}
			set
			{
				SetThroughState(RatingHeaderSchema.TH_QuoteDate, null, value);
				StartDateInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo StartDateInfo => GetZPropertyInfo(nameof(StartDate));

		#endregion

		#region End Date

		[ResourceStringData("ViewQuotedBooking|EndDate", ShortCaption = "End", Caption = "End Date", FullDescription = "The ending date for the validity of the quotation.")]
		public ZDate EndDate
		{
			get
			{
				return Quote == null ? ZDate.Empty : Quote.TH_QuoteEndDate;
			}
			set
			{
				SetThroughState(RatingHeaderSchema.TH_QuoteEndDate, null, value);
				EndDateInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo EndDateInfo => GetZPropertyInfo(nameof(EndDate));

		#endregion

		#endregion

		#region Transit Time

		[List("TransitTimesList")]
		[MaxLength(RateOneOffShipment.Schema.TT_TransitTimeMaxLength)]
		[ResourceStringData("ViewQuotedBooking|TransitTime", ShortCaption = "Transit", Caption = "Transit Time", FullDescription = "The transit time for this quotation.")]
		public ZString TransitTime
		{
			get
			{
				return new ZString(GetQuoteProperty(RateOneOffShipmentSchema.TT_TransitTime));
			}
			set
			{
				SetThroughState(RateOneOffShipmentSchema.TT_TransitTime, null, value);
				TransitTimeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo TransitTimeInfo
		{
			get
			{
				return GetZPropertyInfo(Schema.TransitTime);
			}
		}

		protected bool TransitTime_ReadOnly
		{
			get { return Mode.IsEmpty || ModeInfo.HasErrors(); }
		}

		#endregion

		#endregion

		#region PickupClose

		[ResourceStringData("ViewQuotedBooking|PickupClose", ShortCaption = "Req. By", MediumCaption = "Required By", Caption = "Pickup Required By", FullDescription = "The latest time the pickup is required by.")]
		public ZDateTime PickupClose
		{
			get { return new ZDateTime(GetThroughState(RatingHeaderSchema.TH_QuoteDate, JobDocsAndCartageSchema.JP_PickupRequiredBy)); }
			set
			{
				SetThroughState(RatingHeaderSchema.TH_QuoteDate, JobDocsAndCartageSchema.JP_PickupRequiredBy, value);
				PickupCloseInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo PickupCloseInfo
		{
			get { return GetZPropertyInfo(Schema.PickupClose); }
		}

		#endregion

		#region DeliveryOpen

		protected bool DeliveryOpen_ReadOnly
		{
			get
			{
				return !Env.Security.QuickBookingEstimatedDeliveryDueDateOverride.IsAllowed;
			}
		}

		[ResourceStringData("ViewQuotedBooking|DeliveryOpen", ShortCaption = "Est Dlv.", MediumCaption = "Est Delivery", Caption = "Estimated Delivery", FullDescription = "The estimated delivery date for this movement.")]
		public ZDateTime DeliveryOpen
		{
			get { return new ZDateTime(GetThroughState(RatingHeaderSchema.TH_QuoteEndDate, JobDocsAndCartageSchema.JP_EstimatedDelivery)); }
			set
			{
				SetThroughState(RatingHeaderSchema.TH_QuoteEndDate, JobDocsAndCartageSchema.JP_EstimatedDelivery, value);
				DeliveryOpenInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo DeliveryOpenInfo
		{
			get { return GetZPropertyInfo(Schema.DeliveryOpen); }
		}

		#endregion

		#region DeliveryClose

		[ResourceStringData("ViewQuotedBooking|DeliveryClose", ShortCaption = "Req. By", MediumCaption = "Required By", Caption = "Delivery Required By", FullDescription = "The latest time the delivery is required by.")]
		public ZDateTime DeliveryClose
		{
			get { return new ZDateTime(GetThroughState(RatingHeaderSchema.TH_QuoteEndDate, JobDocsAndCartageSchema.JP_DeliveryRequiredBy)); }
			set
			{
				SetThroughState(RatingHeaderSchema.TH_QuoteEndDate, JobDocsAndCartageSchema.JP_DeliveryRequiredBy, value);
				DeliveryCloseInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo DeliveryCloseInfo
		{
			get { return GetZPropertyInfo(Schema.DeliveryClose); }
		}

		#endregion

		#region ExportReceivingDepot

		public ZGuid ExportReceivingDepot
		{
			get { return new ZGuid(GetThroughState(null, JobShipmentSchema.JS_OA_ExportReceivingDepot)); }
			set
			{
				if (Booking != null)
				{
					if (Booking.JS_OA_ExportReceivingDepot != value)
					{
						if (!Booking.JS_A_RCV.IsEmpty && CheckOverwriteReceivedCargo != null && !Booking.JS_OA_ExportReceivingDepot.IsEmpty)
						{
							CancelEventArgs e = new CancelEventArgs();
							CheckOverwriteReceivedCargo(this, e);
							overrideDepot = !e.Cancel;
						}

						if (overrideDepot)
						{
							Booking.JS_OA_ExportReceivingDepot = value;
						}

						if (!IsValidationSuspended)
						{
							Validation.ValidateExportReceivingDepot();
						}
					}
				}
				ExportReceivingDepotInfo.RefreshBinding();
			}
		}
		bool overrideDepot = true;
		public event CancelEventHandler CheckOverwriteReceivedCargo;

		public ZPropertyInfo ExportReceivingDepotInfo
		{
			get { return GetZPropertyInfo(Schema.ExportReceivingDepot); }
		}

		[EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
		public ZAddress ExportReceivingDepot_ZAddress
		{
			get
			{
				if (exportReceivingDepot_ZAddress == null)
				{
					exportReceivingDepot_ZAddress = new ZAddress(ExportReceivingDepotInfo);
					exportReceivingDepot_ZAddress.DefaultAddressType = AddressType.OFC;
				}
				return exportReceivingDepot_ZAddress;
			}
		}
		ZAddress exportReceivingDepot_ZAddress;

		#endregion

		#region ImportReleaseDepot

		public ZGuid ImportReleaseDepot
		{
			get { return new ZGuid(GetThroughState(null, JobShipmentSchema.JS_OA_ImportReleaseDepot)); }
			set
			{
				if (Booking != null)
				{
					if (Booking.JS_OA_ImportReleaseDepot != value)
					{
						Booking.JS_OA_ImportReleaseDepot = value;

						if (!IsValidationSuspended)
						{
							Validation.ValidateImportReleaseDepot();
						}
					}
				}
				ImportReleaseDepotInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ImportReleaseDepotInfo
		{
			get { return GetZPropertyInfo(Schema.ImportReleaseDepot); }
		}

		[EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
		public ZAddress ImportReleaseDepot_ZAddress
		{
			get
			{
				if (importReleaseDepot_ZAddress == null)
				{
					importReleaseDepot_ZAddress = new ZAddress(ImportReleaseDepotInfo);
					importReleaseDepot_ZAddress.DefaultAddressType = AddressType.OFC;
				}
				return importReleaseDepot_ZAddress;
			}
		}
		ZAddress importReleaseDepot_ZAddress;

		#endregion

		#region Commodity

		[List("Quote+CurrentOneOffQuote.Lookups+Commodities")]
		[MaxLength(RateOneOffShipment.Schema.TT_RH_NKCommodityMaxLength)]
		[ResourceStringData("ViewQuotedBooking|Commodity", Caption = "Commodity", FullDescription = "The commodity that is being quoted/moved.")]
		public ZString Commodity
		{
			get
			{
				return new ZString(GetThroughState(RateOneOffShipmentSchema.TT_RH_NKCommodity, JobShipmentSchema.JS_RH_NKRateCommodity));
			}
			set
			{
				var oldValue = Commodity;

				if (oldValue != value)
				{
					rateLocalCode = null;
					FMCTariffID = ZString.Empty;
				}
				SetThroughState(RateOneOffShipmentSchema.TT_RH_NKCommodity, JobShipmentSchema.JS_RH_NKRateCommodity, value);
				SetCommodityOnPackLinesAndContainers(oldValue, value);

				CommodityInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CommodityInfo
		{
			get { return GetZPropertyInfo(Schema.Commodity); }
		}

		public ZString RateLocalCode
		{
			get
			{
				if (rateLocalCode == null)
				{
					var query = new ZQuery(RefCommodityCodeMapSchema.LC_RH_NKCommodityCode, Commodity)
						.AddToFilter(new ZQuery(RefCommodityCodeMapSchema.LC_LocalCodeProvider, GlobalCommodityCodeProviderList.Codes.Rating));
					var result = Factory.LoadTop1<RefCommodityCodeMap>(query);
					rateLocalCode = result?.LC_LocalCode ?? string.Empty;
				}

				return rateLocalCode;
			}
		}
		string rateLocalCode;

		public ZPropertyInfo RateLocalCodeInfo => GetZPropertyInfo(nameof(RateLocalCode));

		/// <summary>
		/// Warning: The setting of Commodity will reset FMCTariffID to ZString.Empty, ensure correct order of value assignment to get desired result
		/// </summary>
		[MaxLength(RateOneOffShipment.Schema.TT_FMCTariffIDMaxLength)]
		[ResourceStringData("ViewQuotedBooking|FMCTariff", Caption = "FMC ID", FullDescription = "The FMC Tariff ID")]
		public ZString FMCTariffID
		{
			get { return new ZString(GetThroughState(RateOneOffShipmentSchema.TT_FMCTariffID, JobShipmentSchema.JS_FMCTariffID)); }
			set
			{
				var oldValue = FMCTariffID;

				SetThroughState(RateOneOffShipmentSchema.TT_FMCTariffID, JobShipmentSchema.JS_FMCTariffID, value);
				FMCTariffIDInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo FMCTariffIDInfo => GetZPropertyInfo(Schema.FMCTariffID);

		public RefCommodityCodeCollection CommodityCodeList => commodityCodeList ??= new(Factory);
		RefCommodityCodeCollection commodityCodeList;

		#region Default Commodity to PackLines and Containers

		void SetCommodityOnPackLinesAndContainers(ZString oldValue, ZString newValue)
		{
			if (Booking != null && !newValue.IsEmpty && newValue != oldValue)
			{
				foreach (ForwardingPackLine line in Booking.OuterPackLines)
				{
					if (line.JL_RH_NKCommodityCode == oldValue
						|| line.JL_RH_NKCommodityCode.IsEmpty
						|| (RegistryCommodityCode != null && line.JL_RH_NKCommodityCode == RegistryCommodityCode.RH_Code))
					{
						line.JL_RH_NKCommodityCode = newValue;
					}
				}

				foreach (ForwardingContainer container in QuotedBookingContainers)
				{
					if (container.JC_RH_NKContainerCommodityCode == oldValue
						|| container.JC_RH_NKContainerCommodityCode.IsEmpty
						|| (RegistryCommodityCode != null && container.JC_RH_NKContainerCommodityCode == RegistryCommodityCode.RH_Code))
					{
						container.JC_RH_NKContainerCommodityCode = newValue;
					}
				}
			}
		}

		#endregion

		#region Commodity Code from Registry

		RefCommodityCode RegistryCommodityCode
		{
			get
			{
				if (!isRegistryCommodityCodeInitalized)
				{
					registryCommodityCode = Factory.Load<RefCommodityCode>(Env.Registry.CommodityCode);
					isRegistryCommodityCodeInitalized = true;
				}

				return registryCommodityCode;
			}
		}

		RefCommodityCode registryCommodityCode;
		bool isRegistryCommodityCodeInitalized;

		#endregion

		#endregion

		#region Via

		[List("Quote+CurrentOneOffQuote.Lookups+ViaLocations")]
		[MaxLength(RateOneOffShipment.Schema.TT_RL_NKViaLocationMaxLength)]
		[ResourceStringData("ViewQuotedBooking|Via", Caption = "Via", FullDescription = "If this is a transhipment, this specifies the Via/Transhipment port.")]
		public ZString Via
		{
			get { return new ZString(GetQuoteProperty(RateOneOffShipmentSchema.TT_RL_NKViaLocation)); }
			set
			{
				var changed = value != Via;

				SetThroughState(RateOneOffShipmentSchema.TT_RL_NKViaLocation, null, value);
				ViaInfo.RefreshBinding();

				if (changed)
				{
					UpdateQuotedBookingCO2eStatusToNotCurrent(new CO2eStatusChangedReason(Quote.CurrentOneOffQuote.TT_RL_NKViaLocationInfo));
				}
			}
		}

		public ZPropertyInfo ViaInfo
		{
			get { return GetZPropertyInfo(Schema.Via); }
		}

		#endregion

		#region OH_Carrier

		public OrgHeader Carrier
		{
			get { return Factory.Load<OrgHeader>(OH_Carrier); }
		}

		[RelatedBusinessObject("Carrier")]
		[List("Carriers")]
		[ResourceStringData("ViewQuotedBooking|OH_Carrier", Caption = "Carrier", FullDescription = "Specifies the carrier or shipping line used for this movement.")]
		public ZGuid OH_Carrier
		{
			get { return ObjectState == QuotedBookingState.QuoteOnly ? (ZGuid)GetQuoteProperty(RateOneOffShipmentSchema.TT_OH_Carrier) : Booking.BookedShippingLinePK; }
			set
			{
				switch (ObjectState)
				{
					case QuotedBookingState.QuoteOnly:
						var previousValue = OH_Carrier;
						SetQuoteProperty(RateOneOffShipmentSchema.TT_OH_Carrier, value);
						if (value != previousValue)
						{
							UpdateQuotedBookingCO2eStatusToNotCurrent(new CO2eStatusChangedReason(OH_CarrierInfo, previousValue));
						}
						break;

					case QuotedBookingState.BookingOnly:
					case QuotedBookingState.UnacceptedBookingWithQuote:
					case QuotedBookingState.AcceptedBookingWithQuote:
						if (ObjectState == QuotedBookingState.AcceptedBookingWithQuote)
						{
							SetQuoteProperty(RateOneOffShipmentSchema.TT_OH_Carrier, value, false);
						}
						Booking.BookedShippingLinePK = value;
						break;
				}

				OH_CarrierInfo.RefreshBinding();
				CarrierServiceLevel = ZString.Empty;
				carrierServiceLevelsCache = null;

				if (Creditor.IsEmpty)
				{
					Creditor = DefaultCreditorHelper.GetDefaultCreditor(
						this,
						Carrier,
						TransportMode,
						ContainerMode,
						Origin,
						Destination,
						Factory);
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateCarrierContractNumber();
				}
			}
		}

		public ZPropertyInfo OH_CarrierInfo
		{
			get { return GetZPropertyInfo(Schema.OH_Carrier); }
		}

		#endregion

		#region CarrierServiceLevel

		[List("CarrierServiceLevels")]
		[MaxLength(CommonShipment.Schema.JS_PL_NKCarrierServiceLevelMaxLength)]
		[ResourceStringData("ViewQuotedBooking|CarrierServiceLevel", ShortCaption = "Car.Svc. Lvl.", Caption = "Carrier Service Level")]
		public ZString CarrierServiceLevel
		{
			get { return new ZString(GetThroughState(RateOneOffShipmentSchema.TT_PL_NKCarrierServiceLevel, JobShipmentSchema.JS_PL_NKCarrierServiceLevel)); }
			set
			{
				SetThroughState(RateOneOffShipmentSchema.TT_PL_NKCarrierServiceLevel, JobShipmentSchema.JS_PL_NKCarrierServiceLevel, value);
				CarrierServiceLevelInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					Validation.ValidateCarrierServiceLevel();
				}
			}
		}

		public ZPropertyInfo CarrierServiceLevelInfo
		{
			get { return GetZPropertyInfo(Schema.CarrierServiceLevel); }
		}

		public OrgCarrierServiceLevelCollection CarrierServiceLevels
		{
			get
			{
				return (carrierServiceLevelsCache ?? (carrierServiceLevelsCache = new CachedProperty<OrgCarrierServiceLevelCollection>(Factory, GetCarrierServiceLevel_List))).Value;
			}
		}

		CachedProperty<OrgCarrierServiceLevelCollection> carrierServiceLevelsCache;

		OrgCarrierServiceLevelCollection GetCarrierServiceLevel_List()
		{
			OrgCarrierServiceLevelCollection carrierServiceLevels = Carrier != null
																	? new OrgCarrierServiceLevelCollection(Carrier.MiscServ)
																	: new OrgCarrierServiceLevelCollection(Factory);
			carrierServiceLevels.Load();
			return carrierServiceLevels;
		}

		#endregion

		#region Comparison Quote Results

		public ComparisonQuoteResultCollection ComparisonQuoteResults
		{
			get
			{
				if (fComparisonQuoteResults == null)
				{
					fComparisonQuoteResults = new ComparisonQuoteResultCollection(Factory);
				}
				return fComparisonQuoteResults;
			}
		}

		ComparisonQuoteResultCollection fComparisonQuoteResults;

		#endregion

		#region IsCompareMode

		[DefaultValue(false)]
		public ZBool IsCompareMode
		{
			get
			{
				return fIsCompareMode;
			}
			set
			{
				fIsCompareMode = value;
				if (fIsCompareMode)
				{
					Mode = ZString.Empty;
				}
				IsCompareModeInfo.RefreshBinding();
			}
		}
		ZBool fIsCompareMode;

		public ZPropertyInfo IsCompareModeInfo
		{
			get { return GetZPropertyInfo(Schema.IsCompareMode); }
		}

		#endregion

		#region IsCompareServiceLevel

		[DefaultValue(false)]
		public ZBool IsCompareServiceLevel
		{
			get
			{
				return fIsCompareServiceLevel;
			}
			set
			{
				fIsCompareServiceLevel = value;
				if (fIsCompareServiceLevel)
				{
					ServiceLevel = ZString.Empty;
				}
				IsCompareServiceLevelInfo.RefreshBinding();
			}
		}
		ZBool fIsCompareServiceLevel;

		public ZPropertyInfo IsCompareServiceLevelInfo
		{
			get { return GetZPropertyInfo(Schema.IsCompareServiceLevel); }
		}

		#endregion

		#region IsConsolidated

		internal bool IsConsolidated => Booking?.JS_IsForwardRegistered ?? false;

		#endregion

		#region QuoteNumberOfEntries

		[ResourceStringData("ViewQuotedBooking|QuoteNumberOfEntries", ShortCaption = "Entries", Caption = "Number of Entries", FullDescription = "The number of customs entries for quotation and rating purposes.")]
		public ZShort QuoteNumberOfEntries
		{
			get { return Quote != null && Quote.CurrentOneOffQuote != null ? Quote.CurrentOneOffQuote.TT_NumberOfEntries : (ZShort)0; }
			set
			{
				if (Quote != null && Quote.CurrentOneOffQuote != null)
				{
					SetThroughState(RateOneOffShipmentSchema.TT_NumberOfEntries, null, value);
				}
			}
		}

		public ZPropertyInfo QuoteNumberOfEntriesInfo
		{
			get
			{
				return GetWrappedZPropertyInfo(Schema.QuoteNumberOfEntries, (p) => Quote != null && Quote.CurrentOneOffQuote != null ?
					Quote.CurrentOneOffQuote.TT_NumberOfEntriesInfo : GetZPropertyInfo(Schema.QuoteNumberOfEntries));
			}
		}

		protected bool QuoteNumberOfEntries_ReadOnly
		{
			get { return Quote == null; }
		}

		#endregion

		#region QuoteNumberOfEntryLines

		[ResourceStringData("ViewQuotedBooking|QuoteNumberOfEntryLines", ShortCaption = "Lines", Caption = "Number of Customs Entry/Invoice Lines", FullDescription = "The number of customs lines for quotation and rating purposes.")]
		public ZShort QuoteNumberOfEntryLines
		{
			get { return Quote != null && Quote.CurrentOneOffQuote != null ? Quote.CurrentOneOffQuote.TT_NumberOfEntryLines : (ZShort)0; }
			set
			{
				if (Quote != null && Quote.CurrentOneOffQuote != null)
				{
					SetThroughState(RateOneOffShipmentSchema.TT_NumberOfEntryLines, null, value);
				}
			}
		}

		public ZPropertyInfo QuoteNumberOfEntryLinesInfo
		{
			get
			{
				return GetWrappedZPropertyInfo(Schema.QuoteNumberOfEntryLines, (p) => Quote != null && Quote.CurrentOneOffQuote != null ?
					Quote.CurrentOneOffQuote.TT_NumberOfEntryLinesInfo : GetZPropertyInfo(Schema.QuoteNumberOfEntryLines));
			}
		}

		protected bool QuoteNumberOfEntryLines_ReadOnly
		{
			get { return Quote == null; }
		}

		#endregion

		#region HBLAWBChargesDisplay

		[List("HBLAWBChargesDisplay_List")]
		[MaxLength(CommonShipment.Schema.JS_HBLAWBChargesDisplayMaxLength)]
		[ResourceStringData("ViewQuotedBooking|HBLAWBChargesDisplay", Caption = "Charges Apply", FullDescription = "Determines the method in which follow on charges are displayed on Sea Freight Bills of Lading.")]
		public ZString HBLAWBChargesDisplay
		{
			get { return new ZString(GetThroughState(null, JobShipmentSchema.JS_HBLAWBChargesDisplay)); }
			set
			{
				SetThroughState(null, JobShipmentSchema.JS_HBLAWBChargesDisplay, value);
				HBLAWBChargesDisplayInfo.RefreshBinding();

				if (!IsValidationSuspended)
				{
					Validation.ValidateHBLAWBChargesDisplay();
				}
			}
		}

		public ZPropertyInfo HBLAWBChargesDisplayInfo
		{
			get { return GetZPropertyInfo(Schema.HBLAWBChargesDisplay); }
		}

		#endregion

		#region IsAviationSecurityApplicableForTransportMode

		public ZBool IsAviationSecurityApplicableForTransportMode => Booking != null && Booking.AviationSecurity.IsAviationSecurityApplicableForTransportMode;

		public ZPropertyInfo IsAviationSecurityApplicableForTransportModeInfo => GetZPropertyInfo(Schema.IsAviationSecurityApplicableForTransportMode);

		#endregion

		#region CO2 Emmision

		public ZString TotalCO2eForBinding => this.GetTotalCO2eForBinding();

		public ZPropertyInfo TotalCO2eForBindingInfo => GetZPropertyInfo(nameof(TotalCO2eForBinding));

		[DecimalPlaces(3)]
		public ZDecimal TotalCO2eForSorting => this.GetTotalCO2e();

		public ZPropertyInfo TotalCO2eForSortingInfo => GetZPropertyInfo(nameof(TotalCO2eForSorting));

		public ZString CO2eStatus => this.GetCO2eStatus();

		#region TotalCO2e

		ZDecimal TotalCO2eByWeight
		{
			get
			{
				if (this.GetCO2ePerTonneInKg() == 0)
				{
					return ZDecimal.Zero;
				}

				var supporter = this as ICO2eLegBasedSupporter;
				return Constants.Weight.ConvertSafe(supporter.Weight, supporter.UnitOfWeight, Constants.Weight.Tonnes) * this.GetCO2ePerTonneInKg();
			}
		}

		ZDecimal TotalCO2eByTEU
		{
			get
			{
				if (this.GetCO2ePerTEUInKg() == 0)
				{
					return ZDecimal.Zero;
				}

				return ((ICO2eTEUProvider)this).NumberOfTEU * this.GetCO2ePerTEUInKg();
			}
		}

		#endregion

		#endregion

		#endregion

		#region Lookups

		public OrganisationsFindBoxCollection Receiver_List
		{
			get
			{
				switch (QuotedBookingHelper.GetReceiverOrganizationTypeFromMode(Mode))
				{
					case QuotedBookingHelper.ReceiverOrganization.SeaCTO:
						return BindToListInstance.SeaCTO_List;

					case QuotedBookingHelper.ReceiverOrganization.PackDepot:
						return BindToListInstance.PackDepot_List;

					case QuotedBookingHelper.ReceiverOrganization.CTO:
						return BindToListInstance.OrgCTO_List;

					default:
						return BindToListInstance.PackDepot_List;
				}
			}
		}

		public OrganisationsFindBoxCollection Delivery_List
		{
			get
			{
				switch (QuotedBookingHelper.GetDeliveryOrganizationTypeFromMode(Mode))
				{
					case QuotedBookingHelper.DeliveryOrganization.UnpackDepot:
						return BindToListInstance.UnpackDepot_List;

					case QuotedBookingHelper.DeliveryOrganization.CTO:
						return BindToListInstance.OrgCTO_List;

					default:
						return BindToListInstance.UnpackDepot_List;
				}
			}
		}

		#region FrequencyUnitList

		public CodeDescriptionPairList FrequencyUnits
		{
			get { return Quote.CurrentOneOffQuote.Lookups.FrequencyUnits; }
		}

		#endregion

		#region FrequencyUnitList

		public CodeDescriptionPairList TransitTimesList
		{
			get
			{
				if (Quote.CurrentOneOffQuote.IsAir)
				{
					return Quote.CurrentOneOffQuote.Lookups.AirTransitTimes;
				}
				else
				{
					return Quote.CurrentOneOffQuote.Lookups.SeaTransitTimes;
				}
			}
		}

		#endregion

		#region Clients

		public OrganisationsFindBoxCollection Clients
		{
			get
			{
				if (clients == null)
				{
					clients = new OrganisationsFindBoxCollection(Factory);
					clients.OrganisationType = OrganisationTypes.Debtor;
				}
				return clients;
			}
		}

		OrganisationsFindBoxCollection clients;

		#endregion

		#region Modes

		public CodeDescriptionPairList SelectedComparisonModes
		{
			get
			{
				var result = new CodeDescriptionPairList();
				foreach (CodeDescriptionBool pair in ComparisonModes)
				{
					if (pair.Bool)
					{
						result.Add(pair);
					}
				}
				return result;
			}
		}

		public CodeDescriptionBoolCollection ComparisonModes
		{
			get
			{
				if (fComparisonModes == null)
				{
					fComparisonModes = new CodeDescriptionBoolCollection(ModesForWebTracker, false);
				}
				return fComparisonModes;
			}
		}
		CodeDescriptionBoolCollection fComparisonModes;

		public CodeDescriptionPairList Modes
		{
			get
			{
				if (fModes == null)
				{
					fModes = GetNewModes(GetFromBooking);
				}
				return fModes;
			}
		}

		CodeDescriptionPairList fModes;

		public CodeDescriptionPairList ModesForWebTracker
		{
			get
			{
				if (modesForWebTracker == null)
				{
					modesForWebTracker = GetOldModes(GetFromBooking);
				}
				return modesForWebTracker;
			}
		}

		CodeDescriptionPairList modesForWebTracker;

		public CodeDescriptionPairList TransportModes => GetNewTransportModes();

		public CodeDescriptionPairList OrgRoles => GetNewOrgRoles();

		public CodeDescriptionPairList ContainerModes => GetContainerModes(TransportMode, GetFromBooking);

		public CodeDescriptionPairList HBLDeliveryModes => GetHBLDeliveryModes();

		public static CodeDescriptionPairList GetNewModes(bool bookingOnly)
		{
			var result = RatingFreightModeLists.RateModeList();
			if (bookingOnly)
			{
				result.RemoveCode(Constants.RateMode.SEA);
				result.RemoveCode(Constants.RateMode.ROA);
				result.RemoveCode(Constants.RateMode.RAI);
				result.RemoveCode(Constants.RateMode.FWL);
			}

			return result;
		}

		public static CodeDescriptionPairList GetOldModes(bool bookingOnly)
		{
			var result = GetNewModes(bookingOnly);
			result.RemoveCode(Constants.RateMode.BCN);
			result.RemoveCode(Constants.RateMode.SCN);
			result.RemoveCode(Constants.RateMode.BLK);
			result.RemoveCode(Constants.RateMode.BBK);
			result.RemoveCode(Constants.ContainerModes.Liquid);
			result.RemoveCode(Constants.RateMode.ROR);
			result.RemoveCode(Constants.RateMode.OBC);
			result.RemoveCode(Constants.RateMode.UNA);

			return result;
		}

		public static CodeDescriptionPairList GetNewTransportModes()
		{
			return RatingFreightModeLists.TransportModeList();
		}

		public static CodeDescriptionPairList GetNewOrgRoles()
		{
			return RatingFreightModeLists.OrgRoleList();
		}

		public static CodeDescriptionPairList GetContainerModes(string transportMode, bool bookingOnly)
		{
			var result = RatingFreightModeLists.ContainerModeList(transportMode);
			if (bookingOnly)
			{
				result.RemoveCode(Constants.RateMode.SEA);
				result.RemoveCode(Constants.RateMode.ROA);
				result.RemoveCode(Constants.RateMode.RAI);
				result.RemoveCode(Constants.RateMode.FWL);
				result.RemoveCode(Constants.RateMode.LRO);
				result.RemoveCode(Constants.RateMode.COU);
			}

			return result;
		}

		public CodeDescriptionPairList GetHBLDeliveryModes()
		{
			if (GetFromBooking)
			{
				return Booking?.Lookups?.JS_HBLContainerPackModeOverride_List ?? new CodeDescriptionPairList();
			}
			else
			{
				return Quote?.CurrentOneOffQuote?.Lookups?.HBLDeliveryModesList ?? new CodeDescriptionPairList();
			}
		}

		#endregion

		#region UnitOfVolumeList

		public CodeDescriptionPairList UnitOfVolumeList
		{
			get { return new CodeDescriptionPairList(OLookUpEditType.Volume); }
		}

		#endregion

		#region UnitOfWeightList

		public CodeDescriptionPairList UnitOfWeightList
		{
			get { return new CodeDescriptionPairList(OLookUpEditType.Weight); }
		}

		#endregion

		#region Organisations

		[OrganisationDefaultProvider(OrganisationType = OrganisationTypes.Consignee)]
		public OrganisationsFindBoxCollection Consignee_List
		{
			get { return AddToConsigneeOrgFilter(BindToListInstance.OrgConsignee_List); }
		}

		[OrganisationDefaultProvider(OrganisationType = OrganisationTypes.Consignor)]
		public OrganisationsFindBoxCollection Consignor_List
		{
			get { return AddToConsignorOrgFilter(BindToListInstance.OrgConsignor_List); }
		}

		protected OrganisationsFindBoxCollection AddToConsigneeOrgFilter(OrganisationsFindBoxCollection orgCollection)
		{
			orgCollection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(
				"Consignee - Related Consignor", "Property", // Filter Strip PropertyFor
				delegate
				{ return (Consignor != null && Consignor.BuyerLinks.Count > 0) ? Consignor.PK : ZGuid.Empty; }));

			return orgCollection;
		}

		protected OrganisationsFindBoxCollection AddToConsignorOrgFilter(OrganisationsFindBoxCollection orgCollection)
		{
			orgCollection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(
				"Consignor - Related Consignee", "Property", // Filter Strip Property
				delegate
				{ return (Consignee != null && Consignee.SupplierLinks.Count > 0) ? Consignee.PK : ZGuid.Empty; }));

			return orgCollection;
		}

		BindToLists BindToListInstance
		{
			get { return BindToLists.GetCachedLists(Factory); }
		}

		public OrgHeaderCollection Organisations
		{
			get
			{
				if (fOrganisations == null)
				{
					fOrganisations = new OrgHeaderCollection(Factory);
				}
				return fOrganisations;
			}
		}

		OrgHeaderCollection fOrganisations;

		[OrganisationDefaultProvider(DocAddressType = DocAddressTypes.Codes.ConsignorPickupDeliveryAddress)]
		public OrgHeaderCollection PickUps
		{
			get { return new OrganisationsFindBoxCollection(Factory); }
		}

		[OrganisationDefaultProvider(DocAddressType = DocAddressTypes.Codes.ConsigneePickupDeliveryAddress)]
		public OrgHeaderCollection Deliveries
		{
			get { return new OrganisationsFindBoxCollection(Factory); }
		}

		#endregion

		#region Carriers

		public ShippingProviderCollection Carriers
		{
			get
			{
				if (fCarriers == null)
				{
					fCarriers = new ShippingProviderCollection(Factory);
				}
				return fCarriers;
			}
		}

		ShippingProviderCollection fCarriers;

		#endregion

		#region Creditors

		public OrganisationsFindBoxCollection Creditors
		{
			get
			{
				if (fCreditors == null)
				{
					fCreditors = new OrganisationsFindBoxCollection(Factory);
				}
				return fCreditors;
			}
		}

		OrganisationsFindBoxCollection fCreditors;

		#endregion

		#region IncoTerms

		public CodeDescriptionPairList IncoTerms
		{
			get { return IsDomesticFreight ? DomesticPaymentTermsList : InternationalPaymentTermsList; }
		}

		CodeDescriptionPairList DomesticPaymentTermsList
		{
			get { return Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.DomesticPaymentTerms); }
		}

		CodeDescriptionPairList InternationalPaymentTermsList
		{
			get
			{
				return Factory.GetCachedValue("QuotedBooking.InternationalPaymentTermsList", () => new IncoTermsCodeDescriptionPairList(IncoTermsListType.ActiveIncoTerms));
			}
		}

		#endregion

		#region Currencies

		public RefCurrencyCollection Currencies
		{
			get
			{
				if (fCurrencies == null)
				{
					fCurrencies = new RefCurrencyCollection(Factory);
				}
				return fCurrencies;
			}
		}

		RefCurrencyCollection fCurrencies;

		#endregion

		#region Commodities

		public virtual RefCommodityCodeCollection Commodities
		{
			get { return new RefCommodityCodeCollection(Factory); }
		}

		#endregion

		#region DeliveryLocations

		public virtual RefUNLOCOCollection DeliveryLocations
		{
			get { return new RefUNLOCOCollection(Factory); }
		}

		#endregion

		#region ReceivalLocations

		public virtual RefUNLOCOCollection ReceivalLocations
		{
			get { return new RefUNLOCOCollection(Factory); }
		}

		#endregion

		#region ViaLocations

		public virtual RefUNLOCOCollection ViaLocations
		{
			get { return new RefUNLOCOCollection(Factory); }
		}

		#endregion

		#region LoadPortLocations

		public virtual RefUNLOCOCollection LoadPortLocations
		{
			get { return new RefUNLOCOCollection(Factory); }
		}

		#endregion

		#region DischargePortLocations

		public virtual RefUNLOCOCollection DischargePortLocations
		{
			get { return new RefUNLOCOCollection(Factory); }
		}

		#endregion

		#region CompanyTariffLevels

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

		#endregion

		#region ServiceLevels

		public CodeDescriptionPairList SelectedComparisonServiceLevels
		{
			get
			{
				CodeDescriptionPairList result = new CodeDescriptionPairList();
				foreach (CodeDescriptionBool pair in ComparisonServiceLevels)
				{
					if (pair.Bool)
					{
						result.Add(pair);
					}
				}
				return result;
			}
		}

		public CodeDescriptionBoolCollection ComparisonServiceLevels
		{
			get
			{
				if (fComparisonServiceLevels == null)
				{
					CodeDescriptionPairList result = new CodeDescriptionPairList();
					foreach (RefServiceLevel serviceLevel in ServiceLevels)
					{
						result.AddPair(serviceLevel.PK, serviceLevel.RS_Code, serviceLevel.RS_DescriptionMultilingual);
					}
					fComparisonServiceLevels = new CodeDescriptionBoolCollection(result, false);
				}
				return fComparisonServiceLevels;
			}
		}
		CodeDescriptionBoolCollection fComparisonServiceLevels;

		public virtual RefServiceLevelCollection ServiceLevels
		{
			get
			{
				if (Globals.IsWeb)
				{
					return WebServiceLevelCollection != null ? WebServiceLevelCollection() : null;
				}

				return new RefServiceLevelCollection(Factory);
			}
		}

		public delegate RefServiceLevelCollection WebServiceLevels();

		public WebServiceLevels WebServiceLevelCollection;

		[SuppressWeaklyTypedCollectionMessage]
		public IList ServiceLevelOrTransitTimeCollection => ForwardingShipmentLookups.GetServiceLevelOrTransitTimeCollection(Booking, ServiceLevels, Factory);

		#endregion

		#region GoodsCurrencies

		public virtual RefCurrencyCollection GoodsCurrencies
		{
			get { return new RefCurrencyCollection(Factory); }
		}

		#endregion

		#region InsureValCurrencies

		public virtual RefCurrencyCollection InsureValCurrencies
		{
			get { return new RefCurrencyCollection(Factory); }
		}

		#endregion

		#region Cartage Equipment

		public CodeDescriptionPairList Equipments
		{
			get
			{
				if (ContainerMode == Core.Constants.ContainerModes.FCL)
				{
					return fFCLEquipmentTypes ?? (fFCLEquipmentTypes = new FCLEquipmentNeededList(false));
				}
				else
				{
					return fLCLAIREquipmentTypes ?? (fLCLAIREquipmentTypes = new LCLAIREquipmentNeededList(false));
				}
			}
		}

		CodeDescriptionPairList fFCLEquipmentTypes;
		CodeDescriptionPairList fLCLAIREquipmentTypes;

		#endregion

		#region HBLAWBChargesDisplay_List

		public CodeDescriptionPairList HBLAWBChargesDisplay_List
		{
			get { return TransportMode == Constants.TransportModes.Air ? ChargesApplyHelper.ChargesApplyPairList : DocumentsDataRegistry.Instance.HBLChargesDefaultDisplayTypesPairList; }
		}

		#endregion

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			Booking?.DefaultControllingCustomer(BuyerSupplierLinksHelper.GetControllingCustomer(), forceDefaulting: false);
			RefCountryRulesHelper.AddRulesToNotes(OriginUNLOCO?.Country, DestinationUNLOCO?.Country, Notes, TransportMode, IsInDatabase, true);
			Validation.ValidateAll();
			base.RunPreSaveValidationCore();
		}

		public QuotedBookingValidation Validation
		{
			get { return GetNewValidation(); }
		}

		protected virtual QuotedBookingValidation GetNewValidation()
		{
			return new QuotedBookingValidation(this);
		}

		#endregion

		#region Quote Events

		void HookQuoteEvents()
		{
			if (Quote != null)
			{
				Quote.CurrentOneOffQuote.IsDomesticFreightInfo.ValueChanged += delegate
				{ IsDomesticFreightInfo.RefreshBinding(); };
				Quote.CurrentOneOffQuote.TT_ActualVolumeInfo.ValueChanged += delegate
				{ VolumeInfo.RefreshBinding(); };
				Quote.CurrentOneOffQuote.TT_UnitOfVolumeInfo.ValueChanged += delegate
				{ VolumeInfo.RefreshBinding(); };
				Quote.CurrentOneOffQuote.TT_ActualWeightInfo.ValueChanged += Weight_ValueChanged;
				Quote.CurrentOneOffQuote.TT_UnitOfWeightInfo.ValueChanged += Weight_ValueChanged;
				Quote.CurrentOneOffQuote.TT_ChargeableInfo.ValueChanged += delegate
				{ ChargeableInfo.RefreshBinding(); };
				Quote.CurrentOneOffQuote.TT_RH_NKCommodityInfo.ValueChanged += Commodity_ValueChanged;
				Quote.CurrentOneOffQuote.PickUpDocAddress.DocAddressChanged += new EventHandler(QuotePickupDocAddress_DocAddressChanged);
				Quote.CurrentOneOffQuote.PickUpDocAddress.E2_OA_AddressInfo.ValueChanged += new EventHandler(QuotePickupDocAddress_DocAddressChanged);
				Quote.CurrentOneOffQuote.DeliveryDocAddress.DocAddressChanged += new EventHandler(QuoteDeliveryDocAddress_DocAddressChanged);
				Quote.CurrentOneOffQuote.DeliveryDocAddress.E2_OA_AddressInfo.ValueChanged += new EventHandler(QuoteDeliveryDocAddress_DocAddressChanged);
				Quote.TH_IsCancelledInfo.ValueChanged += new EventHandler(IsCancelledInfo_ValueChanged);
				HookOneOffQuoteContainers();
			}
		}

		void HookOneOffQuoteContainers()
		{
			HookOOQContainerEventForRequireTEU();
			HookRequiresTemperatureControlEvents(Quote.CurrentOneOffQuote.Containers, container => [container.TC_RCInfo]);
		}

		void HookOOQContainerEventForRequireTEU()
		{
			if (ObjectState != QuotedBookingState.QuoteOnly)
			{
				return;
			}

			originalRequireTEU = (this as ICO2eCalculationSupporter).RequireTEU;
			void ContainerTEUChanged(object s, EventArgs e)
			{
				if (!this.SkipCO2eStatusCheck())
				{
					var newRequireTEU = (this as ICO2eCalculationSupporter).RequireTEU;

					if ((newRequireTEU || originalRequireTEU) && !IsDeleted && !IsDeleting)
					{
						UpdateQuotedBookingCO2eStatusToNotCurrent(new CO2eStatusChangedReason(e as ValueChangedEventArgs));
					}

					originalRequireTEU = newRequireTEU;
				}
			}

			foreach (var container in Quote.CurrentOneOffQuote.Containers.Cast<RateOneOffContainers>())
			{
				container.TC_ContainerCountInfo.ValueChanged += ContainerTEUChanged;
				container.TC_RCInfo.ValueChanged += ContainerTEUChanged;
			}

			Quote.CurrentOneOffQuote.Containers.CountChanged += (sender, args) =>
			{
				if (!(args.BizObject is RateOneOffContainers container))
				{
					return;
				}

				if (!this.SkipCO2eStatusCheck())
				{
					var newRequireTEU = (this as ICO2eCalculationSupporter).RequireTEU;
					if ((newRequireTEU || originalRequireTEU) && !IsDeleted && !IsDeleting)
					{
						UpdateQuotedBookingCO2eStatusToNotCurrent(new CO2eStatusChangedReason(args, (NoResString)"One Off Quote container"));
					}
					originalRequireTEU = newRequireTEU;
				}

				if (args.ItemAdded)
				{
					container.TC_ContainerCountInfo.ValueChanged += ContainerTEUChanged;
					container.TC_RCInfo.ValueChanged += ContainerTEUChanged;
				}
				else if (args.ItemRemoved)
				{
					container.TC_ContainerCountInfo.ValueChanged -= ContainerTEUChanged;
					container.TC_RCInfo.ValueChanged -= ContainerTEUChanged;
				}
			};
		}

		#region Commodity Changed

		void Commodity_ValueChanged(object sender, EventArgs e)
		{
			if (QuotedBookingContainers != null)
			{
				QuotedBookingContainers.CommodityCode = Commodity;
			}
		}

		void Mode_ValueChanged(object sender, EventArgs e)
		{
			var mode = Mode;
			if (GetFromBooking)
			{
				if (ContainerMode != Core.Constants.ContainerModes.ULD && (Booking.OuterPackLines?.Any() ?? false))
				{
					foreach (var item in Booking.OuterPackLines.Cast<ForwardingPackLine>())
					{
						item.LooseCargoContainerType = string.Empty;
					}
				}

				if (!((ISailingChooserParent)this).IsDirectEnabled)
				{
					Booking.JS_IsDirectBooking = false;
				}
			}

			if (SetOnQuote)
			{
				if (!IsValidationSuspended)
				{
					Validation.ValidateTransportMode();
					Validation.ValidateContainerMode();
					Validation.ValidateMode();
				}
				if (TransitTime_ReadOnly || !TransitTimesList.ContainsCode(Quote.CurrentOneOffQuote.TT_TransitTime))
				{
					Quote.CurrentOneOffQuote.TT_TransitTime = ZString.Empty;
				}
			}

			if (!mode.IsEmpty)
			{
				IsCompareMode = false;
			}

			if (!ContainerMode.IsEmpty)
			{
				UpdateCollectionsFromMode();
			}

			if (!IsSettingDefaultValues)
			{
				((IDefaultNumberOfDecimalsSupporter)this).RoundMeasurePropertiesOnTransportModeChanged();
			}

			if (!IsValidationSuspended)
			{
				Validation.ValidateLoadPort();
				Validation.ValidateDischargePort();
				Validation.ValidateExportReceivingDepot();
				Validation.ValidateImportReleaseDepot();
				Validation.ValidateCarrierContractNumber();
			}
		}

		#endregion

		#region DocAddressChanged

		void QuotePickupDocAddress_DocAddressChanged(object sender, EventArgs e)
		{
			if (ObjectState == QuotedBookingState.QuoteOnly)
			{
				ZBool shouldDefault = !FreightDataRegistry.Instance.DefaultShipmentOriginFromConsolLoad.Value;
				if (shouldDefault)
				{
					ZString newOriginCode;
					if (!Quote.CurrentOneOffQuote.PickUpDocAddress.E2_AddressOverride &&
						Quote.CurrentOneOffQuote.PickUpDocAddress.Address != null &&
						!Quote.CurrentOneOffQuote.PickUpDocAddress.Address.OA_RL_NKRelatedPortCode.IsEmpty)
					{
						newOriginCode = Quote.CurrentOneOffQuote.PickUpDocAddress.Address.OA_RL_NKRelatedPortCode;
					}
					else
					{
						newOriginCode = Consignor != null ? Consignor.OH_RL_NKClosestPort : ZString.Empty;
					}

					if (!newOriginCode.IsEmpty && (!IsInDatabase || Origin.IsEmpty))
					{
						Origin = newOriginCode;
					}
				}
			}
		}

		void QuoteDeliveryDocAddress_DocAddressChanged(object sender, EventArgs e)
		{
			if (ObjectState == QuotedBookingState.QuoteOnly)
			{
				ZBool shouldDefault = !FreightDataRegistry.Instance.DefaultShipmentDestinationFromConsolDischarge.Value;
				if (shouldDefault)
				{
					ZString newDestination;
					if (!Quote.CurrentOneOffQuote.DeliveryDocAddress.E2_AddressOverride &&
						Quote.CurrentOneOffQuote.DeliveryDocAddress.Address != null &&
						!Quote.CurrentOneOffQuote.DeliveryDocAddress.Address.OA_RL_NKRelatedPortCode.IsEmpty)
					{
						newDestination = Quote.CurrentOneOffQuote.DeliveryDocAddress.Address.OA_RL_NKRelatedPortCode;
					}
					else
					{
						newDestination = Consignee != null ? Consignee.OH_RL_NKClosestPort : ZString.Empty;
					}

					if (!newDestination.IsEmpty && (!IsInDatabase || Destination.IsEmpty))
					{
						Destination = newDestination;
					}
				}
			}
		}

		#endregion

		#endregion

		#region Booking Events

		void HookBookingEvents()
		{
			if (Booking != null)
			{
				Booking.IsDomesticFreightInfo.ValueChanged += delegate
				{ IsDomesticFreightInfo.RefreshBinding(); };
				Booking.JS_RL_NKOriginInfo.ValueChanged += new EventHandler(JS_RL_NKOriginInfo_ValueChanged);
				Booking.JS_RL_NKDestinationInfo.ValueChanged += new EventHandler(JS_RL_NKDestinationInfo_ValueChanged);
				Booking.JS_INCOInfo.ValueChanged += new EventHandler(JS_INCOInfo_ValueChanged);
				Booking.JS_ActualVolumeInfo.ValueChanged += delegate
				{ VolumeInfo.RefreshBinding(); };
				Booking.JS_ActualWeightInfo.ValueChanged += Weight_ValueChanged;
				Booking.JS_UnitOfWeightInfo.ValueChanged += Weight_ValueChanged;
				Booking.JS_ActualChargeableInfo.ValueChanged += delegate
				{ ChargeableInfo.RefreshBinding(); };
				Booking.JS_OA_ExportReceivingDepotInfo.ValueChanged += new EventHandler(JS_OA_ExportReceivingDepotInfo_ValueChanged);
				Booking.JS_OA_ImportReleaseDepotInfo.ValueChanged += new EventHandler(JS_OA_ImportReleaseDepotInfo_ValueChanged);
				Booking.JS_IsCancelledInfo.ValueChanged += new EventHandler(IsCancelledInfo_ValueChanged);
				Booking.JS_HBLAWBChargesDisplayInfo.ValueChanged += delegate
				{ HBLAWBChargesDisplayInfo.RefreshBinding(); };
				Booking.OnSavingShipment += delegate
				{ Booking.MAWBAllocator = ScheduleChooser.GetMAWBAllocator(); };
				Booking.OnShipmentSaved += delegate
				{ notes?.ReloadIfLoaded(); };
				Booking.GetDefaultImportBrokerFromBuyerSupplierRelationshipDelegate = GetDefaultImportBrokerFromBuyerSupplierRelationship;
				Booking.JS_MarksAndNumbersInfo.ValueChanged += delegate
				{ notes?.ReloadIfLoaded(); };
				Booking.JS_JXInfo.ValueChanged += JS_JXInfoOnValueChanged;
				Booking.JS_RH_NKRateCommodityInfo.ValueChanged += Commodity_ValueChanged;
				Booking.BookedShippingLinePKInfo.ValueChanged += BookedShippingLinePK_ValueChanged;
				HookPackLineEvents();
			}
		}

		void Weight_ValueChanged(object sender, EventArgs e)
		{
			WeightInfo.RefreshBinding();
			if (e is ValueChangedEventArgs args)
			{
				UpdateQuotedBookingCO2eStatusToNotCurrent(new CO2eStatusChangedReason(args));
			}
		}

		void BookedShippingLinePK_ValueChanged(object sender, EventArgs e)
		{
			if (e is ValueChangedEventArgs args)
			{
				UpdateQuotedBookingCO2eStatusToNotCurrent(new CO2eStatusChangedReason(args));
			}
		}

		void JS_JXInfoOnValueChanged(object sender, EventArgs e)
		{
			UpdateQuotedBookingCO2eStatusToNotCurrent(new CO2eStatusChangedReason(e as ValueChangedEventArgs));
		}

		void OuterPackLines_OnSettingDefaultCommodityForNewChild(PackLine line)
		{
			if (!Commodity.IsEmpty)
			{
				line.JL_RH_NKCommodityCode = Commodity;
			}
		}

		void IsCancelledInfo_ValueChanged(object sender, EventArgs e)
		{
			SetReadOnlyIncludingChildren(((ICancellable)this).IsCancelled);
		}

		void JS_RL_NKOriginInfo_ValueChanged(object sender, EventArgs e)
		{
			if (ObjectState == QuotedBookingState.AcceptedBookingWithQuote)
			{
				Quote.CurrentOneOffQuote.TT_RL_NKReceivalLocation = Origin;
			}

			if (!Origin.IsEmpty &&
				DefaultsLoadDischargeFromOriginDestination &&
				Origin != LoadPort &&
					(Globals.IsWeb ||
					LoadPort.IsEmpty ||
					LoadPort.Length < 5))
			{
				LoadPort = Origin;
			}

			if (this.IsCrossTrade() != PrevIsCrossTrade)
			{
				PrevIsCrossTrade = this.IsCrossTrade();
				OnCrossTradeChange();
			}

			OriginInfo.RefreshBinding();
		}

		void JS_RL_NKDestinationInfo_ValueChanged(object sender, EventArgs e)
		{
			if (ObjectState == QuotedBookingState.AcceptedBookingWithQuote)
			{
				Quote.CurrentOneOffQuote.TT_RL_NKDeliveryLocation = Destination;
			}

			if (!Destination.IsEmpty &&
				DefaultsLoadDischargeFromOriginDestination &&
				Destination != DischargePort &&
					(Globals.IsWeb ||
					DischargePort.IsEmpty ||
					DischargePort.Length < 5))
			{
				DischargePort = Destination;
			}

			if (this.IsCrossTrade() != PrevIsCrossTrade)
			{
				PrevIsCrossTrade = this.IsCrossTrade();
				OnCrossTradeChange();
			}

			DestinationInfo.RefreshBinding();
		}

		protected virtual bool DefaultsLoadDischargeFromOriginDestination
		{
			get { return defaultLoadDischargeFromOriginDestination; }
		}

		public void EnableLoadDischargeDefaulting() => defaultLoadDischargeFromOriginDestination = true;

		public void DisableLoadDischargeDefaulting() => defaultLoadDischargeFromOriginDestination = false;

		bool defaultLoadDischargeFromOriginDestination = true;

		void JS_INCOInfo_ValueChanged(object sender, EventArgs e)
		{
			if (ObjectState == QuotedBookingState.AcceptedBookingWithQuote)
			{
				Quote.CurrentOneOffQuote.TT_IncoTerm = PaymentTerms;
			}

			PaymentTermsInfo.RefreshBinding();
		}

		void JS_OA_ExportReceivingDepotInfo_ValueChanged(object sender, EventArgs e)
		{
			var isCFSDepot = false;

			if (Booking.ExportReceivingDepot != null && Booking.ExportReceivingDepot.Header != null)
			{
				isCFSDepot = Booking.ExportReceivingDepot.Header.IsProxyOrgOfAnyCompany();
			}

			if (isCFSDepot)
			{
				if (Booking.Origin != null && (Booking.IsSea || Booking.IsAir))
				{
					OrgAddress agent = Booking.Origin.GetBestAgent(Booking.TransportMode, AgentDirectionList.Codes.Export);
					Booking.JS_OH_HandledOnBehalfOfForwarder = (agent != null) ? agent.Header.PK : ZGuid.Empty;
				}

				if (Booking.JS_OH_HandledOnBehalfOfForwarder.IsEmpty && Booking.ExportReceivingDepot != null)
				{
					Booking.JS_OH_HandledOnBehalfOfForwarder = Booking.ExportReceivingDepot.Header.PK;
				}
			}
			else
			{
				Booking.JS_OH_HandledOnBehalfOfForwarder = ZGuid.Empty;
			}

			ExportReceivingDepotInfo.RefreshBinding();
		}

		void JS_OA_ImportReleaseDepotInfo_ValueChanged(object sender, EventArgs e)
		{
			ImportReleaseDepotInfo.RefreshBinding();
		}

		ZGuid? GetDefaultImportBrokerFromBuyerSupplierRelationship()
		{
			return Consignee != null && !Booking.IsChangingConsigneeAddress && BuyerSupplierLinksHelper != null && !IsInDatabase
				? BuyerSupplierLinksHelper.GetImportBrokerFromBuyerSupplierLink()
				: null;
		}

		void HookPackLineEvents()
		{
			Booking.OuterPackLines.OnSettingDefaultCommodityForNewChild += OuterPackLines_OnSettingDefaultCommodityForNewChild;
			HookRequiresTemperatureControlEvents(Booking.OuterPackLines, packLine => [packLine.JL_RequiredTemperatureMinimumInfo, packLine.JL_RequiredTemperatureMaximumInfo]);
		}

		void HookRequiresTemperatureControlEvents<E>(BusinessObjectCollection<E> collection, Func<E, List<ZPropertyInfo>> infosGetter)
			where E : BusinessObject
		{
			CO2eHelper.HookUpdateToNotCurrentEvents(this,
				collection,
				() => this.oldRequiresTemperatureControl,
				n => oldRequiresTemperatureControl = n,
				provider => (provider as ICO2eLegBasedSupporter).RequiresTemperatureControl,
				infosGetter,
				e => e switch
				{
					ValueChangedEventArgs args => new CO2eStatusChangedReason(args),
					CollectionCountChangedEventArgs args => new CO2eStatusChangedReason(args, args.BizObject is IPackTypeDafaultable ? $"Loose Cargo" : $"Container"),
					_ => CO2eStatusChangedReason.Empty
				});
		}

		#endregion

		#region Discrepancy

		public void AcceptDiscrepancy()
		{
			if (ObjectState != QuotedBookingState.UnacceptedBookingWithQuote)
			{
				throw new InvalidOperationException("Invalid State to Accept Discrepancy");
			}

			Quote.TH_Accepted = ZDateTime.Now;
			SynchDocAddressesBookingWithQuote();
		}

		public enum DiscrepancyCheck
		{
			Client,
			TransportMode,
			ContainerMode,
			Inco,
			Pickup,
			Delivery,
			Service,
			Origin,
			Destination,
			Carrier,
			CarrierServiceLevel,
			Commodity,
			FMCTariffID,
			Weight,
			WeightUnit,
			Volume,
			VolumeUnit,
			Chargeable,
			GoodsValue,
			GoodsCurrency,
			InsuranceValue,
			InsuranceCurrency,
			PackCount,
			PackType,
			AdditionalTerms,
			CompanyTariffLevelOverride,
			HBLDeliveryMode,
		}

		public bool HasDiscrepancyFor(DiscrepancyCheck discrepancyCheck)
		{
			switch (discrepancyCheck)
			{
				case DiscrepancyCheck.Carrier:
					return (Booking.JS_OA_BookedShippingLineAddress != (Quote.CurrentOneOffQuote.Carrier?.MainAddress.PK ?? ZGuid.Empty));

				case DiscrepancyCheck.CarrierServiceLevel:
					return (Booking.JS_PL_NKCarrierServiceLevel != Quote.CurrentOneOffQuote.TT_PL_NKCarrierServiceLevel);

				case DiscrepancyCheck.Commodity:
					return (Booking.JS_RH_NKRateCommodity != Quote.CurrentOneOffQuote.TT_RH_NKCommodity);

				case DiscrepancyCheck.FMCTariffID:
					return (Booking.JS_FMCTariffID != Quote.CurrentOneOffQuote.TT_FMCTariffID);

				case DiscrepancyCheck.Client:
					return (Job == null || Job.LocalChargesPK != Quote.TH_OH);

				case DiscrepancyCheck.TransportMode:
					return (Booking.JS_TransportMode != Quote.CurrentOneOffQuote.TT_TransportMode);

				case DiscrepancyCheck.ContainerMode:
					return (Booking.JS_PackingMode != Quote.CurrentOneOffQuote.TT_ContainerMode);

				case DiscrepancyCheck.Inco:
					return (Booking.JS_INCO != Quote.CurrentOneOffQuote.TT_IncoTerm);

				case DiscrepancyCheck.Pickup:
					return (!Booking.ConsignorDocumentaryAddress.IsTheSameAddressAs(Quote.CurrentOneOffQuote.PickUpDocAddress));

				case DiscrepancyCheck.Delivery:
					return (!Booking.ConsigneeDocumentaryAddress.IsTheSameAddressAs(Quote.CurrentOneOffQuote.DeliveryDocAddress));

				case DiscrepancyCheck.Service:
					return (Booking.JS_RS_NKServiceLevel != Quote.CurrentOneOffQuote.TT_RS_NKServiceLevel);

				case DiscrepancyCheck.Origin:
					return (Booking.JS_RL_NKOrigin != Quote.CurrentOneOffQuote.TT_RL_NKReceivalLocation);

				case DiscrepancyCheck.Destination:
					return (Booking.JS_RL_NKDestination != Quote.CurrentOneOffQuote.TT_RL_NKDeliveryLocation);

				case DiscrepancyCheck.Weight:
					return (Booking.JS_ActualWeight != Quote.CurrentOneOffQuote.TT_ActualWeight);

				case DiscrepancyCheck.WeightUnit:
					return (Booking.JS_UnitOfWeight != Quote.CurrentOneOffQuote.TT_UnitOfWeight);

				case DiscrepancyCheck.Volume:
					return (Booking.JS_ActualVolume != Quote.CurrentOneOffQuote.TT_ActualVolume);

				case DiscrepancyCheck.VolumeUnit:
					return (Booking.JS_UnitOfVolume != Quote.CurrentOneOffQuote.TT_UnitOfVolume);

				case DiscrepancyCheck.Chargeable:
					return (Booking.JS_ActualChargeable != Quote.CurrentOneOffQuote.TT_Chargeable);

				case DiscrepancyCheck.GoodsValue:
					return (Booking.JS_GoodsValue != Quote.CurrentOneOffQuote.TT_ValueOfGoods);

				case DiscrepancyCheck.GoodsCurrency:
					return (Booking.JS_RX_NKGoodsValueCurr != Quote.CurrentOneOffQuote.TT_RX_NKGoodsCurrency);

				case DiscrepancyCheck.InsuranceValue:
					return (Booking.JS_InsuranceValue != Quote.CurrentOneOffQuote.TT_InsureVal);

				case DiscrepancyCheck.InsuranceCurrency:
					return (Booking.JS_RX_NKInsuranceCurrency != Quote.CurrentOneOffQuote.TT_RX_NKInsureValCurr);

				case DiscrepancyCheck.PackCount:
					return (Booking.JS_OuterPacks != GetQuoteTotalLoosePackCount());

				case DiscrepancyCheck.PackType:
					return (Booking.JS_F3_NKPackType != GetQuotePackType());

				case DiscrepancyCheck.AdditionalTerms:
					return (Booking.JS_AdditionalTerms != Quote.CurrentOneOffQuote.TT_AdditionalTerms);

				case DiscrepancyCheck.CompanyTariffLevelOverride:
					return (Booking.JS_CompanyTariffLevelOverride != Quote.CurrentOneOffQuote.TT_CompanyTariffLevelOverride);

				case DiscrepancyCheck.HBLDeliveryMode:
					return (Booking.JS_HBLContainerPackModeOverride != Quote.CurrentOneOffQuote.TT_HBLDeliveryMode);

				default:
					throw new ArgumentException("Field doesn't exist. Please add");
			}
		}

		#endregion

		#region Convert Quote to QuotedBooking

		public void ConvertQuoteToQuotedBooking()
		{
			// Create Booking
			var booking = Factory.Load<ForwardingShipment>(new ZQuery(JobShipmentSchema.JS_TH_OneTimeQuote, quotePK)).FirstOrDefault();
			var createNewBooking = booking == null;

			using (ShipmentFieldStateChange.InitializingShipment(Factory))
			{
				// When OOQ converted, status should be accepted
				Quote.TH_Accepted = ZDateTime.Now;

				if (createNewBooking)
				{
					booking = CreateNewBooking(Factory);
					booking.JS_TH_OneTimeQuote = quotePK;
				}

				bookingPK = booking.PK;

				var view = ViewQuotedBooking.LoadOrCreate(this);
				view.VB_JS = bookingPK;

				CleanupPossibleCarriers();

				// Copy values from Quote to Booking
				CopyQuoteValuesToBooking();
				HookBookingEvents();
				RegisterEditableChildObject(Booking);
				UnRegisterEditableChildObject(Quote);

				UpdateNotesCollectionMasterToBooking();

				LoadPort = Origin;
				DischargePort = Destination;

				// Now in a Converted State. Once Quote accepted. Will become a QuotedBooking (synch with quote)
				UpdateCollectionsFromMode();

				//Ensure Consignor/ee are loaded from the shipment once converted
				fConsignorDocumentaryAddress = null;
				fConsigneeDocumentaryAddress = null;

				AddBQWEvent();

				(JobCO2eCollection as IActiveBusinessObjectCollection).DeleteAll();
			}

			if (createNewBooking)
			{
				this.BusinessObjectRelationChanged(Booking, BusinessObjectParentLocatorEvent.ParentAdded);
			}
		}

		void CleanupPossibleCarriers()
		{
			var oneOffQuote = Quote.CurrentOneOffQuote;
			var possibleCarriers = oneOffQuote?.PossibleCarriers;
			if (possibleCarriers == null || !possibleCarriers.Any())
			{
				return;
			}

			DeleteUnusedPossibleCarrierCharges(possibleCarriers, oneOffQuote.TT_OH_Carrier, oneOffQuote.TT_OH_Creditor);
			possibleCarriers.RemoveAndDeleteAll();
		}

		void DeleteUnusedPossibleCarrierCharges(RateOneOffCarrierCollection possibleCarriers, ZGuid mainCarrierPK, ZGuid mainCreditorPK)
		{
			var job = Job;
			if (job == null)
			{
				return;
			}

			var orgPKsToRemove = possibleCarriers.Cast<RateOneOffCarrier>()
				.SelectMany(x => new[] { x.TTC_OH_Carrier, x.TTC_OH_Creditor })
				.Where(pk => pk != mainCarrierPK && pk != mainCreditorPK)
				.ToHashSet();

			var charges = Factory.Load<JobCharge>(new ZQuery(JobChargeSchema.JR_JH, job.PK));
			foreach (var charge in charges)
			{
				DeleteChargeIfUnrelated(charge, orgPKsToRemove);
			}
		}

		void DeleteChargeIfUnrelated(JobCharge charge, HashSet<ZGuid> orgPKsToRemove)
		{
			var costOrgPK = charge.JR_OH_CostAccount;
			if (!costOrgPK.IsEmpty && orgPKsToRemove.Contains(costOrgPK))
			{
				charge.Delete();
			}
		}

		void AddBQWEvent()
		{
			//Add BQW Event
			if (((IStmALogParent)Quote).Logs.HasLogWith(StmALogSchema.SL_SE_NKEvent, Events.QuoteAutoratedWithWiseRatesCode))
			{
				EventHandler saving = null;
				EventHandler saved = null;

				saving = (sender, args) =>
				{
					var logReference = string.Format(CultureInfo.InvariantCulture, (NoResString)"RatesServiceUsage:Quote {0},Booking {1}", // Log Reference Values should be in English Only
						Quote.TH_QuoteNumber,
						Booking.JS_UniqueConsignRef);

					var log = Booking.Logs.AddNew(Events.BookingWithQuoteAutoratedWithRatesService, logReference, ZDateTimeOffset.Now, true);

					saved = (savedSender, savedArgs) =>
					{
						Booking.OnShipmentSaved -= saved;
						if (((CommonShipment.SavedEventArgs)savedArgs).HasSaveSucceeded)
						{
							Booking.OnSavingShipment -= saving;
						}
						else
						{
							log.Delete();
						}
					};

					Booking.OnShipmentSaved += saved;
				};

				Booking.OnSavingShipment += saving;
			}
		}

		void UpdateNotesCollectionMasterToBooking()
		{
			foreach (QuotedBookingStmNote note in Notes.GetAllNotes())
			{
				note.Master = this;
			}
		}

		void SynchDocAddressesBookingWithQuote()
		{
			Quote.CurrentOneOffQuote.PickUpDocAddress.SynchroniseWithParent(Booking.ConsignorDocumentaryAddress);
			Quote.CurrentOneOffQuote.DeliveryDocAddress.SynchroniseWithParent(Booking.ConsigneeDocumentaryAddress);
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "Can't be made simpler really.")]
		public void CopyQuoteValuesToBooking()
		{
			if (ObjectState == QuotedBookingState.QuoteOnly)
			{
				throw new NotSupportedException("State should have a booking");
			}

			if (HasDiscrepancyFor(DiscrepancyCheck.Client) && Job != null)
			{
				if (Quote.CurrentOneOffQuote.TT_OrgRole == Core.Constants.OrgRoles.OverseasAgent)
				{
					Job.AgentCollectPK = Quote.TH_OH;
				}
				else if (Quote.CurrentOneOffQuote.TT_OrgRole == Core.Constants.OrgRoles.LocalClient)
				{
					Job.LocalChargesPK = Quote.TH_OH;
				}
			}

			if (HasDiscrepancyFor(DiscrepancyCheck.Pickup) && !Quote.CurrentOneOffQuote.PickUpDocAddress.IsEmpty)
			{
				Booking.ConsignorDocumentaryAddress.SynchroniseWithParent(Quote.CurrentOneOffQuote.PickUpDocAddress);
				Booking.ConsignorDocumentaryAddress.DeSynchroniseWithParent();

				Booking.ConsignorPickupAddress.SynchroniseWithParent(Quote.CurrentOneOffQuote.PickUpDocAddress);
				Booking.ConsignorPickupAddress.DeSynchroniseWithParent();
			}

			if (HasDiscrepancyFor(DiscrepancyCheck.Delivery) && !Quote.CurrentOneOffQuote.DeliveryDocAddress.IsEmpty)
			{
				Booking.ConsigneeDocumentaryAddress.SynchroniseWithParent(Quote.CurrentOneOffQuote.DeliveryDocAddress);
				Booking.ConsigneeDocumentaryAddress.DeSynchroniseWithParent();

				Booking.ConsigneeDeliveryAddress.SynchroniseWithParent(Quote.CurrentOneOffQuote.DeliveryDocAddress);
				Booking.ConsigneeDeliveryAddress.DeSynchroniseWithParent();
			}

			if (HasDiscrepancyFor(DiscrepancyCheck.TransportMode) || HasDiscrepancyFor(DiscrepancyCheck.ContainerMode))
			{
				Booking.JS_TransportMode = Quote.CurrentOneOffQuote.TT_TransportMode;
				Booking.JS_PackingMode = Quote.CurrentOneOffQuote.BookingContainerMode;
			}

			if (HasDiscrepancyFor(DiscrepancyCheck.Service))
			{
				Booking.JS_RS_NKServiceLevel = Quote.CurrentOneOffQuote.TT_RS_NKServiceLevel;
			}

			if (HasDiscrepancyFor(DiscrepancyCheck.Origin))
			{
				Booking.JS_RL_NKOrigin = Quote.CurrentOneOffQuote.TT_RL_NKReceivalLocation;
			}

			if (HasDiscrepancyFor(DiscrepancyCheck.Destination))
			{
				Booking.JS_RL_NKDestination = Quote.CurrentOneOffQuote.TT_RL_NKDeliveryLocation;
			}

			if (HasDiscrepancyFor(DiscrepancyCheck.Weight))
			{
				Booking.JS_ActualWeight = Quote.CurrentOneOffQuote.TT_ActualWeight;
			}

			if (HasDiscrepancyFor(DiscrepancyCheck.WeightUnit))
			{
				using (Booking.SuspendSettingActualChargeable())
				{
					Booking.JS_UnitOfWeight = Quote.CurrentOneOffQuote.TT_UnitOfWeight;
				}
			}

			if (HasDiscrepancyFor(DiscrepancyCheck.Volume))
			{
				Booking.JS_ActualVolume = Quote.CurrentOneOffQuote.TT_ActualVolume;
			}

			if (HasDiscrepancyFor(DiscrepancyCheck.VolumeUnit))
			{
				using (Booking.SuspendSettingActualChargeable())
				{
					Booking.JS_UnitOfVolume = Quote.CurrentOneOffQuote.TT_UnitOfVolume;
				}
			}

			if (HasDiscrepancyFor(DiscrepancyCheck.Chargeable))
			{
				Booking.JS_ActualChargeable = Quote.CurrentOneOffQuote.TT_Chargeable;
			}

			if (HasDiscrepancyFor(DiscrepancyCheck.GoodsValue))
			{
				Booking.JS_GoodsValue = Quote.CurrentOneOffQuote.TT_ValueOfGoods;
			}

			if (HasDiscrepancyFor(DiscrepancyCheck.GoodsCurrency))
			{
				Booking.JS_RX_NKGoodsValueCurr = Quote.CurrentOneOffQuote.TT_RX_NKGoodsCurrency;
			}

			if (HasDiscrepancyFor(DiscrepancyCheck.InsuranceValue))
			{
				Booking.JS_InsuranceValue = Quote.CurrentOneOffQuote.TT_InsureVal;
			}

			if (HasDiscrepancyFor(DiscrepancyCheck.InsuranceCurrency))
			{
				Booking.JS_RX_NKInsuranceCurrency = Quote.CurrentOneOffQuote.TT_RX_NKInsureValCurr;
			}

			if (HasDiscrepancyFor(DiscrepancyCheck.Inco))
			{
				Booking.JS_INCO = Quote.CurrentOneOffQuote.TT_IncoTerm;
			}

			if (HasDiscrepancyFor(DiscrepancyCheck.PackCount))
			{
				Booking.JS_OuterPacks = GetQuoteTotalLoosePackCount();
			}

			if (HasDiscrepancyFor(DiscrepancyCheck.PackType))
			{
				Booking.JS_F3_NKPackType = GetQuotePackType();
			}

			if (HasDiscrepancyFor(DiscrepancyCheck.Carrier))
			{
				Booking.JS_OA_BookedShippingLineAddress = Quote.CurrentOneOffQuote.Carrier?.MainAddress.PK ?? ZGuid.Empty;
			}

			if (HasDiscrepancyFor(DiscrepancyCheck.CarrierServiceLevel))
			{
				Booking.JS_PL_NKCarrierServiceLevel = Quote.CurrentOneOffQuote.TT_PL_NKCarrierServiceLevel;
			}

			if (HasDiscrepancyFor(DiscrepancyCheck.Commodity))
			{
				Booking.JS_RH_NKRateCommodity = Quote.CurrentOneOffQuote.TT_RH_NKCommodity;
			}

			if (HasDiscrepancyFor(DiscrepancyCheck.FMCTariffID))
			{
				Booking.JS_FMCTariffID = Quote.CurrentOneOffQuote.TT_FMCTariffID;
			}

			if (HasDiscrepancyFor(DiscrepancyCheck.AdditionalTerms))
			{
				Booking.JS_AdditionalTerms = Quote.CurrentOneOffQuote.TT_AdditionalTerms;
			}

			if (HasDiscrepancyFor(DiscrepancyCheck.CompanyTariffLevelOverride))
			{
				Booking.JS_CompanyTariffLevelOverride = Quote.CurrentOneOffQuote.TT_CompanyTariffLevelOverride;
			}

			if (HasDiscrepancyFor(DiscrepancyCheck.HBLDeliveryMode))
			{
				Booking.JS_HBLContainerPackModeOverride = Quote.CurrentOneOffQuote.TT_HBLDeliveryMode;
			}

			Booking.JS_OH_Creditor = Quote.CurrentOneOffQuote.TT_OH_Creditor;

			Booking.DocsAndCartage.JP_FCLPickupEquipmentNeeded = Quote.CurrentOneOffQuote.TT_PickupEquipment;
			Booking.DocsAndCartage.JP_FCLDeliveryEquipmentNeeded = Quote.CurrentOneOffQuote.TT_DeliveryEquipment;

			CopyOuterPackLinesToBooking();
			CopyContainersToBooking();
			CopyNumbersToBooking();
		}

		void CopyOuterPackLinesToBooking()
		{
			Booking.OuterPackLines.RemoveAndDeleteAll();

			foreach (var packLine in Quote.CurrentOneOffQuote.LooseCargo)
			{
				ForwardingPackLine bookingPackLine = Booking.OuterPackLines.AddNew();

				bookingPackLine.JL_PackageCount = packLine.TPL_PackLineCount;
				bookingPackLine.JL_F3_NKPackType = packLine.TPL_F3_NKPackType;

				bookingPackLine.JL_Length = packLine.TPL_Length;
				bookingPackLine.JL_Width = packLine.TPL_Width;
				bookingPackLine.JL_Height = packLine.TPL_Height;
				bookingPackLine.JL_UnitOfDimension = packLine.TPL_DimensionUQ;

				bookingPackLine.JL_ActualWeight = packLine.TPL_Weight;
				bookingPackLine.JL_ActualWeightUQ = packLine.TPL_WeightUQ;

				bookingPackLine.JL_ActualVolumeUQ = packLine.TPL_VolumeUQ;
				bookingPackLine.JL_ActualVolume = packLine.TPL_Volume;

				if (Constants.RateMode.ULD.Equals(Quote.CurrentOneOffQuote.TT_ContainerMode, StringComparison.InvariantCultureIgnoreCase))
				{
					bookingPackLine.JL_RC_ContainerType = packLine.TPL_RC_RefContainer;
				}

				if (Constants.ContainerModes.RollOnRollOff.Equals(Quote.CurrentOneOffQuote.TT_ContainerMode, StringComparison.InvariantCultureIgnoreCase))
				{
					bookingPackLine.JL_VehicleMake = packLine.TPL_VehicleMake;
					bookingPackLine.JL_VehicleModel = packLine.TPL_VehicleModel;
					bookingPackLine.JL_VehicleYear = packLine.TPL_VehicleYear;
					bookingPackLine.JL_VehicleColor = packLine.TPL_VehicleColor;
					bookingPackLine.JL_VehicleNumberOfDoors = packLine.TPL_VehicleNumberOfDoors;
					bookingPackLine.JL_VehicleTransmission = packLine.TPL_VehicleTransmission;
					bookingPackLine.JL_RefNumber = packLine.TPL_RefVehicleIdentificationNumber;
				}

				if (!Commodity.IsEmpty)
				{
					bookingPackLine.JL_RH_NKCommodityCode = Commodity;
				}
			}
		}

		void CopyNumbersToBooking()
		{
			Booking.Numbers.RemoveAndDeleteAll();
			foreach (CusEntryNumber number in Quote.CurrentOneOffQuote.Numbers)
			{
				var bookingNumber = Booking.Numbers.AddNew();
				bookingNumber.CE_EntryNum = number.CE_EntryNum;
				bookingNumber.CE_EntryType = number.CE_EntryType;
				bookingNumber.CE_EntryLineReference = number.CE_EntryLineReference;
				bookingNumber.CE_EntryStatus = number.CE_EntryStatus;
				bookingNumber.CE_Category = number.CE_Category;
				bookingNumber.CE_IssueDate = number.CE_IssueDate;
				bookingNumber.CE_RN_NKCountryCode = number.CE_RN_NKCountryCode;
				bookingNumber.CE_ExpiryDate = number.CE_ExpiryDate;
			}
		}

		void CopyContainersToBooking()
		{
			QuotedBookingContainers.RemoveAndDeleteAll();
			foreach (RateOneOffContainers container in Quote.CurrentOneOffQuote.Containers)
			{
				ForwardingContainer bookingContainer = QuotedBookingContainers.AddNew();
				bookingContainer.JC_ContainerCount = container.TC_ContainerCount;
				bookingContainer.JC_RC = container.TC_RC;
			}
		}

		ZInt GetQuoteTotalLoosePackCount()
		{
			ZInt result = 0;
			foreach (var cont in Quote.CurrentOneOffQuote.LooseCargo)
			{
				result += cont.TPL_PackLineCount;
			}
			return result;
		}

		ZString GetQuotePackType()
		{
			ZString result = Quote.CurrentOneOffQuote.LooseCargo.Count > 0 ? Quote.CurrentOneOffQuote.LooseCargo[0].TPL_F3_NKPackType : (ZString)Constants.PkgUnit.Package;
			foreach (var cont in Quote.CurrentOneOffQuote.LooseCargo)
			{
				if (result != cont.TPL_F3_NKPackType)
				{
					result = Constants.PkgUnit.Package;
					break;
				}
			}
			return result;
		}

		ZInt GetQuoteContainerCount()
		{
			ZInt result = 0;

			if (Quote?.CurrentOneOffQuote == null)
			{
				return result;
			}
			foreach (RateOneOffContainers container in Quote.CurrentOneOffQuote.Containers)
			{
				result += container.TC_ContainerCount;
			}
			return result;
		}

		ZInt GetQuoteContainerCountByType(Func<RateOneOffContainers, bool> isValidContainer)
		{
			ZInt result = 0;

			if (Quote?.CurrentOneOffQuote == null)
			{
				return result;
			}
			foreach (RateOneOffContainers container in Quote.CurrentOneOffQuote.Containers)
			{
				if (container.Container != null && isValidContainer(container))
				{
					result += container.TC_ContainerCount;
				}
			}
			return result;
		}

		ZDecimal GetQuoteTEUCount()
		{
			return Quote.CurrentOneOffQuote.Containers.Cast<RateOneOffContainers>().Sum(cont => cont.Container != null ? (decimal)cont.Container.RC_TEU * cont.TC_ContainerCount : 0m);
		}

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();

			if (!IsCancelled && (Booking == null || !Booking.JS_IsForwardRegistered))
			{
				new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(this);
			}
		}

		protected override void OnFactorySaved(bool saveSucceeded)
		{
			base.OnFactorySaved(saveSucceeded);
			if (saveSucceeded)
			{
				OneOffQuoteStatistics?.ClearHasChanges();
			}
		}

		public bool IsCancelled
		{
			get { return this.Quote != null && this.Quote.IsCancelled || this.Booking != null && this.Booking.IsCancelled; }
		}

		public void CheckAndCopyBookingValuesToQuoteIfRequired()
		{
			if (ObjectState == QuotedBookingState.AcceptedBookingWithQuote && !IsForwardRegistered)
			{
				CopyBookingValuesToQuote();
			}
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public void CopyBookingValuesToQuote()
		{
			var oneOffQuote = Quote.CurrentOneOffQuote;

			if (HasDiscrepancyFor(DiscrepancyCheck.Client) && Job != null)
			{
				Quote.TH_OH = Job.LocalChargesPK;
			}

			if (HasDiscrepancyFor(DiscrepancyCheck.TransportMode) || HasDiscrepancyFor(DiscrepancyCheck.ContainerMode))
			{
				oneOffQuote.TT_TransportMode = Booking.JS_TransportMode;
				oneOffQuote.TT_ContainerMode = Booking.JS_PackingMode;
			}

			if (HasDiscrepancyFor(DiscrepancyCheck.Pickup))
			{
				Quote.CurrentOneOffQuote.PickUpDocAddress.SynchroniseWithParent(Booking.ConsignorDocumentaryAddress);
			}

			if (HasDiscrepancyFor(DiscrepancyCheck.Delivery))
			{
				Quote.CurrentOneOffQuote.DeliveryDocAddress.SynchroniseWithParent(Booking.ConsigneeDocumentaryAddress);
			}

			if (HasDiscrepancyFor(DiscrepancyCheck.Service))
			{
				oneOffQuote.TT_RS_NKServiceLevel = Booking.JS_RS_NKServiceLevel;
			}

			if (HasDiscrepancyFor(DiscrepancyCheck.Origin))
			{
				oneOffQuote.TT_RL_NKReceivalLocation = Booking.JS_RL_NKOrigin;
			}

			if (HasDiscrepancyFor(DiscrepancyCheck.Destination))
			{
				oneOffQuote.TT_RL_NKDeliveryLocation = Booking.JS_RL_NKDestination;
			}

			if (HasDiscrepancyFor(DiscrepancyCheck.GoodsValue))
			{
				oneOffQuote.TT_ValueOfGoods = Booking.JS_GoodsValue;
			}

			if (HasDiscrepancyFor(DiscrepancyCheck.GoodsCurrency))
			{
				oneOffQuote.TT_RX_NKGoodsCurrency = Booking.JS_RX_NKGoodsValueCurr;
			}

			if (HasDiscrepancyFor(DiscrepancyCheck.InsuranceValue))
			{
				oneOffQuote.TT_InsureVal = Booking.JS_InsuranceValue;
			}

			if (HasDiscrepancyFor(DiscrepancyCheck.InsuranceCurrency))
			{
				oneOffQuote.TT_RX_NKInsureValCurr = Booking.JS_RX_NKInsuranceCurrency;
			}

			if (HasDiscrepancyFor(DiscrepancyCheck.Inco))
			{
				oneOffQuote.TT_IncoTerm = Booking.JS_INCO;
			}

			if (HasDiscrepancyFor(DiscrepancyCheck.Carrier))
			{
				oneOffQuote.TT_OH_Carrier = Booking.BookedShippingLinePK;
			}

			if (HasDiscrepancyFor(DiscrepancyCheck.CarrierServiceLevel))
			{
				oneOffQuote.TT_PL_NKCarrierServiceLevel = Booking.JS_PL_NKCarrierServiceLevel;
			}

			oneOffQuote.TT_PickupEquipment = Booking.DocsAndCartage.JP_FCLPickupEquipmentNeeded;
			oneOffQuote.TT_DeliveryEquipment = Booking.DocsAndCartage.JP_FCLDeliveryEquipmentNeeded;

			DeleteDismatchedAddAbsent(oneOffQuote.LooseCargo, Booking.OuterPackLines.Cast<ForwardingPackLine>());

			DeleteDismatchedAddAbsent(oneOffQuote.Containers, QuotedBookingContainers.Cast<ForwardingContainer>());

			if (HasDiscrepancyFor(DiscrepancyCheck.Weight))
			{
				oneOffQuote.TT_ActualWeight = Booking.JS_ActualWeight;
			}

			if (HasDiscrepancyFor(DiscrepancyCheck.WeightUnit))
			{
				oneOffQuote.TT_UnitOfWeight = Booking.JS_UnitOfWeight;
			}

			if (HasDiscrepancyFor(DiscrepancyCheck.Volume))
			{
				oneOffQuote.TT_ActualVolume = Booking.JS_ActualVolume;
			}

			if (HasDiscrepancyFor(DiscrepancyCheck.VolumeUnit))
			{
				oneOffQuote.TT_UnitOfVolume = Booking.JS_UnitOfVolume;
			}

			if (HasDiscrepancyFor(DiscrepancyCheck.Chargeable))
			{
				oneOffQuote.TT_Chargeable = Booking.JS_ActualChargeable;
			}
		}

		void DeleteDismatchedAddAbsent(RateOneOffContainersCollection oneOffContainers, IEnumerable<ForwardingContainer> source)
		{
			var matchesForwardingContainers = new List<ForwardingContainer>();

			for (var i = oneOffContainers.Count - 1; i >= 0; i--)
			{
				var match = source.FirstOrDefault(x => !matchesForwardingContainers.Contains(x) && Equals(oneOffContainers[i], x));

				if (match != null)
				{
					matchesForwardingContainers.Add(match);
				}
				else
				{
					oneOffContainers[i].Delete();
				}
			}

			foreach (var elementToAdd in source.Where(x => !matchesForwardingContainers.Contains(x)))
			{
				Add(Quote.CurrentOneOffQuote, elementToAdd);
			}
		}

		void DeleteDismatchedAddAbsent(RateOneOffPackLineCollection oneOffPackLines, IEnumerable<ForwardingPackLine> source)
		{
			var matchesForwardingContainers = new List<ForwardingPackLine>();

			for (var i = oneOffPackLines.Count - 1; i >= 0; i--)
			{
				var match = source.FirstOrDefault(x => !matchesForwardingContainers.Contains(x) && Equals(oneOffPackLines[i], x));

				if (match != null)
				{
					matchesForwardingContainers.Add(match);
				}
				else
				{
					oneOffPackLines[i].Delete();
				}
			}

			foreach (var elementToAdd in source.Where(x => !matchesForwardingContainers.Contains(x)))
			{
				Add(Quote.CurrentOneOffQuote, elementToAdd);
			}
		}

		void Add(RateOneOffShipment quoteOneOff, ForwardingContainer container)
		{
			RateOneOffContainers rateContainer = quoteOneOff.Containers.AddNew();
			rateContainer.TC_ContainerCount = container.JC_ContainerCount;
			rateContainer.TC_RC = container.JC_RC;
		}

		void Add(RateOneOffShipment quoteOneOff, ForwardingPackLine packLine)
		{
			var ratePackLine = quoteOneOff.LooseCargo.AddNew();

			ZShort count;
			if (ZShort.TryParse(packLine.JL_PackageCount.ToString(), out count))
			{
				ratePackLine.TPL_PackLineCount = count;
			}

			ratePackLine.TPL_F3_NKPackType = packLine.JL_F3_NKPackType;
			ratePackLine.TPL_DimensionUQ = packLine.JL_UnitOfDimension;
			ratePackLine.TPL_VolumeUQ = packLine.PackLineVolumeUnit;
			ratePackLine.TPL_WeightUQ = packLine.PackLineWeightUnit;

			ratePackLine.TPL_Height = packLine.JL_Height;
			ratePackLine.TPL_Width = packLine.JL_Width;
			ratePackLine.TPL_Length = packLine.JL_Length;
			ratePackLine.TPL_Weight = packLine.JL_ActualWeight;
			ratePackLine.TPL_Volume = packLine.JL_ActualVolume;
			ratePackLine.TPL_VehicleMake = packLine.JL_VehicleMake;
			ratePackLine.TPL_VehicleModel = packLine.JL_VehicleModel;
			ratePackLine.TPL_VehicleYear = packLine.JL_VehicleYear;
			ratePackLine.TPL_VehicleColor = packLine.JL_VehicleColor;
			ratePackLine.TPL_VehicleNumberOfDoors = packLine.JL_VehicleNumberOfDoors;
			ratePackLine.TPL_VehicleTransmission = packLine.JL_VehicleTransmission;
			ratePackLine.TPL_RefVehicleIdentificationNumber = packLine.JL_RefNumber;
		}

		bool Equals(RateOneOffPackLine ratePackLine, ForwardingPackLine packLine)
		{
			bool result = ratePackLine.TPL_F3_NKPackType == packLine.JL_F3_NKPackType;
			result = result && ratePackLine.TPL_DimensionUQ == packLine.JL_UnitOfDimension;
			result = result && ratePackLine.TPL_VolumeUQ == packLine.PackLineVolumeUnit;
			result = result && ratePackLine.TPL_WeightUQ == packLine.PackLineWeightUnit;

			result = result && ratePackLine.TPL_Height == packLine.JL_Height;
			result = result && ratePackLine.TPL_Width == packLine.JL_Width;
			result = result && ratePackLine.TPL_Length == packLine.JL_Length;
			result = result && ratePackLine.TPL_Weight == packLine.JL_ActualWeight;
			result = result && ratePackLine.TPL_Volume == packLine.JL_ActualVolume;
			result = result && ratePackLine.TPL_RC_RefContainer == packLine.JL_RC_ContainerType;

			result = result && ratePackLine.TPL_VehicleMake == packLine.JL_VehicleMake;
			result = result && ratePackLine.TPL_VehicleModel == packLine.JL_VehicleModel;
			result = result && ratePackLine.TPL_VehicleYear == packLine.JL_VehicleYear;
			result = result && ratePackLine.TPL_VehicleColor == packLine.JL_VehicleColor;
			result = result && ratePackLine.TPL_VehicleNumberOfDoors == packLine.JL_VehicleNumberOfDoors;
			result = result && ratePackLine.TPL_VehicleTransmission == packLine.JL_VehicleTransmission;
			result = result && ratePackLine.TPL_RefVehicleIdentificationNumber == packLine.JL_RefNumber;

			ZShort count;
			if (ZShort.TryParse(packLine.JL_PackageCount.ToString(), out count))
			{
				result = result && ratePackLine.TPL_PackLineCount == count;
			}

			return result;
		}

		bool Equals(RateOneOffContainers rateContainer, ForwardingContainer container)
		{
			return rateContainer.TC_ContainerCount == container.JC_ContainerCount && rateContainer.TC_RC == container.JC_RC;
		}

		public void CheckJobDefaults()
		{
			if (ObjectState == QuotedBookingState.QuoteOnly || ObjectState == QuotedBookingState.AcceptedBookingWithQuote)
			{
				JobHeader quoteHeader = new JobHeader.Loader(this).Load();
				if (quoteHeader != null)
				{
					quoteHeader.SetDefaultsForJob();
				}
			}

			if (ObjectState == QuotedBookingState.UnacceptedBookingWithQuote || ObjectState == QuotedBookingState.AcceptedBookingWithQuote || ObjectState == QuotedBookingState.BookingOnly)
			{
				JobHeader bookingHeader = new JobHeader.Loader(Booking).Load();
				if (bookingHeader != null)
				{
					bookingHeader.SetDefaultsForJob();
				}
			}
		}

		#endregion

		#region Direction

		public MasterFiles.Business.Directions JobDirection
		{
			get { return ImportExportHelper.GetJobDirection(Origin, Destination); }
		}

		ZString Direction
		{
			get
			{
				switch (((IImportExport)this).JobDirection)
				{
					case Enterprise.MasterFiles.Business.Directions.Import:
						return DirectionsContext.Import;

					case Enterprise.MasterFiles.Business.Directions.Export:
						return DirectionsContext.Export;

					case Enterprise.MasterFiles.Business.Directions.Domestic:
						return DirectionsContext.Domestic;

					default:
						return string.Empty;
				}
			}
		}

		#endregion

		#region IJobInvoicingPlugIn Members

		IJobInvoicingSupporter invoicingSupporter;
		public IJobInvoicingSupporter InvoicingSupporter
		{
			get { return invoicingSupporter ?? (invoicingSupporter = GetNewInvoicingSupporter()); }
		}

		JobInvoicingSupporter GetNewInvoicingSupporter()
		{
			if (GetFromBooking)
			{
				return new BookingInvoicingSupporter(this);
			}

			return new QuoteInvoicingSupporter(this);
		}

		#endregion

		#region IJobHeaderParent Members

		BusinessObjectFactory IJobHeaderParentCore.Factory
		{
			get { return Factory; }
		}

		void IJobHeaderParent.OnJobCreating(JobHeader job)
		{
			if (JobHeaderParent != null)
			{
				JobHeaderParent.OnJobCreating(job);
			}

			if (JobCreating != null)
			{
				JobCreating(this, EventArgs.Empty);
			}
		}

		void IJobHeaderParent.OnJobCreated(JobHeader job)
		{
			if (JobHeaderParent != null)
			{
				JobHeaderParent.OnJobCreated(job);
			}

			if (JobCreated != null)
			{
				JobCreated(this, EventArgs.Empty);
			}
		}

		void IJobHeaderParent.OnJobDeleting(JobHeader job)
		{
			if (JobDeleting != null)
			{
				JobDeleting(this, EventArgs.Empty);
			}
		}

		void IJobHeaderParent.OnJobDeleted(JobHeader job)
		{
		}

		void OnCrossTradeChange()
		{
			if (CrossTradeChanged != null)
			{
				CrossTradeChanged(this, EventArgs.Empty);
			}
		}

		public event EventHandler JobCreated;
		public event EventHandler JobCreating;
		public event EventHandler JobDeleting;
		public event EventHandler CrossTradeChanged;

		void IJobHeaderParent.SetJobNumberFieldOnSaving()
		{
			if (JobHeaderParent != null)
			{
				JobHeaderParent.SetJobNumberFieldOnSaving();
			}
		}

		string IJobHeaderParentCore.TableName
		{
			get { return JobHeaderParent != null ? JobHeaderParent.TableName : string.Empty; }
		}

		string IJobNumber.JobNumber
		{
			get { return JobHeaderParent != null ? JobHeaderParent.JobNumber : string.Empty; }
		}

		internal bool UseJobFromBooking
		{
			get { return ObjectState == QuotedBookingState.BookingOnly; }
		}

		internal IJobHeaderParent JobHeaderParent
		{
			get { return UseJobFromBooking ? Booking : Quote; }
		}

		ZGuid IJobHeaderParentCore.PK
		{
			get { return JobHeaderParent != null ? JobHeaderParent.PK : ZGuid.Empty; }
		}

		bool IJobHeaderParent.AllowInvoiceDeletion
		{
			get { return true; }
		}

		#endregion

		#region IScreeningPartyProvider

		ZString IScreeningPartyProvider.GetWorstScreeningStatus()
		{
			return Booking?.JS_BookedVesselScreeningStatus ?? ZString.Empty;
		}

		ZString IScreeningPartyProvider.GetWorstScreeningStatusUnlessManuallyCleared()
		{
			return (this as IScreeningPartyProvider).GetWorstScreeningStatus();
		}

		ScreeningParty[] IScreeningPartyProvider.ScreeningParties
		{
			get { return new ScreeningParty[] { new ScreeningParty(this, Res.GetString("94F356D0-981D-4147-AA19-F48E2AEDE68B", "Vessel"), this) }; }
		}

		ZString IScreeningStatusProvider.ScreeningStatus
		{
			get { return Booking?.JS_BookedVesselScreeningStatus ?? ZString.Empty; }
			set
			{
				if (Booking != null)
				{
					Booking.JS_BookedVesselScreeningStatus = value;
				}
			}
		}

		#endregion

		#region Compliance Risk

		public ComplianceRiskStatusObject ComplianceRiskStatus => ObjectFactory.Get<IComplianceRiskStatusSupporter>().GetStatus(this);

		ZBool IComplianceJobDirectionProvider.IsInternational => (this.IsCrossTrade() || this.IsExport() || this.IsImport());

		#endregion

		public QuotationDocumentMode DocumentPrintMode => IsOneOffQuote
			? Quote.DocumentPrintMode
			: QuotationDocumentMode.Unknown;

		public bool HideClientReplyLink => !IsOneOffQuote
			|| Quote.HideClientReplyLink;

		public bool IsLocalClientRole => !IsOneOffQuote
			|| Quote.IsLocalClientRole;

		public string QuotationAcceptText => Quote.QuotationAcceptText;

		public string QuotationAcceptTooltip => Quote.QuotationAcceptTooltip;

		internal OrgHeader ExportBroker
		{
			get { return (Booking != null ? Booking.ExportBroker : null) ?? ConsignorRelatedExportBroker; }
		}

		OrgHeader ConsignorRelatedExportBroker
		{
			get
			{
				if (Consignor != null && Consignor.OH_IsConsignor)
				{
					return Consignor.GetRelatedParty(RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Pickup, TransportMode, ContainerMode);
				}

				return null;
			}
		}

		internal OrgHeader ImportBroker
		{
			get { return (Booking != null ? Booking.ImportBroker : null) ?? ConsigneeRelatedImportBroker; }
		}

		internal OrgHeader ConsigneeRelatedImportBroker
		{
			get
			{
				if (Consignee != null && Consignee.OH_IsConsignee)
				{
					return Consignee.GetRelatedParty(RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Delivery, TransportMode, ContainerMode);
				}

				return null;
			}
		}

		JobSailing Sailing
		{
			get
			{
				if (Booking == null)
				{
					return null;
				}

				if (sailing == null || sailing.PK != Booking.JS_JX)
				{
					sailing = Factory.Load<JobSailing>(Booking.JS_JX);
				}

				return sailing;
			}
		}

		JobSailing sailing;

		#region IEDocsPluginHostDecider Members

		IBusiness IEDocsPluginHostDecider.HostBusinessEntity
		{
			get { return (GetFromBooking) ? Booking : Quote; }
		}

		#endregion

		#region ISailingChooserParent Members

		ZDateTime ISailingChooserParent.BookedDate
		{
			get { return Booking.JS_A_BKD; }
		}

		event EventHandler ISailingChooserParent.BookedDateChanged
		{
			add { Booking.JS_A_BKDInfo.ValueChanged += value; }
			remove { Booking.JS_A_BKDInfo.ValueChanged -= value; }
		}

		ZDateTime ISailingChooserParent.ETD
		{
			get { return Booking.JS_E_DEP; }
		}

		ZDateTime ISailingChooserParent.RequestedByDate
		{
			get { return Booking.JS_ClientRequestedETA; }
		}

		event EventHandler ISailingChooserParent.RequestedByDateChanged
		{
			add { Booking.JS_ClientRequestedETAInfo.ValueChanged += value; }
			remove { Booking.JS_ClientRequestedETAInfo.ValueChanged -= value; }
		}

		ZString ISailingChooserParent.AWBServiceLevel
		{
			get { return Booking.JS_AWBServiceLevel; }
			set { Booking.JS_AWBServiceLevel = value; }
		}

		event EventHandler ISailingChooserParent.AWBServiceLevelChanged
		{
			add { Booking.JS_AWBServiceLevelInfo.ValueChanged += value; }
			remove { Booking.JS_AWBServiceLevelInfo.ValueChanged -= value; }
		}

		ZBool ISailingChooserParent.IsDirect
		{
			get { return Booking.JS_IsDirectBooking; }
			set { Booking.JS_IsDirectBooking = value; }
		}

		ZBool ISailingChooserParent.IsDirectEnabled
		{
			get { return Booking.IsAir || Booking.IsSea; }
		}

		event EventHandler ISailingChooserParent.IsDirectChanged
		{
			add { Booking.JS_IsDirectBookingInfo.ValueChanged += value; }
			remove { Booking.JS_IsDirectBookingInfo.ValueChanged -= value; }
		}

		ZBool ISailingChooserParent.IsNeutralMaster
		{
			get { return Booking.JS_IsNeutralMaster; }
			set { Booking.JS_IsNeutralMaster = value; }
		}

		event EventHandler ISailingChooserParent.IsNeutralMasterChanged
		{
			add { Booking.JS_IsNeutralMasterInfo.ValueChanged += value; }
			remove { Booking.JS_IsNeutralMasterInfo.ValueChanged -= value; }
		}

		ZString ISailingChooserParent.MawbNumber
		{
			get { return Booking.JS_HouseBill; }
			set { Booking.JS_HouseBill = value; }
		}

		event EventHandler ISailingChooserParent.MawbNumberChanged
		{
			add { Booking.JS_HouseBillInfo.ValueChanged += value; }
			remove { Booking.JS_HouseBillInfo.ValueChanged -= value; }
		}

		ZGuid ISailingChooserParent.Carrier
		{
			get { return Booking.BookedShippingLinePK; }
			set { Booking.BookedShippingLinePK = value; }
		}

		event EventHandler ISailingChooserParent.CarrierChanged
		{
			add { Booking.JS_OA_BookedShippingLineAddressInfo.ValueChanged += value; }
			remove { Booking.JS_OA_BookedShippingLineAddressInfo.ValueChanged -= value; }
		}

		ZString ISailingChooserParent.TransportMode
		{
			get { return Booking.JS_TransportMode; }
		}

		event EventHandler ISailingChooserParent.TransportModeChanged
		{
			add { Booking.JS_TransportModeInfo.ValueChanged += value; }
			remove { Booking.JS_TransportModeInfo.ValueChanged -= value; }
		}

		ZString ISailingChooserParent.ContainerMode
		{
			get { return Booking.JS_PackingMode; }
		}

		event EventHandler ISailingChooserParent.ContainerModeChanged
		{
			add { Booking.JS_PackingModeInfo.ValueChanged += value; }
			remove { Booking.JS_PackingModeInfo.ValueChanged -= value; }
		}

		ZString ISailingChooserParent.ReservedMasterBill
		{
			get { return Booking.JS_CFSReference; }
			set { Booking.JS_CFSReference = value; }
		}

		ZString ISailingChooserParent.LoadPort
		{
			get { return Booking.JS_RL_NKLoadPort; }
		}

		event EventHandler ISailingChooserParent.LoadPortChanged
		{
			add { Booking.JS_RL_NKLoadPortInfo.ValueChanged += value; }
			remove { Booking.JS_RL_NKLoadPortInfo.ValueChanged -= value; }
		}

		ZString ISailingChooserParent.DischargePort
		{
			get { return Booking.JS_RL_NKDischargePort; }
		}

		event EventHandler ISailingChooserParent.DischargePortChanged
		{
			add { Booking.JS_RL_NKDischargePortInfo.ValueChanged += value; }
			remove { Booking.JS_RL_NKDischargePortInfo.ValueChanged -= value; }
		}

		ZGuid ISailingChooserParent.SailingJX
		{
			get
			{
				ZGuid result = Booking.JS_JX;

				if (result.IsEmpty &&
					Booking.DepartureConsol != null &&
					Booking.DepartureConsol.Schedule != null)
				{
					result = Booking.DepartureConsol.Schedule.PK;
				}

				return result;
			}
			set
			{
				Booking.JS_JX = value;
				if (!Booking.IsValidationSuspended)
				{
					Booking.Validation.ValidateJS_OA_BookedShippingLineAddress();
					OH_CarrierInfo.RefreshBinding();
				}

				DefaultLoadAndDischargeFromSailingIfEmpty();
				DefaultETDAndETAFromSailingIfEmpty();
				TotalCO2eForBindingInfo.RefreshBinding();
				TotalCO2eForSortingInfo.RefreshBinding();
			}
		}

		event EventHandler ISailingChooserParent.SailingJXChanged
		{
			add { Booking.JS_JXInfo.ValueChanged += value; }
			remove { Booking.JS_JXInfo.ValueChanged -= value; }
		}

		void ISailingChooserParent.SetIsNeutralMasterReadOnly(bool value)
		{
			if (Booking != null)
			{
				Booking.JS_IsNeutralMaster_ReadOnly = value;
			}
		}

		#endregion

		#region Default Load and Discharge From Sailing

		void DefaultLoadAndDischargeFromSailingIfEmpty()
		{
			if (LoadPort.IsEmpty &&
				DischargePort.IsEmpty &&
				Booking != null &&
				Sailing != null)
			{
				LoadPort = Sailing.JX_JA_RL_NKPortOfLoading;
				DischargePort = Sailing.JX_JB_RL_NKPortOfDischarge;
			}
		}

		#endregion

		#region Default ETD and ETA From Sailing

		void DefaultETDAndETAFromSailingIfEmpty()
		{
			if (ETD.IsEmpty &&
				ETA.IsEmpty &&
				Booking != null &&
				Sailing != null)
			{
				ETD = Sailing.JX_JA_E_DEP;
				ETA = Sailing.JX_JB_E_ARV;
			}
		}

		#endregion

		#region IEDocsProvider Members

		EDocsProviderSupporter IEDocsProvider.GetEDocsProviderSupporter()
		{
			return GetFromBooking ? ((IEDocsProvider)Booking).GetEDocsProviderSupporter() : ((IEDocsProvider)Quote).GetEDocsProviderSupporter();
		}

		#endregion

		#region IDocumentSupportable Members

		public virtual DocumentSupporter DocumentSupporter
		{
			get { return GetFromBooking ? GetBookingDocumentSupporter() : ((IDocumentSupportable)Quote).DocumentSupporter; }
		}

		protected virtual DocumentSupporter GetBookingDocumentSupporter()
		{
			return new QuotedBookingDocumentSupporter(this);
		}

		#endregion

		#region IDocumentSupportableOverrideType Members

		public Type DocumentSupportableType => GetFromBooking
			? Booking.GetType()
			: Quote.GetType();

		#endregion

		#region IDocManagerSupport Members

		DocManagerInfo IDocManagerSupport.DocManagerInfo
		{
			get { return GetFromBooking ? QuotedBookingDocManagerInfo : ((IDocManagerSupport)Quote).DocManagerInfo; }
		}

		DocManagerInfo QuotedBookingDocManagerInfo
		{
			get { return quotedBookingDocManagerInfo ?? (quotedBookingDocManagerInfo = new QuotedBookingDocManagerInfo(this)); }
		}
		DocManagerInfo quotedBookingDocManagerInfo;

		#endregion

		#region IParentDocManagerSupport Members

		ZGuid IParentDocManagerSupport.ParentGuid => GetFromBooking ? Booking.PK : PK;

		ZString IParentDocManagerSupport.ParentTableName => GetFromBooking ? JobShipmentSchema.Constants.TableName : ViewQuotedBookingSchema.Constants.TableName;

		#endregion

		#region IDocumentDeliveredLogSupporter members

		ZGuid IDocumentDeliveredLogSupporter.Identifier => GetFromBooking ? Booking.PK : PK;
		Type IDocumentDeliveredLogSupporter.BusinessObjectTypeToLogAgainst => GetFromBooking ? Booking.GetType() : typeof(ViewQuotedBooking);

		#endregion

		#region IQuotedBooking Members

		BusinessObject IQuotedBooking.ForwardingShipment
		{
			get { return Booking; }
		}

		ZString IQuotedBooking.PackingMode
		{
			get { return Booking != null ? Booking.JS_PackingMode : ZString.Empty; }
			set
			{
				if (Booking != null)
				{
					Booking.JS_PackingMode = value;
				}
			}
		}

		IDependentBusinessObjectCollection IQuotedBooking.QuotedBookingContainers
		{
			get { return this.QuotedBookingContainers; }
		}

		ZGuid IQuotedBooking.SailingJX
		{
			get { return ((ISailingChooserParent)this).SailingJX; }
			set { ((ISailingChooserParent)this).SailingJX = value; }
		}

		ZGuid IQuotedBooking.CartagePK
		{
			get { return Booking != null ? Booking.DocsAndCartage.PK : ZGuid.Empty; }
		}

		ZString IQuotedBooking.TransportMode
		{
			get { return Booking == null ? ZString.Empty : Booking.JS_TransportMode; }
			set
			{
				if (Booking != null)
				{
					Booking.JS_TransportMode = value;
				}
			}
		}

		ZString IQuotedBooking.UniqueConsignRef
		{
			get { return Booking == null ? ZString.Empty : Booking.JS_UniqueConsignRef; }
			set
			{
				if (Booking != null)
				{
					Booking.JS_UniqueConsignRef = value;
				}
			}
		}

		BusinessObject IQuotedBooking.JobSailing
		{
			get { return ScheduleChooser.Sailing; }
		}

		BusinessObject IQuotedBooking.Quote
		{
			get { return Quote; }
		}

		IJobHeader IQuotedBooking.Job
		{
			get { return Job; }
		}

		IOrgHeader IQuotedBooking.ControllingCustomer => ControllingCustomer;

		IOrgHeader IQuotedBooking.Client => Client;

		IOrgHeader IQuotedBooking.Consignor => Consignor;

		IOrgHeader IQuotedBooking.Consignee => Consignee;

		ZString IQuotedBooking.Via => Via;

		ZString IQuotedBooking.Origin => Origin;

		ZString IQuotedBooking.Destination => Destination;

		#endregion

		#region ICancellable Members

		string ICancellable.CanCancel()
		{
			string result = "";
			switch (ObjectState)
			{
				case QuotedBookingState.BookingOnly:
					result = Booking.CanCancel();
					break;

				case QuotedBookingState.AcceptedBookingWithQuote:
				case QuotedBookingState.UnacceptedBookingWithQuote:
					result = Booking.CanCancel() + JobHeaderParentDeletionHelper.CheckIfCanCancelJobHeaderParent(Quote.PK, Quote.HumanReadableName);
					break;

				case QuotedBookingState.QuoteOnly:
					result = JobHeaderParentDeletionHelper.CheckIfCanCancelJobHeaderParent(Quote.PK, Quote.HumanReadableName);
					break;
			}

			if (string.IsNullOrEmpty(result)
				&& Quote != null
				&& ObjectState == QuotedBookingState.QuoteOnly
				&& Quote.QuoteStatus == Quote.QuoteStatusOptions.ClientAccepted)
			{
				result = Res.GetString("5607D9E4-24D4-49FE-893A-4289026367A5", "The selected one off quote has already been accepted by the client and cannot be deactivated.");
			}

			return result;
		}

		string ICancellable.CanReactivate()
		{
			string result = "";
			switch (ObjectState)
			{
				case QuotedBookingState.BookingOnly:
				case QuotedBookingState.AcceptedBookingWithQuote:
				case QuotedBookingState.UnacceptedBookingWithQuote:
					result = CanReactivateBooking();
					break;

				case QuotedBookingState.QuoteOnly:
					result = null;
					break;
			}
			return result;
		}

		string CanReactivateBooking()
		{
			var anyEBKLogs = Booking.Logs.Find(IsEBKLog).Any();
			return ShipmentStatus == ShipmentStatusList.Codes.ElectronicBooking || anyEBKLogs
				? Res.GetString("66B2ADC9-5486-4021-BAE4-9E52D76BE574", "A booking created via a Booking Request EDI Message cannot be re-activated.")
				: Booking.CanReactivate();
		}

		bool IsEBKLog(StmALog log)
		{
			return log.SL_SE_NKEvent == Events.StatusUpdatedCode
				&& log.Parameters.TryGetValue(Params.New, out var status)
				&& status == ShipmentStatusList.Codes.ElectronicBooking;
		}

		bool ICancellable.IsCancelled
		{
			get
			{
				ZBool result = ZBool.False;
				switch (ObjectState)
				{
					case QuotedBookingState.BookingOnly:
						result = ((ICancellable)Booking).IsCancelled;
						break;

					case QuotedBookingState.AcceptedBookingWithQuote:
					case QuotedBookingState.UnacceptedBookingWithQuote:
						result = ((ICancellable)Booking).IsCancelled && Quote.TH_IsCancelled;
						break;

					case QuotedBookingState.QuoteOnly:
						result = Quote.TH_IsCancelled;
						break;
				}
				return result;
			}
			set
			{
				switch (ObjectState)
				{
					case QuotedBookingState.BookingOnly:
						SetBookingIsCancelled(value);
						break;

					case QuotedBookingState.AcceptedBookingWithQuote:
					case QuotedBookingState.UnacceptedBookingWithQuote:
						SetBookingIsCancelled(value);
						SetQuoteIsCancelled(value);
						break;

					case QuotedBookingState.QuoteOnly:
						SetQuoteIsCancelled(value);
						break;
				}
			}
		}

		void SetBookingIsCancelled(bool value)
		{
			if (!value)
			{
				Booking.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
			}

			((ICancellable)Booking).IsCancelled = value;
		}

		void SetQuoteIsCancelled(bool value)
		{
			if (value)
			{
				Quote.CancelQuote();
			}
			else
			{
				Quote.Reactivate();
			}
		}

		bool ICancellable.IsCancelledHasChanged
		{
			get
			{
				ZBool result = ZBool.False;
				switch (ObjectState)
				{
					case QuotedBookingState.BookingOnly:
						result = ((ICancellable)Booking).IsCancelledHasChanged;
						break;

					case QuotedBookingState.AcceptedBookingWithQuote:
					case QuotedBookingState.UnacceptedBookingWithQuote:
						result = ((ICancellable)Booking).IsCancelledHasChanged || Quote.TH_IsCancelledInfo.HasChanges;
						break;

					case QuotedBookingState.QuoteOnly:
						result = Quote.TH_IsCancelledInfo.HasChanges;
						break;
				}
				return result;
			}
		}

		#endregion

		#region IMAWBParent Members

		ZGuid IMAWBParent.PK
		{
			get { return Booking != null ? Booking.PK : ZGuid.Empty; }
		}

		IMAWBAllocationParent IMAWBParent.MAWBAllocationParent
		{
			get { return Booking != null ? ScheduleChooser : null; }
		}

		#endregion

		#region ITemplateCopyable Members

		ZGuid ClonedQuotePK
		{
			get
			{
				var clonedQuotePK = ZGuid.Empty;
				var quote = Quote;

				if (quote != null)
				{
					bool oldSameClientCopy = quote.SameClientCopy;
					quote.SameClientCopy = true;
					try
					{
						var clonedQuote = (Quote)((ITemplateCopyable)quote).TemplateCopy();
						clonedQuotePK = clonedQuote.PK;
					}
					finally
					{
						quote.SameClientCopy = oldSameClientCopy;
					}
				}

				return clonedQuotePK;
			}
		}

		ZGuid ClonedBookingPK
		{
			get
			{
				var clonedBookingPK = ZGuid.Empty;
				var booking = Booking;

				if (booking != null)
				{
					using (booking.UseBookingOnlyCloningMode())
					{
						var clonedBooking = (ForwardingShipment)((ITemplateCopyable)booking).TemplateCopy();
						clonedBooking.JS_IsForwardRegistered = false;
						clonedBooking.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
						clonedBookingPK = clonedBooking.PK;
					}
				}

				return clonedBookingPK;
			}
		}

		IBusiness ITemplateCopyable.TemplateCopy()
		{
			var clonedQuotePK = ClonedQuotePK;
			var clonedBookingPK = ClonedBookingPK;
			var copiedBooking = New(clonedQuotePK, clonedBookingPK, Factory, scheduleChooserCreator);
			// This is consistent with the behavior as creating a new Booking with Quote
			if (!clonedQuotePK.IsDefault && !clonedBookingPK.IsEmpty)
			{
				copiedBooking.Quote.TH_Accepted = ZDateTime.Now;
			}

			this.CopyJobCO2eTo(copiedBooking);
			return copiedBooking;
		}

		public IBusiness TemplateCopyOnlyQuote()
		{
			var copiedBooking = New(ClonedQuotePK, ZGuid.Empty, Factory);
			this.CopyJobCO2eTo(copiedBooking);
			return copiedBooking;
		}

		public bool CanCopyAsOneOffQuoteAmendment =>
			IsOneOffQuote &&
			!IsConsolidated &&
			!Quote.TH_IsOneOffQuoteConsumed &&
			ObjectState == QuotedBookingState.QuoteOnly;

		#endregion

		#region ISupportDataImporting Members

		public bool IsImportingData { get; set; }

		#endregion

		#region IBuyerSupplierRelationshipConsumer Members

		event EventHandler IBuyerSupplierRelationshipConsumer.ModesChanged
		{
			add
			{
				if (Booking != null)
				{
					Booking.JS_TransportModeInfo.ValueChanged += value;
					Booking.JS_PackingModeInfo.ValueChanged += value;
				}

				if (Quote != null)
				{
					Quote.CurrentOneOffQuote.TT_TransportModeInfo.ValueChanged += value;
					Quote.CurrentOneOffQuote.TT_ContainerModeInfo.ValueChanged += value;
				}
			}
			remove
			{
				if (Booking != null)
				{
					Booking.JS_TransportModeInfo.ValueChanged -= value;
					Booking.JS_PackingModeInfo.ValueChanged -= value;
				}

				if (Quote != null)
				{
					Quote.CurrentOneOffQuote.TT_TransportModeInfo.ValueChanged -= value;
					Quote.CurrentOneOffQuote.TT_ContainerModeInfo.ValueChanged -= value;
				}
			}
		}

		ZBool IBuyerSupplierRelationshipConsumer.ShouldRestorePickupDeliveryAndNotifyPartyAddress
		{
			get { return true; }
		}

		OrgHeader IBuyerSupplierRelationshipConsumer.Consignee
		{
			get { return Consignee; }
			set
			{
				if (!ConsigneeDocumentaryAddress.ReadOnly)
				{
					ConsigneeDocumentaryAddress.OrganisationPK = value.PK;
				}
			}
		}

		OrgHeader IBuyerSupplierRelationshipConsumer.Consignor
		{
			get { return Consignor; }
			set
			{
				if (!ConsignorDocumentaryAddress.ReadOnly)
				{
					ConsignorDocumentaryAddress.OrganisationPK = value.PK;
				}
			}
		}

		event EventHandler IBuyerSupplierRelationshipConsumer.ConsigneeChanged
		{
			add { ConsigneeDocumentaryAddress.E2_OA_AddressInfo.ValueChanged += value; }
			remove { ConsigneeDocumentaryAddress.E2_OA_AddressInfo.ValueChanged -= value; }
		}

		event EventHandler IBuyerSupplierRelationshipConsumer.ConsignorChanged
		{
			add { ConsignorDocumentaryAddress.E2_OA_AddressInfo.ValueChanged += value; }
			remove { ConsignorDocumentaryAddress.E2_OA_AddressInfo.ValueChanged -= value; }
		}

		JobDocAddress IBuyerSupplierRelationshipConsumer.ConsigneeDeliveryAddress
		{
			get { return (Booking != null) ? Booking.ConsigneeDeliveryAddress : null; }
		}

		JobDocAddress IBuyerSupplierRelationshipConsumer.ConsignorPickupAddress
		{
			get { return (Booking != null) ? Booking.ConsignorPickupAddress : null; }
		}

		ZGuid IBuyerSupplierRelationshipConsumer.PickupCartageCoPK
		{
			get { return (Booking != null && !Booking.IsDeleted) ? Booking.DocsAndCartage.PickupCartageCoPK : ZGuid.Empty; }
			set
			{
				if (Booking != null && !Booking.IsDeleted)
				{
					Booking.DocsAndCartage.PickupCartageCoPK = value;
				}
			}
		}

		ZGuid IBuyerSupplierRelationshipConsumer.DeliveryCartageCoPK
		{
			get { return (Booking != null && !Booking.IsDeleted) ? Booking.DocsAndCartage.DeliveryCartageCoPK : ZGuid.Empty; }
			set
			{
				if (Booking != null && !Booking.IsDeleted)
				{
					Booking.DocsAndCartage.DeliveryCartageCoPK = value;
				}
			}
		}

		ZGuid IBuyerSupplierRelationshipConsumer.ImportBrokerPK
		{
			get { return (Booking != null) ? Booking.JS_OH_ImportBroker : ZGuid.Empty; }
			set
			{
				if (Booking != null && (!Booking.IsChangingConsigneeAddress || !Booking.IsInDatabase) && (!Booking.IsChangingConsignorAddress || !Booking.IsInDatabase))
				{
					Booking.JS_OH_ImportBroker = value;
				}
			}
		}

		JobDocAddress IBuyerSupplierRelationshipConsumer.NotifyPartyDocumentaryAddress
		{
			get { return Booking?.NotifyPartyDocumentaryAddress; }
		}

		ZGuid IBuyerSupplierRelationshipConsumer.ShippingLinePK
		{
			get { return OH_Carrier; }
			set { OH_Carrier = value; }
		}

		ZByte IBuyerSupplierRelationshipConsumer.NoCopyBills
		{
			get { return (Booking != null) ? Booking.JS_NoCopyBills : ZByte.Zero; }
			set
			{
				if (Booking != null)
				{
					Booking.JS_NoCopyBills = value;
				}
			}
		}

		ZByte IBuyerSupplierRelationshipConsumer.NoOriginalBills
		{
			get { return (Booking != null) ? Booking.JS_NoOriginalBills : ZByte.Zero; }
			set
			{
				if (Booking != null)
				{
					Booking.JS_NoOriginalBills = value;
				}
			}
		}

		ZString IBuyerSupplierRelationshipConsumer.GoodsDescription
		{
			get { return (Booking != null) ? Booking.JS_GoodsDescription : ZString.Empty; }
			set
			{
				if (Booking != null)
				{
					Booking.JS_GoodsDescription = value;
				}
			}
		}

		ZString IBuyerSupplierRelationshipConsumer.ReleaseType
		{
			get { return (Booking != null) ? Booking.JS_ReleaseType : ZString.Empty; }
		}

		ZBool IBuyerSupplierRelationshipConsumer.ShouldPromptToSaveBuyerSupplierRelationship
		{
			get { return Env.Registry.PromptToSaveBuyerSupplier; }
		}

		ZGuid IBuyerSupplierRelationshipConsumer.SendingAgentPK
		{
			get { return ZGuid.Empty; }
			set { }
		}

		ZGuid IBuyerSupplierRelationshipConsumer.ReceivingAgentPK
		{
			get { return ZGuid.Empty; }
			set { }
		}

		ZBool IBuyerSupplierRelationshipConsumer.ShouldRestorePaymentTerm
		{
			get { return !IsDomesticFreight; }
		}

		ZBool IBuyerSupplierRelationshipConsumer.ShouldRestoreHandlingInformation
		{
			get { return true; }
		}

		ZString IBuyerSupplierRelationshipConsumer.LoadPort
		{
			get { return LoadPort; }
			set
			{
				if (ObjectState != QuotedBookingState.QuoteOnly)
				{
					LoadPort = value;
				}
			}
		}

		ZString IBuyerSupplierRelationshipConsumer.DischargePort
		{
			get { return DischargePort; }
			set
			{
				if (ObjectState != QuotedBookingState.QuoteOnly)
				{
					DischargePort = value;
				}
			}
		}

		void IBuyerSupplierRelationshipConsumer.RestoreServiceLevelFallback()
		{
			var bookingAsRelationshipConsumer = Booking as IBuyerSupplierRelationshipConsumer;
			if (bookingAsRelationshipConsumer != null && string.IsNullOrEmpty(Quote?.CurrentOneOffQuote?.TT_RS_NKServiceLevel))
			{
				bookingAsRelationshipConsumer.RestoreServiceLevelFallback();
			}
		}

		void IBuyerSupplierRelationshipConsumer.RestoreSendingAgentFallback()
		{
		}

		void IBuyerSupplierRelationshipConsumer.RestoreReceivingAgentFallback()
		{
		}

		void IBuyerSupplierRelationshipConsumer.RestoreGoodsCurrencyFallback()
		{
		}

		void IBuyerSupplierRelationshipConsumer.RestoreNumberOfBillsWithFallback()
		{
			var bookingAsRelationshipConsumer = Booking as IBuyerSupplierRelationshipConsumer;
			if (bookingAsRelationshipConsumer != null && !string.IsNullOrEmpty(Booking?.JS_ReleaseType))
			{
				bookingAsRelationshipConsumer.RestoreNumberOfBillsWithFallback();
			}
		}

		public BuyerSupplierLinksHelper<QuotedBooking> BuyerSupplierLinksHelper { get; private set; }

		void IBuyerSupplierRelationshipConsumer.RestoreEFreightStatusFallback(ZString defaultStatus)
		{
		}

		ZBool IBuyerSupplierRelationshipConsumer.PreventBuyerSupplierRelationships
		{
			get { return false; }
		}

		ZBool IBuyerSupplierRelationshipConsumer.ShouldDefaultContainerModeAndIsContainerised(ZString containerMode) => false;

		#endregion

		#region ISometimesWorkflowProvider Members

		public const string BookingWithQuoteCode = "BWQ";
		public const string QuickBookingCode = "QBN";
		/// <summary>
		/// SpotQuote is also known as One-Off Quote
		/// </summary>
		public const string SpotQuoteCode = "SQT";

		public ZString QuotedBookingTypeCode
		{
			get
			{
				switch (ObjectState)
				{
					case QuotedBookingState.AcceptedBookingWithQuote:
					case QuotedBookingState.UnacceptedBookingWithQuote:
						return BookingWithQuoteCode;
					case QuotedBookingState.BookingOnly:
						return QuickBookingCode;
					case QuotedBookingState.QuoteOnly:
						return SpotQuoteCode;
					default:
						return ZString.Empty;
				}
			}
		}

		public ZPropertyInfo QuotedBookingTypeCodeInfo
		{
			get { return GetZPropertyInfo(Schema.QuotedBookingTypeCode); }
		}

		public override string TableName
		{
			get { return ViewQuotedBookingSchema.Constants.TableName; }
		}

		public override string TablePrefix
		{
			get { return ObjectFactory.Get<IApplicationSchemaResolver>().GetColumnNamePrefix(TableName); }
		}

		IColumnValueRanker IWorkflowProviderCore.GetTemplateSelectionCriteria()
		{
			var result = new ColumnValueRanker();

			var jobHeader = Job;
			ZString containerModeForWorkflow = ContainerMode == Core.Constants.ContainerModes.FCL ? Core.Constants.ContainerModes.FCL : Core.Constants.ContainerModes.Other;

			result.Add(ProcessTaskTemplateSchema.P0_OH_Client, GetClientsInTemplateSelectionOrder());
			result.Add(ProcessTaskTemplateSchema.P0_GB, jobHeader != null && jobHeader.JH_GB.IsValid ? jobHeader.JH_GB : GlbBranch.CurrentBranch.PK, ZGuid.Empty);
			result.Add(ProcessTaskTemplateSchema.P0_GE, jobHeader != null && jobHeader.JH_GE.IsValid ? jobHeader.JH_GE : GlbDepartment.CurrentDepartment.PK, ZGuid.Empty);
			result.Add(ProcessTaskTemplateSchema.P0_SubType1, TransportMode, containerModeForWorkflow, ZString.Empty);
			result.Add(ProcessTaskTemplateSchema.P0_SubType2, QuotedBookingTypeCode, ZString.Empty);
			result.Add(ProcessTaskTemplateSchema.P0_SubType3, Direction, ZString.Empty);
			result.Add(ProcessTaskTemplateSchema.P0_LoadPortCountry, Origin, Origin.Substring(0, 2), ZString.Empty);
			result.Add(ProcessTaskTemplateSchema.P0_DischargePortCountry, Destination, Destination.Substring(0, 2), ZString.Empty);

			return result;
		}

		IZType[] GetClientsInTemplateSelectionOrder()
		{
			var result = new List<IZType>();
			var workflowType = ((IWorkflowProvider)this).WorkflowType;

			var collection = WorkflowDataRegistry.Instance.ClientInTemplateSelection.Value;
			var entry = collection.GetValueByCode(workflowType);

			if (entry != null)
			{
				foreach (ClientInTemplateSelectionCriteriaOrgType orgType in entry.SelectedItems)
				{
					switch (orgType.OrgTypeCode)
					{
						case WorkflowSelectionOrgTypeCodes.Client:
							if (Client != null)
							{
								result.Add(ClientPK);
							}

							break;

						case WorkflowSelectionOrgTypeCodes.ControllingCustomer:
							if (ControllingCustomer != null)
							{
								result.Add(ControllingCustomer.PK);
							}

							break;
					}
				}

				result.Add(ZGuid.Empty);
			}

			return result.ToArray();
		}

		void IProcessTaskTemplateUpdatable.Update(IProcessTaskTemplate taskTemplate)
		{
			ProcessTaskTemplate concreteTaskTemplate = taskTemplate as ProcessTaskTemplate;

			if (concreteTaskTemplate != null)
			{
				concreteTaskTemplate.P0_SubType1 = TransportMode;
				concreteTaskTemplate.P0_SubType2 = QuotedBookingTypeCode;
				concreteTaskTemplate.P0_SubType3 = Direction;
			}
		}

		IProcessHeaderCollection IWorkflowProvider.Workflows => Workflows;

		[ChildEditable]
		public IProcessHeaderCollection Workflows
		{
			get
			{
				if (workflows == null)
				{
					workflows = ProcessJobHeaderProvider.GetWorkflowsForParent(this, Factory);
					RegisterEditableChildObject(workflows);
				}

				return workflows;
			}
		}
		IProcessHeaderCollection workflows;

		ProcessTaskCollection IWorkflowProvider.WorkflowItems
		{
			get { return WorkflowItems; }
		}

		[ChildEditable(true)]
		public ProcessTaskCollection WorkflowItems
		{
			get
			{
				if (workflowItems == null)
				{
					workflowItems = this.GetOrCreateProcessTaskCollection(() => new QuotedBookingProcessTaskCollection(this));
					RegisterEditableChildObject(workflowItems);
				}

				return workflowItems;
			}
		}
		QuotedBookingProcessTaskCollection workflowItems;

		ZString IWorkflowProviderCore.WorkflowType
		{
			get { return WorkflowDescriptors.QuotedBookingWorkflowDescriptorCode; }
		}

		IWorkflowInformationProvider IWorkflowProvider.GetWorkflowInformationProvider()
		{
			return WorkflowInformationProvider;
		}

		WorkflowInformationProvider WorkflowInformationProvider
		{
			get
			{
				if (workflowInformationProvider == null)
				{
					var companyPks = (ObjectState == QuotedBookingState.BookingOnly)
						? new[] { GlbCompany.CurrentCompany.PK }
						: new[] { Quote.Company.PK };

					workflowInformationProvider = new WorkflowInformationProvider(companyPks);
				}

				workflowInformationProvider.Destination = (DestinationUNLOCO != null) ? DestinationUNLOCO.RL_PortName : ZString.Empty;
				workflowInformationProvider.Origin = (OriginUNLOCO != null) ? OriginUNLOCO.RL_PortName : ZString.Empty;
				workflowInformationProvider.BusinessContext = TrackingConstants.BusinessContext.Booking;

				return workflowInformationProvider;
			}
		}
		WorkflowInformationProvider workflowInformationProvider;

		bool ISometimesWorkflowProvider.ShouldSupportWorkflowTemplateApplication => !IsConsolidated;

		#endregion

		#region ICustomFieldProvider

		CustomBusinessObject ICustomFieldProvider.GetCustomBusinessObject(bool shouldRefresh)
		{
			if (customBusinessObject == null || shouldRefresh)
			{
				var parent = IsForwardRegistered && AreCustomFieldsMovedForOldConvertedBookings ? (BusinessObject)Booking : this;
				var properties = new UserDefinedPropertyCollection(parent).WithWorkflowTemplateCustomFields(this);
				customBusinessObject = new CustomBusinessObject(Factory, parent, properties);
			}
			return customBusinessObject;
		}
		CustomBusinessObject customBusinessObject;

		bool AreCustomFieldsMovedForOldConvertedBookings
		{
			get
			{
				var customFieldsQuery = new ZQuery(GenCustomAddOnValueSchema.XV_ParentTableCode, new[] { ViewQuotedBookingSchema.Constants.Prefix, JobShipmentSchema.Constants.Prefix });
				customFieldsQuery.AddToFilter(GenCustomAddOnValueSchema.XV_ParentID, new[] { PK, bookingPK });
				var customFields = Factory.Load<GenCustomAddOnValue>(customFieldsQuery);

				return !customFields.Any(t => t.XV_ParentID == PK && t.XV_ParentTableCode == ViewQuotedBookingSchema.Constants.Prefix)
					&& customFields.Any(t => t.XV_ParentID == bookingPK && t.XV_ParentTableCode == JobShipmentSchema.Constants.Prefix);
			}
		}

		#endregion

		#region ICartageParent Members

		event EventHandler ICartageParent.CartageTypesChanged
		{
			add { }
			remove { }
		}

		IReadOnlyCollection<CartageType> ICartageParent.CartageTypes
		{
			get { return new[] { ((ICartageParent)this).GetLocalCartageType }; }
		}

		CartageType ICartageParent.GetLocalCartageType
		{
			get { return new QuotedBookingPickupCartageType(this); }
		}

		ZString ICartageParent.UniqueConsignmentID
		{
			get { return ((ICartageParent)Booking).UniqueConsignmentID; }
		}

		ZGuid ICartageParent.CartageParentID
		{
			get { return Booking.PK; }
		}

		ZString ICartageParent.CartageParentTableCode
		{
			get { return JobShipmentSchema.Constants.Prefix; }
		}

		ZGuid ICartageParent.BranchPK
		{
			get { return ((ICartageParent)Booking).BranchPK; }
		}

		ZString ICartageParent.OrderReferenceNumber
		{
			get { return ((ICartageParent)Booking).OrderReferenceNumber; }
		}

		ZString ICartageParent.ServiceLevel
		{
			get { return ((ICartageParent)Booking).ServiceLevel; }
		}

		ControllerID ICartageParent.ControllerID
		{
			get { return ControllerIDs.QuotedBookings; }
		}

		ZString ICartageParent.WayBillNumber
		{
			get { return ((ICartageParent)Booking).WayBillNumber; }
		}

		ZString ICartageParent.GoodsDescription
		{
			get { return ((ICartageParent)Booking).GoodsDescription; }
		}

		ZGuid ICartageParent.JobHeaderPK
		{
			get { return ((ICartageParent)Booking).JobHeaderPK; }
		}

		ZGuid ICartageParent.LocalClientAddressPK
		{
			get { return ((ICartageParent)Booking).LocalClientAddressPK; }
		}

		ZInt ICartageParent.TotalPackages
		{
			get { return ((ICartageParent)Booking).TotalPackages; }
		}

		ZString ICartageParent.TotalPackType
		{
			get { return ((ICartageParent)Booking).TotalPackType; }
		}

		ZDecimal ICartageParent.TotalWeight
		{
			get { return ((ICartageParent)Booking).TotalWeight; }
		}

		ZString ICartageParent.TotalWeightUnit
		{
			get { return ((ICartageParent)Booking).TotalWeightUnit; }
		}

		ZDecimal ICartageParent.TotalVolume
		{
			get { return ((ICartageParent)Booking).TotalVolume; }
		}

		ZString ICartageParent.TotalVolumeUnit
		{
			get { return ((ICartageParent)Booking).TotalVolumeUnit; }
		}

		bool ICartageParent.RebuildLocalCartageMenuOnClick
		{
			get { return false; }
		}

		bool ICartageParent.UseJobTotals
		{
			get { return false; }
		}

		void ICartageParent.CartageCreatedAndSaved()
		{
		}

		IStmALogParent ICartageParent.BusinessObjectForRelatedEvents
		{
			get { return Booking; }
		}

		IDocManagerSupport ICartageParent.BusinessObjectForRelatedEDocs
		{
			get { return Booking; }
		}

		#endregion

		#region ITemplateReversible Members

		void ITemplateReversible.Reverse()
		{
			if (Quote != null && Quote.CurrentOneOffQuote != null)
			{
				((ITemplateReversible)Quote.CurrentOneOffQuote).Reverse();
			}
			if (Booking != null)
			{
				((ITemplateReversible)Booking).Reverse();
			}
			fConsigneeDocumentaryAddress = null;
			fConsignorDocumentaryAddress = null;
		}

		#endregion

		#region IRelatedJob Members

		ZString IRelatedJob.JobDescription
		{
			get { return HumanReadableNameWithoutID; }
		}

		ZString IRelatedJob.JobNumber
		{
			get
			{
				if (Booking != null)
				{
					return Booking.JS_UniqueConsignRef;
				}
				else if (Quote != null)
				{
					return Quote.TH_QuoteNumber;
				}

				return ZString.Empty;
			}
		}

		ZString IRelatedJob.JobStatus
		{
			get
			{
				if (Booking != null)
				{
					return Booking.JS_ShipmentStatus;
				}
				else if (Quote != null)
				{
					return Quote.QuoteStatus;
				}

				return ZString.Empty;
			}
		}

		#endregion

		#region IControllerIdProvider members

		Guid IControllerIDProvider.BusinessObjectPK
		{
			get { return PK.ToGuid(); }
		}

		ControllerID IControllerIDProvider.ControllerID
		{
			get { return ControllerIDs.QuotedBookings; }
		}

		#endregion

		#region Delete

		public override void Delete()
		{
			if (IsTemplateRecord)
			{
				TemplateRecord.Delete();
				return;
			}

			WorkflowItems.RemoveAndDeleteAll();
			RelatedChildActivityPivotCollection.DeleteAll();
			RelatedParentActivityPivotCollection.DeleteAll();
			new GenCustomAddOnRuleAckCollection(this).DeleteAll();
			base.Delete();
		}

		#endregion

		#region Metadata

		public IMetadata Metadata
		{
			get
			{
				if (!this.IsDeleted && (metadata == null || this.HasChanges))
				{
					metadata = NewMetadata();
				}
				return metadata;
			}
		}

		IMetadata metadata;

		IMetadata NewMetadata()
		{
			return ObjectFactory.Get<IMetadataProvider>().GetMetadataFromBO(this);
		}

		#endregion

		#region IStmNoteParent

		[MaxLength(StmNote.Schema.ST_NoteTextMaxLength)]
		public ZString DetailedGoodsDescriptionNoteText
		{
			get
			{
				if (IsOneOffQuote)
				{
					return ZString.Empty;
				}

				var note = DetailedGoodsDescriptionNote;
				if (note != null)
				{
					return note.ST_NoteText;
				}
				return ZString.Empty;
			}
			set
			{
				if (IsOneOffQuote)
				{
					return;
				}

				var note = DetailedGoodsDescriptionNote;
				if (note == null)
				{
					note = Notes.AddNew();
					note.ST_IsCustomDescription = false;
					note.ST_Description = PredefinedNoteTypes.Instance.DetailedGoodsDescription.Description;
				}

				BusinessObject.CheckMaximumLength(DetailedGoodsDescriptionNoteTextInfo, value);
				note.ST_NoteText = value;
				DetailedGoodsDescriptionNoteTextInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo DetailedGoodsDescriptionNoteTextInfo => GetZPropertyInfo(nameof(DetailedGoodsDescriptionNoteText));

		internal StmNote DetailedGoodsDescriptionNote
			=> Notes.FindByDescription(PredefinedNoteTypes.Instance.DetailedGoodsDescription.Code, false).FirstOrDefault();

		BusinessObject[] IStmNoteParent.BusinessObjectsWithRelatedNotes
		{
			get
			{
				List<BusinessObject> result = new List<BusinessObject>();

				if (Booking != null)
				{
					result.AddRange(Booking.BusinessObjectsWithRelatedNotes);
				}

				if (Quote != null)
				{
					result.AddRange(Quote.BusinessObjectsWithRelatedNotes);
				}

				return result.ToArray();
			}
		}

		[BusinessObjectTestExclude]
		public GetValueDelegate<NoteTypeCollection> CustomNoteTypesDelegate { get; set; }

		StmNoteContexts IStmNoteParent.NoteContextsForRelatedNotes
		{
			get
			{
				StmNoteContexts result = null;

				if (GetFromBooking)
				{
					result = StmNoteContextUtils.GetContextFromTransportModeImportExport(Constants.GlobalModuleNamesConstants.Forwarding, string.Empty, TransportMode, string.Empty);

					if (this.IsImport())
					{
						result.Direction |= StmNoteContextDirection.I;
						result.Direction |= StmNoteContextDirection.B;
					}

					if (this.IsExport())
					{
						result.Direction |= StmNoteContextDirection.E;
						result.Direction |= StmNoteContextDirection.B;
					}

					if (IsDomesticFreight)
					{
						result.Direction |= StmNoteContextDirection.D;
					}

					StmNoteContexts bookingNoteContextsForRelatedNotes = ((IStmNoteParent)Booking).NoteContextsForRelatedNotes;

					result.Module |= bookingNoteContextsForRelatedNotes.Module;
					result.Direction |= bookingNoteContextsForRelatedNotes.Direction;
					result.FreightMode |= bookingNoteContextsForRelatedNotes.FreightMode;
				}
				else
				{
					result = ((IStmNoteParent)Quote).NoteContextsForRelatedNotes;
				}

				return result;
			}
		}

		public NoteTypeCollection NoteTypes
		{
			get
			{
				if (noteTypes == null)
				{
					hasInstallCustomNoteTypes = false;

					var predefinedNoteTypes = new List<PredefinedNoteType>();

					if (Quote != null)
					{
						predefinedNoteTypes.AddRange(Quote.NoteTypes.Cast<PredefinedNoteType>());
					}

					if (Booking != null)
					{
						predefinedNoteTypes.AddRange(Booking.NoteTypes.Cast<PredefinedNoteType>());
					}

					noteTypes = new NoteTypeCollection();

					noteTypes.Add((NoteTypeCollection)Metadata.NoteTypes);

					foreach (PredefinedNoteType predefinedNoteType in predefinedNoteTypes.Distinct())
					{
						noteTypes.Add(predefinedNoteType);
					}
				}

				if (!hasInstallCustomNoteTypes && CustomNoteTypes != null)
				{
					hasInstallCustomNoteTypes = true;
					noteTypes.Add(CustomNoteTypes);
				}

				return noteTypes;
			}
		}
		NoteTypeCollection noteTypes;

		bool hasInstallCustomNoteTypes;

		NoteTypeCollection CustomNoteTypes
		{
			get
			{
				if (customNoteTypes == null && CustomNoteTypesDelegate != null)
				{
					customNoteTypes = CustomNoteTypesDelegate();
				}
				return customNoteTypes;
			}
		}
		NoteTypeCollection customNoteTypes;

		public Notes Notes
		{
			get { return notes ?? (notes = new QuotedBookingNotes(this)); }
		}
		QuotedBookingNotes notes;

		BusinessObjectFactory IStmNoteParent.NotesFactory
		{
			get { return Factory; }
		}

		/// <summary>
		/// Be aware, the PK is the Quote PK if there is a quote, otherwise the Booking PK
		/// </summary>
		ZGuid IStmNoteParent.NotesParentPK
		{
			get { return PK; } // used by DocDataPlugIn to load DocumentNote with mutex
		}

		string IStmNoteParent.NotesParentTableName
		{
			get { return ViewQuotedBookingSchema.Constants.TableName; } // used by DocDataPlugIn to load DocumentNote with mutex
		}

		bool IStmNoteParent.SupportsNotes
		{
			get { return true; }
		}

		public bool IsNoteParent(StmNote note)
		{
			return note != null && (Booking != null && note.ST_ParentID == Booking.PK || Quote != null && note.ST_ParentID == Quote.PK);
		}

		#endregion

		#region IStmALogParent Members

		public Logs Logs
		{
			get { return logs ?? (logs = GetNewLogs()); }
		}
		Logs logs;

		protected Logs GetNewLogs()
		{
			return new QuotedBookingLogs(this);
		}

		ZGuid IStmALogParent.LogsParentPK
		{
			get { return PK; }
		}

		string IStmALogParent.LogsParentTableName
		{
			get { return ViewQuotedBookingSchema.Constants.TableName; }
		}

		BusinessObjectFactory IStmALogProvider.LogsFactory
		{
			get { return Factory; }
		}

		BusinessObject[] IStmALogParent.BusinessObjectsWithRelatedEvents
		{
			get
			{
				List<BusinessObject> result = new List<BusinessObject>();

				if (Booking != null)
				{
					result.Add(Booking);
					result.AddRange(Booking.BusinessObjectsWithRelatedEvents);
					result.AddRange(TransportBooking.TransportBookingLoader.GetRelatedTransportBookingEvents(this));
				}

				if (Quote != null)
				{
					result.Add(Quote);
					result.AddRange(Quote.BusinessObjectsWithRelatedEvents);
				}

				result.AddRange(GetEventRelatedBufferManagementBusinessObjects());

				var job = GetJobHeaderForWorkflow();
				if (job != null)
				{
					result.Add(job);
				}

				return result.ToArray();
			}
		}

		JobHeader GetJobHeaderForWorkflow()
		{
			if (jobHeader != null && !jobHeader.IsDeleted && GlbCompany.CurrentCompany.PK == jobHeader.JH_GC)
			{
				return jobHeader;
			}

			return new JobHeader.Loader(JobParent).Load();
		}

		// Set 'this' as JobParent for consistency in JobHeader.Loader across LoadOrCreateJobHeaderWithMutex and InvoicingPluginToFreight.
		IJobHeaderParent JobParent => this;

		BusinessObject[] GetEventRelatedBufferManagementBusinessObjects()
		{
			var bizObjs = new List<BusinessObject>();
			var relatedBizoProviders = ObjectFactory.Get<IEnumerable>("BusinessObjectsWithRelatedEventsProviders");

			foreach (IRelatedBusinessObjectProvider provider in relatedBizoProviders)
			{
				bizObjs.AddRange(provider.GetRelatedBusinessObjects(this));
			}

			return bizObjs.ToArray();
		}

		void IStmALogParent.ProcessLog(IStmALog log)
		{
		}

		bool IStmALogParent.DeferFiringWorkflow
		{
			get { return false; }
		}

		#endregion

		#region IAutoRatingFreightConditionsSupportable Members

		RateLineConditionsSupporter IAutoRatingFreightConditionsSupportable.ConditionsSupporter
		{
			get { return conditionsSupporter ?? (conditionsSupporter = new QuotedBookingRateLineConditionsSupporter(this)); }
		}
		RateLineConditionsSupporter conditionsSupporter;

		#endregion

		#region IRatingSupporter Members

		RatingAdaptersProvider IRatingSupporter.AdaptersProvider
		{
			get { return new QuotedBookingRatingAdaptersProvider(this); }
		}

		#endregion

		#region IRelatableActivity Members

		ZBool IRelatableActivity.ShouldIgnoreSuperAndSubActivityRelationships
		{
			get { return true; }
		}

		ZString IRelatableActivity.ActivityType
		{
			get { return RelatableActivityTypeList.Codes.OneOffQuotes; }
		}

		IOrgHeader IRelatableActivity.Client
		{
			get { return Client; }
		}

		ZBool IRelatableActivity.ClientHasChanges
		{
			get { return ClientPKInfo.HasChanges; }
		}

		IOrgContact IRelatableActivity.Contact
		{
			get { return null; }
		}

		ZBool IRelatableActivity.ContactHasChanges
		{
			get { return false; }
		}

		ZString IRelatableActivity.Summary
		{
			get
			{
				var result = HumanReadableNameWithoutID;
				if (Quote != null && Quote.Company != null)
				{
					result = string.Format(CultureInfo.InvariantCulture, "({0}) {1}", Quote.Company.GC_RN_NKCountryCode, result);
				}

				return result;
			}
		}

		void IRelatableActivity.OnRelatedActivitySaving(IRelatableActivity relatedActivity)
		{
		}

		ZBool IRelatableActivity.SupportViewRelatedCommunications => ZBool.True;

		ZDateTime IAuditDetails.SystemCreateTimeUtc
		{
			get
			{
				switch (ObjectState)
				{
					case QuotedBookingState.QuoteOnly:
						return ((IAuditDetails)Quote).SystemCreateTimeUtc;

					case QuotedBookingState.BookingOnly:
						return ((IAuditDetails)Booking).SystemCreateTimeUtc;

					case QuotedBookingState.UnacceptedBookingWithQuote:
					case QuotedBookingState.AcceptedBookingWithQuote:
						return ((IAuditDetails)Quote).SystemCreateTimeUtc;

					default:
						return ZDateTime.Empty;
				}
			}
		}

		ZString IAuditDetails.SystemCreateUser
		{
			get
			{
				switch (ObjectState)
				{
					case QuotedBookingState.QuoteOnly:
						return ((IAuditDetails)Quote).SystemCreateUser;

					case QuotedBookingState.BookingOnly:
						return ((IAuditDetails)Booking).SystemCreateUser;

					case QuotedBookingState.UnacceptedBookingWithQuote:
					case QuotedBookingState.AcceptedBookingWithQuote:
						return ((IAuditDetails)Quote).SystemCreateUser;

					default:
						return ZString.Empty;
				}
			}
		}

		ZDateTime IAuditDetails.SystemLastEditTimeUtc
		{
			get
			{
				switch (ObjectState)
				{
					case QuotedBookingState.QuoteOnly:
						return ((IAuditDetails)Quote).SystemLastEditTimeUtc;

					case QuotedBookingState.BookingOnly:
						return ((IAuditDetails)Booking).SystemLastEditTimeUtc;

					case QuotedBookingState.UnacceptedBookingWithQuote:
					case QuotedBookingState.AcceptedBookingWithQuote:
						return ((IAuditDetails)Quote).SystemLastEditTimeUtc;

					default:
						return ZDateTime.Empty;
				}
			}
		}

		ZString IAuditDetails.SystemLastEditUser
		{
			get
			{
				switch (ObjectState)
				{
					case QuotedBookingState.QuoteOnly:
						return ((IAuditDetails)Quote).SystemLastEditUser;

					case QuotedBookingState.BookingOnly:
						return ((IAuditDetails)Booking).SystemLastEditUser;

					case QuotedBookingState.UnacceptedBookingWithQuote:
					case QuotedBookingState.AcceptedBookingWithQuote:
						return ((IAuditDetails)Quote).SystemLastEditUser;

					default:
						return ZString.Empty;
				}
			}
		}

		ZString IAuditDetailsWithContext.SystemCreateBranch
		{
			get
			{
				switch (ObjectState)
				{
					case QuotedBookingState.QuoteOnly:
						return ((IAuditDetailsWithContext)Quote).SystemCreateBranch;
					case QuotedBookingState.BookingOnly:
						return ((IAuditDetailsWithContext)Booking).SystemCreateBranch;
					case QuotedBookingState.UnacceptedBookingWithQuote:
					case QuotedBookingState.AcceptedBookingWithQuote:
						return ((IAuditDetailsWithContext)Quote).SystemCreateBranch;
					default:
						return ZString.Empty;
				}
			}
		}

		ZString IAuditDetailsWithContext.SystemCreateDepartment
		{
			get
			{
				switch (ObjectState)
				{
					case QuotedBookingState.QuoteOnly:
						return ((IAuditDetailsWithContext)Quote).SystemCreateDepartment;
					case QuotedBookingState.BookingOnly:
						return ((IAuditDetailsWithContext)Booking).SystemCreateDepartment;
					case QuotedBookingState.UnacceptedBookingWithQuote:
					case QuotedBookingState.AcceptedBookingWithQuote:
						return ((IAuditDetailsWithContext)Quote).SystemCreateDepartment;
					default:
						return ZString.Empty;
				}
			}
		}

		public IRelatedChildActivityPivotCollection RelatedChildActivityPivotCollection
		{
			get
			{
				if (relatedChildActivityPivotCollection == null)
				{
					relatedChildActivityPivotCollection = new RelatedChildActivityPivotCollection(this);
				}
				return relatedChildActivityPivotCollection;
			}
		}
		RelatedChildActivityPivotCollection relatedChildActivityPivotCollection;

		public IRelatedParentActivityPivotCollection RelatedParentActivityPivotCollection
		{
			get
			{
				if (relatedParentActivityPivotCollection == null)
				{
					relatedParentActivityPivotCollection = new RelatedParentActivityPivotCollection(this);
				}
				return relatedParentActivityPivotCollection;
			}
		}
		RelatedParentActivityPivotCollection relatedParentActivityPivotCollection;

		#endregion

		#region ISalesRelationActivity Members

		public ISalesRelationModel SalesRelationModel
		{
			get
			{
				if (salesRelationModel == null)
				{
					salesRelationModel = new SalesRelationModel(this);
					RegisterEditableChildObject(salesRelationModel);
				}

				return salesRelationModel;
			}
		}
		SalesRelationModel salesRelationModel;

		ZString ISalesRelationActivity.ActivityNotePropertyName
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region IImportParentRelatedActivityInfoOnNew Members

		bool IImportParentRelatedActivityInfoOnNew.ImportParentInfo(IRelatableActivity parentActivity, IImportRelatedActivityDeciderFactory deciderFactory)
		{
			if (parentActivity.Client != null)
			{
				ClientPK = parentActivity.Client.PK;
			}

			var salesAssociatedEntity = parentActivity as ISalesValueAssociatedEntity;
			if (salesAssociatedEntity != null)
			{
				var decider = deciderFactory.GetIfAvailable<IImportRelatedActivityTradeDetailDecider>();
				var decision = decider?.GetDecision(salesAssociatedEntity);
				if (decision != null)
				{
					if (decision.Cancelled)
					{
						return false;
					}
					else if (decision.SelectedTradeDetail != null)
					{
						ImportTradeDetailData(decision.SelectedTradeDetail);
					}
				}
			}

			return true;
		}

		#endregion

		#region ISupportTradeDetailImporting Members

		public bool ImportTradeDetailData(OrgTradeDetail tradeDetail)
		{
			if (tradeDetail.Parent == null)
			{
				return false;
			}

			var freightMode = ObjectFactory.Get<IFreightRatingHelper>().CalculateFreightMode(
				tradeDetail.TransportMode,
				tradeDetail.PA_TradeType,
				() => (tradeDetail.PA_TradeType == Constants.ContainerModes.LCL) ? FreightMode.NonContainerised : FreightMode.Containerised);

			var parentOrg = tradeDetail.Parent.ParentOrganisation;
			if (parentOrg != null)
			{
				ClientDocAddress.OrganisationPK = parentOrg.PK;
			}

			ConsigneeDocumentaryAddress.OrganisationPK = tradeDetail.PA_Calc_OH_Buyer;
			ConsignorDocumentaryAddress.OrganisationPK = tradeDetail.PA_Calc_OH_Supplier;
			TransportMode = tradeDetail.TransportMode;
			ContainerMode = tradeDetail.PA_TradeType;
			ServiceLevel = tradeDetail.ProspectDetail.PAP_RS_NKServiceLevel;

			Origin = ToUnlocoCode(tradeDetail.Parent.Origin);
			Destination = ToUnlocoCode(tradeDetail.Parent.Destination);

			Weight = tradeDetail.CurrentProspectPeriod.PAS_Weight;
			WeightUnit = tradeDetail.CurrentProspectPeriod.PAS_WeightUQ;
			Volume = tradeDetail.CurrentProspectPeriod.PAS_Volume;
			VolumeUnit = tradeDetail.CurrentProspectPeriod.PAS_VolumeUQ;
			PaymentTerms = tradeDetail.ProspectDetail.PAP_IncoTradeTerm;
			Commodity = tradeDetail.ProspectDetail.PAP_RH_NKCommodityCode;
			OH_Carrier = tradeDetail.ProspectDetail.PAP_OH_ServiceProvider;

			if (!Quote.CurrentOneOffQuote.Containers.ReadOnly && tradeDetail.ProspectDetail.Container != null)
			{
				var container = Quote.CurrentOneOffQuote.Containers.AddNew();
				container.TC_ContainerCount = (ZShort)tradeDetail.CurrentProspectPeriod.PAS_Units.ToZInt();
				container.TC_RC = tradeDetail.ProspectDetail.Container.PK;
			}

			return true;
		}

		static string ToUnlocoCode(ViewLocation viewLocation)
		{
			if (viewLocation != null && viewLocation.IsUNLOCO)
			{
				return viewLocation.VLO_Code
					.SubstringSafe(0, 5); // unloco should all have a code length of 5, but this substring is needed so that CodeAnalysis EDI003 doesn't complain
			}

			return string.Empty;
		}

		#endregion

		#region Services

		public JobServiceDependentCollection Services
		{
			get { return Booking?.DocsAndCartage?.Services; }
		}

		#endregion

		#region IDefaultNumberOfDecimalsSupporter Members

		ZString IDefaultNumberOfDecimalsSupporter.TransportMode
		{
			get { return TransportMode; }
		}

		public int GetDefaultNumberOfDecimals(PropertyDescriptor property)
		{
			return DefaultNumberOfDecimalsSupporterHelperForFreight.GetDefaultNumberOfDecimalsMetaDataProperty(this, property);
		}

		ZString IDefaultNumberOfDecimalsSupporter.GetUnitOfMeasure(PropertyDescriptor property)
		{
			var unitOfMeasure = ZString.Empty;
			var propertyName = DefaultNumberOfDecimals.GetBoundPropertyName(property.Name);

			switch (propertyName)
			{
				case Schema.Weight:
					unitOfMeasure = WeightUnit;
					break;

				case Schema.Volume:
					unitOfMeasure = VolumeUnit;
					break;

				case Schema.Chargeable:
					unitOfMeasure = ChargeableUnit;
					break;

				default:
					break;
			}

			return unitOfMeasure;
		}

		public ZDecimal GetRoundedValue(SchemaColumn column, PropertyDescriptor property, ZDecimal value)
		{
			return DefaultNumberOfDecimalsSupporterWithSchemaColumnHelperForFreight.GetRoundedValue(this, column, property, value);
		}

		public ZDecimal GetRoundedValue(PropertyDescriptor property, ZDecimal value)
		{
			return DefaultNumberOfDecimalsSupporterHelperForFreight.GetRoundedValue(this, property, value);
		}

		void IDefaultNumberOfDecimalsSupporter.RoundMeasurePropertiesOnTransportModeChanged()
		{
			this.SetRoundedValue(WeightInfo);
			this.SetRoundedValue(VolumeInfo);
			this.SetRoundedValue(ChargeableInfo);
		}

		#endregion

		#region IDtbBooking Members

		ZString TransportBooking.IDtbBookingParent.JobTypeDescription
		{
			get { return HumanReadableNameWithoutID; }
		}

		ZString TransportBooking.IDtbBookingParent.JobType
		{
			get { return ((IWorkflowProvider)this).WorkflowType; }
		}

		DtbBookingDirection[] TransportBooking.IDtbBookingParent.GetSupportedDirections()
		{
			return CanCreateTransportBooking ? new DtbBookingDirection[] { DtbBookingDirection.PIC } : Array.Empty<DtbBookingDirection>();
		}

		IJobInvoicingPlugIn TransportBooking.IDtbBookingParent.InvoicingJob
		{
			get { return this; }
		}

		void TransportBooking.IDtbBookingParent.TransportBookingCreatedOrUpdated(IEnumerable<IDtbBooking> bookings)
		{
		}

		ZBool TransportBooking.IDtbBookingParent.IsSupportsDirectSchedule
		{
			get { return true; }
		}

		public ZQuery TransportBookingTemplateFilters => new ZQuery();

		bool TransportBooking.IDtbBookingParent.RequiresMultiContainerBooking => true;

		#endregion

		#region ISendEmailSource

		AddressBookSelection ISendEmailSource.GetAddressBookSelection()
		{
			return GetAddressBookSelection();
		}

		AddressBookSelection GetAddressBookSelection()
		{
			if (GetFromBooking)
			{
				return ((ISendEmailSource)Booking)?.GetAddressBookSelection();
			}
			else
			{
				var result = new AddressBookSelection();
				result.AddRecipient(Client);
				result.AddRecipient(Consignor);
				result.AddRecipient(Consignee);
				result.AddRecipient(ExportBroker);
				result.AddRecipient(ImportBroker);
				result.AddRecipient(ControllingCustomer);
				result.AddRecipient(Carrier);

				return result;
			}
		}

		string ISendEmailSource.EmailSubject
		{
			get
			{
				var emailSubject = GetFromBooking
					? ((ISendEmailSource)Booking).EmailSubject
					: Res.GetString("43640f16-0ad6-4f8d-86db-d950434ff648", "{0} - {1}", Env.Instance.Registry.Rating.OneOffQuoteTitleText, Quote.TH_QuoteNumber);

				return emailSubject;
			}
		}

		string ISendEmailSource.TemplateCategory
		{
			get { return MailTemplateCategoryList.Codes.Bookings; }
		}

		string ISendEmailSource.DefaultFromDisplayName
		{
			get { return GlbStaff.CurrentUser.GS_FullName; }
		}

		string ISendEmailSource.OverridingDefaultFromEmailAddress
		{
			get { return null; }
		}

		Type ISendEmailSource.DocWrapperType
		{
			get { return ((ISendEmailSource)Booking)?.DocWrapperType; }
		}

		Logs ISendEmailSource.Logs
		{
			get { return GetFromBooking ? Booking.Logs : Quote.Logs; }
		}

		#endregion

		#region Template record

		bool ITemplateRecordProvider.IsTemplateRecord
		{
			get => IsTemplateRecord;
			set => IsTemplateRecord = value;
		}

		bool IsTemplateRecord { get; set; }

		public ZBool IsTemplate => IsTemplateRecord;

		public ZPropertyInfo IsTemplateInfo => GetZPropertyInfo(nameof(IsTemplate));

		ITemplateRecord ITemplateRecordProvider.TemplateRecord
		{
			get => TemplateRecord;
			set => TemplateRecord = (StmTemplateRecord)value;
		}

		public StmTemplateRecord TemplateRecord
		{
			get { return templateRecord; }
			set
			{
				templateRecord?.UnRegisterEditableChildObject(this);
				templateRecord = value;
				templateRecord?.RegisterEditableChildObject(this);
			}
		}
		StmTemplateRecord templateRecord;

		void ITemplateRecordProvider.SaveToTemplateRecord()
		{
			if (!IsDeleted && !TemplateRecord.IsDeleted)
			{
				WriteToTemplateRecord(TemplateRecord);
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "XML header")]
		void WriteToTemplateRecord(StmTemplateRecord targetTemplateRecord)
		{
			const string xmlDeclaration = @"<?xml version=""1.0"" encoding=""utf-8""?>";

			var manager = (IShipmentDataContextManager)this.GetUniversalDataContextManager();
			var actionInfo = new ActionInfo(new[] { new RecipientRoleDetail { Type = RecipientRoleType.FOR, ServiceCode = ServiceCodeType.HLD } }, this)
			{
				ActionType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML
			};
			var outboundSessionTracker = new DataWritingManager(actionInfo);
			var writer = manager.GetShipmentDataObjectWriter(outboundSessionTracker);
			var shipment = writer.GetDataObject(this);

			string bookingAsString;

			using (var stream = new CargoWise.IO.Shim.SubStreamableStream())
			{
				var xmlWriter = ObjectFactory.Get<IXmlWriter>();
				xmlWriter.WriteXML(shipment, stream);

				using (var reader = new StreamReader(stream))
				{
					bookingAsString = reader.ReadToEnd();
					if (bookingAsString.StartsWith(xmlDeclaration, StringComparison.Ordinal))
					{
						bookingAsString = bookingAsString.Remove(0, xmlDeclaration.Length);
					}
				}
			}

			targetTemplateRecord.STR_Data = bookingAsString;

			targetTemplateRecord.STR_ModuleID = nameof(ModuleId.QuotedBookings);
			targetTemplateRecord.PopulateIdIfNeeded();
			if (QuotedBookingNumber.IsEmpty)
			{
				QuotedBookingNumber = targetTemplateRecord.STR_ReferenceId;
			}
		}

		void ITemplateRecordProvider.LoadFromTemplateRecord(ITemplateRecord sourceTemplateRecord)
		{
			if (!IsDeleted)
			{
				ReadFromTemplateRecord((StmTemplateRecord)sourceTemplateRecord);
			}
		}

		void ReadFromTemplateRecord(StmTemplateRecord sourceTemplateRecord)
		{
			var logger = new UniversalDataBuss.Integration.DummyLogger();

			var shipmentDataObject = new Shipment(DefaultDataObjectWriterStrategy.Instance);
			var data = sourceTemplateRecord.STR_Data;
			var bookingAsString = data;

			using (var stream = (SubStreamableStream)new MemoryStream(Encoding.UTF8.GetBytes(bookingAsString)))
			{
				ObjectFactory.Get<IXmlReader>().ReadXML(shipmentDataObject, stream, logger);
			}

			SetCompanyAndDataProviderDetails(shipmentDataObject, logger);

			var manager = this.GetUniversalDataContextManager() as IShipmentDataContextManagerInternal
				?? throw new InvalidOperationException("Unsupported Data Context Manager");

			var universalObjectFactory = new UniversalObjectFactory(Factory);
			var reader = manager.GetShipmentDataObjectReader(shipmentDataObject, logger, universalObjectFactory)
				?? throw new InvalidOperationException(FormattableString.Invariant($"{manager.GetType().FullName} returned <null> reader for QuotedBooking Template Record {sourceTemplateRecord.STR_ReferenceId}"));

			IsTemplateRecord = true;
			TemplateRecord = sourceTemplateRecord;

			//create Booking so templateRecordReader.ReadDirectlyIntoBusinessObject can set its values
			if (Booking == null)
			{
				bookingPK = CreateNewBooking(this.Factory).PK;
				this.BusinessObjectRelationChanged(Booking, BusinessObjectParentLocatorEvent.ParentAdded);

				// the code that hydrates the notes need existing notes (with matching descriptions) to get the right type
				// i.e. ForwardingShipmentStmNote rather than StmNote
				if (shipmentDataObject.NoteCollection != null)
				{
					foreach (var note in shipmentDataObject.NoteCollection)
					{
						if (note.Description.HasValue)
						{
							var placeholderNote = Booking!.Notes.AddNew();
							placeholderNote.ST_Description = note.Description.Value;
						}
					}
				}
			}
			Booking.SetIsTemplateRecord(true);

			var templateRecordReader = reader as ITopLevelDataObjectReaderForTemplateRecord;
			if (templateRecordReader == null)
			{
				ErrorReporter.ReportOnce(FormattableString.Invariant($"{manager.GetType().FullName} returned {reader.GetType().FullName} which does not implement {nameof(ITopLevelDataObjectReaderForTemplateRecord)} to read QuotedBooking Template Record {sourceTemplateRecord.STR_ReferenceId}."));

				BusinessObject bizo = this;
				reader.ReadIntoBusinessObject(ref bizo);
			}
			else
			{
				templateRecordReader.ReadDirectlyIntoBusinessObject(this);
			}

			if (QuotedBookingNumber.IsEmpty)
			{
				QuotedBookingNumber = sourceTemplateRecord.STR_ReferenceId;
			}

			if (sourceTemplateRecord.IsInDatabase)
			{
				FixHasChangesAfterTemplateRead();
			}
		}

		void FixHasChangesAfterTemplateRead()
		{
			((IBusinessObjectState)this).ClearHasChangesIncludingChildren();
			if (this.Booking != null)
			{
				((IBusinessObjectState)this.Booking).ClearHasChangesIncludingChildren();
				((INeedRow)this.Booking).Row.AcceptChanges();
				foreach (var address in this.Booking.DocAddresses)
				{
					((IBusinessObjectState)address).ClearHasChangesIncludingChildren();
					((INeedRow)address).Row.AcceptChanges();
				}
			}
				((INeedRow)this.Quote)?.Row.AcceptChanges();
		}

		BusinessObject ITemplateRecordProvider.InstantiateFromTemplateRecord(BusinessObjectFactory factory, Type elementType, ITemplateRecord otherTemplateRecord)
		{
			var otherElement = QuotedBooking.New(factory, otherTemplateRecord); //We will load from the template inside of QuotedBooking.Initialize.
			if (elementType == typeof(QuotedBooking))
			{
				return otherElement;
			}
			else if (elementType == typeof(ViewQuotedBooking))
			{
				return ViewQuotedBooking.LoadOrCreate(otherElement);
			}
			else
			{
				throw new InvalidOperationException("Type was unexpected: " + elementType.ToString());
			}
		}

		void SetCompanyAndDataProviderDetails(TopLevelDataObject bizo, UniversalDataBuss.Integration.DummyLogger logger)
		{
			if (bizo.DataContext != null)
			{
				bizo.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
				logger.TopLevelDataObject = bizo;
			}
		}

		#endregion

		#region IDocAddresses Members

		ZValidation IDocAddresses.PiggyBackedDocAddressValidation(JobDocAddress addressToValidate)
		{
			return new QuotedBookingDocAddressValidation(addressToValidate, Booking);
		}

		SecurityCheckpoint IDocAddresses.GetCanOverrideCheckpoint(JobDocAddress docAddress)
		{
			return Env.Security.None;
		}

		#region GetDocAddressRequirements

		Lazy<JobDocAddressRequirement> NotifyPartyAddressRequirement => new Lazy<JobDocAddressRequirement>(() => new JobDocAddressRequirement(DocAddressType.NotifyParty, ContactType.NotifyParty));
		Lazy<JobDocAddressRequirement> NotifyParty2AddressRequirement => new Lazy<JobDocAddressRequirement>(() => new JobDocAddressRequirement(DocAddressType.NotifyParty2, ContactType.NotifyParty));
		Lazy<JobDocAddressRequirement> NotifyParty3AddressRequirement => new Lazy<JobDocAddressRequirement>(() => new JobDocAddressRequirement(DocAddressType.NotifyParty3, ContactType.NotifyParty));

		JobDocAddressRequirement IDocAddresses.GetDocAddressRequirement(DocAddressType addressType)
		{
			switch (addressType)
			{
				case DocAddressType.NotifyParty:
					return NotifyPartyAddressRequirement.Value;
				case DocAddressType.NotifyParty2:
					return NotifyParty2AddressRequirement.Value;
				case DocAddressType.NotifyParty3:
					return NotifyParty3AddressRequirement.Value;
				default:
					return null;
			}
		}

		#endregion

		void IDocAddresses.DocAddressChanged(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OrgAddressBeforeChange(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OnBeforeDocAddressDeleted(JobDocAddress docAddress)
		{
			if (!IsDeleted && !IsDeleting)
			{
				docAddress.OrganisationPK = ZGuid.Empty;
			}
		}

		void IDocAddresses.AnyAddressFieldBeforeChange(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OrgHeaderAfterChange(JobDocAddress docAddress)
		{
		}

		bool IDocAddresses.CanDeleteAddress(JobDocAddress docAddress)
		{
			return true;
		}

		OrgHeaderCollection IDocAddresses.GetOrgHeaderList(DocAddressType addressType)
		{
			return null;
		}

		JobDocAddressDependentCollection IDocAddresses.DocAddresses
		{
			get => ((IDocAddresses)Booking ?? Quote).DocAddresses;
		}

		#region SupportedAddressTypes
		IReadOnlyList<DocAddressType> IDocAddresses.SupportedAddressTypes
		{
			get
			{
				return new DocAddressType[]
				{
					DocAddressType.ConsignorDocumentaryAddress,
					DocAddressType.ConsignorPickupDeliveryAddress,
					DocAddressType.ConsigneeDocumentaryAddress,
					DocAddressType.ConsigneePickupDeliveryAddress,
					DocAddressType.PickupAgent,
					DocAddressType.DeliveryAgent,
					DocAddressType.ExportBroker,
					DocAddressType.ImportBroker,
					DocAddressType.NotifyParty,
					DocAddressType.NotifyParty2,
					DocAddressType.NotifyParty3
				};
			}
		}
		#endregion

		#region ICreditControlledDocumentDelivery Members

		bool ICreditControlledDocumentDelivery.IsDPSFreightMovementRestricted
		{
			get
			{
				if (((IComplianceItemRiskStatusProvider)this).IsEnabledComplianceWise)
				{
					return ObjectFactory.Get<IComplianceRiskStatusSupporter>().IsDPSFreightMovementRestricted(Booking?.JS_ScreeningStatus ?? ScreeningStatusesList.Codes.Release, this, this);
				}

				return ((ICreditControlledDocumentDelivery)Booking)?.IsDPSFreightMovementRestricted ?? false;
			}
		}

		bool ICreditControlledDocumentDelivery.IsAviationSecurityFreightMovementRestricted =>
			((ICreditControlledDocumentDelivery)Booking)?.IsAviationSecurityFreightMovementRestricted ?? false;

		OrgHeader[] ICreditControlledDocumentDelivery.OrganisationsForCreditChecks
		{
			get
			{
				var result = new List<OrgHeader>();

				if (Consignor is OrgHeader consignor && this.IsCreditLimitCheckRequired(OrgCodes.Consignor))
				{
					result.Add(consignor);
				}
				if (Consignee is OrgHeader consignee && this.IsCreditLimitCheckRequired(OrgCodes.Consignee))
				{
					result.Add(consignee);
				}
				if (Job?.LocalCharges is OrgHeader localCharges && this.IsCreditLimitCheckRequired(OrgCodes.LocalClient))
				{
					result.Add(localCharges);
				}
				if (Debtors is OrgHeader[] debtors && debtors.Length > 0 && this.IsCreditLimitCheckRequired(OrgCodes.AllDebtors))
				{
					result.AddRange(debtors);
				}
				if (AccountingMasterFilesRegistry.Instance.EnableCreditControlEvaluationForControllingCustomersAndControllingAgents.Value)
				{
					if (ControllingCustomer != null && this.IsCreditLimitCheckRequired(OrgCodes.ControllingCustomer))
					{
						result.Add(ControllingCustomer);
					}
					if (ControllingAgent != null && this.IsCreditLimitCheckRequired(OrgCodes.ControllingAgent))
					{
						result.Add(ControllingAgent);
					}
				}

				return result.Distinct(BusinessObjectEqualityComparer<OrgHeader>.PKOnlyComparer).ToArray();
			}
		}

		OrgHeader[] Debtors => Factory.GetValue(ref sellAccountsCachedProperty, () =>
		{
			OrgHeader[] result = null;
			if (Job is JobHeader job)
			{
				result = Factory.Load<JobCharge>(new ZQuery(JobChargeSchema.JR_JH, job.PK)).Select(x => x.SellAccount)
					.Where(x => x != null).Distinct().ToArray();
			}
			return result ?? Array.Empty<OrgHeader>();
		});
		CachedProperty<OrgHeader[]> sellAccountsCachedProperty;

		string ICreditControlledDocumentDelivery.DescriptionOfOrganisationBeingCheckedForCredit => Res.GetString("ea5708d9-0617-4595-b38f-53b17e1520b1", "Consignee, Consignor, Local Client or any Debtors for charges on the Billing tab");

		CustomMessageBoxCallback ICreditControlledDocumentDelivery.DocumentLoginMessageBoxCallback { get; set; }

		string[] IRelatedJobNumber.JobNumber => IsOneOffQuote
			? new string[] { Quote.TH_QuoteNumber }
			: new string[] { Booking.JS_UniqueConsignRef };

		event EventHandler<SecurityLoginEventArgs> ICreditControlledDocumentDelivery.GetDocumentLogin
		{
			add { fGetDocumentLogin += value; }
			remove { fGetDocumentLogin -= value; }
		}
		event EventHandler<SecurityLoginEventArgs> fGetDocumentLogin;

		ScreeningParty[] ICreditControlledDocumentDelivery.GetScreeningParties() => Array.Empty<ScreeningParty>();

		void ICreditControlledDocumentDelivery.RaiseOnGetDocumentLogin(SecurityLoginEventArgs e)
		{
			fGetDocumentLogin?.Invoke(this, e);
		}
		#endregion

		#endregion

		#region JS_QuoteBookingGenericOrderNumber

		public ZString JS_QuoteBookingGenericOrderNumber
		{
			get
			{
				var sortedOrders = Booking?.LegacyOrders ?? new List<IAttachedOrder>();
				if (sortedOrders != null)
				{
					var orders = new ZStringBuilder();

					foreach (IAttachedOrder order in sortedOrders.Take(3))
					{
						orders.Append(order.JobNo);
					}

					var result = orders.ToStringWithDelimiterBetweenAppends(", ");

					return (sortedOrders.Count > 3) ? result + "..." : result;
				}

				return ZString.Empty;
			}
		}

		#endregion

		#region TT_QuoteApprovedByManager

		[ResourceStringData("ViewQuotedBooking|OneOffQuoteApprovalStatus", Caption = "Quote Status", FullDescription = "One Off Quote Approval Status")]
		public ZBool OneOffQuoteApprovalStatus
		{
			get
			{
				return Quote?.CurrentOneOffQuote?.TT_QuoteApprovedByManager ?? false;
			}
			set
			{
				if (Quote?.CurrentOneOffQuote != null)
				{
					Quote.CurrentOneOffQuote.TT_QuoteApprovedByManager = value;
					OneOffQuoteApprovalStatusInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo OneOffQuoteApprovalStatusInfo
		{
			get { return GetZPropertyInfo(Schema.OneOffQuoteApprovalStatus); }
		}

		#endregion

		#region OneOffQuoteIsAmended

		[ResourceStringData("ViewQuotedBooking|OneOffQuoteIsAmended", Caption = "Amended", FullDescription = "One Off Quote Is Amended")]
		public ZBool OneOffQuoteIsAmended
		{
			get
			{
				if (IsOneOffQuote)
				{
					var latestAmendedQuote = Quote?.GetMostRecentAmendment(true);
					return
						latestAmendedQuote != null &&
						Quote.TH_QuoteNumber != latestAmendedQuote.TH_QuoteNumber;
				}
				return ZBool.False;
			}
		}

		public ZPropertyInfo OneOffQuoteIsAmendedInfo => GetZPropertyInfo(Schema.OneOffQuoteIsAmended);

		#endregion

		#region IConversationProvider

		JobConversation IConversationProvider.eConversation => ((IConversationProvider)Booking)?.eConversation;
		ModuleIdentifier IConversationProvider.ParentModule => ((IConversationProvider)Booking)?.ParentModule;
		ControllerID IConversationProvider.ParentController => ((IConversationProvider)Booking)?.ParentController;
		IEnumerable<EConversation.Business.RelatedParty> IConversationProvider.AdditionalParticipants => ((IConversationProvider)Booking)?.AdditionalParticipants;
		bool IConversationProvider.SendEmailNotificationsOnSave => ((IConversationProvider)Booking)?.SendEmailNotificationsOnSave ?? false;
		void IConversationProvider.RunConversationUpdateActionBeforeSaving() => ((IConversationProvider)Booking)?.RunConversationUpdateActionBeforeSaving();
		string IConversationProvider.EmailSubjectContentOverride => ((IConversationProvider)Booking)?.EmailSubjectContentOverride;
		string IConversationProvider.FromAddressOverride => ((IConversationProvider)Booking)?.FromAddressOverride;
		NotificationEmailTemplate IConversationProvider.NotificationEmailTemplateOverride => ((IConversationProvider)Booking)?.NotificationEmailTemplateOverride;

		#endregion

		#region IUniversalCopyValidationStrategy Member

		public string ValidateUniversalCopyPreconditions(CopyTemplateTree configurationTree) =>
			QuotedBookingHelper.ValidateUniversalCopyPreconditions(configurationTree, this);

		#endregion

		#region GetStrategies

		protected override IBusinessObjectStrategy[] GetStrategies()
		{
			if (strategies == null)
			{
				var baseStrategies = base.GetStrategies();

				var strategyList = new List<IBusinessObjectStrategy>();
				if (baseStrategies != null)
				{
					strategyList.AddRange(baseStrategies);
				}

				strategyList.Add(new QuotedBookingLoggingStrategy());

				strategies = strategyList.ToArray();
			}
			return strategies;
		}

		IBusinessObjectStrategy[] strategies;

		#endregion

		#region RecalculateRelatedParties

		public (bool WasSuccessful, ZString Log) RecalculateRelatedParties()
		{
			if (Booking == null)
			{
				return (false,
					Res.GetString("24c5c975-3703-447c-80d1-93e68284f45d",
						"Recalculation of related parties is only available for Booking with Quote and Quick Booking."));
			}

			if (!this.IsExport() && !this.IsImport())
			{
				return (false,
					Res.GetString("2888ec0c-ace0-4fdb-a43e-afc45af4ace9",
						"You cannot Recalculate Related Parties for this company because the company does not match Pickup or Delivery direction of this job."));
			}

			RecalculateExportParties();
			RecalculateImportParties();

			this.OnElementChanged();

			return (true, ZString.Empty);
		}

		void RecalculateExportParties()
		{
			if (this.IsExport())
			{
				RedefaultExportBroker();
				RedefaultPickupCompany();
				RedefaultPickupAgent();
				RedefaultPickupCFS();
				RedefaultControllingCustomer();
				RedefaultControllingAgent();
			}
		}

		void RedefaultExportBroker()
		{
			if (!Booking.JS_OH_ExportBroker.IsEmpty)
			{
				var args = RaiseAttemptEvent(ResString.GetMultilingualString("ff7b8c88-e066-4a80-97bf-9633e17965fb", "Export Broker"));
				if (!args.Cancel)
				{
					Booking.SetDefaultExportBroker();
				}
			}
			else
			{
				Booking.SetDefaultExportBroker();
			}
		}

		void RedefaultPickupCompany()
		{
			if (!Booking.DocsAndCartage.JP_OA_PickupCartageCoAddr_ZAddress.OrgPK.IsEmpty)
			{
				var args = RaiseAttemptEvent(ResString.GetMultilingualString("9acbfa12-02b1-4f4b-b674-06c7e372903b", "Port Transport"));
				if (!args.Cancel)
				{
					Booking.DefaultPickupCompany();
				}
			}
			else
			{
				Booking.DefaultPickupCompany();
			}
		}

		void RedefaultPickupCFS()
		{
			if (!Booking.JS_OA_ExportReceivingDepot.IsEmpty)
			{
				var args = RaiseAttemptEvent(QuotedBookingHelper.GetPickupDeliveryOrgTitles(Mode).PickupOrgTitle);
				if (!args.Cancel)
				{
					Booking.SetPickupCFS();
				}
			}
			else
			{
				Booking.SetPickupCFS();
			}
		}

		void RedefaultPickupAgent()
		{
			if (!Booking.PickupAgentDocumentaryAddress.OrganisationPK.IsEmpty)
			{
				var args = RaiseAttemptEvent(ResString.GetMultilingualString("ce47dce2-18c6-4380-afd5-3d9160d87c0b", "Pickup Agent"));
				if (!args.Cancel)
				{
					Booking.SetPickupAgent();
				}
			}
			else
			{
				Booking.SetPickupAgent();
			}
		}

		void RecalculateImportParties()
		{
			if (this.IsImport())
			{
				RedefaultImportBroker();
				RedefaultDeliveryCFS();
				RedefaultDeliveryAgent();
				RedefaultControllingCustomer();
				RedefaultControllingAgent();
			}
		}

		void RedefaultImportBroker()
		{
			if (!Booking.JS_OH_ImportBroker.IsEmpty)
			{
				var args = RaiseAttemptEvent(ResString.GetMultilingualString("84cce9c6-0e3a-4c87-91cc-6aea7ab5a96f", "Import Broker"));
				if (!args.Cancel)
				{
					Booking.SetDefaultImportBroker();
				}
			}
			else
			{
				Booking.SetDefaultImportBroker();
			}
		}

		void RedefaultDeliveryCFS()
		{
			if (!Booking.JS_OA_ImportReleaseDepot.IsEmpty)
			{
				var args = RaiseAttemptEvent(QuotedBookingHelper.GetPickupDeliveryOrgTitles(Mode).DeliveryOrgTitle);
				if (!args.Cancel)
				{
					Booking.SetDeliveryCFS();
				}
			}
			else
			{
				Booking.SetDeliveryCFS();
			}
		}

		void RedefaultDeliveryAgent()
		{
			if (!Booking.JS_OH_DeliveryAgent.IsEmpty)
			{
				var args = RaiseAttemptEvent(ResString.GetMultilingualString("8bf23d02-b763-417f-ae09-75e338d904cc", "Delivery Agent"));
				if (!args.Cancel)
				{
					Booking.SetDeliveryAgent();
				}
			}
			else
			{
				Booking.SetDeliveryAgent();
			}
		}

		void RedefaultControllingAgent()
		{
			if (!Booking.ControllingAgentDocumentaryAddress.E2_OA_Address.IsEmpty)
			{
				var args = RaiseAttemptEvent(ResString.GetMultilingualString("d3d4c99c-acf2-45f6-9e86-e05a27db36d9", "Controlling Agent"));
				if (!args.Cancel)
				{
					Booking.DefaultControllingAgent(true);
				}
			}
			else
			{
				Booking.DefaultControllingAgent(true);
			}
		}

		void RedefaultControllingCustomer()
		{
			if (!Booking.ControllingCustomerAddress.OrganisationPK.IsEmpty)
			{
				var args = RaiseAttemptEvent(ResString.GetMultilingualString("175e725d-2904-4f81-b30a-531986c59100", "Controlling Customer"));
				if (!args.Cancel)
				{
					Booking.DefaultControllingCustomer();
				}
			}
			else
			{
				Booking.DefaultControllingCustomer();
			}
		}

		RecalculateRelatedPartyCancelEventArgs RaiseAttemptEvent(ZString propertyName)
		{
			var args = new RecalculateRelatedPartyCancelEventArgs();
			args.RecalculatedPropertyName = propertyName;

			if (AttemptToUpdateExistingValueInRecalculation != null)
			{
				AttemptToUpdateExistingValueInRecalculation(this, args);
			}

			return args;
		}

		public class RecalculateRelatedPartyCancelEventArgs : CancelEventArgs
		{
			public string RecalculatedPropertyName { get; set; }
		}

		public event EventHandler<RecalculateRelatedPartyCancelEventArgs> AttemptToUpdateExistingValueInRecalculation;

		#endregion

		#region IComplianceRiskStatusProvider

		IEnumerable<IScreeningParty> ICompliancePartyRiskStatusProvider.Parties
		{
			get
			{
				FetchForPartiesComplianceSynchronization();

				var parties = new List<IScreeningParty>();

				if (Booking != null)
				{
					parties.AddRange(((ICompliancePartyRiskStatusProvider)this.Booking).Parties);
				}

				if (ClientAddrPK_ZAddress?.OrgHeader is OrgHeader clientAddrOrg)
				{
					parties.Add(new ScreeningParty(this, Res.GetString("bc1252d3-444b-4c2e-ba1a-8e6e685d9659", "Client"), clientAddrOrg));
				}

				if (Sailing != null && Sailing.JX_TransportMode == Constants.TransportModes.Sea && !Sailing.JX_JV_NKVessel.IsEmpty)
				{
					if (Sailing.Vessel != null)
					{
						parties.Add(new ScreeningParty(this, Res.GetString("c58e685d-d7f2-4259-a54c-1fdad68d506c", "Vessel"), sailing.Vessel));
					}
					else
					{
						parties.AddRange(((IScreeningPartyProvider)this).ScreeningParties);
					}
				}

				if (Job != null)
				{
					parties.Add(new ScreeningParty(this, Res.GetString("9cbe2281-4ca7-4e73-a02e-38a0c974757e", "Local Client"), Job.LocalCharges));
					parties.Add(new ScreeningParty(this, Res.GetString("27b376ce-50de-4e33-b6df-1c50a868693e", "Overseas Agent"), Job.AgentCollect));
				}

				foreach (var container in QuotedBookingContainers.OfType<ForwardingContainer>())
				{
					var arrivalYard = container.ArrivalContainerYardAddress != null ? Factory.Load<OrgHeader>(container.ArrivalContainerYardAddress.OA_OH) : null;
					if (arrivalYard != null)
					{
						parties.Add(new ScreeningParty(this, Res.GetString("2c2cf94e-bb8d-4ce2-a8ff-05404998a3de", "Arrival Container Yard"), arrivalYard));
					}

					var departureYard = container.DepartureContainerYardAddress != null ? Factory.Load<OrgHeader>(container.DepartureContainerYardAddress.OA_OH) : null;
					if (departureYard != null)
					{
						parties.Add(new ScreeningParty(this, Res.GetString("f6a1ffda-05b1-43f4-b7e2-2b7f45fdce77", "Departure Container Yard"), departureYard));
					}
				}

				if( Booking?.JS_TH_OneTimeQuote != ZGuid.Empty )
				{
					parties.AddRange(((IGlobalCommercialInvoiceProvider)this).DataProvider.Parties);
				}

				return parties;
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Screening party strings")]
		IEnumerable<IComplianceLocation> IComplianceLocationRiskStatusProvider.Locations
		{
			get
			{
				FetchForLocationsComplianceSynchronization();

				var countries = new List<IComplianceLocation>();

				AddCountryToList("Origin Country", OriginUNLOCO?.Country);
				AddCountryToList("Destination Country", DestinationUNLOCO?.Country);
				AddCountryToList("Planned Load Country", LoadPortUNLOCO?.Country);
				AddCountryToList("Planned Discharge Country", DischargePortUNLOCO?.Country);
				AddCountryToList("Via Country", Quote?.CurrentOneOffQuote?.ViaLocation?.Country);

				if (Sailing != null)
				{
					AddCountryToList("Sailing Load Country", Sailing.PortOfLoading?.Country);
					AddCountryToList("Sailing Discharge Country", Sailing.PortOfDischarge?.Country);
				}

				foreach (ScreeningParty party in ((ICompliancePartyRiskStatusProvider)this).Parties)
				{
					RefCountry country = null;

					if (party.DocAddress?.Address != null)
					{
						if (party.DocAddress.E2_AddressOverride)
						{
							country = party.DocAddress.Country;
						}
						else
						{
							country = party.DocAddress.Address.Country;
						}
					}
					else if (party.Header != null)
					{
						country = party.Header.Country;
					}
					else if (!string.IsNullOrEmpty(party.OrgCode))
					{
						var org = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, party.OrgCode));
						country = org.Country;
					}

					AddCountryToList(party.Description, country);
				}

				countries.AddRange(((IGlobalCommercialInvoiceProvider)this).DataProvider.Locations);

				return countries;

				void AddCountryToList(string description, RefCountry country)
				{
					if (country != null)
					{
						countries.Add(new ScreeningParty(this, description, country));
					}
				}
			}
		}

		IEnumerable<IComplianceCommodity> IComplianceCommodityRiskStatusProvider.Commodities
		{
			get
			{
				if (Booking == null)
				{
					return Enumerable.Empty<IComplianceCommodity>();
				}

				var commodities = new List<IComplianceCommodity>();
				var lines = Booking.OuterPackLines.OfType<ForwardingPackLine>();

				foreach (var line in lines)
				{
					if (!line.JL_HarmonisedCode.IsEmpty
						&& line.Shipment != null && line.Shipment.JS_UniqueConsignRef == Booking.JS_UniqueConsignRef)
					{
						commodities.Add(new ComplianceCommodity(line.JL_HarmonisedCode, WorldCustomsOrganisationWCO, Booking.JS_UniqueConsignRef, ((IComplianceItemRiskStatusProvider)this).ParentID, line.JL_RN_NKOrigin, Res.GetString("51581A51-2B3A-4D36-B7B7-37697D92FDBC", "Packing"), line.JL_Description));
					}
				}

				commodities.AddRange(((IGlobalCommercialInvoiceProvider)this).DataProvider.Commodities);

				return commodities;
			}
		}

		bool hasFetchedForPartiesComplianceSynchronization;

		void FetchForPartiesComplianceSynchronization()
		{
			if (!hasFetchedForPartiesComplianceSynchronization)
			{
				hasFetchedForPartiesComplianceSynchronization = true;

				var orgAddressPKs = new HashSet<ZGuid>();
				var orgPKs = new HashSet<ZGuid>();

				if (ClientAddrPK_ZAddress != null)
				{
					orgPKs.Add(ClientAddrPK_ZAddress.OrgPK);
				}

				if (Booking != null)
				{
					Factory.AddFetchHint(typeof(JobSailing), Booking.JS_JX);
				}

				if (Job != null && !Job.IsDeleted)
				{
					orgAddressPKs.Add(Job.JH_OA_LocalChargesAddr);
					orgAddressPKs.Add(Job.JH_OA_AgentCollectAddr);
				}

				foreach (var container in QuotedBookingContainers.OfType<ForwardingContainer>())
				{
					orgAddressPKs.Add(container.JC_OA_ArrivalContainerYardAddress);
					orgAddressPKs.Add(container.JC_OA_DepartureContainerYardAddress);
				}

				// Load all addresses in one go
				foreach (var orgAddressPK in orgAddressPKs)
				{
					Factory.AddFetchHint(typeof(OrgAddress), orgAddressPK);
				}

				foreach (var orgAddressPK in orgAddressPKs)
				{
					var orgAddress = Factory.Load<OrgAddress>(orgAddressPK);
					if (orgAddress != null)
					{
						orgPKs.Add(orgAddress.OA_OH);
					}
				}

				// load all org headers in one go
				foreach (var orgPK in orgPKs)
				{
					Factory.AddFetchHint(typeof(OrgHeader), orgPK);
				}
			}
		}

		bool hasFetchedForLocationsComplianceSynchronization;

		void FetchForLocationsComplianceSynchronization()
		{
			if (!hasFetchedForLocationsComplianceSynchronization)
			{
				hasFetchedForLocationsComplianceSynchronization = true;

				var countryCodes = new HashSet<ZString>();

				if (OriginUNLOCO != null)
				{
					countryCodes.Add(OriginUNLOCO.RL_RN_NKCountryCode);
				}

				if (DestinationUNLOCO != null)
				{
					countryCodes.Add(DestinationUNLOCO.RL_RN_NKCountryCode);
				}

				if (LoadPortUNLOCO != null)
				{
					countryCodes.Add(LoadPortUNLOCO.RL_RN_NKCountryCode);
				}

				if (DischargePortUNLOCO != null)
				{
					countryCodes.Add(DischargePortUNLOCO.RL_RN_NKCountryCode);
				}

				if (Quote?.CurrentOneOffQuote?.ViaLocation != null)
				{
					countryCodes.Add(Quote.CurrentOneOffQuote.ViaLocation.RL_RN_NKCountryCode);
				}

				if (Sailing != null)
				{
					if (Sailing.PortOfLoading != null)
					{
						countryCodes.Add(Sailing.PortOfLoading.RL_RN_NKCountryCode);
					}

					if (Sailing.PortOfDischarge != null)
					{
						countryCodes.Add(Sailing.PortOfDischarge.RL_RN_NKCountryCode);
					}
				}

				foreach (ScreeningParty party in ((ICompliancePartyRiskStatusProvider)this).Parties)
				{
					if (party.DocAddress?.Address != null)
					{
						if (party.DocAddress.E2_AddressOverride)
						{
							countryCodes.Add(party.DocAddress.E2_RN_NKCountryCode);
						}
						else
						{
							Factory.AddFetchHint(typeof(RefCountry), new ZQuery(RefCountrySchema.RN_Code, party.DocAddress.Address.OA_RN_NKCountryCode));
							countryCodes.Add(party.DocAddress.Address.OA_RN_NKCountryCode);
						}
					}
					else if (party.Header?.ClosestPort != null)
					{
						countryCodes.Add(party.Header.ClosestPort.RL_RN_NKCountryCode);
					}
				}

				foreach (var countryCode in countryCodes)
				{
					Factory.AddFetchHint(typeof(RefCountry), new ZQuery(RefCountrySchema.RN_Code, countryCode));
				}
			}
		}

		ZDateTime IComplianceCommodityRiskStatusProvider.EffectiveDate
		{
			get
			{
				var result = Booking?.JS_E_DEP;

				if (result == null || !result.Value.IsValid)
				{
					result = ZDateTime.Now;
				}

				return (ZDateTime)result;
			}
		}

		ZBool IComplianceCommodityRiskStatusProvider.IsEditingCommoditySupported => ZBool.True;

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Movement description constant strings")]
		ComplianceAssessmentPointPairInfo IComplianceCommodityRiskStatusProvider.AssessmentPointPairInfo
		{
			get
			{
				var pointPairs = new[] {
					new ComplianceCheckRequestPointPair
					{
						OriginPoint = new ComplianceCheckRequestPointPairLocation
						{
							Country = OriginUNLOCO?.RL_RN_NKCountryCode,
							UNLOCO = OriginUNLOCO?.RL_Code,
							MovementDescription = "Origin"
						},
						DestinationPoint = new ComplianceCheckRequestPointPairLocation
						{
							Country = DestinationUNLOCO?.RL_RN_NKCountryCode,
							UNLOCO = DestinationUNLOCO?.RL_Code,
							MovementDescription = "Destination"
						},
						EstimatedTimeOfArrival = ETA,
						EstimatedTimeOfDeparture = ETD,
						Mode = TransportMode
					}
				};

				return new ComplianceAssessmentPointPairInfo(pointPairs);
			}
		}

		ZGuid IComplianceItemRiskStatusProvider.ParentID => JobParentForCompliance.PK;

		ZString IComplianceItemRiskStatusProvider.ParentTableCode => JobParentForCompliance.TablePrefix();

		// Separated from JobParent because ParentTableCode requires Booking for 'QuickBooking' in 'ComplianceRiskStatusSupporter.FetchCommodityDetailInDB' and 'AddComplianceDocumentHoldStatusEventLog'."
		IJobHeaderParent JobParentForCompliance => UseJobFromBooking ? Booking : this;

		ComplianceRiskSupport IComplianceItemRiskStatusProvider.ComplianceRiskSupport { get; } = ComplianceRiskSupport.SupportInitialization;

		Func<DocumentDeliveryResultForComplianceWorkflow> IComplianceItemRiskStatusProvider.InitializeComplianceWorkflowPopupIfNeeded { get; set; }

		IEnumerable<IComplianceItemRiskStatusProvider> IComplianceItemRiskStatusProvider.SubComplianceRiskStatusProviders => Enumerable.Empty<IComplianceItemRiskStatusProvider>();

		IEnumerable<IComplianceItemRiskStatusProvider> IComplianceItemRiskStatusProvider.ParentComplianceRiskStatusProviders => Enumerable.Empty<IComplianceItemRiskStatusProvider>();

		(ZBool IsCurrent, ZDateTime JobEndDate) IComplianceItemRiskStatusProvider.JobTime => ComplianceRiskHelper.GetJobEndDateAndIsCurrent(Job?.JH_Status, Booking?.JS_E_DEP ?? ZDateTime.Empty,
			Booking?.JS_E_ARV ?? ZDateTime.Empty, this, Booking?.Transports.Select(u => new ComplianceRouting { ETD = u.JW_ETD, ETA = u.JW_ETA, ATD = u.JW_ATD, ATA = u.JW_ATA }));

		ZBool IComplianceItemRiskStatusProvider.IsEnabledComplianceWise => Booking != null && ComplianceRiskHelper.IsFreightEnabledComplianceWise && !IsTemplate;

		CommodityRiskCalculateFactor IComplianceCommodityRiskStatusProvider.RiskCalculateFactor => CommodityRiskCalculateFactor.All;

		SecurityCheckpoint IComplianceItemRiskStatusProvider.AllowOverrideOverallRiskStatusSecurity => Env.Security.BookingsComplianceAllowOverrideOverallRiskStatus;

		SecurityCheckpoint IComplianceItemRiskStatusProvider.AllowResynchronizeRiskStatusSecurity => Env.Security.BookingsComplianceAllowResynchronizeRiskStatus;

		SecurityCheckpoint IComplianceItemRiskStatusProvider.AllowOverrideFreightMovementRestrictionsSecurity => Env.Security.BookingsComplianceAllowOverrideFreightMovementRestrictions;

		SecurityCheckpoint IComplianceCommodityRiskStatusProvider.EditHarmonizedCodeSecurity => Env.Security.BookingsComplianceEditHarmonizedCode;

		SecurityCheckpoint IComplianceCommodityRiskStatusProvider.EditComplianceAssessmentSecurity => Env.Security.BookingsComplianceEditComplianceAssessment;

		SecurityCheckpoint IComplianceCommodityRiskStatusProvider.AllowComplianceAssessmentSecurity => Env.Security.BookingsComplianceAllowComplianceAssessment;

		SecurityCheckpoint IComplianceCommodityRiskStatusProvider.DeclineComplianceAssessmentSecurity => Env.Security.BookingsComplianceDeclineComplianceAssessment;

		#endregion

		#region IScreeningPartyForVessel

		ZString IScreeningPartyForVessel.Code
		{
			get { return Booking?.Sailing?.JX_JV_NKVessel ?? ZString.Empty; }
		}

		ZString IScreeningPartyForVessel.CurrentScreeningStatus
		{
			get { return Booking?.JS_BookedVesselScreeningStatus ?? ZString.Empty; }
		}

		#endregion

		#region ICO2eProvider

		public void SetCO2eStatus(ZString value)
		{
			if (IsForwardRegistered)
			{
				return;
			}
			this.SetCO2eStatus(value, string.Empty);
		}

		void UpdateQuotedBookingCO2eStatusToNotCurrent(CO2eStatusChangedReason reason) => this.UpdateCO2eStatusToNotCurrent(reason, IsCopying || IsForwardRegistered);

		bool ICO2eProvider.RequireTEU
		{
			get
			{
#if DEBUG
				if (OnRequireTEUCalled != null)
				{
					OnRequireTEUCalled(this, EventArgs.Empty);
				}
#endif
				return (this as ICO2eLegBasedSupporter).RequireTEUForTransportMode(TransportMode);
			}
		}

#if DEBUG
		public event EventHandler OnRequireTEUCalled;
#endif

		void ICO2eProvider.RecordLog(CO2eEventType type, string extra, decimal previousCO2eValue) => this.LogGHGEvent(type, extra, previousCO2eValue);

		#endregion

		#region ICO2eParent

		public IJobCO2eCollection JobCO2eCollection
		{
			get
			{
				if (jobCO2eCollection == null)
				{
					jobCO2eCollection = new JobCO2eCollection(this);
					jobCO2eCollection.JobCO2e_StatusChanged += CO2eStatusChanged;
				}
				return jobCO2eCollection;
			}
		}
		IJobCO2eCollection jobCO2eCollection;

		public bool HaveJobCO2e => this.JobCO2eExists();

		public ZGuid JobCO2eParentID => PK;

		public ZString JobCO2eParentTableCode => TablePrefix;

		void ICO2eParent.RefreshCO2e()
		{
			RefreshTotalCO2eBinding();
			RefreshTotalCO2eForSorting();
		}

		void CO2eStatusChanged(object sender, EventArgs e) => RefreshTotalCO2eBinding();

		void RefreshTotalCO2eBinding()
		{
			if (HaveJobCO2e)
			{
				if (!IsValidationSuspended)
				{
					Validation.ValidateTotalCO2eForBinding();
				}
				TotalCO2eForBindingInfo.RefreshBinding();
			}
		}

		void RefreshTotalCO2eForSorting()
		{
			if (!IsValidationSuspended)
			{
				Validation.ValidateTotalCO2eForSorting();
			}
			TotalCO2eForSortingInfo.RefreshBinding();
		}

		#endregion

		#region ICO2eCalculationSupporter members

		ZString ICO2eLegBasedSupporter.TransportMode => TransportMode;

		ZDecimal ICO2eLegBasedSupporter.Weight => Weight;

		ZString ICO2eLegBasedSupporter.UnitOfWeight => string.IsNullOrWhiteSpace(WeightUnit) ? (ZString)Constants.Weight.Kilograms : WeightUnit;

		RefUNLOCO ICO2eLegBasedSupporter.LoadPort
		{
			get
			{
				return ObjectState == QuotedBookingState.QuoteOnly
					? Quote.CurrentOneOffQuote?.ReceivalLocation
					: OriginUNLOCO ?? LoadPortUNLOCO;
			}
		}

		RefUNLOCO ICO2eLegBasedSupporter.DischargePort
		{
			get
			{
				return ObjectState == QuotedBookingState.QuoteOnly
					? Quote.CurrentOneOffQuote?.DeliveryLocation
					: DestinationUNLOCO ?? DischargePortUNLOCO;
			}
		}

		IPrePostCarriageLocation ICO2eLegBasedSupporter.LoadPortForCO2eCalc => new PrePostCarriageLocationWrapper((this as ICO2eLegBasedSupporter).LoadPort?.Code ?? ZString.Empty);

		IPrePostCarriageLocation ICO2eLegBasedSupporter.DischargePortForCO2eCalc => new PrePostCarriageLocationWrapper((this as ICO2eLegBasedSupporter).DischargePort?.Code ?? ZString.Empty);

		IPrePostCarriageLocation ICO2eLegBasedSupporter.AdditionalLoadPortForCO2eCalc => new PrePostCarriageLocationWrapper(LoadPortUNLOCO?.Code ?? ZString.Empty);

		IPrePostCarriageLocation ICO2eLegBasedSupporter.AdditionalDischargePortForCO2eCalc => new PrePostCarriageLocationWrapper(DischargePortUNLOCO?.Code ?? ZString.Empty);

		IPrePostCarriageLocation ICO2eLegBasedSupporter.ViaPortForCO2eCalc => new PrePostCarriageLocationWrapper(Quote?.CurrentOneOffQuote?.ViaLocation?.Code ?? ZString.Empty);

		ZDateTime ICO2eLegBasedSupporter.ETA => ObjectState != QuotedBookingState.QuoteOnly ? ETA : ZDateTime.Empty;

		ZDateTime ICO2eLegBasedSupporter.ETD => ObjectState != QuotedBookingState.QuoteOnly ? ETD : ZDateTime.Empty;

		ZString ICO2eLegBasedSupporter.ContainerMode
		{
			get
			{
				if (GetFromBooking)
				{
					var value = Booking.JS_PackingMode;
					switch (value)
					{
						case Constants.ContainerModes.BuyersConsol:
						case Constants.ContainerModes.ShippersConsol:
							return Constants.ContainerModes.FCL;
						case Constants.ContainerModes.OnBoardCourier:
							return ZString.Empty;
						default:
							return value;
					}
				}
				else if (Quote != null)
				{
					var value = Quote.CurrentOneOffQuote.TT_ContainerMode;
					switch (value)
					{
						case RateMode.SEA:
						case RateMode.RAI:
						case RateMode.ROA:
						case RateMode.FWL:
						case RateMode.COU:
							return ZString.Empty;
						case RateMode.LRO:
							return Constants.ContainerModes.LCL;
						case RateMode.FRO:
						case RateMode.SCN:
						case RateMode.BCN:
							return Constants.ContainerModes.FCL;
						default:
							return value;
					}
				}
				return ZString.Empty;
			}
		}

		List<ICO2eLegProvider> ICO2eLegBasedSupporter.Legs
		{
			get
			{
				return ObjectState == QuotedBookingState.QuoteOnly || Sailing is null ? null : [Sailing];
			}
		}

		bool ICO2eLegBasedSupporter.ShouldPopulateCO2eForLegs => ObjectState != QuotedBookingState.QuoteOnly;

		bool ICO2eLegBasedSupporter.SupportVirtualLegs => ObjectState != QuotedBookingState.QuoteOnly;

		ZDecimal ICO2eLegBasedSupporter.GetNumberOfTEUForLeg(ICO2eLegProvider leg) => (this as ICO2eTEUProvider).NumberOfTEU;

		IList<CO2eEmptyContainer> ICO2eLegBasedSupporter.EmptyContainers => Array.Empty<CO2eEmptyContainer>();

		bool ICO2eLegBasedSupporter.RequiresTemperatureControl
		{
			get
			{
				if (GetFromBooking)
				{
					return QuotedBookingContainers.Cast<ForwardingContainer>().Any(cont => cont.RefContainer != null && cont.RefContainer.RC_ContainerType == Constants.ContainerTypes.Refrigerated)
						|| Booking.OuterPackLines.Cast<ForwardingPackLine>().Any(p => !p.JL_RequiredTemperatureMinimum.IsDefault || !p.JL_RequiredTemperatureMaximum.IsDefault);
				}
				else if (ObjectState == QuotedBookingState.QuoteOnly)
				{
					return Quote.CurrentOneOffQuote.Containers.Cast<RateOneOffContainers>().Any(cont => cont.Container != null && cont.Container.RC_ContainerType == Constants.ContainerTypes.Refrigerated);
				}
				return false;
			}
		}
		bool oldRequiresTemperatureControl;

		ZDecimal ICO2eLegBasedSupporter.GetTotalEmptyContainerEmissions() => 0;

		bool ICO2eLegBasedSupporter.RequireTEUForTransportMode(string transportMode)
		{
			return transportMode switch
			{
				Constants.TransportModes.Sea when HasContainersForTEU
					=> true,

				Constants.TransportModes.Road or Constants.TransportModes.Rail when
					(this as ICO2eLegBasedSupporter).ContainerMode == Constants.ContainerModes.FCL
					&& (this as ICO2eLegBasedSupporter).Weight == 0
					&& ContainerCount != 0
					=> true,

				_ => false,
			};
		}

		bool HasContainersForTEU =>
			(this as ICO2eLegBasedSupporter).ContainerMode == Constants.ContainerModes.FCL
			&& (GetFromBooking
					? QuotedBookingContainers.Cast<ForwardingContainer>().Any(cont => cont.JC_Calc_TEUCount > 0)
					: Quote.CurrentOneOffQuote.Containers.Cast<RateOneOffContainers>().Any(cont => cont.Container != null && cont.Container.RC_TEU * cont.TC_ContainerCount > 0));

		List<string> ICO2eCalculationSupporter.ValidateInputs()
		{
			var reasons = new List<string>();
			ICO2eLegBasedSupporter supporter = this;

			void AddReason(bool condition, string reason)
			{
				if (condition)
				{
					reasons.Add(reason);
				}
			}

			var rightArrowKey = Res.GetString("84cbb925-da3a-4b46-8fce-c2d1df26e86b", ">");

			var reasonPrefix = $"{BusinessObjectName} {rightArrowKey} ";

			AddReason(string.IsNullOrEmpty(supporter.TransportMode), $"{reasonPrefix}{Res.GetString("676406e9-e59b-4842-81f6-e01d7a858f7b", "Transport Mode")}");
			AddReason(supporter.LoadPort is null, $"{reasonPrefix}{Res.GetString("3d673ac4-5f09-4a4d-8185-3609842f2a2b", "Origin")}");
			AddReason(supporter.DischargePort is null, $"{reasonPrefix}{Res.GetString("d320ac9a-3767-429f-8039-c98b0b761f13", "Destination")}");
			AddReason(!supporter.IncludeTEU && supporter.Weight.IsEmpty, $"{reasonPrefix}{Res.GetString("b19546b1-0607-4877-841d-4872eafe294f", "Goods Details > Weight")}");
			return reasons;
		}

		bool ICO2eCalculationSupporter.SaveEmissionsLogToNoteOnCalculated => false;

		void ICO2eCalculationSupporter.OnRequested() => CO2eLegBasedSupporterHelper.OnRequested(this);

		void ICO2eCalculationSupporter.OnRejected(string reason) => CO2eLegBasedSupporterHelper.OnRejected(this, reason);

		void ICO2eCalculationSupporter.OnCalculated(bool succeeded) { }

		AdditionalCalculationSupporter[] ICO2eCalculationSupporter.AdditionalCalculationSupporters => Array.Empty<AdditionalCalculationSupporter>();

		void ICO2eCalculationSupporter.OnAdditionalSupporterCalculated(ICO2eCalculationSupporter additionalSupporter) { }

		#endregion

		#region ICO2eTEUProvider

		bool ICO2eTEUProvider.IncludeTEU => HasContainersForTEU;

		ZDecimal ICO2eTEUProvider.NumberOfTEU => TEUCount;

		ZDecimal ICO2eTEUProvider.TonnesPerTEU => TEUCount == 0 ? 0 : Constants.Weight.ConvertSafe(Weight, WeightUnit, Constants.Weight.Tonnes) / TEUCount;

		ZDecimal ICO2eTEUProvider.ContainerEmptyWeightPerTEU
		{
			get
			{
				var teuCount = ((ICO2eTEUProvider)this).NumberOfTEU;

				if (teuCount == 0)
				{
					return 0;
				}

				return GetFromBooking
					? QuotedBookingContainers.Cast<ForwardingContainer>().Sum(cont => Constants.Weight.Convert(cont.JC_TareWeight + cont.JC_DunnageWeight, cont.ContainerWeightUnit, Constants.Weight.Kilograms)) / teuCount
					: Quote.CurrentOneOffQuote.Containers.Cast<RateOneOffContainers>().Sum(c => c.TC_ContainerCount * c.Container.RC_TareWeight) / teuCount;
			}
		}

		ZString ICO2eTEUProvider.ContainerEmptyWeightPerTEUUnit => Constants.Weight.Kilograms;

		#region Pre/Post Carriage

		bool ICO2ePrePostCarriage.RequiresPrePostCarriageLegs => false;

		IPrePostCarriageLocation[] ICO2ePrePostCarriage.GetPreCarriageLocations(ZString hblDeliveryMode) => Array.Empty<PrePostCarriageLocationWrapper>();

		IPrePostCarriageLocation[] ICO2ePrePostCarriage.GetPostCarriageLocations(ZString hblDeliveryMode) => Array.Empty<PrePostCarriageLocationWrapper>();

		void ICO2ePrePostCarriage.OnTransportBookingCalculated(IDtbBooking dtbBooking) { }

		void ICO2ePrePostCarriage.OnTransportBookingCO2eStatusChanged(IDtbBooking dtbBooking) { }

		void ICO2ePrePostCarriage.OnTransportBookingActiveStatusChanged(IDtbBooking dtbBooking) { }

		#endregion

		#endregion

		#region ICCACommonAssignmentValidationData Members

		ZString ICCACommonAssignmentValidationData.Name => Res.GetString("114ad0ab-0d25-c284-449c-4e465aa5e04d", "Booking");

		IOrgHeader ICCACommonAssignmentValidationData.ContractServiceProvider => Carrier;

		IEnumerable<IForwardingContainer> ICCACommonAssignmentValidationData.Containers => QuotedBookingContainers.Cast<IForwardingContainer>();

		ZString ICCACommonAssignmentValidationData.UniqueConsignRef => ((IQuotedBooking)this).UniqueConsignRef;

		ZString ICCACommonAssignmentValidationData.VoyageFlight => Sailing?.JX_JV_VoyageFlight ?? ZString.Empty;

		ZString ICCACommonAssignmentValidationData.Vessel => Sailing?.JX_JV_NKVessel ?? ZString.Empty;

		IRatingContract ICCACommonAssignmentValidationData.CarrierContract => ContractAllocationHelper.GetCarrierContract(Factory, CarrierContractNumber, OH_Carrier);

		IRatingContractAllocationLine ICCACommonAssignmentValidationData.AllocationRoute => Factory.Load<IRatingContractAllocationLine>(AllocationLinePK);

		ZString ICCACommonAssignmentValidationData.TransportMode => ((IQuotedBooking)this).TransportMode;

		#endregion

		#region CarrierContractAndAllocation

		[MaxLength(CommonShipment.Schema.JS_CarrierContractNumberMaxLength)]
		[List(nameof(Booking) + "." + nameof(ForwardingShipment.Lookups) + "." + nameof(ForwardingShipmentLookups.CarrierContractCollection))]
		[ResourceStringData("ViewQuotedBooking|CarrierContractNumber", Caption = "Carrier Contract Number", ShortCaption = "Contract No.")]
		public ZString CarrierContractNumber
		{
			get => Booking?.JS_CarrierContractNumber ?? ZString.Empty;
			set
			{
				if (Booking == null)
				{
					return;
				}

				if (Booking.JS_CarrierContractNumber != value)
				{
					Booking.JS_CarrierContractNumber = value;
					CarrierContractNumberInfo.RefreshBinding();

					if (value.Length <= CusEntryNumSchema.CE_EntryNum.MaxLength && !IsCopying)
					{
						UpdateOrCreateContractCusEntryNumber(value);
					}

					if (value.IsEmpty)
					{
						ClearAllAllocationRoutes();
					}

					if (!IsValidationSuspended)
					{
						Validation.ValidateCarrierContractNumber();
					}
				}
			}
		}

		void UpdateOrCreateContractCusEntryNumber(ZString carrierContractNumber)
		{
			var contractNumber = Booking.Numbers.Cast<CusEntryNumber>().FirstOrDefault(number => number.CE_EntryType == AdditionalReferenceNumbersCodes.CON);
			if (contractNumber == null)
			{
				contractNumber = Booking.Numbers.AddNew();
				contractNumber.CE_RN_NKCountryCode = ZString.Empty;
				contractNumber.CE_EntryType = AdditionalReferenceNumbersCodes.CON;
			}

			contractNumber.CE_EntryNum = carrierContractNumber;
		}

		public ZPropertyInfo CarrierContractNumberInfo => GetZPropertyInfo(Schema.CarrierContractNumber);

		void ClearAllAllocationRoutes()
		{
			AllocationLinePK = ZGuid.Empty;
			DefaultAllocationRouteOnAllContainers(ZGuid.Empty);
		}

		public IRatingContractAllocationLineCollection AllocationLineCollection
		{
			get
			{
				if (allocationLineCollection == null)
				{
					allocationLineCollection = new FilteredRatingContractAllocationLineCollection(Factory, () => ContractAllocationHelper.GetCarrierContract(Factory, CarrierContractNumber, OH_Carrier)?.PK);
				}

				return allocationLineCollection;
			}
		}

		FilteredRatingContractAllocationLineCollection allocationLineCollection;

		[List(nameof(AllocationLineCollection))]
		[ResourceStringData("ViewQuotedBooking|AllocationLinePK", Caption = "Allocation ID")]
		public ZGuid AllocationLinePK
		{
			get => Booking?.JS_RCA_BookingAllocationLine ?? ZGuid.Empty;
			set
			{
				if (Booking == null)
				{
					return;
				}

				if (Booking.JS_RCA_BookingAllocationLine != value)
				{
					Booking.JS_RCA_BookingAllocationLine = value;
					AllocationLinePKInfo.RefreshBinding();

					TryDefaultFieldsFromAllocationRoute(value);
					DefaultAllocationRouteOnAllContainers(value);

					if (!IsValidationSuspended)
					{
						Validation.ValidateAllocationLinePK();
					}
				}
			}
		}

		public ZPropertyInfo AllocationLinePKInfo => GetZPropertyInfo(Schema.AllocationLinePK);

		void TryDefaultFieldsFromAllocationRoute(ZGuid allocationRoutePK)
		{
			var allocationRoute = Factory.Load<RatingContractAllocationLine>(allocationRoutePK);
			if (allocationRoute == null)
			{
				return;
			}

			if (allocationRoute.RCA_JX_SailingSchedule.IsEmpty)
			{
				if (LoadPort.IsEmpty && LocationIsUNLOCO(allocationRoute.RCA_LoadLocation))
				{
					LoadPort = allocationRoute.RCA_LoadLocation;
				}

				if (DischargePort.IsEmpty && LocationIsUNLOCO(allocationRoute.RCA_DischargeLocation))
				{
					DischargePort = allocationRoute.RCA_DischargeLocation;
				}
			}
			else if (Booking?.JS_JX.IsEmpty ?? false)
			{
				((ISailingChooserParent)this).SailingJX = allocationRoute.RCA_JX_SailingSchedule;
			}
		}

		bool LocationIsUNLOCO(ZString location) => location.Length == 5;

		void DefaultAllocationRouteOnAllContainers(ZGuid allocationRoutePK)
		{
			foreach (var container in QuotedBookingContainers.OfType<ForwardingContainer>())
			{
				container.JC_RCA_AllocationLine = allocationRoutePK;
			}
		}

		#endregion

		#region IAllocationRouteAssignable

		void IAllocationRouteAssignable.UpdateCarrierContractAndAllocationDetails(IRatingContractAllocationLine route)
		{
			CarrierContractNumber = route.Contract?.RCT_ContractNumber ?? ZString.Empty;
			OH_Carrier = route.Contract?.RCT_OH ?? ZGuid.Empty;
			AllocationLinePK = route.PK;
		}

		bool IAllocationRouteAssignable.HasCarrierOrRouteDifferentToOverride(IRatingContractAllocationLine route)
		{
			return (!CarrierContractNumber.IsEmpty && !string.Equals(CarrierContractNumber, route.Contract?.RCT_ContractNumber, StringComparison.OrdinalIgnoreCase))
				|| (!OH_Carrier.IsEmpty && OH_Carrier != route.Contract?.RCT_OH)
				|| (!AllocationLinePK.IsEmpty && AllocationLinePK != route.PK);
		}

		bool IAllocationRouteAssignable.IsAssignedAllocationRoute(IRatingContractAllocationLine route)
		{
			return string.Equals(CarrierContractNumber, route.Contract?.RCT_ContractNumber, StringComparison.OrdinalIgnoreCase)
				&& (OH_Carrier == route.Contract?.RCT_OH)
				&& (AllocationLinePK == route.PK);
		}

		ZString IAllocationRouteAssignable.HumanReadableName => HumanReadableName;
		ZString IAllocationRouteAssignable.CarrierContractNumber => CarrierContractNumber;
		ZString IAllocationRouteAssignable.AllocationRouteID
		{
			get
			{
				var allocationLine = Factory.Load<RatingContractAllocationLine>(AllocationLinePK);
				return allocationLine?.RCA_AllocationLineID ?? string.Empty;
			}
		}
		ZString IAllocationRouteAssignable.ServiceProvider => Carrier?.OH_Code ?? string.Empty;

		#endregion

		#region IChargeCreditorDefaulting

		ZGuid IChargeCreditorDefaulting.DefaultCreditorPK =>
			Creditor != ZGuid.Empty
			? Creditor
			: OH_Carrier;

		#endregion

		#region IConfirmAddressParent

		JobDocAddress IConfirmAddressParent.GetConsigneeDeliveryDocAddress => Booking?.ConsigneeDeliveryAddress;

		JobDocAddress IConfirmAddressParent.GetConsignorPickupDocAddress => Booking?.ConsignorPickupAddress;

		JobDocAddress IConfirmAddressParent.GetArrivalCFSDocAddress => Booking?.GetArrivalCFSDocAddress;

		JobDocAddress IConfirmAddressParent.GetDepartureCFSDocAddress => Booking?.GetDepartureCFSDocAddress;

		JobDocAddress IConfirmAddressParent.GetDepartureCTODocAddress => Booking?.GetDepartureCTODocAddress;

		JobDocAddress IConfirmAddressParent.GetArrivalCTODocAddress => Booking?.GetArrivalCTODocAddress;

		JobDocAddress IConfirmAddressParent.GetDepartureContainerYardDocAddress => Booking?.GetDepartureContainerYardDocAddress;

		JobDocAddress IConfirmAddressParent.GetArrivalContainerYardDocAddress => Booking?.GetArrivalContainerYardDocAddress;

		#endregion

		#region IChargeApplicableForCopy

		ZBool IChargeApplicableForCopy.IsApplicable(ICharge charge)
		{
			var costAccountPk = charge.JR_OH_CostAccount;
			var currentQuote = Quote.CurrentOneOffQuote;

			// If cost account is null or matches the creditor or carrier, the charge is not applicable.
			if (costAccountPk.IsEmpty ||
				costAccountPk == currentQuote?.Creditor?.PK ||
				costAccountPk == currentQuote?.Carrier?.PK)
			{
				return false;
			}

			// If the cost account matches any possible carrier's creditor or carrier, the charge is not applicable.
			if (currentQuote?.PossibleCarriers?.Cast<RateOneOffCarrier>().Any(pc =>
				pc.Creditor?.PK == costAccountPk ||
				pc.Carrier?.PK == costAccountPk) == true)
			{
				return false;
			}

			// In all other cases, the charge is applicable.
			return true;
		}

		#endregion

		#region Global Commercial Invoice

		IGlobalCommercialInvoiceComplianceProvider globalCommercialInvoiceProvider;
		IGlobalCommercialInvoiceComplianceProvider IGlobalCommercialInvoiceProvider.DataProvider => globalCommercialInvoiceProvider ??= ObjectFactory.Get<IGlobalCommercialInvoiceComplianceProcessor>().GetGlobalCommercialInvoiceBusinessObject(this);

		ZGuid IGlobalCommercialInvoiceJobProvider.ParentID => JobParentForCompliance.PK;

		ZString IGlobalCommercialInvoiceJobProvider.ParentTableCode => JobParentForCompliance.TablePrefix();

		ZString IGlobalCommercialInvoiceJobProvider.JobNumber => Booking?.JobNumber ?? ZString.Empty;

		ZString IGlobalCommercialInvoiceJobProvider.VolumeUnitOfMeasurement => Booking?.ShipmentVolumeUnit ?? Env.Registry.FreightVolumeUnit;

		ZString IGlobalCommercialInvoiceJobProvider.WeightUnitOfMeasurement => Booking?.ShipmentWeightUnit ?? Env.Registry.FreightWeightUnit;

		#endregion

		#region ITriggerActionProvider

		ZString ITriggerActionProvider.ReasonForDoNotTriggerAction
		{
			get
			{
				if (IsForwardRegistered)
				{
					return Res.GetString("7774E596-415D-4F8E-9D53-497FB90A9474", "{0} has been converted to Shipment# {1}.", HumanReadableNameCore, Booking.JS_UniqueConsignRef);
				}
				return ZString.Empty;
			}
		}

		#endregion
	}
}
