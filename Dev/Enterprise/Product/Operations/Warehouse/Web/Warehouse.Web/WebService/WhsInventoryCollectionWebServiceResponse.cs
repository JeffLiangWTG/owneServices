using System.Collections.Generic;
using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService
{
	public class WhsInventoryCollectionWebServiceResponse : WebServiceResponse
	{
		public WhsInventoryCollectionWebServiceResponse()
			: base()
		{
		}

		#region LineInfoCollection

		public WhsInventoryLineInfoCollection LineInfoCollection
		{
			get { return lineInfoCollection ?? (lineInfoCollection = new WhsInventoryLineInfoCollection()); }
			set { lineInfoCollection = value; }
		}

		WhsInventoryLineInfoCollection lineInfoCollection;

		#endregion

		#region AvailableInventorySerialNumbers

		public List<string> AvailableInventorySerialNumbers
		{
			get { return availableInventorySerialNumbers ?? (availableInventorySerialNumbers = new List<string>()); }
			set { availableInventorySerialNumbers = value; }
		}

		List<string> availableInventorySerialNumbers;

		#endregion

		#region TotalCount

		public int TotalCount { get; set; }

		#endregion
	}
}
