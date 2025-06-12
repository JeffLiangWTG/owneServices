using System.Configuration;
using System.IO;
using System.Net;
using NUnit.Framework;

namespace CargoWise.eHub.Products.GBCustoms.CNS.InboundMessageWebService.Tests
{
    [TestFixture]
    class InboundMessageHealthCheckTest
    {
        [Test, Ignore("Does not have setup, needs to get fixed")]
        public void TestCredentialWebServiceHealthCheck()
        {
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            var endPoint = ConfigurationManager.AppSettings["CargoWise.eServices.GBCustomsCNSNotificationWebService.HealthCheck.Http.Endpoint"];
            var request = HttpWebRequest.Create(endPoint);
            request.Credentials = CredentialCache.DefaultCredentials;

            using (var response = (HttpWebResponse)request.GetResponse())
            {
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                Assert.IsFalse(response.IsFromCache);
                Assert.That(response.ContentType, Is.EqualTo("text/plain; charset=utf-8"));

                using (var responseStream = response.GetResponseStream())
                {
                    using (var reader = new StreamReader(responseStream))
                    {
                        var result = reader.ReadToEnd();
                        Assert.That(result, Is.EqualTo("INFO(GBCustomsCNSNotificationWebService): Service is alive."));
                    }
                }
            }
        }
    }
}
