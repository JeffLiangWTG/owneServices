using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.NZ.Business.Testing
{
	using BaseJobMessageTypeList = Customs.Business.JobMessageTypeList;

	public class JobMessageTypeListTest : TestCaseWithFactory
	{
		public void TestCodesMatchBaseCustoms()
		{
			BaseJobMessageTypeList baseList = new BaseJobMessageTypeList();

			AssertEquals("Codes.Import", BaseJobMessageTypeList.Codes.Import, JobMessageTypeList.Codes.Import);
			AssertEquals("Descriptions.Import", BaseJobMessageTypeList.Descriptions.Import, JobMessageTypeList.Descriptions.Import);

			AssertEquals("Codes.Export", BaseJobMessageTypeList.Codes.Export, JobMessageTypeList.Codes.Export);
			AssertEquals("Descriptions.Export", BaseJobMessageTypeList.Descriptions.Export, JobMessageTypeList.Descriptions.Export);

			AssertNotEquals("baseList.ContainsCode(JobMessageTypeList.Codes.Excise)", true, baseList.ContainsCode(JobMessageTypeList.Codes.Excise));

			AssertEquals("Codes.MiscellaneousCustoms", BaseJobMessageTypeList.Codes.MiscellaneousCustoms, JobMessageTypeList.Codes.MiscellaneousCustoms);
			AssertEquals("Descriptions.MiscellaneousCustoms", BaseJobMessageTypeList.Descriptions.MiscellaneousCustoms, JobMessageTypeList.Descriptions.MiscellaneousCustoms);
		}
	}
}
