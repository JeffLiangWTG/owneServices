using System;
using System.ComponentModel;
using Enterprise.Rating.GUI.RateSelection;

namespace Enterprise.Rating.GUI.RateSelector.Models
{
	public class SortOptionViewModel : ViewModelWithNotificationBase, IDisposable
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Unicode character")]
		const string UpArrowCharacter = "\u2B61";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Unicode character")]
		const string DownArrowCharacter = "\u2B63";

		public string PropertyName { get; set; }

		public string DisplayName { get; set; }

		public bool IsSelected
		{
			get => isSelected;
			set
			{
				isSelected = value;
				OnPropertyChanged(nameof(DisplayNameWithDirection));
			}
		}
		bool isSelected;

		public ListSortDirection Direction
		{
			get => direction;
			set
			{
				direction = value;
				OnPropertyChanged(nameof(DisplayNameWithDirection));
			}
		}
		ListSortDirection direction;

		public string DisplayNameWithDirection
		{
			get
			{
				if (IsSelected)
				{
					return $"{DisplayName} {(Direction == ListSortDirection.Ascending ? UpArrowCharacter : DownArrowCharacter)}";
				}

				return DisplayName;
			}
		}

		public void Dispose()
		{
		}
	}
}
