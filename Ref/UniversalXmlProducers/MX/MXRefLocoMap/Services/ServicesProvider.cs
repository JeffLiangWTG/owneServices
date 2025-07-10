namespace CargoWise.RefDbRepo.MXRefLocoMap.Business
{
	public static class ServicesProvider
	{
		public static ILogger Logger { get; } = new ConsoleLogger();
		public static IExcelParser ExcelParser { get; } = new ExcelParser();
	}
}
