using System;
using System.Globalization;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;

namespace CargoWise.RefDbRepo.CNReferenceData.Services
{
	public class EChinaAPIProxy
	{
		public EChinaAPIProxy(IEChinaAPIConfig config, ILog logger)
		{
			Logger = logger;
			Config = config;
		}
		protected ILog Logger { get; set; }
		IEChinaAPIConfig Config { get; set; }

		IFlurlRequest GetRequestWithEChinaAppCode(string baseUrl, params (string name, string value)[] parameters)
		{
			return new FlurlRequest(GetUrlWithEChinaAppCode(baseUrl, parameters));
		}

		Url GetUrlWithEChinaAppCode(string baseUrl, params (string name, string value)[] parameters)
		{
			var url = new Url(baseUrl);
			url.SetQueryParam("appcode", Config.EChinaAppCode);
			foreach (var (name, value) in parameters)
			{
				url.SetQueryParam(name, value);
			}
			return url;
		}

		public virtual async Task<int> GetUpdatedCountTask()
		{
			var result = 0;

			CheckForUpdatesResponse checkResult = null;
			var request = GetRequestWithEChinaAppCode(Config.EChinaCheckForUpdateUrl);

			Logger.Info($"Checking for updates from {request.Url}...");

#pragma warning disable CA1031 // Do not catch general exception types
			try
			{
				checkResult = await request.GetJsonAsync<CheckForUpdatesResponse>();
			}
			catch (Exception ex)
			{
				Logger.Error($" Check for Update failes: {ex.Message}");
			}
#pragma warning restore CA1031 // Do not catch general exception types

			if (checkResult != null)
			{
				if (checkResult.IsSuccess() && checkResult.HaveIsUpdate != null)
				{
					Logger.Info($"Result Status: {checkResult.StateInfo()}");
					Logger.Info($"Count of updates: {checkResult.HaveIsUpdate.IS_UPDATE_COUNT}");

					result = checkResult.HaveIsUpdate.IS_UPDATE_COUNT;
				}
				else
				{
					Logger.Error($"Result Status: {checkResult.StateInfo()}");
				}
			}

			return result;
		}

		public virtual async Task<string> GetUpdateDataTask(DateTime startDate, DateTime endDate, int pageNumber, int pageSize)
		{
			var url = GetUrlWithEChinaAppCode(Config.EChinaGetUpdatesUrl,
				("updatedatastart", startDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)),
				("updatedataend", endDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)),
				("startnum", pageNumber.ToString(CultureInfo.InvariantCulture)),
				("searchcount", pageSize.ToString(CultureInfo.InvariantCulture))
			);

			Logger.Info($@"Getting updates from {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}, page {pageNumber}");
			Logger.Info($@"URL: {url}...");

			string result = null;

			using (var client = new FlurlClient(url))
			{
#pragma warning disable CA1031 // Do not catch general exception types
				try
				{
					result = await client.Request().GetStringAsync();
				}
				catch (Exception ex)
				{
					Logger.Error($"Error on getting response from Page {pageNumber}.", ex);
				}
#pragma warning restore CA1031 // Do not catch general exception types
			}
			return result;
		}
	}
}
