using CargoWise.Application;

namespace Enterprise.MasterFiles.Business
{
	public static class GlbStaffWrapperHelper
	{
		public static Integration.Customs.AU.IAUGlbStaffWrapper GetAUWrapper(this GlbStaff staff)
		{
			return staff.GetWrapper<Integration.Customs.AU.IAUGlbStaffWrapper, Integration.Customs.AU.IAUGlbStaffWrapperProvider>();
		}

		public static Integration.CustomsIntegration.IT.IGlbStaffWrapper GetITWrapper(this GlbStaff staff)
		{
			return staff.GetWrapper<Integration.CustomsIntegration.IT.IGlbStaffWrapper, Integration.CustomsIntegration.IT.IGlbStaffWrapperProvider>();
		}

		public static Integration.Customs.NZ.INZGlbStaffWrapper GetNZWrapper(this GlbStaff staff)
		{
			return staff.GetWrapper<Integration.Customs.NZ.INZGlbStaffWrapper, Integration.Customs.NZ.INZGlbStaffWrapperProvider>();
		}

		public static Integration.Customs.SG.ISGGlbStaffWrapper GetSGWrapper(this GlbStaff staff)
		{
			return staff.GetWrapper<Integration.Customs.SG.ISGGlbStaffWrapper, Integration.Customs.SG.ISGGlbStaffWrapperProvider>();
		}

		public static Integration.Customs.TW.ITWGlbStaffWrapper GetTWWrapper(this GlbStaff staff)
		{
			return staff.GetWrapper<Integration.Customs.TW.ITWGlbStaffWrapper, Integration.Customs.TW.ITWGlbStaffWrapperProvider>();
		}

		public static Integration.Customs.BR.IBRGlbStaffWrapper GetBRWrapper(this GlbStaff staff)
		{
			return staff.GetWrapper<Integration.Customs.BR.IBRGlbStaffWrapper, Integration.Customs.BR.IBRGlbStaffWrapperProvider>();
		}

		public static Integration.Customs.PL.IPLGlbStaffWrapper GetPLWrapper(this GlbStaff staff)
		{
			return staff.GetWrapper<Integration.Customs.PL.IPLGlbStaffWrapper, Integration.Customs.PL.IPLGlbStaffWrapperProvider>();
		}

		public static Integration.Customs.IN.IINGlbStaffWrapper GetINWrapper(this GlbStaff staff)
		{
			return staff.GetWrapper<Integration.Customs.IN.IINGlbStaffWrapper, Integration.Customs.IN.IINGlbStaffWrapperProvider>();
		}

		public static Integration.Customs.MX.IMXGlbStaffWrapper GetMXWrapper(this GlbStaff staff)
		{
			return staff.GetWrapper<Integration.Customs.MX.IMXGlbStaffWrapper, Integration.Customs.MX.IMXGlbStaffWrapperProvider>();
		}

		static T GetWrapper<T, TProvider>(this GlbStaff staff)
			where T : Integration.IGlbStaffWrapper
			where TProvider : Integration.IGlbStaffWrapperProvider
		{
			T wrapper = default(T);
			if (staff != null)
			{
				var provider = ObjectFactory.Get<TProvider>();
				wrapper = (T)provider.GetWrapper(staff);
			}
			return wrapper;
		}
	}
}
