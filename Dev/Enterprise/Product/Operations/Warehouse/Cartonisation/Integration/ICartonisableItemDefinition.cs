using System;

namespace Enterprise.Warehouse.Cartonisation.Integration
{
	public interface ICartonisableItemDefinition
	{
		Guid PK { get; }
		decimal Height { get; }
		decimal Length { get; }
		decimal Width { get; }
		string DimensionUQ { get; }
		decimal Volume { get; }
		string VolumeUQ { get; }
		decimal Weight { get; }
		string WeightUQ { get; }
		bool KeepUpright { get; }
	}
}
