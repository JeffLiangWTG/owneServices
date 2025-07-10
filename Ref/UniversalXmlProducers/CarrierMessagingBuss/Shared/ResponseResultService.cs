using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.CarrierMessagingBuss.Shared.Model;
using CargoWise.RefDbRepo.Common.Utils;

namespace CargoWise.RefDbRepo.CarrierMessagingBuss.Shared
{
	public class ResponseResultService<T>(
		IHttpWebHelper<ResponseResult<T[]>> httpWebHelper,
		IAccessTokenProvider tokenProvider) where T : class
	{
		private const string ApiPath = "api/Accessorial/accessorialInfo?pageNumber={0}";
		private readonly IHttpWebHelper<ResponseResult<T[]>> _httpWebHelper = httpWebHelper;
		private readonly IAccessTokenProvider _tokenProvider = tokenProvider;

		public async Task<IEnumerable<ResponseResult<T[]>>> GetRefAccessorialListAsync(AppConfiguration appConfiguration)
		{
			var token = _tokenProvider.GetAccessToken();
			var results = new List<ResponseResult<T[]>>();
			int pageNumber = 1;

			while (true)
			{
				var url = string.Format(CultureInfo.InvariantCulture, appConfiguration.WebServiceBaseUrl + ApiPath, pageNumber);
				var response = await _httpWebHelper.GetAsync(url, token).ConfigureAwait(false) ?? throw new InvalidOperationException("Api response is null.");

				if (!response.IsSuccess)
				{
					throw new InvalidOperationException($"Api response failed with message: {response.Message ?? "No error message provided."}");
				}

				if (response.Value == null ||  response.Value.Length == 0)
					break;

				results.Add(response);
				pageNumber++;
			}

			return results;
		}
	}
}
