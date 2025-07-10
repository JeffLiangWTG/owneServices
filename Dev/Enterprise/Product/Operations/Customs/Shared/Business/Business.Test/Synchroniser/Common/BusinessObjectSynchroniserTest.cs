using System;
using System.Linq;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.Business.Testing
{
	sealed class BusinessObjectSynchroniserTest : SynchroniserTestCase
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentNullException>("destination is null", () => new TestBusinessObjectSynchroniser(null, Factory.New<DummyCusCodeData>()));
				AssertExceptionThrown<ArgumentNullException>("source is null", () => new TestBusinessObjectSynchroniser(Factory.New<DummyCusCodeData>(), null));
			});
		}

		public void TestHookSynchronisersWillNotRun_IfDestinationIsDeleted()
		{
			var source = Factory.New<DummyCusCodeData>();
			var destination = Factory.New<DummyCusCodeData>();
			var synchroniser = new TestBusinessObjectSynchroniser(destination, source);
			destination.Delete();
			synchroniser.HookEvents_Exposed();
			AssertEquals(false, synchroniser.IsHookSynchronisersCalled);
		}

		public void TestHookSynchronisersWillNotRun_IfSourceIsDeleted()
		{
			var source = Factory.New<DummyCusCodeData>();
			var destination = Factory.New<DummyCusCodeData>();
			var synchroniser = new TestBusinessObjectSynchroniser(destination, source);
			source.Delete();
			synchroniser.HookEvents_Exposed();
			AssertEquals(false, synchroniser.IsHookSynchronisersCalled);
		}

		public void TestForceSynchroniseCore()
		{
			var source = Factory.New<DummyCusCodeData>();
			var destination = Factory.New<DummyCusCodeData>();
			var synchroniser = new TestBusinessObjectSynchroniser(destination, source);
			var codeSynchroniser = new FieldSynchroniser(source.CY_CodeInfo, destination.CY_CodeInfo);
			var dataSynchroniser = new FieldSynchroniser(source.CY_DataInfo, destination.CY_DataInfo);
			synchroniser.SynchronisersExposed.Add(codeSynchroniser);
			synchroniser.SynchronisersExposed.Add(dataSynchroniser);
			source.Delete();
			synchroniser.ForceSynchroniseCoreExposed();
			Assert("ForceSynchroniseCore should clear all synchronisers in the list if Source or Destination is deleted.", !synchroniser.SynchronisersExposed.Any());
		}
	}
}
