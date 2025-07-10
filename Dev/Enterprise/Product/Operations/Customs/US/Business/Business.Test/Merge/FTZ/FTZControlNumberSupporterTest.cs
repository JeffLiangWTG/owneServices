using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class FTZControlNumberSupporterTest : TestCaseWithFactory
	{
		public void TestGetReasonToStopProceeding()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			IAllocateNumberSupporter supporter = new FTZControlNumberSupporter(declaration);
			AssertEquals(ZString.Empty, supporter.GetReasonToStopProceeding());
			declaration.AdmissionStatus = FTZMessageStatusList.Codes.AwaitingFTZAdmissionAdd;
			AssertEquals(ZString.Empty, supporter.GetReasonToStopProceeding());
			declaration.FTZAdmissionNumber = "2140000|18|00000001";
			AssertEquals(JobDeclaration.Constants.FTZControlNumberAllocation.FTZControlNumberAlreadyAllocated("00000001"), supporter.GetReasonToStopProceeding());
			declaration.AdmissionStatus = FTZMessageStatusList.Codes.ErrorFTZAdmissionAdd;
			AssertEquals(ZString.Empty, supporter.GetReasonToStopProceeding());
			declaration.AdmissionStatus = FTZMessageStatusList.Codes.ClearFTZAdmissionAddWithWarnings;
			AssertEquals(JobDeclaration.Constants.FTZControlNumberAllocation.FTZControlNumberAlreadyAllocated("00000001"), supporter.GetReasonToStopProceeding());
			declaration.AdmissionStatus = FTZMessageStatusList.Codes.AwaitingFTZAdmissionAmend;
			AssertEquals(JobDeclaration.Constants.FTZControlNumberAllocation.FTZControlNumberAlreadyAllocated("00000001"), supporter.GetReasonToStopProceeding());
			declaration.AdmissionStatus = FTZMessageStatusList.Codes.ClearFTZAdmissionAmend;
			AssertEquals(JobDeclaration.Constants.FTZControlNumberAllocation.FTZControlNumberAlreadyAllocated("00000001"), supporter.GetReasonToStopProceeding());
			declaration.AdmissionStatus = FTZMessageStatusList.Codes.AwaitingFTZAdmissionDelete;
			AssertEquals(JobDeclaration.Constants.FTZControlNumberAllocation.FTZControlNumberAlreadyAllocated("00000001"), supporter.GetReasonToStopProceeding());
			declaration.AdmissionStatus = FTZMessageStatusList.Codes.ClearFTZAdmissionDelete;
			AssertEquals(JobDeclaration.Constants.FTZControlNumberAllocation.FTZControlNumberAlreadyAllocated("00000001"), supporter.GetReasonToStopProceeding());
		}
	}
}
