using System;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.DataRegistry.Business.Testing
{
	sealed class DeclarationEventLockInfoLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				var declarationEventLockInfo = new DeclarationEventLockInfo();
				AssertExceptionThrown<ArgumentNullException>("factory can not be null", () => new DeclarationEventLockInfoLookups(declarationEventLockInfo, null));
				AssertNoExceptionThrown("No error", () => new DeclarationEventLockInfoLookups(declarationEventLockInfo, Factory));
			});
		}

		public void TestEventTypeList()
		{
			var info = new DeclarationEventLockInfo();
			var list = info.Lookups.EventTypeList;

			AssertEquals(typeof(StmEventCodeDescriptionPairList), list.GetType());

			var codes = list.GetAllCodes();
			AssertCollectionNotContains(AutoEvents.AddedARecordToTheSystemCode, codes);
			AssertCollectionNotContains(AutoEvents.EditedARecordCode, codes);
			AssertCollectionNotContains(AutoEvents.DeletedARecordInTheSystemCode, codes);
			AssertCollectionNotContains(AutoEvents.LockForEditCode, codes);
			AssertCollectionNotContains(AutoEvents.UnlockForEditCode, codes);
			AssertCollectionNotContains(AutoEvents.StaffFlaggedAsDeviceOnlyCode, codes);
			AssertCollectionNotContains(AutoEvents.StaffUnFlaggedAsDeviceOnlyCode, codes);
			AssertCollectionNotContains(AutoEvents.UserSeatCode, codes);
		}

		public void TestEventTypeList_Cache()
		{
			var oldValue = ZArchitecture.Environment.DataRegistry.Instance.ProductivityWiseModeEnabled;
			using (new DisposableAction(() => ZArchitecture.Environment.DataRegistry.Instance.ProductivityWiseModeEnabled = oldValue))
			{
				CombineAssertions(() =>
				{
					ZArchitecture.Environment.DataRegistry.Instance.ProductivityWiseModeEnabled = true;

					var info = new DeclarationEventLockInfo();
					var list = info.Lookups.EventTypeList;
					AssertSame("EventTypeList Cached in Factory and same list for the same instance", list, info.Lookups.EventTypeList);

					var info2 = new DeclarationEventLockInfo();
					var list2 = info2.Lookups.EventTypeList;
					AssertSame("EventTypeList Cached in Factory and same list for different instance in the same Factory", list, list2);

					ZArchitecture.Environment.DataRegistry.Instance.ProductivityWiseModeEnabled = false;
					var list3 = info2.Lookups.EventTypeList;
					AssertNotSame("New EventTypeList with new key", list, list3);
				});
			}
		}

		public void TestEventSourceList()
		{
			var info = new DeclarationEventLockInfo();
			var actualCodes = info.Lookups.EventSourceList.GetAllCodes();
			var expectedCodes = new[] { Constants.Customs.EventLockSourceTypes.Codes.Declaration, Constants.Customs.EventLockSourceTypes.Codes.EntryHeader };

			AssertContainsExactElementsInAnyOrder("Default declaration type", expectedCodes, actualCodes);

			foreach (var declarationType in new[] { "ARN", "DEP", "ULR" })
			{
				var config = new DeclarationLockConfig();
				config.DeclarationType = declarationType;
				info = config.EventInfos.AddNew();
				actualCodes = info.Lookups.EventSourceList.GetAllCodes();
				expectedCodes = new[] { Constants.Customs.EventLockSourceTypes.Codes.NctsHeader };

				AssertContainsExactElementsInAnyOrder($"Declaration type: {declarationType}", expectedCodes, actualCodes);
			}

			foreach (var declarationType in new[] { "TST" })
			{
				var config = new DeclarationLockConfig();
				config.DeclarationType = declarationType;
				info = config.EventInfos.AddNew();
				actualCodes = info.Lookups.EventSourceList.GetAllCodes();
				expectedCodes = new[] { Constants.Customs.EventLockSourceTypes.Codes.TemporaryStorage };

				AssertContainsExactElementsInAnyOrder($"Declaration type: {declarationType}", expectedCodes, actualCodes);
			}
		}

		public void TestEntryTypeList()
		{
			var fallbackLevel = new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK);
			var eventLockInfo = new DeclarationEventLockInfo(fallbackLevel, Factory);

			var list = eventLockInfo.Lookups.EntryTypeList;
			Assert("Should always contains 'ALL'.", list.ContainsCode(Core.Constants.Customs.EntryHeaderTypes.Codes.All));
		}
	}
}
