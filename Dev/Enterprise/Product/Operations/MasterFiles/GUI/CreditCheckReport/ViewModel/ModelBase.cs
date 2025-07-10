using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Enterprise.MasterFiles.GUI
{
	public abstract class ModelBase<TViewModel> : INotifyPropertyChanged where TViewModel : class
	{
		public event PropertyChangedEventHandler PropertyChanged;

		protected void NotifyPropertyChanged([CallerMemberName] string property = null)
		{
			if (!string.IsNullOrEmpty(property))
			{
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
			}
		}

#if DEBUG

		internal int GetEventHandlerCount()
		{
			var count = PropertyChanged?.GetInvocationList().Length ?? 0;
			return count;
		}

#endif
	}
}
