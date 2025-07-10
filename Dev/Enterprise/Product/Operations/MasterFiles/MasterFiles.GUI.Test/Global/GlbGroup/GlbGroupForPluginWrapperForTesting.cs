using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class GlbGroupForPluginWrapperForTesting : NonPersistentBusinessObject
	{
		public GlbGroupForPluginWrapperForTesting(GlbGroup group)
			: base(group.Factory)
		{
			this.group = group;
		}
		readonly GlbGroup group;

		public ZGlobalMutex Mutex => fMutext ?? (fMutext = new ZGlobalMutex(MutexIDs.GroupCredentialsPlugInBeingCreated, group.PK + Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(GlbCompany.CurrentCompany.GC_RN_NKCountryCode)));
		ZGlobalMutex fMutext;
	}
}
