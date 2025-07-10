using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.Shared;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.Testing
{
	sealed class SharedCusPermitHeaderTest : TestCaseWithFactory
	{
		public void TestCPH_SubType_ReadOnly()
		{
			var permitHeader = Factory.NewWithValidTestData<BaseCusPermitHeader>();
			permitHeader.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Guarantee;
			Factory.Save();
			GlbStaff.CurrentUser.GS_IsController = false;
			AssertEquals(true, permitHeader.CPH_SubType_ReadOnly);
			GlbStaff.CurrentUser.GS_IsController = true;
			AssertEquals(false, permitHeader.CPH_SubType_ReadOnly);
		}
	}
}
