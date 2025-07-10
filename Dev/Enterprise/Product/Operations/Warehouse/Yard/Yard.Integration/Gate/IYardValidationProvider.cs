namespace Enterprise.Warehouse.Yard.Integration;

public interface IYardValidationProvider
{
	IYardValidationData Get(IYardValidationRequest request);
}

public interface IYardDropoffValidationProvider : IYardValidationProvider { }

public interface IYardPickupValidationProvider : IYardValidationProvider { }
