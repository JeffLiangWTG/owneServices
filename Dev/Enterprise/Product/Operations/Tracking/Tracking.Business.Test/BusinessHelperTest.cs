using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Tracking.Business.Testing
{
	sealed class BusinessHelperTest : TestCaseWithFactory
	{
		#region Setup

		BusinessHelper Helper;
		OrgHeader Company;
		const string CompanyCode = "MyCode";

		protected override void SetUp()
		{
			base.SetUp();
			Helper = new BusinessHelper(Factory);
			Company = Factory.New<OrgHeader>();
			Company.OH_Code = CompanyCode;
		}

		#endregion

		public void TestGetPKFromOrgCode()
		{
			AssertEquals(Company.PK, Helper.GetPKFromOrgCode(CompanyCode));
			AssertEquals(ZGuid.Invalid, Helper.GetPKFromOrgCode("my random string"));
			AssertEquals(ZGuid.Empty, Helper.GetPKFromOrgCode(ZString.Empty));
		}

		public void TestGetOrgCode()
		{
			AssertEquals(ZString.Empty, Helper.GetOrgCode(ZGuid.Empty));
			AssertEquals(ZString.Empty, Helper.GetOrgCode(ZGuid.Invalid));
			AssertEquals(CompanyCode, Helper.GetOrgCode(Company.PK));
		}
	}
}
