using CargoWise.EntityFramework;

namespace Enterprise.Customs.NZ.Business.Express.Testing
{
	using System;
	using CargoWise.Common;
	using CargoWise.EntityFramework.Testing;
	using Enterprise.Freight.Forwarding.Business;

	public class FactoryExtensionsTest : TestCaseWithFactory
	{
		public static IDisposable SetHVLVAccess(BusinessObjectFactory factory, bool value)
		{
			var rawValue = Enterprise.Registry.Business.HVLVDataRegistry.HasHVLVClearance;
			Enterprise.Registry.Business.HVLVDataRegistry.HasHVLVClearance = value;
			return new DisposableAction(() => { Enterprise.Registry.Business.HVLVDataRegistry.HasHVLVClearance = rawValue; });
		}

		public void TestHasHVLVAccess()
		{
			using (SetHVLVAccess(Factory, value: false))
			{
				Assert(!Factory.HasHVLVAccess());
			}
			using (SetHVLVAccess(Factory, value: true))
			{
				Assert(Factory.HasHVLVAccess());
			}
		}

		public void TestGetSeaCargoMutexLock()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00003275";
			AssertEquals("GetSeaCargoMutexLock", "Someone else", Factory.GetSeaCargoMutexLock(consol.PK));
		}

		public void TestLockAndReleaseSeaCargoMutex()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00003434";
			var shipment = consol.Shipments.AddNew();
			shipment.JS_UniqueConsignRef = "S00006565";
			Assert(Factory.LockSeaCargoMutex(consol.PK));
			var newFactory = new BusinessObjectFactory();
			Assert(!newFactory.LockSeaCargoMutex(consol.PK));
			Factory.ReleaseConsolSeaCargoLock(consol.PK);
			Assert(newFactory.LockSeaCargoMutex(consol.PK));
			newFactory.ReleaseConsolSeaCargoLock(consol.PK);
		}
	}
}
