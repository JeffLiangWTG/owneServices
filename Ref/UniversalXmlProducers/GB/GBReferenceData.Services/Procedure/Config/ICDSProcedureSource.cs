namespace CargoWise.RefDbRepo.GBReferenceData.Services.Procedure.Config
{
	public interface ICDSProcedureSource
	{
		string Name { get; }

		string ProcedureUrl { get; }

		string AdditionalProcedureUrl { get; }

		string AdditionalProcedureMatrixUrl { get; }

		string ShipmentType { get; }
	}
}
