using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.IO;
using CargoWise.Macros;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Integration;
using Enterprise.BufferManagement.Integration;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.Customs.Common;
using Enterprise.DeniedPartyScreening.Integration;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.DocumentEngineIntegration.DocumentParsing;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Extensions;
using Enterprise.Freight.Business.HelperClasses;
using Enterprise.Freight.CarbonEmissions.Business;
using Enterprise.Freight.CarbonEmissions.Integration;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Forwarding.Business.AWB;
using Enterprise.Freight.Forwarding.Business.HelperClasses;
using Enterprise.Freight.Forwarding.Business.HelperClasses.FilteredRatingContractAllocationLine;
using Enterprise.Freight.Forwarding.Registry;
using Enterprise.Freight.Integration;
using Enterprise.Freight.LocalCartage.Integration;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.Freight;
using Enterprise.Integration.TransportBooking;
using Enterprise.MasterData.Business;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Business.Rating;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.TransportBookings.Shared;
using Enterprise.TransportCommon.Shared;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.EventManagement;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using static Enterprise.Integration.Customs;
using static Enterprise.Integration.Forwarding;
using AWBSpecialHandlingCodeDescriptionPairList = Enterprise.Freight.Forwarding.AWB.Business.AWBSpecialHandlingCodeDescriptionPairList;
using Constants = Enterprise.Core.Constants;
using EventConstants = CargoWise.EventReference.Constants;
using IAUCusMAWB = Enterprise.Integration.Customs.AU.ICusMAWB;
using IAUCusSCAOceanBill = Enterprise.Integration.Customs.AU.ICusSCAOceanBill;
using ICusMAWB = Enterprise.Integration.Customs.Shared.ICusMAWB;
using IForwardingConsol = Enterprise.Integration.Forwarding.IForwardingConsol;
using IForwardingShipment = Enterprise.Integration.Forwarding.IForwardingShipment;
using Shipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.Business
{
	[SystemDefinedValues]
	[ProvideMetaDataProperty("ReadOnlySecurity", MetaDataTypes.ReadOnly), UniversalDataContext(DataContextType.ForwardingConsol)]
	[Metadata.Integration.MetadataContext(Enterprise.Metadata.Integration.MetadataContext.ForwardingConsol)]
	[UniversalCopyChildrenSubstituteType(typeof(CommonShipment), typeof(ForwardingShipment))]
	[UniversalCopyWithExtendedEntities(FinishCopyMethod = nameof(AfterUniversalCopy))]
	[UniversalCopyIgnoreElement("JK_OA_CoLoadAddress")]
	[VisualizableDocumentsSupportable("ForwardingConsolVisualizableDocumentSupporter")]
	public partial class ForwardingConsol :
		CommonConsol,
		IForwardingConsol,
		IServiceLocator,
		IAWBParent,
		IMAWBAllocationParent,
		IWorkflowProviderIncludingRelated,
		IWorkflowTriggerFieldChangeSource,
		IHaveRequiredDocuments,
		IMAWBParent,
		ICusInBondParent,
		IJobInvoicingPlugIn,
		IJobCostingPlugIn,
		ITransportParent,
		IScreeningPartyProvider,
		IShouldUpdateScreeningStatus,
		IBillDetails,
		ICustomFieldProvider,
		ICusAddInfoTypeSupporter,
		IProcessHandlingInfoProvider,
		IControllerIDProvider,
		IConsolOrShipment,
		IValidateForCustomsMessagingSupporter,
		ITriggerActionMessagingSupporterProvider,
		IRelatableActivity,
		ISupportsPostingOverseasAgentCharge,
		IUniversalXMLNoteParent,
		ISendEmailSource,
		IRatingSupporter,
		IContainerTrackingProvider,
		ICustomLabelsConfigOrgProvider,
		IGateway,
		IRelatedOrgDeniedPartyScreenable,
		INZManifestHeader,
		ICusEntryNumberValidationDeciderOfType,
		IRequiredTemperature,
		IDtbBookingParent,
		IAdditionalReferenceNumberValidationProvider,
		ITransitWarehouseInstructionSupporter,
		ITemplateRecordProvider,
		IDeniedPartyProvider,
		ICompliancePartyRiskStatusProvider,
		IComplianceLocationRiskStatusProvider,
		IComplianceCommodityRiskStatusProvider,
		IUniqueConsolProvider,
		IAirlineTrackingEventProvider,
		ICO2eLegBasedSupporter,
		IAddressesValidation,
		ICCACommonAssignmentValidationData,
		IAllocationRouteAssignable,
		IExternalRequestGenerationProvider,
		IGatePassMovementProviderFactory
	{
		#region Schema

		public new abstract class Schema : CommonConsol.Schema
		{
			public const string AWBCurrentStatus = "AWBCurrentStatus";
			public const string MAWBLabelDesc = "MAWBLabelDesc";
			public const string MasterBillAirlinePrefix = "MasterBillAirlinePrefix";
			public const string MasterBillMAWB = "MasterBillMAWB";
			public const string MasterBillNeutralMAWB = "MasterBillNeutralMAWB";
			public const string FinalMAWBPrintedDate = "FinalMAWBPrintedDate";
			public const string UAEInstalmentNumber = "UAEInstalmentNumber";

			public const string JK_RL_NKLoadForExportTransport = "JK_RL_NKLoadForExportTransport";
			public const string JK_RL_NKDiscForExportTransport = "JK_RL_NKDiscForExportTransport";
			public const string JK_RL_NKLoadForImportTransport = "JK_RL_NKLoadForImportTransport";
			public const string JK_RL_NKDiscForImportTransport = "JK_RL_NKDiscForImportTransport";
			public const string JK_ATAForImportTransport = "JK_ATAForImportTransport";
			public const string JK_ATAForLastTransport = "JK_ATAForLastTransport";
			public const string JK_ETAForImportTransport = "JK_ETAForImportTransport";
			public const string JK_DepartureForFirstTransport = "JK_DepartureForFirstTransport";
			public const string JK_Calc_IsCargoOnly = "JK_Calc_IsCargoOnly";
			public const string JK_Calc_AirBookingStatus = "JK_Calc_AirBookingStatus";

			public const string VolumeVerificationUnit = "VolumeVerificationUnit";
			public const string WeightVerificationUnit = "WeightVerificationUnit";
			public const string SecurityStatusCode = "SecurityStatusCode";
		}

		#endregion

		public ForwardingConsol(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			factory.SetFreightDomainContext(FreightDomainContext.Forwarding);
			RegisterCountrySpecificSupport();
		}

		#region Load

		public static ForwardingConsol LoadFromRef(BusinessObjectFactory factory, ZString uniqueConsignRef)
		{
			return !uniqueConsignRef.IsEmpty ? factory.LoadFromNaturalKey<ForwardingConsol>(JobConsolSchema.JK_UniqueConsignRef, uniqueConsignRef) : null;
		}

		#endregion

		#region Validation

		protected override JobConsolValidation GetNewValidation()
		{
			InitialisePhaseDependantMandatoryValidation();

			var result = new ForwardingConsolValidation(this);
			result.Add(ObjectFactory.New<HK.IExtendedForwardingConsolValidation>(this) as ForwardingConsolValidation);
			return result;
		}

		public new ForwardingConsolValidation Validation
		{
			get { return (ForwardingConsolValidation)base.Validation; }
		}

		protected override void MarkAsNeedingValidationCore()
		{
			base.MarkAsNeedingValidationCore();
			if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Iceland)
			{
				Validation.ValidateJK_CRN();
			}
		}

		Type ICusEntryNumberValidationDeciderOfType.GetCusEntryNumberValidationType()
		{
			return typeof(ForwardingConsolAdditionalRefEntryNumValidation);
		}

		void IAdditionalReferenceNumberValidationProvider.ValidateEntryType(ZPropertyInfo entryTypeInfo, ZString category, ZString countryCode)
		{
		}

		bool IAdditionalReferenceNumberValidationProvider.EntryTypeShouldBeUnique(ZString entryType, ZString category, ZString countryCode)
		{
			return entryType != CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CON;
		}

		#endregion

		#region AirCargoSynchroniser

		public IUpdateFromConsol AirCargoSynchroniser
		{
			get { return airCargoSynchroniserRetriever == null ? null : airCargoSynchroniserRetriever(); }
		}

		public void SetAirCargoSynchroniserRetriever(GetIUpdateFromConsolDelegate inputAirCargoSynchroniserRetriever)
		{
			this.airCargoSynchroniserRetriever = inputAirCargoSynchroniserRetriever;
		}

		GetIUpdateFromConsolDelegate airCargoSynchroniserRetriever;

		public delegate IUpdateFromConsol GetIUpdateFromConsolDelegate();

		#endregion

		#region Fetch Strategy

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new ForwardingConsolFetchStrategy(this);
		}

		#endregion

		#region Loading / Default Values

		public override void OnLoaded()
		{
			IsLoading = true;
			try
			{
				base.OnLoaded();
				if (IsAir)
				{
					MasterBillMAWBInfo.RefreshBinding();
					MasterBillNeutralMAWBInfo.RefreshBinding();
					MasterBillAirlinePrefixInfo.RefreshBinding();
					JK_IsNeutralMasterInfo.RefreshBinding();
				}

				using (SuspendSettingHasChanges())
				{
					WeightVerificationUnit = IsAir ? JK_TotalShipmentChargeableUnit : JK_TotalShipmentActOtherUnit;
					VolumeVerificationUnit = IsAir ? JK_TotalShipmentActOtherUnit : JK_TotalShipmentChargeableUnit;
				}

				if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Iceland)
				{
					AutoFillCRN(Transports.MostInterestingTransport);
				}
			}
			catch (Exception ex)
			{
				ErrorReporter.ReportOnce("Error when calling OnLoaded", ex);
				throw;
			}
			finally
			{
				IsLoading = false;
			}
		}

		protected override void DocAddressesOnLoad(JobDocAddressDependentCollection docAddresses)
		{
			base.DocAddressesOnLoad(docAddresses);

			foreach (var docAddress in docAddresses.Cast<JobDocAddress>().ToArray())
			{
				if (docAddress.DocAddressType == DocAddressType.SelfFiler)
				{
					docAddress.ReadOnlyStrategy = new JobDocAddressReadOnlyStrategy(() => true, () => true);
				}
			}
		}

		protected override void DocAddressOnAddedEvent(DocAddressType docAddressType)
		{
			if (docAddressType == DocAddressType.CarrierExportCreditor && (this.IsExport() || this.IsDomestic()))
			{
				if (IsCoLoad)
				{
					if (!JK_OA_CreditorAddress.IsEmpty)
					{
						var creditorExportAddress = GetDefaultCreditorFromCarrier(JK_OA_CreditorAddress);
						CarrierExportCreditorAddress.E2_OA_Address = creditorExportAddress.IsEmpty ? JK_OA_CreditorAddress : creditorExportAddress;
					}
				}
				else
				{
					if (!JK_OA_CreditorAddress.IsEmpty)
					{
						CarrierExportCreditorAddress.E2_OA_Address = JK_OA_CreditorAddress;
					}
					else if (!JK_OA_ShippingLineAddress.IsEmpty)
					{
						var creditorExportAddress = GetDefaultCreditorFromCarrier(JK_OA_ShippingLineAddress);

						if (!creditorExportAddress.IsEmpty)
						{
							CarrierExportCreditorAddress.E2_OA_Address = creditorExportAddress;
						}
					}
				}
			}

			if (docAddressType == DocAddressType.CarrierImportCreditor && (this.IsImport() || this.IsCrossTrade()))
			{
				if (!JK_OA_CreditorAddress.IsEmpty)
				{
					CarrierImportCreditorAddress.E2_OA_Address = JK_OA_CreditorAddress;
				}
				else if (!JK_OA_ShippingLineAddress.IsEmpty)
				{
					var creditorImportAddress = GetDefaultCreditorFromCarrier(JK_OA_ShippingLineAddress);

					if (!creditorImportAddress.IsEmpty)
					{
						CarrierImportCreditorAddress.E2_OA_Address = creditorImportAddress;
					}
				}
			}
		}

		public void HookupDocAddressEventHandler()
		{
			if (this.IsExport() || this.IsDomestic())
			{
				SetupCarrierExportCreditorAddress();
				return;
			}
			if (this.IsImport() || this.IsCrossTrade())
			{
				SetupCarrierImportCreditorAddress();
				return;
			}
		}

		public override void SetDefaultSendingForwarderAddressWithFallbackToProxy(OrgHeader proxy)
		{
			if (IsDirect)
			{
				JK_OA_SendingForwarderAddress = ZGuid.Empty;
			}
			else
			{
				base.SetDefaultSendingForwarderAddressWithFallbackToProxy(proxy);
			}
		}

		public override void SetDefaultReceivingForwarderAddressWithFallbackToProxy(OrgHeader proxy)
		{
			if (IsDirect)
			{
				JK_OA_ReceivingForwarderAddress = ZGuid.Empty;
			}
			else
			{
				base.SetDefaultReceivingForwarderAddressWithFallbackToProxy(proxy);
			}
		}

		protected override void SetDefaultValues()
		{
			IsSettingDefaultValues = true;
			try
			{
				base.SetDefaultValues();

				JK_IsNeutralMaster = false;
				JK_PrintOptionForPackagesOnAWB = Env.Registry.Freight.AirWaybill.MAWBDimensionsDefault;
				JK_ReleaseType = DocumentsDataRegistry.Instance.ReleaseType.Value;
				JK_Phase = ForwardingConfigurationRegistry.Instance.ConsolDefaultPhase.Value;
				JK_AgentType = ForwardingConfigurationRegistry.Instance.ConsolTypeDefaultForConsol.Value;

				if (!JK_RL_NKLoadPort.IsEmpty)
				{
					SetDefaultSendingForwarderAddressWithFallbackToProxy(GlbBranch.CurrentBranch.OrgProxy);
					SetSendingForwarderHandlingTypeDefaultForAnyCompany();
				}

				if (!JK_RL_NKDischargePort.IsEmpty)
				{
					SetDefaultReceivingForwarderAddressWithFallbackToProxy(GlbBranch.CurrentBranch.OrgProxy);
					SetReceivingForwarderHandlingTypeDefaultForAnyCompany();
				}

				if (JK_TransportMode == Constants.TransportModes.Air)
				{
					JK_MBLAWBChargesDisplay = GetDefaultChargesApply();
				}
			}
			finally
			{
				IsSettingDefaultValues = false;
			}
		}

		bool IsLoading;
		bool IsSettingDefaultValues;
		bool isLegRemoved;

		public override void OnTransportRemoved(Transport transport)
		{
			base.OnTransportRemoved(transport);

			if (transport != null && transport.IsInDatabase)
			{
				isLegRemoved = true;
			}
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

			if (!IsDeleted)
			{
				if (IsAWBLoaded)
				{
					AWBHeader.Delete();
				}

				if (GetAllocatedMAWB(Factory) != null)
				{
					throw new CannotDeleteException("Consol job cannot be removed because a MAWB is allocated. (Delete the three character prefix of the MAWB to deallocate it.)");
				}
				ObjectFactory.Get<IChildrenDeletionHelper>().DeleteChildren(this);
			}

			WorkflowItems.RemoveAndDeleteAll();
			RelatedChildActivityPivotCollection.DeleteAll();
			RelatedParentActivityPivotCollection.DeleteAll();
			ConsolDGRestrictionCollection.DeleteAll();
			CountrySpecificJobSupport?.Unregister();
			AWBSpecialHandlingItems.DeleteAll();
			SecurityStatusCodeSpecialHandling?.Delete();
			AutoratingPreferences.Delete();
			new GenCustomAddOnRuleAckCollection(this).DeleteAll();
			base.Delete();
		}

		#endregion

		#region Saving

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();
			FactorySavingBeforeTransactionCore?.Invoke(this, EventArgs.Empty);
			if (JK_IsForwarding)
			{
				new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(this);
				if (HasChanges)
				{
					ProposeWorkflowRelationships();
				}
			}
		}

		public event EventHandler FactorySavingBeforeTransactionCore;

		void ProposeWorkflowRelationships()
		{
			var links = Factory.Load<JobConShipLink>(new ZQuery(JobConShipLinkSchema.JN_JK, PK));

			foreach (var link in links)
			{
				link.ProposeWorkflowRelationships();
			}

			foreach (var container in Containers.Cast<ForwardingContainer>())
			{
				container.ProposeWorkflowRelationships();
			}
		}

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();

			AllocationConsumptionLogger.TryLogAllocationConsumptionChanges();
			AllocationRouteContainerWeightLimitHelper.TryCancelOldOverrideEvents();

			TryLogCargoSecureEvent();

			var shouldUpdateScreeningStatus = ((IShouldUpdateScreeningStatus)this).ShouldUpdateScreeningStatus;

			if (shouldUpdateScreeningStatus)
			{
				if (screeningPartySnapshot == null)
				{
					if (IsInDatabase)
					{
						var newFactory = new ReadOnlyBusinessObjectFactory();
						var reloaded = newFactory.Load<ForwardingConsol>(PK);
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

			UpdateStatusFromScreeningParties(false);
		}

		#region Screening Statuses

		List<ScreeningPartiesSnapshot> screeningPartySnapshot;

		List<ScreeningPartiesSnapshot> currentSnapshot;

		List<ScreeningPartiesSnapshot> GetSnapshot(ForwardingConsol forwardingConsol) => ScreeningStatusUpdater.GetScreeningPartiesSnapshot(((IScreeningPartyProvider)forwardingConsol).ScreeningParties, forwardingConsol);

		#endregion

		internal void UpdateStatusFromScreeningParties(bool setShouldUpdateScreeningStatus)
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

					ScreeningStatusUpdater.UpdateJobStatusFromItsScreeningParties(this, this, JK_ScreeningStatusInfo.OriginalValue?.ToString());
				}
			}
		}

		long lastTransactionIdForScreeningUpdate;

		protected override void RunPreSaveValidationCore()
		{
			RefCountryRulesHelper.AddRulesToNotes(LoadPort?.Country, DischargePort?.Country, Notes, TransportMode, IsInDatabase, true);
			base.RunPreSaveValidationCore();
		}

		public override void OnSaving()
		{
			RelatedChildActivityPivotCollection.RelinkRelatedSuperAndSubActivities();
			RelatedParentActivityPivotCollection.RelinkRelatedSuperAndSubActivities();

			if (JK_OC_SendingForwarderContact == ZGuid.Empty && (!this.IsInDatabase || JK_OA_SendingForwarderAddressInfo.HasChanges))
			{
				JK_OC_SendingForwarderContact = GetDefaultContactForSendingForwarder(SendingForwarder, SendingForwarderAddress);
			}

			if (JK_OC_ReceivingForwarderContact == ZGuid.Empty && (!this.IsInDatabase || JK_OA_ReceivingForwarderAddressInfo.HasChanges))
			{
				JK_OC_ReceivingForwarderContact = GetDefaultContactForReceivingForwarder(ReceivingForwarder, ReceivingForwarderAddress);
			}

			base.OnSaving();

			RefCountryRulesHelper.AddRulesToNotes(LoadPort?.Country, DischargePort?.Country, Notes, TransportMode, IsInDatabase, false);

			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(JK_MasterBillNum), IsInDatabase && IsAir
				? ConcurrencyPolicy.Strict
				: ConcurrencyPolicy.Default);

			if (!JK_MasterBillNum.IsEmpty)
			{
				if (JK_MasterBillNumInfo.HasChanges || !IsInDatabase)
				{
					Logs.AddNew(Events.WaybillBillOfLadingAssigned, string.Format(CultureInfo.InvariantCulture, "Master Bill Number \"{0}\" Was Entered", JK_MasterBillNum));
				}
			}
			else
			{
				if (JK_MasterBillNumInfo.HasChanges)
				{
					Logs.AddNew(Events.WaybillBillOfLadingUnassigned);
				}
			}

			UpdatePreAllocatedAmountExceededStatusIfNecessary();

			if (!IsInDatabase || JK_MasterBillNumInfo.HasChanges)
			{
				UpdateTransportFlightSubscriptionEvent();
			}

			if (IsAir && IsCoLoad)
			{
				FlightMonitoringSystemManager.UpdateFlightSubscriptionEvent(this);
			}

			new ContainerTrackingSubscriptionRequestedManager(this).UpdateIfNecessary();

			SetDefaultSelfFilerIfNecessary();

			if (Syncroniser != null)
			{
				Syncroniser.Syncronise(this);
			}
		}

		protected override void OnConsolSaving()
		{
			var mawbAllocated = MAWBAllocation.PerformMAWBAllocation();
			if (!mawbAllocated && JK_IsNeutralMaster && MasterBillMAWB.IsEmpty)
			{
				using (GetValidationSuspender())
				{
					JK_IsNeutralMaster = false;
				}
			}

			base.OnConsolSaving();

			if (LastImportTransport?.JW_ATAInfo?.HasChanges ?? false)
			{
				foreach (var container in Containers.Cast<ForwardingContainer>())
				{
					container.CalculateJC_EmptyReturnedBy();
				}
			}

			var shipmentsToRecalculate = Shipments.Cast<ForwardingShipment>().ToList();
			foreach (var shipmentPK in ShipmentsDetachedThisSession.Keys)
			{
				var shipment = Factory.Load<ForwardingShipment>(shipmentPK);
				if (shipment != null && !shipment.IsDeleted && !shipment.IsDeleting)
				{
					shipmentsToRecalculate.Add(shipment);
				}
			}

			foreach (var shipment in shipmentsToRecalculate)
			{
				shipment.CalculateDeliveryDueDateIfNecessary();
			}
		}

		void UpdateTransportFlightSubscriptionEvent()
		{
			foreach (var transport in Transports.OfType<Transport>().Where(t => t.IsAir))
			{
				FlightMonitoringSystemManager.UpdateFlightSubscriptionEvent(transport);
			}
		}

		public event EventHandler ConsolSaveSucceeded;

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);

			if (saveSucceeded)
			{
				((IShouldUpdateScreeningStatus)this).ShouldUpdateScreeningStatus = false;
				isLegRemoved = false;
				ConsolSaveSucceeded?.Invoke(this, EventArgs.Empty);
			}
		}

		protected override void OnFactorySaved(bool saveSucceeded)
		{
			base.OnFactorySaved(saveSucceeded);
			if (saveSucceeded)
			{
				OverrideWaybillDefaultsHasChanges = false;
				OverrideSecurityDeclarationHasChanges = false;
				forceUpdatePreAllocatedAmountExceededStatus = false;
				FreightHasBeenVerifiedAsSecure = false;
				screeningPartySnapshot = currentSnapshot;
				originalRequireTEU = (this as ICO2eCalculationSupporter).RequireTEU;
			}

			SuppressAutoGenerateMasterBillNumber = false;
		}

		#region CountrySpecificSupport

		void RegisterCountrySpecificSupport()
		{
			var supports = new List<CountrySpecificJobSupport<ForwardingConsol>>
			{
				new IcelandForwardingConsolSupport(),
				new CanadaForwardingConsolSupport()
			};

			foreach (var support in supports)
			{
				if (support.Register(this))
				{
					CountrySpecificJobSupport = support;
					break;
				}
			}
		}

		CountrySpecificJobSupport<ForwardingConsol> CountrySpecificJobSupport { get; set; }

		#endregion

		void AddRequiredDocuments()
		{
			JobRequiredDocumentDependentCollection.DirectionFilterType directionType = IsDomesticFreight ? JobRequiredDocumentDependentCollection.DirectionFilterType.Domestic : JobRequiredDocumentDependentCollection.DirectionFilterType.Both;
			RequiredDocuments.AddCountryRequiredDocuments(RefCountryRequiredDocumentSchema.RD_OnConsol, JK_TransportMode, JK_ConsolMode, directionType, JK_RL_NKLoadPort, JK_RL_NKDischargePort);
		}

		#region Pre-Allocation Status

		long lastFactoryTransactionId;

		protected override bool AnyPreAllocatedAmountPropertyHasChanges
		{
			get
			{
				return
				JK_TotalShipmentActWeightCheckInfo.HasChanges || WeightVerificationUnitInfo.HasChanges
				|| JK_TotalShipmentActVolumeCheckInfo.HasChanges || VolumeVerificationUnitInfo.HasChanges
				|| JK_TotalShipmentChargableCheckInfo.HasChanges || JK_TotalShipmentChargeableUnitInfo.HasChanges
				|| JK_TotalShipmentCountCheckInfo.HasChanges;
			}
		}

		public override void UpdatePreAllocatedAmountExceededStatus()
		{
			if (lastFactoryTransactionId != Factory.TransactionId || Factory.TransactionId == 0)
			{
				lastFactoryTransactionId = Factory.TransactionId;
				var exceededLog = FindOrCreatePreAllocationExceededLog();
				UpdateLogToCurrentBranchAndDepartment(exceededLog);

				if (IsPreAllocationExceeded)
				{
					if (exceededLog.SL_IsCancelled)
					{
						exceededLog.Reactivate(ZDateTime.Now);
					}
				}
				else if (!exceededLog.SL_IsCancelled)
				{
					exceededLog.Cancel(ZDateTime.Now);
				}

				Validation.ValidatePreAllocationValues();
			}
		}

		StmALog FindOrCreatePreAllocationExceededLog()
		{
			var query = new ZQuery();
			query.AddToFilter(StmALogSchema.SL_Parent, PK);
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.PreAllocatedAmountExceeded.Code);
			query.OrderBy = "SL_EventTime desc"; // this is a column name

			return Logs.Factory.LoadTop1<StmALog>(query)
				?? Logs.AddNew(Events.PreAllocatedAmountExceeded);
		}

		void UpdateLogToCurrentBranchAndDepartment(StmALog log)
		{
			log.SL_GB_NKBranch = GlbBranch.CurrentBranch.GB_Code;
			log.SL_GE_NKDepartment = GlbDepartment.CurrentDepartment.GE_Code;
		}

		bool IsPreAllocationExceeded
		{
			get
			{
				return Validation.WeightExceedsPreAllocationPercentage
					|| Validation.VolumeExceedsPreAllocationPercentage
					|| Validation.ChargeableExceedsPreAllocationPercentage
					|| Validation.ShipmentCountExceedsPreAllocationPercentage;
			}
		}

		public bool IsPreAllocationExceededAndRestricted
		{
			get
			{
				var allocationChecks = ForwardingConfigurationRegistry.Instance.ConsolPreAllocationCheck.Value;

				return (allocationChecks.Weight.IsRestriction && Validation.WeightExceedsPreAllocationPercentage)
					|| (allocationChecks.Volume.IsRestriction && Validation.VolumeExceedsPreAllocationPercentage)
					|| (allocationChecks.Chargeable.IsRestriction && Validation.ChargeableExceedsPreAllocationPercentage)
					|| (allocationChecks.ShipmentCount.IsRestriction && Validation.ShipmentCountExceedsPreAllocationPercentage);
			}
		}

		#endregion

		#region Achieved Quantities

		public ConsolDensity Density => density ?? (density = new ConsolDensity(this));
		ConsolDensity density;

		[DecimalPlaces(2)]
		public ZDecimal JK_Calc_CostFreePercentage
		{
			get
			{
				if (JK_ConsolChargeable > ZDecimal.Zero)
				{
					if (JK_Calc_TotalShipmentChargeableUnit == JK_ConsolChargeableUnit)
					{
						return (JK_TotalShipmentChargeable / JK_ConsolChargeable - 1) * 100;
					}

					var convertedConsolChargeable = IsConsolChargeableByWeight ?
						Constants.Weight.ConvertSafe(JK_ConsolChargeable, JK_ConsolChargeableUnit, JK_Calc_TotalShipmentChargeableUnit, false) :
						Constants.Volume.ConvertSafe(JK_ConsolChargeable, JK_ConsolChargeableUnit, JK_Calc_TotalShipmentChargeableUnit, false);

					if (convertedConsolChargeable > ZDecimal.Zero)
					{
						return (JK_TotalShipmentChargeable / convertedConsolChargeable - 1) * 100;
					}
				}

				return ZDecimal.Zero;
			}
		}

		public ZPropertyInfo JK_Calc_CostFreePercentageInfo => GetZPropertyInfo(nameof(JK_Calc_CostFreePercentage));

		public override ZDecimal JK_ConsolChargeable
		{
			get => base.JK_ConsolChargeable;
			set
			{
				base.JK_ConsolChargeable = value;
				JK_Calc_CostFreePercentageInfo.RefreshBinding();
				JK_Calc_ConsolidatedFreightCostChargeableInfo.RefreshBinding();
			}
		}

		public ZString JK_CostFreeUnitForBinding => JK_ConsolChargeableUnit;

		public int LocalCurrencyDecimals => GlbCompany.CurrentCompany.GetLocalDecimals();

		[DecimalPlaces(nameof(LocalCurrencyDecimals))]
		public ZDecimal JK_Calc_ConsolidatedFreightCostChargeable
		{
			get
			{
				if (JK_ConsolChargeable > ZDecimal.Zero)
				{
					return GetTotalFreightGroupLocalCostAmount() / JK_ConsolChargeable;
				}

				return ZDecimal.Zero;
			}
		}

		public ZPropertyInfo JK_Calc_ConsolidatedFreightCostChargeableInfo => GetZPropertyInfo(nameof(JK_Calc_ConsolidatedFreightCostChargeable));

		[DecimalPlaces(nameof(LocalCurrencyDecimals))]
		public ZDecimal JK_Calc_ShipmentFreightCostChargeable
		{
			get
			{
				if (JK_TotalShipmentChargeable > ZDecimal.Zero)
				{
					return GetTotalFreightGroupLocalCostAmount() / JK_TotalShipmentChargeable;
				}

				return ZDecimal.Zero;
			}
		}

		public ZPropertyInfo JK_Calc_ShipmentFreightCostChargeableInfo => GetZPropertyInfo(nameof(JK_Calc_ShipmentFreightCostChargeable));

		ZDecimal GetTotalFreightGroupLocalCostAmount()
		{
			if (GetFreightGroupChargesPks().Any())
			{
				var costQuery = new ZQuery();
				costQuery.AddToFilter(JobConsolCostSchema.E6_ParentID, PK);
				costQuery.AddToFilter(JobConsolCostSchema.E6_GC, GlbCompany.CurrentCompany.PK);
				costQuery.AddToFilter(JobConsolCostSchema.E6_ParentTableCode, JobConsolSchema.Constants.Prefix);
				costQuery.AddToFilter(JobConsolCostSchema.E6_AC_ChargeCode, GetFreightGroupChargesPks());

				var jobConsolCosts = Factory.Load<IJobConsolCost>(costQuery)?.Cast<BusinessObject>();
				return jobConsolCosts.Any() ? (ZDecimal)jobConsolCosts.Sum(x => (ZDecimal)(x[JobConsolCostSchema.E6_LocalCostAmount])) : ZDecimal.Zero;
			}

			return ZDecimal.Zero;
		}

		IEnumerable<ZGuid> GetFreightGroupChargesPks()
		{
			if (freightGroupChargesPk == null)
			{
				var chargeQuery = new ZDBOnlyQuery(typeof(AccChargeCode));
				chargeQuery.AddToFilter(AccChargeCodeSchema.AC_IsActive, true);
				chargeQuery.AddToFilter(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK);
				chargeQuery.AddToFilter(AccChargeCodeSchema.AC_ChargeGroup, ChargeCodeGroupList.Codes.Freight);

				freightGroupChargesPk = Factory.Load<AccChargeCode>(chargeQuery)?.Select(x => x.PK).ToList();
			}
			return freightGroupChargesPk;
		}
		IEnumerable<ZGuid> freightGroupChargesPk;

		public ZString JK_Calc_ConsolidatedFreightCostChargeableDesc
		{
			get
			{
				if (JK_ConsolChargeableUnit.IsEmpty)
				{
					return ZString.Empty;
				}

				return Res.GetString("EBCE1FB2-96DB-44AC-A14B-7B51A8DE14C8", "{0} PER {1}", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, JK_ConsolChargeableUnit);
			}
		}

		public ZString JK_Calc_ShipmentFreightCostChargeableDesc
		{
			get
			{
				if (JK_Calc_TotalShipmentChargeableUnit.IsEmpty)
				{
					return ZString.Empty;
				}

				return Res.GetString("0EF40C43-0ED5-4FF3-A9DE-297CBD433F30", "{0} PER {1}", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, JK_Calc_TotalShipmentChargeableUnit);
			}
		}

		#endregion

		#endregion

		#region HasChanges

		public override bool HasChanges
		{
			get => base.HasChanges;
			set
			{
				base.HasChanges = value;

				if (!IsRowDeletedOrDetachedOrNull && !IsSettingHasChangesSuspended && !IsInDatabase)
				{
					UpdateAgencyRelatedDocumentaryAddressHasChangesWhenConsolIsNotInDatabase(masterBillShipperOverrideDocumentaryAddress);
					UpdateAgencyRelatedDocumentaryAddressHasChangesWhenConsolIsNotInDatabase(masterBillConsigneeOverrideDocumentaryAddress);
					UpdateAgencyRelatedDocumentaryAddressHasChangesWhenConsolIsNotInDatabase(notifyPartyDocumentaryAddress);
				}
			}
		}

		void UpdateAgencyRelatedDocumentaryAddressHasChangesWhenConsolIsNotInDatabase(JobDocAddress agencyRelatedDocumentaryAddress)
		{
			if (agencyRelatedDocumentaryAddress != null && !agencyRelatedDocumentaryAddress.IsRowDeletedOrDetachedOrNull
				&& !agencyRelatedDocumentaryAddress.IsEmpty && agencyRelatedDocumentaryAddress.HasChanges != HasChanges)
			{
				agencyRelatedDocumentaryAddress.HasChanges = HasChanges;
			}
		}

		#endregion

		#region Buyer Supplier Relationship Link

		Dictionary<string, OrgSupplierBuyerLink> BuyerSupplierLinksCache
		{
			get
			{
				if (buyerSupplierLinks == null)
				{
					buyerSupplierLinks = new Dictionary<string, OrgSupplierBuyerLink>();
					return buyerSupplierLinks;
				}
				return buyerSupplierLinks;
			}
		}
		Dictionary<string, OrgSupplierBuyerLink> buyerSupplierLinks { get; set; }

		public List<OrgSupplierBuyerLink> SavedBuyerSupplierLinksWithEmptyImportCountryCode
		{
			get
			{
				return BuyerSupplierLinksCache
					.Select(link => new { ImportCountry = new ZString(link.Key.Split('|')[2]).SubstringSafe(0, 2), Link = link.Value })
					.Where(item => item.ImportCountry == ZString.Empty && item.Link != null)
					.Select(item => item.Link)
					.ToList();
			}
		}

		public bool BuyerSupplierContainsPair(OrgHeader buyer, OrgHeader supplier, ZString importCountryCode)
		{
			return BuyerSupplierLinksCache.ContainsKey(GetBuyerSupplierKey(buyer, supplier, importCountryCode));
		}

		public void AddToBuyerSupplierLinks(OrgHeader buyer, OrgHeader supplier, ZString importCountryCode, OrgSupplierBuyerLink link)
		{
			BuyerSupplierLinksCache.Add(GetBuyerSupplierKey(buyer, supplier, importCountryCode), link);
		}

		string GetBuyerSupplierKey(OrgHeader buyer, OrgHeader supplier, ZString importCountryCode)
		{
			return buyer.PK + "|" + supplier.PK + "|" + importCountryCode;
		}

		public RefAirlineEFreightRule GetEFreightRule()
		{
			if (ShippingLine != null && ShippingLine.MiscServ != null && ShippingLine.MiscServ.IsAirline)
			{
				var airLine = ShippingLine.MiscServ.Airline;
				if (airLine != null)
				{
					return airLine.EFreightStatusCollection.GetBestMatchRule(LoadPort, DischargePort);
				}
			}

			return null;
		}

		#endregion

		#region IMAWBParent Members

		ZGuid IMAWBParent.PK
		{
			get { return PK; }
		}

		IMAWBAllocationParent IMAWBParent.MAWBAllocationParent
		{
			get { return this; }
		}

		#endregion

		#region IMAWBAllocationParent Members

		public MAWBAllocation MAWBAllocation
		{
			get
			{
				if (mawbAllocation == null)
				{
					mawbAllocation = new MAWBAllocation(this);
					mawbAllocation.MawbDeallocated += MawbAllocation_MawbDeallocated;
				}
				return mawbAllocation;
			}
		}

		void MawbAllocation_MawbDeallocated(object sender, EventArgs e)
		{
			if (AWBHeader != null)
			{
				AWBHeader.EH_FinalizationDate = ZDateTime.Empty;
			}
		}

		MAWBAllocation mawbAllocation;

		ZString IMAWBAllocationParent.AWBServiceLevel
		{
			get { return JK_AWBServiceLevel; }
		}

		ZString IMAWBAllocationParent.MawbBookingReference
		{
			get { return JK_BookingReference; }
		}

		public ZBool IsValidForNeutralMaster
		{
			get
			{
				return (IsAgent || IsDirect || IsAWBCoload)
					&& (ImportExportHelper.IsBranchCountry(MawbPortOfLoading) || ImportExportHelper.IsExport(MawbPortOfLoading, MawbPortOfDischarge));
			}
		}

		ZBool IMAWBAllocationParent.IsNeutralMaster
		{
			get { return JK_IsNeutralMaster; }
			set { JK_IsNeutralMaster = value; }
		}

		ZString IMAWBAllocationParent.MasterBill
		{
			get { return JK_MasterBillNum; }
		}

		public ZString TwoLetterAirlineCode
		{
			get { return JK_JX_JV_VoyageFlight.Left(2); }
		}

		public ZString MawbPortOfLoading
		{
			get { return LoadPort != null ? LoadPort.RL_Code : ZString.Empty; }
		}

		public ZString MawbPortOfDischarge
		{
			get { return DischargePort != null ? DischargePort.RL_Code : ZString.Empty; }
		}

		public ZString Prefix
		{
			get { return JobConsolSchema.Constants.Prefix; }
		}

		IStmNoteParent IMAWBAllocationParent.NotesParent
		{
			get { return this; }
		}

		public event EventHandler<JobMawbReallocationEventArgs> OnReallocatingPrintedMawb
		{
			add { MAWBAllocation.OnReallocatingPrintedMawb += value; }
			remove { MAWBAllocation.OnReallocatingPrintedMawb -= value; }
		}

		#endregion

		#region New Properties

		public ZDateTime JK_DepartureForTheFirstInternationalLeg
		{
			get
			{
				var result = ZDateTime.Empty;
				var firstInternationalLeg = Transports.Cast<Transport>().OrderBy(x => x.JW_LegOrder).FirstOrDefault(x => !x.IsDomestic);
				if (firstInternationalLeg != null)
				{
					result = firstInternationalLeg.JW_ATD.IsValid ? firstInternationalLeg.JW_ATD : firstInternationalLeg.JW_ETD;
				}
				return result;
			}
		}

		/// <summary>
		/// Marks consol which was opened from ZController. Used in Consol.shipment's UniqueIndexFailureHandler, BueyrSupplierLink functionality
		/// </summary>
		public bool IsRoot { get; set; }

		#region Temporary properties until generic properties are written - BS 8th May 2009

		[MaxLength(GenAddOnColumn.Schema.XA_DataMaxLength)]
		public ZString JK_CustomString1
		{
			get { return this.GetSystemDefinedValue<ZString>("CustomString1"); }
			set
			{
				this.SetSystemDefinedValue("CustomString1", value);
				JK_CustomString1Info.RefreshBinding();
				Validation.ValidateJK_CustomString1();
			}
		}

		public ZPropertyInfo JK_CustomString1Info
		{
			get { return GetZPropertyInfo(Schema.JK_CustomString1); }
		}

		[MaxLength(GenAddOnColumn.Schema.XA_DataMaxLength)]
		public ZString JK_CustomString2
		{
			get { return this.GetSystemDefinedValue<ZString>("CustomString2"); }
			set
			{
				this.SetSystemDefinedValue("CustomString2", value);
				JK_CustomString2Info.RefreshBinding();
				Validation.ValidateJK_CustomString2();
			}
		}

		public ZPropertyInfo JK_CustomString2Info
		{
			get { return GetZPropertyInfo(Schema.JK_CustomString2); }
		}

		[DecimalPlaces(2)]
		public ZDecimal JK_CustomNumber1
		{
			get { return this.GetSystemDefinedValue<ZDecimal>("CustomNumber1"); }
			set
			{
				this.SetSystemDefinedValue("CustomNumber1", value);
				JK_CustomNumber1Info.RefreshBinding();
				Validation.ValidateJK_CustomNumber1();
			}
		}

		public ZPropertyInfo JK_CustomNumber1Info
		{
			get { return GetZPropertyInfo(Schema.JK_CustomNumber1); }
		}

		[DecimalPlaces(2)]
		public ZDecimal JK_CustomNumber2
		{
			get { return this.GetSystemDefinedValue<ZDecimal>("CustomNumber2"); }
			set
			{
				this.SetSystemDefinedValue("CustomNumber2", value);
				JK_CustomNumber2Info.RefreshBinding();
				Validation.ValidateJK_CustomNumber2();
			}
		}

		public ZPropertyInfo JK_CustomNumber2Info
		{
			get { return GetZPropertyInfo(Schema.JK_CustomNumber2); }
		}

		#endregion

		public ZBool IsAirExpress
		{
			get
			{
				return FreightDataRegistry.Instance.AirExpressShipmentServiceLevelCode.Value != null && JK_AWBServiceLevel == FreightDataRegistry.Instance.AirExpressShipmentServiceLevelCode.Value;
			}
		}

		public ZInt ShipmentCount
		{
			get { return Shipments.Count; }
		}

		#region ICS2

		public bool IsSelfFiler
		{
			get
			{
				if (IsDirect)
				{
					var directShipmentConsignee = DirectShipment?.Consignee;
					return directShipmentConsignee != null && ((directShipmentConsignee.MiscServ?.OM_IMAdvanceCargoReportingSelfFiler ?? false) || GetMatchedOrgRelatedPartyRecord(directShipmentConsignee) != null);
				}
				else
				{
					return ReceivingForwarder != null && ((ReceivingForwarder.MiscServ?.OM_FWAdvanceCargoReportingSelfFiler ?? false) || GetMatchedOrgRelatedPartyRecord(ReceivingForwarder) != null);
				}
			}
		}

		public OrgRelatedParty GetMatchedOrgRelatedPartyRecord(OrgHeader org)
		{
			var allRelatedParties = org?.AllRelatedParties;
			if (allRelatedParties == null)
			{
				return null;
			}

			foreach (var ics2Transport in SeaOrInlandWaterwaysTransports)
			{
				var ics2RelatedParties = allRelatedParties.Cast<OrgRelatedParty>().Where(relatedParty => relatedParty.PR_PartyType == RelatedPartyTypeList.Codes.SelfFilerForICS2
					&& (GlbCompany.CurrentCompany?.PK != null && relatedParty.PR_GC == GlbCompany.CurrentCompany.PK || relatedParty.PR_GC.IsEmpty)).ToArray();
				var matchedRelatedParty = RelatedPartyAddressHelper.MatchRelatedParty(TransportModes.Sea, string.Empty, ics2Transport.JW_RL_NKDiscPort, ics2Transport.JW_RL_NKDiscPort.SubstringSafe(0, 2), ics2RelatedParties);

				if (matchedRelatedParty != null)
				{
					return matchedRelatedParty;
				}
			}

			return null;
		}

		public bool IsICS2
		{
			get => SeaOrInlandWaterwaysTransports.Any();
		}

		public IEnumerable<Transport> SeaOrInlandWaterwaysTransports
		{
			get
			{
				return Transports.Cast<Transport>().Where(t =>
				(
					t.JW_TransportMode == Constants.TransportModes.Sea ||
					t.JW_TransportMode == Constants.TransportModes.InlandWaterwayTransport
				) &&
				t.JW_RL_NKDiscPort.SubstringSafe(0, 2) != t.JW_RL_NKLoadPort.SubstringSafe(0, 2) &&
				(
					t.JW_RL_NKDiscPort.SubstringSafe(0, 2) == CountryCodes.Norway ||
					t.JW_RL_NKDiscPort.SubstringSafe(0, 2) == CountryCodes.Switzerland ||
					(t.DiscPort != null && (t.DiscPort.IsInEU || t.DiscPort.IsInNorthernIreland))
				) &&
				(
					t.JW_RL_NKLoadPort.SubstringSafe(0, 2) != CountryCodes.Norway &&
					t.JW_RL_NKLoadPort.SubstringSafe(0, 2) != CountryCodes.Switzerland &&
					t.LoadPort != null && !t.LoadPort.IsInEU && !t.LoadPort.IsInNorthernIreland
				));
			}
		}

		#endregion

		#region CusMAWBs

		public static TMAWB GetFirstMatchingMAWB<TMAWB>(
			BusinessObjectFactory factory,
			ZGuid pk,
			bool? isCTO,
			bool excludeOld,
			params ZString[] appCodes) where TMAWB : class, ICusMAWB
		{
			var query = new ZQuery(CusMAWBSchema.CM_JK, pk);
			if (appCodes.Length > 0)
			{
				query.AddToFilter(CusMAWBSchema.CM_ApplicationCode, appCodes);
			}
			if (isCTO != null)
			{
				query.AddToFilter(CusMAWBSchema.CM_IsCTOMAWB, isCTO.Value);
			}
			if (excludeOld)
			{
				var dateQuery = new ZQuery(CusMAWBSchema.CM_ArrivalDate, ZDateTime.Empty);
				dateQuery.AddToFilter(JoinCondition.Or, CusMAWBSchema.CM_ArrivalDate, SQLComparisonOperator.GreaterThan, ZDateTime.Now.AddMonths(-FreightDataRegistry.Instance.MAWBRecyclePeriod.Value));
				query.AddToFilter(dateQuery);
			}

			return factory.LoadTop1(ObjectFactory.GetType<TMAWB>(), query) as TMAWB;
		}

		static TMAWB GetMAWB<TMAWB>(
			BusinessObjectFactory factory,
			ZGuid pk,
			params ZString[] appCodes) where TMAWB : class, ICusMAWB
		{
			var mawbs = GetMAWBs<TMAWB>(factory, pk, reloadExistingRows: false, activeOnly: true, appCodes: appCodes);
			if (mawbs.IsNullOrEmpty())
			{
				mawbs = GetMAWBs<TMAWB>(factory, pk, reloadExistingRows: true, activeOnly: true, appCodes: appCodes);
			}
			if (mawbs.IsCountMoreThan(1))
			{
				var cusMAWBsDetails = new ZStringBuilder();
				foreach (var mawb in mawbs)
				{
					cusMAWBsDetails.Append(string.Format(CultureInfo.InvariantCulture, "{0} ({1})", mawb.GetType().FullName, mawb.PK));
				}
				ErrorReporter.ReportOnce("Did not expect so many records back for this CusMAWB and Consol", string.Format(CultureInfo.InvariantCulture, "Did not expect {1} records of CusMAWB for Consol ({0}):\r\n{2}", pk, mawbs.Count(), cusMAWBsDetails.ToStringWithNewLineBetweenAppends()));
			}

			return mawbs.FirstOrDefault();
		}

		static IEnumerable<TMAWB> GetMAWBs<TMAWB>(
			BusinessObjectFactory factory,
			ZGuid pk,
			bool reloadExistingRows,
			bool activeOnly,
			params ZString[] appCodes) where TMAWB : class, ICusMAWB
		{
			var filter = GetMAWBsFilter(pk, reloadExistingRows, appCodes);
			filter.IgnoreActiveFilter = !activeOnly;
			return factory.Load(ObjectFactory.GetType<TMAWB>(), filter).Cast<TMAWB>();
		}

		public static ZQuery GetMAWBsFilter(ZGuid pk, bool reloadExistingRows, params ZString[] appCodes)
		{
			var result = new ZQuery(CusMAWBSchema.CM_JK, pk)
			{
				ReLoadExistingRows = reloadExistingRows
			};
			if (appCodes.Length > 0)
			{
				result.AddToFilter(CusMAWBSchema.CM_ApplicationCode, appCodes);
			}

			return result;
		}

#if DEBUG
		public
#else
		internal
#endif
 static ZString[] GetAUCusMAWBAppCodes()
		{
			return new[] { new ZString(Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages) };
		}

		TMAWB GetFirstMatchingMAWB<TMAWB>(bool? isCTO, bool excludeOld, params ZString[] appCodes) where TMAWB : class, ICusMAWB
		{
			return GetFirstMatchingMAWB<TMAWB>(Factory, PK, isCTO, excludeOld, appCodes);
		}

		TMAWB GetMAWB<TMAWB>(params ZString[] appCodes) where TMAWB : class, ICusMAWB
		{
			return GetMAWB<TMAWB>(Factory, PK, appCodes);
		}

		IEnumerable<TMAWB> GetMAWBs<TMAWB>(bool reloadExistingRows, bool activeOnly, params ZString[] appCodes) where TMAWB : class, ICusMAWB
		{
			return GetMAWBs<TMAWB>(Factory, PK, reloadExistingRows, activeOnly, appCodes);
		}

		public IAUCusMAWB AUCusMAWB
		{
			get { return GetMAWB<IAUCusMAWB>(GetAUCusMAWBAppCodes()); }
		}

		public GB.CCSUK.ICusMAWBCollection GBCusMAWBs
		{
			get
			{
				// Used  by BusinessObjectsWithRelatedEvents to show these objects' workflow events on the consol form. and by consol document supporter
				var gBCusMAWBs = ObjectFactory.New<GB.CCSUK.ICusMAWBCollection>(Factory, PK);
				gBCusMAWBs.Load();
				return gBCusMAWBs;
			}
		}

		#endregion // CusMAWBs

		#region CusSCAOceanBill

		public IAUCusSCAOceanBill AUCMRCusSCAOceanBill
		{
			get
			{
				var query = new ZQuery(CusSCAOceanBillSchema.CB_ParentId, PK);
				query.AddToFilter(CusSCAOceanBillSchema.CB_ParentTableCode, JobConsolSchema.Constants.Prefix);
				query.AddToFilter(CusSCAOceanBillSchema.CB_ApplicationCode, Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages);
				query.FetchOnlyFromLocalCache = !IsInDatabase;
				query.ReLoadExistingRows = false;
				query.IgnoreActiveFilter = false;
				var oceanBills = Factory.Load<IAUCusSCAOceanBill>(query);
				if (oceanBills.IsNullOrEmpty() && IsInDatabase)
				{
					query.ReLoadExistingRows = true;
					oceanBills = Factory.Load<IAUCusSCAOceanBill>(query);
				}
				return oceanBills?.FirstOrDefault();
			}
		}

		#endregion

		#region MAWBLabelDesc

		public ZString MAWBLabelDesc
		{
			get
			{
				ZString result = "";

				if (MAWBAllocation.IsMAWBPrinted)
				{
					result = (NoResString)"Neutral Master Printed"; // Schema constant
				}
				else if (JK_IsNeutralMaster)
				{
					result = (NoResString)"Neutral Master"; // Schema constant
				}
				//Master House
				else if (IsCoLoad)
				{
					result = (NoResString)"Master House"; // Schema constant
				}
				else if (IsAgentOrDirect || IsAWBCoload)//if carrier
				{
					result = (NoResString)"Carrier Master"; // Schema constant
				}

				return result;
			}
		}

		public ZPropertyInfo MAWBLabelDescInfo
		{
			get { return GetZPropertyInfo(Schema.MAWBLabelDesc); }
		}

		#endregion

		#region MasterBillAirlinePrefix

		[MaxLength(3)]
		public ZString MasterBillAirlinePrefix
		{
			get { return JK_MasterBillNum.Left(3); }
			set
			{
				if (MasterBillAirlinePrefix != value)
				{
					CheckMaximumLength(MasterBillAirlinePrefixInfo, value);

					JK_MasterBillNum = value + MasterBillMAWB;
					if ((ImportExportHelper.IsBranchCountry(MawbPortOfLoading) || ImportExportHelper.IsExport(MawbPortOfLoading, MawbPortOfDischarge))
						&& IsValidForNeutralMaster
						&& MAWBAllocation.FreightJobMawbLink.NoJobMawbsAvailable(value, new ZString[] { JK_AWBServiceLevel, OrgCarrierServiceLevel.AllCode }) > 0)
					{
						JK_IsNeutralMaster = true;
					}

					if (JK_IsNeutralMaster)
					{
						if (MAWBAllocation.IsAllocationOfMawbAllowed)
						{
							if (MAWBAllocation.MarkForReallocationIfPrefixChanged(value))
							{
								JK_MasterBillNum = value;
							}
						}
						else
						{
							JK_IsNeutralMaster = false;
						}
					}

					UpdateCarrierByMAWB();
				}

				Validation.ValidateMasterBillNeutralMAWB();
				MasterBillAirlinePrefixInfo.RefreshBinding();
				MasterBillNeutralMAWBInfo.RefreshBinding();
			}
		}

		public void UpdateCarrierByMAWB()
		{
			if (!MasterBillAirlinePrefix.IsEmpty)
			{
				foreach (Transport transport in Transports)
				{
					if (ShouldDefaultFlightDetailsFrom(transport))
					{
						RefAirline airline = RefAirline.LoadFromAirlinePrefix(Factory, MasterBillAirlinePrefix);
						SetShippingLineBaseOnAirline(airline);
					}
				}
			}
		}

		public ZPropertyInfo MasterBillAirlinePrefixInfo
		{
			get { return GetZPropertyInfo(Schema.MasterBillAirlinePrefix); }
		}

		protected bool MasterBillAirlinePrefix_ReadOnly
		{
			get { return MAWBAllocation.IsMAWBPrinted; }
		}

		#endregion

		#region MasterBillMAWB

		[MaxLength(8)]
		[BusinessObjectTestExclude()]
		public ZString MasterBillMAWB
		{
			get
			{
				return JK_MasterBillNum.SubstringSafe(3, MasterBillMAWBInfo.MaxLength);
			}
			set
			{
				try
				{
					if (MasterBillMAWB != value)
					{
						CheckMaximumLength(MasterBillMAWBInfo, value);
						JK_MasterBillNum = MasterBillAirlinePrefix + value;

						Shipments.Cast<ForwardingShipment>().ForEach(x => x.AviationSecurityParty_HasChanges(SupplyChainSecurityOrganisationTypes.ConsolForwarderYouHaveBorrowedMAWBStockFrom));
					}

					MasterBillMAWBInfo.RefreshBinding();
					MasterBillNeutralMAWBInfo.RefreshBinding();
				}
				catch (Exception ex)
				{
					var stackTrace = new System.Diagnostics.StackTrace(true).ToString();
					ErrorReporter.ReportOnce("StackTrace of error while setting MasterBillMAWB:\n" + stackTrace, ex);
					throw;
				}
			}
		}

		public ZPropertyInfo MasterBillMAWBInfo
		{
			get { return GetZPropertyInfo(Schema.MasterBillMAWB); }
		}

		protected bool MasterBillMAWB_ReadOnly
		{
			get { return JK_IsNeutralMaster && IsValidForNeutralMaster; }
		}

		public ZString MasterBillNeutralMAWB
		{
			get
			{
				return MAWBAllocation.MasterBillNeutralMAWB;
			}
		}

		public ZPropertyInfo MasterBillNeutralMAWBInfo
		{
			get { return GetZPropertyInfo(Schema.MasterBillNeutralMAWB); }
		}

		#endregion

		protected override NumberGeneratorTarget NewMasterBillNumberGeneratorTarget()
		{
			return !IsInDatabase && ShouldAutoAllocateMasterBillNumbersToConsols() ? new RoadConsolMasterBillNumberGeneratorTarget() : (NumberGeneratorTarget)null;
		}

		public bool SuppressAutoGenerateMasterBillNumber { get; set; }

		bool ShouldAutoAllocateMasterBillNumbersToConsols()
		{
			if (JK_TransportMode == Constants.TransportModes.Road && JK_MasterBillNum.IsEmpty && !SuppressAutoGenerateMasterBillNumber)
			{
				var bolNumberCustomisation = FreightConfigurationRegistry.Instance.RoadConsolMasterBillNumber.GetFallBackValueAtAllLevels(Env.CurrentCompanyPK, Env.CurrentBranchPK, Env.CurrentDepartmentPK);

				return bolNumberCustomisation != null && bolNumberCustomisation.AutoAllocateMasterBillNumbersToConsols;
			}

			return false;
		}

		public bool AreAllShipmentsApprovedForAviationSecurity
		{
			get { return Shipments.Cast<ForwardingShipment>().All(shipment => shipment.AviationSecurity.IsApprovedForAviationSecurity); }
		}

		public bool HasOnlyEmptyContainers
		{
			get { return !Containers.Cast<CommonContainer>().Any(container => container.JC_ContainerMode != Core.Constants.ContainerModes.Empty); }
		}

		public ZBool IsNeutralMAWBPrinted
		{
			get { return JK_IsNeutralMaster && MAWBAllocation.IsMAWBPrinted; }
		}

		#region ForwarderAddressWithContact

		#region ReceivingForwarderWithContact

		[EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
		public ZAddressWithContact ReceivingForwarderWithContact
		{
			get
			{
				if (receivingForwarderWithContact == null)
				{
					using (SuspendSettingHasChanges())
					{
						receivingForwarderWithContact = GetNewReceivingForwarderWithContact();
					}
				}
				return receivingForwarderWithContact;
			}
		}

		void UpdateReceivingForwarderHandlingType(object sender, EventArgs e)
		{
			if (ReceivingForwarderWithContact?.OrgPK == Guid.Empty)
			{
				JK_ReceivingForwarderHandlingType = ZString.Empty;
			}
		}

		ZAddressWithContact receivingForwarderWithContact;

		protected virtual ZAddressWithContact GetNewReceivingForwarderWithContact()
		{
			var newReceivingForwarder = new ZAddressWithContact(JK_OC_ReceivingForwarderContactInfo, JK_OA_ReceivingForwarderAddressInfo, GetDefaultContactForReceivingForwarder);
			newReceivingForwarder.GetDefaultAddress = org => base.GetDefaultReceivingForwarderAddress(org as OrgHeader, true);
			newReceivingForwarder.OrgPKInfo.ValueChanged -= UpdateReceivingForwarderHandlingType;
			newReceivingForwarder.OrgPKInfo.ValueChanged += UpdateReceivingForwarderHandlingType;
			return newReceivingForwarder;
		}

		ZGuid GetDefaultContactForReceivingForwarder(IOrgHeader orgHeader, IOrgAddress orgAddress)
		{
			return GetDefaultContact(orgHeader as OrgHeader, orgAddress, ContactType.ImportFreightAgent, ContactType.ImportAirFreightAgent, ContactType.ImportSeaFreightAgent);
		}

		#endregion

		#region SendingForwarderWithContact

		[EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
		public ZAddressWithContact SendingForwarderWithContact
		{
			get
			{
				if (sendingForwarderWithContact == null)
				{
					using (SuspendSettingHasChanges())
					{
						sendingForwarderWithContact = GetNewSendingForwarderWithContact();
					}
				}
				return sendingForwarderWithContact;
			}
		}
		ZAddressWithContact sendingForwarderWithContact;

		protected virtual ZAddressWithContact GetNewSendingForwarderWithContact()
		{
			var newSendingForwarder = new ZAddressWithContact(JK_OC_SendingForwarderContactInfo, JK_OA_SendingForwarderAddressInfo, GetDefaultContactForSendingForwarder);
			newSendingForwarder.GetDefaultAddress = org => base.GetDefaultSendingForwarderAddress(org as OrgHeader, true);
			return newSendingForwarder;
		}

		ZGuid GetDefaultContactForSendingForwarder(IOrgHeader orgHeader, IOrgAddress orgAddress)
		{
			return GetDefaultContact(orgHeader as OrgHeader, orgAddress, ContactType.ExportFreightAgent, ContactType.ExportAirFreightAgent, ContactType.ExportSeaFreightAgent);
		}

		#endregion

		ZGuid GetDefaultContact(OrgHeader orgHeader, IOrgAddress orgAddress, ContactType defaultType, ContactType defaultAirType, ContactType defaultSeaType)
		{
			if (orgHeader == null || orgAddress == null)
			{
				return ZGuid.Empty;
			}
			var matchingContacts = orgHeader.GetActiveContacts().Cast<OrgContact>().Where(t => t.WorkingAddressPK == orgAddress.PK);
			Factory.AddFetchHint(typeof(OrgDocument), new ZQuery(OrgDocumentSchema.OD_OC, matchingContacts.Select(contact => contact.PK)));
			var matchingCount = matchingContacts.Count();

			if (matchingCount == 1)
			{
				return matchingContacts.First().PK;
			}
			else if (matchingCount == 0)
			{
				return ZGuid.Empty;
			}

			OrgContact matchingContact = null;
			if (JK_TransportMode == Constants.TransportModes.Air)
			{
				matchingContact = GetDefaultContactForType(matchingContacts, defaultAirType);
			}
			else if (JK_TransportMode == Constants.TransportModes.Sea)
			{
				matchingContact = GetDefaultContactForType(matchingContacts, defaultSeaType);
			}

			if (matchingContact == null)
			{
				matchingContact = GetDefaultContactForType(matchingContacts, defaultType);
			}
			return matchingContact?.PK ?? ZGuid.Empty;
		}

		OrgContact GetDefaultContactForType(IEnumerable<OrgContact> matchingContacts, ContactType type)
		{
			foreach (var contact in matchingContacts)
			{
				if (contact.OC_IsActive && contact.Documents.ContainsDefault(type.Code))
				{
					return contact;
				}
			}
			return null;
		}

		#endregion

		#region JK_RL_NKLoadPort

		public override ZString JK_RL_NKLoadPort
		{
			get
			{
				return base.JK_RL_NKLoadPort;
			}
			set
			{
				if (JK_RL_NKLoadPort != value)
				{
					base.JK_RL_NKLoadPort = value;
					DefaultCarrierBookingAgentFromLoadPort();

					foreach (ForwardingShipment shipment in Shipments)
					{
						shipment.SetHandledOnBehalfOfForwarder(ForwardingShipment.ShipmentPropertyChanged.ConsolLoadPort);
					}

					RequiredDocuments.SetAllDocumentsReceivedEventLogger();
					DefaultSpecialHandlingItems();

					if (!IsValidationSuspended)
					{
						Validation.ValidateJK_SendingForwarderHandlingType();
					}

					this.UpdateCO2eStatusToNotCurrent(new CO2eStatusChangedReason(JK_RL_NKLoadPortInfo), IsCopying);
				}
			}
		}

		#endregion

		#region JK_RL_NKLoadForExportTransport

		public ZString JK_RL_NKLoadForExportTransport
		{
			get
			{
				Transport transport = Transports.ExportTransport;
				return transport == null ? ZString.Empty : transport.JW_RL_NKLoadPort;
			}
		}

		public ZPropertyInfo JK_RL_NKLoadForExportTransportInfo
		{
			get { return GetZPropertyInfo(Schema.JK_RL_NKLoadForExportTransport); }
		}

		#endregion

		#region JK_RL_NKDiscForExportTransport

		public ZString JK_RL_NKDiscForExportTransport
		{
			get
			{
				Transport transport = Transports.ExportTransport;
				return transport == null ? ZString.Empty : transport.JW_RL_NKDiscPort;
			}
		}

		public ZPropertyInfo JK_RL_NKDiscForExportTransportInfo
		{
			get { return GetZPropertyInfo(Schema.JK_RL_NKDiscForExportTransport); }
		}

		public ZDateTime JK_DepartureForFirstExportTransport
		{
			get
			{
				if (FirstExportTransport == null)
				{
					return ZDateTime.Empty;
				}

				return FirstExportTransport.JW_ATD.IsEmpty ? FirstExportTransport.JW_ETD : FirstExportTransport.JW_ATD;
			}
		}

		public ZDateTime JK_ATDForFirstExportTransport => FirstExportTransport?.JW_ATD ?? ZDateTime.Empty;

		#endregion

		#region JK_DepartureForFirstTransport

		public ZDateTime JK_DepartureForFirstTransport
		{
			get
			{
				var firstTransport = Transports?.Cast<Transport>().FirstOrDefault();
				if (firstTransport == null)
				{
					return ZDateTime.Empty;
				}

				return !firstTransport.JW_ATD.IsEmpty ? firstTransport.JW_ATD : firstTransport.JW_ETD;
			}
		}

		public ZPropertyInfo JK_DepartureForFirstTransportInfo
		{
			get => GetWrappedZPropertyInfo(Schema.JK_DepartureForFirstTransport, x => GetJK_DepartureForFirstTransportInfo());
		}

		ZPropertyInfo GetJK_DepartureForFirstTransportInfo()
		{
			var firstTransport = Transports?.Cast<Transport>().FirstOrDefault();
			if (firstTransport == null)
			{
				return GetZPropertyInfo(Schema.JK_DepartureForFirstTransport);
			}
			return !firstTransport.JW_ATD.IsEmpty ? firstTransport.JW_ATDInfo : firstTransport.JW_ETDInfo;
		}

		#endregion

		#region JK_RL_NKLoadForImportTransport

		public ZString JK_RL_NKLoadForImportTransport
		{
			get
			{
				Transport transport = Transports.ImportTransport;
				return transport == null ? ZString.Empty : transport.JW_RL_NKLoadPort;
			}
		}

		public ZPropertyInfo JK_RL_NKLoadForImportTransportInfo
		{
			get { return GetZPropertyInfo(Schema.JK_RL_NKLoadForImportTransport); }
		}

		public virtual ZString JK_VoyageFlightForLastImportTransport
		{
			get { return LastImportTransport == null ? ZString.Empty : LastImportTransport.JW_VoyageFlight; }
		}

		#endregion

		#region JK_RL_NKDiscForImportTransport

		public ZString JK_RL_NKDiscForImportTransport
		{
			get
			{
				Transport transport = Transports.ImportTransport;
				return transport == null ? ZString.Empty : transport.JW_RL_NKDiscPort;
			}
		}

		public ZPropertyInfo JK_RL_NKDiscForImportTransportInfo
		{
			get { return GetZPropertyInfo(Schema.JK_RL_NKDiscForImportTransport); }
		}

		#endregion

		#region JK_ATAForImportTransport

		public ZDateTime JK_ATAForImportTransport
		{
			get
			{
				Transport transport = Transports.ImportTransport;
				return transport == null ? ZDateTime.Empty : transport.JW_ATA;
			}
		}

		public ZPropertyInfo JK_ATAForImportTransportInfo
		{
			get { return GetZPropertyInfo(Schema.JK_ATAForImportTransport); }
		}

		#endregion

		#region JK_ATAForLastTransport

		public ZDateTime JK_ATAForLastTransport
		{
			get
			{
				var lastTransport = Transports?.Cast<Transport>().OrderBy(x => x.JW_LegOrder).LastOrDefault();
				if (lastTransport == null)
				{
					return ZDateTime.Empty;
				}

				return lastTransport.JW_ATA;
			}
		}

		public ZPropertyInfo JK_ATAForLastTransportInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.JK_ATAForLastTransport, x => GetJK_ATAForLastTransportInfo()); }
		}

		ZPropertyInfo GetJK_ATAForLastTransportInfo()
		{
			var last = Transports?.Cast<Transport>().OrderBy(transport => transport.JW_LegOrder).LastOrDefault();
			if (last == null)
			{
				return GetZPropertyInfo(Schema.JK_ATAForLastTransport);
			}
			return last.JW_ATAInfo;
		}

		#endregion

		#region JK_ETAForImportTransport

		public ZDateTime JK_ETAForImportTransport
		{
			get
			{
				Transport transport = Transports.ImportTransport;
				return transport == null ? ZDateTime.Empty : transport.JW_ETA;
			}
		}

		public ZPropertyInfo JK_ETAForImportTransportInfo
		{
			get { return GetZPropertyInfo(Schema.JK_ETAForImportTransport); }
		}

		#endregion

		#region Earliest CTO Storage Start

		public ZDateTime EarliestCTOStorageStartForBinding
		{
			get
			{
				var earliestContainer = Containers?
					.Cast<ForwardingContainer>()
					.OrderBy(t => t.JC_ArrivalCTOStorageStartDate)
					.FirstOrDefault();

				return earliestContainer == null ? ZDateTime.Empty : earliestContainer.JC_ArrivalCTOStorageStartDate;
			}
		}

		public ZPropertyInfo EarliestCTOStorageStartForBindingInfo => GetZPropertyInfo(nameof(EarliestCTOStorageStartForBinding));

		#endregion

		#region Earliest Empty Required By Date

		public ZDateTime EarliestEmptyRequiredByDateForBinding
		{
			get
			{
				var earliestContainer = Containers?
					.Cast<ForwardingContainer>()
					.OrderBy(t => t.JC_EmptyReturnedBy)
					.FirstOrDefault();

				return earliestContainer == null ? ZDateTime.Empty : earliestContainer.JC_EmptyReturnedBy;
			}
		}

		public ZPropertyInfo EarliestEmptyRequiredByDateForBindingInfo => GetZPropertyInfo(nameof(EarliestEmptyRequiredByDateForBinding));

		#endregion

		#region RoutingLegColumns

		Transport FirstSeaTransport
		{
			get
			{
				return Transports?
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
				return Transports?
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

		public ZDateTime FirstLegLoadPortETDForBinding
		{
			get
			{
				var firstTransport = Transports?.Cast<Transport>().OrderBy(t => t.JW_LegOrder).FirstOrDefault();
				return firstTransport == null ? ZDateTime.Empty : firstTransport.JW_ETD;
			}
		}

		public ZPropertyInfo FirstLegLoadPortETDForBindingInfo => GetZPropertyInfo(nameof(FirstLegLoadPortETDForBinding));

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

		public ZDateTime LastLegDischargePortETAForBinding
		{
			get
			{
				var lastTransport = Transports?.Cast<Transport>().OrderByDescending(t => t.JW_LegOrder).FirstOrDefault();
				return lastTransport == null ? ZDateTime.Empty : lastTransport.JW_ETA;
			}
		}

		public ZPropertyInfo LastLegDischargePortETAForBindingInfo => GetZPropertyInfo(nameof(LastLegDischargePortETAForBinding));

		#endregion

		#region Verification/Pre-Allocation Details

		[List("WeightUnits")]
		[MaxLength(CommonConsol.Schema.JK_TotalShipmentChargeableUnitMaxLength)]
		[ReadOnlyMember(nameof(IsPreAllocationsReadOnly))]
		public ZString WeightVerificationUnit
		{
			get { return fWeightVerificationUnit; }
			set
			{
				if (fWeightVerificationUnit != value)
				{
					CheckMaximumLength(WeightVerificationUnitInfo, value);
					SetNonPersistentPropertyValue(WeightVerificationUnitInfo, ref fWeightVerificationUnit, value);

					this.SetRoundedValue(JobConsolSchema.JK_TotalShipmentActWeightCheck, JK_TotalShipmentActWeightCheckInfo);

					if (!IsValidationSuspended)
					{
						Validation.ValidateWeightVerificationUnit();
					}
					UpdateVerificationUnits();
				}
			}
		}

		ZString fWeightVerificationUnit;

		public ZPropertyInfo WeightVerificationUnitInfo
		{
			get { return GetZPropertyInfo(nameof(WeightVerificationUnit)); }
		}

		[List("VolumeUnits")]
		[MaxLength(CommonConsol.Schema.JK_TotalShipmentChargeableUnitMaxLength)]
		[ReadOnlyMember(nameof(IsPreAllocationsReadOnly))]
		public ZString VolumeVerificationUnit
		{
			get { return fVolumeVerificationUnit; }
			set
			{
				if (fVolumeVerificationUnit != value)
				{
					CheckMaximumLength(VolumeVerificationUnitInfo, value);
					SetNonPersistentPropertyValue(VolumeVerificationUnitInfo, ref fVolumeVerificationUnit, value);

					this.SetRoundedValue(JobConsolSchema.JK_TotalShipmentActVolumeCheck, JK_TotalShipmentActVolumeCheckInfo);

					if (!IsValidationSuspended)
					{
						Validation.ValidateVolumeVerificationUnit();
					}
					UpdateVerificationUnits();
				}
			}
		}

		ZString fVolumeVerificationUnit;

		public ZPropertyInfo VolumeVerificationUnitInfo
		{
			get { return GetZPropertyInfo(nameof(VolumeVerificationUnit)); }
		}

		void UpdateVerificationUnits()
		{
			if (!IsLoading)
			{
				if (IsAir)
				{
					JK_TotalShipmentChargeableUnit = WeightVerificationUnit;
					JK_TotalShipmentActOtherUnit = VolumeVerificationUnit;
				}
				else
				{
					JK_TotalShipmentChargeableUnit = VolumeVerificationUnit;
					JK_TotalShipmentActOtherUnit = WeightVerificationUnit;
				}
			}
		}

		#region AfterUniversalCopy

		protected void AfterUniversalCopy()
		{
			using (SuspendSettingHasChanges())
			{
				fWeightVerificationUnit = IsAir ? JK_TotalShipmentChargeableUnit : JK_TotalShipmentActOtherUnit;
				fVolumeVerificationUnit = IsAir ? JK_TotalShipmentActOtherUnit : JK_TotalShipmentChargeableUnit;
			}

			using (DisableAutoUpdatePackLineContainers())
			{
				Containers.ReloadFromLocalCache();

				foreach (var container in Containers.Cast<ForwardingContainer>())
				{
					foreach (var pivotWithAbsentConShipLink in ContainerPackLineRelationshipHelper.GetContainerPackPivotsWithAbsentConShipLink(Factory, container, null))
					{
						if (pivotWithAbsentConShipLink.PackLine?.Shipment != null)
						{
							Shipments.Add(pivotWithAbsentConShipLink.PackLine.Shipment);
						}
					}
				}
			}
		}

		#endregion

		[ReadOnlyMember(nameof(IsPreAllocationsReadOnly))]
		[MeasureUnit(Schema.VolumeVerificationUnit, MeasureUnitType.Volume)]
		public override ZDecimal JK_TotalShipmentActVolumeCheck
		{
			get { return base.JK_TotalShipmentActVolumeCheck; }
			set
			{
				var roundedValue = this.GetRoundedValue(JobConsolSchema.JK_TotalShipmentActVolumeCheck, JK_TotalShipmentActVolumeCheckInfo, value);

				if (base.JK_TotalShipmentActVolumeCheck != roundedValue)
				{
					base.JK_TotalShipmentActVolumeCheck = roundedValue;
					if (!IsValidationSuspended)
					{
						Validation.ValidateVolumeVerificationUnit();
					}
				}
			}
		}

		[ReadOnlyMember(nameof(IsPreAllocationsReadOnly))]
		[MeasureUnit(Schema.WeightVerificationUnit, MeasureUnitType.Weight)]
		public override ZDecimal JK_TotalShipmentActWeightCheck
		{
			get { return base.JK_TotalShipmentActWeightCheck; }
			set
			{
				var roundedValue = this.GetRoundedValue(JobConsolSchema.JK_TotalShipmentActWeightCheck, JK_TotalShipmentActWeightCheckInfo, value);

				if (base.JK_TotalShipmentActWeightCheck != roundedValue)
				{
					base.JK_TotalShipmentActWeightCheck = roundedValue;
					if (!IsValidationSuspended)
					{
						Validation.ValidateWeightVerificationUnit();
					}
					this.UpdateCO2eStatusToNotCurrent(new CO2eStatusChangedReason(JK_TotalShipmentActWeightCheckInfo), IsCopying);
				}
			}
		}

		[ReadOnlyMember(nameof(IsPreAllocationsReadOnly))]
		public override ZDecimal JK_TotalShipmentChargableCheck
		{
			get { return base.JK_TotalShipmentChargableCheck; }
			set { base.JK_TotalShipmentChargableCheck = this.GetRoundedValue(JobConsolSchema.JK_TotalShipmentChargableCheck, JK_TotalShipmentChargableCheckInfo, value); }
		}

		public override ZString JK_TotalShipmentChargeableUnit
		{
			get { return base.JK_TotalShipmentChargeableUnit; }
			set
			{
				var changed = JK_TotalShipmentChargeableUnit != value;
				base.JK_TotalShipmentChargeableUnit = value;

				if (IsAir)
				{
					WeightVerificationUnit = JK_TotalShipmentChargeableUnit;
					if (UsePreAllocationWeight && changed)
					{
						this.UpdateCO2eStatusToNotCurrent(new CO2eStatusChangedReason(JK_TotalShipmentChargeableUnitInfo), IsCopying);
					}
				}
				else
				{
					VolumeVerificationUnit = JK_TotalShipmentChargeableUnit;
				}

				this.SetRoundedValue(JobConsolSchema.JK_TotalShipmentChargableCheck, JK_TotalShipmentChargableCheckInfo);
			}
		}

		public override ZString JK_TotalShipmentActOtherUnit
		{
			get { return base.JK_TotalShipmentActOtherUnit; }
			set
			{
				var changed = JK_TotalShipmentActOtherUnit != value;
				base.JK_TotalShipmentActOtherUnit = value;

				if (IsAir)
				{
					VolumeVerificationUnit = JK_TotalShipmentActOtherUnit;
				}
				else
				{
					WeightVerificationUnit = JK_TotalShipmentActOtherUnit;
					if (UsePreAllocationWeight && changed)
					{
						this.UpdateCO2eStatusToNotCurrent(new CO2eStatusChangedReason(JK_TotalShipmentActOtherUnitInfo), IsCopying);
					}
				}
			}
		}

		[ReadOnlyMember(nameof(IsPreAllocationsReadOnly))]
		public override ZShort JK_TotalShipmentCountCheck
		{
			get { return base.JK_TotalShipmentCountCheck; }
			set { base.JK_TotalShipmentCountCheck = value; }
		}

		protected bool IsPreAllocationsReadOnly
		{
			get
			{
				return IsGatewaySendingAgent ? !Env.Security.GatewayConsolPreAllocationEditing.IsAllowed : !Env.Security.ConsolPreAllocationEditing.IsAllowed;
			}
		}

		public bool IsGatewaySendingAgent
		{
			get
			{
				var gatewayAgentTypes = new List<string>()
				{
					AgentStatusList.Codes.GatewayAgent,
					AgentStatusList.Codes.GatewayAgentWithTariff,
				};

				return gatewayAgentTypes.Contains(base.JK_SendingForwarderHandlingType);
			}
		}

		#endregion

		#region FreightPayableAt

		public RefUNLOCO FreightPayableAt
		{
			get
			{
				RefUNLOCO result = null;

				if (IsDirect)
				{
					if (DirectShipment != null)
					{
						result = DirectShipment.FreightPayableAt;
						if (result == null)
						{
							if (DirectShipment.IsCollect)
							{
								result = DeliveryLocation;
							}
							else if (DirectShipment.IsPrepaid)
							{
								result = PickupLocation;
							}
						}
					}
				}
				else if (JK_PrepaidCollect == Core.Constants.PaymentType.Prepaid)
				{
					return LoadPort;
				}
				else if (JK_PrepaidCollect == Core.Constants.PaymentType.Collect)
				{
					return DischargePort;
				}

				return result;
			}
		}

		public ForwardingShipment DirectShipment
		{
			get
			{
				ForwardingShipment result = null;
				if (IsDirect)
				{
					result = Shipments
						.OfType<ForwardingShipment>()
						.FirstOrDefault(shipment => shipment.JS_JS_ColoadMasterShipment.IsEmpty && shipment.IsDirectShipment);
				}

				return result;
			}
		}

		#endregion

		#region PickupLocation

		public RefUNLOCO PickupLocation
		{
			get
			{
				RefUNLOCO result = null;

				if (Shipments.Count > 0)
				{
					result = Shipments[0].Origin;
					if (result != null)
					{
						foreach (ForwardingShipment shipment in Shipments)
						{
							if (shipment.Origin == null || shipment.Origin.RL_Code != result.RL_Code)
							{
								result = null;
								break;
							}
						}
					}
				}

				if (result == null && PackDepotAddress != null)
				{
					result = PackDepotAddress.RelatedPortCode;
				}

				if (result == null)
				{
					result = LoadPort;
				}

				return result;
			}
		}

		#endregion

		#region DeliveryLocation

		public RefUNLOCO DeliveryLocation
		{
			get
			{
				RefUNLOCO result = null;

				if (Shipments.Count > 0)
				{
					result = Shipments[0].Destination;
					if (result != null)
					{
						foreach (ForwardingShipment shipment in Shipments)
						{
							if (shipment.Destination == null || shipment.Destination.RL_Code != result.RL_Code)
							{
								result = null;
								break;
							}
						}
					}
				}

				if (result == null && UnpackDepotAddress != null)
				{
					result = UnpackDepotAddress.RelatedPortCode;
				}

				if (result == null)
				{
					result = DischargePort;
				}

				return result;
			}
		}

		#endregion

		#region InboundFlightDestinationLoco

		public RefUNLOCO InboundFlightDestinationLoco
		{
			get
			{
				return Transports?
					.Cast<Transport>()
					.Where(t => t.JW_TransportMode == TransportModes.Air)
					.OrderBy(t => t.JW_LegOrder)
					.LastOrDefault()?.DiscPort;
			}
		}

		#endregion

		#region JK_ConsolMode

		public override ZString JK_ConsolMode
		{
			get
			{
				return base.JK_ConsolMode;
			}

			set
			{
				if (JK_ConsolMode != value)
				{
					using (RequireTEUMonitor.MonitorChange(new CO2eStatusChangedReason(JK_ConsolModeInfo), UpdateTransportLegsCO2eStatusToNCU))
					{
						base.JK_ConsolMode = value;
					}

					if (value != Constants.ContainerModes.Groupage)
					{
						Validation.ValidateJK_RCA_AllocationLine();

						foreach (var container in this.Containers.OfType<ForwardingContainer>().Where(container => !container.JC_RCA_AllocationLine.IsEmpty))
						{
							container.Validation.ValidateJC_RCA_AllocationLine();
						}
					}
				}
			}
		}

		void UpdateTransportLegsCO2eStatusToNCU()
		{
			Transports.Cast<Transport>().ForEach(leg => leg.UpdateCO2eStatusToNotCurrent());
		}

		#endregion

		#region JK_Calc_DGClass and JK_Calc_DGSubstance

		public ZString JK_Calc_DGClass
		{
			get => GetDGRestrictionsCollection(dgRestriction => dgRestriction.JKD_Class);
		}

		public ZString JK_Calc_DGSubstance
		{
			get => GetDGRestrictionsCollection(dgRestriction => dgRestriction.JKD_Calc_Substance);
		}

		ZString GetDGRestrictionsCollection(Func<ConsolDGRestrictions, ZString> getDGRestrictionList)
		{
			var dgRestrictionList = ConsolDGRestrictionCollection.Select(dg => getDGRestrictionList(dg)).Distinct().ToList();

			switch (dgRestrictionList.Count)
			{
				case 0:
					return ZString.Empty;
				case 1:
					return dgRestrictionList.First();
				default:
					return Res.GetString("00330656-EE1B-41E9-BD48-4003D6249AC4", "Mixed");
			}
		}

		#endregion

		#region JK_Calc_CarrierBookingLatest*

		StmALog JKCalcCarrierBookingLatestLog()
		{
			var documentDataLoader = ObjectFactory.Get<IVisualizerDocumentDataLoader>();
			var seaBookingRequest2DocumentData = documentDataLoader.Load(this, ConsolDocumentDataStoreNames.SeaBookingRequest2) as VisualizerDocumentData;

			if (seaBookingRequest2DocumentData == null)
			{
				return null;
			}

			var logs = seaBookingRequest2DocumentData.Logs.GetAllLogs().OfType<StmALog>();

			var shippingInstructionEventCodes = new List<string>() { Events.MessageSentCode, Events.InterchangeReceiptAcknowledgedCode, Events.InterchangeRejectedCode, Events.MessageAcceptedCode, Events.MessageRejectedCode, Events.StatusUpdatedCode, Events.MessagePendingProcessingCode };

			var resultLog = logs
				.Where(log => log.Parameters.GetValueSafe(EventConstants.EventReferenceParameters.Codes.MessageType) == ConsolDocumentNames.ShippingInstruction && !log.IsCancelled && shippingInstructionEventCodes.Contains(log.SL_SE_NKEvent))
				.OrderByDescending(log => log.SL_PostedTimeUtc).FirstOrDefault();

			if (resultLog == null)
			{
				var bookingRequestEventCodes = new List<string>(shippingInstructionEventCodes) { Events.MessageWithdrawCancelRequestCode, Events.MessageWithdrawCancelAcceptedCode };

				resultLog = logs
					.Where(log => log.Parameters.GetValueSafe(EventConstants.EventReferenceParameters.Codes.MessageType) == ConsolDocumentNames.BookingRequest && !log.IsCancelled && bookingRequestEventCodes.Contains(log.SL_SE_NKEvent))
					.OrderByDescending(log => log.SL_PostedTimeUtc).FirstOrDefault();
			}

			return resultLog;
		}

		#region JK_Calc_CarrierBookingLatestStatus

		[List("JK_CarrierBookingStatus_List")]
		public ZString JK_Calc_CarrierBookingLatestStatus
		{
			get => CalcCarrierBookingLatestStatus(JKCalcCarrierBookingLatestLog());
		}

		public ZPropertyInfo JK_Calc_CarrierBookingLatestStatusInfo
		{
			get => GetZPropertyInfo(nameof(JK_Calc_CarrierBookingLatestStatus));
		}

		ZString CalcCarrierBookingLatestStatus(StmALog latestLog)
		{
			if (latestLog == null || latestLog.SL_SE_NKEvent == Events.StatusUpdatedCode)
			{
				return FreightConstants.CarrierBookingStatus.Codes.NotSent;
			}

			if (latestLog.Parameters.GetValueSafe(EventConstants.EventReferenceParameters.Codes.MessageType) == ConsolDocumentNames.ShippingInstruction)
			{
				switch (latestLog.SL_SE_NKEvent)
				{
					case Events.MessageSentCode:
						return FreightConstants.CarrierBookingStatus.Codes.ShippingInstruction.Sent;
					case Events.InterchangeReceiptAcknowledgedCode:
						return FreightConstants.CarrierBookingStatus.Codes.ShippingInstruction.Acknowledged;
					case Events.InterchangeRejectedCode:
						return FreightConstants.CarrierBookingStatus.Codes.ShippingInstruction.RejectedByInterchange;
					case Events.MessageAcceptedCode:
						return FreightConstants.CarrierBookingStatus.Codes.ShippingInstruction.Confirmed;
					case Events.MessageRejectedCode:
						return FreightConstants.CarrierBookingStatus.Codes.ShippingInstruction.Rejected;
					case Events.MessagePendingProcessingCode:
						return FreightConstants.CarrierBookingStatus.Codes.ShippingInstruction.PendingProcessing;
				}
			}
			else
			{
				switch (latestLog.SL_SE_NKEvent)
				{
					case Events.MessageSentCode:
						return FreightConstants.CarrierBookingStatus.Codes.BookingRequest.Sent;
					case Events.InterchangeReceiptAcknowledgedCode:
						return FreightConstants.CarrierBookingStatus.Codes.BookingRequest.Acknowledged;
					case Events.InterchangeRejectedCode:
						return FreightConstants.CarrierBookingStatus.Codes.BookingRequest.RejectedByInterchange;
					case Events.MessageAcceptedCode:
						return FreightConstants.CarrierBookingStatus.Codes.BookingRequest.Confirmed;
					case Events.MessageRejectedCode:
						if (HasMessageWithdrawCancelRequestCode())
						{
							return FreightConstants.CarrierBookingStatus.Codes.BookingRequest.WithdrawalRejected;
						}
						return FreightConstants.CarrierBookingStatus.Codes.BookingRequest.Rejected;
					case Events.MessageWithdrawCancelRequestCode:
						return FreightConstants.CarrierBookingStatus.Codes.BookingRequest.WithdrawalSent;
					case Events.MessageWithdrawCancelAcceptedCode:
						return FreightConstants.CarrierBookingStatus.Codes.BookingRequest.WithdrawalAccepted;
					case Events.MessagePendingProcessingCode:
						return FreightConstants.CarrierBookingStatus.Codes.BookingRequest.PendingProcessing;
				}
			}

			return ZString.Empty;
		}

		bool HasMessageWithdrawCancelRequestCode()
		{
			var documentDataLoader = ObjectFactory.Get<IVisualizerDocumentDataLoader>();
			var seaBookingRequest2DocumentData = documentDataLoader.Load(this, ConsolDocumentDataStoreNames.SeaBookingRequest2) as VisualizerDocumentData;

			if (seaBookingRequest2DocumentData == null)
			{
				return false;
			}

			var bookingRequestLog = seaBookingRequest2DocumentData.Logs.GetAllLogs().OfType<StmALog>()
				.Where(log => log.Parameters.GetValueSafe(EventConstants.EventReferenceParameters.Codes.MessageType) == ConsolDocumentNames.BookingRequest && !log.IsCancelled && (log.SL_SE_NKEvent == Events.MessageWithdrawCancelRequestCode || log.SL_SE_NKEvent == Events.StatusUpdatedCode))
				.OrderByDescending(log => log.SL_EventTime)
				.FirstOrDefault();

			return bookingRequestLog != null && bookingRequestLog.SL_SE_NKEvent == Events.MessageWithdrawCancelRequestCode;
		}

		#endregion

		#region JK_Calc_CarrierBookingLatestDate

		public ZDateTime JK_Calc_CarrierBookingLatestDate
		{
			get
			{
				var latestLog = JKCalcCarrierBookingLatestLog();
				return (latestLog == null || latestLog.SL_SE_NKEvent == Events.StatusUpdatedCode) ? ZDateTime.Empty : latestLog.SL_PostedTimeUtc;
			}
		}

		public ZPropertyInfo JK_Calc_CarrierBookingLatestDateInfo
		{
			get => GetZPropertyInfo(nameof(JK_Calc_CarrierBookingLatestDate));
		}

		#endregion

		#endregion

		#region ShowBECertifiedPickupDocument

		public bool HasCertifiedPickupAcceptOrDecline => Containers
			.Cast<ForwardingContainer>()
			.Any(container =>
			{
				return CertifiedPickupContainerEventHelper.CheckStatusIsReadyToShowInAcceptDeclineMode(container);
			});

		public bool HasCertifiedPickupTransfer => Containers
			.Cast<ForwardingContainer>()
			.Any(container =>
			{
				return CertifiedPickupContainerEventHelper.CheckStatusIsReadyToShowInTransferMode(container);
			});

		public bool HasCertifiedPickupRevoke => Containers
			.Cast<ForwardingContainer>()
			.Any(container =>
			{
				return CertifiedPickupContainerEventHelper.CheckStatusIsReadyToShowInRevokeMode(container);
			});

		#endregion

		#region ShowMultiModeDGDocument

		public bool HasContainerizedDGs => Containers
			.Cast<ForwardingContainer>()
			.Any(c => c.PackLines
				.Cast<ForwardingPackLine>()
				.Any(p => p.UNDGs
					.Any(u => !u.SubstanceCode.IsEmpty)));

		#endregion

		#region ShowSecureContainerReleaseDocument

		public bool HasSecureContainerReleaseTransfer => Containers
			.Cast<ForwardingContainer>()
			.Any(container =>
			{
				return SecureContainerReleaseContainerEventHelper.CheckStatusIsReadyToShowInTransferMode(SecureContainerReleaseContainerEventHelper.GetCurrentStatusFromEvents(container));
			});

		public bool HasSecureContainerReleaseRevoke => Containers
			.Cast<ForwardingContainer>()
			.Any(container =>
			{
				return SecureContainerReleaseContainerEventHelper.CheckStatusIsReadyToShowInRevokeMode(SecureContainerReleaseContainerEventHelper.GetCurrentStatusFromEvents(container));
			});

		public bool HasSecureContainerReleaseAuthorisedEvent => Containers
			.Cast<ForwardingContainer>()
			.Any(c => c.Logs
				.GetAllLogs()
				.OfType<StmALog>()
				.Any(l => !l.SL_IsCancelled
					&& l.SL_SE_NKEvent == Events.AuthorisedCode
					&& IsSecureContainerReleasePickupLog(l, (NoResString)"Secure Container Release")));

		public bool HasSecureContainerReleaseMessageAcceptedEvent => Containers
			.Cast<ForwardingContainer>()
			.Any(c => c.Logs
				.GetAllLogs()
				.OfType<StmALog>()
				.Any(l => !l.SL_IsCancelled
					&& l.SL_SE_NKEvent == Events.MessageAcceptedCode
					&& IsSecureContainerReleasePickupLog(l, (NoResString)"Secure Container Release - Transfer")));

		bool IsSecureContainerReleasePickupLog(StmALog log, string secureContainerReleaseMessageType)
		{
			var messageType = log.Parameters.GetValueSafe(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType);
			return string.Compare(messageType, secureContainerReleaseMessageType, StringComparison.OrdinalIgnoreCase) == 0;
		}

		#endregion

		#region Electronic Bill Of Lading

		#region JK_ElectronicBillOfLadingTerms

		[ReadOnly(true)]
		[List("JK_ElectronicBillOfLadingTerms_List")]
		public override ZString JK_ElectronicBillOfLadingTerms
		{
			get { return base.JK_ElectronicBillOfLadingTerms; }
			set { base.JK_ElectronicBillOfLadingTerms = value; }
		}

		#endregion

		#region JK_ElectronicBillOfLadingType

		[ReadOnly(true)]
		[List("JK_ElectronicBillOfLadingType_List")]
		public override ZString JK_ElectronicBillOfLadingType
		{
			get { return base.JK_ElectronicBillOfLadingType; }
			set { base.JK_ElectronicBillOfLadingType = value; }
		}

		#endregion

		#region JK_Calc_BillOfLadingBillStatus

		[List("Lookups.BillOfLadingBillStatusList")]
		public ZString JK_Calc_BillOfLadingBillStatus
		{
			get
			{
				var latestBillOfLadingBillStatus = GetLatestValidBluLogWithBillOfLadingBillStatus();
				return latestBillOfLadingBillStatus.billSatatus;
			}
		}

		public ZPropertyInfo JK_Calc_BillOfLadingBillStatusInfo => GetZPropertyInfo(nameof(JK_Calc_BillOfLadingBillStatus));

		#endregion

		#region JK_Calc_BillOfLadingBillStatusDescription

		public ZString JK_Calc_BillOfLadingBillStatusDescription
		{
			get
			{
				return JK_Calc_BillOfLadingBillStatus.IsEmpty ? ZString.Empty : Lookups.BillOfLadingBillStatusList.GetDescriptionFromCode(JK_Calc_BillOfLadingBillStatus);
			}
		}

		public ZPropertyInfo JK_Calc_BillOfLadingBillStatusDescriptionInfo => GetZPropertyInfo(nameof(JK_Calc_BillOfLadingBillStatusDescription));

		#endregion

		#region JK_Calc_BillOfLadingBillDate

		public ZDateTime JK_Calc_BillOfLadingBillDate
		{
			get
			{
				return GetLatestValidBluLogWithBillOfLadingBillStatus().bluLog?.PostedLocalBranchTime ?? ZDateTime.Empty;
			}
		}

		public ZPropertyInfo JK_Calc_BillOfLadingBillDateInfo => GetZPropertyInfo(nameof(JK_Calc_BillOfLadingBillDate));

		#endregion

		(StmALog bluLog, ZString billSatatus) GetLatestValidBluLogWithBillOfLadingBillStatus()
		{
			var bluLogs = Logs.GetAllLogsByEventOrderByPostedTimeUtcDESC(AutoEvents.BillStatusUpdated);
			foreach (var bluLog in bluLogs)
			{
				if (bluLog.Parameters.TryGetValue(EventConstants.EventReferenceParameters.Codes.Type, out var messageType))
				{
					var billStatus = BillStatusUpdatedEventTypeToBillOfLadingBillStatus(messageType);
					if (!billStatus.IsEmpty)
					{
						return (bluLog, billStatus);
					}
				}
			}

			return default;
		}

		public static ZString BillStatusUpdatedEventTypeToBillOfLadingBillStatus(string billStatusUpdatedEventType)
		{
			switch (billStatusUpdatedEventType)
			{
				case BillStatusUpdatedTypes.OriginalBillPublished:
				case BillStatusUpdatedTypes.AmendmentDenied:
				case BillStatusUpdatedTypes.AmendmentBillReceived:
					return FreightConstants.BillOfLadingBillStatus.Codes.OriginalBillReceived;
				case BillStatusUpdatedTypes.OriginalBillTransferred:
					return FreightConstants.BillOfLadingBillStatus.Codes.OriginalBillTransferred;
				case BillStatusUpdatedTypes.AmendmentRequested:
				case BillStatusUpdatedTypes.AmendmentGranted:
					return FreightConstants.BillOfLadingBillStatus.Codes.OriginalBillAmendmentInProgress;
				case BillStatusUpdatedTypes.SwitchedToPaper:
					return FreightConstants.BillOfLadingBillStatus.Codes.SwitchedToPaper;
				case BillStatusUpdatedTypes.Surrendered:
					return FreightConstants.BillOfLadingBillStatus.Codes.Surrendered;
				default:
					return ZString.Empty;
			}
		}

		#endregion

		#endregion

		#region Property Overrides

		[List("JK_ReleaseType_List")]
		public override ZString JK_ReleaseType
		{
			get { return base.JK_ReleaseType; }
			set
			{
				ZString oldValue = base.JK_ReleaseType;
				base.JK_ReleaseType = value;
				if (JK_ReleaseType != oldValue || IsSettingDefaultValues)
				{
					if (!IsImportingData)
					{
						JK_NoOriginalBills = (byte)FreightDataRegistry.Instance.ReleaseTypes.Value.OriginalsNumberByType(JK_ReleaseType);
						JK_NoCopyBills = (byte)FreightDataRegistry.Instance.ReleaseTypes.Value.CopiesNumberByType(JK_ReleaseType);
					}
				}
			}
		}

		[WorkflowSetFieldReadonly]
		public override ZString JK_UniqueConsignRef
		{
			get { return base.JK_UniqueConsignRef; }
			set { base.JK_UniqueConsignRef = value; }
		}

		[List("AWBDimsCodeDescriptionPairList")]
		public override ZString JK_PrintOptionForPackagesOnAWB
		{
			get { return base.JK_PrintOptionForPackagesOnAWB; }
			set { base.JK_PrintOptionForPackagesOnAWB = value; }
		}

		[List("JK_PrintOptionForColoads_List")]
		public override ZString JK_PrintOptionForColoadsOnOtherDocs
		{
			get { return base.JK_PrintOptionForColoadsOnOtherDocs; }
			set { base.JK_PrintOptionForColoadsOnOtherDocs = value; }
		}

		[List("JK_PrintOptionForColoads_List")]
		public override ZString JK_PrintOptionForColoadsOnManifest
		{
			get { return base.JK_PrintOptionForColoadsOnManifest; }
			set { base.JK_PrintOptionForColoadsOnManifest = value; }
		}

		#region JK_SendingForwarderHandlingType

		[List(nameof(GatewayHandlingTypeList))]
		public override ZString JK_SendingForwarderHandlingType
		{
			get => base.JK_SendingForwarderHandlingType;
			set
			{
				ZString oldValue = base.JK_SendingForwarderHandlingType;
				base.JK_SendingForwarderHandlingType = value;

				if (JK_SendingForwarderHandlingType != oldValue)
				{
					if (JK_SendingForwarderHandlingType_InitValue == null)
					{
						JK_SendingForwarderHandlingType_InitValue = oldValue;
					}

					if (!IsGatewayAgentOrGatewayAgentWithTariff(oldValue) && IsGatewayAgentOrGatewayAgentWithTariff(value))
					{
						foreach (ForwardingShipment shipment in Shipments)
						{
							shipment.SynchronizeShipmentGatewayFromConsols(this, ZGuid.Empty, isDetachedConsol: false);
						}
					}

					if (IsGatewayAgentOrGatewayAgentWithTariff(oldValue) && !IsGatewayAgentOrGatewayAgentWithTariff(value))
					{
						foreach (ForwardingShipment shipment in Shipments)
						{
							shipment.SynchronizeShipmentGatewayFromConsols(this, JK_OA_SendingForwarderAddress, isDetachedConsol: true);
						}
					}
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateJK_AgentType();
					Validation.ValidateJK_OA_SendingForwarderAddress();

					if (IsGatewayServiceLevelValidationNecessary(JK_SendingForwarderHandlingType_InitValue, JK_SendingForwarderHandlingType))
					{
						Validation.ValidateJK_RS_NKGatewayServiceLevel();
					}

					if (!CCARouteValidationHelper.IsGatewayAgentAssigned(this))
					{
						Validation.ValidateJK_RCA_AllocationLine();

						foreach (var container in this.Containers.OfType<ForwardingContainer>().Where(container => !container.JC_RCA_AllocationLine.IsEmpty))
						{
							container.Validation.ValidateJC_RCA_AllocationLine();
						}
					}
				}
			}
		}

		internal string JK_SendingForwarderHandlingType_Defaulted { get; private set; }
		string JK_SendingForwarderHandlingType_InitValue;

		#endregion

		#region JK_MBLAWBChargesDisplay

		[List(nameof(ChargesApplyLookup))]
		public override ZString JK_MBLAWBChargesDisplay
		{
			get => base.JK_MBLAWBChargesDisplay;
			set => base.JK_MBLAWBChargesDisplay = value;
		}

		#endregion

		#region JK_ReceivingForwarderHandlingType

		[List(nameof(GatewayHandlingTypeList))]
		public override ZString JK_ReceivingForwarderHandlingType
		{
			get => base.JK_ReceivingForwarderHandlingType;
			set
			{
				ZString oldValue = base.JK_ReceivingForwarderHandlingType;
				base.JK_ReceivingForwarderHandlingType = value;

				if (JK_ReceivingForwarderHandlingType != oldValue)
				{
					if (JK_ReceivingForwarderHandlingType_InitValue == null)
					{
						JK_ReceivingForwarderHandlingType_InitValue = oldValue;
					}

					if (!IsGatewayAgentOrGatewayAgentWithTariff(oldValue) && IsGatewayAgentOrGatewayAgentWithTariff(value))
					{
						foreach (ForwardingShipment shipment in Shipments)
						{
							shipment.SynchronizeShipmentGatewayFromConsols(this, ZGuid.Empty, isDetachedConsol: false);
						}
					}

					if (IsGatewayAgentOrGatewayAgentWithTariff(oldValue) && !IsGatewayAgentOrGatewayAgentWithTariff(value))
					{
						foreach (ForwardingShipment shipment in Shipments)
						{
							shipment.SynchronizeShipmentGatewayFromConsols(this, JK_OA_ReceivingForwarderAddress, isDetachedConsol: true);
						}
					}
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateJK_AgentType();
					Validation.ValidateJK_OA_ReceivingForwarderAddress();

					if (IsGatewayServiceLevelValidationNecessary(JK_ReceivingForwarderHandlingType_InitValue, JK_ReceivingForwarderHandlingType))
					{
						Validation.ValidateJK_RS_NKGatewayServiceLevel();
					}

					if (!CCARouteValidationHelper.IsGatewayAgentAssigned(this))
					{
						Validation.ValidateJK_RCA_AllocationLine();

						foreach (var container in this.Containers.OfType<ForwardingContainer>().Where(container => !container.JC_RCA_AllocationLine.IsEmpty))
						{
							container.Validation.ValidateJC_RCA_AllocationLine();
						}
					}
				}
			}
		}

		internal string JK_ReceivingForwarderHandlingType_Defaulted { get; private set; }
		string JK_ReceivingForwarderHandlingType_InitValue;

		#endregion

		#region JK_RequiresTemperatureControl

		public bool JK_RequiresTemperatureControl_ReadOnly => !Env.Security.ConsolTempControl.IsAllowed;

		#endregion

		#region JK_RequiredTemperatureMaximum

		public override ZDecimal JK_RequiredTemperatureMaximum
		{
			get => base.JK_RequiredTemperatureMaximum;
			set
			{
				if (base.JK_RequiredTemperatureMaximum != value)
				{
					base.JK_RequiredTemperatureMaximum = value;

					if (JK_RequiresTemperatureControl)
					{
						if (!IsValidationSuspended)
						{
							Validation.ValidateJK_RequiresTemperatureControl();
						}
					}
					else
					{
						JK_RequiresTemperatureControl = true;
					}

					if (!IsValidationSuspended)
					{
						Validation.ValidateJK_RequiredTemperatureMinimum();
					}
				}
			}
		}

		public bool JK_RequiredTemperatureMaximum_ReadOnly => !Env.Security.ConsolTempControl.IsAllowed;

		#endregion

		#region JK_RequiredTemperatureMinimum

		public override ZDecimal JK_RequiredTemperatureMinimum
		{
			get => base.JK_RequiredTemperatureMinimum;
			set
			{
				if (base.JK_RequiredTemperatureMinimum != value)
				{
					base.JK_RequiredTemperatureMinimum = value;

					if (JK_RequiresTemperatureControl)
					{
						if (!IsValidationSuspended)
						{
							Validation.ValidateJK_RequiresTemperatureControl();
						}
					}
					else
					{
						JK_RequiresTemperatureControl = true;
					}

					if (!IsValidationSuspended)
					{
						Validation.ValidateJK_RequiredTemperatureMaximum();
					}
				}
			}
		}

		public bool JK_RequiredTemperatureMinimum_ReadOnly => !Env.Security.ConsolTempControl.IsAllowed;

		#endregion

		#region JK_RequiredTemperatureUnit

		public override ZString JK_RequiredTemperatureUnit
		{
			get => base.JK_RequiredTemperatureUnit;
			set
			{
				if (base.JK_RequiredTemperatureUnit != value)
				{
					base.JK_RequiredTemperatureUnit = value;

					if (!IsValidationSuspended)
					{
						Validation.ValidateJK_RequiredTemperatureMaximum();
						Validation.ValidateJK_RequiredTemperatureMinimum();
						Validation.ValidateJK_RequiresTemperatureControl();
					}
				}
			}
		}

		public bool JK_RequiredTemperatureUnit_ReadOnly => !Env.Security.ConsolTempControl.IsAllowed;

		#endregion

		#region JK_MaximumAllowablePackageDimensions

		[ReadOnlyMember(nameof(IsPreAllocationsReadOnly))]
		public override ZDecimal JK_MaximumAllowablePackageLength
		{
			get => base.JK_MaximumAllowablePackageLength;
			set
			{
				var roundedValue = this.GetRoundedValue(JobConsolSchema.JK_MaximumAllowablePackageLength, JK_MaximumAllowablePackageLengthInfo, value);

				if (base.JK_MaximumAllowablePackageLength != roundedValue)
				{
					base.JK_MaximumAllowablePackageLength = roundedValue;

					if (!IsValidationSuspended)
					{
						Validation.ValidateJK_MaximumAllowablePackageHeight();
						Validation.ValidateJK_MaximumAllowablePackageWidth();
						Validation.ValidateJK_MaximumAllowablePackageUnit();
					}
				}
			}
		}

		[ReadOnlyMember(nameof(IsPreAllocationsReadOnly))]
		public override ZDecimal JK_MaximumAllowablePackageWidth
		{
			get => base.JK_MaximumAllowablePackageWidth;
			set
			{
				var roundedValue = this.GetRoundedValue(JobConsolSchema.JK_MaximumAllowablePackageWidth, JK_MaximumAllowablePackageWidthInfo, value);

				if (base.JK_MaximumAllowablePackageWidth != roundedValue)
				{
					base.JK_MaximumAllowablePackageWidth = roundedValue;

					if (!IsValidationSuspended)
					{
						Validation.ValidateJK_MaximumAllowablePackageLength();
						Validation.ValidateJK_MaximumAllowablePackageHeight();
						Validation.ValidateJK_MaximumAllowablePackageUnit();
					}
				}
			}
		}

		[ReadOnlyMember(nameof(IsPreAllocationsReadOnly))]
		public override ZDecimal JK_MaximumAllowablePackageHeight
		{
			get => base.JK_MaximumAllowablePackageHeight;
			set
			{
				var roundedValue = this.GetRoundedValue(JobConsolSchema.JK_MaximumAllowablePackageHeight, JK_MaximumAllowablePackageHeightInfo, value);

				if (base.JK_MaximumAllowablePackageHeight != roundedValue)
				{
					base.JK_MaximumAllowablePackageHeight = roundedValue;

					if (!IsValidationSuspended)
					{
						Validation.ValidateJK_MaximumAllowablePackageLength();
						Validation.ValidateJK_MaximumAllowablePackageWidth();
						Validation.ValidateJK_MaximumAllowablePackageUnit();
					}
				}
			}
		}

		[ReadOnlyMember(nameof(IsPreAllocationsReadOnly))]
		public override ZString JK_MaximumAllowablePackageUnit
		{
			get => base.JK_MaximumAllowablePackageUnit;
			set
			{
				base.JK_MaximumAllowablePackageUnit = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateJK_MaximumAllowablePackageLength();
					Validation.ValidateJK_MaximumAllowablePackageWidth();
					Validation.ValidateJK_MaximumAllowablePackageHeight();
				}
			}
		}

		public override bool ShipmentContainsAllowableCargoDimensions(CommonShipment shipment, bool checkIfDimensionsAreRestricted)
		{
			if (checkIfDimensionsAreRestricted)
			{
				var checks = ForwardingConfigurationRegistry.Instance.ConsolPreAllocationCheck.Value;
				var action = checks?.Dimensions?.Action ?? ZString.Empty;
				if (action != PreAllocationCheck.Actions.Restriction)
				{
					return true;
				}
			}

			var allContainersAreValid = shipment.Containers
				.Cast<ForwardingContainer>()
				.All(container =>
				{
					var checkCanFitInConsolResult = CargoDimensionsHelpers.CheckCanFitInConsol(this, container);
					return checkCanFitInConsolResult.ItemFitsHeight && checkCanFitInConsolResult.ItemFitsWidthAndLength;
				});

			var allPacklinesAreValid = shipment.OuterPackLines
				.Cast<ForwardingPackLine>()
				.All(packLine =>
				{
					var checkCanFitInConsolResult = CargoDimensionsHelpers.CheckCanFitInConsol(this, packLine);
					return checkCanFitInConsolResult.ItemFitsHeight && checkCanFitInConsolResult.ItemFitsWidthAndLength;
				});

			return allContainersAreValid && allPacklinesAreValid;
		}

		#endregion

		#region Gateway control

		public bool IsGatewayAgentOrGatewayAgentWithTariff(ZString agentStatus)
		{
			return agentStatus == AgentStatusList.Codes.GatewayAgent
			|| agentStatus == AgentStatusList.Codes.GatewayAgentWithTariff;
		}

		#endregion

		#region JK_PackageGrouping

		[List("JK_PackageGrouping_List")]
		public override ZString JK_PackageGrouping
		{
			get => base.JK_PackageGrouping;
			set => base.JK_PackageGrouping = value;
		}

		void DefaultJK_PackageGrouping()
		{
			var packageGrouping = ZString.Empty;
			if (IsSea)
			{
				packageGrouping = SendingForwarder?.MiscServ.OM_FWAgentPackageGrouping ?? ZString.Empty;

				if (packageGrouping.IsEmpty || packageGrouping == PackageGrouping.Codes.DefaultFromCarrier)
				{
					packageGrouping = (IsCoLoad ? Creditor : ShippingLine)?.MiscServ.OM_CarrierPackageGrouping ?? ZString.Empty;
				}
			}

			JK_PackageGrouping = packageGrouping.IsEmpty ? new ZString(PackageGrouping.Codes.DoNotGroup) : packageGrouping;
		}

		#endregion

		#region HVLVConsignmentCount

		public int HVLVConsignmentCount
		{
			get
			{
				var key = $"{PK}-{nameof(HVLVConsignmentCount)}";
				return Factory.GetCachedValue(key, delegate
				{
					var shipmentAndConsignmentCount = 0;
					foreach (CommonShipment shipment in Shipments)
					{
						if (shipment.IsHighVolumeLowValue && shipment is ForwardingShipment forwardingShipment && forwardingShipment.HasHVLVDataCreated)
						{
							var consignmentCount = forwardingShipment.HVLVConsignmentHeader.ConsignmentCountWithoutLoading;
							shipmentAndConsignmentCount += consignmentCount;
						}
					}

					return shipmentAndConsignmentCount;
				}, CacheStalenessPolicy.StaleOnFactorySave);
			}
		}

		#endregion

		public override ZString JK_CRN
		{
			get { return base.JK_CRN; }
			set
			{
				if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Iceland)
				{
					SendingarnumerHelper.FormatCRN(value);
					base.JK_CRN = SendingarnumerHelper.Code.ToUpper();
				}
				else
				{
					base.JK_CRN = value;
				}
			}
		}

		protected override ZString GetCusEntryNumType()
		{
			ZString result;
			if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Iceland)
			{
				result = CusEntryNumberTypes.Iceland.CRN;
			}
			else
			{
				result = base.GetCusEntryNumType();
			}
			return result;
		}

		public override ZGuid JK_OA_CreditorAddress
		{
			get { return base.JK_OA_CreditorAddress; }
			set
			{
				ZGuid oldValue = base.JK_OA_CreditorAddress;
				base.JK_OA_CreditorAddress = value;
				if (base.JK_OA_CreditorAddress != oldValue)
				{
					DefaultCarrierBookingOffice();
					ScreeningStatusUpdater.SetShouldUpdateScreeningStatusByOrgAddress(this, JK_ScreeningStatus, Factory, JK_OA_CreditorAddress);
					UpdateShipmentsForAviationSecurity(SupplyChainSecurityOrganisationTypes.ConsolCoLoadWithOrganization);

					if (IsCoLoad)
					{
						DefaultJK_PackageGrouping();
					}
				}

				//if new creditor is equal to old creditor, we should update import/export address, because creditor could be set while user logged in to a compnay that
				//consol was not import/export and we hadn't synced import/export address
				UpdateExportCreditorDocAddressFromCreditor();
				UpdateImportCreditorDocAddressFromCreditor();
			}
		}

		public override ZGuid GetCreditorPKForConsolCost(ZGuid rateProviderOrgPK)
		{
			var creditorPK = GetConsolCreditorBasedOnExportImportCreditorAddress(rateProviderOrgPK, CarrierExportCreditorAddress, CarrierImportCreditorAddress);

			if (creditorPK.IsValid) // the new logic introduced in WI00489890 and WI00489878 to set the creditor
			{
				return creditorPK;
			}

			return base.GetCreditorPKForConsolCost(rateProviderOrgPK);
		}

		ZGuid GetConsolCreditorBasedOnExportImportCreditorAddress(ZGuid rateProviderOrgPK, JobDocAddress carrierExportCreditorAddress, JobDocAddress carrierImportCreditorAddress)
		{
			var applicableTransport = this.IsExport() || this.IsImport() || this.IsDomestic();
			var isCoload = IsCoLoad;

			if (applicableTransport)
			{
				return GetCreditorFromAddressesForOverseasTransportConsols(rateProviderOrgPK, carrierExportCreditorAddress, carrierImportCreditorAddress);
			}

			if (this.IsCrossTrade())
			{
				if (isCoload)
				{
					return GetCreditorFromAddressesForCrossTradeColoadConsol(rateProviderOrgPK);
				}

				return GetCreditorFromAddressesForCrossTradeNonColoadConsol(rateProviderOrgPK);
			}

			return ZGuid.Empty;
		}

		ZGuid GetCreditorFromExportImportCreditorAddressWithFallback(ZGuid rateProviderOrgPK, JobDocAddress carrierCreditorAddress, Transport transport, bool isExportOrDomestic)
		{
			var isCoload = IsCoLoad;

			if (carrierCreditorAddress?.OrganisationPK == rateProviderOrgPK)
			{
				return carrierCreditorAddress.OrganisationPK;
			}

			if (isCoload && CreditorPK == rateProviderOrgPK)
			{
				if (carrierCreditorAddress?.OrganisationPK.IsValid ?? false)
				{
					return carrierCreditorAddress.OrganisationPK;
				}

				return CreditorPK;
			}

			if (JK_OA_ShippingLineAddress_ZAddress?.OrgHeader?.PK == rateProviderOrgPK)
			{
				if (isCoload)
				{
					var creditorPK = GetCreditorFromOrgRelatedPartiesForImportExportConsols(JK_OA_ShippingLineAddress_ZAddress.OrgHeader as OrgHeader, transport, isExportOrDomestic);

					if (creditorPK.IsValid)
					{
						return creditorPK;
					}
				}
				else if (!carrierCreditorAddress?.OrganisationPK.IsEmpty ?? false)
				{
					return carrierCreditorAddress.OrganisationPK;
				}

				return JK_OA_ShippingLineAddress_ZAddress.OrgHeader.PK;
			}

			var provider = GetRateProviders().FirstOrDefault(p => p.OrgHeader?.PK == rateProviderOrgPK);

			if (provider != null)
			{
				var creditorPK = GetCreditorFromOrgRelatedPartiesForImportExportConsols(provider.OrgHeader as OrgHeader, transport, isExportOrDomestic);

				if (creditorPK.IsValid)
				{
					return creditorPK;
				}

				return rateProviderOrgPK;
			}

			return ZGuid.Empty;
		}

		ZGuid GetCreditorWhenRoutingCarrierOrCreditorIsRateProvider(ZGuid rateProviderOrgPK, Transport transport)
		{
			if (transport == null)
			{
				return ZGuid.Empty;
			}

			if (transport.JW_OA_CreditorAddress_ZAddress?.OrgHeader?.PK == rateProviderOrgPK)
			{
				return transport.JW_OA_CreditorAddress_ZAddress.OrgHeader.PK;
			}

			if (transport.JW_OA_CarrierAddress_ZAddress?.OrgHeader?.PK == rateProviderOrgPK)
			{
				if (!transport.JW_OA_CreditorAddress_ZAddress?.OrgHeader?.PK.IsEmpty ?? false)
				{
					return transport.JW_OA_CreditorAddress_ZAddress.OrgHeader.PK;
				}

				return transport.JW_OA_CarrierAddress_ZAddress.OrgHeader.PK;
			}

			return ZGuid.Empty;
		}

		ZGuid GetCreditorFromOrgRelatedPartiesForImportExportConsols(OrgHeader addressOrgHeader, Transport transport, bool isExportOrDomestic)
		{
			var orgRelatedPartyFilter = new DefaultCreditorHelper.OrgRelatedPartyFilter()
			{
				TransportMode = JK_TransportMode,
				ContainerMode = JK_ConsolMode,
				CreditorType = isExportOrDomestic ? DefaultCreditorHelper.CreditorType.ExportConsol : DefaultCreditorHelper.CreditorType.ImportConsol,
				UNLOCO = (isExportOrDomestic ? transport?.JW_RL_NKLoadPort : transport?.JW_RL_NKDiscPort) ?? ZString.Empty
			};

			var creditor = DefaultCreditorHelper.GetCreditorOrgHeaderFromOrgRelatedParties(addressOrgHeader, orgRelatedPartyFilter, Factory);

			if (creditor != null)
			{
				return creditor.PK;
			}

			return ZGuid.Empty;
		}

		ZGuid GetCreditorFromAddressesForCrossTradeNonColoadConsol(ZGuid rateProviderOrgPK)
		{
			var multiRouteAutoCostingRegistry = RatingDataRegistry.Instance.MultiModalRatingCost.Value;
			var transport = Transports.MostInterestingTransport;

			if (!rateProviderOrgPK.IsValid)
			{
				if (CarrierImportCreditorAddress?.OrganisationPK.IsValid ?? false)
				{
					return CarrierImportCreditorAddress.OrganisationPK;
				}
				else if (JK_OA_ShippingLineAddress_ZAddress?.OrgHeader?.PK.IsValid ?? false)
				{
					return JK_OA_ShippingLineAddress_ZAddress.OrgHeader.PK;
				}
				return ZGuid.Empty;
			}

			if (CarrierImportCreditorAddress?.OrganisationPK == rateProviderOrgPK)
			{
				if (CarrierImportCreditorAddress.OrganisationPK.IsValid)
				{
					return CarrierImportCreditorAddress.OrganisationPK;
				}
				return ZGuid.Empty;
			}

			if (!multiRouteAutoCostingRegistry && JK_OA_ShippingLineAddress_ZAddress?.OrgHeader?.PK == rateProviderOrgPK)
			{
				if (CarrierImportCreditorAddress.OrganisationPK.IsValid)
				{
					return CarrierImportCreditorAddress.OrganisationPK;
				}
				else if (JK_OA_ShippingLineAddress.IsValid)
				{
					return JK_OA_ShippingLineAddress_ZAddress.OrgHeader.PK;
				}
				return ZGuid.Empty;
			}

			var routingCreditor = GetCreditorWhenRoutingCarrierOrCreditorIsRateProvider(rateProviderOrgPK, transport);
			if (routingCreditor.IsValid)
			{
				return routingCreditor;
			}

			if (rateProviderOrgPK.IsValid && GetRateProviders().Any(p => p.OrgHeader?.PK == rateProviderOrgPK))
			{
				return rateProviderOrgPK;
			}

			return ZGuid.Empty;
		}

		ZGuid GetCreditorFromAddressesForCrossTradeColoadConsol(ZGuid rateProviderOrgPK)
		{
			var multiRouteAutoCostingRegistry = RatingDataRegistry.Instance.MultiModalRatingCost.Value;
			var transport = Transports.MostInterestingTransport;

			if (!rateProviderOrgPK.IsValid)
			{
				if (CarrierImportCreditorAddress?.OrganisationPK.IsValid ?? false)
				{
					return CarrierImportCreditorAddress.OrganisationPK;
				}
				else if (CreditorPK.IsValid)
				{
					return CreditorPK;
				}
				return ZGuid.Empty;
			}

			if (CarrierImportCreditorAddress?.OrganisationPK == rateProviderOrgPK)
			{
				if (CarrierImportCreditorAddress.OrganisationPK.IsValid)
				{
					return CarrierImportCreditorAddress.OrganisationPK;
				}
				return ZGuid.Empty;
			}

			if (CreditorPK == rateProviderOrgPK)
			{
				if (CarrierImportCreditorAddress.OrganisationPK.IsValid)
				{
					return CarrierImportCreditorAddress.OrganisationPK;
				}
				else if (CreditorPK.IsValid)
				{
					return CreditorPK;
				}
				return ZGuid.Empty;
			}

			if (!multiRouteAutoCostingRegistry && JK_OA_ShippingLineAddress_ZAddress?.OrgHeader?.PK == rateProviderOrgPK)
			{
				var creditorPKFromRelatedParties = GetCreditorFromOrgRelatedPartiesForCrossTradeConsol(JK_OA_ShippingLineAddress_ZAddress.OrgHeader as OrgHeader, transport);

				if (!creditorPKFromRelatedParties.IsEmpty)
				{
					return creditorPKFromRelatedParties;
				}
				return JK_OA_ShippingLineAddress_ZAddress.OrgHeader.PK;
			}

			if (multiRouteAutoCostingRegistry)
			{
				var routingCreditor = GetCreditorWhenRoutingCarrierOrCreditorIsRateProvider(rateProviderOrgPK, transport);
				if (routingCreditor.IsValid)
				{
					return routingCreditor;
				}
			}

			if (rateProviderOrgPK.IsValid && GetRateProviders().Any(p => p.OrgHeader?.PK == rateProviderOrgPK))
			{
				return rateProviderOrgPK;
			}

			return ZGuid.Empty;
		}

		ZGuid GetCreditorFromOrgRelatedPartiesForCrossTradeConsol(OrgHeader addressOrgHeader, Transport transport)
		{
			var orgRelatedPartyFilter = new DefaultCreditorHelper.OrgRelatedPartyFilter()
			{
				TransportMode = JK_TransportMode,
				ContainerMode = JK_ConsolMode,
				CreditorType = DefaultCreditorHelper.CreditorType.CrossTradeConsol,
				UNLOCO = transport?.JW_RL_NKDiscPort ?? ZString.Empty
			};

			var creditor = DefaultCreditorHelper.GetCreditorOrgHeaderFromOrgRelatedParties(addressOrgHeader, orgRelatedPartyFilter, Factory);

			if (creditor != null)
			{
				return creditor.PK;
			}

			return ZGuid.Empty;
		}

		ZGuid GetCreditorFromAddressesForOverseasTransportConsols(ZGuid rateProviderOrgPK, JobDocAddress carrierExportCreditorAddress, JobDocAddress carrierImportCreditorAddress)
		{
			var isExportOrDomestic = this.IsExport() || this.IsDomestic();
			var isCoload = IsCoLoad;
			var transport = Transports.MostInterestingTransport;
			var isChargeFromManualEnteredOrStandardCosting = rateProviderOrgPK.IsEmpty;
			var carrierCreditorAddress = isExportOrDomestic ? carrierExportCreditorAddress : carrierImportCreditorAddress;

			if (isChargeFromManualEnteredOrStandardCosting)
			{
				if (carrierCreditorAddress?.OrganisationPK.IsValid ?? false)
				{
					return carrierCreditorAddress.OrganisationPK;
				}

				if (isCoload)
				{
					if (CreditorPK.IsValid)
					{
						return CreditorPK;
					}
				}
				else if (JK_OA_ShippingLineAddress_ZAddress?.OrgHeader?.PK.IsValid ?? false)
				{
					return JK_OA_ShippingLineAddress_ZAddress.OrgHeader.PK;
				}

				return ZGuid.Empty;
			}

			var creditor = GetCreditorFromExportImportCreditorAddressWithFallback(rateProviderOrgPK, carrierCreditorAddress, transport, isExportOrDomestic);

			if (creditor.IsValid)
			{
				return creditor;
			}

			var routingCreditor = GetCreditorWhenRoutingCarrierOrCreditorIsRateProvider(rateProviderOrgPK, transport);

			if (routingCreditor.IsValid)
			{
				return routingCreditor;
			}

			return ZGuid.Empty;
		}

		IEnumerable<ZAddress> GetRateProviders()
		{
			yield return SendingForwarderWithContact;
			yield return ReceivingForwarderWithContact;
			yield return JK_OA_DepartureCTOAddress_ZAddress;
			yield return JK_OA_PackDepotAddress_ZAddress;
			yield return JK_OA_DeparturePackCFSTransportAddress_ZAddress;
			yield return JK_OA_ArrivalCTOAddress_ZAddress;
			yield return JK_OA_UnpackDepotAddress_ZAddress;
			yield return JK_OA_ArrivalUnpackCFSTransportAddress_ZAddress;
		}

		void UpdateExportCreditorDocAddressFromCreditor()
		{
			if (this.IsExport() || this.IsDomestic())
			{
				if (!IsChangingExportCreditorAddress && (JK_OA_CreditorAddress_ZAddress.OrgPK.IsEmpty || JK_OA_CreditorAddress.IsValid))
				{
					UpdateCreditorDocAddress(CarrierExportCreditorAddress, JK_OA_CreditorAddress);
				}
			}
		}

		void UpdateImportCreditorDocAddressFromCreditor()
		{
			if (this.IsImport() || this.IsCrossTrade())
			{
				if (!IsChangingImportCreditorAddress && (JK_OA_CreditorAddress_ZAddress.OrgPK.IsEmpty || JK_OA_CreditorAddress.IsValid))
				{
					UpdateCreditorDocAddress(CarrierImportCreditorAddress, JK_OA_CreditorAddress);
				}
			}
		}

		void UpdateCreditorDocAddress(JobDocAddress docAddress, ZGuid orgAddressId)
		{
			var shouldValidate = docAddress.OrganisationPKInfo.HasErrors();
			var creditorAddressId = orgAddressId;

			if (IsCoLoad)
			{
				creditorAddressId = GetDefaultCreditorFromCarrier(orgAddressId);
			}

			docAddress.E2_OA_Address = creditorAddressId;

			if (creditorAddressId.IsEmpty)
			{
				docAddress.OrganisationPK = ZGuid.Empty;
				shouldValidate = true;
			}

			if (shouldValidate)
			{
				docAddress.Validation.ValidateAll();
			}
		}

		#region Recalculate Related Parties

		public void RecalculateExportParties()
		{
			if (this.IsExport())
			{
				RedefaultDepartureCFS();
				RedefaultDepartureTransportPort();

				if (IsCoLoad)
				{
					RedefaultCoLoadWith();
				}
			}
		}

		void RedefaultDepartureTransportPort()
		{
			if (JK_OA_DeparturePackCFSTransportAddress_ZAddress != null && !JK_OA_DeparturePackCFSTransportAddress_ZAddress.OrgPK.IsEmpty)
			{
				var args = RaiseAttemptEvent(ResString.GetMultilingualString("d3fd5660-fd6c-4291-ad71-d36d81c7a5b9", "Departure Port Transport"));
				if (!args.Cancel)
				{
					SetDepartureTransport();
				}
			}
			else
			{
				SetDepartureTransport();
			}
		}

		void RedefaultDepartureCFS()
		{
			if (JK_OA_PackDepotAddress_ZAddress != null && !JK_OA_PackDepotAddress_ZAddress.OrgPK.IsEmpty)
			{
				var args = RaiseAttemptEvent(ResString.GetMultilingualString("6036b1dc-6cab-4859-b2b7-6cfdd2faafb3", "Departure CFS Address"));
				if (!args.Cancel)
				{
					SetDepartureDepot();
				}
			}
			else
			{
				SetDepartureDepot();
			}
		}

		void RedefaultCoLoadWith()
		{
			if (JK_OA_CreditorAddress_ZAddress != null && !JK_OA_CreditorAddress_ZAddress.OrgPK.IsEmpty)
			{
				var args = RaiseAttemptEvent(ResString.GetMultilingualString("9293d687-a10d-4df2-b371-cbddcf962abc", "Co-Load With"));
				if (!args.Cancel)
				{
					SetCoLoadWith();
				}
			}
			else
			{
				SetCoLoadWith();
			}
		}

		public void RecalculateImportParties()
		{
			if (this.IsImport())
			{
				RedefaultArrivalCFS();
				RedefaultArrivalTransportPort();
			}
		}

		void RedefaultArrivalTransportPort()
		{
			if (JK_OA_ArrivalUnpackCFSTransportAddress_ZAddress != null && !JK_OA_ArrivalUnpackCFSTransportAddress_ZAddress.OrgPK.IsEmpty)
			{
				var args = RaiseAttemptEvent(ResString.GetMultilingualString("ecd7eb75-47b3-41d7-8d60-d2114498044b", "Arrival Port Transport"));
				if (!args.Cancel)
				{
					SetArrivalLocalTransport();
				}
			}
			else
			{
				SetArrivalLocalTransport();
			}
		}

		void RedefaultArrivalCFS()
		{
			if (JK_OA_UnpackDepotAddress_ZAddress != null && !JK_OA_UnpackDepotAddress_ZAddress.OrgPK.IsEmpty)
			{
				var args = RaiseAttemptEvent(ResString.GetMultilingualString("e6a92364-b3ae-4a1e-a00d-6df69bcab1b6", "Arrival CFS Address"));
				if (!args.Cancel)
				{
					SetArrivalDepot();
				}
			}
			else
			{
				SetArrivalDepot();
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

		internal void RedefaultCreditor()
		{
			if (IsCoLoad)
			{
				UpdateExportCreditorDocAddressFromCreditor();
				UpdateImportCreditorDocAddressFromCreditor();
			}
			else
			{
				SetDefaultCreditorFromCarrier(JK_OA_ShippingLineAddress);
			}
		}

		#endregion

		#region JK_OA_SendingForwarderAddress

		public override ZGuid JK_OA_SendingForwarderAddress
		{
			get { return base.JK_OA_SendingForwarderAddress; }
			set
			{
				ZGuid oldValue = base.JK_OA_SendingForwarderAddress;
				base.JK_OA_SendingForwarderAddress = value;
				if (base.JK_OA_SendingForwarderAddress != oldValue)
				{
					if (!JK_OA_SendingForwarderAddress_InitValue.HasValue)
					{
						JK_OA_SendingForwarderAddress_InitValue = oldValue;
					}

					SetDefaultShipper();
					ScreeningStatusUpdater.SetShouldUpdateScreeningStatusByOrgAddress(this, JK_ScreeningStatus, Factory, JK_OA_SendingForwarderAddress);
					UpdateShipmentsForAviationSecurity(SupplyChainSecurityOrganisationTypes.ConsolSendingAgent);

					if (IsGatewayAgentOrGatewayAgentWithTariff(JK_SendingForwarderHandlingType) && !oldValue.IsEmpty && oldValue != ZGuid.Empty)
					{
						foreach (ForwardingShipment shipment in Shipments)
						{
							shipment.SynchronizeShipmentGatewayFromConsols(this, oldValue, isDetachedConsol: true);
						}
					}

					if (!IsInDatabase)
					{
						SetSendingForwarderHandlingTypeDefaultForAnyCompany();
					}
					else
					{
						SetSendingForwarderHandlingTypeDefault();
					}

					DefaultJK_PackageGrouping();

					if (!IsValidationSuspended && IsGatewayServiceLevelValidationNecessary(JK_OA_SendingForwarderAddress_InitValue, JK_OA_SendingForwarderAddress))
					{
						Validation.ValidateJK_RS_NKGatewayServiceLevel();
					}

					if (AllocationLine is not null && AllocationLine.AgentPivots.Count > 0)
					{
						Validation.ValidateJK_RCA_AllocationLine();

						foreach (var container in this.Containers.OfType<ForwardingContainer>().Where(container => !container.JC_RCA_AllocationLine.IsEmpty))
						{
							container.Validation.ValidateJC_RCA_AllocationLine();
						}
					}
				}

				SetShipmentHandledOnBehalfOfForwarder();
			}
		}

		IGateway Gateway => this;
		ZGuid? JK_OA_SendingForwarderAddress_InitValue;

		void SetSendingForwarderHandlingTypeDefault()
		{
			if ((IsAgent || IsCoLoad) && GlbDepartment.CurrentDepartment.IsGatewayDepartment)
			{
				var defaultHandlingType = ((ForwardingConsolGatewayBillingSupporter)Gateway.GatewayBillingSupporter).GetDefaultSendingForwarderAddressGatewayType();

				JK_SendingForwarderHandlingType = defaultHandlingType;
				JK_SendingForwarderHandlingType_Defaulted = defaultHandlingType;
			}
		}

		void SetSendingForwarderHandlingTypeDefaultForAnyCompany()
		{
			if ((IsAgent || IsCoLoad)
				&& GlbDepartment.CurrentDepartment.IsGatewayDepartment
				&& Gateway.GatewayBillingSupporter is ForwardingConsolGatewayBillingSupporter billingSupporter)
			{
				var handlingType = billingSupporter
					.AllGlbCompanies
					.Select(company => billingSupporter.GetDefaultSendingForwarderAddressGatewayType(company))
					.FirstOrDefault(gatewayType => !gatewayType.IsEmpty);

				JK_SendingForwarderHandlingType = handlingType;
				JK_SendingForwarderHandlingType_Defaulted = handlingType;
			}
		}

		#endregion

		#region JK_OA_ShippingLineAddress

		public override ZGuid JK_OA_ShippingLineAddress
		{
			get { return base.JK_OA_ShippingLineAddress; }
			set
			{
				ZGuid oldValue = base.JK_OA_ShippingLineAddress;
				base.JK_OA_ShippingLineAddress = value;
				if (base.JK_OA_ShippingLineAddress != oldValue)
				{
					DefaultCarrierBookingOffice();
					SetDefaultPlaceOfIssue();
					DefaultCarrierBookingAgentFromCarrier();
					DefaultSpecialHandlingItems();
					ScreeningStatusUpdater.SetShouldUpdateScreeningStatusByOrgAddress(this, JK_ScreeningStatus, Factory, JK_OA_ShippingLineAddress);
					UpdateShipmentsForAviationSecurity(SupplyChainSecurityOrganisationTypes.ConsolAirline);

					if (!IsCoLoad)
					{
						DefaultJK_PackageGrouping();
					}
				}
			}
		}

		void SetDefaultPlaceOfIssue()
		{
			JK_RL_NKMasterBillIssuePlace = (IsSea && ShippingLineAddress != null)
				? ShippingLineAddress.OA_RL_NKRelatedPortCode
				: ZString.Empty;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1045")]
		public bool IsAllowedToChangeShippingLine(ref ZString message)
		{
			var warningBuilder = new ZStringBuilder();
			var containsRoutingLegs = Transports.Any();
			var containsContainerInfo = !JK_Calc_ContainerCount.IsEmpty;
			var containsConsolCosting = HasConsolCosts(GlbCompany.CurrentCompany);

			if (containsRoutingLegs)
			{
				warningBuilder.AppendLine(Res.GetString("2b505460-6667-df99-4dfb-7f2f8b767f11", "Routing Legs"));
			}
			if (containsContainerInfo)
			{
				warningBuilder.AppendLine(Res.GetString("214ca8ab-1d1f-0d9d-4047-9afb39497324", "Container Count and Container Numbers"));
			}
			if (containsConsolCosting)
			{
				warningBuilder.AppendLine(Res.GetString("bbc3dc21-3977-8cad-45d5-49aa7d7c78f6", "Consol Costing"));
			}

			if (!warningBuilder.IsEmpty)
			{
				warningBuilder.Prepend(Res.GetString("6ca80825-6b3b-94af-484f-c916e656c46e", "Please verify and manually change the following information: "));
				message = warningBuilder.ToString();
			}

			var bookingRequestedLog = (NoResString)"Booking Requested"; // log's reference

			var maaEvent = Logs.MostRecentLogByPostedTime(Events.MessageAccepted, log => log.Parameters.ContainsKey(EventConstants.EventReferenceParameters.Codes.MessageType)
				&& log.Parameters[EventConstants.EventReferenceParameters.Codes.MessageType] == bookingRequestedLog);

			var mwrEvent = Logs.MostRecentLogByPostedTime(Events.MessageWithdrawCancelRequest, log => log.Parameters.ContainsKey(EventConstants.EventReferenceParameters.Codes.MessageType)
				&& log.Parameters[EventConstants.EventReferenceParameters.Codes.MessageType] == bookingRequestedLog);

			var noEvents = maaEvent == null;
			var eventsWithdrawn = maaEvent != null && mwrEvent != null && maaEvent.SL_PostedTimeUtc < mwrEvent.SL_PostedTimeUtc;

			return noEvents || eventsWithdrawn;
		}

		public void SetShippingLine(ZGuid shippingLineAddressPK, ZString reason)
		{
			var oldCarrier = ShippingLineAddress?.OA_Code ?? ZString.Empty;

			CarrierBookingAgentDocumentaryAddress.OrganisationPK = ZGuid.Empty;
			JK_OA_ArrivalCTOAddress = ZGuid.Empty;
			JK_OA_DepartureCTOAddress = ZGuid.Empty;
			JK_OA_ContainerYardEmptyPickupAddress = ZGuid.Empty;
			JK_OA_ContainerYardEmptyReturnAddress = ZGuid.Empty;
			JK_RL_NKMasterBillIssuePlace = ZString.Empty;

			JK_OA_ShippingLineAddress = shippingLineAddressPK;

			var newCarrier = ShippingLineAddress?.OA_Code ?? ZString.Empty;
			var parameters = new[]
			{
				new KeyValuePair<string, string>(EventConstants.EventReferenceParameters.Codes.Type, (NoResString)"Carrier Changed"), // log message
				new KeyValuePair<string, string>(EventConstants.EventReferenceParameters.Codes.Old, oldCarrier),
				new KeyValuePair<string, string>(EventConstants.EventReferenceParameters.Codes.New, newCarrier),
				new KeyValuePair<string, string>(EventConstants.EventReferenceParameters.Codes.Reason, reason)
			};
			Logs.AddNew(Events.StatusUpdated, parameters);
		}

		#endregion

		public override ZGuid JK_OA_DepartureCTOAddress
		{
			get { return base.JK_OA_DepartureCTOAddress; }
			set
			{
				ZGuid oldValue = base.JK_OA_DepartureCTOAddress;
				base.JK_OA_DepartureCTOAddress = value;
				if (base.JK_OA_DepartureCTOAddress != oldValue)
				{
					ScreeningStatusUpdater.SetShouldUpdateScreeningStatusByOrgAddress(this, JK_ScreeningStatus, Factory, JK_OA_DepartureCTOAddress);
				}
			}
		}

		public override ZGuid JK_OA_PackDepotAddress
		{
			get { return base.JK_OA_PackDepotAddress; }
			set
			{
				ZGuid oldValue = base.JK_OA_PackDepotAddress;
				base.JK_OA_PackDepotAddress = value;
				if (base.JK_OA_PackDepotAddress != oldValue)
				{
					Shipments.Cast<ForwardingShipment>().ForEach(shipment => shipment.UpdateCO2eStatusToNotCurrent(new CO2eStatusChangedReason(JK_OA_PackDepotAddressInfo), IsCopying));
					UpdateContainerCO2eStatusWhenJK_OA_PackDepotAddressChanged();
					ScreeningStatusUpdater.SetShouldUpdateScreeningStatusByOrgAddress(this, JK_ScreeningStatus, Factory, JK_OA_PackDepotAddress);
					UpdateShipmentsForAviationSecurity(SupplyChainSecurityOrganisationTypes.ConsolCFS);
				}
			}
		}

		public override ZGuid JK_OA_ContainerYardEmptyPickupAddress
		{
			get { return base.JK_OA_ContainerYardEmptyPickupAddress; }
			set
			{
				var oldValue = base.JK_OA_ContainerYardEmptyPickupAddress;
				(CommonContainer container, ZGuid oldCYValue)[] containersWithOldCYValue = Containers.Select(container => (container, container.JC_OA_DepartureContainerYardAddress)).ToArray();

				base.JK_OA_ContainerYardEmptyPickupAddress = value;
				if (base.JK_OA_ContainerYardEmptyPickupAddress != oldValue)
				{
					ScreeningStatusUpdater.SetShouldUpdateScreeningStatusByOrgAddress(this, JK_ScreeningStatus, Factory, JK_OA_ContainerYardEmptyPickupAddress);
					foreach (var (container, oldCYValue) in containersWithOldCYValue)
					{
						if (oldCYValue != container.JC_OA_DepartureContainerYardAddress)
						{
							container.JC_OA_DepartureContainerYardAddressInfo.RefreshBinding();
						}
					}
				}
			}
		}

		public override ZGuid JK_OA_ArrivalCTOAddress
		{
			get { return base.JK_OA_ArrivalCTOAddress; }
			set
			{
				ZGuid oldValue = base.JK_OA_ArrivalCTOAddress;
				base.JK_OA_ArrivalCTOAddress = value;
				if (base.JK_OA_ArrivalCTOAddress != oldValue)
				{
					ScreeningStatusUpdater.SetShouldUpdateScreeningStatusByOrgAddress(this, JK_ScreeningStatus, Factory, JK_OA_ArrivalCTOAddress);
				}
			}
		}

		public override ZGuid JK_OA_UnpackDepotAddress
		{
			get { return base.JK_OA_UnpackDepotAddress; }
			set
			{
				ZGuid oldValue = base.JK_OA_UnpackDepotAddress;
				base.JK_OA_UnpackDepotAddress = value;
				if (base.JK_OA_UnpackDepotAddress != oldValue)
				{
					Shipments.Cast<ForwardingShipment>().ForEach(shipment => shipment.UpdateCO2eStatusToNotCurrent(new CO2eStatusChangedReason(JK_OA_UnpackDepotAddressInfo), IsCopying));
					UpdateContainersCO2eStatusOnJK_OA_UnpackDepotAddressChanged();
					ScreeningStatusUpdater.SetShouldUpdateScreeningStatusByOrgAddress(this, JK_ScreeningStatus, Factory, JK_OA_UnpackDepotAddress);
				}
			}
		}

		public override ZGuid JK_OA_ContainerYardEmptyReturnAddress
		{
			get { return base.JK_OA_ContainerYardEmptyReturnAddress; }
			set
			{
				var oldValue = base.JK_OA_ContainerYardEmptyReturnAddress;
				(CommonContainer container, ZGuid oldCYValue)[] containersWithOldCYValue = Containers.Select(container => (container, container.JC_OA_ArrivalContainerYardAddress)).ToArray();

				base.JK_OA_ContainerYardEmptyReturnAddress = value;
				if (base.JK_OA_ContainerYardEmptyReturnAddress != oldValue)
				{
					ScreeningStatusUpdater.SetShouldUpdateScreeningStatusByOrgAddress(this, JK_ScreeningStatus, Factory, JK_OA_ContainerYardEmptyReturnAddress);
					foreach (var (container, oldCYValue) in containersWithOldCYValue)
					{
						if (oldCYValue != container.JC_OA_ArrivalContainerYardAddress)
						{
							container.JC_OA_ArrivalContainerYardAddressInfo.RefreshBinding();
						}
					}
				}
			}
		}

		public override ZGuid JK_OA_DeparturePackCFSTransportAddress
		{
			get { return base.JK_OA_DeparturePackCFSTransportAddress; }
			set
			{
				ZGuid oldValue = base.JK_OA_DeparturePackCFSTransportAddress;
				base.JK_OA_DeparturePackCFSTransportAddress = value;
				if (base.JK_OA_DeparturePackCFSTransportAddress != oldValue)
				{
					UpdateShipmentsForAviationSecurity(SupplyChainSecurityOrganisationTypes.ConsolTransportCompany);
				}
			}
		}

		#region SendingarnumerHelper

		public ConsolSendingarnumerHelper SendingarnumerHelper
		{
			get { return sendingarnumerHelper ?? (sendingarnumerHelper = new ConsolSendingarnumerHelper(this)); }
		}
		ConsolSendingarnumerHelper sendingarnumerHelper;

		#endregion

		#region Master Bill Number
		public override ZString JK_MasterBillNum
		{
			get { return base.JK_MasterBillNum; }
			set
			{
				if (JK_MasterBillNum != value)
				{
					base.JK_MasterBillNum = value;
					Validation.ValidateJK_RCA_AllocationLine();

					MAWBAllocation.LoadMawbFromParentMawbDetails();

					if (AirCargoSynchroniser != null)
					{
						AirCargoSynchroniser.MAWBNumber = value;
					}
					DefaultSpecialHandlingItems();
					ConsolMAWBNumberConcurrencyCheck.Register(Factory, base.JK_MasterBillNum, PK, JK_TransportMode, IsCoLoad, JK_SystemCreateTimeUtc, IsImportingData);
				}

				MAWBLabelDescInfo.RefreshBinding();
			}
		}

		public delegate ZDialogResult ShowConfirmMessageOnGUI(string message, string caption = "Warning", ZMessageBoxButtons buttons = ZMessageBoxButtons.OKCancel, ZMessageBoxIcon icon = ZMessageBoxIcon.Warning);
		public ShowConfirmMessageOnGUI OnShowConfirmMessageOnGUI;
		#endregion

		#region Carrier Booking Reference
		public override ZString JK_BookingReference
		{
			get { return base.JK_BookingReference; }
			set
			{
				if (JK_BookingReference != value)
				{
					base.JK_BookingReference = value;
					Validation.ValidateJK_RCA_AllocationLine();
				}
			}
		}
		#endregion

		#region JK_TransportMode

		[List("JK_TransportMode_List")]
		public override ZString JK_TransportMode
		{
			get { return base.JK_TransportMode; }
			set
			{
				if (JK_TransportMode != value)
				{
					if (!JK_IsNeutralMaster || value == Constants.TransportModes.Air || !MAWBAllocation.IsMAWBPrinted || AllowDeallocatePrintedNeutralMAWB())
					{
						base.JK_TransportMode = value;

						MasterBillMAWBInfo.RefreshBinding();
						MasterBillAirlinePrefixInfo.RefreshBinding();

						SetDefaultPlaceOfIssue();
						UpdateVerificationUnits();
					}

					if (AirCargoSynchroniser != null)
					{
						AirCargoSynchroniser.IsAir = IsAir;
					}

					if (!IsAir && JK_IsNeutralMaster)
					{
						JK_IsNeutralMaster = false;
						JK_IsNeutralMasterInfo.RefreshBinding();
					}

					JK_MBLAWBChargesDisplay = IsAir
						? GetDefaultChargesApply()
						: ZString.Empty;

					if (IsAir)
					{
						DefaultSpecialHandlingItems();
						UpdateTransportsDefaultStatusForAirMode();
					}

					if (!IsValidationSuspended)
					{
						Validation.ValidateJK_RL_NKLoadPort();
						Validation.ValidateJK_SendingForwarderHandlingType();
						Validation.ValidateJK_ReceivingForwarderHandlingType();
					}

					DefaultCarrierBookingOffice();
					this.UpdateCO2eStatusToNotCurrent(new CO2eStatusChangedReason(JK_TransportModeInfo), IsCopying);
					DefaultJK_PackageGrouping();
					ConsolMAWBNumberConcurrencyCheck.Register(Factory, JK_MasterBillNum, PK, base.JK_TransportMode, IsCoLoad, JK_SystemCreateTimeUtc, IsImportingData);
				}
			}
		}

		void UpdateTransportsDefaultStatusForAirMode()
		{
			foreach (Transport transport in Transports)
			{
				if (!transport.IsInDatabase && transport.IsAir)
				{
					transport.JW_Status = Constants.TransportStatus.Planned;
				}
			}
		}

		#endregion

		public override ZString JK_AgentType
		{
			get { return base.JK_AgentType; }
			set
			{
				if (JK_AgentType != value)
				{
					if (value == Constants.AgentType.AWBMaster && Shipments.Any())
					{
						if (ShowMessageOnGUI != null)
						{
							string caption = ResString.GetMultilingualString("e045e153-9a6e-4ada-93ea-ddb1c0a711ba", "Could not change to {0}", Constants.AgentTypeDescriptions.AWBMaster);
							string message = ResString.GetMultilingualString("2aa2d371-c98d-433f-b54f-fa511b996958", "All shipments should be detached first.");
							ShowMessageOnGUI(this, new ShowMessageOnGUIEventArgs(caption, message));
						}

						return;
					}
					else if (JK_AgentType == Constants.AgentType.AWBMaster && ColoadConsols.Any())
					{
						if (ShowMessageOnGUI != null)
						{
							string caption = ResString.GetMultilingualString("6791ce2a-e244-4e89-9f93-7fedb7b767e7", "Could not change from {0}", Constants.AgentTypeDescriptions.AWBMaster);
							string message = ResString.GetMultilingualString("0cb553c5-1acf-4dd2-b774-03144fa2c3a5", "All coload consols should be detached first.");
							ShowMessageOnGUI(this, new ShowMessageOnGUIEventArgs(caption, message));
						}

						return;
					}

					var firstLinkedShipment = Shipments.Cast<ForwardingShipment>().FirstOrDefault();
					if (value == Constants.AgentType.Direct
								&& !IsInDatabase
								&& ShipmentCount == 1
								&& firstLinkedShipment != null
								&& !firstLinkedShipment.IsInDatabase
								&& firstLinkedShipment.IsPendingAllocationSetBySystem)
					{
						firstLinkedShipment.JS_HouseBill = ZString.Empty;
						firstLinkedShipment.IsPendingAllocationSetBySystem = false;
					}

					if (!JK_IsNeutralMaster || value == Constants.AgentType.Direct || value == Constants.AgentType.Agent || !MAWBAllocation.IsMAWBPrinted || AllowDeallocatePrintedNeutralMAWB())
					{
						if (value == Constants.AgentType.AWBMaster || JK_AgentType == Constants.AgentType.AWBMaster)
						{
							ResetTotalsAndTopLevels();
						}

						var isChangedFromOrToCoLoad = false;
						if ((IsCoLoad && value != Constants.AgentType.CoLoad)
								|| (!IsCoLoad && Constants.AgentType.CoLoad == value))
						{
							JK_OA_CreditorAddress_ZAddress.SetOrgWithoutSettingDefaultAddress(ZGuid.Empty);
							JK_OA_CreditorAddress = ZGuid.Empty;
							JK_CoLoadBookingReference = ZString.Empty;
							JK_CoLoadMasterBill = ZString.Empty;

							isChangedFromOrToCoLoad = true;
						}

						base.JK_AgentType = value;
						if (isChangedFromOrToCoLoad)
						{
							DefaultCarrierBookingOffice();
							DefaultJK_PackageGrouping();
						}
					}

					if (IsDirect)
					{
						DirectShipment?.SetDefaultSelfFiler();
					}

					if (!IsValidForNeutralMaster && JK_IsNeutralMaster)
					{
						JK_IsNeutralMaster = false;
						JK_IsNeutralMasterInfo.RefreshBinding();
					}

					if (!IsValidationSuspended)
					{
						Validation.ValidateJK_SendingForwarderHandlingType();
						Validation.ValidateJK_ReceivingForwarderHandlingType();
					}
					ConsolMAWBNumberConcurrencyCheck.Register(Factory, JK_MasterBillNum, PK, JK_TransportMode, IsCoLoad, JK_SystemCreateTimeUtc, IsImportingData);
				}
			}
		}

		public event EventHandler<ShowMessageOnGUIEventArgs> ShowMessageOnGUI;

		#region JK_AWBServiceLevel

		public override ZString JK_AWBServiceLevel
		{
			get { return base.JK_AWBServiceLevel; }
			set
			{
				if (JK_AWBServiceLevel != value)
				{
					base.JK_AWBServiceLevel = value;
					MAWBAllocation.MarkForReallocation(checkIsNotPrinted: true);
				}
			}
		}

		#endregion

		#region JK_IsNeutralMaster

		public override ZBool JK_IsNeutralMaster
		{
			get { return base.JK_IsNeutralMaster; }
			set
			{
				if (JK_IsNeutralMaster != value)
				{
					if (value || !MAWBAllocation.IsMAWBPrinted || AllowDeallocatePrintedNeutralMAWB())
					{
						base.JK_IsNeutralMaster = value;
						if (!value)
						{
							MAWBAllocation.MarkForDeallocation();
							MasterBillMAWB = "";
						}
						else
						{
							if (MAWBAllocation.AllocatedMawb != null)
							{
								MAWBAllocation.MarkForReallocationIfPrefixChanged(MasterBillAirlinePrefix);
								if (MAWBAllocation.AllocatedMawb != null)
								{
									MasterBillMAWB = MAWBAllocation.AllocatedMawb.JM_MAWB;
								}
							}
							else
							{
								MAWBAllocation.MarkForAllocation();
								MasterBillMAWB = "";
							}
						}

						MasterBillMAWBInfo.RefreshBinding();
						MasterBillNeutralMAWBInfo.RefreshBinding();
						MasterBillAirlinePrefixInfo.RefreshBinding();
					}
					else
					{
						JK_IsNeutralMasterInfo.RefreshBinding();
					}

					if (!value)
					{
						Validation.ValidateMasterBillNeutralMAWB();
					}
				}
			}
		}

		protected bool JK_IsNeutralMaster_ReadOnly
		{
			get
			{
				return !JK_IsNeutralMaster && !IsValidForNeutralMaster;
			}
		}

		bool AllowDeallocatePrintedNeutralMAWB()
		{
			if (Env.Security.JobMAWBResetPrintedFlag.IsAllowed)
			{
				CancelEventArgs args = new CancelEventArgs();

				CancelEventHandler handler = DeallocatePrintedNeutralMAWB;

				if (handler != null)
				{
					handler(this, args);
				}

				return !args.Cancel;
			}
			else
			{
				Globals.Message.Show(Env.Security.JobMAWBResetPrintedFlag.ErrorMessageForNotAllowed);
				return false;
			}
		}

		public event CancelEventHandler DeallocatePrintedNeutralMAWB;

		#endregion

		#region JK_RL_NKDischargePort

		public override ZString JK_RL_NKDischargePort
		{
			get { return base.JK_RL_NKDischargePort; }
			set
			{
				bool prevIsImport = this.IsImport();
				bool prevIsExport = this.IsExport();
				base.JK_RL_NKDischargePort = value;
				if (this.IsImport() != prevIsImport || this.IsExport() != prevIsExport)
				{
					OnImportExportChangedAfterDischargePortChange();
				}

				foreach (ForwardingShipment shipment in Shipments)
				{
					shipment.SetHandledOnBehalfOfForwarder(ForwardingShipment.ShipmentPropertyChanged.ConsolDischargePort);
				}
				DefaultSpecialHandlingItems();

				if (!IsValidationSuspended)
				{
					Validation.ValidateJK_RL_NKLoadPort();
					Validation.ValidateJK_ReceivingForwarderHandlingType();
				}
				RequiredDocuments.SetAllDocumentsReceivedEventLogger();

				this.UpdateCO2eStatusToNotCurrent(new CO2eStatusChangedReason(JK_RL_NKDischargePortInfo), IsCopying);
			}
		}

		void OnImportExportChangedAfterDischargePortChange()
		{
			ImportExportChangedAfterDischargePortChange?.Invoke(this, EventArgs.Empty);
		}

		public event EventHandler ImportExportChangedAfterDischargePortChange;

		#endregion

		#region JK_OA_ReceivingForwarderAddress

		public override ZGuid JK_OA_ReceivingForwarderAddress
		{
			get { return base.JK_OA_ReceivingForwarderAddress; }
			set
			{
				ZGuid oldValue = base.JK_OA_ReceivingForwarderAddress;
				base.JK_OA_ReceivingForwarderAddress = value;
				if (base.JK_OA_ReceivingForwarderAddress != oldValue)
				{
					if (!JK_OA_ReceivingForwarderAddress_InitValue.HasValue)
					{
						JK_OA_ReceivingForwarderAddress_InitValue = oldValue;
					}

					SetDefaultNotifyParty();
					SetDefaultConsignee();

					ScreeningStatusUpdater.SetShouldUpdateScreeningStatusByOrgAddress(this, JK_ScreeningStatus, Factory, JK_OA_ReceivingForwarderAddress);

					if (IsGatewayAgentOrGatewayAgentWithTariff(JK_ReceivingForwarderHandlingType) && oldValue != ZGuid.Empty)
					{
						foreach (ForwardingShipment shipment in Shipments)
						{
							shipment.SynchronizeShipmentGatewayFromConsols(this, oldValue, isDetachedConsol: true);
						}
					}

					if (!IsInDatabase)
					{
						SetReceivingForwarderHandlingTypeDefaultForAnyCompany();
					}
					else
					{
						SetReceivingForwarderHandlingTypeDefault();
					}

					if (!IsValidationSuspended && IsGatewayServiceLevelValidationNecessary(JK_OA_ReceivingForwarderAddress_InitValue, JK_OA_ReceivingForwarderAddress))
					{
						Validation.ValidateJK_RS_NKGatewayServiceLevel();
					}

					var agents = AllocationLine?.AgentPivots.GetAllAgents();
					if (agents is not null && agents.Count > 0)
					{
						Validation.ValidateJK_RCA_AllocationLine();

						foreach (var container in this.Containers.OfType<ForwardingContainer>().Where(container => !container.JC_RCA_AllocationLine.IsEmpty))
						{
							container.Validation.ValidateJC_RCA_AllocationLine();
						}
					}
				}

				SetShipmentHandledOnBehalfOfForwarder();
			}
		}

		ZGuid? JK_OA_ReceivingForwarderAddress_InitValue;

		void SetReceivingForwarderHandlingTypeDefault()
		{
			if ((IsAgent || IsCoLoad) && GlbDepartment.CurrentDepartment.IsGatewayDepartment)
			{
				var defaultHandlingType = ((ForwardingConsolGatewayBillingSupporter)Gateway.GatewayBillingSupporter).GetDefaultReceivingForwarderAddressGatewayType();

				JK_ReceivingForwarderHandlingType = defaultHandlingType;
				JK_ReceivingForwarderHandlingType_Defaulted = defaultHandlingType;
			}
		}

		void SetReceivingForwarderHandlingTypeDefaultForAnyCompany()
		{
			if ((IsAgent || IsCoLoad)
				&& GlbDepartment.CurrentDepartment.IsGatewayDepartment
				&& Gateway.GatewayBillingSupporter is ForwardingConsolGatewayBillingSupporter billingSupporter)
			{
				var handlingType = billingSupporter
					.AllGlbCompanies
					.Select(company => billingSupporter.GetDefaultReceivingForwarderAddressGatewayType(company))
					.FirstOrDefault(gatewayType => !gatewayType.IsEmpty);

				JK_ReceivingForwarderHandlingType = handlingType;
				JK_ReceivingForwarderHandlingType_Defaulted = handlingType;
			}
		}

		void SetShipmentHandledOnBehalfOfForwarder()
		{
			foreach (ForwardingShipment shipment in Shipments)
			{
				if (shipment.JS_JS_ColoadMasterShipment.IsEmpty && shipment.JS_IsCFSRegistered)
				{
					if (shipment.IsImport() && (CountryCode(JK_RL_NKDischargePort) == CountryCode(shipment.JS_RL_NKDestination)))
					{
						shipment.JS_OH_HandledOnBehalfOfForwarder = ReceivingForwarderPK;
					}
					else if (shipment.IsExport() && (CountryCode(JK_RL_NKLoadPort) == CountryCode(shipment.JS_RL_NKOrigin)))
					{
						shipment.JS_OH_HandledOnBehalfOfForwarder = SendingForwarderPK;
					}
				}
			}
		}

		#region SetDefaultShipper

		void SetDefaultShipper()
		{
			if (SendingForwarder == null)
			{
				return;
			}

			if (!IsDirect && TransportMode == Constants.TransportModes.Sea && SendingForwarder.MasterBillShipperOverrideDocumentaryAddress != null)
			{
				MasterBillShipperOverrideDocumentaryAddress.E2_OA_Address = SendingForwarder.MasterBillShipperOverrideDocumentaryAddress.E2_OA_Address;
				MasterBillShipperOverrideDocumentaryAddress.E2_Contact = SendingForwarder.MasterBillShipperOverrideDocumentaryAddress.E2_Contact;
				MasterBillShipperOverrideDocumentaryAddress.ContactPK = new DefaultContactFinder(SendingForwarder.MasterBillShipperOverride, false).DefaultContact(ContactType.Consignor)?.PK ?? ZGuid.Empty;
			}
		}

		#endregion

		#region SetDefaultNotifyParty

		void SetDefaultNotifyParty()
		{
			if (ReceivingForwarder == null)
			{
				return;
			}

			if (IsDirect || ReceivingForwarder.NotifyParty == null)
			{
				var contact = new DefaultContactFinder(ReceivingForwarder, false).DefaultContact(ContactType.NotifyParty);
				if (contact != null)
				{
					NotifyPartyDocumentaryAddress.OrganisationPK = ReceivingForwarder.PK;
					NotifyPartyDocumentaryAddress.ContactPK = contact.PK;
				}
			}
			else
			{
				if (!IsDirect && TransportMode == Constants.TransportModes.Sea && ReceivingForwarder.NotifyPartyDocumentaryAddress != null)
				{
					NotifyPartyDocumentaryAddress.E2_OA_Address = ReceivingForwarder.NotifyPartyDocumentaryAddress.E2_OA_Address;
					NotifyPartyDocumentaryAddress.E2_Contact = ReceivingForwarder.NotifyPartyDocumentaryAddress.E2_Contact;
					NotifyPartyDocumentaryAddress.ContactPK = new DefaultContactFinder(ReceivingForwarder.NotifyParty, false).DefaultContact(ContactType.NotifyParty)?.PK ?? ZGuid.Empty;
				}
			}
		}

		#endregion

		#region SetDefaultConsignee

		void SetDefaultConsignee()
		{
			if (ReceivingForwarder == null)
			{
				return;
			}

			if (!IsDirect && TransportMode == Constants.TransportModes.Sea && ReceivingForwarder.MasterBillConsigneeOverrideDocumentaryAddress != null)
			{
				MasterBillConsigneeOverrideDocumentaryAddress.E2_OA_Address = ReceivingForwarder.MasterBillConsigneeOverrideDocumentaryAddress.E2_OA_Address;
				MasterBillConsigneeOverrideDocumentaryAddress.E2_Contact = ReceivingForwarder.MasterBillConsigneeOverrideDocumentaryAddress.E2_Contact;
				MasterBillConsigneeOverrideDocumentaryAddress.ContactPK = new DefaultContactFinder(ReceivingForwarder.MasterBillConsigneeOverride, false).DefaultContact(ContactType.Consignee)?.PK ?? ZGuid.Empty;
			}
		}

		#endregion

		#region SetDefaultSelfFiler

		public void SetDefaultSelfFilerIfNecessary()
		{
			if (!IsInDatabase
					|| JK_AgentTypeInfo.HasChanges
					|| JK_OA_ReceivingForwarderAddressInfo.HasChanges
					|| JK_RL_NKLoadPortInfo.HasChanges
					|| JK_RL_NKDischargePortInfo.HasChanges
					|| Transports.HasChanges)
			{
				SetDefaultSelfFiler();
			}
		}

		void SetDefaultSelfFiler()
		{
			RemovePreviousSelfFiler();

			if (IsDirect || ReceivingForwarder == null || !IsICS2)
			{
				return;
			}

			var selfFilerOrg = (ReceivingForwarder?.MiscServ?.OM_FWAdvanceCargoReportingSelfFiler ?? false) ? ReceivingForwarder : GetFallbackSelfFilerOrg();
			if (selfFilerOrg != null)
			{
				var selfFilerAddressType = DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.SelfFiler);
				selfFilerAddressType.OrganisationPK = selfFilerOrg.PK;
				selfFilerAddressType.ReadOnlyStrategy = new JobDocAddressReadOnlyStrategy(() => true, () => true);
			}
		}

		OrgHeader GetFallbackSelfFilerOrg()
		{
			if (ReceivingForwarder == null)
			{
				return null;
			}

			var matchedOrgPK = GetMatchedOrgRelatedPartyRecord(ReceivingForwarder)?.PR_OH_RelatedParty;
			return matchedOrgPK.HasValue ? Factory.Load<OrgHeader>(matchedOrgPK.Value) : null;
		}

		public void RemovePreviousSelfFiler()
		{
			var selfFilerAddressType = DocAddresses.FindByDocAddressType(DocAddressType.SelfFiler);
			if (selfFilerAddressType != null)
			{
				DocAddresses.RemoveAndDelete(selfFilerAddressType);
			}
		}

		#endregion

		#endregion

		static ZString CountryCode(ZString unloco)
		{
			return unloco.SubstringSafe(0, 2);
		}

		#region JK_ScreeningStatus

		[ReadOnly(true)]
		[List("ScreeningStatusesList")]
		public override ZString JK_ScreeningStatus
		{
			get { return base.JK_ScreeningStatus; }
			set
			{
				base.JK_ScreeningStatus = value;
			}
		}

		#endregion

		#region JK_ConsolChargeableRate

		public override ZDecimal JK_ConsolChargeableRate
		{
			get { return base.JK_ConsolChargeableRate; }
			set
			{
				if (base.JK_ConsolChargeableRate != value)
				{
					bool continueWithOverride = true;
					if (!IsSettingChargeableRateFromAutoRating
						&& IsAWBHeaderAccessible
						&& AWBHeader.CalculationLogsAnalyzer.LogsWrapper != null
						&& !AWBHeader.CalculationLogsAnalyzer.LogsWrapper.IsDisabled)
					{
						CancelEventArgs cancelArgs = new CancelEventArgs(false);
						if (OnOverridingChargeableRate != null)
						{
							OnOverridingChargeableRate(this, cancelArgs);
						}

						if (cancelArgs.Cancel)
						{
							continueWithOverride = false;
						}
						else
						{
							AWBHeader.CalculationLogsAnalyzer.DisableLogs();
						}
					}

					if (continueWithOverride)
					{
						base.JK_ConsolChargeableRate = value;
					}

					JK_ConsolChargeableRateInfo.RefreshBinding();
				}
			}
		}

		internal bool IsSettingChargeableRateFromAutoRating;
		public event CancelEventHandler OnOverridingChargeableRate;

		#endregion

		#region JK_ConsolCutOffDate

		protected bool JK_ConsolCutOffDate_ReadOnly
		{
			get { return IsInDatabase && !Env.Security.ConsolAttachDetachShipmentAfterCutOffDate.IsAllowed; }
		}

		#endregion

		#region JK_Phase

		[List("Phases")]
		[WorkflowSetFieldReadonlyCheckBypass]
		public override ZString JK_Phase
		{
			get { return base.JK_Phase; }
			set
			{
				if (base.JK_Phase != value)
				{
					base.JK_Phase = value;

					if (!IsSettingDefaultValues && !IsImportingData)
					{
						InitialisePhaseDependantMandatoryValidation();
						InitialisePhaseDependantCustomBusinessObjectValidation();
						MarkAsNeedingValidation();
					}
				}
			}
		}

		protected bool JK_Phase_ReadOnly
		{
			get { return !Env.Security.ConsolPhaseSecurityOverride.IsAllowed; }
		}

		void InitialisePhaseDependantMandatoryValidation()
		{
			if (mandatoryValidationPhase != JK_Phase)
			{
				mandatoryValidationPhase = JK_Phase;
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
			if (customBusinessObjectMandatoryValidationPhase != JK_Phase || forceReinitialise)
			{
				customBusinessObjectMandatoryValidationPhase = JK_Phase;
				var selectedOnes = PhaseResolver.InitialisePhaseDependantMandatoryValidation(CustomBusinessObject.ZPropertyInfoHash.Cast<ZPropertyInfo>())
					.OfType<ZWrappedPropertyInfo>()
					.Select(p => p.InnerInfo.BizObj);
				MarkAsNeedingValidation(selectedOnes);
			}
		}
		ZString customBusinessObjectMandatoryValidationPhase = ZString.Empty;

		#endregion

		#region JK_IsCancelled

		public override ZBool JK_IsCancelled
		{
			get { return base.JK_IsCancelled; }
			set
			{
				if (base.JK_IsCancelled != value)
				{
					base.JK_IsCancelled = value;
					GetMAWBs<ICusMAWB>(reloadExistingRows: true, activeOnly: value)
						.ForEach(mawb => mawb.IsCancelled = value);

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
					relatedCancellableDataSupporter = ObjectFactory.Get<IConsolRelatedCancellableDataSupporter>();
				}

				return relatedCancellableDataSupporter;
			}
		}
		IRelatedCancellableDataSupporter relatedCancellableDataSupporter;

		#endregion

		#region JK_SZB

		public ZString JK_SZB
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

		public ZString JK_SZBInformation
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

		public ZDateTime JK_SZBIssueDate
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
			return CusEntryNums.OfType<CusEntryNumber>().FirstOrDefault(n => n.CE_EntryType == GermanyAdditionalReferenceNumberTypes.Codes.SZBNumber);
		}

		CusEntryNumber FindOrCreateSZBCusEntryNumber()
		{
			var entryNumber = FindSZBCusEntryNumber();
			if (entryNumber == null)
			{
				entryNumber = CusEntryNums.AddNew();
				entryNumber.CE_ParentID = PK;
				entryNumber.CE_ParentTable = TableName;
				entryNumber.CE_EntryType = GermanyAdditionalReferenceNumberTypes.Codes.SZBNumber;
				entryNumber.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
				entryNumber.CE_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				entryNumber.CE_EntryIsSystemGenerated = false;
			}

			return entryNumber;
		}

		#endregion

		#region JK_MasterBillIssueDate

		public override ZDateTime JK_MasterBillIssueDate
		{
			get { return base.JK_MasterBillIssueDate; }
			set
			{
				if (base.JK_MasterBillIssueDate != value)
				{
					base.JK_MasterBillIssueDate = value;

					if (!IsImportingData && !IsSettingDefaultValues)
					{
						Shipments.Cast<ForwardingShipment>()
							.Where(x => x.AviationSecurity.SupplyChainSecurityConfiguration.IsRecalculationNeeded(x.JS_InspectionTypeCode).Equals(true)).ToList()
							.ForEach(x => x.SetApprovedShipperStatus(Res.GetString("0eba32a5-7cfc-47f4-8c9b-adcc35285650", "{0} has been changed", JK_MasterBillIssueDateInfo.HumanReadableName)));
					}

					if (!IsValidationSuspended)
					{
						Shipments.ForEach(x => x.MarkAsNeedingValidation());
					}
				}
			}
		}

		#endregion

		#region Carrier Contract

		[List(nameof(CarrierContractCollection))]
		public override ZString JK_CarrierContractNumber
		{
			get => base.JK_CarrierContractNumber;
			set
			{
				if (base.JK_CarrierContractNumber != value)
				{
					base.JK_CarrierContractNumber = value;

					if (value.IsEmpty)
					{
						ClearAllAllocationRoutes();
					}

					if (!IsValidationSuspended)
					{
						Validation.ValidateJK_CarrierContractNumber();
					}
				}
			}
		}

		public IRatingContract CarrierContract => ContractAllocationHelper.GetCarrierContract(Factory, JK_CarrierContractNumber, ShippingLinePK);

		void ClearAllAllocationRoutes()
		{
			JK_RCA_AllocationLine = ZGuid.Empty;
			DefaultAllocationRouteOnAllContainers(ZGuid.Empty);
		}

		void DefaultAllocationRouteOnAllContainers(ZGuid allocationRoute)
		{
			foreach (var container in Containers.OfType<ForwardingContainer>())
			{
				container.JC_RCA_AllocationLine = allocationRoute;
			}
		}

		#endregion

		#region Allocation Line

		[List(nameof(AllocationLineCollection))]
		public override ZGuid JK_RCA_AllocationLine
		{
			get => base.JK_RCA_AllocationLine;
			set
			{
				if (base.JK_RCA_AllocationLine != value)
				{
					base.JK_RCA_AllocationLine = value;

					DefaultAllocationRouteOnAllContainers(value);
					AllocationRouteContainerWeightLimitHelper.CheckAndPromptOverrideOnContainerWeightLimit(value);
					PopulateBlankFieldsFromRoute();

					if (!IsValidationSuspended)
					{
						Validation.ValidateJK_RCA_AllocationLine();
					}

					AllocationRouteContainerWeightLimitHelper.SetRequireApprovalEventCancellationCheck();
				}
			}
		}

		void PopulateBlankFieldsFromRoute()
		{
			if (AllocationLine == null)
			{
				return;
			}

			if (AllocationLine.RCA_JX_SailingSchedule.IsEmpty)
			{
				PopulateConsolFieldsFromRoute();
				PopulateTransportFieldsFromRoute();
			}
		}

		void PopulateConsolFieldsFromRoute()
		{
			var allocationRoute = AllocationLine;

			if (ContractAllocationHelper.ConsolLoadWillBeDefaulted(this, allocationRoute))
			{
				JK_RL_NKLoadPort = allocationRoute.RCA_LoadLocation;
			}

			if (ContractAllocationHelper.ConsolDischargeWillBeDefaulted(this, allocationRoute))
			{
				JK_RL_NKDischargePort = allocationRoute.RCA_DischargeLocation;
			}
		}

		void PopulateTransportFieldsFromRoute()
		{
			if (Transports.Count != 1)
			{
				return;
			}

			var transport = Transports[0];
			var allocationRoute = AllocationLine;

			if (ContractAllocationHelper.SingleLegLoadWillBeDefaulted(this, allocationRoute))
			{
				transport.JW_RL_NKLoadPort = allocationRoute.RCA_LoadLocation;
			}

			if (ContractAllocationHelper.SingleLegDischargeWillBeDefaulted(this, allocationRoute))
			{
				transport.JW_RL_NKDiscPort = allocationRoute.RCA_DischargeLocation;
			}

			if (ContractAllocationHelper.CanSingleLegScheduleDetailsBeDefaulted(this, allocationRoute))
			{
				transport.JW_Vessel = allocationRoute.RCA_RV_NKVessel;
				transport.JW_VoyageFlight = allocationRoute.RCA_VoyageNumber;
				transport.JW_ServiceString = allocationRoute.RCA_ServiceLoop;
			}
		}

		public IRatingContractAllocationLine AllocationLine => Factory.Load<IRatingContractAllocationLine>(JK_RCA_AllocationLine);

		#endregion

		#region JK_RL_NKCarrierBookingOffice

		void DefaultCarrierBookingOffice()
		{
			var sourceAddress = IsSea ? IsCoLoad ? CreditorAddress : ShippingLineAddress : null;
			JK_RL_NKCarrierBookingOffice = sourceAddress?.OA_RL_NKRelatedPortCode ?? ZString.Empty;
		}

		#endregion

		#endregion

		#region Logs

#if DEBUG
		public BusinessObject[] GetBusinessObjectsWithRelatedEvents() => BusinessObjectsWithRelatedEventsCore;
#endif
		protected override BusinessObject[] BusinessObjectsWithRelatedEventsCore
		{
			get
			{
				List<BusinessObject> result = new List<BusinessObject>(base.BusinessObjectsWithRelatedEventsCore);
				if (SeaCargoOceanBill != null)
				{
					result.Add(SeaCargoOceanBill);
					result.AddRange(SeaCargoContainers);
					result.AddRange(SeaCargoHouses);
				}
				var auMawb = AUCusMAWB as BusinessObject;
				if (auMawb != null)
				{
					result.Add(auMawb);
				}
				var gbMawbs = GBCusMAWBs;
				if (gbMawbs != null)
				{
					foreach (EnterpriseBusinessObject gbMawb in gbMawbs)
					{
						result.Add(gbMawb);
						result.AddRange(gbMawb.BusinessObjectsWithRelatedEvents);
					}
				}
				var globalManifests = GetGlobalManifestHeaders();
				if (globalManifests != null)
				{
					foreach (EnterpriseBusinessObject manifest in globalManifests)
					{
						result.Add(manifest);
						result.AddRange(manifest.BusinessObjectsWithRelatedEvents);
					}
				}

				var nctsheader = (EnterpriseBusinessObject)NctsHeaderForDocuments;
				if (nctsheader != null)
				{
					result.Add(nctsheader);
					result.AddRange(nctsheader.BusinessObjectsWithRelatedEvents);
				}

				result.AddRange(TransportBookingLoader.GetRelatedTransportBookingEvents(this));

				var jpAFRHeader = this.GetAFRHeader(false) as EnterpriseBusinessObject;
				if (jpAFRHeader != null)
				{
					result.Add(jpAFRHeader);
					result.AddRange(jpAFRHeader.BusinessObjectsWithRelatedEvents);
				}

				var documentDataLoader = ObjectFactory.Get<IVisualizerDocumentDataLoader>();
				var documentData = documentDataLoader.Load(this);

				if (documentData != null)
				{
					result.AddRange(documentData.Cast<BusinessObject>());
				}

				var cusMHMaster = (EnterpriseBusinessObject)CusCAeMHMaster;
				if (cusMHMaster != null)
				{
					result.Add(cusMHMaster);
					result.AddRange(cusMHMaster.BusinessObjectsWithRelatedEvents);
				}

				var usAMS = USAMS as EnterpriseBusinessObject;
				if (usAMS != null)
				{
					result.Add(usAMS);
					result.AddRange(usAMS.BusinessObjectsWithRelatedEvents);
				}

				if (Job != null)
				{
					result.Add(Job);
				}

				if (IsTemplate && TemplateRecord != null)
				{
					result.Add(TemplateRecord);
				}

				return result.ToArray();
			}
		}

		protected override Logs GetNewLogs()
		{
			var result = base.GetNewLogs();
			if (!Env.CurrentUser.IsSupportUser)
			{
				result.EventsThatCannotBeAdded.Add(AutoEvents.OceanCarrierBookingByTEU);
			}

			return result;
		}

		#endregion

		#region Notes

		public override BusinessObject[] BusinessObjectsWithRelatedNotes
		{
			get
			{
				var result = new List<BusinessObject>(base.BusinessObjectsWithRelatedNotes);
				var documentDataLoader = ObjectFactory.Get<IVisualizerDocumentDataLoader>();
				result.AddRange(documentDataLoader.Load(this).Cast<BusinessObject>());

				return result.ToArray();
			}
		}

		public ConsolNotesChecker NotesChecker => notesChecker ?? (notesChecker = new ConsolNotesChecker(this));
		ConsolNotesChecker notesChecker;

		public override Notes Notes => notes ?? (notes = new ForwardingConsolStmNotes(this));
		Notes notes;

		#endregion

		#region Lookups

		public new ForwardingConsolLookups Lookups => (ForwardingConsolLookups)base.Lookups;

		protected override JobConsolLookups GetNewLookups()
		{
			return new ForwardingConsolLookups(this);
		}

		#region ChargesApplyLookup

		public CodeDescriptionPairList ChargesApplyLookup => Factory.GetCachedValue("ChargesApplyHelper.ChargesApplyPairList", () => ChargesApplyHelper.ChargesApplyPairList);

		#endregion

		#region Service Levels

		public OrgCarrierServiceLevelCollection NeutralAirWaybillServiceLevelList
		{
			get
			{
				if (fCarrierServiceLevels == null || fCarrierServiceLevels.Master != ShippingLine)
				{
					OrgCarrierServiceLevelCollection lCarrierServiceLevels;
					if (ShippingLine == null)
					{
						lCarrierServiceLevels = new OrgCarrierServiceLevelCollection(Factory);
					}
					else
					{
						lCarrierServiceLevels = new OrgCarrierServiceLevelCollection(ShippingLine.MiscServ);
					}
					lCarrierServiceLevels.Load();
					fCarrierServiceLevels = lCarrierServiceLevels;
				}

				return fCarrierServiceLevels;
			}
		}

		OrgCarrierServiceLevelCollection fCarrierServiceLevels;

		#endregion

		#region Cartage Local Transport List

		public LocalTransportCollection LocalTransportList
		{
			get { return fLocalTransportList ?? (fLocalTransportList = new LocalTransportCollection(Factory)); }
		}
		protected LocalTransportCollection fLocalTransportList;

		#endregion

		#region AWB Dimensions

		public CodeDescriptionPairList AWBDimsCodeDescriptionPairList
		{
			get { return fAWBDimsCodeDescriptionPairList ?? (fAWBDimsCodeDescriptionPairList = new CodeDescriptionPairList(OLookUpEditType.AWBDimensions)); }
		}
		CodeDescriptionPairList fAWBDimsCodeDescriptionPairList;

		#endregion

		#region CarrierContractCollection

		public ICarrierContractCollection CarrierContractCollection
		{
			get
			{
				if (carrierContractCollection == null)
				{
					carrierContractCollection = ObjectFactory.Get<ICarrierContractCollection>(nameof(ICarrierContractCollection), Factory);
				}

				return carrierContractCollection;
			}
		}

		ICarrierContractCollection carrierContractCollection;

		#endregion

		#region AllocationLineCollection

		public FilteredRatingContractAllocationLineCollection AllocationLineCollection
		{
			get
			{
				if (allocationLineCollection == null)
				{
					allocationLineCollection = new FilteredRatingContractAllocationLineCollection(Factory, () => CarrierContract?.PK);
				}

				return allocationLineCollection;
			}
		}

		FilteredRatingContractAllocationLineCollection allocationLineCollection;

		#endregion

		#region GatewayHandlingTypeList

		public CodeDescriptionPairList GatewayHandlingTypeList
		{
			get
			{
				return Factory.GetCachedValue("FreightCodePairLists.GatewayHandlingTypeList", () => FreightCodePairLists.GatewayForwarderHandlingTypeList());
			}
		}

		#endregion

		#region Units

		public CodeDescriptionPairList WeightUnits
		{
			get { return Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Weight); }
		}

		public CodeDescriptionPairList VolumeUnits
		{
			get { return Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Volume); }
		}

		#endregion

		#region Screening Statuses

		public CodeDescriptionPairList ScreeningStatusesList
		{
			get { return fScreeningStatusesList ?? (fScreeningStatusesList = new ScreeningStatusesList()); }
		}
		CodeDescriptionPairList fScreeningStatusesList;

		#endregion

		#region Phases

		public CodeDescriptionPairList Phases
		{
			get
			{
				if (phases == null)
				{
					phases = PhaseConstants.GetCommonPhaseList();
					phases.AddRange(PhaseResolver.GetPhaseList());
				}

				return phases;
			}
		}
		CodeDescriptionPairList phases;

		#endregion

		#region Binding

		public CodeDescriptionPairList TemperatureUnits
		{
			get { return BindToLists.GetCachedLists(Factory).TemperatureUnits; }
		}

		#endregion

		#region SecurityStatusList

		public CodeDescriptionPairList SecurityStatusList => Factory.GetCachedValue("ForwardingConsolSecurityStatusList", () =>
		{
			var result = new CodeDescriptionPairList();
			foreach (ICodeDescription codePair in new AWBSpecialHandlingCodeDescriptionPairList())
			{
				if (AWBSpecialHandlingCodeDescriptionPairList.IsCargoSecurityStatusCode(codePair.Code))
				{
					result.Add(codePair);
				}
			}

			if (ShouldRemoveSCOSecurityStatus)
			{
				result.RemoveCode(AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForAllCargoAircraftOnly);
			}

			result.Add(new CodeDescriptionPair(SecurityJobConsolAWBSpecialHandling.NotSecured,
				Res.GetString("d1e22dfb-9030-668e-4b3c-b93465f5e572",
					"Not secured - SPH will not default to the MAWB")));
			return result;
		});

		#endregion

		public bool ShouldRemoveSCOSecurityStatus => CountryCodes.EuropeanUnionAviationSecurityMembersList.Contains(GlbCompany.CurrentCompany.GC_RN_NKCountryCode.ToString()) && CountryCodes.EuropeanUnionAviationSecurityMembersList.Contains(CountryCode(JK_RL_NKLoadPort).ToString());

		#endregion

		#region Related Business Objects

		#region AWBHeader

		public ZBool IsAWBHeaderAccessible
		{
			get { return IsAir && !IsCourier && !IsMultiAWBMaster; }
		}

		bool IAWBParent.IsAWBHeaderAccessible => IsAWBHeaderAccessible;

		public ExportAWBHeader AWBHeader
		{
			get { return AWBHeaderManager.AWBHeader; }
		}

		[ChildEditable(true)]
		public AWBHeaderManager<ForwardingConsol> AWBHeaderManager
		{
			get
			{
				if (awbHeaderManager == null)
				{
					awbHeaderManager = new AWBHeaderManager<ForwardingConsol>(this);
					RegisterEditableChildObject(awbHeaderManager);
					awbHeaderManager.SetReadOnlyIncludingChildren(!JK_OverrideWaybillDefaults);
				}

				return awbHeaderManager;
			}
		}
		AWBHeaderManager<ForwardingConsol> awbHeaderManager;

		public event CancelEventHandler OnOverrideWaybillDefaultsChanging;

		public override ZBool JK_OverrideWaybillDefaults
		{
			get { return base.JK_OverrideWaybillDefaults; }
			set
			{
				if (base.JK_OverrideWaybillDefaults != value)
				{
					if (!IsOverrideAllowed && !IsSettingDefaultValues)
					{
						JK_OverrideWaybillDefaultsInfo.RefreshBinding();

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
							base.JK_OverrideWaybillDefaults = value;
							OverrideWaybillDefaultsHasChanges = true;

							if (IsAWBHeaderAccessible)
							{
								AWBHeader.Populate();
							}
						}
						else
						{
							JK_OverrideWaybillDefaultsInfo.RefreshBinding();
						}
					}
					else
					{
						base.JK_OverrideWaybillDefaults = value;
						if (IsAWBHeaderAccessible)
						{
							AWBHeader.EH_AreRateLinesOverridden = true;
						}

						OverrideWaybillDefaultsHasChanges = true;
					}

					AWBHeaderManager.SetReadOnlyIncludingChildren(!JK_OverrideWaybillDefaults);

					if (IsAWBHeaderAccessible)
					{
						AWBHeader.ExportAWBSecurityStatusLines.SetReadOnlyIncludingChildren(!JK_OverrideSecurityDeclarationDefaults);
					}
				}
			}
		}

		public bool OverrideWaybillDefaultsHasChanges { get; private set; }

		public event CancelEventHandler OnOverrideSecurityDeclarationDefaultsChanging;

		public override ZBool JK_OverrideSecurityDeclarationDefaults
		{
			get { return base.JK_OverrideSecurityDeclarationDefaults; }
			set
			{
				if (base.JK_OverrideSecurityDeclarationDefaults != value)
				{
					if (!IsOverrideAllowed && !IsSettingDefaultValues)
					{
						JK_OverrideSecurityDeclarationDefaultsInfo.RefreshBinding();

						return;
					}

					if (value == ZBool.False)
					{
						CancelEventArgs args = new CancelEventArgs(false);
						if (OnOverrideSecurityDeclarationDefaultsChanging != null)
						{
							OnOverrideSecurityDeclarationDefaultsChanging(this, args);
						}

						if (!args.Cancel)
						{
							base.JK_OverrideSecurityDeclarationDefaults = value;
							OverrideSecurityDeclarationHasChanges = true;

							if (IsAWBHeaderAccessible)
							{
								AWBHeader.PopulateSecurityDeclarationIfNotOverridden();
							}
						}
						else
						{
							JK_OverrideSecurityDeclarationDefaultsInfo.RefreshBinding();
						}
					}
					else
					{
						base.JK_OverrideSecurityDeclarationDefaults = value;
						OverrideSecurityDeclarationHasChanges = true;
					}

					if (IsAWBHeaderAccessible)
					{
						AWBHeader.ExportAWBSecurityStatusLines.SetReadOnlyIncludingChildren(!value);
					}

					AWBHeaderManager.RefreshBinding();
				}
			}
		}

		public bool OverrideSecurityDeclarationHasChanges { get; private set; }

		public void PopulateAWB()
		{
			if (IsAWBHeaderAccessible)
			{
				AWBHeader.PopulateIfNotOverridden();
				AWBHeader.AWBSpecialHandlingItems.ValidateAll();
			}
		}

		ExportAWBHeader IAWBParent.LoadOrCreateAWB()
		{
			return ConsolExportAWBHeader.LoadOrCreate(this);
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

		public bool IsOverrideAllowed => Env.Security.MaintainConsolAWBOverride.IsAllowed;

		public ZBool IsAWBValuesOverriddenProperty
		{
			get { return JK_OverrideWaybillDefaults; }
			set { JK_OverrideWaybillDefaults = value; }
		}

		public ZPropertyInfo IsAWBValuesOverriddenPropertyInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(IsAWBValuesOverriddenProperty), x => JK_OverrideWaybillDefaultsInfo); }
		}

		public ZBool IsCSDValuesOverriddenProperty
		{
			get { return JK_OverrideSecurityDeclarationDefaults; }
			set { JK_OverrideSecurityDeclarationDefaults = value; }
		}

		public ZPropertyInfo IsCSDValuesOverriddenPropertyInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(IsCSDValuesOverriddenProperty), x => JK_OverrideSecurityDeclarationDefaultsInfo); }
		}

		public ZDateTime FinalMAWBPrintedDate
		{
			get { return IsAWBHeaderAccessible ? AWBHeader.EH_FinalizationDate : ZDateTime.Empty; }
		}

		public ZBool IsFinalMAWBPrinted
		{
			get => !FinalMAWBPrintedDate.IsEmpty;
		}

		event EventHandler IAWBParent.IsAWBHeaderAccessibleChanged
		{
			add
			{
				JK_TransportModeInfo.ValueChanged += value;
				JK_AgentTypeInfo.ValueChanged += value;
			}
			remove
			{
				JK_TransportModeInfo.ValueChanged -= value;
				JK_AgentTypeInfo.ValueChanged -= value;
			}
		}

		ZString IAWBParent.HAWB
		{
			get { return ZString.Empty; }
		}

		ZString IAWBParent.MAWB
		{
			get { return JK_MasterBillNum; }
		}

		#region CIM/AWB/FWB Messages

		#region CIM Message Collection

		public CIMEDIMessageCollection CIMEDIMessages
		{
			get
			{
				if (cIMEDIMessages == null)
				{
					cIMEDIMessages = new CIMEDIMessageCollection(this);
					var filter = new ZQuery(EDIMessageSchema.EM_LinkTable, Schema.TableName);
					cIMEDIMessages.Load(filter);
					cIMEDIMessages.IsManagedForDataRefresh = true;
					Validation.ValidateAWBCurrentStatus();
				}

				return cIMEDIMessages;
			}
		}
		CIMEDIMessageCollection cIMEDIMessages;

		#endregion

		#region AWBCurrentStatus

		[MaxLength(1024)]
		public ZString AWBCurrentStatus
		{
			get { return CIMEDIMessages.CurrentStatus.ToUpper(); }
		}

		public ZPropertyInfo AWBCurrentStatusInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.AWBCurrentStatus); }
		}

		#endregion

		public bool AnyGoodsTravelToOrThroughFHLCountry
		{
			get { return ShipmentOriginateOrDestinedForFHLCountry || TransportRoutedThroughFHLCountry; }
		}

		bool ShipmentOriginateOrDestinedForFHLCountry
		{
			get
			{
				foreach (ForwardingShipment shipment in Shipments)
				{
					if ((shipment.Origin != null && IsFHLCountry(shipment.Origin.RL_RN_NKCountryCode))
						|| (shipment.Destination != null && IsFHLCountry(shipment.Destination.RL_RN_NKCountryCode)))
					{
						return true;
					}
				}

				return false;
			}
		}

		bool TransportRoutedThroughFHLCountry
		{
			get
			{
				foreach (Transport transport in Transports)
				{
					if ((transport.LoadPort != null && IsFHLCountry(transport.LoadPort.RL_RN_NKCountryCode))
						|| (transport.DiscPort != null && IsFHLCountry(transport.DiscPort.RL_RN_NKCountryCode)))
					{
						return true;
					}
				}

				return false;
			}
		}

		bool IsFHLCountry(string countryCode)
		{
			return (FHLCountries.ContainsCode(countryCode) || FHLEUCountries.ContainsCode(countryCode));
		}

		internal CodeDescriptionPairList FHLCountries
		{
			get
			{
				if (fhlCountries == null)
				{
					fhlCountries = new CodeDescriptionPairList();
					fhlCountries.AddPair(Core.Constants.CountryCodes.UnitedStates, Res.GetString("69387db6-6bb2-4265-a6d9-96e8310310b0", "United States"));
					fhlCountries.AddPair(Core.Constants.CountryCodes.Canada, Res.GetString("b1095690-52a5-4763-95ba-69bec99b637f", "Canada"));
					fhlCountries.AddPair(Core.Constants.CountryCodes.Indonesia, Res.GetString("3ef4afc8-82e9-4316-9d7e-98e6d13f01f1", "Indonesia"));
					fhlCountries.AddPair(Core.Constants.CountryCodes.Malaysia, Res.GetString("59216cd6-2e63-4013-8f00-c054e1c7821d", "Malaysia"));
					fhlCountries.AddPair(Core.Constants.CountryCodes.SouthAfrica, Res.GetString("07937608-c972-4af2-8452-d753a6365d89", "South Africa"));
					fhlCountries.AddPair(Core.Constants.CountryCodes.India, Res.GetString("219940de-7771-4904-8254-e5cde8635da7", "India"));
					fhlCountries.AddPair(Core.Constants.CountryCodes.China, Res.GetString("9086641b-4667-43d9-a2a1-8d1ca77a4db5", "China"));
					fhlCountries.AddPair(Core.Constants.CountryCodes.KoreaSouth, Res.GetString("2f489e4b-fe44-4039-8b94-f67f39f9060e", "South Korea"));
					fhlCountries.AddPair(Core.Constants.CountryCodes.Thailand, Res.GetString("cc3b1389-e472-4e16-ade7-b5664f682d00", "Thailand"));
					fhlCountries.AddPair(Core.Constants.CountryCodes.Taiwan, Res.GetString("c7f80be1-638f-4eb1-8162-ddccfe8d7f80", "Taiwan"));
					fhlCountries.AddPair(Core.Constants.CountryCodes.VietNam, Res.GetString("56b5e4d7-4e08-42ed-9021-5629a369c737", "Vietnam"));
					fhlCountries.AddPair(Core.Constants.CountryCodes.Japan, Res.GetString("05252e41-dd59-10b2-4483-c0423393aad8", "Japan"));
				}

				return fhlCountries;
			}
		}

		CodeDescriptionPairList fhlCountries;

		internal CodeDescriptionPairList FHLEUCountries
		{
			get
			{
				if (fhlEUCountries == null)
				{
					fhlEUCountries = new CodeDescriptionPairList();
					RefCountry[] countries = Factory.Load<RefCountry>(new ZQuery(RefCountrySchema.RN_EconomicGrouping, EconomicGroupList.Codes.EuropeanUnion));
					fhlEUCountries.AddRange(countries);
				}

				return fhlEUCountries;
			}
		}

		CodeDescriptionPairList fhlEUCountries;

		#endregion

		public static string MAWBPrintedLogReference
		{
			get { return (NoResString)"MAWB Printed"; } // Print Log Identifier
		}

		public static string HAWBPrintedLogReference
		{
			get { return (NoResString)"HAWB Printed"; } // Print Log Identifier
		}

		public ZString AWBAgentApprovalNumber => AWBHeader?.EH_AgentApprovalNumber ?? ZString.Empty;

		public ZString AWBAgentApprovalCountryCode => AWBHeader?.EH_RN_NKAgentApprovalCountryCode ?? ZString.Empty;

		#endregion

		public void ValidateDocs()
		{
			if (!IsValidationSuspended)
			{
				AWBSpecialHandlingItems.RunPreSaveValidation();
			}
		}

		#region Special Handling

		[ChildEditable]
		public NonSecurityJobConsolAWBSpecialHandlingCollection AWBSpecialHandlingItems
		{
			get
			{
				if (awbSpecialHandlingItems == null)
				{
					awbSpecialHandlingItems = new NonSecurityJobConsolAWBSpecialHandlingCollection(this);
					awbSpecialHandlingItems.Load();
					RegisterEditableChildObject(awbSpecialHandlingItems);
				}
				return awbSpecialHandlingItems;
			}
		}
		NonSecurityJobConsolAWBSpecialHandlingCollection awbSpecialHandlingItems;

		#endregion

		#region Security Status

		[BusinessObjectTestExclude()]
		[List(nameof(SecurityStatusList))]
		[MaxLength(3)]
		public ZString SecurityStatusCode
		{
			get
			{
				if (!IsAir)
				{
					return string.Empty;
				}

				if (!securityStatusCode.HasValue)
				{
					securityStatusCode = SecurityStatusCodeSpecialHandling != null
						? SecurityStatusCodeSpecialHandling.JKH_Code
						: GetAviationSecurityCode();
				}

				return securityStatusCode.Value;
			}
			set
			{
				if (SecurityStatusCode != value)
				{
					SecurityStatusCodeHasChanges = true;

					if (SecurityStatusCodeSpecialHandling != null)
					{
						if (string.IsNullOrWhiteSpace(value))
						{
							SecurityStatusCodeSpecialHandling.Delete();
						}
						else if (SecurityStatusCodeSpecialHandling.IsInDatabase)
						{
							SecurityStatusCodeSpecialHandling.JKH_Code = value;
						}
						else if (value == GetAviationSecurityCode())
						{
							SecurityStatusCodeSpecialHandling.Delete();
						}
						else
						{
							SecurityStatusCodeSpecialHandling.JKH_Code = value;
						}
					}
					else if (!string.IsNullOrWhiteSpace(value))
					{
						CreateSecurityStatusCodeSpecialHandling(value);
					}

					RefreshSecurityStatusCode();
					if (!IsValidationSuspended)
					{
						Validation.ValidateSecurityStatusCode();
					}
				}
			}
		}
		ZString? securityStatusCode;

		public ZPropertyInfo SecurityStatusCodeInfo => GetZPropertyInfo(Schema.SecurityStatusCode);

		internal bool SecurityStatusCodeHasChanges { get; private set; }

		SecurityJobConsolAWBSpecialHandling SecurityStatusCodeSpecialHandling
		{
			get
			{
				if (securityStatusCodeSpecialHandling == null || securityStatusCodeSpecialHandling.IsDeleted)
				{
					securityStatusCodeSpecialHandling = LoadSecurityStatusCodeSpecialHandling();
					RegisterEditableChildObject(securityStatusCodeSpecialHandling);
				}

				return securityStatusCodeSpecialHandling;
			}
		}
		SecurityJobConsolAWBSpecialHandling securityStatusCodeSpecialHandling;

		SecurityJobConsolAWBSpecialHandling LoadSecurityStatusCodeSpecialHandling()
		{
			var securityStatusCodes = SecurityStatusList.GetAllCodes().ToList();
			if (!securityStatusCodes.Contains(string.Empty))
			{
				securityStatusCodes.Add(string.Empty);
			}

			var query = new ZQuery(JobConsolAWBSpecialHandlingSchema.JKH_Code, securityStatusCodes);
			query.AddToFilter(JoinCondition.And, JobConsolAWBSpecialHandlingSchema.JKH_JK_Consol, PK);
			query.FetchOnlyFromLocalCache = !IsInDatabase;

			return Factory.LoadTop1<SecurityJobConsolAWBSpecialHandling>(query);
		}

		void CreateSecurityStatusCodeSpecialHandling(ZString code)
		{
			securityStatusCodeSpecialHandling = Factory.New<SecurityJobConsolAWBSpecialHandling>();
			securityStatusCodeSpecialHandling.JKH_JK_Consol = PK;
			SecurityStatusCodeSpecialHandling.JKH_Code = code;
			RegisterEditableChildObject(securityStatusCodeSpecialHandling);
		}

		internal void RefreshSecurityStatusCode()
		{
			securityStatusCode = null;
			SecurityStatusCodeInfo.RefreshBinding();
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
			get { return JK_MasterBillNumInfo; }
		}

		ZPropertyInfo IBillDetails.BKGBillNumberInfo
		{
			get { return null; }
		}

		ZPropertyInfo[] IBillDetails.GetNumberOfPackesInfos(ForwardingShipment shipment)
		{
			return Array.Empty<ZPropertyInfo>();
		}
		ZPropertyInfo[] IBillDetails.GetTypeOfPackesInfos(ForwardingShipment shipment)
		{
			return Array.Empty<ZPropertyInfo>();
		}

		IEnumerable<OrgHeader> IBillDetails.SCACIssuers
		{
			get
			{
				var masterBillIssuingParty = MasterBillIssuingParty;
				if (masterBillIssuingParty != null)
				{
					yield return masterBillIssuingParty;
				}
				var shippingLine = ShippingLine;
				if (shippingLine != null)
				{
					yield return shippingLine;
				}
			}
		}

		ZDateTime IBillDetails.BillUssueDate => JK_MasterBillIssueDate;

		#endregion

		#region Numbers

		protected override void OnNumbersLoaded()
		{
			base.OnNumbersLoaded();

			Numbers.CountChanged += Numbers_CountChanged;
		}

		void Numbers_CountChanged(object sender, CollectionCountChangedEventArgs args)
		{
			if (args?.BizObject is CusEntryNumber cusEntryNumber
				&& args.ItemAdded
				&& !cusEntryNumber.IsDeleted
				&& IsAdditionalReferenceNumberForSystemOnly(cusEntryNumber))
			{
				AddCannotDeleteNumberHandler(cusEntryNumber);
			}
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
				ConsolNonCustomsAdditionalReferenceCodesCodeList.Codes.CarrierShipperReference,
				ConsolNonCustomsAdditionalReferenceCodesCodeList.Codes.CarrierMessageReference
			};

			return entryTypesForSystemOnly.Contains(cusEntryNumber.CE_EntryType) && cusEntryNumber.CE_EntryIsSystemGenerated;
		}

		void AddCannotDeleteNumberHandler(CusEntryNumber cusEntryNumber)
		{
			AddCannotDeleteNumberHandler(cusEntryNumber, ResString.GetMultilingualString("af4627d8-fbfd-4b60-9124-17a47a19c32e", "The {0} is system generated and cannot be deleted.", cusEntryNumber.CE_EntryType));
		}

		#endregion

		#region Containers

		public new ForwardingContainerCollection Containers
		{
			get { return (ForwardingContainerCollection)base.Containers; }
		}

		protected override CommonContainerCollection GetNewContainerCollection()
		{
			return new ForwardingContainerCollection(this, Factory);
		}

		bool originalRequireTEU;

		protected override void OnContainersCreated()
		{
			originalRequireTEU = (this as ICO2eProvider).RequireTEU;
			void UpdateCO2eStatusToNCUWhenContainerTEUChanged(object s, EventArgs e)
			{
				var shipments = Shipments
					.OfType<ForwardingShipment>()
					.Where(shipment => (s as ForwardingContainer).PackLines.OfType<ForwardingPackLine>().Any(packline => packline.JL_JS == shipment.PK))
					.Cast<ICO2eProvider>();

				if (!this.SkipCO2eStatusCheck() || shipments.Any(shipment => !shipment.SkipCO2eStatusCheck()))
				{
					var newRequireTEU = (this as ICO2eProvider).RequireTEU;

					if ((originalRequireTEU || newRequireTEU) && !IsDeleted && !IsDeleting)
					{
						var reason = e is ValueChangedEventArgs valueChangedEventArgs
							? new CO2eStatusChangedReason(valueChangedEventArgs)
							: new CO2eStatusChangedReason(freeTextReason: (NoResString)"Container");

						this.UpdateCO2eStatusToNotCurrent(reason, IsCopying);
						shipments?.ForEach(shipment => shipment.UpdateCO2eStatusToNotCurrent(reason, IsCopying));
					}
				}
			}

			void UpdateCO2eStatusToNCUWhenContainerWeightChanged(object sender, EventArgs e)
			{
				if (!this.SkipCO2eStatusCheck() && (this as ICO2eProvider).RequireTEU && !IsDeleted && !IsDeleting)
				{
					this.UpdateCO2eStatusToNotCurrent(new CO2eStatusChangedReason(e as ValueChangedEventArgs), IsCopying);
				}
			}

			foreach (var container in Containers.Cast<ForwardingContainer>())
			{
				container.JC_ContainerCountInfo.ValueChanged += UpdateCO2eStatusToNCUWhenContainerTEUChanged;
				container.JC_RCInfo.ValueChanged += UpdateCO2eStatusToNCUWhenContainerTEUChanged;
				container.JC_TareWeightInfo.ValueChanged += UpdateCO2eStatusToNCUWhenContainerWeightChanged;
				container.CO2eStatusChangedEvent += ContainerCO2eStatusChanged;
			}

			Containers.CountChanged += (sender, args) =>
			{
				if (!(args.BizObject is ForwardingContainer container))
				{
					return;
				}

				if (!this.SkipCO2eStatusCheck() && (originalRequireTEU || (this as ICO2eProvider).RequireTEU) && !IsDeleted && !IsDeleting)
				{
					this.UpdateCO2eStatusToNotCurrent(new CO2eStatusChangedReason(args, (NoResString)"Container"), IsCopying);
				}

				if (args.ItemAdded)
				{
					container.JC_ContainerCountInfo.ValueChanged += UpdateCO2eStatusToNCUWhenContainerTEUChanged;
					container.JC_RCInfo.ValueChanged += UpdateCO2eStatusToNCUWhenContainerTEUChanged;
					container.JC_TareWeightInfo.ValueChanged += UpdateCO2eStatusToNCUWhenContainerWeightChanged;
					container.CO2eStatusChangedEvent += ContainerCO2eStatusChanged;
				}
				else if (args.ItemRemoved)
				{
					container.JC_ContainerCountInfo.ValueChanged -= UpdateCO2eStatusToNCUWhenContainerTEUChanged;
					container.JC_RCInfo.ValueChanged -= UpdateCO2eStatusToNCUWhenContainerTEUChanged;
					container.JC_TareWeightInfo.ValueChanged -= UpdateCO2eStatusToNCUWhenContainerWeightChanged;
					container.CO2eStatusChangedEvent -= ContainerCO2eStatusChanged;
				}
			};
		}

		#endregion

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

		#endregion

		#region Shipments

		#region GridShipments

		public new TopLevelForwardingShipmentCollection GridShipments
		{
			get { return (TopLevelForwardingShipmentCollection)base.GridShipments; }
		}

		protected override TopLevelShipmentCollection GetNewTopLevelShipmentCollection()
		{
			return JK_AgentType == Constants.AgentType.AWBMaster
				? new TopLevelForwardingShipmentCollection(ColoadConsolsShipments, this)
				: new TopLevelForwardingShipmentCollection(Shipments, this);
		}

		#endregion

		#region Shipments

		[ChildEditable(false)]
		public new ForwardingConsolShipmentCollection Shipments
		{
			get { return (ForwardingConsolShipmentCollection)base.Shipments; }
		}

		protected override ConsolShipmentCollection GetNewConsolShipmentCollection()
		{
			return new ForwardingConsolShipmentCollection(this);
		}

		#endregion

		#region Shipment_List

		protected override ModuleShipmentCollection GetModuleShipmentCollection()
		{
			return ShipmentDomainService.GetInstance(Factory).ModuleShipmentCollection;
		}

		#endregion

		protected override void AfterConsolShipmentCollectionCreated()
		{
			base.AfterConsolShipmentCollectionCreated();

			if (JK_ConsolCutOffDate.IsValid && JK_ConsolCutOffDate < Env.Time.CurrentUtcDateTime)
			{
				base.Shipments.AllowAddNew = !IsInDatabase || Env.Security.ConsolAttachDetachShipmentAfterCutOffDate.IsAllowed;
			}
			else
			{
				base.Shipments.AllowAddNew = true;
			}

			Shipments.CountChanged += new CollectionCountChangedEventHandler(OnShipments_CountChanged);
			Shipments.Cast<ForwardingShipment>().ForEach(shipment => RegisterInspectionCodeChangeTracking(shipment.JS_InspectionTypeCodeInfo));
			Shipments.Cast<ForwardingShipment>().ForEach(shipment => RegisterShipmentChangeTracking(shipment));
		}

		public void TriggerShipmentsCountChanged(CollectionCountChangedEventArgs e)
		{
			OnShipments_CountChanged(Shipments, e);
		}

		void OnShipments_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			var shipment = e.BizObject as ForwardingShipment;

			if (shipment != null)
			{
				if (e.ItemAdded)
				{
					if (ShipmentsDetachedThisSession.ContainsKey(shipment.PK))
					{
						ShipmentsDetachedThisSession.Remove(shipment.PK);
					}
					else if (!ShipmentsAttachedThisSession.ContainsKey(shipment.PK))
					{
						ShipmentsAttachedThisSession.Add(shipment.PK, ZDateTime.UtcNow);
					}

					RegisterShipmentChangeTracking(shipment);
					RegisterInspectionCodeChangeTracking(shipment.JS_InspectionTypeCodeInfo);
				}
				else
				{
					if (ShipmentsAttachedThisSession.ContainsKey(shipment.PK))
					{
						ShipmentsAttachedThisSession.Remove(shipment.PK);
					}
					else if (!ShipmentsDetachedThisSession.ContainsKey(shipment.PK))
					{
						ShipmentsDetachedThisSession.Add(shipment.PK, ZDateTime.UtcNow);
					}

					DeregisterInspectionCodeChangeTracking(shipment.JS_InspectionTypeCodeInfo);
					DeregisterShipmentChangeTracking(shipment);
				}

				if (shipment.IsInDatabase || !shipment.ConsignorPK.IsEmpty)
				{
					foreach (Transport transport in Transports)
					{
						transport.Validation.ValidateJW_IsCargoOnly();
					}
				}

				RefreshSecurityStatusCode();
				RefreshSpecialHandlingItems();
				DeliveryDueDateFactorHasChanged(shipment, DeliveryDueDateChangedFactor.ConsolAttached);
				forceUpdatePreAllocatedAmountExceededStatus = true;

				var freeTextReason = e.ItemAdded ? (NoResString)"Shipment added" : (NoResString)"Shipment removed";
				this.UpdateCO2eStatusToNotCurrent(new CO2eStatusChangedReason(e, (NoResString)"Shipment"), IsCopying);
			}

			Validation.ValidatePreAllocationValues();
			Validation.ValidateJK_ConsolCutOffDate();
			ValidateAllUNDGsOfAllPackLinesOfAllShipments();
		}

		void RegisterShipmentChangeTracking(ForwardingShipment shipment)
		{
			shipment.OnSavingShipment -= ShipmentSavingTracking;
			shipment.OnSavingShipment += ShipmentSavingTracking;
		}

		void DeregisterShipmentChangeTracking(ForwardingShipment shipment)
		{
			shipment.OnSavingShipment -= ShipmentSavingTracking;
		}

		void ShipmentSavingTracking(object sender, EventArgs e)
		{
			var shipment = sender as ForwardingShipment;

			if (shipment != null
				&& shipment.HasChanges
				&& shipment.IsInDatabase
				&& shipment.OuterPackLines.Any(line => line.HasChanges)
				&& shipment.OuterPackLines.Cast<ForwardingPackLine>().Any(line => ((IBusinessObjectState)line.UNDGs).HasChanges))
			{
				RefreshSpecialHandlingItems();
			}
		}

		void ValidateAllUNDGsOfAllPackLinesOfAllShipments()
		{
			foreach (var shipment in Shipments.OfType<ForwardingShipment>())
			{
				shipment.ValidateAllUNDGsOfAllPacklinesAndRefreshFirstItemBindings();
			}
		}

		void RegisterInspectionCodeChangeTracking(ZPropertyInfo inspectionCodeInfo)
		{
			DeregisterInspectionCodeChangeTracking(inspectionCodeInfo);
			inspectionCodeInfo.ValueChanged += TriggerSecurityStatusCodeRefreshOnValueChanged;
		}

		void DeregisterInspectionCodeChangeTracking(ZPropertyInfo inspectionCodeInfo)
		{
			inspectionCodeInfo.ValueChanged -= TriggerSecurityStatusCodeRefreshOnValueChanged;
		}

		EventHandler TriggerSecurityStatusCodeRefreshOnValueChanged
		{
			get => fTriggerSecurityStatusCodeRefresh ?? (fTriggerSecurityStatusCodeRefresh = new EventHandler((sender, e) => { RefreshSecurityStatusCode(); }));
		}

		EventHandler fTriggerSecurityStatusCodeRefresh;

		public bool HasUpdatedAssemblyMasterAsDirectMaster()
		{
			var assemblyMasterShipment = Shipments.Cast<ForwardingShipment>().FirstOrDefault(s => s.IsAssemblyMaster);
			if (assemblyMasterShipment != null)
			{
				return IsDirect
					&& (!IsInDatabase || JK_AgentTypeInfo.HasChanges
					|| !assemblyMasterShipment.IsInDatabase || assemblyMasterShipment.JS_ShipmentTypeInfo.HasChanges
					|| IsShipmentAttachedThisSession(assemblyMasterShipment.PK));
			}

			return false;
		}

		bool IsShipmentAttachedThisSession(ZGuid shipmentPK)
		{
			return ShipmentsAttachedThisSession.ContainsKey(shipmentPK);
		}

		internal Dictionary<ZGuid, ZDateTime> ShipmentsAttachedThisSession
		{
			get { return shipmentsAttachedThisSession; }
		}
		readonly Dictionary<ZGuid, ZDateTime> shipmentsAttachedThisSession = new Dictionary<ZGuid, ZDateTime>();

		internal Dictionary<ZGuid, ZDateTime> ShipmentsDetachedThisSession
		{
			get { return shipmentsDetachedThisSession; }
		}
		readonly Dictionary<ZGuid, ZDateTime> shipmentsDetachedThisSession = new Dictionary<ZGuid, ZDateTime>();

		#endregion

		#region Sea Cargo OceanBill / Containers

		public CA.ICusCAeMHMaster CusCAeMHMaster
		{
			get
			{
				if (cusCAeMHMaster == null)
				{
					var query = new ZQuery(CusCAeMHMasterSchema.BP_ParentID, PK);
					query.FetchOnlyFromLocalCache = !this.IsInDatabase;
					cusCAeMHMaster = Factory.LoadTop1<CA.ICusCAeMHMaster>(query);
				}
				return cusCAeMHMaster;
			}
		}
		CA.ICusCAeMHMaster cusCAeMHMaster;

		BusinessObject SeaCargoOceanBill
		{
			get
			{
				var seaCargoQuery = new ZQuery(CusSCAOceanBillSchema.CB_ParentId, PK);
				seaCargoQuery.AddToFilter(CusSCAOceanBillSchema.CB_ParentTableCode, JobConsolSchema.Constants.Prefix);
				seaCargoQuery.FetchOnlyFromLocalCache = !IsInDatabase;
				return (BusinessObject)Factory.LoadTop1<Shared.IBaseCusSCAOceanBill>(seaCargoQuery);
			}
		}

		BusinessObject[] SeaCargoContainers
		{
			get { return (BusinessObject[])Factory.Load<Shared.IBaseCusSCAContainer>(new ZQuery(CusSCAContainerSchema.CN_CB, SeaCargoOceanBill.PK)); }
		}

		BusinessObject[] SeaCargoHouses
		{
			get { return (BusinessObject[])Factory.Load<Shared.IBaseCusSCAHouse>(new ZQuery(CusSCAHouseSchema.CA_CB, SeaCargoOceanBill.PK)); }
		}

		#endregion

		#region Shipment_List

		protected override void SetFiltersOnModuleShipmentCollection(ModuleShipmentCollection collection)
		{
			ForwardingShipmentDefaultFilterProvider provider = new ForwardingShipmentDefaultFilterProvider();
			provider.TransportMode = JK_TransportMode;
			provider.ContainerMode = Enterprise.Freight.Business.FreightUtilities.ShipmentContainerMode(JK_ConsolMode, JK_TransportMode);
			provider.OriginPort = JK_RL_NKLoadPort;
			provider.DestinationPort = JK_RL_NKDischargePort;
			provider.ETDTo = Transports.DepartureTransport.JW_ETD;
			provider.ETAFrom = Transports.ArrivalTransport.JW_ETA;
			provider.SetDefaultFilters(collection);
		}

		#endregion

		#region Related Packlines

		protected override UnAllocatedPackLinesView CreateUnAllocatedPackLines()
		{
			return new ForwardingUnallocatedPackLinesView(this, RelatedPackLines);
		}

		protected override IEnumerable<CommonShipment> GetShipmentsWithRelatedPacklines()
		{
			if (IsMultiAWBMaster)
			{
				return ColoadConsols.SelectMany(coloadConsol => coloadConsol.Shipments.Cast<CommonShipment>());
			}

			return base.GetShipmentsWithRelatedPacklines();
		}

		#endregion

		#region Coload Consols

		#region Calculated Properties

		#region JK_Calc_TotalColoadConsolQuantity

		public ZInt JK_Calc_TotalColoadConsolQuantity
		{
			get { return (int)TotalCalculation.GetTotal(ColoadConsols, nameof(JK_TotalShipmentQuantity)); }
		}

		public ZPropertyInfo JK_Calc_TotalColoadConsolQuantityInfo
		{
			get { return GetZPropertyInfo(nameof(JK_Calc_TotalColoadConsolQuantity)); }
		}

		#endregion

		#region JK_Calc_TotalColoadConsolWeight

		[DecimalPlaces(1)]
		public ZDecimal JK_Calc_TotalColoadConsolWeight
		{
			get
			{
				return TotalCalculation.GetTotalWeight(ColoadConsols, nameof(JK_TotalShipmentWeight), nameof(JK_TotalShipmentWeightUnit), JK_Calc_TotalColoadConsolWeightUnit);
			}
		}

		public ZPropertyInfo JK_Calc_TotalColoadConsolWeightInfo
		{
			get { return GetZPropertyInfo(nameof(JK_Calc_TotalColoadConsolWeight)); }
		}

		#endregion

		#region JK_Calc_TotalColoadConsolWeightUnit

		public ZString JK_Calc_TotalColoadConsolWeightUnit
		{
			get { return ColoadConsols.Any(x => x.JK_TotalShipmentWeightUnit == Constants.Weight.Pounds) ? Constants.Weight.Pounds : Constants.Weight.Kilograms; }
		}

		public ZPropertyInfo JK_Calc_TotalColoadConsolWeightUnitInfo
		{
			get { return GetZPropertyInfo(nameof(JK_Calc_TotalColoadConsolWeightUnit)); }
		}

		#endregion

		#region JK_Calc_TotalColoadConsolVolume

		[DecimalPlaces(3)]
		public ZDecimal JK_Calc_TotalColoadConsolVolume
		{
			get
			{
				return TotalCalculation.GetTotalVolume(ColoadConsols, nameof(JK_TotalShipmentVolume), nameof(JK_TotalShipmentVolumeUnit), JK_Calc_TotalColoadConsolVolumeUnit);
			}
		}

		public ZPropertyInfo JK_Calc_TotalColoadConsolVolumeInfo
		{
			get { return GetZPropertyInfo(nameof(JK_Calc_TotalColoadConsolVolume)); }
		}

		#endregion

		#region JK_Calc_TotalColoadConsolVolumeUnit

		public ZString JK_Calc_TotalColoadConsolVolumeUnit
		{
			get { return ColoadConsols.Any(x => x.JK_TotalShipmentVolumeUnit == Constants.Volume.CubicFeet) ? Constants.Volume.CubicFeet : Constants.Volume.CubicMetres; }
		}

		public ZPropertyInfo JK_Calc_TotalColoadConsolVolumeUnitInfo
		{
			get { return GetZPropertyInfo(nameof(JK_Calc_TotalColoadConsolVolumeUnit)); }
		}

		#endregion

		#region JK_Calc_TotalColoadConsolChargeable

		[DecimalPlaces(1)]
		public ZDecimal JK_Calc_TotalColoadConsolChargeable
		{
			get
			{
				if (Constants.Volume.ContainsCode(JK_Calc_TotalShipmentChargeableUnit))
				{
					return TotalCalculation.GetTotalVolume(ColoadConsols, nameof(JK_TotalShipmentChargeable), nameof(JK_Calc_TotalShipmentChargeableUnit), JK_Calc_TotalColoadConsolChargeableUnit);
				}
				return TotalCalculation.GetTotalWeight(ColoadConsols, nameof(JK_TotalShipmentChargeable), nameof(JK_Calc_TotalShipmentChargeableUnit), JK_Calc_TotalColoadConsolChargeableUnit);
			}
		}

		public ZPropertyInfo JK_Calc_TotalColoadConsolChargeableInfo
		{
			get { return GetZPropertyInfo(nameof(JK_Calc_TotalColoadConsolChargeable)); }
		}

		#endregion

		#region JK_Calc_TotalColoadConsolChargeableUnit

		public ZString JK_Calc_TotalColoadConsolChargeableUnit
		{
			get { return ChargeableAmountCalculator.GetChargeableUnit(JK_TransportMode, JK_Calc_TotalColoadConsolWeightUnit, JK_Calc_TotalColoadConsolVolumeUnit); }
		}

		public ZPropertyInfo JK_Calc_TotalColoadConsolChargeableUnitInfo
		{
			get { return GetZPropertyInfo(nameof(JK_Calc_TotalColoadConsolChargeableUnit)); }
		}

		#endregion

		#region JK_Calc_TotalPrepaidColoadConsolChargeableAmount

		[DecimalPlaces(2)]
		public ZDecimal JK_Calc_TotalPrepaidColoadConsolChargeableAmount
		{
			get { return TotalCalculation.GetTotal(ColoadConsols, nameof(JK_TotalPrepaidShipmentChargeableAmount)); }
		}

		public ZPropertyInfo JK_Calc_TotalPrepaidColoadConsolChargeableAmountInfo
		{
			get { return GetZPropertyInfo(nameof(JK_Calc_TotalPrepaidColoadConsolChargeableAmount)); }
		}

		public ZString JK_Calc_TotalPrepaidColoadConsolChargeableAmountCurrencyCode
		{
			get { return JK_TotalPrepaidShipmentChargeableAmountCurrencyCode; }
		}

		public ZPropertyInfo JK_Calc_TotalPrepaidColoadConsolChargeableAmountCurrencyCodeInfo
		{
			get { return GetZPropertyInfo(nameof(JK_Calc_TotalPrepaidColoadConsolChargeableAmountCurrencyCode)); }
		}

		#endregion

		#region JK_Calc_TotalCollectColoadConsolChargeableAmount

		[DecimalPlaces(2)]
		public ZDecimal JK_Calc_TotalCollectColoadConsolChargeableAmount
		{
			get { return TotalCalculation.GetTotal(ColoadConsols, nameof(JK_TotalCollectShipmentChargeableAmount)); }
		}

		public ZPropertyInfo JK_Calc_TotalCollectColoadConsolChargeableAmountInfo
		{
			get { return GetZPropertyInfo(nameof(JK_Calc_TotalCollectColoadConsolChargeableAmount)); }
		}

		public ZString JK_TotalCollectColoadConsolChargeableAmountCurrencyCode
		{
			get { return JK_TotalCollectShipmentChargeableAmountCurrencyCode; }
		}

		public ZPropertyInfo JK_TotalCollectColoadConsolChargeableAmountCurrencyCodeInfo
		{
			get { return GetZPropertyInfo(nameof(JK_TotalCollectColoadConsolChargeableAmountCurrencyCode)); }
		}

		#endregion

		public ZInt ColoadConsolCount
		{
			get { return ColoadConsols.Count; }
		}

		#endregion

		public new ForwardingConsol MasterConsol
		{
			get { return Factory.Load<ForwardingConsol>(JK_JK_MasterConsol); }
		}

		[ChildEditable(true)]
		public ColoadConsolCollection ColoadConsols
		{
			get
			{
				if (coloadConsols == null)
				{
					coloadConsols = new ColoadConsolCollection(this);
					RegisterEditableChildObject(coloadConsols);

					coloadConsols.CollectionCountChange += ColoadConsols_CollectionCountChange;
				}

				return coloadConsols;
			}
		}
		ColoadConsolCollection coloadConsols;

		internal Dictionary<ZGuid, ZDateTime> ConsolsAttachedThisSession => consolsAttachedThisSession;

		readonly Dictionary<ZGuid, ZDateTime> consolsAttachedThisSession = new Dictionary<ZGuid, ZDateTime>();

		internal Dictionary<ZGuid, ZDateTime> ConsolsDetachedThisSession => consolsDetachedThisSession;

		readonly Dictionary<ZGuid, ZDateTime> consolsDetachedThisSession = new Dictionary<ZGuid, ZDateTime>();

		void ColoadConsols_CollectionCountChange(object sender, CollectionCountChangedEventArgs e)
		{
			var coloadConsol = e.BizObject as ForwardingConsol;
			if (coloadConsol != null)
			{
				if (e.ItemAdded)
				{
					if (ConsolsDetachedThisSession.ContainsKey(coloadConsol.PK))
					{
						ConsolsDetachedThisSession.Remove(coloadConsol.PK);
					}

					if (!ConsolsAttachedThisSession.ContainsKey(coloadConsol.PK))
					{
						ConsolsAttachedThisSession.Add(coloadConsol.PK, ZDateTime.UtcNow);
					}
				}
				else
				{
					if (ConsolsAttachedThisSession.ContainsKey(coloadConsol.PK))
					{
						ConsolsAttachedThisSession.Remove(coloadConsol.PK);
					}

					if (!ConsolsDetachedThisSession.ContainsKey(coloadConsol.PK))
					{
						ConsolsDetachedThisSession.Add(coloadConsol.PK, ZDateTime.UtcNow);
					}
				}

				foreach (CommonShipment shipment in coloadConsol.Shipments)
				{
					if (e.ItemAdded)
					{
						RefreshRelatedPackLinesWhenShipmentIsAdded(shipment, false);
					}
					else
					{
						RefreshRelatedPackLinesWhenShipmentIsRemoved(shipment);
						ConsolShipmentRelationshipHelper.UnpackShipment(this, shipment);
					}
				}

				RebuildUnAllocatedPackLines();

				Validation.ValidatePreAllocationValues();
				Validation.ValidateJK_ConsolCutOffDate();
			}
		}

		public ColoadConsolCollection ColoadConsols_List
		{
			get
			{
				var collection = ColoadConsolCollection.GetColoadConsolsList(Factory, GetFreightNumber(), JK_ConsolCutOffDate);

				var filterProvider = new ForwardingConsolDefaultFilterProvider();
				filterProvider.TransportMode = JK_TransportMode;
				filterProvider.ConsolType = Constants.AgentType.AWBCoload;
				filterProvider.LoadPort = JK_RL_NKLoadPort;
				filterProvider.DischargePort = JK_RL_NKDischargePort;
				filterProvider.ETDFrom = JK_JX_JA_E_DEP;
				filterProvider.ETATo = JK_JX_JB_E_ARV;
				filterProvider.SetDefaultFilters(collection);

				return collection;
			}
		}

		ZString GetFreightNumber()
		{
			var freightNumber = ZString.Empty;
			var transport = MostInterestingTransportForBinding.FirstOrDefault() as Transport;

			if (transport != null)
			{
				freightNumber = transport.JW_VoyageFlight;
			}

			return freightNumber;
		}

		public ColoadConsolsShipmentsCollection ColoadConsolsShipments
		{
			get { return coloadConsolsShipments ?? (coloadConsolsShipments = new ColoadConsolsShipmentsCollection(this)); }
		}
		ColoadConsolsShipmentsCollection coloadConsolsShipments;

		public override bool IsLinkedTo(CommonShipment shipment)
		{
			if (shipment != null && JK_AgentType == Constants.AgentType.AWBMaster)
			{
				return ColoadConsols.Any(x => shipment.Consols.Contains(x));
			}

			return base.IsLinkedTo(shipment);
		}

		protected override ShipmentsForTotallingCollection GetNewShipmentsForTotallingCollection()
		{
			if (JK_AgentType == Constants.AgentType.AWBMaster)
			{
				return new ShipmentsForTotallingCollection(ColoadConsolsShipments, this);
			}

			return base.GetNewShipmentsForTotallingCollection();
		}

		#endregion

		#region AMS

		public US.USAMS.ICusInBondHeader USAMS
		{
			get
			{
				if (fUSAMS == null)
				{
					var query = new ZQuery(CusInBondHeaderSchema.BH_ParentID, PK);
					query.AddToFilter(CusInBondHeaderSchema.BH_ApplicationCode, Customs.Common.CusInBondApplicationCodeList.Codes.AMS);
					query.FetchOnlyFromLocalCache = !IsInDatabase;
					fUSAMS = Factory.LoadTop1<US.USAMS.ICusInBondHeader>(query);
				}
				return fUSAMS;
			}
		}
		US.USAMS.ICusInBondHeader fUSAMS;

		#endregion

		#endregion

		#region Iceland Specific

		public void AutoFillCRN(Transport currentTransport)
		{
			if (GlbBranch.CurrentBranch?.Country != null
				&& GlbBranch.CurrentBranch.Country.Code == Core.Constants.CountryCodes.Iceland
				&& !(SendingarnumerHelper.Sendingarnumer.IsAir && SendingarnumerHelper.Sendingarnumer.IsImport)
				&& currentTransport == FirstForExportLastForImport)
			{
				AutoFillCarrierCode();
				AutoFillFlightNumber();
				AutoFillArrivalDepartureDate();
				AutoFillPortOfLoading();

				SendingarnumerHelper.RefreshCRN();
			}
		}

		void AutoFillCarrierCode()
		{
			if (ShippingLine != null)
			{
				ZQuery carrierCodeFilter = new ZQuery();
				carrierCodeFilter.AddToFilter(OrgCusCodeSchema.OK_RN_NKCodeCountry, Core.Constants.CountryCodes.Iceland);
				carrierCodeFilter.AddToFilter(OrgCusCodeSchema.OK_CodeType, OrgCusCode.CodeTypes.CarrierCode);

				if (ShippingLine.CustomsCodes.Find(carrierCodeFilter).Length > 0)
				{
					SendingarnumerHelper.Sendingarnumer.CarrierCode = ((OrgCusCode)ShippingLine.CustomsCodes.Find(carrierCodeFilter)[0]).OK_CustomsRegNo;
				}
			}
		}

		void AutoFillPortOfLoading()
		{
			if (FirstForExportLastForImport != null)
			{
				ZString portOfLoadingAsString = FirstForExportLastForImport.JW_RL_NKLoadPort;
				SendingarnumerHelper.Sendingarnumer.PortOfLoadingCountryCode = portOfLoadingAsString.SubstringSafe(0, 2);
				SendingarnumerHelper.Sendingarnumer.PortOfLoadingPortCode = portOfLoadingAsString.SubstringSafe(2, 3);
			}
		}

		void AutoFillArrivalDepartureDate()
		{
			if (FirstForExportLastForImport != null)
			{
				if (this.IsImport())
				{
					if (FirstForExportLastForImport.JW_ATA.IsValid)
					{
						SendingarnumerHelper.Sendingarnumer.ArrivalDepartureDate = FirstForExportLastForImport.JW_ATA;
					}
					else if (FirstForExportLastForImport.JW_ETA.IsValid)
					{
						SendingarnumerHelper.Sendingarnumer.ArrivalDepartureDate = FirstForExportLastForImport.JW_ETA;
					}
				}
				else if (this.IsExport())
				{
					if (FirstForExportLastForImport.JW_ATD.IsValid)
					{
						SendingarnumerHelper.Sendingarnumer.ArrivalDepartureDate = FirstForExportLastForImport.JW_ATD;
					}
					else if (FirstForExportLastForImport.JW_ETD.IsValid)
					{
						SendingarnumerHelper.Sendingarnumer.ArrivalDepartureDate = FirstForExportLastForImport.JW_ETD;
					}
				}
			}
		}

		void AutoFillFlightNumber()
		{
			if (FirstForExportLastForImport != null)
			{
				if (IsAir)
				{
					SendingarnumerHelper.Sendingarnumer.VesselCodeFlightNumber = FirstForExportLastForImport.JW_VoyageFlight.SubstringSafe(2, 3);
				}
				else if (IsSea && FirstForExportLastForImport.Vessel != null)
				{
					SendingarnumerHelper.Sendingarnumer.VesselCodeFlightNumber = FirstForExportLastForImport.Vessel.RV_CarrierCode;
				}
			}
		}

		Transport FirstForExportLastForImport
		{
			get
			{
				Transport result = null;
				if (this.IsImport())
				{
					result = Transports.ArrivalTransport;
				}
				else if (this.IsExport())
				{
					result = Transports.DepartureTransport;
				}
				return result;
			}
		}

		#endregion

		#region UAE Specific

		public ZString UAEInstalmentNumber
		{
			get
			{
				var consol = Numbers.Find(num => num.CE_EntryType == UnitedArabEmiratesAdditionalReferenceNumberTypes.Codes.UAEInstalmentNumber).FirstOrDefault();
				return (consol != null) ? consol.CE_EntryNum : ZString.Empty;
			}
		}

		#endregion

		#region CarrierShipperReferenceWithFallback

		public ZString CarrierShipperReferenceWithFallback
		{
			get
			{
				var csrNumber = ConsolCarrierShipperReferenceNumberCalculator.GetCarrierShipperReferenceNumber(this);
				return csrNumber.IsEmpty ? JK_UniqueConsignRef : csrNumber;
			}
		}

		#endregion

		#region Is Cargo Only

		public ZBool JK_Calc_IsCargoOnly
		{
			get
			{
				var airTransports = Transports.Cast<Transport>().Where(x => x.TransportMode == Constants.TransportModes.Air);
				return airTransports.Any() && airTransports.All(x => x.JW_IsCargoOnly);
			}
		}

		public ZPropertyInfo JK_Calc_IsCargoOnlyInfo
		{
			get { return GetZPropertyInfo(Schema.JK_Calc_IsCargoOnly); }
		}

		#endregion

		#region Air Booking Status

		public ZString JK_Calc_AirBookingStatus
		{
			get
			{
				if (JK_TransportMode != Core.Constants.TransportModes.Air)
				{
					return ZString.Empty;
				}

				var airTransports = Transports.Cast<Transport>().Where(transport => transport.JW_TransportMode == Core.Constants.TransportModes.Air);
				return string.Join("-", airTransports.Select(transport => transport.JW_Status)); // concat string
			}
		}

		public ZPropertyInfo JK_Calc_AirBookingStatusInfo
		{
			get { return GetZPropertyInfo(Schema.JK_Calc_AirBookingStatus); }
		}

		#endregion

		#region Container Summaries

		[ResourceStringData("c47ba697-f5bb-4f85-95df-b12b581e2267", ShortCaption = "Cont. Types", Caption = "Container Types Summary")]
		public ZString JK_Calc_ContainerTypesSummary
		{
			get
			{
				var dictionary = GetSortedDictionaryForRefContainerProperty(refContainer => refContainer.RC_Code);
				return GetSummaryOfDictionary(dictionary);
			}
		}

		[ResourceStringData("50172553-59c2-4af7-a17a-5e162a64af5d", ShortCaption = "Cont. Classes", Caption = "Container Storage Classes Summary")]
		public ZString JK_Calc_ContainerStorageClassesSummary
		{
			get
			{
				var dictionary = GetSortedDictionaryForRefContainerProperty(refContainer => refContainer.RC_Calc_StorageClass);
				return GetSummaryOfDictionary(dictionary);
			}
		}

		SortedDictionary<ZString, int> GetSortedDictionaryForRefContainerProperty(GetRefContainerProperty getProperty)
		{
			var dictionary = new SortedDictionary<ZString, int>();
			foreach (ForwardingContainer container in Containers)
			{
				if (container.Container != null)
				{
					var propertyValue = getProperty(container.Container);
					dictionary.TryGetValue(propertyValue, out int count);
					dictionary[propertyValue] = count + container.JC_ContainerCount;
				}
			}

			return dictionary;
		}

		delegate ZString GetRefContainerProperty(RefContainer container);

		ZString GetSummaryOfDictionary(SortedDictionary<ZString, int> dictionary)
		{
			return string.Join(", ", dictionary
				.Where(kvp => kvp.Key != ZString.Empty)
				.Select(kvp => kvp.Value + (NoResString)"x" + kvp.Key)); // Calculated string
		}

		#endregion

		#region ShouldCheckCreditOnHold

		protected override bool ShouldCheckCreditOnHold(ZString organizationType)
		{
			return this.IsCreditLimitCheckRequired(organizationType);
		}

		#endregion

		#region ForwardAirBillStmNums

		public ViewStmNums ForwardAirBillStmNums
		{
			get
			{
				return ShippingLine != null
					? ShippingLine.OrgFountains.FirstOrDefault(c => c.SN_Type == OrgConstants.NumberFountains.Code.ForwardAirBillNumbers)
					: null;
			}
		}

		#endregion

		#region Clone

		protected override IEnumerable<string> GetPropertiesToExcludeFromCloning()
		{
			List<string> result = new List<string>(base.GetPropertiesToExcludeFromCloning());
			result.Add(JobConsolSchema.JK_OverrideWaybillDefaults.Name);
			result.Add(JobConsolSchema.JK_MasterBillNum.Name);
			result.Add(JobConsolSchema.JK_CustomsReference.Name);
			return result;
		}

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			var consol = (ForwardingConsol)base.CloneInternal(args);
			consol.MasterBillAirlinePrefix = MasterBillAirlinePrefix;
			consol.JK_IsNeutralMaster = JK_IsNeutralMaster;
			consol.WeightVerificationUnit = WeightVerificationUnit;
			consol.VolumeVerificationUnit = VolumeVerificationUnit;
			CloneAWBSpecialHandlingItems(consol);
			this.CopyJobCO2eTo(consol);

			var securityStatusCode = SecurityStatusCode;
			if (!securityStatusCode.IsEmpty)
			{
				consol.SecurityStatusCode = securityStatusCode;
			}

			var argsForCollections = new BusinessObjectCloneArgs(args.AlternativeFactoryToInstantiateCloneIn, Enumerable.Empty<string>(), null, true);

			CopyDocAddressesToClonedConsol(consol, argsForCollections);

			return consol;
		}

		void CopyDocAddressesToClonedConsol(ForwardingConsol clonedConsol, BusinessObjectCloneArgs argsForCollections)
		{
			clonedConsol.DocAddresses.RemoveAndDeleteAll();

			foreach (JobDocAddress address in DocAddresses.ToArray())
			{
				clonedConsol.DocAddresses.Add(address.Clone(argsForCollections));
			}
		}

		void CloneAWBSpecialHandlingItems(ForwardingConsol consol)
		{
			consol.AWBSpecialHandlingItems.DeleteAll();
			var items = AWBSpecialHandlingItems.Cast<NonSecurityJobConsolAWBSpecialHandling>().Select(item => item.JKH_Code);
			foreach (var item in items)
			{
				var specialHandlingItem = consol.AWBSpecialHandlingItems.AddNew();
				specialHandlingItem.JKH_Code = item;
			}
		}

		#endregion

		#region Template Copy

		public override CommonConsol TemplateCopy(ZBool copyShipments, ZBool copySecondaryTransports, ZBool includeDatesAndPorts = default, BusinessObjectFactory alternativeFactory = null)
		{
			var clonedConsol = base.TemplateCopy(copyShipments, copySecondaryTransports, includeDatesAndPorts, alternativeFactory) as ForwardingConsol;

			using (clonedConsol.GetValidationSuspender())
			{
				CopyDGRestrictionsToClonedConsol(clonedConsol, alternativeFactory);
			}

			return clonedConsol;
		}

		void CopyDGRestrictionsToClonedConsol(ForwardingConsol clonedConsol, BusinessObjectFactory alternativeFactory)
		{
			var cloneArgs = new BusinessObjectCloneArgs(alternativeFactory, Array.Empty<string>(), typeof(ConsolDGRestrictions), false);
			foreach (var restriction in ConsolDGRestrictionCollection)
			{
				var clonedRestriction = (ConsolDGRestrictions)restriction.Clone(cloneArgs);
				clonedConsol.ConsolDGRestrictionCollection.Add(clonedRestriction);
			}
		}

		protected override void CopyNotesToClonedConsol(CommonConsol clonedConsol, BusinessObjectFactory alternativeFactory)
		{
			clonedConsol.Notes.SuspendValidation();
			try
			{
				var cloneArgs = new BusinessObjectCloneArgs(alternativeFactory, Array.Empty<string>(), typeof(ForwardingConsolStmNote), false);
				foreach (var note in Notes.GetAllNotes().Cast<ForwardingConsolStmNote>().Where(n => n.ST_Description != PredefinedNoteTypes.Instance.OriginalBillNotes.Description))
				{
					clonedConsol.Notes.Add(note.Clone(cloneArgs));
				}
			}
			finally
			{
				clonedConsol.Notes.ResumeValidation();
			}
		}

		#endregion

		#region IDocAddresses Members

		protected override bool CanDeleteAddress(JobDocAddress docAddress)
		{
			return (!IsICS2 || !IsSelfFiler) && docAddress.DocAddressType == DocAddressType.SelfFiler;
		}

		#endregion

		#region ITransportParent Members

		TransportSupporter ITransportParent.TransportSupporter
		{
			get { return new ForwardingConsolTransportSupporter<ForwardingConsol>(this); }
		}

		#endregion

		#region IDocumentSupportable Members

		#region UpdatePrinted

		public event EventHandler OnPrintFinalMaster;

		public void UpdateAWBPrinted()
		{
			UpdateAWBPrintedCore();
			if (OnPrintFinalMaster != null)
			{
				OnPrintFinalMaster(this, EventArgs.Empty);
			}
		}

		protected virtual void UpdateAWBPrintedCore()
		{
			if (!IsAWBHeaderAccessible)
			{
				return;
			}

			var log = Logs.AddNew(Events.DocumentSent, MAWBPrintedLogReference);

			AWBHeader.EH_FinalizationDate = log.SL_EventTime;

			if (JK_IsNeutralMaster)
			{
				JobMawb mawb = GetAllocatedMAWB(Factory);
				if (mawb != null)
				{
					mawb.JM_IsPrinted = ZBool.True;
				}

				JK_IsNeutralMasterInfo.RefreshBinding();
				MasterBillAirlinePrefixInfo.RefreshBinding();
				MAWBLabelDescInfo.RefreshBinding();
			}

			if (JK_MasterBillIssueDate.IsEmpty)
			{
				JK_MasterBillIssueDate = GetAWBIssueDate();
			}

			AWBHeader.ForceSavingByFactory = !JK_OverrideWaybillDefaults;
			try
			{
				ZExceptionReporting.ProcessWithSaveExceptionHandling(Factory.Save, null, true);
			}
			finally
			{
				AWBHeader.ForceSavingByFactory = false;
			}
		}

		internal JobMawb GetAllocatedMAWB(BusinessObjectFactory awbFactory)
		{
			ZQuery filter = new ZQuery(JobMawbSchema.JM_ParentTableCode, JobConsolSchema.Constants.Prefix);
			filter.AddToFilter(JoinCondition.And, JobMawbSchema.JM_ParentID, SQLComparisonOperator.Equal, PK);

			return awbFactory.LoadTop1<JobMawb>(filter);
		}

		#region AWB IssueDate

		public virtual ZDateTime GetAWBIssueDate()
		{
			if (FreightConfigurationRegistry.Instance.AWBIssueDate.Value == FreightConfigurationRegistry.Instance.AWBIssueDateIsCutOffDate)
			{
				Transport transport = GetTransportByPlanningType(Constants.TransportPlanningType.Flight1);
				if (transport != null)
				{
					switch (JK_ConsolMode)
					{
						case Core.Constants.ContainerModes.ULD:
						case Core.Constants.ContainerModes.BuyersConsol:
							return transport.JW_TerminalCutOff.IsEmpty ? ZDateTime.Now : transport.JW_TerminalCutOff;

						default:
							return transport.JW_DepotCutOff.IsEmpty ? ZDateTime.Now : transport.JW_DepotCutOff;
					}
				}
			}

			return IsAir ? ZDateTime.Now : ZDateTime.Today;
		}

		#endregion

		protected bool IsNeutralMasterAndAllocatedMAWBPrinted
		{
			get
			{
				bool result = JK_IsNeutralMaster;

				if (result)
				{
					JobMawb jobMawb = GetAllocatedMAWB(new BusinessObjectFactory());
					result = jobMawb != null ? jobMawb.JM_IsPrinted : ZBool.False;
				}

				return result;
			}
		}

		#endregion

		#region DocumentSupporter

		public override DocumentSupporter DocumentSupporter
		{
			get { return documentSupporter ?? (documentSupporter = (ForwardingConsolDocumentSupporter)ForwardingConsolDocumentSupporter.New(this)); }
		}
		ForwardingConsolDocumentSupporter documentSupporter;

		#endregion

		#endregion

		#region ICDArchive Members

		public override CDArchiveInfo CDArchiveInfo
		{
			get { return new ForwardingConsolCDArchiveInfo(this); }
		}

		#region ForwardingConsolCDArchiveInfo

		public class ForwardingConsolCDArchiveInfo : ConsolCDArchiveInfo
		{
			public ForwardingConsolCDArchiveInfo(ForwardingConsol consol)
				: base(consol)
			{
			}

			ForwardingConsol Consol
			{
				get { return (ForwardingConsol)BusinessEntity; }
			}

			public override ZString[] InvoiceNumbersList
			{
				get
				{
					AccTransactionHeaderCollection transactions = new InvoiceLoader(Factory).GetInvoicesForUniqueRef(new ZGuid[] { Consol.ReceivingForwarderPK }, Consol.JK_UniqueConsignRef);
					ZString[] headers = new ZString[transactions.Count];

					for (int i = 0; i < transactions.Count; i++)
					{
						headers[i] = ObjectFactory.Get<IAccounting>().UseJobNumberBasedInvoiceNumbers ? transactions[i].AH_ConsolidatedInvoiceRef : transactions[i].AH_TransactionNum;
					}
					return headers;
				}
			}

			public override ZString[] OrderNumbersList
			{
				get
				{
					return GetCombinedList(Consol.Shipments, info =>
					{
						return info.OrderNumbersList;
					});
				}
			}
		}
		#endregion

		#endregion

		#region IDocManagerSupport Members

		public override DocManagerInfo DocManagerInfo
		{
			get { return docManagerInfo ?? (docManagerInfo = new ForwardingConsolDocManagerInfo(this)); }
		}
		DocManagerInfo docManagerInfo;

		#endregion

		#region IJobNumber Members

		string IJobNumber.JobNumber
		{
			get
			{
				return JK_UniqueConsignRef;
			}
		}

		#endregion

		#region IJobHeaderParent Members

		void IJobHeaderParent.OnJobCreating(JobHeader job)
		{
			// tested by IDtbBookingParentTestCase<T>.TestIfJobInvoicingPluginAndCanCreateInvoicingJobThenAddsChargesFromChildPortTransportJob() and TestIfJobInvoicingPluginAndCanCreateInvoicingJobThenAddsChargesFromGrandchildPortTransportJob()
			CartageHelper.AttachCartageJobsToParentJob(job, PK);
			// tested by IDtbBookingParentTestCase<T>.TestIfJobInvoicingPluginAndCanCreateInvoicingJobThenAddsChargesFromGrandchildLandTransportConsignment()
			ConsignmentJobHelper.AttachLTConsignmentJobsToParentJob(job, PK);
		}

		void IJobHeaderParent.OnJobCreated(JobHeader job)
		{
		}

		void IJobHeaderParent.OnJobDeleting(JobHeader job)
		{
		}

		void IJobHeaderParent.OnJobDeleted(JobHeader job)
		{
		}

		void IJobHeaderParent.SetJobNumberFieldOnSaving()
		{
			PopulateJK_UniqueConsignRefIfNeeded();
		}

		public virtual void OnJobCreating(JobHeader job)
		{
		}

		bool IJobHeaderParent.AllowInvoiceDeletion
		{
			get { return true; }
		}

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

		#endregion

		#region IWorkflowProvider Members

		ZString IWorkflowProviderCore.WorkflowType
		{
			get { return JobInvoicingConsumerTypes.Consol.Code; }
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
					workflowItems = this.GetOrCreateProcessTaskCollection(() => new ForwardingConsolProcessTaskCollection(this));
					RegisterEditableChildObject(workflowItems);
				}
				return workflowItems;
			}
		}
		ForwardingConsolProcessTaskCollection workflowItems;

		IColumnValueRanker IWorkflowProviderCore.GetTemplateSelectionCriteria()
		{
			ColumnValueRanker result = new ColumnValueRanker();

			result.Add(ProcessTaskTemplateSchema.P0_SubType1, JK_TransportMode, ZString.Empty);
			result.Add(ProcessTaskTemplateSchema.P0_SubType2, DirectionsHelper<ForwardingConsol>.FromBusinessObject(this), ZString.Empty);
			result.Add(ProcessTaskTemplateSchema.P0_LoadPortCountry, JK_RL_NKLoadPort, JK_RL_NKLoadPort.Substring(0, 2), ZString.Empty);
			result.Add(ProcessTaskTemplateSchema.P0_DischargePortCountry, JK_RL_NKDischargePort, JK_RL_NKDischargePort.Substring(0, 2), ZString.Empty);
			result.Add(ProcessTaskTemplateSchema.P0_OH_Client, ShippingLinePK, ZGuid.Empty);

			return result;
		}

		IEnumerable<IWorkflowProvider> IWorkflowProviderIncludingRelated.RelatedIWorkflowProviders
		{
			get
			{
				var result = new List<IWorkflowProvider>();

				//ForwardingConsol

				var mawb = AUCusMAWB as IWorkflowProvider;
				if (mawb != null)
				{
					result.Add(mawb);
				}

				//CommonConsol

				foreach (Transport transport in Transports)
				{
					var transportWorkflowProvider = transport as IWorkflowProvider; //not sure if any subclasses of Transport are IWorkflowProvider
					if (transportWorkflowProvider != null)
					{
						result.Add(transportWorkflowProvider); //if the answer is 'no' can be taken out
					}

					var voyageWorkflowProvider = transport.Voyage as IWorkflowProvider;
					if (voyageWorkflowProvider != null)
					{
						result.Add(voyageWorkflowProvider);
					}
				}

				result.AddRange(Containers.OfType<IWorkflowProvider>());

				return result;
			}
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
					var companyPks = companies.Where(x => (SendingForwarder != null && SendingForwarder.IsProxyOrg(x))
											|| (ReceivingForwarder != null && ReceivingForwarder.IsProxyOrg(x)))
										.Select(x => x.PK).ToArray();

					workflowInformationProvider = new WorkflowInformationProvider(companyPks);
				}

				workflowInformationProvider.Destination = (DischargePort != null) ? DischargePort.RL_PortName : ZString.Empty;
				workflowInformationProvider.Origin = (LoadPort != null) ? LoadPort.RL_PortName : ZString.Empty;
				workflowInformationProvider.BusinessContext = TrackingConstants.BusinessContext.Consol;

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
				BusinessObject[] shipments = Shipments.ToArray();
				List<IWorkflowProvider> result = new List<IWorkflowProvider>();
				foreach (BusinessObject shipment in shipments)
				{
					IWorkflowProvider workflowProvider = shipment as IWorkflowProvider;
					if (workflowProvider != null)
					{
						result.Add(workflowProvider);
					}
				}
				return result.ToArray();
			}
		}

		#endregion

		#region IHaveRequiredDocuments Members

		OrgHeader IHaveRequiredDocuments.ExportBroker
		{
			get { return null; }
		}

		ZString IHaveRequiredDocuments.HouseBill
		{
			get { return null; }
		}

		ZString IHaveRequiredDocuments.MasterBill
		{
			get { return JK_MasterBillNum; }
		}

		#region RequiredDocuments

		[ChildEditable(true)]
		public JobRequiredDocumentDependentCollection RequiredDocuments
		{
			get
			{
				if (requiredDocuments == null)
				{
					requiredDocuments = new JobRequiredDocumentDependentCollection(this, Factory);
					requiredDocuments.Load();
					RegisterEditableChildObject(requiredDocuments);
				}
				return requiredDocuments;
			}
		}
		JobRequiredDocumentDependentCollection requiredDocuments;

		#endregion

		ZString IHaveRequiredDocuments.TableCode
		{
			get { return TablePrefix; }
		}

		BusinessObject IHaveRequiredDocuments.UltimateDocumentParent
		{
			get { return this; }
		}

		ZString IHaveRequiredDocuments.UniqueConsignRef
		{
			get { return JK_UniqueConsignRef; }
		}

		IReadOnlyList<ZString> IHaveRequiredDocuments.AdditionalRefTypes
		{
			get { return Array.Empty<ZString>(); }
		}

		void IHaveRequiredDocuments.PreLogAllDocumentsReceivedEvents()
		{
			if (!IsInDatabase && !IsDeleted && !requiredDocumentsAdded)
			{
				AddRequiredDocuments();
				requiredDocumentsAdded = true;
			}
		}

		bool requiredDocumentsAdded;

		#endregion

		#region AWBCurrency

		public RefCurrency AWBCurrency
		{
			get
			{
				RefCurrency awbCurrency = GlbCompany.CurrentCompany.Country != null && GlbCompany.CurrentCompany.Country.AirWaybillCurrency != null
																		? GlbCompany.CurrentCompany.Country.AirWaybillCurrency
																		: GlbCompany.CurrentCompany.LocalCurrency;

				if (awbCurrency == null)
				{
					return null;
				}

				return GetExchangeRateFromFreightCostsOrSchedule(awbCurrency.RX_Code) > 0 ? awbCurrency : GlbCompany.CurrentCompany.LocalCurrency;
			}
		}

		#endregion

		#region ICustomLabelsConfigOrgProvider Members

		event EventHandler ICustomLabelsConfigOrgProvider.ConfigOrgChanged
		{
			add { }
			remove { }
		}

		OrgHeader ICustomLabelsConfigOrgProvider.ConfigOrg
		{
			get { return null; }
		}

		#endregion

		#region ICustomLabelsProvider Members

		public class CustomLabelsProvider : ICustomLabelsProvider
		{
			public CustomLabelsProvider(ICustomLabelsConfigOrgProvider configOrgProvider)
			{
				this.fConfigOrgProvider = configOrgProvider;
			}

			public ICustomLabelsConfigOrgProvider ConfigOrgProvider
			{
				get { return fConfigOrgProvider; }
			}

			readonly ICustomLabelsConfigOrgProvider fConfigOrgProvider;

			public CustomLabelInfoList GetCustomFields(OrgHeader configOrg, BusinessObjectFactory factory)
			{
				CustomLabelInfoList result = new CustomLabelInfoList(typeof(ForwardingConsol), configOrg, ResString.GetMultilingualString("b26c48bb-6bc3-45dd-8aeb-f326d9ef467c", "the organization proxy of the current company"), factory);

				result.Add(Constants.CustomLabels.Consol.CustomFlag1, Schema.JK_CustomFlag1, Constants.CustomLabels.Descriptions.CustomFlag(1));
				result.Add(Constants.CustomLabels.Consol.CustomFlag2, Schema.JK_CustomFlag2, Constants.CustomLabels.Descriptions.CustomFlag(2));
				result.Add(Constants.CustomLabels.Consol.CustomDate1, Schema.JK_CustomDate1, Constants.CustomLabels.Descriptions.CustomDate(1));
				result.Add(Constants.CustomLabels.Consol.CustomDate2, Schema.JK_CustomDate2, Constants.CustomLabels.Descriptions.CustomDate(2));

				result.Add(Constants.CustomLabels.Consol.CustomString1, Schema.JK_CustomString1, Constants.CustomLabels.Descriptions.CustomText(1));
				result.Add(Constants.CustomLabels.Consol.CustomString2, Schema.JK_CustomString2, Constants.CustomLabels.Descriptions.CustomText(2));
				result.Add(Constants.CustomLabels.Consol.CustomNumber1, Schema.JK_CustomNumber1, Constants.CustomLabels.Descriptions.CustomNumber(1));
				result.Add(Constants.CustomLabels.Consol.CustomNumber2, Schema.JK_CustomNumber2, Constants.CustomLabels.Descriptions.CustomNumber(2));

				return result;
			}
		}

		#endregion

		#region IServiceLocator Members

		object IServiceLocator.GetService(Type serviceType)
		{
			if (serviceType == typeof(ICustomsCharges))
			{
				object locator = Activator.CreateInstance(ObjectFactory.GetType<IConsolChargesLocator>(), this);
				return ServiceLocator.GetService<ICustomsCharges>(locator);
			}
			return null;
		}

		#endregion

		#region IJobInvoicingPlugIn Members

		public IJobInvoicingSupporter InvoicingSupporter
		{
			get { return invoicingSupporter ?? (invoicingSupporter = new ForwardingConsolInvoicingSupporter(this)); }
		}
		ForwardingConsolInvoicingSupporter invoicingSupporter;

		#endregion

		#region Rating

		RatingAdaptersProvider IRatingSupporter.AdaptersProvider
		{
			get { return new ForwardingConsolRatingAdaptersProvider(this); }
		}

		protected override IAutoRating GetRatingAdapterCore()
		{
			return new ForwardingConsolRatingAdapter(new ConsolRatingRoute(this));
		}

		protected override JobInvoicingConsumerType GetConsumerTypeCore()
		{
			return InvoicingSupporter.ConsumerType;
		}

		#endregion

		#region IGenericJobCostPlugIn Members

		public IGenericJobCostSupporter CostSupporter
		{
			get { return consolCostSupporter ?? (consolCostSupporter = new ForwardingConsolCostSupporter(this)); }
		}
		ForwardingConsolCostSupporter consolCostSupporter;

		public class ForwardingConsolCostSupporter : GenericJobCostSupporter
		{
			public ForwardingConsolCostSupporter(ForwardingConsol parent)
			{
				this.parent = parent;
			}
			readonly ForwardingConsol parent;

			public override ZGuid PK
			{
				get
				{
					return parent == null ? ZGuid.Empty : parent.PK;
				}
			}

			public override ZString Type
			{
				get { return parent.ConsolType; }
			}

			public override ZGuid[] ShipmentsListPKs
			{
				get { return parent.Shipments.GetPKs().ToArray(); }
			}

			public override IJobInvoicingPlugIn[] ShipmentsList
			{
				get
				{
					IJobInvoicingPlugIn[] result = new IJobInvoicingPlugIn[parent.Shipments.Count];
					for (int i = 0; i < parent.Shipments.Count; i++)
					{
						result[i] = parent.Shipments[i];
					}
					return result;
				}
			}

			public override bool HasChanges
			{
				get { return parent.HasChanges; }
			}

			public override bool IsInDatabase
			{
				get { return parent.IsInDatabase; }
			}

			public override DocumentSupporter DocumentSupporter
			{
				get { return parent.DocumentSupporter; }
			}

			public override ZString MasterBillNum
			{
				get { return parent.JK_MasterBillNum; }
			}

			public override ZString TransportMode
			{
				get { return parent.JK_TransportMode; }
			}

			public override ZGuid GetCreditorPK(ZString chargeCodeGroup, ZGuid rateProviderOrgPK)
			{
				return chargeCodeGroup.IsEmpty ? ZGuid.Empty : parent.GetCreditorPKForConsolCost(rateProviderOrgPK);
			}

			public override ZString TotalChargeableUnit
			{
				get { return parent.JK_ConsolChargeableUnit; }
			}

			public override MasterFiles.Business.Directions Direction => parent.JobDirection;

			public override bool IsBuyersConsol
			{
				get { return parent.IsBuyersConsol; }
			}

			public override ZString PortOfLoading
			{
				get { return parent.JK_RL_NKLoadPort; }
			}

			public override ZString PortOfDischarge
			{
				get { return parent.JK_RL_NKDischargePort; }
			}

			public override ZString ConsolMode
			{
				get { return parent.JK_ConsolMode; }
			}

			public override OrgHeader SendingForwarder
			{
				get { return parent.SendingForwarder; }
			}

			public override OrgHeader ReceivingForwarder
			{
				get { return parent.ReceivingForwarder; }
			}

			[DocumentFieldExcludeFromMap]
			public override ManyToManyBusinessObjectCollection Shipments
			{
				get { return parent.Shipments; }
			}

			public override ZDateTime ETD
			{
				get { return parent.JK_JX_JA_E_DEP; }
			}

			public override ZDateTime ETA
			{
				get { return parent.JK_JX_JB_E_ARV; }
			}

			public override ZDecimal FreeSpace => parent.JK_Calc_FreeSpace;
		}

		#endregion

		#region IJobCostingPlugIn Members

		public ZString GetPrepaidCollect(IJobInvoicingPlugIn apportionableJob)
		{
			var shipment = apportionableJob as ForwardingShipment;
			return shipment != null ? shipment.JS_PaymentTerm : ZString.Empty;
		}

		bool IJobCostingPlugIn.IsMasterCollect
		{
			get { return JK_PrepaidCollect == Constants.PaymentType.Collect; }
		}

		RefCurrency IJobCostingPlugIn.ConsolCurrency
		{
			get { return FreightCostsCurrency; }
		}

		decimal IJobCostingPlugIn.ConsolExchangeRate
		{
			get { return FreightCostsExchangeRate; }
		}

		OrgHeader IJobCostingPlugIn.ReceivingAgent
		{
			get { return ReceivingForwarder; }
		}

		OrgHeader IJobCostingPlugIn.ReceivingAgentAPInvoicingParty
		{
			get
			{
				OrgHeader result = null;
				if (ReceivingForwarder != null)
				{
					result = ReceivingForwarder.GetRelatedParty(RelatedPartyTypeList.Codes.APNettingGroup, RelatedPartyDirectionList.Codes.Forwarder);
				}
				return result ?? ReceivingForwarder;
			}
		}

		OrgHeader IJobCostingPlugIn.ReceivingAgentARInvoicingParty
		{
			get
			{
				OrgHeader result = null;
				if (ReceivingForwarder != null)
				{
					result = ReceivingForwarder.GetRelatedParty(RelatedPartyTypeList.Codes.ARNettingGroup, RelatedPartyDirectionList.Codes.Forwarder);
				}
				return result ?? ReceivingForwarder;
			}
		}

		OrgHeader IJobCostingPlugIn.SendingAgent
		{
			get { return SendingForwarder; }
		}

		OrgHeader IJobCostingPlugIn.SendingAgentAPInvoicingParty
		{
			get
			{
				OrgHeader result = null;
				if (SendingForwarder != null)
				{
					result = SendingForwarder.GetRelatedParty(RelatedPartyTypeList.Codes.APNettingGroup, RelatedPartyDirectionList.Codes.Forwarder);
				}
				return result ?? SendingForwarder;
			}
		}

		OrgHeader IJobCostingPlugIn.SendingAgentARInvoicingParty
		{
			get
			{
				OrgHeader result = null;
				if (SendingForwarder != null)
				{
					result = SendingForwarder.GetRelatedParty(RelatedPartyTypeList.Codes.ARNettingGroup, RelatedPartyDirectionList.Codes.Forwarder);
				}
				return result ?? SendingForwarder;
			}
		}

		public CodeDescriptionPairList PrepaidCollectList
		{
			get
			{
				var list = new CodeDescriptionPairList(OLookUpEditType.PaymentType);
				list.AddPair(PrepaidCollectFreightForwardingList.Codes.CTS, PrepaidCollectFreightForwardingList.Descriptions.CTS);
				list.AddPair(PrepaidCollectFreightForwardingList.Codes.All, PrepaidCollectFreightForwardingList.Descriptions.All);

				list.AddPair(PrepaidCollectFreightForwardingList.Codes.FOG, PrepaidCollectFreightForwardingList.Descriptions.FOG);
				list.AddPair(PrepaidCollectFreightForwardingList.Codes.LOG, PrepaidCollectFreightForwardingList.Descriptions.LOG);
				list.AddPair(PrepaidCollectFreightForwardingList.Codes.FDT, PrepaidCollectFreightForwardingList.Descriptions.FDT);
				list.AddPair(PrepaidCollectFreightForwardingList.Codes.LDT, PrepaidCollectFreightForwardingList.Descriptions.LDT);

				return list;
			}
		}

		void IJobCostingPlugIn.AddNewToLogs(Event @event, ZString reference)
		{
			Logs.AddNew(@event, reference);
		}

		decimal IJobCostingPlugIn.ExchangeRateForCurrency(RefCurrency currency, ZGuid currentCostPK)
		{
			return GetExchangeRateFromFreightCostsOrSchedule(currency != null ? currency.RX_Code : ZString.Empty, currentCostPK);
		}

		ZString IJobCostingPlugIn.ContainerMode => JK_ConsolMode;

		ZString IJobCostingPlugIn.ConsolType => JK_AgentType;

		ZString IJobCostingPlugIn.Module => ApportionmentMethodModules.Forwarding;

		ZString IJobCostingPlugIn.Direction => IJobCostingPlugInHelper.GetDirectionCode(CostSupporter.Direction);

		public ZGuid[] GetShipmentsListPKs()
		{
			return CostSupporter.ShipmentsListPKs;
		}

		public IJobInvoicingPlugIn[] GetShipmentsList()
		{
			return CostSupporter.ShipmentsList;
		}

		public JobProfitLossCollection ProfitLossContainer
		{
			get { return profitLossContainer ?? (profitLossContainer = new JobProfitLossCollection(Factory)); }
		}
		JobProfitLossCollection profitLossContainer;

		#endregion

		#region IDeniedPartyProvider Members

		ZString IDeniedPartyProvider.ReferenceId => JK_UniqueConsignRef;

		#endregion

		#region IScreeningPartyProvider Members

		ZString IScreeningStatusProvider.ScreeningStatus
		{
			get { return JK_ScreeningStatus; }
			set { JK_ScreeningStatus = value; }
		}

		ZBool IShouldUpdateScreeningStatus.ShouldUpdateScreeningStatus { get; set; }

		ScreeningParty[] IScreeningPartyProvider.ScreeningParties
		{
			get
			{
				var result = GetScreeningPartiesCommon();

				foreach (Transport transport in Transports)
				{
					result.AddRange(transport.GetScreeningPartiesFromVessel(this, allowInlandWaterwayTransport: true));
				}

				foreach (ForwardingShipment shipment in Shipments)
				{
					ScreeningParty[] partiesFromShipments = ((IScreeningPartyProvider)shipment).ScreeningParties;
					foreach (ScreeningParty party in partiesFromShipments)
					{
						party.AddParent(this);
					}
					result.AddRange(partiesFromShipments);
				}

				return result.ToArray();
			}
		}

		List<ScreeningParty> GetScreeningPartiesCommon()
		{
			List<ScreeningParty> result = new List<ScreeningParty>();

			result.AddRange(from docAddress in DocAddresses.Cast<JobDocAddress>() select new ScreeningParty(this, docAddress.AddressCaption, docAddress));

			if (Job != null)
			{
				result.Add(new ScreeningParty(this, Res.GetString("29f83525-cf52-4dbf-9f6c-ed8c01a24c17", "Local Client"), Job.LocalCharges));
			}

			result.Add(new ScreeningParty(this, Res.GetString("58c4b827-b7a5-4e7a-b175-857962069756", "Sending Agent"), SendingForwarder));

			result.Add(new ScreeningParty(this, Res.GetString("54773dbc-24fb-4462-92c2-5db9aae069b3", "Receiving Agent"), ReceivingForwarder));

			result.Add(new ScreeningParty(this, Res.GetString("ead8902e-5548-4b20-ad88-a8c6e3e8db77", "Carrier"), ShippingLine));

			result.Add(new ScreeningParty(this, Res.GetString("78ade7cf-c132-423a-9e34-93ab15dd4b11", "Creditor"), Creditor));

			if (DepartureCTOAddress != null)
			{
				result.Add(new ScreeningParty(this, Res.GetString("8e74b1c7-73e4-4497-9625-750057092af8", "Departure CTO"), DepartureCTOAddress.Header));
			}

			if (PackDepotAddress != null)
			{
				result.Add(new ScreeningParty(this, Res.GetString("1ba0c93f-4482-4157-9f33-606779892316", "Pack Depot"), PackDepotAddress.Header));
			}

			if (ContainerYardEmptyPickupAddress != null)
			{
				result.Add(new ScreeningParty(this, Res.GetString("5db7be65-c057-4675-9b27-9d25f5959bb0", "Pickup Container Yard"), ContainerYardEmptyPickupAddress.Header));
			}

			if (ArrivalCTOAddress != null)
			{
				result.Add(new ScreeningParty(this, Res.GetString("3da7983b-1f62-45fc-a5e3-31825eb2d18a", "Arrival CTO"), ArrivalCTOAddress.Header));
			}

			if (UnpackDepotAddress != null)
			{
				result.Add(new ScreeningParty(this, Res.GetString("3a34234c-161d-4c47-b496-1811a08cfd6a", "Unpack Depot"), UnpackDepotAddress.Header));
			}

			if (ContainerYardEmptyReturnAddress != null)
			{
				result.Add(new ScreeningParty(this, Res.GetString("c1412ca3-7503-4d73-a2bc-f765bf1f8d9c", "Return Container Yard"), ContainerYardEmptyReturnAddress.Header));
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

			if (JK_ScreeningStatus != MasterFiles.Integration.ScreeningStatusesList.Codes.JobCleared)
			{
				GetScreeningStatus(statuses);
			}
			else
			{
				statuses.Add(MasterFiles.Integration.ScreeningStatusesList.Codes.JobCleared);
			}

			return ScreeningStatusUpdater.GetWorstScreeningStatus(statuses);
		}

		void GetScreeningStatus(List<ZString> statuses)
		{
			AddScreeningStatusToList(statuses, SendingForwarder);
			AddScreeningStatusToList(statuses, ReceivingForwarder);
			AddScreeningStatusToList(statuses, ShippingLine);
			AddScreeningStatusToList(statuses, Creditor);

			AddScreeningStatusToList(statuses, Job?.LocalCharges);
			AddScreeningStatusToList(statuses, DepartureCTOAddress?.Header);
			AddScreeningStatusToList(statuses, PackDepotAddress?.Header);
			AddScreeningStatusToList(statuses, ContainerYardEmptyPickupAddress?.Header);
			AddScreeningStatusToList(statuses, ArrivalCTOAddress?.Header);
			AddScreeningStatusToList(statuses, UnpackDepotAddress?.Header);
			AddScreeningStatusToList(statuses, ContainerYardEmptyReturnAddress?.Header);

			AddScreeningStatusToList(statuses, GetWorstTransportScreeningStatus());
			AddScreeningStatusToList(statuses, GetWorstShipmentScreeningStatus());
			AddScreeningStatusToList(statuses, GetWorstDocAddressScreeningStatus());
		}

		ZString GetWorstTransportScreeningStatus()
		{
			var parties = new List<IScreeningPartyProvider>();
			var linkedPaties = Transports.Cast<Transport>().Where(tran => tran.JW_IsLinked && (tran.JW_TransportMode == Core.Constants.TransportModes.Sea || tran.JW_TransportMode == Core.Constants.TransportModes.InlandWaterwayTransport))
								.Select(tran => tran.Vessel)
								.Cast<IScreeningPartyProvider>();

			if (linkedPaties?.Any() ?? false)
			{
				parties.AddRange(linkedPaties);
			}

			var notLinkedParties = Transports.Cast<Transport>().Where(tran => !tran.JW_IsLinked && !string.IsNullOrWhiteSpace(tran.JW_Vessel) && (tran.JW_TransportMode == Core.Constants.TransportModes.Sea || tran.JW_TransportMode == Core.Constants.TransportModes.InlandWaterwayTransport))
									.Select(tran => tran.Vessel == null ? tran : tran.Vessel as IScreeningPartyProvider);

			if (notLinkedParties?.Any() ?? false)
			{
				parties.AddRange(notLinkedParties);
			}

			return ScreeningStatusUpdater.GetWorstScreeningStatus(parties);
		}

		ZString GetWorstShipmentScreeningStatus()
		{
			return ScreeningStatusUpdater.GetWorstScreeningStatus(Shipments.Cast<IScreeningPartyProvider>());
		}

		ZString GetWorstDocAddressScreeningStatus()
		{
			return ScreeningStatusUpdater.GetWorstScreeningStatus(DocAddresses.Cast<IScreeningPartyProvider>());
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
				var pks = new List<ZGuid>();
				Shipments.Cast<ForwardingShipment>().ForEach(o =>
				{
					pks.AddRange(o.GetPksFromAllSubShipmentsAndDeclarationsWithoutChildren());
					pks.Add(o.PK);
					o.AddPksFromDeclaration(o, pks);
				});

				pks.Add(PK);
				return RelatedOrgPartyScreeningStatusHelper.GetRelatedOrgPartyScreeningStatusCollection(Factory, ((IScreeningPartyProvider)this).ScreeningParties, relatedJobPKs: pks.ToArray());
			}
		}

		#endregion

		#region ICreditControlledDocumentDelivery Members

		protected override bool IsDPSFreightMovementRestrictedCore()
		{
			return ObjectFactory.Get<IComplianceRiskStatusSupporter>().IsDPSFreightMovementRestricted(JK_ScreeningStatus, this, this);
		}

		protected override bool IsAviationSecurityFreightMovementRestrictedCore()
		{
			return SupplyChainSecurityConfiguration.IsAviationSecurityFreightMovementRestricted(this);
		}

		protected override ScreeningParty[] GetScreeningPartiesCore()
		{
			return ((IScreeningPartyProvider)this).ScreeningParties;
		}

		#endregion

		#region IDocAddresses

		protected override DocAddressType[] SupportedAddressTypes
		{
			get
			{
				var carrierCreditors = new List<DocAddressType>();

				carrierCreditors.Add(DocAddressType.CarrierExportCreditor);
				carrierCreditors.Add(DocAddressType.CarrierImportCreditor);

				if (IsDirect)
				{
					return new DocAddressType[]
					{
						DocAddressType.NotifyParty,
						DocAddressType.NotifyParty2,
						DocAddressType.NotifyParty3,
						DocAddressType.MasterBillIssuingParty,
						DocAddressType.CarrierBookingAgent,
						DocAddressType.CarrierHandlingAgent,
						DocAddressType.FreightPayer
					}.Concat(carrierCreditors).ToArray();
				}
				return new DocAddressType[]
				{
					DocAddressType.NotifyParty,
					DocAddressType.NotifyParty2,
					DocAddressType.NotifyParty3,
					DocAddressType.MasterBillIssuingParty,
					DocAddressType.CarrierBookingAgent,
					DocAddressType.CarrierHandlingAgent,
					DocAddressType.MasterBillShipperOverride,
					DocAddressType.MasterBillConsigneeOverride,
					DocAddressType.FreightPayer
				}.Concat(carrierCreditors).ToArray();
			}
		}

		protected override JobDocAddressRequirement GetDocAddressRequirement(DocAddressType addressType)
		{
			switch (addressType)
			{
				case DocAddressType.NotifyParty:
					return NotifyPartyDocAddressRequirement;
				case DocAddressType.NotifyParty2:
					return NotifyParty2DocAddressRequirement;
				case DocAddressType.NotifyParty3:
					return NotifyParty3DocAddressRequirement;
				case DocAddressType.MasterBillIssuingParty:
					return MasterBillIssuingPartyDocAddressRequirement;
				case DocAddressType.CarrierBookingAgent:
					return CarrierBookingAgentDocAddressRequirement;
				case DocAddressType.CarrierHandlingAgent:
					return CarrierHandlingAgentDocAddressRequirement;
				case DocAddressType.MasterBillShipperOverride:
					return MasterBillShipperOverrideDocAddressRequirement;
				case DocAddressType.MasterBillConsigneeOverride:
					return MasterBillConsigneeOverrideDocAddressRequirement;
				case DocAddressType.CarrierExportCreditor:
					return CarrierExportCreditorDocAddressRequirement;
				case DocAddressType.CarrierImportCreditor:
					return CarrierImportCreditorDocAddressRequirement;
				case DocAddressType.FreightPayer:
					return FreightPayerDocAddressRequirement;
				default:
					return base.GetDocAddressRequirement(addressType);
			}
		}

		#region NotifyPartyDocumentaryAddresses

		public OrgHeader NotifyParty
		{
			get { return (NotifyPartyDocumentaryAddress == null || !NotifyPartyDocumentaryAddress.HasRealOrganisation) ? null : NotifyPartyDocumentaryAddress.Organisation; }
		}

		public JobDocAddress NotifyPartyDocumentaryAddress
		{
			get
			{
				if (BusinessObject.IsNullOrDeleted(notifyPartyDocumentaryAddress))
				{
					notifyPartyDocumentaryAddress = DocAddresses.FindOrCreateWithRequirement(NotifyPartyDocAddressRequirement);
				}
				return notifyPartyDocumentaryAddress;
			}
		}
		JobDocAddress notifyPartyDocumentaryAddress;

		JobDocAddressRequirement NotifyPartyDocAddressRequirement
		{
			get { return notifyPartyDocAddressRequirement ?? (notifyPartyDocAddressRequirement = GetNotifyPartyDocAddressRequirement()); }
		}
		JobDocAddressRequirement notifyPartyDocAddressRequirement;

		protected virtual JobDocAddressRequirement GetNotifyPartyDocAddressRequirement()
		{
			return new JobDocAddressRequirement(DocAddressType.NotifyParty, ContactType.NotifyParty);
		}

		public OrgHeader NotifyParty2
		{
			get { return (NotifyParty2DocumentaryAddress == null || !NotifyParty2DocumentaryAddress.HasRealOrganisation) ? null : NotifyParty2DocumentaryAddress.Organisation; }
		}

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

		public OrgHeader NotifyParty3
		{
			get { return (NotifyParty3DocumentaryAddress == null || !NotifyParty3DocumentaryAddress.HasRealOrganisation) ? null : NotifyParty3DocumentaryAddress.Organisation; }
		}

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

		#region MasterBillIssuingParty

		public OrgHeader MasterBillIssuingParty
		{
			get { return (MasterBillIssuingPartyDocumentaryAddress == null || !MasterBillIssuingPartyDocumentaryAddress.HasRealOrganisation) ? null : MasterBillIssuingPartyDocumentaryAddress.Organisation; }
		}

		public JobDocAddress MasterBillIssuingPartyDocumentaryAddress
		{
			get
			{
				if (BusinessObject.IsNullOrDeleted(masterBillIssuingPartyDocumentaryAddress))
				{
					masterBillIssuingPartyDocumentaryAddress = DocAddresses.FindOrCreateWithRequirement(MasterBillIssuingPartyDocAddressRequirement);
				}
				return masterBillIssuingPartyDocumentaryAddress;
			}
		}
		JobDocAddress masterBillIssuingPartyDocumentaryAddress;

		JobDocAddressRequirement MasterBillIssuingPartyDocAddressRequirement
		{
			get { return masterBillIssuingPartyDocAddressRequirement ?? (masterBillIssuingPartyDocAddressRequirement = GetMasterBillIssuingPartyDocAddressRequirement()); }
		}
		JobDocAddressRequirement masterBillIssuingPartyDocAddressRequirement;

		protected virtual JobDocAddressRequirement GetMasterBillIssuingPartyDocAddressRequirement()
		{
			return new JobDocAddressRequirement(DocAddressType.MasterBillIssuingParty);
		}

		#endregion

		#region CarrierExportCreditor

		public override OrgHeader CarrierExportCreditor
		{
			get { return (CarrierExportCreditorAddress == null || !CarrierExportCreditorAddress.HasRealOrganisation) ? null : CarrierExportCreditorAddress.Organisation; }
		}

		public override JobDocAddress CarrierExportCreditorAddress
		{
			get
			{
				SetupCarrierExportCreditorAddress();
				return carrierExportCreditorDocumentaryAddress;
			}
		}

		JobDocAddress carrierExportCreditorDocumentaryAddress;

		void SetupCarrierExportCreditorAddress()
		{
			if (IsNullOrDeleted(carrierExportCreditorDocumentaryAddress))
			{
				UnRegisterListChangedCalledRefreshBinding(carrierExportCreditorDocumentaryAddress);
				carrierExportCreditorDocumentaryAddress = DocAddresses.FindOrCreateWithRequirement(CarrierExportCreditorDocAddressRequirement);
				AddEventHandlersToCarrierExportCreditorAddressEvents();
				RegisterListChangedCalledRefreshBinding(carrierExportCreditorDocumentaryAddress);
			}
		}

		#region AddEventHandlersToCarrierExportCreditorAddressEvents

		protected virtual void AddEventHandlersToCarrierExportCreditorAddressEvents()
		{
			carrierExportCreditorDocumentaryAddress.OrgAddressBeforeChange -= CreditorExportDocAddress_OrgAddressBeforeChange;
			carrierExportCreditorDocumentaryAddress.DocAddressChanged -= CreditorExportDocAddress_DocAddressChanged;

			carrierExportCreditorDocumentaryAddress.OrgAddressBeforeChange += CreditorExportDocAddress_OrgAddressBeforeChange;
			carrierExportCreditorDocumentaryAddress.DocAddressChanged += CreditorExportDocAddress_DocAddressChanged;
		}

		public bool IsChangingExportCreditorAddress { get; private set; }

		void CreditorExportDocAddress_OrgAddressBeforeChange(object sender, EventArgs e)
		{
			IsChangingExportCreditorAddress = true;
		}

		void CreditorExportDocAddress_DocAddressChanged(object sender, EventArgs e)
		{
			SetCreditorFromDocCreditorExportAddress();
			IsChangingExportCreditorAddress = false;
		}

		void SetCreditorFromDocCreditorExportAddress()
		{
			if (!IsCoLoad
				&& JK_OA_CreditorAddress != carrierExportCreditorDocumentaryAddress.E2_OA_Address
				&& (this.IsExport() || this.IsDomestic())
				&& (carrierExportCreditorDocumentaryAddress.Organisation?.OH_IsCreditor ?? true))
			{
				if (carrierExportCreditorDocumentaryAddress.E2_OA_Address.IsEmpty)
				{
					JK_OA_CreditorAddress_ZAddress.SetOrgWithoutSettingDefaultAddress(ZGuid.Empty); //clear creditor text
				}

				JK_OA_CreditorAddress = carrierExportCreditorDocumentaryAddress.E2_OA_Address;
			}
		}

		#endregion

		JobDocAddressRequirement CarrierExportCreditorDocAddressRequirement
		{
			get { return carrierExportCreditorDocAddressRequirement ?? (carrierExportCreditorDocAddressRequirement = GetCarrierExportCreditorDocAddressRequirement()); }
		}
		JobDocAddressRequirement carrierExportCreditorDocAddressRequirement;

		protected virtual JobDocAddressRequirement GetCarrierExportCreditorDocAddressRequirement()
		{
			var req = new JobDocAddressRequirement(DocAddressType.CarrierExportCreditor);
			return req;
		}

		#endregion

		#region CarrierImportCreditor

		public override OrgHeader CarrierImportCreditor
		{
			get { return (CarrierImportCreditorAddress == null || !CarrierImportCreditorAddress.HasRealOrganisation) ? null : CarrierImportCreditorAddress.Organisation; }
		}

		public override JobDocAddress CarrierImportCreditorAddress
		{
			get
			{
				SetupCarrierImportCreditorAddress();
				return carrierImportCreditorDocumentaryAddress;
			}
		}

		JobDocAddress carrierImportCreditorDocumentaryAddress;

		void SetupCarrierImportCreditorAddress()
		{
			if (IsNullOrDeleted(carrierImportCreditorDocumentaryAddress))
			{
				UnRegisterListChangedCalledRefreshBinding(carrierImportCreditorDocumentaryAddress);
				carrierImportCreditorDocumentaryAddress = DocAddresses.FindOrCreateWithRequirement(CarrierImportCreditorDocAddressRequirement);
				AddEventHandlersToCarrierImportCreditorAddressEvents();
				RegisterListChangedCalledRefreshBinding(carrierImportCreditorDocumentaryAddress);
			}
		}

		#region AddEventHandlersToCarrierImportCreditorAddressEvents

		protected virtual void AddEventHandlersToCarrierImportCreditorAddressEvents()
		{
			carrierImportCreditorDocumentaryAddress.OrgAddressBeforeChange -= CreditorImportDocAddress_OrgAddressBeforeChange;
			carrierImportCreditorDocumentaryAddress.DocAddressChanged -= CreditorImportDocAddress_DocAddressChanged;

			carrierImportCreditorDocumentaryAddress.OrgAddressBeforeChange += CreditorImportDocAddress_OrgAddressBeforeChange;
			carrierImportCreditorDocumentaryAddress.DocAddressChanged += CreditorImportDocAddress_DocAddressChanged;
		}

		public bool IsChangingImportCreditorAddress { get; private set; }

		void CreditorImportDocAddress_OrgAddressBeforeChange(object sender, EventArgs e)
		{
			IsChangingImportCreditorAddress = true;
		}

		void CreditorImportDocAddress_DocAddressChanged(object sender, EventArgs e)
		{
			SetCreditorFromDocCreditorImportAddress();
			IsChangingImportCreditorAddress = false;
		}

		void SetCreditorFromDocCreditorImportAddress()
		{
			if (!IsCoLoad
				&& JK_OA_CreditorAddress != carrierImportCreditorDocumentaryAddress.E2_OA_Address
				&& (this.IsImport() || this.IsCrossTrade())
				&& (carrierImportCreditorDocumentaryAddress.Organisation?.OH_IsCreditor ?? true))
			{
				if (carrierImportCreditorDocumentaryAddress.E2_OA_Address.IsEmpty)
				{
					//clear creditor text
					JK_OA_CreditorAddress_ZAddress.SetOrgWithoutSettingDefaultAddress(ZGuid.Empty);
				}

				JK_OA_CreditorAddress = carrierImportCreditorDocumentaryAddress.E2_OA_Address;
			}
		}

		#endregion

		JobDocAddressRequirement CarrierImportCreditorDocAddressRequirement
		{
			get { return carrierImportCreditorDocAddressRequirement ?? (carrierImportCreditorDocAddressRequirement = GetCarrierImportCreditorDocAddressRequirement()); }
		}
		JobDocAddressRequirement carrierImportCreditorDocAddressRequirement;

		protected virtual JobDocAddressRequirement GetCarrierImportCreditorDocAddressRequirement()
		{
			var req = new JobDocAddressRequirement(DocAddressType.CarrierImportCreditor);
			return req;
		}

		#endregion

		#region CarrierBookingAgent

		void DefaultCarrierBookingAgent(string message)
		{
			var currentCarrierBookingAgent = DocAddresses.FindDocAddressesByType(DocAddressType.CarrierBookingAgent).FirstOrDefault();
			var appointedCarrierBookingAgentAddress = ShippingLine?.CarrierAppointedAgentPorts_Agency?.FindAddress(JK_RL_NKLoadPort);

			if (appointedCarrierBookingAgentAddress == null || currentCarrierBookingAgent?.Address == appointedCarrierBookingAgentAddress)
			{
				return;
			}

			if (!IsInDatabase || currentCarrierBookingAgent == null || DoesUserWantToDefaultCarrierBookingAgent(message))
			{
				CarrierBookingAgentDocumentaryAddress.OrganisationPK = appointedCarrierBookingAgentAddress.OA_OH;
			}
		}

		public ConfirmationPrompt BookingAgentDefaultingAsker { get; set; }

		void DefaultCarrierBookingAgentFromLoadPort()
		{
			DefaultCarrierBookingAgent(
				ResString.GetMultilingualString(
					"f443fbfd-6785-48a6-be1a-7ed8796a638c",
					"The 1st Load Port has been changed.  Do you wish to re-default the Carrier Booking Agent from the Carrier based on the new 1st Load Port?"));
		}

		void DefaultCarrierBookingAgentFromCarrier()
		{
			DefaultCarrierBookingAgent(
				ResString.GetMultilingualString(
					"d14e2ff3-c1f0-4bdc-8629-d133cf90ee57",
					"The Carrier has been changed.  Do you wish to re-default the Carrier Booking Agent from the Carrier based on the new 1st Load Port?"));
		}

		bool DoesUserWantToDefaultCarrierBookingAgent(string message)
		{
			if (BookingAgentDefaultingAsker == null || Factory.IsInTransaction)
			{
				return false;
			}

			var caption = ResString.GetMultilingualString("952aabc0-ebd1-4d0a-8848-30433b615085", "Carrier Booking Agent Defaulting");
			return BookingAgentDefaultingAsker.PromptYesOrNo(message, caption);
		}

		public OrgHeader CarrierBookingAgent
		{
			get { return (CarrierBookingAgentDocumentaryAddress == null || !CarrierBookingAgentDocumentaryAddress.HasRealOrganisation) ? null : CarrierBookingAgentDocumentaryAddress.Organisation; }
		}

		public JobDocAddress CarrierBookingAgentDocumentaryAddress
		{
			get
			{
				if (BusinessObject.IsNullOrDeleted(carrierBookingAgentDocumentaryAddress))
				{
					carrierBookingAgentDocumentaryAddress = DocAddresses.FindOrCreateWithRequirement(CarrierBookingAgentDocAddressRequirement);
				}
				return carrierBookingAgentDocumentaryAddress;
			}
		}
		JobDocAddress carrierBookingAgentDocumentaryAddress;

		JobDocAddressRequirement CarrierBookingAgentDocAddressRequirement
		{
			get { return carrierBookingAgentDocAddressRequirement ?? (carrierBookingAgentDocAddressRequirement = new JobDocAddressRequirement(DocAddressType.CarrierBookingAgent)); }
		}
		JobDocAddressRequirement carrierBookingAgentDocAddressRequirement;

		#endregion

		#region CarrierHandlingAgent

		public OrgHeader CarrierHandlingAgent
		{
			get { return (CarrierHandlingAgentDocumentaryAddress == null || !CarrierHandlingAgentDocumentaryAddress.HasRealOrganisation) ? null : CarrierHandlingAgentDocumentaryAddress.Organisation; }
		}

		public JobDocAddress CarrierHandlingAgentDocumentaryAddress
		{
			get
			{
				if (BusinessObject.IsNullOrDeleted(carrierHandlingAgentDocumentaryAddress))
				{
					carrierHandlingAgentDocumentaryAddress = DocAddresses.FindOrCreateWithRequirement(CarrierHandlingAgentDocAddressRequirement);
				}
				return carrierHandlingAgentDocumentaryAddress;
			}
		}
		JobDocAddress carrierHandlingAgentDocumentaryAddress;

		JobDocAddressRequirement CarrierHandlingAgentDocAddressRequirement
		{
			get { return carrierHandlingAgentDocAddressRequirement ?? (carrierHandlingAgentDocAddressRequirement = new JobDocAddressRequirement(DocAddressType.CarrierHandlingAgent)); }
		}
		JobDocAddressRequirement carrierHandlingAgentDocAddressRequirement;

		#endregion

		#region MasterBillShipperOverride

		public OrgHeader MasterBillShipperOverride
		{
			get { return (MasterBillShipperOverrideDocumentaryAddress == null || !MasterBillShipperOverrideDocumentaryAddress.HasRealOrganisation) ? null : MasterBillShipperOverrideDocumentaryAddress.Organisation; }
		}

		public JobDocAddress MasterBillShipperOverrideDocumentaryAddress
		{
			get
			{
				if (BusinessObject.IsNullOrDeleted(masterBillShipperOverrideDocumentaryAddress))
				{
					masterBillShipperOverrideDocumentaryAddress = DocAddresses.FindOrCreateWithRequirement(MasterBillShipperOverrideDocAddressRequirement);
				}
				return masterBillShipperOverrideDocumentaryAddress;
			}
		}
		JobDocAddress masterBillShipperOverrideDocumentaryAddress;

		JobDocAddressRequirement MasterBillShipperOverrideDocAddressRequirement
		{
			get { return masterBillShipperOverrideDocAddressRequirement ?? (masterBillShipperOverrideDocAddressRequirement = new JobDocAddressRequirement(DocAddressType.MasterBillShipperOverride)); }
		}
		JobDocAddressRequirement masterBillShipperOverrideDocAddressRequirement;

		#endregion

		#region MasterBillConsigneeOverride

		public OrgHeader MasterBillConsigneeOverride
		{
			get { return (MasterBillConsigneeOverrideDocumentaryAddress == null || !MasterBillConsigneeOverrideDocumentaryAddress.HasRealOrganisation) ? null : MasterBillConsigneeOverrideDocumentaryAddress.Organisation; }
		}

		public JobDocAddress MasterBillConsigneeOverrideDocumentaryAddress
		{
			get
			{
				if (BusinessObject.IsNullOrDeleted(masterBillConsigneeOverrideDocumentaryAddress))
				{
					masterBillConsigneeOverrideDocumentaryAddress = DocAddresses.FindOrCreateWithRequirement(MasterBillConsigneeOverrideDocAddressRequirement);
				}
				return masterBillConsigneeOverrideDocumentaryAddress;
			}
		}
		JobDocAddress masterBillConsigneeOverrideDocumentaryAddress;

		JobDocAddressRequirement MasterBillConsigneeOverrideDocAddressRequirement
		{
			get { return masterBillConsigneeOverrideDocAddressRequirement ?? (masterBillConsigneeOverrideDocAddressRequirement = new JobDocAddressRequirement(DocAddressType.MasterBillConsigneeOverride)); }
		}
		JobDocAddressRequirement masterBillConsigneeOverrideDocAddressRequirement;

		#endregion

		#region FreightPayer

		public OrgHeader FreightPayer
		{
			get { return (FreightPayerDocumentaryAddress == null || !FreightPayerDocumentaryAddress.HasRealOrganisation) ? null : FreightPayerDocumentaryAddress.Organisation; }
		}

		public JobDocAddress FreightPayerDocumentaryAddress
		{
			get
			{
				if (BusinessObject.IsNullOrDeleted(freightPayerDocumentaryAddress))
				{
					freightPayerDocumentaryAddress = DocAddresses.FindOrCreateWithRequirement(FreightPayerDocAddressRequirement);
				}
				return freightPayerDocumentaryAddress;
			}
		}
		JobDocAddress freightPayerDocumentaryAddress;

		JobDocAddressRequirement FreightPayerDocAddressRequirement
		{
			get { return freightPayerDocAddressRequirement ?? (freightPayerDocAddressRequirement = new JobDocAddressRequirement(DocAddressType.FreightPayer)); }
		}
		JobDocAddressRequirement freightPayerDocAddressRequirement;

		#endregion

		#region Special Handling Items

		public void DefaultSpecialHandlingItems()
		{
			if (!IsAWBHeaderAccessible || (IsInDatabase && !AWBSpecialHandlingItems.IsNullOrEmpty()))
			{
				return;
			}

			var eFreightStatus = GetEFreightStatus();

			var specialHandling = AWBSpecialHandlingItems.Cast<JobConsolAWBSpecialHandling>().FirstOrDefault(x => x.JKH_Code == previousDefaultedSpecialHandling);
			if (!string.IsNullOrEmpty(previousDefaultedSpecialHandling) && specialHandling != null)
			{
				AWBSpecialHandlingItems.RemoveAndDelete(specialHandling);
			}

			if (!string.IsNullOrEmpty(eFreightStatus))
			{
				previousDefaultedSpecialHandling = eFreightStatus;
				if (!AWBSpecialHandlingItems.Cast<JobConsolAWBSpecialHandling>().Any(x => x.JKH_Code == eFreightStatus))
				{
					var specialHandlingItem = AWBSpecialHandlingItems.AddNew();
					specialHandlingItem.JKH_Code = eFreightStatus;
					AWBHeader.AWBSpecialHandlingItems.ValidateAll();
				}
			}

			RefreshSpecialHandlingItems();
		}

		public string GetEFreightStatus()
		{
			var carrier = RefAirline.LoadFromAirlinePrefix(Factory, MasterBillAirlinePrefix);
			return this.EFreightStatus(carrier);
		}

		void RefreshSpecialHandlingItems()
		{
			if (JK_TransportMode == Core.Constants.TransportModes.Air)
			{
				var specialHandlingCodes = GetShipmentSpecialHandlingCodes();

				foreach (var code in specialHandlingCodes)
				{
					if (AWBSpecialHandlingItems.Cast<JobConsolAWBSpecialHandling>().All(hand => hand.JKH_Code != code))
					{
						var specialHandlingItem = AWBSpecialHandlingItems.AddNew();
						specialHandlingItem.JKH_Code = code;
					}
				}

				foreach (var item in AWBSpecialHandlingItems.Cast<JobConsolAWBSpecialHandling>().Where(x => !specialHandlingCodes.Contains(x.JKH_Code)))
				{
					item.Validation.ValidateJKH_Code();
				}

				AWBSpecialHandlingItems.RefreshBinding();
			}
		}

		public List<string> GetShipmentSpecialHandlingCodes()
		{
			return Shipments.Cast<ForwardingShipment>().SelectMany(ship => ship.OuterPackLines)
				.Cast<ForwardingPackLine>().SelectMany(line => line.UNDGs)
				.Cast<UNDGDataItem>().SelectMany(item => ProcessLithiumBatteryAndGetShipmentSpecialHandlingCodes(item))
				.Where(code => !code.IsNullOrEmpty()).Distinct().ToList();
		}

		static string[] ProcessLithiumBatteryAndGetShipmentSpecialHandlingCodes(UNDGDataItem dataItem)
		{
			if (dataItem == null || dataItem.Substance == null)
			{
				return Array.Empty<string>();
			}

			var specialHandlingCodes = new List<string>();
			switch (dataItem.Substance.DG_UNNO)
			{
				case LithiumBatteryConstants.UNNOCodes.LithiumIonBatteries:
					specialHandlingCodes.Add(dataItem.DI_PackingInstructionSection == PackingInstructionSectionTypeList.Codes.SectionII ? LithiumBatteryConstants.SpecialHandlingCodes.EBI : LithiumBatteryConstants.SpecialHandlingCodes.RBI);
					break;
				case LithiumBatteryConstants.UNNOCodes.LithiumMetalBatteries:
					specialHandlingCodes.Add(dataItem.DI_PackingInstructionSection == PackingInstructionSectionTypeList.Codes.SectionII ? LithiumBatteryConstants.SpecialHandlingCodes.EBM : LithiumBatteryConstants.SpecialHandlingCodes.RBM);
					break;
				case LithiumBatteryConstants.UNNOCodes.PackedLithiumIonBatteries:
					specialHandlingCodes.Add(dataItem.DI_PackingInstructionSection == PackingInstructionSectionTypeList.Codes.SectionII ? LithiumBatteryConstants.SpecialHandlingCodes.ELI : LithiumBatteryConstants.SpecialHandlingCodes.RLI);
					break;
				case LithiumBatteryConstants.UNNOCodes.PackedLithiumMetalBatteries:
					specialHandlingCodes.Add(dataItem.DI_PackingInstructionSection == PackingInstructionSectionTypeList.Codes.SectionII ? LithiumBatteryConstants.SpecialHandlingCodes.ELM : LithiumBatteryConstants.SpecialHandlingCodes.RLM);
					break;
				default:
					specialHandlingCodes.AddRange(new String[] { dataItem.Substance.DG_SpecialHandlingCode1, dataItem.Substance.DG_SpecialHandlingCode2, dataItem.Substance.DG_SpecialHandlingCode3 });
					break;
			}

			var caoCodes = GetCAO(dataItem);
			if (caoCodes.Any())
			{
				specialHandlingCodes.AddRange(caoCodes);
			}

			return specialHandlingCodes.ToArray();
		}

		static IEnumerable<string> GetCAO(UNDGDataItem dataItem)
		{
			return RequireCargoAircraftOnly(dataItem)
				? new[] { AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoAircraftOnly }
				: Enumerable.Empty<string>();
		}

		public static bool RequireCargoAircraftOnly(UNDGDataItem dataItem)
		{
			if (dataItem == null || dataItem.Substance == null)
			{
				return false;
			}

			var showCargoAirCraftOnly = false;
			if (dataItem.IsForbiddenForPassengerAircraft())
			{
				showCargoAirCraftOnly = !dataItem.IsForbiddenForCargoAircraft();
			}
			else if ((dataItem.Substance.DG_LQ2OrPaxMaxAmtType == UNDGSubstanceLookups.LimitedQuantityTypes.NLTCode && dataItem.Substance.DG_CargoPackAmtType == UNDGSubstanceLookups.LimitedQuantityTypes.NLTCode)
				|| (dataItem.Substance.DG_LQ2OrPaxMaxAmtType == UNDGSubstanceLookups.LimitedQuantityTypes.NLMCode && dataItem.Substance.DG_LQ2OrPaxMaxAmtUQ.IsEmpty && dataItem.Substance.DG_CargoPackAmtType == UNDGSubstanceLookups.LimitedQuantityTypes.NLMCode && dataItem.Substance.DG_CargoMaxAmtUQ.IsEmpty)
				|| (dataItem.Substance.DG_LQ2OrPaxMaxAmtType == dataItem.Substance.DG_CargoPackAmtType && !dataItem.Substance.DG_LQ2OrPaxMaxAmt.IsEmpty && dataItem.Substance.DG_LQ2OrPaxMaxAmt == dataItem.Substance.DG_CargoMaxAmt && dataItem.Substance.DG_LQ2OrPaxMaxAmtUQ == dataItem.Substance.DG_CargoMaxAmtUQ))
			{
				showCargoAirCraftOnly = false;
			}
			else
			{
				showCargoAirCraftOnly = ExceedsSubstanceWeightOrVolume(dataItem, dataItem.Substance.DG_LQ2OrPaxMaxAmtUQ, dataItem.Substance.DG_LQ2OrPaxMaxAmt);
			}

			return showCargoAirCraftOnly;
		}

		static bool ExceedsSubstanceWeightOrVolume(UNDGDataItem item, ZString maxAmountUnit, ZDecimal maxAmount)
		{
			if (item.DI_PackageCount == 0)
			{
				return false;
			}

			maxAmountUnit = maxAmountUnit.ToUpper();
			if (Weight.ContainsCode(maxAmountUnit))
			{
				return Weight.ConvertSafe(item.DI_DGWeight, item.DI_UnitOfWeight, maxAmountUnit) / item.DI_PackageCount > maxAmount;
			}
			else if (Volume.ContainsCode(maxAmountUnit))
			{
				return Volume.ConvertSafe(item.DI_DGVolume, item.DI_UnitOfVolume, maxAmountUnit) / item.DI_PackageCount > maxAmount;
			}

			return false;
		}

		ZString previousDefaultedSpecialHandling;

		#endregion

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

				var currentTemplateMatchKeys = templateMatches.Matches.Select(m => m.Identifier).ToArray();

				if (lastCustomFieldTemplateMatches == null || !lastCustomFieldTemplateMatches.SequenceEqual(currentTemplateMatchKeys))
				{
					customBusinessObject = null;
					lastCustomFieldTemplateMatches = currentTemplateMatchKeys;
				}

				if (customBusinessObject == null)
				{
					var properties = new UserDefinedPropertyCollection(this);

					AddOrganisationCustomFields(properties);
					properties.Add(templateMatches);

					customBusinessObject = new PhaseSecuritySupportableCustomBusinessObject(Factory, this, properties, this);

					InitialisePhaseDependantCustomBusinessObjectValidation(templateMatches != null);
				}

				return customBusinessObject;
			}
		}
		CustomBusinessObject customBusinessObject;
		ZGuid[] lastCustomFieldTemplateMatches;

		void AddOrganisationCustomFields(UserDefinedPropertyCollection properties)
		{
			var customLabelsProvider = new CustomLabelsProvider(this);
			var list = customLabelsProvider.GetCustomFields(customLabelsProvider.ConfigOrgProvider.ConfigOrg, Factory);
			foreach (CustomLabelInfo field in list)
			{
				if (field.IsEnabled)
				{
					properties.Add(field.PropertyName, field.Caption, field.Position);
				}
			}
		}

		#endregion

		#region Read Only Security

		protected virtual bool GetReadOnlySecurity(PropertyDescriptor property)
		{
			bool result = false;

			if (property.Name == nameof(ShowSubHouseBillShipments))
			{
				return result;
			}

			if (property.Name != ForwardingConsol.Schema.JK_Phase)
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
			return new ConsolPhaseSecurityResolver(this);
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
			return ((child is JobHeader) || (child is IApportionmentListing));
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

		#region IControllerIDProvider Members

		Guid IControllerIDProvider.BusinessObjectPK
		{
			get { return PK.ToGuid(); }
		}

		ControllerID IControllerIDProvider.ControllerID
		{
			get { return ControllerIDs.JobConsol; }
		}

		#endregion

		#region IForwardingConsol Members

		IEnumerable<IForwardingShipment> IForwardingConsol.Shipments
		{
			get { return Shipments.Cast<IForwardingShipment>(); }
		}

		ITransport IForwardingConsol.Transports_AddNew()
		{
			return this.Transports.AddNew();
		}

		ITransport IForwardingConsol.Transports_Get(int index)
		{
			return (index < 0 || index >= this.Transports.Count) ? null : this.Transports[index];
		}

		void IForwardingConsol.AddShipment(IForwardingShipment shipment)
		{
			Shipments.Add((CommonShipment)shipment);
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

			if (!checkingCanCancelRelatedObjects)
			{
				checkingCanCancelRelatedObjects = true;
				try
				{
					foreach (var mawb in GetMAWBs<ICusMAWB>(reloadExistingRows: true, activeOnly: true))
					{
						var canCancelMAWB = mawb.CanCancel();
						if (!string.IsNullOrEmpty(canCancelMAWB))
						{
							return Res.GetString(
								"26A95EE0-8946-4A7E-9776-C9572FE141D4",
								"This record cannot be deactivated as one of its related records cannot be deactivated due to the following reason.") +
							System.Environment.NewLine + mawb.HumanReadableName + ": " + canCancelMAWB;
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

		#endregion

		#region IValidateForCustomsMessagingSupporter

		ZBool IValidateForCustomsMessagingSupporter.SupportValidateCustomsMessaging
		{
			get { return true; }
		}

		BusinessObject IValidateForCustomsMessagingSupporter.GetEntityToValidate(string triggerAction)
		{
			BusinessObject result = null;
			switch (triggerAction)
			{
				case WorkflowTriggerActionTypeConstants.Codes.ReconcileOutturn:
					if (IsAir)
					{
						result = (BusinessObject)AUCusMAWB;
					}
					break;
				case WorkflowTriggerActionTypeConstants.Codes.ValidateForAUCargoMessaging:
					if (IsAir)
					{
						result = (BusinessObject)AUCusMAWB;
					}
					else if (IsSea)
					{
						result = ((IValidateForCustomsMessagingSupporter)AUCMRCusSCAOceanBill)?.GetEntityToValidate(triggerAction);
					}
					break;
			}
			return result;
		}

		#endregion

		ITriggerActionMessagingSupporter ITriggerActionMessagingSupporterProvider.GetSupporter(string triggerType)
		{
			ITriggerActionMessagingSupporter result = null;
			switch (triggerType)
			{
				case WorkflowTriggerActionTypeConstants.Codes.ScheduleOrSendAUCargoMessage:
					if (IsAir)
					{
						result = (ITriggerActionMessagingSupporter)AUCusMAWB;
					}
					else if (IsSea)
					{
						result = (ITriggerActionMessagingSupporter)AUCMRCusSCAOceanBill;
					}
					break;

				case WorkflowTriggerActionTypeConstants.Codes.ScheduleOrSendAUAirCargoOutturnMessage:
					if (IsAir)
					{
						result = (ITriggerActionMessagingSupporter)AUCusMAWB;
					}
					break;
			}
			return result;
		}

		#region ISendEmailSource Members

		AddressBookSelection ISendEmailSource.GetAddressBookSelection()
		{
			AddressBookSelection result = new AddressBookSelection();

			result.AddRecipient(SendingForwarder);
			result.AddRecipient(ReceivingForwarder);
			result.AddRecipient(ShippingLine);
			result.AddRecipient(Creditor);

			return result;
		}

		string ISendEmailSource.EmailSubject
		{
			get { return base.GetEmailSubject(); }
		}

		string ISendEmailSource.TemplateCategory
		{
			get { return MailTemplateCategoryList.Codes.ForwardingConsols; }
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
			get { return ObjectFactory.GetType<DocumentWrappers.IDocForwardingConsol>(); }
		}

		#endregion

		#region IDefaultNumberOfDecimalsSupporter

		protected override int GetDefaultNumberOfDecimalsCore(PropertyDescriptor property)
		{
			return DefaultNumberOfDecimalsSupporterHelperForFreight.GetDefaultNumberOfDecimalsMetaDataProperty(this, property);
		}

		protected override ZString GetUnitOfMeasureCore(PropertyDescriptor property)
		{
			var unitOfMeasure = ZString.Empty;
			var propertyName = DefaultNumberOfDecimals.GetBoundPropertyName(property.Name);

			switch (propertyName)
			{
				case Schema.JK_TotalShipmentActVolumeCheck:
					unitOfMeasure = VolumeVerificationUnit.IsEmpty ? JK_TotalShipmentVolumeUnit : VolumeVerificationUnit;
					break;

				case Schema.JK_TotalShipmentActWeightCheck:
					unitOfMeasure = WeightVerificationUnit.IsEmpty ? JK_TotalShipmentWeightUnit : WeightVerificationUnit;
					break;

				case Schema.JK_TotalShipmentChargableCheck:
					unitOfMeasure = JK_TotalShipmentChargeableUnit;
					break;
				case Schema.JK_MaximumAllowablePackageLength:
				case Schema.JK_MaximumAllowablePackageWidth:
				case Schema.JK_MaximumAllowablePackageHeight:
					unitOfMeasure = JK_MaximumAllowablePackageUnit;
					break;
				default:
					unitOfMeasure = base.GetUnitOfMeasureCore(property);
					break;
			}

			return unitOfMeasure;
		}

		protected override void RoundMeasurePropertiesOnTransportModeChangedCore()
		{
			base.RoundMeasurePropertiesOnTransportModeChangedCore();

			this.SetRoundedValue(JobConsolSchema.JK_TotalShipmentActVolumeCheck, JK_TotalShipmentActVolumeCheckInfo);
			this.SetRoundedValue(JobConsolSchema.JK_TotalShipmentActWeightCheck, JK_TotalShipmentActWeightCheckInfo);
			this.SetRoundedValue(JobConsolSchema.JK_TotalShipmentChargableCheck, JK_TotalShipmentChargableCheckInfo);
		}

		#endregion

		#region IRelatableActivity Members

		ZBool IRelatableActivity.ShouldIgnoreSuperAndSubActivityRelationships
		{
			get { return false; }
		}

		ZString IRelatableActivity.ActivityType
		{
			get { return RelatableActivityTypeList.Codes.Consolidations; }
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
			get { return string.Join("; ", new[] { JK_AgentType, JK_TransportMode, JK_ConsolMode, ZString.Format("{0} > {1}", JK_RL_NKLoadPort, JK_RL_NKDischargePort) }.Where(x => !x.IsEmpty)); }
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

		#region IRequiredTemperature Members

		public ZBool RequiresTemperatureControl
		{
			get { return JK_RequiresTemperatureControl; }
			set { JK_RequiresTemperatureControl = value; }
		}

		public override ZBool JK_RequiresTemperatureControl
		{
			get => base.JK_RequiresTemperatureControl;
			set
			{
				if (base.JK_RequiresTemperatureControl != value)
				{
					base.JK_RequiresTemperatureControl = value;
					this.UpdateCO2eStatusToNotCurrent(new CO2eStatusChangedReason(JK_RequiresTemperatureControlInfo), IsCopying);

					foreach (var container in Containers)
					{
						container.MarkAsNeedingValidation();
					}
				}
			}
		}

		public ZPropertyInfo RequiresTemperatureControlInfo
		{
			get => GetWrappedZPropertyInfo(nameof(RequiresTemperatureControl), x => JK_RequiresTemperatureControlInfo);
		}

		public ZDecimal RequiredTemperatureMaximum
		{
			get { return JK_RequiredTemperatureMaximum; }
			set { JK_RequiredTemperatureMaximum = value; }
		}

		public ZPropertyInfo RequiredTemperatureMaximumInfo
		{
			get => GetWrappedZPropertyInfo(nameof(RequiredTemperatureMaximum), x => JK_RequiredTemperatureMaximumInfo);
		}

		public ZDecimal RequiredTemperatureMinimum
		{
			get { return JK_RequiredTemperatureMinimum; }
			set { JK_RequiredTemperatureMinimum = value; }
		}

		public ZPropertyInfo RequiredTemperatureMinimumInfo
		{
			get => GetWrappedZPropertyInfo(nameof(RequiredTemperatureMinimum), x => JK_RequiredTemperatureMinimumInfo);
		}

		[List(nameof(TemperatureUnits))]
		[MaxLength(1)]
		public ZString RequiredTemperatureUnit
		{
			get { return JK_RequiredTemperatureUnit; }
			set { this.JK_RequiredTemperatureUnit = value; }
		}

		public ZPropertyInfo RequiredTemperatureUnitInfo
		{
			get => GetWrappedZPropertyInfo(nameof(RequiredTemperatureUnit), x => JK_RequiredTemperatureUnitInfo);
		}

		#endregion

		#region ICCACommonAssignmentValidationData Members

		ZString ICCACommonAssignmentValidationData.Name => Res.GetString("f4127038-7ae6-cdba-4a2f-8371c2bfb927", "Consol");

		IOrgHeader ICCACommonAssignmentValidationData.ContractServiceProvider => ShippingLine;

		IRatingContract ICCACommonAssignmentValidationData.CarrierContract => CarrierContract;

		IRatingContractAllocationLine ICCACommonAssignmentValidationData.AllocationRoute => Factory.Load<IRatingContractAllocationLine>(JK_RCA_AllocationLine);

		ZDateTime ICCACommonAssignmentValidationData.ETD => Transports?.MostInterestingTransport?.JW_ETD ?? ZDateTime.Empty;

		ZString ICCACommonAssignmentValidationData.LoadPort => Transports?.MostInterestingTransport?.JW_RL_NKLoadPort ?? ZString.Empty;

		ZString ICCACommonAssignmentValidationData.DischargePort => Transports?.MostInterestingTransport?.JW_RL_NKDiscPort ?? ZString.Empty;

		ZString ICCACommonAssignmentValidationData.VoyageFlight => Transports?.MostInterestingTransport?.JW_VoyageFlight ?? ZString.Empty;

		ZString ICCACommonAssignmentValidationData.Vessel => Transports?.MostInterestingTransport?.JW_Vessel ?? ZString.Empty;

		IEnumerable<IForwardingContainer> ICCACommonAssignmentValidationData.Containers => Containers.Cast<IForwardingContainer>();

		ZString ICCACommonAssignmentValidationData.UniqueConsignRef => JK_UniqueConsignRef;

		ZString ICCACommonAssignmentValidationData.TransportMode => JK_TransportMode;

		#endregion

		ProcessHandlingInfo IProcessHandlingInfoProvider.ProcessHandlingInfo
		{
			get { return new ForwardingConsolProcessHandlingInfo(this); }
		}

		protected override void OnUpdatedByDataRefresh()
		{
			base.OnUpdatedByDataRefresh();
			if (OnUpdatedByDataRefreshForCustomsSynchronisation != null)
			{
				OnUpdatedByDataRefreshForCustomsSynchronisation(this, new EventArgs());
			}
		}

		// Allows consol to tell declaration that it has been refreshed and may no longer be relevant to the declaration
		public event EventHandler OnUpdatedByDataRefreshForCustomsSynchronisation;

		#region Logging

		internal KeyValuePair<string, string>[] GetParametersForEvent(Event evnt)
		{
			var parameters = new Dictionary<string, string>();

			if (evnt == Events.SubscriptionRequested)
			{
				parameters[EventConstants.EventReferenceParameters.Codes.Type] = Constants.EventReferenceParameterTypes.ContainerTracking;
			}

			return parameters.ToArray();
		}

		ZString GetReferenceForEvent(Event evnt)
		{
			return StmALog.GenerateEventReference(string.Empty, GetParametersForEvent(evnt));
		}

		protected override void ProcessLogCore(IStmALog log)
		{
			base.ProcessLogCore(log);

			switch (log.SL_SE_NKEvent)
			{
				case Events.FreightLoadedCode:
				case Events.FreightUnloadedCode:
				case Events.ReceivedCode:
					var handler = new AirlinePartialEventHandler();
					handler.Handle(log, this);
					break;

				case Events.ArrivalCode:
				case Events.DepartureCode:
					if (!log.SL_Reference.StartsWith(NormalEventPropagatedFromShipmentString, StringComparison.OrdinalIgnoreCase))
					{
						FreightEventsHelper.PropagateArivalDepartureEventToMatchedTransportLeg(log, this.Transports);
					}
					break;

				case Events.DataLinkedCode:
					ObjectFactory.Get<ILinkedDocumentMessageProcessorProvider>().Process(this, log);
					break;
			}

			if (this.IsAirTrackingEvent(log))
			{
				this.HandleAirTrackingEvent(log);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "log's reference")]
		const string NormalEventPropagatedFromShipmentString = "Propagated: All Shipments";

		#endregion

		#region Aviation Security

		void UpdateShipmentsForAviationSecurity(string partyType)
		{
			if (!IsSettingDefaultValues && !IsImportingData && IsAir
				&& (SupplyChainSecurityConfiguration.IsEnabled || SupplyChainSecurityConfiguration.UsesGenericScheme))
			{
				foreach (var shipment in Shipments.Cast<ForwardingShipment>()
					.Where(x => x.AviationSecurity.IsAviationSecurityApplicableForTransportMode && x.DepartureConsol == this))
				{
					shipment.AviationSecurityParty_HasChanges(partyType);
				}
			}
		}

		#endregion

		#region Log SPX Special Handling for UK Security

		public bool UserHasVerifiedFreightIsSecure(bool mawbValueMayNotBeSynced = false)
		{
			if (Factory.IsInSaveTransaction)
			{
				return false;
			}

			var verificationChecker = Factory.GetValue<ISecuredFreightVerificationChecker>();
			return verificationChecker?.FreightIsVerifiedToBeSecure(mawbValueMayNotBeSynced) ?? false;
		}

		void TryLogCargoSecureEvent()
		{
			if (!FreightHasBeenVerifiedAsSecure)
			{
				return;
			}

			if (JK_OverrideWaybillDefaults
				&& AWBHeader
					.AWBSpecialHandlingItems
					.OfType<ExportAWBSpecialHandling>()
					.FirstOrDefault(sh => sh.EP_SpecialHandling == AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForPassengerAndAllCargoAircraft) is ExportAWBSpecialHandling awbSpecialHandling)
			{
				GenerateCargoSecureLog(awbSpecialHandling);
			}
			else if (SecurityStatusCodeSpecialHandling != null
				&& SecurityStatusCodeSpecialHandling.JKH_Code == AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForPassengerAndAllCargoAircraft)
			{
				GenerateCargoSecureLog(SecurityStatusCodeSpecialHandling);
			}
		}

		void GenerateCargoSecureLog(SecurityJobConsolAWBSpecialHandling securityConsolSpecialHandling)
		{
			var originalValue = securityConsolSpecialHandling.IsInDatabase
				? (ZString)securityConsolSpecialHandling.JKH_CodeInfo.OriginalValue
				: ZString.Empty;

			GenerateSPXSecurityModifiedEvent(originalValue);
		}

		public void GenerateCargoSecureLog(ExportAWBSpecialHandling mawbSpecialHandling)
		{
			var originalValue = mawbSpecialHandling.IsInDatabase
				? (ZString)mawbSpecialHandling.EP_SpecialHandlingInfo.OriginalValue
				: ZString.Empty;

			GenerateSPXSecurityModifiedEvent(originalValue);
		}

		void GenerateSPXSecurityModifiedEvent(ZString originalValue)
		{
			var parameters = new[]
			{
				new KeyValuePair<string, string>(EventConstants.EventReferenceParameters.Codes.Old, originalValue),
				new KeyValuePair<string, string>(EventConstants.EventReferenceParameters.Codes.New, AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForPassengerAndAllCargoAircraft),
				new KeyValuePair<string, string>(EventConstants.EventReferenceParameters.Codes.Reason, (NoResString)"AWB Special Handling - SPX verified")// log parameters
			};

			Logs.AddNew(Events.SecurityModified, parameters);
		}

		public bool FreightHasBeenVerifiedAsSecure { get; set; }

		#endregion

		#region IContainerTrackingProvider

		bool IContainerTrackingProvider.TransportModeHasChanges
		{
			get { return this.JK_TransportModeInfo.HasChanges; }
		}

		ZString IContainerTrackingProvider.TransportMode
		{
			get { return this.JK_TransportMode; }
		}

		bool IContainerTrackingProvider.CarrierCodeHasChanges
		{
			get { return JK_OA_ShippingLineAddressInfo.HasChanges; }
		}

		bool IContainerTrackingProvider.CoLoadHasChanges
		{
			get
			{
				return JK_CoLoadMasterBillInfo.HasChanges ||
						JK_CoLoadBookingReferenceInfo.HasChanges ||
						JK_OA_CreditorAddressInfo.HasChanges;
			}
		}

		ZString IContainerTrackingProvider.CoLoadWithCarrierBookingReference
		{
			get { return this.JK_CoLoadBookingReference; }
		}

		ZString IContainerTrackingProvider.CoLoadWithMasterBillNumber
		{
			get { return this.JK_CoLoadMasterBill; }
		}

		OrgHeader IContainerTrackingProvider.CoLoadWith
		{
			get { return IsCoLoad ? this.Creditor : null; }
		}

		bool IContainerTrackingProvider.MasterBillNumberHasChanges
		{
			get { return this.JK_MasterBillNumInfo.HasChanges; }
		}

		ZString IContainerTrackingProvider.MasterBillNumber
		{
			get { return this.JK_MasterBillNum; }
		}

		bool IContainerTrackingProvider.CarrierBookingReferenceHasChanges
		{
			get { return this.JK_BookingReferenceInfo.HasChanges; }
		}

		ZString IContainerTrackingProvider.CarrierBookingReference
		{
			get { return this.JK_BookingReference; }
		}

		bool IContainerTrackingProvider.ContainerModeHasChanges
		{
			get { return this.JK_ConsolModeInfo.HasChanges; }
		}

		ZString IContainerTrackingProvider.ContainerMode
		{
			get { return this.JK_ConsolMode; }
		}

		IEnumerable<ITrackableContainer> IContainerTrackingProvider.Containers
		{
			get { return this.Containers.Cast<ITrackableContainer>(); }
		}

		bool IContainerTrackingProvider.SubscribeToContainersOnly
		{
			get { return this.IsCoLoad; }
		}

		bool IContainerTrackingProvider.SubscribeToContainersOnlyHasChanges
		{
			get { return this.JK_AgentType != (ZString)JK_AgentTypeInfo.OriginalValue; }
		}

		ZDateTime IContainerTrackingProvider.GetLastArrivalDate()
		{
			var arrivalTransport = Transports.ArrivalTransport;
			if (arrivalTransport != null)
			{
				return arrivalTransport.JW_ATA.IsValid && !arrivalTransport.JW_ATA.IsEmpty
					? arrivalTransport.JW_ATA
					: arrivalTransport.JW_ETA;
			}

			return ZDateTime.Empty;
		}

		ZDateTime IContainerTrackingProvider.GetFirstDepartureDate()
		{
			var departureTransport = Transports.DepartureTransport;
			if (departureTransport != null)
			{
				return departureTransport.JW_ATD.IsValid && !departureTransport.JW_ATD.IsEmpty
					? departureTransport.JW_ATD
					: departureTransport.JW_ETD;
			}

			return ZDateTime.Empty;
		}

		#endregion

		#region ICusInBondParent Members

		event EventHandler ICusInBondParent.VisibilityChanged
		{
			add { JK_TransportModeInfo.ValueChanged += value; }
			remove { JK_TransportModeInfo.ValueChanged -= value; }
		}

		ZGuid ICusInBondParent.GetDeclarationPK(ZGuid companyPK)
		{
			return ZGuid.Empty;
		}

		void ICusInBondParent.PopulateJobNumberIfNeeded()
		{
		}

		ZGuid ICusInBondParent.PK
		{
			get { return PK; }
		}

		string ICusInBondParent.TablePrefix
		{
			get { return TablePrefix; }
		}

		ZString ICusInBondParent.ParentType
		{
			get { return Res.GetString("487AD65F-16B2-4A72-98F8-F791A8ABE4DF", "Consol"); }
		}

		ZString ICusInBondParent.JobNumber
		{
			get { return ZString.Empty; }
		}

		ZString ICusInBondParent.HouseBill
		{
			get { return ZString.Empty; }
		}

		bool ICusInBondParent.IsVisible
		{
			get { return true; }
		}

		bool ICusInBondParent.IsInternalBrokerage
		{
			get { return false; }
		}

		#endregion

		public US.InBond.ICusInBondHeader InBondHeader
		{
			get { return fInBondHeader ?? (fInBondHeader = (US.InBond.ICusInBondHeader)this.GetInBondHeader(Customs.Common.CusInBondApplicationCodeList.Codes.InBond, false)); }
		}

		US.InBond.ICusInBondHeader fInBondHeader;

		#region ICusAddInfoTypeSupporter Members
		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return (IBusinessObjectFetchStrategy)Activator.CreateInstance(ObjectFactory.GetType<ICusAddInfoTypeSupporterFetchStrategy>(), this);
		}

		IDictionary<ZString, Type> ICusAddInfoTypeSupporter.GetCusAddInfoTypes()
		{
			var result = new Dictionary<ZString, Type>();
			result.Add(CusAddInfoTypeAttribute.Codes.GbMawbExport, ObjectFactory.Get<GB.GBChief.ICusAddInfoImplementer>().GetMawbExportAddInfoType());
			return result;
		}

		public static class CusAddInfoTypeAttribute
		{
			public static class Codes
			{
				public const string GbMawbExport = "GBM";
			}
		}

		#endregion

		#region SupplyChainSecurityConfiguration

		internal SupplyChainSecurityConfiguration SupplyChainSecurityConfiguration
		{
			get { return supplyChainSecurityConfiguration ?? (supplyChainSecurityConfiguration = SupplyChainSecurityConfiguration.New()); }
		}
		SupplyChainSecurityConfiguration supplyChainSecurityConfiguration;

		internal SupplyChainSecurityConfiguration DestinationSupplyChainSecurityConfiguration
		{
			get
			{
				var countryCode = JK_RL_NKDischargePort.SubstringSafe(0, 2);
				var cacheKey = "ForwardingConsol.DestinationSupplyChainSecurityConfiguration." + countryCode;
				return Factory.GetCachedValue(cacheKey, delegate
				{
					return SupplyChainSecurityConfiguration.New(countryCode);
				});
			}
		}

		#endregion

		#region INZManifestHeader Implementation
		ZString INZManifestHeader.JobName => (NoResString)"Consol"; // Only for program, not to show to the user.

		ZString INZManifestHeader.DocumentParentType => Constants.DocManagerCodes.Consol;

		Logs INZManifestHeader.Logs => Logs;

		ZString INZManifestHeader.MasterBillNumber => JK_MasterBillNum;
		ZString INZManifestHeader.JobNumber => JK_UniqueConsignRef;

		CusEntryNumber INZManifestHeader.LoadAndCreateRegistrationNumber()
		{
			var entryNumber = CusEntryNumber.LoadOrCreate(this, "ORN", GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			return entryNumber;
		}
		#endregion

		#region IGateway

		List<ZGuid> IGateway.SortedGatewayAgentPKs => new List<ZGuid>();

		List<ZGuid> IGateway.SortedControllingCustomerPKs => new List<ZGuid>();

		List<ZGuid> IGateway.GatewayAgentPKsForIntercompanyTariff => new List<ZGuid>();

		IDictionary<ZString, IList<ZGuid>> IGateway.LoginGatewayAgentRoles => new Dictionary<ZString, IList<ZGuid>>();

		ZString IGateway.GatewayLoginRole => ZString.Empty;

		List<ILocation> IGateway.SortedOverridenPlannedLoad => new List<ILocation>();

		List<ILocation> IGateway.SortedOverridenPlannedDischarge => new List<ILocation>();

		ZBool IGateway.IsGatewaySellApplicableToGatewayConsol(CostSell costSell) => true;

		ZBool IGateway.IsContainerNegotiatedCostApplicable(CostSell costSell) => true;

		bool IGateway.IsIntercompanyTariffApplicable(BillingType billingType, CostSell costSell) => false;

		bool IGateway.ContinueWithDefaultCosting(BillingType billingType) => true;

		ZString IGateway.GatewayAgentTypeFilteredReason(string agentType, ZGuid gatewayAgentPk, BillingType billingType, CostSell costSell) => ZString.Empty;

		ZString IGateway.GatewayServiceLevelFilteredReason(ZString gatewayServiceLevel, ZGuid gatewayAgentPk)
		{
			if (gatewayServiceLevel.IsEmpty || gatewayServiceLevel == GetStandardServiceLevel(JK_RS_NKGatewayServiceLevel))
			{
				return ZString.Empty;
			}

			return Res.GetString("09838A5C-9030-4FFA-BAD5-C714119D2C22", "Gateway Service Level '{0}' cannot be used for consol autorating", gatewayServiceLevel);

			ZString GetStandardServiceLevel(ZString serviceLevel)
				=> serviceLevel.IsEmpty ? new ZString("STD") : serviceLevel;
		}

		ZString IGateway.ShipmentGatewayServiceLevel => ZString.Empty;

		public ZBool ShouldRemoveNonIntercompanyTariffFRTEntries(BillingType billingType) => false;

		IGatewayBillingSupporter IGateway.GatewayBillingSupporter => gatewayBillingSupporter ?? (gatewayBillingSupporter = new ForwardingConsolGatewayBillingSupporter(this));
		ForwardingConsolGatewayBillingSupporter gatewayBillingSupporter;

		#endregion

		#region IDtbBookingParent members

		public ZString JobNumber
		{
			get { return JK_UniqueConsignRef; }
		}

		public ZString JobDescription
		{
			get { return Res.GetString("dd1f2606-5bdf-4095-956f-954481dbd854", "Consol"); }
		}

		public ZString JobStatus
		{
			get { return JK_ConsolStatus; }
		}

		public ZQuery TransportBookingTemplateFilters => new ZQuery();

		public ZBool IsSupportsDirectSchedule
		{
			get { return false; }
		}

		public ZString JobType
		{
			get { return ((IWorkflowProvider)this).WorkflowType; }
		}

		public ZString JobTypeDescription
		{
			get { return Res.GetString("4e530e0d-53e9-4a8c-9a55-9a2bc8a0abb7", "Consol"); }
		}

		public DtbBookingDirection[] GetSupportedDirections()
		{
			return new DtbBookingDirection[] { DtbBookingDirection.PIC, DtbBookingDirection.DLV };
		}

		public IJobInvoicingPlugIn InvoicingJob
		{
			get { return this; }
		}

		public bool RequiresMultiContainerBooking => true;

		public bool CanCreateTransportBooking => true;

		public void TransportBookingCreatedOrUpdated(IEnumerable<IDtbBooking> bookings)
		{
			if (Shipments.Cast<ForwardingShipment>().Any(s => s.HaveJobCO2e))
			{
				Shipments.ForEach(shipment => ((ICO2eCalculationSupporter)shipment).UpdateCO2eStatusToNotCurrent());
				ZExceptionReporting.ProcessWithSaveExceptionHandling(Factory.Save, null, true);
			}
		}

		public ZGuid BookingParentPK => PK;

		public string BookingParentTablePrefix => TablePrefix;

		public (bool isShouldShow, string caption, string message, string confirmation) GetExtendingConfirmMessageBeforeCreateTransportBooking() => (false, null, null, null);

		#endregion

		public bool IsLegacyGateway => ((ForwardingConsolGatewayBillingSupporter)((IGateway)this).GatewayBillingSupporter).IsLegacyGateway;

		#region Aviation Security Code

		internal ZString GetAviationSecurityCode()
		{
			if (IsAir && SupplyChainSecurityConfiguration.IsExportForAviationSecurityPurposes(this))
			{
				var euAirTransports = Transports.Cast<Transport>().Where(x => x.IsAir && x.DiscPort != null && x.DiscPort.IsInEU);
				if (SupplyChainSecurityConfiguration.IsEnabled || euAirTransports.Any())
				{
					var transportsWhereCargoValidationApplies = Transports.Cast<Transport>().Where(x => x.IsAir);
					var transportsToCheckForCargoOnly = euAirTransports.Union(transportsWhereCargoValidationApplies);
					if (transportsToCheckForCargoOnly.Any() && Shipments.Any())
					{
						if (SupplyChainSecurityConfiguration.UseConsignmentSecurityDeclaration && AreAllShipmentsApprovedForAviationSecurity)
						{
							return GetAviationSecurityCodeWhereAllShipmentsAreApprovedForAviationSecurity();
						}

						return AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoHasNotBeenSecuredYetForPassengerOrAllCargoAircraft;
					}
				}
			}

			return ZString.Empty;
		}

		ZString GetAviationSecurityCodeWhereAllShipmentsAreApprovedForAviationSecurity()
		{
			if (Shipments.Cast<ForwardingShipment>().All(shipment => shipment.AviationSecurity.IsHighRiskShipment))
			{
				return SupplyChainSecurityConfiguration.AllowDefaultingCargoSecureForPassengerAndAllCargoAircraft(this)
					? (ZString)AWBSpecialHandlingCodeDescriptionPairList.Codes.SecureForPassengerAllCargoAndAllMailAircraftInAccordanceWithHighRiskRequirements
					: ZString.Empty;
			}
			else if (Shipments.Cast<ForwardingShipment>().Any(shipment => !shipment.AviationSecurity.IsAllowedOnPassengerFlights()
				|| (shipment.JS_InspectionTypeCode == BaseJobShipmentLookups.InspectionType_Approved
					&& !shipment.AviationSecurity.RelevantOrganisationsAreApprovedForShippingOnPassengerFlights)))
			{
				return SupplyChainSecurityConfiguration.GetCargoSecureForAllCargoAircraftOnlyIsAllowed(this)
					? AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForAllCargoAircraftOnly
					: AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoHasNotBeenSecuredYetForPassengerOrAllCargoAircraft;
			}

			return SupplyChainSecurityConfiguration.AllowDefaultingCargoSecureForPassengerAndAllCargoAircraft(this)
				? (ZString)AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForPassengerAndAllCargoAircraft
				: ZString.Empty;
		}

		#endregion

		#region Dangerous Goods Restrictions

		public override ZBool JK_IsHazardous
		{
			get => base.JK_IsHazardous;
			set
			{
				base.JK_IsHazardous = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateJK_CarrierContractNumber();
				}
			}
		}

		public bool JK_IsHazardous_ReadOnly => !Env.Security.MaintainConsolAllowEditOfAcceptedDangerousGoodsOnConsol.IsAllowed;

		[ChildEditable(true)]
		public ConsolDGRestrictionsCollection ConsolDGRestrictionCollection
		{
			get
			{
				if (consolDGRestrictionsCollection == null)
				{
					consolDGRestrictionsCollection = new ConsolDGRestrictionsCollection(this);
					RegisterEditableChildObject(consolDGRestrictionsCollection);
					consolDGRestrictionsCollection.CollectionCountChange += ConsolDGRestrictionCollection_CountChanged;
				}
				return consolDGRestrictionsCollection;
			}
		}

		ConsolDGRestrictionsCollection consolDGRestrictionsCollection;

		void ConsolDGRestrictionCollection_CountChanged(object sender, CollectionCountChangedEventArgs args)
		{
			if (!IsValidationSuspended)
			{
				Validation.ValidateJK_IsHazardous();
			}
		}

		public override bool AllowsForShipmentsDangerousGoods(CommonShipment shipment)
		{
			if (shipment == null)
			{
				return true;
			}

			if (JK_IsHazardous)
			{
				if (ConsolDGRestrictionCollection.Count == 0)
				{
					return true;
				}

				return ShipmentDangerousGoodsIsCompatible(shipment);
			}
			return !shipment.IsHazardous;
		}

		internal bool ShipmentDangerousGoodsIsCompatible(CommonShipment shipment)
		{
			var dangerousGoods = shipment.OuterPackLines.OfType<ForwardingPackLine>().SelectMany(p => p.UNDGs).ToArray();
			foreach (var substance in dangerousGoods)
			{
				if (substance.DI_IMOClass.IsEmpty && substance.DI_DG.IsEmpty)
				{
					continue;
				}

				if (!ConsolDGRestrictionCollection.Any(dg => (substance.Substance != null && string.Equals(dg.JKD_Calc_Substance, substance.Substance.DG_Code, StringComparison.OrdinalIgnoreCase))
					|| (dg.JKD_Class == substance.DI_IMOClass && (dg.JKD_Calc_Substance.IsEmpty || (substance.Substance != null && string.Equals(dg.JKD_Calc_Substance, substance.Substance.DG_Code, StringComparison.OrdinalIgnoreCase))))))
				{
					return false;
				}
			}

			return true;
		}

		#endregion

		#region Shipments with Lithium Batteries

		public override bool AllowsForShipmentsLithiumBatteries(CommonShipment shipment)
		{
			var hasPassengerFlight = Transports.OfType<Transport>().Any(transport => transport.TransportMode == Constants.TransportModes.Air && !transport.JW_IsCargoOnly);
			if (!hasPassengerFlight)
			{
				return true;
			}

			var hasLithiumBattery = shipment.OuterPackLines.OfType<PackLine>().Any(packLine =>
			{
				return packLine.UNDGs.Any(dangerousGood => (
					(dangerousGood.Substance?.DG_Code ?? ZString.Empty) == LithiumBatteryConstants.UNNOCodes.LithiumMetalBatteries
				 || (dangerousGood.Substance?.DG_Code ?? ZString.Empty) == LithiumBatteryConstants.UNNOCodes.LithiumIonBatteries
				));
			});

			return !hasLithiumBattery;
		}

		public override bool AllShipmentsContainPermissibleQuantities(IEnumerable<CommonShipment> shipments)
		{
			if (shipments == null)
			{
				return true;
			}
			var forwardingShipments = shipments.Cast<ForwardingShipment>();
			return ForwardingUNDGPermissableQuantitiesHelper.AreRelatedShipmentsLithiumBatteriesPermissibleForPacking(forwardingShipments);
		}

		#endregion

		#region Temperature Controlled Cargo

		public override bool ShipmentTemperatureRangeIsValid(CommonShipment shipment)
		{
			var packLinesRequiringTemperatureControl = shipment
				.OuterPackLines
				.OfType<ForwardingPackLine>()
				.Where(p => p.JL_RequiresTemperatureControl)
				.ToList();

			if (packLinesRequiringTemperatureControl.Count > 0)
			{
				if (JK_RequiresTemperatureControl
					&& TemperatureUnits.ContainsCode(JK_RequiredTemperatureUnit)
					&& packLinesRequiringTemperatureControl.All(p => p.TemperatureUnits.ContainsCode(p.JL_RequiredTemperatureUnit)))
				{
					var arrayOfWhetherEachPkLineIsQualifiedInShipment = packLinesRequiringTemperatureControl.ConvertAll(p =>
					{
						var packLineMinTemp = decimal.Round(Temperature.Convert(p.JL_RequiredTemperatureMinimum, p.JL_RequiredTemperatureUnit, JK_RequiredTemperatureUnit), 1);
						var packLineMaxTemp = decimal.Round(Temperature.Convert(p.JL_RequiredTemperatureMaximum, p.JL_RequiredTemperatureUnit, JK_RequiredTemperatureUnit), 1);
						return packLineMinTemp <= JK_RequiredTemperatureMinimum && packLineMaxTemp >= JK_RequiredTemperatureMaximum;
					}).ToArray();

					return Array.IndexOf(arrayOfWhetherEachPkLineIsQualifiedInShipment, false) == -1;
				}
				else
				{
					return false;
				}
			}

			return true;
		}

		#endregion

		#region Default Charges Apply

		public ZString GetDefaultChargesApply()
		{
			var firstSet = Env.Registry.Freight.AirWaybill.PrintAsAgreedOnFirstSetMAWB;
			var secondSet = Env.Registry.Freight.AirWaybill.PrintAsAgreedOnSecondSetMAWB;

			return ChargesApplyHelper.GetDefaultChargesApply(firstSet, secondSet);
		}

		#endregion

		#region Booking Reference Number Split

		public static ReadOnlyCollection<char> BookingReferenceSeperatorCharacters => new ReadOnlyCollection<char>(new char[] { ' ', ';', ':', ',' });

		public void SplitBookingReferenceNumbersIntoAdditionalReferenceCollection()
		{
			if (string.IsNullOrEmpty(JK_BookingReference))
			{
				return;
			}

			var listOfUniqueBookingNumbers = JK_BookingReference.Split(BookingReferenceSeperatorCharacters.ToArray()).Distinct().Where(bookingNumber => !string.IsNullOrWhiteSpace(bookingNumber)).ToList();
			JK_BookingReference = listOfUniqueBookingNumbers.FirstOrDefault();

			foreach (var bookingNumber in listOfUniqueBookingNumbers.Where(x => x != JK_BookingReference))
			{
				Numbers.AddNewIfNotExist(CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.BKG, bookingNumber);
			}
		}

		public bool ReferenceNumberShouldBeSplitIntoNumbers(ZString referenceNumber)
		{
			return JK_TransportMode == Core.Constants.TransportModes.Sea && !string.IsNullOrEmpty(referenceNumber) && referenceNumber.IndexOfAny(BookingReferenceSeperatorCharacters.ToArray()) != -1;
		}

		#endregion

		#region ITransitWarehouseInstructionSupporter

		OrgAddress ITransitWarehouseInstructionSupporter.PickupTransitWarehouse => PackDepotAddress;

		OrgAddress ITransitWarehouseInstructionSupporter.DeliveryTransitWarehouse => UnpackDepotAddress;

		ZDateTime ITransitWarehouseInstructionSupporter.PickupReceiptRequestedDate
		{
			get => JK_PackDepotReceiptRequested;
			set => JK_PackDepotReceiptRequested = value;
		}

		ZDateTime ITransitWarehouseInstructionSupporter.PickupDispatchRequestedDate
		{
			get => JK_PackDepotDispatchRequested;
			set => JK_PackDepotDispatchRequested = value;
		}

		ZDateTime ITransitWarehouseInstructionSupporter.DeliveryReceiptRequestedDate
		{
			get => JK_UnpackDepotReceiptRequested;
			set => JK_UnpackDepotReceiptRequested = value;
		}

		ZDateTime ITransitWarehouseInstructionSupporter.DeliveryDispatchRequestedDate
		{
			get => JK_UnpackDepotDispatchRequested;
			set => JK_UnpackDepotDispatchRequested = value;
		}

		ZString ITransitWarehouseInstructionSupporter.PickupDescription => Res.GetString("7117f9be-293b-4303-a966-18e095a1c196", "Departure");

		ZString ITransitWarehouseInstructionSupporter.DeliveryDescription => Res.GetString("a12f2200-49f8-47ba-8c75-8c5f61778843", "Arrival");

		ZString ITransitWarehouseInstructionSupporter.TransitWarehouseDescription => Res.GetString("05ea65a0-be98-4b3f-8c5a-3465753ddf39", "CFS");

		#endregion

		#region ConsolDashboard

		ConsolDashboardCalculationWrapper calculationWrapper;
		public ConsolDashboardCalculationWrapper CalculationWrapper => calculationWrapper ?? (calculationWrapper = new ConsolDashboardCalculationWrapper(this));

		#endregion

		#region Template Record

		public bool IsTemplateRecord { get; set; }

		public ZBool IsTemplate => IsTemplateRecord;

		ITemplateRecord ITemplateRecordProvider.TemplateRecord
		{
			get => TemplateRecord;
			set => TemplateRecord = (StmTemplateRecord)value;
		}

		/// This handles two scenarios:
		/// a) Using a template record which is persisted through STR_Data
		/// b) Loading a template record from a Consol which was created from a template record
		public new StmTemplateRecord TemplateRecord
		{
			get
			{
				if (IsDeleted)
				{
					return null;
				}

				if (IsTemplateRecord)
				{
					return templateRecord;
				}
				else
				{
					templateRecordUsedToCreateConsol = Factory.Load<StmTemplateRecord>(JK_STR);
					templateRecordUsedToCreateConsol?.RegisterEditableChildObject(this);

					return templateRecordUsedToCreateConsol;
				}
			}
			set
			{
				if (IsTemplateRecord)
				{
					templateRecord?.UnRegisterEditableChildObject(this);
					templateRecord = value;
					templateRecord?.RegisterEditableChildObject(this);
				}
				else
				{
					templateRecordUsedToCreateConsol?.UnRegisterEditableChildObject(this);
				}

				JK_STR = value.PK;
			}
		}

		StmTemplateRecord templateRecord;
		StmTemplateRecord templateRecordUsedToCreateConsol;

		void ITemplateRecordProvider.SaveToTemplateRecord()
		{
			if (!IsDeleted && TemplateRecord != null && !TemplateRecord.IsDeleted)
			{
				WriteToTemplateRecord(TemplateRecord);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "XML header")]
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
			var consol = writer.GetDataObject(this);

			var stream = (SubStreamableStream)new MemoryStream();
			try
			{
				var xmlWriter = ObjectFactory.Get<IXmlWriter>();
				xmlWriter.WriteXML(consol, stream);

				using (var reader = new StreamReader(stream))
				{
					stream = null;

					var xmlAsString = reader.ReadToEnd();
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

			targetTemplateRecord.STR_ModuleID = nameof(ModuleId.JobConsol);
			targetTemplateRecord.PopulateIdIfNeeded();
			if (JK_UniqueConsignRef.IsEmpty)
			{
				JK_UniqueConsignRef = targetTemplateRecord.STR_ReferenceId;
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

			var consolDataObject = new Shipment(DefaultDataObjectWriterStrategy.Instance);
			using (var stream = (SubStreamableStream)new MemoryStream(Encoding.UTF8.GetBytes(sourceTemplateRecord.STR_Data)))
			{
				ObjectFactory.Get<IXmlReader>().ReadXML(consolDataObject, stream, logger);
			}

			var manager = this.GetUniversalDataContextManager() as IShipmentDataContextManagerInternal
				?? throw new InvalidOperationException("Unsupported Data Context Manager");

			var universalObjectFactory = new UniversalObjectFactory(Factory);
			var reader = manager.GetShipmentDataObjectReader(consolDataObject, logger, universalObjectFactory)
				?? throw new InvalidOperationException(
					FormattableString.Invariant(
						$"{manager.GetType().FullName} returned <null> reader for Consol Template Record {sourceTemplateRecord.STR_ReferenceId}"
					)
				);

			IsTemplateRecord = true;
			TemplateRecord = sourceTemplateRecord;

			var templateRecordReader = reader as ITopLevelDataObjectReaderForTemplateRecord;
			if (templateRecordReader == null)
			{
				ErrorReporter.ReportOnce(
					FormattableString.Invariant(
						$"{manager.GetType().FullName} returned {reader.GetType().FullName} which does not implement {nameof(ITopLevelDataObjectReaderForTemplateRecord)} to read Shipment Template Record {sourceTemplateRecord.STR_ReferenceId}."
					)
				);

				BusinessObject bizo = this;
				reader.ReadIntoBusinessObject(ref bizo);
			}
			else
			{
				templateRecordReader.ReadDirectlyIntoBusinessObject(this);
			}

			if (JK_UniqueConsignRef.IsEmpty)
			{
				JK_UniqueConsignRef = sourceTemplateRecord.STR_ReferenceId;
			}

			if (sourceTemplateRecord.IsInDatabase)
			{
				((IBusinessObjectState)this).ClearHasChangesIncludingChildren();
				((INeedRow)this).Row.AcceptChanges();
			}
		}

		public BusinessObject InstantiateFromTemplateRecord(BusinessObjectFactory factory, Type elementType, ITemplateRecord otherTemplateRecord)
		{
			var otherElement = factory.New(elementType);
			var otherTemplateRecordProvider = (ITemplateRecordProvider)otherElement;
			otherTemplateRecordProvider.LoadFromTemplateRecord(otherTemplateRecord);
			return otherElement;
		}

		#endregion

		#region IComplianceItemRiskStatusProvider

		IEnumerable<IScreeningParty> ICompliancePartyRiskStatusProvider.Parties
		{
			get
			{
				FetchForPartiesComplianceSynchronization();

				var parties = GetScreeningPartiesCommon().ToList<IScreeningParty>();

				parties.Add(new ScreeningParty(this, Res.GetString("f7f2ab00-b452-4cf0-86a6-3667798bf9c6", "Departure Port Transport"), DeparturePackCFSTransport));
				parties.Add(new ScreeningParty(this, Res.GetString("c6ebd7d4-af8f-4085-bc0a-7ab38a653076", "Arrival Port Transport"), ArrivalUnpackCFSTransport));

				foreach (Transport transport in Transports)
				{
					parties.AddRange(transport.GetScreeningPartiesFromVessel(this));
				}

				foreach (ForwardingContainer container in Containers)
				{
					var emptyPickupFrom = Factory.Load<OrgHeader>(container.JC_Calc_DepartureContainerYardAddressOrg);
					parties.Add(new ScreeningParty(this, Res.GetString("4c0c24a-44cd-4170-aee5-c90dd2e2fe45", "Empty Pickup From"), emptyPickupFrom));

					var emptyReturnTo = Factory.Load<OrgHeader>(container.JC_Calc_ArrivalContainerYardAddressOrg);
					parties.Add(new ScreeningParty(this, Res.GetString("6b27304e-20dd-4b7f-8554-ff42f0d5f919", "Empty Return To"), emptyReturnTo));

					var warehouseParties = container.PackLines.Cast<PackLine>()
						.SelectMany(packLine => packLine.PackLocations).Cast<PackLocation>()
						.Select(packLocation => new ScreeningParty(this, Res.GetString("bdb1b444-c2cd-47af-acaf-c743693b6e0c", "Warehouse"), (OrgHeader)packLocation.WhsLocation?.Warehouse?.WarehouseAddress?.Header));
					parties.AddRange(warehouseParties);

					var services = container.Services.Cast<JobService>();
					var contractorParties = services.Select(service => new ScreeningParty(this, Res.GetString("63a0eab3-d279-4d1c-9443-276c08da337b", "Contractor"), service.Contractor));
					parties.AddRange(contractorParties);
					var serviceLocationParties = services.Select(service => new ScreeningParty(this, Res.GetString("a61b8083-2082-44f9-8820-b7fff4f4f81f", "Service Location"), service.Location?.Header));
					parties.AddRange(serviceLocationParties);
				}

				var costQuery = new ZQuery(JobConsolCostSchema.E6_ParentID, PK);
				costQuery.AddToFilter(JobConsolCostSchema.E6_ParentTableCode, JobConsolSchema.Constants.Prefix);
				costQuery.AddToFilter(JobConsolCostSchema.E6_GC, GlbCompany.CurrentCompany.PK);
				var costs = Factory.Load<IJobConsolCost>(costQuery).Cast<BusinessObject>();

				foreach (var cost in costs)
				{
					var creditorOrg = Factory.Load<OrgHeader>((ZGuid)cost[JobConsolCostSchema.E6_OH_Creditor]);
					parties.Add(new ScreeningParty(this, Res.GetString("76894835-5170-4d97-9cf7-67620829e000", "Consol Costing Creditor"), creditorOrg));
				}

				foreach (ForwardingShipment shipment in Shipments)
				{
					var partiesFromShipments = ((ICompliancePartyRiskStatusProvider)shipment).Parties;
					foreach (ScreeningParty party in partiesFromShipments)
					{
						party.AddParent(this);
					}
					parties.AddRange(partiesFromShipments);
				}

				return parties;
			}
		}

		IEnumerable<IComplianceLocation> IComplianceLocationRiskStatusProvider.Locations
		{
			get
			{
				FetchForLocationsComplianceSynchronization();

				var countries = new List<IComplianceLocation>();
				AddCountryToList(this, (NoResString)"First Load Country", LoadPort?.Country);
				AddCountryToList(this, (NoResString)"Last Discharge Country", DischargePort?.Country);

				foreach (Transport transport in Transports)
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

				AddCountryToList(this, (NoResString)"First Foreign Country", FirstForeignPort?.Country);
				AddCountryToList(this, (NoResString)"Last Foreign Country", LastForeignPort?.Country);
				AddCountryToList(this, (NoResString)"First Arrival Country", PortOfFirstArrival?.Country);

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

				foreach (IComplianceLocationRiskStatusProvider shipment in Shipments)
				{
					var countriesFromShipments = shipment.Locations;
					foreach (ScreeningParty country in countriesFromShipments)
					{
						country.AddParent(this);
					}
					countries.AddRange(countriesFromShipments);
				}

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

				foreach (IComplianceCommodityRiskStatusProvider shipment in Shipments)
				{
					commodities.AddRange(shipment.Commodities);
					commodities.AddRange(ObjectFactory.Get<IComplianceRiskStatusSupporter>().FetchCommodityDetailsInDB(shipment));
				}

				return commodities;
			}
		}

		bool hasFetchedPartiesForComplianceSynchronization;

		void FetchForPartiesComplianceSynchronization()
		{
			if (!hasFetchedPartiesForComplianceSynchronization && !IsDeleted)
			{
				hasFetchedPartiesForComplianceSynchronization = true;

				Factory.AddFetchHint(JobShipmentSchema.Instance, new ZQuery(JobShipmentSchema.JS_JS_ColoadMasterShipment, Shipments.Select(s => s.PK)));

				var orgAddressPKs = new HashSet<ZGuid>();
				var orgPKs = new HashSet<ZGuid>();

				orgAddressPKs.Add(JK_OA_DeparturePackCFSTransportAddress);
				orgAddressPKs.Add(JK_OA_ArrivalUnpackCFSTransportAddress);

				foreach (ForwardingContainer container in Containers)
				{
					orgAddressPKs.Add(container.JC_OA_DepartureContainerYardAddress);
					orgAddressPKs.Add(container.JC_OA_ArrivalContainerYardAddress);

					var addressPKs = container.PackLines.Cast<PackLine>()
						.SelectMany(packLine => packLine.PackLocations).Cast<PackLocation>()
						.Select(packLocation => packLocation.WhsLocation?.Warehouse?.WW_OA_WarehouseAddress)
						.Where(address => address != null);

					foreach (var addressPK in addressPKs)
					{
						orgAddressPKs.Add(addressPK.Value);
					}

					foreach (JobService service in container.Services)
					{
						orgPKs.Add(service.ES_OH_Contractor);
						orgAddressPKs.Add(service.ES_OA_Location);
					}
				}

				var costQuery = new ZQuery(JobConsolCostSchema.E6_ParentID, PK);
				costQuery.AddToFilter(JobConsolCostSchema.E6_ParentTableCode, JobConsolSchema.Constants.Prefix);
				costQuery.AddToFilter(JobConsolCostSchema.E6_GC, GlbCompany.CurrentCompany.PK);
				var costs = Factory.Load<IJobConsolCost>(costQuery).Cast<BusinessObject>();

				foreach (var cost in costs)
				{
					orgPKs.Add((ZGuid)cost[JobConsolCostSchema.E6_OH_Creditor]);
				}

				#region ScreeningPartiesCommon

				foreach (var docAddress in DocAddresses.Cast<JobDocAddress>())
				{
					orgAddressPKs.Add(docAddress.E2_OA_Address);
				}

				if (Job != null)
				{
					orgAddressPKs.Add(Job.JH_OA_LocalChargesAddr);
				}

				orgAddressPKs.Add(JK_OA_SendingForwarderAddress);
				orgAddressPKs.Add(JK_OA_ReceivingForwarderAddress);
				orgAddressPKs.Add(JK_OA_ShippingLineAddress);
				orgAddressPKs.Add(JK_OA_CreditorAddress);
				orgAddressPKs.Add(JK_OA_DepartureCTOAddress);
				orgAddressPKs.Add(JK_OA_PackDepotAddress);
				orgAddressPKs.Add(JK_OA_ContainerYardEmptyPickupAddress);
				orgAddressPKs.Add(JK_OA_ArrivalCTOAddress);
				orgAddressPKs.Add(JK_OA_UnpackDepotAddress);
				orgAddressPKs.Add(JK_OA_ContainerYardEmptyReturnAddress);

				#endregion

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

		bool hasFetchedLocationsForComplianceSynchronization;

		void FetchForLocationsComplianceSynchronization()
		{
			if (!hasFetchedLocationsForComplianceSynchronization && !IsDeleted)
			{
				hasFetchedLocationsForComplianceSynchronization = true;

				Factory.AddFetchHint(JobShipmentSchema.Instance, new ZQuery(JobShipmentSchema.JS_JS_ColoadMasterShipment, Shipments.Select(s => s.PK)));

				var countryCodes = new HashSet<ZString>();

				if (LoadPort != null)
				{
					countryCodes.Add(LoadPort.RL_RN_NKCountryCode);
				}

				if (DischargePort != null)
				{
					countryCodes.Add(DischargePort.RL_RN_NKCountryCode);
				}

				foreach (Transport transport in Transports)
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

				if (FirstForeignPort != null)
				{
					countryCodes.Add(FirstForeignPort.RL_RN_NKCountryCode);
				}

				if (LastForeignPort != null)
				{
					countryCodes.Add(LastForeignPort.RL_RN_NKCountryCode);
				}

				if (PortOfFirstArrival != null)
				{
					countryCodes.Add(PortOfFirstArrival.RL_RN_NKCountryCode);
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

				ObjectFactory.Get<IComplianceRiskStatusSupporter>().PreFetchHintsCommodityDetails(Shipments.OfType<IComplianceCommodityRiskStatusProvider>().ToArray(), Factory);
			}
		}

		ZBool IComplianceCommodityRiskStatusProvider.IsEditingCommoditySupported => ZBool.False;

		ZDateTime IComplianceCommodityRiskStatusProvider.EffectiveDate
		{
			get
			{
				var result = JK_DepartureForFirstExportTransport;

				if (!result.IsValid)
				{
					result = JK_SystemCreateTimeUtc.ToLocalBranchTime();
				}

				if (!result.IsValid)
				{
					result = ZDateTime.Now;
				}

				return result;
			}
		}

		ComplianceAssessmentPointPairInfo IComplianceCommodityRiskStatusProvider.AssessmentPointPairInfo => new ComplianceAssessmentPointPairInfo();

		CommodityRiskCalculateFactor IComplianceCommodityRiskStatusProvider.RiskCalculateFactor => CommodityRiskCalculateFactor.All;

		IEnumerable<IComplianceItemRiskStatusProvider> IComplianceItemRiskStatusProvider.SubComplianceRiskStatusProviders => GridShipments.OfType<IComplianceItemRiskStatusProvider>();

		IEnumerable<IComplianceItemRiskStatusProvider> IComplianceItemRiskStatusProvider.ParentComplianceRiskStatusProviders => Enumerable.Empty<IComplianceItemRiskStatusProvider>();

		ZGuid IComplianceItemRiskStatusProvider.ParentID => PK;

		ZString IComplianceItemRiskStatusProvider.ParentTableCode => TablePrefix;

		ComplianceRiskSupport IComplianceItemRiskStatusProvider.ComplianceRiskSupport { get; } = ComplianceRiskSupport.SupportSubCompliances;

		Func<DocumentDeliveryResultForComplianceWorkflow> IComplianceItemRiskStatusProvider.InitializeComplianceWorkflowPopupIfNeeded { get; set; }

		(ZBool IsCurrent, ZDateTime JobEndDate) IComplianceItemRiskStatusProvider.JobTime => ComplianceRiskHelper.GetJobEndDateAndIsCurrent(Job?.JH_Status, ZDateTime.Empty,
			ZDateTime.Empty, this, Transports.Select(u => new ComplianceRouting { ETD = u.JW_ETD, ETA = u.JW_ETA, ATD = u.JW_ATD, ATA = u.JW_ATA }));

		ZBool IComplianceItemRiskStatusProvider.IsEnabledComplianceWise => ComplianceRiskHelper.IsFreightEnabledComplianceWise;

		SecurityCheckpoint IComplianceItemRiskStatusProvider.AllowOverrideOverallRiskStatusSecurity => Env.Security.ConsolidationsComplianceAllowOverrideOverallRiskStatus;

		SecurityCheckpoint IComplianceItemRiskStatusProvider.AllowResynchronizeRiskStatusSecurity => Env.Security.ConsolidationsComplianceAllowResynchronizeRiskStatus;

		SecurityCheckpoint IComplianceItemRiskStatusProvider.AllowOverrideFreightMovementRestrictionsSecurity => Env.Security.ConsolidationsComplianceAllowOverrideFreightMovementRestrictions;

		SecurityCheckpoint deniedCheckpoint;

		SecurityCheckpoint operationDeniedCheckpoint => deniedCheckpoint ??= new DeniedSecurityCheckpoint();

		SecurityCheckpoint IComplianceCommodityRiskStatusProvider.EditHarmonizedCodeSecurity => operationDeniedCheckpoint;

		SecurityCheckpoint IComplianceCommodityRiskStatusProvider.EditComplianceAssessmentSecurity => operationDeniedCheckpoint;

		SecurityCheckpoint IComplianceCommodityRiskStatusProvider.AllowComplianceAssessmentSecurity => operationDeniedCheckpoint;

		SecurityCheckpoint IComplianceCommodityRiskStatusProvider.DeclineComplianceAssessmentSecurity => operationDeniedCheckpoint;

		#endregion

		#region ISupportJobDirection

		IForwardingContainerCollection IForwardingConsol.Containers => Containers;

		#endregion

		#region Unique Consol

		IForwardingConsol IUniqueConsolProvider.UniqueConsol => this;

		#endregion

		#region JobRatingPreference

		public JobRatingPreference AutoratingPreferences => autoratingPreferences ?? LoadOrCreateJobRatingPreference();
		JobRatingPreference autoratingPreferences;

		/// <summary>
		///	Depending on the value of registry `AllowConsolAutoratingDateSynchronizedAcrossCompanies`:
		///	1. When the preference should be the same across companies - registry is ON, load the one with null companyPk.
		///	 Fallback to loading the preference specific to current login company.<br/>
		///	2. On the other hand, if the registry is OFF (by default), load the preference with current company.
		///	 Fallback to loading the one (with null companyPk) for any company.
		///	 Because of no synchronization, the company should have its own entry. Create a new entry for the company from the fallback.<br/>
		///	3. If the fallbacks are not found, create a new one.
		/// </summary>
		JobRatingPreference LoadOrCreateJobRatingPreference()
		{
			var isSynchronized = RatingDataRegistry.Instance.AllowConsolAutoratingDateSynchronizedAcrossCompanies.Value;
			Guid? defaultCompanyPk = isSynchronized ? null : Env.CurrentCompanyPK;
			var preference = JobRatingPreference.Load(this, defaultCompanyPk);

			// Fallback using reversed companyPk values. Ie, Env.CurrentCompanyPK vs null.
			if (preference == null)
			{
				Guid? fallbackCompanyPk = isSynchronized ? Env.CurrentCompanyPK : null;
				var fallbackPreference = JobRatingPreference.Load(this, fallbackCompanyPk);

				// Found a value for all companies. Duplicate it for the login company.
				if (fallbackPreference != null && fallbackCompanyPk == null)
				{
					preference = JobRatingPreference.Create(this, Env.CurrentCompanyPK, fallbackPreference.JRP_AutoratingDate, fallbackPreference.JRP_IsDateOverridden);
				}
				else
				{
					preference = fallbackPreference;
				}
			}

			// There is no match from the DB. Create a new one.
			if (preference == null)
			{
				preference = JobRatingPreference.Create(this, defaultCompanyPk, null, null);
			}

			autoratingPreferences = preference;
			RegisterEditableChildObject(autoratingPreferences);
			return autoratingPreferences;
		}

		/// <summary>
		/// Get security settings for editing `Rates > Overriding values`.
		/// Note: being protected for accessing from derived classes.
		/// </summary>
		protected bool AutoratingDateOverriddenSecurityIsAllowed =>
			Env.Security.MaintainConsolTariffsAndRatesOverrideRateDate.IsAllowed;

		// Note: being protected for accessing from derived classes.
		protected bool IsAutoratingDateReadOnly => ReadOnly || !AutoratingDateOverriddenSecurityIsAllowed;

		[ReadOnlyMember(nameof(IsAutoratingDateReadOnly))]
		public override ZDate AutoratingDate
		{
			get => AutoratingPreferences.JRP_AutoratingDate.Date;
			set
			{
				if (AutoratingPreferences.JRP_AutoratingDate == value)
				{
					return;
				}

				var autoratingPreferences = AutoratingPreferences;
				autoratingPreferences.JRP_AutoratingDate = value;
				var isSynchronized = RatingDataRegistry.Instance.AllowConsolAutoratingDateSynchronizedAcrossCompanies.Value;

				// "promote" current autoratingPreferences to synced
				if (isSynchronized && autoratingPreferences.Company != null)
				{
					autoratingPreferences.JRP_GC_Company = ZGuid.Empty;
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateAutoratingDate();
				}

				AutoratingDateInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo AutoratingDateInfo => GetZPropertyInfo(nameof(AutoratingDate));

		bool IContainerTrackingProvider.RoutingLegsHaveChanges
		{
			get
			{
				return isLegRemoved || Transports.Cast<Transport>().Any(leg => !leg.IsInDatabase || GetTrackingTransportPropertyInfos(leg).Any(c => c.HasChanges) || leg.JW_VoyageFlightHasChanges());
			}
		}

		IEnumerable<ZPropertyInfo> GetTrackingTransportPropertyInfos(Transport transport)
		{
			yield return transport.JW_TransportModeInfo;
			yield return transport.JW_VesselInfo;
			yield return transport.JW_RL_NKLoadPortInfo;
			yield return transport.JW_RL_NKDiscPortInfo;
		}

		#endregion

		#region IAirlineTrackingEventProvider

		IEnumerable<BookingConfirmation> IAirlineTrackingEventProvider.LoadBookingConfirmations()
		{
			return this.LoadBookingConfirmations();
		}

		IEnumerable<ActualEvent> IAirlineTrackingEventProvider.LoadActualEvents()
		{
			return this.LoadActualEvents();
		}

		void IAirlineTrackingEventProvider.UpdateShipmentDeliveredTime()
		{
			using (SuspendSettingHasChanges())
			{
				this.UpdateShipmentDeliveredTime();
			}
		}

		bool IAirlineTrackingEventProvider.MatchAndUpdateRoutingLeg(BookingConfirmation bookingConfirmation, Func<bool> needUpdate)
		{
			return this.MatchAndUpdateRoutingLeg(bookingConfirmation, needUpdate);
		}

		bool IAirlineTrackingEventProvider.MatchAndUpdateRoutingLeg(ActualEvent actualEvent, Func<bool> needUpdate)
		{
			return this.MatchAndUpdateRoutingLeg(actualEvent, needUpdate);
		}

		void IAirlineTrackingEventProvider.PropagateTransportEventToConsol(IStmALog log)
		{
			if (this.IsAirTrackingEvent(log))
			{
				this.PropagateTransportEventToConsol(log);
			}
		}

		#endregion

		#region Greenhouse Gas Emissions

		void UpdateContainerCO2eStatusWhenJK_OA_PackDepotAddressChanged()
		{
			if (JK_AgentType != Constants.AgentType.Direct)
			{
				Containers.ForEach(c => (c as ForwardingContainer)?.UpdateCO2eStatusToNotCurrent(new CO2eStatusChangedReason(JK_OA_PackDepotAddressInfo), IsCopying, CO2eTypes.EmptyPickup));
			}
			else
			{
				var shipment = Shipments.FirstOrDefault() as ForwardingShipment;

				if (shipment != null)
				{
					var isPickUpCFSAddressEmpty = shipment.ExportReceivingDepot == null;

					var con1 = shipment.JS_PackingMode == Constants.ContainerModes.FCL && !isPickUpCFSAddressEmpty;
					var con2 = shipment.JS_PackingMode == Constants.ContainerModes.FCL && isPickUpCFSAddressEmpty && !shipment.ConsignorPickupAddress.IsEmpty;

					if (!con1 && !con2)
					{
						Containers.ForEach(c => (c as ForwardingContainer)?.UpdateCO2eStatusToNotCurrent(new CO2eStatusChangedReason(JK_OA_PackDepotAddressInfo), IsCopying, CO2eTypes.EmptyPickup));
					}
				}
			}
		}

		void UpdateContainersCO2eStatusOnJK_OA_UnpackDepotAddressChanged()
		{
			if (JK_AgentType != Constants.AgentType.Direct)
			{
				Containers.ForEach(c => (c as ForwardingContainer).UpdateCO2eStatusToNotCurrent(new CO2eStatusChangedReason(JK_OA_UnpackDepotAddressInfo), IsCopying, CO2eTypes.EmptyReturn));
			}
			else
			{
				var shipment = Shipments.FirstOrDefault() as ForwardingShipment;

				if (shipment != null)
				{
					var isDeliveryCFSAddressEmpty = shipment.JS_OA_ImportReleaseDepot == ZGuid.Empty;

					var con1 = shipment.JS_PackingMode == Constants.ContainerModes.FCL && !isDeliveryCFSAddressEmpty;
					var con2 = shipment.JS_PackingMode == Constants.ContainerModes.FCL && isDeliveryCFSAddressEmpty && !shipment.ConsigneeDeliveryAddress.IsEmpty;

					if (!con1 && !con2)
					{
						Containers.ForEach(c => (c as ForwardingContainer).UpdateCO2eStatusToNotCurrent(new CO2eStatusChangedReason(JK_OA_UnpackDepotAddressInfo), IsCopying, CO2eTypes.EmptyReturn));
					}
				}
			}
		}

		void ContainerCO2eStatusChanged(object sender, JobCO2e jobCo2e)
		{
			if (sender is ForwardingContainer container)
			{
				var updateToNotCurrent = container.GetCO2eStatus(jobCo2e.JCO_Type).ToString() == CO2eStatusList.Codes.NotCurrent;

				if (updateToNotCurrent)
				{
					Shipments.Cast<ICO2eLegBasedSupporter>().ForEach(shipment =>
					{
						if (shipment.EmptyContainers.Any(x => x.Container != null && (x.Container as ForwardingContainer).PK == container.PK))
						{
							shipment.UpdateCO2eStatusToNotCurrent(new CO2eStatusChangedReason((NoResString)"Container"), IsCopying);
						}
					});
				}
			}
		}

		protected override void OnMostInterestingTransportBindingCollectionCreated()
		{
			if (MostInterestingTransportForBinding.Count > 0)
			{
				MostInterestingTransportForBinding[0].CO2eStatusChangedEvent += TransportCO2eStatusChanged;
			}
		}

		protected override void OnTransportsIncludingRelatedCreated()
		{
			foreach (var transport in ((IRoutingSupport)this).TransportsIncludingRelated.Cast<Transport>())
			{
				if (transport.GetCO2eStatus() == CO2eStatusList.Codes.NotCurrent)
				{
					this.UpdateCO2eStatusToNotCurrent(new CO2eStatusChangedReason(freeTextReason: (NoResString)"Transport"), IsCopying);
				}

				transport.CO2eStatusChangedEvent += TransportCO2eStatusChanged;
			}

			((IRoutingSupport)this).TransportsIncludingRelated.CountChanged += (sender, args) =>
			{
				if (!(args.BizObject is Transport transport))
				{
					return;
				}

				if (!IsDeleted && !IsDeleting)
				{
					this.UpdateCO2eStatusToNotCurrent(new CO2eStatusChangedReason(args, (NoResString)"Transport"), IsCopying);
					Shipments.ForEach(shipment => (shipment as ForwardingShipment).UpdateCO2eStatusToNotCurrent());
				}

				if (args.ItemAdded)
				{
					transport.CO2eStatusChangedEvent += TransportCO2eStatusChanged;
				}
				else if (args.ItemRemoved)
				{
					transport.CO2eStatusChangedEvent -= TransportCO2eStatusChanged;
				}
			};
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

		#region TotalCO2e

		public ZDecimal TotalCO2e => this.GetTotalCO2e();

		public ZString TotalCO2eForBinding => this.GetTotalCO2eForBinding();

		public ZPropertyInfo TotalCO2eForBindingInfo => GetZPropertyInfo(nameof(TotalCO2eForBinding));

		[DecimalPlaces(3)]
		public ZDecimal TotalCO2eForSorting => this.GetTotalCO2e();

		public ZPropertyInfo TotalCO2eForSortingInfo => GetZPropertyInfo(nameof(TotalCO2eForSorting));

		public ZString CO2eStatus => this.GetCO2eStatus();

		ZDecimal WeightForCO2e => UsePreAllocationWeight ? JK_TotalShipmentActWeightCheck : JK_TotalShipmentWeight;

		ZString UnitOfWeightForCO2e
		{
			get
			{
				if (UsePreAllocationWeight)
				{
					return WeightVerificationUnit.IsEmpty ? (ZString)Weight.Kilograms : WeightVerificationUnit;
				}

				return JK_TotalShipmentWeightUnit;
			}
		}

		bool UsePreAllocationWeight => Shipments.Count <= 0 || JK_TotalShipmentWeight == 0;

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
				return (this as ICO2eLegBasedSupporter).RequireTEUForTransportMode(JK_TransportMode);
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

		#region ICO2eCalculationSupporter members

		ZString ICO2eLegBasedSupporter.TransportMode => JK_TransportMode;

		ZDecimal ICO2eLegBasedSupporter.Weight => WeightForCO2e;

		ZString ICO2eLegBasedSupporter.UnitOfWeight => UnitOfWeightForCO2e;

		IPrePostCarriageLocation ICO2eLegBasedSupporter.LoadPortForCO2eCalc => new PrePostCarriageLocationWrapper(JK_RL_NKLoadPort);

		IPrePostCarriageLocation ICO2eLegBasedSupporter.DischargePortForCO2eCalc => new PrePostCarriageLocationWrapper(JK_RL_NKDischargePort);

		IPrePostCarriageLocation ICO2eLegBasedSupporter.AdditionalLoadPortForCO2eCalc => new PrePostCarriageLocationWrapper(null);

		IPrePostCarriageLocation ICO2eLegBasedSupporter.AdditionalDischargePortForCO2eCalc => new PrePostCarriageLocationWrapper(null);

		IPrePostCarriageLocation ICO2eLegBasedSupporter.ViaPortForCO2eCalc => new PrePostCarriageLocationWrapper(null);

		ZDateTime ICO2eLegBasedSupporter.ETA => ZDateTime.Empty;

		ZDateTime ICO2eLegBasedSupporter.ETD => ZDateTime.Empty;

		ZString ICO2eLegBasedSupporter.ContainerMode => JK_ConsolMode;

		List<ICO2eLegProvider> ICO2eLegBasedSupporter.Legs => (this as IRoutingSupport).TransportsIncludingRelated.Cast<ICO2eLegProvider>().ToList();

		bool ICO2eLegBasedSupporter.SupportVirtualLegs => true;

		bool ICO2eLegBasedSupporter.ShouldPopulateCO2eForLegs => true;

		ZDecimal ICO2eLegBasedSupporter.GetNumberOfTEUForLeg(ICO2eLegProvider leg) => (this as ICO2eTEUProvider).NumberOfTEU;

		IList<CO2eEmptyContainer> ICO2eLegBasedSupporter.EmptyContainers => Array.Empty<CO2eEmptyContainer>();

		bool ICO2eLegBasedSupporter.RequiresTemperatureControl => JK_RequiresTemperatureControl;

		bool HasContainersForTEU => (JK_ConsolMode == ContainerModes.FCL || JK_ConsolMode == ContainerModes.Groupage || JK_ConsolMode == ContainerModes.BuyersConsol || JK_ConsolMode == ContainerModes.ShippersConsol)
			&& Containers.Cast<ForwardingContainer>().Any(c => c.JC_Calc_TEUCount > 0);

		ZDecimal ICO2eLegBasedSupporter.GetTotalEmptyContainerEmissions() => 0;

		bool ICO2eLegBasedSupporter.RequireTEUForTransportMode(string transportMode)
		{
			return transportMode switch
			{
				TransportModes.Sea when HasContainersForTEU
					=> true,

				TransportModes.Road or TransportModes.Rail when
					JK_ConsolMode == ContainerModes.FCL
					&& Containers.Count != 0
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

			AddReason(string.IsNullOrEmpty(supporter.TransportMode), Res.GetString("65beb06a-daa2-4386-999b-367f5b898bbb", "Consol > Details > Transport Mode"));
			AddReason(supporter.LoadPort is null, Res.GetString("61aa9ff9-9a4b-448d-86dd-3787fcd34bbb", "Consol > Details > 1st Load"));
			AddReason(supporter.DischargePort is null, Res.GetString("30436a27-1e5e-4683-a034-4701089cabbb", "Consol > Details > Last Discharge"));
			AddReason(!supporter.IncludeTEU && supporter.Weight == 0, Res.GetString("cf7e0c4d-1b65-4e0c-8e17-abeaa6f4cfbd", "Consol > Total Shipment Weight"));

			AddReason(this.IsCountryEmptyWithValidCityOrPostcode(PackDepotAddress), Res.GetString("8fb584c9-9fe3-4ac4-96eb-034d13a3c9ee", "Consol > Departure > CFS (Ctry/Rgn. field is required with City field)"));
			AddReason(this.IsCountryEmptyWithValidCityOrPostcode(UnpackDepotAddress), Res.GetString("4ec0de15-75bf-4eba-9b48-fc63d667bb4e", "Consol > Arrival > CFS (Ctry/Rgn. field is required with City field)"));

			var hasDuplicates = supporter.Legs.Cast<Transport>()
				.Where(x => !x.JW_RL_NKLoadPort.IsEmpty && !x.JW_RL_NKDiscPort.IsEmpty)
				.GroupBy(x => new { x.LoadPort, x.DiscPort })
				.Any(g => g.Count() > 1);

			AddReason(hasDuplicates, Res.GetString("cf7e0c4a-1b64-4e0b-8e16-aae9a6f4cfbb", "Consol > Routing > Duplicate Load Ports and Discharge"));

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

		ZDecimal ICO2eTEUProvider.NumberOfTEU => Containers.TEUCount;

		ZDecimal ICO2eTEUProvider.TonnesPerTEU
		{
			get
			{
				var teuCount = (this as ICO2eTEUProvider).NumberOfTEU;
				if (teuCount == 0)
				{
					return 0;
				}
				return Weight.ConvertSafe(WeightForCO2e, UnitOfWeightForCO2e, Weight.Tonnes) / teuCount;
			}
		}

		ZDecimal ICO2eTEUProvider.ContainerEmptyWeightPerTEU
		{
			get
			{
				var teuCount = (this as ICO2eTEUProvider).NumberOfTEU;
				if (teuCount == 0)
				{
					return 0;
				}
				return Containers.Cast<CommonContainer>().Sum(c => Weight.Convert(c.JC_TareWeight + c.JC_DunnageWeight, c.ContainerWeightUnit, Weight.Kilograms) / teuCount);
			}
		}

		ZString ICO2eTEUProvider.ContainerEmptyWeightPerTEUUnit => Weight.Kilograms;

		#region Pre/Post Carriage

		bool ICO2ePrePostCarriage.RequiresPrePostCarriageLegs => false;

		IPrePostCarriageLocation[] ICO2ePrePostCarriage.GetPreCarriageLocations(ZString hblDeliveryMode)
		{
			if (hblDeliveryMode.IsEmpty
				|| CO2eHelper.IsDoorPickup(hblDeliveryMode)
				|| CO2eHelper.IsCFSPickup(hblDeliveryMode))
			{
				return new[]
				{
					new PrePostCarriageLocationWrapper(PackDepotAddress, CFSDepartureByTransportMode),
					new PrePostCarriageLocationWrapper(JK_RL_NKLoadPort)
				};
			}

			return new[]
			{
				new PrePostCarriageLocationWrapper(JK_RL_NKLoadPort)
			};
		}

		IPrePostCarriageLocation[] ICO2ePrePostCarriage.GetPostCarriageLocations(ZString hblDeliveryMode)
		{
			if (hblDeliveryMode.IsEmpty
				|| CO2eHelper.IsDoorDelivery(hblDeliveryMode)
				|| CO2eHelper.IsCFSDelivery(hblDeliveryMode))
			{
				return new[]
				{
					new PrePostCarriageLocationWrapper(JK_RL_NKDischargePort),
					new PrePostCarriageLocationWrapper(UnpackDepotAddress, CFSArrivalByTransportMode)
				};
			}

			return new[]
			{
				new PrePostCarriageLocationWrapper(JK_RL_NKDischargePort)
			};
		}

		void ICO2ePrePostCarriage.OnTransportBookingCalculated(IDtbBooking dtbBooking)
		{
			Shipments.Cast<ICO2eCalculationSupporter>()
				.ForEach(shipment => shipment.OnAdditionalSupporterCalculated((ICO2eCalculationSupporter)dtbBooking));
		}

		void ICO2ePrePostCarriage.OnTransportBookingCO2eStatusChanged(IDtbBooking dtbBooking)
		{
			if (((ICO2eProvider)dtbBooking).GetCO2eStatus() == CO2eStatusList.Codes.NotCurrent)
			{
				ForEachShipmentWithAdditionalSupporter((BusinessObject)dtbBooking,
														shipment => shipment.UpdateCO2eStatusToNotCurrent(),
														includeInactiveTBs: false);
			}
		}

		void ICO2ePrePostCarriage.OnTransportBookingActiveStatusChanged(IDtbBooking dtbBooking)
		{
			ForEachShipmentWithAdditionalSupporter((BusinessObject)dtbBooking,
													shipment =>
													{
														shipment.UpdateCO2eStatusToNotCurrent();
														shipment.RefreshCO2e();
													},
													includeInactiveTBs: true);
		}

		void ForEachShipmentWithAdditionalSupporter(BusinessObject supporter, Action<ICO2eCalculationSupporter> action, bool includeInactiveTBs)
		{
			foreach (var shipment in Shipments)
			{
				if ((shipment as ForwardingShipment).GetAdditionalCalculationSupporters(includeInactiveTBs: includeInactiveTBs).Any(x => ((BusinessObject)x.Supporter).PK == supporter.PK))
				{
					action(shipment as ICO2eCalculationSupporter);
				}
			}
		}

		ISupportWebAddressValidation[] IAddressesValidation.AddressesToValidate
		{
			get
			{
				return new ISupportWebAddressValidation[]
				{
					PackDepotAddress,
					UnpackDepotAddress
				};
			}
		}

		#endregion

		#endregion

		#region Delivery Due Date

		void DeliveryDueDateFactorHasChanged(ForwardingShipment shipment, DeliveryDueDateChangedFactor factor = DeliveryDueDateChangedFactor.ConsolAttached)
		{
			if (!shipment.IsDeleted && !shipment.IsDeleting)
			{
				shipment.DeliveryDueDateFactorHasChanged(factor);
			}
		}

		#endregion

		#endregion

		#region ForwardingConsolShareProperty

		public bool EnableBoleroEBLIntegration
		{
			get => FreightDataRegistry.Instance.EnableBoleroEBLIntegration.Value.EnableEBLIntegration;
		}

		public bool EnablePackageGrouping
		{
			get => FreightDataRegistry.Instance.EnablePackageGrouping.Value;
		}

		#endregion

		#region IAllocationRouteAssignable

		void IAllocationRouteAssignable.UpdateCarrierContractAndAllocationDetails(IRatingContractAllocationLine route)
		{
			JK_OA_ShippingLineAddress = route.Contract?.ServiceProvider?.MainAddress?.PK ?? ZGuid.Empty;
			JK_CarrierContractNumber = route.Contract?.RCT_ContractNumber ?? ZString.Empty;
			JK_RCA_AllocationLine = route.PK;
		}

		bool IAllocationRouteAssignable.HasCarrierOrRouteDifferentToOverride(IRatingContractAllocationLine route)
		{
			return (!JK_CarrierContractNumber.IsEmpty && !string.Equals(JK_CarrierContractNumber, route.Contract?.RCT_ContractNumber, StringComparison.OrdinalIgnoreCase))
				|| (ShippingLine != null && ShippingLine.PK != route.Contract?.RCT_OH)
				|| (!JK_RCA_AllocationLine.IsEmpty && JK_RCA_AllocationLine != route.PK);
		}

		bool IAllocationRouteAssignable.IsAssignedAllocationRoute(IRatingContractAllocationLine route)
		{
			return string.Equals(JK_CarrierContractNumber, route.Contract?.RCT_ContractNumber, StringComparison.OrdinalIgnoreCase)
				&& (ShippingLine?.PK == route.Contract?.RCT_OH)
				&& (JK_RCA_AllocationLine == route.PK);
		}

		ZString IAllocationRouteAssignable.HumanReadableName => HumanReadableName;
		ZString IAllocationRouteAssignable.CarrierContractNumber => JK_CarrierContractNumber;
		ZString IAllocationRouteAssignable.AllocationRouteID => AllocationLine?.RCA_AllocationLineID ?? string.Empty;
		ZString IAllocationRouteAssignable.ServiceProvider => ShippingLine?.OH_Code ?? string.Empty;

		#endregion

		#region TranportModeBindings

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
					Shipments.Cast<ForwardingShipment>().ForEach(shipment => shipment.UpdateCO2eStatusToNotCurrent(new CO2eStatusChangedReason(CFSDepartureByTransportModeInfo, previousValue), IsCopying));
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
					Shipments.Cast<ForwardingShipment>().ForEach(shipment => shipment.UpdateCO2eStatusToNotCurrent(new CO2eStatusChangedReason(CFSArrivalByTransportModeInfo, previousValue), IsCopying));
				}
			}
		}

		#endregion

		#region IL Customs

		#region JK_GMN

		[MaxLength(CusEntryNumber.Schema.CE_EntryNumMaxLength)]
		public ZString JK_GMN
		{
			get => GMNEntryNumber?.CE_EntryNum ?? ZString.Empty;
			set
			{
				if (JK_GMN != value)
				{
					if (GMNEntryNumber == null)
					{
						gmnEntryNumber = CusEntryNumber.New<CusEntryNumber>(this, CusEntryNumberTypes.Israel.GatepassMovementNumber, CountryCodes.Israel);
					}
					GMNEntryNumber.CE_EntryNum = value;
					RegisterEditableChildObject(gmnEntryNumber);
					JK_GMNInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo JK_GMNInfo => GetZPropertyInfo(nameof(JK_GMN));

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
					var objectHandle = (ObjectHandle)providers[nameof(ForwardingConsol)];
					gatePassMovementProvider = new Lazy<IGatePassMovementProvider>(() => (IGatePassMovementProvider)objectHandle.GetObject(this));
				}

				return gatePassMovementProvider.Value;
			}
		}

		Lazy<IGatePassMovementProvider> gatePassMovementProvider;

		#endregion

		#region AllocationConsumptionLogging

		public AllocationConsumptionLogger AllocationConsumptionLogger => allocationConsumptionLogger ??= new AllocationConsumptionLogger(this);

		AllocationConsumptionLogger allocationConsumptionLogger;

		#endregion

		#region ContainerWeightLimitLogging

		public AllocationRouteContainerWeightLimitHelper AllocationRouteContainerWeightLimitHelper => allocationRouteContainerWeightLimitHelper ??= new AllocationRouteContainerWeightLimitHelper(this);
		AllocationRouteContainerWeightLimitHelper allocationRouteContainerWeightLimitHelper;

		#endregion

		#region IExternalRequestGenerationProvider

		public ZString GetRequestJobID() => JK_UniqueConsignRef;

		public ZString GetRequestTypeCode() => ExternalRequestTypes.Codes.Consol;

		public (ZGuid OrginzationPK, ZGuid ContactPK) GetRequestSupportedAddressInfo(ZString addressType) => (ZGuid.Empty, ZGuid.Empty);

		#endregion
	}
}

#region Test
#if DEBUG

#region Test Methods

namespace Enterprise.Freight.Forwarding.Business
{
	partial class ForwardingConsol
	{
		internal void ResetSupplyChainSecurityConfigurationForTesting()
		{
			supplyChainSecurityConfiguration = null;
		}
	}
}

#endregion

#endif
#endregion
