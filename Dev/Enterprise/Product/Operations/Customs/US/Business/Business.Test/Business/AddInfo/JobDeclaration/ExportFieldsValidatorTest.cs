using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ExportFieldsValidatorTest : TestCaseWithFactory
	{
		public void TestFTZIndicatorFormat()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.US_ForeignTradeZone = "AAA1234";
			AssertHasMessageError(declaration.US_ForeignTradeZoneInfo, ExportFieldsValidator.InvalidFTZIndicatorFormat);
			declaration.US_ForeignTradeZone = "1234A00";
			AssertNoMessageError(declaration.US_ForeignTradeZoneInfo, ExportFieldsValidator.InvalidFTZIndicatorFormat);
			declaration.US_ForeignTradeZone = "1234A0B";
			AssertNoMessageError(declaration.US_ForeignTradeZoneInfo, ExportFieldsValidator.InvalidFTZIndicatorFormat);
			declaration.US_ForeignTradeZone = "AAA123456";
			AssertHasMessageError(declaration.US_ForeignTradeZoneInfo, ExportFieldsValidator.InvalidFTZIndicatorFormat);
			declaration.US_ForeignTradeZone = "1234A0012";
			AssertNoMessageError(declaration.US_ForeignTradeZoneInfo, ExportFieldsValidator.InvalidFTZIndicatorFormat);
			declaration.US_ForeignTradeZone = "1234A0BCD";
			AssertNoMessageError(declaration.US_ForeignTradeZoneInfo, ExportFieldsValidator.InvalidFTZIndicatorFormat);
		}

		public void TestOriginalITNNumberFormat()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.US_OriginalITNNumber = "XDDWE3333";
			AssertHasMessageError(declaration.US_OriginalITNNumberInfo, ExportFieldsValidator.InvalidOriginalTINFormat);
			declaration.US_OriginalITNNumber = "X20170231666666";
			AssertHasMessageError(declaration.US_OriginalITNNumberInfo, ExportFieldsValidator.InvalidOriginalTINFormat);
			declaration.US_OriginalITNNumber = "X20170619666666";
			AssertNoMessageError(declaration.US_OriginalITNNumberInfo, ExportFieldsValidator.InvalidOriginalTINFormat);
		}
	}
}
