using System;

namespace Enterprise.Warehouse.Cartonisation.Business
{
	public class CartonisableItem
	{
		public CartonisableItem(CartonisableItemDefinition product)
		{
			ItemDefinition = product;
		}

		public CartonisableItemDefinition ItemDefinition { get; }

		public Guid OriginalPK { get; set; }
		public Guid PK { get; set; }
		public Guid LocationPK { get; set; }
		public decimal QTY
		{
			get => qty;
			set
			{
				qty = value;
				ItemWeight = qty * ItemDefinition.Weight;
				ItemVolume = qty * ItemDefinition.Volume;
			}
		}
		decimal qty;

		public decimal ItemWeight { get; private set; }
		public decimal ItemVolume { get; private set; }
	}
}
