namespace Enterprise.Warehouse.Integration
{
	public interface ITransitBookingInfo
	{
		bool IsValidationResultAccepted { get; }

		string DeclinedReason { get; }

		string ContainerNumber { get; }
		string ContainerISOCode { get; }
		bool IsLadenContainer { get; }

		string CargoReferenceNumber { get; }

		string CargoDescription { get; }

		int Quantity { get; }

		string[] CargoTypes { get; }

		string[] IMOCodes { get; }

		string UnitOfMeasurement { get; }

		decimal GrossVolume { get; }
		string GrossVolumeUnit { get; }

		decimal GrossWeight { get; }
		string GrossWeightUnit { get; }

		decimal Temperature { get; }
		string TemperatureUnit { get; }
	}
}
