namespace CargoWise.RefDbRepo.USReferenceData.Business.ExchangeRate
{
	public class ExcelParserConfiguration
	{
		public ExcelParserConfiguration()
		{
			SheetIndex = 1;

			PublishDateRowIndex = 5;
			PublishDateColumnIndex = 2;

			StartingRow = 8;
			LastRow = 45;
		}

		public int SheetIndex { get; set; }

		public int PublishDateRowIndex { get; set; }

		public int PublishDateColumnIndex { get; set; }

		public int StartingRow { get; set; }

		public int LastRow { get; set; }
	}
}
