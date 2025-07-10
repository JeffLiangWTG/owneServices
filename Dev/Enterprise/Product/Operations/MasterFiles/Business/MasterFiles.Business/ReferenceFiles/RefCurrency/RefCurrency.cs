using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration.ZArchitecture;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[DebuggerDisplay("RefCurrency: {" + RefCurrency.Schema.RX_Code + "}")]
	[DescriptionProperty(AutoRefCurrency.Schema.RX_Desc)]
	public class RefCurrency : AutoRefCurrency, ICurrency, IRefCurrency, IDocManagerSupport, IZUnit
	{
		public new abstract class Schema : AutoRefCurrency.Schema
		{
			public const string RX_IsExcludedCFXCalculation = "RX_IsExcludedCFXCalculation";
		}

		public RefCurrency(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static RefCurrency New(BusinessObjectFactory factory)
		{
			return factory.New<RefCurrency>();
		}

		#region Default Values + Loading

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			RX_IsSystem = false;
			if (!IsInDatabase)
			{
				RX_IsActive = true;
				RX_ISOSubUnitRatio = 100;
			}
		}

		public static RefCurrency LoadFromForeignCode(BusinessObjectFactory factory, ZString foreignCode, OrgHeader organisation)
		{
			var currencyFilter = new ZQuery(OrgPatternMatchOverrideSchema.OO_OH, organisation.PK);
			currencyFilter.AddToFilter(OrgPatternMatchOverrideSchema.OO_Relationship, Core.Constants.OrgPatternMatchOverrideRelationships.Currency);
			currencyFilter.AddToFilter(OrgPatternMatchOverrideSchema.OO_ForeignCode, foreignCode);

			var orgPatternMatchOverrides = factory.Load<OrgPatternMatchOverride>(currencyFilter);
			if (orgPatternMatchOverrides.Length == 1)
			{
				var orgPatternMatch = orgPatternMatchOverrides[0];
				return factory.Load<RefCurrency>(orgPatternMatch.OO_LocalGuid);
			}

			return null;
		}

		public static RefCurrency LoadFromCurrencyCode(BusinessObjectFactory factory, ZString currencyCode)
		{
			return (RefCurrency)factory.LoadFromNaturalKey(typeof(RefCurrency), RefCurrencySchema.RX_Code, currencyCode);
		}

		#endregion

		public static bool IsExcludedCFXCalculation(ZString currencyCode)
		{
			return AccountingMasterFilesRegistry.Instance.ExcludeCurrencyFromCFXCalculation.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty).ContainsCode(currencyCode);
		}

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();

			if (rx_IsExcludedCFXCalculationHasChanges)
			{
				if (RX_CodeInfo.HasChanges && CFXCalculationExcludedCurrencies.ContainsCode((ZString)RX_CodeInfo.OriginalValue))
				{
					AddRemoveCFXCalculationExcludedCurrencies(RX_Code, (ZString)RX_CodeInfo.OriginalValue);
				}

				if (CFXCalculationExcludedCurrencies.ContainsCode(RX_Code) != RX_IsExcludedCFXCalculation)
				{
					if (RX_IsExcludedCFXCalculation)
					{
						AddRemoveCFXCalculationExcludedCurrencies(RX_Code, ZString.Empty);
					}
					else
					{
						AddRemoveCFXCalculationExcludedCurrencies(ZString.Empty, RX_Code);
					}

					if (!IsInDatabase || ZPropertyInfoHash.Cast<ZPropertyInfo>().Any(x => x.IsPersistent && x.HasChanges))
					{
						ShouldAppendCFXCalculationEditLogOnCreateAutoAdminLog = true;
					}
					else //system auto log will not be created.
					{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
						var editLog = Logs.AddNew(Events.EditedARecord);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
						AppendCFXCalculationEditLog(editLog);
					}
				}
			}
		}

		protected override void OnCreateAutoAdminLog()
		{
			base.OnCreateAutoAdminLog();

			var autoCreatedLog = Logs.AutoCreatedLog;
			if (ShouldAppendCFXCalculationEditLogOnCreateAutoAdminLog && autoCreatedLog != null && !autoCreatedLog.IsInDatabase)
			{
				AppendCFXCalculationEditLog(autoCreatedLog);
				ShouldAppendCFXCalculationEditLogOnCreateAutoAdminLog = false;
			}
		}

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);

			if (saveSucceeded)
			{
				rx_IsExcludedCFXCalculationHasChanges = false;
			}
		}

		#region Properties

		#region Decimals

		public int Decimals
		{
			get
			{
				if (RX_SubUnitRatio <= 1)
				{
					return 0;
				}

				return (int)Math.Log10(RX_SubUnitRatio);
			}
		}

		public const int MAX_DECIMAL = 3;
		#endregion

		#region ISODecimals

		public int ISODecimals => RX_ISOSubUnitRatio <= 1 ? 0 : (int)Math.Log10(RX_ISOSubUnitRatio);

		#endregion

		#region Current Exchange Rates

		#region CurrentSellRate

		public ZDecimal CurrentSellRate
		{
			get
			{
				if (currentSellRate == null)
				{
					currentSellRate = new CachedProperty<ZDecimal>(Factory, delegate
					{ return GetCurrentExchangeRate(Core.Constants.ExchangeRateTypes.Code.SellRate); });
				}
				return currentSellRate.Value;
			}
		}
		CachedProperty<ZDecimal> currentSellRate;

		#endregion

		#region CurrentBuyRate

		public ZDecimal CurrentBuyRate
		{
			get
			{
				if (currentBuyRate == null)
				{
					currentBuyRate = new CachedProperty<ZDecimal>(Factory, delegate
					{ return GetCurrentExchangeRate(Core.Constants.ExchangeRateTypes.Code.BuyRate); });
				}
				return currentBuyRate.Value;
			}
		}
		CachedProperty<ZDecimal> currentBuyRate;

		#endregion

		#region CurrentCustomsRate

		public ZDecimal CurrentCustomsRate
		{
			get
			{
				if (currentCustomsRate == null)
				{
					currentCustomsRate = new CachedProperty<ZDecimal>(Factory, delegate
					{ return GetCurrentExchangeRate(Core.Constants.ExchangeRateTypes.Code.CustomsRate); });
				}
				return currentCustomsRate.Value;
			}
		}
		CachedProperty<ZDecimal> currentCustomsRate;

		#endregion

		ZDecimal GetCurrentExchangeRate(ZString exRateType)
		{
			ZQuery filter = new ZQuery();
			filter.AddToFilter(RefExchangeRateSchema.RE_RX_NKExCurrency, RX_Code);
			filter.AddToFilter(RefExchangeRateSchema.RE_StartDate, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, ZDateTime.Today);
			filter.AddToFilter(RefExchangeRateSchema.RE_ExpiryDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, ZDateTime.Today);
			filter.AddToFilter(RefExchangeRateSchema.RE_ExRateType, exRateType);
			filter.AddToFilter(RefExchangeRateSchema.RE_GC, GlbCompany.CurrentCompany.PK);
			RefExchangeRate rate = (RefExchangeRate)Factory.LoadTop1(typeof(RefExchangeRate), filter);

			return (rate != null) ? rate.RE_SellRate : (ZDecimal)0m;
		}

		#endregion

		[ReadOnlyMember(RefCurrencySchema.Constants.RX_IsSystem)]
		public override ZInt RX_SubUnitRatio
		{
			get { return base.RX_SubUnitRatio; }
			set { base.RX_SubUnitRatio = value; }
		}

		[ResourceStringData("12df7581-c4f9-43f6-8c95-6b1534932f63", Caption = "ISO Minor Unit Ratio")]
		[ReadOnlyMember(RefCurrencySchema.Constants.RX_IsSystem)]
		public override ZInt RX_ISOSubUnitRatio
		{
			get { return base.RX_ISOSubUnitRatio; }
			set { base.RX_ISOSubUnitRatio = value; }
		}

		[ReadOnlyMember(RefCurrencySchema.Constants.RX_IsSystem)]
		public override ZString RX_Code
		{
			get { return base.RX_Code; }
			set { base.RX_Code = value; }
		}

		[ReadOnlyMember(RefCurrencySchema.Constants.RX_IsSystem)]
		public override ZString RX_Symbol
		{
			get { return base.RX_Symbol; }
			set { base.RX_Symbol = value; }
		}

		[BusinessObjectTestExclude]
		[RefCurrencyTranslatableDataField(Schema.RX_UnitName, MaxLength = Schema.RX_UnitNameMaxLength, Asmid = ResString.AssemblyId)]
		[ReadOnlyMember(RefCurrencySchema.Constants.RX_IsSystem)]
		public override ZString RX_UnitName
		{
			get { return base.RX_UnitName; }
			set { base.RX_UnitName = value; }
		}

		public MultilingualString RX_UnitNameMultilingual
		{
			get { return new MultilingualLanguageText(PK.IsEmpty ? null : PK.ToString(), RefCurrencySchema.Constants.Prefix, Schema.RX_UnitName, RX_UnitName, Factory); }
		}

		[BusinessObjectTestExclude]
		[RefCurrencyTranslatableDataField(Schema.RX_SubUnitName, MaxLength = Schema.RX_SubUnitNameMaxLength, Asmid = ResString.AssemblyId)]
		[ReadOnlyMember(RefCurrencySchema.Constants.RX_IsSystem)]
		public override ZString RX_SubUnitName
		{
			get { return base.RX_SubUnitName; }
			set { base.RX_SubUnitName = value; }
		}

		public MultilingualString RX_SubUnitNameMultilingual
		{
			get { return new MultilingualLanguageText(PK.IsEmpty ? null : PK.ToString(), RefCurrencySchema.Constants.Prefix, Schema.RX_SubUnitName, RX_SubUnitName, Factory); }
		}

		[BusinessObjectTestExclude]
		[RefCurrencyTranslatableDataField(Schema.RX_Desc, MaxLength = Schema.RX_DescMaxLength, Asmid = ResString.AssemblyId)]
		[ReadOnlyMember(RefCurrencySchema.Constants.RX_IsSystem)]
		public override ZString RX_Desc
		{
			get { return base.RX_Desc; }
			set { base.RX_Desc = value; }
		}

		public MultilingualString RX_DescMultilingual
		{
			get { return new MultilingualLanguageText(PK.IsEmpty ? null : PK.ToString(), RefCurrencySchema.Constants.Prefix, Schema.RX_Desc, RX_Desc, Factory); }
		}

		[ReadOnly(true)]
		public override ZBool RX_IsSystem
		{
			get { return base.RX_IsSystem; }
			set { base.RX_IsSystem = value; }
		}

		public ZBool RX_IsExcludedCFXCalculation
		{
			get
			{
				if (rx_IsExcludedCFXCalculationCompany != GlbCompany.CurrentCompany.PK)
				{
					rx_IsExcludedCFXCalculation = CFXCalculationExcludedCurrencies.ContainsCode(RX_Code);
					rx_IsExcludedCFXCalculationCompany = GlbCompany.CurrentCompany.PK;
				}
				return rx_IsExcludedCFXCalculation;
			}
			set
			{
				SetNonPersistentPropertyValue(RX_IsExcludedCFXCalculationInfo, ref rx_IsExcludedCFXCalculation, value);
				rx_IsExcludedCFXCalculationHasChanges = true;
			}
		}

		ZBool rx_IsExcludedCFXCalculation;
		ZGuid rx_IsExcludedCFXCalculationCompany;
		bool rx_IsExcludedCFXCalculationHasChanges;

		public ZPropertyInfo RX_IsExcludedCFXCalculationInfo
		{
			get { return GetZPropertyInfo(Schema.RX_IsExcludedCFXCalculation); }
		}

		#endregion

		#region Delete

		public override bool CanDelete
		{
			get
			{
				if (RX_IsSystem)
				{
					return false;
				}
				else
				{
					return base.CanDelete;
				}
			}
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get
			{
				return ResString.GetMultilingualString("287BE539-74FD-4542-8355-AE4CB74A0036", "This is a system currency and cannot be deleted. If you wish to disable it, edit the currency and untick the \"Is Active\" checkbox.");
			}
		}

		public override void Delete()
		{
			string errorMessage = AssociatedWithCompany();
			if (errorMessage == null)
			{
				if (IsInDatabase && RX_IsExcludedCFXCalculation)
				{
					AddRemoveCFXCalculationExcludedCurrencies(ZString.Empty, RX_Code);
				}
				base.Delete();
				ExchangeRates.DeleteAll();
			}
			else
			{
				throw new CannotDeleteException(errorMessage);
			}
		}

		string AssociatedWithCompany()
		{
			var query = new ZQuery();
			query.AddToFilter(GlbCompanySchema.GC_RX_NKLocalCurrency, RX_Code);
			var companies = (GlbCompany[])Factory.Load(typeof(GlbCompany), query);
			if (companies.Length > 0)
			{
				StringBuilder s = new StringBuilder();
				for (int i = 0; i < 3 && i < companies.Length; ++i)
				{
					s.Append(i > 0 ? ", " : "");
					s.Append(companies[i].GC_Name);
					s.Append(i == 2 && companies.Length > 3 ? "..." : "");
				}
				return Res.GetString("cfdee395-8562-41ea-8ed8-e1a894cbdf9f", "You cannot delete {0} because it is used by company(s) ({1}).", HumanReadableName, s);
			}
			return null;
		}

		#endregion

		#region Logging

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		protected override BusinessObject[] BusinessObjectsWithRelatedEventsCore
		{
			get
			{
				ArrayList objects = new ArrayList();
				objects.AddRange(ExchangeRates);

				return (BusinessObject[])objects.ToArray(typeof(BusinessObject));
			}
		}

		#endregion

		#region Related Business Objects

		#region ExchangeRates

		[ChildEditable(true)]
		public RefExchangeRateDependentCollection ExchangeRates
		{
			get
			{
				if (fExchangeRates == null)
				{
					fExchangeRates = new RefExchangeRateDependentCollection(this);
					RegisterEditableChildObject(fExchangeRates);
				}

				return fExchangeRates;
			}
		}
		RefExchangeRateDependentCollection fExchangeRates;

		#endregion

		#endregion

		#region Exchange Rates

		public ZDecimal GetRateForDate(ExchangeRateType rateType, ZDateTime dateForRate, int maximumDaysToFallback)
		{
			return CurrencyConverter.New(Factory, dateForRate, rateType, maximumDaysToFallback).GetExchangeRate(this);
		}

		public ZDecimal GetRateForDate(ExchangeRateType rateType, ZDateTime dateForRate, int maximumDaysToFallback, out ZDateTime foundRateDate)
		{
			return CurrencyConverter.New(Factory, dateForRate, rateType, maximumDaysToFallback).GetExchangeRate(this, out foundRateDate);
		}

		public ZDecimal GetCustomsRate(ZDateTime dateForRate)
		{
			return GetRateForDate(ZArchitecture.Core.ExchangeRateType.Customs, dateForRate, 0);
		}

		public RefExchangeRate SetCustomsRate(ZDateTime exchangeRateDateStart, ZDateTime exchangeRateDateExpire, ZDecimal exchangeRate)
		{
			ZQuery sqlFilter = new ZQuery();
			sqlFilter.AddToFilter(RefExchangeRateSchema.RE_RX_NKExCurrency, RX_Code);
			sqlFilter.AddToFilter(RefExchangeRateSchema.RE_GC, GlbCompany.CurrentCompany.PK);
			sqlFilter.AddToFilter(RefExchangeRateSchema.RE_StartDate, exchangeRateDateStart);
			sqlFilter.AddToFilter(RefExchangeRateSchema.RE_ExRateType, Core.Constants.ExchangeRateTypes.Code.CustomsRate);

			Func<RefExchangeRate, IDisposable> getRateValidationSuspender = rate => IsValidationSuspended ? rate.GetValidationSuspender() : null;
			RefExchangeRate refExchangeRate = (RefExchangeRate)Factory.LoadTop1(typeof(RefExchangeRate), sqlFilter);
			if (refExchangeRate == null)
			{
				refExchangeRate = Factory.New<RefExchangeRate>();
				using (getRateValidationSuspender(refExchangeRate))
				{
					refExchangeRate.RE_RX_NKExCurrency = RX_Code;
					refExchangeRate.RE_GC = GlbCompany.CurrentCompany.PK;
					refExchangeRate.RE_StartDate = exchangeRateDateStart;
					refExchangeRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRate;
					refExchangeRate.RE_SellRate = exchangeRate;
				}
			}
			using (getRateValidationSuspender(refExchangeRate))
			{
				refExchangeRate.RE_ExpiryDate = exchangeRateDateExpire;
				refExchangeRate.RE_SellRate = exchangeRate;
			}

			return refExchangeRate;
		}

		public ZDecimal ConvertUsingCustomsRate(ZDateTime dateForRate, ZDecimal amount, RefCurrency destinationCurrency)
		{
			Money originalAmount = new Money(amount, this);
			CurrencyConverter cC = CurrencyConverter.New(Factory, dateForRate, ZArchitecture.Core.ExchangeRateType.Customs, 0);
			Money resultAmount = cC.ConvertRounded(originalAmount, destinationCurrency);

			return resultAmount.Amount;
		}

		public ZDecimal ConvertUsingSellRate(ZDateTime dateForRate, ZDecimal amount, RefCurrency destinationCurrency)
		{
			Money originalAmount = new Money(amount, this);
			CurrencyConverter cC = CurrencyConverter.New(Factory, dateForRate, ZArchitecture.Core.ExchangeRateType.Sell, 0);
			Money resultAmount = cC.ConvertRounded(originalAmount, destinationCurrency);

			return resultAmount.Amount;
		}

		#endregion

		#region CFX Calculation Excluded Currencies

		CodeDescriptionBoolCollection CFXCalculationExcludedCurrencies
		{
			get { return AccountingMasterFilesRegistry.Instance.ExcludeCurrencyFromCFXCalculation.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty); }
		}

		void AddRemoveCFXCalculationExcludedCurrencies(ZString addCode, ZString removeCode)
		{
			AccountingMasterFilesRegistry.Instance.RemoveItemFromCacheIfOlderThan(AccountingMasterFilesRegistry.Instance.ExcludeCurrencyFromCFXCalculation.Name, TimeSpan.Zero);
			var newCFXCalculationExcludedCurrencies = new CodeDescriptionBoolCollection(CFXCalculationExcludedCurrencies);
			if (!addCode.IsEmpty)
			{
				newCFXCalculationExcludedCurrencies.Add(addCode);
			}
			if (!removeCode.IsEmpty)
			{
				newCFXCalculationExcludedCurrencies.Remove(newCFXCalculationExcludedCurrencies.Cast<CodeDescriptionBool>().FirstOrDefault(x => x.Code == removeCode));
			}

			AccountingMasterFilesRegistry.Instance.ExcludeCurrencyFromCFXCalculation.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, newCFXCalculationExcludedCurrencies);
		}

		#endregion

		#region ICurrency Members

		public string Code
		{
			get { return RX_Code; }
		}

		Guid ICurrency.PK
		{
			get { return base.PK.ToGuid(); }
		}

		#endregion

		#region HumanReadableNameCore

		protected override ZString HumanReadableNameCore
		{
			get { return RX_Code; }
		}

		#endregion

		#region IDocManagerSupport Members

		DocManagerInfo IDocManagerSupport.DocManagerInfo
		{
			get
			{
				if (docManagerInfo == null)
				{
					docManagerInfo = new DocManagerInfo(this, Core.Constants.DocManagerCodes.Currency);
				}
				return docManagerInfo;
			}
		}
		DocManagerInfo docManagerInfo;

		#endregion

		#region ZUnit

		ZString IZUnit.Code => RX_Code;
		ZUnitType IZUnit.Type => ZUnitType.Currency;

		public static implicit operator ZUnit(RefCurrency currency) => currency == null ? default(ZUnit) : new ZUnit(currency);
		public ZUnit ToZUnit() => this;

		#endregion

		#region CFXCalculationEditLog

		bool ShouldAppendCFXCalculationEditLogOnCreateAutoAdminLog { get; set; }

		void AppendCFXCalculationEditLog(StmALog log)
		{
			var cfxLog = Res.GetString("B96D01F2-81E8-48DA-82FA-67ACC01056A0", "Exclude from CFX Calculations Flag set to '{0}', Company: {1}",
				RX_IsExcludedCFXCalculation ? Res.GetString("C3FFCE77-CBD8-4059-920A-DB04A3BCE442", "Yes") : Res.GetString("8F6489D9-0C4C-40BB-B07D-6D8474E0DD7D", "No"),
				GlbCompany.CurrentCompany.GC_Code);

			var newRef = ZString.Format("{0}{1}{2}",
				log.SL_Reference,
				(log.SL_Reference.IsEmpty || log.SL_Reference.EndsWith("|", StringComparison.OrdinalIgnoreCase) ? "" : "|"),
				cfxLog);

			log.UpdateReference(newRef);
		}

		#endregion
	}
}
