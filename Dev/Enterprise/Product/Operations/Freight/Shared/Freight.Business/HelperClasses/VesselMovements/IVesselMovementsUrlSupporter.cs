namespace Enterprise.Freight.Business
{
	public interface IVesselMovementsUrlSupporter
	{
		(VesselMovementsUrlModel model, string errorMessage) GetVesselMovementsUrlModel();
	}
}
