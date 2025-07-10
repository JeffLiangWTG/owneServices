namespace CargoWise.RefDbRepo.NLReferenceData.Services.Testing
{
	public class CommonExcelProcessManagerForTest : CommonExcelProcessManagerAbstract<CommonExcelTestData>
	{
		public CommonExcelProcessManagerForTest(IDataBuilder<CommonExcelTestData> dataBuilder) : base(dataBuilder, new ExcelParserForTest())
		{
		}

		protected override string ResourceContent => "CargoWise.RefDbRepo.NLReferenceData.Tests.TestFiles.ExcelProcessing.Input.CommonExcel.xlsx";
		protected override string ExcelFileName => "CommonExcel.xlsx";
		protected override string EntityName => "Common Excel";
	}
}
