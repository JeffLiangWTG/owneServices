using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.Testing
{
	public class AMSProgramListTest : TestCaseWithFactory
	{
		public void TestIsAMSProgramButNotNOP()
		{
			AssertEquals(true, AMSProgramList.IsAMSProgramButNotNOP(Factory, AMSProgramList.Codes.EG1));
			AssertEquals(true, AMSProgramList.IsAMSProgramButNotNOP(Factory, AMSProgramList.Codes.EG2));
			AssertEquals(true, AMSProgramList.IsAMSProgramButNotNOP(Factory, AMSProgramList.Codes.MO1));
			AssertEquals(true, AMSProgramList.IsAMSProgramButNotNOP(Factory, AMSProgramList.Codes.MO2));
			AssertEquals(true, AMSProgramList.IsAMSProgramButNotNOP(Factory, AMSProgramList.Codes.MO3));
			AssertEquals(true, AMSProgramList.IsAMSProgramButNotNOP(Factory, AMSProgramList.Codes.MO4));
			AssertEquals(true, AMSProgramList.IsAMSProgramButNotNOP(Factory, AMSProgramList.Codes.MO5));
			AssertEquals(true, AMSProgramList.IsAMSProgramButNotNOP(Factory, AMSProgramList.Codes.MO6));
			AssertEquals(true, AMSProgramList.IsAMSProgramButNotNOP(Factory, AMSProgramList.Codes.MO7));
			AssertEquals(true, AMSProgramList.IsAMSProgramButNotNOP(Factory, AMSProgramList.Codes.MO8));
			AssertEquals(true, AMSProgramList.IsAMSProgramButNotNOP(Factory, AMSProgramList.Codes.PN1));
			AssertEquals(false, AMSProgramList.IsAMSProgramButNotNOP(Factory, AMSProgramList.Codes.OR1));
			AssertEquals(false, AMSProgramList.IsAMSProgramButNotNOP(Factory, AMSProgramList.Codes.OR2));
			AssertEquals(false, AMSProgramList.IsAMSProgramButNotNOP(Factory, ZString.Empty));
			AssertEquals(false, AMSProgramList.IsAMSProgramButNotNOP(Factory, "TTT"));
		}
	}
}
