using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.ZArchitecture;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.Business
{
	public class AutoRater
	{
		#region AutoRate Execution

		public virtual AutoRateResult AutoRate(BusinessObjectFactory factory, IAutoRating itemToRate, CostSell costOrSell, IRatingContext ratingContext)
		{
			if (!itemToRate.StatusInformation.CanExecute)
			{
				return new AutoRateResult(factory, costOrSell);
			}

			var freightAutoRater = new FreightAutoRater(ratingContext);
			var autoRatingInfo = new AutoRatingProxy(itemToRate);
			var criteria = new RatingCriteria(autoRatingInfo, factory);

			if (ratingContext.IsInRebateCalculationMode)
			{
				return freightAutoRater.AutoRate(criteria, costOrSell);
			}

			if (OneOffQuoteSelected != null)
			{
				freightAutoRater.OneOffQuoteSelected += OneOffQuoteSelected;
			}

			var result = freightAutoRater.AutoRate(criteria, costOrSell, true);

			if (result.HasAdditionalRates && !PromptAdditionalRates(autoRatingInfo, new AdditionalRatesNotifications(result)))
			{
				result.CancelAutoRating(new AutoRatingCancellation(AutoRatingCancellation.Reasons.AdditionalRatesPrompt, string.Empty));
			}

			if (!result.UserCancelledAutoRating && costOrSell == CostSell.Revenue && criteria.LocalClient != null && !ratingContext.SearchForRatesMode)
			{
#pragma warning disable 0618
				var freightEntries = result.RateInfoCollection.Select(x => x.Entry).Where(x => x.IsFreightEntry());
				var emailFactory = _Rating.IsSaveCalledManuallyAfterSession ? null : factory;
				SendRevenueRelatedEmailNotifications(emailFactory, criteria, result.JustExpiredRates, freightEntries);
#pragma warning restore 0618
			}

			itemToRate.OnAutoRated(result.RateInfoCollection);

			return result;
		}

		public event EventHandler<RevenueRatesLoader.QuoteEventArgs> OneOffQuoteSelected;

		#endregion

		#region Notifications

		public static ZInt ExpiredRateNotificationDays
		{
			get { return Env.Registry.Rating.ExpiredRateNotificationPeriod; }
		}

		public static ZInt ExpiringRateNotificationDays
		{
			get { return Env.Registry.Rating.ExpiringRateNotificationPeriod; }
		}

		#region Unaccepted Quotes, Expired/Expiring Rates Event

		public class AdditionalRatesNotifications
		{
			public List<IRateEntry> RatesGoingToExpire { get { return fRatesGoingToExpire; } }
			public List<IRateEntry> JustExpiredRates { get { return fJustExpiredRates; } }
			public List<IRatingHeader> UnacceptedQuotes { get { return fUnacceptedQuotes; } }
			public PossibleMatchesWrapper PossibleMatchesWrapper { get { return fPossibleMatchesWrapper; } }

			public bool HasRatesGoingToExpire { get { return fRatesGoingToExpire != null && fRatesGoingToExpire.Count > 0; } }
			public bool HasJustExpiredRates { get { return fJustExpiredRates != null && fJustExpiredRates.Count > 0; } }
			public bool HasUnacceptedQuotes { get { return fUnacceptedQuotes != null && fUnacceptedQuotes.Count > 0; } }
			public bool HasPossibleMatches { get { return totalMatchesFound == 0 && fPossibleMatchesWrapper != null && fPossibleMatchesWrapper.PossibleMatches.Count > 0; } }

			public bool HasClientRatesGoingToExpire
			{
				get { return fRatesGoingToExpire.Any(entry => !entry.IsCompanyTariff()); }
			}

			public bool HasGlobalRatesGoingToExpire
			{
				get { return fRatesGoingToExpire.Any(x => x.IsCompanyTariff()); }
			}

			public bool HasClientJustExpiredRates
			{
				get { return fJustExpiredRates.Any(x => !x.IsCompanyTariff()); }
			}

			public bool HasGlobalJustExpiredRates
			{
				get { return fJustExpiredRates.Any(x => x.IsCompanyTariff()); }
			}

			internal AdditionalRatesNotifications(AutoRateResult autoRateResult)
			{
				fRatesGoingToExpire = autoRateResult.RatesGoingToExpire;
				fJustExpiredRates = autoRateResult.JustExpiredRates;
				fUnacceptedQuotes = autoRateResult.UnacceptedQuotes;
				fPossibleMatchesWrapper = autoRateResult.PossibleMatchesWrapper;
				totalMatchesFound = autoRateResult.RateInfoCollection.Count;
			}

			readonly List<IRateEntry> fRatesGoingToExpire;
			readonly List<IRateEntry> fJustExpiredRates;
			readonly List<IRatingHeader> fUnacceptedQuotes;
			readonly PossibleMatchesWrapper fPossibleMatchesWrapper;
			readonly int totalMatchesFound;
		}

		public delegate bool AdditionalRatesNotificationsHandler(IAutoRating itemToRate, AdditionalRatesNotifications additionalRatesNotifications);
		public event AdditionalRatesNotificationsHandler AdditionalRatesNotification;
		bool additionalRatesNotificationHasAlreadyBeenShown;

		bool PromptAdditionalRates(AutoRatingProxy autoRatingInfo, AdditionalRatesNotifications additionalRatesNotifications)
		{
			if (!additionalRatesNotificationHasAlreadyBeenShown && AdditionalRatesNotification != null)
			{
				additionalRatesNotificationHasAlreadyBeenShown = true;
				return AdditionalRatesNotification(autoRatingInfo, additionalRatesNotifications);
			}

			return true;
		}

		public void Reset()
		{
			additionalRatesNotificationHasAlreadyBeenShown = false;
		}

		#endregion

		#region Revenue Related Email Notification

		void SendRevenueRelatedEmailNotifications(BusinessObjectFactory emailFactory, RatingCriteria criteria, List<IRateEntry> justExpiredRates, IEnumerable<IRateEntry> freightEntries)
		{
			var emails = new List<RatingEmailDef>();

			var hasUsedFreightEntry = freightEntries.Any();
			if (hasUsedFreightEntry && freightEntries.Any(x => x.IsOneOffQuote()))
			{
				RatingEmailDef email = RatingEmailDef.OneOffQuoteUsed(criteria, freightEntries.First(x => x.IsOneOffQuote()).ParentRatingHeader);
				if (email != null && email.IsActive)
				{
					emails.Add(email);
				}
			}
			else
			{
				var justExpiredRatesNotification = false;
				foreach (var entry in justExpiredRates)
				{
					RatingEmailDef email;
					if (entry.IsCompanyTariff())
					{
						email = RatingEmailDef.CompanyTariffJustExpired(criteria, entry);
					}
					else
					{
						email = RatingEmailDef.ClientRateJustExpired(criteria, entry);
					}

					if (email != null && email.IsActive)
					{
						emails.Add(email);
						justExpiredRatesNotification = true;
						break;
					}
				}

				if (!justExpiredRatesNotification && !hasUsedFreightEntry && criteria.ChargeCodeGroups.Contains(ChargeCodeGroupList.Codes.Freight))
				{
					var party = criteria.ChargesPaidBy(null, null, ChargeCodeGroupList.Codes.Freight, CostSell.Revenue);
					if (party != ChargedParty.None && party != ChargedParty.Unknown)
					{
						RatingEmailDef email = RatingEmailDef.ClientRateNotFound(criteria, criteria.LocalClient);
						if (email != null && email.IsActive)
						{
							emails.Add(email);
						}
					}
				}
			}

			foreach (var email in emails)
			{
				SendEmail(emailFactory, email);
			}
		}

#if DEBUG
		public virtual
#endif
		bool SendEmail(BusinessObjectFactory factory, RatingEmailDef email)
		{
			return email.Send(factory);
		}

		#endregion

		#endregion

		#region Exceptions

		[Serializable]
		public class RatingCancelledException : AutoRaterException
		{
			//TODO: Move the usages from here into the other constructor
			// that requires a reason
			public RatingCancelledException(string errorMessage = null) : base(errorMessage ?? string.Empty) { }

			public RatingCancelledException(AutoRatingCancellation reason, string errorMessage = null) : base(errorMessage ?? string.Empty)
			{
				this.CancelReason = reason;
			}

#if NETFRAMEWORK
			public RatingCancelledException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
				: base(info, context)
			{
			}
#endif

			public AutoRatingCancellation CancelReason { get; }
		}

		/// <summary>
		/// Represents an autorating exception that occurs during a Calcuation of
		/// a single rate line.
		///
		/// If autorating with non-manual-selection, these exceptions will cause a halt of autorating.
		/// and a message box should be shown to the user.
		///
		/// If autorating with manual-selection, these exceptions can be handled by displaying
		/// the error message on the user interface directly against the charge which failed
		/// calculation.
		/// </summary>
		[Serializable]
		public class AutoRaterCalculationException : AutoRaterException
		{
			public AutoRaterCalculationException(string message) : this(message, message)
			{
			}

			public AutoRaterCalculationException(string message, string messageForManualSelect) : base(message)
			{
				MessageForManualSelect = messageForManualSelect;
			}

#if NETFRAMEWORK
			protected AutoRaterCalculationException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
			{
			}
#endif

			public virtual string MessageForManualSelect { get; protected set; }
		}

		[Serializable]
		public class CallForPriceException : AutoRaterCalculationException
		{
			public CallForPriceException(IRateLine line, RatingCriteria criteria, string message)
				: base(message)
			{
				var email = RatingEmailDef.CallForPricing(line, criteria, message);
				if (email != null && email.IsActive)
				{
					email.Send();
				}
			}

#if NETFRAMEWORK
			protected CallForPriceException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
				: base(info, context)
			{
			}
#endif
		}

		[Serializable]
		public class NoExchangeRateException : AutoRaterCalculationException
		{
			public NoExchangeRateException(GlbCompany company, params ICurrency[] currencies)
				: base(ErrorMessage)
			{
				var nonLocalCurrencies = new List<ICurrency>();
				foreach (var currency in currencies)
				{
					if (currency.Code != company.GC_RX_NKLocalCurrency)
					{
						nonLocalCurrencies.Add(currency);
					}
				}

				fCurrencies = nonLocalCurrencies.ToArray();
			}

#if NETFRAMEWORK
			public NoExchangeRateException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
				: base(info, context)
			{
			}
#endif

			readonly ICurrency[] fCurrencies;
			public ICurrency[] Currencies
			{
				get { return fCurrencies; }
			}

			static string ErrorMessage
			{
				get { return Res.GetString("3c5998b5-3b1a-423f-979d-4a6d8d2293ab", "Autorating cannot be performed as there are some rates in foreign currencies where no exchange rates have been specified."); }
			}
		}

		[Serializable]
		public class InvalidApplyToException : AutoRaterCalculationException
		{
			public InvalidApplyToException(IRateLineItem line)
				: base((NoResString)"InvalidApplyToException overrides the Message") // Text not visible to the user. It is overriden.
			{
				var parentRateLine = line.ParentRateLine;

				var chargeCode = parentRateLine.ChargeCode.AC_Code;
				var calculatorCode = parentRateLine.TL_RateCalculator;
				var ratingHeaderDescription = parentRateLine.ParentRateEntry.ParentRatingHeader.DisplayInfo();
				var applyTo = line.TM_Text;

				Message = Res.GetString("9decd7c7-898f-416b-903e-45d5767d14ac", "Cannot complete Auto-Rating as this Job has been matched to an invalid Rate Line. Please either correct the 'Apply to' field or delete the '{0}' Rate Line that uses a '{1}' Calculator with currently an Apply To of '{2}' on {3}.", chargeCode, calculatorCode, applyTo, ratingHeaderDescription);
				//Lowercase 't' on 'the charge' is intentional.
				MessageForManualSelect = Res.GetString("de6851b3-c51f-4030-b735-4058fddf3d51", "the charge '{0}' with a calculator '{1}' having an unexpected Apply To of '{2}' in {3}", chargeCode, calculatorCode, applyTo, ratingHeaderDescription);
			}

			public override string Message { get; }

#if NETFRAMEWORK
			protected InvalidApplyToException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
				: base(info, context)
			{
			}
#endif
		}

		#endregion
	}
}

