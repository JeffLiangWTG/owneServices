using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Integration;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.AU;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.Environment;
using Enterprise.Freight.Business.Extensions;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Integration;
using Enterprise.Freight.LocalCartage.Integration;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.Freight;
using Enterprise.Integration.Schedule;
using Enterprise.Integration.ZArchitecture;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.CreditControl.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Metadata.Integration;
using Enterprise.Rating.Integration;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.DataMapping;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using static CargoWise.EventReference.Constants;
using static Enterprise.Freight.Business.ShipmentVsConsolHelper;
using static Enterprise.Freight.Integration.Forwarding;
using static Enterprise.Freight.Integration.IGlobalCommercialInvoiceComplianceProcessor;
using static Enterprise.Integration.Customs;
using BusinessContext = Enterprise.Integration.Accounting.BusinessContext;
using Constants = Enterprise.Core.Constants;
using EnterpriseBusinessObject = Enterprise.ZArchitecture.EnterpriseBusinessObject;
using EventConstants = CargoWise.EventReference.Constants;
using IBaseJobComInvoiceHeader = Enterprise.Integration.Customs.Shared.IBaseJobComInvoiceHeader;
using IBaseJobDeclaration = Enterprise.Integration.Customs.IBaseJobDeclaration;
using ICAManualReleaseNote = Enterprise.Integration.Customs.CA.IManualReleaseNote;
using ICAManualReleaseSupport = Enterprise.Integration.Customs.CA.IManualReleaseSupport;
using IDirectionIsDomesticFreight = Enterprise.Integration.Forwarding.IDirectionIsDomesticFreight;
using INctsHeader = Enterprise.Integration.Customs.EU.NCTS.ICusInBondHeader;
using OrgCodes = Enterprise.MasterFiles.Business.AccountingMasterFilesConstants.OrganisationTypeCodes;

namespace Enterprise.Freight.Business
{
	[CodeProperty(CommonShipment.Schema.JS_UniqueConsignRef), DescriptionProperty(CommonShipment.Schema.JS_HouseBill)]
	[System.Diagnostics.DebuggerDisplay("PK = {PK} NO = {JS_UniqueConsignRef}")]
	[MasterFiles.Business.CustomValues.UserDefinedValues]
	[MasterFiles.Business.CustomValues.SystemDefinedValues]
	[UniversalCopyWithExtendedEntities(FinishCopyMethod = nameof(OnUniversalCopyFinish))]
	[UniversalCopyIgnoreElement("DocsAndCartageDetails",
		JobShipmentSchema.Constants.JS_UniqueConsignRef,
		JobShipmentSchema.Constants.JS_OverrideWaybillDefaults,
		CommonShipment.Schema.JS_AdditionalInspectionTypeCode,
		nameof(ShipmentJobHeader),
		nameof(Job),
		"OriginPickupConfirms",
		"DestinationDeliveryConfirms",
		"OriginCFSArrivalConfirms",
		"OriginCFSDepartureConfirms",
		"DestinationCFSArrivalConfirms",
		"DestinationCFSDepartureConfirms",
		"HVLVItemRunningTotals",
		JobShipmentSchema.Constants.JS_ElectronicBillOfLadingVersion,
		JobShipmentSchema.Constants.JS_ElectronicBillOfLadingType,
		JobShipmentSchema.Constants.JS_ElectronicBillOfLadingTerms,
		JobShipmentSchema.Constants.JS_ElectronicBillOfLadingStatus,
		JobShipmentSchema.Constants.JS_ElectronicBillOfLadingReference,
		JobShipmentSchema.Constants.JS_ElectronicBillOfLadingHouseBill)]
	[UniversalCopyAssociateElement("Addresses", "DocAddresses")]
	[MetadataContext(Enterprise.Metadata.Integration.MetadataContext.CommonShipment)]
	[ProvideMetaDataProperty("DefaultNumberOfDecimals", MetaDataTypes.DecimalPlaces)]
	public partial class CommonShipment :
		AutoJobShipment,
		ICommonShipment,
		ITemplateCopyable,
		IShipmentWithDocsAndCartage,
		ISupportDataImporting,
		IManifestProvider,
		IBillGenerationSupport,
		ICDArchive,
		ILocalShippingLineProvider,
		ILocationConsumer,
		ITransportParent,
		IRoutingSupport,
		ICusEntryNumberSupporter,
		IAdditionalReferenceNumberSupporter,
		IAdditionalReferenceNumberTypeProvider,
		ICusEntryNumberValidationDeciderOfType,
		IJobAddressAdditionalInfoSupport,
		IJobInvoicingPlugIn,
		IJobInvoicingPlugInAdditionalJobs,
		IEDocsProvider,
		ICreditControlledDocumentDelivery,
		IConfirmAddressParent,
		IFlightDetailsSuppression,
		ITemplateReversible,
		IConfirmationsHost,
		IHandleEventsForOtherObjects,
		IRatingSupporterWithAdapter,
		ICusCodeDataTypeSupporter,
		IPhaseSecuritySupportable,
		IDefaultNumberOfDecimalsSupporterWithSchemaColumn,
		IUniversalXMLNoteParent,
		IEventDatePropertyChecker,
		IParentToJobDocsAndCartage,
		IHaveJobHeader,
		IOriginDestinationForDocumentDeliveryRestriction,
		IComplianceJobDirectionProvider,
		ISupportInspectionType,
		ICAManualReleaseSupport,
		IStmNoteParentWithSystemNote,
		IEDIMessageCollectionOwner,
		IDirectionIsDomesticFreight,
		IGlobalCommercialInvoiceJobProvider,
		IGlobalCommercialInvoiceProvider
	{
		public event EventHandler<ValueNotSetEventArgs> ValueNotSet;

		#region Schema

		public new abstract class Schema : AutoJobShipment.Schema
		{
			public const string ConsignorContact = "ConsignorContact";
			public const string ConsigneeContact = "ConsigneeContact";
			public const string NotifyContact = "NotifyContact";

			public const string PickupAgentCompanyCode = "PickupAgentCompanyCode";
			public const string NotifyPartyCompanyCode = "NotifyPartyCompanyCode";
			public const string DetailedGoodsDescriptionNoteText = "DetailedGoodsDescriptionNoteText";

			public const string JS_ChargeableUnit = "JS_ChargeableUnit";
			public const string JS_PaymentTerm = "JS_PaymentTerm";

			public const string JS_Calc_20GPCount = "JS_Calc_20GPCount";
			public const string JS_Calc_40GPCount = "JS_Calc_40GPCount";
			public const string JS_Calc_20RECount = "JS_Calc_20RECount";
			public const string JS_Calc_40RECount = "JS_Calc_40RECount";
			public const string JS_Calc_ContainerCount = "JS_Calc_ContainerCount";
			public const string JS_Calc_OtherContainerCount = "JS_Calc_OtherContainerCount";
			public const string JS_Calc_TEUCount = "JS_Calc_TEUCount";

			public const string JS_Calc_ConsignorCompanyName = "JS_Calc_ConsignorCompanyName";
			public const string JS_Calc_ConsignorCompanyCode = "JS_Calc_ConsignorCompanyCode";
			public const string JS_Calc_ConsigneeCompanyName = "JS_Calc_ConsigneeCompanyName";
			public const string JS_Calc_ConsigneeCompanyCode = "JS_Calc_ConsigneeCompanyCode";

			public const string ColoadMasterShipmentHouseBill = "ColoadMasterShipmentHouseBill";
			public const string CustomsEntryNumber = "CustomsEntryNumber";
			public const string CustomsEntryNumberType = "CustomsEntryNumberType";
			public const string CustomsEntryNumberIssueDate = "CustomsEntryNumberIssueDate";
			public const string CustomsEntryNumberExpiryDate = "CustomsEntryNumberExpiryDate";
			public const string TotalInnerPackLinePackages = "TotalInnerPackLinePackages";
			public const string TotalInnerPackLineVolume = "TotalInnerPackLineVolume";
			public const string TotalInnerPackLineWeight = "TotalInnerPackLineWeight";
			public const string TotalInnerPackLineLoadingMeters = "TotalInnerPackLineLoadingMeters";
			public const string TotalOuterPacks = "TotalOuterPacks";
			public const string TotalOuterPacksUnit = "TotalOuterPacksUnit";
			public const string TotalOuterPacksVolume = "TotalOuterPacksVolume";
			public const string TotalOuterPacksWeight = "TotalOuterPacksWeight";
			public const string TotalOuterPacksLoadingMeters = "TotalOuterPacksLoadingMeters";
			public const string TotalOuterPacksPillaged = "TotalOuterPacksPillaged";
			public const string TotalOuterPacksDamaged = "TotalOuterPacksDamaged";
			public const string TotalPackLineVolumeUnit = "TotalPackLineVolumeUnit";
			public const string TotalPackLineWeightUnit = "TotalPackLineWeightUnit";
			public const string LocationWhsGuid = "LocationWhsGuid";
			public const string LocationString = "LocationString";

			public const string JS_MarksAndNumbers = "JS_MarksAndNumbers";
			public const string JS_MarksAndNumbersShort = "JS_MarksAndNumbersShort";

			public const string JS_Calc_CurrentVessel = "JS_Calc_CurrentVessel";
			public const string JS_Calc_CurrentVoyageFlight = "JS_Calc_CurrentVoyageFlight";
			public const string JS_Calc_CurrentLoadPort = "JS_Calc_CurrentLoadPort";
			public const string JS_Calc_CurrentDischargePort = "JS_Calc_CurrentDischargePort";
			public const string JS_Calc_CurrentETD = "JS_Calc_CurrentETD";
			public const string JS_Calc_CurrentETA = "JS_Calc_CurrentETA";

			public const string JS_Calc_LastDischargePort = "JS_Calc_LastDischargePort";
			public const string JS_Calc_LastETA = "JS_Calc_LastETA";

			public const string JS_JK_ConsolID = "JS_JK_ConsolID";
			public const string JS_JK_VoyageFlight = "JS_JK_VoyageFlight";
			public const string JS_JK_Vessel = "JS_JK_Vessel";
			public const string JS_JK_ReceivingAgent = "JS_JK_ReceivingAgent";
			public const string JS_JK_SendingAgent = "JS_JK_SendingAgent";
			public const string JS_JK_MasterBillNum = "JS_JK_MasterBillNum";

			public const string JS_JS_ColoadMasterShipmentForBinding = "JS_JS_ColoadMasterShipmentForBinding";

			public const string JS_InspectionTypeCode = "JS_InspectionTypeCode";
			public const string JS_AdditionalInspectionTypeCode = "JS_AdditionalInspectionTypeCode";

			public const string IsReceived = "IsReceived";
			public const string IsDomesticFreight = "IsDomesticFreight";

			public const string ConsignorPK = "ConsignorPK";
			public const string ConsigneePK = "ConsigneePK";
			public const string PickupAgentPK = "PickupAgentPK";

			public const string ConsignorNameOrPK = "ConsignorNameOrPK";
			public const string ConsigneeNameOrPK = "ConsigneeNameOrPK";
			public const string ControllingCustomerPK = "ControllingCustomerPK";
			public const string ControllingCustomerNameOrPK = "ControllingCustomerNameOrPK";
			public const string ControllingAgentNameOrPK = "ControllingAgentNameOrPK";
			public const string BookedShippingLinePK = "BookedShippingLinePK";

			public const string ShipmentJobHeaderPK = "ShipmentJobHeaderPK";

			public const string JS_ActualWeightReadOnly = "JS_ActualWeightReadOnly";
			public const string JS_ActualVolumeReadOnly = "JS_ActualVolumeReadOnly";
			public const string TotalOuterPacksWeight_Imperial = "TotalOuterPacksWeight_Imperial";
			public const string TotalOuterPacksVolume_Imperial = "TotalOuterPacksVolume_Imperial";
			public const string JS_ActualWeight_Imperial = "JS_ActualWeight_Imperial";
			public const string JS_ActualVolume_Imperial = "JS_ActualVolume_Imperial";
			public const string JS_Calc_DocumentedWeight_Converted = "JS_Calc_DocumentedWeight_Converted";
			public const string JS_Calc_DocumentedVolume_Converted = "JS_Calc_DocumentedVolume_Converted";

			public const string JS_Calc_ActualVolumeWeight = "JS_Calc_ActualVolumeWeight";
			public const string JS_Calc_ActualVolumeWeightUnit = "JS_Calc_ActualVolumeWeightUnit";
			public const string JS_Calc_ExcessActualVolumeWeight = "JS_Calc_ExcessActualVolumeWeight";
			public const string JS_Calc_ExcessChargeableVolumeWeight = "JS_Calc_ExcessChargeableVolumeWeight";

			public const string OuterPackLines = "OuterPackLines";

			public const string PickupByTransportMode = "PickupByTransportMode";
			public const string CFSDepartureByTransportMode = "CFSDepartureByTransportMode";
			public const string DeliveryByTransportMode = "DeliveryByTransportMode";
			public const string CFSArrivalByTransportMode = "CFSArrivalByTransportMode";
		}

		public const string PreAllocatedHouseBillPrefix = "PPH";

		#endregion

		#region Construction

		public CommonShipment(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
#pragma warning disable
			((IBusinessObjectState)this).UpdatedByDataRefreshIncludingChildren += new EventHandler(Shipment_UpdatedByDataRefreshIncludingChildren);
#pragma warning restore
			OriginalLoginCompanyPK = GlbCompany.CurrentCompany.PK;
		}

		public static CommonShipment New(BusinessObjectFactory factory)
		{
			return factory.New<CommonShipment>();
		}

		#endregion

		#region Type Decider

		public static readonly ShipmentTypeDecider TypeDecider = new ShipmentTypeDecider();

		#endregion

		#region Fetch Strategy

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new ShipmentFetchStrategy(this);
		}

		#endregion

		#region Active Filter

		public static ZQuery ActiveFilter
		{
			get { return new ZQuery(JobShipmentSchema.JS_IsCancelled, SQLComparisonOperator.Equal, ZBool.False); }
		}

		#endregion

		#region Validation

		protected override JobShipmentValidation GetNewValidation()
		{
			return new CommonShipmentValidation(this);
		}

		public new CommonShipmentValidation Validation
		{
			get { return (CommonShipmentValidation)base.Validation; }
		}

		#region Events for validation

		ZBool CanChangeShipmentType(ZString oldValue, ZString newValue)
		{
			bool result = true;
			var handler = ShipmentTypeChanging;

			if (handler != null)
			{
				var args = new ShipmentTypeChangingCancelEventArgs(oldValue, newValue);
				handler(this, args);
				result = !args.Cancel;
			}
			return result;
		}

		public ShipmentTypeChangingCancelEventHandler ShipmentTypeChanging;

		#endregion

		#region ICusEntryNumberValidationDeciderOfType

		Type ICusEntryNumberValidationDeciderOfType.GetCusEntryNumberValidationType()
		{
			return typeof(CommonAdditionalRefEntryNumValidation);
		}

		#endregion

		#endregion

		#region Default Values

		public ZBool IsSettingDefaultValues { get; set; }

		public ZBool IsSettingDefaultValuesForHBLAWBChargesDisplay { get; set; } = true;

		protected override void SetDefaultValues()
		{
			IsSettingDefaultValues = true;
			try
			{
				base.SetDefaultValues();

				if (RegistryServiceLevel != null)
				{
					JS_RS_NKServiceLevel = RegistryServiceLevel.RS_Code;
				}
				JS_UnitOfWeight = DefaultWeightUnit;
				JS_UnitOfVolume = DefaultVolumeUnit;
				JS_F3_NKTotalCountPackType = FreightPacksDataRegistry.Instance.InnerPackUnit.Value;
				JS_F3_NKPackType = FreightPacksDataRegistry.Instance.OuterPackUnit.Value;
				JS_RX_NKGoodsValueCurr = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

				JS_TransportMode = GlbDepartment.CurrentDepartment.TransportMode;
				IsDomesticFreight = GlbDepartment.CurrentDepartment.GE_Domestic;
				JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;

				JS_INCO = "";

				JS_ShippedOnBoard = FreightConstants.ShippedOnBoardType.Shipped;
				JS_ReleaseType = DefaultReleaseType;

				JS_HouseBillOfLadingType = Lookups.JS_HouseBillOfLadingTypeDefault;

				if (GlbDepartment.CurrentDepartment.GE_Export)
				{
					JS_RL_NKOrigin = FreightDefaultPortHelper.GetDefaultShipmentOriginPort(GlbBranch.CurrentBranch, JS_TransportMode, JS_PackingMode);
				}
				else if (GlbDepartment.CurrentDepartment.GE_Import)
				{
					JS_RL_NKDestination = FreightDefaultPortHelper.GetDefaultShipmentDestinationPort(GlbBranch.CurrentBranch, JS_TransportMode, JS_PackingMode);
				}
				else if (GlbDepartment.CurrentDepartment.GE_Domestic)
				{
					JS_RL_NKOrigin = GlbBranch.CurrentBranch.HomePort.RL_RN_NKCountryCode;
					JS_RL_NKDestination = GlbBranch.CurrentBranch.HomePort.RL_RN_NKCountryCode;
				}

				JS_OverrideWaybillDefaults = false;
				JS_IsForwardRegistered = true;

				JS_FreightSpotRateAutoratingMode = Constants.FreightRateAutoratingModes.Code.StandardRate;
				JS_FreightCostRateAutoratingMode = Constants.FreightRateAutoratingModes.Code.StandardRate;
				JS_FreightGatewaySellRateAutoratingMode = Constants.FreightRateAutoratingModes.Code.StandardRate;
			}
			finally
			{
				IsSettingDefaultValues = false;
			}
		}

		protected virtual ZString DefaultWeightUnit
		{
			get { return Env.Registry.FreightWeightUnit; }
		}

		protected virtual ZString DefaultVolumeUnit
		{
			get { return Env.Registry.FreightVolumeUnit; }
		}

		public override void OnLoaded()
		{
			base.OnLoaded();
			using (SuspendSettingHasChanges())
			{
				if (JS_IsCancelled)
				{
					UpdateReadOnlyForWhenCancelled();
				}
			}
		}

		#endregion

		#region Saving

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		protected override ZString HumanReadableNameCore
		{
			get
			{
				string result = HumanReadableNameWithoutID;

				if (!JS_UniqueConsignRef.IsEmpty)
				{
					result += " " + JS_UniqueConsignRef;
				}
				if (!JS_HouseBill.IsEmpty)
				{
					result += " " + Res.GetString("79e6928f-42ca-47fc-843e-64ebf384194a", "(House Bill='{0}')", JS_HouseBill);
				}

				return result;
			}
		}

		public ZString HumanReadableNameWithoutID => HumanReadableNameWithoutIDCore;

		protected virtual ZString HumanReadableNameWithoutIDCore
		{
			get { return Res.GetString("c6a03b66-67ed-46e0-ae32-9d70897c1233", "Shipment"); }
		}

		protected override ZString HumanReadableShortcutNameCore
		{
			get
			{
				var result = string.Empty;

				if (!JS_UniqueConsignRef.IsEmpty)
				{
					result += JS_UniqueConsignRef;
				}

				if (Job != null && Job.LocalCharges != null)
				{
					result += " - " + Job.LocalCharges.OH_FullNameTruncated;
				}
				else
				{
					if (ConsignorDocumentaryAddress != null && !ConsignorDocumentaryAddress.E2_CompanyName.IsEmpty)
					{
						result += " - " + ConsignorDocumentaryAddress.E2_CompanyNameTruncated;
					}

					if (ConsigneeDocumentaryAddress != null && !ConsigneeDocumentaryAddress.E2_CompanyName.IsEmpty)
					{
						result += " - " + ConsigneeDocumentaryAddress.E2_CompanyNameTruncated;
					}
				}

				return result;
			}
		}

		public void PreLogAllDocumentsReceivedEvents()
		{
			if (!IsInDatabase && DefaultRequiredDocuments && !requiredDocumentsAdded)
			{
				AddRequiredDocuments();
				requiredDocumentsAdded = true;
			}
		}

		bool requiredDocumentsAdded;

		public override void OnSaving()
		{
			DeactivateJobHeaderWhenIsCancelled();
			base.OnSaving();

			PopulateBillAndShipmentNumberIfNeeded();
			PopulatePackLineIdsIfNeeded();

			if (CoLoadMasterShipment != null && !CoLoadMasterShipment.IsInDatabase)
			{
				Logs.UpdateEventReferenceNumbers(Events.Attached, CoLoadMasterShipment.PK.ToString(), CoLoadMasterShipment.LogReference(false));
			}

			ConvertWebCreatedInspectionType();

			if (OnSavingShipment != null)
			{
				OnSavingShipment(this, EventArgs.Empty);
			}

			if (JS_PackingModeInfo.HasChanges && IsDocsAndCartageSet)
			{
				//As ALL validation on the confirm may be suppressed, invalid confirms must be deleted before transaction core begins
				DeleteIncompatiblePickupDeliveryConfirms();
			}

			foreach (var info in StrictBizPropertyInfos)
			{
				ConcurrencyInfo.SetConcurrencyPolicy(this, info.Name, IsInDatabase ? ConcurrencyPolicy.Strict : ConcurrencyPolicy.Default);
			}

			if (Consols.Cast<CommonConsol>().Any(c => c.JK_IsCFS && c.JK_IsForwarding) && JS_IsForwardRegistered && !JS_IsCFSRegistered)
			{
				JS_IsCFSRegistered = true;
			}

			if (AviationSecurity.IsAviationSecurityApplicableForTransportMode && AviationSecurity.SupplyChainSecurityConfiguration.IsExportForAviationSecurityPurposes(this as ISupplyChainSecurityImportExportSupporter) && AviationSecurity.ReasonForAviationSecurityNotBeingAvailable.IsEmpty)
			{
				if (InspectionTypeCusEntryNumber != null && InspectionTypeCusEntryNumber.CE_EntryNum.IsEmpty && !string.IsNullOrEmpty(InspectionTypeDebugLog))
				{
					ErrorReporter.ReportOnce("CE_EntryNum Empty", InspectionTypeDebugLog);
				}
			}
		}

		void DeactivateJobHeaderWhenIsCancelled()
		{
			if (!this.HasContext(BusinessContext.InvoicingPlugInGUI) && IsCancelled && IsCancelledHasChanged)
			{
				JobHeader.DeactivateAllJobs(this, true);
			}
		}

		void AddRequiredDocuments()
		{
			OrgSupplierBuyerLink orgConsignorConsigneeLink = OrgSupplierBuyerLink.GetExistingOrgSupplierBuyerLink(Consignor, Consignee, JS_RL_NKDestination.Left(2));
			DocsAndCartage.RequiredDocuments.AddAndAcquitRequiredDocuments(orgConsignorConsigneeLink, null, null, null, JS_E_DEP);

			if (!this.IsExport())
			{
				DocsAndCartage.RequiredDocuments.AddAndAcquitRequiredDocuments(Consignee, JS_RL_NKOrigin, JS_RL_NKDestination, Consignee, JS_E_DEP);
				DocsAndCartage.RequiredDocuments.AddAndAcquitRequiredDocuments(Consignor, JS_RL_NKOrigin, JS_RL_NKDestination, Consignor, JS_E_DEP);
			}
			else
			{
				DocsAndCartage.RequiredDocuments.AddAndAcquitRequiredDocuments(Consignor, JS_RL_NKOrigin, JS_RL_NKDestination, Consignor, JS_E_DEP);
				DocsAndCartage.RequiredDocuments.AddAndAcquitRequiredDocuments(Consignee, JS_RL_NKOrigin, JS_RL_NKDestination, Consignee, JS_E_DEP);
			}

			JobRequiredDocumentDependentCollection.DirectionFilterType directionType = (IsDomesticFreight) ? JobRequiredDocumentDependentCollection.DirectionFilterType.Domestic : JobRequiredDocumentDependentCollection.DirectionFilterType.Both;
			DocsAndCartage.RequiredDocuments.AddCountryRequiredDocuments(RefCountryRequiredDocumentSchema.RD_OnShipment, JS_TransportMode, JS_PackingMode, directionType, JS_RL_NKOrigin, JS_RL_NKDestination);
		}

		protected virtual bool DefaultRequiredDocuments
		{
			get { return true; }
		}

		public event EventHandler OnSavingShipment;

		protected bool InAddDateEvent { get; private set; }

		void AddDateEvent(ZPropertyInfo info, Event @event)
		{
			try
			{
				InAddDateEvent = true;
				var oldValue = (ZDateTime)info.OriginalValue;
				var newValue = (ZDateTime)info.Value;

				var reference = (!info.BizObj.IsInDatabase || oldValue.IsEmpty) ? "" : DatePrefixFrom + oldValue.ToShortDateString() + " ";
				var parameters = GetDateEventParameters(@event);

				var unloco = parameters.TryGetValue(EventConstants.EventReferenceParameters.Codes.Location, out var unlocoCode)
					? Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, unlocoCode)
					: null;
				var eventValue = new EventValue(@event, reference: reference + DatePrefixTo + newValue.ToShortDateString(), eventTime: newValue.ToDateTimeOffset(unloco), isEstimate: true, parameters: parameters);
				Logs.CreateRecreateOrUpdateEventLog(eventValue, Logs.MostRecentLogByEventTime(@event, IsOldDateEvent));
			}
			finally
			{
				InAddDateEvent = false;
			}
		}

		protected virtual IDictionary<string, string> GetDateEventParameters(Event eventType)
		{
			var parameters = new Dictionary<string, string>();

			if (eventType == Events.Arrival && !JS_RL_NKDestination.IsEmpty)
			{
				parameters[EventConstants.EventReferenceParameters.Codes.Location] = JS_RL_NKDestination;
			}
			else if (eventType == Events.Departure && !JS_RL_NKOrigin.IsEmpty)
			{
				parameters[EventConstants.EventReferenceParameters.Codes.Location] = JS_RL_NKOrigin;
			}

			return parameters;
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Log Reference")]
		const string DatePrefixFrom = "From: ";
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Log Reference")]
		const string DatePrefixTo = "To: ";

		static readonly Regex SkipMilestoneGuidRegex = new Regex("[0-9A-F]{8}[-](?:[0-9A-F]{4}[-]?){3}[0-9A-F]{12}\\|", RegexOptions.Compiled | RegexOptions.IgnoreCase);

		bool IsOldDateEvent(StmALog log)
		{
			if (log.IsDeleted || !log.SL_IsEstimate)
			{
				return false;
			}
			var reference = SkipMilestoneGuidRegex.Replace(log.SL_Reference, "");
			return string.IsNullOrEmpty(reference) || reference.StartsWith(DatePrefixFrom, StringComparison.OrdinalIgnoreCase) || reference.StartsWith(DatePrefixTo, StringComparison.OrdinalIgnoreCase);
		}

		void PopulatePackLineIdsIfNeeded()
		{
			if (!JS_IsCancelled)
			{
				PackLine.PopulatePackLineIdIfNeeded(Factory, OuterPackLines.Cast<PackLine>().ToArray());
			}
		}

		public void PopulateBillAndShipmentNumberIfNeeded()
		{
			if (!IsDeleted)
			{
				PopulateBillAndShipmentNumberIfNeededCore();
			}
		}

		protected virtual bool NeedsHouseBill
		{
			get
			{
				return IsHighVolumeLowValue ||
					(IsDirectionForHouseBillGeneration(Directions.Export) || IsDirectionForHouseBillGeneration(Directions.Domestic)) && !IsDirectShipment && BookingNeedsHouseBill;
			}
		}

		public virtual bool IsPendingAllocationSetBySystem { get; set; }

		bool BookingNeedsHouseBill
		{
			get { return !JS_IsBooking || !JS_IsDirectBooking; }
		}

		bool IsDirectionForHouseBillGeneration(Directions direction)
		{
			return direction == ImportExportHelper.GetJobDirection(JS_RL_NKOrigin, JS_RL_NKDestination, GlbBranch.CurrentBranch);
		}

		protected IDisposable EnableShipmentNumberRegeneration() => new DisposableAction(() => isShipmentNumberRegenerationEnabledCount++, () => isShipmentNumberRegenerationEnabledCount--);

		int isShipmentNumberRegenerationEnabledCount;

		bool IsShipmentNumberRegenerationEnabled => isShipmentNumberRegenerationEnabledCount > 0;

		protected virtual void PopulateBillAndShipmentNumberIfNeededCore()
		{
			// Direct shipments get their house bill number from the single Shipment
			if (SuppressBillNumberGeneration)
			{
				PopulateFormattedNumberPropertyIfRequired(JS_UniqueConsignRefInfo, NumberFountainForUniqueConsignRef, true, IsShipmentNumberRegenerationEnabled);
				consignRefHandler = new UniqueIndexHandler(NumberFountainForUniqueConsignRef);

				if (NeedsHouseBill && JS_HouseBill.IsEmpty)
				{
					SetGeneratedHouseBill(JS_UniqueConsignRef);
				}
			}
			else
			{
				PopulateNumberPropertyIfRequired(JS_UniqueConsignRefInfo, GetShipmentTargetID, true, IsShipmentNumberRegenerationEnabled);
			}
		}

		ZString GetShipmentTargetID(BusinessObjectFactory factory)
		{
			NumberGeneratorTarget shipmentTarget = NewShipmentNumberGeneratorTarget();
			NumberGeneratorTarget billTarget = null;

			NumberGenerator generator = new NumberGenerator();
			generator.Factory = factory;
			generator.Context = NewBillOfLadingGeneratorContext();
			generator.BaseFountain = NumberFountainForUniqueConsignRef;
			generator.FountainGetter = GetGeneratorFountain;
			generator.PrimaryTarget = shipmentTarget;
			generator.ValueProviders.AddRange(new StandardValueSource());
			generator.ValueProviders.AddRange(new FreightValueSource(this));
			generator.TargetBO = this;

			if (NeedsHouseBill && (JS_HouseBill.IsEmpty || JS_HouseBill.EqualsIgnoringCase($"Pending Allocation..")))
			{
				billTarget = NewBillOfLadingNumberGeneratorTarget();
				generator.AdditionalTargets.Add(billTarget);
			}

			GenerateNumbers(generator);

			consignRefHandler = new UniqueIndexHandler(shipmentTarget);

			if (billTarget != null)
			{
				SetGeneratedHouseBill(billTarget.Value);
			}

			return shipmentTarget.Value.ToUpper();
		}

		#region SetGeneratedHouseBill (and Log)

		protected void SetGeneratedHouseBill(ZString houseBill)
		{
			SuspendHouseBillValidation = true;
			JS_HouseBill = houseBill;
			HouseBillWasGenerated = true;

			var reference = string.Format((NoResString)"House Bill number '{0}' has been generated.", JS_HouseBill);
			generatedHouseBillLog = Logs.CreateRecreateOrUpdateEventLog(Events.EditedARecord, EstimateActual.Actual, ZDateTimeOffset.Now, reference);
		}

		StmALog generatedHouseBillLog;

		protected bool SuspendHouseBillValidation { get; set; }

		bool HouseBillWasGenerated { get; set; }

		#endregion

		protected virtual void GenerateNumbers(NumberGenerator generator)
		{
			generator.Generate();
			generator.EnforceMaxLengths();
		}

		public bool IsDirectShipment
		{
			get { return ContainsDirectConsol() && CanBeDirect(); }
		}

		public ZBool ContainsDirectConsol()
		{
			return Consols.Any((c) => ((CommonConsol)c).IsDirect);
		}

		public ZBool CanBeDirect()
		{
			return ((IsStandardHouse || IsHighVolumeLowValue) && (CoLoadMasterShipment == null || CoLoadMasterShipment.IsAssemblyMaster))
				|| (IsAssemblyMaster && CoLoadMasterShipment == null);
		}

		public override void OnSaved(bool hasSaveSucceeded)
		{
			if (!hasSaveSucceeded)
			{
				if (!IsInDatabase && !Env.Registry.AllowManualShipmentEntry)
				{
					foreach (CommonShipment subShipment in CoLoadShipments)
					{
						subShipment.Logs.UpdateEventReferenceNumbers(Events.Attached, JS_UniqueConsignRef, PK.ToString(), deferFiringWorkflow: true);
					}

					JS_UniqueConsignRef = ZString.Empty;

					if (HouseBillWasGenerated)
					{
						JS_HouseBill = ZString.Empty;
					}
				}

				if (HouseBillWasGenerated && IsInDatabase)
				{
					JS_HouseBill = JS_HouseBillInfo.OriginalValue.ToString();
				}

				if (generatedHouseBillLog != null && !generatedHouseBillLog.IsInDatabase)
				{
					generatedHouseBillLog.Delete();
					generatedHouseBillLog = null;
				}
			}
			else
			{
				JS_RS_NKGatewayServiceLevel_InitValue = JS_RS_NKGatewayServiceLevel;
			}

			OnShipmentSaved?.Invoke(this, new SavedEventArgs(hasSaveSucceeded));

			base.OnSaved(hasSaveSucceeded);
		}

		public event EventHandler OnShipmentSaved;

		public class SavedEventArgs : EventArgs
		{
			public SavedEventArgs(bool hasSaveSucceeded) : base()
			{
				HasSaveSucceeded = hasSaveSucceeded;
			}

			public bool HasSaveSucceeded;
		}

		#endregion

		#region Delete

		public override void Delete()
		{
			if (!IsDeleting)
			{
				if (IsRoot && !IsInDatabase)
				{
					ErrorReporter.ReportOnce("This new Shipment is not removed from shipment form. Please inform IL team. WI00058157");
				}

				if (!IsDeleted && IsInDatabase && (JS_IsForwardRegistered || JS_IsCFSRegistered))
				{
					throw new CannotDeleteException("This Shipment has been consolidated and cannot be removed.");
				}

				InnerPackLines.RemoveAndDeleteAll();
				CoLoadShipments.RemoveAll();
				OuterPackLines.RemoveAndDeleteAll();
				(JobAddressAdditionalInfoCollection as JobAddressAdditionalInfoCollection).DeleteAll();

				if (inspectionTypeCusEntryNumbersForAllCountries != null)
				{
					inspectionTypeCusEntryNumbersForAllCountries.CountChanged -= CusEntryNumbersCollectionChanged;

					foreach (CusEntryNumber cusEntryNumber in inspectionTypeCusEntryNumbersForAllCountries)
					{
						UnHookEntryNumber(cusEntryNumber);
					}
				}

#pragma warning disable
			((IBusinessObjectState)this).UpdatedByDataRefreshIncludingChildren -= new EventHandler(Shipment_UpdatedByDataRefreshIncludingChildren);
#pragma warning restore

				if (!IsDeleted && ShouldDeleteJobDocsAndCartage())
				{
					DeleteDocsAndCartage();
				}

				PickupPenalties.DeleteAll();
				DeliveryPenalties.DeleteAll();

				if (shipmentJobHeader != null && !shipmentJobHeader.IsInDatabase)
				{
					shipmentJobHeader.Delete();
				}

				try
				{
					base.Delete();
				}
				finally
				{
					if (!IsDeleted)// && !Enterprise.ZArchitecture.Environment.Globals.IsTest)
					{
						ErrorReporter.ReportOnce("WI00216233", string.Format(CultureInfo.InvariantCulture, "Called base delete but Shipment is not marked as deleted: JS_PK: {0}", PK));
					}
				}
			}
		}

		protected virtual bool ShouldDeleteJobDocsAndCartage()
		{
			return Declarations.Length == 0;
		}

		protected void DeleteDocsAndCartage()
		{
			if (IsDocsAndCartageSet)
			{
				UnRegisterEditableChildObject(DocsAndCartage);
			}

			JobDocsAndCartage docsAndCartageIfThere = JobDocsAndCartage.Load(this);
			if (docsAndCartageIfThere != null)
			{
				UnRegisterEditableChildObject(docsAndCartageIfThere);
				docsAndCartageIfThere.Delete();
			}
		}

		#endregion

		#region Circular References

		internal bool HasCircularCoLoadMasterReference()
		{
			List<CommonShipment> encountered = new List<CommonShipment>();

			for (CommonShipment current = this; current != null; current = current.CoLoadMasterShipment)
			{
				if (encountered.Contains(current))
				{
					return true;
				}

				encountered.Add(current);
			}

			return false;
		}

		#endregion

		#region Clone

		public void CopyPersistentValuesFrom(CommonShipment shipment)
		{
			base.CopyPersistentValuesFrom(shipment);
			DocsAndCartage.CopyPersistentValuesFrom(shipment.DocsAndCartage);
			DocsAndCartage.JP_ParentID = this.PK;
		}

		protected bool IsCloning;

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			var result = (CommonShipment)base.CloneInternal(args);
			try
			{
				result.IsCloning = true;

				result.JS_TransportMode = JS_TransportMode;
				result.JS_PackingMode = JS_PackingMode;
				result.JS_ShipmentType = JS_ShipmentType;
				result.JS_RS_NKServiceLevel = JS_RS_NKServiceLevel;

				result.DocsAndCartage.CopyPersistentValuesFrom(DocsAndCartage);
				result.DocsAndCartage.JP_ParentID = result.PK;

				if (!DetailedGoodsDescriptionNoteText.IsEmpty)
				{
					result.DetailedGoodsDescriptionNoteText = DetailedGoodsDescriptionNoteText;
				}

				result.JS_OH_ImportBroker = JS_OH_ImportBroker;
				result.JS_OH_ExportBroker = JS_OH_ExportBroker;

				var argsForCollections = new BusinessObjectCloneArgs(args.AlternativeFactoryToInstantiateCloneIn, Enumerable.Empty<string>(), null, true);

				var jobDocAddressTypesToExcludeFromCopy = GetJobDocAddressTypesToExcludeFromCopy();
				result.DocAddresses.RemoveAndDeleteAll(); // remove the addresses added by set default values.
				foreach (JobDocAddress address in DocAddresses.ToArray())
				{
					if (!jobDocAddressTypesToExcludeFromCopy.Contains(address.E2_AddressType))
					{
						result.DocAddresses.Add(address.Clone(argsForCollections));
					}
				}

				result.InnerPackLines.RemoveAndDeleteAll();
				foreach (PackLine line in this.InnerPackLines)
				{
					PackLine clonedLine = (PackLine)line.Clone(argsForCollections);
					result.InnerPackLines.Add(clonedLine);
				}

				result.OuterPackLines.RemoveAndDeleteAll();
				foreach (PackLine line in this.OuterPackLines)
				{
					PackLine clonedLine = (PackLine)line.Clone(argsForCollections);

					if (JS_IsBooking)
					{
						SetOutturnValuesToDefault(clonedLine);
					}

					result.OuterPackLines.Add(clonedLine);
				}
			}
			finally
			{
				result.IsCloning = false;
			}

			return result;
		}

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		void SetOutturnValuesToDefault(PackLine packline)
		{
			packline.JL_Outturn = 0;
			packline.JL_OutturnedLength = (decimal)0;
			packline.JL_OutturnedHeight = (decimal)0;
			packline.JL_OutturnedWidth = (decimal)0;
			packline.JL_OutturnedVolume = (decimal)0;
			packline.JL_OutturnedWeight = (decimal)0;

			packline.JL_OutturnComment = ZString.Empty;
			packline.JL_MarksAndNumbers = ZString.Empty;
			packline.JL_Damaged = 0;
			packline.JL_Pillaged = 0;
		}

		protected override IEnumerable<string> GetPropertiesToExcludeFromCloning()
		{
			var result = new List<string>(base.GetPropertiesToExcludeFromCloning())
			{
				JobShipmentSchema.Constants.JS_ElectronicBillOfLadingVersion,
				JobShipmentSchema.Constants.JS_ElectronicBillOfLadingType,
				JobShipmentSchema.Constants.JS_ElectronicBillOfLadingTerms,
				JobShipmentSchema.Constants.JS_ElectronicBillOfLadingStatus,
				JobShipmentSchema.Constants.JS_ElectronicBillOfLadingReference,
				JobShipmentSchema.Constants.JS_ElectronicBillOfLadingHouseBill
			};

			return result;
		}

		protected virtual IEnumerable<ZString> GetJobDocAddressTypesToExcludeFromCopy()
		{
			return Array.Empty<ZString>();
		}

		#endregion

		#region Events

		protected override BusinessObject[] BusinessObjectsWithRelatedEventsCore
		{
			get
			{
				var result = new List<BusinessObject>(base.BusinessObjectsWithRelatedEventsCore);
				if (!IsDeleted)
				{
					result.Add(DocsAndCartage);
					result.AddRange(Containers.ToArray());
					result.AddRange(Consols);

					foreach (Transport transport in TransportsIncludingRelated)
					{
						result.Add(transport);

						if (transport.Voyage != null)
						{
							result.Add(transport.Voyage);
						}
					}

					Action<CommonShipment> masterShipmentAdder = null;
					masterShipmentAdder = (s) =>
					{
						if (s.CoLoadMasterShipment != null)
						{
							result.Add(s.CoLoadMasterShipment);
							masterShipmentAdder(s.CoLoadMasterShipment);
						}
					};

					masterShipmentAdder(this);

					var declarations = Declarations.Cast<BusinessObject>().ToArray();   // Caching for perfomance: Declarations calls Factory.Load
					result.AddRange(declarations);
					result.AddRange(GetEntryHeadersForDeclarations(declarations));
					var invoiceHeaders = GetInvoicesForDeclarations(declarations);
					result.AddRange(invoiceHeaders);
					foreach (var invoiceHeader in invoiceHeaders.Cast<EnterpriseBusinessObject>())
					{
						result.AddRange(invoiceHeader.BusinessObjectsWithRelatedEvents);
					}

					if (IsInDatabase)
					{
						InvoiceLoader loader = new InvoiceLoader(Factory);
						result.AddRange(loader.GetInvoicesForUniqueRef(JS_UniqueConsignRef));
					}

					result.AddRange(Numbers);

					JobHeader header = GetShipmentJobHeaderForWorkflow();
					if (header != null)
					{
						result.Add(header);
					}

					result.AddRange(PickupConfirms);
					result.AddRange(DeliveryConfirms);
					result.AddRange(OriginCFSArrivals);
					result.AddRange(OriginCFSDepartures);
					result.AddRange(DestinationCFSArrivals);
					result.AddRange(DestinationCFSDepartures);
				}
				return result.ToArray();
			}
		}

		#endregion

		#region Index Failures

		protected override IEnumerable<IUniqueIndexFailureHandler> UniqueIndexFailureHandlers
		{
			get
			{
				if (fUniqueIndexFailureHandler == null)
				{
					fUniqueIndexFailureHandler = new ShipmentNumberFountainUniqueIndexFailureHandler(this);
				}

				yield return fUniqueIndexFailureHandler;
			}
		}

		IUniqueIndexFailureHandler fUniqueIndexFailureHandler;

		protected class ShipmentNumberFountainUniqueIndexFailureHandler : NumberFountainUniqueIndexFailureHandler
		{
			public ShipmentNumberFountainUniqueIndexFailureHandler(CommonShipment shipment)
				: base(JobShipmentSchema.Constants.Indexes.NR_UC__JS_UniqueConsignRef, shipment)
			{
				this.shipment = shipment;
			}

			protected override INumberFountainProxy NumberFountainToFix
			{
				get { return this.shipment.consignRefHandler != null && this.shipment.consignRefHandler.NumberFountain != null ? this.shipment.consignRefHandler.NumberFountain : this.shipment.NumberFountainForUniqueConsignRef; }
			}

			protected override DbCommand CommandToFindMaxValueInDatabase(DbConnection connection)
			{
				if (!this.shipment.SuppressBillNumberGeneration && shipment.consignRefHandler != null)
				{
					return shipment.consignRefHandler.FindMaxValueInDatabase(connection, JobShipmentSchema.JS_UniqueConsignRef);
				}
				else
				{
					return base.CommandToFindMaxValueInDatabase(connection);
				}
			}

			readonly CommonShipment shipment;
		}

		UniqueIndexHandler consignRefHandler;

		protected virtual INumberFountainProxy NumberFountainForUniqueConsignRef
		{
			get { return Env.NumberFountains.JobShipmentNumber; }
		}

		#endregion

		#region Test Data
#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			if ((kind & TestBusinessObjectKind.PopulateStrings) != 0)
			{
				JS_F3_NKTotalCountPackType = Core.Constants.PkgUnit.BreakBulk;
				JS_F3_NKPackType = Core.Constants.PkgUnit.Bundle;
				JS_UnitOfWeight = Core.Constants.Weight.ShortTons;
				JS_UnitOfVolume = Core.Constants.Volume.CubicFeet;
			}

			if ((kind & TestBusinessObjectKind.PopulateRelatedObjects) != 0)
			{
				OrgHeader consignee = Factory.New<OrgHeader>();
				OrgHeader consignor = Factory.New<OrgHeader>();
				consignee.OH_IsConsignor = true;
				consignee.OH_FullName = "Consignee/Buyer";
				consignor.OH_FullName = "Consignor/Supplier";
				ConsigneePK = consignee.PK;
				ConsignorPK = consignor.PK;

				var pickupCartageCompany = Factory.New<OrgHeader>();
				pickupCartageCompany.Addresses.AddNew(OrgAddressType.Office, true);

				DocsAndCartage.PickupCartageCoPK = pickupCartageCompany.PK;
				DocsAndCartage.PickupCartageCo.OH_FullName = "PickupCartageCompany";

				var deliveryCartageCompany = Factory.New<OrgHeader>();
				deliveryCartageCompany.Addresses.AddNew(OrgAddressType.Office, true);

				DocsAndCartage.DeliveryCartageCoPK = deliveryCartageCompany.PK;
				DocsAndCartage.DeliveryCartageCo.OH_FullName = "DeliveryCartageCompany";
			}

			if ((kind & TestBusinessObjectKind.PopulateAllDependentAndRelatedObjectsDeeply) != 0)
			{
				foreach (var container in OuterPackLines.Cast<PackLine>().SelectMany(p => p.Containers))
				{
					if (!Consols[0].Containers.Contains(container))
					{
						Consols[0].Containers.Add(container);
					}
				}
			}

			JS_HouseBillIssueDate = ZDateTime.Today;
		}

		protected override BusinessObjectTestDataHelper NewBusinessObjectTestDataHelper()
		{
			return new ShipmentTestDataHelper();
		}

		class ShipmentTestDataHelper : BusinessObjectTestDataHelper
		{
			protected override void PopulateFK(ZPropertyInfo fKProperty, TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
			{
				if (fKProperty.Name != JobShipmentSchema.JS_JS_ColoadMasterShipment.Name)
				{
					base.PopulateFK(fKProperty, kind, propertyPath);
				}
			}

			protected override void FillDependentCollectionWithData(IBusinessObjectCollection collection, TestBusinessObjectKind kind, PropertyDescriptor collectionProperty, PropertyDescriptor[] propertyPath)
			{
				// temporary until problem for ManyToMany collection with additional filter is fixed
				if (collectionProperty.Name == "AttachedWarehouseOrders" && collection != null && collection.Count == 0 && collection.AllowNew)
				{
					var whsOrder = (BusinessObject)collection.Factory.New<IWhsOrder>();
					whsOrder.FillWithValidTestData();
					collection.Add(whsOrder);
				}
				base.FillDependentCollectionWithData(collection, kind, collectionProperty, propertyPath);
			}
		}

#endif
		#endregion

		#region RegisterEditableChildObject

		public override void RegisterEditableChildObject(IBusiness child)
		{
			base.RegisterEditableChildObject(child);
			if (!IsDeleted && JS_IsCancelled)
			{
				child.IncrementReadOnlyIncludingChildren();
			}
		}

		protected virtual void RegisterEditableChildObject(IBusiness child, ZString childName)
		{
			RegisterEditableChildObject(child);
		}

		#endregion

		#region Phase Security

		public bool IsReadOnlyDueToPhase
		{
			get { return IsReadOnlyDueToPhaseCore; }
		}

		protected virtual bool IsReadOnlyDueToPhaseCore
		{
			get { return false; }
		}

		public bool IsPropertyReadOnlyDueToPhase(ZString propertyName)
		{
			return IsPropertyReadOnlyDueToPhaseCore(propertyName);
		}

		protected virtual bool IsPropertyReadOnlyDueToPhaseCore(ZString propertyName)
		{
			return false;
		}

		#endregion

		#region Related Business Objects

		#region Addresses

		#region Consignee

		public OrgHeader Consignee
		{
			get { return (ConsigneeDocumentaryAddress == null || !ConsigneeDocumentaryAddress.HasRealOrganisation) ? null : ConsigneeDocumentaryAddress.Organisation; }
		}

		[List("Lookups.Consignee_List")]
		public virtual ZGuid ConsigneePK
		{
			get { return ConsigneeDocumentaryAddress.OrganisationPK; }
			set { ConsigneeDocumentaryAddress.OrganisationPK = value; }
		}

		public ZPropertyInfo ConsigneePKInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(ConsigneePK), x => ConsigneeDocumentaryAddress.OrganisationPKInfo); }
		}

		public virtual JobDocAddress ConsigneeDocumentaryAddress
		{
			get
			{
				if (fConsigneeDocumentaryAddress == null || fConsigneeDocumentaryAddress.IsDeleted)
				{
					UnRegisterListChangedCalledRefreshBinding(fConsigneeDocumentaryAddress);
					fConsigneeDocumentaryAddress = GetNewConsigneeDocumentaryAddress();
					AddEventHandlersToConsigneeDocumentaryAddressEvents();
					RegisterListChangedCalledRefreshBinding(fConsigneeDocumentaryAddress);
				}

				return fConsigneeDocumentaryAddress;
			}
		}

		#region AddEventHandlersToConsigneeDocumentaryAddressEvents

		protected virtual JobDocAddress AddEventHandlersToConsigneeDocumentaryAddressEvents()
		{
			fConsigneeDocumentaryAddress.E2_OA_AddressInfo.ValueChanged -= CneAddressInfoOnValueChanged;
			fConsigneeDocumentaryAddress.OrgAddressBeforeChange -= CneDocAddress_OrgAddressBeforeChange;
			fConsigneeDocumentaryAddress.DocAddressChanged -= CneDocAddress_DocAddressChanged;
			fConsigneeDocumentaryAddress.OrgHeaderAfterChange -= CneDocAddress_OrgHeaderAfterChange;

			fConsigneeDocumentaryAddress.E2_OA_AddressInfo.ValueChanged += CneAddressInfoOnValueChanged;
			fConsigneeDocumentaryAddress.OrgAddressBeforeChange += CneDocAddress_OrgAddressBeforeChange;
			fConsigneeDocumentaryAddress.DocAddressChanged += CneDocAddress_DocAddressChanged;
			fConsigneeDocumentaryAddress.OrgHeaderAfterChange += CneDocAddress_OrgHeaderAfterChange;

			return fConsigneeDocumentaryAddress;
		}

		void CneDocAddress_OrgAddressBeforeChange(object sender, EventArgs e)
		{
			IsChangingConsigneeAddress = true;
		}

		void CneDocAddress_DocAddressChanged(object sender, EventArgs e)
		{
			ConsigneeDocumentaryAddressContentsChanged();
			IsChangingConsigneeAddress = false;
		}

		void CneDocAddress_OrgHeaderAfterChange(object sender, EventArgs e)
		{
			ConsigneeDocumentaryOrgHeaderChanged();
		}

		void CneAddressInfoOnValueChanged(object sender, EventArgs args)
		{
			ConsigneeDocumentaryAddressChanged();
		}

		#endregion

		public bool IsChangingConsigneeAddress { get; private set; }

		void UpdateImportBroker()
		{
			var defaultImportBrokerPK = GetDefaultImportBrokerPK();
			if (defaultImportBrokerPK.HasValue && defaultImportBrokerPK.Value != JS_OH_ImportBroker && Globals.IsUserInteractive && !Factory.IsInSaveTransaction)
			{
				var shouldUpdateBroker = true;

				if (IsInDatabase && OnExportOrImportBrokerUpdate != null)
				{
					var eventArgs = new BrokerDefaultingEventArgs(DocAddressType.ImportBroker);
					OnExportOrImportBrokerUpdate(this, eventArgs);
					shouldUpdateBroker = eventArgs.ShouldUpdateBroker;
				}

				if (shouldUpdateBroker)
				{
					JS_OH_ImportBroker = defaultImportBrokerPK.Value;
				}
			}
		}

		public event EventHandler<BrokerDefaultingEventArgs> OnExportOrImportBrokerUpdate;

		protected virtual JobDocAddress GetNewConsigneeDocumentaryAddress()
		{
			return DocAddresses.FindOrCreateWithRequirement(ConsigneeDocAddressRequirement);
		}

		public JobDocAddress ConsigneeDeliveryAddress
		{
			get
			{
				if (ConsigneeDocumentaryAddress != null && (fConsigneeDeliveryAddress == null || fConsigneeDeliveryAddress.IsDeleted))
				{
					if (fConsigneeDeliveryAddress != null)
					{
						fConsigneeDeliveryAddress.DocAddressChanged -= new EventHandler(fConsigneeDeliveryAddress_DocAddressChanged);
						fConsigneeDeliveryAddress.OrgHeaderAfterChange -= new EventHandler(ReloadNotes);
					}
					fConsigneeDeliveryAddress = GetNewConsigneeDeliveryAddress();
					fConsigneeDeliveryAddress.DocAddressChanged += new EventHandler(fConsigneeDeliveryAddress_DocAddressChanged);
					fConsigneeDeliveryAddress.OrgHeaderAfterChange += new EventHandler(ReloadNotes);
				}

				return fConsigneeDeliveryAddress;
			}
		}

		void ReloadNotes(object o, EventArgs e)
		{
			ReloadNotes();
		}

		void ReloadNotes()
		{
			Notes.ForceReloadRelatedElementsOnNextAccess = true;
		}

		protected virtual JobDocAddress GetNewConsigneeDeliveryAddress()
		{
			return DocAddresses.FindOrCreateWithRequirement(ConsigneePickupDeliveryAddressRequirement);
		}

		void fConsigneeDeliveryAddress_DocAddressChanged(object sender, EventArgs e)
		{
			ConsigneeDeliveryAddressChanged();
		}

		protected virtual void ConsigneeDocumentaryAddressContentsChanged()
		{
		}

		JobDocAddress fConsigneeDocumentaryAddress;
		JobDocAddress fConsigneeDeliveryAddress;

		public ZString ConsigneeContact
		{
			get { return ConsigneeDocumentaryAddress.E2_Contact; }
			set { ConsigneeDocumentaryAddress.E2_Contact = value; }
		}

		public ZPropertyInfo ConsigneeContactInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ConsigneeContact, x => ConsigneeDocumentaryAddress.E2_ContactInfo); }
		}

		public ZString ConsignorContact
		{
			get { return ConsignorDocumentaryAddress.E2_Contact; }
			set { ConsignorDocumentaryAddress.E2_Contact = value; }
		}

		public ZPropertyInfo ConsignorContactInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ConsignorContact, x => ConsignorDocumentaryAddress.E2_ContactInfo); }
		}

		public ZString NotifyContact
		{
			get { return NotifyPartyDocumentaryAddress.E2_Contact; }
			set { NotifyPartyDocumentaryAddress.E2_Contact = value; }
		}

		public ZPropertyInfo NotifyContactInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.NotifyContact, x => NotifyPartyDocumentaryAddress.E2_ContactInfo); }
		}

		[ReadOnly(true)]
		[MaxLength(OrgHeader.Schema.OH_CodeMaxLength)]
		public ZString NotifyPartyCompanyCode
		{
			get
			{
				ZString result = ZString.Empty;
				if (NotifyPartyDocumentaryAddress.HasRealOrganisation)
				{
					result = NotifyPartyDocumentaryAddress.Organisation.OH_Code;
				}
				return result;
			}
		}

		public ZPropertyInfo NotifyPartyCompanyCodeInfo
		{
			get { return GetZPropertyInfo(Schema.NotifyPartyCompanyCode); }
		}

		#region Consignee Code/Company Name (for Grids)

		public ZString JS_Calc_ConsigneeCompanyName
		{
			get { return ConsigneeDocumentaryAddress.E2_CompanyNameTruncated; }
		}

		public ZPropertyInfo JS_Calc_ConsigneeCompanyNameInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(JS_Calc_ConsigneeCompanyName), x => ConsigneeDocumentaryAddress.E2_CompanyNameInfo); }
		}

		[ReadOnly(true)]
		public ZString JS_Calc_ConsigneeCompanyCode
		{
			get
			{
				ZString result = "";
				if (ConsigneeDocumentaryAddress.HasRealOrganisation)
				{
					result = ConsigneeDocumentaryAddress.Organisation.OH_Code;
				}
				else if (!JS_Calc_ConsigneeCompanyName.IsEmpty)
				{
					result = new ZString('(' + JS_Calc_ConsigneeCompanyName.Replace(" ", "") + ')');
				}

				return result;
			}
		}

		public ZPropertyInfo JS_Calc_ConsigneeCompanyCodeInfo
		{
			get
			{
				return GetZPropertyInfo(Schema.JS_Calc_ConsigneeCompanyCode);
			}
		}

		#endregion

		#endregion

		#region Consignor

		public OrgHeader Consignor
		{
			get { return (ConsignorDocumentaryAddress == null || !ConsignorDocumentaryAddress.HasRealOrganisation) ? null : ConsignorDocumentaryAddress.Organisation; }
		}

		[List("Lookups.Consignor_List")]
		public virtual ZGuid ConsignorPK
		{
			get { return ConsignorDocumentaryAddress.OrganisationPK; }
			set { ConsignorDocumentaryAddress.OrganisationPK = value; }
		}

		public ZPropertyInfo ConsignorPKInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(ConsignorPK), x => ConsignorDocumentaryAddress.OrganisationPKInfo); }
		}

		public virtual JobDocAddress ConsignorDocumentaryAddress
		{
			get
			{
				if (fConsignorDocumentaryAddress == null || fConsignorDocumentaryAddress.IsDeleted)
				{
					UnRegisterListChangedCalledRefreshBinding(fConsignorDocumentaryAddress);
					fConsignorDocumentaryAddress = GetNewConsignorDocumentaryAddress();
					AddEventHandlersToConsignorDocumentaryAddressEvents();
					RegisterListChangedCalledRefreshBinding(fConsignorDocumentaryAddress);
				}
				return fConsignorDocumentaryAddress;
			}
		}

		#region AddEventHandlersToConsignorDocumentaryAddressEvents

		void AddEventHandlersToConsignorDocumentaryAddressEvents()
		{
			fConsignorDocumentaryAddress.E2_OA_AddressInfo.ValueChanged -= CnrAddressInfoOnValueChanged;
			fConsignorDocumentaryAddress.OrgAddressBeforeChange -= CnrDocAddress_OrgAddressBeforeChange;
			fConsignorDocumentaryAddress.DocAddressChanged -= CnrDocAddress_DocAddressChanged;
			fConsignorDocumentaryAddress.OrgHeaderAfterChange -= CnrDocAddress_OrgHeaderAfterChange;

			fConsignorDocumentaryAddress.E2_OA_AddressInfo.ValueChanged += CnrAddressInfoOnValueChanged;
			fConsignorDocumentaryAddress.OrgAddressBeforeChange += CnrDocAddress_OrgAddressBeforeChange;
			fConsignorDocumentaryAddress.DocAddressChanged += CnrDocAddress_DocAddressChanged;
			fConsignorDocumentaryAddress.OrgHeaderAfterChange += CnrDocAddress_OrgHeaderAfterChange;
		}

		void CnrDocAddress_OrgAddressBeforeChange(object sender, EventArgs e)
		{
			IsChangingConsignorAddress = true;
		}

		void CnrDocAddress_DocAddressChanged(object sender, EventArgs e)
		{
			ConsignorDocumentaryAddressContentsChanged();
			IsChangingConsignorAddress = false;
		}

		void CnrDocAddress_OrgHeaderAfterChange(object sender, EventArgs e)
		{
			ConsignorDocumentaryOrgHeaderChanged();
		}

		void CnrAddressInfoOnValueChanged(object sender, EventArgs args)
		{
			ConsignorDocumentaryAddressChanged();
		}

		#endregion

		public bool IsChangingConsignorAddress { get; private set; }

		void UpdateExportBroker()
		{
			var defaultExportBrokerPK = GetDefaultExportBrokerPK();
			if (defaultExportBrokerPK.HasValue && defaultExportBrokerPK.Value != JS_OH_ExportBroker && Globals.IsUserInteractive && !Factory.IsInSaveTransaction)
			{
				var shouldUpdateBroker = true;
				if (IsInDatabase && OnExportOrImportBrokerUpdate != null)
				{
					var eventArgs = new BrokerDefaultingEventArgs(DocAddressType.ExportBroker);
					OnExportOrImportBrokerUpdate(this, eventArgs);
					shouldUpdateBroker = eventArgs.ShouldUpdateBroker;
				}

				if (shouldUpdateBroker)
				{
					JS_OH_ExportBroker = defaultExportBrokerPK.Value;
				}
			}
		}

		protected virtual JobDocAddress GetNewConsignorDocumentaryAddress()
		{
			return DocAddresses.FindOrCreateWithRequirement(ConsignorDocAddressRequirement);
		}

		protected virtual void ConsignorDocumentaryAddressContentsChanged()
		{
		}

		public JobDocAddress ConsignorPickupAddress
		{
			get
			{
				if (ConsignorDocumentaryAddress != null && (fConsignorPickupAddress == null || fConsignorPickupAddress.IsDeleted))
				{
					if (fConsignorPickupAddress != null)
					{
						fConsignorPickupAddress.DocAddressChanged -= new EventHandler(fConsignorPickupAddress_DocAddressChanged);
						fConsignorPickupAddress.OrgHeaderAfterChange -= new EventHandler(ReloadNotes);
					}
					fConsignorPickupAddress = GetNewConsignorPickupAddress();
					fConsignorPickupAddress.DocAddressChanged += new EventHandler(fConsignorPickupAddress_DocAddressChanged);
					fConsignorPickupAddress.OrgHeaderAfterChange += new EventHandler(ReloadNotes);
				}

				return fConsignorPickupAddress;
			}
		}

		protected virtual JobDocAddress GetNewConsignorPickupAddress()
		{
			return DocAddresses.FindOrCreateWithRequirement(ConsignorPickupDeliveryAddressRequirement);
		}

		void fConsignorPickupAddress_DocAddressChanged(object sender, EventArgs e)
		{
			ConsignorPickupAddressChanged();
		}

		JobDocAddress fConsignorDocumentaryAddress;
		JobDocAddress fConsignorPickupAddress;

		#region Consignor Code/Company Name (for Grids)

		public ZString JS_Calc_ConsignorCompanyName
		{
			get { return ConsignorDocumentaryAddress.E2_CompanyNameTruncated; }
		}

		public ZPropertyInfo JS_Calc_ConsignorCompanyNameInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(JS_Calc_ConsignorCompanyName), x => ConsignorDocumentaryAddress.E2_CompanyNameInfo); }
		}

		[ReadOnly(true)]
		public ZString JS_Calc_ConsignorCompanyCode
		{
			get
			{
				ZString result = "";
				if (ConsignorDocumentaryAddress.HasRealOrganisation)
				{
					result = ConsignorDocumentaryAddress.Organisation.OH_Code;
				}
				else if (!JS_Calc_ConsignorCompanyName.IsEmpty)
				{
					result = new ZString('(' + JS_Calc_ConsignorCompanyName.Replace(" ", "") + ')');
				}

				return result;
			}
		}

		public ZPropertyInfo JS_Calc_ConsignorCompanyCodeInfo
		{
			get
			{
				return GetZPropertyInfo(Schema.JS_Calc_ConsignorCompanyCode);
			}
		}

		#endregion

		#endregion

		#region NotifyParty

		public OrgHeader NotifyParty
		{
			get { return (NotifyPartyDocumentaryAddress == null || !NotifyPartyDocumentaryAddress.HasRealOrganisation) ? null : NotifyPartyDocumentaryAddress.Organisation; }
		}

		public ZGuid NotifyPartyContactPK
		{
			get { return NotifyPartyDocumentaryAddress.ContactPK; }
			set
			{
				if (value != NotifyPartyDocumentaryAddress.ContactPK)
				{
					OrgContact contact = Factory.Load<OrgContact>(value);
					if (contact != null)
					{
						NotifyPartyDocumentaryAddress.OrganisationPK = contact.Header.PK;
						NotifyPartyDocumentaryAddress.ContactPK = value;
					}
				}
			}
		}

		public JobDocAddress NotifyPartyDocumentaryAddress
		{
			get
			{
				if (notifyPartyDocumentaryAddress == null || notifyPartyDocumentaryAddress.IsDeleted)
				{
					UnRegisterListChangedCalledRefreshBinding(notifyPartyDocumentaryAddress);
					notifyPartyDocumentaryAddress = GetNotifyPartyDocumentaryAddress();
					AddEventHandlersToNotifyPartyDocumentaryAddressEvents();
					RegisterListChangedCalledRefreshBinding(notifyPartyDocumentaryAddress);
				}

				if (notifyPartyDocumentaryAddress != null)
				{
					notifyPartyDocumentaryAddress.ReadOnly = IsNotifyPartyDocAddressReadOnly;
				}

				return notifyPartyDocumentaryAddress;
			}
		}

		bool IsNotifyPartyDocAddressReadOnly
		{
			get
			{
				var consigneeDocumentaryAddress = DocAddresses.FindByDocAddressType(DocAddressType.ConsigneeDocumentaryAddress);
				return consigneeDocumentaryAddress == null || !consigneeDocumentaryAddress.IsValidAddress || IsPropertyReadOnlyDueToPhase(Schema.NotifyContact);
			}
		}

		protected virtual JobDocAddress AddEventHandlersToNotifyPartyDocumentaryAddressEvents()
		{
			return notifyPartyDocumentaryAddress;
		}

		JobDocAddress notifyPartyDocumentaryAddress;

		protected virtual JobDocAddress GetNotifyPartyDocumentaryAddress()
		{
			return DocAddresses.FindOrCreateWithRequirement(NotifyPartyDocAddressRequirement);
		}

		#endregion

		#region NotifyParty2DocumentaryAddress

		public JobDocAddress NotifyParty2DocumentaryAddress
		{
			get
			{
				if (BusinessObject.IsNullOrDeleted(notifyParty2DocumentaryAddress))
				{
					notifyParty2DocumentaryAddress = DocAddresses.FindOrCreateWithRequirement(NotifyParty2DocAddressRequirement);
				}
				return notifyParty2DocumentaryAddress;
			}
		}
		JobDocAddress notifyParty2DocumentaryAddress;

		JobDocAddressRequirement NotifyParty2DocAddressRequirement
		{
			get { return notifyParty2DocAddressRequirement ?? (notifyParty2DocAddressRequirement = GetNotifyParty2DocAddressRequirement()); }
		}
		JobDocAddressRequirement notifyParty2DocAddressRequirement;

		protected virtual JobDocAddressRequirement GetNotifyParty2DocAddressRequirement()
		{
			return new JobDocAddressRequirement(DocAddressType.NotifyParty2, ContactType.NotifyParty);
		}

		#endregion

		#region NotifyParty3DocumentaryAddress

		public JobDocAddress NotifyParty3DocumentaryAddress
		{
			get
			{
				if (BusinessObject.IsNullOrDeleted(notifyParty3DocumentaryAddress))
				{
					notifyParty3DocumentaryAddress = DocAddresses.FindOrCreateWithRequirement(NotifyParty3DocAddressRequirement);
				}
				return notifyParty3DocumentaryAddress;
			}
		}
		JobDocAddress notifyParty3DocumentaryAddress;

		JobDocAddressRequirement NotifyParty3DocAddressRequirement
		{
			get { return notifyParty3DocAddressRequirement ?? (notifyParty3DocAddressRequirement = GetNotifyParty3DocAddressRequirement()); }
		}
		JobDocAddressRequirement notifyParty3DocAddressRequirement;

		protected virtual JobDocAddressRequirement GetNotifyParty3DocAddressRequirement()
		{
			return new JobDocAddressRequirement(DocAddressType.NotifyParty3, ContactType.NotifyParty);
		}

		#endregion

		#region BuyerDocAddress
		public JobDocAddress BuyerDocAddress
		{
			get
			{
				if (fBuyerDocAddress == null || fBuyerDocAddress.IsDeleted)
				{
					fBuyerDocAddress = DocAddresses.FindOrCreateWithRequirement(BuyerDocAddressRequirement);
				}
				return fBuyerDocAddress;
			}
		}
		JobDocAddress fBuyerDocAddress;

		JobDocAddressRequirement BuyerDocAddressRequirement
		{
			get
			{
				if (fBuyerDocAddressRequirement == null)
				{
					fBuyerDocAddressRequirement = new JobDocAddressRequirement(DocAddressType.BuyerDocumentaryAddress, ContactType.Consignee);
				}
				return fBuyerDocAddressRequirement;
			}
		}
		JobDocAddressRequirement fBuyerDocAddressRequirement;
		#endregion

		#region InsuredByDocAddress
		public JobDocAddress InsuredByDocAddress
		{
			get
			{
				if (fInsuredByDocAddress == null || fInsuredByDocAddress.IsDeleted)
				{
					fInsuredByDocAddress = DocAddresses.FindOrCreateWithRequirement(InsuredByDocAddressRequirement);
				}
				return fInsuredByDocAddress;
			}
		}
		JobDocAddress fInsuredByDocAddress;

		JobDocAddressRequirement InsuredByDocAddressRequirement
		{
			get
			{
				if (fInsuredByDocAddressRequirement == null)
				{
					fInsuredByDocAddressRequirement = new JobDocAddressRequirement(DocAddressType.InsuredByDocumentaryAddress, ContactType.Consignee);
				}
				return fInsuredByDocAddressRequirement;
			}
		}
		JobDocAddressRequirement fInsuredByDocAddressRequirement;
		#endregion

		#region AssuredPartyDocAddress
		public JobDocAddress AssuredPartyDocAddress
		{
			get
			{
				if (fAssuredPartyDocAddress == null || fAssuredPartyDocAddress.IsDeleted)
				{
					fAssuredPartyDocAddress = DocAddresses.FindOrCreateWithRequirement(AssuredPartyDocAddressRequirement);
				}
				return fAssuredPartyDocAddress;
			}
		}
		JobDocAddress fAssuredPartyDocAddress;

		JobDocAddressRequirement AssuredPartyDocAddressRequirement
		{
			get
			{
				if (fAssuredPartyDocAddressRequirement == null)
				{
					fAssuredPartyDocAddressRequirement = new JobDocAddressRequirement(DocAddressType.AssuredPartyDocumentaryAddress, ContactType.Consignee);
				}
				return fAssuredPartyDocAddressRequirement;
			}
		}
		JobDocAddressRequirement fAssuredPartyDocAddressRequirement;
		#endregion

		#region ClaimsPayableByDocAddress
		public JobDocAddress ClaimsPayableByDocAddress
		{
			get
			{
				if (fClaimsPayableByDocAddress == null || fClaimsPayableByDocAddress.IsDeleted)
				{
					fClaimsPayableByDocAddress = DocAddresses.FindOrCreateWithRequirement(ClaimsPayableByDocAddressRequirement);
				}
				return fClaimsPayableByDocAddress;
			}
		}
		JobDocAddress fClaimsPayableByDocAddress;

		JobDocAddressRequirement ClaimsPayableByDocAddressRequirement
		{
			get
			{
				if (fClaimsPayableByDocAddressRequirement == null)
				{
					fClaimsPayableByDocAddressRequirement = new JobDocAddressRequirement(DocAddressType.ClaimsPayableByDocumentaryAddress, ContactType.Consignee);
				}
				return fClaimsPayableByDocAddressRequirement;
			}
		}
		JobDocAddressRequirement fClaimsPayableByDocAddressRequirement;
		#endregion

		#region SurveyReportPartyDocAddress
		public JobDocAddress SurveyReportPartyDocAddress
		{
			get
			{
				if (fSurveyReportPartyDocAddress == null || fSurveyReportPartyDocAddress.IsDeleted)
				{
					fSurveyReportPartyDocAddress = DocAddresses.FindOrCreateWithRequirement(SurveyReportPartyDocAddressRequirement);
				}
				return fSurveyReportPartyDocAddress;
			}
		}
		JobDocAddress fSurveyReportPartyDocAddress;

		JobDocAddressRequirement SurveyReportPartyDocAddressRequirement
		{
			get
			{
				if (fSurveyReportPartyDocAddressRequirement == null)
				{
					fSurveyReportPartyDocAddressRequirement = new JobDocAddressRequirement(DocAddressType.SurveyReportPartyDocumentaryAddress, ContactType.Consignee);
				}
				return fSurveyReportPartyDocAddressRequirement;
			}
		}
		JobDocAddressRequirement fSurveyReportPartyDocAddressRequirement;
		#endregion

		#region ControllingCustomer

		[MaxLength(3)]
		public ZString ControllingCustomerFieldType
		{
			get
			{
				return ControllingCustomerAddress.E2_AddressOverride ?
					nameof(FieldType.Text) :
					nameof(FieldType.OrganisationGuid);
			}
		}

		public ZPropertyInfo ControllingCustomerFieldTypeInfo
		{
			get { return GetZPropertyInfo(nameof(ControllingCustomerFieldType)); }
		}

		public ZGuid ControllingCustomerPK
		{
			get => ControllingCustomer?.PK ?? ZGuid.Empty;
		}

		public ZPropertyInfo ControllingCustomerPKInfo => GetWrappedZPropertyInfo(nameof(ControllingCustomerPK), _ => ControllingCustomerAddress.OrganisationPKInfo);

		[BusinessObjectTestExclude]
		[RelatedBusinessObject("ControllingCustomer")]
		[List("Lookups.OrgHeader_List")]
		public ZString ControllingCustomerNameOrPK
		{
			get { return ControllingCustomerAddress.OrganisationNameOrPK; }
			set { ControllingCustomerAddress.OrganisationNameOrPK = value; }
		}

		public ZPropertyInfo ControllingCustomerNameOrPKInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ControllingCustomerNameOrPK, x => ControllingCustomerAddress.OrganisationNameOrPKInfo); }
		}

		public OrgHeader ControllingCustomer
		{
			get { return !ControllingCustomerAddress.HasRealOrganisation ? null : ControllingCustomerAddress.Organisation; }
		}

		public JobDocAddress ControllingCustomerAddress
		{
			get
			{
				if (BusinessObject.IsNullOrDeleted(controllingCustomerAddress))
				{
					controllingCustomerAddress = DocAddresses.FindOrCreateWithRequirement(ControllingCustomerDocAddressRequirement);
					SetControllingCustomerAddressDefaults(controllingCustomerAddress);
				}

				if (controllingCustomerAddress != null && IsPropertyReadOnlyDueToPhase(Schema.ControllingCustomerNameOrPK))
				{
					controllingCustomerAddress.SetReadOnlyIncludingChildren(true);
				}

				return controllingCustomerAddress;
			}
		}
		JobDocAddress controllingCustomerAddress;

		protected virtual void ControllingCustomerAddressChanged(object sender, EventArgs e)
		{
			MarkAsNeedingValidation();
		}

		public SecurityCheckpoint GetControllingCustomerSecurityCheckPoint()
		{
			if (IsAir)
			{
				return Env.Security.MaintainShipmentAllowSaveWithoutControllingCustomerAir;
			}

			if (IsSea)
			{
				return Env.Security.MaintainShipmentAllowSaveWithoutControllingCustomerSea;
			}

			if (IsRoad)
			{
				return Env.Security.MaintainShipmentAllowSaveWithoutControllingCustomerRoad;
			}

			if (IsRail)
			{
				return Env.Security.MaintainShipmentAllowSaveWithoutControllingCustomerRail;
			}

			return Env.Security.MaintainShipmentAllowSaveWithoutControllingCustomer;
		}

		public DateTimeRegistryItem GetMandatoryControllingAgentEffectiveDateRegistry()
		{
			DateTimeRegistryItem registryItem = null;

			if (IsAir)
			{
				registryItem = FreightDataRegistry.Instance.MandatoryControllingAgentEffectiveDateAir;
			}
			else if (IsSea)
			{
				registryItem = FreightDataRegistry.Instance.MandatoryControllingAgentEffectiveDateSea;
			}
			else if (IsRoad)
			{
				registryItem = FreightDataRegistry.Instance.MandatoryControllingAgentEffectiveDateRoad;
			}
			else if (IsRail)
			{
				registryItem = FreightDataRegistry.Instance.MandatoryControllingAgentEffectiveDateRail;
			}

			if (registryItem == null || registryItem.Value == DateTime.MinValue)
			{
				registryItem = FreightDataRegistry.Instance.MandatoryControllingAgentEffectiveDateAll;
			}

			return registryItem;
		}

		public DateTimeRegistryItem GetMandatoryControllingCustomerEffectiveDateRegistry()
		{
			DateTimeRegistryItem registryItem = null;

			if (IsAir)
			{
				registryItem = FreightDataRegistry.Instance.MandatoryControllingCustomerEffectiveDateAir;
			}
			else if (IsSea)
			{
				registryItem = FreightDataRegistry.Instance.MandatoryControllingCustomerEffectiveDateSea;
			}
			else if (IsRoad)
			{
				registryItem = FreightDataRegistry.Instance.MandatoryControllingCustomerEffectiveDateRoad;
			}
			else if (IsRail)
			{
				registryItem = FreightDataRegistry.Instance.MandatoryControllingCustomerEffectiveDateRail;
			}

			if (registryItem == null || registryItem.Value == DateTime.MinValue)
			{
				registryItem = FreightDataRegistry.Instance.MandatoryControllingCustomerEffectiveDateAll;
			}

			return registryItem;
		}

		#endregion

		#region ControllingAgent
		public OrgHeader ControllingAgent
		{
			get { return !ControllingAgentDocumentaryAddress.HasRealOrganisation ? null : ControllingAgentDocumentaryAddress.Organisation; }
		}

		public JobDocAddress ControllingAgentDocumentaryAddress
		{
			get
			{
				if (BusinessObject.IsNullOrDeleted(controllingAgentDocumentaryAddress))
				{
					controllingAgentDocumentaryAddress = DocAddresses.FindOrCreateWithRequirement(ControllingAgentDocAddressRequirement);
					SetControllingAgentAddressDefaults(controllingAgentDocumentaryAddress);
				}

				if (controllingAgentDocumentaryAddress != null && IsPropertyReadOnlyDueToPhase(Schema.ControllingAgentNameOrPK))
				{
					controllingAgentDocumentaryAddress.SetReadOnlyIncludingChildren(true);
				}

				return controllingAgentDocumentaryAddress;
			}
		}
		JobDocAddress controllingAgentDocumentaryAddress;

		protected void SetControllingAgentAddressDefaults(JobDocAddress docAddress)
		{
			docAddress.HasChangesChanged += (sender, e) => MarkAsNeedingValidation();
		}
		#endregion

		#region Pickup Agent

		public JobDocAddress PickupAgentDocumentaryAddress
		{
			get
			{
				if (BusinessObject.IsNullOrDeleted(pickupAgentDocumentaryAddress))
				{
					pickupAgentDocumentaryAddress = DocAddresses.FindOrCreateWithRequirement(PickupAgentDocAddressRequirement);
					AddEventHandlersToPickupAgentDocumentaryAddressEvents();

					if (pickupAgentDocumentaryAddress != null)
					{
						if (IsPropertyReadOnlyDueToPhase(Schema.PickupAgentPK))
						{
							pickupAgentDocumentaryAddress.SetReadOnlyIncludingChildren(true);
						}
					}
				}

				return pickupAgentDocumentaryAddress;
			}
		}
		JobDocAddress pickupAgentDocumentaryAddress;

		JobDocAddress AddEventHandlersToPickupAgentDocumentaryAddressEvents()
		{
			pickupAgentDocumentaryAddress.E2_OA_AddressInfo.ValueChanged -= PickupAgentAddressInfoOnValueChanged;
			pickupAgentDocumentaryAddress.E2_OA_AddressInfo.ValueChanged += PickupAgentAddressInfoOnValueChanged;
			return pickupAgentDocumentaryAddress;
		}

		void PickupAgentAddressInfoOnValueChanged(object sender, EventArgs args) => PickupAgentDocumentaryAddressChanged();

		protected virtual void PickupAgentDocumentaryAddressChanged() { }

		protected JobDocAddressRequirement PickupAgentDocAddressRequirement
		{
			get { return pickupAgentDocAddressRequirement ?? (pickupAgentDocAddressRequirement = new JobDocAddressRequirement(DocAddressType.PickupAgent, ContactType.FreightAgent)); }
		}
		JobDocAddressRequirement pickupAgentDocAddressRequirement;

		public OrgHeader PickupAgent
		{
			get { return PickupAgentDocumentaryAddress.HasRealOrganisation ? PickupAgentDocumentaryAddress.Organisation : null; }
		}

		[ReadOnly(true)]
		[MaxLength(OrgHeader.Schema.OH_CodeMaxLength)]
		public ZString PickupAgentCompanyCode
		{
			get { return PickupAgent != null ? PickupAgent.OH_Code : ZString.Empty; }
		}

		public ZPropertyInfo PickupAgentCompanyCodeInfo
		{
			get { return GetZPropertyInfo(Schema.PickupAgentCompanyCode); }
		}

		public ZGuid PickupAgentPK
		{
			get { return PickupAgentDocumentaryAddress.OrganisationPK; }
			set { PickupAgentDocumentaryAddress.OrganisationPK = value; }
		}

		public ZPropertyInfo PickupAgentPKInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(PickupAgentPK), x => PickupAgentDocumentaryAddress.OrganisationPKInfo); }
		}

		#endregion

		#region ManufacturerDocAddress

		public JobDocAddress ManufacturerDocAddress
		{
			get
			{
				if (manufacturerDocAddress == null || manufacturerDocAddress.IsDeleted)
				{
					manufacturerDocAddress = DocAddresses.FindOrCreateWithRequirement(ManufacturerDocAddressRequirement);
				}
				return manufacturerDocAddress;
			}
		}
		JobDocAddress manufacturerDocAddress;

		JobDocAddressRequirement ManufacturerDocAddressRequirement
		{
			get
			{
				if (manufacturerDocAddressRequirement == null)
				{
					manufacturerDocAddressRequirement = new JobDocAddressRequirement(DocAddressType.Manufacturer);
				}
				return manufacturerDocAddressRequirement;
			}
		}
		JobDocAddressRequirement manufacturerDocAddressRequirement;

		#endregion

		#endregion

		#region OuterPackLines

		[ChildEditable(false)]
		public OuterPackLineCollection OuterPackLines
		{
			get
			{
				if (outerPackLines == null)
				{
					outerPackLines = GetNewOuterPackLineCollection();
					outerPackLines.Load();
					RegisterEditableChildObject(outerPackLines, "OuterPackLines");
					outerPackLines.CountChanged += OuterPackLines_CountChanged;
					OnOuterPackLinesCreated();
				}

				if (reloadOuterPackLines)
				{
					reloadOuterPackLines = false;
					isLoadingOuterPackLines = true;
					outerPackLines.Load();
					isLoadingOuterPackLines = false;
				}

				return outerPackLines;
			}
		}

		protected bool isLoadingOuterPackLines;

		protected virtual void OnOuterPackLinesCreated()
		{
		}

		protected OuterPackLineCollection outerPackLines;

		protected virtual void OuterPackLines_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
		}

		internal void ReloadOuterPackLines()
		{
			CommonShipment currentShipment = this;

			List<CommonShipment> meAndAllMyParents = new List<CommonShipment>();
			while (currentShipment != null && !meAndAllMyParents.Contains(currentShipment))
			{
				currentShipment.reloadOuterPackLines = true;
				meAndAllMyParents.Add(currentShipment);
				currentShipment = currentShipment.CoLoadMasterShipment;
			}
		}
		bool reloadOuterPackLines;

		public OuterPackLineCollection GetNewOuterPackLineCollection()
		{
			return GetNewOuterPackLineCollectionCore();
		}

		protected virtual OuterPackLineCollection GetNewOuterPackLineCollectionCore()
		{
			return new OuterPackLineCollection(this, Factory);
		}

		public bool IsRoot { get; set; }

		public bool CanHaveOwnPackLines
		{
			get { return !IsMasterShipmentRepresentingAllChildShipments; }
		}

		public virtual void OnPackLinePackedOrUnpacked(PackLine packline, CommonContainer container, bool isPacked)
		{
		}

		#endregion

		#region Consols / OnConsolChanged

		public ConsolCollection Consols
		{
			get
			{
				if (fConsols == null)
				{
					fConsols = GetNewConsolCollection();

					if (!consolsInitialised)
					{
						consolsInitialised = true;
						fConsols.CountChanged += new CollectionCountChangedEventHandler(Consols_CountChanged);
					}

					if (customsManifestVisibilityChanged != null)
					{
						fConsols.ConsolLoadOrDischargePortChanged += ConsolLoadOrDischargePortChanged_ForCustomsManifestVisibilityChanged;
						fConsols.CountChanged += ConsolCountChanged_ForCustomsManifestVisibilityChanged;
					}

					fConsols.Load();

					var state = ChildEditableService.GetState(Factory);
					if (state == ChildEditableServiceStates.Shipment || state == ChildEditableServiceStates.Order)
					{
						RegisterEditableChildObject(fConsols);
					}

					fConsols.IsManagedForDataRefresh = true;
				}

				return fConsols;
			}
		}
		ConsolCollection fConsols;

		bool consolsInitialised;

		void Consols_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			if (e.ItemAdded)
			{
				DocsAndCartage.Validation.ValidateJP_EstimatedDelivery();
			}
		}

		public IEnumerable<CommonConsol> CandidateConsolsForDepartureArrival
		{
			get => this.IsDirectShipment ? Consols.Cast<CommonConsol>().Where(c => c.IsDirect) : Consols.Cast<CommonConsol>();
		}

		protected virtual ConsolCollection GetNewConsolCollection()
		{
			return new ConsolCollection(this);
		}

		public bool HasConsolPastCutOffDate
		{
			get { return Consols.Cast<CommonConsol>().Any(consol => consol.JK_ConsolCutOffDate.IsValid && consol.JK_ConsolCutOffDate < ZDateTime.UtcNow); }
		}

		#endregion

		#region Containers

		/// <summary>
		/// Collection of all Containers that the packlines on this Shipment have been packed into.
		/// </summary>
		public IEnumerable<CommonContainer> Containers
		{
			get
			{
				if (!IsDeleted && (JS_IsForwardRegistered || !JS_IsBooking))
				{
					return Consols.AllContainers
						.Where(container => !container.IsDeleted
							&& container.PackLines.GetPKs()
								.Intersect(OuterPackLines.GetPKs()).Any());
				}

				return Enumerable.Empty<CommonContainer>();
			}
		}

		public void UnpackFromContainer(CommonContainer container)
		{
			if (container != null)
			{
				UnpackFromContainer(container.PK);
			}
		}

		public void UnpackFromContainer(ZGuid containerPK)
		{
			foreach (var line in OuterPackLines.Cast<PackLine>().Where(x => x.Containers.Contains(containerPK)))
			{
				line.Containers.Remove(containerPK);
			}
		}

		internal void OnContainerAdded(CommonContainer container)
		{
			if (ContainerAdded != null)
			{
				ContainerAdded(this, new ContainerAddedEventArgs(container));
			}
		}

		public class ContainerAddedEventArgs : EventArgs
		{
			public ContainerAddedEventArgs(CommonContainer container)
			{
				Container = container;
			}

			public CommonContainer Container { get; }
		}

		public event EventHandler<ContainerAddedEventArgs> ContainerAdded;

		public ZDecimal ContainerTEUCount
		{
			get { return Containers.Sum(x => x.JC_Calc_TEUCount); }
		}

		public int NumberOfEmptyContainers
		{
			get { return Containers.Count(x => x.JC_ContainerMode == Core.Constants.ContainerModes.Empty); }
		}

		public ShipmentContainerCollectionForBinding ContainersForBinding
		{
			get
			{
				if (containersForBinding == null)
				{
					containersForBinding = GetNewShipmentContainerCollectionForBinding();
				}

				containersForBinding.Load();

				return containersForBinding;
			}
		}

		ShipmentContainerCollectionForBinding containersForBinding;

		protected virtual ShipmentContainerCollectionForBinding GetNewShipmentContainerCollectionForBinding()
		{
			return new ShipmentContainerCollectionForBinding(this, Factory);
		}

		public class ShipmentContainerCollectionForBinding : BusinessObjectCollection<CommonContainer>
		{
			public ShipmentContainerCollectionForBinding(CommonShipment shipment, BusinessObjectFactory factory)
			: base(factory)
			{
				this.shipment = shipment;
			}

			readonly CommonShipment shipment;

			public override void Load()
			{
				var containersToAdd = shipment.Containers.ToArray();
				RemoveRange(this.Except(containersToAdd).ToArray());
				AddRange(containersToAdd);
			}
		}

		#endregion

		#region Port Delivery Time

		public void UpdateETAWithPortDefaultDeliveryTime()
		{
			OnUpdateETAWithPortDefaultDeliveryTime(true);
		}

		public void UpdateETAWithPortDefaultDeliveryTimeIfEmpty()
		{
			OnUpdateETAWithPortDefaultDeliveryTime(false);
		}

		public void UpdateETDeliveryWithPortDefaultDeliveryTime()
		{
			OnUpdateETDeliveryWithPortDefaultDeliveryTime();
		}

		protected GlbPortDeliveryTime DefaultPortDeliveryTime
		{
			get { return new GlbPortDeliveryTime.Loader(Factory, LastPortOfDischarge, Destination, Consignee, FreightMode.ToString(), ZBool.True).Load(); }
		}

		void OnUpdateETAWithPortDefaultDeliveryTime(bool updateWithConsolETA)
		{
			ZDateTime lastETA = JS_Calc_LastETA;
			if (lastETA.IsValid)
			{
				GlbPortDeliveryTime deliveryTime = DefaultPortDeliveryTime;
				if (deliveryTime != null)
				{
					JS_E_ARV = lastETA.AddDays(deliveryTime.G1_DaysDelayFromArrivalToDeliver);
				}
				else if (updateWithConsolETA)
				{
					JS_E_ARV = lastETA;
				}
			}
		}

		void OnUpdateETDeliveryWithPortDefaultDeliveryTime()
		{
			ZDateTime eTA = JS_E_ARV;
			if (eTA.IsValid && !fIsImportingData)
			{
				ZBool defaultDeliverySet = FreightDataRegistry.Instance.DefaultDeliveryWhenDelayIsNotSet.Value;
				GlbPortDeliveryTime deliveryTime = DefaultPortDeliveryTime;
				if (deliveryTime != null && (deliveryTime.G1_DaysFromDestinationArrivalToClientDelivery > 0 || defaultDeliverySet))
				{
					DocsAndCartage.JP_EstimatedDelivery = eTA.AddDays(deliveryTime.G1_DaysFromDestinationArrivalToClientDelivery);
				}
			}
		}

		ZString FreightMode
		{
			get
			{
				ZString result = ZString.Empty;
				if (JS_PackingMode == Constants.ContainerModes.FCL)
				{
					result = JS_PackingMode;
				}
				else if (JS_PackingMode == Constants.ContainerModes.LCL
					 || JS_TransportMode == Constants.TransportModes.AirSea)
				{
					result = Constants.ContainerModes.LCL;
				}
				else if (JS_TransportMode == Constants.TransportModes.SeaAir)
				{
					result = Constants.TransportModes.Air;
				}
				else if (JS_TransportMode == Constants.TransportModes.Air
					 || JS_TransportMode == Constants.TransportModes.Road
					 || JS_TransportMode == Constants.TransportModes.Rail
					 || JS_TransportMode == Constants.TransportModes.Sea)
				{
					result = JS_TransportMode;
				}
				return result;
			}
		}

		#endregion

		#region Job

		public JobHeader Job
		{
			get { return new JobHeader.Loader(this).Load(true, false); }
		}

		public JobHeader GetJob(GlbCompany company)
		{
			return new JobHeader.Loader(this).Load(true, company, false);
		}

		#endregion

		#region Documents and Cartage Info

		protected void RefreshVisibleBindingsForTransportAndPackingModeChange()
		{
			if (IsDocsAndCartageSet)
			{
				DocsAndCartage.JP_OA_PickupCartageCoAddrInfo.RefreshBinding();
				DocsAndCartage.JP_FCLPickupEquipmentNeededInfo.RefreshBinding();
				DocsAndCartage.JP_OA_DeliveryCartageCoAddrInfo.RefreshBinding();
			}
		}

		#endregion

		#region CoLoad Master Shipment

		public CommonShipment CoLoadMasterShipment
		{
			get { return (CommonShipment)Factory.Load(GetType(), JS_JS_ColoadMasterShipment); }
		}

		protected internal virtual void AdjustCoLoadMasterListFilterAndFilterBusinessObjectDefaults(IRelatedShipmentsCollection coloadMasterCollection)
		{
		}

		public bool HasMaster(ZString shipmentType)
		{
			return HasMaster(this, shipmentType);
		}

		bool HasMaster(CommonShipment shipment, ZString shipmentType)
		{
			if (shipment.JS_JS_ColoadMasterShipment.IsValid)
			{
				var master = shipment.CoLoadMasterShipment;
				if (master != null)
				{
					return (master.JS_ShipmentType == shipmentType) || HasMaster(master, shipmentType);
				}
			}

			return false;
		}

		#endregion

		#region CoLoad Sub Shipments

		internal void AddRelatedShipmentCollectionFilterBusinessObjectDefaults(IRelatedShipmentsCollection relatedShipmentsCollection)
		{
			AddRelatedShipmentCollectionFilterBusinessObjectDefaultsCore(relatedShipmentsCollection);
		}

		protected virtual void AddRelatedShipmentCollectionFilterBusinessObjectDefaultsCore(IRelatedShipmentsCollection relatedShipmentsCollection)
		{
		}

		[List("Lookups.CoLoadShipment_List")]
		public CoLoadShipmentCollection CoLoadShipments
		{
			get
			{
				EnsureCoLoadShipmentCollectionIsInitialised();
				return fCoLoadShipments;
			}
		}
		CoLoadShipmentCollection fCoLoadShipments;

		protected virtual CoLoadShipmentCollection GetNewCoLoadShipmentCollection()
		{
			return new CoLoadShipmentCollection(this, Factory);
		}

		void EnsureCoLoadShipmentCollectionIsInitialised()
		{
			if (fCoLoadShipments == null)
			{
				fCoLoadShipments = GetNewCoLoadShipmentCollection();
				fCoLoadShipments.Load();
				fCoLoadShipments.IsManagedForDataRefresh = true;
				RegisterSubShipmentsIfRequired();
			}
		}

		void EnsureCoLoadShipmentsAreLoaded()
		{
			if (JS_JS_ColoadMasterShipment.IsValid && CoLoadMasterShipment != null)
			{
				object loaded = CoLoadMasterShipment.CoLoadShipments;
			}
		}

		public string NestedCoLoadShipmentsList()
		{
			StringBuilder shipmentList = new StringBuilder();
			foreach (CommonShipment shipment in CoLoadShipments)
			{
				shipmentList.AppendLine(shipment.JobNumber);
				shipmentList.Append(shipment.NestedCoLoadShipmentsList());
			}

			return shipmentList.ToString();
		}

		void Shipment_UpdatedByDataRefreshIncludingChildren(object sender, EventArgs e)
		{
			RemoveDetachedShipmentFromCoLoadMasterCollection();
			AttachToColoadMasterCollectionIfNecessary();
		}

		void AttachToColoadMasterCollectionIfNecessary()
		{
			if (!IsDeleted && CoLoadMasterShipment != null && !CoLoadMasterShipment.CoLoadShipments.Contains(this))
			{
				IsChangingColoadMaster = true;
				try
				{
					CoLoadMasterShipment.CoLoadShipments.Add(this);
				}
				finally
				{
					IsChangingColoadMaster = false;
				}
			}
		}

		void RemoveDetachedShipmentFromCoLoadMasterCollection()
		{
			CommonShipment coloadMasterToRemoveFrom = null;

			foreach (BusinessObjectCollection collection in ((IBusinessObjectInternals)this).ParentCollections)
			{
				CoLoadShipmentCollection coLoadShipmentCollection = collection as CoLoadShipmentCollection;
				if (coLoadShipmentCollection != null && JS_JS_ColoadMasterShipment != coLoadShipmentCollection.Master.PK)
				{
					coloadMasterToRemoveFrom = coLoadShipmentCollection.Master;
				}
			}

			if (coloadMasterToRemoveFrom != null)
			{
				coloadMasterToRemoveFrom.CoLoadShipments.Remove(this);
			}
		}

		public List<ZGuid> GetPksFromAllSubShipmentsWithoutChildren()
		{
			return GetPksFromAllSubShipmentsAndDeclarationsWithoutChildren(this, false);
		}

		public List<ZGuid> GetPksFromAllSubShipmentsAndDeclarationsWithoutChildren()
		{
			return GetPksFromAllSubShipmentsAndDeclarationsWithoutChildren(this, true);
		}
		List<ZGuid> GetPksFromAllSubShipmentsAndDeclarationsWithoutChildren(CommonShipment master, bool containDeclarationsPK)
		{
			var pks = new List<ZGuid>();

			if (master.IsMasterShipmentRepresentingAllChildShipments && !master.HasCircularCoLoadMasterReference())
			{
				foreach (CommonShipment shipment in master.CoLoadShipments)
				{
					if (shipment.IsMasterShipmentRepresentingAllChildShipments)
					{
						pks.AddRange(GetPksFromAllSubShipmentsAndDeclarationsWithoutChildren(shipment, containDeclarationsPK));
					}
					else
					{
						pks.Add(shipment.PK);

						if (containDeclarationsPK)
						{
							AddPksFromDeclaration(shipment, pks);
						}
					}
				}
			}

			return pks;
		}

		public void AddPksFromDeclaration(CommonShipment shipment, List<ZGuid> pks)
		{
			if (shipment.Declarations.Length > 0)
			{
				foreach (IBaseJobDeclaration declaration in shipment.Declarations)
				{
					pks.Add(declaration.PK);
				}
			}
		}

		public bool IsMasterInSubShipmentContext { get; set; }

		#endregion

		#region Consols

		public bool HasConsolsDischargingInCurrentCountry
		{
			get { return Consols.Cast<CommonConsol>().Any(consol => consol.JK_RL_NKDischargePort.StartsWith(GlbCompany.CurrentCompany.GC_RN_NKCountryCode.ToString())); }
		}

		public bool HasConsolsLoadingInCurrentCountry
		{
			get { return Consols.Cast<CommonConsol>().Any(consol => consol.JK_RL_NKLoadPort.StartsWith(GlbCompany.CurrentCompany.GC_RN_NKCountryCode.ToString())); }
		}

		public CommonConsol ArrivalConsol => Consols.Count == 1 ? Consols[0] : GetArrivalConsol();

		protected CommonConsol GetArrivalConsol()
		{
			CommonConsol arrivalConsol = null;
			foreach (CommonConsol consol in CandidateConsolsForDepartureArrival)
			{
				if (CountryCode(consol.JK_RL_NKDischargePort) == CountryCode(JS_RL_NKDestination))
				{
					arrivalConsol = consol;
				}
			}

			return arrivalConsol;
		}

		public CommonConsol DepartureConsol
		{
			get { return Consols.Count == 1 ? Consols[0] : GetDepartureConsol(); }
		}

		protected CommonConsol GetDepartureConsol()
		{
			CommonConsol departureConsol = null;

			foreach (CommonConsol consol in CandidateConsolsForDepartureArrival)
			{
				if (CountryCode(consol.JK_RL_NKLoadPort) == CountryCode(JS_RL_NKOrigin))
				{
					departureConsol = consol;
				}
			}

			return departureConsol;
		}

		public virtual CommonConsol MostInterestingDepartureConsol
		{
			get
			{
				CommonConsol consol = Consols.Count == 1 ? Consols[0] : null;
				var candidateConsols = CandidateConsolsForDepartureArrival.ToArray();

				if (consol == null)
				{
					consol = (from c in candidateConsols.OfType<CommonConsol>()
										where (c.JK_TransportMode == JS_TransportMode ||
										c.JK_TransportMode == Constants.TransportModes.Sea && JS_TransportMode == Constants.TransportModes.SeaAir ||
										c.JK_TransportMode == Constants.TransportModes.Air && JS_TransportMode == Constants.TransportModes.AirSea) &&
										(CountryCode(c.JK_RL_NKLoadPort) == CountryCode(JS_RL_NKOrigin))
										select c).FirstOrDefault();
				}

				if (consol == null)
				{
					consol = (from c in candidateConsols.OfType<CommonConsol>()
										where CountryCode(c.JK_RL_NKLoadPort) == CountryCode(JS_RL_NKOrigin)
										select c).FirstOrDefault();
				}

				return consol;
			}
		}

		public virtual CommonConsol CurrentBranchDepartureAirConsol
		{
			get
			{
				var airConsols = (from consol in CandidateConsolsForDepartureArrival.Cast<CommonConsol>()
													where consol.IsAir
													select consol).ToList();

				CommonConsol currentBranchDepartureAirConsol = (airConsols.Count == 1 ? airConsols[0] : null)
																 ?? (from consol in airConsols
																		 where consol.LoadPort != null && consol.LoadPort.Country != null && consol.LoadPort.Country.Code == GlbBranch.CurrentBranch.Country.Code
																		 select consol).FirstOrDefault();

				return currentBranchDepartureAirConsol;
			}
		}

		/// <summary>
		/// Returns the departure consol for transhipments, otherwise returns nearest arrival or departure in current branch
		/// </summary>
		public CommonConsol LocalConsol
		{
			get
			{
				CommonConsol result = null;
				if (Consols.Count == 1)
				{
					result = Consols[0];
				}
				else if (Consols.Count > 1)
				{
					var helper = new TranshipmentHelper(this);

					result = this.IsExport() || this.IsCrossTrade()
						? helper.ExportConsol ?? DepartureConsol ?? Consols[0]
						: helper.ImportConsol ?? ArrivalConsol ?? Consols[0];
				}
				return result;
			}
		}

		protected ZString CountryCode(ZString unloco)
		{
			return unloco.SubstringSafe(0, 2);
		}

		protected enum ShipmentTypes { Import, Export, Other }

		public virtual CommonConsol FindCorrectConsol()
		{
			ShipmentTypes type = ShipmentType;

			foreach (CommonConsol consol in Consols)
			{
				ZString countryToSearch;

				if (type == ShipmentTypes.Export && consol.LoadPort != null)
				{
					countryToSearch = consol.LoadPort.RL_RN_NKCountryCode;
				}
				else if (consol.DischargePort != null)
				{
					countryToSearch = consol.DischargePort.RL_RN_NKCountryCode;
				}
				else
				{
					countryToSearch = ZString.Empty;
				}

				if (countryToSearch == GlbCompany.CurrentCompany.GC_RN_NKCountryCode)
				{
					return consol;
				}
			}
			return null;
		}

		protected ShipmentTypes ShipmentType
		{
			get
			{
				if (Destination != null && Destination.RL_RN_NKCountryCode == GlbCompany.CurrentCompany.GC_RN_NKCountryCode)
				{
					return ShipmentTypes.Import;
				}

				if (Origin != null && Origin.RL_RN_NKCountryCode == GlbCompany.CurrentCompany.GC_RN_NKCountryCode)
				{
					return ShipmentTypes.Export;
				}

				return ShipmentTypes.Other;
			}
		}

		protected internal virtual bool ShipmentTravellingInReverseOfAConsol()
		{
			bool isInReverse = false;

			if (!JS_RL_NKOrigin.IsEmpty && !JS_RL_NKDestination.IsEmpty && JS_RL_NKOrigin != JS_RL_NKDestination)
			{
				for (int count = 0; count < Consols.Count && !isInReverse; count++)
				{
					CommonConsol consol = Consols[count];

					if (consol.JK_JX_JA_RL_NKPortOfLoading == JS_RL_NKDestination &&
						consol.JK_JX_JB_RL_NKPortOfDischarge == JS_RL_NKOrigin)
					{
						isInReverse = true;
					}
				}
			}

			return isInReverse;
		}

		#endregion

		#region Containers

		public CommonContainerCollection ArrivalContainers
		{
			get { return ArrivalConsol != null ? ContainersOnConsol(ArrivalConsol) : new EmptyReadOnlyContainerCollection(Factory); }
		}

		public CommonContainerCollection AllArrivalContainers
		{
			get { return ArrivalConsol != null ? ArrivalConsol.Containers : new EmptyReadOnlyContainerCollection(Factory); }
		}

		public CommonContainerCollection DepartureContainers
		{
			get { return DepartureConsol != null ? ContainersOnConsol(DepartureConsol) : new EmptyReadOnlyContainerCollection(Factory); }
		}

		public CommonContainerCollection AllDepartureContainers
		{
			get { return DepartureConsol != null ? DepartureConsol.Containers : new EmptyReadOnlyContainerCollection(Factory); }
		}

		#region EmptyReadOnlyContainerCollection
#if DEBUG
		public
#endif
		class EmptyReadOnlyContainerCollection : CommonContainerCollection
		{
			public EmptyReadOnlyContainerCollection(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			protected override bool AllowNewCore
			{
				get { return false; }
			}
		}

		#endregion

		public CommonContainerCollection ContainersOnConsol(CommonConsol consol)
		{
			CommonContainerCollection containers = new CommonContainerCollection(consol, Factory);
			containers.UpdatePackLineContainerOnAdd = false;
			foreach (PackLine packLine in OuterPackLines)
			{
				foreach (CommonContainer aContainer in packLine.Containers)
				{
					if (aContainer.JC_JK == consol.PK && !containers.Contains(aContainer.PK))
					{
						containers.Add(aContainer);
					}
				}
			}

			containers.Sort(CommonContainer.Schema.JC_ContainerNum, System.ComponentModel.ListSortDirection.Ascending);
			return containers;
		}

		#endregion

		#region Customs Entry Numbers

		[BusinessObjectTestExclude()]
		public ZString ECNOrCAN // Australia Only
		{
			get
			{
				ZString result = "";
				ZString numberType = CustomsEntryNumberType;
				if (numberType == CusEntryNumberTypes.Australia.ECN || numberType == CANType.CustomsAuthorityNumber.Code)
				{
					result = CustomsEntryNumber;
				}
				else if (ShipmentCustomsEntryNumber.IsExemptionCode(numberType))
				{
					result = numberType;
				}

				return result;
			}
		}

		#region Type

		[BusinessObjectTestExclude]
		[ActionField(MaxLength = 3)]
		[List("ShipmentCustomsEntryNumber.EntryType_List")]
		public virtual ZString CustomsEntryNumberType
		{
			get { return ShipmentCustomsEntryNumber.EntryType; }
			set { ShipmentCustomsEntryNumber.EntryType = value; }
		}

		public ZPropertyInfo CustomsEntryNumberTypeInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CustomsEntryNumberType, (p) => ShipmentCustomsEntryNumber.EntryTypeInfo); }
		}

		#endregion

		#region Number

		[ActionField(MaxLength = 35)]
		public virtual ZString CustomsEntryNumber
		{
			get { return ShipmentCustomsEntryNumber.EntryNumber; }
			set { ShipmentCustomsEntryNumber.EntryNumber = value; }
		}

		public virtual ZPropertyInfo CustomsEntryNumberInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CustomsEntryNumber, (p) => ShipmentCustomsEntryNumber.EntryNumberInfo); }
		}

		#endregion

		#region All Numbers

		[ChildEditable(false)]
		public CusEntryNumCollection CusEntryNumbers
		{
			get
			{
				if (fCusEntryNumbers == null)
				{
					fCusEntryNumbers = GetLoadedCusEntryNumbers(false);
					fCusEntryNumbers.Sort(new CusEntryNumbersComparer());
					fCusEntryNumbers.IsManagedForDataRefresh = true;
					RegisterEditableChildObject(fCusEntryNumbers);
				}

				return fCusEntryNumbers;
			}
		}
		CusEntryNumCollection fCusEntryNumbers;

		class CusEntryNumbersComparer : IComparer<CusEntryNumber>
		{
			public int Compare(CusEntryNumber cusEntryNumber1, CusEntryNumber cusEntryNumber2)
			{
				return AdjustedCE_EntryType(cusEntryNumber1.CE_EntryType).CompareTo(AdjustedCE_EntryType(cusEntryNumber2.CE_EntryType));
			}
		}

		static string AdjustedCE_EntryType(ZString entryType)
		{
			return string.Format("{0}{1}", entryType == CanadaAdditionalReferenceNumberTypes.Codes.CTN ? "A" : "B", entryType);
		}

		public CusEntryNumCollection CusEntryNumbersForAllCountries
		{
			get
			{
				if (fCusEntryNumbersForAllCountries == null)
				{
					fCusEntryNumbersForAllCountries = GetLoadedCusEntryNumbers(true);
				}
				return fCusEntryNumbersForAllCountries;
			}
		}
		CusEntryNumCollection fCusEntryNumbersForAllCountries;

		CusEntryNumCollection GetLoadedCusEntryNumbers(bool loadForAllCountries)
		{
			var result = GetLoadedCusEntryNumbers(loadForAllCountries, false);
			if (!loadForAllCountries && !result.Any() && AllowLoadCusEntryNumbersForRelatedCountries)
			{
				result = GetLoadedCusEntryNumbers(loadForAllCountries, true);
			}

			return result;
		}

		CusEntryNumCollection GetLoadedCusEntryNumbers(bool loadForAllCountries, bool includeRelatedCountries)
		{
			var parentIDClearedOnlyFilters = new Dictionary<string, ZQuery>();

			var areAnyObjectsInDatabase = IsInDatabase;
			areAnyObjectsInDatabase |= AddDeclarationCusEntryNumbersQuery(parentIDClearedOnlyFilters);
			if (JS_IsForwardRegistered)
			{
				areAnyObjectsInDatabase |= AddCusHAWBsCusEntryNumbersQuery(parentIDClearedOnlyFilters);
				areAnyObjectsInDatabase |= AddCusSCAHouseCusEntryNumbersQuery(parentIDClearedOnlyFilters);
			}

			areAnyObjectsInDatabase |= AddNCTSHeadersCusEntryNumbersQuery(parentIDClearedOnlyFilters);

			var parentIDClearedOnlyFilter = new ZQuery(CusEntryNumSchema.CE_ParentID, PK);
			parentIDClearedOnlyFilter.DefaultJoinCondition = JoinCondition.Or;
			foreach (var pair in parentIDClearedOnlyFilters)
			{
				parentIDClearedOnlyFilter.AddToFilter(pair.Value);
			}

			var query = includeRelatedCountries ? new ZDBOnlyQuery(typeof(CusEntryNumber)) : new ZQuery() { FetchOnlyFromLocalCache = !areAnyObjectsInDatabase };
			query.AddToFilter(parentIDClearedOnlyFilter);
			query.AddToFilter(JoinCondition.And, CusEntryNumSchema.CE_Category, CusEntryNumber.Categories.CustomsPermitClearanceNumber);

			if (!loadForAllCountries)
			{
				if (includeRelatedCountries)
				{
					AddEURelatedCountriesCusEntryNumbersQuery(query);
					AddUSRelatedCountriesCusEntryNumbersQuery(query);
				}
				else
				{
					query.AddToFilter(JoinCondition.And, CusEntryNumSchema.CE_RN_NKCountryCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
				}

				AddCusEntryNumberForCanadianExport(query);
			}

			var result = GetNewCusEntryNumCollection(query);
			result.Load();

			return result;
		}

		bool AddDeclarationCusEntryNumbersQuery(Dictionary<string, ZQuery> parentIDClearedOnlyFilters)
		{
			var areAnyObjectsInDatabase = false;

			foreach (var declaration in Declarations)
			{
				areAnyObjectsInDatabase |= declaration.IsInDatabase;
				var filterProvider = declaration as ICusEntryNumFilterProvider;
				var query = filterProvider.ValidCusEntryNumFilter;
				if (!query.IsNoResultQuery)
				{
					var literalTextADO = query.LiteralTextADO;
					if (!parentIDClearedOnlyFilters.ContainsKey(literalTextADO))
					{
						parentIDClearedOnlyFilters.Add(literalTextADO, query);
					}
				}
			}

			return areAnyObjectsInDatabase;
		}

		bool AddCusHAWBsCusEntryNumbersQuery(Dictionary<string, ZQuery> parentIDClearedOnlyFilters)
		{
			var areAnyObjectsInDatabase = false;

			var cusHawbQuery = new ZQuery(CusHAWBSchema.CS_JS, PK);
			cusHawbQuery.FetchOnlyFromLocalCache = !IsInDatabase;

			var hawbs = Factory.Load<Shared.ICusHAWB>(cusHawbQuery);
			foreach (var cusHAWB in hawbs)
			{
				var filterProvider = cusHAWB as ICusEntryNumFilterProvider;
				if (filterProvider != null)
				{
					var query = filterProvider.ValidCusEntryNumFilter;
					if (query != null && !query.IsNoResultQuery)
					{
						areAnyObjectsInDatabase |= cusHAWB.IsInDatabase;
						var literalTextADO = query.LiteralTextADO;
						if (!parentIDClearedOnlyFilters.ContainsKey(literalTextADO))
						{
							parentIDClearedOnlyFilters.Add(literalTextADO, query);
						}
					}
				}
			}

			return areAnyObjectsInDatabase;
		}

		bool AddCusSCAHouseCusEntryNumbersQuery(Dictionary<string, ZQuery> parentIDClearedOnlyFilters)
		{
			var areAnyObjectsInDatabase = false;
			var cusSCAHouseQuery = new ZQuery(CusSCAHouseSchema.CA_JS, PK);
			cusSCAHouseQuery.FetchOnlyFromLocalCache = !IsInDatabase;

			var scaHouseBills = Factory.Load<Shared.IBaseCusSCAHouse>(cusSCAHouseQuery);
			foreach (BusinessObject houseBill in scaHouseBills)
			{
				var filterProvider = houseBill as ICusEntryNumFilterProvider;
				if (filterProvider != null)
				{
					ZQuery query = filterProvider.ValidCusEntryNumFilter;
					if (query != null && !query.IsNoResultQuery)
					{
						areAnyObjectsInDatabase |= houseBill.IsInDatabase;
						var literalTextADO = query.LiteralTextADO;
						if (!parentIDClearedOnlyFilters.ContainsKey(literalTextADO))
						{
							parentIDClearedOnlyFilters.Add(literalTextADO, query);
						}
					}
				}
			}

			return areAnyObjectsInDatabase;
		}

		bool AddNCTSHeadersCusEntryNumbersQuery(Dictionary<string, ZQuery> parentIDClearedOnlyFilters)
		{
			var areAnyObjectsInDatabase = false;

			var nctsHeaderQuery = new ZQuery(CusInBondHeaderSchema.BH_ParentID, PK);
			nctsHeaderQuery.AddToFilter(CusInBondHeaderSchema.BH_ApplicationCode, new[] { CusInBondApplicationCodeList.Codes.NCTS4, CusInBondApplicationCodeList.Codes.NCTS5 });
			nctsHeaderQuery.FetchOnlyFromLocalCache = !IsInDatabase;

			var nctsHeaders = Factory.Load<INctsHeader>(nctsHeaderQuery);
			foreach (BusinessObject nctsHeader in nctsHeaders)
			{
				var filterProvider = nctsHeader as ICusEntryNumFilterProvider;
				if (filterProvider != null)
				{
					var query = filterProvider.ValidCusEntryNumFilter;
					if (query != null && !query.IsNoResultQuery)
					{
						areAnyObjectsInDatabase |= nctsHeader.IsInDatabase;
						var literalTextADO = query.LiteralTextADO;
						if (!parentIDClearedOnlyFilters.ContainsKey(literalTextADO))
						{
							parentIDClearedOnlyFilters.Add(literalTextADO, query);
						}
					}
				}
			}

			return areAnyObjectsInDatabase;
		}

		void AddEURelatedCountriesCusEntryNumbersQuery(ZQuery query)
		{
			if (GlbCompany.CurrentCompany.Country.RN_EconomicGrouping == EconomicGroupList.Codes.EuropeanUnion)
			{
				var euCountries = Factory.Load<RefCountry>(new ZQuery(RefCountrySchema.RN_EconomicGrouping, GlbCompany.CurrentCompany.Country.RN_EconomicGrouping));
				var euCountryCodes = euCountries.Select(c => c.RN_Code);
				query.AddToFilter(JoinCondition.And, CusEntryNumSchema.CE_RN_NKCountryCode, euCountryCodes);
			}
		}

		void AddUSRelatedCountriesCusEntryNumbersQuery(ZQuery query)
		{
			if (Constants.CountryCodes.IsUsaOrTerritory(GlbCompany.CurrentCompany.GC_RN_NKCountryCode))
			{
				query.AddToFilter(JoinCondition.And, CusEntryNumSchema.CE_RN_NKCountryCode, new[] { Constants.CountryCodes.UnitedStates, GlbCompany.CurrentCompany.GC_RN_NKCountryCode.ToString() });
			}
		}

		void AddCusEntryNumberForCanadianExport(ZQuery query)
		{
			if (this.IsExport() && GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Constants.CountryCodes.Canada)
			{
				var declarations = Declarations.Where(x => x.IsExport && x.IsDeclarationMatchSpecificCountry(Constants.CountryCodes.Canada)).ToArray();
				var declaration = declarations.Length == 1 ? declarations[0] : null;
				if (declaration == null && declarations.Length > 1)
				{
					declaration = declarations.FirstOrDefault(x => x.CompanyPK == GlbCompany.CurrentCompany.PK);
				}

				CusEntryNumber por = null;
				if (declaration != null && declaration is CA.IJobDeclaration)
				{
					var caDeclaration = (CA.IJobDeclaration)declaration;
					por = (CusEntryNumber)caDeclaration.JE_CAEDProofOfReportCusEntryNumber;
				}

				if (por == null || por.CE_EntryNum.IsEmpty)
				{
					por = CusEntryNumber.Load(this, CanadaAdditionalReferenceNumberTypes.Codes.CTN, Core.Constants.CountryCodes.Canada);
				}

				if (por != null && !por.CE_EntryNum.IsEmpty)
				{
					query.AddToFilter(JoinCondition.Or, CusEntryNumSchema.PK, por.PK);
				}
			}
		}

		bool AllowLoadCusEntryNumbersForRelatedCountries
		{
			get
			{
				return GlbCompany.CurrentCompany.Country.RN_EconomicGrouping == EconomicGroupList.Codes.EuropeanUnion
					|| (GlbCompany.CurrentCompany.GC_RN_NKCountryCode != Constants.CountryCodes.UnitedStates && Constants.CountryCodes.IsUsaOrTerritory(GlbCompany.CurrentCompany.GC_RN_NKCountryCode) && !UseImportEntryTypeList);
			}
		}

		public void ResetCusEntryNumbers()
		{
			if (fCusEntryNumbers != null)
			{
				UnRegisterEditableChildObject(fCusEntryNumbers);
				ShipmentCustomsEntryNumber.Reset();
				fCusEntryNumbers = null;
			}
		}

		protected virtual CusEntryNumCollection GetNewCusEntryNumCollection(ZQuery filter)
		{
			return new CusEntryNumCollection(Factory, filter);
		}

		#endregion

		#region Issue Date

		public ZDateTime CustomsEntryNumberIssueDate
		{
			get { return ShipmentCustomsEntryNumber.IssueDate; }
			set { ShipmentCustomsEntryNumber.IssueDate = value; }
		}

		public void UpdateCustomsEntryIssueDate(ZString numberType, ZString countryCode, ZDateTime dateTime)
		{
			RefCountry country = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, countryCode);
			if (country != null)
			{
				CusEntryNumber number = GetCusEntryNumber(numberType, country, CusEntryNumber.Categories.CustomsPermitClearanceNumber);
				if (number != null)
				{
					number.CE_IssueDate = dateTime;
				}
			}
		}

		public ZPropertyInfo CustomsEntryNumberIssueDateInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CustomsEntryNumberIssueDate, (p) => ShipmentCustomsEntryNumber.IssueDateInfo); }
		}

		#endregion

		#region Expiry Date

		public ZDateTime CustomsEntryNumberExpiryDate
		{
			get { return ShipmentCustomsEntryNumber.ExpiryDate; }
			set { ShipmentCustomsEntryNumber.ExpiryDate = value; }
		}

		public ZPropertyInfo CustomsEntryNumberExpiryDateInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CustomsEntryNumberExpiryDate, (p) => ShipmentCustomsEntryNumber.ExpiryDateInfo); }
		}

		#endregion

		public bool CustomsEntryNumberTypeIsAnExemptionCode
		{
			get { return ShipmentCustomsEntryNumber.IsExemptionCode(CustomsEntryNumberType); }
		}

		public ShipmentCustomsEntryNumber ShipmentCustomsEntryNumber
		{
			get
			{
				if (shipmentCustomsEntryNumber == null)
				{
					shipmentCustomsEntryNumber = GetNewShipmentCustomsEntryNumber();
					RegisterEditableChildObject(shipmentCustomsEntryNumber);
				}

				return shipmentCustomsEntryNumber;
			}
		}

		ShipmentCustomsEntryNumber shipmentCustomsEntryNumber;

		protected virtual ShipmentCustomsEntryNumber GetNewShipmentCustomsEntryNumber()
		{
			return new CommonShipmentCustomsEntryNumber(this);
		}

		#region Community Transit Status - EU only - ask Daniel

		[List("Lookups.CommunityTransitStatusCodes")]
		public override ZString JS_CommunityTransitStatus
		{
			get { return base.JS_CommunityTransitStatus; }
			set { base.JS_CommunityTransitStatus = value; }
		}

		#endregion

		protected internal void NotifyElementChanged()
		{
			OnElementChanged();
		}

		#endregion

		#region InnerPackLines

		[ChildEditable(false)]
		public InnerPackLineCollection InnerPackLines
		{
			get
			{
				if (innerPackLines == null)
				{
					innerPackLines = GetNewInnerPackLinesCollection();
					if (!IsDeleted)
					{
						innerPackLines.Load();
						AddDefaultInnerPackLineIfRequired();

						RegisterEditableChildObject(innerPackLines, "InnerPackLines");
						if (!IsDeleted && !ReadOnly && !IsPropertyReadOnlyDueToPhase("InnerPackLines"))
						{
							innerPackLines.SetReadOnlyIncludingChildren(IsMasterShipmentRepresentingAllChildShipments);
						}
					}
				}

				if (reloadInnerPackLines)
				{
					reloadInnerPackLines = false;
					innerPackLines.Load();
				}

				return innerPackLines;
			}
		}
		protected InnerPackLineCollection innerPackLines;

		protected internal void ReloadInnerPackLines()
		{
			CommonShipment currentShipment = this;

			List<CommonShipment> meAndAllMyParents = new List<CommonShipment>();
			while (currentShipment != null && !meAndAllMyParents.Contains(currentShipment))
			{
				currentShipment.reloadInnerPackLines = true;
				meAndAllMyParents.Add(currentShipment);
				currentShipment = currentShipment.CoLoadMasterShipment;
			}
		}
		bool reloadInnerPackLines;

		protected virtual InnerPackLineCollection GetNewInnerPackLinesCollection()
		{
			return new InnerPackLineCollection(this, Factory);
		}

		void AddDefaultInnerPackLineIfRequired()
		{
			if (!IsCloning && JS_TotalPackageCount > 0 && !IsCoLoadMaster && !IsBlindCoLoadMaster && !IsAssemblyMaster && InnerPackLines.Count == 0)
			{
				PackLine defaultInnerPackLine = InnerPackLines.AddNew();
				defaultInnerPackLine.JL_PackageCount = JS_TotalPackageCount;
				defaultInnerPackLine.JL_F3_NKPackType = JS_F3_NKTotalCountPackType;

				defaultInnerPackLine.JL_ActualWeight = JS_ActualWeight;
				defaultInnerPackLine.JL_ActualWeightUQ = JS_UnitOfWeight;

				defaultInnerPackLine.JL_ActualVolume = JS_ActualVolume;
				defaultInnerPackLine.JL_ActualVolumeUQ = JS_UnitOfVolume;

				defaultInnerPackLine.JL_LoadingMeters = JS_LoadingMeters;
				defaultInnerPackLine.HasChanges = false;
			}
		}

		public bool UpdatePackLines
		{
			get
			{
				return !IsCloning && !IsSettingDefaultValues && !IsUpdatingShipmentFromPackLines && !UpdatingShipmentFromRelated && CanHaveOwnPackLines && !SuppressPackLinesUpdate &&
					!BusinessObjectUniversalCopyFactoryService.IsRunningUniversalCopy(Factory);
			}
		}

		public bool SuppressPackLinesUpdate { get; set; }

		void UpdateRelevantPackLinesWeight(ZDecimal oldValue, ZDecimal newValue)
		{
			foreach (var notifiable in PackLinesNotifiables)
			{
				notifiable.NotifyWeightChanged(oldValue, newValue);
			}
		}

		void UpdateRelevantPackLinesWeightUQ(ZString oldValue, ZString newValue)
		{
			foreach (var notifiable in PackLinesNotifiables)
			{
				notifiable.NotifyWeightUQChanged(oldValue, newValue);
			}
		}

		void UpdateRelevantPackLinesVolume(ZDecimal oldValue, ZDecimal newValue)
		{
			foreach (var notifiable in PackLinesNotifiables)
			{
				notifiable.NotifyVolumeChanged(oldValue, newValue);
			}
		}

		void UpdateRelevantPackLinesVolumeUQ(ZString oldValue, ZString newValue)
		{
			foreach (var notifiable in PackLinesNotifiables)
			{
				notifiable.NotifyVolumeUQChanged(oldValue, newValue);
			}
		}

		void UpdateRelevantPackLoadingMeters(ZDecimal oldValue, ZDecimal newValue)
		{
			foreach (var notifiable in PackLinesNotifiables)
			{
				notifiable.NotifyLoadingMetersChanged(oldValue, newValue);
			}
		}

		void UpdateRelevantPackLinesDescription(ZString oldValue, ZString newValue)
		{
			foreach (var notifiable in PackLinesNotifiables)
			{
				notifiable.NotifyDescriptionChanged(oldValue, newValue);
			}
		}

		IEnumerable<IPackLineParentChangeNotifiable> PackLinesNotifiables
		{
			get { return PackLinesNotifiablesCore ?? Enumerable.Empty<IPackLineParentChangeNotifiable>(); }
		}

		protected virtual IEnumerable<IPackLineParentChangeNotifiable> PackLinesNotifiablesCore
		{
			get { yield return OuterPackLines; }
		}

		protected bool IsUpdatingShipmentFromPackLines { get; set; }

		internal bool UpdatingShipmentFromRelated { get; set; }

		protected ZDecimal GetPackLineTotalVolume(PackLineCollection packLines)
		{
			return Constants.Volume.Convert(packLines.TotalVolume, packLines.TotalVolumeUnit, ShipmentVolumeUnit, false);
		}

		protected ZDecimal GetPackLineTotalWeight(PackLineCollection packLines)
		{
			return Constants.Weight.Convert(packLines.TotalWeight, packLines.TotalWeightUnit, ShipmentWeightUnit, false);
		}

		protected ZDecimal GetPackLineTotalLoadingMeters(PackLineCollection packLines)
		{
			return Utilities.Round(packLines.TotalLoadingMeters, JobShipmentSchema.JS_LoadingMeters.Scale);
		}

		#endregion

		#region NoteTypes

		public override BusinessObject[] BusinessObjectsWithRelatedNotes
		{
			get
			{
				List<BusinessObject> result = new List<BusinessObject>(base.BusinessObjectsWithRelatedNotes);
				if (Consignee != null)
				{
					result.Add(Consignee);
				}
				if (Consignor != null)
				{
					result.Add(Consignor);
				}

				var declarations = Declarations.Cast<BusinessObject>().ToArray();   // Caching for perfomance: Declarations calls Factory.Load
				result.AddRange(declarations);
				result.AddRange(GetInvoicesForDeclarations(declarations));

				result.AddRange(Containers);

				var cartages = (BusinessObject[])Factory.Load<ICommonCartage>(new ZQuery(JobCartageSchema.JJ_ParentID, PK));
				result.AddRange(cartages);

				return result.ToArray();
			}
		}

		protected override StmNoteContexts NoteContextsForRelatedNotes
		{
			get
			{
				StmNoteContexts result = new StmNoteContexts();

				result = StmNoteContextUtils.GetContextFromTransportModeImportExport(Constants.GlobalModuleNamesConstants.Forwarding, "", JS_TransportMode, JS_PackingMode);
				result.Module |= base.NoteContextsForRelatedNotes.Module;
				result.Direction |= base.NoteContextsForRelatedNotes.Direction;
				result.FreightMode |= base.NoteContextsForRelatedNotes.FreightMode;

				if (Declarations.Length > 0)
				{
					result.Module |= StmNoteContextModule.D;
				}

				if (this.IsImport())
				{
					result.Direction |= StmNoteContextDirection.I;
				}

				if (this.IsExport())
				{
					result.Direction |= StmNoteContextDirection.E;
				}

				if (this.IsDomestic())
				{
					result.Direction |= StmNoteContextDirection.D;
				}

				if (this.IsImport() || this.IsExport())
				{
					result.Direction |= StmNoteContextDirection.B;
				}

				if (this.IsCrossTrade())
				{
					result.Direction |= StmNoteContextDirection.X;
				}

				return result;
			}
		}

		public IBaseJobDeclaration[] DeclarationsForNoteTypes
		{
			get
			{
				return CanHaveDeclarations
					? Declarations
					: LoadDeclarations(Factory.GetCachedReadOnlyFactory(), !JS_IsCancelled);
			}
		}

		#region Metadata Extensions

		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Metadata architecture requires array")]
		public IMetadata[] MetadataExtensions
		{
			get
			{
				var list = new List<IMetadata>();

				if (JS_IsForwardRegistered || JS_IsBooking)
				{
					list.Add(new ForwardingMetadataExtension());
				}

				if (JS_IsCFSRegistered)
				{
					list.Add(new GatePassMetadataExtension());
				}

				return list.ToArray();
			}
		}

		[MetadataContext(MetadataContext.ForwardingShipment)]
		public class ForwardingMetadataExtension : IMetadata, IForwardingShipmentShareProperty
		{
			public INoteTypeCollection NoteTypes { get; set; }

			public bool EnableBoleroEHBLIntegration
			{
				get => FreightDataRegistry.Instance.EnableBoleroEHBLIntegration.Value.EnableEBLIntegration;
			}
		}

		[MetadataContext(MetadataContext.GatePassShipment)]
		public class GatePassMetadataExtension : IMetadata
		{
			public INoteTypeCollection NoteTypes { get; set; }
		}

		#endregion

		#endregion

		#region Transports

		[ChildEditable(false)]
		public TransportCollection Transports
		{
			get
			{
				if (transports == null)
				{
					transports = GetNewTransportCollection();
					transports.Load();
					RegisterEditableChildObject(transports);
				}

				return transports;
			}
		}
		TransportCollection transports;

		protected virtual TransportCollection GetNewTransportCollection()
		{
			return new TransportCollection(this);
		}

		#endregion

		#region MostInterestingTransport

		public Transport MostInterestingTransport
		{
			get
			{
				if (mostInteresetingTransport == null)
				{
					mostInteresetingTransport = new CachedProperty<Transport>(Factory, delegate
					{
						TransportOrderHelper helper = new TransportOrderHelper(TransportsIncludingRelated);

						if (this.IsImport())
						{
							return helper.LastLegWithTransportMode(JS_TransportMode, null, null) ?? helper.LastLeg;
						}
						else
						{
							return helper.FirstLegWithTransportMode(JS_TransportMode) ?? helper.FirstLeg;
						}
					});
				}

				return mostInteresetingTransport.Value;
			}
		}

		CachedProperty<Transport> mostInteresetingTransport;

		#endregion

		#region Sailing

		public ZGuid SailingPK
		{
			get
			{
				ZGuid result = ZGuid.Empty;
				if (Consols.Count > 1)
				{
					if (this.IsExport() && DepartureConsol != null)
					{
						result = DepartureConsol.JK_JX_Sailing;
					}
					else if (ArrivalConsol != null)
					{
						result = ArrivalConsol.JK_JX_Sailing;
					}
					else
					{
						result = Consols[0].JK_JX_Sailing;
					}
				}
				else if (Consols.Count == 1)
				{
					result = Consols[0].JK_JX_Sailing;
				}
				else
				{
					result = JS_JX;
				}
				return result;
			}
		}

		public virtual JobSailing Sailing
		{
			get { return Factory.Load<JobSailing>(SailingPK); }
		}

		#endregion

		#region Numbers

		/// <summary>
		/// Additional Reference Numbers
		/// </summary>
		[ChildEditable(true)]
		public CusEntryNumAdditionalReferenceCollection Numbers
		{
			get
			{
				if (fNumbers == null)
				{
					fNumbers = new CusEntryNumAdditionalReferenceCollection(this);
					fNumbers.Load();
					RegisterEditableChildObject(fNumbers);
					OnNumbersLoaded();
					RaiseNumbersLoaded();
				}
				return fNumbers;
			}
		}
		CusEntryNumAdditionalReferenceCollection fNumbers;

		public ZString NumbersAsString => Numbers.AllNumbersAsString;

		protected virtual void OnNumbersLoaded()
		{
			foreach (var cusEntryNumber in Numbers.OfType<CusEntryNumber>())
			{
				if (AdditionalReferenceNumberCannotBeDeleted(cusEntryNumber))
				{
					AddCannotDeleteNumberHandler(cusEntryNumber, ResString.GetMultilingualString("E12EE871-E49D-4BAE-9151-E2BBA8EC3E8A", "The {0} is system generated and cannot be deleted.", cusEntryNumber.CE_EntryType));
				}
			}
		}

		protected virtual bool AdditionalReferenceNumberCannotBeDeleted(CusEntryNumber cusEntryNumber)
		{
			return cusEntryNumber.CE_EntryIsSystemGenerated && cusEntryNumber.CE_EntryType == CusEntryNumLookups.HIR;
		}

		protected void AddCannotDeleteNumberHandler(CusEntryNumber number, MultilingualString reasonForNotAbleToDelete)
		{
			if (number != null)
			{
				number.ReadOnly = true;
				number.CanDeleteHandler += (s, eventArgs) =>
				{
					eventArgs.CanDelete = false;
					eventArgs.ReasonForNotAbleToDelete = reasonForNotAbleToDelete;
				};
			}
		}

		public event EventHandler NumbersLoaded;

		void RaiseNumbersLoaded()
		{
			if (NumbersLoaded != null)
			{
				NumbersLoaded(this, EventArgs.Empty);
			}
		}

		#endregion

		#region Declarations

		public virtual IBaseJobDeclaration[] Declarations
		{
			get { return CanHaveDeclarations ? LoadDeclarations(Factory, !IsDeleted && !JS_IsCancelled) : Array.Empty<IBaseJobDeclaration>(); }
		}

		protected virtual bool CanHaveDeclarations
		{
			get { return true; }
		}

		protected IBaseJobDeclaration[] LoadDeclarations(BusinessObjectFactory factoryToBeUsedForDeclarations, bool activeOnly)
		{
			var filter = new ZQuery(JobDeclarationSchema.JE_JS, PK)
			{
				FetchOnlyFromLocalCache = !IsInDatabase,
				IgnoreActiveFilter = !activeOnly,
			};

			return factoryToBeUsedForDeclarations.Load<IBaseJobDeclaration>(filter);
		}

		BusinessObject[] GetEntryHeadersForDeclarations(BusinessObject[] declarations)
		{
			if (declarations.Any())
			{
				ZQuery filter = new ZQuery(CusEntryHeaderSchema.CH_JE, declarations.Select(declaration => declaration.PK));
				return (BusinessObject[])Factory.Load<ICusEntryHeader>(filter);
			}

			return Array.Empty<BusinessObject>();
		}

		BusinessObject[] GetInvoicesForDeclarations(BusinessObject[] declarations)
		{
			if (declarations.Any())
			{
				ZQuery filter = new ZQuery(JobComInvoiceHeaderSchema.JZ_JE, declarations.Select(declaration => declaration.PK));
				filter.AddToFilter(JobComInvoiceHeaderSchema.JZ_GroupInvoice, "N");

				return (BusinessObject[])Factory.Load<IBaseJobComInvoiceHeader>(filter);
			}

			return Array.Empty<BusinessObject>();
		}

		#endregion

		#endregion

		#region Properties

		public RefUNLOCO FreightPayableAt
		{
			get
			{
				if (IsPrepaid)
				{
					return Origin;
				}
				else if (IsCollect)
				{
					return Destination;
				}

				return null;
			}
		}

		#region BookedShippingLine

		public OrgHeader BookedShippingLine => (OrgHeader)JS_OA_BookedShippingLineAddress_ZAddress.OrgHeader;

		[List("Lookups.ShippingLine_List")]
		public ZGuid BookedShippingLinePK
		{
			get => JS_OA_BookedShippingLineAddress_ZAddress.OrgPK;
			set => JS_OA_BookedShippingLineAddress_ZAddress.OrgPK = value;
		}

		public ZPropertyInfo BookedShippingLinePKInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.BookedShippingLinePK, x => JS_OA_BookedShippingLineAddress_ZAddress.OrgPKInfo); }
		}

		protected override ZAddress GetNewJS_OA_BookedShippingLineAddress_ZAddress()
		{
			var result = base.GetNewJS_OA_BookedShippingLineAddress_ZAddress();
			result.DefaultAddressType = AddressType.OFC;
			return result;
		}

		public bool CarrierOfBookedShippingLineIsNVOCC
		{
			get
			{
				var isNVO = BookedShippingLine?.ShippingLine?.RSL_IsNVO ?? BookedShippingLine?.OH_IsSeaWholesaler ?? false;
				var isShippingLine = BookedShippingLine?.ShippingLine?.RSL_IsShippingLine ?? BookedShippingLine?.OH_IsShippingLine ?? false;

				return isNVO && (!isShippingLine || JS_PackingMode == Constants.ContainerModes.LCL);
			}
		}

		public bool CarrierOfBookedShippingLineIsCW1User
		{
			get
			{
				return BookedShippingLine?.ShippingLine?.RSL_IsCW1User ?? false;
			}
		}

		public bool CarrierOfBookedShippingLineHasBookingRequestIntegration
		{
			get
			{
				return BookedShippingLine?.ShippingLine?.RSL_BookingRequestAvailable ?? false;
			}
		}

		#endregion

		[ActionField(FieldType = ActionFieldType.Hidden)]
		[DocumentEngineIntegration.DocumentParsing.DocumentFieldExcludeFromMap]
		[WorkflowSetFieldReadonly]
		public override ZBool JS_IsShipping
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.JS_IsShipping; }
			[System.Diagnostics.DebuggerStepThrough]
			set
			{
				if (JS_IsShipping != value)
				{
					base.JS_IsShipping = value;

					if (!JS_IsShipping && IsInDatabase && (ZBool)JS_IsShippingInfo.OriginalValue && !IsSuppressedCheckReset)
					{
						ErrorReporter.ReportOnce("CommonShipmentShouldNotResetJS_IsShipping", "Once a shipment has been marked as shipping and saved it should not be unmarked");
					}
				}
			}
		}

		ZString InspectionTypeDebugLog = string.Empty;

		public void AddEmptyCE_EntryNumLogIfNeeded(CusEntryNumber number)
		{
			if (number != null && number.CE_EntryNum.IsEmpty)
			{
				var logString = new ZStringBuilder();
				logString.AppendLine($"No CE_EntryNum Found : {this.PK} Country: {number.CE_RN_NKCountryCode}, Type: {number.CE_EntryType}");
				logString.AppendLine(System.Environment.StackTrace);
				InspectionTypeDebugLog = logString.ToStringWithNewLineBetweenAppends();
				return;
			}

			InspectionTypeDebugLog = string.Empty;
		}

		#region Inspection Type Codes

		#region JS_InspectionTypeCode

		[BusinessObjectTestExclude]
		[List("Lookups.InspectionTypes")]
		[MaxLength(3)]
		public virtual ZString JS_InspectionTypeCode
		{
			get
			{
				var number = InspectionTypeCusEntryNumber ?? FallbackInspectionTypeCusEntryNumberForAllCountries;

				if (number != null && !Globals.IsWeb && number.CE_EntryNum == BaseJobShipmentLookups.InspectionType_Web)
				{
					((ILightValidationInternals)this).IsValid = false;
					return SupplyChainSecurityConfiguration.InspectionTypeDefault;
				}

				return number != null ? number.CE_EntryNum : SupplyChainSecurityConfiguration.InspectionTypeDefault;
			}
			set
			{
				ReportErrorWhenSettingApprovedInspectionTypeWhenImportingData(value);

				if (value != JS_InspectionTypeCode)
				{
					CheckMaximumLength(JS_InspectionTypeCodeInfo, value);

					if (value == SupplyChainSecurityConfiguration.InspectionTypeDefault && FallbackInspectionTypeCusEntryNumberForAllCountries == null)
					{
						if (InspectionTypeCusEntryNumber != null)
						{
							UnHookEntryNumber(InspectionTypeCusEntryNumber);
							InspectionTypeCusEntryNumbersForAllCountries.RemoveAndDelete(InspectionTypeCusEntryNumber);
						}
					}
					else
					{
						SetInspectionTypeCusEntryNumberValue(value);
					}

					if (!IsSettingDefaultValues)
					{
						JS_InspectionTypeCodeHasChanges = true;
					}

					if (!IsValidationSuspended)
					{
						Validation.ValidateJS_InspectionTypeCode();
					}

					JS_InspectionTypeCodeInfo.RefreshBinding();

					AddEmptyCE_EntryNumLogIfNeeded(this.InspectionTypeCusEntryNumber);
				}
			}
		}

		public void SetInspectionTypeCodeForCountry(string countryCode, string value)
		{
			if (countryCode == CountryOrEuropeanUnionCode)
			{
				JS_InspectionTypeCode = value;
			}
			else if (GetInspectionTypeCodeForCountry(countryCode) != value)
			{
				LoadOrCreateInspectionTypeCusEntryNumber(countryCode).CE_EntryNum = value;
			}
		}

		public ZString GetInspectionTypeCodeForCountry(string countryCode)
		{
			if (countryCode == CountryOrEuropeanUnionCode)
			{
				return JS_InspectionTypeCode;
			}

			return GetInspectionTypeCusEntryNumber(countryCode)?.CE_EntryNum ?? SupplyChainSecurityConfiguration.InspectionTypeDefault;
		}

		protected virtual void ReportErrorWhenSettingApprovedInspectionTypeWhenImportingData(ZString value)
		{
			if (value == BaseJobShipmentLookups.InspectionType_Approved
				&& IsSettingDefaultOrImportingData
				&& FreightDataRegistry.Instance.ShipmentInspectionTypeDefault.Value != BaseJobShipmentLookups.InspectionType_Approved
				&& JS_IsBooking
				&& !JS_IsForwardRegistered)
			{
				var error = string.Format("Should not set JS_InspectionTypeCode to Approved when importing and defaulting data.{0}Registry Setting for Default Inspection Type: {1}{0}Registry Setting for Shipment Inspection Organisation To Use: {2}{0}StackTrace: {3}", System.Environment.NewLine, FreightDataRegistry.Instance.ShipmentInspectionTypeDefault.Value, FreightDataRegistry.Instance.ShipmentInspectionOrganisationToUse.Value, System.Environment.StackTrace);
				ErrorReporter.ReportOnce("{4D8BBEBB-366B-45CF-93E0-EAF886340632}", error);
			}
		}

		public virtual void UpdateInspectionTypeFromPackLines() { }

		public ZPropertyInfo JS_InspectionTypeCodeInfo
		{
			get { return GetZPropertyInfo(Schema.JS_InspectionTypeCode); }
		}

		public bool JS_InspectionTypeCodeHasChanges { get; set; }

		CusEntryNumber InspectionTypeCusEntryNumber
		{
			get { return GetInspectionTypeCusEntryNumber(CountryOrEuropeanUnionCode); }
		}

		public CusEntryNumber GetInspectionTypeCusEntryNumber(string countryCode)
		{
			return InspectionTypeCusEntryNumbersForAllCountries.Cast<CusEntryNumber>().FirstOrDefault(x => x.CE_RN_NKCountryCode == countryCode);
		}

		CusEntryNumber FallbackInspectionTypeCusEntryNumberForAllCountries
		{
			get { return InspectionTypeCusEntryNumbersForAllCountries.Cast<CusEntryNumber>().FirstOrDefault(x => x.CE_RN_NKCountryCode.IsEmpty); }
		}

		[ChildEditable(true)]
		CusEntryNumCollection InspectionTypeCusEntryNumbersForAllCountries => inspectionTypeCusEntryNumbersForAllCountries ?? (inspectionTypeCusEntryNumbersForAllCountries = GetInspectionTypeCusEntryNumberCollection(CusEntryNumber.EntryType.InspectionStatus));
		CusEntryNumCollection inspectionTypeCusEntryNumbersForAllCountries;

		void SetInspectionTypeCusEntryNumberValue(ZString value)
		{
			var number = LoadOrCreateInspectionTypeCusEntryNumber();
			number.CE_EntryNum = value;
		}

		CusEntryNumber LoadOrCreateInspectionTypeCusEntryNumber()
		{
			return LoadOrCreateInspectionTypeCusEntryNumber(CountryOrEuropeanUnionCode);
		}

		public CusEntryNumber LoadOrCreateInspectionTypeCusEntryNumber(ZString countryCode)
		{
			var number = GetInspectionTypeCusEntryNumber(countryCode);
			if (number == null)
			{
				number = InspectionTypeCusEntryNumbersForAllCountries.AddNew();
				number.CE_ParentTable = TableName;
				number.CE_Category = CusEntryNumber.Categories.InspectionStatus;
				number.CE_EntryType = CusEntryNumber.EntryType.InspectionStatus;
				number.CE_EntryIsSystemGenerated = false;
				number.CE_ParentID = PK;
				number.CE_RN_NKCountryCode = countryCode;
				HookEntryNumber(number);
			}

			return number;
		}

		void ConvertWebCreatedInspectionType()
		{
			if (!Globals.IsWeb)
			{
				CusEntryNumber number = InspectionTypeCusEntryNumber ?? FallbackInspectionTypeCusEntryNumberForAllCountries;
				if (number != null && number.CE_EntryNum == BaseJobShipmentLookups.InspectionType_Web)
				{
					if (InspectionTypeCusEntryNumber != null && FallbackInspectionTypeCusEntryNumberForAllCountries == null)
					{
						UnHookEntryNumber(InspectionTypeCusEntryNumber);
						InspectionTypeCusEntryNumbersForAllCountries.RemoveAndDelete(InspectionTypeCusEntryNumber);
					}
					else if (InspectionTypeCusEntryNumber != null && FallbackInspectionTypeCusEntryNumberForAllCountries != null)
					{
						InspectionTypeCusEntryNumber.CE_EntryNum = JS_InspectionTypeCode;
					}
					else if (InspectionTypeCusEntryNumber == null && FallbackInspectionTypeCusEntryNumberForAllCountries != null)
					{
						SetInspectionTypeCusEntryNumberValue(JS_InspectionTypeCode);
					}
				}
			}
		}

		public virtual bool SetApprovedShipperStatus(ZString reason, bool overrideUserEnteredValue = false)
		{
			return true;
		}

		public virtual bool SupportsPackLineApprovedCode => false;

		public virtual bool RequiresSecuredCargoFromWarehouse => false;

		public IDisposable SetIsMarkingPackLinesAsSecuredAllowed()
		{
			return new DisposableAction(() => isMarkingPackLinesAsSecuredAllowedIndex++, () => isMarkingPackLinesAsSecuredAllowedIndex--);
		}

		public bool IsMarkingPackLinesAsSecuredAllowed => isMarkingPackLinesAsSecuredAllowedIndex > 0;

		int isMarkingPackLinesAsSecuredAllowedIndex;

		#region Redefaulting Inspection Type Codes Suspender

		public IDisposable SuspendRedefaultingInspectionTypeCodes()
		{
			return new DisposableAction(() => isRedefaultingInspectionTypeCodesIndex++, () => isRedefaultingInspectionTypeCodesIndex--);
		}

		public bool IsRedefaultingInspectionTypeCodesSuspended => isRedefaultingInspectionTypeCodesIndex > 0;

		int isRedefaultingInspectionTypeCodesIndex;

		#endregion

		#endregion

		#region JS_AdditionalInspectionTypeCode

		[List("Lookups.AdditionalInspectionTypes")]
		[MaxLength(3)]
		public virtual ZString JS_AdditionalInspectionTypeCode
		{
			get
			{
				var number = AdditionalInspectionTypeCusEntryNumber ?? FallbackAdditionalInspectionTypeCusEntryNumberForAllCountries;
				return number != null ? number.CE_EntryNum : (ZString)FreightDataRegistry.AviationSecurity_Unknown_Code;
			}
			set
			{
				if (value != JS_AdditionalInspectionTypeCode)
				{
					CheckMaximumLength(JS_AdditionalInspectionTypeCodeInfo, value);

					if (value == FreightDataRegistry.AviationSecurity_Unknown_Code && FallbackAdditionalInspectionTypeCusEntryNumberForAllCountries == null)
					{
						if (AdditionalInspectionTypeCusEntryNumber != null)
						{
							UnHookEntryNumber(AdditionalInspectionTypeCusEntryNumber);
							AdditionalInspectionTypeCusEntryNumbersForAllCountries.RemoveAndDelete(AdditionalInspectionTypeCusEntryNumber);
						}
					}
					else
					{
						SetAdditionalInspectionTypeCusEntryNumberValue(value);
					}

					if (!IsSettingDefaultValues)
					{
						JS_AdditionalInspectionTypeCodeHasChanges = true;
					}

					if (!IsValidationSuspended)
					{
						Validation.ValidateJS_AdditionalInspectionTypeCode();
					}

					JS_AdditionalInspectionTypeCodeInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo JS_AdditionalInspectionTypeCodeInfo => GetZPropertyInfo(Schema.JS_AdditionalInspectionTypeCode);

		public bool JS_AdditionalInspectionTypeCodeHasChanges { get; set; }

		CusEntryNumber AdditionalInspectionTypeCusEntryNumber
		{
			get { return AdditionalInspectionTypeCusEntryNumbersForAllCountries.Cast<CusEntryNumber>().FirstOrDefault(x => x.CE_RN_NKCountryCode == CountryOrEuropeanUnionCode); }
		}

		CusEntryNumber FallbackAdditionalInspectionTypeCusEntryNumberForAllCountries
		{
			get { return AdditionalInspectionTypeCusEntryNumbersForAllCountries.Cast<CusEntryNumber>().FirstOrDefault(x => x.CE_RN_NKCountryCode.IsEmpty); }
		}

		[ChildEditable(true)]
		CusEntryNumCollection AdditionalInspectionTypeCusEntryNumbersForAllCountries => additionalInspectionTypeCusEntryNumbersForAllCountries ?? (additionalInspectionTypeCusEntryNumbersForAllCountries = GetInspectionTypeCusEntryNumberCollection(CusEntryNumber.EntryType.AdditionalInspectionStatus));
		CusEntryNumCollection additionalInspectionTypeCusEntryNumbersForAllCountries;

		void SetAdditionalInspectionTypeCusEntryNumberValue(ZString value)
		{
			var number = LoadOrCreateAdditionalInspectionTypeCusEntryNumber();
			number.CE_EntryNum = value;
		}

		CusEntryNumber LoadOrCreateAdditionalInspectionTypeCusEntryNumber()
		{
			var number = AdditionalInspectionTypeCusEntryNumber;
			if (number == null)
			{
				number = AdditionalInspectionTypeCusEntryNumbersForAllCountries.AddNew();
				number.CE_ParentTable = TableName;
				number.CE_Category = CusEntryNumber.Categories.InspectionStatus;
				number.CE_EntryType = CusEntryNumber.EntryType.AdditionalInspectionStatus;
				number.CE_EntryIsSystemGenerated = false;
				number.CE_ParentID = PK;
				number.CE_RN_NKCountryCode = CountryOrEuropeanUnionCode;
				HookEntryNumber(number);
			}

			return number;
		}

		#endregion

		CusEntryNumCollection GetInspectionTypeCusEntryNumberCollection(string securityInspectionStatusCusEntryType)
		{
			ZQuery filter = new ZQuery();
			filter.AddToFilter(CusEntryNumSchema.CE_ParentID, PK);
			filter.AddToFilter(CusEntryNumSchema.CE_Category, CusEntryNumber.Categories.InspectionStatus);
			filter.AddToFilter(CusEntryNumSchema.CE_EntryType, securityInspectionStatusCusEntryType);

			var collection = new CusEntryNumCollection(Factory, filter);
			collection.IsManagedForDataRefresh = true;
			collection.Load();
			collection.CountChanged += CusEntryNumbersCollectionChanged;

			foreach (CusEntryNumber cusEntryNumber in collection)
			{
				HookEntryNumber(cusEntryNumber);
			}

			RegisterEditableChildObject(collection);

			return collection;
		}

		ZString CountryOrEuropeanUnionCode
		{
			get
			{
				return SupplyChainSecurityConfiguration.LicenceEconomicGroupingCode.IsEmpty ? GlbCompany.CurrentCompany.GC_RN_NKCountryCode : SupplyChainSecurityConfiguration.LicenceEconomicGroupingCode;
			}
		}

		void HookEntryNumber(CusEntryNumber number)
		{
			number.CE_EntryNumInfo.ValueChanged += CusEntryNumberChanged;
		}

		void UnHookEntryNumber(CusEntryNumber number)
		{
			number.CE_EntryNumInfo.ValueChanged -= CusEntryNumberChanged;
		}

		void CusEntryNumberChanged(object sender, EventArgs e)
		{
			JS_InspectionTypeCodeInfo.RefreshBinding();
			JS_AdditionalInspectionTypeCodeInfo.RefreshBinding();
		}

		void CusEntryNumbersCollectionChanged(object sender, CollectionCountChangedEventArgs e)
		{
			if (e.ItemAdded)
			{
				HookEntryNumber((CusEntryNumber)e.BizObject);
			}

			if (e.ItemRemoved)
			{
				UnHookEntryNumber((CusEntryNumber)e.BizObject);
			}

			JS_InspectionTypeCodeInfo.RefreshBinding();
			JS_AdditionalInspectionTypeCodeInfo.RefreshBinding();
		}

		#endregion

		#region JS_HBLAWBChargesDisplay

		[List(nameof(ChargesApplyLookup))]
		public override ZString JS_HBLAWBChargesDisplay
		{
			get { return base.JS_HBLAWBChargesDisplay; }
			set { base.JS_HBLAWBChargesDisplay = value; }
		}

		#endregion

		#region JS_HBLContainerPackModeOverride

		[List("Lookups.JS_HBLContainerPackModeOverride_List")]
		public override ZString JS_HBLContainerPackModeOverride
		{
			get { return base.JS_HBLContainerPackModeOverride; }
			set
			{
				if (base.JS_HBLContainerPackModeOverride != value)
				{
					base.JS_HBLContainerPackModeOverride = value;
				}
			}
		}

		#endregion

		#region JS_RL_NKOrigin

		[List("Lookups.RefUNLOCO_List")]
		public override ZString JS_RL_NKOrigin
		{
			get { return IsDeleted ? ZString.Empty : base.JS_RL_NKOrigin; }
			set
			{
				base.JS_RL_NKOrigin = value;

				SetIsDomestic();

				if (!IsSettingDefaultOrImportingData)
				{
					SetDefaultExportBroker();
					DefaultPickupCompany();
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateJS_OH_ImportBroker();
					Validation.ValidateJS_HBLAWBChargesDisplay();
					Validation.ValidateJS_CommunityTransitStatus();
				}

				using (SuspendSettingOriginFromConsignorLocation())
				{
					if (!ConsignorPickupAddress.E2_AddressOverride)
					{
						ConsignorPickupAddress.UpdateDefaultAddress(Origin);
					}

					if (!IsSettingConsignorFromOriginSuspended && !ConsignorDocumentaryAddress.E2_AddressOverride && !fIsImportingData)
					{
						ConsignorDocumentaryAddress.UpdateDefaultAddress(Origin);
					}

					MarkDocsAndCartageAsNeedsValidation();
					Transports.MarkAsNeedingValidation();
				}

				RefreshCalculatedVolumeWeightValues();
				DocsAndCartage.RequiredDocuments.SetAllDocumentsReceivedEventLogger();
			}
		}

		public IDisposable SuspendSettingOriginFromConsignorLocation()
		{
			suspendSettingOriginFromConsignorLocation = true;

			return new DisposableAction(() => suspendSettingOriginFromConsignorLocation = false);
		}

		bool suspendSettingOriginFromConsignorLocation;

		#endregion

		#region JS_RL_NKDestination

		[List("Lookups.RefUNLOCO_List")]
		public override ZString JS_RL_NKDestination
		{
			get { return IsDeleted ? ZString.Empty : base.JS_RL_NKDestination; }
			set
			{
				base.JS_RL_NKDestination = value;

				SetIsDomestic();

				if (!IsSettingDefaultOrImportingData)
				{
					SetDefaultImportBroker();
					DefaultDeliveryCompany();
					SetDefaultExportBroker();
				}

				using (SuspendSettingDestFromConsigneeLocation())
				{
					if (!ConsigneeDeliveryAddress.E2_AddressOverride)
					{
						ConsigneeDeliveryAddress.UpdateDefaultAddress(Destination);
					}

					if (!IsSettingConsigneeFromDestinationSuspended && !ConsigneeDocumentaryAddress.E2_AddressOverride && !fIsImportingData)
					{
						ConsigneeDocumentaryAddress.UpdateDefaultAddress(Destination);
					}

					if (!IsSettingDefaultValues)
					{
						UpdateETAWithPortDefaultDeliveryTime();
						if (!fIsImportingData)
						{
							UpdateETDeliveryWithPortDefaultDeliveryTime();
						}
					}

					MarkDocsAndCartageAsNeedsValidation();
					Transports.MarkAsNeedingValidation();

					if (!IsValidationSuspended)
					{
						Validation.ValidateJS_HBLAWBChargesDisplay();
					}
				}

				RefreshCalculatedVolumeWeightValues();
				DocsAndCartage.RequiredDocuments.SetAllDocumentsReceivedEventLogger();

				if (!IsValidationSuspended)
				{
					Validation.ValidateJS_CommunityTransitStatus();
				}
			}
		}

		public IDisposable SuspendSettingDestFromConsigneeLocation()
		{
			suspendSettingDestFromConsigneeLocation = true;

			return new DisposableAction(() => suspendSettingDestFromConsigneeLocation = false);
		}

		bool suspendSettingDestFromConsigneeLocation;

		#endregion

		#region SetIsDomestic

		void SetIsDomestic()
		{
			if (!this.IsUnknown())
			{
				IsDomesticFreight = this.IsDomestic();
			}
		}

		#endregion

		#region JS_UniqueConsignRef

		[NotDefaultingPropertyValue]
		public override ZString JS_UniqueConsignRef
		{
			get { return base.JS_UniqueConsignRef; }
			set { base.JS_UniqueConsignRef = value; }
		}

		#endregion

		#region JS_TransportMode

		[List("Lookups.JS_TransportMode_List")]
		public override ZString JS_TransportMode
		{
			get { return base.JS_TransportMode; }
			set
			{
				var oldTransportModeValue = base.JS_TransportMode;
				if (oldTransportModeValue != value)
				{
					base.JS_TransportMode = value;
					if (!IsSettingDefaultValues)
					{
						((IDefaultNumberOfDecimalsSupporter)this).RoundMeasurePropertiesOnTransportModeChanged();
					}
					MarkDocsAndCartageAsNeedsValidation();
				}

				JS_ChargeableUnitInfo.RefreshBinding();
				UpdateChargeableWeights();
				RefreshHouseBillOfLadingType();
				DefaultElectronicBillOfLadingFields(oldTransportModeValue, value, false);

				if (value == Constants.TransportModes.Courier)
				{
					if (JS_INCO.IsEmpty)
					{
						JS_INCO = FreightDataRegistry.Instance.CourierIncoTerm.Value;
					}
				}

				if (IsSettingDefaultValuesForHBLAWBChargesDisplay)
				{
					if (value == Constants.TransportModes.AirSea)
					{
						JS_HBLAWBChargesDisplay = "";
					}
					else
					{
						JS_HBLAWBChargesDisplay = GetDefaultChargesDisplay();
					}
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateJS_OuterPacks();
				}

				if (!fIsImportingData)
				{
					ZString defaultContainerMode = ZString.Empty;
					if (!JS_TransportModeInfo.HasNotifications())
					{
						defaultContainerMode = GetRegistryDefaultContainerMode();
					}

					if (defaultContainerMode.IsEmpty)
					{
						var packingModeList = Lookups.JS_PackingMode_List;
						if (packingModeList != null && packingModeList.Count > 0)
						{
							defaultContainerMode = packingModeList[0].Code;
						}
					}

					if (!defaultContainerMode.IsEmpty)
					{
						JS_PackingMode = defaultContainerMode;
					}
				}

				if (!IsSettingDefaultValues)
				{
					SetTransportModeChangeImplications();
					UpdateETAWithPortDefaultDeliveryTime();
					if (!fIsImportingData)
					{
						UpdateETDeliveryWithPortDefaultDeliveryTime();
					}
				}

				RefreshCalculatedVolumeWeightValues();
			}
		}

		void DefaultElectronicBillOfLadingFields(ZString previousValue, ZString newValue, ZBool isOnUniversalCopyFinish)
		{
			if (!IsSettingDefaultValues && previousValue == newValue && !isOnUniversalCopyFinish)
			{
				return;
			}

			var isSea = (ZString transportMode) => transportMode == Constants.TransportModes.Sea || transportMode == Constants.TransportModes.SeaAir;

			if ((IsSettingDefaultValues || isOnUniversalCopyFinish || isSea(previousValue)) && !isSea(newValue))
			{
				JS_ElectronicBillOfLadingType = ZString.Empty;
				JS_ElectronicBillOfLadingTerms = ZString.Empty;
				JS_ElectronicBillOfLadingStatus = ZString.Empty;
				JS_ElectronicBillOfLadingHouseBill = ZString.Empty;
			}
			else if ((IsSettingDefaultValues || isOnUniversalCopyFinish || !isSea(previousValue)) && isSea(newValue))
			{
				JS_ElectronicBillOfLadingType = Constants.BillOfLadingBillType.Codes.Straight;
				JS_ElectronicBillOfLadingTerms = Constants.BillOfLadingBillTerms.Codes.NonTransferable;
				JS_ElectronicBillOfLadingStatus = ZString.Empty;
				JS_ElectronicBillOfLadingHouseBill = ZString.Empty;
			}
		}

		protected virtual ZString GetDefaultChargesDisplay()
		{
			if (JS_TransportMode == Constants.TransportModes.Air)
			{
				var firstSet = Env.Registry.Freight.AirWaybill.PrintAsAgreedOnFirstSetHAWB;
				var secondSet = Env.Registry.Freight.AirWaybill.PrintAsAgreedOnSecondSetHAWB;
				return ChargesApplyHelper.GetDefaultChargesApply(firstSet, secondSet);
			}

			return DocumentsDataRegistry.Instance.HBLChargesDefaultDisplay.Value;
		}

		void RefreshHouseBillOfLadingType()
		{
			if (!Lookups.JS_HouseBillOfLadingType_List.ContainsCode(JS_HouseBillOfLadingType))
			{
				JS_HouseBillOfLadingType = Lookups.JS_HouseBillOfLadingTypeDefault;
			}
		}

		#endregion

		#region JS_ShipmentType

		[List("Lookups.JS_ShipmentType_List")]
		public override ZString JS_ShipmentType
		{
			get { return base.JS_ShipmentType; }
			set
			{
				if (base.JS_ShipmentType != value)
				{
					bool continueWithChange = true;

					switch (value)
					{
						case Constants.ShipmentTypes.StandardHouse:
						case Constants.ShipmentTypes.HighVolumeLowValueLegacy:
						case Constants.ShipmentTypes.HighVolumeLowValue:
							continueWithChange = AreSubShipmentRequrementsMet ||
								CanChangeShipmentType(JS_ShipmentType, value);

							if (continueWithChange)
							{
								CoLoadShipments.RemoveAll();
							}
							break;

						case Constants.ShipmentTypes.CoLoadMaster:
						case Constants.ShipmentTypes.BlindCoLoadMaster:
						case Constants.ShipmentTypes.AssemblyMaster:
							continueWithChange = AreMasterAndLeadShipmentRequrementsMet ||
								CanChangeShipmentType(JS_ShipmentType, value);

							if (continueWithChange)
							{
								InnerPackLines.RemoveAndDeleteAll();
								OuterPackLines.RemoveAndDeleteAll();
							}
							break;
					}

					if (continueWithChange)
					{
						bool oldCanHaveOwnPackLines = CanHaveOwnPackLines;
						ZString oldShipmentType = JS_ShipmentType;

						base.JS_ShipmentType = value;

						if (oldCanHaveOwnPackLines != CanHaveOwnPackLines)
						{
							InnerPackLines.SetCountedReadOnlyIncludingChildren(!CanHaveOwnPackLines);
							OuterPackLines.SetCountedReadOnlyIncludingChildren(!CanHaveOwnPackLines);
							InnerPackLines.Load();
							OuterPackLines.Load();
						}

						if (oldShipmentType == Constants.ShipmentTypes.CoLoadMaster
							|| oldShipmentType == Constants.ShipmentTypes.BlindCoLoadMaster
							|| JS_ShipmentType == Constants.ShipmentTypes.CoLoadMaster
							|| JS_ShipmentType == Constants.ShipmentTypes.BlindCoLoadMaster)
						{
							ShipmentTypeChangedToOrFromCoload();
						}

						if (!IsSettingDefaultValues)
						{
							if (!IsReadOnlyDueToPhase)
							{
								CoLoadShipments.SetReadOnlyIncludingChildren(!IsLeadOrMaster || IsHighVolumeLowValue || IsHighVolumeLowValueLegacy);
							}
							CoLoadShipments.RefreshBindingIncludingChildren();
							DocsAndCartage.MarkAsNeedingValidation();
						}

						RegisterSubShipmentsIfRequired();
					}

					if (!IsValidationSuspended)
					{
						Validation.ValidateJS_PackingMode();
					}
				}
			}
		}

		bool AreSubShipmentRequrementsMet
		{
			get { return CoLoadShipments.Count == 0; }
		}

		bool AreMasterAndLeadShipmentRequrementsMet
		{
			get
			{
				return OuterPackLines.Count == 0 &&
					InnerPackLines.Count == 0;
			}
		}

		#region Implementation

		protected virtual void ShipmentTypeChangedToOrFromCoload()
		{
		}

		void RegisterSubShipmentsIfRequired()
		{
			if (!IsDeleted && ChildEditableService.GetState(Factory) == ChildEditableServiceStates.Shipment && IsRoot)
			{
				if (IsLeadOrMaster && !IsRegisteredEditableChildObject(CoLoadShipments))
				{
					RegisterEditableChildObject(CoLoadShipments);
				}
				else if (!IsLeadOrMaster && IsRegisteredEditableChildObject(CoLoadShipments))
				{
					UnRegisterEditableChildObject(CoLoadShipments);
				}
			}
		}

		#endregion

		#region Bool properties

		public bool IsStandardHouse
		{
			get { return JS_ShipmentType == Constants.ShipmentTypes.StandardHouse; }
		}

		public bool IsCoLoadMaster
		{
			get { return JS_ShipmentType == Constants.ShipmentTypes.CoLoadMaster; }
		}

		public bool IsBlindCoLoadMaster
		{
			get { return JS_ShipmentType == Constants.ShipmentTypes.BlindCoLoadMaster; }
		}

		public bool IsBuyersConsolLead
		{
			get { return JS_ShipmentType == Constants.ShipmentTypes.BuyersConsolLead; }
		}

		public bool IsShippersConsolLead
		{
			get { return JS_ShipmentType == Constants.ShipmentTypes.ShippersConsolLead; }
		}

		public bool IsAssemblyMaster
		{
			get { return JS_ShipmentType == Constants.ShipmentTypes.AssemblyMaster; }
		}

		public bool IsLeadOrMaster
		{
			get
			{
				return JS_ShipmentType != Constants.ShipmentTypes.StandardHouse
					&& JS_ShipmentType != Constants.ShipmentTypes.HighVolumeLowValue
					&& JS_ShipmentType != Constants.ShipmentTypes.HighVolumeLowValueLegacy;
			}
		}

		public bool IsHighVolumeLowValueLegacy
		{
			get { return JS_ShipmentType == Constants.ShipmentTypes.HighVolumeLowValueLegacy; }
		}

		public bool IsHighVolumeLowValue
		{
			get { return JS_ShipmentType == Constants.ShipmentTypes.HighVolumeLowValue; }
		}

		public bool IsHighVolumeLowValueMaster
		{
			get { return JS_ShipmentType == Constants.ShipmentTypes.HighVolumeLowValueMaster; }
		}

		public bool IsThirdPartyOwnershipHouse
		{
			get { return JS_ShipmentType == Constants.ShipmentTypes.ThirdPartyOwnershipHouse; }
		}

		/// <summary>
		/// Specifies whether this shipment is a "fake" master shipment registered
		/// simply to automatically sum and represent all child shipments.
		/// </summary>
		public bool IsMasterShipmentRepresentingAllChildShipments
		{
			get { return (IsCoLoadMaster || IsBlindCoLoadMaster || IsAssemblyMaster) && CoLoadShipments.Count > 0; }
		}

		#endregion

		#endregion

		#region JS_HouseBillOfLadingType
		[List("Lookups.JS_HouseBillOfLadingType_List")]
		public override ZString JS_HouseBillOfLadingType
		{
			get { return base.JS_HouseBillOfLadingType; }
			set { base.JS_HouseBillOfLadingType = value; }
		}

		protected bool JS_HouseBillOfLadingType_ReadOnly
		{
			get { return (!IsSea && !IsRoad && !IsRail); }
		}

		#endregion

		#region JS_HouseBill

		public override ZString JS_HouseBill
		{
			get { return base.JS_HouseBill.ToUpper(); }
			set
			{
				base.JS_HouseBill = value.ToUpper();
				HouseBillWasGenerated = false;
				SuspendHouseBillValidation = false;

				DefaultElectronicBillOfLadingHouseBill();
			}
		}

		public void DefaultElectronicBillOfLadingHouseBill()
		{
			if (FreightDataRegistry.Instance.EnableBoleroEHBLIntegration.Value.EnableEBLIntegration && IsSea && JS_ElectronicBillOfLadingHouseBill.IsEmpty)
			{
				JS_ElectronicBillOfLadingHouseBill = JS_HouseBill;
			}
		}

		#endregion

		#region JS_HouseBillIssueDate

		public override ZDateTime JS_HouseBillIssueDate
		{
			get { return new ZDateTime(base.JS_HouseBillIssueDate, DateTimeKind.Unspecified); }
			set { base.JS_HouseBillIssueDate = value; }
		}

		#endregion

		#region JS_ClientRequestedETA

		public override ZDateTime JS_ClientRequestedETA
		{
			get { return new ZDateTime(base.JS_ClientRequestedETA, DateTimeKind.Unspecified); }
			set { base.JS_ClientRequestedETA = value; }
		}

		#endregion

		#region JS_A_BKD

		public override ZDateTime JS_A_BKD
		{
			get { return new ZDateTime(base.JS_A_BKD, DateTimeKind.Unspecified); }
			set { base.JS_A_BKD = value; }
		}

		#endregion

		#region JS_ActualWeight

		[MeasureUnit(Schema.JS_UnitOfWeight, MeasureUnitType.Weight)]
		public override ZDecimal JS_ActualWeight
		{
			get { return base.JS_ActualWeight; }

			set
			{
				if (isSettingActualWeight)
				{
					return;
				}

				var previousActualWeight = base.JS_ActualWeight;
				var roundedValue = this.GetRoundedValue(JobShipmentSchema.JS_ActualWeight, JS_ActualWeightInfo, value);

				if (roundedValue != previousActualWeight)
				{
					isSettingActualWeight = true;
					EnsureCoLoadShipmentsAreLoaded();

					if (UpdatePackLines)
					{
						UpdateRelevantPackLinesWeight(previousActualWeight, roundedValue);
					}

					CheckNotWithinSqlPrecisionAndScale(JobShipmentSchema.JS_ActualWeight, previousActualWeight, roundedValue);

					base.JS_ActualWeight = roundedValue;

					if (JS_ActualWeight > 0.0M && JS_UnitOfWeight.IsEmpty)
					{
						JS_UnitOfWeight = Env.Registry.FreightWeightUnit;
					}

					// Update documented weight, if it has not been changed manually
					if (JS_DocumentedWeight != JS_ActualWeight && JS_DocumentedWeight == previousActualWeight)
					{
						JS_DocumentedWeight = roundedValue;
					}

					// Update manifested weight, if it has not been changed manually
					if (JS_ManifestedWeight != JS_ActualWeight && JS_ManifestedWeight == previousActualWeight)
					{
						JS_ManifestedWeight = roundedValue;
					}

					if (!JS_ActualWeightInfo.HasErrors() && !isSettingActualChargeable)
					{
						UpdateActualChargeableWeight();
					}

					RefreshCalculatedVolumeWeightValues();
					isSettingActualWeight = false;
				}
			}
		}

		protected bool isSettingActualWeight;

		void CheckNotWithinSqlPrecisionAndScale(SchemaDecimalColumn schemaColumn, ZDecimal previousValue, ZDecimal newValue)
		{
			if (IsValidationSuspended && !newValue.IsWithinSqlPrecisionAndScale(schemaColumn.Precision, schemaColumn.Scale))
			{
				var msg = string.Format(CultureInfo.InvariantCulture, "Shipment PK = {0}, JS_UniqueConsignRef = {1}\r\n{2} previous value = {3}, new value = {4}\r\n", PK, JS_UniqueConsignRef, schemaColumn.Name, previousValue, newValue);
				ErrorReporter.ReportOnce("CommonShipment_NotWithinSqlPrecisionAndScale", msg);
			}
		}

		#endregion

		#region JS_ActualVolume

		[MeasureUnit(Schema.JS_UnitOfVolume, MeasureUnitType.Volume)]
		public override ZDecimal JS_ActualVolume
		{
			get { return base.JS_ActualVolume; }
			set
			{
				if (isSettingActualVolume)
				{
					return;
				}

				ZDecimal previousActualVolume = base.JS_ActualVolume;
				var roundedValue = this.GetRoundedValue(JobShipmentSchema.JS_ActualVolume, JS_ActualVolumeInfo, value);

				if (roundedValue != previousActualVolume)
				{
					isSettingActualVolume = true;
					EnsureCoLoadShipmentsAreLoaded();

					if (UpdatePackLines)
					{
						UpdateRelevantPackLinesVolume(previousActualVolume, roundedValue);
					}

					CheckNotWithinSqlPrecisionAndScale(JobShipmentSchema.JS_ActualVolume, previousActualVolume, roundedValue);

					base.JS_ActualVolume = roundedValue;

					if (JS_ActualVolume > 0.0M && JS_UnitOfVolume.IsEmpty)
					{
						JS_UnitOfVolume = Env.Registry.FreightVolumeUnit;
					}

					// Update documented volume, if it has not been changed manually
					if (JS_DocumentedVolume != JS_ActualVolume && JS_DocumentedVolume == previousActualVolume)
					{
						JS_DocumentedVolume = roundedValue;
					}

					// Update manifested volume, if it has not been changed manually
					if (JS_ManifestedVolume != JS_ActualVolume && JS_ManifestedVolume == previousActualVolume)
					{
						JS_ManifestedVolume = roundedValue;
					}

					if (!JS_ActualVolumeInfo.HasErrors() && !isSettingActualChargeable)
					{
						UpdateActualChargeableWeight();
					}

					RefreshCalculatedVolumeWeightValues();
					isSettingActualVolume = false;
				}
			}
		}

		protected bool isSettingActualVolume;

		#endregion

		#region JS_ActualChargeable

		public override ZDecimal JS_ActualChargeable
		{
			get { return base.JS_ActualChargeable; }
			set
			{
				if (isSettingActualChargeable)
				{
					return;
				}

				ZDecimal previousActualChargeable = base.JS_ActualChargeable;
				var roundedValue = this.GetRoundedValue(JobShipmentSchema.JS_ActualChargeable, JS_ActualChargeableInfo, value);

				base.JS_ActualChargeable = roundedValue;
				EnsureCoLoadShipmentsAreLoaded();

				// Update documented chargeable, if it has not been changed manually
				if (JS_DocumentedChargeable != JS_ActualChargeable && JS_DocumentedChargeable == previousActualChargeable)
				{
					JS_DocumentedChargeable = roundedValue;
				}

				// Update manifested chargeable, if it has not been changed manually
				if (JS_ManifestedChargeable != JS_ActualChargeable && JS_ManifestedChargeable == previousActualChargeable)
				{
					JS_ManifestedChargeable = roundedValue;
				}

				if (!fIsImportingData)
				{
					isSettingActualChargeable = true;
					try
					{
						UpdateActualsFromChargeable();
					}
					finally
					{
						isSettingActualChargeable = false;
					}
				}
			}
		}

		protected bool isSettingActualChargeable;

		void UpdateActualsFromChargeable()
		{
			if (IsCloning || IsCopying || JS_ActualChargeable.IsEmpty || (!JS_ActualVolume.IsEmpty && !JS_ActualWeight.IsEmpty)
				|| isSettingActualWeight || isSettingActualVolume || isSettingLoadingMeters || isSettingUnitOfWeight)
			{
				return;
			}

			if (FreightDataRegistry.Instance.WeightChargableTransportModes.Contains((string)JS_TransportMode) && JS_ActualVolume.IsEmpty && !isSettingActualVolume)
			{
				ZVolume? newActualVolume = ChargeableAmountCalculator.GetActualFromChargeable(
					JS_TransportMode, IsDomesticFreightForChargeableWeightCalculations, new ZWeight(JS_ActualChargeable, JS_ChargeableUnit),
					new ZWeight(JS_ActualWeight, ShipmentWeightUnit), new ZVolume(JS_ActualVolume, ShipmentVolumeUnit));

				if (newActualVolume.HasValue)
				{
					JS_ActualVolume = newActualVolume.Value.Amount;
				}
			}
			else if (FreightDataRegistry.Instance.VolumeChargableTransportModes.Contains((string)JS_TransportMode) && JS_ActualWeight.IsEmpty && !isSettingActualWeight)
			{
				ZWeight? newActualWeight = ChargeableAmountCalculator.GetActualFromChargeable(
					 JS_TransportMode, IsDomesticFreightForChargeableWeightCalculations, new ZVolume(JS_ActualChargeable, JS_ChargeableUnit),
					 new ZWeight(JS_ActualWeight, ShipmentWeightUnit), new ZVolume(JS_ActualVolume, ShipmentVolumeUnit));

				if (newActualWeight.HasValue)
				{
					JS_ActualWeight = newActualWeight.Value.Amount;
				}
			}
		}

		#endregion

		#region JS_LoadingMeters

		public bool IsRoadLoadingMetersEnabled
		{
			get { return IsRoad && FreightDataRegistry.Instance.EnableRoadLoadingMeters.Value; }
		}

		public override ZDecimal JS_LoadingMeters
		{
			get { return base.JS_LoadingMeters; }
			set
			{
				if (isSettingLoadingMeters)
				{
					return;
				}

				ZDecimal previousLoadingMeters = base.JS_LoadingMeters;
				if (value != previousLoadingMeters)
				{
					isSettingLoadingMeters = true;
					EnsureCoLoadShipmentsAreLoaded();

					if (UpdatePackLines)
					{
						UpdateRelevantPackLoadingMeters(previousLoadingMeters, value);
					}

					base.JS_LoadingMeters = value;

					// Update documented value, if it has not been changed manually
					if (JS_DocumentedLoadingMeters == previousLoadingMeters)
					{
						JS_DocumentedLoadingMeters = value;
					}

					// Update manifested value, if it has not been changed manually
					if (JS_ManifestedLoadingMeters == previousLoadingMeters)
					{
						JS_ManifestedLoadingMeters = value;
					}

					if (!JS_LoadingMetersInfo.HasErrors() && !isSettingActualChargeable)
					{
						UpdateActualChargeableWeight();
					}

					isSettingLoadingMeters = false;
				}
			}
		}

		protected bool isSettingLoadingMeters;

		protected bool JS_LoadingMeters_ReadOnly
		{
			get { return !IsRoadLoadingMetersEnabled; }
		}

		#endregion

		#region JS_OuterPacks

		public override ZInt JS_OuterPacks
		{
			get { return base.JS_OuterPacks; }
			set
			{
				ZInt previousPacks = base.JS_OuterPacks;
				if (previousPacks != value)
				{
					EnsureCoLoadShipmentsAreLoaded();
					if (UpdatePackLines)
					{
						OuterPackLines.NotifyPackageCountChanged(previousPacks, value);
					}

					base.JS_OuterPacks = value;

					if (JS_OuterPacks > 0.0 && JS_F3_NKPackType.IsEmpty)
					{
						JS_F3_NKPackType = FreightPacksDataRegistry.Instance.OuterPackUnit.Value;
					}
				}
			}
		}

		#endregion

		[List("Lookups.ShipperCODPaymentTypes")]
		public override ZString JS_ShipperCODPayMethod
		{
			get { return base.JS_ShipperCODPayMethod; }
			set { base.JS_ShipperCODPayMethod = value; }
		}

		[List("Lookups.RefCurrency_List")]
		public override ZString JS_RX_NKInsuranceCurrency
		{
			get { return base.JS_RX_NKInsuranceCurrency; }
			set
			{
				base.JS_RX_NKInsuranceCurrency = value;
				EnsureCoLoadShipmentsAreLoaded();
			}
		}

		[List("Lookups.RefCurrency_List")]
		public override ZString JS_RX_NKGoodsValueCurr
		{
			get { return base.JS_RX_NKGoodsValueCurr; }
			set
			{
				base.JS_RX_NKGoodsValueCurr = value;
				EnsureCoLoadShipmentsAreLoaded();
			}
		}

		[DecimalPlaces(2)]
		public override ZDecimal JS_GoodsValue
		{
			get { return base.JS_GoodsValue; }
			set
			{
				base.JS_GoodsValue = value;
				EnsureCoLoadShipmentsAreLoaded();
			}
		}

		[DecimalPlaces(2)]
		public override ZDecimal JS_InsuranceValue
		{
			get { return base.JS_InsuranceValue; }
			set
			{
				base.JS_InsuranceValue = value;
				EnsureCoLoadShipmentsAreLoaded();
			}
		}

		[DecimalPlaces(2)]
		public override ZDecimal JS_ShipperCODAmount
		{
			get { return base.JS_ShipperCODAmount; }
			set { base.JS_ShipperCODAmount = value; }
		}

		#region JS_TotalPackageCount

		public override ZInt JS_TotalPackageCount
		{
			get { return base.JS_TotalPackageCount; }
			set
			{
				ZInt previousValue = base.JS_TotalPackageCount;
				EnsureCoLoadShipmentsAreLoaded();
				if (value != previousValue)
				{
					base.JS_TotalPackageCount = value;

					if (JS_TotalPackageCount > 0.0 && JS_F3_NKTotalCountPackType.IsEmpty)
					{
						JS_F3_NKTotalCountPackType = FreightPacksDataRegistry.Instance.InnerPackUnit.Value;
					}

					AddDefaultInnerPackLineIfRequired();

					if (UpdatePackLines)
					{
						InnerPackLines.NotifyPackageCountChanged(previousValue, value);
					}
				}
			}
		}

		#endregion

		#region JS_E_ARV

		[ActionField(DateTimeFormat = ZDateTimePickerFormat.Long)]
		[EventDateProperty(AutoEvents.ArrivalCode, EstimateActual.Estimate)]
		public override ZDateTime JS_E_ARV
		{
			get { return new ZDateTime(base.JS_E_ARV, DateTimeKind.Unspecified); }
			set
			{
				if (InAddDateEvent)
				{ return; }
				base.JS_E_ARV = value;
				MarkDocsAndCartageAsNeedsValidation();
				AddDateEvent(JS_E_ARVInfo, Events.Arrival);
				if (!DocsAndCartage.IsValidationSuspended)
				{
					DocsAndCartage.Validation.ValidateJP_EstimatedDelivery();
				}

				if (!fIsImportingData)
				{
					UpdateETDeliveryWithPortDefaultDeliveryTime();
				}
			}
		}

		#endregion

		#region JS_E_ARV_UTC

		public ZDateTime JS_E_ARV_UTC
		{
			get { return JS_E_ARV.IsValid ? Env.Time.GetUtcFromUnlocoTime(JS_RL_NKDestination.ToString(), JS_E_ARV.ToDateTime()) : ZDateTime.Empty; }
		}

		#endregion

		#region JS_E_DEP

		[ActionField(DateTimeFormat = ZDateTimePickerFormat.Long)]
		[EventDateProperty(AutoEvents.DepartureCode, EstimateActual.Estimate)]
		public override ZDateTime JS_E_DEP
		{
			get { return new ZDateTime(base.JS_E_DEP, DateTimeKind.Unspecified); }
			set
			{
				if (InAddDateEvent)
				{ return; }
				base.JS_E_DEP = value;
				MarkDocsAndCartageAsNeedsValidation();
				AddDateEvent(JS_E_DEPInfo, Events.Departure);
			}
		}

		#endregion

		#region JS_E_DEP_UTC

		public ZDateTime JS_E_DEP_UTC
		{
			get { return JS_E_DEP.IsValid ? Env.Time.GetUtcFromUnlocoTime(JS_RL_NKOrigin.ToString(), JS_E_DEP.ToDateTime()) : ZDateTime.Empty; }
		}

		#endregion

		#region Documented Measures

		public override ZDecimal JS_DocumentedWeight
		{
			get { return base.JS_DocumentedWeight; }
			set
			{
				base.JS_DocumentedWeight = this.GetRoundedValue(JobShipmentSchema.JS_DocumentedWeight, JS_DocumentedWeightInfo, value);

				if (!isSettingActualChargeable)
				{
					UpdateDocumentedChargeableWeight();
				}
			}
		}

		public override ZDecimal JS_DocumentedVolume
		{
			get { return base.JS_DocumentedVolume; }
			set
			{
				base.JS_DocumentedVolume = this.GetRoundedValue(JobShipmentSchema.JS_DocumentedVolume, JS_DocumentedVolumeInfo, value);

				if (!isSettingActualChargeable)
				{
					UpdateDocumentedChargeableWeight();
				}
			}
		}

		public override ZDecimal JS_DocumentedChargeable
		{
			get { return base.JS_DocumentedChargeable; }
			set { base.JS_DocumentedChargeable = this.GetRoundedValue(JobShipmentSchema.JS_DocumentedChargeable, JS_DocumentedChargeableInfo, value); }
		}

		public override ZDecimal JS_DocumentedLoadingMeters
		{
			get { return base.JS_DocumentedLoadingMeters; }
			set
			{
				base.JS_DocumentedLoadingMeters = value;
				if (!isSettingActualChargeable)
				{
					UpdateDocumentedChargeableWeight();
				}
			}
		}

		protected bool JS_DocumentedLoadingMeters_ReadOnly
		{
			get { return !IsRoadLoadingMetersEnabled; }
		}

		#endregion

		#region Manifested Measures

		public override ZDecimal JS_ManifestedWeight
		{
			get { return base.JS_ManifestedWeight; }
			set
			{
				base.JS_ManifestedWeight = this.GetRoundedValue(JobShipmentSchema.JS_ManifestedWeight, JS_ManifestedWeightInfo, value);
				if (!isSettingActualChargeable)
				{
					UpdateManifestedChargeableWeight();
				}
			}
		}

		public override ZDecimal JS_ManifestedVolume
		{
			get { return base.JS_ManifestedVolume; }
			set
			{
				base.JS_ManifestedVolume = this.GetRoundedValue(JobShipmentSchema.JS_ManifestedVolume, JS_ManifestedVolumeInfo, value);
				if (!isSettingActualChargeable)
				{
					UpdateManifestedChargeableWeight();
				}
			}
		}

		public override ZDecimal JS_ManifestedChargeable
		{
			get { return base.JS_ManifestedChargeable; }
			set { base.JS_ManifestedChargeable = this.GetRoundedValue(JobShipmentSchema.JS_ManifestedChargeable, JS_ManifestedChargeableInfo, value); }
		}

		public override ZDecimal JS_ManifestedLoadingMeters
		{
			get { return base.JS_ManifestedLoadingMeters; }
			set
			{
				base.JS_ManifestedLoadingMeters = value;
				if (!isSettingActualChargeable)
				{
					UpdateManifestedChargeableWeight();
				}
			}
		}

		protected bool JS_ManifestedLoadingMeters_ReadOnly
		{
			get { return !IsRoadLoadingMetersEnabled; }
		}

		#endregion

		#region JS_RS_NKServiceLevel

		[List("Lookups.RefServiceLevel_List")]
		public override ZString JS_RS_NKServiceLevel
		{
			get { return IsDeleted ? ZString.Empty : base.JS_RS_NKServiceLevel; }
			set { base.JS_RS_NKServiceLevel = value; }
		}

		#endregion

		#region

		[RelatedBusinessObject("GatewayServiceLevel")]
		[List("Lookups.GatewayServiceLevels")]
		public override ZString JS_RS_NKGatewayServiceLevel
		{
			get => base.JS_RS_NKGatewayServiceLevel;
			set
			{
				base.JS_RS_NKGatewayServiceLevel = value;

				if (JS_RS_NKGatewayServiceLevel_InitValue == null && JS_RS_NKGatewayServiceLevelInfo.HasChanges)
				{
					JS_RS_NKGatewayServiceLevel_InitValue = JS_RS_NKGatewayServiceLevelInfo.OriginalValue.ToString();
				}
			}
		}

		public string JS_RS_NKGatewayServiceLevel_InitValue { get; private set; }

		#endregion

		#region GatewayServiceLevel Helper

		public RequestPermissionByImpersonation RequestPermissionByImpersonation { get; set; }

		public ExclusiveGatewayServiceChecker ExclusiveGatewayServiceChecker
			=> exclusiveGatewayServiceChecker ?? (exclusiveGatewayServiceChecker = new ExclusiveGatewayServiceChecker());
		ExclusiveGatewayServiceChecker exclusiveGatewayServiceChecker;

		#endregion

		#region ServiceLevel

		public override RefServiceLevel ServiceLevel
		{
			get
			{
				RefServiceLevel result;
				if (!JS_RS_NKServiceLevel.IsEmpty)
				{
					result = Factory.LoadFromNaturalKey<RefServiceLevel>(RefServiceLevelSchema.RS_Code, JS_RS_NKServiceLevel);
				}
				else
				{
					result = RegistryServiceLevel;
				}
				return result;
			}
		}

		public virtual RefServiceLevel RegistryServiceLevel
		{
			get { return Factory.Load<RefServiceLevel>(Env.Registry.ServiceLevel); }
		}

		#endregion

		#region JS_INCO

		[List("Lookups.JS_INCO_List")]
		public override ZString JS_INCO
		{
			get { return base.JS_INCO; }
			set
			{
				base.JS_INCO = value;
				JS_PaymentTermInfo.RefreshBinding();
			}
		}

		#endregion

		#region JS_ReleaseType

		[List("Lookups.JS_ReleaseType_List")]
		public override ZString JS_ReleaseType
		{
			get { return base.JS_ReleaseType; }
			set
			{
				ZString oldValue = JS_ReleaseType;
				base.JS_ReleaseType = value;

				if (JS_ReleaseType != oldValue || IsSettingDefaultValues)
				{
					if (!fIsImportingData)
					{
						DefaultNumberOfBillsByReleaseType(JS_ReleaseType);
					}

					if (JS_ReleaseType == Constants.ShipmentReleaseTypes.OriginalReq)
					{
						var directionType = this.IsDomestic()
							? JobRequiredDocument.DocUsage.Domestic
							: JobRequiredDocument.DocUsage.Import;

						var requiredHBLDocument = DocsAndCartage.RequiredDocuments.AddIfNotExists(Constants.RefDocTypes.HouseBill, directionType);
						if (requiredHBLDocument != null)
						{
							requiredHBLDocument.EQ_OriginalDocRequired = true;
						}
					}
					else if (oldValue == Constants.ShipmentReleaseTypes.OriginalReq)
					{
						var hblDoc = DocsAndCartage.RequiredDocuments.GetDocByType(Constants.RefDocTypes.HouseBill);
						if (hblDoc != null && !hblDoc.IsInDatabase)
						{
							DocsAndCartage.RequiredDocuments.RemoveIfExists(Constants.RefDocTypes.HouseBill);
						}
					}
				}
			}
		}

		#endregion

		#region JS_IsCFSRegistered

		[ActionField(FieldType = ActionFieldType.Hidden)]
		[DocumentEngineIntegration.DocumentParsing.DocumentFieldExcludeFromMap]
		[WorkflowSetFieldReadonly]
		public override ZBool JS_IsCFSRegistered
		{
			get
			{
				return base.JS_IsCFSRegistered;
			}
			set
			{
				if (!value && IsInDatabase && (ZBool)JS_IsCFSRegisteredInfo.OriginalValue && !Globals.IsTest && !IsSuppressedCheckReset)
				{
					ErrorReporter.ReportOnce("CommonShipmentShouldNotResetJS_IsCFSRegistered", "Once a shipment has been marked as CFS registered and saved it should not be unmarked");
				}
				else
				{
					base.JS_IsCFSRegistered = value;
					JS_TranshipToOtherCFS = value && this.IsImport() && !IsCoLoadMaster && !IsBlindCoLoadMaster;
				}
			}
		}

		#endregion

		protected bool JS_NoOriginalBills_ReadOnly
		{
			get { return JS_ReleaseType == Core.Constants.ShipmentReleaseTypes.ExpressBofL; }
		}

		protected virtual ZString DefaultReleaseType
		{
			get { return FreightDataRegistry.Instance.ReleaseType.Value; }
		}

		protected virtual void DefaultNumberOfBillsByReleaseType(ZString releaseType)
		{
			DefaultNumberOfBillsWithFallback(releaseType);
		}

		protected virtual void DefaultNumberOfBillsWithFallback(ZString releaseType)
		{
			JS_NoOriginalBills = (byte)FreightDataRegistry.Instance.ReleaseTypes.Value.OriginalsNumberByType(releaseType);
			JS_NoCopyBills = (byte)FreightDataRegistry.Instance.ReleaseTypes.Value.CopiesNumberByType(releaseType);
		}

		public virtual ZBool IsDomesticFreight
		{
			get
			{
				if (isDomesticFreight == null)
				{
					isDomesticFreight = this.IsDomestic();
				}

				return (ZBool)isDomesticFreight;
			}
			set
			{
				if (isDomesticFreight != value && !isSettingDomesticFreight)
				{
					isSettingDomesticFreight = true;
					try
					{
						isDomesticFreight = value;
						IsDomesticFreightInfo.RefreshBinding();

						if (value)
						{
							if (JS_RL_NKOrigin.IsEmpty)
							{
								JS_RL_NKOrigin = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
							}

							if (JS_RL_NKDestination.IsEmpty)
							{
								JS_RL_NKDestination = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
							}
						}

						if (!JS_INCO.IsEmpty && !Lookups.JS_INCO_List.ContainsCode(JS_INCO))
						{
							JS_INCO = ZString.Empty;
						}
					}
					finally
					{
						isSettingDomesticFreight = false;
					}

					if (!IsValidationSuspended)
					{
						Validation.ValidateIsDomesticFreight();
					}
				}
			}
		}

		ZBool? isDomesticFreight;
		bool isSettingDomesticFreight;

		public ZPropertyInfo IsDomesticFreightInfo
		{
			get { return GetZPropertyInfo(nameof(IsDomesticFreight)); }
		}

		public string JobNumber
		{
			get { return JS_UniqueConsignRef; }
		}

		public ZString ShipmentWeightUnit
		{
			get { return FreightUtilities.IsValidWeightUnit(JS_UnitOfWeight) ? JS_UnitOfWeight.ToString() : Env.Registry.FreightWeightUnit; }
		}

		public ZString ShipmentVolumeUnit
		{
			get { return FreightUtilities.IsValidVolumeUnit(JS_UnitOfVolume) ? JS_UnitOfVolume.ToString() : Env.Registry.FreightVolumeUnit; }
		}

		public bool IsAirOrSeaAirAndHasFirstAirLegLoadingInCurrentCountry
		{
			get { return IsAir || IsSeaAirAndHasFirstAirLegLoadingInCurrentCountry; }
		}

		public bool IsSeaAirAndHasFirstAirLegLoadingInCurrentCountry
		{
			get { return IsSeaAir && (TransportsIncludingRelated.FirstLegMatching(leg => leg.IsAir)?.JW_RL_NKLoadPort.StartsWith(GlbCompany.CurrentCompany.GC_RN_NKCountryCode) ?? false); }
		}

		public bool IsSeaAir
		{
			get { return JS_TransportMode == Constants.TransportModes.SeaAir; }
		}

		public bool IsAir
		{
			get { return JS_TransportMode == Constants.TransportModes.Air || JS_TransportMode == Constants.TransportModes.AirSea; }
		}

		public bool IsSea
		{
			get { return JS_TransportMode == Constants.TransportModes.Sea || JS_TransportMode == Constants.TransportModes.SeaAir; }
		}

		public bool IsRail
		{
			get { return JS_TransportMode == Constants.TransportModes.Rail; }
		}

		public bool IsRoad
		{
			get { return JS_TransportMode == Constants.TransportModes.Road; }
		}

		public bool IsCourier
		{
			get { return JS_TransportMode == Constants.TransportModes.Courier; }
		}

		#region IsConfirmsComplete

		public bool IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType confirmType, ConfirmTimesSyncHelper.ConfirmDateType confirmDateType)
		{
			string confirmDateFieldName = ConfirmTimesSyncHelper.GetConfirmDateFieldName(confirmDateType);

			if (ShowContainerisedConfirms(confirmType))
			{
				var containers = (confirmType == ConfirmTimesSyncHelper.ConfirmType.Pickup) ? DepartureContainers : ArrivalContainers;
				foreach (CommonContainer container in containers)
				{
					var confirm = (confirmType == ConfirmTimesSyncHelper.ConfirmType.Pickup) ? container.OriginConfirm : container.DestinationConfirm;
					if (((ZDateTime)confirm[confirmDateFieldName]).IsEmpty)
					{
						return false;
					}
				}
			}
			else
			{
				var confirms = (confirmType == ConfirmTimesSyncHelper.ConfirmType.Pickup) ? PickupConfirms : DeliveryConfirms;
				if (confirms.Any(confirm => ((ZDateTime)confirm[confirmDateFieldName]).IsEmpty))
				{
					return false;
				}

				foreach (PackLine packLine in OuterPackLines)
				{
					if (packLine.PackagesConfirmed(confirms) < packLine.JL_Calc_PackagesToDeliver)
					{
						return false;
					}
				}
			}

			return true;
		}

		public bool ShowContainerisedConfirms(ConfirmTimesSyncHelper.ConfirmType confirmType)
		{
			bool result;
			if (confirmType == ConfirmTimesSyncHelper.ConfirmType.Pickup)
			{
				result = PackingMode == Constants.ContainerModes.FCL;
			}
			else
			{
				result = PackingMode == Constants.ContainerModes.FCL || (TransportMode == Constants.TransportModes.Sea && PackingMode == Constants.ContainerModes.BuyersConsol);
			}

			return result;
		}

		public bool HasUndeliveredPackages(ConfirmTimesSyncHelper.ConfirmType confirmType)
		{
			if (JS_PackingMode == Constants.ContainerModes.FCL || JS_PackingMode == Constants.ContainerModes.BuyersConsol)
			{
				return false;
			}

			var confirms = (confirmType == ConfirmTimesSyncHelper.ConfirmType.Pickup) ? PickupConfirms : DeliveryConfirms;

			foreach (PackLine packLine in OuterPackLines)
			{
				if (packLine.PackagesConfirmed(confirms) < packLine.JL_Calc_PackagesToDeliver)
				{
					return true;
				}
			}

			return false;
		}

		public virtual bool AutoCreateLooseConfirmations
		{
			get { return true; }
		}

		#endregion

		#region Consignee

		[MaxLength(3)]
		public ZString ConsigneeFieldType
		{
			get
			{
				return ConsigneeDocumentaryAddress.E2_AddressOverride ?

					nameof(FieldType.Text) :
					nameof(FieldType.OrganisationGuid);
			}
		}

		public ZPropertyInfo ConsigneeFieldTypeInfo
		{
			get { return GetZPropertyInfo(nameof(ConsigneeFieldType)); }
		}

		[BusinessObjectTestExclude]
		[RelatedBusinessObject("Consignee")]
		[List("Lookups.ConsigneeForwarder_List")]
		public virtual ZString ConsigneeNameOrPK
		{
			get { return ConsigneeDocumentaryAddress.OrganisationNameOrPK; }
			set { ConsigneeDocumentaryAddress.OrganisationNameOrPK = value; }
		}

		public ZPropertyInfo ConsigneeNameOrPKInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ConsigneeNameOrPK, x => ConsigneeDocumentaryAddress.OrganisationNameOrPKInfo); }
		}

		#endregion

		#region Consignor

		[MaxLength(3)]
		public ZString ConsignorFieldType
		{
			get
			{
				return ConsignorDocumentaryAddress.E2_AddressOverride ?

					nameof(FieldType.Text) :
					nameof(FieldType.OrganisationGuid);
			}
		}

		public ZPropertyInfo ConsignorFieldTypeInfo
		{
			get { return GetZPropertyInfo(nameof(ConsignorFieldType)); }
		}

		[BusinessObjectTestExclude]
		[RelatedBusinessObject("Consignor")]
		[List("Lookups.ConsignorForwarder_List")]
		public virtual ZString ConsignorNameOrPK
		{
			get { return ConsignorDocumentaryAddress.OrganisationNameOrPK; }
			set { ConsignorDocumentaryAddress.OrganisationNameOrPK = value; }
		}

		public ZPropertyInfo ConsignorNameOrPKInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ConsignorNameOrPK, x => ConsignorDocumentaryAddress.OrganisationNameOrPKInfo); }
		}

		#endregion

		#region JS_ChargeableUnit

		public ZString JS_ChargeableUnit
		{
			get { return ChargeableAmountCalculator.GetChargeableUnit(JS_TransportMode, JS_UnitOfWeight, JS_UnitOfVolume); }
		}

		public ZPropertyInfo JS_ChargeableUnitInfo
		{
			get { return GetZPropertyInfo(Schema.JS_ChargeableUnit); }
		}

		#endregion

		#region JS_Calc_CoLoadsGridLabel

		public ZString JS_Calc_CoLoadsGridLabel
		{
			get
			{
				string result = "";
				if (IsBuyersConsolLead)
				{
					result = Res.GetString("Freight|CommonShipment|BuyersConsolRelatedShipments", "Buyers Consol Related Shipments:");
				}
				else if (IsShippersConsolLead)
				{
					result = Res.GetString("Freight|CommonShipment|ShippersConsolRelatedShipments", "Shippers Consol Related Shipments:");
				}
				else if (IsAssemblyMaster)
				{
					result = Res.GetString("Freight|CommonShipment|AssemblySubShipments", "Assembly Sub Shipments:");
				}
				else
				{
					result = Res.GetString("Freight|CommonShipment|CoLoadSubShipments", "Co-Load Sub Shipments:");
				}
				return result;
			}
		}

		public ZPropertyInfo JS_Calc_CoLoadsGridLabelInfo
		{
			get { return GetZPropertyInfo(nameof(JS_Calc_CoLoadsGridLabel)); }
		}

		#endregion

		#region JS_PaymentTerm

		public ZString JS_PaymentTerm => RatingAdapter.PaymentTerm.GetPrepaidCollect(CostSell.Revenue, ChargeCodeGroupList.Codes.Freight);

		public ZPropertyInfo JS_PaymentTermInfo
		{
			get { return GetZPropertyInfo(Schema.JS_PaymentTerm); }
		}

		public ZString JS_PaymentTermDisplay
		{
			get
			{
				if (IsPrepaid)
				{
					return Res.GetString("ab33594d-478c-40cd-a0d3-9f5c9cef8d71", "Freight Prepaid");
				}
				else if (IsCollect)
				{
					return Res.GetString("8a6858bf-8c5b-4e15-b8ee-ccafba78ad14", "Freight Collect");
				}
				else
				{
					return "";
				}
			}
		}

		public ZPropertyInfo JS_PaymentTermDisplayInfo
		{
			get { return GetZPropertyInfo(nameof(JS_PaymentTermDisplay)); }
		}

		#endregion

		#region CopyBillsLabel

		public ZString JS_Calc_CopyBillsLabel
		{
			get { return JS_ReleaseType == Core.Constants.ShipmentReleaseTypes.ExpressBofL ? Res.GetString("770a4345-5e3c-4772-9087-e8530254c5fa", "Exp. Bills") : Res.GetString("f41597b1-0b45-4b1a-959f-777f8cf33364", "Copy Bills"); }
		}

		public ZPropertyInfo JS_Calc_CopyBillsLabelInfo
		{
			get { return GetZPropertyInfo(nameof(JS_Calc_CopyBillsLabel)); }
		}

		#endregion

		#region Notes Properties

		#region DetailedGoodsDescriptionNoteText

		[MaxLength(StmNote.Schema.ST_NoteTextMaxLength)]
		public ZString DetailedGoodsDescriptionNoteText
		{
			get
			{
				ZString result = "";
				StmNote note = DetailedGoodsDescriptionNote;
				if (note != null)
				{
					result = note.ST_NoteText;
				}
				return result;
			}
			set
			{
				StmNote note = DetailedGoodsDescriptionNote;
				if (note == null)
				{
					note = Notes.AddNew();
					note.ST_ParentID = PK;
					note.ST_Table = TableName;
					note.ST_IsCustomDescription = false;

					note.ST_Description = PredefinedNoteTypes.Instance.DetailedGoodsDescription.Description;
				}
				CheckMaximumLength(DetailedGoodsDescriptionNoteTextInfo, value);
				note.ST_NoteText = value;
				DetailedGoodsDescriptionNoteTextInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo DetailedGoodsDescriptionNoteTextInfo
		{
			get { return GetZPropertyInfo(Schema.DetailedGoodsDescriptionNoteText); }
		}

		protected virtual StmNote DetailedGoodsDescriptionNote
		{
			get
			{
				ZQuery filter = new ZQuery(StmNoteSchema.ST_ParentID, PK);
				filter.FetchOnlyFromLocalCache = !IsInDatabase;
				filter.AddToFilter(StmNoteSchema.ST_Description, PredefinedNoteTypes.Instance.DetailedGoodsDescription.Code);
				filter.AddToFilter(StmNoteSchema.ST_Table, TableName);
				return Factory.LoadTop1<StmNote>(filter);
			}
		}

		#endregion

		#region helper functions

		protected ZString GetShortText(ZString noteText)
		{
			int newLinePos = noteText.IndexOf("\r\n");
			int length = (newLinePos >= 35 || newLinePos == -1) ? 35 : newLinePos;

			return noteText.SubstringSafe(0, length);
		}

		protected ZString ReplaceFirstLine(ZString noteText, ZString shortText)
		{
			int newLinePos = noteText.IndexOf("\r\n");
			int start = (newLinePos >= 35 || newLinePos == -1) ? 35 : newLinePos;

			return shortText + noteText.SubstringSafe(start);
		}

		#endregion

		#endregion

		#region JS_Calc_Container*

		public virtual ZDecimal JS_Calc_TEUCount
		{
			//Applied ToArray() to Containers collection to prevent the exception "Collection was modified; enumeration operation may not execute."
			get { return Containers.ToArray().Cast<CommonContainer>().Sum(container => container.JC_Calc_TEUCount); }
		}

		public virtual ZInt JS_Calc_ContainerCount
		{
			get { return GetSumOfCalc_ContainerCount(container => true); }
		}

		public ZPropertyInfo JS_Calc_ContainerCountInfo
		{
			get { return GetZPropertyInfo(nameof(JS_Calc_ContainerCount)); }
		}

		public virtual ZInt JS_Calc_20GPCount
		{
			get { return GetSumOfCalc_ContainerCount(container => container.JC_Is20GP); }
		}

		public virtual ZInt JS_Calc_40GPCount
		{
			get { return GetSumOfCalc_ContainerCount(container => container.JC_Is40GP); }
		}

		public virtual ZInt JS_Calc_20RECount
		{
			get { return GetSumOfCalc_ContainerCount(container => container.JC_Is20RE); }
		}

		public virtual ZInt JS_Calc_40RECount
		{
			get { return GetSumOfCalc_ContainerCount(container => container.JC_Is40RE); }
		}

		public virtual ZInt JS_Calc_OtherContainerCount
		{
			get { return GetSumOfCalc_ContainerCount(container => container.JC_IsOtherContainerType); }
		}

		ZInt GetSumOfCalc_ContainerCount(Predicate<CommonContainer> predicate)
		{
			return Containers.ToArray().Cast<CommonContainer>().Where(container => predicate(container)).Sum(container => container.JC_Calc_ContainerCount);
		}

		#endregion

		#region JS_OrderReferences

		public ZString JS_OrderReferences
		{
			get { return DocsAndCartage.JP_OrderItemsAsString; }
		}

		public ZPropertyInfo JS_OrderReferencesInfo
		{
			get { return GetZPropertyInfo(nameof(JS_OrderReferences)); }
		}
		#endregion

		#region JS_ActualWeightCount_ReadOnly

		public ZDecimal JS_ActualWeightReadOnly
		{
			get { return JS_ActualWeight; }
		}

		public const string JS_ActualWeightReadOnlyName = "JS_ActualWeightReadOnly";
		public ZPropertyInfo JS_ActualWeightReadOnlyInfo
		{
			get { return GetZPropertyInfo(JS_ActualWeightReadOnlyName); }
		}

		#endregion

		#region JS_ActualVolume_ReadOnly

		public ZDecimal JS_ActualVolumeReadOnly
		{
			get { return JS_ActualVolume; }
		}

		public const string JS_ActualVolumeReadOnlyName = "JS_ActualVolumeReadOnly";
		public ZPropertyInfo JS_ActualVolumeReadOnlyInfo
		{
			get { return GetZPropertyInfo(JS_ActualVolumeReadOnlyName); }
		}

		#endregion

		#region Chargeable Amount

		[DecimalPlaces(2)]
		public ZDecimal ChargeableAmount
		{
			get { return JS_ActualChargeable * JS_UnitFreightRate; }
		}

		public ZPropertyInfo ChargeableAmountInfo
		{
			get { return GetZPropertyInfo(nameof(ChargeableAmount)); }
		}

		#endregion

		#region Master CoLoad

		public ZString ColoadMasterShipmentHouseBill
		{
			get
			{
				if (CoLoadMasterShipment != null)
				{
					return CoLoadMasterShipment.JS_HouseBill;
				}

				if (IsCoLoadMaster)
				{
					return JS_HouseBill;
				}

				return "";
			}
		}

		public ZPropertyInfo ColoadMasterShipmentHouseBillInfo
		{
			get { return GetZPropertyInfo(Schema.ColoadMasterShipmentHouseBill); }
		}

		public ZString ColoadMasterHouseBillList
		{
			get
			{
				return GetCoLoadMasterHouseBillList(this);
			}
		}

		ZString GetCoLoadMasterHouseBillList(CommonShipment shipment)
		{
			if (shipment.CoLoadMasterShipment != null)
			{
				return GetCoLoadMasterHouseBillList(shipment.CoLoadMasterShipment) + "=>" + shipment.JS_HouseBill;
			}
			else
			{
				return shipment.JS_HouseBill;
			}
		}

		#region JS_JS_ColoadMasterShipment

		[RelatedBusinessObject("CoLoadMasterShipment")]
		[List("Lookups.CoLoadMaster_List")]
		[ResourceStringData("11AD1E3A-2879-4D69-80E6-5636D4B47021", Caption = "Master / Lead")]
		public ZGuid JS_JS_ColoadMasterShipmentForBinding
		{
			get { return JS_JS_ColoadMasterShipment; }
			set
			{
				if (!Env.Security.MaintainShipmentAllowDetachingSubShipments.IsAllowed && value == ZGuid.Empty)
				{
					JS_JS_ColoadMasterShipmentForBindingInfo.RefreshBinding();

					if (ValueNotSet != null)
					{
						ValueNotSetEventArgs args = new ValueNotSetEventArgs(Schema.JS_JS_ColoadMasterShipment,
							Env.Security.GetErrorMessageForNotAllowed(Env.Security.MaintainShipmentAllowDetachingSubShipments));

						ValueNotSet(this, args);
					}
				}
				else
				{
					JS_JS_ColoadMasterShipment = value;
				}
			}
		}

		public ZPropertyInfo JS_JS_ColoadMasterShipmentForBindingInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.JS_JS_ColoadMasterShipmentForBinding, x => JS_JS_ColoadMasterShipmentInfo); }
		}

		[RelatedBusinessObject("CoLoadMasterShipment")]
		[List("Lookups.CoLoadMaster_List")]
		public override ZGuid JS_JS_ColoadMasterShipment
		{
			get { return base.JS_JS_ColoadMasterShipment; }
			set
			{
				if (value != JS_JS_ColoadMasterShipment && !IsChangingColoadMaster)
				{
					IsChangingColoadMaster = true;

					var oldMasterPK = base.JS_JS_ColoadMasterShipment;
					var newMasterPK = value;

					try
					{
						UnhookColoadFromPreviousMaster();

						// setting base.JS_JS_ColoadMasterShipment = value means when we access CoLoadMasterShipment.CoLoadShipments,
						// the Load() method will load this object from the factory as it now matches CoLoadShipments' filter. Totalling
						// is of course disabled during load. In order to maintain totalling integrity we must pre-initialise the
						// CoLoadShipmentCollection for the new Master Shipment.
						if (value.IsValid)
						{
							var masterShipment = (CommonShipment)Factory.Load(GetType(), value);
							if (masterShipment != null)
							{
								masterShipment.EnsureCoLoadShipmentCollectionIsInitialised();
							}
						}

						base.JS_JS_ColoadMasterShipment = value;

						ColoadMasterShipmentHouseBillInfo.RefreshBinding();

						if (CoLoadMasterShipment != null)
						{
							bool thisShipmentAddedToCollectionDuringCollectionLoad = CoLoadMasterShipment.CoLoadShipments.Contains(this);

							if (!thisShipmentAddedToCollectionDuringCollectionLoad && !CoLoadMasterShipment.CoLoadShipments.IsNonCommittedCollectionElement(this))
							{
								CoLoadMasterShipment.CoLoadShipments.Add(this);
							}

							AddCoLoadMasterShipmentAttachEvent();
							AttachColoadMasterShipmentConsols();
						}

						if (!IsRefreshingByDataRefreshBus)
						{
							OnMasterChanged(oldMasterPK, newMasterPK);
						}

						if (!IsValidationSuspended)
						{
							Validation.ValidateJS_JS_ColoadMasterShipment();
						}
					}
					finally
					{
						IsChangingColoadMaster = false;
					}
				}
			}
		}

		void AddCoLoadMasterShipmentAttachEvent()
		{
			var parameters = new Dictionary<string, string>()
			{
				[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type] = Constants.EventReferenceParameterTypes.Shipment
			};

			Logs.AddATCEvent(CoLoadMasterShipment.IsInDatabase, CoLoadMasterShipment.LogReference(true), parameters);
		}

		void UnhookColoadFromPreviousMaster()
		{
			if (JS_JS_ColoadMasterShipment.IsValid)
			{
				var previousMaster = CoLoadMasterShipment;
				if (previousMaster != null && previousMaster.CoLoadShipments.Contains(this))
				{
					previousMaster.CoLoadShipments.Remove(this);

					if (!IsDeleted)
					{
						Logs.AddDTCEvent(previousMaster.IsInDatabase, previousMaster.LogReference(true), new KeyValuePair<string, string>(EventConstants.EventReferenceParameters.Codes.Type, Core.Constants.EventReferenceParameterTypes.Shipment));
					}
				}
			}
		}

		void AttachColoadMasterShipmentConsols()
		{
			foreach (CommonConsol consol in CoLoadMasterShipment.Consols.Except(this.Consols))
			{
				var attachRequest = FreightShipmentVsConsolMessageHelper.Instance.IsAllowedToAttachConsol(this, consol);
				if (attachRequest.Errors.IsEmpty
					&& !HasConsolWithSameLoadOrDischargePort(consol)
					&& consol.Shipments.GetRelationshipBusinessObject(this) == null)
				{
					this.Consols.Add(consol);
				}
			}
		}

		bool HasConsolWithSameLoadOrDischargePort(CommonConsol consol)
		{
			return Consols.Cast<CommonConsol>()
					.Any(c => (!consol.JK_RL_NKLoadPort.IsEmpty && consol.JK_RL_NKLoadPort == c.JK_RL_NKLoadPort)
						|| (!consol.JK_RL_NKDischargePort.IsEmpty && consol.JK_RL_NKDischargePort == c.JK_RL_NKDischargePort));
		}

		bool IsChangingColoadMaster;

		public event EventHandler<MasterChangedEventArgs> MasterChanged;

		protected void OnMasterChanged(ZGuid oldMasterPK, ZGuid newMasterPK)
		{
			if (MasterChanged != null)
			{
				MasterChanged(this, new MasterChangedEventArgs(oldMasterPK, newMasterPK));
			}
		}

		#endregion

		#region Notes Properties

		#region JS_MarksAndNumbers

		[MaxLength(10000)]
		public ZString JS_MarksAndNumbers
		{
			get
			{
				ZString result = "";
				StmNote note = MarksAndNumbersNote;
				if (note != null)
				{
					if (note.ST_IsTextOnly)
					{
						result = note.ST_NoteText;
					}
					else
					{
						result = note.ST_NoteDataAsText;
					}
				}
				return result;
			}
			set
			{
				StmNote newMarksAndNumbers = MarksAndNumbersNote;
				if (newMarksAndNumbers == null)
				{
					newMarksAndNumbers = Notes.AddNew();
					newMarksAndNumbers.ST_ParentID = PK;
					newMarksAndNumbers.ST_Table = TableName;
					newMarksAndNumbers.ST_Description = PredefinedNoteTypes.Instance.MarksAndNumbers.Description;
				}

				CheckMaximumLength(JS_MarksAndNumbersInfo, value);

				ZBool valueShouldBeChanged = newMarksAndNumbers.ST_NoteText != value && value.Length > 0 || !newMarksAndNumbers.ST_NoteText.IsEmpty && value.Length == 0;

				if (value.Length > 0)
				{
					if (newMarksAndNumbers.ST_IsTextOnly)
					{
						newMarksAndNumbers.ST_NoteText = value;
					}
					else
					{
						newMarksAndNumbers.ST_NoteDataAsText = value;
					}
				}
				else
				{
					newMarksAndNumbers.Delete();
				}

				HasChanges = valueShouldBeChanged;

				if (!IsValidationSuspended)
				{
					Validation.ValidateJS_MarksAndNumbers();
				}
				JS_MarksAndNumbersInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo JS_MarksAndNumbersInfo
		{
			get { return GetZPropertyInfo(CommonShipment.Schema.JS_MarksAndNumbers); }
		}

		protected internal StmNote MarksAndNumbersNote
		{
			get
			{
				var filter = new ZQuery(StmNoteSchema.ST_ParentID, PK);
				filter.FetchOnlyFromLocalCache = !IsInDatabase;
				filter.AddToFilter(JoinCondition.And, StmNoteSchema.ST_Description, SQLComparisonOperator.Equal, PredefinedNoteTypes.Instance.MarksAndNumbers.Code);
				filter.AddToFilter(StmNoteSchema.ST_Table, TableName);
				var note = Factory.LoadTop1<StmNote>(filter);
				if (note != null)
				{
					note.NoteTextMaxLength = JS_MarksAndNumbersInfo.MaxLength;
				}
				return note;
			}
		}

		#endregion

		#region JS_MarksAndNumbersShort

		/// <summary>
		/// First line or first 35 characters of Marks and Numbers Notes.
		/// Used for binding to text box.
		/// </summary>
		[MaxLength(35)]
		public ZString JS_MarksAndNumbersShort
		{
			get { return GetShortText(JS_MarksAndNumbers); }
			set
			{
				if (value != JS_MarksAndNumbersShort)
				{
					CheckMaximumLength(JS_MarksAndNumbersShortInfo, value);
					JS_MarksAndNumbers = ReplaceFirstLine(JS_MarksAndNumbers, value);
					HasChanges = true;
				}
				JS_MarksAndNumbersShortInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo JS_MarksAndNumbersShortInfo
		{
			get { return GetZPropertyInfo(Schema.JS_MarksAndNumbersShort); }
		}

		#endregion

		#endregion

		#endregion

		#region Shipment Units/Package Types

		public ZString ShipmentOuterPacksUnit
		{
			get { return !JS_F3_NKPackType.IsEmpty ? JS_F3_NKPackType.ToString() : FreightPacksDataRegistry.Instance.OuterPackUnit.Value; }
		}

		public ZString ShipmentInnerPacksUnit
		{
			get { return !JS_F3_NKTotalCountPackType.IsEmpty ? JS_F3_NKTotalCountPackType.ToString() : FreightPacksDataRegistry.Instance.InnerPackUnit.Value; }
		}

		#endregion

		#region Inner PackLines Totals

		#region TotalInnerPackLinePackages

		public ZInt TotalInnerPackLinePackages
		{
			get { return InnerPackLines.TotalPackages; }
		}

		public ZPropertyInfo TotalInnerPackLinePackagesInfo
		{
			get { return GetZPropertyInfo(Schema.TotalInnerPackLinePackages); }
		}

		#endregion

		#region TotalInnerPackLineWeight

		public ZDecimal TotalInnerPackLineWeight
		{
			get { return this.GetRoundedValue(TotalInnerPackLineWeightInfo, GetPackLineTotalWeight(InnerPackLines)); }
		}

		public ZPropertyInfo TotalInnerPackLineWeightInfo
		{
			get { return GetZPropertyInfo(Schema.TotalInnerPackLineWeight); }
		}

		#endregion

		#region TotalInnerPackLineVolume

		public ZDecimal TotalInnerPackLineVolume
		{
			get { return this.GetRoundedValue(TotalInnerPackLineVolumeInfo, GetPackLineTotalVolume(InnerPackLines)); }
		}

		public ZPropertyInfo TotalInnerPackLineVolumeInfo
		{
			get { return GetZPropertyInfo(Schema.TotalInnerPackLineVolume); }
		}

		#endregion

		#region TotalInnerPackLineLoadingMeters

		[DecimalPlaces(3)]
		public ZDecimal TotalInnerPackLineLoadingMeters
		{
			get { return GetPackLineTotalLoadingMeters(InnerPackLines); }
		}

		public ZPropertyInfo TotalInnerPackLineLoadingMetersInfo
		{
			get { return GetZPropertyInfo(Schema.TotalInnerPackLineLoadingMeters); }
		}

		#endregion

		#region SyncMeasuresWithInnerPackLines

		public void SyncMeasuresWithInnerPackLines(bool requireCancelEventHandler = true)
		{
			ReloadInnerPackLines();

			if ((!requireCancelEventHandler || UpdatingShipmentVolumeFromPacks != null) && PacksTotalsDiffer())
			{
				var args = new CancelEventArgs();
				UpdatingShipmentVolumeFromPacks?.Invoke(this, args);
				if (!args.Cancel)
				{
					UpdateShipmentFromInnerPackLines();
				}
			}
		}

		public event CancelEventHandler UpdatingShipmentVolumeFromPacks;

		#endregion

		#region UpdateShipmentFromInnerPackLines

		public void UpdateShipmentFromInnerPackLines()
		{
			IsUpdatingShipmentFromPackLines = true;
			try
			{
				JS_TotalPackageCount = TotalInnerPackLinePackages;
				JS_F3_NKTotalCountPackType = InnerPackLines.TotalPackagesUnit;
			}
			finally
			{
				IsUpdatingShipmentFromPackLines = false;
			}
		}

		#endregion

		#endregion

		#region Outer PackLine Totals

		#region TotalOuterPacks

		public ZInt TotalOuterPacks
		{
			get { return OuterPackLines.TotalPackages; }
		}

		public ZPropertyInfo TotalOuterPacksInfo
		{
			get { return GetZPropertyInfo(Schema.TotalOuterPacks); }
		}

		#endregion

		#region TotalOuterPacksUnit

		[List("Lookups.JS_PackType_List")]
		public ZString TotalOuterPacksUnit
		{
			get { return ShipmentOuterPacksUnit; }
		}

		public ZPropertyInfo TotalOuterPacksUnitInfo
		{
			get { return GetZPropertyInfo(Schema.TotalOuterPacksUnit); }
		}

		#endregion

		#region TotalOuterPacksWeight

		/// <summary>
		/// Total Weight of Outer Packlines converted to Shipment unit
		/// </summary>
		public ZDecimal TotalOuterPacksWeight
		{
			get { return this.GetRoundedValue(TotalOuterPacksWeightInfo, GetPackLineTotalWeight(OuterPackLines)); }
		}

		public ZPropertyInfo TotalOuterPacksWeightInfo
		{
			get { return GetZPropertyInfo(Schema.TotalOuterPacksWeight); }
		}

		#endregion

		#region TotalOuterPacksVolume

		/// <summary>
		/// Total Volume of Outer Packlines converted to Shipment unit and rounded
		/// </summary>
		public ZDecimal TotalOuterPacksVolume
		{
			get { return this.GetRoundedValue(TotalOuterPacksVolumeInfo, GetPackLineTotalVolume(OuterPackLines)); }
		}

		public ZPropertyInfo TotalOuterPacksVolumeInfo
		{
			get { return GetZPropertyInfo(Schema.TotalOuterPacksVolume); }
		}

		#endregion

		#region TotalOuterPacksLoadingMeters

		[DecimalPlaces(3)]
		public ZDecimal TotalOuterPacksLoadingMeters
		{
			get { return GetPackLineTotalLoadingMeters(OuterPackLines); }
		}

		public ZPropertyInfo TotalOuterPacksLoadingMetersInfo
		{
			get { return GetZPropertyInfo(Schema.TotalOuterPacksLoadingMeters); }
		}

		#endregion

		#region TotalOuterPacksPillaged

		public ZInt TotalOuterPacksPillaged
		{
			get
			{
				try
				{
					return Convert.ToInt32(TotalCalculation.GetTotal(OuterPackLines, PackLine.Schema.JL_Pillaged));
				}
				catch (OverflowException)
				{
					return int.MaxValue;
				}
			}
		}

		public ZPropertyInfo TotalOuterPacksPillagedInfo
		{
			get { return GetZPropertyInfo(Schema.TotalOuterPacksPillaged); }
		}

		#endregion

		#region TotalOuterPacksDamaged

		public ZInt TotalOuterPacksDamaged
		{
			get
			{
				try
				{
					return Convert.ToInt32(TotalCalculation.GetTotal(OuterPackLines, PackLine.Schema.JL_Damaged));
				}
				catch (OverflowException)
				{
					return int.MaxValue;
				}
			}
		}

		public ZPropertyInfo TotalOuterPacksDamagedInfo
		{
			get { return GetZPropertyInfo(Schema.TotalOuterPacksDamaged); }
		}

		#endregion

		#region TotalPackLineVolumeUnit

		[List("Lookups.JS_UnitOfVolume_List")]
		public ZString TotalPackLineVolumeUnit
		{
			get { return ShipmentVolumeUnit; }
		}

		public ZPropertyInfo TotalPackLineVolumeUnitInfo
		{
			get { return GetZPropertyInfo(Schema.TotalPackLineVolumeUnit); }
		}

		#endregion

		#region TotalPackLineWeightUnit

		[List("Lookups.JS_UnitOfWeight_List")]
		public ZString TotalPackLineWeightUnit
		{
			get { return ShipmentWeightUnit; }
		}

		public ZPropertyInfo TotalPackLineWeightUnitInfo
		{
			get { return GetZPropertyInfo(Schema.TotalPackLineWeightUnit); }
		}

		#endregion

		#region Imperial Conversions

		public ZDecimal TotalOuterPacksWeight_Imperial
		{
			get
			{
				var result = Constants.Weight.Convert(TotalOuterPacksWeight, TotalPackLineWeightUnit, Constants.Weight.Pounds, false);
				return this.GetRoundedValue(TotalOuterPacksWeight_ImperialInfo, result);
			}
		}

		public ZPropertyInfo TotalOuterPacksWeight_ImperialInfo
		{
			get { return GetZPropertyInfo(Schema.TotalOuterPacksWeight_Imperial); }
		}

		public ZDecimal TotalOuterPacksVolume_Imperial
		{
			get
			{
				var result = Constants.Volume.Convert(TotalOuterPacksVolume, TotalPackLineVolumeUnit, Constants.Volume.CubicFeet, false);
				return this.GetRoundedValue(TotalOuterPacksVolume_ImperialInfo, result);
			}
		}

		public ZPropertyInfo TotalOuterPacksVolume_ImperialInfo
		{
			get { return GetZPropertyInfo(Schema.TotalOuterPacksVolume_Imperial); }
		}

		public ZDecimal JS_ActualWeight_Imperial
		{
			get
			{
				var result = Constants.Weight.Convert(JS_ActualWeight, TotalPackLineWeightUnit, Constants.Weight.Pounds, false);
				return this.GetRoundedValue(JS_ActualWeight_ImperialInfo, result);
			}
		}

		public ZPropertyInfo JS_ActualWeight_ImperialInfo
		{
			get { return GetZPropertyInfo(Schema.JS_ActualWeight_Imperial); }
		}

		public ZDecimal JS_ActualVolume_Imperial
		{
			get
			{
				var result = Constants.Volume.Convert(JS_ActualVolume, TotalPackLineVolumeUnit, Constants.Volume.CubicFeet, false);
				return this.GetRoundedValue(JS_ActualVolume_ImperialInfo, result);
			}
		}

		public ZPropertyInfo JS_ActualVolume_ImperialInfo
		{
			get { return GetZPropertyInfo(Schema.JS_ActualVolume_Imperial); }
		}

		#endregion

		#endregion

		#region ResetAllValuesFromSubs

		public void ResetAllValuesFromSubs()
		{
			if (CoLoadShipments != null)
			{
				JS_UnitOfWeight = CoLoadShipments.GetCommonUnit(CommonShipment.Schema.JS_UnitOfWeight, Env.Registry.FreightWeightUnit);
				JS_ActualWeight = TotalCalculation.GetTotalWeight(CoLoadShipments, CommonShipment.Schema.JS_ActualWeight, CommonShipment.Schema.JS_UnitOfWeight, JS_UnitOfWeight);
				JS_UnitOfVolume = CoLoadShipments.GetCommonUnit(CommonShipment.Schema.JS_UnitOfVolume, Env.Registry.FreightVolumeUnit);
				JS_ActualVolume = TotalCalculation.GetTotalVolume(CoLoadShipments, CommonShipment.Schema.JS_ActualVolume, CommonShipment.Schema.JS_UnitOfVolume, JS_UnitOfVolume);
				JS_LoadingMeters = CoLoadShipments.Cast<CommonShipment>().Sum(shipment => shipment.JS_LoadingMeters);
				UpdateChargeableWeights();
				JS_OuterPacks = ZInt.ParseSafe(TotalCalculation.GetTotal(CoLoadShipments, CommonShipment.Schema.JS_OuterPacks).ToString(), 0);
				JS_F3_NKTotalCountPackType = CoLoadShipments.GetCommonUnit(CommonShipment.Schema.JS_F3_NKPackType, Constants.PkgUnit.Package);
				JS_TotalPackageCount = ZInt.ParseSafe(TotalCalculation.GetTotal(CoLoadShipments, CommonShipment.Schema.JS_TotalPackageCount).ToString(), 0);
				JS_F3_NKPackType = CoLoadShipments.GetCommonUnit(CommonShipment.Schema.JS_F3_NKPackType, Constants.PkgUnit.Package);
				JS_OuterPacks = ZInt.ParseSafe(TotalCalculation.GetTotal(CoLoadShipments, CommonShipment.Schema.JS_OuterPacks).ToString(), 0);

				JS_RX_NKGoodsValueCurr = CoLoadShipments.GetCommonUnit(CommonShipment.Schema.JS_RX_NKGoodsValueCurr, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);
				ZDecimal totalValue = 0m;
				foreach (CommonShipment shipment in CoLoadShipments)
				{
					RefCurrency commonCurrency = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, JS_RX_NKGoodsValueCurr));

					if (shipment.PK != PK && commonCurrency != null)
					{
						RefCurrency currency = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, shipment.JS_RX_NKGoodsValueCurr));
						if (currency != null)
						{
							totalValue += currency.ConvertUsingSellRate(ZDateTime.Now, shipment.JS_GoodsValue, commonCurrency);
						}
					}
				}
				JS_GoodsValue = totalValue;

				JS_RX_NKInsuranceCurrency = CoLoadShipments.GetCommonUnit(CommonShipment.Schema.JS_RX_NKInsuranceCurrency, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);
				totalValue = 0m;
				foreach (CommonShipment shipment in CoLoadShipments)
				{
					RefCurrency commonCurrency = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, JS_RX_NKInsuranceCurrency));

					if (shipment.PK != PK && commonCurrency != null)
					{
						RefCurrency currency = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, shipment.JS_RX_NKInsuranceCurrency));
						if (currency != null)
						{
							totalValue += currency.ConvertUsingSellRate(ZDateTime.Now, shipment.JS_InsuranceValue, commonCurrency);
						}
					}
				}
				JS_InsuranceValue = totalValue;
			}
		}

		#endregion

		#region Sailing Calculated Properties

		#region JS_Calc_CurrentVessel

		public virtual ZString JS_Calc_CurrentVessel
		{
			get
			{
				ZString result;

				if (MostInterestingTransport != null)
				{
					result = MostInterestingTransport.JW_Vessel;
				}
				else if (Sailing != null)
				{
					result = Sailing.JX_JV_NKVessel;
				}
				else
				{
					result = ZString.Empty;
				}

				return result;
			}
		}

		public ZPropertyInfo JS_Calc_CurrentVesselInfo
		{
			get { return GetZPropertyInfo(Schema.JS_Calc_CurrentVessel); }
		}

		#endregion

		#region JS_Calc_CurrentVoyageFlight

		public virtual ZString JS_Calc_CurrentVoyageFlight
		{
			get
			{
				ZString result;

				if (MostInterestingTransport != null)
				{
					result = MostInterestingTransport.JW_VoyageFlight;
				}
				else if (Sailing != null)
				{
					result = Sailing.JX_JV_VoyageFlight;
				}
				else
				{
					result = ZString.Empty;
				}

				return result;
			}
		}

		public ZPropertyInfo JS_Calc_CurrentVoyageFlightInfo
		{
			get { return GetZPropertyInfo(Schema.JS_Calc_CurrentVoyageFlight); }
		}

		#endregion

		#region JS_Calc_CurrentLoadPort

		[MaxLength(VoyageOrigin.Schema.JA_RL_NKPortOfLoadingMaxLength)]
		public ZString JS_Calc_CurrentLoadPort
		{
			get
			{
				ZString result;

				if (MostInterestingTransport != null)
				{
					result = MostInterestingTransport.JW_RL_NKLoadPort;
				}
				else if (Sailing != null)
				{
					result = Sailing.JX_JA_RL_NKPortOfLoading;
				}
				else
				{
					result = ZString.Empty;
				}

				return result;
			}
		}

		public ZPropertyInfo JS_Calc_CurrentLoadPortInfo
		{
			get { return GetZPropertyInfo(Schema.JS_Calc_CurrentLoadPort); }
		}

		#endregion

		#region JS_Calc_CurrentDischargePort

		[MaxLength(VoyageDestination.Schema.JB_RL_NKPortOfDischargeMaxLength)]
		public ZString JS_Calc_CurrentDischargePort
		{
			get
			{
				ZString result;

				if (MostInterestingTransport != null)
				{
					result = MostInterestingTransport.JW_RL_NKDiscPort;
				}
				else if (Sailing != null)
				{
					result = Sailing.JX_JB_RL_NKPortOfDischarge;
				}
				else
				{
					result = ZString.Empty;
				}

				return result;
			}
		}

		public ZPropertyInfo JS_Calc_CurrentDischargePortInfo
		{
			get { return GetZPropertyInfo(Schema.JS_Calc_CurrentDischargePort); }
		}

		#endregion

		#region JS_Calc_CurrentETD

		public ZPropertyInfo JS_Calc_CurrentETDInfo
		{
			get
			{
				return GetZPropertyInfo(Schema.JS_Calc_CurrentETD);
			}
		}

		public ZDateTime JS_Calc_CurrentETD
		{
			get
			{
				ZDateTime result;

				if (MostInterestingTransport != null)
				{
					result = MostInterestingTransport.JW_ETD;
				}
				else if (Sailing != null)
				{
					result = Sailing.JX_JA_E_DEP;
				}
				else
				{
					result = ZDateTime.Empty;
				}
				return result;
			}
		}

		#endregion

		#region JS_Calc_CurrentETA

		public ZDateTime JS_Calc_CurrentETA
		{
			get
			{
				ZDateTime result;

				if (MostInterestingTransport != null)
				{
					result = MostInterestingTransport.JW_ETA;
				}
				else if (Sailing != null)
				{
					result = Sailing.JX_JB_E_ARV;
				}
				else
				{
					result = ZDateTime.Empty;
				}

				return result;
			}
		}

		public ZPropertyInfo JS_Calc_CurrentETAInfo
		{
			get { return GetZPropertyInfo(Schema.JS_Calc_CurrentETA); }
		}

		#endregion

		#region JS_Calc_LastDischargePort

		public virtual ZString JS_Calc_LastDischargePort
		{
			get
			{
				CommonConsol arrivalConsol = this.ArrivalConsol;
				return (arrivalConsol != null) ? arrivalConsol.JK_RL_NKDischargePort : ZString.Empty;
			}
		}

		public ZPropertyInfo JS_Calc_LastDischargePortInfo
		{
			get { return GetZPropertyInfo(Schema.JS_Calc_LastDischargePort); }
		}

		public virtual RefUNLOCO LastPortOfDischarge
		{
			get { return Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, JS_Calc_LastDischargePort); }
		}

		#endregion

		#region JS_Calc_LastETA

		public virtual ZDateTime JS_Calc_LastETA
		{
			get
			{
				CommonConsol consol = ArrivalConsol;
				return (consol?.Transports.ArrivalTransport == null) ? ZDateTime.Empty : consol.Transports.ArrivalTransport.JW_ETA;
			}
		}

		public ZPropertyInfo JS_Calc_LastETAInfo
		{
			get { return GetZPropertyInfo(Schema.JS_Calc_LastETA); }
		}

		#endregion

		#endregion

		#region Location

		[List("Lookups.Warehouses")]
		public ZGuid LocationWhsGuid
		{
			get { return WhsLocation?.WLV_WW_Whs ?? locationWhsGuid; }
			set
			{
				locationWhsGuid = value;
				LocationWhsGuidInfo.RefreshBinding();
			}
		}

		ZGuid locationWhsGuid;

		public ZPropertyInfo LocationWhsGuidInfo
		{
			get { return GetZPropertyInfo(Schema.LocationWhsGuid); }
		}

		public IWhsLocation WhsLocation => Factory.Load<IWhsLocation>(JS_WL);

		[MaxLength(36)]
		public virtual ZString LocationString
		{
			get { return WhsLocation?.WLV_LocationString ?? locationString; }
			set
			{
				var location = WhsLocation;
				if (location != null && location.WLV_LocationString != value || locationString != value || !JS_WL.IsValid)
				{
					CheckMaximumLength(LocationStringInfo, value);
					locationString = value;
					var whsLocationDataHelper = ObjectFactory.New<IWhsLocationDataHelper>();
					JS_WL = whsLocationDataHelper.FindLocationPK(Factory, value, LocationWhsGuid);

					if (!IsValidationSuspended)
					{
						Validation.ValidateLocationString();
					}
				}
				LocationStringInfo.RefreshBinding();
			}
		}

		ZString locationString;

		public virtual ZPropertyInfo LocationStringInfo
		{
			get { return GetZPropertyInfo(Schema.LocationString); }
		}

		#endregion

		#region ConsigneeDocumentaryAddressChanged

		protected virtual void ConsigneeDocumentaryAddressChanged()
		{
			using (SuspendSettingConsigneeFromDestination())
			{
				MarkAsNeedingValidation();

				if (!IsSettingDefaultValues)
				{
					SetConsigneeChangeImplications();
				}
			}
		}

		void SetConsigneeChangeImplications()
		{
			if (!SettingChangeImplications && !IsSettingDefaultOrImportingData)
			{
				try
				{
					SettingChangeImplications = true;

					SetImportCartage();
					SetDeliveryEquipmentNeeded();
					SetDestinationToConsigneeLocation();
					SetDefaultNotifyParty();

					SetConsigneeChanges();
					SetConsignorConsigneeCommonChanges();

					UpdateETDeliveryWithPortDefaultDeliveryTime();
				}
				finally
				{
					SettingChangeImplications = false;
				}
			}
		}

		#region Setting Consignee From Destination Suspender

		public IDisposable SuspendSettingConsigneeFromDestination()
		{
			return new DisposableAction(() => settingConsigneeFromDestinationSuspendedIndex++, () => settingConsigneeFromDestinationSuspendedIndex--);
		}

		bool IsSettingConsigneeFromDestinationSuspended
		{
			get { return settingConsigneeFromDestinationSuspendedIndex > 0; }
		}

		int settingConsigneeFromDestinationSuspendedIndex;

		#endregion

		#endregion

		#region ConsignorDocumentaryAddressChanged

		protected virtual void ConsignorDocumentaryAddressChanged()
		{
			using (SuspendSettingConsignorFromOrigin())
			{
				MarkAsNeedingValidation();

				if (!IsSettingDefaultValues)
				{
					SetConsignorChangeImplications();
				}
			}
		}

		void SetConsignorChangeImplications()
		{
			if (!SettingChangeImplications && !IsSettingDefaultOrImportingData)
			{
				try
				{
					SettingChangeImplications = true;

					SetExportCartage();
					SetPickupEquipmentNeeded();
					SetOriginToConsignorLocation();

					SetConsignorChanges();
					SetConsignorConsigneeCommonChanges();
				}
				finally
				{
					SettingChangeImplications = false;
				}
			}
		}

		#region Setting Consignor From Origin Suspender

		public IDisposable SuspendSettingConsignorFromOrigin()
		{
			return new DisposableAction(() => settingConsignorFromOriginSuspendedIndex++, () => settingConsignorFromOriginSuspendedIndex--);
		}

		bool IsSettingConsignorFromOriginSuspended
		{
			get { return settingConsignorFromOriginSuspendedIndex > 0; }
		}

		int settingConsignorFromOriginSuspendedIndex;

		#endregion

		#endregion

		#region ConsignorDocumentaryOrgHeaderChanged

		protected virtual void ConsignorDocumentaryOrgHeaderChanged()
		{
			UpdateExportBroker();
			ReloadNotes();
		}

		#endregion

		#region ConsigneeDocumentaryOrgHeaderAfterChange

		protected virtual void ConsigneeDocumentaryOrgHeaderChanged()
		{
			UpdateImportBroker();
			ReloadNotes();
		}

		#endregion

		#region ConsigneeDeliveryAddressChanged

		protected virtual void ConsigneeDeliveryAddressChanged()
		{
			if (!IsSettingDefaultOrImportingData)
			{
				SetDeliveryEquipmentNeeded();
				if (UserHasEnteredDeliverAddressControl)
				{
					DeliveryAddressHasBeenChangedByShipmentUser = true;
					ConsigneeDeliveryAddress.Validation.ValidateOrganisationPK();
				}

				DefaultDeliveryCompany();

				JS_Calc_ACIConsigneeDestinationZoneInfo.RefreshBinding();
				JS_Calc_DeliveryCartageZoneInfo.RefreshBinding();
			}
		}

		public bool UserHasEnteredDeliverAddressControl { get; set; }
		public bool DeliveryAddressHasBeenChangedByShipmentUser { get; set; }

		#endregion

		#region DefaultDeliveryCompany

		public void DefaultDeliveryCompany()
		{
			if (ConsigneeDeliveryAddress.HasRealOrganisation)
			{
				OrgAddress newAddress = Factory.Load<OrgAddress>(ConsigneeDeliveryAddress.E2_OA_Address);
				if (newAddress != null && newAddress.Header != null)
				{
					OrgHeader relatedParty = newAddress.Header.GetRelatedPartyWithAddressFallback(newAddress.PK, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Delivery, JS_TransportMode, JS_PackingMode, JS_RL_NKDestination);
					if (relatedParty != null)
					{
						DocsAndCartage.JP_OA_DeliveryCartageCoAddr = relatedParty.MainAddress.PK;
					}
				}
			}
		}

		#endregion

		#region ConsignorPickupAddressChanged

		protected virtual void ConsignorPickupAddressChanged()
		{
			if (!IsSettingDefaultOrImportingData)
			{
				SetPickupEquipmentNeeded();
				DefaultPickupCompany();
				JS_Calc_ACIConsignorOriginZoneInfo.RefreshBinding();
				JS_Calc_PickupCartageZoneInfo.RefreshBinding();
			}
		}

		#endregion

		#region DefaultPickupCompany

		public void DefaultPickupCompany()
		{
			if (ConsignorPickupAddress.HasRealOrganisation)
			{
				OrgAddress newAddress = Factory.Load<OrgAddress>(ConsignorPickupAddress.E2_OA_Address);
				if (newAddress != null && newAddress.Header != null)
				{
					OrgHeader relatedParty = newAddress.Header.GetRelatedPartyWithAddressFallback(newAddress.PK, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Pickup, JS_TransportMode, JS_PackingMode, JS_RL_NKOrigin);
					if (relatedParty != null)
					{
						DocsAndCartage.JP_OA_PickupCartageCoAddr = relatedParty.MainAddress.PK;
					}
				}
			}
		}

		#endregion

		#region JS_PackingMode

		[List("Lookups.JS_PackingMode_List")]
		public override ZString JS_PackingMode
		{
			get { return IsDeleted ? ZString.Empty : base.JS_PackingMode; }
			set
			{
				base.JS_PackingMode = value;
				if (!IsSettingDefaultValues)
				{
					SetPackingModeChangeImplications();
				}

				if (!fIsImportingData)
				{
					UpdateHBLContainerPackModeOverride();
				}

				MarkDocsAndCartageAsNeedsValidation();
				foreach (var packLine in OuterPackLines)
				{
					packLine.MarkAsNeedingValidation();
				}
				if (JS_PackingMode != Constants.ContainerModes.BuyersConsol && IsBuyersConsolLead)
				{
					JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
				}
			}
		}

		internal virtual void SetPackingModeChangeImplications()
		{
			if (!SettingChangeImplications && !IsSettingDefaultOrImportingData)
			{
				try
				{
					SettingChangeImplications = true;

					SetExportCartage();
					SetPickupEquipmentNeeded();
					SetImportCartage();
					SetDeliveryEquipmentNeeded();

					RefreshVisibleBindingsForTransportAndPackingModeChange();
				}
				finally
				{
					SettingChangeImplications = false;
				}
			}
		}

		#endregion

		#region JS_TransportMode

		internal virtual void SetTransportModeChangeImplications()
		{
			if (!SettingChangeImplications && !IsSettingDefaultOrImportingData)
			{
				try
				{
					SettingChangeImplications = true;

					SetDefaultExportBroker();
					SetExportCartage();
					SetPickupEquipmentNeeded();
					SetImportCartage();
					SetDeliveryEquipmentNeeded();
					SetDefaultImportBroker();

					JS_OH_ExportBrokerInfo.RefreshBinding();
					JS_OH_ImportBrokerInfo.RefreshBinding();

					RefreshVisibleBindingsForTransportAndPackingModeChange();
				}
				finally
				{
					SettingChangeImplications = false;
				}
			}
		}

		#endregion

		#region JS_GoodsDescription

		public override ZString JS_GoodsDescription
		{
			get { return base.JS_GoodsDescription; }
			set
			{
				ZString previousDescription = base.JS_GoodsDescription;
				if (value != previousDescription)
				{
					base.JS_GoodsDescription = value;
					if (UpdatePackLines)
					{
						UpdateRelevantPackLinesDescription(previousDescription, value);
					}
				}
			}
		}

		#endregion

		#region JS_F3_NKPackType

		[List("Lookups.JS_PackType_List")]
		public override ZString JS_F3_NKPackType
		{
			get { return base.JS_F3_NKPackType; }
			set
			{
				ZString previousPackType = base.JS_F3_NKPackType;
				EnsureCoLoadShipmentsAreLoaded();
				if (previousPackType != value)
				{
					base.JS_F3_NKPackType = value;
					if (UpdatePackLines)
					{
						OuterPackLines.NotifyPackageTypeChanged(previousPackType, value);
					}
				}
			}
		}

		#endregion

		#region JS_F3_NKTotalCountPackType

		[List("Lookups.JS_PackType_List")]
		public override ZString JS_F3_NKTotalCountPackType
		{
			get { return base.JS_F3_NKTotalCountPackType; }
			set
			{
				ZString previousPackType = base.JS_F3_NKTotalCountPackType;
				EnsureCoLoadShipmentsAreLoaded();
				if (value != previousPackType)
				{
					base.JS_F3_NKTotalCountPackType = value;
					if (UpdatePackLines)
					{
						InnerPackLines.NotifyPackageTypeChanged(previousPackType, value);
					}
				}
			}
		}

		#endregion

		#region JS_UnitOfVolume

		[List("Lookups.JS_UnitOfVolume_List")]
		public override ZString JS_UnitOfVolume
		{
			get { return base.JS_UnitOfVolume; }
			set
			{
				ZString previousUnitOfVolume = base.JS_UnitOfVolume;
				EnsureCoLoadShipmentsAreLoaded();
				if (value != previousUnitOfVolume)
				{
					base.JS_UnitOfVolume = value;

					base.JS_ActualVolume = this.GetRoundedValue(JobShipmentSchema.JS_ActualVolume, JS_ActualVolumeInfo, JS_ActualVolume);
					this.SetRoundedValue(JobShipmentSchema.JS_DocumentedVolume, JS_DocumentedVolumeInfo);
					this.SetRoundedValue(JobShipmentSchema.JS_ManifestedVolume, JS_ManifestedVolumeInfo);

					if (UpdatePackLines)
					{
						UpdateRelevantPackLinesVolumeUQ(previousUnitOfVolume, value);
						GlobalCommercialInvoiceUpdateUnitOfMeasurement(JS_UnitOfVolumeInfo, previousUnitOfVolume, value);
					}
					UpdateChargeableWeights();
					RefreshCalculatedVolumeWeightValues();
				}
			}
		}

		#endregion

		#region JS_UnitOfWeight

		[List("Lookups.JS_UnitOfWeight_List")]
		[SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", Justification = "Calling ContainersForBinding getter")]
		public override ZString JS_UnitOfWeight
		{
			get { return base.JS_UnitOfWeight; }
			set
			{
				ZString previousUnitOfWeight = base.JS_UnitOfWeight;
				EnsureCoLoadShipmentsAreLoaded();
				if (value != previousUnitOfWeight)
				{
					base.JS_UnitOfWeight = value;

					base.JS_ActualWeight = this.GetRoundedValue(JobShipmentSchema.JS_ActualWeight, JS_ActualWeightInfo, JS_ActualWeight);
					this.SetRoundedValue(JobShipmentSchema.JS_DocumentedWeight, JS_DocumentedWeightInfo);
					this.SetRoundedValue(JobShipmentSchema.JS_ManifestedWeight, JS_ManifestedWeightInfo);

					if (UpdatePackLines)
					{
						UpdateRelevantPackLinesWeightUQ(previousUnitOfWeight, value);
						GlobalCommercialInvoiceUpdateUnitOfMeasurement(JS_UnitOfWeightInfo, previousUnitOfWeight, value);
						var ensureLoadedToGetFatalNotifications = ContainersForBinding;
					}

					using (new DisposableAction(() => isSettingUnitOfWeight = true, () => isSettingUnitOfWeight = false))
					{
						UpdateChargeableWeights();
					}

					RefreshCalculatedVolumeWeightValues();
				}
			}
		}

		protected bool isSettingUnitOfWeight;

		#endregion

		#region JS_ShippedOnBoardDate / JS_ShippedOnBoard

		[BusinessObjectTestExclude]
		[EventDateProperty(AutoEvents.FreightLoadedCode, EstimateActual.Actual, shouldOnlyUpdateEmptyDate: true)]
		[EventDateProperty(AutoEvents.CargoReceivedAtDepotCode, EstimateActual.Actual)]
		public override ZDateTime JS_ShippedOnBoardDate
		{
			get { return new ZDateTime(base.JS_ShippedOnBoardDate, DateTimeKind.Unspecified); }
			set
			{
				if (base.JS_ShippedOnBoardDate != value)
				{
					base.JS_ShippedOnBoardDate = value;
					CreateRecreateOrUpdateShippedOnBoardEvent(value.ToOffset());
				}
			}
		}

		[List("Lookups.JS_ShippedOnBoard_List")]
		public override ZString JS_ShippedOnBoard
		{
			get { return base.JS_ShippedOnBoard; }
			set
			{
				if (base.JS_ShippedOnBoard != value)
				{
					CreateRecreateOrUpdateShippedOnBoardEvent(ZDateTimeOffset.Empty);
					base.JS_ShippedOnBoard = value;
					CreateRecreateOrUpdateShippedOnBoardEvent(JS_ShippedOnBoardDate.ToOffset());
				}
			}
		}

		void CreateRecreateOrUpdateShippedOnBoardEvent(ZDateTimeOffset date)
		{
			if (IsSettingDefaultValues)
			{
				return;
			}

			if (JS_ShippedOnBoard == FreightConstants.ShippedOnBoardType.Received)
			{
				Logs.CreateRecreateOrUpdateEventLog(AutoEvents.CargoReceivedAtDepot, EstimateActual.Actual, date);
			}
			else
			{
				var eventParameters = GetParametersForEvent(AutoEvents.FreightLoaded).ToArray();
				Logs.CreateOrRecreateEventLog(AutoEvents.FreightLoaded, EstimateActual.Actual, date, "", eventParameters);
			}
		}

		#endregion

		#region Fields from Consol

		#region JS_JK_MasterBill

		public ZString JS_JK_MasterBillNum
		{
			get { return (Consols.Count > 0) ? Consols[0].JK_MasterBillNum : ZString.Empty; }
		}

		public ZPropertyInfo JS_JK_MasterBillNumInfo
		{
			get { return GetZPropertyInfo(Schema.JS_JK_MasterBillNum); }
		}

		#endregion

		#region JS_JK_MasterBillNumbers

		public ZString JS_JK_MasterBillNumbers
		{
			get
			{
				return (Consols.Count > 0) ?
					ZString.Join(", ", Consols.ToArray<CommonConsol>()
						.OrderBy(s => s.JK_UniqueConsignRef)
						.Select(c => c.JK_MasterBillNum.IsEmpty ? (NoResString)"<none>" : c.JK_MasterBillNum)
						.ToArray())
					: ZString.Empty;
			}
		}

		#endregion

		#region JS_JK_ConsolID

		public ZString JS_JK_ConsolID
		{
			get
			{
				return (Consols.Count > 0)
					? Consols.ToArray<CommonConsol>().Select(c => c.JK_UniqueConsignRef).OrderBy(s => s).Aggregate((r, n) => r + ", " + n)
					: ZString.Empty;
			}
		}

		public ZPropertyInfo JS_JK_ConsolIDInfo
		{
			get { return GetZPropertyInfo(Schema.JS_JK_ConsolID); }
		}

		#endregion

		#region JS_JK_Vessel

		public ZString JS_JK_Vessel
		{
			get { return Consols.Count > 0 ? Consols[0].JK_JX_JV_NKVessel : ZString.Empty; }
		}

		public ZPropertyInfo JS_JK_VesselInfo
		{
			get { return GetZPropertyInfo(Schema.JS_JK_Vessel); }
		}

		#endregion

		#region JS_JK_VoyageFlight

		public ZString JS_JK_VoyageFlight
		{
			get { return Consols.Count > 0 ? Consols[0].JK_JX_JV_VoyageFlight : ZString.Empty; }
		}

		public ZPropertyInfo JS_JK_VoyageFlightInfo
		{
			get { return GetZPropertyInfo(Schema.JS_JK_VoyageFlight); }
		}

		#endregion

		#region Sending Agent

		public ZString JS_JK_SendingAgent
		{
			get
			{
				if (Consols.Count > 0 && Consols[0].SendingForwarder != null)
				{
					return Consols[0].SendingForwarder.OH_Code;
				}
				else
				{
					return ZString.Empty;
				}
			}
		}

		public ZPropertyInfo JS_JK_SendingAgentInfo
		{
			get { return GetZPropertyInfo(Schema.JS_JK_SendingAgent); }
		}

		#endregion

		#region Receiving Agent

		public ZString JS_JK_ReceivingAgent
		{
			get
			{
				if (Consols.Count > 0 && Consols[0].ReceivingForwarder != null)
				{
					return Consols[0].ReceivingForwarder.OH_Code;
				}
				else
				{
					return ZString.Empty;
				}
			}
		}

		public ZPropertyInfo JS_JK_ReceivingAgentInfo
		{
			get { return GetZPropertyInfo(Schema.JS_JK_ReceivingAgent); }
		}

		#endregion

		#region JS_JK_Carrier

		public ZGuid JS_JK_Carrier
		{
			get { return (Consols.Count > 0) ? Consols[0].ShippingLinePK : ZGuid.Empty; }
		}

		public ZPropertyInfo JS_JK_CarrierInfo
		{
			get { return GetZPropertyInfo(nameof(JS_JK_Carrier)); }
		}

		#endregion

		#endregion

		#region JS_Calc_CoLoadMasterShipmentID

		public ZString JS_Calc_CoLoadMasterShipmentID
		{
			get
			{
				if (JS_JS_ColoadMasterShipment.IsValid)
				{
					return (Factory.Load<CommonShipment>(JS_JS_ColoadMasterShipment)).JS_UniqueConsignRef;
				}
				else
				{
					return "";
				}
			}
		}

		public ZPropertyInfo JS_Calc_CoLoadMasterShipmentIDInfo
		{
			get { return GetZPropertyInfo(nameof(JS_Calc_CoLoadMasterShipmentID)); }
		}

		#endregion

		#region JS_Calc_CoLoadMasterBillNo

		public ZString JS_Calc_CoLoadMasterBillNo
		{
			get { return JS_JS_ColoadMasterShipment.IsValid ? (Factory.Load<CommonShipment>(JS_JS_ColoadMasterShipment)).JS_HouseBill : ZString.Empty; }
		}

		public ZPropertyInfo JS_Calc_CoLoadMasterBillNoInfo
		{
			get { return GetZPropertyInfo(nameof(JS_Calc_CoLoadMasterBillNo)); }
		}

		#endregion

		#region JS_OuterPacks_ReadOnly

		public const string JS_OuterPacksReadOnlyName = "JS_OuterPacksReadOnly";

		public ZInt JS_OuterPacksReadOnly
		{
			get { return JS_OuterPacks; }
		}

		public ZPropertyInfo JS_OuterPacksReadOnlyInfo
		{
			get { return GetZPropertyInfo(JS_OuterPacksReadOnlyName); }
		}

		#endregion

		#region JS_InnerPacks_ReadOnly

		public const string JS_InnerPacks_ReadOnlyName = "JS_InnerPacks_ReadOnly";
		public ZInt JS_InnerPacks_ReadOnly
		{
			get { return JS_TotalPackageCount; }
		}

		public ZPropertyInfo JS_InnerPacks_ReadOnlyInfo
		{
			get { return GetZPropertyInfo(JS_InnerPacks_ReadOnlyName); }
		}

		#endregion

		#region Freight Spot Rates

		[DecimalPlaces(4)]
		public override ZDecimal JS_UnitFreightRate
		{
			get { return base.JS_UnitFreightRate; }
			set
			{
				if (base.JS_UnitFreightRate != value)
				{
					base.JS_UnitFreightRate = value;

					if (value != ZDecimal.Zero)
					{
						if (JS_RX_NKFrtRateCurrency.IsEmpty)
						{
							JS_RX_NKFrtRateCurrency = GetCurrencyForFreightRate();
						}

						if (JS_FreightSpotRateAutoratingMode.IsEmpty || JS_FreightSpotRateAutoratingMode == Constants.FreightRateAutoratingModes.Code.StandardRate)
						{
							JS_FreightSpotRateAutoratingMode = Constants.FreightRateAutoratingModes.Code.FreightPlusRate;
						}
					}

					JS_FreightSpotRateAutoratingModeInfo.RefreshBinding();
					JS_RX_NKFrtRateCurrencyInfo.RefreshBinding();
				}
			}
		}

		[List("Lookups.FreightRateAutoratingMode_List")]
		public override ZString JS_FreightSpotRateAutoratingMode
		{
			get { return base.JS_FreightSpotRateAutoratingMode; }
			set
			{
				if (base.JS_FreightSpotRateAutoratingMode != value)
				{
					base.JS_FreightSpotRateAutoratingMode = value;
					if (value == Constants.FreightRateAutoratingModes.Code.StandardRate)
					{
						JS_UnitFreightRate = ZDecimal.Zero;
					}

					JS_UnitFreightRateInfo.RefreshBinding();
					JS_RX_NKFrtRateCurrencyInfo.RefreshBinding();
				}
			}
		}

		[DecimalPlaces(4)]
		public override ZDecimal JS_FreightCostRate
		{
			get { return base.JS_FreightCostRate; }
			set
			{
				if (base.JS_FreightCostRate != value)
				{
					base.JS_FreightCostRate = value;

					if (value != ZDecimal.Zero)
					{
						if (JS_RX_NKFreightCostRateCurrency.IsEmpty)
						{
							JS_RX_NKFreightCostRateCurrency = GetCurrencyForFreightRate();
						}

						if (JS_FreightCostRateAutoratingMode.IsEmpty || JS_FreightCostRateAutoratingMode == Constants.FreightRateAutoratingModes.Code.StandardRate)
						{
							JS_FreightCostRateAutoratingMode = Constants.FreightRateAutoratingModes.Code.FreightPlusRate;
						}
					}

					JS_FreightCostRateAutoratingModeInfo.RefreshBinding();
					JS_RX_NKFreightCostRateCurrencyInfo.RefreshBinding();
				}
			}
		}

		[List("Lookups.FreightRateAutoratingMode_List")]
		public override ZString JS_FreightCostRateAutoratingMode
		{
			get { return base.JS_FreightCostRateAutoratingMode; }
			set
			{
				if (base.JS_FreightCostRateAutoratingMode != value)
				{
					base.JS_FreightCostRateAutoratingMode = value;
					if (value == Constants.FreightRateAutoratingModes.Code.StandardRate)
					{
						JS_FreightCostRate = ZDecimal.Zero;
					}

					JS_FreightCostRateInfo.RefreshBinding();
					JS_RX_NKFreightCostRateCurrencyInfo.RefreshBinding();
				}
			}
		}

		[DecimalPlaces(4)]
		public override ZDecimal JS_GatewayFreightSellRate
		{
			get { return base.JS_GatewayFreightSellRate; }
			set
			{
				if (base.JS_GatewayFreightSellRate != value)
				{
					base.JS_GatewayFreightSellRate = value;

					if (value != ZDecimal.Zero)
					{
						if (JS_RX_NKGatewayFreightSellRateCurrency.IsEmpty)
						{
							JS_RX_NKGatewayFreightSellRateCurrency = GetCurrencyForFreightRate();
						}

						if (JS_FreightGatewaySellRateAutoratingMode.IsEmpty || JS_FreightGatewaySellRateAutoratingMode == Constants.FreightRateAutoratingModes.Code.StandardRate)
						{
							JS_FreightGatewaySellRateAutoratingMode = Constants.FreightRateAutoratingModes.Code.FreightPlusRate;
						}
					}

					JS_FreightGatewaySellRateAutoratingModeInfo.RefreshBinding();
					JS_RX_NKGatewayFreightSellRateCurrencyInfo.RefreshBinding();
				}
			}
		}

		[List("Lookups.FreightRateAutoratingMode_List")]
		public override ZString JS_FreightGatewaySellRateAutoratingMode
		{
			get { return base.JS_FreightGatewaySellRateAutoratingMode; }
			set
			{
				if (base.JS_FreightGatewaySellRateAutoratingMode != value)
				{
					base.JS_FreightGatewaySellRateAutoratingMode = value;
					if (value == Constants.FreightRateAutoratingModes.Code.StandardRate)
					{
						JS_GatewayFreightSellRate = ZDecimal.Zero;
					}

					JS_GatewayFreightSellRateInfo.RefreshBinding();
					JS_RX_NKGatewayFreightSellRateCurrencyInfo.RefreshBinding();
				}
			}
		}

		ZString GetCurrencyForFreightRate()
		{
			ZString result;

			switch (JS_TransportMode)
			{
				case Constants.TransportModes.Sea:
					result = (JS_PackingMode == Constants.ContainerModes.FCL || JS_PackingMode == Constants.ContainerModes.LCL)
						? new ZString(Constants.CurrencyCodes.UnitedStates)
						: ZString.Empty;
					break;

				case Constants.TransportModes.Air:
					var originLocation = Origin as ILocation;
					result = GetCurrencyForFreightRate(originLocation);
					break;

				default:
					var destinationLocation = Destination as ILocation;
					result = GetCurrencyForFreightRate(destinationLocation);
					break;
			}

			return result;
		}

		ZString GetCurrencyForFreightRate(ILocation location)
		{
			return (location != null && location.Country != null)
						? location.Country.RN_RX_NKLocalCurrency
						: GlbCompany.CurrentCompany.LocalCurrency.RX_Code;
		}

		#endregion

		[EventDateProperty(AutoEvents.InterimReceiptProducedCode, EstimateActual.Actual)]
		public override ZDateTime JS_A_RCV
		{
			get { return new ZDateTime(base.JS_A_RCV, DateTimeKind.Unspecified); }
			set
			{
				base.JS_A_RCV = value;

				var parameters = new Dictionary<string, string>();
				parameters[EventConstants.EventReferenceParameters.Codes.Location] = JS_OA_ExportReceivingDepot_ZAddress.OrgAddress is IOrgAddress orgAddress ? orgAddress?.OA_RL_NKRelatedPortCode : null;
				parameters[EventConstants.EventReferenceParameters.Codes.Facility] = EventConstants.Facilities.Code.Depot;
				Logs.CreateRecreateOrUpdateEventLog(AutoEvents.InterimReceiptProduced, EstimateActual.Actual, value.ToOffset(), (IsCopying ? (NoResString)"This event has been copied from old shipment." : ZString.Empty), parameters.ToArray());
			}
		}

		#region JS_Calc_FreightExRate

		protected ZExchangeRate fJS_Calc_FreightExRate;
		public ZExchangeRate JS_Calc_FreightExRate
		{
			get
			{
				if (fJS_Calc_FreightExRate == null)
				{
					fJS_Calc_FreightExRate = new ZExchangeRate(this, ZArchitecture.Core.ExchangeRateType.Sell, JS_UnitFreightRateInfo, (ZPropertyInfoString)JS_RX_NKFrtRateCurrencyInfo);
				}
				return fJS_Calc_FreightExRate;
			}
		}

		#endregion

		#region Documented Values Converted to Standard Units

		#region JS_Calc_DocumentedWeight_Converted

		/// <summary>
		/// Documented Weight converted to standard system unit as setup in the registry
		/// </summary>
		public ZDecimal JS_Calc_DocumentedWeight_Converted
		{
			get
			{
				ZDecimal result;

				if (FreightUtilities.IsValidWeightUnit(JS_UnitOfWeight) && JS_UnitOfWeight != Env.Registry.FreightWeightUnit)
				{
					result = Constants.Weight.Convert(JS_DocumentedWeight, JS_UnitOfWeight, Env.Registry.FreightWeightUnit, false);
				}
				else
				{
					result = JS_DocumentedWeight;
				}

				return this.GetRoundedValue(JS_Calc_DocumentedWeight_ConvertedInfo, result);
			}
		}

		public const string JS_Calc_DocumentedWeight_ConvertedName = "JS_Calc_DocumentedWeight_Converted";
		public ZPropertyInfo JS_Calc_DocumentedWeight_ConvertedInfo
		{
			get { return GetZPropertyInfo(JS_Calc_DocumentedWeight_ConvertedName); }
		}

		#endregion

		#region JS_Calc_DocumentedVolume_Converted

		/// <summary>
		/// Documented Volume converted to standard system unit as setup in the registry
		/// </summary>
		public ZDecimal JS_Calc_DocumentedVolume_Converted
		{
			get
			{
				ZDecimal result;

				if (FreightUtilities.IsValidVolumeUnit(JS_UnitOfVolume) && JS_UnitOfVolume != Env.Registry.FreightVolumeUnit)
				{
					result = Constants.Volume.Convert(JS_DocumentedVolume, JS_UnitOfVolume, Env.Registry.FreightVolumeUnit, false);
				}
				else
				{
					result = JS_DocumentedVolume;
				}

				return this.GetRoundedValue(JS_Calc_DocumentedVolume_ConvertedInfo, result);
			}
		}

		public const string JS_Calc_DocumentedVolume_ConvertedName = "JS_Calc_DocumentedVolume_Converted";
		public ZPropertyInfo JS_Calc_DocumentedVolume_ConvertedInfo
		{
			get { return GetZPropertyInfo(JS_Calc_DocumentedVolume_ConvertedName); }
		}

		#endregion

		#endregion

		#region JS_Calc_NotifyPartyDisplay

		public ZString JS_Calc_NotifyPartyDisplay
		{
			get
			{
				ZString result = "";
				if (NotifyPartyDocumentaryAddress.IsValidAddress)
				{
					result = NotifyPartyDocumentaryAddress.E2_Contact + "\n";
					result += NotifyPartyDocumentaryAddress.E2_CompanyNameTruncated + "\n";
					result += NotifyPartyDocumentaryAddress.E2_Address1 + "\n";
					result += NotifyPartyDocumentaryAddress.E2_Address2;
				}

				return result;
			}
		}

		public ZPropertyInfo JS_Calc_NotifyPartyDisplayInfo
		{
			get { return GetZPropertyInfo(nameof(JS_Calc_NotifyPartyDisplay)); }
		}

		#endregion

		[ActionField(FieldType = ActionFieldType.Hidden)]
		[DocumentEngineIntegration.DocumentParsing.DocumentFieldExcludeFromMap]
		[WorkflowSetFieldReadonly]
		public override ZBool JS_IsForwardRegistered
		{
			get { return base.JS_IsForwardRegistered; }
			set
			{
				if (base.JS_IsForwardRegistered != value)
				{
					base.JS_IsForwardRegistered = value;
					MarkDocsAndCartageAsNeedsValidation();

					if (!Globals.IsTest && !JS_IsForwardRegistered && IsInDatabase)
					{
						if ((ZBool)JS_IsForwardRegisteredInfo.OriginalValue && !IsSuppressedCheckReset)
						{
							ErrorReporter.ReportOnce("CommonShipmentShouldNotResetJS_IsForwardRegistered", "Once a shipment has been marked as forward registered and saved it should not be unmarked");
						}
						else
						{
							var debugLog = new ZStringBuilder();
							debugLog.Append((NoResString)"A shipment has been marked as forward registered and then unmarked");
							debugLog.Append(FormattableString.Invariant($"{GetType().Name} {PK}, OriginalValue: {(ZBool)JS_IsForwardRegisteredInfo.OriginalValue}, IsSuppressedCheckReset: {IsSuppressedCheckReset}"));
							ErrorReporter.ReportOnce("CS01154849 - TRABORALB - Converting Quoted Booking into Shipments", debugLog.ToStringWithNewLineBetweenAppends());
						}
					}
				}
			}
		}

		#region JS_IsBooking

		[ActionField(FieldType = ActionFieldType.Hidden)]
		[DocumentEngineIntegration.DocumentParsing.DocumentFieldExcludeFromMap]
		[WorkflowSetFieldReadonly]
		public override ZBool JS_IsBooking
		{
			get { return base.JS_IsBooking; }
			set
			{
				if (base.JS_IsBooking != value)
				{
					base.JS_IsBooking = value;
					MarkDocsAndCartageAsNeedsValidation();

					if (!JS_IsBooking && IsInDatabase && (ZBool)JS_IsBookingInfo.OriginalValue && !IsSuppressedCheckReset)
					{
						ErrorReporter.ReportOnce("CommonShipmentShouldNotResetJS_IsBooking", "Once a shipment has been marked as booking and saved it should not be unmarked");
					}
				}
			}
		}

		#endregion

		#region JS_TH_OneTimeQuote

		[ActionField(FieldType = ActionFieldType.Hidden)]
		[DocumentEngineIntegration.DocumentParsing.DocumentFieldExcludeFromMap]
		[WorkflowSetFieldReadonly]
		public override ZGuid JS_TH_OneTimeQuote
		{
			get { return base.JS_TH_OneTimeQuote; }
			set
			{
				if (JS_TH_OneTimeQuote != value)
				{
					base.JS_TH_OneTimeQuote = value;

					if (!IsValidationSuspended)
					{
						Validation.ValidateJS_TH_OneTimeQuote();
					}
				}
			}
		}

		#endregion

		#region ACIZone - US/Canada

		#region JS_Calc_ACIConsignorOriginZone

		public ZString JS_Calc_ACIConsignorOriginZone
		{
			get
			{
				if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.UnitedStates ||
					GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Canada)
				{
					var address = (IDocAddress)ConsignorPickupAddress;
					var portCode = !address.E2_PortCode.IsEmpty ? address.E2_PortCode : JS_RL_NKOrigin;

					string cacheKey = string.Format("{0}-{1}-{2}", address.E2_Postcode, portCode, address.E2_City);

					if (cacheKey != aciConsignorOriginZoneCacheKey)
					{
						aciConsignorOriginZoneCacheKey = cacheKey;

						var zone = RefDomesticCartageZone.GetZone(Factory, address.E2_Postcode, portCode, address.E2_City);

						aciConsignorOriginZoneCacheValue = zone != null
							? zone.F1_Zone
							: ZString.Empty;
					}

					Lookups.RefUNLOCO_List.SetPostCodeDefault(false);
					return aciConsignorOriginZoneCacheValue;
				}

				Lookups.RefUNLOCO_List.SetPostCodeDefault(true);
				return ZString.Empty;
			}
		}

		ZString aciConsignorOriginZoneCacheKey;
		ZString aciConsignorOriginZoneCacheValue;

		public ZPropertyInfo JS_Calc_ACIConsignorOriginZoneInfo
		{
			get { return GetZPropertyInfo(nameof(JS_Calc_ACIConsignorOriginZone)); }
		}

		#endregion

		#region JS_Calc_ACIConsigneeDestinationZone

		public ZString JS_Calc_ACIConsigneeDestinationZone
		{
			get
			{
				if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.UnitedStates ||
					GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Canada)
				{
					var address = (IDocAddress)ConsigneeDeliveryAddress;
					var portCode = !address.E2_PortCode.IsEmpty ? address.E2_PortCode : JS_RL_NKDestination;

					string cacheKey = string.Format("{0}-{1}-{2}", address.E2_Postcode, portCode, address.E2_City);

					if (cacheKey != aciConsigneeDestinationZoneCacheKey)
					{
						aciConsigneeDestinationZoneCacheKey = cacheKey;

						var zone = RefDomesticCartageZone.GetZone(Factory, address.E2_Postcode, portCode, address.E2_City);

						aciConsigneeDestinationZoneCacheValue = zone != null
							? zone.F1_Zone
							: ZString.Empty;
					}

					Lookups.RefUNLOCO_List.SetPostCodeDefault(false);
					return aciConsigneeDestinationZoneCacheValue;
				}

				Lookups.RefUNLOCO_List.SetPostCodeDefault(true);
				return ZString.Empty;
			}
		}

		ZString aciConsigneeDestinationZoneCacheKey;
		ZString aciConsigneeDestinationZoneCacheValue;

		public ZPropertyInfo JS_Calc_ACIConsigneeDestinationZoneInfo
		{
			get { return GetZPropertyInfo(nameof(JS_Calc_ACIConsigneeDestinationZone)); }
		}

		#endregion

		#endregion

		#region CartageZone

		public ZString JS_Calc_PickupCartageZone
		{
			get
			{
				IOrgHeader transportProvider = DocsAndCartage.PickupCartageCo;
				ILocation location = LocationHelper.GetLocationFromString(JS_RL_NKOrigin, Factory);
				string postCode = ConsignorPickupAddress.E2_Postcode;
				string citySuburb = ConsignorPickupAddress.E2_City;
				string countryCode = ConsignorPickupAddress.E2_RN_NKCountryCode;

				string cacheKey = string.Format("{0}-{1}-{2}-{3}-{4}",
					transportProvider != null ? transportProvider.Code.ToString() : string.Empty,
					location != null ? location.Code.ToString() : string.Empty,
					countryCode,
					postCode,
					citySuburb);

				if (cacheKey != pickupCartageZoneCacheKey)
				{
					pickupCartageZoneCacheKey = cacheKey;
					pickupCartageZoneCacheValue = RateTransportZoneHelper.GetZoneName(Factory, transportProvider, location, countryCode, postCode, citySuburb);
				}

				return pickupCartageZoneCacheValue;
			}
		}

		ZString pickupCartageZoneCacheKey;
		ZString pickupCartageZoneCacheValue;

		public ZPropertyInfo JS_Calc_PickupCartageZoneInfo
		{
			get { return GetZPropertyInfo(nameof(JS_Calc_PickupCartageZone)); }
		}

		public ZString JS_Calc_DeliveryCartageZone
		{
			get
			{
				IOrgHeader transportProvider = DocsAndCartage.DeliveryCartageCo;
				ILocation location = LocationHelper.GetLocationFromString(JS_RL_NKDestination, Factory);
				string postCode = ConsigneeDeliveryAddress.E2_Postcode;
				string citySuburb = ConsigneeDeliveryAddress.E2_City;
				string countryCode = ConsigneeDeliveryAddress.E2_RN_NKCountryCode;

				string cacheKey = string.Format("{0}-{1}-{2}-{3}-{4}",
					transportProvider != null ? transportProvider.Code.ToString() : string.Empty,
					location != null ? location.Code.ToString() : string.Empty,
					countryCode,
					postCode,
					citySuburb);

				if (cacheKey != deliveryCartageZoneCacheKey)
				{
					deliveryCartageZoneCacheKey = cacheKey;
					deliveryCartageZoneCacheValue = RateTransportZoneHelper.GetZoneName(Factory, transportProvider, location, countryCode, postCode, citySuburb);
				}

				return deliveryCartageZoneCacheValue;
			}
		}

		ZString deliveryCartageZoneCacheKey;
		ZString deliveryCartageZoneCacheValue;

		public ZPropertyInfo JS_Calc_DeliveryCartageZoneInfo
		{
			get { return GetZPropertyInfo(nameof(JS_Calc_DeliveryCartageZone)); }
		}

		internal IRateTransportZoneHelper RateTransportZoneHelper
		{
			get { return fRateTransportZoneHelper ?? (fRateTransportZoneHelper = ObjectFactory.Get<IRateTransportZoneHelper>()); }
		}
		IRateTransportZoneHelper fRateTransportZoneHelper;

		#endregion

		#region IsStandAloneShipmentFromBooking

		public bool IsStandAloneShipmentFromBooking
		{
			get
			{
				return JS_IsBooking && JS_IsForwardRegistered && !Consols.Any() && !Logs.Find(x => x.SL_SE_NKEvent == Events.Detached.Code).Any();
			}
		}

		#endregion

		protected bool fWasAttachedToConsolOnRetrieve;
		public bool HasBeenUnallocatedSinceRetrieve
		{
			get { return fWasAttachedToConsolOnRetrieve && Consols.Count == 0; }
		}

		public override Notes Notes
		{
			get { return notes ?? (notes = new CommonShipmentNotes(this)); }
		}
		Notes notes;

		public bool SuppressShipmentNumberValidation { get; set; }

		[List("Lookups.EFreightStatus_List")]
		public override ZString JS_EFreightStatus
		{
			get { return base.JS_EFreightStatus; }
			set { base.JS_EFreightStatus = value; }
		}

		[List("Lookups.RefUNLOCO_List")]
		public override ZString JS_RL_NKPlaceOfReceipt
		{
			get { return base.JS_RL_NKPlaceOfReceipt; }
			set { base.JS_RL_NKPlaceOfReceipt = value; }
		}

		[List("Lookups.RefUNLOCO_List")]
		public override ZString JS_RL_NKPlaceOfDischarge
		{
			get { return base.JS_RL_NKPlaceOfDischarge; }
			set { base.JS_RL_NKPlaceOfDischarge = value; }
		}

		#region JS_Calc_ActualVolumeWeight

		[ReadOnly(true)]
		[BusinessObjectTestExclude]
		public ZDecimal JS_Calc_ActualVolumeWeight
		{
			get
			{
				var unit = JS_Calc_ActualVolumeWeightUnit;
				if (unit.IsEmpty)
				{
					return 0m;
				}

				var conversionFactor = GetShipmentConversionFactor();
				var quantityToConvert = IsShipmentChargeableByWeight
					? new ZVolume(JS_ActualVolume, JS_UnitOfVolume)
					: (IQuantity)new ZWeight(JS_ActualWeight, JS_UnitOfWeight);

				if (conversionFactor.IsEmpty || !quantityToConvert.IsValid)
				{
					return 0m;
				}

				return quantityToConvert.Convert(unit, new[] { conversionFactor }, true)?.Amount ?? 0m;
			}
		}

		public ZPropertyInfo JS_Calc_ActualVolumeWeightInfo => GetZPropertyInfo(nameof(JS_Calc_ActualVolumeWeight));

		[ReadOnly(true)]
		[List("Lookups.JS_Calc_ActualVolumeWeightUnit_List")]
		public ZString JS_Calc_ActualVolumeWeightUnit
		{
			get
			{
				var unit = IsShipmentChargeableByWeight && Constants.Weight.ContainsCode(JS_UnitOfWeight)
					? JS_UnitOfWeight
					: Constants.Volume.ContainsCode(JS_UnitOfVolume)
						? JS_UnitOfVolume
						: ZString.Empty;

				return unit;
			}
		}

		public ZPropertyInfo JS_Calc_ActualVolumeWeightUnitInfo => GetZPropertyInfo(nameof(JS_Calc_ActualVolumeWeightUnit));

		void RefreshCalculatedVolumeWeightValues()
		{
			JS_Calc_ActualVolumeWeightInfo.RefreshBinding();
			JS_Calc_ActualVolumeWeightUnitInfo.RefreshBinding();
		}

		ConversionFactor GetShipmentConversionFactor()
		{
			var chargeableFactor = ChargeableFactor.GetDefault(IsDomesticFreightForChargeableWeightCalculations ? ChargeableFactorSource.Domestic : ChargeableFactorSource.International, JS_TransportMode);
			if (chargeableFactor == null)
			{
				return ConversionFactor.Empty;
			}

			var isImperial = IsShipmentChargeableByWeight
				? Constants.Weight.IsImperial(JS_ChargeableUnit)
				: Constants.Volume.IsImperial(JS_ChargeableUnit);

			return isImperial ? chargeableFactor.ImperialFactor : chargeableFactor.MetricFactor;
		}

		public bool IsShipmentChargeableByWeight
		{
			get { return Constants.Weight.ContainsCode(JS_ChargeableUnit); }
		}

		#endregion

		#region JS_Calc_ExcessActualVolumeWeight

		[BusinessObjectTestExclude]
		public ZDecimal JS_Calc_ExcessActualVolumeWeight
		{
			get
			{
				var result = 0m;

				if (IsShipmentChargeableByWeight)
				{
					result = JS_ActualWeight - JS_Calc_ActualVolumeWeight;
				}
				else
				{
					result = JS_ActualVolume - JS_Calc_ActualVolumeWeight;
				}

				return result <= 0 ? 0m : result;
			}
		}

		[BusinessObjectTestExclude]
		public ZString JS_Calc_ExcessActualVolumeWeightUnit
		{
			get
			{
				return JS_Calc_ActualVolumeWeightUnit;
			}
		}

		#endregion

		#region JS_Calc_ExcessChargeableVolumeWeight

		[BusinessObjectTestExclude]
		public ZDecimal JS_Calc_ExcessChargeableVolumeWeight
		{
			get
			{
				var result = 0m;

				if (IsShipmentChargeableByWeight)
				{
					result = JS_Calc_ActualVolumeWeight - JS_ActualWeight;
				}
				else
				{
					result = JS_Calc_ActualVolumeWeight - JS_ActualVolume;
				}

				return result <= 0 ? 0m : result;
			}
		}

		#endregion

		#region IsHazardous

		public ZBool IsHazardous => OuterPackLines.Cast<PackLine>().Any(x => x.JL_RH_NKCommodityCode == "HAZ" || x.CommodityCode != null && x.CommodityCode.RH_IsHazardous || x.UNDGs.Any(y => !y.DI_IMOClass.IsEmpty));

		public ZPropertyInfo IsHazardousInfo => GetZPropertyInfo(nameof(IsHazardous));

		#endregion

		#region Advance Cargo Reporting Self-Filer

		public ZBool IsAdvanceCargoReportingSelfFiler
		{
			get
			{
				return (Consignee?.MiscServ is OrgMiscServ consigneeMiscServ && consigneeMiscServ.OM_IMAdvanceCargoReportingSelfFiler);
			}
		}

		#endregion

		public ZGuid OriginalLoginCompanyPK { get; }

		#endregion

		#region Lookups

		public CodeDescriptionPairList PreCarriageOnCarriageTransportMode_List
		{
			get { return Factory.GetCachedValue<JobAddressAdditionalInfoTransportModeCodeDescriptionPairList>(); }
		}

		public virtual CodeDescriptionPairList ChargesApplyLookup => JS_TransportMode == Constants.TransportModes.Air ? ChargesApplyHelper.ChargesApplyPairList : Lookups.JS_HBLAWBChargesDisplay_List;

		#region GetNewLookups

		public new BaseJobShipmentLookups Lookups
		{
			get { return lookups ?? (lookups = (BaseJobShipmentLookups)GetNewLookups()); }
		}
		BaseJobShipmentLookups lookups;

		protected override JobShipmentLookups GetNewLookups()
		{
			return new BaseJobShipmentLookups(this);
		}

		#endregion

		public ZString PaymentTermLabel
		{
			get { return IsDomesticFreight ? Res.GetString("BaseShipment|PaymentTerm|PaytTerm", "Payment Term") : Res.GetString("BaseShipment|PaymentTerm|Incoterm", "Incoterm"); }
		}

		public ZPropertyInfo PaymentTermLabelInfo
		{
			get { return GetZPropertyInfo(nameof(PaymentTermLabel)); }
		}

		public BindToLists BindToLists
		{
			get { return BindToLists.GetCachedLists(Factory); }
		}

		#endregion

		#region ILocationConsumer Member

		ZGuid ILocationConsumer.LocationPK
		{
			get { return JS_WL; }
		}

		ZGuid ILocationConsumer.WarehousePK
		{
			get { return LocationWhsGuid; }
		}

		ZString ILocationConsumer.LocationTypeForMessages
		{
			get { return ZString.Empty; }
		}

		ZString ILocationConsumer.LocationTitle { get; set; }

		#endregion

		#region SupplyChainSecurityConfiguration

		internal ISupplyChainSecurityConfiguration SupplyChainSecurityConfiguration
		{
			get { return supplyChainSecurityConfiguration ?? (supplyChainSecurityConfiguration = ObjectFactory.Get<ISupplyChainSecurityConfigurationHelper>().GetConfiguration()); }
		}
		ISupplyChainSecurityConfiguration supplyChainSecurityConfiguration;

		#endregion

		#region ITemplateCopyable Members

		public virtual IBusiness TemplateCopy()
		{
			return TemplateCopy(null);
		}

		public virtual IBusiness TemplateCopy(BusinessObjectFactory alternativeFactory = null)
		{
			var columnNamesToExcludeFromCopy = new List<string>
			{
				JobShipmentSchema.Constants.JS_TH_OneTimeQuote,
				JobShipmentSchema.Constants.JS_JS_ColoadMasterShipment,
				JobShipmentSchema.Constants.JS_A_RCV
			};

			CommonShipment result;
			if (alternativeFactory == null)
			{
				result = (CommonShipment)Clone(new BusinessObjectCloneArgs(columnNamesToExcludeFromCopy.ToArray()));
			}
			else
			{
				result = (CommonShipment)Clone(new BusinessObjectCloneArgs(alternativeFactory, columnNamesToExcludeFromCopy.ToArray(), null, true));
			}

			using (result.GetValidationSuspender())
			{
				ClearAllDateFields(result);
				ClearAllDateFields(result.DocsAndCartage);

				result.JS_A_BKD = ZDateTime.Today;
				result.JS_UniqueConsignRef = ZString.Empty;
				result.JS_HouseBill = ZString.Empty;
			}

			return result;
		}

		public IEnumerable<CommonShipment> TemplateCopy(ZGuid coloadMasterPK)
		{
			CommonShipment newMaster = null;
			newMaster = (CommonShipment)TemplateCopy();

			using (newMaster.GetValidationSuspender())
			{
				newMaster.JS_JS_ColoadMasterShipment = coloadMasterPK;
			}

			yield return newMaster;

			foreach (CommonShipment originalChild in CoLoadShipments.ToList())
			{
				foreach (CommonShipment clonedChildOrSubChild in originalChild.TemplateCopy(newMaster.PK))
				{
					yield return clonedChildOrSubChild;
				}
			}
		}

		void ClearAllDateFields(BusinessObject businessObject)
		{
			foreach (ZPropertyInfo propertyInfo in businessObject.ZPropertyInfoHash)
			{
				if (propertyInfo.PropertyType == typeof(ZDateTime) && propertyInfo.HasSetter)
				{
					propertyInfo.Value = ZDateTime.Empty;
				}
			}
		}

		#endregion

		#region IDocAddresses Members

		#region DocAddresses

		[ChildEditable(true)]
		[UniversalCopySplitCollection("Consignor address", JobDocAddressSchema.Constants.E2_AddressType + " IN ('" + AutoDocAddressTypes.Codes.ConsignorDocumentaryAddress + "', '" + AutoDocAddressTypes.Codes.ConsignorPickupDeliveryAddress + "')")]
		[UniversalCopySplitCollection("Consignee address", JobDocAddressSchema.Constants.E2_AddressType + " IN ('" + AutoDocAddressTypes.Codes.ConsigneeDocumentaryAddress + "', '" + AutoDocAddressTypes.Codes.ConsigneePickupDeliveryAddress + "')")]
		public virtual JobDocAddressDependentCollection DocAddresses
		{
			get
			{
				if (fDocAddresses == null)
				{
					fDocAddresses = new JobDocAddressDependentCollection(this);
					fDocAddresses.Load();
					SetDocAddressDefaults();
					RegisterEditableChildObject(fDocAddresses);
				}

				return fDocAddresses;
			}
		}

		JobDocAddressDependentCollection fDocAddresses;

		void SetDocAddressDefaults()
		{
			foreach (JobDocAddress docAddress in fDocAddresses.ToList())
			{
				SetDocumentAddressPhaseSecurity(docAddress);
				SetDocAddressTypeSpecificDefaults(docAddress);
			}
		}

		protected virtual void SetDocAddressTypeSpecificDefaults(JobDocAddress docAddress)
		{
			switch (docAddress.DocAddressType)
			{
				case DocAddressType.NotifyParty:
					docAddress.ReadOnly = IsNotifyPartyDocAddressReadOnly;
					break;
				case DocAddressType.ControllingCustomer:
					SetControllingCustomerAddressDefaults(docAddress);
					break;
			}
		}

		void SetControllingCustomerAddressDefaults(JobDocAddress docAddress)
		{
			docAddress.HasChangesChanged += new EventHandler<HasChangesChangedEventArgs>(ControllingCustomerAddressChanged);
		}

		void SetDocumentAddressPhaseSecurity(JobDocAddress docAddress)
		{
			string property;
			if (DocAddressTypesAndProperties.TryGetValue(docAddress.DocAddressType, out property))
			{
				if (IsPropertyReadOnlyDueToPhase(property))
				{
					docAddress.SetReadOnlyIncludingChildren(true);
				}
			}
		}

		ImmutableDictionary<DocAddressType, string> DocAddressTypesAndProperties
		{
			get
			{
				return fDocAddressTypeAndProperties ?? (fDocAddressTypeAndProperties = GetNewDocAddressTypesAndProperties().ToImmutableDictionary());
			}
		}

		ImmutableDictionary<DocAddressType, string> fDocAddressTypeAndProperties;

		protected virtual Dictionary<DocAddressType, string> GetNewDocAddressTypesAndProperties()
		{
			return new Dictionary<DocAddressType, string>()
			{
				{ DocAddressType.ControllingCustomer, Schema.ControllingCustomerNameOrPK }
			};
		}

		protected bool IsDocAddressesLoaded
		{
			get { return fDocAddresses != null; }
		}

		#endregion

		#region DocAddressManager

		public JobDocAddressManager DocAddressManager
		{
			get
			{
				if (fDocAddressManager == null)
				{
					fDocAddressManager = new JobDocAddressManager();
					fDocAddressManager.AddRequirement(ConsigneeDocAddressRequirement);
					fDocAddressManager.AddRequirement(ConsignorDocAddressRequirement);
					fDocAddressManager.AddRequirement(ConsigneePickupDeliveryAddressRequirement);
					fDocAddressManager.AddRequirement(ConsignorPickupDeliveryAddressRequirement);
					fDocAddressManager.AddRequirement(NotifyPartyDocAddressRequirement);
				}
				return fDocAddressManager;
			}
		}

		JobDocAddressManager fDocAddressManager;

		#endregion

		#region DocAddress Requirements

		void ValidateAddressIsRealForHVLV(JobDocAddressValidation addressValidation)
		{
			bool AddressTypeRequiresValidation(JobDocAddress docAddress)
			{
				var ignoredTypes = new[] { DocAddressType.ConsignorPickupDeliveryAddress };
				return !ignoredTypes.Contains(docAddress.DocAddressType);
			}

			if (IsHighVolumeLowValue)
			{
				var docAddress = addressValidation.Parent;
				if (!docAddress.HasRealAddress && AddressTypeRequiresValidation(docAddress))
				{
					docAddress.E2_OA_AddressInfo.AddError(Res.GetString("E50DEF71-7D19-4D2B-9166-A75270746E73", "A real Organization & Address is required for HVLV data."));
				}
			}
		}

		#region ConsigneeDocAddressRequirement

		public JobDocAddressRequirement ConsigneeDocAddressRequirement
		{
			get
			{
				if (fConsigneeDocAddressRequirement == null)
				{
					fConsigneeDocAddressRequirement = GetConsigneeDocAddressRequirement();
				}

				return fConsigneeDocAddressRequirement;
			}
		}
		JobDocAddressRequirement fConsigneeDocAddressRequirement;

		protected virtual JobDocAddressRequirement GetConsigneeDocAddressRequirement()
		{
			var requirement = new JobDocAddressRequirement(DocAddressType.ConsigneeDocumentaryAddress, AddressType.OFC, ContactType.Consignee);
			AddConsigneeLinkedRequirement(requirement);
			requirement.ValidateOrganisationPK = Validation.ValidateConsigneePK;
			return requirement;
		}

		void AddConsigneeLinkedRequirement(JobDocAddressRequirement requirement)
		{
			if (!Globals.IsWeb)
			{
				requirement.AddLinkedRequirement(ConsigneePickupDeliveryAddressRequirement);
			}
		}

		#endregion

		#region ConsigneePickupDeliveryAddressRequirement

		public JobDocAddressRequirement ConsigneePickupDeliveryAddressRequirement
		{
			get
			{
				if (fConsigneePickupDeliveryAddressRequirement == null)
				{
					fConsigneePickupDeliveryAddressRequirement = GetConsigneePickupDeliveryAddressRequirement();
				}
				return fConsigneePickupDeliveryAddressRequirement;
			}
		}
		JobDocAddressRequirement fConsigneePickupDeliveryAddressRequirement;

		JobDocAddressRequirement GetConsigneePickupDeliveryAddressRequirement()
		{
			var requirement = new JobDocAddressRequirement(DocAddressType.ConsigneePickupDeliveryAddress, AddressType.DLV, ContactType.LocalTransport);
			AddConsigneeDeliveryAddressLinkedRequirement(requirement);
			return requirement;
		}

		void AddConsigneeDeliveryAddressLinkedRequirement(JobDocAddressRequirement requirement)
		{
			if (Globals.IsWeb)
			{
				requirement.AddLinkedRequirement(ConsigneeDocAddressRequirement);
			}
		}

		#endregion

		#region ConsignorDocAddressRequirement

		public JobDocAddressRequirement ConsignorDocAddressRequirement
		{
			get
			{
				if (fConsignorDocAddressRequirement == null)
				{
					fConsignorDocAddressRequirement = GetConsignorDocAddressRequirement();
				}
				return fConsignorDocAddressRequirement;
			}
		}
		JobDocAddressRequirement fConsignorDocAddressRequirement;

		protected virtual JobDocAddressRequirement GetConsignorDocAddressRequirement()
		{
			var requirement = new JobDocAddressRequirement(DocAddressType.ConsignorDocumentaryAddress, AddressType.OFC, ContactType.Consignor);
			AddConsignorLinkedRequirement(requirement);
			requirement.ValidateOrganisationPK = Validation.ValidateConsignorPK;
			return requirement;
		}

		void AddConsignorLinkedRequirement(JobDocAddressRequirement requirement)
		{
			if (!Globals.IsWeb)
			{
				requirement.AddLinkedRequirement(ConsignorPickupDeliveryAddressRequirement);
			}

			requirement.ValidateDelegateToBeFiredUponValidationOf_E2_OA_Address += ValidateAddressIsRealForHVLV;
		}

		#endregion

		#region ConsignorPickupDeliveryAddressRequirement

		public JobDocAddressRequirement ConsignorPickupDeliveryAddressRequirement
		{
			get
			{
				if (fConsignorPickupDeliveryAddressRequirement == null)
				{
					fConsignorPickupDeliveryAddressRequirement = GetConsignorPickupDeliveryAddressRequirement();
				}
				return fConsignorPickupDeliveryAddressRequirement;
			}
		}
		JobDocAddressRequirement fConsignorPickupDeliveryAddressRequirement;

		JobDocAddressRequirement GetConsignorPickupDeliveryAddressRequirement()
		{
			var requirement = new JobDocAddressRequirement(DocAddressType.ConsignorPickupDeliveryAddress, AddressType.PIC, ContactType.LocalTransport);
			AddConsignorPickupAddressLinkedRequirement(requirement);
			return requirement;
		}

		void AddConsignorPickupAddressLinkedRequirement(JobDocAddressRequirement requirement)
		{
			if (Globals.IsWeb)
			{
				requirement.AddLinkedRequirement(ConsignorDocAddressRequirement);
			}

			requirement.ValidateDelegateToBeFiredUponValidationOf_E2_OA_Address += ValidateAddressIsRealForHVLV;
		}

		#endregion

		#region NotifyOrganisationDocAddressRequirement

		internal JobDocAddressRequirement NotifyPartyDocAddressRequirement
		{
			get
			{
				if (fNotifyPartyDocAddressRequirement == null)
				{
					fNotifyPartyDocAddressRequirement = GetNotifyPartyDocAddressRequirement();
				}

				return fNotifyPartyDocAddressRequirement;
			}
		}

		protected virtual JobDocAddressRequirement GetNotifyPartyDocAddressRequirement()
		{
			return new JobDocAddressRequirement(DocAddressType.NotifyParty, ContactType.NotifyParty);
		}

		JobDocAddressRequirement fNotifyPartyDocAddressRequirement;

		#endregion

		#region ControllingOrganisationDocAddressRequirement

		protected JobDocAddressRequirement ControllingCustomerDocAddressRequirement
		{
			get
			{
				if (controllingCustomerDocAddressRequirement == null)
				{
					controllingCustomerDocAddressRequirement = new JobDocAddressRequirement(DocAddressType.ControllingCustomer)
					{
						ValidateOrganisationPK = Validation.ValidateControllingCustomerPK,
						ValidateDelegateToBeFiredUponValidationOf_E2_OA_Address = validation => validation.ValidateOrganisationPK()
					};
				}

				return controllingCustomerDocAddressRequirement;
			}
		}
		JobDocAddressRequirement controllingCustomerDocAddressRequirement;

		protected JobDocAddressRequirement ControllingAgentDocAddressRequirement
		{
			get
			{
				if (controllingAgentDocAddressRequirement == null)
				{
					controllingAgentDocAddressRequirement = new JobDocAddressRequirement(DocAddressType.ControllingAgent)
					{
						ValidateOrganisationPK = Validation.ValidateControllingAgentPK,
						ValidateDelegateToBeFiredUponValidationOf_E2_OA_Address = validation => validation.ValidateOrganisationPK()
					};
				}

				return controllingAgentDocAddressRequirement;
			}
		}
		JobDocAddressRequirement controllingAgentDocAddressRequirement;
		#endregion

		#endregion

		#region SupportedAddressTypes

		IReadOnlyList<DocAddressType> IDocAddresses.SupportedAddressTypes
		{
			get { return SupportedAddressTypes; }
		}

		protected virtual DocAddressType[] SupportedAddressTypes
		{
			get
			{
				return new DocAddressType[]
				{
					DocAddressType.ConsignorDocumentaryAddress,
					DocAddressType.ConsignorPickupDeliveryAddress,
					DocAddressType.ConsigneeDocumentaryAddress,
					DocAddressType.ConsigneePickupDeliveryAddress,
					DocAddressType.NotifyParty,
					DocAddressType.NotifyParty2,
					DocAddressType.NotifyParty3,
					DocAddressType.BuyerDocumentaryAddress,
					DocAddressType.InsuredByDocumentaryAddress,
					DocAddressType.AssuredPartyDocumentaryAddress,
					DocAddressType.ClaimsPayableByDocumentaryAddress,
					DocAddressType.SurveyReportPartyDocumentaryAddress,
					DocAddressType.ControllingCustomer,
					DocAddressType.PickupAgent,
					DocAddressType.Manufacturer
				};
			}
		}

		#endregion

		JobDocAddressRequirement IDocAddresses.GetDocAddressRequirement(DocAddressType addressType)
		{
			return GetDocAddressRequirement(addressType);
		}

		protected virtual JobDocAddressRequirement GetDocAddressRequirement(DocAddressType addressType)
		{
			switch (addressType)
			{
				case DocAddressType.ConsignorDocumentaryAddress:
					return ConsignorDocAddressRequirement;
				case DocAddressType.ConsignorPickupDeliveryAddress:
					return ConsignorPickupDeliveryAddressRequirement;
				case DocAddressType.ConsigneeDocumentaryAddress:
					return ConsigneeDocAddressRequirement;
				case DocAddressType.ConsigneePickupDeliveryAddress:
					return ConsigneePickupDeliveryAddressRequirement;
				case DocAddressType.BuyerDocumentaryAddress:
					return BuyerDocAddressRequirement;
				case DocAddressType.InsuredByDocumentaryAddress:
					return InsuredByDocAddressRequirement;
				case DocAddressType.AssuredPartyDocumentaryAddress:
					return AssuredPartyDocAddressRequirement;
				case DocAddressType.ClaimsPayableByDocumentaryAddress:
					return ClaimsPayableByDocAddressRequirement;
				case DocAddressType.SurveyReportPartyDocumentaryAddress:
					return SurveyReportPartyDocAddressRequirement;
				case DocAddressType.NotifyParty:
					return NotifyPartyDocAddressRequirement;
				case DocAddressType.NotifyParty2:
					return NotifyParty2DocAddressRequirement;
				case DocAddressType.NotifyParty3:
					return NotifyParty3DocAddressRequirement;
				case DocAddressType.ControllingCustomer:
					return ControllingCustomerDocAddressRequirement;
				case DocAddressType.PickupAgent:
					return PickupAgentDocAddressRequirement;
				default:
					return null;
			}
		}

		void IDocAddresses.DocAddressChanged(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.AnyAddressFieldBeforeChange(JobDocAddress docAddress)
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

		void IDocAddresses.OrgHeaderAfterChange(JobDocAddress docAddress)
		{
		}

		SecurityCheckpoint IDocAddresses.GetCanOverrideCheckpoint(JobDocAddress docAddress)
		{
			return GetCanOverrideAddressCheckpoint(docAddress);
		}

		bool IDocAddresses.CanDeleteAddress(JobDocAddress docAddress)
		{
			return true;
		}

		#region GetOrgHeaderList

		OrgHeaderCollection IDocAddresses.GetOrgHeaderList(DocAddressType addressType)
		{
			return GetOrgHeaderList(addressType);
		}

		protected virtual OrgHeaderCollection GetOrgHeaderList(DocAddressType addressType)
		{
			OrganisationsFindBoxCollection result = null;
			switch (addressType)
			{
				case DocAddressType.ConsignorDocumentaryAddress:
					result = GetOrganisationsFindBoxCollectionWithOriginalProviderType(nameof(Lookups.ConsignorForwarder_List));
					break;
				case DocAddressType.ConsignorPickupDeliveryAddress:
					result = GetOrganisationsFindBoxCollectionWithOriginalProviderType(nameof(Lookups.ConsignorForwarderPickup_List));
					break;
				case DocAddressType.ConsigneeDocumentaryAddress:
					result = GetOrganisationsFindBoxCollectionWithOriginalProviderType(nameof(Lookups.ConsigneeForwarder_List));
					break;
				case DocAddressType.ConsigneePickupDeliveryAddress:
					result = GetOrganisationsFindBoxCollectionWithOriginalProviderType(nameof(Lookups.ConsigneeForwarderDelivery_List));
					break;
				case DocAddressType.NotifyParty:
					result = GetOrganisationsFindBoxCollectionWithOriginalProviderType(nameof(Lookups.NotifyParty_List));
					break;
				case DocAddressType.PickupAgent:
					result = GetOrganisationsFindBoxCollectionWithOriginalProviderType(nameof(Lookups.Forwarder_List));
					break;
			}

			return result;
		}

		protected OrganisationsFindBoxCollection GetOrganisationsFindBoxCollectionWithOriginalProviderType(string baseJobShipmentLookupsPropertyName)
		{
			if (string.IsNullOrEmpty(baseJobShipmentLookupsPropertyName))
			{
				throw new ArgumentNullException("Property name");
			}

			OrganisationsFindBoxCollection result = null;
			var parentType = Lookups.GetType();
			var propertyInfo = parentType.GetProperty(baseJobShipmentLookupsPropertyName);
			OrganisationDefaultProviderAttribute[] attributes = null;
			if (propertyInfo != null)
			{
				attributes = propertyInfo.GetCustomAttributes<OrganisationDefaultProviderAttribute>(false).ToArray();
				if (attributes.Length > 1)
				{
					throw new InvalidOperationException(string.Format("There should only ever be one OrganisationDefaultProviderAttribute applied in this collection. Error on Type: {0}", parentType.FullName));
				}

				if (attributes.Length == 1)
				{
					var originalProviderType = attributes[0];
					result = new OrganisationsFindBoxCollection(Factory)
					{
						OrganisationType = originalProviderType.OrganisationType,
						OrganisationSubType = originalProviderType.OrganisationSubType,
						DocAddressType = originalProviderType.DocAddressType
					};
				}
				else
				{
					result = new OrganisationsFindBoxCollection(Factory);
				}
			}
			else
			{
				throw new InvalidOperationException(string.Format("This property does not exist in this collection. Error on Property name: {0}", baseJobShipmentLookupsPropertyName));
			}

			return result;
		}

		#endregion

		#endregion

		#region IDocsAndCartageParent Members

		public virtual Type DocsAndCartageType
		{
			get { return typeof(JobDocsAndCartage); }
		}

		public virtual Type DocsAndCartageParentType
		{
			get { return this.GetType(); }
		}

		IHaveRequiredDocuments IDocsAndCartageParent.RequiredDocumentsProvider
		{
			get { return DocsAndCartage; }
		}

		[UniversalCopyRelatedEntity(CommaSeparatedSkipPropertiesNames = "JP_ParentID,JP_ParentTableCode", DisableCopyMethodLink = true)]
		public JobDocsAndCartage DocsAndCartage
		{
			get
			{
				JobDocsAndCartage result = fDocsAndCartage;
				if (fDocsAndCartage == null || fDocsAndCartage.IsDeleted)
				{
					fDocsAndCartage = JobDocsAndCartage.GetOrCreateDocsAndCartageFromParent(this);
					if (DocsAndCartageFirstSet != null)
					{
						DocsAndCartageFirstSet(this, EventArgs.Empty);
					}
					if (fDocsAndCartage != null)
					{
						RegisterEditableChildObject(fDocsAndCartage, "DocsAndCartage");
					}

					result = fDocsAndCartage;
				}

				return result;
			}
		}

		JobDocsAndCartage fDocsAndCartage;

		public event EventHandler DocsAndCartageFirstSet;

		public bool IsDocsAndCartageSet
		{
			get { return (fDocsAndCartage != null && !fDocsAndCartage.IsDeleted); }
		}

		#region IHaveInternalCartage

		bool IHaveInternalCartage.IsForPickupCartage
		{
			get { return IsForPickupCartage; }
		}

		protected virtual bool IsForPickupCartage
		{
			get { return true; }
		}

		ZGuid IHaveInternalCartage.BranchPK
		{
			get { return DefaultBranchPKForLocalCartage; }
		}

		protected virtual ZGuid DefaultBranchPKForLocalCartage
		{
			get { return ZGuid.Empty; }
		}

		ZString IHaveInternalCartage.ServiceLevel
		{
			get { return JS_RS_NKServiceLevel; }
		}

		ZString IHaveInternalCartage.OwnerRef
		{
			get { return JS_OrderReferences; }
		}

		bool IHaveInternalCartage.IsAllowedToUpdateAdviseDates
		{
			get { return true; }
		}

		OrgHeader IHaveInternalCartage.PickupCartageOrg
		{
			get
			{
				OrgHeader result = null;
				if (DocsAndCartage != null)
				{
					result = DocsAndCartage.PickupCartageCo;
				}
				return result;
			}
		}

		OrgHeader IHaveInternalCartage.DeliveryCartageOrg
		{
			get
			{
				OrgHeader result = null;
				if (DocsAndCartage != null)
				{
					result = DocsAndCartage.DeliveryCartageCo;
				}
				return result;
			}
		}

		IContainer[] IHaveInternalCartage.GetContainers()
		{
			return GetContainersForInternalCartage();
		}

		protected virtual IContainer[] GetContainersForInternalCartage()
		{
			return Containers.Where(x => x.JC_ContainerMode != Core.Constants.ContainerModes.Groupage).ToArray();
		}

		IPackLineInfo[] IHaveInternalCartage.GetPackLines()
		{
			return (IPackLineInfo[])OuterPackLines.ToArray(typeof(IPackLineInfo));
		}

		ZBool IHaveInternalCartage.DeliveryAndPickupCartageApplicable
		{
			get { return true; }
		}

		ZBool IHaveInternalCartage.PackedAtDepot
		{
			get { return ZBool.False; }
		}

		ZString IHaveInternalCartage.TransportMode
		{
			get { return JS_TransportMode; }
		}

		ZString IHaveInternalCartage.ContainerMode
		{
			get { return JS_PackingMode; }
		}

		ZString IHaveInternalCartage.CartageTypeOverride
		{
			get { return JS_CartageTypeOverride; }
		}

		protected virtual ZString JS_CartageTypeOverride
		{
			get { return ZString.Empty; }
		}

		ZPropertyInfo IHaveInternalCartage.DeliveryCartageAdvisedInfo
		{
			get { return DocsAndCartage.JP_DeliveryCartageAdvisedInfo; }
		}

		ZPropertyInfo IHaveInternalCartage.PickupCartageAdvisedInfo
		{
			get { return DocsAndCartage.JP_PickupCartageAdvisedInfo; }
		}

		#region Addresses

		#region Depot (CFS) Addresses

		public ZGuid CartagePickupDepotAddress
		{
			get { return PickupDepotAddress; }
		}

		ZPropertyInfo IHaveInternalCartage.CartagePickupDepotAddressInfo
		{
			get { return PickupDepotAddressInfo; }
		}

		public ZGuid CartageDeliveryDepotAddress
		{
			get { return DeliveryDepotAddress; }
		}

		ZPropertyInfo IHaveInternalCartage.CartageDeliveryDepotAddressInfo
		{
			get { return DeliveryDepotAddressInfo; }
		}

		#endregion

		#region CTO Addresses

		ZGuid IHaveInternalCartage.CartagePickupCTOAddress
		{
			get { return PickupCTOAddress; }
		}

		ZPropertyInfo IHaveInternalCartage.CartagePickupCTOAddressInfo
		{
			get { return PickupCTOAddressInfo; }
		}

		ZGuid IHaveInternalCartage.CartageDeliveryCTOAddress
		{
			get { return DeliveryCTOAddress; }
		}

		ZPropertyInfo IHaveInternalCartage.CartageDeliveryCTOAddressInfo
		{
			get { return DeliveryCTOAddressInfo; }
		}

		#endregion

		#region ContainerYard Addresses

		ZGuid IHaveInternalCartage.CartagePickupContainerYardAddress
		{
			get { return PickupContainerYardAddress; }
		}

		ZPropertyInfo IHaveInternalCartage.CartagePickupContainerYardAddressInfo
		{
			get { return PickupContainerYardAddressInfo; }
		}

		ZGuid IHaveInternalCartage.CartageDeliveryContainerYardAddress
		{
			get { return DeliveryContainerYardAddress; }
		}

		ZPropertyInfo IHaveInternalCartage.CartageDeliveryContainerYardAddressInfo
		{
			get { return DeliveryContainerYardAddressInfo; }
		}

		#endregion

		#region Importer/Exporter Addresses

		JobDocAddress IHaveInternalCartage.CartageExporterDocAddress
		{
			get { return ExporterDocAddress; }
		}

		JobDocAddress IHaveInternalCartage.CartageImporterDocAddress
		{
			get { return ImporterDocAddress; }
		}

		ZValidation IDocAddresses.PiggyBackedDocAddressValidation(JobDocAddress addressToValidate)
		{
			return PiggyBackedDocAddressValidation(addressToValidate);
		}

		protected virtual ShipmentDocAddressValidation PiggyBackedDocAddressValidation(JobDocAddress addressToValidate)
		{
			return new ShipmentDocAddressValidation(addressToValidate);
		}

		#endregion

		#endregion

		event EventHandler IShipmentWithDocsAndCartage.TransportModeChanged
		{
			add { JS_TransportModeInfo.ValueChanged += value; }
			remove { JS_TransportModeInfo.ValueChanged -= value; }
		}

		bool IHaveInternalCartage.TypeSpecificPreCreationCheck()
		{
			return true;
		}

		ZBool IHaveInternalCartage.InternalCartageEnabled
		{
			get { return IsInternalCartageAllowed; }
		}

		#endregion

		protected virtual ZBool IsInternalCartageAllowed
		{
			get { return ZBool.True; }
		}

		protected virtual ZGuid CartageClientIDDefaultOnShipmentSpecificInternalCartageSetup
		{
			get { return this.IsImport() ? ConsigneePK : ConsignorPK; }
		}

		bool IShipmentWithDocsAndCartage.ShouldValidateDeliveryAndPickupCartageCoBeingSame()
		{
			return !this.IsDomestic();
		}

		bool IShipmentWithDocsAndCartage.ShouldValidateDeliveryCoPK()
		{
			return JS_IsForwardRegistered;
		}

		bool IShipmentWithDocsAndCartage.RequiresOrderNumbersOnDocs()
		{
			return RequireOrderNumbersOnDocs();
		}

		bool IShipmentWithDocsAndCartage.RequiresOrderTrackLink()
		{
			return (Consignee?.MiscServ is OrgMiscServ consigneeMiscServ && consigneeMiscServ.OM_IMJobRequireOrderTrackLink) ||
				(Consignor?.MiscServ is OrgMiscServ consignorMiscServ && consignorMiscServ.OM_EXJobRequireOrderTrackLink);
		}

		JobDocsAndCartageValidation IShipmentWithDocsAndCartage.PiggyBackedValidation
		{
			get { return Declaration == null ? null : Declaration.PiggyBackedValidation; }
		}

		#region Declaration

		IShipmentWithDocsAndCartage Declaration
		{
			get
			{
				if (fDeclaration == null && CanHaveDeclarations)
				{
					var jobDecFilter = JobDeclarationFilter.ForCompanyAndShipment(true, GlbCompany.CurrentCompany, PK);
					if (!IsInDatabase)
					{
						jobDecFilter.FetchOnlyFromLocalCache = true;
					}
					fDeclaration = (IShipmentWithDocsAndCartage)Factory.LoadTop1<IBaseJobDeclaration>(jobDecFilter);
				}
				return fDeclaration;
			}
		}
		IShipmentWithDocsAndCartage fDeclaration;

		#endregion

		string IShipmentWithDocsAndCartage.UniqueConsignRef
		{
			get { return JS_UniqueConsignRef; }
		}

		string IShipmentWithDocsAndCartage.MasterBillNumber
		{
			get { return (Consols.Count > 0) ? Consols[0].JK_MasterBillNum : ZString.Empty; }
		}

		string IShipmentWithDocsAndCartage.HouseBillNumber
		{
			get { return JS_HouseBill; }
		}

		protected virtual bool RequireOrderNumbersOnDocs()
		{
			return (Consignee?.MiscServ is OrgMiscServ consigneeMiscServ && consigneeMiscServ.OM_IMImporterRequiresOrderNumbersOnDocs) ||
				(Consignor?.MiscServ is OrgMiscServ consignorMiscServ && consignorMiscServ.OM_EXExporterRequiresOrderNumbersOnDocs);
		}

		#endregion

		#region IManifestProvider Members

		public virtual EDIMessageCollection Messages
		{
			get
			{
				if (fMessages == null)
				{
					fMessages = new EDIMessageCollection(this, Factory);
					fMessages.Load();
					fMessages.IsManagedForDataRefresh = true;
				}
				return fMessages;
			}
		}

		EDIMessageCollection fMessages;

		event EventHandler IManifestProvider.CustomsManifestVisibilityChanged
		{
			add
			{
				JS_TransportModeInfo.ValueChanged += value;
				JS_ShipmentTypeInfo.ValueChanged += value;
				if (fConsols != null && customsManifestVisibilityChanged == null)
				{
					fConsols.ConsolLoadOrDischargePortChanged += ConsolLoadOrDischargePortChanged_ForCustomsManifestVisibilityChanged;
					fConsols.CountChanged += ConsolCountChanged_ForCustomsManifestVisibilityChanged;
				}
				customsManifestVisibilityChanged += value;
				JS_HouseBillInfo.ValueChanged += value;
				JS_RL_NKDestinationInfo.ValueChanged += value;
				JS_RL_NKOriginInfo.ValueChanged += value;
			}
			remove
			{
				JS_TransportModeInfo.ValueChanged -= value;
				JS_ShipmentTypeInfo.ValueChanged -= value;
				customsManifestVisibilityChanged -= value;
				if (fConsols != null && customsManifestVisibilityChanged == null)
				{
					fConsols.ConsolLoadOrDischargePortChanged -= ConsolLoadOrDischargePortChanged_ForCustomsManifestVisibilityChanged;
					fConsols.CountChanged -= ConsolCountChanged_ForCustomsManifestVisibilityChanged;
				}
				JS_HouseBillInfo.ValueChanged -= value;
				JS_RL_NKDestinationInfo.ValueChanged -= value;
				JS_RL_NKOriginInfo.ValueChanged -= value;
			}
		}
		EventHandler customsManifestVisibilityChanged;

		void ConsolCountChanged_ForCustomsManifestVisibilityChanged(object sender, CollectionCountChangedEventArgs e)
		{
			if (e.ItemAdded)
			{
				var consol = (CommonConsol)e.BizObject;
				if (consol.JK_IsCFS && consol.JK_IsForwarding && !JS_IsCFSRegistered)
				{
					JS_IsCFSRegistered = true;
				}
			}

			if (customsManifestVisibilityChanged != null)
			{
				customsManifestVisibilityChanged(this, EventArgs.Empty);
			}
		}

		void ConsolLoadOrDischargePortChanged_ForCustomsManifestVisibilityChanged(object sender, EventArgs e)
		{
			if (customsManifestVisibilityChanged != null)
			{
				customsManifestVisibilityChanged(this, EventArgs.Empty);
			}
		}

		#endregion

		#region IImportExport Members

		public Directions JobDirection
		{
			get { return ImportExportHelper.GetJobDirection(JS_RL_NKOrigin, JS_RL_NKDestination); }
		}

		#endregion

		#region IShipmentProvider Members

		CommonShipment IShipmentProvider.Shipment
		{
			get { return this; }
		}

		#endregion

		#region ICusCodeDataTypeSupporter Members

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return (IBusinessObjectFetchStrategy)Activator.CreateInstance(ObjectFactory.GetType<ICusCodeDataTypeSupporterFetchStrategy>(), this);
		}

		IDictionary<ZString, Type> ICusCodeDataTypeSupporter.GetCusCodeDataTypes()
		{
			return GetCusCodeDataTypesCore();
		}

		protected virtual IDictionary<ZString, Type> GetCusCodeDataTypesCore()
		{
			var result = new Dictionary<ZString, Type>();
			result.Add(Customs.Common.AU.CusCodeDataTypeList.Codes.CustomsManifestLineSequence, ObjectFactory.GetType<AU.ICustomsManifestLineSequence>()); // TODO: Delete ForwardingShipmentBusinessObjectTest.TestCorrectlyDeleteChildrenIfSupporterInterfacesIsUsed when ICustomsManifestLineSequence is removed
			return result;
		}

		#endregion

		void DeactivateActiveCollection(IActiveBusinessObjectCollection collection)
		{
			if (collection != null)
			{
				collection.Deactivate();
			}
		}

		public virtual void DeactivateActiveBusinessObjectCollections()
		{
			if (outerPackLines != null)
			{
				outerPackLines.DeactivateActiveBusinessObjectCollections();
			}

			if (innerPackLines != null)
			{
				innerPackLines.DeactivateActiveBusinessObjectCollections();
			}

			DeactivateActiveCollection(pickupConfirms);
			DeactivateActiveCollection(deliveryConfirms);
			DeactivateActiveCollection(originCFSArrival);
			DeactivateActiveCollection(originCFSDeparture);
			DeactivateActiveCollection(destinationCFSArrival);
			DeactivateActiveCollection(destinationCFSDeparture);
		}

		#region Implementation

		protected void DefaultJS_ShipmentStatus()
		{
			if (JS_IsForwardRegistered)
			{
				JS_ShipmentStatus = IsSea ? ShipmentStatusList.Codes.Confirmed : string.Empty;
			}
		}

		#region PacksTotalsDiffer

		protected bool PacksTotalsDiffer()
		{
			bool result = false;
			if (InnerPackLines.Count > 0)
			{
				result = TotalInnerPackLinePackages != JS_TotalPackageCount;
			}
			return result;
		}

		#endregion

		public virtual ZGuid GetContainerPKForNewChild()
		{
			return ZGuid.Empty;
		}

		protected bool fIsImportingData;
		bool ISupportDataImporting.IsImportingData
		{
			get { return fIsImportingData; }
			set { fIsImportingData = value; }
		}

		#region IsSettingDefaultOrImportingData

		public bool IsSettingDefaultOrImportingData
		{
			get { return IsSettingDefaultValues || fIsImportingData; }
		}

		#endregion

		#region Tasks

		ProcessTask.Loader ProcessTaskLoader
		{
			get
			{
				if (processTaskLoader == null)
				{
					processTaskLoader = new ProcessTask.Loader(Factory);
				}
				return processTaskLoader;
			}
		}
		ProcessTask.Loader processTaskLoader;

		#endregion

		void MarkDocsAndCartageAsNeedsValidation()
		{
			JobDocsAndCartage docsAndCartage = JobDocsAndCartage.Load(this, !IsInDatabase);
			if (docsAndCartage != null)
			{
				docsAndCartage.MarkAsNeedingValidation();
			}
		}

		public ZString GetRegistryDefaultContainerMode()
		{
			var result = ZString.Empty;
			if (!JS_TransportMode.IsEmpty)
			{
				var containerModes = Factory.GetCachedValue("DefaultContainerMode", () => FreightConfigurationRegistry.Instance.DefaultContainerModes.Value);
				result = containerModes.FindDefaultContainerMode(JS_TransportMode);
			}
			return result;
		}

		protected override ZAddress GetNewJS_OA_ExportReceivingDepot_ZAddress()
		{
			ZAddress result = base.GetNewJS_OA_ExportReceivingDepot_ZAddress();
			result.DefaultAddressType = AddressType.PIC;
			return result;
		}

		protected override ZAddress GetNewJS_OA_ImportReleaseDepot_ZAddress()
		{
			ZAddress result = base.GetNewJS_OA_ImportReleaseDepot_ZAddress();
			result.DefaultAddressType = AddressType.DLV;
			return result;
		}

		#region INCO Terms

		public bool IsCollect
			=> RatingAdapter.PaymentTerm.GetPrepaidCollect(CostSell.Revenue, ChargeCodeGroupList.Codes.Freight) == Constants.PaymentType.Collect;

		public bool IsPrepaid
			=> RatingAdapter.PaymentTerm.GetPrepaidCollect(CostSell.Revenue, ChargeCodeGroupList.Codes.Freight) == Constants.PaymentType.Prepaid;

		#endregion

		#region Chargeable Weight

		public void UpdateChargeableWeights()
		{
			UpdateActualChargeableWeight();
			UpdateDocumentedChargeableWeight();
			UpdateManifestedChargeableWeight();
		}

		protected void UpdateActualChargeableWeight()
		{
			if (WeightVolumeUnitsAreValid && !isSettingActualChargeable && !IsCloning && !IsCopying && !IsSettingActualChargeableSuspended)
			{
				JS_ActualChargeable = ChargeableWeight(JS_ActualVolume, JS_ActualWeight, JS_LoadingMeters);
			}
		}

		protected virtual void UpdateDocumentedChargeableWeight()
		{
			if (WeightVolumeUnitsAreValid && !isSettingActualChargeable)
			{
				JS_DocumentedChargeable = ChargeableWeight(JS_DocumentedVolume, JS_DocumentedWeight, JS_DocumentedLoadingMeters);
			}
		}

		protected virtual void UpdateManifestedChargeableWeight()
		{
			if (WeightVolumeUnitsAreValid && !isSettingActualChargeable)
			{
				JS_ManifestedChargeable = ChargeableWeight(JS_ManifestedVolume, JS_ManifestedWeight, JS_ManifestedLoadingMeters);
			}
		}

		protected bool WeightVolumeUnitsAreValid
		{
			get { return !JS_UnitOfVolumeInfo.HasErrors() && !JS_UnitOfWeightInfo.HasErrors(); }
		}

		protected decimal ChargeableWeight(decimal volume, decimal weight, decimal loadingMeters = 0m)
		{
			if (!IsValidationSuspended)
			{
				Validation.ValidateJS_UnitOfWeight();
				Validation.ValidateJS_UnitOfVolume();
			}

			string weightUnit = FreightUtilities.IsValidWeightUnit(JS_UnitOfWeight) ? JS_UnitOfWeight.ToString() : Env.Registry.FreightWeightUnit;
			string volumeUnit = FreightUtilities.IsValidVolumeUnit(JS_UnitOfVolume) ? JS_UnitOfVolume.ToString() : Env.Registry.FreightVolumeUnit;

			return ChargeableWeight(weight, weightUnit, volume, volumeUnit, loadingMeters);
		}

		protected decimal ChargeableWeight(decimal weight, string weightUnit, decimal volume, string volumeUnit, decimal loadingMeters)
		{
			return ChargeableAmountCalculator.CalculateChargeable(new ChargeableParameters
			{
				Weight = new ZWeight(weight, weightUnit),
				Volume = new ZVolume(volume, volumeUnit),
				LoadingLength = new Quantity(loadingMeters, Constants.LoadingLength.LoadingMeters),
				TargetUnit = JS_ChargeableUnit,
				ConversionFactors = ChargeableAmountCalculator.GetDefaultConversionFactors(IsDomesticFreightForChargeableWeightCalculations, JS_TransportMode, JS_ChargeableUnit)
			}).Chargeable.Amount;
		}

		bool IsDomesticFreightForChargeableWeightCalculations
		{
			get
			{
				return IsDomesticFreight || AreBothPortsInUnitedStatesOrUSOverseasTerritories();
			}
		}

		bool AreBothPortsInUnitedStatesOrUSOverseasTerritories()
		{
			var originCountry = JS_RL_NKOrigin.SubstringSafe(0, 2);
			var destinationCountry = JS_RL_NKDestination.SubstringSafe(0, 2);

			return Constants.CountryCodes.IsUsaOrTerritory(originCountry)
				&& Constants.CountryCodes.IsUsaOrTerritory(destinationCountry);
		}

		#endregion

		#region Setting Actual Chargeable Suspender

		public IDisposable SuspendSettingActualChargeable()
		{
			return new DisposableAction(() => settingActualChargeableSuspendedIndex++, () => settingActualChargeableSuspendedIndex--);
		}

		public bool IsSettingActualChargeableSuspended
		{
			get { return settingActualChargeableSuspendedIndex > 0; }
		}

		int settingActualChargeableSuspendedIndex;

		#endregion

		#region HBLContainerPackModeOverride

		protected virtual void UpdateHBLContainerPackModeOverride()
		{
			JS_HBLContainerPackModeOverride = !JS_PackingMode.IsEmpty
				? FreightUtilities.ShipmentHBLDeliveryMode(JS_PackingMode).DefaultHBLDeliveryMode
				: ZString.Empty;
		}

		#endregion

		protected virtual void OnTransportsIncludingRelatedCreated()
		{
		}

		#endregion

		#region Set Defaults When Consignee/or Changes

		bool SettingChangeImplications;

		#region SetConsignorOrConsigneeChanges

		protected virtual void SetConsignorChanges()
		{
			SetDefaultExportBroker();

			if (ShouldSetGoodsCurrencyOnConsignorChanges)
			{
				SetDefaultGoodsCurrency();
			}
		}

		protected virtual bool ShouldSetGoodsCurrencyOnConsignorChanges
		{
			get { return true; }
		}

		protected virtual void SetConsigneeChanges()
		{
			SetDefaultImportBroker();
		}

		protected virtual void SetConsignorConsigneeCommonChanges()
		{
			if (ShouldSetServiceLevelOnConsignorOrConsigneeChange)
			{
				SetServiceLevel();
			}
		}

		protected virtual bool ShouldSetServiceLevelOnConsignorOrConsigneeChange
		{
			get { return true; }
		}

		#endregion

		#region SetDefaultExportBroker

		public void SetDefaultExportBroker()
		{
			var defaultExportBrokerPK = GetDefaultExportBrokerPK();
			if (defaultExportBrokerPK.HasValue)
			{
				JS_OH_ExportBroker = defaultExportBrokerPK.Value;
			}
		}

		ZGuid? GetDefaultExportBrokerPK()
		{
			if (Consignor != null && !IsChangingConsignorAddress && (this.IsExport() || IsExportFromAnyCountry && ImportExportHelper.IsAnyBranchCountry(Factory, JS_RL_NKOrigin)))
			{
				var exportBroker = Consignor.GetRelatedParty(RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Pickup, JS_TransportMode, JS_PackingMode,
					(DepartureConsol != null && !DepartureConsol.JK_RL_NKLoadPort.IsEmpty) ? DepartureConsol.JK_RL_NKLoadPort : JS_RL_NKOrigin);

				return exportBroker != null ? exportBroker.PK : ZGuid.Empty;
			}

			return null;
		}

		bool IsExportFromAnyCountry
		{
			get { return !JS_RL_NKOrigin.IsEmpty && !JS_RL_NKDestination.IsEmpty && JS_RL_NKOrigin.SubstringSafe(0, 2) != JS_RL_NKDestination.SubstringSafe(0, 2); }
		}

		#endregion

		#region SetDefaultImportBroker

		public void SetDefaultImportBroker()
		{
			var defaultImportBrokerPK = GetDefaultImportBrokerPK();
			if (defaultImportBrokerPK.HasValue)
			{
				JS_OH_ImportBroker = defaultImportBrokerPK.Value;
			}
		}

		protected bool IsImportToAnyCountry
		{
			get { return !JS_RL_NKOrigin.IsEmpty && !JS_RL_NKDestination.IsEmpty && JS_RL_NKOrigin.SubstringSafe(0, 2) != JS_RL_NKDestination.SubstringSafe(0, 2); }
		}

		protected virtual ZGuid? GetDefaultImportBrokerPK()
		{
			if (Consignee != null && !IsChangingConsigneeAddress && (this.IsImport() || IsImportToAnyCountry && ImportExportHelper.IsAnyBranchCountry(Factory, JS_RL_NKDestination)))
			{
				var importBroker = Consignee.GetRelatedParty(RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Delivery, JS_TransportMode, JS_PackingMode,
					JS_Calc_LastDischargePort.IsEmpty ? JS_RL_NKDestination : JS_Calc_LastDischargePort);
				return importBroker != null ? importBroker.PK : ZGuid.Empty;
			}

			return null;
		}

		#endregion

		#region SetCartage

		void SetCartage(OrgHeader org, ZGuid cartageCompanyPK, ZString pickUpOrDelivery, ZString port, Action<ZGuid> setCartageCoPK)
		{
			if (IsDocsAndCartageSet)
			{
				if (org != null && (!IsInDatabase || cartageCompanyPK.IsEmpty))
				{
					ZGuid defaultCartageCompanyPK = ZGuid.Empty;

					OrgHeader cartageOrg = null;
					if (JS_TransportMode == Constants.TransportModes.Air)
					{
						cartageOrg = org.GetRelatedParty(RelatedPartyTypeList.Codes.LocalTransport, pickUpOrDelivery, Constants.TransportModes.Air, ZString.Empty, port);
						defaultCartageCompanyPK = FreightDataRegistry.Instance.AIRCartageCompany.Value;
					}
					else if (JS_TransportMode == Constants.TransportModes.Sea)
					{
						bool isPickup = pickUpOrDelivery == RelatedPartyDirectionList.Codes.Pickup;

						if (JS_PackingMode == Constants.ContainerModes.FCL || (!isPickup && JS_PackingMode == Constants.ContainerModes.BuyersConsol))
						{
							cartageOrg = org.GetRelatedParty(RelatedPartyTypeList.Codes.LocalTransport, pickUpOrDelivery, JS_TransportMode, Constants.ContainerModes.FCL, port);
							defaultCartageCompanyPK = FreightDataRegistry.Instance.FCLCartageCompany.Value;
						}
						else if (JS_PackingMode == Constants.ContainerModes.LCL || (isPickup && JS_PackingMode == Constants.ContainerModes.BuyersConsol))
						{
							cartageOrg = org.GetRelatedParty(RelatedPartyTypeList.Codes.LocalTransport, pickUpOrDelivery, JS_TransportMode, Constants.ContainerModes.LCL, port);
							defaultCartageCompanyPK = FreightDataRegistry.Instance.LCLCartageCompany.Value;
						}
					}
					else
					{
						cartageOrg = org.GetRelatedParty(RelatedPartyTypeList.Codes.LocalTransport, pickUpOrDelivery, JS_TransportMode, JS_PackingMode, port);
					}

					if (cartageOrg != null)
					{
						setCartageCoPK(cartageOrg.PK);
					}
					else if (!defaultCartageCompanyPK.IsEmpty && ShouldDefaultCartageCompanyFromRegistry(org.OH_RL_NKClosestPort))
					{
						setCartageCoPK(defaultCartageCompanyPK);
					}
				}
			}
		}

		protected bool ShouldDefaultCartageCompanyFromRegistry(ZString closestPortUnloco)
		{
			bool result = (closestPortUnloco == GlbBranch.CurrentBranch.GB_RL_NKHomePort);

			if (!result)
			{
				foreach (GlbBranchExtraPorts extraPort in GlbBranch.CurrentBranch.ExtraPorts)
				{
					if (closestPortUnloco == extraPort.GY_RL_NKAdditionalBranchRelatedPort)
					{
						result = true;
						break;
					}
				}
			}

			return result;
		}

		void SetExportCartage()
		{
			SetCartage(Consignor, DocsAndCartage.PickupCartageCoPK, RelatedPartyDirectionList.Codes.Pickup, JS_RL_NKOrigin, c => DocsAndCartage.PickupCartageCoPK = c);
		}

		protected virtual void SetImportCartage()
		{
			SetCartage(Consignee, DocsAndCartage.DeliveryCartageCoPK, RelatedPartyDirectionList.Codes.Delivery, JS_RL_NKDestination, c => DocsAndCartage.DeliveryCartageCoPK = c);
		}

		void DeleteIncompatiblePickupDeliveryConfirms()
		{
			List<CommonPickupDeliveryConfirm> allPickupDeliveryConfirms = new List<CommonPickupDeliveryConfirm>();
			allPickupDeliveryConfirms.AddRange(PickupConfirms);
			allPickupDeliveryConfirms.AddRange(DeliveryConfirms);

			foreach (CommonContainer container in Containers)
			{
				allPickupDeliveryConfirms.AddRange(container.Confirms);
			}

			foreach (CommonPickupDeliveryConfirm confirm in allPickupDeliveryConfirms)
			{
				if (!confirm.IsDeleted && confirm.IsConfirmHiddenOnAllShipments)
				{
					confirm.Delete();
				}
			}
		}

		#endregion

		#region SetPickupEquipmentNeeded

		protected void SetPickupEquipmentNeeded()
		{
			if (IsDocsAndCartageSet)
			{
				ZString pickupEquipmentNeeded = "";
				if (Consignor != null && Consignor.MiscServ != null)
				{
					if (JS_TransportMode == Core.Constants.TransportModes.Air)
					{
						pickupEquipmentNeeded = ConsignorPickupAddress.AirCartageEquipmentNeeded;
					}
					else
					{
						switch (JS_PackingMode)
						{
							case Constants.ContainerModes.BuyersConsol:
							case Constants.ContainerModes.LCL:
								pickupEquipmentNeeded = ConsignorPickupAddress.LCLCartageEquipmentNeeded;
								break;
							case Constants.ContainerModes.FCL:
								pickupEquipmentNeeded = ConsignorPickupAddress.FCLCartageEquipmentNeeded;
								break;
						}
					}
				}

				if (JS_IsBooking && pickupEquipmentNeeded == Constants.EquipmentNeeded.Ask)
				{
					pickupEquipmentNeeded = "";
				}
				DocsAndCartage.JP_FCLPickupEquipmentNeeded = pickupEquipmentNeeded;
			}
		}

		#endregion

		#region SetOriginToConsignorLocation

		protected virtual void SetOriginToConsignorLocation()
		{
			ZBool shouldDefault = (!JS_IsForwardRegistered || !FreightDataRegistry.Instance.DefaultShipmentOriginFromConsolLoad.Value)
				&& !suspendSettingOriginFromConsignorLocation;
			if (shouldDefault)
			{
				ZString newOriginCode;
				if (ConsignorDocumentaryAddress.Address != null && !ConsignorDocumentaryAddress.Address.OA_RL_NKRelatedPortCode.IsEmpty)
				{
					newOriginCode = ((Consignor != null) ? ConsignorDocumentaryAddress.Address.OA_RL_NKRelatedPortCode : ZString.Empty);
				}
				else
				{
					newOriginCode = ((Consignor != null) ? Consignor.OH_RL_NKClosestPort : ZString.Empty);
				}

				if (!newOriginCode.IsEmpty && (!IsInDatabase || JS_RL_NKOriginInfo.Value.IsEmpty))
				{
					JS_RL_NKOrigin = newOriginCode;
				}
			}
		}

		#endregion

		#region SetDefaultGoodsCurrency

		protected virtual void SetDefaultGoodsCurrency()
		{
			if (JS_RX_NKGoodsValueCurr.IsEmpty || JS_GoodsValue.IsEmpty || Convert.ToDouble(JS_GoodsValue) == 0.0)
			{
				RefCurrency currency = (Consignor != null) ? Consignor.MiscServ.EXDefCurrency : null;
				if (currency != null && currency.RX_Code != ZString.Empty)
				{
					JS_RX_NKGoodsValueCurr = currency.RX_Code;
				}
				else
				{
					JS_RX_NKGoodsValueCurr = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
				}
			}
		}

		#endregion

		#region SetServiceLevel

		protected void SetServiceLevel()
		{
			if (Consignee != null && Consignee.MiscServ != null && !Consignee.MiscServ.OM_RS_NKIMDefaultServiceLevel.IsEmpty)
			{
				JS_RS_NKServiceLevel = Consignee.MiscServ.OM_RS_NKIMDefaultServiceLevel;
			}
			else if (Consignor != null && Consignor.MiscServ != null && !Consignor.MiscServ.OM_RS_NKEXDefaultServiceLevel.IsEmpty)
			{
				JS_RS_NKServiceLevel = Consignor.MiscServ.OM_RS_NKEXDefaultServiceLevel;
			}
			else if (!IsInDatabase && RegistryServiceLevel != null)
			{
				JS_RS_NKServiceLevel = RegistryServiceLevel.RS_Code;
			}
		}

		#endregion

		#region SetDeliveryEquipmentNeeded

		protected void SetDeliveryEquipmentNeeded()
		{
			if (IsDocsAndCartageSet)
			{
				ZString deliveryEquipmentNeeded = "";
				if (Consignee != null && Consignee.MiscServ != null)
				{
					if (JS_TransportMode == Core.Constants.TransportModes.Air)
					{
						deliveryEquipmentNeeded = ConsigneeDeliveryAddress.AirCartageEquipmentNeeded;
					}
					else
					{
						switch (JS_PackingMode)
						{
							case Constants.ContainerModes.LCL:
								deliveryEquipmentNeeded = ConsigneeDeliveryAddress.LCLCartageEquipmentNeeded;
								break;
							case Constants.ContainerModes.BuyersConsol:
							case Constants.ContainerModes.FCL:
								deliveryEquipmentNeeded = ConsigneeDeliveryAddress.FCLCartageEquipmentNeeded;
								break;
						}
					}
				}

				if (JS_IsBooking && deliveryEquipmentNeeded == Constants.EquipmentNeeded.Ask)
				{
					deliveryEquipmentNeeded = "";
				}
				DocsAndCartage.JP_FCLDeliveryEquipmentNeeded = deliveryEquipmentNeeded;
			}
		}

		#endregion

		#region SetDestinationToConsigneeLocation

		protected void SetDestinationToConsigneeLocation()
		{
			ZBool shouldDefault = (!JS_IsForwardRegistered || !FreightDataRegistry.Instance.DefaultShipmentDestinationFromConsolDischarge.Value)
				&& !suspendSettingDestFromConsigneeLocation;
			if (shouldDefault)
			{
				ZString newDestination;
				if (ConsigneeDocumentaryAddress.Address != null && !ConsigneeDocumentaryAddress.Address.OA_RL_NKRelatedPortCode.IsEmpty)
				{
					newDestination = (Consignee != null) ? ConsigneeDocumentaryAddress.Address.OA_RL_NKRelatedPortCode : ZString.Empty;
				}
				else
				{
					newDestination = (Consignee != null) ? Consignee.OH_RL_NKClosestPort : ZString.Empty;
				}

				if (!newDestination.IsEmpty && (!IsInDatabase || JS_RL_NKDestination.IsEmpty))
				{
					JS_RL_NKDestination = newDestination;
				}
			}
		}

		#endregion

		#region SetDefaultNotifyParty

		protected virtual void SetDefaultNotifyParty()
		{
			NotifyPartyDocumentaryAddress.ContactPK = DefaultContactFinder.GetDefaultNotifyPartyContact(Consignee);
		}

		#endregion

		public bool HasRelatedReceivingAgent(CommonConsol consol)
		{
			if (consol != null)
			{
				var consigneeReceivingAgentDelivery = FindRelatedPartyPK(Consignee, RelatedPartyTypeList.Codes.ReceivingAgent, RelatedPartyDirectionList.Codes.Delivery, consol.JK_TransportMode, consol.JK_ConsolMode, consol.JK_RL_NKDischargePort);
				var consigneeReceivingAgentPickupAndDelivery = FindRelatedPartyPK(Consignee, RelatedPartyTypeList.Codes.ReceivingAgent, RelatedPartyDirectionList.Codes.PickupAndDelivery, consol.JK_TransportMode, consol.JK_ConsolMode, consol.JK_RL_NKDischargePort);
				var consignorReceivingAgentPickup = FindRelatedPartyPK(Consignor, RelatedPartyTypeList.Codes.ReceivingAgent, RelatedPartyDirectionList.Codes.Pickup, consol.JK_TransportMode, consol.JK_ConsolMode, consol.JK_RL_NKDischargePort);

				if (consigneeReceivingAgentDelivery.IsEmpty && consigneeReceivingAgentPickupAndDelivery.IsEmpty && consignorReceivingAgentPickup.IsEmpty)
				{
					return true;
				}

				return consol.ReceivingForwarderPK == consigneeReceivingAgentDelivery
						 || consol.ReceivingForwarderPK == consigneeReceivingAgentPickupAndDelivery
						 || consol.ReceivingForwarderPK == consignorReceivingAgentPickup;
			}

			return true;
		}

		public bool HasRelatedSendingAgent(CommonConsol consol)
		{
			if (consol != null)
			{
				var consignorSendingAgentPickup = FindRelatedPartyPK(Consignor, RelatedPartyTypeList.Codes.SendingAgent, RelatedPartyDirectionList.Codes.Pickup, consol.JK_TransportMode, consol.JK_ConsolMode, consol.JK_RL_NKLoadPort);
				var consignorSendingAgentPickupAndDelivery = FindRelatedPartyPK(Consignor, RelatedPartyTypeList.Codes.SendingAgent, RelatedPartyDirectionList.Codes.PickupAndDelivery, consol.JK_TransportMode, consol.JK_ConsolMode, consol.JK_RL_NKLoadPort);
				var consigneeSendingAgentDelivery = FindRelatedPartyPK(Consignee, RelatedPartyTypeList.Codes.SendingAgent, RelatedPartyDirectionList.Codes.Delivery, consol.JK_TransportMode, consol.JK_ConsolMode, consol.JK_RL_NKLoadPort);

				if (consigneeSendingAgentDelivery.IsEmpty && consignorSendingAgentPickup.IsEmpty && consignorSendingAgentPickupAndDelivery.IsEmpty)
				{
					return true;
				}

				return consol.SendingForwarderPK == consigneeSendingAgentDelivery
						|| consol.SendingForwarderPK == consignorSendingAgentPickup
						|| consol.SendingForwarderPK == consignorSendingAgentPickupAndDelivery;
			}

			return true;
		}

		ZGuid FindRelatedPartyPK(OrgHeader organisation, ZString partyType, ZString direction, ZString transportMode, ZString containerMode, ZString location)
		{
			if (organisation != null)
			{
				var relatedParty = organisation.GetRelatedParty(partyType, direction, transportMode, containerMode, location);
				return relatedParty != null ? relatedParty.PK : ZGuid.Empty;
			}

			return ZGuid.Empty;
		}

		#endregion

		#region Default Forwarding Agents on Consol

		public ZGuid FindRelatedReceivingForwarderAddressPK()
		{
			OrgHeader relatedParty = null;
			if (Consignee != null)
			{
				relatedParty = Consignee.GetRelatedParty(RelatedPartyTypeList.Codes.ReceivingAgent, RelatedPartyDirectionList.Codes.Delivery, JS_TransportMode, JS_PackingMode, JS_RL_NKDestination)
					?? Consignee.GetRelatedParty(RelatedPartyTypeList.Codes.ReceivingAgent, RelatedPartyDirectionList.Codes.PickupAndDelivery, JS_TransportMode, JS_PackingMode, JS_RL_NKDestination);
			}

			if (relatedParty == null && Consignor != null)
			{
				relatedParty = Consignor.GetRelatedParty(RelatedPartyTypeList.Codes.ReceivingAgent, RelatedPartyDirectionList.Codes.Pickup, JS_TransportMode, JS_PackingMode, JS_RL_NKDestination);
			}

			return relatedParty != null ? relatedParty.MainAddress.PK : ZGuid.Empty;
		}

		public ZGuid FindRelatedSendingForwarderAddressPK()
		{
			OrgHeader relatedParty = null;

			if (Consignor != null)
			{
				relatedParty = Consignor.GetRelatedParty(RelatedPartyTypeList.Codes.SendingAgent, RelatedPartyDirectionList.Codes.Pickup, JS_TransportMode, JS_PackingMode, JS_RL_NKOrigin)
					?? Consignor.GetRelatedParty(RelatedPartyTypeList.Codes.SendingAgent, RelatedPartyDirectionList.Codes.PickupAndDelivery, JS_TransportMode, JS_PackingMode, JS_RL_NKOrigin);
			}

			if (relatedParty == null && Consignee != null)
			{
				relatedParty = Consignee.GetRelatedParty(RelatedPartyTypeList.Codes.SendingAgent, RelatedPartyDirectionList.Codes.Delivery, JS_TransportMode, JS_PackingMode, JS_RL_NKOrigin);
			}

			return relatedParty != null ? relatedParty.MainAddress.PK : ZGuid.Empty;
		}

		#endregion

		#region Shipment Passes Through Country

		public ZBool ShipmentPassesThroughCountry(ZString country)
		{
			bool result = false;

			if (JS_RL_NKDestination.Left(2) == country || JS_RL_NKOrigin.Left(2) == country)
			{
				result = true;
			}
			else
			{
				foreach (CommonConsol consol in Consols)
				{
					if (consol.ConsolPassesThroughCountry(country))
					{
						result = true;
						break;
					}
				}
			}

			return result;
		}

		#endregion

		#region IBillGenerationSupport Members

		ZString IBillGenerationSupport.TransportMode
		{
			get { return JS_TransportMode; }
		}

		ZString IBillGenerationSupport.ServiceLevel
		{
			get { return JS_RS_NKServiceLevel; }
		}

		protected NumberGeneratorContext NewBillOfLadingGeneratorContext()
		{
			if (NewBillOfLadingGeneratorContextOverride != null)
			{
				return NewBillOfLadingGeneratorContextOverride();
			}
			else
			{
				return NewBillOfLadingGeneratorContextCore();
			}
		}

		protected virtual NumberGeneratorContext NewBillOfLadingGeneratorContextCore()
		{
			return new NumberGeneratorContext();
		}
		public NumberGeneratorContextProviderDelegate NewBillOfLadingGeneratorContextOverride;

		protected virtual NumberGenerator.FountainGetterDelegate GetGeneratorFountain
		{
			get { return Env.NumberFountains.GetForwardingGeneratorFountain; }
		}

		OrgHeader IBillGenerationSupport.CarrierPrincipal
		{
			get
			{
				OrgHeader result = null;
				if (Consols != null && Consols.Count > 0)
				{
					result = Consols[0].ShippingLine;
				}
				else
				{
					result = BookedShippingLine;
				}
				return result;
			}
		}

		ZString IBillGenerationSupport.TranshipmentIndicator
		{
			get { return ""; }
		}

		NumberGeneratorTarget NewShipmentNumberGeneratorTarget()
		{
			return NewShipmentNumberGeneratorTargetCore();
		}

		protected virtual NumberGeneratorTarget NewShipmentNumberGeneratorTargetCore()
		{
			return new ShipmentNumberGeneratorTarget();
		}

		protected NumberGeneratorTarget NewBillOfLadingNumberGeneratorTarget()
		{
			return NewBillOfLadingNumberGeneratorTargetCore();
		}

		protected virtual NumberGeneratorTarget NewBillOfLadingNumberGeneratorTargetCore()
		{
			return new BillOfLadingNumberGeneratorTarget();
		}

		RefUNLOCO IBillGenerationSupport.Load
		{
			get { return LoadCore; }
		}

		protected virtual RefUNLOCO LoadCore
		{
			get { return null; }
		}

		RefUNLOCO IBillGenerationSupport.Discharge
		{
			get { return DischargeCore; }
		}

		protected virtual RefUNLOCO DischargeCore
		{
			get { return null; }
		}

		#region SuppressGeneration

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		protected bool SuppressBillNumberGeneration
		{
			get
			{
				if (suppressBillNumberGeneration.HasValue)
				{
					return suppressBillNumberGeneration.Value;
				}
				else
				{
					bool value = GetSuppressBillNumberGenerationCore();
					suppressBillNumberGeneration = value;
					return value;
				}
			}
		}
		bool? suppressBillNumberGeneration;

		protected virtual bool GetSuppressBillNumberGenerationCore()
		{
			return Env.Registry.AllowManualShipmentEntry;
		}

		#endregion

		#endregion

		#region IJobDocsAndCartage Members

		#region Depot(CFS) Addresses

		protected virtual ZGuid PickupDepotAddress
		{
			get { return (PickupDepotAddressInfo != null) ? (ZGuid)PickupDepotAddressInfo.Value : ZGuid.Empty; }
		}

		protected virtual ZPropertyInfo PickupDepotAddressInfo
		{
			get
			{
				ZPropertyInfo result = null;
				if (JS_OA_ExportReceivingDepot.IsValid)
				{
					result = JS_OA_ExportReceivingDepotInfo;
				}
				else if (DepartureConsol != null)
				{
					result = DepartureConsol.JK_OA_PackDepotAddressInfo;
				}

				return result;
			}
		}

		protected virtual ZGuid DeliveryDepotAddress
		{
			get { return (DeliveryDepotAddressInfo != null) ? (ZGuid)DeliveryDepotAddressInfo.Value : ZGuid.Empty; }
		}

		protected virtual ZPropertyInfo DeliveryDepotAddressInfo
		{
			get
			{
				ZPropertyInfo result = null;
				if (JS_OA_ImportReleaseDepot.IsValid)
				{
					result = JS_OA_ImportReleaseDepotInfo;
				}
				else if (ArrivalConsol != null)
				{
					result = ArrivalConsol.JK_OA_UnpackDepotAddressInfo;
				}

				return result;
			}
		}

		#endregion

		#region CTO Addresses

		protected virtual ZGuid PickupCTOAddress
		{
			get { return (PickupCTOAddressInfo != null) ? (ZGuid)PickupCTOAddressInfo.Value : ZGuid.Empty; }
		}

		protected virtual ZPropertyInfo PickupCTOAddressInfo
		{
			get { return (DepartureConsol != null) ? DepartureConsol.JK_OA_DepartureCTOAddressInfo : null; }
		}

		protected virtual ZGuid DeliveryCTOAddress
		{
			get { return (DeliveryCTOAddressInfo != null) ? (ZGuid)DeliveryCTOAddressInfo.Value : ZGuid.Empty; }
		}

		protected virtual ZPropertyInfo DeliveryCTOAddressInfo
		{
			get { return (ArrivalConsol != null) ? ArrivalConsol.JK_OA_ArrivalCTOAddressInfo : null; }
		}

		#endregion

		#region ContainerYard Addresses

		protected virtual ZGuid PickupContainerYardAddress
		{
			get { return (PickupContainerYardAddressInfo != null) ? (ZGuid)PickupContainerYardAddressInfo.Value : ZGuid.Empty; }
		}

		protected virtual ZPropertyInfo PickupContainerYardAddressInfo
		{
			get { return (DepartureConsol != null) ? DepartureConsol.JK_OA_ContainerYardEmptyPickupAddressInfo : null; }
		}

		protected virtual ZGuid DeliveryContainerYardAddress
		{
			get { return (DeliveryContainerYardAddressInfo != null) ? (ZGuid)DeliveryContainerYardAddressInfo.Value : ZGuid.Empty; }
		}

		protected virtual ZPropertyInfo DeliveryContainerYardAddressInfo
		{
			get { return (ArrivalConsol != null) ? ArrivalConsol.JK_OA_ContainerYardEmptyReturnAddressInfo : null; }
		}

		#endregion

		#region Importer/Exporter Addresses

		protected JobDocAddress ExporterDocAddress
		{
			get { return ConsignorPickupAddress; }
		}

		protected JobDocAddress ImporterDocAddress
		{
			get { return ConsigneeDeliveryAddress; }
		}

		#endregion

		#endregion

		#region IDocManagerSupport Members

		public DocManagerInfo DocManagerInfo
		{
			get { return docManagerInfo ?? (docManagerInfo = NewDocManager()); }
		}
		protected virtual DocManagerInfo NewDocManager()
		{
			return new ShipmentDocManagerInfo(this);
		}
		DocManagerInfo docManagerInfo;

		#endregion

		#region ICancellable Members

		public override string CanCancel()
		{
			string result = JobHeaderParentDeletionHelper.CheckIfCanCancelJobHeaderParent(PK, HumanReadableName);

			if (!string.IsNullOrEmpty(result))
			{
				return result;
			}

			var hasCustomsMessages = Messages
				.Cast<EDIMessage>()
				.Any(message => message.EM_ApplicationCode != ApplicationCodeList.Codes.UniversalDataMessaging
								&& message.EM_ApplicationCode != ApplicationCodeList.Codes.CargoIMPPhase2
								&& message.EM_ApplicationCode != ApplicationCodeList.Codes.XMS);

			if (result == ZString.Empty && hasCustomsMessages)
			{
				result = Res.GetString("ad2a07a8-ed3e-4ed6-9124-80c7ccc13d97", "You cannot deactivate {0} since it has customs messages attached.", HumanReadableName);
			}
			else if (Consols.Count > 0)
			{
				result = Res.GetString("1b618131-945b-4ee5-920b-097bfd24d806", "You cannot deactivate {0} since it is attached to consolidation.", HumanReadableName);
			}
			else if (CoLoadShipments.Count > 0)
			{
				result = Res.GetString("be120b0f-f762-4afe-be7b-0213ea66ae3c", "You cannot deactivate {0} since it is a Master / Lead shipment with sub-shipments attached.", HumanReadableName);
			}

			return result;
		}

		public override string CanReactivate()
		{
			return null;
		}

		public override bool IsCancelledHasChanged
		{
			get { return JS_IsCancelledInfo.HasChanges; }
		}

		public override ZBool JS_IsCancelled
		{
			get { return base.JS_IsCancelled; }
			set
			{
				if (base.JS_IsCancelled != value)
				{
					base.JS_IsCancelled = value;
					UpdateReadOnlyForWhenCancelled();
					DocsAndCartage.MarkAsNeedingValidation();
				}
			}
		}

		void UpdateReadOnlyForWhenCancelled()
		{
			if (!IsDeleted)
			{
				SetReadOnlyIncludingChildren(JS_IsCancelled);
			}
		}

		#endregion

		#region ICDArchive Members

		public virtual CDArchiveInfo CDArchiveInfo
		{
			get { return new ShipmentCDArchiveInfo(this); }
		}

		#region ShipmentCDArchiveInfo

		public class ShipmentCDArchiveInfo : CDArchiveInfo
		{
			public ShipmentCDArchiveInfo(CommonShipment shipment)
				: base(shipment)
			{
			}

			protected CommonShipment Shipment
			{
				get { return (CommonShipment)BusinessEntity; }
			}

			public override ZString ConsigneeCode
			{
				get { return (Shipment.Consignee != null) ? Shipment.Consignee.OH_Code : ZString.Empty; }
			}

			public override ZString ConsignorCode
			{
				get { return (Shipment.Consignor != null) ? Shipment.Consignor.OH_Code : ZString.Empty; }
			}

			public override ZDateTime ETA
			{
				get { return Shipment.JS_E_ARV; }
			}

			public override ZDateTime ETD
			{
				get { return Shipment.JS_E_DEP; }
			}

			public override ZString Destination
			{
				get { return (Shipment.Destination != null) ? Shipment.Destination.RL_Code : ZString.Empty; }
			}

			public override ZString Origin
			{
				get { return (Shipment.Origin != null) ? Shipment.Origin.RL_Code : ZString.Empty; }
			}

			public override ZString HouseBill
			{
				get { return Shipment.JS_HouseBill; }
			}

			public override ZString Vessel
			{
				get { return (Shipment.Consols.Count > 0) ? Shipment.Consols[0].JK_JX_JV_NKVessel : ZString.Empty; }
			}

			public override ZString VoyageFlight
			{
				get { return (Shipment.Consols.Count > 0) ? Shipment.Consols[0].JK_JX_JV_VoyageFlight : ZString.Empty; }
			}

			public override ZString MasterBill
			{
				get { return (Shipment.Consols.Count > 0) ? Shipment.Consols[0].JK_MasterBillNum : ZString.Empty; }
			}

			public override ZString[] OrderNumbersList
			{
				get { return Array.Empty<ZString>(); }
			}

			public override ZString[] ContainerNumbersList
			{
				get { return Shipment.Containers.Select(x => x.JC_ContainerNum).ToArray(); }
			}

			public override ZString[] EntryNumbersList
			{
				get
				{
					return GetCombinedList(Shipment.Declarations, info => info.EntryNumbersList);
				}
			}

			public override ZString[] InvoiceNumbersList
			{
				get
				{
					AccTransactionHeaderCollection transactions = new InvoiceLoader(Factory).GetInvoicesForUniqueRef(new ZGuid[] { Shipment.ConsigneePK }, Shipment.JS_UniqueConsignRef);
					ZString[] headers = new ZString[transactions.Count];
					for (int i = 0; i < transactions.Count; i++)
					{
						headers[i] = transactions[i].AH_ConsolidatedInvoiceRef;
					}
					return headers;
				}
			}

			public override ZString JobNumber
			{
				get { return Shipment.JS_UniqueConsignRef; }
			}
		}

		#endregion

		#endregion

		#region ILocalShippingLineProvider Members

		OrgHeader ILocalShippingLineProvider.ShippingLine
		{
			get { return LocalConsol != null ? LocalConsol.ShippingLine : null; }
		}

		#endregion

		#region ITransportParent Members

		TransportSupporter ITransportParent.TransportSupporter
		{
			get { return new CommonShipmentTransportSupporter<CommonShipment>(this); }
		}

		ZString ITransportParentCommon.TypeCode
		{
			get { return Constants.TransportParentTypes.Shipment; }
		}

		#endregion

		#region ITransportChangeNotifier Members

		void ITransportChangeNotifier.NotifyChanged(TransportChangeNotifyType notifyType, Transport transport, IZType previousValue)
		{
			TransportChangeNotifyHandler handler;
			if (TransportChangeNotifierDictionary.TryGetValue(notifyType, out handler))
			{
				if (handler != null)
				{
					handler(transport, previousValue);
				}
			}
		}

		Dictionary<TransportChangeNotifyType, TransportChangeNotifyHandler> TransportChangeNotifierDictionary
		{
			get { return transportChangeNotifierDictionary ?? (transportChangeNotifierDictionary = new Dictionary<TransportChangeNotifyType, TransportChangeNotifyHandler>()); }
		}
		Dictionary<TransportChangeNotifyType, TransportChangeNotifyHandler> transportChangeNotifierDictionary;

		public void AddTransportChangeNotifier(TransportChangeNotifyType notifyType, TransportChangeNotifyHandler notifier)
		{
			TransportChangeNotifyHandler handler;
			if (TransportChangeNotifierDictionary.TryGetValue(notifyType, out handler))
			{
				handler -= notifier;
				handler += notifier;
			}
			else
			{
				TransportChangeNotifierDictionary.Add(notifyType, notifier);
			}
		}

		public void RemoveTransportChangeNotifier(TransportChangeNotifyType notifyType, TransportChangeNotifyHandler notifier)
		{
			TransportChangeNotifyHandler handler;
			if (TransportChangeNotifierDictionary.TryGetValue(notifyType, out handler))
			{
				handler -= notifier;
			}
		}

		#endregion

		#region IRoutingSupport Members

		public RoutingCollection TransportsIncludingRelated
		{
			get
			{
				if (transportsIncludingRelated == null)
				{
					transportsIncludingRelated = new ShipmentRoutingCollection(this);
					OnTransportsIncludingRelatedCreated();

					if (IsPropertyReadOnlyDueToPhase((NoResString)"Routing"))
					{
						transportsIncludingRelated.SetReadOnlyIncludingChildren(true);
					}
				}
				return transportsIncludingRelated;
			}
		}
		RoutingCollection transportsIncludingRelated;

		ZString IRoutingSupport.TransportMode
		{
			get { return JS_TransportMode; }
		}

		string IRoutingSupport.AdditionalETAUpdateMsg
		{
			get { return CommonConsol.AdditionalETAUpdateMsg; }
		}

		string IRoutingSupport.AdditionalETDUpdateMsg
		{
			get { return CommonConsol.AdditionalETDUpdateMsg; }
		}

		#endregion

		#region ICusEntryNumberSupporter Members

		CusEntryNumber GetCusEntryNumber(ZString numberType, RefCountry country, ZString category)
		{
			return Factory.LoadTop1<CusEntryNumber>(GetCusEntryNumberQuery(numberType, country.Code, category));
		}

		CusEntryNumber GetCusEntryNumber(ZString numberType, ZString number, RefCountry country, ZString category)
		{
			ZQuery query = GetCusEntryNumberQuery(numberType, country.Code, category);
			query.AddToFilter(CusEntryNumSchema.CE_EntryNum, number);
			return Factory.LoadTop1<CusEntryNumber>(query);
		}

		ZQuery GetCusEntryNumberQuery(ZString numberType, ZString country, ZString category)
		{
			ZQuery query = new ZQuery(CusEntryNumSchema.CE_EntryType, numberType);
			query.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, country);
			query.AddToFilter(CusEntryNumSchema.CE_ParentID, PK);
			query.AddToFilter(CusEntryNumSchema.CE_Category, category);
			query.AddToFilter(CusEntryNumSchema.CE_ParentTable, TableName);

			return query;
		}

		void CreateCusEntryNumber(ZString numberType, RefCountry country, ZString value, string category)
		{
			CusEntryNumber number;
			if (fNumbers == null)
			{
				number = Factory.New<CusEntryNumber>();
				number.Parent = this;
			}
			else
			{
				number = Numbers.AddNew();
			}
			number.CE_Category = category;
			number.CE_EntryIsSystemGenerated = category != CusEntryNumber.Categories.CustomsPermitClearanceNumber;
			number.CE_EntryNum = value;
			number.CE_EntryType = numberType;
			number.CE_RN_NKCountryCode = country.Code;
		}

		void ICusEntryNumberSupporter.CreateOrUpdate(ZString numberType, ZString countryCode, ZString value, INotifications notify)
		{
			if (!CusEntryNumberTypes.CountrySpecificCustomsEntryNumberTypeList(Factory, countryCode, this.IsImport()).ContainsCode(CMRExportExemptionCodes.Get4CharCode(numberType)))
			{
				notify.Notify(new WarningNotification(WarningType.Warning, Res.GetString("28aca9e3-2dc7-442e-a6d9-900aea3eb868", "No support for customs entry number type '{0}'", numberType)));
				return;
			}

			RefCountry country = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, countryCode);
			if (country != null)
			{
				CusEntryNumber number = GetCusEntryNumber(numberType, country, CusEntryNumber.Categories.CustomsPermitClearanceNumber);
				if (number != null)
				{
					number.CE_EntryNum = value;
				}
				else
				{
					ZQuery query = new ZQuery(CusEntryNumSchema.CE_RN_NKCountryCode, country.Code);
					query.AddToFilter(CusEntryNumSchema.CE_ParentID, PK);
					query.AddToFilter(CusEntryNumSchema.CE_Category, CusEntryNumber.Categories.CustomsPermitClearanceNumber);
					query.AddToFilter(CusEntryNumSchema.CE_ParentTable, TableName);
					CusEntryNumber[] numbers = Factory.Load<CusEntryNumber>(query);
					if (numbers != null && numbers.Length == 1)
					{
						numbers[0].CE_EntryNum = value;
						numbers[0].CE_EntryType = numberType;
					}
					else
					{
						CreateCusEntryNumber(numberType, country, value, CusEntryNumber.Categories.CustomsPermitClearanceNumber);
					}
				}
			}
		}

		#endregion

		#region IAdditionalReferenceNumberSupporter Members

		void IAdditionalReferenceNumberSupporter.CreateOrUpdate(ZString numberType, ZString countryCode, ZString value, INotifications notify)
		{
			if (numberType == CusEntryNumLookups.IMR)
			{
				JS_InterimReceipt = value;
			}
			else
			{
				CodeDescriptionPairList additionalReferenceNumberTypes = ((IAdditionalReferenceNumberTypeProvider)this).GetAdditionalReferenceNumberTypeList(CusEntryNumber.Categories.AdditionalReferenceNumber, countryCode);

				if (!additionalReferenceNumberTypes.ContainsCode(numberType))
				{
					notify.Notify(new WarningNotification(WarningType.Warning, Res.GetString("3447a9ce-d522-433b-a9fc-9bcc3e53cf02", "No support for additional reference number type '{0}'", numberType)));
					return;
				}

				RefCountry country = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, countryCode);

				if (country != null)
				{
					string[] nonUniqueEntryNumberTypes = additionalReferenceNumberTypes.NonUnique();

					CusEntryNumber number = nonUniqueEntryNumberTypes.Contains<string>(numberType) ?
						GetCusEntryNumber(numberType, value, country, CusEntryNumber.Categories.AdditionalReferenceNumber) :
						GetCusEntryNumber(numberType, country, CusEntryNumber.Categories.AdditionalReferenceNumber);

					if (number != null)
					{
						number.CE_EntryNum = value;
					}
					else
					{
						CreateCusEntryNumber(numberType, country, value, CusEntryNumber.Categories.AdditionalReferenceNumber);
					}
				}
			}
		}

		bool IAdditionalReferenceNumberSupporter.IncludeSpecialCustomsInstructionsItems
		{
			get { return false; }
		}

		void IAdditionalReferenceNumberSupporter.OnEntryNumChanged(CusEntryNumber additionalReferenceNumber)
		{
		}

		CusEntryNumAdditionalReferenceCollection IAdditionalReferenceNumberSupporter.AdditionalReferenceNumbers
		{
			get { return Numbers; }
		}

		void IAdditionalReferenceNumberSupporter.AdditionalEntryNumberValidation(ZPropertyInfo info, ZString type, ZString number)
		{
		}

		#endregion

		#region IAdditionalReferenceNumberTypeProvider Members

		CodeDescriptionPairList IAdditionalReferenceNumberTypeProvider.GetAdditionalReferenceNumberTypeList(ZString category, ZString countryCode)
		{
			return category.ToString() switch
			{
				CusEntryNumber.Categories.CustomsPermitClearanceNumber => CusEntryNumberTypes.CountrySpecificCustomsEntryNumberTypeList(Factory, countryCode, UseImportEntryTypeList),
				CusEntryNumber.Categories.AdditionalReferenceNumber => GetAdditionalReferenceNumberTypeListForOTH(),
				_ => Factory.GetCachedValue<CodeDescriptionPairList>(),
			};

			CodeDescriptionPairList GetAdditionalReferenceNumberTypeListForOTH()
			{
				var port = !JS_RL_NKDestination.IsEmpty ? JS_RL_NKDestination : ArrivalConsol?.JK_RL_NKDischargePort ?? ZString.Empty;
				var parameters = new AdditionalReferenceNumberTypesParameters(countryCode) { Parent = this, DischargeCountryCode = port.Left(2) };
				return Factory.GetCachedValue($"GetAdditionalReferenceNumberTypeList_CommonShipment_{parameters.Key}", () =>
				{
					var result = CusEntryNumLookups.GetAdditionalReferenceNumberTypes(parameters);
					result.AddPairsIfNotExist(new ShipmentNonCustomsAdditionalReferenceCodesCodeList().ToArray());
					return result;
				});
			}
		}

		#endregion

		#region Weight/Volume/Chargeable/Loading Meters for documents

		public ZDecimal GetWeightForDoc(string displayType)
		{
			return GetWeightWithScaleForDoc(displayType).Item1;
		}

		public Tuple<ZDecimal, ZByte> GetWeightWithScaleForDoc(string displayType)
		{
			return GetMeasureForDoc(displayType, JS_DocumentedWeight, JobShipmentSchema.JS_DocumentedWeight.Scale,
				JS_ManifestedWeight, JobShipmentSchema.JS_ManifestedWeight.Scale,
				JS_ActualWeight, JobShipmentSchema.JS_ActualWeight.Scale);
		}

		public ZDecimal GetVolumeForDoc(string displayType)
		{
			return GetVolumeWithScaleForDoc(displayType).Item1;
		}

		public Tuple<ZDecimal, ZByte> GetVolumeWithScaleForDoc(string displayType)
		{
			return GetMeasureForDoc(displayType, JS_DocumentedVolume, JobShipmentSchema.JS_DocumentedVolume.Scale,
				JS_ManifestedVolume, JobShipmentSchema.JS_ManifestedVolume.Scale,
				JS_ActualVolume, JobShipmentSchema.JS_ActualVolume.Scale);
		}

		public ZDecimal GetChargeableForDoc(string displayType)
		{
			return GetChargeableWithScaleForDoc(displayType).Item1;
		}

		public Tuple<ZDecimal, ZByte> GetChargeableWithScaleForDoc(string displayType)
		{
			return GetMeasureForDoc(displayType, JS_DocumentedChargeable, JobShipmentSchema.JS_DocumentedChargeable.Scale,
				JS_ManifestedChargeable, JobShipmentSchema.JS_ManifestedChargeable.Scale,
				JS_ActualChargeable, JobShipmentSchema.JS_ActualChargeable.Scale);
		}

		public ZDecimal GetLoadingMetersForDoc(string displayType)
		{
			return GetLoadingMetersWithScaleForDoc(displayType).Item1;
		}

		public Tuple<ZDecimal, ZByte> GetLoadingMetersWithScaleForDoc(string displayType)
		{
			return GetMeasureForDoc(displayType, JS_DocumentedLoadingMeters, JobShipmentSchema.JS_DocumentedLoadingMeters.Scale,
				JS_ManifestedLoadingMeters, JobShipmentSchema.JS_ManifestedLoadingMeters.Scale,
				JS_LoadingMeters, JobShipmentSchema.JS_LoadingMeters.Scale);
		}

		Tuple<ZDecimal, ZByte> GetMeasureForDoc(string displayType, ZDecimal documented, ZByte documentsScale, ZDecimal manifested, ZByte manifesteScale, ZDecimal actual, ZByte actualScale)
		{
			switch (displayType)
			{
				case Enterprise.Core.WeightAndVolumeDisplayTypes.Codes.Client:
					return new Tuple<ZDecimal, ZByte>(documented, documentsScale);
				case Enterprise.Core.WeightAndVolumeDisplayTypes.Codes.Carrier:
					return new Tuple<ZDecimal, ZByte>(manifested, manifesteScale);
				default:
					return new Tuple<ZDecimal, ZByte>(actual, actualScale);
			}
		}

		#endregion

		#region Invoicing

		#region JobHeader

		public ZGuid ShipmentJobHeaderPK
		{
			get => ShipmentJobHeader?.PK ?? ZGuid.Empty;
		}

		public ZPropertyInfo ShipmentJobHeaderPKInfo
		{
			get => GetZPropertyInfo(nameof(ShipmentJobHeaderPK));
		}

		[ImportInitializationMethodName("CreateShipmentJobHeaderWithMutex")]
		public JobHeader ShipmentJobHeader
		{
			get
			{
				if (shipmentJobHeader != null && !shipmentJobHeader.IsDeleted && GlbCompany.CurrentCompany.PK != shipmentJobHeader.JH_GC)
				{
					shipmentJobHeader.LocalChargesAddrChanged -= LocalChargesAddressChanged;
				}

				if (!IsShipmentJobHeaderValid)
				{
					var originalJobHeader = shipmentJobHeader;
					shipmentJobHeader = new JobHeader.Loader(this).Load(true, false);

					if (shipmentJobHeader != null)
					{
						RegisterEditableChildObject(shipmentJobHeader);
						shipmentJobHeader.LocalChargesAddrChanged += LocalChargesAddressChanged;
					}
				}

				return shipmentJobHeader;
			}
		}
		JobHeader shipmentJobHeader;

		JobHeader GetShipmentJobHeaderForWorkflow()
		{
			if (IsShipmentJobHeaderValid)
			{
				return shipmentJobHeader;
			}

			return new JobHeader.Loader(this).Load(true, false);
		}

		public string CreateShipmentJobHeaderWithMutex()
		{
			var errorMessage = "";
			if (!IsShipmentJobHeaderValid)
			{
				createShipmentJobHeaderWithMutexContext = string.Join(System.Environment.NewLine, Env.CurrentCompany.Code, System.Environment.StackTrace);
				var loader = new JobHeader.Loader(this);
				shipmentJobHeader = loader.TryLoadOrCreateWithMutex();
				if (shipmentJobHeader != null)
				{
					RegisterEditableChildObject(shipmentJobHeader);
					shipmentJobHeader.LocalChargesAddrChanged += LocalChargesAddressChanged;
				}
				else
				{
					errorMessage = loader.GetJobCreationError();
				}
			}

			return errorMessage;
		}

		protected virtual void LocalChargesAddressChanged(object sender, EventArgs e)
		{
			ShipmentJobHeaderPKInfo.RefreshBinding();
		}

		bool IsShipmentJobHeaderValid => shipmentJobHeader != null
			&& !shipmentJobHeader.IsDeleted
			&& shipmentJobHeader.JH_GC == GlbCompany.CurrentCompany.PK;

		string createShipmentJobHeaderWithMutexContext = string.Empty;

		#endregion

		#region IJobHeaderParent Members

		public void SetJobNumberFieldOnSaving()
		{
			PopulateBillAndShipmentNumberIfNeeded();
		}

		public virtual void OnJobCreating(JobHeader job)
		{
		}

		public virtual void OnJobCreated(JobHeader job)
		{
		}

		public virtual void OnJobDeleting(JobHeader job)
		{
			if (JobDeleting != null)
			{
				JobDeleting(this, EventArgs.Empty);
			}
		}

		public virtual void OnJobDeleted(JobHeader job)
		{
		}

		bool IJobHeaderParent.AllowInvoiceDeletion
		{
			get { return true; }
		}

		public event EventHandler JobDeleting;

		#endregion

		#region IJobInvoicingPlugIn Members

		public IJobInvoicingSupporter InvoicingSupporter
		{
			get { return invoicingSupporter ?? (invoicingSupporter = GetNewInvoicingSupporter()); }
		}
		CommonShipmentInvoicingSupporter invoicingSupporter;

		protected virtual CommonShipmentInvoicingSupporter GetNewInvoicingSupporter()
		{
			return new CommonShipmentInvoicingSupporter(this);
		}

		#endregion

		#region IJobInvoicingPlugInAdditionalJobs Members

		IJobInvoicingPlugIn[] IJobInvoicingPlugInAdditionalJobs.AdditionalJobsToShowChargesFor
		{
			get
			{
				var list = new List<IJobInvoicingPlugIn>();
				if (ShouldAddAdditionalJobsToShowChargesForBCN || ShouldAddAdditionalJobsToShowChargesForSCN)
				{
					list.AddRange((IJobInvoicingPlugIn[])CoLoadShipments.ToArray(typeof(IJobInvoicingPlugIn)));
				}

				list.AddRange(AdditionalJobsToShowChargesForCore);

				return list.ToArray();
			}
		}

		bool ShouldAddAdditionalJobsToShowChargesForBCN
			=> JS_PackingMode == Constants.ContainerModes.BuyersConsol
				&& Job?.LocalCharges != null
				&& (Job.LocalCharges.CompanyData.EffectiveBuyersConsolInvoicingStyle.ToString().In(Constants.ConsolInvoicingStyles.Master, Constants.ConsolInvoicingStyles.ApportionInvoiceMaster));

		bool ShouldAddAdditionalJobsToShowChargesForSCN
			=> JS_PackingMode == Constants.ContainerModes.ShippersConsol
				&& Job?.LocalCharges != null
				&& (Job.LocalCharges.CompanyData.EffectiveShippersConsolInvoicingStyle.ToString().In(Constants.ConsolInvoicingStyles.Master, Constants.ConsolInvoicingStyles.ApportionInvoiceMaster));

		protected virtual IJobInvoicingPlugIn[] AdditionalJobsToShowChargesForCore
		{
			get { return Array.Empty<IJobInvoicingPlugIn>(); }
		}

		#endregion

		#endregion

		#region Rating/Invoicing

		public event OnlyShipmentInConsolEventHandler OnlyShipmentInConsol;
		public delegate void OnlyShipmentInConsolEventHandler(OnlyShipmentInConsolEventArgs args);

		public class OnlyShipmentInConsolEventArgs : CancelEventArgs
		{
			public OnlyShipmentInConsolEventArgs(ZString message)
			{
				this.Message = message;
			}

			public ZString Message;
		}

		internal OnlyShipmentInConsolEventArgs OnOnlyShipmentInConsol(ZString message)
		{
			var args = new OnlyShipmentInConsolEventArgs(message);

			if (OnlyShipmentInConsol != null)
			{
				OnlyShipmentInConsol(args);
			}

			return args;
		}

		public IAutoRating RatingAdapter
		{
			get { return ratingAdapter ?? (ratingAdapter = GetRatingAdapterCore()); }
		}
		IAutoRating ratingAdapter;

		protected virtual IAutoRating GetRatingAdapterCore()
		{
			return new ShipmentRatingAdapter<CommonShipment>(this);
		}

		#endregion

		#region Document Support

		#region New Properties

		#region ImportPickUpAddress

		public OrgAddress ImportPickUpAddress
		{
			get
			{
				OrgAddress address = null;
				CommonConsol relevantConsol = this.IsExport() ? DepartureConsol : ArrivalConsol;

				if (ImportReleaseDepot != null)
				{
					address = ImportReleaseDepot;
				}
				else if (relevantConsol != null)
				{
					if (PackingMode == Core.Constants.ContainerModes.FCL ||
						PackingMode == Core.Constants.ContainerModes.BreakBulk ||
						PackingMode == Core.Constants.ContainerModes.RollOnRollOff ||
						(relevantConsol.JK_ConsolMode == Core.Constants.ContainerModes.BuyersConsol && TransportMode == Core.Constants.TransportModes.Sea))
					{
						address = relevantConsol.ArrivalCTOAddress;
					}
					else
					{
						address = relevantConsol.UnpackDepotAddress;
					}
				}

				return address;
			}
		}

		#endregion

		#region DepotOrCTOHeading

		public ZString DepotOrCTOHeading
		{
			get
			{
				return (PackingMode == Core.Constants.ContainerModes.FCL) ? "CTO" : "DEPOT";
			}
		}

		#endregion

		public ZString TransportMode
		{
			get { return JS_TransportMode; }
		}

		public ZString PackingMode
		{
			get { return JS_PackingMode; }
		}

		#region IFlightDetailsSuppression Members

		public virtual ZBool HasETDPassed
		{
			get { return (DepartureConsolForDocuments != null && DepartureConsolForDocuments.HasETDPassed); }
		}

		public ZBool HasActualRCVPassed
		{
			get { return (!DocsAndCartage.JP_PickupCartageCompleted.IsEmpty && ZDateTime.Now > DocsAndCartage.JP_PickupCartageCompleted); }
		}

		public ZBool HasFinalRoutingLegATDPassed
		{
			get
			{
				Transport lastLeg = new TransportOrderHelper(TransportsIncludingRelated).LastLegWithTransportMode(Constants.TransportModes.Air, null, null);
				if (lastLeg == null)
				{
					return true;
				}
				return (!lastLeg.JW_ATD.IsEmpty && ZDateTime.Now > lastLeg.JW_ATD);
			}
		}

		public ZBool IsPassengerFlight
		{
			get
			{
				Transport lastLeg = new TransportOrderHelper(TransportsIncludingRelated).LastLegWithTransportMode(Constants.TransportModes.Air, null, null);
				if (lastLeg == null)
				{
					return true;
				}
				return (!lastLeg.JW_IsCargoOnly);
			}
		}

		#endregion

		#region OutturnComments

		public ZString OutturnComments
		{
			get
			{
				ZString comments = "";

				foreach (PackLine line in InnerPackLines)
				{
					if (line.JL_OutturnComment != "")
					{
						comments += (comments == "" ? "" : ", ") + line.JL_OutturnComment;
					}
				}

				return comments;
			}
		}

		#endregion

		public ZString ClientsWithCreditOnHold
		{
			get
			{
				var result = new ZStringBuilder();

				foreach (var org in ((ICreditControlledDocumentDelivery)this).OrganisationsForCreditChecks)
				{
					if (org.CreditChecker.IsCreditOnHold())
					{
						result.Append(org.OH_FullNameTruncated);
					}
				}

				return result.ToStringWithDelimiterBetweenAppends(" " + Res.GetString("fe018f35-edcf-4f19-abde-77d372213ab5", "and") + " ");
			}
		}

		public bool RequiresContainerCartageAdvice(string docDirection)
		{
			return (docDirection == nameof(DocumentDirection.ARV) &&
				(JS_PackingMode == Core.Constants.ContainerModes.FCL || JS_PackingMode == Core.Constants.ContainerModes.BuyersConsol))
				|| (docDirection == nameof(DocumentDirection.DEP) && JS_PackingMode == Core.Constants.ContainerModes.FCL);
		}

		public DebtorToSelectFromForPrintingCollection Debtors
		{
			get
			{
				OrgHeaderCollection debtors = new OrgHeaderCollection(Factory);

				if (ShipmentJobHeader != null)
				{
					ZQuery findChargesForJobQuery = new ZQuery(JobChargeSchema.JR_JH, ShipmentJobHeader.PK);
					var charges = Factory.Load<JobCharge>(findChargesForJobQuery);

					foreach (JobCharge charge in charges)
					{
						if (charge.SellAccount != null && !debtors.Contains(charge.SellAccount))
						{
							debtors.Add(charge.SellAccount);
						}
					}
				}
				DebtorToSelectFromForPrintingCollection result = new DebtorToSelectFromForPrintingCollection(debtors);
				return result;
			}
		}

		#region ExportStatement

		public ZString ExportStatement => ExportStatementCore("MM/dd/yyyy", includeStatementCode: true);

		public ZString ExportStatement_CargoIMP => ExportStatementCore("yyyyMMdd", includeStatementCode: false);

		ZString ExportStatementCore(string dateFormat, bool includeStatementCode)
		{
			ZString result = ZString.Empty;
			if (this.IsExport() || this.IsCrossTrade())
			{
				ExportStatementSetting exportStatementSetting = this.ExportStatementSetting;
				if (exportStatementSetting != null)
				{
					IExportStatement declarationForDocuments = this.DeclarationForDocuments as IExportStatement;
					if (declarationForDocuments != null && GetEntryHeadersForDeclarations(new BusinessObject[] { (BusinessObject)this.DeclarationForDocuments }).Length > 0)
					{
						result = declarationForDocuments.GetExportStatement(exportStatementSetting);
					}

					if (result.IsEmpty)
					{
						var exportStatementCreator = new ShipmentExportStatementCreator(this, exportStatementSetting, dateFormat);
						result = includeStatementCode ? exportStatementCreator.ExportStatement : exportStatementCreator.ExportStatementFields;
					}
				}
			}
			return result;
		}

		public ExportStatementSetting ExportStatementSetting
		{
			get
			{
				ExportStatementSetting result = null;
				CountryExportStatementSetting countrySetting = FreightDataRegistry.Instance.ExportStatementSettings.Value[JS_RL_NKOrigin.Left(2)];
				if (countrySetting != null)
				{
					result = countrySetting.Statements[DocsAndCartage.JP_ExportStatement];
				}
				return result;
			}
		}
		#endregion

		#region PackLocations

		public List<PackLocationForDocument> PackLocations
		{
			get { return new PackLocationForDocumentCollection(this, Factory).PackLocations; }
		}

		#endregion

		#region IsReceived

		public ZPropertyInfo IsReceivedInfo
		{
			get { return GetZPropertyInfo(Schema.IsReceived); }
		}

		public ZBool IsReceived
		{
			get { return (!JS_A_RCV.IsEmpty || !JS_InterimReceipt.IsEmpty); }
		}

		#endregion

		#region IsLoose

		public bool IsLoose
		{
			get
			{
				return JS_PackingMode == Constants.ContainerModes.LCL
					|| JS_PackingMode == Constants.ContainerModes.BreakBulk
					|| JS_PackingMode == Constants.ContainerModes.Bulk
					|| JS_PackingMode == Constants.ContainerModes.Liquid
					|| JS_PackingMode == Constants.ContainerModes.RollOnRollOff
					|| JS_PackingMode == Constants.ContainerModes.NonContainerised
					|| JS_PackingMode == Constants.ContainerModes.FTL
					|| JS_PackingMode == Constants.ContainerModes.LTL;
			}
		}

		#endregion

		public bool UseImportEntryTypeList => !(CusEntryNumberTypes.IsUSCustomsCountryNeededEXPEntryType(JS_RL_NKOrigin.Left(2)) && CusEntryNumberTypes.IsUSCustomsCountryNeededEXPEntryType(JS_RL_NKDestination.Left(2))) && (bool)this.IsImport();

		#region IsArrivalContainerModeFCLorULD

		public bool IsArrivalContainerModeFCLorULD
		{
			get
			{
				return Constants.ContainerModes.IsFCLType(JS_PackingMode) || ArrivalConsol != null && ArrivalConsol.IsBuyersConsol;
			}
		}

		#endregion

		#region IsContainerised

		public bool IsContainerised
		{
			get
			{
				return JS_PackingMode == Constants.ContainerModes.FCL
					|| JS_PackingMode == Constants.ContainerModes.FCLMixedShipper
					|| JS_PackingMode == Constants.ContainerModes.BuyersConsol
					|| JS_PackingMode == Constants.ContainerModes.Containerised
					|| JS_PackingMode == Constants.ContainerModes.Combination;
			}
		}

		#endregion

		#endregion

		#region Arrival and Departure Consol For Documents

		public CommonConsol ArrivalConsolForDocuments
		{
			get
			{
				if (Consols.Count == 1)
				{
					return Consols[0];
				}
				else
				{
					CommonConsol arrivalConsol = null;
					var candidateConsols = CandidateConsolsForDepartureArrival.ToArray();

					foreach (CommonConsol currentConsol in candidateConsols)
					{
						arrivalConsol = currentConsol;
						foreach (CommonConsol consol in candidateConsols)
						{
							if (arrivalConsol.JK_RL_NKDischargePort == consol.JK_RL_NKLoadPort)
							{
								arrivalConsol = null;
								break;
							}
						}
						if (arrivalConsol != null)
						{
							break;
						}
					}
					return arrivalConsol;
				}
			}
		}

		public CommonConsol DepartureConsolForDocuments
		{
			get
			{
				if (Consols.Count == 1)
				{
					return Consols[0];
				}
				else
				{
					CommonConsol departureConsol = null;
					var candidateConsols = CandidateConsolsForDepartureArrival.ToArray();

					foreach (CommonConsol currentConsol in candidateConsols)
					{
						departureConsol = currentConsol;
						foreach (CommonConsol consol in candidateConsols)
						{
							if (departureConsol.JK_RL_NKLoadPort == consol.JK_RL_NKDischargePort)
							{
								departureConsol = null;
								break;
							}
						}
						if (departureConsol != null)
						{
							break;
						}
					}
					return departureConsol;
				}
			}
		}

		#endregion

		#region IsBrokerage

		public BusinessObject GetDeclarationFor(ZGuid companyPK)
		{
			BusinessObject result = null;
			if (CanHaveDeclarations)
			{
				var company = Factory.Load<GlbCompany>(companyPK);
				if (company != null)
				{
					var decQuery = new ZQuery(JobDeclarationSchema.JE_JS, PK);
					decQuery.AddToFilter(JobDeclarationSchema.JE_GB, company.Branches.GetPKs());
					decQuery.FetchOnlyFromLocalCache = !IsInDatabase;
					if (JS_IsCancelled)
					{
						decQuery.IgnoreActiveFilter = true;
					}
					decQuery.OrderBy = JobDeclarationSchema.JE_SystemCreateTimeUtc.Name;
					result = (BusinessObject)Factory.LoadTop1<IBaseJobDeclaration>(decQuery);
				}
			}

			return result;
		}

		public
#if DEBUG
 virtual
#endif
		IBaseJobDeclaration DeclarationForDocuments
		{
			get
			{
				if (!useDeclarationForDocuments)
				{
					return null;
				}

				if (fDeclarationForDocuments == null || !IsTargetBranchCompanyEqualToCurrentCompany(fDeclarationForDocuments.JE_GB))
				{
					fDeclarationForDocuments = Declarations.FirstOrDefault(dec => dec is IDocumentSupportable && IsTargetBranchCompanyEqualToCurrentCompany(dec.JE_GB));
					if (fDeclarationForDocuments is IJobDeclarationWithShipmentSynchonisation declarationWithShipmentSynchonisation && declarationWithShipmentSynchonisation.ShouldSynchroniseWithShipmentForDocument)
					{
						declarationWithShipmentSynchonisation.SynchroniseWithShipmentIfNeeded();
					}
				}

				return fDeclarationForDocuments;
			}
		}
		IBaseJobDeclaration fDeclarationForDocuments;

		public ZString DecForDocMessageType
		{
			get { return (IsBrokerageAndDeclarationExists) ? DeclarationForDocuments["MessageTypeForDocumentFilter"].ToString() : ""; }
		}

		public ZString DecForDocMessageTypes
		{
			get { return (IsBrokerageAndDeclarationExists) ? DeclarationForDocuments["MessageTypesForDocumentFilter"].ToString() : ""; }
		}

		public ZString CurrentCountryCode
		{
			get { return GlbCompany.CurrentCompany.GC_RN_NKCountryCode; }
		}

		public ZString BrokerageCountryCode => Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(CurrentCountryCode);

		internal void ResetDeclarationForDocuments()
		{
			fDeclarationForDocuments = null;
		}

		public IDisposable SuspendDeclarationForDocuments()
		{
			return new DeclarationForDocumentsSuspender(this);
		}

		bool useDeclarationForDocuments = true;

		sealed class DeclarationForDocumentsSuspender : IDisposable
		{
			public DeclarationForDocumentsSuspender(CommonShipment shipment)
			{
				this.shipment = shipment;

				shipment.useDeclarationForDocuments = false;
			}

			readonly CommonShipment shipment;

			public void Dispose()
			{
				shipment.useDeclarationForDocuments = true;
			}
		}

		internal ZBool IsBrokerageAndDeclarationExists
		{
			get
			{
				ZBool countryIsValid = GlbCompany.CurrentCompany.GC_RN_NKCountryCode != Core.Constants.CountryCodes.Singapore && GlbCompany.CurrentCompany.GC_RN_NKCountryCode != Core.Constants.CountryCodes.Malaysia;

				return countryIsValid && DeclarationForDocuments != null;
			}
		}

		[ResolveTypeFromObjectFactoryForDocData]
		public INctsHeader NctsHeaderForDocuments
		{
			get
			{
				if (nctsHeaderForDocuments == null || !IsTargetBranchCompanyEqualToCurrentCompany(nctsHeaderForDocuments.BH_GB))
				{
					var linkedNctsHeaders = LoadNctsHeaders(Factory, !IsDeleted && !JS_IsCancelled);
					nctsHeaderForDocuments = linkedNctsHeaders.FirstOrDefault(nctsHeader => nctsHeader is IDocumentSupportable && IsTargetBranchCompanyEqualToCurrentCompany(nctsHeader.BH_GB));
				}

				return nctsHeaderForDocuments;
			}
		}
		INctsHeader nctsHeaderForDocuments;

		INctsHeader[] LoadNctsHeaders(BusinessObjectFactory factory, bool activeOnly)
		{
			var query = new ZQuery(CusInBondHeaderSchema.BH_ParentID, PK);
			query.AddToFilter(CusInBondHeaderSchema.BH_ApplicationCode, new[] { CusInBondApplicationCodeList.Codes.NCTS4, CusInBondApplicationCodeList.Codes.NCTS5 });
			query.FetchOnlyFromLocalCache = !IsInDatabase;
			query.IgnoreActiveFilter = !activeOnly;
			return factory.Load<INctsHeader>(query);
		}

		bool IsTargetBranchCompanyEqualToCurrentCompany(ZGuid branchPk)
		{
			var branch = Factory.Load<GlbBranch>(branchPk);
			return branch != null && branch.GB_GC == GlbCompany.CurrentCompany.PK;
		}

		#endregion

		#region Address Override Security

		protected SecurityCheckpoint GetCanOverrideAddressCheckpoint(JobDocAddress docAddress)
		{
			SecurityCheckpoint result = DefaultGetCanOverrideAddressCheckpoint;

			if (docAddress.E2_AddressType == DocAddressTypes.Codes.ConsigneeDocumentaryAddress)
			{
				result = ConsigneeAddressOverrideCheckpoint;
			}
			else if (docAddress.E2_AddressType == DocAddressTypes.Codes.ConsignorDocumentaryAddress)
			{
				result = ConsignorAddressOverrideCheckpoint;
			}

			return result;
		}

		protected virtual SecurityCheckpoint DefaultGetCanOverrideAddressCheckpoint
		{
			get { return Env.Security.None; }
		}

		protected virtual SecurityCheckpoint ConsigneeAddressOverrideCheckpoint
		{
			get { return Env.Security.None; }
		}

		protected virtual SecurityCheckpoint ConsignorAddressOverrideCheckpoint
		{
			get { return Env.Security.None; }
		}

		#endregion

		#region UpdatingShipmentFromPackLines

		public event CancelEventHandler UpdateShipmentTotalsPackQuantityVariation;

		public void CheckTotalsDiffer(bool requireCancelEventHandler = true)
		{
			if ((!requireCancelEventHandler || UpdateShipmentTotalsPackQuantityVariation != null) && ShipmentTotalsDiffer)
			{
				var e = new CancelEventArgs();
				UpdateShipmentTotalsPackQuantityVariation?.Invoke(this, e);
				if (!e.Cancel)
				{
					UpdateShipmentTotals();
				}
			}
		}

		protected virtual bool ShipmentTotalsDiffer
		{
			get { return OuterPacksTotalsDifferAndCanBeUpdated || InnerPacksTotalsDiffer; }
		}

		public bool OuterPacksTotalsDifferAndCanBeUpdated
		{
			get { return CanHaveOwnPackLines && OuterPacksTotalsDiffer(); }
		}

		public bool InnerPacksTotalsDifferAndCanBeUpdated
		{
			get { return InnerPacksTotalsDiffer; }
		}

		protected virtual void UpdateShipmentTotals()
		{
			UpdateShipmentFromOuterPackLines();
		}

		public void UpdateShipmentFromOuterPackLines()
		{
			IsUpdatingShipmentFromPackLines = true;
			try
			{
				JS_OuterPacks = OuterPackLines.TotalPackages;
				JS_F3_NKPackType = OuterPackLines.TotalPackagesUnit;

				var weight = TotalOuterPacksWeight;
				var weightUnit = ShipmentWeightUnit;
				var volume = TotalOuterPacksVolume;
				var volumeUnit = ShipmentVolumeUnit;

				new WeightConversionStrategy().ReScale(ref weight, ref weightUnit, 9, 3);
				new VolumeConversionStrategy().ReScale(ref volume, ref volumeUnit, 9, 3);

				JS_ActualWeight = weight;
				JS_UnitOfWeight = weightUnit;
				JS_ActualVolume = volume;
				JS_UnitOfVolume = volumeUnit;
				JS_LoadingMeters = TotalOuterPacksLoadingMeters;
			}
			finally
			{
				IsUpdatingShipmentFromPackLines = false;
			}
		}

		protected ZBool OuterPacksTotalsDiffer()
		{
			return OuterPackLines.TotalPackages != JS_OuterPacks
				|| OuterPackLines.TotalPackagesUnit != JS_F3_NKPackType
				|| Utilities.Round(TotalOuterPacksWeight, 3) != Utilities.Round(JS_ActualWeightReadOnly, 3)
				|| Utilities.Round(TotalOuterPacksVolume, 3) != Utilities.Round(JS_ActualVolumeReadOnly, 3)
				|| (IsRoadLoadingMetersEnabled && Utilities.Round(TotalOuterPacksLoadingMeters, 3) != Utilities.Round(JS_LoadingMeters, 3));
		}

		protected virtual bool InnerPacksTotalsDiffer => false;

		#endregion

		#region BOL Printing

		public virtual ZBool AWBOrHBLPrintingShouldBeConfirmed()
		{
			ZDecimal maximumTotalForHarmonised = GetMaximumTotalForHarmonisedPackLines();

			ZBool relatedCustomsBrokerIsOrgProxy = false;

			if (Consignor != null)
			{
				OrgHeader relatedCustomsAgentBroker = Consignor.GetRelatedParty(RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Pickup, JS_TransportMode, JS_PackingMode);
				if (relatedCustomsAgentBroker != null)
				{
					relatedCustomsBrokerIsOrgProxy = relatedCustomsAgentBroker.IsProxyOrg(GlbCompany.CurrentCompany) || relatedCustomsAgentBroker.IsProxyOrg(GlbBranch.CurrentBranch.Company);
				}
			}

			return GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Constants.CountryCodes.UnitedStates
				&& CustomsEntryNumber.IsEmpty
				&& maximumTotalForHarmonised > (decimal)ObjectFactory.Get<US.IUSCustomsDataRegistry>().ShipmentHTSMaximumValue.Value
				&& (!(bool)ObjectFactory.Get<US.IUSCustomsDataRegistry>().ShipmentAuditShouldCheckCompany.Value || relatedCustomsBrokerIsOrgProxy);
		}

		ZDecimal GetMaximumTotalForHarmonisedPackLines()
		{
			ZDecimal result = 0;
			Dictionary<ZString, ZDecimal> harmonisedTotals = new Dictionary<ZString, ZDecimal>();
			foreach (PackLine packLine in OuterPackLines)
			{
				if (harmonisedTotals.ContainsKey(packLine.JL_HarmonisedCode))
				{
					harmonisedTotals[packLine.JL_HarmonisedCode] += packLine.JL_LinePrice;
				}
				else
				{
					harmonisedTotals.Add(packLine.JL_HarmonisedCode, packLine.JL_LinePrice);
				}
			}
			foreach (ZString harmonCode in harmonisedTotals.Keys)
			{
				if (harmonisedTotals[harmonCode] > result)
				{
					result = harmonisedTotals[harmonCode];
				}
			}
			return result;
		}

		#endregion

		#region PickupDeliveryConfirms - Loose

		[ChildEditable()]
		public CommonPickupDeliveryConfirmCollection PickupConfirms
		{
			get
			{
				if (pickupConfirms == null)
				{
					pickupConfirms = GetNewCommonPickupDeliveryConfirmCollection(Constants.PickupDeliveryConfirmTypes.OriginPickup);

					ConfirmationPackLineConcurrencyCheck.Register(Factory);
					RegisterEditableChildObject(pickupConfirms, "PickupConfirms");
				}
				return pickupConfirms;
			}
		}
		CommonPickupDeliveryConfirmCollection pickupConfirms;

		[ChildEditable()]
		public CommonPickupDeliveryConfirmCollection DeliveryConfirms
		{
			get
			{
				if (deliveryConfirms == null)
				{
					deliveryConfirms = GetNewCommonPickupDeliveryConfirmCollection(Constants.PickupDeliveryConfirmTypes.DestinationDelivery);

					ConfirmationPackLineConcurrencyCheck.Register(Factory);
					RegisterEditableChildObject(deliveryConfirms, "DeliveryConfirms");
				}
				return deliveryConfirms;
			}
		}
		CommonPickupDeliveryConfirmCollection deliveryConfirms;

		[ChildEditable()]
		public CommonPickupDeliveryConfirmCollection OriginCFSArrivals
		{
			get
			{
				if (originCFSArrival == null)
				{
					originCFSArrival = GetNewCommonPickupDeliveryConfirmCollection(Constants.PickupDeliveryConfirmTypes.OriginCFSArrival);
					RegisterEditableChildObject(originCFSArrival);
				}
				return originCFSArrival;
			}
		}
		CommonPickupDeliveryConfirmCollection originCFSArrival;

		[ChildEditable()]
		public CommonPickupDeliveryConfirmCollection OriginCFSDepartures
		{
			get
			{
				if (originCFSDeparture == null)
				{
					originCFSDeparture = GetNewCommonPickupDeliveryConfirmCollection(Constants.PickupDeliveryConfirmTypes.OriginCFSDeparture);
					RegisterEditableChildObject(originCFSDeparture);
				}
				return originCFSDeparture;
			}
		}
		CommonPickupDeliveryConfirmCollection originCFSDeparture;

		[ChildEditable()]
		public CommonPickupDeliveryConfirmCollection DestinationCFSArrivals
		{
			get
			{
				if (destinationCFSArrival == null)
				{
					destinationCFSArrival = GetNewCommonPickupDeliveryConfirmCollection(Constants.PickupDeliveryConfirmTypes.DestinationCFSArrival);
					RegisterEditableChildObject(destinationCFSArrival);
				}
				return destinationCFSArrival;
			}
		}
		CommonPickupDeliveryConfirmCollection destinationCFSArrival;

		[ChildEditable()]
		public CommonPickupDeliveryConfirmCollection DestinationCFSDepartures
		{
			get
			{
				if (destinationCFSDeparture == null)
				{
					destinationCFSDeparture = GetNewCommonPickupDeliveryConfirmCollection(Constants.PickupDeliveryConfirmTypes.DestinationCFSDeparture);
					RegisterEditableChildObject(destinationCFSDeparture);
				}
				return destinationCFSDeparture;
			}
		}
		CommonPickupDeliveryConfirmCollection destinationCFSDeparture;

		protected CommonPickupDeliveryConfirmCollection GetNewCommonPickupDeliveryConfirmCollection(string pickupDeliveryType)
		{
			return new CommonPickupDeliveryConfirmCollection(Factory, this, pickupDeliveryType);
		}

		#region RefreshDeliveryBinding

		public virtual void RefreshDeliveryBinding()
		{
		}

		#endregion

		#endregion

		#region IDocumentSupportable Members

		public virtual DocumentSupporter DocumentSupporter
		{
			get { return new CommonShipmentDocumentSupporter(this); }
		}

		public DocumentWrapper[] GetWrappersForARInvoice(OrgHeader orgHeaderForInvoices)
		{
			return GetWrappersForARInvoice(orgHeaderForInvoices, false);
		}

		public DocumentWrapper[] GetWrappersForARInvoice(OrgHeader orgHeaderForInvoices, bool useDocBuilderInvoice)
		{
			var orgs = new List<ZGuid>();
			if (orgHeaderForInvoices != null)
			{
				orgs.Add(orgHeaderForInvoices.PK);

				var billToOrg = orgHeaderForInvoices.GetFreightBillTo(false, TransportMode, PackingMode);
				if (billToOrg != null)
				{
					orgs.Add(billToOrg.PK);
				}
			}

			return new InvoiceLoader(Factory).GetWrappersForARInvoice(orgs, JS_UniqueConsignRef, useDocBuilderInvoice);
		}

		#endregion

		#region IEDocsProvider Members

		EDocsProviderSupporter IEDocsProvider.GetEDocsProviderSupporter()
		{
			return new JobInvoicingEDocsProviderSupporter(this);
		}

		#endregion

		#region IContainerLegParent Members

		public JobDocAddress GetConsigneeDocAddress
		{
			get { return ConsigneeDocumentaryAddress; }
		}

		public JobDocAddress GetConsigneeDeliveryDocAddress
		{
			get { return ConsigneeDeliveryAddress; }
		}

		public JobDocAddress GetConsignorDocAddress
		{
			get { return ConsignorDocumentaryAddress; }
		}

		public JobDocAddress GetConsignorPickupDocAddress
		{
			get { return ConsignorPickupAddress; }
		}

		public virtual JobDocAddress GetArrivalCFSDocAddress
		{
			get
			{
				ZGuid arrivalCFSAddressPK = DeliveryDepotAddress;
				DocAddressType addressType = DocAddressType.ArrivalCFSAddress;

				return JobDocAddress.GetOrCreateNonPersistantDocAddress(this, addressType, arrivalCFSAddressPK);
			}
		}

		public JobDocAddress GetArrivalCTODocAddress
		{
			get
			{
				ZGuid arrivalCTOAddressPK = DeliveryCTOAddress;
				DocAddressType addressType = DocAddressType.ArrivalCTOAddress;

				return JobDocAddress.GetOrCreateNonPersistantDocAddress(this, addressType, arrivalCTOAddressPK);
			}
		}

		public JobDocAddress GetArrivalContainerYardDocAddress
		{
			get
			{
				ZGuid arrivalCYDAddressPK = DeliveryContainerYardAddress;
				DocAddressType addressType = DocAddressType.ArrivalCYDAddress;

				return JobDocAddress.GetOrCreateNonPersistantDocAddress(this, addressType, arrivalCYDAddressPK);
			}
		}

		public virtual JobDocAddress GetDepartureCFSDocAddress
		{
			get
			{
				ZGuid departureCFSAddressPK = PickupDepotAddress;
				DocAddressType addressType = DocAddressType.DepartureCFSAddress;

				return JobDocAddress.GetOrCreateNonPersistantDocAddress(this, addressType, departureCFSAddressPK);
			}
		}

		public JobDocAddress GetDepartureCTODocAddress
		{
			get
			{
				ZGuid departureCTOAddressPK = PickupCTOAddress;
				DocAddressType addressType = DocAddressType.DepartureCTOAddress;

				return JobDocAddress.GetOrCreateNonPersistantDocAddress(this, addressType, departureCTOAddressPK);
			}
		}

		public JobDocAddress GetDepartureContainerYardDocAddress
		{
			get
			{
				ZGuid departureCYDAddressPK = PickupContainerYardAddress;
				DocAddressType addressType = DocAddressType.DepartureCYDAddress;

				return JobDocAddress.GetOrCreateNonPersistantDocAddress(this, addressType, departureCYDAddressPK);
			}
		}

		#endregion

		#region IRelatedJobNumber Members

		string[] IRelatedJobNumber.JobNumber
		{
			get { return new string[] { JS_UniqueConsignRef }; }
		}

		#endregion

		#region ICreditControlledDocumentDelivery Members

		event EventHandler<SecurityLoginEventArgs> ICreditControlledDocumentDelivery.GetDocumentLogin
		{
			add
			{
				fGetDocumentLogin += value;

				if (DeclarationForDocuments is ICreditControlledDocumentDelivery declarationDocumentDelivery)
				{
					declarationDocumentDelivery.GetDocumentLogin += value;
				}
			}
			remove
			{
				fGetDocumentLogin -= value;

				if (DeclarationForDocuments is ICreditControlledDocumentDelivery declarationDocumentDelivery)
				{
					declarationDocumentDelivery.GetDocumentLogin -= value;
				}
			}
		}
		event EventHandler<SecurityLoginEventArgs> fGetDocumentLogin;

		CustomMessageBoxCallback ICreditControlledDocumentDelivery.DocumentLoginMessageBoxCallback { get; set; }

		bool ICreditControlledDocumentDelivery.IsDPSFreightMovementRestricted
		{
			get
			{
				return IsDPSFreightMovementRestrictedCore();
			}
		}

		bool ICreditControlledDocumentDelivery.IsAviationSecurityFreightMovementRestricted
		{
			get
			{
				return IsAviationSecurityFreightMovementRestrictedCore();
			}
		}

		protected virtual bool IsAviationSecurityFreightMovementRestrictedCore()
		{
			return false;
		}

		ScreeningParty[] ICreditControlledDocumentDelivery.GetScreeningParties()
		{
			return GetScreeningPartiesCore();
		}

		OrgHeader[] ICreditControlledDocumentDelivery.OrganisationsForCreditChecks
		{
			get
			{
				return GetCreditCheckEnabledOrganisations();
			}
		}

		string ICreditControlledDocumentDelivery.DescriptionOfOrganisationBeingCheckedForCredit
		{
			get { return Res.GetString("c7f0ff08-39b9-4464-9199-ba1f6b2d5d3b", "Consignee, Consignor, Local Client or any Debtors for charges on the Billing tab"); }
		}

		void ICreditControlledDocumentDelivery.RaiseOnGetDocumentLogin(SecurityLoginEventArgs e)
		{
			if (fGetDocumentLogin != null)
			{
				fGetDocumentLogin(this, e);
			}
		}

		OrgHeader[] GetCreditCheckEnabledOrganisations()
		{
			var result = new List<OrgHeader>();

			if (Consignor != null && this.IsCreditLimitCheckRequired(OrgCodes.Consignor))
			{
				result.Add(Consignor);
			}
			if (Consignee != null && this.IsCreditLimitCheckRequired(OrgCodes.Consignee))
			{
				result.Add(Consignee);
			}
			if (this.IsCreditLimitCheckRequired(OrgCodes.LocalClient) && ShipmentJobHeader != null && ShipmentJobHeader.LocalCharges != null)
			{
				result.Add(ShipmentJobHeader.LocalCharges);
			}
			if (this.IsCreditLimitCheckRequired(OrgCodes.AllDebtors) && this.Debtors.Any())
			{
				result.AddRange(Debtors.Cast<DebtorToSelectFromForPrinting>().Select(x => x.Debtor));
			}
			if (AccountingMasterFilesRegistry.Instance.EnableCreditControlEvaluationForControllingCustomersAndControllingAgents.Value)
			{
				if (ControllingAgent != null && this.IsCreditLimitCheckRequired(OrgCodes.ControllingAgent))
				{
					result.Add(ControllingAgent);
				}
				if (ControllingCustomer != null && this.IsCreditLimitCheckRequired(OrgCodes.ControllingCustomer))
				{
					result.Add(ControllingCustomer);
				}
			}

			return result.Distinct(BusinessObjectEqualityComparer<OrgHeader>.PKOnlyComparer).ToArray();
		}

		protected virtual bool IsDPSFreightMovementRestrictedCore()
		{
			return false;
		}

		protected virtual ScreeningParty[] GetScreeningPartiesCore()
		{
			return Array.Empty<ScreeningParty>();
		}

		#endregion

		#endregion

		#region ITemplateReversible Members

		void ITemplateReversible.Reverse()
		{
			ReverseCore();
		}

		protected virtual void ReverseCore()
		{
			ZString origin = JS_RL_NKOrigin;
			JS_RL_NKOrigin = JS_RL_NKDestination;
			JS_RL_NKDestination = origin;

			ZString tempAddressType = ConsigneeDocumentaryAddress.E2_AddressType;
			ConsigneeDocumentaryAddress.E2_AddressType = ConsignorDocumentaryAddress.E2_AddressType;
			ConsignorDocumentaryAddress.E2_AddressType = tempAddressType;
			fConsigneeDocumentaryAddress = null;
			fConsignorDocumentaryAddress = null;

			tempAddressType = ConsigneeDeliveryAddress.E2_AddressType;
			ConsigneeDeliveryAddress.E2_AddressType = ConsignorPickupAddress.E2_AddressType;
			ConsignorPickupAddress.E2_AddressType = tempAddressType;
			fConsigneeDeliveryAddress = null;
			fConsignorPickupAddress = null;
		}

		#endregion

		#region IRatingSupporter Members

		RatingAdaptersProvider IRatingSupporter.AdaptersProvider
		{
			get { return new ShipmentRatingAdaptersProvider<CommonShipment>(this); }
		}

		#endregion

		#region IHandleEventsForOtherObjects Members

		BusinessObject[] IHandleEventsForOtherObjects.GetHandledObjects()
		{
			var jobDocs = DocsAndCartage;
			return jobDocs != null ? new[] { DocsAndCartage } : Array.Empty<BusinessObject>();
		}

		#endregion

		#region IDefaultNumberOfDecimalsSupporter Members

		ZString IDefaultNumberOfDecimalsSupporter.TransportMode
		{
			get
			{
				switch (JS_TransportMode)
				{
					case Constants.TransportModes.AirSea:
						return Constants.TransportModes.Air;

					case Constants.TransportModes.SeaAir:
						return Constants.TransportModes.Sea;

					default:
						return JS_TransportMode;
				}
			}
		}

		public int GetDefaultNumberOfDecimals(PropertyDescriptor property)
		{
			return GetDefaultNumberOfDecimalsCore(property);
		}

		protected virtual int GetDefaultNumberOfDecimalsCore(PropertyDescriptor property)
		{
			return DefaultNumberOfDecimalsSupporterHelperForFreight.GetDefaultNumberOfDecimalsMetaDataProperty(this, property);
		}

		ZString IDefaultNumberOfDecimalsSupporter.GetUnitOfMeasure(PropertyDescriptor property)
		{
			return GetUnitOfMeasureCore(property);
		}

		protected virtual ZString GetUnitOfMeasureCore(PropertyDescriptor property)
		{
			var unitOfMeasure = ZString.Empty;
			var propertyName = DefaultNumberOfDecimals.GetBoundPropertyName(property.Name);

			switch (propertyName)
			{
				case Schema.JS_ActualWeight:
				case Schema.JS_DocumentedWeight:
				case Schema.JS_ManifestedWeight:
				case Schema.JS_ActualWeightReadOnly:
					unitOfMeasure = JS_UnitOfWeight;
					break;

				case Schema.JS_ActualVolume:
				case Schema.JS_DocumentedVolume:
				case Schema.JS_ManifestedVolume:
				case Schema.JS_ActualVolumeReadOnly:
					unitOfMeasure = JS_UnitOfVolume;
					break;

				case Schema.JS_ActualChargeable:
				case Schema.JS_DocumentedChargeable:
				case Schema.JS_ManifestedChargeable:
					unitOfMeasure = JS_ChargeableUnit;
					break;

				case Schema.TotalInnerPackLineWeight:
				case Schema.TotalOuterPacksWeight:
					unitOfMeasure = TotalPackLineWeightUnit;
					break;

				case Schema.TotalInnerPackLineVolume:
				case Schema.TotalOuterPacksVolume:
					unitOfMeasure = TotalPackLineVolumeUnit;
					break;

				case Schema.TotalOuterPacksWeight_Imperial:
				case Schema.JS_ActualWeight_Imperial:
					unitOfMeasure = Constants.Weight.Pounds;
					break;

				case Schema.TotalOuterPacksVolume_Imperial:
				case Schema.JS_ActualVolume_Imperial:
					unitOfMeasure = Constants.Volume.CubicFeet;
					break;

				case Schema.JS_Calc_DocumentedWeight_Converted:
					unitOfMeasure = Env.Registry.FreightWeightUnit;
					break;

				case Schema.JS_Calc_DocumentedVolume_Converted:
					unitOfMeasure = Env.Registry.FreightVolumeUnit;
					break;

				case Schema.JS_Calc_ActualVolumeWeight:
					unitOfMeasure = JS_Calc_ActualVolumeWeightUnit;
					break;

				default:
					break;
			}

			return unitOfMeasure;
		}

		public ZDecimal GetRoundedValue(PropertyDescriptor property, ZDecimal value)
		{
			return GetRoundedValueCore(null, property, value);
		}

		public ZDecimal GetRoundedValue(SchemaColumn column, PropertyDescriptor property, ZDecimal value)
		{
			return GetRoundedValueCore(column, property, value);
		}

		protected virtual ZDecimal GetRoundedValueCore(SchemaColumn column, PropertyDescriptor property, ZDecimal value)
		{
			return DefaultNumberOfDecimalsSupporterWithSchemaColumnHelperForFreight.GetRoundedValue(this, column, property, value);
		}

		void IDefaultNumberOfDecimalsSupporter.RoundMeasurePropertiesOnTransportModeChanged()
		{
			RoundMeasurePropertiesOnTransportModeChangedCore();
		}

		protected virtual void RoundMeasurePropertiesOnTransportModeChangedCore()
		{
			this.SetRoundedValue(JobShipmentSchema.JS_ActualWeight, JS_ActualWeightInfo);
			this.SetRoundedValue(JobShipmentSchema.JS_DocumentedWeight, JS_DocumentedWeightInfo);
			this.SetRoundedValue(JobShipmentSchema.JS_ManifestedWeight, JS_ManifestedWeightInfo);

			this.SetRoundedValue(JobShipmentSchema.JS_ActualVolume, JS_ActualVolumeInfo);
			this.SetRoundedValue(JobShipmentSchema.JS_DocumentedVolume, JS_DocumentedVolumeInfo);
			this.SetRoundedValue(JobShipmentSchema.JS_ManifestedVolume, JS_ManifestedVolumeInfo);

			this.SetRoundedValue(JobShipmentSchema.JS_ActualChargeable, JS_ActualChargeableInfo);
			this.SetRoundedValue(JobShipmentSchema.JS_DocumentedChargeable, JS_DocumentedChargeableInfo);
			this.SetRoundedValue(JobShipmentSchema.JS_ManifestedChargeable, JS_ManifestedChargeableInfo);

			if (outerPackLines != null)
			{
				foreach (PackLine packLine in outerPackLines)
				{
					IDefaultNumberOfDecimalsSupporter decimalsSupporter = packLine;
					decimalsSupporter.RoundMeasurePropertiesOnTransportModeChanged();
				}
			}
			if (innerPackLines != null)
			{
				foreach (PackLine packLine in innerPackLines)
				{
					IDefaultNumberOfDecimalsSupporter decimalsSupporter = packLine;
					decimalsSupporter.RoundMeasurePropertiesOnTransportModeChanged();
				}
			}
			if (deliveryConfirms != null)
			{
				foreach (CommonPickupDeliveryConfirm pickupDeliveryConfirm in deliveryConfirms)
				{
					IDefaultNumberOfDecimalsSupporter decimalsSupporter = pickupDeliveryConfirm;
					decimalsSupporter.RoundMeasurePropertiesOnTransportModeChanged();
				}
			}
		}

		#endregion

		#region IEventDatePropertyChecker

		bool IEventDatePropertyChecker.CanUpdateProperty(IStmALog log, ZPropertyInfo property)
		{
			if (log.SL_SE_NKEvent == Events.FreightLoadedCode && JS_ShippedOnBoard == FreightConstants.ShippedOnBoardType.Received)
			{
				return false;
			}

			if (log.SL_SE_NKEvent == Events.CargoReceivedAtDepotCode && JS_ShippedOnBoard != FreightConstants.ShippedOnBoardType.Received)
			{
				return false;
			}

			if ((log.SL_SE_NKEvent == Events.ArrivalCode || log.SL_SE_NKEvent == Events.DepartureCode)
				&& IsSuspendedFromUpdatingEtaEtdFromLogs)
			{
				return false;
			}

			return IsEventFacilityMatched(log)
				&& IsEventLocationMatched(log);
		}

		protected virtual bool IsEventFacilityMatched(IStmALog log)
		{
			return true;
		}

		protected virtual bool IsEventLocationMatched(IStmALog log)
		{
			log.Parameters.TryGetValue(EventConstants.EventReferenceParameters.Codes.Location, out string locationInEvent);

			switch (log.SL_SE_NKEvent)
			{
				case Events.FreightLoadedCode:
					{
						var parameters = GetParametersForEvent(Events.All[log.SL_SE_NKEvent]);
						parameters.TryGetValue(EventConstants.EventReferenceParameters.Codes.Location, out string shipmentLocation);

						return string.IsNullOrEmpty(locationInEvent) || locationInEvent == shipmentLocation;
					}
				case Events.DepartureCode:
					return JS_RL_NKOrigin == locationInEvent;
				case Events.ArrivalCode:
					return JS_RL_NKDestination == locationInEvent;
				default:
					return true;
			}
		}

		#region SuspendColoadMasterFromUpdatingEtaEtdFromLogs

		int suspendUpdatingEtaEtdFromLogsCount;
		bool IsSuspendedFromUpdatingEtaEtdFromLogs => suspendUpdatingEtaEtdFromLogsCount > 0;

		void SuspendUpdatingEtaEtdFromLogs()
		{
			suspendUpdatingEtaEtdFromLogsCount++;
			CoLoadMasterShipment?.SuspendUpdatingEtaEtdFromLogs();
		}

		void ResumeUpdatingEtaEtdFromLogs()
		{
			suspendUpdatingEtaEtdFromLogsCount--;
			CoLoadMasterShipment?.ResumeUpdatingEtaEtdFromLogs();
		}

		public IDisposable SuspendColoadMasterFromUpdatingEtaEtdFromLogs()
		{
			return new DisposableAction(() => CoLoadMasterShipment?.SuspendUpdatingEtaEtdFromLogs(), () => CoLoadMasterShipment?.ResumeUpdatingEtaEtdFromLogs());
		}

		#endregion

		#endregion

		#region EnterpriseBusinessObject

		protected override void ProcessLogCore(IStmALog log)
		{
			base.ProcessLogCore(log);

			if (log.SL_SE_NKEvent == Events.CargoAvailable.Code)
			{
				if (DocsAndCartage != null)
				{
					string eventLocation;
					string eventFacitlity;
					string containerLocation = GetCAVEventLocation();

					log.Parameters.TryGetValue(EventConstants.EventReferenceParameters.Codes.Location, out eventLocation);
					log.Parameters.TryGetValue(EventConstants.EventReferenceParameters.Codes.Facility, out eventFacitlity);

					if (eventLocation == containerLocation && eventFacitlity == EventConstants.Facilities.Code.Depot)
					{
						if (DocsAndCartage.JP_LCLAvailable != log.SL_EventTime)
						{
							DocsAndCartage.JP_LCLAvailable = log.SL_EventTime;
						}
					}
				}
			}
		}

		#endregion

		#region Events Paramaters

		public virtual IReadOnlyDictionary<string, string> GetParametersForEvent(Event eventType)
		{
			var parameters = new Dictionary<string, string>();

			if (eventType == Events.CargoAvailable)
			{
				parameters[EventConstants.EventReferenceParameters.Codes.Location] = GetCAVEventLocation();
				parameters[EventConstants.EventReferenceParameters.Codes.Facility] = IsArrivalContainerModeFCLorULD ? EventConstants.Facilities.Code.Terminal
																												: EventConstants.Facilities.Code.Depot;
			}
			else if (eventType == Events.FreightLoaded)
			{
				var firstConsol = Consols.GetEarliestConsol();
				var consolLoadPort = FreightEventsHelper.GetLoadPort(firstConsol);
				parameters[EventConstants.EventReferenceParameters.Codes.Location] = string.IsNullOrEmpty(consolLoadPort) ? null : consolLoadPort;
				parameters[EventConstants.EventReferenceParameters.Codes.Facility] = EventConstants.Facilities.Code.Terminal;
			}
			else if (eventType == Events.CargoReceivedAtDepot)
			{
				var firstConsol = Consols.GetEarliestConsol();

				parameters[EventConstants.EventReferenceParameters.Codes.Location] = firstConsol != null ? firstConsol.JK_RL_NKLoadPort : null;
				parameters[EventConstants.EventReferenceParameters.Codes.Facility] = EventConstants.Facilities.Code.Terminal;
			}

			return parameters;
		}

		string GetCAVEventLocation()
		{
			if (DocsAndCartage != null && DocsAndCartage.JP_LCLDatesOverrideConsol)
			{
				return JS_RL_NKDestination;
			}

			var lastLeg = ArrivalConsol != null ? new TransportOrderHelper(ArrivalConsol.Transports).LastLeg : null;
			var location = lastLeg != null ? lastLeg.JW_RL_NKDiscPort : JS_RL_NKDestination;

			return location;
		}

		#endregion

		#region StrictBizPropertyInfos

		public IEnumerable<ZPropertyInfo> StrictBizPropertyInfos
		{
			get
			{
				yield return JS_PackingModeInfo;
				yield return JS_TransportModeInfo;
				yield return JS_ShipmentTypeInfo;
			}
		}

		IJobHeader IHaveJobHeader.JobHeader => ShipmentJobHeader;

		public bool HasCriticalChangesOnProperty(ZPropertyInfo propertyInfo)
		{
			return propertyInfo != null
					&& IsInDatabase
					&& propertiesWithCriticalChanges.Contains(propertyInfo.Name);
		}

		#endregion

		#region HasDangerousGoodsSubstancesForbiddenOnPassengerAircraft

		public virtual bool HasDangerousGoodsSubstancesForbiddenOnPassengerAircraft()
		{
			return OuterPackLines.OfType<PackLine>()
				.Any(packline => packline.UNDGs.OfType<UNDGDataItem>()
					.Any(undg => undg.UNDGSubstance != null && undg.UNDGSubstance.DG_Standard == UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA && undg.UNDGSubstance.DG_LQ2OrPaxMaxAmtType == UNDGSubstanceLookups.LimitedQuantityTypes.FOBCode));
		}

		#endregion

		#region UpdatedByDataRefresh

		protected override void OnBeforeUpdatedByDataRefresh()
		{
			base.OnBeforeUpdatedByDataRefresh();

			snapshotForProperties = TakeSnapshotOf(StrictBizPropertyInfos);
		}

		protected override void OnUpdatedByDataRefresh()
		{
			base.OnUpdatedByDataRefresh();

			CompareSnapshotOf(StrictBizPropertyInfos);
		}

		IDictionary<ZString, IZType> TakeSnapshotOf(IEnumerable<ZPropertyInfo> propertyInfos)
		{
			var result = new Dictionary<ZString, IZType>();

			if (IsInDatabase && !IsDeleted)
			{
				foreach (var info in propertyInfos)
				{
					result.Add(info.Name, info.Value);
				}
			}

			return result;
		}

		void CompareSnapshotOf(IEnumerable<ZPropertyInfo> propertyInfos)
		{
			var currentSnapshot = TakeSnapshotOf(propertyInfos);

			foreach (var key in snapshotForProperties.Keys)
			{
				if (currentSnapshot.ContainsKey(key) && !currentSnapshot[key].Equals(snapshotForProperties[key]))
				{
					propertiesWithCriticalChanges.Add(key);
					MarkAsNeedingValidation();
				}
			}
		}

		readonly HashSet<ZString> propertiesWithCriticalChanges = new HashSet<ZString>();
		IDictionary<ZString, IZType> snapshotForProperties;

		#endregion

		#region Universal Copy

		protected virtual void OnUniversalCopyFinish()
		{
			SetIsDomestic();
			DocsAndCartage.RequiredDocuments.SetAllDocumentsReceivedEventLogger();

			DefaultElectronicBillOfLadingFields(JS_TransportMode, JS_TransportMode, true);

			var jobDocAddressTypesToExcludeFromCopy = GetJobDocAddressTypesToExcludeFromCopy();
			foreach (JobDocAddress address in DocAddresses.ToArray())
			{
				if (jobDocAddressTypesToExcludeFromCopy.Contains(address.E2_AddressType))
				{
					DocAddresses.RemoveAndDelete(address);
				}
			}
		}

		#endregion

		#region Manual Release

		ZString ManualReleaseNoteType
		{
			get { return PredefinedNoteTypes.Instance.CustomsManualStatus.Description; }
		}

		public StmNote ManualStatusNote
		{
			get { return GetNoteCore(ManualReleaseNoteType); }
		}

		public ZString CustomsManualStatus
		{
			get { return GetNoteText(ManualReleaseNoteType); }
			set { SetOrCreateNoteText(ManualReleaseNoteType, value); }
		}

		void SetOrCreateNoteText(string description, ZString value)
		{
			var note = GetNoteCore(description);
			if (note != null)
			{
				note.ST_NoteText = value;
			}
			else
			{
				Notes.AddNew(false, description, value);
			}
		}

		void ICAManualReleaseSupport.DeleteManualRelease()
		{
			this.ManualStatusNote.Delete();
		}

		void ICAManualReleaseSupport.ManualRelease(ICAManualReleaseNote note)
		{
			this.CustomsManualStatus = note.NoteText;
		}

		ZString ICAManualReleaseSupport.ManualReleaseNoteText => CustomsManualStatus;

		ZString ICAManualReleaseSupport.GetReasonForCannotManualRelease()
		{
			return ZString.Empty;
		}

		ZString GetNoteText(string description)
		{
			var note = GetNoteCore(description);
			if (note != null)
			{
				return note.ST_NoteText;
			}
			else
			{
				return ZString.Empty;
			}
		}

		StmNote GetNoteCore(string description)
		{
			return Notes.FindByDescription(description).FirstOrDefault();
		}

		public ZString ManualReleaseDate
		{
			get { return IsNoteTextValid ? IndexList[0] : ZString.Empty; }
		}

		public ZString ManualReleaseReason
		{
			get { return IsNoteTextValid ? IndexList[1] : ZString.Empty; }
		}

		public ZString ManualReleaseUser
		{
			get { return IsNoteTextValid ? IndexList[2] : ZString.Empty; }
		}

		public ZString ManualReleaseSystemDate
		{
			get { return IsNoteTextValid ? IndexList[3] : ZString.Empty; }
		}

		bool IsNoteTextValid
		{
			get { return IndexList.Count == 4; }
		}

		List<ZString> IndexList
		{
			get
			{
				if (indexList == null || indexList.Count == 0)
				{
					indexList = new List<ZString>();
					var splitstr = CustomsManualStatus.Split('\r', '\n');
					foreach (var zString in splitstr.Where(zString => !zString.IsEmpty))
					{
						indexList.Add(zString);
					}
				}
				return indexList;
			}
		}

		List<ZString> indexList;

		bool IStmNoteParentWithSystemNote.IsSystemNote(StmNote note)
		{
			return note != null
					 && (note.ST_Description == PredefinedNoteTypes.Instance.CustomsManualStatus.Description);
		}

		#endregion

		#region Aviation Security

		public CommonAviationSecuritySupport AviationSecurity => aviationSecurity ?? (aviationSecurity = GetNewAviationSecurity());
		CommonAviationSecuritySupport aviationSecurity;

		protected virtual CommonAviationSecuritySupport GetNewAviationSecurity()
		{
			return new CommonAviationSecuritySupport(this);
		}

		#endregion

		#region ShipmentContainsPermissibleQuantities

		public virtual bool ShipmentContainsPermissibleQuantities()
		{
			return true;
		}

		#endregion

		public bool IsSuppressedETAETDOnAttachToConsol { get; set; }

		protected override Logs GetNewLogs()
		{
			return new CommonShipmentLogs(this);
		}

		#region CarrierServiceLevel

		// Test Failure : DefaultClientServiceLevelFromParent
		// LoadFromNaturalKey was used on a table + field that does not have a unique index
		public override OrgCarrierServiceLevel CarrierServiceLevel
		{
			get { return Factory.LoadTop1<OrgCarrierServiceLevel>(new ZQuery(OrgCarrierServiceLevelSchema.PL_Code, JS_PL_NKCarrierServiceLevel)); }
		}

		#endregion

		#region Container Penalties

		[ChildEditable(true)]
		public ShipmentContainerPenaltyCollection PickupPenalties
		{
			get
			{
				if (pickupPenalties == null)
				{
					pickupPenalties = new ShipmentContainerPenaltyCollection(this, Core.Constants.ContainerPenaltyProcessType.Pickup);
					RegisterEditableChildObject(pickupPenalties);
				}

				return pickupPenalties;
			}
		}
		ShipmentContainerPenaltyCollection pickupPenalties;

		[ChildEditable(true)]
		public ShipmentContainerPenaltyCollection DeliveryPenalties
		{
			get
			{
				if (deliveryPenalties == null)
				{
					deliveryPenalties = new ShipmentContainerPenaltyCollection(this, Core.Constants.ContainerPenaltyProcessType.Delivery);
					RegisterEditableChildObject(deliveryPenalties);
				}

				return deliveryPenalties;
			}
		}
		ShipmentContainerPenaltyCollection deliveryPenalties;

		public bool SupportsDeliveryPenalties => JS_PackingMode == Core.Constants.ContainerModes.FCL || JS_PackingMode == Core.Constants.ContainerModes.BuyersConsol && JS_ShipmentType == Core.Constants.ShipmentTypes.BuyersConsolLead;

		public bool SupportsPickupPenalties => JS_PackingMode == Core.Constants.ContainerModes.FCL;

		#endregion

		ZBool IsSuppressedCheckReset { get; set; }

		public DisposableAction SuspendCheckReset()
		{
			return new DisposableAction(() => IsSuppressedCheckReset = true, () => IsSuppressedCheckReset = false);
		}

		#region IOriginDestinationForDocumentDeliveryRestriction

		string IOriginDestinationForDocumentDeliveryRestriction.OriginCountryCode => CountryCode(JS_RL_NKOrigin);

		string IOriginDestinationForDocumentDeliveryRestriction.DestinationCountryCode => CountryCode(JS_RL_NKDestination);

		#endregion

		#region Compliance Risk

		public ComplianceRiskStatusObject ComplianceRiskStatus => ObjectFactory.Get<IComplianceRiskStatusSupporter>().GetStatus((IComplianceItemRiskStatusProvider)this);

		[CargoWise.Macros.MacroIgnore]
		public ZString OverallComplianceRisk => ComplianceRiskStatus.GetOverallRiskDescription();

		[CargoWise.Macros.MacroIgnore]
		public ZString PartyComplianceRisk => ComplianceRiskStatus.GetPartyRiskDescription();

		[CargoWise.Macros.MacroIgnore]
		public ZString LocationComplianceRisk => ComplianceRiskStatus.GetLocationRiskDescription();

		[CargoWise.Macros.MacroIgnore]
		public ZString CommodityComplianceRisk => ComplianceRiskStatus.GetCommodityRiskDescription();

		ZBool IComplianceJobDirectionProvider.IsInternational => (this.IsCrossTrade() || this.IsExport() || this.IsImport());

		#endregion

		#region HasSentAdvancedCargoReport

		public bool HasSentAdvancedCargoReport
		{
			get
			{
				var log = Logs.GetAllLogs()
					.Cast<StmALog>()
					.OrderByDescending(l => l.SL_EventTime)
					.FirstOrDefault(l => (l.SL_SE_NKEvent == Events.MessageSentCode ||
																l.SL_SE_NKEvent == Events.MessageWithdrawCancelAcceptedCode ||
																l.SL_SE_NKEvent == Events.InterchangeRejectedCode ||
																l.SL_SE_NKEvent == Events.MessageRejectedCode)
						&& l.Parameters.ContainsKey(EventReferenceParameters.Codes.Department)
						&& string.Compare(l.Parameters[EventReferenceParameters.Codes.Department], (NoResString)"Customs", StringComparison.OrdinalIgnoreCase) == 0
						&& l.Parameters.ContainsKey(EventReferenceParameters.Codes.MessageType)
						&& string.Compare(l.Parameters[EventReferenceParameters.Codes.MessageType], (NoResString)"Advanced Cargo Report", StringComparison.OrdinalIgnoreCase) == 0);

				return log != null && log.SL_SE_NKEvent == Events.MessageSentCode;
			}
		}

		#endregion

		#region ISupportInspectionType

		public CusEntryNumber AdditionalInspectionType => LoadOrCreateAdditionalInspectionTypeCusEntryNumber();

		public CusEntryNumber InspectionType => LoadOrCreateInspectionTypeCusEntryNumber();

		#endregion

		#region Delivery Due Date

		public virtual void CalculateDeliveryDueDateIfNecessary()
		{
		}

		public virtual void DeliveryDueDateFactorHasChanged(DeliveryDueDateChangedFactor changedFactor)
		{
		}

		#endregion

		#region AddressAdditionalInfo

		[JobAddressAdditionalInfoAddressTypes(
			AutoDocAddressTypes.Codes.ConsignorPickupDeliveryAddress,
			AutoDocAddressTypes.Codes.DepartureCFSAddress,
			AutoDocAddressTypes.Codes.ArrivalCFSAddress,
			AutoDocAddressTypes.Codes.ConsigneePickupDeliveryAddress)]
		public IJobAddressAdditionalInfoCollection JobAddressAdditionalInfoCollection
		{
			get
			{
				if (jobAddressAdditionalInfoCollection == null)
				{
					jobAddressAdditionalInfoCollection = new JobAddressAdditionalInfoCollection(this, Factory);
					if (!IsDeleted && !IsDeleting)
					{
						this.HookEventsToDependentAddresses();
					}
					RegisterEditableChildObject(jobAddressAdditionalInfoCollection as JobAddressAdditionalInfoCollection);
				}
				return jobAddressAdditionalInfoCollection;
			}
		}

		IJobAddressAdditionalInfoCollection jobAddressAdditionalInfoCollection { get; set; }

		ZPropertyInfo IJobAddressAdditionalInfoSupport.GetDependentAddress(string addressType)
		{
			return addressType switch
			{
				AutoDocAddressTypes.Codes.ConsignorPickupDeliveryAddress => ConsignorPickupAddress.OrganisationPKInfo,
				AutoDocAddressTypes.Codes.DepartureCFSAddress => JS_OA_ExportReceivingDepotInfo,
				AutoDocAddressTypes.Codes.ConsigneePickupDeliveryAddress => ConsigneeDeliveryAddress.OrganisationPKInfo,
				AutoDocAddressTypes.Codes.ArrivalCFSAddress => JS_OA_ImportReleaseDepot_ZAddress.OrgPKInfo,
				_ => null
			};
		}

		[BusinessObjectTestExclude]
		public virtual ZString PickupByTransportMode
		{
			get => this.GetTransportModeOrDefault(AutoDocAddressTypes.Codes.ConsignorPickupDeliveryAddress, ConsignorPickupAddress.E2_AddressOverride);
			set => this.SetTransportMode(AutoDocAddressTypes.Codes.ConsignorPickupDeliveryAddress, value);
		}

		public ZPropertyInfo PickupByTransportModeInfo => GetZPropertyInfo(Schema.PickupByTransportMode);

		public bool PickupByTransportMode_ReadOnly => ConsignorPickupAddress.IsEmpty;

		[BusinessObjectTestExclude]
		public virtual ZString CFSDepartureByTransportMode
		{
			get => this.GetTransportModeOrDefault(AutoDocAddressTypes.Codes.DepartureCFSAddress);
			set => this.SetTransportMode(AutoDocAddressTypes.Codes.DepartureCFSAddress, value);
		}

		public ZPropertyInfo CFSDepartureByTransportModeInfo => GetZPropertyInfo(Schema.CFSDepartureByTransportMode);

		public bool CFSDepartureByTransportMode_ReadOnly => GetNewJS_OA_ExportReceivingDepot_ZAddress().OrgPK == Guid.Empty;

		[BusinessObjectTestExclude]
		public virtual ZString DeliveryByTransportMode
		{
			get => this.GetTransportModeOrDefault(AutoDocAddressTypes.Codes.ConsigneePickupDeliveryAddress, ConsigneeDeliveryAddress.E2_AddressOverride);
			set => this.SetTransportMode(AutoDocAddressTypes.Codes.ConsigneePickupDeliveryAddress, value);
		}

		public ZPropertyInfo DeliveryByTransportModeInfo => GetZPropertyInfo(Schema.DeliveryByTransportMode);

		public bool DeliveryByTransportMode_ReadOnly => ConsigneeDeliveryAddress.IsEmpty;

		[BusinessObjectTestExclude]
		public virtual ZString CFSArrivalByTransportMode
		{
			get => this.GetTransportModeOrDefault(AutoDocAddressTypes.Codes.ArrivalCFSAddress);
			set => this.SetTransportMode(AutoDocAddressTypes.Codes.ArrivalCFSAddress, value);
		}

		public ZPropertyInfo CFSArrivalByTransportModeInfo => GetZPropertyInfo(Schema.CFSArrivalByTransportMode);

		public bool CFSArrivalByTransportMode_ReadOnly => JS_OA_ImportReleaseDepot_ZAddress.OrgPK == Guid.Empty;

		public ZGuid JobAddressAdditionalInfoParentID => this.PK;

		public ZString JobAddressAdditionalInfoTableCode => this.TablePrefix;

		#endregion

		#region IEDIMessageCollectionOwner
		BusinessObject IEDIMessageCollectionOwner.MessageOwner => this;

		IBusinessObjectCollection IEDIMessageCollectionOwner.Messages => this.Messages;

		#endregion

		#region Global Commercial Invoice

		IGlobalCommercialInvoiceComplianceProvider globalCommercialInvoiceProvider;
		IGlobalCommercialInvoiceComplianceProvider IGlobalCommercialInvoiceProvider.DataProvider => globalCommercialInvoiceProvider ??= ObjectFactory.Get<IGlobalCommercialInvoiceComplianceProcessor>().GetGlobalCommercialInvoiceBusinessObject(this);

		ZGuid IGlobalCommercialInvoiceJobProvider.ParentID => PK;

		ZString IGlobalCommercialInvoiceJobProvider.ParentTableCode => TablePrefix;

		ZString IGlobalCommercialInvoiceJobProvider.JobNumber => JobNumber;

		ZString IGlobalCommercialInvoiceJobProvider.VolumeUnitOfMeasurement => ShipmentVolumeUnit;

		ZString IGlobalCommercialInvoiceJobProvider.WeightUnitOfMeasurement => ShipmentWeightUnit;

		public virtual void GlobalCommercialInvoiceUpdateUnitOfMeasurement(ZPropertyInfo propertyInfo, ZString oldValue, ZString newValue)
		{
		}

		#endregion

		#region CommonShipmentJobDatesProvider Support

		public virtual ZDate RevenueAutoratingDate { get; set; }

		#endregion
	}
}
