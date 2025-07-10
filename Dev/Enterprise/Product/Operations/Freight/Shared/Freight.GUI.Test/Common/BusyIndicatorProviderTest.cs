using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.GUI.Testing
{
	sealed class BusyIndicatorProviderTest : TestCaseWithFactory
	{
		public void TestNewBusyIndicator_Is_ZWaitCursorChanger()
		{
			using (var busyIndicator = new BusyIndicatorProvider().NewBusyIndicator())
			{
				AssertEquals(true, busyIndicator is ZWaitCursorChanger);
			}
		}

		public void TestRegister_Factory()
		{
			AssertEquals("Prerequisite", null, Factory.GetValue<IBusyIndicatorProvider>());

			BusyIndicatorProvider.Register(Factory);
			AssertEquals(true, Factory.GetValue<IBusyIndicatorProvider>() is BusyIndicatorProvider);
		}

		public void TestRegister_ZForm()
		{
			AssertEquals("Prerequisite", null, Factory.GetValue<IBusyIndicatorProvider>());

			var dummy = Factory.New<DummyBusinessObject>();
			using (var form = new ZForm(dummy))
			{
				BusyIndicatorProvider.Register(form);
				AssertEquals(true, Factory.GetValue<IBusyIndicatorProvider>() is BusyIndicatorProvider);
			}
		}
	}
}
