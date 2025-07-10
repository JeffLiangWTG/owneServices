using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class UpdateCriticalChangesVersionIDServiceProviderTest : WhsTestCaseWithFactory
	{
		public void TestConstractor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => _ = new UpdateCriticalChangesVersionIDServiceProviderForTesting(null));
		}

		public void TestRegisterBusinessObjectPK()
		{
			var provider = new UpdateCriticalChangesVersionIDServiceProviderForTesting(Factory);
			AssertExceptionThrown<ArgumentException>(() => provider.RegisterBusinessObjectPK(ZGuid.Empty));
			AssertExceptionThrown<ArgumentException>(() => provider.RegisterBusinessObjectPK(ZGuid.Invalid));
			AssertNoExceptionThrown(() => provider.RegisterBusinessObjectPK(ZGuid.NewZGuid()));
		}

		public void TestClearPKs()
		{
			var provider = new UpdateCriticalChangesVersionIDServiceProviderForTesting(Factory);
			Factory.ServiceContainer.AddAfterOnSavingService(provider);
			var bizo = Factory.New<DummyWithVersionID>();
			AssertEquals("Precondition", true, bizo.PK.IsValid);
			provider.RegisterBusinessObjectPK(bizo.PK);
			AssertEquals("Precondition", false, provider.PKsCleared);
			AssertEquals("Precondition", false, bizo.IsInDatabase);

			Factory.Save();
			AssertEquals("Should clear PKs", true, provider.PKsCleared);
			AssertEquals("When is not in DB we do not need set version ID.", true, bizo.CriticalChangesVersionID.IsEmpty);
		}

		public void TestSetCriticalChangesVersionIDBasedOnIsImmutableStatus()
		{
			var provider = new UpdateCriticalChangesVersionIDServiceProviderForTesting(Factory);
			var bizo = Factory.New<DummyWithVersionID>();
			Factory.Save();
			Factory.ServiceContainer.AddAfterOnSavingService(provider);
			AssertEquals("Precondition", true, bizo.IsInDatabase);
			provider.RegisterBusinessObjectPK(bizo.PK);
			AssertEquals("Precondition", false, bizo.IsImmutableStatus);

			Factory.Save();
			AssertEquals("When object is in ImmutableStatus need version ID.", true, bizo.CriticalChangesVersionID.IsValid);

			bizo.IsImmutableStatus = true;
			provider.RegisterBusinessObjectPK(bizo.PK);
			AssertEquals("Precondition", true, bizo.IsImmutableStatus);

			Factory.Save();
			AssertEquals("When object is in ImmutableStatus do not need version ID.", true, bizo.CriticalChangesVersionID.IsEmpty);
		}
	}

	class DummyWithVersionID : DummyBusinessObject, ICriticalChangesVersionID
	{
		public DummyWithVersionID(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public ZGuid CriticalChangesVersionID { get; set; }

		public bool IsImmutableStatus { get; set; }
	}

	class UpdateCriticalChangesVersionIDServiceProviderForTesting : UpdateCriticalChangesVersionIDServiceProvider<DummyWithVersionID>
	{
		public UpdateCriticalChangesVersionIDServiceProviderForTesting(BusinessObjectFactory factory) : base(factory)
		{
			PKsCleared = false;
		}

		protected override void ClearPKsCore()
		{
			PKsCleared = true;
		}
		public bool PKsCleared;
	}
}
