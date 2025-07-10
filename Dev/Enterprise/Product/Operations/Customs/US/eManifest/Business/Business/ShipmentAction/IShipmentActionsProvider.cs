namespace Enterprise.Customs.US.eManifest.Business
{
	using CargoWise.EntityFramework;

	public interface IShipmentActionsProvider : IBusiness
	{
		void ResetShipmentsActionsIfMessageTypeChanged(string messageTypeToBeSubmitted);
		ShipmentActionCollection ShipmentsActions { get; }
	}
}
