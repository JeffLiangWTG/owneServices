using System.Globalization;
using CsvHelper.Configuration;

namespace CargoWise.RefDbRepo.ESReferenceData.Business
{
	public class ExportCPCConcessionItem : IConcessionItem
	{
		public string Code { get; set; }

		public string Description { get; set; }

		public string StartDate { get; set; }

		public string EndDate { get; set; }

		public string GroupTypes { get; set; }
	}

	public sealed class ExportCPCConcessionItemMap : ClassMap<ExportCPCConcessionItem>
	{
		public ExportCPCConcessionItemMap()
		{
			AutoMap();
			Map(m => m.Code).Name("Código");
			Map(m => m.Description).Name("Descripción");
			Map(m => m.StartDate).Name("Fecha Inicio");
			Map(m => m.EndDate).Name("Fecha Fin");
			Map(m => m.GroupTypes).Name("TIPDEC C1.1 COMPATIB");
		}
	}
}
