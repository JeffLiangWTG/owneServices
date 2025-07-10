using CargoWise.Types;

namespace Enterprise.Freight.Business
{
	public interface IDeclarationAsCartageParent : IShipmentWithDocsAndCartage
	{
		ZString JE_VesselName { get; }
		ZString JE_VoyageFlightNo { get; }
		ZString JE_RL_NKPortOfLoading { get; }
		ZString JE_RL_NKPortOfArrival { get; }
		ZDateTime JE_ExportDate { get; }
		ZDateTime JE_DateOfArrival { get; }
	}
}
