using CsvHelper.Configuration;

namespace CargoWise.RefDbRepo.ESReferenceData.Business
{
	public class C44DocumentItem
	{
		public string Code { get; set; }

		public string Description { get; set; }
	}

	public class C44DocumentItemMap : ClassMap<C44DocumentItem>
	{
		public C44DocumentItemMap()
		{
			AutoMap();
			Map(m => m.Code).Name(new [] { "Código", "Codigo" });
			Map(m => m.Description).Name("Descripción");
		}
	}
}
