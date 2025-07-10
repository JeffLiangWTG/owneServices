using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Warehouse.GateManagement.Integration
{
	public interface IGateFacilityValidationService
	{
		string[] ValidateMovement(ITopLevelDataObject shipment, string validationType);
	}
}
