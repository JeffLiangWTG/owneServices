using System;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[CodeProperty(Schema.RE_RX_NKExCurrency), DescriptionProperty(Schema.RE_ExRateType)]
	public class RefExchangeRate : AutoRefExchangeRate, IRefExchangeRate, IHaveNonUniquePrimaryKey
	{
		public RefExchangeRate(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Loader

		public new class Loader : BusinessObject.Loader
		{
			public Loader(BusinessObjectFactory factory)
				: base(factory)
			{ }

			public RefExchangeRate GetEffectiveRateOn(ZDateTime date, ZString currency, ZString rateType, ZGuid companyPK)
			{
				if (!date.IsValid || currency.IsEmpty || rateType.IsEmpty || !companyPK.IsValid)
				{
					return null;
				}

				var query = new ZQuery(RefExchangeRateSchema.RE_RX_NKExCurrency, currency);
				query.AddToFilter(RefExchangeRateSchema.RE_ExRateType, rateType);
				query.AddToFilter(RefExchangeRateSchema.RE_StartDate, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, date);
				query.AddToFilter(RefExchangeRateSchema.RE_ExpiryDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, date);
				query.AddToFilter(RefExchangeRateSchema.RE_GC, companyPK);
				query.OrderBy = RefExchangeRateSchema.RE_StartDate.Name + " DESC";
				return Factory.LoadTop1<RefExchangeRate>(query);
			}

			public static ZQuery GetFilterByPK(ZGuid pk)
			{
				var filter = new ZQuery(RefExchangeRateSchema.RE_GC, GlbCompany.CurrentCompany.PK);
				return filter.AddToFilter(RefExchangeRateSchema.PK, pk);
			}

			protected override Type GetTypeOfBusinessObjectToLoad()
			{
				return typeof(RefExchangeRate);
			}
		}

		#endregion

		public static RefExchangeRate New(BusinessObjectFactory factory) => factory.New<RefExchangeRate>();

		#region Default Values and Loading

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			RE_GC = GlbCompany.CurrentCompany.PK;
			RE_SellRate = (decimal)1;
			RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.BuyRate;
			RE_StartDate = ZDateTime.Today;
			RE_ExpiryDate = ZDateTime.Today;
		}

		public override void OnLoaded()
		{
			base.OnLoaded();
			ReadOnly = (IsCustomsExchangeRate && !Env.Security.CustomsExchangeRateUpdate.IsAllowed) || (IsGlobalCreditExchangeRate && !Env.Security.GCBExchangeRateUpdate.IsAllowed);
		}

		#endregion

		#region Business Object Functionality

		public override void OnSaving()
		{
			base.OnSaving();
			if (RE_ExpiryDate.IsValid)
			{
				ExchangeRateReader.GetReaderInstance().ClearCache(GlbCompany.CurrentCompany.PK.ToGuid(), RE_ExRateType, RE_RX_NKExCurrency, RE_ExpiryDate.ToDateTime());
			}
		}

		[SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
		public override bool IsSavedByFactory
		{
			get
			{
				var result = base.IsSavedByFactory;

				if (result && !IsDeleted && (!RE_StartDate.IsValid || !RE_ExpiryDate.IsValid))
				{
					var message =
						FormattableString.Invariant($@"Saving invalid DateTime on Exchange Rate, saving stack trace: 
{new StackTrace()}");
					ErrorReporter.ReportOnce("Saving_Invalide_DateTime", message);
					result = false;
				}

				return result;
			}
		}

		public override bool CanDelete
		{
			get
			{
				var result = base.CanDelete;

				if (result && IsReadOnlyCustomsExchangeRateForWhichNoSecurityRightExists || RE_IsSystem)
				{
					result = false;
				}
				return result;
			}
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get
			{
				var result = base.ReasonForNotAbleToDelete;

				if (IsReadOnlyCustomsExchangeRateForWhichNoSecurityRightExists || RE_IsSystem)
				{
					result = ResString.GetMultilingualString("9833e548-d24f-4039-972a-8153785f99f1", "Cannot delete a System Defined Customs exchange rate.");
				}
				return result;
			}
		}

		bool IsReadOnlyCustomsExchangeRateForWhichNoSecurityRightExists => IsCustomsExchangeRate && ReadOnly && !Env.Security.CustomsExchangeRateUpdate.IsAllowed;

		#endregion

		#region Properties

		public static readonly TimeSpan EndOfDay = new TimeSpan(23, 59, 0);
		public static readonly TimeSpan BeginningOfDay = new TimeSpan(0, 0, 0);

		[RelatedBusinessObject("Company")]
		[List("Lookups.Companies")]
		public override ZGuid RE_GC { get => base.RE_GC; set => base.RE_GC = value; }

		public virtual GlbCompany Company => (GlbCompany)Factory.Load(typeof(GlbCompany), RE_GC);

		#region RE_StartDate

		[BusinessObjectTestExclude] // the setter sets the time part to the start of the day (00:00:00)
		public override ZDateTime RE_StartDate
		{
			get { return base.RE_StartDate; }
			set
			{
				if (base.RE_ExRateType == Core.Constants.ExchangeRateTypes.Code.PeriodEndRate)
				{
					value = AccountingPeriodCalculator.GetLastDayForPeriod(value);
				}
				// build up new zdatetime for beginning of date
				if (value.IsValid)
				{
					base.RE_StartDate = new ZDateTime(value.Year, value.Month, value.Day).Add(BeginningOfDay);
				}
				else
				{
					base.RE_StartDate = value;
				}
				if (base.RE_ExRateType == Core.Constants.ExchangeRateTypes.Code.PeriodEndRate)
				{
					RE_ExpiryDate = RE_StartDate;
				}
			}
		}

		#endregion

		#region RE_ExpiryDate

		[BusinessObjectTestExclude] // the setter sets the time part to the end of the day (23:59:00)
		public override ZDateTime RE_ExpiryDate
		{
			get { return base.RE_ExpiryDate; }
			set
			{
				if (value.IsValid)
				{
					base.RE_ExpiryDate = new ZDateTime(value.Year, value.Month, value.Day).Add(EndOfDay);
				}
				else
				{
					base.RE_ExpiryDate = value;
				}
			}
		}

		protected bool RE_ExpiryDate_ReadOnly
		{
			get { return RE_ExRateType == Core.Constants.ExchangeRateTypes.Code.PeriodEndRate; }
		}

		#endregion

		#region RE_ExRateType

		[List("Lookups.ExRateTypes")]
		public override ZString RE_ExRateType
		{
			get { return base.RE_ExRateType; }
			set
			{
				var oldValue = RE_ExRateType;
				base.RE_ExRateType = value;
				if (value == Core.Constants.ExchangeRateTypes.Code.PeriodEndRate)
				{
					RE_StartDate = RE_StartDate;
				}
				if (!IsCopying && oldValue != RE_ExRateType)
				{
					PopulateRE_AsPublishedIfNeeded();
				}
			}
		}

		#endregion

		#region RE_RX_NKExCurrency

		[List("Lookups.Currencies")]
		public override ZString RE_RX_NKExCurrency
		{
			get => base.RE_RX_NKExCurrency;
			set
			{
				var oldValue = RE_RX_NKExCurrency;
				base.RE_RX_NKExCurrency = value;
				if (!IsCopying && oldValue != RE_RX_NKExCurrency)
				{
					PopulateRE_AsPublishedIfNeeded();
				}
			}
		}

		#endregion

		#region RE_OH_Client

		[RelatedBusinessObject("Client")]
		[List("Lookups.LocalClients")]
		public override ZGuid RE_OH_Client { get => base.RE_OH_Client; set => base.RE_OH_Client = value; }

		public virtual OrgHeader Client => (OrgHeader)Factory.Load(typeof(OrgHeader), RE_OH_Client);

		#endregion

		#region IsAutoLogged

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		#endregion

		public override ZDecimal RE_SellRate
		{
			get => base.RE_SellRate;
			set
			{
				var oldValue = RE_SellRate;
				base.RE_SellRate = value;
				if (!IsCopying && oldValue != RE_SellRate)
				{
					PopulateRE_AsPublishedIfNeeded();
				}
			}
		}

		void PopulateRE_AsPublishedIfNeeded()
		{
			if (!AsPublishedIsApplicable)
			{
				if (!RE_AsPublished.IsEmpty)
				{
					RE_AsPublished = ZString.Empty;
				}
			}
			else if (IsIndiaCustomsRate)
			{
				var asPublished = ZString.Empty;

				if (RE_SellRate > ZDecimal.Zero && !RE_RX_NKExCurrency.IsEmpty && TryGetCurrencyMultiplier(out var multiplier))
				{
					asPublished = $"{multiplier} {RE_RX_NKExCurrency} = {RE_SellRate * multiplier} {Core.Constants.CurrencyCodes.India}";
				}

				if (RE_AsPublished != asPublished)
				{
					RE_AsPublished = asPublished;
				}
			}
		}

		internal bool TryGetCurrencyMultiplier(out int multiplier)
		{
			var attributeValue = ObjectFactory.Get<Enterprise.Integration.Customs.Shared.Universal.IRefCusCodeListProvider>().GetAttributeValues(Factory,
					Core.Constants.CountryCodes.India,
					Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.INCustomsStandardCurrency,
					ZDateTime.Today, RE_RX_NKExCurrency,
					Core.Constants.Customs.Universal.RefCusCodeList.Attributes.Multiplier).FirstOrDefault();
			multiplier = 0;
			return (!attributeValue.IsEmpty && int.TryParse(attributeValue, out multiplier));
		}

		public ZBool IsIndiaCustomsRate => Company.GC_RN_NKCountryCode == Core.Constants.CountryCodes.India
			&& (RE_ExRateType == Core.Constants.ExchangeRateTypes.Code.CustomsRateSecondary || RE_ExRateType == Core.Constants.ExchangeRateTypes.Code.CustomsRate);

		public ZBool IsCustomsExchangeRate => RE_ExRateType == Core.Constants.ExchangeRateTypes.Code.CustomsRate;

		public ZBool AsPublishedIsApplicable => IsCustomsExchangeRate || IsIndiaCustomsRate;

		public ZBool LocalClientIsApplicable => Factory.IsLocalClientExchangeRatefieldNeeded();

		public ZBool IsGlobalCreditExchangeRate => RE_ExRateType == Core.Constants.ExchangeRateTypes.Code.GlobalCreditControl;

		#endregion

		#region Implementation

		AccountingPeriodCalculator AccountingPeriodCalculator => fAccountingPeriodCalculator ?? (fAccountingPeriodCalculator = new AccountingPeriodCalculator(Factory));
		AccountingPeriodCalculator fAccountingPeriodCalculator;

		public override bool ReadOnly => base.ReadOnly || RE_IsSystem;

		#endregion
	}

	public static class RefExchangeRateExtensions
	{
		public static bool IsLocalClientExchangeRatefieldNeeded(this BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("IsLocalClientExchangeRatefieldNeeded", delegate
			{
				var query = new ZQuery(new ZQuery(RefExchangeRateSchema.RE_OH_Client, SQLComparisonOperator.NotEqual, ZGuid.Empty));
				return factory.LoadTop1<RefExchangeRate>(query) != null;
			});
		}
	}
}
