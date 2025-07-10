namespace Enterprise.Warehouse.Integration
{
	public interface ITWHValidationProvider
	{
		ITWHValidationResponse Get(ITWHValidationRequest request);
	}

	public interface ITWHPickupValidationProvider : ITWHValidationProvider { }

	public interface ITWHDeliveryValidationProvider : ITWHValidationProvider { }
}
