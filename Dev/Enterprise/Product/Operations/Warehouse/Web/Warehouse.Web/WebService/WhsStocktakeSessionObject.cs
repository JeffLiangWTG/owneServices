using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService
{
	public class WhsStocktakeSessionObject
	{
		public WhsStocktakeSessionObject(WhsStocktakeInfo stocktake, WhsStocktakeLineInfoCollection linesToCount)
		{
			Stocktake = stocktake;
			LinesToCount = linesToCount;
		}

		#region Properties

		#region Stocktake

		public WhsStocktakeInfo Stocktake { get; private set; }

		#endregion

		#region LinesToCount

		public WhsStocktakeLineInfoCollection LinesToCount { get; private set; }

		#endregion

		#endregion

		#region GetLocationsToCount

		public List<string> GetLocationsToCount()
		{
			return LinesToCount.Select(l => l.LocationString).Distinct().ToList();
		}

		#endregion

		#region GetLinesToCountForLocation

		public WhsStocktakeLineInfoCollection GetLinesToCountForLocation(string location)
		{
			var result = new WhsStocktakeLineInfoCollection();
			result.AddRange(LinesToCount.Cast<WhsStocktakeLineInfo>().Where(l => l.LocationString == location));
			return result;
		}

		#endregion

		#region GetLine

		public WhsStocktakeLineInfo GetLine(Guid stocktakeLinePK)
		{
			return LinesToCount.Single(l => l.PK.Equals(stocktakeLinePK));
		}

		#endregion
	}
}
