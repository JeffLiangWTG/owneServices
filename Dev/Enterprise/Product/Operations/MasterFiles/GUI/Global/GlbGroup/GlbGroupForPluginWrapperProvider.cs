using System.Collections;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public abstract class GlbGroupForPluginWrapperProvider
	{
		protected GlbGroupForPluginWrapperProvider() { }

		public static GlbGroupForPluginWrapperProvider GetProvider()
		{
			GlbGroupForPluginWrapperProvider result = null;
			var countryCode = Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			var writers = ObjectFactory.Get<Hashtable>("GlbGroupForPluginWrapperProviders");
			var objectHandle = (ObjectHandle)writers[countryCode];
			if (objectHandle != null)
			{
				result = (GlbGroupForPluginWrapperProvider)objectHandle.GetObject();
			}

			return result;
		}

		public IBusiness GetWrapperWithMutex(GlbGroup group)
		{
			var countryCode = Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			return group != null
				? group.Factory.GetCachedValue(group.PK + countryCode, () => CreateNewGlbGroupWrapper(group))
				: null;
		}

		protected abstract IBusiness CreateNewGlbGroupWrapper(GlbGroup group);

		public abstract ZUserControl GetNewUserControl();
	}
}
