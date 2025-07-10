using System.Collections.ObjectModel;
using CargoWise.Common;
using Enterprise.DeniedPartyScreening.Business;
using Enterprise.DeniedPartyScreening.Common;

namespace Enterprise.DeniedPartyScreening.GUI
{
	public class AddressMatchWinModel : GenericMatchWinModel
	{
		readonly AddressMatchModel addressMatchModel;

		public AddressMatchWinModel(AddressMatchModel addressMatchModel, bool visibility)
		{
			Argument.NotNull(addressMatchModel, nameof(addressMatchModel));
			this.addressMatchModel = addressMatchModel;
			MatchViewVisibility = visibility;
		}

		public bool MatchViewVisibility { get; }

		public override string ExpanderTitle => Res.GetString("CBE32D07-2A2F-4546-853C-0F96B83FADD2", "Address");

		public override string OddName => ExpanderTitle;

		public override string PluralName => Res.GetString("02013DE5-2F80-49D7-AC10-8F92FEB81980", "Addresses");

		public override ObservableCollection<ScreenedDeniedItemWinModel> TotalScreenedDeniedItems
		{
			get
			{
				if (totalScreenedDeniedItems == null)
				{
					totalScreenedDeniedItems = new ObservableCollection<ScreenedDeniedItemWinModel>();

					foreach (var deniedItem in addressMatchModel.TotalScreenedDeniedItems)
					{
						if (deniedItem.AddressMatchInfo != null)
						{
							totalScreenedDeniedItems.Add(new ScreenedDeniedItemWinModel(GetFormattedAddress(deniedItem.AddressMatchInfo.RequestAddress), GetFormattedAddress(deniedItem.ProfileAddressInfo), deniedItem.DisplayScore, deniedItem.ScoreGrade, deniedItem.Score));
						}
						else
						{
							totalScreenedDeniedItems.Add(new ScreenedDeniedItemWinModel(GetFormattedAddress(deniedItem.ProfileAddressInfo)));
						}
					}
				}

				return totalScreenedDeniedItems;
			}
		}
		ObservableCollection<ScreenedDeniedItemWinModel> totalScreenedDeniedItems;

		string GetFormattedAddress(ProfileAddressInfo address)
		{
			return (GetAddressElement(address.Street) +
				   GetAddressElement(address.City) +
				   GetAddressElement(address.StateProvince) +
				   GetAddressElement(address.PostCode) +
				   GetAddressElement(address.Country)).TrimEnd(System.Environment.NewLine.ToCharArray());
		}

		string GetFormattedAddress(DpsAddressCandidate address)
		{
			return (GetAddressElement(address.Address1) +
				   GetAddressElement(address.City) +
				   GetAddressElement(address.State) +
				   GetAddressElement(address.PostCode) +
				   GetAddressElement(address.Country)).TrimEnd(System.Environment.NewLine.ToCharArray());
		}

		string GetAddressElement(string addressElement)
		{
			if (!string.IsNullOrWhiteSpace(addressElement))
			{
				return addressElement + System.Environment.NewLine;
			}

			return string.Empty;
		}
	}
}
