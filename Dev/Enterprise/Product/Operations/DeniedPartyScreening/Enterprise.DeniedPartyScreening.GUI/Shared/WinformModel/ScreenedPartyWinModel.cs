using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.DeniedPartyScreening.Business;
using Enterprise.DeniedPartyScreening.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.DeniedPartyScreening.GUI
{
	public class ScreenedPartyWinModel : WinModelBase, IScreenedParty
	{
		readonly ScreenedPartyModel screenedPartyModel;
		readonly Action save;
		readonly Action<bool> navigate;

		public ScreenedPartyWinModel(ScreenedPartyModel screenedPartyModel, Action<bool> navigate, Action save = null, bool standAlone = false)
		{
			Argument.NotNull(screenedPartyModel, nameof(screenedPartyModel));
			Argument.NotNull(navigate, nameof(navigate));

			this.screenedPartyModel = screenedPartyModel;
			this.save = save;
			this.navigate = navigate;

			StandAlone = standAlone;
			ShowExcluded = false;
			SelectedPotentialMatchWinModel = PotentialMatchWinModels.FirstOrDefault();
		}

		public Func<ScreenedPartyWinModel, (int Index, int TotalCount)> GetIndexInfo { get; set; }

		public bool StandAlone { get; }

		public bool IsSaved { get; private set; }

		public bool SaveButtonEnabled => screeningStatusWinModel.SaveButtonEnabled;

		public string SaveCaption => Res.GetString("B1ED10FF-1992-4C30-86B9-D0F6DB2E7180", "Save");

		public int Index => GetIndexInfo?.Invoke(this).Index ?? 0;

		public int TotalRecordsCount => GetIndexInfo?.Invoke(this).TotalCount ?? 0;

		public int? PersistentTotalRecordsCount => persistentTotalRecordsCount ?? (persistentTotalRecordsCount = TotalRecordsCount);
		int? persistentTotalRecordsCount;

		public string NavigationLabel => string.Format(CultureInfo.InvariantCulture, "{0}/{1}", Index, TotalRecordsCount);

		public string PartyName => screenedPartyModel.PartyName;

		public PartyTypes PartyType => screenedPartyModel.PartyType;

		public string ParentsDescription => screenedPartyModel.ParentsDescription;

		public string PotentialMatchesText => Res.GetString("9CA06992-F11F-4710-B76E-279289DAFBD6", "Potential Matches");

		public int PotentialMatchWinModelsCount => PotentialMatchWinModels.Count;

		public int TotalPotentialMatchWinModelsCount => TotalPotentialMatchWinModels.Count;

		public bool HasPotentialMatches => PotentialMatchWinModels.Count > 0;

		public bool ShowExcluded
		{
			get => showExcluded;
			set
			{
				showExcluded = value;
				PotentialMatchWinModels = value ? TotalPotentialMatchWinModels : new ObservableCollection<PotentialMatchWinModel>(TotalPotentialMatchWinModels.Where(x => !x.IsExcluded && x.ScoreGrade > ScoreGrades.Low));
				if (SelectedPotentialMatchWinModel != null)
				{
					if (PotentialMatchWinModels.Count > 0 && PotentialMatchWinModels.IndexOf(SelectedPotentialMatchWinModel) < 0)
					{
						SelectedPotentialMatchWinModel = PotentialMatchWinModels.FirstOrDefault();
					}
				}
			}
		}
		bool showExcluded;

		public string ShowExcludedText => Res.GetString("3FE6A319-B729-4E04-B490-69E52D206356", "Show Excluded", ShowExcludedCount);

		public int ShowExcludedCount => TotalPotentialMatchWinModels.Count(x => x.IsExcluded);

		public bool ShowExcludedEnabled => ShowExcludedCount > 0 && TotalPotentialMatchWinModelsCount > ShowExcludedCount;

		public string ProfileNameText => Res.GetString("A5A62D8E-968B-48E1-BDAB-7D7EAB7FEC89", "Profile Name");

		public ObservableCollection<PotentialMatchWinModel> TotalPotentialMatchWinModels
		{
			get
			{
				if (totalPotentialMatchWinModels == null)
				{
					var result = new List<PotentialMatchWinModel>();
					foreach (var matchModel in screenedPartyModel.PotentialMatchModels)
					{
						result.Add(new PotentialMatchWinModel(matchModel));
					}

					totalPotentialMatchWinModels = new ObservableCollection<PotentialMatchWinModel>(result);
				}

				return totalPotentialMatchWinModels;
			}
		}
		ObservableCollection<PotentialMatchWinModel> totalPotentialMatchWinModels;

		public ObservableCollection<PotentialMatchWinModel> PotentialMatchWinModels { get; set; }

		public PotentialMatchWinModel SelectedPotentialMatchWinModel
		{
			get => selectedPotentialMatchWinModel;
			set
			{
				selectedPotentialMatchWinModel = value;
				NotifyPropertyChanged();
			}
		}
		PotentialMatchWinModel selectedPotentialMatchWinModel;

		public DpsImageSources EntityTypeIcon
		{
			get
			{
				if (entityTypeIconOverride != null)
				{
					return entityTypeIconOverride.Value;
				}

				switch (PartyType)
				{
					case PartyTypes.Country:
						return DpsImageSources.Country;
					case PartyTypes.Vessel:
						return DpsImageSources.Vessel;
					case PartyTypes.JobDocAddress:
						if (screenedPartyModel.ResponseWithScreeningParty.ScreeningParty.DocAddress?.E2_IsResidential ?? false)
						{
							return DpsImageSources.Person;
						}
						else
						{
							return DpsImageSources.Organization;
						}
					case PartyTypes.Organization:
						if (screenedPartyModel.ResponseWithScreeningParty.ScreeningParty.Header?.OH_Category.ToString() == OrgConstants.Category.NaturalPersonIndividual)
						{
							return DpsImageSources.Person;
						}
						else
						{
							return DpsImageSources.Organization;
						}
					default:
						throw new ArgumentException("Invalid argument.", nameof(PartyType));
				}
			}
			set => entityTypeIconOverride = value;
		}
		DpsImageSources? entityTypeIconOverride;

		ScreeningStatusWinModel screeningStatusWinModel;
		public ScreeningStatusWinModel ScreeningStatusWinModel => screeningStatusWinModel ?? (screeningStatusWinModel = new ScreeningStatusWinModel(
			PotentialMatchWinModels.OrderByDescending(x => x.ScoreGrade).Select(x => x.ScoreGrade).FirstOrDefault(),
			screenedPartyModel.ResponseWithScreeningParty.ScreeningParty,
			() => { NotifyPropertyChanged(nameof(SaveButtonEnabled)); },
			StandAlone));

		public void ExecuteSaveCommand()
		{
			IsSaved = true;
			save?.Invoke();
		}

		public string CurrentScreeningStatus => screenedPartyModel.ResponseWithScreeningParty.ScreeningParty.CurrentScreeningStatus;

		public string NewScreeningStatus
		{
			get
			{
				string result;
				if (HasPotentialMatches)
				{
					if (IsSaved && !ScreeningStatusWinModel.ScreeningStatus.IsEmpty)
					{
						result = ScreeningStatusWinModel.ScreeningStatus;
					}
					else
					{
						result = ScreeningStatusesList.Codes.Canceled;
					}
				}
				else
				{
					result = ScreeningStatusesList.Codes.Clear;
				}

				return result;
			}
		}

		public string FullClearingReason
		{
			get
			{
				string fullClearedReason;

				if (ScreeningStatusWinModel.ScreeningStatus == ScreeningStatusesList.Codes.Clear ||
					ScreeningStatusWinModel.ScreeningStatus == ScreeningStatusesList.Codes.PermanentClear)
				{
					var allClearedReasonComponents = new string[]
					{
						ScreeningStatusWinModel.ClearingReason, ScreeningStatusWinModel.ClearingReasons.GetDescriptionFromCode(ScreeningStatusWinModel.ClearingReason), ScreeningStatusWinModel.ClearingReasonText
					};

					fullClearedReason = string.Join(", ", allClearedReasonComponents.Where(r => !string.IsNullOrWhiteSpace(r)));
				}
				else
				{
					fullClearedReason = string.Empty;
				}

				return fullClearedReason;
			}
		}

		public void NavigateUp()
		{
			navigate.Invoke(true);
		}

		public void NavigateDown()
		{
			navigate.Invoke(false);
		}

		public BusinessObject ScreenedEntity => screenedPartyModel.ResponseWithScreeningParty.ScreeningParty.ScreeningEntity;

		public BusinessObject[] Parents => screenedPartyModel.ResponseWithScreeningParty.ScreeningParty.Parents.ToArray();

		public DpsRequestHeaderWithAddressMatching RequestHeaderWithAddressMatching => screenedPartyModel.ResponseWithScreeningParty.RequestHeaderWithAddressMatching;

		public DpsResponse Response => screenedPartyModel.ResponseWithScreeningParty.Response;

		public string HighConfidenceResults => screenedPartyModel.HighConfidenceResults;

		public string MediumConfidenceResults => screenedPartyModel.MediumConfidenceResults;

		public int LowConfidenceResultsCount => screenedPartyModel.LowConfidenceResultsCount;
	}
}
