using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Enterprise.MarketingManager.GUI
{
	public abstract class ViewModelBase : INotifyPropertyChanged
	{
		protected ViewModelBase()
		{
		}

		protected ViewModelBase(params INotifyPropertyChanged[] childNotifiers)
		{
#if !WINZOR
			foreach (var child in childNotifiers.Where(c => c != null))
			{
				/*
				 * This bit of memory leak preventing magic comes from here:
				 * https://msdn.microsoft.com/en-us/library/aa970850%28v=vs.110%29.aspx
				 * 
				 * 'propertyName' specified as string.Empty means all property changes will trigger an event.
				 */
				PropertyChangedEventManager.AddHandler(child, new EventHandler<PropertyChangedEventArgs>(OnPropertyOfChildChanged), propertyName: string.Empty);
			}
#endif
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference")]
		protected void Set<T>(ref T currentValue, T newValue, [CallerMemberName] string propertyName = "")
		{
			if (!EqualityComparer<T>.Default.Equals(currentValue, newValue))
			{
				currentValue = newValue;
				OnPropertyChanged(propertyName);
			}
		}

		void OnPropertyOfChildChanged(object sender, PropertyChangedEventArgs e)
		{
			if (sender != this)
			{
				OnPropertyChanged(e.PropertyName);
			}
		}

		protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
		{
			NotifyPropertyChangedCore(propertyName);
		}

		void NotifyPropertyChangedCore(string propertyName)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));

				foreach (var wrappingProperty in GetWrappingProperties(propertyName))
				{
					PropertyChanged(this, new PropertyChangedEventArgs(wrappingProperty));
				}
			}
		}

		protected virtual IEnumerable<string> GetWrappingProperties(string propertyName)
		{
			yield break;
		}

		public event PropertyChangedEventHandler PropertyChanged;
	}
}
