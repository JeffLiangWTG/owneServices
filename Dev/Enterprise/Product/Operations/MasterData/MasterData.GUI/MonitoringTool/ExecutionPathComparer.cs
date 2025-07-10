using System;
using System.Collections.Generic;
using System.Globalization;

namespace Enterprise.MasterData.GUI
{
	class ExecutionPathComparer : IComparer<string>
	{
		public int Compare(string x, string y)
		{
			int xCounter = Convert.ToInt32(x.Substring(x.LastIndexOf('_') + 1), CultureInfo.InvariantCulture);
			int yCounter = Convert.ToInt32(y.Substring(y.LastIndexOf('_') + 1), CultureInfo.InvariantCulture);

			return xCounter.CompareTo(yCounter);
		}
	}
}
