using System.ComponentModel;
using Enterprise.Rating.Business.Testing;
using Enterprise.Rating.GUI.RateSelector.Models;

namespace Enterprise.Rating.Testing.GUITests.RateSelector
{
	public class SortOptionViewModelTest : RatingTestCase
	{
		public void TestPropertyChangesCalledWhenSettingIsSelected()
		{
			var viewModel = new SortOptionViewModel();
			var propertyName = string.Empty;

			viewModel.PropertyChanged += (sender, args) => propertyName = args.PropertyName;
			viewModel.IsSelected = true;
			AssertEquals(nameof(SortOptionViewModel.DisplayNameWithDirection), propertyName);
		}

		public void TestPropertyChangesCalledWhenSettingDirection()
		{
			var viewModel = new SortOptionViewModel();
			var propertyName = string.Empty;

			viewModel.PropertyChanged += (sender, args) => propertyName = args.PropertyName;
			viewModel.Direction = ListSortDirection.Ascending;
			AssertEquals(nameof(SortOptionViewModel.DisplayNameWithDirection), propertyName);
		}

		public void TestDisplayNameWithDirection()
		{
			const string displayName = "Price";

			void CreateAndAssert(bool isSelected, ListSortDirection direction, string expectedString)
			{
				var sortOption = new SortOptionViewModel
				{
					DisplayName = displayName,
					Direction = direction,
					IsSelected = isSelected
				};
				AssertEquals(expectedString, sortOption.DisplayNameWithDirection);
			}

			CreateAndAssert(false, ListSortDirection.Ascending, "Price");
			CreateAndAssert(false, ListSortDirection.Descending, "Price");
			CreateAndAssert(true, ListSortDirection.Ascending, "Price \u2B61");
			CreateAndAssert(true, ListSortDirection.Descending, "Price \u2B63");
		}
	}
}
