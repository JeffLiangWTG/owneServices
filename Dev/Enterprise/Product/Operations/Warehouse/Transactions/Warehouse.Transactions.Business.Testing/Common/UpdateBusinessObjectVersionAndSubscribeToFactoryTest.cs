using System;
using CargoWise.Types;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class UpdateBusinessObjectVersionAndSubscribeToFactoryTest : WhsTestCaseWithFactory
	{
		public void TestUpdateBusinessObjectVersionAndSubscribeToFactory_ValidArgument()
		{
			var dummyBizO = Factory.New<DummyWithVersionID>();
			Factory.Save();

			Assert("Precondition", dummyBizO.IsInDatabase);
			var versionID = dummyBizO.CriticalChangesVersionID;

			UpdateBusinessObjectCriticalChangesVersionIDHelper<DummyWithVersionID>
				.UpdateBusinessObjectVersionAndSubscribeToFactory(Factory, dummyBizO.PK);

			Factory.Save();

			var updatedVersionID = dummyBizO.CriticalChangesVersionID;
			AssertNotEquals("VersionID should be updated.", versionID, updatedVersionID);
		}

		public void TestUpdateBusinessObjectVersionAndSubscribeToFactory_InvalidArgument()
		{
			AssertExceptionThrown<ArgumentException>("BusinessObject PK should be valid", "BusinessObject PK should be valid.",
				() => UpdateBusinessObjectCriticalChangesVersionIDHelper<DummyWithVersionID>.UpdateBusinessObjectVersionAndSubscribeToFactory(NewFactory(),
					ZGuid.Empty));

			AssertExceptionThrown<ArgumentException>("BusinessObject PK should be valid", "BusinessObject PK should be valid.",
				() => UpdateBusinessObjectCriticalChangesVersionIDHelper<DummyWithVersionID>.UpdateBusinessObjectVersionAndSubscribeToFactory(NewFactory(),
					ZGuid.Invalid));

			AssertExceptionThrown<ArgumentNullException>(() =>
				UpdateBusinessObjectCriticalChangesVersionIDHelper<DummyWithVersionID>.UpdateBusinessObjectVersionAndSubscribeToFactory(null,
					ZGuid.NewZGuid()));

			AssertNoExceptionThrown(() =>
				UpdateBusinessObjectCriticalChangesVersionIDHelper<DummyWithVersionID>.UpdateBusinessObjectVersionAndSubscribeToFactory(NewFactory(),
					ZGuid.NewZGuid()));
		}
	}
}
