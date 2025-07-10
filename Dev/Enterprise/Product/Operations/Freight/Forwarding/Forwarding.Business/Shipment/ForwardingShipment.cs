using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.IO;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Integration;
using Enterprise.BufferManagement.Integration;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.Universal;
using Enterprise.DeniedPartyScreening.Integration;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.EConversation.Business;
using Enterprise.Environment;
using Enterprise.eTail.Integration;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Extensions;
using Enterprise.Freight.CarbonEmissions.Business;
using Enterprise.Freight.CarbonEmissions.Integration;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Forwarding.Business.AWB;
using Enterprise.Freight.Forwarding.Business.HelperClasses;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Freight.Forwarding.Registry;
using Enterprise.Freight.Integration;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.Freight.LocalCartage.Integration;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.Freight;
using Enterprise.Integration.TransitWarehouse;
using Enterprise.Integration.TransportBooking;
using Enterprise.MasterData.Business;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Business.Rating;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Packing.Business;
using Enterprise.Packing.Integration;
using Enterprise.Rating.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Security;
using Enterprise.TransportBookings.Shared;
using Enterprise.TransportCommon.Shared;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Integration.CodeLists;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.EventManagement;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using static Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;
using static Enterprise.Freight.Integration.Forwarding;
using static Enterprise.Integration.Customs;
using static Enterprise.Integration.Customs.EUExitControl;
using static Enterprise.Integration.Forwarding;
using static Enterprise.ZArchitecture.Business.ReferenceStrategy;
using AdditionalReferenceNumbersCodes = Enterprise.Registry.Business.CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes;
using CommonConsol = Enterprise.Freight.Business.CommonConsol;
using Constants = Enterprise.Core.Constants;
using EnterpriseBusinessObject = Enterprise.ZArchitecture.EnterpriseBusinessObject;
using Event = Enterprise.ZArchitecture.Business.Event;
using EventConstants = CargoWise.EventReference.Constants;
using IAUCusHAWB = Enterprise.Integration.Customs.AU.ICusHAWB;
using IAUCusSCAHouse = Enterprise.Integration.Customs.AU.ICusSCAHouse;
using IAWBParent = Enterprise.Freight.Forwarding.Business.AWB.IAWBParent;
using IBaseJobDeclaration = Enterprise.Integration.Customs.IBaseJobDeclaration;
using ICusHAWB = Enterprise.Integration.Customs.Shared.ICusHAWB;
using IForwardingShipment = Enterprise.Integration.Forwarding.IForwardingShipment;
using MetaData = CargoWise.ComponentModel.MetaData;
using Order = Enterprise.Freight.Forwarding.Orders.Business.Order;
using Params = CargoWise.EventReference.Constants.EventReferenceParameters.Codes;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;
using WorkflowSelectionOrgTypeCodes = Enterprise.Registry.Business.ClientInTemplateSelectionOrgTypeList.Codes;

namespace Enterprise.Freight.Forwarding.Business
{
	[SupportExRateSource(ExRateSourceType.BillingJob)]
	[ProvideMetaDataProperty("ReadOnlySecurity", MetaDataTypes.ReadOnly), UniversalDataContext(DataContextType.ForwardingShipment)]
	[UniversalCopyChildrenSubstituteType(typeof(CommonConsol), typeof(ForwardingConsol))]
	[UniversalCopyAssociateElement("JobOrderHeaders", "AttachedOrders")]
	[UniversalCopyIgnoreElement("JobCO2Emissions")]
	[VisualizableDocumentsSupportable("ForwardingShipmentVisualizableDocumentSupporter")]
	[IDontMindLoadingASubclassInstead]
	public partial class ForwardingShipment : CommonShipment,
		IForwardingShipment,
		IAttachOrders,
		IAttachGenericOrders,
		ICustomLabelsConfigOrgProvider,
		ISendEmailSource,
		IAWBParent,
		IParentForCargoReporter,
		IWorkflowProviderIncludingRelated,
		IWorkflowTriggerFieldChangeSource,
		IWorkflowTriggerEventSource,
		ICustomsChargesAdditionalCharges,
		ICartageParent,
		ICartageParentExtra,
		IBuyerSupplierRelationshipConsumer,
		IDocumentDataStateManager,
		IScreeningPartyProvider,
		IShouldUpdateScreeningStatus,
		ICustomsJobInfoProvider,
		IBillDetails,
		ICustomFieldProvider,
		IDtbBookingParent,
		IPackLineSynchroniseProvider,
		IProcessHandlingInfoProvider,
		ICusInBondParent,
		IRatingSupporterWithAdapter,
		IModuleToModule,
		IRelatedItemsNameProvider,
		IConsolOrShipment,
		MasterFiles.Business.DIS.IDISHostProvider,
		ILocalClientJobHandler,
		IRelatableActivity,
		ISupportsPostingOverseasAgentCharge,
		IJobInvoicingExRateSourceProvider,
		IHaveDetailedGoodsDescription,
		IAdditionalFetchHintsForJobParent,
		IValidateForCustomsMessagingSupporter,
		IStmNoteParentWithSystemNote,
		ITemplateRecordProvider,
		ISupplyChainSecurityImportExportSupporter,
		IRelatedOrgDeniedPartyScreenable,
		ISupportJobDocumentRecipient,
		ICusEntryNumberValidationDeciderOfType,
		IAdditionalReferenceNumberTypeProvider,
		IAdditionalReferenceNumberValidationProvider,
		IAllowAdditionalRootType,
		ITransitWarehouseParent,
		ITransitWarehouseInstructionSupporter,
		IConversationProvider,
		IConversationAdditionalParticipantProvider,
		IConversationParentHyperlinkProvider,
		IAllowAttachEmailsToEDocs,
		ISometimesWorkflowProvider,
		IWorkflowTemplateParameterProvider,
		IDeniedPartyProvider,
		ICompliancePartyRiskStatusProvider,
		IComplianceLocationRiskStatusProvider,
		IComplianceCommodityRiskStatusProvider,
		IComplianceWiseShipment,
		IUniqueConsolProvider,
		ICO2eLegBasedSupporter,
		IAddressesValidation,
		IRegisterStatusChangeContext,
		IShouldPackTrackedPackagesViaDivot,
		IGlobalSearchBusinessObjectProvider,
		IOperationalActionsNullValueInitializer,
		IForwardingShipmentDeclarationProvider,
		IDGPortalLaunchErrorProvider,
		IExternalRequestGenerationProvider,
		IGatePassMovementProviderFactory
	{
		public new class Schema : CommonShipment.Schema
		{
			public const string PickupGoodsSignedForBy = "PickupGoodsSignedForBy";
			public const string DeliveryGoodsSignedForBy = "DeliveryGoodsSignedForBy";
			public const string IsPPQForm368Box13Compatible = "IsPPQForm368Box13Compatible";
			public const string IsPGARecapDocumentToBeShown = "IsPGARecapDocumentToBeShown";
			public const string IsProofOfReleaseDocumentToBeShown = "IsProofOfReleaseDocumentToBeShown";
			public const string BKGNumber = "BKGNumber";
			public const string CustomsEntryNumberForBinding = "CustomsEntryNumberForBinding";
			public const string ExitStatus = "ExitStatus";
		}

		[SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals")]
		public ForwardingShipment(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			factory.SetFreightDomainContext(FreightDomainContext.Forwarding);
			CountrySpecificForwardingShipmentSupport countrySpecificSupport = CountrySpecificSupport;
			BuyerSupplierLinksHelper = new BuyerSupplierLinksHelper<ForwardingShipment>(this);
			BuyerSupplierLinksHelper.Register();
			JS_MarksAndNumbersInfo.ValueChanged += JS_MarksAndNumbersInfo_ValueChanged;
			GetDefaultImportBrokerFromBuyerSupplierRelationshipDelegate = GetDefaultImportBrokerFromBuyerSupplierRelationship;
		}

		public bool IsShipmentJobHeaderForCurrentCompany => ShipmentJobHeader != null && OriginalLoginCompanyPK == ShipmentJobHeader.JH_GC;

		protected override ZString HumanReadableNameWithoutIDCore
		{
			get
			{
				var nameWithoutId = base.HumanReadableNameWithoutIDCore;
				if (IsTemplateRecord)
				{
					nameWithoutId = Res.GetString("32345378-dbed-4db4-8017-11abda5c86d3", "Template") + " " + nameWithoutId;
				}
				return nameWithoutId;
			}
		}

		protected bool ConsolShouldUpdateScreeningStatus(ForwardingConsol consol)
		{
			return consol.IsDeleted || ((JS_ScreeningStatus != ScreeningStatusesList.Codes.JobCleared && JS_ScreeningStatus != ScreeningStatusesList.Codes.Clear) || consol.JK_ScreeningStatus != ScreeningStatusesList.Codes.JobCleared);
		}

		protected bool CoLoadMasterShipmentShouldUpdateScreeningStatus(ForwardingShipment coLoadMasterShipment)
		{
			return coLoadMasterShipment.IsDeleted || ((JS_ScreeningStatus != ScreeningStatusesList.Codes.JobCleared && JS_ScreeningStatus != ScreeningStatusesList.Codes.Clear) || coLoadMasterShipment.JS_ScreeningStatus != ScreeningStatusesList.Codes.JobCleared);
		}

		#region Fetch Strategy

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new ForwardingShipmentFetchStrategy(this);
		}

		#endregion

		#region Validation

		protected override JobShipmentValidation GetNewValidation()
		{
			InitialisePhaseDependantMandatoryValidation();
			return new ForwardingShipmentValidation(this);
		}

		public new ForwardingShipmentValidation Validation
		{
			get { return (ForwardingShipmentValidation)base.Validation; }
		}

		#endregion

		#region Country Specific Support

		Type[] CountrySpecificSupportClasses
		{
			get
			{
				if (countrySpecificSupportClasses == null)
				{
					countrySpecificSupportClasses = new Type[]
					{
						typeof(UAEForwardingShipmentSupport),
						typeof(IcelandForwardingShipmentSupport),
						typeof(SouthAfricaForwardingShipmentSupport),
						typeof(CountrySpecificForwardingShipmentSupport)
					};
				}

				return countrySpecificSupportClasses;
			}
		}
		Type[] countrySpecificSupportClasses;

		internal CountrySpecificForwardingShipmentSupport CountrySpecificSupport
		{
			get
			{
				if (countrySpecificSupport == null)
				{
					foreach (var type in CountrySpecificSupportClasses)
					{
						var support = (CountrySpecificForwardingShipmentSupport)Activator.CreateInstance(type);
						if (support.Register(this))
						{
							countrySpecificSupport = support;
							break;
						}
					}
				}

				return countrySpecificSupport;
			}
		}

		[SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals")]
		CountrySpecificForwardingShipmentSupport countrySpecificSupport;

		#endregion

		#region Aviation Security

		public new AviationSecuritySupport AviationSecurity => aviationSecurity ?? (aviationSecurity = (AviationSecuritySupport)GetNewAviationSecurity());
		AviationSecuritySupport aviationSecurity;

		protected override CommonAviationSecuritySupport GetNewAviationSecurity()
		{
			return new AviationSecuritySupport(this);
		}

		bool isAutomaticInspectionTypeCalculation;

		public override bool SetApprovedShipperStatus(ZString reason, bool overrideUserEnteredValue = false)
		{
			using (new DisposableAction(() => isAutomaticInspectionTypeCalculation = !overrideUserEnteredValue, () => isAutomaticInspectionTypeCalculation = false))
			{
				var result = AviationSecurity.SetApprovedShipperStatus(reason, overrideUserEnteredValue);
				if (result)
				{
					foreach (var relatedCountry in AviationSecurity.SupplyChainSecurityConfiguration.RelatedCountriesForSupplyChainSecurity)
					{
						new AviationSecuritySupport(this, relatedCountry).SetApprovedShipperStatus(reason, overrideUserEnteredValue);
					}
				}

				return result;
			}
		}

		internal void AviationSecurityParty_HasChanges(string partyType)
		{
			if (!fIsImportingData && !IsSettingDefaultValues &&
				(AviationSecurity.SupplyChainSecurityConfiguration.OrganisationsToUse.ContainsKey(partyType) || AviationSecurity.SupplyChainSecurityConfiguration.GetPartyTypeChangeForcesRecalculateApproveShipperStatus(partyType)))
			{
				SetApprovedShipperStatus(Res.GetString("2149e537-917c-47e5-9dbd-629b99c7b10b", "{0} has been changed", SupplyChainSecurityOrganisationTypes.GetDescription(partyType)));
			}
		}

		protected override void LocalChargesAddressChanged(object sender, EventArgs e)
		{
			base.LocalChargesAddressChanged(sender, e);
			AviationSecurityParty_HasChanges(SupplyChainSecurityOrganisationTypes.LocalClient);
		}

		internal ZString MostRecentInspectionTypeChangeReason { get; set; }

		#endregion

		#region Default Values / Load

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			try
			{
				IsSettingDefaultValues = true;

				JS_IsForwardRegistered = true;
				JS_Phase = ForwardingConfigurationRegistry.Instance.ShipmentDefaultPhase.Value;
				JS_ShipmentStatus = ShipmentStatusList.Codes.Confirmed;

				if (JS_TransportMode == Constants.TransportModes.Air)
				{
					JS_HBLAWBChargesDisplay = GetDefaultChargesDisplay();
				}
			}
			finally
			{
				IsSettingDefaultValues = false;
			}
		}

		void JS_MarksAndNumbersInfo_ValueChanged(object sender, EventArgs e)
		{
			TriggerPackLineSynchroniser();
		}

		void JS_RL_NKFreightRateOriginSetDefault()
		{
			if (ExportReceivingDepot != null && JS_RL_NKFreightRateOrigin.IsEmpty && FreightDataRegistry.Instance.DefaultRateOriginDestination.Value)
			{
				JS_RL_NKFreightRateOrigin = ExportReceivingDepot.OA_RL_NKRelatedPortCode;
			}
		}

		void JS_RL_NKFreightRateDestinationSetDefault()
		{
			if (ImportReleaseDepot != null && JS_RL_NKFreightRateDestination.IsEmpty && FreightDataRegistry.Instance.DefaultRateOriginDestination.Value)
			{
				JS_RL_NKFreightRateDestination = ImportReleaseDepot.OA_RL_NKRelatedPortCode;
			}
		}

		#endregion

		#region Lookups

		public new ForwardingShipmentLookups Lookups
		{
			get { return (ForwardingShipmentLookups)base.Lookups; }
		}

		protected override JobShipmentLookups GetNewLookups()
		{
			return new ForwardingShipmentLookups(this);
		}

		#endregion

		#region Saving

		void ConsolPreSaveValidation()
		{
			foreach (ForwardingConsol consol in Consols)
			{
				consol.Validation.ValidateJK_IsHazardous();
			}
		}

		protected override void RunPreSaveValidationCore()
		{
			DefaultControllingCustomer(BuyerSupplierLinksHelper?.GetControllingCustomer(), false);
			DefaultControllingAgent(false);

			if (!IsRunningPreSaveValidationAsChild)
			{
				RefCountryRulesHelper.AddRulesToNotes(Origin?.Country, Destination?.Country, Notes, TransportMode, IsInDatabase, true);
			}

			ConsolPreSaveValidation();
			base.RunPreSaveValidationCore();
		}

#if DEBUG
		virtual
#endif
		public IShipmentAnnouncer ShipmentAnnouncer
		{
			get
			{
				if (fShipmentAnnouncer == null)
				{
					if (Declarations.Length > 0)
					{
						fShipmentAnnouncer = (IShipmentAnnouncer)Activator.CreateInstance(ObjectFactory.GetType<IShipmentAnnouncer>());
					}
				}
				return fShipmentAnnouncer;
			}
		}
		IShipmentAnnouncer fShipmentAnnouncer;

		protected override void OnFactorySaved(bool saveSucceeded)
		{
			base.OnFactorySaved(saveSucceeded);

			if (saveSucceeded)
			{
				OverrideWaybillDefaultsHasChanges = false;

				if (DeliveryAddressHasBeenChangedByShipmentUser)
				{
					if (ShipmentAnnouncer != null)
					{
						ShipmentAnnouncer.Announce(this);
					}

					DeliveryAddressHasBeenChangedByShipmentUser = false;
				}
			}
		}

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();
			CreateTasksAndMilestonesFromTemplate(new ProcessTask.Loader(Factory));

			if (!IsDeleted)
			{
				LogInspectionTypeCodeChanges();
				if (FreightDataRegistry.Instance.CalculateDeliveryDueDateByTransportMode.Value.IsTransportModeActive(JS_TransportMode))
				{
					LogDeliveryDateUpdatedEvent();
					if (JS_IsForwardRegistered)
					{
						LogRevisedDeliveryDateUpdatedEvent();
						LogRDDByDeliveryDueDateChanged();
					}
				}
			}
		}

		public void CreateTasksAndMilestonesFromTemplate(ProcessTask.Loader processLoader, TemplateApplicationParameters parameters = null)
		{
			if (JS_IsForwardRegistered)
			{
				processLoader.CreateTasksAndMilestonesFromTemplateIfRequired(this, parameters);
				ProposeWorkflowRelationships();
			}
		}

		void ProposeWorkflowRelationships()
		{
			var links = Factory.Load<JobConShipLink>(new ZQuery(JobConShipLinkSchema.JN_JS, PK));

			foreach (var link in links)
			{
				link.ProposeWorkflowRelationships();
			}
		}

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();

			var shouldUpdateScreeningStatus = ((IShouldUpdateScreeningStatus)this).ShouldUpdateScreeningStatus;
			UpdateStatusFromScreeningParties(false);

			if (shouldUpdateScreeningStatus)
			{
				foreach (ForwardingConsol consol in Consols)
				{
					if (ConsolShouldUpdateScreeningStatus(consol))
					{
						consol.UpdateStatusFromScreeningParties(true);
					}
				}

				var coLoadMasterShipment = base.CoLoadMasterShipment as ForwardingShipment;
				if (coLoadMasterShipment?.IsMasterShipmentRepresentingAllChildShipments ?? false)
				{
					if (CoLoadMasterShipmentShouldUpdateScreeningStatus(coLoadMasterShipment))
					{
						coLoadMasterShipment.UpdateStatusFromScreeningParties(true);
					}
				}

				if (screeningPartySnapshot == null)
				{
					if (IsInDatabase)
					{
						var newFactory = new ReadOnlyBusinessObjectFactory();
						var reloaded = newFactory.Load<ForwardingShipment>(PK);
						screeningPartySnapshot = reloaded == null ? new List<ScreeningPartiesSnapshot>() : GetSnapshot(reloaded);
					}
					else
					{
						screeningPartySnapshot = new List<ScreeningPartiesSnapshot>();
					}
				}

				currentSnapshot = GetSnapshot(this);

				var screeningLogCollection = (IStmEntityScreeningLogCollection)Activator.CreateInstance(ObjectFactory.GetType<IStmEntityScreeningLogCollection>(), this);
				screeningLogCollection.AddRemoveOrInsertPartyScreeningLog(screeningPartySnapshot, currentSnapshot);
			}
		}

		void UpdateStatusFromScreeningParties(bool setShouldUpdateScreeningStatus)
		{
			if (((IShouldUpdateScreeningStatus)this).ShouldUpdateScreeningStatus || setShouldUpdateScreeningStatus)
			{
				if (lastTransactionIdForScreeningUpdate != Factory.TransactionId || Factory.TransactionId == 0)
				{
					lastTransactionIdForScreeningUpdate = Factory.TransactionId;
					if (setShouldUpdateScreeningStatus)
					{
						((IShouldUpdateScreeningStatus)this).ShouldUpdateScreeningStatus = true;
					}

					ScreeningStatusUpdater.UpdateJobStatusFromItsScreeningParties(this, this, JS_ScreeningStatusInfo.OriginalValue?.ToString());
				}
			}
		}

		long lastTransactionIdForScreeningUpdate;

		#region Populating JC_EmptyReturnedBy

		void CalculateJC_EmptyReturnedBy_ForContainersIfRequired()
		{
			if (IsDocAddressesLoaded && IsJobDocAddressOrganisationChanged(ConsigneeDocumentaryAddress))
			{
				CommonContainer[] copiedContainers = Containers.ToArray();
				foreach (CommonContainer container in copiedContainers)
				{
					container.CalculateJC_EmptyReturnedBy();
				}
			}
			else if (HasChanges)
			{
				CommonContainer[] copiedContainers = Containers.ToArray();

				foreach (CommonContainer container in copiedContainers)
				{
					BusinessObject relationship = container.Consol == null ?
						null
						:
						Consols.GetRelationshipBusinessObject(container.Consol);

					if (relationship != null && !relationship.IsInDatabase)
					{
						container.CalculateJC_EmptyReturnedBy();
					}
				}
			}
		}

		bool IsJobDocAddressOrganisationChanged(JobDocAddress docAddress)
		{
			bool result = false;
			if (docAddress != null && docAddress.HasChanges)
			{
				if (!docAddress.IsInDatabase)
				{
					result = true;
				}
				else if (docAddress.E2_OA_AddressInfo.HasChanges)
				{
					ZGuid oldOrgPk = ZGuid.Empty;
					ZGuid newOrgPk = ZGuid.Empty;
					if (docAddress.HasRealOrganisation)
					{
						newOrgPk = docAddress.OrganisationPK;
					}

					if (docAddress.E2_OA_AddressInfo.OriginalValue != null)
					{
						ZGuid oldAddressPk = (ZGuid)docAddress.E2_OA_AddressInfo.OriginalValue;
						if (oldAddressPk.IsValid && !oldAddressPk.IsEmpty)
						{
							OrgAddress oldAddress = Factory.Load<OrgAddress>(oldAddressPk);
							if (oldAddress != null)
							{
								oldOrgPk = oldAddress.OA_OH;
							}
						}
					}

					if (oldOrgPk != newOrgPk)
					{
						result = true;
					}
				}
			}

			return result;
		}

		#endregion

		bool hasAnyPackageBeenDetached;

		public override void OnSaving()
		{
			DebugLog.AppendLine((NoResString)"Start OnSaving.");
			LogDebugInfo();

			RelatedChildActivityPivotCollection.RelinkRelatedSuperAndSubActivities();
			RelatedParentActivityPivotCollection.RelinkRelatedSuperAndSubActivities();

			base.OnSaving();

			RefCountryRulesHelper.AddRulesToNotes(Origin?.Country, Destination?.Country, Notes, TransportMode, IsInDatabase, false);

			if (MAWBAllocator != null)
			{
				MAWBAllocator.MAWBAllocation.PerformMAWBAllocation();
			}
			UpdatePreAllocatedAmountExceededStatusIfNecessary();
			DefaultShipmentGatewaysIfNecessary();
			ReportShipmentStatusIsEmptyOnBooking();
			CalculateJC_EmptyReturnedBy_ForContainersIfRequired();
			CalculateDeliveryDueDateIfNecessary();
			DefaultRateCommodityIfNecessary();
			UpdateConsolsCO2eStatusWhenWeightChanges();
			RequireSendingPrepareDispatchTWInstructionForBlindPackages = HasAnyBlindPackageBeenAttached() || hasAnyPackageBeenDetached;
			this.ClearCachedDocumentData();

			DebugLog.AppendLine((NoResString)"End OnSaving.");
			LogDebugInfo();
		}

		void UpdateConsolsCO2eStatusWhenWeightChanges()
		{
			if (JS_ActualWeightInfo.HasChanges || JS_UnitOfWeightInfo.HasChanges)
			{
				Consols.Cast<ICO2eProvider>()
					.ForEach(consol => consol.UpdateCO2eStatusToNotCurrent(new CO2eStatusChangedReason(freeTextReason: (NoResString)"Shipment Weight"), IsCopying));
			}
		}

		void DefaultRateCommodityIfNecessary()
		{
			if (JS_RH_NKRateCommodity.IsEmpty)
			{
				if (!(Job?.LocalCharges?.CompanyData?.RateCommodityDefaultingRules).IsNullOrEmpty())
				{
					JS_RH_NKRateCommodity = GetDefaultRateCommodity(Job.LocalCharges.CompanyData.RateCommodityDefaultingRules);
				}
				else if (!(Consignor?.CompanyData?.RateCommodityDefaultingRules).IsNullOrEmpty()
					&& (this.IsExport()
						|| this.IsDomestic()))
				{
					JS_RH_NKRateCommodity = GetDefaultRateCommodity(Consignor.CompanyData.RateCommodityDefaultingRules);
				}
				else if (!(Consignee?.CompanyData?.RateCommodityDefaultingRules).IsNullOrEmpty()
					&& (this.IsImport()
						|| this.IsCrossTrade()))
				{
					JS_RH_NKRateCommodity = GetDefaultRateCommodity(Consignee.CompanyData.RateCommodityDefaultingRules);
				}
				else if (!DataRegistryRating.Instance.DefaultRateCommodities.Value.IsNullOrEmpty())
				{
					JS_RH_NKRateCommodity = GetDefaultRateCommodityByShipment(
						JS_RL_NKOrigin,
						JS_RL_NKDestination,
						JS_TransportMode,
						JS_PackingMode,
						DirectionsHelper<ForwardingShipment>.FromBusinessObject(this),
						JS_RS_NKServiceLevel);
				}
			}
		}

		ZString GetDefaultRateCommodityByShipment(ZString origin, ZString destination, ZString transportMode, ZString containerMode, ZString direction, ZString serviceLevel)
		{
			var rateCommodityDefaultHelper = new OrgRateCommodityDefaultingRuleHelper();
			var rules = DataRegistryRating.Instance.DefaultRateCommodities.Value.Cast<RateCommodityDefaultingRule>().ToList();
			return rateCommodityDefaultHelper.GetDefaultRateCommodity(
				rules,
				r => r.Origin, origin,
				r => r.Destination, destination,
				r => r.TransportMode, transportMode,
				r => r.ContainerMode, containerMode,
				r => r.ServiceLevel, serviceLevel,
				r => r.Direction, direction,
				r => r.RateCommodityCode);
		}

		ZString GetDefaultRateCommodity(OrgRateCommodityDefaultingRuleCollection defaultingRules)
		{
			var rateCommodityDefaultHelper = new OrgRateCommodityDefaultingRuleHelper();
			var rules = defaultingRules.ToList();
			return rateCommodityDefaultHelper.GetDefaultRateCommodity(
				rules,
				r => r.ORC_Origin, JS_RL_NKOrigin,
				r => r.ORC_Destination, JS_RL_NKDestination,
				r => r.ORC_TransportMode, JS_TransportMode,
				r => r.ORC_ContainerMode, JS_PackingMode,
				r => r.ORC_RS_NKServiceLevel, JS_RS_NKServiceLevel,
				r => r.ORC_Direction, DirectionsHelper<ForwardingShipment>.FromBusinessObject(this),
				r => r.ORC_RH_NKCommodityCode);
		}

		void DefaultShipmentGatewaysIfNecessary()
		{
			if (ShouldDefaultGateways())
			{
				this.UpdateGateways();
			}
		}

		bool ShouldDefaultGateways()
		{
			return (!IsInDatabase
				|| (JS_RL_NKDischargePortInfo.HasChanges
				|| JS_RL_NKDestinationInfo.HasChanges
				|| JS_RL_NKLoadPortInfo.HasChanges
				|| JS_RL_NKOriginInfo.HasChanges
				|| JS_TransportModeInfo.HasChanges
				|| JS_PackingModeInfo.HasChanges))
				&& JS_IsForwardRegistered;
		}

		void ReportShipmentStatusIsEmptyOnBooking()
		{
			if (JS_ShipmentStatus.IsEmpty
				&& JS_IsBooking
				&& !Globals.IsTest
				&& !JS_IsForwardRegistered
				&& IsSea)
			{
				var message = ZString.Format("Attempting to empty HBL Booking Status: Booking PK: {0}, JS_UniqueConsignRef: {1}, IsInDatabase: {2}\r\n{3}", PK, JS_UniqueConsignRef, IsInDatabase, JS_ShipmentStatusChangeLog); // Error Reporting
				ErrorReporter.ReportOnce("ForwardingShipment|EmptyShipmentStatus", message);
			}
		}

		internal bool IsFirstAirLegFlightDeparted()
		{
			var firstAirLeg = DepartureConsol?.Transports.FirstTransportWithTransportMode(Constants.TransportModes.Air);
			if (firstAirLeg != null)
			{
				return !firstAirLeg.JW_ATD.IsEmpty && firstAirLeg.JW_ATD < ZDateTime.Now;
			}
			return false;
		}

		#region UpdateShipmentTotals

		protected override bool InnerPacksTotalsDiffer
		{
			get
			{
				if (IsHighVolumeLowValue && HVLVConsignmentHeader is IHVLVConsignmentHeader consignmentHeader)
				{
					var updateMethod = HVLVDataRegistry.Instance.HVLVShipmentWeightUpdateMethod.Value;

					if (Core.Constants.ShipmentWeightUpdateOptions.IsUpdateFromConsignmentsDetails(updateMethod))
					{
						return !(JS_TotalPackageCount == HVLVItemCount
								&& JS_ActualWeight == consignmentHeader.TotalWeight
								&& JS_ActualVolume == consignmentHeader.TotalVolume);
					}
					else if (Core.Constants.ShipmentWeightUpdateOptions.IsUpdateFromPackingDetails(updateMethod))
					{
						return !(JS_TotalPackageCount == HVLVItemCount);
					}
				}

				ReloadInnerPackLines();
				return PacksTotalsDiffer();
			}
		}

		protected override void UpdateShipmentTotals()
		{
			base.UpdateShipmentTotals();
			if (IsHighVolumeLowValue && HVLVConsignmentHeader is IHVLVConsignmentHeader consignmentHeader)
			{
				JS_TotalPackageCount = HVLVItemCount;

				if (Core.Constants.ShipmentWeightUpdateOptions.IsUpdateFromConsignmentsDetails(HVLVDataRegistry.Instance.HVLVShipmentWeightUpdateMethod.Value))
				{
					JS_ActualWeight = consignmentHeader.TotalWeight;
					JS_ActualVolume = consignmentHeader.TotalVolume;
				}
			}
			else
			{
				UpdateShipmentFromInnerPackLines();
			}
		}

		#endregion

		#region Pre-Allocation Amount Status Update
		bool forceUpdatePreAllocatedAmountExceededStatus;
		void UpdatePreAllocatedAmountExceededStatusIfNecessary()
		{
			if (forceUpdatePreAllocatedAmountExceededStatus || !IsInDatabase || AnyWeightVolumeChargeablePropertyHasChanges())
			{
				foreach (ForwardingConsol consol in Consols)
				{
					consol.UpdatePreAllocatedAmountExceededStatus();
				}
			}
		}

		bool AnyWeightVolumeChargeablePropertyHasChanges()
		{
			return
				JS_ActualVolumeInfo.HasChanges || JS_UnitOfVolumeInfo.HasChanges
				|| JS_ActualWeightInfo.HasChanges || JS_UnitOfWeightInfo.HasChanges
				|| JS_ActualChargeableInfo.HasChanges || JS_ChargeableUnitInfo.HasChanges;
		}
		#endregion

		public override void OnSaved(bool saveSucceeded)
		{
			DebugLog.AppendLine($"saveSucceeded: {saveSucceeded}");
			DebugLog.AppendLine((NoResString)"Start OnSaved.");
			LogDebugInfo();

			var oldJS_UniqueConsignRef = JS_UniqueConsignRef;
			base.OnSaved(saveSucceeded);

			if (saveSucceeded)
			{
				((IShouldUpdateScreeningStatus)this).ShouldUpdateScreeningStatus = false;
				forceUpdatePreAllocatedAmountExceededStatus = false;
				JS_ShipmentStatusChangeLog = ZString.Empty;
				JobDeclarationFilter.FetchOnlyFromLocalCache = false;
				currentDeliveryDueDateChangedFactors = DeliveryDueDateChangedFactor.None;

				JS_ActualWeightModifiedDateUtc = ZDateTime.Empty;
				JS_UnitOfWeightModifiedDateUtc = ZDateTime.Empty;
				JS_ActualVolumeModifiedDateUtc = ZDateTime.Empty;
				JS_UnitOfVolumeModifiedDateUtc = ZDateTime.Empty;

				deliveryDueDateChangeTracker = null;
				revisedDeliveryDueDateChangeTracker = null;

				screeningPartySnapshot = currentSnapshot;
			}
			else
			{
				if (!IsInDatabase)
				{
					foreach (var order in AttachedOrders)
					{
						order.Logs.UpdateEventReferenceNumbers(Events.Attached, oldJS_UniqueConsignRef, PK.ToString());
					}
				}

				if (regeneratedHouseBillLog != null && !regeneratedHouseBillLog.IsInDatabase)
				{
					regeneratedHouseBillLog.Delete();
					regeneratedHouseBillLog = null;
				}
			}

			IsPendingAllocationSetBySystem = false;

			DebugLog.AppendLine((NoResString)"End OnSaved.");
			LogDebugInfo();

			if (ShouldReportErrors())
			{
				ErrorReporter.ReportOnce("CS01154849 - TRABORALB - Converting Quoted Booking into Shipments", DebugLog.ToString());
			}
		}

		bool HasAnyBlindPackageBeenAttached()
		{
			var blindPackages = OuterPackLines.Cast<ForwardingPackLine>().Where(packLine =>
				packLine.BlindPackageAttached && !packLine.IsDeleted && !packLine.IsInDatabase);
			if (blindPackages.Any())
			{
				var supporter = (ITransitWarehouseInstructionSupporter)this;
				var isPickup = supporter.PickupTransitWarehouse != null
					&& blindPackages.Any(packLine => packLine.JL_OA_LastKnownTransitWarehouseAddress == supporter.PickupTransitWarehouse.PK);
				var isDelivery = supporter.DeliveryTransitWarehouse != null
					&& blindPackages.Any(packLine => packLine.JL_OA_LastKnownTransitWarehouseAddress == supporter.DeliveryTransitWarehouse.PK);
				if (isPickup && isDelivery)
				{
					PrepareDispatchTWInstructionForBlindPackagesDirection =
						TransitWarehouseInstructionHelper.Direction.Both;
				}
				else
				{
					PrepareDispatchTWInstructionForBlindPackagesDirection = isDelivery
						? TransitWarehouseInstructionHelper.Direction.Delivery
						: TransitWarehouseInstructionHelper.Direction.Pickup;
				}
				return true;
			}

			return false;
		}

		public bool RequireSendingPrepareDispatchTWInstructionForBlindPackages
		{
			get => requireSendingPrepareDispatchTWInstructionForBlindPackages;
			set => requireSendingPrepareDispatchTWInstructionForBlindPackages = value;
		}
		bool requireSendingPrepareDispatchTWInstructionForBlindPackages;

		public TransitWarehouseInstructionHelper.Direction PrepareDispatchTWInstructionForBlindPackagesDirection
		{
			get => prepareDispatchTWInstructionForBlindPackagesDirection;
			set => prepareDispatchTWInstructionForBlindPackagesDirection = value;
		}
		TransitWarehouseInstructionHelper.Direction prepareDispatchTWInstructionForBlindPackagesDirection;

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Search strings")]
		bool ShouldReportErrors()
		{
			if (GlbCompany.CurrentCompany?.OrgProxy?.OH_Code.ToString() != "TRABORALB")
			{
				return false;
			}

			var debugLog = DebugLog.ToString();
			return debugLog.Contains("After converting to shipment") && debugLog.Contains("After reading notes");
		}

		public void MarkAsNeedingValidationWhenControllingCustomerOrAgentRequireDefaulting()
		{
			if (LightValidationIsValid && LightValidationEnabled && (ControllingCustomerRequiresDefaulting || ControllingAgentRequiresDefaulting))
			{
				MarkAsNeedingValidation();
			}
		}

		bool ControllingCustomerRequiresDefaulting
		{
			get { return ControllingCustomerAddress.IsEmpty && FreightDataRegistry.Instance.DefaultShipmentControllingCustomer.Value; }
		}

		public void DefaultControllingCustomer()
		{
			DefaultControllingCustomer(BuyerSupplierLinksHelper?.GetControllingCustomer(), true);
		}

		public void DefaultControllingCustomer(OrgHeader controllingCustomer, bool forceDefaulting)
		{
			if ((ControllingCustomerAddress.IsEmpty || forceDefaulting) && FreightDataRegistry.Instance.DefaultShipmentControllingCustomer.Value)
			{
				if (controllingCustomer != null)
				{
					var controllingCustomerAddress = DocAddresses.FindOrCreateWithRequirement(ControllingCustomerDocAddressRequirement);
					controllingCustomerAddress.OrganisationPK = controllingCustomer.PK;
				}
				else
				{
					var precedenceRule = FreightDataRegistry.Instance.DefaultControllingCustomerRule.GetFallBackValueAtAllLevels(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty);
					var orgTypesPrecedence = precedenceRule.SelectedItems;

					foreach (EntityPrecedenceRuleItem orgTypeRuleItem in orgTypesPrecedence)
					{
						var org = GetOrganizationByOrgType(RelatedPartyTypeList.Codes.ControllingCustomer, orgTypeRuleItem.Code);
						if (org != null)
						{
							ControllingCustomerAddress.OrganisationPK = org.PK;
							break;
						}
					}
				}
			}
		}

		protected override void ControllingCustomerAddressChanged(object sender, EventArgs e)
		{
			base.ControllingCustomerAddressChanged(sender, e);
			controllingAgentMarkedForDefaulting = true;
		}

		bool controllingAgentMarkedForDefaulting;

		bool ControllingAgentRequiresDefaulting
		{
			get { return (ControllingAgentDocumentaryAddress.IsEmpty || controllingAgentMarkedForDefaulting) && FreightDataRegistry.Instance.DefaultShipmentControllingAgent.Value; }
		}

		public void DefaultControllingAgent(bool forceDefaulting)
		{
			if ((ControllingAgentDocumentaryAddress.IsEmpty || controllingAgentMarkedForDefaulting || forceDefaulting)
				&& FreightDataRegistry.Instance.DefaultShipmentControllingAgent.Value)
			{
				var precedenceRule = FreightDataRegistry.Instance.DefaultControllingAgentRule.GetFallBackValueAtAllLevels(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
				var orgTypesPrecedence = precedenceRule.SelectedItems;

				foreach (EntityPrecedenceRuleItem orgTypeRuleItem in orgTypesPrecedence)
				{
					var org = GetOrganizationByOrgType(RelatedPartyTypeList.Codes.ControllingAgent, orgTypeRuleItem.Code);

					if (org != null)
					{
						ControllingAgentDocumentaryAddress.E2_OA_Address = org.MainAddress.PK;
						break;
					}
				}
			}

			controllingAgentMarkedForDefaulting = false;
		}

		#region Regenerate House Bill On saving

		bool regenerateHouseBill;

		public void RegenerateHouseBillInSaving(bool manuallyRegenerated = false)
		{
			if (!regenerateHouseBill)
			{
				regenerateHouseBill = true;

				if (IsInDatabase)
				{
					BusinessObjectFactory.SavingEventHandler generationDelegate = null;
					generationDelegate = delegate
					{
						RegenerateHouseBillNumber(manuallyRegenerated);
						regenerateHouseBill = false;
						Factory.Saving -= generationDelegate;
					};

					Factory.Saving += generationDelegate;
				}

				if (manuallyRegenerated)
				{
					IsPendingAllocationSetBySystem = true;
					JS_HouseBill = (NoResString)"Pending Allocation.."; // Used for Hint Message
				}
			}
		}

		#endregion

		OrgHeader GetOrganizationByOrgType(string controllingPartyType, string relatedPartyType)
		{
			Argument.NotNullOrEmpty(relatedPartyType, nameof(relatedPartyType));
			Argument.NotNullOrEmpty(controllingPartyType, nameof(controllingPartyType));

			if (controllingPartyType == RelatedPartyTypeList.Codes.ControllingAgent)
			{
				if (relatedPartyType == Constants.DefaultControllingAgentOrgTypes.Code.ControllingCustomer)
				{
					var controllingCustomer = ControllingCustomer;
					if (controllingCustomer != null)
					{
						return GetRelatedParty(controllingCustomer, controllingPartyType, RelatedPartyDirectionList.Codes.Sales);
					}
				}
				else if (relatedPartyType == Constants.DefaultControllingAgentOrgTypes.Code.BillToParty)
				{
					return GetControllingCustomerAgentFromBillToParty(controllingPartyType);
				}
				else if (relatedPartyType == Constants.DefaultControllingAgentOrgTypes.Code.BookingParty && JS_IsBooking && !JS_IsForwardRegistered)
				{
					return GetControllingCustomerAgentFromBookingParty(controllingPartyType);
				}
				else if (relatedPartyType == Constants.DefaultControllingAgentOrgTypes.Code.ConsignorConsignee)
				{
					return GetControllingCustomerAgentFromConsignorOrConsignee(controllingPartyType);
				}
			}
			else if (controllingPartyType == RelatedPartyTypeList.Codes.ControllingCustomer)
			{
				if (relatedPartyType == Constants.DefaultControllingCustomerOrgTypes.Code.BookingParty && JS_IsBooking && !JS_IsForwardRegistered)
				{
					return GetControllingCustomerAgentFromBookingParty(controllingPartyType);
				}
				else if (relatedPartyType == Constants.DefaultControllingCustomerOrgTypes.Code.BillToParty)
				{
					return GetControllingCustomerAgentFromBillToParty(controllingPartyType);
				}
				else if (relatedPartyType == Constants.DefaultControllingCustomerOrgTypes.Code.ConsignorConsignee)
				{
					return GetControllingCustomerAgentFromConsignorOrConsignee(controllingPartyType);
				}
			}

			return null;
		}

		OrgHeader GetControllingCustomerAgentFromConsignorOrConsignee(string controllingPartyType)
		{
			var isPrepaid = IsPrepaid;
			var org = isPrepaid ? Consignor : Consignee;

			if (org != null)
			{
				var direction = isPrepaid ? RelatedPartyDirectionList.Codes.Pickup : RelatedPartyDirectionList.Codes.Delivery;
				var iftOrg = GetRelatedParty(org, RelatedPartyTypeList.Codes.InvoiceFreightJobsTo, direction);

				OrgHeader controllingOrg = null;

				direction = controllingPartyType == RelatedPartyTypeList.Codes.ControllingAgent
					? RelatedPartyDirectionList.Codes.Sales
					: direction;

				if (iftOrg != null && !iftOrg.OH_IsBroker && !iftOrg.OH_IsForwarder)
				{
					controllingOrg = GetRelatedParty(iftOrg, controllingPartyType, direction);
				}

				controllingOrg = controllingOrg ?? GetRelatedParty(org, controllingPartyType, direction);
				return controllingOrg;
			}

			return null;
		}

		OrgHeader GetControllingCustomerAgentFromBookingParty(string controllingPartyType)
		{
			if (JS_IsBooking && JS_IsForwardRegistered)
			{
				return null;
			}

			var quotedBookingBuilder = ObjectFactory.Get<IQuotedBookingBuilder>();
			if (quotedBookingBuilder.Load(Factory, PK) is IQuotedBooking quotedBooking
				&& Factory.Load<OrgHeader>(quotedBooking.ClientPK) is OrgHeader bookingParty)
			{
				var direction = controllingPartyType == RelatedPartyTypeList.Codes.ControllingAgent
					? RelatedPartyDirectionList.Codes.Sales
					: RelatedPartyDirectionList.Codes.Pickup;

				return GetRelatedParty(bookingParty, controllingPartyType, direction);
			}

			return null;
		}

		OrgHeader GetControllingCustomerAgentFromBillToParty(string controllingPartyType)
		{
			var job = Job;

			if (job != null)
			{
				var freightChargeCode = Factory.Load<AccChargeCode>(Env.Registry.GetFreightChargeCode(Env.CurrentCompanyPK));

				if (freightChargeCode != null)
				{
					ZGuid getDebtorPK(IJobNumber jobParent) => job.GetDebtorPK(freightChargeCode, jobParent.JobNumber);
					var debtorPk = getDebtorPK(this);

					var billToParty = Factory.Load<OrgHeader>(debtorPk);

					if (billToParty != null && !billToParty.OH_IsBroker && !billToParty.OH_IsForwarder)
					{
						var direction = controllingPartyType == RelatedPartyTypeList.Codes.ControllingAgent
							? RelatedPartyDirectionList.Codes.Sales
							: (IsPrepaid ? RelatedPartyDirectionList.Codes.Pickup : RelatedPartyDirectionList.Codes.Delivery);

						return GetRelatedParty(billToParty, controllingPartyType, direction);
					}
				}
			}

			return null;
		}

		OrgHeader GetRelatedParty(OrgHeader org, string controllingPartyType, string direction)
		{
			Argument.NotNull(org, nameof(org));
			Argument.NotNullOrEmpty(controllingPartyType, nameof(controllingPartyType));
			Argument.NotNullOrEmpty(direction, nameof(direction));

			var relatedParty = org.AllRelatedParties.GetRelatedParty(controllingPartyType, direction);
			return relatedParty != null ? relatedParty.RelatedParty : null;
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

			for (var i = AttachedOrders.Count - 1; i >= 0; i--)
			{
				var order = AttachedOrders[i];
				AttachedOrders.RemoveFromRelationship(order);

				if (!order.IsInDatabase)
				{
					order.Delete();
				}
			}

			for (var i = AttachedWarehouseOrders.Count - 1; i >= 0; i--)
			{
				var whsOrder = AttachedWarehouseOrders[i] as BusinessObject;
				if (whsOrder != null)
				{
					AttachedWarehouseOrders.RemoveFromRelationship(whsOrder);

					if (!whsOrder.IsInDatabase)
					{
						whsOrder.Delete();
					}
				}
			}

			for (var i = Gateways.Count - 1; i >= 0; i--)
			{
				var gateway = Gateways[i];
				Gateways.RemoveFromRelationship(gateway);

				if (!gateway.IsInDatabase)
				{
					gateway.Delete();
				}
			}

			CountrySpecificSupport?.Unregister();
			DeleteAllDeclarations();
			//This code is added to catch the issue described in WI00087838. Remove it please as soon as it’s resolved.
			deleteCallStack = System.Environment.StackTrace;

			if (IsInDatabase && !Globals.IsTest)
			{
				var message = string.Format(CultureInfo.InvariantCulture, "Attempting to delete a saved shipment, Related to WI00209808: Shipment PK: {0}, JS_UniqueConsignRef: {1}", PK, JS_UniqueConsignRef); // Error Reporting
				ErrorReporter.ReportOnce("ForwardingShipment|DeletingSavedShipment", message);
			}

			new GenCustomAddOnRuleAckCollection(this).DeleteAll();

			// Because AutoratingPreferences is created automatically, setting HasChanges = true
			// while cancelling a newly created shipment doesn't make sense.
			// `HasChanges = true` will propagate the value up and will stop the cancellation.
			using (AutoratingPreferences.SuspendSettingHasChanges())
			{
				AutoratingPreferences.Delete();
			}
			base.Delete();
		}

		string deleteCallStack;

		void DeleteAllDeclarations()
		{
			ZQuery query = new ZQuery(JobDeclarationSchema.JE_JS, PK);
			query.FetchOnlyFromLocalCache = true;

			IEnumerable<IBaseJobDeclaration> declarations = Factory.Load<IBaseJobDeclaration>(query);
			foreach (var declaration in declarations)
			{
				declaration.Delete();
			}
		}

		#endregion

		#region OnUniversalCopyFinish

		protected override void OnUniversalCopyFinish()
		{
			base.OnUniversalCopyFinish();

			OuterPackLines.ReloadFromLocalCache();
			foreach (var packLine in OuterPackLines.Cast<ForwardingPackLine>())
			{
				foreach (var pivotWithAbsentConShipLink in ContainerPackLineRelationshipHelper.GetContainerPackPivotsWithAbsentConShipLink(Factory, null, packLine))
				{
					if (pivotWithAbsentConShipLink.Container?.Consol != null)
					{
						using (pivotWithAbsentConShipLink.Container.Consol.DisableAutoUpdatePackLineContainers())
						{
							Consols.Add(pivotWithAbsentConShipLink.Container.Consol);
						}
					}
				}
			}
		}

		#endregion

		#region Notes / Events

		protected override BusinessObject[] BusinessObjectsWithRelatedEventsCore
		{
			get
			{
				var result = new List<BusinessObject>(base.BusinessObjectsWithRelatedEventsCore);

				if (SeaCargoJobExists())
				{
					result.Add(SeaCargoJob);
					result.AddRange(SeaCargoJobPivots);
				}

				var auHawb = AUCusHAWB as BusinessObject;
				if (auHawb != null)
				{
					result.Add(auHawb);
				}

				var gbHawbs = GBCusHAWBs;
				if (gbHawbs != null)
				{
					foreach (EnterpriseBusinessObject gbHawb in gbHawbs)
					{
						result.Add(gbHawb);
						result.AddRange(gbHawb.BusinessObjectsWithRelatedEvents);
					}
				}

				var globalManifests = GetGlobalManifestHeaders();
				if (globalManifests != null)
				{
					result.AddRange(globalManifests);
					result.AddRange(GetRelatedAsycudaBillsEvents(globalManifests.Select(x => x.PK).ToArray()));
				}

				result.AddRange(TransportBookingLoader.GetRelatedTransportBookingEvents(this));
				result.AddRange((BusinessObject[])Factory.Load<ICommonCartage>(new ZQuery(JobCartageSchema.JJ_ParentID, PK)));
				result.AddRange(AttachedWarehouseOrders.Cast<BusinessObject>());

				var inBondHeader = (EnterpriseBusinessObject)InBondHeader;
				if (inBondHeader != null)
				{
					result.Add(inBondHeader);
					result.AddRange(inBondHeader.BusinessObjectsWithRelatedEvents);
				}

				var receive = RelatedWarehouseReceive;
				if (receive != null)
				{
					result.Add(receive);
				}

				var nctsheader = (EnterpriseBusinessObject)NctsHeaderForDocuments;
				if (nctsheader != null)
				{
					result.Add(nctsheader);
					result.AddRange(nctsheader.BusinessObjectsWithRelatedEvents);
				}

				if (IsTemplate && TemplateRecord != null)
				{
					result.Add(TemplateRecord);
				}

				var documentDataLoader = ObjectFactory.Get<IVisualizerDocumentDataLoader>();
				var documentData = documentDataLoader.Load(this);

				if (documentData != null)
				{
					result.AddRange(documentData.Cast<BusinessObject>());
				}

				if (JS_ShipmentType == Constants.ShipmentTypes.HighVolumeLowValue)
				{
					var relatedMasterBills = CusRelatedEventsFinder.GetRelatedParentBizOs(this, JS_JK_MasterBillNum);
					if (!relatedMasterBills.IsNullOrEmpty())
					{
						result.AddRange(relatedMasterBills);
					}
				}

				if (this.IsExport())
				{
					var exitHeaderLoader = ObjectFactory.Get<ICusExitHeaderLoader>("EUExitControl.ICusExitHeaderLoader", Factory);
					var exitHeader = exitHeaderLoader.Load(!IsInDatabase, PK, TablePrefix).SingleOrDefault();
					if (exitHeader != null)
					{
						result.Add((BusinessObject)exitHeader);
						var exitReportQuery = new ZQuery(CusExitReportSchema.CER_ClusterKey, exitHeader.CXH_ClusterKey);
						exitReportQuery.FetchOnlyFromLocalCache = !IsInDatabase;
						var exitReports = Factory.Load<ICusExitReport>(exitReportQuery);
						if (exitReports.Length > 0)
						{
							result.AddRange(exitReports.Cast<BusinessObject>());
						}
					}
				}
				return result.ToArray();
			}
		}

		BusinessObject[] GetGlobalManifestHeaders()
		{
			var result = new List<BusinessObject>();
			foreach (ForwardingConsol consol in Consols)
			{
				var manifests = (BusinessObject[])consol.GetGlobalManifestHeaders();
				result.AddRange(manifests);
			}
			return result.ToArray();
		}

		BusinessObject[] GetRelatedAsycudaBillsEvents(ZGuid[] headerPKs)
		{
			var result = new List<BusinessObject>();
			var query = new ZQuery(AsycudaBillSchema.ABL_JS_Shipment, PK);
			query.AddToFilter(AsycudaBillSchema.ABL_AMA, headerPKs);
			var bills = (BusinessObject[])Factory.Load<ASYCUDA.IAsycudaBill>(query);
			if (bills.Length > 0)
			{
				result.AddRange(bills);
				result.AddRange(bills.OfType<EnterpriseBusinessObject>().SelectMany(bill => bill.BusinessObjectsWithRelatedEvents));
			}
			return result.ToArray();
		}

		/// <summary>
		/// USA inbond only
		/// </summary>
		public US.InBond.ICusInBondHeader InBondHeader
		{
			get { return fInBondHeader ?? (fInBondHeader = (US.InBond.ICusInBondHeader)this.GetInBondHeader(CusInBondApplicationCodeList.Codes.InBond, false)); }
		}
		US.InBond.ICusInBondHeader fInBondHeader;

		public ZBool HasInBond
		{
			get { return InBondHeader != null; }
		}

		public override BusinessObject[] BusinessObjectsWithRelatedNotes
		{
			get
			{
				var result = new List<BusinessObject>(base.BusinessObjectsWithRelatedNotes);

				if (AttachedOrders.Any())
				{
					result.AddRange(AttachedOrders);
				}

				var documentDataLoader = ObjectFactory.Get<IVisualizerDocumentDataLoader>();
				result.AddRange(documentDataLoader.Load(this).Cast<BusinessObject>());

				if (ControllingCustomer != null)
				{
					result.Add(ControllingCustomer);
				}

				return result.ToArray();
			}
		}

		public ShipmentNotesChecker NotesChecker => notesChecker ?? (notesChecker = new ShipmentNotesChecker(this));
		ShipmentNotesChecker notesChecker;

		protected override ZArchitecture.Business.Logs GetNewLogs()
		{
			var result = base.GetNewLogs();
			result.EventsThatCannotBeAdded.Add(AutoEvents.OceanCarrierBookingByTEU);

			return result;
		}

		#endregion

		#region Clone

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			if (isInUseBookingOnlyCloningMode)
			{
				args.AddExcludedColumns(BookingExcludedColumns);
			}

			var returnValue = (ForwardingShipment)base.CloneInternal(args);
			RecloneColumnIfNotExcluded(args, returnValue, JobShipmentSchema.JS_OA_ExportReceivingDepot.Name);
			RecloneColumnIfNotExcluded(args, returnValue, JobShipmentSchema.JS_OA_ImportReleaseDepot.Name);
			returnValue.SetApprovedShipperStatus(ZString.Empty);

			if (!JS_IsBooking || JS_IsForwardRegistered)
			{
				this.CopyJobCO2eTo(returnValue);
			}

			return returnValue;
		}

		void RecloneColumnIfNotExcluded(BusinessObjectCloneArgs args, ForwardingShipment clonedShipment, string columnName)
		{
			if (!args.GetExcludedColumns().Contains(columnName))
			{
				clonedShipment[columnName] = this[columnName];
			}
		}

		protected override IEnumerable<string> GetPropertiesToExcludeFromCloning()
		{
			var result = new List<string>(base.GetPropertiesToExcludeFromCloning())
			{
				JobShipmentSchema.JS_OverrideWaybillDefaults.Name,
				JobShipmentSchema.Constants.JS_TH_OneTimeQuote
			};

			return result;
		}

		public IDisposable UseBookingOnlyCloningMode()
		{
			isInUseBookingOnlyCloningMode = true;
			return new DisposableAction(() => isInUseBookingOnlyCloningMode = false);
		}
		bool isInUseBookingOnlyCloningMode;

		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
		static readonly IReadOnlyList<string> bookingExcludedColumns = new string[]
		{
			JobShipmentSchema.PK.Name,
			JobShipmentSchema.JS_IsCFSRegistered.Name,
			JobShipmentSchema.JS_JS_ColoadMasterShipment.Name,
			JobShipmentSchema.JS_Phase.Name,
			JobShipmentSchema.JS_IsHighRisk.Name,
			JobShipmentSchema.JS_BookedVesselScreeningStatus.Name,
			JobShipmentSchema.JS_CarrierContractNumber.Name,
			JobShipmentSchema.JS_RCT_CarrierContract.Name,
			JobShipmentSchema.JS_RCA_BookingAllocationLine.Name,
			JobShipmentSchema.JS_RH_NKRateCommodity.Name,
			JobShipmentSchema.JS_FMCTariffID.Name,
			JobShipmentSchema.JS_JSB_SupplierBooking.Name,
			JobShipmentSchema.JS_CLH_ContainerLoadPlan.Name,
			JobShipmentSchema.JS_InterimReceipt.Name,
			JobShipmentSchema.JS_A_RCV.Name,
		};

		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
		[SuppressMessage("Microsoft.Usage", "CA2211:NonConstantFieldsShouldNotBeVisible")]
		public static IReadOnlyList<string> BookingExcludedColumns = bookingExcludedColumns;

		protected override IEnumerable<ZString> GetJobDocAddressTypesToExcludeFromCopy()
		{
			var result = new List<ZString>(base.GetJobDocAddressTypesToExcludeFromCopy())
			{
				AutoDocAddressTypes.Codes.ConsigneeElectronicBOLAddress,
				AutoDocAddressTypes.Codes.Holder,
				AutoDocAddressTypes.Codes.Shipper,
				AutoDocAddressTypes.Codes.SurrenderParty
			};

			return result;
		}

		#endregion

		#region Related Business Objects

		#region PreAdvice

		public JobShipmentPreplanning PreAdvice
		{
			get { return Factory.LoadTop1<JobShipmentPreplanning>(new ZQuery(JobShipmentPreplanningSchema.EF_JS, PK)); }
		}

		#endregion

		#region Consols

		public new ForwardingConsolManyToManyCollection Consols
		{
			get
			{
				var consols = (ForwardingConsolManyToManyCollection)base.Consols;

				if (!consolsInitialised)
				{
					consolsInitialised = true;
					consols.CountChanged += new CollectionCountChangedEventHandler(Consols_CountChanged);
				}

				return consols;
			}
		}

		protected override ConsolCollection GetNewConsolCollection()
		{
			return new ForwardingConsolManyToManyCollection(this);
		}

		public AllMasterConsolCollection AllMasterConsols
		{
			get { return allMasterConsols ?? (allMasterConsols = new AllMasterConsolCollection(this)); }
		}
		AllMasterConsolCollection allMasterConsols;

		void Consols_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			if (e.ItemAdded)
			{
				DocsAndCartage.Validation.ValidateJP_EstimatedDelivery();
			}

			if (!IsDeleting && (DepartureConsol != previousDepartureConsol || previousDepartureConsol == null))
			{
				previousDepartureConsol = DepartureConsol;

				if (AviationSecurity.SupplyChainSecurityConfiguration.IsRecalculationNeeded(this.JS_InspectionTypeCode))
				{
					SetApprovedShipperStatus(Res.GetString("3eba7ec8-59b6-4794-bd32-143af92e3cb9", "Departure Consol has been changed"));
				}

				ReDefaultPackLineInspectionTypeCodes();

				if (!IsCopying && !fIsImportingData && !IsSettingDefaultValues)
				{
					DefaultEFreightStatusFromConsol();
				}

				DeliveryDueDateFactorHasChanged(DeliveryDueDateChangedFactor.ConsolAttached);
			}
			forceUpdatePreAllocatedAmountExceededStatus = true;
			ValidateAllUNDGsOfAllPacklinesAndRefreshFirstItemBindings();
			if (SurrenderPartyDocAddress != null)
			{
				DefaultSurrenderPartyDocAddress();
			}

			if (IsHighVolumeLowValue)
			{
				HVLVConsignmentHeader.OnConsolChanged();
			}
		}

		public void ValidateAllUNDGsOfAllPacklinesAndRefreshFirstItemBindings()
		{
			foreach (var packline in OuterPackLines.OfType<ForwardingPackLine>())
			{
				foreach (var undg in packline.UNDGs)
				{
					undg.RunPreSaveValidation();
				}
				packline.UNDGs.FirstItemForBinding.RefreshBinding();
			}
		}

		ForwardingConsol previousDepartureConsol;

		bool consolsInitialised;

		public new ForwardingConsol LocalConsol
		{
			get { return (ForwardingConsol)base.LocalConsol; }
		}

		public new ForwardingConsol ArrivalConsol
		{
			get { return (ForwardingConsol)base.ArrivalConsol; }
		}

		public new ForwardingConsol DepartureConsol
		{
			get { return base.DepartureConsol as ForwardingConsol; }
		}

		public new ForwardingConsol MostInterestingDepartureConsol
		{
			get { return (ForwardingConsol)base.MostInterestingDepartureConsol; }
		}

		public ForwardingConsol CurrentConsolForDocuments { get; set; }

		public ForwardingConsol CurrentRootConsol
		{
			get { return Consols.Cast<ForwardingConsol>().FirstOrDefault(consol => consol.IsRoot); }
		}

		public ForwardingConsol FirstLoadConsol
		{
			get
			{
				return Consols.Cast<ForwardingConsol>().FirstOrDefault(consol => consol.JK_RL_NKLoadPort == JS_RL_NKOrigin)
					?? Consols.Cast<ForwardingConsol>().FirstOrDefault(consol => (consol.MostInterestingTransportPortOfLoading?.Code ?? ZString.Empty) == JS_RL_NKOrigin);
			}
		}

		public ForwardingConsol LastDischargeConsol
		{
			get
			{
				return Consols.Cast<ForwardingConsol>().FirstOrDefault(consol => consol.JK_RL_NKDischargePort == JS_RL_NKDestination)
					?? Consols.Cast<ForwardingConsol>().FirstOrDefault(consol => (consol.MostInterestingTransportPortOfDischarge?.Code ?? ZString.Empty) == JS_RL_NKDestination);
			}
		}

		#endregion

		#region PackLines

		[ChildEditable(false)]
		public new ForwardingPackLineCollection OuterPackLines
		{
			get { return (ForwardingPackLineCollection)base.OuterPackLines; }
		}

		void HandlePacklineTEUChange(ForwardingConsol consol, CO2eStatusChangedReason reason, ZGuid containerPK)
		{
#if DEBUG
			if (OnHandlePackLineTEUChangeCalled != null)
			{
				OnHandlePackLineTEUChangeCalled(this, EventArgs.Empty);
			}
#endif

			var shipments = new List<ICO2eProvider>();

			if (consol != null && containerPK.IsValid)
			{
				var container = Factory.Load<ForwardingContainer>(containerPK);
				if (container is null)
				{
					return;
				}

				shipments.AddRange(consol.Shipments
					.OfType<ForwardingShipment>()
					.Where(shipment => container.PackLines.OfType<ForwardingPackLine>().Any(packline => packline.JL_JS == shipment.PK))
					.Cast<ICO2eProvider>());
			}

			if (!this.SkipCO2eStatusCheck() || shipments.Any(shipment => !shipment.SkipCO2eStatusCheck()))
			{
				if ((this as ICO2eProvider).RequireTEU && !IsDeleted && !IsDeleting)
				{
					this.UpdateCO2eStatusToNotCurrent(reason, IsCopying);
					shipments?.ForEach(shipment => shipment.UpdateCO2eStatusToNotCurrent(reason, IsCopying));
				}
			}
		}

#if DEBUG
		public event EventHandler OnHandlePackLineTEUChangeCalled;
#endif

		public override void OnPackLinePackedOrUnpacked(PackLine packline, CommonContainer container, bool isPacked)
		{
			var reason = new CO2eStatusChangedReason(freeTextReason: (NoResString)"PackLine Container allocation changed");
			HandlePacklineTEUChange(container.Consol as ForwardingConsol, reason, container.PK);
		}

		protected override void OnOuterPackLinesCreated()
		{
			HookPackLineEventsForRequireTEU();
			HookPackLineEventsForRequiresTemperatureControl();
		}

		void HookPackLineEventsForRequireTEU()
		{
			void UpdateCO2eStatusWhenPacklineWeightOrWeightUnitIsChanged(object s, EventArgs e)
			{
				var packLine = s as ForwardingPackLine;
				HandlePacklineTEUChange((ForwardingConsol)(packLine).CurrentConsol, new CO2eStatusChangedReason(e as ValueChangedEventArgs), packLine.JL_JC);
			}

			void UpdateCO2eStatusWhenOuterPackLinesCountChanged(object sender, CollectionCountChangedEventArgs args)
			{
				if (args.BizObject is not ForwardingPackLine packline || IsDeleted || IsDeleting)
				{
					return;
				}

				if (!this.SkipCO2eStatusCheck() && (this as ICO2eCalculationSupporter).RequireTEU && !IsDeleted && !IsDeleting)
				{
					this.UpdateCO2eStatusToNotCurrent(new CO2eStatusChangedReason(args, (NoResString)"PackLine"), IsCopying);
				}

				if (args.ItemAdded)
				{
					packline.JL_ActualWeightInfo.ValueChanged += UpdateCO2eStatusWhenPacklineWeightOrWeightUnitIsChanged;
					packline.JL_ActualWeightUQInfo.ValueChanged += UpdateCO2eStatusWhenPacklineWeightOrWeightUnitIsChanged;
				}
				else if (args.ItemRemoved)
				{
					packline.JL_ActualWeightInfo.ValueChanged -= UpdateCO2eStatusWhenPacklineWeightOrWeightUnitIsChanged;
					packline.JL_ActualWeightUQInfo.ValueChanged -= UpdateCO2eStatusWhenPacklineWeightOrWeightUnitIsChanged;
				}
			}

			foreach (var packline in OuterPackLines.Cast<ForwardingPackLine>())
			{
				packline.JL_ActualWeightInfo.ValueChanged += UpdateCO2eStatusWhenPacklineWeightOrWeightUnitIsChanged;
				packline.JL_ActualWeightUQInfo.ValueChanged += UpdateCO2eStatusWhenPacklineWeightOrWeightUnitIsChanged;
			}

			OuterPackLines.CountChanged += UpdateCO2eStatusWhenOuterPackLinesCountChanged;
		}

		void HookPackLineEventsForRequiresTemperatureControl()
		{
			var oldRequiresTemperatureControl = (this as ICO2eLegBasedSupporter).RequiresTemperatureControl;

			CO2eHelper.HookUpdateToNotCurrentEvents(this,
				OuterPackLines,
				() => oldRequiresTemperatureControl,
				n => oldRequiresTemperatureControl = n,
				provider => (provider as ICO2eLegBasedSupporter).RequiresTemperatureControl,
				packLine => [packLine.JL_RequiresTemperatureControlInfo],
				e => e switch
				{
					ValueChangedEventArgs args => new CO2eStatusChangedReason(args),
					CollectionCountChangedEventArgs args => new CO2eStatusChangedReason(args, "PackLine"),
					_ => CO2eStatusChangedReason.Empty
				});
		}

		protected override void OuterPackLines_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			base.OuterPackLines_CountChanged(sender, e);
			if (!(fIsImportingData || IsRedefaultingInspectionTypeCodesSuspended))
			{
				UpdateInspectionTypeFromPackLines();
				Validation.ValidateJS_InspectionTypeCode();
			}
		}

		#region Monitor RequireTEU

		RequireTEUMonitor RequireTEUMonitor
		{
			get
			{
				if (requireTEUMonitor == null)
				{
					requireTEUMonitor = new RequireTEUMonitor(this, () => IsCopying);
				}
				return requireTEUMonitor;
			}
		}
		RequireTEUMonitor requireTEUMonitor;

		public IDisposable MonitorRequireTEUChange(CO2eStatusChangedReason reason)
		{
			return RequireTEUMonitor.MonitorChange(reason, skip: this.SkipCO2eStatusCheck());
		}

		#endregion

		public override void UpdateInspectionTypeFromPackLines()
		{
			if (AviationSecurity.SupplyChainSecurityConfiguration.IsPackLevelScreeningAvailable(this)
				&& JS_InspectionTypeCode != BaseJobShipmentLookups.InspectionType_Approved
				&& !isLoadingOuterPackLines)
			{
				var packlines = OuterPackLines.Cast<ForwardingPackLine>();
				if (packlines.Any(p => p.JL_InspectionTypeCode.IsEmpty || p.JL_InspectionTypeCode == FreightDataRegistry.AviationSecurity_Unknown_Code))
				{
					JS_InspectionTypeCode = FreightDataRegistry.AviationSecurity_Unknown_Code;
				}
				else if (packlines.Any())
				{
					var inspection = packlines.First().JL_InspectionTypeCode;
					JS_InspectionTypeCode = packlines.All(p => p.JL_InspectionTypeCode == inspection)
						? inspection
						: BaseJobShipmentLookups.InspectionType_Screened;
				}
			}
		}

		protected override OuterPackLineCollection GetNewOuterPackLineCollectionCore()
		{
			return new ForwardingPackLineCollection(this);
		}

		protected override InnerPackLineCollection GetNewInnerPackLinesCollection()
		{
			return new ForwardingInnerPackLineCollection(this);
		}

		protected override IEnumerable<IPackLineParentChangeNotifiable> PackLinesNotifiablesCore
		{
			get
			{
				yield return InnerPackLines;
				yield return OuterPackLines;
			}
		}

		public ZBool HasDamagedPackages => OuterPackLines.Cast<ForwardingPackLine>().Any(l => l.JL_Damaged > 0);
		public ZBool HasPillagedPackages => OuterPackLines.Cast<ForwardingPackLine>().Any(l => l.JL_Pillaged > 0);
		public ZString OriginTransitWarehouseStatuses => ZString.Join(", ", OuterPackLines.Cast<ForwardingPackLine>().Select(l => l.JL_OriginTransitWarehouseStatus).Distinct().ToArray());

		#endregion PackLines

		#region DocsAndCartage

		[UniversalCopyRelatedEntity(CommaSeparatedSkipPropertiesNames = "JP_ParentID,JP_ParentTableCode", DisableCopyMethodLink = true)]
		public new ForwardingDocsAndCartage DocsAndCartage
		{
			get
			{
				var docsAndCartage = (ForwardingDocsAndCartage)base.DocsAndCartage;

				if (docsAndCartage != null && !docsAndCartage.IsDeleted)
				{
					if (!pickupCartageCoAddrInitialised)
					{
						pickupCartageCoAddrInitialised = true;
						docsAndCartage.JP_OA_PickupCartageCoAddrInfo.ValueChanged += JP_OA_PickupCartageCoAddr_ValueChanged;
						docsAndCartage.JP_PickupRequiredByInfo.ValueChanged += JP_PickupRequiredBy_ValueChanged;
						docsAndCartage.JP_PickupCartageCompletedInfo.ValueChanged += JP_PickupCartageCompleted_ValueChanged;
					}

					docsAndCartage.InitialisePhaseDependantMandatoryValidation();
				}

				return docsAndCartage;
			}
		}

		public override Type DocsAndCartageType
		{
			get { return typeof(ForwardingDocsAndCartage); }
		}

		public override Type DocsAndCartageParentType
		{
			get { return this.GetType(); }
		}

		void JP_OA_PickupCartageCoAddr_ValueChanged(object sender, EventArgs e)
		{
			AviationSecurityParty_HasChanges(SupplyChainSecurityOrganisationTypes.ShipmentPickupTransportCompany);
		}

		bool pickupCartageCoAddrInitialised;

		void JP_PickupRequiredBy_ValueChanged(object sender, EventArgs e)
		{
			if ((IsHBLContainerPackModeDOOR_X && DocsAndCartage.JP_PickupCartageCompleted.IsEmpty)
				|| (IsHBLContainerPackModeCFS_X && JS_A_RCV.IsEmpty))
			{
				DeliveryDueDateFactorHasChanged(DeliveryDueDateChangedFactor.PickupRequiredBy);
			}
		}

		void JP_PickupCartageCompleted_ValueChanged(object sender, EventArgs e)
		{
			if (IsHBLContainerPackModeDOOR_X)
			{
				DeliveryDueDateFactorHasChanged(DeliveryDueDateChangedFactor.ActualPickup);
			}
		}

		#endregion

		#region Containers

		protected override ShipmentContainerCollectionForBinding GetNewShipmentContainerCollectionForBinding()
		{
			var result = base.GetNewShipmentContainerCollectionForBinding();

			if (ChildEditableService.GetState(Factory) == ChildEditableServiceStates.Shipment)
			{
				RegisterEditableChildObject(result);
			}

			return result;
		}

		#endregion

		#region BookingContainerCollectionForBinding

		public BookingContainerCollectionForBinding BookingContainersForBinding
		{
			get
			{
				if (bookingContainersForBinding == null)
				{
					bookingContainersForBinding = new BookingContainerCollectionForBinding(this, Factory);
					bookingContainersForBinding.Load();
				}

				return bookingContainersForBinding;
			}
		}
		BookingContainerCollectionForBinding bookingContainersForBinding;

		public class BookingContainerCollectionForBinding : DependentBusinessObjectCollection<ForwardingContainer, ForwardingShipment>
		{
			public BookingContainerCollectionForBinding(ForwardingShipment shipment, BusinessObjectFactory factory)
				: base(shipment, factory)
			{
			}

			protected override SchemaGuidColumn FKSchemaColumnInDependent
			{
				get { return JobContainerSchema.JC_JS_FCLBookingOnlyLink; }
			}
		}

		#endregion

		#region Pickup/Delivery Confirmations

		public ZString DeliveryGoodsSignedForBy
		{
			get
			{
				ZString result = GetGoodsSignedForBy(DeliveryConfirms);
				if (result.IsEmpty)
				{
					result = GetContainerizedGoodsSignedForBy(Core.Constants.PickupDeliveryConfirmTypes.DestinationDelivery);
				}
				return result;
			}
		}

		public ZString PickupGoodsSignedForBy
		{
			get
			{
				ZString result = GetGoodsSignedForBy(PickupConfirms);
				if (result.IsEmpty)
				{
					result = GetContainerizedGoodsSignedForBy(Core.Constants.PickupDeliveryConfirmTypes.OriginPickup);
				}
				return result;
			}
		}

		ZString GetContainerizedGoodsSignedForBy(ZString confirmType)
		{
			List<ZString> names = new List<ZString>();
			foreach (CommonContainer container in Containers)
			{
				foreach (CommonPickupDeliveryConfirm confirm in container.Confirms)
				{
					if (confirm.EU_PickupDeliveryType == confirmType && !names.Contains(confirm.EU_GoodsSignForBy))
					{
						names.Add(confirm.EU_GoodsSignForBy);
					}
				}
			}

			if (names.Count == 1)
			{
				return names[0];
			}
			else if (names.Count > 1)
			{
				return Res.GetString("94F5F75A-3C1E-4eca-8115-D4C106A82B64", "Multiple");
			}
			else
			{
				return ZString.Empty;
			}
		}

		ZString GetGoodsSignedForBy(CommonPickupDeliveryConfirmCollection confirms)
		{
			List<ZString> names = new List<ZString>();

			foreach (CommonPickupDeliveryConfirm confirm in confirms)
			{
				if (!names.Contains(confirm.EU_GoodsSignForBy))
				{
					names.Add(confirm.EU_GoodsSignForBy);
				}
			}

			if (names.Count == 1)
			{
				return names[0];
			}
			else if (names.Count > 1)
			{
				return Res.GetString("ad7c7c4e-e3ab-4ae7-b526-1e698e0187b2", "Multiple");
			}
			else
			{
				return ZString.Empty;
			}
		}

		public override bool AutoCreateLooseConfirmations
		{
			get { return FreightConfigurationRegistry.Instance.AutoCreateLooseConfirmations.Value; }
		}

		#endregion

		#region Documentary Address Changed

		protected override void PickupAgentDocumentaryAddressChanged()
		{
			base.PickupAgentDocumentaryAddressChanged();
			Job?.SetDefaultsForJob();
		}

		protected override void ConsigneeDocumentaryAddressChanged()
		{
			using (SuspendSettingConsigneeFromDestination())
			{
				base.ConsigneeDocumentaryAddressChanged();

				if (!IsDeleted)
				{
					DocsAndCartage.MarkAsNeedingValidation(); //attached orders validation
				}

				CheckQueryAttachRelatedOrder();

				SetConsigneeValues();
				PopulateAWB(onlyIfAWBExists: true);
				HookedConsignee = Consignee;
				DocsAndCartage.Validation.ValidateJP_OrderItemsAsString();
				SetDeliveryAgent();

				RaiseConsigneeDocumentaryAddressChanged();

				if (IsMasterShipmentRepresentingAllChildShipments)
				{
					SetHandledOnBehalfOfForwarder(ShipmentPropertyChanged.Consignee);
				}
			}
		}

		protected override void ConsigneeDeliveryAddressChanged()
		{
			base.ConsigneeDeliveryAddressChanged();
			SetConsigneeValues();
			SetDeliveryCFS();
			SetDefaultSelfFiler();
			if (!ConsigneeDeliveryAddress.E2_AddressOverride
				|| ConsigneeDeliveryAddress.E2_PostcodeInfo.HasChanges
				|| ConsigneeDeliveryAddress.E2_StateInfo.HasChanges
				|| ConsigneeDeliveryAddress.E2_RN_NKCountryCodeInfo.HasChanges
				|| ConsigneeDeliveryAddress.E2_CityInfo.HasChanges)
			{
				DeliveryDueDateFactorHasChanged(DeliveryDueDateChangedFactor.DeliveryToAddress);
			}

			if (!IsDeleted)
			{
				this.UpdateCO2eStatusToNotCurrent(new CO2eStatusChangedReason((NoResString)"Consignee Delivery Address"), IsCopying);
				UpdateContainersCO2eOnConsigneeAddressChanged();
			}
		}

		protected override JobDocAddress GetNewConsigneeDocumentaryAddress()
		{
			JobDocAddress result = base.GetNewConsigneeDocumentaryAddress();
			if (result != null && IsPropertyReadOnlyDueToPhase(Schema.ConsigneeNameOrPK))
			{
				result.SetReadOnlyIncludingChildren(true);
			}

			hookedConsignee = (result == null || !result.HasRealOrganisation)
							? null
							: result.Organisation;

			return result;
		}

		public void SetDeliveryCFS()
		{
			if (FreightDataRegistry.Instance.CalculateDeliveryDueDateByTransportMode.Value.IsTransportModeActive(JS_TransportMode))
			{
				if (defaultCFSForDeliveryDueDateDeferred)
				{
					requireFireDeferredDefaultCFSForDeliveryDueDate = true;
					return;
				}
#if DEBUG
				if (OnDefaultDeliveryCFSForDeliveryDueDate != null)
				{
					OnDefaultDeliveryCFSForDeliveryDueDate(this, EventArgs.Empty);
				}
#endif
				if (ConsigneeDeliveryAddress != null && !ConsigneeDeliveryAddress.Postcode.IsEmpty)
				{
					var transitTimeZoneOwnerAddress = GetRelatedZoneOwnerAddress(
						isDelivery: true,
						postcode: ConsigneeDeliveryAddress.Postcode,
						city: ConsigneeDeliveryAddress.City,
						state: ConsigneeDeliveryAddress.E2_State,
						country: ConsigneeDeliveryAddress.E2_RN_NKCountryCode
						);
					if (transitTimeZoneOwnerAddress != null)
					{
						JS_OA_ImportReleaseDepot_ZAddress.AddressFK = transitTimeZoneOwnerAddress.PK;
						return;
					}
				}
			}

			var defaultDepot = Consignee?.GetRelatedParty(RelatedPartyTypeList.Codes.ClientCFS, RelatedPartyDirectionList.Codes.Delivery, JS_TransportMode, JS_PackingMode, JS_RL_NKDestination);
			if (defaultDepot != null)
			{
				JS_OA_ImportReleaseDepot = defaultDepot.GetAddressWithFallback(JS_OA_ImportReleaseDepot_ZAddress.DefaultAddressType)?.PK ?? ZGuid.Empty;
			}
		}

		public void SetDeliveryAgent()
		{
			if (Consignee != null)
			{
				var agent = Consignee.GetRelatedParty(RelatedPartyTypeList.Codes.DeliveryAgent, RelatedPartyDirectionList.Codes.Delivery, JS_TransportMode, JS_PackingMode, JS_RL_NKDestination);

				if (agent != null)
				{
					JS_OH_DeliveryAgent = agent.PK;
				}
			}
		}

		public void SetPickupAgent()
		{
			if (Consignor != null)
			{
				var agent = Consignor.GetRelatedParty(RelatedPartyTypeList.Codes.PickupAgent, RelatedPartyDirectionList.Codes.Pickup, JS_TransportMode, JS_PackingMode, JS_RL_NKOrigin);

				if (agent != null)
				{
					JobDocAddress agentAddress = DocAddresses.FindOrCreateWithRequirement(PickupAgentDocAddressRequirement);
					agentAddress.OrganisationPK = agent.PK;
				}
			}
		}

		public void SetDefaultSelfFiler()
		{
			if (!ContainsDirectConsol())
			{
				return;
			}

			RemovePreviousSelfFiler();

			var directConsol = Consols.Cast<ForwardingConsol>().FirstOrDefault(c => c.IsDirect);
			if (directConsol == null || !directConsol.IsICS2)
			{
				return;
			}

			var selfFilerOrg = (Consignee?.MiscServ?.OM_IMAdvanceCargoReportingSelfFiler ?? false) ? Consignee : GetFallbackSelfFilerOrg(directConsol);
			if (selfFilerOrg != null)
			{
				var selfFilerAddressType = directConsol.DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.SelfFiler);
				selfFilerAddressType.OrganisationPK = selfFilerOrg.PK;
				selfFilerAddressType.ReadOnlyStrategy = new JobDocAddressReadOnlyStrategy(() => true, () => true);
			}
		}

		void RemovePreviousSelfFiler()
		{
			if (!ContainsDirectConsol())
			{
				return;
			}

			var directConsol = Consols.Cast<ForwardingConsol>().FirstOrDefault(c => c.IsDirect);
			directConsol?.RemovePreviousSelfFiler();
		}

		OrgHeader GetFallbackSelfFilerOrg(ForwardingConsol directConsol)
		{
			if (Consignee == null)
			{
				return null;
			}

			var matchedOrgPK = directConsol.GetMatchedOrgRelatedPartyRecord(Consignee)?.PR_OH_RelatedParty;
			return matchedOrgPK.HasValue ? Factory.Load<OrgHeader>(matchedOrgPK.Value) : null;
		}

		void SetPickupAddress()
		{
			if (ConsignorPickupAddress == null || (BuyerSupplierLinksHelper?.RestoreOverridePickupAddress() ?? false))
			{
				return;
			}

			var relatedParty = Consignor?.AllRelatedParties.GetRelatedParty(RelatedPartyTypeList.Codes.PickupFrom, RelatedPartyDirectionList.Codes.Pickup, JS_TransportMode, JS_PackingMode, JS_RL_NKOrigin);
			if (relatedParty == null)
			{
				return;
			}

			var addressPK = relatedParty.PR_OA;
			if (addressPK.IsEmpty)
			{
				addressPK = relatedParty.RelatedParty?.GetAddressWithFallback(AddressType.PIC)?.PK ?? ZGuid.Empty;
			}

			if (!addressPK.IsEmpty)
			{
				ConsignorPickupAddress.E2_OA_Address = addressPK;
			}
		}

		void SetDeliveryAddress()
		{
			if (ConsigneeDeliveryAddress == null || (BuyerSupplierLinksHelper?.RestoreOverrideDeliveryAddress() ?? false))
			{
				return;
			}

			var relatedParty = Consignee?.AllRelatedParties.GetRelatedParty(RelatedPartyTypeList.Codes.DeliveryTo, RelatedPartyDirectionList.Codes.Delivery, JS_TransportMode, JS_PackingMode, JS_RL_NKDestination);
			if (relatedParty == null)
			{
				return;
			}

			var addressPK = relatedParty.PR_OA;
			if (addressPK.IsEmpty)
			{
				addressPK = relatedParty.RelatedParty?.GetAddressWithFallback(AddressType.DLV)?.PK ?? ZGuid.Empty;
			}

			if (!addressPK.IsEmpty)
			{
				ConsigneeDeliveryAddress.E2_OA_Address = addressPK;
			}
		}

		protected override void SetDefaultNotifyParty()
		{
			if (BuyerSupplierLinksHelper?.RestoreOverrideNotifyPartyAddress() ?? false)
			{
				return;
			}

			var relatedParty = (Consignee?.AllRelatedParties.GetRelatedParty(RelatedPartyTypeList.Codes.NotifyParty, RelatedPartyDirectionList.Codes.Delivery, JS_TransportMode, JS_PackingMode))
				?? (Consignor?.AllRelatedParties.GetRelatedParty(RelatedPartyTypeList.Codes.NotifyParty, RelatedPartyDirectionList.Codes.Pickup, JS_TransportMode, JS_PackingMode));

			if (relatedParty != null)
			{
				var addressPK = relatedParty.PR_OA;
				if (addressPK.IsEmpty)
				{
					addressPK = relatedParty.RelatedParty?.GetAddressWithFallback(AddressType.OFC)?.PK ?? ZGuid.Empty;
				}

				if (!addressPK.IsEmpty)
				{
					NotifyPartyDocumentaryAddress.E2_OA_Address = addressPK;
					NotifyPartyDocumentaryAddress.ContactPK = DefaultContactFinder.GetDefaultNotifyPartyContact(relatedParty.RelatedParty);
					return;
				}
			}

			base.SetDefaultNotifyParty();
		}

		public event EventHandler ConsigneeDocumentaryAddressValueChanged;

		void RaiseConsigneeDocumentaryAddressChanged()
		{
			if (ConsigneeDocumentaryAddressValueChanged != null)
			{
				ConsigneeDocumentaryAddressValueChanged(this, EventArgs.Empty);
			}
		}

		protected override void ConsignorDocumentaryAddressChanged()
		{
			using (SuspendSettingConsignorFromOrigin())
			{
				base.ConsignorDocumentaryAddressChanged();

				if (!IsDeleted)
				{
					DocsAndCartage.MarkAsNeedingValidation(); //attached orders validation
				}

				CheckQueryAttachRelatedOrder();

				SetConsignorValues();
				PopulateAWB(onlyIfAWBExists: true);

				AviationSecurityParty_HasChanges(SupplyChainSecurityOrganisationTypes.Consignor);

				HookedConsignor = Consignor;
				SetPickupAgent();

				if (Job != null)
				{
					Job.SetDefaultsForJob();
				}

				RaiseConsignorDocumentaryAddressChanged();

				if (IsMasterShipmentRepresentingAllChildShipments)
				{
					SetHandledOnBehalfOfForwarder(ShipmentPropertyChanged.Consignor);
				}
			}
		}

		protected override JobDocAddress GetNewConsignorDocumentaryAddress()
		{
			JobDocAddress result = base.GetNewConsignorDocumentaryAddress();
			if (result != null && IsPropertyReadOnlyDueToPhase(Schema.ConsignorNameOrPK))
			{
				result.SetReadOnlyIncludingChildren(true);
			}

			hookedConsignor = (result == null || !result.HasRealOrganisation)
					? null
					: result.Organisation;

			return result;
		}

		public void SetPickupCFS()
		{
			if (FreightDataRegistry.Instance.CalculateDeliveryDueDateByTransportMode.Value.IsTransportModeActive(JS_TransportMode))
			{
				if (defaultCFSForDeliveryDueDateDeferred)
				{
					requireFireDeferredDefaultCFSForDeliveryDueDate = true;
					return;
				}
#if DEBUG
				if (OnDefaultPickupCFSForDeliveryDueDate != null)
				{
					OnDefaultPickupCFSForDeliveryDueDate(this, EventArgs.Empty);
				}
#endif

				if (ConsignorPickupAddress != null && !ConsignorPickupAddress.Postcode.IsEmpty)
				{
					var transitTimeZoneOwnerAddress = GetRelatedZoneOwnerAddress(
						isDelivery: false,
						postcode: ConsignorPickupAddress.Postcode,
						city: ConsignorPickupAddress.City,
						state: ConsignorPickupAddress.E2_State,
						country: ConsignorPickupAddress.E2_RN_NKCountryCode);
					if (transitTimeZoneOwnerAddress != null)
					{
						JS_OA_ExportReceivingDepot_ZAddress.AddressFK = transitTimeZoneOwnerAddress.PK;
						return;
					}
				}
			}

			var defaultDepot = Consignor?.GetRelatedParty(RelatedPartyTypeList.Codes.ClientCFS, RelatedPartyDirectionList.Codes.Pickup, JS_TransportMode, JS_PackingMode, JS_RL_NKOrigin);
			if (defaultDepot != null)
			{
				JS_OA_ExportReceivingDepot = defaultDepot.GetAddressWithFallback(JS_OA_ExportReceivingDepot_ZAddress.DefaultAddressType)?.PK ?? ZGuid.Empty;
			}
		}

		OrgAddress GetRelatedZoneOwnerAddress(bool isDelivery, ZString postcode, ZString city, ZString state, ZString country)
		{
			var relatedTransportZoneOwner = DeliveryDueDateCalculationHelper.GetRelatedTransportZoneOwner(postcode, city, state, country, Factory);
			return relatedTransportZoneOwner == null ? null : RelatedAddress(relatedTransportZoneOwner, isDelivery ? OrgAddressType.Delivery.Code : OrgAddressType.Pickup.Code);
		}

		OrgAddress RelatedAddress(OrgHeader org, ZString capabilityType)
		{
			var preferredAddress =
				org.Addresses.Cast<OrgAddress>().FirstOrDefault(address => (address.AddressCapability.GetAddressCapabilityOnCode(capabilityType)?.Enabled ?? false) && address.OA_IsActive);

			if (preferredAddress != null)
			{
				return preferredAddress;
			}

			var pickupAndDeliveryAddress =
				org.Addresses.Cast<OrgAddress>().FirstOrDefault(address =>
					(address.AddressCapability.GetAddressCapabilityOnCode(OrgAddressType.PickupAndDelivery.Code)?.Enabled ?? false) && address.OA_IsActive);
			if (pickupAndDeliveryAddress != null)
			{
				return pickupAndDeliveryAddress;
			}

			return org.MainAddress;
		}

		public IDisposable DeferDefaultCFSForDeliveryDueDate()
		{
			defaultCFSForDeliveryDueDateDeferred = true;
			requireFireDeferredDefaultCFSForDeliveryDueDate = false;
			return new DisposableAction(() =>
			{
				defaultCFSForDeliveryDueDateDeferred = false;
				if (requireFireDeferredDefaultCFSForDeliveryDueDate)
				{
					SetPickupCFS();
					SetDeliveryCFS();
					requireFireDeferredDefaultCFSForDeliveryDueDate = false;
				}
			});
		}

		bool defaultCFSForDeliveryDueDateDeferred;
		bool requireFireDeferredDefaultCFSForDeliveryDueDate;

#if DEBUG
		public event EventHandler OnDefaultPickupCFSForDeliveryDueDate;
		public event EventHandler OnDefaultDeliveryCFSForDeliveryDueDate;
#endif

		public event EventHandler ConsignorDocumentaryAddressValueChanged;

		void RaiseConsignorDocumentaryAddressChanged()
		{
			if (ConsignorDocumentaryAddressValueChanged != null)
			{
				ConsignorDocumentaryAddressValueChanged(this, EventArgs.Empty);
			}
		}

		void JH_OA_LocalChargesAddrInfo_ValueChanged(object sender, EventArgs e)
		{
			var job = (sender as JobHeader) ?? Job;
			if (job != null && !job.IsDeleted && PreviousJH_OH_LocalCharges != job.LocalChargesPK)
			{
				PreviousJH_OH_LocalCharges = job.LocalChargesPK;
				AviationSecurityParty_HasChanges(SupplyChainSecurityOrganisationTypes.LocalClient);
			}
		}

		ZGuid PreviousJH_OH_LocalCharges;

		protected override void ConsignorDocumentaryAddressContentsChanged()
		{
			base.ConsignorDocumentaryAddressContentsChanged();
			SetConsignorValues();
		}

		protected override void ConsigneeDocumentaryAddressContentsChanged()
		{
			base.ConsigneeDocumentaryAddressContentsChanged();
			SetConsigneeValues();
			Job?.SetDefaultsForJob();
		}

		protected override JobDocAddress GetNotifyPartyDocumentaryAddress()
		{
			JobDocAddress result = base.GetNotifyPartyDocumentaryAddress();
			if (result != null)
			{
				result.DocAddressChanged += new EventHandler(NotifyPartyDocAddress_Changed);
			}

			return result;
		}

		void NotifyPartyDocAddress_Changed(object sender, EventArgs e)
		{
			PopulateAWB(onlyIfAWBExists: true);
		}

		#endregion

		#region RecalculateRelatedParties

		public (bool WasSuccessful, ZString Log) RecalculateRelatedParties()
		{
			if (!this.IsExport() && !this.IsImport())
			{
				return (false,
					Res.GetString("b18f0e34-1aa3-48da-a490-2f330ded1f6e",
						"You cannot Recalculate Related Parties for this company because the company you are logged in does not match Pickup or Delivery directions of this job."));
			}

			RecalculateExportParties();
			RecalculateImportParties();

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
			}
		}

		void RedefaultExportBroker()
		{
			if (!JS_OH_ExportBroker.IsEmpty)
			{
				var args = RaiseAttemptEvent(ResString.GetMultilingualString("082255c1-6dbb-4aba-874e-d5e361982689", "Export Broker"));
				if (!args.Cancel)
				{
					SetDefaultExportBroker();
				}
			}
			else
			{
				SetDefaultExportBroker();
			}
		}

		void RedefaultPickupCompany()
		{
			if (!DocsAndCartage.JP_OA_PickupCartageCoAddr_ZAddress.OrgPK.IsEmpty)
			{
				var args = RaiseAttemptEvent(ResString.GetMultilingualString("64f3b767-5182-4393-af8b-4a926a7b7c31", "Pickup Company"));
				if (!args.Cancel)
				{
					DefaultPickupCompany();
				}
			}
			else
			{
				DefaultPickupCompany();
			}
		}

		void RedefaultPickupCFS()
		{
			if (!JS_OA_ExportReceivingDepot.IsEmpty)
			{
				var args = RaiseAttemptEvent(ResString.GetMultilingualString("9839c08b-0fcf-4dbf-b45b-94d1224f7b2b", "Pickup CFS"));
				if (!args.Cancel)
				{
					SetPickupCFS();
				}
			}
			else
			{
				SetPickupCFS();
			}
		}

		void RedefaultPickupAgent()
		{
			if (!PickupAgentDocumentaryAddress.OrganisationPK.IsEmpty)
			{
				var args = RaiseAttemptEvent(ResString.GetMultilingualString("399bdf5e-b0e9-48bb-8d32-bf7670c41d5a", "Pickup Agent"));
				if (!args.Cancel)
				{
					SetPickupAgent();
				}
			}
			else
			{
				SetPickupAgent();
			}
		}

		void RecalculateImportParties()
		{
			if (this.IsImport())
			{
				RedefaultImportBroker();
				RedefaultDeliveryCompany();
				RedefaultDeliveryCFS();
				RedefaultDeliveryAgent();
			}
		}

		void RedefaultImportBroker()
		{
			if (!JS_OH_ImportBroker.IsEmpty)
			{
				var args = RaiseAttemptEvent(ResString.GetMultilingualString("1be0584b-413e-457a-854a-b93dcff1c070", "Import Broker"));
				if (!args.Cancel)
				{
					SetDefaultImportBroker();
				}
			}
			else
			{
				SetDefaultImportBroker();
			}
		}

		void RedefaultDeliveryCompany()
		{
			if (!DocsAndCartage.JP_OA_DeliveryCartageCoAddr_ZAddress.OrgPK.IsEmpty)
			{
				var args = RaiseAttemptEvent(ResString.GetMultilingualString("6964f366-1192-4d04-9fe0-13678c2271bf", "Delivery Company"));
				if (!args.Cancel)
				{
					DefaultDeliveryCompany();
				}
			}
			else
			{
				DefaultDeliveryCompany();
			}
		}

		void RedefaultDeliveryCFS()
		{
			if (!JS_OA_ImportReleaseDepot.IsEmpty)
			{
				var args = RaiseAttemptEvent(ResString.GetMultilingualString("f89901df-30c0-4256-95c2-6b9629bf8733", "Delivery CFS"));
				if (!args.Cancel)
				{
					SetDeliveryCFS();
				}
			}
			else
			{
				SetDeliveryCFS();
			}
		}

		void RedefaultDeliveryAgent()
		{
			if (!JS_OH_DeliveryAgent.IsEmpty)
			{
				var args = RaiseAttemptEvent(ResString.GetMultilingualString("0f65c51d-b45c-4b81-8c52-37c48dff2da3", "Delivery Agent"));
				if (!args.Cancel)
				{
					SetDeliveryAgent();
				}
			}
			else
			{
				SetDeliveryAgent();
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

		#region Creating User

		public GlbStaff SystemCreateUser
		{
			get { return Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, JS_SystemCreateUser); }
		}

		#endregion

		#region CountriesOfRouting -  Transport countries in order

		public
#if DEBUG
 virtual
#endif
 List<ZString> CountriesOfRouting
		{
			get
			{
				List<Transport> legs = new List<Transport>();
				foreach (Transport leg in TransportsInLegOrder)
				{
					legs.Add(leg);
				}
				return GetCountriesOfRoutingFromTransportLegs(legs);
			}
		}
		#endregion

		public static List<ZString> GetCountriesOfRoutingFromTransportLegs(IEnumerable<Transport> legs)
		{
			List<ZString> countries = new List<ZString>();
			foreach (Transport trans in legs)
			{
				ZString depCountry = ZString.Empty;
				ZString arrCountry = ZString.Empty;
				if (trans.LoadPort != null && trans.LoadPort.Country != null)
				{
					depCountry = trans.LoadPort.RL_RN_NKCountryCode;
				}
				if (trans.DiscPort != null && trans.DiscPort.Country != null)
				{
					arrCountry = trans.DiscPort.RL_RN_NKCountryCode;
				}

				if (!depCountry.IsEmpty && !countries.Contains(depCountry))
				{
					countries.Add(depCountry);
				}
				if (!arrCountry.IsEmpty && !countries.Contains(arrCountry))
				{
					countries.Add(arrCountry);
				}
			}
			return countries;
		}

		#region TransportsInLegOrder

		public ForwardingTransportCollection TransportsInLegOrder
		{
			get
			{
				if (transportLegs == null)
				{
					transportLegs = new ForwardingTransportCollection(Factory);

					foreach (ForwardingConsol consol in Consols)
					{
						foreach (Transport transport in consol.Transports)
						{
							transportLegs.Add(transport);
						}
					}

					transportLegs.Sort(MovementLegComparer.PortsAndDatesBased(transportLegs));
					Consols.HasChangesChanged += new EventHandler<HasChangesChangedEventArgs>(Consols_HasChangesChanged);
				}
				return transportLegs;
			}
		}

		void Consols_HasChangesChanged(object sender, HasChangesChangedEventArgs e)
		{
			transportLegs = null;
		}

		ForwardingTransportCollection transportLegs;

		#endregion

		#region Consignee / Consignor Documentary Addresses

		#region Address Override Security

		protected override SecurityCheckpoint DefaultGetCanOverrideAddressCheckpoint
		{
			get { return Env.Security.MaintainShipmentShipments; }
		}

		protected override SecurityCheckpoint ConsigneeAddressOverrideCheckpoint
		{
			get
			{
				SecurityCheckpoint result;

				if (this.IsImport())
				{
					switch (JS_TransportMode)
					{
						case Constants.TransportModes.Air:
						case Constants.TransportModes.AirSea:
							result = Env.Security.MaintainShipmentImpShipImpAirConsigneeD;
							break;

						case Constants.TransportModes.Sea:
						case Constants.TransportModes.SeaAir:
							result = Env.Security.MaintainShipmentImpShipImpSeaConsigneeD;
							break;

						case Constants.TransportModes.Rail:
							result = Env.Security.MaintainShipmentImpShipImpRailConsigneeD;
							break;

						case Constants.TransportModes.Road:
							result = Env.Security.MaintainShipmentImpShipImpRoadConsigneeD;
							break;

						default:
							result = Env.Security.MaintainShipmentImpShipImpOtConsignee;
							break;
					}
				}
				else if (this.IsExport())
				{
					switch (JS_TransportMode)
					{
						case Constants.TransportModes.Air:
						case Constants.TransportModes.AirSea:
							result = Env.Security.MaintainShipmentExpShipExpAirConsigneeD;
							break;

						case Constants.TransportModes.Sea:
						case Constants.TransportModes.SeaAir:
							result = Env.Security.MaintainShipmentExpShipExpSeaConsigneeD;
							break;

						case Constants.TransportModes.Rail:
							result = Env.Security.MaintainShipmentExpShipExpRailConsigneeD;
							break;

						case Constants.TransportModes.Road:
							result = Env.Security.MaintainShipmentExpShipExpRoadConsigneeD;
							break;

						default:
							result = Env.Security.MaintainShipmentExpShipExpOtConsignee;
							break;
					}
				}
				else
				{
					result = Env.Security.MaintainShipmentShipments;
				}

				return result;
			}
		}

		protected override SecurityCheckpoint ConsignorAddressOverrideCheckpoint
		{
			get
			{
				SecurityCheckpoint result;

				if (this.IsImport())
				{
					switch (JS_TransportMode)
					{
						case Constants.TransportModes.Air:
						case Constants.TransportModes.AirSea:
							result = Env.Security.MaintainShipmentImpShipImpAirConsignorD;
							break;

						case Constants.TransportModes.Sea:
						case Constants.TransportModes.SeaAir:
							result = Env.Security.MaintainShipmentImpShipImpSeaConsignorD;
							break;

						case Constants.TransportModes.Rail:
							result = Env.Security.MaintainShipmentImpShipImpRailConsignorD;
							break;

						case Constants.TransportModes.Road:
							result = Env.Security.MaintainShipmentImpShipImpRoadConsignorD;
							break;

						default:
							result = Env.Security.MaintainShipmentImpShipImpOtConsignor;
							break;
					}
				}
				else if (this.IsExport())
				{
					switch (JS_TransportMode)
					{
						case Constants.TransportModes.Air:
						case Constants.TransportModes.AirSea:
							result = Env.Security.MaintainShipmentExpShipExpAirConsignorD;
							break;

						case Constants.TransportModes.Sea:
						case Constants.TransportModes.SeaAir:
							result = Env.Security.MaintainShipmentExpShipExpSeaConsignorD;
							break;

						case Constants.TransportModes.Rail:
							result = Env.Security.MaintainShipmentExpShipExpRailConsignorD;
							break;

						case Constants.TransportModes.Road:
							result = Env.Security.MaintainShipmentExpShipExpRoadConsignorD;
							break;

						default:
							result = Env.Security.MaintainShipmentExpShipExpOtConsignor;
							break;
					}
				}
				else
				{
					result = Env.Security.MaintainShipmentShipments;
				}

				return result;
			}
		}

		#endregion

		#endregion

		#region Co-Load Master Shipments

		protected override void AdjustCoLoadMasterListFilterAndFilterBusinessObjectDefaults(IRelatedShipmentsCollection masterShipmentCollection)
		{
			var provider = new ForwardingShipmentDefaultFilterProvider
			{
				ShipmentType = ForwardingShipmentDefaultFilterProvider.ShipmentTypes.AssemblyMaster
								| ForwardingShipmentDefaultFilterProvider.ShipmentTypes.ColoadMaster
								| ForwardingShipmentDefaultFilterProvider.ShipmentTypes.BuyersConsolLead
								| ForwardingShipmentDefaultFilterProvider.ShipmentTypes.Standard
			};

			if (Consols.Count > 0)
			{
				provider.ConsolNo = Consols[0].JK_UniqueConsignRef;
			}

			if (masterShipmentCollection is IFilterBusinessObjectDefaultsProvider defaultsProvider)
			{
				provider.SetDefaultFilters(defaultsProvider);
			}
		}

		#endregion

		#region Co-Load Shipments

		[List("Lookups.CoLoadShipment_List")]
		public new CoLoadForwardingShipmentCollection CoLoadShipments
		{
			get { return (CoLoadForwardingShipmentCollection)base.CoLoadShipments; }
		}

		protected override CoLoadShipmentCollection GetNewCoLoadShipmentCollection()
		{
			return new CoLoadForwardingShipmentCollection(this, Factory);
		}

		#endregion

		#region Related Shipments

		protected override void AddRelatedShipmentCollectionFilterBusinessObjectDefaultsCore(IRelatedShipmentsCollection relatedShipmentsCollection)
		{
			ForwardingShipmentDefaultFilterProvider provider = new ForwardingShipmentDefaultFilterProvider();
			provider.ShipmentType = ForwardingShipmentDefaultFilterProvider.ShipmentTypes.AssemblyMaster | ForwardingShipmentDefaultFilterProvider.ShipmentTypes.Standard;

			if (Consols.Count > 0)
			{
				provider.ConsolNo = Consols[0].JK_UniqueConsignRef;
			}

			if (relatedShipmentsCollection is IFilterBusinessObjectDefaultsProvider defaultsProvider)
			{
				provider.SetDefaultFilters(defaultsProvider);
			}
		}

		#endregion

		#region Services

		public JobServiceDependentCollection Services
		{
			get { return DocsAndCartage.Services; }
		}

		#endregion

		#region CusHAWBs

		IEnumerable<THAWB> GetHAWBs<THAWB>(bool reloadExistingRows, params ZString[] appCodes)
			where THAWB : class, ICusHAWB
		{
			Func<THAWB, bool> hawbHasOneOfApplicationCodes = hawb =>
			{
				var mawb = hawb.MAWB;
				return mawb != null && appCodes.Contains(mawb.CM_ApplicationCode);
			};
			return Factory.Load<ICusHAWB>(GetHAWBsFilter(reloadExistingRows))
							.OfType<THAWB>()
							.Where(hawbHasOneOfApplicationCodes)
							.ToArray();
		}

		ZQuery GetHAWBsFilter(bool reloadExistingRows)
		{
			var result = new ZQuery(CusHAWBSchema.CS_JS, PK)
			{
				ReLoadExistingRows = reloadExistingRows
			};
			result.AddToFilter(CusHAWBSchema.CS_CM, SQLComparisonOperator.NotEqual, DBNull.Value);
			result.OrderBy = CusHAWBSchema.Constants.CS_SystemCreateTimeUtc;
			return result;
		}

		public IAUCusHAWB AUCusHAWB
		{
			get
			{
				IAUCusHAWB result = null;

				if (IsCurrentCompanyOfCountry(Core.Constants.CountryCodes.Australia) && HasDischargeConsolInCountry(Core.Constants.CountryCodes.Australia))
				{
					var allHouseBills = Factory.Load<ICusHAWB>(GetHAWBsFilter(false));
					result = allHouseBills.OfType<IAUCusHAWB>().FirstOrDefault();
				}

				return result;
			}
		}

		public IAUCusSCAHouse AUCusSCAHouse
		{
			get
			{
				IAUCusSCAHouse result = null;

				if (IsCurrentCompanyOfCountry(Core.Constants.CountryCodes.Australia) && HasDischargeConsolInCountry(Core.Constants.CountryCodes.Australia))
				{
					var query = new ZQuery(CusSCAHouseSchema.CA_JS, PK);
					query.AddToFilter(CusSCAHouseSchema.CA_CB, SQLComparisonOperator.NotEqual, DBNull.Value);
					query.OrderBy = CusSCAHouseSchema.Constants.CA_SystemCreateTimeUtc;

					var allHouseBills = Factory.Load<Shared.IBaseCusSCAHouse>(query);
					result = allHouseBills.OfType<IAUCusSCAHouse>().FirstOrDefault();
				}

				return result;
			}
		}

		bool IsCurrentCompanyOfCountry(string countryCode)
		{
			return GlbCompany.CurrentCompany.GC_RN_NKCountryCode == countryCode;
		}

		bool HasDischargeConsolInCountry(string countryCode)
		{
			return Consols.Cast<ForwardingConsol>().Any(x => x.JK_RL_NKDischargePort.StartsWith(countryCode, StringComparison.Ordinal));
		}

		#endregion

		#region Gateways

		[ChildEditable(true)]
		public ShipmentGatewayCollection Gateways
		{
			get
			{
				if (gateways == null)
				{
					gateways = new ShipmentGatewayCollection(this);
					RegisterEditableChildObject(gateways);
				}

				return gateways;
			}
		}
		ShipmentGatewayCollection gateways;

		public void SynchronizeShipmentGatewayFromConsols(ForwardingConsol consol, ZGuid removedConsolGatewayAgentAddress, bool isDetachedConsol)
		{
			if (!consol.JK_SendingForwarderHandlingType.IsEmpty || !consol.JK_ReceivingForwarderHandlingType.IsEmpty || !removedConsolGatewayAgentAddress.IsEmpty)
			{
				var sortedGatewaysAddressList = OrderGatewaysListByConsols(consol, removedConsolGatewayAgentAddress, isDetachedConsol);

				SetGatewaysLength(sortedGatewaysAddressList.Count);

				for (int i = 0; i < Gateways.Count; i++)
				{
					if (Gateways[i].JSG_OA_ForwarderAddress != sortedGatewaysAddressList[i])
					{
						Gateways[i].JSG_OA_ForwarderAddress = sortedGatewaysAddressList[i];
					}
				}

				this.ReorderGateways();

				ValidateGatewayAddresses();
			}
		}

		List<ZGuid> OrderGatewaysListByConsols(ForwardingConsol consol, ZGuid removedConsolGatewayAgentAddress, bool isDetachedConsol)
		{
			var allGatewaysPKList = GetAllUnorderedGatewayPKs(consol, removedConsolGatewayAgentAddress, isDetachedConsol);
			var sortedConsols = Consols.Cast<ForwardingConsol>().ToArray();
			MovementLegComparer.SortMovementLegsByPorts(sortedConsols);
			var gatewaysOrderedList = new List<ZGuid>();

			foreach (var sortedConsol in sortedConsols)
			{
				if (!sortedConsol.JK_SendingForwarderHandlingType.IsEmpty && !sortedConsol.JK_OA_SendingForwarderAddress.IsEmpty && !gatewaysOrderedList.Contains(sortedConsol.JK_OA_SendingForwarderAddress))
				{
					gatewaysOrderedList.Add(sortedConsol.JK_OA_SendingForwarderAddress);
				}

				if (!sortedConsol.JK_ReceivingForwarderHandlingType.IsEmpty && !sortedConsol.JK_OA_ReceivingForwarderAddress.IsEmpty && !gatewaysOrderedList.Contains(sortedConsol.JK_OA_ReceivingForwarderAddress))
				{
					gatewaysOrderedList.Add(sortedConsol.JK_OA_ReceivingForwarderAddress);
				}
			}

			foreach (var address in allGatewaysPKList)
			{
				if (!gatewaysOrderedList.Contains(address))
				{
					gatewaysOrderedList.Add(address);
				}
			}

			return gatewaysOrderedList;
		}

		List<ZGuid> GetAllUnorderedGatewayPKs(ForwardingConsol consol, ZGuid removedConsolGatewayAgentAddress, bool isDetachedConsol)
		{
			var unorderedGatewayPKs = Gateways.Select(x => x.JSG_OA_ForwarderAddress).ToList();

			if (!removedConsolGatewayAgentAddress.IsEmpty)
			{
				UpdateList(unorderedGatewayPKs, removedConsolGatewayAgentAddress, isRemove: true);
			}
			else
			{
				if (!consol.JK_SendingForwarderHandlingType.IsEmpty)
				{
					UpdateList(unorderedGatewayPKs, consol.SendingForwarderAddress?.PK ?? ZGuid.Empty, isDetachedConsol);
				}

				if (!consol.JK_ReceivingForwarderHandlingType.IsEmpty)
				{
					UpdateList(unorderedGatewayPKs, consol.ReceivingForwarderAddress?.PK ?? ZGuid.Empty, isDetachedConsol);
				}
			}

			return unorderedGatewayPKs;
		}

		void UpdateList(List<ZGuid> addressList, ZGuid address, bool isRemove)
		{
			if (!address.IsEmpty)
			{
				if (isRemove)
				{
					if (addressList.Contains(address))
					{
						addressList.Remove(address);
					}
				}
				else
				{
					if (!addressList.Contains(address))
					{
						addressList.Add(address);
					}
				}
			}
		}

		void SetGatewaysLength(int desiredLength)
		{
			while (Gateways.Count < desiredLength)
			{
				Gateways.AddNew();
			}

			while (Gateways.Count > desiredLength)
			{
				Gateways.Delete(Gateways.Last());
			}
		}

		void ValidateGatewayAddresses()
		{
			foreach (var gateway in Gateways)
			{
				gateway.Validation.ValidateJSG_OA_ForwarderAddress();
			}
		}

		#endregion

		#region Screening Statuses

		List<ScreeningPartiesSnapshot> screeningPartySnapshot;

		List<ScreeningPartiesSnapshot> currentSnapshot;

		List<ScreeningPartiesSnapshot> GetSnapshot(ForwardingShipment shipment) => ScreeningStatusUpdater.GetScreeningPartiesSnapshot(((IScreeningPartyProvider)shipment).ScreeningParties, shipment);

		#endregion

		#endregion

		#region GC Custom HAWBs

		[BusinessObjectTestExclude]
		public GB.CCSUK.ICusHAWBCollection GBCusHAWBs
		{
			get
			{
				// Used only by BusinessObjectsWithRelatedEvents to show these objects' workflow events on the shipment form. Lame.
				// Also used by ForwardingShipmentDocumentSupporter
				if (HasDischargeConsolInCountry(Core.Constants.CountryCodes.UnitedKingdom))
				{
					var gBCusHAWBs = ObjectFactory.New<GB.CCSUK.ICusHAWBCollection>(Factory, PK);
					gBCusHAWBs.Load();
					return gBCusHAWBs;
				}

				return null;
			}
		}

		#endregion

		#region Rate Local code and Commodity

		public RefCommodityCodeCollection JS_RH_NKRateCommodity_List => base.Factory.GetCachedValue("ShipmentCommodityList", () => new RefCommodityCodeCollection(base.Factory));

		/// <summary>
		/// Warning: The setting of JS_RH_NKRateCommodity will reset JS_FMCTariffID to ZString.Empty, ensure correct order of assignment to get desired result
		/// </summary>
		public override ZString JS_RH_NKRateCommodity
		{
			get => base.JS_RH_NKRateCommodity;
			set
			{
				if (base.JS_RH_NKRateCommodity != value)
				{
					rateLocalCode = null;
					base.JS_FMCTariffID = ZString.Empty;
				}
				base.JS_RH_NKRateCommodity = value;
			}
		}

		public ZString RateLocalCode
		{
			get
			{
				if (rateLocalCode == null)
				{
					var query = new ZQuery(RefCommodityCodeMapSchema.LC_RH_NKCommodityCode, JS_RH_NKRateCommodity)
						.AddToFilter(new ZQuery(RefCommodityCodeMapSchema.LC_LocalCodeProvider, GlobalCommodityCodeProviderList.Codes.Rating));

					var result = Factory.LoadTop1<RefCommodityCodeMap>(query);
					rateLocalCode = result?.LC_LocalCode ?? string.Empty;
				}

				return rateLocalCode;
			}
		}
		string rateLocalCode;

		public ZPropertyInfo RateLocalCodeInfo
		{
			get { return GetZPropertyInfo(nameof(RateLocalCode)); }
		}

		#endregion

		#region Properties

		public override ZGuid JS_RCA_BookingAllocationLine
		{
			get { return base.JS_RCA_BookingAllocationLine; }
			set
			{
				base.JS_RCA_BookingAllocationLine = value;
			}
		}

		public override ZString JS_CarrierContractNumber
		{
			get { return base.JS_CarrierContractNumber; }
			set
			{
				base.JS_CarrierContractNumber = value;
			}
		}

		[List("Lookups.ServiceLevelOrTransitTimeCollection")]
		public override ZString JS_RS_NKServiceLevel
		{
			get { return base.JS_RS_NKServiceLevel; }
			set
			{
				var oldValue = base.JS_RS_NKServiceLevel;
				base.JS_RS_NKServiceLevel = value;
				if (oldValue != value)
				{
					DeliveryDueDateFactorHasChanged(DeliveryDueDateChangedFactor.ServiceLevel);
					if (!IsValidationSuspended)
					{
						Validation.ValidateJS_RS_NKServiceLevel();
					}
				}
			}
		}

		public override ZDateTime JS_A_RCV
		{
			get { return base.JS_A_RCV; }
			set
			{
				if (JS_A_RCV != value)
				{
					base.JS_A_RCV = value;
					if (IsHBLContainerPackModeCFS_X)
					{
						DeliveryDueDateFactorHasChanged(DeliveryDueDateChangedFactor.InterimReceipt);
					}
				}
			}
		}

		public void OnServiceLevelOrTransitTimeSelected(object selectedServiceLevelOrTransitTime)
		{
			if (FreightDataRegistry.Instance.CalculateDeliveryDueDateByTransportMode.Value.IsTransportModeActive(JS_TransportMode) && selectedServiceLevelOrTransitTime is TransitTimeServiceLevelCombinationView transitTimeServiceLevelCombination)
			{
				JS_RS_NKServiceLevel = transitTimeServiceLevelCombination.TSC_Code;
			}
		}

		OrgAddress GetAddressFromZoneRelatedOrg(OrgHeader header, OrgAddressType type)
		{
			var addressList = header.Addresses.AddressesOfType(type);

			if (addressList.Count == 0)
			{
				addressList = header.Addresses.AddressesOfType(OrgAddressType.PickupAndDelivery);
			}

			return addressList.Count == 0 ? header.Addresses.MainAddress : addressList[0];
		}

		public override ZString JS_HBLContainerPackModeOverride
		{
			get { return base.JS_HBLContainerPackModeOverride; }
			set
			{
				if (value != JS_HBLContainerPackModeOverride)
				{
					base.JS_HBLContainerPackModeOverride = value;
					DeliveryDueDateFactorHasChanged(DeliveryDueDateChangedFactor.HBLDeliveryMode);
					if (FreightDataRegistry.Instance.EnablePrePostCarriageCalculation.Value &&
						FreightDataRegistry.Instance.UseHBLDeliveryModeForGHGCalculation.Value)
					{
						this.UpdateCO2eStatusToNotCurrent(new CO2eStatusChangedReason(JS_HBLContainerPackModeOverrideInfo), IsCopying);
					}
				}
			}
		}

		internal bool IsHBLContainerPackModeCFS_X => JS_HBLContainerPackModeOverride == Constants.HBLDeliveryModes.Codes.CFS_DOOR
			|| JS_HBLContainerPackModeOverride == Constants.HBLDeliveryModes.Codes.CFS_CFS
			|| JS_HBLContainerPackModeOverride == Constants.HBLDeliveryModes.Codes.CFS_CY
			|| JS_HBLContainerPackModeOverride == Constants.HBLDeliveryModes.Codes.CFS_ARPT;

		internal bool IsHBLContainerPackModeDOOR_X => JS_HBLContainerPackModeOverride == Constants.HBLDeliveryModes.Codes.DOOR_DOOR
			|| JS_HBLContainerPackModeOverride == Constants.HBLDeliveryModes.Codes.DOOR_CFS
			|| JS_HBLContainerPackModeOverride == Constants.HBLDeliveryModes.Codes.DOOR_CY
			|| JS_HBLContainerPackModeOverride == Constants.HBLDeliveryModes.Codes.DOOR_ARPT
			|| JS_HBLContainerPackModeOverride == Constants.HBLDeliveryModes.Codes.DOOR_PORT;

		internal bool IsHBLContainerPackModeX_DOOR => JS_HBLContainerPackModeOverride == Constants.HBLDeliveryModes.Codes.CFS_DOOR
			|| JS_HBLContainerPackModeOverride == Constants.HBLDeliveryModes.Codes.DOOR_DOOR
			|| JS_HBLContainerPackModeOverride == Constants.HBLDeliveryModes.Codes.CY_DOOR
			|| JS_HBLContainerPackModeOverride == Constants.HBLDeliveryModes.Codes.ARPT_DOOR
			|| JS_HBLContainerPackModeOverride == Constants.HBLDeliveryModes.Codes.PORT_DOOR;

		internal bool IsHBLContainerPackModeARPT_X => DeliveryDueDateCalculationHelper.IsHBLContainerPackModeARPT_X(JS_HBLContainerPackModeOverride);

		#region Chargeables

		public override ZDecimal JS_ManifestedChargeable
		{
			get => base.JS_ManifestedChargeable;
			set => SetChargeableValue(JobShipmentSchema.JS_ManifestedChargeable, setValue => base.JS_ManifestedChargeable = setValue, currentValue: JS_ManifestedChargeable, propertyInfo: JS_ManifestedChargeableInfo, newValue: value);
		}

		public override ZDecimal JS_DocumentedChargeable
		{
			get => base.JS_DocumentedChargeable;
			set => SetChargeableValue(JobShipmentSchema.JS_DocumentedChargeable, setValue => base.JS_DocumentedChargeable = setValue, currentValue: JS_DocumentedChargeable, propertyInfo: JS_DocumentedChargeableInfo, newValue: value);
		}

		public override ZDecimal JS_ActualChargeable
		{
			get => base.JS_ActualChargeable;
			set => SetChargeableValue(JobShipmentSchema.JS_ActualChargeable, setValue => base.JS_ActualChargeable = setValue, currentValue: JS_ActualChargeable, propertyInfo: JS_ActualChargeableInfo, newValue: value);
		}

		void SetChargeableValue(SchemaColumn column, Action<decimal> setter, decimal currentValue, ZPropertyInfo propertyInfo, decimal newValue)
		{
			if (IsAir)
			{
				newValue = ChargeableWeightRoundingHelper.GetRoundedValueAir(column, newValue);
			}
			else if (!IsSettingDefaultValues && IsSea)
			{
				var minChargeWeight = FreightDataRegistry.Instance.SeaMinimumChargeableWeight.Value;
				if (minChargeWeight > 0 && newValue < minChargeWeight)
				{
					newValue = minChargeWeight;
				}
			}

			if (currentValue != newValue)
			{
				setter.Invoke(newValue);
				propertyInfo.RefreshBinding();
				UpdateConsolTotalChargeableAmounts();
			}
		}

		protected override void OnUpdatedByDataRefresh()
		{
			base.OnUpdatedByDataRefresh();
			UpdateConsolTotalChargeableAmounts();
			UpdateDensity();
			UpdateAchievedQuantities();
		}

		void UpdateAchievedQuantities()
		{
			if (!fIsImportingData)
			{
				foreach (ForwardingConsol consol in Consols)
				{
					consol.CalculationWrapper.UpdateCalculation();
				}
			}
		}

		void UpdateDensity()
		{
			Density.RefreshAllValues();
			JS_ChargeableUnitInfo.RefreshBinding();
		}

		void UpdateConsolTotalChargeableAmounts()
		{
			if (!fIsImportingData)
			{
				foreach (ForwardingConsol consol in Consols)
				{
					consol.JK_TotalPrepaidShipmentChargeableAmountInfo.RefreshBinding();
					consol.JK_TotalCollectShipmentChargeableAmountInfo.RefreshBinding();
				}
			}
		}

		#endregion

		#region Chargeable Weights

		protected override void UpdateDocumentedChargeableWeight()
		{
			if (IsAir && WeightVolumeUnitsAreValid)
			{
				var newValue = ChargeableWeight(JS_DocumentedVolume, JS_DocumentedWeight);
				JS_DocumentedChargeable = ChargeableWeightRoundingHelper.GetRoundedValueAir(JobShipmentSchema.JS_DocumentedChargeable, newValue);
			}
			else
			{
				base.UpdateDocumentedChargeableWeight();
			}
		}

		protected override void UpdateManifestedChargeableWeight()
		{
			if (IsAir && WeightVolumeUnitsAreValid)
			{
				var newValue = ChargeableWeight(JS_ManifestedVolume, JS_ManifestedWeight);
				JS_ManifestedChargeable = ChargeableWeightRoundingHelper.GetRoundedValueAir(JobShipmentSchema.JS_ManifestedChargeable, newValue);
			}
			else
			{
				base.UpdateManifestedChargeableWeight();
			}
		}

		#endregion

		public ZBool HasContainersNewMBOL
		{
			get
			{
				if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.UnitedStates
					&& CountriesOfRouting.Contains(Core.Constants.CountryCodes.UnitedStates))
				{
					foreach (ForwardingContainer container in Containers)
					{
						if (container.HasNewMBOL)
						{
							var consol = container.Consol;
							if (consol != null && consol.JK_TransportMode == Core.Constants.TransportModes.Sea)
							{
								return true;
							}
						}
					}
				}
				return false;
			}
		}

		public ZString CustomsEntryNumberForBinding
		{
			get
			{
				var count = CusEntryNumbers.Count;
				if (count == 1)
				{
					return CusEntryNumbers[0].CE_EntryNum;
				}
				if (count > 1)
				{
					return GetValidEntryNumberOrMany();
				}

				return ZString.Empty;
			}
			set
			{
				CustomsEntryNumber = value;
			}
		}

		ZString GetValidEntryNumberOrMany()
		{
			var manyString = Res.GetString("b122ae23-5cd5-46f1-9936-b4fa28181237", "Many");
			var firstValidEntryNum = ZString.Empty;

			foreach (CusEntryNumber entry in CusEntryNumbers)
			{
				if (entry.CE_EntryNum != ZString.Empty)
				{
					if (firstValidEntryNum != ZString.Empty)
					{
						return manyString;
					}

					firstValidEntryNum = entry.CE_EntryNum;
				}
			}

			return firstValidEntryNum == ZString.Empty ? ZString.Empty : firstValidEntryNum;
		}

		public ZPropertyInfo CustomsEntryNumberForBindingInfo => CustomsEntryNumberInfo;

		public bool CustomsEntryNumberForBinding_ReadOnly
		{
			get
			{
				if (CusEntryNumbers.Count > 1)
				{
					return true;
				}
				if (CusEntryNumbers.Count == 1 && CusEntryNumbers[0].CE_EntryIsSystemGenerated)
				{
					return true;
				}

				return false;
			}
		}

		#region Property Overrides

		public override ZString CustomsEntryNumber
		{
			get
			{
				return base.CustomsEntryNumber;
			}
			set
			{
				if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Iceland)
				{
					base.CustomsEntryNumberType = CusEntryNumberTypes.Iceland.CRN;
				}

				base.CustomsEntryNumber = value;

				if (!IsValidationSuspended)
				{
					OuterPackLines.OfType<ForwardingPackLine>()
						.ForEach(packline => packline.Validation.ValidateJL_ExportRefNumber());
				}
			}
		}

		public override ZString CustomsEntryNumberType
		{
			get
			{
#if NETFRAMEWORK
				if (CusEntryNumbers.Cast<CusEntryNumber>().DistinctBy(x => x.CE_EntryType).Count() > 1)
#else
				if (Enumerable.DistinctBy(CusEntryNumbers.Cast<CusEntryNumber>(), x => x.CE_EntryType).Count() > 1)
#endif
				{
					return ZString.Empty;
				}
				return base.CustomsEntryNumberType;
			}
			set
			{
				base.CustomsEntryNumberType = value;
				if (!IsValidationSuspended)
				{
					OuterPackLines.OfType<ForwardingPackLine>()
						.ForEach(packline => packline.Validation.ValidateJL_ExportRefNumber());
				}
			}
		}

		public IUpdateFromShipment AirCargoSynchroniser;

		public override ZString JS_HouseBill
		{
			get { return base.JS_HouseBill; }
			set
			{
				base.JS_HouseBill = value;
				if (AirCargoSynchroniser != null)
				{
					AirCargoSynchroniser.HouseBillNumber = value;
				}

				TriggerPackLineSynchroniser();
			}
		}

		#region JS_HouseBill_ReadOnly

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Non-translatable literal constant")]
		protected bool JS_HouseBill_ReadOnly
		{
			get
			{
				return (JS_HouseBill.ToUpper() == "PENDING ALLOCATION.." && IsPendingAllocationSetBySystem) || IsHouseBillReadOnlyWhenEHBLEnabled();
			}
		}

		bool IsHouseBillReadOnlyWhenEHBLEnabled()
		{
			return FreightDataRegistry.Instance.EnableBoleroEHBLIntegration.Value.EnableEBLIntegration && IsSea && new ZString[]
			{
				FreightConstants.BillOfLadingBillStatus.Codes.SentForPublication,
				FreightConstants.BillOfLadingBillStatus.Codes.OriginalBillPublished,
				FreightConstants.BillOfLadingBillStatus.Codes.OriginalBillTransferred,
			}.Contains(JS_ElectronicBillOfLadingStatus);
		}

		#endregion

		public override ZDateTime JS_HouseBillIssueDate
		{
			get => base.JS_HouseBillIssueDate;
			set
			{
				base.JS_HouseBillIssueDate = value;
				TriggerPackLineSynchroniser();
			}
		}

		public override ZString JS_INCO
		{
			get { return base.JS_INCO; }
			set
			{
				if (base.JS_INCO != value)
				{
					base.JS_INCO = value;

					if (AirCargoSynchroniser != null)
					{
						AirCargoSynchroniser.PaymentTerm = JS_PaymentTerm;
					}

					if (IsDomesticFreight && Job != null)
					{
						Job.SetDefaultsForJob();
					}

					if (string.IsNullOrWhiteSpace(JS_PaymentTermAutoratingOverride))
					{
						JS_PaymentTermAutoratingOverride = JS_PaymentTerm;
					}
				}
			}
		}

		public override ZGuid JS_JS_ColoadMasterShipment
		{
			get
			{
				ZGuid result = base.JS_JS_ColoadMasterShipment;

				if (fJS_JS_ColoadMasterShipment != result)
				{
					fJS_JS_ColoadMasterShipment = result;
					if (CoLoadMasterShipment != null)
					{
						CoLoadMasterShipment.JS_HouseBillInfo.ValueChanged += new EventHandler(ColoadMasterShipmentHouseBillInfo_ValueChanged);

						if (AviationSecurity.SupplyChainSecurityConfiguration.IsEnabled)
						{
							CoLoadMasterShipment.ConsignorDocumentaryAddress.DocAddressChanged += new EventHandler(ColoadMasterShipmentConsignor_AviationSecurityPartyUpdated);
							AviationSecurityParty_HasChanges(SupplyChainSecurityOrganisationTypes.CoLoadMasterShipmentSendingForwarder);
						}
					}
				}

				return result;
			}
			set
			{
				if (base.JS_JS_ColoadMasterShipment != value)
				{
					if (CoLoadMasterShipment != null)
					{
						CoLoadMasterShipment.JS_HouseBillInfo.ValueChanged -= new EventHandler(ColoadMasterShipmentHouseBillInfo_ValueChanged);

						if (AviationSecurity.SupplyChainSecurityConfiguration.IsEnabled)
						{
							CoLoadMasterShipment.ConsignorDocumentaryAddress.DocAddressChanged -= new EventHandler(ColoadMasterShipmentConsignor_AviationSecurityPartyUpdated);
						}

						((ForwardingShipment)CoLoadMasterShipment).TriggerPackLineSynchroniser();
					}

					base.JS_JS_ColoadMasterShipment = value;
					if (AirCargoSynchroniser != null)
					{
						AirCargoSynchroniser.CoLoadMasterShipmentPK = value;
						AirCargoSynchroniser.CoLoadMasterShipmentHouseBillNumber = CoLoadMasterShipment != null ? CoLoadMasterShipment.JS_HouseBill : ZString.Empty;
					}

					if (CoLoadMasterShipment != null)
					{
						((ForwardingShipment)CoLoadMasterShipment).TriggerPackLineSynchroniser();
					}

					if (!IsValidationSuspended)
					{
						Validation.ValidateJS_ShipmentType();
					}

					SetHandledOnBehalfOfForwarder(ShipmentPropertyChanged.CoLoadMasterShipment);
				}
			}
		}

		public void SetHandledOnBehalfOfForwarder(ShipmentPropertyChanged propertyChanged)
		{
			if (JS_IsCFSRegistered)
			{
				CommonConsol arrivalConsol;
				CommonConsol departureConsol;

				if (this.IsImport()
					&& (arrivalConsol = GetArrivalConsol()) != null
					&& !arrivalConsol.IsDeleted)
				{
					SetImportHandledOnBehalfOfForwarder(propertyChanged);
				}
				else if (this.IsExport()
					&& (departureConsol = GetDepartureConsol()) != null
					&& !departureConsol.IsDeleted)
				{
					SetExportHandledOnBehalfOfForwarder(propertyChanged, departureConsol);
				}
				else
				{
					JS_OH_HandledOnBehalfOfForwarder = ZGuid.Empty;
				}
			}
		}

		void SetExportHandledOnBehalfOfForwarder(ShipmentPropertyChanged propertyChanged, CommonConsol departureConsol)
		{
			if ((propertyChanged == ShipmentPropertyChanged.ConsolLoadPort
				 || propertyChanged == ShipmentPropertyChanged.ShipmentOrigin
				 || propertyChanged == ShipmentPropertyChanged.CoLoadMasterShipment
				 || propertyChanged == ShipmentPropertyChanged.IsCFSRegistered)
				 && !JS_JS_ColoadMasterShipment.IsEmpty && CoLoadMasterShipment != null)
			{
				JS_OH_HandledOnBehalfOfForwarder = CoLoadMasterShipment.ConsignorPK;
			}
			else
			{
				switch (propertyChanged)
				{
					case ShipmentPropertyChanged.Consignor:
						foreach (ForwardingShipment shipment in CoLoadShipments)
						{
							shipment.JS_OH_HandledOnBehalfOfForwarder = ConsignorPK;
						}
						break;
					case ShipmentPropertyChanged.IsCFSRegistered:
					case ShipmentPropertyChanged.ShipmentOrigin:
					case ShipmentPropertyChanged.CoLoadMasterShipment:
						JS_OH_HandledOnBehalfOfForwarder = departureConsol.SendingForwarderPK;
						break;
				}
			}
		}

		void SetImportHandledOnBehalfOfForwarder(ShipmentPropertyChanged propertyChanged)
		{
			if ((propertyChanged == ShipmentPropertyChanged.ConsolDischargePort
						 || propertyChanged == ShipmentPropertyChanged.ShipmentDestination
						 || propertyChanged == ShipmentPropertyChanged.CoLoadMasterShipment
						 || propertyChanged == ShipmentPropertyChanged.IsCFSRegistered)
						&& !JS_JS_ColoadMasterShipment.IsEmpty && CoLoadMasterShipment != null)
			{
				JS_OH_HandledOnBehalfOfForwarder = CoLoadMasterShipment.ConsigneePK;
			}
			else
			{
				switch (propertyChanged)
				{
					case ShipmentPropertyChanged.Consignee:
						foreach (ForwardingShipment shipment in CoLoadShipments)
						{
							shipment.JS_OH_HandledOnBehalfOfForwarder = ConsigneePK;
						}
						break;
					case ShipmentPropertyChanged.IsCFSRegistered:
					case ShipmentPropertyChanged.ShipmentDestination:
					case ShipmentPropertyChanged.CoLoadMasterShipment:
						JS_OH_HandledOnBehalfOfForwarder = ArrivalConsol.ReceivingForwarderPK;
						break;
				}
			}
		}

		public enum ShipmentPropertyChanged
		{
			Consignee,
			Consignor,
			CoLoadMasterShipment,
			IsCFSRegistered,
			ShipmentDestination,
			ShipmentOrigin,
			ConsolLoadPort,
			ConsolDischargePort
		}

		ZGuid fJS_JS_ColoadMasterShipment;

		void TriggerPackLineSynchroniser()
		{
			if (PackLineSynchroniser != null)
			{
				PackLineSynchroniser.MarkSyncDirty();
			}
		}

		protected override void ShipmentTypeChangedToOrFromCoload()
		{
			if (IsCoLoadMaster || IsBlindCoLoadMaster)
			{
				if (Consignor != null && !Consignor.OH_IsForwarder)
				{
					ConsignorPK = ZGuid.Empty;
				}
				if (Consignee != null && !Consignee.OH_IsForwarder)
				{
					ConsigneePK = ZGuid.Empty;
				}
			}
			else
			{
				if (Consignor != null && !Consignor.OH_IsConsignor)
				{
					ConsignorPK = ZGuid.Empty;
				}
				if (Consignee != null && !Consignee.OH_IsConsignee)
				{
					ConsigneePK = ZGuid.Empty;
				}
			}

			if (JS_IsCFSRegistered && this.IsImport())
			{
				JS_TranshipToOtherCFS = !IsCoLoadMaster && !IsBlindCoLoadMaster;
			}

			if (AirCargoSynchroniser != null)
			{
				AirCargoSynchroniser.IsCoload = (IsCoLoadMaster || IsBlindCoLoadMaster);
			}
		}

		public override ZString JS_GoodsDescription
		{
			get { return base.JS_GoodsDescription; }
			set
			{
				if (value.Trim().Length > 35)
				{
					ErrorReporter.ReportOnce("The Goods Description length exceeded maximum 35 characters allowed.");
				}

				base.JS_GoodsDescription = value;

				if (AirCargoSynchroniser != null)
				{
					AirCargoSynchroniser.GoodsDescription = FullGoodsDescription;
				}
			}
		}

		public ZString FullGoodsDescription
		{
			get { return DetailedGoodsDescriptionNoteText.IndexOf(JS_GoodsDescription, StringComparison.OrdinalIgnoreCase) >= 0 ? DetailedGoodsDescriptionNoteText : (ZString)(JS_GoodsDescription + DetailedGoodsDescriptionNoteText); }
		}

		public bool JS_GoodsDescription_ReadOnly
		{
			get;
			set;
		}

		public override ZDecimal JS_GoodsValue
		{
			get => base.JS_GoodsValue;
			set
			{
				if (ShouldDefaultInsuranceDetailsFromGoodsDetails)
				{
					JS_InsuranceValue = value;
				}

				base.JS_GoodsValue = value;

				if (AirCargoSynchroniser != null)
				{
					AirCargoSynchroniser.GoodsValue = value;
				}
			}
		}

		public override ZDecimal JS_InsuranceValue
		{
			get => base.JS_InsuranceValue;
			set
			{
				base.JS_InsuranceValue = value;

				if (JS_RX_NKInsuranceCurrency.IsEmpty)
				{
					JS_RX_NKInsuranceCurrency = JS_RX_NKGoodsValueCurr;
				}
			}
		}

		public override ZInt JS_OuterPacks
		{
			get { return base.JS_OuterPacks; }
			set
			{
				base.JS_OuterPacks = value;
				if (AirCargoSynchroniser != null)
				{
					AirCargoSynchroniser.OuterPacks = value;
				}
			}
		}

		public override ZString JS_UnitOfWeight
		{
			get { return base.JS_UnitOfWeight; }
			set
			{
				if (base.JS_UnitOfWeight != value)
				{
					JS_UnitOfWeightModifiedDateUtc = ZDateTime.UtcNow;
					base.JS_UnitOfWeight = value;
					this.UpdateCO2eStatusToNotCurrent(new CO2eStatusChangedReason(JS_UnitOfWeightInfo), IsCopying);
				}

				if (AirCargoSynchroniser != null)
				{
					AirCargoSynchroniser.UnitOfWeight = value;
				}
			}
		}

		internal ZDateTime JS_UnitOfWeightModifiedDateUtc { get; private set; }

		public override ZDecimal JS_ActualWeight
		{
			get { return base.JS_ActualWeight; }
			set
			{
				if (base.JS_ActualWeight != value)
				{
					JS_ActualWeightModifiedDateUtc = ZDateTime.UtcNow;
					base.JS_ActualWeight = value;
					this.UpdateCO2eStatusToNotCurrent(new CO2eStatusChangedReason(JS_ActualWeightInfo), IsCopying);
				}

				if (AirCargoSynchroniser != null)
				{
					AirCargoSynchroniser.ActualWeight = value;
				}
			}
		}

		internal ZDateTime JS_ActualWeightModifiedDateUtc { get; private set; }

		public override ZDecimal JS_ActualVolume
		{
			get { return base.JS_ActualVolume; }
			set
			{
				if (base.JS_ActualVolume != value)
				{
					JS_ActualVolumeModifiedDateUtc = ZDateTime.UtcNow;
					base.JS_ActualVolume = value;
				}
			}
		}

		internal ZDateTime JS_ActualVolumeModifiedDateUtc { get; private set; }

		public override ZString JS_UnitOfVolume
		{
			get { return base.JS_UnitOfVolume; }
			set
			{
				if (base.JS_UnitOfVolume != value)
				{
					JS_UnitOfVolumeModifiedDateUtc = ZDateTime.UtcNow;
					base.JS_UnitOfVolume = value;
				}
			}
		}

		internal ZDateTime JS_UnitOfVolumeModifiedDateUtc { get; private set; }

		void ColoadMasterShipmentHouseBillInfo_ValueChanged(object sender, EventArgs e)
		{
			if (AirCargoSynchroniser != null)
			{
				AirCargoSynchroniser.CoLoadMasterShipmentHouseBillNumber = CoLoadMasterShipment == null ? ZString.Empty : CoLoadMasterShipment.JS_HouseBill;
			}
		}

		void ColoadMasterShipmentConsignor_AviationSecurityPartyUpdated(object sender, EventArgs e)
		{
			AviationSecurityParty_HasChanges(SupplyChainSecurityOrganisationTypes.CoLoadMasterShipmentSendingForwarder);
		}

		[DocumentEngineIntegration.DocumentParsing.DocumentFieldExcludeFromMap]
		[WorkflowSetFieldReadonly]
		public override ZBool JS_IsCFSRegistered
		{
			get { return base.JS_IsCFSRegistered; }
			set
			{
				base.JS_IsCFSRegistered = value;

				if (value && JS_OH_HandledOnBehalfOfForwarder.IsEmpty)
				{
					SetHandledOnBehalfOfForwarder(ShipmentPropertyChanged.IsCFSRegistered);
				}
			}
		}

		public override ZString JS_TransportMode
		{
			get { return base.JS_TransportMode; }
			set
			{
				if (base.JS_TransportMode != value || IsSettingDefaultValues)
				{
					using (((ISingleElementListInternal)this).SuspendListChanged())
					{
						var oldValue = base.JS_TransportMode;
						base.JS_TransportMode = value;

						if (oldValue != value)
						{
							DeliveryDueDateFactorHasChanged(DeliveryDueDateChangedFactor.TransportMode);
						}

						RaiseTransportModeChanged();

						if (value == Constants.TransportModes.Sea)
						{
							var currentValue = JS_ActualChargeable;
							SetChargeableValue(JobShipmentSchema.JS_ActualChargeable, setValue => base.JS_ActualChargeable = setValue, currentValue, JS_ActualChargeableInfo, newValue: currentValue);
						}

						DefaultJS_ShipmentStatus();

						if (base.JS_TransportMode != oldValue || IsSettingDefaultValuesForHBLAWBChargesDisplay)
						{
							JS_HBLAWBChargesDisplay = GetDefaultChargesDisplay();
						}

						if (AirCargoSynchroniser != null)
						{
							AirCargoSynchroniser.IsAir = (value == Enterprise.Core.Constants.TransportModes.Air);
						}

						if (!IsSettingDefaultValues)
						{
							SetConsignorChanges();
							SetConsigneeChanges();
							SetConsignorConsigneeCommonChanges();

							SetApprovedShipperStatus(Res.GetString("0200cfc4-dcb1-49b8-b9f0-98677750b9db", "{0} has been changed", JS_TransportModeInfo.HumanReadableName));
							ReDefaultPackLineInspectionTypeCodes();
							TryMatchingDangerousGoodsToNewStandard();
						}

						if (BuyerSupplierLinksHelper != null)
						{
							BuyerSupplierLinksHelper.RestoreEFreightStatus();
						}

						if (!IsValidationSuspended)
						{
							Validation.ValidateJS_HouseBill();
							Validation.ValidateJS_RL_NKOrigin();
							ValidateOuterPackLineUNDGStandard();
						}
					}
					RefreshBinding();
					this.UpdateCO2eStatusToNotCurrent(new CO2eStatusChangedReason(JS_TransportModeInfo), IsCopying);
				}
			}
		}

		public event EventHandler TransportModeValueChanged;

		void RaiseTransportModeChanged()
		{
			if (TransportModeValueChanged != null)
			{
				TransportModeValueChanged(this, EventArgs.Empty);
			}
		}

		void TryMatchingDangerousGoodsToNewStandard()
		{
			var correspondingStandard = DGStandardCalculator.GetCorrespondingStandardForShipmentMode(this);

			var allDGsToTryChange = OuterPackLines
				.OfType<ForwardingPackLine>()
				.SelectMany(packline => packline.UNDGs)
				.Where(undg => (undg.Substance?.DG_Standard ?? ZString.Empty) != correspondingStandard)
				.ToArray();

			if (allDGsToTryChange.Length > 0)
			{
				ForwardingUNDGStandardRelinker.TryRelinkAllDataItemsToTargetStandard(Factory, allDGsToTryChange, correspondingStandard);
			}
		}

		void ValidateOuterPackLineUNDGStandard()
		{
			OuterPackLines.ForEach(packLine => (packLine as ForwardingPackLine)?.UNDGs?.ForEach(item => item.Validation.ValidateDI_DG()));
		}

		[List("Lookups.JS_PackingMode_List")]
		public override ZString JS_PackingMode
		{
			get { return base.JS_PackingMode; }
			set
			{
				if (base.JS_PackingMode != value)
				{
					using (RequireTEUMonitor.MonitorChange(new CO2eStatusChangedReason(JS_PackingModeInfo), UpdateTransportLegsCO2eStatusToNCU))
					{
						base.JS_PackingMode = value;

						if (!IsSettingDefaultValues)
						{
							DocsAndCartage.Services.UpdateDefaultContractor(Constants.FreightServiceType.Codes.Fumigation);

							SetConsignorChanges();
							SetConsigneeChanges();
							SetConsignorConsigneeCommonChanges();
							RedefaultFCLDetentionCharges();
						}
					}
				}
			}
		}

		void UpdateTransportLegsCO2eStatusToNCU()
		{
			TransportsInLegOrder.Cast<Transport>().ForEach(leg => leg.UpdateCO2eStatusToNotCurrent());
		}

		void RedefaultFCLDetentionCharges()
		{
			if (IsAir || Constants.ContainerModes.IsLCLType(JS_PackingMode))
			{
				DocsAndCartage.JP_FCLDeliveryDetentionFreeDays = 0;
				DocsAndCartage.JP_FCLDeliveryDetentionDays = 0;
				DocsAndCartage.JP_FCLDeliveryDetentionCharge = 0;
				DocsAndCartage.JP_FCLPickupDetentionFreeDays = 0;
				DocsAndCartage.JP_FCLPickupDetentionDays = 0;
				DocsAndCartage.JP_FCLPickupDetentionCharge = 0;
			}
		}

		void SetConsignorValues()
		{
			if (AirCargoSynchroniser != null)
			{
				AirCargoSynchroniser.CopyConsignorDetails(this);
			}
		}

		void SetConsigneeValues()
		{
			if (AirCargoSynchroniser != null)
			{
				AirCargoSynchroniser.CopyConsigneeDetails(this);
			}
		}

		public override ZString JS_RL_NKDestination
		{
			get { return base.JS_RL_NKDestination; }
			set
			{
				if (base.JS_RL_NKDestination != value)
				{
					bool prevIsImport = this.IsImport();
					bool prevIsExport = this.IsExport();
					bool prevIsCrossTrade = this.IsCrossTrade();

					bool checkTranshipmentHasChanged = AviationSecurity.SupplyChainSecurityConfiguration.UseTranshipmentAviationSecurityStatus;
					bool prevIsTranshipment = checkTranshipmentHasChanged && AviationSecurity.SupplyChainSecurityConfiguration.IsTranshipment(this);

					base.JS_RL_NKDestination = value;

					bool newIsTranshipment = checkTranshipmentHasChanged && AviationSecurity.SupplyChainSecurityConfiguration.IsTranshipment(this);

					AttachedOrders.MarkAsNeedingValidation();

					if (AirCargoSynchroniser != null)
					{
						AirCargoSynchroniser.Destination = value;
					}

					JS_Calc_ACIConsigneeDestinationZoneInfo.RefreshBinding();

					if (this.IsImport() != prevIsImport || this.IsExport() != prevIsExport)
					{
						OnImportExportChanged();
					}

					if (!IsSettingDefaultOrImportingData)
					{
						SetDeliveryCFS();
						SetDeliveryAgent();
						SetDeliveryAddress();

						if (newIsTranshipment != prevIsTranshipment && AviationSecurity.SupplyChainSecurityConfiguration.IsRecalculationNeeded(this.JS_InspectionTypeCode))
						{
							SetApprovedShipperStatus(Res.GetString("7f41bb75-3c0d-4c71-8083-bb91d29021f8", "{0} has been changed", JS_RL_NKDestinationInfo.HumanReadableName));
						}

						ReDefaultPackLineInspectionTypeCodes();
					}

					SetHandledOnBehalfOfForwarder(ShipmentPropertyChanged.ShipmentDestination);

					if (!IsValidationSuspended)
					{
						Validation.ValidateJS_HouseBill();
						Validation.ValidateJS_RL_NKOrigin();
					}

					if (this.IsCrossTrade() != prevIsCrossTrade)
					{
						OnCrossTradeChanged();
					}

					this.UpdateCO2eStatusToNotCurrent(new CO2eStatusChangedReason(JS_RL_NKDestinationInfo), IsCopying);

					if (IsHighVolumeLowValue)
					{
						PopulateHCHUsageType();
					}

					UpdateSavedBuyerSupplierLinksImportCountryCode();
				}
			}
		}

		void UpdateSavedBuyerSupplierLinksImportCountryCode()
		{
			if (CurrentRootConsol != null && CurrentRootConsol.SavedBuyerSupplierLinksWithEmptyImportCountryCode.Any() && Destination != null && Destination.Country != null)
			{
				CurrentRootConsol.SavedBuyerSupplierLinksWithEmptyImportCountryCode.ForEach(link => link.OL_RN_NKImporterCountry = Destination.Country.RN_Code);
			}
		}

		public override ZString JS_RL_NKDischargePort
		{
			get { return base.JS_RL_NKDischargePort; }
			set
			{
				if (base.JS_RL_NKDischargePort != value)
				{
					base.JS_RL_NKDischargePort = value;

					if (JS_RL_NKDestination.IsEmpty)
					{
						this.UpdateCO2eStatusToNotCurrent(new CO2eStatusChangedReason(JS_RL_NKDischargePortInfo), IsCopying);
					}
				}
			}
		}

		public override ZString JS_RL_NKOrigin
		{
			get { return base.JS_RL_NKOrigin; }
			set
			{
				if (base.JS_RL_NKOrigin != value)
				{
					bool prevIsImport = this.IsImport();
					bool prevIsExport = this.IsExport();
					bool prevIsCrossTrade = this.IsCrossTrade();
					base.JS_RL_NKOrigin = value;

					AttachedOrders.MarkAsNeedingValidation();

					if (AirCargoSynchroniser != null)
					{
						AirCargoSynchroniser.Origin = value;
					}

					JS_Calc_ACIConsignorOriginZoneInfo.RefreshBinding();

					if (this.IsImport() != prevIsImport || this.IsExport() != prevIsExport)
					{
						OnImportExportChanged();
					}

					if (!IsSettingDefaultOrImportingData)
					{
						SetPickupCFS();
						SetPickupAgent();
						SetPickupAddress();
						SetApprovedShipperStatus(Res.GetString("44bc10ab-4ae8-4ce6-8ab9-988e444db8f9", "{0} has been changed", JS_RL_NKOriginInfo.HumanReadableName));
						ReDefaultPackLineInspectionTypeCodes();
					}

					SetHandledOnBehalfOfForwarder(ShipmentPropertyChanged.ShipmentOrigin);

					if (!IsValidationSuspended)
					{
						Validation.ValidateJS_HouseBill();
					}
					if (this.IsCrossTrade() != prevIsCrossTrade)
					{
						OnCrossTradeChanged();
					}

					this.UpdateCO2eStatusToNotCurrent(new CO2eStatusChangedReason(JS_RL_NKOriginInfo), IsCopying);
				}
			}
		}

		public override ZString JS_RL_NKLoadPort
		{
			get { return base.JS_RL_NKLoadPort; }
			set
			{
				if (base.JS_RL_NKLoadPort != value)
				{
					base.JS_RL_NKLoadPort = value;

					if (JS_RL_NKOrigin.IsEmpty)
					{
						this.UpdateCO2eStatusToNotCurrent(new CO2eStatusChangedReason(JS_RL_NKLoadPortInfo), IsCopying);
					}
				}
			}
		}

		void OnImportExportChanged()
		{
			if (ImportExportChanged != null)
			{
				ImportExportChanged(this, EventArgs.Empty);
			}
		}

		void OnCrossTradeChanged()
		{
			if (CrossTradeChanged != null)
			{
				CrossTradeChanged(this, EventArgs.Empty);
			}
		}

		public event EventHandler ImportExportChanged;

		public override ZString JS_UniqueConsignRef
		{
			get { return base.JS_UniqueConsignRef; }
			set
			{
				base.JS_UniqueConsignRef = value;
				if (AirCargoSynchroniser != null)
				{
					AirCargoSynchroniser.UniqueConsignRef = value;
				}
			}
		}

		public override ZString JS_ShipmentType
		{
			get { return base.JS_ShipmentType; }
			set
			{
				base.JS_ShipmentType = value;

				CreateHVLVConsignmentHeaderIfNecessary();
			}
		}

		public bool JS_ShipmentType_ReadOnly
		{
			get { return this.HasHVLVDataCreated || this.IsHighVolumeLowValueMaster; }
		}

		protected override ShipmentCustomsEntryNumber GetNewShipmentCustomsEntryNumber()
		{
			return new ForwardingShipmentCustomsEntryNumber(this) { IsEntryNumberForShipmentBinding = true };
		}

		#region JS_UniqueConsignRef

		protected virtual bool JS_UniqueConsignRef_ReadOnly
		{
			get { return !Env.Registry.AllowManualShipmentEntry || IsInDatabase; }
		}

		#endregion

		public bool JS_IsNeutralMaster_ReadOnly
		{
			get;
			set;
		}

		#region JS_OA_ExportReceivingDepot, JS_OA_ImportReleaseDepot

		public override ZGuid JS_OA_ExportReceivingDepot
		{
			get { return base.JS_OA_ExportReceivingDepot; }
			set
			{
				if (JS_OA_ExportReceivingDepot != value)
				{
					base.JS_OA_ExportReceivingDepot = value;

					this.UpdateCO2eStatusToNotCurrent(new CO2eStatusChangedReason(JS_OA_ExportReceivingDepotInfo), IsCopying);
					UpdateContainersCO2eStatusOnJS_OA_ExportReceivingDepotChanged();
					UpdateIsCFSRegisteredFromDepot(ExportReceivingDepot);
					ScreeningStatusUpdater.SetShouldUpdateScreeningStatusByOrgAddress(this, JS_ScreeningStatus, Factory, JS_OA_ExportReceivingDepot);
					AviationSecurityParty_HasChanges(SupplyChainSecurityOrganisationTypes.ShipmentPackingCFS);
					JS_RL_NKFreightRateOriginSetDefault();
					DeliveryDueDateFactorHasChanged(DeliveryDueDateChangedFactor.PickupCFSAddress);
					Validation.ValidateJS_OA_ExportReceivingDepot();
				}
			}
		}

		public override ZGuid JS_OA_ImportReleaseDepot
		{
			get { return base.JS_OA_ImportReleaseDepot; }
			set
			{
				if (JS_OA_ImportReleaseDepot != value)
				{
					base.JS_OA_ImportReleaseDepot = value;
					UpdateIsCFSRegisteredFromDepot(ImportReleaseDepot);
					ScreeningStatusUpdater.SetShouldUpdateScreeningStatusByOrgAddress(this, JS_ScreeningStatus, Factory, JS_OA_ImportReleaseDepot);
					JS_RL_NKFreightRateDestinationSetDefault();
					DeliveryDueDateFactorHasChanged(DeliveryDueDateChangedFactor.DeliveryCFSAddress);
					Validation.ValidateJS_OA_ImportReleaseDepot();
					this.UpdateCO2eStatusToNotCurrent(new CO2eStatusChangedReason(JS_OA_ImportReleaseDepotInfo), IsCopying);
					UpdateContainersCO2eStatusOnJS_OA_ImportReleaseDepotChanged();
				}
			}
		}

		void UpdateIsCFSRegisteredFromDepot(OrgAddress depotAddress)
		{
			if (JS_IsForwardRegistered && depotAddress != null && depotAddress.Header != null && depotAddress.Header.IsProxyOrgOfAnyCompany())
			{
				JS_IsCFSRegistered = true;
			}
		}

		#endregion

		#region JS_DeliveryDueDate

		public bool JS_DeliveryDueDate_ReadOnly => !Env.Security.MaintainShipmentDeliveryDueDateOverride.IsAllowed;

		[BusinessObjectTestExclude]
		public override ZDateTime JS_DeliveryDueDate
		{
			get
			{
				return base.JS_DeliveryDueDate;
			}
			set
			{
				if (base.JS_DeliveryDueDate == value &&
					currentDeliveryDueDateCalculationMode == DeliveryDueDateCalculationMode.Manual)
				{
					OnDeliveryDueDateNotChangedInManualCalculation();
				}

				if (base.JS_DeliveryDueDate != value &&
					TryGetReasonForChangingDeliveryDueDate(base.JS_DeliveryDueDate, value))
				{
					latestDeliveryDueDateCalculationMode = currentDeliveryDueDateCalculationMode;
					if (currentDeliveryDueDateCalculationMode == DeliveryDueDateCalculationMode.Override)
					{
						CreateOrUpdateDeliveryDueDateCalculationNote(null, base.JS_DeliveryDueDate, value, DeliveryDueDateChangedFactor.None);
					}
					base.JS_DeliveryDueDate = value;
				}
			}
		}

		public event EventHandler<EventArgs> DeliveryDueDateNotChangedInManualCalculation;

		public event EventHandler<ReasonForChangingDeliveryDueDateEventArgs> GetReasonForChangingDeliveryDueDateEventHandler;

		ZDateTime GetCalculatedDeliveryDueDateCore(DeliveryDueDateCalculationMode deliveryDueDateCalculationMode = DeliveryDueDateCalculationMode.Manual)
		{
			var deliveryDueDateCalculatorManager = ObjectFactory.Get<IDeliveryDueDateCalculatorManager>();
			var result = deliveryDueDateCalculatorManager?.Calculate(this);

			if (result == null || !result.IsSuccess)
			{
				return ZDateTime.Empty;
			}
			else
			{
				return result.DeliveryDueDate;
			}
		}

		public void CalculateDeliveryDueDate(DeliveryDueDateCalculationMode deliveryDueDateCalculationMode = DeliveryDueDateCalculationMode.Manual, DeliveryDueDateChangedFactor changedFactor = DeliveryDueDateChangedFactor.None)
		{
			using (new DisposableAction(() => currentDeliveryDueDateCalculationMode = deliveryDueDateCalculationMode, () => currentDeliveryDueDateCalculationMode = default))
			{
				var deliveryDueDateCalculatorManager = ObjectFactory.Get<IDeliveryDueDateCalculatorManager>();
				var result = deliveryDueDateCalculatorManager?.Calculate(this);

				var previousDeliveryDueDate = JS_DeliveryDueDate;

				if (result == null || !result.IsSuccess)
				{
					reasonForNotBeingAbleToCalculateDeliveryDueDate = result?.ErrorMessage ?? ZString.Empty;
					JS_DeliveryDueDate = ZDateTime.Empty;
					if (!IsValidationSuspended)
					{
						Validation.ValidateJS_DeliveryDueDate();
					}
				}
				else
				{
					reasonForNotBeingAbleToCalculateDeliveryDueDate = ZString.Empty;
					JS_DeliveryDueDate = result.DeliveryDueDate;
				}

				CreateOrUpdateDeliveryDueDateCalculationNote(result, previousDeliveryDueDate, JS_DeliveryDueDate, changedFactor);
			}
		}

		void CreateOrUpdateDeliveryDueDateCalculationNote(
				IDeliveryDueDateCalculationResult result,
				ZDateTime previousDeliveryDueDate,
				ZDateTime currentDeliveryDueDate,
				DeliveryDueDateChangedFactor changedFactor,
				IStmALog triggeringEvent = null,
				bool isRevisedDeliveryDueDateNote = false)
		{
			if (IsTemplateRecord)
			{
				return;
			}

			if (isRevisedDeliveryDueDateNote && result.DeliveryDueDate == JS_DeliveryDueDate)
			{
				return;
			}

			var description = PredefinedNoteTypes.Instance.DeliveryDueDateCalculationLog.Description;
			var calculationNote = Notes
				.FindByDescription(description)
				.Where(x => !x.IsNull && x.IsBelongingToCurrentLoginCompany).SingleOrDefault();
			if (calculationNote == null)
			{
				calculationNote = Notes.AddNew();
				calculationNote.ST_Description = description;
			}

			var calculationLogBuilder = new ZStringBuilder();
			AddLoginInfoToNote();
			AddPreviousAndCurrentValuesToNote();
			AddReasonToNote();
			AddCalculationResult();
			if (calculationNote.ST_NoteText != ZString.Empty)
			{
				calculationLogBuilder.AppendLine(Res.GetString("7c0272c3-590e-44aa-8e03-eb5ad603c3e2", "------------------------------------------------------------------------------------"));
				calculationLogBuilder.AppendLine(calculationNote.ST_NoteText);
				calculationNote.ST_NoteText = TrimToFit(calculationLogBuilder.ToString(), calculationNote.NoteTextMaxLength);
			}
			else
			{
				calculationNote.ST_NoteText = TrimToFit(calculationLogBuilder.ToString(), calculationNote.NoteTextMaxLength);
			}
			calculationNote.Validation.ValidateAll();

			void AddLoginInfoToNote()
			{
				calculationLogBuilder.AppendLine(Res.GetString("d5ba4413-5766-475f-a1f7-524cafa380b6", "[Date: {0}] - [User: {1}] - [Company: {2}] - [Branch: {3}]",
													ZDateTime.Now, Env.CurrentUser.FullName, Env.CurrentCompany.Name, Env.CurrentBranch.Name));
			}

			void AddPreviousAndCurrentValuesToNote()
			{
				if (!isRevisedDeliveryDueDateNote)
				{
					calculationLogBuilder.AppendLine(Res.GetString("168ce29e-a228-4066-9ff8-fbab6b04c3e6", "Previous Delivery Due Date: {0}",
						previousDeliveryDueDate == ZDateTime.Empty ? DeliveryDueDateCalculationHelper.EmptyValueSignForLog : previousDeliveryDueDate.ToString()));
					calculationLogBuilder.AppendLine(Res.GetString("f57bd9cb-d537-4feb-90be-02d51e0ac3a3", "Current Delivery Due Date: {0}",
						currentDeliveryDueDate == ZDateTime.Empty ? DeliveryDueDateCalculationHelper.EmptyValueSignForLog : currentDeliveryDueDate.ToString()));
				}

				if (revisedDeliveryDueDateChangeTracker != null && revisedDeliveryDueDateChangeTracker.NewValue != revisedDeliveryDueDateChangeTracker.OriginalValue)
				{
					calculationLogBuilder.AppendLine(Res.GetString("825ef75a-a712-44f7-b3c2-20147cc3ef1f", "Previous Revised Delivery Due Date: {0}",
						revisedDeliveryDueDateChangeTracker.OriginalValue == ZDateTimeOffset.Empty ? DeliveryDueDateCalculationHelper.EmptyValueSignForLog : revisedDeliveryDueDateChangeTracker.OriginalValue.ToString()));
					calculationLogBuilder.AppendLine(Res.GetString("352d233b-a1f2-49c4-bc93-835bcfdb59d5", "Calculated Revised Delivery Due Date: {0}",
						revisedDeliveryDueDateChangeTracker.NewValue == ZDateTimeOffset.Empty ? DeliveryDueDateCalculationHelper.EmptyValueSignForLog : revisedDeliveryDueDateChangeTracker.NewValue.ToString()));
				}
				else if (isRevisedDeliveryDueDateNote)
				{
					calculationLogBuilder.AppendLine(Res.GetString("163dbb3f-fc25-49b1-ae51-7103ac82e421", "Previous Revised Delivery Due Date: {0}",
						JS_RevisedDeliveryDueDate == ZDateTimeOffset.Empty ? DeliveryDueDateCalculationHelper.EmptyValueSignForLog : JS_RevisedDeliveryDueDate.ToZDateTime().ToString()));
					calculationLogBuilder.AppendLine(Res.GetString("4c624687-a050-4230-ba7f-63558ac11f19", "Calculated Revised Delivery Due Date: {0}",
						result.DeliveryDueDate == ZDateTime.Empty ? DeliveryDueDateCalculationHelper.EmptyValueSignForLog : result.DeliveryDueDate.ToString()));
				}
			}

			void AddReasonToNote()
			{
				if (isRevisedDeliveryDueDateNote)
				{
					if (triggeringEvent != null)
					{
						calculationLogBuilder.AppendLine(Res.GetString("c6640041-f84f-4eff-945e-81837ae437f1", "Reason: Event [{0}] triggered a trigger/milestone", triggeringEvent.SL_SE_NKEvent));
					}
					else
					{
						calculationLogBuilder.AppendLine(Res.GetString("c3ccc31e-064c-47c5-895f-9c204c4e95ba", "Reason: {0} has been called", nameof(GetCalculatedDeliveryDueDateWithExceptions)));
					}
				}
				else
				{
					if (deliveryDueDateChangeTracker != null)
					{
						calculationLogBuilder.AppendLine(Res.GetString("de1559cd-b281-4947-96d8-944a52aabe8f", "Reason: {0}", deliveryDueDateChangeTracker.Reason));
					}
					else if (currentDeliveryDueDateCalculationMode == DeliveryDueDateCalculationMode.Override)
					{
						calculationLogBuilder.AppendLine(Res.GetString("dd558d0c-e749-434d-b021-c624eba2e912", "Delivery Due Date has been changed manually"));
					}
					else if (changedFactor == DeliveryDueDateChangedFactor.None)
					{
						calculationLogBuilder.AppendLine(Res.GetString("14a3ae4b-60d7-4d16-a9e4-9c419404f3c0", "Reason: Trigger Action/Manual Calculation", changedFactor.ToString()));
					}
					else
					{
						calculationLogBuilder.AppendLine(Res.GetString("08e6d237-c964-4359-968d-70376a5c19a7", "Reason: {0} changed", changedFactor.ToString()));
					}
				}
			}

			void AddCalculationResult()
			{
				if ((currentDeliveryDueDateCalculationMode != DeliveryDueDateCalculationMode.Override) || isRevisedDeliveryDueDateNote)
				{
					if (result != null)
					{
						calculationLogBuilder.AppendLine();
						calculationLogBuilder.AppendLine(result.CalculationLog.ToString());
					}
					else
					{
						calculationLogBuilder.AppendLine(Res.GetString("8b23b29b-7c9b-4634-9257-b5b00651b725", "Calculation has not been done."));
					}
				}
			}
		}

		ZString TrimToFit(string text, int maxLength, string trimMessage = "...trimmed to fit")
		{
			return (maxLength > 0 && (text?.Length ?? 0) > maxLength) ? (text.Substring(0, maxLength - trimMessage.Length) + trimMessage) : text;
		}

		public enum DeliveryDueDateCalculationMode : byte
		{
			Override,
			Manual,
			Automatic,
			FactorChanged,
			WorkflowTrigger
		}

		void OnDeliveryDueDateNotChangedInManualCalculation()
		{
			if (DeliveryDueDateNotChangedInManualCalculation != null)
			{
				DeliveryDueDateNotChangedInManualCalculation(this, EventArgs.Empty);
			}
		}

		public override void DeliveryDueDateFactorHasChanged(DeliveryDueDateChangedFactor changedFactor)
		{
			currentDeliveryDueDateChangedFactors |= changedFactor;

			if (!IsSettingDefaultOrImportingData
				&& FreightDataRegistry.Instance.CalculateDeliveryDueDateByTransportMode.Value.IsTransportModeActive(JS_TransportMode)
				&& JS_DeliveryDueDate.IsEmpty
				&& AreAllDeliveryDueDateFactorsProvided)
			{
				CalculateDeliveryDueDate(DeliveryDueDateCalculationMode.Automatic, changedFactor);
			}

			if (!IsValidationSuspended)
			{
				Validation.ValidateJS_DeliveryDueDate();
			}
		}

		bool AreAllDeliveryDueDateFactorsProvided =>
			!JS_RS_NKServiceLevel.IsEmpty
			&& DeliveryDueDateCalculator.SupportedDeliveryModes.Contains(JS_HBLContainerPackModeOverride.ToString())
			&& IsDocsAndCartageSet
			&& ReadyDateValidity.IsValid
			&& !JS_TransportMode.IsEmpty
			&& PickupCFSValidity.IsValid
			&& DeliveryCFSValidity.IsValid
			&& (!ConsignorPickupAddress.IsEmpty || !IsHBLContainerPackModeDOOR_X)
			&& (!ConsigneeDeliveryAddress.IsEmpty || !IsHBLContainerPackModeX_DOOR);

		public (bool IsValid, string InvalidFieldName) ReadyDateValidity
		{
			get
			{
				if (JS_IsForwardRegistered)
				{
					if (IsHBLContainerPackModeDOOR_X)
					{
						return (IsDocsAndCartageSet && (!DocsAndCartage.JP_PickupCartageCompleted.IsEmpty || !DocsAndCartage.JP_PickupRequiredBy.IsEmpty), Res.GetString("b0227ab9-c447-4d35-8725-38761a955ec0", "Actual Pickup or Pickup Required By"));
					}
					if (IsHBLContainerPackModeCFS_X)
					{
						return (!JS_A_RCV.IsEmpty || (IsDocsAndCartageSet && !DocsAndCartage.JP_PickupRequiredBy.IsEmpty), Res.GetString("31390f41-d8a1-4a6f-be4e-a7ce4ef3a3c3", "Interim Receipt Date or Pickup Required By"));
					}
				}
				else if (IsHBLContainerPackModeDOOR_X || IsHBLContainerPackModeCFS_X)
				{
					return (IsDocsAndCartageSet && !DocsAndCartage.JP_PickupRequiredBy.IsEmpty, Res.GetString("8270ad63-34dd-4f59-b88a-64ab3403ea5e", "Pickup Required By"));
				}
				if (IsHBLContainerPackModeARPT_X)
				{
					return (true, string.Empty);
				}

				return (false, Res.GetString("f3611e39-7ac1-42df-9d2f-d00ef8bfb6ff", "Valid HBL Delivery Mode"));
			}
		}

		public (bool IsValid, string InvalidFieldName) PickupCFSValidity
		{
			get
			{
				if (JS_OA_ExportReceivingDepot.IsValid)
				{
					return (true, string.Empty);
				}

				var serviceLevel = Factory.LoadFromNaturalKey<RefServiceLevel>(RefServiceLevelSchema.RS_Code, JS_RS_NKServiceLevel);
				if (serviceLevel?.RS_DefaultTransitHours > 0)
				{
					return (true, string.Empty);
				}

				return (false, Res.GetString("2B6667BE-ADF7-422D-B21A-FF4FE009887C", "Origin CFS"));
			}
		}

		public (bool IsValid, string InvalidFieldName) DeliveryCFSValidity
		{
			get
			{
				if ((IsDTC && JS_OH_DeliveryAgent.IsValid) || JS_OA_ImportReleaseDepot.IsValid)
				{
					return (true, string.Empty);
				}

				var serviceLevel = Factory.LoadFromNaturalKey<RefServiceLevel>(RefServiceLevelSchema.RS_Code, JS_RS_NKServiceLevel);
				if (serviceLevel?.RS_DefaultTransitHours > 0)
				{
					return (true, string.Empty);
				}

				return (false, Res.GetString("0fd30daf-74fd-404d-94c6-e57946e97c4c", "Destination CFS"));
			}
		}

		bool TryGetReasonForChangingDeliveryDueDate(ZDateTime originalValue, ZDateTime newValue)
		{
			return TryGetReasonForChangingDeliveryDueDate(originalValue, newValue, ref deliveryDueDateChangeTracker, currentDeliveryDueDateCalculationMode);
		}

		bool TryGetReasonForChangingDeliveryDueDate<T>(
			T originalValue, T newValue, ref ValueChangeTrackerWithReason<T> changeTracker, DeliveryDueDateCalculationMode currentCalculationMode)
			where T : struct
		{
			originalValue = changeTracker?.OriginalValue ?? originalValue;

			if (EqualityComparer<T>.Default.Equals(originalValue, newValue))
			{
				changeTracker = null;
				return true;
			}

			var reason = ZString.Empty;
			if (currentCalculationMode == DeliveryDueDateCalculationMode.Automatic)
			{
				reason = IsInDatabase ? automaticallyCalculated : automaticallyCalculatedForNewShipment;
			}
			else if (currentCalculationMode == DeliveryDueDateCalculationMode.FactorChanged)
			{
				if (currentDeliveryDueDateChangedFactors == DeliveryDueDateChangedFactor.None)
				{
					return false;
				}

				reason = IsInDatabase ? DeliveryDueDateChangedFactorsHelper.GetChangedEventReason(currentDeliveryDueDateChangedFactors) : automaticallyCalculatedForNewShipment;
			}
			else if (currentCalculationMode == DeliveryDueDateCalculationMode.WorkflowTrigger)
			{
				var unknownReason = Res.GetString("ae5e99a0-1de0-42bd-80d0-e5eb2bcde701", "Unknown");
				reason = triggerActionReason;
				if (triggeringEvent != null && triggeringEvent.SL_SE_NKEvent == AutoEvents.ExceptionRaisedCode)
				{
					return false;
				}
				else if (triggeringEvent != null && triggeringEvent.SL_SE_NKEvent == AutoEvents.CalculateDeliveryDateWithExceptionsRequestedCode
					&& triggeringEvent.Parameters.TryGetValue(Params.FieldChange, out var changeReason))
				{
					reason = changeReason;
				}
			}
			else
			{
				if (GetReasonForChangingDeliveryDueDateEventHandler == null)
				{
					return false;
				}

				var args = new ReasonForChangingDeliveryDueDateEventArgs();

				if (!Factory.IsInTransaction)
				{
					GetReasonForChangingDeliveryDueDateEventHandler(this, args);
				}

				if (args.Reason.IsEmpty)
				{
					return false;
				}

				reason = args.Reason;
			}

			changeTracker = new ValueChangeTrackerWithReason<T>(originalValue, newValue, reason, currentCalculationMode.ToString());
			return true;
		}

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Search string prefix")]
		string GetExceptionNameFromEventReference(string eventReference)
		{
			if (!eventReference.StartsWith("Type:"))
			{
				return string.Empty;
			}

			int start = eventReference.IndexOf('[');
			int end = eventReference.IndexOf(']');
			int length = end - start - 1;
			var exceptionCode = eventReference.Substring(start + 1, length);
			var exceptionType = Factory.LoadTop1<ProcessWorkflowExceptionType>(new ZQuery(ProcessWorkflowExceptionTypeSchema.WET_Code, exceptionCode));

			return exceptionType != null ? exceptionType.WET_DescriptionMultilingual : string.Empty;
		}

		void LogDeliveryDateUpdatedEvent()
		{
			if (deliveryDueDateChangeTracker != null && deliveryDueDateChangeTracker.NewValue != deliveryDueDateChangeTracker.OriginalValue)
			{
				var parameters = new Dictionary<string, string>();
				if (!deliveryDueDateChangeTracker.OriginalValue.IsEmpty)
				{
					parameters.Add(Params.Old, deliveryDueDateChangeTracker.OriginalValue.ToLongTimeString());
				}

				parameters.Add(Params.Type, Res.GetString("bb3e75c1-10f5-40d1-bec3-df4424d58db4", "Original"));
				parameters.Add(Params.New, deliveryDueDateChangeTracker.NewValue.ToLongTimeString());
				parameters.Add(Params.Reason, deliveryDueDateChangeTracker.Reason);
				parameters.Add(Params.Action, latestDeliveryDueDateCalculationMode.ToString());

				Logs.AddNew(Events.DeliveryDateUpdated, ZDateTimeOffset.Now, parameters.ToArray());
			}
		}

		DeliveryDueDateCalculationMode currentDeliveryDueDateCalculationMode;
		DeliveryDueDateCalculationMode latestDeliveryDueDateCalculationMode;
		DeliveryDueDateChangedFactor currentDeliveryDueDateChangedFactors;
		ZString reasonForNotBeingAbleToCalculateDeliveryDueDate = ZString.Empty;

		public ZString ReasonForNotBeingAbleToCalculateDeliveryDueDate
		{
			get => reasonForNotBeingAbleToCalculateDeliveryDueDate;
		}

		ValueChangeTrackerWithReason<ZDateTime> deliveryDueDateChangeTracker;
		public const string ConvertingBookingToShipment = "ConvertingBookingToShipment";

		public void CreateDeliveryDueDateChangeTracker(string reason)
		{
			deliveryDueDateChangeTracker = new ValueChangeTrackerWithReason<ZDateTime>(
				JS_DeliveryDueDate,
				JS_DeliveryDueDate,
				reason);
		}

		public override void CalculateDeliveryDueDateIfNecessary()
		{
			var isImportingDataWithBlankOrMissingDeliveryDueDate = IsSettingDefaultOrImportingData && JS_DeliveryDueDate.IsEmpty;

			if (FreightDataRegistry.Instance.CalculateDeliveryDueDateByTransportMode.Value.IsTransportModeActive(JS_TransportMode)
				&& (IsInDatabase || isImportingDataWithBlankOrMissingDeliveryDueDate)
				&& (!JS_DeliveryDueDateInfo.HasChanges || isImportingDataWithBlankOrMissingDeliveryDueDate)
				&& AreAllDeliveryDueDateFactorsProvided
				&& currentDeliveryDueDateChangedFactors != DeliveryDueDateChangedFactor.None)
			{
				CalculateDeliveryDueDate(DeliveryDueDateCalculationMode.FactorChanged, currentDeliveryDueDateChangedFactors);
				LogDeliveryDateUpdatedEvent();

				if (JS_IsForwardRegistered)
				{
					LogRDDByDeliveryDueDateChanged();
				}
			}
		}

		public ZBool IsDTC
		{
			get
			{
				return (JS_HBLContainerPackModeOverride == Core.Constants.HBLDeliveryModes.Codes.DOOR_DOOR
					|| JS_HBLContainerPackModeOverride == Core.Constants.HBLDeliveryModes.Codes.CFS_DOOR)
					&& (JS_OA_ImportReleaseDepot_ZAddress?.OrgAddress as OrgAddress) == null;
			}
		}

		void LogRDDByDeliveryDueDateChanged()
		{
			if (deliveryDueDateChangeTracker != null && deliveryDueDateChangeTracker.NewValue != deliveryDueDateChangeTracker.OriginalValue)
			{
				var originalValue = deliveryDueDateChangeTracker.OriginalValue.IsEmpty
					? (NoResString)"Blank"
					: deliveryDueDateChangeTracker.OriginalValue.ToLongTimeString();

				var newValue = deliveryDueDateChangeTracker.NewValue.IsEmpty
					? (NoResString)"Blank"
					: deliveryDueDateChangeTracker.NewValue.ToLongTimeString();

				var populatedMessage = $"Delivery Due Date Changed {originalValue} To {newValue}";

				var parameters = new Dictionary<string, string>
				{
					{ Params.FieldChange, populatedMessage }
				};

				Logs.AddNew(Events.CalculateDeliveryDateWithExceptionsRequested, ZDateTimeOffset.Now, parameters.ToArray());
			}
			else if (deliveryDueDateChangeTracker != null && deliveryDueDateChangeTracker.Reason == ConvertingBookingToShipment)
			{
				var populatedMessage = $"Delivery Due Date {JS_DeliveryDueDate} Converted From Booking";

				var parameters = new Dictionary<string, string>
				{
					{ Params.FieldChange, populatedMessage }
				};

				Logs.AddNew(Events.CalculateDeliveryDateWithExceptionsRequested, ZDateTimeOffset.Now, parameters.ToArray());
			}
		}

		public ZDateTime GetCalculatedDeliveryDueDate
		{
			get => GetCalculatedDeliveryDueDateCore(DeliveryDueDateCalculationMode.WorkflowTrigger);
		}

		#endregion

		#region JS_RevisedDeliveryDueDate

		protected bool JS_RevisedDeliveryDueDate_ReadOnly => !Env.Security.MaintainShipmentRevisedDeliveryDueDateOverride.IsAllowed;

		[BusinessObjectTestExclude]
		public override ZDateTimeOffset JS_RevisedDeliveryDueDate
		{
			get
			{
				return base.JS_RevisedDeliveryDueDate;
			}
			set
			{
				if (base.JS_RevisedDeliveryDueDate == value && currentRevisedDeliveryDueDateCalculationMode == DeliveryDueDateCalculationMode.Manual)
				{
					OnDeliveryDueDateNotChangedInManualCalculation();
				}

				if (base.JS_RevisedDeliveryDueDate != value &&
					TryGetReasonForChangingRevisedDeliveryDueDate(base.JS_RevisedDeliveryDueDate, value))
				{
					base.JS_RevisedDeliveryDueDate = value;
					// IFC trigger action is fired after OnFactorySaving, so we need to add log before saving.
					if (currentRevisedDeliveryDueDateCalculationMode == DeliveryDueDateCalculationMode.WorkflowTrigger)
					{
						LogRevisedDeliveryDateUpdatedEvent();
					}
				}
			}
		}

		protected ZDateTime GetCalculatedDeliveryDueDateWithExceptionsCore(DeliveryDueDateCalculationMode deliveryDueDateCalculationMode = DeliveryDueDateCalculationMode.Manual)
		{
			var calculator = new DeliveryDueDateWithExceptionsCalculator(this, triggeringEvent);
			var result = calculator.CalculateDeliveryDueDateWithExceptions();
			CreateOrUpdateDeliveryDueDateCalculationNote(result, base.JS_DeliveryDueDate, base.JS_DeliveryDueDate, DeliveryDueDateChangedFactor.None, triggeringEvent, isRevisedDeliveryDueDateNote: true);
			return result.DeliveryDueDate;
		}

		bool TryGetReasonForChangingRevisedDeliveryDueDate(ZDateTimeOffset originalValue, ZDateTimeOffset newValue)
		{
			return TryGetReasonForChangingDeliveryDueDate(originalValue, newValue, ref revisedDeliveryDueDateChangeTracker, currentRevisedDeliveryDueDateCalculationMode);
		}

		void LogRevisedDeliveryDateUpdatedEvent()
		{
			if (IsDeleted || revisedDeliveryDueDateChangeTracker == null)
			{
				return;
			}

			var isFirstAutomaticChange = revisedDeliveryDueDateChangeTracker.OriginalValue.IsEmpty
				&& !(revisedDeliveryDueDateChangeTracker.Source == nameof(DeliveryDueDateCalculationMode.Override) || revisedDeliveryDueDateChangeTracker.Source == nameof(DeliveryDueDateCalculationMode.Manual));

			if (revisedDeliveryDueDateChangeTracker.NewValue != revisedDeliveryDueDateChangeTracker.OriginalValue
				&& !isFirstAutomaticChange)
			{
				var parameters = new Dictionary<string, string>();
				if (!revisedDeliveryDueDateChangeTracker.OriginalValue.IsEmpty)
				{
					parameters.Add(Params.Old, revisedDeliveryDueDateChangeTracker.OriginalValue.ToZDateTime().ToLongTimeString());
				}

				parameters.Add(Params.Type, Res.GetString("647d3092-66ee-4094-bfbe-93e337685c1a", "Revised"));
				parameters.Add(Params.New, revisedDeliveryDueDateChangeTracker.NewValue.ToZDateTime().ToLongTimeString());
				parameters.Add(Params.Reason, revisedDeliveryDueDateChangeTracker.Reason);
				parameters.Add(Params.Action, currentRevisedDeliveryDueDateCalculationMode.ToString());

				Logs.AddNew(Events.DeliveryDateUpdated, ZDateTimeOffset.Now, parameters.ToArray());
				revisedDeliveryDueDateChangeTracker = null;
			}
		}

		DeliveryDueDateCalculationMode currentRevisedDeliveryDueDateCalculationMode;

		ValueChangeTrackerWithReason<ZDateTimeOffset> revisedDeliveryDueDateChangeTracker;

		public ZDateTime GetCalculatedDeliveryDueDateWithExceptions
		{
			get => GetCalculatedDeliveryDueDateWithExceptionsCore(DeliveryDueDateCalculationMode.WorkflowTrigger);
		}

		#endregion

		#region JS_IsHighRisk

		public override ZBool JS_IsHighRisk
		{
			get
			{
				return base.JS_IsHighRisk;
			}
			set
			{
				if (!value == JS_IsHighRisk)
				{
					if (!JS_IsHighRiskHasChanges && !IsSettingDefaultValues)
					{
						JS_IsHighRiskOriginalValue = JS_IsHighRisk;
						JS_IsHighRiskHasChanges = true;
					}

					base.JS_IsHighRisk = value;

					if (!value)
					{
						JS_AdditionalInspectionTypeCode = FreightDataRegistry.AviationSecurity_Unknown_Code;
					}

					if (!IsSettingDefaultOrImportingData)
					{
						bool packLineIsHighRiskNeedsUpdate = JS_IsHighRisk
							? OuterPackLines.Cast<ForwardingPackLine>().Any(x => !x.JL_IsHighRisk)
							: OuterPackLines.Cast<ForwardingPackLine>().Any(x => x.JL_IsHighRisk);

						if (!IsUpdatedByIsHighRiskPackLineLevel && packLineIsHighRiskNeedsUpdate)
						{
							ReDefaultPackLineIsHighRisk(true);
						}
						else
						{
							IsUpdatedByIsHighRiskPackLineLevel = false;
						}
					}

					JS_AdditionalInspectionTypeCodeInfo.RefreshBinding();

					Validation.ValidateJS_InspectionTypeCode();
					Validation.ValidateJS_AdditionalInspectionTypeCode();
				}
			}
		}
		public bool JS_IsHighRiskHasChanges { get; set; }
		public ZBool JS_IsHighRiskOriginalValue { get; set; }

		public bool ApplyShipmentIsHighRisk = true;
		public bool IsUpdatedByIsHighRiskPackLineLevel;

		public bool JS_IsHighRisk_ReadOnly => !Env.Security.MaintainShipmentAllowToOverrideInspection.IsAllowed;

		public event CancelEventHandler ReDefaultPackLineIsHighRiskEventHandler;

		internal void ReDefaultPackLineIsHighRisk(bool isSettingNewIsHighRisk = false)
		{
			var reDefaultIsHighRisk = false;
			if (ReDefaultPackLineIsHighRiskEventHandler != null
				&& isSettingNewIsHighRisk
				&& OuterPackLines.Cast<ForwardingPackLine>().Any(p => !p.JL_IsHighRiskInfo.ReadOnly)
				&& AviationSecurity.SupplyChainSecurityConfiguration.IsHighRiskApplicable)
			{
				var e = new CancelEventArgs();

				if (!Factory.IsInTransaction)
				{
					ReDefaultPackLineIsHighRiskEventHandler(this, e);
				}

				if (!e.Cancel)
				{
					ApplyShipmentIsHighRisk = true;
					reDefaultIsHighRisk = true;
				}
				else
				{
					ApplyShipmentIsHighRisk = false;
				}
			}

			if (!IsRedefaultingAdditionalInspectionTypeCodesSuspended)
			{
				using (SuspendRedefaultingIsHighRisk())
				{
					OuterPackLines.Cast<ForwardingPackLine>().ForEach(p => p.ReDefaultIsHighRisk(reDefaultIsHighRisk));
				}
			}
		}

		#region Redefaulting Is High Risk

		internal IDisposable SuspendRedefaultingIsHighRisk()
		{
			return new DisposableAction(() => isRedefaultingIsHighRiskIndex++, () => isRedefaultingIsHighRiskIndex--);
		}

		public bool IsRedefaultingIsHighRiskSuspended => isRedefaultingIsHighRiskIndex > 0;

		int isRedefaultingIsHighRiskIndex;

		#endregion

		#endregion

		#region Aviation Security Inspection Type

		#region JS_InspectionTypeCode

		public override ZString JS_InspectionTypeCode
		{
			get { return base.JS_InspectionTypeCode; }
			set
			{
				if (base.JS_InspectionTypeCode == value)
				{
					return;
				}
				else
				{
					if (!JS_InspectionTypeCodeHasChanges && !IsSettingDefaultValues)
					{
						JS_InspectionTypeCodeOriginalValue = JS_InspectionTypeCode;
					}

					if (isUpdatedFromParentHVMShipment)
					{
						base.JS_InspectionTypeCode = value;
					}
					else if (TryGetReasonChangingInspectionStatus(base.JS_InspectionTypeCode, value))
					{
						var previousValue = base.JS_InspectionTypeCode;
						base.JS_InspectionTypeCode = value;

						if (!isAutomaticInspectionTypeCalculation)
						{
							MostRecentInspectionTypeChangeReason = ZString.Empty;
						}

						if (base.JS_ShipmentType == Constants.ShipmentTypes.HighVolumeLowValueMaster)
						{
							UpdateSubHVLShipmentsInspectionType();
						}

						if ((previousValue == BaseJobShipmentLookups.InspectionType_Approved || previousValue == BaseJobShipmentLookups.InspectionType_Screened)
								&& RequiresSecuredCargoFromWarehouse
								&& !IsSettingDefaultOrImportingData
								&& !IsRedefaultingInspectionTypeCodesSuspended)
						{
							using (SuspendRedefaultingInspectionTypeCodes())
							{
								foreach (ForwardingPackLine packLine in OuterPackLines)
								{
									packLine.JL_InspectionTypeCode = FreightDataRegistry.AviationSecurity_Unknown_Code;
								}
							}
						}

						if (!IsSettingDefaultOrImportingData
						&& AviationSecurity.SupplyChainSecurityConfiguration.ShouldReDefaultPackLineInspectionTypeCodes(this)
							&& !RequiresSecuredCargoFromWarehouse)
						{
							ReDefaultPackLineInspectionTypeCodes(true);
						}

						if (!isAutomaticInspectionTypeCalculation)
						{
							foreach (var relatedCountry in AviationSecurity.SupplyChainSecurityConfiguration.RelatedCountriesForSupplyChainSecurity)
							{
								LoadOrCreateInspectionTypeCusEntryNumber(relatedCountry).CE_EntryNum =
									JS_InspectionTypeCode;
							}
						}

						if (!IsValidationSuspended)
						{
							foreach (Transport transport in TransportsIncludingRelated)
							{
								transport.Validation.ValidateJW_IsCargoOnly();
							}

							foreach (ForwardingPackLine packLine in OuterPackLines)
							{
								packLine.Validation.ValidateJL_InspectionTypeCode();
							}

							Validation.ValidateJS_AdditionalInspectionTypeCode();
						}
					}
				}
			}
		}

		public ZString JS_InspectionTypeCodeOriginalValue { get; set; }

		protected bool JS_InspectionTypeCode_ReadOnly
		{
			get
			{
				return !(AviationSecurity.IsAviationSecurityApplicableForTransportMode
						&& (AviationSecurity.SupplyChainSecurityConfiguration.IsEnabled || AviationSecurity.SupplyChainSecurityConfiguration.UsesGenericScheme)
						&& (JS_TransportMode != Core.Constants.TransportModes.SeaAir || IsTranshipmentFromCurrentCountryByAir()))
					|| !Env.Security.MaintainShipmentAllowToOverrideInspection.IsAllowed;
			}
		}

		bool IsTranshipmentFromCurrentCountryByAir()
		{
			return TransportsInLegOrder.Cast<Transport>().FirstOrDefault(transport => transport.JW_TransportMode == Core.Constants.TransportModes.Air)?.LoadPort?.Country?.Code == GlbCompany.CurrentCompany?.GC_RN_NKCountryCode;
		}

		internal bool HasUserSelectedUnknownInspectionType
		{
			get
			{
				if (JS_InspectionTypeCode == FreightDataRegistry.AviationSecurity_Unknown_Code)
				{
					if (InspectionTypeTracker != null)
					{
						return InspectionTypeTracker.Reason != automaticallyCalculated;
					}

					return HasSecurityLogForUserSelectedInspectionType;
				}

				return false;
			}
		}

		bool HasSecurityLogForUserSelectedInspectionType
		{
			get
			{
				var logQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.SecurityModified.Code);
				logQuery.AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.Contains, FormattableString.Invariant($"|{Params.New}="));
				logQuery.AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.NotContains, FormattableString.Invariant($"|{Params.Type}={highRisk}"));

				var secEvent = Logs.Find(logQuery)
					.Where(l => !l.SL_IsCancelled && !PropagationHandler.IsPropagatedEventLog(l))
					.OrderByDescending(l => l.SL_PostedTimeUtc)
					.FirstOrDefault();

				return secEvent != null
					&& secEvent.Parameters.ContainsKey(Params.New)
					&& secEvent.Parameters[Params.New] == FreightDataRegistry.AviationSecurity_Unknown_Code
					&& (!secEvent.Parameters.ContainsKey(Params.Reason) || secEvent.Parameters[Params.Reason] != automaticallyCalculated);
			}
		}

		public event CancelEventHandler ReDefaultPackLineInspectionTypeCodesEventHandler;

		internal void ReDefaultPackLineInspectionTypeCodes(bool isSettingNewInspectionTypeCode = false)
		{
			var applyShipmentInspectionType = false;

			if (ReDefaultPackLineInspectionTypeCodesEventHandler != null
				&& isSettingNewInspectionTypeCode
				&& OuterPackLines.Any()
				&& JS_InspectionTypeCode != BaseJobShipmentLookups.InspectionType_Approved
				&& JS_InspectionTypeCode != BaseJobShipmentLookups.InspectionType_Screened
				&& JS_InspectionTypeCode != FreightDataRegistry.AviationSecurity_Unknown_Code
				&& Lookups.InspectionTypes.ContainsCode(JS_InspectionTypeCode)
				&& AviationSecurity.SupplyChainSecurityConfiguration.IsPackLevelScreeningAvailable(this))
			{
				var e = new CancelEventArgs();

				if (!Factory.IsInTransaction)
				{
					ReDefaultPackLineInspectionTypeCodesEventHandler(this, e);
				}

				if (!e.Cancel)
				{
					applyShipmentInspectionType = true;
				}
			}

			if (!IsRedefaultingInspectionTypeCodesSuspended)
			{
				using (SuspendRedefaultingInspectionTypeCodes())
				{
					foreach (ForwardingPackLine packLine in OuterPackLines)
					{
						packLine.ReDefaultInspectionTypeCode(applyShipmentInspectionType);
					}
				}
			}
		}

		public override bool SupportsPackLineApprovedCode => AviationSecurity.SupplyChainSecurityConfiguration.IsSecuredForPackLineInspectionType;

		public override bool RequiresSecuredCargoFromWarehouse => SupportsPackLineApprovedCode
			&& this.IsAir
			&& this.IsExport()
			&& (JS_Calc_LastKnownTransitWarehouseStatus != ZString.Empty && JS_Calc_LastKnownTransitWarehouseStatus != (": " + UnknownTWStatus))
			&& WarehouseDataRegistry.Instance.DriverSecurityCertificationCheckingActivated.Value;

		#endregion

		#region Inspection Type Tracker

		ValueChangeTrackerWithReason<string> InspectionTypeTracker;

		bool TryGetReasonChangingInspectionStatus(string originalValue, string newValue)
		{
			if (InspectionTypeTracker != null
				&& !isAutomaticInspectionTypeCalculation
				&& InspectionTypeTracker.OriginalValue == FreightDataRegistry.AviationSecurity_Unknown_Code
				&& originalValue != FreightDataRegistry.AviationSecurity_Unknown_Code
				&& newValue == FreightDataRegistry.AviationSecurity_Unknown_Code
				&& !HasSecurityLogForUserSelectedInspectionType)
			{
				originalValue = string.Empty;
			}
			else
			{
				originalValue = InspectionTypeTracker != null ? InspectionTypeTracker.OriginalValue : originalValue;
			}

			if (originalValue == newValue)
			{
				InspectionTypeTracker = null;
				return true;
			}

			var reason = ZString.Empty;
			if (isAutomaticInspectionTypeCalculation)
			{
				reason = automaticallyCalculated;
			}
			else if (overridenChangingInspectionStatusReason != default)
			{
				reason = overridenChangingInspectionStatusReason;
			}
			else if (originalValue != FreightDataRegistry.AviationSecurity_Unknown_Code
				&& GetReasonChangingSecurityInspectionStatusEventHandler != null
				&& AviationSecurity.SupplyChainSecurityConfiguration.GetUncertifiedUserForScreeningError(GlbStaff.CurrentUser).IsEmpty)
			{
				var args = new ReasonForChangingSecurityInspectionStatusEventArgs();

				if (!Factory.IsInTransaction)
				{
					GetReasonChangingSecurityInspectionStatusEventHandler(this, args);
				}

				if (args.Reason.IsEmpty)
				{
					return false;
				}

				reason = args.Reason;
			}

			InspectionTypeTracker = new ValueChangeTrackerWithReason<string>(originalValue, newValue, reason);

			return true;
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Event Parameter")]
		const string automaticallyCalculated = "Automatically calculated";
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Event Parameter")]
		const string automaticallyCalculatedForNewShipment = "Automatically calculated for new shipment";
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Event Parameter")]
		const string triggerActionReason = "Trigger Action";

		#endregion

		#region Update Sub HVL Shipment Inspection Type

		public event CancelEventHandler UpdateSubHVLShipmentsInspectionTypeEventHandler;

		bool isUpdatedFromParentHVMShipment;

		DisposableAction SetIsUpdatedFromParentHVMShipment()
		{
			isUpdatedFromParentHVMShipment = true;
			return new DisposableAction(() => isUpdatedFromParentHVMShipment = false);
		}

		internal void UpdateSubHVLShipmentsInspectionType()
		{
			var subHVLShipments = CoLoadShipments.Where(s => s.JS_ShipmentType == Constants.ShipmentTypes.HighVolumeLowValue);
			if (UpdateSubHVLShipmentsInspectionTypeEventHandler != null
				&& subHVLShipments.Any()
				&& Lookups.InspectionTypes.ContainsCode(JS_InspectionTypeCode))
			{
				var e = new CancelEventArgs();

				if (!Factory.IsInTransaction)
				{
					UpdateSubHVLShipmentsInspectionTypeEventHandler(this, e);
				}

				if (!e.Cancel)
				{
					foreach (ForwardingShipment shipment in subHVLShipments)
					{
						using (shipment.SetIsUpdatedFromParentHVMShipment())
						{
							shipment.JS_InspectionTypeCode = JS_InspectionTypeCode;
						}
					}
				}
			}
		}

		#endregion

		#region JS_AdditionalInspectionTypeCode

		public override ZString JS_AdditionalInspectionTypeCode
		{
			get { return base.JS_AdditionalInspectionTypeCode; }
			set
			{
				if (base.JS_AdditionalInspectionTypeCode != value && TryGetReasonChangingAdditionalInspectionStatus(base.JS_AdditionalInspectionTypeCode, value))
				{
					if (!JS_AdditionalInspectionTypeCodeHasChanges && !IsSettingDefaultValues)
					{
						JS_AdditionalInspectionTypeCodeOriginalValue = JS_AdditionalInspectionTypeCode;
					}
					using (GetValidationSuspender())
					{
						base.JS_AdditionalInspectionTypeCode = value;
					}
					if (!IsSettingDefaultOrImportingData && JS_IsHighRisk)
					{
						if (!IsUpdatedByAdditionalInspectionTypePackLineLevel && OuterPackLines.Cast<ForwardingPackLine>().Any(x => x.JL_IsHighRisk))
						{
							ReDefaultPackAdditionalLineInspectionTypeCodes(true);
						}
						else
						{
							IsUpdatedByAdditionalInspectionTypePackLineLevel = false;
							if (!IsValidationSuspended)
							{
								Validation.ValidateJS_AdditionalInspectionTypeCode();
							}
						}
					}

					if (!IsValidationSuspended)
					{
						Validation.ValidateJS_InspectionTypeCode();
					}
				}
			}
		}

		public ZString JS_AdditionalInspectionTypeCodeOriginalValue { get; set; }

		public bool ApplyShipmentAdditionalInspectionType = true;
		public bool IsUpdatedByAdditionalInspectionTypePackLineLevel;
		public bool HaveFinishedRedefault = true;
		public bool JS_AdditionalInspectionTypeCode_ReadOnly => !AviationSecurity.IsHighRiskShipment || !Env.Security.MaintainShipmentAllowToOverrideInspection.IsAllowed;

		public event CancelEventHandler ReDefaultPackLineAdditionalInspectionTypeCodesEventHandler;

		internal void ReDefaultPackAdditionalLineInspectionTypeCodes(bool isSettingNewAdditionalInspectionTypeCode = false)
		{
			var reDefaultAdditionalInspectionTypeCode = false;
			if (ReDefaultPackLineAdditionalInspectionTypeCodesEventHandler != null
				&& isSettingNewAdditionalInspectionTypeCode
				&& OuterPackLines.Cast<ForwardingPackLine>().Any(p => !p.JL_AdditionalInspectionTypeCodeInfo.ReadOnly)
				&& Lookups.AdditionalInspectionTypes.ContainsCode(JS_AdditionalInspectionTypeCode)
				&& AviationSecurity.SupplyChainSecurityConfiguration.IsHighRiskApplicable
				&& AviationSecurity.SupplyChainSecurityConfiguration.GetUncertifiedUserForScreeningError(GlbStaff.CurrentUser).IsEmpty
				&& JS_IsHighRisk
				&& JS_AdditionalInspectionTypeCode != BaseJobShipmentLookups.InspectionType_Screened)
			{
				var e = new CancelEventArgs();

				if (!Factory.IsInTransaction)
				{
					ReDefaultPackLineAdditionalInspectionTypeCodesEventHandler(this, e);
				}

				if (!e.Cancel)
				{
					ApplyShipmentAdditionalInspectionType = true;
					reDefaultAdditionalInspectionTypeCode = true;
				}
				else
				{
					ApplyShipmentAdditionalInspectionType = false;
				}
			}

			if (!IsRedefaultingAdditionalInspectionTypeCodesSuspended)
			{
				using (SuspendRedefaultingAdditionalInspectionTypeCodes())
				{
					HaveFinishedRedefault = false;
					OuterPackLines.Cast<ForwardingPackLine>().Where(p => p.JL_IsHighRisk).ForEach(x => x.ReDefaultAdditionalInspectionTypeCode(reDefaultAdditionalInspectionTypeCode));
					HaveFinishedRedefault = true;
				}
			}
			if (!IsValidationSuspended)
			{
				Validation.ValidateJS_AdditionalInspectionTypeCode();
			}
		}

		#region Redefaulting Additional Inspection Type Codes Suspender

		internal IDisposable SuspendRedefaultingAdditionalInspectionTypeCodes()
		{
			return new DisposableAction(() => isRedefaultingAdditionalInspectionTypeCodesIndex++, () => isRedefaultingAdditionalInspectionTypeCodesIndex--);
		}

		public bool IsRedefaultingAdditionalInspectionTypeCodesSuspended => isRedefaultingAdditionalInspectionTypeCodesIndex > 0;

		int isRedefaultingAdditionalInspectionTypeCodesIndex;

		#endregion

		#endregion

		#region Additional Inspection Type Tracker

		ValueChangeTrackerWithReason<string> AdditionalInspectionTypeTracker;

		bool TryGetReasonChangingAdditionalInspectionStatus(string originalValue, string newValue)
		{
			if (AdditionalInspectionTypeTracker != null)
			{
				originalValue = AdditionalInspectionTypeTracker.OriginalValue;
			}

			if (originalValue == newValue)
			{
				AdditionalInspectionTypeTracker = null;
				return true;
			}

			var reason = ZString.Empty;
			if (overridenChangingAdditionalInspectionStatusReason != default)
			{
				reason = overridenChangingAdditionalInspectionStatusReason;
			}
			else if (originalValue != FreightDataRegistry.AviationSecurity_Unknown_Code
				&& GetReasonChangingSecurityAdditionalInspectionStatusEventHandler != null
				&& AviationSecurity.SupplyChainSecurityConfiguration.GetUncertifiedUserForScreeningError(GlbStaff.CurrentUser).IsEmpty)
			{
				var args = new ReasonForChangingSecurityInspectionStatusEventArgs();
				GetReasonChangingSecurityAdditionalInspectionStatusEventHandler(this, args);
				if (args.Reason.IsEmpty)
				{
					return false;
				}

				reason = args.Reason;
			}

			AdditionalInspectionTypeTracker = new ValueChangeTrackerWithReason<string>(originalValue, newValue, reason);

			return true;
		}

		#endregion

		public event EventHandler<ReasonForChangingSecurityInspectionStatusEventArgs> GetReasonChangingSecurityInspectionStatusEventHandler;

		public event EventHandler<ReasonForChangingSecurityInspectionStatusEventArgs> GetReasonChangingSecurityAdditionalInspectionStatusEventHandler;

		public IDisposable OverrideChangingInspectionStatusReason(string reason)
		{
			if (overridenChangingInspectionStatusReason != default)
			{
				return DisposableAction.NoAction;
			}
			return new DisposableAction(() => overridenChangingInspectionStatusReason = reason, () => overridenChangingInspectionStatusReason = default);
		}
		string overridenChangingInspectionStatusReason;

		public IDisposable OverrideChangingAdditionalInspectionStatusReason(string reason)
		{
			if (overridenChangingAdditionalInspectionStatusReason != default)
			{
				return DisposableAction.NoAction;
			}
			return new DisposableAction(() => overridenChangingAdditionalInspectionStatusReason = reason, () => overridenChangingAdditionalInspectionStatusReason = default);
		}
		string overridenChangingAdditionalInspectionStatusReason;

		void LogInspectionTypeCodeChanges()
		{
			foreach (var logAdditionalInspectionTypeChanges in new[] { false, true })
			{
				var tracker = logAdditionalInspectionTypeChanges ? AdditionalInspectionTypeTracker : InspectionTypeTracker;
				if (tracker != null && tracker.NewValue != tracker.OriginalValue)
				{
					var typeParameter = logAdditionalInspectionTypeChanges ? highRisk : string.Empty;

					var parameters = new KeyValuePair<string, string>[]
					{
						new KeyValuePair<string, string>(Params.Old, tracker.OriginalValue),
						new KeyValuePair<string, string>(Params.New, tracker.NewValue),
						new KeyValuePair<string, string>(Params.Type, typeParameter),
						new KeyValuePair<string, string>(Params.Reason, tracker.Reason)
					};

					Logs.AddNew(Events.SecurityModified, ZDateTimeOffset.Now, parameters);
				}
				if (logAdditionalInspectionTypeChanges)
				{
					AdditionalInspectionTypeTracker = null;
				}
				else
				{
					InspectionTypeTracker = null;
				}
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Event Parameter")]
		const string highRisk = "High Risk";

		#endregion

		#region ValueChangeTrackerWithReason

		class ValueChangeTrackerWithReason<T>
		{
			public ValueChangeTrackerWithReason(T originalValue, T newValue, string reason, string source = "")
			{
				OriginalValue = originalValue;
				NewValue = newValue;
				Reason = reason;
				Source = source;
			}

			public T OriginalValue { get; }
			public T NewValue { get; }
			public string Reason { get; }
			public string Source { get; }
		}

		#endregion

		bool ShouldDefaultInsuranceDetailsFromGoodsDetails
			=> FreightDataRegistry.Instance.InsuranceValueDefaulting.Value && !InsuranceDetailsManuallyEdited;

		bool InsuranceDetailsManuallyEdited
		{
			get
			{
				return (JS_InsuranceValue != 0 && JS_GoodsValue != JS_InsuranceValue)
					|| (!JS_RX_NKInsuranceCurrency.IsEmpty && JS_RX_NKGoodsValueCurr != JS_RX_NKInsuranceCurrency);
			}
		}

		#region JS_ScreeningStatus

		[ReadOnly(true)]
		[List("Lookups.ScreeningStatusesList")]
		public override ZString JS_ScreeningStatus
		{
			get { return base.JS_ScreeningStatus; }
			set
			{
				if (value != base.JS_ScreeningStatus)
				{
					base.JS_ScreeningStatus = value;

					var originalShouldUpdateScreeningStatus = (this as IShouldUpdateScreeningStatus).ShouldUpdateScreeningStatus;

					foreach (var item in Declarations)
					{
						if (item.JE_ScreeningStatus != value)
						{
							item.JE_ScreeningStatus = value;
						}
					}

					(this as IShouldUpdateScreeningStatus).ShouldUpdateScreeningStatus = originalShouldUpdateScreeningStatus;

					if (value == ScreeningStatusesList.Codes.Matched)
					{
						foreach (ForwardingConsol consol in Consols)
						{
							if (consol.JK_ScreeningStatus != ScreeningStatusesList.Codes.Matched)
							{
								consol.JK_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
							}
						}
					}
					else if (value == ScreeningStatusesList.Codes.Unknown)
					{
						foreach (ForwardingConsol consol in Consols)
						{
							if (consol.JK_ScreeningStatus == ScreeningStatusesList.Codes.Clear || consol.JK_ScreeningStatus == ScreeningStatusesList.Codes.JobCleared)
							{
								consol.JK_ScreeningStatus = ScreeningStatusesList.Codes.Unknown;
							}
						}
					}

					foreach (ForwardingConsol consol in Consols)
					{
						if (ConsolShouldUpdateScreeningStatus(consol))
						{
							var c = consol as IShouldUpdateScreeningStatus;
							if (c != null)
							{
								c.ShouldUpdateScreeningStatus = true;
							}
						}
					}

					if (CoLoadMasterShipment is ForwardingShipment coLoadMasterShipment && coLoadMasterShipment.IsMasterShipmentRepresentingAllChildShipments)
					{
						if (value == ScreeningStatusesList.Codes.Matched)
						{
							coLoadMasterShipment.JS_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
						}
						else if (value == ScreeningStatusesList.Codes.Unknown && (coLoadMasterShipment.JS_ScreeningStatus == ScreeningStatusesList.Codes.Clear || coLoadMasterShipment.JS_ScreeningStatus == ScreeningStatusesList.Codes.JobCleared))
						{
							coLoadMasterShipment.JS_ScreeningStatus = ScreeningStatusesList.Codes.Unknown;
						}

						if (CoLoadMasterShipmentShouldUpdateScreeningStatus(coLoadMasterShipment))
						{
							((IShouldUpdateScreeningStatus)coLoadMasterShipment).ShouldUpdateScreeningStatus = true;
						}
					}
				}
			}
		}

		#endregion

		#region JS_PaymentTermAutoratingOverride

		[List("Lookups.JS_PaymentTermAutoratingOverride_List")]
		public override ZString JS_PaymentTermAutoratingOverride
		{
			get => base.JS_PaymentTermAutoratingOverride;
			set => base.JS_PaymentTermAutoratingOverride = value;
		}

		#endregion

		#region JS_Phase

		[List("Lookups.Phases")]
		[WorkflowSetFieldReadonlyCheckBypass]
		public override ZString JS_Phase
		{
			get { return base.JS_Phase; }
			set
			{
				if (base.JS_Phase != value)
				{
					base.JS_Phase = value;

					if (!IsSettingDefaultOrImportingData)
					{
						InitialisePhaseDependantMandatoryValidation();
						InitialisePhaseDependantCustomBusinessObjectValidation();
						MarkAsNeedingValidation();

						DocsAndCartage.InitialisePhaseDependantMandatoryValidation();
						DocsAndCartage.MarkAsNeedingValidation();
					}
				}
			}
		}

		protected bool JS_Phase_ReadOnly
		{
			get { return !Env.Security.ShipmentPhaseSecurityOverride.IsAllowed; }
		}

		void InitialisePhaseDependantMandatoryValidation()
		{
			if (mandatoryValidationPhase != JS_Phase)
			{
				mandatoryValidationPhase = JS_Phase;
				var selectedOnes = PhaseResolver.InitialisePhaseDependantMandatoryValidation(ZPropertyInfoHash.Cast<ZPropertyInfo>())
					.OfType<ZWrappedPropertyInfo>()
					.Select(p => p.InnerInfo.BizObj);
				MarkAsNeedingValidation(selectedOnes);
			}
		}
		ZString mandatoryValidationPhase = ZString.Empty;

		void MarkAsNeedingValidation(IEnumerable<BusinessObject> selectedOnes)
		{
#if NETFRAMEWORK
			selectedOnes.DistinctBy(bo => bo.PK).ForEach(bo => bo.MarkAsNeedingValidation());
#elif NET
			Enumerable.DistinctBy(selectedOnes, bo => bo.PK).ForEach(bo => bo.MarkAsNeedingValidation());
#else
#error Unexpected target platform
#endif
		}

		void InitialisePhaseDependantCustomBusinessObjectValidation(bool forceReinitialise = false)
		{
			if (customBusinessObjectMandatoryValidationPhase != JS_Phase || forceReinitialise)
			{
				customBusinessObjectMandatoryValidationPhase = JS_Phase;
				var selectedOnes = PhaseResolver.InitialisePhaseDependantMandatoryValidation(CustomBusinessObject.ZPropertyInfoHash.Cast<ZPropertyInfo>())
					.OfType<ZWrappedPropertyInfo>()
					.Select(p => p.InnerInfo.BizObj);
				MarkAsNeedingValidation(selectedOnes);
			}
		}
		ZString customBusinessObjectMandatoryValidationPhase = ZString.Empty;

		#endregion

		#region JS_ShipmentStatus

		[List("Lookups.JS_ShipmentStatus_List")]
		public override ZString JS_ShipmentStatus
		{
			get => base.JS_ShipmentStatus;
			set
			{
				var originalStatus = base.JS_ShipmentStatus;
				if (originalStatus != value)
				{
					if (value == ShipmentStatusList.Codes.Confirmed && !IsSettingDefaultValues)
					{
						if (JS_ShipmentStatus == ShipmentStatusList.Codes.ElectronicShippingInstruction
							&& IsElectronicShippingInstructionReceived
							&& isShipmentStatusUpdateActionsEnabled
							&& OnShipmentBookingStatusUpdate != null)
						{
							var eventArgs = new ShipmentBookingStatusEventArgs(JS_ShipmentStatus, value);
							OnShipmentBookingStatusUpdate(this, eventArgs);
							return;
						}
						else
						{
							GenerateStatusEvent(value, (NoResString)"Shipping Instruction Confirmed"); // Event Parameter Constant.
						}
					}
					else if (JS_ShipmentStatus == ShipmentStatusList.Codes.Amendment && value == ShipmentStatusList.Codes.Booked)
					{
						GenerateStatusEvent(value, (NoResString)"Booking Amendment Processed"); // Event Parameter Constant.
					}
					else if (value == ShipmentStatusList.Codes.SIRejected)
					{
						if (isShipmentStatusUpdateActionsEnabled)
						{
							var reason = ZString.Empty;
							if (OnShipmentBookingStatusUpdate != null)
							{
								var eventArgs = new ShipmentBookingStatusEventArgs(JS_ShipmentStatus, value);
								OnShipmentBookingStatusUpdate(this, eventArgs);
								reason = eventArgs.StatusUpdatedReason;

								if (reason.IsEmpty)
								{
									return;
								}
							}
							GenerateStatusEvent(value, reason);
						}
					}

					base.JS_ShipmentStatus = value;

					JS_ShipmentStatusChangeLog = ZString.Format((NoResString)"Original Status: {0}, Current Status: {1}\r\n{2}", originalStatus, JS_ShipmentStatus, System.Environment.StackTrace); // error report
					if (originalStatus == ShipmentStatusList.Codes.Booked)
					{
						ReportShipmentStatusIsEmptyOnBooking();
					}
				}
			}
		}

		ZString JS_ShipmentStatusChangeLog;

		public void GenerateStatusEvent(string newValue, string reason)
		{
			var parameters = new KeyValuePair<string, string>[]
			{
						new KeyValuePair<string, string>(Params.New, newValue),
						new KeyValuePair<string, string>(Params.Old, JS_ShipmentStatus),
						new KeyValuePair<string, string>(Params.Reason, GetReasonWithPrefix(newValue, reason)), // Event Parameter Constant.
						new KeyValuePair<string, string>(Params.Type, (NoResString)"Shipment Status") // Event Parameter Constant.
			};
			Logs.CreateOrRecreateEventLog(Events.StatusUpdated, EstimateActual.Actual, ZDateTimeOffset.Now, ZString.Empty, parameters);
		}

		ZString GetReasonWithPrefix(ZString newValue, ZString reason)
		{
			if (newValue == ShipmentStatusList.Codes.SIRejected && isShipmentStatusUpdateActionsEnabled)
			{
				return reason.IsEmpty ? (NoResString)"Shipping Instruction Rejected" : (NoResString)"Shipping Instruction Rejected, " + reason; // Event Parameter Constant.
			}

			return reason;
		}

		public IDisposable SuspendShipmentStatusUpdateActions()
		{
			isShipmentStatusUpdateActionsEnabled = false;
			return new DisposableAction(() => isShipmentStatusUpdateActionsEnabled = true);
		}

		bool isShipmentStatusUpdateActionsEnabled = true;

		public event EventHandler<ShipmentBookingStatusEventArgs> OnShipmentBookingStatusUpdate;

		public bool JS_ShipmentStatus_ReadOnly
		{
			get => (JS_ShipmentStatus == ShipmentStatusList.Codes.WebBooking && IsInDatabase && !JS_ShipmentStatusInfo.HasChanges)
				|| JS_ShipmentStatus == ShipmentStatusList.Codes.Confirmed
				|| JS_ShipmentStatus == ShipmentStatusList.Codes.SIRejected
				|| JS_ShipmentStatus == ShipmentStatusList.Codes.Booked;
		}

		#endregion

		public event EventHandler<JobDeclarationCreationEventArgs> OnJobDeclarationCreated;

		[SuppressMessage("Microsoft.Design", "CA1030:UseEventsWhereAppropriate")]
		public void RaiseJobDeclarationCreated(IBaseJobDeclaration declaration)
		{
			OnJobDeclarationCreated?.Invoke(this, new JobDeclarationCreationEventArgs(declaration));
		}

		#region JS_IsCancelled

		public override ZBool JS_IsCancelled
		{
			get { return base.JS_IsCancelled; }
			set
			{
				if (base.JS_IsCancelled != value)
				{
					base.JS_IsCancelled = value;
					LoadDeclarations(Factory, activeOnly: value)
						.ForEach(declaration => declaration.IsCancelled = value);

					RelatedCancellableDataSupporter.SetIsCancelled(this, value);
				}
			}
		}

		IRelatedCancellableDataSupporter RelatedCancellableDataSupporter
		{
			get
			{
				if (relatedCancellableDataSupporter == null)
				{
					relatedCancellableDataSupporter = ObjectFactory.Get<IShipmentRelatedCancellableDataSupporter>();
				}

				return relatedCancellableDataSupporter;
			}
		}
		IRelatedCancellableDataSupporter relatedCancellableDataSupporter;

		#endregion

		public override ZDateTime JS_E_DEP
		{
			get { return base.JS_E_DEP; }
			set
			{
				if (base.JS_E_DEP != value)
				{
					base.JS_E_DEP = value;

					if (AviationSecurity.SupplyChainSecurityConfiguration.IsEnabled && AviationSecurity.SupplyChainSecurityConfiguration.IsRecalculationNeeded(this.JS_InspectionTypeCode))
					{
						SetApprovedShipperStatus(Res.GetString("ef1704e0-f55a-467e-acb7-d67861d926e3", "{0} has been changed", JS_E_DEPInfo.HumanReadableName));
					}

					if (!IsValidationSuspended)
					{
						Validation.ValidateJS_InspectionTypeCode();
					}
				}
			}
		}

		public override ZGuid JS_JSB_SupplierBooking
		{
			get => base.JS_JSB_SupplierBooking;
			set
			{
				if (JS_JSB_SupplierBooking != value)
				{
					base.JS_JSB_SupplierBooking = value;
					DocsAndCartage.MarkAsNeedingValidation();
				}
			}
		}

		public override ZGuid JS_CLH_ContainerLoadPlan
		{
			get => base.JS_CLH_ContainerLoadPlan;
			set
			{
				if (JS_CLH_ContainerLoadPlan != value)
				{
					base.JS_CLH_ContainerLoadPlan = value;
					DocsAndCartage.MarkAsNeedingValidation();
				}
			}
		}

		public override ZDateTime JS_SystemCreateTimeUtc
		{
			get { return base.JS_SystemCreateTimeUtc; }
			set
			{
				if (base.JS_SystemCreateTimeUtc != value)
				{
					base.JS_SystemCreateTimeUtc = value;

					if (AviationSecurity.SupplyChainSecurityConfiguration.IsEnabled && AviationSecurity.SupplyChainSecurityConfiguration.IsRecalculationNeeded(this.JS_InspectionTypeCode))
					{
						SetApprovedShipperStatus(Res.GetString("4aaac679-c118-4058-a44b-aa2191de841f", "{0} has been changed", JS_SystemCreateTimeUtcInfo.HumanReadableName));
					}

					DocsAndCartage.MarkAsNeedingValidation();
				}
			}
		}

		protected override AutologState AutoLoggingState =>
			(!IsTemplate && base.AutoLoggingState == AutologState.AutoLogged)
				? EnterpriseBusinessObject.AutologState.AutoLogged
				: EnterpriseBusinessObject.AutologState.NotLogged;

		#endregion

		#region Iceland Specific

		public void SetPreviousConsignorConsigneeAsNotLoaded()
		{
			fPreviousConsignorLoaded = false;
			fPreviousConsigneeLoaded = false;
		}

		public OrgHeader Warehouse
		{
			get
			{
				var address = DocAddresses.FindByDocAddressType(DocAddressType.Warehouse);
				return address?.Organisation;
			}
		}

		public OrgHeader PreviousConsignor
		{
			get
			{
				LoadPreviousConsignor();
				return fPreviousConsignor;
			}
		}
		OrgHeader fPreviousConsignor;

		public ZString PreviousConsignorName
		{
			get
			{
				LoadPreviousConsignor();
				return fPreviousConsignorName;
			}
		}
		ZString fPreviousConsignorName;

		bool fPreviousConsignorLoaded;

		public OrgHeader PreviousConsignee
		{
			get
			{
				LoadPreviousConsignee();
				return fPreviousConsignee;
			}
		}
		OrgHeader fPreviousConsignee;

		public ZString PreviousConsigneeName
		{
			get
			{
				LoadPreviousConsignee();
				return fPreviousConsigneeName;
			}
		}
		ZString fPreviousConsigneeName;

		bool fPreviousConsigneeLoaded;

		void LoadPreviousConsignor()
		{
			if (CountrySpecificSupport is IcelandForwardingShipmentSupport)
			{
				((IcelandForwardingShipmentSupport)CountrySpecificSupport).LoadPreviousDocAddressValues((NoResString)"Consignor", ref fPreviousConsignorLoaded, ref fPreviousConsignor, ref fPreviousConsignorName); // Constant
			}
		}

		void LoadPreviousConsignee()
		{
			if (CountrySpecificSupport is IcelandForwardingShipmentSupport)
			{
				((IcelandForwardingShipmentSupport)CountrySpecificSupport).LoadPreviousDocAddressValues((NoResString)"Consignee", ref fPreviousConsigneeLoaded, ref fPreviousConsignee, ref fPreviousConsigneeName); // Constant
			}
		}

		public ZString PreviousCOC
		{
			get
			{
				LoadPreviousCOC();
				return fPreviousCOC;
			}
		}
		ZString fPreviousCOC;

		void LoadPreviousCOC()
		{
			if (CountrySpecificSupport is IcelandForwardingShipmentSupport)
			{
				fPreviousCOC = ((IcelandForwardingShipmentSupport)CountrySpecificSupport).GetPreviousCOC(this);
			}
		}

		#endregion

		#region Calculated Properties

		#region JS_Calc_EstimatedExportClearanceDate

		[UniversalCopyExtraProperty]
		public ZDateTime JS_Calc_EstimatedExportClearanceDate
		{
			get
			{
				if (!estimatedExportClearanceDate.HasValue)
				{
					lastUsedECCEvent = Logs.MostRecentLogByEventTime(Events.ExportCustomsCleared, new ZQuery(StmALogSchema.SL_IsEstimate, true));
					estimatedExportClearanceDate = lastUsedECCEvent != null ? new ZDateTime(lastUsedECCEvent.SL_EventTime) : ZDateTime.Empty;
					HookEventHandlersForECCEvents();
				}

				return estimatedExportClearanceDate.Value;
			}
			set
			{
				if (JS_Calc_EstimatedExportClearanceDate != value)
				{
					SetNonPersistentPropertyValue(JS_Calc_EstimatedExportClearanceDateInfo, ref estimatedExportClearanceDate, value);

					if (!IsValidationSuspended)
					{
						Validation.ValidateJS_Calc_EstimatedExportClearanceDate();
					}

					if (JS_Calc_EstimatedExportClearanceDateInfo.HasErrors())
					{
						HasChanges = true;
					}
					else if (value.IsEmpty || value.IsValid)
					{
						UnhookEventHandlersForECCEvents();
						CancelAllCurrentEstimatedECCEvents();

						if (!value.IsEmpty)
						{
							lastUsedECCEvent = Logs.AddNew(Events.ExportCustomsCleared, value.ToOffset(), true);
							HookEventHandlersForECCEvents();
						}
					}
				}
			}
		}

		public ZPropertyInfo JS_Calc_EstimatedExportClearanceDateInfo
		{
			get { return GetZPropertyInfo(nameof(JS_Calc_EstimatedExportClearanceDate)); }
		}

		ZDateTime? estimatedExportClearanceDate;
		StmALog lastUsedECCEvent;

		void HookEventHandlersForECCEvents()
		{
			if (lastUsedECCEvent != null)
			{
				lastUsedECCEvent.SL_IsCancelledInfo.ValueChanged += ECCEventFromLogsIsCancelledInfo_ValueChanged;
			}
		}

		void UnhookEventHandlersForECCEvents()
		{
			if (lastUsedECCEvent != null)
			{
				lastUsedECCEvent.SL_IsCancelledInfo.ValueChanged -= ECCEventFromLogsIsCancelledInfo_ValueChanged;
			}
		}

		void ECCEventFromLogsIsCancelledInfo_ValueChanged(object sender, EventArgs e)
		{
			if (!IsValidationSuspended)
			{
				Validation.ValidateJS_Calc_EstimatedExportClearanceDate();
			}
		}

		void CancelAllCurrentEstimatedECCEvents()
		{
			foreach (StmALog eccEvent in Logs.GetAllLogs())
			{
				if (eccEvent.SL_SE_NKEvent == Events.ExportCustomsClearedCode && eccEvent.SL_IsEstimate && !eccEvent.IsCancelled)
				{
					eccEvent.IsCancelled = true;
				}
			}
		}

		#endregion

		#region JS_Calc_PossibleOversize

		public ZString JS_Calc_PossibleOversize
		{
			get
			{
				return FindPossibleOversizePacklineDisplayMessage();
			}
		}

		public ZPropertyInfo JS_Calc_PossibleOversizeInfo => GetZPropertyInfo(nameof(JS_Calc_PossibleOversize));

		public ZString FindPossibleOversizePacklineDisplayMessage(ForwardingConsol forwardingConsol = null)
		{
			if (!IsAir && !IsSea)
			{
				return ZString.Empty;
			}

			switch (JS_PackingMode)
			{
				case Constants.ContainerModes.BreakBulk:
				case Constants.ContainerModes.Bulk:
				case Constants.ContainerModes.Liquid:
				case Constants.ContainerModes.RollOnRollOff:
					return ZString.Empty;
			}

			switch (JS_TransportMode)
			{
				case Constants.TransportModes.Air:
					var consol = forwardingConsol ?? Consols.Cast<ForwardingConsol>().FirstOrDefault(c => !c.ShipmentContainsAllowableCargoDimensions(this, false));
					if (consol != null)
					{
						return (consol.JK_MaximumAllowablePackageUnit == ZString.Empty && consol.JK_MaximumAllowablePackageLength == 0m
							&& consol.JK_MaximumAllowablePackageWidth == 0m && consol.JK_MaximumAllowablePackageHeight == 0m) ?
							FindPossibleOversizePacklineDisplayMessage(300m, 200m, 160m, Constants.Length.Centimetres) :
							FindPossibleOversizePacklineDisplayMessage(consol.JK_MaximumAllowablePackageLength, consol.JK_MaximumAllowablePackageWidth, consol.JK_MaximumAllowablePackageHeight, consol.JK_MaximumAllowablePackageUnit);
					}
					else if (!Consols.Any() || Consols.Cast<ForwardingConsol>().Any(c => CargoDimensionsHelpers.ConsolCargoDimensionsAreUnbounded(c)))
					{
						return FindPossibleOversizePacklineDisplayMessage(300m, 200m, 160m, Constants.Length.Centimetres);
					}

					return ZString.Empty;

				case Constants.TransportModes.Sea:
					return FindPossibleOversizePacklineDisplayMessage(240m, 96m, 102m, Constants.Length.Inches, (NoResString)"dimensions of 20GP container"); // Dimensions description to be used in Res.GetString

				default:
					return ZString.Empty;
			}
		}

		ZString FindPossibleOversizePacklineDisplayMessage(ZDecimal maxLength, ZDecimal maxWidth, ZDecimal maxHeight, ZString maxDimensionUnits, string maxDimensionsDisplayedAsString = null)
		{
			ForwardingPackLine possibleOversizePackLine = null;
			var mostOversizeDimension = 0m;

			foreach (ForwardingPackLine packLine in OuterPackLines)
			{
				var packLineLength = Constants.Length.ConvertSafe(packLine.JL_Length, packLine.JL_UnitOfDimension, maxDimensionUnits);
				var packLineWidth = Constants.Length.ConvertSafe(packLine.JL_Width, packLine.JL_UnitOfDimension, maxDimensionUnits);
				var packLineHeight = Constants.Length.ConvertSafe(packLine.JL_Height, packLine.JL_UnitOfDimension, maxDimensionUnits);

				var currentMostOversizeDimension = 0m;

				var packLineLengthAndWidthFits = packLineLength <= maxLength && packLineWidth <= maxWidth;
				var packLineLengthAndWidthFitsRotated = packLineWidth <= maxLength && packLineLength <= maxWidth;

				if (!packLineLengthAndWidthFits && !packLineLengthAndWidthFitsRotated)
				{
					var oversizeDimension = Math.Max(packLineLength - maxLength, packLineWidth - maxWidth);
					var oversizeDimensionRotated = Math.Max(packLineLength - maxWidth, packLineWidth - maxLength);
					var fitsBestRotated = oversizeDimensionRotated < oversizeDimension;

					currentMostOversizeDimension = fitsBestRotated ? oversizeDimensionRotated : oversizeDimension;
				}

				var packLineHeightFits = packLineHeight <= maxHeight;
				if (!packLineHeightFits)
				{
					currentMostOversizeDimension = Math.Max(currentMostOversizeDimension, packLineHeight - maxHeight);
				}

				if (currentMostOversizeDimension > 0 && currentMostOversizeDimension > mostOversizeDimension)
				{
					possibleOversizePackLine = packLine;
					mostOversizeDimension = currentMostOversizeDimension;
				}
			}

			if (possibleOversizePackLine == null)
			{
				return ZString.Empty;
			}

			var dimensions = new ZDecimal[]
			{
				Utilities.Round(possibleOversizePackLine.JL_Length, 0),
				Utilities.Round(possibleOversizePackLine.JL_Width, 0),
				Utilities.Round(possibleOversizePackLine.JL_Height, 0)
			};

			var possibleOversizeMessage = string.Join((NoResString)"x", dimensions) // Dimension serialization
				+ " " + possibleOversizePackLine.JL_UnitOfDimension;

			if (maxDimensionsDisplayedAsString == null)
			{
				var maxDimensions = new ZDecimal[]
				{
					Utilities.Round(maxLength, 0),
					Utilities.Round(maxWidth, 0),
					Utilities.Round(maxHeight, 0)
				};

				maxDimensionsDisplayedAsString = string.Join((NoResString)"x", maxDimensions) // Dimension serialization
					+ " " + maxDimensionUnits;
			}

			return Res.GetString("1e58efd0-15a3-4380-b499-1d7bd3f86bef",
					"{0} exceeds {1}", possibleOversizeMessage, maxDimensionsDisplayedAsString);
		}

		#endregion

		#region Density

		public ShipmentDensity Density => density ?? (density = new ShipmentDensity(this));
		ShipmentDensity density;

		#endregion

		#region Commodity Attributes

		public ZBool IsPerishable => OuterPackLines.Cast<PackLine>().Any(x => x.CommodityCode != null && x.CommodityCode.RH_IsPerishable);

		public ZPropertyInfo IsPerishableInfo => GetZPropertyInfo(nameof(IsPerishable));

		public ZBool IsFlammable => OuterPackLines.Cast<PackLine>().Any(x => x.CommodityCode != null && x.CommodityCode.RH_IsFlammable);

		public ZPropertyInfo IsFlammableInfo => GetZPropertyInfo(nameof(IsFlammable));

		public ZBool IsTimber => OuterPackLines.Cast<PackLine>().Any(x => x.CommodityCode != null && x.CommodityCode.RH_IsTimber);

		public ZPropertyInfo IsTimberInfo => GetZPropertyInfo(nameof(IsTimber));

		public ZBool IsContainerVentRequired => OuterPackLines.Cast<PackLine>().Any(x => x.CommodityCode != null && x.CommodityCode.RH_ContainerVentRequired);

		public ZPropertyInfo IsContainerVentRequiredInfo => GetZPropertyInfo(nameof(IsContainerVentRequired));

		#endregion

		#region Commodity Codes

		public ZString CommodityCodes => string.Join(", ", OuterPackLines.Cast<PackLine>().Select(x => x.JL_RH_NKCommodityCode).Cast<ZString>().Where(x => !x.IsEmpty).OrderBy(x => x).Distinct());

		public ZPropertyInfo CommodityCodesInfo => GetZPropertyInfo(nameof(CommodityCodes));

		#endregion

		#region TotalCO2e

		public ZDecimal TotalCO2e => this.GetTotalCO2e();

		public ZString TotalCO2eForBinding => this.GetTotalCO2eForBinding();

		public ZPropertyInfo TotalCO2eForBindingInfo => GetZPropertyInfo(nameof(TotalCO2eForBinding));

		[DecimalPlaces(3)]
		public ZDecimal TotalCO2eForSorting => this.GetTotalCO2e();

		public ZPropertyInfo TotalCO2eForSortingInfo => GetZPropertyInfo(nameof(TotalCO2eForSorting));

		public ZString CO2eStatus => this.GetCO2eStatus();

		#endregion

		protected override void OnTransportsIncludingRelatedCreated()
		{
			foreach (var transport in (this as ICO2eLegBasedSupporter).Legs.Cast<Transport>())
			{
				if (transport.GetCO2eStatus() == CO2eStatusList.Codes.NotCurrent)
				{
					this.UpdateCO2eStatusToNotCurrent(new CO2eStatusChangedReason(freeTextReason: (NoResString)"Transport"), IsCopying);
				}
			}

			RefreshTransportsIncludingRelatedHooks();

			TransportsIncludingRelated.CountChanged += (sender, args) =>
			{
				if (!(args.BizObject is Transport transport))
				{
					return;
				}

				if (!IsDeleted && !IsDeleting && !IsCalculatedAsPartOfPrePostCarriageLeg)
				{
					this.UpdateCO2eStatusToNotCurrent(new CO2eStatusChangedReason(args, (NoResString)"Transport"), IsCopying);
				}

				if (args.ItemRemoved)
				{
					transport.CO2eStatusChangedEvent -= TransportCO2eStatusChanged;
					transport.JW_JXInfo.ValueChanged -= TransportSailingChanged;
				}

				RefreshTransportsIncludingRelatedHooks();
			};
		}

		void RefreshTransportsIncludingRelatedHooks()
		{
			foreach (var transport in TransportsIncludingRelated.Cast<Transport>())
			{
				transport.CO2eStatusChangedEvent -= TransportCO2eStatusChanged;
				transport.JW_JXInfo.ValueChanged -= TransportSailingChanged;
			}
			foreach (var transport in (this as ICO2eLegBasedSupporter).Legs.Cast<Transport>())
			{
				transport.CO2eStatusChangedEvent += TransportCO2eStatusChanged;
				transport.JW_JXInfo.ValueChanged += TransportSailingChanged;
			}
		}

		void TransportCO2eStatusChanged(object sender, EventArgs e)
		{
			if (sender is IJobCO2e jobCO2e && !IsDeleted && !IsDeleting)
			{
				if (jobCO2e.Status == CO2eStatusList.Codes.NotCurrent)
				{
					this.UpdateCO2eStatusToNotCurrent(new CO2eStatusChangedReason(freeTextReason: (NoResString)"Transport"), IsCopying);
				}
			}
		}

		void TransportSailingChanged(object sender, EventArgs e)
		{
			if (sender is Transport t && !t.IsSettingIsLinked && t.JW_IsLinked && !IsDeleted && !IsDeleting)
			{
				this.UpdateCO2eStatusToNotCurrent(new CO2eStatusChangedReason(freeTextReason: (NoResString)"Transport linked flight/sailing"), IsCopying);
			}
		}

		#endregion

		#endregion

		[ReadOnlyMember(nameof(IsCompanyTariffLevelReadonly))]
		[List("Lookups.CompanyTariffLevelOverrideList")]
		public override ZByte JS_CompanyTariffLevelOverride
		{
			get { return base.JS_CompanyTariffLevelOverride; }
			set
			{
				base.JS_CompanyTariffLevelOverride = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateJS_CompanyTariffLevelOverride();
				}
			}
		}

		protected bool IsCompanyTariffLevelReadonly => !DataRegistryRating.Instance.AllowOverrideCompanyTariffLevel.Value;

		[ReadOnlyMember(nameof(IsCompanyTariffLevelReadonly))]
		[List("Lookups.CompanyTariffLevelOverrideList")]
		public ZString CompanyTariffLevelOverrideAsString
		{
			get
			{
				if (JS_CompanyTariffLevelOverride != 0)
				{
					return JS_CompanyTariffLevelOverride.ToString();
				}
				return ZString.Empty;
			}

			set
			{
				ZByte.TryParse(value, out var companyTariffLevelOverride);
				if (JS_CompanyTariffLevelOverride != companyTariffLevelOverride)
				{
					JS_CompanyTariffLevelOverride = companyTariffLevelOverride;
				}
			}
		}

		public ZWrappedPropertyInfo CompanyTariffLevelOverrideAsStringInfo => GetWrappedZPropertyInfo(nameof(JS_CompanyTariffLevelOverride), x => JS_CompanyTariffLevelOverrideInfo);

		#region PackLine synchronisation for Customs

		public IPackLineSynchronise PackLineSynchroniser;

		#endregion

		#region UseTrueWHSLocation

		public ZBool UseTrueWHSLocation
		{
			get { return Enterprise.Registry.Business.Warehouse.WarehouseDataRegistry.Instance.FreightLocationIsTrueWHSLocation.Value; }
		}

		#endregion

		#region GoodsValueCurrency

		public override ZString JS_RX_NKGoodsValueCurr
		{
			get { return base.JS_RX_NKGoodsValueCurr; }
			set
			{
				if (ShouldDefaultInsuranceDetailsFromGoodsDetails)
				{
					JS_RX_NKInsuranceCurrency = value;
				}

				base.JS_RX_NKGoodsValueCurr = value;

				if (AirCargoSynchroniser != null)
				{
					AirCargoSynchroniser.GoodsCurrency = GoodsValueCurr != null ? GoodsValueCurr.RX_Code : ZString.Empty;
				}

				if (GoodsValueCurr != null)
				{
					GoodsValueCurrencyPK = GoodsValueCurr.PK;
				}
			}
		}

		// Needed for FieldSynchroniser in Customs
		[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters")]
		public ZGuid GoodsValueCurrencyPK
		{
			get { return GoodsValueCurr != null ? GoodsValueCurr.PK : ZGuid.Empty; }
			set { GoodsValueCurrencyPKInfo.RefreshBinding(); }
		}

		public ZPropertyInfo GoodsValueCurrencyPKInfo
		{
			get { return GetZPropertyInfo(nameof(GoodsValueCurrencyPK)); }
		}

		#endregion

		#region Delegates

		public delegate ZGuid? DelegateForDefaultImportBroker();

		public DelegateForDefaultImportBroker GetDefaultImportBrokerFromBuyerSupplierRelationshipDelegate { get; set; }

		#endregion

		public ZDateTime FallbackPortOfFirstArrivalETA
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;
				if (ArrivalConsol != null)
				{
					result = ArrivalConsol.JK_DatePortOfFirstArrival;
					if (result.IsEmpty && ArrivalConsol.Transports.ArrivalTransport != null)
					{
						result = ArrivalConsol.Transports.ArrivalTransport.JW_ETA;
					}
				}
				if (result.IsEmpty)
				{
					result = JS_E_ARV;
				}

				return result;
			}
		}

		public ZString FallbackPortOfFirstArrival
		{
			get
			{
				ZString result = ZString.Empty;
				if (ArrivalConsol != null)
				{
					result = ArrivalConsol.JK_RL_NKPortOfFirstArrival;
					if (result.IsEmpty && ArrivalConsol.Transports.ArrivalTransport != null)
					{
						result = ArrivalConsol.Transports.ArrivalTransport.JW_RL_NKDiscPort;
					}
				}
				if (result.IsEmpty)
				{
					result = JS_RL_NKDestination;
				}

				return result;
			}
		}

		#region RelatedWarehouseReceive

		public BusinessObject RelatedWarehouseReceive => WarehouseReceiveHelper.GetRelatedWarehouseReceiveForPivot(Factory, PK, TablePrefix);

		#endregion

		public ZShort CargoReportAcceptedEventSafetyMargin
		{
			get
			{
				ZShort result = 0;
				StmEvent marginEvent = Factory.LoadFromNaturalKey<StmEvent>(StmEventSchema.SE_Code, ((IParentForCargoReporter)this).CargoReportAcceptedEvent.Code);
				if (IsAir)
				{
					result = marginEvent.SE_AirExceptionSafetyMargin;
				}
				else if (IsSea)
				{
					result = marginEvent.SE_SeaExceptionSafetyMargin;
				}
				else if (IsRoad)
				{
					result = marginEvent.SE_RoadExceptionSafetyMargin;
				}
				else if (IsRail)
				{
					result = marginEvent.SE_RailExceptionSafetyMargin;
				}
				return result;
			}
		}

		public ForwardingShipmentProcessTask OverdueCargoReportException
		{
			get
			{
				ForwardingShipmentProcessTask result = null;
				ProcessTask[] cachedMilestones = ForwardingShipmentProcessTask.CargoReportAcceptedMilestones(this);
				if (cachedMilestones.Length > 0)
				{
					foreach (ProcessTask mileStone in cachedMilestones)
					{
						if (IsCargoReportOverdue(mileStone))
						{
							result = ForwardingShipmentProcessTask.UnactionedExceptionForMiletsone(mileStone);
							break;
						}
					}
				}
				return result;
			}
		}

		public bool IsCargoReportOverdue(ProcessTask mileStone)
		{
			return !mileStone.IsClosed && mileStone.P9_ScheduledDate.IsValid && mileStone.TriggerProperties.ScheduledDate.AddHours(CargoReportAcceptedEventSafetyMargin) < ZDateTimeOffset.Now;
		}

		#region Additional Reference Numbers

		protected override void OnNumbersLoaded()
		{
			base.OnNumbersLoaded();
			AddCannotDeleteNumberHandler(FindSZBCusEntryNumber(), ResString.GetMultilingualString("c7560721-6bd6-4a24-ba10-0fc9c510463d", "This SZB number was received from DAKOSY, it could not be deleted."));

			if (JS_IsBooking && !JS_IsForwardRegistered)
			{
				foreach (var number in Numbers.OfType<CusEntryNumber>())
				{
					SetupCarrierContractNumberSynchronization(number);
				}
			}

			Numbers.CountChanged += Numbers_CountChanged;
		}

		protected override bool AdditionalReferenceNumberCannotBeDeleted(CusEntryNumber cusEntryNumber)
		{
			return base.AdditionalReferenceNumberCannotBeDeleted(cusEntryNumber)
				|| IsAdditionalReferenceNumberForSystemOnly(cusEntryNumber);
		}

		bool IsAdditionalReferenceNumberForSystemOnly(CusEntryNumber cusEntryNumber)
		{
			var entryTypesForSystemOnly = new ZString[]
			{
				ShipmentNonCustomsAdditionalReferenceCodesCodeList.Codes.CMR
			};

			return entryTypesForSystemOnly.Contains(cusEntryNumber.CE_EntryType) && cusEntryNumber.CE_EntryIsSystemGenerated;
		}

		void Numbers_CountChanged(object sender, CollectionCountChangedEventArgs args)
		{
			var cusEntryNumber = args?.BizObject as CusEntryNumber;
			if (cusEntryNumber != null
				&& args.ItemAdded
				&& !cusEntryNumber.IsDeleted)
			{
				if (JS_IsBooking && !JS_IsForwardRegistered)
				{
					PopulateCarrierContractNumber(cusEntryNumber);
					SetupCarrierContractNumberSynchronization(cusEntryNumber);
				}

				if (IsAdditionalReferenceNumberForSystemOnly(cusEntryNumber))
				{
					AddCannotDeleteNumberHandler(cusEntryNumber, ResString.GetMultilingualString("DA2A50C6-8E14-4870-9335-758D73193645", "The {0} is system generated and cannot be deleted.", cusEntryNumber.CE_EntryType));
				}
			}
		}

		void SetupCarrierContractNumberSynchronization(CusEntryNumber number)
		{
			AddCONAdditionalReferenceNumberDeletionHandler(number);
			AddCONAdditionalReferenceNumberValueChangeHandlers(number);
		}

		void AddCONAdditionalReferenceNumberValueChangeHandlers(CusEntryNumber number)
		{
			number.CE_EntryNumInfo.ValueChanged += AdditionalReferenceNumber_ValueChanged;
			number.CE_EntryTypeInfo.ValueChanged += AdditionalReferenceNumber_ValueChanged;
		}

		void AdditionalReferenceNumber_ValueChanged(object sender, EventArgs e)
		{
			var additionalReferenceNumber = sender as CusEntryNumber;
			if (additionalReferenceNumber != null)
			{
				PopulateCarrierContractNumber(additionalReferenceNumber);
			}
		}

		void PopulateCarrierContractNumber(CusEntryNumber number)
		{
			if (number.CE_EntryType == AdditionalReferenceNumbersCodes.CON
				&& !number.CE_EntryNum.IsEmpty
				&& JS_CarrierContractNumber.IsEmpty)
			{
				JS_CarrierContractNumber = number.CE_EntryNum;
			}
		}

		void AddCONAdditionalReferenceNumberDeletionHandler(CusEntryNumber number)
		{
			number.CanDeleteHandler += (_, eventArgs) =>
			{
				eventArgs.CanDelete = CanDeleteCusEntryNumber(number);
				eventArgs.ReasonForNotAbleToDelete = ResString
					.GetMultilingualString("217ecf0c-23ec-ebb9-4fb6-050461ad70c0", "Carrier Contract number should be the same as Reference Number with Type of CON.");
			};
		}

		[MaxLength(CusEntryNumber.Schema.CE_EntryNumMaxLength)]
		public ZString ITNumber
		{
			get { return ITCusEntryNumber == null ? ZString.Empty : ITCusEntryNumber.CE_EntryNum; }
		}

		bool CanDeleteCusEntryNumber(CusEntryNumber cusEntryNumber)
		{
			if (cusEntryNumber.CE_EntryType != AdditionalReferenceNumbersCodes.CON
				|| JS_CarrierContractNumber.IsEmpty
				|| JS_CarrierContractNumber.Length > AutoCusEntryNum.Schema.CE_EntryNumMaxLength)
			{
				return true;
			}

			var numberOfCONAdditionalRefNumbers = Numbers
				.OfType<CusEntryNumber>()
				.Count(number => number.CE_EntryType == AdditionalReferenceNumbersCodes.CON);

			return numberOfCONAdditionalRefNumbers > 1;
		}

		public ZPropertyInfo ITNumberInfo
		{
			get { return GetZPropertyInfo(nameof(ITNumber)); }
		}

		public ZDateTime ITDate
		{
			get { return ITCusEntryNumber == null ? ZDateTime.Empty : ITCusEntryNumber.CE_IssueDate; }
		}

		public ZPropertyInfo ITDateInfo
		{
			get { return GetZPropertyInfo(nameof(ITDate)); }
		}

		CusEntryNumber ITCusEntryNumber
		{
			get
			{
				foreach (CusEntryNumber cusEntryNum in Numbers)
				{
					if (cusEntryNum.CE_EntryType == UnitedStatesAdditionalReferenceNumberTypes.Codes.IT &&
						cusEntryNum.CE_RN_NKCountryCode == Core.Constants.CountryCodes.UnitedStates)
					{
						return cusEntryNum;
					}
				}
				return null;
			}
		}

		#region JS_SZB

		public ZString JS_SZB
		{
			get
			{
				var cusEntryNum = FindSZBCusEntryNumber();
				return cusEntryNum == null ? ZString.Empty : cusEntryNum.CE_EntryNum;
			}
			set
			{
				var entryNumber = FindOrCreateSZBCusEntryNumber();
				entryNumber.CE_EntryNum = value;
			}
		}

		public ZString JS_SZBInformation
		{
			get
			{
				var cusEntryNum = FindSZBCusEntryNumber();
				return cusEntryNum == null ? ZString.Empty : cusEntryNum.CE_EntryLineReference;
			}
			set
			{
				var entryNumber = FindOrCreateSZBCusEntryNumber();
				entryNumber.CE_EntryLineReference = value;
			}
		}

		public ZDateTime JS_SZBIssueDate
		{
			get
			{
				var cusEntryNum = FindSZBCusEntryNumber();
				return cusEntryNum == null ? ZDateTime.Empty : cusEntryNum.CE_IssueDate;
			}
			set
			{
				var entryNumber = FindOrCreateSZBCusEntryNumber();
				entryNumber.CE_IssueDate = value;
			}
		}

		CusEntryNumber FindSZBCusEntryNumber()
		{
			return Numbers.OfType<CusEntryNumber>()
				.FirstOrDefault(n => n.CE_EntryType == GermanyAdditionalReferenceNumberTypes.Codes.SZBNumber && n.CE_EntryIsSystemGenerated);
		}

		CusEntryNumber FindOrCreateSZBCusEntryNumber()
		{
			var entryNumber = FindSZBCusEntryNumber();
			if (entryNumber == null)
			{
				entryNumber = Numbers.AddNew();
				entryNumber.CE_ParentID = PK;
				entryNumber.CE_ParentTable = TableName;
				entryNumber.CE_EntryType = GermanyAdditionalReferenceNumberTypes.Codes.SZBNumber;
				entryNumber.CE_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				entryNumber.CE_EntryIsSystemGenerated = true;
			}

			return entryNumber;
		}

		#endregion

		#endregion

		#region Freight Spot Rates ReadOnly

		public bool JS_UnitFreightRate_ReadOnly
		{
			get { return GlbDepartment.CurrentDepartment.IsGatewayDepartment && HasConsolSendingAgentActingAsGatewayInAnyCompany; }
		}

		public bool JS_FreightSpotRateAutoratingMode_ReadOnly
		{
			get { return JS_UnitFreightRate_ReadOnly; }
		}

		public bool JS_RX_NKFrtRateCurrency_ReadOnly
		{
			get { return JS_UnitFreightRate_ReadOnly; }
		}

		public bool JS_FreightCostRate_ReadOnly
		{
			get { return !GlbDepartment.CurrentDepartment.IsGatewayDepartment && HasConsolSendingAgentActingAsGatewayInAnyCompany; }
		}

		public bool JS_FreightCostRateAutoratingMode_ReadOnly
		{
			get { return JS_FreightCostRate_ReadOnly; }
		}

		public bool JS_RX_NKFreightCostRateCurrency_ReadOnly
		{
			get { return JS_FreightCostRate_ReadOnly; }
		}

		public bool JS_GatewayFreightSellRate_ReadOnly
		{
			get { return (!GlbDepartment.CurrentDepartment.IsGatewayDepartment && !JS_UnitFreightRate.IsEmpty && HasConsolSendingAgentActingAsGatewayInAnyCompany) || (Consols.Any() && !HasConsolSendingAgentActingAsGatewayInAnyCompany); }
		}

		public bool JS_FreightGatewaySellRateAutoratingMode_ReadOnly
		{
			get { return JS_GatewayFreightSellRate_ReadOnly; }
		}

		public bool JS_RX_NKGatewayFreightSellRateCurrency_ReadOnly
		{
			get { return JS_GatewayFreightSellRate_ReadOnly; }
		}

		#endregion

		public ZBool IsTemperatureControlled => OuterPackLines.OfType<PackLine>().Any(packLine => packLine.JL_RequiresTemperatureControl);

		bool HasConsolSendingAgentActingAsGatewayInAnyCompany
		{
			get
			{
				if (hasConsolSendingAgentActingAsGatewayInAnyCompany == null)
				{
					hasConsolSendingAgentActingAsGatewayInAnyCompany = new CachedProperty<bool>(Factory, () =>
					{
						return FreightRatingHelper.HasConsolSendingAgentActingAsGatewayInAnyCompany(this);
					});
				}

				return hasConsolSendingAgentActingAsGatewayInAnyCompany.Value;
			}
		}

		CachedProperty<bool> hasConsolSendingAgentActingAsGatewayInAnyCompany;

		internal bool HasConsolSendingAgentActingAsGatewayInAnyCompanyExposed => HasConsolSendingAgentActingAsGatewayInAnyCompany;

		#region JS_Calcs

		public ZString JS_Calc_DGClass
		{
			get => GetPropertyFromFlattenedPackLineUNDGCollection(undg => undg.DI_IMOClass);
		}

		public ZString JS_Calc_DGSubstance
		{
			get => GetPropertyFromFlattenedPackLineUNDGCollection(undg => undg.Substance?.DG_Code ?? ZString.Empty);
		}

		public ZString JS_Calc_DGPSA
		{
			get => GetPropertyFromFlatUNDGsDistinctByUNDGSubstance(undg => undg.PSAGroup);
		}

		public ZString JS_Calc_DIHazardousWasteCode
		{
			get => GetPropertyFromFlattenedPackLineUNDGCollection(undg => undg.DI_HazardousWasteCode, true, false);
		}

		public ZString JS_Calc_DISpecialPermitIssueDate
		{
			get => GetPropertyFromFlattenedPackLineUNDGCollection(undg => undg.DI_SpecialPermitIssueDate.ToString(), true, false);
		}

		public ZString JS_Calc_DISpecialPermitNumber
		{
			get => GetPropertyFromFlattenedPackLineUNDGCollection(undg => undg.DI_SpecialPermitNumber, true, false);
		}

		public ZString JS_Calc_DIIsSalvagePackaging
		{
			get => GetPropertyFromFlattenedPackLineUNDGCollection(undg => undg.DI_IsSalvagePackaging ? "Y" : "N", true, false);
		}

		public ZString JS_Calc_DIIsResidueLastContained
		{
			get => GetPropertyFromFlattenedPackLineUNDGCollection(undg => undg.DI_IsResidueLastContained ? "Y" : "N", true, false);
		}

		ZString GetPropertyFromFlatUNDGsDistinctByUNDGSubstance(Func<UNDGDataItem, ZString> getPropertyFunc)
		{
			var undgs = OuterPackLines.Cast<PackLine>().SelectMany(packline => packline.UNDGs).GroupBy(t => new
			{
				Substance = t.DI_DG,
				Value = getPropertyFunc(t)
			}).Select(t => t.Key.Value);
			return GetValueFromFlatList(undgs, true);
		}

		ZString GetPropertyFromFlattenedPackLineUNDGCollection(Func<UNDGDataItem, ZString> getPropertyFunc, bool useManyInsteadOfMixed = false, bool ignoreEmpty = true)
		{
			var listOfUniqueUNDGProperties = this.OuterPackLines.Cast<PackLine>()
				.SelectMany(packline => packline.UNDGs.Select(undg => getPropertyFunc(undg)))
				.Distinct()
				.Where(text => !string.IsNullOrEmpty(text) || !ignoreEmpty);

			return GetValueFromFlatList(listOfUniqueUNDGProperties, useManyInsteadOfMixed);
		}

		ZString GetValueFromFlatList(IEnumerable<ZString> listOfUniqueUNDGProperties, bool useManyInsteadOfMixed = false)
		{
			switch (listOfUniqueUNDGProperties.Count())
			{
				case 0:
					return ZString.Empty;
				case 1:
					return listOfUniqueUNDGProperties.First();
				default:
					{
						return useManyInsteadOfMixed ?
							Res.GetString("411c595c-1d23-898a-45c7-a3baa94a1f84", "Many") :
							Res.GetString("D1A60B1D-A9D9-4A8A-915C-A30E17B6E72F", "Mixed");
					}
			}
		}

		#endregion

		#region JS_Calc_LastKnownTransitWarehouseStatus

		public ZString JS_Calc_LastKnownTransitWarehouseStatus
		{
			get => string.Join(" | ", GetSortedPackLinesGroupsByTWAddress().Select(g => FormatPackLinesTransitWarehouseStatusGroup(g)));
		}

		ZString FormatPackLinesTransitWarehouseStatusGroup(IGrouping<ZString, PackLine> group)
		{
			var allStatuses = string.Join(", ", group.ToArray().Select(p => p.JL_LastKnownTransitWarehouseStatus.IsEmpty ? UnknownTWStatus : p.JL_LastKnownTransitWarehouseStatus.ToString()).Distinct().OrderByDescending(x => x));

			return group.Key + ": " + allStatuses;
		}

		IEnumerable<IGrouping<ZString, PackLine>> GetSortedPackLinesGroupsByTWAddress()
		{
			var groups = OuterPackLines.Cast<PackLine>().GroupBy(p => p.LastKnownTransitWarehouseAddress?.Header?.OH_Code ?? ZString.Empty);
			var processedOrgPKs = new List<ZString>();

			var emptyGroup = groups.FirstOrDefault(g => g.Key == ZString.Empty);
			if (emptyGroup != null)
			{
				processedOrgPKs.Add(emptyGroup.Key);
				yield return emptyGroup;
			}

			var consignorPickupOrg = ConsignorPickupAddress.Organisation?.OH_Code ?? ZString.Empty;
			if (!consignorPickupOrg.IsEmpty)
			{
				var pickupGroup = groups.FirstOrDefault(g => g.Key == consignorPickupOrg);
				if (pickupGroup != null)
				{
					processedOrgPKs.Add(pickupGroup.Key);
					yield return pickupGroup;
				}
			}

			var sortedConsols = Consols.Cast<ForwardingConsol>().ToArray();
			MovementLegComparer.SortMovementLegsByPorts(sortedConsols);
			foreach (var consol in sortedConsols)
			{
				var packDepotOrg = consol.PackDepotAddress?.Header?.OH_Code ?? ZString.Empty;
				if (!packDepotOrg.IsEmpty && !processedOrgPKs.Contains(packDepotOrg))
				{
					var consolDepartureGroup = groups.FirstOrDefault(g => g.Key == packDepotOrg);
					if (consolDepartureGroup != null)
					{
						processedOrgPKs.Add(consolDepartureGroup.Key);
						yield return consolDepartureGroup;
					}
				}

				var unPackDepotOrg = consol.UnpackDepotAddress?.Header?.OH_Code ?? ZString.Empty;
				if (!unPackDepotOrg.IsEmpty && !processedOrgPKs.Contains(unPackDepotOrg))
				{
					var consolArrivalGroup = groups.FirstOrDefault(g => g.Key == unPackDepotOrg);
					if (consolArrivalGroup != null)
					{
						processedOrgPKs.Add(consolArrivalGroup.Key);
						yield return consolArrivalGroup;
					}
				}
			}

			var consigneeDeliveryOrg = ConsigneeDeliveryAddress.Organisation?.OH_Code ?? ZString.Empty;
			if (!consigneeDeliveryOrg.IsEmpty && !processedOrgPKs.Contains(consigneeDeliveryOrg))
			{
				var deliveryGroup = groups.FirstOrDefault(g => g.Key == consigneeDeliveryOrg);
				if (deliveryGroup != null)
				{
					processedOrgPKs.Add(deliveryGroup.Key);
					yield return deliveryGroup;
				}
			}

			foreach (var group in groups.Where(g => g.Key != ZString.Empty && !processedOrgPKs.Contains(g.Key)))
			{
				yield return group;
			}
		}

		public ZPropertyInfo JS_Calc_LastKnownTransitWarehouseStatusInfo
		{
			get { return GetZPropertyInfo(nameof(JS_Calc_LastKnownTransitWarehouseStatus)); }
		}

		const string UnknownTWStatus = "UNK";

		#endregion

		#region RoutingLegColumns

		Transport FirstSeaTransport
		{
			get
			{
				return TransportsIncludingRelated?
					.Cast<Transport>()
					.Where(t => t.JW_TransportMode == TransportModes.Sea && (!t.JW_ETD.IsEmpty || !t.JW_ATD.IsEmpty))
					.OrderBy(t => !t.JW_ETD.IsEmpty ? t.JW_ETD : t.JW_ATD)
					.FirstOrDefault();
			}
		}

		Transport LastSeaTransport
		{
			get
			{
				return TransportsIncludingRelated?
					.Cast<Transport>()
					.Where(t => t.JW_TransportMode == TransportModes.Sea && (!t.JW_ETA.IsEmpty || !t.JW_ATA.IsEmpty))
					.OrderBy(t => !t.JW_ETA.IsEmpty ? t.JW_ETA : t.JW_ATA)
					.LastOrDefault();
			}
		}

		public ZString FirstSeaLegLoadPortForBinding
		{
			get
			{
				return FirstSeaTransport == null ? ZString.Empty : FirstSeaTransport.JW_RL_NKLoadPort;
			}
		}

		public ZPropertyInfo FirstSeaLegLoadPortForBindingInfo => GetZPropertyInfo(nameof(FirstSeaLegLoadPortForBinding));

		public ZDateTime FirstSeaLegLoadPortETDForBinding
		{
			get
			{
				return FirstSeaTransport == null ? ZDateTime.Empty : FirstSeaTransport.JW_ETD;
			}
		}

		public ZPropertyInfo FirstSeaLegLoadPortETDForBindingInfo => GetZPropertyInfo(nameof(FirstSeaLegLoadPortETDForBinding));

		public ZDateTime FirstSeaLegLoadPortATDForBinding
		{
			get
			{
				return FirstSeaTransport == null ? ZDateTime.Empty : FirstSeaTransport.JW_ATD;
			}
		}

		public ZPropertyInfo FirstSeaLegLoadPortATDForBindingInfo => GetZPropertyInfo(nameof(FirstSeaLegLoadPortATDForBinding));

		public ZString LastSeaLegDischargePortForBinding
		{
			get
			{
				return LastSeaTransport == null ? ZString.Empty : LastSeaTransport.JW_RL_NKDiscPort;
			}
		}

		public ZPropertyInfo LastSeaLegDischargePortForBindingInfo => GetZPropertyInfo(nameof(LastSeaLegDischargePortForBinding));

		public ZDateTime LastSeaLegDischargePortETAForBinding
		{
			get
			{
				return LastSeaTransport == null ? ZDateTime.Empty : LastSeaTransport.JW_ETA;
			}
		}

		public ZPropertyInfo LastSeaLegDischargePortETAForBindingInfo => GetZPropertyInfo(nameof(LastSeaLegDischargePortETAForBinding));

		public ZDateTime LastSeaLegDischargePortATAForBinding
		{
			get
			{
				return LastSeaTransport == null ? ZDateTime.Empty : LastSeaTransport.JW_ATA;
			}
		}

		public ZPropertyInfo LastSeaLegDischargePortATAForBindingInfo => GetZPropertyInfo(nameof(LastSeaLegDischargePortATAForBinding));

		#endregion

		public IWhsWarehouse PickupWarehouse
		{
			get
			{
				if (pickupWarehouse == null)
				{
					var pickupWarehouseQuery = new ZQuery(WhsWarehouseSchema.WW_OA_WarehouseAddress, JS_OA_ExportReceivingDepot);
					pickupWarehouseQuery.AddToFilter(WhsWarehouseSchema.WW_WarehouseType, WarehouseTypes.Codes.Transit);
					pickupWarehouseQuery.AddToFilter(WhsWarehouseSchema.WW_IsActive, true);
					pickupWarehouse = Factory.LoadTop1<IWhsWarehouse>(pickupWarehouseQuery);
				}
				return pickupWarehouse;
			}
		}

		IWhsWarehouse pickupWarehouse;

		public IWhsWarehouse DeliveryWarehouse
		{
			get
			{
				if (deliveryWarehouse == null)
				{
					var deliveryWarehouseQuery = new ZQuery(WhsWarehouseSchema.WW_OA_WarehouseAddress, JS_OA_ImportReleaseDepot);
					deliveryWarehouseQuery.AddToFilter(WhsWarehouseSchema.WW_WarehouseType, WarehouseTypes.Codes.Transit);
					deliveryWarehouseQuery.AddToFilter(WhsWarehouseSchema.WW_IsActive, true);
					deliveryWarehouse = Factory.LoadTop1<IWhsWarehouse>(deliveryWarehouseQuery);
				}
				return deliveryWarehouse;
			}
		}

		IWhsWarehouse deliveryWarehouse;

		public IWhsItemReceiveConsignment[] PickupWarehouseReceiveConsignments
		{
			get
			{
				if (pickupWarehouseReceiveConsignments == null && PickupWarehouse != null)
				{
					var pickupRcnQuery = new ZQuery(WhsItemReceiveConsignmentSchema.WRC_ParentID, PK);
					pickupRcnQuery.AddToFilter(new ZQuery(WhsItemReceiveConsignmentSchema.WRC_WW_IntendedWarehouse, PickupWarehouse.PK));
					pickupWarehouseReceiveConsignments = Factory.Load<IWhsItemReceiveConsignment>(pickupRcnQuery);
				}
				return pickupWarehouseReceiveConsignments ?? Array.Empty<IWhsItemReceiveConsignment>();
			}
		}

		IWhsItemReceiveConsignment[] pickupWarehouseReceiveConsignments;

		public IWhsItemDispatchConsignment[] PickupWarehouseDispatchConsignments
		{
			get
			{
				if (pickupWarehouseDispatchConsignments == null && PickupWarehouse != null)
				{
					var pickupDcnQuery = new ZQuery(WhsItemDispatchConsignmentSchema.WDC_ParentID, PK);
					pickupDcnQuery.AddToFilter(new ZQuery(WhsItemDispatchConsignmentSchema.WDC_WW_Warehouse, PickupWarehouse.PK));
					pickupWarehouseDispatchConsignments = Factory.Load<IWhsItemDispatchConsignment>(pickupDcnQuery);
				}
				return pickupWarehouseDispatchConsignments ?? Array.Empty<IWhsItemDispatchConsignment>();
			}
		}

		IWhsItemDispatchConsignment[] pickupWarehouseDispatchConsignments;

		public IWhsItemReceiveConsignment[] DeliveryWarehouseReceiveConsignments
		{
			get
			{
				if (deliveryWarehouseReceiveConsignments == null && DeliveryWarehouse != null)
				{
					var deliveryRcnQuery = new ZQuery(WhsItemReceiveConsignmentSchema.WRC_ParentID, PK);
					deliveryRcnQuery.AddToFilter(new ZQuery(WhsItemReceiveConsignmentSchema.WRC_WW_IntendedWarehouse, DeliveryWarehouse.PK));
					deliveryWarehouseReceiveConsignments = Factory.Load<IWhsItemReceiveConsignment>(deliveryRcnQuery);
				}
				return deliveryWarehouseReceiveConsignments ?? Array.Empty<IWhsItemReceiveConsignment>();
			}
		}

		IWhsItemReceiveConsignment[] deliveryWarehouseReceiveConsignments;

		public IWhsItemDispatchConsignment[] DeliveryWarehouseDispatchConsignments
		{
			get
			{
				if (deliveryWarehouseDispatchConsignments == null && DeliveryWarehouse != null)
				{
					var deliveryDcnQuery = new ZQuery(WhsItemDispatchConsignmentSchema.WDC_ParentID, PK);
					deliveryDcnQuery.AddToFilter(new ZQuery(WhsItemDispatchConsignmentSchema.WDC_WW_Warehouse, DeliveryWarehouse.PK));
					deliveryWarehouseDispatchConsignments = Factory.Load<IWhsItemDispatchConsignment>(deliveryDcnQuery);
				}
				return deliveryWarehouseDispatchConsignments ?? Array.Empty<IWhsItemDispatchConsignment>();
			}
		}

		IWhsItemDispatchConsignment[] deliveryWarehouseDispatchConsignments;

		#region IsLinkedDCNSplitted

		public ZBool IsLinkedDCNSplitted
		{
			get
			{
				return (PrepareDispatchTWInstructionForBlindPackagesDirection == TransitWarehouseInstructionHelper.Direction.Pickup && PickupWarehouseDispatchConsignments.Length > 1)
					|| (PrepareDispatchTWInstructionForBlindPackagesDirection == TransitWarehouseInstructionHelper.Direction.Delivery && DeliveryWarehouseDispatchConsignments.Length > 1)
					|| (PrepareDispatchTWInstructionForBlindPackagesDirection == TransitWarehouseInstructionHelper.Direction.Both && (PickupWarehouseDispatchConsignments.Length > 1 || DeliveryWarehouseDispatchConsignments.Length > 1));
			}
		}

		#endregion

		#region Earliest Last Free Day

		public ZDateTime EarliestLastFreeDayImportDemurrageForBinding
		{
			get
			{
				var earliestLastFreeDayImportLastFreeDay = Containers?
							.Where(container => container.DeliveryPenalties != null)
							.SelectMany(container => container.DeliveryPenalties?
										.Where(penalty => penalty != null
														&& (penalty.CPY_PenaltyType == ContainerPenaltyPenaltyType.Codes.Storage || penalty.CPY_PenaltyType == ContainerPenaltyPenaltyType.Codes.MergedDemurrageAndDetention)
														&& penalty.CPY_CreditorType == Core.Constants.ContainerPenaltyCreditorType.Codes.Carrier))
							.Min(penalty => penalty?.LastFreeDay);

				return earliestLastFreeDayImportLastFreeDay == null ? ZDateTime.Empty : earliestLastFreeDayImportLastFreeDay.Value;
			}
		}

		public ZPropertyInfo EarliestLastFreeDayImportDemurrageForBindingInfo => GetZPropertyInfo(nameof(EarliestLastFreeDayImportDemurrageForBinding));

		public ZDateTime EarliestLastFreeDayImportDetentionForBinding
		{
			get
			{
				var earliestLastFreeDayImportLastFreeDay = Containers?
							.Where(container => container.DeliveryPenalties != null)
							.SelectMany(container => container.DeliveryPenalties?
										.Where(penalty => penalty != null
											&& (penalty.CPY_PenaltyType == ContainerPenaltyPenaltyType.Codes.Detention || penalty.CPY_PenaltyType == ContainerPenaltyPenaltyType.Codes.MergedDemurrageAndDetention)
											&& penalty.CPY_CreditorType == Core.Constants.ContainerPenaltyCreditorType.Codes.Carrier))
							.Min(penalty => penalty?.LastFreeDay);

				return earliestLastFreeDayImportLastFreeDay == null ? ZDateTime.Empty : earliestLastFreeDayImportLastFreeDay.Value;
			}
		}

		public ZPropertyInfo EarliestLastFreeDayImportDetentionForBindingInfo => GetZPropertyInfo(nameof(EarliestLastFreeDayImportDetentionForBinding));

		public ZDateTime EarliestLastFreeDayImportStorageForBinding
		{
			get
			{
				var earliestLastFreeDayImportLastFreeDay = Containers?
							.Where(container => container.DeliveryPenalties != null)
							.SelectMany(container => container.DeliveryPenalties?
										.Where(penalty => penalty != null && penalty.CPY_PenaltyType == ContainerPenaltyPenaltyType.Codes.Storage && penalty.CPY_CreditorType == Core.Constants.ContainerPenaltyCreditorType.Codes.CTO))
							.Min(penalty => penalty?.LastFreeDay);

				return earliestLastFreeDayImportLastFreeDay == null ? ZDateTime.Empty : earliestLastFreeDayImportLastFreeDay.Value;
			}
		}

		public ZPropertyInfo EarliestLastFreeDayImportStorageForBindingInfo => GetZPropertyInfo(nameof(EarliestLastFreeDayImportStorageForBinding));

		public ZDateTime EarliestLastFreeDayExportDemurrageForBinding
		{
			get
			{
				var earliestLastFreeDayExportLastFreeDay = Containers?
							.Where(container => container.PickupPenalties != null)
							.SelectMany(container => container.PickupPenalties?
										.Where(penalty => penalty != null
											&& (penalty.CPY_PenaltyType == ContainerPenaltyPenaltyType.Codes.Storage || penalty.CPY_PenaltyType == ContainerPenaltyPenaltyType.Codes.MergedDemurrageAndDetention)
											&& penalty.CPY_CreditorType == Core.Constants.ContainerPenaltyCreditorType.Codes.Carrier))
							.Min(penalty => penalty?.LastFreeDay);

				return earliestLastFreeDayExportLastFreeDay == null ? ZDateTime.Empty : earliestLastFreeDayExportLastFreeDay.Value;
			}
		}

		public ZPropertyInfo EarliestLastFreeDayExportDemurrageForBindingInfo => GetZPropertyInfo(nameof(EarliestLastFreeDayExportDemurrageForBinding));

		public ZDateTime EarliestLastFreeDayExportDetentionForBinding
		{
			get
			{
				var earliestLastFreeDayExportLastFreeDay = Containers?
							.Where(container => container.PickupPenalties != null)
							.SelectMany(container => container.PickupPenalties?
										.Where(penalty => penalty != null
											&& (penalty.CPY_PenaltyType == ContainerPenaltyPenaltyType.Codes.Detention || penalty.CPY_PenaltyType == ContainerPenaltyPenaltyType.Codes.MergedDemurrageAndDetention)
											&& penalty.CPY_CreditorType == Core.Constants.ContainerPenaltyCreditorType.Codes.Carrier))
							.Min(penalty => penalty?.LastFreeDay);

				return earliestLastFreeDayExportLastFreeDay == null ? ZDateTime.Empty : earliestLastFreeDayExportLastFreeDay.Value;
			}
		}

		public ZPropertyInfo EarliestLastFreeDayExportDetentionForBindingInfo => GetZPropertyInfo(nameof(EarliestLastFreeDayExportDetentionForBinding));

		public ZDateTime EarliestLastFreeDayExportStorageForBinding
		{
			get
			{
				var earliestLastFreeDayExportLastFreeDay = Containers?
							.Where(container => container.PickupPenalties != null)
							.SelectMany(container => container.PickupPenalties?
										.Where(penalty => penalty != null && penalty.CPY_PenaltyType == ContainerPenaltyPenaltyType.Codes.Storage && penalty.CPY_CreditorType == Core.Constants.ContainerPenaltyCreditorType.Codes.CTO))
							.Min(penalty => penalty?.LastFreeDay);

				return earliestLastFreeDayExportLastFreeDay == null ? ZDateTime.Empty : earliestLastFreeDayExportLastFreeDay.Value;
			}
		}

		public ZPropertyInfo EarliestLastFreeDayExportStorageForBindingInfo => GetZPropertyInfo(nameof(EarliestLastFreeDayExportStorageForBinding));

		#endregion

		#region Validation

		protected override void MarkAsNeedingValidationCore()
		{
			base.MarkAsNeedingValidationCore();

			if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Iceland)
			{
				ShipmentCustomsEntryNumber.Validation.ValidateEntryNumber();
			}
		}

		protected override ShipmentDocAddressValidation PiggyBackedDocAddressValidation(JobDocAddress addressToValidate)
		{
			return new ForwardingShipmentDocAddressValidation(addressToValidate);
		}

		internal bool HouseBillCheckDigitValidationCanBeApplied
		{
			get { return (JS_HouseBillInfo.HasChanges || !IsInDatabase) && !SuspendHouseBillValidation; }
		}

		Type ICusEntryNumberValidationDeciderOfType.GetCusEntryNumberValidationType()
		{
			return typeof(ForwardingShipmentAdditionalRefEntryNumValidation);
		}

		void IAdditionalReferenceNumberValidationProvider.ValidateEntryType(ZPropertyInfo entryTypeInfo, ZString category, ZString countryCode)
		{
		}

		bool IAdditionalReferenceNumberValidationProvider.EntryTypeShouldBeUnique(ZString entryType, ZString category, ZString countryCode)
		{
			return entryType != BrazilAdditionalReferenceNumberTypes.Codes.RUC;
		}

		public bool IsAnyPacklineHSCodeEmpty => OuterPackLines.Cast<ForwardingPackLine>().Any(p => p.JL_HarmonisedCode.IsEmpty && p.HarmonisedCodes.HSCountryManager.Value.IsEmpty && p.HarmonisedCodes.HSCodeManager.Value.IsEmpty);

		#endregion

		#region Organisation fields

		[List("Lookups.Broker_List")]
		public override ZGuid JS_OH_ImportBroker
		{
			get { return base.JS_OH_ImportBroker; }
			set
			{
				if (base.JS_OH_ImportBroker != value)
				{
					base.JS_OH_ImportBroker = value;
					if (!IsValidationSuspended)
					{
						Validation.ValidateJS_OH_ExportBroker();
					}
					ScreeningStatusUpdater.SetShouldUpdateScreeningStatusByOrgHeader(this, JS_ScreeningStatus, Factory, JS_OH_ImportBroker);
				}
			}
		}

		[List("Lookups.Broker_List")]
		public override ZGuid JS_OH_ExportBroker
		{
			get { return base.JS_OH_ExportBroker; }
			set
			{
				if (base.JS_OH_ExportBroker != value)
				{
					base.JS_OH_ExportBroker = value;
					if (!IsValidationSuspended)
					{
						Validation.ValidateJS_OH_ImportBroker();
					}
					ScreeningStatusUpdater.SetShouldUpdateScreeningStatusByOrgHeader(this, JS_ScreeningStatus, Factory, JS_OH_ExportBroker);
				}
			}
		}

		[RelatedBusinessObject("DeliveryAgent")]
		[List("Lookups.DeliveryAgents")]
		public override ZGuid JS_OH_DeliveryAgent
		{
			get { return base.JS_OH_DeliveryAgent; }
			set
			{
				if (base.JS_OH_DeliveryAgent != value)
				{
					base.JS_OH_DeliveryAgent = value;
					Job?.SetDefaultsForJob();
					if (!IsValidationSuspended)
					{
						Validation.ValidateJS_OH_DeliveryAgent();
					}
					ScreeningStatusUpdater.SetShouldUpdateScreeningStatusByOrgHeader(this, JS_ScreeningStatus, Factory, JS_OH_DeliveryAgent);
				}
			}
		}

		public ZString ShippingLineOrgCodeFallbackToBookedShippingLine
		{
			get
			{
				return Consols.OfType<ForwardingConsol>().Any(x => !x.ShippingLinePK.IsEmpty)
					? LocalConsol?.ShippingLine?.OH_Code ?? ZString.Empty
					: BookedShippingLine?.OH_Code ?? ZString.Empty;
			}
		}

		public ZPropertyInfo ShippingLineOrgCodeFallbackToBookedShippingLineInfo
		{
			get { return GetZPropertyInfo(nameof(ShippingLineOrgCodeFallbackToBookedShippingLine)); }
		}

		#endregion

		#region DocAddresses

		protected override void SetDocAddressTypeSpecificDefaults(JobDocAddress docAddress)
		{
			if (docAddress.DocAddressType == DocAddressType.ControllingAgent)
			{
				SetControllingAgentAddressDefaults(docAddress);
			}
			else if (docAddress.DocAddressType == DocAddressType.BookingPartyDocumentaryAddress)
			{
				SetBookingPartyDocumentaryAddressDefaults(docAddress);
			}
			else
			{
				base.SetDocAddressTypeSpecificDefaults(docAddress);
			}
		}

		#endregion

		#region BookingPartyDocumentaryAddress

		public OrgHeader BookingParty
		{
			get { return (BookingPartyDocumentaryAddress != null && BookingPartyDocumentaryAddress.HasRealOrganisation) ? BookingPartyDocumentaryAddress.Organisation : null; }
		}

		public JobDocAddress BookingPartyDocumentaryAddress
		{
			get
			{
				if (IsNullOrDeleted(bookingPartyDocumentaryAddress))
				{
					bookingPartyDocumentaryAddress = DocAddresses.FindOrCreateWithRequirement(BookingPartyDocAddressRequirement);
					SetBookingPartyDocumentaryAddressDefaults(bookingPartyDocumentaryAddress);
				}
				return bookingPartyDocumentaryAddress;
			}
		}
		JobDocAddress bookingPartyDocumentaryAddress;

		JobDocAddressRequirement BookingPartyDocAddressRequirement
		{
			get
			{
				return bookingPartyDocAddressRequirement ?? (bookingPartyDocAddressRequirement = GetBookingPartyDocAddressRequirement());
			}
		}
		JobDocAddressRequirement bookingPartyDocAddressRequirement;

		protected virtual JobDocAddressRequirement GetBookingPartyDocAddressRequirement()
		{
			return new JobDocAddressRequirement(DocAddressType.BookingPartyDocumentaryAddress, ContactType.LocalClient);
		}

		#endregion

		#region IDocManagerSupport Members

		protected override DocManagerInfo NewDocManager()
		{
			return new ForwardingShipmentDocManagerInfo(this) { UseBusinessEntityFactoryAsInternal = true };
		}

		#endregion

		#region Customs Integration

		[UniversalCopyRelatedEntity(CommaSeparatedSkipPropertiesNames = JobDeclarationSchema.Constants.JE_JS, CreationMethodName = nameof(CreateNewDeclarationForUniversalCopy), DisableCopyMethodLink = true)]
		public IBaseJobDeclaration JobDeclaration => (IBaseJobDeclaration)GetDeclaration();

		protected void CreateNewDeclarationForUniversalCopy()
		{
			var declaration = Factory.New<IBaseJobDeclaration>();
			declaration.JE_JS = PK;
		}

		public BusinessObject GetDeclaration()
		{
			var result = Factory.LoadTop1<IBaseJobDeclaration>(JobDeclarationFilter);
			if (result == null && IsInDatabase && !ReadOnly && !JobDeclarationFilter.ReLoadExistingRows)
			{
				try
				{
					JobDeclarationFilter.ReLoadExistingRows = true; // if Mutex was released by others and other user saves a created declaration for the shipment, need to bypass the cached query
					result = Factory.LoadTop1<IBaseJobDeclaration>(JobDeclarationFilter);
				}
				finally
				{
					JobDeclarationFilter.ReLoadExistingRows = false;
				}
			}
			return (BusinessObject)result;
		}

		protected ZQuery JobDeclarationFilter
		{
			get
			{
				if (fJobDeclarationFilter == null)
				{
					fJobDeclarationFilter = Enterprise.Customs.Common.JobDeclarationFilter.ForCompanyAndShipment(true, GlbCompany.CurrentCompany, PK);
					fJobDeclarationFilter.OrderBy = JobDeclarationSchema.Constants.JE_SystemCreateTimeUtc;
					fJobDeclarationFilter.FetchOnlyFromLocalCache = !IsInDatabase;
				}
				return fJobDeclarationFilter;
			}
		}
		ZQuery fJobDeclarationFilter;

		protected override ZGuid DefaultBranchPKForLocalCartage
		{
			get { return ShipmentJobHeader != null ? ShipmentJobHeader.JH_GB : ZGuid.Empty; }
		}

		IShipmentCustomsInformation CustomsInformation
		{
			get
			{
				return customsInformation ?? (customsInformation = (IShipmentCustomsInformation)Activator.CreateInstance(ObjectFactory.GetType("ForwardingShipmentCustomsInformation"), new object[] { this }));
			}
		}
		IShipmentCustomsInformation customsInformation;

		public ZString ISFBillStatus
		{
			get { return CustomsInformation.ISFBillStatus; }
		}

		public ZString ISFBillStatusDescription
		{
			get { return CustomsInformation.ISFBillStatusDescription; }
		}

		public ZBool IsPPQForm368Box13Compatible
		{
			get { return GetBooleanFlagFromDeclarationForUSDocuments(Schema.IsPPQForm368Box13Compatible); }
		}

		public ZBool IsPGARecapDocumentToBeShown
		{
			get { return GetBooleanFlagFromDeclarationForUSDocuments(Schema.IsPGARecapDocumentToBeShown); }
		}

		public ZBool IsProofOfReleaseDocumentToBeShown
		{
			get { return GetBooleanFlagFromDeclarationForUSDocuments(Schema.IsProofOfReleaseDocumentToBeShown); }
		}

		ZBool GetBooleanFlagFromDeclarationForUSDocuments(ZString fieldName)
		{
			var result = false;

			var declaration = DeclarationForDocuments;

			if (declaration != null && (declaration.IsDeclarationMatchSpecificCountry(Constants.CountryCodes.UnitedStates) || declaration.IsDeclarationMatchSpecificCountry(Constants.CountryCodes.PuertoRico)))
			{
				return new ZBool(declaration[fieldName]);
			}

			return result;
		}

		public bool RequiresShipmentEntryNumberSelection
		{
			get { return RequiresShipmentEntryNumberSelectionCore; }
		}

		protected virtual bool RequiresShipmentEntryNumberSelectionCore
		{
			get { return true; }
		}

		bool SeaCargoJobExists()
		{
			return SeaCargoJob != null;
		}

		BusinessObject SeaCargoJob
		{
			get
			{
				ZQuery seaCargoQuery = new ZQuery(CusSCAHouseSchema.CA_JS, PK);
				seaCargoQuery.FetchOnlyFromLocalCache = !IsInDatabase;
				return (BusinessObject)Factory.LoadTop1<Shared.IBaseCusSCAHouse>(seaCargoQuery);
			}
		}

		BusinessObject[] SeaCargoJobPivots
		{
			get { return (BusinessObject[])Factory.Load<Shared.IBaseCusSCAPivot>(new ZQuery(CusSCAPivotSchema.CV_CA, SeaCargoJob.PK)); }
		}

		#region BKGNumber

		[MaxLength(CusEntryNumber.Schema.CE_EntryNumMaxLength)]
		public virtual ZString BKGNumber
		{
			get
			{
				var bkgNumber = GetReferenceNumber(CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.BKG, ref oldEntryNumber);
				return bkgNumber;
			}
			set
			{
				if (BKGNumber != value)
				{
					CheckMaximumLength(BKGNumberInfo, value);
					SetReferenceNumber(CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.BKG, value);
				}
				BKGNumberInfo.RefreshBinding();
			}
		}

		public virtual ZPropertyInfo BKGNumberInfo
		{
			get { return GetZPropertyInfo(Schema.BKGNumber); }
		}

		CusEntryNumber OldEntryNumber
		{
			get { return oldEntryNumber; }
			set { oldEntryNumber = value; }
		}

		CusEntryNumber oldEntryNumber;

		ZString GetReferenceNumber(ZString referenceType, ref CusEntryNumber oldnumber)
		{
			var refNum = Numbers.GetFirstReferenceNumberByType(referenceType);
			oldnumber = refNum;
			return refNum != null ? refNum.CE_EntryNum : ZString.Empty;
		}

		void SetReferenceNumber(ZString referenceType, ZString referenceNumber)
		{
			var refNum = Numbers.Find(x => x == OldEntryNumber).FirstOrDefault();

			if (referenceNumber.IsEmpty)
			{
				if (refNum != null)
				{
					Numbers.RemoveAndDelete(refNum);
				}
			}
			else
			{
				if (refNum == null)
				{
					refNum = Numbers.AddNew();
					refNum.CE_EntryType = referenceType;
				}
				refNum.CE_EntryNum = referenceNumber;
			}
		}

		#endregion

		#region Destination Goods Value

		[DecimalPrecision(20)]
		[DecimalPlaces(2)]
		public ZDecimal DestinationGoodsValue => CustomsInformation.DestinationGoodsValue;

		[List(nameof(Lookups) + "." + nameof(ForwardingShipmentLookups.RefCurrency_List))]
		public ZString DestinationCurrencyCode => CustomsInformation.DestinationCurrencyCode;

		[DecimalPrecision(18)]
		[DecimalPlaces(6)]
		public ZDecimal DestinationExchangeRate => CustomsInformation.DestinationExchangeRate;

		public ZPropertyInfo DestinationExchangeRateInfo => GetWrappedZPropertyInfo(nameof(DestinationExchangeRate), x => CustomsInformation.DestinationExchangeRateInfo);

		#endregion

		#endregion

		#region CreateJobHeaderWithMutex

		public void CreateJobHeaderWithMutex()
		{
			CreateShipmentJobHeaderWithMutex();
		}

		#endregion

		#region JobHeader

		public JobHeader JobHeader
		{
			get { return ShipmentJobHeader; }
		}

		#endregion

		#region Hook / UnHook Consignee

		OrgHeader HookedConsignee
		{
			get { return (hookedConsignee == null || hookedConsignee.IsDeleted) ? null : hookedConsignee; }
			set
			{
				if (hookedConsignee != value)
				{
					if (hookedConsignee != null)
					{
						UnHookConsignee(hookedConsignee);
					}

					hookedConsignee = value;

					if (hookedConsignee != null)
					{
						HookConsignee(hookedConsignee);
					}

					SetDeliveryCFS();
				}
			}
		}

		OrgHeader hookedConsignee;

		void HookConsignee(OrgHeader consignee)
		{
			if (consignee.MiscServ != null)
			{
				consignee.MiscServ.OM_IMJobRequireOrderTrackLinkInfo.ValueChanged += new EventHandler(IMJobRequireOrderTrackLinkChanged);
			}
		}

		void UnHookConsignee(OrgHeader consignee)
		{
			if (consignee.MiscServ != null)
			{
				consignee.MiscServ.OM_IMJobRequireOrderTrackLinkInfo.ValueChanged -= new EventHandler(IMJobRequireOrderTrackLinkChanged);
			}
		}

		void IMJobRequireOrderTrackLinkChanged(object sender, EventArgs e)
		{
			DocsAndCartage.MarkAsNeedingValidation();
		}

		#endregion

		#region Hook / UnHook Consignor

		OrgHeader HookedConsignor
		{
			get { return (hookedConsignor == null || hookedConsignor.IsDeleted) ? null : hookedConsignor; }
			set
			{
				if (hookedConsignor != value)
				{
					hookedConsignor = value;
					SetPickupCFS();
				}
			}
		}

		OrgHeader hookedConsignor;

		#endregion

		#region Unique Constraint

		protected override IEnumerable<IUniqueIndexFailureHandler> UniqueIndexFailureHandlers
		{
			get
			{
				var handlers = new List<IUniqueIndexFailureHandler>();
				if (Globals.IsUserInteractive && ((JS_IsBooking && !JS_IsForwardRegistered) || Env.Registry.AllowManualShipmentEntry))
				{
					handlers.Add(new ForwardingShipmentUniqueIndexFailureHandler(this));
				}
				else
				{
					handlers.AddRange(base.UniqueIndexFailureHandlers);
				}

				return handlers;
			}
		}

		class ForwardingShipmentUniqueIndexFailureHandler : IUniqueIndexFailureHandler
		{
			public ForwardingShipmentUniqueIndexFailureHandler(ForwardingShipment shipment)
			{
				this.shipment = shipment;
			}
			readonly ForwardingShipment shipment;

			#region IUniqueIndexFailureHandler Members

			public IEnumerable<string> HandledUniqueIndexNames
			{
				get
				{
					yield return JobShipmentSchema.Constants.Indexes.NR_UC__JS_UniqueConsignRef;
					yield return JobShipmentSchema.Constants.Indexes.FK_UX__JS_TH_OneTimeQuote;
				}
			}

			public void NotifyUserAndAttemptToResolve(INotificationHandler notifier, string indexName)
			{
				if (indexName == JobShipmentSchema.Constants.Indexes.NR_UC__JS_UniqueConsignRef && Env.Registry.AllowManualShipmentEntry)
				{
					if (shipment.CurrentRootConsol != null)
					{
						shipment.Validation.ValidateJS_UniqueConsignRefAfterConstraintFailure();
					}
					else
					{
						notifier.ReportError(Res.GetString("ce66f24b-a7a7-4518-9a87-32f3a9468a22", "The shipment number '{0}' is already in use; please enter a unique shipment number and try again.",
							shipment.JS_UniqueConsignRef), Res.GetString("106e9e8d-14ce-45b9-ab0b-eb9be8d7811d", "Error"));
					}
				}
				else if (indexName == JobShipmentSchema.Constants.Indexes.FK_UX__JS_TH_OneTimeQuote)
				{
					var message = Res.GetString("ce476f82-b127-4414-9d90-6e76ce4790c5", "This quoted booking cannot be saved because the spot quote has also been converted by another user and saved.\r\n\r\nYou must cancel your changes and reopen converted booking with quote.");

					notifier.ReportError(message, Res.GetString("3b645516-a264-4c87-a0a3-b3fc1cd1ffa7", "Information"));
				}
			}

			#endregion
		}

		#endregion

		#region House Bill Number Customisation

		protected override NumberGeneratorTarget NewBillOfLadingNumberGeneratorTargetCore()
		{
			return new ForwardingBillOfLadingNumberGeneratorTarget(this);
		}

		public ZBool ShouldRegenerateHouseBillNumber
		{
			get { return !SuppressBillNumberGeneration && NeedsHouseBill && (ZString)JS_RS_NKServiceLevelInfo.OriginalValue != JS_RS_NKServiceLevel && FreightDataRegistry.Instance.HouseBillNumberRegeneration.Value; }
		}

		protected override bool NeedsHouseBill => base.NeedsHouseBill || ForceHouseBillGeneration;

		public bool ForceHouseBillGeneration { get; set; }

		public void RegenerateHouseBillNumber(bool createChangeOfIdentifierLog = false)
		{
			NumberGeneratorTarget billTarget = NewBillOfLadingNumberGeneratorTarget();

			NumberGenerator generator = new NumberGenerator();
			generator.Factory = Factory;
			generator.Context = NewBillOfLadingGeneratorContext();
			generator.BaseFountain = NumberFountainForUniqueConsignRef;
			generator.FountainGetter = GetGeneratorFountain;
			generator.PrimaryTarget = billTarget;
			generator.ValueProviders.AddRange(new StandardValueSource());
			generator.ValueProviders.AddRange(new FreightValueSource(this));
			generator.TargetBO = this;

			GenerateNumbers(generator);
			SetGeneratedHouseBill(billTarget.Value);

			if (createChangeOfIdentifierLog)
			{
				var parameters = new Dictionary<string, string>
				{
					[EventConstants.EventReferenceParameters.Codes.Old] = JS_HouseBillInfo.OriginalValue.ToString(),
					[EventConstants.EventReferenceParameters.Codes.New] = JS_HouseBillInfo.Value.ToString(),
					[EventConstants.EventReferenceParameters.Codes.Type] = (NoResString)"House Bill",                                // Event Logs references do not allow localized text
					[EventConstants.EventReferenceParameters.Codes.Reason] = (NoResString)"Manually Regenerated House Bill Number"   // Event Logs references do not allow localized text
				};

				regeneratedHouseBillLog = Logs.CreateRecreateOrUpdateEventLog(Events.ChangeOfIdentifier, EstimateActual.Actual, ZDateTimeOffset.Now, "", parameters.ToArray());
			}
		}

		StmALog regeneratedHouseBillLog;

		#endregion

		#region StorageMain

		public ZString StorageLocation
		{
			get
			{
				return StorageMain?.PhysicalLocation ?? ZString.Empty;
			}
		}

		internal IStorageMain StorageMain
		{
			get
			{
				if (storageMain == null)
				{
					storageMain = DocFactory.GetStorageMainForPK(PK);
				}

				return storageMain;
			}
		}

		public ZBool ContainsDocType(ZString docType)
		{
			IStorageDocsBaseCollection eDocs = StorageMain?.Files;
			if (eDocs != null)
			{
				return eDocs.ContainsDocType(docType);
			}
			return false;
		}

		IStorageMain storageMain;

		public ZPropertyInfo StorageLocationInfo
		{
			get { return GetZPropertyInfo(nameof(StorageLocation)); }
		}

		internal IDocumentFactory DocFactory
		{
			get
			{
				if (docFactory == null)
				{
					docFactory = DocManagerInfo.MasterFactory;
				}
				return docFactory;
			}
		}

		IDocumentFactory docFactory;

		#endregion

		#region IWorkflowProvider Members

		ZString IWorkflowProviderCore.WorkflowType
		{
			get { return JobInvoicingConsumerTypes.Shipment.Code; }
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
		public ForwardingShipmentProcessTaskCollection WorkflowItems
		{
			get
			{
				if (workflowItems == null)
				{
					workflowItems = this.GetOrCreateProcessTaskCollection(GetNewWorkflowItems);
					RegisterEditableChildObject(workflowItems);
				}
				return workflowItems;
			}
		}
		ForwardingShipmentProcessTaskCollection workflowItems;

		IEnumerable<IWorkflowProvider> IWorkflowProviderIncludingRelated.RelatedIWorkflowProviders
		{
			get
			{
				var result = new List<IWorkflowProvider>();

				if (!HasHVMParent)
				{
					//ForwardingShipment

					var hawb = AUCusHAWB as IWorkflowProvider;
					if (hawb != null)
					{
						result.Add(hawb);
					}

					result.AddRange(TransportBookingLoader.GetRelatedTransportBookingEvents(this).OfType<IWorkflowProvider>());

					result.AddRange((Factory.Load<ICommonCartage>(new ZQuery(JobCartageSchema.JJ_ParentID, PK))).OfType<IWorkflowProvider>());

					result.AddRange(AttachedWarehouseOrders.OfType<IWorkflowProvider>());

					var receive = RelatedWarehouseReceive as IWorkflowProvider;
					if (receive != null)
					{
						result.Add(receive);
					}

					//CommonShipment

					result.AddRange(Containers.OfType<IWorkflowProvider>());

					result.AddRange(Consols.OfType<IWorkflowProvider>());

					Action<CommonShipment> masterShipmentAdder = null;
					masterShipmentAdder = (s) =>
					{
						var masterShipmentWorkflowProvider = s.CoLoadMasterShipment as IWorkflowProvider;
						if (masterShipmentWorkflowProvider != null)
						{
							result.Add(masterShipmentWorkflowProvider);
						}
						if (s.CoLoadMasterShipment != null)
						{
							masterShipmentAdder(s.CoLoadMasterShipment);
						}
					};

					masterShipmentAdder(this);

					result.AddRange(Declarations.OfType<IWorkflowProvider>());
				}

				return result;
			}
		}

		protected virtual ForwardingShipmentProcessTaskCollection GetNewWorkflowItems()
		{
			return new ForwardingShipmentProcessTaskCollection(this);
		}

		IColumnValueRanker IWorkflowProviderCore.GetTemplateSelectionCriteria()
		{
			var result = new ColumnValueRanker();
			var job = new JobHeader.Loader(this).Load(true, false);

			result.Add(ProcessTaskTemplateSchema.P0_OH_Client, GetClientsInTemplateSelectionOrder());
			result.Add(ProcessTaskTemplateSchema.P0_SubType1, JS_TransportMode, ZString.Empty);
			result.Add(ProcessTaskTemplateSchema.P0_SubType2, DirectionsHelper<ForwardingShipment>.FromBusinessObject(this), ZString.Empty);
			result.Add(ProcessTaskTemplateSchema.P0_GB, job != null && job.JH_GB.IsValid ? job.JH_GB : GlbBranch.CurrentBranch.PK, ZGuid.Empty);
			result.Add(ProcessTaskTemplateSchema.P0_GE, job != null && job.JH_GE.IsValid ? job.JH_GE : GlbDepartment.CurrentDepartment.PK, ZGuid.Empty);
			result.Add(ProcessTaskTemplateSchema.P0_LoadPortCountry, JS_RL_NKOrigin, JS_RL_NKOrigin.Substring(0, 2), ZString.Empty);
			result.Add(ProcessTaskTemplateSchema.P0_DischargePortCountry, JS_RL_NKDestination, JS_RL_NKDestination.Substring(0, 2), ZString.Empty);

			return result;
		}

		IZType[] GetClientsInTemplateSelectionOrder()
		{
			List<IZType> result = new List<IZType>();
			ZString workflowType = ((IWorkflowProvider)this).WorkflowType;

			var collection = WorkflowDataRegistry.Instance.ClientInTemplateSelection.Value;
			var entry = collection.GetValueByCode(workflowType);

			if (entry != null)
			{
				foreach (ClientInTemplateSelectionCriteriaOrgType orgType in entry.SelectedItems)
				{
					switch (orgType.OrgTypeCode)
					{
						case WorkflowSelectionOrgTypeCodes.ConsigneeConsignor:
							if (this.IsImport())
							{
								if (ConsigneePK.IsValid)
								{
									result.Add(ConsigneePK);
								}

								if (ConsignorPK.IsValid)
								{
									result.Add(ConsignorPK);
								}
							}
							else
							{
								if (ConsignorPK.IsValid)
								{
									result.Add(ConsignorPK);
								}

								if (ConsigneePK.IsValid)
								{
									result.Add(ConsigneePK);
								}
							}

							break;

						case WorkflowSelectionOrgTypeCodes.LocalClient:
							JobHeader job = new JobHeader.Loader(this).Load();
							if (job != null)
							{
								result.Add(job.LocalChargesPK);
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
					var companies = Factory.Load<GlbCompany>(new ZQuery(GlbCompanySchema.GC_IsActive, ZBool.True));
					var allOrgs = Consols.Cast<CommonConsol>().SelectMany(consol => new[] { consol.ReceivingForwarder, consol.SendingForwarder }).Where(x => x != null).Distinct();

					var companyPks = allOrgs.SelectMany(org => companies.Where(company => org.IsProxyOrg(company))).Select(x => x.PK).Distinct().ToArray();
					workflowInformationProvider = new WorkflowInformationProvider(companyPks);
				}
				workflowInformationProvider.Destination = (Destination != null) ? Destination.RL_PortName : ZString.Empty;
				workflowInformationProvider.Origin = (Origin != null) ? Origin.RL_PortName : ZString.Empty;
				workflowInformationProvider.BusinessContext = TrackingConstants.BusinessContext.Shipment;

				return workflowInformationProvider;
			}
		}
		WorkflowInformationProvider workflowInformationProvider;

		#endregion

		#region IWorkflowTriggerFieldChangeSource Members

		IReadOnlyList<IWorkflowProvider> IWorkflowTriggerFieldChangeSource.ParentWorkflowProviders
		{
			get
			{
				var result = new List<IWorkflowProvider>();
				ZQuery preAdviceQuery = new ZQuery(JobShipmentPreplanningSchema.EF_JS, PK);
				preAdviceQuery.FetchOnlyFromLocalCache = !IsInDatabase;
				result.AddRange(Factory.Load<JobShipmentPreplanning>(preAdviceQuery));
				result.AddRange(Declarations.ToList().Cast<IWorkflowProvider>());
				return result.ToArray();
			}
		}

		#endregion

		#region IWorkflowTriggerEventSource

		IReadOnlyList<IWorkflowProviderCore> IWorkflowTriggerEventSource.ParentWorkflowProviders
		{
			get
			{
				if (!JS_IsBooking || JS_IsForwardRegistered)
				{
					return Array.Empty<IWorkflowProviderCore>();
				}

				var quotedBookingBuilder = ObjectFactory.Get<IQuotedBookingBuilder>();
				var quotedBooking = (IQuotedBooking)quotedBookingBuilder.Load(Factory, PK);
				var workflowProvider = quotedBooking as IWorkflowProviderCore;

				if (workflowProvider == null)
				{
					return Array.Empty<IWorkflowProviderCore>();
				}

				return new[] { workflowProvider };
			}
		}

		IGlbCompany IWorkflowTriggerEventSource.JobHeaderCompany
		{
			get { return GlbCompany.CurrentCompany; }
		}

		#endregion

		#region IDocumentSupportable Members

		#region Events

		public event EventHandler<ShowMessageOnGUIEventArgs> OnShowMessageOnGUI;

		public void ShowMessageOnGUI(ZString title, ZString message)
		{
			if (OnShowMessageOnGUI != null)
			{
				OnShowMessageOnGUI(this, new ShowMessageOnGUIEventArgs(title, message));
			}
		}

		#endregion

		public override DocumentSupporter DocumentSupporter
		{
			get { return documentSupporter ?? (documentSupporter = ForwardingShipmentDocumentSupporter.New(this)); }
		}
		DocumentSupporter documentSupporter;

		public ZBool UseFormBuilderBillsOfLading => RawDataRegistry.Instance.UseFormBuilderHouseBills.Value
			&& BillOfLadingCodesSupportedInFormBuilder.Any(code => JS_HouseBillOfLadingType == code);

		IEnumerable<string> BillOfLadingCodesSupportedInFormBuilder
		{
			get
			{
				var result = new List<string>()
				{
					Constants.HouseBillOfLadingTypes.Code.TTClubUnitedStates,
					Constants.HouseBillOfLadingTypes.Code.DataHawkBill,
					Constants.HouseBillOfLadingTypes.Code.FIATAHBL,
					Constants.HouseBillOfLadingTypes.Code.FIATAHBLPreprinted,
					Constants.HouseBillOfLadingTypes.Code.TANHBL,
					Constants.HouseBillOfLadingTypes.Code.TANHBLPreprinted,
					Constants.HouseBillOfLadingTypes.Code.TTClubAustraliaNZ,
					Constants.HouseBillOfLadingTypes.Code.TTClubAustraliaNZPreprinted,
					Constants.HouseBillOfLadingTypes.Code.CargowiseBill,
					Constants.HouseBillOfLadingTypes.Code.CargowiseBillPreprinted,
					Constants.HouseBillOfLadingTypes.Code.ITClubAustralia,
					Constants.HouseBillOfLadingTypes.Code.ITClubAustraliaNoTerms,
					Constants.HouseBillOfLadingTypes.Code.TTClubUnitedStatesPreprinted,
					Constants.HouseBillOfLadingTypes.Code.ITClubAustraliaNoTermsNoLaw,
					Constants.HouseBillOfLadingTypes.Code.ITClubNewZealand,
					Constants.HouseBillOfLadingTypes.Code.ITClubAustraliaPreprinted,
					Constants.HouseBillOfLadingTypes.Code.ITClubNewZealandPreprinted
				};

				var additionalCodes = Array.Empty<string>();
				switch (JS_TransportMode)
				{
					case Constants.TransportModes.Sea:
						additionalCodes = FreightDataRegistry.Instance.HouseBillOfLadingTypesForSea.Value.GetCodeDescriptionPairList().GetAllCodes();
						break;
					case Constants.TransportModes.Rail:
						additionalCodes = FreightDataRegistry.Instance.HouseBillOfLadingTypesForRail.Value.GetCodeDescriptionPairList().GetAllCodes();
						break;
					case Constants.TransportModes.Road:
						additionalCodes = FreightDataRegistry.Instance.HouseBillOfLadingTypesForRoad.Value.GetCodeDescriptionPairList().GetAllCodes();
						break;
				}

				result.AddRange(additionalCodes);
				return result.Distinct();
			}
		}

		#region ShowChargesOnBookingConfirmation

		public ZBool ShowChargesOnBookingConfirmation
		{
			get { return Env.Registry.ShowChargesOnForwardingBookingConfirmation; }
		}

		#endregion

		#endregion

		#region ISupportDocumentRecipientSuggestions

		IEnumerable<(MultilingualString organisationType, IOrgHeader orgHeader)> ISupportJobDocumentRecipient.SuggestedOrganisations
		{
			get
			{
				yield return (ResString.GetMultilingualString("0d59cee6-ec43-4a96-911e-36ced6a28044", "Consignee"), Consignee);
				yield return (FreightDataRegistry.Instance.ConsignorShipperTerminology.Value, Consignor);
				yield return (ResString.GetMultilingualString("0ab928b5-33ed-4d51-b13f-fb368f0bced8", "Notify Party"), NotifyParty);
				yield return (ResString.GetMultilingualString("7cd6f3c2-3243-4e05-ac21-a92cea62afb5", "Local Client"), Job?.LocalCharges);
				yield return (ResString.GetMultilingualString("73765901-c32e-4493-8991-47031b70a06c", "Export Broker"), ExportBroker);
				yield return (ResString.GetMultilingualString("7e319bb4-83fc-4de3-a2c0-220e12cb5be9", "Pickup Local Transport Company"), DocsAndCartage?.PickupCartageCo);
				yield return (ResString.GetMultilingualString("2002ed97-bb67-4f1f-9dd6-011a67687ab1", "Pickup Agent"), PickupAgentDocumentaryAddress?.Organisation);
				yield return (ResString.GetMultilingualString("a3004438-627b-4520-8780-18ed213a4262", "Export CFS"), ExportReceivingDepot?.Header);
				yield return (ResString.GetMultilingualString("2abb7e4d-2d76-4eeb-8f94-ae8c6081b9a5", "Import Broker"), ImportBroker);
				yield return (ResString.GetMultilingualString("29503dde-5b93-42f0-be64-a4873651612b", "Delivery Local Transport Company"), DocsAndCartage?.DeliveryCartageCo);
				yield return (ResString.GetMultilingualString("5bb633f9-f877-4e1e-a3c6-2f913d79318a", "Delivery Agent"), DeliveryAgent);
				yield return (ResString.GetMultilingualString("749821b3-f7b0-4e32-8c5e-6125361646c3", "Import CFS"), ImportReleaseDepot?.Header);
			}
		}

		#endregion

		#region ShowOnManifest

		public ZBool ShowOnManifest
		{
			get
			{
				if (CurrentConsolForDocuments == null)
				{
					return true;
				}
				else
				{
					return (CurrentConsolForDocuments.JK_PrintOptionForColoadsOnManifest == FreightConstants.PrintOptionForCoLoads.MastersOnly && CoLoadMasterShipment == null)
						|| (CurrentConsolForDocuments.JK_PrintOptionForColoadsOnManifest == FreightConstants.PrintOptionForCoLoads.SubHouseBillsOnly && CoLoadShipments.Count == 0)
						|| (CurrentConsolForDocuments.JK_PrintOptionForColoadsOnManifest == FreightConstants.PrintOptionForCoLoads.All);
				}
			}
		}

		#endregion

		#region ShowOnOtherDocs

		public ZBool ShowOnOtherDocs
		{
			get
			{
				if (CurrentConsolForDocuments == null)
				{
					return true;
				}
				else
				{
					return (CurrentConsolForDocuments.JK_PrintOptionForColoadsOnOtherDocs == FreightConstants.PrintOptionForCoLoads.MastersOnly && CoLoadMasterShipment == null)
						|| (CurrentConsolForDocuments.JK_PrintOptionForColoadsOnOtherDocs == FreightConstants.PrintOptionForCoLoads.SubHouseBillsOnly && CoLoadShipments.Count == 0)
						|| (CurrentConsolForDocuments.JK_PrintOptionForColoadsOnOtherDocs == FreightConstants.PrintOptionForCoLoads.All);
				}
			}
		}

		#endregion

		#region ShowMultiModeDGDocument

		public ZBool ShowMultiModeDGDocument
		{
			get
			{
				return Containers.Any()
					&& OuterPackLines.OfType<ForwardingPackLine>().Any(p => p.JL_RH_NKCommodityCode == Core.Constants.CargoTypes.Hazardous || p.UNDGs.Count > 0);
			}
		}

		#endregion

		#region IJobHeaderParent Members

		public override void OnJobCreating(JobHeader job)
		{
			base.OnJobCreating(job);

			job.JH_OA_LocalChargesAddrInfo.ValueChanged += JH_OA_LocalChargesAddrInfo_ValueChanged;
			PreviousJH_OH_LocalCharges = job.LocalChargesPK;

			// tested by IDtbBookingParentTestCase<T>.TestIfJobInvoicingPluginAndCanCreateInvoicingJobThenAddsChargesFromChildPortTransportJob() and TestIfJobInvoicingPluginAndCanCreateInvoicingJobThenAddsChargesFromGrandchildPortTransportJob()
			CartageHelper.AttachCartageJobsToParentJob(job, PK);
			// tested by IDtbBookingParentTestCase<T>.TestIfJobInvoicingPluginAndCanCreateInvoicingJobThenAddsChargesFromGrandchildLandTransportConsignment()
			ConsignmentJobHelper.AttachLTConsignmentJobsToParentJob(job, PK);
			TransitWarehouseConsignmentJobHelper.AttachConsignmentJobsToParentJob(job, PK);

			if (JobCreating != null)
			{
				JobCreating(this, EventArgs.Empty);
			}
		}

		public override void OnJobCreated(JobHeader job)
		{
			base.OnJobCreated(job);

			if (JobCreated != null)
			{
				JobCreated(this, EventArgs.Empty);
			}
		}

		public override void OnJobDeleting(JobHeader job)
		{
			job.JH_OA_LocalChargesAddrInfo.ValueChanged -= JH_OA_LocalChargesAddrInfo_ValueChanged;
			base.OnJobDeleting(job);
		}

		public override void OnJobDeleted(JobHeader job)
		{
			base.OnJobDeleted(job);

			if (JobDeleted != null)
			{
				JobDeleted(this, EventArgs.Empty);
			}
		}

		public event EventHandler JobCreating;
		public event EventHandler JobCreated;
		public event EventHandler JobDeleted;
		public event EventHandler CrossTradeChanged;

		ICartageHelper CartageHelper
		{
			get { return cartageHelper ?? (cartageHelper = ObjectFactory.Get<ICartageHelper>()); }
		}
		ICartageHelper cartageHelper;

		IConsignmentJobHelper ConsignmentJobHelper
		{
			get { return consignmentJobHelper ?? (consignmentJobHelper = ObjectFactory.Get<IConsignmentJobHelper>()); }
		}
		IConsignmentJobHelper consignmentJobHelper;

		ITransitWarehouseConsignmentJobHelper TransitWarehouseConsignmentJobHelper
		{
			get { return transitWarehouseConsignmentJobHelper ?? (transitWarehouseConsignmentJobHelper = ObjectFactory.Get<ITransitWarehouseConsignmentJobHelper>()); }
		}
		ITransitWarehouseConsignmentJobHelper transitWarehouseConsignmentJobHelper;

		#endregion

		#region ICustomsChargesAdditionalCharges Members

		ICustomsCharges[] ICustomsChargesAdditionalCharges.AdditionalCharges
		{
			get
			{
				List<ICustomsCharges> charges = new List<ICustomsCharges>();
				foreach (BusinessObject declaration in Declarations)
				{
					ICustomsCharges charge = ServiceLocator.GetService<ICustomsCharges>(declaration);
					if (charge.IsActive)
					{
						charges.Add(charge);
					}
				}

				return charges.ToArray();
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
			get { return new CartageType[] { new ShipmentPickupCartageType(this), new ShipmentDeliveryCartageType(this) }; }
		}

		CartageType ICartageParent.GetLocalCartageType
		{
			get { return this.IsExport() ? new ShipmentPickupCartageType(this) : new ShipmentDeliveryCartageType(this); }
		}

		ZString ICartageParent.UniqueConsignmentID
		{
			get { return JS_UniqueConsignRef; }
		}

		ZGuid ICartageParent.CartageParentID
		{
			get { return PK; }
		}

		ZString ICartageParent.CartageParentTableCode
		{
			get { return JobShipmentSchema.Constants.Prefix; }
		}

		ZGuid ICartageParent.BranchPK
		{
			get { return ZGuid.Empty; }
		}

		ZString ICartageParent.OrderReferenceNumber
		{
			get
			{
				ZString result = ZString.Empty;
				if (attachedOrders != null && attachedOrders.Count > 0)
				{
					result = string.Join(",", attachedOrders.Select(order => order.JD_OrderNumber));
				}

				if (result.IsEmpty)
				{
					result = JS_OrderReferences;
				}

				return result;
			}
		}

		ZString ICartageParent.ServiceLevel
		{
			get { return JS_RS_NKServiceLevel; }
		}

		ControllerID ICartageParent.ControllerID
		{
			get { return ControllerIDs.JobShipment; }
		}

		ZString ICartageParent.WayBillNumber
		{
			get { return JS_HouseBill; }
		}

		ZString ICartageParent.GoodsDescription
		{
			get { return JS_GoodsDescription; }
		}

		ZGuid ICartageParent.JobHeaderPK
		{
			get { return Job != null ? Job.PK : ZGuid.Empty; }
		}

		ZGuid ICartageParent.LocalClientAddressPK
		{
			get { return ShipmentJobHeader != null ? ShipmentJobHeader.JH_OA_LocalChargesAddr : ZGuid.Empty; }
		}

		ZInt ICartageParent.TotalPackages
		{
			get { return JS_OuterPacks; }
		}

		ZString ICartageParent.TotalPackType
		{
			get { return JS_F3_NKPackType; }
		}

		ZDecimal ICartageParent.TotalWeight
		{
			get { return JS_ActualWeight; }
		}

		ZString ICartageParent.TotalWeightUnit
		{
			get { return JS_UnitOfWeight; }
		}

		ZDecimal ICartageParent.TotalVolume
		{
			get { return JS_ActualVolume; }
		}

		ZString ICartageParent.TotalVolumeUnit
		{
			get { return JS_UnitOfVolume; }
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
			get { return this; }
		}

		IDocManagerSupport ICartageParent.BusinessObjectForRelatedEDocs
		{
			get { return this; }
		}

		#endregion

		#region ICartageParentExtra Members

		ZString ICartageParentExtra.CustomAttrib1
		{
			get { return DocsAndCartage.JP_CustomAttrib1; }
		}

		ZString ICartageParentExtra.CustomAttrib2
		{
			get { return DocsAndCartage.JP_CustomAttrib2; }
		}

		ZDateTime ICartageParentExtra.CustomDate1
		{
			get { return DocsAndCartage.JP_CustomDate1; }
		}

		ZDateTime ICartageParentExtra.CustomDate2
		{
			get { return DocsAndCartage.JP_CustomDate2; }
		}

		ZDecimal ICartageParentExtra.CustomDecimal1
		{
			get { return DocsAndCartage.JP_CustomDecimal1; }
		}

		ZDecimal ICartageParentExtra.CustomDecimal2
		{
			get { return DocsAndCartage.JP_CustomDecimal2; }
		}

		ZBool ICartageParentExtra.CustomFlag1
		{
			get { return DocsAndCartage.JP_CustomFlag1; }
		}

		ZBool ICartageParentExtra.CustomFlag2
		{
			get { return DocsAndCartage.JP_CustomFlag2; }
		}

		#endregion

		protected override void ProcessLogCore(IStmALog log)
		{
			base.ProcessLogCore(log);

			if (log.SL_SE_NKEvent == Events.PickupCartageCompleteFinalisedCode)
			{
				if (IsHBLContainerPackModeDOOR_X && DocsAndCartage.JP_PickupCartageCompletedInfo.HasChanges)
				{
					currentDeliveryDueDateChangedFactors |= DeliveryDueDateChangedFactor.ActualPickup;
				}
			}

			if (FreightDataRegistry.Instance.EnableBoleroEHBLIntegration.Value.EnableEBLIntegration && log.SL_SE_NKEvent == Events.BillStatusUpdatedCode)
			{
				if (log.Parameters.TryGetValue(Params.Type, out var eventType)
					&& log.Parameters.TryGetValue(Params.Department, out var department)
					&& department == ElectronicBOLConstants.EHBLEventDepartments.TitleRegistry)
				{
					switch (eventType)
					{
						case BillStatusUpdatedTypes.AmendmentRequested:
						case BillStatusUpdatedTypes.DenyAmendmentAccepted:
						case BillStatusUpdatedTypes.DenyAmendmentRejected:
						case BillStatusUpdatedTypes.SwitchedToPaper:
						case BillStatusUpdatedTypes.OriginalBillTransferred:
						case BillStatusUpdatedTypes.Surrendered:
							JS_ElectronicBillOfLadingStatus = ConvertBillStatusUpdatedTypesToEHBLStatus(eventType);
							break;
						case BillStatusUpdatedTypes.OriginalBillNotPublished:
							JS_ElectronicBillOfLadingStatus = FreightConstants.BillOfLadingBillStatus.Codes.PublishingRejected;
							JS_ElectronicBillOfLadingVersion--;
							break;
						case BillStatusUpdatedTypes.OriginalBillPublished:
							JS_ElectronicBillOfLadingStatus = ConvertBillStatusUpdatedTypesToEHBLStatus(eventType);
							if (log.Parameters.TryGetValue(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.ReferenceNumber, out var referenceNumber))
							{
								if (JS_ElectronicBillOfLadingReference != referenceNumber)
								{
									JS_ElectronicBillOfLadingReference = referenceNumber;
								}
							}
							break;
					}
				}
			}

			if (log.SL_SE_NKEvent == Events.MessageSentCode
				&& FreightDataRegistry.Instance.EnableBoleroEHBLIntegration.Value.EnableEBLIntegration
				&& log.Parameters.TryGetValue(Params.Department, out var msnDepartment) && msnDepartment == ElectronicBOLConstants.EHBLEventDepartments.TitleRegistry
				&& log.Parameters.TryGetValue(Params.Type, out var type)
				&& new string[] { BillStatusUpdatedTypes.OriginalBillSentForPublication, BillStatusUpdatedTypes.AmendmentGranted, BillStatusUpdatedTypes.AmendmentDenied }.Contains(type)
				)
			{
				this.OnSelfPublish(log);
			}
		}

		#region IBuyerSupplierRelationshipConsumer Members

		public BuyerSupplierLinksHelper<ForwardingShipment> BuyerSupplierLinksHelper;

		ZString IBuyerSupplierRelationshipConsumer.Origin
		{
			get { return JS_RL_NKOrigin; }
			set { JS_RL_NKOrigin = value; }
		}

		ZString IBuyerSupplierRelationshipConsumer.Destination
		{
			get { return JS_RL_NKDestination; }
			set { JS_RL_NKDestination = value; }
		}

		ZString IBuyerSupplierRelationshipConsumer.LoadPort
		{
			get { return ""; }
			set { }
		}

		ZString IBuyerSupplierRelationshipConsumer.DischargePort
		{
			get { return ""; }
			set { }
		}

		event EventHandler IBuyerSupplierRelationshipConsumer.ModesChanged
		{
			add
			{
				JS_TransportModeInfo.ValueChanged += value;
				JS_PackingModeInfo.ValueChanged += value;
			}
			remove
			{
				JS_TransportModeInfo.ValueChanged -= value;
				JS_PackingModeInfo.ValueChanged -= value;
			}
		}

		ZString IBuyerSupplierRelationshipConsumer.ContainerMode
		{
			get { return JS_PackingMode; }
			set { JS_PackingMode = value; }
		}

		ZString IBuyerSupplierRelationshipConsumer.TransportMode
		{
			get { return JS_TransportMode; }
			set { JS_TransportMode = value; }
		}

		ZString IBuyerSupplierRelationshipConsumer.ServiceLevel
		{
			get { return JS_RS_NKServiceLevel; }
			set { JS_RS_NKServiceLevel = value; }
		}

		void IBuyerSupplierRelationshipConsumer.RestoreServiceLevelFallback()
		{
			SetServiceLevel();
		}

		ZGuid IBuyerSupplierRelationshipConsumer.PickupCartageCoPK
		{
			get { return IsDeleted ? ZGuid.Empty : DocsAndCartage.PickupCartageCoPK; }
			set
			{
				if (!IsDeleted)
				{
					DocsAndCartage.PickupCartageCoPK = value;
				}
			}
		}

		ZGuid IBuyerSupplierRelationshipConsumer.ShippingLinePK
		{
			get
			{
				ZGuid result = ZGuid.Empty;

				CommonConsol consol = Consols.Count == 1 ? Consols[0] : FindCorrectConsol();
				if (consol != null && consol.ShippingLinePK.IsValid)
				{
					result = consol.ShippingLinePK;
				}

				return result;
			}
			set { }
		}

		ZGuid IBuyerSupplierRelationshipConsumer.DeliveryCartageCoPK
		{
			get { return IsDeleted ? ZGuid.Empty : DocsAndCartage.DeliveryCartageCoPK; }
			set
			{
				if (!IsDeleted)
				{
					DocsAndCartage.DeliveryCartageCoPK = value;
				}
			}
		}

		ZGuid IBuyerSupplierRelationshipConsumer.ImportBrokerPK
		{
			get { return JS_OH_ImportBroker; }
			set
			{
				if (!IsChangingConsigneeAddress && (!IsChangingConsignorAddress || !this.IsInDatabase))
				{
					JS_OH_ImportBroker = value;
				}
			}
		}

		ZGuid IBuyerSupplierRelationshipConsumer.ReceivingAgentPK
		{
			get { return ZGuid.Empty; }
			set { }
		}

		ZGuid IBuyerSupplierRelationshipConsumer.SendingAgentPK
		{
			get { return ZGuid.Empty; }
			set { }
		}

		void IBuyerSupplierRelationshipConsumer.RestoreSendingAgentFallback()
		{
		}

		void IBuyerSupplierRelationshipConsumer.RestoreReceivingAgentFallback()
		{
		}

		ZString IBuyerSupplierRelationshipConsumer.GoodsCurrency
		{
			get { return JS_RX_NKGoodsValueCurr; }
			set { JS_RX_NKGoodsValueCurr = value; }
		}

		void IBuyerSupplierRelationshipConsumer.RestoreGoodsCurrencyFallback()
		{
			SetDefaultGoodsCurrency();
		}

		ZString IBuyerSupplierRelationshipConsumer.GoodsDescription
		{
			get { return JS_GoodsDescription; }
			set { JS_GoodsDescription = value; }
		}

		ZBool IBuyerSupplierRelationshipConsumer.ShouldPromptToSaveBuyerSupplierRelationship
		{
			get { return (!IsCoLoadMaster || !IsBlindCoLoadMaster) && Env.Registry.PromptToSaveBuyerSupplier; }
		}

		ZByte IBuyerSupplierRelationshipConsumer.NoCopyBills
		{
			get { return JS_NoCopyBills; }
			set { JS_NoCopyBills = value; }
		}

		ZByte IBuyerSupplierRelationshipConsumer.NoOriginalBills
		{
			get { return JS_NoOriginalBills; }
			set { JS_NoOriginalBills = value; }
		}

		void IBuyerSupplierRelationshipConsumer.RestoreNumberOfBillsWithFallback()
		{
			DefaultNumberOfBillsWithFallback(this.JS_ReleaseType);
		}

		void IBuyerSupplierRelationshipConsumer.RestoreImportBrokerFallback()
		{
			var defaultImportBrokerPK = GetImportBrokerFallback();
			if (defaultImportBrokerPK.HasValue)
			{
				JS_OH_ImportBroker = defaultImportBrokerPK.Value;
			}
		}

		void IBuyerSupplierRelationshipConsumer.RestoreEFreightStatusFallback(ZString defaultValue)
		{
			DefaultEFreightStatusWithFallBack(defaultValue);
		}

		ZBool IBuyerSupplierRelationshipConsumer.PreventBuyerSupplierRelationships
		{
			get { return false; }
		}

		ZString IBuyerSupplierRelationshipConsumer.PaymentTerms
		{
			get { return JS_INCO; }
			set
			{
				if (Lookups.JS_INCO_List.ContainsCode(value))
				{
					JS_INCO = value;
				}
			}
		}

		ZBool IBuyerSupplierRelationshipConsumer.ShouldRestorePaymentTerm
		{
			get { return true; }
		}

		ZBool IBuyerSupplierRelationshipConsumer.ShouldRestoreHandlingInformation
		{
			get { return true; }
		}

		ZString IBuyerSupplierRelationshipConsumer.ReleaseType
		{
			get { return JS_ReleaseType; }
		}

		OrgHeader IBuyerSupplierRelationshipConsumer.Consignee
		{
			get { return Consignee; }
			set
			{
				if (!ConsigneeDocumentaryAddress.ReadOnly && !ConsigneeDocumentaryAddress.E2_AddressOverride)
				{
					ConsigneeDocumentaryAddress.OrganisationPK = value.PK;
				}
			}
		}

		public event EventHandler ConsigneeChanged
		{
			add { ConsigneeDocumentaryAddressValueChanged += value; }
			remove { ConsigneeDocumentaryAddressValueChanged -= value; }
		}

		OrgHeader IBuyerSupplierRelationshipConsumer.Consignor
		{
			get { return Consignor; }
			set
			{
				if (!ConsignorDocumentaryAddress.ReadOnly && !ConsignorDocumentaryAddress.E2_AddressOverride)
				{
					ConsignorDocumentaryAddress.OrganisationPK = value.PK;
				}
			}
		}

		event EventHandler IBuyerSupplierRelationshipConsumer.ConsignorChanged
		{
			add { ConsignorDocumentaryAddressValueChanged += value; }
			remove { ConsignorDocumentaryAddressValueChanged -= value; }
		}

		ZBool IBuyerSupplierRelationshipConsumer.ShouldRestorePickupDeliveryAndNotifyPartyAddress
		{
			get { return true; }
		}

		ZBool IBuyerSupplierRelationshipConsumer.ShouldDefaultContainerModeAndIsContainerised(ZString containerMode) => false;

		#endregion

		#region Buyer / Supplier / Buyer Links

		#region ShouldPromptToSaveSupplierBuyerRelationship

		void PromptToSaveSupplierBuyerIfOnConsolForm()
		{
			if (CurrentRootConsol != null && (BuyerSupplierLinksHelper?.ShouldPromptToSaveSupplierBuyerRelationship ?? false))
			{
				var importCountryCode = ZString.Empty;

				if (Destination != null && Destination.Country != null)
				{
					importCountryCode = Destination.Country.RN_Code;
				}

				if (!CurrentRootConsol.BuyerSupplierContainsPair(Consignee, Consignor, importCountryCode))
				{
					var result = ShipmentDomainService.GetInstance(Factory).QueryNewSupplierBuyerLink();
					if (result == ZDialogResult.Cancel)
					{
						return;
					}
					if (result != ZDialogResult.Yes)
					{
						CurrentRootConsol.AddToBuyerSupplierLinks(Consignee, Consignor, importCountryCode, null);
						return;
					}
					var filter = new ZQuery();
					filter.AddToFilter(OrgSupplierBuyerLinkSchema.OL_OH_Buyer, Consignee.PK);
					filter.AddToFilter(OrgSupplierBuyerLinkSchema.OL_OH_Supplier, Consignor.PK);
					filter.AddToFilter(OrgSupplierBuyerLinkSchema.OL_RN_NKImporterCountry, importCountryCode);
					if (!IsDeleted && Factory.Load<OrgSupplierBuyerLink>(filter).Length == 0)
					{
						var link = BuyerSupplierLinksHelper.AddNewBuyerSupplierLinkGivenOrgs(Consignee, Consignor, importCountryCode);
						CurrentRootConsol.AddToBuyerSupplierLinks(Consignee, Consignor, importCountryCode, link);
					}
				}
			}
		}

		#endregion

		#region Consignee Or Consignor changed

		protected override void SetConsignorChanges()
		{
			base.SetConsignorChanges();

			if (!fIsImportingData && !IsCopying)
			{
				SetPickupAgent();
			}
		}

		protected override bool ShouldSetGoodsCurrencyOnConsignorChanges
		{
			get { return false; }
		}

		protected override void SetImportCartage()
		{
			if (!JS_IsBooking || JS_IsForwardRegistered)
			{
				base.SetImportCartage();
			}
		}

		protected override void SetConsigneeChanges()
		{
			base.SetConsigneeChanges();

			if (!fIsImportingData && !IsCopying)
			{
				SetDeliveryAgent();
			}
		}

		protected override void SetConsignorConsigneeCommonChanges()
		{
			base.SetConsignorConsigneeCommonChanges();

			if (!fIsImportingData && !IsCopying)
			{
				PromptToSaveSupplierBuyerIfOnConsolForm();
				SetPickupAddress();
				SetDeliveryAddress();
				SetDefaultNotifyParty();
			}
		}

		protected override bool ShouldSetServiceLevelOnConsignorOrConsigneeChange
		{
			get { return false; }
		}

		protected override void ConsignorPickupAddressChanged()
		{
			AviationSecurityParty_HasChanges(SupplyChainSecurityOrganisationTypes.ShipmentPickupFrom);
			SetPickupCFS();
			if (!ConsignorPickupAddress.E2_AddressOverride
				|| ConsignorPickupAddress.E2_PostcodeInfo.HasChanges
				|| ConsignorPickupAddress.E2_StateInfo.HasChanges
				|| ConsignorPickupAddress.E2_RN_NKCountryCodeInfo.HasChanges
				|| ConsignorPickupAddress.E2_CityInfo.HasChanges)
			{
				DeliveryDueDateFactorHasChanged(DeliveryDueDateChangedFactor.PickupFromAddress);
			}
			base.ConsignorPickupAddressChanged();

			if (!IsDeleted)
			{
				MarkAsNeedingValidation();

				this.UpdateCO2eStatusToNotCurrent(new CO2eStatusChangedReason((NoResString)"Consignor Pickup Address"), IsCopying);
				UpdateContainersCO2eStatusOnConsignorPickupAddressChanged();
			}
		}

		#endregion

		protected override void DefaultNumberOfBillsByReleaseType(ZString releaseType)
		{
			if (this.BuyerSupplierLinksHelper != null)
			{
				this.BuyerSupplierLinksHelper.RestoreNumberOfBills();
			}
			else
			{
				DefaultNumberOfBillsWithFallback(releaseType);
			}
		}

		protected override void DefaultNumberOfBillsWithFallback(ZString releaseType)
		{
			if (!IsCopying)
			{
				if (!IsSettingDefaultValues
					&& releaseType != Core.Constants.ShipmentReleaseTypes.ExpressBofL
					&& Consignee != null && Consignee.MiscServ != null
					&& (Consignee.MiscServ.OM_IMOriginalSeaBills > 0 || Consignee.MiscServ.OM_IMCopySeaBills > 0))
				{
					JS_NoOriginalBills = Consignee.MiscServ.OM_IMOriginalSeaBills;
					JS_NoCopyBills = Consignee.MiscServ.OM_IMCopySeaBills;
				}
				else
				{
					JS_NoOriginalBills = Factory.GetCachedValue("OriginalsNumberByType:" + releaseType, () => (byte)FreightDataRegistry.Instance.ReleaseTypes.Value.OriginalsNumberByType(releaseType)); // this is a key for a dictionary
					JS_NoCopyBills = Factory.GetCachedValue("CopiesNumberByType:" + releaseType, () => (byte)FreightDataRegistry.Instance.ReleaseTypes.Value.CopiesNumberByType(releaseType)); // this is a key for a dictionary
				}
			}
		}

		protected override ZGuid? GetDefaultImportBrokerPK()
		{
			ZGuid? result = null;

			result = GetDefaultImportBrokerFromBuyerSupplierRelationshipDelegate();

			if (!result.HasValue)
			{
				result = GetImportBrokerFallback();
			}

			return result;
		}

		ZGuid? GetDefaultImportBrokerFromBuyerSupplierRelationship()
		{
			ZGuid? result = null;

			if (BuyerSupplierLinksHelper != null)
			{
				result = BuyerSupplierLinksHelper.GetImportBrokerFromBuyerSupplierLink();
			}

			return result;
		}

		ZGuid? GetImportBrokerFallback()
		{
			return (!IsChangingConsigneeAddress || !IsInDatabase) && (!IsChangingConsignorAddress || !IsInDatabase) ? base.GetDefaultImportBrokerPK() : null;
		}

		void DefaultEFreightStatusWithFallBack(ZString defaultValue)
		{
			if (!IsCopying && !fIsImportingData && !IsSettingDefaultValues && IsAir)
			{
				if (!defaultValue.IsEmpty)
				{
					JS_EFreightStatus = defaultValue;
				}
				else
				{
					DefaultEFreightStatusFromConsol();
				}
			}
		}

		void DefaultEFreightStatusFromConsol()
		{
			var consol = DepartureConsol;
			if (JS_EFreightStatus.IsEmpty && consol != null)
			{
				var rule = consol.GetEFreightRule();
				if (rule != null)
				{
					JS_EFreightStatus = rule.RME_EFreightStatus;
				}
			}
		}

		#endregion

		#region IDeniedPartyProvider Members

		ZString IDeniedPartyProvider.ReferenceId => JS_UniqueConsignRef;

		#endregion

		#region IScreeningPartyProvider Members

		ZString IScreeningStatusProvider.ScreeningStatus
		{
			get { return JS_ScreeningStatus; }
			set { JS_ScreeningStatus = value; }
		}

		ZBool IShouldUpdateScreeningStatus.ShouldUpdateScreeningStatus
		{
			get;
			set;
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		ScreeningParty[] IScreeningPartyProvider.ScreeningParties
		{
			get
			{
				var result = GetScreeningPartiesCommon();

				foreach (Transport transport in Transports)
				{
					result.AddRange(transport.GetScreeningPartiesFromVessel(this));
				}

				if (IsMasterShipmentRepresentingAllChildShipments || IsHighVolumeLowValue)
				{
					IEnumerable<ScreeningParty> childrenParties;

					if (IsHighVolumeLowValue && HVLVDataRegistry.Instance.HVLVEnablePartyScreening.Value.EnableHVLVPartyScreening)
					{
						childrenParties = (HVLVConsignmentHeader as IScreeningPartyProvider).ScreeningParties;
					}
					else
					{
						var subForwardingShipments = CoLoadShipments.Where(shipment => shipment is ForwardingShipment);
						childrenParties = subForwardingShipments.OfType<IScreeningPartyProvider>().SelectMany(c => c.ScreeningParties);
					}

					foreach (var party in childrenParties)
					{
						party.AddParent(this);
					}

					result.AddRange(childrenParties);
				}

				return result.ToArray();
			}
		}

		List<ScreeningParty> GetScreeningPartiesCommon()
		{
			List<ScreeningParty> result = new List<ScreeningParty>();

			foreach (JobDocAddress docAddress in DocAddresses)
			{
				result.Add(new ScreeningParty(this, docAddress.AddressCaption, docAddress));
			}

			result.Add(new ScreeningParty(this, Res.GetString("7f183a22-ca1f-4506-a476-8cca8051f8ad", "Delivery Agent"), DeliveryAgent));

			if (Job != null)
			{
				result.Add(new ScreeningParty(this, Res.GetString("7cd6f3c2-3243-4e05-ac21-a92cea62afb5", "Local Client"), Job.LocalCharges));
			}

			result.Add(new ScreeningParty(this, Res.GetString("73765901-c32e-4493-8991-47031b70a06c", "Export Broker"), ExportBroker));

			if (!IsDeleted)
			{
				result.Add(new ScreeningParty(this, Res.GetString("7e319bb4-83fc-4de3-a2c0-220e12cb5be9", "Pickup Local Transport Company"), DocsAndCartage.PickupCartageCo));
			}

			if (ExportReceivingDepot != null)
			{
				result.Add(new ScreeningParty(this, Res.GetString("a3004438-627b-4520-8780-18ed213a4262", "Export CFS"), ExportReceivingDepot.Header));
			}

			result.Add(new ScreeningParty(this, Res.GetString("2abb7e4d-2d76-4eeb-8f94-ae8c6081b9a5", "Import Broker"), ImportBroker));

			if (!IsDeleted)
			{
				result.Add(new ScreeningParty(this, Res.GetString("29503dde-5b93-42f0-be64-a4873651612b", "Delivery Local Transport Company"), DocsAndCartage.DeliveryCartageCo));
			}

			if (ImportReleaseDepot != null)
			{
				result.Add(new ScreeningParty(this, Res.GetString("749821b3-f7b0-4e32-8c5e-6125361646c3", "Import CFS"), ImportReleaseDepot.Header));
			}

			foreach (BusinessObject declaration in Declarations)
			{
				IScreeningPartyProvider declarationProvider = declaration as IScreeningPartyProvider;
				if (declarationProvider != null)
				{
					ScreeningParty[] partiesFromDeclaration = declarationProvider.ScreeningParties;
					foreach (ScreeningParty party in partiesFromDeclaration)
					{
						party.AddParent(this);
						party.LinkEntityToAssociatedJobIfApplicable(this);
					}
					result.AddRange(partiesFromDeclaration);
				}
			}

			foreach (PackLine line in OuterPackLines)
			{
				foreach (UNDGDataItem dgData in line.UNDGs)
				{
					result.Add(new ScreeningParty(this, Res.GetString("7E68118C-B304-47A5-8647-75EA54B571E2", "UNDG Contact Name"), dgData.DGContact?.Header));
				}
			}

			return result;
		}

		ZString IScreeningPartyProvider.GetWorstScreeningStatus()
		{
			var statuses = new List<ZString>();
			GetScreeningStatus(statuses);
			return ScreeningStatusUpdater.GetWorstScreeningStatus(statuses);
		}

		ZString IScreeningPartyProvider.GetWorstScreeningStatusUnlessManuallyCleared()
		{
			var statuses = new List<ZString>();

			if (JS_ScreeningStatus != ScreeningStatusesList.Codes.JobCleared)
			{
				GetScreeningStatus(statuses);
			}
			else
			{
				statuses.Add(ScreeningStatusesList.Codes.JobCleared);
			}

			return ScreeningStatusUpdater.GetWorstScreeningStatus(statuses);
		}

		void GetScreeningStatus(List<ZString> statuses)
		{
			AddScreeningStatusToList(statuses, Job?.LocalCharges);
			AddScreeningStatusToList(statuses, DocsAndCartage?.PickupCartageCo);
			AddScreeningStatusToList(statuses, ExportReceivingDepot?.Header);
			AddScreeningStatusToList(statuses, DocsAndCartage?.DeliveryCartageCo);
			AddScreeningStatusToList(statuses, ImportReleaseDepot?.Header);
			AddScreeningStatusToList(statuses, ExportBroker);
			AddScreeningStatusToList(statuses, ImportBroker);
			AddScreeningStatusToList(statuses, DeliveryAgent);

			AddScreeningStatusToList(statuses, GetWorstTransportScreeningStatus());
			AddScreeningStatusToList(statuses, GetWorstDeclarationScreeningStatus());
			AddScreeningStatusToList(statuses, GetWorstDocAddressScreeningStatus());
			AddScreeningStatusToList(statuses, GetWorstUNDGContactScreeningStatus());
			AddScreeningStatusToList(statuses, GetWorstSubShipmentsScreeningStatus());
		}

		ZString GetWorstTransportScreeningStatus()
		{
			var parties = Transports.Cast<Transport>()
									.Select(tran => tran.Vessel)
									.Cast<IScreeningPartyProvider>();

			return ScreeningStatusUpdater.GetWorstScreeningStatus(parties);
		}

		ZString GetWorstDeclarationScreeningStatus()
		{
			return ScreeningStatusUpdater.GetWorstScreeningStatus(Declarations.Cast<IScreeningPartyProvider>());
		}

		ZString GetWorstDocAddressScreeningStatus()
		{
			return ScreeningStatusUpdater.GetWorstScreeningStatus(DocAddresses.Cast<IScreeningPartyProvider>());
		}

		ZString GetWorstUNDGContactScreeningStatus()
		{
			var dgContacts = from line in OuterPackLines.Cast<PackLine>()
							 from dgData in line.UNDGs.Cast<UNDGDataItem>()
							 select dgData.DGContact?.Header as IScreeningPartyProvider;

			return ScreeningStatusUpdater.GetWorstScreeningStatus(dgContacts);
		}

		ZString GetWorstSubShipmentsScreeningStatus()
		{
			ZString result = ZString.Empty;
			if (IsMasterShipmentRepresentingAllChildShipments || IsHighVolumeLowValue)
			{
				IEnumerable<IScreeningPartyProvider> childrenParties;

				if (IsHighVolumeLowValue && HVLVConsignmentHeader != null && HVLVDataRegistry.Instance.HVLVEnablePartyScreening.Value.EnableHVLVPartyScreening)
				{
					result = ((IScreeningPartyProvider)HVLVConsignmentHeader).GetWorstScreeningStatus();
				}
				else
				{
					childrenParties = from shipment in CoLoadShipments.Where(shipment => shipment is ForwardingShipment)
									  select shipment as IScreeningPartyProvider;

					result = ScreeningStatusUpdater.GetWorstScreeningStatus(childrenParties);
				}
			}

			return result;
		}

		void AddScreeningStatusToList(List<ZString> list, IScreeningPartyProvider party)
		{
			var status = party?.GetWorstScreeningStatus();
			AddScreeningStatusToList(list, status);
		}

		void AddScreeningStatusToList(List<ZString> list, ZString? status)
		{
			if (status.HasValue)
			{
				list.Add(status.Value);
			}
		}

		#endregion

		#region IRelatedOrgDeniedPartyScreenable Members

		public IRelatedOrgPartyScreeningStatusCollection RelatedOrgPartyScreeningStatusCollection
		{
			get
			{
				var pks = GetPksFromAllSubShipmentsAndDeclarationsWithoutChildren();
				pks.Add(PK);
				AddPksFromDeclaration(this, pks);

				return RelatedOrgPartyScreeningStatusHelper.GetRelatedOrgPartyScreeningStatusCollection(Factory, ((IScreeningPartyProvider)this).ScreeningParties, relatedJobPKs: pks.ToArray());
			}
		}

		#endregion

		#region ICreditControlledDocumentDelivery Members

		protected override bool IsDPSFreightMovementRestrictedCore()
		{
			return ObjectFactory.Get<IComplianceRiskStatusSupporter>().IsDPSFreightMovementRestricted(JS_ScreeningStatus, this, this);
		}

		protected override ScreeningParty[] GetScreeningPartiesCore()
		{
			return ((IScreeningPartyProvider)this).ScreeningParties;
		}

		protected override bool IsAviationSecurityFreightMovementRestrictedCore()
		{
			return AviationSecurity.SupplyChainSecurityConfiguration.IsAviationSecurityFreightMovementRestricted(this);
		}

		#endregion

		#region IDocumentDataStateManager Members

		string IDocumentDataStateManager.DataStateErrorMessage
		{
			get { return dataStateErrorMessage; }
		}
		string dataStateErrorMessage;

		DocumentDataStateManagerResult IDocumentDataStateManager.Evaluate(IStmMenuItem commandAboutToBeRun)
		{
			DocumentDataStateManagerResult result = DocumentDataStateManagerResult.NotApplicable;
			isDataStateValid = false;
			IDocumentDataStateManager dataStateManager = ServiceLocator.GetService<IDocumentDataStateManager>(DeclarationForDocuments);
			if (dataStateManager != null)
			{
				result = dataStateManager.Evaluate(commandAboutToBeRun);
				dataStateErrorMessage = dataStateManager.DataStateErrorMessage;
				isDataStateValid = dataStateManager.IsDataStateValid;
			}
			return result;
		}

		bool IDocumentDataStateManager.IsDataStateValid
		{
			get { return isDataStateValid; }
		}
		bool isDataStateValid;

		#endregion

		#region ICustomLabelsConfigOrgProvider Members

		OrgHeader ICustomLabelsConfigOrgProvider.ConfigOrg
		{
			get { return this.Consignee; }
		}

		event EventHandler ICustomLabelsConfigOrgProvider.ConfigOrgChanged
		{
			add { ConsigneePKInfo.ValueChanged += value; }
			remove { ConsigneePKInfo.ValueChanged -= value; }
		}

		#endregion

		#region ISendEmailSource Members

		AddressBookSelection ISendEmailSource.GetAddressBookSelection()
		{
			return GetAddressBookSelection();
		}

		protected virtual AddressBookSelection GetAddressBookSelection()
		{
			AddressBookSelection result = new AddressBookSelection();

			result.AddRecipient(NotifyParty);
			result.AddRecipient(Consignor);
			result.AddRecipient(Consignee);
			foreach (ISendEmailSource declaration in Declarations.OfType<ISendEmailSource>())
			{
				result.Add(declaration.GetAddressBookSelection());
			}
			result.AddRecipient(DeliveryAgent);
			result.AddRecipient(ExportBroker);
			result.AddRecipient(DocsAndCartage.PickupCartageCo);
			result.AddRecipient(ImportBroker);
			result.AddRecipient(DocsAndCartage.DeliveryCartageCo);

			JobHeader job = new JobHeader.Loader(this).Load(false, false);
			result.AddRecipient(job == null ? null : job.LocalCharges);
			result.AddRecipient(job == null ? null : job.AgentCollect);

			return result;
		}

		string ISendEmailSource.EmailSubject
		{
			get { return Res.GetString("c854c20c-99b4-43ce-a0f8-20d2cce0b639", "Shipment -") + " " + this.JS_UniqueConsignRef; }
		}

		string ISendEmailSource.TemplateCategory
		{
			get { return GetTemplateCategory(); }
		}

		protected virtual string GetTemplateCategory()
		{
			return MailTemplateCategoryList.Codes.ForwardingShipments;
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
			get { return ObjectFactory.GetType<DocumentWrappers.IDocForwardingShipment>(); }
		}

		#endregion

		#region ITemplateCopyable Members

		public override IBusiness TemplateCopy(BusinessObjectFactory alternativeFactory = null)
		{
			if (IsTemplate && alternativeFactory == null)
			{
				alternativeFactory = new BusinessObjectFactory();
				ChildEditableService.SetState(alternativeFactory, ChildEditableServiceStates.Shipment);
			}

			IBusiness result = base.TemplateCopy(alternativeFactory);

			if (ShouldCustomsDataBeCopied)
			{
				var jobDecFilter = Enterprise.Customs.Common.JobDeclarationFilter.ForCompanyAndShipment(true, GlbCompany.CurrentCompany, PK);
				var jobDecPlugged = Factory.LoadTop1<IBaseJobDeclaration>(jobDecFilter);
				if (jobDecPlugged != null)
				{
					var originalDec = jobDecPlugged as ITemplateCopyable;
					var copiedJobDec = originalDec.TemplateCopy() as BusinessObject;
					copiedJobDec[ZArchitecture.Schema.JobDeclarationSchema.JE_JS.Name] = ((BusinessObject)result).PK;
				}

				Customs.Common.Shared.CustomsTemplateCopyableFinder.CopyCountrySpecificData(this, (ICommonShipment)result);
			}

			return result;
		}

		ZBool ShouldCustomsDataBeCopied => Env.Security.CustomsDeclarationEnquiryNew.IsAllowed
			&& !isInUseBookingOnlyCloningMode
			&& ShouldCustomsDataBeCopiedCore;

		protected virtual ZBool ShouldCustomsDataBeCopiedCore => true;

		#endregion

		#region JS_OverrideWaybillDefaults

		public event CancelEventHandler OnOverrideWaybillDefaultsChanging;

		public override ZBool JS_OverrideWaybillDefaults
		{
			get { return base.JS_OverrideWaybillDefaults; }
			set
			{
				if (base.JS_OverrideWaybillDefaults != value)
				{
					if (!IsOverrideAllowed && !IsSettingDefaultOrImportingData)
					{
						JS_OverrideWaybillDefaultsInfo.RefreshBinding();

						return;
					}

					if (value == ZBool.False)
					{
						CancelEventArgs args = new CancelEventArgs(false);
						if (OnOverrideWaybillDefaultsChanging != null)
						{
							OnOverrideWaybillDefaultsChanging(this, args);
						}

						if (!args.Cancel)
						{
							base.JS_OverrideWaybillDefaults = value;
							OverrideWaybillDefaultsHasChanges = true;

							if (IsAWBHeaderAccessible)
							{
								AWBHeader.Populate();
							}
						}
						else
						{
							JS_OverrideWaybillDefaultsInfo.RefreshBinding();
						}
					}
					else
					{
						base.JS_OverrideWaybillDefaults = value;

						if (IsAWBHeaderAccessible)
						{
							AWBHeader.EH_AreRateLinesOverridden = true;
						}

						OverrideWaybillDefaultsHasChanges = true;
					}

					AWBHeaderManager.SetReadOnlyIncludingChildren(!JS_OverrideWaybillDefaults);
				}
			}
		}

		public bool OverrideWaybillDefaultsHasChanges { get; private set; }

		#endregion

		#region IAWBParent Members

		public bool IsAWBHeaderAccessible
		{
			get { return IsAir || IsSeaAir; }
		}

		public ExportAWBHeader AWBHeader
		{
			get { return AWBHeaderManager.AWBHeader; }
		}

		[ChildEditable(true)]
		public AWBHeaderManager<ForwardingShipment> AWBHeaderManager
		{
			get
			{
				if (awbHeaderManager == null)
				{
					awbHeaderManager = new AWBHeaderManager<ForwardingShipment>(this);
					RegisterEditableChildObject(awbHeaderManager);
					awbHeaderManager.SetReadOnlyIncludingChildren(!JS_OverrideWaybillDefaults);
				}

				return awbHeaderManager;
			}
		}
		AWBHeaderManager<ForwardingShipment> awbHeaderManager;

		public void PopulateAWBForDocuments()
		{
			if (!AWBForDocumentsPopulated)
			{
				PopulateAWB();
				AWBForDocumentsPopulated = true;
			}
		}
		internal bool AWBForDocumentsPopulated { get; set; }

		public void PopulateAWB()
		{
			PopulateAWB(onlyIfAWBExists: false);
		}

		void PopulateAWB(bool onlyIfAWBExists)
		{
			if (IsAWBHeaderAccessible && (!onlyIfAWBExists || IsAWBLoaded) && !(TemplateRecord?.IsForTemplateSearch ?? false))
			{
				AWBHeader.PopulateIfNotOverridden();
			}
		}

		ExportAWBHeader IAWBParent.LoadOrCreateAWB()
		{
			return ShipmentExportAWBHeader.LoadOrCreate(this);
		}

		void IAWBParent.NotifyConcurrencyHandled()
		{
			AWBHeaderManager.Refresh();
		}

		public void ResetAddressPickerDropLists()
		{
			if (IsAWBHeaderAccessible)
			{
				AWBHeader.ResetAddressPickerDropLists();
			}
		}

		public bool IsAWBLoaded
		{
			get { return AWBHeaderManager.Count > 0; }
		}

		public ZBool IsAWBValuesOverriddenProperty
		{
			get { return JS_OverrideWaybillDefaults; }
			set { JS_OverrideWaybillDefaults = value; }
		}

		public ZPropertyInfo IsAWBValuesOverriddenPropertyInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(IsAWBValuesOverriddenProperty), x => JS_OverrideWaybillDefaultsInfo); }
		}

		event EventHandler IAWBParent.IsAWBHeaderAccessibleChanged
		{
			add
			{
				JS_TransportModeInfo.ValueChanged += value;
			}
			remove
			{
				JS_TransportModeInfo.ValueChanged -= value;
			}
		}

		public bool IsOverrideAllowed => Env.Security.MaintainShipmentAWBOverride.IsAllowed;

		ZString IAWBParent.HAWB
		{
			get { return JS_HouseBill; }
		}

		ZString IAWBParent.MAWB
		{
			get { return (DepartureConsol != null) ? DepartureConsol.JK_MasterBillNum : ZString.Empty; }
		}

		#endregion

		#region IBillDetails Members

		ZPropertyInfo IBillDetails.AMSBillNumberInfo
		{
			get
			{
				CusEntryNumber bill = Numbers.GetFirstReferenceNumberByType(CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.AMS);
				if (bill != null)
				{
					return bill.CE_EntryNumInfo;
				}
				return null;
			}
		}

		ZPropertyInfo IBillDetails.BillNumberInfo
		{
			get { return JS_HouseBillInfo; }
		}

		ZPropertyInfo IBillDetails.BKGBillNumberInfo
		{
			get
			{
				if (this.JS_TransportMode == Constants.TransportModes.Sea || this.JS_TransportMode == Constants.TransportModes.AirSea)
				{
					var referenceNumbers = Numbers.Cast<CusEntryNumber>().Where(x => x.CE_EntryType == CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.BKG && !x.CE_EntryNum.IsEmpty);

					if (referenceNumbers.Count() == 1)
					{
						return referenceNumbers.First().CE_EntryNumInfo;
					}
				}
				return null;
			}
		}

		ZPropertyInfo[] IBillDetails.GetNumberOfPackesInfos(ForwardingShipment shipment)
		{
			return new[] { JS_OuterPacksInfo };
		}
		ZPropertyInfo[] IBillDetails.GetTypeOfPackesInfos(ForwardingShipment shipment)
		{
			return new[] { JS_F3_NKPackTypeInfo };
		}

		IEnumerable<OrgHeader> IBillDetails.SCACIssuers
		{
			get
			{
				var houseBillIssuingParty = HouseBillIssuingParty;
				if (houseBillIssuingParty != null)
				{
					yield return houseBillIssuingParty;
				}
				if (!JS_JK_SendingAgent.IsEmpty)
				{
					var sendingAgent = Consols[0].SendingForwarder;
					yield return sendingAgent;
				}
				yield return Factory.Load<OrgHeader>(GlbCompany.CurrentCompany.GC_OH_OrgProxy);
			}
		}
		ZDateTime IBillDetails.BillUssueDate => JS_HouseBillIssueDate;
		#endregion

		#region IParentForCargoReporter Members

		Event IParentForCargoReporter.CargoReportSentEvent
		{
			get { return Events.CargoReportSent; }
		}

		Event IParentForCargoReporter.CargoReportAcceptedEvent
		{
			get { return Events.CargoReportAccepted; }
		}

		Event IParentForCargoReporter.CargoReportRejectedEvent
		{
			get { return Events.CargoReportRejected; }
		}

		Event IParentForCargoReporter.CargoReportWithdrawEvent
		{
			get { return Events.CargoReportWithdraw; }
		}

		ZString IParentForCargoReporter.CargoReporterReferenceText
		{
			get { return Res.GetString("df03688c-a2d4-4853-a8b4-aa839ea0ae12", "Shipment: {0}", JS_UniqueConsignRef); }
		}

		#endregion

		#region Order Support / IAttachOrders Members

		#region QueryAttachRelatedOrder Event

		protected void CheckQueryAttachRelatedOrder()
		{
			if (ShouldQueryAttachRelatedOrder())
			{
				ShipmentDomainService.GetInstance(Factory).RequestAttachRelatedOrder(this);
			}
		}

		protected bool ShouldQueryAttachRelatedOrder()
		{
			return
				!JS_TransportMode.IsEmpty && !JS_TransportModeInfo.HasErrors() &&
				!JS_PackingMode.IsEmpty && !JS_PackingModeInfo.HasErrors() &&
				ConsignorPK.IsValid &&
				ConsigneePK.IsValid &&
				!IsInDatabase &&
				AttachedOrders.Count == 0 &&
				StrictlyMatchedRelatedOrders.Count > 0;
		}

		public OrderCollection StrictlyMatchedRelatedOrders
		{
			get
			{
				// Do not cache the resulting collection, it should be re-created on every access with up-to-date filter

				ZQuery strictFilter = new ZQuery();
				strictFilter.AddToFilter(JobOrderHeaderSchema.JD_TransportMode, JS_TransportMode);
				if (JS_PackingMode == Constants.ContainerModes.BuyersConsol
					|| JS_PackingMode == Constants.ContainerModes.ShippersConsol)
				{
					strictFilter.AddToFilter(JobOrderHeaderSchema.JD_ContainerMode, Constants.ContainerModes.LCL);
				}
				else
				{
					strictFilter.AddToFilter(JobOrderHeaderSchema.JD_ContainerMode, JS_PackingMode);
				}

				if (Consignee != null)
				{
					var buyerAddresses = Consignee.Addresses.Select(x => x.PK);
					strictFilter.AddToFilter(JobOrderHeaderSchema.JD_OA_BuyerAddress, buyerAddresses);
				}
				else
				{
					strictFilter.AddToFilter(JobOrderHeaderSchema.JD_OA_BuyerAddress, ZGuid.Empty);
				}

				if (Consignor != null)
				{
					var supplierAddresses = Consignor.Addresses.Select(x => x.PK);
					strictFilter.AddToFilter(JobOrderHeaderSchema.JD_OA_SupplierAddress, supplierAddresses);
				}
				else
				{
					strictFilter.AddToFilter(JobOrderHeaderSchema.JD_OA_SupplierAddress, null);
				}

				strictFilter.AddToFilter(JobOrderHeaderSchema.JD_JS, null);
				strictFilter.AddToFilter(JobOrderHeaderSchema.JD_JE, null);

				OrderCollection result = Order.GetPossibleOrdersForAttachment_List(this);
				result.AdditionalFilter.AddToFilter(strictFilter);

				return result;
			}
		}

		#endregion

		[ChildEditable(false)]
		public OrderCollection AttachedOrders
		{
			get
			{
				if (attachedOrders == null)
				{
					attachedOrders = GetAttachedOrdersCollection();
					attachedOrders.CountChanged += delegate
					{
						ValidateWhenOrderCollectionCountChanged();
					};
					attachedOrders.CountChanged += (sender, args) => attachedGenericOrders?.BuildCollection();

					if (ChildEditableService.GetState(Factory) != ChildEditableServiceStates.Order)
					{
						RegisterEditableChildObject(attachedOrders);
					}
				}
				return attachedOrders;
			}
		}
		OrderCollection attachedOrders;

		protected virtual OrderCollection GetAttachedOrdersCollection()
		{
			return new OrderCollection(Factory, this);
		}

		public virtual OrderCollection PossibleOrdersForAttachment_List
		{
			get { return Order.GetPossibleOrdersForAttachment_List(this); }
		}

		public OrderCollection AttachedOrdersFromSupplierBooking => attachedOrdersFromSupplierBooking ??= GetAttachedOrdersFromSupplierBooking();
		OrderCollection attachedOrdersFromSupplierBooking;

		OrderCollection GetAttachedOrdersFromSupplierBooking()
		{
			var script = @$"
{JobOrderHeaderSchema.Constants.PK} IN (
	SELECT {JobOrderLineSchema.Constants.JO_JD}
	FROM {JobPackLinesSchema.Constants.SqlSchemaName}.{JobPackLinesSchema.Constants.TableName}
	INNER JOIN {ContainerLoadListLineSchema.Constants.SqlSchemaName}.{ContainerLoadListLineSchema.Constants.TableName} ON {ContainerLoadListLineSchema.Constants.CLL_JL_PackLine} = {JobPackLinesSchema.Constants.PK}
	INNER JOIN {JobSupplierBookingLineSchema.Constants.SqlSchemaName}.{JobSupplierBookingLineSchema.Constants.TableName} ON {JobSupplierBookingLineSchema.Constants.PK} = {ContainerLoadListLineSchema.Constants.CLL_JSL_BookingLine}
	INNER JOIN {JobOrderLineSchema.Constants.SqlSchemaName}.{JobOrderLineSchema.Constants.TableName} ON {JobOrderLineSchema.Constants.PK} = {JobSupplierBookingLineSchema.Constants.JSL_JO_OrderLine}
	WHERE {JobPackLinesSchema.Constants.JL_JS} = @shipmentPK
UNION
	SELECT {JobOrderLineSchema.Constants.JO_JD}
	FROM {JobPackLinesSchema.Constants.SqlSchemaName}.{JobPackLinesSchema.Constants.TableName}
	INNER JOIN {JobSupplierBookingLineSchema.Constants.SqlSchemaName}.{JobSupplierBookingLineSchema.Constants.TableName} ON {JobSupplierBookingLineSchema.Constants.PK} = {JobPackLinesSchema.Constants.JL_JSL_BookingLine}
	INNER JOIN {JobOrderLineSchema.Constants.SqlSchemaName}.{JobOrderLineSchema.Constants.TableName} ON {JobOrderLineSchema.Constants.PK} = {JobSupplierBookingLineSchema.Constants.JSL_JO_OrderLine}
	WHERE {JobPackLinesSchema.Constants.JL_JS} = @shipmentPK AND {JobPackLinesSchema.Constants.JL_JSL_BookingLine} IS NOT NULL)";

			var orderQuery = new ZDBOnlyQuery(typeof(Order));
			orderQuery.AddFilterAndZSQLParameterCollection(script, new ZSqlParameterCollection { { "@shipmentPK", this.PK, JobShipmentSchema.PK } });

			return new OrderCollection(Factory, orderQuery);
		}

		public void SetDefaultsOnOrder(Order newOrder)
		{
			if (!fIsImportingData)
			{
				((IBusinessObjectInternals)newOrder).IsCopying = true;
				try
				{
					//Set Buyer and Supplier before Transport & Container mode
					//as the Order sets Transport and Container mode based on Buyer + Supplier
					if (ConsigneeDocumentaryAddress.HasRealAddress || Consignee != null)
					{
						newOrder.JD_OA_BuyerAddress = ConsigneeDocumentaryAddress.HasRealAddress ? ConsigneeDocumentaryAddress.E2_OA_Address : Consignee.MainAddress.PK;
					}

					if (ConsignorDocumentaryAddress.HasRealAddress || Consignor != null)
					{
						newOrder.JD_OA_SupplierAddress = ConsignorDocumentaryAddress.HasRealAddress ? ConsignorDocumentaryAddress.E2_OA_Address : Consignor.MainAddress.PK;
					}

					newOrder.JD_TransportMode = JS_TransportMode;
					newOrder.JD_F3_NKPackType = JS_F3_NKPackType;

					if (JS_PackingMode == Constants.ContainerModes.BuyersConsol
						|| JS_PackingMode == Constants.ContainerModes.ShippersConsol)
					{
						newOrder.JD_ContainerMode = Constants.ContainerModes.LCL;
					}
					else
					{
						newOrder.JD_ContainerMode = JS_PackingMode;
					}

					newOrder.JD_OrderGoodsDescription = JS_GoodsDescription;
					newOrder.JD_RS_NKServiceLevel_NI = JS_RS_NKServiceLevel;
					newOrder.JD_IncoTerm = JS_INCO;
					if (Consols.Count == 1)
					{
						newOrder.JD_OH_SendingAgent = Consols[0].SendingForwarderPK;
						newOrder.JD_OH_ReceivingAgent = Consols[0].ReceivingForwarderPK;
					}

					newOrder.DefaultControllingCustomer();
				}
				finally
				{
					((IBusinessObjectInternals)newOrder).IsCopying = false;
					newOrder.RunDelayedUpdateDatesFromAttachedShipmentIfRequired();
				}
			}
		}

		public void OnOrderAttached(Order attachedOrder)
		{
			if (attachedOrder != null)
			{
				attachedOrder.RequiredDocuments.CopyToOtherCollection(DocsAndCartage.RequiredDocuments);

				if (attachedOrder.IsInDatabase)
				{
					attachedOrder.PopulateShipment();
				}
			}
		}

		public void CreatePackLinesFromOrderLines(IEnumerable<Order> orders)
		{
			var newFactory = new BusinessObjectFactory();
			var ordersInAnotherFactory = newFactory.Load<Order>(new ZQuery(JobOrderHeaderSchema.PK, orders.Select(o => o.PK).Distinct()));
			var helper = new OrderLineToPackLineConversionHelper(newFactory, this, ordersInAnotherFactory);

			if (OnOrderLineToPackLineConversion != null && helper.OrderLines.Count > 0)
			{
				var eventArgs = new OrderLineToPackLineConversionEventArgs(helper, false);
				OnOrderLineToPackLineConversion(this, eventArgs);

				if (string.IsNullOrEmpty(helper.ReasonForNotAbleToConvert) && eventArgs.ShouldCreatePacklines)
				{
					helper.CreatePackLines();
				}
			}
		}

		public event EventHandler<OrderLineToPackLineConversionEventArgs> OnOrderLineToPackLineConversion;

		public ZString[] GetOrderNumbers()
		{
			List<ZString> orderNumbers = new List<ZString>();
			if (AttachedOrders.Count == 0 && !IsDeleted)
			{
				foreach (OrderItem order in DocsAndCartage.OrderItems)
				{
					orderNumbers.Add(order.JT_OrderReference);
				}
			}
			else
			{
				foreach (Order order in AttachedOrders)
				{
					orderNumbers.Add(order.JD_OrderNumber);
				}
			}

			return orderNumbers.ToArray();
		}

		#endregion

		#region Warehouse

		[ChildEditable(false)]
		public IChildWhsOrderCollection AttachedWarehouseOrders
		{
			get
			{
				if (attachedWarehouseOrders == null)
				{
					attachedWarehouseOrders = GetAttachedWarehouseOrdersCollection();
					RegisterEditableChildObject(attachedWarehouseOrders);

					attachedWarehouseOrders.CountChanged += (sender, args) => attachedGenericOrders?.BuildCollection();
				}

				return attachedWarehouseOrders;
			}
		}

		IChildWhsOrderCollection attachedWarehouseOrders;

		protected IChildWhsOrderCollection GetAttachedWarehouseOrdersCollection()
		{
			return ObjectFactory.New<IChildWhsOrderCollection>(this);
		}

		#endregion

		#region IAttachGenericOrders Members

		[ChildEditable(false)]
		public GenericOrderCollection GenericOrders
		{
			get
			{
				if (attachedGenericOrders == null)
				{
					attachedGenericOrders = new GenericOrderCollection(Factory, this);
					attachedGenericOrders.BuildCollection();
					if (ChildEditableService.GetState(Factory) != ChildEditableServiceStates.Order)
					{
						RegisterEditableChildObject(attachedGenericOrders);
					}

					attachedGenericOrders.CountChanged += delegate
					{
						ValidateWhenOrderCollectionCountChanged();
					};
				}

				return attachedGenericOrders;
			}
		}

		public IList<IAttachedOrder> LegacyOrders => AttachedOrders.OfType<IAttachedOrder>().Union(AttachedWarehouseOrders.OfType<IAttachedOrder>()).OrderBy(o => o.JobType).ToList();// this is the order collection excludes supplier booking linked orders to avoid causing unnecessary performance issue for module search grid.

		void ValidateWhenOrderCollectionCountChanged()
		{
			if (IsDeleted)
			{
				return;
			}

			if (!IsValidationSuspended)
			{
				if (!DocsAndCartage.IsDeleted && !DocsAndCartage.IsValidationSuspended)
				{
					DocsAndCartage.Validation.ValidateJP_OrderItemsAsString();
				}

				if (!ConsigneeDocumentaryAddress.IsDeleted && !ConsigneeDocumentaryAddress.IsValidationSuspended)
				{
					ConsigneeDocumentaryAddress.Validation.ValidateOrganisationPK();
				}
			}
		}

		GenericOrderCollection attachedGenericOrders;

		#endregion

		#region ICDArchive Members

		public override CDArchiveInfo CDArchiveInfo
		{
			get { return new ForwardingShipmentCDArchiveInfo(this); }
		}

		public class ForwardingShipmentCDArchiveInfo : ShipmentCDArchiveInfo
		{
			public ForwardingShipmentCDArchiveInfo(ForwardingShipment shipment)
				: base(shipment)
			{
			}

			protected ForwardingShipment ShipmentWithOrders
			{
				get { return (ForwardingShipment)BusinessEntity; }
			}

			public static ZString[] GetOrderAndReferenceNumbers(CommonShipment shipment, IAttachOrders attachOrders)
			{
				List<ZString> result = new List<ZString>();
				if (shipment != null)
				{
					foreach (OrderItem orderItem in shipment.DocsAndCartage.OrderItems)
					{
						AddToList(result, orderItem.JT_OrderReference);
					}
				}
				foreach (Order order in attachOrders.AttachedOrders)
				{
					AddToList(result, order.JD_OrderNumber);
				}
				return result.ToArray();
			}

			public override ZString[] OrderNumbersList
			{
				get
				{
					ZString[] result;
					if (ShipmentWithOrders.Declarations.Length == 0)
					{
						result = GetOrderAndReferenceNumbers(ShipmentWithOrders, ShipmentWithOrders);
					}
					else
					{
						result = GetCombinedList(ShipmentWithOrders.Declarations, (CDArchiveInfo info) => info.OrderNumbersList);
					}
					return result;
				}
			}
		}

		#endregion

		#region IJobInvoicingPlugIn Members

		protected override CommonShipmentInvoicingSupporter GetNewInvoicingSupporter()
		{
			return new ForwardingShipmentInvoicingSupporter(this);
		}

		public class ForwardingShipmentInvoicingSupporter : BaseForwardingShipmentInvoicingSupporter
		{
			public ForwardingShipmentInvoicingSupporter(ForwardingShipment parent)
				: base(parent)
			{
				forwardingShipment = parent;
			}

			readonly ForwardingShipment forwardingShipment;
			public override string GetReasonNotToAllowAutoRate(AutoRateOptions options = default)
			{
				var result = base.GetReasonNotToAllowAutoRate(options);
				// Only Forwarding Shipment checks for Client Contract Number rules before autorating revenue now.
				if (string.IsNullOrEmpty(result) && options.AutoRateRevenue && Shipment.Numbers.HasClientContractNumberRulesViolation())
				{
					result = Res.GetString("97290b43-226a-4b65-be9d-084ed923308d", "Only one Client Contract Number, regardless same or different Country/Region of Issue, can be used for Autorating Revenue.");
				}

				return result;
			}

			public override OrgHeader GetDefaultCreditor(DefaultCreditorSetting defaultCreditorSetting)
			{
				var rateProviderOrgPK = defaultCreditorSetting.RateProviderOrgPK;

				if (forwardingShipment != null && rateProviderOrgPK.IsValid)
				{
					var pickupTransport = forwardingShipment.DocsAndCartage?.JP_OA_PickupCartageCoAddr_ZAddress.OrgHeader as OrgHeader;

					if (pickupTransport != null &&
						forwardingShipment.ConsignorPickupAddress?.E2_OA_Address_ZAddress?.OrgAddress is OrgAddress pickupFrom &&
						!string.IsNullOrWhiteSpace(pickupFrom.ClosestPort))
					{
						var orgRelatedPartyFilter = new DefaultCreditorHelper.OrgRelatedPartyFilter()
						{
							TransportMode = forwardingShipment.JS_TransportMode,
							ContainerMode = forwardingShipment.JS_PackingMode,
							CreditorType = DefaultCreditorHelper.CreditorType.PickupTransport,
							UNLOCO = pickupFrom.ClosestPort
						};

						var creditor = DefaultCreditorHelper.GetCreditorOrgHeaderFromOrgRelatedParties(pickupTransport, orgRelatedPartyFilter, forwardingShipment.Factory);

						if (creditor != null)
						{
							if (creditor.PK == rateProviderOrgPK || pickupTransport.PK == rateProviderOrgPK)
							{
								return creditor;
							}
						}

						if (pickupTransport.PK == rateProviderOrgPK)
						{
							return pickupTransport;
						}
					}

					var deliveryTransport = forwardingShipment.DocsAndCartage?.JP_OA_DeliveryCartageCoAddr_ZAddress.OrgHeader as OrgHeader;

					if (deliveryTransport != null &&
						forwardingShipment.ConsigneeDeliveryAddress?.E2_OA_Address_ZAddress?.OrgAddress is OrgAddress deliverTo &&
						!string.IsNullOrWhiteSpace(deliverTo.ClosestPort))
					{
						var orgRelatedPartyFilter = new DefaultCreditorHelper.OrgRelatedPartyFilter()
						{
							TransportMode = forwardingShipment.JS_TransportMode,
							ContainerMode = forwardingShipment.JS_PackingMode,
							CreditorType = DefaultCreditorHelper.CreditorType.DeliveryTransport,
							UNLOCO = deliverTo.ClosestPort
						};

						var creditor = DefaultCreditorHelper.GetCreditorOrgHeaderFromOrgRelatedParties(deliveryTransport, orgRelatedPartyFilter, forwardingShipment.Factory);

						if (creditor != null)
						{
							if (creditor.PK == rateProviderOrgPK || deliveryTransport.PK == rateProviderOrgPK)
							{
								return creditor;
							}
						}

						if (deliveryTransport.PK == rateProviderOrgPK)
						{
							return deliveryTransport;
						}
					}
				}

				return base.GetDefaultCreditor(defaultCreditorSetting);
			}
		}

		public class BaseForwardingShipmentInvoicingSupporter : CommonShipmentInvoicingSupporter, IServiceDirection, ISalesRepDefaultingFromControllingCustomer, IGatewayJobInvoicingSupporter
		{
			public BaseForwardingShipmentInvoicingSupporter(ForwardingShipment parent)
				: base(parent)
			{
			}

			public override bool CreateAccountingJobOnSavingOfOperationsJob
			{
				get
				{
					return Shipment.CoLoadMasterShipment != null
						? Shipment.CoLoadMasterShipment.JS_ShipmentType.Equals(Constants.ShipmentTypes.BuyersConsolLead)
						: base.CreateAccountingJobOnSavingOfOperationsJob;
				}
			}

			public ReadOnlyCollection<IJobInvoicingPlugIn> OrderedInvoiceTargets
			{
				get
				{
					var orderedTransports = new TransportOrderHelper(Shipment.TransportsIncludingRelated);

					var selectedOnes = orderedTransports
						.Where(x => x.JW_ParentType == Constants.TransportParentTypes.Consol)
						.Select(x => x.Parent)
						.OfType<ForwardingConsol>();
#if NETFRAMEWORK
					var orderedConsols = selectedOnes.DistinctBy(x => x.PK).Cast<IJobInvoicingPlugIn>()
						.Where(x => !string.IsNullOrWhiteSpace(x.JobNumber));
#else
					var orderedConsols = Enumerable.DistinctBy(selectedOnes, x => x.PK).Cast<IJobInvoicingPlugIn>()
						.Where(x => !string.IsNullOrWhiteSpace(x.JobNumber));
#endif
					return new ReadOnlyCollection<IJobInvoicingPlugIn>(orderedConsols.ToList());
				}
			}

			public override OrgHeader EarliestSendingAgent => AccountingMasterFilesRegistry.Instance.DefaultOSAgentFromPickupAgent.Value && Shipment.PickupAgent != null
																? Shipment.PickupAgent
																: Shipment.Consols.GetEarliestConsol()?.SendingForwarder;

			public override OrgHeader LatestReceivingAgent => Shipment.DeliveryAgent ?? Shipment.Consols.GetLatestConsol()?.ReceivingForwarder;

			public override OrgHeader Consignor
			{
				get
				{
					OrgHeader result = Shipment.Consignor;
					if (Shipment.NotifyParty != null && CreatedViaWeb)
					{
						result = Shipment.NotifyParty;
					}
					return result;
				}
			}

			public override OrgHeader Consignee
			{
				get
				{
					OrgHeader result = Shipment.Consignee;
					if (Shipment.NotifyParty != null && CreatedViaWeb)
					{
						result = Shipment.NotifyParty;
					}
					return result;
				}
			}

			public override OrgHeader ControllingCustomer
			{
				get
				{
					var address = Shipment.DocAddresses.FindByDocAddressType(DocAddressType.ControllingCustomer);
					return address?.Organisation;
				}
			}

			public override OrgHeader ControllingAgent
			{
				get
				{
					var address = Shipment.DocAddresses.FindByDocAddressType(DocAddressType.ControllingAgent);
					return address?.Organisation;
				}
			}

			public OrgHeader WarehouseClient
			{
				get
				{
					var address = Shipment.DocAddresses.FindByDocAddressType(DocAddressType.WarehouseClient);
					return address?.Organisation;
				}
			}

			bool CreatedViaWeb
			{
				get { return Shipment.Logs.CreatedByUserInitials == "ZZ"; } // Created via ediWebTracker
			}

			public new ForwardingShipment Shipment
			{
				get { return (ForwardingShipment)base.Shipment; }
			}

			public override string GetReasonNotToAllowChangeOnCostDetails(ZGuid chargeCodePK)
			{
				string result = null;

				foreach (BusinessObject declaration in Shipment.Declarations)
				{
					if ((ZGuid)declaration[JobDeclarationSchema.JE_GB] == GlbBranch.CurrentBranch.PK)
					{
						result = ((IJobInvoicingPlugIn)declaration).InvoicingSupporter.GetReasonNotToAllowChangeOnCostDetails(chargeCodePK);
						break;
					}
				}

				return result;
			}

			public override string GetReasonNotToAllowAutoRate(AutoRateOptions options = default)
			{
				string result = base.GetReasonNotToAllowAutoRate(options);

				if (string.IsNullOrEmpty(result))
				{
					var jobInvoicingSupporter = JobInvoicingSupporterFromDeclarations;
					if (jobInvoicingSupporter != null)
					{
						result = jobInvoicingSupporter.GetReasonNotToAllowAutoRate();
					}
				}

				if (string.IsNullOrEmpty(result) && Shipment.JS_TransportModeInfo.HasErrors())
				{
					result = Res.GetString("99810DC8-5972-486A-BB82-6A9630F11AC1", "Autorating cannot be run because transport '{0}' is invalid.", Shipment.JS_TransportMode);
				}

				return result;
			}

			public override string GetWarningForContinueAutoRate()
			{
				string result = base.GetWarningForContinueAutoRate();

				if (string.IsNullOrEmpty(result))
				{
					var jobInvoicingSupporter = JobInvoicingSupporterFromDeclarations;
					if (jobInvoicingSupporter != null)
					{
						result = jobInvoicingSupporter.GetWarningForContinueAutoRate();
					}
				}

				return result;
			}

			IJobInvoicingSupporter JobInvoicingSupporterFromDeclarations
			{
				get
				{
					if (jobInvoicingSupporterFromDeclarations == null)
					{
						jobInvoicingSupporterFromDeclarations = new CachedProperty<IJobInvoicingSupporter>(Shipment.Factory, () =>
						{
							foreach (BusinessObject declaration in Shipment.Declarations)
							{
								var companyPK = (ZGuid)declaration[JobDeclarationSchema.JE_GC];
								if (companyPK == GlbCompany.CurrentCompany.PK)
								{
									return ((IJobInvoicingPlugIn)declaration).InvoicingSupporter;
								}
							}
							return null;
						});
					}

					return jobInvoicingSupporterFromDeclarations.Value;
				}
			}

			CachedProperty<IJobInvoicingSupporter> jobInvoicingSupporterFromDeclarations;

			public override ILocation FixedPlaceOfSupply
			{
				get
				{
					var result = base.FixedPlaceOfSupply;

					if (AccountingMasterFilesRegistry.Instance.UseCusClearPortAsHomeCntryForTaxOvrds.Value)
					{
						var jobInvoicingSupporter = JobInvoicingSupporterFromDeclarations;
						if (jobInvoicingSupporter != null)
						{
							result = jobInvoicingSupporter.FixedPlaceOfSupply;
						}
					}

					return result;
				}
			}

			public override RefUNLOCO Destination
			{
				get
				{
					var result = base.Destination;

					if (AccountingMasterFilesRegistry.Instance.UseBrokerageLocationsForBilling.Value)
					{
						var jobInvoicingSupporter = JobInvoicingSupporterFromDeclarations;
						if (jobInvoicingSupporter != null)
						{
							result = jobInvoicingSupporter.Destination;
						}
					}

					return result;
				}
			}

			public override RefUNLOCO Origin
			{
				get
				{
					var result = base.Origin;

					if (AccountingMasterFilesRegistry.Instance.UseBrokerageLocationsForBilling.Value)
					{
						var jobInvoicingSupporter = JobInvoicingSupporterFromDeclarations;
						if (jobInvoicingSupporter != null)
						{
							result = jobInvoicingSupporter.Origin;
						}
					}

					return result;
				}
			}

			public override bool IsStandaloneShipment => Shipment.Consols.IsNullOrEmpty();

			public override bool IsImport
			{
				get { return Shipment.IsImport(); }
			}

			public override bool IsExport
			{
				get { return Shipment.IsExport(); }
			}

			public override bool IsDomestic
			{
				get { return Shipment.IsDomestic(); }
			}

			public override bool IsCrossTrade
			{
				get
				{
					ZString origin = Origin != null ? Origin.Code : ZString.Empty;
					ZString destination = Destination != null ? Destination.Code : ZString.Empty;
					return ImportExportHelper.IsCrossTrade(origin, destination);
				}
			}

			public ZString ServiceDirection
			{
				get
				{
					ZString result = OrgConstants.ServiceDirection.Code.Unknown;
					if (IsImport)
					{
						result = OrgConstants.ServiceDirection.Code.Import;
					}
					else if (IsDomestic)
					{
						result = OrgConstants.ServiceDirection.Code.Domestic;
					}
					else if (IsExport)
					{
						result = OrgConstants.ServiceDirection.Code.Export;
					}
					else if (IsCrossTrade)
					{
						result = OrgConstants.ServiceDirection.Code.CrossTrade;
					}
					return result;
				}
			}

			protected override SecurityCheckpoint GetAuditSecurityCore()
			{
				return Env.Security.MaintainShipmentAuditBilling;
			}

			protected override bool IncludeInConsolCostingCore(bool includeRelatedShipments)
			{
				return (!Shipment.IsCancelled && (includeRelatedShipments || Shipment.InvoicingSupporter.ShipmentNumberOfColoadMaster.IsEmpty));
			}

			public override OrgHeader OverriddenDefaultLocalClient
			{
				get { return Shipment.IsHighVolumeLowValueLegacy && Consignor != null ? Consignor.GetFreightBillTo(false, TransportMode, ContainerMode) : null; }
			}

			public override bool HasContainerCostShare(AccChargeCode chargeCode)
			{
				return Shipment.Containers != null && chargeCode != null
						&& Shipment.Containers.OfType<ForwardingContainer>().Any(c => c.Services.OfType<JobService>().Any(s => s.ES_ServiceCode == chargeCode.AC_ChargeSubGroup));
			}

			public override bool TryGetContainersCostShare(AccChargeCode chargeCode, IEnumerable<IJobPaymentBasis> paymentBases, ZGuid consolPK, out ZDecimal cost)
			{
				var consol = Shipment.Consols
					.Cast<ForwardingConsol>()
					.FirstOrDefault((c) => c.PK == consolPK);

				bool result = false;
				cost = 0m;

				if (paymentBases.Any())
				{
					if (ConsolCostApportionmentHelper.ContainersSetupHasChanged(paymentBases, consol))
					{
						return false;
					}

					var containerResultsFromBasis = ConsolCostApportionmentHelper.GetContainerResultsFromBasis(paymentBases);

					var totalAmount = containerResultsFromBasis.Sum(c => c.Result);
					if (totalAmount == 0)
					{
						return false;
					}

					foreach (var containerResult in containerResultsFromBasis)
					{
						ZDecimal containerShare;
						if (TryGetContainerShare(consol, Shipment, containerResult.ContainerNumber, containerResult.ContainerTypeCode, out containerShare))
						{
							cost += containerShare * containerResult.Result / totalAmount;
							result = true;
						}
						else
						{
							return result;
						}
					}
				}

				return result;
			}

			bool TryGetContainerShare(ForwardingConsol consol, ForwardingShipment shipment, ZString containerNumber, ZString containerTypeCode, out ZDecimal containerShare)
			{
				bool result = false;
				containerShare = 0m;
				ForwardingContainer[] containers = null;

				containers = consol.Containers
					.Cast<ForwardingContainer>()
					.Where(c => c.JC_ContainerNum == containerNumber
						&& c.RefContainer != null
						&& c.RefContainer.MatchesClass(containerTypeCode))
					.ToArray();

				if (containers.Length > 0)
				{
					bool isShipmentSharingContainers = shipment.Containers.Any(sc => containers.Any(cc => cc.PK == sc.PK));
					result = true;

					if (isShipmentSharingContainers)
					{
						containerShare = containers.Sum(x => FreightRatingHelper.GetShipmentShareInContainer(shipment, x.PK));
					}
				}

				return result;
			}

			#region ISalesRepDefaultingFromControllingCustomer Members

			ZBool ISalesRepDefaultingFromControllingCustomer.IsAllowedToDefaultSalesRepFromControllingCustomer => FreightDataRegistry.Instance.ControllingCustomerUseSalesRep.Value;

			void ISalesRepDefaultingFromControllingCustomer.NotifyControllingCustomerChanged(EventHandler onChangedAction)
			{
				if (onChangedAction == null)
				{
					Shipment.ControllingCustomerAddress.E2_OA_AddressInfo.ValueChanged -= onControllingCustomerAddressChangedAction;
					onControllingCustomerAddressChangedAction = null;
				}
				else if (onControllingCustomerAddressChangedAction == null)
				{
					Shipment.ControllingCustomerAddress.E2_OA_AddressInfo.ValueChanged += onChangedAction;
					onControllingCustomerAddressChangedAction = onChangedAction;
				}
				else if (onControllingCustomerAddressChangedAction != onChangedAction)
				{
					var developerMessage = BuildDeveloperErrorMessage(onControllingCustomerAddressChangedAction, onChangedAction);
					ErrorReporter.ReportOnce("NotifyControllingCustomerChangedCalledWithDifferentDelegates_4", $@"NotifyControllingCustomerChanged can't be called with different delegates. All such calls are ignored.
{developerMessage}");
				}
			}
			EventHandler onControllingCustomerAddressChangedAction;

			string BuildDeveloperErrorMessage(EventHandler onControllingCustomerAddressChangedAction, EventHandler onChangedAction)
			{
				var errorMessageBuilder = new ZStringBuilder();
				errorMessageBuilder.AppendLine();
				errorMessageBuilder.AppendLine(FormattableString.Invariant($"Current Login Company PK: {Env.CurrentCompanyPK}")); // Developer Error Message
				errorMessageBuilder.AppendLine(FormattableString.Invariant($"Current Login Company Code: {Env.CurrentCompany.Code}")); // Developer Error Message
				errorMessageBuilder.AppendLine(FormattableString.Invariant($"Shipment PK: {Shipment.PK}")); // Developer Error Message
				errorMessageBuilder.AppendLine(FormattableString.Invariant($"Shipment Number: {Shipment.JS_UniqueConsignRef}")); // Developer Error Message
				errorMessageBuilder.AppendLine(FormattableString.Invariant($"Shipment Factory: {Shipment.Factory._Instance}")); // Developer Error Message
				errorMessageBuilder.AppendLine(FormattableString.Invariant($"Shipment Business Contexts: {CriticalValidationInfoExtensions.GetBusinessContextForBizObj(Shipment)}")); // Developer Error Message
				errorMessageBuilder.AppendLine();
				if (onControllingCustomerAddressChangedAction.Target is JobHeader subscribedJob)
				{
					errorMessageBuilder.AppendLine(FormattableString.Invariant($"Subscribed Job PK: {subscribedJob.PK}")); // Developer Error Message
					errorMessageBuilder.AppendLine(FormattableString.Invariant($"Subscribed Job Factory: {subscribedJob.Factory._Instance}")); // Developer Error Message
					errorMessageBuilder.AppendLine(FormattableString.Invariant($"Subscribed Job Is In Database: {subscribedJob.IsInDatabase}")); // Developer Error Message
					errorMessageBuilder.AppendLine(FormattableString.Invariant($"Subscribed Job Is Deleted: {subscribedJob.IsDeleted}")); // Developer Error Message
					errorMessageBuilder.AppendLine(FormattableString.Invariant($"Subscribed Job Business Contexts: {CriticalValidationInfoExtensions.GetBusinessContextForBizObj(subscribedJob)}")); // Developer Error Message
					if (!subscribedJob.IsDeleted)
					{
						errorMessageBuilder.AppendLine(FormattableString.Invariant($"Subscribed Job Number: {subscribedJob.JH_JobNum}")); // Developer Error Message
						errorMessageBuilder.AppendLine(FormattableString.Invariant($"Subscribed Job Company PK: {subscribedJob.JH_GC}")); // Developer Error Message
						errorMessageBuilder.AppendLine(FormattableString.Invariant($"Subscribed Job Company Code: {subscribedJob.Company?.GC_Code ?? ZString.Empty}")); // Developer Error Message
						errorMessageBuilder.AppendLine(FormattableString.Invariant($"Subscribed Job Parent Table Code and ID : {subscribedJob.JH_ParentTableCode} - [{subscribedJob.JH_ParentID}]")); // Developer Error Message
					}
					errorMessageBuilder.AppendLine(FormattableString.Invariant($"Subscribed Job Header Constructor Stacktrace : {subscribedJob.ConstructorStackTrace ?? (NoResString)"Not Collected"}")); // Developer Error Message
				}
				else
				{
					errorMessageBuilder.AppendLine(FormattableString.Invariant($"onControllingCustomerAddressChangedAction Taget Type: {onControllingCustomerAddressChangedAction.Target.GetType()}")); // Developer Error Message
				}
				errorMessageBuilder.AppendLine();
				if (onChangedAction.Target is JobHeader subsequentJob)
				{
					errorMessageBuilder.AppendLine(FormattableString.Invariant($"Subsequent Job PK: {subsequentJob.PK}")); // Developer Error Message
					errorMessageBuilder.AppendLine(FormattableString.Invariant($"Subsequent Job Factory: {subsequentJob.Factory._Instance}")); // Developer Error Message
					errorMessageBuilder.AppendLine(FormattableString.Invariant($"Subsequent Job Is In Database: {subsequentJob.IsInDatabase}")); // Developer Error Message
					errorMessageBuilder.AppendLine(FormattableString.Invariant($"Subsequent Job Is Deleted: {subsequentJob.IsDeleted}")); // Developer Error Message
					errorMessageBuilder.AppendLine(FormattableString.Invariant($"Subsequent Job Business Contexts: {CriticalValidationInfoExtensions.GetBusinessContextForBizObj(subsequentJob)}")); // Developer Error Message
					if (!subsequentJob.IsDeleted)
					{
						errorMessageBuilder.AppendLine(FormattableString.Invariant($"Subsequent Job Number: {subsequentJob.JH_JobNum}")); // Developer Error Message
						errorMessageBuilder.AppendLine(FormattableString.Invariant($"Subsequent Job Company PK: {subsequentJob.JH_GC}")); // Developer Error Message
						errorMessageBuilder.AppendLine(FormattableString.Invariant($"Subsequent Job Company Code: {subsequentJob.Company?.GC_Code ?? ZString.Empty}")); // Developer Error Message
						errorMessageBuilder.AppendLine(FormattableString.Invariant($"Subsequent Job Parent Table Code and ID : {subsequentJob.JH_ParentTableCode} - [{subsequentJob.JH_ParentID}]")); // Developer Error Message
					}
					errorMessageBuilder.AppendLine(FormattableString.Invariant($"Subsequent Job Header Constructor Stacktrace : {subsequentJob.ConstructorStackTrace ?? (NoResString)"Not Collected"}")); // Developer Error Message
				}
				else
				{
					errorMessageBuilder.AppendLine(FormattableString.Invariant($"onChangedAction Taget Type: {onChangedAction.Target.GetType()}")); // Developer Error Message
				}
				return errorMessageBuilder.ToString();
			}

			#endregion
		}

		#endregion

		#region CommonShipment Members

		public override bool HasDangerousGoodsSubstancesForbiddenOnPassengerAircraft()
		{
			return OuterPackLines.OfType<ForwardingPackLine>()
				.Any(packline => packline.UNDGs.OfType<ForwardingUNDGDataItem>()
					.Any(undg => IsSubstanceForbiddenOnAir(undg)));
		}

		static bool IsSubstanceForbiddenOnAir(ForwardingUNDGDataItem undg)
		{
			if (undg.UNDGSubstance == null || undg.UNDGSubstance.DG_Standard != UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA)
			{
				return false;
			}
			undg.Validation.ValidateDI_DG();
			if (undg.DI_DGInfo.HasErrors())
			{
				return true;
			}
			undg.Validation.ValidateDI_DGWeight();
			if (undg.DI_DGWeightInfo.HasErrors())
			{
				return true;
			}
			undg.Validation.ValidateDI_DGVolume();
			return undg.DI_DGVolumeInfo.HasErrors();
		}

		#endregion

		#region ICustomsJobInfoProvider Members

		ICustomsJobInfo ICustomsJobInfoProvider.GetCustomsJobInfo(ZGuid companyPK)
		{
			foreach (BusinessObject declaration in Declarations)
			{
				ZGuid branchPK = (ZGuid)declaration[JobDeclarationSchema.JE_GB];

				GlbBranch branch = Factory.Load<GlbBranch>(branchPK);

				if (branch != null && branch.GB_GC == companyPK)
				{
					return (ICustomsJobInfo)declaration;
				}
			}
			return null;
		}

		#endregion

		#region AdditionalJobsToShowChargesFor

		protected override IJobInvoicingPlugIn[] AdditionalJobsToShowChargesForCore
		{
			get
			{
				IJobInvoicingPlugIn[] result;

				if (IsInDatabase)
				{
					var relatedOrders = new List<IJobInvoicingPlugIn>(AttachedWarehouseOrders.Count);

					foreach (IWhsOrder order in AttachedWarehouseOrders)
					{
						if (order.IncludeInRelatedShipmentCharges)
						{
							relatedOrders.Add((IJobInvoicingPlugIn)order);
						}
					}

					result = relatedOrders.ToArray();
				}
				else
				{
					result = Array.Empty<IJobInvoicingPlugIn>();
				}

				return result;
			}
		}

		#endregion

		#region ICancellable Members

		public override string CanCancel()
		{
			var result = base.CanCancel();
			if (!string.IsNullOrEmpty(result))
			{
				return result;
			}

			if (AttachedOrders.Count > 0 || AttachedWarehouseOrders.Count > 0)
			{
				return Res.GetString("4202b4fb-dfc6-486c-bd69-f5fd24d9f944", "You cannot deactivate {0} since it has orders attached.", HumanReadableName);
			}

			if (IsHighVolumeLowValue && HVLVConsignments.Any())
			{
				return Res.GetString("f163e068-cf0f-4657-a570-f563c2beb0c5", "You cannot deactivate {0} since it has active consignments.", HumanReadableName);
			}

			if (!checkingCanCancelRelatedObjects)
			{
				checkingCanCancelRelatedObjects = true;
				try
				{
					foreach (var declaration in Declarations)
					{
						var canCancelDeclaration = declaration.CanCancel();
						if (!string.IsNullOrEmpty(canCancelDeclaration))
						{
							var branch = declaration.JE_GB.IsValid ? Factory.Load<GlbBranch>(declaration.JE_GB) : null;
							var company = branch != null ? branch.Company : null;
							var country = company != null ? company.GC_RN_NKCountryCode : GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
							return Res.GetString(
								"26A95EE0-8946-4A7E-9776-C9572FE141D4",
								"This record cannot be deactivated as one of its related records cannot be deactivated due to the following reason.") +
								System.Environment.NewLine + declaration.HumanReadableName + " (" + country + "): " + canCancelDeclaration;
						}
					}

					var canCancel = RelatedCancellableDataSupporter.CanCancel(this);

					if (!string.IsNullOrWhiteSpace(canCancel))
					{
						return canCancel;
					}
				}
				finally
				{
					checkingCanCancelRelatedObjects = false;
				}
			}

			return null;
		}
		bool checkingCanCancelRelatedObjects;

		public override void DeactivateActiveBusinessObjectCollections()
		{
			base.DeactivateActiveBusinessObjectCollections();
			if (attachedOrders != null)
			{
				attachedOrders.Deactivate();
			}

			if (attachedWarehouseOrders != null)
			{
				attachedWarehouseOrders.Deactivate();
			}
		}

		#endregion

		#region ICustomFieldProvider Members

		CustomBusinessObject ICustomFieldProvider.GetCustomBusinessObject(bool shouldRefresh)
		{
			customBusinessObject = shouldRefresh ? null : customBusinessObject;
			return CustomBusinessObject;
		}

		CustomBusinessObject CustomBusinessObject
		{
			get
			{
				var customBusinessObjectProcessTaskTemplateLoader = (IProcessTaskTemplateLoader)Activator.CreateInstance(ObjectFactory.GetType<IProcessTaskTemplateLoader>(), Factory);
				var templateMatches = customBusinessObjectProcessTaskTemplateLoader.FindMatches(this);

				var currentMatchKeys = templateMatches.Matches.Select(s => s.Identifier).ToArray();

				if (lastMatches == null || !lastMatches.SequenceEqual(currentMatchKeys))
				{
					customBusinessObject = null;
					lastMatches = currentMatchKeys;
				}

				if (customBusinessObject == null)
				{
					var properties = new UserDefinedPropertyCollection(this);

					var customFieldsDescriptor = new JobDocsAndCartageCustomFieldsDescriptor();
					foreach (var customFieldInfo in customFieldsDescriptor.ActiveCustomFieldsInfos)
					{
						properties.Add(customFieldsDescriptor.BindTo("DocsAndCartage", customFieldInfo), customFieldInfo.Caption); // binding path to custom field
					}

					properties.Add(templateMatches);

					customBusinessObject = new PhaseSecuritySupportableCustomBusinessObject(Factory, this, properties, this);

					InitialisePhaseDependantCustomBusinessObjectValidation(templateMatches != null);
				}

				return customBusinessObject;
			}
		}
		CustomBusinessObject customBusinessObject;
		ZGuid[] lastMatches;

		#endregion

		#region MatchShipmentTotalsOnPackLinesDetailForm

		public bool MatchShipmentTotalsOnPackLinesDetailForm
		{
			get
			{
				if (matchShipmentTotals == null)
				{
					matchShipmentTotals = true;
				}
				return (bool)matchShipmentTotals;
			}
			set { matchShipmentTotals = value; }
		}

		[ThreadStatic]
		static bool? matchShipmentTotals;

		#endregion

		#region ITemplateReversible Members

		protected override void ReverseCore()
		{
			base.ReverseCore();

			ClearPickupValues();
			ClearDeliveryValues();

			var origin = JS_RL_NKOrigin;
			var destination = JS_RL_NKDestination;

			ConsignorDocumentaryAddressChanged();
			ConsigneeDocumentaryAddressChanged();

			SetDeliveryCFS();
			SetPickupCFS();

			JS_RL_NKOrigin = origin;
			JS_RL_NKDestination = destination;
		}

		void ClearPickupValues()
		{
			JS_OH_ExportBroker = ZGuid.Empty;
			DocsAndCartage.JP_OA_PickupCartageCoAddr = ZGuid.Empty;
			DocsAndCartage.JP_OA_PickupCartageCoAddr_ZAddress.SetOrgWithoutSettingDefaultAddress(ZGuid.Empty);
			JS_OA_ExportReceivingDepot = ZGuid.Empty;
			JS_OA_ExportReceivingDepot_ZAddress.SetOrgWithoutSettingDefaultAddress(ZGuid.Empty);

			JS_InterimReceipt = ZString.Empty;
			JS_BookingReference = ZString.Empty;
			DocsAndCartage.JP_PickupLabourCharge = ZDecimal.Zero;
			DocsAndCartage.JP_PickupTruckWaitCharge = ZDecimal.Zero;
		}

		void ClearDeliveryValues()
		{
			JS_OH_ImportBroker = ZGuid.Empty;
			DocsAndCartage.JP_OA_DeliveryCartageCoAddr = ZGuid.Empty;
			DocsAndCartage.JP_OA_DeliveryCartageCoAddr_ZAddress.SetOrgWithoutSettingDefaultAddress(ZGuid.Empty);
			JS_OH_DeliveryAgent = ZGuid.Empty;
			JS_OA_ImportReleaseDepot = ZGuid.Empty;
			JS_OA_ImportReleaseDepot_ZAddress.SetOrgWithoutSettingDefaultAddress(ZGuid.Empty);

			DocsAndCartage.JP_DeliveryLabourCharge = ZDecimal.Zero;
			DocsAndCartage.JP_DeliveryTruckWaitCharge = ZDecimal.Zero;
			DocsAndCartage.JP_LCLAirStorageCharge = ZDecimal.Zero;
			DocsAndCartage.JP_LCLAirStorageDaysOrHours = ZByte.Zero;
		}

		#endregion

		#region Read Only Security

		protected virtual bool GetReadOnlySecurity(PropertyDescriptor property)
		{
			bool result = false;
			if (property.Name != ForwardingShipment.Schema.JS_Phase)
			{
				result = IsPropertyReadOnlyDueToPhase(property.Name);
			}

			return result || MetaData.GetReadOnlyExcludingMethodProvider(this, property);
		}

		protected override bool IsReadOnlyDueToPhaseCore
		{
			get { return ReadOnly || !PhaseResolver.IsEditingAllowed; }
		}

		protected override bool IsPropertyReadOnlyDueToPhaseCore(ZString propertyName)
		{
			return IsReadOnlyDueToPhase || PhaseResolver.IsPropertyReadOnly(propertyName);
		}

		internal IPhaseSecurityResolver PhaseResolver
		{
			get { return phaseResolver ?? (phaseResolver = GetPhaseSecurityResolver()); }
		}
		IPhaseSecurityResolver phaseResolver;

		protected virtual IPhaseSecurityResolver GetPhaseSecurityResolver()
		{
			return new ShipmentPhaseSecurityResolver(this);
		}

		#region Child Objects

		public override void RegisterEditableChildObject(IBusiness child)
		{
			base.RegisterEditableChildObject(child);
			if (child != null && IsReadOnlyDueToPhase && !ShouldExcludeFromPhaseSecurity(child))
			{
				child.IncrementReadOnlyIncludingChildren();
			}
		}

		protected bool ShouldExcludeFromPhaseSecurity(IBusiness child)
		{
			return (child is JobHeader);
		}

		protected override void RegisterEditableChildObject(IBusiness child, ZString childName)
		{
			RegisterEditableChildObject(child);
			if (!IsReadOnlyDueToPhase)
			{
				PhaseResolver.RegisterEditableChild(child, childName);
				if (IsPropertyReadOnlyDueToPhase(childName))
				{
					child.IncrementReadOnlyIncludingChildren();
				}
			}
		}

		public virtual bool IsChildPropertyReadOnlyDueToPhase(ZGuid childPK, ZString childPropertyName)
		{
			return IsReadOnlyDueToPhase || PhaseResolver.IsChildPropertyReadOnly(childPK, childPropertyName);
		}

		#endregion

		#endregion

		#region IDtbBookingParent Members

		ZString IDtbBookingParent.JobTypeDescription
		{
			get { return HumanReadableNameWithoutID; }
		}

		ZString IDtbBookingParent.JobType
		{
			get { return ((IWorkflowProvider)this).WorkflowType; }
		}

		DtbBookingDirection[] IDtbBookingParent.GetSupportedDirections()
		{
			return new DtbBookingDirection[] { DtbBookingDirection.PIC, DtbBookingDirection.DLV };
		}

		IJobInvoicingPlugIn IDtbBookingParent.InvoicingJob
		{
			get { return this; }
		}

		void IDtbBookingParent.TransportBookingCreatedOrUpdated(IEnumerable<IDtbBooking> bookings)
		{
			if (HaveJobCO2e && FreightDataRegistry.Instance.EnablePrePostCarriageCalculation.Value)
			{
				this.UpdateCO2eStatusToNotCurrent();
				ZExceptionReporting.ProcessWithSaveExceptionHandling(Factory.Save, null, true);
			}
		}

		ZBool IDtbBookingParent.IsSupportsDirectSchedule
		{
			get { return false; }
		}

		bool IDtbBookingParent.RequiresMultiContainerBooking => true;

		public ZQuery TransportBookingTemplateFilters => new ZQuery();

		public bool CanCreateTransportBooking => true;

		public ZGuid BookingParentPK => PK;

		public string BookingParentTablePrefix => TablePrefix;

		public (bool isShouldShow, string caption, string message, string confirmation) GetExtendingConfirmMessageBeforeCreateTransportBooking()
		{
			string caption = null;
			string message = null;
			string confirmation = null;
			bool isShouldShow = false;
			if (DeliveryWarehouseDispatchConsignments.Length != 0 || PickupWarehouseDispatchConsignments.Length != 0)
			{
				caption = Res.GetString("57cdebb7-494a-4530-ad7f-42d42dd694ff", "Warning");
				message = Res.GetString("33759783-d6e4-44d9-8acb-5532881b5534", "There are Transit Warehouse Dispatch Consignments attached to this Shipment. Creating Transport Bookings from the Shipment may delete all Transport Bookings created via the Dispatch Consignments. We suggest creating/updating TBs from the Planning Portal to ensure the physical structure is sent to the Transport Company. Are you sure you want to continue creating the TB directly from the Shipment?");
				confirmation = Res.GetString("63f1f838-1d65-4144-b4c3-1f19522dce3a", "Yes");
				isShouldShow = true;
			}
			return (isShouldShow, caption, message, confirmation);
		}

		#endregion

		#region IPackLineSynchroniseProvider

		IPackLineSynchronise IPackLineSynchroniseProvider.PackLineSynchronise
		{
			get { return PackLineSynchroniser; }
		}

		#endregion

		#region IControllerIDProvider Members

		Guid IControllerIDProvider.BusinessObjectPK
		{
			get { return PK.ToGuid(); }
		}

		ControllerID IControllerIDProvider.ControllerID
		{
			get { return ControllerIDs.JobShipment; }
		}

		#endregion

		#region IRelatedJob Members

		ZString IRelatedJob.JobDescription
		{
			get { return HumanReadableNameWithoutID; }
		}

		ZString IRelatedJob.JobNumber
		{
			get { return JS_UniqueConsignRef; }
		}

		ZString IRelatedJob.JobStatus
		{
			get { return JS_ShipmentStatus; }
		}

		#endregion

		#region ICusInBondParent Members

		ZGuid ICusInBondParent.GetDeclarationPK(ZGuid companyPK)
		{
			var result = ZGuid.Empty;
			var declaration = GetDeclarationFor(companyPK);
			if (declaration != null)
			{
				result = declaration.PK;
			}
			return result;
		}

		ZString ICusInBondParent.ParentType
		{
			get { return Res.GetString("5d6d8c4f-7471-4ace-9a6f-a821ade1f0e9", "Shipment"); }
		}

		string ICusInBondParent.TablePrefix
		{
			get { return TablePrefix; }
		}

		void ICusInBondParent.PopulateJobNumberIfNeeded()
		{
			PopulateBillAndShipmentNumberIfNeeded();
		}

		ZString ICusInBondParent.JobNumber
		{
			get { return JobNumber; }
		}

		ZString ICusInBondParent.HouseBill
		{
			get { return JS_HouseBill; }
		}

		event EventHandler ICusInBondParent.VisibilityChanged
		{
			add { }
			remove { }
		}

		bool ICusInBondParent.IsVisible
		{
			get { return true; }
		}

		bool ICusInBondParent.IsInternalBrokerage
		{
			get
			{
				var brokerPK = GetBrokerPK();
				bool result = false;
				if (!brokerPK.IsEmpty)
				{
					foreach (GlbBranch branch in GlbCompany.CurrentCompany.Branches)
					{
						if (branch.GB_OH_OrgProxy == brokerPK)
						{
							result = true;
							break;
						}
					}
				}

				return result;
			}
		}

		ZGuid GetBrokerPK()
		{
			var result = ZGuid.Empty;
			if (this.IsExport())
			{
				result = JS_OH_ExportBroker;
			}
			else if (this.IsImport())
			{
				result = JS_OH_ImportBroker;
			}

			return result;
		}

		#endregion

		#region ITransitWarehouseParent Members

		void ITransitWarehouseParent.AttachPackages(ITransitPackage[] packagesToAttach)
		{
			var existingPackageIds = OuterPackLines.Cast<ForwardingPackLine>().SelectMany(t => t.PkgPackageCollection.Select(pkg => pkg.KP_PackageID));
			var groups = packagesToAttach.Where(p => !existingPackageIds.Contains(p.PackageID)).GroupBy(GetPackageIdentifier).ToList();

			var groupSum = groups.Sum(CombineIntoPackline);
			if (groupSum > 0)
			{
				var parameters = new Dictionary<string, string>()
				{
					[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.DeclarationID] = $"{groupSum}x Blind Transit Package(s) attached to Shipment."
				};
				Logs.AddATCEvent(IsInDatabase, string.Empty, parameters);
			}

			if (groups.Count > 0)
			{
				// if there are packages added from plugin but outers are matching then outerpacklines may not have any changes.
				// but factory will require a save in this case to save changes happened in the plugin
				HasChanges = true;
			}
		}

		dynamic GetPackageIdentifier(ITransitPackage p)
		{
			return new
			{
				RCN = Constants.CountryCodes.IsFranceOrTerritory(ExportReceivingDepot?.OA_RN_NKCountryCode) ? p.RCN : ZString.Empty,
				IsHighRisk = AviationSecurity.SupplyChainSecurityConfiguration.IsHighRiskApplicable && p.IsHighRisk
			};
		}

		int CombineIntoPackline(IEnumerable<ITransitPackage> packagesGroup)
		{
			var packLine = OuterPackLines.AddNew();
			var groupCount = 0;

			var dic = new Dictionary<ZGuid, ITransitPackage>();
			if (FreightConfigurationRegistry.Instance.EnableTWPackageLinking.Value)
			{
				packLine.PkgPackageCollection.AddRange(packagesGroup.Select(p => p.TransitPackage).Cast<PkgPackage>().ToList());
				dic = packagesGroup.ToDictionary(k => ((PkgPackage)k.TransitPackage).PK, v => v);
			}
			else
			{
				var parentPackageJob = GetParentPackageJob();
				foreach (var item in packagesGroup)
				{
					var pkgPackageBO = packLine.PkgPackageCollection.AddNew();
					PopulateFromReadonlyPackage(pkgPackageBO, (PkgPackage)item.TransitPackage, parentPackageJob.PK);
					PopulatePackageScreeningMethod(pkgPackageBO, item);
					dic.Add(pkgPackageBO.PK, item);
				}
			}

			if (packLine.PkgPackageCollection.Count == 0)
			{
				OuterPackLines.Remove(packLine);
			}
			else
			{
				using (SetIsMarkingPackLinesAsSecuredAllowed())
				{
					packLine.UpdateAndSplitPacklineFromPkgPackageCollection(
						p =>
						{
							if (p is ForwardingPackLine item && !item.PkgPackageCollection.IsNullOrEmpty() && dic.TryGetValue(item.PkgPackageCollection[0].PK, out var package))
							{
								item.JL_LastKnownTransitWarehouseStatus = FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Received;
								item.JL_OA_LastKnownTransitWarehouseAddress = (this as ITransitWarehouseParent).GetPickupCFSOrgAddressPK();
								item.JL_LastKnownTransitWarehouseStatusDateTime = packagesGroup.Max(g => g.UnloadTime);
							}
						},
						p =>
						{
							if (p is ForwardingPackLine item && dic.TryGetValue(item.PkgPackageCollection[0].PK, out var package))
							{
								PopulatePANAndERC(item, package);
								item.JL_RefNumber = package.UnitType == FreightConstants.TransitPackageStateUnitType.Overpack && packagesGroup.Count() == 1 ? package.PackageID : ZString.Empty;
								item.JL_OriginTransitWarehouseStatus = FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Surplus;
								item.JL_IsHighRisk = AviationSecurity.SupplyChainSecurityConfiguration.IsHighRiskApplicable && package.IsHighRisk;
								item.BlindPackageAttached = true;
								groupCount++;
							}
						});
				}
			}

			return groupCount;
		}

		void PopulatePANAndERC(ForwardingPackLine packLine, ITransitPackage package)
		{
			if (!package.RCN.IsEmpty)
			{
				var ercNumber = packLine.AdditionalReferenceNumbers.AddNew();
				ercNumber.CE_EntryNum = package.RCN;
				ercNumber.CE_EntryType = ForwardingPackingLineAdditionalReferenceNumberTypes.Codes.ERC;
				if (ExportReceivingDepot != null)
				{
					ercNumber.CE_RN_NKCountryCode = ExportReceivingDepot.OA_RN_NKCountryCode;
				}

				ercNumber.CE_EntryIsSystemGenerated = true;
			}

			var panNumbers = package.Numbers?.Where(c => !c.CE_EntryNum.IsEmpty);
			if (panNumbers != null && panNumbers.Any())
			{
				var exportRefNumberHasBeenUpdated = false;
				var pickupCFSCountry = ExportReceivingDepot?.OA_RN_NKCountryCode ?? ZString.Empty;

				foreach (var pan in panNumbers)
				{
					var newPANNumber = (ICusEntryNumber)packLine.PortReferences.AddNew();
					newPANNumber.CE_EntryNum = pan.CE_EntryNum;
					newPANNumber.CE_EntryType = ForwardingPackingLineAdditionalReferenceNumberTypes.Codes.PAN;
					newPANNumber.CE_RN_NKCountryCode = pan.CE_RN_NKCountryCode;
					newPANNumber.CE_EntryStatus = pan.CE_EntryStatus;
					newPANNumber.CE_Category = CargoWise.Definitions.TransitWarehouseReferenceCategories.Codes.PortReference;
					newPANNumber.CE_EntryIsSystemGenerated = true;

					if (!exportRefNumberHasBeenUpdated && pickupCFSCountry == newPANNumber.CE_RN_NKCountryCode && Constants.CountryCodes.IsFranceOrTerritory(pickupCFSCountry))
					{
						packLine.JL_ExportRefNumber = newPANNumber.CE_EntryNum;
						exportRefNumberHasBeenUpdated = true;
					}
				}
			}
		}

		internal ForwardingPackageJob GetParentPackageJob()
		{
			var result = Factory.LoadTop1<ForwardingPackageJob>(new ZQuery(PkgPackageJobSchema.KJ_ParentID, PK)); // there should only ever be one.

			if (result == null)
			{
				result = Factory.New<ForwardingPackageJob>();
				result.KJ_JobID = JobNumber;
				result.KJ_ParentID = PK;
				result.KJ_ParentTableCode = TablePrefix;
			}
			return result;
		}

		void PopulateUNDGFromUNDGItem(UNDGDataItem dg, IUNDGDataItem item)
		{
			dg.DI_DG = item.DI_DG;
			dg.DI_IMOClass = item.DI_IMOClass;
			dg.DI_DGFlashPoint = item.DI_DGFlashPoint;
			dg.DI_OC_DGContact = item.DI_OC_DGContact;
			dg.DI_MPMarinePollutant = item.DI_MPMarinePollutant;
			dg.DI_TechnicalName = item.DI_TechnicalName;
			dg.DI_DGVolume = item.DI_DGVolume;
			dg.DI_UnitOfVolume = item.DI_UnitOfVolume;
			dg.DI_DGWeight = item.DI_DGWeight;
			dg.DI_UnitOfWeight = item.DI_UnitOfWeight;
			dg.DI_PackageCount = item.DI_PackageCount;
			dg.DI_F3_NKPackType = item.DI_F3_NKPackType;
			dg.DI_HasOverpack = item.DI_HasOverpack;
			dg.DI_OverpackID = item.DI_OverpackID;
			dg.DI_RadioactiveLabelCategory = item.DI_RadioactiveLabelCategory;
			dg.DI_RadioactiveTransportIndex = item.DI_RadioactiveTransportIndex;
			dg.DI_RadioactiveMaximumActivity = item.DI_RadioactiveMaximumActivity;
			dg.DI_RadioactiveMaximumActivityUnit = item.DI_RadioactiveMaximumActivityUnit;
			dg.DI_RadionuclideElement = item.DI_RadionuclideElement;
			dg.DI_RadionuclideElementSuffix = item.DI_RadionuclideElementSuffix;
			dg.DI_IsLimitedQuantity = item.DI_IsLimitedQuantity;
			dg.DI_IsExclusiveUse = item.DI_IsExclusiveUse;
			dg.DI_IsFissileExcepted = item.DI_IsFissileExcepted;
			dg.DI_IsHighwayRouteControlledQuantity = item.DI_IsHighwayRouteControlledQuantity;
			dg.DI_MaterialFormDescription = item.DI_MaterialFormDescription;
			dg.DI_PackingInstructionSection = item.DI_PackingInstructionSection;
		}

		void PopulatePackageScreenings(PkgPackageScreening pkgPackageScreeningBO, PkgPackageScreening pkgPackageScreening)
		{
			pkgPackageScreeningBO.KPS_GS_NKScreenedBy = pkgPackageScreening.KPS_GS_NKScreenedBy;
			pkgPackageScreeningBO.KPS_Time = pkgPackageScreening.KPS_Time;
			pkgPackageScreeningBO.KPS_Method = pkgPackageScreening.KPS_Method;
			pkgPackageScreeningBO.KPS_Passed = pkgPackageScreening.KPS_Passed;
			pkgPackageScreeningBO.KPS_FailureReason = pkgPackageScreening.KPS_FailureReason;
			pkgPackageScreeningBO.KPS_IsScreenedViaParent = pkgPackageScreening.KPS_IsScreenedViaParent;
		}

		void PopulateFromReadonlyPackage(ForwardingPackage pkgPackageBO, PkgPackage package, ZGuid parentPackageJob, bool isOuter = true)
		{
			pkgPackageBO.KP_F3_NKPackType = package.KP_F3_NKPackType;
			pkgPackageBO.KP_Sequence = 1;
			pkgPackageBO.KP_PackageQty = package.KP_PackageQty;
			pkgPackageBO.KP_RequiredTemperatureMaximum = package.KP_RequiredTemperatureMaximum;
			pkgPackageBO.KP_RequiredTemperatureMinimum = package.KP_RequiredTemperatureMinimum;
			pkgPackageBO.KP_RequiresTemperatureControl = package.KP_RequiresTemperatureControl;
			pkgPackageBO.KP_GoodsDescription = package.KP_GoodsDescription;
			pkgPackageBO.KP_MarksAndNumbers = package.KP_MarksAndNumbers;
			pkgPackageBO.KP_Height = package.KP_Height;
			pkgPackageBO.KP_Width = package.KP_Width;
			pkgPackageBO.KP_Length = package.KP_Length;
			pkgPackageBO.KP_VolumeUQ = package.KP_VolumeUQ;
			pkgPackageBO.KP_WeightUQ = package.KP_WeightUQ;
			pkgPackageBO.KP_DimensionUQ = package.KP_DimensionUQ;
			pkgPackageBO.KP_Volume = package.KP_Volume;
			pkgPackageBO.KP_Weight = package.KP_Weight;
			pkgPackageBO.KP_PackageID = package.KP_PackageID;
			pkgPackageBO.KP_RequiredTemperatureUnit = package.KP_RequiredTemperatureUnit;
			pkgPackageBO.KP_RH_NKCommodityCode = package.KP_RH_NKCommodityCode;
			pkgPackageBO.KP_HSCode = package.KP_HSCode;
			pkgPackageBO.KP_IsDamaged = package.KP_IsDamaged;
			pkgPackageBO.KP_DamagedReason = package.KP_DamagedReason;
			pkgPackageBO.KP_KJ_ParentPackageJob = parentPackageJob;
			pkgPackageBO.KP_ExternalReference = package.KP_ExternalReference;

			(package.UNDGDataItems ?? Enumerable.Empty<IUNDGDataItem>()).ForEach(item => PopulateUNDGFromUNDGItem(pkgPackageBO.UNDGs.AddNew(), item));

			if (isOuter)
			{
				package.PackageHandlingUnitHandlingUnitDivots.ForEach(divot => CreateInnerPackage(divot, pkgPackageBO, parentPackageJob));
			}
		}

		static void PopulatePackageScreeningMethod(ForwardingPackage pkgPackageBO, ITransitPackage transitPackage)
		{
			pkgPackageBO.SetIsHighRisk(transitPackage.IsHighRisk);

			var (screeningMethod, aviationSecurityAdditionalInspectionType) = transitPackage.GetScreeningMethod();
			if (screeningMethod.HasValue)
			{
				pkgPackageBO.SetScreeningMethod(screeningMethod.Value);
			}
			if (aviationSecurityAdditionalInspectionType.HasValue)
			{
				pkgPackageBO.SetAdditionalScreeningMethod(aviationSecurityAdditionalInspectionType.Value);
			}
		}

		void CreateInnerPackage(PkgPackageHandlingUnitDivot divot, ForwardingPackage outerPackage, ZGuid parentPackageJob)
		{
			if (divot?.Package == null)
			{
				return;
			}

			var innerPackage = Factory.New<ForwardingPackage>();
			PopulateFromReadonlyPackage(innerPackage, divot.Package, parentPackageJob, false);
			var transitPackage = Factory.LoadFromUniqueKey<IWhsItemPackageState>(WhsItemPackageStateSchema.WPS_KP_Package, divot.Package.PK) as ITransitPackage;
			if (transitPackage != null)
			{
				PopulatePackageScreeningMethod(innerPackage, transitPackage);
			}

			innerPackage.KP_KP_TopHandlingUnitPackage = divot.KPD_UnpackedTime == ZDateTimeOffset.Empty ? outerPackage.PK : ZGuid.Empty;

			var newDivot = Factory.New<ForwardingPackageHandlingUnitDivot>();
			newDivot.KPD_KP_HandlingUnit = outerPackage.PK;
			newDivot.KPD_KP_Package = innerPackage.PK;
			newDivot.KPD_PackedTime = divot.KPD_PackedTime;
			newDivot.KPD_GS_NKPackedUser = divot.KPD_GS_NKPackedUser;
			newDivot.KPD_UnpackedTime = divot.KPD_UnpackedTime;
			newDivot.KPD_GS_NKUnpackedUser = divot.KPD_GS_NKUnpackedUser;
		}

		string GetDGIdentifier(IUNDGDataItem dg)
		{
			if (dg == null)
			{
				return string.Empty;
			}

			var result = string.Join("-",
					dg.DI_DG,
					dg.DI_DGFlashPoint,
					dg.DI_IMOClass,
					dg.DI_MPMarinePollutant,
					dg.DI_IsLimitedQuantity,
					dg.DI_TechnicalName,
					dg.DI_UnitOfVolume,
					dg.DI_UnitOfWeight,
					dg.DI_F3_NKPackType,
					dg.DI_OC_DGContact);

			return result;
		}

		void ITransitWarehouseParent.RemovePackages(ITransitPackage[] packagesToRemove)
		{
			hasAnyPackageBeenDetached = false;

			var packageIdsToRemove = packagesToRemove.Select(t => t.PackageID.IsEmpty ? t.ExternalReference : t.PackageID).ToList();

			foreach (var packLine in OuterPackLines.Cast<ForwardingPackLine>().ToList())
			{
				foreach (var package in packLine.PkgPackageCollection.ToList())
				{
					if (packageIdsToRemove.Contains(package.KP_PackageID.IsEmpty ? package.KP_ExternalReference : package.KP_PackageID))
					{
						var packageJob = package.PackageJob;
						packLine.PkgPackageCollection.RemoveFromRelationship(package);

						if (FreightConfigurationRegistry.Instance.EnableTWPackageLinking.Value)
						{
							if (packageJob?.KJ_ParentTableCode.Equals(this.TablePrefix) ?? false)
							{
								package.Delete();

								if (packageJob.Packages.Count == 0)
								{
									packageJob.Delete();
								}
							}
						}
						else
						{
							package.Delete();
						}

						hasAnyPackageBeenDetached = true;

						if (packLine.PkgPackageCollection.Count == 0)
						{
							if (packLine.InnerPackLines.Count > 0)
							{
								packLine.InnerPackLines.DeleteAll();
							}
							OuterPackLines.Remove(packLine);
							packLine.Delete();
						}
					}
				}

				if (!packLine.IsDeleted)
				{
					packLine.SetQuantityWeightAndVolumeFromPackageTotals();
				}
			}

			if (hasAnyPackageBeenDetached)
			{
				// if there are packages to remove from plugin but outers are not matching then outerpacklines may not have any changes.
				// factory will require a save in this case to save changes happened in the plugin
				HasChanges = true;
			}
		}

		void ITransitWarehouseParent.SetTransportCompany(ZGuid tranpsportCompanyAddressPK)
		{
			if (!tranpsportCompanyAddressPK.IsEmpty && DocsAndCartage.JP_OA_PickupCartageCoAddr.IsEmpty)
			{
				DocsAndCartage.JP_OA_PickupCartageCoAddr = tranpsportCompanyAddressPK;
			}
		}

		string ITransitWarehouseParent.JobNumber => JobNumber;

		ZString ITransitWarehouseParent.ConsignorCompanyName => ConsignorPickupAddress.CompanyName;

		ZString ITransitWarehouseParent.ConsigneeCompanyName => ConsigneeDeliveryAddress.CompanyName;

		ZGuid ITransitWarehouseParent.GetPickupCFSOrgAddressPK()
		{
			return JS_OA_ExportReceivingDepot;
		}

		SortedList<int, ZGuid> ITransitWarehouseParent.GetOrderedCFSOrgAddressPKs()
		{
			var portsInOrder = new SortedList<int, ZGuid>();
			var departureCFSFromShipment = JS_OA_ExportReceivingDepot;
			var arrivalCFSFromShipment = JS_OA_ImportReleaseDepot;
			AddPortInOrder(portsInOrder, departureCFSFromShipment);

			var consols = Consols.ToArray<ForwardingConsol>();
			MovementLegComparer.SortMovementLegsByPorts(consols);
			for (int index = 0; index < consols.Length; index++)
			{
				var consol = consols[index];

				AddPortInOrder(portsInOrder, consol.JK_OA_PackDepotAddress);
				AddPortInOrder(portsInOrder, consol.JK_OA_UnpackDepotAddress);
			}

			AddPortInOrder(portsInOrder, arrivalCFSFromShipment);

			return portsInOrder;
		}

		void AddPortInOrder(SortedList<int, ZGuid> portsInOrder, ZGuid depotToAdd)
		{
			if (depotToAdd != ZGuid.Empty && !portsInOrder.Any(p => p.Value == depotToAdd))
			{
				portsInOrder.Add(portsInOrder.Count + 1, depotToAdd);
			}
		}

		ZString ITransitWarehouseParent.GetPreAttachingValidationMessage(ITransitPackage[] packagesToAttach)
		{
			if (!packagesToAttach.Any() && !Constants.CountryCodes.IsFranceOrTerritory(ExportReceivingDepot?.OA_RN_NKCountryCode))
			{
				return ZString.Empty;
			}

			var rcns = packagesToAttach.Where(p => !p.RCN.IsEmpty).Select(p => p.RCN).Distinct();

			var entryNumberQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
			entryNumberQuery.AddToFilter(CusEntryNumSchema.CE_ParentTable, ForwardingPackLine.Schema.TableName);
			entryNumberQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, ForwardingPackingLineAdditionalReferenceNumberTypes.Codes.ERC);
			entryNumberQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, rcns);

			var packLineQuery = new ZDBOnlyQuery(typeof(ForwardingPackLine));
			packLineQuery.AddToFilter(JobPackLinesSchema.JL_JS, SQLComparisonOperator.NotEqual, PK);
			packLineQuery.AddSubQuery(entryNumberQuery, JoinCondition.And);

			var packlines = Factory.Load<ForwardingPackLine>(packLineQuery);

			PreviousShipmentsByAttachedPackages = packlines.Select(l => l.Shipment).Distinct().ToList();

			return packlines.Any()
				? Res.GetString("73f0d196-9b13-4d7f-bbe3-74c4ffd00056", "Packages from this Receive Consignment (RCN) are already attached to another Shipment. Do you want to continue attaching the selected packages to shipment")
				: string.Empty;
		}

		public IEnumerable<ForwardingShipment> PreviousShipmentsByAttachedPackages { get; set; } = new List<ForwardingShipment>();

		ZBool ITransitWarehouseParent.CanAttachPackages(out ZString errorMessage)
		{
			if (JS_ShipmentType == Constants.ShipmentTypes.CoLoadMaster
				|| JS_ShipmentType == Constants.ShipmentTypes.BlindCoLoadMaster
				|| JS_ShipmentType == Constants.ShipmentTypes.AssemblyMaster)
			{
				errorMessage = Res.GetString("f5dea964-2840-4d6d-abaa-8ea929e7d0f3", "Attaching blind packages to master shipments is not allowed.");
				return false;
			}

			if (OuterPackLines.Cast<ForwardingPackLine>().Any(packLine => packLine.JL_LastKnownTransitWarehouseStatus != FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Received))
			{
				errorMessage = Res.GetString("79394293-d32a-4f15-9299-7ac783ec7014", "Blind packages cannot be attached to a shipment when existing pack lines have not been received in Transit Warehouse.");
				return false;
			}

			errorMessage = ZString.Empty;
			return true;
		}

		ZBool ITransitWarehouseParent.CanDetachPackages(out ZString errorMessage)
		{
			if (OuterPackLines.Cast<ForwardingPackLine>().Any(packLine => packLine.JL_LastKnownTransitWarehouseStatus == FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Dispatched))
			{
				errorMessage = Res.GetString("be0abe66-c346-43ed-ad84-57afce77b5bd", "Packages cannot be detached when existing pack lines have been dispatched from Transit Warehouse.");
				return false;
			}

			errorMessage = ZString.Empty;
			return true;
		}

		#endregion

		#region ITransitWarehouseInstructionSupporter

		OrgAddress ITransitWarehouseInstructionSupporter.PickupTransitWarehouse => ExportReceivingDepot;

		OrgAddress ITransitWarehouseInstructionSupporter.DeliveryTransitWarehouse => ImportReleaseDepot;

		ZDateTime ITransitWarehouseInstructionSupporter.PickupReceiptRequestedDate
		{
			get => JS_ExportReceivingDepotReceiptRequested;
			set => JS_ExportReceivingDepotReceiptRequested = value;
		}

		ZDateTime ITransitWarehouseInstructionSupporter.PickupDispatchRequestedDate
		{
			get => JS_ExportReceivingDepotDispatchRequested;
			set => JS_ExportReceivingDepotDispatchRequested = value;
		}

		ZDateTime ITransitWarehouseInstructionSupporter.DeliveryReceiptRequestedDate
		{
			get => JS_ImportReleaseDepotReceiptRequested;
			set => JS_ImportReleaseDepotReceiptRequested = value;
		}

		ZDateTime ITransitWarehouseInstructionSupporter.DeliveryDispatchRequestedDate
		{
			get => JS_ImportReleaseDepotDispatchRequested;
			set => JS_ImportReleaseDepotDispatchRequested = value;
		}

		ZString ITransitWarehouseInstructionSupporter.PickupDescription => Res.GetString("51956a2e-1df0-4c09-89f7-b8d1c3dcfa55", "Pickup");

		ZString ITransitWarehouseInstructionSupporter.DeliveryDescription => Res.GetString("bf180b7a-fd7e-4e4e-8ab5-aa59936a13d9", "Delivery");

		ZString ITransitWarehouseInstructionSupporter.TransitWarehouseDescription => Res.GetString("2b3b4da0-5133-4343-a3fb-b4b9310f2cca", "CFS / Transit Warehouse");

		#endregion

		#region ProcessHandlingInfo

		public ProcessHandlingInfo ProcessHandlingInfo
		{
			get { return new ForwardingShipmentProcessHandlingInfo(this); }
		}

		#endregion

		#region HVLVItems

		public bool HasHVLVItems
		{
			get { return Factory.LoadTop1<IHVLVItem>(new ZQuery(HVLVItemSchema.HVI_JS_LoadedOnShipment, PK)) != null; }
		}

		public IHVLVItemCollectionForDocument HVLVItemCollectionForDocument
		{
			get
			{
				if (hvlvItemCollectionForDocument == null)
				{
					hvlvItemCollectionForDocument = ObjectFactory.Get<IHVLVItemCollectionForDocument>(nameof(IHVLVItemCollectionForDocument), this);
					hvlvItemCollectionForDocument.Load();
				}
				return hvlvItemCollectionForDocument;
			}
		}
		IHVLVItemCollectionForDocument hvlvItemCollectionForDocument;

		[CargoWise.Macros.MacroIgnore]
		public IEnumerable<IHVLVItem> HVLVItems
		{
			get { return Factory.Load<IHVLVItem>(HVLVItemQuery); }
		}

		public int HVLVItemCount
		{
			get
			{
				var bizoType = BusinessObjectFactory.GetBusinessObjectBaseTypeFromTablePrefix(HVLVItemSchema.Constants.Prefix);
				var dbCount = Factory.GetDatabaseCount(bizoType, HVLVItemQuery);

				var changeSet = Factory.GetChanges();
				var newBizos = changeSet.GetAddedObjects().Where(x => x is IHVLVItem && !x.IsInDatabase).Cast<IHVLVItem>();
				var newBizosCount = newBizos.Count(x => x.HVI_JS_LoadedOnShipment == PK);

				var activeStateChanges = 0;
				var bizoChangeSets = changeSet.GetChangedObjects().Where(x => x.IsExistsInDatabase);

				foreach (var change in bizoChangeSets)
				{
					if (change.SessionInstance is IHVLVItem itemInSession
						&& change.DatabaseInstance is IHVLVItem itemInDatabase
						&& itemInSession.HVI_JS_LoadedOnShipment == PK
						&& itemInDatabase.HVI_JS_LoadedOnShipment == PK
						&& itemInSession.HVI_IsActive != itemInDatabase.HVI_IsActive)
					{
						activeStateChanges += itemInSession.HVI_IsActive ? 1 : -1;
					}
				}

				return dbCount + newBizosCount + activeStateChanges;
			}
		}

		#endregion

		#region HVLVConsignments

		public IHVLVConsignmentCollectionForDocument HVLVConsignmentCollectionForDocument
		{
			get
			{
				if (hvlvConsignmentCollectionForDocument == null)
				{
					hvlvConsignmentCollectionForDocument = ObjectFactory.Get<IHVLVConsignmentCollectionForDocument>(nameof(IHVLVConsignmentCollectionForDocument), this);
					hvlvConsignmentCollectionForDocument.Load();
				}
				return hvlvConsignmentCollectionForDocument;
			}
		}
		IHVLVConsignmentCollectionForDocument hvlvConsignmentCollectionForDocument;

		[CargoWise.Macros.MacroIgnore]
		public IEnumerable<IHVLVConsignment> HVLVConsignments
		{
			get
			{
				return HVLVConsignmentHeader?.Consignments ?? Enumerable.Empty<IHVLVConsignment>();
			}
		}

		public ZQuery HVLVItemQuery => new ZQuery(HVLVItemSchema.HVI_JS_LoadedOnShipment, PK);

		#endregion

		#region HVLVConsignmentHeader

		public IHVLVConsignmentHeader HVLVConsignmentHeader
		{
			get { return Factory.LoadTop1<IHVLVConsignmentHeader>(new ZQuery(HVLVConsignmentHeaderSchema.HCH_JS_Shipment, PK)); }
		}

		void CreateHVLVConsignmentHeaderIfNecessary()
		{
			if (IsHighVolumeLowValue)
			{
				if (HVLVConsignmentHeader == null || ((BusinessObject)HVLVConsignmentHeader).IsDeleted)
				{
					var hvlvConsignmentHeader = Factory.New<IHVLVConsignmentHeader>();
					hvlvConsignmentHeader.HCH_JS_Shipment = PK;
					RegisterEditableChildObject(hvlvConsignmentHeader);
					PopulateHCHUsageType();
				}
			}
		}

		public bool HasHVLVDataCreated => HVLVConsignmentHeader is IHVLVConsignmentHeader hvlvConsignmentHeader && hvlvConsignmentHeader.HasHVLVDataCreated;

		List<ZString> ValidPlusUsageCountries => new List<ZString> { CountryCodes.UnitedStates, CountryCodes.SouthAfrica };

		public bool IsPlusUsageType
		{
			get
			{
				var shipmentDestinationCustomsJurisdictionCountry = CountryCodes.GetCustomsCountryOfJurisdiction(Destination?.Country?.Code);
				var branchCustomsJurisdictionCountry = CountryCodes.GetCustomsCountryOfJurisdiction(GlbBranch.CurrentBranch.Country?.Code);
				var isSameCustomsJurisdictionCountry = (branchCustomsJurisdictionCountry == shipmentDestinationCustomsJurisdictionCountry);
				var isValidPlusUsageCountry = ValidPlusUsageCountries.Contains(shipmentDestinationCustomsJurisdictionCountry);

				if (shipmentDestinationCustomsJurisdictionCountry == CountryCodes.UnitedStates)
				{
					return isValidPlusUsageCountry && (isSameCustomsJurisdictionCountry || HVLVDataRegistry.Instance.EnableSecurityFilingsForNonUSACompanies.Value);
				}

				return isValidPlusUsageCountry && isSameCustomsJurisdictionCountry;
			}
		}

		void PopulateHCHUsageType()
		{
			if (IsPlusUsageType)
			{
				HVLVConsignmentHeader.HCH_UsageType = HVLVConstants.HVLVConsignmentHeaderUsageTypesCodes.Plus;
			}
			else
			{
				HVLVConsignmentHeader.HCH_UsageType = HVLVConstants.HVLVConsignmentHeaderUsageTypesCodes.Standard;
			}
		}

		#endregion

		#region Rating

		RatingAdaptersProvider IRatingSupporter.AdaptersProvider
		{
			get
			{
				if (JS_ShipmentType == Constants.ShipmentTypes.HighVolumeLowValue && HVLVDataRegistry.Instance.IsHVLVAutoRatingEnabled.Value)
				{
					return (RatingAdaptersProvider)ObjectFactory.New<IHVLVRatingAdapterProvider>(this);
				}

				return new ForwardingShipmentRatingAdaptersProvider(this);
			}
		}

		protected override IAutoRating GetRatingAdapterCore()
		{
			return new ForwardingShipmentRatingAdapter(this);
		}

		#endregion

		#region IRelatedItemsNameProvider Members

		ZString IRelatedItemsNameProvider.GetNameOfRelatedItem(IBusiness relatedItem)
		{
			if (CoLoadMasterShipment != null)
			{
				ZString result = ((IRelatedItemsNameProvider)CoLoadMasterShipment).GetNameOfRelatedItem(relatedItem);
				if (!result.IsEmpty)
				{
					return ForwardingShipmentWorkflowEventContextMasterTypes.Descriptions.MasterShipment + ", " + result;
				}
			}

			var shipment = relatedItem as CommonShipment;
			if (shipment != null && CoLoadMasterShipment != null && CoLoadMasterShipment.PK == shipment.PK)
			{
				return ForwardingShipmentWorkflowEventContextMasterTypes.Descriptions.MasterShipment;
			}

			var consol = relatedItem as CommonConsol;
			if (consol != null)
			{
				return GetContextForConsol(consol);
			}

			return ZString.Empty;
		}

		string GetContextForConsol(CommonConsol consol)
		{
			var sortedConsols = Consols.ToArray<CommonConsol>();
			MovementLegComparer.SortMovementLegsByPorts(sortedConsols);
			if (sortedConsols != null && sortedConsols.Length > 0)
			{
				for (int i = 0; i < sortedConsols.Length; i++)
				{
					if (sortedConsols[i].PK == consol.PK)
					{
						string classifier = string.Empty;
						if (sortedConsols.Length > 1)
						{
							if (i == 0)
							{
								classifier = ForwardingShipmentWorkflowEventContextMasterClassifiers.Descriptions.Departure;
							}
							else if (i == sortedConsols.Length - 1)
							{
								classifier = ForwardingShipmentWorkflowEventContextMasterClassifiers.Descriptions.Arrival;
							}
							else
							{
								classifier = ForwardingShipmentWorkflowEventContextMasterClassifiers.Descriptions.Transship;
							}
						}

						return (!string.IsNullOrEmpty(classifier) ? classifier + " " : string.Empty) + ForwardingShipmentWorkflowEventContextMasterTypes.Descriptions.Consol;
					}
				}
			}

			return string.Empty;
		}

		#endregion

		#region IForwardingShipment Members

		IJobDocAddress IForwardingShipment.ConsigneeDocumentaryAddress
		{
			get { return ConsigneeDocumentaryAddress; }
		}

		IJobDocAddress IForwardingShipment.ConsignorDocumentaryAddress
		{
			get { return ConsignorDocumentaryAddress; }
		}

		IJobDocsAndCartage IForwardingShipment.DocsAndCartage
		{
			get { return DocsAndCartage; }
		}

		#endregion

		#region IModuleToModule Members

		void IModuleToModule.AddToRelatedJobs(BusinessObject loadedJob)
		{
		}

		bool IModuleToModule.CanExportData(out ZString errorMessage)
		{
			var limitNotification = new OrdersOnShipmentLimitHelper(this).CreateNotification(GenericOrders.Count + 1);
			errorMessage = (limitNotification != null && limitNotification.Type == CargoWise.EntityFramework.NotificationType.Error)
				? limitNotification.Message
				: "";

			return errorMessage.IsEmpty;
		}

		BusinessObject IModuleToModule.GetRelatedObject()
		{
			return null;
		}

		IOrgHeader IModuleToModule.RecipientOrganisation
		{
			get { return null; }
		}

		#endregion

		#region IDocAddresses

		protected override Dictionary<DocAddressType, string> GetNewDocAddressTypesAndProperties()
		{
			var result = base.GetNewDocAddressTypesAndProperties();
			result.Add(DocAddressType.ConsigneeDocumentaryAddress, Schema.ConsigneeNameOrPK);
			result.Add(DocAddressType.ConsignorDocumentaryAddress, Schema.ConsignorNameOrPK);
			result.Add(DocAddressType.ControllingAgent, Schema.ControllingAgentNameOrPK);
			result.Add(DocAddressType.PickupAgent, Schema.PickupAgentPK);

			return result;
		}

		protected override DocAddressType[] SupportedAddressTypes
		{
			get
			{
				var result = new List<DocAddressType>(base.SupportedAddressTypes);

				result.Add(DocAddressType.ControllingAgent);
				result.Add(DocAddressType.HouseBillIssuingParty);
				result.Add(DocAddressType.WarehouseClient);
				result.Add(DocAddressType.Warehouse);
				result.Add(DocAddressType.Creditor);

				var buyerDocumentaryAddressIndex = result.IndexOf(DocAddressType.BuyerDocumentaryAddress);
				if (buyerDocumentaryAddressIndex != -1)
				{
					result.Insert(buyerDocumentaryAddressIndex, DocAddressType.SupplierDocumentaryAddress);
				}
				else
				{
					result.Add(DocAddressType.SupplierDocumentaryAddress);
				}

				if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Constants.CountryCodes.Australia)
				{
					var declaration = GetDeclaration() as AU.IJobDeclaration;
					var isQuarantine = declaration != null && declaration.IsQuarantine;

					if (isQuarantine)
					{
						result.Add(DocAddressType.AQISProcessingEstablishment);
					}
				}

				result.Add(DocAddressType.BookingPartyDocumentaryAddress);

				return result.ToArray();
			}
		}

		protected override JobDocAddressRequirement GetDocAddressRequirement(DocAddressType addressType)
		{
			switch (addressType)
			{
				case DocAddressType.AQISProcessingEstablishment:
					return AQISProcessingEstablishmentAddressRequirement;
				case DocAddressType.ControllingAgent:
					return ControllingAgentDocAddressRequirement;
				case DocAddressType.HouseBillIssuingParty:
					return HouseBillIssuingPartyDocAddressRequirement;
				case DocAddressType.WarehouseClient:
					return WarehouseClientDocAddressRequirement;
				default:
					return base.GetDocAddressRequirement(addressType);
			}
		}

		protected override OrgHeaderCollection GetOrgHeaderList(DocAddressType addressType)
		{
			switch (addressType)
			{
				case DocAddressType.ControllingCustomer:
					return GetOrganisationsFindBoxCollectionWithOriginalProviderType(nameof(Lookups.ControllingCustomerList));
				case DocAddressType.ControllingAgent:
					return GetOrganisationsFindBoxCollectionWithOriginalProviderType(nameof(Lookups.ControllingAgentList));
				default:
					return base.GetOrgHeaderList(addressType);
			}
		}

		#region AQISProcessingEstablishment

		JobDocAddressRequirement AQISProcessingEstablishmentAddressRequirement
		{
			get
			{
				if (fAQISProcessingEstablishmentAddressRequirement == null)
				{
					fAQISProcessingEstablishmentAddressRequirement = new JobDocAddressRequirement(DocAddressType.AQISProcessingEstablishment)
					{
						DefaultMax = 0,
					};
					DocAddressManager.AddRequirement(fAQISProcessingEstablishmentAddressRequirement);
				}
				return fAQISProcessingEstablishmentAddressRequirement;
			}
		}
		JobDocAddressRequirement fAQISProcessingEstablishmentAddressRequirement;

		#endregion

		#region Controlling Agent

		public SecurityCheckpoint GetControllingAgentSecurityCheckPoint()
		{
			if (IsAir)
			{
				return Env.Security.MaintainShipmentAllowSaveWithoutControllingAgentAir;
			}

			if (IsSea)
			{
				return Env.Security.MaintainShipmentAllowSaveWithoutControllingAgentSea;
			}

			if (IsRoad)
			{
				return Env.Security.MaintainShipmentAllowSaveWithoutControllingAgentRoad;
			}

			if (IsRail)
			{
				return Env.Security.MaintainShipmentAllowSaveWithoutControllingAgentRail;
			}

			return Env.Security.MaintainShipmentAllowSaveWithoutControllingAgent;
		}

		public ZString ControllingAgentFieldType
		{
			get
			{
				return ControllingAgentDocumentaryAddress.E2_AddressOverride ?
					nameof(FieldType.Text) :
					nameof(FieldType.OrganisationGuid);
			}
		}

		[BusinessObjectTestExclude]
		[List("Lookups.OrgHeader_List")]
		public ZString ControllingAgentNameOrPK
		{
			get { return ControllingAgentDocumentaryAddress.OrganisationNameOrPK; }
			set { ControllingAgentDocumentaryAddress.OrganisationNameOrPK = value; }
		}

		public ZPropertyInfo ControllingAgentNameOrPKInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ControllingAgentNameOrPK, x => ControllingAgentDocumentaryAddress.OrganisationNameOrPKInfo); }
		}

		void SetBookingPartyDocumentaryAddressDefaults(JobDocAddress docAddress)
		{
			if (isBookingPartyDocumentaryAddressReadonly.HasValue)
			{
				docAddress.ReadOnly = isBookingPartyDocumentaryAddressReadonly.Value;
			}
		}

		public void SetBookingPartyDocumentaryAddressReadonly(bool isReadOnly)
		{
			isBookingPartyDocumentaryAddressReadonly = isReadOnly;

			if (!IsNullOrDeleted(BookingPartyDocumentaryAddress))
			{
				BookingPartyDocumentaryAddress.ReadOnly = isReadOnly;
			}
		}

		bool? isBookingPartyDocumentaryAddressReadonly;

		#endregion

		#endregion // IDocAddresses

		#region IRelatableActivity

		ZBool IRelatableActivity.ShouldIgnoreSuperAndSubActivityRelationships
		{
			get { return false; }
		}

		ZString IRelatableActivity.ActivityType
		{
			get { return RelatableActivityTypeList.Codes.Shipments; }
		}

		IOrgHeader IRelatableActivity.Client
		{
			get { return null; }
		}

		ZBool IRelatableActivity.ClientHasChanges
		{
			get { return false; }
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
			get { return string.Join("; ", new[] { JS_TransportMode, JS_PackingMode, ZString.Format("{0} > {1}", JS_RL_NKOrigin, JS_RL_NKDestination) }.Where(x => !x.IsEmpty)); }
		}

		void IRelatableActivity.OnRelatedActivitySaving(IRelatableActivity relatedActivity)
		{
		}

		ZBool IRelatableActivity.SupportViewRelatedCommunications => ZBool.True;

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

		#region IAdditionalFetchHintsForJobParent

		void IAdditionalFetchHintsForJobParent.LoadAdditionalFetchHints()
		{
			Factory.AddFetchHint(WhsDocketJobPivotSchema.WV_ParentId, PK);
			Factory.AddFetchHint(CusHAWBSchema.CS_JS, PK);
			Factory.AddFetchHint(CusSCAHouseSchema.CA_JS, PK);
		}

		#endregion

		#region IDISHostProvider

		MasterFiles.Business.DIS.IDISHost MasterFiles.Business.DIS.IDISHostProvider.DISHost
		{
			get
			{
				foreach (IBaseJobDeclaration declaration in Declarations)
				{
					if (declaration.CompanyPK == GlbCompany.CurrentCompany.PK)
					{
						var declarationAsDISHost = declaration as MasterFiles.Business.DIS.IDISHost;

						if (declarationAsDISHost != null)
						{
							return declarationAsDISHost;
						}
					}
				}
				return null;
			}
		}

		#endregion

		#region MAWB Allocation

		public IMAWBAllocationParent MAWBAllocator { get; set; }

		#endregion

		#region IValidateForCustomsMessagingSupporter Members

		ZBool IValidateForCustomsMessagingSupporter.SupportValidateCustomsMessaging
		{
			get { return true; }
		}

		BusinessObject IValidateForCustomsMessagingSupporter.GetEntityToValidate(string triggerAction)
		{
			return this.GetDeclaration();
		}

		#endregion

		#region Debug CS01154849

		public ZStringBuilder DebugLog => debugLogForCS01154849;

		readonly ZStringBuilder debugLogForCS01154849 = new ZStringBuilder();

		public void LogDebugInfo() => DebugLog.AppendLine(DebugInfoForCS01154849);

		string DebugInfoForCS01154849 => FormattableString.Invariant($"PK: {PK}, JS_IsCancelled: {JS_IsCancelled}, JS_IsForwardRegistered: {JS_IsForwardRegistered}, JS_IsBooking: {JS_IsBooking}, JS_TH_OneTimeQuote: {JS_TH_OneTimeQuote}, IsInDatabase: {IsInDatabase}");

		#endregion

		bool IStmNoteParentWithSystemNote.IsSystemNote(StmNote note)
		{
			return note != null
				&& (note.ST_Description == PredefinedNoteTypes.Instance.ManualRelease.Description
					|| note.ST_Description == PredefinedNoteTypes.Instance.ManualCancel.Description
					|| note.ST_Description == PredefinedNoteTypes.Instance.ManualSubmission.Description);
		}

		public IExchangeRateSource GetExRateSource(ExRateSourceType sourceType)
		{
			if (sourceType == ExRateSourceType.BillingJob)
			{
				if (Job != null)
				{
					return (IExchangeRateSource)Job;
				}
			}

			return null;
		}

		void IHaveDetailedGoodsDescription.NotifyChanged()
		{
			if (AirCargoSynchroniser != null)
			{
				AirCargoSynchroniser.GoodsDescription = FullGoodsDescription;
			}
		}

		protected override IDictionary<ZString, Type> GetCusCodeDataTypesCore()
		{
			var result = base.GetCusCodeDataTypesCore();
			result.Add(Customs.Common.SG.CusCodeDataTypeList.Codes.CMD, ObjectFactory.GetType<SG.ICMDPermitNumber>());
			return result;
		}

		#region IForwardingConsol Members

		IEnumerable<IForwardingShipment> IForwardingShipment.CoLoadShipments
		{
			get { return CoLoadShipments.Cast<IForwardingShipment>(); }
		}

		ZBool IForwardingShipment.IsMasterShipmentRepresentingAllChildShipments
		{
			get { return IsMasterShipmentRepresentingAllChildShipments; }
		}

		ITransport IForwardingShipment.Transports_AddNew()
		{
			return this.Transports.AddNew();
		}

		#endregion

		#region HouseBillIssuingParty

		public OrgHeader HouseBillIssuingParty
		{
			get { return (HouseBillIssuingPartyDocumentaryAddress == null || !HouseBillIssuingPartyDocumentaryAddress.HasRealOrganisation) ? null : HouseBillIssuingPartyDocumentaryAddress.Organisation; }
		}

		public JobDocAddress HouseBillIssuingPartyDocumentaryAddress
		{
			get
			{
				if (BusinessObject.IsNullOrDeleted(houseBillIssuingPartyDocumentaryAddress))
				{
					houseBillIssuingPartyDocumentaryAddress = DocAddresses.FindOrCreateWithRequirement(HouseBillIssuingPartyDocAddressRequirement);
				}
				return houseBillIssuingPartyDocumentaryAddress;
			}
		}
		JobDocAddress houseBillIssuingPartyDocumentaryAddress;

		JobDocAddressRequirement HouseBillIssuingPartyDocAddressRequirement
		{
			get { return houseBillIssuingPartyDocAddressRequirement ?? (houseBillIssuingPartyDocAddressRequirement = new JobDocAddressRequirement(DocAddressType.HouseBillIssuingParty)); }
		}
		JobDocAddressRequirement houseBillIssuingPartyDocAddressRequirement;

		#endregion

		#region SupplierDocAddress

		public JobDocAddress SupplierDocAddress
		{
			get
			{
				if (fSupplierDocAddress == null || fSupplierDocAddress.IsDeleted)
				{
					fSupplierDocAddress = DocAddresses.FindOrCreateWithRequirement(SupplierDocAddressRequirement);
				}
				return fSupplierDocAddress;
			}
		}
		JobDocAddress fSupplierDocAddress;

		JobDocAddressRequirement SupplierDocAddressRequirement
		{
			get
			{
				if (fSupplierDocAddressRequirement == null)
				{
					fSupplierDocAddressRequirement = new JobDocAddressRequirement(DocAddressType.SupplierDocumentaryAddress);
					DocAddressManager.AddRequirement(fSupplierDocAddressRequirement);
				}
				return fSupplierDocAddressRequirement;
			}
		}
		JobDocAddressRequirement fSupplierDocAddressRequirement;

		#endregion

		#region WarehouseClientDocAddressRequirement

		JobDocAddressRequirement WarehouseClientDocAddressRequirement
		{
			get
			{
				if (warehouseClientDocAddressRequirement == null)
				{
					warehouseClientDocAddressRequirement = new JobDocAddressRequirement(DocAddressType.WarehouseClient)
					{
						ValidateOrganisationPK = ValidationWarehouseClientDocAddress,
					};
					warehouseClientDocAddressRequirement.CanOverride = false;
				}
				return warehouseClientDocAddressRequirement;
			}
		}
		JobDocAddressRequirement warehouseClientDocAddressRequirement;

		void ValidationWarehouseClientDocAddress(JobDocAddressValidation validation)
		{
			var parent = validation.Parent;
			if (parent?.Organisation != null
					&& !parent.Organisation.OH_IsWarehouseClient
					&& !parent.OrganisationPKInfo.HasError(warehouseClientError))
			{
				parent.OrganisationPKInfo.AddError(warehouseClientError);
			}
		}
		readonly string warehouseClientError = ResString.GetMultilingualString("8bf4b8da-e410-413e-87ae-ef0a4da17bd7", "An Organization added for a Warehouse Client Address must have an Organization Type of Warehouse.");

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
				Consols.SetReadOnlyIncludingChildren(true);
				templateRecord?.RegisterEditableChildObject(this);
			}
		}
		StmTemplateRecord templateRecord;

		public class DummyLoggerForTemplate : UniversalDataBuss.Integration.DummyLogger
		{
			protected override bool OrgMatchingDisabledCore => true;
		}

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
			var manager = (IShipmentDataContextManager)this.GetUniversalDataContextManager();
			var actionInfo = new ActionInfo(new[] { new RecipientRoleDetail { Type = RecipientRoleType.FOR, ServiceCode = ServiceCodeType.HLD } }, this)
			{
				ActionType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML
			};
			var outboundSessionTracker = new DataWritingManager(actionInfo);
			var writer = manager.GetShipmentDataObjectWriter(outboundSessionTracker);
			var shipment = writer.GetDataObject(this);

			var stream = (SubStreamableStream)new MemoryStream();
			try
			{
				var xmlWriter = ObjectFactory.Get<IXmlWriter>();
				xmlWriter.WriteXML(shipment, stream);

				using (var reader = new StreamReader(stream))
				{
					stream = null; // Will be disposed by reader

					var xmlAsString = reader.ReadToEnd();
					const string xmlDeclaration = @"<?xml version=""1.0"" encoding=""utf-8""?>";
					if (xmlAsString.StartsWith(xmlDeclaration, StringComparison.Ordinal))
					{
						xmlAsString = xmlAsString.Remove(0, xmlDeclaration.Length);
					}
					targetTemplateRecord.STR_Data = xmlAsString;
				}
			}
			finally
			{
				if (stream != null)
				{
					stream.Dispose();
				}
			}

			targetTemplateRecord.STR_ModuleID = nameof(ModuleId.JobShipment);
			targetTemplateRecord.PopulateIdIfNeeded();
			if (JS_UniqueConsignRef.IsEmpty)
			{
				JS_UniqueConsignRef = targetTemplateRecord.STR_ReferenceId;
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
			var shipmentDataObject = new Shipment(DefaultDataObjectWriterStrategy.Instance);
			using (var stream = (SubStreamableStream)new MemoryStream(Encoding.UTF8.GetBytes(sourceTemplateRecord.STR_Data)))
			{
				ObjectFactory.Get<IXmlReader>().ReadXML(shipmentDataObject, stream, sourceTemplateRecord.Logger);
			}

			SetCompanyAndDataProviderDetails(shipmentDataObject, sourceTemplateRecord.Logger);

			var manager = this.GetUniversalDataContextManager() as IShipmentDataContextManagerInternal ?? throw new InvalidOperationException("Unsupported Data Context Manager");

			var universalObjectFactory = new UniversalObjectFactory(Factory);
			var reader = manager.GetShipmentDataObjectReader(shipmentDataObject, sourceTemplateRecord.Logger, universalObjectFactory) ?? throw new InvalidOperationException(FormattableString.Invariant($"{manager.GetType().FullName} returned <null> reader for Shipment Template Record {sourceTemplateRecord.STR_ReferenceId}"));

			IsTemplateRecord = true;
			TemplateRecord = sourceTemplateRecord;

			var templateRecordReader = reader as ITopLevelDataObjectReaderForTemplateRecord;
			templateRecordReader.ReadDirectlyIntoBusinessObject(this);

			if (JS_UniqueConsignRef.IsEmpty)
			{
				JS_UniqueConsignRef = sourceTemplateRecord.STR_ReferenceId;
			}

			if (sourceTemplateRecord.IsInDatabase)
			{
				((IBusinessObjectState)this).ClearHasChangesIncludingChildren();
				((INeedRow)this).Row.AcceptChanges();
			}
		}

		BusinessObject ITemplateRecordProvider.InstantiateFromTemplateRecord(BusinessObjectFactory factory, Type elementType, ITemplateRecord otherTemplateRecord)
		{
			var otherElement = factory.New(elementType);
			var otherTemplateRecordProvider = (ITemplateRecordProvider)otherElement;
			otherTemplateRecordProvider.LoadFromTemplateRecord(otherTemplateRecord);
			return otherElement;
		}

		void SetCompanyAndDataProviderDetails(TopLevelDataObject bizo, UniversalDataBuss.Integration.DummyLogger logger)
		{
			if (bizo.DataContext != null)
			{
				bizo.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
				logger.TopLevelDataObject = bizo;
			}
		}

		protected override void ReportErrorWhenSettingApprovedInspectionTypeWhenImportingData(ZString value)
		{
			if (!IsTemplateRecord)
			{
				base.ReportErrorWhenSettingApprovedInspectionTypeWhenImportingData(value);
			}
		}

		public void SetIsTemplateRecord(bool value)
		{
			IsTemplateRecord = value;
		}

		#endregion

		#region ISupplyChainSecurityImportExportSupporter

		ZString ISupplyChainSecurityImportExportSupporter.LoadCountryForSupplyChainSecurity
		{
			get
			{
				if (JS_TransportMode != Core.Constants.TransportModes.SeaAir)
				{
					return JS_RL_NKOrigin.SubstringSafe(0, 2);
				}

				var airTransport = TransportsInLegOrder.Cast<Transport>().FirstOrDefault(transport => transport.JW_TransportMode == Core.Constants.TransportModes.Air);
				if (airTransport == null || airTransport.LoadPort == null || airTransport.LoadPort.Country?.Code != GlbCompany.CurrentCompany?.GC_RN_NKCountryCode)
				{
					return JS_RL_NKOrigin.SubstringSafe(0, 2);
				}

				return airTransport.LoadPort.Country.Code;
			}
		}

		ZString ISupplyChainSecurityImportExportSupporter.DischargeCountryForSupplyChainSecurity => JS_RL_NKDestination.SubstringSafe(0, 2);

		IEnumerable<ITransport> ISupplyChainSecurityImportExportSupporter.SupplyChainSecurityRelatedTransports
		{
			get { return TransportsIncludingRelated.Cast<ITransport>(); }
		}

		#endregion

		#region GetStrategies

		IBusinessObjectStrategy[] strategies;
		protected override IBusinessObjectStrategy[] GetStrategies()
		{
			if (strategies == null)
			{
				var baseStrategies = base.GetStrategies();
				var strategyList = new List<IBusinessObjectStrategy>(baseStrategies);
				strategyList.Add(new CommissionSourceMonitorStrategy(new[] { JS_TransportModeInfo, JS_RL_NKOriginInfo, JS_RL_NKDestinationInfo }, this));
				strategies = strategyList.ToArray();
			}

			return strategies;
		}

		#endregion

		#region Notes

		public override Notes Notes => notes ??= new ForwardingShipmentStmNotes(this);
		Notes notes;

		#endregion

		#region DetailedGoodsDescriptionNote

		protected override StmNote DetailedGoodsDescriptionNote
		{
			get
			{
				var filter = new ZQuery(StmNoteSchema.ST_ParentID, PK);
				filter.FetchOnlyFromLocalCache = !IsInDatabase;
				filter.AddToFilter(StmNoteSchema.ST_Description, PredefinedNoteTypes.Instance.DetailedGoodsDescription.Code);
				filter.AddToFilter(StmNoteSchema.ST_Table, TableName);
				return Factory.LoadTop1<ForwardingShipmentStmNote>(filter);
			}
		}

		#endregion

		public ZBool AddCountryCodeToEntryDetailsCaption => CountrySpecificSupport.AddCountryCodeToEntryDetailsCaption;

		#region IConversationProvider
		JobConversation IConversationProvider.eConversation
		{
			get
			{
				if (!IsInDatabase)
				{
					return null;
				}
				return conversation ?? (conversation = GetOrCreateConversation());
			}
		}
		JobConversation conversation;

		ModuleIdentifier IConversationProvider.ParentModule => JS_IsBooking && !JS_IsForwardRegistered ? ModuleIDs.QuotedBookings : ModuleIDs.JobShipment;
		ControllerID IConversationProvider.ParentController => JS_IsBooking && !JS_IsForwardRegistered ? ControllerIDs.QuotedBookings : ControllerIDs.JobShipment;
		IEnumerable<EConversation.Business.RelatedParty> IConversationProvider.AdditionalParticipants => Enumerable.Empty<EConversation.Business.RelatedParty>();
		bool IConversationProvider.SendEmailNotificationsOnSave => true;

		void IConversationProvider.RunConversationUpdateActionBeforeSaving()
		{
		}

		string IConversationProvider.EmailSubjectContentOverride => JS_IsBooking && !JS_IsForwardRegistered ? Res.GetString("E4E5D742-7C35-47E3-9C4E-A8AF154850CD", "Forwarding Booking {0}", JS_UniqueConsignRef) : $"{HumanReadableNameWithoutID} {JS_UniqueConsignRef}"; // No translation needed
		string IConversationProvider.FromAddressOverride => GlowRegistry.Instance.NeoEnableConversations.Value ? GlowRegistry.Instance.NeoConversationsEmailAddress.Value : default;
		NotificationEmailTemplate IConversationProvider.NotificationEmailTemplateOverride => default;

		JobConversation GetOrCreateConversation()
		{
			var result = JobConversation.GetOrCreate(this);
			RegisterEditableChildObject(result);
			return result;
		}

		#endregion

		#region IConversationAdditionalParticipantProvider Members

		IEnumerable<IConversationParticipant> IConversationAdditionalParticipantProvider.GetAdditionalParticipants(IReadOnlyCollection<IConversationParticipant> subscribedParticipants, JobConversationParticipant sender)
		{
			if (!GlowRegistry.Instance.NeoEnableConversations.Value ||
				subscribedParticipants.Any(p => !(p is IOrgContact)))
			{
				return Enumerable.Empty<IConversationParticipant>();
			}

			return ObjectFactory.Get<IForwardingShipmentParticipantProvider>().GetAdditionalParticipants(this, sender);
		}

		#endregion

		#region IConversationParentHyperlinkProvider Members

		bool IConversationParentHyperlinkProvider.ShouldUseThisProviderForHyperlink(IConversationParticipant participant)
		{
			return participant == null || participant is OrgContact;
		}

		string IConversationParentHyperlinkProvider.GetHyperlinkToConversationParent() => GlowRegistry.GetNeoDefaultFormFlowUrl(this);

		#endregion

		string IAllowAttachEmailsToEDocs.ReferenceNumber => JS_UniqueConsignRef;

		#region ShipmentContainsPermissibleQuantities

		public override bool ShipmentContainsPermissibleQuantities()
		{
			return ForwardingUNDGPermissableQuantitiesHelper.AreRelatedShipmentsLithiumBatteriesPermissibleForPacking(this);
		}

		#endregion

		#region ISometimesWorkflowProvider

		bool ISometimesWorkflowProvider.ShouldSupportWorkflowTemplateApplication => !(JS_IsBooking && !JS_IsForwardRegistered);

		#endregion

		#region IWorkflowTemplateParameterProvider

		TemplateApplicationParameters IWorkflowTemplateParameterProvider.TemplateApplicationParameters => HasHVMParent ?
																				TemplateApplicationParameters.ApplySpecificEntityTypes(TemplateEntityType.Triggers) :
																				null;

		bool HasHVMParent => IsHighVolumeLowValue && CoLoadMasterShipment is ForwardingShipment masterShipment && masterShipment.IsHighVolumeLowValueMaster;

		#endregion

		#region NVOCC House Bill of Lading (HBL)

		public bool IsElectronicShippingInstructionReceived
		{
			get
			{
				var logQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.StatusUpdated.Code);
				logQuery.AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.Contains, FormattableString.Invariant($"|{Params.Type}={Constants.EventReferenceMessageTypes.ShipmentStatus}"));

				return Logs.Find(logQuery).Any(l => !l.SL_IsCancelled && l.Parameters.ContainsKey(Params.New) && l.Parameters[Params.New] == ShipmentStatusList.Codes.ElectronicShippingInstruction);
			}
		}

		#endregion

		#region NVOCC IsElectronicBookingReceived

		public bool IsElectronicBookingReceived
		{
			get
			{
				var logQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.StatusUpdated.Code);
				logQuery.AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.Contains, FormattableString.Invariant($"|{Params.Type}={Constants.EventReferenceMessageTypes.ShipmentStatus}"));

				var stuEvent = Logs.Find(logQuery)
					.Where(l => !l.SL_IsCancelled)
					.OrderByDescending(l => l.SL_PostedTimeUtc)
					.FirstOrDefault();

				return stuEvent != null
					&& stuEvent.Parameters.ContainsKey(Params.New)
					&& stuEvent.Parameters[Params.New] == ShipmentStatusList.Codes.ElectronicBooking;
			}
		}

		#endregion

		#region Menu Item Filter

		public bool HasHIRAndStartWithSHPNumber => !IsCancelled
			&& Numbers.Cast<CusEntryNumber>().Any(y => !y.IsDeleted && y.CE_EntryType == CustomsReferenceNumberType.eHubInterchangeReference.HIR && y.CE_EntryNum.StartsWith("SHP"));

		public bool HasCOCAndMatchConsolsCFSAddressCTRNumber
		{
			get
			{
				var cocNUmber = Numbers.Cast<CusEntryNumber>().FirstOrDefault(number => !number.IsDeleted && number.CE_EntryType == AdditionalReferenceNumbersCodes.COC && number.CE_RN_NKCountryCode == Constants.CountryCodes.France)?.CE_EntryNum ?? ZString.Empty;
				return !cocNUmber.IsEmpty
					&& Consols.Cast<ForwardingConsol>().Any
						(c => cocNUmber == (c.PackDepotAddress?.Header?.CustomsCodes?.Cast<OrgCusCode>().FirstOrDefault(code => code.OK_RN_NKCodeCountry == CountryCodes.France && code.OK_CodeType == OrgCusCode.CodeTypes.CustomsOfficeForTransit)?.SecuredCustomsRegNo ?? ZString.Empty));
			}
		}

		#endregion

		#region IsSentConsolidationAdvice

		public ZBool IsSentConsolidationAdvice
		{
			get => Logs.GetAllLogs().OfType<StmALog>()
				.FirstOrDefault(log => log.Parameters.GetValueSafe(EventConstants.EventReferenceParameters.Codes.MessageType) == ShipmentDocumentConstants.DocumentNames.ConsolidationAdvice && log.SL_SE_NKEvent == Events.MessageSentCode) != null;
		}

		#endregion

		#region IComplianceItemRiskStatusProvider

		IEnumerable<IScreeningParty> ICompliancePartyRiskStatusProvider.Parties
		{
			get
			{
				FetchForPartiesComplianceSynchronization();

				var parties = GetScreeningPartiesCommon().ToList<IScreeningParty>();

				if (Job != null)
				{
					parties.Add(new ScreeningParty(this, Res.GetString("69b5c07f-35af-4aa2-82df-55cb5d5c927e", "Overseas Agent"), Job.AgentCollect));
				}

				parties.Add(new ScreeningParty(this, Res.GetString("e28c74d-c8aa-45e8-9669-6f6ba979ea49", "Planned Carrier"), BookedShippingLine));

				if (Creditor != null)
				{
					parties.Add(new ScreeningParty(this, Res.GetString("bc17a02f-2050-4451-8195-df6582837534", "Creditor"), Creditor));
				}

				foreach (JobService service in Services)
				{
					parties.Add(new ScreeningParty(this, Res.GetString("954e2a1c-f944-4375-a9e6-c292fe852c2a", "Contractor"), service.Contractor));
					parties.Add(new ScreeningParty(this, Res.GetString("c808f570-beaf-40ac-aa6c-76c35074dfcd", "Service Provider"), service.ServiceProvider));
				}

				foreach (ShipmentGateway gateway in Gateways)
				{
					parties.Add(new ScreeningParty(this, Res.GetString("a86f3122-60f7-4e5c-aeec-c66f9432d0d4", "Gateway Forwarder"), gateway.Forwarder));
				}

				foreach (Transport transport in TransportsIncludingRelated)
				{
					parties.Add(new ScreeningParty(this, Res.GetString("16a8798d-e0e9-4de4-bac8-454064a1fd33", "Carrier"), transport.Carrier));
					parties.Add(new ScreeningParty(this, Res.GetString("bc17a02f-2050-4451-8195-df6582837534", "Creditor"), transport.Creditor));
					var departureOrg = transport.DepartureLocation != null ? Factory.Load<OrgHeader>(transport.DepartureLocation.OA_OH) : null;
					parties.Add(new ScreeningParty(this, Res.GetString("ba871608-4aa0-49e8-aa1f-8732a685668d", "Depart From"), departureOrg));
					var arrivalOrg = transport.ArrivalLocation != null ? Factory.Load<OrgHeader>(transport.ArrivalLocation.OA_OH) : null;
					parties.Add(new ScreeningParty(this, Res.GetString("fdb806d5-1fea-4fe7-9a58-eaa70913d592", "Arrival At"), arrivalOrg));
					parties.AddRange(transport.GetScreeningPartiesFromVessel(this));
				}

				foreach (PackLine packLine in OuterPackLines)
				{
					var transitWarehouse = packLine.LastKnownTransitWarehouseAddress != null ? Factory.Load<OrgHeader>(packLine.LastKnownTransitWarehouseAddress.OA_OH) : null;
					parties.Add(new ScreeningParty(this, Res.GetString("d5c35d6e-c600-4891-8498-c63fafd0c233", "Transit Warehouse"), transitWarehouse));
				}

				var subForwardingShipments = CoLoadShipments.Where(shipment => shipment is ForwardingShipment);
				foreach (var shipment in subForwardingShipments)
				{
					var partiesFromShipments = ((ICompliancePartyRiskStatusProvider)shipment).Parties;
					foreach (ScreeningParty party in partiesFromShipments)
					{
						party.AddParent(this);
					}
					parties.AddRange(partiesFromShipments);
				}

				if (IsHighVolumeLowValue && HVLVDataRegistry.Instance.HVLVEnablePartyScreening.Value.EnableHVLVPartyScreening)
				{
					var partiesFromHVLV = ((IScreeningPartyProvider)HVLVConsignmentHeader).ScreeningParties;

					foreach (var party in partiesFromHVLV)
					{
						party.AddParent(this);
						party.LinkEntityToAssociatedJobIfApplicable(this);
					}

					parties.AddRange(partiesFromHVLV);
				}

				parties.AddRange(((IGlobalCommercialInvoiceComplianceProcessor.IGlobalCommercialInvoiceProvider)this).DataProvider.Parties);

				return parties;
			}
		}

		IEnumerable<IComplianceLocation> IComplianceLocationRiskStatusProvider.Locations
		{
			get
			{
				FetchForLocationsComplianceSynchronization();

				var countries = new List<IComplianceLocation>();
				AddCountryToList(this, (NoResString)"Origin Country", Origin?.Country);
				AddCountryToList(this, (NoResString)"Destination Country", Destination?.Country);
				AddCountryToList(this, (NoResString)"Planned Load Country", LoadPort?.Country);
				AddCountryToList(this, (NoResString)"Planned Discharge Country", DischargePort?.Country);

				foreach (Transport transport in TransportsIncludingRelated)
				{
					AddCountryToList(this, (NoResString)"Routing Load Country", transport.LoadPort?.Country);
					AddCountryToList(this, (NoResString)"Routing Discharge Country", transport.DiscPort?.Country);
				}

				if (AWBHeaderManager.LoadExistingAWBHeader() != null)
				{
					if (AWBHeader.EH_IsConsigneeOverriden)
					{
						RefCountry country = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, AWBHeader.EH_ConsigneeCountryCode);
						AddCountryToList(this, (NoResString)"Consignee Overridden Country", country);
					}

					if (AWBHeader.EH_IsShipperOverriden)
					{
						RefCountry country = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, AWBHeader.EH_ShipperCountryCode);
						AddCountryToList(this, (NoResString)"Shipper Overridden Country", country);
					}

					if (AWBHeader.EH_IsNotifyOverriden)
					{
						RefCountry country = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, AWBHeader.EH_AlsoNotifyCountryCode);
						AddCountryToList(this, (NoResString)"Notify Overridden Country", country);
					}
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

					AddCountryToList(party.Parent, party.Description, country);
				}

				var subForwardingShipments = CoLoadShipments.Where(shipment => shipment is ForwardingShipment);
				foreach (var shipment in subForwardingShipments)
				{
					var countriesFromShipments = ((IComplianceLocationRiskStatusProvider)shipment).Locations;
					foreach (ScreeningParty country in countriesFromShipments)
					{
						country.AddParent(this);
					}
					countries.AddRange(countriesFromShipments);
				}

				countries.AddRange(((IGlobalCommercialInvoiceComplianceProcessor.IGlobalCommercialInvoiceProvider)this).DataProvider.Locations);

				return countries;

				void AddCountryToList(BusinessObject parent, string description, RefCountry country)
				{
					if (country != null)
					{
						countries.Add(new ScreeningParty(parent, description, country));
					}
				}
			}
		}

		IEnumerable<IComplianceCommodity> IComplianceCommodityRiskStatusProvider.Commodities
		{
			get
			{
				FetchForCommoditiesComplianceSynchronization();

				var commodities = new List<IComplianceCommodity>();
				AddPackingLineHarmonisedCode(commodities);
				AddInvoiceLineTariffCode(commodities);
				AddSubShipmentCommodities(commodities);

				commodities.AddRange(((IGlobalCommercialInvoiceComplianceProcessor.IGlobalCommercialInvoiceProvider)this).DataProvider.Commodities);

				return commodities;
			}
		}

		bool hasFetchedPartiesForComplianceSynchronization;

		void FetchForPartiesComplianceSynchronization()
		{
			if (!hasFetchedPartiesForComplianceSynchronization && !IsDeleted)
			{
				hasFetchedPartiesForComplianceSynchronization = true;

				var subForwardingShipments = CoLoadShipments.Where(shipment => shipment is ForwardingShipment);
				Factory.AddFetchHint(JobShipmentSchema.Instance, new ZQuery(JobShipmentSchema.JS_JS_ColoadMasterShipment, subForwardingShipments.Select(s => s.PK)));

				var orgAddressPKs = new HashSet<ZGuid>();
				var vesselCodes = new HashSet<ZString>();
				var contactPKs = new HashSet<ZGuid>();
				var orgPKs = new HashSet<ZGuid>();

				if (Job != null)
				{
					orgAddressPKs.Add(Job.JH_OA_AgentCollectAddr);
				}

				orgAddressPKs.Add(JS_OA_BookedShippingLineAddress);

				orgPKs.Add(JS_OH_Creditor);

				foreach (JobService service in Services)
				{
					orgPKs.Add(service.ES_OH_Contractor);
					orgAddressPKs.Add(service.ES_OA_Location);
				}

				foreach (ShipmentGateway gateway in Gateways)
				{
					orgAddressPKs.Add(gateway.JSG_OA_ForwarderAddress);
				}

				foreach (Transport transport in TransportsIncludingRelated)
				{
					orgAddressPKs.Add(transport.JW_OA_CarrierAddress);
					orgAddressPKs.Add(transport.JW_OA_CreditorAddress);
					orgAddressPKs.Add(transport.JW_OA_DepartureLocation);
					orgAddressPKs.Add(transport.JW_OA_ArrivalLocation);

					if (transport.JW_TransportMode == Constants.TransportModes.Sea)
					{
						vesselCodes.Add(transport.JW_Vessel.ToUpper().Trim());
					}
				}

				foreach (PackLine packLine in OuterPackLines)
				{
					orgAddressPKs.Add(packLine.JL_OA_LastKnownTransitWarehouseAddress);
				}

				#region ScreeningPartiesCommon

				foreach (var docAddress in DocAddresses.Cast<JobDocAddress>())
				{
					orgAddressPKs.Add(docAddress.E2_OA_Address);
				}

				orgPKs.Add(JS_OH_DeliveryAgent);

				if (Job != null)
				{
					orgAddressPKs.Add(Job.JH_OA_LocalChargesAddr);
				}

				orgPKs.Add(JS_OH_ExportBroker);

				orgAddressPKs.Add(DocsAndCartage.JP_OA_PickupCartageCoAddr);
				orgAddressPKs.Add(DocsAndCartage.JP_OA_DeliveryCartageCoAddr);

				orgAddressPKs.Add(JS_OA_ExportReceivingDepot);

				orgPKs.Add(JS_OH_ImportBroker);

				orgAddressPKs.Add(JS_OA_ImportReleaseDepot);

				foreach (PackLine line in OuterPackLines)
				{
					foreach (UNDGDataItem dgData in line.UNDGs)
					{
						if (dgData.DI_OC_DGContact.IsValid)
						{
							contactPKs.Add(dgData.DI_OC_DGContact);
						}
					}
				}

				#endregion

				// Load all addresses in one go
				foreach (var orgAddressPK in orgAddressPKs)
				{
					Factory.AddFetchHint(typeof(OrgAddress), orgAddressPK);
				}

				// Load all vessels in one go
				foreach (var vesselCode in vesselCodes)
				{
					Factory.AddFetchHint(typeof(RefVessel), new ZQuery(RefVesselSchema.RV_Code, vesselCode));
				}

				// Load all contacts in one go
				foreach (var contactPK in contactPKs)
				{
					Factory.AddFetchHint(typeof(OrgContact), contactPK);
				}

				foreach (var orgAddressPK in orgAddressPKs)
				{
					var orgAddress = Factory.Load<OrgAddress>(orgAddressPK);
					if (orgAddress != null)
					{
						orgPKs.Add(orgAddress.OA_OH);
					}
				}

				foreach (var vesselCode in vesselCodes)
				{
					var vessel = Factory.LoadTop1<RefVessel>(new ZQuery(RefVesselSchema.RV_Code, vesselCode));
					if (vessel != null)
					{
						orgPKs.Add(vessel.RV_OH);
					}
				}

				foreach (var contactPK in contactPKs)
				{
					var contact = Factory.Load<OrgContact>(contactPK);
					if (contact != null)
					{
						orgPKs.Add(contact.OC_OH);
					}
				}

				// load all org headers in one go
				foreach (var orgPK in orgPKs)
				{
					Factory.AddFetchHint(typeof(OrgHeader), orgPK);
				}
			}
		}

		bool hasFetchedLocationsForComplianceSynchronization;

		void FetchForLocationsComplianceSynchronization()
		{
			if (!hasFetchedLocationsForComplianceSynchronization && !IsDeleted)
			{
				hasFetchedLocationsForComplianceSynchronization = true;

				var subForwardingShipments = CoLoadShipments.Where(shipment => shipment is ForwardingShipment);
				Factory.AddFetchHint(JobShipmentSchema.Instance, new ZQuery(JobShipmentSchema.JS_JS_ColoadMasterShipment, subForwardingShipments.Select(s => s.PK)));

				var countryCodes = new HashSet<ZString>();

				if (Origin != null)
				{
					countryCodes.Add(Origin.RL_RN_NKCountryCode);
				}

				if (Destination != null)
				{
					countryCodes.Add(Destination.RL_RN_NKCountryCode);
				}

				if (LoadPort != null)
				{
					countryCodes.Add(LoadPort.RL_RN_NKCountryCode);
				}

				if (DischargePort != null)
				{
					countryCodes.Add(DischargePort.RL_RN_NKCountryCode);
				}

				foreach (Transport transport in TransportsIncludingRelated)
				{
					if (transport.LoadPort != null)
					{
						countryCodes.Add(transport.LoadPort.RL_RN_NKCountryCode);
					}

					if (transport.DiscPort != null)
					{
						countryCodes.Add(transport.DiscPort.RL_RN_NKCountryCode);
					}
				}

				if (AWBHeaderManager.LoadExistingAWBHeader() != null)
				{
					if (AWBHeader.EH_IsConsigneeOverriden)
					{
						countryCodes.Add(AWBHeader.EH_ConsigneeCountryCode);
					}

					if (AWBHeader.EH_IsShipperOverriden)
					{
						countryCodes.Add(AWBHeader.EH_ShipperCountryCode);
					}

					if (AWBHeader.EH_IsNotifyOverriden)
					{
						countryCodes.Add(AWBHeader.EH_AlsoNotifyCountryCode);
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

		bool hasFetchedCommoditiesForComplianceSynchronization;

		void FetchForCommoditiesComplianceSynchronization()
		{
			if (!hasFetchedCommoditiesForComplianceSynchronization && !IsDeleted)
			{
				hasFetchedCommoditiesForComplianceSynchronization = true;

				var subForwardingShipments = CoLoadShipments.Where(shipment => shipment is ForwardingShipment).OfType<IComplianceItemRiskStatusProvider>().ToArray();
				ObjectFactory.Get<IComplianceRiskStatusSupporter>().PreFetchHintsCommodityDetails(subForwardingShipments, Factory);

				if (Declarations != null)
				{
					Factory.AddFetchHint(JobComInvoiceLineSchema.Instance, new ZQuery(JobComInvoiceLineSchema.JI_ClusterKey, Declarations.Select(dec => dec.JE_ClusterKey)));
				}
			}
		}

		ZBool IComplianceCommodityRiskStatusProvider.IsEditingCommoditySupported => ZBool.True;

		void AddPackingLineHarmonisedCode(List<IComplianceCommodity> commodities)
		{
			var lines = OuterPackLines.OfType<ForwardingPackLine>();

			foreach (var line in lines)
			{
				if (!line.JL_HarmonisedCode.IsEmpty
					&& line.Shipment != null && line.Shipment.JS_UniqueConsignRef == JS_UniqueConsignRef)
				{
					commodities.Add(new ComplianceCommodity(line.JL_HarmonisedCode, WorldCustomsOrganisationWCO, JS_UniqueConsignRef, ((IComplianceItemRiskStatusProvider)this).ParentID, line.JL_RN_NKOrigin, Res.GetString("D4871372-BA15-4873-9050-BE8658253EAA", "Packing"), line.JL_Description));
				}
			}
		}

		readonly Dictionary<ZInt, IEnumerable<IComplianceCommodity>> lightCommodities = new();
		readonly Lazy<ZString> commoditySource = new(() => Res.GetString("8EA1F4A4-A2D2-4D98-8F25-CED67BF5F259", "Brokerage"));

		void AddInvoiceLineTariffCode(List<IComplianceCommodity> commodities)
		{
			if (Declarations is not null)
			{
				var notFound = new List<ZInt>();
				var parentID = ((IComplianceItemRiskStatusProvider)this).ParentID;

				foreach (var declaration in Declarations)
				{
					if (declaration.IsInvoiceLinesLoaded)
					{
						var values = declaration.InvoiceLines
							.Cast<IBaseJobComInvoiceLine>()
							.Where((x) => !x.JI_Tariff.IsEmpty)
							.Select((x) => new ComplianceCommodity(groupingOrCountry: WorldCustomsOrganisationWCO, source: JS_UniqueConsignRef, parentJobID: parentID, commoditySource: commoditySource.Value, line: x));

						commodities.AddRange(values);
					}
					else if (lightCommodities.TryGetValue(declaration.JE_ClusterKey, out var values))
					{
						commodities.AddRange(values);
					}
					else
					{
						notFound.Add(declaration.JE_ClusterKey);
					}
				}

				if (notFound.Count > 0)
				{
					var lines = ComplianceRiskHelper.GetCommercialInvoiceLines(
						clusteredKeys: notFound,
						groupingOrCountry: WorldCustomsOrganisationWCO,
						source: JS_UniqueConsignRef,
						parentID,
						commoditySource: commoditySource.Value,
						factory: Factory);

					commodities.AddRange(lines.SelectMany((x) => x.Value));
					lines.ForEach((x) => lightCommodities.Add(x.Key, x.Value));
				}
			}
		}

		void AddSubShipmentCommodities(List<IComplianceCommodity> commodities)
		{
			var subForwardingShipments = CoLoadShipments.Where(shipment => shipment is ForwardingShipment);
			foreach (var shipment in subForwardingShipments)
			{
				var commoditiesFromShipments = ((IComplianceCommodityRiskStatusProvider)shipment).Commodities;
				commodities.AddRange(commoditiesFromShipments);
				commodities.AddRange(ObjectFactory.Get<IComplianceRiskStatusSupporter>().FetchCommodityDetailsInDB((IComplianceCommodityRiskStatusProvider)shipment));
			}
		}

		IEnumerable<IComplianceItemRiskStatusProvider> IComplianceItemRiskStatusProvider.SubComplianceRiskStatusProviders => CoLoadShipments.OfType<IComplianceItemRiskStatusProvider>();

		IEnumerable<IComplianceItemRiskStatusProvider> IComplianceItemRiskStatusProvider.ParentComplianceRiskStatusProviders =>
			(CoLoadMasterShipment is IComplianceItemRiskStatusProvider provider ? new[] { provider } : Enumerable.Empty<IComplianceItemRiskStatusProvider>())
			.Concat(base.Consols?.OfType<IComplianceItemRiskStatusProvider>() ?? Enumerable.Empty<IComplianceItemRiskStatusProvider>());

		ZDateTime IComplianceCommodityRiskStatusProvider.EffectiveDate
		{
			get
			{
				var result = DepartureConsol?.JK_DepartureForFirstExportTransport;

				if (!isResultValid())
				{
					result = JS_E_DEP;
				}

				if (!isResultValid())
				{
					result = JS_SystemCreateTimeUtc.ToLocalBranchTime();
				}

				if (!isResultValid())
				{
					result = ZDateTime.Now;
				}

				bool isResultValid()
				{
					return result != null && result.Value.IsValid;
				}

				return (ZDateTime)result;
			}
		}

		ZGuid IComplianceItemRiskStatusProvider.ParentID => PK;

		ZString IComplianceItemRiskStatusProvider.ParentTableCode => TablePrefix;

		ComplianceRiskSupport IComplianceItemRiskStatusProvider.ComplianceRiskSupport { get; } = ComplianceRiskSupport.FullySupported;

		Func<DocumentDeliveryResultForComplianceWorkflow> IComplianceItemRiskStatusProvider.InitializeComplianceWorkflowPopupIfNeeded { get; set; }

		(ZBool IsCurrent, ZDateTime JobEndDate) IComplianceItemRiskStatusProvider.JobTime => ComplianceRiskHelper.GetJobEndDateAndIsCurrent(Job?.JH_Status, JS_E_DEP,
			JS_E_ARV, this, Transports.Select(u => new ComplianceRouting { ETD = u.JW_ETD, ETA = u.JW_ETA, ATD = u.JW_ATD, ATA = u.JW_ATA }));

		ZBool IComplianceItemRiskStatusProvider.IsEnabledComplianceWise => ComplianceRiskHelper.IsFreightEnabledComplianceWise;

		ComplianceAssessmentPointPairInfo IComplianceCommodityRiskStatusProvider.AssessmentPointPairInfo
		{
			get
			{
				var pointPairs = new[] {
					new ComplianceCheckRequestPointPair
					{
						OriginPoint = new ComplianceCheckRequestPointPairLocation
						{
							Country = Origin?.RL_RN_NKCountryCode,
							UNLOCO = Origin?.RL_Code,
							MovementDescription = (NoResString)"Origin"
						},
						DestinationPoint = new ComplianceCheckRequestPointPairLocation
						{
							Country = Destination?.RL_RN_NKCountryCode,
							UNLOCO = Destination?.RL_Code,
							MovementDescription = (NoResString)"Destination"
						},
						EstimatedTimeOfArrival = JS_E_ARV,
						EstimatedTimeOfDeparture = JS_E_DEP,
						Mode = TransportMode
					}
				};

				return new ComplianceAssessmentPointPairInfo(pointPairs);
			}
		}

		CommodityRiskCalculateFactor IComplianceCommodityRiskStatusProvider.RiskCalculateFactor => CommodityRiskCalculateFactor.All;

		SecurityCheckpoint IComplianceItemRiskStatusProvider.AllowOverrideOverallRiskStatusSecurity => Env.Security.ShipmentsComplianceAllowOverrideOverallRiskStatus;

		SecurityCheckpoint IComplianceItemRiskStatusProvider.AllowResynchronizeRiskStatusSecurity => Env.Security.ShipmentsComplianceAllowResynchronizeRiskStatus;

		SecurityCheckpoint IComplianceItemRiskStatusProvider.AllowOverrideFreightMovementRestrictionsSecurity => Env.Security.ShipmentsComplianceAllowOverrideFreightMovementRestrictions;

		SecurityCheckpoint IComplianceCommodityRiskStatusProvider.EditHarmonizedCodeSecurity => Env.Security.ShipmentsComplianceEditHarmonizedCode;

		SecurityCheckpoint IComplianceCommodityRiskStatusProvider.EditComplianceAssessmentSecurity => Env.Security.ShipmentsComplianceEditComplianceAssessment;

		SecurityCheckpoint IComplianceCommodityRiskStatusProvider.AllowComplianceAssessmentSecurity => Env.Security.ShipmentsComplianceAllowComplianceAssessment;

		SecurityCheckpoint IComplianceCommodityRiskStatusProvider.DeclineComplianceAssessmentSecurity => Env.Security.ShipmentsComplianceDeclineComplianceAssessment;

		#endregion

		#region Unique Consol

		IForwardingConsol IUniqueConsolProvider.UniqueConsol
			=> Consols.Count == 1 ? Consols[0] : null;

		#endregion

		#region ICO2eProvider

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
				return (this as ICO2eLegBasedSupporter).RequireTEUForTransportMode(JS_TransportMode);
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

		ZGuid ICO2eParent.JobCO2eParentID => PK;

		ZString ICO2eParent.JobCO2eParentTableCode => TablePrefix;

		void ICO2eParent.RefreshCO2e()
		{
			RefreshTotalCO2eBinding();
			RefreshTotalCO2eForSorting();
		}

		void CO2eStatusChanged(object sender, EventArgs e)
		{
			RefreshTotalCO2eBinding();
			RefreshTotalCO2eForSorting();
		}

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
			if (HaveJobCO2e)
			{
				if (!IsValidationSuspended)
				{
					Validation.ValidateTotalCO2eForSorting();
				}
				TotalCO2eForSortingInfo.RefreshBinding();
			}
		}

		#endregion

		#region ICO2eLegBasedSupporter members

		ZString ICO2eLegBasedSupporter.TransportMode => this.TransportMode;

		ZDecimal ICO2eLegBasedSupporter.Weight => this.JS_ActualWeight;

		ZString ICO2eLegBasedSupporter.UnitOfWeight => this.ShipmentWeightUnit;

		RefUNLOCO ICO2eLegBasedSupporter.LoadPort => TransportsIncludingRelated.FirstLeg?.LoadPort ?? LoadPort ?? Origin;

		RefUNLOCO ICO2eLegBasedSupporter.DischargePort => TransportsIncludingRelated.LastLeg?.DiscPort ?? DischargePort ?? Destination;

		IPrePostCarriageLocation ICO2eLegBasedSupporter.LoadPortForCO2eCalc
		{
			get
			{
				var port = FirstMainCarriageLocation;
				if (port.IsEmpty)
				{
					var hblDeliveryModeToUse = FreightDataRegistry.Instance.UseHBLDeliveryModeForGHGCalculation.Value
						? JS_HBLContainerPackModeOverride
						: ZString.Empty;
					return (this as ICO2ePrePostCarriage).GetPreCarriageLocations(hblDeliveryModeToUse).LastOrDefault(location => !location.IsEmpty) ?? new PrePostCarriageLocationWrapper(ZString.Empty);
				}
				return port;
			}
		}

		IPrePostCarriageLocation ICO2eLegBasedSupporter.DischargePortForCO2eCalc
		{
			get
			{
				var port = LastMainCarriageLocation;
				if (port.IsEmpty)
				{
					var hblDeliveryModeToUse = FreightDataRegistry.Instance.UseHBLDeliveryModeForGHGCalculation.Value
						? JS_HBLContainerPackModeOverride
						: ZString.Empty;
					return (this as ICO2ePrePostCarriage).GetPostCarriageLocations(hblDeliveryModeToUse).FirstOrDefault(location => !location.IsEmpty) ?? new PrePostCarriageLocationWrapper(ZString.Empty);
				}
				return port;
			}
		}

		IPrePostCarriageLocation ICO2eLegBasedSupporter.AdditionalLoadPortForCO2eCalc => (this as ICO2eLegBasedSupporter).LoadPortForCO2eCalc;

		IPrePostCarriageLocation ICO2eLegBasedSupporter.AdditionalDischargePortForCO2eCalc => (this as ICO2eLegBasedSupporter).DischargePortForCO2eCalc;

		IPrePostCarriageLocation ICO2eLegBasedSupporter.ViaPortForCO2eCalc => new PrePostCarriageLocationWrapper(null);

		ZDateTime ICO2eLegBasedSupporter.ETA => JS_E_ARV;

		ZDateTime ICO2eLegBasedSupporter.ETD => JS_E_DEP;

		ZString ICO2eLegBasedSupporter.ContainerMode => JS_PackingMode;

		List<Transport> ConsolsTransports => TransportsIncludingRelated.Where(transport => transport.GetParentSafe() is ForwardingConsol).ToList();

		List<ICO2eLegProvider> ICO2eLegBasedSupporter.Legs => IsCalculatedAsPartOfPrePostCarriageLeg ? ConsolsTransports.Cast<ICO2eLegProvider>().ToList() : TransportsIncludingRelated.Cast<ICO2eLegProvider>().ToList();

		bool ICO2eLegBasedSupporter.SupportVirtualLegs => true;

		bool ICO2eLegBasedSupporter.ShouldPopulateCO2eForLegs => true;

		ZDecimal ICO2eLegBasedSupporter.GetNumberOfTEUForLeg(ICO2eLegProvider leg)
		{
			if (leg is Transport transport && transport.GetParentSafe() is ForwardingConsol consol)
			{
				return GetNumberOfTEUForConsol(consol);
			}

			return (this as ICO2eTEUProvider).NumberOfTEU;
		}

		IList<CO2eEmptyContainer> ICO2eLegBasedSupporter.EmptyContainers
		{
			get
			{
				var hasPickupFromHblDeliveryMode = !FreightDataRegistry.Instance.UseHBLDeliveryModeForGHGCalculation.Value
					|| JS_HBLContainerPackModeOverride.IsEmpty
					|| CO2eHelper.IsDoorPickup(JS_HBLContainerPackModeOverride)
					|| CO2eHelper.IsCFSPickup(JS_HBLContainerPackModeOverride);

				var hasReturnFromHblDeliveryMode = !FreightDataRegistry.Instance.UseHBLDeliveryModeForGHGCalculation.Value
					|| JS_HBLContainerPackModeOverride.IsEmpty
					|| CO2eHelper.IsDoorDelivery(JS_HBLContainerPackModeOverride)
					|| CO2eHelper.IsCFSDelivery(JS_HBLContainerPackModeOverride);

				var emptyContainers = new List<CO2eEmptyContainer>();
				foreach (ForwardingConsol consol in Consols)
				{
					var hasPickup = hasPickupFromHblDeliveryMode && !(DepartureConsol != null && consol.PK == DepartureConsol.PK && DepartureConsolHasPickupTransportBooking);
					var hasReturn = hasReturnFromHblDeliveryMode && !(ArrivalConsol != null && consol.PK == ArrivalConsol.PK && ArrivalConsolHasDeliveryTransportBooking);

					if (hasPickup || hasReturn)
					{
						emptyContainers.AddRange(OuterPackLines.Cast<PackLine>()
							.Select(packline => packline.GetContainer(consol))
							.WhereNotNull()
							.Cast<ICO2eEmptyContainerProvider>()
							.Select(container => new CO2eEmptyContainer(container, hasPickup, hasReturn)));
					}
				}

				return emptyContainers
					.GroupBy(x => ((BusinessObject)x.Container).PK)
					.Select(group => new CO2eEmptyContainer(group.First().Container, group.Any(h => h.HasPickup), group.Any(h => h.HasReturn)))
					.ToList();
			}
		}

		bool ICO2eLegBasedSupporter.RequiresTemperatureControl => OuterPackLines.OfType<PackLine>().Any(p => p.JL_RequiresTemperatureControl);

		bool HasContainersForTEU =>
			(JS_PackingMode == ContainerModes.FCL || JS_PackingMode == ContainerModes.ShippersConsol || JS_PackingMode == ContainerModes.BuyersConsol)
			&& Consols.Count != 0
			&& OuterPackLines.Count != 0
			&& AllPacked
			&& Consols.Cast<ForwardingConsol>().Any(consol => OuterPackLines.Cast<PackLine>().Any(packline =>
			{
				var container = packline.GetContainer(consol);
				return container != null && container.JC_Calc_TEUCount > 0 && packline.JL_ActualWeight > 0 && Weight.ContainsCode(packline.JL_ActualWeightUQ);
			}));

		public ZDecimal GetTotalEmptyContainerEmissions()
		{
			var emptyContainerEmissions = 0m;
			if ((this as ICO2eLegBasedSupporter).RequiresPrePostCarriageLegs)
			{
				foreach (var emptyContainer in (this as ICO2eLegBasedSupporter).EmptyContainers.Where(x => x.Container != null))
				{
					var container = emptyContainer.Container as ForwardingContainer;
					var packlineWeight = OuterPackLines.Cast<ForwardingPackLine>()
						.Where(x => x.Containers.Any(y => y.PK == container.PK))
						.Sum(x => x.JL_ActualWeight);
					var containerWeight = container.JC_Calc_TotalWeight;
					var weightRatio = containerWeight > 0 ? packlineWeight / containerWeight : 0;

					if (emptyContainer.HasPickup)
					{
						emptyContainerEmissions += emptyContainer.Container.GetTotalCO2e(CO2eTypes.EmptyPickup) * weightRatio;
					}

					if (emptyContainer.HasReturn)
					{
						emptyContainerEmissions += emptyContainer.Container.GetTotalCO2e(CO2eTypes.EmptyReturn) * weightRatio;
					}
				}
			}
			return emptyContainerEmissions;
		}

		bool ICO2eLegBasedSupporter.RequireTEUForTransportMode(string transportMode)
		{
			return transportMode switch
			{
				TransportModes.Sea when HasContainersForTEU
					=> true,

				TransportModes.Road or TransportModes.Rail when
					JS_PackingMode == ContainerModes.FCL
					&& Consols.Count != 0 && Consols.AllContainers.Count != 0
					&& (this as ICO2eLegBasedSupporter).Weight == 0
					=> true,

				_ => false,
			};
		}

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

			var hasTransport = TransportsIncludingRelated.Any();
			var loadPort = supporter.LoadPort;
			var dischargePort = supporter.DischargePort;

			AddReason(string.IsNullOrEmpty(supporter.TransportMode), Res.GetString("65beb06a-daa2-4386-999b-367f5b898dac", "Shipment > Basic Registration > Transport Mode"));
			AddReason(!supporter.IncludeTEU && supporter.Weight == 0, Res.GetString("cf7e0c4d-1b65-4e0c-8e17-abe976f4cfbd", "Shipment > Basic Registration > Weight"));

			AddReason(!hasTransport && loadPort is null, Res.GetString("61aa9ff9-934b-448d-86dd-3787fcd34a8c", "Shipment > Basic Registration > Origin (or Shipment > Additional Details > Planned Load)"));
			AddReason(!hasTransport && dischargePort is null, Res.GetString("30436a27-1e5e-4683-a034-4701089ca4dc", "Shipment > Basic Registration > Destination (or Shipment > Additional Details > Planned Discharge)"));
			AddReason(hasTransport && loadPort is null, Res.GetString("f3dda724-eeb9-4323-9347-2ba4d65df324", "Shipment > Basic Registration > Origin (or Shipment > Routing > Load)"));
			AddReason(hasTransport && dischargePort is null, Res.GetString("6439ad3b-0ed3-4c66-854b-d86e20aab301", "Shipment > Basic Registration > Destination (or Shipment > Routing > Discharge)"));

			AddReason(this.IsCountryEmptyWithValidCityOrPostcode(ExportReceivingDepot), Res.GetString("ca62e12d-f6f3-4c39-8791-b8ad6c618f64", "Shipment > Pickup > CFS (Ctry/Rgn. field is required with City field)"));
			AddReason(this.IsCountryEmptyWithValidCityOrPostcode(ConsignorPickupAddress), Res.GetString("54938407-b331-4864-92bc-3b9b44c6ca0b", "Shipment > Pickup > Pickup From (Ctry/Rgn. field is required with City field)"));

			AddReason(this.IsCountryEmptyWithValidCityOrPostcode(ImportReleaseDepot), Res.GetString("46a35744-b8d4-43ca-95bd-5bf441e800b1", "Shipment > Delivery > CFS (Ctry/Rgn. field is required with City field)"));
			AddReason(this.IsCountryEmptyWithValidCityOrPostcode(ConsigneeDeliveryAddress), Res.GetString("3913c861-8c14-4e6f-aeb3-e5c6dd2832ab", "Shipment > Delivery > Deliver To (Ctry/Rgn. field is required with City field)"));

			if (loadPort != null && dischargePort != null && loadPort.RL_Code == dischargePort.RL_Code)
			{
				AddReason(hasTransport, Res.GetString("3ec8e825-1b5a-45f2-a292-bd9b9f1f818e", "Shipment > Origin (or Planned Load or Routing > Load) and Destination (or Planned Discharge or Routing > Discharge) cannot be the same"));
				AddReason(!hasTransport, Res.GetString("aa72f1c0-f6ec-47b4-ac34-02510120d76a", "Shipment > Origin (or Planned Load) and Destination (or Planned Discharge) cannot be the same"));
			}

			var hasDuplicates = supporter.Legs.Cast<Transport>()
				.Where(x => !x.JW_RL_NKLoadPort.IsEmpty && !x.JW_RL_NKDiscPort.IsEmpty)
				.GroupBy(x => new { x.LoadPort, x.DiscPort })
				.Any(g => g.Count() > 1);

			AddReason(hasDuplicates, Res.GetString("cf7e0c4d-1b64-4e0b-8e17-aae976f4cfaa", "Shipment > Routing > Duplicate Load Ports and Discharge"));

			return reasons;
		}

		bool ICO2eCalculationSupporter.SaveEmissionsLogToNoteOnCalculated => FreightDataRegistry.Instance.AddEmissionsCalculationLogForShipment.Value;

		void ICO2eCalculationSupporter.OnRequested() => CO2eLegBasedSupporterHelper.OnRequested(this);

		void ICO2eCalculationSupporter.OnRejected(string reason) => CO2eLegBasedSupporterHelper.OnRejected(this, reason);

		void ICO2eCalculationSupporter.OnCalculated(bool succeeded)
		{
			if (succeeded)
			{
				DeleteJobCO2eForUnlinkedLegsIfNecessary();
			}
		}

		void DeleteJobCO2eForUnlinkedLegsIfNecessary()
		{
			foreach (Transport transport in TransportsIncludingRelated)
			{
				if (IsCalculatedAsPartOfPrePostCarriageLeg && transport.GetParentSafe() is not ForwardingConsol && !transport.JW_IsLinked)
				{
					var jobCO2e = (JobCO2e)transport.JobCO2eCollection.Get(default);
					if (jobCO2e != null)
					{
						((JobCO2eCollection)transport.JobCO2eCollection).Delete(jobCO2e);
					}
				}
			}
		}

		public AdditionalCalculationSupporter[] GetAdditionalCalculationSupporters(bool includeInactiveTBs)
		{
			var result = new List<AdditionalCalculationSupporter>();

			if (!FreightDataRegistry.Instance.EnablePrePostCarriageCalculation.Value)
			{
				return result.ToArray();
			}

			result.AddRange(this.GetTransportBookings(includeInactiveTBs: includeInactiveTBs)
				.Cast<ICO2eCalculationSupporter>()
				.Select(tb => new AdditionalCalculationSupporter(tb, () => tb.GetTotalCO2e())));

			var earliestConsol = Consols.GetEarliestConsol();
			if (earliestConsol != null)
			{
				var consolPickupTBs = earliestConsol.GetTransportBookings(DtbBookingDirection.PIC, includeInactiveTBs: includeInactiveTBs);
				result.AddRange(consolPickupTBs
					.Cast<ICO2eCalculationSupporter>()
					.Select(tb => new AdditionalCalculationSupporter(tb, () => GetApportionedValueFromConsolDtbBooking(tb, earliestConsol))));
			}

			var latestConsol = Consols.GetLatestConsol();
			if (latestConsol != null)
			{
				var consolDeliveryTBs = latestConsol.GetTransportBookings(DtbBookingDirection.DLV, includeInactiveTBs: includeInactiveTBs);
				result.AddRange(consolDeliveryTBs
					.Cast<ICO2eCalculationSupporter>()
					.Select(tb => new AdditionalCalculationSupporter(tb, () => GetApportionedValueFromConsolDtbBooking(tb, latestConsol))));
			}

			return result.ToArray();
		}

		AdditionalCalculationSupporter[] ICO2eCalculationSupporter.AdditionalCalculationSupporters => GetAdditionalCalculationSupporters(includeInactiveTBs: false);

		ZDecimal GetApportionedValueFromConsolDtbBooking(ICO2eCalculationSupporter dtbBooking, ForwardingConsol consol)
		{
			var shipmentWeight = Weight.ConvertSafe(JS_ActualWeight, JS_UnitOfWeight, consol.JK_TotalShipmentWeightUnit);
			return dtbBooking.GetTotalCO2e() * (shipmentWeight / consol.JK_TotalShipmentWeight);
		}

		void ICO2eCalculationSupporter.OnAdditionalSupporterCalculated(ICO2eCalculationSupporter additionalSupporter)
		{
			this.ApplyAdditionalCO2e(additionalSupporter);
		}

		bool IsCalculatedAsPartOfPrePostCarriageLeg => ((ICO2ePrePostCarriage)this).RequiresPrePostCarriageLegs && Consols.Count > 0;

		#endregion

		#region ICO2eTEUProvider

		bool ICO2eTEUProvider.IncludeTEU => HasContainersForTEU;

		ZDecimal ICO2eTEUProvider.NumberOfTEU => Consols.Cast<ForwardingConsol>().Average(consol => GetNumberOfTEUForConsol(consol));

		public ZDecimal GetNumberOfTEUForConsol(ForwardingConsol consol)
		{
			return OuterPackLines.Cast<PackLine>().Sum(packline =>
			{
				var packLineGoodWeightInKG = Weight.ConvertSafe(packline.JL_ActualWeight, packline.JL_ActualWeightUQ, Weight.Kilograms);

				var container = packline.GetContainer(consol);
				if (container == null)
				{
					return 0m;
				}

				var containerGoodWeightInKG = container.JC_Calc_TotalWeightInKgs;
				if (containerGoodWeightInKG == 0m)
				{
					return 0m;
				}

				var teuCount = container.JC_Calc_TEUCount;
				var ratio = packLineGoodWeightInKG / containerGoodWeightInKG;
				return teuCount * ratio;
			});
		}

		ZDecimal ICO2eTEUProvider.TonnesPerTEU
		{
			get
			{
				var teuCount = ((ICO2eLegBasedSupporter)this).NumberOfTEU;
				if (teuCount == 0)
				{
					return 0;
				}
				return Weight.ConvertSafe(JS_ActualWeight, JS_UnitOfWeight, Weight.Tonnes) / teuCount;
			}
		}

		ZDecimal ICO2eTEUProvider.ContainerEmptyWeightPerTEU => Consols.Cast<ICO2eLegBasedSupporter>().Average(consol => consol.ContainerEmptyWeightPerTEU);

		ZString ICO2eTEUProvider.ContainerEmptyWeightPerTEUUnit => Weight.Kilograms;

		bool AllPacked => Consols.Cast<ForwardingConsol>().All(consol => OuterPackLines.Cast<PackLine>().All(packline => packline.GetContainer(consol) != null));

		#region Pre/Post Carriage

		bool DepartureConsolHasPickupTransportBooking
		{
			get
			{
				return DepartureConsol?.HasTransportBooking(DtbBookingDirection.PIC) ?? false;
			}
		}

		bool ArrivalConsolHasDeliveryTransportBooking
		{
			get
			{
				return ArrivalConsol?.HasTransportBooking(DtbBookingDirection.DLV) ?? false;
			}
		}

		bool ICO2ePrePostCarriage.RequiresPrePostCarriageLegs
		{
			get
			{
				return FreightDataRegistry.Instance.EnablePrePostCarriageCalculation.Value;
			}
		}

		IPrePostCarriageLocation[] ICO2ePrePostCarriage.GetPreCarriageLocations(ZString hblDeliveryMode)
		{
			if (this.HasTransportBooking(DtbBookingDirection.PIC))
			{
				return Array.Empty<PrePostCarriageLocationWrapper>();
			}

			var port = Consols.Count > 0
				? new PrePostCarriageLocationWrapper(ZString.Empty)
				: FirstMainCarriageLocation;

			if (hblDeliveryMode.IsEmpty || CO2eHelper.IsDoorPickup(hblDeliveryMode))
			{
				return new[]
				{
					new PrePostCarriageLocationWrapper(ConsignorPickupAddress.E2_AddressOverride ? ConsignorPickupAddress : ConsignorPickupAddress.Address, PickupByTransportMode),
					new PrePostCarriageLocationWrapper(ExportReceivingDepot, CFSDepartureByTransportMode),
					port
				};
			}
			else if (CO2eHelper.IsCFSPickup(hblDeliveryMode) && Consols.Count == 0)
			{
				return new[]
				{
					new PrePostCarriageLocationWrapper(ExportReceivingDepot, CFSDepartureByTransportMode),
					port
				};
			}

			return Array.Empty<PrePostCarriageLocationWrapper>();
		}

		IPrePostCarriageLocation FirstMainCarriageLocation => new PrePostCarriageLocationWrapper((TransportsIncludingRelated.FirstLeg?.JW_RL_NKLoadPort ?? ZString.Empty)
																								.FallbackIfEmpty(JS_RL_NKLoadPort));

		IPrePostCarriageLocation[] ICO2ePrePostCarriage.GetPostCarriageLocations(ZString hblDeliveryMode)
		{
			if (this.HasTransportBooking(DtbBookingDirection.DLV))
			{
				return Array.Empty<PrePostCarriageLocationWrapper>();
			}

			var port = Consols.Count > 0
				? new PrePostCarriageLocationWrapper(ZString.Empty)
				: LastMainCarriageLocation;

			if (hblDeliveryMode.IsEmpty || CO2eHelper.IsDoorDelivery(hblDeliveryMode))
			{
				return new[]
				{
					port,
					new PrePostCarriageLocationWrapper(ImportReleaseDepot, CFSArrivalByTransportMode),
					new PrePostCarriageLocationWrapper(ConsigneeDeliveryAddress.E2_AddressOverride ? ConsigneeDeliveryAddress : ConsigneeDeliveryAddress.Address, DeliveryByTransportMode)
				};
			}
			else if (CO2eHelper.IsCFSDelivery(hblDeliveryMode) && Consols.Count == 0)
			{
				return new[]
				{
					port,
					new PrePostCarriageLocationWrapper(ImportReleaseDepot, CFSArrivalByTransportMode),
				};
			}

			return Array.Empty<PrePostCarriageLocationWrapper>();
		}

		IPrePostCarriageLocation LastMainCarriageLocation => new PrePostCarriageLocationWrapper((TransportsIncludingRelated.LastLeg?.JW_RL_NKDiscPort ?? ZString.Empty)
																								.FallbackIfEmpty(JS_RL_NKDischargePort));

		void ICO2ePrePostCarriage.OnTransportBookingCalculated(IDtbBooking dtbBooking)
		{
			((ICO2eCalculationSupporter)this).OnAdditionalSupporterCalculated((ICO2eCalculationSupporter)dtbBooking);
		}

		void ICO2ePrePostCarriage.OnTransportBookingCO2eStatusChanged(IDtbBooking dtbBooking)
		{
			if (((ICO2eProvider)dtbBooking).GetCO2eStatus() == CO2eStatusList.Codes.NotCurrent)
			{
				this.UpdateCO2eStatusToNotCurrent();
			}
		}

		void ICO2ePrePostCarriage.OnTransportBookingActiveStatusChanged(IDtbBooking dtbBooking)
		{
			this.UpdateCO2eStatusToNotCurrent();
		}

		ISupportWebAddressValidation[] IAddressesValidation.AddressesToValidate
		{
			get
			{
				if (!((ICO2ePrePostCarriage)this).RequiresPrePostCarriageLegs)
				{
					return Array.Empty<ISupportWebAddressValidation>();
				}

				return new ISupportWebAddressValidation[]
					{
						ConsignorPickupAddress.E2_AddressOverride ? ConsignorPickupAddress : ConsignorPickupAddress.Address,
						ExportReceivingDepot, ImportReleaseDepot,
						ConsigneeDeliveryAddress.E2_AddressOverride ? ConsigneeDeliveryAddress : ConsigneeDeliveryAddress.Address
					}
					.Concat(GetAddressesForEmptyContainers())
					.Concat(GetAddrressesForConsols())
					.Where(address => address != null)
					.GroupBy(x => x.EntityPK)
					.Select(x => x.FirstOrDefault())
					.ToArray();
			}
		}

		IEnumerable<ISupportWebAddressValidation> GetAddressesForEmptyContainers()
		{
			return (this as ICO2eLegBasedSupporter).EmptyContainers
				.Where(x => x.Container != null)
				.SelectMany(emptyContainer =>
				{
					var (pickupFromAddress, pickupToAddress, _) = emptyContainer.Container.EmptyPickupAddress(this);
					var (returnFromAddress, returnToAddress, _) = emptyContainer.Container.EmptyReturnAddress(this);
					return new[] { pickupFromAddress, pickupToAddress, returnFromAddress, returnToAddress };
				});
		}

		IEnumerable<ISupportWebAddressValidation> GetAddrressesForConsols()
		{
			return Consols.Cast<IAddressesValidation>().SelectMany(consol => consol.AddressesToValidate);
		}

		#endregion

		#endregion

		#region IShouldPackTrackedPackagesViaDivot

		bool IShouldPackTrackedPackagesViaDivot.ShouldPackTrackedPackagesViaDivot => true;

		#endregion

		#region IsEditingElectronicBOL

		public ZBool IsEditingElectronicBOL
		{
			get; set;
		}

		#endregion

		#region Captions

		public ResourceStringData PrepaidBillToPartyCaption => Res.GetData("F28F37D1-D8E1-49A8-B0B5-7527A60BC33E", "Prepaid Bill-To Party", "This field defaults the job's Consignor. If Cross Trade jobs are not configured to bill the job's Controlling Party, then the Prepaid Bill-To Party (or their IFT party if they have one) will default as the charge line debtor for prepaid charges on this job.");

		public ResourceStringData LocalClientCaption => Res.GetData("E7D42E1A-7D1B-4D7D-BA81-02F923E94CD2", "Local Client");

		public MultilingualString PrepaidBillToPartyText => ResString.GetMultilingualString("26D68249-6176-4DE1-8E3C-88C1BF729675", "Prepaid Bill-To Party");

		public MultilingualString LocalClientText => ResString.GetMultilingualString("8E4C8059-9891-45E1-BB8C-BE3A6AAE4DD6", "Local Client");

		#endregion

		#region IRegisterStatusChangeMode

		IStmALog triggeringEvent;

		IDisposable IRegisterStatusChangeContext.TemporarilySetStatusChangedByTriggerEvent(IStmALog triggeringEvent)
		{
			currentDeliveryDueDateCalculationMode = DeliveryDueDateCalculationMode.WorkflowTrigger;
			currentRevisedDeliveryDueDateCalculationMode = DeliveryDueDateCalculationMode.WorkflowTrigger;
			this.triggeringEvent = triggeringEvent;

			return new DisposableAction(() =>
			{
				currentDeliveryDueDateCalculationMode = default;
				currentRevisedDeliveryDueDateCalculationMode = default;
				this.triggeringEvent = null;
			});
		}

		#endregion

		#region Inspection

		public IEnumerable<string> PackLineInspectionList
		{
			get
			{
				var result = new List<string>();
				if (JS_ShipmentType == Constants.ShipmentTypes.HighVolumeLowValueMaster)
				{
					foreach (ForwardingPackLine packLine in CoLoadShipments.Where(shipment => shipment.JS_ShipmentType == Constants.ShipmentTypes.HighVolumeLowValue).SelectMany(s => s.OuterPackLines))
					{
						result.Add(packLine.JL_InspectionTypeCode);
					}
				}
				else
				{
					foreach (ForwardingPackLine packLine in OuterPackLines)
					{
						result.Add(packLine.JL_InspectionTypeCode);
					}
				}

				return result;
			}
		}

		public IEnumerable<string> PackLineAdditionalInspectionList
		{
			get
			{
				var result = new List<string>();
				if (JS_ShipmentType == Constants.ShipmentTypes.HighVolumeLowValueMaster)
				{
					foreach (ForwardingPackLine packLine in CoLoadShipments.Where(shipment => shipment.JS_ShipmentType == Constants.ShipmentTypes.HighVolumeLowValue).SelectMany(s => s.OuterPackLines.Where(p => p.JL_IsHighRisk)))
					{
						result.Add(packLine.JL_AdditionalInspectionTypeCode);
					}
				}
				else
				{
					foreach (ForwardingPackLine packLine in OuterPackLines.Where(p => p.JL_IsHighRisk))
					{
						result.Add(packLine.JL_AdditionalInspectionTypeCode);
					}
				}

				return result;
			}
		}

		public ZString GetEditInspectionErrorForUncertifiedUser()
		{
			return AviationSecurity.SupplyChainSecurityConfiguration.GetEditInspectionErrorForUncertifiedUser(this);
		}

		#endregion

		#region IGlobalSearchBusinessObjectProvider

		BusinessObject IGlobalSearchBusinessObjectProvider.BusinessObjectForController => JS_IsBooking && !JS_IsForwardRegistered ? ObjectFactory.Get<IQuotedBookingBuilder>().LoadViewQuotedBooking(Factory, PK) : this;

		#endregion

		public void AddSPTRefNumber(ZString refNumber)
		{
			var sptNumber = this.Numbers.AddNew();
			sptNumber.CE_EntryType = ShipmentNonCustomsAdditionalReferenceCodesCodeList.Codes.SPT;
			sptNumber.CE_EntryNum = refNumber;
			sptNumber.CE_RN_NKCountryCode = string.Empty;
		}

		#region TranportModeBindings

		[List("PreCarriageOnCarriageTransportMode_List")]
		[MaxLength(3)]
		public override ZString PickupByTransportMode
		{
			get
			{
				return base.PickupByTransportMode;
			}
			set
			{
				var previousValue = base.PickupByTransportMode;
				if (previousValue != value)
				{
					base.PickupByTransportMode = value;
					this.UpdateCO2eStatusToNotCurrent(new CO2eStatusChangedReason(PickupByTransportModeInfo, previousValue), IsCopying);
				}
			}
		}

		[List("PreCarriageOnCarriageTransportMode_List")]
		[MaxLength(3)]
		public override ZString CFSDepartureByTransportMode
		{
			get
			{
				return base.CFSDepartureByTransportMode;
			}
			set
			{
				var previousValue = base.CFSDepartureByTransportMode;
				if (previousValue != value)
				{
					base.CFSDepartureByTransportMode = value;
					this.UpdateCO2eStatusToNotCurrent(new CO2eStatusChangedReason(CFSDepartureByTransportModeInfo, previousValue), IsCopying);
				}
			}
		}

		[List("PreCarriageOnCarriageTransportMode_List")]
		[MaxLength(3)]
		public override ZString DeliveryByTransportMode
		{
			get
			{
				return base.DeliveryByTransportMode;
			}
			set
			{
				var previousValue = base.DeliveryByTransportMode;
				if (previousValue != value)
				{
					base.DeliveryByTransportMode = value;
					this.UpdateCO2eStatusToNotCurrent(new CO2eStatusChangedReason(DeliveryByTransportModeInfo, previousValue), IsCopying);
				}
			}
		}

		[List("PreCarriageOnCarriageTransportMode_List")]
		[MaxLength(3)]
		public override ZString CFSArrivalByTransportMode
		{
			get
			{
				return base.CFSArrivalByTransportMode;
			}
			set
			{
				var previousValue = base.CFSArrivalByTransportMode;
				if (previousValue != value)
				{
					base.CFSArrivalByTransportMode = value;
					this.UpdateCO2eStatusToNotCurrent(new CO2eStatusChangedReason(CFSArrivalByTransportModeInfo, previousValue), IsCopying);
				}
			}
		}

		#endregion

		#region CO2e

		void UpdateContainersCO2eOnConsigneeAddressChanged()
		{
			var isDeliveryCFSAddressEmpty = JS_OA_ImportReleaseDepot == ZGuid.Empty;
			if (JS_PackingMode == Constants.ContainerModes.FCL && IsDirectShipment && isDeliveryCFSAddressEmpty)
			{
				Containers.ForEach(c => (c as ForwardingContainer)?.UpdateCO2eStatusToNotCurrent(new CO2eStatusChangedReason((NoResString)"Shipment Consignee Delivery Address"), IsCopying, CO2eTypes.EmptyReturn));
			}
		}

		void UpdateContainersCO2eStatusOnJS_OA_ExportReceivingDepotChanged()
		{
			if (JS_PackingMode == Constants.ContainerModes.FCL && IsDirectShipment)
			{
				Containers.ForEach(c => (c as ForwardingContainer).UpdateCO2eStatusToNotCurrent(new CO2eStatusChangedReason((NoResString)"Shipment Pickup CFS Address"), IsCopying, CO2eTypes.EmptyPickup));
			}
		}

		void UpdateContainersCO2eStatusOnJS_OA_ImportReleaseDepotChanged()
		{
			if (JS_PackingMode == Constants.ContainerModes.FCL && IsDirectShipment)
			{
				Containers.ForEach(c => (c as ForwardingContainer).UpdateCO2eStatusToNotCurrent(new CO2eStatusChangedReason((NoResString)"Shipment Delivery CFS Address"), IsCopying, CO2eTypes.EmptyReturn));
			}
		}

		void UpdateContainersCO2eStatusOnConsignorPickupAddressChanged()
		{
			var isPickUpCFSAddressEmpty = JS_OA_ExportReceivingDepot == ZGuid.Empty;
			if (JS_PackingMode == Constants.ContainerModes.FCL && IsDirectShipment && isPickUpCFSAddressEmpty)
			{
				Containers.ForEach(c => (c as ForwardingContainer).UpdateCO2eStatusToNotCurrent(new CO2eStatusChangedReason((NoResString)"Shipment Consignor Pickup Address"), IsCopying, CO2eTypes.EmptyPickup));
			}
		}

		#endregion

		#region Operational Actions

		public void CreateValueIfNull(PropertyInfo info)
		{
			if (info.Name == nameof(JobHeader) && JobHeader == null || info.Name == nameof(Job) && Job == null)
			{
				CreateJobHeaderWithMutex();
			}
		}

		#endregion

		public ICusExitHeader[] ExitHeaders => Factory.GetValue(ref exitHeadersCached, () =>
		{
			if (!this.IsExport())
			{
				return Array.Empty<ICusExitHeader>();
			}

			var exitHeaderLoader = ObjectFactory.Get<ICusExitHeaderLoader>("EUExitControl.ICusExitHeaderLoader", Factory);
			return exitHeaderLoader.Load(!IsInDatabase, PK, TablePrefix);
		});
		CachedProperty<ICusExitHeader[]> exitHeadersCached;

		public ICusExitReport[] ExitReports => Factory.GetValue(ref exitReportsCached, () =>
		{
			if (!this.IsExport())
			{
				return Array.Empty<ICusExitReport>();
			}

			ICusExitReport[] exitReports = null;
			var exitHeaders = ExitHeaders;
			if (exitHeaders.Length > 0)
			{
				var query = new ZQuery(CusExitReportSchema.CER_ClusterKey, exitHeaders.Select(x => x.CXH_ClusterKey));
				query.FetchOnlyFromLocalCache = !IsInDatabase;
				exitReports = Factory.Load<ICusExitReport>(query);
			}
			return exitReports ?? Array.Empty<ICusExitReport>();
		});
		CachedProperty<ICusExitReport[]> exitReportsCached;

		[ResourceStringData("C68BDB95-38E4-470D-B2DC-94B412ECBD9F", Caption = "Exit Status", MediumCaption = "Exit Stat.", ShortCaption = "Exit St.")]
		public ZString ExitStatus => ExitReports?.GetSingleValueOrManyText(e => e.CER_Status, CommonEntryStatusList.Codes.MultipleEntryStatus) ?? ZString.Empty;

		[ResourceStringData("47BE8EA7-AF63-49FB-B852-75DBFF924BFB", Caption = "Exit Status Description", MediumCaption = "Exit Status Desc.", ShortCaption = "Exit St. Desc.")]
		public ZString ExitStatusDescription => ExitReports?.GetSingleValueOrManyText(e => ExitStatusDescriptionFromCode(e.CER_Status), e => e.CER_Status, CommonEntryStatusList.Descriptions.MultipleEntryStatus) ?? ZString.Empty;

		ZString ExitStatusDescriptionFromCode(ZString statusCode) => ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, statusCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode
			, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExportCustomsStatus, ZDate.Today, includeParentDataGrouping: false)?.ZZD_Description ?? ZString.Empty;

		#region JS_ElectronicBillOfLadingTerms

		[List("Lookups.JS_ElectronicBillOfLadingTerms_List")]
		public override ZString JS_ElectronicBillOfLadingTerms
		{
			get { return base.JS_ElectronicBillOfLadingTerms; }
			set { base.JS_ElectronicBillOfLadingTerms = value; }
		}

		#endregion

		#region JS_ElectronicBillOfLadingType

		[List("Lookups.JS_ElectronicBillOfLadingType_List")]
		public override ZString JS_ElectronicBillOfLadingType
		{
			get { return base.JS_ElectronicBillOfLadingType; }
			set
			{
				base.JS_ElectronicBillOfLadingType = value;
				if (value == Constants.BillOfLadingBillType.Codes.BlankEndorse
					|| value == Constants.BillOfLadingBillType.Codes.ToOrder)
				{
					JS_ElectronicBillOfLadingTerms = Constants.BillOfLadingBillTerms.Codes.Transferable;
				}
				else if (value == Constants.BillOfLadingBillType.Codes.Straight)
				{
					JS_ElectronicBillOfLadingTerms = Constants.BillOfLadingBillTerms.Codes.NonTransferable;
				}

				JS_ElectronicBillOfLadingConsigneeForBindingInfo.RefreshBinding();
			}
		}

		#endregion

		#region JS_ElectronicBillOfLadingConsigneeForBinding

		public ZString JS_ElectronicBillOfLadingConsigneeForBinding
		{
			get
			{
				if (JS_ElectronicBillOfLadingType == Constants.BillOfLadingBillType.Codes.Straight)
				{
					return ZString.Empty;
				}
				else if (JS_ElectronicBillOfLadingType == Constants.BillOfLadingBillType.Codes.ToOrder)
				{
					return Res.GetString("3C67A2CF-EF4B-40EA-9FD2-4B56DAA079A2", "To Order");
				}
				else if (JS_ElectronicBillOfLadingType == Constants.BillOfLadingBillType.Codes.BlankEndorse)
				{
					return ZString.Empty;
				}

				return ZString.Empty;
			}
		}

		public ZPropertyInfo JS_ElectronicBillOfLadingConsigneeForBindingInfo => GetZPropertyInfo(nameof(JS_ElectronicBillOfLadingConsigneeForBinding));

		#endregion

		#region JS_ElectronicBillOfLadingAmendmentRequestDetailsForBinding

		public ZString JS_ElectronicBillOfLadingAmendmentRequestDetailsForBinding
		{
			get
			{
				var details = ZString.Empty;
				switch (JS_ElectronicBillOfLadingStatus)
				{
					case FreightConstants.BillOfLadingBillStatus.Codes.OriginalBillAmendmentInProgress:
						details = GetBluNotificationDetails(Core.Constants.BillStatusUpdatedTypes.DenyAmendmentRejected);
						if (details.IsEmpty)
						{
							details = GetBluNotificationDetails(Core.Constants.BillStatusUpdatedTypes.AmendmentRequested);
						}
						break;
					case FreightConstants.BillOfLadingBillStatus.Codes.SwitchedToPaper:
						details = GetBluNotificationDetails(Core.Constants.BillStatusUpdatedTypes.SwitchedToPaper);
						break;
					case FreightConstants.BillOfLadingBillStatus.Codes.PublishingRejected:
						details = GetBluNotificationDetails(Core.Constants.BillStatusUpdatedTypes.OriginalBillNotPublished);
						break;
					case FreightConstants.BillOfLadingBillStatus.Codes.Surrendered:
						details = GetBluNotificationDetails(Core.Constants.BillStatusUpdatedTypes.Surrendered);
						break;
				}
				return details;
			}
		}

		public ZPropertyInfo JS_ElectronicBillOfLadingAmendmentRequestDetailsForBindingInfo => GetZPropertyInfo(nameof(JS_ElectronicBillOfLadingAmendmentRequestDetailsForBinding));

		#endregion

		#region JS_ElectronicBillOfLadingStatus

		[ReadOnly(true)]
		[List("Lookups.JS_ElectronicBillOfLadingBillStatus_List")]
		public override ZString JS_ElectronicBillOfLadingStatus
		{
			get { return base.JS_ElectronicBillOfLadingStatus; }
			set { base.JS_ElectronicBillOfLadingStatus = value; }
		}

		public void RevertToPreviousEHBLStatus()
		{
			var eventLog = GetLatestAmendmentRequestedEHBLStatusEventLog();

			if (eventLog != null)
			{
				var previousBillStatusEventLog = this.Logs.GetAllLogs().OfType<StmALog>()
					.Where(log => log.SL_PostedTimeUtc < eventLog.SL_PostedTimeUtc && log.IsCancelled == ZBool.False && log.SL_SE_NKEvent == Events.BillStatusUpdatedCode
									&& log.Parameters.TryGetValue(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, out var department) && department == ElectronicBOLConstants.EHBLEventDepartments.TitleRegistry
									&& log.Parameters.TryGetValue(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, out var eventType) && eventType != BillStatusUpdatedTypes.AmendmentRequested
									&& !ConvertBillStatusUpdatedTypesToEHBLStatus(eventType).IsEmpty).OrderByDescending(log => log.SL_PostedTimeUtc).FirstOrDefault();

				if (previousBillStatusEventLog != null && previousBillStatusEventLog.Parameters.TryGetValue(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, out var eventType))
				{
					JS_ElectronicBillOfLadingStatus = ConvertBillStatusUpdatedTypesToEHBLStatus(eventType);
				}
			}
		}

		public StmALog GetLatestAmendmentRequestedEHBLStatusEventLog()
		{
			return this.Logs.MostRecentLogByPostedTime(Events.BillStatusUpdated, log => log.Parameters.TryGetValue(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, out var eventType)
				&& log.Parameters.TryGetValue(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, out var department)
				&& eventType == BillStatusUpdatedTypes.AmendmentRequested && department == ElectronicBOLConstants.EHBLEventDepartments.TitleRegistry);
		}

		ZString ConvertBillStatusUpdatedTypesToEHBLStatus(string billStatusUpdatedType)
		{
			switch (billStatusUpdatedType)
			{
				case BillStatusUpdatedTypes.OriginalBillPublished:
				case BillStatusUpdatedTypes.DenyAmendmentAccepted:
					return FreightConstants.BillOfLadingBillStatus.Codes.OriginalBillPublished;
				case BillStatusUpdatedTypes.AmendmentRequested:
				case BillStatusUpdatedTypes.DenyAmendmentRejected:
					return FreightConstants.BillOfLadingBillStatus.Codes.OriginalBillAmendmentInProgress;
				case BillStatusUpdatedTypes.OriginalBillTransferred:
					return FreightConstants.BillOfLadingBillStatus.Codes.OriginalBillTransferred;
				case BillStatusUpdatedTypes.Surrendered:
					return FreightConstants.BillOfLadingBillStatus.Codes.Surrendered;
				case BillStatusUpdatedTypes.SwitchedToPaper:
					return FreightConstants.BillOfLadingBillStatus.Codes.SwitchedToPaper;
				case BillStatusUpdatedTypes.OriginalBillNotPublished:
					return FreightConstants.BillOfLadingBillStatus.Codes.PublishingRejected;
			}
			return ZString.Empty;
		}

		#endregion

		#region GetBluNotificationDetails

		ZString GetBluNotificationDetails(ZString bluType)
		{
			var latestBluLog = this.Logs.MostRecentLogByEventTime(Events.BillStatusUpdated,
						log => log.Parameters.TryGetValue(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, out var type)
						&& type == bluType
						&& log.Parameters.TryGetValue(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, out var department)
						&& department == ElectronicBOLConstants.EHBLEventDepartments.TitleRegistry);

			var universalEvent = latestBluLog?.RelatedEDIMessage?.Message.GetEM_MessageTextReader().Parse<UniversalEvent>();
			if (universalEvent?.ContextCollection != null)
			{
				var notificationDetails = universalEvent.ContextCollection.FirstOrDefault(c => c.Type == "NotificationDetails");
				if (notificationDetails != null && notificationDetails.Value.HasValue)
				{
					return string.Join("\r\n", notificationDetails.Value.Value.Split('\r', '\n').ToList().Select(x => x.Trim()).Where(x => !string.IsNullOrEmpty(x)));
				}
			}

			return ZString.Empty;
		}

		#endregion

		#region EnabledElectronicBOL

		public bool EnabledElectronicBOL
		{
			get
			{
				return IsSea &&
					!JS_HouseBill.IsEmpty &&
					Env.Security.MaintainShipmentAllowPublisheHBL.IsAllowed &&
					FreightDataRegistry.Instance.EnableBoleroEHBLIntegration.Value.EnableEBLIntegration &&
					IsOriginalBillRequired() &&
					!TRIRecordHelper.GetTRIRecord().IsEmpty;
			}
		}

		bool IsOriginalBillRequired()
		{
			if (JS_ReleaseType == ShipmentReleaseTypes.ExpressBofL
				|| JS_ReleaseType == ShipmentReleaseTypes.NonNegotiable
				|| JS_ReleaseType == ShipmentReleaseTypes.SeaWaybill)
			{
				return IsInDatabase && !JS_ElectronicBillOfLadingStatus.IsEmpty;
			}

			return true;
		}

		public ZBool CheckElectronicBOLMinimumRequirements()
		{
			if (EnabledElectronicBOL)
			{
				HolderDocAddress.Validation.ValidateAll();
				SurrenderPartyDocAddress.Validation.ValidateAll();
				ShipperDocAddress.Validation.ValidateAll();
				JS_ElectronicBillOfLadingConsigneeDocAddress.Validation.ValidateAll();
				JS_ElectronicBillOfLadingToOrderDocAddress.Validation.ValidateAll();
				Validation.ValidateJS_ElectronicBillOfLadingType();
				Validation.ValidateJS_ElectronicBillOfLadingTerms();

				return !HolderDocAddress.OrganisationPKInfo.HasErrors() && !HolderDocAddress.OrganisationPKInfo.HasMessageErrors()
					&& !SurrenderPartyDocAddress.OrganisationPKInfo.HasErrors() && !SurrenderPartyDocAddress.OrganisationPKInfo.HasMessageErrors()
					&& !ShipperDocAddress.OrganisationPKInfo.HasErrors() && !ShipperDocAddress.OrganisationPKInfo.HasMessageErrors()
					&& !JS_ElectronicBillOfLadingConsigneeDocAddress.OrganisationPKInfo.HasErrors() && !JS_ElectronicBillOfLadingConsigneeDocAddress.OrganisationPKInfo.HasMessageErrors()
					&& !JS_ElectronicBillOfLadingToOrderDocAddress.OrganisationPKInfo.HasErrors() && !JS_ElectronicBillOfLadingToOrderDocAddress.OrganisationPKInfo.HasMessageErrors()
					&& !JS_ElectronicBillOfLadingTypeInfo.HasErrors() && !JS_ElectronicBillOfLadingTypeInfo.HasMessageErrors()
					&& !JS_ElectronicBillOfLadingTermsInfo.HasErrors() && !JS_ElectronicBillOfLadingTermsInfo.HasMessageErrors();
			}

			return false;
		}

		#endregion

		#region ShipperDocAddress

		protected override JobDocAddressRequirement GetConsignorDocAddressRequirement()
		{
			var requirement = base.GetConsignorDocAddressRequirement();
			if (ShipperDocAddress != null)
			{
				requirement.AddLinkedRequirement(ShipperDocAddress.Requirement);
			}

			return requirement;
		}

		public JobDocAddress ShipperDocAddress
		{
			get
			{
				if (EnabledElectronicBOL && fShipperDocAddress == null)
				{
					fShipperDocAddress = DocAddresses.FindOrCreateWithRequirement(ShipperDocAddressRequirement);
					fShipperDocAddress.SetReadOnlyIncludingChildren(true);
					fShipperDocAddress.MakeNonPersistent();
					DefaultShipperDocAddress();
				}

				return fShipperDocAddress;
			}
		}
		JobDocAddress fShipperDocAddress;

		void DefaultShipperDocAddress()
		{
			if (!ConsignorDocumentaryAddress.E2_AddressOverride)
			{
				ShipperDocAddress.OrganisationPK = ConsignorDocumentaryAddress.OrganisationPK;
			}
			else
			{
				foreach (ZString field in GetAddressFields(ShipperDocAddress))
				{
					ShipperDocAddress[field] = ConsignorDocumentaryAddress[field];
				}

				foreach (ZString field in GetContactFields(ShipperDocAddress))
				{
					ShipperDocAddress[field] = ConsignorDocumentaryAddress[field];
				}
			}
		}

		public JobDocAddressRequirement ShipperDocAddressRequirement
		{
			get
			{
				if (fShipperDocAddressRequirement == null)
				{
					fShipperDocAddressRequirement = new JobDocAddressRequirement(DocAddressType.Shipper, AddressType.OFC, ContactType.Consignor);
					fShipperDocAddressRequirement.CanOverride = false;
					fShipperDocAddressRequirement.ValidateOrganisationPK = Validation.ValidateShipperPK;
				}
				return fShipperDocAddressRequirement;
			}
		}
		JobDocAddressRequirement fShipperDocAddressRequirement;

		#endregion

		List<ZString> GetContactFields(JobDocAddress jobDocAddress)
		{
			List<ZString> result = new List<ZString>();
			result.Add(jobDocAddress.E2_ContactInfo.Name);
			result.Add(jobDocAddress.E2_EmailInfo.Name);
			result.Add(jobDocAddress.E2_FaxInfo.Name);
			result.Add(jobDocAddress.E2_MobileInfo.Name);
			result.Add(jobDocAddress.E2_PhoneInfo.Name);
			return result;
		}

		List<ZString> GetAddressFields(JobDocAddress jobDocAddress)
		{
			List<ZString> result = new List<ZString>();
			result.Add(jobDocAddress.E2_AdditionalAddressInformationInfo.Name);
			result.Add(jobDocAddress.E2_Address1Info.Name);
			result.Add(jobDocAddress.E2_Address2Info.Name);
			result.Add(jobDocAddress.E2_CityInfo.Name);
			result.Add(jobDocAddress.E2_PostcodeInfo.Name);
			result.Add(jobDocAddress.E2_RN_NKCountryCodeInfo.Name);
			result.Add(jobDocAddress.E2_StateInfo.Name);
			result.Add(jobDocAddress.E2_AddressOverrideInfo.Name);
			result.Add(jobDocAddress.E2_CompanyNameInfo.Name);
			return result;
		}

		#region JS_ElectronicBillOfLadingConsigneeDocAddress

		protected override JobDocAddressRequirement GetConsigneeDocAddressRequirement()
		{
			var requirement = base.GetConsigneeDocAddressRequirement();
			if (JS_ElectronicBillOfLadingConsigneeDocAddress != null)
			{
				requirement.AddLinkedRequirement(JS_ElectronicBillOfLadingConsigneeDocAddress.Requirement);
			}
			return requirement;
		}

		public JobDocAddress JS_ElectronicBillOfLadingConsigneeDocAddress
		{
			get
			{
				if (EnabledElectronicBOL && fJS_ElectronicBillOfLadingConsigneeDocAddress == null)
				{
					fJS_ElectronicBillOfLadingConsigneeDocAddress = DocAddresses.FindOrCreateWithRequirement(JS_ElectronicBillOfLadingConsigneeDocAddressRequirement);
					fJS_ElectronicBillOfLadingConsigneeDocAddress.SetReadOnlyIncludingChildren(true);
					fJS_ElectronicBillOfLadingConsigneeDocAddress.MakeNonPersistent();
					DefaultJS_ElectronicBillOfLadingConsigneeDocAddress();
				}

				return fJS_ElectronicBillOfLadingConsigneeDocAddress;
			}
		}
		JobDocAddress fJS_ElectronicBillOfLadingConsigneeDocAddress;

		public JobDocAddressRequirement JS_ElectronicBillOfLadingConsigneeDocAddressRequirement
		{
			get
			{
				if (fJS_ElectronicBillOfLadingConsigneeDocAddressRequirement == null)
				{
					fJS_ElectronicBillOfLadingConsigneeDocAddressRequirement = new JobDocAddressRequirement(DocAddressType.ConsigneeElectronicBOLAddress, AddressType.OFC, ContactType.Consignee);
					fJS_ElectronicBillOfLadingConsigneeDocAddressRequirement.CanOverride = false;
					fJS_ElectronicBillOfLadingConsigneeDocAddressRequirement.ValidateOrganisationPK = Validation.ValidateElectronicBillOfLadingConsigneePK;
				}
				return fJS_ElectronicBillOfLadingConsigneeDocAddressRequirement;
			}
		}
		JobDocAddressRequirement fJS_ElectronicBillOfLadingConsigneeDocAddressRequirement;

		void DefaultJS_ElectronicBillOfLadingConsigneeDocAddress()
		{
			if (!ConsigneeDocumentaryAddress.E2_AddressOverride)
			{
				JS_ElectronicBillOfLadingConsigneeDocAddress.OrganisationPK = ConsigneeDocumentaryAddress.OrganisationPK;
			}
			else
			{
				foreach (ZString field in GetAddressFields(JS_ElectronicBillOfLadingConsigneeDocAddress))
				{
					JS_ElectronicBillOfLadingConsigneeDocAddress[field] = ConsigneeDocumentaryAddress[field];
				}

				foreach (ZString field in GetContactFields(JS_ElectronicBillOfLadingConsigneeDocAddress))
				{
					JS_ElectronicBillOfLadingConsigneeDocAddress[field] = ConsigneeDocumentaryAddress[field];
				}
			}
		}

		#endregion

		#region JS_ElectronicBillOfLadingToOrderDocAddress

		public JobDocAddress JS_ElectronicBillOfLadingToOrderDocAddress
		{
			get
			{
				if (EnabledElectronicBOL && fJS_ElectronicBillOfLadingToOrderDocAddress == null)
				{
					fJS_ElectronicBillOfLadingToOrderDocAddress = DocAddresses.FindOrCreateWithRequirement(JS_ElectronicBillOfLadingToOrderDocAddressRequirement);
				}

				return fJS_ElectronicBillOfLadingToOrderDocAddress;
			}
		}

		JobDocAddress fJS_ElectronicBillOfLadingToOrderDocAddress;

		public JobDocAddressRequirement JS_ElectronicBillOfLadingToOrderDocAddressRequirement
		{
			get
			{
				if (fJS_ElectronicBillOfLadingToOrderDocAddressRequirement == null)
				{
					fJS_ElectronicBillOfLadingToOrderDocAddressRequirement = new JobDocAddressRequirement(DocAddressType.ToOrder, AddressType.OFC, ContactType.Consignee);
					fJS_ElectronicBillOfLadingToOrderDocAddressRequirement.CanOverride = false;
					fJS_ElectronicBillOfLadingToOrderDocAddressRequirement.ValidateOrganisationPK = Validation.ValidateElectronicBillOfLadingToOrderPK;
				}
				return fJS_ElectronicBillOfLadingToOrderDocAddressRequirement;
			}
		}

		JobDocAddressRequirement fJS_ElectronicBillOfLadingToOrderDocAddressRequirement;

		#endregion

		#region HolderDocAddress

		protected override void ConsignorDocumentaryOrgHeaderChanged()
		{
			base.ConsignorDocumentaryOrgHeaderChanged();
			if (HolderDocAddress != null)
			{
				DefaultHolderDocAddress();
			}
		}

		public JobDocAddress HolderDocAddress
		{
			get
			{
				if (EnabledElectronicBOL && fHolderDocAddress == null)
				{
					fHolderDocAddress = DocAddresses.FindOrCreateWithRequirement(HolderDocAddressRequirement);
					fHolderDocAddress.MakePersistentEvenIfEmpty();
					DefaultHolderDocAddress();
				}

				return fHolderDocAddress;
			}
		}

		JobDocAddress fHolderDocAddress;

		JobDocAddressRequirement HolderDocAddressRequirement
		{
			get
			{
				if (fHolderDocAddressRequirement == null)
				{
					fHolderDocAddressRequirement = new JobDocAddressRequirement(DocAddressType.Holder, ContactType.Consignor);
					fHolderDocAddressRequirement.CanOverride = false;
					fHolderDocAddressRequirement.ValidateOrganisationPK = Validation.ValidateHolderPK;
				}
				return fHolderDocAddressRequirement;
			}
		}

		JobDocAddressRequirement fHolderDocAddressRequirement;

		void DefaultHolderDocAddress()
		{
			if (JS_ElectronicBillOfLadingStatus.IsEmpty
				&& HolderDocAddress.OrganisationPK.IsEmpty
				&& !ConsignorDocumentaryAddress.E2_AddressOverride
				&& ConsignorDocumentaryAddress.Organisation != null)
			{
				HolderDocAddress.OrganisationPK = ConsignorDocumentaryAddress.OrganisationPK;
				HolderDocAddress.E2_OA_Address = ConsignorDocumentaryAddress.E2_OA_Address;
				HolderDocAddress.E2_Contact = ConsignorDocumentaryAddress.E2_Contact;
			}
		}

		#endregion

		#region ExportReleaseMessages

		public ExportReleaseMessageCollectionView ExportReleaseMessages => new ExportReleaseMessageCollectionView(base.Messages);

		#endregion

		#region SurrenderPartyDocAddress

		public JobDocAddress SurrenderPartyDocAddress
		{
			get
			{
				if (EnabledElectronicBOL && fSurrenderPartyDocAddress == null)
				{
					fSurrenderPartyDocAddress = DocAddresses.FindOrCreateWithRequirement(SurrenderPartyDocAddressRequirement);
					fSurrenderPartyDocAddress.MakePersistentEvenIfEmpty();
					DefaultSurrenderPartyDocAddress();
				}

				return fSurrenderPartyDocAddress;
			}
		}
		JobDocAddress fSurrenderPartyDocAddress;

		JobDocAddressRequirement SurrenderPartyDocAddressRequirement
		{
			get
			{
				if (fSurrenderPartyDocAddressRequirement == null)
				{
					fSurrenderPartyDocAddressRequirement = new JobDocAddressRequirement(DocAddressType.SurrenderParty, ContactType.Consignor);
					fSurrenderPartyDocAddressRequirement.CanOverride = false;
					fSurrenderPartyDocAddressRequirement.ValidateOrganisationPK = Validation.ValidateSurrenderPartyPK;
				}
				return fSurrenderPartyDocAddressRequirement;
			}
		}

		JobDocAddressRequirement fSurrenderPartyDocAddressRequirement;

		void DefaultSurrenderPartyDocAddress()
		{
			if (SurrenderPartyDocAddress.OrganisationPK.IsEmpty)
			{
				var defaultFromConsol = GetDefaultSurrenderParty();
				if (defaultFromConsol != null)
				{
					SurrenderPartyDocAddress.OrganisationPK = defaultFromConsol.OrgPK;
					SurrenderPartyDocAddress.E2_OA_Address = defaultFromConsol.AddressFK;
					if (!defaultFromConsol.ContactFK.IsEmpty)
					{
						SurrenderPartyDocAddress.E2_Contact = Factory.Load<OrgContact>(defaultFromConsol.ContactFK).OC_ContactName;
					}
				}
			}
		}

		ZAddressWithContact GetDefaultSurrenderParty()
		{
			var consols = Consols.Cast<ForwardingConsol>().ToArray();
			MovementLegComparer.SortMovementLegsByPorts(consols);
			var sortedConsols = consols.Reverse();
			foreach (var consol in sortedConsols)
			{
				if (consol.JK_RL_NKDischargePort == JS_RL_NKDestination)
				{
					return consol.ReceivingForwarderWithContact;
				}
			}

			return null;
		}

		#endregion

		#region JS_Calc_ElectronicBillOfLadingDate

		public ZDateTime JS_Calc_ElectronicBillOfLadingDate
		{
			get
			{
				var mostRecentBillStatusUpdatedLog = Logs.MostRecentLogByPostedTime(
					Events.BillStatusUpdated,
					log => log.Parameters.TryGetValue(Params.Type, out var type) && IsBLUType(type)
						&& log.Parameters.TryGetValue(Params.Department, out var department) && department == ElectronicBOLConstants.EHBLEventDepartments.TitleRegistry)?.PostedLocalBranchTime ?? ZDateTime.Empty;

				var documentDataLoader = ObjectFactory.Get<IVisualizerDocumentDataLoader>();
				var billOfLadingDocumentData = documentDataLoader.Load(this, ShipmentDocumentDataStoreNames.BillOfLading) as EnterpriseBusinessObject;
				var mostRecentMessageSentLog = billOfLadingDocumentData?.Logs.MostRecentLogByPostedTime(
					Events.MessageSent,
					log => log.Parameters.TryGetValue(Params.Type, out var type) && type == BillStatusUpdatedTypes.OriginalBillSentForPublication
						&& log.Parameters.TryGetValue(Params.Department, out var department) && department == ElectronicBOLConstants.EHBLEventDepartments.TitleRegistry)?.PostedLocalBranchTime ?? ZDateTime.Empty;

				return mostRecentBillStatusUpdatedLog > mostRecentMessageSentLog ? mostRecentBillStatusUpdatedLog : mostRecentMessageSentLog;
			}
		}

		ZBool IsBLUType(string type)
		{
			if (string.IsNullOrEmpty(type))
			{
				return false;
			}

			switch (type)
			{
				case BillStatusUpdatedTypes.SwitchedToPaper:
				case BillStatusUpdatedTypes.Surrendered:
				case BillStatusUpdatedTypes.AmendmentRequested:
				case BillStatusUpdatedTypes.OriginalBillPublished:
				case BillStatusUpdatedTypes.OriginalBillNotPublished:
				case BillStatusUpdatedTypes.OriginalBillTransferred:
				case BillStatusUpdatedTypes.DenyAmendmentAccepted:
				case BillStatusUpdatedTypes.DenyAmendmentRejected:
					return true;
				default:
					return false;
			}
		}

		public ZPropertyInfo JS_Calc_ElectronicBillOfLadingDateInfo => GetZPropertyInfo(nameof(JS_Calc_ElectronicBillOfLadingDate));

		#endregion

		public ZString JS_ElectronicBillOfLadingStatusDescription => JS_ElectronicBillOfLadingStatus.IsEmpty ? ZString.Empty : Lookups.JS_ElectronicBillOfLadingBillStatus_List.GetDescriptionFromCode(JS_ElectronicBillOfLadingStatus);

		public ZString JS_ElectronicBillOfLadingTypeDescription => JS_ElectronicBillOfLadingType.IsEmpty ? ZString.Empty : Lookups.JS_ElectronicBillOfLadingType_List.GetDescriptionFromCode(JS_ElectronicBillOfLadingType);

		public ZString JS_ElectronicBillOfLadingTermsDescription => JS_ElectronicBillOfLadingTerms.IsEmpty ? ZString.Empty : Lookups.JS_ElectronicBillOfLadingTerms_List.GetDescriptionFromCode(JS_ElectronicBillOfLadingTerms);

		#region IComplianceWiseShipment

		ZBool IComplianceWiseShipment.IsBooking => JS_IsBooking;

		ZBool IComplianceWiseShipment.IsForwardRegistered => JS_IsForwardRegistered;

		ZGuid IComplianceWiseShipment.OneTimeQuotePK => JS_TH_OneTimeQuote;

		#endregion

		#region IDangerousGoodsPortalErrorProvider

		public ZString DGPortalLaunchError =>
			TransportMode != TransportModes.Sea &&
			TransportMode != TransportModes.AirSea &&
			TransportMode != TransportModes.SeaAir &&
			!TransportsIncludingRelated.Cast<Transport>().Any(t => t.TransportMode == TransportModes.Sea)
			? Res.GetString("8547CB61-5886-434F-BEA5-B76ECFBB56C3", "This shipment does not have a Sea leg movement.")
			: ZString.Empty;

		#endregion

		#region Global Commercial Invoice

		public override void GlobalCommercialInvoiceUpdateUnitOfMeasurement(ZPropertyInfo propertyInfo, ZString oldValue, ZString newValue)
		{
			base.GlobalCommercialInvoiceUpdateUnitOfMeasurement(propertyInfo, oldValue, newValue);
			ObjectFactory.Get<IGlobalCommercialInvoiceJobProcessor>().UpdateUnitOfMeasurement(this, propertyInfo, oldValue, newValue);
		}

		#endregion

		#region JobRatingPreference

		public JobRatingPreference AutoratingPreferences
		{
			get
			{
				if (autoratingPreferences == null)
				{
					autoratingPreferences = JobRatingPreference.Load(this) ?? JobRatingPreference.Create(this);
					// Changes in the preference have to notify its parent - this shipment.
					RegisterEditableChildObject(autoratingPreferences);
				}

				return autoratingPreferences;
			}
		}
		JobRatingPreference autoratingPreferences;

		bool IsAutoratingDateReadOnly =>
			ReadOnly || !Env.Security.MaintainShipmentTariffsAndRatesRevenueAutoratingDate.IsAllowed;

		/// <summary>
		///	Proxy the AutoratingPreferences.JRP_AutoratingDate property for autorating date.
		///	The date field is used for Revenue.
		/// </summary>
		[ReadOnlyMember(nameof(IsAutoratingDateReadOnly))]
		public override ZDate RevenueAutoratingDate
		{
			get => AutoratingPreferences.JRP_AutoratingDate.Date;
			set
			{
				if (AutoratingPreferences.JRP_AutoratingDate == value)
				{
					return;
				}

				AutoratingPreferences.JRP_AutoratingDate = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateAutoratingDate();
				}

				RevenueAutoratingDateInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo RevenueAutoratingDateInfo => GetZPropertyInfo(nameof(RevenueAutoratingDate));

		#endregion
		#region IETAProvider

		public IETAProvider GetEtaProvider() => new ForwardingShipmentETAProvider(this);

		#endregion

		#region IL Customs

		#region JS_DLO

		[MaxLength(CusEntryNumber.Schema.CE_EntryNumMaxLength)]
		public ZString JS_DLO
		{
			get => DONEntryNumber?.CE_EntryNum ?? ZString.Empty;
			set
			{
				if (JS_DLO != value)
				{
					if (DONEntryNumber == null)
					{
						donEntryNumber = CusEntryNumber.New<CusEntryNumber>(this, CusEntryNumberTypes.Israel.DeliveryOrderNumber, CountryCodes.Israel);
					}
					DONEntryNumber.CE_EntryNum = value;
					RegisterEditableChildObject(donEntryNumber);
					JS_DLOInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo JS_DLOInfo => GetZPropertyInfo(nameof(JS_DLO));

		CusEntryNumber DONEntryNumber
		{
			get
			{
				if (donEntryNumber == null || donEntryNumber.IsDeleted)
				{
					donEntryNumber = CusEntryNumber.Load<CusEntryNumber>(this, CusEntryNumberTypes.Israel.DeliveryOrderNumber, CountryCodes.Israel);
				}
				return donEntryNumber;
			}
		}
		CusEntryNumber donEntryNumber;

		#endregion

		#region JS_GMN

		[MaxLength(CusEntryNumber.Schema.CE_EntryNumMaxLength)]
		public ZString JS_GMN
		{
			get => GMNEntryNumber?.CE_EntryNum ?? ZString.Empty;
			set
			{
				if (JS_GMN != value)
				{
					if (GMNEntryNumber == null)
					{
						gmnEntryNumber = CusEntryNumber.New<CusEntryNumber>(this, CusEntryNumberTypes.Israel.GatepassMovementNumber, CountryCodes.Israel);
					}
					GMNEntryNumber.CE_EntryNum = value;
					RegisterEditableChildObject(gmnEntryNumber);
					JS_GMNInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo JS_GMNInfo => GetZPropertyInfo(nameof(JS_GMN));

		CusEntryNumber GMNEntryNumber
		{
			get
			{
				if (gmnEntryNumber == null || gmnEntryNumber.IsDeleted)
				{
					gmnEntryNumber = CusEntryNumber.Load<CusEntryNumber>(this, CusEntryNumberTypes.Israel.GatepassMovementNumber, CountryCodes.Israel);
				}
				return gmnEntryNumber;
			}
		}
		CusEntryNumber gmnEntryNumber;

		#endregion

		public IGatePassMovementProvider GatePassMovementProvider
		{
			get
			{
				if (gatePassMovementProvider == null)
				{
					var providers = (Hashtable)ObjectFactory.Get("IGatePassMovementProvider");
					var objectHandle = (ObjectHandle)providers[nameof(ForwardingShipment)];
					gatePassMovementProvider = new Lazy<IGatePassMovementProvider>(() => (IGatePassMovementProvider)objectHandle.GetObject(new object[] { this }));
				}

				return gatePassMovementProvider.Value;
			}
		}

		Lazy<IGatePassMovementProvider> gatePassMovementProvider;

		public IDeliveryOrderProvider DeliveryOrderProvider
		{
			get
			{
				if (deliveryOrderProvider == null)
				{
					var providerFactory = ObjectFactory.Get<IDeliveryOrderProviderFactory>();
					deliveryOrderProvider = new Lazy<IDeliveryOrderProvider>(() => providerFactory.CreateDeliveryOrderProvider(this));
				}

				return deliveryOrderProvider.Value;
			}
		}

		Lazy<IDeliveryOrderProvider> deliveryOrderProvider;

		#endregion

		#region IExternalRequestGenerationProvider

		public ZString GetRequestJobID() => JS_UniqueConsignRef;

		public ZString GetRequestTypeCode() => ExternalRequestTypes.Codes.Shipment;

		public (ZGuid OrginzationPK, ZGuid ContactPK) GetRequestSupportedAddressInfo(ZString addressType) => addressType.ToString() switch
		{
			DocAddressTypes.Codes.ConsigneeDocumentaryAddress => RequestGenerationProviderHelper.GetRequestSupportedAddressInfo(ConsigneeDocumentaryAddress),
			DocAddressTypes.Codes.ConsignorDocumentaryAddress => RequestGenerationProviderHelper.GetRequestSupportedAddressInfo(ConsignorDocumentaryAddress),
			DocAddressTypes.Codes.ControllingCustomer => RequestGenerationProviderHelper.GetRequestSupportedAddressInfo(ControllingCustomerAddress),
			DocAddressTypes.Codes.LocalClient => (ShipmentJobHeader?.LocalChargesAddr?.OA_OH ?? ZGuid.Empty, ZGuid.Empty),
			_ => (ZGuid.Empty, ZGuid.Empty),
		};

		#endregion
	}
}

#region Test
#if DEBUG

#region Test Methods

namespace Enterprise.Freight.Forwarding.Business
{
	partial class ForwardingShipment
	{
		internal void ResetAviationSecurityForTesting()
		{
			aviationSecurity = null;
		}
	}
}

#endregion

#endif
#endregion
