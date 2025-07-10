using System;

namespace Enterprise.Warehouse.Cartonisation.Integration
{
	public interface ICartonisableItem
	{
		Guid PK { get; }
		Guid LocationPK { get; }

		decimal Quantity { get; }
		ICartonisableItemDefinition ItemDefinition { get; }
	}
}
