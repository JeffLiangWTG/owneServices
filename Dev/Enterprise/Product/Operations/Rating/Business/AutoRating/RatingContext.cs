using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Integration.Rating;
using Enterprise.MasterFiles.Business.Rating.Services;
using Enterprise.Rating.Integration;
using Enterprise.Registry.Business;

namespace Enterprise.Rating.Business
{
	public class RatingContext : IRatingContext
	{
		public RatingContext(ILogger logger = null, IDialogService dialogService = null)
			: this(new LoggerDecorator(logger), new ReadOnlyBusinessObjectFactory() { NameForDebugging = "Rating Context" }, dialogService) // Debug Factory Name
		{
		}

		public RatingContext(LoggerDecorator loggerDecorator, BusinessObjectFactory factory, IDialogService dialogService)
			: this(loggerDecorator, factory, CreateCW1Provider(loggerDecorator, factory), CreateWRProviderIfAllowed(loggerDecorator, factory), dialogService)
		{
		}

		public RatingContext(LoggerDecorator logger, BusinessObjectFactory factory, CW1RatesProvider cw1RatesProvider, IUrsRatesProvider ursRatesProvider, IDialogService dialogService, bool isManualRateSelect = false)
		{
			Factory = factory;
			Logger = logger;
			UrsRatesProvider = ursRatesProvider;
			CW1Provider = cw1RatesProvider;
			var providers = new List<IRatesProvider>(2);
			if (cw1RatesProvider != null)
			{
				providers.Add(cw1RatesProvider);
			}
			if (ursRatesProvider != null)
			{
				providers.Add(ursRatesProvider);
			}
			RatesProvider = new AggregatedRatesProvider(providers.ToArray());
			IsManualCostSelectMode = isManualRateSelect;
			DialogService = dialogService;
			RateCalculationLogWrapper = new CalculationLogsWrapper();
		}

		public static RatingContext CreateForManualSelect(ILogger logger, IDialogService dialogService)
		{
			Argument.NotNull(dialogService, nameof(dialogService));

			var loggerDecorator = new LoggerDecorator(logger);
			var factory = new ReadOnlyBusinessObjectFactory();
			// Separate logger for providers since search logging is displayed separately
			var providerLogger = new ElementaryLogger();
			return new RatingContext(loggerDecorator, factory, CreateCW1Provider(providerLogger, factory), CreateWRProviderIfAllowed(providerLogger, factory), dialogService, true);
		}

		public static IUrsRatesProvider CreateWRProviderIfAllowed(ILogger logger, BusinessObjectFactory factory)
		{
			if (!DataRegistryRating.Instance.IsLicenseForRatesServiceValid(out string reason))
			{
				logger.Information(reason);
				return null;
			}

			if (!DataRegistryRating.Instance.IsRateServiceSubscriptionEnabled(out reason))
			{
				logger.Information(reason);
				return null;
			}

			if (RatingFeatureHelper.Urs.IsEnabled)
			{
				return new UrsRatesProvider(logger, ObjectFactory.Get<IUrsRatesClientFactory>(), factory);
			}

			return new WiseRatesProvider(factory, ObjectFactory.Get<IWiseRatesClientFactory>(), logger);
		}

		public static RatingContext CreateInstance(IFactoryProvider factoryProvider, LoggerDecorator logger = null, IDialogService dialogService = null, [CallerMemberName] string callerMethod = "")
		{
			var factory = factoryProvider?.Factory ?? new ReadOnlyBusinessObjectFactory { NameForDebugging = $"RatingContext Created from {callerMethod}" };
			logger = logger ?? new LoggerDecorator();

			return new RatingContext(logger, factory, dialogService);
		}

		static CW1RatesProvider CreateCW1Provider(ILogger logger, BusinessObjectFactory factory)
			=> new CW1RatesProvider(factory, logger);

		IUrsRatesProvider UrsRatesProvider { get; }
		CW1RatesProvider CW1Provider { get; }

		public LoggerDecorator Logger { get; }
		ILogger IRatingContext.Logger => Logger;
		public IDialogService DialogService { get; }
		public BusinessObjectFactory Factory { get; }
		public bool SearchForRatesMode { get; set; }
		public bool IsInRebateCalculationMode { get; set; }
		public string RawResponse => UrsRatesProvider?.LastRawResponse;

		[SuppressMessage("Microsoft.Design", "CA1006:Do not nest generic types in member signatures")]
		public Dictionary<ZGuid, HashSet<ZGuid>> PercentageLinesApplied { get; } = new Dictionary<ZGuid, HashSet<ZGuid>>();

		public bool IsManualCostSelectMode { get; }

		public IRatesProvider RatesProvider { get; }
		public IUrsRatesProvider ProviderUrsRates => UrsRatesProvider;
		public ICW1RatesProvider ProviderCW1Rates => CW1Provider;
		public List<AutoRateInfo> InterimAutoRatingResults { get; } = new List<AutoRateInfo>();
		public CalculationLogsWrapper RateCalculationLogWrapper { get; }
	}
}
