using System.Collections.Generic;

using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService
{
	public class WhsPalletIDNeutralWebServiceResponse : WebServiceResponse
	{
		public WhsPickLineInfo NewPickLine { get; set; }
		public List<string> CompletePalletPickingPallets { get; set; }
	}
}
