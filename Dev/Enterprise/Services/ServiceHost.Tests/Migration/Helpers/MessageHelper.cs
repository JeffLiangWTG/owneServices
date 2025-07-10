using System.Net;

namespace Enterprise.Services.ServiceHost.Tests
{
	public static class MessageHelper
	{
#if NETFRAMEWORK
		public static string DecorateAsResponse(HttpStatusCode code, string message)
		{
			if (code == HttpStatusCode.BadRequest)
			{
				return $"{{\"Message\":\"{message}\"}}";
			}
			else
			{
				return $"\"{message}\"";
			}
		}
#elif NET
		public static string DecorateAsResponse(HttpStatusCode code, string message)
		{
			return message;
		}
#endif
	}
}
