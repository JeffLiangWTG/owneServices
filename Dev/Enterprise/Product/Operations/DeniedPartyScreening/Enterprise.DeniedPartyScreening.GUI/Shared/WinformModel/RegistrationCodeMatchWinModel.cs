using System.Collections.ObjectModel;
using CargoWise.Common;
using Enterprise.DeniedPartyScreening.Business;

namespace Enterprise.DeniedPartyScreening.GUI
{
	public class RegistrationCodeMatchWinModel : GenericMatchWinModel
	{
		readonly RegistrationCodeMatchModel registrationCodeMatchModel;

		public RegistrationCodeMatchWinModel(RegistrationCodeMatchModel registrationCodeMatchModel, bool visibility)
		{
			Argument.NotNull(registrationCodeMatchModel, nameof(registrationCodeMatchModel));

			this.registrationCodeMatchModel = registrationCodeMatchModel;

			MatchViewVisibility = visibility;
		}

		public bool MatchViewVisibility { get; }

		public override string ExpanderTitle => Res.GetString("013BAAE3-1D61-4708-A383-D3E3382553E1", "Registration Code");

		public override string OddName => Res.GetString("E9B9D174-E49C-4F57-9599-786258081E12", "Code");

		public override string PluralName => Res.GetString("49058FBB-448A-41EC-8611-1B170E29DDE8", "Codes");

		ObservableCollection<ScreenedDeniedItemWinModel> totalScreenedDeniedItems;

		public override ObservableCollection<ScreenedDeniedItemWinModel> TotalScreenedDeniedItems
		{
			get
			{
				if (totalScreenedDeniedItems == null)
				{
					totalScreenedDeniedItems = new ObservableCollection<ScreenedDeniedItemWinModel>();

					foreach (var deniedItem in registrationCodeMatchModel.TotalScreenedDeniedItems)
					{
						if (deniedItem.RegistrationCodeMatchInfo != null)
						{
							totalScreenedDeniedItems.Add(new ScreenedDeniedItemWinModel(GetScreenedParty(deniedItem), GetDeniedParty(deniedItem), deniedItem.DisplayScore, deniedItem.ScoreGrade, deniedItem.Score));
						}
						else
						{
							totalScreenedDeniedItems.Add(new ScreenedDeniedItemWinModel(GetDeniedParty(deniedItem)));
						}
					}
				}

				return totalScreenedDeniedItems;
			}
		}

		string GetDeniedParty(ScreenedDeniedRegistrationCodeItemModel model) => BuildParty(model.ProfileRegistrationCodeInfo.IdType, model.ProfileRegistrationCodeInfo.IdNumber);

		string GetScreenedParty(ScreenedDeniedRegistrationCodeItemModel model) => BuildParty(model.RegistrationCodeMatchInfo.RequestRegistrationCode.RegCodeType, model.RegistrationCodeMatchInfo.RequestRegistrationCode.RegCodeValue);

		string BuildParty(string type, string value)
		{
			string result = null;
			if (!string.IsNullOrWhiteSpace(type))
			{
				result += type + " : ";
			}

			return result + value;
		}
	}
}
