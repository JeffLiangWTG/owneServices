using System;
using System.Net.Http;
using System.Xml.Linq;
using Common.Logging;

namespace CargoWise.eHub.Products.GBCustoms.Core.InboundMessageWebService.Handlers
{
    public interface IMessageHandler
    {
	    string GetName();
	    HttpResponseMessage Process(HttpRequestMessage request, ILog logger, Guid ID, XDocument requestMessage, XDocument body = null, bool isTransportLayer = false, string stringBody = "");
    }
}
