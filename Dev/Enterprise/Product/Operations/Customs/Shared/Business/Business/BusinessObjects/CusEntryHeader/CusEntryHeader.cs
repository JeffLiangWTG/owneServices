using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Integration;
using Enterprise.BufferManagement.Integration;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.BusinessObjects.CusPermit;
using Enterprise.Customs.Business.BusinessObjects.Interfaces;
using Enterprise.Customs.Business.CommonGoodsItemsIntegration;
using Enterprise.Customs.Business.Extensions;
using Enterprise.Customs.Business.InterfaceImplementations;
using Enterprise.Customs.Business.WarehouseExtensions;
using Enterprise.Customs.Common;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.Universal;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Integration.BondedWarehouse;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.ClusterKey;
using Enterprise.ZArchitecture.Business.EventManagement;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

[assembly: ClusterKeyMetaData(typeof(CusEntryHeader), ParentTableName = JobDeclarationSchema.Constants.TableName, ParentFkColumnName = nameof(CusEntryHeader.CH_JE))]

namespace Enterprise.Customs.Business
{
	[TestExcludeWorkflowProviderHasTestCase]
	[SingleObjectAroundARow]
	[DependentBusinessObject(typeof(BaseJobDeclaration), "CustomsEntryHeaders")]
	[CodeProperty(CusEntryHeader.Schema.CH_BGMReference)]
	[UniversalDataContext(DataContextType.WarehouseCustomsEntry)]
	public class CusEntryHeader : AutoCusEntryHeader,
		Integration.Customs.ICusEntryHeader,
		IServiceLocator,
		IClusterKeyWorker,
		IDocumentSupportable,
		IDeclarationProvider,
		ICurrencyConverterProvider,
		IDocManagerSupport,
		IDocManagerSupportProvider,
		IAccInvoiceDataProvider,
		IRegistryAccessingSupporter,
		ICustomsChargeEntry,
		IWorkflowTriggerEventSource,
		IAdditionalReferenceNumberTypeProvider,
		ICDArchive,
		IJobNumber,
		IStmNoteParentWithSystemNote,
		IDeclarationWarehouseIntegrationSupporter,
		IDocsAndCartageParent,
		ICustomsFileParent,
		IWorkflowTriggerFieldChangeSource,
		Integration.Customs.ICustomsDocumentGeneratorSupporter,
		IAllowPermitProcessing,
		IBranchProvider,
		IAddInfoChildSupporter,
		IEDIMessageCollectionOwner,
		ICommonGoodsItemsIntegratorProvider,
		IRelatedJob,
		ITypeDeciderContext,
		IProcessHandlingInfoProvider,
		IDataModelSupporter
	{
		#region Schema
		public new class Schema : AutoCusEntryHeader.Schema
		{
			public const string Duty = "Duty";
			public const string VAT = "VAT";
			public const string TotalAmountPayable = "TotalAmountPayable";
			public const string EntryNumber = "EntryNumber";
			public const string GSTAmount = "GSTAmount";
			public const string CustomsValue = "CustomsValue";
			public const string PackagesCount = "PackagesCount";
			public const string CH_CustomsMessageRemarks = "CH_CustomsMessageRemarks";
			public const string CH_CustomsDeliveryInstructions = "CH_CustomsDeliveryInstructions";
			public const string CH_MessageTypeDescription = "CH_MessageTypeDescription";
			public const string CH_WarehouseTransactionStatusDescription = "CH_WarehouseTransactionStatusDescription";
			public const string ClearanceDate = "ClearanceDate";
			public const string DeclarationDate = "DeclarationDate";
			public const string DeclarationReference = "DeclarationReference";
			public const string EffectiveValuationDate = "EffectiveValuationDate";
			public const string EntryHeaderStatusDescription = "EntryHeaderStatusDescription";
			public const string MessageStatusDescription = "MessageStatusDescription";
			public const string MovementReferenceNumber = "MovementReferenceNumber";
			public const string TransactionValue = "TransactionValue";
			public const string TotalDutyAmount = "TotalDutyAmount";
			public const string MovementReferenceNumberIssueDate = "MovementReferenceNumberIssueDate";
			public const string DeclarationUCR = "DeclarationUCR";
			public const string ValueForVAT = nameof(ValueForVAT);
			public const string MovementReferenceNumberEntryStatus = nameof(CusEntryHeader.MovementReferenceNumberEntryStatus);
			public const string MovementReferenceNumberExpiryDate = nameof(CusEntryHeader.MovementReferenceNumberExpiryDate);

			public const int CE_EntryNumMaxLength = 35;
		}

		#endregion

		public static readonly CusEntryHeaderTypeDecider TypeDecider = new CusEntryHeaderTypeDecider();

		public CusEntryHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(CH_AddInfo), ConcurrencyPolicy.Strict);
			cH_JECachedOnRemovedFromDependentCollection = CH_JE;
		}

		public new class Loader : BusinessObject.Loader
		{
			public Loader(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			protected override Type GetTypeOfBusinessObjectToLoad()
			{
				return typeof(CusEntryHeader);
			}

			/// <summary>
			/// Finds an entry with match entry number in the current company
			/// </summary>
			/// <param name="entryNumber"></param>
			/// <returns></returns>
			public CusEntryHeader FindByEntryNumberAndCurrentCompany(string entryNumber)
			{
				return FindByEntryNumberAndCurrentCompanyCore(entryNumber);
			}

			protected virtual CusEntryHeader FindByEntryNumberAndCurrentCompanyCore(string entryNumber)
			{
				return FindByEntryNumberAndCurrentCompany(entryNumber, (CusEntryHeader entryHeader) => true);
			}

			public CusEntryHeader FindByEntryNumberAndCurrentCompany(string entryNumber, Predicate<CusEntryHeader> entryHeaderFilter)
			{
				return FindByEntryNumberAndCurrentCompanyCore(entryNumber, entryHeaderFilter);
			}

			protected virtual CusEntryHeader FindByEntryNumberAndCurrentCompanyCore(string entryNumber, Predicate<CusEntryHeader> entryHeaderFilter)
			{
				if (entryHeaderFilter == null)
				{
					throw new ArgumentNullException(nameof(entryHeaderFilter));
				}

				CusEntryHeader result = null;
				ZQuery entryNumberFilter = new ZQuery(CusEntryNumSchema.CE_ParentTable, CusEntryHeaderSchema.Constants.TableName);
				entryNumberFilter.AddToFilter(CusEntryNumSchema.CE_EntryNum, entryNumber);
				foreach (CusEntryNumber bizO in Factory.Load<CusEntryNumber>(entryNumberFilter))
				{
					CusEntryHeader entryHeader = Factory.Load<CusEntryHeader>(bizO.CE_ParentID);
					if (entryHeader != null
						&& entryHeader.Declaration != null
						&& entryHeader.Declaration.Branch.GB_GC == GlbCompany.CurrentCompany.PK
						&& entryHeaderFilter(entryHeader))
					{
						result = entryHeader;
						break;
					}
				}
				return result;
			}
		}

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new FetchStrategies.CusEntryHeaderFetchStrategy(this);
		}

		#region EntryHeaderTypes
		public static class EntryHeaderTypes
		{
			public static class NZ
			{
				public const string FormalEntry = "NZF";
				public const string Completion = "NZC";
				public const string Original = "NZR";
				public const string ECIWriteOff = "NZE";
				public const string ECIWriteOffManifest = "NZM";
				public const string PrimaryIndustries = "NZI";
			}
		}
		#endregion

		#region ResetTotals
		public void ResetTotalsAndCachedValues()
		{
			CH_TotalPaid = 0;

			foreach (CusEntryHeaderCharges charge in Charges)
			{
				if (charge.ShouldResetDataOnMerging)
				{
					charge.C1_ChargeAmount = 0m;
				}

				charge.C1_IsLandedCostOnly = false;
			}
			fGroupInvoices = null;
			ResetIsCustomsValueCalculated();
			ResetIsValueForVATCalculated();
			ResetCachedValues();
			invoiceHeaders = null;
			fRandomHeader = null;
			invoiceLines = null;
		}

		protected virtual void ResetCachedValues()
		{
		}

		#endregion

		#region EntryChargeTypeList
		public EntryChargeTypeList EntryChargeTypeList => GetEntryChargeTypeList();

		protected virtual EntryChargeTypeList GetEntryChargeTypeList()
		{
			return EntryChargeTypeList.GetCachedList(Factory, CountryCode);
		}

		#endregion

		#region EffectiveValuationDate
		public virtual ZDateTime EffectiveValuationDate
		{
			get { return RandomHeader.EffectiveValuationDate; }
		}

		public ZPropertyInfo EffectiveValuationDateInfo
		{
			get { return GetZPropertyInfo(Schema.EffectiveValuationDate); }
		}
		#endregion

		#region Status Description
		[ResourceStringData("Enterprise.Customs.usiness.CusEntryHeader|EntryHeaderStatusDescription", Caption = "Entry Status Description", ShortCaption = "Entry Status Desc.")]
		public virtual ZString EntryHeaderStatusDescription
		{
			get
			{
				string result;
				var entryStatusList = Lookups.CH_EntryStatusList;
				if (entryStatusList.ContainsCode(CH_EntryStatus))
				{
					result = entryStatusList.GetDescriptionFromCode(CH_EntryStatus);
				}
				else if (CH_EntryStatus.IsEmpty)
				{
					result = DefaultStatusDescription;
				}
				else
				{
					result = Res.GetString("f597e3f7-b632-4962-b268-9fe5950f083f", "Unknown");
				}
				return result;
			}
		}

		public ZPropertyInfo EntryHeaderStatusDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.EntryHeaderStatusDescription); }
		}

		public virtual ZString DefaultStatusDescription
		{
			get
			{
				return Res.GetString("a2e8e840-95c7-4e40-8d1b-a93f589f3f09", "Not Sent");
			}
		}

		public virtual ZString MessageStatusDescription
		{
			get
			{
				ZString result;
				var messageStatusList = Lookups.MessageStatusList;
				if (messageStatusList.ContainsCode(CH_Status))
				{
					result = messageStatusList.GetDescriptionFromCode(CH_Status);
				}
				else if (CH_Status.IsEmpty)
				{
					result = Res.GetString("5d5aa17c-d2a7-4e17-b359-60681eba874d", "Not Sent");
				}
				else
				{
					result = Res.GetString("f9165c3c-5385-47c4-94c8-98546e3ae162", "Unknown");
				}

				return result;
			}
		}

		public ZPropertyInfo MessageStatusDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.MessageStatusDescription); }
		}

		#endregion

		#region Business Object Overrides

		protected override void OnFactorySaved(bool saveSucceeded)
		{
			base.OnFactorySaved(saveSucceeded);

			if (!saveSucceeded)
			{
				DeleteAnyNewMessages();
			}
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			if (!IsValidationSuspended)
			{
				CusEntryHeaderValidation.ValidatePackagesCount();
			}
		}

		protected override CusEntryHeaderValidation GetNewValidation()
		{
			return new CusEntryHeaderValidation(this);
		}

		public override void Delete()
		{
			if (!IsDeleted)
			{
				FetchForLoadChildEditableObjectsIfNeeded();
				this.DeleteAllCusAddInfoCodeDataAndSupportingInfoChildrenIfSupported();
			}
			DeleteAllCusEntryNumbers();
			EntryPayInfos.RemoveAndDeleteAll();
			Charges.RemoveAndDeleteAll();
			AllEntryLines.RemoveAndDeleteAll();
			PivotsToContainers.DeleteAll();
			Snapshots.DeleteAll();

			if (IsInDatabase && Declaration is BaseJobDeclaration declaration && !declaration.DeletedEntryHeaderPKsInDatabase.Contains(this.PK))
			{
				declaration.DeletedEntryHeaderPKsInDatabase.Add(this.PK);
			}

			base.Delete();
		}

		public void DeleteAllCusEntryNumbers()
		{
			ZQuery filter = new ZQuery(CusEntryNumSchema.CE_ParentID, PK);
			Factory.Load<CusEntryNumber>(filter).DeleteAll();
		}

		public override void OnSaving()
		{
			base.OnSaving();
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(CH_Status), ConcurrencyPolicy.Strict);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(CH_EntryStatus), ConcurrencyPolicy.Strict);

			if (IsChangingToClearStatusForAccIntegration)
			{
				statusChangedToClearForAccIntegration = true;
			}

			PopulateDataModelIfNeeded();
		}

		bool statusChangedToClearForAccIntegration;

		/// <summary>
		/// Override when cleared date is not calculated from the last incoming message time
		/// </summary>
		/// <returns>The date the you wish to use for the 'cleared' event - calculated however you wish.</returns>
		protected virtual ZDateTime GetCustomsClearedDateWithoutUsingLoggedEvent()
		{
			ZDateTime result = ZDateTime.Empty;
			if (Messages.LastIncomingMessage != null)
			{
				result = Messages.LastIncomingMessage.EM_DateTimeInterchangeSent;
			}
			if (result.IsEmpty)
			{
				result = ZDateTime.Now;
			}
			return result;
		}

		void AddClearedCustomsLogIfRequired()
		{
			if (ShouldLogCustomsClearedEvent())
			{
				if (ShouldLogCustomsClearedToDeclarationOrShipment)
				{
					Declaration.LogCustomsClearedIfNeeded();
				}
				if (ShouldLogCustomsClearedToEntryHeader)
				{
					Logs.AddNew(Declaration.CustomsClearedEventType, ClearanceEventReference, GetCustomsClearedDateWithoutUsingLoggedEvent().ToOffset());
				}
			}
		}

		protected virtual bool ShouldLogCustomsClearedToEntryHeader => Logs.MostRecentLogByEventTimeExcludingEstimated(Declaration.CustomsClearedEventType) == null;

		protected virtual bool ShouldLogCustomsClearedEvent()
		{
			return IsCustomsClearedEventSupported && IsStatusChangedToClearedForLoggingCLREvent;
		}

		public virtual ZString ClearanceEventReference
		{
			get { return ZString.Empty; }
		}

		protected virtual bool IsCustomsClearedEventSupported
		{
			get { return true; }
		}

		public virtual bool ShouldLogCustomsClearedToDeclarationOrShipment
		{
			get { return true; }
		}

		void AddHeldCustomsLogIfRequired()
		{
			if (IsCustomsImpedimentReceivedEventSupported && StatusChangedToHeldSinceLoading)
			{
				var timeImpedimentReceived = Messages.LastIncomingMessage != null ? Messages.LastIncomingMessage.EM_DateTimeInterchangeSent.ToOffset() : ZDateTimeOffset.Now;
				using (DisposableEnvironment.ForBranch(Declaration.Branch.PK.ToGuid()))
				{
					Logs.AddNew(Events.CustomsImpedimentReceived, timeImpedimentReceived.IsEmpty ? ZDateTimeOffset.Now : timeImpedimentReceived);
				}
			}
		}

		protected virtual bool IsCustomsImpedimentReceivedEventSupported
		{
			get { return true; }
		}

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();

			LogStatusIfRequired(ShouldLogEntryStatus, CH_EntryStatusInfo, CH_EntryStatus, Events.CustomsEntryStatus);
			AddClearedCustomsLogIfRequired();
			PopulateReleaseDateIfNeeded();
			AddHeldCustomsLogIfRequired();
			IntegrateWithWarehouseIfRequired();
		}

		protected void LogStatusIfRequired(bool shouldLogStatus, ZPropertyInfo statusInfo, ZString status, Event statusEvent)
		{
			if (shouldLogStatus && !PK.IsEmpty)
			{
				if ((ZString)statusInfo.OriginalValue != status)
				{
					Func<StmALog, bool> eventCriteria = (StmALog log) => { return log.SL_SE_NKEvent == statusEvent.Code && !log.IsInDatabase && log.SL_Reference == status; };
					var unsavedEvents = Logs.Find(eventCriteria);

					if (unsavedEvents == null || !unsavedEvents.Any())
					{
						using (DisposableEnvironment.ForBranch(Declaration.Branch.PK.ToGuid()))
						{
							Logs.AddNew(statusEvent, status, ZDateTimeOffset.Now);
						}
					}
				}
			}
		}

		public virtual bool ShouldLogEntryStatus { get { return false; } }

		#region Accounting Invoicing Integration

		protected internal virtual ZDecimal GetTotalChargeValueFor(EntryChargeType chargeTypeElement, ZString methodOfPaymentCode)
		{
			ZDecimal result = ZDecimal.Zero;
			var rateCodes = GetRateCodes(chargeTypeElement.Code).ToList();
			foreach (var code in rateCodes)
			{
				result += Charges.GetAmount(code);
			}
			foreach (CusEntryLine entryLine in MergedLines)
			{
				foreach (var feeType in rateCodes)
				{
					result += entryLine.Fees.GetAmount(feeType);
				}
			}
			return result;
		}

		protected IEnumerable<ZString> GetRateCodes(ZString rateType)
		{
			var list = GetRateCodesCore(rateType).ToList();
			list.Add(rateType);
			return list.Distinct();
		}

		protected virtual IEnumerable<ZString> GetRateCodesCore(ZString rateType)
		{
			return CusRefRateCodeView.Loader.LoadByRateType(Factory, CountryCode, rateType).Select(x => x.ZY1_RateCode);
		}

		protected internal virtual CustomsCharge[] GetNonFeeCountrySpecificCharges()
		{
			return Array.Empty<CustomsCharge>();
		}

		public virtual bool IsFeePaidByBroker(string feeCode, ZString methodOfPaymentCode, ILogger logger)
		{
			return Declaration.JE_PaidBy != Enterprise.MasterFiles.Business.Customs.PaidByCodeList.Codes.CLI;
		}

		ZDecimal ICustomsChargeEntry.GetTotalChargeValueFor(EntryChargeType chargeType, ZString methodOfPaymentCode)
		{
			return GetTotalChargeValueFor(chargeType, methodOfPaymentCode);
		}

		public virtual ZString ReferenceNumber
		{
			get { return CH_BGMReference; }
		}

		CustomsCharge[] ICustomsChargeEntry.GetNonFeeCountrySpecificCharges()
		{
			return GetNonFeeCountrySpecificCharges();
		}

		ZString[] ICustomsChargeEntry.GetMethodsOfPaymentThatCanInfluenceAutoRating()
		{
			return GetMethodsOfPaymentThatCanInfluenceAutoRatingCore();
		}

		protected virtual ZString[] GetMethodsOfPaymentThatCanInfluenceAutoRatingCore()
		{
			return new[] { ZString.Empty };
		}

		ZGuid ICustomsChargeEntry.CreditorPK
		{
			get { return ((ICustomsJobInfo)Declaration).CreditorPK; }
		}

		bool ICustomsChargeEntry.EntryReferenceInChargeDescSupported
		{
			get { return RatingDataRegistry.Instance.IncludeEntryHeaderReferenceInCustomsDisbursementCharges.GetFallBackValueAtAllLevels(RegistryCompanyPK, Guid.Empty, Guid.Empty); }
		}

		JobHeader ICustomsChargeEntry.Job => Declaration.Job;

		ZString ICustomsChargeEntry.UniqueNumber => ((IAccInvoiceDataProvider)this).UniqueNumber;

		ZString ICustomsChargeEntry.PreviousUniqueNumber => ((IAccInvoiceDataProvider)this).PreviousUniqueNumber;

		#endregion

		#region IServiceLocator Members

		object IServiceLocator.GetService(Type serviceType)
		{
			if (serviceType == typeof(ICustomsCharges))
			{
				return GetCustomsChargesProvider();
			}
			return null;
		}

		#endregion

		#region Warehouse Integration

		public virtual bool HaveAmendmentsBeenMadeAndNotYetClearedByCustoms
		{
			get { return haveAmendmentsBeenMadeAndNotYetClearedByCustoms; }
		}
		bool haveAmendmentsBeenMadeAndNotYetClearedByCustoms;

#if DEBUG
		public void SetHaveAmendmentsBeenMadeAndNotYetClearedByCustomsForTesting(bool value)
		{
			haveAmendmentsBeenMadeAndNotYetClearedByCustoms = value;
		}
#endif

		void IntegrateWithWarehouseIfRequired()
		{
			var declaration = Declaration;
			if (declaration != null && !declaration.IsWHSUniversalXMLActive)
			{
				if (ShouldCreateBondedWarehouseInwards || ShouldFinaliseBondedWarehouseOutwards)
				{
					if (HaveAmendmentsBeenMadeAndNotYetClearedByCustoms)
					{
						NotifyThatStockCannotBeUpdated(Res.GetString("dd1bc5ec-eb08-42ff-a6b1-64dac56e7edf", "Please update the bonded warehouse with this job once all amendment messages have been sent and cleared by Customs. This job has not been transferred to the bonded warehouse."));
					}
					else
					{
						try
						{
							if (!declaration.IsExWarehouse)
							{
								try
								{
									declaration.CreateOrUpdateBondedWarehouseInward();
								}
								catch (CannotUpdateStockException ex)
								{
									NotifyThatStockCannotBeUpdated(ex.Message); // TODO: test end-to-end once this works
								}
								catch (MissingDataException ex)
								{
									NotifyThatStockCannotBeUpdated(ex.Message);
								}
							}
							else if (declaration.IsExWarehouse)
							{
								declaration.NotifyBondedWarehouseThatExWarehouseEntryHasCleared(); // TODO: test end-to-end once this works
							}
						}
						catch (Exception ex) when (!ex.IsCriticalException())
						{
							ErrorReporter.ReportOnce("Warehouse integration is broken for Entry: " + EntryNumber + "(" + PK + ")" + " on Dec " + declaration.PK, ex);
						}
					}
				}
			}
		}

#if DEBUG
		protected virtual
#endif
		void NotifyThatStockCannotBeUpdated(string humanReadableExplanation)
		{
			Env.OutgoingCustomsMailManager.CreateAndSaveToCompanyNotificationGroup(Res.GetString("e691be53-205b-44cb-abf0-6ed8968f6c05", "Entry {0} ({1}) : warehouse stock could not be updated.", EntryNumber, Declaration.JobNumber), humanReadableExplanation);
		}

		bool BondedWarehouseActionRequired
		{
			get
			{
				return Declaration != null &&
					Declaration.SupportsBondedWarehousing &&
					Declaration.HaveAllEntriesCleared &&
					StatusChangedToClearedSinceLoading;
			}
		}

		internal bool ShouldCreateBondedWarehouseInwards
		{
			get { return BondedWarehouseActionRequired && !Declaration.IsExWarehouse && Declaration.HasLineGoingIntoAnAutomatedBondedWarehouse; }
		}

		internal bool ShouldFinaliseBondedWarehouseOutwards
		{
			get { return BondedWarehouseActionRequired && Declaration.IsExWarehouse; }
		}

		void PublishEventForDisablingIntegration()
		{
			var supporter = (IWarehouseIntegrationSupporter)this;
			supporter.Logs.AddNew(Events.WarehouseIntegrationDisabled, ZString.Empty);
		}

		#endregion

		#region IDataModelSupporter

		public void PopulateDataModelIfNeeded() => this.PopulateDataModelFromParentIfNeeded(Declaration);

		ZString IDataModelSupporter.DataModel { get => CH_DataModel; set => CH_DataModel = value; }

		#endregion

		public virtual bool IsFormalEntry
		{
			get { return true; }
		}

		internal protected virtual bool HasBeenLodgedAtCustomsForAccIntegration
		{
			get { return HasBeenLodgedAtCustoms; }
		}

		protected virtual bool IsChangingToClearStatusForAccIntegration
		{
			get { return StatusChangedToClearedSinceLoading; }
		}

		protected virtual bool IsStatusChangedToClearedForLoggingCLREvent
		{
			get { return StatusChangedToClearedSinceLoading; }
		}

		protected internal bool StatusChangedToClearedSinceLoading
		{
			get { return IsInDatabase && IsStatusChangingToCleared(PreviousStatus, CurrentStatus); }
		}

		protected virtual ZString PreviousStatus => (ZString)CH_StatusInfo.OriginalValue;
		protected virtual ZString CurrentStatus => CH_Status;

#if DEBUG
		public bool IsStatusChangingToClearedForTesting;
		public bool OverrideIsStatusChangingToClearedForTesting;
#endif
		protected virtual internal bool IsStatusChangingToCleared(ZString originalStatus, ZString newStatus)
		{
#if DEBUG
			if (OverrideIsStatusChangingToClearedForTesting)
			{
				return IsStatusChangingToClearedForTesting;
			}
#endif
			return false;
		}

		protected bool StatusChangedToHeldSinceLoading
		{
			get { return IsInDatabase && StatusChangedToHeldSinceLoadingCore(); }
		}

		protected virtual bool StatusChangedToHeldSinceLoadingCore()
		{
			return false;
		}

		protected virtual bool IsStatusChangingFromAmendmentPendingToCleared(ZString originalStatus, ZString newStatus)
		{
			return false;
		}

		#region PopulateReleaseDateIfNeeded OnFactorySavingBeforeTrabsaction

		void PopulateReleaseDateIfNeeded()
		{
			if (IsInDatabase && ShouldPopulateReleaseDate)
			{
				CH_EntryReleaseDate = GetReleaseDate();
			}
		}

		protected virtual bool IsEntryStatusChangedToClearSinceLoading
		{
			get
			{
				return IsEntryStatusCleared && !CustomsStatusAttributeHelper.IsStatusCleared(Factory, (ZString)CH_EntryStatusInfo.OriginalValue, CountryCode, ZDateTime.Today);
			}
		}

		protected virtual bool ShouldPopulateReleaseDate => false;

		protected virtual ZDateTime GetReleaseDate()
		{
			return ZDateTime.Empty;
		}

		#endregion

		public override void OnSaved(bool saveSucceeded)
		{
			if (!saveSucceeded)
			{
				RecoverFromUnsuccessfulSave();
			}
			base.OnSaved(saveSucceeded);

			if (saveSucceeded && IsFormalEntry && statusChangedToClearForAccIntegration && Declaration != null)
			{
				Declaration.IntegrateWithAccountingIfRequired();
				MarkNeedsAutoRateASPOnSaved();
			}
		}

		public void MarkNeedsAutoRateASPOnSaved()
		{
			NeedsAutoRateASPOnSaved = true;
		}

		public void ClearNeedsAutoRateASP()
		{
			NeedsAutoRateASPOnSaved = false;
		}

#if DEBUG
		public
#else
		protected
#endif
		ZBool NeedsAutoRateASPOnSaved;

		public void RecoverFromUnsuccessfulSave()
		{
			if (IsInDatabase)
			{
				if (ShouldResetBGMReferenceOnUnsuccessfulSave)
				{
					CH_BGMReference = (ZString)CH_BGMReferenceInfo.OriginalValue;
				}
				CH_Status = (ZString)CH_StatusInfo.OriginalValue;
			}
			else
			{
				if (ShouldResetBGMReferenceOnUnsuccessfulSave)
				{
					CH_BGMReference = ZString.Empty;
				}
				CH_Status = ZString.Empty;
			}
		}

		public virtual bool ShouldResetBGMReferenceOnUnsuccessfulSave => true;

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

		#endregion

		#region Related Business objects/Collection

		#region PivotsToEntries
		[ChildEditable(true)]
		[ChildEditableTestExclude]
		public EntryCusContainerEntryHeaderPivotCollection PivotsToContainers
		{
			get
			{
				if (pivotsToContainers == null)
				{
					pivotsToContainers = new EntryCusContainerEntryHeaderPivotCollection(this);
					if (Declaration?.SupportContainerEntryHeaderPivot ?? false)
					{
						pivotsToContainers.Load();
						RegisterEditableChildObject(pivotsToContainers);
					}
					else
					{
						((ILegacyBusinessObjectCollectionInternals)pivotsToContainers).SetOverriddenAdditionalFilter(ZQuery.NoResultQuery);
						pivotsToContainers.SetCountedReadOnlyIncludingChildren(true);
					}
				}
				return pivotsToContainers;
			}
		}
		EntryCusContainerEntryHeaderPivotCollection pivotsToContainers;
		#endregion PivotsToEntries

		public virtual CusEntryInstruction EntryInstruction
		{
			get { return Factory.Load<CusEntryInstruction>(CH_CEI_Instruction); }
		}

		[ChildEditable(true)]
		public virtual EDIMessageCollection Messages
		{
			get
			{
				if (fMessages == null)
				{
					fMessages = GetNewMessageCollection();
					fMessages.Load();
					fMessages.SetReadOnlyIncludingChildren(true);
					RegisterEditableChildObject(fMessages);
				}
				return fMessages;
			}
		}
		EDIMessageCollection fMessages;

		public bool MessagesAreLoaded
		{
			get { return fMessages != null; }
		}

		protected virtual EDIMessageCollection GetNewMessageCollection()
		{
			return new EDIMessageCollection(this, Factory);
		}

		/// <summary>
		/// Active entry lines. It excludes deleted and pending-deletion entry lines
		/// </summary>
		public ICusEntryLineCollection<CusEntryLine> MergedLines
		{
			get
			{
				if (fMergedLines == null)
				{
					fMergedLines = GetMergedLineCollection();
					fMergedLines.CustomSort();
				}
				return fMergedLines;
			}
		}
		ICusEntryLineCollection<CusEntryLine> fMergedLines;

		protected virtual ICusEntryLineCollection<CusEntryLine> GetMergedLineCollection()
		{
			return new CusEntryLineCollection<CusEntryLine>(this);
		}

		/// <summary>
		/// Entry lines whose invoice lines are deleted from db, but yet to be finalised at Customs
		/// </summary>
		public EntryLineStatusFilterCollection PendingDeletionEntryLines
		{
			get
			{
				if (fPendingDeletionEntryLines == null)
				{
					fPendingDeletionEntryLines = new EntryLineStatusFilterCollection(this, EntryLineStatusList.Codes.DeletePending);
				}
				return fPendingDeletionEntryLines;
			}
		}
		EntryLineStatusFilterCollection fPendingDeletionEntryLines;

		public EntryLineStatusFilterCollection DeletedEntryLines
		{
			get
			{
				if (fDeletedEntryLines == null)
				{
					fDeletedEntryLines = new EntryLineStatusFilterCollection(this, EntryLineStatusList.Codes.Deleted);
				}
				return fDeletedEntryLines;
			}
		}
		EntryLineStatusFilterCollection fDeletedEntryLines;

		[ChildEditable(true)]
		public IAllCusEntryLineCollection<CusEntryLine> AllEntryLines
		{
			get
			{
				if (fAllEntryLines == null)
				{
					fAllEntryLines = GetAllEntryLinesCollection();
					RegisterEditableChildObject(fAllEntryLines);
					fAllEntryLines.Load();
				}
				return fAllEntryLines;
			}
		}
		IAllCusEntryLineCollection<CusEntryLine> fAllEntryLines;

		protected virtual IAllCusEntryLineCollection<CusEntryLine> GetAllEntryLinesCollection() => new AllCusEntryLineCollection<CusEntryLine>(this);

		[ChildEditable(true)]
		public ICusEntryPayInfoCollection<CusEntryPayInfo> EntryPayInfos
		{
			get
			{
				if (fEntryPayInfos == null)
				{
					fEntryPayInfos = CreateNewEntryPayInfosCollection();
					fEntryPayInfos.Load();
					RegisterEditableChildObject(fEntryPayInfos);
				}
				return fEntryPayInfos;
			}
		}
		ICusEntryPayInfoCollection<CusEntryPayInfo> fEntryPayInfos;

		protected virtual ICusEntryPayInfoCollection<CusEntryPayInfo> CreateNewEntryPayInfosCollection()
		{
			return new CusEntryPayInfoCollection<CusEntryPayInfo>(this);
		}

		BaseJobDeclaration fDeclaration;
		public BaseJobDeclaration Declaration
		{
			get { return DeclarationCore; }
		}

		// for mocking
		protected virtual BaseJobDeclaration DeclarationCore
		{
			get
			{
				ZGuid foreignKey = (IsDeleted || CH_JE.IsEmpty) ? cH_JECachedOnRemovedFromDependentCollection : CH_JE;
				if (fDeclaration == null || fDeclaration.PK != foreignKey)
				{
					fDeclaration = Factory.Load<BaseJobDeclaration>(foreignKey);
				}
				return fDeclaration != null && !fDeclaration.IsDeleted ? fDeclaration : null;
			}
		}

		public ZString HouseBillsCommaSeparated
		{
			get
			{
				ZStringBuilder result = new ZStringBuilder();
				foreach (Bill bill in Bills)
				{
					if (bill.CU_BillType == BillTypeList.Codes.HouseBill)
					{
						result.Append(bill.CU_BillNum);
					}
				}
				return result.ToStringWithDelimiterBetweenAppends(",");
			}
		}

		public ZString MasterBillsCommaSeparated
		{
			get
			{
				ZStringBuilder result = new ZStringBuilder();
				foreach (Bill bill in Bills)
				{
					if (bill.CU_BillType == BillTypeList.Codes.MasterBill)
					{
						result.Append(bill.CU_BillNum);
					}
				}
				return result.ToStringWithDelimiterBetweenAppends(",");
			}
		}

		public ZString DirectMasterOrLinkedMasterBillsCommaSeparated
		{
			get
			{
				ZStringBuilder result = new ZStringBuilder();
				foreach (Bill bill in Bills)
				{
					ZString masterBill = bill.CU_MasterBill;
					if (!masterBill.IsEmpty && bill.IsLowestBill)
					{
						result.Append(masterBill);
					}
				}
				return result.ToStringWithDelimiterBetweenAppends(",");
			}
		}

		/// <summary>
		/// This includes all kinds of bills master bill/house bill/subhouse bill.
		/// </summary>
		public
#if DEBUG
		virtual
#endif
		BillCollectionForEntry Bills
		{
			get
			{
				if (fBills == null)
				{
					fBills = GetBillsForEntry();
					fBills.PopulateBills();
				}
				return fBills;
			}
		}

		protected virtual BillCollectionForEntry GetBillsForEntry()
		{
			return new BillCollectionForEntry(this);
		}
		BillCollectionForEntry fBills;

		public (IDictionary<BaseCusContainer, IReadOnlyCollection<CusEntryLine>> containers, IDictionary<CusEquipment, IReadOnlyCollection<CusEntryLine>> equipments) GetContainerOrEquipmentToEntryLineMapping()
			=> Factory.GetValue(ref containerOrEquipmentToInvoiceLineMapping, () =>
				{
					var containers = new Dictionary<BaseCusContainer, IReadOnlyCollection<CusEntryLine>>();
					var equipments = new Dictionary<CusEquipment, IReadOnlyCollection<CusEntryLine>>();
					if (Declaration is BaseJobDeclaration declaration)
					{
						var containerCount = declaration.ContainersRequired ? declaration.CusContainers.Count : 0;
						var equipmentCount = declaration.EquipmentsRequired ? declaration.Equipments.Count : 0;
						var hasContainers = containerCount > 0;
						var hasEquipments = equipmentCount > 0;
						if (hasContainers || hasEquipments)
						{
							foreach (CusEntryLine entryLine in MergedLines)
							{
								GatherContainerOrEquipmentToEntryLineMapping(hasContainers, containerCount, containers, hasEquipments, equipmentCount, equipments, entryLine);
							}
						}
					}
					return (containers, equipments);
				});

		void GatherContainerOrEquipmentToEntryLineMapping(bool hasContainers, int containerCount, Dictionary<BaseCusContainer, IReadOnlyCollection<CusEntryLine>> containerDictonary, bool hasEquipments, int equipmentCount, Dictionary<CusEquipment, IReadOnlyCollection<CusEntryLine>> equipmentsDictonary, CusEntryLine entryLine)
		{
			var entryLineContainers = new HashSet<BaseCusContainer>();
			var entryLineEquipments = new HashSet<CusEquipment>();
			foreach (BaseJobComInvoiceLine invoiceLine in entryLine.InvoiceLines)
			{
				foreach (var package in invoiceLine.PackagesPivot.Cast<InvoiceLinePackagePivot>().Select(p => p.Package))
				{
					if (package?.PackingGroup is BasePackingGroup packingGroup)
					{
						if (hasContainers && packingGroup.Container is BaseCusContainer container)
						{
							AddOrUpdateContainerOrEquipmentDictionary(containerDictonary, container, entryLine);
							if (entryLineContainers.Add(container) && entryLineContainers.Count == containerCount
								&& entryLineEquipments.Count == equipmentCount)
							{
								return;
							}
						}
						else if (hasEquipments && packingGroup.Equipment is CusEquipment equipment)
						{
							AddOrUpdateContainerOrEquipmentDictionary(equipmentsDictonary, equipment, entryLine);
							if (entryLineEquipments.Add(equipment) && entryLineEquipments.Count == equipmentCount
								&& entryLineContainers.Count == containerCount)
							{
								return;
							}
						}
					}
				}
			}
		}

		CachedProperty<(IDictionary<BaseCusContainer, IReadOnlyCollection<CusEntryLine>> containers, IDictionary<CusEquipment, IReadOnlyCollection<CusEntryLine>> equipments)> containerOrEquipmentToInvoiceLineMapping;

		void AddOrUpdateContainerOrEquipmentDictionary<T>(Dictionary<T, IReadOnlyCollection<CusEntryLine>> dictonary, T containerOrEquipment, CusEntryLine cusEntryLine)
			where T : BusinessObject
		{
			HashSet<CusEntryLine> list;
			if (dictonary.TryGetValue(containerOrEquipment, out var dictionaryValue))
			{
				list = (HashSet<CusEntryLine>)dictionaryValue;
			}
			else
			{
				list = new HashSet<CusEntryLine>();
				dictonary.Add(containerOrEquipment, list);
			}
			list.Add(cusEntryLine);
		}

		public BaseCusContainer[] Containers => Factory.GetValue(ref containersCached, () => new ContainerCalculator(this).GetContainers());
		CachedProperty<BaseCusContainer[]> containersCached;

#if DEBUG

		public void SetDeclarationForTesting(BaseJobDeclaration declaration)
		{
			if (!Globals.IsTest)
			{
				throw new NotSupportedException("test only!");
			}

			CH_JE = declaration.PK;
			fDeclaration = declaration;
		}

#endif

		[ChildEditable(true)]
		public ICusEntryHeaderChargesCollection<CusEntryHeaderCharges> Charges
		{
			get
			{
				if (fCharges == null)
				{
					fCharges = CreateNewCusEntryHeaderChargesCollection();
					fCharges.Load();
					RegisterEditableChildObject(fCharges);
				}
				return fCharges;
			}
		}
		ICusEntryHeaderChargesCollection<CusEntryHeaderCharges> fCharges;

		protected virtual ICusEntryHeaderChargesCollection<CusEntryHeaderCharges> CreateNewCusEntryHeaderChargesCollection() => new CusEntryHeaderChargesCollection<CusEntryHeaderCharges>(this);

		[ChildEditable(true)]
		public IConfirmedCusEntryHeaderChargesCollection<CusEntryHeaderCharges> ConfirmedCharges
		{
			get
			{
				if (fConfirmedCharges == null)
				{
					fConfirmedCharges = CreateNewConfirmedCusEntryHeaderChargesCollection();
					fConfirmedCharges.SetReadOnlyIncludingChildren(true);
					fConfirmedCharges.Load();
					RegisterEditableChildObject(fConfirmedCharges);
				}
				return fConfirmedCharges;
			}
		}
		IConfirmedCusEntryHeaderChargesCollection<CusEntryHeaderCharges> fConfirmedCharges;

		protected virtual IConfirmedCusEntryHeaderChargesCollection<CusEntryHeaderCharges> CreateNewConfirmedCusEntryHeaderChargesCollection() => new ConfirmedCusEntryHeaderChargesCollection<CusEntryHeaderCharges>(this);

		public bool IsInProcessOfMerging => Declaration is BaseJobDeclaration declaration && declaration.IsMergeInProgress;

		public IEnumerable<BaseJobComInvoiceLine> InvoiceLines
		{
			get
			{
				if (IsInProcessOfMerging)
				{
					throw new NotSupportedException("Entry is being merged and this list does not have complete invoice lines");
				}

				if (invoiceLines == null)
				{
					invoiceLines = new List<BaseJobComInvoiceLine>();

					foreach (CusEntryLine entryLine in MergedLines)
					{
						if (entryLine.AddInvoiceLinesToEntryInvoiceLines)
						{
							invoiceLines.AddRange(new TypedEnumerable<BaseJobComInvoiceLine>(entryLine.InvoiceLines));
						}
					}
				}
				return invoiceLines;
			}
		}
		List<BaseJobComInvoiceLine> invoiceLines;

		[ChildEditable(true)]
		[ChildEditableTestExclude]
		public ICusEntrySnapshotCollection<CusEntrySnapshot> Snapshots
		{
			get
			{
				if (snapshots == null)
				{
					snapshots = CreateNewEntrySnapshotsCollection();
					if (Declaration?.SupportEntrySnpashots ?? false)
					{
						snapshots.Load();
						RegisterEditableChildObject(snapshots);
					}
					else
					{
						((ILegacyBusinessObjectCollectionInternals)snapshots).SetOverriddenAdditionalFilter(ZQuery.NoResultQuery);
						snapshots.SetCountedReadOnlyIncludingChildren(true);
					}
				}
				return snapshots;
			}
		}
		ICusEntrySnapshotCollection<CusEntrySnapshot> snapshots;

		protected virtual ICusEntrySnapshotCollection<CusEntrySnapshot> CreateNewEntrySnapshotsCollection()
		{
			return new CusEntrySnapshotCollection<CusEntrySnapshot>(this);
		}

		#endregion

		#region Properties overrides

		public override ZString CH_DataModel
		{
			get { return base.CH_DataModel; }
			set
			{
				this.ReportDataModelErrorIfNeeded(CH_DataModelInfo, value);
				base.CH_DataModel = value;
			}
		}

		public override ZGuid CH_CEI_Instruction
		{
			get => base.CH_CEI_Instruction;
			set
			{
				var oldEntryInstruction = IsCopying ? null : EntryInstruction;
				var oldValue = CH_CEI_Instruction;
				base.CH_CEI_Instruction = value;
				if (!IsCopying && oldValue != CH_CEI_Instruction)
				{
					oldEntryInstruction?.MarkAsNeedingValidation();
					EntryInstruction?.MarkAsNeedingValidation();
				}
			}
		}

		ZGuid cH_JECachedOnRemovedFromDependentCollection;
		public override ZGuid CH_JE
		{
			get { return base.CH_JE; }
			set
			{
				bool hasChanged = CH_JE != value;
				if (hasChanged && value.IsEmpty)
				{
					cH_JECachedOnRemovedFromDependentCollection = base.CH_JE;
				}
				base.CH_JE = value;
			}
		}

		[List(nameof(Lookups) + "." + nameof(CusEntryHeaderLookups.WarehouseTransactionStatusList))]
		public override ZString CH_WarehouseTransactionStatus
		{
			get { return base.CH_WarehouseTransactionStatus; }
			set
			{
				var oldValue = CH_WarehouseTransactionStatus;
				base.CH_WarehouseTransactionStatus = value;
				if (!IsCopying && oldValue != CH_WarehouseTransactionStatus)
				{
					if (value == WarehouseTransactionStatusList.Codes.AutomationIsDisabled)
					{
						PublishEventForDisablingIntegration();
					}

					var dec = Declaration;
					if (dec != null)
					{
						dec.WarehouseTransactionStatusInfo.RefreshBinding();
						dec.WarehouseTransactionStatusDescriptionInfo.RefreshBinding();
					}
				}
			}
		}

		[ResourceStringData("Enterprise.Customs.Business.CusEntryHeader|CH_WarehouseTransactionStatusDescription", Caption = "Warehouse Status Description", ShortCaption = "WHS Status Desc.")]
		public ZString CH_WarehouseTransactionStatusDescription
		{
			get { return Lookups.WarehouseTransactionStatusList.GetDescriptionFromCode(CH_WarehouseTransactionStatus); }
		}

		public ZPropertyInfo CH_WarehouseTransactionStatusDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.CH_WarehouseTransactionStatusDescription); }
		}

		[ResourceStringData("Enterprise.Customs.Business.CusEntryHeader|CH_HasManualWhsUpdate", Caption = "Manual Warehouse Update done", ShortCaption = "Manual WHS Updated")]
		public override ZBool CH_HasManualWhsUpdate { get => base.CH_HasManualWhsUpdate; set => base.CH_HasManualWhsUpdate = value; }

		[List(nameof(Lookups) + "." + nameof(CusEntryHeaderLookups.CH_MessageTypeList))]
		public override ZString CH_MessageType
		{
			get { return base.CH_MessageType; }
			set { base.CH_MessageType = value; }
		}

		public override ZString CH_Status
		{
			get { return base.CH_Status; }
			set
			{
				OnChangingCH_Status(CH_Status, value);
				base.CH_Status = value;
			}
		}

		protected virtual void OnChangingCH_Status(ZString oldStatus, ZString newStatus)
		{
			if (IsStatusChangingFromNotAcceptedToAccepted(oldStatus, newStatus))
			{
				UpdateWhenStatusIsAboutToChangeToClear(oldStatus, newStatus);
			}
			if (IsStatusChangingFromAmendmentPendingToCleared(oldStatus, newStatus))
			{
				if (Declaration?.ShouldKeepDeletedLinesOnAmendmentCleared ?? true)
				{
					ChangeEntryLineStatusToDeletedWhenAmendmentCleared();
				}
				else
				{
					PendingDeletionEntryLines.RemoveAndDeleteAll();
				}
			}
		}

		[ResourceStringData("Enterprise.Customs.Business.CusEntryHeader|CH_EntryReleaseDate", Caption = "Release Date")]
		public override ZDateTime CH_EntryReleaseDate { get => base.CH_EntryReleaseDate; set => base.CH_EntryReleaseDate = value; }

		/// <summary>
		/// Warning: The field is not to be used to manage Exit but rather it is used in order to record the Exited Status of the Export. Under UCC6, there should only be a couple of statuses i.e. Exported or Refused Exit.
		/// <see href="">WI00517471 - FR - Update status ESO</see>
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Decruftification", "WTG3007:Overrides should not simply call base.", Justification = "Retained for comment")]
		public override ZString CH_ExitedStatus { get => base.CH_ExitedStatus; set => base.CH_ExitedStatus = value; }

		#endregion

		#region New Properties

		public string EntryHeaderDescriptiveMenuItemText
		{
			get
			{
				var result = new ZStringBuilder();
				var entryInstruction = EntryInstruction;
				if (entryInstruction != null)
				{
					result.AppendIfNotEmpty(entryInstruction.CEI_Style);
					result.AppendIfNotEmpty(entryInstruction.CEI_Description);
				}
				result.AppendIfNotEmpty(EntryNumber);
				return result.ToStringWithDelimiterBetweenAppends(" - ");
			}
		}

		public OrgAddress WarehouseAddress
		{
			get { return Declaration?.WarehouseDocAddress?.Address; }
		}

		public bool HasLineComingOutOfABondedWarehouse
		{
			get
			{
				if (hasLineComingOutOfABondedWarehouse == null)
				{
					hasLineComingOutOfABondedWarehouse = new CachedProperty<bool>(Factory, GetHasLineComingOutOfABondedWarehouse);
				}
				return hasLineComingOutOfABondedWarehouse.Value;
			}
		}
		CachedProperty<bool> hasLineComingOutOfABondedWarehouse;

		bool GetHasLineComingOutOfABondedWarehouse()
		{
			return InvoiceLines.OfType<BaseJobComInvoiceLine>().Any(x => x.UseBondedWarehouseAutomation);
		}

		public bool HasLineGoingIntoABondedWarehouse
		{
			get
			{
				if (hasLineGoingIntoABondedWarehouse == null)
				{
					hasLineGoingIntoABondedWarehouse = new CachedProperty<bool>(Factory, GetHasLineGoingIntoABondedWarehouse);
				}
				return hasLineGoingIntoABondedWarehouse.Value;
			}
		}
		CachedProperty<bool> hasLineGoingIntoABondedWarehouse;

		bool GetHasLineGoingIntoABondedWarehouse()
		{
			return InvoiceLines.OfType<BaseJobComInvoiceLine>().Any(x => x.IsGoingIntoBondedWarehouse);
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
			return HasLineGoingIntoABondedWarehouse;
		}

		public bool HasWHSTransaction
		{
			get { return Enterprise.Customs.Business.WarehouseExtensions.JobDeclarationWarehouseExtensions.HasWHSTransaction(this); }
		}

		public bool HasWHSInwardTransaction
		{
			get
			{
				if (hasWHSInwardTransactionCached == null)
				{
					hasWHSInwardTransactionCached = new CachedProperty<bool>(Factory, () =>
					{
						var whsStatus = CH_WarehouseTransactionStatus;
						return whsStatus != WarehouseTransactionStatusList.Codes.InwardCanceled && IsWarehouseIntegrationActive && WarehouseTransactionStatusList.IsInwardCode(whsStatus);
					});
				}
				return hasWHSInwardTransactionCached.Value;
			}
		}
		CachedProperty<bool> hasWHSInwardTransactionCached;

		public bool HasWHSOutwardTransaction
		{
			get
			{
				if (hasWHSOutwardTransactionCached == null)
				{
					hasWHSOutwardTransactionCached = new CachedProperty<bool>(Factory, () =>
					{
						var whsStatus = CH_WarehouseTransactionStatus;
						return whsStatus != WarehouseTransactionStatusList.Codes.OutwardCanceled && IsWarehouseIntegrationActive && WarehouseTransactionStatusList.IsOutwardCode(whsStatus);
					});
				}
				return hasWHSOutwardTransactionCached.Value;
			}
		}
		CachedProperty<bool> hasWHSOutwardTransactionCached;

		public bool HasWHSChangeOfOwnershipTransaction
		{
			get
			{
				if (hasWHSChangeOfOwnershipTransactionCached == null)
				{
					hasWHSChangeOfOwnershipTransactionCached = new CachedProperty<bool>(Factory, () =>
					{
						var whsStatus = CH_WarehouseTransactionStatus;
						return whsStatus != WarehouseTransactionStatusList.Codes.ChangeOfOwnershipCanceled && IsWarehouseIntegrationActive && WarehouseTransactionStatusList.IsChangeOfOwnershipCode(whsStatus);
					});
				}
				return hasWHSChangeOfOwnershipTransactionCached.Value;
			}
		}
		CachedProperty<bool> hasWHSChangeOfOwnershipTransactionCached;

		public bool HasWHSChangeOfRegimeTransaction
		{
			get
			{
				if (hasWHSChangeOfRegimeTransactionCached == null)
				{
					hasWHSChangeOfRegimeTransactionCached = new CachedProperty<bool>(Factory, () =>
					{
						var whsStatus = CH_WarehouseTransactionStatus;
						return whsStatus != WarehouseTransactionStatusList.Codes.ChangeOfRegimeCanceled && IsWarehouseIntegrationActive && WarehouseTransactionStatusList.IsChangeOfRegimeCode(whsStatus);
					});
				}
				return hasWHSChangeOfRegimeTransactionCached.Value;
			}
		}
		CachedProperty<bool> hasWHSChangeOfRegimeTransactionCached;

		public bool IsChangeOfOwnershipWarehousing => EntryInstruction?.IsChangeOfOwnershipWarehousing ?? false;

		public bool IsChangeOfOwnershipBondedWarehousingEnabled
		{
			get
			{
				if (isChangeOfOwnershipBondedWarehousingEnabledCached == null)
				{
					isChangeOfOwnershipBondedWarehousingEnabledCached = new CachedProperty<bool>(Factory, () =>
					{
						return SupportsBondedWarehousing && IsChangeOfOwnershipWarehousing;
					});
				}
				return isChangeOfOwnershipBondedWarehousingEnabledCached.Value;
			}
		}
		CachedProperty<bool> isChangeOfOwnershipBondedWarehousingEnabledCached;

		public bool IsChangeOfRegimeWarehousing => EntryInstruction?.IsChangeOfRegimeWarehousing ?? false;

		public bool IsChangeOfRegimeWarehousingEnabled
		{
			get
			{
				if (isChangeOfRegimeWarehousingEnabledCached == null)
				{
					isChangeOfRegimeWarehousingEnabledCached = new CachedProperty<bool>(Factory, () =>
					{
						return SupportsBondedWarehousing && IsChangeOfRegimeWarehousing;
					});
				}
				return isChangeOfRegimeWarehousingEnabledCached.Value;
			}
		}
		CachedProperty<bool> isChangeOfRegimeWarehousingEnabledCached;

		public bool IsOutwardBondedWarehousingEnabled
		{
			get
			{
				if (isOutwardBondedWarehousingEnabledCached == null)
				{
					isOutwardBondedWarehousingEnabledCached = new CachedProperty<bool>(Factory, () =>
					{
						return SupportsBondedWarehousing && IsOutwardBondedWarehousingEnabledCore;
					});
				}
				return isOutwardBondedWarehousingEnabledCached.Value;
			}
		}
		CachedProperty<bool> isOutwardBondedWarehousingEnabledCached;

		protected virtual bool IsOutwardBondedWarehousingEnabledCore
		{
			get
			{
				var declaration = Declaration;
				return declaration != null && (declaration.SupportMultipleWarehouseEntry ? HasLineComingOutOfABondedWarehouse : (bool)declaration.IsExWarehouse);
			}
		}

		public bool IsExWarehouse
		{
			get { return Declaration?.IsExWarehouse ?? false; }
		}

		public bool IsInwardBondedWarehousingEnabled
		{
			get
			{
				if (isInwardBondedWarehousingEnabledCached == null)
				{
					isInwardBondedWarehousingEnabledCached = new CachedProperty<bool>(Factory, () =>
					{
						return SupportsBondedWarehousing && IsInwardBondedWarehousingEnabledCore;
					});
				}
				return isInwardBondedWarehousingEnabledCached.Value;
			}
		}
		CachedProperty<bool> isInwardBondedWarehousingEnabledCached;

		protected virtual bool IsInwardBondedWarehousingEnabledCore
		{
			get
			{
				var declaration = Declaration;
				return declaration != null && (declaration.SupportMultipleWarehouseEntry ? HasLineGoingIntoABondedWarehouse : declaration.IsImport && HasLinesForInwardBondedWarehousing);
			}
		}

		public bool IsImport => IsImportCore();

		protected virtual bool IsImportCore() => Declaration?.IsImport ?? false;

		public bool IsExport => IsExportCore();

		protected virtual bool IsExportCore() => Declaration?.IsExport ?? false;

		public virtual bool IsBondedWarehousingDisabled
		{
			get { return WarehouseTransactionStatusList.IsAutomationDisabled(CH_WarehouseTransactionStatus); }
		}

		public bool SupportsBondedWarehousing
		{
			get
			{
				var declaration = Declaration;
				return declaration != null && declaration.IsWHSUniversalXMLActive && SupportsBondedWarehousingCore(declaration);
			}
		}

		internal bool SupportsBondedWarehousingCore(BaseJobDeclaration declaration)
		{
			return declaration.SupportMultipleWarehouseEntry ? SupportsBondedWarehousingForEntry : declaration.SupportsBondedWarehousing;
		}

		protected bool SupportsBondedWarehousingForEntry
		{
			get
			{
				var result = HasWHSTransaction;
				if (!result)
				{
					var entryInstruction = EntryInstruction;
					if (entryInstruction != null)
					{
						result = GetSupportsBondedWarehousingForEntry(entryInstruction);
					}
				}
				return result;
			}
		}

		protected virtual bool GetSupportsBondedWarehousingForEntry(CusEntryInstruction entryInstruction)
		{
			var clientIsBondedWarehousing = entryInstruction.ClientIsBondedWarehousing;
			var warehouseIsBondedWarehousing = entryInstruction.WarehouseIsBondedWarehousing;
			var warehouse2IsBondedWarehousing = entryInstruction.Warehouse2IsBondedWarehousing;
			var ownerIsBondedWarehousing = entryInstruction.OwnerIsBondedWarehousing;

			return
				((clientIsBondedWarehousing || warehouseIsBondedWarehousing || warehouse2IsBondedWarehousing || ownerIsBondedWarehousing) && entryInstruction.HasBothOutOfAndIntoRegimeProcedure) ||
				((warehouse2IsBondedWarehousing || clientIsBondedWarehousing) && entryInstruction.HasIntoWarehouseProcedure) ||
				((warehouseIsBondedWarehousing || clientIsBondedWarehousing) && entryInstruction.HasOutOfWarehouseProcedure) ||
				GetSupportsBondedWarehousingForEntryInwardProcessingOrOutwardProcessing(entryInstruction);
		}

		bool GetSupportsBondedWarehousingForEntryInwardProcessingOrOutwardProcessing(CusEntryInstruction entryInstruction)
		{
			var clientIsInwardProcessing = entryInstruction.ClientIsInwardProcessing;
			var clientIsOutwardProcessing = entryInstruction.ClientIsOutwardProcessing;
			return
				((clientIsInwardProcessing || entryInstruction.Warehouse2IsInwardProcessing) && entryInstruction.HasIntoInwardProcessingProcedure) ||
				((clientIsOutwardProcessing || entryInstruction.Warehouse2IsOutwardProcessing) && entryInstruction.HasIntoOutwardProcessingProcedure) ||
				((clientIsInwardProcessing || entryInstruction.WarehouseIsInwardProcessing) && entryInstruction.HasOutOfInwardProcessingProcedure) ||
				((clientIsOutwardProcessing || entryInstruction.WarehouseIsOutwardProcessing) && entryInstruction.HasOutOfOutwardProcessingProcedure);
		}

		public bool IsBondedWarehousingFieldValidationRequired
		{
			get
			{
				if (isBondedWarehousingFieldValidationRequiredCached == null)
				{
					isBondedWarehousingFieldValidationRequiredCached = new CachedProperty<bool>(Factory, () =>
					{
						var declaration = Declaration;
						return declaration != null && declaration.IsWHSUniversalXMLActive && SupportsBondedWarehousingCore(declaration) && (HasWHSTransaction || IsInwardBondedWarehousingEnabled || IsOutwardBondedWarehousingEnabled || IsChangeOfOwnershipBondedWarehousingEnabled || IsChangeOfRegimeWarehousingEnabled);
					});
				}
				return isBondedWarehousingFieldValidationRequiredCached.Value;
			}
		}
		CachedProperty<bool> isBondedWarehousingFieldValidationRequiredCached;

		public bool HasABondedWarehousingInvoiceWithDifferentWarehouseClient
		{
			get
			{
				if (hasABondedWarehousingInvoiceWithDifferentWarehouseClientCached == null)
				{
					hasABondedWarehousingInvoiceWithDifferentWarehouseClientCached = new CachedProperty<bool>(Factory, () =>
					{
						return IsExport ?
							InvoiceHeaders.OfType<BaseJobComInvoiceHeader>()
							.Any(x => x.IsABondedWarehousingInvoiceWithDifferentSupplier) :
							InvoiceHeaders.OfType<BaseJobComInvoiceHeader>()
							.Any(x => x.IsABondedWarehousingInvoiceWithDifferentImporter);
					});
				}
				return hasABondedWarehousingInvoiceWithDifferentWarehouseClientCached.Value;
			}
		}
		CachedProperty<bool> hasABondedWarehousingInvoiceWithDifferentWarehouseClientCached;

		public bool HasAnInvoiceLineMarkedForBondedWarehousingWithoutAnOwnerProduct
		{
			get
			{
				if (hasAnInvoiceLineMarkedForBondedWarehousingWithoutAnOwnerProductCached == null)
				{
					hasAnInvoiceLineMarkedForBondedWarehousingWithoutAnOwnerProductCached = new CachedProperty<bool>(Factory, GetHasAnInvoiceLineMarkedForBondedWarehousingWithoutAnOwnerProduct);
				}
				return hasAnInvoiceLineMarkedForBondedWarehousingWithoutAnOwnerProductCached.Value;
			}
		}
		CachedProperty<bool> hasAnInvoiceLineMarkedForBondedWarehousingWithoutAnOwnerProductCached;

		bool GetHasAnInvoiceLineMarkedForBondedWarehousingWithoutAnOwnerProduct()
		{
			var result = false;
			var entryInstruction = EntryInstruction;
			if (entryInstruction != null && entryInstruction.HasBothOutOfAndIntoRegimeProcedure && !entryInstruction.IsOutOfRegime)
			{
				result = false;
				foreach (BaseJobComInvoiceLine invoiceLine in InvoiceLines)
				{
					var ownerPart = (invoiceLine as IChangeOfOwnershipLineDetails)?.OwnerPart;
					if (ownerPart == null)
					{
						result = true;
						break;
					}
				}
			}
			return result;
		}

		public bool HasAnInvoiceLineMarkedForBondedWarehousingWithoutAProduct
		{
			get
			{
				if (hasAnInvoiceLineMarkedForBondedWarehousingWithoutAProductCached == null)
				{
					hasAnInvoiceLineMarkedForBondedWarehousingWithoutAProductCached = new CachedProperty<bool>(Factory, GetHasAnInvoiceLineMarkedForBondedWarehousingWithoutAProduct);
				}
				return hasAnInvoiceLineMarkedForBondedWarehousingWithoutAProductCached.Value;
			}
		}
		CachedProperty<bool> hasAnInvoiceLineMarkedForBondedWarehousingWithoutAProductCached;

		bool GetHasAnInvoiceLineMarkedForBondedWarehousingWithoutAProduct()
		{
			var result = false;
			if (SupportsBondedWarehousing)
			{
				var declaration = Declaration;
				if (declaration != null && (!declaration.SupportMultipleWarehouseEntry || ProductIsApplicableForProcedure))
				{
					foreach (BaseJobComInvoiceLine invoiceLine in InvoiceLines)
					{
						if (invoiceLine.Part == null && BondedWarehousingHelper.IsMarkedForBondedWarehousing(invoiceLine))
						{
							result = true;
							break;
						}
					}
				}
			}
			return result;
		}

		bool ProductIsApplicableForProcedure
		{
			get
			{
				var entryInstruction = EntryInstruction;
				return (entryInstruction != null && (entryInstruction.HasBothOutOfAndIntoRegimeProcedure ?
					entryInstruction.WarehouseIsInventoryManagementOn || entryInstruction.Warehouse2IsInventoryManagementOn || entryInstruction.ClientIsInventoryManagementOn
					: entryInstruction.HasOutOfRegimeProcedure || entryInstruction.HasIntoRegimeProcedure));
			}
		}

		public bool HasAnInvoiceLineMarkedForBondedWarehousing
		{
			get
			{
				if (hasAnInvoiceLineMarkedForBondedWarehousingCached == null)
				{
					hasAnInvoiceLineMarkedForBondedWarehousingCached = new CachedProperty<bool>(Factory, GetHasAnInvoiceLineMarkedForBondedWarehousing);
				}
				return hasAnInvoiceLineMarkedForBondedWarehousingCached.Value;
			}
		}
		CachedProperty<bool> hasAnInvoiceLineMarkedForBondedWarehousingCached;

		bool GetHasAnInvoiceLineMarkedForBondedWarehousing()
		{
			var result = false;
			if (SupportsBondedWarehousing)
			{
				result = InvoiceLines.OfType<BaseJobComInvoiceLine>().Any(x => BondedWarehousingHelper.IsMarkedForBondedWarehousing(x));
			}
			return result;
		}

		public static string BondedWarehouseIsRequiredForOutwardBondedWarehousing(string importerCode, string entryDetail)
		{
			return Res.GetString("{8806E9CD-41B6-43FC-BA58-175575B5A8F9}", "Inventory recording/Bonded Warehouse Integration is active for Importer '{0}'. Please enter a Bonded Warehouse that will be used to withdraw the stock that will be declared for Entry ({1}).", importerCode, entryDetail);
		}

		public static string BondedWarehouseIsRequiredForInwardBondedWarehousing(string importerCode, string entryDetail)
		{
			return Res.GetString("{A8A25FC6-7124-449B-8BB8-6CF00CDF54B0}", "Inventory recording/Bonded Warehouse Integration is active for Importer '{0}'. Please enter a Bonded Warehouse that will be used to receive the stock that will be declared for Entry ({1}).", importerCode, entryDetail);
		}

		public static string BondedWarehouseAddressShouldBeInsideDeclarationCountry(string importerCode, string entryDetail, string country)
		{
			return Res.GetString("{0C302A50-6A37-4501-B4FB-85D1EDFF3510}", "Inventory recording/Bonded Warehouse Integration is active for Importer '{0}'. Please enter a Bonded Warehouse on Entry ({1}) that is located within {2}.", importerCode, entryDetail, country);
		}

		public ZString GetMessageErrorOfRequiredFieldsForBondedWarehousing(bool checkProduct = true, bool checkQuantity = true, bool checkEntryDetails = true)
		{
			var messageErrors = new ZStringBuilder();
			if (IsBondedWarehousingFieldValidationRequired)
			{
				var declaration = Declaration;
				if (declaration.SupportMultipleWarehouseEntry)
				{
					if (IsInwardBondedWarehousingEnabled)
					{
						CheckWarehouseAddress(messageErrors, declaration, GetIntoWarehouseAddress(), BondedWarehouseIsRequiredForInwardBondedWarehousing, BondedWarehouseAddressShouldBeInsideDeclarationCountry);
					}
					if (IsOutwardBondedWarehousingEnabled)
					{
						CheckWarehouseAddress(messageErrors, declaration, GetOutOfWarehouseAddress(), BondedWarehouseIsRequiredForOutwardBondedWarehousing, BondedWarehouseAddressShouldBeInsideDeclarationCountry);
					}
				}
				else
				{
					CheckWarehouseAddress(messageErrors, declaration, GetIntoWarehouseAddress(), (importerCode, entryDetail) =>
					{
						return BaseJobDeclaration.BondedWarehouseIsRequiredForBondedWarehousing(declaration.TermNameForBondedWarehouse, importerCode);
					},
					(importerCode, entryDetail, country) =>
					{
						return BaseJobDeclaration.BondedWarehouseAddressShouldBeInsideDeclarationCountry(declaration.TermNameForBondedWarehouse, importerCode, country);
					});
				}

				var isExport = declaration.IsExport;
				if (isExport)
				{
					if (declaration.SupplierDocumentaryAddress.Address == null)
					{
						messageErrors.Append(declaration.SupplierDocumentaryAddressIsRequiredForBondedWarehousing);
					}
					else if (!(declaration.Supplier?.OH_IsWarehouseClient ?? false))
					{
						messageErrors.Append(BaseJobDeclaration.SupplierMustBeMarkedAsWarehouseClient(declaration.TermNameForBondedWarehouse));
					}
				}

				if (IsImport)
				{
					if (declaration.ImporterDocumentaryAddress.Address == null)
					{
						messageErrors.Append(declaration.ImporterDocumentaryAddressIsRequiredForBondedWarehousing);
					}
					else if (!(declaration.Importer?.OH_IsWarehouseClient ?? false))
					{
						messageErrors.Append(BaseJobDeclaration.ImporterMustBeMarkedAsWarehouseClient(declaration.TermNameForBondedWarehouse));
					}
				}

				if (HasABondedWarehousingInvoiceWithDifferentWarehouseClient)
				{
					messageErrors.Append(isExport ? BaseJobDeclaration.BondedWarehousingInvoiceCannotHaveDifferentSupplierMessage(declaration.TermNameForBondedWarehouse) : BaseJobDeclaration.BondedWarehousingInvoiceCannotHaveDifferentImporterMessage(declaration.TermNameForBondedWarehouse));
				}
				if (checkProduct)
				{
					if (HasAnInvoiceLineMarkedForBondedWarehousingWithoutAProduct)
					{
						messageErrors.Append(BaseJobDeclaration.InvoiceLineMarkedForBondedWarehousingRequiresAProduct(declaration.TermNameForBondedWarehouse));
					}
					if (IsOutwardBondedWarehousingEnabled && !HasAnInvoiceLineMarkedForBondedWarehousing)
					{
						messageErrors.Append(BaseJobDeclaration.AtLeastOneInvoiceLineMarkedForBondedWarehousingIsRequired(declaration.TermNameForBondedWarehouse));
					}
					if (HasAnInvoiceLineMarkedForBondedWarehousingWithoutAnOwnerProduct)
					{
						messageErrors.Append(BaseJobDeclaration.InvoiceLineMarkedForBondedWarehousingChangeOfOwnershipRequiresAnOwnerProduct(declaration.TermNameForBondedWarehouse));
					}
				}
				if (checkQuantity)
				{
					if (declaration.IsInvoiceQuantityRequiredForBondedWarehouse && HasAnInvoiceLineMarkedForBondedWarehousingWithoutAnInvoiceQuantityOrUnit)
					{
						messageErrors.Append(BaseJobDeclaration.InvoiceLineMarkedForBondedWarehousingRequiresAnInvoiceQuantityAndUnit(declaration.TermNameForBondedWarehouse));
					}
					if (declaration.IsBondedWhsQuantityRequiredForBondedWarehouse && HasAnInvoiceLineMarkedForBondedWarehousingWithoutACountableQuantityOrUnit)
					{
						messageErrors.Append(BaseJobDeclaration.InvoiceLineMarkedForBondedWarehousingRequiresACountableQuantityAndUnit(declaration.TermNameForBondedWarehouse));
					}
					if (declaration.IsAllocatedQuantityRequiredForBondedWarehouse && HasAnInvoiceLineMarkedForAllocatedInventoryWithoutACountableQuantity)
					{
						messageErrors.Append(BaseJobDeclaration.InvoiceLineMarkedForAllocatedInventoryRequiresACountableQuantity(declaration.TermNameForBondedWarehouse));
					}
				}
				if (checkEntryDetails && HasAnInvoiceLineMarkedForBondedWarehousingWithoutEntryDetailsAndNonAssembledProduct)
				{
					messageErrors.Append(GetInvoiceLineMarkedForBondedWarehousingRequiresEntryDetailsMessage());
				}
				AddExtraRequiredFieldsMessageError(messageErrors, checkProduct, checkQuantity, checkEntryDetails);
			}
			return messageErrors.ToStringWithNewLineBetweenAppends();
		}

		void CheckWarehouseAddress(ZStringBuilder messageErrors, BaseJobDeclaration declaration, OrgAddress warehouseAddress, Func<string, string, string> getWarehouseMessage, Func<string, string, string, string> getWarehouseMessageCountry)
		{
			if (warehouseAddress == null)
			{
				var importer = declaration.Importer;
				messageErrors.Append(getWarehouseMessage(importer == null ? ZString.Empty : importer.OH_Code, EntryHeaderDescriptiveMenuItemText));
			}
			else if (declaration.IsWarehouseAddressOutsideOfDeclarationCountry(warehouseAddress))
			{
				var importer = declaration.Importer;
				var country = declaration.Country;
				messageErrors.Append(getWarehouseMessageCountry(importer == null ? ZString.Empty : importer.OH_Code, EntryHeaderDescriptiveMenuItemText, country == null ? ZString.Empty : country.RN_DescMultilingual));
			}
		}

		public OrgAddress GetOutOfWarehouseAddress()
		{
			var declaration = this.Declaration;
			return declaration.SupportMultipleWarehouseEntry ? EntryInstruction?.Warehouse : declaration.WarehouseAddress;
		}

		public OrgAddress GetIntoWarehouseAddress()
		{
			var declaration = this.Declaration;
			return declaration.SupportMultipleWarehouseEntry ? EntryInstruction?.Warehouse2 : declaration.WarehouseAddress;
		}

		protected virtual string GetInvoiceLineMarkedForBondedWarehousingRequiresEntryDetailsMessage()
		{
			return BaseJobDeclaration.InvoiceLineMarkedForBondedWarehousingRequiresEntryDetails(Declaration.TermNameForBondedWarehouse);
		}

		public BondedWarehousingHelper BondedWarehousingHelper
		{
			get { return Declaration?.BondedWarehousingHelper; }
		}

		public InventoryAutomationAction GetInventoryAutomationAction()
		{
			var automationAction = InventoryAutomationAction.Inward;
			if (IsChangeOfRegimeWarehousingEnabled)
			{
				automationAction = InventoryAutomationAction.ChangeOfRegime;
			}
			else if (IsChangeOfOwnershipBondedWarehousingEnabled)
			{
				automationAction = InventoryAutomationAction.ChangeOfOwnership;
			}
			else if (IsOutOfWarehouseWarehousing)
			{
				automationAction = InventoryAutomationAction.Outward;
			}
			return automationAction;
		}

		public bool IsIntoWarehouseWarehousing
		{
			get
			{
				var declaration = this.Declaration;
				return declaration.SupportMultipleWarehouseEntry ? (EntryInstruction?.IsIntoRegime ?? false) : declaration.IsInwardBondedWarehousingEnabled;
			}
		}

		public bool IsOutOfWarehouseWarehousing
		{
			get
			{
				var declaration = this.Declaration;
				return declaration.SupportMultipleWarehouseEntry ? (EntryInstruction?.IsOutOfRegime ?? false) : declaration.IsOutwardBondedWarehousingEnabled;
			}
		}

		public bool HasAnInvoiceLineMarkedForBondedWarehousingWithEntryDetails
		{
			get
			{
				if (hasAnInvoiceLineMarkedForBondedWarehousingWithEntryDetailsCached == null)
				{
					hasAnInvoiceLineMarkedForBondedWarehousingWithEntryDetailsCached = new CachedProperty<bool>(Factory, GetHasAnInvoiceLineMarkedForBondedWarehousingWithEntryDetails);
				}
				return hasAnInvoiceLineMarkedForBondedWarehousingWithEntryDetailsCached.Value;
			}
		}
		CachedProperty<bool> hasAnInvoiceLineMarkedForBondedWarehousingWithEntryDetailsCached;

		bool GetHasAnInvoiceLineMarkedForBondedWarehousingWithEntryDetails()
		{
			return SupportsBondedWarehousing && InvoiceLines.OfType<BaseJobComInvoiceLine>().Any(x => BondedWarehousingHelper.IsMarkedForBondedWarehousing(x) && BondedWarehousingHelper.HasBondedWarehouseEntryDetails(x, IsIntoWarehouseWarehousing, IsOutOfWarehouseWarehousing, IsChangeOfOwnershipWarehousing));
		}

		public bool HasAnInvoiceLineMarkedForBondedWarehousingWithoutACountableQuantityOrUnit
		{
			get
			{
				if (hasAnInvoiceLineMarkedForBondedWarehousingWithoutACountableQuantityOrUnitCached == null)
				{
					hasAnInvoiceLineMarkedForBondedWarehousingWithoutACountableQuantityOrUnitCached = new CachedProperty<bool>(Factory, GetHasAnInvoiceLineMarkedForBondedWarehousingWithoutACountableQuantityOrUnit);
				}
				return hasAnInvoiceLineMarkedForBondedWarehousingWithoutACountableQuantityOrUnitCached.Value;
			}
		}
		CachedProperty<bool> hasAnInvoiceLineMarkedForBondedWarehousingWithoutACountableQuantityOrUnitCached;

		bool GetHasAnInvoiceLineMarkedForBondedWarehousingWithoutACountableQuantityOrUnit()
		{
			var result = SupportsBondedWarehousing;
			if (result)
			{
				result = InvoiceLines.OfType<BaseJobComInvoiceLine>().Any(x => (x.JI_BondedWhsQuantity.IsEmpty || x.JI_BondedWhsUnitQty.IsEmpty) && BondedWarehousingHelper.IsMarkedForBondedWarehousing(x));
			}
			return result;
		}

		public bool HasAnInvoiceLineMarkedForBondedWarehousingWithoutAnInvoiceQuantityOrUnit
		{
			get
			{
				if (hasAnInvoiceLineMarkedForBondedWarehousingWithoutAnInvoiceQuantityOrUnitCached == null)
				{
					hasAnInvoiceLineMarkedForBondedWarehousingWithoutAnInvoiceQuantityOrUnitCached = new CachedProperty<bool>(Factory, GetHasAnInvoiceLineMarkedForBondedWarehousingWithoutAnInvoiceQuantityOrUnit);
				}
				return hasAnInvoiceLineMarkedForBondedWarehousingWithoutAnInvoiceQuantityOrUnitCached.Value;
			}
		}
		CachedProperty<bool> hasAnInvoiceLineMarkedForBondedWarehousingWithoutAnInvoiceQuantityOrUnitCached;

		bool GetHasAnInvoiceLineMarkedForBondedWarehousingWithoutAnInvoiceQuantityOrUnit()
		{
			var result = SupportsBondedWarehousing;
			if (result)
			{
				result = InvoiceLines.OfType<BaseJobComInvoiceLine>().Any(x => (x.JI_InvoiceQuantity.IsEmpty || x.JI_InvoiceUQ.IsEmpty) && BondedWarehousingHelper.IsMarkedForBondedWarehousing(x));
			}
			return result;
		}

		public bool HasAnInvoiceLineMarkedForBondedWarehousingWithoutEntryDetailsAndNonAssembledProduct
		{
			get
			{
				if (hasAnInvoiceLineMarkedForBondedWarehousingWithoutEntryDetailsAndNonAssembledProductCached == null)
				{
					hasAnInvoiceLineMarkedForBondedWarehousingWithoutEntryDetailsAndNonAssembledProductCached = new CachedProperty<bool>(Factory, GetHasAnInvoiceLineMarkedForBondedWarehousingWithoutEntryDetailsAndNonAssembledProduct);
				}
				return hasAnInvoiceLineMarkedForBondedWarehousingWithoutEntryDetailsAndNonAssembledProductCached.Value;
			}
		}
		CachedProperty<bool> hasAnInvoiceLineMarkedForBondedWarehousingWithoutEntryDetailsAndNonAssembledProductCached;

		bool GetHasAnInvoiceLineMarkedForBondedWarehousingWithoutEntryDetailsAndNonAssembledProduct() => SupportsBondedWarehousing && InvoiceLines.OfType<BaseJobComInvoiceLine>()
			.Any(x => BondedWarehousingHelper.IsMarkedForBondedWarehousing(x)
				&& ContainsNonAssembledProduct(x)
				&& !BondedWarehousingHelper.HasBondedWarehouseEntryDetails(x, IsIntoWarehouseWarehousing, IsOutOfWarehouseWarehousing, IsChangeOfOwnershipWarehousing));

		public bool HasAnInvoiceLineMarkedForAllocatedInventoryWithoutACountableQuantity
		{
			get
			{
				if (hasAnInvoiceLineMarkedForAllocatedInventoryWithoutACountableQuantityCached == null)
				{
					hasAnInvoiceLineMarkedForAllocatedInventoryWithoutACountableQuantityCached = new CachedProperty<bool>(Factory, GetHasAnInvoiceLineMarkedForAllocatedInventoryWithoutACountableQuantity);
				}
				return hasAnInvoiceLineMarkedForAllocatedInventoryWithoutACountableQuantityCached.Value;
			}
		}
		CachedProperty<bool> hasAnInvoiceLineMarkedForAllocatedInventoryWithoutACountableQuantityCached;

		bool GetHasAnInvoiceLineMarkedForAllocatedInventoryWithoutACountableQuantity()
		{
			var result = SupportsBondedWarehousing;
			if (result)
			{
				result = InvoiceLines.OfType<BaseJobComInvoiceLine>().Any(x => x.Part != null && x.ComponentInventoryCollection.Cast<JobComInvLineComponentInventory>().Any(x => x.Inventory != null && x.JIV_QuantityToDraw <= 0) && BondedWarehousingHelper.IsMarkedForBondedWarehousing(x));
			}
			return result;
		}

		bool ContainsNonAssembledProduct(BaseJobComInvoiceLine invoiceLine)
		{
			var result = true;
			if (invoiceLine.Part is OrgSupplierPart product)
			{
				result = product.BillOfMaterials.Count == 0;
			}
			return result;
		}

		protected virtual void AddExtraRequiredFieldsMessageError(ZStringBuilder messageErrors, bool checkProduct = true, bool checkQuantity = true, bool checkEntryDetails = true)
		{
		}

		#region Exposed Customs Charge Fields

		public ZDecimal Duty
		{
			get { return Charges.GetAmount(DutyCode); }
			set
			{
				if (!string.IsNullOrEmpty(DutyCode))
				{
					Charges[DutyCode].C1_ChargeAmount = value;
				}
				DutyInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo DutyInfo
		{
			get { return GetZPropertyInfo(Schema.Duty); }
		}

		public string DutyCode => GetDutyCode();

		protected virtual string GetDutyCode()
		{
			return EntryChargeTypeList?.DutyCode ?? string.Empty;
		}

		public ZDecimal VAT
		{
			get { return Charges.GetAmount(TaxCode); }
			set
			{
				if (!string.IsNullOrEmpty(TaxCode))
				{
					Charges[TaxCode].C1_ChargeAmount = value;
				}
				VATInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo VATInfo
		{
			get { return GetZPropertyInfo(Schema.VAT); }
		}

		public string TaxCode => GetTaxCode();

		protected virtual string GetTaxCode()
		{
			return EntryChargeTypeList?.TaxCode ?? string.Empty;
		}

		#endregion

		public ZString MovementReferenceNumberEntryStatus => MovementReferenceCusEntryNumber?.CE_EntryStatus ?? ZString.Empty;

		public ZPropertyInfo MovementReferenceNumberEntryStatusInfo => GetZPropertyInfo(Schema.MovementReferenceNumberEntryStatus);

		public ZDateTime MovementReferenceNumberIssueDate => MovementReferenceCusEntryNumber?.CE_IssueDate ?? ZDateTime.Empty;

		public ZPropertyInfo MovementReferenceNumberIssueDateInfo => GetZPropertyInfo(Schema.MovementReferenceNumberIssueDate);

		public ZDateTime MovementReferenceNumberExpiryDate => MovementReferenceCusEntryNumber?.CE_ExpiryDate ?? ZDateTime.Empty;

		public virtual ZString MovementReferenceNumber => MovementReferenceCusEntryNumber?.CE_EntryNum ?? ZString.Empty;

		public ZPropertyInfo MovementReferenceNumberInfo => GetZPropertyInfo(Schema.MovementReferenceNumber);

		public void MovementReferenceNumberSetter(ZString mrn, ZDateTime? issueDate = null, ZString? entryStatus = null, ZDateTime? expiryDate = null)
		{
			var result = CusEntryNumber.LoadOrCreate(this, CusEntryNumberTypes.Standard.MovementReferenceNumber, CountryCode);
			result.CE_EntryIsSystemGenerated = true;
			result.CE_EntryNum = mrn;
			if (issueDate.HasValue)
			{
				result.CE_IssueDate = issueDate.Value;
			}
			if (entryStatus.HasValue)
			{
				result.CE_EntryStatus = entryStatus.Value;
			}
			if (expiryDate.HasValue)
			{
				result.CE_ExpiryDate = expiryDate.Value;
			}
		}

		protected void SetEntryNumber(ZString entryType, ZString entryNumber, ZDateTime issueDate)
		{
			var setItem = CusEntryNumber.LoadOrCreate(this, entryType, CountryCode);
			setItem.CE_EntryIsSystemGenerated = true;
			setItem.CE_EntryNum = entryNumber;
			setItem.CE_IssueDate = issueDate;
		}

		/// <summary>
		/// Loads an existing CEN, may return null.
		/// </summary>
		protected CusEntryNumber MovementReferenceCusEntryNumber
		{
			get { return CusEntryNumber.Load(this, CusEntryNumberTypes.Standard.MovementReferenceNumber, CountryCode); }
		}

		public ZString DeclarationUCR => Factory.GetValue(ref declarationUCRCached, () =>
		{
			var ucr = Factory.LoadTop1<CusEntryNumber>(EntryNumberQueryGenerator.GetEntryNumberByEntryTypeQuery(PK, CountryCode, CusEntryNumberTypes.Standard.UniqueConsignementReference, IsInDatabase));
			return ucr != null ? ucr.CE_EntryNum.Split('/')[0] : ZString.Empty;
		});
		CachedProperty<ZString> declarationUCRCached;

		public ZString DeclarationUCRPartSuffix
		{
			get
			{
				var ucr = Factory.LoadTop1<CusEntryNumber>(EntryNumberQueryGenerator.GetEntryNumberByEntryTypeQuery(PK, CountryCode, CusEntryNumberTypes.Standard.UniqueConsignementReference, IsInDatabase));
				if (ucr != null)
				{
					ZString[] results = ucr.CE_EntryNum.Split('/');
					return results.Length > 1 ? results[1] : ZString.Empty;
				}

				return "";
			}
		}

		public void LoadOrCreateUCRNumber(ZString uCRNumber)
		{
			var ucr = Factory.LoadTop1<CusEntryNumber>(EntryNumberQueryGenerator.GetEntryNumberByEntryTypeQuery(PK, CountryCode, CusEntryNumberTypes.Standard.UniqueConsignementReference, IsInDatabase));
			if (ucr == null)
			{
				ucr = Factory.New<CusEntryNumber>();
				ucr.CE_ParentID = PK;
				ucr.CE_ParentTable = CusEntryHeaderSchema.Constants.TableName;
				ucr.CE_EntryType = CusEntryNumberTypes.Standard.UniqueConsignementReference;
				ucr.CE_RN_NKCountryCode = CountryCode;
				ucr.CE_EntryIsSystemGenerated = false;
				ucr.CE_Category = "CUS";
			}
			ucr.CE_EntryNum = uCRNumber;
		}

		public ZString InvoiceEffectiveUCR => Factory.GetValue(ref getUCRIfAllAreSameCached, () => this.InvoiceHeaders.AsEnumerable().Select(x => x.EffectiveUCR).SameOrDefault());
		CachedProperty<ZString> getUCRIfAllAreSameCached;

		//BasePackage does not effect Merge
		public ActiveBusinessObjectCollection<BasePackage> Packages
		{
			get
			{
				if (packagesCached == null)
				{
					packagesCached = new CachedProperty<ActiveBusinessObjectCollection<BasePackage>>(Factory, delegate
					{
						ActiveBusinessObjectCollection<BasePackage> result = new ActiveBusinessObjectCollection<BasePackage>(Factory, new AdhocCollectionRelationship(typeof(BasePackage)));
						foreach (Bill houseBill in Bills)
						{
							foreach (BasePackingGroup packingGroup in houseBill.PackingGroups)
							{
								result.AddRange(packingGroup.Packages);
							}
						}
						return result;
					});
				}
				return packagesCached.Value;
			}
		}
		CachedProperty<ActiveBusinessObjectCollection<BasePackage>> packagesCached;

		public Money TotalTAndI
		{
			get
			{
				Money result = Money.Empty;
				foreach (CusEntryLine entryLine in MergedLines)
				{
					result = CurrencyConverter.Add(result, entryLine.TAndI);
				}
				return result;
			}
		}

		public virtual ZDecimal TotalDutyAmount
		{
			get
			{
				ZDecimal result = 0m;
				foreach (CusEntryLine entryLine in MergedLines)
				{
					result += entryLine.DutyAmount;
				}
				return result;
			}
		}

		#region CIF
		public Money CIF
		{
			get
			{
				if (cIFCached == null)
				{
					cIFCached = new CachedProperty<Money>(Factory, GetCIF);
				}
				return cIFCached.Value;
			}
		}
		CachedProperty<Money> cIFCached;

		protected virtual Money GetCIF()
		{
			Money result = CurrencyConverter.Add(FOB, OverseasFreight);
			return CurrencyConverter.Add(result, OverseasInsurance);
		}

		public RefCurrency LocalCurrency
		{
			get { return RefCurrency.LoadFromCurrencyCode(Factory, BaseJobDeclaration.GetLocalCurrencyCodeFor(Declaration)) ?? GlbCompany.CurrentCompany.Country.LocalCurrency; }
		}

		public ZString LocalCurrencyCode
		{
			get { return BaseJobDeclaration.GetLocalCurrencyCodeFor(Declaration); }
		}

		public static RefCurrency GetLocalCurrencyFor(CusEntryHeader header)
		{
			return (header != null) ? header.LocalCurrency : RefCurrency.LoadFromCurrencyCode(GlbCompany.CurrentCompany.Factory, GlbCompany.CurrentCompany.Country.RN_RX_NKLocalCurrency);
		}

		public Money CIFInLocalCurrency
		{
			get { return CurrencyConverter.ConvertExact(CIF, LocalCurrency); }
		}

		public Money FOB
		{
			get
			{
				if (fOBCached == null)
				{
					fOBCached = new CachedProperty<Money>(Factory, GetFOB);
				}
				return fOBCached.Value;
			}
		}
		CachedProperty<Money> fOBCached;

		protected virtual Money GetFOB()
		{
			Money result = Money.Empty;
			foreach (CusEntryLine cusLine in MergedLines)
			{
				result = CurrencyConverter.Add(result, cusLine.FOB);
			}
			return result;
		}

		public Money FOBInLocalCurrency
		{
			get
			{
				if (fOBInLocalCurrencyCached == null)
				{
					fOBInLocalCurrencyCached = new CachedProperty<Money>(Factory, GetFOBInLocalCurrency);
				}
				return fOBInLocalCurrencyCached.Value;
			}
		}
		CachedProperty<Money> fOBInLocalCurrencyCached;

		protected virtual Money GetFOBInLocalCurrency()
		{
			Money result = Money.Empty;
			foreach (CusEntryLine cusLine in MergedLines)
			{
				result = CurrencyConverter.Add(result, cusLine.FOBInLocalCurrency);
			}
			return result;
		}

		public virtual Money OverseasFreight
		{
			get
			{
				Money result = Money.Empty;
				foreach (CusEntryLine cusLine in MergedLines)
				{
					result = CurrencyConverter.Add(result, cusLine.OverseasFreight);
				}
				return result;
			}
		}

		public virtual Money OverseasInsurance
		{
			get
			{
				Money result = Money.Empty;
				foreach (CusEntryLine cusLine in MergedLines)
				{
					result = CurrencyConverter.Add(result, cusLine.OverseasInsurance);
				}
				return RoundOverseasInsurance(result);
			}
		}

		protected virtual Money RoundOverseasInsurance(Money overseasInsurance)
		{
			return overseasInsurance.FuzzyRoundDown(2);
		}

		#endregion

		#region ClearanceDate
		public
#if DEBUG
		virtual
#endif
		ZDateTime ClearanceDate
		{
			get
			{
				var result = ZDateTime.Empty;
				var query = new ZQuery();
				query.AddToFilter(StmALogSchema.SL_Parent, PK);
				query.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.CustomsCleared.Code);
				var entryLog = Factory.LoadTop1<StmALog>(query);
				if (entryLog != null)
				{
					result = entryLog.SL_EventTime;
				}
				return result;
			}
		}

		public ZPropertyInfo ClearanceDateInfo
		{
			get { return GetZPropertyInfo(Schema.ClearanceDate); }
		}

		public virtual ZDateTime DeclarationDate
		{
			get
			{
				ZDateTime result = ClearanceDate;
				if (!result.IsValid)
				{
					result = Declaration.JE_EntrySubmittedDate;
				}

				return result;
			}
		}
		public ZPropertyInfo DeclarationDateInfo
		{
			get { return GetZPropertyInfo(Schema.DeclarationDate); }
		}

		#endregion

		#region IsActive
		public virtual bool IsActive
		{
			get { return !IsDeleted; }
			set
			{
				if (value)
				{
					if (!IsActive)
					{
						ErrorReporter.ReportOnce("CannotReActivateDeactivatedHeader", "Cannot reactivate a previously deactivated entry header");
					}
				}
				else
				{
					if (!IsDeleted && !HasBeenLodgedAtCustoms && !IsWaitingForResponse)
					{
						Delete();
					}
					else
					{
						if (Globals.IsTest && CountryCode != "ER" && CountryCode != "AI")
						{
							throw new NotSupportedException("This entry is being deactivated, but has an active Customs transaction or deleted already");
						}
					}
				}
			}
		}

		public virtual bool HasTransactionsWithCustoms
		{
			get { return Messages.Count > 0; }
		}

		public virtual bool IsWaitingForResponse
		{
			get { return Messages.IsWaitingForAResponse; }
		}

		public virtual bool HasBeenLodgedAtCustoms
		{
			get { return !EntryNumber.IsEmpty; }
		}

		public virtual bool NeedToMaintainLinesDuringMerge
		{
			get { return HasBeenLodgedAtCustoms; }
		}

		/// <summary>
		/// AU Customs disallows nature changes after an entry is lodged
		/// </summary>
		public bool HasNonAmendableChanges
		{
			get { return Declaration != null && !Declaration.IsCustomsHeaderAmendmentATotalReplacement && HasBeenLodgedAtCustoms && HasNonAmendableChangesCore && !HasBeenWithdrawn; }
		}

		protected virtual bool HasNonAmendableChangesCore
		{
			get { return false; }
		}

		public virtual bool HasBeenWithdrawn
		{
			get
			{
				if (Globals.IsTest && CountryCode != "ER" && CountryCode != "AI")
				{
					throw new NotSupportedException("Implement this in each country. If you are testing base behaviour, please use mock instead");
				}
				return false;
			}
		}

#pragma warning disable IDE0001 // Simplify Names
		/// <summary>
		/// <p>
		/// Defines whether instances of <see cref="Enterprise.Customs.Common.CusEntryNumber"/>
		/// related to this header should be included or excluded by <see cref="ICusEntryNumFilterProvider.ValidCusEntryNumFilter"/>.
		/// </p>
		/// <p>
		/// Usually it excludes instances of <see cref="Enterprise.Customs.Common.CusEntryNumber"/> related to cancelled headers.
		/// </p>
		/// </summary>
#pragma warning restore IDE0001 // Simplify Names
		public bool ShouldBeIncludedInCusEntryNumberFilter
		{
			get { return ShouldBeIncludedInCusEntryNumberFilterCore(); }
		}

		protected virtual bool ShouldBeIncludedInCusEntryNumberFilterCore()
		{
			return CH_EntryStatus != Enterprise.Customs.Common.Shared.EntryStatusList.Codes.Cancelled;
		}

		/// <summary>
		/// Usually, entries are attached to invoice and invoice lines and at the end of merge,
		/// if an entry does not have an invoice line, it is right to deactivate it.
		/// But US inbond entries are created even when no invoice lines are attached.
		/// </summary>
		protected internal virtual bool ShouldDeactivateAfterMergeIsDoneAsNoInvoiceLinesLinked
		{
			get { return true; }
		}

		public bool ShouldCompletelyReassignNumbers => ShouldCompletelyReassignNumbersCore;
		protected virtual bool ShouldCompletelyReassignNumbersCore => Declaration.IsCustomsLineAmendmentATotalReplacement || !HasBeenLodgedAtCustoms;

		#endregion

		#region DateForDutyRate
		public ZDate DateForDutyRate
		{
			get { return Declaration != null ? Declaration.DateForDutyRate : ZDate.Today; }
		}
		#endregion

		#region Suppliers
		public virtual OrgHeaderCollection Suppliers
		{
			get
			{
				OrgHeaderCollection coll = new OrgHeaderCollection(this.Factory);
				foreach (BaseJobComInvoiceHeader invoiceHeader in InvoiceHeaders)
				{
					if (!invoiceHeader.IsDeleted && invoiceHeader.Supplier != null)
					{
						if (!coll.Contains(invoiceHeader.Supplier.PK))
						{
							coll.Add(invoiceHeader.Supplier);
						}
					}
				}
				return coll;
			}
		}
		#endregion

		#region IsMultiSupplier
		public bool IsMultiSupplier
		{
			get { return Suppliers.Count > 1; }
		}
		#endregion

		public virtual bool IsMultiInvoiceCurrency
		{
			get
			{
				ZGuid currencyGuid = ZGuid.Empty;
				RefCurrency refCurrency;

				foreach (BaseJobComInvoiceHeader invoiceHeader in InvoiceHeaders)
				{
					refCurrency = RefCurrency.LoadFromCurrencyCode(Factory, invoiceHeader.JZ_RX_NKInvoice_Currency);
					if (refCurrency != null)
					{
						if (currencyGuid.IsEmpty)
						{
							currencyGuid = refCurrency.PK;
						}
						else if (currencyGuid != refCurrency.PK)
						{
							return true;
						}
					}
				}
				return false;
			}
		}

		#region InvoiceHeaders

		public BaseJobComInvoiceHeader[] InvoiceHeaders
		{
			get
			{
				if (IsInProcessOfMerging)
				{
					ErrorReporter.ReportOnce("InvoiceHeaders.CusEntryHeader is accessed while it is merging", "InvoiceHeaders.CusEntryHeader is accessed while it is merging");
				}

				if (invoiceHeaders == null)
				{
					invoiceHeaders = GetInvoiceHeaders();
				}
				return invoiceHeaders;
			}
		}
		BaseJobComInvoiceHeader[] invoiceHeaders;

		public void ResetInvoiceHeadersAndLines()
		{
			invoiceHeaders = null;
			invoiceLines = null;
		}

		protected virtual BaseJobComInvoiceHeader[] GetInvoiceHeaders()
		{
			ArrayList result = new ArrayList();
			foreach (CusEntryLine mergedLine in MergedLines)
			{
				foreach (BaseJobComInvoiceLine line in mergedLine.InvoiceLines)
				{
					var invoiceHeader = line.InvoiceHeader;
					if (invoiceHeader != null && !result.Contains(invoiceHeader))
					{
						result.Add(invoiceHeader);
					}
				}
			}

			Type arrayType = (result.Count > 0) ? result[0].GetType() : BaseJobComInvoiceHeader.TypeDecider.GetTypeForNew();
			return (BaseJobComInvoiceHeader[])result.ToArray(arrayType);
		}

		#endregion

		#region Group Invoices

		public ReadOnlyCollection<BaseJobComInvoiceGroupHeader> GroupInvoices
		{
			get
			{
				if (fGroupInvoices == null)
				{
					CheckNotInProcessOfMerging();

					List<BaseJobComInvoiceGroupHeader> result = new List<BaseJobComInvoiceGroupHeader>();
					foreach (BaseJobComInvoiceHeader invoice in InvoiceHeaders)
					{
						foreach (BaseJobComInvoiceGroupHeader groupInvoice in invoice.AllGroupInvoices)
						{
							if (!result.Contains(groupInvoice))
							{
								result.Add(groupInvoice);
							}
						}
					}

					fGroupInvoices = new ReadOnlyCollection<BaseJobComInvoiceGroupHeader>(result);
				}
				return fGroupInvoices;
			}
		}

		ReadOnlyCollection<BaseJobComInvoiceGroupHeader> fGroupInvoices;

		#endregion

		#region TransactionValue
		public ZDecimal TransactionValue
		{
			get { return TransactionValueCore; }
		}

		protected virtual ZDecimal TransactionValueCore
		{
			get
			{
				ZDecimal result = 0m;
				foreach (BaseJobComInvoiceHeader header in InvoiceHeaders)
				{
					if (header.Invoice_Currency != null)
					{
						result += header.InvoiceLineTotalInLocalCurrency;
					}
				}
				return result;
			}
		}
		#endregion

		#region CustomsValue
		public virtual ZDecimal CustomsValue
		{
			get
			{
				if (!isCustomsValueCalculated)
				{
					CheckNotInProcessOfMerging();

					isCustomsValueCalculated = true;
					fCustomsValue = 0m;
					foreach (CusEntryLine entryLine in MergedLines)
					{
						fCustomsValue += entryLine.CL_CustomsValue;
					}
				}
				return fCustomsValue;
			}
		}
		ZDecimal fCustomsValue;
		bool isCustomsValueCalculated;

		public void ResetIsCustomsValueCalculated()
		{
			isCustomsValueCalculated = false;
		}

		public ZPropertyInfo CustomsValueInfo
		{
			get { return GetZPropertyInfo(Schema.CustomsValue); }
		}

		void CheckNotInProcessOfMerging()
		{
			if (IsInProcessOfMerging)
			{
				throw new InvalidOperationException("You should not access this property or method now as system is still merging lines");
			}
		}

		#endregion

		#region VAT/GST Value
		public virtual ZDecimal ValueForVAT
		{
			get
			{
				if (!isValueForVATCalculated)
				{
					CheckNotInProcessOfMerging();

					isValueForVATCalculated = true;
					fValueForVAT = 0m;
					foreach (CusEntryLine entryLine in MergedLines)
					{
						fValueForVAT += entryLine.CL_ValueForVAT;
					}
				}
				return fValueForVAT;
			}
		}
		ZDecimal fValueForVAT;
		bool isValueForVATCalculated;

		public void ResetIsValueForVATCalculated()
		{
			isValueForVATCalculated = false;
		}
		#endregion

		#region TotalAmountPayable
		public virtual ZDecimal TotalAmountPayable
		{
			get
			{
				return HasAnyConfirmedFeesOnAnyMergedLine
					? MergedLines.SelectMany(x => x.ConfirmedFees).Cast<CusEntryLineFee>().Sum(x => x.CF_ChargeAmount)
					: MergedLines.SelectMany(x => x.Fees.Cast<CusEntryLineFee>()).Where(x => x.CF_Source == CusEntryLineFeeSourceCodeList.Codes.CW1).Sum(x => x.CF_ChargeAmount);
			}
		}

		public ZPropertyInfo TotalAmountPayableInfo
		{
			get { return GetZPropertyInfo(Schema.TotalAmountPayable); }
		}
		#endregion

		#region RandomHeader
		public BaseJobComInvoiceHeader RandomHeader
		{
			get
			{
				if (fRandomHeader == null && MergedLines.Count > 0)
				{
					fRandomHeader = Factory.Load<BaseJobComInvoiceHeader>(MergedLines[0].RandomLine.JI_JZ);
				}
				return fRandomHeader == null || fRandomHeader.IsDeleted ? (BaseJobComInvoiceHeader)Factory.GetNull(BaseJobComInvoiceHeader.TypeDecider.GetTypeForCountryCode(CountryCode)) : fRandomHeader;
			}
		}
		BaseJobComInvoiceHeader fRandomHeader;

		internal void RefreshRandomHeader()
		{
			fRandomHeader = null;
		}

		public ZString CountryCode
		{
			get { return Declaration != null ? Declaration.CountryCode : GlbCompany.CurrentCompany.GC_RN_NKCountryCode; }
		}

		public virtual ZString GoodsTypeForDocumentFilter => ZString.Empty;

		#endregion

		#region MinimumLine
		public BaseJobComInvoiceLine MinimumLine
		{
			get
			{
				BaseJobComInvoiceLine result = null;
				foreach (CusEntryLine entryLine in MergedLines)
				{
					foreach (BaseJobComInvoiceLine invLine in entryLine.InvoiceLines)
					{
						if (result == null || InvoiceLineComparer.Compare(invLine, result) < 0)
						{
							result = invLine;
						}
					}
				}
				return result;
			}
		}
		#endregion

		#region GSTAmount
		public virtual ZDecimal GSTAmount
		{
			get
			{
				ZDecimal result = 0m;
				foreach (CusEntryLine entryLine in MergedLines)
				{
					result += entryLine.GSTVATAmount;
				}
				return result;
			}
		}

		public virtual ZPropertyInfo GSTAmountInfo
		{
			get { return GetZPropertyInfo(Schema.GSTAmount); }
		}
		#endregion

		#region CurrencyConverter

		public CurrencyConverter CurrencyConverter
		{
			get { return CurrencyConverterCore; }
		}

		protected virtual CurrencyConverter CurrencyConverterCore
		{
			get
			{
				CurrencyConverter result = RandomHeader.CurrencyConverter;

				if (RandomHeader.IsNull && Declaration != null)
				{
					result = new CurrencyConverterWithDataProvider(Factory, Declaration);
				}

				return result;
			}
		}

		#endregion

		#region GrossWeight
		public ZWeight GrossWeight
		{
			get
			{
				var calculator = WeightCalculator;
				return new ZWeight(calculator.Weight, calculator.UQ);
			}
		}
		#endregion

		#region WeightCalculator
		public WeightUQCalculator WeightCalculator
		{
			get { return GetWeightCalculator(); }
		}

		protected virtual WeightUQCalculator GetWeightCalculator()
		{
			return new WeightUQCalculator(this);
		}
		#endregion

		public ZDecimal TotalInvoiceLinesGrossWeightInKG => Factory.GetValue(ref totalInvoiceLinesGrossWeightInKGCached, () => InvoiceLines.Sum(x => x.GrossWeightInKG));
		CachedProperty<ZDecimal> totalInvoiceLinesGrossWeightInKGCached;

		#region Packages

		[BusinessObjectTestExclude]
		public virtual ZInt PackagesCount => Factory.GetValue(ref packagesCountCached, () =>
		{
			var result = ZInt.Zero;
			if (Declaration is BaseJobDeclaration declaration)
			{
				result = ShouldCalculatePackagesCountBasedOnLinesPackagesPivot ? CalculatePackagesAmountBasedOnLinesPackagesPivot() : CalculatePackagesAmountBasedOnInvoicesWithDeclarationFallback(declaration);
			}
			return result;
		});
		CachedProperty<ZInt> packagesCountCached;

		public ZPropertyInfo PackagesCountInfo
		{
			get { return GetZPropertyInfo(Schema.PackagesCount); }
		}

		protected virtual bool ShouldCalculatePackagesCountBasedOnLinesPackagesPivot => false;

		ZInt CalculatePackagesAmountBasedOnLinesPackagesPivot()
		{
			return InvoiceLines
				.SelectMany(x => x.PackagesPivot).Cast<InvoiceLinePackagePivot>()
				.Sum(x => x.CHC_NumberOfPacks);
		}

		ZInt CalculatePackagesAmountBasedOnInvoicesWithDeclarationFallback(BaseJobDeclaration declaration)
		{
			ZInt result = 0;
			BaseJobComInvoiceHeader[] headers = InvoiceHeaders;
			if (headers.Length > 0)
			{
				if (headers[0].JobComInvoiceLines.Count > 0 && headers[0].JobComInvoiceLines[0].IsGoingIntoBondedWarehouse)
				{
					foreach (BaseJobComInvoiceHeader invoiceHeader in headers)
					{
						result += invoiceHeader.PackagesBond;
					}
				}
				else
				{
					foreach (BaseJobComInvoiceHeader invoiceHeader in headers)
					{
						result += invoiceHeader.PackagesFreeStore;
					}
				}
			}
			if (result == 0 && !declaration.HasSplitEntries)
			{
				result = declaration.JE_TotalNoOfPacks;
			}
			return result;
		}

		#endregion

		#region CH_CustomsMessageRemarks
		public virtual ZString CH_CustomsMessageRemarks
		{
			get
			{
				return CustomsMessageRemarksNoteManager.Value;
			}
			set
			{
				CustomsMessageRemarksNoteManager.Value = value;
				Validation.ValidateCH_CustomsMessageRemarks();
				CH_CustomsMessageRemarksInfo.RefreshBinding();
			}
		}

		ProxiedNotePropertyManager CustomsMessageRemarksNoteManager
		{
			get { return customsMessageRemarksNoteManager ?? (customsMessageRemarksNoteManager = new ProxiedNotePropertyManager(this, PredefinedNoteTypes.Instance.CustomsMessageRemarks)); }
		}
		ProxiedNotePropertyManager customsMessageRemarksNoteManager;

		public ZPropertyInfo CH_CustomsMessageRemarksInfo
		{
			get { return GetZPropertyInfo(Schema.CH_CustomsMessageRemarks); }
		}

		public int CH_CustomsMessageRemarks_MaxLength
		{
			get { return CustomsMessageRemarksNoteManager.MaxLength; }
		}
		#endregion

		#region CH_CustomsDeliveryInstructions
		public virtual ZString CH_CustomsDeliveryInstructions
		{
			get
			{
				return CustomsDeliveryInstructionsNoteManager.Value;
			}
			set
			{
				CustomsDeliveryInstructionsNoteManager.Value = value;
				CheckMaximumLength(CH_CustomsDeliveryInstructionsInfo, value);
				CH_CustomsDeliveryInstructionsInfo.RefreshBinding();
			}
		}

		ProxiedNotePropertyManager CustomsDeliveryInstructionsNoteManager
		{
			get { return customsDeliveryInstructionsNoteManager ?? (customsDeliveryInstructionsNoteManager = new ProxiedNotePropertyManager(this, PredefinedNoteTypes.Instance.CustomsDeliveryInstructions)); }
		}
		ProxiedNotePropertyManager customsDeliveryInstructionsNoteManager;

		public ZPropertyInfo CH_CustomsDeliveryInstructionsInfo
		{
			get { return GetZPropertyInfo(Schema.CH_CustomsDeliveryInstructions); }
		}

		public int CH_CustomsDeliveryInstructions_MaxLength
		{
			get { return CustomsDeliveryInstructionsNoteManager.MaxLength; }
		}
		#endregion

		#region DeclarationReference

		public ZString DeclarationReference => Declaration?.JE_DeclarationReference ?? ZString.Empty;

		public ZPropertyInfo DeclarationReferenceInfo
		{
			get { return GetZPropertyInfo(Schema.DeclarationReference); }
		}

		#endregion

		#region CH_MessageTypeDescription

		public ZString CH_MessageTypeDescription
		{
			get { return Lookups.CH_MessageTypeList.GetDescriptionFromCode(CH_MessageType); }
		}

		public ZPropertyInfo CH_MessageTypeDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.CH_MessageTypeDescription); }
		}

		#endregion

		public virtual bool IsEntryStatusCleared => CustomsStatusAttributeHelper.IsStatusCleared(Factory, CH_EntryStatus, CountryCode, ZDateTime.Today);

		public virtual bool IsClearedEntry => IsEntryStatusCleared;

		#endregion

		#region LineComparer

		public class BaseCusEntryComparer : IComparer
		{
			public BaseCusEntryComparer()
			{
				lineComparer = GetInvoiceLineComparer();
			}

			#region IComparer Members

			public int Compare(object x, object y)
			{
				CusEntryHeader headerX = (CusEntryHeader)x;
				CusEntryHeader headerY = (CusEntryHeader)y;
				if (headerX == null && headerY != null)
				{
					return -1;
				}
				else if (headerX != null && headerY == null)
				{
					return 1;
				}
				else if (headerX == null && headerY == null)
				{
					return 0;
				}

				BaseJobComInvoiceLine lineX = headerX.MinimumLine;
				BaseJobComInvoiceLine lineY = headerY.MinimumLine;
				return lineComparer.Compare(lineX, lineY);
			}

			#endregion

			protected IComparer lineComparer;

			protected virtual IComparer GetInvoiceLineComparer()
			{
				return new BaseJobComInvoiceLine.LineComparer();
			}
		}

		IComparer fLineComparer;
		protected IComparer InvoiceLineComparer
		{
			get
			{
				if (fLineComparer == null)
				{
					fLineComparer = GetInvoiceLineComparer();
				}
				return fLineComparer;
			}
		}

		protected virtual IComparer GetInvoiceLineComparer()
		{
			return new BaseJobComInvoiceLine.LineComparer();
		}

		#endregion

		#region Entry Number Property and Support Code

		#region EntryNumber
		[ResourceStringData("9A47FC68-6503-4628-8CCA-430845D89862", Caption = "Entry Number")]
		[MaxLength(CusEntryNumber.Schema.CE_EntryNumMaxLength)]
		public virtual ZString EntryNumber
		{
			get
			{
				CusEntryNumber entryNumber = CusEntryNumber;
				return entryNumber == null ? ZString.Empty : entryNumber.CE_EntryNum;
			}
			set
			{
				if (EntryNumber != value)
				{
					if (value.IsEmpty)
					{
						ThrowAwayEntryNumber();
					}
					else
					{
						CreateCusEntryNumberIfNeeded();
						CusEntryNumber.CE_EntryNum = value;
					}
					if (!IsValidationSuspended)
					{
						Validation.ValidateEntryNumber();
					}
				}
				EntryNumberInfo.RefreshBinding();
			}
		}

		public void ThrowAwayEntryNumber()
		{
			if (CusEntryNumber != null)
			{
				CusEntryNumber.Delete();
			}
		}

		public bool IsEntryNumberGeneratedButNotSavedYet
		{
			get { return CusEntryNumber != null && !CusEntryNumber.IsInDatabase; }
		}

		public ZPropertyInfo EntryNumberInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.EntryNumber); }
		}

		protected virtual bool EntryNumber_ReadOnly
		{
			get { return true; }
		}

		public CusEntryNumber CusEntryNumber
		{
			get
			{
				if (cusEntryNumberCache == null)
				{
					cusEntryNumberCache = new CachedProperty<CusEntryNumber>(Factory, LoadCusEntryNumber);
				}
				return cusEntryNumberCache.Value;
			}
		}
		CachedProperty<CusEntryNumber> cusEntryNumberCache;

		protected void CreateCusEntryNumberIfNeeded()
		{
			if (CusEntryNumber == null)
			{
				CreateCusEntryNumber(); // Cached property will automatically pick this up
			}
		}

		protected virtual CusEntryNumber CreateCusEntryNumber()
		{
			var result = Factory.New<CusEntryNumber>();
			result.CE_EntryIsSystemGenerated = true;
			result.CE_ParentID = PK;
			result.CE_ParentTable = TableName;
			result.CE_RN_NKCountryCode = CountryCode;
			result.CE_EntryType = EntryNumberType;
			return result;
		}

		protected virtual CusEntryNumber LoadCusEntryNumber()
		{
			var query = new ZQuery(CusEntryNumSchema.CE_ParentID, PK);
			query.AddToFilter(CusEntryNumSchema.CE_EntryType, EntryNumberType);
			query.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, CountryCode);
			query.FetchOnlyFromLocalCache = !IsInDatabase;
			query.OrderBy = CusEntryNumSchema.CE_SystemCreateTimeUtc.Name;
			return Factory.LoadTop1<CusEntryNumber>(query);
		}

		protected virtual ZString EntryNumberType
		{
			get
			{
				var declaration = Declaration;
				var result = JobMessageTypeList.Codes.Import;

				if (declaration != null)
				{
					if (!CH_MessageType.IsEmpty)
					{
						result = CH_MessageType;
					}
					else if (!declaration.JE_MessageType.IsEmpty)
					{
						result = declaration.JE_MessageType;
					}
				}

				return result;
			}
		}

		#endregion

		#endregion

		#region Guarantee properties

		public bool HasConsumingGuaranteeProcedure
		{
			get
			{
				if (hasConsumingGuaranteeProcedureCached == null)
				{
					hasConsumingGuaranteeProcedureCached = new CachedProperty<bool>(Factory, () =>
					{
						return InvoiceLines.Any(x => x.HasGuaranteeConsumingProcedure);
					});
				}
				return hasConsumingGuaranteeProcedureCached.Value;
			}
		}

		CachedProperty<bool> hasConsumingGuaranteeProcedureCached;

		public bool HasReleasingGuaranteeProcedure
		{
			get
			{
				if (hasReleasingGuaranteeProcedureCached == null)
				{
					hasReleasingGuaranteeProcedureCached = new CachedProperty<bool>(Factory, () =>
					{
						return InvoiceLines.Any(x => x.HasGuaranteeReleasedProcedure);
					});
				}
				return hasReleasingGuaranteeProcedureCached.Value;
			}
		}
		CachedProperty<bool> hasReleasingGuaranteeProcedureCached;

		public bool IsGuaranteeConsumed => IsGuaranteeConsumedCore();

		protected virtual bool IsGuaranteeConsumedCore() => HasConsumingGuaranteeProcedure;

		public bool IsGuaranteeReleased => IsGuaranteeReleasedCore();

		protected virtual bool IsGuaranteeReleasedCore() => HasReleasingGuaranteeProcedure;

		public bool IsGuaranteeDeferredPaymentUsed
		{
			get
			{
				if (isGuaranteeDeferredPaymentUsedCached == null)
				{
					isGuaranteeDeferredPaymentUsedCached = new CachedProperty<bool>(Factory, () =>
					{
						return IsGuaranteeDeferredPaymentUsedCore();
					});
				}
				return isGuaranteeDeferredPaymentUsedCached.Value;
			}
		}

		CachedProperty<bool> isGuaranteeDeferredPaymentUsedCached;

		protected virtual bool IsGuaranteeDeferredPaymentUsedCore() => this.AllEntryLines.Cast<CusEntryLine>().Any(x => x.IsGuaranteeDeferredPaymentUsed);

		#endregion

		#region New methods

		public Type GetEntryLineType() => GetEntryLineTypeCore();
		protected virtual Type GetEntryLineTypeCore() => null;

		public bool HasMessageResponseError(short lineNumber)
		{
			return EntryLinesWithErrors.Contains(lineNumber);
		}

		public static CusEntryHeader LoadForBGMReference(BusinessObjectFactory factory, ZString bGMReference)
		{
			CusEntryHeader result = null;
			if (!bGMReference.IsEmpty)
			{
				ZQuery filter = new ZQuery(CusEntryHeaderSchema.CH_BGMReference, bGMReference);
				foreach (CusEntryHeader entryHeader in factory.Load(typeof(CusEntryHeader), filter))
				{
					if (entryHeader.Declaration.Branch.GB_GC == GlbCompany.CurrentCompany.PK)
					{
						result = entryHeader;
						break;
					}
				}
			}
			return result;
		}

		public static CusEntryHeader LoadForBGMReferenceAndEntryNumber(BusinessObjectFactory factory, ZString bGMReference, ZString entryNumber)
		{
			CusEntryHeader result = null;
			if (!bGMReference.IsEmpty)
			{
				ZQuery filter = new ZQuery(CusEntryHeaderSchema.CH_BGMReference, bGMReference);
				foreach (CusEntryHeader entryHeader in factory.Load(typeof(CusEntryHeader), filter))
				{
					if (entryHeader.Declaration.Branch.GB_GC == GlbCompany.CurrentCompany.PK)
					{
						if (entryNumber.IsEmpty || entryHeader.EntryNumber.IsEmpty || entryHeader.EntryNumber == entryNumber)
						{
							result = entryHeader;
							break;
						}
					}
				}
			}
			return result;
		}

		#region CH_Status post-setting logic

		protected virtual bool IsStatusChangingFromNotAcceptedToAccepted(string oldStatus, string newStatus)
		{
			return IsStatusChangingFromNotClearToClear(oldStatus, newStatus);
		}

		protected bool IsStatusChangingFromNotClearToClear(string oldStatus, string newStatus)
		{
			return !IsStatusClear(oldStatus) && IsStatusClear(newStatus);
		}

		protected virtual void UpdateWhenStatusIsAboutToChangeToClear(string oldStatus, string newStatus)
		{
			SaveHighestEntryLineNumber();
		}

		protected virtual bool IsStatusClear(string status)
		{
			return status == "CLR";
		}

		void SaveHighestEntryLineNumber()
		{
			var highestLineNumber = MaxLineNumberToAssignAsHighestLineNumber;

			if (CH_HighestLineNumber != highestLineNumber)
			{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				Declaration?.Logs?.AddNew(Events.EditedARecord, string.Format((NoResString)"Highest Line Number changed from {0} to {1}", CH_HighestLineNumber, highestLineNumber));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.

				CH_HighestLineNumber = highestLineNumber;
			}
		}

		protected virtual ZShort MaxLineNumberToAssignAsHighestLineNumber
		{
			get
			{
				var maxNumber = MergedLines.Count == 0 ? (ZShort)0 : MergedLines.Cast<CusEntryLine>().Select(x => x.CL_LineNumber).Max();
				return CH_HighestLineNumber < maxNumber ? maxNumber : CH_HighestLineNumber;
			}
		}

		protected void ChangeEntryLineStatusToDeletedWhenAmendmentCleared()
		{
			foreach (CusEntryLine entryLine in PendingDeletionEntryLines)
			{
				entryLine.CL_CustomsPostedStatus = EntryLineStatusList.Codes.Deleted;
			}
		}

		public virtual void PopulateEntrySubmittedDateIfRequired(ZDateTime? submittedDate = null)
		{
			if (CH_EntrySubmittedDate.IsEmpty)
			{
				CH_EntrySubmittedDate = submittedDate ?? ZDateTime.Now;

				if (Declaration != null && ShouldPopulateJE_EntrySubmittedDate)
				{
					Declaration.JE_EntrySubmittedDate = CH_EntrySubmittedDate;
				}
			}
		}

		public virtual bool ShouldPopulateJE_EntrySubmittedDate
		{
			get { return Declaration.JE_EntrySubmittedDate.IsEmpty && IsFormalEntry; }
		}

		#endregion

		#endregion

		public bool IsAllEntryLinesNotVatSuspended
		{
			get
			{
				if (isAllEntryLinesNotVatSuspendedCached == null)
				{
					isAllEntryLinesNotVatSuspendedCached = new CachedProperty<bool>(Factory, () =>
					{
						return InvoiceLines.All(x => !x.HasAnyProcedureWithSuspendedVat);
					});
				}
				return isAllEntryLinesNotVatSuspendedCached.Value;
			}
		}
		CachedProperty<bool> isAllEntryLinesNotVatSuspendedCached;

		public bool IsAllEntryLinesVatSuspended
		{
			get
			{
				if (isAllEntryLinesVatSuspendedCached == null)
				{
					isAllEntryLinesVatSuspendedCached = new CachedProperty<bool>(Factory, () =>
					{
						return InvoiceLines.All(x => x.HasAnyProcedureWithSuspendedVat);
					});
				}
				return isAllEntryLinesVatSuspendedCached.Value;
			}
		}
		CachedProperty<bool> isAllEntryLinesVatSuspendedCached;

		void IWarehouseIntegrationSupporter.UpdateHoldData(UniversalDataBuss.DataObjects.Universal.Shipment shipment, RecipientRoleType recipientRoleType)
		{
			// USXML was created at message sending.  It's about to be passed over to warehouse now that we're processing the response message, but update some data that's now available
			UpdateHeldUniversalShipmentDataForHoldWarehouseEntry(shipment, recipientRoleType);
		}

		protected virtual void UpdateHeldUniversalShipmentDataForHoldWarehouseEntry(UniversalDataBuss.DataObjects.Universal.Shipment shipment, RecipientRoleType recipientRoleType)
		{
			var entryXml = shipment.EntryHeaderCollection.SingleOrDefault(xe => xe.Reference.Value == CH_BGMReference);
			if (entryXml != null)
			{
				Logs.AddNew(Events.CustomsEntryStatus, "Updating held entry-warehouse data");
				var newEntryStatus = CH_EntryStatus;
				var newReleaseDate = CH_EntryReleaseDate;
				var newBondValidToDate = CH_BondValidToDate;
				var newMessageStatus = CH_Status;

				if (ShouldUpdateHeldUniversalShipmentEntryStatus(entryXml, newEntryStatus))
				{
					entryXml.EntryStatus.Code = newEntryStatus;
				}
				if (!newBondValidToDate.IsEmpty && (!entryXml.BondValidToDate.HasValue || entryXml.BondValidToDate.Value.IsEmpty))
				{
					entryXml.BondValidToDate = newBondValidToDate;
				}
				if (!newReleaseDate.IsEmpty && (!entryXml.EntryReleaseDate.HasValue || entryXml.EntryReleaseDate.Value.IsEmpty))
				{
					entryXml.EntryReleaseDate = newReleaseDate;
				}
				if (ShouldUpdateHeldUniversalShipmentMessageStatus(entryXml, newMessageStatus))
				{
					entryXml.MessageStatus.Code = newMessageStatus;
				}
			}
		}

		protected virtual bool ShouldUpdateHeldUniversalShipmentEntryStatus(UniversalDataBuss.DataObjects.Universal.Customs.EntryHeader entryXml, ZString newEntryStatus) => !newEntryStatus.IsEmpty && (!entryXml.EntryStatus.Code.HasValue || entryXml.EntryStatus.Code.Value.IsEmpty);

		protected virtual bool ShouldUpdateHeldUniversalShipmentMessageStatus(UniversalDataBuss.DataObjects.Universal.Customs.EntryHeader entryXml, ZString newMessageStatus) => !newMessageStatus.IsEmpty && (!entryXml.MessageStatus.Code.HasValue || entryXml.MessageStatus.Code.Value.IsEmpty);

		#region Implementation

		protected virtual ZQuery MessageFilter
		{
			get { return new ZQuery(EDIMessageSchema.EM_LinkUniqueID, PK); }
		}

		internal CusEntryHeaderValidation CusEntryHeaderValidation
		{
			get { return Validation; }
		}

		#region Response Message Error Management

		HybridDictionary fEntryLinesWithErrors;
		HybridDictionary EntryLinesWithErrors
		{
			get
			{
				if (fEntryLinesWithErrors == null)
				{
					fEntryLinesWithErrors = new HybridDictionary();
					foreach (short lineNumber in ErrorLineNumbersFromLastResponseMessage())
					{
						fEntryLinesWithErrors[lineNumber] = lineNumber;
					}
				}
				return fEntryLinesWithErrors;
			}
		}

		protected virtual short[] ErrorLineNumbersFromLastResponseMessage()
		{
			return Array.Empty<short>();
		}

		#endregion

		protected ZDecimal GetChargeFromHeaderCharges(string chargeType)
		{
			return Charges.GetAmount(chargeType);
		}

		protected ZDecimal GetTotalChargeFromHeaderCharges(string chargeType, bool includeLandedCostOnly)
		{
			return Charges.GetTotalAmount(chargeType, includeLandedCostOnly);
		}

		#endregion

		#region IDocumentSupportable Members

		public DocumentSupporter DocumentSupporter
		{
			get { return CreateNewDocumentSupporter(); }
		}

		protected virtual DocumentSupporter CreateNewDocumentSupporter()
		{
			return new CusEntryHeaderDocumentSupporter(this);
		}

		#endregion

		public ZString MessageTypeForDocumentFilter
		{
			get { return GetMessageTypeForDocumentFilter(); }
		}

		protected virtual ZString GetMessageTypeForDocumentFilter()
		{
			return CH_MessageType;
		}

		#region IWorkflowProvider Members

		IWorkflowProvider WorkflowProvider => Declaration;

		public IProcessHeaderCollection Workflows => WorkflowProvider?.Workflows;

		public ProcessTaskCollection WorkflowItems => WorkflowProvider?.WorkflowItems;

		IWorkflowInformationProvider IWorkflowProvider.GetWorkflowInformationProvider()
		{
			return WorkflowProvider?.GetWorkflowInformationProvider();
		}

		#region IWorkflowProviderCore Members

		ZString IWorkflowProviderCore.WorkflowType => WorkflowProvider?.WorkflowType ?? ZString.Empty;

		IColumnValueRanker IWorkflowProviderCore.GetTemplateSelectionCriteria()
		{
			return WorkflowProvider?.GetTemplateSelectionCriteria() ?? new ColumnValueRanker();
		}

		#endregion

		#endregion

		bool IStmNoteParentWithSystemNote.IsSystemNote(StmNote note)
		{
			return note != null && note.ST_Description == WarehouseConstants.UniversalHoldShipmentNoteDescription;
		}

		#region IWarehouseIntegrationSupporter Members

		bool IWarehouseIntegrationSupporter.SupportModificationState => false;

		bool IWarehouseIntegrationSupporter.HasManualWhsUpdate
		{
			get { return CH_HasManualWhsUpdate; }
			set { CH_HasManualWhsUpdate = value; }
		}

		void IWarehouseIntegrationSupporter.DoActionOnOutwardAccepted(BusinessObject job)
		{
		}

		Notes IWarehouseIntegrationSupporter.Notes => this.GetNotes();

		void IWarehouseIntegrationSupporter.DoActionOnOutwardCanceled(BusinessObject job)
		{
		}

		void IWarehouseIntegrationSupporter.PostUpdateBondedWarehouseInwardAction() { }
		void IWarehouseIntegrationSupporter.PostUpdateBondedWarehouseOutwardAction() { }
		void IWarehouseIntegrationSupporter.PostCancelBondedWarehouseInwardAction() { }
		void IWarehouseIntegrationSupporter.PostCancelBondedWarehouseOutwardAction() { }

		GlbCompany IWarehouseIntegrationSupporter.Company { get { return Declaration?.Company; } }
		ISendsMessagesToCustoms IWarehouseIntegrationSupporter.MessageInitiator { get { return Declaration?.MessageInitiator; } }
		bool IWarehouseIntegrationSupporter.IsActive { get { return IsWarehouseIntegrationActive; } }
		protected virtual bool IsWarehouseIntegrationActive { get { return Declaration?.IsWHSUniversalXMLActive ?? false; } }
		ZGuid IWarehouseIntegrationSupporter.ClientPK { get { return Declaration?.JE_OH_Importer ?? ZGuid.Empty; } }
		OrgAddress IWarehouseIntegrationSupporter.WarehouseAddress
		{
			get
			{
				OrgAddress result = null;
				var entryInstruction = EntryInstruction;
				if (entryInstruction != null)
				{
					result = entryInstruction.HasOutOfRegimeProcedure ? entryInstruction.Warehouse : entryInstruction.Warehouse2;
				}
				return result;
			}
		}
		ZString IWarehouseIntegrationSupporter.WarehouseTransactionStatus
		{
			get { return CH_WarehouseTransactionStatus; }
			set { CH_WarehouseTransactionStatus = value; }
		}

		ZString IWarehouseIntegrationSupporter.EntryNumber
		{
			get { return EntryNumber; }
		}

		public bool IsIntoTemporaryImportEnabled => (Declaration?.IsIntoTemporaryImportEnabled ?? false) && EntryInstruction.HasIntoTemporaryImportProcedure;

		public bool IsOutOfTemporaryImportEnabled => (Declaration?.IsOutOfTemporaryImportEnabled ?? false) && EntryInstruction.HasOutOfTemporaryImportProcedure;

		public bool IsIntoTemporaryExportEnabled => (Declaration?.IsIntoTemporaryExportEnabled ?? false) && EntryInstruction.HasIntoTemporaryExportProcedure;

		public bool IsOutOfTemporaryExportEnabled => (Declaration?.IsOutOfTemporaryExportEnabled ?? false) && EntryInstruction.HasOutOfTemporaryExportProcedure;

		public bool IsIntoInwardProcessingEnabled => (Declaration?.IsIntoInwardProcessingEnabled ?? false) && EntryInstruction.HasIntoInwardProcessingProcedure;

		public bool IsOutOfInwardProcessingEnabled => (Declaration?.IsOutOfInwardProcessingEnabled ?? false) && EntryInstruction.HasOutOfInwardProcessingProcedure;

		public bool IsIntoOutwardProcessingEnabled => (Declaration?.IsIntoOutwardProcessingEnabled ?? false) && EntryInstruction.HasIntoOutwardProcessingProcedure;

		public bool IsOutOfOutwardProcessingEnabled => (Declaration?.IsOutOfOutwardProcessingEnabled ?? false) && EntryInstruction.HasOutOfOutwardProcessingProcedure;

		#endregion

		protected override NoteTypeCollection NoteTypesCore
		{
			get
			{
				NoteTypeCollection fNoteTypes = base.NoteTypesCore;

				fNoteTypes.Add(PredefinedNoteTypes.Instance.CustomsMessageRemarks);
				fNoteTypes.Add(PredefinedNoteTypes.Instance.CustomsDeliveryInstructions);

				return fNoteTypes;
			}
		}

		#region IDocManagerSupport Members

		DocManagerInfo IDocManagerSupport.DocManagerInfo
		{
			get
			{
				if (docManagerInfo == null)
				{
					docManagerInfo = new DocManagerInfo(this, Core.Constants.DocManagerCodes.CustomsEntry);
				}
				return docManagerInfo;
			}
		}
		DocManagerInfo docManagerInfo;

		#endregion

		#region IDocManagerSupportProvider

		IEnumerable<IDocManagerSupport> IDocManagerSupportProvider.DocManagerSupports
		{
			get
			{
				if (Declaration?.Shipment is IDocManagerSupport shipment)
				{
					yield return shipment;
				}
				yield return Declaration;
			}
		}

		#endregion

		#region ICustomsChargesProvider Members

		ICustomsCharges[] IAccInvoiceDataProvider.CustomsCharges
		{
			get
			{
				ICustomsCharges customsCharges = GetCustomsChargesProvider();
				return new ICustomsCharges[] { new CusEntryHeaderCustomsCharges.CustomsChargeCache(customsCharges) };
			}
		}

		protected virtual ICustomsCharges GetCustomsChargesProvider()
		{
			return new CusEntryHeaderCustomsCharges(this);
		}

		ICustomsJobInfo IAccInvoiceDataProvider.CustomsJob
		{
			get { return Declaration; }
		}

		ZDateTime IAccInvoiceDataProvider.InvoiceDate
		{
			get { return ZDateTime.Today; }
		}

		ZDateTime IAccInvoiceDataProvider.APDueDate
		{
			get { return ZDateTime.Today; }
		}

		bool IAccInvoiceDataProvider.IsBillable
		{
			get { return true; }
		}

		string IAccInvoiceDataProvider.ReasonForUnbillability
		{
			get { return ""; }
		}

		ZString IAccInvoiceDataProvider.UniqueNumber
		{
			get { return GetUniqueNumberForAccountingIntegrationCore(); }
		}

		protected virtual ZString GetUniqueNumberForAccountingIntegrationCore()
		{
			return EntryNumber;
		}

		ZString IAccInvoiceDataProvider.PreviousUniqueNumber
		{
			get { return GetPreviousUniqueNumberForAccountingIntegrationCore(); }
		}

		protected virtual ZString GetPreviousUniqueNumberForAccountingIntegrationCore()
		{
			return ZString.Empty;
		}

		AutoPostingNotification IAccInvoiceDataProvider.AutoPostingNotification
		{
			get
			{
				if (Declaration != null)
				{
					return ((ICustomsJobInfo)Declaration).AutoPostingNotification;
				}
				else
				{
					var groupNotification = Enterprise.Customs.DataRegistry.Business.CustomsDataRegistry.Instance.AutoBillingEmailNotificationGroup.Value;
					return new AutoPostingNotification(new ZGuid[] { groupNotification.SendGroupPK }, groupNotification.SuppressUnpostARNotificaiton);
				}
			}
		}

		string IAccInvoiceDataProvider.EntryWithdrawnStatusTerm
		{
			get { return Res.GetString("b102f204-dffd-4d01-9c4e-98fdd9bb48a7", "withdrawn"); }
		}

		bool IAccInvoiceDataProvider.IsAutoBillingDueDateFromPaymentTerms => CustomsDataRegistry.Instance.AutoBillingDueDateFromPaymentTerms.Value;
		
		bool IAccInvoiceDataProvider.IsEligibleForIntegration
		{
			get { return HasBeenLodgedAtCustomsForAccIntegration || statusChangedToClearForAccIntegration; }
		}

		bool IAccInvoiceDataProvider.APInvoiceNumberAlwaysIncludeChargeCode => false;

		bool IAccInvoiceDataProvider.MatchCustomsChargesToClear(ZString apInvoiceNumber, ZString description) => CusEntryHeaderCustomsCharges.MatchCustomsCharges(this, apInvoiceNumber, description);

		#endregion

		#region IRegistryAccessingSupporter Members

		public virtual Guid RegistryCompanyPK
		{
			get
			{
				var declaration = Declaration;
				return declaration == null ? GlbCompany.CurrentCompany.PK.ToGuid() : declaration.RegistryCompanyPK;
			}
		}

		public virtual Guid RegistryBranchPK
		{
			get
			{
				var declaration = Declaration;
				return declaration == null ? GlbBranch.CurrentBranch.PK.ToGuid() : declaration.RegistryBranchPK;
			}
		}

		#endregion

		#region IAddInfoChildSupporter Members

		BusinessObject IAddInfoChildSupporter.AddInfoChild => GetAddInfoChild();
		protected virtual BusinessObject GetAddInfoChild() => null;

		SchemaGuidColumn IAddInfoChildSupporter.ChildForeignKeyColumn => GetChildForeignKeyColumn();
		protected virtual SchemaGuidColumn GetChildForeignKeyColumn() => null;

		void IAddInfoChildSupporter.RegisterListChangedCalledRefreshBinding(System.ComponentModel.IBindingList element) => RegisterListChangedCalledRefreshBinding(element);
		void IAddInfoChildSupporter.UnRegisterListChangedCalledRefreshBinding(System.ComponentModel.IBindingList element) => UnRegisterListChangedCalledRefreshBinding(element);
		#endregion
		#region IWorkflowTriggerEventSource Members

		public IGlbCompany JobHeaderCompany
		{
			get { return Declaration != null ? Declaration.Company : null; }
		}

		public IReadOnlyList<IWorkflowProviderCore> ParentWorkflowProviders
		{
			get
			{
				var list = new List<IWorkflowProviderCore>();
				var declaration = Declaration;
				if (declaration != null)
				{
					list.Add(declaration);
				}
				return list;
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
				var declaration = Declaration;
				if (declaration != null)
				{
					result = declaration.Lookups.MessageTypeList;
				}
			}
			return result;
		}

		#endregion

		#region ICDArchive Members

		public CDArchiveInfo CDArchiveInfo
		{
			get { return new CusEntryHeaderCDArchiveInfo(this); }
		}

		public class CusEntryHeaderCDArchiveInfo : CDArchiveInfo
		{
			public CusEntryHeaderCDArchiveInfo(CusEntryHeader entry)
				: base(entry)
			{
			}

			CusEntryHeader Entry
			{
				get { return (CusEntryHeader)BusinessEntity; }
			}

			BaseJobDeclaration.DeclarationCDArchiveInfo DeclarationInfo
			{
				get { return (BaseJobDeclaration.DeclarationCDArchiveInfo)((ICDArchive)Entry.Declaration).CDArchiveInfo; }
			}

			public override ZString ConsigneeCode
			{
				get { return DeclarationInfo.ConsigneeCode; }
			}

			public override ZString ConsignorCode
			{
				get { return DeclarationInfo.ConsignorCode; }
			}

			public override ZString[] ContainerNumbersList
			{
				get { return Entry.Declaration.ContainerNumbersListCore; }
			}

			public override ZString Destination
			{
				get { return DeclarationInfo.Destination; }
			}

			public override ZString[] EntryNumbersList
			{
				get
				{
					var result = new List<ZString>();
					AddToList(result, Entry.EntryNumber);
					return result.ToArray();
				}
			}

			public override ZDateTime ETA
			{
				get { return DeclarationInfo.ETA; }
			}

			public override ZDateTime ETD
			{
				get { return DeclarationInfo.ETD; }
			}

			public override ZString VoyageFlight
			{
				get { return DeclarationInfo.VoyageFlight; }
			}

			public override ZString HouseBill
			{
				get { return DeclarationInfo.HouseBill; }
			}

			public override ZString[] InvoiceNumbersList
			{
				get { return Entry.Declaration.InvoiceNumbersListCore; }
			}

			public override ZString JobNumber
			{
				get { return DeclarationInfo.JobNumber; }
			}

			public override ZString MasterBill
			{
				get { return DeclarationInfo.MasterBill; }
			}

			public override ZString[] OrderNumbersList
			{
				get { return DeclarationInfo.OrderNumbersList; }
			}

			public override ZString Origin
			{
				get { return DeclarationInfo.Origin; }
			}

			public override ZString Vessel
			{
				get { return DeclarationInfo.Vessel; }
			}
		}

		#endregion

		Type IDocsAndCartageParent.DocsAndCartageParentType
		{
			get { return ((IDocsAndCartageParent)Declaration).DocsAndCartageParentType; }
		}

		Type IDocsAndCartageParent.DocsAndCartageType
		{
			get { return ((IDocsAndCartageParent)Declaration).DocsAndCartageType; }
		}

		IHaveRequiredDocuments IDocsAndCartageParent.RequiredDocumentsProvider
		{
			get { return ((IDocsAndCartageParent)Declaration).RequiredDocumentsProvider; }
		}

		string IJobNumber.JobNumber
		{
			get { return CH_BGMReference.IsEmpty ? Declaration?.JE_DeclarationReference ?? ZString.Empty : CH_BGMReference; }
		}

		#region ICustomsFileParent

		ZString ICustomsFileParent.DeclarationType
		{
			get { return Declaration.JE_MessageType; }
		}

		ZPropertyInfo ICustomsFileParent.DeclarationTypeInfo
		{
			get { return Declaration.JE_MessageTypeInfo; }
		}

		ZGuid ICustomsFileParent.BranchPk
		{
			get { return Declaration.JE_GB; }
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
		}

		#endregion

		public CusEntryLine RandomEntryLine
		{
			get { return MergedLines.Count > 0 ? MergedLines[0] : null; }
		}

		public bool HasAnyConfirmedFeesOnAnyMergedLine => MergedLines.Any(x => x.ConfirmedFees.Count > 0);

		IReadOnlyList<IWorkflowProvider> IWorkflowTriggerFieldChangeSource.ParentWorkflowProviders => Declaration == null ? Array.Empty<IWorkflowProvider>() : new IWorkflowProvider[] { Declaration };

		public ICommonGoodsItemsIntegrator CommonGoodsItemsIntegrator => CommonGoodsItemsIntegratorCore;
		protected virtual ICommonGoodsItemsIntegrator CommonGoodsItemsIntegratorCore => new CommonGoodsItemsIntegrator(this);

		#region ICustomsDocumentGeneratorSupporter Members

		ZString Integration.Customs.ICustomsDocumentGeneratorSupporter.GetDocumentName(ZString actionCode)
		{
			return GetDocumentNameForDocumentGeneratorCore(actionCode);
		}

		protected virtual ZString GetDocumentNameForDocumentGeneratorCore(ZString actionCode)
		{
			throw new InvalidOperationException("This method should be overridden in the seperate class for each country.");
		}

		ZString Integration.Customs.ICustomsDocumentGeneratorSupporter.JobReference
		{
			get { return JobReferenceForDocumentGeneratorCore; }
		}

		protected virtual ZString JobReferenceForDocumentGeneratorCore => Declaration?.JE_DeclarationReference ?? CH_BGMReference;

		ZBool Integration.Customs.ICustomsDocumentGeneratorSupporter.GenerateCustomsDocument(ZString actionCode)
		{
			return GenerateCustomsDocumentForDocumentGeneratorCore(actionCode);
		}

		protected virtual ZBool GenerateCustomsDocumentForDocumentGeneratorCore(ZString actionCode)
		{
			return ZBool.False;
		}

		Guid Integration.Customs.ICustomsDocumentGeneratorSupporter.BranchPK => RegistryBranchPK;

		ZString Integration.Customs.ICustomsDocumentGeneratorSupporter.GetReasonForUnableToGenerateCustomsDocument()
		{
			return GetReasonForUnableToGenerateCustomsDocumentCore();
		}

		protected virtual ZString GetReasonForUnableToGenerateCustomsDocumentCore()
		{
			return ZString.Empty;
		}

		#endregion

		#region IAllowPermitProcessing

		public virtual ZString GetPermitReference() => CH_BGMReference;
		public virtual ZInt GetPermitReferenceNumberLine() => 0;

		public virtual ZString GetPermitComment(PermitRecord permitRecord)
		{
			var procedure = permitRecord?.Procedure ?? ZString.Empty;
			return procedure.IsEmpty ? new ZString((NoResString)"Customs Entry") : new ZString((NoResString)"Customs Entry - " + procedure);
		}
		public IList<PermitRecord> GetPermitRecords() => GetPermitRecordsCore();

		protected virtual IList<PermitRecord> GetPermitRecordsCore() => new List<PermitRecord>();

		public ZInt PermitValueDecimalPlaceCount => RandomEntryLine?.InvoiceCurrency?.Decimals ?? 2;

		public ZInt PermitQuantityDecimalPlaceCount => RandomEntryLine.RandomLine.GetDecimalPlacesMetaData(AutoJobComInvoiceLine.Schema.JI_CustomsQuantity);

		ZInt IAllowPermitProcessing.PackageCount => PackagesCount;
		#endregion

		#region IClusterKeyWorker

		ZPropertyInfoInt IClusterKeyEntity.ClusterKeyPty => (ZPropertyInfoInt)CH_ClusterKeyInfo;
		Type IClusterKeyWorker.ParentBizObjType => typeof(BaseJobDeclaration);
		ZPropertyInfoGuid IClusterKeyWorker.FkToParentPty => (ZPropertyInfoGuid)CH_JEInfo;

		IEnumerable<ClusterKeyChildInfo> IClusterKeyWorker.ClusterKeyChildList
		{
			get
			{
				yield return new ClusterKeyChildInfo(typeof(CusEntryHeaderCharges), CusEntryHeaderChargesSchema.C1_CH);
				yield return new ClusterKeyChildInfo(typeof(CusEntryPayInfo), CusEntryPayInfoSchema.C9_CH);
				yield return new ClusterKeyChildInfo(typeof(CusEntryLine), CusEntryLineSchema.CL_CH);
				yield return new ClusterKeyChildInfo(ObjectFactory.GetType<Integration.Customs.EU.ICusEUEntryHeader>(), CusEUEntryHeaderSchema.EUH_CH);
				yield return new ClusterKeyChildInfo(ObjectFactory.GetType<Integration.Customs.AU.IQuarantineColsHeader>(), QuarantineColsHeaderSchema.QCH_CH_CusEntryHeader);
			}
		}

		#endregion

		#region IBranchProvider

		public GlbBranch Branch => Declaration?.Branch ?? Factory.Load<GlbBranch>(GlbBranch.CurrentBranch.PK);

		#endregion

		#region IEDIMessageCollectionOwner
		BusinessObject IEDIMessageCollectionOwner.MessageOwner => this;

		IBusinessObjectCollection IEDIMessageCollectionOwner.Messages => Messages;
		#endregion

		#region IProcessHandlingInfoProvider

		ProcessHandlingInfo IProcessHandlingInfoProvider.ProcessHandlingInfo => GetProcessHandingInfoCore();

		protected virtual ProcessHandlingInfo GetProcessHandingInfoCore() => new CusEntryHeaderProcessHandlingInfo(this);

		#endregion

		#region IControllerIDProvider

		ControllerID IControllerIDProvider.ControllerID => ControllerIDs.Customs.EntryHeader;

		Guid IControllerIDProvider.BusinessObjectPK => PK.ToGuid();

		#endregion

		#region IRelatedJob

		ZString IRelatedJob.JobDescription => HumanReadableName;

		ZString IRelatedJob.JobStatus => CH_EntryStatus;

		ZString IRelatedJob.JobNumber => CH_BGMReference;

		#endregion

		public ZString BrokerageCountryCode => Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(CountryCode);

		#region Amendment Snapshot

		public AmendmentSnapshotManager GetNewAmendmentSnapshotManager() => GetNewAmendmentSnapshotManagerCore();

		protected virtual AmendmentSnapshotManager GetNewAmendmentSnapshotManagerCore() => null;

		#endregion

		#region ITypeDeciderContext Members

		string ITypeDeciderContext.Country => (Declaration as ITypeDeciderContext)?.Country ?? GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

		#endregion
	}
}
