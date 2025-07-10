using System;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.InterfaceImplementations;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Customs.US;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	[SystemDefinedValues]
	[ProvideMetaDataProperty("ShouldPropertiesBeReadOnly", MetaDataTypes.ReadOnly)]
	[DependentBusinessObject(typeof(CusStatementHeader), "StatementLines")]
	[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
	public class CusStatementLine : BaseCusStatementLine,
		IStatementDeleteTransaction,
		IAccInvoiceDataProvider,
		IUSCustomsChargeEntry,
		Integration.Customs.US.ICusStatementLine,
		ICustomsChargeEntry,
		IControllerIDProvider
	{
		public CusStatementLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Fetch Strategy

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new Strategy(this);
		}

		class Strategy : EnterpriseBusinessObjectFetchStrategy
		{
			public Strategy(CusStatementLine statementLine)
				: base(statementLine)
			{
			}

			CusStatementLine StatementLine
			{
				get { return (CusStatementLine)BusinessObject; }
			}

			protected override void FetchForLoadChildEditableObjectsCore()
			{
				base.FetchForLoadChildEditableObjectsCore();
				Factory.AddFetchHint(CusStatementLineChargeSchema.B4_B3, StatementLine.PK);
			}

			protected override void FetchForValidateCore()
			{
				base.FetchForValidateCore();
				Factory.AddFetchHint(CusStatementLineChargeSchema.B4_B3, StatementLine.PK);
			}
		}

		#endregion

		public new class Schema : AutoCusStatementLine.Schema
		{
			public const string APFullyPaid = "APFullyPaid";
			public const string APPostedAmount = "APPostedAmount";
			public const string APUnPostedAmount = "APUnPostedAmount";
			public const string APTotalAmount = "APTotalAmount";
			public const string ARPostedAmount = "ARPostedAmount";
			public const string ARUnPostedAmount = "ARUnPostedAmount";
			public const string ARTotalAmount = "ARTotalAmount";
			public const string DifferenceBetweenARInvoiceAndCustomsAmount = "DifferenceBetweenARInvoiceAndCustomsAmount";
			public const string DifferenceBetweenAPInvoiceAndCustomsAmount = "DifferenceBetweenAPInvoiceAndCustomsAmount";
			public const string HasDiscrepancyBetweenInvoicesAndCustomsAmount = "HasDiscrepancyBetweenInvoicesAndCustomsAmount";
			public const string DeclarationPK = "DeclarationPK";
			public const string ReleaseStatus = "ReleaseStatus";
		}

		#region Loader

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
		public new class Loader : Customs.Business.AutoCusStatementLine.Loader
		{
			public Loader(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			protected override Type GetTypeOfBusinessObjectToLoad()
			{
				return typeof(CusStatementLine);
			}

			/// <summary>
			/// Will return the first non-deleted CusStatementLine which matches entry number and filer code.
			/// </summary>
			public CusStatementLine LoadTop1NotDeleted(ZString entryNumber, ZString filerCode, ZGuid companyPK)
			{
				return GetStatementLine(filerCode, entryNumber, SQLComparisonOperator.NotEqual, companyPK);
			}

			public CusStatementLine GetStatementLine(ZString entryFilerCode, ZString entryNumber, SQLComparisonOperator comparisonOpForDeletedStatus, ZGuid companyPK)
			{
				var query = new ZQuery(CusStatementLineSchema.B3_EntryNum, entryNumber);
				query.AddToFilter(CusStatementLineSchema.B3_EntryFilerCode, entryFilerCode);
				query.AddToFilter(CusStatementLineSchema.B3_Status, comparisonOpForDeletedStatus, StatementLineStatusList.Codes.Deleted);
				query.OrderBy = CusStatementLineSchema.Constants.B3_SystemCreateTimeUtc + OrderByClause.Descending;

				var statementLines = Factory.Load<BaseCusStatementLine>(query);//cannot assume it is a US statementline here without checking companyPK
				return (CusStatementLine)statementLines.FirstOrDefault(s => s.StatementHeader.B2_GC == companyPK);
			}
		}

		#endregion

		#region New Properties

		protected bool GetShouldPropertiesBeReadOnly(PropertyDescriptor property)
		{
			return true;
		}

		public ZBool IsPaid
		{
			get { return StatementHeader != null && StatementHeader.IsPaid; }
		}

		public ZBool IsActive
		{
			get { return B3_Status == StatementLineStatusList.Codes.Active; }
		}

		public ZBool IsStatusDeleted
		{
			get { return B3_Status == StatementLineStatusList.Codes.Deleted; }
		}

		public ZBool IsDeletionPending
		{
			get { return B3_Status == StatementLineStatusList.Codes.DeletionPending; }
		}

		public ZBool HasAmountsToPay
		{
			get { return B3_CustomsFeesTotal > 0; }
		}

		public ZDecimal BrokerPaymentAmount
		{
			get { return StatementHeader.IsPaidByBroker ? B3_CustomsFeesTotal : ZDecimal.Zero; }
		}

		public override void Delete()
		{
			Charges.RemoveAndDeleteAll();
			base.Delete();
		}

		public void ChangeLineStatus(bool isActive)
		{
			B3_Status = isActive ? StatementLineStatusList.Codes.Active : StatementLineStatusList.Codes.Deleted;

			if (!isActive && StatementHeader.StatementLines.AreAllLinesDeleted && !StatementHeader.IsStatusDeleted)
			{
				StatementHeader.B2_Status = StatementHeaderStatusList.Codes.Deleted;
			}
			else if (isActive && !StatementHeader.IsPreliminary)
			{
				StatementHeader.B2_Status = StatementHeaderStatusList.Codes.Preliminary;
			}
		}

		public ZBool APFullyPaid
		{
			get { return AccountingAP_ARInvoiceQueryResult.APFullyPaid; }
		}

		public ZPropertyInfo APFullyPaidInfo
		{
			get { return GetZPropertyInfo(Schema.APFullyPaid); }
		}

		public ZDecimal APPostedAmount
		{
			get { return AccountingAP_ARInvoiceQueryResult.APPostedAmount; }
		}

		public ZPropertyInfo APPostedAmountInfo
		{
			get { return GetZPropertyInfo(Schema.APPostedAmount); }
		}

		public ZDecimal APUnPostedAmount
		{
			get { return AccountingAP_ARInvoiceQueryResult.APUnPostedAmount; }
		}

		public ZPropertyInfo APUnPostedAmountInfo
		{
			get { return GetZPropertyInfo(Schema.APUnPostedAmount); }
		}

		public ZDecimal APTotalAmount
		{
			get { return APPostedAmount + APUnPostedAmount; }
		}

		public ZPropertyInfo APTotalAmountInfo
		{
			get { return GetZPropertyInfo(Schema.APTotalAmount); }
		}

		public ZDecimal ARPostedAmount
		{
			get { return AccountingAP_ARInvoiceQueryResult.ARPostedAmount; }
		}

		public ZPropertyInfo ARPostedAmountInfo
		{
			get { return GetZPropertyInfo(Schema.ARPostedAmount); }
		}

		public ZDecimal ARUnPostedAmount
		{
			get { return AccountingAP_ARInvoiceQueryResult.ARUnPostedAmount; }
		}

		public ZPropertyInfo ARUnPostedAmountInfo
		{
			get { return GetZPropertyInfo(Schema.ARUnPostedAmount); }
		}

		public ZDecimal ARTotalAmount
		{
			get { return ARPostedAmount + ARUnPostedAmount; }
		}

		public ZPropertyInfo ARTotalAmountInfo
		{
			get { return GetZPropertyInfo(Schema.ARTotalAmount); }
		}

		public ZDecimal DifferenceBetweenAPInvoiceAndCustomsAmount
		{
			get
			{
				return Math.Abs(BrokerPaymentAmount - (APPostedAmount + APUnPostedAmount));
			}
		}

		public ZPropertyInfo DifferenceBetweenAPInvoiceAndCustomsAmountInfo
		{
			get { return GetZPropertyInfo(Schema.DifferenceBetweenAPInvoiceAndCustomsAmount); }
		}

		public ZDecimal DifferenceBetweenARInvoiceAndCustomsAmount
		{
			get
			{
				return Math.Abs(BrokerPaymentAmount - (ARPostedAmount + ARUnPostedAmount));
			}
		}

		public ZPropertyInfo DifferenceBetweenARInvoiceAndCustomsAmountInfo
		{
			get { return GetZPropertyInfo(Schema.DifferenceBetweenARInvoiceAndCustomsAmount); }
		}

		public ZBool HasDiscrepancyBetweenInvoicesAndCustomsAmount
		{
			get
			{
				return DifferenceBetweenAPInvoiceAndCustomsAmount != 0m ||
					DifferenceBetweenARInvoiceAndCustomsAmount != 0m;
			}
		}

		public void RefreshAccountingAP_ARInvoiceQueryResult()
		{
			accountingAP_ARInvoiceQueryResult = null;

			RefreshBinding();
		}

		internal AP_ARInvoiceQueryResult AccountingAP_ARInvoiceQueryResult
		{
			get
			{
				if (!accountingAP_ARInvoiceQueryResult.HasValue)
				{
					accountingAP_ARInvoiceQueryResult = new AP_ARInvoiceQueryResult();
					var declaration = Declaration;
					if (declaration != null)
					{
						var invoiceQuery = GetAccountingAP_ARInvoiceQuery();

						accountingAP_ARInvoiceQueryResult = invoiceQuery.GetInvoiceAmount(declaration, RatingDataRegistry.Instance.EntryChargeTypesAndCodes.GetAllChargeCodesIncludingDefault());
					}
				}
				return accountingAP_ARInvoiceQueryResult.Value;
			}
		}
		AP_ARInvoiceQueryResult? accountingAP_ARInvoiceQueryResult;

#if DEBUG
		internal IAccountingAP_ARInvoiceQuery AccInvQueryExposedForTesting;
#endif
		protected virtual IAccountingAP_ARInvoiceQuery GetAccountingAP_ARInvoiceQuery()
		{
			IAccountingAP_ARInvoiceQuery result = ObjectFactory.Get<IAccountingAP_ARInvoiceQuery>();
#if DEBUG
			if (AccInvQueryExposedForTesting != null)
			{
				result = AccInvQueryExposedForTesting;
			}
#endif
			return result;
		}

		[List(nameof(Lookups) + "." + nameof(CusStatementLineLookups.Declarations))]
		public ZGuid DeclarationPK
		{
			get
			{
				var declaration = Declaration;
				return declaration != null ? declaration.PK : ZGuid.Empty;
			}
		}

		public ZPropertyInfo DeclarationPKInfo
		{
			get { return GetZPropertyInfo(Schema.DeclarationPK); }
		}

		#endregion

		#region Related Objects

		[ChildEditable(true)]
		public CusStatementLineChargeCollection Charges
		{
			get
			{
				if (fCharges == null)
				{
					fCharges = new CusStatementLineChargeCollection(this);
					fCharges.Load();
					RegisterEditableChildObject(fCharges);
				}
				return fCharges;
			}
		}
		CusStatementLineChargeCollection fCharges;

		public IStatementLineDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					var loadedDeclaration = CusEntryNumberExtension.GetJobDeclarationByEntryNumberAndFilerCode(Factory, B3_EntryNum, B3_EntryFilerCode);
					if (loadedDeclaration != null)
					{
						if (loadedDeclaration.IsReconMessageType)
						{
							declaration = ReconDeclaration.Get(loadedDeclaration);
						}
						else
						{
							declaration = loadedDeclaration;
						}
					}
				}

				return declaration;
			}
		}
		IStatementLineDeclaration declaration;

		#endregion

		#region BusinessObjects override

		public override bool CanDelete
		{
			get { return false; }
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get
			{
				return ResString.GetMultilingualString("C21F5EDC-85CE-4DE0-BAF7-AB3A2B52CC18", "Entries on statements are created from Customs messages. You cannot delete them this way.");
			}
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			B3_Status = StatementLineStatusList.Codes.Active;
		}

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);

			if (!saveSucceeded)
			{
				if (IsInDatabase && B3_StatusInfo.HasChanges)
				{
					B3_Status = (ZString)B3_StatusInfo.OriginalValue;
				}
			}
		}

		#endregion

		#region Properties For Documents

		public ZDecimal UserFees
		{
			get
			{
				ZDecimal result = 0m;

				foreach (CusStatementLineCharge charge in Charges)
				{
					if (EntryChargeTypeList.IsFeeType(charge.B4_ChargeType) && EntryTypeList.IsCustomsChargeIncludedInTotalAmountDue(B3_EntryType, charge.B4_ChargeType))
					{
						result += charge.B4_ChargeAmount;
					}
				}

				result += GetPayableAmount(Core.Constants.USCustoms.FeeCodes.ReconciliationInterest);

				return result;
			}
		}

		public ZDecimal EstimatedDuty
		{
			get
			{
				return GetPayableAmount(Core.Constants.USCustoms.FeeCodes.Duty);
			}
		}

		public ZDecimal EstimatedTax
		{
			get { return GetAmount(Core.Constants.USCustoms.FeeCodes.ExciseTaxPayable) + GetAmount(Core.Constants.USCustoms.FeeCodes.ExciseTaxDeferred); }
		}

		public ZBool IsDeferredTaxIndicator
		{
			get
			{
				ZDecimal result = GetAmount(Core.Constants.USCustoms.FeeCodes.ExciseTaxDeferred);
				return result != 0;
			}
		}

		public ZDecimal EstimatedCVD
		{
			get { return GetPayableAmount(Core.Constants.USCustoms.FeeCodes.CountervailingDuty); }
		}

		public ZDecimal EstimatedADD
		{
			get { return GetPayableAmount(Core.Constants.USCustoms.FeeCodes.AntidumpingDuty); }
		}

		public ZString InterestForReconciliationIndicator
		{
			get
			{
				ZDecimal result = GetAmount(Core.Constants.USCustoms.FeeCodes.ReconciliationInterest);
				return result > ZDecimal.Zero ? "I" : "";
			}
		}

		/// <summary>
		/// For warehouse entries, only HMF is payable even though duty and other fees appear in statement messages.
		/// </summary>
		internal ZDecimal GetPayableAmount(string chargeType)
		{
			return EntryTypeList.IsCustomsChargeIncludedInTotalAmountDue(B3_EntryType, chargeType) ?
				GetAmount(chargeType) : ZDecimal.Zero;
		}

		/// <summary>
		/// Returns an amount that appears on a statement
		/// </summary>
		internal ZDecimal GetAmount(string chargeType)
		{
			CusStatementLineCharge charge = Charges.GetFirstCharge(chargeType);

			return charge == null ? ZDecimal.Zero : charge.B4_ChargeAmount;
		}

		public ZString FormattedEntryNumber
		{
			get { return B3_EntryFilerCode + "-" + B3_EntryNum.SubstringSafe(0, 7) + "-" + B3_EntryNum.SubstringSafe(7, 1); }
		}

		#endregion

		#region Properties For Binding

		[DecimalPlaces(2)]
		public override ZDecimal B3_CustomsFeesTotal
		{
			get { return base.B3_CustomsFeesTotal; }
			set { base.B3_CustomsFeesTotal = value; }
		}

		public ZString LineStatusDescription
		{
			get { return Lookups.StatementLineStatusList.GetDescriptionFromCode(B3_Status); }
		}

		public ZPropertyInfo LineStatusDescriptionInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(nameof(LineStatusDescription)); }
		}

		public ZString EntryStatusDescription
		{
			get
			{
				ZString result = Lookups.StatementEntryStatus.GetDescriptionFromCode(B3_EntryStatus);
				if (result.IsEmpty)
				{
					var jobDeclaration = Declaration;
					if (jobDeclaration != null && !jobDeclaration.IsACE)
					{
						result = "Documents Required";
					}
					else
					{
						result = StatementEntryStatus.Descriptions.Paperless;
					}
				}
				return result;
			}
		}

		public ZPropertyInfo EntryStatusDescriptionInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(nameof(EntryStatusDescription)); }
		}

		public ZString JobDeclarationBrokerReferenceNumber
		{
			get
			{
				var declaration = Declaration;
				return declaration != null ? declaration.BrokerReferenceNumber : ZString.Empty;
			}
		}

		public ZPropertyInfo JobDeclarationBrokerReferenceNumberInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(nameof(JobDeclarationBrokerReferenceNumber)); }
		}

		public ZBool IsElectronicInvoiceRequired
		{
			get { return B3_EIIndicator == YesNoDefaultList.Codes.Yes; }
		}

		public ZString ReleaseStatus
		{
			get
			{
				var declaration = Declaration;
				return declaration != null ? declaration.ReleaseStatus : ZString.Empty;
			}
		}

		public ZPropertyInfo ReleaseStatusInfo
		{
			get { return GetZPropertyInfo(Schema.ReleaseStatus); }
		}

		public ZString ReleaseStatusDescription
		{
			get
			{
				var declaration = Declaration;
				return declaration != null ? declaration.ReleaseStatusDescription : ZString.Empty;
			}
		}

		#endregion

		#region IStatementDeleteAndAddEntity Members

		ZString IStatementDeleteTransaction.EntryNumber
		{
			get { return B3_EntryNum; }
		}

		ZString IStatementDeleteTransaction.EntryFilerCode
		{
			get { return B3_EntryFilerCode; }
		}

		ZString IStatementDeleteTransaction.ProcessingPort
		{
			get { return StatementHeader.B2_ProcessPort; }
		}

		ZString IStatementDeleteTransaction.PortOfEntry
		{
			get { return B3_EntryProcessPort; }
		}

		bool IStatementDeleteTransaction.ShouldPopulatePreparerSite
		{
			get
			{
				var declaration = Declaration;
				return declaration != null && declaration.IsRemoteLocationFiling;
			}
		}

		ZString IStatementDeleteTransaction.PreparerPort
		{
			get
			{
				var declaration = Declaration;
				return declaration != null ? declaration.US_PreparerDistrictPort : ZString.Empty;
			}
		}

		ZString IStatementDeleteTransaction.PreparerOfficeCode
		{
			get
			{
				var declaration = Declaration;
				return declaration != null ? declaration.US_PreparerOfficeCode : ZString.Empty;
			}
		}

		bool IStatementDeleteTransaction.IsACE
		{
			get
			{
				var declaration = Declaration;
				return declaration != null && declaration.IsACE;
			}
		}

		BusinessObjectFactory IStatementDeleteTransaction.Factory
		{
			get { return Factory; }
		}

		GlbBranch IStatementDeleteTransaction.Branch
		{
			get
			{
				var declaration = Declaration;
				return declaration != null ? declaration.Branch : Factory.Load<GlbBranch>(GlbBranch.CurrentBranch.PK);
			}
		}

		void IStatementDeleteTransaction.AddMessages(MQEDIMessage message)
		{
			var declaration = Declaration;

			if (declaration != null)
			{
				declaration.Messages.Add(message);
			}
			else
			{
				StatementHeader.Messages.Add(message);
			}
		}

		ZString IStatementDeleteTransaction.PaymentType
		{
			get { return StatementHeader.B2_PaymentType; }
		}

		ZDateTime IStatementDeleteTransaction.PreliminaryStatementPrintDate
		{
			get { return ZDateTime.Empty; }
		}

		ZDateTime IStatementDeleteTransaction.ReleaseDate
		{
			get { return ZDateTime.Empty; }
		}

		ZString IStatementDeleteTransaction.ClientBranchDesignation
		{
			get { return StatementHeader.B2_BranchDesignation; }
		}

		ZString IStatementDeleteTransaction.PeriodicStatementMonth
		{
			get
			{
				var declaration = Declaration;
				return declaration != null ? declaration.US_PeriodicStatementMM : ZString.Empty;
			}
		}

		bool IStatementDeleteTransaction.ShouldGenerateACEStatementMessage
		{
			get
			{
				var transaction = Declaration as IStatementDeleteTransaction;
				return transaction?.ShouldGenerateACEStatementMessage ?? true;
			}
		}

		bool IStatementDeleteTransaction.IsStatementUpdateMessagePending
		{
			get
			{
				var transaction = Declaration as IStatementDeleteTransaction;
				return transaction?.IsStatementUpdateMessagePending ?? false;
			}
		}

		Guid IStatementDeleteTransaction.RegistryCompanyPK
		{
			get
			{
				var declaration = Declaration;
				return declaration != null ? declaration.RegistryCompanyPK : GlbCompany.CurrentCompany.PK.ToGuid();
			}
		}

		#endregion

		#region IAccInvoiceDataProvider Members

		ICustomsCharges[] IAccInvoiceDataProvider.CustomsCharges
		{
			get
			{
				ICustomsCharges customsCharges = new CusEntryHeaderCustomsCharges(this);
				return new ICustomsCharges[] { new CusEntryHeaderCustomsCharges.CustomsChargeCache(customsCharges) };
			}
		}

		ICustomsJobInfo IAccInvoiceDataProvider.CustomsJob
		{
			get { return Declaration; }
		}

		AutoPostingNotification IAccInvoiceDataProvider.AutoPostingNotification
		{
			get
			{
				var customsJob = ((IAccInvoiceDataProvider)this).CustomsJob;
				if (customsJob != null)
				{
					return customsJob.AutoPostingNotification;
				}
				else
				{
					var groupNotification = CustomsDataRegistry.Instance.AutoBillingEmailNotificationGroup.Value;
					return new AutoPostingNotification(new ZGuid[] { groupNotification.SendGroupPK }, groupNotification.SuppressUnpostARNotificaiton);
				}
			}
		}

		ZDateTime IAccInvoiceDataProvider.InvoiceDate => StatementHeader?.PaymentDateCalculated ?? CustomsWorkingDays.GetInstance(Factory).GetAnotherStandardWorkingDay(ZDate.Today.ToDateTime(), -1).Date;

		string IAccInvoiceDataProvider.EntryWithdrawnStatusTerm
		{
			get { return "deleted"; }
		}

		ZDateTime IAccInvoiceDataProvider.APDueDate
		{
			get
			{
				var result = ZDateTime.Empty;

				var statementHeader = this.StatementHeader;

				if (statementHeader != null)
				{
					result = statementHeader.B2_PaymentAuthorizationDate.IsValid ? statementHeader.B2_PaymentAuthorizationDate : statementHeader.B2_PrintDate;

					if (statementHeader.IsPeriodicDailyStatement)
					{
						result = statementHeader.B2_PrintDate;

						var declaration = Declaration;
						if (result.IsValid && declaration != null)
						{
							var workingDays = CustomsWorkingDays.GetInstance(Factory);

							int month = ZInt.ParseSafe(declaration.US_PeriodicStatementMM, result.Month);
							int year = month != result.Month && month == 1 ? result.Year + 1 : result.Year;

							result = workingDays.GetAnotherStandardWorkingDay(new DateTime(year, month, 1), 14);
						}
					}
				}

				return result.IsEmpty || result < ZDateTime.Today ? ZDateTime.Today : result;
			}
		}

		bool IAccInvoiceDataProvider.IsBillable
		{
			get { return Declaration != null; }
		}

		string IAccInvoiceDataProvider.ReasonForUnbillability
		{
			get { return ((IAccInvoiceDataProvider)this).IsBillable ? "" : "No entry exists in the system with this entry number."; }
		}

		ZString IAccInvoiceDataProvider.UniqueNumber
		{
			get { return B3_EntryNum; }
		}

		ZString IAccInvoiceDataProvider.PreviousUniqueNumber
		{
			get { return ZString.Empty; }
		}

		bool IAccInvoiceDataProvider.HasBeenWithdrawn
		{
			get { return HasEntryBeenWithdrawn; }
		}

		bool IAccInvoiceDataProvider.IsEligibleForIntegration
		{
			get { return B3_Status != StatementLineStatusList.Codes.Deleted; }
		}

		bool IAccInvoiceDataProvider.APInvoiceNumberAlwaysIncludeChargeCode => false;

		bool IAccInvoiceDataProvider.MatchCustomsChargesToClear(ZString apInvoiceNumber, ZString description) => true;

		bool IAccInvoiceDataProvider.IsAutoBillingDueDateFromPaymentTerms => CustomsDataRegistry.Instance.AutoBillingDueDateFromPaymentTerms.Value;
		#endregion

		#region ICustomsChargeEntry Members

		JobHeader ICustomsChargeEntry.Job => null;

		ZString ICustomsChargeEntry.UniqueNumber => ZString.Empty;

		ZString ICustomsChargeEntry.PreviousUniqueNumber => ZString.Empty;

		public bool HasEntryBeenWithdrawn
		{
			get
			{
				var declaration = Declaration;
				return declaration != null && declaration.HasEntryBeenWithdrawn;
			}
		}

		ZString ICustomsChargeEntry.LocalCurrencyCode
		{
			get
			{
				var declaration = Declaration;
				return declaration != null ? Declaration.LocalCurrencyCode : ZString.Empty;
			}
		}

		bool ICustomsChargeEntry.HasBeenWithdrawn
		{
			get { return HasEntryBeenWithdrawn; }
		}

		Registry.Business.Customs.EntryChargeTypeList ICustomsChargeEntry.EntryChargeTypeList
		{
			get { return new EntryChargeTypeList(Factory); }
		}

		CustomsCharge[] ICustomsChargeEntry.GetNonFeeCountrySpecificCharges()
		{
			return Array.Empty<CustomsCharge>();
		}

		ZDecimal ICustomsChargeEntry.GetTotalChargeValueFor(Registry.Business.Customs.EntryChargeType chargeType, ZString methodOfPaymentCode)
		{
			CusStatementLineCharge charge = Charges.GetFirstCharge(chargeType.Code);
			return charge != null ? charge.B4_ChargeAmount : ZDecimal.Zero;
		}

		ZString[] ICustomsChargeEntry.GetMethodsOfPaymentThatCanInfluenceAutoRating() => new ZString[] { ZString.Empty };

		bool ICustomsChargeEntry.IsFeePaidByBroker(string chargeType, ZString methodOfPaymentCode, ILogger logger)
		{
			return
				StatementHeader != null && StatementHeader.IsPaidByBroker &&
				EntryTypeList.IsCustomsChargeIncludedInTotalAmountDue(B3_EntryType, chargeType);
		}

		ZString ICustomsChargeEntry.ReferenceNumber
		{
			get { return ZString.Empty; }// Not Supported
		}

		ZString IUSCustomsChargeEntry.EntryFilerCode
		{
			get { return B3_EntryFilerCode; }
		}

		ZString IUSCustomsChargeEntry.EntryNumber
		{
			get { return B3_EntryNum; }
		}

		ZString IUSCustomsChargeEntry.PaymentType
		{
			get { return StatementHeader != null ? StatementHeader.B2_PaymentType : ZString.Empty; }
		}

		ZDate IUSCustomsChargeEntry.DueDate
		{
			get { return StatementHeader != null ? StatementHeader.B2_DueDate.Date : ZDate.Empty; }
		}

		bool IUSCustomsChargeEntry.IsPaidByImporter
		{
			get { return StatementHeader != null && !StatementHeader.IsPaidByBroker; }
		}

		ZGuid ICustomsChargeEntry.CreditorPK
		{
			get
			{
				OrgHeader org = Factory.Load<OrgHeader>(Registry.Business.RatingDataRegistry.Instance.CustomsDisbursementCreditor.GetFallBackValueAtAllLevels(StatementHeader.B2_GC.ToGuid(), Guid.Empty, Guid.Empty));
				return org == null ? ZGuid.Empty : org.PK;
			}
		}

		bool ICustomsChargeEntry.EntryReferenceInChargeDescSupported
		{
			get { return false; }
		}

		#endregion

		#region type safe

		public new CusStatementHeader StatementHeader
		{
			get { return (CusStatementHeader)base.StatementHeader; }
		}

		public new CusStatementLineValidation Validation
		{
			get { return (CusStatementLineValidation)base.Validation; }
		}

		public new CusStatementLineLookups Lookups
		{
			get { return (CusStatementLineLookups)base.Lookups; }
		}

		protected override Customs.Business.CusStatementLineLookups GetNewLookups()
		{
			return new CusStatementLineLookups(this);
		}

		protected override Customs.Business.CusStatementLineValidation GetNewValidation()
		{
			return new CusStatementLineValidation(this);
		}

		#endregion

		#region IControllerIDProvider Members

		IControllerIDProvider ControllerIDProvider
		{
			get { return StatementHeader; }
		}

		ControllerID IControllerIDProvider.ControllerID
		{
			get
			{
				var controllerIDProvider = ControllerIDProvider;
				return controllerIDProvider != null ? controllerIDProvider.ControllerID : null;
			}
		}

		Guid IControllerIDProvider.BusinessObjectPK
		{
			get
			{
				var controllerIDProvider = ControllerIDProvider;
				return controllerIDProvider != null ? controllerIDProvider.BusinessObjectPK : Guid.Empty;
			}
		}

		#endregion

		public ZString StatementNumber
		{
			get { return StatementHeader.B2_StatementNumber; }
		}
	}
}
