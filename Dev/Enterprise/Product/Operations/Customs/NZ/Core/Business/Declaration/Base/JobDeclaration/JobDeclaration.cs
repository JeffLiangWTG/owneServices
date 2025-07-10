using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Integration;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.CustomsLists;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.NZ;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.NZ.Business.Declaration.ECIWriteOff;
using Enterprise.Customs.NZ.Business.Declaration.FormalEntry;
using Enterprise.Customs.NZ.Business.Declaration.InterfaceImplementations;
using Enterprise.Customs.NZ.Business.MAFeBACCa;
using Enterprise.Customs.NZ.Business.MAFeBACCa.CodedLists;
using Enterprise.Customs.NZ.Business.MAFeBACCa.Interfaces;
using Enterprise.Customs.NZ.Business.MAFeBACCa.MessageBuilders;
using Enterprise.Customs.NZ.Registry;
using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineIntegration;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.Universal.Constants;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.NZ.Business.Declaration
{
	[SystemDefinedValues]
	[UniversalCopyAddInfo(JobDeclarationSchema.Constants.Prefix, NZ.Business.NZAddInfo.Schema.Prefix)]
	public class JobDeclaration : BaseJobDeclaration
		, ICurrencyConverterDataProvider
		, ILandedCostHeader
		, ITransmitDateSourceData
		, Integration.Customs.NZ.IJobDeclaration
		, IHaveNZAddInfo
		, ICusAddInfoTypeSupporter
		, IApportionInvoiceHolder
		, IAddInfoManager
		, IManifestProvider
		, ITranshipmentRequestParent
		, IMPIAccountDetails
		, ISourceIdentifierProvider
	{
		public JobDeclaration(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static new JobDeclaration New(BusinessObjectFactory factory)
		{
			return factory.New<JobDeclaration>();
		}

		public new class Schema : BaseJobDeclaration.Schema
		{
			public const string JE_MAF_ConsignmentNumber = "JE_MAF_ConsignmentNumber";
			public const string JE_MAF_MessagingStatusDescription = "JE_MAF_MessagingStatusDescription";
			public const string JE_EDITransmitDate = "JE_EDITransmitDate";
			public const string JE_DeclaredWeight = "JE_DeclaredWeight";
			public const string JE_DeclaredWeightUQ = "JE_DeclaredWeightUQ";
			public const string JE_ECI_InvoiceAmount = "JE_ECI_InvoiceAmount";
			public const string JE_ECI_InvoiceCurrency = "JE_ECI_InvoiceCurrency";
			public const string JE_ECI_LastResponseStatus = "JE_ECI_LastResponseStatus";

			public const string JE_SoldOrConsigned = "JE_SoldOrConsigned";
			public const string JE_RL_NKProcessingPort = "JE_RL_NKProcessingPort";
			public const string JE_SendMCDContainerQuarantineDeclaration = "JE_SendMCDContainerQuarantineDeclaration";
			public const string JE_HaveMAFContainerDeclaration = "JE_HaveMAFContainerDeclaration";
			public const string JE_IsContainerClean = "JE_IsContainerClean";
			public const string JE_IsPackagingMaterialContaminated = "JE_IsPackagingMaterialContaminated";
			public const string JE_IsWoodPackagingUsed = "JE_IsWoodPackagingUsed";
			public const string JE_IsWoodPackagingTreated = "JE_IsWoodPackagingTreated";
			public const string JE_IsWoodPackagingTreatmentCertificateAvailable = "JE_IsWoodPackagingTreatmentCertificateAvailable";
			public const string JE_OriginalEntryNumber = "JE_OriginalEntryNumber";
			public const string JE_OriginalEntryType = "JE_OriginalEntryType";

			public const string JE_SEPOtherInfoValue = "JE_SEPOtherInfoValue";
			public const string JE_ATFOtherInfoValue = "JE_ATFOtherInfoValue";
			public const string JE_PDOOtherInfoValue = "JE_PDOOtherInfoValue";

			public const string MiscSupplierName = "MiscSupplierName";
			public const string MiscImporterName = "MiscImporterName";

			public const string JE_TransactionNature = "JE_TransactionNature";
			public const string JE_TSWCombinedStatus = "JE_TSWCombinedStatus";
			public const string ZX_PaymentMethod = "ZX_PaymentMethod";
			public const string ZX_AccountHolder = "ZX_AccountHolder";
			public const string ZX_AccountNumber = "ZX_AccountNumber";

			public const string JE_ManifestBioStatus = "JE_ManifestBioStatus";
			public const string JE_ManifestNZCSStatus = "JE_ManifestNZCSStatus";

			public const string JE_RL_NKPortOfDeliveryNotify = "JE_RL_NKPortOfDeliveryNotify";
			public const int JE_Cal_GoodsLocationMaxLength = 100;
		}

		protected override IEnumerable<ZString> JobDeclarationMessageCollectionApplicationCodeListCore
		{
			get { return new ZString[] { ApplicationCodeList.Codes.NZMAFeBACCa }; }
		}

		#region Validation
		public new JobDeclarationValidation Validation
		{
			get { return (JobDeclarationValidation)base.Validation; }
		}

		protected override Customs.Business.JobDeclarationValidation GetNewValidation()
		{
			if (IsECIWriteoff)
			{
				return new JobDeclarationValidationECIWriteOff(this);
			}
			else
			{
				return new JobDeclarationValidationFormalEntry(this);
			}
		}

		public override ZValidation PiggyBackedDocAddressValidation(JobDocAddress addressToValidate)
		{
			return new JobDocAddressValidation(addressToValidate, this);
		}

		#endregion

		#region Lookups
		protected override Customs.Business.JobDeclarationLookups GetNewLookups()
		{
			return new JobDeclarationLookups(this);
		}

		public new JobDeclarationLookups Lookups
		{
			get { return (JobDeclarationLookups)base.Lookups; }
		}

		#endregion

		protected override Customs.Business.JobDeclarationSynchroniser GetNewShipmentSynchroniser()
		{
			return new JobDeclarationSynchroniser(this);
		}

		public override ZBool AreMultipleEntryInstructionsAllowed => false;

		protected override bool IsIntegrationWithAccountingSupported => !IsDrawback;

		#region MergeManager
		public override Customs.Business.MergeManager MergeManager
		{
			get
			{
				Type requiredMergeManagerType = IsECIWriteoff ? typeof(ECIWriteOff.MergeManager) : typeof(FormalEntry.MergeManager);
				if (fMergeManager != null && fMergeManager.GetType() != requiredMergeManagerType)
				{
					fMergeManager = null;
				}
				return base.MergeManager;
			}
		}

		protected override Customs.Business.MergeManager GetMergeManager()
		{
			if (IsECIWriteoff)
			{
				return new ECIWriteOff.MergeManager(this);
			}
			else
			{
				return new FormalEntry.MergeManager(this);
			}
		}
		#endregion

		public bool SaveHandlingSaveExceptions()
		{
			// Calling save in this way is completely unsafe.
			bool result = false;
			try
			{
				Factory.Save();
				result = true;
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				ZExceptionReporting.HandleSaveException(e);
			}
			return result;
		}

		public void CancelQueuedMessagesAndResetEDITransmitDate()
		{
			if (IsQueuedForSending)
			{
				CusEntryHeader.CancelQueuedMessages();
				CusEntryHeader.SetMessagingStatusToNotSent();
				SetMessagingStatusToNotSent(this);
				SetEDITransmitDateIfRequired();
			}
		}

		public bool IsQueuedForSending
		{
			get { return IsFormalEntry && JE_EntryStatus == FormalEntryStatusList.Codes.QueuedForSending; }
		}

		protected override bool IsDeclarationIntegratedCore() => IsInterface;

		protected override bool ShowSubmitMenuItemCore() => IsInterface;

		public override ZString JE_ContainerMode
		{
			get { return base.JE_ContainerMode; }
			set
			{
				if (HasCusContainers && !JE_ContainerMode.IsEmpty && value.IsEmpty && !IsAir)
				{
					ErrorReporter.ReportOnce("NZ ContainerMode set to empty", "ContainerMode is set to empty by something. This is not exposed on GUI");
				}

				if (!SetterSuspender.IsSetterSuspended(JobDeclaration.Schema.JE_ContainerMode))
				{
					base.JE_ContainerMode = value;
				}
			}
		}

		protected override ZString ContainerModeForCartage => CusContainers.GetFirstValidContainerMode() ?? JE_ContainerMode;

		#region ResetToOriginal
		public void ResetToOriginal()
		{
			CusEntryHeader.CancelQueuedMessages();
			CusEntryHeader.CH_IsActive = false;
			SetMessagingStatusToNotSent(this);
		}

		void SetMessagingStatusToNotSent(JobDeclaration declaration)
		{
			declaration.JE_EntrySubmittedDate = ZDateTime.Empty;
			if (declaration.IsECIWriteoff)
			{
				declaration.JE_EntryStatus = LowValueConsignmentStatusList.Codes.NotSentToCustoms;
				declaration.JE_ECI_LastResponseStatus = LowValueConsignmentStatusList.Codes.NoStatusReported;
			}
			else
			{
				declaration.JE_EntryStatus = FormalEntryStatusList.Codes.NotSentToCustoms;
			}

			if (IsTSWDeclaration)
			{
				SetTSWStatusValuesToNotSent(declaration);
			}
		}

		void SetMAFMessagingStatusToNotSent(JobDeclaration declaration)
		{
			declaration.AddInfo.ZN_MAF_MessagingStatus = ZString.Empty;
			declaration.AddInfo.ZN_MAF_ConsignmentNumber = ZString.Empty;
			declaration.AddInfo.ZN_MAF_ReceiptNumber = ZString.Empty;
		}

		void SetTSWStatusValuesToNotSent(JobDeclaration declaration)
		{
			declaration.AddInfo.ZN_TSWCombinedStatus = ZString.Empty;
			declaration.AddInfo.ZN_NZCSStatus = ZString.Empty;
			declaration.AddInfo.ZN_MPIBioStatus = ZString.Empty;
			declaration.AddInfo.ZN_MPIFoodStatus = ZString.Empty;
		}

		public bool HasNotBeenSentToCustoms => Extensions.RemoveConsolidatedStatus(JE_EntryStatus) == FormalEntryStatusList.Codes.NotSentToCustoms;
		#endregion

		#region Strongly Typed Collections
		#region HouseBills

		[ChildEditable(true)]
		public new BillCollection<Bill, JobDeclaration> Bills
		{
			get { return (BillCollection<Bill, JobDeclaration>)base.Bills; }
		}

		protected override IBillCollection<Customs.Business.Bill, BaseJobDeclaration> CreateNewBillCollection()
		{
			return new BillCollection<Bill, JobDeclaration>(this, Factory);
		}

		#endregion

		#region CusContainers

		[ChildEditable(true)]
		public new CusContainerCollection CusContainers
		{
			get { return (CusContainerCollection)base.CusContainers; }
		}

		protected override ICusContainerCollection<BaseCusContainer> NewCusContainersCollection()
		{
			return new CusContainerCollection(this, Factory);
		}
		#endregion

		#region JobComInvoiceGroupHeaders

		[ChildEditable(false)]
		public new IJobComInvoiceGroupHeaderCollection<JobComInvoiceGroupHeader> JobComInvoiceGroupHeaders => (IJobComInvoiceGroupHeaderCollection<JobComInvoiceGroupHeader>)base.JobComInvoiceGroupHeaders;

		protected override IJobComInvoiceGroupHeaderCollection<BaseJobComInvoiceGroupHeader> CreateNewJobComInvoiceGroupHeaderCollection() => new BaseJobComInvoiceGroupHeaderCollection<JobComInvoiceGroupHeader>(this);

		public new JobComInvoiceGroupHeader TopGroupInvoice
		{
			get { return (JobComInvoiceGroupHeader)base.TopGroupInvoice; }
		}

		#endregion

		#region Invoices
		[ChildEditable]
		public new InvoiceHeaderActiveCollection Invoices
		{
			get { return (InvoiceHeaderActiveCollection)base.Invoices; }
		}

		protected override Customs.Business.InvoiceHeaderActiveCollection CreateNewInvoiceHeaderCollection()
		{
			return new InvoiceHeaderActiveCollection(this);
		}

		public JobComInvoiceHeader GetFirstInvoiceCurrencyIndicatorOfSameCurrency(ZString currencyCode, JobComInvoiceHeader exceptInvoiceHeader)
		{
			return
				Invoices.Except(exceptInvoiceHeader).FirstOrDefault(
					x => x is JobComInvoiceHeader && x.JZ_RX_NKInvoice_Currency == currencyCode) as JobComInvoiceHeader;
		}

		public IEnumerable<JobComInvoiceHeader> GetAllInvoicesOfSameCurrencyHavingDifferentCurrencyIndicator(ZString currencyCode, JobComInvoiceHeader exceptInvoiceHeader)
		{
			return
				Invoices
				.Except(exceptInvoiceHeader)
				.Where(
						x =>
							x is JobComInvoiceHeader && x.JZ_RX_NKInvoice_Currency == currencyCode &&
							x.JZ_InvoiceCurrExRateType != exceptInvoiceHeader.JZ_InvoiceCurrExRateType)
				.Cast<JobComInvoiceHeader>();
		}

		public IEnumerable<JobComInvoiceHeader> GetOtherInvoices(JobComInvoiceHeader exceptInvoiceHeader)
		{
			return
				Invoices
				.Except(exceptInvoiceHeader)
				.Cast<JobComInvoiceHeader>();
		}

		public ZBool HasTheSameGSTDetailsOnAllInvoices => Factory.GetValue(ref hasTheSameGSTDetailsOnAllInvoicesCached, () =>
		{
			var result = true;
			var firstInvoice = Invoices.Cast<JobComInvoiceHeader>().FirstOrDefault();
			if (firstInvoice != null)
			{
				var supplierGSTNumber = firstInvoice.JZ_SupplierGSTNumber;
				var isGSTPrePaid = firstInvoice.JZ_IsGSTPrePaid;
				result = Invoices.Cast<JobComInvoiceHeader>().All(i => i.JZ_SupplierGSTNumber == supplierGSTNumber && i.JZ_IsGSTPrePaid == isGSTPrePaid);
			}
			return result;
		});

		CachedProperty<ZBool> hasTheSameGSTDetailsOnAllInvoicesCached;

		public ZInt NoOfInvoices => Invoices.Count;

		#endregion

		#region InvoiceLines
		public new IInvoiceLineViewCollection<JobComInvoiceLine> FilteredInvoiceLines
		{
			get { return (IInvoiceLineViewCollection<JobComInvoiceLine>)base.FilteredInvoiceLines; }
		}

		protected override IInvoiceLineViewCollection<BaseJobComInvoiceLine> GetNewInvoiceLineViewCollection()
		{
			return new InvoiceLineViewCollection<JobComInvoiceLine>(this);
		}

		[ChildEditable(true)]
		public new InvoiceLineCompleteCollection InvoiceLines
		{
			get { return (InvoiceLineCompleteCollection)base.InvoiceLines; }
		}

		protected override Customs.Business.InvoiceLineCompleteCollection GetNewInvoiceLineCompleteCollection()
		{
			return new InvoiceLineCompleteCollection(this);
		}
		#endregion

		#region CusEntryHeader/s
		/// <summary>
		/// Contains the Currently Active CusEntryHeader for this Declaration.
		/// </summary>
		public CusEntryHeader CusEntryHeader => Factory.GetValue(ref cusEntryHeaderProperty, delegate
		{
			CusEntryHeader result = ExistingCusEntryHeader;
			if (result == null)
			{
				if (IsTSWCancellation || !IsPersistent)
				{
					result = Factory.GetNull<CusEntryHeader>();
				}
				else
				{
					result = CustomsEntryHeaders.AddNew();
					existingCusEntryHeaderProperty = null;
				}
			}
			return result;
		});

		CachedProperty<CusEntryHeader> cusEntryHeaderProperty;

		public CusEntryHeader ExistingCusEntryHeader => Factory.GetValue(ref existingCusEntryHeaderProperty, delegate
		{
			CusEntryHeader result = GetAppropriateCusEntryHeaderIfExists();
			return result;
		});

		CachedProperty<CusEntryHeader> existingCusEntryHeaderProperty;

		internal CusEntryHeader GetAppropriateCusEntryHeaderIfExists()
		{
			var entryHeaders = EntryHeadersMatchingCurrentDeclarationType;
			CusEntryHeader result = null;
			if (entryHeaders.Count() == 1)
			{
				result = entryHeaders.FirstOrDefault();
			}
			else if (entryHeaders.Count() > 1)
			{
				result = entryHeaders.FirstOrDefault(x => x.IsActive);
				if (result == null)
				{
					result = entryHeaders.FirstOrDefault(x => !x.EntryNumber.IsEmpty || x.Messages.Count > 0);
				}
				if (result == null)
				{
					result = entryHeaders.FirstOrDefault(x => x.IsInDatabase);
				}
				if (result == null)
				{
					result = entryHeaders.FirstOrDefault();
				}
				entryHeaders.Where(x => x != result && x.EntryNumber.IsEmpty && x.Messages.Count == 0).ForEach(x => x.Delete());
			}
			return result;
		}

		public
#if DEBUG
 virtual
#endif
 Type TypeOfEntryHeaderRequiredForCurrentDeclarationSettings
		{
			get
			{
				Type result = typeof(FormalEntry.CusEntryHeader);
				if (IsECIWriteoff)
				{
					if (IsECIManifestDeclarationReference)
					{
						result = typeof(ECIWriteOff.Manifesting.CusEntryHeader);
					}
					else
					{
						result = typeof(ECIWriteOff.CusEntryHeader);
					}
				}
				else if (IsCompletion)
				{
					result = typeof(CompletionCusEntryHeader);
				}
				else if (IsPrimaryIndustriesImportDeclaration)
				{
					result = typeof(PrimaryIndustriesCusEntryHeader);
				}

				return result;
			}
		}

		#region CustomsEntryHeaders

		[ChildEditable(true)]
		public new CusEntryHeaderCollection<CusEntryHeader> CustomsEntryHeaders => (CusEntryHeaderCollection<CusEntryHeader>)base.CustomsEntryHeaders;

		protected override ICusEntryHeaderCollection<Customs.Business.CusEntryHeader> NewCustomsEntryHeaders() => new CusEntryHeaderCollection(this, Factory);

		#endregion

		#endregion

		#region ActiveEntryHeaders - Used for Merging in Base, not the same as CH_IsActive

		public new ActiveCusEntryHeaderCollection ActiveEntryHeaders
		{
			get { return (ActiveCusEntryHeaderCollection)base.ActiveEntryHeaders; }
		}

		protected override Customs.Business.ActiveCusEntryHeaderCollection GetActiveEntryHeaderCollection()
		{
			return new ActiveCusEntryHeaderCollection(this);
		}

		#endregion

		#region Packing Groups/ Packages

		[ChildEditable(true)]
		public new BaseDeclarationLevelPackageCollection<Package> Packages
		{
			get { return (BaseDeclarationLevelPackageCollection<Package>)base.Packages; }
		}

		protected override IDeclarationLevelPackageCollection<BasePackage> GetPackagesCollection()
		{
			return new BaseDeclarationLevelPackageCollection<Package>(this);
		}

		[ChildEditable(true)]
		public new DeclarationLevelPackingGroupCollection PackingGroups
		{
			get { return (DeclarationLevelPackingGroupCollection)base.PackingGroups; }
		}

		protected override BaseDeclarationLevelPackingGroupCollection CreateNewPackingGroups()
		{
			return new DeclarationLevelPackingGroupCollection(this);
		}

		#endregion

		#endregion

		#region Overrides

		protected override void LogEventIfJE_EntryStatusChangedCore()
		{
			if (JE_EntryStatus != FormalEntryStatusList.Codes.DeliveryOrderReceived && JE_EntryStatus != LowValueConsignmentStatusList.Codes.ConsignmentWrittenOff)
			{
				base.LogEventIfJE_EntryStatusChangedCore();
			}
		}

		protected override ZBool IsReciprocalRatesCore
		{
			get { return IsReciprocalRatesConstant; }
		}

		public static bool IsReciprocalRatesConstant
		{
			get { return false; }
		}

		protected override ZString LocalCurrencyCodeCore
		{
			get { return LocalCurrencyConstantCode; }
		}

		public static ZString LocalCurrencyConstantCode
		{
			get { return Enterprise.Core.Constants.CurrencyCodes.NewZealand; }
		}

		internal static RefCurrency GetLocalCurrency()
		{
			return RefCurrency.LoadFromCurrencyCode(GlbCompany.CurrentCompany.Factory, LocalCurrencyConstantCode);
		}

		public override ZGuid JE_OH_Supplier
		{
			get { return base.JE_OH_Supplier; }
			set
			{
				bool hasChanged = base.JE_OH_Supplier != value;
				base.JE_OH_Supplier = value;
				if (hasChanged)
				{
					Invoices.MarkAsNeedingValidation();
					InvoiceLines.MarkAsNeedingValidation();
				}
			}
		}

		protected override string SubmissionTypeBuiltinCode => JobApplicationCodeList.Codes.TSW;

		[BusinessObjectTestExclude]
		[ResourceStringData("D41DE869-BB50-4E09-B62F-9778303FD47B", FullDescription = "Customs messaging submission method.", Caption = "Submit Type", ShortCaption = "Type")]
		public override ZString JE_ApplicationCode
		{
			get { return base.JE_ApplicationCode; }
			set
			{
				bool hasChanges = base.JE_ApplicationCode != value;
				base.JE_ApplicationCode = value;

				if (hasChanges)
				{
					RefreshIncotermAndChargeFactory();
					ResetDefaultPDOOtherInfoIfRequired();
					RefreshInvoiceLineProductMaxLimit();
					Validation.ValidateJE_MessageSubType();
					Packages.MarkAsNeedingValidation();
				}
			}
		}

		public override ZGuid JE_OH_Forwarder
		{
			get { return base.JE_OH_Forwarder; }
			set
			{
				bool hasChanges = base.JE_OH_Forwarder != value;
				base.JE_OH_Forwarder = value;

				if (hasChanges)
				{
					if (!IsCopying)
					{
						RefreshDefaultGoodsLocation();
					}

					ValidateGoodsLocatedAtIfRequired();
				}
			}
		}

		public override ZGuid JE_OH_ShippingLine
		{
			get { return base.JE_OH_ShippingLine; }
			set
			{
				bool hasChanges = base.JE_OH_ShippingLine != value;
				base.JE_OH_ShippingLine = value;

				if (hasChanges)
				{
					if (!IsCopying)
					{
						RefreshDefaultGoodsLocation();
					}

					ValidateGoodsLocatedAtIfRequired();
				}
			}
		}

		void ValidateGoodsLocatedAtIfRequired()
		{
			if (IsTSWDeclaration && JE_GoodsLocatedAtVisible)
			{
				AddInfo.Validation.ValidateZN_GoodsLocatedAt();
			}
		}

		protected override bool IsHouseBillMandatory
		{
			get
			{
				var result = true;
				if (IsPeriodic)
				{
					result = false;
				}
				else if (IsTSWDeclaration)
				{
					result = IsTSWCREWriteOff;
				}

				return result;
			}
		}

		protected override void SetDefaultValues()
		{
			try
			{
				settingDefaultValues = true;
				base.SetDefaultValues();
				using (SuspendMarkingAsNeedingValidation())
				{
					JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
					SetMessagingStatusToNotSent(this);
					DefaultJE_RL_NKProcessingPortFromRegistry();
					JE_PaymentMethod = PaymentMethodList.Codes.BrokerDeferred;
					if (JE_TransactionNatureVisible)
					{
						JE_TransactionNature = NatureOfTransactionList.Codes.N10;
					}
				}
			}
			finally
			{
				settingDefaultValues = false;
			}
		}

		bool settingDefaultValues;

		protected void DefaultJE_RL_NKProcessingPortFromRegistry()
		{
			JE_RL_NKProcessingPort = NZCustomsDataRegistry.Instance.DefaultCustomsProcessingPort.Value;
		}

		protected override string DefaultTotalNoOfPacksPackType
		{
			get { return UniversalReferenceConstants.PackageTypeListCodes.Package; }
		}

		protected override void ResetValuesOnTemplateCopyAfterClone(BaseJobDeclaration baseDeclaration, CloneType cloneType)
		{
			base.ResetValuesOnTemplateCopyAfterClone(baseDeclaration, cloneType);
			JobDeclaration declaration = (JobDeclaration)baseDeclaration;
			SetMessagingStatusToNotSent(declaration);
			SetMAFMessagingStatusToNotSent(declaration);
			SetTSWStatusValuesToNotSent(declaration);
			declaration.AddInfo.ZN_IsFormalChangedToIPI = false;
			declaration.ResetDefaultPDOOtherInfoIfRequired();
		}

		public override Notes Notes
		{
			get
			{
				if (Shipment == null || isCloning)
				{
					return base.Notes;
				}
				else
				{
					return Shipment.Notes;
				}
			}
		}

		#region Clone Implementation
		bool isCloning;
		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			BusinessObject result = null;
			isCloning = true;
			try
			{
				result = base.CloneInternal(args);
			}
			finally
			{
				isCloning = false;
			}
			return result;
		}

		#endregion

		public override ZString DeclarationNumber
		{
			get => ExistingCusEntryHeader?.EntryNumber ?? ZString.Empty;
			set
			{
				CusEntryHeader.EntryNumber = value;
				DeclarationNumberInfo.RefreshBinding();
			}
		}

		protected override void DefaultValuesFromLocalPartyWhenEnteredCore(OrgHeader party)
		{
			base.DefaultValuesFromLocalPartyWhenEnteredCore(party);
			DefaultJE_PaymentMethodFromLocalParty(party);
		}

		protected void DefaultJE_PaymentMethodFromLocalParty(OrgHeader localParty)
		{
			var miscServ = localParty.MiscServ;
			if (miscServ != null && !miscServ.OM_IMPaymentMethod.IsEmpty && Lookups.PaymentPartyList.ContainsCode(miscServ.OM_IMPaymentMethod))
			{
				JE_PaymentMethod = miscServ.OM_IMPaymentMethod;
			}
		}

		protected override ZDate GetDateForDutyRateCore()
		{
			ZDateTime barrierDate = BarrierDate;
			ZDateTime dutyRateDate = barrierDate.IsEmpty || !barrierDate.IsValid ? CachedTodaysDate : barrierDate;
			return dutyRateDate.Date;
		}

		protected override void SynchroniseWithShipmentIfNeededCore()
		{
			base.SynchroniseWithShipmentIfNeededCore();
			SetupImporterJobDocAddresses();
		}

		[ResourceStringData("D045F368-4FB0-4858-A34F-0993B93BD42B", Caption = "Parcel No", IsApplicableMember = nameof(IsPost))]
		public override ZString JE_HouseBill
		{
			get => base.JE_HouseBill;
			set
			{
				bool hasChanged = base.JE_HouseBill != value;
				base.JE_HouseBill = value;
				if (!IsCopying && hasChanged)
				{
					Packages.MarkAsNeedingValidation();
				}
			}
		}

		[BusinessObjectTestExclude]
		public override ZString JE_RL_NKPortOfFirstArrival
		{
			get { return ZString.Empty; }
			set { }
		}

		[BusinessObjectTestExclude]
		public override ZDateTime JE_DateOfFirstArrival
		{
			get { return ZDateTime.Empty; }
			set { }
		}

		public override ZString JE_TransportMode
		{
			get { return base.JE_TransportMode; }
			set
			{
				bool hasChanged = base.JE_TransportMode != value;
				base.JE_TransportMode = value;
				if (hasChanged)
				{
					SetEDITransmitDateIfRequired();
					SetGoodsLocatedIfRequired();
					if (value == JobTransportModeList.Codes.Air)
					{
						ResetContainersAndEquipmentsOnDeclaration_List();
						JE_ContainerMode = string.Empty;
					}
					else
					{
						if (HasCusContainers && JE_ContainerMode.IsEmpty)
						{
							JE_ContainerMode = GetDefaultContainerisedContainerMode();
						}
					}
				}
			}
		}

		public override ZDateTime JE_ExportDate
		{
			get { return base.JE_ExportDate; }
			set
			{
				bool hasChanged = base.JE_ExportDate != value;
				base.JE_ExportDate = value;
				if (hasChanged)
				{
					InvoiceLines.MarkAsNeedingValidation();
					if (IsExport)
					{
						SetEDITransmitDateIfRequired();
					}
				}
			}
		}

		public override ZDateTime JE_DateOfArrival
		{
			get { return base.JE_DateOfArrival; }
			set
			{
				bool hasChanged = base.JE_DateOfArrival != value;
				base.JE_DateOfArrival = value;
				if (hasChanged)
				{
					InvoiceLines.MarkAsNeedingValidation();
				}
			}
		}

		public override ZGuid JE_OH_Importer
		{
			get { return base.JE_OH_Importer; }
			set
			{
				bool hasChanged = base.JE_OH_Importer != value;
				base.JE_OH_Importer = value;
				if (hasChanged)
				{
					Invoices.MarkAsNeedingValidation();
				}
			}
		}

		public override ZString JE_RL_NKPortOfLoading
		{
			get { return base.JE_RL_NKPortOfLoading; }
			set
			{
				bool hasChanged = base.JE_RL_NKPortOfLoading != value;
				base.JE_RL_NKPortOfLoading = value;
				if (hasChanged && IsExport && JE_RL_NKProcessingPort.IsEmpty && Lookups.ProcessingPortList.ContainsCode(value))
				{
					JE_RL_NKProcessingPort = value;
				}

				if (hasChanged && !IsCopying)
				{
					RefreshDefaultGoodsLocation();
				}
			}
		}

		public override ZString JE_RL_NKPortOfArrival
		{
			get { return base.JE_RL_NKPortOfArrival; }
			set
			{
				bool hasChanged = base.JE_RL_NKPortOfArrival != value;
				base.JE_RL_NKPortOfArrival = value;
				if (hasChanged && IsImport && JE_RL_NKProcessingPort.IsEmpty && Lookups.ProcessingPortList.ContainsCode(value))
				{
					JE_RL_NKProcessingPort = value;
				}

				if (hasChanged && !IsCopying)
				{
					RefreshDefaultGoodsLocation();
				}
			}
		}

		public override ZString JE_RL_NKFinalDestination
		{
			get => base.JE_RL_NKFinalDestination;
			set
			{
				if (JE_RL_NKFinalDestination != value)
				{
					base.JE_RL_NKFinalDestination = value;

					if (!IsCopying)
					{
						RefreshDefaultGoodsLocation();
					}
				}
			}
		}

		public override ZString JE_MessageType
		{
			get { return base.JE_MessageType; }
			set
			{
				bool hasChanged = base.JE_MessageType != value;
				base.JE_MessageType = value;
				if (hasChanged && !settingDefaultValues)
				{
					JE_MessageTypeHasChanged();
					SetGoodsLocatedIfRequired();
				}
			}
		}

		public override ZGuid JE_GB
		{
			get { return base.JE_GB; }
			set
			{
				bool hasChanges = base.JE_GB != value;
				base.JE_GB = value;
				if (hasChanges)
				{
					Packages.MarkAsNeedingValidation();
				}
			}
		}

		public override ZGuid JE_GC
		{
			get { return base.JE_GC; }
			set
			{
				bool hasChanged = base.JE_GC != value;
				base.JE_GC = value;
				if (hasChanged)
				{
					Packages.MarkAsNeedingValidation();
				}
			}
		}

		public override ZString JE_MessageSubType
		{
			get { return base.JE_MessageSubType; }
			set
			{
				var typeOfCurrentEntryHeader = TypeOfEntryHeaderRequiredForCurrentDeclarationSettings;
				var oldValue = base.JE_MessageSubType;

				base.JE_MessageSubType = value;
				if (value != oldValue && !settingDefaultValues)
				{
					CustomsEntryHeaders.MarkAsNeedingValidation();
					JE_MessageSubTypeHasChanged(typeOfCurrentEntryHeader);
					ResetPaymentTypeIfChangingList(value);
					ActiveEntryHeaders.Rebuild();
				}
			}
		}

		public ZString MappedTSWMessageSubType
		{
			get
			{
				var result = IsExport ? MessageTypeList.Codes.E40 : MessageTypeList.Codes.I10;
				switch (JE_MessageSubType)
				{
					case JobMessageSubTypeList.Codes.Normal:
					case JobMessageSubTypeList.Codes.Completion:
						switch (JE_MessageType)
						{
							case JobMessageTypeList.Codes.Export:
								result = MessageTypeList.Codes.E40;
								break;
							case JobMessageTypeList.Codes.Import:
								result = MessageTypeList.Codes.I10;
								break;
						}
						break;
					case JobMessageSubTypeList.Codes.Simplified:
						result = MessageTypeList.Codes.I11;
						break;
					case JobMessageSubTypeList.Codes.Temporary:
						result = MessageTypeList.Codes.I51;
						break;
					case JobMessageSubTypeList.Codes.Sight:
						result = MessageTypeList.Codes.I52;
						break;
					case JobMessageSubTypeList.Codes.Periodic:
						result = MessageTypeList.Codes.I53;
						break;
					case "IPI":
						result = MessageTypeList.Codes.IPI;
						break;
					case JobMessageSubTypeList.Codes.Drawback:
						result = MessageTypeList.Codes.E41;
						break;
					case JobMessageSubTypeList.Codes.WriteOff:
						switch (JE_MessageType)
						{
							case JobMessageTypeList.Codes.Export:
								result = MessageTypeList.Codes.CRE;
								break;
							case JobMessageTypeList.Codes.Import:
								result = MessageTypeList.Codes.ICR;
								break;
						}
						break;
				}

				return result;
			}
		}

		public override ZString JE_DeclarationReference
		{
			get { return base.JE_DeclarationReference; }
			set
			{
				bool hasChanges = base.JE_DeclarationReference != value;
				base.JE_DeclarationReference = value;
				if (hasChanges)
				{
					CustomsEntryHeaders.Reload(false);
					CustomsEntryHeaders.MarkAsNeedingValidation();
				}
			}
		}

		public override ZString JE_GoodsDescription
		{
			get { return base.JE_GoodsDescription.ToUpper(); }
			set { base.JE_GoodsDescription = value.ToUpper(); }
		}

		internal static ZString GetWarningMessageIfCurrentEntryTotalAmountAndReturnedOneAreDifferent(CusEntryHeader cusEntryHeader)
		{
			if (cusEntryHeader.IsImport && !cusEntryHeader.CH_TotalAmountReturned.IsEmpty && cusEntryHeader.CH_TotalAmountReturned != cusEntryHeader.TotalAmountPayableIncludingEntryFee)
			{
				const string decimalFormat = "C";
				return string.Format(@"The total figure calculated for the current entry does not match the last figure returned from Customs.

Calculated Total: {0}
Total from Customs: {1}

This means that the values on this job are not in sync with the values currently lodged with Customs.
This can be caused by Customs making an 'Off the Page' modification where they make manual adjustments
to a Declaration at their end.

If this happens the System has no way of knowing what changes have been made and must be manually
updated with the same changes that Customs made before the right figures will be shown.",
									 cusEntryHeader.TotalAmountPayableIncludingEntryFee.ToString(decimalFormat),
									 cusEntryHeader.CH_TotalAmountReturned.ToString(decimalFormat));
			}
			return string.Empty;
		}

		#endregion

		#region IRatingSupporterWithAdapter

		protected override RatingAdaptersProvider GetRatingAdaptersProviderCore()
		{
			return new JobDeclarationRatingAdaptersProvider(this);
		}

		protected override IAutoRating GetRatingAdapterCore()
		{
			return new JobDeclarationRatingAdapter<JobDeclaration>(this);
		}

		#endregion

		#region New Properties

		[ResourceStringData("Enterprise.Customs.NZ.Business.Declaration.JobDeclaration|NZCSStatusDesc", Caption = "NZ Customs Status")]
		public ZString NZCSStatusDesc
		{
			get { return ExistingCusEntryHeader?.CH_NZCSStatusDesc ?? ZString.Empty; }
		}

		[ResourceStringData("Enterprise.Customs.NZ.Business.Declaration.JobDeclaration|MPIFoodStatusDesc", Caption = "MPI Food Status")]
		public ZString MPIFoodStatusDesc
		{
			get { return ExistingCusEntryHeader?.CH_MPIFoodStatusDesc ?? ZString.Empty; }
		}

		[ResourceStringData("Enterprise.Customs.NZ.Business.Declaration.JobDeclaration|MPIBiosecurityStatusDesc", Caption = "MPI Biosecurity Status")]
		public ZString MPIBiosecurityStatusDesc
		{
			get { return ExistingCusEntryHeader?.CH_MPIBioStatusDesc ?? ZString.Empty; }
		}

		public bool IsNZCSDeliveryOnPayment
		{
			get { return StatusList.IsDeliveryOnPayment(CusEntryHeader.CH_NZCSStatus); }
		}

		public bool IsNZCSEntryRestored
		{
			get
			{
				var result = false;
				if (IsTSWDeclaration)
				{
					result = CusEntryHeader.CH_NZCSStatus == StatusList.Codes.EntryRestored;
				}

				return result;
			}
		}

		public ZString CustomsMessageRemarks
		{
			get { return CusEntryHeader.CH_CustomsMessageRemarks; }
			set { CusEntryHeader.CH_CustomsMessageRemarks = value; }
		}

		public ZString CustomsDeliveryInstructions
		{
			get { return CustomsDOInstructions(CusEntryHeader); }
			set { CusEntryHeader.CH_CustomsDeliveryInstructions = value; }
		}
		ZString CustomsDOInstructions(CusEntryHeader entryHeader)
		{
			var result = ZString.Empty;
			if (entryHeader != null)
			{
				result = entryHeader.CH_CustomsDeliveryInstructions;
				if (!entryHeader.CH_NZCSStatus.IsEmpty)
				{
					var delInstruction = result.IsEmpty ? string.Empty : result + "\r\n";
					result = delInstruction + Lookups.StatusList.GetDescriptionFromCode(entryHeader.CH_NZCSStatus);
				}
			}

			return result.IsEmpty ? new ZString("NO DELIVERY ORDER HAS BEEN ISSUED FOR THESE GOODS") : result;
		}

		public ZString CustomsDeliveryInstructionsWithITR
		{
			get
			{
				var builder = new ZStringBuilder();
				builder.Append(CustomsDeliveryInstructions + "\r\n");
				var decTranshipmentRequest = TranshipmentRequest;
				if (decTranshipmentRequest != null)
				{
					if (IsImport)
					{
						var transitTo = decTranshipmentRequest.C4_RL_NKTranshipDestPort.IsEmpty ? decTranshipmentRequest.C4_OA_DestinationAddress_ZAddress.AddressFull : decTranshipmentRequest.C4_RL_NKTranshipDestPort;
						builder.Append("To:     " + transitTo);
					}

					builder.Append("Method of Transport of Transfer:     " + decTranshipmentRequest.Lookups.ModeOfMovement.GetDescriptionFromCode(decTranshipmentRequest.C4_ModeOfMovement));
					var voyageFlightDate = IsImport ? "     Date of Export: " + decTranshipmentRequest.C4_TranshipDepartureDate.ToString(DateFormat, CultureInfo.InvariantCulture)
													: "     Arrival Date: " + decTranshipmentRequest.C4_ArrivalDate.ToString(DateFormat, CultureInfo.InvariantCulture);

					if (!decTranshipmentRequest.C4_TranshipBySeaVoyage.IsEmpty)
					{
						var craftDirection = IsImport ? "Exporting Craft: " : "Incoming Craft: ";
						builder.Append(craftDirection + decTranshipmentRequest.C4_TranshipBySeaVessel + "     Voyage Number: " + decTranshipmentRequest.C4_TranshipBySeaVoyage + voyageFlightDate);
					}
					else if (!decTranshipmentRequest.C4_FlightNo.IsEmpty)
					{
						builder.Append("Flight Number: " + decTranshipmentRequest.C4_FlightNo + voyageFlightDate);
					}
				}

				return builder.ToStringWithNewLineBetweenAppends();
			}
		}
		const string DateFormat = "dd/MM/yyyy";

		#region Consolidated Declaration

		public ConsolidatedDeclaration ConsolidatedDec => (ConsolidatedDeclaration)ConsolidatedDeclaration.GetConsolidatedDeclaration(this);
		JobDeclaration LeadDeclaration => (JobDeclaration)ConsolidatedDec?.LeadDeclaration;
		CusEntryHeader LeadDecEntryHeader => LeadDeclaration?.CusEntryHeader;

		public ZString CustomsDeliveryInstructionsForConsolidatedDO => CustomsDOInstructions(LeadDecEntryHeader);

		protected override bool NeedRemoveChargeOnConsolidatedDeclaration(CusEntryHeaderCharges charge)
		{
			return charge.C1_ChargeType == EntryChargeTypeList.Codes.EntryFee || charge.C1_ChargeType == EntryChargeTypeList.Codes.EntryFeeGST;
		}

		#endregion

		public ZString BarrierPort
		{
			get
			{
				ZString result = ZString.Empty;
				if (IsImport)
				{
					result = JE_RL_NKPortOfArrival;
				}
				else if (IsExport)
				{
					result = JE_RL_NKPortOfLoading;
				}
				return result;
			}
		}

		public ZDateTime BarrierDate
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;
				if (IsImport)
				{
					result = JE_DateOfArrival;
				}
				else if (IsExport)
				{
					result = JE_ExportDate;
				}
				return result;
			}
		}

		public ZString EntryType
		{
			get { return Lookups.MessageTypeList.GetDescriptionFromCode(JE_MessageType); }
		}

		public ZString EntryStyle
		{
			get { return Lookups.MessageSubTypeList.GetDescriptionFromCode(JE_MessageSubType); }
		}

		public ZString EntryTypeAndStyle
		{
			get
			{
				ZString entryType = EntryType;
				ZString entryStyle = EntryStyle;
				return (entryType == entryStyle ? entryType : new ZString(entryType + " (" + entryStyle + ")"));
			}
		}

		public ZInt PackageCountFromPackagesCollection
		{
			get
			{
				int result = 0;
				foreach (Package package in Packages)
				{
					result += package.CW_PackQty;
				}
				return result;
			}
		}

		public string FormattedMasterBill
		{
			get
			{
				if (IsSea || JE_MasterBill.IndexOf("-") == 3 || JE_MasterBill == "")
				{
					return JE_MasterBill;
				}
				else
				{
					return JE_MasterBill.Left(3) + "-" + (JE_MasterBill.Length > 3 ? JE_MasterBill.Substring(3).ToString() : "");
				}
			}
		}

		public GlbStaff LastBrokerToSubmitOrBrokerSelectedOrCurrentUser
		{
			get
			{
				GlbStaff result = null;
				EDIMessage lastOutgoingMessage = CusEntryHeader.Messages.LastOutgoingMessage;
				if (lastOutgoingMessage != null)
				{
					result = lastOutgoingMessage.UserWhoQueuedThisRecord;
				}
				if (result == null)
				{
					result = CusAgent;
				}
				if (result == null)
				{
					result = GlbStaff.CurrentUser;
				}
				return result;
			}
		}

		public ZString PaymentMethod
		{
			get { return Lookups.PaymentPartyList.GetDescriptionFromCode(JE_PaymentMethod); }
		}

		public ZDecimal JE_DeclaredWeight
		{
			get
			{
				ZDecimal result = 0m;
				try
				{
					result = Core.Constants.Weight.Convert(JE_TotalWeight, JE_TotalWeightUnit, JE_DeclaredWeightUQ);
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{ }
				return result;
			}
		}

		public ZPropertyInfo JE_DeclaredWeightInfo
		{
			get { return GetZPropertyInfo(Schema.JE_DeclaredWeight); }
		}

		public ZString JE_DeclaredWeightUQ
		{
			get { return Core.Constants.Weight.Kilograms; }
		}

		public ZPropertyInfo JE_DeclaredWeightUQInfo
		{
			get { return GetZPropertyInfo(Schema.JE_DeclaredWeightUQ); }
		}

		#region JE_EDITransmitDate
		[ReadOnlyMember(nameof(JE_EDITransmitDate_ReadOnly))]
		public virtual ZDateTime JE_EDITransmitDate
		{
			get
			{
				return Factory.GetValue(ref cachedEDITransmitDate, GetCurrentEDITransmitDate);
			}
			set
			{
				if (value != JE_EDITransmitDate)
				{
					CusEntryHeader.CH_EDITransmitDate = value;
					if (!IsValidationSuspended)
					{
						Validation.ValidateJE_EDITransmitDate();
						foreach (PermitCode permitCode in PermitCodes)
						{
							permitCode.ValidateZO_Code();
						}
					}

					if (!IsTSW_IPI_Declaration)
					{
						Invoices.SetExchangeRate();
						MarkApportionmentDirty();
					}
				}

				JE_EDITransmitDateInfo.RefreshBinding();
			}
		}

		CachedProperty<ZDateTime> cachedEDITransmitDate;

		ZDateTime GetCurrentEDITransmitDate()
		{
			return CustomsEntryHeaders.Count == 0 ? ZDateTime.Empty : CusEntryHeader.CH_EDITransmitDate;
		}

		public ZPropertyInfo JE_EDITransmitDateInfo
		{
			get { return GetZPropertyInfo(Schema.JE_EDITransmitDate); }
		}

		public bool JE_EDITransmitDate_ReadOnly
		{
			get
			{
				return JE_EDITransmitDateFinalised && !CusEntryHeader.CH_IsRestored;
			}
		}

		public bool JE_EDITransmitDateFinalised
		{
			get
			{
				var query = new ZQuery(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Transmit);
				query.AddToFilter(EDIMessageSchema.EM_Status, SQLComparisonOperator.NotEqual, EDIMessage.Status.Cancelled);

				return JE_EDITransmitDate.IsValid
					&& (ConsolidatedDeclaration.IsConsolidated(this)
						? ConsolidatedDeclaration.GetConsolidatedDeclaration(this)?.Messages.Find(query).Any() ?? false // It may not be able to get linked consolidated declaration when this is an aggregate declaration
						: CusEntryHeader.Messages.Find(query).Any());
			}
		}

		#endregion

		public ZDateTime JE_ExchangeRateDate
		{
			get
			{
				if (IsCompletion && EntryHeaderForOriginalEntryNumber != null && EntryHeaderForOriginalEntryNumber.CH_EDITransmitDate.IsValid)
				{
					return EntryHeaderForOriginalEntryNumber.CH_EDITransmitDate;
				}
				else
				{
					return JE_EDITransmitDate.IsValid ? JE_EDITransmitDate : CachedTodaysDate;
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.GoodsLocationList))]
		[MaxLength(Schema.JE_LocationOfGoodsMaxLength)]
		public ZString JE_Cal_GoodsLocation
		{
			get
			{
				var result = JE_LocationOfGoods;

				if (result.IsEmpty)
				{
					result = GetDefaultGoodsLocationCore();
				}

				return result;
			}
			set
			{
				if (JE_LocationOfGoods != value)
				{
					JE_LocationOfGoods = value;
					Validation.ValidateJE_Cal_GoodsLocation();

					if (!IsCopying)
					{
						JE_Cal_GoodsLocationInfo.RefreshBinding();
					}
				}
			}
		}

		public bool JE_Cal_GoodsLocation_ReadOnly => JE_GoodsLocatedAt == GoodsLocatedAtListForSeaImport.Codes.DES || JE_GoodsLocatedAt == GoodsLocatedAtListForSeaImport.Codes.DIS || JE_GoodsLocatedAt == GoodsLocatedAtListForSeaExport.Codes.PC;

		public ZPropertyInfo JE_Cal_GoodsLocationInfo => GetZPropertyInfo(nameof(JE_Cal_GoodsLocation));

		void RefreshDefaultGoodsLocation()
		{
			JE_Cal_GoodsLocation = GetDefaultGoodsLocationCore();
			MarkAsNeedingValidation();
		}

		ZString GetDefaultGoodsLocationCore()
		{
			ZString result;

			// Kingy advises that by default Customs assume the goods location is Port of Discharge i.e. GoodsLocatedAtListForSeaImport.Codes.DIS
			// it is not required to be sent in the message

			/*
			 * Update Jan 2020 - WI00285273
			 *	In NZ Declaration when shipment is SeaFreight there are 2 Goods Location options. DIS (for Imports) and PC (exports).
			 *	Currently when either of these codes are selected NZ Customs does not expect the UNLOCODE port code to be written to the <GoodsLocation><ID>, however Customs will change this to be mandatory in the future.
			 *	
			 */

			if (JE_GoodsLocatedAt == GoodsLocatedAtListForSeaImport.Codes.DES)
			{
				result = JE_RL_NKFinalDestination;
			}
			else if (JE_GoodsLocatedAt == GoodsLocatedAtListForSeaImport.Codes.DIS)
			{
				result = JE_RL_NKPortOfArrival; // "Port of Discharge" for ImportSea
			}
			else if (JE_GoodsLocatedAt == GoodsLocatedAtListForSeaExport.Codes.PC)
			{
				result = JE_RL_NKPortOfLoading;
			}
			else
			{
				var header = GetGoodsLocationOrg();
				var address = header != null ? GetGoodsLocationAddressPK() : ZGuid.Empty;

				var defaultGoodsLocation = header != null ? GetCustomsCodeForAddress(header, address, OrgCusCode.CodeTypes.ControlledPremisesID) : ZString.Empty;
				result = defaultGoodsLocation.SubstringSafe(0, Schema.JE_Cal_GoodsLocationMaxLength);
			}

			return result;
		}

		protected override void WarehouseDocAddress_DocAddressChanged(object sender, EventArgs e)
		{
			base.WarehouseDocAddress_DocAddressChanged(sender, e);
			RefreshDefaultGoodsLocation();
		}

		protected override void ContainerYardDocAddress_ValueChanged(object sender, EventArgs e)
		{
			base.ContainerYardDocAddress_ValueChanged(sender, e);
			RefreshDefaultGoodsLocation();
		}

		#endregion

		#region ICurrencyConverterDataProvider Members

		public override ZDateTime DateOfValuation
		{
			get { return JE_ExchangeRateDate; }
		}

		ExchangeRateType ICurrencyConverterDataProvider.RateType
		{
			get { return ZArchitecture.Core.ExchangeRateType.Customs; }
		}

		int ICurrencyConverterDataProvider.MaximumDaysToFallback
		{
			get
			{
				return 0; // no fall back is allowed for NZ
			}
		}

		ZDateTime ICurrencyConverterDataProvider.DateOfValuation
		{
			get { return DateOfValuation; }
		}
		#endregion

		#region AddInfo and Associated Child Collections/New Fields
		NZAddInfo fAddInfo;
		NZAddInfo AddInfo
		{
			get
			{
				if (fAddInfo == null)
				{
					fAddInfo = new NZAddInfo(this, JE_AddInfoInfo);
					fAddInfo.LoadPropertiesFromString(JE_AddInfo);
					RegisterEditableChildObject(fAddInfo);
					RegisterListChangedCalledRefreshBinding(fAddInfo);
				}
				return fAddInfo;
			}
		}

		NZAddInfo IHaveNZAddInfo.AddInfo
		{
			get { return AddInfo; }
		}

		public HeaderOtherInfoCollection OtherInfos
		{
			get { return AddInfo.HeaderOtherInfos; }
		}

		public PermitCodeCollection PermitCodes
		{
			get { return AddInfo.PermitCodes; }
		}

		[UniversalCopyAddInfoPropertyMapping(AutoNZAddInfo.Schema.ZN_SoldOrConsigned)]
		public ZString JE_SoldOrConsigned
		{
			get { return AddInfo.ZN_SoldOrConsigned; }
			set { AddInfo.ZN_SoldOrConsigned = value; }
		}

		public ZPropertyInfo JE_SoldOrConsignedInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.JE_SoldOrConsigned, x => AddInfo.ZN_SoldOrConsignedInfo); }
		}

		[UniversalCopyAddInfoPropertyMapping(AutoNZAddInfo.Schema.ZN_RL_NKProcessingPort)]
		public ZString JE_RL_NKProcessingPort
		{
			get { return AddInfo.ZN_RL_NKProcessingPort; }
			set { AddInfo.ZN_RL_NKProcessingPort = value; }
		}

		public ZPropertyInfo JE_RL_NKProcessingPortInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.JE_RL_NKProcessingPort, x => AddInfo.ZN_RL_NKProcessingPortInfo); }
		}

		public bool ProcessingPortVisible => !IsTSWDeclaration;

		[ResourceStringData("Enterprise.Customs.NZ.Business.Declaration.JobDeclaration|JE_MAF_ConsignmentNumber", Caption = "MPI Consignment Number")]
		public ZString JE_MAF_ConsignmentNumber
		{
			get { return AddInfo.ZN_MAF_ConsignmentNumber; }
		}

		public ZPropertyInfo JE_MAF_ConsignmentNumberInfo
		{
			get { return GetZPropertyInfo(Schema.JE_MAF_ConsignmentNumber); }
		}

		[ResourceStringData("Enterprise.Customs.NZ.Business.Declaration.JobDeclaration|JE_MAF_MessagingStatusDescription", Caption = "MPI Status")]
		public ZString JE_MAF_MessagingStatusDescription
		{
			get { return Lookups.MessagingStatuses.GetDescriptionFromCode(AddInfo.ZN_MAF_MessagingStatus); }
		}

		public ZPropertyInfo JE_MAF_MessagingStatusDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.JE_MAF_MessagingStatusDescription); }
		}

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.TransactionNatureList))]
		[UniversalCopyAddInfoPropertyMapping(AutoNZAddInfo.Schema.ZN_TransactionNature)]
		public ZString JE_TransactionNature
		{
			get { return AddInfo.ZN_TransactionNature; }
			set { AddInfo.ZN_TransactionNature = value; }
		}

		public ZPropertyInfo JE_TransactionNatureInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.JE_TransactionNature, x => AddInfo.ZN_TransactionNatureInfo); }
		}

		public ZBool JE_TransactionNatureVisible
		{
			get { return IsTSWDeclaration; }
		}

		[ResourceStringData("Enterprise.Customs.NZ.Business.Declaration.JobDeclaration|JE_TSWCombinedStatus", Caption = "TSW Status")]
		public ZString JE_TSWCombinedStatus
		{
			get { return AddInfo.ZN_TSWCombinedStatus; }
			set
			{
				bool hasChanges = JE_TSWCombinedStatus != value;
				var bioStatus = CusEntryHeader != null && CusEntryHeader.IsActive ? CusEntryHeader.CH_MPIBioStatus : ZString.Empty;
				if (hasChanges || bioStatus == StatusList.Codes.MPIBiosecurityDirectionsGivenCleared)
				{
					if (IsTSWDeclaration && !value.IsEmpty)
					{
						ZString entryType = IsTSWWriteOff ? TSWEntryStatusList.EntryTypes.WriteOff : IsPrimaryIndustriesImportDeclaration ? MessageTypeList.Codes.IPI : "";
						this.LogCustomsEventsIfRequired(JE_TSWCombinedStatus, value, AgencyMessageBeingProcessed, entryType, bioStatus, IsExport, ClearanceEventReference);
					}

					AddInfo.ZN_TSWCombinedStatus = value;
				}
			}
		}

		public ZPropertyInfo JE_TSWCombinedStatusInfo => GetWrappedZPropertyInfo(Schema.JE_TSWCombinedStatus, x => AddInfo.ZN_TSWCombinedStatusInfo);

		public ZString AgencyMessageBeingProcessed
		{
			get { return fAgencyMessageBeingProcessed; }
			set
			{
				string result = ResponsibleGovernmentAgencyList.Codes.TSW;
				switch (value)
				{
					case ResponsibleGovernmentAgencyList.Codes.MPIBIO:
						result = TSWMessage.ResponseMessage.MessageSubTypes.MPIBIO;
						break;
					case ResponsibleGovernmentAgencyList.Codes.MPIFOOD:
						result = TSWMessage.ResponseMessage.MessageSubTypes.MPIFood;
						break;
					case ResponsibleGovernmentAgencyList.Codes.NZCS:
						result = TSWMessage.ResponseMessage.MessageSubTypes.NZCustoms;
						break;
				}

				fAgencyMessageBeingProcessed = result;
			}
		}
		ZString fAgencyMessageBeingProcessed;

		public ZBool JE_TSWCombinedStatusVisible
		{
			get { return IsTSWDeclaration; }
		}

		[ResourceStringData("Enterprise.Customs.NZ.Business.Declaration.JobDeclaration|JE_TSWCombinedStatusDesc", Caption = "TSW Combined Agency Status Desc.")]
		public ZString JE_TSWCombinedStatusDesc
		{
			get { return JE_TSWCombinedStatus.IsEmpty ? string.Empty : Lookups.TSWEntryStatusList.GetDescriptionFromCode(JE_TSWCombinedStatus); }
		}

		public ZBool IPIDeclarationFromFormalDec
		{
			get { return FormalEntryExists && (IsTSW_IPI_Declaration || IPIEntryAlreadyExists); }
		}

		public bool FormalEntryExists => Factory.GetValue(ref formalEntryExists, delegate
		{
			bool result = DoesFormalCusEntryHeaderExist;
			return result;
		});

		CachedProperty<bool> formalEntryExists;

		bool DoesFormalCusEntryHeaderExist
		{
			get
			{
				bool result = false;
				foreach (CusEntryHeader entryHeader in CustomsEntryHeaders)
				{
					if (entryHeader.IsFormalEntry)
					{
						result = true;
						break;
					}
				}

				return result;
			}
		}

		public bool IPIEntryAlreadyExists => Factory.GetValue(ref ipiEntryAlreadyExists, delegate
		{
			bool result = DoesIPIEntryExists;
			return result;
		});

		CachedProperty<bool> ipiEntryAlreadyExists;

		bool DoesIPIEntryExists
		{
			get
			{
				var result = false;
				foreach (CusEntryHeader entryHeader in CustomsEntryHeaders)
				{
					if (entryHeader.IsIPIEntryHeader)
					{
						result = true;
						break;
					}
				}

				return result;
			}
		}

		public bool IsTSWCancellation
		{
			get
			{
				return (
						JE_TSWCombinedStatus == LowValueConsignmentStatusList.Codes.ConsignmentCancelled ||
						JE_TSWCombinedStatus == TSWEntryStatusList.Codes.CAN ||
						JE_TSWCombinedStatus == TSWEntryStatusList.Codes.DCC ||     // Declaration Cancelled
						JE_TSWCombinedStatus == TSWEntryStatusList.Codes.DCP ||     // Declaration Cancellation Pending
						JE_TSWCombinedStatus == TSWEntryStatusList.Codes.DCA ||     // Declaration Cancellation Acknowledged
						JE_TSWCombinedStatus == TSWEntryStatusList.Codes.DCI);      // Declaration Cancellation Inspection Required
			}
		}

		public bool IsTSWCancelledOrPendingCancellation
		{
			get
			{
				return (
						JE_TSWCombinedStatus == LowValueConsignmentStatusList.Codes.ConsignmentCancelled ||
						JE_TSWCombinedStatus == TSWEntryStatusList.Codes.CAN ||
						JE_TSWCombinedStatus == TSWEntryStatusList.Codes.DCC ||     // Declaration Cancelled
						JE_TSWCombinedStatus == TSWEntryStatusList.Codes.DCP);      // Declaration Cancellation Pending
			}
		}

		public bool IsTSWCREWriteOff
		{
			get { return IsTSWDeclaration && IsExport && IsECIWriteoff; }
		}

		public bool IsTSWEmptyContainerWriteOff
		{
			get { return IsTSWWriteOff && HasContainersAndTheyreAllEmpty; }
		}

		public bool IsTSWCustomsCleared
		{
			get { return JE_TSWCombinedStatus.SubstringSafe(0, 1) == "0"; }
		}

		public bool IsTSWICRWriteOff
		{
			get { return IsTSWDeclaration && IsImport && IsECIWriteoff; }
		}

		public bool IsTSWWriteOff
		{
			get { return IsTSWICRWriteOff || IsTSWCREWriteOff; }
		}

		public bool TSWSimplifiedMiscEntry
		{
			get { return IsTSWDeclaration && IsSimplified && IsMiscellaneousImporter; }
		}

		public bool IsMiscellaneousImporter
		{
			get { return Importer != null && Importer.PK == CachedMiscOrgPK; }
		}

		#endregion

		#region New and Overridden Boolean Flags

		protected override bool IsCustomsHeaderAmendmentATotalReplacement
		{
			get { return false; }
		}

		protected override bool IsCustomsLineAmendmentATotalReplacement
		{
			get { return true; }
		}

		protected override bool SupportJE_PaymentMethodUsageCore
		{
			get { return true; }
		}

		public bool IsInTestMode
		{
			get
			{
				bool result = true;

				if (IsECIWriteoff)
				{
					if (IsImport)
					{
						result = NZCustomsDataRegistry.Instance.ImportEciTestMode.Value;
					}
					else if (IsExport)
					{
						result = NZCustomsDataRegistry.Instance.ExportEciTestMode.Value;
					}
				}
				else
				{
					if (IsImport || IsExcise)
					{
						result = NZCustomsDataRegistry.Instance.ImportDeclarationsTestMode.Value;
					}
					else if (IsExport)
					{
						result = NZCustomsDataRegistry.Instance.ExportDeclarationsTestMode.Value;
					}
				}

				return result;
			}
		}

		public bool IsECIWriteoff
		{
			get { return JE_MessageSubType == JobMessageSubTypeList.Codes.WriteOff; }
		}

		public bool IsFormalEntry
		{
			get { return !IsECIWriteoff; }
		}

		public bool IsQuarantineGroupBoxVisible => IsTSWICRWriteOff || (!IsECIWriteoff && !(IsTSWDeclaration && IsExport));

		public bool HasContainersAndTheyreAllEmpty
		{
			get
			{
				bool result = false;
				foreach (CusContainer container in CusContainers)
				{
					result = (container.CO_FCL_LCL_AIR == ContainerModeList.Codes.Empty);
					if (!result)
					{
						break;
					}
				}
				return result;
			}
		}

		public bool LastCustomsStatusIsImpediment
		{
			get
			{
				bool result = false;
				if (IsECIWriteoff)
				{
					result = LowValueConsignmentStatusList.LastStatusIsImpediment(JE_EntryStatus);
				}
				else
				{
					switch (JE_EntryStatus)
					{
						case FormalEntryStatusList.Codes.EntryRejected:
						case FormalEntryStatusList.Codes.EntryInError:
						case FormalEntryStatusList.Codes.InspectionsAuditRequirements:
							result = true;
							break;
					}
				}
				return result;
			}
		}

		public override bool HasCustomsMessages
		{
			get { return ExistingCusEntryHeader?.HasNonCancelledMessages ?? false; }
		}

		public bool HasCancellationPending
		{
			get
			{
				return JE_TSWCombinedStatus == TSWEntryStatusList.Codes.DCP ||
					   JE_TSWCombinedStatus == TSWEntryStatusList.Codes.DCA;
			}
		}

		public bool IsInternationalTranshipmentApproved => JE_EntryStatus == ConsignmentGoodsStatusList.Codes.InternationalTranshipmentApproved;

		public bool IsExcise
		{
			get { return JE_MessageType == JobMessageTypeList.Codes.Excise; }
		}

		public bool IsPrimaryIndustriesImportDeclaration
		{
			get { return JE_MessageSubType == MessageTypeList.Codes.IPI; }
		}

		public bool IsPeriodic
		{
			get { return JE_MessageSubType == JobMessageSubTypeList.Codes.Periodic; }
		}

		public bool IsTemporary
		{
			get { return JE_MessageSubType == JobMessageSubTypeList.Codes.Temporary; }
		}

		public bool IsSight
		{
			get { return JE_MessageSubType == JobMessageSubTypeList.Codes.Sight; }
		}

		public bool IsBond
		{
			get { return !WarehouseDocAddress.E2_OA_Address.IsEmpty; }
		}

		public bool IsDutyDeminimus
		{
			get { return CusEntryHeader.TotalAmountPayable < 50.00m; }
		}

		public bool IsPrivateImport
		{
			get { return OtherInfos.AggregatedCodes.Contains(HeaderOtherInfoList.Codes.PrivateImportTransactionFee); }
		}

		public bool IsDiplomatic
		{
			get { return OtherInfos.AggregatedCodes.Contains(HeaderOtherInfoList.Codes.DiplomaticPrivilege); }
		}

		public bool IsPeriodicDrawback
		{
			get { return IsExport && IsDrawback && OtherInfos.AggregatedCodes.Contains(HeaderOtherInfoList.Codes.PeriodicDrawbackEntry); }
		}

		internal bool FlightNoIndicatesPeriodic
		{
			get { return PeriodicFlightNos.Contains("~" + JE_VoyageFlightNo + "~"); }
		}
		internal const string PeriodicFlightNos = "~PD0001~PD0002~PD0003~PD0004~PD0005~PD0006~PD0007~PD0008~PD0009~PD0010~PD0011~PD0012~";

		[ResourceStringData("847129F3-6311-43CC-9535-6D7663B80DE8", Caption = "Flight Number", IsApplicableMember = nameof(IsAir))]
		public override ZString JE_VoyageFlightNo
		{
			get { return base.JE_VoyageFlightNo; }
			set
			{
				ZString oldValue = base.JE_VoyageFlightNo;
				base.JE_VoyageFlightNo = value;
				if (value != oldValue)
				{
					if (IsExport && IsDrawback && FlightNoIndicatesPeriodic && !OtherInfos.AggregatedCodes.Contains(HeaderOtherInfoList.Codes.PeriodicDrawbackEntry))
					{
						OtherInfos.AddNew(HeaderOtherInfoList.Codes.PeriodicDrawbackEntry, "");
					}
				}
			}
		}

		public override bool HasSpecialFlightTerm => JE_VoyageFlightNo == "VARIOUS";

		internal bool VesselIndicatesPeriodic
		{
			get { return JE_VesselName == PeriodicVessel; }
		}
		internal const string PeriodicVessel = "PERIODIC VARIOUS";

		public override ZString JE_VesselName
		{
			get { return base.JE_VesselName; }
			set
			{
				ZString oldValue = base.JE_VesselName;
				base.JE_VesselName = value;
				if (value != oldValue)
				{
					if (IsExport && IsDrawback && VesselIndicatesPeriodic && !OtherInfos.AggregatedCodes.Contains(HeaderOtherInfoList.Codes.PeriodicDrawbackEntry))
					{
						OtherInfos.AddNew(HeaderOtherInfoList.Codes.PeriodicDrawbackEntry, "");
					}
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public ZString CalculateCombinedJobStatus(ZString bioStatus, ZString customsStatus)
		{
			var jobStatus = ZString.Empty;
			if (bioStatus.IsEmpty || customsStatus.IsEmpty)
			{
				jobStatus = LowValueConsignmentStatusList.Codes.AgencyResponsePending;   // Overall Job status can only be determined when BOTH agencies have responded
			}
			else if (bioStatus == LowValueConsignmentStatusList.Codes.ConsignmentWrittenOff && customsStatus == LowValueConsignmentStatusList.Codes.ConsignmentWrittenOff)
			{
				jobStatus = LowValueConsignmentStatusList.Codes.ConsignmentWrittenOff;  // WOF status should only be shown if BOTH agencies have written off the consignment!
			}
			else if (bioStatus == LowValueConsignmentStatusList.Codes.ConsignmentCancelled || customsStatus == LowValueConsignmentStatusList.Codes.ConsignmentCancelled || customsStatus == StatusList.Codes.EntryCancelled)
			{
				jobStatus = LowValueConsignmentStatusList.Codes.ConsignmentCancelled;
			}
			else if (bioStatus == LowValueConsignmentStatusList.Codes.ConsignmentInError || customsStatus == LowValueConsignmentStatusList.Codes.ConsignmentInError)
			{
				jobStatus = LowValueConsignmentStatusList.Codes.ConsignmentInError;
			}
			else if (bioStatus == LowValueConsignmentStatusList.Codes.ConsignmentHeld || customsStatus == LowValueConsignmentStatusList.Codes.ConsignmentHeld)
			{
				jobStatus = LowValueConsignmentStatusList.Codes.ConsignmentHeld;
			}
			else if (bioStatus == LowValueConsignmentStatusList.Codes.RescindPreviousStatusNotification || customsStatus == LowValueConsignmentStatusList.Codes.RescindPreviousStatusNotification)
			{
				jobStatus = LowValueConsignmentStatusList.Codes.RescindPreviousStatusNotification;
			}
			else if (bioStatus == LowValueConsignmentStatusList.Codes.FormalDeclarationRequired || customsStatus == LowValueConsignmentStatusList.Codes.FormalDeclarationRequired)
			{
				jobStatus = LowValueConsignmentStatusList.Codes.FormalDeclarationRequired;
			}
			else if (bioStatus == LowValueConsignmentStatusList.Codes.MpiImportDecRequired || customsStatus == LowValueConsignmentStatusList.Codes.MpiImportDecRequired)
			{
				jobStatus = LowValueConsignmentStatusList.Codes.MpiImportDecRequired;
			}
			else if (bioStatus == LowValueConsignmentStatusList.Codes.ImportDeclarationRequired || customsStatus == LowValueConsignmentStatusList.Codes.ImportDeclarationRequired)
			{
				jobStatus = LowValueConsignmentStatusList.Codes.ImportDeclarationRequired;
			}
			else if (bioStatus == LowValueConsignmentStatusList.Codes.ConsolidationIcrRequired || customsStatus == LowValueConsignmentStatusList.Codes.ConsolidationIcrRequired)
			{
				jobStatus = LowValueConsignmentStatusList.Codes.ConsolidationIcrRequired;
			}
			else if (bioStatus == LowValueConsignmentStatusList.Codes.InternationalTranshipmentDeclined || customsStatus == LowValueConsignmentStatusList.Codes.InternationalTranshipmentDeclined)
			{
				jobStatus = LowValueConsignmentStatusList.Codes.InternationalTranshipmentDeclined;
			}
			else if (bioStatus == LowValueConsignmentStatusList.Codes.AgencyResponsePending || customsStatus == LowValueConsignmentStatusList.Codes.AgencyResponsePending ||
				 bioStatus == LowValueConsignmentStatusList.Codes.NoStatusReported || customsStatus == LowValueConsignmentStatusList.Codes.NoStatusReported)
			{
				jobStatus = LowValueConsignmentStatusList.Codes.AgencyResponsePending;
			}
			else if (bioStatus == LowValueConsignmentStatusList.Codes.InternationalTranshipmentApproved || customsStatus == LowValueConsignmentStatusList.Codes.InternationalTranshipmentApproved)
			{
				jobStatus = LowValueConsignmentStatusList.Codes.InternationalTranshipmentApproved;
			}

			return jobStatus;
		}

		public bool ContainsThisOtherInfoCode(string code)
		{
			return OtherInfos.AggregatedCodes.Contains(code);
		}

		public override bool IsDrawback
		{
			get { return JE_MessageSubType == JobMessageSubTypeList.Codes.Drawback; }
		}

		public bool IsCompletion
		{
			get { return JE_MessageSubType == JobMessageSubTypeList.Codes.Completion; }
		}

		public ZString CompletionEntryNumber
		{
			get { return IsCompletion && CusEntryHeader.CusEntryNumber != null ? CusEntryHeader.CusEntryNumber.CE_EntryNum : ZString.Empty; }
		}

		public bool IsWriteOffChangedToFormal
		{
			get
			{
				var result = false;
				if (IsTSWDeclaration && IsFormalEntry)
				{
					result = EntryHeaderForOriginalEntryNumber == null && EntryHeaderForWriteOffEntryNumber != null;
				}

				return result;
			}
		}

		public bool IsFormalChangedToWriteOff
		{
			get
			{
				return IsECIWriteoff && EntryHeaderForOriginalEntryNumber != null;
			}
		}

		public bool IsCRE
		{
			get { return IsTSWDeclaration && IsExport && JE_MessageSubType == JobMessageSubTypeList.Codes.WriteOff; }
		}

		public bool IsTSWDeclaration
		{
			get { return JE_ApplicationCode == JobApplicationCodeList.Codes.TSW || IsInterface; }
		}

		public bool IsTSWExportDeclaration
		{
			get { return IsTSWDeclaration && IsExport; }
		}

		public bool IsTSWImportDeclaration
		{
			get { return IsTSWDeclaration && IsImport; }
		}

		public bool IsTSW_IPI_Declaration
		{
			get { return IsTSWDeclaration && IsPrimaryIndustriesImportDeclaration; }
		}

		public OrgHeader GetGoodsLocationOrg()
		{
			switch (JE_GoodsLocatedAt)
			{
				case GoodsLocatedAtList.Codes.BW:
					return WarehouseDocAddress.Organisation;
				case GoodsLocatedAtList.Codes.C:
					return ShippingLine;
				case GoodsLocatedAtList.Codes.CTO:
					return ContainerTerminalOperatorDocAddress.Organisation;
				case GoodsLocatedAtList.Codes.CY:
					return ContainerYardDocAddress.Organisation;
				case GoodsLocatedAtList.Codes.D:
					return DepotDocAddress.Organisation;
				case GoodsLocatedAtList.Codes.FW:
					return Forwarder;
				default:
					return null;
			}
		}

		public ZGuid GetGoodsLocationAddressPK()
		{
			switch (JE_GoodsLocatedAt)
			{
				case GoodsLocatedAtList.Codes.BW:
					return WarehouseDocAddress.E2_OA_Address;
				case GoodsLocatedAtList.Codes.CTO:
					return ContainerTerminalOperatorDocAddress.E2_OA_Address;
				case GoodsLocatedAtList.Codes.CY:
					return ContainerYardDocAddress.E2_OA_Address;
				case GoodsLocatedAtList.Codes.D:
					return DepotDocAddress.E2_OA_Address;
				case GoodsLocatedAtList.Codes.C:
				case GoodsLocatedAtList.Codes.FW:
				default:
					return ZGuid.Empty;
			}
			//	JE_GoodsLocatedAt == GoodsLocatedAtList.Codes.FW
			//	JE_GoodsLocatedAt == GoodsLocatedAtList.Codes.C
			//	Forwarder & Shipping line (Carrier) uses org only
		}

		public ZString GetCustomsCodeForAddress(OrgHeader organisation, ZGuid addressToMatch, ZString customsCode)
		{
			var result = ZString.Empty;
			if (organisation != null)
			{
				foreach (OrgCusCode cusCode in organisation.CustomsCodes.GetOrgCusCodesForCodeAndCountry(customsCode, Core.Constants.CountryCodes.NewZealand))
				{
					if (cusCode.OK_OA_PremisesAddress == addressToMatch)
					{
						result = cusCode.OK_CustomsRegNo;
						break;
					}
					else if (cusCode.OK_OA_PremisesAddress.IsEmpty || cusCode.OK_OA_PremisesAddress == organisation.MainAddress.PK)    // default code for all non specific addresses
					{
						result = cusCode.OK_CustomsRegNo;
					}
				}
			}

			return result;
		}

		public bool IsExportedUnderSecureExportPartnershipScheme
		{
			get { return (IsFormalEntry && OtherInfos.AggregatedCodes.Contains(HeaderOtherInfoList.Codes.SecureExportPartnership)); }
		}

		public bool IsSimplified
		{
			get { return JE_MessageSubType == JobMessageSubTypeList.Codes.Simplified; }
		}

		public bool IsNormal
		{
			get { return JE_MessageSubType == JobMessageSubTypeList.Codes.Normal; }
		}

		public bool IsEntryStyleUnPayable => IsSimplified || IsNormal;

		public bool IsVFDUnderDeminimus
		{
			get
			{
				var totalCustomsValue = TotalFOBInLocalCurrency.Amount.Round(0);    // This is kept for the multitude of duty and charges test cases that do not use entryHeader/Lines and merge for CusEntryLines
				if ((ExistingCusEntryHeader?.MergedLines?.Count ?? 0) > 0)
				{
					totalCustomsValue = CusEntryHeader.MergedLines.Cast<CusEntryLine>().Sum(x => x.CL_CustomsValue.Round(0));
				}

				return totalCustomsValue <= VFDDeminimus;
			}
		}

		public ZDecimal VFDDeminimus => Factory.GetCachedValue("NZ.JobDeclration.VDFPayableLimitation" + JE_DateOfArrival, () => UniversalReferenceHelper.GetTaxOrFee(Factory, RateTypes.Deminimus, JE_DateOfArrival));

		public override ZBool IsPost
		{
			get { return JE_TransportMode == JobTransportModeList.Codes.Post; }
		}

		public bool IsAttachedToShipment => Shipment != null;

		public IDisposable TemporarilySetTSWMessagingValidation()
		{
			return new DisposableAction(() => fTSWMessagingValidationIndex++, () => fTSWMessagingValidationIndex--);
		}
		int fTSWMessagingValidationIndex;

		public bool IsTSWMessagingValidation
		{
			get { return fTSWMessagingValidationIndex > 0; }
		}

		public ZBool EntryFeeUnPayable => IsImport && IsEntryStyleUnPayable && IsVFDUnderDeminimus;

		public ZBool IsECIWriteoffAndGSTIsApplicable => IsECIWriteoff && IsImport;

		#endregion

		#region MiscData

		#region Payment Details

		[UniversalCopyAddInfoPropertyMapping(AutoNZAddInfo.Schema.ZN_MPIAccountNumber)]
		public ZString ZX_AccountNumber
		{
			get { return AddInfo.ZN_MPIAccountNumber; }
			set
			{
				AddInfo.ZN_MPIAccountNumber = value;
				if (!value.IsEmpty)
				{
					ZX_PaymentMethod = MAFPaymentMethodList.Codes.Account;
				}
			}
		}

		public ZPropertyInfo ZX_AccountNumberInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ZX_AccountNumber, x => AddInfo.ZN_MPIAccountNumberInfo); }
		}

		[UniversalCopyAddInfoPropertyMapping(AutoNZAddInfo.Schema.ZN_MPIAccountHolder)]
		public ZString ZX_AccountHolder
		{
			get { return AddInfo.ZN_MPIAccountHolder; }
			set
			{
				AddInfo.ZN_MPIAccountHolder = value;
				if (!value.IsEmpty)
				{
					ZX_PaymentMethod = MAFPaymentMethodList.Codes.Account;
				}
			}
		}

		public ZPropertyInfo ZX_AccountHolderInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ZX_AccountHolder, x => AddInfo.ZN_MPIAccountHolderInfo); }
		}

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.MPIPaymentMethods))]
		[UniversalCopyAddInfoPropertyMapping(AutoNZAddInfo.Schema.ZN_MPIPaymentMethod)]
		public ZString ZX_PaymentMethod
		{
			get { return AddInfo.ZN_MPIPaymentMethod; }
			set
			{
				AddInfo.ZN_MPIPaymentMethod = value;
				if (value != MAFPaymentMethodList.Codes.Account)
				{
					ZX_AccountNumber = ZString.Empty;
					ZX_AccountHolder = ZString.Empty;
				}
			}
		}

		public ZPropertyInfo ZX_PaymentMethodInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ZX_PaymentMethod, x => AddInfo.ZN_MPIPaymentMethodInfo); }
		}

		public ZString PaymentTypeToBeSent
		{
			get
			{
				return !IsPaymentMethodOverriden && IsAccountDetailsSet(PlugInSupport.AccountDetails) ? (ZString)MAFPaymentMethodList.Codes.Account
						: ZX_PaymentMethod.IsEmpty ? (ZString)MAFPaymentMethodList.Codes.Cash : ZX_PaymentMethod;
			}
		}

		public ZString AccountNumberToBeSent
		{
			get { return IsPaymentMethodOverriden ? ZX_AccountNumber : PlugInSupport.AccountDetails.AccountNumber; }
		}

		public ZString AccountHolderNameToBeSent
		{
			get { return IsPaymentMethodOverriden ? ZX_AccountHolder : PlugInSupport.AccountDetails.AccountHolderName; }
		}

		public ZString PaymentDetailsSent
		{
			get
			{
				var result = new ZStringBuilder();
				string paymentTypeToBeSent = PaymentTypeToBeSent;
				result.Append(paymentTypeToBeSent + " - " + Lookups.MPIPaymentMethods.GetDescriptionFromCode(paymentTypeToBeSent));

				if (!IsPaymentMethodOverriden)
				{
					var accountDetails = PlugInSupport.AccountDetails;
					if (!IsAccountDetailsSet(accountDetails))
					{
						result.Append("NB: " + accountDetails.WhereToSetupAccountDetailsDescription);
					}
					else
					{
						result.Append(accountDetails.AccountNumber + " / " + accountDetails.AccountHolderName);
						result.Append("NB: Defaulted from " + accountDetails.AccountHolderDescription + ".");
					}
				}
				else if (paymentTypeToBeSent == MAFPaymentMethodList.Codes.Account)
				{
					if (!ZX_AccountNumber.IsEmpty && !ZX_AccountHolder.IsEmpty)
					{
						result.Append(ZX_AccountNumber + " / " + ZX_AccountHolder);
					}
				}
				return result.ToStringWithNewLineBetweenAppends();
			}
		}

		bool IsPaymentMethodOverriden
		{
			get
			{
				return (!ZX_PaymentMethod.IsEmpty && ZX_PaymentMethod != MAFPaymentMethodList.Codes.Account)
					   || !ZX_AccountNumber.IsEmpty || !ZX_AccountHolder.IsEmpty;
			}
		}

		bool IsAccountDetailsSet(IMAFAccountDetails details)
		{
			return !details.AccountHolderName.IsEmpty && !details.AccountNumber.IsEmpty;
		}

		public IMAFPlugInSupport PlugInSupport
		{
			get
			{
				return new MAFPlugInSupportDeclarationWrapper(this);
			}
		}

		ParentLevel IMPIAccountDetails.ParentLevel => ParentLevel.Declaration;

		#endregion

		#endregion

		#region ILandedCostHeader Members
		bool ILandedCostHeader.IsLCSupported
		{
			get { return !IsECIWriteoff && IsImport; }
		}

		string ILandedCostHeader.MessageShownWhenLCIsNotSupported
		{
			get { return IsECIWriteoff ? "Landed Costing is not supported for ECI Writeoffs." : BaseMessageShownWhenLCIsNotSupported; }
		}
		#endregion

		#region IDocumentSupportable Override
		protected override DocumentSupporter CreateNewDocumentSupporter()
		{
			return new JobDeclarationDocumentSupporter(this);
		}
		#endregion

		#region ILandedCostHeader Members

		// TODO: Will be replaced with GenericLandedCostingConfig
		DutyTaxEntryFee ILandedCostHeader.TotalDutyTaxEntryFeeItems => Factory.GetValue(ref dutyTaxEntryFeeCached, delegate
		{
			DutyTaxEntryFee result = new DutyTaxEntryFee();
			foreach (CusEntryHeader entryHeader in ActiveEntryHeaders)
			{
				FormalEntry.CusEntryHeader formalEntryHeader = entryHeader as FormalEntry.CusEntryHeader;
				if (formalEntryHeader != null)
				{
					result[CustomsDisbursementChargeCode.TotalDuty] += formalEntryHeader.DutyAmount;
					result[CustomsDisbursementChargeCode.EntryFees] += formalEntryHeader.EntryFeeAmount;
					result[CustomsDisbursementChargeCode.OtherOrFlatDutyAmount] += formalEntryHeader.AntiDumpingDutyAmount + formalEntryHeader.CountervailingDutyAmount + formalEntryHeader.SyntheticGreenhouseGasesLevyAmount;
					result[CustomsDisbursementChargeCode.SpecialTax1] += formalEntryHeader.ALACLevyAmount;
					result[CustomsDisbursementChargeCode.SpecialTax2] += formalEntryHeader.HERALevyAmount;
					result[CustomsDisbursementChargeCode.SpecialTax3] += formalEntryHeader.ACCFuelLevyAmount;
				}
			}
			return result;
		});

		CachedProperty<DutyTaxEntryFee> dutyTaxEntryFeeCached;

		void ILandedCostHeader.DoStuffBeforeRunningLCDistribution()
		{
		}

		protected override bool IsEntryClearCore
		{
			get
			{
				return JE_EntryStatus == TSWEntryStatusList.Codes.CCC ||
					   JE_EntryStatus == LowValueConsignmentStatusList.Codes.ConsignmentWrittenOff ||
					   JE_EntryStatus == FormalEntryStatusList.Codes.AdjustmentAccepted ||
					   JE_EntryStatus == FormalEntryStatusList.Codes.DeliveryOrderReceived ||
					   JE_EntryStatus == FormalEntryStatusList.Codes.EntryRestored ||
					   JE_EntryStatus == FormalEntryStatusList.Codes.ManualEntryCannotBeSentToCustoms ||
					   JE_EntryStatus == FormalEntryStatusList.Codes.EntryCleared;
			}
		}

		#endregion

		#region EDI Transmit Date
		EDITransmitDateManager fTransmitDateManager;
		public EDITransmitDateManager TransmitDateManager
		{
			get
			{
				if (fTransmitDateManager == null)
				{
					fTransmitDateManager = new EDITransmitDateManager(this);
				}
				return fTransmitDateManager;
			}
		}

		void SetEDITransmitDateIfRequired()
		{
			if (!settingDefaultValues && !JE_EDITransmitDateFinalised && IsExport)
			{
				ZDateTime recommendedDate = TransmitDateManager.RecommendedDate;
				if (recommendedDate.IsValid)
				{
					JE_EDITransmitDate = recommendedDate;
				}
			}
		}
		#endregion

		#region ITranshipmentRequestParent
		bool ITranshipmentRequestParent.IsTranshipmentRequestRelevant => IsTSWWriteOff;

		ITransportParent ITranshipmentRequestParent.TransportParent
		{
			get
			{
				ITransportParent result = null;
				var shipment = Shipment;
				if (shipment != null)
				{
					result = shipment;
					var consol = IsExport ? shipment.DepartureConsol : shipment.ArrivalConsol;
					if (consol != null)
					{
						result = consol;
					}
				}
				else
				{
					result = this;
				}

				return result;
			}
		}

		ZDateTime ITranshipmentRequestParent.DepartureDate => JE_ExportDate;

		ZDateTime ITranshipmentRequestParent.ArrivalDate => JE_DateOfArrival;

		event EventHandler ITranshipmentRequestParent.MessageTypeChanged
		{
			add { JE_MessageTypeInfo.ValueChanged += value; }
			remove { JE_MessageTypeInfo.ValueChanged -= value; }
		}

		event EventHandler ITranshipmentRequestParent.MessageSubTypeChanged
		{
			add { JE_MessageSubTypeInfo.ValueChanged += value; }
			remove { JE_MessageSubTypeInfo.ValueChanged -= value; }
		}

		event EventHandler ITranshipmentRequestParent.TransportModeChanged
		{
			add { JE_TransportModeInfo.ValueChanged += value; }
			remove { JE_TransportModeInfo.ValueChanged -= value; }
		}

		#endregion

		#region ECI Writeoff Only
		internal ECIInvoiceManager InvoiceManager
		{
			get
			{
				if (fInvoiceManager == null)
				{
					fInvoiceManager = new ECIInvoiceManager(this);
				}
				return fInvoiceManager;
			}
		}
		ECIInvoiceManager fInvoiceManager;

		public class ECIInvoiceManager
		{
			public ECIInvoiceManager(JobDeclaration declaration)
			{
				this.declaration = declaration;
			}
			readonly JobDeclaration declaration;

			public void SetAmountAndCurrency(ZDecimal invoiceAmount, ZString invoiceCurrencyCode)
			{
				JobComInvoiceHeader invoiceHeader = GetInvoiceHeaderForSettingIfItsPossibleToSetFromADeclarationLevel(invoiceAmount != defaultInvoiceAmount || invoiceCurrencyCode != defaultInvoiceCurrency.RX_Code);
				if (invoiceHeader != null)
				{
					invoiceHeader.JZ_InvoiceAmount = invoiceAmount;
					invoiceHeader.JZ_RX_NKInvoice_Currency = invoiceCurrencyCode;
				}
			}
			public ZDecimal InvoiceAmount
			{
				get { return InvoiceValue.Amount; }
				set
				{
					JobComInvoiceHeader invoiceHeader = GetInvoiceHeaderForSettingIfItsPossibleToSetFromADeclarationLevel(value != defaultInvoiceAmount);
					if (invoiceHeader != null)
					{
						invoiceHeader.JZ_InvoiceAmount = value;
					}
				}
			}

			public ZString InvoiceCurrencyNK
			{
				get { return InvoiceValue.Currency != null ? (ZString)InvoiceValue.Currency.Code : ZString.Empty; }
				set
				{
					JobComInvoiceHeader invoiceHeader = GetInvoiceHeaderForSettingIfItsPossibleToSetFromADeclarationLevel(value != defaultInvoiceCurrency.RX_Code);
					if (invoiceHeader != null)
					{
						invoiceHeader.JZ_RX_NKInvoice_Currency = value;
					}
				}
			}

			Money InvoiceValue => declaration.Factory.GetValue(ref cachedInvoiceValue, GetCurrentInvoiceValue);

			CachedProperty<Money> cachedInvoiceValue;
			readonly ZDecimal defaultInvoiceAmount = 0.00m;
			readonly RefCurrency defaultInvoiceCurrency = JobDeclaration.GetLocalCurrency();
			Money GetCurrentInvoiceValue()
			{
				Money result = new Money(defaultInvoiceAmount, defaultInvoiceCurrency);
				if (FirstJobComInvoiceHeaderIfTheresOnlyOneInvoiceHeaderWithNoInvoiceLines != null)
				{
					result = FirstJobComInvoiceHeaderIfTheresOnlyOneInvoiceHeaderWithNoInvoiceLines.InvoiceAmount;
				}
				else if (declaration.InvoiceLines.Count > 0)
				{
					result = declaration.Invoices.TotalInvoiceLinesAmount;
				}
				return result;
			}

			JobComInvoiceHeader FirstJobComInvoiceHeaderIfTheresOnlyOneInvoiceHeaderWithNoInvoiceLines => declaration.Factory.GetValue(ref cachedFirstInvoiceHeader, GetFirstJobComInvoiceHeaderIfTheresOnlyOneInvoiceHeaderWithNoInvoiceLine);

			CachedProperty<JobComInvoiceHeader> cachedFirstInvoiceHeader;

			JobComInvoiceHeader GetFirstJobComInvoiceHeaderIfTheresOnlyOneInvoiceHeaderWithNoInvoiceLine()
			{
				JobComInvoiceHeader result = null;
				if (declaration.Invoices.Count == 1 && declaration.InvoiceLines.Count == 0)
				{
					result = declaration.Invoices[0];
				}
				return result;
			}

			JobComInvoiceHeader GetInvoiceHeaderForSettingIfItsPossibleToSetFromADeclarationLevel(bool isTheValueBeingSetNonDefault)
			{
				JobComInvoiceHeader result = null;
				if (declaration.IsECIWriteoff)
				{
					if (declaration.Invoices.Count == 0 && isTheValueBeingSetNonDefault)
					{
						result = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
						if (result.JZ_IncoTerm.IsEmpty)
						{
							result.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
						}
					}
					else
					{
						result = FirstJobComInvoiceHeaderIfTheresOnlyOneInvoiceHeaderWithNoInvoiceLines;
					}
				}
				return result;
			}

			public bool IsPossibleToSetFromDeclarationLevel
			{
				get
				{
					return (declaration.Invoices.Count == 0 || FirstJobComInvoiceHeaderIfTheresOnlyOneInvoiceHeaderWithNoInvoiceLines != null)
							&& !(declaration.IsTSWCREWriteOff && declaration.InvoiceLines.Count > 0);
				}
			}
		}

		[BusinessObjectTestExclude()]
		[ReadOnlyMember(nameof(JE_ECI_InvoiceAmountReadOnly))]
		public ZDecimal JE_ECI_InvoiceAmount
		{
			get
			{
				if (!IsECIWriteoff)
				{
					return 0m;
				}
				return InvoiceManager.InvoiceAmount;
			}
			set
			{
				InvoiceManager.InvoiceAmount = value;
				Validation.ValidateJE_ECI_InvoiceAmount();
				JE_ECI_InvoiceAmountInfo.RefreshBinding();
			}
		}

		public bool JE_ECI_InvoiceAmountReadOnly
		{
			get { return IsDataSyncFromShipment || !InvoiceManager.IsPossibleToSetFromDeclarationLevel; }
		}

		public ZPropertyInfo JE_ECI_InvoiceAmountInfo
		{
			get { return GetZPropertyInfo(Schema.JE_ECI_InvoiceAmount); }
		}

		[BusinessObjectTestExclude(), RelatedBusinessObject("ECI_InvoiceCurrency")]
		[ReadOnlyMember(nameof(JE_ECI_InvoiceCurrencyReadOnly))]
		public ZGuid JE_ECI_InvoiceCurrency
		{
			get
			{
				if (!IsECIWriteoff)
				{
					return ZGuid.Empty;
				}
				RefCurrency currency = RefCurrency.LoadFromCurrencyCode(Factory, InvoiceManager.InvoiceCurrencyNK);
				if (currency != null)
				{
					return currency.PK;
				}
				else
				{
					return ZGuid.Empty;
				}
			}
			set
			{
				RefCurrency currency = Factory.Load<RefCurrency>(value);
				if (currency != null)
				{
					InvoiceManager.InvoiceCurrencyNK = currency.RX_Code;
				}
				else
				{
					InvoiceManager.InvoiceCurrencyNK = ZString.Empty;
				}
				Validation.ValidateJE_ECI_InvoiceCurrency();
				JE_ECI_InvoiceCurrencyInfo.RefreshBinding();
			}
		}

		RefCurrency JE_ECI_InvoiceAmountCurrency
		{
			get { return (RefCurrency)Factory.Load(typeof(RefCurrency), JE_ECI_InvoiceCurrency); }
		}

		public ZDecimal JE_ECI_InvoiceAmountInLocalCurrency
		{
			get
			{
				var result = ZDecimal.Zero;
				var eciInvoiceAmount = JE_ECI_InvoiceAmount;
				if (eciInvoiceAmount > 0)
				{
					var eciInvoiceAmountCurrency = JE_ECI_InvoiceAmountCurrency;
					if (eciInvoiceAmountCurrency == LocalCurrency || eciInvoiceAmountCurrency == null)
					{
						result = eciInvoiceAmount;
					}
					else
					{
						result = ConvertToLocalAmount(eciInvoiceAmount, eciInvoiceAmountCurrency).Amount;
					}
				}
				return result;
			}
		}

		public bool JE_ECI_InvoiceCurrencyReadOnly
		{
			get { return !InvoiceManager.IsPossibleToSetFromDeclarationLevel || IsDataSyncFromShipment; }
		}

		public ZPropertyInfo JE_ECI_InvoiceCurrencyInfo
		{
			get { return GetZPropertyInfo(Schema.JE_ECI_InvoiceCurrency); }
		}

		public RefCurrency ECI_InvoiceCurrency
		{
			get { return (RefCurrency)Factory.Load(typeof(RefCurrency), JE_ECI_InvoiceCurrency); }
		}

		[ResourceStringData("Enterprise.Customs.NZ.Business.Declaration.JobDeclaration|JE_ECI_LastResponseStatus", Caption = "EFT Mode")]
		public ZString JE_ECI_LastResponseStatus
		{
			get { return JE_EFTMode; }
			set { JE_EFTMode = value; }
		}

		public ZPropertyInfo JE_ECI_LastResponseStatusInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.JE_ECI_LastResponseStatus, x => JE_EFTModeInfo); }
		}

		public bool IsECIManifestDeclarationReference
		{
			get { return IsManifestedDeclarationReference(JE_DeclarationReference); }
		}

		public static bool IsManifestedDeclarationReference(ZString declarationReference)
		{
			return IsNewStyleManifestedDeclarationReference(declarationReference)
				|| IsOldStyleManifestedDeclarationReference(declarationReference);
		}

		static bool IsNewStyleManifestedDeclarationReference(ZString declarationReference)
		{
			return declarationReference.Left(1) == NumberFountains.ECIManifestReferencePrefix && declarationReference.SubstringSafe(9, 1) == "-";
		}

		static bool IsOldStyleManifestedDeclarationReference(ZString declarationReference)
		{
			return declarationReference.Left(3) == NumberFountains.OldECIManifestReferencePrefix && declarationReference.SubstringSafe(11, 1) == "-";
		}

		internal static void ProcessRestoredEntry(JobDeclaration declaration, CusEntryHeader entryHeader1)
		{
			var restoredTransmitDate = ZDateTime.Empty;
			foreach (CusEntryHeader entryHeader in declaration.CustomsEntryHeaders)
			{
				entryHeader.CH_IsActive = false;
				if (!entryHeader.CH_EDITransmitDate.IsEmpty && entryHeader.EntryNumber == entryHeader1.EntryNumber)
				{
					restoredTransmitDate = entryHeader.CH_EDITransmitDate;
				}
			}

			entryHeader1.CH_IsActive = true;
			entryHeader1.CH_EDITransmitDate = restoredTransmitDate;
			entryHeader1.CH_IsRestored = true;
			entryHeader1.CH_IsEntryCancelled = false;
			var entryHeaders = declaration.EntryHeadersMatchingCurrentDeclarationType;
			entryHeaders.Where(x => x.PK != entryHeader1.PK).ForEach(x => x.CH_IsActive = false);
			declaration.CustomsEntryHeaders.RemoveAndDeleteUnNecessaryCusEntryHeaders();
		}

		public ZString ECIManifestReference
		{
			get
			{
				if (IsNewStyleManifestedDeclarationReference(JE_DeclarationReference))
				{
					return JE_DeclarationReference.Left(9);
				}
				else if (IsOldStyleManifestedDeclarationReference(JE_DeclarationReference))
				{
					return JE_DeclarationReference.Left(11);
				}
				return ZString.Empty;
			}
		}

		public ZString ECIManifestLineNumber
		{
			get
			{
				if (IsNewStyleManifestedDeclarationReference(JE_DeclarationReference))
				{
					return JE_DeclarationReference.SubstringSafe(10);
				}
				else if (IsOldStyleManifestedDeclarationReference(JE_DeclarationReference))
				{
					return JE_DeclarationReference.SubstringSafe(12);
				}
				return new ZString("1");
			}
		}

		protected override bool DeclarationReferenceOverriddenAndCannotBeChanged
		{
			get { return IsECIManifestDeclarationReference; }
		}

		internal void LinkToManifest(ECIWriteOff.Manifesting.CusEntryHeader manifestEntryHeader, ZInt lineNumber)
		{
			int totalDecsInManifest = manifestEntryHeader.Declarations.Count;
			int paddingWidth = totalDecsInManifest.ToString(CultureInfo.InvariantCulture).Length;

			ZString lineNumberString = lineNumber.ToString();
			ZString manifestSequence = lineNumberString.PadLeft(paddingWidth, '0');

			CusEntryHeader.IsActive = false;
			JE_DeclarationReference = manifestEntryHeader.CH_BGMReference + "-" + manifestSequence;
			JE_EntryStatus = LowValueConsignmentStatusList.Codes.ManifestedReadyToSend;
		}
		#endregion

		#region MCD Container Quarantine Declaration Fields

		const int containerCleanCodePosition = 0;
		const int PackagingMaterialContaminatedCodePosition = 1;
		const int WoodPackagingUsedCodePosition = 2;
		const int WoodPackagingTreatedCodePosition = 3;
		const int WoodPackagingTreatmentCertificatedAvailableCodePosition = 4;

		HeaderOtherInfo MCDOtherInfo => (HeaderOtherInfo)OtherInfos?.GetElementWithThisCode(HeaderOtherInfoList.Codes.MAFContainerDeclaration);

		public ZString MCDOtherInfoValue => MCDOtherInfo?.ZO_Data ?? ZString.Empty;

		public ZBool JE_SendMCDContainerQuarantineDeclaration
		{
			get
			{
				return MCDOtherInfo != null;
			}
			set
			{
				var mcdOtherInfo = MCDOtherInfo;
				if (value)
				{
					if (mcdOtherInfo == null)
					{
						var newMCDOtherInfo = OtherInfos.AddNew();
						newMCDOtherInfo.ZO_Code = HeaderOtherInfoList.Codes.MAFContainerDeclaration;
					}
				}
				else
				{
					if (mcdOtherInfo != null)
					{
						OtherInfos.RemoveAndDelete(mcdOtherInfo);
					}
				}
				if (!IsValidationSuspended)
				{
					Validation.ValidateJE_SendMCDContainerQuarantineDeclaration();
				}
			}
		}

		public ZPropertyInfo JE_SendMCDContainerQuarantineDeclarationInfo
		{
			get { return GetZPropertyInfo(Schema.JE_SendMCDContainerQuarantineDeclaration); }
		}

		public ZBool JE_HaveMAFContainerDeclaration
		{
			get
			{
				return !MCDOtherInfo?.ZO_Data.ToUpper().IsEmpty ?? false;
			}
			set
			{
				var oldValue = JE_HaveMAFContainerDeclaration;
				if (oldValue != value)
				{
					if (value)
					{
						using (OtherInfos.SuspendCodesChangedRelatedActions())
						{
							JE_SendMCDContainerQuarantineDeclaration = true;
						}
						MCDOtherInfo.ZO_Data = "NNNNN";
					}
					else
					{
						if (MCDOtherInfo != null)
						{
							MCDOtherInfo.ZO_Data = ZString.Empty;
						}
					}
				}
			}
		}

		public ZPropertyInfo JE_HaveMAFContainerDeclarationInfo
		{
			get { return GetZPropertyInfo(Schema.JE_HaveMAFContainerDeclaration); }
		}

		void SetUpMAFContainerFlag(int position, bool flag)
		{
			var code = flag ? "Y" : "N";
			if (flag)
			{
				using (OtherInfos.SuspendCodesChangedRelatedActions())
				{
					JE_HaveMAFContainerDeclaration = true;
				}
			}
			if (JE_HaveMAFContainerDeclaration)
			{
				var mcdFlag = MCDOtherInfo.ZO_Data;
				mcdFlag = mcdFlag.RemoveSafe(position, 1);
				mcdFlag = mcdFlag.InsertSafe(position, code);
				MCDOtherInfo.ZO_Data = mcdFlag;
			}
		}

		bool CheckMAFContainerFlag(int position)
		{
			var result = false;
			if (JE_HaveMAFContainerDeclaration)
			{
				result = MCDOtherInfo.ZO_Data.SubstringSafe(position, 1).EqualsIgnoringCase("Y");
			}
			return result;
		}

		public ZBool JE_IsContainerClean
		{
			get
			{
				return CheckMAFContainerFlag(containerCleanCodePosition);
			}
			set
			{
				var oldValue = JE_IsContainerClean;
				if (oldValue != value)
				{
					SetUpMAFContainerFlag(containerCleanCodePosition, value);
				}
			}
		}

		public ZPropertyInfo JE_IsContainerCleanInfo
		{
			get { return GetZPropertyInfo(Schema.JE_IsContainerClean); }
		}

		public ZBool JE_IsPackagingMaterialContaminated
		{
			get
			{
				return CheckMAFContainerFlag(PackagingMaterialContaminatedCodePosition);
			}
			set
			{
				var oldValue = JE_IsPackagingMaterialContaminated;
				if (oldValue != value)
				{
					SetUpMAFContainerFlag(PackagingMaterialContaminatedCodePosition, value);
				}
			}
		}

		public ZPropertyInfo JE_IsPackagingMaterialContaminatedInfo
		{
			get { return GetZPropertyInfo(Schema.JE_IsPackagingMaterialContaminated); }
		}

		public ZBool JE_IsWoodPackagingUsed
		{
			get
			{
				return CheckMAFContainerFlag(WoodPackagingUsedCodePosition);
			}
			set
			{
				var oldValue = JE_IsWoodPackagingUsed;
				if (oldValue != value)
				{
					SetUpMAFContainerFlag(WoodPackagingUsedCodePosition, value);
				}
			}
		}

		public ZPropertyInfo JE_IsWoodPackagingUsedInfo
		{
			get { return GetZPropertyInfo(Schema.JE_IsWoodPackagingUsed); }
		}

		public ZBool JE_IsWoodPackagingTreated
		{
			get
			{
				return CheckMAFContainerFlag(WoodPackagingTreatedCodePosition);
			}
			set
			{
				var oldValue = JE_IsWoodPackagingTreated;
				if (oldValue != value)
				{
					SetUpMAFContainerFlag(WoodPackagingTreatedCodePosition, value);
				}
			}
		}

		public ZPropertyInfo JE_IsWoodPackagingTreatedInfo
		{
			get { return GetZPropertyInfo(Schema.JE_IsWoodPackagingTreated); }
		}

		public ZBool JE_IsWoodPackagingTreatmentCertificateAvailable
		{
			get
			{
				return CheckMAFContainerFlag(WoodPackagingTreatmentCertificatedAvailableCodePosition);
			}
			set
			{
				var oldValue = JE_IsWoodPackagingTreatmentCertificateAvailable;
				if (oldValue != value)
				{
					SetUpMAFContainerFlag(WoodPackagingTreatmentCertificatedAvailableCodePosition, value);
				}
			}
		}

		public ZPropertyInfo JE_IsWoodPackagingTreatmentCertificateAvailableInfo
		{
			get { return GetZPropertyInfo(Schema.JE_IsWoodPackagingTreatmentCertificateAvailable); }
		}
		#endregion

		protected override bool DoMergeCore(ISendsMessagesToCustoms notifier)
		{
			bool result = true;
			if (ShouldMergeInvoiceLines)
			{
				result = base.DoMergeCore(notifier);
			}
			return result;
		}

		internal bool ShouldMergeInvoiceLines
		{
			get { return IsFormalEntry; }
		}

		public override bool IsMergeDone
		{
			get
			{
				var entryHeaders = ActiveEntryHeaders.Take(2).ToArray();
				if (entryHeaders.Length == 1)
				{
					var entryHeader = (CusEntryHeader)entryHeaders[0];
					return entryHeader.MergedLines.Count > 0;
				}
				return base.IsMergeDone;
			}
		}

		public bool HasDuplicatedActiveEntryHeader()
		{
			var result = false;
			var newEntryHeaders = CustomsEntryHeaders.Cast<CusEntryHeader>().Where(x => !x.IsInDatabase && x.IsEntryHeaderCurrentDeclarationType(this)).ToArray();
			if (newEntryHeaders.Length > 0)
			{
				CustomsEntryHeaders.Reload(false);
				var appropriateEntryHeader = GetAppropriateCusEntryHeaderIfExists();
				foreach (CusEntryHeader entryHeader in newEntryHeaders)
				{
					if (appropriateEntryHeader != null && appropriateEntryHeader.PK != entryHeader.PK)
					{
						result = true;
						break;
					}
				}
			}
			return result;
		}

		#region MessageSubType Defaulting
		void JE_MessageTypeHasChanged()
		{
			UpdateJE_MessageSubTypeIfSubTypeIsNotValidForCurrentJE_MessageType();
			SetEDITransmitDateIfRequired();
			RefreshValidationOnAllChildZeroRatingFields();
			RefreshInvoiceLineProductMaxLimit();
			ResetCurrencyIndicatorIfRequired();
		}

		void UpdateJE_MessageSubTypeIfSubTypeIsNotValidForCurrentJE_MessageType()
		{
			CodeDescriptionPairList currentMessageSubTypeList = Lookups.MessageSubTypeList;
			if (!currentMessageSubTypeList.ContainsCode(JE_MessageSubType))
			{
				JE_MessageSubType = currentMessageSubTypeList[0].Code;
			}
		}

		void ResetCurrencyIndicatorIfRequired()
		{
			foreach (JobComInvoiceHeader invoiceHader in Invoices)
			{
				invoiceHader.DefaultExchangeRateIndicator();
			}
		}

		void ResetDefaultPDOOtherInfoIfRequired()
		{
			var isTSWDeclaration = IsTSWDeclaration;
			if (!IsInDatabase && (isTSWDeclaration != JE_PDOOtherInfoValue))
			{
				JE_PDOOtherInfoValue = isTSWDeclaration;
			}
		}

		void ResetPaymentTypeIfChangingList(ZString newValue)
		{
			if (IsTSWDeclaration)
			{
				if (newValue == JobMessageSubTypeList.Codes.WriteOff)
				{
					if (PaymentMethodList.IsValidCode(JE_PaymentMethod))
					{
						JE_PaymentMethod = ZString.Empty;
					}
				}
				else
				{
					if (!PaymentMethodList.IsValidCode(JE_PaymentMethod))
					{
						JE_PaymentMethod = ZString.Empty;
					}
				}
			}
		}

		#endregion

		#region Entry Type Change Management
		void JE_MessageSubTypeHasChanged(Type previousEntryHeaderType)
		{
			if (previousEntryHeaderType != TypeOfEntryHeaderRequiredForCurrentDeclarationSettings)
			{
				SetupEntryHeaderForNewEntryType();
			}

			RefreshInvoiceLineProductMaxLimit();
		}

		void SetupEntryHeaderForNewEntryType()
		{
			CustomsEntryHeaders.RemoveAndDeleteUnNecessaryCusEntryHeaders();
			CusEntryHeader entryHeader = GetAppropriateCusEntryHeaderIfExists();
			if (entryHeader != null)
			{
				entryHeader.SetDeclarationStatusesWhenSetToCurrent(this);
			}
			else
			{
				SetMessagingStatusToNotSent(this);
			}

			if (IsFormalEntry)
			{
				SetEDITransmitDateIfRequired();
				OrgHeader localParty = LocalParty;
				if (localParty != null)
				{
					if (!Lookups.PaymentPartyList.ContainsCode(JE_PaymentMethod))
					{
						DefaultJE_PaymentMethodFromLocalParty(localParty);
					}

					if (!Lookups.MergeByList.ContainsCode(JE_MergeBy))
					{
						DefaultJE_MergeByFromLocalParty(localParty);
					}
				}

				if (!Lookups.ProcessingPortList.ContainsCode(JE_RL_NKProcessingPort))
				{
					DefaultJE_RL_NKProcessingPortFromRegistry();
				}
			}
		}

		#endregion

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new FetchStrategies.JobDeclarationFetchStrategy(this);
		}

		public void AutoCreatePackagesIfPossible()
		{
			if (!JE_HouseBill.IsEmpty && !JE_TotalNoOfPacks.IsEmpty && !JE_TotalNoOfPacksPackType.IsEmpty
				&& Packages.Count == 0 && Bills.Count == 1 && CusContainers.Count <= 1)
			{
				Package pack = Packages.AddNew();
				pack.CW_PackQty = JE_TotalNoOfPacks;
				pack.CW_PackType = JE_TotalNoOfPacksPackType;
				pack.CW_HouseBill = JE_HouseBill;
				if (CusContainers.Count == 1)
				{
					pack.CW_ContainerNoOrEquipmentNo = CusContainers[0].CO_ContainerNumber;
				}
			}
		}

		public ZString MergeAndSaveIfNotMergedAlreadyReturningErrors()
		{
			if (CusEntryHeader.MergedLines.Count == 0 || MergeManager.RequiresMerge)
			{
				var mergeResultGetter = new SendsMessagesToCustomsReturningResultsAsProperties(true);
				if (!DoMerge(mergeResultGetter))
				{
					return mergeResultGetter.MergeResult;
				}
				try
				{
					if (!HasErrors)
					{
						Factory.Save();
					}
					else
					{
						return new ZString("Document cannot be printed due to validation errors(s).");
					}
				}
				catch (Exception e) when (!e.IsCriticalException())
				{
					ZExceptionReporting.HandleSaveException(e);
					return new ZString("Error saving successful merge.");
				}
			}

			return ZString.Empty;
		}

		#region EntryHeaderForOriginalEntryNumber
		public CusEntryHeader EntryHeaderForOriginalEntryNumber
		{
			get
			{
				if (fEntryHeaderForOriginalEntryNumber == null || fEntryHeaderForOriginalEntryNumber.IsDeleted || !fEntryHeaderForOriginalEntryNumber.CH_IsActive)
				{
					fEntryHeaderForOriginalEntryNumber = GetEntryHeaderForOriginalEntryNumber();
				}

				return fEntryHeaderForOriginalEntryNumber;
			}
		}
		CusEntryHeader fEntryHeaderForOriginalEntryNumber;

		CusEntryHeader GetEntryHeaderForOriginalEntryNumber()
		{
			CusEntryHeader result = null;
			foreach (CusEntryHeader entryHeader in CustomsEntryHeaders)
			{
				if (entryHeader.CH_IsActive)
				{
					if (entryHeader.GetType() == typeof(FormalEntry.CusEntryHeader) && !entryHeader.EntryNumber.IsEmpty)
					{
						result = entryHeader;
						break;
					}
					else if (entryHeader is OriginalCusEntryHeader)
					{
						result = entryHeader;
					}
				}
			}

			return result;
		}

		CusEntryHeader GetEntryHeaderForManualEntryOfOriginalEntryNumber()
		{
			CusEntryHeader result = EntryHeaderForOriginalEntryNumber ?? CustomsEntryHeaders.AddNew(typeof(OriginalCusEntryHeader));

			return result;
		}
		#endregion

		#region JE_OriginalEntryNumber
		[BusinessObjectTestExclude()]
		[MaxLength(8)]
		public ZString JE_OriginalEntryNumber
		{
			get { return !IsCompletion || EntryHeaderForOriginalEntryNumber == null ? ZString.Empty : EntryHeaderForOriginalEntryNumber.EntryNumber; }
			set
			{
				if (value != JE_OriginalEntryNumber)
				{
					GetEntryHeaderForManualEntryOfOriginalEntryNumber().EntryNumber = value;
					Validation.ValidateJE_OriginalEntryNumber();
				}
				JE_OriginalEntryNumberInfo.RefreshBinding();
			}
		}

		protected bool JE_OriginalEntryNumber_ReadOnly
		{
			get { return EntryHeaderForOriginalEntryNumber != null && EntryHeaderForOriginalEntryNumber.GetType() == typeof(FormalEntry.CusEntryHeader); }
		}

		public ZPropertyInfo JE_OriginalEntryNumberInfo
		{
			get { return GetZPropertyInfo(Schema.JE_OriginalEntryNumber); }
		}
		#endregion

		#region EntryHeaderForWriteOff

		public CusEntryHeader EntryHeaderForWriteOffEntryNumber
		{
			get
			{
				if (fEntryHeaderForWriteOffEntryNumber == null || fEntryHeaderForWriteOffEntryNumber.IsDeleted || !fEntryHeaderForWriteOffEntryNumber.CH_IsActive)
				{
					fEntryHeaderForWriteOffEntryNumber = GetEntryHeaderForWriteOffEntryNumber();
				}
				return fEntryHeaderForWriteOffEntryNumber;
			}
		}
		CusEntryHeader fEntryHeaderForWriteOffEntryNumber;

		CusEntryHeader GetEntryHeaderForWriteOffEntryNumber()
		{
			CusEntryHeader result = null;
			foreach (CusEntryHeader entryHeader in CustomsEntryHeaders)
			{
				if (entryHeader.CH_IsActive)
				{
					if (entryHeader.GetType() == typeof(ECIWriteOff.CusEntryHeader) && !entryHeader.EntryNumber.IsEmpty)
					{
						result = entryHeader;
						break;
					}
				}
			}
			return result;
		}

		#endregion

		#region EntryHeaderForIPI

		public CusEntryHeader EntryHeaderForIPIEntryNumber
		{
			get
			{
				if (fEntryHeaderForIPIEntryNumber == null || fEntryHeaderForIPIEntryNumber.IsDeleted || !fEntryHeaderForIPIEntryNumber.CH_IsActive)
				{
					fEntryHeaderForIPIEntryNumber = GetEntryHeaderForIPIEntryNumber();
				}

				return fEntryHeaderForIPIEntryNumber;
			}
		}
		CusEntryHeader fEntryHeaderForIPIEntryNumber;

		CusEntryHeader GetEntryHeaderForIPIEntryNumber()
		{
			CusEntryHeader result = null;
			foreach (CusEntryHeader entryHeader in CustomsEntryHeaders)
			{
				if (entryHeader.CH_IsActive)
				{
					if (entryHeader.GetType() == typeof(PrimaryIndustriesCusEntryHeader) || entryHeader.CH_LastEntryStyle == MessageSubTypeCombinedList.Codes.IPI || entryHeader.CH_MessageType == Customs.Business.CusEntryHeader.EntryHeaderTypes.NZ.PrimaryIndustries)
					{
						result = entryHeader;
						break;
					}
				}
			}

			return result;
		}

		#endregion

		#region JE_OriginalEntryType

		[BusinessObjectTestExclude()]
		[MaxLength(3)]
		public ZString JE_OriginalEntryType
		{
			get
			{
				return !IsCompletion || EntryHeaderForOriginalEntryNumber == null ? ZString.Empty : GetEntryHeaderForManualEntryOfOriginalEntryNumber().CH_LastEntryStyle;
			}
			set
			{
				if (value != JE_OriginalEntryType)
				{
					GetEntryHeaderForManualEntryOfOriginalEntryNumber().CH_LastEntryStyle = value;
					Validation.ValidateJE_OriginalEntryType();
				}
				JE_OriginalEntryTypeInfo.RefreshBinding();
			}
		}

		protected bool JE_OriginalEntryType_ReadOnly
		{
			get { return EntryHeaderForOriginalEntryNumber != null && EntryHeaderForOriginalEntryNumber.GetType() == typeof(FormalEntry.CusEntryHeader); }
		}

		public ZPropertyInfo JE_OriginalEntryTypeInfo
		{
			get { return GetZPropertyInfo(Schema.JE_OriginalEntryType); }
		}

		#endregion

		#region JE_ManifestBioStatus

		[UniversalCopyAddInfoPropertyMapping(AutoNZAddInfo.Schema.ZN_ManifestBioStatus)]
		public ZString JE_ManifestBioStatus
		{
			get { return AddInfo.ZN_ManifestBioStatus; }
			set { AddInfo.ZN_ManifestBioStatus = value; }
		}

		public ZPropertyInfo JE_ManifestBioStatusInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.JE_ManifestBioStatus, x => AddInfo.ZN_ManifestBioStatusInfo); }
		}

		#endregion

		#region JE_ManifestNZCSStatus

		[UniversalCopyAddInfoPropertyMapping(AutoNZAddInfo.Schema.ZN_ManifestNZCSStatus)]
		public ZString JE_ManifestNZCSStatus
		{
			get { return AddInfo.ZN_ManifestNZCSStatus; }
			set { AddInfo.ZN_ManifestNZCSStatus = value; }
		}

		public ZPropertyInfo JE_ManifestNZCSStatusInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.JE_ManifestNZCSStatus, x => AddInfo.ZN_ManifestNZCSStatusInfo); }
		}

		#endregion

		#region Proxied Other Info Fields
		[MaxLength(12)]
		public ZString JE_SEPOtherInfoValue
		{
			get { return SEPOtherInfo != null ? SEPOtherInfo.ZO_Data : ZString.Empty; }
			set
			{
				if (value != JE_SEPOtherInfoValue)
				{
					CheckMaximumLength(JE_SEPOtherInfoValueInfo, value);
					SetOtherInfoValue(value, SEPOtherInfo, HeaderOtherInfoList.Codes.SecureExportPartnership);
				}
				JE_SEPOtherInfoValueInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo JE_SEPOtherInfoValueInfo
		{
			get { return GetZPropertyInfo(Schema.JE_SEPOtherInfoValue); }
		}

		public OtherInfo SEPOtherInfo => Factory.GetValue(ref fSEPOtherInfo, delegate
		{ return (OtherInfo)OtherInfos.GetElementWithThisCode(HeaderOtherInfoList.Codes.SecureExportPartnership); });

		CachedProperty<OtherInfo> fSEPOtherInfo;

		[MaxLength(10)]
		public ZString JE_ATFOtherInfoValue
		{
			get { return ATFOtherInfo != null ? ATFOtherInfo.ZO_Data : ZString.Empty; }
			set
			{
				if (value != JE_ATFOtherInfoValue)
				{
					CheckMaximumLength(JE_ATFOtherInfoValueInfo, value);
					SetOtherInfoValue(value, ATFOtherInfo, HeaderOtherInfoList.Codes.ApprovedTransitionalFacility);
				}
				JE_ATFOtherInfoValueInfo.RefreshBinding();
			}
		}

		void SetOtherInfoValue(ZString newValue, OtherInfo existingOtherInfo, string otherInfoCodeForNew)
		{
			bool haveAValueToStore = !newValue.IsEmpty;
			if (haveAValueToStore != (existingOtherInfo != null))
			{
				if (haveAValueToStore)
				{
					OtherInfos.AddNew(otherInfoCodeForNew, newValue);
					return;
				}
				else
				{
					OtherInfos.RemoveAndDelete(existingOtherInfo);
				}
			}
			else if (haveAValueToStore)
			{
				existingOtherInfo.ZO_Data = newValue;
				existingOtherInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo JE_ATFOtherInfoValueInfo
		{
			get { return GetZPropertyInfo(Schema.JE_ATFOtherInfoValue); }
		}

		OtherInfo ATFOtherInfo => Factory.GetValue(ref fATFOtherInfo, delegate
		{ return (OtherInfo)OtherInfos.GetElementWithThisCode(HeaderOtherInfoList.Codes.ApprovedTransitionalFacility); });

		CachedProperty<OtherInfo> fATFOtherInfo;

		public ZBool JE_PDOOtherInfoValue
		{
			get { return PDOOtherInfo != null; }
			set
			{
				if (JE_PDOOtherInfoValue != value)
				{
					if (value)
					{
						OtherInfos.AddNew(HeaderOtherInfoList.Codes.PrintDeliveryOrder, "");
					}
					else
					{
						OtherInfos.RemoveAndDelete(PDOOtherInfo);
					}
				}
				JE_PDOOtherInfoValueInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo JE_PDOOtherInfoValueInfo
		{
			get { return GetZPropertyInfo(Schema.JE_PDOOtherInfoValue); }
		}

		OtherInfo PDOOtherInfo => Factory.GetValue(ref fPDOOtherInfo, delegate
		{ return (OtherInfo)OtherInfos.GetElementWithThisCode(HeaderOtherInfoList.Codes.PrintDeliveryOrder); });

		CachedProperty<OtherInfo> fPDOOtherInfo;
		#endregion

		protected override void ImporterDeliveryAddressChanged(ZGuid oldAddressPK, ZGuid newAddressPK)
		{
			base.ImporterDeliveryAddressChanged(oldAddressPK, newAddressPK);
			DefaultATFFromImporterAndDeliveryAddressOnImportClearances(Importer, ImporterDeliveryAddress.Address);
		}

		protected override void JE_OH_ImporterChanged(ZGuid oldValue, ZGuid newValue)
		{
			base.JE_OH_ImporterChanged(oldValue, newValue);
			DefaultATFFromImporterAndDeliveryAddressOnImportClearances(Importer, ImporterDeliveryAddress.Address);
			DefaultInsurancePercentagePerLine();
		}

		protected override void JE_OH_SupplierChanged(ZGuid oldValue, ZGuid newValue)
		{
			base.JE_OH_SupplierChanged(oldValue, newValue);
			DefaultSEPCodeFromSupplierOnExportClearances(Supplier);
			DefaultInsurancePercentagePerLine();
		}

		void DefaultInsurancePercentagePerLine()
		{
			if (Supplier != null && Importer != null)
			{
				ZDecimal registryDefaultInsuranceCharge = NZCustomsDataRegistry.Instance.DefaultOverseasInsurance.Value;
				OrgSupplierBuyerLink supplierImporterLink = GetLinkBetweenSupplierAndImporter(Supplier, Importer);
				ZDecimal supplierImporterDefaultInsuranceCharge = supplierImporterLink != null ? supplierImporterLink.OL_InsuranceUplift : ZDecimal.Zero;
				if (!registryDefaultInsuranceCharge.IsEmpty || !supplierImporterDefaultInsuranceCharge.IsEmpty)
				{
					BaseJobComInvHeaderCharge insuranceCharge = null;
					foreach (BaseJobComInvHeaderCharge charge in JobComInvoiceGroupHeaders[0].Charges)
					{
						if (charge.ChargeCode.Code == CustomsChargeTypeList.Codes.OverseasInsurance)
						{
							insuranceCharge = charge;
							break;
						}
					}

					if (insuranceCharge == null)
					{
						insuranceCharge = JobComInvoiceGroupHeaders[0].Charges.AddNew();
						insuranceCharge.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasInsurance;
					}

					if (insuranceCharge.J7_Percentage == 0)
					{
						if (!supplierImporterDefaultInsuranceCharge.IsEmpty)
						{
							insuranceCharge.J7_Percentage = supplierImporterDefaultInsuranceCharge;
						}
						else if (!registryDefaultInsuranceCharge.IsEmpty)
						{
							insuranceCharge.J7_Percentage = registryDefaultInsuranceCharge;
						}
					}
				}
			}
		}

		OrgSupplierBuyerLink GetLinkBetweenSupplierAndImporter(OrgHeader supplier, OrgHeader importer)
		{
			OrgSupplierBuyerLink result = null;
			if (supplier != null && importer != null)
			{
				BusinessObject[] links = importer.SupplierLinks.Find(new ZQuery(OrgSupplierBuyerLinkSchema.OL_OH_Supplier, supplier.PK));
				if (links.Length > 0)
				{
					result = (OrgSupplierBuyerLink)links[0];
				}
			}
			return result;
		}

		void DefaultSEPCodeFromSupplierOnExportClearances(OrgHeader supplier)
		{
			if (IsExport && supplier != null)
			{
				OrgCusCode cusCode = OrgCusCode.Load(Factory, OrgCusCode.NZCodeTypes.SecureExportPartner, RefCountry_NZCode, supplier.PK, ZGuid.Empty);
				if (cusCode != null)
				{
					JE_SEPOtherInfoValue = cusCode.OK_CustomsRegNo.SubstringSafe(0, JE_SEPOtherInfoValueInfo.MaxLength);
				}
			}
		}

		void DefaultSEPCodeFromDepotOnTSWExportSeaClearances(OrgHeader depot)
		{
			if (IsTSWDeclaration && IsExport && IsSea && depot != null)
			{
				var cusCode = OrgCusCode.Load(Factory, OrgCusCode.NZCodeTypes.SecureExportPartner, RefCountry_NZCode, depot.PK, ZGuid.Empty);
				if (cusCode != null)
				{
					JE_SEPOtherInfoValue = cusCode.OK_CustomsRegNo.SubstringSafe(0, JE_SEPOtherInfoValueInfo.MaxLength);
					foreach (CusContainer container in CusContainers)
					{
						container.CO_SealingParty = depot.OH_FullName.SubstringSafe(0, container.CO_SealingPartyInfo.MaxLength);
					}
				}
			}
		}

		public ZString DefaultSealingParty
		{
			get
			{
				var result = ZString.Empty;
				if (IsTSWDeclaration && IsExport && IsSea && DepotDocAddress.Organisation != null)
				{
					result = DepotDocAddress.Organisation.OH_FullName.Left(AutoNZCusContainer.Schema.CO_SealingPartyMaxLength);
				}

				return result;
			}
		}

		void DefaultATFFromImporterAndDeliveryAddressOnImportClearances(OrgHeader importer, OrgAddress deliveryAddress)
		{
			if (IsImport) //JE_ContainerMode is hidden in NZ Declaration GUI.
			{
				bool aTFAlreadyFound = false;
				if (DepotDocAddress.Organisation != null)
				{
					aTFAlreadyFound = SetATFFromOrganisation(DepotDocAddress.Organisation.PK, DepotDocAddress.E2_OA_Address);
				}

				if (!aTFAlreadyFound && deliveryAddress != null)
				{
					aTFAlreadyFound = SetATFFromOrganisation(deliveryAddress.OA_OH, deliveryAddress.PK);
				}

				if (!aTFAlreadyFound && importer != null)
				{
					aTFAlreadyFound = SetATFFromOrganisation(importer.PK, ZGuid.Empty);
				}
			}
		}

		bool SetATFFromOrganisation(ZGuid organisation, ZGuid premises)
		{
			var cusCode = OrgCusCode.Load(Factory, OrgCusCode.NZCodeTypes.ApprovedTransitionalFacility, RefCountry_NZCode, organisation, premises);
			if (cusCode != null)
			{
				JE_ATFOtherInfoValue = cusCode.OK_CustomsRegNo.Left(JE_ATFOtherInfoValueInfo.MaxLength);
				return true;
			}
			else if (!premises.IsEmpty)
			{
				cusCode = OrgCusCode.Load(Factory, OrgCusCode.NZCodeTypes.ApprovedTransitionalFacility, RefCountry_NZCode, organisation, ZGuid.Empty);
				if (cusCode != null)
				{
					JE_ATFOtherInfoValue = cusCode.OK_CustomsRegNo.Left(JE_ATFOtherInfoValueInfo.MaxLength);
					return true;
				}
			}

			return false;
		}

		#region RefCountry_NZCode
		ZString RefCountry_NZCode
		{
			get
			{
				if (refCountry_NZCode.IsEmpty)
				{
					refCountry_NZCode = "NZ";
				}
				return refCountry_NZCode;
			}
		}
		ZString refCountry_NZCode;
		#endregion

		#region ECI_ApportionedContainerAndLoosePackageValues
		public ContainerAndLoosePackagesValueApportioner ECI_ApportionedContainerAndLoosePackageValues
		{
			get
			{
				if (fECI_ApportionedContainerAndLoosePackageValues == null)
				{
					fECI_ApportionedContainerAndLoosePackageValues = new ContainerAndLoosePackagesValueApportioner(this);
				}
				return fECI_ApportionedContainerAndLoosePackageValues;
			}
		}
		ContainerAndLoosePackagesValueApportioner fECI_ApportionedContainerAndLoosePackageValues;
		#endregion

		protected override void DefaultMessageTypeFromSupplierOrImporter(MessageTypeDefaultingTriggerSource source)
		{
			if (JE_OH_Importer == CachedMiscOrgPK || JE_OH_Supplier == CachedMiscOrgPK)
			{
				if (JE_MessageType != JobMessageTypeList.Codes.Import)
				{
					JE_MessageType = JobMessageTypeList.Codes.Import;
				}

				if (JE_MessageSubType != JobMessageSubTypeList.Codes.Simplified)
				{
					JE_MessageSubType = JobMessageSubTypeList.Codes.Simplified;
				}

				return;
			}
			else
			{
				OrgHeader supplier = Supplier;
				OrgHeader importer = Importer;
				if (supplier != null && importer != null)
				{
					ZString supplierCountryCode = supplier.UNLOCO != null ? supplier.UNLOCO.RL_RN_NKCountryCode : ZString.Empty;
					ZString importerCountryCode = importer.UNLOCO != null ? importer.UNLOCO.RL_RN_NKCountryCode : ZString.Empty;

					if (!supplierCountryCode.IsEmpty && !importerCountryCode.IsEmpty)
					{
						ZString desiredMessageType = ZString.Empty;
						ZString localCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

						if (importerCountryCode == localCountryCode && supplierCountryCode != localCountryCode)
						{
							desiredMessageType = JobMessageTypeList.Codes.Import;
						}
						else if (importerCountryCode != localCountryCode && supplierCountryCode == localCountryCode)
						{
							desiredMessageType = JobMessageTypeList.Codes.Export;
						}
						else if (importerCountryCode != localCountryCode && supplierCountryCode != localCountryCode)
						{
							desiredMessageType = JobMessageTypeList.Codes.Export;
						}
						else
						{
							desiredMessageType = JobMessageTypeList.Codes.Excise;
						}

						if (JE_MessageType != desiredMessageType)
						{
							JE_MessageType = desiredMessageType;
						}
						return;
					}
				}
			}

			base.DefaultMessageTypeFromSupplierOrImporter(source);
		}

		protected override ZQuery GetValidCusEntryNumFilter()
		{
			CusEntryHeader header = ExistingCusEntryHeader;
			if (header != null && header.ShouldBeIncludedInCusEntryNumberFilter)
			{
				return new ZQuery(CusEntryNumSchema.CE_ParentID, header.PK);
			}
			return ZQuery.NoResultQuery;
		}

		public new JobDeclaration TemplateCopy()
		{
			return (JobDeclaration)base.TemplateCopy();
		}

		protected override IEnumerable<string> GetPropertiesToExcludeFromCloning()
		{
			var result = new List<string>(base.GetPropertiesToExcludeFromCloning())
			{
				JobDeclarationSchema.Constants.JE_ApplicationCode
			};

			return result;
		}

		protected override bool IsPackingInformationRelevantCore
		{
			get { return true; }
		}

		protected override void ContainerTerminalOperatorDocAddress_ValueChanged(object sender, EventArgs e)
		{
			base.ContainerTerminalOperatorDocAddress_ValueChanged(sender, e);

			RefreshDefaultGoodsLocation();
			ValidateGoodsLocatedAtIfRequired();
		}

		protected override void DepotDocAddress_DocAddressChanged(object sender, EventArgs e)
		{
			base.DepotDocAddress_DocAddressChanged(sender, e);

			DefaultATFFromImporterAndDeliveryAddressOnImportClearances(Importer, ImporterDeliveryAddress.Address);
			RefreshDefaultGoodsLocation();
			ValidateGoodsLocatedAtIfRequired();

			if (DepotDocAddress.HasChanges)
			{
				if (DepotDocAddress.Organisation == null)
				{
					fOrgCode = ZString.Empty;
				}
				else if (fOrgCode != DepotDocAddress.Organisation.OH_Code)
				{
					DefaultSEPCodeFromDepotOnTSWExportSeaClearances(DepotDocAddress.Organisation);
					fOrgCode = DepotDocAddress.Organisation.OH_Code;
				}
			}
		}
		ZString fOrgCode;

		protected override void WarehouseDocAddress_ValueChanged(object sender, EventArgs e)
		{
			base.WarehouseDocAddress_ValueChanged(sender, e);
			ValidateGoodsLocatedAtIfRequired();
		}

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.LocationList))]
		[UniversalCopyAddInfoPropertyMapping(AutoNZAddInfo.Schema.ZN_GoodsLocatedAt)]
		public ZString JE_GoodsLocatedAt
		{
			get { return AddInfo.ZN_GoodsLocatedAt; }
			set
			{
				if (JE_GoodsLocatedAt != value)
				{
					AddInfo.ZN_GoodsLocatedAt = value;

					if (!IsCopying)
					{
						RefreshDefaultGoodsLocation();
					}
				}
			}
		}

		public ZPropertyInfo JE_GoodsLocatedAtInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(JE_GoodsLocatedAt), x => AddInfo.ZN_GoodsLocatedAtInfo); }
		}

		public ZBool JE_GoodsLocatedAtVisible
		{
			get { return IsTSWDeclaration && (IsImport || IsExport) && (IsAir || IsSea || IsPost); }
		}

		void SetGoodsLocatedIfRequired()
		{
			if (JE_GoodsLocatedAtVisible && !settingDefaultValues)
			{
				if (IsAir)
				{
					JE_GoodsLocatedAt = GoodsLocatedAtList.Codes.CTO;
				}
				else if (IsSea && IsImport)
				{
					JE_GoodsLocatedAt = GoodsLocatedAtListForSeaImport.Codes.DIS;
				}
				else if (IsSea && IsExport)
				{
					JE_GoodsLocatedAt = GoodsLocatedAtListForSeaExport.Codes.PC;
				}
			}
		}

		void RefreshInvoiceLineProductMaxLimit()
		{
			foreach (JobComInvoiceLine invoiceLine in FilteredInvoiceLines)
			{
				invoiceLine.CommodityProducts.UpdateMaxCountValidation();
			}
		}

		IEnumerable<CusEntryHeader> EntryHeadersMatchingCurrentDeclarationType
		{
			get
			{
				List<CusEntryHeader> result = new List<CusEntryHeader>();
				foreach (CusEntryHeader entryHeader in CustomsEntryHeaders)
				{
					if (entryHeader.IsEntryHeaderCurrentDeclarationType(this))
					{
						result.Add(entryHeader);
					}
				}
				return result.OrderBy(x => x.CH_RecordAdded);
			}
		}

		#region ZeroRating

		[UniversalCopyAddInfoPropertyMapping(AutoNZAddInfo.Schema.ZN_IsZeroRatedAll)]
		public ZString JE_IsZeroRatedAll
		{
			get { return AddInfo.ZN_IsZeroRatedAll; }
			set
			{
				string oldValue = AddInfo.ZN_IsZeroRatedAll;
				AddInfo.ZN_IsZeroRatedAll = value;
				if (oldValue != value)
				{
					RefreshBindingOnAllChildZeroRatingFields();
					RefreshBindingOnInvoiceHeader(value);
				}
			}
		}

		public ZPropertyInfo JE_IsZeroRatedAllInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(JE_IsZeroRatedAll), x => AddInfo.ZN_IsZeroRatedAllInfo); }
		}

		void RefreshValidationOnAllChildZeroRatingFields()
		{
			if (InvoicesIsLoaded)
			{
				foreach (JobComInvoiceHeader invoiceHeader in Invoices)
				{
					invoiceHeader.Validation.ValidateJZ_IsZeroRatedDuty();
					invoiceHeader.Validation.ValidateJZ_IsZeroRatedExcise();
					invoiceHeader.Validation.ValidateJZ_IsZeroRatedGST();
					invoiceHeader.Validation.ValidateJZ_IsZeroRatedLevies();
				}
			}
			if (FilteredInvoiceLinesIsLoaded)
			{
				foreach (JobComInvoiceLine invoiceLine in FilteredInvoiceLines)
				{
					var validation = invoiceLine.Validation;
					validation.ValidateJI_IsZeroRatedDuty();
					validation.ValidateJI_IsZeroRatedExcise();
					validation.ValidateJI_IsZeroRatedGST();
					validation.ValidateJI_IsZeroRatedLevies();
				}
			}
		}

		void RefreshBindingOnInvoiceHeader(string setValue)
		{
			if (InvoicesIsLoaded)
			{
				foreach (JobComInvoiceHeader invoiceHeader in Invoices)
				{
					invoiceHeader.JZ_IsZeroRatedDuty = setValue;
					invoiceHeader.JZ_IsZeroRatedExcise = setValue;
					invoiceHeader.JZ_IsZeroRatedLevies = setValue;
					invoiceHeader.JZ_IsZeroRatedGST = setValue;
					invoiceHeader.RefreshBinding();
				}
			}
		}

		void RefreshBindingOnAllChildZeroRatingFields()
		{
			if (InvoicesIsLoaded)
			{
				foreach (JobComInvoiceHeader invoiceHeader in Invoices)
				{
					invoiceHeader.ZeroRatedDutyDescriptionInfo.RefreshBinding();
					invoiceHeader.ZeroRatedExciseDescriptionInfo.RefreshBinding();
					invoiceHeader.ZeroRatedGSTDescriptionInfo.RefreshBinding();
					invoiceHeader.ZeroRatedLeviesDescriptionInfo.RefreshBinding();
				}
			}
			if (FilteredInvoiceLinesIsLoaded)
			{
				foreach (JobComInvoiceLine invoiceLine in FilteredInvoiceLines)
				{
					invoiceLine.ZeroRatedDutyDescriptionInfo.RefreshBinding();
					invoiceLine.ZeroRatedExciseDescriptionInfo.RefreshBinding();
					invoiceLine.ZeroRatedGSTDescriptionInfo.RefreshBinding();
					invoiceLine.ZeroRatedLeviesDescriptionInfo.RefreshBinding();
				}
			}
		}

		public ZBool IsAnyZeroRated
		{
			get
			{
				return ((IBusinessObjectCollection)FilteredInvoiceLines).ToArray<JobComInvoiceLine>().Any(line =>
										line.EffectiveIsZeroRatedDuty ||
										line.EffectiveIsZeroRatedExcise ||
										line.EffectiveIsZeroRatedGST ||
										line.EffectiveIsZeroRatedLevies);
			}
		}

		#endregion

		#region Misc Org Handling for Simplified Entries
		public override ZString JE_ImporterMiscFields
		{
			get
			{
				if (JE_OH_Importer == CachedMiscOrgPK && AllowsMiscSupplierAndImporter)
				{
					return Schema.MiscImporterName;
				}
				return base.JE_ImporterMiscFields;
			}
		}

		public override ZString JE_SupplierMiscFields
		{
			get
			{
				if (JE_OH_Supplier == CachedMiscOrgPK && AllowsMiscSupplierAndImporter)
				{
					return Schema.MiscSupplierName;
				}
				return base.JE_SupplierMiscFields;
			}
		}

		public bool AllowsMiscSupplierAndImporter
		{
			get { return IsSimplified && IsImport; }
		}

		ZGuid fCachedMiscOrgPK;
		public ZGuid CachedMiscOrgPK
		{
			get
			{
#if DEBUG
				if (ReturnInvalidMiscOrg)
				{
					return ZGuid.NewZGuid();
				}
#endif
				if (fCachedMiscOrgPK.IsEmpty)
				{
					fCachedMiscOrgPK = OrganisationsDataRegistry.Instance.MiscOrganisation.Value.Organisation;

					if (fCachedMiscOrgPK.IsEmpty)
					{
						fCachedMiscOrgPK = ZGuid.NewZGuid();
					}
				}
				return fCachedMiscOrgPK;
			}
		}
#if DEBUG
		public bool ReturnInvalidMiscOrg;
#endif

		[UniversalCopyAddInfoPropertyMapping(AutoNZAddInfo.Schema.ZN_SupplierName)]
		public ZString MiscSupplierName
		{
			get { return AddInfo.ZN_SupplierName; }
			set { AddInfo.ZN_SupplierName = value.ToUpper(); }
		}

		public ZPropertyInfo MiscSupplierNameInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.MiscSupplierName, x => AddInfo.ZN_SupplierNameInfo); }
		}

		[UniversalCopyAddInfoPropertyMapping(AutoNZAddInfo.Schema.ZN_ImporterName)]
		public ZString MiscImporterName
		{
			get { return AddInfo.ZN_ImporterName; }
			set { AddInfo.ZN_ImporterName = value.ToUpper(); }
		}

		public ZPropertyInfo MiscImporterNameInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.MiscImporterName, x => AddInfo.ZN_ImporterNameInfo); }
		}

		public override void OnSaving()
		{
			if (JE_OH_Importer != CachedMiscOrgPK && !MiscImporterName.IsEmpty)
			{
				MiscImporterName = ZString.Empty;
			}
			if (JE_OH_Supplier != CachedMiscOrgPK && !MiscSupplierName.IsEmpty)
			{
				MiscSupplierName = ZString.Empty;
			}

			base.OnSaving();
		}

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);
			if (!saveSucceeded)
			{
				if (IsInDatabase)
				{
					JE_EntryStatus = (ZString)JE_EntryStatusInfo.OriginalValue;
					JE_EntrySubmittedDate = (ZDateTime)JE_EntrySubmittedDateInfo.OriginalValue;
				}
				else
				{
					SetMessagingStatusToNotSent(this);
				}
			}
		}

		#endregion

		public override object GetService(Type serviceType)
		{
			if (serviceType == typeof(ICustomsCharges))
			{
				return new JobDeclarationCustomsCharges(this);
			}

			return base.GetService(serviceType);
		}

		#region New JobDocAddresses

		#region NotifyPartyDocAddress

		public ZBool NotifyPartyDocumentaryAddressVisible
		{
			get { return IsTSWCREWriteOff; }
		}

		#endregion

		#region DeliveryDestinationPartyDocAddress

		public JobDocAddress DeliveryDestinationPartyDocAddress
		{
			get
			{
				if (deliveryDestinationPartyDocAddress == null || deliveryDestinationPartyDocAddress.IsDeleted)
				{
					if (deliveryDestinationPartyDocAddress != null)
					{
						deliveryDestinationPartyDocAddress.DocAddressChanged -= DeliveryDestinationPartyDocAddress_DocAddressChanged;
					}

					deliveryDestinationPartyDocAddress = DocAddresses.FindOrCreateWithRequirement(DeliveryDestinationPartyDocAddressRequirement);
					deliveryDestinationPartyDocAddress.DocAddressChanged += DeliveryDestinationPartyDocAddress_DocAddressChanged;
				}

				return deliveryDestinationPartyDocAddress;
			}
		}

		protected virtual void DeliveryDestinationPartyDocAddress_DocAddressChanged(object sender, EventArgs e)
		{
		}

		JobDocAddress deliveryDestinationPartyDocAddress;

		JobDocAddressRequirement DeliveryDestinationPartyDocAddressRequirement
		{
			get
			{
				if (deliveryDestinationPartyDocAddressRequirement == null)
				{
					deliveryDestinationPartyDocAddressRequirement = new JobDocAddressRequirement(DocAddressType.ImporterPickupDeliveryAddress, ContactType.Administration);
					DocAddressManager.AddRequirement(deliveryDestinationPartyDocAddressRequirement);
				}
				return deliveryDestinationPartyDocAddressRequirement;
			}
		}
		JobDocAddressRequirement deliveryDestinationPartyDocAddressRequirement;

		public ZBool DeliveryDestinationPartyDocAddressVisible
		{
			get { return IsTSWCREWriteOff || IsTSWExportDeclaration || IsTSWImportDeclaration; }
		}

		#endregion

		#region TreatmentProviderDocAddress

		public JobDocAddress TreatmentProviderDocAddress
		{
			get
			{
				if (treatmentProviderDocAddress == null || treatmentProviderDocAddress.IsDeleted)
				{
					treatmentProviderDocAddress = DocAddresses.FindOrCreateWithRequirement(TreatmentProviderDocAddressRequirement);
				}
				return treatmentProviderDocAddress;
			}
		}
		JobDocAddress treatmentProviderDocAddress;

		JobDocAddressRequirement TreatmentProviderDocAddressRequirement
		{
			get
			{
				if (treatmentProviderDocAddressRequirement == null)
				{
					treatmentProviderDocAddressRequirement = new JobDocAddressRequirement(DocAddressType.CustomsTreatmentProviderAddress, AddressType.DLV, ContactType.TransportServices);
					DocAddressManager.AddRequirement(treatmentProviderDocAddressRequirement);
				}
				return treatmentProviderDocAddressRequirement;
			}
		}
		JobDocAddressRequirement treatmentProviderDocAddressRequirement;

		#endregion

		#region DeliveryNotificationPort

		[MaxLength(RefUNLOCO.Schema.RL_CodeMaxLength)]
		[RelatedBusinessObject(nameof(DeliveryNotificationPort))]
		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.DeliveryNotifyPortList))]
		public ZString JE_RL_NKPortOfDeliveryNotify
		{
			get => this.GetSystemDefinedValue<ZString>(Schema.JE_RL_NKPortOfDeliveryNotify);
			set
			{
				value = value.TrimEndSpaceTab().ToUpperInvariant().Left(RefUNLOCO.Schema.RL_CodeMaxLength);
				if (value != JE_RL_NKPortOfDeliveryNotify)
				{
					this.SetSystemDefinedValue(Schema.JE_RL_NKPortOfDeliveryNotify, AddOnColumnDataType.Codes.String, value);
					JE_RL_NKPortOfDeliveryNotifyInfo.RefreshBinding();

					if (!IsValidationSuspended)
					{
						Validation.ValidateJE_RL_NKPortOfDeliveryNotify();
					}
				}
			}
		}

		public ZPropertyInfo JE_RL_NKPortOfDeliveryNotifyInfo => GetZPropertyInfo(Schema.JE_RL_NKPortOfDeliveryNotify);

		public RefUNLOCO DeliveryNotificationPort => Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, JE_RL_NKPortOfDeliveryNotify);

		#endregion

		protected override DocAddressType[] SupportedAddressTypesCore
		{
			get
			{
				return base.SupportedAddressTypesCore.Concat(new[]
				{
					DocAddressType.CustomsTreatmentProviderAddress
				}).ToArray();
			}
		}

		protected override JobDocAddressRequirement GetDocAddressRequirement(DocAddressType addressType)
		{
			switch (addressType)
			{
				case DocAddressType.CustomsTreatmentProviderAddress:
					return TreatmentProviderDocAddressRequirement;
				default:
					return base.GetDocAddressRequirement(addressType);
			}
		}
		#endregion

		public DocManagerInfo GetDocManagerInfo()
		{
			var eDocsParent = (IDocManagerSupport)Shipment ?? this;
			return eDocsParent.DocManagerInfo;
		}

		protected override BaseJobDeclaration.DeclarationDocManagerInfo GetNewDocManagerInfo()
		{
			return new DeclarationDocManagerInfo(this);
		}

		public new class DeclarationDocManagerInfo : BaseJobDeclaration.DeclarationDocManagerInfo
		{
			public DeclarationDocManagerInfo(JobDeclaration parent)
				: base(parent)
			{
			}

			JobDeclaration Declaration
			{
				get { return (JobDeclaration)BusinessEntity; }
			}

			protected override BusinessObject[] GetEDocsChildrenForAFreightJobToDisplayCore()
			{
				var query = new ZQuery(EDIMessageSchema.EM_LinkUniqueID, Declaration.PK);
				return (from NZMMessage message in Declaration.Factory.Load<NZMMessage>(query)
						where message.DocManagerInfo.AllEDocs.Count != 0
						select message).ToArray();
			}
		}

		public List<IStorageDocsBaseCollection> eDocsForSelection
		{
			get
			{
				if (ConsolidatedDeclaration.IsConsolidated(this))
				{
					var eDocsStorageForSelection = new List<IStorageDocsBaseCollection>();
					var consolidatedDeclaration = ConsolidatedDeclaration.GetConsolidatedDeclaration(this);
					foreach (JobDeclaration dec in consolidatedDeclaration.JobDeclarations)
					{
						eDocsStorageForSelection.AddRange(dec.eDocsForSelectionOnSingleDeclaration);
					}
					return eDocsStorageForSelection;
				}
				else
				{
					return eDocsForSelectionOnSingleDeclaration;
				}
			}
		}
		List<IStorageDocsBaseCollection> eDocsForSelectionOnSingleDeclaration
		{
			get
			{
				var eDocsStorageForSelection = new List<IStorageDocsBaseCollection>();
				eDocsStorageForSelection.Add(DocManagerInfo.AllEDocs);
				if (Shipment != null)
				{
					eDocsStorageForSelection.Add(Shipment.DocManagerInfo.AllEDocs);
				}
				return eDocsStorageForSelection;
			}
		}

		#region ICusAddInfoTypeSupporter Members

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return new Customs.Business.FetchStrategies.CusAddInfoTypeSupporterFetchStrategy(this);
		}

		IDictionary<ZString, Type> ICusAddInfoTypeSupporter.GetCusAddInfoTypes()
		{
			var result = new Dictionary<ZString, Type>();
			result.Add(CusAddInfoTypeAttribute.Codes.NZMAFFiles, typeof(CusAddInfo<MAFFile>));
			return result;
		}

		#endregion

		protected override ZString GetTransportModeGeneric()
		{
			switch (JE_TransportMode)
			{
				case JobTransportModeList.Codes.Air:
					return TransportTypeGenericList.Codes.Air;
				case JobTransportModeList.Codes.Post:
					return TransportTypeGenericList.Codes.PostMail;
				case JobTransportModeList.Codes.Sea:
					return TransportTypeGenericList.Codes.Sea;
			}
			return ZString.Empty;
		}

		protected override IEnumerable<ZString> GetMessageTypesForDocumentFilter()
		{
			if (IsImport || IsExWarehouse)
			{
				yield return JobMessageTypeList.Codes.Import;
			}
			else if (IsExport)
			{
				yield return JobMessageTypeList.Codes.Export;
				if (IsDrawback)
				{
					yield return JobMessageSubTypeList.Codes.Drawback;
				}
			}
		}
		protected override ZString GetMessageTypeForHSAssist() => JE_MessageType == NZJobMessageTypeList.Codes.Excise
			? SharedJobMessageTypeList.Codes.Import
			: base.GetMessageTypeForHSAssist();

		protected override Customs.Business.JobDeclarationConsolidatedEntryProvider GetConsolidatedEntryProvider() => new JobDeclarationConsolidatedEntryProvider(this, new ConsolidatedEntryDeclarationRemover());

		#region IAddInfoManager Members

		IAddInfo IAddInfoManager.AddInfo
		{
			get { return AddInfo; }
		}

		#endregion

		#region IApportionInvoiceHolder Members
		string IApportionInvoiceHolder.CountryContext
		{
			get { return IsTSWImportDeclaration ? NZTSW : Core.Constants.CountryCodes.NewZealand; }
		}
		internal const string NZTSW = "NZTSW";
		#endregion

#if DEBUG
		public override ZString TransportModeMailCodeForTesting
		{
			get { return JobTransportModeList.Codes.Post; }
		}
#endif

		event EventHandler IManifestProvider.CustomsManifestVisibilityChanged
		{
			add
			{
				JE_TransportModeInfo.ValueChanged += value;
				JE_MessageTypeInfo.ValueChanged += value;
			}
			remove
			{
				JE_TransportModeInfo.ValueChanged -= value;
				JE_MessageTypeInfo.ValueChanged -= value;
			}
		}

		EDIMessageCollection IManifestProvider.Messages
		{
			get
			{
				if (fManifestMessages == null)
				{
					fManifestMessages = new EDIMessageCollection(this, Factory);
					fManifestMessages.Load();
					fManifestMessages.IsManagedForDataRefresh = true;
				}
				return fManifestMessages;
			}
		}
		EDIMessageCollection fManifestMessages;

		public TranshipmentRequest TranshipmentRequest
		{
			get
			{
				if (fTranshipmentRequest == null)
				{
					fTranshipmentRequest = TranshipmentRequest.Load(this);
					if (fTranshipmentRequest != null)
					{
						RegisterEditableChildObject(fTranshipmentRequest);
						RegisterListChangedCalledRefreshBinding(fTranshipmentRequest);
					}
				}
				return fTranshipmentRequest;
			}
		}

		TranshipmentRequest fTranshipmentRequest;

		#region ISourceIdentifierProvider

		ZGuid ISourceIdentifierProvider.SourceIdentifier => ConsolidatedDec?.PK ?? PK;

		#endregion
	}
}
