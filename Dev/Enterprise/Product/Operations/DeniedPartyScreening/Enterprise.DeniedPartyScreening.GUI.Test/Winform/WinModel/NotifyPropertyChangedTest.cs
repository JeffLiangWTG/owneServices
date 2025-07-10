using NUnit.Framework;

namespace Enterprise.DeniedPartyScreening.GUI.Test
{
	class NotifyPropertyChangedTest : TestCase
	{
		public void TestInvoke()
		{
			var callCount = 0;
			var notifyPropertyChanged = new NotifyPropertyChanged();
			notifyPropertyChanged.ChangedActions.Add(() => { callCount++; notifyPropertyChanged.Invoke(); });
			notifyPropertyChanged.Invoke();
			AssertEquals("Prevent infinite loop", 1, callCount);
		}
	}
}
