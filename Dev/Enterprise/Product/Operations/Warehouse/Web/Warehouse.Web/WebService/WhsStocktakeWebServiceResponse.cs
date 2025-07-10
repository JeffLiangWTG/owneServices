using System.Collections.Generic;
using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService
{
	public class WhsStocktakeWebServiceResponse : WebServiceResponse
	{
		#region Properties

		#region StocktakeHeader

		public WhsStocktakeInfo Stocktake { get; set; }

		#endregion

		#region LinesToCount

		public WhsStocktakeLineInfoCollection LinesToCount
		{
			get { return linesToCount ?? (linesToCount = new WhsStocktakeLineInfoCollection()); }
			set { linesToCount = value; }
		}
		WhsStocktakeLineInfoCollection linesToCount;

		#endregion

		#region LocationsToCount

		public List<string> LocationsToCount
		{
			get { return locationsToCount ?? (locationsToCount = new List<string>()); }
			set { locationsToCount = value; }
		}
		List<string> locationsToCount;

		#endregion

		#endregion

	}
}
