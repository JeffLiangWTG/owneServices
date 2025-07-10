using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.Integration;
using WiseRates.Api.Client;
using WiseRates.Api.Model;

namespace Enterprise.Rating.Business
{
	public interface IRatesProvider
	{
		IEnumerable<IRateEntry> GetCostRateEntries(RatingCriteria criteria);
		void ReconfigureLogger(ILogger newLogger);
	}

	#region ICW1RatesProvider

	public interface ICW1RatesProvider : IRatesProvider
	{
		IEnumerable<IRateEntry> GetCostRateEntries(RatingCriteria criteria, bool ignoreCachedRates);
	}

	#endregion

	#region IWiseRatesProvider

	public interface IWiseRatesProvider : IUrsRatesProvider
	{
		WiseRatesSearchRequestAsync BeginGetRawRates(RatingCriteria criteria, RatesQuery ratesQuery, int pageID);
		RatesSearchResponseDTO EndGetRawRates(WiseRatesSearchRequestAsync request);
	}

	public sealed class WiseRatesSearchRequestAsync : IDisposable
	{
		public WiseRatesSearchRequestAsync(IWiseRatesClient client, RatesSearchRequest request, string requestID, CancellationTokenSource cancelTokenSource)
		{
			Client = client;
			Request = request;
			RequestID = requestID;
			CancelTokenSource = cancelTokenSource;
		}

		public WiseRatesSearchRequestAsync(RatesSearchResponseDTO responseFromCache)
		{
			this.CachedResponseDTO = responseFromCache;
		}

		Task<RatesSearchResponse> task;
		public RatesSearchResponseDTO CachedResponseDTO;

		public IWiseRatesClient Client { get; }
		public RatesSearchRequest Request { get; }
		public string RequestID { get; }
		public CancellationTokenSource CancelTokenSource { get; }

		public void BeginRequest()
		{
			if (CachedResponseDTO == null)
			{
				task = Client.SearchAsync(Request, RequestID, CancelTokenSource.Token);
			}
		}

		/// <summary>
		/// Should only be called from WiseRatesProvider.GetResponse which has all the processing and logging
		/// </summary>
		public RatesSearchResponse GetResponse()
		{
			return task.GetAwaiter().GetResult();
		}

		public void Dispose()
		{
			Client?.Dispose();
			CancelTokenSource?.Dispose();
		}
	}

	#endregion

	#region IUrsRatesProvider

	public interface IUrsRatesProvider : IRatesProvider
	{
		string LastRawResponse { get; }
		ILogger Logger { get; }
	}
	
	#endregion
}
