namespace Enterprise.Warehouse.Web.WebService
{
	public class TransferPutawayWebServiceResponse : WebServiceResponse
	{
		public TransferPutawayWebServiceResponse()
			: base()
		{
		}

		public bool IsFullPalletIDTransferred { get; set; }

		public bool IsSingleProductTransferred { get; set; }

		public bool IsValidSourcePalletID { get; set; }

		public bool IsValidDestPalletID { get; set; }

		public bool IsValidProduct { get; set; }

		public bool IsValidDestLocation { get; set; }

		public bool IsAllTransferLinesTransferredOrFinalised { get; set; }

		public bool IsValidInventoryHeldCode { get; set; }

		public bool IsValidAttributes { get; set; }

		public decimal TotalQuantityAvailableForPutaway { get; set; }

		public decimal TotalQuantityTransferred { get; set; }
	}
}
