using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace Enterprise.Services.ServiceHost
{
	public sealed class UniversalXmlResult<T> : IHttpActionResult // TODO we can make this class service other entities than GateValidationResponse
	{
		public UniversalXmlResult(T result)
		{
			this.result = result;
		}

		readonly T result;

		public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
		{
			var responseMessage = new HttpResponseMessage(HttpStatusCode.OK);

			//TODO we should use DataObjectWriter
			var xml = $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" version=""1.1"">
   <Shipment>
      <DataContext>
         <DataTargetCollection>
            <DataTarget>
               <Type>GateBooking</Type>
            </DataTarget>
         </DataTargetCollection>
         <Company>
            <Code>XYZ</Code>
         </Company>
         <EnterpriseID>EDI</EnterpriseID>
         <ServerID>DAT</ServerID>
      </DataContext>
      <AddInfoCollection>
         <AddInfo>
            <Key>Result</Key>
            <Value>{result}</Value>
         </AddInfo>
      </AddInfoCollection>
   </Shipment>
</UniversalShipment>";

			responseMessage.Content = new StringContent(xml);
			responseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue("application/xml");

			return Task.FromResult(responseMessage);
		}
	}
}
