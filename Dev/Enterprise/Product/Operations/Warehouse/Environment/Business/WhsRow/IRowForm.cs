using System.Collections.Generic;

namespace Enterprise.Warehouse.Environment.Business
{
	public interface IRowForm
	{
		List<WhsLocation> SelectedLocations { get; }
	}
}
