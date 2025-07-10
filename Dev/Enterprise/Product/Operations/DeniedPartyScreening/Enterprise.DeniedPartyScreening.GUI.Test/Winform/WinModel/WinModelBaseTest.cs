using System;
using NUnit.Framework;

namespace Enterprise.DeniedPartyScreening.GUI.Test
{
	class WinModelBaseTest : TestCase
	{
		public void TestRegisterNotifyPropertyChangeEventAndTestNotifyPropertyChanged()
		{
			var count = 0;
			Action action1 = () => count++;

			var model = new WinModelBaseForTest();
			model.RegisterNotifyPropertyChangeEvent("Dummy", action1, false);

			model.NotifyPropertyChangedExposed("Dummy");
			AssertEquals(1, count);

			model.NotifyPropertyChangedExposed("Dummy2");
			AssertEquals(1, count);

			Action action2 = () => count++;
			model.RegisterNotifyPropertyChangeEvent("Dummy", action2, false);
			model.NotifyPropertyChangedExposed("Dummy");
			AssertEquals(3, count);

			Action action3 = () => count++;
			model.RegisterNotifyPropertyChangeEvent("Dummy", action3, true);
			model.NotifyPropertyChangedExposed("Dummy");
			AssertEquals("Previous Events has been cleared", 4, count);
		}

		public void TestClearNotifyPropertyChangeEvent()
		{
			var count = 0;
			Action action1 = () => count++;
			var model = new WinModelBaseForTest();
			model.RegisterNotifyPropertyChangeEvent("Dummy", action1, false);
			model.NotifyPropertyChangedExposed("Dummy");
			AssertEquals(1, count);

			model.ClearNotifyPropertyChangeEvent("Dummy");
			model.NotifyPropertyChangedExposed("Dummy");
			AssertEquals("Event is cleared", 1, count);
		}
	}

	class WinModelBaseForTest : WinModelBase
	{
		public void NotifyPropertyChangedExposed(string property) => NotifyPropertyChanged(property);
	}
}
