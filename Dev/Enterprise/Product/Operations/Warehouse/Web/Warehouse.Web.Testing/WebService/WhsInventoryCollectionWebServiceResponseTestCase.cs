using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	public class WhsInventoryCollectionWebServiceResponseTestCase : WebServiceResponseTestCase
	{
		#region Properties

		public void TestLineInfoCollection()
		{
			AssertEquals("If no LineInfoCollection was set, should have created an empty one.", 0, Response.LineInfoCollection.InventoryLineInfos.Count);

			var lineInfoCollection = new WhsInventoryLineInfoCollection();
			Response.LineInfoCollection = lineInfoCollection;
			AssertEquals(lineInfoCollection, Response.LineInfoCollection);
		}

		#endregion

		#region Implementation

		protected new WhsInventoryCollectionWebServiceResponse Response
		{
			get { return (WhsInventoryCollectionWebServiceResponse)base.Response; }
		}

		protected override WebServiceResponse GetNewResponse()
		{
			return new WhsInventoryCollectionWebServiceResponse();
		}

		#endregion
	}
}
