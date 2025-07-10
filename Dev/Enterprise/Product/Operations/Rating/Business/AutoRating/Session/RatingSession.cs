using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Rating.Business
{
	public static class _Rating
	{
		[ThreadStatic]
		static GlobalRatingSession session;

		public static bool IsOn
		{
			get { return session != null; }
		}

		/// <summary>
		/// Class to represent a modal UI element that is suspending a current rating run to ask for user input.
		/// Under rare circumstances, probably due to a RemoteApp bug, the user is able to interact
		/// with the job window when it should be in the background, blocked by the modal UI.
		/// If they try and run autorating again, this interface is called to handle the situation.
		/// </summary>
		public interface ISuspendOwner
		{
			void HandleAttemptToCreateNestedSession();
		}

		public static IDisposable Suspend(ISuspendOwner suspender)
		{
			return new DisposableAction(
				() => activeSuspender = suspender,
				() => activeSuspender = null);
		}

		public static bool IsSuspended
			=> activeSuspender != null;

		[ThreadStatic]
		static ISuspendOwner activeSuspender;

		public static RatingSupporterSession LocalSession
		{
			get { return RatingCache.LocalSession; }
		}

		public static bool IsBeingCopiedFromQuote(JobCharge charge)
		{
			return LocalSession != null && LocalSession.IsBeingCopiedFromQuote(charge);
		}

		public static void MarkAsBeingCopiedFromQuote(JobCharge charge)
		{
			if (LocalSession != null)
			{
				LocalSession.MarkAsBeingCopiedFromQuote(charge);
			}
		}

		public static IDisposable Start(IAutoRatingGUIInteractor interactor, AutoRateOptions options = default, bool isEqualization = false, bool isSaveCalledManuallyAfterSession = false, BillingType billingType = BillingType.Default)
		{
			if (session == null)
			{
				return session = new GlobalRatingSession(interactor, options, isEqualization, isSaveCalledManuallyAfterSession, billingType);
			}

			if (activeSuspender != null)
			{
				activeSuspender.HandleAttemptToCreateNestedSession();
				throw new AutoRater.RatingCancelledException();
			}

			throw new InvalidOperationException("Not allowed to run more than one RatingSession at once.");
		}

		public static IDisposable StartSubSession(IBusiness ratingSupporter)
		{
			return RatingCache.StartRatingSession(ratingSupporter);
		}

		public static IDisposable StartCost()
		{
			if (session != null && !session.RatingCost && !session.RatingSell)
			{
				return session.Set(CostSell.Cost);
			}

			throw new InvalidOperationException();
		}

		public static IDisposable StartSell()
		{
			if (session != null && !session.RatingCost && !session.RatingSell)
			{
				return session.Set(CostSell.Revenue);
			}

			throw new InvalidOperationException();
		}

		public static bool Cost
		{
			get { return session != null && session.RatingCost; }
		}

		public static bool Sell
		{
			get { return session != null && session.RatingSell; }
		}

		public static CostSell CostOrSell
		{
			get
			{
				if (Cost)
				{
					return CostSell.Cost;
				}

				if (Sell)
				{
					return CostSell.Revenue;
				}

				throw new InvalidOperationException();
			}
		}

		public static bool IsEqualization
		{
			get { return session != null && session.IsEqualization; }
		}

		public static EqualizationInfo EqualizationInfo
		{
			get
			{
				if (session == null)
				{
					throw new InvalidOperationException("Session is not available");
				}

				return session.EqualizationInfo;
			}
		}

		public static bool CanCreateInvalidJobConsolCosts
		{
			get { return session != null && session.IsSaveCalledManuallyAfterSession && !session.IsEqualization; }
		}

		/// <summary>
		/// Save is called manually when autorating is run from the UI and the user gets to review and edit the results before manually saving.
		/// This is false when autorating is run from a workflow trigger or other background task where save will be called automatically at the end.
		/// </summary>
		public static bool IsSaveCalledManuallyAfterSession
			=> session?.IsSaveCalledManuallyAfterSession ?? false;

		public static IAutoRatingGUIInteractor Interactor
		{
			get
			{
				if (session != null)
				{
					return session.Interactor;
				}

				throw new InvalidOperationException();
			}
		}

		public static bool ExcludeConsolLevelChargesOnCosting => session?.Options.ExcludeConsolLevelChargesOnCosting ?? false;

		public static BillingType BillingType
		{
			get
			{
				if (session == null)
				{
					return BillingType.Default;
				}

				return session.BillingType;
			}
		}

		class GlobalRatingSession : IDisposable
		{
			public GlobalRatingSession(IAutoRatingGUIInteractor interactor, AutoRateOptions options, bool isEqualization, bool isSaveCalledManuallyAfterSession, BillingType billingType)
			{
				Interactor = interactor;
				Options = options;
				IsEqualization = isEqualization;
				if (isEqualization)
				{
					equalizationInfo = new EqualizationInfo();
				}
				IsSaveCalledManuallyAfterSession = isSaveCalledManuallyAfterSession;
				this.billingType = billingType;
			}

			public bool RatingCost
			{
				get { return costSell == CostSell.Cost; }
			}

			public bool RatingSell
			{
				get { return costSell == CostSell.Revenue; }
			}

			public IDisposable Set(CostSell costOrSell)
			{
				return new SessionMode(this, costOrSell);
			}

			public bool IsEqualization { get; }

			public EqualizationInfo EqualizationInfo
			{
				get { return equalizationInfo; }
			}

			public IAutoRatingGUIInteractor Interactor { get; }

			public AutoRateOptions Options { get; }

			public bool IsSaveCalledManuallyAfterSession { get; }

			CostSell? costSell;
			EqualizationInfo equalizationInfo;

			public BillingType BillingType
			{
				get { return billingType; }
			}

			readonly BillingType billingType;

			public void Dispose()
			{
				session = null;
				equalizationInfo = null;
			}

			class SessionMode : IDisposable
			{
				public SessionMode(GlobalRatingSession parentSession, CostSell costOrSell)
				{
					this.parentSession = parentSession;
					parentSession.costSell = costOrSell;
				}

				readonly GlobalRatingSession parentSession;

				public void Dispose()
				{
					parentSession.costSell = null;
				}
			}
		}
	}
}
