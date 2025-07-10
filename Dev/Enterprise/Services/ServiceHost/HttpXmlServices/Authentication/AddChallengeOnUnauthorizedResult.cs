using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using CargoWise.Common.Interop;

namespace Enterprise.Services.ServiceHost
{
	public class AddChallengeOnUnauthorizedResult : IHttpActionResult
	{
		public AddChallengeOnUnauthorizedResult(AuthenticationHeaderValue challenge, IHttpActionResult innerResult)
		{
			this.Challenge = challenge;
			this.InnerResult = innerResult;
		}

		public AuthenticationHeaderValue Challenge { get; private set; }
		public IHttpActionResult InnerResult { get; private set; }

		public async Task<HttpResponseMessage> ExecuteAsync(CancellationToken cacellationToken)
		{
			HttpResponseMessage response;
			var stopWatch = new Stopwatch();
			try
			{
				stopWatch.Start();
				response = await InnerResult.ExecuteAsync(cacellationToken);
				stopWatch.Stop();
			}
			catch (HttpException ex) when (ex.HResult == HResult.E_FAIL)
			{
				stopWatch.Stop();
				throw new HttpException($"InnerResult Type: {InnerResult.GetType().ToString()}. Time Elapsed: {stopWatch.Elapsed.ToString("g")}.", ex);
			}

			if (response.StatusCode == HttpStatusCode.Unauthorized)
			{
				response.Headers.WwwAuthenticate.Add(Challenge);
			}

			return response;
		}
	}
}