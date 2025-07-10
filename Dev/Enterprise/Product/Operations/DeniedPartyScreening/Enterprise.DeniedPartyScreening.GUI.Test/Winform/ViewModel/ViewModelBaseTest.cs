using NUnit.Framework;

namespace Enterprise.DeniedPartyScreening.GUI.Test
{
	class ViewModelBaseTest : TestCase
	{
		public void TestSetValue()
		{
			bool propertyChangedCalled = false;
			var testViewModel = new TestViewModel();
			testViewModel.PropertyChanged += (sender, args) => propertyChangedCalled = args.PropertyName == nameof(TestViewModel.PropertyToTest);
			AssertEquals(testViewModel.PropertyToTest, 0);
			testViewModel.SetProperty();
			AssertEquals(testViewModel.PropertyToTest, 10);
			AssertEquals(propertyChangedCalled, true);
		}
	}

	class TestViewModel : ViewModelBase<TestViewModel>
	{
		public int PropertyToTest { get; set; }

		public void SetProperty()
		{
			SetValue(() => PropertyToTest, 10);
		}
	}
}
