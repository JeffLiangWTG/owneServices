using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Freight.OnlineSailingSchedules;

namespace Enterprise.Freight.GUI.OnlineSailingSchedules
{
	public class OnlineSailingSchedulesImportedEventArgs : EventArgs
	{
		public OnlineSailingSchedulesImportedEventArgs(IEnumerable<Route> importedRoutes)
		{
			ImportedRoutes = importedRoutes ?? Enumerable.Empty<Route>();
		}

		public readonly IEnumerable<Route> ImportedRoutes;
	}
}
