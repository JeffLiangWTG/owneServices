using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Environment.Business.Testing
{
	class LocationStringReloaderServiceTest : TestCaseWithFactory
	{
		#region TestAddLocationReloaderService

		public void TestAddLocationReloaderService()
		{
			var dummyBizO = Factory.New<ReloadLocationStringDummy>();
			Factory.Save();

			AssertNull("Precondition.", Factory.ServiceContainer.GetAfterOnSavingService<LocationViewReloaderService>());
			LocationViewReloaderService.AddLocationReloaderService(Factory);

			dummyBizO.Z0_Description = "Some column will not cause location to reload.";
			Factory.Save();
			AssertEquals("Reload Location should not happen.", false, dummyBizO.ReloadLocationSuccessful);

			dummyBizO.Z0_Code = "ABC";
			Factory.Save();
			AssertEquals("Reload Location should happen.", true, dummyBizO.ReloadLocationSuccessful);
		}

		public void TestAddLocationReloaderService_NullFactory()
		{
			AssertExceptionThrown<ArgumentNullException>(() => LocationViewReloaderService.AddLocationReloaderService(null));
		}

		public void TestAddLocationReloaderService_AddedTwice()
		{
			AssertNull("Precondition.", Factory.ServiceContainer.GetAfterOnSavingService<LocationViewReloaderService>());
			LocationViewReloaderService.AddLocationReloaderService(Factory);

			var service = Factory.ServiceContainer.GetAfterOnSavingService<LocationViewReloaderService>();
			AssertNotNull("Should have added reloader service.", service);

			AssertNoExceptionThrown(() => LocationViewReloaderService.AddLocationReloaderService(Factory));
			AssertEquals("Should not have changed reloader service.", service, Factory.ServiceContainer.GetAfterOnSavingService<LocationViewReloaderService>());
		}

		#endregion

		#region TestLocationStringAffectService_ReloadIfNotInDB

		public void TestLocationStringAffectService_ReloadIfNotInDB()
		{
			var dummyBizO = Factory.New<ReloadLocationStringDummy>();

			LocationViewReloaderService.AddLocationReloaderService(Factory);

			dummyBizO.Z0_Description = "Some column will not cause location to reload.";
			Factory.Save();
			AssertEquals("Reload Location should happen.", true, dummyBizO.ReloadLocationSuccessful);
		}

		#endregion

		#region TestLocationStringAffectService_DoNotReloadIfParentAlreadyReload

		public void TestLocationStringAffectService_DoNotReloadIfParentAlreadyReload()
		{
			var parentDummyBizO = Factory.New<ReloadLocationStringDummy>();
			var childDummyBizO = Factory.New<ReloadLocationStringDummy>();
			childDummyBizO.ParentThatMayReloadMyLocations = parentDummyBizO.PK;
			Factory.Save();

			LocationViewReloaderService.AddLocationReloaderService(Factory);

			parentDummyBizO.Z0_Code = "ABC";
			childDummyBizO.Z0_Code = "ABC";
			Factory.Save();
			AssertEquals("Reload Location should happen.", true, parentDummyBizO.ReloadLocationSuccessful);
			AssertEquals("Location is already reloaded with parent object reloading.", false, childDummyBizO.ReloadLocationSuccessful);
			AssertEquals("Reload Location should happen 1 time for parent object.", 1, parentDummyBizO.ReloadCount);
			AssertEquals("Location is already reloaded with parent object reloading.", 0, childDummyBizO.ReloadCount);
		}

		#endregion

		class ReloadLocationStringDummy : DummyBusinessObject, IAffectLocationView
		{
			internal ZBool ReloadLocationSuccessful => ReloadCount > 0;
			internal ZInt ReloadCount;

			public ReloadLocationStringDummy(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public SchemaColumn[] GetColumnsThatAffectLocationView()
			{
				return new SchemaColumn[]
				{
					DummyBizoSchema.Z0_Code
				};
			}

			public ZGuid ParentThatMayReloadMyLocations { get; set; }

			public void ReloadLocationsFromDB()
			{
				ReloadCount++;
			}
		}
	}
}
