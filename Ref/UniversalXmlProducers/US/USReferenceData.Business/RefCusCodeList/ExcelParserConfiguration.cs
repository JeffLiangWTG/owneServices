namespace CargoWise.RefDbRepo.USReferenceData.Business
{
	public class ExcelParserConfiguration
	{
		public ExcelParserConfiguration()
		{
			SheetIndex = 1;
			HeaderRow = 1;
			StartingRow = 2;
			LastRow = int.MaxValue;
		}

		public int SheetIndex { get; set; }
		public int HeaderRow { get; set; }
		public int StartingRow { get; set; }
		public int LastRow { get; set; }
	}
}
