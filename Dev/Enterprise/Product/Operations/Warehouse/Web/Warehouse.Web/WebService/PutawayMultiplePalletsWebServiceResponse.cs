using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService
{
	public class PutawayMultiplePalletsWebServiceResponse : WebServiceResponse
	{
		public PutawayMultiplePalletsWebServiceResponse()
			: base()
		{
		}

		#region PalletInfos

		public PutawayPalletInfo[] PalletInfos
		{
			get;
			set;
		}

		#endregion

		#region ShowStockOnHandWarningOnPutaway

		public bool ShowStockOnHandWarningOnPutaway { get; set; }

		#endregion
	}
}
