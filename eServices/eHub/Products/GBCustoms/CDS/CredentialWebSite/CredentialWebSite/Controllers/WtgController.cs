using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;

namespace CargoWise.eHub.Products.GBCustoms.CDS.CredentialWebSite
{
    public class WtgController : ApiController
    {
        [HttpGet]
        [HttpHead]
        public HttpResponseMessage Status()
        {
            var documentText = new GBCustomsCredentialWebSiteHealthCheckHttpTaskAsyncHandler().GetDocumentText().Result;
            var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(documentText, Encoding.UTF8, "text/plain")
            };

            return httpResponseMessage;
        }
    }
}