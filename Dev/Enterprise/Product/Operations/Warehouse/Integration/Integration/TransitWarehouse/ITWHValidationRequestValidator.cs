namespace Enterprise.Warehouse.Integration
{
	public interface ITWHValidationRequestValidator
	{
		bool IsValid(ITWHValidationRequest request, out string message);
	}
}

