using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using WTG.AddressCleansing.Common;

namespace Enterprise.MasterFiles.GUI.WebAddressValidation
{
	public partial class CityTownSuggestionControl : ZUserControl, ISuggestionControl
	{
		[SuppressMessage("CargoWiseOne", "CW1017", Justification = "Paras passed in is supposed to be scaled")]
		public CityTownSuggestionControl(
			ISupportWebAddressValidation address,
			CandidateCityTown[] cityTownSuggestions,
			string enteredCity,
			string enteredStateCode,
			string enteredPostcode,
			Form applicationForm,
			Control parentControl,
			Control referenceControl,
			int maxWidth = 0)
		{
			Address = address;
			Width = maxWidth;
			InitializeComponent();

			if (cityTownSuggestions != null && cityTownSuggestions.Length > 0)
			{
				SetupListView(cityTownSuggestions);
				SelectRecommendedAddress();
			}

			this.cityTownSuggestions = cityTownSuggestions;

			UpdateFilter(enteredCity, enteredStateCode, enteredPostcode);
			UpdateSize();
			UpdateLocation(applicationForm, parentControl, referenceControl);

			Focus();

			CityTownSelected += HandleCityTownSelected;
			Disposed += (sender, e) => CityTownSelected -= HandleCityTownSelected;
		}

		public void MoveSelection(int amountToMove)
		{
			var count = CityTownListView.Items.Count;
			if (count > 0 && amountToMove != 0 && !CityTownListView.IsDisposed)
			{
				var currentItemIndex = CityTownListView.SelectedItems.Count == 0 ? CityTownListView.Items[0].Index : CityTownListView.SelectedItems[0].Index;
				var newSelectedItemIndex = currentItemIndex + amountToMove;
				newSelectedItemIndex = ((newSelectedItemIndex % count) + count) % count;

				if (CityTownListView.SelectedItems.Count != 0)
				{
					CityTownListView.SelectedItems[0].Selected = false;
				}

				CityTownListView.Items[newSelectedItemIndex].Selected = true;
#if !WINZOR // TODO fix this for Winzor
				CityTownListView.Items[newSelectedItemIndex].EnsureVisible();
#endif
			}
		}

		readonly CandidateCityTown[] cityTownSuggestions;

		public readonly ISupportWebAddressValidation Address;
		public event EventHandler<CityTownSelectedEventArgs> CityTownSelected;

		public class CityTownSelectedEventArgs : EventArgs
		{
			public CandidateCityTown SelectedCityTown { get; set; }
			public ISupportWebAddressValidation Address { get; set; }
		}

		public void UpdateFilter(string enteredCity, string enteredStateCode, string enteredPostcode)
		{
			if(CityTownListView.IsDisposed || CityTownListView.Disposing)
			{
				return;
			}

			CityTownListView.Items.Clear();
#if !WINZOR // TODO fix this for Winzor
			CityTownListView.Groups.Clear();
#endif

			var matchesViewItemList = new List<ListViewItem>();
			var cityTownSuggestionsThatMatch = new List<CandidateCityTown>();
			var cityTownSuggestionsThatDoNotMatch = new List<CandidateCityTown>();

			// Find matched and unmatched
			foreach (var cityTown in cityTownSuggestions)
			{
				if (SuggestionMatchesFilter(cityTown, enteredCity, enteredStateCode, enteredPostcode))
				{
					cityTownSuggestionsThatMatch.Add(cityTown);
				}
				else
				{
					cityTownSuggestionsThatDoNotMatch.Add(cityTown);
				}
			}

			var matching = CreateListViewItems(cityTownSuggestionsThatMatch);
			var partialMatching = CreateListViewItems(cityTownSuggestionsThatDoNotMatch);

			if (matching.Count > 0)
			{
				CityTownListView.SuspendLayout();

#if !WINZOR
				var matchesGroup = new ListViewGroup(Res.GetString("d87d675c-84d0-4e79-b027-0a6e7e592adc", "Exact Matches"));

				foreach (var item in matching)
				{
					item.Group = matchesGroup;
				}

				CityTownListView.Groups.Add(matchesGroup);
#endif
				CityTownListView.Items.AddRange(matching.ToArray());
				CityTownListView.ResumeLayout();
			}

			if(partialMatching.Count > 0)
			{
				CityTownListView.SuspendLayout();

#if !WINZOR // TODO fix this for Winzor
				var partialMatchesGroup = new ListViewGroup(Res.GetString("50A75146-C6BA-400D-97CB-720F7B557995", "Partial Matches"));

				foreach(var item in partialMatching)
				{
					item.Group = partialMatchesGroup;
				}

				CityTownListView.Groups.Add(partialMatchesGroup);
#endif

				CityTownListView.Items.AddRange(partialMatching.ToArray());
				CityTownListView.ResumeLayout();
			}

			if (CityTownListView.Items.Count > 0)
			{
				CityTownListView.Items[0].Selected = true;
			}
		}

		List<ListViewItem> CreateListViewItems(List<CandidateCityTown> cityTowns)
		{
			var cityTownListViewItems = new List<ListViewItem>();
			var cityTownStrings = new HashSet<string>();

			foreach (var cityTown in cityTowns)
			{
				var cityTownString = ConstructCityTownForDisplay(cityTown);

				if (cityTownStrings.Add(cityTownString))
				{
					var viewItem = new ListViewItem(cityTownString) { Tag = cityTown };
					cityTownListViewItems.Add(viewItem);
				}
			}

			return cityTownListViewItems;
		}

		bool SuggestionMatchesFilter(CandidateCityTown cityTown, string enteredCity, string enteredStateCode, string enteredPostcode)
		{
			if (string.IsNullOrWhiteSpace(enteredCity) && string.IsNullOrWhiteSpace(enteredPostcode))
			{
				return true;
			}
			else
			{
				if (cityTown == null)
				{
					return false;
				}

				var cityMatches = FieldValueMatchesFilter(cityTown.City, enteredCity);
				var stateCodeMatches = FieldValueMatchesFilter(cityTown.State, enteredStateCode);
				var postcodeMatches = FieldValueMatchesFilter(cityTown.Postcode, enteredPostcode);

				if (cityMatches && stateCodeMatches && postcodeMatches)
				{
					return true;
				}
				else
				{
					return false;
				}
			}
		}

		bool FieldValueMatchesFilter(string candidateFieldValue, string enteredFieldValue)
		{
			return string.IsNullOrWhiteSpace(enteredFieldValue)
				   || !string.IsNullOrWhiteSpace(candidateFieldValue)
				   && enteredFieldValue.Length <= candidateFieldValue.Length
				   && enteredFieldValue.Equals(candidateFieldValue.Substring(0, enteredFieldValue.Length), StringComparison.OrdinalIgnoreCase);
		}

		[SuppressMessage("CargoWiseOne", "CW1017", Justification = "Control properties are fetched on the fly hence already scaled.")]
		internal void UpdateSize()
		{
			if (!CityTownListView.IsDisposed)
			{
				SuspendLayout();
				CityTownListView.SuspendLayout();
#if WINZOR
				var itemHeight = ControlDpiScalingHelper.ScaleToCurrentDpiY(18);
				var desiredHeight = (itemHeight * CityTownListView.Items.Count) + CityTownListView.Margin.Top + CityTownListView.Margin.Bottom;
#else

				var headerHeight = ControlDpiScalingHelper.ScaleToCurrentDpiY(24); 
				var itemHeight = CityTownListView.Items.Count > 0 ? CityTownListView.GetItemRect(0).Height + ControlDpiScalingHelper.ScaleToCurrentDpiY(1) : ControlDpiScalingHelper.ScaleToCurrentDpiY(18);
				var margins = CityTownListView.Margin.Top + CityTownListView.Margin.Bottom;
				var desiredHeight = margins + (itemHeight * CityTownListView.Items.Count) + (headerHeight * CityTownListView.Groups.Count);
#endif

				CityTownListView.Height = desiredHeight;
				Height = CityTownListView.Bottom;
				CityTownListView.Width = Width;
				var scrollBarSize = ControlDpiScalingHelper.ScaleToCurrentDpiY(16);
				CityTownListView.Columns[0].Width = Width - CityTownListView.Margin.Left - CityTownListView.Margin.Right - scrollBarSize;
				CityTownListView.ResumeLayout();
				ResumeLayout();
			}
		}

		void UpdateLocation(Form applicationForm, Control parentControl, Control referenceControl)
		{
			this.AdjustLocation(applicationForm, parentControl, referenceControl, Orientation.Vertical);
		}

		string ConstructCityTownForDisplay(CandidateCityTown cityTown)
		{
			var result = string.Empty;
			if (Address?.Country != null && Address.Country.IsStateMustNotBeEntered)
			{
				result = string.Join(", ", (new string[] { cityTown.Postcode, cityTown.City }).Where(s => !string.IsNullOrEmpty(s)));
			}
			else
			{
				result = string.Join(", ", (new string[] { cityTown.Postcode, cityTown.City, cityTown.State }).Where(s => !string.IsNullOrEmpty(s)));
			}

			return result;
		}

		void SetupListView(CandidateCityTown[] candidateCityTowns)
		{
			var matchesViewItemList = new List<ListViewItem>();

			if (candidateCityTowns.Length > 0)
			{
				CityTownListView.SuspendLayout();

				foreach (var cityTown in candidateCityTowns)
				{
					var viewItem = new ListViewItem(ConstructCityTownForDisplay(cityTown)) { Tag = cityTown };
					matchesViewItemList.Add(viewItem);
				}

				CityTownListView.Items.AddRange(matchesViewItemList.ToArray());
				CityTownListView.ResumeLayout();
			}

			SetupListViewProperties();
		}

		void SetupListViewProperties()
		{
#if !WINZOR // TODO fix this for Winzor
			CityTownListView.ShowGroups = true;
			CityTownListView.View = View.Details;
			CityTownListView.PreviewKeyDown += CityTownListView_PreviewKeyDown;
#endif
			CityTownListView.MultiSelect = false;
			CityTownListView.HeaderStyle = ColumnHeaderStyle.None;
			CityTownListView.DoubleClick += CityTownListView_DoubleClick;
			CityTownListView.KeyDown += CityTownListView_KeyDown;
		}

		void SelectRecommendedAddress()
		{
			if (CityTownListView.Items.Count > 0)
			{
				CityTownListView.Items[0].Selected = true;
				CityTownListView.Select();
				CityTownListView.HideSelection = false;
				CityTownListView.Focus();
			}
		}

		void CityTownListView_DoubleClick(object sender, EventArgs e)
		{
			SelectAddressAndClose();
		}

#if !WINZOR
		void CityTownListView_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
		{
			if (e.KeyCode == Keys.Escape)
			{
				e.IsInputKey = true;
			}
		}
#endif

		void CityTownListView_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
			{
				SelectAddressAndClose();
			}
			else if (e.KeyCode == Keys.Escape)
			{
				Close();
			}
		}

		void HandleCityTownSelected(object sender, CityTownSelectedEventArgs e)
		{
			e.Address.IsValidatingAddress = true;
			AddressValidationService.SetSuggestedCityTownToForm(e.SelectedCityTown, e.Address);
			e.Address.IsValidatingAddress = false;
		}

		public void SelectAddressAndClose()
		{
			if (CityTownListView.SelectedItems.Count > 0)
			{
				var cityTown = CityTownListView.SelectedItems[0].Tag as CandidateCityTown;
				if (cityTown != null && CityTownSelected != null)
				{
					CityTownSelected(this, new CityTownSelectedEventArgs() { SelectedCityTown = cityTown, Address = this.Address });
					AddressSuggestionControlHelper.LastSuggestedResults = null;
				}
				Close();
			}
		}

		public void Close()
		{
			if (Parent != null)
			{
				Parent.Controls.Remove(this);
			}
			Dispose();
		}
	}
}
