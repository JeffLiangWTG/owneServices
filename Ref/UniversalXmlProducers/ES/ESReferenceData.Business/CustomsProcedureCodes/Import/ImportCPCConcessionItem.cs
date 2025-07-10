using System.Globalization;
using CsvHelper.Configuration;

namespace CargoWise.RefDbRepo.ESReferenceData.Business
{
	public class ImportCPCConcessionItem : IConcessionItem
	{
		public string Code { get; set; }

		public string Description { get; set; }

		public string StartDate { get; set; }

		public string EndDate { get; set; }

		public string GroupTypes { get; set; }

		public string Duty { get; set; }

		public string VAT { get; set; }
	}

	public sealed class ImportCPCConcessionItemMap : ClassMap<ImportCPCConcessionItem>
	{
		public ImportCPCConcessionItemMap()
		{
			AutoMap();
			Map(m => m.Code).Name("Regímenes Importación Cas. 37.2");
			Map(m => m.Description).Name("Descripción");
			Map(m => m.StartDate).Name("Fecha de Inicio");
			Map(m => m.EndDate).Name("Fecha de Fin");
			Map(m => m.GroupTypes).Name("Tipo Declaración Cas. 1.1 Compatible");
			Map(m => m.Duty).Name("Indicador paga/garantiza Arancel");
			Map(m => m.VAT).Name("Indicador paga/garantiza IVA");
		}
	}
}
