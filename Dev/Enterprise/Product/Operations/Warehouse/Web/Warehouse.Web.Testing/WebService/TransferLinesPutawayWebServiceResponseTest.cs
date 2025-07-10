using System.Linq;
using CargoWise.Types;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	public class TransferLinesPutawayWebServiceResponseTest : WebServiceResponseTestCase
	{
		protected override void TestPropertiesCore()
		{
			var response = new TransferLinesPutawayWebServiceResponse();
			AssertNotNull(response.NonFinalisedTransferLinesPks);

			var guid = ZGuid.NewZGuid().ToGuid();
			response.NonFinalisedTransferLinesPks.Add(guid);
			AssertEquals(guid, response.NonFinalisedTransferLinesPks.Single());
		}

		#region Implementation

		protected override WebServiceResponse GetNewResponse()
		{
			return new TransferLinesPutawayWebServiceResponse();
		}

		protected new TransferLinesPutawayWebServiceResponse Response
		{
			get { return (TransferLinesPutawayWebServiceResponse)base.Response; }
		}

		#endregion
	}
}
