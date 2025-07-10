using System;

namespace Enterprise.Warehouse.Cartonisation.Integration
{
	public interface ICartonDefinition
	{
		Guid PK { get; }
		decimal Height { get; }
		decimal Length { get; }
		decimal Width { get; }
		string DimensionUQ { get; }
		decimal Volume { get; }
		string VolumeUQ { get; }
		decimal MaxFillPercent { get; }
		decimal MaxNumberOfUnits { get; }
		decimal MaxWeight { get; }
		decimal EmptyWeight { get; }
		string WeightUQ { get; }
		decimal Cost { get; }
	}
}
