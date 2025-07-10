namespace Enterprise.Warehouse.Yard.Integration;

public interface IYardValidationRequestValidator
{
	bool IsValid(IYardValidationRequest? request, out string? message);
}

public interface IYardDropoffRequestValidator : IYardValidationRequestValidator { }

public interface IYardPickupRequestValidator : IYardValidationRequestValidator { }
