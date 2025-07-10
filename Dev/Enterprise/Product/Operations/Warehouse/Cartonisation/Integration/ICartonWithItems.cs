using System;
using System.Collections.Generic;

namespace Enterprise.Warehouse.Cartonisation.Integration
{
	public interface ICartonWithItems
	{
		Guid CartonPK { get; }
		IEnumerable<IContentResult> Items { get; }
	}
}
