using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using CargoWise.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.GUI.RateSelection;

namespace Enterprise.Rating.GUI.RateSelector.Models
{
	public class ChargesViewModel : ViewModelWithNotificationBase, IDisposable
	{
		public ChargesViewModel(string groupName)
		{
			GroupName = Argument.NotNullOrEmpty(groupName, nameof(groupName));
			//TOOD: Add disposable leak listener to make sure this is disposed
			//      and event handles cleaned up
			//      DisposableLeakListener.Instance.RegisterDisposable(this) here
			//      DisposableLeakListener.Instance.UnRegisterDisposable(this) on the dispose method
		}

		#region Properties

		public string GroupName { get; set; }

		public bool IsValid => Charges.Where(c => c.IsSelected).All(c => c.IsValid);

		public string TotalPriceError
		{
			get
			{
				var errors = Charges
					.Where(c => c.IsSelected)
					.Select(c => c.LocalAmountError)
					.Where(error => !string.IsNullOrEmpty(error))
					.Distinct()
					.ToArray();

				if (errors.Any())
				{
					return string.Join(System.Environment.NewLine, errors);
				}

				return string.Empty;
			}
		}

		public ErrorLevel TotalPriceErrorLevel
		{
			get
			{
				return string.IsNullOrEmpty(TotalPriceError) ? ErrorLevel.None : ErrorLevel.Warning;
			}
		}

		public decimal TotalPrice => Charges.Where(c => c.IsSelected).Sum(c => c.LocalAmount);

		public string TotalPriceString
		{
			get
			{
				var money = new Money(TotalPrice, GlbCompany.CurrentCompany.LocalCurrency);
				return money.ToString();
			}
		}

		public virtual IEnumerable<ChargeViewModel> Charges => charges;

		#endregion

		#region IDisposable

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				foreach (var charge in Charges)
				{
					charge.PropertyChanged -= ChargePropertyChanged;
				}
			}
		}

		#endregion

		public void Add(ChargeViewModel charge)
		{
			Argument.NotNull(charge, nameof(charge));
			charge.PropertyChanged += ChargePropertyChanged;
			charges.Add(charge);
			NotifyChanged();
		}

		public void Clear()
		{
			foreach (var charge in charges)
			{
				charge.PropertyChanged -= ChargePropertyChanged;
			}

			charges.Clear();
			NotifyChanged();
		}

		void ChargePropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(ChargeViewModel.IsSelected) || e.PropertyName == nameof(ChargeViewModel.LocalAmountError))
			{
				NotifyChanged();
			}
			else if (e.PropertyName == nameof(ChargeViewModel.IsValid))
			{
				OnPropertyChanged(nameof(IsValid));
			}
		}

		void NotifyChanged()
		{
			OnPropertyChanged(nameof(IsValid));
			OnPropertyChanged(nameof(TotalPrice));
			OnPropertyChanged(nameof(TotalPriceError));
			OnPropertyChanged(nameof(TotalPriceString));
		}

		readonly ObservableCollection<ChargeViewModel> charges = new ObservableCollection<ChargeViewModel>();
	}
}
