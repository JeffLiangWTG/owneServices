using System.Collections.ObjectModel;
using CargoWise.Common;
using Enterprise.DeniedPartyScreening.Business;

namespace Enterprise.DeniedPartyScreening.GUI
{
	public class NameMatchWinModel : GenericMatchWinModel
	{
		public NameMatchWinModel(NameMatchModel nameMatchModel, bool visibility)
		{
			Argument.NotNull(nameMatchModel, nameof(nameMatchModel));
			NameMatchModel = nameMatchModel;

			MatchViewVisibility = visibility;
		}

		public bool MatchViewVisibility { get; }

		public override string ExpanderTitle => Res.GetString("92f84a6d-ce65-47e6-97b7-f7c4fc56f8c4", "Name");

		public override string OddName => ExpanderTitle;

		public override string PluralName => Res.GetString("DB822140-B25D-41E7-A43C-C93408E343C1", "Names");

		ObservableCollection<ScreenedDeniedItemWinModel> totalScreenedDeniedItems;

		public override ObservableCollection<ScreenedDeniedItemWinModel> TotalScreenedDeniedItems
		{
			get
			{
				if (totalScreenedDeniedItems == null)
				{
					totalScreenedDeniedItems = new ObservableCollection<ScreenedDeniedItemWinModel>();

					foreach (var deniedItem in NameMatchModel.TotalScreenedDeniedItems)
					{
						if (deniedItem.NameMatchInfo != null)
						{
							totalScreenedDeniedItems.Add(new ScreenedDeniedItemWinModel(deniedItem.NameMatchInfo.RequestName.FullName, deniedItem.ProfileNameInfo.FullName, deniedItem.DisplayScore, deniedItem.ScoreGrade, deniedItem.Score));
						}
						else
						{
							totalScreenedDeniedItems.Add(new ScreenedDeniedItemWinModel(deniedItem.ProfileNameInfo.FullName));
						}
					}
				}

				return totalScreenedDeniedItems;
			}
		}

		public NameMatchModel NameMatchModel { get; }
	}
}
