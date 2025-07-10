using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.NZ.Business
{
	public static class FactoryExtensions
	{
		public static bool HasHVLVAccess(this BusinessObjectFactory factory)
		{
			return Enterprise.Registry.Business.HVLVDataRegistry.HasHVLVClearance;
		}

		#region CusSCAOceanBill Mutex

		public static string GetSeaCargoMutexLock(this BusinessObjectFactory factory, ZGuid consolPK)
		{
			return factory.GetChildLockByInfo(MutexIDs.CusSCAOceanBillJobBeingCreatedForConsol, consolPK, Core.Constants.CountryCodes.NewZealand);
		}

		public static bool LockSeaCargoMutex(this BusinessObjectFactory factory, ZGuid consolPK)
		{
			return factory.LockChild(MutexIDs.CusSCAOceanBillJobBeingCreatedForConsol, consolPK, Core.Constants.CountryCodes.NewZealand);
		}

		public static void ReleaseConsolSeaCargoLock(this BusinessObjectFactory factory, ZGuid consolPK)
		{
			factory.UnlockChild(MutexIDs.CusSCAOceanBillJobBeingCreatedForConsol, consolPK, Core.Constants.CountryCodes.NewZealand);
		}

		#endregion
	}
}
