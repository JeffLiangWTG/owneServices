using System;
using System.Web.Http;

namespace CargoWise.eHub.Products.TWCustoms.TWCServiceSampleClient
{
	public class MockServiceController : ApiController
    {
        [HttpPost]
        public async void PostMessage()
        {
            try
            {
                Program.mainForm.log($"Receiving message. lenght:{Request.Content.Headers.ContentLength}\r\n", null, null);
                var messageTask = Request.Content.ReadAsStringAsync();
                string message = await messageTask;
                Program.mainForm.log($"Message received. Content:\r\n{message}\r\n", null, null);
            }
            catch (Exception ex)
            {
                Program.mainForm.log($"Error receiving message.", null, ex);

            }
        }
    }
}
