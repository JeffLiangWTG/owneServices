using System.Collections;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class RefExchangeRateValidation : AutoRefExchangeRateValidation
	{
		public RefExchangeRateValidation(AutoRefExchangeRate parent) : base(parent)
		{
		}

		RefExchangeRate ExRate => (RefExchangeRate)Parent;

		#region RE_ExpiryDate

		protected override void CheckRE_ExpiryDateIsValidZDateTimeRange()
		{
			if (!ExRate.RE_IsSystem && !ExRate.IsCustomsExchangeRate)
			{
				base.CheckRE_ExpiryDateIsValidZDateTimeRange();
			}
		}

		protected override void CheckRE_ExpiryDate()
		{
			base.CheckRE_ExpiryDate();

			if (!ExRate.RE_IsSystem)
			{
				MandatoryValidation.CheckEntered(ExRate.RE_ExpiryDateInfo);
				if (ExRate.RE_ExpiryDate < ExRate.RE_StartDate)
				{
					ExRate.RE_ExpiryDateInfo.AddError(Res.GetString("bf9f6078-f045-4879-84ea-4ff47fd8de49", "Expiry Date must be after Start Date."));
				}
				else
				{
					bool expiryDateExists = !ExRate.RE_ExpiryDate.IsEmpty;

					if (expiryDateExists && ExRate.RE_StartDate > ExRate.RE_ExpiryDate)
					{
						ExRate.RE_ExpiryDateInfo.AddError(Res.GetString("bf9f6078-f045-4879-84ea-4ff47fd8de49", "Expiry Date must be after Start Date."));
					}
					else
					{
						if (ExRate.ExCurrency != null && ExRate.HasChanges)
						{
							foreach (RefExchangeRate exchangeRate in ExRate.ExCurrency.ExchangeRates)
							{
								if (ExRate != exchangeRate && !exchangeRate.IsDeleted && ExRate.RE_ExRateType == exchangeRate.RE_ExRateType && ExRate.RE_OH_Client == exchangeRate.RE_OH_Client
									 && (!exchangeRate.RE_IsSystem || exchangeRate.RE_ExRateType == Core.Constants.ExchangeRateTypes.Code.CustomsRate))
								{
									bool hasOtherRowValidDateRange = !exchangeRate.RE_ExpiryDateInfo.HasErrors() && !exchangeRate.RE_StartDateInfo.HasErrors();
									bool hasOtherRowValidExpiryDate = !exchangeRate.RE_ExpiryDate.IsEmpty;
									if (hasOtherRowValidDateRange && hasOtherRowValidExpiryDate)
									{
										bool isStartDateInOtherRowDateRange = ExRate.RE_StartDate >= exchangeRate.RE_StartDate && ExRate.RE_StartDate <= exchangeRate.RE_ExpiryDate;
										bool dateRangeCoversOtherRowDateRange = ExRate.RE_StartDate < exchangeRate.RE_StartDate && ExRate.RE_ExpiryDate > exchangeRate.RE_ExpiryDate;
										if (isStartDateInOtherRowDateRange || dateRangeCoversOtherRowDateRange)
										{
											ExRate.RE_ExpiryDateInfo.AddError(Res.GetString("cd23a7ce-e620-4880-892c-17078d66b40a", "Overlapped start date is entered."));
											break;
										}
									}
								}
							}
						}
					}
				}
			}
		}

		#endregion

		#region RE_StartDate

		protected override void CheckRE_StartDate()
		{
			base.CheckRE_StartDate();
			ValidateRE_ExpiryDate();
		}

		#endregion

		#region RE_SellRate

		protected override void CheckRE_SellRate()
		{
			base.CheckRE_SellRate();
			MandatoryValidation.CheckEntered(ExRate.RE_SellRateInfo);
			if (ExRate.RE_SellRate <= 0)
			{
				ExRate.RE_SellRateInfo.AddError(Res.GetString("75bfe8d2-c7cc-4b15-94bf-52c895094461", "Sell Rate must be positive."));
			}
		}

		#endregion

		#region RE_ExRateType

		protected override void CheckRE_ExRateType()
		{
			base.CheckRE_ExRateType();
			if (!ExRate.ReadOnly)
			{
				if (ExRate.RE_ExRateType == Core.Constants.ExchangeRateTypes.Code.GlobalCreditControl && !Env.Security.GCBExchangeRateUpdate.IsAllowed)
				{
					ExRate.RE_ExRateTypeInfo.AddError(Res.GetString("5a00feb3-958a-474a-aaf9-7de048ec7db0", "You do not have security access to add a GCB - Global Credit Control exchange rate."));
				}
				else
				{
					ListValidation.ErrorIfInvalidCode(ExRate.RE_ExRateTypeInfo);
					MandatoryValidation.CheckEntered(ExRate.RE_ExRateTypeInfo);
				}
				ValidateRE_ExpiryDate();

				if (ExRate.RE_OH_Client.IsValid &&
					ExRate.RE_ExRateType != Core.Constants.ExchangeRateTypes.Code.BuyRate &&
					ExRate.RE_ExRateType != Core.Constants.ExchangeRateTypes.Code.SellRate)
				{
					ExRate.RE_ExRateTypeInfo.AddError(Res.GetString("7d4b5d87-4562-4305-a975-5a48b9bbf87e", "Only BUY/SEL rate can have a Local Client."));
				}
			}
			var currency = ExRate.ExCurrency;
			if (!ExRate.RE_IsSystem && (!ExRate.IsInDatabase || ExRate.RE_ExRateTypeInfo.HasChanges) && !ExRate.RE_ExRateType.IsEmpty && currency != null && !currency.RX_Code.IsEmpty)
			{
				var query = new ZQuery(RefExchangeRateSchema.RE_IsSystem, true);
				query.AddToFilter(RefExchangeRateSchema.RE_ExRateType, ExRate.RE_ExRateType);
				query.AddToFilter(RefExchangeRateSchema.RE_GC, ExRate.RE_GC);
				if (ExRate.Factory.LoadTop1<RefExchangeRate>(query) != null)
				{
					ExRate.RE_ExRateTypeInfo.AddWarning(Res.GetString("95C71008-FDB1-4700-8A94-216A66F84396",
						"Rate Type '{0}' is being managed by the system. It might be replaced by the system rate once the system is updated with new rates.", ExRate.RE_ExRateType));
				}
			}
		}

		#endregion

		#region RE_RX_NKExCurrency

		protected override void CheckRE_RX_NKExCurrency()
		{
			base.CheckRE_RX_NKExCurrency();
			var targetInfo = Parent.RE_RX_NKExCurrencyInfo;
			MandatoryValidation.CheckEntered(targetInfo);
			ListValidation.ErrorIfInvalidCode(targetInfo);

			if (Parent.RE_RX_NKExCurrency == GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency)
			{
				var validationMessage = Res.GetString("aa7372cb-e1e4-4943-9452-fe82c225dd64", "Currency for exchange rate should not be the same as Currency for logged in Company");
				var rateType = Parent.RE_ExRateType.ToUpperInvariant();
				if (rateType == Core.Constants.ExchangeRateTypes.Code.BuyRate || rateType == Core.Constants.ExchangeRateTypes.Code.SellRate)
				{
					targetInfo.AddError(validationMessage);
				}
				else
				{
					targetInfo.AddWarning(validationMessage);
				}
			}
		}

		#endregion

		#region RE_OH_Client

		protected override void CheckRE_OH_ClientIsValidZGuid()
		{
			// We don't want the default error message supported by the control, so we do this check in CheckRE_OH_Client
		}

		protected override void CheckRE_OH_Client()
		{
			base.CheckRE_OH_Client();
			var needValidation = ExRate.LocalClientIsApplicable;
			if (needValidation)
			{
				var error = Res.GetString("d43f1325-93b0-4e06-b209-7439787d6c68", "Local Client must be a Debtor.");
				var client = ExRate.RE_OH_Client;
				if (!client.IsEmpty)
				{
					if (!client.IsValid)
					{
						ExRate.RE_OH_ClientInfo.AddError(error);
					}
					else
					{
						var org = ExRate.Factory.Load<OrgHeader>(ExRate.RE_OH_Client);
						if (org != null && org.CompanyData != null && !org.CompanyData.OB_IsDebtor)
						{
							ExRate.RE_OH_ClientInfo.AddError(error);
						}
					}
				}
				ExRate.RE_OH_ClientInfo.AddWarning(Res.GetString("DB33FD69-8972-4215-BAFF-4A6DCE2C6462", "This functionality is being phased out. Instead, record the client’s preferred Ex Rate Type on the AR > Invoicing > Job Billing Exchange Rates tab. Additional exchange rate types can be created in the Registry: Accounting > Custom Exchange Rate Types."));
			}
		}

		#endregion

		#region RE_AsPublished
		protected override void CheckRE_AsPublished()
		{
			base.CheckRE_AsPublished();

			var validator = GetExRateValidationHelper(ExRate.Company.Country.Code);
			if (validator != null)
			{
				validator.CheckIfAsPublishedIsInRightFormat(ExRate.RE_SellRate, ExRate.RE_AsPublished, ExRate.RE_AsPublishedInfo);
			}
		}

		Enterprise.Integration.Customs.Shared.IExRateValidationHelper GetExRateValidationHelper(string countryCode)
		{
			Enterprise.Integration.Customs.Shared.IExRateValidationHelper result = null;

			if (!string.IsNullOrEmpty(countryCode))
			{
				var types = ObjectFactory.Get<Hashtable>("ExRateValidationHelpers");
				var objectHandle = (ObjectHandle)types[countryCode];
				result = (Enterprise.Integration.Customs.Shared.IExRateValidationHelper)objectHandle?.GetObject();
			}

			return result;
		}
		#endregion
	}
}
