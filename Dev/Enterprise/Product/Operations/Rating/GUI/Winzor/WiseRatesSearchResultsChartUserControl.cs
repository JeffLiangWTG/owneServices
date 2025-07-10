using System.Collections.Generic;
using System.Linq;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.GUI
{
	public class WiseRatesSearchResultsChartUserControl : ZUserControl
	{
		public void DisplayResults(RatesSearchResponseDTO dataSource, IEnumerable<string> warnings)
		{
		}

		public void Reset()
		{
			DisplayResults(null, Enumerable.Empty<string>());
		}
	}
}
