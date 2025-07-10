using System.Collections;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MasterFiles.GUI
{
	public abstract class GlbStaffWrapperProvider : IGlbStaffWrapperProvider
	{
		protected GlbStaffWrapperProvider() { }

		public static GlbStaffWrapperProvider GetProvider()
		{
			return GetProvider(GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
		}

		public static GlbStaffWrapperProvider GetProvider(ZString actualCountry)
		{
			GlbStaffWrapperProvider result = null;
			string countryCode = Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(actualCountry);
			var writers = ObjectFactory.Get<Hashtable>("GlbStaffWrapperProviders");
			var objectHandle = (ObjectHandle)writers[countryCode];
			if (objectHandle != null)
			{
				result = (GlbStaffWrapperProvider)objectHandle.GetObject();
			}

			return result;
		}

		public virtual MenuItem GetNewTopLevelMenu(GlbStaffWrapper wrapper)
		{
			return null;
		}

		public GlbStaffWrapper GetWrapper(GlbStaff staff) => GetWrapperCore(staff);
		protected abstract GlbStaffWrapper GetWrapperCore(GlbStaff staff);

		public StaffCredentialsUserControl GetNewUserControl() => GetNewUserControlCore();
		protected abstract StaffCredentialsUserControl GetNewUserControlCore();

		public ContinueWithSave ShowPreSaveDialogs(GlbStaffWrapper wrapper) => ShowPreSaveDialogsCore(wrapper);
		protected virtual ContinueWithSave ShowPreSaveDialogsCore(GlbStaffWrapper wrapper) => ContinueWithSave.Yes;

		IGlbStaffWrapper IGlbStaffWrapperProvider.GetWrapper(IGlbStaff staff) => GetWrapper((GlbStaff)staff);
	}
}
