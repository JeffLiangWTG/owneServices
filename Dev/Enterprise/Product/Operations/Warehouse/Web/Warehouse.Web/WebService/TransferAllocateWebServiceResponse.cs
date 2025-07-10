using System;
using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService
{
	public class TransferAllocateWebServiceResponse : WebServiceResponse
	{
		public TransferAllocateWebServiceResponse()
			: base()
		{
		}

		public WhsDocketInfo Transfer { get; set; }

		public WhsDocketLineInfoCollection NewTransferLines { get; set; }

		public Guid[] PickedTransferLinePks { get; set; }

		public bool IsValidLocation { get; set; }

		public bool IsValidPalletID { get; set; }

		public bool IsValidProduct { get; set; }

		public bool IsValidInventoryHeldCode { get; set; }

		public bool IsValidAttributes { get; set; }

		public decimal TotalQuantityAvailableToPick { get; set; }

		public decimal TotalPickLineQuantity { get; set; }

		public bool ShowStockOnHandWarningOnPutaway { get; set; }
	}
}
