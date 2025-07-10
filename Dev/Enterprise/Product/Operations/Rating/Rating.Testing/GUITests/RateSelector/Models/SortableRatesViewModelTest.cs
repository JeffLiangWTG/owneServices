using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.Rating.GUI.RateSelector.Models;

namespace Enterprise.Rating.Testing.GUITests.RateSelector
{
	public class SortableRatesViewModelTest : RatingTestCase
	{
		public void TestSortedRatesInOrderAfterReSort()
		{
			using (var viewModel = CreateSortableRatesViewModel())
			{
				var numberSortOption = viewModel.SupportedSortOptions.FirstOrDefault(x => x.PropertyName == nameof(RateViewModelForTest.NumberProperty));
				var dateTimeSortOption = viewModel.SupportedSortOptions.FirstOrDefault(x => x.PropertyName == nameof(RateViewModelForTest.DateTimeProperty));
				numberSortOption.IsSelected = false;
				dateTimeSortOption.IsSelected = false;

				numberSortOption.IsSelected = true;
				viewModel.ReSort();
				AssertContainsExactElementsInExactOrder(
					"Sorted by NumberProperty Ascending by default",
					viewModel.Rates.OrderBy(s => s.NumberProperty).Select(s => s.NumberProperty),
					viewModel.SortedRates.Cast<RateViewModelForTest>().Select(s => s.NumberProperty));

				numberSortOption.Direction = System.ComponentModel.ListSortDirection.Descending;
				viewModel.ReSort();
				AssertContainsExactElementsInExactOrder(
					"Resort by NumberProperty will sort by Descending order",
					viewModel.Rates.OrderByDescending(s => s.NumberProperty).Select(s => s.NumberProperty),
					viewModel.SortedRates.Cast<RateViewModelForTest>().Select(s => s.NumberProperty));

				numberSortOption.IsSelected = false;
				dateTimeSortOption.IsSelected = true;
				viewModel.ReSort();
				AssertContainsExactElementsInExactOrder(
					"Sorted by Ascending the first time",
					viewModel.Rates.OrderBy(s => s.DateTimeProperty).Select(s => s.DateTimeProperty),
					viewModel.SortedRates.Cast<RateViewModelForTest>().Select(s => s.DateTimeProperty));

				dateTimeSortOption.Direction = System.ComponentModel.ListSortDirection.Descending;
				viewModel.ReSort();
				AssertContainsExactElementsInExactOrder(
					"Resorted by DateTimeProperty will sort by Descending order",
					viewModel.Rates.OrderByDescending(s => s.DateTimeProperty).Select(s => s.DateTimeProperty),
					viewModel.SortedRates.Cast<RateViewModelForTest>().Select(s => s.DateTimeProperty));
			}
		}

		public void TestSortedRatesPropertyChangedAfterReSort()
		{
			using (var viewModel = CreateSortableRatesViewModel())
			{
				string propertyChanged = null;
				viewModel.PropertyChanged += (sender, e) =>
				{
					propertyChanged = e.PropertyName;
				};
				viewModel.ReSort();

				AssertEquals("SortedRates should have been updated", nameof(viewModel.SortedRates), propertyChanged);
			}
		}

		SortableRatesViewModelForTest CreateSortableRatesViewModel()
		{
			var today = DateTime.Today;
			return new SortableRatesViewModelForTest(new[]
			{
				new RateViewModelForTest
				{
					NumberProperty = 10.5m,
					DateTimeProperty = today
				},
				new RateViewModelForTest
				{
					NumberProperty = 10.8m,
					DateTimeProperty = today.AddDays(1)
				},
				new RateViewModelForTest
				{
					NumberProperty = 9.9m,
					DateTimeProperty = today.AddDays(2)
				},
			});
		}

		class SortableRatesViewModelForTest : SortableRatesViewModel
		{
			internal SortableRatesViewModelForTest(IEnumerable<RateViewModelForTest> rates)
			{
				this.rates = rates;
				SupportedSortOptions.Add(new SortOptionViewModel { PropertyName = nameof(RateViewModelForTest.NumberProperty) });
				SupportedSortOptions.Add(new SortOptionViewModel { PropertyName = nameof(RateViewModelForTest.DateTimeProperty) });
				SetDefaultSortOption();
			}

			public IEnumerable<RateViewModelForTest> Rates => rates;

			public override string DefaultSortOptionPropertyName => nameof(RateViewModelForTest.NumberProperty);

			protected override object GetRatesSource() => Rates;

			readonly IEnumerable<RateViewModelForTest> rates;

			protected override object GetSortProperty(object o, SortOptionViewModel selectedSort)
			{
				var vm = o as RateViewModelForTest;

				switch (selectedSort.PropertyName)
				{
					case nameof(vm.TotalPriceAmount):
						return vm.TotalPriceAmount;
					case nameof(vm.NumberProperty):
						return vm.NumberProperty;
					case nameof(vm.DateTimeProperty):
						return vm.DateTimeProperty;
					default:
						throw new ArgumentException($"Unexpected {nameof(selectedSort.PropertyName)}: {selectedSort.PropertyName}", nameof(selectedSort));
				}
			}
		}

		class RateViewModelForTest : RateViewModel
		{
			public override object Clone()
			{
				throw new NotImplementedException();
			}

			public override IEnumerable<AutoRateInfo> GetAutoRateInfos()
			{
				throw new NotImplementedException();
			}

			protected override void SetCharges(IEnumerable<AutoRateInfo> autoRateInfoCollection, string logs)
			{
				throw new NotImplementedException();
			}

			public decimal NumberProperty
			{
				get => numberProperty;
				set => numberProperty = value;
			}

			public DateTime DateTimeProperty
			{
				get => dateTimeProperty;
				set => dateTimeProperty = value;
			}

			decimal numberProperty;
			DateTime dateTimeProperty;
		}
	}
}
