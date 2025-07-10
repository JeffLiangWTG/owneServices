using System;

namespace Enterprise.Warehouse.Web.WebService
{
	public class WhsLocationWebServiceResponse : WebServiceResponse
	{
		#region Constructors

		public WhsLocationWebServiceResponse()
			: base()
		{
			Location = "";
			LocationUserFriendly = "";
			LocationFormattedCheckDigit = "";

			LocationPK = Guid.Empty;
		}

		#endregion

		#region Properties

		public string Location { get; set; }

		public string LocationUserFriendly { get; set; }

		public string LocationFormattedCheckDigit { get; set; }

		public Guid LocationPK { get; set; }

		public bool IsFixed { get; set; }

		public bool IsVoidLocation { get; set; }

		public decimal QuantityLeftUntilFull { get; set; }

		public bool IsDockDoorLocation { get; set; }

		public bool IsPackingStation { get; set; }

		public bool IsPackingConsolidation { get; set; }

		public bool HasPalletSpaces { get; set; }

		#endregion
	}
}
