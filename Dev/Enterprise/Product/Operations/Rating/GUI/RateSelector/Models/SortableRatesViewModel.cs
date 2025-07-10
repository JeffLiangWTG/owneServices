using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.Common.Testing;
using Enterprise.Integration;
using Enterprise.Rating.Business.RateSelector;
using Enterprise.Rating.GUI.RateSelection;

namespace Enterprise.Rating.GUI.RateSelector.Models
{
	public abstract class SortableRatesViewModel : ViewModelWithNotificationBase, IDisposable
	{
		protected SortableRatesViewModel(MemoryLogger logger = null)
		{
			Logger = logger ?? new MemoryLogger();
			Logger.LogsChanged += Logger_LogsChanged;
			SupportedSortOptions.Add(new SortOptionViewModel
			{
				PropertyName = nameof(RateViewModel.TotalPriceAmount),
				DisplayName = Res.GetString("43805e45-96ec-47c6-bffa-e20e2b03cb6e", "Price"),
			});
			SetDefaultSortOption();

			DisposableLeakListener.Instance.RegisterDisposable(this);
		}

		protected void SetDefaultSortOption()
		{
			var priceSortOption = SupportedSortOptions.FirstOrDefault(x => x.PropertyName == DefaultSortOptionPropertyName);
			if (priceSortOption != null)
			{
				priceSortOption.IsSelected = true;
				ReSort();
			}
		}

		public List<SortOptionViewModel> SupportedSortOptions { get; } = new List<SortOptionViewModel>();

		public virtual bool ShowSortOptions => false;

		public virtual string DefaultSortOptionPropertyName => nameof(RateViewModel.TotalPriceAmount);

		protected abstract object GetRatesSource();

		protected abstract object GetSortProperty(object o, SortOptionViewModel selectedSort);

		readonly List<object> sortedRates = new List<object>();
		/// <summary>
		/// The output of <see cref="GetRatesSource()"/> sorted by the selected <see cref="SupportedSortOptions"/>
		/// </summary>
		/// <remarks>Regenerate this list via <see cref="ReSort"/></remarks>
		public IReadOnlyList<object> SortedRates => sortedRates;

		string statusText;
		public string StatusText
		{
			get => statusText;
			set
			{
				statusText = value;
				OnPropertyChanged(nameof(StatusText));
			}
		}

		public MemoryLogger Logger { get; }

		void Logger_LogsChanged(object sender, MemoryLogger.LogEventArgs e)
		{
			var lastLogLevel = e.LogAdded?.Level ?? LogType.Error;

			if (lastLogLevel == LogType.Warning || lastLogLevel == LogType.Error)
			{
				OnPropertyChanged(nameof(LogText));
				OnPropertyChanged(nameof(WarningsCount));
				OnPropertyChanged(nameof(WarningsCountText));
			}
		}

		IEnumerable<string> WarningLogs => Logger.Logs.Where(l => l.Level == LogType.Warning || l.Level == LogType.Error).Select(l => l.ToString());
		public int WarningsCount => WarningLogs.Count();
		public string LogText => string.Join("\r\n\r\n", WarningLogs);

		public string WarningsCountText => Res.GetString("6a0343fd-6195-4ef2-9cec-f34df1114449", "Errors / Warnings ({0})", WarningsCount);
		public string CardViewText => Res.GetString("a8b2cffa-0319-45bb-9aae-49113fb2d043", "Rate View");

		public void ReSort()
		{
			var unsortedRates = GetRatesSource() as IEnumerable<object>;
			if (unsortedRates is null)
			{
				return;
			}

			var selectedSort = SupportedSortOptions.FirstOrDefault(so => so.IsSelected);

			var sortedRates = unsortedRates;
			if (selectedSort != null)
			{
				var ascending = selectedSort.Direction == ListSortDirection.Ascending;
				sortedRates = unsortedRates.OrderBy(o => GetSortProperty(o, selectedSort));
				if (!ascending)
				{
					sortedRates = unsortedRates.OrderByDescending(o => GetSortProperty(o, selectedSort));
				}
			}

			this.sortedRates.Clear();
			this.sortedRates.AddRange(sortedRates);
			OnPropertyChanged(nameof(SortedRates));
		}

		public void Dispose()
		{
			Dispose(disposing: true);

			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				DisposableLeakListener.Instance.UnRegisterDisposable(this);

				Logger.LogsChanged -= Logger_LogsChanged;
				SupportedSortOptions.ForEach(sortOption => sortOption.Dispose());
			}
		}
	}
}
