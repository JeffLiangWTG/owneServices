using System;
using System.Globalization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public class ExternalMessageValidation : ValidationProvider
	{
		public ExternalMessageValidation(BusinessObject businessObject) : base(businessObject)
		{
			this.businessObject = businessObject;
		}

		/// <summary>
		/// Sets a Message Error if the field is empty.
		/// </summary>
		/// <param name="propertyInfo">The property to validate.</param>
		public virtual void CheckEntered(ZPropertyInfo propertyInfo)
		{
			CheckEntered(propertyInfo, "");
		}

		public void CheckEntered(ZPropertyInfo propertyInfo, ZString customMessageError)
		{
			if (propertyInfo.Value.IsEmpty)
			{
				if (customMessageError.IsEmpty)
				{
					propertyInfo.AddMessageError(MandatoryValidation.MustBeEnteredMessage(propertyInfo.Description));
				}
				else
				{
					propertyInfo.AddMessageError(customMessageError);
				}
			}
			else
			{
				CheckInvalidValue(propertyInfo);
			}
		}

		public void CheckInvalidValue(ZPropertyInfo propertyInfo)
		{
			if (!propertyInfo.Value.IsValid && !propertyInfo.Value.IsEmpty && !propertyInfo.HasErrors())
			{
				propertyInfo.AddError(Res.GetString("6f4a0ebf-3e96-457e-9989-85e641450ca4", "Please enter a valid {0}", propertyInfo.Description));
			}
		}

		public virtual void ValidateExchangeRateExist(RefCurrencyCurrencyConverter currencyConverter, ZPropertyInfo currInfo, int maximumDaysToFallbackBeforeWarningShown = 0)
		{
			var currency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, (ZString)currInfo.Value);

			ZString localCurrency;
			var declaration = businessObject as BaseJobDeclaration;
			if (declaration != null)
			{
				localCurrency = declaration.LocalCurrencyCode;
			}
			else
			{
				var invoice = businessObject as BaseJobComInvoiceHeader;
				if (invoice != null)
				{
					localCurrency = invoice.LocalCurrencyCode;
				}
				else
				{
					var branchProvider = businessObject as IBranchProvider;
					localCurrency = (branchProvider != null && branchProvider.Branch.Country != null ? branchProvider.Branch.Company.Country : GlbCompany.CurrentCompany.Country).RN_RX_NKLocalCurrency;
				}
			}

			if (currency != null && currency.RX_Code != localCurrency)
			{
				if (currencyConverter.DateForRate.IsValid)
				{
					ZDateTime foundRateDate = ZDateTime.Empty;
					decimal fTodayRate = currencyConverter.GetExchangeRate(currency, out foundRateDate);//7 days is already set when CurrencyConverter is created and RateType is set depending on Declaration type
					if (fTodayRate == 0)
					{
						currInfo.AddMessageError(NoValidExRatesMessage(currencyConverter.DateForRate.ToString("d", null)));
					}
					else if ((currencyConverter.DateForRate.Date - foundRateDate).TotalDays > maximumDaysToFallbackBeforeWarningShown)//currencyConverter.DateForRate.DayOfYear - maximumDaysToFallbackBeforeWarningShown > foundRateDate.DayOfYear)
					{
						AddExchangeRateOutOfRangeError(currInfo, ExchangeRateOutOfRangeWarningMessage(currencyConverter.DateForRate.Date.ToString("d"), currency.RX_Code, foundRateDate.Date.ToString("d")));
					}
					else if (ValidateEachWorkingDayHasAnExchangeRate)
					{
						RefExchangeRate rate = currencyConverter.GetExchangeRateObject(currency);
						if (rate != null && rate.RE_ExpiryDate.DayOfYear - rate.RE_StartDate.DayOfYear > 0)
						{
							int workingDays = 0;
							for (ZDateTime currentDate = rate.RE_StartDate; currentDate <= rate.RE_ExpiryDate; currentDate = currentDate.AddDays(1))
							{
								if (currentDate.DayOfWeek != DayOfWeek.Saturday && currentDate.DayOfWeek != DayOfWeek.Sunday)
								{
									workingDays++;
									if (workingDays > 1)
									{
										break;
									}
								}
							}

							if (workingDays > 1)
							{
								ZStringBuilder warningMessage = new ZStringBuilder();
								warningMessage.Append(Res.GetString("d5f01884-56b4-404f-80b4-cbc331aa346f", "The exchange rate applied for this currency covers more than one working day (START: {0} {1}/END: {2}). Please check if Customs has published new rates for {3}.", rate.RE_StartDate.DayOfWeek.ToString(), rate.RE_StartDate.ToShortDateString(), rate.RE_ExpiryDate.ToShortDateString(), rate.RE_ExpiryDate.ToShortDateString()));
								warningMessage.Append(GetAdviceHowToFixNoValidExchangeRates());
								currInfo.AddWarning(warningMessage.ToString());
							}
						}
					}
				}
				else
				{
					currInfo.AddNotification(DateOfExportNotificationSeverity, Res.GetString("fa3c1c8a-b087-4431-8ca9-d9a0553637ea", "A valid date of export is required to calculate exchange rates."));
				}
			}
		}

		protected virtual INotificationType DateOfExportNotificationSeverity => CargoWise.EntityFramework.NotificationType.MessageError;

		public virtual string NoValidExRatesMessage(string currencyConverterDateForRate)
		{
			ZStringBuilder noValidExRatesMessage = new ZStringBuilder();
			noValidExRatesMessage.Append(Res.GetString("6834f88f-4e6d-4f94-8f65-940f82ef7e51", "There is no valid exchange rate for this currency for {0}.", currencyConverterDateForRate));
			noValidExRatesMessage.Append(GetAdviceHowToFixNoValidExchangeRates());
			return noValidExRatesMessage.ToString();
		}

		public virtual string ExchangeRateOutOfRangeWarningMessage(string currencyConverterDateForRate, string currencyCode, string foundRateDate)
		{
			ZStringBuilder message = new ZStringBuilder();
			message.Append(Res.GetString("f3eb4d22-4d5b-4419-998c-210daf6d9f31", "There is no exchange rate in the database for the valuation date {0}.\r\nThe closest match for exchange rate for {1} is the rate for the date {2}.", currencyConverterDateForRate, currencyCode, foundRateDate));
			message.Append(GetAdviceHowToFixNoValidExchangeRates());
			return message.ToString();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1066:DoNotUseGoToCaseOrDefault", Justification = "Baseline")]

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Regex Pattern for ASCII, plain text")]
		public static void CheckForIllegalCharacters(ZPropertyInfo info, bool reportCRandTAB = true)
		{
			var value = (ZString)info.Value;
			string pattern = string.Format(CultureInfo.InvariantCulture, "[{0}{1}]", @"^0-9a-zA-Z ~`!@#$%^&*()+=<>?,./:;""'\-\\\[\]\{\}", reportCRandTAB ? "" : @"\n\r\t");

			var matches = System.Text.RegularExpressions.Regex.Matches(value, pattern);
			if (matches.Count > 0)
			{
				var values = new System.Collections.Generic.List<String>();

				var maxMatches = matches.Count < 7 ? matches.Count : 7;
				for (int i = 0; i < maxMatches; i++)
				{
					var val = matches[i].Value;
					if (reportCRandTAB)
					{
						switch (val)
						{
							case "\r":
								if (i != 0 && matches[i - 1].Value == "\n")
								{
									continue;
								}
								goto case "\n";
							case "\n":
								if (i != 0 && matches[i - 1].Value == "\r")
								{
									continue;
								}
								val = (NoResString)"Enter";
								break;
							case "\t":
								val = (NoResString)"Tab";
								break;
						}
					}
					values.Add(string.Format(CultureInfo.InvariantCulture, "[{0}] {1}", matches[i].Index + 1, val));
				}

				if (values.Count > 0)
				{
					var matchesText = String.Join(", ", values);

					if (maxMatches < matches.Count)
					{
						matchesText += " ...";
					}

					info.AddWarning(Res.GetString("db998044-2031-41bb-b72a-184253a7381a", "Contains illegal Characters: {0}", matchesText));
				}
			}
		}

		protected virtual void AddExchangeRateOutOfRangeError(ZPropertyInfo currInfo, ZString message)
		{
			currInfo.AddWarning(message);
		}

		protected virtual bool ValidateEachWorkingDayHasAnExchangeRate
		{
			get { return false; }
		}

		protected internal virtual string GetAdviceHowToFixNoValidExchangeRates()
		{
			return "\r\n" + Res.GetString("aea82501-ad01-4205-b2a8-277c876d15ef", "Please report missing exchange rate data to your system administrator.");
		}

		readonly BusinessObject businessObject;
	}
}
