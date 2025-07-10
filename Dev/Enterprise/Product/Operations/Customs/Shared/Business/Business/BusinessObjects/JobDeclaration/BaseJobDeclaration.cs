using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Integration;
using Enterprise.BatchProcessor;
using Enterprise.BufferManagement.Integration;
using Enterprise.Customs.Business.CustomsLists;
using Enterprise.Customs.Business.Extensions;
using Enterprise.Customs.Business.WarehouseExtensions;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.AU;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.Universal;
using Enterprise.DeniedPartyScreening.Integration;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineIntegration;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Freight.LocalCartage.Integration;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.Freight;
using Enterprise.Integration.TransportBooking;
using Enterprise.MasterData.Business;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.CreditControl.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.Warehouse.Integration.BondedWarehouse;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Business;
using Enterprise.ZArchitecture.Business.ClusterKey;
using Enterprise.ZArchitecture.Business.EventManagement;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs.EUExitControl;
using BusinessContext = Enterprise.Integration.Accounting.BusinessContext;
using Constants = Enterprise.Core.Constants;
using DtbBookingDirection = Enterprise.TransportCommon.Shared.DtbBookingDirection;
using EventReferenceParameters = CargoWise.EventReference.Constants.EventReferenceParameters;
using ICommonCartage = Enterprise.Freight.LocalCartage.Integration.ICommonCartage;
using IConsignmentJobHelper = Enterprise.TransportCommon.Shared.IConsignmentJobHelper;
using OrgCodes = Enterprise.MasterFiles.Business.AccountingMasterFilesConstants.OrganisationTypeCodes;
using TransportBookingLoader = Enterprise.TransportBookings.Shared.TransportBookingLoader;

namespace Enterprise.Customs.Business
{
	public interface IExternalFactoryRefreshable
	{
		bool ExternalFactoryRefreshEnabled
		{
			get;
			set;
		}
	}

	[CodeProperty(BaseJobDeclaration.Schema.JE_DeclarationReference), DescriptionProperty(BaseJobDeclaration.Schema.Description)]
	[SingleObjectAroundARow]
	[UserDefinedValues]
	[UniversalDataContext(DataContextType.CustomsDeclaration)]
	[Metadata.Integration.MetadataContext(Enterprise.Metadata.Integration.MetadataContext.BaseJobDeclaration)]
	[ProvideMetaDataProperty("ShouldPropertiesBeReadOnly", MetaDataTypes.ReadOnly)]
	[UniversalCopyIgnoreElement("CusEntryHeaders", "ReferenceNumbers", "DocsAndCartageDetails", JobDeclarationSchema.Constants.JE_ConsolidatedCargoStatus, JobDeclarationSchema.Constants.JE_MessageStatus,
		JobDeclarationSchema.Constants.JE_EntryStatus, JobDeclarationSchema.Constants.JE_EntryAuthorisationDate, JobDeclarationSchema.Constants.JE_EntrySubmittedDate, JobDeclarationSchema.Constants.JE_AddInfo, JobDeclarationSchema.Constants.JE_WarehouseTransactionStatus)]
	[UniversalCopyAssociateElement("Addresses", "DocAddresses")]
	[UniversalCopyAssociateElement("JobComInvoiceHeaders", "Invoices")]
	[UniversalCopyAssociateElement("CusDecHouseBills", "Bills")]
	[UniversalCopyAssociateElement("CusEntryInstructions", "CustomsEntryInstructions")]
	[UniversalCopyExtraCollection("CustomFields", "IAddOnValue", GenCustomAddOnValueSchema.Constants.TableName, GenCustomAddOnValueSchema.Constants.XV_ParentID, GenCustomAddOnValueSchema.Constants.XV_ParentTableCode)]
	[UniversalCopyAddInfo]
	[UniversalCopyWithExtendedEntities(FinishCopyMethod = "AfterUniversalCopy")]
	[AdditionalRootType(BaseJobDeclaration.Schema.TableName)]
	public partial class BaseJobDeclaration :
		AutoJobDeclaration,
		Integration.Customs.IJobDeclarationWithShipmentSynchonisation,
		IServiceLocator,
		ICustomsJobInfo,
		ITemplateCopyable,
		ICustomsCustomLabelsConfigOrgProvider,
		IExternalFactoryRefreshable,
		ISupportDataImporting,
		ISupportUXMLDataImporting,
		IAttachOrders,
		IInvoiceParent,
		ICDArchive,
		ILandedCostHeader,
		ILandedCostHeaderProvider,
		IInvoiceLinkProvider,
		ICurrencyConverterProvider,
		IDeclarationAsCartageParent,
		ICurrencyConverterDataProvider,
		IApportionInvoiceHolder,
		Freight.Integration.Forwarding.ICommercialInvoice,
		ICusEntryNumFilterProvider,
		ILocalShippingLineProvider,
		ISometimesWorkflowProvider,
		IExportStatement,
		IEDocsProvider,
		IRelatedJobNumber,
		Freight.Integration.Forwarding.ICommercialInvoiceProvider,
		ICartageParent,
		ICartageParentExtra,
		ICartageLooseCargo,
		IWeightHolder,
		IBillGenerationSupport,
		IInvoicesProvider,
		ICommonInvoiceDataProvider,
		ICreditControlledDocumentDelivery,
		IRegistryAccessingSupporter,
		ISequenceNumberHeader,
		ICustomsJobInfoProvider,
		IFlightDetailsSuppression,
		IScreeningPartyProvider,
		IShouldUpdateScreeningStatus,
		Freight.Integration.Forwarding.IJobDeclarationSailingDateUpdater,
		IJobInvoicingPlugIn,
		IRatingSupporterWithAdapter,
		IFountainResolver,
		ICustomFieldProvider,
		IBuyerSupplierRelationshipConsumer,
		IWorkflowTriggerEventSource,
		IWorkflowTriggerFieldChangeSource,
		IMessageSenderSupporter,
		IStmALogParentProvider,
		IAdditionalReferenceNumberTypeProvider,
		IAdditionalReferenceNumberValidationProvider,
		IControllerIDProvider,
		IJobHeaderParentProvider,
		IProcessHandlingInfoProvider,
		IRelatableActivity,
		ISupportsPostingOverseasAgentCharge,
		IWarehouseIntegrationSupporter,
		IUniversalXMLNoteParent,
		ISynchroniserReadOnlyMembersProvider,
		IContainerParent,
		IBranchProvider,
		ISendEmailSource,
		IContainerTrackingProvider,
		IHandleEventsForOtherObjects,
		IParentToJobDocsAndCartage,
		ICustomsFileParent,
		IValidateForCustomsMessagingSupporter,
		Integration.Customs.IJobDeclarationAutoSendingMessageSupporter,
		IRelatedOrgDeniedPartyScreenable,
		IHaveJobHeader,
		ISupportingDocObject,
		IClusterKeyMaster,
		IAddInfoChildSupporter,
		IServicesParent,
		IAuditColumnProviderForBOLogger,
		IDeniedPartyProvider,
		IComplianceJobDirectionProvider,
		ICusGoodsLocationTypeSupporter,
		IAdditionalDebuggingDetails,
		ITypeDeciderContext,
		IDocAddresses,
		ISupportMultipleResourceStringData,
		ISupportOverrideOperationalActionRecipientEmail,
		IPropertyChecker,
		ISourceIdentifierProvider,
		IDataModelSupporter,
		IAuditParent,
		IAuthorityToActDeclarationProvider
	{
		#region Schema

		public new class Schema : AutoJobDeclaration.Schema
		{
			public const string JE_EntryStatusDescription = "JE_EntryStatusDescription";
			public const string JE_MessageStatusDescription = nameof(BaseJobDeclaration.JE_MessageStatusDescription);
			public const string DeclarationNumber = "DeclarationNumber";
			public const string Description = "Description";
			public const string MergeBy = "MergeBy";
			public const string JE_MarksAndNumbers = "JE_MarksAndNumbers";
			public const string JE_MarksAndNumbersShort = "JE_MarksAndNumbersShort";
			public const string PackagesRequiredPackageCount = "PackagesRequiredPackageCount";
			public const string PackagesActualPackageCount = "PackagesActualPackageCount";
			public const string OrderNumbers = "OrderNumbers";
			public const string HouseBillsCommaSeparated = "HouseBillsCommaSeparated";
			public const string MasterBillsCommaSeparated = "MasterBillsCommaSeparated";
			public const string HouseBillIssuedDate = "HouseBillIssuedDate";
			public const string JP_Calc_CartageAdvised = "JP_Calc_CartageAdvised";
			public const string JE_CartageCompleted = "JE_CartageCompleted";
			public const string JE_EstimatedDeliveryOrPickup = "JE_EstimatedDeliveryOrPickup";
			public const string JE_FCLDeliveryOrPickupEquipmentNeeded = nameof(BaseJobDeclaration.JE_FCLDeliveryOrPickupEquipmentNeeded);
			public const string JE_DeliveryOrPickupLabourTime = "JE_DeliveryOrPickupLabourTime";
			public const string JE_DeliveryOrPickupLabourCharge = "JE_DeliveryOrPickupLabourCharge";
			public const string JE_PickupOrDeliveryTruckWaitTime = "JE_PickupOrDeliveryTruckWaitTime";
			public const string JE_PickupOrDeliveryTruckWaitCharge = "JE_PickupOrDeliveryTruckWaitCharge";
			public const string JE_OA_DeliveryOrPickupCartageCoAddr = "JE_OA_DeliveryOrPickupCartageCoAddr";
			public const string DeliveryOrPickupCartageCoPK = "DeliveryOrPickupCartageCoPK";
			public const string JE_DeliveryOrPickupRequiredBy = "JE_DeliveryOrPickupRequiredBy";
			public const string WarehouseTransactionStatus = "WarehouseTransactionStatus";
			public const string WarehouseTransactionStatusDescription = "WarehouseTransactionStatusDescription";
			public const string HasManualWarehouseUpdate = "HasManualWarehouseUpdate";

			public const string HouseBillLabel = "HouseBillLabel";

			public const string JE_QuarantineMessagingRemarks = "JE_QuarantineMessagingRemarks";
			public const string ConsolidatedCargoStatusDescription = "ConsolidatedCargoStatusDescription";
			public const string EarliestCustomsEntryIssueDate = "EarliestCustomsEntryIssueDate";

			public const string BrokerName = "BrokerName";
			public const string ImporterName = "ImporterName";
			public const string SupplierName = "SupplierName";
			public const string ForwarderName = "ForwarderName";

			public const string AuditDate = "AuditDate";
			public const string AuditReference = "AuditReference";
			public const string AuditLogUser = "AuditLogUser";
			public const string AuditLogUserName = "AuditLogUserName";
			public const string FreightContainerMode = "FreightContainerMode";

			public const string TotalOutstandingAmount = "TotalOutstandingAmount";
			public const string TotalInvoicedAmount = "TotalInvoicedAmount";
			public const string TotalBilledAmount = "TotalBilledAmount";

			public const string DeclarationBranch = "DeclarationBranch";
			public const string BillingBranch = "BillingBranch";
			public const string BillingDepartment = "BillingDepartment";
			public const string BillingOperator = "BillingOperator";
			public const string BillingTaxBranch = "BillingTaxBranch";

			public const string ShipToPartyOrgPK = "ShipToPartyOrgPK";
			public const string SoldToPartyOrgPK = "SoldToPartyOrgPK";
			public const string SellerOrgPK = "SellerOrgPK";
			public const string ManufacturerOrgPK = "ManufacturerOrgPK";
			public const string DistributorOrgPK = "DistributorOrgPK";
			public const string PackagerOrgPK = "PackagerOrgPK";
			public const string ShipperOrgPK = "ShipperOrgPK";

			public const string JE_ETAOfDischarge = "JE_ETAOfDischarge";
			public const string JE_ETDOfLoading = "JE_ETDOfLoading";
			public const string ConsigneeAddressOrgPK = "ConsigneeAddressOrgPK";
			public const string JE_BillsFilterBy = "JE_BillsFilterBy";

			public const string LocalClientCode = "LocalClientCode";
			public const string LocalClientName = "LocalClientName";
			public const string LocalClientAddressAsString = "LocalClientAddressAsString";
			public const string LocalClientAddressShortCode = "LocalClientAddressShortCode";
			public const string LocalClientAddress1 = "LocalClientAddress1";
			public const string LocalClientAddress2 = "LocalClientAddress2";
			public const string LocalClientCity = "LocalClientCity";
			public const string LocalClientState = "LocalClientState";
			public const string LocalClientCountry = "LocalClientCountry";

			public const string EntryReleaseDate = nameof(BaseJobDeclaration.EntryReleaseDate);

			public const string RelatedTransportBookingsJobNumbers = "RelatedTransportBookingsJobNumbers";
			public const string ImporterAddressOrgPK = "ImporterAddressOrgPK";
			public const string SupplierAddressOrgPK = "SupplierAddressOrgPK";
			public const string DocsAndCartage = nameof(BaseJobDeclaration.DocsAndCartage);

			public const string DeclarantCode = nameof(BaseJobDeclaration.DeclarantCode);
			public const string DeclarantName = nameof(BaseJobDeclaration.DeclarantName);

			public const string PhaseStatus = nameof(BaseJobDeclaration.PhaseStatus);
			public const string PhaseStatusDescription = nameof(BaseJobDeclaration.PhaseStatusDescription);
		}
		#endregion

		public static BaseJobDeclaration New(BusinessObjectFactory factory)
		{
			return factory.New<BaseJobDeclaration>();
		}

		public BaseJobDeclaration(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			fAutoCreateChargesBasedOnIncoTerm = true;
			BuyerSupplierLinksHelper = new BuyerSupplierLinksHelper<BaseJobDeclaration>(this);
			BuyerSupplierLinksHelper.Register();
		}

		public MostInterestingLegProvider MostInterestingLegProvider
		{
			get { return MostInterestingLegProviderCore; }
		}

		protected virtual MostInterestingLegProvider MostInterestingLegProviderCore
		{
			get { return new MostInterestingLegProvider(this); }
		}

		public ComInvoiceReconciliator GetNewComInvoiceReconciliator(Order[] selectedOrders, ComInvReconciliationQuantityType quantityType)
		{
			return GetNewComInvoiceReconciliatorCore(selectedOrders, quantityType);
		}

		protected virtual ComInvoiceReconciliator GetNewComInvoiceReconciliatorCore(Order[] selectedOrders, ComInvReconciliationQuantityType quantityType)
		{
			return new ComInvoiceReconciliator(this, selectedOrders, quantityType);
		}

		#region FetchHints

		public void AddJobComInvoiceLineFetchHintsIfNeeded()
		{
			if (!hasJobComInvoiceLineFetchHintBeingAdded)
			{
				hasJobComInvoiceLineFetchHintBeingAdded = true;
				var invoicesQuery = new ZQuery(JobComInvoiceHeaderSchema.JZ_ClusterKey, JE_ClusterKey);
				invoicesQuery.AddToFilter(JobComInvoiceHeaderSchema.JZ_JE, PK);
				invoicesQuery.AddToFilter(JobComInvoiceHeaderSchema.JZ_GroupInvoice, ZBool.False);
				if (!IsInDatabase)
				{
					invoicesQuery.FetchOnlyFromLocalCache = true;
				}
				var invoices = Factory.Load<BaseJobComInvoiceHeader>(invoicesQuery);
				if (invoices.Length > 0)
				{
					foreach (var invoice in invoices.Where(x => x.IsInDatabase))
					{
						Factory.AddFetchHint(JobComInvoiceLineSchema.JI_JZ, invoice.PK);
					}
				}
			}
		}
		bool hasJobComInvoiceLineFetchHintBeingAdded;

		public void AddInvoiceChargesFetchHintsIfNeeded()
		{
			if (!hasInvoiceChargesFetchHintBeingAdded)
			{
				hasInvoiceChargesFetchHintBeingAdded = true;
				foreach (BaseJobComInvoiceGroupHeader groupHeader in AllGroupHeaders.Where(x => x.IsInDatabase))
				{
					Factory.AddFetchHint(JobComInvHeaderChargeSchema.J7_ParentID, groupHeader.PK);
				}
				AddJobComInvoiceLineFetchHintsIfNeeded();
				foreach (BaseJobComInvoiceHeader invoice in Invoices.Where(x => x.IsInDatabase))
				{
					invoice.AddInvoicChargesFetchHintsIfNeeded();
				}
			}
		}
		bool hasInvoiceChargesFetchHintBeingAdded;

		void AddFetchHintsForMarkAsNeedingValidationIfNeeded()
		{
			if (!hasAddFetchHintsForMarkAsNeedingValidation)
			{
				AddFetchHintsForMarkAsNeedingValidationCore();
				hasAddFetchHintsForMarkAsNeedingValidation = true;
			}
		}
		bool hasAddFetchHintsForMarkAsNeedingValidation;

		protected virtual void AddFetchHintsForMarkAsNeedingValidationCore()
		{
			AllGroupHeaders.ForEach(c => c.FetchForLoadChildEditableObjectsIfNeeded());
			Invoices.ForEach(c => c.FetchForLoadChildEditableObjectsIfNeeded());
			InvoiceLines.ForEach(c => c.FetchForLoadChildEditableObjectsIfNeeded());
			CusContainers.ForEach(c => c.FetchForLoadChildEditableObjectsIfNeeded());
			Bills.Cast<Bill>().ForEach(c => c.FetchForLoadChildEditableObjectsIfNeeded());
			CustomsEntryHeaders.ForEach(c => c.FetchForLoadChildEditableObjectsIfNeeded());
			PackingGroups.ForEach(c => c.FetchForLoadChildEditableObjectsIfNeeded());
		}

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new FetchStrategies.BaseJobDeclarationFetchStrategy(this);
		}

		#endregion

		public virtual IEnumerable<string> StmALogProxyFieldsNames
		{
			get
			{
				return Enumerable.Empty<string>();
			}
		}

		void IWarehouseIntegrationSupporter.UpdateHoldData(UniversalDataBuss.DataObjects.Universal.Shipment shipment, RecipientRoleType recipientRoleType)
		{
		}

		public void MarkAsNeedingValidationForMajorDataChange()
		{
			if (!IsMarkingAsNeedingValidationSuspended)
			{
				MarkAsNeedingValidationForMajorDataChangeCore();
			}
		}

		protected virtual void MarkAsNeedingValidationForMajorDataChangeCore()
		{
			MarkAsNeedingValidation();

			AddFetchHintsForMarkAsNeedingValidationIfNeeded();

			AllGroupHeaders.MarkAsNeedingValidationIncludingChildren();
			Invoices.MarkAsNeedingValidationIncludingChildren();
			InvoiceLines.MarkAsNeedingValidation();
			CusContainers.MarkAsNeedingValidationIncludingChildren();
			Bills.MarkAsNeedingValidationIncludingChildren();
			CustomsEntryHeaders.MarkAsNeedingValidationIncludingChildren();
			PackingGroups.MarkAsNeedingValidationIncludingChildren();
		}

		public virtual void DefaultValueForFakeDeclaration()
		{
		}

		public void PreLogAllDocumentsReceivedEvents()
		{
			if (!IsInDatabase && !requiredDocumentsAdded)
			{
				AddRequiredDocuments();
				requiredDocumentsAdded = true;
			}
		}

		bool requiredDocumentsAdded;

		#region Licence

		public event LicenceLoginEventHandler LicenceLogin;

		internal bool CanRaiseLicenceLogin
		{
			get { return CurrentUserHasSecurityAccess && LicenceLogin != null; }
		}

		public void RaiseLicenceLogin(LicenceLoginEventArgs e)
		{
			LicenceLogin?.Invoke(this, e);
		}

		#region Inventory Management Licence and Security

		public event LicenceLoginEventHandler BondedWarehouseLicenceLogin;

		public bool CurrentUserHasBondedWarehouseSecurityAccess
		{
			get
			{
				return Env.Security.ImportEditBondedWarehouse.IsAllowed;
			}
		}

		internal bool CanRaiseBondedWarehouseLicenceLogin
		{
			get { return CurrentUserHasBondedWarehouseSecurityAccess && BondedWarehouseLicenceLogin != null; }
		}

		public void RaiseBondedWarehouseLicenceLogin(LicenceLoginEventArgs e)
		{
			BondedWarehouseLicenceLogin?.Invoke(this, e);
		}

		#endregion

		#endregion

		#region Type Decider

		public static readonly BaseJobDeclarationTypeDecider TypeDecider = new BaseJobDeclarationTypeDecider();

		#endregion

		#region Active Filter

		public static ZQuery ActiveFilter
		{
			get { return new ZQuery(JobDeclarationSchema.JE_IsCancelled, SQLComparisonOperator.Equal, ZBool.False); }
		}

		#endregion

		#region Business Object Overrides

		protected override Logs GetNewLogs()
		{
			var result = base.GetNewLogs();
			result.EventsThatCannotBeAdded.Add(AutoEvents.LockForEdit);
			result.EventsThatCannotBeAdded.Add(AutoEvents.UnlockForEdit);

			return result;
		}

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		public override bool IsSavedByFactory
		{
			get { return isPersistent && base.IsSavedByFactory; }
		}

		public bool IsPersistent
		{
			get { return isPersistent; }
		}

		bool isPersistent = true;

		public virtual void MakeNonPersistent()
		{
			isPersistent = false;
		}

		bool isLegRemoved;

		public void OnTransportRemoved(Transport transport)
		{
			if (transport != null && transport.IsInDatabase)
			{
				isLegRemoved = true;
			}
		}

		protected override ZString HumanReadableNameCore
		{
			get
			{
				var result = HumanReadableNameWithoutID;

				if (!JE_DeclarationReference.IsEmpty)
				{
					result += " " + JE_DeclarationReference;
				}
				else if (!JE_MasterBill.IsEmpty || !HouseBillsCommaSeparated.IsEmpty)
				{
					result += " (";

					if (!JE_MasterBill.IsEmpty)
					{
						result += Res.GetString("044DC0D6-24DD-48f0-B36C-B287DE393B00", "Master Bill='{0}'", JE_MasterBill);

						if (!HouseBillsCommaSeparated.IsEmpty)
						{
							result += " ";
						}
					}

					if (!HouseBillsCommaSeparated.IsEmpty)
					{
						result += Res.GetString("F6D7071F-4DC4-4752-B5DD-2192942C57F7", "House Bill='{0}'", HouseBillsCommaSeparated);
					}

					result += ")";
				}

				return result;
			}
		}

		protected string HumanReadableNameWithoutID
		{
			get
			{
				return Res.GetString("ab218c4d-d398-479b-9e9b-c9bcfe799917", "Declaration");
			}
		}

		protected override JobDeclarationValidation GetNewValidation() => new BaseJobDeclarationValidation(this);

		protected override void RunPreSaveValidationCore()
		{
			if (ApportionmentDirty)
			{
				ResumeApportionment();
			}

			var isExport = IsExport;
			var isImport = IsImport;
			this.DefaultControllingCustomer(isExport, isImport);
			this.DefaultControllingAgent(isExport, isImport);
			RefCountryRulesHelper.AddRulesToNotes(Origin?.Country, FinalDestination?.Country, Notes, TransportMode, IsInDatabase, true);
			base.RunPreSaveValidationCore();
		}

		#region Set Default Values

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			using (SetSettingDefaultValuesInProgress())
			using (SuspendMarkingAsNeedingValidation())
			{
				using (GetValidationSuspender())
				{
					JE_AutoWeightApportion = CustomsDataRegistry.Instance.EnableAutoApportionWeight.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid());
					JobComInvoiceGroupHeaders.IsMarkAsNeedingValidationSuspended = true;

					var topGroupInvoice = JobComInvoiceGroupHeaders.AddNew();
					using (topGroupInvoice.SuspendSettingHasChanges())
					{
						topGroupInvoice.IsDefaultTopGroupHeader = true;
						topGroupInvoice.JZ_InvoiceNumber = BaseJobComInvoiceGroupHeader.AllInvoices;
					}

					JobComInvoiceGroupHeaders.IsMarkAsNeedingValidationSuspended = false;
					var currentBranch = GlbBranch.CurrentBranch;
					JE_GB = currentBranch.PK;
					JE_GC = currentBranch.GB_GC;
					JE_MessageType = GetDefaultMessageType(GlbDepartment.CurrentDepartment.GE_Import);
					JE_TransportMode = DefaultTransportMode;
					JE_MergeBy = DefaultMergeBy;
					JE_TotalNoOfPacksPackType = DefaultTotalNoOfPacksPackType;

					JE_RS_NKServiceLevel = DefaultServiceLevel != null ? DefaultServiceLevel.RS_Code : ZString.Empty;
				}

				JE_LandedCostByWeight = (ZByte)(IsSea ? 0 : 100);
				JE_LandedCostByVolume = (ZByte)(IsSea ? 100 : 0);
				JE_LandedCostByUnits = 0;
				JE_LandedCostByCost = 0;

				DefaultJE_ApplicationCode();
			}
		}

		protected virtual bool IsBuiltinSubmissionType(string type) => type.Equals(SubmissionTypeBuiltinCode, StringComparison.InvariantCultureIgnoreCase);
		protected virtual string SubmissionTypeBuiltinCode => DeclarationApplicationCodeList.Codes.Builtin;
		protected virtual void DefaultJE_ApplicationCode()
		{
			var customsInterface = DataRegistry.Business.CustomsDataRegistry.Instance.LocalCountryCustomsInterface.GetFallBackValueAtAllLevels(RegistryCompanyPK, Guid.Empty, Guid.Empty);
			if (IsLocalCountryCustomsInterfaceEmpty(customsInterface))
			{
				DefaultJE_ApplicationCodeWhenLocalCountryCustomsInterfaceIsEmpty();
			}
			else
			{
				var submissionType = customsInterface.SubmissionType.ToUpperInvariant();
				switch (customsInterface.SubmissionType)
				{
					case DeclarationApplicationCodeListForRegistry.Codes.BothBuiltInDefaulted:
					case DeclarationApplicationCodeList.Codes.Builtin:
						JE_ApplicationCode = SubmissionTypeBuiltinCode;
						break;
					case DeclarationApplicationCodeList.Codes.Interfaced:
					case DeclarationApplicationCodeListForRegistry.Codes.BothInterfaceDefaulted:
						JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
						break;
				}
			}
		}
		protected bool IsLocalCountryCustomsInterfaceEmpty(LocalCountryCustomsInterface customsInterface) => customsInterface == null || (customsInterface.RecipientID.IsEmpty && customsInterface.SubmissionType.IsEmpty);

		protected virtual void DefaultJE_ApplicationCodeWhenLocalCountryCustomsInterfaceIsEmpty()
		{
			if (IsABMInterfaceActivated)
			{
				JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
			}
			else
			{
				JE_ApplicationCode = CountryHasBuiltInDeclaration ? SubmissionTypeBuiltinCode : DeclarationApplicationCodeList.Codes.Interfaced;
			}
		}

		bool CountryHasBuiltInDeclaration
		{
			get
			{
				var country = CountryCode;
				return !country.IsEmpty && IntegratedCountryHelper.CountryHasBuiltInDeclaration(country);
			}
		}

		RefServiceLevel DefaultServiceLevel
		{
			get { return defaultServiceLevel ?? (defaultServiceLevel = Factory.Load<RefServiceLevel>(Env.Registry.ServiceLevel)); }
		}
		RefServiceLevel defaultServiceLevel;

		#region Default Data Grouping

		public ZString GetDefaultDataGroupingCode(DefaultDataGroupingType dataGroupingType = DefaultDataGroupingType.None)
		{
			switch (dataGroupingType)
			{
				case DefaultDataGroupingType.Tariff:
					return DefaultDataGroupingForTariffsCore;
				case DefaultDataGroupingType.DutyRateCodes:
					return DefaultDataGroupingForDutyRateCodesCore;
				case DefaultDataGroupingType.CusProcedure:
					return DefaultDataGroupingForCusProcedureCore;
				case DefaultDataGroupingType.AdditionalDocumentCodes:
					return DefaultDataGroupingForAdditionalDocumentCodesCore;
				default:
					return DefaultDataGroupingCore;
			}
		}

		protected virtual ZString DefaultDataGroupingCore => CountryCode;
		protected virtual ZString DefaultDataGroupingForTariffsCore => DefaultDataGroupingCore;
		protected virtual ZString DefaultDataGroupingForDutyRateCodesCore => DefaultDataGroupingForTariffsCore;
		protected virtual ZString DefaultDataGroupingForCusProcedureCore => DefaultDataGroupingCore;
		protected virtual ZString DefaultDataGroupingForAdditionalDocumentCodesCore => DefaultDataGroupingForTariffsCore;

		#endregion

		protected internal virtual string GetDefaultMessageType(bool import)
		{
			return import ? JobMessageTypeList.Codes.Import : JobMessageTypeList.Codes.Export;
		}

		protected virtual string DefaultTransportMode
		{
			get { return CurrentDepartmentTransportMode; }
		}

		protected virtual string DefaultMergeBy => OrgConstants.MergeInvoiceLines.Tariff;

		protected virtual string CurrentDepartmentTransportMode
		{
			get { return GlbDepartment.CurrentDepartment.TransportMode; }
		}

		protected virtual string DefaultTotalNoOfPacksPackType
		{
			get { return Constants.PkgUnit.Package; }
		}

		#endregion

		public bool IsDeclarationMatchSpecificCountry(ZString countryCode) => IsDeclarationMatchSpecificCountryCore(countryCode);

		protected virtual bool IsDeclarationMatchSpecificCountryCore(ZString countryCode)
		{
			var company = Company;
			return company != null && company.GC_RN_NKCountryCode == countryCode;
		}

		public ZGuid CompanyPK
		{
			get { return Company?.PK ?? ZGuid.Empty; }
		}

		public RefCountry Country
		{
			get { return Company?.Country; }
		}

		public ZString CountryCode
		{
			get
			{
				var country = Country;
				return country != null ? country.Code : ZString.Empty;
			}
		}

		public bool LockDoMergeMutex() => DoMergeMutex.Lock();

		public string GetDoMergeMutexLockInfo() => DoMergeMutex.GetMutexLockByInfo();

		public void UnlockDoMergeMutex()
		{
			if (doMergeMutex != null && doMergeMutex.HasLock)
			{
				doMergeMutex.Unlock();
			}
		}

#if DEBUG
		public
#endif
		ZGlobalMutex DoMergeMutex
		{
			get { return doMergeMutex ?? (doMergeMutex = new ZGlobalMutex(MutexIDs.DataProcessing, "DoMerge" + PK.ToString())); }
		}
		ZGlobalMutex doMergeMutex;

		protected override void OnFactorySaved(bool saveSucceeded)
		{
			try
			{
				base.OnFactorySaved(saveSucceeded);

				if (saveSucceeded)
				{
					fDeletedEntryHeaderPKsInDatabase = null;
					screeningPartySnapshot = currentSnapshot;
					return;
				}

				DeleteAnyNewMessages();
			}
			finally
			{
				UnlockDoMergeMutex();
			}
		}

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();
			var shipment = Shipment;
			if (shipment != null && !shipment.IsDeleted)
			{
				JE_RS_NKServiceLevel = shipment.JS_RS_NKServiceLevel;
			}

			var shouldUpdateScreeningStatus = ((IShouldUpdateScreeningStatus)this).ShouldUpdateScreeningStatus;

			if (shouldUpdateScreeningStatus)
			{
				if (screeningPartySnapshot == null)
				{
					if (IsInDatabase)
					{
						var newFactory = new ReadOnlyBusinessObjectFactory();
						var reloaded = newFactory.Load<BaseJobDeclaration>(PK);
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

			ScreeningStatusUpdater.UpdateJobStatusFromItsScreeningParties(this, this, JE_ScreeningStatusInfo.OriginalValue?.ToString());
		}

		#region Screening Statuses

		List<ScreeningPartiesSnapshot> screeningPartySnapshot;

		List<ScreeningPartiesSnapshot> currentSnapshot;

		List<ScreeningPartiesSnapshot> GetSnapshot(BaseJobDeclaration declaration) => ScreeningStatusUpdater.GetScreeningPartiesSnapshot(((IScreeningPartyProvider)declaration).ScreeningParties, declaration);

		#endregion

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();

			var shipment = Shipment;
			if (IsPersistent && shipment == null)
			{
				DefaultDeliveryOrPickupDatesFromContainers();
			}

			var processTaskLoader = new ProcessTask.Loader(Factory);
			processTaskLoader.CreateTasksAndMilestonesFromTemplateIfRequired(this, TemplateApplicationParameters.ApplyIgnoreHasChanges(!IsInDatabase));

			if ((!IsInDatabase || HasChanges) && shipment != null && !shipment.HasChanges)
			{
				shipment.CreateTasksAndMilestonesFromTemplate(processTaskLoader, TemplateApplicationParameters.ApplyIgnoreHasChanges());
			}

			//When data is imported, no validation runs. It is still important to 'balance' the numbers. Apportioned amounts are never data imported,
			//only the actual costs on either the group, header or the lines are imported.
			if (ApportionmentDirty)
			{
				try
				{
					ResumeApportionment();
				}
				catch (InvalidCastException ex)
				{
					var message = $"Declaration (Type:{GetType().FullName}, Country:{CountryCode}), Login Country ({GlbCompany.CurrentCompany.GC_RN_NKCountryCode}).";
					throw new InvalidCastException(message, ex);
				}
			}
		}

		void DefaultDeliveryOrPickupDatesFromContainers()
		{
			if (CusContainers.Count > 0)
			{
				var propertiesToDefault = new[] { JE_EstimatedDeliveryOrPickupInfo, JP_Calc_CartageAdvisedInfo, JE_CartageCompletedInfo };
				var containerPropertiesToDefaultFrom = IsImport ? new[] { CommonContainer.Schema.JC_ArrivalEstimatedDelivery, CommonContainer.Schema.JC_ArrivalCartageAdvised, CommonContainer.Schema.JC_ArrivalCartageComplete }
														: new[] { CommonContainer.Schema.JC_DepartureEstimatedPickup, CommonContainer.Schema.JC_DepartureCartageAdvised, CommonContainer.Schema.JC_DepartureCartageComplete };

				for (var i = 0; i < propertiesToDefault.Length; i++)
				{
					DefaultDateFromLastDeliveredOrPickedUpContainer(propertiesToDefault[i], containerPropertiesToDefaultFrom[i]);
				}
			}
		}

		void DefaultDateFromLastDeliveredOrPickedUpContainer(IZPropertyInfo declarationDateInfoToUpdate, string containerPropertyToUpdateFrom)
		{
			if (declarationDateInfoToUpdate.Value.IsEmpty
				&& CusContainers.All(cont => !((ZDateTime)cont.JobContainer[containerPropertyToUpdateFrom]).IsEmpty))
			{
				var latestContainerDate = (
					from BaseCusContainer cusContainer in CusContainers
					select (ZDateTime)cusContainer.JobContainer[containerPropertyToUpdateFrom]).Max();

				if (!latestContainerDate.IsEmpty)
				{
					declarationDateInfoToUpdate.Value = latestContainerDate;
				}
			}
		}

		public ZString FreightContainerMode
		{
			get
			{
				ZString result = "";
				if (Shipment != null)
				{
					result = Shipment.JS_PackingMode;
				}

				if (result.IsEmpty)
				{
					bool fCL = false;
					bool lCL = false;
					bool bBK = false;
					bool bLK = false;

					foreach (BaseCusContainer container in CusContainers)
					{
						lCL = lCL || container.CO_FCL_LCL_AIR == Constants.ContainerModes.LCL;
						fCL = fCL || container.CO_FCL_LCL_AIR == Constants.ContainerModes.FCL || container.CO_FCL_LCL_AIR == Constants.ContainerModes.FCLMixedShipper || container.CO_FCL_LCL_AIR == Constants.ContainerModes.Empty;
						bBK = bBK || container.CO_FCL_LCL_AIR == Constants.ContainerModes.BreakBulk;
						bLK = bLK || container.CO_FCL_LCL_AIR == Constants.ContainerModes.Bulk;
					}

					if (bLK)
					{
						result = Constants.ContainerModes.Bulk;
					}
					else if (bBK)
					{
						result = Constants.ContainerModes.BreakBulk;
					}
					else if (fCL)
					{
						result = Constants.ContainerModes.FCL;
					}
					else if (lCL)
					{
						result = Constants.ContainerModes.LCL;
					}
				}

				return result;
			}
		}

		protected void ResetToDefaultIfNotUsed(bool isUsed, ZPropertyInfo info)
		{
			if (!isUsed && !info.Value.IsDefault)
			{
				info.Value = info.Value.Default;
			}
		}

		public override void OnSaving()
		{
			ResetToDefaultIfNotUsed(UseImporterAddress, JE_OA_ImporterAddressInfo);
			ResetToDefaultIfNotUsed(UseSupplierAddress, JE_OA_SupplierAddressInfo);

			SetConcurrencyPolicies();
			RelatedChildActivityPivotCollection.RelinkRelatedSuperAndSubActivities();
			RelatedParentActivityPivotCollection.RelinkRelatedSuperAndSubActivities();
			DeactivateJobHeaderWhenIsCancelled();

			base.OnSaving();
			PopulateDataModelIfNeeded();
			PopulateJE_DeclarationReferenceIfNeeded();
			PopulateJE_OwnerRefIfNeeded();
			LogEventIfJE_EntryStatusChanged();
			ReportEventIfJE_MessageTypeChanged();
			LogEventIfDeclarationHasMessageErrors();
			LogATCEventIfAnyCommercialInvoiceDoNotHaveATCEvent();

			if (!IsWHSUniversalXMLActive && !JE_DeclarationReference.IsEmpty && IsExWarehouse)
			{
				UpdateJobDeclarationReferenceOnWarehouseSide();
			}

			if (NotesAndEventsVisible)
			{
				RefCountryRulesHelper.AddRulesToNotes(Origin?.Country, FinalDestination?.Country, Notes, TransportMode, IsInDatabase, false);
			}

			if (!IsInDatabase)
			{
				ThrowExceptionIfDeclarationAlreadyExistsForShipment();

				if (JE_GS_NKCusAgent.IsEmpty)
				{
					DefaultCusAgent();
				}
			}

			if (!IsPackingInformationRelevant)
			{
				DeleteDuplicatedBills();
			}

			if (this.IsStandAlone)
			{
				new ContainerTrackingSubscriptionRequestedManager(this).UpdateIfNecessary();

				if (!IsInDatabase || JE_MasterBillInfo.HasChanges)
				{
					UpdateTransportFlightSubscriptionEvent();
				}
			}
		}

		protected virtual ZBool ShouldLogEventIfDeclarationHasMessageErrors => CustomsDataRegistry.Instance.RaiseEventDCEWhenSavingErrorMSG.Value;

		void LogEventIfDeclarationHasMessageErrors()
		{
			if (ShouldLogEventIfDeclarationHasMessageErrors && HasMessageErrors)
			{
				Logs.AddNew(Events.DeclarationHasErrors, "DECLARATION SAVED WITH MESSAGE ERRORS");
			}
		}

		void LogATCEventIfAnyCommercialInvoiceDoNotHaveATCEvent()
		{
			foreach (var invoice in Invoices)
			{
				var supplierOrgCode = invoice.Supplier?.OH_Code ?? string.Empty;
				var parameters = new Dictionary<string, string>()
				{
					[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.DeclarationID] = invoice.JZ_InvoiceNumber + ":" + supplierOrgCode,
					[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type] = Constants.EventReferenceParameterTypes.CommercialInvoice
				};

				var eventReference = StmALog.GenerateEventReference(ZString.Empty, parameters);
				if (!Logs.HasLogWith(x => x.SL_Reference == eventReference))
				{
					var eventValue = new EventValue(AutoEvents.Attached, eventTime: ZDateTimeOffset.Now, reference: eventReference);
					Logs.AddNew(eventValue);
				}
			}
		}

		public ZString GetDataModelAndPopulateDataModelIfNeeded()
		{
			PopulateDataModelIfNeeded();
			return JE_DataModel;
		}

		bool hasSetConcurrencyPolicy;
		readonly Dictionary<string, ZPropertyInfo> strictPolicyProperties = new Dictionary<string, ZPropertyInfo>();

		void SetConcurrencyPolicies()
		{
			if (IsInDatabase && !hasSetConcurrencyPolicy)
			{
				hasSetConcurrencyPolicy = true;
				foreach (ZPropertyInfo info in ZPropertyInfoHash)
				{
					if (info.IsPersistent && !strictPolicyProperties.ContainsKey(info.Name))
					{
						ConcurrencyInfo.SetConcurrencyPolicy(this, info.Name, ConcurrencyPolicy.Observe);
					}
				}

				ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(JE_InvisibleTabsXML), ConcurrencyPolicy.Ignore);
			}
		}

		void SetStrictConcurrencyPolicy(ZPropertyInfo info)
		{
			Factory.ThreadSentry.EnsureCurrentThreadIsOwner();

			if (IsInDatabase && info.HasChanges)
			{
				ConcurrencyInfo.SetConcurrencyPolicy(this, info.Name, ConcurrencyPolicy.Strict);
				if (!strictPolicyProperties.ContainsKey(info.Name))
				{
					strictPolicyProperties.Add(info.Name, info);
				}
			}
		}

		void UnsetStrictConcurrencyPolicies()
		{
			// Resetting policies to observe on successfull save
			foreach (var prop in strictPolicyProperties.Values)
			{
				ConcurrencyInfo.SetConcurrencyPolicy(this, prop.Name, ConcurrencyPolicy.Observe);
			}
			strictPolicyProperties.Clear();
		}

		void UpdateTransportFlightSubscriptionEvent()
		{
			foreach (var transport in Transports.OfType<Transport>().Where(t => t.IsAir))
			{
				FlightMonitoringSystemManager.UpdateFlightSubscriptionEvent(transport);
			}
		}

		void DeactivateJobHeaderWhenIsCancelled()
		{
			if (!this.HasContext(BusinessContext.InvoicingPlugInGUI) && IsCancelled && IsCancelledHasChanged)
			{
				JobHeader.DeactivateAllJobs(this, true);
			}
		}

		void DeleteDuplicatedBills()
		{
			Bills.Load();

			using (BillGUIPresentationFlagUpdater.SuspendSynch())
			{
				var pks = new List<ZGuid>(GetPrimaryBillsPKs());
				var billsToDelete = (from Bill bill in Bills where !pks.Contains(bill.PK) select bill).ToList();
				if (billsToDelete.Count > 0)
				{
					PackingGroups.Load();
					Packages.Load();
					billsToDelete.ForEach(bill =>
					{
						bill.LoadChildEditableObjects();
						bill.Delete();
					});
				}
			}
		}

		IEnumerable<ZGuid> GetPrimaryBillsPKs()
		{
			var primaryMasterBill = PrimaryMasterBill;
			if (primaryMasterBill != null)
			{
				yield return primaryMasterBill.PK;
			}

			var primaryHouseBill = PrimaryHouseBill;
			if (primaryHouseBill != null)
			{
				yield return primaryHouseBill.PK;
			}
		}

		bool DeclarationAlreadyExistsForShipment
		{
			get
			{
				bool result = false;
				if (!JE_JS.IsEmpty && Branch != null)
				{
					var filter = JobDeclarationFilter.ForCompanyAndShipment(true, Branch.Company, JE_JS);
					filter.FetchOnlyFromLocalCache = true;
					result = Factory.Load<BaseJobDeclaration>(filter).Length > 1 || (!IsInDatabase && Factory.ExistsInDatabase(BaseJobDeclaration.Schema.TableName, filter));
				}
				return result;
			}
		}

		void ThrowExceptionIfDeclarationAlreadyExistsForShipment()
		{
			if (DeclarationAlreadyExistsForShipment)
			{
				throw new ZCannotSaveException("Declaration already exists for current shipment. Please reopen the form and you will be able to edit it.", "DuplicatedDeclaration");
			}
		}

		public virtual bool ShouldDeleteContainers
		{
			get { return IsAir ? !ContainersAlwaysRequired : !ContainersRequired; }
		}

		void AddRequiredDocuments()
		{
			DocsAndCartage.RequiredDocuments.AddAndAcquitRequiredDocuments(SupplierImporterLink, null, null, null, JE_DateAtOrigin);

			if (!IsExport)
			{
				DocsAndCartage.RequiredDocuments.AddAndAcquitRequiredDocuments(Importer, JE_RL_NKOrigin, JE_RL_NKFinalDestination, Importer, JE_DateAtOrigin);
				DocsAndCartage.RequiredDocuments.AddAndAcquitRequiredDocuments(Supplier, JE_RL_NKOrigin, JE_RL_NKFinalDestination, Supplier, JE_DateAtOrigin);
			}
			else
			{
				DocsAndCartage.RequiredDocuments.AddAndAcquitRequiredDocuments(Supplier, JE_RL_NKOrigin, JE_RL_NKFinalDestination, Supplier, JE_DateAtOrigin);
				DocsAndCartage.RequiredDocuments.AddAndAcquitRequiredDocuments(Importer, JE_RL_NKOrigin, JE_RL_NKFinalDestination, Importer, JE_DateAtOrigin);
			}

			JobRequiredDocumentDependentCollection.DirectionFilterType directionType = IsExport ? JobRequiredDocumentDependentCollection.DirectionFilterType.Export : JobRequiredDocumentDependentCollection.DirectionFilterType.Import;//Need To Fix
			DocsAndCartage.RequiredDocuments.AddCountryRequiredDocuments(RefCountryRequiredDocumentSchema.RD_OnBrokerage, JE_TransportMode, CartageContainerMode, directionType, JE_RL_NKOrigin, JE_RL_NKFinalDestination);
		}

		public OrgSupplierBuyerLink SupplierImporterLink
		{
			get
			{
				OrgSupplierBuyerLink result = null;
				result = GetSupplierImporterLink(FinalDestinationCountryCode);
				if (result == null)
				{
					result = GetSupplierImporterLink(BranchCompanyCountryCode);
				}
				return result;
			}
		}

		OrgSupplierBuyerLink GetSupplierImporterLink(ZString countryCode)
		{
			return OrgSupplierBuyerLink.GetExistingOrgSupplierBuyerLink(Supplier, Importer, countryCode);
		}

		public override void OnSaved(bool saveSucceeded)
		{
			if (!saveSucceeded)
			{
				RecoverFromUnsuccessfulSave();
			}
			declarationReferenceAssignedFromNumberFountainOrShipment = false;
			ownerRefAssignedFromNumberFountain = false;
			if (saveSucceeded)
			{
				((IShouldUpdateScreeningStatus)this).ShouldUpdateScreeningStatus = false;
				UnsetStrictConcurrencyPolicies();

				isLegRemoved = false;
			}
			base.OnSaved(saveSucceeded);
		}

		public virtual void RecoverFromUnsuccessfulSave()
		{
			if (declarationReferenceAssignedFromNumberFountainOrShipment)
			{
				JE_DeclarationReference = ZString.Empty;
				declarationReferenceAssignedFromNumberFountainOrShipment = false;
			}
			if (ownerRefAssignedFromNumberFountain)
			{
				JE_OwnerRef = ZString.Empty;
				ownerRefAssignedFromNumberFountain = false;
			}

			if (IsInDatabase)
			{
				JE_MessageStatus = (ZString)JE_MessageStatusInfo.OriginalValue;
				JE_ConsolidatedCargoStatus = (ZString)JE_ConsolidatedCargoStatusInfo.OriginalValue;
			}
			else
			{
				JE_MessageStatus = ZString.Empty;
				JE_ConsolidatedCargoStatus = ZString.Empty;
			}
		}

		public void DeleteAnyNewMessages()
		{
			if (fMessages == null)
			{
				return;
			}

			foreach (EDIMessage message in Messages.ToArray())
			{
				if (message.IsTransmitMessage && !message.IsInDatabase && !message.IsDeleted)
				{
					message.Delete();
				}
			}
		}

		public override void Delete()
		{
			var shipment = Shipment;
			if (shipment == null)
			{
				RegisterEditableChildObject(DocsAndCartage); // need to registered so that fetch for delete to work correctly.
			}
			else if (shipment.IsInDatabase)
			{
				ErrorReporter.ReportOnce("JobDeclaration should not be deleted if its shipment is still in the database!");
			}
			this.LoadChildrenForDeletionIncludingCusAddInfoAndCusCodeData();

			if (!IsStandAlone && IsInDatabase)
			{
				ErrorReporter.ReportOnce("JobDeclaration should not be deleted if it is still in the database and not stand alone!");
			}

			using (GetValidationSuspender())
			{
				PackingGroups.ForEach(packingGroup => packingGroup.FetchStrategy.FetchForDelete());
				Bills.RemoveAndDeleteAll();
				CusContainers.RemoveAndDeleteAll();
				CustomsEntryHeaders.RemoveAndDeleteAll();
				CustomsEntryInstructionProvider.DeleteAll();
				JobComInvoiceGroupHeaders.RemoveAndDeleteAll();
				Factory.Load<CusPackingList>(new ZQuery(CusPackingListSchema.CUL_JE, PK)).DeleteAll();
				DeclarationRefs.DeleteAll();
				Equipments.DeleteAll();

				foreach (Order anOrder in AttachedOrders.ToArray())
				{
					if (anOrder.JD_JE == PK)
					{
						anOrder.JD_JE = ZGuid.Empty;
					}
				}
				if (shipment == null)
				{
					DeleteDocsAndCartage();
				}
				this.DeleteAllCusAddInfoCodeDataAndSupportingInfoChildrenIfSupported();
			}
			WorkflowItems.RemoveAndDeleteAll();
			RelatedChildActivityPivotCollection.DeleteAll();
			RelatedParentActivityPivotCollection.DeleteAll();
			DetachAdditionalInvoices();
			if (Shipment != null)
			{
				ShipmentSynchroniser.SetEnabled(false, ShipmentSynchroniser.DetectEnabled);
			}
			new GenCustomAddOnRuleAckCollection(this).DeleteAll();
			UnlockDoMergeMutex();
			base.Delete();
		}

		public void ReleaseInvoiceLines()
		{
			fInvoiceLines = null;
			fFilteredInvoiceLines = null;
		}

		protected virtual List<ZPropertyInfo> PropertiesThatShouldNotBeCloned
		{
			get { return new List<ZPropertyInfo>(); }
		}

		protected virtual ZBool CloneBills
		{
			get { return true; }
		}

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		public ZBool SupportScreeningPresentation => SupportScreeningPresentationCore;

		protected virtual ZBool SupportScreeningPresentationCore => ZBool.True;

		public ZBool IsBillIssueDateVisible
		{
			get { return IsBillIssueDateVisibleCore; }
		}

		protected virtual ZBool IsBillIssueDateVisibleCore
		{
			get { return IsPackingInformationRelevant; }
		}

		protected override BusinessObject[] BusinessObjectsWithRelatedEventsCore
		{
			get
			{
				var result = new List<BusinessObject>(base.BusinessObjectsWithRelatedEventsCore);
				if (!IsDeleted)
				{
					var loader = new InvoiceLoader(Factory);
					result.AddRange(loader.GetInvoicesForUniqueRef(JE_DeclarationReference));
					result.AddRange(CustomsEntryHeaders);
					result.AddRange(CusContainers);
					result.AddRange(CusContainers.Cast<BaseCusContainer>().Select(container => container.JobContainer));

					var jobHeader = Job;
					if (jobHeader != null)
					{
						result.Add(jobHeader);
					}

					result.AddRange(Transports.OfType<Transport>());

					result.AddRange((BusinessObject[])Factory.Load<ICommonCartage>(new ZQuery(JobCartageSchema.JJ_ParentID, PK)));
					result.AddRange(TransportBookingLoader.GetRelatedTransportBookingEvents(this));
					result.AddRange(Invoices);

					foreach (var invoiceHeader in Invoices)
					{
						result.AddRange(invoiceHeader.BusinessObjectsWithRelatedEvents);
					}
				}
				return result.ToArray();
			}
		}

		public override Notes Notes
		{
			get
			{
				var result = base.Notes;

				if (!IsPersistent && shouldOverrideNotes && Invoices.Count > 0)
				{
					result = new Notes(Invoices[0]);
				}
				return result;
			}
		}

		#region JI_AddInfo

		[BusinessObjectTestExclude]
		public override ZString JE_AddInfo
		{
			get
			{
				return this is INAddInfoSupporter ? AddInfoParser.ConcatAddInfoStrings(base.JE_AddInfo, base.JE_NAddInfo) : base.JE_AddInfo;
			}
			set
			{
				if (this.GetBaseAddInfoWithNAddInfoSupport() is BaseAddInfo addInfo)
				{
					var addInfoStrings = addInfo.SplitAddInfoString(value);

					base.JE_AddInfo = addInfoStrings.Item1;
					base.JE_NAddInfo = addInfoStrings.Item2;
				}
				else
				{
					base.JE_AddInfo = value;
				}
			}
		}

		#endregion

#if DEBUG
		protected override BusinessObjectTestDataHelper NewBusinessObjectTestDataHelper()
		{
			return new JobDeclarationBusinessObjectTestDataHelper(this);
		}

		public class JobDeclarationBusinessObjectTestDataHelper : BusinessObjectTestDataHelper
		{
			public JobDeclarationBusinessObjectTestDataHelper(BaseJobDeclaration declaration)
			{
				this.declaration = declaration;
			}

			protected override void FillDependentCollectionWithData(IBusinessObjectCollection collection, TestBusinessObjectKind kind, PropertyDescriptor collectionProperty, PropertyDescriptor[] propertyPath)
			{
				if ((collectionProperty.Name != "CusContainers" && collectionProperty.Name != "AUCusContainers" || declaration.CusContainers.Count == 0)
					&& collectionProperty.Name != "RelatedDeclarations" && collectionProperty.Name != "DeclarationRefs" && collectionProperty.Name != nameof(BaseJobDeclaration.Equipments))
				{
					base.FillDependentCollectionWithData(collection, kind, collectionProperty, propertyPath);
				}
			}

			protected override void PopulateUniqueString(ZPropertyInfo property, PropertyDescriptor[] propertyPath, int maxLength)
			{
				if (property.Name == Schema.JE_DeclarationReference)
				{
					property.Value = ZString.Empty;
					return;
				}

				base.PopulateUniqueString(property, propertyPath, maxLength);
			}

			readonly BaseJobDeclaration declaration;
		}
#endif

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.IATALoadPorts))]
		public override ZString JE_IATALoadPort { get => base.JE_IATALoadPort; set => base.JE_IATALoadPort = value; }

		[NotDefaultingPropertyValue]
		public override ZString JE_DeclarationReference
		{
			get { return base.JE_DeclarationReference; }
			set
			{
				if (CustomsEntryHeaders.Count > 0)
				{
					using (new MergeManager.ChangingMergedDeclarationInAWayThatDoesNotRequireReMerge(this))
					{
						base.JE_DeclarationReference = value;
					}
				}
				else
				{
					base.JE_DeclarationReference = value;
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.GoodsOrigin))]
		public override ZString JE_GoodsOrigin
		{
			get
			{
				return base.JE_GoodsOrigin;
			}
			set
			{
				base.JE_GoodsOrigin = value;
			}
		}

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.GoodsDestination))]
		public override ZString JE_GoodsDestination
		{
			get
			{
				return base.JE_GoodsDestination;
			}
			set
			{
				base.JE_GoodsDestination = value;
			}
		}

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.IncoTermList))]
		public override ZString JE_ShipmentIncoTerm
		{
			get => base.JE_ShipmentIncoTerm;
			set => base.JE_ShipmentIncoTerm = value;
		}

		[ResourceStringData("2F490578-C7C5-4C98-8D7F-1BD5DF0EE85C", Caption = "Language")]
		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.DeclarationLanguageList))]
		[MaxLength(2)]
		public override ZString JE_DeclarationLanguage
		{
			get => base.JE_DeclarationLanguage;
			set => base.JE_DeclarationLanguage = value;
		}

		public override ZGuid JE_GB
		{
			get { return base.JE_GB; }
			set
			{
				var oldValue = JE_GB;
				base.JE_GB = value;
				if (!IsCopying && oldValue != JE_GB)
				{
					EnsureThatJE_GCIsSameAsBranchCompany();
					CustomsEntryHeaders.MarkAsNeedingValidation();
					Bills.MarkAsNeedingValidation();
					Invoices.MarkAsNeedingValidation();
					InvoiceLines.MarkAsNeedingValidation();
					RefreshIncotermAndChargeFactory();
				}
			}
		}

		public override ZGuid JE_GC
		{
			get { return base.JE_GC; }
			set
			{
				var oldValue = JE_GC;
				base.JE_GC = value;
				if (!IsCopying && oldValue != JE_GC)
				{
					EnsureThatJE_GBBelongsToSameCompany();
					CustomsEntryHeaders.MarkAsNeedingValidation();
					Bills.MarkAsNeedingValidation();
					Invoices.MarkAsNeedingValidation();
					InvoiceLines.MarkAsNeedingValidation();
					ActiveEntryHeaders.Cast<CusEntryHeader>().ForEach(x => x.Charges.ForEach(y => y.MarkAsNeedingValidation()));
					RefreshIncotermAndChargeFactory();
				}
			}
		}

		public virtual ZString BillingBranch
		{
			get
			{
				if (jobBranch == null)
				{
					jobBranch = Job?.Branch;
				}

				return jobBranch != null ? jobBranch.GB_Code : ZString.Empty;
			}
		}
		GlbBranch jobBranch;

		public virtual ZString BillingDepartment
		{
			get
			{
				if (jobDepartment == null)
				{
					jobDepartment = Job?.Department;
				}

				return jobDepartment != null ? jobDepartment.GE_Code : ZString.Empty;
			}
		}
		GlbDepartment jobDepartment;

		public virtual ZString BillingTaxBranch
		{
			get
			{
				if (jobTaxBranch == null)
				{
					jobTaxBranch = Job?.TaxBranch;
				}

				return jobTaxBranch != null ? jobTaxBranch.GB_Code : ZString.Empty;
			}
		}
		GlbBranch jobTaxBranch;

		public virtual ZString BillingOperator
		{
			get { return Job != null ? Job.JH_GS_NKRepOps : ZString.Empty; }
		}

		public override bool ReadOnly
		{
			get { return base.ReadOnly; }
			set
			{
				base.ReadOnly = value;

				if (readOnlyIncludingChildrenNeedsRecalculation)
				{
					readOnlyIncludingChildrenNeedsRecalculation = false;
					SetReadOnlyIncludingChildren(value);
				}
			}
		}
		bool readOnlyIncludingChildrenNeedsRecalculation = true;

		#endregion

		#region Collections

		public virtual bool SupportInvoiceLineRefs => false;

		public bool SupportsJobComInvoiceLineTax => SupportsJobComInvoiceLineTaxCore;
		protected virtual bool SupportsJobComInvoiceLineTaxCore => false;

		internal protected virtual bool SupportContainerEntryHeaderPivot => false;

		internal protected virtual bool SupportContainerEntryInstructionPivot => false;

		#region Declaration Refs Collection

		[ChildEditable(true)]
		[ChildEditableTestExclude]
		public JobDecRefsCollection DeclarationRefs
		{
			get
			{
				if (declarationRefs == null)
				{
					declarationRefs = GetNewJobDecRefsCollection();
					if (SupportDeclarationRefs)
					{
						RegisterEditableChildObject(declarationRefs);
					}
					else
					{
						declarationRefs.AdditionalFilter = ZQuery.NoResultQuery;
					}
				}
				return declarationRefs;
			}
		}
		JobDecRefsCollection declarationRefs;

		protected virtual JobDecRefsCollection GetNewJobDecRefsCollection()
		{
			return new JobDecRefsCollection(this);
		}

		internal protected virtual bool SupportDeclarationRefs
		{
			get { return false; }
		}

		#endregion

		[ChildEditable(false)]
		public IJobComInvoiceGroupHeaderCollection<BaseJobComInvoiceGroupHeader> JobComInvoiceGroupHeaders
		{
			get
			{
				if (fJobComInvoiceGroupHeaders == null)
				{
					fJobComInvoiceGroupHeaders = CreateNewJobComInvoiceGroupHeaderCollection();
					RegisterEditableChildObject(fJobComInvoiceGroupHeaders);
				}
				return fJobComInvoiceGroupHeaders;
			}
		}

		protected BaseJobComInvoiceGroupHeaderSingleElementCollection fActiveGroupHeader;
		public BaseJobComInvoiceGroupHeaderSingleElementCollection ActiveGroupHeader
		{
			get
			{
				if (fActiveGroupHeader == null)
				{
					fActiveGroupHeader = GetSingleElementCollection();
				}

				if (fActiveGroupHeader.Count == 0 && JobComInvoiceGroupHeaders.Count > 0)
				{
					fActiveGroupHeader.Add(JobComInvoiceGroupHeaders[0]);
				}

				return fActiveGroupHeader;
			}
		}

		protected virtual BaseJobComInvoiceGroupHeaderSingleElementCollection GetSingleElementCollection()
		{
			return new BaseJobComInvoiceGroupHeaderSingleElementCollection(Factory);
		}

		#region Equipment Collection

		[ChildEditable(true)]
		[ChildEditableTestExclude]
		public ICusEquipmentCollection<CusEquipment> Equipments
		{
			get
			{
				if (equipments == null)
				{
					equipments = GetNewCusEquipmentCollection();
					if (SupportEquipments)
					{
						RegisterEditableChildObject(equipments);
					}
					else
					{
						equipments.AdditionalFilter = ZQuery.NoResultQuery;
					}
				}
				return equipments;
			}
		}
		ICusEquipmentCollection<CusEquipment> equipments;

		protected virtual ICusEquipmentCollection<CusEquipment> GetNewCusEquipmentCollection() => new CusEquipmentCollection<CusEquipment>(this);

		public virtual bool EquipmentsRequired => SupportEquipments;
		public bool SupportEquipments => SupportEquipmentsCore;
		protected virtual bool SupportEquipmentsCore => false;

		public string ContainerOrEquipmentCaption
		{
			get
			{
				string caption;
				var isContainersRequired = ContainersRequired;
				var isEquipmentsRequired = EquipmentsRequired;
				if (isContainersRequired && isEquipmentsRequired)
				{
					caption = Res.GetString("F9BF6B27-7B8E-4E07-A924-6F10C128F8AD", "Container/Equipment");
				}
				else if (isEquipmentsRequired)
				{
					caption = Res.GetString("F9BF6B27-7B8E-4E07-A924-6F10C128F8AE", "Equipment");
				}
				else
				{
					caption = Res.GetString("F9BF6B27-7B8E-4E07-A924-6F10C128F8AF", "Container No");
				}
				return caption;
			}
		}

		#endregion

		[ChildEditable(true)]
		public ICusContainerCollection<BaseCusContainer> CusContainers
		{
			get
			{
				if (fCusContainers == null)
				{
					fCusContainers = NewCusContainersCollection();
					fCusContainers.Load();
					fCusContainers.CountChanged += CusContainers_CountChanged;
					RegisterEditableChildObject(fCusContainers);
				}
				return fCusContainers;
			}
		}

		public bool HasCusContainers
		{
			get { return CusContainers.Count > 0; }
		}

		public CodeDescriptionPairList ContainersAndEquipmentsOnDeclaration_List
		{
			get
			{
				if (containersAndEquipmentsOnDeclaration_List == null)
				{
					containersAndEquipmentsOnDeclaration_List = new CodeDescriptionPairList();
					if (ContainersRequired)
					{
						CusContainers.Cast<BaseCusContainer>().ForEach(x => AddToListIfNeeded(containersAndEquipmentsOnDeclaration_List, x.CO_ContainerNumberInfo));
					}
					if (EquipmentsRequired)
					{
						Equipments.Cast<CusEquipment>().ForEach(x => AddToListIfNeeded(containersAndEquipmentsOnDeclaration_List, x.CEQ_IdentificationNumberInfo));
					}
					containersAndEquipmentsOnDeclaration_List.Sort();
				}
				return containersAndEquipmentsOnDeclaration_List;
			}
		}
		CodeDescriptionPairList containersAndEquipmentsOnDeclaration_List;

		public void ResetContainersAndEquipmentsOnDeclaration_List() => containersAndEquipmentsOnDeclaration_List = null;

		void CusContainers_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			if (e.ItemRemoved)
			{
				RemoveFromContainersAndEquipmentsOnDeclaration_ListIfNeeded(e.BizObject);
			}
		}

		public void AddToContainersAndEquipmentsOnDeclaration_ListIfNeeded(ZPropertyInfo info, bool sort = false)
		{
			if (containersAndEquipmentsOnDeclaration_List != null)
			{
				AddToListIfNeeded(containersAndEquipmentsOnDeclaration_List, info);
				if (sort)
				{
					containersAndEquipmentsOnDeclaration_List.Sort();
				}
			}
		}

		void AddToListIfNeeded(CodeDescriptionPairList list, ZPropertyInfo info)
		{
			if (info.Value is ZString number)
			{
				var bizObj = info.BizObj;
				var descriptionPrefix = bizObj is CusEquipment ? (NoResString)"Equipment:" : string.Empty;
				list.Add(new BusinessObjectElement(bizObj, number, descriptionPrefix + number));
			}
		}

		public void RemoveFromContainersAndEquipmentsOnDeclaration_ListIfNeeded(BusinessObject bizObj)
		{
			if (containersAndEquipmentsOnDeclaration_List != null)
			{
				RemoveFromListIfNeeded(containersAndEquipmentsOnDeclaration_List, bizObj);
			}
		}

		public void RemoveFromListIfNeeded(CodeDescriptionPairList list, BusinessObject bizObj)
		{
			var elementToRemove = list.Cast<BusinessObjectElement>().FirstOrDefault(x => object.ReferenceEquals(x.BizObject, bizObj));
			if (elementToRemove != null)
			{
				list.Remove(elementToRemove);
			}
		}

		public IDisposable SuspendMessageTypeChangeProcess()
		{
			return new MessageTypeChangeProcessSuspender(this);
		}

		protected bool IsMessageTypeChangeProcessSuspended
		{
			get { return messageTypeChangeProcessIndex > 0; }
		}

		#region SellerOrgPK
		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.SellerConsignors))]
		public ZGuid SellerOrgPK
		{
			get { return JE_OA_SellerAddress_ZAddress.OrgPK; }
			set { JE_OA_SellerAddress_ZAddress.OrgPK = value; }
		}

		public ZPropertyInfo SellerOrgPKInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.SellerOrgPK, x => JE_OA_SellerAddress_ZAddress.OrgPKInfo); }
		}

		public OrgHeader Seller
		{
			get { return Factory.Load<OrgHeader>(SellerOrgPK); }
		}

		#endregion

		#region ConsigneeAddressOrgPK
		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.Consignees))]
		public ZGuid ConsigneeAddressOrgPK
		{
			get { return JE_OA_ConsigneeAddress_ZAddress.OrgPK; }
			set { JE_OA_ConsigneeAddress_ZAddress.OrgPK = value; }
		}

		public ZPropertyInfo ConsigneeAddressOrgPKInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ConsigneeAddressOrgPK, x => JE_OA_ConsigneeAddress_ZAddress.OrgPKInfo); }
		}

		public OrgHeader ConsigneeOrgAddress
		{
			get { return Factory.Load<OrgHeader>(ConsigneeAddressOrgPK); }
		}

		#endregion

		public virtual bool UseImporterAddress => false;

		#region ImporterAddressOrgPK

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.Importers))]
		public override ZGuid JE_OA_ImporterAddress
		{
			get => base.JE_OA_ImporterAddress;
			set
			{
				var oldValue = JE_OA_ImporterAddress;
				base.JE_OA_ImporterAddress = value;
				if (UseImporterAddress && !IsImporterAddressDefaultingSuspended && !IsCopying && oldValue != JE_OA_ImporterAddress)
				{
					var importerPK = ImporterAddressOrgPK;
					if (JE_OH_Importer != importerPK)
					{
						using (SuspendImporterAddressDefaulting())
						{
							JE_OH_Importer = importerPK;
						}
					}
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.Importers))]
		public ZGuid ImporterAddressOrgPK
		{
			get { return JE_OA_ImporterAddress_ZAddress.OrgPK; }
			set { JE_OA_ImporterAddress_ZAddress.OrgPK = value; }
		}

		public ZPropertyInfo ImporterAddressOrgPKInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ImporterAddressOrgPK, x => JE_OA_ImporterAddress_ZAddress.OrgPKInfo); }
		}

		public OrgHeader ImporterOrgAddress
		{
			get { return Factory.Load<OrgHeader>(ImporterAddressOrgPK); }
		}

		#endregion

		public virtual bool UseSupplierAddress => false;

		#region SupplierAddressOrgPK

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.Suppliers))]
		public override ZGuid JE_OA_SupplierAddress
		{
			get => base.JE_OA_SupplierAddress;
			set
			{
				var oldValue = JE_OA_SupplierAddress;
				base.JE_OA_SupplierAddress = value;
				if (UseSupplierAddress && !IsSupplierAddressDefaultingSuspended && !IsCopying && oldValue != JE_OA_SupplierAddress)
				{
					var supplierPK = SupplierAddressOrgPK;
					if (JE_OH_Supplier != supplierPK)
					{
						using (SuspendSupplierAddressDefaulting())
						{
							JE_OH_Supplier = supplierPK;
						}
					}
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.Suppliers))]
		public ZGuid SupplierAddressOrgPK
		{
			get { return JE_OA_SupplierAddress_ZAddress.OrgPK; }
			set { JE_OA_SupplierAddress_ZAddress.OrgPK = value; }
		}

		public ZPropertyInfo SupplierAddressOrgPKInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.SupplierAddressOrgPK, x => JE_OA_SupplierAddress_ZAddress.OrgPKInfo); }
		}

		public OrgHeader SupplierOrgAddress
		{
			get { return Factory.Load<OrgHeader>(SupplierAddressOrgPK); }
		}

		#endregion

		#region ManufacturerOrgPK

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.Manufacturers))]
		public override ZGuid JE_OA_ManufacturerAddress
		{
			get => base.JE_OA_ManufacturerAddress;
			set => base.JE_OA_ManufacturerAddress = value;
		}

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.Manufacturers))]
		public ZGuid ManufacturerOrgPK
		{
			get { return JE_OA_ManufacturerAddress_ZAddress.OrgPK; }
			set { JE_OA_ManufacturerAddress_ZAddress.OrgPK = value; }
		}

		public ZPropertyInfo ManufacturerOrgPKInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ManufacturerOrgPK, x => JE_OA_ManufacturerAddress_ZAddress.OrgPKInfo); }
		}
		#endregion

		#region ShipToPartyOrgPK
		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.ShipToParties))]
		public ZGuid ShipToPartyOrgPK
		{
			get { return JE_OA_ShipToPartyAddress_ZAddress.OrgPK; }
			set { JE_OA_ShipToPartyAddress_ZAddress.OrgPK = value; }
		}

		public ZPropertyInfo ShipToPartyOrgPKInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ShipToPartyOrgPK, x => JE_OA_ShipToPartyAddress_ZAddress.OrgPKInfo); }
		}

		public OrgHeader ShipToParty
		{
			get { return Factory.Load<OrgHeader>(ShipToPartyOrgPK); }
		}

		#endregion

		#region DistributorOrgPK
		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.Distributors))]
		public ZGuid DistributorOrgPK
		{
			get { return JE_OA_DistributorAddress_ZAddress.OrgPK; }
			set { JE_OA_DistributorAddress_ZAddress.OrgPK = value; }
		}

		public ZPropertyInfo DistributorOrgPKInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.DistributorOrgPK, x => JE_OA_DistributorAddress_ZAddress.OrgPKInfo); }
		}
		#endregion

		#region PackagerOrgPK
		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.Packagers))]
		public ZGuid PackagerOrgPK
		{
			get { return JE_OA_PackagerAddress_ZAddress.OrgPK; }
			set { JE_OA_PackagerAddress_ZAddress.OrgPK = value; }
		}

		public ZPropertyInfo PackagerOrgPKInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.PackagerOrgPK, x => JE_OA_PackagerAddress_ZAddress.OrgPKInfo); }
		}
		#endregion

		#region ShipperOrgPK
		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.Shippers))]
		public ZGuid ShipperOrgPK
		{
			get { return JE_OA_ShipperAddress_ZAddress.OrgPK; }
			set { JE_OA_ShipperAddress_ZAddress.OrgPK = value; }
		}

		public ZPropertyInfo ShipperOrgPKInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ShipperOrgPK, x => JE_OA_ShipperAddress_ZAddress.OrgPKInfo); }
		}
		#endregion

		#region JE_MessageType

		int messageTypeChangeProcessIndex;
		class MessageTypeChangeProcessSuspender : IDisposable
		{
			public MessageTypeChangeProcessSuspender(BaseJobDeclaration declaration)
			{
				this.declaration = declaration;
				declaration.messageTypeChangeProcessIndex++;
			}

			readonly BaseJobDeclaration declaration;

			public void Dispose()
			{
				declaration.messageTypeChangeProcessIndex--;
			}
		}

		internal sealed class ValueChangeEventLog
		{
			public IZType OldValue { get; set; }

			public IZType NewValue { get; set; }

			public string StackTrace { get; set; }
		}

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.MessageTypeList))]
		public override ZString JE_MessageType
		{
			get
			{
				return base.JE_MessageType;
			}
			set
			{
				if (!IsJE_MessageTypeSettingSuspended && !SetterSuspender.IsSetterSuspended(BaseJobDeclaration.Schema.JE_MessageType))
				{
					if (ShouldCreateInvoiceForExWarehouse
						&& !IsCopying && base.JE_MessageType != JobMessageTypeList.Codes.ExWarehouse
						&& value == JobMessageTypeList.Codes.ExWarehouse
						&& Invoices.Count == 0)
					{
						BaseJobComInvoiceHeader header = Invoices.AddNew();
						header.JZ_InvoiceNumber = GetAutoCreatedExBondInvoiceNumber();
						header.JZ_RX_NKInvoice_Currency = LocalCurrencyCode;
					}

					var oldValue = base.JE_MessageType;
					var isOldValueExportOrNonTransport = IsExportOrNonTransport;
					base.JE_MessageType = value;
					var newValue = JE_MessageType;
					if (!IsCopying && newValue != oldValue)
					{
						OnMultipleKeyToUseChanged();
						if (!IsMessageTypeChangeProcessSuspended && !IsDataChangeSuspendedByFakeDeclaration)
						{
							ResetContainersAndEquipmentsOnDeclaration_List();
							MarkAsNeedingValidationForMajorDataChange();
							JE_MessageTypeChanged(oldValue, newValue);
							ResetCachedValuesOnChangeOfMessageType();

							DefaultPackDetailsToPackingGroupIfRequired(Schema.JE_TotalNoOfPacks);
							if (!IsSettingDefaultValues)
							{
								DefaultCartageEquipment();
								SetDefaultCartageOrg();
								MessageTypeBasedValueDefaulter.DefaultAll();
								var isNewValueExportOrNonTransport = IsExportOrNonTransport;
								if (isOldValueExportOrNonTransport != isNewValueExportOrNonTransport && Shipment == null)
								{
									ClearCartageData(isNewValueExportOrNonTransport);
								}
								var isExport = IsExport;

								var isImport = IsImport;
								if (isImport || isExport)
								{
									this.DefaultExternalBroker(isExport, isImport);
									this.DefaultForwarder(isExport, isImport);
									this.DefaultDepot(isExport, isImport);
									this.DefaultCTO(isExport, isImport);
									this.DefaultContainerYard(isExport, isImport);
									this.DefaultWarehouseDocAddress(isExport, isImport);
								}
							}
						}
					}

					NotifyInvoiceLinkIfChangedToFromExWarehousing(oldValue, value);
					SetStrictConcurrencyPolicy(JE_MessageTypeInfo);
				}
			}
		}

		protected bool IsJE_MessageTypeSettingSuspended => isJE_MessageTypeSettingSuspended != 0;

		public IDisposable SuspendJE_MessageTypeSetting()
		{
			return new DisposableAction(() => isJE_MessageTypeSettingSuspended++, () => isJE_MessageTypeSettingSuspended--);
		}
		byte isJE_MessageTypeSettingSuspended;

		public virtual bool IsMessageTypeChangeAnError => DeclarationMessagesHaveBeenSent(reloadMessages: true);

		internal bool ShouldLogEventJE_MessageTypeChanged => IsInDatabase && ((IUser)GlbStaff.CurrentUser).IsBatchProcessor && IsMessageTypeChangeAnError;

		public void ResetMessageTypeChangeLogs()
		{
			if (messageTypeChangeLogs != null)
			{
				messageTypeChangeLogs.Clear();
				messageTypeChangeLogs = null;
			}
		}

		internal IEnumerable<ValueChangeEventLog> GetMessageTypeChangeLogs() => messageTypeChangeLogs ?? Enumerable.Empty<ValueChangeEventLog>();
		List<ValueChangeEventLog> messageTypeChangeLogs;

		void LogEventJE_MessageTypeChanged(ZString oldValue, ZString newValue)
		{
			if (ShouldLogEventJE_MessageTypeChanged)
			{
				var eventLog = new ValueChangeEventLog
				{
					NewValue = newValue,
					OldValue = oldValue,
					StackTrace = System.Environment.StackTrace,
				};
				(messageTypeChangeLogs ??= new List<ValueChangeEventLog>()).Add(eventLog);
			}
			else
			{
				ResetMessageTypeChangeLogs();
			}
		}

		void ReportEventIfJE_MessageTypeChanged()
		{
			if (messageTypeChangeLogs?.Count > 0)
			{
				foreach (var jeEntryStatusChangeLog in messageTypeChangeLogs)
				{
					var key = $"JE_MessageType changed for {JE_ApplicationCode}/{CountryCode}/{GetType().FullName}";
					var msg = $"JE_MessageType was changed " +
						$"from {jeEntryStatusChangeLog.OldValue} " +
						$"to {jeEntryStatusChangeLog.NewValue} " +
						$"when not allowed for " +
						$"JE_ApplicationCode: {JE_ApplicationCode}/" +
						$"CountryCode: {CountryCode}/" +
						$"Type: {GetType().FullName}, " +
						$"changed by system user. " +
						$"StackTrace: {jeEntryStatusChangeLog.StackTrace}";

					ErrorReporter.ReportOnce(key, msg);
				}

				messageTypeChangeLogs.Clear();
				messageTypeChangeLogs = null;
			}
		}

		#endregion

		void ClearCartageData(bool isExportOrNonTransport)
		{
			var docsAndCartage = DocsAndCartage;
			if (isExportOrNonTransport)
			{
				ClearData(docsAndCartage.JP_EstimatedDeliveryInfo);
				ClearData(docsAndCartage.JP_DeliveryRequiredByInfo);
				ClearData(docsAndCartage.JP_DeliveryCartageAdvisedInfo);
				ClearData(docsAndCartage.JP_DeliveryCartageCompletedInfo);
				ClearData(docsAndCartage.JP_DeliveryLabourTimeInfo);
				ClearData(docsAndCartage.JP_DeliveryLabourChargeInfo);
				ClearData(docsAndCartage.JP_DeliveryTruckWaitTimeInfo);
				ClearData(docsAndCartage.JP_DeliveryTruckWaitChargeInfo);
				ClearData(docsAndCartage.DeliveryCartageCoPKInfo);
			}
			else
			{
				ClearData(docsAndCartage.JP_EstimatedPickupInfo);
				ClearData(docsAndCartage.JP_PickupRequiredByInfo);
				ClearData(docsAndCartage.JP_PickupCartageAdvisedInfo);
				ClearData(docsAndCartage.JP_PickupCartageCompletedInfo);
				ClearData(docsAndCartage.JP_PickupLabourTimeInfo);
				ClearData(docsAndCartage.JP_PickupLabourChargeInfo);
				ClearData(docsAndCartage.JP_PickupTruckWaitTimeInfo);
				ClearData(docsAndCartage.JP_PickupTruckWaitChargeInfo);
				ClearData(docsAndCartage.PickupCartageCoPKInfo);
			}
		}

		protected void ClearData(ZPropertyInfo info)
		{
			if (!info.Value.IsEmpty)
			{
				info.ClearValue();
			}
		}

		void ResetCachedValuesOnChangeOfMessageType()
		{
			documentSupporter = null;
		}

		public bool IsJE_MessageTypeChangedSinceLoading
		{
			get { return IsInDatabase && (ZString)JE_MessageTypeInfo.OriginalValue != JE_MessageType; }
		}

		public bool JE_MessageType_ReadOnly
		{
			get => JE_MessageType_ReadOnlyCore;
			set
			{
				if (messageType_ReadOnly != value)
				{
					messageType_ReadOnly = value;
					JE_MessageTypeInfo.RefreshBinding();
				}
			}
		}
		bool messageType_ReadOnly;

		protected virtual bool JE_MessageType_ReadOnlyCore => messageType_ReadOnly;

		protected virtual void JE_MessageTypeChanged(ZString oldValue, ZString newValue)
		{
			LogEventJE_MessageTypeChanged(oldValue, newValue);
			if (Shipment != null && ShouldSynchroniseWithShipment())
			{
				ShipmentSynchroniser.Synchronise(true);
			}
			else
			{
				ReCalculateRelatedPorts();
			}
		}

		void ReCalculateRelatedPorts()
		{
			var countryCode = CountryCode;
			if (!fIsImportingData)
			{
				if (IsExport && Supplier != null && !JE_RL_NKPortOfLoading.StartsWith(countryCode, StringComparison.CurrentCulture) && JE_RL_NKPortOfLoading == Supplier.OH_RL_NKClosestPort)
				{
					JE_RL_NKPortOfLoading = ZString.Empty;
					JE_RL_NKOrigin = ZString.Empty;
				}
				if (ShouldSetDefaultValuesFromSupplier && JE_RL_NKOrigin.IsEmpty)
				{
					DefaultOriginFromSupplier();
				}

				if (IsImport && Importer != null && !JE_RL_NKPortOfArrival.StartsWith(countryCode, StringComparison.CurrentCulture) && JE_RL_NKPortOfArrival == Importer.OH_RL_NKClosestPort)
				{
					JE_RL_NKPortOfArrival = ZString.Empty;
					JE_RL_NKFinalDestination = ZString.Empty;
				}
				if (JE_RL_NKFinalDestination.IsEmpty)
				{
					DefaultFinalDestinationPortFromImporter();
				}
			}
		}

		protected virtual bool ShouldCreateInvoiceForExWarehouse
		{
			get { return true; }
		}

		protected virtual string GetAutoCreatedExBondInvoiceNumber()
		{
			return "EX-BOND";
		}

		protected virtual bool ShouldDeactivateExistingEntriesOnMessageTypeSubTypeChanging(ZString oldMessageType, ZString newMessageType, ZString oldMessageSubType, ZString newMessageSubType)
		{
			return oldMessageType != newMessageType;
		}

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.MessageSubTypeList))]
		public override ZString JE_MessageSubType
		{
			get
			{
				return base.JE_MessageSubType;
			}
			set
			{
				bool hasChanged = base.JE_MessageSubType != value;
				base.JE_MessageSubType = value;
				if (hasChanged)
				{
					MarkAsNeedingValidationForMajorDataChange();
				}
			}
		}

		public bool JE_MessageSubType_ReadOnly { get; private set; }

#if DEBUG //Set by form basher to not modify the property value
		public void SetMessageSubTypeReadOnlyForTest(bool value)
		{
			JE_MessageSubType_ReadOnly = value;
		}
#endif

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.ApplicationCodeList))]
		[ReadOnlyMember(nameof(JE_ApplicationCode_ReadOnly))]
		public override ZString JE_ApplicationCode
		{
			get
			{
				return base.JE_ApplicationCode;
			}
			set
			{
				var oldValue = JE_ApplicationCode;
				base.JE_ApplicationCode = value;
				var newValue = JE_ApplicationCode;
				if (newValue != oldValue)
				{
					OnMultipleKeyToUseChanged();
					JE_PaidBy = IsDeclarationIntegrated && IsInterface ? PaidByCodeList.Codes.BRK : string.Empty;
					if (!IsDataChangeSuspendedByFakeDeclaration)
					{
						MarkAsNeedingValidationForMajorDataChange();
					}
				}
				SetStrictConcurrencyPolicy(JE_ApplicationCodeInfo);
			}
		}

		protected bool JE_ApplicationCode_ReadOnly => Factory.GetValue(ref je_ApplicationCode_ReadOnlyCached, CalculateJE_ApplicationCode_ReadOnly);
		CachedProperty<bool> je_ApplicationCode_ReadOnlyCached;

		protected virtual bool CalculateJE_ApplicationCode_ReadOnly()
		{
			var supportMultipleBuiltInTypes = SupportMultipleBuiltInTypes;
			var interfaceSubmissionType = GetInterfaceSubmissionType();
			return (!supportMultipleBuiltInTypes && interfaceSubmissionType == DeclarationApplicationCodeList.Codes.Builtin)
				|| interfaceSubmissionType == DeclarationApplicationCodeList.Codes.Interfaced
				|| (supportMultipleBuiltInTypes && IsBuiltinSubmissionType(JE_ApplicationCode.ToUpperInvariant()) ? ActiveEntryHeaders.Cast<CusEntryHeader>().Any(x => x.Messages.Count > 0) : CustomsEntryHeaders.Count > 0)
				|| (CountryCode is ZString countryCode && interfaceSubmissionType.IsEmpty && (!(IntegratedCountryHelper.CountryHasBuiltInDeclaration(countryCode) || IntegratedCountryHelper.CountryHasDeclarationInDevelopment(countryCode)) || IntegratedCountryHelper.IsUsedToBeBuiltInOnlyCountry(countryCode)));
		}

		protected virtual bool SupportMultipleBuiltInTypes => false;

		public ZString GetInterfaceSubmissionType()
		{
			var customsInterface = DataRegistry.Business.CustomsDataRegistry.Instance.LocalCountryCustomsInterface.GetFallBackValueAtAllLevels(RegistryCompanyPK, Guid.Empty, Guid.Empty);
			return customsInterface?.SubmissionType.ToUpperInvariant() ?? ZString.Empty;
		}

		#region DataChangeByFakeDeclarationSuspender
		public bool IsDataChangeSuspendedByFakeDeclaration
		{
			get { return suspendDataChangeByFakeDeclarationIndex > 0; }
		}
		int suspendDataChangeByFakeDeclarationIndex;

		public IDisposable SuspendDataChangeByFakeDeclaration()
		{
			return new DataChangeByFakeDeclarationSuspender(this);
		}

		class DataChangeByFakeDeclarationSuspender : IDisposable
		{
			public DataChangeByFakeDeclarationSuspender(BaseJobDeclaration declaration)
			{
				this.declaration = declaration;
				declaration.suspendDataChangeByFakeDeclarationIndex++;
			}

			public void Dispose()
			{
				declaration.suspendDataChangeByFakeDeclarationIndex--;
			}

			readonly BaseJobDeclaration declaration;
		}
		#endregion

		void NotifyInvoiceLinkIfChangedToFromExWarehousing(ZString oldValue, ZString newValue)
		{
			if (fWarehouseInvoiceLink != null && (
													(oldValue == JobMessageTypeList.Codes.ExWarehouse && newValue != JobMessageTypeList.Codes.ExWarehouse) ||
													(oldValue != JobMessageTypeList.Codes.ExWarehouse && newValue == JobMessageTypeList.Codes.ExWarehouse)))
			{
				fWarehouseInvoiceLink.NotifyChangedToFromExWarehousing();
			}
		}

		public bool HaveAllEntriesCleared
		{
			get
			{
				bool result = ActiveEntryHeaders.Count > 0;
				if (result)
				{
					foreach (CusEntryHeader header in ActiveEntryHeaders)
					{
						if (header.ClearanceDate.IsEmpty)
						{
							result = false;
							break;
						}
					}
				}
				return result;
			}
		}

		public ZString BGMReferences
		{
			get { return BGMReferencesCore; }
		}

		protected virtual ZString BGMReferencesCore
		{
			get { return ActiveEntryHeaders.BGMReferencesAsCommaDelimitedString; }
		}

		#region Invoices Collection (Invoice Headers)

		[ChildEditable(true)]
		public InvoiceHeaderActiveCollection Invoices
		{
			get
			{
				if (fInvoices == null)
				{
					EnsureNoStackOverflow(ref gettingInvoicesInProgress, nameof(Invoices), () =>
					{
						fInvoices = CreateNewInvoiceHeaderCollection();
						fInvoices.CountChanged += delegate
						{ InvoiceStructureChangeEvent.OnInvoiceStructureChanged(Factory); };
						fInvoices.CollectionCountChange += OnInvoicesCollectionCountChange;
						RegisterEditableChildObject(fInvoices);
					});
				}
				return fInvoices;
			}
		}
		InvoiceHeaderActiveCollection fInvoices;

		protected virtual void OnInvoicesCollectionCountChange(object sender, CollectionCountChangedEventArgs e)
		{
		}

		public virtual bool IsInvoicesRequiredToBeInSameCurrency => false;
		public virtual bool IsInvoicesRequiredToBeInSameIncoTerm => false;

		protected bool InvoicesIsLoaded
		{
			get { return fInvoices != null; }
		}

		protected virtual InvoiceHeaderActiveCollection CreateNewInvoiceHeaderCollection()
		{
			return new InvoiceHeaderActiveCollection(this);
		}

		public CodeDescriptionPairList SortedInvoiceList
		{
			get
			{
				if (sortedInvoiceList == null)
				{
					sortedInvoiceList = new CodeDescriptionPairList();
					var list = GetInvoiceListToSort();
					list.Sort((x, y) => StringComparer.OrdinalIgnoreCase.Compare(x.JZ_InvoiceDisplaySequence, y.JZ_InvoiceDisplaySequence));
					sortedInvoiceList.AddRange(list);
				}
				return sortedInvoiceList;
			}
		}
		CodeDescriptionPairList sortedInvoiceList;

		protected virtual List<BaseJobComInvoiceHeader> GetInvoiceListToSort() => Invoices.ToList();

		public void RefreshSortedInvoiceList()
		{
			sortedInvoiceList = null;
		}

		public virtual List<BaseJobComInvoiceHeader> LoadInvoicesFromQuery(ZQuery query)
		{
			return Factory.Load<BaseJobComInvoiceHeader>(query).ToList();
		}

		#endregion

		#region Packages

		[ChildEditable(true)]
		public IDeclarationLevelPackageCollection<BasePackage> Packages
		{
			get
			{
				if (fPackages == null)
				{
					fPackages = GetPackagesCollection();
					if (IsInDatabase)
					{
						fPackages.Load();
					}
					RegisterEditableChildObject(fPackages);
				}
				return fPackages;
			}
		}
		IDeclarationLevelPackageCollection<BasePackage> fPackages;

		protected virtual IDeclarationLevelPackageCollection<BasePackage> GetPackagesCollection()
		{
			return new BaseDeclarationLevelPackageCollection<BasePackage>(this);
		}

		/// <summary>
		/// SOON TO BE IMPLEMENTED
		/// </summary>
		public ZInt DangerousPackagesCount
		{
			get { return 0; }
		}

		public ZDecimal PackagesActualPackageCount
		{
			get
			{
				ZDecimal result = 0;
				foreach (BasePackingGroup packGroup in PackingGroups)
				{
					result += packGroup.TotalPackageCount();
				}
				return result;
			}
		}

		public ZPropertyInfo PackagesActualPackageCountInfo
		{
			get { return GetZPropertyInfo(Schema.PackagesActualPackageCount); }
		}

		public ZInt PackagesRequiredPackageCount
		{
			get
			{
				return JE_TotalNoOfPacks;
			}
		}

		public ZPropertyInfo PackagesRequiredPackageCountInfo
		{
			get { return GetZPropertyInfo(Schema.PackagesRequiredPackageCount); }
		}

		#endregion

		#region Lookups

		protected override JobDeclarationLookups GetNewLookups()
		{
			return new JobDeclarationLookups(this);
		}

		#endregion

		#region InvoiceLines Collection

		/// <summary>
		/// Filtered collection off from 'InvoiceLines' when users want to view classified lines
		/// this is the collection that is exposed on the form
		/// </summary>
		public IInvoiceLineViewCollection<BaseJobComInvoiceLine> FilteredInvoiceLines
		{
			get
			{
				if (fFilteredInvoiceLines == null)
				{
					fFilteredInvoiceLines = GetNewInvoiceLineViewCollection();
					fFilteredInvoiceLines.CopyLastLineDetailsToNewLines = AlwaysCopyFromPreviousLine;
				}
				return fFilteredInvoiceLines;
			}
		}
		IInvoiceLineViewCollection<BaseJobComInvoiceLine> fFilteredInvoiceLines;

		protected bool FilteredInvoiceLinesIsLoaded
		{
			get { return fFilteredInvoiceLines != null; }
		}

		protected virtual IInvoiceLineViewCollection<BaseJobComInvoiceLine> GetNewInvoiceLineViewCollection()
		{
			return new InvoiceLineViewCollection<BaseJobComInvoiceLine>(this);
		}

		/// <summary>
		/// Collection that has all the invoice lines for a declaration
		/// </summary>
		[ChildEditable(true)]
		public InvoiceLineCompleteCollection InvoiceLines
		{
			get
			{
				if (fInvoiceLines == null)
				{
					EnsureNoStackOverflow(ref gettingInvoiceLinesInProgress, nameof(InvoiceLines), () =>
					{
						fInvoiceLines = GetNewInvoiceLineCompleteCollection();
						using (WeightApportionManager.DeferWeightApportionment())
						{
							fInvoiceLines.Load();
						}
						RegisterEditableChildObject(fInvoiceLines);
					});
				}

				return fInvoiceLines;
			}
		}
		InvoiceLineCompleteCollection fInvoiceLines;

		IBusinessObjectCollection Integration.Customs.IBaseJobDeclaration.InvoiceLines => InvoiceLines;

		public bool IsInvoiceLinesLoaded
		{
			get { return fInvoiceLines != null; }
		}

		protected virtual InvoiceLineCompleteCollection GetNewInvoiceLineCompleteCollection()
		{
			return new InvoiceLineCompleteCollection(this);
		}
		#endregion

		#region SortedInvoiceLines
		public BaseJobComInvoiceLine[] SortedInvoiceLines
		{
			get
			{
				BaseJobComInvoiceLine[] unsortedLines = new BaseJobComInvoiceLine[InvoiceLines.Count];
				int counter = 0;
				foreach (BaseJobComInvoiceLine line in InvoiceLines)
				{
					unsortedLines[counter] = line;
					counter++;
				}
				BaseJobComInvoiceLine[] result = new BaseJobComInvoiceLine[unsortedLines.Length];
				BaseJobComInvoiceLine.LineComparer myComparer = new BaseJobComInvoiceLine.LineComparer();
				for (int j = 0; j < unsortedLines.Length; j++)
				{
					int minLineNum = -1;
					for (int i = 0; i < unsortedLines.Length; i++)
					{
						if (unsortedLines[i] != null && (minLineNum == -1 || myComparer.Compare(unsortedLines[i], unsortedLines[minLineNum]) < 0))
						{
							minLineNum = i;
						}
					}
					result[j] = unsortedLines[minLineNum];
					unsortedLines[minLineNum] = null;
				}
				return result;
			}
		}
		#endregion

		public ActiveCusEntryHeaderCollection ActiveEntryHeaders
		{
			get
			{
				if (fActiveEntryHeaders == null)
				{
					fActiveEntryHeaders = GetActiveEntryHeaderCollection();
				}
				return fActiveEntryHeaders;
			}
		}
		ActiveCusEntryHeaderCollection fActiveEntryHeaders;

		protected virtual ActiveCusEntryHeaderCollection GetActiveEntryHeaderCollection()
		{
			return new ActiveCusEntryHeaderCollection(this);
		}

		[ChildEditable(true)]
		public ICusEntryHeaderCollection<CusEntryHeader> CustomsEntryHeaders
		{
			get
			{
				if (fCustomsEntryHeaders == null)
				{
					fCustomsEntryHeaders = NewCustomsEntryHeaders();
					fCustomsEntryHeaders.Load();
					RegisterEditableChildObject(fCustomsEntryHeaders);
				}
				return fCustomsEntryHeaders;
			}
		}

		[ChildEditable(true)]
		[ChildEditableTestExclude]
		public BaseDeclarationLevelPackingGroupCollection PackingGroups
		{
			get
			{
				if (fPackingGroups == null)
				{
					fPackingGroups = CreateNewPackingGroups();
					fPackingGroups.Load();
				}
				if (IsPackingGroupCollectionRegisteredEditable)
				{
					RegisterEditableChildObject(fPackingGroups);
				}
				else
				{
					UnRegisterEditableChildObject(fPackingGroups);
				}
				return fPackingGroups;
			}
		}
		BaseDeclarationLevelPackingGroupCollection fPackingGroups;

		protected virtual bool IsPackingGroupCollectionRegisteredEditable
		{
			get { return true; }
		}

		protected virtual BaseDeclarationLevelPackingGroupCollection CreateNewPackingGroups()
		{
			return new BaseDeclarationLevelPackingGroupCollection(this);
		}

		#endregion

		#region Accounting Integration

		internal void IntegrateWithAccountingIfRequired()
		{
			new InvoicePostingAccountingIntegrator().IntegrateIfNecessary(GetJobDeclarationIAccIntegrationDataProvider());
		}

		protected virtual JobDeclarationIAccIntegrationDataProvider GetJobDeclarationIAccIntegrationDataProvider()
		{
			return new JobDeclarationIAccIntegrationDataProvider(this);
		}

		protected internal virtual IEnumerable<CusEntryHeader> EntryHeadersForPreCreditCheck
		{
			get => this.GetFormalEntries();
		}

		protected internal virtual void OnIntegratedWithAccountingSuccessfully()
		{
			RefreshAccounting_ARInvoiceQueryResult();
		}

		protected internal virtual bool IsJobReadyForPost
		{
			get { return true; }
		}

		protected internal virtual bool IsIntegrationWithAccountingSupported
		{
			get { return false; }
		}

		IJobInvoicingPlugIn ICustomsJobInfo.TopLevelObjectForJobToReference
		{
			get { return Shipment == null ? this : Shipment; }
		}

		ZString[] ICustomsJobInfo.GetValidAPInvoiceNumsToMatchAndValidateAgainst()
		{
			var result = new List<ZString>();

			var companyPK = Branch != null ? Branch.GB_GC.ToGuid() : GlbCompany.CurrentCompany.PK.ToGuid();
			var options = Customs.DataRegistry.Business.CustomsDataRegistry.Instance.EnableAccountingIntegration.GetFallBackValueAtAllLevels(companyPK, Guid.Empty, Guid.Empty);

			if (options.EnableAccountingIntegration)
			{
				foreach (CusEntryHeader entry in ActiveEntryHeaders)
				{
					if (entry.IsFormalEntry)
					{
						result.Add(((IAccInvoiceDataProvider)entry).UniqueNumber);
					}
				}
			}

			return result.ToArray();
		}

		AutoPostingNotification ICustomsJobInfo.AutoPostingNotification
		{
			get
			{
				var emailRecipients = new List<ZGuid>();
				var branch = Branch;
				var suspendUnpostedARNotification = false;
				if (branch != null)
				{
					var groupNotification = CustomsDataRegistry.Instance.AutoBillingEmailNotificationGroup.GetFallBackValueAtAllLevels(branch.GB_GC.ToGuid(), branch.PK.ToGuid(), Guid.Empty);
					suspendUnpostedARNotification = groupNotification.SuppressUnpostARNotificaiton;
					var notificationGroup = groupNotification.SendGroupPK;
					if (notificationGroup.IsValid)
					{
						emailRecipients.Add(notificationGroup);
					}
				}

				if (emailRecipients.Count == 0)
				{
					var staff = this.CusAgent;
					if (staff != null && !staff.GS_EmailAddress.IsEmpty)
					{
						emailRecipients.Add(staff.PK);
					}
				}

				return new AutoPostingNotification(emailRecipients.ToArray(), suspendUnpostedARNotification);
			}
		}

		ZGuid ICustomsJobInfo.CreditorPK
		{
			get
			{
				OrgHeader org = Factory.Load<OrgHeader>(Registry.Business.RatingDataRegistry.Instance.CustomsDisbursementCreditor.GetFallBackValueAtAllLevels(Branch.GB_GC.ToGuid(), Guid.Empty, Guid.Empty));
				return org == null ? ZGuid.Empty : org.PK;
			}
		}

		IJobHeaderParent IJobHeaderParentProvider.Parent
		{
			get { return Shipment == null ? this : Shipment; }
		}

		#endregion

		#region Warehouse Integration & Processings

		public ZString GetMessageErrorOfRequiredFieldsForBondedWarehousing(bool checkProduct = true, bool checkQuantity = true, bool checkEntryDetails = true)
		{
			return SupportMultipleWarehouseEntry ? (ZString)Res.GetString("{2891B7A3-2679-4532-8321-A73DF829C198}", "Cannot determine inventory management requirement for multi entries system.") : SingleWarehouseEntry?.GetMessageErrorOfRequiredFieldsForBondedWarehousing(checkProduct, checkQuantity, checkEntryDetails) ?? ZString.Empty;
		}

		public static string AtLeastOneInvoiceLineMarkedForBondedWarehousingIsRequired(string term)
		{
			return Res.GetString("D7BD3E95-83F1-4B58-BFA8-CA9790766442", "Please enter at least one invoice line with data for {0}.", term);
		}

		public static string SupplierMustBeMarkedAsWarehouseClient(string term)
		{
			return Res.GetString("4270E8DE-A57B-4ABC-B204-D3C6DCA91272", "A Supplier marked as a Warehouse Client is needed when {0} is enabled.", term);
		}

		public static string ImporterMustBeMarkedAsWarehouseClient(string term)
		{
			return Res.GetString("{4692F249-FE51-4A26-A68F-1E7542BDA852}", "An Importer marked as a Warehouse Client is needed when {0} is enabled.", term);
		}

		public static string InvoiceLineMarkedForBondedWarehousingRequiresAProduct(string term)
		{
			return Res.GetString("B1099ABE-0915-4311-848B-3C10B2BA054E", "An Invoice Line marked for {0} must have a valid product; not all Invoice Lines marked for {0} have a valid product specified.", term);
		}

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "String comparison")]
		public static string InvoiceLineMarkedForBondedWarehousingChangeOfOwnershipRequiresAnOwnerProduct(string term)
		{
			return Res.GetString("{FA00C6BB-083E-4462-8A5E-FB427370935C}", "An Invoice Line marked for {0} Change Of Ownership must have a valid owner's product; not all Invoice Lines marked for {0} Change Of Ownership have a valid owner's product specified.", term == "Inventory" ? (NoResString)"Bonded Warehouse" : term);
		}

		public static string InvoiceLineMarkedForBondedWarehousingRequiresAnInvoiceQuantityAndUnit(string term)
		{
			return Res.GetString("D329FEA3-F616-417B-9E3C-67383CB9C644", "An Invoice Line marked for {0} must have an invoice quantity and an invoice unit of quantity; not all Invoice Lines marked for {0} have an invoice quantity and an invoice unit of quantity specified.", term);
		}

		public static string InvoiceLineMarkedForBondedWarehousingRequiresACountableQuantityAndUnit(string term)
		{
			return Res.GetString("{1098A847-392B-4B1E-A602-2ADF077D06D3}", "An Invoice Line marked for {0} must have a Countable Quantity and a Countable Unit of Quantity; not all Invoice Lines marked for {0} have an Countable Quantity and a Countable Unit of Quantity specified.", term);
		}

		public static string InvoiceLineMarkedForAllocatedInventoryRequiresACountableQuantity(string term)
		{
			return Res.GetString("353E6EDB-1A55-4B9E-8147-9DBED9684D2F", "An Invoice Line marked for {0} must have a Countable Quantity to Draw; not all Invoice Lines marked for {0} have an Countable Quantity to Draw specified.", term);
		}

		public static string BondedWarehousingInvoiceCannotHaveDifferentImporterMessage(string term)
		{
			return Res.GetString("9C886029-58F1-40B3-94F6-E9DE1FC7B96A", "An Invoice which has an Invoice Line marked for {0} cannot have a different Importer to Declaration's Importer.", term);
		}

		public static string BondedWarehousingInvoiceCannotHaveDifferentSupplierMessage(string term)
		{
			return Res.GetString("9C886029-58F1-40B3-94F6-E9DE1FC7B999", "An Invoice which has an Invoice Line marked for {0} cannot have a different Supplier to Declaration's Supplier.", term);
		}

		public static string InvoiceLineMarkedForBondedWarehousingRequiresEntryDetails(string term)
		{
			return Res.GetString("B0799D43-A73F-469D-872F-542C57728C7C", "An Invoice Line marked for {0} must have an Entry Number and Entry Line Number; not all Invoice Lines marked for {0} have a related Entry Number and Entry Line Number specified.", term);
		}

		public BondedWarehousingHelper BondedWarehousingHelper
		{
			get { return bondedWarehousingHelper ?? (bondedWarehousingHelper = GetNewBondedWarehousingHelper()); }
		}
		BondedWarehousingHelper bondedWarehousingHelper;

		protected virtual BondedWarehousingHelper GetNewBondedWarehousingHelper()
		{
			return new BondedWarehousingHelper(this);
		}

		public bool IsWHSUniversalXMLActive
		{
			get
			{
				if (!isWHSUniversalXMLActive.HasValue)
				{
					isWHSUniversalXMLActive = GetIsWHSUniversalXMLActive();
				}
				return isWHSUniversalXMLActive.Value;
			}
		}
		bool? isWHSUniversalXMLActive;

		protected void ResetIsWHSUniversalXMLActive()
		{
			isWHSUniversalXMLActive = null;
		}

		protected virtual bool GetIsWHSUniversalXMLActive()
		{
#if DEBUG
			if (IsWHSUniversalXMLActiveValueForTestingSetup)
			{
				return getIsWHSUniversalXMLActiveValueForTesting(this);
			}
#endif
			return true;
		}

#if DEBUG
		[SuppressMessage("Microsoft.Usage", "CA2211: Non-constant fields should not be visible")]
		[ThreadStatic]
		protected static Func<BaseJobDeclaration, bool> getIsWHSUniversalXMLActiveValueForTesting;

		public static bool IsWHSUniversalXMLActiveValueForTestingSetup => getIsWHSUniversalXMLActiveValueForTesting != null;

		public static void SetupWHSUniversalXMLForTesting(bool enable)
		{
			if (enable)
			{
				if (getIsWHSUniversalXMLActiveValueForTesting == null)
				{
					getIsWHSUniversalXMLActiveValueForTesting = IsWHSUniversalXMLActiveValueForTesting;
				}
			}
			else
			{
				getIsWHSUniversalXMLActiveValueForTesting = null;
			}
		}

		static bool IsWHSUniversalXMLActiveValueForTesting(BaseJobDeclaration declaration)
		{
			return DataRegistry.Business.CustomsDataRegistry.IsWHSUniversalXMLActive(declaration.JE_SystemCreateTimeUtc);
		}

		public static IDisposable SetupWHSUniversalXMLForTesting()
		{
			SetupWHSUniversalXMLForTesting(true);
			return new DisposableAction(() => SetupWHSUniversalXMLForTesting(false));
		}

#endif

		public bool SupportsSingleEntryBondedWarehousing
		{
			get { return !SupportMultipleWarehouseEntry && SupportsBondedWarehousing; }
		}

		public InventoryManagementSetting ClientInventoryManagementSetting => Factory.GetValue(ref clientInventoryManagementSettingCached, () => InventoryManagementSetting.New(WarehouseClient));
		CachedProperty<InventoryManagementSetting> clientInventoryManagementSettingCached;

		public bool ClientIsBondedWarehousing => ClientInventoryManagementSetting.SupportBondedWarehouse;

		public bool WarehouseAddressIsBondedWarehousing
		{
			get
			{
				if (warehouseAddressIsBondedWarehousingCached == null)
				{
					warehouseAddressIsBondedWarehousingCached = new CachedProperty<bool>(Factory, () => WarehouseAddress?.Header?.CompanyData?.OB_IMUsedBondedWhs ?? false);
				}
				return warehouseAddressIsBondedWarehousingCached.Value;
			}
		}
		CachedProperty<bool> warehouseAddressIsBondedWarehousingCached;

		public bool HasABondedWarehousingEntryOnMultipleWarehouseEntrySystem
		{
			get
			{
				if (hasABondedWarehousingEntryOnMultipleWarehouseEntrySystemgCached == null)
				{
					hasABondedWarehousingEntryOnMultipleWarehouseEntrySystemgCached = new CachedProperty<bool>(Factory, () => SupportMultipleWarehouseEntry && ActiveEntryHeaders.OfType<CusEntryHeader>().Any(x => x.SupportsBondedWarehousing));
				}
				return hasABondedWarehousingEntryOnMultipleWarehouseEntrySystemgCached.Value;
			}
		}
		CachedProperty<bool> hasABondedWarehousingEntryOnMultipleWarehouseEntrySystemgCached;

		public bool SupportsBondedWarehousingForSingleOrMultipleEntry
		{
			get
			{
				if (supportsBondedWarehousingForSingleOrMultipleEntryCached == null)
				{
					supportsBondedWarehousingForSingleOrMultipleEntryCached = new CachedProperty<bool>(Factory, () => SupportMultipleWarehouseEntry ? HasABondedWarehousingEntryOnMultipleWarehouseEntrySystem : SupportsBondedWarehousing);
				}
				return supportsBondedWarehousingForSingleOrMultipleEntryCached.Value;
			}
		}
		CachedProperty<bool> supportsBondedWarehousingForSingleOrMultipleEntryCached;

		public bool SupportsBondedWarehousing
		{
			get
			{
#if DEBUG
				if (overrideSupportsBondedWarehousing)
				{
					return overrideSupportsBondedWarehousingValue;
				}
#endif
				return !SupportMultipleWarehouseEntry && SupportsBondedWarehousingCore &&
					(HasWHSTransaction || ClientIsBondedWarehousing || (ShouldCheckWarehouseAddress && WarehouseAddressIsBondedWarehousing));
			}
		}

		public bool IsBondedWarehousingDisabledForAllEntries
		{
			get { return SupportMultipleWarehouseEntry ? IsBondedWarehousingDisabledForMultipleWarehouseEntry : IsBondedWarehousingDisabled; }
		}

		bool IsBondedWarehousingDisabledForMultipleWarehouseEntry
		{
			get
			{
				if (isBondedWarehousingDisabledForMultipleWarehouseEntryCached == null)
				{
					isBondedWarehousingDisabledForMultipleWarehouseEntryCached = new CachedProperty<bool>(Factory, () =>
					{
						return ActiveEntryHeaders.OfType<CusEntryHeader>().Where(x => x.SupportsBondedWarehousing).All(x => x.IsBondedWarehousingDisabled);
					});
				}
				return isBondedWarehousingDisabledForMultipleWarehouseEntryCached.Value;
			}
		}
		CachedProperty<bool> isBondedWarehousingDisabledForMultipleWarehouseEntryCached;

		public bool IsAutoUpdateBondedWarehouseEnabled => IsAutoUpdateBondedWarehouseEnabledCore;

		protected virtual bool IsAutoUpdateBondedWarehouseEnabledCore => false;

		public bool IsWarehouseOrderFunctionActivated => IsWarehouseOrderFunctionActivatedCore;

		protected virtual bool IsWarehouseOrderFunctionActivatedCore => false;

		public bool IsBondedWarehousingDisabled
		{
			get { return WarehouseTransactionStatusList.IsAutomationDisabled(WarehouseTransactionStatus); }
		}

		bool ShouldCheckWarehouseAddress
		{
			get { return !SupportMultipleWarehouseEntry && (IsImport || IsExWarehouse); }
		}

		public bool IsBondedWarehousingFieldValidationRequired
		{
			get
			{
				if (isBondedWarehousingFieldValidationRequiredCached == null)
				{
					isBondedWarehousingFieldValidationRequiredCached = new CachedProperty<bool>(Factory, () =>
					{
						return IsWHSUniversalXMLActive && SupportsBondedWarehousing &&
								(IsInwardBondedWarehousingEnabled || IsOutwardBondedWarehousingEnabled || HasWHSTransaction);
					});
				}
				return isBondedWarehousingFieldValidationRequiredCached.Value;
			}
		}
		CachedProperty<bool> isBondedWarehousingFieldValidationRequiredCached;

		public bool IsInwardBondedWarehousingEnabledForSingleOrMultipleEntry
		{
			get
			{
				if (isInwardBondedWarehousingEnabledForSingleOrMultipleEntryCached == null)
				{
					isInwardBondedWarehousingEnabledForSingleOrMultipleEntryCached = new CachedProperty<bool>(Factory, () =>
					{
						return SupportMultipleWarehouseEntry ? ActiveEntryHeaders.OfType<CusEntryHeader>().Any(x => x.IsInwardBondedWarehousingEnabled) : IsInwardBondedWarehousingEnabled;
					});
				}
				return isInwardBondedWarehousingEnabledForSingleOrMultipleEntryCached.Value;
			}
		}
		CachedProperty<bool> isInwardBondedWarehousingEnabledForSingleOrMultipleEntryCached;

		public bool IsInwardBondedWarehousingEnabled
		{
			get
			{
				if (isInwardBondedWarehousingEnabledCached == null)
				{
					isInwardBondedWarehousingEnabledCached = new CachedProperty<bool>(Factory, () =>
					{
						return SupportsBondedWarehousing && IsInwardBondedWarehousingEnabledCore && !IsBondedWarehousePermitEnabled;
					});
				}
				return isInwardBondedWarehousingEnabledCached.Value;
			}
		}
		CachedProperty<bool> isInwardBondedWarehousingEnabledCached;

		protected virtual bool IsInwardBondedWarehousingEnabledCore
		{
			get { return IsImport && HasLineGoingIntoABondedWarehouse; }
		}

		public bool IsOutwardBondedWarehousingEnabledForSingleOrMultipleEntry
		{
			get
			{
				if (isOutwardBondedWarehousingEnabledForSingleOrMultipleEntryCached == null)
				{
					isOutwardBondedWarehousingEnabledForSingleOrMultipleEntryCached = new CachedProperty<bool>(Factory, () =>
					{
						return SupportMultipleWarehouseEntry ? ActiveEntryHeaders.OfType<CusEntryHeader>().Any(x => x.IsOutwardBondedWarehousingEnabled) : IsOutwardBondedWarehousingEnabled;
					});
				}
				return isOutwardBondedWarehousingEnabledForSingleOrMultipleEntryCached.Value;
			}
		}
		CachedProperty<bool> isOutwardBondedWarehousingEnabledForSingleOrMultipleEntryCached;

		public bool IsOutwardBondedWarehousingEnabled
		{
			get
			{
				if (isOutwardBondedWarehousingEnabledCached == null)
				{
					isOutwardBondedWarehousingEnabledCached = new CachedProperty<bool>(Factory, () =>
					{
						return SupportsBondedWarehousing && IsOutwardBondedWarehousingEnabledCore && !IsBondedWarehousePermitEnabled;
					});
				}
				return isOutwardBondedWarehousingEnabledCached.Value;
			}
		}
		CachedProperty<bool> isOutwardBondedWarehousingEnabledCached;

		protected virtual bool IsOutwardBondedWarehousingEnabledCore
		{
			get { return IsExWarehouse && (IsWHSUniversalXMLActive || HasALineWithExBondAutomation); }
		}

		public virtual event EventHandler OnBondedWarehouseRelatedFieldChanged
		{
			add
			{
				JE_OH_ImporterInfo.ValueChanged -= value;
				JE_OH_ImporterInfo.ValueChanged += value;
				JE_MessageTypeInfo.ValueChanged -= value;
				JE_MessageTypeInfo.ValueChanged += value;
			}
			remove
			{
				JE_OH_ImporterInfo.ValueChanged -= value;
				JE_MessageTypeInfo.ValueChanged -= value;
			}
		}

#if DEBUG
		bool overrideSupportsBondedWarehousing;
		bool overrideSupportsBondedWarehousingValue;
		public void SetSupportsBondedWarehousingForTesting(bool supportsBondedWarehousing)
		{
			if (!Globals.IsTest)
			{
				throw new NotSupportedException("For testing only");
			}

			overrideSupportsBondedWarehousing = true;
			overrideSupportsBondedWarehousingValue = supportsBondedWarehousing;
			Factory.InvalidateCachedProperties();
		}
#endif

		protected virtual internal bool SupportsBondedWarehousingCore => DataRegistry.Business.CustomsDataRegistry.Instance.EnableWarehouseInventory.GetValueWithoutFallback(CompanyPK.ToGuid(), Guid.Empty, Guid.Empty);

		public bool SupportsBondedWarehouse()
		{
			return SupportsBondedWarehousingCore;
		}

		public
#if DEBUG
 virtual
#endif
 ZBool HasLineGoingIntoAnAutomatedBondedWarehouse
		{
			get
			{
				return SupportsBondedWarehousing && HasLineGoingIntoABondedWarehouse;
			}
		}

		public ZBool HasLineGoingIntoABondedWarehouse
		{
			get
			{
				if (hasLineGoingIntoABondedWarehouse == null)
				{
					hasLineGoingIntoABondedWarehouse = new CachedProperty<ZBool>(Factory, GetHasLineGoingIntoABondedWarehouse);
				}
				return hasLineGoingIntoABondedWarehouse.Value;
			}
		}
		CachedProperty<ZBool> hasLineGoingIntoABondedWarehouse;

		ZBool GetHasLineGoingIntoABondedWarehouse()
		{
			return InvoiceLines.OfType<BaseJobComInvoiceLine>().Any(x => x.IsGoingIntoBondedWarehouse);
		}

		public bool HasLineGoingIntoBondAndLineNotGoingIntoBond
		{
			get
			{
				if (hasLineGoingIntoBondAndLineNotGoingIntoBondCachedProperty == null)
				{
					hasLineGoingIntoBondAndLineNotGoingIntoBondCachedProperty = new CachedProperty<bool>(Factory, GetHasLineGoingIntoBondAndLineNotGoingIntoBond);
				}
				return hasLineGoingIntoBondAndLineNotGoingIntoBondCachedProperty.Value;
			}
		}
		CachedProperty<bool> hasLineGoingIntoBondAndLineNotGoingIntoBondCachedProperty;

		bool GetHasLineGoingIntoBondAndLineNotGoingIntoBond()
		{
			bool hasNonBondLine = false;
			bool hasBondLine = false;

			foreach (BaseJobComInvoiceLine line in InvoiceLines)
			{
				if (line.IsGoingIntoBondedWarehouse)
				{
					hasBondLine = true;
				}
				else
				{
					hasNonBondLine = true;
				}
				if (hasBondLine && hasNonBondLine)
				{
					break;
				}
			}
			return hasBondLine && hasNonBondLine;
		}

		public bool HasLinesForInwardBondedWarehousing
		{
			get
			{
				if (hasLinesForInwardBondedWarehousingCached == null)
				{
					hasLinesForInwardBondedWarehousingCached = new CachedProperty<bool>(Factory, GetHasLinesForInwardBondedWarehousing);
				}
				return hasLinesForInwardBondedWarehousingCached.Value;
			}
		}
		CachedProperty<bool> hasLinesForInwardBondedWarehousingCached;

		protected virtual bool GetHasLinesForInwardBondedWarehousing()
		{
			return SupportMultipleWarehouseEntry ? ActiveEntryHeaders.OfType<CusEntryHeader>().Any(x => x.HasLinesForInwardBondedWarehousing) : (bool)HasLineGoingIntoAnAutomatedBondedWarehouse;
		}

		public bool EntriesExistAndAllHaveEntryNumbers => EntriesExistAndAllHaveEntryNumbersCore;
		protected virtual bool EntriesExistAndAllHaveEntryNumbersCore => CustomsEntryHeaders.EntriesExistAndAllHaveEntryNumbers;

		public bool EntriesExistWithExBondAutomationAndAllHaveEntryNumbers
		{
			get
			{
				return IsExBondAutomationEnabled && CustomsEntryHeaders.EntriesExistAndAllHaveEntryNumbers;
			}
		}

		public bool HasALineWithExBondAutomation
		{
			get
			{
				if (hasALineWithExBondAutomationCachedProperty == null)
				{
					hasALineWithExBondAutomationCachedProperty = new CachedProperty<bool>(Factory, () =>
					{
						return HasALineWithExBondAutomationCore();
					});
				}
				return hasALineWithExBondAutomationCachedProperty.Value;
			}
		}
		CachedProperty<bool> hasALineWithExBondAutomationCachedProperty;

		protected virtual bool HasALineWithExBondAutomationCore()
		{
			foreach (BaseJobComInvoiceLine line in InvoiceLines)
			{
				if (line.UseBondedWarehouseAutomation)
				{
					return true;
				}
			}
			return false;
		}

		public bool IsExBondAutomationEnabled
		{
			get
			{
				if (isExBondAutomationEnabledCachedProperty == null)
				{
					isExBondAutomationEnabledCachedProperty = new CachedProperty<bool>(Factory, GetExBondAutomationEnabled);
				}
				return isExBondAutomationEnabledCachedProperty.Value;
			}
		}
		CachedProperty<bool> isExBondAutomationEnabledCachedProperty;

		bool GetExBondAutomationEnabled()
		{
			var result = false;
			if (IsOutwardBondedWarehousingEnabled)
			{
				result = IsWHSUniversalXMLActive ? HasBondedWarehouse() : HasALineWithExBondAutomation;
			}
			return result;
		}

		protected virtual bool HasBondedWarehouse()
		{
			return HasABondedWarehouseFor(WarehouseAddress);
		}

		protected bool HasABondedWarehouseFor(OrgAddress warehouseAddress)
		{
			return warehouseAddress.GetWhsWarehouse() != null;
		}

		public LoggingInformation Logger { get; set; }

		public
#if DEBUG
 virtual
#endif
 void CreateAndUpdateInvoicesForExBondAutomation()
		{
			if (!IsExWarehouse)
			{
				throw new NotSupportedException("ExWarehouse jobs only");
			}

			using (new LogDiagnosticTicks((NoResString)"Create And Update Invoices For ExBondAutomation", Logger))
			{
				IBondedWarehouseLink warehouseLink = new BondedWarehouseLinkCreator().GetNewBondedWarehouseLink(Factory);
				BondedWarehouseTransaction sourceTransaction = GetNewBondedWarehouseTransaction();
				sourceTransaction.SetInvoiceLineMode();
				sourceTransaction.UseEntryKeyFromEntry = false;
				IWhsBondedWarehouseTransaction transactionBuiltUpByWarehouse = warehouseLink.CreateOrUpdateOutwardMovement(sourceTransaction, false);
				((IInvoiceLinkProvider)this).Link.CreateOrUpdateInvoices(transactionBuiltUpByWarehouse);
			}
		}

		public bool SendMessageWithBondedWarehouseAutomation(Func<bool> sendMessage, MessageAction messageAction, Action saveFactory = null, bool additionalBondedWarehouseRequirement = true, bool reportHasChanges = true)
		{
			return SendMessageWithBondedWarehouseAutomation(this, GetInventoryAutomationAction(), sendMessage, messageAction, saveFactory: saveFactory, additionalBondedWarehouseRequirement: additionalBondedWarehouseRequirement, reportHasChanges: reportHasChanges);
		}

		public bool SendMessageWithBondedWarehouseAutomation(IWarehouseIntegrationSupporter supporter, InventoryAutomationAction inventoryAutomationAction, Func<bool> sendMessage, MessageAction messageAction, Action saveFactory = null, bool additionalBondedWarehouseRequirement = true, bool reportHasChanges = true)
		{
			return JobDeclarationBondedWarehouseAutomation.SendMessageWithBondedWarehouseAutomation(supporter, () => MessageInitiator, inventoryAutomationAction, sendMessage, messageAction, saveFactory, additionalBondedWarehouseRequirement, reportHasChanges);
		}

		public bool IsBondedWarehousePermitEnabled
		{
			get { return IsBondedWarehousePermitEnabledCore; }
		}

		protected virtual bool IsBondedWarehousePermitEnabledCore
		{
			get { return false; }
		}

		public ZString SynchronizeWithOrders()
		{
			return GetNewSynchronizeWithOrders().Synchronize();
		}

		protected virtual SynchronizeWithOrders GetNewSynchronizeWithOrders()
		{
			throw new NotSupportedException();
		}

		public bool IsInventorySelectionEnabled
		{
			get { return IsWHSUniversalXMLActive && IsInventorySelectionEnabledCore; }
		}

		protected virtual bool IsInventorySelectionEnabledCore
		{
			get { return IsOutwardBondedWarehousingEnabled; }
		}

		public bool ShouldUpdateOutwardLinesWithInventoryDetails
		{
			get { return ShouldUpdateOutwardLinesWithInventoryDetailsCore; }
		}

		protected virtual bool ShouldUpdateOutwardLinesWithInventoryDetailsCore
		{
			get { return IsExWarehouse; }
		}

		public ZString UpdateOutwardLinesWithInventoryDetails()
		{
			var result = ZString.Empty;
			if (IsInventorySelectionEnabled)
			{
				result = InventorySelectionHeader.UpdateOutwardLinesWithInventoryDetails(InvoiceLines.Cast<BaseJobComInvoiceLine>());
			}
			return result;
		}

		public DeclarationInventorySelectionHeader InventorySelectionHeader
		{
			get { return GetNewInventorySelectionHeader(); }
		}

		protected virtual DeclarationInventorySelectionHeader GetNewInventorySelectionHeader()
		{
			return new DeclarationInventorySelectionHeader(this);
		}

		public
#if DEBUG
 virtual
#endif
 void CreateOrUpdateBondedWarehouseInward()
		{
			using (new LogDiagnosticTicks((NoResString)"Create Or Update Bonded Warehouse Inward", Logger))
			{
				IBondedWarehouseLink warehouseLink = new BondedWarehouseLinkCreator().GetNewBondedWarehouseLink(Factory);
				BondedWarehouseTransaction sourceTransaction = GetNewBondedWarehouseTransaction();
				sourceTransaction.SetInvoiceLineMode();
				if (sourceTransaction.Lines.Count > 0)
				{
					warehouseLink.CreateOrUpdateInwardMovement(sourceTransaction);
				}
			}
		}

		public
#if DEBUG
 virtual
#endif
 void NotifyBondedWarehouseThatExWarehouseEntryHasCleared()
		{
			using (new LogDiagnosticTicks((NoResString)"Notify Bonded Warehouse ThatExWarehouse Entry Has Cleared", Logger))
			{
				IBondedWarehouseLink warehouseLink = new BondedWarehouseLinkCreator().GetNewBondedWarehouseLink(Factory);
				BondedWarehouseTransaction sourceTransaction = GetNewBondedWarehouseTransaction();
				sourceTransaction.SetInvoiceLineMode();
				warehouseLink.NotifyGoodsAreClearedForRelease(PK, sourceTransaction);
			}
		}

#if DEBUG
		public
			virtual
#endif
		void UpdateJobDeclarationReferenceOnWarehouseSide()
		{
			using (new LogDiagnosticTicks((NoResString)"Update Job Declaration Reference On Warehouse Side", Logger))
			{
				IBondedWarehouseLink warehouseLink = new BondedWarehouseLinkCreator().GetNewBondedWarehouseLink(Factory);
				warehouseLink.UpdateDeclarationReference(PK, JE_DeclarationReference);
			}
		}

		public
#if DEBUG
 virtual
#endif
 void CancelBondedWarehouseIntegration()
		{
			using (new LogDiagnosticTicks((NoResString)"Cancel Bonded Warehouse Integration", Logger))
			{
				IBondedWarehouseLink warehouseLink = new BondedWarehouseLinkCreator().GetNewBondedWarehouseLink(Factory);
				warehouseLink.CancelOutwardMovement(PK);
				foreach (BaseJobComInvoiceLine line in InvoiceLines.ToArray())
				{
					if (line.UseBondedWarehouseAutomation)
					{
						line.UseBondedWarehouseAutomation = false;
					}
				}
			}
		}

		protected virtual BondedWarehouseTransaction GetNewBondedWarehouseTransaction()
		{
			return new BondedWarehouseTransaction(this);
		}

#if DEBUG

		public BondedWarehouseTransaction GetNewBondedWarehouseTransactionForTesting()
		{
			if (!Globals.IsTest)
			{
				throw new NotSupportedException("Testing only");
			}

			return GetNewBondedWarehouseTransaction();
		}

#endif

		#region IInvoiceLink

		IInvoiceLink IInvoiceLinkProvider.Link
		{
			get
			{
				if (fWarehouseInvoiceLink == null)
				{
					fWarehouseInvoiceLink = GetNewWarehouseInvoiceLink();
				}

				return fWarehouseInvoiceLink;
			}
		}

		protected WarehouseInvoiceLink fWarehouseInvoiceLink;

		protected virtual WarehouseInvoiceLink GetNewWarehouseInvoiceLink()
		{
			return new WarehouseInvoiceLink(this);
		}

		#endregion

		public bool IsByProductFunctionalityEnabled => DataRegistry.Business.CustomsDataRegistry.Instance.EnableByProductFunctionality.Value;

		public bool SupportOutwardProcessing => false;

		public bool SupportInwardProcessing => SupportInwardProcessingCore && IsByProductFunctionalityEnabled && SupportsBondedWarehouse();

		protected virtual bool SupportInwardProcessingCore => false;

		#endregion

		#region ReadOnly

		public ZString FinalDestinationCountryCode => JE_RL_NKFinalDestination.Left(2);
		public ZString BranchCompanyCountryCode => Branch?.Company?.GC_RN_NKCountryCode ?? ZString.Empty;

		public List<string> SynchroniserReadOnlyMembers { get { return synchroniserReadOnlyMembers ?? (synchroniserReadOnlyMembers = new List<string>()); } }
		List<string> synchroniserReadOnlyMembers;

		protected bool GetShouldPropertiesBeReadOnly(PropertyDescriptor property)
		{
			return MetaData.GetReadOnlyExcludingMethodProvider(this, property) || SynchroniserReadOnlyMembers.Contains(property.Name);
		}
		#endregion

		#region New Properties

		public bool HasInvoiceLineWithoutEntryInstruction => Factory.GetValue(ref hasInvoiceLineWithoutEntryInstructionCached, () => !CustomsEntryInstructionProvider.IsNoEntryInstruction && InvoiceLines.Cast<BaseJobComInvoiceLine>().Any(x => x.JI_CEI.IsEmpty));
		CachedProperty<bool> hasInvoiceLineWithoutEntryInstructionCached;

		public bool HasWHSTransaction
		{
			get { return Enterprise.Customs.Business.WarehouseExtensions.JobDeclarationWarehouseExtensions.HasWHSTransaction(this); }
		}

		public ZBool IsReciprocalRates
		{
			get { return IsReciprocalRatesCore; }
		}

		protected virtual ZBool IsReciprocalRatesCore  // This should be consistant with csfn_IsReciprocalInline.sql
		{
			get { return GlbCompany.CurrentCompany.GC_IsReciprocal; }
		}

		public ZString LocalCurrencyCode
		{
			get
			{
				if (LocalCurrencyCodeCore.IsEmpty)
				{
					var refCountry = Branch?.Company?.Country ?? GlbCompany.CurrentCompany.Country;
					return refCountry.RN_RX_NKLocalCurrency;
				}
				return LocalCurrencyCodeCore;
			}
		}

		protected virtual ZString LocalCurrencyCodeCore
		{
			get { return ZString.Empty; }
		}

		public RefCurrency LocalCurrency
		{
			get { return RefCurrency.LoadFromCurrencyCode(Factory, LocalCurrencyCode) ?? GlbCompany.CurrentCompany.Country.LocalCurrency; }
		}

		public static ZString GetLocalCurrencyCodeFor(BaseJobDeclaration declaration)
		{
			return declaration != null ? declaration.LocalCurrencyCode : GlbCompany.CurrentCompany.Country.RN_RX_NKLocalCurrency;
		}

		public virtual ZString CustomsClearanceStatus { get; set; } = ZString.Empty;

		public virtual ZString JE_MasterBillForGenericWrapper
		{
			get { return JE_MasterBill; }
		}

		public virtual ZString JE_HouseBillForGenericWrapper
		{
			get { return JE_HouseBill; }
		}

		public virtual ZString JE_SupplierMiscFields
		{
			get { return ZString.Empty; }
		}

		public virtual ZString JE_ImporterMiscFields
		{
			get { return ZString.Empty; }
		}

		public bool NeedsAdditionalLinkBetweenInvoiceLineAndEntryLine
		{
			get { return AdditionalInvoiceLineEntryLineLinkSupporter.DoesSupport(CountryCode); }
		}

		public bool ShouldKeepDeletedLinesOnAmendment
		{
			get { return ShouldKeepDeletedLinesOnAmendmentCore; }
		}

		protected virtual bool ShouldKeepDeletedLinesOnAmendmentCore
		{
			get { return !IsCustomsLineAmendmentATotalReplacement; }
		}

		public virtual bool ShouldKeepDeletedLinesOnAmendmentCleared => true;

		public virtual bool ShouldCopyProcedureFromPreviousInvoiceLine => false;

		/// <summary>
		/// If it is a total replacement, previous lodgement state need not be stored, eg NZ
		/// But AU Customs is an accumulative amendment where for example, line number and nature reported at the time of lodgement are important
		/// This flag is used to make a decision for entry line number calculation and keeping entry lines about to be deleted
		/// </summary>
		internal protected virtual bool IsCustomsLineAmendmentATotalReplacement
		{
			get
			{
				//base test and customs template test bypass the exception
				if (Globals.IsTest && (CountryCode == "ER" || CountryCode == "AI"))
				{
					return false;
				}
				if (!IsDeclarationIntegrated)
				{
					throw new NotSupportedException("Each country should implement this. If you are testing base behaviour, please use mock instead.");
				}
				return false;
			}
		}

		/// <summary>
		/// Are any kinds of changes allowed at the header level?
		/// AU does not allow Nature/Transport Mode change after it is lodged. This flag is used to validate on deactivated entries after merged
		/// NZ does not allow change of invoice date.
		/// </summary>
		internal protected virtual bool IsCustomsHeaderAmendmentATotalReplacement
		{
			get
			{
				//base test and customs template test bypass the exception
				if (Globals.IsTest && (CountryCode == "ER" || CountryCode == "AI"))
				{
					return false;
				}
				if (!IsDeclarationIntegrated)
				{
					throw new NotSupportedException("Each country should implement this. If you are testing base behaviour, please use mock instead.");
				}
				return false;
			}
		}

		public bool SupportEntrySnpashots => SupportEntrySnpashotsCore;

		protected virtual bool SupportEntrySnpashotsCore => false;

		public ZString CustomsVATTypeCaption => CustomsVATTypeCaptionCore;

		protected virtual ZString CustomsVATTypeCaptionCore => Enterprise.ZArchitecture.Environment.Country.GetConsumptionTaxRegistrationOrgCusCode(CountryCode) ?? Res.GetString("187460E8-E063-441B-BABC-5F045C30071B", "VAT");

		#region Estimate Date For View

		public ZDateTime JE_ETAOfDischarge
		{
			get
			{
				if (etaOfDischargeForView == null)
				{
					var transport = GetTransportForDischarge();
					etaOfDischargeForView = transport?.JW_ETAForBinding ?? ZDateTime.Empty;
				}

				return etaOfDischargeForView.Value;
			}
		}
		ZDateTime? etaOfDischargeForView;

		public ZPropertyInfo JE_ETAOfDischargeInfo
		{
			get { return GetZPropertyInfo(Schema.JE_ETAOfDischarge); }
		}

		Transport GetTransportForDischarge()
		{
			Transport result = null;

			var routingCollection = ((IRoutingSupport)this).TransportsIncludingRelated;

			if (!string.IsNullOrWhiteSpace(JE_RL_NKPortOfArrival))
			{
				var compare = new TransportParentTypeCompare();

				result = routingCollection
					.Where(c => c.JW_RL_NKDiscPort == JE_RL_NKPortOfArrival)
					.OrderBy(c => c.JW_ParentType, compare)
					.OrderBy(c => c.JW_LegOrder)
					.FirstOrDefault();

				if (result == null)
				{
					var countryCode = JE_RL_NKPortOfArrival.SubstringSafe(0, 2);

					result = routingCollection
						.Where(c => c.JW_RL_NKDiscPort.SubstringSafe(0, 2) == countryCode)
						.OrderByDescending(c => c.JW_ParentType, compare)
						.OrderBy(c => c.JW_LegOrder)
						.LastOrDefault();
				}
			}

			return result;
		}

		public ZDateTime JE_ETDOfLoading
		{
			get
			{
				if (etdOfLoadingForView == null)
				{
					var transport = GetTransportForLoading();
					etdOfLoadingForView = transport?.JW_ETDForBinding ?? ZDateTime.Empty;
				}

				return etdOfLoadingForView.Value;
			}
		}
		ZDateTime? etdOfLoadingForView;

		public ZPropertyInfo JE_ETDOfLoadingInfo
		{
			get { return GetZPropertyInfo(Schema.JE_ETDOfLoading); }
		}

		Transport GetTransportForLoading()
		{
			Transport result = null;

			var routingCollection = ((IRoutingSupport)this).TransportsIncludingRelated;

			if (!string.IsNullOrWhiteSpace(JE_RL_NKPortOfLoading))
			{
				var compare = new TransportParentTypeCompare();

				result = routingCollection
					.Where(c => c.JW_RL_NKLoadPort == JE_RL_NKPortOfLoading)
					.OrderBy(c => c.JW_ParentType, compare)
					.OrderBy(c => c.JW_LegOrder)
					.FirstOrDefault();

				if (result == null)
				{
					var countryCode = JE_RL_NKPortOfLoading.SubstringSafe(0, 2);
					result = routingCollection
							.Where(c => c.JW_RL_NKLoadPort.SubstringSafe(0, 2) == countryCode)
							.OrderByDescending(c => c.JW_ParentType, compare)
							.OrderBy(c => c.JW_LegOrder)
							.LastOrDefault();
				}
			}

			return result;
		}

		sealed class TransportParentTypeCompare : IComparer<ZString>
		{
			public int Compare(ZString x, ZString y)
			{
				return GeProportion(x) - GeProportion(y);
			}

			int GeProportion(ZString value)
			{
				switch (value)
				{
					case Constants.TransportParentTypes.Declaration:
						return 4;
					case Constants.TransportParentTypes.Consol:
						return 2;
					case Constants.TransportParentTypes.Shipment:
						return 1;
					default:
						return -1;
				}
			}
		}

#if DEBUG

		internal void ResetEstimateDateForView()
		{
			etaOfDischargeForView = null;
			etdOfLoadingForView = null;
		}

#endif

		#endregion

		#region DocumentNote

		[ActionFieldFollow(false)]
		public DocumentNote DocNote
		{
			get
			{
				if (fDocNote == null)
				{
					fDocNote = DocumentNote.LoadNote(this);
					RegisterEditableChildObject(fDocNote);
				}
				return fDocNote;
			}
		}
		DocumentNote fDocNote;

		#endregion

		#region Concatenated Bills (Master and House)

		public ZString HouseBillsCommaSeparated
		{
			get { return Bills.HouseBillsCommaSeparated; }
		}

		public ZPropertyInfo HouseBillsCommaSeparatedInfo
		{
			get { return GetZPropertyInfo(Schema.HouseBillsCommaSeparated); }
		}

		public ZString MasterBillsCommaSeparated
		{
			get { return Bills.MasterBillsCommaSeparated; }
		}

		public ZPropertyInfo MasterBillsCommaSeparatedInfo
		{
			get { return GetZPropertyInfo(Schema.MasterBillsCommaSeparated); }
		}

		#endregion

		public bool HaveAmendmentsBeenMadeAndNotYetClearedByCustoms
		{
			get { return CustomsEntryHeaders.Any(header => header.HaveAmendmentsBeenMadeAndNotYetClearedByCustoms); }
		}

		public bool HasSplitEntries
		{
			get { return HasSplitEntriesCore; }
		}

		protected virtual bool HasSplitEntriesCore
		{
			get { return CustomsEntryHeaders.Count > 1; }
		}

		public bool HasMultipleEntriesForDeterminingContainerCount
		{
			get { return HasMultipleEntriesForDeterminingContainerCountCore; }
		}

		protected virtual bool HasMultipleEntriesForDeterminingContainerCountCore
		{
			get { return CustomsEntryHeaders.Count > 1; }
		}

		public bool UseDeclarationContainersForSingleContainerWhenMultipleEntriesExist => UseDeclarationContainersForSingleContainerWhenMultipleEntriesExistCore;

		protected virtual bool UseDeclarationContainersForSingleContainerWhenMultipleEntriesExistCore => true;

		public bool UseDeclarationContainersIfNoneFoundOnEntry => UseDeclarationContainersIfNoneFoundOnEntryCore;

		protected virtual bool UseDeclarationContainersIfNoneFoundOnEntryCore => true;

		public OrgHeader LocalParty
		{
			get { return LocalPartyCore; }
		}

		protected virtual OrgHeader LocalPartyCore
		{
			get { return IsImport ? Importer : Supplier; }
		}

		public Money TotalFOB
		{
			get
			{
				Money result = new Money(0m, LocalCurrency);
				foreach (BaseJobComInvoiceHeader header in Invoices)
				{
					Money fOB = new Money(header.JZ_Calc_FOBAmount, header.Invoice_Currency);
					result = header.CurrencyConverter.Add(result, fOB);
				}
				return result;
			}
		}

		public Money TotalFOBInLocalCurrency
		{
			get { return ConvertToLocalAmount(TotalFOB); }
		}

		public ZWeight TotalCustomsWeight
		{
			get
			{
				if (totalCustomsWeight == null)
				{
					totalCustomsWeight = new CachedProperty<ZWeight>(Factory, CalculateTotalCustomsWeight);
				}
				return totalCustomsWeight.Value;
			}
		}

		CachedProperty<ZWeight> totalCustomsWeight;

		ZWeight CalculateTotalCustomsWeight()
		{
			ZWeight result = ZWeight.Empty;
			foreach (BaseJobComInvoiceLine invoiceLine in InvoiceLines)
			{
				ZWeight potentialWeight = invoiceLine.CustomsWeight;
				if (potentialWeight.IsValid)
				{
					result += potentialWeight;
				}
			}
			return result;
		}

		public ZDecimal TotalCustomsValueInLocalCurrency
		{
			get { return TotalCustomsValueInLocalCurrencyCore; }
		}

		protected virtual ZDecimal TotalCustomsValueInLocalCurrencyCore
		{
			get
			{
				ZDecimal result = 0m;
				foreach (CusEntryHeader entryHeader in ActiveEntryHeaders)
				{
					result += entryHeader.CustomsValue;
				}
				return result;
			}
		}

		public ZDecimal TotalInvoiceAmountInLocalCurrency
		{
			get
			{
				ZDecimal result = 0m;

				foreach (BaseJobComInvoiceHeader invoiceHeader in Invoices)
				{
					result += invoiceHeader.JZ_InvoiceAmountInLocalCurrency;
				}
				return result;
			}
		}

		public Money TotalInvoiceAmount
		{
			get
			{
				Money result = Money.Empty;
				foreach (BaseJobComInvoiceHeader invoiceHeader in Invoices)
				{
					result = invoiceHeader.CurrencyConverter.Add(result, invoiceHeader.InvoiceAmount);
				}
				return result;
			}
		}

		public ZDate DateForDutyRate
		{
			get { return GetDateForDutyRateCore(); }
		}

		protected virtual ZDate GetDateForDutyRateCore()
		{
			return ZDate.Today;
		}

		public bool IsMultiSupplier
		{
			get
			{
				for (int i = 0; i < Invoices.Count - 1; i++)
				{
					BaseJobComInvoiceHeader header = Invoices[i];
					BaseJobComInvoiceHeader nextHeader = Invoices[i + 1];

					if ((header.Supplier != null) && (nextHeader.Supplier != null))
					{
						if (header.Supplier.PK.Equals(nextHeader.Supplier.PK))
						{
							return false;
						}
					}
				}
				return true;
			}
		}

		public ZDecimal ConsignmentValue
		{
			get
			{
				if (consignmentValueCached == null)
				{
					consignmentValueCached = new CachedProperty<ZDecimal>(Factory, () => Invoices.OfType<BaseJobComInvoiceHeader>().Sum(x => x.JZ_InvoiceAmount));
				}
				return consignmentValueCached.Value;
			}
		}
		CachedProperty<ZDecimal> consignmentValueCached;

		public bool AutoCreateChargesBasedOnIncoTerm
		{
			get { return fAutoCreateChargesBasedOnIncoTerm; }
			set { fAutoCreateChargesBasedOnIncoTerm = value; }
		}
		bool fAutoCreateChargesBasedOnIncoTerm;

		public bool HasJobInvoicing
		{
			get { return JobInvoicing != null; }
		}

		public ILandedCostChargeHolder JobInvoicing
		{
			get
			{
				if (jobInvoicingCached == null)
				{
					jobInvoicingCached = new CachedProperty<ILandedCostChargeHolder>(Factory, delegate
					{
						return (ILandedCostChargeHolder)Factory.LoadTop1<IJobHeader>(JobInvoicingFilter);
					}
						);
				}
				return jobInvoicingCached.Value;
			}
		}
		CachedProperty<ILandedCostChargeHolder> jobInvoicingCached;

		protected ZQuery JobInvoicingFilter
		{
			get
			{
				ZQuery filter = new ZQuery(JobHeaderSchema.JH_GC, SQLComparisonOperator.Equal, GlbCompany.CurrentCompany.PK);
				if (Shipment == null)
				{
					filter.AddToFilter(JoinCondition.And, JobHeaderSchema.JH_ParentID, SQLComparisonOperator.Equal, PK);
					filter.FetchOnlyFromLocalCache = !IsInDatabase;
				}
				else
				{
					filter.AddToFilter(JoinCondition.And, JobHeaderSchema.JH_ParentID, SQLComparisonOperator.Equal, Shipment.PK);
					filter.FetchOnlyFromLocalCache = !Shipment.IsInDatabase;
				}
				return filter;
			}
		}

		public bool DoesJobInvoicingHaveFreightAmount
		{
			get
			{
				if (doesJobInvoicingHaveFreightAmountCached == null)
				{
					doesJobInvoicingHaveFreightAmountCached = new CachedProperty<bool>(Factory, GetDoesJobInvoicingHaveFreightAmount);
				}
				return doesJobInvoicingHaveFreightAmountCached.Value;
			}
		}
		CachedProperty<bool> doesJobInvoicingHaveFreightAmountCached;

		bool GetDoesJobInvoicingHaveFreightAmount()
		{
			bool result = false;
			ILandedCostChargeHolder jobInvoicing = this.JobInvoicing;

			if (jobInvoicing != null)
			{
				foreach (IDefaultLandedCostInput charge in jobInvoicing.ChargesToImportForLandedCosting)
				{
					if (charge.FKToChargeCode == Env.Registry.FreightChargeCode)
					{
						result = true;
						break;
					}
				}
			}
			return result;
		}

		public virtual ZString ImporterCity
		{
			get
			{
				ZString result = ZString.Empty;
				OrgHeader importer = this.Importer;
				if (importer != null)
				{
					if (importer.Addresses.Count > 0 && !importer.MainAddress.OA_City.IsEmpty)
					{
						result = importer.MainAddress.OA_City;
					}
					else if (importer.ClosestPort != null)
					{
						result = importer.ClosestPort.RL_PortName;
					}
				}
				return result;
			}
		}

		public ZString CartageAdvisedCaption
		{
			get { return IsImport ? Res.GetString("a90c44b7-79b0-490e-8de6-b9634203a560", "Port Trn. Advised") : Res.GetString("aedc01cc-8604-4053-8c06-ffbf310a6c31", "Docs to Carrier"); }
		}

		public ZPropertyInfo CartageAdvisedCaptionInfo
		{
			get { return GetZPropertyInfo(nameof(CartageAdvisedCaption)); }
		}

		public virtual ZBool BondedWarehouseEditable
		{
			get { return IsImport || IsExport || IsExWarehouse; }
		}

		public ZWeight GrossWeight
		{
			get { return new ZWeight(JE_TotalWeight, JE_TotalWeightUnit); }
			set
			{
				JE_TotalWeight = value.Amount;
				JE_TotalWeightUnit = value.Unit;
			}
		}

		public ZVolume Volume
		{
			get { return new ZVolume(JE_TotalVolume, JE_TotalVolumeUnit); }
			set
			{
				JE_TotalVolume = value.Amount;
				JE_TotalVolumeUnit = value.Unit;
			}
		}

		public virtual bool CannotUpdatePart
		{
			get { return false; }
		}

		ZDateTime todaysDate;
		public ZDateTime CachedTodaysDate
		{
			get
			{
				if (!todaysDate.IsValid)
				{
					todaysDate = ZDateTime.Today;
				}
				return todaysDate;
			}
		}

		public virtual ZDateTime DateOfValuation
		{
			get
			{
				var result = CachedTodaysDate;
				if (JE_ValuationDate.IsValid && !JE_ValuationDate.IsEmpty)
				{
					result = JE_ValuationDate;
				}
				else if (JE_ExportDate.IsValid && (!IsExport || JE_ExportDate <= CachedTodaysDate))
				{
					result = JE_ExportDate;
				}

				return result;
			}
		}

		public ZPropertyInfo DateOfValuationInfo
		{
			get { return GetZPropertyInfo(nameof(DateOfValuation)); }
		}

		public override ZDate JE_ValuationDate
		{
			get => base.JE_ValuationDate;
			set
			{
				base.JE_ValuationDate = value;
				Invoices.MarkAsNeedingValidation();
				InvoiceLines.MarkAsNeedingValidation();
			}
		}

		public string FreightModeForPortDeliveryTimes
		{
			get
			{
				string result = "";

				if (IsAir)
				{
					result = Core.Constants.TransportModes.Air;
				}
				else if (IsSea)
				{
					if (JE_ContainerMode == Core.Constants.ContainerModes.FCL || JE_ContainerMode == Core.Constants.ContainerModes.LCL)
					{
						result = JE_ContainerMode;
					}
					else if (HasCusContainers)
					{
						List<ZString> containerModes = new List<ZString>();
						foreach (BaseCusContainer container in CusContainers)
						{
							if (!container.CO_FCL_LCL_AIR.IsEmpty && !containerModes.Contains(container.CO_FCL_LCL_AIR))
							{
								containerModes.Add(container.CO_FCL_LCL_AIR);
							}
						}

						if (containerModes.Count == 1 && (containerModes[0] == Core.Constants.ContainerModes.FCL || containerModes[0] == Core.Constants.ContainerModes.LCL))
						{
							result = containerModes[0];
						}
					}

					if (string.IsNullOrEmpty(result))
					{
						result = Core.Constants.TransportModes.Sea;
					}
				}
				else if (IsRoad)
				{
					result = Core.Constants.TransportModes.Road;
				}
				else if (IsRail)
				{
					result = Core.Constants.TransportModes.Rail;
				}
				else
				{
					result = JE_TransportMode;
				}

				return result;
			}
		}

		public virtual ZString EntryDetailsInARInvoice
		{
			get { return ZString.Empty; }
		}

		#region DeclarationNumber

		[BusinessObjectTestExclude]
		[ReadOnlyMember(nameof(IsDeclarationNumberReadOnly))]
		[MaxLength(35)]
		[ResourceStringData("Enterprise.Customs.Business.BaseJobDeclaration.DeclarationNumber", Caption = "Entry Number")]
		public virtual ZString DeclarationNumber
		{
			get
			{
				ZStringBuilder result = new ZStringBuilder();
				foreach (CusEntryHeader header in CustomsEntryHeaders)
				{
					if (!header.EntryNumber.IsEmpty)
					{
						result.Append(header.EntryNumber);
					}
				}
				return result.ToStringWithDelimiterBetweenAppends(",");
			}
			set
			{
				(Validation as BaseJobDeclarationValidation)?.ValidateDeclarationNumber();
				DeclarationNumberInfo.RefreshBinding();
			}
		}

		protected virtual bool IsDeclarationNumberReadOnly
		{
			get { return true; }
		}

		public virtual ZPropertyInfo DeclarationNumberInfo
		{
			get { return GetZPropertyInfo(Schema.DeclarationNumber); }
		}

		protected CusEntryNumber CreateNewCusEntryNumber()
		{
			CusEntryNumber result = Factory.New<CusEntryNumber>();
			result.CE_RN_NKCountryCode = CountryCode;
			result.CE_ParentID = PK;
			result.CE_ParentTable = TableName;
			return result;
		}

		public virtual ZDateTime EarliestCustomsEntryIssueDate
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;
				foreach (CusEntryHeader header in CustomsEntryHeaders)
				{
					CusEntryNumber entryNum = header.CusEntryNumber;
					if (entryNum != null)
					{
						if (result.IsEmpty || entryNum.CE_IssueDate < result)
						{
							result = entryNum.CE_IssueDate;
						}
					}
				}
				return result;
			}
		}
		public ZPropertyInfo EarliestCustomsEntryIssueDateInfo
		{
			get { return GetZPropertyInfo(Schema.EarliestCustomsEntryIssueDate); }
		}

		#endregion

		#region AttachExportDeclaration
		public bool AttachExportDeclaration(ZGuid pK)
		{
			bool result = false;
			var exportDeclaration = Factory.Load<BaseJobDeclaration>(pK);
			if (exportDeclaration != null && exportDeclaration.IsExport)
			{
				foreach (BaseJobComInvoiceHeader invoice in exportDeclaration.Invoices)
				{
					if (AddInvoiceFromExportDeclaration(invoice))
					{
						result = true;
					}
				}
			}
			return result;
		}
		bool AddInvoiceFromExportDeclaration(BaseJobComInvoiceHeader invoice)
		{
			bool result = true;
			foreach (BaseJobComInvoiceHeader currentJobInvoice in Invoices)
			{
				if (currentJobInvoice.JZ_InvoiceNumber == invoice.JZ_InvoiceNumber)
				{
					result = false;
					break;
				}
			}
			if (result)
			{
				BaseJobComInvoiceHeader newHeader = (BaseJobComInvoiceHeader)AddInvoiceHeaderFromExportDeclarationCore(invoice);
				foreach (BaseJobComInvoiceLine invoiceLine in invoice.JobComInvoiceLines)
				{
					AddInvoiceLineFromExportDeclarationCore(invoiceLine, newHeader);
				}
			}
			return result;
		}
		protected virtual BusinessObject AddInvoiceHeaderFromExportDeclarationCore(BaseJobComInvoiceHeader invoice)
		{
			return null;
		}
		protected virtual void AddInvoiceLineFromExportDeclarationCore(BaseJobComInvoiceLine invoiceLine, BaseJobComInvoiceHeader newHeader)
		{
		}
		#endregion

		public
#if DEBUG
 virtual
#endif
 ZString MessageSubTypeDescription
		{
			get { return Lookups.MessageSubTypeList.GetDescriptionFromCode(JE_MessageSubType); }
		}

		public
#if DEBUG
 virtual
#endif
 ZString MessageTypeDescription
		{
			get { return Lookups.MessageTypeList.GetDescriptionFromCode(JE_MessageType); }
		}

		public ZString Description
		{
			get
			{
				return HumanReadableShortcutNameCore;
			}
		}

		protected override ZString HumanReadableShortcutNameCore
		{
			get
			{
				var descriptionCore = DescriptionCore;
				return descriptionCore.IsEmpty ? JE_DeclarationReference : descriptionCore;
			}
		}

		ZString DescriptionCore
		{
			get
			{
				var declarationDescriptionCustomization = IsExport
					? CustomsDataRegistry.Instance.DeclarationExportDescriptionCustomization
					: CustomsDataRegistry.Instance.DeclarationImportDescriptionCustomization;

				var descriptionCustomizationCollection = declarationDescriptionCustomization.GetFallBackValueAtAllLevels(RegistryCompanyPK, RegistryBranchPK, Guid.Empty);

				var builder = new ZStringBuilder();

				var reference = descriptionCustomizationCollection.GetBoolFromCode("JNO")
					? Res.GetString("C1F50040-D147-4A59-BF59-0559FF93EA9E", "{0} - ", JE_DeclarationReference)
					: string.Empty;

				if (descriptionCustomizationCollection.GetBoolFromCode("MBL") && !JE_MasterBill.IsEmpty)
				{
					builder.Append(Res.GetString("ADD71A7E-B888-4620-86C0-CB7597F5AB5A", "MBL: {0}", JE_MasterBill));
				}

				if (descriptionCustomizationCollection.GetBoolFromCode("HBL") && !JE_HouseBill.IsEmpty)
				{
					builder.Append(Res.GetString("A7C7DB1B-17C1-457D-829E-6C0414A8013B", "HBL: {0}", JE_HouseBill));
				}

				if (descriptionCustomizationCollection.GetBoolFromCode("ENT"))
				{
					builder.Append(Res.GetString("8CBF1B08-12B7-4264-85EC-86E11F49453B", "ENT: {0}", DeclarationNumber));
				}

				if (descriptionCustomizationCollection.GetBoolFromCode("IMP") && Importer != null && !Importer.OH_Code.IsEmpty)
				{
					builder.Append(Res.GetString("F95C1268-87A2-4B7C-BDE4-2432C4DDED01", "IMP: {0}", Importer.OH_Code));
				}

				if (descriptionCustomizationCollection.GetBoolFromCode("EXP") && Supplier != null && !Supplier.OH_Code.IsEmpty)
				{
					builder.Append(Res.GetString("D43A6409-A138-4D28-9C11-5FC15F289A94", "EXP: {0}", Supplier.OH_Code));
				}

				if (descriptionCustomizationCollection.GetBoolFromCode("ORF") && !OtherReferenceNumber.IsEmpty)
				{
					builder.Append(Res.GetString("D0ACAB29-B2BA-4D8C-82D9-CEF883434E1E", "{0}: {1}", OtherReferenceNumberCaption, OtherReferenceNumber));
				}

				return builder.IsEmpty ? JE_DeclarationReference : new ZString(reference + builder.ToStringWithDelimiterBetweenAppends(", "));
			}
		}

		protected virtual ZString EntryStatusNotSendCode => CustomsEntryStatus.NotSent.Code;

		[ResourceStringData("Enterprise.Customs.Business.BaseJobDeclaration.JE_EntryStatusDescription", Caption = "Status")]
		public virtual ZString JE_EntryStatusDescription
		{
			get
			{
				var result = ZString.Empty;
				var entryStatusList = Lookups.EntryStatusList;
				if (entryStatusList.GetDescriptionFromCode(JE_EntryStatus) is string entryStatusDesc && entryStatusDesc.Length > 0)
				{
					result = entryStatusDesc;
				}
				else if (JE_EntryStatus.IsEmpty)
				{
					result = entryStatusList.GetDescriptionFromCode(EntryStatusNotSendCode);//TODO: Change this back when the Not Sent code is 'NOT'
				}
				else if (Company is GlbCompany company && DeclarationStatusHelper.ShouldCombineEntryStatusFromHeaders(company.GC_RN_NKCountryCode, company.PK) && JE_EntryStatus == CommonEntryStatusList.Codes.MultipleEntryStatus)
				{
					result = CommonEntryStatusList.Descriptions.MultipleEntryStatus;
				}
				else if (IsDeclarationIntegrated)
				{
					var integratedCountryCommonEntryStatusList = Factory.GetCachedValue<IntegratedCountryCommonEntryStatusList>();
					result = integratedCountryCommonEntryStatusList.GetDescriptionFromCode(JE_EntryStatus);
				}

				if (result.IsEmpty && !JE_EntryStatus.IsEmpty)
				{
					result = Res.GetString("db5919a4-fa44-46fd-bfc0-23730469a2e9", "Unknown");
				}
				return result;
			}
		}

		public ZPropertyInfo JE_EntryStatusDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.JE_EntryStatusDescription); }
		}

		public virtual ZString JE_MessageStatusDescription
		{
			get
			{
				var messageStatus = JE_MessageStatus;
				ZString result = Lookups.MessageStatusList.GetDescriptionFromCode(messageStatus) ?? ZString.Empty;

				if (messageStatus == CommonMessageStatusList.Codes.MultipleMessageStatus && DeclarationStatusHelper.ShouldCombineMessageStatusFromHeaders(this.CountryCode))
				{
					var messageStatuses = ActiveEntryHeaders.Cast<CusEntryHeader>().Select(x => x.CH_Status).ToHashSet();
					if (messageStatuses.Count > 0)
					{
						var messageStatusCategoryList = Lookups.MessageStatusCategoryList;
						var defaultCategory = MessageStatusList.GetCategoryForStatus(messageStatuses.First(), messageStatusCategoryList);

						var multipleStatus = ZString.Empty;
						if (messageStatuses.All(status => MessageStatusList.GetCategoryForStatus(status, messageStatusCategoryList) == defaultCategory))
						{
							multipleStatus = messageStatusCategoryList[defaultCategory, StringComparison.CurrentCulture].Description;
						}
						else if (messageStatuses.Any(status => IsMessageStatusAwaiting(status)))
						{
							multipleStatus = MessageStatusCategoryList.Descriptions.Awaiting;
						}
						else if (messageStatuses.Any(status => IsMessageStatusError(status)))
						{
							multipleStatus = MessageStatusCategoryList.Descriptions.Error;
						}

						if (!multipleStatus.IsEmpty)
						{
							result = MultipleMessageStatusWithSameCategory(multipleStatus);
						}
						else
						{
							result = CommonMessageStatusList.Descriptions.MultipleMessageStatus;
						}
					}
				}

				if (result.IsEmpty && !messageStatus.IsEmpty)
				{
					result = Res.GetString("50a9e07e-5b85-47e6-b308-f0c54dd69734", "Unknown");
				}
				return result;
			}
		}

		protected virtual bool IsMessageStatusAwaiting(string status) => MessageStatusList.IsAwaiting(status);
		protected virtual bool IsMessageStatusError(string status) => MessageStatusList.IsError(status);

		public static string MultipleMessageStatusWithSameCategory(string multipleStatus)
		{
			return Res.GetString("d44bf89c-0877-427b-afe2-a15f48610b22", "Multiple ({0}) - See Entries", multipleStatus);
		}

		public ZPropertyInfo JE_MessageStatusDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.JE_MessageStatusDescription); }
		}

		public virtual ZString ConsolidatedCargoStatusDescription
		{
			get
			{
				return ZString.Empty;
			}
		}

		public ZPropertyInfo ConsolidatedCargoStatusDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.ConsolidatedCargoStatusDescription); }
		}

		CurrencyConverter fCurrencyConverter;
		CurrencyConverter ICurrencyConverterProvider.CurrencyConverter
		{
			get
			{
				if (fCurrencyConverter == null)
				{
					fCurrencyConverter = new CurrencyConverterWithDataProvider(Factory, this);
				}
				return fCurrencyConverter;
			}
		}

		public virtual bool WillThereBeMultipleEntryHeaders
		{
			get { return false; }
		}

		public ZBool ShouldCloneContainersEvenNotLinked { get; set; } = false;

		public virtual ZBool IsSea
		{
			get { return JE_TransportMode == Enterprise.Core.Constants.TransportModes.Sea; }
		}

		public virtual bool IsAir
		{
			get { return JE_TransportMode == Enterprise.Core.Constants.TransportModes.Air; }
		}

		public ZBool IsAirZBool
		{
			get { return IsAir; }
		}

		public bool IsMail
		{
			get { return JE_TransportMode == Constants.TransportModes.Mail; }
		}

		public bool IsWaterwayTransport
		{
			get { return JE_TransportMode == Constants.TransportModes.InlandWaterwayTransport; }
		}

		public bool IsOwnPropulsion
		{
			get { return JE_TransportMode == Constants.TransportModes.OwnPropulsion; }
		}

		public bool IsAirInland
		{
			get { return JE_TransportModeInland == Constants.TransportModes.Air; }
		}

		public bool IsRoadInland
		{
			get { return JE_TransportModeInland == Constants.TransportModes.Road; }
		}

		public bool IsSeaInland
		{
			get { return JE_TransportModeInland == Constants.TransportModes.Sea; }
		}

		public bool IsWaterwayTransportsInland
		{
			get { return JE_TransportModeInland == Constants.TransportModes.InlandWaterwayTransport; }
		}

		public bool IsRailInland
		{
			get { return JE_TransportModeInland == Constants.TransportModes.Rail; }
		}

		public bool IsOwnPropulsionInland
		{
			get { return JE_TransportModeInland == Constants.TransportModes.OwnPropulsion; }
		}

		public bool IsFixedInstallationInland
		{
			get { return JE_TransportModeInland == Constants.TransportModes.FixedTransportInstallations; }
		}

		public bool IsMailInland
		{
			get { return JE_TransportModeInland == Constants.TransportModes.Mail; }
		}

		[ResourceStringData("02ACC9E2-9C65-4D10-B93E-6B29ED2C09EB", Caption = "Nationality", MediumCaption = "Nation", ShortCaption = "Nat.")]
		public override ZString JE_RN_NKTransportNationalityInland
		{
			get => base.JE_RN_NKTransportNationalityInland;
			set => base.JE_RN_NKTransportNationalityInland = value;
		}

		[ResourceStringData("1F42BFCA-AE1F-4B1E-B9A4-845ADFD89728", Caption = "Nationality", MediumCaption = "Nation", ShortCaption = "Nat.")]
		public override ZString JE_RN_NKTransportNationality
		{
			get => base.JE_RN_NKTransportNationality;
			set => base.JE_RN_NKTransportNationality = value;
		}

		[ResourceStringData("008FD9C5-3933-4BBB-842C-8B29204086BE", Caption = "Nationality", MediumCaption = "Nation", ShortCaption = "Nat.")]
		public override ZString JE_RN_NKTrailer1Nationality
		{
			get => base.JE_RN_NKTrailer1Nationality;
			set => base.JE_RN_NKTrailer1Nationality = value;
		}

		[ResourceStringData("D7A371E9-5F01-4863-84C2-5A9A709DE098", Caption = "Nationality", MediumCaption = "Nation", ShortCaption = "Nat.")]
		public override ZString JE_RN_NKTrailer2Nationality
		{
			get => base.JE_RN_NKTrailer2Nationality;
			set => base.JE_RN_NKTrailer2Nationality = value;
		}

		[ResourceStringData("D17749E7-1DC7-4D40-B9F5-1FD9BACEA7EA", Caption = "Aircraft ID", MediumCaption = "Plane ID", ShortCaption = "ID")]
		public override ZString JE_AircraftRegistrationInland
		{
			get => base.JE_AircraftRegistrationInland;
			set => base.JE_AircraftRegistrationInland = value;
		}

		public bool FlightNoHasNumericAirlineCode
		{
			get
			{
				var result = false;
				if (IsAir && !JE_VoyageFlightNo.IsEmpty)
				{
					var airlineCode = JE_VoyageFlightNo.SubstringSafe(0, 2);
					if (airlineCode.Length == 2 && !airlineCode.IsNumbersOnlyOrEmpty && !airlineCode.IsLettersOnlyOrEmpty)    // Numeric Airline codes are formatted 9X or X9 only, no 99 codes.
					{
						var validAirline = Factory.LoadTop1<RefAirline>(new ZQuery(RefAirlineSchema.RM_TwoCharacterCode, airlineCode));
						result = validAirline != null;
					}
				}

				return result;
			}
		}

		ZBool IFlightDetailsSuppression.HasActualRCVPassed
		{
			get { return !JE_CartageCompleted.IsEmpty && ZDateTime.Now > JE_CartageCompleted; }
		}

		ZBool IFlightDetailsSuppression.HasETDPassed
		{
			get { return !JE_ExportDate.IsEmpty && ZDateTime.Now > JE_ExportDate; }
		}

		ZBool IFlightDetailsSuppression.IsPassengerFlight
		{
			get
			{
				Transport lastLeg = new TransportOrderHelper(TransportsIncludingRelated).LastLegWithTransportMode(Constants.TransportModes.Air, null, null);
				if (lastLeg == null)
				{
					return true;
				}
				return !lastLeg.JW_IsCargoOnly;
			}
		}

		ZBool IFlightDetailsSuppression.HasFinalRoutingLegATDPassed
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

		public virtual ZBool IsPost
		{
			get { return JE_TransportMode == Enterprise.Core.Constants.TransportModes.Mail; }
		}

		public virtual ZBool IsRoad
		{
			get { return JE_TransportMode == Enterprise.Core.Constants.TransportModes.Road; }
		}

		public virtual ZBool IsRail
		{
			get { return JE_TransportMode == Enterprise.Core.Constants.TransportModes.Rail; }
		}

		public virtual ZBool IsFixedInstallation
		{
			get { return JE_TransportMode == Enterprise.Core.Constants.TransportModes.FixedTransportInstallations; }
		}

		public bool ExportGoodsTypeIsPostal
		{
			get { return JE_ExportGoodsType == "PO"; }
		}

		public bool ExportGoodsTypeIsStores
		{
			get { return JE_ExportGoodsType == "ST"; }
		}

		public bool ExportGoodsTypeIsSpares
		{
			get { return JE_ExportGoodsType == "SP"; }
		}

		public ZString AirlinePrefix
		{
			get { return IsAir ? CarrierId : ZString.Empty; }
		}

		public
#if DEBUG
 virtual
#endif
 ZString CarrierId
		{
			get { return JE_VoyageFlightNo.ToUpper().Left(2); }
		}

		public virtual ZBool ContainersAlwaysRequired
		{
			get { return false; }
		}

		public virtual ZBool ContainersRequired
		{
			get { return ((!IsNonTransportDeclarationType && IsSea) || (!IsAir && Enterprise.Core.Constants.ContainerModes.IsContainerised(JE_ContainerMode)) || ContainersAlwaysRequired); }
		}

		#region JobDocsAndCartage Import/Export Properties

		#region JP_Calc_CartageAdvised

		public ZDateTime JP_Calc_CartageAdvised
		{
			get { return IsExport ? DocsAndCartage.JP_PickupCartageAdvised : DocsAndCartage.JP_DeliveryCartageAdvised; }
			set
			{
				if (IsExport)
				{
					DocsAndCartage.JP_PickupCartageAdvised = value;
				}
				else
				{
					DocsAndCartage.JP_DeliveryCartageAdvised = value;
				}
			}
		}

		public ZPropertyInfo JP_Calc_CartageAdvisedInfo
		{
			get
			{
				return GetWrappedZPropertyInfo(BaseJobDeclaration.Schema.JP_Calc_CartageAdvised,
												 x => IsExport ? DocsAndCartage.JP_PickupCartageAdvisedInfo : DocsAndCartage.JP_DeliveryCartageAdvisedInfo);
			}
		}

		#endregion

		public Money GoodsValue
		{
			get
			{
				Money result = Money.Empty;
				foreach (BaseJobComInvoiceGroupHeader groupHeader in JobComInvoiceGroupHeaders)
				{
					foreach (BaseJobComInvoiceHeader invoiceHeader in groupHeader.AllJobComInvoiceHeaders)
					{
						if (Validation is BaseJobDeclarationValidation validation && validation.IsValidCurrency(invoiceHeader.JZ_Calc_FOBCurrency))
						{
							result = invoiceHeader.CurrencyConverter.Add(result, new Money(invoiceHeader.JZ_Calc_FOBAmount, invoiceHeader.Invoice_Currency));
						}
						else
						{
							result = new Money(result, false);
						}
					}
				}
				return result;
			}
		}

		#region ClientPickupDeliveryAddressPK

		public ZGuid ClientPickupDeliveryAddressPK
		{
			get { return ClientPickupDeliveryAddress.E2_OA_Address; }
			set { ClientPickupDeliveryAddress.E2_OA_Address = value; }
		}

		public JobDocAddress ClientPickupDeliveryAddress
		{
			get { return IsExport ? SupplierPickupAddress : ImporterDeliveryAddress; }
		}

		#endregion

		#region JE_CartageCompleted

		public ZDateTime JE_CartageCompleted
		{
			get { return IsExport ? DocsAndCartage.JP_PickupCartageCompleted : DocsAndCartage.JP_DeliveryCartageCompleted; }
			set
			{
				if (IsExport)
				{
					var oldDateValue = DocsAndCartage.JP_PickupCartageCompleted;
					DocsAndCartage.JP_PickupCartageCompleted = value;
					SetLastestContainerTimes(value, oldDateValue, CommonContainer.Schema.JC_DepartureCartageComplete, ConfirmTimesSyncHelper.ConfirmDateType.Actual);
				}
				else
				{
					var oldDateValue = DocsAndCartage.JP_DeliveryCartageCompleted;
					DocsAndCartage.JP_DeliveryCartageCompleted = value;
					SetLastestContainerTimes(value, oldDateValue, CommonContainer.Schema.JC_ArrivalCartageComplete, ConfirmTimesSyncHelper.ConfirmDateType.Actual);
				}
			}
		}

		public ZPropertyInfo JE_CartageCompletedInfo
		{
			get
			{
				return GetWrappedZPropertyInfo(BaseJobDeclaration.Schema.JE_CartageCompleted,
												 x => IsExport ? DocsAndCartage.JP_PickupCartageCompletedInfo : DocsAndCartage.JP_DeliveryCartageCompletedInfo);
			}
		}

		#endregion

		#region JE_EstimatedDeliveryOrPickup

		public ZDateTime JE_EstimatedDeliveryOrPickup
		{
			get { return (ZDateTime)JE_EstimatedDeliveryOrPickupInfo.InnerInfo.Value; }
			set
			{
				var oldDateValue = (ZDateTime)JE_EstimatedDeliveryOrPickupInfo.InnerInfo.Value;
				JE_EstimatedDeliveryOrPickupInfo.InnerInfo.Value = value;
				SetLastestContainerTimes(value, oldDateValue, IsImport ? CommonContainer.Schema.JC_ArrivalEstimatedDelivery : CommonContainer.Schema.JC_DepartureEstimatedPickup, ConfirmTimesSyncHelper.ConfirmDateType.Planned);
			}
		}

		void SetLastestContainerTimes(ZDateTime newDate, ZDateTime oldDateValue, string containerColumnNameToSet, ConfirmTimesSyncHelper.ConfirmDateType actualOrEstimated)
		{
			var latestContainer = (from BaseCusContainer cusContainer in CusContainers orderby (ZDateTime)cusContainer.JobContainer[containerColumnNameToSet] select cusContainer).LastOrDefault();
			if (latestContainer != null)
			{
				ConfirmTimesSyncHelper.SetContainerTimes(
					latestContainer.JobContainer,
					IsImport ? ConfirmTimesSyncHelper.ConfirmType.Delivery : ConfirmTimesSyncHelper.ConfirmType.Pickup,
					actualOrEstimated,
					oldDateValue,
					newDate);
			}
		}

		public ZWrappedPropertyInfo JE_EstimatedDeliveryOrPickupInfo
		{
			get
			{
				return GetWrappedZPropertyInfo(BaseJobDeclaration.Schema.JE_EstimatedDeliveryOrPickup,
												 x => IsExport ? DocsAndCartage.JP_EstimatedPickupInfo : DocsAndCartage.JP_EstimatedDeliveryInfo);
			}
		}

		#endregion

		#region JE_FCLDeliveryOrPickupEquipmentNeeded

		[List(nameof(DeliveryOrPickupEquipmentNeededList))]
		public ZString JE_FCLDeliveryOrPickupEquipmentNeeded
		{
			get { return IsExport ? DocsAndCartage.JP_FCLPickupEquipmentNeeded : DocsAndCartage.JP_FCLDeliveryEquipmentNeeded; }
			set
			{
				if (IsExport)
				{
					DocsAndCartage.JP_FCLPickupEquipmentNeeded = value;
				}
				else
				{
					DocsAndCartage.JP_FCLDeliveryEquipmentNeeded = value;
				}
			}
		}

		public ZPropertyInfo JE_FCLDeliveryOrPickupEquipmentNeededInfo
		{
			get
			{
				return GetWrappedZPropertyInfo(BaseJobDeclaration.Schema.JE_FCLDeliveryOrPickupEquipmentNeeded,
												 x => IsExport ? DocsAndCartage.JP_FCLPickupEquipmentNeededInfo : DocsAndCartage.JP_FCLDeliveryEquipmentNeededInfo);
			}
		}

		public CodeDescriptionPairList DeliveryOrPickupEquipmentNeededList => IsExport ? DocsAndCartage.Lookups.PickupEquipmentNeededList : DocsAndCartage.Lookups.DeliveryEquipmentNeededList;

		#endregion

		#region JE_DeliveryOrPickupLabourTime

		public ZDateTime JE_DeliveryOrPickupLabourTime
		{
			get { return IsExport ? DocsAndCartage.JP_PickupLabourTime : DocsAndCartage.JP_DeliveryLabourTime; }
			set
			{
				if (IsExport)
				{
					DocsAndCartage.JP_PickupLabourTime = value;
				}
				else
				{
					DocsAndCartage.JP_DeliveryLabourTime = value;
				}
			}
		}

		public ZPropertyInfo JE_DeliveryOrPickupLabourTimeInfo
		{
			get
			{
				return GetWrappedZPropertyInfo(BaseJobDeclaration.Schema.JE_DeliveryOrPickupLabourTime,
												 x => IsExport ? DocsAndCartage.JP_PickupLabourTimeInfo : DocsAndCartage.JP_DeliveryLabourTimeInfo);
			}
		}

		#endregion

		#region JE_DeliveryOrPickupLabourCharge

		public ZDecimal JE_DeliveryOrPickupLabourCharge
		{
			get { return IsExport ? DocsAndCartage.JP_PickupLabourCharge : DocsAndCartage.JP_DeliveryLabourCharge; }
			set
			{
				if (IsExport)
				{
					DocsAndCartage.JP_PickupLabourCharge = value;
				}
				else
				{
					DocsAndCartage.JP_DeliveryLabourCharge = value;
				}
			}
		}

		public ZPropertyInfo JE_DeliveryOrPickupLabourChargeInfo
		{
			get
			{
				return GetWrappedZPropertyInfo(BaseJobDeclaration.Schema.JE_DeliveryOrPickupLabourCharge,
												 x => IsExport ? DocsAndCartage.JP_PickupLabourChargeInfo : DocsAndCartage.JP_DeliveryLabourChargeInfo);
			}
		}

		#endregion

		#region JE_PickupOrDeliveryTruckWaitTime

		public ZDateTime JE_PickupOrDeliveryTruckWaitTime
		{
			get { return IsExport ? DocsAndCartage.JP_PickupTruckWaitTime : DocsAndCartage.JP_DeliveryTruckWaitTime; }
			set
			{
				if (IsExport)
				{
					DocsAndCartage.JP_PickupTruckWaitTime = value;
				}
				else
				{
					DocsAndCartage.JP_DeliveryTruckWaitTime = value;
				}
			}
		}

		public ZPropertyInfo JE_PickupOrDeliveryTruckWaitTimeInfo
		{
			get
			{
				return GetWrappedZPropertyInfo(BaseJobDeclaration.Schema.JE_PickupOrDeliveryTruckWaitTime,
												 x => IsExport ? DocsAndCartage.JP_PickupTruckWaitTimeInfo : DocsAndCartage.JP_DeliveryTruckWaitTimeInfo);
			}
		}

		#endregion

		#region JE_PickupOrDeliveryTruckWaitCharge

		public ZDecimal JE_PickupOrDeliveryTruckWaitCharge
		{
			get { return IsExport ? DocsAndCartage.JP_PickupTruckWaitCharge : DocsAndCartage.JP_DeliveryTruckWaitCharge; }
			set
			{
				if (IsExport)
				{
					DocsAndCartage.JP_PickupTruckWaitCharge = value;
				}
				else
				{
					DocsAndCartage.JP_DeliveryTruckWaitCharge = value;
				}
			}
		}

		public ZPropertyInfo JE_PickupOrDeliveryTruckWaitChargeInfo
		{
			get
			{
				return GetWrappedZPropertyInfo(BaseJobDeclaration.Schema.JE_PickupOrDeliveryTruckWaitCharge,
												 x => IsExport ? DocsAndCartage.JP_PickupTruckWaitChargeInfo : DocsAndCartage.JP_DeliveryTruckWaitChargeInfo);
			}
		}

		#endregion

		#region JE_OA_DeliveryOrPickupCartageCoAddr

		#region ZAddress

		[EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
		public ZAddress JE_OA_DeliveryOrPickupCartageCoAddr_ZAddress
		{
			get { return IsExport ? DocsAndCartage.JP_OA_PickupCartageCoAddr_ZAddress : DocsAndCartage.JP_OA_DeliveryCartageCoAddr_ZAddress; }
		}

		#endregion

		[RelatedBusinessObject("DeliveryOrPickupCartageCoAddr")]
		public ZGuid JE_OA_DeliveryOrPickupCartageCoAddr
		{
			get { return IsExport ? DocsAndCartage.JP_OA_PickupCartageCoAddr : DocsAndCartage.JP_OA_DeliveryCartageCoAddr; }
			set
			{
				if (IsExport)
				{
					DocsAndCartage.JP_OA_PickupCartageCoAddr = value;
				}
				else
				{
					DocsAndCartage.JP_OA_DeliveryCartageCoAddr = value;
				}
			}
		}

		public ZPropertyInfo JE_OA_DeliveryOrPickupCartageCoAddrInfo
		{
			get
			{
				return GetWrappedZPropertyInfo(Schema.JE_OA_DeliveryOrPickupCartageCoAddr,
												 x => IsExport ? DocsAndCartage.JP_OA_PickupCartageCoAddrInfo : DocsAndCartage.JP_OA_DeliveryCartageCoAddrInfo);
			}
		}

		public OrgAddress DeliveryOrPickupCartageCoAddr
		{
			get { return Factory.Load<OrgAddress>(JE_OA_DeliveryOrPickupCartageCoAddr); }
		}

		public OrgHeader DeliveryOrPickupCartageCo
		{
			get { return DeliveryOrPickupCartageCoAddr?.Header; }
		}

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.CartageList))]
		public virtual ZGuid DeliveryOrPickupCartageCoPK
		{
			get { return JE_OA_DeliveryOrPickupCartageCoAddr_ZAddress.OrgPK; }
			set { JE_OA_DeliveryOrPickupCartageCoAddr_ZAddress.OrgPK = value; }
		}

		public ZPropertyInfo DeliveryOrPickupCartageCoPKInfo
		{
			get
			{
				return GetWrappedZPropertyInfo(Schema.DeliveryOrPickupCartageCoPK, x => JE_OA_DeliveryOrPickupCartageCoAddr_ZAddress.OrgPKInfo);
			}
		}
		#endregion

		#region JE_DeliveryOrPickupRequiredBy

		public ZDateTime JE_DeliveryOrPickupRequiredBy
		{
			get { return IsExport ? DocsAndCartage.JP_PickupRequiredBy : DocsAndCartage.JP_DeliveryRequiredBy; }
			set
			{
				if (IsExport)
				{
					DocsAndCartage.JP_PickupRequiredBy = value;
				}
				else
				{
					DocsAndCartage.JP_DeliveryRequiredBy = value;
				}
			}
		}

		public ZPropertyInfo JE_DeliveryOrPickupRequiredByInfo
		{
			get
			{
				return GetWrappedZPropertyInfo(Schema.JE_DeliveryOrPickupRequiredBy,
												 x => IsExport ? DocsAndCartage.JP_PickupRequiredByInfo : DocsAndCartage.JP_DeliveryRequiredByInfo);
			}
		}

		#endregion

		#endregion

		public ZBool IsBulk
		{
			get { return JE_ContainerMode == Enterprise.Core.Constants.ContainerModes.Bulk; }
		}

		public ZBool IsLiquid
		{
			get { return JE_ContainerMode == Enterprise.Core.Constants.ContainerModes.Liquid; }
		}

		public virtual ZBool IsBreakBulk
		{
			get { return JE_ContainerMode == Enterprise.Core.Constants.ContainerModes.BreakBulk; }
		}

		public ZBool HasBreakBulk
		{
			get
			{
				ZBool result = IsBreakBulk;
				if (!result)
				{
					foreach (BaseCusContainer container in CusContainers)
					{
						result |= container.CO_FCL_LCL_AIR == Enterprise.Core.Constants.ContainerModes.BreakBulk;
					}
				}
				return result;
			}
		}

		public virtual ZBool IsEmptyContainer
		{
			get { return JE_ContainerMode == Core.Constants.ContainerModes.Empty; }
		}

		public virtual ZBool IsContainerised
		{
			get { return Core.Constants.ContainerModes.IsContainerised(JE_ContainerMode); }
		}

		public virtual ZString GetDefaultContainerisedContainerMode()
		{
			return Enterprise.Core.Constants.ContainerModes.Containerised;
		}

		public ZBool IsStandAlone
		{
			get { return (JE_JS == ZGuid.Empty); }
		}

		public ZString MarksAndNumbers
		{
			get
			{
				StmNote[] marksAndNumbersNotes = NotesOfDeclarationOrShipment.FindByDescription(PredefinedNoteTypes.Instance.MarksAndNumbers.Description);
				return GetStringValueForNotes(marksAndNumbersNotes);
			}
		}

		protected string GetStringValueForNotes(StmNote[] notes)
		{
			StringBuilder result = new StringBuilder();

			foreach (StmNote note in notes)
			{
				result.Append(note.ST_NoteText);
			}

			return result.ToString();
		}

		public Notes NotesOfDeclarationOrShipment
		{
			get { return Shipment != null ? Shipment.Notes : Notes; }
		}

		public Logs LogsOfDeclarationOrShipment
		{
			get { return LogParent.Logs; }
		}

		public bool EntryHasBeenSubmitted
		{
			get
			{
				return MostRecentCustomsCommencedEvent != null;
			}
		}

		StmALog MostRecentCustomsCommencedEvent => LogsOfDeclarationOrShipment.MostRecentLogByEventTime(CustomsCommencedEventType);

		public bool IsValidToSendToCustoms
		{
			get
			{
				LoadChildEditableObjects();
				RunPreSaveValidation();
				bool result = !HasErrors;
				return result;
			}
		}

		public bool IsThereAtLeastOneInvoiceHeaderAndOneInvoiceLinePerInvoiceHeader => Factory.GetValue(ref isThereAtLeastOneInvoiceHeaderAndOneInvoiceLinePerInvoiceHeaderCached, () =>
		{
			var result = false;
			if (JobComInvoiceGroupHeaders.Count > 0)
			{
				var allJobComInvoiceHeaders = JobComInvoiceGroupHeaders[0].AllJobComInvoiceHeaders.ToList();
				if (this.SupportAdditionalInvoices)
				{
					foreach (var invoice in Invoices)
					{
						if (Invoices.IsAdditionalInvoice(invoice))
						{
							allJobComInvoiceHeaders.Add(invoice);
						}
					}
				}

				result = allJobComInvoiceHeaders.Count > 0;
				if (result)
				{
					foreach (BaseJobComInvoiceHeader aHeader in allJobComInvoiceHeaders)
					{
						result = (aHeader.JobComInvoiceLines.Count > 0);
						if (!result)
						{
							break;
						}
					}
				}
			}
			return result;
		});
		CachedProperty<bool> isThereAtLeastOneInvoiceHeaderAndOneInvoiceLinePerInvoiceHeaderCached;

		[MaxLength(20)]
		public ZString OrderNumbers
		{
			get
			{
				ZString result = "";
				foreach (Order order in AttachedOrders)
				{
					if (result.Length > 0)
					{
						result += ", ";
					}

					result += order.JD_OrderNumber;
				}
				return result.SubstringSafe(0, 20);
			}
		}

		public ZPropertyInfo OrderNumbersInfo
		{
			get { return GetZPropertyInfo(Schema.OrderNumbers); }
		}

		public ZBool ShouldDefaultFCLCartageCo
		{
			get
			{
				if (IsImport)
				{
					return HasContainersOfMode(Core.Constants.ContainerModes.FCL)
							 || HasContainersOfMode(Core.Constants.ContainerModes.FCLMixedShipper)
							 || (JE_ContainerMode == Core.Constants.ContainerModes.FCL && IsSea && CusContainers.Count == 0)
							 || (JE_ContainerMode == Core.Constants.ContainerModes.FCLMixedShipper && IsSea && CusContainers.Count == 0);
				}
				else if (IsExport)
				{
					return HasContainersOfMode(Core.Constants.ContainerModes.FCL);
				}
				else
				{
					return false;
				}
			}
		}

		public ZString MessageTypeForDocumentFilter
		{
			get { return GetMessageTypeForDocumentFilter(); }
		}

		protected virtual ZString GetMessageTypeForDocumentFilter()
		{
			ZString result = "";

			if (IsImport || IsExWarehouse)
			{
				result = JobMessageTypeList.Codes.Import;
			}
			else if (IsExport)
			{
				result = JobMessageTypeList.Codes.Export;
			}
			else if (IsDrawback)
			{
				result = JobMessageTypeList.Codes.Drawback;
			}

			return result;
		}

		/// <summary>
		/// Message Type(s) for document filtering; eg in US a job can be for ENS, INB, CRL but they are all for import
		/// </summary>
		public ZString MessageTypesForDocumentFilter
		{
			get
			{
				var result = new ZStringBuilder(GetMessageTypesForDocumentFilter());
				var extraPad = result.IsEmpty ? "" : ",";
				return extraPad + result.ToStringWithDelimiterBetweenAppends(",") + extraPad;
			}
		}

		protected virtual IEnumerable<ZString> GetMessageTypesForDocumentFilter()
		{
			if (IsImport || IsExWarehouse)
			{
				yield return JobMessageTypeList.Codes.Import;
			}
			else if (IsExport)
			{
				yield return JobMessageTypeList.Codes.Export;
			}
			else if (IsDrawback)
			{
				yield return JobMessageTypeList.Codes.Drawback;
			}
		}

		public ZString MessageTypeForHSAssist => GetMessageTypeForHSAssist();

		protected virtual ZString GetMessageTypeForHSAssist() => JE_MessageType;

		[MaxLength(3)]
		public ZString JE_BillsFilterBy
		{
			get { return FilteredBills.FilterBy; }
			set
			{
				CheckMaximumLength(JE_BillsFilterByInfo, value);
				FilteredBills.FilterBy = value;
				JE_BillsFilterByInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo JE_BillsFilterByInfo
		{
			get { return GetZPropertyInfo(Schema.JE_BillsFilterBy); }
		}

		public bool IsDataSyncFromShipment
		{
			get { return ShouldSynchroniseWithShipment(); }
		}

		public bool SupportUseOwnerRefAsQuarantineRefUsage
		{
			get { return SupportUseOwnerRefAsQuarantineRefUsageCore; }
		}

		protected virtual bool SupportUseOwnerRefAsQuarantineRefUsageCore
		{
			get { return false; }
		}

		public bool SupportJE_PaymentMethodUsage
		{
			get { return SupportJE_PaymentMethodUsageCore; }
		}

		protected virtual bool SupportJE_PaymentMethodUsageCore
		{
			get { return false; }
		}

		public bool SuspendAddingWorkflow
		{
			get;
			set;
		}

		public virtual bool AddingWorkflowByBaseJobDeclaration
		{
			get { return true; }
		}

		public virtual bool SupportsParentPackage
		{
			get { return false; }
		}

		public virtual event EventHandler ParentPackageColumnSupportedChanged
		{
			add { }
			remove { }
		}

		public virtual bool AllowEntryLinesToBeLinkedToAnotherJob => false;

		#endregion

		#region ApportionmentEnabled

		public event ApportionmentDirtyChangedEventHandler OnApportionmentDirtyChanged;
		public delegate void ApportionmentDirtyChangedEventHandler();

		public bool ApportionmentDirty
		{
			get { return fApportionmentDirty; }
			set
			{
				bool hasChanges = fApportionmentDirty != value;
				fApportionmentDirty = value;

				if (hasChanges)
				{
					OnApportionmentDirtyChanged?.Invoke();
				}
			}
		}

		public void MarkApportionmentDirty()
		{
			if (!IsDeleted && !IsMarkApportionmentDirtySuspended)
			{
				ApportionmentDirty = true;
				MarkApportionmentDirtyCore();
			}
		}

		protected virtual void MarkApportionmentDirtyCore()
		{
		}

		/// <summary>
		/// Used when SetDefaultValues for invoice for incoterm and currency or Clone happens
		/// </summary>
		/// <returns></returns>
		public IDisposable SuspendMarkApportionmentDirty()
		{
			return new MarkApportionmentDirtySuspender(this);
		}

		bool IsMarkApportionmentDirtySuspended
		{
			get { return markApportionmentDirtySuspenderIndex > 0; }
		}

		int markApportionmentDirtySuspenderIndex;
		class MarkApportionmentDirtySuspender : IDisposable
		{
			public MarkApportionmentDirtySuspender(BaseJobDeclaration declaration)
			{
				this.declaration = declaration;
				declaration.markApportionmentDirtySuspenderIndex++;
			}

			readonly BaseJobDeclaration declaration;

			public void Dispose()
			{
				declaration.markApportionmentDirtySuspenderIndex--;
			}
		}

		public void ValidateIncoTerms()
		{
			foreach (BaseJobComInvoiceHeader invoice in Invoices)
			{
				invoice.Validation.ValidateJZ_IncoTerm();
			}
		}

		public void ResumeApportionment()
		{
			if (!isApportionmentInProgress)
			{
				isApportionmentInProgress = true;

				try
				{
					OnApportioning();
					ApportionManager.ApportionAll();
					ApportionmentDirty = false;
					OnApportioned();
				}
				finally
				{
					isApportionmentInProgress = false;
				}
			}
		}

		bool isApportionmentInProgress;

		#region ApportionManager

		ApportionManager ApportionManager
		{
			get { return fApportionManager ?? (fApportionManager = new ApportionManager(this, GetNewApportionStrategy())); }
		}
		ApportionManager fApportionManager;
		protected virtual IApportionStrategy GetNewApportionStrategy() => new ApportionStrategy();

		#endregion

		protected virtual void OnApportioning()
		{
			currentApportionment = 0;

			RefreshExRateToLatestRateAvailableIfNeeded();
		}

		protected virtual void OnApportioned()
		{
			foreach (BaseJobComInvoiceHeader invoice in Invoices)
			{
				invoice.JZ_Calc_BalanceStringInfo.RefreshBinding();

				if (invoice.Validation is InvoiceHeaderValidation validation)
				{
					validation.ValidateJZ_Calc_TNI();
					validation.ValidateJZ_Calc_CIFAmount();
					validation.ValidateJZ_IncoTerm();
					validation.ValidateJZ_Calc_Balance();
					validation.ValidateJZ_Calc_OFTInInvoiceCurrency();
					validation.ValidateJZ_Calc_ONSInInvoiceCurrency();
				}
			}
		}

		public void RefreshExRateToLatestRateAvailableIfNeeded()
		{
			if (ShouldRefreshExchangeRates)
			{
				RefreshExRateToLatestRateAvailable();
			}
		}

		protected virtual bool ShouldRefreshExchangeRates
		{
			get { return true; }
		}

		protected void RefreshExRateToLatestRateAvailable()
		{
			LoadFetchHintForRefreshExRate();
			var currencyProviders = GetCurrencyProvidersToRefreshExRatesFor();

			BeforeExRatesRefreshed(currencyProviders);

			foreach (ICurrencyProvider currencyProvider in currencyProviders)
			{
				currencyProvider.SetExchangeRateIfNotUserOverridden();
			}

			AfterExRatesRefreshed(currencyProviders);

			if (!isApportionmentInProgress && HasChanges)
			{
				MarkApportionmentDirty();
			}
		}

		void LoadFetchHintForRefreshExRate()
		{
			if (FetchStrategy is FetchStrategies.BaseJobDeclarationFetchStrategy declarationFetchStrategy)
			{
				declarationFetchStrategy.FetchForRefreshExRate();
			}
		}

		protected virtual void BeforeExRatesRefreshed(IEnumerable<ICurrencyProvider> foreignCurrencyProviders)
		{
		}

		protected virtual void AfterExRatesRefreshed(IEnumerable<ICurrencyProvider> foreignCurrencyProviders)
		{
		}

		IEnumerable<ICurrencyProvider> GetCurrencyProvidersToRefreshExRatesFor()
		{
			var localCurrency = LocalCurrencyCode;

			if (!localCurrency.IsEmpty)
			{
				AddInvoiceChargesFetchHintsIfNeeded();
				foreach (BaseJobComInvoiceGroupHeader groupInvoice in AllGroupHeaders)
				{
					foreach (ICurrencyProvider charge in groupInvoice.Charges)
					{
						if (ShouldRefreshExRatesOnApportionment(charge, localCurrency))
						{
							yield return charge;
						}
					}
				}

				foreach (BaseJobComInvoiceHeader invoice in Invoices)
				{
					foreach (ICurrencyProvider currencyProvider in invoice.GetCurrencyProvidersToRefreshExRatesFor())
					{
						yield return currencyProvider;
					}
				}
			}
		}

		protected internal virtual bool ShouldRefreshExRatesOnApportionment(ICurrencyProvider currencyProvider, ZString localCurrency)
		{
			return currencyProvider.IsForeignCurrency(localCurrency);
		}

		bool fApportionmentDirty;

		protected internal int currentApportionment;
		protected internal int TotalApportionments
		{
			get
			{
				int result = 0;

				int groupCharge = GetTotalGroupCharge(JobComInvoiceGroupHeaders[0]);

				int invoiceCharge = 0;
				foreach (BaseJobComInvoiceHeader invoice in Invoices)
				{
					invoiceCharge += invoice.Charges.Count * invoice.JobComInvoiceLines.Count;
				}

				result = invoiceCharge + groupCharge;
				return result;
			}
		}

		int GetTotalGroupCharge(BaseJobComInvoiceGroupHeader groupHeader)
		{
			int result = 0;
			foreach (BaseJobComInvoiceGroupHeader subGroup in groupHeader.JobComInvoiceGroupHeaders)
			{
				result += GetTotalGroupCharge(subGroup);
			}

			result += groupHeader.Charges.Count * (groupHeader.AllJobComInvoiceHeaders.Count + groupHeader.AllJobComInvoiceLines.Count);

			return result;
		}

		public event ApportionmentProgressEventHandler OnApportionmentProgressChanged;
		public delegate void ApportionmentProgressEventHandler(int percentage);

		public void UpdateApportionmentProgress()
		{
			if (OnApportionmentProgressChanged != null)
			{
				int totalApportionmentsCached = TotalApportionments;
				if (currentApportionment < totalApportionmentsCached)
				{
					currentApportionment++;
				}

				int result = 0;
				if (totalApportionmentsCached != 0)
				{
					decimal percentage = (Convert.ToDecimal(currentApportionment) / Convert.ToDecimal(totalApportionmentsCached)) * 100m;
					result = Convert.ToInt16(percentage);
				}

				OnApportionmentProgressChanged(result);
			}
		}

		string IApportionInvoiceHolder.CountryContext
		{
			get { return GetIApportionInvoiceHolderCountryContextCore(); }
		}

		protected virtual string GetIApportionInvoiceHolderCountryContextCore()
		{
			return CountryCode;
		}

		#endregion

		#region Weight Apportionment

		public override ZBool JE_AutoWeightApportion
		{
			get
			{
				return base.JE_AutoWeightApportion;
			}
			set
			{
				bool oldValue = JE_AutoWeightApportion;
				base.JE_AutoWeightApportion = value;
				if (!IsCopying && oldValue != JE_AutoWeightApportion)
				{
					ApportionInvoiceWeight(null);
				}
			}
		}

		public void ApportionInvoiceWeight(BaseJobComInvoiceHeader uncommittedHeader)
		{
			if (ShouldRunWeightApportionment)
			{
				WeightApportionManager.ApportionAll(this, uncommittedHeader);
			}
		}

		internal bool ShouldRunWeightApportionment
		{
			get { return !IsImportingData && IsWeightApportionmentSupported && JE_AutoWeightApportion; }
		}

		public WeightApportionManager WeightApportionManager
		{
			get { return fWeightApportionManager ?? (fWeightApportionManager = new WeightApportionManager()); }
		}
		WeightApportionManager fWeightApportionManager;

		public IDisposable SuspendWeightApportionment()
		{
			return new WeightApportionmentSuspender(this);
		}

		public bool WeightApportionmentEnabled
		{
			get
			{
				return weightApportionmentSuspendedCounter == 0;
			}
		}

		int weightApportionmentSuspendedCounter;

		public bool IsWeightApportionmentSupported
		{
			get { return IsWeightApportionmentSupportedCore; }
		}

		// AU ExWarehouse does not have declaration or invoice weight fields exposed
		protected virtual bool IsWeightApportionmentSupportedCore
		{
			get { return true; }
		}

		class WeightApportionmentSuspender : IDisposable
		{
			public WeightApportionmentSuspender(BaseJobDeclaration declaration)
			{
				this.declaration = declaration;
				this.declaration.weightApportionmentSuspendedCounter++;
			}

			readonly BaseJobDeclaration declaration;

			public void Dispose()
			{
				this.declaration.weightApportionmentSuspendedCounter--;
			}
		}

		#endregion

		#region BatchProcessor and Messaging

		#region Message Initiator

		public ISendsMessagesToCustoms MessageInitiator
		{
			get
			{
				if (fMessageInitiator == null)
				{
					throw new ApplicationException("You can't perform this action that results in a message being sent because you have not hooked up a ISendsMessagesToCustoms to the Declaration");
				}

				return fMessageInitiator;
			}
			set { fMessageInitiator = value; }
		}

		public bool HasMessageInitiator
		{
			get { return fMessageInitiator != null; }
		}

		protected ISendsMessagesToCustoms fMessageInitiator;

		#endregion

		#region Messaging

		public EDIMessageCollection Messages
		{
			get
			{
				if (fMessages == null)
				{
					fMessages = GetNewMessagesCollection();
					var legacyMessages = (ILegacyBusinessObjectCollectionInternals)fMessages;
					legacyMessages.SetOverriddenAdditionalFilter(legacyMessages.AdditionalFilter.AddToFilter(MessageFilter));
					fMessages.Load();
					fMessages.IsManagedForDataRefresh = true;
				}
				return fMessages;
			}
		}

		protected virtual EDIMessageCollection GetNewMessagesCollection()
		{
			return new EDIMessageCollection(this, Factory);
		}

		public EDIMessageFlattenedCollection MessagesIncludingInterchangeRejections
		{
			get { return fMessagesIncludingInterchangeRejections ?? (fMessagesIncludingInterchangeRejections = Messages.MessagesIncludingInterchangeRejections(ZString.Empty)); }
		}
		EDIMessageFlattenedCollection fMessagesIncludingInterchangeRejections;

		public virtual bool HasCustomsMessages
		{
			get { return Messages.Count != 0; }
		}

		#endregion

		#endregion

		#region Public Methods

		public void RoundCustomsValueForAllMergedLines()
		{
			foreach (CusEntryHeader header in ActiveEntryHeaders)
			{
				foreach (CusEntryLine line in header.MergedLines)
				{
					line.RoundCustomsValue();
				}
			}
		}

		public virtual void DefaultIncoTerm()
		{
			var inco = GetDefaultINCOFromSupplierBuyerLink();
			if (!inco.IsEmpty)
			{
				JE_ShipmentIncoTerm = inco;
			}
		}

		public ZString GetDefaultINCOFromSupplierBuyerLink()
		{
			return OrgSupplierBuyerLink.GetDefaultINCO(Consignor, Consignee, JE_RL_NKFinalDestination.Left(2), TransportMode, ContainerMode);
		}

		public void DeriveDeclarationStatus()
		{
			DeriveDeclarationStatusCore();
		}

		protected virtual void DeriveDeclarationStatusCore()
		{
			if (IsImport)
			{
				DeriveImportDeclarationStatus();
			}
			else if (IsExport)
			{
				DeriveExportDeclarationStatus();
			}
		}

		protected virtual void DeriveImportDeclarationStatus()
		{
		}

		protected virtual void DeriveExportDeclarationStatus()
		{
		}

		public void ThrowAwayMerge()
		{
			try
			{
				IsThrowingAwayMerge = true;
				ThrowAwayMergeCore();
			}
			finally
			{
				IsThrowingAwayMerge = false;
			}
		}
		public virtual bool IsThrowingAwayMerge { get; private set; }

		protected virtual void ThrowAwayMergeCore()
		{
			CustomsEntryHeaders.Load();

			SetMessageParentToJobDeclaration();
			DiscardedMessages.Load();
			RemoveAndDeleteAllRequiredEntryHeaders();
			CancelCustomsEvents();
			DeriveDeclarationStatus();
			SetReadOnlyIncludingChildren(false);
			if (Shipment != null)
			{
				Shipment.ResetCusEntryNumbers();
			}
			PopulateEntrySubmittedDate(ZDateTime.Empty);
		}

		protected virtual void RemoveAndDeleteAllRequiredEntryHeaders()
		{
			CustomsEntryHeaders.RemoveAndDeleteAll();
		}

		protected EDIMessageCollection fDiscardedMessages;
		public EDIMessageCollection DiscardedMessages
		{
			get
			{
				if (fDiscardedMessages == null)
				{
					fDiscardedMessages = new EDIMessageCollection(this, GetDiscardedMessagesFilter());
					fDiscardedMessages.Load();
					fDiscardedMessages.IsManagedForDataRefresh = true;
				}
				return fDiscardedMessages;
			}
		}

		protected virtual ZQuery GetDiscardedMessagesFilter()
		{
			return null;
		}

#if DEBUG
		public
#else
		protected
#endif
 void SetMessageParentToJobDeclaration()
		{
			foreach (CusEntryHeader entryHeader in CustomsEntryHeaders)
			{
				EDIMessage[] entryHeaderMessages = (EDIMessage[])entryHeader.Messages.ToArray(typeof(EDIMessage));
				foreach (EDIMessage message in entryHeaderMessages)
				{
					entryHeader.Messages.Remove(message);
					Messages.Add(message);
					message.EM_Status = EDIMessage.Status.Discarded;
				}
			}

			JE_ConsolidatedCargoStatus = ZString.Empty;
			foreach (BasePackingGroup pack in PackingGroups)
			{
				pack.CR_CargoStatus = ZString.Empty;
				EDIMessage[] packingGroupMessages = (EDIMessage[])pack.Messages.ToArray(typeof(EDIMessage));
				foreach (EDIMessage message in packingGroupMessages)
				{
					pack.Messages.Remove(message);
					Messages.Add(message);
					message.EM_Status = EDIMessage.Status.Discarded;
				}
			}
		}

		protected MergeManager fMergeManager;
		public virtual MergeManager MergeManager
		{
			get
			{
				if (fMergeManager == null)
				{
					fMergeManager = GetMergeManager();
				}
				return fMergeManager;
			}
		}

		protected virtual MergeManager GetMergeManager()
		{
			return new MergeManager(this);
		}

		protected override IDisposable GetValidationDataSuspender()
		{
			return MergeManager.SuspendRequiresMergeCalculation();
		}

		public bool DoMerge()
		{
			return DoMerge(MessageInitiator);
		}

		public bool DoMerge(ISendsMessagesToCustoms notifier)
		{
			bool success = IsDeclarationIntegrated || DoMergeWithMutex(notifier);
			if (success)
			{
				OnSuccessfulMerge();
			}

			return success;
		}

		public virtual bool IsMergeDone
		{
			get { return CustomsEntryHeaders.Count != 0; }
		}

		public bool IsMergeInProgress
		{
			get { return fIsMergeInProgress; }
			protected internal set { fIsMergeInProgress = value; }
		}
		bool fIsMergeInProgress;

		bool DoMergeWithMutex(ISendsMessagesToCustoms notifier)
		{
			var result = false;
			try
			{
				if (LockDoMergeMutex())
				{
					if (HasEntryCreatedByOthers())
					{
						notifier.NotifyUserOfAnInvalidOperation(Res.GetString("4251E6D2-4EBD-49DF-8B54-A3077953BA9D", "Another user has created new entry details.\r\nPlease reload the data before trying merging again."));
					}
					else
					{
						result = DoMergeCore(notifier);
					}
				}
				else
				{
					notifier.NotifyUserOfAnInvalidOperation(Res.GetString("41A0CCF7-4D7D-4B40-A12E-96D88D3EF549", "{0} is in the process of merging this job; system cannot merge this data as it will result in a different entry details.\r\nPlease retry merging when the other user has finished.", GetDoMergeMutexLockInfo()));
				}
			}
			finally
			{
#if DEBUG
				if (NeedUnlockAfterMergeForTestHelper.NeedUnlockAfterMergeForTest)
				{
					UnlockDoMergeMutex();
				}
#endif
			}

			return result;
		}

		bool HasEntryCreatedByOthers()
		{
			var result = false;
			if (IsInDatabase)
			{
				var query = new ZQuery(CusEntryHeaderSchema.CH_JE, PK);
				query.AddToFilter(CusEntryHeaderSchema.PK, SQLComparisonOperator.NotEqual, CustomsEntryHeaders.GetPKs().Concat(DeletedEntryHeaderPKsInDatabase).Distinct());
				result = Factory.ExistsInDatabase(CusEntryHeader.Schema.TableName, query);
			}
			return result;
		}

		public bool HasFetchForLoadChildEditableObjectsBeenCalled { get; set; }

		internal List<ZGuid> DeletedEntryHeaderPKsInDatabase => fDeletedEntryHeaderPKsInDatabase ?? (fDeletedEntryHeaderPKsInDatabase = new List<ZGuid>());
		List<ZGuid> fDeletedEntryHeaderPKsInDatabase;

		protected virtual bool DoMergeCore(ISendsMessagesToCustoms notifier)
		{
			bool userChooseToContinueOrNoSeriousError = MergeManager.Execute(notifier);
			return userChooseToContinueOrNoSeriousError && MergeManager.MergeSuccessful;
		}

		public delegate void MergedSuccessfullyHandler();
		public event MergedSuccessfullyHandler MergedSuccessfully;

		protected virtual void OnSuccessfulMerge()
		{
			MergedSuccessfully?.Invoke();
			if (IsAutoUpdateBondedWarehouseEnabled)
			{
				Factory.Saved -= Factory_Saved;
				Factory.Saved += Factory_Saved;
			}
		}

#if DEBUG
		protected virtual
#endif
		void Factory_Saved(BusinessObjectFactory factory, bool savedSuccessfully)
		{
			factory.Saved -= Factory_Saved;
			if (savedSuccessfully)
			{
				var newFactory = new BusinessObjectFactory();
				var declaration = newFactory.Load<BaseJobDeclaration>(PK);
				if (declaration != null)
				{
					var notification = new ZStringBuilder();
					var entries = declaration.ActiveEntryHeaders.OfType<CusEntryHeader>().Where(x => x.CH_HasManualWhsUpdate).ToArray();
					for (int i = 0; i < entries.Length; i++)
					{
						var entry = entries[i];
						var errorMessage = entry.GetMessageErrorOfRequiredFieldsForBondedWarehousing();
						var entryInfo = Res.GetString("{1D346ABB-E4D2-42D8-B868-972F7420DDC0}", "Entry ({0}): ", entry.EntryHeaderDescriptiveMenuItemText);
						if (errorMessage.IsEmpty)
						{
							IWarehouseIntegrationSupporter supporter = entry;
							var warehouseTransactionStatus = entry.CH_WarehouseTransactionStatus;
							if (WarehouseTransactionStatusList.IsInwardCode(warehouseTransactionStatus))
							{
								var result = supporter.PublishShipmentForWHSInward(false);
								if (result != null)
								{
									if (result.ResultType == UniversalResult.HadErrors)
									{
										notification.AppendLine(entryInfo + result.ErrorMessage);
									}
									else
									{
										var warehouseJob = result.FindJobIfExists() as IRelatedJob;
										result = supporter.PublishAcceptEventForWHSInwardAndSaveIfNeeded(true);
										notification.AppendLine(entryInfo + Res.GetString("{C47C0D55-2730-4D18-8DC5-C922F2BB7BB3}", "Stock Levels have been updated.{0}", warehouseJob == null ? "" : string.Format(Culture.Invariant, (NoResString)" (WHS Receipt: {0})", warehouseJob.JobNumber)));
									}
								}
							}
							else if (WarehouseTransactionStatusList.IsOutwardCode(warehouseTransactionStatus))
							{
								var result = supporter.PublishShipmentForWHSOutward(true);
								if (result != null)
								{
									if (result.ResultType == UniversalResult.HadErrors)
									{
										notification.AppendLine(entryInfo + result.ErrorMessage);
									}
									else
									{
										var warehouseJob = result.FindJobIfExists() as IRelatedJob;
										result = supporter.PublishAcceptEventForWHSOutwardAndSaveIfNeeded(true);
										notification.AppendLine(entryInfo + Res.GetString("{73E0E14D-32AB-4300-9493-B051FAA83F11}", "Stock Release has been updated.{0}", warehouseJob == null ? "" : string.Format(Culture.Invariant, (NoResString)" (WHS Order: {0})", warehouseJob.JobNumber)));
									}
								}
							}
						}
						else
						{
							notification.AppendLine(entryInfo + errorMessage);
						}
					}

					if (!notification.IsEmpty)
					{
						MessageInitiator.NotifyUserOfASuccessfulSend(notification.ToString());
					}
				}
			}
		}

		/// <summary>
		/// To make merge work for applications for which users dont have to enter invoice lines
		/// eg. AU SAC without lines, US Inbond without lines
		/// </summary>
		public virtual bool ShouldCreateDummyInvoiceLinesForMerge
		{
			get { return false; }
		}

		public bool ShouldSynchroniseWithShipment()
		{
			return !JE_OverrideFreightDefaults && Shipment != null && !DeclarationMessagesHaveBeenSent();
		}

		public void SetShouldOverrideNotes(bool newValue)
		{
			shouldOverrideNotes = newValue;
		}
		protected bool shouldOverrideNotes;

		public bool DeclarationMessagesHaveBeenSent(bool reloadMessages = false)
		{
			return !IsDeclarationIntegrated && DeclarationMessagesHaveBeenSentCore(reloadMessages);
		}

		protected virtual bool DeclarationMessagesHaveBeenSentCore(bool reloadMessages)
		{
			if (reloadMessages)
			{
				Messages.Reload(true);
			}

			if (Messages.HasNonDiscardedMessage())
			{
				return true;
			}
			foreach (CusEntryHeader entryHeader in CustomsEntryHeaders)
			{
				if (reloadMessages)
				{
					entryHeader.Messages.Reload(true);
				}

				if (entryHeader.Messages.HasNonDiscardedMessage())
				{
					return true;
				}
			}
			return false;
		}

		public OrgAddress PhysicalAddressForExporter
		{
			get { return Supplier?.MainAddress; }
		}

		public OrgAddress PhysicalAddressForImporter
		{
			get { return Importer?.MainAddress; }
		}

		public void SelectContainerForAllInvoiceLines(NonPersistentCusContainer containerToSelect)
		{
			var container = containerToSelect.Container;
			if (container != null)
			{
				foreach (BaseJobComInvoiceLine invoiceLine in InvoiceLines)
				{
					invoiceLine.ToggleLinkageWithContainer(container, true);
				}
			}
		}

		public void UnSelectContainerForAllInvoiceLines(NonPersistentCusContainer containerToUnSelect)
		{
			var container = containerToUnSelect.Container;
			if (container != null)
			{
				foreach (BaseJobComInvoiceLine invoiceLine in InvoiceLines)
				{
					invoiceLine.ToggleLinkageWithContainer(container, false);
				}
			}
		}

		#region NonLinkedToContainerInvoiceLines

		public void MarkContainersAsIsForInvoiceLine()
		{
			foreach (BaseJobComInvoiceLine line in nonLinkedToContainerInvoiceLines)
			{
				if (line.ContainersForInvoiceLinesForBindingOnly.Count > 0)
				{
					line.ContainersForInvoiceLinesForBindingOnly[0].IsForInvoiceLine = true;
				}
			}
		}

		public ZBool ShouldShowInvoiceLinesAreNotLinkedToTheContainerDialog
		{
			get
			{
				bool result = false;

				if (InvoiceLines.Count > 0 && InvoiceLines[0].IsContainerLinkMandatory && CusContainers.Count == 1)
				{
					nonLinkedToContainerInvoiceLines = GetNonLinkedToContainerInvoiceLines();
					result = nonLinkedToContainerInvoiceLines.Length > 0;
				}

				return result;
			}
		}

		BaseJobComInvoiceLine[] nonLinkedToContainerInvoiceLines;

		BaseJobComInvoiceLine[] GetNonLinkedToContainerInvoiceLines()
		{
			List<BaseJobComInvoiceLine> result = new List<BaseJobComInvoiceLine>();

			foreach (BaseJobComInvoiceLine line in InvoiceLines)
			{
				if (line.IsContainerisedMode && line.IsNonContainerised)
				{
					result.Add(line);
				}
			}

			return result.ToArray();
		}

		#endregion

		public virtual void UpdateJE_ContainerCount(BaseCusContainer container, bool removed)
		{
			if (removed)
			{
				JE_ContainerCount = (ZShort)(CusContainers.Contains(container) ? CusContainers.Count - 1 : CusContainers.Count);
			}
			else
			{
				JE_ContainerCount = (ZShort)(CusContainers.Contains(container) ? CusContainers.Count : CusContainers.Count + 1);
			}
		}

		#region GetReasonForNotAbleToUpdate

		public string GetReasonForNotAbleToUpdate()
		{
			return GetReasonForNotAbleToUpdateCore();
		}

		protected virtual string GetReasonForNotAbleToUpdateCore()
		{
			var cannotUpdateAsMessageHasBeenSent = ResString.GetMultilingualString("b9b69b97-eeed-4633-bd2f-bd8176ffd1bd", "Message has been sent for this Declaration.");
			return DeclarationMessagesHaveBeenSent() ? string.Format(CultureInfo.InvariantCulture, (NoResString)"{0} Job Number: {1}", cannotUpdateAsMessageHasBeenSent, JobNumber) : string.Empty;
		}

		#endregion

		public ICodeDescriptionPairList GetBillTypeList()
		{
			return GetBillTypeListCore();
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Use for Key identifier")]
		protected virtual ICodeDescriptionPairList GetBillTypeListCore()
		{
			var key = GetCU_BillTypeListKey();
			return Factory.GetCachedValue("BaseBillCU_BillTypeList" + key, () =>
			{
				var result = new CodeDescriptionPairList();
				switch (key)
				{
					case "Post":
						result.AddPair(BillTypeList.Codes.HouseBill, BillTypeList.Codes.HouseBill);
						break;
					case "OtherWithoutSubBill":
						result.AddRange(Factory.GetCachedValue<BillTypeList>());
						result.RemoveCode(BillTypeList.Codes.SubHouseBill);
						break;
					default:
						result.AddRange(Factory.GetCachedValue<BillTypeList>());
						break;
				}
				return result;
			});
		}

		string GetCU_BillTypeListKey()
		{
			string result;
			if (IsPost)
			{
				result = (NoResString)"Post";
			}
			else if (!ShouldIncludeSubBillInBillTypeList)
			{
				result = "OtherWithoutSubBill";
			}
			else
			{
				result = (NoResString)"Other";
			}

			return result;
		}

		protected virtual bool ShouldIncludeSubBillInBillTypeList
		{
			get { return false; }
		}

		public LinkedToDeclarationData GetNewLinkedToDeclarationData()
		{
			return GetNewLinkedToDeclarationDataCore();
		}

		protected virtual LinkedToDeclarationData GetNewLinkedToDeclarationDataCore()
		{
			return new LinkedToDeclarationData(this);
		}

		public ZString GetContainerModeForDeclaration(ZString transportMode, ZString shipmentPackingMode)
		{
			return GetContainerModeForDeclarationCore(transportMode, shipmentPackingMode);
		}

		protected virtual ZString GetContainerModeForDeclarationCore(ZString transportMode, ZString shipmentPackingMode)
		{
			var result = ZString.Empty;

			if (shipmentPackingMode == Enterprise.Core.Constants.ContainerModes.ShippersConsol)
			{
				result = Core.Constants.ContainerModes.Containerised;
			}
			else if (transportMode == Core.Constants.TransportModes.Sea || transportMode == Core.Constants.TransportModes.SeaAir)
			{
				if (IsImport)
				{
					result = GetContainerModeForImportDeclaration(shipmentPackingMode);
				}
				else
				{
					if (shipmentPackingMode == Enterprise.Core.Constants.ContainerModes.FCL)
					{
						result = Core.Constants.ContainerModes.Containerised;
					}
					else if (shipmentPackingMode == Enterprise.Core.Constants.ContainerModes.LCL || shipmentPackingMode == Enterprise.Core.Constants.ContainerModes.BreakBulk)
					{
						result = Core.Constants.ContainerModes.NonContainerised;
					}
					else if (shipmentPackingMode == Enterprise.Core.Constants.ContainerModes.Bulk)
					{
						result = Core.Constants.ContainerModes.Bulk;
					}
					else if (shipmentPackingMode == Enterprise.Core.Constants.ContainerModes.Liquid)
					{
						result = Core.Constants.ContainerModes.Liquid;
					}
				}
			}
			else if (transportMode == Core.Constants.TransportModes.Air || transportMode == Core.Constants.TransportModes.AirSea)
			{
				result = Core.Constants.TransportModes.Air;
			}

			return result;
		}

		protected virtual ZString GetContainerModeForImportDeclaration(ZString shipmentPackingMode)
		{
			return GetContainerModeOfCNTorNCTFromShipmentMode(shipmentPackingMode);
		}

		protected ZString GetContainerModeOfCNTorNCTFromShipmentMode(ZString shipmentPackingMode)
		{
			var result = Core.Constants.ContainerModes.NonContainerised;

			if (shipmentPackingMode != Enterprise.Core.Constants.ContainerModes.Bulk &&
				shipmentPackingMode != Enterprise.Core.Constants.ContainerModes.Liquid &&
				shipmentPackingMode != Enterprise.Core.Constants.ContainerModes.BreakBulk &&
				shipmentPackingMode != Enterprise.Core.Constants.ContainerModes.RollOnRollOff)
			{
				result = Core.Constants.ContainerModes.Containerised;
			}

			return result;
		}

		#endregion

		#region Property Overrides

		public override ZString JE_DataModel
		{
			get { return base.JE_DataModel; }
			set
			{
				this.ReportDataModelErrorIfNeeded(JE_DataModelInfo, value);
				base.JE_DataModel = value;
			}
		}

		[DecimalPlaces(3)]
		[MeasureUnit(Schema.JE_TotalWeightUnit, MeasureUnitType.Weight)]
		public override ZDecimal JE_TotalWeight
		{
			get { return base.JE_TotalWeight; }
			set
			{
				ZDecimal oldValue = JE_TotalWeight;
				base.JE_TotalWeight = value;
				if (!IsCopying && oldValue != JE_TotalWeight)
				{
					ApportionInvoiceWeight(null);
					InvoiceLines.MarkAsNeedingValidation();
				}
			}
		}

		public ZDecimal TotalInvoiceLineGrossWeightInKG
		{
			get
			{
				if (totalInvoiceLineGrossWeightInKG == null)
				{
					totalInvoiceLineGrossWeightInKG = new CachedProperty<ZDecimal>(Factory, delegate
					{
						return InvoiceLines.Cast<BaseJobComInvoiceLine>().Sum(x => x.GrossWeightInKG);
					});
				}

				return totalInvoiceLineGrossWeightInKG.Value;
			}
		}
		CachedProperty<ZDecimal> totalInvoiceLineGrossWeightInKG;

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.WeightUnitList))]
		public override ZString JE_TotalWeightUnit
		{
			get { return base.JE_TotalWeightUnit; }
			set
			{
				ZString oldValue = JE_TotalWeightUnit;
				base.JE_TotalWeightUnit = value;
				if (!IsCopying && oldValue != JE_TotalWeightUnit)
				{
					ApportionInvoiceWeight(null);
					InvoiceLines.MarkAsNeedingValidation();
				}
			}
		}

		[DecimalPlaces(3)]
		[MeasureUnit(Schema.JE_TotalVolumeUnit, MeasureUnitType.Volume)]
		public override ZDecimal JE_TotalVolume
		{
			get { return base.JE_TotalVolume; }
			set { base.JE_TotalVolume = value; }
		}

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.VolumeUnitList))]
		public override ZString JE_TotalVolumeUnit
		{
			get { return base.JE_TotalVolumeUnit; }
			set { base.JE_TotalVolumeUnit = value; }
		}

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.PaymentPartyList))]
		public override ZString JE_PaymentMethod
		{
			get { return base.JE_PaymentMethod; }
			set { base.JE_PaymentMethod = value; }
		}

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.PaidByList))]
		public override ZString JE_PaidBy
		{
			get { return base.JE_PaidBy; }
			set { base.JE_PaidBy = value; }
		}

		void UpdateRoutingIfNoSpecialFlightTerm(ZPropertyInfo fieldChanged)
		{
			if (!HasSpecialFlightTerm)
			{
				UpdateRoutingIfRequired(fieldChanged);
			}
		}

		#region DateOfArrival

		public override ZDateTime JE_DateOfArrival
		{
			get { return base.JE_DateOfArrival; }
			set
			{
				bool hasChanged = JE_DateOfArrival != value;
				base.JE_DateOfArrival = value;
				if (hasChanged)
				{
					if (!IsCopying)
					{
						if (!fIsImportingData && !IsSettingDefaultValues)
						{
							CusContainers.MarkAsNeedingValidation();
						}

						UpdateRoutingIfNoSpecialFlightTerm(JE_DateOfArrivalInfo);
						UpdateETAAndThenDelivery();

						UpdateDateOfFirstArrivalIfPossible();
					}
				}
			}
		}

		public override ZDateTime JE_DateOfFirstArrival
		{
			get { return base.JE_DateOfFirstArrival; }
			set
			{
				bool hasChanged = JE_DateOfFirstArrival != value;
				base.JE_DateOfFirstArrival = value;
				if (hasChanged)
				{
					if (!IsCopying && !fIsImportingData && !IsSettingDefaultValues)
					{
						UpdateDateOfArrivalFromFirstArrivalIfPossible();
						InvoiceLines.MarkAsNeedingValidation();
					}
				}
			}
		}

		void UpdateDateOfArrivalFromFirstArrivalIfPossible()
		{
			if (!IsDataSyncFromShipment && JE_DateOfFirstArrival.IsValid)
			{
				if (JE_RL_NKPortOfFirstArrival == JE_RL_NKPortOfArrival && !JE_RL_NKPortOfArrival.IsEmpty && !JE_RL_NKPortOfFirstArrival.IsEmpty && JE_DateOfArrival != JE_DateOfFirstArrival)
				{
					JE_DateOfArrival = JE_DateOfFirstArrival;
				}
			}
		}

		protected void UpdateETAAndThenDelivery()
		{
			if (JE_JS.IsEmpty && JE_DateOfArrival.IsValid)
			{
				if (IsImport && DefaultPortDeliveryTime != null && !RoutingDefaultInProgress)
				{
					JE_DateAtFinalDestination = JE_DateOfArrival.AddDays(DefaultPortDeliveryTime.G1_DaysDelayFromArrivalToDeliver);
				}
				else
				{
					if (JE_DateAtFinalDestination.IsEmpty && JE_RL_NKFinalDestination == JE_RL_NKPortOfArrival || JE_DateOfArrival > JE_DateAtFinalDestination)
					{
						JE_DateAtFinalDestination = JE_DateOfArrival;
					}
				}
			}

			UpdateETDelivery();
		}

		internal void UpdateETADeliveryIfPortDeliveryTimeFound()
		{
			if (DefaultPortDeliveryTime != null)
			{
				UpdateETDelivery();
			}
		}

		protected void UpdateETDelivery()
		{
			var eTA = JE_DateAtFinalDestination;
			if (IsImport && JE_JS.IsEmpty && eTA.IsValid && !fIsImportingData && !IsSettingDefaultValues)
			{
				var defaultDeliverySet = FreightDataRegistry.Instance.DefaultDeliveryWhenDelayIsNotSet.GetFallBackValueAtAllLevels(RegistryCompanyPK, RegistryBranchPK, Guid.Empty);
				var deliveryTime = DefaultPortDeliveryTime;
				if (deliveryTime != null && (deliveryTime.G1_DaysFromDestinationArrivalToClientDelivery > 0 || defaultDeliverySet))
				{
					JE_EstimatedDeliveryOrPickup = eTA.AddDays(deliveryTime.G1_DaysFromDestinationArrivalToClientDelivery);
				}
			}
		}

		#endregion

		#region JE_MarksAndNumbers

		[MaxLength(20000)]
		public ZString JE_MarksAndNumbers
		{
			get
			{
				ZString result = "";
				StmNote note = MarksAndNumbersNote;
				if (note != null)
				{
					result = note.ST_NoteText;
				}
				return result;
			}
			set
			{
				StmNote newMarksAndNumbers = MarksAndNumbersNote;
				if (newMarksAndNumbers == null)
				{
					if (Shipment != null)
					{
						newMarksAndNumbers = Shipment.Notes.AddNew();
						newMarksAndNumbers.ST_ParentID = Shipment.PK;
						newMarksAndNumbers.ST_Table = Shipment.TableName;
					}
					else
					{
						newMarksAndNumbers = Notes.AddNew();
						newMarksAndNumbers.ST_ParentID = PK;
						newMarksAndNumbers.ST_Table = TableName;
					}
					newMarksAndNumbers.ST_Description = PredefinedNoteTypes.Instance.MarksAndNumbers.Description;
				}
				CheckMaximumLength(JE_MarksAndNumbersInfo, value);
				newMarksAndNumbers.ST_NoteText = value;
				JE_MarksAndNumbersInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo JE_MarksAndNumbersInfo
		{
			get { return GetZPropertyInfo(Schema.JE_MarksAndNumbers); }
		}

		protected StmNote MarksAndNumbersNote
		{
			get
			{
				StmNote[] marksAndNumbersNotes = NotesOfDeclarationOrShipment.FindByDescription(PredefinedNoteTypes.Instance.MarksAndNumbers.Description);
				return marksAndNumbersNotes.Length > 0 ? marksAndNumbersNotes[0] : null;
			}
		}

		#endregion

		#region JE_MarksAndNumbersShort

		#region Helper Functions

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

		/// <summary>
		/// First line or first 35 characters of Marks and Numbers Notes.
		/// Used for binding to text box.
		/// </summary>
		[MaxLength(35)]
		[ResourceStringData("Enterprise.Customs.Business.BaseJobDeclaration.JE_MarksAndNumbersShort", Caption = "Marks & Numbers")]
		public ZString JE_MarksAndNumbersShort
		{
			get
			{
				return GetShortText(JE_MarksAndNumbers);
			}
			set
			{
				if (value != JE_MarksAndNumbersShort)
				{
					CheckMaximumLength(JE_MarksAndNumbersShortInfo, value);
					JE_MarksAndNumbers = ReplaceFirstLine(JE_MarksAndNumbers, value);
					HasChanges = true;
				}
				JE_MarksAndNumbersShortInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo JE_MarksAndNumbersShortInfo
		{
			get { return GetZPropertyInfo(Schema.JE_MarksAndNumbersShort); }
		}

		#endregion

		#region JE_GoodsDescription

		public override ZString JE_GoodsDescription
		{
			get { return base.JE_GoodsDescription; }
			set
			{
				base.JE_GoodsDescription = value.SubstringSafe(0, JE_GoodsDescriptionInfo.MaxLength);
			}
		}

		#endregion

		#region JE_GoodsDescriptionDetailed
		public ZString JE_GoodsDescriptionDetailed
		{
			get
			{
				StmNote detailedGoodsDescriptionNote = GetDetailedGoodsDescriptionNote();
				return detailedGoodsDescriptionNote != null ? detailedGoodsDescriptionNote.ST_NoteText : JE_GoodsDescription;
			}
		}

		protected StmNote GetDetailedGoodsDescriptionNote()
		{
			StmNote[] detailedGoodsDescriptionNotes = NotesOfDeclarationOrShipment.FindByDescription(PredefinedNoteTypes.Instance.DetailedGoodsDescription.Description);
			return detailedGoodsDescriptionNotes.Length > 0 ? detailedGoodsDescriptionNotes[0] : null;
		}
		#endregion

		[ReadOnlyMember(nameof(JE_ContainerCount_ReadOnly))]
		public override ZShort JE_ContainerCount
		{
			get { return base.JE_ContainerCount; }
			set { base.JE_ContainerCount = value; }
		}

		protected virtual bool JE_ContainerCount_ReadOnly
		{
			get { return true; }
		}

		public bool CanReferenceNoBeReAssigned
		{
			get { return !ShouldDeclarationReferenceBePopulatedFromXml && (JE_DeclarationReference.IsEmpty || (!HasCustomsMessages && !HasJobInvoicing)) && !DeclarationReferenceOverriddenAndCannotBeChanged; }
		}

		protected virtual bool DeclarationReferenceOverriddenAndCannotBeChanged
		{
			get { return false; }
		}

		public bool ShouldDeclarationReferenceBePopulatedFromXml
		{
			get { return shouldDeclarationReferenceBePopulatedFromXml; }
			set { shouldDeclarationReferenceBePopulatedFromXml = value; }
		}
		bool shouldDeclarationReferenceBePopulatedFromXml;

		public virtual void PopulateJE_DeclarationReferenceIfNeeded()
		{
#if DEBUG
			if (DontReAssignReferenceNoForUnitTest)
			{
				return;
			}
#endif
			if (CanReferenceNoBeReAssigned)
			{
				if (JE_DeclarationReference.IsEmpty)
				{
					declarationReferenceAssignedFromNumberFountainOrShipment = true;
				}

				if (Shipment != null)
				{
					Shipment.PopulateBillAndShipmentNumberIfNeeded();
					JE_DeclarationReference = Shipment.JS_UniqueConsignRef;
				}
				else
				{
					PopulateNumberPropertyIfRequired(JE_DeclarationReferenceInfo, objectFactory => GetNewDeclarationReference(objectFactory), true);
				}
			}

#if DEBUG
			if (ForceExceptionAfterDeclarationReferenceAllocated)
			{
				throw new NotSupportedException("Crash Save");
			}
#endif
		}
		bool declarationReferenceAssignedFromNumberFountainOrShipment;

		#region IFountainResolver Members

		void IFountainResolver.TryToResolve()
		{
			using (var transactionManager = CargoWise.Data.Db.Connection.BeginTransactionWithManager())
			{
				GetNewDeclarationReference(Factory); // it will delete erroring number and jump to the next one, nice and sweet
				transactionManager.CommitTransaction();
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
					fUniqueIndexFailureHandler = new DeclarationNumberFountainUniqueIndexFailureHandler(this);
				}
				yield return fUniqueIndexFailureHandler;
			}
		}
		IUniqueIndexFailureHandler fUniqueIndexFailureHandler;

		protected class DeclarationNumberFountainUniqueIndexFailureHandler : NumberFountainUniqueIndexFailureHandler
		{
			public DeclarationNumberFountainUniqueIndexFailureHandler(BaseJobDeclaration declaration)
				: base(JobDeclarationSchema.Constants.Indexes.NR_UX__JE_DeclarationReference_JE_GC, declaration)
			{
				this.declaration = declaration;
			}
			readonly BaseJobDeclaration declaration;

			protected override INumberFountainProxy NumberFountainToFix
			{
				get
				{
					return this.declaration?.declarationReferenceHandler?.NumberFountain ?? this.declaration.NumberFountainForUniqueDeclarationReference;
				}
			}

			protected override CargoWise.Data.DbCommand CommandToFindMaxValueInDatabase(CargoWise.Data.DbConnection connection)
			{
				if (this.declaration.CanReferenceNoBeReAssigned && this.declaration.declarationReferenceHandler != null)
				{
					return this.declaration.declarationReferenceHandler.FindMaxValueInDatabase(connection, JobDeclarationSchema.JE_DeclarationReference);
				}
				return base.CommandToFindMaxValueInDatabase(connection);
			}
		}

		UniqueIndexHandler declarationReferenceHandler;

		protected virtual INumberFountainProxy NumberFountainForUniqueDeclarationReference
		{
			get { return IsWarehousedByExternalAgent ? Env.NumberFountains.CustomsJobNoByExternalAgent : Env.NumberFountains.CustomsJobNo; }
		}

		#endregion

		protected virtual ZString GetNewDeclarationReference(BusinessObjectFactory factory)
		{
			var declarationTarget = new DeclarationNumberGeneratorTarget();

			var generator = new NumberGenerator
			{
				Factory = factory,
				Context = GetNewNumberGeneratorContext(),
				BaseFountain = NumberFountainForUniqueDeclarationReference,
				FountainGetter = IsWarehousedByExternalAgent ? Env.NumberFountains.GetCustomsJobNoByExternalAgent : Env.NumberFountains.GetCustomsJobNoGeneratorFountain,
				PrimaryTarget = declarationTarget
			};
			generator.ValueProviders.AddRange(new StandardValueSource());
			generator.ValueProviders.AddRange(new FreightValueSource(this));

			generator.Generate();
			generator.EnforceMaxLengths();

			declarationReferenceHandler = new UniqueIndexHandler(declarationTarget);

			return declarationTarget.Value.ToUpper();
		}

		protected virtual NumberGeneratorContext GetNewNumberGeneratorContext()
		{
			return JE_GB.IsEmpty ? new NumberGeneratorContext() : new NumberGeneratorContext(GlbCompany.CurrentCompany.PK, JE_GB, GlbDepartment.CurrentDepartment.PK);
		}

#if DEBUG
		public bool DontReAssignReferenceNoForUnitTest;
		public bool ForceExceptionAfterDeclarationReferenceAllocated;
#endif

		public void PopulateJE_OwnerRefIfNeeded()
		{
			if (AutoAssignImporterRef)
			{
				PopulateNumberPropertyIfRequired(JE_OwnerRefInfo, objectFactory =>
				{
					ownerRefAssignedFromNumberFountain = true;
					return new ZString(Env.NumberFountains.ImporterJobReference(JE_OH_Importer.ToGuid()).GetNextFormatted(objectFactory));
				}, ignoreInDatabaseCheck: true);
			}
			else
			{
				ownerRefAssignedFromNumberFountain = false;
			}
		}

		bool ownerRefAssignedFromNumberFountain;

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.EFTModeList))]
		public override ZString JE_EFTMode
		{
			get { return base.JE_EFTMode; }
			set { base.JE_EFTMode = value; }
		}

		public void LogEventIfJE_EntryStatusChanged()
		{
			if (ShouldLogEventIfJE_EntryStatusChanged)
			{
				LogEventIfJE_EntryStatusChangedCore();
			}
		}

		protected internal virtual bool ShouldLogEventIfJE_EntryStatusChanged => true;

		protected virtual void LogEventIfJE_EntryStatusChangedCore()
		{
			var newEntryStatus = base.JE_EntryStatus;
			if ((ZString)JE_EntryStatusInfo.OriginalValue != newEntryStatus)
			{
				var cesEventToAdd = Events.CustomsEntryStatus;
				Func<StmALog, bool> cesCriteria = (StmALog log) => { return log.SL_SE_NKEvent == cesEventToAdd.Code && !log.IsInDatabase && log.SL_Reference == newEntryStatus; };
				var unsavedCESEvents = LogParent.Logs.Find(cesCriteria);
				if (LogParent != this)
				{
					var unsavedCESEventsOnDeclaration = Logs.Find(cesCriteria);
					if (unsavedCESEventsOnDeclaration != null)
					{
						unsavedCESEvents = unsavedCESEvents == null ? unsavedCESEventsOnDeclaration : unsavedCESEvents.Union(unsavedCESEventsOnDeclaration);
					}
				}
				if (unsavedCESEvents == null || !unsavedCESEvents.Any())
				{
					using (DisposableEnvironment.ForBranch(Branch.PK.ToGuid()))
					{
						Logs.AddNew(cesEventToAdd, newEntryStatus);
					}
				}
			}
		}

		public override ZString JE_RL_NKPortOfFirstArrival
		{
			get { return base.JE_RL_NKPortOfFirstArrival; }
			set
			{
				base.JE_RL_NKPortOfFirstArrival = value;
				if (!IsCopying && !IsDataSyncFromShipment)
				{
					UpdateDateOfFirstArrivalIfPossible();

					if (!JE_RL_NKPortOfFirstArrival.IsEmpty && IsFirstArrivalDateAndPortUsed)
					{
						DefaultArrivalAndLoadingDatesFromDestinationAndOrigin();
					}
				}
			}
		}

		public bool IsFirstArrivalDateAndPortUsed
		{
			get { return FirstArrivalDateAndPortUseDecider.IsUsed(CountryCode, JE_MessageType); }
		}

		protected virtual void UpdateDateOfFirstArrivalIfPossible()
		{
			if (!IsDataSyncFromShipment && JE_DateOfArrival.IsValid && IsFirstArrivalDateAndPortUsed)
			{
				if (JE_RL_NKPortOfFirstArrival == JE_RL_NKPortOfArrival && !JE_RL_NKPortOfArrival.IsEmpty && !JE_RL_NKPortOfFirstArrival.IsEmpty && JE_DateOfFirstArrival != JE_DateOfArrival)
				{
					JE_DateOfFirstArrival = JE_DateOfArrival;
				}
			}
		}

		public ZString ForwarderName
		{
			get { return Forwarder != null ? Forwarder.OH_FullNameTruncated : ZString.Empty; }
		}

		#region JobDocAddresses

		#region LocalClient
		public OrgAddress LocalClientAddress
		{
			get
			{
				if (localClientAddress == null)
				{
					localClientAddress = Job?.LocalChargesAddr;
				}
				return localClientAddress;
			}
		}
		OrgAddress localClientAddress;

		public OrgHeader LocalClient
		{
			get
			{
				if (localClient == null)
				{
					localClient = Job?.LocalCharges;
				}
				return localClient;
			}
		}
		OrgHeader localClient;

		[ResourceStringData("6C2A99AC-D093-4433-84DD-38F3C57C5BFF", Caption = "Local Client Code")]
		public ZString LocalClientCode => LocalClient?.OH_Code ?? ZString.Empty;

		[ResourceStringData("F97610F6-73CF-4A53-B0EC-C27C8925E808", Caption = "Local Client Name")]
		public ZString LocalClientName => LocalClient?.OH_FullName ?? ZString.Empty;

		[ResourceStringData("CDE30328-A038-4EBA-B70E-EB837DA96E31", Caption = "Local Client Address")]
		public ZString LocalClientAddressAsString => LocalClientAddress?.AddressAsASingleLine ?? ZString.Empty;

		[ResourceStringData("DD2A57A3-8226-4404-BC59-63DB11B239F3", Caption = "Local Client Address Short Code")]
		public ZString LocalClientAddressShortCode => LocalClientAddress?.OA_Code ?? ZString.Empty;

		[ResourceStringData("9C071A62-9160-40AF-BBD5-E3F3A94A1A84", Caption = "Local Client Address 1")]
		public ZString LocalClientAddress1 => LocalClientAddress?.Address1 ?? ZString.Empty;

		[ResourceStringData("651518D0-CB13-4647-B125-AA499FADE3FC", Caption = "Local Client Address 2")]
		public ZString LocalClientAddress2 => LocalClientAddress?.Address2 ?? ZString.Empty;

		[ResourceStringData("C48BC1D9-4C12-40D4-94FA-D17B0DC83970", Caption = "Local Client City")]
		public ZString LocalClientCity => LocalClientAddress?.CityFallback ?? ZString.Empty;

		[ResourceStringData("36C767DC-4025-4EC9-8CA4-8038A869A02B", Caption = "Local Client State")]
		public ZString LocalClientState => LocalClientAddress?.OA_State ?? ZString.Empty;

		[ResourceStringData("AA5C6F23-C7AD-428F-8F47-D781452E635F", Caption = "Local Client Country")]
		public ZString LocalClientCountry => LocalClientAddress?.RelatedCountry?.RN_Code ?? ZString.Empty;

		#endregion

		#region Importer
		public override ZGuid JE_OH_Importer
		{
			get { return base.JE_OH_Importer; }
			set
			{
				if (!IsJE_OH_ImporterSettingSuspended)
				{
					ZGuid oldValue = JE_OH_Importer;
					if (value != oldValue && !oldValue.IsEmpty && oldValue.IsValid && !IsCopying && !fIsImportingData)
					{
						UnhookFromMiscServUpdatedByDataRefresh(Importer);
					}
					SetJE_OH_ImporterBaseValueOnly(value);
					var newValue = JE_OH_Importer;
					if (!IsCopying && newValue != oldValue)
					{
						var isPersistent = IsPersistent;
						if (isPersistent)
						{
							JE_OH_ImporterChanged(oldValue, newValue);
						}

						if (UseImporterAddress && !IsImporterAddressDefaultingSuspended)
						{
							var importerAddressOrgPK = ImporterAddressOrgPK;
							if (importerAddressOrgPK != newValue)
							{
								using (SuspendImporterAddressDefaulting())
								{
									ImporterAddressOrgPK = newValue;
								}
							}
						}

						if (!IsSettingDefaultValues && IsImport)
						{
							this.DefaultExternalBroker(false, true);
							this.DefaultForwarder(false, true);
							this.DefaultDepot(false, true);
							this.DefaultWarehouseDocAddress(false, true);
						}

						if (isPersistent)
						{
							CusContainers.MarkAsNeedingValidation();
						}

						Invoices.MarkAsNeedingValidation();
						if (!fIsImportingData)
						{
							InvoiceLines.MarkAsNeedingValidationIncludingChildren();
						}
						JE_OwnerRefInfo.RefreshBinding();
						ScreeningStatusUpdater.SetShouldUpdateScreeningStatusByOrgHeader(this, JE_ScreeningStatus, Factory, JE_OH_Importer);
						MarkDocsAndCartageAsNeedingValidationIfNotSettingDefaultValues();
					}
				}

				FlushImporterDocumentaryAddressIfBlank(value);
				DocsAndCartage.RequiredDocuments.SetAllDocumentsReceivedEventLogger();
			}
		}

		protected void SetJE_OH_ImporterBaseValueOnly(ZGuid value) => base.JE_OH_Importer = value;

		protected bool IsJE_OH_ImporterSettingSuspended => isJE_OH_ImporterSettingSuspended != 0;

		public IDisposable SuspendJE_OH_ImporterSetting()
		{
			return new DisposableAction(() => isJE_OH_ImporterSettingSuspended++, () => isJE_OH_ImporterSettingSuspended--);
		}
		byte isJE_OH_ImporterSettingSuspended;

		protected bool IsImporterAddressDefaultingSuspended => suspendImporterAddressDefaulting != 0;

		protected IDisposable SuspendImporterAddressDefaulting()
		{
			return new DisposableAction(() => suspendImporterAddressDefaulting++, () => suspendImporterAddressDefaulting--);
		}
		byte suspendImporterAddressDefaulting;

		protected virtual void FlushImporterDocumentaryAddressIfBlank(ZGuid je_oh_importer)
		{
			if (je_oh_importer.IsEmpty && fImporterDocumentaryAddress != null && !JE_JS.IsEmpty)
			{
				fImporterDocumentaryAddress.Delete();
				fImporterDocumentaryAddress = null;
			}
		}

		public virtual ZString ImporterName
		{
			get { return Importer != null ? Importer.OH_FullNameTruncated : ZString.Empty; }
		}

		protected virtual void JE_OH_ImporterChanged(ZGuid oldValue, ZGuid newValue)
		{
			if (!fIsImportingData)
			{
				DefaultImporterDocAddresses(newValue);
			}

			if (Importer != null)
			{
				if (!fIsImportingData)
				{
					DefaultMessageTypeFromSupplierOrImporter(MessageTypeDefaultingTriggerSource.Importer);
					DefaultCartageEquipment();
					DefaultFinalDestinationPortFromImporter();
					RefreshAllInvoiceLinesPartSyncManagers();
					SetDefaultCartageOrg();
				}

				DefaultIncoTerm();
				MessageTypeBasedValueDefaulter.DefaultValuesFromLocalParty();
				DefaultServiceLevelFromImporter();
			}
		}

		public void DefaultImporterDocAddresses(ZGuid newValue)
		{
			if (IsDefaultImporterDocAddressesEnabled)
			{
				if (!ImporterDocumentaryAddress.E2_AddressOverride)
				{
					ImporterDocumentaryAddress.OrganisationPK = newValue;
				}
				else if (!ImporterDeliveryAddress.E2_AddressOverride)
				{
					ImporterDeliveryAddress.OrganisationPK = newValue;
				}
			}
		}
		protected virtual bool IsDefaultImporterDocAddressesEnabled => true;

		internal void DefaultMessageTypeFromImporterUNLOCO(OrgHeader importer)
		{
			if (!importer.IsMiscellaneous && importer.UNLOCO != null)
			{
				if (IsImporterSupplierCountryCodeConsideredTheSameAsCurrentCompany(importer.UNLOCO.RL_RN_NKCountryCode))
				{
					if (!IsImport)
					{
						JE_MessageType = GetDefaultMessageType(true);
					}
				}
				else
				{
					var supplier = Supplier;
					if (supplier != null && !supplier.IsMiscellaneous && supplier.UNLOCO != null && !IsImporterSupplierCountryCodeConsideredTheSameAsCurrentCompany(supplier.UNLOCO.RL_RN_NKCountryCode))
					{
						JE_MessageType = GetDefaultMessageType(GlbDepartment.CurrentDepartment.GE_Import);
					}
					else if (!IsExport)
					{
						JE_MessageType = GetDefaultMessageType(false);
					}
				}
			}
		}

		bool AreImporterAndSupplierUnderTheSameCountryOfJurisdiction
		{
			get
			{
				var importer = Importer;
				var supplier = Supplier;
				return importer != null && supplier != null && importer.UNLOCO != null && supplier.UNLOCO != null
					&& Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(importer.UNLOCO.RL_RN_NKCountryCode) == Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(supplier.UNLOCO.RL_RN_NKCountryCode);
			}
		}

		bool IsImporterSupplierCountryCodeConsideredTheSameAsCurrentCompany(ZString countryCode)
		{
			var result = false;
			if (AreImporterAndSupplierUnderTheSameCountryOfJurisdiction)
			{
				result = countryCode == GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			}
			else
			{
				result = Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(countryCode) == Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			}
			return result;
		}

		#region ImporterDocAddresses
		public virtual JobDocAddress ImporterDocumentaryAddress
		{
			get
			{
				SetupImporterJobDocAddresses();
				return fImporterDocumentaryAddress;
			}
		}
		JobDocAddress fImporterDocumentaryAddress;

		public virtual JobDocAddress ImporterDeliveryAddress
		{
			get
			{
				SetupImporterJobDocAddresses();
				return fImporterDeliveryAddress;
			}
		}
		JobDocAddress fImporterDeliveryAddress;

		public bool IsImporterDeliveryAddressInitialised
		{
			get { return fImporterDeliveryAddress != null; }
		}

		protected void SetupImporterJobDocAddresses()
		{
			if (fImporterDocumentaryAddress == null || fImporterDocumentaryAddress.IsDeleted)
			{
				ImporterDocAddressRequirement = AddImporterDocAddressRequirement();
				fImporterDocumentaryAddress = DocAddresses.FindOrCreateWithRequirement(ImporterDocAddressRequirement);
				if (fImporterDocumentaryAddress != null)
				{
					SetupImporterDocumentaryAddress(fImporterDocumentaryAddress);
				}
			}

			if (fImporterDeliveryAddress == null || fImporterDeliveryAddress.IsDeleted)
			{
				if (Shipment != null && Shipment.ConsigneeDeliveryAddress != null)
				{
					fImporterDeliveryAddress = Shipment.ConsigneeDeliveryAddress;
					fImporterDeliveryAddress.AdditionalValidation = PiggyBackedDocAddressValidation(fImporterDeliveryAddress);
				}
				else
				{
					fImporterDeliveryAddress = GetImporterDeliveryAddress();
				}
				if (fImporterDeliveryAddress != null)
				{
					currentImporterDeliveryAddressE2_OA_Address = fImporterDeliveryAddress.E2_OA_Address;
					fImporterDeliveryAddress.DocAddressChanged += new EventHandler(ImporterDeliveryAddressChanged);
				}
			}
		}

		protected virtual JobDocAddress GetImporterDeliveryAddress() => DocAddresses.FindOrCreateWithDocAddressType(ImporterDocAddressRequirement.SupportedDocAddressTypes[0]);

		protected virtual void SetupImporterDocumentaryAddress(JobDocAddress importerDocumentaryAddress)
		{
			importerDocumentaryAddress.DocAddressChanged += new EventHandler(ImporterDocumentaryAddressChanged);
		}

		protected virtual void ImporterDocumentaryAddressChanged(object sender, EventArgs e)
		{
		}

		void ImporterDeliveryAddressChanged(object sender, EventArgs e)
		{
			if (Shipment != null && !Shipment.DeliveryAddressHasBeenChangedByShipmentUser)
			{
				HasChanges = true;
			}

			ZGuid newImporterDeliveryAddressE2_OA_Address = fImporterDeliveryAddress.E2_OA_Address;

			DefaultCartageCompany(newImporterDeliveryAddressE2_OA_Address, RelatedPartyDirectionList.Codes.Delivery, TransportMode);

			if (newImporterDeliveryAddressE2_OA_Address != currentImporterDeliveryAddressE2_OA_Address)
			{
				ImporterDeliveryAddressChanged(currentImporterDeliveryAddressE2_OA_Address, newImporterDeliveryAddressE2_OA_Address);
				currentImporterDeliveryAddressE2_OA_Address = newImporterDeliveryAddressE2_OA_Address;
				SetDefaultCartageOrg();
			}
			DefaultCartageEquipment();
		}
		ZGuid currentImporterDeliveryAddressE2_OA_Address;

		protected virtual void ImporterDeliveryAddressChanged(ZGuid oldAddressPK, ZGuid newAddressPK)
		{
		}

		void DefaultCartageCompany(ZGuid newAddressPK, ZString direction, ZString transportMode)
		{
			if (Shipment == null) //when shipment's importer and supplier are entered, system would already have defaulted this
			{
				OrgAddress newAddress = Factory.Load<OrgAddress>(newAddressPK);
				if (newAddress != null && newAddress.Header != null)
				{
					var containerMode = ZString.Empty;
					if (transportMode == Constants.TransportModes.Sea)
					{
						if (JE_ContainerMode == Constants.ContainerModes.FCL || JE_ContainerMode == Constants.ContainerModes.LCL)
						{
							containerMode = JE_ContainerMode;
						}
						else if (JE_ContainerMode == Core.Constants.ContainerModes.FCLMixedShipper)
						{
							containerMode = Constants.ContainerModes.FCL;
						}
					}

					var relatedParty = newAddress.Header.AllRelatedParties.GetRelatedParty(newAddressPK, RelatedPartyTypeList.Codes.LocalTransport, direction, transportMode, containerMode, newAddress.RelatedPortCode?.Code ?? ZString.Empty);
					if (relatedParty != null && relatedParty.RelatedParty != null)
					{
						JE_OA_DeliveryOrPickupCartageCoAddr = relatedParty.RelatedParty.MainAddress.PK;
					}
					else if (transportMode != Constants.TransportModes.All)
					{
						relatedParty = newAddress.Header.AllRelatedParties.GetRelatedParty(newAddressPK, RelatedPartyTypeList.Codes.LocalTransport, direction, Constants.TransportModes.All, ZString.Empty, newAddress.RelatedPortCode?.Code ?? ZString.Empty);
						if (relatedParty != null && relatedParty.RelatedParty != null)
						{
							JE_OA_DeliveryOrPickupCartageCoAddr = relatedParty.RelatedParty.MainAddress.PK;
						}
					}
				}
			}
		}

		public virtual bool IsNotificationRequiredIfDeliveryAddressChangedByFreight
		{
			get { return false; }
		}

		public virtual string AdditionalMailTextWhenDeliveryAddressChangedByFreight
		{
			get { return ""; }
		}

		public virtual string[] MailRecipentsWhenDeliveryAddressChangedByFreight
		{
			get { return Array.Empty<string>(); }
		}
		#endregion

		protected void DefaultServiceLevelFromImporter()
		{
			if (Importer is OrgHeader importer
				&& importer.MiscServ is OrgMiscServ miscServ
				&& !importer.IsMiscellaneous
				&& !miscServ.OM_RS_NKIMDefaultServiceLevel.IsEmpty
				&& Shipment == null //value comes from shipment if there is a shipment
				&&
				(JE_RS_NKServiceLevel.IsEmpty || DefaultServiceLevel != null && JE_RS_NKServiceLevel == DefaultServiceLevel.RS_Code))
			{
				JE_RS_NKServiceLevel = miscServ.OM_RS_NKIMDefaultServiceLevel;
			}
		}

		#endregion

		#region Supplier

		public override ZGuid JE_OH_Supplier
		{
			get { return base.JE_OH_Supplier; }
			set
			{
				if (!IsJE_OH_SupplierSettingSuspended)
				{
					var oldValue = JE_OH_Supplier;
					SetJE_OH_SupplierBaseValueOnly(value);
					var newValue = JE_OH_Supplier;
					if (!IsCopying && newValue != oldValue)
					{
						JE_OH_SupplierChanged(oldValue, newValue);
						if (UseSupplierAddress && !IsSupplierAddressDefaultingSuspended)
						{
							var supplierAddressOrgPK = SupplierAddressOrgPK;
							if (supplierAddressOrgPK != newValue)
							{
								using (SuspendSupplierAddressDefaulting())
								{
									SupplierAddressOrgPK = newValue;
								}
							}
						}
						MarkInvoiceLinesAsNeedingValidationOnJE_OH_SupplierChanged();
						Invoices.MarkAsNeedingValidation();
						CusContainers.MarkAsNeedingValidation();
						ScreeningStatusUpdater.SetShouldUpdateScreeningStatusByOrgHeader(this, JE_ScreeningStatus, Factory, JE_OH_Supplier);
						MarkDocsAndCartageAsNeedingValidationIfNotSettingDefaultValues();
						if (!IsSettingDefaultValues && IsExport)
						{
							this.DefaultExternalBroker(true, false);
							this.DefaultForwarder(true, false);
							this.DefaultDepot(true, false);
							this.DefaultWarehouseDocAddress(true, false);
						}
					}
					FlushSupplierDocumentaryAddressIfBlank(value);
					DocsAndCartage.RequiredDocuments.SetAllDocumentsReceivedEventLogger();
				}
			}
		}

		protected void SetJE_OH_SupplierBaseValueOnly(ZGuid value) => base.JE_OH_Supplier = value;

		protected bool IsJE_OH_SupplierSettingSuspended => isJE_OH_SupplierSettingSuspended != 0;

		public IDisposable SuspendJE_OH_SupplierSetting()
		{
			return new DisposableAction(() => isJE_OH_SupplierSettingSuspended++, () => isJE_OH_SupplierSettingSuspended--);
		}
		byte isJE_OH_SupplierSettingSuspended;

		protected virtual void MarkInvoiceLinesAsNeedingValidationOnJE_OH_SupplierChanged()
		{
			if (IsExport)
			{
				InvoiceLines.MarkAsNeedingValidation();
			}
		}

		protected bool IsSupplierAddressDefaultingSuspended => suspendSupplierAddressDefaulting != 0;

		protected IDisposable SuspendSupplierAddressDefaulting()
		{
			return new DisposableAction(() => suspendSupplierAddressDefaulting++, () => suspendSupplierAddressDefaulting--);
		}
		byte suspendSupplierAddressDefaulting;

		protected virtual void FlushSupplierDocumentaryAddressIfBlank(ZGuid je_oh_supplier)
		{
			if (je_oh_supplier.IsEmpty && fSupplierDocumentaryAddress != null && !JE_JS.IsEmpty)
			{
				fSupplierDocumentaryAddress.Delete();
				fSupplierDocumentaryAddress = null;
			}
		}

		public virtual ZString SupplierName
		{
			get { return Supplier != null ? Supplier.OH_FullNameTruncated : ZString.Empty; }
		}

		protected virtual void JE_OH_SupplierChanged(ZGuid oldValue, ZGuid newValue)
		{
			if (ShouldSetDefaultValuesFromSupplier)
			{
				if (!fIsImportingData)
				{
					DefaultSupplierDocAddresses(newValue);
				}

				if (Supplier != null)
				{
					if (!fIsImportingData)
					{
						DefaultMessageTypeFromSupplierOrImporter(MessageTypeDefaultingTriggerSource.Supplier);
						DefaultCartageEquipment();
						DefaultOriginFromSupplier();
						SetDefaultCartageOrg();
					}
					DefaultIncoTerm();
					MessageTypeBasedValueDefaulter.DefaultValuesFromLocalParty();
				}
			}
		}

		protected virtual ZBool ShouldSetDefaultValuesFromSupplier
		{
			get { return true; }
		}

		public void DefaultSupplierDocAddresses(ZGuid newValue)
		{
			if (IsDefaultSupplierDocAddressesEnabled)
			{
				if (!SupplierDocumentaryAddress.E2_AddressOverride)
				{
					SupplierDocumentaryAddress.OrganisationPK = newValue;
				}
				else if (IsDefaultSupplierPickupAddressEnabled && !SupplierPickupAddress.E2_AddressOverride)
				{
					SupplierPickupAddress.OrganisationPK = newValue;
				}
			}
		}
		protected virtual bool IsDefaultSupplierDocAddressesEnabled => true;

		protected virtual bool IsDefaultSupplierPickupAddressEnabled => true;

		public enum MessageTypeDefaultingTriggerSource { Supplier, Importer }

#if DEBUG
		public bool DisableMessageTypeChangeOnSupplierChangeForTesting;
#endif

		protected virtual void DefaultMessageTypeFromSupplierOrImporter(MessageTypeDefaultingTriggerSource source)
		{
#if DEBUG
			if (DisableMessageTypeChangeOnSupplierChangeForTesting)
			{
				return;
			}
#endif
			if (!JE_MessageTypeInfo.ReadOnly && !IsNonTransportDeclarationType)
			{
				if (source == MessageTypeDefaultingTriggerSource.Supplier)
				{
					DefaultMessageTypeFromSupplierUNLOCO(Supplier);
				}
				else if (source == MessageTypeDefaultingTriggerSource.Importer)
				{
					DefaultMessageTypeFromImporterUNLOCO(Importer);
				}
			}
		}

		internal void DefaultMessageTypeFromSupplierUNLOCO(OrgHeader supplier)
		{
			if (!supplier.IsMiscellaneous && supplier.UNLOCO != null)
			{
				if (IsImporterSupplierCountryCodeConsideredTheSameAsCurrentCompany(supplier.UNLOCO.RL_RN_NKCountryCode))
				{
					if (!IsExport)
					{
						JE_MessageType = GetDefaultMessageType(false);
					}
				}
				else
				{
					var importer = Importer;
					if (importer != null && !importer.IsMiscellaneous && importer.UNLOCO != null && !IsImporterSupplierCountryCodeConsideredTheSameAsCurrentCompany(importer.UNLOCO.RL_RN_NKCountryCode))
					{
						JE_MessageType = GetDefaultMessageType(GlbDepartment.CurrentDepartment.GE_Import);
					}
					else if (!IsImport)
					{
						JE_MessageType = GetDefaultMessageType(true);
					}
				}
			}
		}

		void DefaultMessageTypeFromOriginOrDestination()
		{
			if (IsDataSyncFromShipment && Shipment.JS_INCO == Core.Constants.IncoTerms.DeliveredDutyPaid)
			{
				var isImport = ImportExportHelper.IsImport(JE_RL_NKOrigin, JE_RL_NKFinalDestination);
				JE_MessageType = GetDefaultMessageType(isImport);
			}
		}

		public virtual JobDocAddress SupplierDocumentaryAddress
		{
			get
			{
				SetupSupplierJobDocAddresses();
				return fSupplierDocumentaryAddress;
			}
		}
		JobDocAddress fSupplierDocumentaryAddress;

		public JobDocAddress SupplierPickupAddress
		{
			get
			{
				SetupSupplierJobDocAddresses();
				return fSupplierPickupAddress;
			}
		}
		JobDocAddress fSupplierPickupAddress;

		public bool IsSupplierPickupAddressInitialised
		{
			get { return fSupplierPickupAddress != null; }
		}

		void SetupSupplierJobDocAddresses()
		{
			if (fSupplierDocumentaryAddress == null || fSupplierDocumentaryAddress.IsDeleted)
			{
				SupplierDocAddressRequirement = AddSupplierDocAddressRequirement();
				fSupplierDocumentaryAddress = DocAddresses.FindOrCreateWithRequirement(SupplierDocAddressRequirement);
				if (fSupplierDocumentaryAddress != null)
				{
					SetupSupplierDocumentaryAddress(fSupplierDocumentaryAddress);
				}
			}
			if (fSupplierPickupAddress == null || fSupplierPickupAddress.IsDeleted)
			{
				if (Shipment != null && Shipment.ConsignorPickupAddress != null)
				{
					fSupplierPickupAddress = Shipment.ConsignorPickupAddress;
				}
				else
				{
					fSupplierPickupAddress = GetSupplierPickupAddress();
				}
				if (fSupplierPickupAddress != null)
				{
					CurrentSupplierPickupAddressE2_OA_Address = fSupplierPickupAddress.E2_OA_Address;
					fSupplierPickupAddress.DocAddressChanged += new EventHandler(SupplierPickupAddressChanged);
				}
			}
		}

		protected virtual JobDocAddress GetSupplierPickupAddress() => DocAddresses.FindOrCreateWithDocAddressType(SupplierDocAddressRequirement.SupportedDocAddressTypes[0]);

		protected virtual void SetupSupplierDocumentaryAddress(JobDocAddress supplierDocumentaryAddress)
		{
			supplierDocumentaryAddress.DocAddressChanged += new EventHandler(SupplierDocumentaryAddressChanged);
		}

		protected virtual void SupplierDocumentaryAddressChanged(object sender, EventArgs e)
		{
		}

		void SupplierPickupAddressChanged(object sender, EventArgs e)
		{
			ZGuid newSupplierPickupAddressE2_OA_Address = fSupplierPickupAddress.E2_OA_Address;

			DefaultCartageCompany(newSupplierPickupAddressE2_OA_Address, RelatedPartyDirectionList.Codes.Pickup, TransportMode);

			if (newSupplierPickupAddressE2_OA_Address != CurrentSupplierPickupAddressE2_OA_Address)
			{
				SupplierPickupAddressChanged(CurrentSupplierPickupAddressE2_OA_Address, newSupplierPickupAddressE2_OA_Address);
				CurrentSupplierPickupAddressE2_OA_Address = newSupplierPickupAddressE2_OA_Address;
			}
			DefaultCartageEquipment();
		}

#if DEBUG
		internal
#endif
		ZGuid CurrentSupplierPickupAddressE2_OA_Address;

		protected virtual void SupplierPickupAddressChanged(ZGuid oldAddressPK, ZGuid newAddressPK)
		{
		}

		#endregion

		#region NotifyPartyDocumentaryAddress
		public JobDocAddress NotifyPartyDocumentaryAddress
		{
			get
			{
				if (fNotifyPartyDocumentaryAddress == null || fNotifyPartyDocumentaryAddress.IsDeleted)
				{
					fNotifyPartyDocumentaryAddress = DocAddresses.FindOrCreateWithRequirement(NotifyPartyDocAddressRequirement);
					OnNotifyPartyDocumentaryAddressFoundOrCreated(fNotifyPartyDocumentaryAddress);
				}
				return fNotifyPartyDocumentaryAddress;
			}
		}
		JobDocAddress fNotifyPartyDocumentaryAddress;

		protected virtual void OnNotifyPartyDocumentaryAddressFoundOrCreated(JobDocAddress notifyPartyDocumentaryAddress)
		{
		}

		JobDocAddressRequirement NotifyPartyDocAddressRequirement
		{
			get
			{
				if (fNotifyPartyDocAddressRequirement == null)
				{
					fNotifyPartyDocAddressRequirement = new JobDocAddressRequirement(DocAddressType.NotifyParty, ContactType.Consignee);
					DocAddressManager.AddRequirement(fNotifyPartyDocAddressRequirement);
				}
				return fNotifyPartyDocAddressRequirement;
			}
		}
		JobDocAddressRequirement fNotifyPartyDocAddressRequirement;
		#endregion

		#region NotifyParty2DocumentaryAddress
		public JobDocAddress NotifyParty2DocumentaryAddress
		{
			get
			{
				if (fNotifyParty2DocumentaryAddress == null || fNotifyParty2DocumentaryAddress.IsDeleted)
				{
					fNotifyParty2DocumentaryAddress = DocAddresses.FindOrCreateWithRequirement(NotifyParty2DocAddressRequirement);
					OnNotifyParty2DocumentaryAddressFoundOrCreated(fNotifyParty2DocumentaryAddress);
				}
				return fNotifyParty2DocumentaryAddress;
			}
		}
		JobDocAddress fNotifyParty2DocumentaryAddress;

		protected virtual void OnNotifyParty2DocumentaryAddressFoundOrCreated(JobDocAddress notifyParty2DocumentaryAddress)
		{
		}

		JobDocAddressRequirement NotifyParty2DocAddressRequirement
		{
			get
			{
				if (fNotifyParty2DocAddressRequirement == null)
				{
					fNotifyParty2DocAddressRequirement = new JobDocAddressRequirement(DocAddressType.NotifyParty2, ContactType.NotifyParty);
					DocAddressManager.AddRequirement(fNotifyParty2DocAddressRequirement);
				}
				return fNotifyParty2DocAddressRequirement;
			}
		}
		JobDocAddressRequirement fNotifyParty2DocAddressRequirement;
		#endregion

		#region NotifyParty3DocumentaryAddress
		public JobDocAddress NotifyParty3DocumentaryAddress
		{
			get
			{
				if (fNotifyParty3DocumentaryAddress == null || fNotifyParty3DocumentaryAddress.IsDeleted)
				{
					fNotifyParty3DocumentaryAddress = DocAddresses.FindOrCreateWithRequirement(NotifyParty3DocAddressRequirement);
					OnNotifyParty3DocumentaryAddressFoundOrCreated(fNotifyParty3DocumentaryAddress);
				}
				return fNotifyParty3DocumentaryAddress;
			}
		}
		JobDocAddress fNotifyParty3DocumentaryAddress;

		protected virtual void OnNotifyParty3DocumentaryAddressFoundOrCreated(JobDocAddress notifyParty3DocumentaryAddress)
		{
		}

		JobDocAddressRequirement NotifyParty3DocAddressRequirement
		{
			get
			{
				if (fNotifyParty3DocAddressRequirement == null)
				{
					fNotifyParty3DocAddressRequirement = new JobDocAddressRequirement(DocAddressType.NotifyParty3, ContactType.NotifyParty);
					DocAddressManager.AddRequirement(fNotifyParty3DocAddressRequirement);
				}
				return fNotifyParty3DocAddressRequirement;
			}
		}
		JobDocAddressRequirement fNotifyParty3DocAddressRequirement;
		#endregion

		#region Buyer
		public override ZGuid JE_OH_Buyer
		{
			get => base.JE_OH_Buyer;
			set
			{
				var oldValue = base.JE_OH_Buyer;
				base.JE_OH_Buyer = value;
				if (!IsCopying && oldValue != value)
				{
					JE_OH_BuyerChanged(oldValue, value);
				}
				FlushBuyerDocAddressIfBlank(value);
			}
		}

		protected virtual void FlushBuyerDocAddressIfBlank(ZGuid je_oh_buyer)
		{
			if (IsDefaultBuyerDocAddressEnabled && je_oh_buyer.IsEmpty && fBuyerDocAddress != null)
			{
				fBuyerDocAddress.Delete();
				fBuyerDocAddress = null;
			}
		}

		protected virtual void JE_OH_BuyerChanged(ZGuid oldValue, ZGuid newValue)
		{
			if (!fIsImportingData)
			{
				DefaultBuyerDocAddresses(newValue);
			}
		}

		public void DefaultBuyerDocAddresses(ZGuid newValue)
		{
			if (IsDefaultBuyerDocAddressEnabled && isPersistent)
			{
				if (!BuyerDocAddress.E2_AddressOverride)
				{
					BuyerDocAddress.OrganisationPK = newValue;
				}
			}
		}
		protected virtual bool IsDefaultBuyerDocAddressEnabled => false;

		#region BuyerDocAddress
		public JobDocAddress BuyerDocAddress
		{
			get
			{
				if (fBuyerDocAddress == null || fBuyerDocAddress.IsDeleted)
				{
					fBuyerDocAddress = DocAddresses.FindOrCreateWithRequirement(BuyerDocAddressRequirement);
					if (fBuyerDocAddress != null)
					{
						SetupBuyerDocumentaryAddress(fBuyerDocAddress);
					}
				}
				return fBuyerDocAddress;
			}
		}
		JobDocAddress fBuyerDocAddress;

		protected virtual void SetupBuyerDocumentaryAddress(JobDocAddress buyerDocAddress)
		{
		}

		JobDocAddressRequirement BuyerDocAddressRequirement
		{
			get
			{
				if (fBuyerDocAddressRequirement == null)
				{
					fBuyerDocAddressRequirement = new JobDocAddressRequirement(DocAddressType.BuyerDocumentaryAddress, ContactType.Consignee);
					DocAddressManager.AddRequirement(fBuyerDocAddressRequirement);
				}
				return fBuyerDocAddressRequirement;
			}
		}
		JobDocAddressRequirement fBuyerDocAddressRequirement;
		#endregion

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
					DocAddressManager.AddRequirement(fInsuredByDocAddressRequirement);
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
					DocAddressManager.AddRequirement(fAssuredPartyDocAddressRequirement);
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
					DocAddressManager.AddRequirement(fClaimsPayableByDocAddressRequirement);
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
					DocAddressManager.AddRequirement(fSurveyReportPartyDocAddressRequirement);
				}
				return fSurveyReportPartyDocAddressRequirement;
			}
		}
		JobDocAddressRequirement fSurveyReportPartyDocAddressRequirement;
		#endregion

		#region ContainerTerminalOperatorDocAddress
		public JobDocAddress ContainerTerminalOperatorDocAddress
		{
			get
			{
				if (fContainerTerminalOperatorDocAddress == null || fContainerTerminalOperatorDocAddress.IsDeleted)
				{
					if (fContainerTerminalOperatorDocAddress != null)
					{
						fContainerTerminalOperatorDocAddress.DocAddressChanged -= ContainerTerminalOperatorDocAddress_ValueChanged;
					}
					fContainerTerminalOperatorDocAddress = DocAddresses.FindOrCreateWithRequirement(ContainerTerminalOperatorDocAddressRequirement);
					fContainerTerminalOperatorDocAddress.DocAddressChanged += ContainerTerminalOperatorDocAddress_ValueChanged;
					fContainerTerminalOperatorDocAddress.AdditionalValidation = PiggyBackedDocAddressValidation(fContainerTerminalOperatorDocAddress);
				}
				return fContainerTerminalOperatorDocAddress;
			}
		}
		JobDocAddress fContainerTerminalOperatorDocAddress;

		protected virtual void ContainerTerminalOperatorDocAddress_ValueChanged(object sender, EventArgs e)
		{
		}

		JobDocAddressRequirement ContainerTerminalOperatorDocAddressRequirement
		{
			get
			{
				if (fContainerTerminalOperatorDocAddressRequirement == null)
				{
					fContainerTerminalOperatorDocAddressRequirement = new JobDocAddressRequirement(DocAddressType.CustomsContainerTerminalOperatorAddress, ContactType.Administration);
					DocAddressManager.AddRequirement(fContainerTerminalOperatorDocAddressRequirement);
				}
				return fContainerTerminalOperatorDocAddressRequirement;
			}
		}
		JobDocAddressRequirement fContainerTerminalOperatorDocAddressRequirement;
		#endregion

		#region DepotDocAddress
		public JobDocAddress DepotDocAddress
		{
			get
			{
				if (depotDocAddress == null || depotDocAddress.IsDeleted)
				{
					if (depotDocAddress != null)
					{
						depotDocAddress.DocAddressChanged -= DepotDocAddress_DocAddressChanged;
					}
					depotDocAddress = DocAddresses.FindOrCreateWithRequirement(DepotDocAddressRequirement);

					depotDocAddress.DocAddressChanged += DepotDocAddress_DocAddressChanged;
				}
				return depotDocAddress;
			}
		}

		protected virtual void DepotDocAddress_DocAddressChanged(object sender, EventArgs e)
		{
		}

		JobDocAddress depotDocAddress;

		JobDocAddressRequirement DepotDocAddressRequirement
		{
			get
			{
				if (depotDocAddressRequirement == null)
				{
					depotDocAddressRequirement = new JobDocAddressRequirement(DocAddressType.CustomsDepotAddress, ContactType.Administration);
					DocAddressManager.AddRequirement(depotDocAddressRequirement);
				}
				return depotDocAddressRequirement;
			}
		}
		JobDocAddressRequirement depotDocAddressRequirement;
		#endregion

		#region WarehouseDocAddress
		public virtual JobDocAddress WarehouseDocAddress
		{
			get
			{
				if (IsSettingDefaultValues)
				{
					ErrorReporter.ReportOnce("WarehouseDocAddress should not be touch during setting default.");
				}
				if (fWarehouseDocAddress == null || fWarehouseDocAddress.IsDeleted)
				{
					if (fWarehouseDocAddress != null)
					{
						fWarehouseDocAddress.DocAddressChanged -= WarehouseDocAddress_DocAddressChanged;
						foreach (ZPropertyInfo propertyInfo in fWarehouseDocAddress.ZPropertyInfoHash)
						{
							propertyInfo.ValueChanged -= WarehouseDocAddress_ValueChanged;
						}
					}

					fWarehouseDocAddress = DocAddresses.FindOrCreateWithRequirement(WarehouseDocAddressRequirement);

					fWarehouseDocAddress.DocAddressChanged += WarehouseDocAddress_DocAddressChanged;
					foreach (ZPropertyInfo propertyInfo in fWarehouseDocAddress.ZPropertyInfoHash)
					{
						propertyInfo.ValueChanged += WarehouseDocAddress_ValueChanged;
					}
				}
				return fWarehouseDocAddress;
			}
		}

		protected virtual void WarehouseDocAddress_DocAddressChanged(object sender, EventArgs e)
		{
			if (IsWHSUniversalXMLActive)
			{
				MarkAsNeedingValidation();
				InvoiceLines.MarkAsNeedingValidation();
			}
		}

		protected virtual void WarehouseDocAddress_ValueChanged(object sender, EventArgs e)
		{
		}

		JobDocAddress fWarehouseDocAddress;

		public JobDocAddressRequirement WarehouseDocAddressRequirement
		{
			get
			{
				if (fWarehouseDocAddressRequirement == null)
				{
					fWarehouseDocAddressRequirement = GetNewWarehouseDocAddressRequirement();
					fWarehouseDocAddressRequirement.ValidateOrganisationPK += WarehouseDocAddressRequirement_ValidateOrganisationPK;
					fWarehouseDocAddressRequirement.ValidateDelegateToBeFiredUponValidationOf_E2_OA_Address += WarehouseDocAddressRequirement_ValidationE2_OA_Address;
					DocAddressManager.AddRequirement(fWarehouseDocAddressRequirement);
				}
				return fWarehouseDocAddressRequirement;
			}
		}
		JobDocAddressRequirement fWarehouseDocAddressRequirement;

		void WarehouseDocAddressRequirement_ValidateOrganisationPK(JobDocAddressValidation validation)
		{
			var parent = validation.Parent;
			if (parent != null)
			{
				if (parent.IsInDatabase)
				{
					(Validation as BaseJobDeclarationValidation)?.CheckWHSTransactionExists(parent.OrganisationPKInfo, () =>
					{
						var oldValue = parent.E2_OA_AddressInfo.OriginalValue;
						var currentValue = parent.E2_OA_AddressInfo.Value;
						return !oldValue.Equals(currentValue);
					});
				}
				if (!IsBondedWarehousingDisabledForAllEntries && IsBondedWarehousingFieldValidationRequired && IsWarehouseDocAddressRequiredForWarehouseValidation)
				{
					var address = parent.Address;
					if (address == null)
					{
						var importer = Importer;
						parent.OrganisationPKInfo.AddMessageError(BondedWarehouseIsRequiredForBondedWarehousing(TermNameForBondedWarehouse, importer == null ? ZString.Empty : importer.OH_Code));
					}
					else if (IsWarehouseAddressOutsideOfDeclarationCountry(address))
					{
						var importer = Importer;
						var country = Country;
						parent.OrganisationPKInfo.AddMessageError(BondedWarehouseAddressShouldBeInsideDeclarationCountry(TermNameForBondedWarehouse, importer == null ? ZString.Empty : importer.OH_Code, country == null ? ZString.Empty : country.RN_DescMultilingual));
					}
					else
					{
						WarehouseDocAddressRequirement_ValidateOrganisationPK_Additional(parent);
					}
				}
			}
		}

		protected virtual void WarehouseDocAddressRequirement_ValidateOrganisationPK_Additional(JobDocAddress parent)
		{
		}

		protected virtual bool IsWarehouseDocAddressRequiredForWarehouseValidation
		{
			get { return true; }
		}

		void WarehouseDocAddressRequirement_ValidationE2_OA_Address(JobDocAddressValidation validation)
		{
			var parent = validation.Parent;
			if (parent != null && parent.IsInDatabase && !parent.OrganisationPKInfo.HasErrors())
			{
				(Validation as BaseJobDeclarationValidation)?.CheckWHSTransactionExists(parent.E2_OA_AddressInfo);
				if (!IsBondedWarehousingDisabledForAllEntries && IsBondedWarehousingFieldValidationRequired && IsWarehouseDocAddressRequiredForWarehouseValidation)
				{
					validation.ValidateOrganisationPK();
				}
			}
		}

		internal bool IsWarehouseAddressOutsideOfDeclarationCountry(OrgAddress address)
		{
			var result = false;
			if (address != null)
			{
				var relatedPortCode = address.RelatedPortCode;
				result = !IsDeclarationMatchSpecificCountry(relatedPortCode == null ? ZString.Empty : relatedPortCode.RL_RN_NKCountryCode);
			}
			return result;
		}

		public virtual string TermNameForBondedWarehouse
		{
			get { return Res.GetString("99518E55-CCB2-4C97-91A5-AA692BC20C97", "Inventory Management"); }
		}

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "String comparison")]
		public static string BondedWarehouseAddressShouldBeInsideDeclarationCountry(string term, string importerCode, string country)
		{
			return Res.GetString("88700054-8B38-4F41-846B-A3CC4FE60477", "Inventory recording/{0} Integration is active for Importer '{1}'. Please enter a {3} that is located within {2}.", term, importerCode, country, term == "Inventory Management" ? "Bonded Warehouse" : term);
		}

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "String comparison")]
		public static string BondedWarehouseIsRequiredForBondedWarehousing(string term, string importerCode)
		{
			return Res.GetString("33B576C9-626F-404F-895B-99AE2E8B92A9", "Inventory recording/{0} Integration is active for Importer '{1}'. Please enter a {2} that will be used to receive the stock that will be declared on this Declaration.", term, importerCode, term == "Inventory Management" ? "Bonded Warehouse" : term);
		}

		protected virtual JobDocAddressRequirement GetNewWarehouseDocAddressRequirement()
		{
			return new JobDocAddressRequirement(DocAddressType.CustomsWarehouseAddress, ContactType.Administration);
		}

		#endregion

		#region ContainerYardDocAddress
		public JobDocAddress ContainerYardDocAddress
		{
			get
			{
				if (fContainerYardDocAddress == null || fContainerYardDocAddress.IsDeleted)
				{
					if (fContainerYardDocAddress != null)
					{
						fContainerYardDocAddress.DocAddressChanged -= ContainerYardDocAddress_ValueChanged;
					}
					fContainerYardDocAddress = DocAddresses.FindOrCreateWithRequirement(ContainerYardDocAddressRequirement);
					fContainerYardDocAddress.DocAddressChanged += ContainerYardDocAddress_ValueChanged;
				}
				return fContainerYardDocAddress;
			}
		}
		JobDocAddress fContainerYardDocAddress;

		protected virtual void ContainerYardDocAddress_ValueChanged(object sender, EventArgs e)
		{
		}

		JobDocAddressRequirement ContainerYardDocAddressRequirement
		{
			get
			{
				if (fContainerYardDocAddressRequirement == null)
				{
					fContainerYardDocAddressRequirement = new JobDocAddressRequirement(DocAddressType.CustomsContainerYardAddress, ContactType.Administration);
					DocAddressManager.AddRequirement(fContainerYardDocAddressRequirement);
				}
				return fContainerYardDocAddressRequirement;
			}
		}
		JobDocAddressRequirement fContainerYardDocAddressRequirement;
		#endregion

		#endregion

		protected virtual void RefreshAllInvoiceLinesPartSyncManagers()
		{
			foreach (BaseJobComInvoiceLine invoiceLine in InvoiceLines.ToArray())
			{
				invoiceLine.PartSyncManager.Refresh();
			}
		}

		protected virtual ZString? GetImporterEquipment()
		{
			ZString? result = null;
			ZString containerMode = CartageContainerMode;

			if (Importer != null && Importer.MiscServ != null)
			{
				if (//IsImporterDeliveryAddressInitialised &&
					!ImporterDeliveryAddress.E2_AddressOverride && ImporterDeliveryAddress.Address != null)
				{
					if (JE_TransportMode == Core.Constants.TransportModes.Air && !ImporterDeliveryAddress.Address.OA_AIREquipmentNeeded.IsEmpty)
					{
						result = ImporterDeliveryAddress.Address.OA_AIREquipmentNeeded;
					}
					else if (containerMode == Core.Constants.ContainerModes.LCL && !ImporterDeliveryAddress.Address.OA_LCLEquipmentNeeded.IsEmpty)
					{
						result = ImporterDeliveryAddress.Address.OA_LCLEquipmentNeeded;
					}
					else if (containerMode == Core.Constants.ContainerModes.FCL || containerMode == Core.Constants.ContainerModes.FCLMixedShipper)
					{
						if (!ImporterDeliveryAddress.Address.OA_FCLEquipmentNeeded.IsEmpty)
						{
							result = ImporterDeliveryAddress.Address.OA_FCLEquipmentNeeded;
						}
					}
				}
				else
				{
					if (JE_TransportMode == Core.Constants.TransportModes.Air)
					{
						var requiredCartageEquipmentAIR = Enterprise.Registry.Business.OrganisationsDataRegistry.Instance.RequiredCartageEquipmentAIR.Value;
						if (!string.IsNullOrEmpty(requiredCartageEquipmentAIR))
						{
							result = requiredCartageEquipmentAIR;
						}
					}
					else if (containerMode == Core.Constants.ContainerModes.LCL)
					{
						var requiredCartageEquipmentLCL = Enterprise.Registry.Business.OrganisationsDataRegistry.Instance.RequiredCartageEquipmentLCL.Value;
						if (!string.IsNullOrEmpty(requiredCartageEquipmentLCL))
						{
							result = requiredCartageEquipmentLCL;
						}
					}
					else if (containerMode == Core.Constants.ContainerModes.FCL || containerMode == Core.Constants.ContainerModes.FCLMixedShipper)
					{
						var requiredCartageEquipmentFCL = Enterprise.Registry.Business.OrganisationsDataRegistry.Instance.RequiredCartageEquipmentFCL.Value;
						if (!string.IsNullOrEmpty(requiredCartageEquipmentFCL))
						{
							result = requiredCartageEquipmentFCL;
						}
					}
				}
			}

			return result;
		}

		public OrgAddress WarehouseAddress
		{
			get { return WarehouseDocAddress.Address; }
		}

		protected ZString? GetSupplierEquipment()
		{
			ZString? result = null;
			ZString containerMode = CartageContainerMode;

			if (Shipment != null && JE_TransportMode == Core.Constants.TransportModes.Sea && containerMode != Core.Constants.ContainerModes.LCL && containerMode != Core.Constants.ContainerModes.FCL)
			{
				containerMode = Shipment.JS_PackingMode;
			}

			if (!string.IsNullOrEmpty(JE_TransportMode) && Supplier != null && Supplier.MiscServ != null)
			{
				if (//IsSupplierPickupAddressInitialised &&
					!SupplierPickupAddress.E2_AddressOverride && SupplierPickupAddress.Address != null)
				{
					if (JE_TransportMode == Core.Constants.TransportModes.Air && !SupplierPickupAddress.Address.OA_AIREquipmentNeeded.IsEmpty)
					{
						result = SupplierPickupAddress.Address.OA_AIREquipmentNeeded;
					}
					else if (containerMode == Core.Constants.ContainerModes.LCL && !SupplierPickupAddress.Address.OA_LCLEquipmentNeeded.IsEmpty)
					{
						result = SupplierPickupAddress.Address.OA_LCLEquipmentNeeded;
					}
					else if (containerMode == Core.Constants.ContainerModes.FCL && !SupplierPickupAddress.Address.OA_FCLEquipmentNeeded.IsEmpty)
					{
						result = SupplierPickupAddress.Address.OA_FCLEquipmentNeeded;
					}
				}
				else
				{
					if (JE_TransportMode == Core.Constants.TransportModes.Air)
					{
						var requiredCartageEquipmentAIR = Enterprise.Registry.Business.OrganisationsDataRegistry.Instance.RequiredCartageEquipmentAIR.Value;
						if (!string.IsNullOrEmpty(requiredCartageEquipmentAIR))
						{
							result = requiredCartageEquipmentAIR;
						}
					}
					else if (containerMode == Core.Constants.ContainerModes.LCL)
					{
						var requiredCartageEquipmentLCL = Enterprise.Registry.Business.OrganisationsDataRegistry.Instance.RequiredCartageEquipmentLCL.Value;
						if (!string.IsNullOrEmpty(requiredCartageEquipmentLCL))
						{
							result = requiredCartageEquipmentLCL;
						}
					}
					else if (containerMode == Core.Constants.ContainerModes.FCL)
					{
						var requiredCartageEquipmentFCL = Enterprise.Registry.Business.OrganisationsDataRegistry.Instance.RequiredCartageEquipmentFCL.Value;
						if (!string.IsNullOrEmpty(requiredCartageEquipmentFCL))
						{
							result = requiredCartageEquipmentFCL;
						}
					}
				}
			}

			return result;
		}

		#region DefaultValuesFromLocalPartyWhenEntered
		internal protected void DefaultValuesFromLocalPartyWhenEntered(OrgHeader party)
		{
			if (party != null)
			{
				DefaultValuesFromLocalPartyWhenEnteredCore(party);
			}
		}

		protected virtual void DefaultValuesFromLocalPartyWhenEnteredCore(OrgHeader party)
		{
			DefaultJE_MergeByFromLocalParty(party);
		}

		protected virtual void DefaultJE_MergeByFromLocalParty(OrgHeader localParty)
		{
			var mergeBy = GetEffectiveMergeCustomsInvoiceLinesBy(localParty);
			if (!mergeBy.IsEmpty && Lookups.MergeByList.ContainsCode(mergeBy))
			{
				JE_MergeBy = mergeBy;
			}
		}

		public ZString GetEffectiveMergeCustomsInvoiceLinesBy(OrgHeader orgHeader)
		{
			var miscServ = orgHeader.MiscServ;
			var result = miscServ == null ? ZString.Empty : GetMergeCustomsInvoiceLinesBy(miscServ);
			if (result == OrgConstants.MergeInvoiceLines.Default)
			{
				var companyPK = Branch == null ? GlbCompany.CurrentCompany.PK : Branch.GB_GC;
				result = Env.Registry.GetCommercialInvoiceLineMergeMethod(companyPK.ToGuid());
			}

			return result;
		}

		protected virtual ZString GetMergeCustomsInvoiceLinesBy(OrgMiscServ miscServ) => miscServ.OM_IMMergeCustomsInvoiceLinesBy;

		#endregion

		public override ZDateTime JE_ExportDate
		{
			get { return base.JE_ExportDate; }
			set
			{
				bool hasChanged = base.JE_ExportDate != value;
				base.JE_ExportDate = value;

				if (hasChanged && !IsCopying)
				{
					UpdateRoutingIfNoSpecialFlightTerm(JE_ExportDateInfo);

					if (ShouldSetExchangeRatesOnChangeOfExportDate)
					{
						Invoices.SetExchangeRate();
					}

					DefaultDateOfOriginFromExportDate();
					if (fInvoices != null && Invoices.Count > 0)
					{
						MarkApportionmentDirty();
					}

					Invoices.MarkAsNeedingValidation();
					InvoiceLines.MarkAsNeedingValidation();
				}
			}
		}

		protected virtual bool ShouldSetExchangeRatesOnChangeOfExportDate
		{
			get { return DateOfValuation == JE_ExportDate; }
		}

		protected virtual bool IsDateAtOriginGreaterThanExportDate => JE_DateAtOrigin > JE_ExportDate;

		void DefaultDateOfOriginFromExportDate()
		{
			if (!IsDataSyncFromShipment && JE_ExportDate.IsValidSmallDateTime)
			{
				if (JE_DateAtOrigin.IsEmpty && JE_RL_NKOrigin == JE_RL_NKPortOfLoading || IsDateAtOriginGreaterThanExportDate)
				{
					JE_DateAtOrigin = JE_ExportDate;
				}
			}
		}

		void DefaultExportDateFromDateOfOriginIfPossible()
		{
			if (!IsDataSyncFromShipment && JE_DateAtOrigin.IsValidSmallDateTime && !AreTransportDetailsIrrelevant)
			{
				if (JE_ExportDate.IsEmpty && JE_RL_NKOrigin == JE_RL_NKPortOfLoading)
				{
					JE_ExportDate = JE_DateAtOrigin;
				}
			}
		}

		#region JE_VesselName

		[RelatedBusinessObject("Vessel")]
		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.Vessels))]
		public override ZString JE_VesselName
		{
			get { return base.JE_VesselName; }
			set
			{
				bool hasChanged = base.JE_VesselName != value;
				base.JE_VesselName = value;

				if (hasChanged)
				{
					if (!IsCopying)
					{
						JE_LloydsIMO = ZString.Empty;
						DefaultLloydsIMO();
						DefaultCarrierFromVessel();
						UpdateRoutingIfNoSpecialFlightTerm(JE_VesselNameInfo);
						DefaultArrivalAndLoadingDatesFromDestinationAndOrigin();
						DefaultCarrierFromSailing();
						DefaultContainerTerminalCTOAddressFromSailing();
					}

					foreach (BaseCusContainer container in CusContainers)
					{
						ContainerEventDataVendor.Instance.NotifyVesselChanged(container.JobContainer);
					}
				}
			}
		}

		void DefaultLloydsIMO()
		{
			var lloydsNumber = Vessel?.RV_LloydsNumber ?? ZString.Empty;
			if (!lloydsNumber.IsEmpty && lloydsNumber != JE_LloydsIMO)
			{
				JE_LloydsIMO = lloydsNumber;
				Validation.ValidateJE_VesselName();
			}
		}

		/// <summary>
		/// When replacing JE_RV_NKVessel with JE_VesselName with regen and removing the unique constraint on Vessel RV_Code(Name)
		/// This Vessel functionality will be effective and required. Until then, only 1 vessel will always be found.
		///
		///	For Declaration, the loading of the RefVessel is changed to take the Lloyds/IMO into consideration:
		///		By default, the VesselName and LloydsIMO will be used. Each country can choose to override if they decide.
		///		The logic should be:
		///		a)	try to load by VesselName + LloydsIMO, if a single vessel is found, then return that vessel;
		///		b)	If the LloydsIMO is not given, try to load by the VesselName only, if a single vessel is found, then return that vessel;
		///		c)	If multiple vessels are found, return null. (User will need to manually select the required vessel)
		///
		/// </summary>
		public RefVessel Vessel => Vessels.Length == 1 ? Vessels[0] : null;

		public bool VesselHasDuplicates => Vessels.Length > 1;

		RefVessel[] Vessels => Factory.GetValue(ref fVesselsCached, () => LoadVesselsCore());
		CachedProperty<RefVessel[]> fVesselsCached;

		protected virtual RefVessel[] LoadVesselsCore()
		{
			var vesselQuery = new ZQuery(RefVesselSchema.RV_Code, JE_VesselName);
			vesselQuery.IgnoreActiveFilter = true;
			if (!JE_LloydsIMO.IsEmpty)
			{
				vesselQuery.AddToFilter(RefVesselSchema.RV_LloydsNumber, JE_LloydsIMO);
			}

			return Factory.Load<RefVessel>(vesselQuery);
		}

		#endregion

		[ResourceStringData("EBC27C1C-4915-4DC1-B085-5F815B1F7C27|AIR", Caption = "Flight/Folio", MediumCaption = "Flight No.", IsApplicableMember = nameof(IsAir))]
		[ResourceStringData("EBC27C1C-4915-4DC1-B085-5F815B1F7C27|SEA", Caption = "Voyage", IsApplicableMember = nameof(IsSea))]
		[ResourceStringData("EBC27C1C-4915-4DC1-B085-5F815B1F7C27|ROAD", Caption = "Registration", IsApplicableMember = nameof(IsRoad))]
		public override ZString JE_VoyageFlightNo
		{
			get { return base.JE_VoyageFlightNo; }
			set
			{
				bool hasChanged = base.JE_VoyageFlightNo != value;
				base.JE_VoyageFlightNo = value;
				if (hasChanged)
				{
					if (!IsCopying)
					{
						UpdateRoutingIfNoSpecialFlightTerm(JE_VoyageFlightNoInfo);
						DefaultArrivalAndLoadingDatesFromDestinationAndOrigin();
						DefaultCarrierFromSailing();
						if (HasSpecialFlightTerm)
						{
							JE_MasterBill = ZString.Empty;
						}
						else
						{
							this.DefaultMasterBillFromAirLine();
						}

						DefaultContainerTerminalCTOAddressFromSailing();
					}

					foreach (BaseCusContainer container in CusContainers)
					{
						ContainerEventDataVendor.Instance.NotifyVoyageChanged(container.JobContainer);
					}
				}
			}
		}

		public virtual bool HasSpecialFlightTerm => false;

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.ShippingLineList))]
		public override ZGuid JE_OH_ShippingLine
		{
			get { return base.JE_OH_ShippingLine; }
			set
			{
				bool hasChanged = base.JE_OH_ShippingLine != value;
				base.JE_OH_ShippingLine = value;
				if (hasChanged && !IsCopying)
				{
					UpdateRoutingIfNoSpecialFlightTerm(JE_OH_ShippingLineInfo);
					if (!IsSettingDefaultValues)
					{
						var isImport = IsImport;
						var isExport = IsExport;
						this.DefaultCTO(isExport, isImport);
						this.DefaultContainerYard(isExport, isImport);
					}
				}

				if (hasChanged)
				{
					ScreeningStatusUpdater.SetShouldUpdateScreeningStatusByOrgHeader(this, JE_ScreeningStatus, Factory, JE_OH_ShippingLine);
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.ForwarderList))]
		public override ZGuid JE_OH_Forwarder
		{
			get { return base.JE_OH_Forwarder; }
			set
			{
				ZGuid oldValue = base.JE_OH_Forwarder;
				base.JE_OH_Forwarder = value;
				if (base.JE_OH_Forwarder != oldValue)
				{
					ScreeningStatusUpdater.SetShouldUpdateScreeningStatusByOrgHeader(this, JE_ScreeningStatus, Factory, JE_OH_Forwarder);
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.ControllingAgents))]
		public override ZGuid JE_OH_ControllingAgent
		{
			get => base.JE_OH_ControllingAgent;
			set => base.JE_OH_ControllingAgent = value;
		}

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.ControllingCustomers))]
		public override ZGuid JE_OH_ControllingCustomer
		{
			get => base.JE_OH_ControllingCustomer;
			set => base.JE_OH_ControllingCustomer = value;
		}

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.ExternalBrokers))]
		public override ZGuid JE_OH_ExternalBroker
		{
			get => base.JE_OH_ExternalBroker;
			set => base.JE_OH_ExternalBroker = value;
		}

		public virtual ZString BrokerName
		{
			get { return CusAgent != null ? CusAgent.GS_FullName : ZString.Empty; }
		}

		public event TransportModeChangedEventHandler OnTransportModeChanged;
		public delegate void TransportModeChangedEventHandler();

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.TransportTypeList))]
		public override ZString JE_TransportMode
		{
			get { return base.JE_TransportMode; }
			set
			{
				bool hasChanged = JE_TransportMode != value;

				if (value.IsEmpty)
				{
					using (SuspendTransportModeRestoration())
					{
						base.JE_TransportMode = value;
					}
				}
				else
				{
					base.JE_TransportMode = value;
				}

				if (!IsSettingDefaultValues)
				{
					DefaultCartageEquipment();
					SetDefaultCartageOrg();
				}

				if (!IsCopying && !fIsImportingData && !IsSettingDefaultValues && hasChanged)
				{
					ResetContainersAndEquipmentsOnDeclaration_List();
					UpdateETAAndThenDelivery();
					MarkAsNeedingValidationForMajorDataChange();
					JE_VesselName = ZString.Empty;
					JE_LloydsIMO = ZString.Empty;
					JE_VoyageFlightNo = ZString.Empty;
					DefaultIncoTerm();
				}

				OnTransportModeChanged?.Invoke();
				if (hasChanged)
				{
					if (!IsCopying)
					{
						UpdateRoutingIfNoSpecialFlightTerm(JE_TransportModeInfo);
						MarkDocsAndCartageAsNeedingValidationIfNotSettingDefaultValues();
					}

					RedefaultCartageCompanyIfRequired();
					if (!IsSettingDefaultValues)
					{
						var isImport = IsImport;
						var isExport = IsExport;
						if (isImport || isExport)
						{
							this.DefaultExternalBroker(isExport, isImport);
							this.DefaultForwarder(isExport, isImport);
							this.DefaultDepot(isExport, isImport);
						}
					}
				}
			}
		}

		public bool JE_TransportMode_ReadOnly { get; private set; }

		internal ZString GetFreightTransportMode()
		{
			var result = Core.Constants.TransportModes.Other;
			if (IsAir)
			{
				result = Core.Constants.TransportModes.Air;
			}
			else if (IsSea)
			{
				result = Core.Constants.TransportModes.Sea;
			}
			else if (IsRail)
			{
				result = Core.Constants.TransportModes.Rail;
			}
			else if (IsRoad)
			{
				result = Core.Constants.TransportModes.Road;
			}
			else if (IsPost)
			{
				result = Core.Constants.TransportModes.Mail;
			}
			return result;
		}

#if DEBUG //Set by form basher to not modify the property value
		public void SetTransportModeReadOnlyForTest(bool value)
		{
			JE_TransportMode_ReadOnly = value;
		}
#endif

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.TransportMeansList))]
		[ResourceStringData("B27AD1B2-F9EF-4251-9FC8-9FBC0628376F", Caption = "Type of ID", MediumCaption = "ID Type")]
		public override ZString JE_TransportMeans
		{
			get => base.JE_TransportMeans;
			set => base.JE_TransportMeans = value;
		}

		bool isSettingDefaultCartageOrg;
		ZGuid currentDefaultCartageOrg;

		internal void SetDefaultCartageOrg()
		{
			if (isSettingDefaultCartageOrg &&
				((IsImport && DocsAndCartage.DeliveryCartageCoPK != currentDefaultCartageOrg) ||
				 (IsExport && DocsAndCartage.PickupCartageCoPK != currentDefaultCartageOrg)))
			{
				isSettingDefaultCartageOrg = false;
				currentDefaultCartageOrg = ZGuid.Empty;
			}

			if (IsImport && (DocsAndCartage.DeliveryCartageCoPK.IsEmpty || isSettingDefaultCartageOrg))
			{
				isSettingDefaultCartageOrg = true;
				SetIfIsValid((ZPropertyInfoGuid)DocsAndCartage.DeliveryCartageCoPKInfo, DefaultImportCartage);
				JE_OA_DeliveryOrPickupCartageCoAddrInfo.RefreshBindingForParentRelationFK();
				currentDefaultCartageOrg = DocsAndCartage.DeliveryCartageCoPK;
			}
			else if (IsExport && (DocsAndCartage.PickupCartageCoPK.IsEmpty || isSettingDefaultCartageOrg))
			{
				isSettingDefaultCartageOrg = true;
				SetIfIsValid((ZPropertyInfoGuid)DocsAndCartage.PickupCartageCoPKInfo, DefaultExportCartage);
				JE_OA_DeliveryOrPickupCartageCoAddrInfo.RefreshBindingForParentRelationFK();
				currentDefaultCartageOrg = DocsAndCartage.PickupCartageCoPK;
			}

			if (IsImport && fImporterDeliveryAddress != null)
			{
				DefaultCartageCompany(ImporterDeliveryAddress.E2_OA_Address, RelatedPartyDirectionList.Codes.Delivery, TransportMode);
			}
		}

		void SetIfIsValid(ZPropertyInfoGuid info, ZGuid pk)
		{
			info.Value = pk;
			if (pk.IsValid && info.HasErrors())
			{
				info.Value = ZGuid.Empty;
			}
		}

		internal void DefaultCartageEquipment()
		{
			if (Shipment == null)//when shipment's importer and supplier are entered, system would already have defaulted this
			{
				var importDefaultEquipment = IsImport ? GetImporterEquipment() : ZString.Empty;
				if (importDefaultEquipment.HasValue)
				{
					DocsAndCartage.JP_FCLDeliveryEquipmentNeeded = importDefaultEquipment.Value;
				}

				var exportDefaultEquipment = IsExport ? GetSupplierEquipment() : ZString.Empty;
				if (exportDefaultEquipment.HasValue)
				{
					DocsAndCartage.JP_FCLPickupEquipmentNeeded = exportDefaultEquipment.Value;
				}

				JE_FCLDeliveryOrPickupEquipmentNeededInfo.RefreshBinding();
			}
		}

		void RedefaultCartageCompanyIfRequired()
		{
			if (fImporterDeliveryAddress != null && !ImporterDeliveryAddress.E2_OA_Address.IsEmpty)
			{
				DefaultCartageCompany(ImporterDeliveryAddress.E2_OA_Address, RelatedPartyDirectionList.Codes.Delivery, TransportMode);
			}
		}

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.CargoIdTypeList))]
		public override ZString JE_ContainerMode
		{
			get { return base.JE_ContainerMode; }
			set
			{
				if (!SetterSuspender.IsSetterSuspended(BaseJobDeclaration.Schema.JE_ContainerMode))
				{
					var oldValue = JE_ContainerMode;

					if (JE_TransportMode.IsEmpty)
					{
						using (SuspendTransportModeRestoration())
						{
							base.JE_ContainerMode = value;
						}
					}
					else
					{
						base.JE_ContainerMode = value;
					}

					if (!IsCopying && oldValue != JE_ContainerMode && !IsSettingDefaultValues)
					{
						ResetContainersAndEquipmentsOnDeclaration_List();
						DefaultCartageEquipment();
						SetDefaultCartageOrg();

						if (IsImport)
						{
							UpdateETAAndThenDelivery();
						}

						PackingGroups.MarkAsNeedingValidation();
						InvoiceLines.MarkAsNeedingValidation();
						DocsAndCartage.MarkAsNeedingValidation();
						if (!IsSettingDefaultValues)
						{
							var isImport = IsImport;
							var isExport = IsExport;
							if (isImport || isExport)
							{
								this.DefaultExternalBroker(isExport, isImport);
								this.DefaultForwarder(isExport, isImport);
								this.DefaultDepot(isExport, isImport);
							}
						}
					}
				}
			}
		}

		public virtual bool ContainerModeVisible => (IsPost || IsSea) && !IsNonTransportDeclarationType;

#if DEBUG
		bool hasHookedListChangedOnNotesForDebugging;
#endif
		public override ZGuid JE_JS
		{
			get { return base.JE_JS; }
			set
			{
#if DEBUG
				if (!hasHookedListChangedOnNotesForDebugging)
				{
					((IBindingList)Notes.GetAllNotes()).ListChanged += new ListChangedEventHandler(OnNotes_Changed);
					hasHookedListChangedOnNotesForDebugging = true;
				}
#endif
				ZGuid oldValue = base.JE_JS;
				base.JE_JS = value;
				MaybeKillDeclarationsOwnJobDocAddresses(value, oldValue);
				if (value != oldValue && !IsCopying)
				{
					if (DeclarationAlreadyExistsForShipment)
					{
						ReportDeclarationAlreadyExistsForShipmentError();
					}

					CusContainers.MarkAsNeedingValidation();
					Bills.MarkAsNeedingValidation();
					DisposeShipmentSynchroniser();

					if (Shipment != null)
					{
						Transports.RemoveAndDeleteAll();
					}
					MarkDocsAndCartageAsNeedingValidationIfNotSettingDefaultValues();
				}
				fDocsAndCartage = JobDocsAndCartage.SwitchDocsAndCartageAndRemoveOriginal(this);
				SetupDataOnDocsAndCartageFirstSet();
				fAttachedOrders = null;
				ClearDocAddressesPendingSwitchToShipment();
			}
		}

		void ReportDeclarationAlreadyExistsForShipmentError()
		{
			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var filter = JobDeclarationFilter.ForCompanyAndShipment(true, Branch.Company, JE_JS);
			filter.FetchOnlyFromLocalCache = false;
			var declarations = newFactory.Load<BaseJobDeclaration>(filter);

			GlbBranch branch = null;
			GlbCompany company = null;
			var message = new StringBuilder();
			foreach (var dec in declarations)
			{
				branch = newFactory.Load<GlbBranch>(dec.JE_GB);
				company = newFactory.Load<GlbCompany>(branch.GB_GC);
				message.AppendLine($"Declaration '{dec.PK}' created by User '{dec.JE_SystemCreateUser}' at {dec.JE_SystemCreateTimeUtc} with BranchPK={branch.PK}, BranchCode={branch.GB_Code}, BranchName={branch.GB_BranchName}, CompanyCode={company.GC_Code}, CompanyName={company.GC_Name}.");
			}

			branch = Branch;
			company = Branch.Company;
			ErrorReporter.ReportOnce("DeclarationAlreadyExistsForShipment",
	$@"Can't create the declaration '{PK}' with CurrentBranchPK={branch?.PK ?? ZGuid.Empty}, BranchCode={branch?.GB_Code ?? ZString.Empty}, BranchName={branch?.GB_BranchName ?? ZString.Empty}, CompanyCode={company?.GC_Code ?? ZString.Empty}, CompanyName={company?.GC_Name ?? ZString.Empty}.
There already exists declarations for the shipment '{JE_JS}'.
{message}");
		}

		void MaybeKillDeclarationsOwnJobDocAddresses(ZGuid value, ZGuid oldValue)
		{
			// if JobDocAddresses have been (incorrectly) created during ctor or initialisiation of declaration, once we set the shipment we want to get rid of our own addresses because we'll just proxy the shipment's addresses
			if (oldValue.IsEmpty && value.IsValid)
			{
				if (Shipment != null)
				{
					if (IsImporterDeliveryAddressInitialised && Shipment.ConsigneeDeliveryAddress != null)
					{
						fImporterDeliveryAddress.Delete();
						fImporterDeliveryAddress = null;
					}
					if (IsSupplierPickupAddressInitialised && Shipment.ConsignorPickupAddress != null)
					{
						fSupplierPickupAddress.Delete();
						fSupplierPickupAddress = null;
					}
				}
			}
		}

		public void DefaultFreightAmountFromShipmentChargeableAmount()
		{
			if (Shipment != null && Shipment.FrtRateCurrency != null)
			{
				BaseGroupInvoiceCharge freightDefault = JobComInvoiceGroupHeaders[0].Charges.AddNew();
				freightDefault.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
				freightDefault.J7_RX_NKCurrency = Shipment.FrtRateCurrency.RX_Code;

				if (Shipment.JS_PackingMode == Core.Constants.ContainerModes.FCL)
				{
					freightDefault.J7_Amount = Shipment.JS_UnitFreightRate * Shipment.Containers.Count();
				}
				else
				{
					freightDefault.J7_Amount = Shipment.ChargeableAmount;
				}
			}
		}

		public void DefaultAdditionalReferenceNumbersFromShipment()
		{
			DefaultAdditionalReferenceNumbersFromShipmentCore();
		}

		protected virtual void DefaultAdditionalReferenceNumbersFromShipmentCore()
		{ }

		void ClearDocAddressesPendingSwitchToShipment()
		{
			fSupplierDocumentaryAddress = null;
			if (fSupplierPickupAddress != null)
			{
				fSupplierPickupAddress.DocAddressChanged -= new EventHandler(SupplierPickupAddressChanged);
				fSupplierPickupAddress = null;
			}
			fImporterDocumentaryAddress = null;
			if (fImporterDeliveryAddress != null)
			{
				fImporterDeliveryAddress.DocAddressChanged -= new EventHandler(ImporterDeliveryAddressChanged);
				fImporterDeliveryAddress = null;
			}
		}

		/// <summary>
		/// Shipment ports section's destination
		/// </summary>
		public override ZString JE_RL_NKFinalDestination
		{
			get { return base.JE_RL_NKFinalDestination; }
			set
			{
				var oldValue = JE_RL_NKFinalDestination;
				if (!settingFinalDestinationInProgress)
				{
					try
					{
						settingFinalDestinationInProgress = true;
						base.JE_RL_NKFinalDestination = value;
						if (oldValue.Left(2) != JE_RL_NKFinalDestination.Left(2))
						{
							DestinationCountryChanged?.Invoke(this, EventArgs.Empty);
						}
					}
					finally
					{
						settingFinalDestinationInProgress = false;
					}
				}
				if (!IsCopying && oldValue != JE_RL_NKFinalDestination)
				{
					if (oldValue.Left(2) != JE_RL_NKFinalDestination.Left(2))
					{
						DefaultIncoTerm();
						DefaultMessageTypeFromOriginOrDestination();
					}

					if (!IsDataSyncFromShipment)
					{
						if (!ImporterDeliveryAddress.E2_AddressOverride)
						{
							ImporterDeliveryAddress.UpdateDefaultAddress(FinalDestination);
						}

						if (!ImporterDocumentaryAddress.E2_AddressOverride)
						{
							ImporterDocumentaryAddress.UpdateDefaultAddress(FinalDestination);
						}

						if (JE_RL_NKPortOfArrival.IsEmpty && !AreTransportDetailsIrrelevant)
						{
							SetPortOfArrival(JE_RL_NKFinalDestination);
						}

						if (JE_RL_NKPortOfFirstArrival.IsEmpty && IsFirstArrivalDateAndPortUsed && !AreTransportDetailsIrrelevant)
						{
							SetPortOfFirstArrival(JE_RL_NKFinalDestination);
						}
					}
					UpdateETAAndThenDelivery();
					if (!IsSettingDefaultValues)
					{
						var isImport = IsImport;
						var isExport = IsExport;
						if (isImport || isExport)
						{
							this.DefaultCTO(isExport, isImport);
							this.DefaultContainerYard(isExport, isImport);
							this.DefaultForwarder(isExport, isImport);

							if (isImport)
							{
								this.DefaultExternalBroker(isExport, isImport);
								this.DefaultDepot(isExport, isImport);
							}
						}
					}
				}

				DocsAndCartage.RequiredDocuments.SetAllDocumentsReceivedEventLogger();
			}
		}
		bool settingFinalDestinationInProgress;

		protected virtual void SetPortOfArrival(ZString arrival)
		{
			JE_RL_NKPortOfArrival = arrival;
		}

		protected virtual void SetPortOfFirstArrival(ZString arrival)
		{
			if (IsFirstArrivalDateAndPortUsed)
			{
				JE_RL_NKPortOfFirstArrival = arrival;
			}
		}

		/// <summary>
		/// Shipment ports section's origin
		/// </summary>
		public override ZString JE_RL_NKOrigin
		{
			get { return base.JE_RL_NKOrigin; }
			set
			{
				var oldValue = JE_RL_NKOrigin;
				bool hasChanged = oldValue != value;
				base.JE_RL_NKOrigin = value;

				if (!IsCopying && hasChanged)
				{
					if (oldValue.Left(2) != JE_RL_NKOrigin.Left(2))
					{
						DefaultMessageTypeFromOriginOrDestination();
					}

					if (JE_RL_NKPortOfLoading.IsEmpty && !IsDataSyncFromShipment)
					{
						if (!AreTransportDetailsIrrelevant)
						{
							SetPortOfLoading(JE_RL_NKOrigin);
						}
						DefaultDateOfOriginFromExportDate();
					}
					DocsAndCartage.RequiredDocuments.SetAllDocumentsReceivedEventLogger();
					if (!IsSettingDefaultValues)
					{
						var isImport = IsImport;
						var isExport = IsExport;
						if (isImport || isExport)
						{
							this.DefaultForwarder(isExport, isImport);
							this.DefaultCTO(isExport, isImport);
							this.DefaultContainerYard(isExport, isImport);

							if (isExport)
							{
								this.DefaultExternalBroker(isExport, isImport);
								this.DefaultDepot(isExport, isImport);
							}
						}
					}
				}
			}
		}

		protected virtual void SetPortOfLoading(ZString origin)
		{
			JE_RL_NKPortOfLoading = origin;
		}

		[SettingInvalidDateOnDateTimePropertyTestExclude]
		public override ZDateTime JE_DateAtFinalDestination
		{
			get { return base.JE_DateAtFinalDestination; }
			set
			{
				if (value.IsValid && !value.IsValidSmallDateTime && (Env.CurrentUser.IsBatchProcessor || GlbStaff.CurrentUser.GS_Code == User.InterchangeUserCode))
				{
					ErrorReporter.ReportOnce("Invalid small date time was set to declaration DateAtFinalDestination", $"The value '{value.ToISO8601String()}' setting to JE_DateAtFinalDestination is out range of smalldatetime 01-Jan-1900 ~ 06-Jun-2079.");
				}

				var oldValue = JE_DateAtFinalDestination;
				base.JE_DateAtFinalDestination = value;
				if (!IsCopying && oldValue != JE_DateAtFinalDestination)
				{
					if (JE_DateAtFinalDestination.IsValid)
					{
						UpdateDateOfArrivalIfPossible();
						UpdateETDelivery();
					}
					if (Shipment == null)
					{
						Logs.CreateRecreateOrUpdateEventLog(Events.Arrival, EstimateActual.Estimate, value.ToOffset());
						foreach (BaseCusContainer container in CusContainers)
						{
							ContainerEventDataVendor.Instance.NotifyETAChanged(container.JobContainer, JE_DateAtFinalDestinationInfo);
						}
					}
					MarkDocsAndCartageAsNeedingValidationIfNotSettingDefaultValues();
				}
			}
		}

		protected void UpdateDateOfArrivalIfPossible()
		{
			if (!IsDataSyncFromShipment && JE_DateAtFinalDestination.IsValid && !AreTransportDetailsIrrelevant)
			{
				if (JE_DateOfArrival.IsEmpty && JE_RL_NKFinalDestination == JE_RL_NKPortOfArrival)
				{
					JE_DateOfArrival = JE_DateAtFinalDestination;
				}
			}
		}

		public override ZDateTime JE_DateAtOrigin
		{
			get { return base.JE_DateAtOrigin; }
			set
			{
				bool hasChanged = JE_DateAtOrigin != value;
				base.JE_DateAtOrigin = value;
				if (hasChanged)
				{
					if (!IsCopying)
					{
						DefaultExportDateFromDateOfOriginIfPossible();
					}
					if (Shipment == null)
					{
						Logs.CreateRecreateOrUpdateEventLog(Events.Departure, EstimateActual.Estimate, value.ToOffset());
						foreach (BaseCusContainer container in CusContainers)
						{
							ContainerEventDataVendor.Instance.NotifyETDChanged(container.JobContainer, JE_DateAtOriginInfo);
						}
					}
					MarkDocsAndCartageAsNeedingValidationIfNotSettingDefaultValues();
				}
			}
		}

		/// <summary>
		/// Transport section's destination port
		/// </summary>
		public override ZString JE_RL_NKPortOfArrival
		{
			get { return base.JE_RL_NKPortOfArrival; }
			set
			{
				bool hasChanged = JE_RL_NKPortOfArrival != value;
				base.JE_RL_NKPortOfArrival = value;

				if (!IsCopying && !fIsImportingData && !IsSettingDefaultValues && hasChanged)
				{
					if (!IsDataSyncFromShipment)
					{
						SetPortOfFirstArrival(JE_RL_NKPortOfArrival);
						UpdateDateOfFirstArrivalIfPossible();

						if (ShouldCopyPortOfArrivalToFinalDestination)
						{
							JE_RL_NKFinalDestination = JE_RL_NKPortOfArrival;
						}
						UpdateDateOfArrivalIfPossible();

						if (!JE_RL_NKPortOfArrival.IsEmpty)
						{
							DefaultArrivalAndLoadingDatesFromDestinationAndOrigin();
							DefaultContainerTerminalCTOAddressFromSailing();
						}
					}

					UpdateETAAndThenDelivery();
				}

				if (hasChanged)
				{
					if (!IsCopying)
					{
						UpdateRoutingIfNoSpecialFlightTerm(JE_RL_NKPortOfArrivalInfo);
					}

					foreach (BaseCusContainer container in CusContainers)
					{
						ContainerEventDataVendor.Instance.NotifyDischargePortChanged(container.JobContainer);
					}
				}
			}
		}

		protected virtual bool ShouldCopyPortOfArrivalToFinalDestination
		{
			get { return JE_RL_NKFinalDestination.IsEmpty; }
		}

		/// <summary>
		/// Transport section's origin port
		/// </summary>
		public override ZString JE_RL_NKPortOfLoading
		{
			get { return base.JE_RL_NKPortOfLoading; }
			set
			{
				bool hasChanged = JE_RL_NKPortOfLoading != value;
				base.JE_RL_NKPortOfLoading = value;
				if (hasChanged && !IsCopying)
				{
					UpdateRoutingIfNoSpecialFlightTerm(JE_RL_NKPortOfLoadingInfo);

					if (!fIsImportingData && !IsDataSyncFromShipment)
					{
						if (!IsExWarehouse && JE_RL_NKOrigin.IsEmpty)
						{
							SetOriginFromPortOfLoading(JE_RL_NKPortOfLoading);
						}

						DefaultDateOfOriginFromExportDate();
						DefaultExportDateFromDateOfOriginIfPossible();

						if (!JE_RL_NKPortOfLoading.IsEmpty)
						{
							DefaultArrivalAndLoadingDatesFromDestinationAndOrigin();
						}
					}
				}
			}
		}

		protected virtual void SetOriginFromPortOfLoading(ZString portOfLoading)
		{
			JE_RL_NKOrigin = portOfLoading;
		}

#if DEBUG
		public virtual ZString GetPotentialOriginPortNames()
		{
			return Supplier.OH_RL_NKClosestPort;
		}
#endif

		public virtual void DefaultArrivalAndLoadingDatesFromDestinationAndOrigin()
		{
			if (!RoutingDefaultInProgress && !IsDataSyncFromShipment)
			{
				using (new RoutingDefaultLock(this))
				{
					var dischargeVoyageDestination = this.DischargeVoyageDestination;
					if (dischargeVoyageDestination != null)
					{
						var dateOfArrival = dischargeVoyageDestination.JB_A_ARV.IsValid ? dischargeVoyageDestination.JB_A_ARV : dischargeVoyageDestination.JB_E_ARV;

						if (dateOfArrival.IsValid)
						{
							JE_DateOfArrival = dateOfArrival;
						}
					}

					if (IsFirstArrivalDateAndPortUsed)
					{
						var firstArrivalVoyageDestination = this.FirstArrivalVoyageDestination;
						if (firstArrivalVoyageDestination != null)
						{
							var dateOfFirstArrival = firstArrivalVoyageDestination.JB_A_ARV.IsValid ? firstArrivalVoyageDestination.JB_A_ARV : firstArrivalVoyageDestination.JB_E_ARV;

							if (dateOfFirstArrival.IsValid)
							{
								JE_DateOfFirstArrival = dateOfFirstArrival;
							}
						}
					}

					var loadingVoyageOrigin = this.LoadingVoyageOrigin;
					if (loadingVoyageOrigin != null)
					{
						var dateOfExport = loadingVoyageOrigin.JA_A_DEP.IsValid ? loadingVoyageOrigin.JA_A_DEP : loadingVoyageOrigin.JA_E_DEP;

						if (dateOfExport.IsValid)
						{
							JE_ExportDate = dateOfExport;
						}
					}
				}
			}
		}

		void DefaultCarrierFromSailing()
		{
			if (!IsSettingDefaultValues && !RoutingDefaultInProgress && !IsDataSyncFromShipment && JE_OH_ShippingLine.IsEmpty)
			{
				var registry = CustomsDataRegistry.Instance.RelatedPartyDefaultingCarrier.GetFallBackValueAtAllLevels(RegistryCompanyPK, RegistryBranchPK, Guid.Empty);
				if (registry == RelatedPartyDefaultingTypeList.Codes.Both
					|| (IsImport && registry == RelatedPartyDefaultingTypeList.Codes.Import)
					|| (IsExport && registry == RelatedPartyDefaultingTypeList.Codes.Export))
				{
					using (new RoutingDefaultLock(this))
					{
						var voyage = Voyage;

						if (voyage != null && !IsSea)
						{
							JE_OH_ShippingLine = voyage.JV_OH_Line;
						}

						if (JE_OH_ShippingLine.IsEmpty)
						{
							this.DefaultCarrierFromAirLine();
						}
					}
				}
			}
		}

		void DefaultCarrierFromVessel()
		{
			if (!IsSettingDefaultValues && !fIsImportingData && Vessel?.Header != null && JE_OH_ShippingLine.IsEmpty)
			{
				var registry = CustomsDataRegistry.Instance.RelatedPartyDefaultingCarrier.GetFallBackValueAtAllLevels(RegistryCompanyPK, RegistryBranchPK, Guid.Empty);
				if (registry == RelatedPartyDefaultingTypeList.Codes.Both
					|| (IsImport && registry == RelatedPartyDefaultingTypeList.Codes.Import)
					|| (IsExport && registry == RelatedPartyDefaultingTypeList.Codes.Export))
				{
					JE_OH_ShippingLine = Vessel.RV_OH;
				}
			}
		}

		void DefaultContainerTerminalCTOAddressFromSailing()
		{
			if (!RoutingDefaultInProgress && !IsDataSyncFromShipment)
			{
				using (new RoutingDefaultLock(this))
				{
					var dischargeVoyageDestination = this.DischargeVoyageDestination;
					if (dischargeVoyageDestination != null && dischargeVoyageDestination.JB_OA_ArrivalCTOAddress.IsValid)
					{
						ContainerTerminalOperatorDocAddress.E2_OA_Address = dischargeVoyageDestination.JB_OA_ArrivalCTOAddress;
					}
				}
			}
		}

		public event CancelEventHandler OnOverrideFreightDefaultsChanging;

		[BusinessObjectTestExclude()]
		public override ZBool JE_OverrideFreightDefaults
		{
			get { return base.JE_OverrideFreightDefaults; }
			set
			{
				if (CustomsEntryHeaders.Count > 0)
				{
					using (new MergeManager.ChangingMergedDeclarationInAWayThatDoesNotRequireReMerge(this))
					{
						SetOverrideFreightDefaults(value);
					}
				}
				else
				{
					SetOverrideFreightDefaults(value);
				}
			}
		}

		void SetOverrideFreightDefaults(ZBool value)
		{
			if (JE_OverrideFreightDefaults != value)
			{
				if (!value && Shipment != null)
				{
					CancelEventArgs args = new CancelEventArgs(false);
					OnOverrideFreightDefaultsChanging?.Invoke(this, args);

					if (!args.Cancel)
					{
						base.JE_OverrideFreightDefaults = value;
						ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
						OnOverrideFreightDefaultSet();
					}
					else
					{
						JE_OverrideFreightDefaultsInfo.RefreshBinding();
					}
				}
				else
				{
					base.JE_OverrideFreightDefaults = value;
					DisposeShipmentSynchroniser();
				}

				RefreshBindingAndChildrenReadOnly();
			}
		}

		protected virtual void OnOverrideFreightDefaultSet()
		{
		}

		#region JE_OverrideFreightDefaults Ancillary Methods

		public JobDeclarationSynchroniser ShipmentSynchroniser
		{
			get
			{
				if (fShipmentSynchroniser == null)
				{
					if (Shipment == null)
					{
						throw new NotSupportedException("You can't synchronise with a shipment when you don't have a shipment.");
					}

					fShipmentSynchroniser = GetNewShipmentSynchroniser();
				}
				return fShipmentSynchroniser;
			}
		}
		JobDeclarationSynchroniser fShipmentSynchroniser;

		public IEnumerable<ForwardingShipment> GetShipmentsToSync()
		{
			var result = Enumerable.Empty<ForwardingShipment>();
			var shipment = this.Shipment;
			if (shipment != null)
			{
				if (!shipment.IsCoLoadMaster && !shipment.IsBlindCoLoadMaster)
				{
					result = result.Concat(new[] { shipment });
				}

				if (shipment.IsBuyersConsolLead || !result.Any())
				{
					var consol = this.RelevantConsol;
					if (consol == null || !consol.IsDirect)
					{
						result = result.Concat(shipment.CoLoadShipments.Cast<ForwardingShipment>().OrderBy(s => s.JS_HouseBill));
					}
				}
			}

			return IsPackingInformationRelevant ? result : result.Take(1);
		}

		protected internal virtual bool IsHouseBillMandatory
		{
			get { return false; }
		}

		protected void DisposeShipmentSynchroniser()
		{
			if (fShipmentSynchroniser != null)
			{
				fShipmentSynchroniser.SetEnabled(false, fShipmentSynchroniser.DetectEnabled);
				fShipmentSynchroniser.Dispose();
				fShipmentSynchroniser = null;
			}
		}

		protected virtual JobDeclarationSynchroniser GetNewShipmentSynchroniser()
		{
			return new JobDeclarationSynchroniser(this);
		}

		public virtual void SetSynchroniserFieldsReadOnly(ZBool isReadOnly)
		{
			if (fPackages != null)
			{
				Packages.SetReadOnlyIncludingChildren(isReadOnly);
			}
			else
			{
				readOnlyIncludingChildrenNeedsRecalculation = true;
			}
		}

		protected void RefreshBindingAndChildrenReadOnly()
		{
			SetSynchroniserFieldsReadOnly(ShouldSynchroniseWithShipment());
			RefreshBindingIncludingChildren();
		}

		#endregion

		public bool AutoAssignImporterRef
		{
			get
			{
				var importer = this.Importer;
				return importer != null && importer.MiscServ != null && importer.MiscServ.OM_IMAutoImpJobRefered;
			}
		}

		public virtual bool JE_OwnerRef_ReadOnly
		{
			get { return ownerRef_ReadOnlyOverride ?? AutoAssignImporterRef; }
			set { ownerRef_ReadOnlyOverride = value; }
		}
		bool? ownerRef_ReadOnlyOverride;

		public ZString GSTOrVATCode => JE_ApplicationCode == DeclarationApplicationCodeList.Codes.Interfaced ? Core.Constants.Customs.CusEntryFeeTypes.VAT : Core.Constants.Customs.CusEntryFeeTypes.GSTVATAmount;

		public bool EstimateDutyAndTaxOnWHEntries
		{
			get
			{
				var importer = this.Importer;
				return importer != null && importer.MiscServ != null && importer.MiscServ.OM_IMShowDutyOnWarehouseEntries;
			}
		}

		public override OrgHeader Importer => (fImporter ?? (fImporter = new CachedRelatedBusinessObject<OrgHeader>(
				(ZPropertyInfoGuid)JE_OH_ImporterInfo, () =>
				{
					var importer = base.Importer;
					if (importer != null)
					{
						HookToMiscServUpdatedByDataRefresh(importer);
					}

					return importer;
				}))).Value;

		CachedRelatedBusinessObject<OrgHeader> fImporter;

		public override OrgHeader Supplier => (fSupplier ?? (fSupplier = new CachedRelatedBusinessObject<OrgHeader>((ZPropertyInfoGuid)JE_OH_SupplierInfo, () => base.Supplier))).Value;
		CachedRelatedBusinessObject<OrgHeader> fSupplier;

		public ZString DeliveryAddress
		{
			get { return DeliveryAddressCore(); }
		}

		protected virtual ZString DeliveryAddressCore()
		{
			return ZString.Empty;
		}

		void MiscServ_OnMiscServUpdatedByDataRefresh(object sender, EventArgs e)
		{
			MiscServ_OnMiscServUpdatedByDataRefresh();
		}

		protected virtual void MiscServ_OnMiscServUpdatedByDataRefresh()
		{
			JE_OwnerRefInfo.RefreshBinding();
		}

		void HookToMiscServUpdatedByDataRefresh(OrgHeader organisation)
		{
			var miscServ = organisation?.MiscServ;
			if (miscServ != null)
			{
#pragma warning disable
				((IBusiness)miscServ).UpdatedByDataRefreshIncludingChildren += new EventHandler(MiscServ_OnMiscServUpdatedByDataRefresh);
				((IBusiness)miscServ).UpdatedByDataRefreshIncludingChildren += new EventHandler(OnPartAttributeCaptionDetailsChanged);
#pragma warning restore
			}
		}

		void UnhookFromMiscServUpdatedByDataRefresh(OrgHeader organisation)
		{
			var miscServ = organisation?.MiscServ;
			if (miscServ != null)
			{
#pragma warning disable
				((IBusiness)miscServ).UpdatedByDataRefreshIncludingChildren -= new EventHandler(MiscServ_OnMiscServUpdatedByDataRefresh);
				((IBusiness)miscServ).UpdatedByDataRefreshIncludingChildren -= new EventHandler(OnPartAttributeCaptionDetailsChanged);
#pragma warning restore
			}
		}

		/// <summary>
		/// When plugged into shipmnet, the field on brokerage disappears and OnFactorySaving, Declaration.ServiceLevel gets synchronised from shipment.
		/// </summary>
		public override ZString JE_RS_NKServiceLevel
		{
			get { return Shipment != null ? Shipment.JS_RS_NKServiceLevel : base.JE_RS_NKServiceLevel; }
			set
			{
				if (CustomsEntryHeaders.Count > 0)
				{
					using (new MergeManager.ChangingMergedDeclarationInAWayThatDoesNotRequireReMerge(this))
					{
						base.JE_RS_NKServiceLevel = value;
					}
				}
				else
				{
					base.JE_RS_NKServiceLevel = value;
				}
			}
		}

		public override ZString JE_EntryStatus
		{
			get
			{
				var entryStatus = base.JE_EntryStatus;
				if (DeclarationStatusHelper.ShouldCombineEntryStatusFromHeaders(CountryCode, JE_GC))
				{
					var entryStatuses = ActiveEntryHeaders.Cast<CusEntryHeader>().Select(entry => entry.CH_EntryStatus).Distinct().Take(2).ToArray();
					switch (entryStatuses.Length)
					{
						case 2:
							entryStatus = (ZString)CommonEntryStatusList.Codes.MultipleEntryStatus;
							break;
						case 1 when !entryStatuses[0].IsEmpty:
							entryStatus = entryStatuses[0];
							break;
					}
				}

				return entryStatus;
			}
			set
			{
				ZString oldValue = JE_EntryStatus;
				if (CustomsEntryHeaders.Count > 0)
				{
					using (new MergeManager.ChangingMergedDeclarationInAWayThatDoesNotRequireReMerge(this))
					{
						base.JE_EntryStatus = value;
					}
				}
				else
				{
					base.JE_EntryStatus = value;
				}
				if (!IsCopying && oldValue != JE_EntryStatus)
				{
					ClearIsConsolidatedCachedValue();
					AddTransferFromCustomsToManifestEventIfRequired(oldValue, JE_EntryStatus);
				}
				SetStrictConcurrencyPolicy(JE_EntryStatusInfo);
			}
		}

		void AddTransferFromCustomsToManifestEventIfRequired(ZString oldEntryStatus, ZString newEntryStatus)
		{
			if (SupportTransferFromCustomsToManifestEvent && !IsClearedEntryStatus(oldEntryStatus) && IsClearedEntryStatus(newEntryStatus))
			{
				var manifestReference = GlobalManifestReference;
				if (!manifestReference.IsEmpty)
				{
					Logs.AddNew(Events.TransferFromCustomsToManifest, manifestReference);
				}
			}
		}

		protected virtual bool IsClearedEntryStatus(ZString status)
		{
			return false;
		}

		public bool IsGlobalManifestIntegrationEnabled => JE_JS.IsEmpty && SupportTransferFromCustomsToManifestEvent && !GlobalManifestReference.IsEmpty;

		protected internal virtual bool SupportTransferFromCustomsToManifestEvent => false;

		public ZString GlobalManifestReference
		{
			get
			{
				if (globalManifestReferenceCached == null)
				{
					globalManifestReferenceCached = new CachedProperty<ZString>(Factory, () => Logs.MostRecentLogByEventTime(Events.TransferFromManifestToCustoms)?.SL_Reference ?? ZString.Empty);
				}
				return globalManifestReferenceCached.Value;
			}
		}
		CachedProperty<ZString> globalManifestReferenceCached;

		public override ZDateTime JE_SystemCreateTimeUtc
		{
			get { return base.JE_SystemCreateTimeUtc; }
			set
			{
				if (CustomsEntryHeaders.Count > 0)
				{
					using (new MergeManager.ChangingMergedDeclarationInAWayThatDoesNotRequireReMerge(this))
					{
						base.JE_SystemCreateTimeUtc = value;
					}
				}
				else
				{
					base.JE_SystemCreateTimeUtc = value;
				}
				ResetIsWHSUniversalXMLActive();
				MarkDocsAndCartageAsNeedingValidationIfNotSettingDefaultValues();
			}
		}

		public override ZDateTime JE_SystemLastEditTimeUtc
		{
			get { return base.JE_SystemLastEditTimeUtc; }
			set
			{
				if (CustomsEntryHeaders.Count > 0)
				{
					using (new MergeManager.ChangingMergedDeclarationInAWayThatDoesNotRequireReMerge(this))
					{
						base.JE_SystemLastEditTimeUtc = value;
					}
				}
				else
				{
					base.JE_SystemLastEditTimeUtc = value;
				}
			}
		}

		public override ZString JE_SystemLastEditUser
		{
			get { return base.JE_SystemLastEditUser; }
			set
			{
				if (CustomsEntryHeaders.Count > 0)
				{
					using (new MergeManager.ChangingMergedDeclarationInAWayThatDoesNotRequireReMerge(this))
					{
						base.JE_SystemLastEditUser = value;
					}
				}
				else
				{
					base.JE_SystemLastEditUser = value;
				}
			}
		}

		public override ZString JE_SystemCreateUser
		{
			get { return base.JE_SystemCreateUser; }
			set
			{
				if (CustomsEntryHeaders.Count > 0)
				{
					using (new MergeManager.ChangingMergedDeclarationInAWayThatDoesNotRequireReMerge(this))
					{
						base.JE_SystemCreateUser = value;
					}
				}
				else
				{
					base.JE_SystemCreateUser = value;
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.MessageStatusList))]
		public override ZString JE_MessageStatus
		{
			get
			{
				var messageStatus = base.JE_MessageStatus;

				if (DeclarationStatusHelper.ShouldCombineMessageStatusFromHeaders(this.CountryCode))
				{
					var messageStatuses = ActiveEntryHeaders.Cast<CusEntryHeader>().Select(entry => entry.CH_Status).Where(x => !x.IsEmpty).Distinct().Take(2).ToArray();
					switch (messageStatuses.Length)
					{
						case 2:
							messageStatus = (ZString)CommonMessageStatusList.Codes.MultipleMessageStatus;
							break;
						case 1:
							messageStatus = messageStatuses[0];
							break;
					}
				}

				return messageStatus;
			}
			set
			{
				if (CustomsEntryHeaders.Count > 0)
				{
					using (new MergeManager.ChangingMergedDeclarationInAWayThatDoesNotRequireReMerge(this))
					{
						base.JE_MessageStatus = value;
					}
				}
				else
				{
					base.JE_MessageStatus = value;
				}
				SetStrictConcurrencyPolicy(JE_MessageStatusInfo);
			}
		}

		#region Consolidated Entry

		public ZString JE_ConsolidationStatus => ConsolidatedEntryProvider.ConsolidationStatus;

		public JobDeclarationConsolidatedEntryProvider ConsolidatedEntryProvider => consolidatedEntryProvider ?? (consolidatedEntryProvider = GetConsolidatedEntryProvider());
		JobDeclarationConsolidatedEntryProvider consolidatedEntryProvider;

		protected virtual JobDeclarationConsolidatedEntryProvider GetConsolidatedEntryProvider() => new JobDeclarationConsolidatedEntryProvider(this);

		public virtual void OnAppliedToConsolidatedDeclaration()
		{
			ClearIsConsolidatedCachedValue();

			if (!IsLeadDeclarationOfConsolidatedDeclarations)
			{
				foreach (var entryHeader in CustomsEntryHeaders)
				{
					foreach (var charge in entryHeader.Charges.ToList())
					{
						if (NeedRemoveChargeOnConsolidatedDeclaration(charge))
						{
							entryHeader.Charges.RemoveAndDelete(charge);
						}
					}
				}
			}
		}

		protected virtual bool NeedRemoveChargeOnConsolidatedDeclaration(CusEntryHeaderCharges charge) => false;

		public bool IsLeadDeclarationOfConsolidatedDeclarations => ConsolidatedDeclaration.GetConsolidatedDeclaration(this)?.LeadDeclaration?.PK == this.PK;

		public bool IsConsolidated => Factory.GetCachedValue(IsConsolidatedCacheKey, () => ConsolidatedDeclaration.IsConsolidated(this));

		void ClearIsConsolidatedCachedValue() => Factory.ClearCachedValue<bool>(IsConsolidatedCacheKey);

		string IsConsolidatedCacheKey => "JobDeclaration_IsConsolidated_" + JE_DeclarationReference;

		#endregion

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.WarehouseTransactionStatusList))]
		[BusinessObjectTestExclude]
		[MaxLength(CusEntryHeader.Schema.CH_WarehouseTransactionStatusMaxLength)]
		public virtual ZString WarehouseTransactionStatus
		{
			get
			{
				if (warehouseTransactionStatusCached == null)
				{
					warehouseTransactionStatusCached = new CachedProperty<ZString>(Factory, GetWarehouseTransactionStatus);
				}
				return warehouseTransactionStatusCached.Value;
			}
			set
			{
				if (SupportMultipleWarehouseEntry)
				{
					ErrorReporter.ReportOnce("WarehouseTransactionStatus should not be set when SupportMultipleWarehouseEntry is true.");
				}
				else
				{
					var entry = SingleWarehouseEntry;
					if (entry != null)
					{
						entry.CH_WarehouseTransactionStatus = value;
					}
				}
			}
		}
		CachedProperty<ZString> warehouseTransactionStatusCached;

		protected virtual ZString GetWarehouseTransactionStatus()
		{
			ZString? result = null;
			foreach (CusEntryHeader entry in GetWarehouseEntries())
			{
				if (!result.HasValue)
				{
					result = entry.CH_WarehouseTransactionStatus;
				}
				else if (result.Value != entry.CH_WarehouseTransactionStatus)
				{
					result = WarehouseTransactionStatusList.Codes.Multiple;
					break;
				}
			}

			return result.GetValueOrDefault();
		}

		public ZPropertyInfo WarehouseTransactionStatusInfo
		{
			get { return GetZPropertyInfo(Schema.WarehouseTransactionStatus); }
		}

		protected virtual IEnumerable<CusEntryHeader> GetWarehouseEntries()
		{
			if (SupportMultipleWarehouseEntry)
			{
				foreach (CusEntryHeader entry in ActiveEntryHeaders)
				{
					yield return entry;
				}
			}
			else
			{
				var entry = ActiveEntryHeaders.Cast<CusEntryHeader>().FirstOrDefault();
				if (entry != null)
				{
					yield return entry;
				}
			}
		}

		public CusEntryHeader SingleWarehouseEntry
		{
			get
			{
				if (!SupportMultipleWarehouseEntry && !IsValidWarehouseEntry(singleWarehouseEntry))
				{
					singleWarehouseEntry = GetWarehouseEntries().FirstOrDefault();
				}
				return singleWarehouseEntry;
			}
		}
		CusEntryHeader singleWarehouseEntry;

		protected virtual bool IsValidWarehouseEntry(CusEntryHeader entry)
		{
			return singleWarehouseEntry != null && !singleWarehouseEntry.IsDeleted;
		}

		public bool SupportMultipleWarehouseEntry
		{
			get { return SupportMultipleWarehouseEntryCore; }
		}

		protected internal virtual bool SupportMultipleWarehouseEntryCore
		{
			get { return false; }
		}

		[ResourceStringData("Enterprise.Customs.Business.JobDeclaration|WarehouseTransactionStatusDescription", Caption = "Warehouse Status Description", ShortCaption = "WHS Status Desc.")]
		public ZString WarehouseTransactionStatusDescription
		{
			get { return Lookups.WarehouseTransactionStatusList.GetDescriptionFromCode(WarehouseTransactionStatus); }
		}

		public ZPropertyInfo WarehouseTransactionStatusDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.WarehouseTransactionStatusDescription); }
		}

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.ConsolidatedCargoStatusList))]
		public override ZString JE_ConsolidatedCargoStatus
		{
			get { return base.JE_ConsolidatedCargoStatus; }
			set
			{
				if (CustomsEntryHeaders.Count > 0)
				{
					using (new MergeManager.ChangingMergedDeclarationInAWayThatDoesNotRequireReMerge(this))
					{
						base.JE_ConsolidatedCargoStatus = value;
					}
				}
				else
				{
					base.JE_ConsolidatedCargoStatus = value;
				}
				SetStrictConcurrencyPolicy(JE_ConsolidatedCargoStatusInfo);
			}
		}

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.OperationalStatusList))]
		public override ZString JE_OperationalStatus
		{
			get { return base.JE_OperationalStatus; }
			set
			{
				if (CustomsEntryHeaders.Count > 0)
				{
					using (new MergeManager.ChangingMergedDeclarationInAWayThatDoesNotRequireReMerge(this))
					{
						base.JE_OperationalStatus = value;
					}
				}
				else
				{
					base.JE_OperationalStatus = value;
				}
			}
		}

		[ReadOnly(true)]
		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.ScreeningStatusesList))]
		public override ZString JE_ScreeningStatus
		{
			get { return base.JE_ScreeningStatus; }
			set
			{
				ZString oldValue = base.JE_ScreeningStatus;
				base.JE_ScreeningStatus = value;
				if (base.JE_ScreeningStatus != oldValue)
				{
					if (Shipment is IShouldUpdateScreeningStatus s)
					{
						s.ShouldUpdateScreeningStatus = true;
					}
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.CustomsOfficeList))]
		public override ZString JE_CustomsOffice
		{
			get { return base.JE_CustomsOffice; }
			set { base.JE_CustomsOffice = value; }
		}

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.DeclarantTypeList))]
		public override ZString JE_DeclarantType
		{
			get { return base.JE_DeclarantType; }
			set { base.JE_DeclarantType = value; }
		}

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.CarrierCodeCollection))]
		public override ZString JE_CarrierCode
		{
			get { return base.JE_CarrierCode; }
			set { base.JE_CarrierCode = value; }
		}

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.LocationOfGoodsCollection))]
		public override ZString JE_LocationOfGoods
		{
			get { return base.JE_LocationOfGoods; }
			set
			{
				if (!SetterSuspender.IsSetterSuspended(BaseJobDeclaration.Schema.JE_LocationOfGoods))
				{
					base.JE_LocationOfGoods = value;
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.SubLocationOfGoodsCollection))]
		public override ZString JE_SubLocationOfGoods
		{
			get { return base.JE_SubLocationOfGoods; }
			set
			{
				if (!SetterSuspender.IsSetterSuspended(BaseJobDeclaration.Schema.JE_SubLocationOfGoods))
				{
					base.JE_SubLocationOfGoods = value;
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.InlandVesselNamesOrLloyds))]
		public override ZString JE_TransportIDInland
		{
			get => base.JE_TransportIDInland;
			set => base.JE_TransportIDInland = value;
		}

		#endregion

		#region Events

		public void LogCustomsCommencedIfNeeded()
		{
			LogCustomsCommencedIfNeeded(ClearanceEventReference, true);
		}

		public void LogCustomsCommencedIfNeeded(string reference, bool shouldPopulateBroker)
		{
			var mostRecentCommencedLog = LogsOfDeclarationOrShipment.MostRecentLogByEventTimeExcludingEstimated(CustomsCommencedEventType, reference, GetBranchQuery());
			if (mostRecentCommencedLog == null)
			{
				LogsOfDeclarationOrShipment.AddNew(CustomsCommencedEventType, reference);

				if (ShouldLogCustomsCommencedDatail)
				{
					JE_CustomsCommencedDate = ZDateTime.Now;
					JE_GS_NKCustomsCommencedUser = GlbStaff.CurrentUser.GS_Code;
				}
			}

			if (shouldPopulateBroker)
			{
				PopulateBrokerWhenLogCustomsCommenced(mostRecentCommencedLog);
			}
		}

		protected ZQuery GetBranchQuery()
		{
			return new ZQuery(StmALogSchema.SL_GB_NKBranch, GlbCompany.CurrentCompany.ActiveBranches.Cast<GlbBranch>().Select(x => x.GB_Code));
		}

		protected virtual void PopulateBrokerWhenLogCustomsCommenced(StmALog mostRecentCommencedLog)
		{
			if (mostRecentCommencedLog == null || JE_GS_NKCusAgent.IsEmpty)
			{
				DefaultCusAgent();
			}
		}

		void DefaultCusAgent()
		{
			if (CurrentUserIsABroker)
			{
				JE_GS_NKCusAgent = GlbStaff.CurrentUser.GS_Code;
			}
		}

		protected virtual bool CurrentUserIsABroker
		{
			get { return !GlbStaff.CurrentUser.GS_IsSystemAccount; }
		}

		protected virtual bool ShouldLogCustomsCommencedDatail => true;

		public void CancelCustomsEvents()
		{
			StmALog mostRecentCommencedLog = LogsOfDeclarationOrShipment.MostRecentLogByEventTimeExcludingEstimated(CustomsCommencedEventType, GetBranchQuery());
			if (mostRecentCommencedLog != null)
			{
				mostRecentCommencedLog.Cancel();
				LogsOfDeclarationOrShipment.AddNew(Events.Cancelled, "Commence Customs Clearance event canceled");
				JE_CustomsCommencedDate = ZDateTime.Empty;
				JE_GS_NKCustomsCommencedUser = ZString.Empty;
			}
			CancelCustomsClearedEvent();
		}

		protected void CancelCustomsClearedEvent()
		{
			var mostRecentClearedLog = LogsOfDeclarationOrShipment.MostRecentLogByEventTimeExcludingEstimated(CustomsClearedEventType);
			if (mostRecentClearedLog != null)
			{
				mostRecentClearedLog.Cancel();
				LogsOfDeclarationOrShipment.AddNew(Events.Cancelled, "Customs Cleared event canceled");
			}
		}

		public void LogCustomsImpediment()
		{
			LogsOfDeclarationOrShipment.AddNew(Events.CustomsImpedimentReceived);
		}

		public void LogCustomsClearedIfNeeded()
		{
			StmALog mostRecentClearedLog = LogsOfDeclarationOrShipment.MostRecentLogByEventTimeExcludingEstimated(CustomsClearedEventType);
			if (mostRecentClearedLog == null)
			{
				LogCustomsCleared();
			}
		}

		public void LogCustomsCleared()
		{
			LogCustomsClearedCore();
		}

		protected virtual void LogCustomsClearedCore()
		{
			LogsOfDeclarationOrShipment.AddNew(CustomsClearedEventType, ClearanceEventReference);
		}

		public void LogCSHForHVLVStandAloneDeclaration()
		{
			if (IsHVLVStandAloneDeclaration && !HasCSHLogsNotInDB)
			{
				var parameters = new Dictionary<string, string>();
				parameters[EventReferenceParameters.Codes.SourceModuleId] = Core.Constants.GlobalModuleNamesConstants.HVLV;

				Logs.AddNew(AutoEvents.ClearanceStatusChanged, StmALog.GenerateEventReference(string.Empty, parameters));
			}
		}

		protected bool HasCSHLogsNotInDB => Logs.LogsNotInDB.Any(log =>
				log.SL_SE_NKEvent == AutoEvents.ClearanceStatusChanged.Code
				&& !log.IsCancelled
				&& log.Parameters.TryGetValue(EventReferenceParameters.Codes.SourceModuleId, out var sourceModule)
				&& sourceModule == Core.Constants.GlobalModuleNamesConstants.HVLV);

		bool IsHVLVStandAloneDeclaration => Logs.HasLogWith(log =>
				log.SL_SE_NKEvent == AutoEvents.TransferredCode
				&& !log.IsCancelled
				&& log.Parameters.TryGetValue(EventReferenceParameters.Codes.Type, out var sourceModule)
				&& sourceModule == Core.Constants.ShipmentTypes.HighVolumeLowValue);

		protected Event CustomsCommencedEventType
		{
			get
			{
				Event result = Events.CustomsCommenced;
				if (IsExport)
				{
					result = Events.ExportCustomsCommenced;
				}
				return result;
			}
		}

		protected internal Event CustomsClearedEventType
		{
			get
			{
				Event result = Events.CustomsCleared;
				if (IsExport)
				{
					result = Events.ExportCustomsCleared;
				}
				return result;
			}
		}

		protected virtual ZString ClearanceEventReference
		{
			get
			{
				ZString description = Res.GetString("edb032ba-41a1-4a79-87f8-eb0cc8ede224", "Other");
				if (IsImport && IsExport)
				{
					description = Res.GetString("0cbdf7f5-6176-4eae-9d34-f56264533d10", "Transhipment");
				}
				else if (IsImport)
				{
					description = Res.GetString("6a6da828-2a26-43ac-8320-6a0e58c22d45", "Import");
				}
				else if (IsExport)
				{
					description = Res.GetString("6fed0b8b-4bc5-4be1-87b0-761405ca5ad2", "Export");
				}

				return GlbCompany.CurrentCompany.GC_RN_NKCountryCode + " " + description;
			}
		}

		#endregion

		#region Audit

		public ZDateTime AuditDate => AuditDateUtc.ToLocalBranchTime();

		public ZDateTime AuditDateUtc
		{
			get => JE_AuditDateUtc;
			set => JE_AuditDateUtc = value;
		}

		public ZString AuditReference
		{
			get => JE_AuditReference;
			set => JE_AuditReference = value.SubstringSafe(0, JE_AuditReferenceInfo.MaxLength);
		}

		public int AuditReferenceMaxLength => JE_AuditReferenceInfo.MaxLength;

		public ZString AuditLogUser
		{
			get => JE_GS_NKAuditUser;
			set => JE_GS_NKAuditUser = value;
		}

		public ZString AuditLogUserName => Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, JE_GS_NKAuditUser)?.GS_FullName ?? ZString.Empty;

		#endregion

		#region Related BO

		public BaseJobComInvoiceGroupHeader TopGroupInvoice
		{
			get
			{
				return SupportAdditionalInvoices
					? JobComInvoiceGroupHeaders.Cast<BaseJobComInvoiceGroupHeader>().FirstOrDefault(x => !JobComInvoiceGroupHeaders.IsAdditionalGroupHeader(x))
					: JobComInvoiceGroupHeaders[0];
			}
		}

		public RefAirline Airline
		{
			get { return RefAirline.LoadFromAirline2LetterCode(Factory, AirlinePrefix); }
		}

		public
#if DEBUG
 virtual
#endif
 ZString AirlineName
		{
			get
			{
				RefAirline result = Airline;
				return result != null ? result.RM_AirlineName1 : ZString.Empty;
			}
		}

		public JobHeader Job
		{
			get
			{
				if (Globals.IsWeb)
				{
					return (Shipment != null) ? Shipment.GetJob(Company) : new JobHeader.Loader(this).Load(false, Company);
				}
				else
				{
					return (Shipment != null) ? Shipment.Job : new JobHeader.Loader(this).Load(true, false);
				}
			}
		}

		#endregion

		#region ICustomsCustomLabelsConfigOrgProvider Members

		ZString ICustomsCustomLabelsConfigOrgProvider.PartAttribute1 => BaseJobComInvoiceLine.Schema.JI_PartAttrib1;
		ZString ICustomsCustomLabelsConfigOrgProvider.PartAttribute2 => BaseJobComInvoiceLine.Schema.JI_PartAttrib2;
		ZString ICustomsCustomLabelsConfigOrgProvider.PartAttribute3 => BaseJobComInvoiceLine.Schema.JI_PartAttrib3;
		ZString ICustomsCustomLabelsConfigOrgProvider.SerialNumber => BaseJobComInvoiceLine.Schema.JI_SerialNumber;

		#endregion

		#region ICustomLabelsConfigOrgProvider Members

		public event EventHandler ConfigOrgChanged
		{
			add
			{
				EventHandler passed = value;

				JE_OH_SupplierInfo.ValueChanged += passed;
				JE_OH_ImporterInfo.ValueChanged += passed;
				JE_MessageTypeInfo.ValueChanged += passed;
			}
			remove
			{
				EventHandler passed = value;

				JE_OH_SupplierInfo.ValueChanged -= passed;
				JE_OH_ImporterInfo.ValueChanged -= passed;
				JE_MessageTypeInfo.ValueChanged -= passed;
			}
		}

		public OrgHeader ConfigOrg
		{
			get
			{
				if (!IsDeleted)
				{
					if (configOrgCached == null)
					{
						configOrgCached = new CachedProperty<OrgHeader>(Factory, () =>
						{
							if (!this.IsPersistent && Invoices.Count == 1 && !Invoices[0].IsAttachedToPersistentDeclaration)
							{
								return Invoices[0].JZ_MessageType == JobMessageTypeList.Codes.Export ? Invoices[0].Supplier : Invoices[0].Buyer;
							}
							else
							{
								return JE_MessageType == JobMessageTypeList.Codes.Export ? Supplier : Importer;
							}
						});
					}
					return configOrgCached.Value;
				}
				return null;
			}
		}
		CachedProperty<OrgHeader> configOrgCached;

		#endregion

		#region IExternalFactoryRefreshable Members

		bool fExternalFactoryRefreshEnabled;
		public bool ExternalFactoryRefreshEnabled
		{
			get { return fExternalFactoryRefreshEnabled; }
			set
			{
				if (fExternalFactoryRefreshEnabled != value)
				{
					fExternalFactoryRefreshEnabled = value;
					OnExternalFactoryRefreshEnabledChanged();
				}
			}
		}

		protected virtual void OnExternalFactoryRefreshEnabledChanged()
		{
			if (fJobComInvoiceGroupHeaders != null)
			{
				fJobComInvoiceGroupHeaders.ExternalFactoryRefreshEnabled = ExternalFactoryRefreshEnabled;
			}
		}

		#endregion

		#region ICustomJobInfo Members

		public EntryInfoCollection Entries
		{
			get
			{
				var result = new EntryInfoCollection();

				foreach (CusEntryHeader header in ActiveEntryHeaders)
				{
					result.AddNew(header.MergedLines.Count, header.MergedLines.InvoiceLineCount, header.CustomsValue);
				}

				if (result.Count == 0 && AreInvoiceLinesUsedAsLinesInCustomsMessage)
				{
					result.AddNew(InvoiceLines.Count, InvoiceLines.Count, 0m);
				}
				return result;
			}
		}

		protected virtual bool AreInvoiceLinesUsedAsLinesInCustomsMessage
		{
			get { return false; }
		}

		#endregion

		#region Notes

		public override BusinessObject[] BusinessObjectsWithRelatedNotes
		{
			get
			{
				List<BusinessObject> result = new List<BusinessObject>();
				if (IsPersistent)
				{
					OrgHeader importer = Importer;
					if (importer != null)
					{
						result.Add(importer);
					}
					OrgHeader supplier = Supplier;
					if (supplier != null)
					{
						result.Add(supplier);
					}
					result.AddRange(AttachedOrders.ToArray());
					foreach (BaseCusContainer cusContainer in CusContainers)
					{
						result.Add(cusContainer.JobContainer);
					}

					foreach (var invoice in Invoices)
					{
						result.Add(invoice);
					}
				}
				return result.ToArray();
			}
		}

		protected override StmNoteContexts NoteContextsForRelatedNotes
		{
			get
			{
				StmNoteContexts result = new StmNoteContexts();
				result = StmNoteContextUtils.GetContextFromTransportModeImportExport(Constants.GlobalModuleNamesConstants.CustomsDeclarations, "", JE_TransportMode, FreightContainerMode);
				result.Module |= base.NoteContextsForRelatedNotes.Module;
				result.Direction |= base.NoteContextsForRelatedNotes.Direction;
				result.FreightMode |= base.NoteContextsForRelatedNotes.FreightMode;

				if (Shipment != null)
				{
					result.Module |= StmNoteContextModule.F;
				}
				if (IsImport)
				{
					result.Direction |= StmNoteContextDirection.I;
				}

				if (IsExport)
				{
					result.Direction |= StmNoteContextDirection.E;
				}

				return result;
			}
		}

		#endregion

		#region Shipment

		CommonShipment IShipmentProvider.Shipment
		{
			get { return Shipment; }
		}

		public ForwardingShipment Shipment
		{
			get
			{
				if (IsDeleted)
				{
					return null;
				}
				if (fShipment != null && JE_JS.IsEmpty)
				{
					fShipment = null;
				}
				else if ((fShipment == null && !JE_JS.IsEmpty) || (fShipment != null && fShipment.PK != JE_JS))
				{
					fShipment = Factory.Load<ForwardingShipment>(JE_JS);
				}

				return fShipment;
			}
		}
		ForwardingShipment fShipment;

		#region HouseBills Collections

		[ChildEditable(true)]
		public IBillCollection<Bill, BaseJobDeclaration> Bills
		{
			get
			{
				if (fBills == null)
				{
					EnsureNoStackOverflow(ref gettingBillsInProgress, nameof(Bills), () =>
					{
						fBills = CreateNewBillCollection();
						fBills.Load();
						RegisterEditableChildObject(fBills);
					});
				}
				return fBills;
			}
		}
		IBillCollection<Bill, BaseJobDeclaration> fBills;

		protected virtual IBillCollection<Bill, BaseJobDeclaration> CreateNewBillCollection()
		{
			return new BillCollection<Bill, BaseJobDeclaration>(this, Factory);
		}

		/// <summary>
		/// This is the collection that is exposed on the form
		/// </summary>
		public IBillTypeViewCollection<Bill, BaseJobDeclaration> FilteredBills => fFilteredBills ?? (fFilteredBills = CreateNewBillTypeCollection());
		IBillTypeViewCollection<Bill, BaseJobDeclaration> fFilteredBills;

		protected virtual IBillTypeViewCollection<Bill, BaseJobDeclaration> CreateNewBillTypeCollection() => new BillTypeViewCollection<Bill, BaseJobDeclaration>(this);

		public ILowestBillCollection<Bill, BaseJobDeclaration> LowestBills => fLowestBills ?? (fLowestBills = CreateNewLowestBills());
		ILowestBillCollection<Bill, BaseJobDeclaration> fLowestBills;

		protected virtual ILowestBillCollection<Bill, BaseJobDeclaration> CreateNewLowestBills() => new LowestBillCollection<Bill, BaseJobDeclaration>(this);

		#region SynchroniseBill

		protected bool IsSynchronisingBill
		{
			get { return BillGUIPresentationFlagUpdater.IsSynchronising; }
		}

		internal BillGUIPresentationFlagUpdater BillGUIPresentationFlagUpdater
		{
			get
			{
				if (fBillGUIPresentationFlagUpdater == null)
				{
					fBillGUIPresentationFlagUpdater = new BillGUIPresentationFlagUpdater(this);
				}
				return fBillGUIPresentationFlagUpdater;
			}
		}
		BillGUIPresentationFlagUpdater fBillGUIPresentationFlagUpdater;

		internal IDisposable GetBillGUIPresentationFlagSuspender()
		{
			return BillGUIPresentationFlagUpdater.SuspendSynch();
		}

		[ResourceStringData("67dfdc4f-c0ce-4875-9d26-a77248160d7e", Caption = "Ocean Bill", FullDescription = "Ocean Bill of the consignment.", IsApplicableMember = nameof(IsSea))]
		[ReadOnlyMember(nameof(JE_MasterBill_ReadOnly))]
		public override ZString JE_MasterBill
		{
			get { return base.JE_MasterBill; }
			set
			{
				if (!IsCopying && IsAir)
				{
					value = value.Replace("-", "").Replace(" ", "");
				}

				bool hasChanged = base.JE_MasterBill != value;
				base.JE_MasterBill = value;
				var shouldSynchroniseMasterBill = hasChanged || (!JE_MasterBill.EqualsIgnoringCase(PrimaryMasterBill?.CU_BillNum ?? ZString.Empty));
				if (shouldSynchroniseMasterBill && !IsCopying)
				{
					BillGUIPresentationFlagUpdater.SynchroniseToBills(value, BillTypeList.Codes.MasterBill, AdditionalPrimaryMasterBillMatching);
					if (!PackingLinesShouldOnlyBeCreatedAtTheHouseBillLevel)
					{
						if (!IsSynchronisingBill && (!JE_TotalNoOfPacks.IsEmpty || !JE_TotalNoOfPacksPackType.IsEmpty) && PrimaryMasterBill != null && PrimaryHouseBill == null)
						{
							DefaultPackDetailsToPackingGroupIfRequired(Schema.JE_TotalNoOfPacks);
							DefaultPackDetailsToPackingGroupIfRequired(Schema.JE_TotalNoOfPacksPackType);
						}
					}
				}

				if (!IsCopying && hasChanged)
				{
					DefaultCarrierFromSailing();
				}
			}
		}

		protected bool JE_MasterBill_ReadOnly
		{
			get { return ShouldSynchroniseWithShipment() || IsGlobalManifestIntegrationEnabled; }
		}

		[ReadOnlyMember(nameof(JE_HouseBill_ReadOnly))]
		public override ZString JE_HouseBill
		{
			get { return base.JE_HouseBill; }
			set
			{
				bool hasChanged = base.JE_HouseBill != value;
				base.JE_HouseBill = value;
				var shouldSynchroniseHouseBill = hasChanged || (!JE_HouseBill.EqualsIgnoringCase(PrimaryHouseBill?.CU_BillNum ?? ZString.Empty));
				if (shouldSynchroniseHouseBill && !IsCopying)
				{
					var oldPrimaryHouseBill = PrimaryHouseBill;
					BillGUIPresentationFlagUpdater.SynchroniseToBills(value, BillTypeList.Codes.HouseBill, AdditionalPrimaryHouseBillMatching);

					var newPrimaryHouseBill = PrimaryHouseBill;
					if (newPrimaryHouseBill != null)
					{
						if (!IsSynchronisingBill && (!JE_TotalNoOfPacks.IsEmpty || !JE_TotalNoOfPacksPackType.IsEmpty))
						{
							DefaultPackDetailsToPackingGroupIfRequired(Schema.JE_TotalNoOfPacks);
							DefaultPackDetailsToPackingGroupIfRequired(Schema.JE_TotalNoOfPacksPackType);
						}
						newPrimaryHouseBill.MarkAsNeedingValidation();
					}
				}

				if (!IsCopying)
				{
					BillGUIPresentationFlagUpdater.CheckHouseBillAndPrimaryBill();
				}
			}
		}

		protected bool JE_HouseBill_ReadOnly
		{
			get { return ShouldSynchroniseWithShipment() || IsGlobalManifestIntegrationEnabled; }
		}

		#endregion

		public IDisposable TemporarySetAdditionalPrimaryMasterBillMatching(Func<Bill, bool> additionalMatch)
		{
			var oldDelegate = additionalPrimaryMasterBillMatchingDelegate;
			return new DisposableAction(() =>
			{
				additionalPrimaryMasterBillMatchingDelegate = additionalMatch;
			},
			() =>
			{
				additionalPrimaryMasterBillMatchingDelegate = oldDelegate;
			});
		}

		Func<Bill, bool> additionalPrimaryMasterBillMatchingDelegate;

		bool AdditionalPrimaryMasterBillMatching(Bill bill)
		{
			return additionalPrimaryMasterBillMatchingDelegate == null || additionalPrimaryMasterBillMatchingDelegate(bill);
		}

		public IDisposable TemporarySetAdditionalPrimaryHouseBillMatching(Func<Bill, bool> additionalMatch)
		{
			var oldDelegate = additionalPrimaryHouseBillMatchDelegate;
			return new DisposableAction(() =>
			{
				additionalPrimaryHouseBillMatchDelegate = additionalMatch;
			},
			() =>
			{
				additionalPrimaryHouseBillMatchDelegate = oldDelegate;
			});
		}

		Func<Bill, bool> additionalPrimaryHouseBillMatchDelegate;

		bool AdditionalPrimaryHouseBillMatching(Bill bill)
		{
			return additionalPrimaryHouseBillMatchDelegate == null ? bill.ParentBill is Bill parentBill && parentBill.CU_BillNum.EqualsIgnoringCase(JE_MasterBill) : additionalPrimaryHouseBillMatchDelegate(bill);
		}

		public Bill PrimaryHouseBill
		{
			get { return Bills.PrimaryHouseBill; }
		}

		public Bill PrimaryMasterBill
		{
			get { return Bills.PrimaryMasterBill; }
		}

		public ZPropertyInfo HouseBillIssuedDateInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.HouseBillIssuedDate); }
		}

		[BusinessObjectTestExclude]
		public ZDateTime HouseBillIssuedDate
		{
			get { return PrimaryHouseBill != null ? PrimaryHouseBill.CU_IssueDate : ZDateTime.Empty; }
			set
			{
				Bill primaryHouseBill = this.PrimaryHouseBill;

				if (!value.IsEmpty && primaryHouseBill == null)
				{
					primaryHouseBill = Bills.CreatePrimaryBill(BillTypeList.Codes.HouseBill);
				}

				if (primaryHouseBill != null)
				{
					primaryHouseBill.CU_IssueDate = value;
				}
				HouseBillIssuedDateInfo.RefreshBinding();
			}
		}

		#endregion

		public bool IsPluggedIntoShipment
		{
			get { return JE_JS.IsValid; }
		}

		#endregion

		#region SoldToPartyOrgPK
		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.SoldToParties))]
		public ZGuid SoldToPartyOrgPK
		{
			get { return JE_OA_SoldToPartyAddress_ZAddress.OrgPK; }
			set { JE_OA_SoldToPartyAddress_ZAddress.OrgPK = value; }
		}

		public ZPropertyInfo SoldToPartyOrgPKInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.SoldToPartyOrgPK, x => JE_OA_SoldToPartyAddress_ZAddress.OrgPKInfo); }
		}

		#endregion

		public ForwardingConsol RelevantConsol
		{
			get
			{
				ForwardingConsol result = null;
				var shipment = Shipment;
				if (shipment != null)
				{
					var consols = shipment.Consols;
					if (consols.Count > 0)
					{
						ZString localPortPropertyName = IsExport ? ForwardingConsol.Schema.JK_RL_NKLoadPort
														: IsImport ? ForwardingConsol.Schema.JK_RL_NKDischargePort : string.Empty;

						if (!localPortPropertyName.IsEmpty)
						{
							var countryCode = Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(CountryCode);
							result = (from ForwardingConsol consol in consols
									  where Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(((ZString)consol[localPortPropertyName]).Left(2)) == countryCode && !consol.IsDomestic()
									  select consol).FirstOrDefault();
							if (result == null && Core.Constants.CountryCodes.IsInEuropeanCustomsUnion(CountryCode))
							{
								result = (from ForwardingConsol consol in consols
										  orderby consol.JK_ETAForImportTransport
										  where Core.Constants.CountryCodes.GetCustomsCountryOfJurisdictionOrEU(((ZString)consol[localPortPropertyName]).Left(2)) == Core.Constants.CountryCodes.EuropeanUnion && !consol.IsDomestic()
										  select consol).FirstOrDefault();
							}
						}
						else if (consols.Count == 1)
						{
							result = consols[0];
						}
					}
				}
				return result;
			}
		}

		#region PreAdvice

		public JobShipmentPreplanning PreAdvice
		{
			get
			{
				ZQuery query = new ZQuery(JobShipmentPreplanningSchema.EF_JE, PK)
				{
					FetchOnlyFromLocalCache = !IsInDatabase
				};
				return Factory.LoadTop1<JobShipmentPreplanning>(query);
			}
		}

		#endregion

		#region Schedule

		public JobVoyage Voyage
		{
			get
			{
				if (voyage == null
					|| voyage.IsDeleted
					|| voyage.JV_RV_NKVessel != JE_VesselName
					|| voyage.JV_VoyageFlight != JE_VoyageFlightNo
					|| voyage.JV_OH_Line != JE_OH_ShippingLine
					|| lastExportDateForVoyageProperty != JE_ExportDate)
				{
					voyage = new JobVoyage.Loader(Factory).Load(JE_TransportMode, JE_VesselName, JE_VoyageFlightNo, JE_OH_ShippingLine, JE_ExportDate);
					lastExportDateForVoyageProperty = JE_ExportDate;
				}

				return voyage;
			}
		}
		ZDateTime lastExportDateForVoyageProperty;
		JobVoyage voyage;

		public VoyageOrigin LoadingVoyageOrigin
		{
			get { return Voyage?.Origins.GetOriginFromLoading(JE_RL_NKPortOfLoading); }
		}

		public VoyageDestination DischargeVoyageDestination
		{
			get { return Voyage?.Destinations.GetDestinationFromDischarge(JE_RL_NKPortOfArrival); }
		}

		public VoyageDestination FirstArrivalVoyageDestination
		{
			get { return Voyage?.Destinations.GetDestinationFromDischarge(JE_RL_NKPortOfFirstArrival.IsEmpty ? JE_RL_NKPortOfArrival : JE_RL_NKPortOfFirstArrival); }
		}

		#endregion

		#region Loading

		public static BaseJobDeclaration LoadFirstMatchingInCurrentCompanyIncludingInActive(BusinessObjectFactory factory, ZString declarationReference)
		{
			if (!declarationReference.IsEmpty)
			{
				var filter = JobDeclarationFilter.ForDeclarationReference(true, declarationReference);
				return factory.LoadTop1<BaseJobDeclaration>(filter);
			}
			return null;
		}

		public static BaseJobDeclaration Load(ForwardingShipment shipment)
		{
			if (shipment == null)
			{
				return null;
			}

			var filter = JobDeclarationFilter.ForCompanyAndShipment(true, GlbCompany.CurrentCompany, shipment.PK);
			filter.FetchOnlyFromLocalCache = !shipment.IsInDatabase;
			return shipment.Factory.LoadTop1<BaseJobDeclaration>(filter);
		}

		#endregion

		#region When Attached to Shipment

		public virtual bool AttachedOrdersVisible
		{
			get { return !JE_JS.IsValid; }
		}

		public bool NotesAndEventsVisible
		{
			get { return !JE_JS.IsValid; }
		}

		public bool ServiceLevelVisible
		{
			get { return !JE_JS.IsValid; }
		}

		protected void OnNotes_Changed(object sender, ListChangedEventArgs e)
		{
			if (JE_JS.IsValid &&
				e.ListChangedType == System.ComponentModel.ListChangedType.ItemAdded &&
				(!(sender is StmNoteCollection) || !(((StmNoteCollection)sender).Master is ForwardingShipment))
				)
			{
				ErrorReporter.ReportOnce("BaseJobDeclaration.OnNotes_Changed", "Cannot add a note to a declaration that is attached to a shipment. Add to the shipment instead. (Declaration PK='" + PK + "')");
			}
		}

		#endregion

		#region Merge Fetch Hints

		public void AddMergeFetchHints()
		{
			if (FetchStrategy is FetchStrategies.BaseJobDeclarationFetchStrategy declarationFetchStrategy)
			{
				declarationFetchStrategy.FetchForMerge();
			}
		}

		#endregion

		#region ISupportDataImporting Members

		protected bool fIsImportingData;
		public bool IsImportingData
		{
			get { return fIsImportingData; }
			set { fIsImportingData = value; }
		}

		#endregion

		#region ISupportUXMLDataImporting Members

		public bool IsUXMLImportingData
		{
			get;
			set;
		}

		#endregion

		#region IAttachOrders Members

		[ChildEditable(false)]
		public OrderCollection AttachedOrders
		{
			get
			{
				OrderCollection result;
				if (Shipment != null)
				{
					result = Shipment.AttachedOrders;
				}
				else
				{
					if (fAttachedOrders == null)
					{
						fAttachedOrders = new OrderCollection(Factory, this);
						RegisterEditableChildObject(fAttachedOrders);
						fAttachedOrders.CollectionCountChange += AttachedOrders_CollectionCountChanged;
						fAttachedOrders.WhereNotNull().ForEach(x => x.JD_OrderNumberInfo.ValueChanged += OnOwnerRefNeedsUpdate);
					}
					result = (OrderCollection)fAttachedOrders.Clone();
					OrderLimitHelper.SetCollectionLimit(result);
				}
				AttachedOrdersAccessed?.Invoke(this, EventArgs.Empty);
				return result;
			}
		}

		OrderCollection fAttachedOrders;
		internal event EventHandler AttachedOrdersAccessed;

		void AttachedOrders_CollectionCountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			if (e.BizObject is Order order)
			{
				if (e.ItemAdded)
				{
					order.JD_OrderNumberInfo.ValueChanged -= OnOwnerRefNeedsUpdate;
					order.JD_OrderNumberInfo.ValueChanged += OnOwnerRefNeedsUpdate;
				}
				else if (e.ItemRemoved)
				{
					order.JD_OrderNumberInfo.ValueChanged -= OnOwnerRefNeedsUpdate;
				}
				if (!IsValidationSuspended)
				{
					if (!DocsAndCartage.IsDeleted && !DocsAndCartage.IsValidationSuspended)
					{
						DocsAndCartage.Validation.ValidateJP_OrderItemsAsString();
					}
				}
			}
			OnOwnerRefNeedsUpdate(sender, e);
		}

		OrdersOnDeclarationLimitHelper OrderLimitHelper
		{
			get { return orderLimitHelper ?? (orderLimitHelper = new OrdersOnDeclarationLimitHelper(this)); }
		}

		OrdersOnDeclarationLimitHelper orderLimitHelper;

		public OrderCollection PossibleOrdersForAttachment_List
		{
			get { return Order.GetPossibleOrdersForAttachment_List(this); }
		}

		public void SetDefaultsOnOrder(Order newOrder)
		{
			if (!IsImportingData)
			{
				((IBusinessObjectInternals)newOrder).IsCopying = true;
				try
				{
					newOrder.BuyerPK = JE_OH_Importer;
					newOrder.SupplierPK = JE_OH_Supplier;

					newOrder.JD_TransportMode = JE_TransportMode;
					newOrder.JD_ContainerMode = FreightContainerMode;
					newOrder.JD_OrderGoodsDescription = JE_GoodsDescription.SubstringSafe(0, newOrder.JD_OrderGoodsDescriptionInfo.MaxLength);
					newOrder.JD_RS_NKServiceLevel_NI = JE_RS_NKServiceLevel;
					newOrder.JD_IncoTerm = JE_ShipmentIncoTerm;
					newOrder.JD_OH_SendingAgent = JE_OH_Forwarder;
					newOrder.JD_OH_ReceivingAgent = JE_OH_Forwarder;
				}
				finally
				{
					((IBusinessObjectInternals)newOrder).IsCopying = false;
				}
			}
		}

		public void OnOrderAttached(Order attachedOrder)
		{
			if (attachedOrder != null)
			{
				attachedOrder.RequiredDocuments.CopyToOtherCollection(DocsAndCartage.RequiredDocuments);
			}
		}

		#endregion

		#region UnRegisterEditableChildObjectsForMessageValidation

		public IDisposable UnRegisterEditableChildObjectsForMessageValidation()
		{
			// Accessing AttachedOrders to make sure that fAttachedOrders is initialized.
			_ = AttachedOrders;
			// fAttachedOrders is not the same collection as AttachedOrders (AttachedOrders is a limited clone of fAttachedOrders).
			var editableOrdersCollection = IsRegisteredEditableChildObject(fAttachedOrders) ? fAttachedOrders : null;

			return new DisposableAction(() =>
			{
				if (editableOrdersCollection != null)
				{
					UnRegisterEditableChildObject(editableOrdersCollection);
				}
			}, () =>
			{
				if (editableOrdersCollection != null)
				{
					RegisterEditableChildObject(editableOrdersCollection);
				}
			});
		}

		#endregion

		#region IJobNumber

		string IJobNumber.JobNumber
		{
			get { return JE_DeclarationReference; }
		}

		public void SetJobNumberFieldOnSaving()
		{
			PopulateJE_DeclarationReferenceIfNeeded();
		}

		public string JobNumber
		{
			get { return JE_DeclarationReference; }
		}

		#endregion

		public virtual ZString IncoTerm
		{
			get { return JE_ShipmentIncoTerm; }
		}

		public new OrgHeader Consignee
		{
			get { return Importer; }
		}

		public OrgHeader IntermConsignee
		{
			get { return base.Consignee; }
		}

		public OrgHeader Consignor
		{
			get { return Supplier; }
		}

		[ResourceStringData("FD9ECBC8-3454-4163-ACDE-523FA176E104", Caption = "Entry Release Date", MediumCaption = "Entry Release", ShortCaption = "Entry Rel.")]
		public ZString EntryReleaseDate
		{
			get
			{
				var dates = ActiveEntryHeaders.Cast<CusEntryHeader>().Select(d => d.CH_EntryReleaseDate).Where(d => !d.IsEmpty).Select(d => d.ToString(DateTimeFormatStrings.ShortDateFormat, CultureInfo.CurrentCulture));
				return dates.GetSingleValueOrManyText(d => d);
			}
		}

		#region IJobHeaderParent Members

		void IJobHeaderParent.OnJobCreating(JobHeader job)
		{
			// tested by IDtbBookingParentTestCase<T>.TestIfJobInvoicingPluginAndCanCreateInvoicingJobThenAddsChargesFromChildPortTransportJob() and TestIfJobInvoicingPluginAndCanCreateInvoicingJobThenAddsChargesFromGrandchildPortTransportJob()
			CartageHelper.AttachCartageJobsToParentJob(job, PK);
			// tested by IDtbBookingParentTestCase<T>.TestIfJobInvoicingPluginAndCanCreateInvoicingJobThenAddsChargesFromGrandchildLandTransportConsignment()
			ConsignmentJobHelper.AttachLTConsignmentJobsToParentJob(job, PK);
			AttachJobToParentJob(job);
		}

		void AttachJobToParentJob(JobHeader job)
		{
			if (CustomsDataRegistry.Instance.EnableInheritanceOfLinkedDeclarationsJobHeader.Value)
			{ 
				if (job != null && !job.IsInDatabase && job.JH_JH_ParentJob.IsEmpty)
				{
					if (!ClonedFromDeclarationReference.IsEmpty)
					{
						var parentDeclarationQuery = new ZQuery(JobDeclarationSchema.JE_DeclarationReference, ClonedFromDeclarationReference);
						parentDeclarationQuery.AddToFilter(JobDeclarationSchema.JE_GC, JE_GC);
						var parentDeclaration = Factory.LoadTop1<BaseJobDeclaration>(parentDeclarationQuery);
						if (parentDeclaration != null && parentDeclaration.IsInDatabase)
						{
							var parentDeclarationJob = parentDeclaration.Job;
							var grandparentDeclarationJob = parentDeclaration.ParentRelatedDeclaration?.Job;

							// If I have a grandparent, but my parent is not attached to my grandparent, then I should not attach myself.
							if (parentDeclarationJob != null && grandparentDeclarationJob != null && parentDeclarationJob.JH_JH_ParentJob.IsEmpty)
							{
								return;
							}

							// If any of my sisters is not attached to parent, I should not attach myself.
							if (parentDeclaration.RelatedDeclarations.Except(this).Any(x => x.Job != null && x.Job.JH_JH_ParentJob.IsEmpty))
							{
								return;
							}

							// If my parent is not attached to anyone, it means I have no grandparent, then I should attach to my parent.
							if (parentDeclarationJob.JH_JH_ParentJob.IsEmpty)
							{
								job.JH_JH_ParentJob = parentDeclarationJob.PK;
							}
							// Otherwise, I should attach to my ancestor, he could be my grandparent, or grandgrandgrandparent.
							else
							{
								job.JH_JH_ParentJob = parentDeclarationJob.JH_JH_ParentJob;
							}
						}
					}
				}
			}
		}

		void IJobHeaderParent.OnJobCreated(JobHeader job)
		{
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

		void IJobHeaderParent.OnJobDeleting(JobHeader job)
		{
		}

		void IJobHeaderParent.OnJobDeleted(JobHeader job)
		{
		}

		bool IJobHeaderParent.AllowInvoiceDeletion
		{
			get { return true; }
		}

		#endregion

		#region IJobInvoicingPlugIn Members

		BaseJobDeclarationInvoicingSupporter fInvoicingSupporter;
		public IJobInvoicingSupporter InvoicingSupporter
		{
			get { return fInvoicingSupporter ?? (fInvoicingSupporter = GetNewInvoicingSupporter()); }
		}

		protected virtual BaseJobDeclarationInvoicingSupporter GetNewInvoicingSupporter()
		{
			return new BaseJobDeclarationInvoicingSupporter(this);
		}

		#endregion

		#region IDocManagerSupport Members

		public DocManagerInfo DocManagerInfo
		{
			get
			{
				if (docManagerInfo == null)
				{
					docManagerInfo = GetNewDocManagerInfo();
				}
				return docManagerInfo;
			}
		}
		DocManagerInfo docManagerInfo;

		public bool IsDocManagerInfoLoaded => docManagerInfo != null;

		protected virtual DeclarationDocManagerInfo GetNewDocManagerInfo()
		{
			return new DeclarationDocManagerInfo(this);
		}

#if DEBUG
		void ResetCachedValuesForTest()
		{
			fCustomsEntryHeaders = null;
			fActiveEntryHeaders = null;
			fInvoices = null;
		}
#endif

		public BusinessObject LandedCostHeaderForDocuments
		{
			get { return (BusinessObject)Factory.LoadTop1<LandedCosting.ILandedCostHeader>(new LandedCostHeaderFilter(this)); }
		}

		public class DeclarationDocManagerInfo : DocManagerInfo, ShipmentDocManagerInfo.IHaveEDocsChildren
		{
			public DeclarationDocManagerInfo(BaseJobDeclaration parent)
				: base(parent, Core.Constants.DocManagerCodes.JobDeclaration)
			{
			}

			BaseJobDeclaration Declaration
			{
				get { return (BaseJobDeclaration)BusinessEntity; }
			}

			protected override JobHeader GetInvoicingJobHeader()
			{
				return (Declaration.Shipment != null) ? Declaration.Shipment.Job : new JobHeader.Loader(Declaration).Load();
			}

			protected override BusinessObject[] GetRelatedObjects()
			{
				var factory = BusinessEntity.Factory;
				var result = new List<BusinessObject>(GetEDocsChildrenForAFreightJobToDisplay());
				result.AddRange(new InvoiceLoader(factory).GetInvoicesForUniqueRef(Declaration.JE_DeclarationReference));

				if (Declaration.Consignee != null)
				{
					result.Add(Declaration.Consignee);
				}

				if (Declaration.Consignor != null)
				{
					result.Add(Declaration.Consignor);
				}

				if (Declaration is ICartageParent)
				{
					result.AddRange((BusinessObject[])factory.Load<ICommonCartage>(new ZQuery(JobCartageSchema.JJ_ParentID, Declaration.PK)));
				}

				result.AddRange(Declaration.CusContainers);
				result.AddRange(TransportBookingLoader.GetRelatedTransportBookingEDocs(Declaration));

				if (Declaration.AttachedOrders != null)
				{
					result.AddRange(Declaration.AttachedOrders);
				}

				var lcHeader = Declaration.LandedCostHeaderForDocuments;
				if (lcHeader != null)
				{
					result.Add(lcHeader);
				}

				var cusPackingList = Declaration.LoadCusPackingList(factory);
				if (cusPackingList != null)
				{
					result.Add(cusPackingList);
				}

				var quote = Quote;
				if (quote != null)
				{
					result.Add(quote);
				}

				return result.ToArray();
			}

			BusinessObject Quote
			{
				get
				{
					BusinessObject quote = null;

					if (Declaration.Job != null && !Declaration.Job.JH_TH_NKQuoteNumber.IsEmpty)
					{
						var query = new ZQuery(RatingHeaderSchema.TH_QuoteNumber, Declaration.Job.JH_TH_NKQuoteNumber);
						quote = Declaration.Factory.LoadTop1(ObjectFactory.Get<Integration.Rating.IRating>().QuoteType, query);
					}

					return quote;
				}
			}

			protected virtual BusinessObject[] GetEDocsChildrenForAFreightJobToDisplayCore()
			{
				var result = new List<BusinessObject>();
				foreach (CusEntryHeader cusEntryHeader in Declaration.CustomsEntryHeaders)
				{
					result.AddRange(cusEntryHeader.Messages);
				}

				foreach (var relatedDeclaration in Declaration.RelatedDeclarations)
				{
					result.Add(relatedDeclaration);
					result.AddRange(relatedDeclaration.ActiveEntryHeaders);
				}
				return result.ToArray();
			}

			public BusinessObject[] GetEDocsChildrenForAFreightJobToDisplay()
			{
#if DEBUG
				Declaration.ResetCachedValuesForTest();
#endif
				List<BusinessObject> result = new List<BusinessObject>();
				result.AddRange(Declaration.Invoices);
				result.AddRange(Declaration.CustomsEntryHeaders);

				if (Declaration.IsPluggedIntoShipment)
				{
					var lcHeader = Declaration.LandedCostHeaderForDocuments;
					if (lcHeader != null)
					{
						result.Add(lcHeader);
					}
				}
				result.AddRange(GetEDocsChildrenForAFreightJobToDisplayCore());
				return result.ToArray();
			}
		}

		#endregion

		#region ITemplateCopyable Members
		public IBusiness TemplateCopy()
		{
			return TemplateCopy(Factory);
		}

		public IBusiness TemplateCopy(BusinessObjectFactory alternateFactory)
		{
			return TemplateCopyCore(alternateFactory, CloneType.TemplateCopy);
		}

		protected virtual IBusiness TemplateCopyCore(BusinessObjectFactory alternateFactory, CloneType cloneType)
		{
			BaseJobDeclaration result = (BaseJobDeclaration)GetTemplateCopyStrategy(alternateFactory, cloneType).Clone();
			using (result.GetValidationSuspender())
			{
				ResetValuesOnTemplateCopyAfterClone(result, cloneType);
				result.LoadChildEditableObjects();
				UpdateAddInfoAfterReset(result);
			}
			result.HasChanges = false;

			return result;
		}

		void UpdateAddInfoAfterReset(IBusiness bizObj)
		{
			if (bizObj != null)
			{
				if (bizObj is BusinessObject businessObject && bizObj is IAddInfoManager addInfoManager
					&& addInfoManager.AddInfo is IAddInfo addInfo
					&& addInfo is BusinessObject addInfoBizObj)
				{
					using (businessObject.GetValidationSuspender())
					using (businessObject.SuspendSettingHasChanges())
					using (addInfoBizObj.GetValidationSuspender())
					using (addInfoBizObj.SuspendSettingHasChanges())
					{
						addInfo.UpdateRelatedPropertyInfo();
					}
				}

				foreach (var child in bizObj.Children)
				{
					if (child is IBusinessObjectCollection childCollection)
					{
						foreach (IBusiness grandChild in childCollection)
						{
							UpdateAddInfoAfterReset(grandChild);
						}
					}
					else
					{
						UpdateAddInfoAfterReset(child);
					}
				}
			}
		}

		protected virtual JobDeclarationDeepCloneStrategy GetTemplateCopyStrategy(BusinessObjectFactory alternateFactory, CloneType cloneType)
		{
			return new JobDeclarationDeepCloneStrategy(this, cloneType, alternateFactory);
		}

		protected virtual void ResetValuesOnTemplateCopyAfterClone(BaseJobDeclaration declaration, CloneType cloneType)
		{
			declaration.JE_ContainerCount = (short)declaration.CusContainers.Count;
			declaration.JE_IsCancelled = false;
			declaration.JE_AuditDateUtc = ZDateTime.Empty;
			declaration.JE_AuditReference = ZString.Empty;
			declaration.JE_GS_NKAuditUser = ZString.Empty;

			foreach (ZPropertyInfo propertyInfo in declaration.ZPropertyInfoHash)
			{
				if (propertyInfo.PropertyType == typeof(ZDateTime) || propertyInfo.PropertyType == typeof(ZDate))// && !PropertyInfo.Name.IndexOf("JobDocsAndCartageJP_") == -1)
				{
					if (propertyInfo.HasSetter)
					{
						propertyInfo.Value = propertyInfo.DefaultValue;
					}
				}
			}

			foreach (BaseJobComInvoiceGroupHeader groupInvoice in declaration.AllGroupHeaders)
			{
				using (groupInvoice.GetValidationSuspender())
				using (groupInvoice.SuspendSettingHasChanges())
				{
					groupInvoice.JZ_InvoiceDate = ZDateTime.Today;
				}
				groupInvoice.HasChanges = false;
			}
		}

		#endregion

		#region Related Declaration

		public BaseJobDeclaration GetNewRelatedDeclaration(BusinessObjectFactory factory, string relationshipType = null)
		{
			return GetNewRelatedDeclarationCore(factory, relationshipType);
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			Type typeToCloneAs = args.TypeToCloneAs ?? GetType();
			BusinessObjectFactory factory = args.AlternativeFactoryToInstantiateCloneIn ?? Factory;
			var result = (BaseJobDeclaration)factory.New(typeToCloneAs);
			using (result.GetValidationSuspender())
			using (result.SuspendSettingHasChanges())
			{
				result.IsCloning = true;
				result.CopyPersistentValuesFrom(this, args);
				if (result.IsSetAllDocumentsReceivedEventLoggerAfterClone)
				{
					result.DocsAndCartage.RequiredDocuments.SetAllDocumentsReceivedEventLogger();
				}
				CopyXmlColumns(result);
			}
			return result;
		}

		public bool IsSetAllDocumentsReceivedEventLoggerAfterClone => !JE_OH_Importer.IsEmpty || !JE_OH_Supplier.IsEmpty || !JE_RL_NKFinalDestination.IsEmpty || !JE_RL_NKOrigin.IsEmpty;

		public bool IsCloning { get; set; }

		protected virtual BaseJobDeclaration GetNewRelatedDeclarationCore(BusinessObjectFactory factory, string relationshipType = null)
		{
			var cloneType = CloneType.DeepTemplateCopy;
			var newDeclaration = (BaseJobDeclaration)TemplateCopyCore(factory, cloneType);
			 
			CleanUpNewDeclarationAfterClone(newDeclaration, cloneType);
			if (UseGenPivotForRelatedDeclarations)
			{
				RelatedDeclarations.Add(newDeclaration, relationshipType);  // This is cross factory???
			}

			return newDeclaration;
		}

		public void CleanUpNewDeclarationAfterClone(BaseJobDeclaration newDeclaration, CloneType cloneType)
		{
			CleanUpNewDeclarationAfterCloneCore(newDeclaration, cloneType);
		}

		protected virtual void CleanUpNewDeclarationAfterCloneCore(BaseJobDeclaration newDeclaration, CloneType cloneType)
		{
			newDeclaration.JE_DateOfArrival = JE_DateOfArrival;
			newDeclaration.JE_ExportDate = JE_ExportDate;
			newDeclaration.JE_GS_NKCusAgent = ZString.Empty;
			newDeclaration.ApportionmentDirty = true;
			newDeclaration.ClonedFromDeclarationReference = JE_DeclarationReference;
		}

		public ZString ClonedFromDeclarationReference { get; private set; }

		#endregion

		#region IImportExport

		public Directions JobDirection
		{
			get { return GetJobDirection(); }
		}

		protected virtual Directions GetJobDirection()
		{
			if (IsImport || IsMiscellaneous)
			{
				return Directions.Import;
			}

			if (IsExport)
			{
				return Directions.Export;
			}

			return Directions.Unknown;
		}

		public bool IsEntryInstructionRequired => Factory.GetValue(ref isEntryInstructionRequiredCached, () => IsEntryInstructionRequiredCore);
		CachedProperty<bool> isEntryInstructionRequiredCached;
		protected virtual bool IsEntryInstructionRequiredCore => IsPersistent && !IsInterface && !CustomsEntryInstructionProvider.IsNoEntryInstruction;

		public virtual ZBool IsInterface => JE_ApplicationCode == DeclarationApplicationCodeList.Codes.Interfaced;

		protected virtual bool AreTransportDetailsIrrelevant
		{
			get { return IsExWarehouse || IsDrawback; }
		}

		public virtual bool IsDeclarationByExternalBroker
		{
			get { return IsImportByExternalBroker || IsExportByExternalBroker; }
		}

		public virtual ZBool IsExport
		{
			get { return JE_MessageType == JobMessageTypeList.Codes.Export || IsExportByExternalBroker; }
		}

		public virtual bool IsExportByExternalBroker
		{
			get { return JE_MessageType == JobMessageTypeList.Codes.ExportDeclarationByExternalBroker; }
		}

		public virtual ZBool IsImport
		{
			get { return JE_MessageType == JobMessageTypeList.Codes.Import || IsWarehousedByExternalAgent || IsImportByExternalBroker; }
		}

		public virtual bool IsImportByExternalBroker
		{
			get { return JE_MessageType == JobMessageTypeList.Codes.ImportDeclarationByExternalBroker; }
		}

		public virtual bool ShouldAutoRatingForExternalBroker => true;

		public bool IsWarehousedByExternalAgent
		{
			get { return JE_MessageType == JobMessageTypeList.Codes.WarehousedByExternalAgent; }
		}

		public bool CurrentUserHasSecurityAccess
		{
			get
			{
				return IsExport && Env.Security.ExportEdit.IsAllowed
						 || !IsExport && Env.Security.ImportEdit.IsAllowed;
			}
		}

		public virtual bool AlwaysUseClassificationDescription
		{
			get { return JE_MergeBy == OrgConstants.MergeInvoiceLines.ClassificationUsingClassificationDescriptionAlways; }
		}

		public virtual bool PrefixPartDescriptionWithPartNumber
		{
			get
			{
				return JE_MergeBy == OrgConstants.MergeInvoiceLines.NotMergeUsingProductNumberInDescription
						 || JE_MergeBy == OrgConstants.MergeInvoiceLines.PartNumberUsingProductNumberInDescription;
			}
		}

		public ZBool IsRefund
		{
			get { return JE_MessageType == JobMessageTypeList.Codes.Refund; }
		}

		public ZBool IsMiscellaneous
		{
			get { return JE_MessageType == JobMessageTypeList.Codes.MiscellaneousCustoms; }
		}

		public ZBool IsDomestic
		{
			get { return ZBool.False; }
		}

		public ZBool IsOffshore
		{
			get { return ZBool.False; }
		}

		public virtual ZBool IsExWarehouse
		{
			get { return JE_MessageType == JobMessageTypeList.Codes.ExWarehouse; }
		}

		public virtual bool IsDrawback
		{
			get { return JE_MessageType == JobMessageTypeList.Codes.Drawback; }
		}

		public bool IsExportOrNonTransport => IsExport || IsNonTransportDeclarationType;

		public virtual bool IsNonTransportDeclarationType
		{
			get { return false; }
		}

		public bool IsFCLorFCX
		{
			get
			{
				return JE_ContainerMode == BaseCusContainer.ContainerModes.FullContainerLoad || JE_ContainerMode == BaseCusContainer.ContainerModes.FCX;
			}
		}

		#endregion

		#region InternalCartageManager

		//    protected InternalCartageRemoteAddressManagerForDeclaration InternalCartageRemoteAddressManager
		//    {
		//      get
		//      {
		//        if (fInternalCartageRemoteAddressManager == null)
		//        {
		//          fInternalCartageRemoteAddressManager = new InternalCartageRemoteAddressManagerForDeclaration(this);
		//        }

		//        return fInternalCartageRemoteAddressManager;
		//      }
		//    }
		//#if DEBUG
		//    protected
		//#endif
		// InternalCartageRemoteAddressManagerForDeclaration fInternalCartageRemoteAddressManager;

		#endregion

		#region IDocsAndCartageParent

		bool IHaveInternalCartage.IsForPickupCartage
		{
			get { return IsExport; }
		}

		ZGuid IHaveInternalCartage.BranchPK
		{
			get { return JE_GB; }
		}

		ZString IHaveInternalCartage.OwnerRef
		{
			get { return JE_OwnerRef; }
		}

		bool IHaveInternalCartage.IsAllowedToUpdateAdviseDates
		{
			get { return true; }
		}

		public virtual Type DocsAndCartageType
		{
			get { return typeof(ForwardingDocsAndCartage); }
		}

		public virtual Type DocsAndCartageParentType
		{
			get { return this.GetType(); }
		}

		IHaveRequiredDocuments IDocsAndCartageParent.RequiredDocumentsProvider
		{
			get { return DocsAndCartage; }
		}

		[UniversalCopyRelatedEntity(CommaSeparatedSkipPropertiesNames = "JP_ParentID,JP_ParentTableCode,JP_OrderItemsAsString", DisableCopyMethodLink = true)]
		public virtual JobDocsAndCartage DocsAndCartage
		{
			get
			{
				if (fDocsAndCartage == null)
				{
					fDocsAndCartage = JobDocsAndCartage.GetOrCreateDocsAndCartageFromParent(this);
					RegisterListChangedCalledRefreshBinding(fDocsAndCartage);
					SetupDataOnDocsAndCartageFirstSet();
					CompleteDocsAndCartageValueSettingsOnInitialisation();

					fDocsAndCartage.JP_OrderItemsAsStringInfo.ValueChanged -= OnOwnerRefNeedsUpdate;
					fDocsAndCartage.JP_OrderItemsAsStringInfo.ValueChanged += OnOwnerRefNeedsUpdate;
				}
				else if (fDocsAndCartage.IsDeleted)
				{
					UnRegisterListChangedCalledRefreshBinding(fDocsAndCartage);
					fDocsAndCartage.JP_OrderItemsAsStringInfo.ValueChanged -= OnOwnerRefNeedsUpdate;
					fDocsAndCartage = null;
				}

				if (DocsAndCartageAccessed != null && fDocsAndCartage != null)
				{
					DocsAndCartageAccessed(this, EventArgs.Empty);
				}

				return fDocsAndCartage;
			}
		}
		JobDocsAndCartage fDocsAndCartage;

		void OnOwnerRefNeedsUpdate(object sender, EventArgs e)
		{
			if (!IsRowDeletedOrDetachedOrNull && IsStandAlone && !AutoAssignImporterRef && IsPopulateOwnerRefWithOrderNumbersEnabled(Consignee))
			{
				UpdateOwnerRefFromOrderNumbers(true);
			}
		}

		internal bool IsPopulateOwnerRefWithOrderNumbersEnabled(OrgHeader consignee)
		{
			var result = false;

			var consigneeAutoPopulateOwnerRef = consignee?.MiscServ.OM_IMAutoPopulateOwnerRefWithOrderNums ?? ZString.Empty;
			if (consigneeAutoPopulateOwnerRef != PopulateOwnerRefList.Codes.No)
			{
				result = consigneeAutoPopulateOwnerRef == PopulateOwnerRefList.Codes.Yes || CustomsDataRegistry.Instance.PopulateOwnersRef.Value;
			}

			return result;
		}

		internal bool UpdateOwnerRefFromOrderNumbers(bool applyChanges)
		{
			var attachedOrders = AttachedOrders.Cast<Order>().Select(x => x.JD_OrderNumber).Where(x => !x.IsEmpty);
			var docOrders = DocsAndCartage.OrderItems.Cast<OrderItem>().Select(x => x.JT_OrderReference).Where(x => !x.IsEmpty);
			ZString combinedOrders = string.Join(", ", attachedOrders.Union(docOrders).OrderBy(x => x));

			var calculatedOwnerRef = combinedOrders.Left(JE_OwnerRefInfo.MaxLength).TrimEndIncludingWhiteSpace(',');
			bool isDifferent = !calculatedOwnerRef.IsEmpty && !JE_OwnerRef.EqualsIgnoringCase(calculatedOwnerRef);
			if (isDifferent && applyChanges)
			{
				JE_OwnerRef = calculatedOwnerRef;
			}

			return isDifferent;
		}

		void CompleteDocsAndCartageValueSettingsOnInitialisation()
		{
			if (fDocsAndCartage != null && !fDocsAndCartage.IsInDatabase)
			{
				using (DocsAndCartage.GetValidationSuspender())
				{
					if ((IsImport && Importer != null && DocsAndCartage.JP_FCLDeliveryEquipmentNeeded.IsEmpty)
						|| (IsExport && Supplier != null && DocsAndCartage.JP_FCLPickupEquipmentNeeded.IsEmpty))
					{
						DefaultCartageEquipment();
					}
				}
			}
		}

		public bool IsDocsAndCartageSet
		{
			get { return (fDocsAndCartage != null && !fDocsAndCartage.IsDeleted); }
		}

		protected virtual void SetupDataOnDocsAndCartageFirstSet()
		{
		}
		internal event EventHandler DocsAndCartageAccessed;

		protected void DeleteDocsAndCartage()
		{
			JobDocsAndCartage result = null;
			if (IsDocsAndCartageSet)
			{
				if (Shipment == null)
				{
					result = DocsAndCartage;
				}
			}
			else if (Shipment == null)
			{
				result = DocsAndCartage;
			}
			if (result != null)
			{
				UnRegisterEditableChildObject(result);
				result.Delete();
			}
			fDocsAndCartage = null;
		}

		Freight.Business.IContainer[] IHaveInternalCartage.GetContainers()
		{
			return (Freight.Business.IContainer[])this.CusContainers.JobContainers.Find(new ZQuery(JobContainerSchema.JC_ContainerMode, SQLComparisonOperator.NotEqual, Core.Constants.ContainerModes.Groupage));
		}

		IPackLineInfo[] IHaveInternalCartage.GetPackLines()
		{
			return (IPackLineInfo[])Packages.ToArray(typeof(IPackLineInfo));
		}

		public OrgHeader DeliveryCartageOrg
		{
			get { return DeliveryOrPickupCartageCo; }
		}

		public OrgHeader PickupCartageOrg
		{
			get { return DeliveryOrPickupCartageCo; }
		}

		ZString IHaveInternalCartage.ServiceLevel
		{
			get { return JE_RS_NKServiceLevel; }
		}

		public ZBool DeliveryAndPickupCartageApplicable
		{
			get { return ZBool.False; }
		}

		public OrgAddress DepotOrCTOAddress
		{
			get { return DepotDocAddress.Address ?? ContainerTerminalOperatorDocAddress.Address; }
		}

		ZGuid IHaveInternalCartage.CartagePickupDepotAddress
		{
			get { return DepotDocAddress.E2_OA_Address; }
		}

		ZGuid IHaveInternalCartage.CartageDeliveryDepotAddress
		{
			get { return DepotDocAddress.E2_OA_Address; }
		}

		ZGuid IHaveInternalCartage.CartagePickupCTOAddress
		{
			get { return ContainerTerminalOperatorDocAddress.E2_OA_Address; }
		}

		ZGuid IHaveInternalCartage.CartageDeliveryCTOAddress
		{
			get { return ContainerTerminalOperatorDocAddress.E2_OA_Address; }
		}

		ZGuid IHaveInternalCartage.CartagePickupContainerYardAddress
		{
			get { return ContainerYardDocAddress.E2_OA_Address; }
		}

		ZGuid IHaveInternalCartage.CartageDeliveryContainerYardAddress
		{
			get { return ContainerYardDocAddress.E2_OA_Address; }
		}

		JobDocAddress IHaveInternalCartage.CartageExporterDocAddress
		{
			get { return SupplierPickupAddress; }
		}

		JobDocAddress IHaveInternalCartage.CartageImporterDocAddress
		{
			get { return ImporterDeliveryAddress; }
		}

		ZPropertyInfo IHaveInternalCartage.CartagePickupDepotAddressInfo
		{
			get { return DepotDocAddress.E2_OA_AddressInfo; }
		}

		ZPropertyInfo IHaveInternalCartage.CartageDeliveryDepotAddressInfo
		{
			get { return DepotDocAddress.E2_OA_AddressInfo; }
		}

		ZPropertyInfo IHaveInternalCartage.CartagePickupCTOAddressInfo
		{
			get { return ContainerTerminalOperatorDocAddress.E2_OA_AddressInfo; }
		}

		ZPropertyInfo IHaveInternalCartage.CartageDeliveryCTOAddressInfo
		{
			get { return ContainerTerminalOperatorDocAddress.E2_OA_AddressInfo; }
		}

		ZPropertyInfo IHaveInternalCartage.CartagePickupContainerYardAddressInfo
		{
			get { return ContainerYardDocAddress.E2_OA_AddressInfo; }
		}

		ZPropertyInfo IHaveInternalCartage.CartageDeliveryContainerYardAddressInfo
		{
			get { return ContainerYardDocAddress.E2_OA_AddressInfo; }
		}

		ZString IHaveInternalCartage.TransportMode
		{
			get { return TransportModeGeneric; }
		}

		ZString IHaveInternalCartage.ContainerMode
		{
			get { return CartageContainerMode; }
		}

		ZString CartageContainerMode
		{
			get
			{
				if (Shipment != null)
				{
					return ((IHaveInternalCartage)Shipment).ContainerMode; // i.e. JS_PackingMode
				}

				var result = GetDefaultCartageContainerMode;

				if (JE_ContainerMode == BaseCusContainer.ContainerModes.FullContainerLoad
					|| JE_ContainerMode == BaseCusContainer.ContainerModes.FCX)
				{
					result = Enterprise.Core.Constants.ContainerModes.FCL;
				}
				else if (JE_ContainerMode == BaseCusContainer.ContainerModes.LessContainerLoad)
				{
					result = Enterprise.Core.Constants.ContainerModes.LCL;
				}
				else if (JE_ContainerMode == BaseCusContainer.ContainerModes.BreakBulk)
				{
					result = Enterprise.Core.Constants.ContainerModes.BreakBulk;
				}
				else
				{
					foreach (BaseCusContainer container in CusContainers)
					{
						result = Enterprise.Core.Constants.ContainerModes.LCL;

						if (container.CO_FCL_LCL_AIR == BaseCusContainer.ContainerModes.FullContainerLoad
							|| container.CO_FCL_LCL_AIR == BaseCusContainer.ContainerModes.FCX)
						{
							result = Enterprise.Core.Constants.ContainerModes.FCL;
							break;
						}
					}
				}

				return result;
			}
		}

		protected virtual ZString GetDefaultCartageContainerMode => string.Empty;

		ZString IHaveInternalCartage.CartageTypeOverride
		{
			get { return ZString.Empty; }
		}

		bool IHaveInternalCartage.TypeSpecificPreCreationCheck()
		{
			return true;
		}

		ZBool IHaveInternalCartage.PackedAtDepot
		{
			get { return false; }
		}

		public ZBool InternalCartageEnabled
		{
			get { return IsImport; }
		}

		event EventHandler IShipmentWithDocsAndCartage.TransportModeChanged
		{
			add { JE_TransportModeInfo.ValueChanged += value; }
			remove { JE_TransportModeInfo.ValueChanged -= value; }
		}

		bool IShipmentWithDocsAndCartage.ShouldValidateDeliveryAndPickupCartageCoBeingSame()
		{
			return false;
		}

		bool IShipmentWithDocsAndCartage.ShouldValidateDeliveryCoPK()
		{
			return true;
		}

		bool IShipmentWithDocsAndCartage.RequiresOrderNumbersOnDocs()
		{
			return RequiresOrderNumbersOnDocsCore();
		}

		protected virtual bool RequiresOrderNumbersOnDocsCore()
		{
			return (Consignee?.MiscServ is OrgMiscServ consigneeMiscServ && consigneeMiscServ.OM_IMImporterRequiresOrderNumbersOnDocs)
					 || (Consignor?.MiscServ is OrgMiscServ consignorMiscServ && consignorMiscServ.OM_EXExporterRequiresOrderNumbersOnDocs);
		}

		bool IShipmentWithDocsAndCartage.RequiresOrderTrackLink()
		{
			return RequiresOrderTrackLinkCore();
		}

		protected virtual bool RequiresOrderTrackLinkCore()
		{
			return (Consignee?.MiscServ is OrgMiscServ consigneeMiscServ && consigneeMiscServ.OM_IMJobRequireOrderTrackLink) ||
					 (Consignor?.MiscServ is OrgMiscServ consignorMiscServ && consignorMiscServ.OM_EXJobRequireOrderTrackLink);
		}

		JobDocAddress IShipmentWithDocsAndCartage.ConsignorDocumentaryAddress
		{
			get { return SupplierDocumentaryAddress; }
		}

		JobDocAddress IShipmentWithDocsAndCartage.ConsigneeDocumentaryAddress
		{
			get { return ImporterDocumentaryAddress; }
		}

		JobDocsAndCartageValidation IShipmentWithDocsAndCartage.PiggyBackedValidation
		{
			get { return PiggyBackedValidationCore; }
		}

		protected virtual JobDocsAndCartageValidation PiggyBackedValidationCore
		{
			get { return null; }
		}

		string IShipmentWithDocsAndCartage.UniqueConsignRef
		{
			get { return JE_DeclarationReference; }
		}

		string IShipmentWithDocsAndCartage.MasterBillNumber
		{
			get { return JE_MasterBill; }
		}

		string IShipmentWithDocsAndCartage.HouseBillNumber
		{
			get { return JE_HouseBill; }
		}

		OrgHeader IShipmentWithDocsAndCartage.ExportBroker
		{
			get { return null; }
		}

		ZPropertyInfo IHaveInternalCartage.DeliveryCartageAdvisedInfo
		{
			get { return DocsAndCartage.JP_DeliveryCartageAdvisedInfo; }
		}

		ZPropertyInfo IHaveInternalCartage.PickupCartageAdvisedInfo
		{
			get { return DocsAndCartage.JP_PickupCartageAdvisedInfo; }
		}

		#endregion

		#region Services

		[UniversalCopyCollectionEntity(JobService.Schema.TableName, JobService.Schema.ES_ParentID, JobService.Schema.ES_ParentTableCode)]
		public JobServiceDependentCollection Services => DocsAndCartage.Services;

		#endregion

		#region Events

		public event CusContainersToPrintEventHandler OnGetCusContainersToPrint;

		public void RaiseOnGetCusContainersToPrint(CusContainersToPrintEventArgs e)
		{
			OnGetCusContainersToPrint?.Invoke(this, e);
		}

		#endregion

		#region Implementation

		void EnsureThatJE_GBBelongsToSameCompany()
		{
			if (!ensureThatJE_GCIsInSyncWithJE_GBInProgress)
			{
				try
				{
					ensureThatJE_GCIsInSyncWithJE_GBInProgress = true;
					if (Company is GlbCompany company && company.Branches.FindByPK(JE_GB) == null)
					{
						JE_GB = company.ActiveBranches.OrderBy(b => b.GB_Code).FirstOrDefault()?.PK ?? ZGuid.Empty;
					}
				}
				finally
				{
					ensureThatJE_GCIsInSyncWithJE_GBInProgress = false;
				}
			}
		}

		void EnsureThatJE_GCIsSameAsBranchCompany()
		{
			if (!ensureThatJE_GCIsInSyncWithJE_GBInProgress)
			{
				try
				{
					ensureThatJE_GCIsInSyncWithJE_GBInProgress = true;
					if (Branch is GlbBranch branch)
					{
						var branchGC = branch.GB_GC;
						if (JE_GC != branchGC)
						{
							JE_GC = branchGC;
						}
					}
				}
				finally
				{
					ensureThatJE_GCIsInSyncWithJE_GBInProgress = false;
				}
			}
		}
		bool ensureThatJE_GCIsInSyncWithJE_GBInProgress;

		bool gettingBillsInProgress;
		bool gettingInvoicesInProgress;
		bool gettingInvoiceLinesInProgress;
		[SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference")]
		protected void EnsureNoStackOverflow(ref bool inProgress, string typeBeingCreated, Action createData)
		{
			if (inProgress)
			{
				ErrorReporter.ReportOnce(System.FormattableString.Invariant($"{typeBeingCreated} is being called while being created; this will cause stack overflow."));
			}
			else
			{
				try
				{
					inProgress = true;
					createData();
				}
				finally
				{
					inProgress = false;
				}
			}
		}

		internal protected virtual bool IsInvoiceQuantityRequiredForBondedWarehouse
		{
			get { return false; }
		}

		internal protected virtual bool IsBondedWhsQuantityRequiredForBondedWarehouse
		{
			get { return true; }
		}

		internal protected virtual bool IsAllocatedQuantityRequiredForBondedWarehouse
		{
			get { return false; }
		}

		protected MessageTypeBasedValueDefaulter MessageTypeBasedValueDefaulter
		{
			get { return messageTypeBasedValueDefaulter ?? (messageTypeBasedValueDefaulter = GetNewMessageTypeBasedValueDefaulter()); }
		}
		MessageTypeBasedValueDefaulter messageTypeBasedValueDefaulter;

		protected virtual MessageTypeBasedValueDefaulter GetNewMessageTypeBasedValueDefaulter()
		{
			return new MessageTypeBasedValueDefaulter(this);
		}

		internal protected virtual bool IsUNDGSupportedOnInvoiceLines
		{
			get { return false; }
		}

		protected bool IsSettingDefaultValues
		{
			get { return settingDefaultValuesIndex > 0; }
		}
		byte settingDefaultValuesIndex;

		protected IDisposable SetSettingDefaultValuesInProgress()
		{
			return new SettingDefaultValuesSetter(this);
		}

		sealed class SettingDefaultValuesSetter : IDisposable
		{
			internal SettingDefaultValuesSetter(BaseJobDeclaration bizObj)
			{
				this.bizObj = bizObj;
				bizObj.settingDefaultValuesIndex++;
			}

			readonly BaseJobDeclaration bizObj;

			public void Dispose()
			{
				bizObj.settingDefaultValuesIndex--;
			}
		}

		EDIMessageCollection fMessages;
		IJobComInvoiceGroupHeaderCollection<BaseJobComInvoiceGroupHeader> fJobComInvoiceGroupHeaders;
		ICusContainerCollection<BaseCusContainer> fCusContainers;
		ICusEntryHeaderCollection<CusEntryHeader> fCustomsEntryHeaders;
		protected bool fIsBulkDataLoading;

		#region Port Delivery Time

		protected GlbPortDeliveryTime DefaultPortDeliveryTime
		{
			get
			{
				return new GlbPortDeliveryTime.Loader(Factory, PortOfArrival, FinalDestination, Importer, FreightModeForPortDeliveryTimes, ZBool.False).Load();
			}
		}

		#endregion

		protected virtual void DefaultOriginFromSupplier()
		{
			if (Supplier != null && !IsDataSyncFromShipment)
			{
				var supplierPort = Supplier.OH_RL_NKClosestPort;

				if ((supplierPort.StartsWith(CountryCode, StringComparison.CurrentCulture) || !IsExport) && !supplierPort.IsEmpty && !JE_RL_NKPortOfLoadingInfo.ReadOnly)
				{
					if (JE_RL_NKPortOfLoading.IsEmpty && !AreTransportDetailsIrrelevant)
					{
						JE_RL_NKPortOfLoading = supplierPort;
					}

					JE_RL_NKOrigin = supplierPort;
				}
			}
		}

		protected virtual IJobComInvoiceGroupHeaderCollection<BaseJobComInvoiceGroupHeader> CreateNewJobComInvoiceGroupHeaderCollection()
		{
			return new BaseJobComInvoiceGroupHeaderCollection<BaseJobComInvoiceGroupHeader>(this);
		}

		protected virtual ICusContainerCollection<BaseCusContainer> NewCusContainersCollection()
		{
			return new BaseCusContainerCollection<BaseCusContainer>(this, Factory);
		}

		protected virtual ICusEntryHeaderCollection<CusEntryHeader> NewCustomsEntryHeaders() => new CusEntryHeaderCollection<CusEntryHeader>(this, Factory);

		protected ZQuery MessageFilter
		{
			get
			{
				var result = new ZQuery(EDIMessageSchema.EM_LinkUniqueID, PK);

				var applicationCodes = JobDeclarationMessageCollectionApplicationCodeListCore;
				if (applicationCodes.Any())
				{
					result.AddToFilter(EDIMessageSchema.EM_ApplicationCode, applicationCodes);
				}
				else
				{
					var applicationFilter = new ZQuery();
					foreach (string code in ApplicationCodeList.GetSystemApplicationCodes())
					{
						applicationFilter.AddToFilter(JoinCondition.And, EDIMessageSchema.EM_ApplicationCode, SQLComparisonOperator.NotEqual, code);
					}

					result.AddToFilter(applicationFilter);
				}

				return result;
			}
		}

		/// <summary>
		/// Override this to tell your EDIMessageCollection which EM_ApplicationCodes to show; i.e. don't show universal XML etc
		/// This does not get used for CusEntryHeader.Messages
		/// </summary>
		protected virtual IEnumerable<ZString> JobDeclarationMessageCollectionApplicationCodeListCore
		{
			get { return Array.Empty<ZString>(); }
		}

		protected virtual void DefaultFinalDestinationPortFromImporter()
		{
			if (Importer != null && !IsDataSyncFromShipment)
			{
				var importerPort = Importer.OH_RL_NKClosestPort;

				if ((importerPort.StartsWith(CountryCode, StringComparison.CurrentCulture) || !IsImport) && !importerPort.IsEmpty && !JE_RL_NKFinalDestinationInfo.ReadOnly)
				{
					JE_RL_NKFinalDestination = importerPort;
					if (JE_RL_NKPortOfArrival.IsEmpty && !AreTransportDetailsIrrelevant)
					{
						JE_RL_NKPortOfArrival = importerPort;
					}
				}
			}
		}

		protected internal virtual ZString ContainerModeForCartage => JE_ContainerMode;

		ZGuid DefaultImportCartage
		{
			get
			{
				var result = ZGuid.Empty;

				if (!IsCopying)
				{
					if (!JE_TransportMode.IsEmpty && Importer != null)
					{
						var containerMode = IsSea && ShouldDefaultFCLCartageCo ? (ZString)Constants.ContainerModes.FCL : ContainerModeForCartage;
						var location = ((IDocAddress)ImporterDeliveryAddress)?.E2_PortCode ?? ZString.Empty;
						var cartageOrg = Importer.GetRelatedParty(RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Delivery, JE_TransportMode, containerMode, location);
						result = cartageOrg?.PK ?? ZGuid.Empty;
					}

					if (result.IsEmpty)
					{
						var finalDestination = FinalDestination;
						var destinationBranch = finalDestination != null ? GlbCompany.CurrentCompany.FirstBranchForUnLoco(finalDestination) : null;
						if (destinationBranch != null)
						{
							var destinationBranchPK = destinationBranch.PK.ToGuid();

							if (IsAir)
							{
								result = FreightDataRegistry.Instance.AIRCartageCompany.GetValueWithoutFallback(Guid.Empty, destinationBranchPK, Guid.Empty);
							}
							else if (IsSea)
							{
								result = ShouldDefaultFCLCartageCo
										? FreightDataRegistry.Instance.FCLCartageCompany.GetValueWithoutFallback(Guid.Empty, destinationBranchPK, Guid.Empty)
										: FreightDataRegistry.Instance.LCLCartageCompany.GetValueWithoutFallback(Guid.Empty, destinationBranchPK, Guid.Empty);
							}
						}
					}
				}
				return result;
			}
		}

		ZGuid DefaultExportCartage
		{
			get
			{
				var result = ZGuid.Empty;

				if (!IsCopying)
				{
					if (!JE_TransportMode.IsEmpty && Supplier != null)
					{
						var containerMode = IsSea && ShouldDefaultFCLCartageCo ? (ZString)Constants.ContainerModes.FCL : ContainerModeForCartage;
						var cartageOrg = Supplier.GetRelatedParty(RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Pickup, JE_TransportMode, containerMode);
						result = cartageOrg?.PK ?? ZGuid.Empty;
					}

					if (result.IsEmpty)
					{
						var origin = Origin;
						var originBranch = origin != null ? GlbCompany.CurrentCompany.FirstBranchForUnLoco(origin) : null;
						if (originBranch != null)
						{
							var originBranchPK = originBranch.PK.ToGuid();

							if (IsAir)
							{
								result = FreightDataRegistry.Instance.AIRCartageCompany.GetValueWithoutFallback(Guid.Empty, originBranchPK, Guid.Empty);
							}
							else if (IsSea)
							{
								result = ShouldDefaultFCLCartageCo
										? FreightDataRegistry.Instance.FCLCartageCompany.GetValueWithoutFallback(Guid.Empty, originBranchPK, Guid.Empty)
										: FreightDataRegistry.Instance.LCLCartageCompany.GetValueWithoutFallback(Guid.Empty, originBranchPK, Guid.Empty);
							}
						}
					}
				}
				return result;
			}
		}

		public override ZDateTime JE_EntrySubmittedDate
		{
			get { return base.JE_EntrySubmittedDate; }
			set
			{
				bool hasChanged = JE_EntrySubmittedDate != value;
				if (CustomsEntryHeaders.Count > 0)
				{
					using (new MergeManager.ChangingMergedDeclarationInAWayThatDoesNotRequireReMerge(this))
					{
						base.JE_EntrySubmittedDate = value;
					}
				}
				else
				{
					base.JE_EntrySubmittedDate = value;
				}
				if (hasChanged)
				{
					InvoiceLines.MarkAsNeedingValidation();
				}
			}
		}
		protected virtual void PopulateEntrySubmittedDate(ZDateTime value)
		{
			if (value.IsEmpty || JE_EntrySubmittedDate.IsEmpty)
			{
				JE_EntrySubmittedDate = value;
			}
		}

		#region CurrencyConversion

		public Money ConvertToLocalAmount(ZDecimal amount, RefCurrency currency)
		{
			Money result = null;

			if (currency != null)
			{
				Money moneyAmount = new Money(amount, currency);
				result = ConvertToLocalAmount(moneyAmount);
			}

			return result;
		}

		Money ConvertToLocalAmount(Money amount)
		{
			return ((ICurrencyConverterProvider)this).CurrencyConverter.ConvertRounded(amount, LocalCurrency);
		}

		#endregion

		bool HasContainersOfMode(ZString containerMode)
		{
			foreach (BaseCusContainer container in CusContainers)
			{
				if (container.CO_FCL_LCL_AIR == containerMode)
				{
					return true;
				}
			}
			return false;
		}

		#endregion

		#region IDocumentSupportable Members

		public string TransportMode
		{
			get { return JE_TransportMode; }
		}

		public string ContainerMode
		{
			get { return JE_ContainerMode; }
		}

		public DocumentSupporter DocumentSupporter
		{
			get { return documentSupporter ?? (documentSupporter = CreateNewDocumentSupporter()); }
		}

		DocumentSupporter documentSupporter;

		protected virtual DocumentSupporter CreateNewDocumentSupporter()
		{
			return new BaseJobDeclarationDocumentSupporter(this);
		}

		#endregion

		#region ICancellable Members

		public override string CanCancel()
		{
			var result = JobHeaderParentDeletionHelper.CheckIfCanCancelJobHeaderParent(PK, HumanReadableName);
			if (!string.IsNullOrEmpty(result))
			{
				return result;
			}

			if (!DoesCustomsEntryStatusAllowCancellation)
			{
				return cancellationMessage;
			}

			if (!checkingCanCancelRelatedObjects)
			{
				checkingCanCancelRelatedObjects = true;
				try
				{
					var shipment = Shipment;
					if (shipment != null)
					{
						var canCancelShipment = shipment.CanCancel();
						if (!string.IsNullOrEmpty(canCancelShipment))
						{
							return Res.GetString(
								"7E6CC523-FF15-4F1D-8A54-5AE4BD862FC7",
								"This record cannot be deactivated as its parent host record cannot be deactivated due to the following reason.") +
							System.Environment.NewLine + shipment.HumanReadableName + ": " + canCancelShipment;
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
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "CA & AU")]
		public const string cancellationMessage = "This declaration must not be deactivated, due to the Customs Entry Status or it is awaiting a reply";

		protected virtual bool DoesCustomsEntryStatusAllowCancellation
		{
			get { return !DeclarationMessagesHaveBeenSent(); }
		}

		public sealed override string CanReactivate() => Logs.HasLogWith(StmALogSchema.SL_SE_NKEvent, Events.JobDeactivatedBySystemCode) ?
			Res.GetString("0FDF4C21-0718-47CE-AD42-6A97C60355C3", "This Job has been deactivated by WiseTech Global and cannot be reactivated.")
			: CanReactivateCore();

		protected virtual string CanReactivateCore() => null;

		public override bool IsCancelledHasChanged
		{
			get { return JE_IsCancelledInfo.HasChanges; }
		}

		#region JE_IsCancelled

		public override ZBool JE_IsCancelled
		{
			get { return base.JE_IsCancelled; }
			set
			{
				if (base.JE_IsCancelled != value)
				{
					base.JE_IsCancelled = value;
					UpdateReadOnlyForWhenCancelled();
					if (Shipment != null)
					{
						Shipment.IsCancelled = value;
					}

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
					relatedCancellableDataSupporter = ObjectFactory.Get<IDeclarationRelatedCancellableDataSupporter>();
				}

				return relatedCancellableDataSupporter;
			}
		}
		IRelatedCancellableDataSupporter relatedCancellableDataSupporter;

		#endregion

		public override void OnLoaded()
		{
			base.OnLoaded();

			if (JE_IsCancelled)
			{
				UpdateReadOnlyForWhenCancelled();
			}
		}

		void UpdateReadOnlyForWhenCancelled()
		{
			if (!IsDeleted)
			{
				readOnlyIncludingChildrenNeedsRecalculation = !JE_IsCancelled;
				this.SetCountedReadOnlyIncludingChildren(JE_IsCancelled);
			}
		}

		public override void RegisterEditableChildObject(IBusiness child)
		{
			base.RegisterEditableChildObject(child);
			if (!IsDeleted && JE_IsCancelled)
			{
				child.IncrementReadOnlyIncludingChildren();
			}
		}

		#endregion

		#region IInvoiceParent Members

		BaseJobDeclaration IDeclarationProvider.Declaration
		{
			get { return this; }
		}

		SchemaGuidColumn IInvoiceParent.ForeignKeyInInvoiceToParent
		{
			get { return JobComInvoiceHeaderSchema.JZ_JE; }
		}

		#endregion

		#region ICDArchive Members

		public CDArchiveInfo CDArchiveInfo
		{
			get { return new DeclarationCDArchiveInfo(this); }
		}

		public class DeclarationCDArchiveInfo : CDArchiveInfo
		{
			public DeclarationCDArchiveInfo(BaseJobDeclaration declaration)
				: base(declaration)
			{
			}

			BaseJobDeclaration Declaration
			{
				get { return (BaseJobDeclaration)BusinessEntity; }
			}

			public override ZString ConsigneeCode
			{
				get { return (Declaration.Consignee != null) ? Declaration.Consignee.OH_Code : ZString.Empty; }
			}

			public override ZString ConsignorCode
			{
				get { return (Declaration.Consignor != null) ? Declaration.Consignor.OH_Code : ZString.Empty; }
			}

			public override ZString[] ContainerNumbersList
			{
				get { return Declaration.ContainerNumbersListCore; }
			}

			public override ZString Destination
			{
				get { return Declaration.JE_RL_NKFinalDestination; }
			}

			public override ZString[] EntryNumbersList
			{
				get
				{
					List<ZString> result = new List<ZString>();
					AddToList(result, Declaration.DeclarationNumber);
					foreach (CusEntryHeader header in Declaration.CustomsEntryHeaders)
					{
						AddToList(result, header.EntryNumber);
					}
					return result.ToArray();
				}
			}

			public override ZDateTime ETA
			{
				get { return Declaration.JE_DateOfArrival; }
			}

			public override ZDateTime ETD
			{
				get { return Declaration.JE_ExportDate; }
			}

			public override ZString VoyageFlight
			{
				get { return Declaration.JE_VoyageFlightNo; }
			}

			public override ZString HouseBill
			{
				get { return Declaration.JE_HouseBill; }
			}

			public override ZString[] InvoiceNumbersList
			{
				get { return Declaration.InvoiceNumbersListCore; }
			}

			public override ZString JobNumber
			{
				get { return Declaration.JE_DeclarationReference; }
			}

			public override ZString MasterBill
			{
				get { return Declaration.JE_MasterBill; }
			}

			public override ZString[] OrderNumbersList
			{
				get
				{
					List<ZString> result = new List<ZString>();
					AddToList(result, Declaration.JE_OwnerRef);

					if (Declaration.Shipment == null)
					{
						var docsAndCartage = Declaration.DocsAndCartage;
						if (docsAndCartage != null)
						{
							AddToList(result, docsAndCartage.JP_OrderItemsAsString);
						}
					}

					AddToList(result, ForwardingShipment.ForwardingShipmentCDArchiveInfo.GetOrderAndReferenceNumbers(Declaration.Shipment, Declaration));
					return result.ToArray();
				}
			}

			public override ZString Origin
			{
				get { return Declaration.JE_RL_NKOrigin; }
			}

			public override ZString Vessel
			{
				get
				{
					return Declaration.JE_VesselName;
				}
			}
		}

		internal ZString[] ContainerNumbersListCore
		{
			get
			{
				var containers = new ZString[CusContainers.Count];

				for (int i = 0; i < CusContainers.Count; i++)
				{
					containers[i] = CusContainers[i].CO_ContainerNumber;
				}

				return containers;
			}
		}

		internal ZString[] InvoiceNumbersListCore
		{
			get
			{
				var transactions = new InvoiceLoader(Factory).GetInvoicesForUniqueRef(new ZGuid[] { JE_OH_Importer }, JE_DeclarationReference);
				var headers = new ZString[transactions.Count];

				for (int i = 0; i < transactions.Count; i++)
				{
					headers[i] = ObjectFactory.Get<IAccounting>().UseJobNumberBasedInvoiceNumbers ? transactions[i].AH_ConsolidatedInvoiceRef : transactions[i].AH_TransactionNum;
				}
				return headers;
			}
		}

		#endregion

		#region ILandedCostHeader Members

		ZBool ILandedCostHeader.SupportsNoCostApportionmentItem => false;

		ZDecimal ILandedCostHeader.DefaultEstimatedDutyPercent
		{
			get { return ZDecimal.Zero; }
		}

		bool ILandedCostHeader.IsAir
		{
			get { return IsAir; }
		}

		ZString ILandedCostHeader.JobNumber
		{
			get { return JE_DeclarationReference; }
		}

		ZDate ILandedCostHeader.DateOfEntry
		{
			get { return (ZDate)JE_EntrySubmittedDate; }
		}

		ZString ILandedCostHeader.LandedCostType
		{
			get
			{
				return LandedCostType.Actual;
			}
		}

		ZString ILandedCostHeader.TableCode
		{
			get { return JobDeclarationSchema.Constants.Prefix; }
		}

		bool ILandedCostHeader.IsLCSupported
		{
			get { return IsImport; }
		}

		public static string BaseMessageShownWhenLCIsNotSupported
		{
			get { return Res.GetString("f6bb797e-86ca-4eb3-8d1d-f30168d5e8a2", "Landed Costing is only supported on Import entries."); }
		}

		string ILandedCostHeader.MessageShownWhenLCIsNotSupported
		{
			get { return BaseMessageShownWhenLCIsNotSupported; }
		}

		bool ILandedCostHeader.IsJobInLCRunnableState
		{
			get { return IsEntryClear; }
		}

		ZString ILandedCostHeader.UniqueReferenceNumber
		{
			get { return JE_DeclarationReference; }
		}

		public bool IsEntryClear
		{
			get { return IsEntryClearCore; }
		}

		protected virtual bool IsEntryClearCore
		{
			get { return JE_EntryStatus == "CLR"; }
		}

		OrgHeader ILandedCostHeader.Consignee
		{
			get { return Consignee; }
		}

		event EventHandler ILandedCostHeader.OnLCSupportedChanged
		{
			add
			{
				JE_MessageTypeInfo.ValueChanged += value;
			}
			remove
			{
				JE_MessageTypeInfo.ValueChanged -= value;
			}
		}

		event EventHandler ILandedCostHeader.OnExchangeRateHolderDeleted
		{
			add
			{
				InvoiceDeleteEvent.AddInvoiceDeletedEventHandler(Factory, value);
			}
			remove
			{
				InvoiceDeleteEvent.RemoveInvoiceDeletedEventHandler(Factory, value);
			}
		}

		IEnumerable<ILandedCostChargeHolder> ILandedCostHeader.ChargeHolders
		{
			get
			{
				foreach (ILandedCostChargeHolder chargeHolder in AllGroupHeaders)
				{
					yield return chargeHolder;
				}

				foreach (ILandedCostChargeHolder chargeHolder in Invoices)
				{
					yield return chargeHolder;
				}

				foreach (ILandedCostChargeHolder chargeHolder in InvoiceLines)
				{
					yield return chargeHolder;
				}

				ILandedCostChargeHolder jobInvoicingCached = JobInvoicing;
				if (jobInvoicingCached != null)
				{
					yield return jobInvoicingCached;
				}
			}
		}

		IEnumerable<ILandedCostDistributeTo> ILandedCostHeader.CandidatesToDistributeCostTo
		{
			get
			{
				foreach (ILandedCostDistributeTo chargeHolder in AllGroupHeaders)
				{
					yield return chargeHolder;
				}

				foreach (ILandedCostDistributeTo chargeHolder in Invoices)
				{
					yield return chargeHolder;
				}

				if (IsSea)
				{
					foreach (ILandedCostDistributeTo chargeHolder in CusContainers)
					{
						yield return chargeHolder;
					}
				}

				foreach (BaseJobComInvoiceLine invoiceLine in InvoiceLines)
				{
					yield return invoiceLine;
				}
			}
		}

		IEnumerable<IUltimateDistributee> ILandedCostHeader.UltimateDistributees
		{
			get { return new TypedEnumerable<IUltimateDistributee>(InvoiceLines); }
		}

		IEnumerable<ILandedCostExchangeRateHolder> ILandedCostHeader.ExchangeRateHolders
		{
			get
			{
				return new TypedEnumerable<ILandedCostExchangeRateHolder>(Invoices);
			}
		}

		internal LCGSTBackRoundingCalculator LCGSTBackRoundingCalculator
		{
			get { return lcGSTBackRoundingCalculator ?? (lcGSTBackRoundingCalculator = new LCGSTBackRoundingCalculator(this)); }
		}
		LCGSTBackRoundingCalculator lcGSTBackRoundingCalculator;

		DutyTaxEntryFee ILandedCostHeader.TotalDutyTaxEntryFeeItems
		{
			get
			{
				if (((ILandedCostHeader)this).LandedCostType == LandedCostType.Actual)
				{
					if (cachedTotalDutyTaxEntryFeeItems == null)
					{
						var landedCostingHelper = GetLandedCostingHelper();
						if (landedCostingHelper != null)
						{
							cachedTotalDutyTaxEntryFeeItems = new CachedProperty<DutyTaxEntryFee>(Factory, () => landedCostingHelper.GetTotalDutyTaxEntryFeeItems(this));
						}
						else
						{
							ErrorReporter.ReportOnce(ILandedCostHeaderMustBeImplementedMessage, ILandedCostHeaderMustBeImplementedMessage);
						}
					}
					if (cachedTotalDutyTaxEntryFeeItems != null)
					{
						return cachedTotalDutyTaxEntryFeeItems.Value;
					}
				}
				return new DutyTaxEntryFee();
			}
		}
		CachedProperty<DutyTaxEntryFee> cachedTotalDutyTaxEntryFeeItems;

		public LandedCostingHelper GetLandedCostingHelper()
		{
			return IsDeclarationIntegrated ? new IntegratedCountryLandedCostingHelper() : GetLandedCostingHelperCore();
		}

		protected virtual LandedCostingHelper GetLandedCostingHelperCore()
		{
			var countryCode = Constants.CountryCodes.GetCustomsCountryOfJurisdiction(CountryCode);
			return GenericLandedCostingConfigProvider.Instance.SupportedCountries.Contains(countryCode) ? new GenericLandedCostingHelper(countryCode) : null;
		}

		void ILandedCostHeader.DoStuffBeforeRunningLCDistribution()
		{
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "for cargowise")]
		public const string ILandedCostHeaderMustBeImplementedMessage = "ILandedCostHeader members (TotalOtherDutyOrFlatDuty, TotalExcise, TotalSpecialTax1, TotalSpecialTax2, TotalSpecialTax3, TotalEntryFees) should be implemented in Country-specific JobDeclaration";

		public GroupHeaderCollection AllGroupHeaders
		{
			get
			{
				if (allGroupHeaders == null)
				{
					allGroupHeaders = CreateAllGroupHeadersCollection();
					allGroupHeaders.Load();
				}
				return allGroupHeaders;
			}
		}
		GroupHeaderCollection allGroupHeaders;

		protected virtual GroupHeaderCollection CreateAllGroupHeadersCollection()
		{
			return new GroupHeaderCollection(this);
		}

		IComparer ILandedCostHeader.LineComparer
		{
			get { return new BaseJobComInvoiceLine.LineComparer(); }
		}

		bool ILandedCostHeader.HasMultiInvoices
		{
			get { return Invoices.Count > 1; }
		}

		ZGuid ILandedCostHeader.CompanyPK
		{
			get { return Branch != null ? Branch.GB_GC : GlbCompany.CurrentCompany.PK; }
		}

		Dictionary<ZString, ZDecimal> ILandedCostHeader.GetDefaultExchangeRates()
		{
			return null;
		}

		IHaveRequiredDocuments ILandedCostHeader.RequiredDocumentsProvider
		{
			get { return ((IDocsAndCartageParent)this).RequiredDocumentsProvider; }
		}

		#endregion

		#region ILandedCostHeaderProvider Members

		ILandedCostHeader ILandedCostHeaderProvider.LCHeaderHost
		{
			get { return this; }
		}

		#endregion

		#region IDocAddresses

		#region DocAddresses

		[ChildEditable(true)]
		[UniversalCopySplitCollection("Bonded Warehouse", JobDocAddressSchema.Constants.E2_AddressType + " = '" + AutoDocAddressTypes.Codes.CustomsWarehouseAddress + "'")]
		[UniversalCopySplitCollection("Importer Documentary Address", JobDocAddressSchema.Constants.E2_AddressType + " = '" + AutoDocAddressTypes.Codes.ImporterDocumentaryAddress + "'")]
		[UniversalCopySplitCollection("Importer Pickup/Delivery Address", JobDocAddressSchema.Constants.E2_AddressType + " = '" + AutoDocAddressTypes.Codes.ImporterPickupDeliveryAddress + "'")]
		[UniversalCopySplitCollection("Supplier Documentary Address", JobDocAddressSchema.Constants.E2_AddressType + " = '" + AutoDocAddressTypes.Codes.SupplierDocumentaryAddress + "'")]
		[UniversalCopySplitCollection("Supplier Pickup/Delivery Address", JobDocAddressSchema.Constants.E2_AddressType + " = '" + AutoDocAddressTypes.Codes.SupplierPickupDeliveryAddress + "'")]
		public virtual JobDocAddressDependentCollection DocAddresses
		{
			get
			{
				if (fDocAddresses == null)
				{
					fDocAddresses = new JobDocAddressDependentCollection(this);
					fDocAddresses.Load();
					RegisterEditableChildObject(fDocAddresses);
				}

				return fDocAddresses;
			}
		}
		JobDocAddressDependentCollection fDocAddresses;

		#endregion

		#region DocAddressManager

		public JobDocAddressManager DocAddressManager
		{
			get
			{
				if (fDocAddressManager == null)
				{
					fDocAddressManager = new JobDocAddressManager();
				}
				return fDocAddressManager;
			}
		}

		JobDocAddressManager fDocAddressManager;

		#endregion

		#region DocAddress Requirements

		#region ImporterDocAddressRequirement

		protected JobDocAddressRequirement ImporterDocAddressRequirement
		{
			get { return fImporterDocAddressRequirement ?? (fImporterDocAddressRequirement = AddImporterDocAddressRequirement()); }
			set { fImporterDocAddressRequirement = value; }
		}
		JobDocAddressRequirement fImporterDocAddressRequirement;

		protected virtual JobDocAddressRequirement AddImporterDocAddressRequirement()
		{
			var requirement = GetImporterDocumentaryAddressRequirement();
			if (Shipment == null)
			{
				requirement.AddLinkedRequirement(ImporterPicDlvAddressRequirement);
			}
			requirement.ValidateOrganisationPK += ImporterDocAddressRequirement_ValidateOrganisationPK;
			DocAddressManager.AddRequirement(requirement);

			return requirement;
		}

		protected virtual JobDocAddressRequirement GetImporterDocumentaryAddressRequirement()
		{
			return new JobDocAddressRequirement(DocAddressType.ImporterDocumentaryAddress, ContactType.Consignee);
		}

		protected virtual void SupplierDocAddressRequirement_ValidateOrganisationPK(JobDocAddressValidation validation)
		{
			var parent = validation.Parent;
			if (parent != null && parent.Address == null && IsSupplierDocumentaryAddressRequiredForWarehouseValidation && !IsBondedWarehousingDisabledForAllEntries && IsBondedWarehousingFieldValidationRequired)
			{
				parent.OrganisationPKInfo.AddMessageError(SupplierDocumentaryAddressIsRequiredForBondedWarehousing);
			}
		}

		bool IsSupplierDocumentaryAddressRequiredForWarehouseValidation => IsExport && IsSupplierDocumentaryAddressRequiredForWarehouseValidationCore;

		protected virtual bool IsSupplierDocumentaryAddressRequiredForWarehouseValidationCore
		{
			get { return true; }
		}

		void ImporterDocAddressRequirement_ValidateOrganisationPK(JobDocAddressValidation validation)
		{
			var parent = validation.Parent;
			if (parent != null && parent.Address == null && IsImporterDocumentaryAddressRequiredForWarehouseValidation && !IsBondedWarehousingDisabledForAllEntries && IsBondedWarehousingFieldValidationRequired)
			{
				parent.OrganisationPKInfo.AddMessageError(ImporterDocumentaryAddressIsRequiredForBondedWarehousing);
			}
		}

		bool IsImporterDocumentaryAddressRequiredForWarehouseValidation => !IsExport && IsImporterDocumentaryAddressRequiredForWarehouseValidationCore;

		protected virtual bool IsImporterDocumentaryAddressRequiredForWarehouseValidationCore
		{
			get { return true; }
		}

		public string ImporterDocumentaryAddressIsRequiredForBondedWarehousing
		{
			get
			{
				return IsStandAlone ? Res.GetString("4372E86F-04CE-4F1C-8284-5D0EF19259F8", "Importer Documentary Address is required for Inventory Management integration.")
					: Res.GetString("116EDE9C-23AC-4589-9EEC-39320BD3900E", "Consignee Documentary Address is required for Inventory Management integration.");
			}
		}

		public string SupplierDocumentaryAddressIsRequiredForBondedWarehousing
		{
			get
			{
				return IsStandAlone ? Res.GetString("4372E86F-04CE-4F1C-8284-5D0EF1925999", "Supplier Documentary Address is required for Inventory Management integration.")
					: Res.GetString("116EDE9C-23AC-4589-9EEC-39320BD39099", "Consignor Documentary Address is required for Inventory Management integration.");
			}
		}

		JobDocAddressRequirement ImporterPicDlvAddressRequirement
		{
			get { return importerPicDlvAddressRequirement ?? (importerPicDlvAddressRequirement = GetImporterPicDlvAddressRequirement()); }
		}
		JobDocAddressRequirement importerPicDlvAddressRequirement;

		protected virtual JobDocAddressRequirement GetImporterPicDlvAddressRequirement() => new JobDocAddressRequirement(DocAddressType.ImporterPickupDeliveryAddress, AddressType.DLV);

		#endregion

		#region SupplierDocAddressRequirement

		protected JobDocAddressRequirement SupplierDocAddressRequirement
		{
			get { return fSupplierDocAddressRequirement ?? (fSupplierDocAddressRequirement = AddSupplierDocAddressRequirement()); }
			set { fSupplierDocAddressRequirement = value; }
		}
		JobDocAddressRequirement fSupplierDocAddressRequirement;

		protected virtual JobDocAddressRequirement AddSupplierDocAddressRequirement()
		{
			var requirement = GetSupplierDocAddressRequirement();
			if (Shipment == null)
			{
				requirement.AddLinkedRequirement(SupplierPicDlvAddressRequirement);
			}
			requirement.ValidateOrganisationPK += SupplierDocAddressRequirement_ValidateOrganisationPK;
			DocAddressManager.AddRequirement(requirement);

			return requirement;
		}

		protected virtual JobDocAddressRequirement GetSupplierDocAddressRequirement()
		{
			return new JobDocAddressRequirement(DocAddressType.SupplierDocumentaryAddress, ContactType.Consignor);
		}

		JobDocAddressRequirement SupplierPicDlvAddressRequirement
		{
			get { return supplierPicDlvAddressRequirement ?? (supplierPicDlvAddressRequirement = GetSupplierPicDlvAddressRequirement()); }
		}
		JobDocAddressRequirement supplierPicDlvAddressRequirement;

		protected virtual JobDocAddressRequirement GetSupplierPicDlvAddressRequirement() => new JobDocAddressRequirement(DocAddressType.SupplierPickupDeliveryAddress, AddressType.PIC);

		#endregion

		#endregion

		public virtual ZValidation PiggyBackedDocAddressValidation(JobDocAddress addressToValidate)
		{
			return null;
		}

		IReadOnlyList<DocAddressType> IDocAddresses.SupportedAddressTypes
		{
			get { return SupportedAddressTypesCore; }
		}

		protected virtual DocAddressType[] SupportedAddressTypesCore
		{
			get
			{
				return new DocAddressType[]
						{
							DocAddressType.SupplierDocumentaryAddress,
							DocAddressType.SupplierPickupDeliveryAddress,
							DocAddressType.ImporterDocumentaryAddress,
							DocAddressType.ImporterPickupDeliveryAddress,
							DocAddressType.NotifyParty,
							DocAddressType.NotifyParty2,
							DocAddressType.NotifyParty3,
							DocAddressType.BuyerDocumentaryAddress,
							DocAddressType.InsuredByDocumentaryAddress,
							DocAddressType.AssuredPartyDocumentaryAddress,
							DocAddressType.ClaimsPayableByDocumentaryAddress,
							DocAddressType.SurveyReportPartyDocumentaryAddress,
							DocAddressType.CustomsContainerTerminalOperatorAddress,
							DocAddressType.CustomsContainerYardAddress,
							DocAddressType.CustomsDepotAddress,
							DocAddressType.CustomsWarehouseAddress,
						};
			}
		}

		JobDocAddressRequirement IDocAddresses.GetDocAddressRequirement(DocAddressType addressType)
		{
			return GetDocAddressRequirement(addressType);
		}

		protected virtual JobDocAddressRequirement GetDocAddressRequirement(DocAddressType addressType)
		{
			switch (addressType)
			{
				case DocAddressType.SupplierDocumentaryAddress:
					return SupplierDocAddressRequirement;
				case DocAddressType.SupplierPickupDeliveryAddress:
					return SupplierPicDlvAddressRequirement;
				case DocAddressType.ImporterDocumentaryAddress:
					return ImporterDocAddressRequirement;
				case DocAddressType.ImporterPickupDeliveryAddress:
					return ImporterPicDlvAddressRequirement;
				case DocAddressType.NotifyParty:
					return NotifyPartyDocAddressRequirement;
				case DocAddressType.NotifyParty2:
					return NotifyParty2DocAddressRequirement;
				case DocAddressType.NotifyParty3:
					return NotifyParty3DocAddressRequirement;
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
				case DocAddressType.CustomsContainerTerminalOperatorAddress:
					return ContainerTerminalOperatorDocAddressRequirement;
				case DocAddressType.CustomsContainerYardAddress:
					return ContainerYardDocAddressRequirement;
				case DocAddressType.CustomsDepotAddress:
					return DepotDocAddressRequirement;
				case DocAddressType.CustomsWarehouseAddress:
					return WarehouseDocAddressRequirement;
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
		}

		void IDocAddresses.OrgHeaderAfterChange(JobDocAddress docAddress)
		{
		}

		SecurityCheckpoint IDocAddresses.GetCanOverrideCheckpoint(JobDocAddress docAddress)
		{
			// Add DocAddressTypes to this list that DO NOT support overrides because they are displayed using simple address control
			switch (docAddress.DocAddressType)
			{
				case DocAddressType.CustomsContainerYardAddress:
				case DocAddressType.CustomsContainerTerminalOperatorAddress:
				case DocAddressType.CustomsDepotAddress:
				case DocAddressType.CustomsWarehouseAddress:
					return new CannotOverrideAddressSecurityCheckpoint();
				default:
					return Env.Security.None;
			}
		}

		bool IDocAddresses.CanDeleteAddress(JobDocAddress docAddress)
		{
			return false;
		}

		OrgHeaderCollection IDocAddresses.GetOrgHeaderList(DocAddressType addressType)
		{
			return GetOrgHeaderListCore(addressType);
		}

		protected virtual OrgHeaderCollection GetOrgHeaderListCore(DocAddressType addressType)
		{
			switch (addressType)
			{
				case DocAddressType.ImporterDocumentaryAddress:
					return Lookups.ImportersList;
				case DocAddressType.SupplierDocumentaryAddress:
					return Lookups.SuppliersList;
				case DocAddressType.CustomsDepotAddress:
					return Lookups.DepotCollection;
				case DocAddressType.NotifyParty:
				case DocAddressType.NotifyParty2:
				case DocAddressType.NotifyParty3:
					return Lookups.NotifyParties;
				case DocAddressType.BuyerDocumentaryAddress:
					return Lookups.Buyers;
				case DocAddressType.CustomsContainerYardAddress:
					return Lookups.ContainerYardCollection;
				case DocAddressType.CustomsContainerTerminalOperatorAddress:
					return Lookups.ContainerTerminalOperatorCollection;
				case DocAddressType.CustomsWarehouseAddress:
					return Lookups.BondedWarehouseCollection;
				case DocAddressType.Carrier:
					return Lookups.CarrierOrganisations;
				case DocAddressType.SellingParty:
					return Lookups.SellingAgents;
				case DocAddressType.Exporter:
					return Lookups.Exporters;
				case DocAddressType.ExternalBroker:
					return Lookups.ExternalBrokers;
			}

			return null;
		}

		#endregion

		#region ICurrencyConverterDataProvider Members

		ExchangeRateType ICurrencyConverterDataProvider.RateType => RateTypeCore;

		protected virtual ExchangeRateType RateTypeCore => ZArchitecture.Core.ExchangeRateType.Customs;

		public const int CurrencyConverterMaximumDaysToFallBack = 7;

		int ICurrencyConverterDataProvider.MaximumDaysToFallback
		{
			get { return CurrencyConverterMaximumDaysToFallBack; }
		}

		GlbCompany ICurrencyConverterDataProvider.Company
		{
			get { return Company; }
		}

		ZString ICurrencyConverterDataProvider.LocalCurrencyCodeOverride
		{
			get { return LocalCurrencyCode; }
		}

		ZBool? ICurrencyConverterDataProvider.IsReciprocalOverride
		{
			get { return IsReciprocalRates; }
		}

		#endregion

		#region IApportionInvoiceHolder Members

		IComparer IApportionInvoiceHolder.ChargeComparer
		{
			get { return null; }
		}

		IChargeHolder[] IApportionInvoiceHolder.ChargeHolders
		{
			get
			{
				ArrayList result = new ArrayList();
				result.AddRange(Invoices);
				result.AddRange(AllGroupHeaders);
				return (IChargeHolder[])result.ToArray(typeof(IChargeHolder));
			}
		}

		IChargeApportionee[] IApportionInvoiceHolder.Invoices
		{
			get
			{
				return (IChargeApportionee[])new ArrayList(Invoices).ToArray(typeof(IChargeApportionee));
			}
		}

		IComparer<IChargeApportionee> IApportionInvoiceHolder.LineChargeApportioneeComparer
		{
			get { return new BaseJobComInvoiceLine.LineComparer(); }
		}

		protected IncoTermAndCustomsChargeFactory fIncoTermAndChargeFactory;
		public bool NeedToGetNewIncoTermAndChargeFactory;
		public IncoTermAndCustomsChargeFactory IncoTermAndChargeFactory
		{
			get
			{
				if (fIncoTermAndChargeFactory == null || NeedToGetNewIncoTermAndChargeFactory)
				{
					fIncoTermAndChargeFactory = IncoTermAndCustomsChargeFactory.GetByCountryCode(((IApportionInvoiceHolder)this).CountryContext);
					NeedToGetNewIncoTermAndChargeFactory = false;
				}
				return fIncoTermAndChargeFactory;
			}
		}

		#endregion

		#region ICusEntryNumFilterProvider Members

		ZQuery ICusEntryNumFilterProvider.ValidCusEntryNumFilter
		{
			get { return GetValidCusEntryNumFilter(); }
		}

		protected virtual ZQuery GetValidCusEntryNumFilter()
		{
			var result = new ZQuery();

			var pkList = new List<ZGuid>() { PK };

			foreach (CusEntryHeader entryHeader in CustomsEntryHeaders)
			{
				if (entryHeader.ShouldBeIncludedInCusEntryNumberFilter)
				{
					pkList.Add(entryHeader.PK);
				}
			}

			result.AddToFilter(CusEntryNumSchema.CE_ParentID, pkList);

			return result;
		}

		#endregion

		#region IWorkflowProvider Members

		bool ISometimesWorkflowProvider.ShouldSupportWorkflowTemplateApplication => IsPersistent && Shipment == null && !SuspendAddingWorkflow && AddingWorkflowByBaseJobDeclaration && GlbCompany.CurrentCompany.PK == (Company?.PK ?? ZGuid.Empty);

		public virtual ZString WorkflowProviderCoreCode
		{
			get { return JobInvoicingConsumerTypes.Brokerage.Code; }
		}

		ZString IWorkflowProviderCore.WorkflowType
		{
			get
			{
				return WorkflowProviderCoreCode;
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
		public virtual ProcessTaskCollection WorkflowItems
		{
			get
			{
				var shipment = this.Shipment;
				if (workflowItems == null && shipment == null)
				{
					workflowItems = this.GetOrCreateProcessTaskCollection(() =>
					{
						var constructedType = typeof(JobDeclarationProcessTaskCollection<>).MakeGenericType(GetType());
						return (ProcessTaskCollection)Activator.CreateInstance(constructedType, this);
					});
					RegisterEditableChildObject(workflowItems);
				}

				return shipment != null ? shipment.WorkflowItems : workflowItems;
			}
		}
		ProcessTaskCollection workflowItems;

		IColumnValueRanker IWorkflowProviderCore.GetTemplateSelectionCriteria()
		{
			return GetRankerForTemplate();
		}

		public virtual ColumnValueRanker GetRankerForTemplate()
		{
			ColumnValueRanker result = new ColumnValueRanker();
			var job = new JobHeader.Loader(this).Load(true, false);

			result.Add(ProcessTaskTemplateSchema.P0_OH_Client, GetClientsInTemplateSelectionOrder());
			result.Add(ProcessTaskTemplateSchema.P0_SubType1, JE_TransportMode, ZString.Empty);
			result.Add(ProcessTaskTemplateSchema.P0_SubType2, JE_MessageType, WorkflowImportOrExport, ZString.Empty);
			result.Add(ProcessTaskTemplateSchema.P0_SubType3, JE_ContainerMode, ZString.Empty);
			result.Add(ProcessTaskTemplateSchema.P0_GB, job != null && job.JH_GB.IsValid ? job.JH_GB : JE_GB, ZGuid.Empty);
			result.Add(ProcessTaskTemplateSchema.P0_GE, job != null && job.JH_GE.IsValid ? job.JH_GE : GlbDepartment.CurrentDepartment.PK, ZGuid.Empty);
			result.Add(ProcessTaskTemplateSchema.P0_LoadPortCountry, JE_RL_NKPortOfLoading, JE_RL_NKPortOfLoading.Substring(0, 2), ZString.Empty);
			result.Add(ProcessTaskTemplateSchema.P0_DischargePortCountry, JE_RL_NKPortOfArrival, JE_RL_NKPortOfArrival.Substring(0, 2), ZString.Empty);
			return result;
		}

		IZType[] GetClientsInTemplateSelectionOrder()
		{
			List<IZType> result = new List<IZType>();
			if (IsImport)
			{
				if (JE_OH_Importer.IsValid)
				{
					result.Add(JE_OH_Importer);
				}

				if (JE_OH_Supplier.IsValid)
				{
					result.Add(JE_OH_Supplier);
				}
			}
			else
			{
				if (JE_OH_Supplier.IsValid)
				{
					result.Add(JE_OH_Supplier);
				}

				if (JE_OH_Importer.IsValid)
				{
					result.Add(JE_OH_Importer);
				}
			}

			JobHeader job = new JobHeader.Loader(this).Load();
			if (job != null)
			{
				result.Add(job.LocalChargesPK);
			}
			result.Add(ZGuid.Empty);
			return result.ToArray();
		}

		protected virtual ZString WorkflowImportOrExport
		{
			get
			{
				ZString result = "";
				if (IsExWarehouse)
				{
					result = JobMessageTypeList.Codes.ExWarehouse;
				}
				else if (IsMiscellaneous)
				{
					result = JobMessageTypeList.Codes.MiscellaneousCustoms;
				}
				else if (IsDrawback)
				{
					result = JobMessageTypeList.Codes.Drawback;
				}
				else if (IsRefund)
				{
					result = JobMessageTypeList.Codes.Refund;
				}
				else if (IsImport && !IsExport)
				{
					result = ImportExportCodeList.Codes.Import;
				}
				else if (IsExport && !IsImport)
				{
					result = ImportExportCodeList.Codes.Export;
				}
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
					var companyPK = CompanyPK;
					workflowInformationProvider = new WorkflowInformationProvider(companyPK.IsEmpty ? Array.Empty<ZGuid>() : new ZGuid[] { companyPK });
				}
				workflowInformationProvider.Destination = FinalDestination?.RL_PortName ?? ZString.Empty;
				workflowInformationProvider.Origin = Origin?.RL_PortName ?? ZString.Empty;
				workflowInformationProvider.BusinessContext = TrackingConstants.BusinessContext.Declaration;

				return workflowInformationProvider;
			}
		}
		WorkflowInformationProvider workflowInformationProvider;

		#endregion

		#region PackingGroup/Package default

		public override ZInt JE_TotalNoOfPacks
		{
			get { return base.JE_TotalNoOfPacks; }
			set
			{
				bool hasChanges = base.JE_TotalNoOfPacks != value;
				base.JE_TotalNoOfPacks = value;

				if (hasChanges && !IsCopying)
				{
					DefaultPackDetailsToPackingGroupIfRequired(Schema.JE_TotalNoOfPacks);
				}
				(Validation as BaseJobDeclarationValidation)?.ValidatePackagesActualPackageCount();
			}
		}

		public override ZString JE_TotalNoOfPacksPackType
		{
			get { return base.JE_TotalNoOfPacksPackType; }
			set
			{
				bool hasChanges = base.JE_TotalNoOfPacksPackType != value;
				base.JE_TotalNoOfPacksPackType = value;

				if (hasChanges && !IsCopying)
				{
					if (ShouldDefaultTotalPackTypeToBillCore)
					{
						DefaultPackDetailsToPackingGroupIfRequired(Schema.JE_TotalNoOfPacksPackType);
					}

					if (Invoices.Count > 0)
					{
						RefreshBindingForInvoices();
					}
				}
			}
		}

		protected void RefreshBindingForInvoices()
		{
			Invoices.RefreshBinding();
		}

		void DefaultPackDetailsToPackingGroupIfRequired(string packDetailProperty)
		{
			if (!IsCopying && ShouldDefaultPackingInfoFromDeclarationToBills && LowestBills.Count == 1)
			{
				var bill = GetOrCreateBillToDefaultPackDetailsTo();

				if (!bill.PackingGroups.HasMultiplePackingGroups)
				{
					if (bill.IsHouseBill)
					{
						RemovePackDetatisFromMasterBillIfRequired();
					}

					var packGroup = bill.PackingGroups.Count == 0 ? bill.PackingGroups.AddNew() : bill.PackingGroups[0];

					switch (packDetailProperty)
					{
						case Schema.JE_TotalNoOfPacksPackType:
							packGroup.DefaultPackTypeIfRequired(JE_TotalNoOfPacksPackType);
							break;
						case Schema.JE_TotalNoOfPacks:
							packGroup.AddTotalOuterPackageIfRequired(JE_TotalNoOfPacks);
							break;
					}
				}
			}
		}
		public NotificationTypes ErrorTypeForCWPackQtyExceededError
		{
			get { return ErrorTypeForCWPackQtyExceededErrorCore; }
		}

		protected virtual NotificationTypes ErrorTypeForCWPackQtyExceededErrorCore
		{
			get { return NotificationTypes.MessageError; }
		}

		protected virtual bool ShouldDefaultTotalPackTypeToBillCore
		{
			get { return true; }//AU has only pack qty exposed in 'Packing' tab.
		}

		protected virtual Bill GetOrCreateBillToDefaultPackDetailsTo()
		{
			return PrimaryHouseBill
					 ?? (PackingLinesShouldOnlyBeCreatedAtTheHouseBillLevel ? null : PrimaryMasterBill)
					 ?? Bills.CreatePrimaryBill(BillTypeList.Codes.HouseBill);
		}

		public bool PackingLinesShouldOnlyBeCreatedAtTheHouseBillLevel
		{
			get { return PackingLinesShouldOnlyBeCreatedAtTheHouseBillLevelCore; }
		}

		protected virtual bool PackingLinesShouldOnlyBeCreatedAtTheHouseBillLevelCore
		{
			get { return false; }
		}

		void RemovePackDetatisFromMasterBillIfRequired()
		{
			var masterBill = PrimaryMasterBill;
			if (masterBill != null && masterBill.PackingGroups.Count == 1)
			{
				masterBill.PackingGroups[0].Delete();
			}
		}

		public bool ShouldDefaultPackingInfoFromDeclarationToBills => ShouldDefaultPackingInfoFromDeclarationToBillsCore;

		protected virtual bool ShouldDefaultPackingInfoFromDeclarationToBillsCore
		{
			get { return CanDefaultPackingInfoFromDeclarationToBills && IsPackingInformationRelevant; }
		}

		protected bool CanDefaultPackingInfoFromDeclarationToBills
		{
			get
			{
				return
#if DEBUG
					!DisableDefaultPackingInformation &&
#endif
					!((ISupportDataImporting)this).IsImportingData &&
					(!IsPluggedIntoShipment || !ShouldSynchroniseWithShipment());
			}
		}

#if DEBUG
		public bool DisableDefaultPackingInformation
		{
			get { return fDisableDefaultPackingInformation; }
			set { fDisableDefaultPackingInformation = value; }
		}
		bool fDisableDefaultPackingInformation;
#endif

		public bool IsPackingInformationRelevant
		{
			get { return IsPackingInformationRelevantCore; }
		}

		protected virtual bool IsPackingInformationRelevantCore
		{
			get { return true; }
		}

		public bool IsContainerInvoiceLinkRelevant => IsContainerInvoiceLinkRelevantCore;

		protected virtual bool IsContainerInvoiceLinkRelevantCore => true;

		public bool IsContainerPackingRequired => IsPackingInformationRelevant && IsContainerPackingRequiredCore;

		protected virtual bool IsContainerPackingRequiredCore => true;

		public IPackingInformationCollection PackingInformationCollection
		{
			get { return (IPackingInformationCollection)Packages; }
		}

		#endregion

		#region IExportStatement Members

		public ZString GetExportStatement(ExportStatementSetting exportStatementSetting)
		{
			if (exportStatementSetting == null || !IsExport)
			{
				return ZString.Empty;
			}
			return CreateNewExportStatementCreator(exportStatementSetting).ExportStatement;
		}

		protected virtual ExportStatementCreator CreateNewExportStatementCreator(ExportStatementSetting exportStatementSetting)
		{
			return new ExportStatementCreator(exportStatementSetting);
		}

		#endregion

		#region TransportModexxxCodeForTesting
#if DEBUG
		public virtual ZString TransportModeAirCodeForTesting
		{
			get { return Core.Constants.TransportModes.Air; }
		}

		public virtual ZString TransportModeSeaCodeForTesting
		{
			get { return Core.Constants.TransportModes.Sea; }
		}

		public virtual ZString TransportModeMailCodeForTesting
		{
			get { return Core.Constants.TransportModes.Mail; }
		}

		public virtual ZString TransportModeRailCodeForTesting
		{
			get { return Core.Constants.TransportModes.Rail; }
		}

		public virtual ZString TransportModeRoadCodeForTesting
		{
			get { return Core.Constants.TransportModes.Road; }
		}

		public virtual ZString TransportModeFixedCodeForTesting
		{
			get { return Core.Constants.TransportModes.FixedTransportInstallations; }
		}
#endif
		#endregion

		#region IEDocsProvider Members

		EDocsProviderSupporter IEDocsProvider.GetEDocsProviderSupporter()
		{
			return new JobInvoicingEDocsProviderSupporter(this);
		}

		#endregion

		#region IRelatedJobNumber Members

		string[] IRelatedJobNumber.JobNumber
		{
			get { return new string[] { this.JobNumber }; }
		}

		#endregion

		#region ICommercialInvoice Members

		void Freight.Integration.Forwarding.ICommercialInvoice.PopulateCommercialInvoice(Freight.Integration.Forwarding.IForwardingPackLineCollection packLines)
		{
			BaseJobComInvoiceHeader header = Invoices.AddNew();
			foreach (ForwardingPackLine packLine in packLines)
			{
				BaseJobComInvoiceLine line = header.JobComInvoiceLines.AddNew();
				if (packLine.Products != null)
				{
					line.JI_PartNo = packLine.Products.PackProductManager.Value;
				}

				if (!packLine.JL_HarmonisedCode.IsEmpty)
				{
					line.JI_Tariff = packLine.JL_HarmonisedCode;
				}
				line.JI_LinePrice = packLine.JL_LinePrice;
				header.JZ_InvoiceAmount += packLine.JL_LinePrice;
				line.JI_InvoiceQuantity = ZDecimal.ParseSafe(packLine.JL_PackageCount.ToString(), 0m);
				line.JI_InvoiceUQ = packLine.JL_F3_NKPackType;
				line.JI_Weight = packLine.JL_ActualWeight;
				line.JI_WeightUQ = packLine.JL_ActualWeightUQ;
				line.JI_Volume = packLine.JL_ActualVolume;
				line.JI_VolumeUQ = packLine.JL_ActualVolumeUQ;
				if (packLine.CommodityCode != null)
				{
					line.JI_RH_NKCommodity_Code = packLine.CommodityCode.RH_Code;
				}

				if (!packLine.JL_Description.IsEmpty)
				{
					line.JI_Description = packLine.JL_Description;
				}
			}
		}

		#endregion

		#region ICommercialInvoiceProvider Members

		IBusinessObjectCollection Freight.Integration.Forwarding.ICommercialInvoiceProvider.Invoices
		{
			get { return Invoices; }
		}

		#endregion

		#region Unmatched Classification Saving

		public bool InvoicesMentionProductsWithoutMatchingClassification
		{
			get
			{
				if (invoicesMentionProductsWithoutMatchingClassificationCached == null)
				{
					invoicesMentionProductsWithoutMatchingClassificationCached = new CachedProperty<bool>(Factory, delegate
					{
						return InvoiceLines.Cast<BaseJobComInvoiceLine>().Any(x => IsUnmatchedProductClassification(x));
					});
				}

				return invoicesMentionProductsWithoutMatchingClassificationCached.Value;
			}
		}
		CachedProperty<bool> invoicesMentionProductsWithoutMatchingClassificationCached;

		internal protected bool IsUnmatchedProductClassification(BaseJobComInvoiceLine line)
		{
			var result = false;

			if (line.Part != null && line.GetPivot() == null)
			{
				var pivots = line.GetPivots();
				if (pivots == null || pivots.Length == 0)
				{
					result = (!line.JI_CC.IsEmpty || !line.JI_Tariff.IsEmpty);
				}
				else if (pivots.Length > 1 && (!line.JI_CC.IsEmpty || !line.JI_Tariff.IsEmpty))
				{
					result = !pivots.Any(p => p.TariffNumber == line.JI_Tariff && p.CI_CC == line.JI_CC);
				}
			}

			return result;
		}

		public void SaveNewPartClassifications()
		{
			InvoiceLines.AddNewPartClassifications();
		}

		#endregion

		#region New Products Saving

		public bool InvoicesMentionNewOrInactiveProducts
		{
			get
			{
				if (invoicesMentionNewOrInactiveProductsCached == null)
				{
					invoicesMentionNewOrInactiveProductsCached = new CachedProperty<bool>(Factory, delegate
					{
						return InvoiceLines.Cast<BaseJobComInvoiceLine>().Any(x => IsNotPersistentActiveProduct(x));
					});
				}

				return invoicesMentionNewOrInactiveProductsCached.Value;
			}
		}
		CachedProperty<bool> invoicesMentionNewOrInactiveProductsCached;

		/// <summary>
		/// States whether the line's product gives a hit for an active record in the database.  Will return FALSE is the product code does not pertain to a record in the DB, or if it does relate to one but it's inactive.
		/// </summary>
		internal protected bool IsNotPersistentActiveProduct(BaseJobComInvoiceLine line)
		{
			return IsNotPersistentActiveProductInternal(line) && !line.JI_InvoiceUQ.IsEmpty;
		}

		protected virtual bool IsNotPersistentActiveProductInternal(BaseJobComInvoiceLine line)
		{
			return !line.JI_PartNo.IsEmpty && line.Part == null && !line.JI_Description.IsEmpty && (!line.JI_CC.IsEmpty || !line.JI_Tariff.IsEmpty);
		}

		public List<string> SaveNewProductsorActivateInactiveOnes(ProductRelationDefaultOption option = ProductRelationDefaultOption.None, DeclarationForProductCreationHelper productCreationHelper = null)
		{
			return InvoiceLines.AddNewPartsOrActivateInactiveOnes(option, productCreationHelper);
		}

		public bool InvoicesContainNotPersistentActiveProductWithMissingInvoiceUQ
		{
			get
			{
				if (invoicesContainNotPersistentActiveProductWithMissingInvoiceUQCached == null)
				{
					invoicesContainNotPersistentActiveProductWithMissingInvoiceUQCached = new CachedProperty<bool>(Factory, delegate
					{
						return InvoiceLines.Cast<BaseJobComInvoiceLine>().Any(x => IsNotPersistentActiveProductInternal(x) && x.JI_InvoiceUQ.IsEmpty);
					});
				}

				return invoicesContainNotPersistentActiveProductWithMissingInvoiceUQCached.Value;
			}
		}

		CachedProperty<bool> invoicesContainNotPersistentActiveProductWithMissingInvoiceUQCached;

		#endregion

		#region ICartageParent Members

		event EventHandler ICartageParent.CartageTypesChanged
		{
			add
			{
				JE_MessageTypeInfo.ValueChanged += value;
			}
			remove
			{
				JE_MessageTypeInfo.ValueChanged -= value;
			}
		}

		ZGuid ICartageParent.BranchPK
		{
			get { return JE_GB; }
		}

		ZGuid ICartageParent.CartageParentID
		{
			get { return PK; }
		}

		ZString ICartageParent.CartageParentTableCode
		{
			get { return JobDeclarationSchema.Constants.Prefix; }
		}

		IReadOnlyCollection<CartageType> ICartageParent.CartageTypes
		{
			get { return new CartageType[] { ((ICartageParent)this).GetLocalCartageType }; }
		}

		CartageType ICartageParent.GetLocalCartageType
		{
			get { return IsImport ? new DeclarationImportCartageType(this) : new DeclarationExportCartageType(this); }
		}

		ControllerID ICartageParent.ControllerID
		{
			get { return ControllerIDs.Customs.JobDeclaration; }
		}

		ZString ICartageParent.GoodsDescription
		{
			get { return JE_GoodsDescription; }
		}

		ZGuid ICartageParent.JobHeaderPK
		{
			get { return Job != null ? Job.PK : ZGuid.Empty; }
		}

		ZGuid ICartageParent.LocalClientAddressPK
		{
			get { return Job != null ? Job.JH_OA_LocalChargesAddr : ZGuid.Empty; }
		}

		ZString ICartageParent.OrderReferenceNumber
		{
			get { return JE_OwnerRef; }
		}

		ZString ICartageParent.ServiceLevel
		{
			get { return JE_RS_NKServiceLevel; }
		}

		ZString ICartageParent.UniqueConsignmentID
		{
			get { return JE_DeclarationReference; }
		}

		ZString ICartageParent.WayBillNumber
		{
			get { return JE_HouseBill; }
		}

		ZInt ICartageParent.TotalPackages
		{
			get { return JE_TotalNoOfPacks; }
		}

		ZString ICartageParent.TotalPackType
		{
			get { return JE_TotalNoOfPacksPackType; }
		}

		ZDecimal ICartageParent.TotalWeight
		{
			get { return JE_TotalWeight; }
		}

		ZString ICartageParent.TotalWeightUnit
		{
			get { return JE_TotalWeightUnit; }
		}

		ZDecimal ICartageParent.TotalVolume
		{
			get { return JE_TotalVolume; }
		}

		ZString ICartageParent.TotalVolumeUnit
		{
			get { return JE_TotalVolumeUnit; }
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

		JobDocAddress ICartageParentExtra.ConsignorDocumentaryAddress
		{
			get { return SupplierDocumentaryAddress; }
		}

		JobDocAddress ICartageParentExtra.ConsigneeDocumentaryAddress
		{
			get { return ImporterDocumentaryAddress; }
		}

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

		#region ICartageLooseCargo Members

		ZString ICartageLooseCargo.BookedDimensionUnit
		{
			get { return ""; }
		}

		ZDecimal ICartageLooseCargo.BookedHeight
		{
			get { return 0m; }
		}

		ZDecimal ICartageLooseCargo.BookedLength
		{
			get { return 0m; }
		}

		ZString ICartageLooseCargo.BookedPackType
		{
			get { return JE_TotalNoOfPacksPackType; }
		}

		ZInt ICartageLooseCargo.BookedPackages
		{
			get { return JE_TotalNoOfPacks; }
		}

		ZDecimal ICartageLooseCargo.BookedVolume
		{
			get { return JE_TotalVolume; }
		}

		ZString ICartageLooseCargo.BookedVolumeUnit
		{
			get { return JE_TotalVolumeUnit; }
		}

		ZDecimal ICartageLooseCargo.BookedWeight
		{
			get { return JE_TotalWeight; }
		}

		ZString ICartageLooseCargo.BookedWeightUnit
		{
			get { return JE_TotalWeightUnit; }
		}

		ZDecimal ICartageLooseCargo.BookedWidth
		{
			get { return 0m; }
		}

		IReadOnlyCollection<UNDGDataItem> ICartageLooseCargo.DangerousGoods
		{
			get { return Array.Empty<UNDGDataItem>(); }
		}

		#endregion

		#region IServiceLocator Members

		public virtual object GetService(Type serviceType)
		{
			if (serviceType == typeof(ICustomsCharges))
			{
				return new InterfaceImplementations.JobDeclarationCustomsCharges(this);
			}
			return null;
		}

		#endregion

		#region IWeightHolder Members

		ZWeight IWeightHolder.TotalWeight
		{
			get { return GrossWeight; }
		}

		ZWeight IWeightHolder.TotalNetWeight
		{
			get { return ZWeight.Empty; }
		}

		IWeightApportionee[] IWeightHolder.AllApportionees
		{
			get
			{
				if (weightApportionees == null)
				{
					weightApportionees = new List<IWeightApportionee>(new TypedEnumerable<IWeightApportionee>(Invoices)).ToArray();
				}
				return weightApportionees;
			}
		}
		IWeightApportionee[] weightApportionees;

		public void ReApportionInvoiceWeightIfNeeded(bool reapportion)
		{
			weightApportionees = null;
			if (reapportion)
			{
				ApportionInvoiceWeight(null);
			}
		}

		IWeightHolder[] IWeightHolder.WeightHolders
		{
			get
			{
				if (weightHoldersCached == null)
				{
					weightHoldersCached = new CachedProperty<IWeightHolder[]>(Factory, delegate
					{
						return new List<IWeightHolder>(new TypedEnumerable<IWeightHolder>(Invoices)).ToArray();
					});
				}
				return weightHoldersCached.Value;
			}
		}
		CachedProperty<IWeightHolder[]> weightHoldersCached;

		#endregion

		#region IInvoicesProvider
		IEnumerable<BaseJobComInvoiceHeader> IInvoicesProvider.Invoices => Invoices;

		IInvoicesProviderLookups IInvoicesProvider.Lookups
		{
			get { return Lookups; }
		}

		IInvoicesProviderValueChangedAnnouncer IInvoicesProviderValueChangedAnnouncerProvider.GetValueChangedAnnouncer()
		{
			return new DeclarationValueChangedAnnouncer(this);
		}

		event EventHandler IInvoicesProvider.OnContainerDetailsChanged
		{
			add
			{
				JE_ContainerModeInfo.ValueChanged += value;
				JE_TransportModeInfo.ValueChanged += value;
			}
			remove
			{
				JE_ContainerModeInfo.ValueChanged -= value;
				JE_TransportModeInfo.ValueChanged -= value;
			}
		}

		event EventHandler IInvoicesProvider.OnPartAttributeCaptionDetailsChanged
		{
			add
			{
				onPartAttributeCaptionDetailsChanged -= value;
				onPartAttributeCaptionDetailsChanged += value;
				JE_OH_ImporterInfo.ValueChanged -= value;
				JE_OH_ImporterInfo.ValueChanged += value;
			}
			remove
			{
				onPartAttributeCaptionDetailsChanged -= value;
				JE_OH_ImporterInfo.ValueChanged -= value;
			}
		}
		event EventHandler onPartAttributeCaptionDetailsChanged;

		protected void OnPartAttributeCaptionDetailsChanged(object sender, EventArgs e)
		{
			onPartAttributeCaptionDetailsChanged?.Invoke(sender, e);
		}

		ZPropertyInfo IInvoicesProvider.MessageTypeInfo => JE_MessageTypeInfo;

		public virtual OrgHeader NewOwner => CustomsEntryInstructionProvider.CustomsEntryInstructions.OfType<CusEntryInstruction>().FirstOrDefault()?.Owner; //In ZA, validation ensures there is only one owner.

		event EventHandler IInvoicesProvider.OnNewOwnerPartAttributeCaptionDetailsChanged
		{
			add
			{
				onNewOwnerPartAttributeCaptionDetailsChanged -= value;
				onNewOwnerPartAttributeCaptionDetailsChanged += value;
			}
			remove
			{
				onNewOwnerPartAttributeCaptionDetailsChanged -= value;
			}
		}
		event EventHandler onNewOwnerPartAttributeCaptionDetailsChanged;

		public void OnNewOwnerPartAttributeCaptionDetailsChanged(object sender, EventArgs e)
		{
			onNewOwnerPartAttributeCaptionDetailsChanged?.Invoke(sender, e);
		}

		event EventHandler IInvoicesProvider.OnInvoiceLinesVisibilityChanged
		{
			add
			{
				JE_MessageSubTypeInfo.ValueChanged += value;
			}
			remove
			{
				JE_MessageSubTypeInfo.ValueChanged -= value;
			}
		}

		#endregion

		#region ICommonInvoiceDataProvider Members
		ICustomsFileParent ICommonInvoiceDataProvider.CustomsFileParent => IsPersistent ? this : Invoices.FirstOrDefault();
		void ICommonInvoiceDataProvider.ApportionWeightIfNeeded(IWeightHolder weightHolder, IWeightApportionee uncommittedApportionee)
		{
			if (ShouldRunWeightApportionment && WeightApportionmentEnabled)
			{
				WeightApportionManager.ApportionAll(weightHolder, uncommittedApportionee);
			}
		}

		void ICommonInvoiceDataProvider.CollapseOneBOMProductLine(BaseJobComInvoiceLine invoiceLine) => InvoiceLines.CollapseOneBOMProductLine(invoiceLine);
		void ICommonInvoiceDataProvider.ExpandOneBOMProductLine(BaseJobComInvoiceLine invoiceLine) => InvoiceLines.ExpandOneBOMProductLine(invoiceLine);
		IEnumerable<BaseJobComInvoiceHeader> ICommonInvoiceDataProvider.Invoices => Invoices;
		IEnumerable<BaseJobComInvoiceLine> ICommonInvoiceDataProvider.FilteredInvoiceLines => FilteredInvoiceLines;
		bool ICommonInvoiceDataProvider.CopyLastLineDetailsToNewLines { get => FilteredInvoiceLines.CopyLastLineDetailsToNewLines; set => FilteredInvoiceLines.CopyLastLineDetailsToNewLines = value; }
		#endregion ICommonInvoiceDataProvider

		[UniversalCopyCollectionEntity(CusEntryNumber.Schema.TableName, CusEntryNumber.Schema.CE_ParentID, CusEntryNumber.Schema.CE_ParentTable)]
		[ChildEditable()]
		public CusEntryNumAdditionalReferenceCollection AdditionalReferenceNumbers
		{
			get
			{
				if (additionalReferenceNumbers == null)
				{
					additionalReferenceNumbers = new CusEntryNumAdditionalReferenceCollection(this);
					additionalReferenceNumbers.Load();
					RegisterEditableChildObject(additionalReferenceNumbers);
				}

				return additionalReferenceNumbers;
			}
		}
		CusEntryNumAdditionalReferenceCollection additionalReferenceNumbers;

		#region Related Declarations

		public virtual bool UseGenPivotForRelatedDeclarations
		{
			get { return true; }
		}

		public RelatedDeclarationCollection RelatedDeclarations
		{
			get
			{
				if (relatedDeclarations == null)
				{
					relatedDeclarations = GetRelatedDeclarations();
					relatedDeclarations.ApplySort(JobDeclarationSchema.Constants.JE_DeclarationReference, System.ComponentModel.ListSortDirection.Ascending);
				}
				return relatedDeclarations;
			}
		}

		RelatedDeclarationCollection relatedDeclarations;

		BaseJobDeclaration ultimateParentRelatedDeclaration;
		public BaseJobDeclaration UltimateParentRelatedDeclaration
		{
			get { return ultimateParentRelatedDeclaration ?? (ultimateParentRelatedDeclaration = GetUltimateParentRelatedDeclaration()); }
		}

		protected BaseJobDeclaration GetUltimateParentRelatedDeclaration()
		{
			BaseJobDeclaration ultimateParentRelatedDeclaration = null;
			BaseJobDeclaration cursor = this;
			while (true)
			{
				if (cursor.ParentRelatedDeclaration is BaseJobDeclaration parentRelatedDeclaration)
				{
					ultimateParentRelatedDeclaration = parentRelatedDeclaration;
					cursor = parentRelatedDeclaration;
				}
				else
				{
					break;
				}
			}

			return ultimateParentRelatedDeclaration;
		}

		BaseJobDeclaration parentRelatedDeclaration;
		public BaseJobDeclaration ParentRelatedDeclaration
		{
			get { return parentRelatedDeclaration ?? (parentRelatedDeclaration = GetParentRelatedDeclaration()); }
		}

		protected virtual BaseJobDeclaration GetParentRelatedDeclaration()
		{
			var queryForParentRelatedDeclaration = new ZDBOnlyQuery(typeof(BaseJobDeclaration));
			var subQueryForParentRelatedDeclaration = new ZDBOnlySubQuery(typeof(GenPivot), GenPivotSchema.XX_Relation1ID);
			subQueryForParentRelatedDeclaration.AddToFilter(GenPivotSchema.XX_RelationType, RelatedDeclarationTypes);
			subQueryForParentRelatedDeclaration.AddToFilter(GenPivotSchema.XX_Relation2ID, PK);
			queryForParentRelatedDeclaration.AddSubQuery(subQueryForParentRelatedDeclaration, JoinCondition.And);
			return Factory.LoadTop1<BaseJobDeclaration>(queryForParentRelatedDeclaration);
		}

		internal protected virtual string[] RelatedDeclarationTypes => new[] { string.Empty };

		/// <summary>
		///
		/// </summary>
		/// <returns></returns>
		protected virtual RelatedDeclarationCollection GetRelatedDeclarations()
		{
			if (UseGenPivotForRelatedDeclarations)
			{
				return RelatedDeclarationCollection.GetPivotRelatedCollection(this);
			}
			else
			{
				throw new NotImplementedException("GetRelatedDeclarations must be implemented when not using GenPivot");
			}
		}

		#endregion

		#region IBillGenerationSupport Members

		OrgHeader IBillGenerationSupport.CarrierPrincipal
		{
			get { return ShippingLine; }
		}

		ZString IBillGenerationSupport.TranshipmentIndicator
		{
			get { return ""; }
		}

		RefUNLOCO IBillGenerationSupport.Destination
		{
			get { return FinalDestination; }
		}

		BusinessObjectFactory IBillGenerationSupport.Factory
		{
			get { return Factory; }
		}

		RefUNLOCO IBillGenerationSupport.Origin
		{
			get { return Origin; }
		}

		ZString IBillGenerationSupport.TransportMode
		{
			get
			{
				ZString result = Core.Constants.TransportModes.Other;
				if (IsAir)
				{
					result = Core.Constants.TransportModes.Air;
				}
				else if (IsSea)
				{
					result = Core.Constants.TransportModes.Sea;
				}
				else if (IsRail)
				{
					result = Core.Constants.TransportModes.Rail;
				}
				else if (IsRoad)
				{
					result = Core.Constants.TransportModes.Road;
				}
				return result;
			}
		}

		ZString IBillGenerationSupport.ServiceLevel
		{
			get { return JE_RS_NKServiceLevel; }
		}

		RefUNLOCO IBillGenerationSupport.Load
		{
			get { return null; }
		}

		RefUNLOCO IBillGenerationSupport.Discharge
		{
			get { return null; }
		}

		#endregion

		#region ICreditControlledDocumentDelivery Members

		event EventHandler<SecurityLoginEventArgs> ICreditControlledDocumentDelivery.GetDocumentLogin
		{
			add { fGetDocumentLogin += value; }
			remove { fGetDocumentLogin -= value; }
		}
		event EventHandler<SecurityLoginEventArgs> fGetDocumentLogin;

		CustomMessageBoxCallback ICreditControlledDocumentDelivery.DocumentLoginMessageBoxCallback { get; set; }

		OrgHeader[] ICreditControlledDocumentDelivery.OrganisationsForCreditChecks
		{
			get
			{
				var orgs = new List<OrgHeader>();
				if (Supplier != null && this.IsCreditLimitCheckRequired(OrgCodes.Consignor))
				{
					orgs.Add(Supplier);
				}
				if (Importer != null && this.IsCreditLimitCheckRequired(OrgCodes.Consignee))
				{
					orgs.Add(Importer);
				}
				if (Job != null && this.IsCreditLimitCheckRequired(OrgCodes.AllDebtors))
				{
					orgs.AddRange(Debtors);
				}
				if (AccountingMasterFilesRegistry.Instance.EnableCreditControlEvaluationForControllingCustomersAndControllingAgents.Value)
				{
					if (ControllingAgent is OrgHeader controllingAgent && this.IsCreditLimitCheckRequired(OrgCodes.ControllingAgent))
					{
						orgs.Add(controllingAgent);
					}
					if (ControllingCustomer is OrgHeader controllingCustomer && this.IsCreditLimitCheckRequired(OrgCodes.ControllingCustomer))
					{
						orgs.Add(controllingCustomer);
					}
				}

				var shipmentLocalCharges = Shipment?.ShipmentJobHeader?.LocalCharges;
				if (shipmentLocalCharges != null && this.IsCreditLimitCheckRequired(OrgCodes.LocalClient))
				{
					orgs.Add(shipmentLocalCharges);
				}
				if (Shipment != null && this.IsCreditLimitCheckRequired(OrgCodes.AllDebtors))
				{
					orgs.AddRange(Shipment.Debtors.Cast<DebtorToSelectFromForPrinting>().Select(x => x.Debtor));
				}

				return orgs.ToArray();
			}
		}

		bool ICreditControlledDocumentDelivery.IsDPSFreightMovementRestricted
		{
			get { return IsDPSFreightMovementRestrictedCore(); }
		}

		bool ICreditControlledDocumentDelivery.IsAviationSecurityFreightMovementRestricted => false;

		ScreeningParty[] ICreditControlledDocumentDelivery.GetScreeningParties()
		{
			return GetScreeningPartiesCore();
		}

		void ICreditControlledDocumentDelivery.RaiseOnGetDocumentLogin(SecurityLoginEventArgs e)
		{
			fGetDocumentLogin?.Invoke(this, e);
		}

		string ICreditControlledDocumentDelivery.DescriptionOfOrganisationBeingCheckedForCredit
		{
			get { return Res.GetString("6883B54E-65D1-425D-B5B7-1E77042E409C", "Importer, Supplier, Local Client for Billing or any Debtors in associated Shipment"); }
		}

		OrgHeader[] Debtors
		{
			get
			{
				var debtors = new List<OrgHeader>();
				if (Job != null)
				{
					var findChargesForJobQuery = new ZQuery(JobChargeSchema.JR_JH, Job.PK);
					var charges = Factory.Load<JobCharge>(new ZQuery(JobChargeSchema.JR_JH, Job.PK));

					var debtorPKs = charges.Where(x => !x.JR_OH_SellAccount.IsEmpty).Select(c => c.JR_OH_SellAccount).Distinct();
					if (debtorPKs.Any())
					{
						debtors.AddRange(Factory.Load<OrgHeader>(new ZQuery(OrgHeaderSchema.PK, debtorPKs.ToArray())));
					}
				}
				return debtors.ToArray();
			}
		}

		protected virtual bool IsDPSFreightMovementRestrictedCore()
		{
			return ObjectFactory.Get<IComplianceRiskStatusSupporter>().IsDPSFreightMovementRestricted(Shipment?.JS_ScreeningStatus ?? JE_ScreeningStatus, this, Shipment as BusinessObject ?? this);
		}

		protected virtual ScreeningParty[] GetScreeningPartiesCore()
		{
			return ((IScreeningPartyProvider)this).ScreeningParties;
		}

		#endregion

		public ZString JE_QuarantineMessagingRemarks
		{
			get
			{
				return QuarantineMessagingRemarksNoteManager.Value;
			}
			set
			{
				QuarantineMessagingRemarksNoteManager.Value = value;
				JE_QuarantineMessagingRemarksInfo.RefreshBinding();
			}
		}

		public virtual ZPropertyInfo JE_QuarantineMessagingRemarksInfo
		{
			get { return GetZPropertyInfo(Schema.JE_QuarantineMessagingRemarks); }
		}

		public int JE_QuarantineMessagingRemarks_MaxLength
		{
			get { return QuarantineMessagingRemarksNoteManager.MaxLength; }
		}

		ProxiedNotePropertyManager QuarantineMessagingRemarksNoteManager
		{
			get { return quarantineMessagingRemarksNoteManager ?? (quarantineMessagingRemarksNoteManager = new ProxiedNotePropertyManager(this, PredefinedNoteTypes.Instance.CustomsQuarantineMessagingRemarks)); }
		}
		ProxiedNotePropertyManager quarantineMessagingRemarksNoteManager;

		#region ISequenceNumberHeader Members

		IEnumerable<ISequenceNumberLine> ISequenceNumberHeader.Lines => Invoices.Cast<ISequenceNumberLine>();

		internal ShortSequenceNumberGenerator InvoiceNumberGenerator
		{
			get { return invoiceNumberGenerator ?? (invoiceNumberGenerator = new ShortSequenceNumberGenerator(this)); }
		}
		ShortSequenceNumberGenerator invoiceNumberGenerator;

		public IDisposable GetInvoiceNumberRenumberingSuspender()
		{
			return InvoiceNumberGenerator.GetLineNumberSuspender();
		}

		#endregion

		public ZString TransportModeGeneric
		{
			get { return GetTransportModeGeneric(); }
		}

		protected virtual ZString GetTransportModeGeneric()
		{
			switch (JE_TransportMode)
			{
				case TransportTypeList.Codes.Air:
					return TransportTypeGenericList.Codes.Air;
				case TransportTypeList.Codes.Mail:
					return TransportTypeGenericList.Codes.PostMail;
				case TransportTypeList.Codes.Sea:
					return TransportTypeGenericList.Codes.Sea;
				case Constants.TransportModes.Road:
					return TransportTypeGenericList.Codes.Road;
				case Constants.TransportModes.Rail:
					return TransportTypeGenericList.Codes.Rail;
				case Constants.TransportModes.FixedTransportInstallations:
					return TransportTypeGenericList.Codes.FixedTransportInstallations;
				case Constants.TransportModes.InlandWaterwayTransport:
					return TransportTypeGenericList.Codes.InlandWaterwayTransport;
				case Constants.TransportModes.OwnPropulsion:
					return TransportTypeGenericList.Codes.OwnPropulsion;
				case Constants.TransportModes.Other:
					return TransportTypeGenericList.Codes.Other;
			}
			return ZString.Empty;
		}

		#region IRegistryAccessingSupporter Members

		public virtual Guid RegistryCompanyPK
		{
			get
			{
				var result = Guid.Empty;
				var branch = Branch;
				if (branch == null)
				{
					result = GlbCompany.CurrentCompany.PK.ToGuid();
				}
				else if (!branch.GB_GC.IsEmpty)
				{
					result = branch.GB_GC.ToGuid();
				}
				return result;
			}
		}

		public virtual Guid RegistryBranchPK
		{
			get
			{
				var branch = Branch;
				return branch == null ? GlbBranch.CurrentBranch.PK.ToGuid() : branch.PK.ToGuid();
			}
		}

		#endregion

		#region ICustomsJobInfoProvider Members

		ICustomsJobInfo ICustomsJobInfoProvider.GetCustomsJobInfo(ZGuid companyPK)
		{
			return this;
		}

		#endregion

		#region IDeniedPartyProvider Member

		ZString IDeniedPartyProvider.ReferenceId => JE_DeclarationReference;

		#endregion

		#region IScreeningPartyProvider Members

		ZString IScreeningStatusProvider.ScreeningStatus
		{
			get { return JE_ScreeningStatus; }
			set { JE_ScreeningStatus = value; }
		}

		ZBool declarationShouldUpdateScreeningStatus;
		ZBool IShouldUpdateScreeningStatus.ShouldUpdateScreeningStatus
		{
			get
			{
				return declarationShouldUpdateScreeningStatus;
			}
			set
			{
				declarationShouldUpdateScreeningStatus = value;

				if (value && Shipment != null)
				{
					((IShouldUpdateScreeningStatus)Shipment).ShouldUpdateScreeningStatus = true;
				}
			}
		}

		ScreeningParty[] IScreeningPartyProvider.ScreeningParties
		{
			get
			{
				return GetScreeningParties().ToArray();
			}
		}

		protected virtual List<ScreeningParty> GetScreeningParties()
		{
			List<ScreeningParty> result = new List<ScreeningParty>();

			foreach (JobDocAddress docAddress in DocAddresses)
			{
				result.Add(new ScreeningParty(this, docAddress.AddressCaption, docAddress));
			}

			if (Job != null)
			{
				result.Add(new ScreeningParty(this, Res.GetString("cc271365-55ad-41b4-a582-7f95811841a9", "Local Client"), Job.LocalCharges));
			}

			result.Add(new ScreeningParty(this, Res.GetString("0a331fb4-3490-4241-ac1a-4c54fbdb1f86", "Supplier"), Supplier));

			result.Add(new ScreeningParty(this, Res.GetString("621276ce-d49a-4063-9c36-c57b47c4ff7e", "Importer"), Importer));

			result.Add(new ScreeningParty(this, Res.GetString("622e3a19-25aa-483f-871b-a16c00903000", "Carrier (Shipping Line)"), ShippingLine));

			result.Add(new ScreeningParty(this, Res.GetString("29fd561d-d595-42fe-817f-13b1bc8633c2", "Forwarder"), Forwarder));

			result.Add(new ScreeningParty(this, Res.GetString("0382fe86-7718-4e2d-915a-d43d43236679", "Delivery/Pickup Port Transport Company"), DeliveryOrPickupCartageCo));

			GetScreeningPartiesForVessels(result);

			return result;
		}

		void GetScreeningPartiesForVessels(List<ScreeningParty> screeningParties)
		{
			if (JE_TransportMode == Constants.TransportModes.Sea && !string.IsNullOrWhiteSpace(JE_VesselName))
			{
				var hasRefVessel = RefVessel.LookupVesselByCode(JE_VesselName, Factory, true);
				if (hasRefVessel != null)
				{
					AddScreeningVessel(((IScreeningPartyProvider)hasRefVessel).ScreeningParties);
				}
			}

			foreach (Transport transport in Transports)
			{
				if (transport.JW_TransportMode == Constants.TransportModes.Sea)
				{
					if (!transport.JW_Vessel.IsEmpty)
					{
						if (transport.Vessel is RefVessel vessel)
						{
							AddScreeningVessel(((IScreeningPartyProvider)vessel).ScreeningParties);
						}
						else
						{
							AddScreeningVessel(((IScreeningPartyProvider)transport).ScreeningParties);
						}
					}

					screeningParties.Add(new ScreeningParty(this, Res.GetString("cbc558ab-c58f-45a1-a3c9-a1ef654ff5a6", "Routing Carrier"), transport.Carrier));
				}
			}

			void AddScreeningVessel(ScreeningParty[] parties)
			{
				parties.ForEach(p => p.AddParent(this));
				screeningParties.AddRange(parties);
			}
		}

		ZString IScreeningPartyProvider.GetWorstScreeningStatus()
		{
			if (Shipment is IScreeningPartyProvider provider)
			{
				return provider.GetWorstScreeningStatus();
			}

			var statuses = new List<ZString>();
			GetScreeningStatus(statuses);
			return ScreeningStatusUpdater.GetWorstScreeningStatus(statuses);
		}

		ZString IScreeningPartyProvider.GetWorstScreeningStatusUnlessManuallyCleared()
		{
			var statuses = new List<ZString>();
			GetScreeningStatus(statuses);

			return ScreeningStatusUpdater.GetWorstScreeningStatus(statuses);
		}

		protected virtual void GetScreeningStatus(List<ZString> statuses)
		{
			AddScreeningStatusToList(statuses, Job?.LocalCharges);
			AddScreeningStatusToList(statuses, Supplier);
			AddScreeningStatusToList(statuses, Importer);
			AddScreeningStatusToList(statuses, ShippingLine);
			AddScreeningStatusToList(statuses, Forwarder);
			AddScreeningStatusToList(statuses, DeliveryOrPickupCartageCo);

			AddScreeningStatusToList(statuses, GetWorstVesselScreeningStatus());
			AddScreeningStatusToList(statuses, GetWorstDocAddressScreeningStatus());
		}

		ZString GetWorstVesselScreeningStatus()
		{
			var screeningParties = new List<IScreeningPartyProvider>();
			var transportVessels = Transports.Cast<Transport>()
				.Where(transport => transport.JW_TransportMode == Constants.TransportModes.Sea && !transport.JW_Vessel.IsEmpty)
				.Select(transport => transport.Vessel is RefVessel vessel ? vessel as IScreeningPartyProvider : transport);

			if (transportVessels.Any())
			{
				screeningParties.AddRange(transportVessels);
			}

			if (JE_TransportMode == Constants.TransportModes.Sea && !JE_VesselName.IsEmpty)
			{
				var hasRefVessel = RefVessel.LookupVesselByCode(JE_VesselName, Factory, true);
				if (hasRefVessel != null)
				{
					screeningParties.Add(hasRefVessel);
				}
			}

			return ScreeningStatusUpdater.GetWorstScreeningStatus(screeningParties);
		}

		ZString GetWorstDocAddressScreeningStatus()
		{
			return ScreeningStatusUpdater.GetWorstScreeningStatus(DocAddresses.Cast<IScreeningPartyProvider>());
		}

		protected void AddScreeningStatusToList(List<ZString> list, IScreeningPartyProvider provider)
		{
			var status = provider?.GetWorstScreeningStatus();
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
				return RelatedOrgPartyScreeningStatusHelper.GetRelatedOrgPartyScreeningStatusCollection(Factory, ((IScreeningPartyProvider)this).ScreeningParties, relatedJobPKs: PK);
			}
		}

		#endregion

		#region IJobDeclarationUpdater Members

		ZString Freight.Integration.Forwarding.IJobDeclarationSailingDateUpdater.UpdateETA(ZDateTime arrivalDate, long ticks)
		{
			ZString result = ShouldUpdateDates(arrivalDate);
			if (result.IsEmpty)
			{
				ZDateTime newDestinationETA = (JE_DateAtFinalDestination.IsEmpty || !JE_DateAtFinalDestination.IsValid) ? ZDateTime.Empty : JE_DateAtFinalDestination.AddTicks(ticks);
				JE_DateAtFinalDestination = newDestinationETA;
				result = Res.GetString("696158cb-a751-4264-a364-bd325992ea83", "{0} ETA updated", JE_DeclarationReference);
			}
			return result;
		}

		ZString Freight.Integration.Forwarding.IJobDeclarationSailingDateUpdater.UpdateETD(ZDateTime departureDate, long ticks)
		{
			ZString result = ShouldUpdateDates(departureDate);
			if (result.IsEmpty)
			{
				ZDateTime newOriginETD = (JE_DateAtOrigin.IsEmpty || !JE_DateAtOrigin.IsValid) ? ZDateTime.Empty : JE_DateAtOrigin.AddTicks(ticks);
				JE_DateAtOrigin = newOriginETD;
				result = Res.GetString("37723a2b-d2dc-4aeb-b26a-bd8f62828440", "{0} ETD updated", JE_DeclarationReference);
			}
			return result;
		}

		ZString ShouldUpdateDates(ZDateTime arrivalOrDepartureDate)
		{
			ZString result = ZString.Empty;
			if (!arrivalOrDepartureDate.IsValid)
			{
				result = Res.GetString("c272f22e-b9a6-449e-8851-f4855db0fd9e", "{0} has not been updated as no valid date has been supplied", JE_DeclarationReference);
			}
			else if (DeclarationMessagesHaveBeenSent())
			{
				result = Res.GetString("3c7f1427-a8cf-4e5a-b751-8a23efcc2296", "{0} has not been updated as messages have been sent", JE_DeclarationReference);
			}
			else if (ShouldSynchroniseWithShipment())
			{
				result = Res.GetString("45acbb4d-0ba8-4f3b-8cf5-8e4cf4ca29d2", "{0} has not been updated as it is synchronized with shipment information", JE_DeclarationReference);
			}
			return result;
		}

		#endregion

		#region IBuyerSupplierRelationshipConsumer Members

		IDisposable SuspendTransportModeRestoration()
		{
			return BuyerSupplierLinksHelper == null ? new DisposableObject() : BuyerSupplierLinksHelper.SuspendTransportModeRestoration();
		}

		public BuyerSupplierLinksHelper<BaseJobDeclaration> BuyerSupplierLinksHelper;

		OrgHeader IBuyerSupplierRelationshipConsumer.Consignee
		{
			get { return Consignee; }
			set { JE_OH_Importer = value.PK; }
		}

		event EventHandler IBuyerSupplierRelationshipConsumer.ConsigneeChanged
		{
			add { JE_OH_ImporterInfo.ValueChanged += value; }
			remove { JE_OH_ImporterInfo.ValueChanged -= value; }
		}

		JobDocAddress IBuyerSupplierRelationshipConsumer.ConsigneeDeliveryAddress
		{
			get { return ImporterDeliveryAddress; }
		}

		OrgHeader IBuyerSupplierRelationshipConsumer.Consignor
		{
			get { return Consignor; }
			set { JE_OH_Supplier = value.PK; }
		}

		event EventHandler IBuyerSupplierRelationshipConsumer.ConsignorChanged
		{
			add { JE_OH_SupplierInfo.ValueChanged += value; }
			remove { JE_OH_SupplierInfo.ValueChanged -= value; }
		}

		JobDocAddress IBuyerSupplierRelationshipConsumer.ConsignorPickupAddress
		{
			get { return SupplierPickupAddress; }
		}

		ZString IBuyerSupplierRelationshipConsumer.ContainerMode
		{
			get { return JE_ContainerMode; }
			set { JE_ContainerMode = value; }
		}

		ZGuid IBuyerSupplierRelationshipConsumer.DeliveryCartageCoPK
		{
			get { return DocsAndCartage?.DeliveryCartageCoPK ?? ZGuid.Empty; }
			set
			{
				var docsAndCartage = DocsAndCartage;
				if (docsAndCartage != null)
				{
					SetIfIsValid((ZPropertyInfoGuid)docsAndCartage.DeliveryCartageCoPKInfo, value);
				}
			}
		}

		ZString IBuyerSupplierRelationshipConsumer.Destination
		{
			get { return JE_RL_NKFinalDestination; }
			set { JE_RL_NKFinalDestination = value; }
		}

		ZString IBuyerSupplierRelationshipConsumer.DischargePort
		{
			get { return JE_RL_NKPortOfArrival; }
			set { JE_RL_NKPortOfArrival = value; }
		}

		ZString IBuyerSupplierRelationshipConsumer.GoodsCurrency
		{
			get { return ZString.Empty; }
			set { }
		}

		ZString IBuyerSupplierRelationshipConsumer.GoodsDescription
		{
			get { return JE_GoodsDescription; }
			set { JE_GoodsDescription = value; }
		}

		ZGuid IBuyerSupplierRelationshipConsumer.ImportBrokerPK
		{
			get { return ZGuid.Empty; }
			set { }
		}

		ZBool IBuyerSupplierRelationshipConsumer.IsSettingDefaultValues
		{
			get { return false; }
			set { }
		}

		ZString IBuyerSupplierRelationshipConsumer.LoadPort
		{
			get { return JE_RL_NKPortOfLoading; }
			set { JE_RL_NKPortOfLoading = value; }
		}

		event EventHandler IBuyerSupplierRelationshipConsumer.ModesChanged
		{
			add
			{
				JE_TransportModeInfo.ValueChanged += value;
				JE_ContainerModeInfo.ValueChanged += value;
				DestinationCountryChanged += value;
			}
			remove
			{
				JE_TransportModeInfo.ValueChanged -= value;
				JE_ContainerModeInfo.ValueChanged -= value;
				DestinationCountryChanged -= value;
			}
		}

		event EventHandler DestinationCountryChanged;

		ZByte IBuyerSupplierRelationshipConsumer.NoCopyBills
		{
			get { return ZByte.Zero; }
			set { }
		}

		ZByte IBuyerSupplierRelationshipConsumer.NoOriginalBills
		{
			get { return ZByte.Zero; }
			set { }
		}

		void IBuyerSupplierRelationshipConsumer.RestoreNumberOfBillsWithFallback()
		{
		}

		JobDocAddress IBuyerSupplierRelationshipConsumer.NotifyPartyDocumentaryAddress
		{
			get { return null; }
		}

		ZString IBuyerSupplierRelationshipConsumer.Origin
		{
			get { return JE_RL_NKOrigin; }
			set { JE_RL_NKOrigin = value; }
		}

		ZString IBuyerSupplierRelationshipConsumer.PaymentTerms
		{
			get { return ZString.Empty; }
			set { }
		}

		ZGuid IBuyerSupplierRelationshipConsumer.PickupCartageCoPK
		{
			get { return DocsAndCartage?.PickupCartageCoPK ?? ZGuid.Empty; }
			set
			{
				var docsAndCartage = DocsAndCartage;
				if (docsAndCartage != null)
				{
					SetIfIsValid((ZPropertyInfoGuid)docsAndCartage.PickupCartageCoPKInfo, value);
				}
			}
		}

		ZGuid IBuyerSupplierRelationshipConsumer.ReceivingAgentPK
		{
			get { return ZGuid.Empty; }
			set { }
		}

		ZString IBuyerSupplierRelationshipConsumer.ReleaseType
		{
			get { return ZString.Empty; }
		}

		void IBuyerSupplierRelationshipConsumer.RestoreGoodsCurrencyFallback()
		{
		}

		void IBuyerSupplierRelationshipConsumer.RestoreReceivingAgentFallback()
		{
		}

		void IBuyerSupplierRelationshipConsumer.RestoreSendingAgentFallback()
		{
		}

		void IBuyerSupplierRelationshipConsumer.RestoreServiceLevelFallback()
		{
		}

		ZGuid IBuyerSupplierRelationshipConsumer.SendingAgentPK
		{
			get { return ZGuid.Empty; }
			set { }
		}

		ZString IBuyerSupplierRelationshipConsumer.ServiceLevel
		{
			get { return JE_RS_NKServiceLevel; }
			set { JE_RS_NKServiceLevel = value; }
		}

		ZGuid IBuyerSupplierRelationshipConsumer.ShippingLinePK
		{
			get { return ZGuid.Empty; }
			set { JE_OH_ShippingLine = value; }
		}

		ZBool IBuyerSupplierRelationshipConsumer.ShouldPromptToSaveBuyerSupplierRelationship
		{
			get { return ShouldPromptToSaveBuyerSupplierRelationship; }
		}

		protected virtual ZBool ShouldPromptToSaveBuyerSupplierRelationship
		{
			get { return Env.Registry.PromptToSaveBuyerSupplier; }
		}

		ZBool IBuyerSupplierRelationshipConsumer.ShouldRestorePaymentTerm
		{
			get { return true; }
		}

		ZBool IBuyerSupplierRelationshipConsumer.ShouldRestoreHandlingInformation
		{
			get { return IsStandAlone; }
		}

		ZBool IBuyerSupplierRelationshipConsumer.ShouldRestorePickupDeliveryAndNotifyPartyAddress
		{
			get { return IsStandAlone; }
		}

		ZString IBuyerSupplierRelationshipConsumer.TransportMode
		{
			get { return JE_TransportMode; }
			set
			{
				if (!IsNonTransportDeclarationType)
				{
					JE_TransportMode = value;
				}
			}
		}

		ZBool IBuyerSupplierRelationshipConsumer.PreventBuyerSupplierRelationships
		{
			get { return PreventBuyerSupplierRelationshipsCore; }
		}

		protected virtual ZBool PreventBuyerSupplierRelationshipsCore
		{
			get { return false; }
		}

		ZBool IBuyerSupplierRelationshipConsumer.ShouldDefaultContainerModeAndIsContainerised(ZString containerMode) => ShouldDefaultContainerModeAndIsContainerisedCore(containerMode);

		protected virtual ZBool ShouldDefaultContainerModeAndIsContainerisedCore(ZString containerMode) => false;

		#endregion

		#region Buyer / Supplier / Buyer Links

		#region Buyer Supplier Relationship Link

		Hashtable BuyerSupplierLinks
		{
			get
			{
				if (fBuyerSupplierLinks == null)
				{
					fBuyerSupplierLinks = new Hashtable();
				}
				return fBuyerSupplierLinks;
			}
		}

		Hashtable fBuyerSupplierLinks;

		public bool BuyerSupplierContainsPair(OrgHeader buyer, OrgHeader supplier)
		{
			return BuyerSupplierLinks.ContainsKey(GetBuyerSupplierKey(buyer, supplier));
		}

		public void AddToBuyerSupplierLinks(OrgHeader buyer, OrgHeader supplier, BaseJobDeclaration declaration)
		{
			BuyerSupplierLinks.Add(GetBuyerSupplierKey(buyer, supplier), declaration);
		}

		string GetBuyerSupplierKey(OrgHeader buyer, OrgHeader supplier)
		{
			return buyer.PK + "|" + supplier.PK;
		}

		#endregion

		#endregion

		#region Invoicing Supporter

		public class BaseJobDeclarationInvoicingSupporter : JobInvoicingSupporter, IServiceDirection
		{
			public BaseJobDeclarationInvoicingSupporter(BaseJobDeclaration parent)
				: base(parent)
			{
				Parent = parent;
			}

			protected readonly BaseJobDeclaration Parent;

			public override ZGuid OverriddenDepartmentPK
			{
				get { return OverridenDepartment; }
			}

			public override ZString ServiceLevel => Parent.JE_RS_NKServiceLevel;

			public override ZString ContainerMode
			{
				get { return Parent.FreightContainerMode; }
			}

			protected virtual ZGuid OverridenDepartment
			{
				get { return ZGuid.Empty; }
			}

			public override PaymentTermInfos PaymentTerm
			{
				get { return Parent.RatingAdapter.PaymentTerm; }
			}

			public override OrgHeader Consignee
			{
				get { return Parent.Consignee; }
			}

			public override OrgHeader Consignor
			{
				get { return Parent.Consignor; }
			}

			public override JobInvoicingConsumerType ConsumerType
			{
				get { return JobInvoicingConsumerTypes.Brokerage; }
			}

			public override ZString HouseBillNumber
			{
				get { return Parent.Shipment != null ? ((IJobInvoicingPlugIn)Parent.Shipment).InvoicingSupporter.HouseBillNumber : Parent.JE_HouseBill; }
			}

			public override ZString MasterBillNumber
			{
				get { return Parent.Shipment != null ? ((IJobInvoicingPlugIn)Parent.Shipment).InvoicingSupporter.MasterBillNumber : Parent.JE_MasterBill; }
			}

			public override ZDateTime ATA
			{
				get { return Parent.JE_DateOfArrival; }
			}

			public override ZDateTime ATD
			{
				get { return Parent.JE_ExportDate; }
			}

			public override ZDateTime ETA
			{
				get { return Parent.JE_DateAtFinalDestination; }
			}

			public override ZDateTime ETD
			{
				get { return Parent.JE_DateAtOrigin; }
			}

			public override ZDateTime ESP
			{
				get { return Parent.JE_EstimatedDeliveryOrPickup; }
			}

			public override ZDateTime ESD
			{
				get { return Parent.JE_EstimatedDeliveryOrPickup; }
			}

			public override ZDateTime ActualPickupDate
			{
				get { return Parent.JE_CartageCompleted; }
			}

			public override ZDateTime ActualDeliveryDate
			{
				get { return Parent.JE_CartageCompleted; }
			}

			public override ZDecimal ActualChargeable { get { return Parent.JE_TotalWeight; } }

			public override ZString ActualChargeableUnit { get { return Parent.JE_TotalWeightUnit; } }

			public override ZDecimal ActualWeight
			{
				get { return Parent.JE_TotalWeight; }
			}

			public override ZString ActualWeightUnit
			{
				get { return Parent.JE_TotalWeightUnit; }
			}

			public override ZDecimal ActualVolume
			{
				get { return Parent.JE_TotalVolume; }
			}

			public override ZString ActualVolumeUnit
			{
				get { return Parent.JE_TotalVolumeUnit; }
			}

			public override bool CreateAccountingJobOnSavingOfOperationsJob
			{
				get { return !Parent.IsPluggedIntoShipment; }
			}

			protected override SecurityCheckpoint GetAuditSecurityCore()
			{
				return Env.Security.CustomsDeclarationAudit;
			}

			protected override SecurityCheckpoint GetJobInvoicingSecurityCore()
			{
				return Env.Security.CustomsDeclarationJobInvoicing;
			}

			#region Interface items not applicable to Customs

			public override RefUNLOCO Destination
			{
				get { return Parent.FinalDestination; }
			}

			public ZString ServiceDirection
			{
				get { return Parent.JE_MessageType; }
			}

			public override ZString TransportMode
			{
				get { return JobInvoicingTransportMode; }
			}

			protected virtual ZString JobInvoicingTransportMode
			{
				get { return Parent.JE_TransportMode; }
			}

			public override ZString ConsolType
			{
				get { return Constants.JobInvoicingDefaultDepartmentConsolType.NoConsol; }
			}

			public override ZDateTime GetOperationsSignificantDate(string significantDateCode)
			{
				ZDateTime resultRevenueRecognitionDate = ZDateTime.Empty;

				if (significantDateCode == AccountingMasterFilesConstants.SignificantDateCodes.CustomsClearanceDate)
				{
					resultRevenueRecognitionDate = GetCustomsClearanceDate();
				}
				else if (significantDateCode == AccountingMasterFilesConstants.SignificantDateCodes.PickupDate)
				{
					resultRevenueRecognitionDate = GetPickupDate();
				}
				else if (significantDateCode == AccountingMasterFilesConstants.SignificantDateCodes.DeliveryDate)
				{
					resultRevenueRecognitionDate = GetDeliveryDate();
				}

				return resultRevenueRecognitionDate;
			}

			public override ZDateTime GetOperationsSignificantDateByDirection(string significantDateCode, string direction)
			{
				return GetOperationsSignificantDate(significantDateCode);
			}

			public override ZDateTime GetCustomsClearanceDate()
			{
				ZDateTime customsClearanceDate = ZDateTime.Empty;

				StmALog mostRecentClearedLog = Parent.LogsOfDeclarationOrShipment.MostRecentLogByEventTime(Parent.CustomsClearedEventType, new ZQuery(StmALogSchema.SL_IsEstimate, false)) ?? Parent.LogsOfDeclarationOrShipment.MostRecentLogByEventTime(Parent.CustomsClearedEventType, new ZQuery(StmALogSchema.SL_IsEstimate, true));

				if (mostRecentClearedLog != null)
				{
					customsClearanceDate = mostRecentClearedLog.SL_EventTime;
				}

				return customsClearanceDate;
			}

			protected override ZDateTime GetPickupDate()
			{
				if (!Parent.IsDeleted && Parent.DocsAndCartage is JobDocsAndCartage docsAndCartage
					&& (docsAndCartage.JP_PickupCartageCompleted.IsValid || docsAndCartage.JP_EstimatedPickup.IsValid))
				{
					return docsAndCartage.JP_PickupCartageCompleted.IsValid ? docsAndCartage.JP_PickupCartageCompleted : docsAndCartage.JP_EstimatedPickup;
				}
				return ZDateTime.Empty;
			}

			protected override ZDateTime GetDeliveryDate()
			{
				if (!Parent.IsDeleted && Parent.DocsAndCartage is JobDocsAndCartage docsAndCartage
					&& (docsAndCartage.JP_DeliveryCartageCompleted.IsValid || docsAndCartage.JP_EstimatedDelivery.IsValid))
				{
					return docsAndCartage.JP_DeliveryCartageCompleted.IsValid ? docsAndCartage.JP_DeliveryCartageCompleted : docsAndCartage.JP_EstimatedDelivery;
				}
				return ZDateTime.Empty;
			}

			#endregion

			public override ZString EditSecurityMessage
			{
				get { return EditSecurityMessageCore; }
			}

			protected virtual ZString EditSecurityMessageCore
			{
				get { return ZString.Empty; }
			}

			public override bool EditSecurityLock
			{
				get { return EditSecurityLockCore; }
			}

			protected virtual bool EditSecurityLockCore
			{
				get { return false; }
			}

			public override int ContainerCount
			{
				get { return Parent.CusContainers.Count; }
			}

			public override bool IsImport
			{
				get { return Parent.IsImport; }
			}

			public override bool IsExport
			{
				get { return Parent.IsExport; }
			}

			public override bool IsDomestic
			{
				get { return Parent.IsDomestic; }
			}

			public override RefUNLOCO Origin
			{
				get
				{
					return Parent.Origin;
				}
			}

			public override ZString VoyageVesselOrFlightDate
			{
				get
				{
					var departureDate = !Parent.JE_ExportDate.IsEmpty ? Parent.JE_ExportDate : Parent.JE_DateAtOrigin;
					return GetVoyageVesselOrFlightDatesCore(Parent.JE_TransportMode, departureDate, Parent.JE_VesselName, Parent.JE_VoyageFlightNo);
				}
			}
		}

		#endregion

		#region Document Override properties and methods

		public ZString GetOwnersRefOverrideForDocuments(OrgHeader debtor)
		{
			return GetOwnersRefOverrideForDocumentsCore(debtor);
		}

		protected virtual ZString GetOwnersRefOverrideForDocumentsCore(OrgHeader debtor)
		{
			return ZString.Empty;
		}

		public ZString OriginForDocuments
		{
			get { return OriginForDocumentsCore; }
		}

		protected virtual ZString OriginForDocumentsCore
		{
			get { return JE_RL_NKOrigin; }
		}

		public ZString FinalDestinationForDocuments
		{
			get { return FinalDestinationForDocumentsCore; }
		}

		protected virtual ZString FinalDestinationForDocumentsCore
		{
			get { return JE_RL_NKFinalDestination; }
		}

		public bool ShowInvoiceNumbersOnDocuments
		{
			get { return ShowInvoiceNumbersOnDocumentsCore; }
		}

		protected virtual bool ShowInvoiceNumbersOnDocumentsCore
		{
			get { return true; }
		}

		public ZString GoodsDescriptionForDocuments
		{
			get { return GoodsDescriptionForDocumentsCore; }
		}

		protected virtual ZString GoodsDescriptionForDocumentsCore
		{
			get { return JE_GoodsDescriptionDetailed; }
		}

		#endregion

		#region ICustomFieldProvider Members

		CustomBusinessObject ICustomFieldProvider.GetCustomBusinessObject(bool shouldRefresh)
		{
			if (customBusinessObject == null || shouldRefresh)
			{
				var properties = new UserDefinedPropertyCollection(this);

				var customFieldsDescriptor = new JobDocsAndCartageCustomFieldsDescriptor();
				foreach (var customFieldInfo in customFieldsDescriptor.ActiveCustomFieldsInfos)
				{
					properties.Add(customFieldsDescriptor.BindTo("DocsAndCartage", customFieldInfo), customFieldInfo.Caption);
				}

				properties.WithWorkflowTemplateCustomFields(this);

				customBusinessObject = new CustomBusinessObject(Factory, this, properties);
			}

			return customBusinessObject;
		}

		CustomBusinessObject customBusinessObject;

		#endregion

		#region IJobDeclarationWithShipmentSynchonisation Members

		void Integration.Customs.IJobDeclarationWithShipmentSynchonisation.SynchroniseWithShipmentIfNeeded() => SynchroniseWithShipmentIfNeededCore();

		ZBool Integration.Customs.IJobDeclarationWithShipmentSynchonisation.ShouldSynchroniseWithShipmentForDocument => ShouldSynchroniseWithShipmentForDocumentCore;

		protected virtual ZBool ShouldSynchroniseWithShipmentForDocumentCore
		{
			get
			{
				return !ActiveEntryHeaders.Any();
			}
		}

		protected virtual void SynchroniseWithShipmentIfNeededCore()
		{
			if (ShouldSynchroniseWithShipment())
			{
				using (GetValidationSuspender())
				{
					ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));
					ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Start));
					Messages.CountChanged += new CollectionCountChangedEventHandler(StopSynchronisationOnMessages_CountChanged);
					CustomsEntryHeaders.CountChanged += new CollectionCountChangedEventHandler(CustomsEntryHeaders_CountChanged);
					foreach (CusEntryHeader cusHeader in CustomsEntryHeaders)
					{
						cusHeader.Messages.CountChanged += new CollectionCountChangedEventHandler(StopSynchronisationOnMessages_CountChanged);
					}
				}
			}
			RefreshBindingAndChildrenReadOnly();
		}

		void StopSynchronisationOnMessages_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			if (DeclarationMessagesHaveBeenSent())
			{
				Messages.CountChanged -= new CollectionCountChangedEventHandler(StopSynchronisationOnMessages_CountChanged);
				CustomsEntryHeaders.CountChanged -= new CollectionCountChangedEventHandler(CustomsEntryHeaders_CountChanged);
				foreach (CusEntryHeader cusHeader in CustomsEntryHeaders)
				{
					cusHeader.Messages.CountChanged -= new CollectionCountChangedEventHandler(StopSynchronisationOnMessages_CountChanged);
				}

				ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Stop));
				RefreshBindingAndChildrenReadOnly();
			}
		}

		void CustomsEntryHeaders_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			if (e.ItemAdded)
			{
				if (e.BizObject is CusEntryHeader header)
				{
					header.Messages.CountChanged += new CollectionCountChangedEventHandler(StopSynchronisationOnMessages_CountChanged);
				}
			}
			else
			{
				if (e.BizObject is CusEntryHeader header)
				{
					header.Messages.CountChanged += new CollectionCountChangedEventHandler(StopSynchronisationOnMessages_CountChanged);
				}
			}
		}

		ZString Integration.Customs.IBaseJobDeclaration.GetContainerMode(ZString transportMode, ZString shipmentPackingMode)
		{
			return GetContainerModeForDeclaration(transportMode, shipmentPackingMode);
		}

		ZString Integration.Customs.IBaseJobDeclaration.GetCreditCheckMessage()
		{
			var result = ZString.Empty;
			if (((IValidateForCustomsMessagingSupporter)this).SupportValidateCustomsMessaging)
			{
				using (MarkDeclarationIsValidatingCustomsMessaging())
				{
					var checker = new MessageManagerCreditCheckWithSecurityHelper(this, true);
					if (!checker.IsCreditCheckOKToSend)
					{
						result = checker.ReasonForNotAllowed;
					}
				}
			}
			return result;
		}

		public virtual bool IsCreditCheckEnabledForValidateCustomsMessaging => CustomsDataRegistry.Instance.CreditCheckOnMessageSend.GetValueWithoutFallback(CompanyPK.ToGuid(), Guid.Empty, Guid.Empty);

		Integration.Customs.IInvoiceHeaderActiveCollection Integration.Customs.IBaseJobDeclaration.Invoices => Invoices;

		#endregion

		#region IWorkflowTriggerEventSource Members

		IGlbCompany IWorkflowTriggerEventSource.JobHeaderCompany
		{
			get { return Company; }
		}

		IReadOnlyList<IWorkflowProviderCore> IWorkflowTriggerEventSource.ParentWorkflowProviders
		{
			get
			{
				var list = new List<IWorkflowProviderCore>();
				var shipment = Shipment;
				if (shipment != null)
				{
					list.Add(shipment);
				}
				return list;
			}
		}

		#endregion

		#region IWorkflowTriggerFieldChangeSource Members
		IReadOnlyList<IWorkflowProvider> IWorkflowTriggerFieldChangeSource.ParentWorkflowProviders
		{
			get
			{
				var workflowProvider = Shipment;

				return workflowProvider == null ?
					Array.Empty<IWorkflowProvider>() :
					new IWorkflowProvider[] { workflowProvider };
			}
		}
		#endregion

		#region IStmALogParentProvider Members

		IStmALogParent IStmALogParentProvider.LogParent
		{
			get { return LogParent; }
		}

		IStmALogParent LogParent
		{
			get
			{
				var shipment = Shipment;
				if (shipment != null)
				{
					return shipment;
				}
				else
				{
					return this;
				}
			}
		}

		#endregion

		#region IAdditionalReferenceNumberTypeProvider Members

		CodeDescriptionPairList IAdditionalReferenceNumberTypeProvider.GetAdditionalReferenceNumberTypeList(ZString category, ZString countryCode)
		{
			CodeDescriptionPairList result = null;
			if (category == CusEntryNumber.Categories.AdditionalReferenceNumber)
			{
				IAdditionalReferenceNumberSupporter additionalRefNumberSupporter = this as IAdditionalReferenceNumberSupporter;
				result = CusEntryNumLookups.GetAdditionalReferenceNumberTypes(Factory, countryCode, additionalRefNumberSupporter != null && additionalRefNumberSupporter.IncludeSpecialCustomsInstructionsItems);
			}
			else
			{
				result = GetAdditionalReferenceNumberTypeListCore(category, countryCode);
			}
			return result;
		}

		protected virtual CodeDescriptionPairList GetAdditionalReferenceNumberTypeListCore(ZString category, ZString countryCode)
		{
			return Lookups.MessageTypeList;
		}

		#endregion

		#region IAdditionalReferenceNumberValidationProvider Members

		void IAdditionalReferenceNumberValidationProvider.ValidateEntryType(ZPropertyInfo entryTypeInfo, ZString category, ZString countryCode)
		{
			ValidateAdditionalReferenceNumberTypeCore(entryTypeInfo, category, countryCode);
		}

		protected virtual void ValidateAdditionalReferenceNumberTypeCore(ZPropertyInfo entryTypeInfo, ZString category, ZString countryCode)
		{
		}

		bool IAdditionalReferenceNumberValidationProvider.EntryTypeShouldBeUnique(ZString entryType, ZString category, ZString countryCode)
		{
			return EntryTypeShouldBeUniqueCore(entryType, category, countryCode);
		}

		protected virtual bool EntryTypeShouldBeUniqueCore(ZString entryType, ZString category, ZString countryCode)
		{
			return true;
		}
		#endregion

		#region IControllerIDProvider Members

		ControllerID IControllerIDProvider.ControllerID
		{
			get { return IsStandAlone ? ControllerIDs.Customs.JobDeclaration : ControllerIDs.Customs.JobDeclarationPluggedIntoShipment; }
		}

		Guid IControllerIDProvider.BusinessObjectPK
		{
			get { return PK.ToGuid(); }
		}

		#endregion

		#region IMessageSenderSupporter Members

		bool IMessageSenderSupporter.HasChanges
		{
			get
			{
				var shipment = Shipment;
				return shipment != null ? shipment.HasChanges : HasChanges;
			}
		}

		bool IMessageSenderSupporter.HasErrors
		{
			get
			{
				var shipment = Shipment;
				return shipment != null ? shipment.HasErrors : HasErrors;
			}
		}

		BusinessObject IMessageSenderSupporter.BusinessObjectForNotifications
		{
			get { return this; }
		}

		#endregion

		public bool IsDeclarationIntegrated => IsDeclarationIntegratedCore();

		protected virtual bool IsDeclarationIntegratedCore()
		{
			return IsInterfaceEnabledCompany && (IsInterface || JE_ApplicationCode.IsEmpty);
		}

		public event EventHandler OnIsDeclarationIntegratedChanged
		{
			add => JE_ApplicationCodeInfo.ValueChanged += value;
			remove => JE_ApplicationCodeInfo.ValueChanged -= value;
		}

		public bool ShowSubmitMenuItem => ShowSubmitMenuItemCore();

		protected virtual bool ShowSubmitMenuItemCore()
		{
			return (IsInterface || JE_ApplicationCode.IsEmpty);
		}

		public bool ShowApportionmentMenuItem => ShowApportionmentMenuItemCore;
		protected virtual bool ShowApportionmentMenuItemCore => !IsDeclarationIntegrated;
		protected bool IsInterfaceEnabledCompany
		{
			get
			{
				var country = CountryCode;
				return !country.IsEmpty && IntegratedCountryHelper.IsInterfaceEnabledCompany(JE_GC, country);
			}
		}

		internal bool IsABMInterfaceActivated => Company is GlbCompany company && IntegratedCountryHelper.IsABMInterfaceActivatedByCompany(company.GC_RN_NKCountryCode, company.PK.ToGuid());

		public virtual bool EnableAttachCommercialInvoice
		{
			get { return true; }
		}

		public virtual bool EnableCopyCommercialInvoice
		{
			get { return true; }
		}

		public virtual bool EnableCommercialInvoiceMenuItem
		{
			get
			{
				return EnableAttachCommercialInvoice || EnableCopyCommercialInvoice;
			}
		}

		public virtual bool EnableImportInvoices
		{
			get { return true; }
		}

		public bool EnableClassificationAssistant => ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(Customs.Universal.Constants.FunctionalityTypes.HSAssistant, CountryCode, ZDateTime.Today, priorityToPilotFunctionality: true);

		#region Amendment Detection Suspender
		public IDisposable SuspendAmendmentDetection()
		{
			return new AmendmentDetectionSuspender(this);
		}

		public bool IsAmendmentDetectionSuspended
		{
			get { return suspendAmendmentDetectionIndex > 0; }
		}

		sealed class AmendmentDetectionSuspender : IDisposable
		{
			public AmendmentDetectionSuspender(BaseJobDeclaration bizObj)
			{
				this.bizObj = bizObj;
				bizObj.suspendAmendmentDetectionIndex++;
			}

			readonly BaseJobDeclaration bizObj;

			public void Dispose()
			{
				bizObj.suspendAmendmentDetectionIndex--;
			}
		}
		byte suspendAmendmentDetectionIndex;

		#endregion

		#region Lines To Print

		public LineToPrintCollection LinesToPrint
		{
			get
			{
				if (fLinesToPrint == null)
				{
					fLinesToPrint = GetNewLineToPrintCollection();
				}
				return fLinesToPrint;
			}
		}
		LineToPrintCollection fLinesToPrint;

		protected virtual void Lines_CountChanged(object sender, EventArgs e)
		{
			fLinesToPrint = null;
		}

		protected virtual LineToPrintCollection GetNewLineToPrintCollection()
		{
			return new LineToPrintCollection(this);
		}

		public event EventHandler<CancelEventArgs> OnGetLinesToPrint;

		public void FireOnGetLinesToPrint(CancelEventArgs eventArgs)
		{
			OnGetLinesToPrint?.Invoke(this, eventArgs);
		}

		public virtual bool SupportSelectingLinesToPrint
		{
			get { return false; }
		}

		#endregion

		#region TransportToPrint

		public Transport TransportToPrint
		{
			get { return fTransportToPrint; }
			set { fTransportToPrint = value; }
		}
		Transport fTransportToPrint;

		public event EventHandler<CancelEventArgs> OnGetTransportToPrint;

		[SuppressMessage("Microsoft.Design", "CA1030:UseEventsWhereAppropriate")]
		public void FireOnGetTransportToPrint(CancelEventArgs eventArgs)
		{
			OnGetTransportToPrint?.Invoke(this, eventArgs);
		}

		#endregion

		#region IRatingSupporterWithAdapter Members

		RatingAdaptersProvider IRatingSupporter.AdaptersProvider
		{
			get { return GetRatingAdaptersProviderCore(); }
		}

		protected virtual RatingAdaptersProvider GetRatingAdaptersProviderCore()
		{
			return new BaseJobDeclarationRatingAdaptersProvider(this);
		}

		public IAutoRating RatingAdapter
		{
			get { return ratingAdapter ?? (ratingAdapter = GetRatingAdapterCore()); }
		}
		IAutoRating ratingAdapter;

		protected virtual IAutoRating GetRatingAdapterCore()
		{
			return new BaseJobDeclarationRatingAdapter<BaseJobDeclaration>(this);
		}

		#endregion

		#region IRelatableActivity Members

		ZBool IRelatableActivity.ShouldIgnoreSuperAndSubActivityRelationships
		{
			get { return false; }
		}

		ZString IRelatableActivity.ActivityType
		{
			get { return RelatableActivityTypeList.Codes.CustomsDeclarations; }
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
			get { return string.Join("; ", new[] { JE_MessageType, JE_MessageSubType, JE_TransportMode, ZString.Format("{0} > {1}", JE_RL_NKOrigin, JE_RL_NKFinalDestination) }.Where(x => !x.IsEmpty)); }
		}

		void IRelatableActivity.OnRelatedActivitySaving(IRelatableActivity relatedActivity)
		{
		}

		ZBool IRelatableActivity.SupportViewRelatedCommunications => SupportViewRelatedCommunicationsCore;

		protected virtual ZBool SupportViewRelatedCommunicationsCore => ZBool.True;

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

		#region IProcessHandlingInfoProvider

		public ProcessHandlingInfo ProcessHandlingInfo
		{
			get { return new BaseJobDeclarationProcessHandlingInfo(this); }
		}

		#endregion

		public InventoryAutomationAction GetInventoryAutomationAction()
		{
			var automationAction = InventoryAutomationAction.Inward;
			IWarehouseIntegrationSupporter supporter = this;
			if (supporter.IsChangeOfRegimeWarehousingEnabled)
			{
				automationAction = InventoryAutomationAction.ChangeOfRegime;
			}
			else if (supporter.IsChangeOfOwnershipBondedWarehousingEnabled)
			{
				automationAction = InventoryAutomationAction.ChangeOfOwnership;
			}
			else if (IsExWarehouse)
			{
				automationAction = InventoryAutomationAction.Outward;
			}
			return automationAction;
		}

		#region IWarehouseIntegrationSupporter Members

		bool IWarehouseIntegrationSupporter.HasManualWhsUpdate
		{
			get
			{
				return Factory.GetValue(ref hasManualWhsUpdateCached,
					() =>
					{
						return SingleWarehouseEntry?.CH_HasManualWhsUpdate ?? false;
					});
			}
			set
			{
				if (SupportMultipleWarehouseEntry)
				{
					ErrorReporter.ReportOnce("HasManualWhsUpdate should not be set when SupportMultipleWarehouseEntry is true.");
				}
				else
				{
					var entry = SingleWarehouseEntry;
					if (entry != null)
					{
						entry.CH_HasManualWhsUpdate = value;
					}
				}
			}
		}
		CachedProperty<bool> hasManualWhsUpdateCached;

		bool IWarehouseIntegrationSupporter.IsChangeOfOwnershipBondedWarehousingEnabled => false;
		bool IWarehouseIntegrationSupporter.IsChangeOfRegimeWarehousingEnabled => false;
		bool IWarehouseIntegrationSupporter.SupportModificationState => false;
		Notes IWarehouseIntegrationSupporter.Notes => SingleWarehouseEntry?.GetNotes();
		void IWarehouseIntegrationSupporter.DoActionOnOutwardAccepted(BusinessObject job) { }
		void IWarehouseIntegrationSupporter.DoActionOnOutwardCanceled(BusinessObject job) { }
		void IWarehouseIntegrationSupporter.PostUpdateBondedWarehouseInwardAction() { }
		void IWarehouseIntegrationSupporter.PostUpdateBondedWarehouseOutwardAction() { }
		void IWarehouseIntegrationSupporter.PostCancelBondedWarehouseInwardAction() { }
		void IWarehouseIntegrationSupporter.PostCancelBondedWarehouseOutwardAction() { }
		bool IWarehouseIntegrationSupporter.IsActive => IsWHSUniversalXMLActive;
		ZGuid IWarehouseIntegrationSupporter.ClientPK => WarehouseClient?.PK ?? ZGuid.Empty;

		public OrgHeader WarehouseClient => GetWarehouseClientCore();
		protected virtual OrgHeader GetWarehouseClientCore() => IsExport ? Supplier : Importer;

		OrgAddress IWarehouseIntegrationSupporter.WarehouseAddress => WarehouseAddress;

		ZString IWarehouseIntegrationSupporter.WarehouseTransactionStatus
		{
			get => WarehouseTransactionStatus;
			set => WarehouseTransactionStatus = value;
		}

		ZString IWarehouseIntegrationSupporter.EntryNumber => SingleWarehouseEntry?.EntryNumber ?? ZString.Empty;

		public bool IsIntoTemporaryImportEnabled => Factory.GetValue(ref isIntoTemporaryImportEnabledCached, () => IsImport && SupportsTemporaryImports && ClientIsTemporaryImports);
		CachedProperty<bool> isIntoTemporaryImportEnabledCached;

		public bool IsOutOfTemporaryImportEnabled => Factory.GetValue(ref isOutOfTemporaryImportEnabledCached, () => IsExport && SupportsTemporaryImports && ClientIsTemporaryImports);
		CachedProperty<bool> isOutOfTemporaryImportEnabledCached;

		public bool IsIntoTemporaryExportEnabled => Factory.GetValue(ref isIntoTemporaryExportEnabledCached, () => IsImport && SupportsTemporaryExports && ClientIsTemporaryExports);
		CachedProperty<bool> isIntoTemporaryExportEnabledCached;

		public bool IsOutOfTemporaryExportEnabled => Factory.GetValue(ref isOutOfTemporaryExportEnabledCached, () => IsExport && SupportsTemporaryExports && ClientIsTemporaryExports);
		CachedProperty<bool> isOutOfTemporaryExportEnabledCached;

		public bool IsIntoInwardProcessingEnabled => Factory.GetValue(ref isIntoInwardProcessingEnabledCached, () => IsImport && SupportsInwardProcessing && ClientIsInwardProcessing);
		CachedProperty<bool> isIntoInwardProcessingEnabledCached;

		public bool IsOutOfInwardProcessingEnabled => Factory.GetValue(ref isOutOfInwardProcessingEnabledCached, () => (IsImport || IsExport) && SupportsInwardProcessing && ClientIsInwardProcessing);
		CachedProperty<bool> isOutOfInwardProcessingEnabledCached;

		public bool IsIntoOutwardProcessingEnabled => false;

		public bool IsOutOfOutwardProcessingEnabled => false;

		public bool ClientIsTemporaryImports
		{
			get
			{
				if (clientIsTemporaryImportsCached == null)
				{
					clientIsTemporaryImportsCached = new CachedProperty<bool>(Factory, () => WarehouseClient?.CompanyData?.OB_CusInventoryForTemporaryImports ?? false);
				}
				return clientIsTemporaryImportsCached.Value;
			}
		}
		CachedProperty<bool> clientIsTemporaryImportsCached;

		public bool ClientIsTemporaryExports
		{
			get
			{
				if (clientIsTemporaryExportsCached == null)
				{
					clientIsTemporaryExportsCached = new CachedProperty<bool>(Factory, () => WarehouseClient?.CompanyData?.OB_CusInventoryForTemporaryExports ?? false);
				}
				return clientIsTemporaryExportsCached.Value;
			}
		}
		CachedProperty<bool> clientIsTemporaryExportsCached;

		public bool ClientIsInwardProcessing => ClientInventoryManagementSetting.SupportInwardProcessing;
		public bool ClientIsOutwardProcessing => ClientInventoryManagementSetting.SupportOutwardProcessing;
		public bool ClientIsInventoryManagementOn => ClientIsInventoryManagementOnCore;
		protected virtual bool ClientIsInventoryManagementOnCore => ClientIsBondedWarehousing || ClientIsInwardProcessing || ClientIsOutwardProcessing;

		protected virtual bool SupportsTemporaryImports
		{
			get { return ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(Universal.Constants.FunctionalityTypes.TMPIMP, !CountryCode.IsEmpty ? CountryCode : GetDefaultDataGroupingCode(), ZDateTime.Today); }
		}

		protected virtual bool SupportsTemporaryExports
		{
			get { return ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(Universal.Constants.FunctionalityTypes.TMPEXP, !CountryCode.IsEmpty ? CountryCode : GetDefaultDataGroupingCode(), ZDateTime.Today); }
		}

		protected virtual bool SupportsInwardProcessing
		{
			get { return ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(Universal.Constants.FunctionalityTypes.IWDPROC, !CountryCode.IsEmpty ? CountryCode : GetDefaultDataGroupingCode(), ZDateTime.Today); }
		}

		#endregion

		#region IContainerParent Members

		IEnumerable<CommonContainer> IContainerParent.Containers
		{
			get { return CusContainers.Cast<BaseCusContainer>().Select(x => x.JobContainer); }
		}

		ZString IContainerParent.TransportMode
		{
			get { return JE_TransportMode; }
		}

		IEnumerable<JobDocsAndCartage> IContainerParent.DocsAndCartage(CommonContainer container)
		{
			yield return DocsAndCartage;
		}

		RefUNLOCO IContainerParent.LoadPort
		{
			get { return Origin; }
		}

		RefUNLOCO IContainerParent.DischargePort
		{
			get { return FinalDestination; }
		}

		OrgAddress IContainerParent.ArrivalCFSAddress
		{
			get { return IsImport && DepotDocAddress != null && DepotDocAddress.Address != null ? DepotDocAddress.Address : null; }
		}

		OrgAddress IContainerParent.DepartureCFSAddress
		{
			get { return IsExport && DepotDocAddress != null && DepotDocAddress.Address != null ? DepotDocAddress.Address : null; }
		}

		OrgAddress IContainerParent.ArrivalCTOAddress
		{
			get { return IsImport && ContainerTerminalOperatorDocAddress != null && ContainerTerminalOperatorDocAddress.Address != null ? ContainerTerminalOperatorDocAddress.Address : null; }
		}

		OrgAddress IContainerParent.DepartureCTOAddress
		{
			get { return IsExport && ContainerTerminalOperatorDocAddress != null && ContainerTerminalOperatorDocAddress.Address != null ? ContainerTerminalOperatorDocAddress.Address : null; }
		}

		OrgAddress IContainerParent.ArrivalUnpackCFSTransportAddress
		{
			get { return IsImport && DeliveryOrPickupCartageCoAddr != null ? DeliveryOrPickupCartageCoAddr : null; }
		}

		OrgAddress IContainerParent.DeparturePackCFSTransportAddress
		{
			get { return IsExport && DeliveryOrPickupCartageCoAddr != null ? DeliveryOrPickupCartageCoAddr : null; }
		}

		OrgAddress IContainerParent.ContainerYardEmptyPickupAddress
		{
			get { return IsExport && ContainerYardDocAddress != null && ContainerYardDocAddress.Address != null ? ContainerYardDocAddress.Address : null; }
		}

		OrgAddress IContainerParent.ContainerYardEmptyReturnAddress
		{
			get { return IsImport && ContainerYardDocAddress != null && ContainerYardDocAddress.Address != null ? ContainerYardDocAddress.Address : null; }
		}

		OrgAddress IContainerParent.SendingForwarderAddress
		{
			get { return IsExport && Forwarder != null ? Forwarder.MainAddress : null; }
		}

		OrgAddress IContainerParent.ReceivingForwarderAddress
		{
			get { return IsImport && Forwarder != null ? Forwarder.MainAddress : null; }
		}

		OrgAddress IContainerParent.ShippingLineAddress
		{
			get { return ShippingLine?.MainAddress; }
		}

		#endregion

		#region Accounting Total Outstanding/Invoiced/Billed Amounts

		public ZDecimal TotalOutstandingAmount
		{
			get { return Accounting_ARInvoiceQueryResult.TotalOutstandingAmount; }
		}

		public ZDecimal TotalInvoicedAmount
		{
			get { return Accounting_ARInvoiceQueryResult.TotalInvoicedAmount; }
		}

		public ZDecimal TotalBilledAmount
		{
			get { return Accounting_ARInvoiceQueryResult.TotalBilledAmount; }
		}

#if DEBUG
		public
#else
		protected
#endif
	void RefreshAccounting_ARInvoiceQueryResult()
		{
			accounting_ARInvoiceQueryResult = null;
			InvoiceQuery.ClearServiceCache(Factory);
		}

		AP_ARInvoiceQueryResult Accounting_ARInvoiceQueryResult
		{
			get
			{
				if (!accounting_ARInvoiceQueryResult.HasValue)
				{
					accounting_ARInvoiceQueryResult = InvoiceQuery.GetTotalInvoicedDetails(this, EntryChargeTypeCodesToMatch);
				}
				return accounting_ARInvoiceQueryResult.Value;
			}
		}
		AP_ARInvoiceQueryResult? accounting_ARInvoiceQueryResult;

		public IAccountingAP_ARInvoiceQuery InvoiceQuery
		{
			get
			{
				return ObjectFactory.Get<IAccountingAP_ARInvoiceQuery>();
			}
		}

		protected virtual List<ZGuid> EntryChargeTypeCodesToMatchCore
		{
			get { return new List<ZGuid>(); }
		}

		public List<ZGuid> EntryChargeTypeCodesToMatch
		{
			get { return EntryChargeTypeCodesToMatchCore; }
		}

		#endregion

		#region SupportAdditionalInvoices

		public virtual bool SupportAdditionalInvoices
		{
			get
			{
				return false;
			}
		}

		void DetachAdditionalInvoices()
		{
			if (fInvoices != null && fInvoices.SupportAdditionalInvoices)
			{
				fInvoices.RemoveAll();
			}
		}

		#endregion

		#region InvoicesOverrideDeclarationSupporter

		public sealed class InvoicesOverrideDeclarationSupporter : IDisposable
		{
			public InvoicesOverrideDeclarationSupporter(BaseJobDeclaration declaration)
			{
				this.declaration = declaration;
				SetOverrideDeclaration(declaration);
				this.declaration.invoicesOverrideDeclarationIndex++;
			}

			void SetOverrideDeclaration(BaseJobDeclaration declaration)
			{
				if (this.declaration.invoicesOverrideDeclarationIndex == 0)
				{
					this.declaration.Invoices.SetOverrideDeclaration(declaration);
					this.declaration.JobComInvoiceGroupHeaders.SetOverrideDeclaration(declaration);
					this.declaration.ActiveGroupHeader.SetOverrideDeclaration(declaration);
					this.declaration.InvoiceLines.SetOverrideDeclaration(declaration);
				}
			}

			BaseJobDeclaration declaration;

			public void Dispose()
			{
				declaration.invoicesOverrideDeclarationIndex--;
				SetOverrideDeclaration(null);
				declaration = null;
			}
		}

		int invoicesOverrideDeclarationIndex;

		#endregion

		#region OtherReferenceNumber
		protected virtual ZString OtherReferenceNumber
		{
			get { return ZString.Empty; }
		}
		#endregion

		#region OtherReferenceNumberCaption
		protected virtual ZString OtherReferenceNumberCaption
		{
			get { return ZString.Empty; }
		}
		#endregion

		#region ISendEmailSource Members

		AddressBookSelection ISendEmailSource.GetAddressBookSelection()
		{
			AddressBookSelection result = new AddressBookSelection();
			result.AddRecipient(Importer);
			result.AddRecipient(Supplier);
			result.AddRecipient(ShippingLine);
			result.AddRecipient(DocsAndCartage.DeliveryCartageCo);
			result.AddRecipient(DocsAndCartage.PickupCartageCo);
			result.AddRecipient(Forwarder);

			JobHeader job = new JobHeader.Loader(this).Load(false, false);
			result.AddRecipient(job?.LocalCharges);
			result.AddRecipient(job?.AgentCollect);
			return result;
		}

		string ISendEmailSource.EmailSubject
		{
			get { return Res.GetString("63a6c499-3b42-4807-b1a8-4b384d1158eb", "Declaration - {0}", this.JE_DeclarationReference); }
		}

		string ISendEmailSource.TemplateCategory
		{
			get { return MailTemplateCategoryList.Codes.CustomsDeclaration; }
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
			get { return ObjectFactory.GetType<Integration.DocumentWrappers.IDocBaseJobDeclaration>(); }
		}
		#endregion

		#region EntryInstruction

		public EntryInstructionProvider CustomsEntryInstructionProvider
		{
			get
			{
				if (customsEntryInstructionProvider == null)
				{
					var provider = GetCustomsEntryInstructionProviderCore();
					customsEntryInstructionProvider = provider ?? EntryInstructionProvider.NoEntryInstructionProvider;
				}
				return customsEntryInstructionProvider;
			}
		}
		EntryInstructionProvider customsEntryInstructionProvider;

		protected virtual EntryInstructionProvider GetCustomsEntryInstructionProviderCore()
		{
			return EntryInstructionProvider.NoEntryInstructionProvider;
		}

		public INotificationType ContainerNotLinkedSeverity
		{
			get { return ContainerNotLinkedSeverityCore; }
		}

		protected virtual INotificationType ContainerNotLinkedSeverityCore
		{
			get { return CargoWise.ComponentModel.NotificationType.Warning; }
		}

		[BusinessObjectTestExclude]
		[ChildEditable(true)]
		[ChildEditableTestExclude]
		public ICusEntryInstructionCollection<CusEntryInstruction> CustomsEntryInstructions => CustomsEntryInstructionProvider.CustomsEntryInstructions;

		#endregion

		#region IContainerTrackingProvider

		bool IContainerTrackingProvider.ContainerModeHasChanges
		{
			get
			{
				return JE_ContainerModeInfo.HasChanges;
			}
		}

		ZString IContainerTrackingProvider.ContainerMode
		{
			get
			{
				return JE_ContainerMode;
			}
		}

		ZString IContainerTrackingProvider.CarrierBookingReference
		{
			get
			{
				return ZString.Empty;
			}
		}

		bool IContainerTrackingProvider.CarrierBookingReferenceHasChanges
		{
			get
			{
				return false;
			}
		}

		bool IContainerTrackingProvider.CoLoadHasChanges
		{
			get
			{
				return false;
			}
		}

		ZString IContainerTrackingProvider.CoLoadWithCarrierBookingReference
		{
			get { return ZString.Empty; }
		}

		ZString IContainerTrackingProvider.CoLoadWithMasterBillNumber
		{
			get { return ZString.Empty; }
		}

		OrgHeader IContainerTrackingProvider.CoLoadWith
		{
			get { return null; }
		}

		public virtual ZString CarrierCode
		{
			get
			{
				var shippingLine = this.ShippingLine;
				return shippingLine != null ? shippingLine.SCACCode : ZString.Empty;
			}
		}

		public virtual bool CarrierCodeHasChanges
		{
			get
			{
				return this.JE_OH_ShippingLineInfo.HasChanges;
			}
		}

		public virtual ZString CarrierC1CCode
		{
			get
			{
				return this.ShippingLine?.C1CCode ?? ZString.Empty;
			}
		}

		ZString IContainerTrackingProvider.MasterBillNumber
		{
			get
			{
				return this.JE_MasterBill;
			}
		}

		bool IContainerTrackingProvider.MasterBillNumberHasChanges
		{
			get
			{
				return this.JE_MasterBillInfo.HasChanges;
			}
		}

		bool IContainerTrackingProvider.TransportModeHasChanges
		{
			get
			{
				return this.JE_TransportModeInfo.HasChanges;
			}
		}

		ZString IContainerTrackingProvider.TransportMode
		{
			get
			{
				return this.JE_TransportMode;
			}
		}

		IEnumerable<ITrackableContainer> IContainerTrackingProvider.Containers
		{
			get
			{
				return this.CusContainers.Cast<BaseCusContainer>().Where(c => c.JobContainer != null).Select(c => c.JobContainer).Cast<ITrackableContainer>();
			}
		}

		bool IContainerTrackingProvider.SubscribeToContainersOnlyHasChanges
		{
			get { return false; }
		}

		bool IContainerTrackingProvider.SubscribeToContainersOnly
		{
			get { return true; }
		}

		ZDateTime IContainerTrackingProvider.GetLastArrivalDate()
		{
			if (JE_DateAtFinalDestination.IsValid && !JE_DateAtFinalDestination.IsEmpty)
			{
				return JE_DateAtFinalDestination;
			}

			var arrivalTransport = new TransportOrderHelper(Transports).LastLeg;
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
			var firstDepartureDate = ZDateTime.Empty;

			var departureTransport = new TransportOrderHelper(Transports).FirstLeg;
			if (departureTransport != null)
			{
				firstDepartureDate = departureTransport.JW_ATD.IsValid && !departureTransport.JW_ATD.IsEmpty
					? departureTransport.JW_ATD
					: departureTransport.JW_ETD;
			}

			return firstDepartureDate.IsEmpty && JE_DateAtOrigin.IsValid && !JE_DateAtOrigin.IsEmpty
				? JE_DateAtOrigin
				: firstDepartureDate;
		}

		bool IContainerTrackingProvider.RoutingLegsHaveChanges
		{
			get
			{
				return isLegRemoved || Transports.Cast<Transport>().Any(leg => !leg.IsInDatabase || GetTrackingTransportPropertyInfos(leg).Any(c => c.HasChanges) || leg.JW_VesselHasChanges() || leg.JW_VoyageFlightHasChanges());
			}
		}

		IEnumerable<ZPropertyInfo> GetTrackingTransportPropertyInfos(Transport transport)
		{
			yield return transport.JW_TransportModeInfo;
			yield return transport.JW_RL_NKLoadPortInfo;
			yield return transport.JW_RL_NKDiscPortInfo;
		}

		#endregion

		public virtual void RefreshIncotermAndChargeFactory()
		{
			NeedToGetNewIncoTermAndChargeFactory = true;
			if (fJobComInvoiceGroupHeaders != null)
			{
				JobComInvoiceGroupHeaders.SetNeedToGetNewChargeIncoTermFactory();
			}
			if (fInvoices != null)
			{
				Invoices.SetNeedToGetNewChargeIncoTermFactory();
			}
			if (fInvoiceLines != null)
			{
				AddInvoiceChargesFetchHintsIfNeeded();
				foreach (BaseJobComInvoiceLine invoiceLine in InvoiceLines)
				{
					invoiceLine.Charges.MarkAsNeedingValidation();
				}
			}
		}

		#region IHandleEventsForOtherObjects Members

		BusinessObject[] IHandleEventsForOtherObjects.GetHandledObjects()
		{
			var jobDocs = DocsAndCartage;
			return jobDocs != null ? new[] { jobDocs } : Array.Empty<BusinessObject>();
		}

		#endregion

		#region IValidateForCustomsMessagingSupporter Members

		ZBool IValidateForCustomsMessagingSupporter.SupportValidateCustomsMessaging
		{
			get { return SupportValidateCustomsMessagingCore; }
		}

		protected virtual ZBool SupportValidateCustomsMessagingCore
		{
			get { return false; }
		}

		BusinessObject IValidateForCustomsMessagingSupporter.GetEntityToValidate(string triggerAction)
		{
			return GetEntityToValidateCore(triggerAction);
		}

		protected virtual BusinessObject GetEntityToValidateCore(string triggerAction) => this;

		public IDisposable MarkDeclarationIsValidatingCustomsMessaging()
		{
			return new ValidateCustomsMessagingNotifier(this);
		}

		public bool IsValidatingCustomsMessaging
		{
			get { return validateCustomsMessagingIndex > 0; }
		}

		int validateCustomsMessagingIndex;
		class ValidateCustomsMessagingNotifier : IDisposable
		{
			public ValidateCustomsMessagingNotifier(BaseJobDeclaration declaration)
			{
				this.declaration = declaration;
				declaration.validateCustomsMessagingIndex++;
			}

			readonly BaseJobDeclaration declaration;

			public void Dispose()
			{
				declaration.validateCustomsMessagingIndex--;
			}
		}

		#endregion

		#region IJobDeclarationMessageSupporter Members

		ZBool Integration.Customs.IJobDeclarationAutoSendingMessageSupporter.SupportEntryDeclarationMessage
		{
			get { return SupportEntryDeclarationMessageCore; }
		}

		protected virtual ZBool SupportEntryDeclarationMessageCore
		{
			get { return false; }
		}

		ZString Integration.Customs.IJobDeclarationAutoSendingMessageSupporter.GetReasonForNotSupportEntryDeclarationMessage
		{
			get { return GetReasonForNotSupportEntryDeclarationMessageCore(); }
		}

		protected virtual ZString GetReasonForNotSupportEntryDeclarationMessageCore()
		{
			return ZString.Format(SendEntryDeclarationTriggerNotSupportedMessage, CountryCode);
		}
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "text in service tasks")]
		internal const string SendEntryDeclarationTriggerNotSupportedMessage = "The Send Entry/Declaration Message trigger is not supported to be sent for {0}.";

		ZBool Integration.Customs.IJobDeclarationAutoSendingMessageSupporter.SupportReleaseMessage
		{
			get { return SupportReleaseMessageCore; }
		}

		protected virtual ZBool SupportReleaseMessageCore
		{
			get { return false; }
		}

		ZString Integration.Customs.IJobDeclarationAutoSendingMessageSupporter.GetReasonForNotSupportReleaseMessage
		{
			get { return GetReasonForNotSupportReleaseMessageCore(); }
		}

		protected virtual ZString GetReasonForNotSupportReleaseMessageCore()
		{
			return ZString.Format(SendReleaseMessageTroggerNotSupportedMessage, CountryCode);
		}
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "text in service tasks")]
		internal const string SendReleaseMessageTroggerNotSupportedMessage = "The Send Release Message trigger is not supported to be sent for {0}.";

		IProcessor Integration.Customs.IJobDeclarationAutoSendingMessageSupporter.CreateEntryDeclarationMessageProcessor()
		{
			return GetEntryDeclarationMessageProcessorCore();
		}

		protected virtual IProcessor GetEntryDeclarationMessageProcessorCore()
		{
			return null;
		}

		IProcessor Integration.Customs.IJobDeclarationAutoSendingMessageSupporter.CreateReleaseMessageProcessor(string eventCode)
		{
			return GetReleaseMessageProcessorCore();
		}

		protected virtual IProcessor GetReleaseMessageProcessorCore()
		{
			return null;
		}

		IProcessor Integration.Customs.IBaseAutoSendingMessageSupporter.CreateStmProcessQueueProcessor(BusinessObject parent, ZString triggerActionCode)
		{
			if (parent is ICusExitReport)
			{
				return new BatchProcessor.CustomsStmProcessQueueCreatorProcessor(parent, CustomsStmProcessQueueLoader.Constants.AutoSendCustomsMessaging, triggerActionCode);
			}
			return new BatchProcessor.CustomsStmProcessQueueCreatorProcessor(this, CustomsStmProcessQueueLoader.Constants.AutoSendCustomsMessaging, triggerActionCode);
		}

		#endregion

		#region Packs For Invoice Line

		public bool SupportsChcPivotBetweenInvoiceLineAndPacking
		{
			get { return SupportsChcPivotBetweenInvoiceLineAndPackingCore; }
		}

		protected virtual bool SupportsChcPivotBetweenInvoiceLineAndPackingCore
		{
			get { return false; }
		}

		public ZInt MaximumNumberOfPacksForEntryLine
		{
			get { return MaximumNumberOfPacksForEntryLineCore; }
		}

		protected virtual ZInt MaximumNumberOfPacksForEntryLineCore
		{
			get { return -1; } // This means that there is no maximum
		}

		#endregion

		#region Packs For Invoice Header

		public bool SupportsChzPivotBetweenInvoiceHeaderAndPacking
		{
			get { return SupportsChzPivotBetweenInvoiceHeaderAndPackingCore; }
		}

		protected virtual bool SupportsChzPivotBetweenInvoiceHeaderAndPackingCore
		{
			get { return false; }
		}

		#endregion

		public ZString PackageMarksAndNumbersAlwaysRequiredValidationMessage => PackageMarksAndNumbersAlwaysRequiredValidationMessageCore;
		protected virtual ZString PackageMarksAndNumbersAlwaysRequiredValidationMessageCore => ZString.Empty;

		public virtual ZBool AreMultipleEntryInstructionsAllowed => true;

		public virtual SupportingDocSendingObject GetSupportingDocSendingObject()
		{
			return new JobDeclarationSupportingDocSendingObject(this);
		}

		#region ICustomsFileParent

		ZString ICustomsFileParent.DeclarationType
		{
			get { return JE_MessageType; }
		}

		ZPropertyInfo ICustomsFileParent.DeclarationTypeInfo
		{
			get { return JE_MessageTypeInfo; }
		}

		ZGuid ICustomsFileParent.BranchPk
		{
			get { return JE_GB; }
		}

		ZBool ICustomsFileParent.IsLocked
		{
			get { return Logs.MostRecentLogByEventTime(AutoEvents.LockForEdit) != null; }
		}

		void ICustomsFileParent.LockFile(ZString reference)
		{
			this.AddLockEvent(reference);
		}

		void ICustomsFileParent.UnlockFile(ZString reference)
		{
			this.AddUnlockEvent(reference);
			var entryHeaders = ActiveEntryHeaders.Cast<ICustomsFileParent>().Where(c => c.IsLocked);

			foreach (var header in entryHeaders)
			{
				header.UnlockFile(reference);
			}
		}

		#endregion

		public static string JE_ApplicationCodeFilterCaption
		{
			get { return Res.GetString("c3a298f6-70ad-4b30-a135-0a882f678b3c", "Submit Type"); }
		}

		IJobHeader IHaveJobHeader.JobHeader => Job;

		bool IServicesParent.NeedsServiceEvents => true;

		bool IServicesParent.NeedsReferenceNumber => true;

		#region SetterSuspender

		public SetterSuspender SetterSuspender => setterSuspender ?? (setterSuspender = new SetterSuspender());
		SetterSuspender setterSuspender;

		#endregion

		#region GetStrategies

		IBusinessObjectStrategy[] strategies;
		protected override IBusinessObjectStrategy[] GetStrategies()
		{
			if (strategies == null)
			{
				var baseStrategies = base.GetStrategies();
				var strategyList = new List<IBusinessObjectStrategy>(baseStrategies);
				strategyList.Add(new CommissionSourceMonitorStrategy(new[] { JE_TransportModeInfo, JE_RL_NKOriginInfo, JE_RL_NKFinalDestinationInfo }, this));

				strategies = strategyList.ToArray();
			}

			return strategies;
		}

		#endregion

		#region Universal Copy

		protected internal virtual bool SkipDuplicateTopGroupInvoiceOnUniversalCopy { get; }

		protected virtual void AfterUniversalCopy()
		{
			AllGroupHeaders.Reload(true);
		}

		#endregion

		#region Related Transport Bookings

		public IEnumerable<IDtbBookingConsolidation> RelatedTransportBookings
		{
			get
			{
				var bookings = TransportBookingLoader.GetBookingConsolidations(this).ToList();
				if (Shipment != null)
				{
					bookings.AddRange(TransportBookingLoader.GetBookingConsolidations(Shipment).ToList());
				}
				return bookings;
			}
		}

		[ResourceStringData("7E7385E8-2540-45F8-A7E4-6ED2897CD3C2", Caption = "Related Transport Bookings")]
		public ZString RelatedTransportBookingsJobNumbers
		{
			get
			{
				var bookings = IsImport ? RelatedTransportBookings?.Where(x => x.KB_JobDirection == nameof(DtbBookingDirection.DLV)) : RelatedTransportBookings?.Where(x => x.KB_JobDirection == nameof(DtbBookingDirection.PIC));
				return bookings == null ? string.Empty : string.Join(",", bookings.SelectMany(x => x.Bookings).Select(x => x.KM_JobID));
			}
		}

		#endregion

		#region Copy Last InvoiceLine Details To NewLines

		public ZBool JE_CopyLastInvoiceLineDetailsToNewLines => FilteredInvoiceLinesIsLoaded ? FilteredInvoiceLines.CopyLastLineDetailsToNewLines : AlwaysCopyFromPreviousLine;

		bool AlwaysCopyFromPreviousLine => CustomsDataRegistry.Instance.AlwaysCopyFromPreviousLine.GetFallBackValueAtAllLevels(RegistryCompanyPK, Guid.Empty, Guid.Empty);

		#endregion

		#region IClusterKeyMaster

		[LightValidationTestExempt]
		public sealed override ZInt JE_ClusterKey
		{
			get => base.JE_ClusterKey;
			set
			{
				if (base.JE_ClusterKey != value)
				{
					this.CheckCanSetMasterClusterKey();
					base.JE_ClusterKey = value;
					if (!IsCopying)
					{
						Invoices?.MarkAsNeedingValidation();
						CustomsEntryInstructions?.MarkAsNeedingValidation();
						DocsAndCartage?.MarkAsNeedingValidation();
						CusContainers?.MarkAsNeedingValidation();
					}
				}
			}
		}

		ZPropertyInfoInt IClusterKeyEntity.ClusterKeyPty => (ZPropertyInfoInt)JE_ClusterKeyInfo;

		#endregion

		#region IAddInfoChildSupporter Members

		BusinessObject IAddInfoChildSupporter.AddInfoChild => GetAddInfoChild();
		protected virtual BusinessObject GetAddInfoChild() => null;

		SchemaGuidColumn IAddInfoChildSupporter.ChildForeignKeyColumn => GetChildForeignKeyColumn();
		protected virtual SchemaGuidColumn GetChildForeignKeyColumn() => null;

		void IAddInfoChildSupporter.RegisterListChangedCalledRefreshBinding(IBindingList element) => RegisterListChangedCalledRefreshBinding(element);
		void IAddInfoChildSupporter.UnRegisterListChangedCalledRefreshBinding(IBindingList element) => UnRegisterListChangedCalledRefreshBinding(element);
		#endregion

		public CusPackingList LoadOrCreateCusPackingList(BusinessObjectFactory factory)
		{
			var packinglist = LoadCusPackingList(factory) ?? CreateCusPackingList(factory);
			return packinglist;
		}

		public CusPackingList LoadCusPackingList(BusinessObjectFactory factory)
		{
			var query = new ZQuery(CusPackingListSchema.CUL_JE, PK);
			query.OrderBy = CusPackingListSchema.Constants.CUL_SystemCreateTimeUtc;
			return (CusPackingList)factory.LoadTop1(PackingListType, query);
		}

		public CusPackingList CreateCusPackingList(BusinessObjectFactory factory)
		{
			return GetNewCusPackingListCore(factory);
		}

		protected virtual CusPackingList GetNewCusPackingListCore(BusinessObjectFactory factory)
		{
			var packingList = (CusPackingList)factory.New(PackingListType);
			packingList.CUL_JE = PK;
			packingList.CUL_Description = JE_GoodsDescription;
			packingList.CUL_PackingListDate = ZDate.Today;
			return packingList;
		}

		protected virtual Type PackingListType => typeof(CusPackingList);

		public bool SupportsCusPackingList => SupportsCusPackingListCore;

		protected virtual bool SupportsCusPackingListCore => false;

		public ZBool HasCusPackingList(BusinessObjectFactory factory) => IsInDatabase && LoadCusPackingList(factory) != null;

		public IBillDetails CreateConsolBillDetails(ForwardingConsol relevantConsol) => relevantConsol == null ? null : CreateConsolBillDetailsCore(relevantConsol);
		protected virtual IBillDetails CreateConsolBillDetailsCore(ForwardingConsol relevantConsol) => relevantConsol;

		void MarkDocsAndCartageAsNeedingValidationIfNotSettingDefaultValues()
		{
			if (!IsSettingDefaultValues)
			{
				DocsAndCartage.MarkAsNeedingValidation();
			}
		}

		#region Credit Restriction Message Caption

		public virtual ZString CreditRestrictionMessageCaption => Res.GetString("f781cf58-c933-4e5e-a478-71df5ce7984f", "Submit message with credit restriction");

		#endregion

		#region SuspendInvoiceLineDataInitialization

		public IDisposable SuspendInvoiceLineDataInitialization()
		{
			return new DisposableAction(() => initialiseInvoiceLineDataIndex++, () => initialiseInvoiceLineDataIndex--);
		}

		bool IsInvoiceLineDataInitializationSuspended
		{
			get { return initialiseInvoiceLineDataIndex > 0; }
		}
		byte initialiseInvoiceLineDataIndex;

		public bool ShouldInitialiseInvoiceLineData => !IsInvoiceLineDataInitializationSuspended && GlbCompany.CurrentCompany.PK == JE_GC;

		#endregion

		#region IComplianceJobDirection

		ZBool IComplianceJobDirectionProvider.IsInternational => (this.IsCrossTrade() || this.IsExport() || this.IsImport());

		#endregion

		#region ICusGoodsLocationTypeSupporter

		Type ICusGoodsLocationTypeSupporter.GoodsLocationType => GoodsLocationTypeCore;

		protected virtual Type GoodsLocationTypeCore => typeof(CusGoodsLocation);

		#endregion

		#region Customs Rule

		public CustomsRule CustomsRule => IsImport ? CustomsRule.Loader.Load(Factory, GetDateForCustomsRuleFilter(), Importer, CountryCode) : null;

		public virtual ZDate GetDateForCustomsRuleFilter()
		{
			return JE_DateOfFirstArrival.Date;
		}

		public virtual ZDecimal CustomsValueForCustomsRuleValidation => TotalCustomsValueInLocalCurrency;

		public virtual ZBool IsBrokerToPay => false;
		public virtual ZBool IsImporterToPay => false;
		public virtual ZDecimal DisbursementAmount => 0m;
		public virtual ZDecimal TotalDutyAmount => 0m;

		#endregion

		#region ProductRefresh

		public ZBool ForcePartRefreshFromUI
		{
			get
			{
				return forcePartRefreshFromUI;
			}
			set
			{
				forcePartRefreshFromUI = value;
			}
		}

		ZBool forcePartRefreshFromUI = false;

		public void RefreshInvoiceLineProducts()
		{
			try
			{
				ForcePartRefreshFromUI = true;
				InvoiceLines.Cast<BaseJobComInvoiceLine>().ForEach(x => x.PartSyncManager?.Refresh());
			}
			finally
			{
				ForcePartRefreshFromUI = false;
			}
		}

		#endregion

		#region IAdditionalDebuggingDetails Members
		IEnumerable<string> IAdditionalDebuggingDetails.AdditionalDetails
		{
			get
			{
				yield return $"{GetType().FullName} - In DB:{IsInDatabase} - {CountryCode} - {JE_DeclarationReference} - {PK}";
			}
		}
		#endregion

		#region ITypeDeciderContext Members

		string ITypeDeciderContext.Country => Branch?.Company?.GC_RN_NKCountryCode ?? GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

		#endregion

		#region ISourceIdentifierProvider

		ZGuid ISourceIdentifierProvider.SourceIdentifier => Shipment?.PK ?? PK;

		#endregion

		public ZString BrokerageCountryCode => Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(CountryCode);

		public virtual bool UseTariffDescriptionForEntryLine => true;

		OrgHeader Declarant => DeclarantAddress?.Header;

		public ZString DeclarantCode => Declarant?.OH_Code ?? ZString.Empty;

		public ZString DeclarantName => Declarant?.OH_FullName ?? ZString.Empty;

		public bool IsOverrideRecipientEmailEnabled(Type bizObjType, ZGuid[] targets) => IsOverrideRecipientEmailEnabledCore(bizObjType, targets);

		protected virtual bool IsOverrideRecipientEmailEnabledCore(Type bizObjType, ZGuid[] targets)
		{
			var query = new ZQuery(BusinessObjectFactory.GetTableSchemaFromType(bizObjType).PK, targets);
			var bizObjects = Factory.Load(bizObjType, query);
			var declarants = bizObjects.OfType<BaseJobDeclaration>().Select(d => d.DeclarantAddress).ToArray();
			return declarants.Length == bizObjects.Length && declarants.AllSame();
		}

		public event EventHandler MultipleKeyToUseChanged;
		protected void OnMultipleKeyToUseChanged()
		{
			var currrentMultipleKeysToUse = MultipleKeysToUse;
			if (previousMultipleKeysToUse != currrentMultipleKeysToUse)
			{
				previousMultipleKeysToUse = currrentMultipleKeysToUse;
				MultipleKeyToUseChanged?.Invoke(this, EventArgs.Empty);
			}
		}

		#region IPropertyChecker

		public bool IsPropertyUpdatableViaXueAdditionalFields(PropertyInfo propertyInfo, object proposedValue, out string errorMessage)
		{
			errorMessage = null;

			if (propertyInfo.Name != AutoJobDeclaration.Schema.JE_ScreeningStatus)
			{
				return true;
			}

			var newStatus = proposedValue.ToString();
			if ((newStatus.Equals(ScreeningStatusesList.Codes.JobClearedExternal) || newStatus.Equals(ScreeningStatusesList.Codes.JobBlockedExternal))
				&& (!JE_JS.IsDefault || !OrganisationsDataRegistry.Instance.EnableExternalOverrideJobScreeningStatuses.Value))
			{
				errorMessage = Res.GetString("f6eaa915-75a3-4437-b770-c3de88f259a3", "The provided screening status value is invalid. Ensure it matches one of the accepted statuses.");
				return false;
			}

			return true;
		}

		#endregion

		#region IDataModelSupporter

		public void PopulateDataModelIfNeeded() => PopulateDataModelIfNeededCore();
		protected virtual void PopulateDataModelIfNeededCore() => this.PopulateDataModelFromCountryCodeIfNeeded(CountryCode);

		ZString IDataModelSupporter.DataModel { get => JE_DataModel; set => JE_DataModel = value; }

		#endregion

		IReadOnlyList<string> previousMultipleKeysToUse;

		public IReadOnlyList<string> MultipleKeysToUse => MultipleKeysToUseCore;
		protected virtual IReadOnlyList<string> MultipleKeysToUseCore => Array.Empty<string>();

		[ResourceStringData("2ECF6C41-E13D-492D-940D-8699ADA76878", Caption = "Phase Status")]
		public virtual ZString PhaseStatus
		{
			get
			{
				var entryPhaseStatuses = ActiveEntryHeaders.Cast<CusEntryHeader>().Select(entry => entry.CH_PhaseStatus).Distinct().Take(2).ToArray();
				switch (entryPhaseStatuses.Length)
				{
					case 2:
						return Common.Shared.CommonEntryStatusList.Codes.MultipleEntryStatus;
					case 1:
						return entryPhaseStatuses[0];
					default:
						return ZString.Empty;
				}
			}
		}

		public ZPropertyInfo PhaseStatusInfo => GetZPropertyInfo(Schema.PhaseStatus);

		[ResourceStringData("91DEE053-77C6-4AEF-B2B9-CEFCDF81ED2C", Caption = "Phase Status Description")]
		public virtual ZString PhaseStatusDescription
		{
			get
			{
				var phaseStatus = PhaseStatus;
				return phaseStatus == Common.Shared.CommonEntryStatusList.Codes.MultipleEntryStatus
					? Common.Shared.CommonEntryStatusList.Descriptions.MultipleEntryStatus
					: Lookups.EntryPhaseStatusList.GetDescriptionFromCode(phaseStatus);
			}
		}

		public ZPropertyInfo PhaseStatusDescriptionInfo => GetZPropertyInfo(Schema.PhaseStatusDescription);

		#region IAuditParent Members

		IEnumerable<AuditChildInfo> IAuditParent.RelatedAuditChildren
		{
			get
			{
				yield return new(CusContainerSchema.CO_ClusterKey, CusContainerSchema.CO_ContainerNumber);
				yield return new(CusDecHouseBillSchema.CU_ClusterKey, CusDecHouseBillSchema.CU_BillNum);
				yield return new(CusDecHouseContainerPackSchema.CW_ClusterKey, null);
				yield return new(CusDecHouseContainerPivotSchema.CR_ClusterKey, null);
				yield return new(CusEntryHeaderSchema.CH_ClusterKey, CusEntryHeaderSchema.CH_BGMReference);
				yield return new(CusEntryHeaderChargesSchema.C1_ClusterKey, null);
				yield return new(CusEntryInstructionSchema.CEI_ClusterKey, null);
				yield return new(CusEntryLineSchema.CL_ClusterKey, null);
				yield return new(CusEntryLineFeeSchema.CF_ClusterKey, null);
				yield return new(CusEntryPayInfoSchema.C9_ClusterKey, null);
				yield return new(CusHouseContPackInvoiceHeaderPivotSchema.CHZ_ClusterKey, null);
				yield return new(CusHouseContPackInvoiceLinePivotSchema.CHC_ClusterKey, null);
				yield return new(CusUnderbondDecSchema.BU_ClusterKey, null);
				yield return new(JobComInvLineRefsSchema.JG_ClusterKey, null);
				yield return new(JobComInvoiceHeaderSchema.JZ_ClusterKey, JobComInvoiceHeaderSchema.JZ_InvoiceNumber);
				yield return new(JobComInvoiceHeaderRefsSchema.J2_ClusterKey, null);
				yield return new(JobComInvoiceLineSchema.JI_ClusterKey, JobComInvoiceLineSchema.JI_MatchingKey);
			}
		}

		#endregion
	}
}
