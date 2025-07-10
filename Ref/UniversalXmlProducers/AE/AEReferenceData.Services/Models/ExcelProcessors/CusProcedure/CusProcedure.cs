namespace CargoWise.RefDbRepo.AEReferenceData.Services;

public sealed class CusProcedure
{
	public string ProcedureCode { get; set; }

	public string DeclarationDescription { get; set; }

	public string ShipmentType { get; set; }

	public string IsTransit { get; set; }

	public bool CalculateDuty { get; set; }

	public string IntoTemporaryImport { get; set; }

	public string Category { get; set; }

	public string DeclarationTypeCode { get; set; }
}
