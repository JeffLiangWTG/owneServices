using System.Globalization;
using CsvHelper.Configuration;

namespace CargoWise.RefDbRepo.ESReferenceData.Business
{
	public class ExportCPCItem : IItem
	{
		public string Code { get; set; }

		public string Description { get; set; }

		public string StartDate { get; set; }

		public string EndDate { get; set; }

		public string GroupTypes { get; set; }

		public string Concessions1 { get; set; }

		public string Concessions2 { get; set; }
	}

	public sealed class ExportCPCItemMap : ClassMap<ExportCPCItem>
	{
		public ExportCPCItemMap()
		{
			AutoMap();
			Map(m => m.Code).Name("Código");
			Map(m => m.Description).Name("Descripción");
			Map(m => m.StartDate).Name("Fecha Inicio");
			Map(m => m.EndDate).Name("Fecha Fin");
			Map(m => m.GroupTypes).Name("TIPDEC C1.1 COMPATIB");
			Map(m => m.Concessions1).Name("COD C37.2 COMPATIB A");
			Map(m => m.Concessions2).Name("COD C37.2 COMPATIB B");
		}
	}
}
