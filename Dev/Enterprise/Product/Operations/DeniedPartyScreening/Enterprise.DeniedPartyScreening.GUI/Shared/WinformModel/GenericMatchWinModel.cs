using System;
using System.Collections.ObjectModel;
using System.Linq;
using Enterprise.DeniedPartyScreening.Business;

namespace Enterprise.DeniedPartyScreening.GUI
{
	public abstract class GenericMatchWinModel : InnerExpanderWinModel<GenericMatchWinModel>
	{
		public string OtherScreenedPartyHeader => Res.GetString("FBDD4CDE-1CFF-4F51-AC83-0CB61CB062AD", "Other Screened Party");

		public string OtherDeniedPartyHeader => Res.GetString("62fe5edb-0d2a-4608-b4f1-b478340bdb64", "Other Denied Party");

		public string ScreenedPartyHeader => Res.GetString("c9f2b514-beb3-4f52-985e-fc6bdb5be77b", "Screened Party");

		public string DeniedPartyHeader => Res.GetString("697ef8ca-7828-4e9e-aa40-926a12751d61", "Denied Party");

		public override string InnerExpanderTitle => IsInnerExpanderExpanded ? Res.GetString("8d3f5653-b4cb-4a9d-b62d-709f8b5bdd4a", "Show Less ({0})", OtherScreenedDeniedItemsCount) : Res.GetString("4AEEAE3C-399D-4E23-A6F8-D3DF724F9130", "Show More ({0})", OtherScreenedDeniedItemsCount);

		public int OtherScreenedDeniedItemsCount => OtherScreenedDeniedItems.Count;

		public int ScreenedDeniedItemsCount => ScreenedDeniedItems.Count;

		public abstract ObservableCollection<ScreenedDeniedItemWinModel> TotalScreenedDeniedItems { get; }

		ObservableCollection<ScreenedDeniedItemWinModel> OrderedTotalScreenedDeniedItems => orderedTotalScreenedDeniedItems ?? (orderedTotalScreenedDeniedItems = new ObservableCollection<ScreenedDeniedItemWinModel>(TotalScreenedDeniedItems.OrderByDescending(x => x.Score).ToList()));
		ObservableCollection<ScreenedDeniedItemWinModel> orderedTotalScreenedDeniedItems;

		public ObservableCollection<ScreenedDeniedItemWinModel> ScreenedDeniedItems
		{
			get
			{
				if (screenedDeniedItems == null)
				{
					screenedDeniedItems = new ObservableCollection<ScreenedDeniedItemWinModel>(OrderedTotalScreenedDeniedItems.Where(x => x.ScoreGrade >= ScoreGrades.Medium).GroupBy(x => x.ScreenedParty).Select(x => x.FirstOrDefault()));
					if (screenedDeniedItems.Count > 0)
					{
						screenedDeniedItems[screenedDeniedItems.Count - 1].BottomLineVisibility = false;
					}
				}

				return screenedDeniedItems;
			}
		}
		ObservableCollection<ScreenedDeniedItemWinModel> screenedDeniedItems;

		public ObservableCollection<ScreenedDeniedItemWinModel> OtherScreenedDeniedItems
		{
			get
			{
				if (otherScreenedDeniedItems == null)
				{
					otherScreenedDeniedItems = new ObservableCollection<ScreenedDeniedItemWinModel>(OrderedTotalScreenedDeniedItems.Where(x => !ScreenedDeniedItems.Contains(x)));
					if (otherScreenedDeniedItems.Count > 0)
					{
						otherScreenedDeniedItems[otherScreenedDeniedItems.Count - 1].BottomLineVisibility = false;
					}
				}

				return otherScreenedDeniedItems;
			}
		}
		ObservableCollection<ScreenedDeniedItemWinModel> otherScreenedDeniedItems;

		public abstract string OddName { get; }

		public abstract string PluralName { get; }

		public override bool IsExpanderEnabled => ScreenedDeniedItemsCount > 0 || OtherScreenedDeniedItemsCount > 0;

		bool shouldSetDefaultValue = true;
		bool isExpanderExpanded;
		public override bool IsExpanderExpanded
		{
			get
			{
				if (shouldSetDefaultValue)
				{
					isExpanderExpanded = IsExpanderEnabled && ScreenedDeniedItemsCount > 0;
					shouldSetDefaultValue = false;
				}

				return isExpanderExpanded;
			}

			set => isExpanderExpanded = value;
		}

		public override string ExpanderDescription
		{
			get
			{
				string result;

				if (ScreenedDeniedItemsCount == 0)
				{
					result = Res.GetString("55A2B99D-A973-4F83-9B47-A3A9F827309C", "No {0} Matched", PluralName);
				}
				else if (ScreenedDeniedItemsCount == 1)
				{
					result = Res.GetString("722519F6-CCD3-4D59-A6DF-F1DB5DE8847E", "1 {0} Matched", OddName);
				}
				else
				{
					result = Res.GetString("369B5CA4-5C2B-4CC6-9B31-939D0E5CD810", "{0} {1} Matched", ScreenedDeniedItemsCount, PluralName);
				}

				return result;
			}
		}

		public Action NotifySizeChangedAction { get; set; }
	}
}
