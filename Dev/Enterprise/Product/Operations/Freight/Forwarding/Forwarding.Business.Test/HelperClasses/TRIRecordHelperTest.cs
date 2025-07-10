using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class TRIRecordHelperTest : TestCaseWithFactory
	{
		public void TestGetTRIRecord()
		{
			GlbBranch.CurrentBranch.OrgProxy.CustomsCodes.RemoveAll();
			GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.RemoveAll();
			var result = TRIRecordHelper.GetTRIRecord();
			AssertEquals(ZString.Empty, result);

			GlbBranch.CurrentBranch.OrgProxy.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CargoWiseOneCarrierCode, "123");
			result = TRIRecordHelper.GetTRIRecord();
			AssertEquals(ZString.Empty, result);

			GlbBranch.CurrentBranch.OrgProxy.CustomsCodes.AddNew(OrgCusCode.CodeTypes.BoleroTitleRegisterID, "456");
			result  = TRIRecordHelper.GetTRIRecord();
			AssertEquals("456", result);

			GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.AddNew(OrgCusCode.CodeTypes.BoleroTitleRegisterID, "789");
			GlbBranch.CurrentBranch.OrgProxy.CustomsCodes.RemoveAll();
			result = TRIRecordHelper.GetTRIRecord();
			AssertEquals(ZString.Empty, result);

			GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.RemoveAll();
			GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.AddNew(OrgCusCode.CodeTypes.BoleroTitleRegisterID, "112");
			GlbBranch.CurrentBranch.GB_OH_OrgProxy = ZGuid.Empty;
			result = TRIRecordHelper.GetTRIRecord();
			AssertEquals("112", result);

			GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.RemoveAll();
			GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CargoWiseOneCarrierCode, "124");
			result = TRIRecordHelper.GetTRIRecord();
			AssertEquals(ZString.Empty, result);

			GlbCompany.CurrentCompany.GC_OH_OrgProxy = ZGuid.Empty;
			result = TRIRecordHelper.GetTRIRecord();
			AssertEquals(ZString.Empty, result);
		}
	}
}
