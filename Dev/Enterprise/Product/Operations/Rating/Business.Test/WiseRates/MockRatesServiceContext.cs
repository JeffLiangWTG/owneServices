using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Integration.Rating;
using Enterprise.MasterFiles.Business.Rating.Services;
using Enterprise.Rating.Integration;
using Moq;
using WiseRates.Api.Client;
using RatesSearchRequest = WiseRates.Api.Model.RatesSearchRequest;
using RatesSearchResponse = WiseRates.Api.Model.RatesSearchResponse;

namespace Enterprise.Rating.Business.Testing
{
	public class MockRatesServiceContext : IRatingContext
	{
		public MockRatesServiceContext(RatesSearchResponse ratesServiceResponse, bool useCW1RatesProvider = false, TestInteractor testInteractor = null)
		{
			var clientMock = new Mock<IWiseRatesClient>();
			clientMock
				.Setup(m => m.SearchAsync(
					It.IsAny<RatesSearchRequest>(),
					It.IsAny<string>(),
					It.IsAny<CancellationToken>()))
				.Returns(Task.FromResult(ratesServiceResponse));

			SetupContext(clientMock.Object, useCW1RatesProvider, testInteractor);
		}

		public MockRatesServiceContext(IWiseRatesClient mockClient, bool useCW1RatesProvider = false)
		{
			SetupContext(mockClient, useCW1RatesProvider);
		}

		void SetupContext(IWiseRatesClient clientMock, bool useCW1RatesProvider, TestInteractor testInteractor = null)
		{
			if (testInteractor == null)
			{
				TestLogger = new TestLogger();
				Logger = new LoggerDecorator(TestLogger);
			}
			else
			{
				TestInteractor = testInteractor;
				Logger = new LoggerDecorator(testInteractor);
			}
			Factory = new BusinessObjectFactory();
			var clientFactoryMock = new Mock<IWiseRatesClientFactory>();
			clientFactoryMock
				.Setup(m => m.TryCreate(
					It.IsAny<string>(),
					It.IsAny<int>(),
					It.IsAny<CancellationToken>(),
					It.IsAny<ILogger>()))
				.Returns((clientMock, string.Empty));

			ProviderUrsRates = new WiseRatesProvider(Factory, clientFactoryMock.Object, Logger);
			var providers = new List<IRatesProvider>
			{
				ProviderUrsRates
			};

			if (useCW1RatesProvider)
			{
				ProviderCW1Rates = new CW1RatesProvider(Factory, new DummyLogger());
				providers.Add(ProviderCW1Rates);
			}
			else
			{
				var cw1MockProvider = new Mock<ICW1RatesProvider>();
				cw1MockProvider.Setup(m => m.GetCostRateEntries(It.IsAny<RatingCriteria>(), It.IsAny<bool>())).Returns(Enumerable.Empty<IRateEntry>());
				ProviderCW1Rates = cw1MockProvider.Object;
			}

			RatesProvider = new AggregatedRatesProvider(providers.ToArray());
		}

		public TestLogger TestLogger { get; private set; }
		public TestInteractor TestInteractor { get; private set; }
		public ILogger Logger { get; private set; }
		public IDialogService DialogService { get; }
		public BusinessObjectFactory Factory { get; private set; }
		public bool SearchForRatesMode { get; set; }
		public bool IsInRebateCalculationMode { get; set; }
		public Dictionary<ZGuid, HashSet<ZGuid>> PercentageLinesApplied { get; } = new Dictionary<ZGuid, HashSet<ZGuid>>();
		public string RawResponse { get; set; }
		public bool IsManualCostSelectMode { get; set; }

		public IRatesProvider RatesProvider { get; set; }

		public IUrsRatesProvider ProviderUrsRates { get; set; }
		public ICW1RatesProvider ProviderCW1Rates { get; set; }
		public List<AutoRateInfo> InterimAutoRatingResults { get; } = new List<AutoRateInfo>();
		public CalculationLogsWrapper RateCalculationLogWrapper { get; } = new CalculationLogsWrapper();
	}
}
