using System.Collections.Generic;
using System.Diagnostics;

using CargoWise.EntityFramework;
using Enterprise.Integration.Accounting;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.Business
{
	[DebuggerDisplay("{DebuggerDisplayText}")]
	public class AutoRateResult
	{
		public AutoRateResult(BusinessObjectFactory factory, CostSell costOrSell)
		{
			this.factory = factory;
			this.costOrSell = costOrSell;
		}

		readonly BusinessObjectFactory factory;
		readonly CostSell costOrSell;

		public void CancelAutoRating(AutoRatingCancellation reason)
		{
			rateInfoCollection = null;
			RateInfoCollection.CancelAutoRating(reason);
		}

		public AutoRatingCancellation Cancellation
		{
			get { return RateInfoCollection.Cancellation; }
		}

		public bool HasAdditionalRates
		{
			get { return RatesGoingToExpire.Count > 0 || JustExpiredRates.Count > 0 || UnacceptedQuotes.Count > 0 || (RateInfoCollection.Count == 0 && PossibleMatchesWrapper.PossibleMatches.Count > 0); }
		}

		public bool UserCancelledAutoRating
		{
			get { return rateInfoCollection != null && rateInfoCollection.UserCancelledAutoRating; }
		}

		void PerformCleanupIfRatingIsCancelled()
		{
			if (UserCancelledAutoRating)
			{
				if (rateInfoCollection.Count != 0)
				{
					var cancellation = rateInfoCollection.Cancellation;
					rateInfoCollection = new AutoRateInfoCollection(factory);
					rateInfoCollection.CancelAutoRating(cancellation);
				}

				ratesGoingToExpire = null;
				justExpiredRates = null;
				unacceptedQuotes = null;
				possibleMatchesWrapper = null;
			}
		}

		public AutoRateInfoCollection RateInfoCollection
		{
			get
			{
				PerformCleanupIfRatingIsCancelled();
				return rateInfoCollection ?? (rateInfoCollection = new AutoRateInfoCollection(factory));
			}
		}
		AutoRateInfoCollection rateInfoCollection;

		public List<IRateEntry> RatesGoingToExpire
		{
			get
			{
				PerformCleanupIfRatingIsCancelled();
				return ratesGoingToExpire ?? (ratesGoingToExpire = new List<IRateEntry>());
			}
		}
		List<IRateEntry> ratesGoingToExpire;

		public List<IRateEntry> JustExpiredRates
		{
			get
			{
				PerformCleanupIfRatingIsCancelled();
				return justExpiredRates ?? (justExpiredRates = new List<IRateEntry>());
			}
		}
		List<IRateEntry> justExpiredRates;

		public List<IRatingHeader> UnacceptedQuotes
		{
			get
			{
				PerformCleanupIfRatingIsCancelled();
				return unacceptedQuotes ?? (unacceptedQuotes = new List<IRatingHeader>());
			}
		}
		List<IRatingHeader> unacceptedQuotes;

		public PossibleMatchesWrapper PossibleMatchesWrapper
		{
			get
			{
				PerformCleanupIfRatingIsCancelled();
				return possibleMatchesWrapper ??= new PossibleMatchesWrapper(costOrSell);
			}
		}
		PossibleMatchesWrapper possibleMatchesWrapper;

		#region DebuggerDisplay
		// ReSharper disable UnusedMember.Local

		string DebuggerDisplayText
		{
			get
			{
				if (!UserCancelledAutoRating)
				{
					return string.Format((NoResString)"Infos: {0}, GoingToExpire: {1}, Expired: {2}, Unaccepted: {3}, Possible: {4}", // DebuggerDisplayText
										RateInfoCollection.Count,
										RatesGoingToExpire.Count,
										JustExpiredRates.Count,
										UnacceptedQuotes.Count,
										PossibleMatchesWrapper.PossibleMatches.Count);
				}
				else
				{
					return (NoResString)"Cancellation Reason: " + Cancellation.Reason; // DebuggerDisplayText
				}
			}
		}

		// ReSharper restore UnusedMember.Local
		#endregion
	}
}
