using Common.Logging;
using System.Collections.Concurrent;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace CargoWise.eHub.Products.GBCustoms.CSP.OutboundWebService
{
	public static class ThreadHandler
	{
		internal static readonly ConcurrentDictionary<string, SemaphoreSlim> accountLock = new ConcurrentDictionary<string, SemaphoreSlim>();

		public static async Task<HttpResponseMessage> SendMessageThreadSafe(IMessageSender messageSender, ILog logger, string logPrefix, HttpRequestMessage httpRequestMessage, string accountName, string uri)
		{
			logger.Debug($"{logPrefix}Checking for unique thread for [{accountName}]");

			int timeout;
			if (!int.TryParse(ConfigurationManager.AppSettings["SemaphoreWait"], out timeout))
			{
				return HTTPHelper.CreateFailureResponse(logger, logPrefix, HttpStatusCode.InternalServerError, $"Account [{accountName}] - Could not find a valid value for the timeout of the semaphore");
			}

			var semaphore = accountLock.GetOrAdd(accountName, new SemaphoreSlim(1, 1));
			try
			{
				if (await semaphore.WaitAsync(timeout))
				{
					logger.Debug($"{logPrefix}Unique thread available for [{accountName}], sending message...");
					return await messageSender.SendMessage(logger, logPrefix, httpRequestMessage, uri);
				}
				else
				{
					return HTTPHelper.CreateFailureResponse(logger, logPrefix, HttpStatusCode.InternalServerError, $"Account [{accountName}] - Timeout while waiting for unique thread.");
				}
			}
			finally
			{
				semaphore.Release();
			}
		}
	}
}
