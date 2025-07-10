using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	public class WhsTransferLineCollectionWebServiceResponseTest : WebServiceResponseTestCase
	{
		#region TestLineInfoCollection

		public void TestLineInfoCollection()
		{
			AssertEquals("If no LineInfoCollection was set, should have created an empty one.", 0, Response.LineInfoCollection.Count);

			var lineInfoCollection = new WhsDocketLineInfoCollection();
			Response.LineInfoCollection = lineInfoCollection;
			AssertEquals(lineInfoCollection, Response.LineInfoCollection);
		}

		#endregion

		#region TestSimpleProperties

		public void TestSimpleProperties()
		{
			var response = new WhsTransferLineCollectionWebServiceResponse();
			response.TotalTransferLinesToLoad = 20;

			AssertEquals("TotalTransferLinesToLoad", 20, response.TotalTransferLinesToLoad);
		}

		#endregion

		#region Implementation

		protected new WhsTransferLineCollectionWebServiceResponse Response
		{
			get { return (WhsTransferLineCollectionWebServiceResponse)base.Response; }
		}

		protected override WebServiceResponse GetNewResponse()
		{
			return new WhsTransferLineCollectionWebServiceResponse();
		}

		#endregion
	}
}
