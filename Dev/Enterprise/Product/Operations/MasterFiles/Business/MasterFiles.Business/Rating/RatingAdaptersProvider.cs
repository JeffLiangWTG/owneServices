using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.Rating;
using Enterprise.Rating.Integration;

namespace Enterprise.MasterFiles.Business
{
	public abstract class RatingAdaptersProvider : IRatingAdaptersProvider
	{
		/// <param name="uiInteractor">
		/// If not specified then the `DefaultInteractor` is used.
		/// </param>
		public List<IAutoRating> GetAdapters(IAutoRatingInteractor uiInteractor, AutoRateOptions options)
		{
			uiInteractor = uiInteractor ?? new DefaultInteractor();

			if (adapters == null)
			{
				adapters = new List<IAutoRating>();
				canExecuteAutoRating = true;

				var adaptersIncludingInvalid = GetAdaptersCore(uiInteractor, options);
				foreach (var adapter in adaptersIncludingInvalid)
				{
					AutoRatingStatusInfo status = adapter.StatusInformation;

					if (status != null && status.CanExecute)
					{
						if (status.IsActive)
						{
							if (!status.Message.IsEmpty)
							{
								if (!uiInteractor.YesNoWarning(status.Message))
								{
									canExecuteAutoRating = false;
									adapters.Clear();
									break;
								}
							}

							adapters.Add(adapter);
						}
					}
					else
					{
						if (status != null && !status.Message.IsEmpty)
						{
							uiInteractor.Log(LogType.Error, status.Message);
						}

						canExecuteAutoRating = false;
						adapters.Clear();
						break;
					}
				}
			}

			return adapters;
		}

		List<IAutoRating> adapters;

		public IQuickCalculateRating GetForQuickCalculate(IAutoRatingInteractor uiInteractor)
		{
			return GetForQuickCalculateCore(uiInteractor);
		}

		public virtual int AdaptersCount(AutoRateOptions options)
		{
			return GetAdapters(null, options).Count;
		}

		public virtual bool IsAdapterAvailable(AutoRateOptions options, ZString chargeCodeGroup, ZString chargeCodeSubGroup)
		{
			return GetAdapters(null, options).Any(adapter => adapter.JobServices.IsEnabled(chargeCodeGroup, chargeCodeSubGroup)
				&& adapter.StatusInformation.CanExecute);
		}

		public ReadOnlyCollection<IJobInvoicingPlugIn> GetAdditionalJobs()
		{
			return GetAdditionalJobsCore();
		}

		protected abstract ReadOnlyCollection<IJobInvoicingPlugIn> GetAdditionalJobsCore();

		public bool CanExecuteAutoRating(IAutoRatingInteractor uiInteractor, AutoRateOptions options)
		{
			GetAdapters(uiInteractor, options);
			return canExecuteAutoRating;
		}

		bool canExecuteAutoRating = true;

		public virtual bool NeedsHandleResult => false;

		public virtual bool HandleResult(IReadOnlyDictionary<IRatingAdapter, List<IAutoRatedCharge>> charges)
		{
			return false;
		}

		protected abstract List<IAutoRating> GetAdaptersCore(IAutoRatingInteractor uiInteractor, AutoRateOptions options);

		protected virtual IQuickCalculateRating GetForQuickCalculateCore(IAutoRatingInteractor uiInteractor)
		{
			var firstAdapter = GetAdapters(uiInteractor, AutoRateOptions.AutorateCosts).FirstOrDefault();
			return QuickCalculateRating.CreateIfNotNull(firstAdapter);
		}

		public virtual JobInvoicingConsumerType ConsumerType => null;

		public virtual ZString ConsumerTypeDescription
		{
			get { return ZString.Empty; }
		}

		public virtual AdaptersProviderOptions AdaptersProviderOptions
		{
			get { return AdaptersProviderOptions.Standard; }
		}

		public virtual bool IncludeChildrenProviders(CostSell costOrSell, BillingType billingType) => true;

		#region Session

		public IDisposable NewRatingSession()
		{
			return new Session(this);
		}

		void StartSession()
		{
			adapters = null;
		}

		void EndSession()
		{
			adapters = null;
		}

		class Session : IDisposable
		{
			public Session(RatingAdaptersProvider provider)
			{
				this.provider = provider;
				provider.StartSession();
			}

			readonly RatingAdaptersProvider provider;

			void IDisposable.Dispose()
			{
				provider.EndSession();
			}
		}

		#endregion

		#region DefaultInteractor

		class DefaultInteractor : IAutoRatingInteractor
		{
			public bool YesNoWarning(string message)
			{
				return true;
			}

			public void Log(LogType type, string message) { }

			public void Log(LogType type, string message, Exception ex) { }
		}

		#endregion

		#region Types

		public static class LogMessages
		{
			[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", Justification = "All the arguments are strings so no need for IFormatProvider")]
			public static string RatingAdaptersCannotBeCreated(params string[] reasons)
			{
				var reasonsJoined = string.Join("\r\n", reasons);

				return $"Please ensure the minimum criteria required to use Autorating / Quick Calculator has been saved on the job:\r\n{reasonsJoined}";
			}
		}

		#endregion
	}

	public class QuickCalculateRating : IQuickCalculateRating
	{
		/// <summary>
		/// Returns a new IQuickCalculateRating if the given IAutoRating is not null, or null otherwise.
		/// </summary>
		public static IQuickCalculateRating CreateIfNotNull(IAutoRating autoRating)
			=> autoRating != null ? new QuickCalculateRating(autoRating) : null;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="autoRating">IAutoRating instance to quick calculate. Must not be null</param>
		/// <param name="quickMeasures">If null then IAutoRating.RateableMeasures will be used as the source of measures.
		/// If not null then used as the source of measures instead.
		/// Useful when the measures for rating are not exactly the same as those for Quick Calculate.</param>
		public QuickCalculateRating(IAutoRating autoRating, IRateableMeasureSet quickMeasures = null)
		{
			Argument.NotNull(autoRating, nameof(autoRating));
			this.autoRating = autoRating;
			this.quickMeasures = quickMeasures;
		}
		readonly IAutoRating autoRating;

		public IRateableMeasureSet QuickMeasures => quickMeasures ?? (quickMeasures = autoRating.RateableMeasures);
		IRateableMeasureSet quickMeasures;

		public OrgHeader Carrier => autoRating.Carrier;
		public IBusiness AutoRatedFor => autoRating.AutoRatedFor?.FirstOrDefault();
		public AdapterType AdapterType => autoRating.AdapterType;
		public ZString OperationalJobCode => autoRating.OperationalJobCode;
		public BusinessObject HostBusinessObject => autoRating as BusinessObject;
		public AutoRatingStatusInfo StatusInformation => autoRating.StatusInformation;

#if DEBUG
		public IAutoRating RatingAdapter_ForTestOnly => autoRating;
#endif
	}

	public abstract class RatingOrCostingAdaptersProvider<T> : RatingAdaptersProvider
		where T : BusinessObject, IRatingSupporter, IBillingPlugin
	{
		protected RatingOrCostingAdaptersProvider(T parent, JobInvoicingConsumerType consumerType)
		{
			Argument.NotNull(parent, "parent");
			ParentRatingSupporter = parent;
			this.consumerType = consumerType;
		}

		public override JobInvoicingConsumerType ConsumerType => consumerType;

		public override ZString ConsumerTypeDescription
		{
			get { return consumerType != null ? consumerType.Description : string.Empty; }
		}

		protected readonly T ParentRatingSupporter;
		readonly JobInvoicingConsumerType consumerType;

		protected virtual List<IAutoRating> GetAdapters(T parent, IAutoRatingInteractor uiInteractor, AutoRateOptions options)
		{
			return new List<IAutoRating>();
		}

		protected override sealed List<IAutoRating> GetAdaptersCore(IAutoRatingInteractor uiInteractor, AutoRateOptions options)
		{
			return GetAdapters(ParentRatingSupporter, uiInteractor, options) ?? new List<IAutoRating>();
		}

		protected virtual IQuickCalculateRating GetForQuickCalculate(T parent, IAutoRatingInteractor uiInteractor)
		{
			var firstAdapter = GetAdapters(parent, uiInteractor, AutoRateOptions.AutorateCosts).FirstOrDefault();
			return QuickCalculateRating.CreateIfNotNull(firstAdapter);
		}

		protected sealed override IQuickCalculateRating GetForQuickCalculateCore(IAutoRatingInteractor uiInteractor)
		{
			return GetForQuickCalculate(ParentRatingSupporter, uiInteractor);
		}

		protected override sealed ReadOnlyCollection<IJobInvoicingPlugIn> GetAdditionalJobsCore()
		{
			return GetAdditionalJobs(ParentRatingSupporter);
		}

		protected virtual ReadOnlyCollection<IJobInvoicingPlugIn> GetAdditionalJobs(T parent)
		{
			return new ReadOnlyCollection<IJobInvoicingPlugIn>(new List<IJobInvoicingPlugIn>());
		}
	}

	public abstract class RatingAdaptersProvider<T> : RatingOrCostingAdaptersProvider<T>
		where T : BusinessObject, IRatingSupporter, IJobInvoicingPlugIn
	{
		protected RatingAdaptersProvider(T parent)
			: base(parent, parent != null && parent.InvoicingSupporter != null ? parent.InvoicingSupporter.ConsumerType : null) { }
	}
}
