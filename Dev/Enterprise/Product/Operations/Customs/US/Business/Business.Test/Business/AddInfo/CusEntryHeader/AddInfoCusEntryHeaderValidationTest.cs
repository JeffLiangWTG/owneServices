using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.US;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class AddInfoCusEntryHeaderValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckUS_ShouldBeReportToCustoms()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			CusEntryHeader header = Factory.New<CusEntryHeader>();
			header.CH_JE = declaration.PK;
			header.CH_Status = AESDirectCustomsEntryStatus.Codes.OriginalSEDClear;
			header.US_ShouldBeReportToCustoms = true;
			AssertNoMessageError(header.US_ShouldBeReportToCustomsInfo, AddInfoCusEntryHeaderValidation.PendingResponse);
			header.CH_Status = AESDirectCustomsEntryStatus.Codes.AwaitingOriginalResponse;
			header.US_ShouldBeReportToCustoms = false;
			AssertNoMessageError(header.US_ShouldBeReportToCustomsInfo, AddInfoCusEntryHeaderValidation.PendingResponse);
			header.CH_Status = AESDirectCustomsEntryStatus.Codes.AwaitingOriginalResponse;
			header.US_ShouldBeReportToCustoms = true;
			AssertHasMessageError(header.US_ShouldBeReportToCustomsInfo, AddInfoCusEntryHeaderValidation.PendingResponse);
			header.CH_Status = AESDirectCustomsEntryStatus.Codes.AwaitingReplacementResponse;
			header.US_ShouldBeReportToCustoms = true;
			AssertHasMessageError(header.US_ShouldBeReportToCustomsInfo, AddInfoCusEntryHeaderValidation.PendingResponse);
			header.CH_Status = AESDirectCustomsEntryStatus.Codes.AwaitingDeleteResponse;
			header.US_ShouldBeReportToCustoms = true;
			AssertHasMessageError(header.US_ShouldBeReportToCustomsInfo, AddInfoCusEntryHeaderValidation.PendingResponse);
		}
	}
}
