using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService
{
	public class WhsTransferLineCollectionWebServiceResponse : WebServiceResponse
	{
		public WhsTransferLineCollectionWebServiceResponse()
			: base()
		{
		}

		#region LineInfoCollection

		public WhsDocketLineInfoCollection LineInfoCollection
		{
			get { return lineInfoCollection ?? (lineInfoCollection = new WhsDocketLineInfoCollection()); }
			set { lineInfoCollection = value; }
		}

		WhsDocketLineInfoCollection lineInfoCollection;

		#endregion

		#region TotalTransferLinesToLoad

		public int TotalTransferLinesToLoad
		{
			get;
			set;
		}

		#endregion
	}
}
