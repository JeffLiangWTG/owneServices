using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business.Extensions;
using Enterprise.Customs.Business.FetchStrategies;
using Enterprise.Customs.Common.Shared;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.Business
{
	public class BaseCusGuaranteeHeader : SharedCusPermitHeader, IDocManagerSupport, IControllerIDProvider, IHaveRequiredDocuments, IBaseCusGuaranteeHeader, ICusCodeDataTypeSupporter, IEDIMessageCollectionProvider
	{
		public BaseCusGuaranteeHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new static readonly BaseCusGuaranteeHeaderTypeDecider TypeDecider = new BaseCusGuaranteeHeaderTypeDecider();

		#region Loader

		public new class Loader : BusinessObject.Loader
		{
			public Loader(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public ZQuery GetLoadQuery(ZString guaranteeNumber, ZQuery additionalFilter = null)
			{
				var query = new ZQuery(CusPermitHeaderSchema.CPH_ApplicationCode, CusPermitHeaderApplicationCodeList.Codes.Guarantee);
				query.AddToFilter(CusPermitHeaderSchema.CPH_Number, guaranteeNumber);
				query.AddToFilter(CusPermitHeaderSchema.CPH_IsActive, true);
				if (additionalFilter != null)
				{
					query.AddToFilter(additionalFilter);
				}
				return query;
			}

			public T[] Load<T>(ZString guaranteeNumber, ZQuery additionalFilter = null)
				where T : BaseCusGuaranteeHeader
			{
				return guaranteeNumber.IsEmpty ? Array.Empty<T>() : Factory.Load<T>(GetLoadQuery(guaranteeNumber, additionalFilter));
			}

			protected override Type GetTypeOfBusinessObjectToLoad()
			{
				return typeof(BaseCusGuaranteeHeader);
			}
		}

		#endregion

		#region Override Properties

		[List(nameof(Lookups) + "." + nameof(CusGuaranteeHeaderLookups.Currencies))]
		[MaxLength(3)]
		[ResourceStringData("86808365-1b09-4dbc-aec5-3d04722260e0", Caption = "Currency")]
		public override ZString CPH_UnitOfMeasure { get => base.CPH_UnitOfMeasure; set => base.CPH_UnitOfMeasure = value; }

		[ReadOnlyMember(nameof(CPH_RN_NKCountryCodeReadOnly))]
		[List(nameof(Lookups) + "." + nameof(CusGuaranteeHeaderLookups.Countries))]
		[ResourceStringData("7EBD1AA6-B194-4D30-85E7-EAC7E5EB62E6", Caption = "Creation Country")]
		public override ZString CPH_RN_NKCountryCode { get => base.CPH_RN_NKCountryCode; set => base.CPH_RN_NKCountryCode = value; }

		protected bool CPH_RN_NKCountryCodeReadOnly => true;

		public override bool CPH_UnitOfMeasure_ReadOnly => CPH_Type == GeneralGuarantee || CusGuaranteeLineTransactions.Any(x => x.IsInDatabase);

		const string GeneralGuarantee = "GEN";

		public override ZString ShortName => Res.GetString("B08A9AEE-094E-4C1A-B3BF-12D76956EA64", "Guarantee");

		[ResourceStringData("b9fa5893-c87e-4d41-8fac-53258d9e1b82", Caption = "Guarantee Type")]
		public override ZString CPH_Type
		{
			get => base.CPH_Type;
			set => base.CPH_Type = value;
		}

		protected override bool ShouldClearUnitOfMeasure => false;

		[ResourceStringData("0918ca2e-d412-4483-837f-32cf03a6833b", Caption = "Sub Type")]
		public override ZString CPH_SubType
		{
			get => base.CPH_SubType;
			set => base.CPH_SubType = value;
		}

		[ResourceStringData("b17213a9-67d9-44ce-ac86-d34274c6a9e9", Caption = "Guarantee Holder")]
		public override ZGuid CPH_OH_PermitHolder
		{
			get => base.CPH_OH_PermitHolder;
			set => base.CPH_OH_PermitHolder = value;
		}

		[ResourceStringData("ecf27ac8-0a44-4b7d-b5ae-97db02418c2d", Caption = "Is Single Transaction")]
		public override ZBool CPH_IsSingleUse
		{
			get => base.CPH_IsSingleUse;
			set => base.CPH_IsSingleUse = value;
		}

		[ResourceStringData("6938122a-cbc6-4a0b-bc1f-b236441d0028", Caption = "Guarantee Number")]
		public override ZString CPH_Number
		{
			get => base.CPH_Number;
			set => base.CPH_Number = value;
		}

		[ResourceStringData("bbfaf2a4-4d67-4608-8c89-1b2a665fb359", Caption = "Quantity")]
		public override ZString CPH_QtyValIndicator
		{
			get => base.CPH_QtyValIndicator;
			set => base.CPH_QtyValIndicator = value;
		}

		[ResourceStringData("5b78eb50-1cef-4962-801b-badc813fb6fd", Caption = "Start Date")]
		public override ZDate CPH_StartDate
		{
			get => base.CPH_StartDate;
			set => base.CPH_StartDate = value;
		}

		[ResourceStringData("02017f85-d0fc-4fc2-ae09-11b168dce9ee", Caption = "End Date")]
		public override ZDate CPH_EndDate
		{
			get => base.CPH_EndDate;
			set => base.CPH_EndDate = value;
		}

		public override ZDecimal CPH_Calc_OpeningBalance => base.CPH_Calc_OpeningBalance + GetOpeningTransactions().Where(x => x.CPL_TransactionType == GuaranteeTransactionTypeList.Codes.OBA).Sum(x => x.CPL_TranValue);

		public override ZDecimal CPH_Balance
		{
			get => base.CPH_Balance;
			set
			{
				if (IsInDatabase && base.CPH_Balance != 0 && base.CPH_Balance != value)
				{
					throw new InvalidOperationException($"Cannot change \"{nameof(CPH_Balance)}\" to non-zero as it has already saved in database!");
				}
				base.CPH_Balance = value;
			}
		}

		public ZPropertyInfo CPH_Calc_OpeningBalanceInfo => GetZPropertyInfo(nameof(CPH_Calc_OpeningBalance));

		#endregion

		#region New Properties

		public ZString QtyValIndicatorDescription => Lookups.PermitQtyValIndicators.GetDescriptionFromCode(CPH_QtyValIndicator);

		public GuaranteeCountrySpecificInstruction CountrySpecificInstruction
		{
			get
			{
				if (countrySpecificInstruction == null)
				{
					countrySpecificInstruction = GuaranteeCountrySpecificInstruction.GetByCountryCode(Factory, CPH_RN_NKCountryCode);
				}
				return countrySpecificInstruction;
			}
		}
		GuaranteeCountrySpecificInstruction countrySpecificInstruction;

		public override ISharedCountrySpecificInstruction GetCountrySpecificInstruction()
			=> CountrySpecificInstruction;

		public bool HasOpeningBalanceTransaction => OpeningCusGuaranteeLineTransactions.Any(x => x.CPL_TransactionType == PermitTransactionTypeList.Codes.OBL);

		public EDIMessageCollection Messages => messagesCached ?? (messagesCached = LoadMessages());

		EDIMessageCollection LoadMessages()
		{
			var query = new ZQuery(EDIMessageSchema.EM_LinkUniqueID, PK);
			query.AddToFilter(EDIMessageSchema.EM_LinkTable, CusPermitHeaderSchema.Constants.TableName);
			query.FetchOnlyFromLocalCache = !IsInDatabase;
			var collection = new EDIMessageCollection(this, query);
			collection.Load();
			return collection;
		}

		EDIMessageCollection messagesCached;

		#region MainAccessCode

		public CusGuaranteeRule MainAccessCodeRule
		{
			get
			{
				if (mainAccessCodeRule == null || mainAccessCodeRule.IsDeleted)
				{
					mainAccessCodeRule = CusGuaranteeRule.Loader.LoadMainAccessCode(this);
					if (mainAccessCodeRule != null)
					{
						RegisterEditableChildObject(mainAccessCodeRule);
					}
				}
				return mainAccessCodeRule;
			}
		}
		CusGuaranteeRule mainAccessCodeRule;

		[MaxLength(CusGuaranteeRule.Schema.CPR_ValueFromMaxLength)]
		public virtual ZString MainAccessCode
		{
			get => MainAccessCodeRule?.CPR_ValueFrom ?? ZString.Empty;
			set
			{
				var oldValue = MainAccessCode;
				if (oldValue != value)
				{
					CheckMaximumLength(MainAccessCodeInfo, value);
					var accessCodeRule = MainAccessCodeRule;
					if (!value.IsEmpty)
					{
						if (accessCodeRule == null)
						{
							accessCodeRule = CreateMainAccessCode();
						}
						AdditionalAccessCodes.SetReadOnlyIncludingChildren(false);
					}
					else
					{
						AdditionalAccessCodes.DeleteAll();
						AdditionalAccessCodes.SetReadOnlyIncludingChildren(true);
					}

					if (accessCodeRule != null)
					{
						accessCodeRule.CPR_ValueFrom = value;
					}

					if (!IsValidationSuspended)
					{
						Validation.ValidateMainAccessCode();
					}
					MainAccessCodeInfo.RefreshBinding(oldValue);
				}
			}
		}

		CusGuaranteeRule CreateMainAccessCode()
		{
			mainAccessCodeRule = CusGuaranteeRule.Loader.CreateMainAccessCode(this);
			RegisterEditableChildObject(mainAccessCodeRule);
			return mainAccessCodeRule;
		}

		[MaxLength(CusGuaranteeRule.Schema.CPR_DescriptionMaxLength)]
		public virtual ZString MainAccessPersonName
		{
			get => MainAccessCodeRule?.CPR_Description ?? ZString.Empty;
			set
			{
				var oldValue = MainAccessPersonName;
				if (oldValue != value)
				{
					CheckMaximumLength(MainAccessPersonNameInfo, value);
					var accessCodeRule = MainAccessCodeRule;
					if (!value.IsEmpty && accessCodeRule == null)
					{
						accessCodeRule = CreateMainAccessCode();
					}

					if (accessCodeRule != null)
					{
						accessCodeRule.CPR_Description = value;
					}

					if (!IsValidationSuspended)
					{
						Validation.ValidateMainAccessPersonName();
					}
					MainAccessPersonNameInfo.RefreshBinding(oldValue);
				}
			}
		}

		public ZPropertyInfo MainAccessCodeInfo => GetZPropertyInfo(nameof(MainAccessCode));

		public ZPropertyInfo MainAccessPersonNameInfo => GetZPropertyInfo(nameof(MainAccessPersonName));

		#endregion MainAccessCode

		#endregion New Properties

		#region Business Object Overrides

		protected override ZString HumanReadableNameCore
		{
			get { return Res.GetString("181631EB-2CA2-4CFD-804B-EE4B8887F03B", "Guarantee {0} - {1}", PermitHolder?.OH_Code ?? ZString.Empty, CPH_Number).Trim(); }
		}

		#endregion

		#region Implementation

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CPH_QtyValIndicator = PermitQtyValIndicatorList.Codes.VAL;
			CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Guarantee;
			CPH_UnitOfMeasure = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
		}

		#endregion base overrides

		#region Reference Numbers

		[ChildEditable(true)]
		public CusGuaranteeReferenceNumberCollection AdditionalGuaranteeReferences => cusTempStorageContainers ?? (cusTempStorageContainers = GetAdditionalGuaranteeReferences());
		CusGuaranteeReferenceNumberCollection cusTempStorageContainers;

		CusGuaranteeReferenceNumberCollection GetAdditionalGuaranteeReferences()
		{
			var result = GetNewAdditionalGuaranteeReferences();
			result.Load();
			RegisterEditableChildObject(result);
			return result;
		}

		protected virtual CusGuaranteeReferenceNumberCollection GetNewAdditionalGuaranteeReferences() => new CusGuaranteeReferenceNumberCollection<CusGuaranteeReferenceNumber>(this);

		public bool SupportsAdditionalCustomsReferences => SupportsAdditionalCustomsReferencesCore;

		protected virtual bool SupportsAdditionalCustomsReferencesCore => CountrySpecificInstruction.SupportsAdditionalCustomsReferences(this.CPH_Type);

		public bool SupportsMessages => SupportsMessagesCore;

		protected virtual bool SupportsMessagesCore => false;

		public ZString GetApplicationSpecificReference(string appType)
		{
			var applicationSpecificReferenceWithoutFallbackToPermitNumber = GetApplicationSpecificReferenceWithoutFallbackToPermitNumber(appType);
			return applicationSpecificReferenceWithoutFallbackToPermitNumber.IsEmpty ? CPH_Number : applicationSpecificReferenceWithoutFallbackToPermitNumber;
		}

		public ZString GetApplicationSpecificReferenceWithoutFallbackToPermitNumber(string appType) => SupportsAdditionalCustomsReferences ? AdditionalGuaranteeReferences.Cast<CusGuaranteeReferenceNumber>().FirstOrDefault(x => x.CY_Code == appType)?.CY_Data ?? ZString.Empty : ZString.Empty;

		#endregion

		#region Rules

		[ChildEditable(true)]
		public CusGuaranteeRuleCollection CusGuaranteeRules
		{
			get
			{
				if (cusGuaranteeRules == null)
				{
					cusGuaranteeRules = CreateNewCusGuaranteeRulesCore();
					RegisterEditableChildObject(cusGuaranteeRules);
				}
				return cusGuaranteeRules;
			}
		}
		CusGuaranteeRuleCollection cusGuaranteeRules;

		protected virtual CusGuaranteeRuleCollection CreateNewCusGuaranteeRulesCore() => new CusGuaranteeRuleCollection<CusGuaranteeRule>(this);

		protected override IEnumerable<SharedCusPermitRule> GetRules() => CusGuaranteeRules;

		public override IActiveBusinessObjectCollection GetRulesCollection() => CusGuaranteeRules;

		public override Type GetRuleType() => typeof(CusGuaranteeRule);

		[ChildEditable(true)]
		public CusGuaranteeRuleCollection AdditionalAccessCodes
		{
			get
			{
				if (additionalAccessCodes == null)
				{
					additionalAccessCodes = CreateNewAdditionalAccessCodesCore();
					RegisterEditableChildObject(additionalAccessCodes);
					additionalAccessCodes.SetReadOnlyIncludingChildren(MainAccessCode.IsEmpty);
				}
				return additionalAccessCodes;
			}
		}
		CusGuaranteeRuleCollection additionalAccessCodes;

		protected virtual CusGuaranteeRuleCollection CreateNewAdditionalAccessCodesCore() => new CusGuaranteeRuleCollection<CusGuaranteeRule>(this, PermitRuleCodeListForAccessCodes.Codes.AccessCodePinPersonalIdentificationNumber);

		#endregion Rules

		#region Transactions

		[ChildEditable(true)]
		public CusGuaranteeLineTransactionCollection OpeningCusGuaranteeLineTransactions
		{
			get
			{
				if (openingCusGuaranteeLineTransactions == null)
				{
					var openingBalanceOnlyAdditionalFilter = new ZQuery(CusPermitLineTransactionSchema.CPL_TransactionType, new[] { GuaranteeTransactionTypeList.Codes.OBA, PermitTransactionTypeList.Codes.OBL });
					openingCusGuaranteeLineTransactions = new CusGuaranteeLineTransactionCollection(this, openingBalanceOnlyAdditionalFilter);
					RegisterEditableChildObject(openingCusGuaranteeLineTransactions);
				}
				return openingCusGuaranteeLineTransactions;
			}
		}
		CusGuaranteeLineTransactionCollection openingCusGuaranteeLineTransactions;

		[ChildEditable(true)]
		public CusGuaranteeLineTransactionCollection CusGuaranteeLineTransactions
		{
			get
			{
				if (cusGuaranteeLineTransactions == null)
				{
					cusGuaranteeLineTransactions = CreateNewCusGuaranteeLineTransactionCollection();
					cusGuaranteeLineTransactions.CollectionCountChange += CusGuaranteeLineTransactions_CollectionCountChange;
					RegisterEditableChildObject(cusGuaranteeLineTransactions);
				}
				return cusGuaranteeLineTransactions;
			}
		}

		protected virtual CusGuaranteeLineTransactionCollection CreateNewCusGuaranteeLineTransactionCollection()
		{
			return new CusGuaranteeLineTransactionCollection(this);
		}

		void CusGuaranteeLineTransactions_CollectionCountChange(object sender, CollectionCountChangedEventArgs e)
		{
			CPH_Calc_OpeningBalanceInfo.RefreshBinding();
		}

		CusGuaranteeLineTransactionCollection cusGuaranteeLineTransactions;

		public override IEnumerable<SharedCusPermitLineTransaction> GetTransactions() => CusGuaranteeLineTransactions;

		protected override IEnumerable<SharedCusPermitLineTransaction> GetOpeningTransactions() => OpeningCusGuaranteeLineTransactions;

		public override void ReloadLatestTransactions()
		{
			if (((IBusinessObjectCollectionInternals)CusGuaranteeLineTransactions).HasChangesFromDatabase())
			{
				CusGuaranteeLineTransactions.RefreshFromDb();
			}
		}

		protected override bool IsNotBusting(ZDecimal value, ZDecimal quantity, ZString transactionType, Action<ZString, ZDecimal> notifier)
		{
			var result = true;
			if (transactionType != GuaranteeTransactionTypeList.Codes.OBA || value <= 0)
			{
				var balance = CPH_Calc_TotalBalanceIncludingPendingDecimal;
				var total = CPH_Calc_OpeningBalance;
				if (IsBursting(balance, value))
				{
					notifier?.Invoke(Res.GetString("9d8a4093-c25f-4939-8dea-7e30ea76cae6", "The guarantee {0} available amount will be exceeded by {1:0.##}. The total is {2:0.##} and the available is {3:0.##}.", CPH_Number, (value + balance) * -1, total, balance), balance);
					result = false;
				}
				else if (total > 0 && value + balance > total)
				{
					notifier?.Invoke(Res.GetString("959f26e7-b30a-47a9-8469-dfc3300fb721", "The guarantee {0} total amount will be exceeded by {1:0.##}. The total is {2:0.##} and the available is {3:0.##}.", CPH_Number, (value + balance - total), total, balance), balance);
					result = false;
				}
			}
			return result;
		}

		protected override IActiveBusinessObjectCollection GetTransactionsCollection() => CusGuaranteeLineTransactions;

		public override Type GetTransactionType() => typeof(BaseCusGuaranteeLineTransaction);

		#endregion Transactions

		public override void Delete()
		{
			if (!IsDeleted)
			{
				FetchForLoadChildEditableObjectsIfNeeded();
				this.DeleteAllCusAddInfoCodeDataAndSupportingInfoChildrenIfSupported();
				AdditionalAccessCodes.DeleteAll();
				MainAccessCodeRule?.Delete();
			}
			base.Delete();
		}

#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			if (guaranteeHolder == null)
			{
				guaranteeHolder = Factory.LoadTop1<OrgHeader>(new ZQuery());
			}
			CPH_OH_PermitHolder = guaranteeHolder.PK;
			CPH_Number = "12345";
			CPH_StartDate = ZDate.BrettsBirthday;
			CPH_Type = PermitTransactionTypeList.Codes.OBL;
			CPH_QtyValIndicator = "VAL";
		}
		OrgHeader guaranteeHolder;

#endif

		protected override string DocManagerCode => Core.Constants.DocManagerCodes.CustomsGuarantee;

		protected override ControllerID ControllerID => ControllerIDs.Customs.Guarantees;

		#region Validation and Lookups

		public new CusGuaranteeHeaderLookups Lookups => (CusGuaranteeHeaderLookups)base.Lookups;

		protected override CusPermitHeaderLookups GetNewLookups() => new CusGuaranteeHeaderLookups(this);

		public new CusGuaranteeHeaderValidation Validation => (CusGuaranteeHeaderValidation)base.Validation;

		protected override CusPermitHeaderValidation GetNewValidation() => new CusGuaranteeHeaderValidation(this);

		#endregion

		#region TransactionAmount

		public GuaranteeTransactionCalculationResult GetTotalDataBaseTransactionAmounts()
		{
			using (var sumCommand = CargoWise.Data.Db.Connection.Command("GetCusPermitHeaderTotals"))
			{
				GuaranteeTransactionCalculationResult result = null;
				sumCommand.CommandTimeout = 0;
				sumCommand.CommandType = CommandType.StoredProcedure;
				sumCommand.AddParameter("@CPH", SqlDbType.UniqueIdentifier, this.PK.ToGuid());
				sumCommand.AddOutputParameter("@calculationDate", SqlDbType.DateTime, 65535, 0, 0, 0);
				sumCommand.AddOutputParameter("@pendingTransactionsBalance", SqlDbType.Money, 65535, 0, 0, 0);
				sumCommand.AddOutputParameter("@confirmedBalance", SqlDbType.Money, 65535, 0, 0, 0);
				var currency = Currency;

				sumCommand.ExecuteNonQuery();
				var value = sumCommand.GetParameterValue("@calculationDate");
				if (value != null && value != DBNull.Value)
				{
					sumCommand.ExecuteNonQuery();
					var when = new ZDateTime(value);
					var pendingBalance = new Money((decimal)sumCommand.GetParameterValue("@pendingTransactionsBalance"), currency);
					var confirmedBalance = new Money((decimal)sumCommand.GetParameterValue("@confirmedBalance"), currency);
					var totalBalance = new Money(pendingBalance.Amount + confirmedBalance.Amount, currency);
					result = new GuaranteeTransactionCalculationResult(confirmedBalance, pendingBalance, totalBalance, when);
				}
				else
				{
					result = new GuaranteeTransactionCalculationResult(new Money(0, currency), new Money(0, currency), new Money(0, currency), ZDateTime.UtcNow);
				}
				return result;
			}
		}

		#endregion

		#region ICusCodeDataTypeSupporter Members

		public IEnumerable<IBusinessObjectFetchStrategy> GetFetchStrategies()
		{
			foreach (var fetchStrategy in GetAdditionalBusinessObjectFetchStrategies())
			{
				yield return fetchStrategy;
			}
		}

		protected virtual IEnumerable<IBusinessObjectFetchStrategy> GetAdditionalBusinessObjectFetchStrategies()
		{
			yield return new CusCodeDataTypeSupporterFetchStrategy(this);
		}

		public IDictionary<ZString, Type> GetCusCodeDataTypes() => GetCusCodeDataTypesCore();

		protected virtual Dictionary<ZString, Type> GetCusCodeDataTypesCore()
		{
			var result = new Dictionary<ZString, Type>
			{
				{ GuaranteeCusCodeDataTypeList.Codes.GRN, typeof(CusGuaranteeReferenceNumber) }
			};
			return result;
		}

		RefCurrency Currency
		{
			get
			{
				var currency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, CPH_UnitOfMeasure)
					?? PermitHolder?.Country?.LocalCurrency ?? GlbBranch.CurrentBranch.Country.LocalCurrency;
				return currency;
			}
		}

		public GuaranteeTransactionCalculationResult LastDataBaseTransactionCalculation => Factory.GetCached(ref lastDataBaseTransactionCalculation, GetTotalDataBaseTransactionAmounts);
		CachedProperty<GuaranteeTransactionCalculationResult> lastDataBaseTransactionCalculation;

		public ZDecimal CPH_Calc_UsedBalance => CPH_Calc_OpeningBalance - CPH_Calc_TotalBalanceIncludingPendingDecimal;
		public ZPropertyInfo CPH_Calc_UsedBalanceInfo => GetZPropertyInfo(nameof(CPH_Calc_UsedBalance));

		public Money CPH_Calc_PendingBalance => new Money(LastDataBaseTransactionCalculation.PendingBalance.Amount + UnsavedTransactionTotal, Currency);
		public Money CPH_Calc_TotalBalanceIncludingPending => new Money(LastDataBaseTransactionCalculation.TotalBalance.Amount + UnsavedTransactionTotal, Currency);

		public ZDecimal CPH_Calc_PendingBalanceDecimal => CPH_Calc_PendingBalance.Amount;
		public ZDecimal CPH_Calc_TotalBalanceIncludingPendingDecimal => CPH_Calc_TotalBalanceIncludingPending.Amount;

		public ZPropertyInfo CPH_Calc_PendingBalanceDecimalInfo => GetZPropertyInfo(nameof(CPH_Calc_PendingBalanceDecimal));
		public ZPropertyInfo CPH_Calc_TotalBalanceIncludingPendingDecimalInfo => GetZPropertyInfo(nameof(CPH_Calc_TotalBalanceIncludingPendingDecimal));

		public ZDecimal UnsavedTransactionTotal => Factory.GetCached(ref unsavedTransactionTotal, () => CusGuaranteeLineTransactions.Where(x => !x.IsInDatabase).Sum(x => x.CPL_TranValue));
		CachedProperty<ZDecimal> unsavedTransactionTotal;
		public ZPropertyInfo UnsavedTransactionTotalInfo => GetZPropertyInfo(nameof(UnsavedTransactionTotal));

		#endregion

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();

			if (!SupportsAdditionalCustomsReferences)
			{
				AdditionalGuaranteeReferences.RemoveAndDeleteAll();
			}
		}

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore() => new BaseCusGuaranteeHeaderFetchStrategy(this);
	}
}
