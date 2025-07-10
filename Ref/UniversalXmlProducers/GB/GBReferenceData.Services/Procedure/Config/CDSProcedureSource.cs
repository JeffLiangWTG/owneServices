namespace CargoWise.RefDbRepo.GBReferenceData.Services.Procedure.Config
{
	public class CDSProcedureSource : ICDSProcedureSource
	{
		public string Name { get; set; }

		public string ProcedureUrl { get; set; }

		public string AdditionalProcedureUrl { get; set; }

		public string AdditionalProcedureMatrixUrl { get; set; }

		public string ShipmentType { get; set; }
	}
}
