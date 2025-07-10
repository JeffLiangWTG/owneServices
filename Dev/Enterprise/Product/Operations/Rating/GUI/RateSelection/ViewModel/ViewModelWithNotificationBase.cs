using System.ComponentModel;
using CargoWise.Common;

namespace Enterprise.Rating.GUI.RateSelection
{
	public abstract class ViewModelWithNotificationBase : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		protected void OnPropertyChanged(string name)
		{
			Argument.NotNullOrEmpty(name, nameof(name));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}
	}
}
