using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	class GlbExternalPasswordTypeDeciderTest : TestCaseWithFactory
	{
		public void TestGetTypeForNew() => AssertNull("GetTypeForNew()", new GlbExternalPasswordTypeDecider().GetTypeForNew());
		public void TestGetTypeForBinding() => AssertNull("GetTypeForBinding()", new GlbExternalPasswordTypeDecider().GetTypeForBinding());

		public void TestGetTypeForLoad()
		{
			var staffCredential = Factory.New<GlbStaffCredential>();
			staffCredential.GP_GS = GlbStaff.CurrentUser.PK;
			var staffCredentialRow = ((INeedRow)staffCredential).Row;
			var companyCredential = Factory.New<GlbCompanyCredential>();
			var companyCredentialRow = ((INeedRow)companyCredential).Row;
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var typeDecider = new GlbExternalPasswordTypeDecider();
			AssertEquals("GlbStaffCredential", typeof(GlbStaffCredential), typeDecider.GetTypeForLoad(staffCredentialRow, newFactory));
			AssertEquals("GlbCompanyCredential", typeof(GlbCompanyCredential), typeDecider.GetTypeForLoad(companyCredentialRow, newFactory));
			AssertType<GlbStaffCredential>("Load via MasterFiles - GlbStaffCredential", newFactory.Load<MasterFiles.Business.GlbExternalPassword>(staffCredential.PK));
			AssertType<GlbStaffCredential>("via US - GlbStaffCredential", newFactory.Load<GlbExternalPassword>(staffCredential.PK));
			AssertType<GlbCompanyCredential>("Load via MasterFiles - GlbCompanyCredential", newFactory.Load<MasterFiles.Business.GlbExternalPassword>(companyCredential.PK));
			AssertType<GlbCompanyCredential>("via US - GlbCompanyCredential", newFactory.Load<GlbExternalPassword>(companyCredential.PK));
		}
	}
}
