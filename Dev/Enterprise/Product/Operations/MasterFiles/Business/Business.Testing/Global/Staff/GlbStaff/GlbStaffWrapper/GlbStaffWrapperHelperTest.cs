using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class GlbStaffWrapperHelperTest : TestCaseWithFactory
	{
		public void TestGetAUWrapper()
		{
			GlbStaff staff = null;
			AssertNull(staff.GetAUWrapper());
			staff = Factory.New<GlbStaff>();
			var wrapper = staff.GetAUWrapper();
			AssertNotNull(wrapper);
			AssertEquals(true, typeof(Integration.Customs.AU.IAUGlbStaffWrapper).IsAssignableFrom(wrapper.GetType()));
		}

		public void TestGetITWrapper()
		{
			GlbStaff staff = null;
			AssertNull(staff.GetITWrapper());
			staff = Factory.New<GlbStaff>();
			var wrapper = staff.GetITWrapper();
			AssertNotNull(wrapper);
			AssertEquals(true, typeof(Integration.CustomsIntegration.IT.IGlbStaffWrapper).IsAssignableFrom(wrapper.GetType()));
		}

		public void TestGetNZWrapper()
		{
			GlbStaff staff = null;
			AssertNull(staff.GetNZWrapper());
			staff = Factory.New<GlbStaff>();
			var wrapper = staff.GetNZWrapper();
			AssertNotNull(wrapper);
			AssertEquals(true, typeof(Integration.Customs.NZ.INZGlbStaffWrapper).IsAssignableFrom(wrapper.GetType()));
		}

		public void TestGetSGWrapper()
		{
			GlbStaff staff = null;
			AssertNull(staff.GetSGWrapper());
			staff = Factory.New<GlbStaff>();
			var wrapper = staff.GetSGWrapper();
			AssertNotNull(wrapper);
			AssertEquals(true, typeof(Integration.Customs.SG.ISGGlbStaffWrapper).IsAssignableFrom(wrapper.GetType()));
		}

		public void TestGetTWWrapper()
		{
			GlbStaff staff = null;
			AssertNull(staff.GetTWWrapper());
			staff = Factory.New<GlbStaff>();
			var wrapper = staff.GetTWWrapper();
			AssertNotNull(wrapper);
			AssertEquals(true, typeof(Integration.Customs.TW.ITWGlbStaffWrapper).IsAssignableFrom(wrapper.GetType()));
		}

		public void TestGetBRWrapper()
		{
			GlbStaff staff = null;
			AssertNull(staff.GetBRWrapper());
			staff = Factory.New<GlbStaff>();
			var wrapper = staff.GetBRWrapper();
			AssertNotNull(wrapper);
			AssertEquals(true, typeof(Integration.Customs.BR.IBRGlbStaffWrapper).IsAssignableFrom(wrapper.GetType()));
		}

		public void TestGetPLWrapper()
		{
			GlbStaff staff = null;
			AssertNull(staff.GetPLWrapper());
			staff = Factory.New<GlbStaff>();
			var wrapper = staff.GetPLWrapper();
			AssertNotNull(wrapper);
			AssertEquals(true, typeof(Integration.Customs.PL.IPLGlbStaffWrapper).IsAssignableFrom(wrapper.GetType()));
		}

		public void TestGetINWrapper()
		{
			GlbStaff staff = null;
			AssertNull(staff.GetINWrapper());
			staff = Factory.New<GlbStaff>();
			var wrapper = staff.GetINWrapper();
			AssertNotNull(wrapper);
			AssertEquals(true, typeof(Integration.Customs.IN.IINGlbStaffWrapper).IsAssignableFrom(wrapper.GetType()));
		}

		public void TestGetMXWrapper()
		{
			GlbStaff staff = null;
			AssertNull(staff.GetMXWrapper());
			staff = Factory.New<GlbStaff>();
			var wrapper = staff.GetMXWrapper();
			AssertNotNull(wrapper);
			AssertEquals(true, typeof(Integration.Customs.MX.IMXGlbStaffWrapper).IsAssignableFrom(wrapper.GetType()));
		}
	}
}
