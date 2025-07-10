using System.Globalization;
using CsvHelper.Configuration;

namespace CargoWise.RefDbRepo.ESReferenceData.Business
{
	public class ImportCPCItem : IItem
	{
		public string Code { get; set; }

		public string Description { get; set; }

		public string StartDate { get; set; }

		public string EndDate { get; set; }

		public string GroupTypes { get; set; }

		public string Concessions1 { get; set; }

		public string Concessions2 { get; set; }

		public string Concessions3 { get; set; }

		public string Concessions4 { get; set; }

		public string Concessions5 { get; set; }

		public string Duty { get; set; }

		public string VAT { get; set; }
	}

	public sealed class ImportCPCItemMap : ClassMap<ImportCPCItem>
	{
		public ImportCPCItemMap()
		{
			AutoMap();
			Map(m => m.Code).Name("Regímenes Importación Cas. 37.1");
			Map(m => m.Description).Name("Descripción");
			Map(m => m.StartDate).Name("Fecha de Inicio");
			Map(m => m.EndDate).Name("Fecha de Fin");
			Map(m => m.GroupTypes).Name("Tipo Declaración Cas. 1.1 Compatible");
			Map(m => m.Concessions1).Name("Código C37.2 A Compatible");
			Map(m => m.Concessions2).Name("Código C37.2 B Compatible");
			Map(m => m.Concessions3).Name("Código C37.2 C Compatible");
			Map(m => m.Concessions4).Name("Código C37.2 D Compatible");
			Map(m => m.Concessions5).Name("Código C37.2 E Compatible");
			Map(m => m.Duty).Name("Indicador paga/garantiza Arancel");
			Map(m => m.VAT).Name("Indicador paga/garantiza IVA");
		}
	}
}
