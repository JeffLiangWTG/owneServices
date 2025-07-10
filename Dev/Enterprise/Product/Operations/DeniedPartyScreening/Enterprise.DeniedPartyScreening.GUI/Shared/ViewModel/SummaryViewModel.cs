using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.DeniedPartyScreening.GUI
{
	public class SummaryViewModel : ViewModelBase<SummaryViewModel>
	{
		readonly List<IScreenedParty> screeningParties;

		public SummaryViewModel(List<IScreenedParty> screeningParties, Action close)
		{
			this.screeningParties = screeningParties;
			CloseCommand = new DelegateCommand(close);
		}

		public ICommand CloseCommand { get; }

		public string Title => Res.GetString("56650B61-449B-4B32-ACA8-129F65CFBB36", "Summary");

		public string CloseText => Res.GetString("E28E90E8-0D6C-4275-9F90-5B1694FB1BF4", "Close");

		public ObservableCollection<SummaryItemViewModel> SummaryItemViewModels
		{
			get
			{
				if (summaryItemViewModels == null)
				{
					summaryItemViewModels = new ObservableCollection<SummaryItemViewModel>();
					var statusesList = new ScreeningStatusesList();
					foreach (var screenedParty in screeningParties)
					{
						var status = screenedParty.NewScreeningStatus == ScreeningStatusesList.Codes.Canceled ? screenedParty.CurrentScreeningStatus : screenedParty.NewScreeningStatus;
						summaryItemViewModels.Add(new SummaryItemViewModel(screenedParty.ScreenedEntity, screenedParty.PartyName, statusesList[status].Description));
					}
				}

				return summaryItemViewModels;
			}
		}
		ObservableCollection<SummaryItemViewModel> summaryItemViewModels;
	}
}
