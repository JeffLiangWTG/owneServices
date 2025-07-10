using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.LandedCosting.Business
{
	public class LandedCostingExRate : NonPersistentBusinessObject, IObsoleteValidation
	{
		public LandedCostingExRate(BusinessObjectFactory factory) : base(factory)
		{
		}

		public LandedCostingExRate(ILandedCostExchangeRateHolder exRateHolder) : base(exRateHolder.Factory)
		{
			ExchangeRateHolder = exRateHolder;
			using (SuspendSettingHasChanges())
			{
				if (ExchangeRate == 0 && ExchangeRateHolder != null)
				{
					ExchangeRate = ExchangeRateHolder.LandedCostExchangeRateDefault;
				}

				if (ExchangeRate == 0)
				{
					GlbCompany company = ExchangeRateHolder.Factory.Load<GlbCompany>(ExchangeRateHolder.CompanyPK);

					if (company.GC_RX_NKLocalCurrency == CurrencyCode)
					{
						ExchangeRate = 1;
					}
				}
			}
		}

		#region Bindable properties

		public ZString ReferenceNumber
		{
			get { return ExchangeRateHolder != null ? ExchangeRateHolder.ReferenceNumber : ZString.Empty; }
		}

		public ZPropertyInfo ReferenceNumberInfo
		{
			get { return GetZPropertyInfo(nameof(ReferenceNumber)); }
		}

		public ZString CurrencyCode
		{
			get { return ExchangeRateHolder != null ? ExchangeRateHolder.CurrencyCode : ZString.Empty; }
		}

		public ZPropertyInfo CurrencyCodeInfo
		{
			get { return GetZPropertyInfo(nameof(CurrencyCode)); }
		}

		public ZDecimal ExchangeRate
		{
			get { return ExchangeRateHolder != null ? ExchangeRateHolder.LandedCostExchangeRate : ZDecimal.Zero; }
			set
			{
				if (!IsSettingHasChangesSuspended)
				{
					HasChanges |= ExchangeRate != value;
				}
				if (ExchangeRateHolder != null)
				{
					ExchangeRateHolder.LandedCostExchangeRate = value;
				}
				ExchangeRateInfo.RefreshBinding();
				ValidateExchangeRate();
			}
		}

		public void ValidateExchangeRate()
		{
			if (!IsValidationSuspended)
			{
				ExchangeRateInfo.ClearAllNotifications();
				if (CurrencyCode != "" && ExchangeRate <= 0)
				{
					ExchangeRateInfo.AddError(ExchangeRateShouldBeGreaterThanZero);
				}
			}
		}
		public static string ExchangeRateShouldBeGreaterThanZero
		{
			get { return Res.GetString("5feac8b9-6732-4382-998c-d773d9db12ba", "Exchange rate should be greater than zero for Landed Costing. Please go to Landed Costing -> Exchange Rates and enter a value there."); }
		}

		public ZPropertyInfo ExchangeRateInfo
		{
			get { return GetZPropertyInfo(nameof(ExchangeRate)); }
		}

		public bool ExchangeRate_ReadOnly
		{
			get
			{
				GlbCompany company = Factory.Load<GlbCompany>(ExchangeRateHolder.CompanyPK);
				return CurrencyCode == company.GC_RX_NKLocalCurrency;
			}
		}

		#endregion

		#region Implementation

		readonly ILandedCostExchangeRateHolder ExchangeRateHolder;

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateExchangeRate();
		}

		#endregion
	}
}
