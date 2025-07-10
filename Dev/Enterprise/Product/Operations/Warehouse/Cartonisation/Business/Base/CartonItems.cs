using System.Collections.Generic;

namespace Enterprise.Warehouse.Cartonisation.Business
{
	class CartonItems
	{
		internal CartonItems(Carton carton, List<CartonisableItem> items)
		{
			Carton = carton;
			ItemsInList = items;
		}

		internal List<CartonisableItem> ItemsInList { get; }
		internal Carton Carton { get; }

		internal CartonItems Clone()
		{
			return new CartonItems(Carton, new List<CartonisableItem>(ItemsInList));
		}
	}
}
