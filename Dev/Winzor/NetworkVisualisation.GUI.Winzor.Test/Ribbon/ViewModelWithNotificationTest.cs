using System.ComponentModel;
using CargoWise.NetworkVisualisation.GUI;

namespace NetworkVisualisation.GUI.Winzor.Test;

class ViewModelWithNotificationTest
{
	[Test]
	public void ViewModelWithNotificationFiresPropertyChangedEvent()
	{
		var viewModelWithNotification = new ViewModelWithNotificationForTest();
		PropertyChangedEventArgs? eventArgs = null;
		viewModelWithNotification.PropertyChanged += (sender, args) => eventArgs = args;

		Assert.That(eventArgs, Is.Null);
		viewModelWithNotification.OnPropertyChangedForTest("SomeProperty");
		Assert.That(eventArgs, Is.Not.Null);
		Assert.That(eventArgs!.PropertyName, Is.EqualTo("SomeProperty"));
	}

	[Test]
	public void ViewModelWithNotificationThrowsExceptionIfPropertyNameIsNull()
	{
		var viewModelWithNotification = new ViewModelWithNotificationForTest();
		Assert.Throws<ArgumentException>(() =>
		{
			viewModelWithNotification.OnPropertyChangedForTest(string.Empty);
		});
	}

	class ViewModelWithNotificationForTest : ViewModelWithNotification
	{
		public void OnPropertyChangedForTest(string name) => OnPropertyChanged(name);
	}
}
