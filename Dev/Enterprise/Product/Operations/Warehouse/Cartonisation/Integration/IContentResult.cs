using System;

namespace Enterprise.Warehouse.Cartonisation.Integration
{
	public interface IContentResult
	{
		Guid CartonisableItemPK { get; }
		decimal Quantity { get; }
	}
}
