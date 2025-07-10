using System;
using System.Collections.Generic;
using Enterprise.Warehouse.Cartonisation.Integration;

namespace Enterprise.Warehouse.Cartonisation.Business
{
	public class CartonWithItems : ICartonWithItems
	{
		public Guid CartonPK { get; set; }
		public IEnumerable<IContentResult> Items { get; set; }
	}
}
