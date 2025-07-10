using System;

namespace Enterprise.Warehouse.Web.WebService
{
	public class WhsInventoryWebServiceResponse : WebServiceResponse
	{
		#region Constructors

		public WhsInventoryWebServiceResponse()
			: base()
		{
		}

		#endregion

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		public ErrorInfo[] InventoryErrorInfos { get; set; }
		public Guid InventoryLinePK { get; set; }
	}
}
