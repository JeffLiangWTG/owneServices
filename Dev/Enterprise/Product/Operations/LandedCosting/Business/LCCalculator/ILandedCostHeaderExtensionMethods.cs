using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.LandedCosting.Business
{
	public static class ILandedCostHeaderExtensionMethods
	{
		public static ZGlobalMutex GetLandedCostMutex(this ILandedCostHeader lcHost)
		{
			return new ZGlobalMutex(MutexIDs.LandedCostingBeingCreatedForDeclarationOrOrder, lcHost.PK.ToString());
		}
	}
}
