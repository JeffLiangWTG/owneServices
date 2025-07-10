using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public abstract class SharedCusPermitHeader : CommonCusPermitHeader, IDocManagerSupport, IControllerIDProvider, IHaveRequiredDocuments
	{
		protected SharedCusPermitHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new static readonly CusPermitHeaderTypeDecider TypeDecider = new CusPermitHeaderTypeDecider();

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new FetchStrategies.SharedCusPermitHeaderFetchStrategy(this);
		}

		#region Override Properties

		[List(nameof(Lookups) + "." + nameof(SharedCusPermitHeaderLookups.PermitTypes))]
		public override ZString CPH_Type
		{
			get { return base.CPH_Type; }
			set
			{
				var originalValue = CPH_Type;
				base.CPH_Type = value;
				if (!IsCopying && originalValue != CPH_Type)
				{
					CPH_SubType = ZString.Empty;
					if (ShouldClearUnitOfMeasure && !IsQTY)
					{
						CPH_UnitOfMeasure = ZString.Empty;
					}

					if (Lookups.PermitQtyValIndicators.Count == 0)
					{
						CPH_QtyValIndicator = ZString.Empty;
					}
				}
			}
		}

		protected virtual bool ShouldClearUnitOfMeasure => true;

		[List(nameof(Lookups) + "." + nameof(SharedCusPermitHeaderLookups.PermitSubTypes))]
		public override ZString CPH_SubType
		{
			get { return base.CPH_SubType; }
			set { base.CPH_SubType = value; }
		}

		[List(nameof(Lookups) + "." + nameof(SharedCusPermitHeaderLookups.PermitQtyValIndicators))]
		public override ZString CPH_QtyValIndicator
		{
			get { return base.CPH_QtyValIndicator; }
			set { base.CPH_QtyValIndicator = value; }
		}

		#endregion

		#region New Properties

		public abstract ISharedCountrySpecificInstruction GetCountrySpecificInstruction();

		#region TransactionCategory
		[List(nameof(Lookups) + "." + nameof(SharedCusPermitHeaderLookups.PermitTransactionCategories))]
		public virtual ZString TransactionCategory
		{
			get { return transactionCategory; }
			set
			{
				SetNonPersistentPropertyValue(TransactionCategoryInfo, ref transactionCategory, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateTransactionCategory();
				}
			}
		}
		ZString transactionCategory = PermitTransactionCategoryList.Codes.CUM;

		public ZPropertyInfo TransactionCategoryInfo => GetZPropertyInfo(nameof(TransactionCategory));

		public bool IsCUM => TransactionCategory == PermitTransactionCategoryList.Codes.CUM;
		#endregion

		public virtual ZDecimal CPH_Calc_OpeningBalance
			=> GetOpeningTransactions().Where(x => x.CPL_TransactionType == PermitTransactionTypeList.Codes.OBL)
				.OrderByDescending(x => x.CPL_TransactionDate)
				.Select(x => x.CPL_TranValue).FirstOrDefault();

		protected virtual IEnumerable<SharedCusPermitLineTransaction> GetOpeningTransactions() => GetTransactions();

		public bool IsQTY
		{
			get
			{
				var indicator = CPH_QtyValIndicator;
				return indicator == PermitQtyValIndicatorList.Codes.BTH || indicator == PermitQtyValIndicatorList.Codes.QTY;
			}
		}

		public bool IsVAL
		{
			get
			{
				var indicator = CPH_QtyValIndicator;
				return indicator == PermitQtyValIndicatorList.Codes.BTH || indicator == PermitQtyValIndicatorList.Codes.VAL;
			}
		}

		public virtual bool IsTransactionsApplicable() => !CPH_QtyValIndicator.IsEmpty;

		#region ValueBalance
		public ZDecimal ValueBalance => Factory.GetValue(ref valueBalance, GetValueBalance);
		CachedProperty<ZDecimal> valueBalance;

		protected ZDecimal GetValueBalance()
			=> IsVAL ? new ZDecimal(GetTransactions()
					.Where(x => x.CPL_TransactionStatus != PermitTransactionStatusList.Codes.Deleted)
					.Sum(x => x.CPL_TranValue))
				: ZDecimal.Zero;

		public ZPropertyInfo ValueBalanceInfo => GetZPropertyInfo(nameof(ValueBalance));
		#endregion

		#region QuantityBalance
		public ZDecimal QuantityBalance => Factory.GetValue(ref quantityBalance, GetQuantityBalance);
		CachedProperty<ZDecimal> quantityBalance;

		protected ZDecimal GetQuantityBalance()
			=> IsQTY ? new ZDecimal(GetTransactions()
					.Where(x => x.CPL_TransactionStatus != PermitTransactionStatusList.Codes.Deleted)
					.Sum(x => x.CPL_TranQty))
				: ZDecimal.Zero;

		public ZPropertyInfo QuantityBalanceInfo => GetZPropertyInfo(nameof(QuantityBalance));
		#endregion

		SharedCusPermitLineTransaction LatestVALCategoryTransaction
		{
			get
			{
				return GetTransactions()
						.Where(x => x.CPL_TransactionCategory == PermitTransactionCategoryList.Codes.VAL)
						.OrderByDescending(x => x.CPL_TransactionDate).FirstOrDefault();
			}
		}

		#region AuthLatestValueBalance
		public ZDecimal AuthLatestValueBalance => Factory.GetValue(ref authLatestValueBalance, GetAuthLatestValueBalance);
		CachedProperty<ZDecimal> authLatestValueBalance;

		public ZPropertyInfo AuthLatestValueBalanceInfo => GetZPropertyInfo(nameof(AuthLatestValueBalance));

		protected ZDecimal GetAuthLatestValueBalance()
			=> IsVAL ? LatestVALCategoryTransaction?.CPL_TranValue ?? ZDecimal.Zero : ZDecimal.Zero;

		public ZString AuthLatestValueBalanceWithMsg
		{
			get
			{
				var valueBalance = AuthLatestValueBalance;
				if (valueBalance.IsEmpty)
				{
					return Res.GetString("1F854676-B132-4482-9777-49EC7EBAEB7D", "Not Available");
				}
				return valueBalance.ToString();
			}
		}

		public ZPropertyInfo AuthLatestValueBalanceWithMsgInfo => GetZPropertyInfo(nameof(AuthLatestValueBalanceWithMsg));
		#endregion

		#region AuthLatestQuantityBalance
		public ZDecimal AuthLatestQuantityBalance => Factory.GetValue(ref authLatestQuantityBalance, GetAuthLatestQuantityBalance);
		CachedProperty<ZDecimal> authLatestQuantityBalance;

		protected ZDecimal GetAuthLatestQuantityBalance()
			=> IsQTY ? LatestVALCategoryTransaction?.CPL_TranQty ?? ZDecimal.Zero : ZDecimal.Zero;

		public ZPropertyInfo AuthLatestQuantityBalanceInfo => GetZPropertyInfo(nameof(AuthLatestQuantityBalance));

		public ZString AuthLatestQuantityBalanceWithMsg
		{
			get
			{
				var qtyBalance = AuthLatestQuantityBalance;
				if (qtyBalance.IsEmpty)
				{
					return Res.GetString("CD264823-D00F-4294-B1DF-3F7C95ECEEE1", "Not Available");
				}
				return qtyBalance.ToString();
			}
		}

		public ZPropertyInfo AuthLatestQuantityBalanceWithMsgInfo => GetZPropertyInfo(nameof(AuthLatestQuantityBalance));
		#endregion

		protected internal virtual ZBool AllowNewLineTransactions => false;

		protected virtual bool HasSystemGeneratedTransactions => GetTransactions().Any(x => systemGeneratedTransactionTypes.Contains(x.CPL_TransactionType));

		readonly ImmutableArray<string> systemGeneratedTransactionTypes = new string[] { PermitTransactionTypeList.Codes.CUS, PermitTransactionTypeList.Codes.TRA }.ToImmutableArray();

		#endregion

		#region ReadOnly

		protected bool CPH_OH_PermitHolder_ReadOnly => HasSystemGeneratedTransactions;

		protected bool CPH_Number_ReadOnly => HasSystemGeneratedTransactions;

		public virtual bool CPH_StartDate_ReadOnly => HasSystemGeneratedTransactions;

		protected virtual bool CPH_Type_ReadOnly => HasSystemGeneratedTransactions;

		public virtual bool CPH_SubType_ReadOnly => (HasSystemGeneratedTransactions || (GetCountrySpecificInstruction()?.GetSubTypeList(CPH_Type).Count ?? ZInt.Zero) == ZInt.Zero) && !GlbStaff.CurrentUser.GS_IsController; // get out of jail free

		public bool CPH_QtyValIndicator_ReadOnly => HasSystemGeneratedTransactions || (GetTransactions()?.Any(x => x.IsInDatabase) ?? false);

		public virtual bool CPH_UnitOfMeasure_ReadOnly => HasSystemGeneratedTransactions;

		protected bool CPH_OA_AppliesTo_ReadOnly => HasSystemGeneratedTransactions;

		#endregion

		protected abstract IEnumerable<SharedCusPermitRule> GetRules();
		public abstract IActiveBusinessObjectCollection GetRulesCollection();
		public abstract Type GetRuleType();

		public abstract IEnumerable<SharedCusPermitLineTransaction> GetTransactions();
		protected abstract IActiveBusinessObjectCollection GetTransactionsCollection();
		public abstract Type GetTransactionType();

		public abstract void ReloadLatestTransactions();

		public void DoActionWithMutexLock(Action doAction, Action<string> notifier, Func<string> getMutexText = null)
		{
			if (LockMutex)
			{
				try
				{
					ReloadLatestTransactions();
					doAction();
				}
				finally
				{
					UnlockMutex();
				}
			}
			else
			{
				var mutexText = getMutexText == null ? MutexText : getMutexText();
				notifier(mutexText);
			}
		}

		public SharedCusPermitLineTransaction AddTransaction(ZString reference, ZString comment, ZString appId, ZString procedure, ZDecimal value, ZDecimal quantity, string status = "", int referenceNumberLine = 0, string transactionType = PermitTransactionTypeList.Codes.TRA, string category = PermitTransactionCategoryList.Codes.CUM, ZDateTime? transactionDate = null, bool? isAggregated = null, Action<ZString, ZDecimal> notifier = null, bool checkBursting = true)
		{
			SharedCusPermitLineTransaction transaction = null;

			if ((!value.IsEmpty || !quantity.IsEmpty) && (transactionType == PermitTransactionTypeList.Codes.OBL || !checkBursting || IsNotBusting(value, quantity, transactionType, notifier)))
			{
				transaction = (SharedCusPermitLineTransaction)GetTransactionsCollection().AddNew();
				transaction.CPL_Reference = reference;
				transaction.CPL_Comment = comment;
				transaction.CPL_TransactionCategory = category;
				transaction.CPL_TransactionType = transactionType;
				transaction.CPL_TranQty = quantity;
				transaction.CPL_TranValue = value;
				transaction.CPL_AppId = appId;
				transaction.CPL_TransactionStatus = string.IsNullOrEmpty(status) && transactionType == PermitTransactionTypeList.Codes.ADJ ? PermitTransactionStatusList.Codes.Confirmed : status;
				transaction.CPL_ReferenceNumberLine = referenceNumberLine;
				transaction.CPL_Procedure = procedure;

				if (transactionDate.HasValue)
				{
					transaction.CPL_TransactionDate = transactionDate.Value;
				}

				if (isAggregated.HasValue)
				{
					transaction.CPL_IsAggregated = isAggregated.Value;
				}
			}
			return transaction;
		}

		public string MutexText => Res.GetString("a54998ee-dd8c-4039-8bc0-4b337191cadf"
			, "{0} is already in the process of adding transaction for this {1}({1} Holder: {2}, Country Code: {3}, Number: {4}, Start Date: {5}, Type: {6}, Sub Type: {7}). You cannot process with it until data is saved. Please try later."
			, GetMutexLockInfo(), ShortName, PermitHolder.OH_Code, CPH_RN_NKCountryCode, CPH_Number, CPH_StartDate.ToShortDateString(), CPH_Type, CPH_SubType);

		protected abstract bool IsNotBusting(ZDecimal value, ZDecimal quantity, ZString transactionType, Action<ZString, ZDecimal> notifier);

		protected bool IsBursting(ZDecimal balance, ZDecimal value) => (balance >= 0 && value + balance < ZDecimal.Zero) || (balance < 0 && value < 0);

		protected sealed override BusinessObject[] BusinessObjectsWithRelatedEventsCore
			=> GetBusinessObjectsWithRelatedLogsCore().ToArray();

		protected virtual List<BusinessObject> GetBusinessObjectsWithRelatedLogsCore()
		{
			var result = new List<BusinessObject>();

			foreach (var rule in GetRules())
			{
				result.Add(rule);
				result.AddRange(rule.CusPermitRuleExceptions);
			}

			result.AddRange(GetTransactions());

			return result;
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CPH_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			var types = GetCountrySpecificInstruction()?.GetTypeList()?.OfType<CodeDescriptionPair>().Take(2).ToArray();
			if (types != null && types.Length == 1)
			{
				CPH_Type = types.First().Code;
			}
		}

		protected override void OnFactorySaving()
		{
			var existing = LoadCusPermitHeaderByKey(Factory, CPH_OH_PermitHolder, CPH_RN_NKCountryCode, CPH_Number, CPH_StartDate, CPH_Type, CPH_SubType, this.PK);
			if (existing != null)
			{
				var message = ZString.Format(
					$@"A {ShortName} with this combination already exists:

{ShortName} Holder:	{0}
Country Code:	{1}
Number:		{2}
Start Date:	{3}
Type:		{4}
Sub Type:	{5}

Please edit the existing {ShortName}.", PermitHolder.OH_Code, CPH_RN_NKCountryCode, CPH_Number, CPH_StartDate.ToShortDateString(), CPH_Type, CPH_SubType);
				throw new ZCannotSaveException(message, $"Duplicate {ShortName}");
			}
		}

		public static SharedCusPermitHeader LoadCusPermitHeaderByKey(BusinessObjectFactory factory, ZGuid permitHolder, string countryCode, string permitNumber, ZDate startDate, string type, string subType = "", ZGuid? excludePK = null)
		{
			SharedCusPermitHeader result = null;
			if (startDate.IsValid)
			{
				var query = new ZQuery(CusPermitHeaderSchema.CPH_OH_PermitHolder, permitHolder);
				query.AddToFilter(CusPermitHeaderSchema.CPH_RN_NKCountryCode, countryCode);
				query.AddToFilter(CusPermitHeaderSchema.CPH_Number, permitNumber);
				query.AddToFilter(CusPermitHeaderSchema.CPH_StartDate, startDate);
				query.AddToFilter(CusPermitHeaderSchema.CPH_Type, type);
				query.AddToFilter(CusPermitHeaderSchema.CPH_SubType, subType);
				if (excludePK != null)
				{
					query.AddToFilter(CusPermitHeaderSchema.PK, SQLComparisonOperator.NotEqual, excludePK);
				}
				result = factory.LoadTop1<SharedCusPermitHeader>(query);
			}
			return result;
		}

		public override void Delete()
		{
			if (!IsDeleted && CanDelete)
			{
				GetRulesCollection()?.DeleteAll();
				GetTransactionsCollection()?.DeleteAll();
			}

			base.Delete();
		}

		public override bool CanDelete => base.CanDelete && !HasSystemGeneratedTransactions && (!IsInDatabase || !GetTransactions().Any());

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get
			{
				var result = base.ReasonForNotAbleToDelete;
				if (HasSystemGeneratedTransactions || (IsInDatabase && GetTransactions().Any()))
				{
					result = ResString.GetMultilingualString("0392EBCE-3E34-4FDC-BD24-08F2F4D61C06",
						"{0} cannot be deleted because it has transaction allocated to it.", HumanReadableName);
				}

				return result;
			}
		}

		protected override IBusinessObjectStrategy[] GetStrategies()
		{
			return base.GetStrategies().Union(new[] { new SharedCusPermitHeaderBizoStrategy() }).ToArray();
		}

		class SharedCusPermitHeaderBizoStrategy : IBusinessObjectStrategy
		{
			public void BeforeSuccessfulDelete(BusinessObject businessObject) { }

			public DeleteDetails DeleteDetails(BusinessObject businessObject)
			{
				var permitHeader = businessObject as SharedCusPermitHeader;
				if (permitHeader == null)
				{
					return null;
				}

				if (permitHeader.HasSystemGeneratedTransactions || (permitHeader.IsInDatabase && permitHeader.GetTransactions().Any()))
				{
					return new DeleteDetails.Disallow(ResString.GetMultilingualString("BCA6A736-BEC4-43DE-AE82-55FDEC40C87C", "This {0} as active transactions recorded against it.", permitHeader.ShortName));
				}

				return new DeleteDetails.Allow();
			}

			public void OnDelete(BusinessObject businessObject) { }

			public void OnSaving(BusinessObject businessObject) { }

			public void OnSaved(BusinessObject businessObject, bool saveSucceeded) { }

			public void OnFactorySaving(BusinessObject businessObject) { }

			public void OnFactorySaved(BusinessObject businessObject, bool saveSucceeded) { }

			public void OnSaveRollback(BusinessObject businessObject) { }

			public void FetchForLoad(BusinessObject businessObject) { }

			public void OnSavingInObjectsWithLateChanges(BusinessObject businessObject) { }
		}

		#region IDocManagerSupport Members

		public virtual DocManagerInfo DocManagerInfo
		{
			get
			{
				if (docManagerInfo == null)
				{
					docManagerInfo = new DocManagerInfo(this, DocManagerCode);
				}
				return docManagerInfo;
			}
		}

		DocManagerInfo docManagerInfo;

		protected abstract string DocManagerCode { get; }

		#endregion

		#region IControllerIDProvider Members

		ControllerID IControllerIDProvider.ControllerID => ControllerID;

		Guid IControllerIDProvider.BusinessObjectPK => PK.ToGuid();

		protected abstract ControllerID ControllerID { get; }

		#endregion

		#region IHaveRequiredDocuments Members

		ZString IHaveRequiredDocuments.UniqueConsignRef => CPH_Number;

		ZString IHaveRequiredDocuments.HouseBill => ZString.Empty;

		ZString IHaveRequiredDocuments.MasterBill => ZString.Empty;

		OrgHeader IHaveRequiredDocuments.ExportBroker => null;

		ZString IHaveRequiredDocuments.TableCode => CusPermitHeaderSchema.Constants.Prefix;

		ZGuid IHaveRequiredDocuments.PK => this.PK;

		void IHaveRequiredDocuments.PreLogAllDocumentsReceivedEvents()
		{
		}

		[ChildEditable(true)]
		public JobRequiredDocumentDependentCollection RequiredDocuments
		{
			get
			{
				if (fRequiredDocuments == null)
				{
					fRequiredDocuments = new JobRequiredDocumentDependentCollection(this, Factory);
					fRequiredDocuments.Load();
					RegisterEditableChildObject(fRequiredDocuments);
				}
				return fRequiredDocuments;
			}
		}
		JobRequiredDocumentDependentCollection fRequiredDocuments;

		BusinessObject IHaveRequiredDocuments.UltimateDocumentParent => this;

		Logs IHaveRequiredDocuments.Logs => this.Logs;

		IReadOnlyList<ZString> IHaveRequiredDocuments.AdditionalRefTypes => Array.Empty<ZString>();

		#endregion

		#region Validation and Lookups

		public new SharedCusPermitHeaderLookups Lookups => (SharedCusPermitHeaderLookups)base.Lookups;

		public new SharedCusPermitHeaderValidation Validation => (SharedCusPermitHeaderValidation)base.Validation;

		#endregion
	}
}
