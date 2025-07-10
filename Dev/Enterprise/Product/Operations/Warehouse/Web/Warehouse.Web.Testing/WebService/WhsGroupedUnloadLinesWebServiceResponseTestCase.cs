using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	public class WhsGroupedDocketLinesWebServiceReponseTestCase : WebServiceResponseTestCase
	{
		#region Properties

		#region TestPalletID

		public void TestPalletID()
		{
			AssertNotNull(Response.PalletID);
			AssertEquals("", Response.PalletID);

			Response.PalletID = "1234";
			AssertEquals("1234", Response.PalletID);

			Response.PalletID = "4321";
			AssertEquals("4321", Response.PalletID);
		}

		#endregion

		#region TestGroupedLines

		public void TestGroupedLines()
		{
			AssertNotNull(Response.GroupedLines);
			AssertEquals(0, Response.GroupedLines.Count);

			var collection = Response.GroupedLines;
			AssertEquals(collection, Response.GroupedLines);

			Response.GroupedLines.Add(new WhsGroupedUnloadLineInfo());
			AssertEquals(1, Response.GroupedLines.Count);

			collection = new WhsGroupedUnloadLineInfoCollection();
			collection.Add(new WhsGroupedUnloadLineInfo());
			collection.Add(new WhsGroupedUnloadLineInfo());
			AssertNotEquals(collection, Response.GroupedLines);
			Response.GroupedLines = collection;
			AssertEquals(collection, Response.GroupedLines);
			AssertEquals(2, Response.GroupedLines.Count);
		}

		#endregion

		#endregion

		#region Implementation

		protected override WebServiceResponse GetNewResponse()
		{
			return new WhsGroupedUnloadLinesWebServiceResponse();
		}

		protected new WhsGroupedUnloadLinesWebServiceResponse Response
		{
			get { return (WhsGroupedUnloadLinesWebServiceResponse)base.Response; }
		}

		#endregion
	}
}
