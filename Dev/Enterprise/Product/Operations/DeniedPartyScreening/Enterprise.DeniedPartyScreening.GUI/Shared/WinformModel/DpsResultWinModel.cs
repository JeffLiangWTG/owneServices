using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Common;
using Enterprise.DeniedPartyScreening.Business;

namespace Enterprise.DeniedPartyScreening.GUI
{
	public class DpsResultWinModel : WinModelBase, IDpsResult
	{
		readonly DpsResultModel resultModel;
		readonly bool standAlone;

		public DpsResultWinModel(DpsResultModel resultModel, bool standAlone = false)
		{
			Argument.NotNull(resultModel, nameof(resultModel));
			this.resultModel = resultModel;
			this.standAlone = standAlone;
			SelectedScreenedParty = ScreenedParties.FirstOrDefault();
			PersistentScreenedPartiesCount = ScreenedPartiesCount;
		}

		public bool AllPartiesClear => ScreenedParties.Count == 0;

		public Action Close { get; set; }

		public string ScreenedPartiesText => Res.GetString("1280D6E0-8CC5-4307-85A8-DD72FDB3C499", "Screened Parties");

		public int PersistentScreenedPartiesCount { get; }

		public int ScreenedPartiesCount => ScreenedParties.Count;

		internal void Navigate(bool up)
		{
			var index = ScreenedParties.IndexOf(SelectedScreenedParty);

			if (up)
			{
				if (index < ScreenedParties.Count - 1)
				{
					SelectedScreenedParty = ScreenedParties[index + 1];
				}
			}
			else if (index > 0)
			{
				SelectedScreenedParty = ScreenedParties[index - 1];
			}
		}

		void Save()
		{
			CredentialOverride = SelectedScreenedParty.ScreeningStatusWinModel.CredentialOverride;
			var index = ScreenedParties.IndexOf(SelectedScreenedParty);

			screenedParties = null;
			NotifyPropertyChanged(nameof(ScreenedParties));
			if (ScreenedPartiesCount > 0)
			{
				SelectedScreenedParty = index <= ScreenedParties.Count - 1 ? ScreenedParties[index] : ScreenedParties[index - 1];
			}
			else
			{
				SelectedScreenedParty = null;
				Close?.Invoke();
			}

			NotifyPropertyChanged(nameof(ScreenedPartiesCount));
		}

		public ObservableCollection<ScreenedPartyWinModel> TotalScreenedParties
		{
			get
			{
				if (totalScreenedParties == null)
				{
					var result = new List<ScreenedPartyWinModel>();
					foreach (var screenedPartyModel in resultModel.ScreenedPartyModels)
					{
						var screenedPartyWinModel = new ScreenedPartyWinModel(screenedPartyModel, Navigate, Save, standAlone);
						result.Add(screenedPartyWinModel);
					}

					totalScreenedParties = new ObservableCollection<ScreenedPartyWinModel>(result);
				}

				return totalScreenedParties;
			}
		}
		ObservableCollection<ScreenedPartyWinModel> totalScreenedParties;

		IEnumerable<IScreenedParty> IDpsResult.AllScreenedParties => totalScreenedParties;

		public ObservableCollection<ScreenedPartyWinModel> ScreenedParties
		{
			get
			{
				if (screenedParties == null)
				{
					var result = new List<ScreenedPartyWinModel>();
					foreach (var screenedParty in TotalScreenedParties)
					{
						if (screenedParty.HasPotentialMatches && !screenedParty.IsSaved)
						{
							screenedParty.GetIndexInfo = model => (result.IndexOf(model) + 1, result.Count);
							result.Add(screenedParty);
						}
					}

					screenedParties = new ObservableCollection<ScreenedPartyWinModel>(result);
				}

				return screenedParties;
			}
		}
		ObservableCollection<ScreenedPartyWinModel> screenedParties;

		public ScreenedPartyWinModel SelectedScreenedParty
		{
			get => selectedScreenedParty;
			set
			{
				selectedScreenedParty = value;
				NotifyPropertyChanged();
			}
		}
		ScreenedPartyWinModel selectedScreenedParty;

		public string CredentialOverride { get; set; }
	}
}
