using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MarketingManager.Business
{
	public class EntitySalesWrapperValidation : OrgSalesValidation
	{
		public EntitySalesWrapperValidation(EntitySalesWrapper parent)
			: base(parent)
		{
			ZValidationInternals = this;
		}

		readonly IValidationInternals ZValidationInternals;

		new EntitySalesWrapper Parent
		{
			get { return (EntitySalesWrapper)base.Parent; }
		}

		#region TotalRevenueCurrencyCode

		public void ValidateTotalRevenueCurrencyCode()
		{
			ZValidationInternals.Validate(Parent.TotalRevenueCurrencyCodeInfo, GetTotalRevenueCurrencyCodeValidationInvoker());
		}

		RunValidationInvoker GetTotalRevenueCurrencyCodeValidationInvoker()
		{
			return delegate
			{
				CheckTotalRevenueCurrencyCode();
			};
		}

		protected virtual void CheckTotalRevenueCurrencyCode()
		{
			MandatoryValidation.CheckEntered(Parent.TotalRevenueCurrencyCodeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.TotalRevenueCurrencyCodeInfo);

			if (!Parent.TotalRevenueCurrencyCodeInfo.HasErrors())
			{
				var missingExchangeRates = Parent.GetMissingExchangeRates();
				if (missingExchangeRates.Any())
				{
					var message = GetNoExchangeRateFoundMessage(
							Parent.DateForExchangeRate,
							Parent.CompanyForExchangeRate,
							missingExchangeRates.Select(x => x.RX_Code.ToString()));

					Parent.TotalRevenueCurrencyCodeInfo.AddWarning(message);
				}
			}
		}

		public static string GetNoExchangeRateFoundMessage(ZDateTime dateForExchangeRate, GlbCompany companyForExchangeRate, IEnumerable<string> missingCurrencies)
		{
			var commaDelimitedMissingCurrencies = string.Join(", ", missingCurrencies.OrderBy(x => x));

			if (companyForExchangeRate.PK == GlbCompany.CurrentCompany.PK)
			{
				return Res.GetString("09dd41d2-90dc-439e-a596-60ae8a6d5098", "There is no exchange rate valid on {0} for the following currency(s): {1}",
					dateForExchangeRate.ToShortDateString(),
					commaDelimitedMissingCurrencies);
			}
			else
			{
				return Res.GetString("a3096367-49a3-472c-a925-e54f800bf886", "{0} ({1}) does not have an exchange rate valid on {2} for the following currency(s): {3}",
					companyForExchangeRate.GC_Name,
					companyForExchangeRate.GC_Code,
					dateForExchangeRate.ToShortDateString(),
					commaDelimitedMissingCurrencies);
			}
		}

		#endregion
	}
}
