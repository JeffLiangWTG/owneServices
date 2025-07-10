using System;
using System.ComponentModel;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.GUI.RateSelection.BaseControls
{
	public class ViewModelBasedControl : ZUserControl
	{
		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			base.OnCurrentDataItemChanging(e);
			if (CurrentDataItem != null && CurrentDataItem is INotifyPropertyChanged viewModel)
			{
				viewModel.PropertyChanged -= ViewModelPropertiesChanged;
			}
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);

			if (!IsDisposing)
			{
				if (CurrentDataItem != null && CurrentDataItem is INotifyPropertyChanged viewModel)
				{
					viewModel.PropertyChanged += ViewModelPropertiesChanged;
				}

				ApplyExtraStaticOneWayBindings();
			}
		}

		protected virtual void ApplyExtraStaticOneWayBindings()
		{
		}

		protected virtual void ViewModelPropertiesChanged(object sender, PropertyChangedEventArgs e)
		{
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (CurrentDataItem is INotifyPropertyChanged viewModel)
				{
					viewModel.PropertyChanged -= ViewModelPropertiesChanged;
				}
			}

			base.Dispose(disposing);
		}
	}
}
