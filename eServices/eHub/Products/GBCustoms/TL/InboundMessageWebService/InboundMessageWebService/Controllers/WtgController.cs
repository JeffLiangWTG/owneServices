using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;

namespace CargoWise.eHub.Products.GBCustoms.TL.InboundMessageWebService.Controllers
{
    public class WtgController : ApiController
    {
	    [HttpHead]
		[HttpGet]
	    [Route("wtg/status")]
	    public HttpResponseMessage Status()
	    {
		    var documentText = new InboundMessageWebServiceHealthCheckHttpTaskAsyncHandler().GetDocumentText().Result;
		    var httpResponseMessage = Request.CreateResponse(HttpStatusCode.OK);
		    httpResponseMessage.Content = new StringContent(documentText, Encoding.UTF8, "text/plain");

		    return httpResponseMessage;
	    }
    }
}
