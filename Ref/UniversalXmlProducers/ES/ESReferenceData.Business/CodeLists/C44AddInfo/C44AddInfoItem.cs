using CsvHelper.Configuration;

namespace CargoWise.RefDbRepo.ESReferenceData.Business
{
	public class C44AddInfoItem
	{
		public string Code { get; set; }

		public string Description { get; set; }

		public string StartDate { get; set; }

		public string EndDate { get; set; }

		public string SpecialIndication { get; set; }

		public string CharacterIndication { get; set; }
	}

	public class C44AddInfoItemMap : ClassMap<C44AddInfoItem>
	{
		public C44AddInfoItemMap()
		{
			AutoMap();
			Map(m => m.Code).Name(new [] { "Código", "Codigo" });
			Map(m => m.Description).Name("Descripción");
			Map(m => m.StartDate).Name("Fecha Inicio");
			Map(m => m.EndDate).Name("Fecha Fin");
			Map(m => m.SpecialIndication).Name("INDICACION ESPECIAL");
			Map(m => m.CharacterIndication).Name("CARACTER INDICACION");
		}
	}
}
