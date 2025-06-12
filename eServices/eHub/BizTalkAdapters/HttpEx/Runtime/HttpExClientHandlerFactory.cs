using System.Net.Http;

namespace CargoWise.eHub.BizTalkAdapters.HttpEx
{
	internal interface IHttpExClientHandlerFactory
	{
		WebRequestHandler CreateHandler();
	}

	internal class HttpExClientHandlerFactory : IHttpExClientHandlerFactory
	{
		public virtual WebRequestHandler CreateHandler()
		{
			return new WebRequestHandler();
		}
	}
}