using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Tools.DuplicateDetector;
using CargoWise.Types;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterData.Business
{
	public abstract class DeduplicationResultDetail<TMaster, TCandidate> : NonPersistentBusinessObject, IDeduplicationResultDetail
		where TMaster : IDeduplicationMaster, IDeduplicationGlowObject
		where TCandidate : DuplicationCandidate
	{
		public TMaster Master { get; }
		protected readonly bool isAdminPanel;
		protected bool isExcludingInactiveResults;
		protected bool isExcludingOtherCountries;
		protected bool isShowIgnoredResults;
		protected readonly Dictionary<ScoringResult, PatternMatchingResult> resultDictionary;
		protected readonly Action closeParentAction;
		public event EventHandler PerformSearch;

		protected DeduplicationResultDetail()
		{
			isAdminPanel = true;
			IsEmpty = true;
		}

		protected DeduplicationResultDetail(
			TMaster master,
			IEnumerable<TMaster> targets,
			IEnumerable<ScoringResult> scoringResults,
			IEnumerable<PatternMatchingResultModel> ptmResultModels,
			Action closeParentAction)
			: this(master, targets, scoringResults, ptmResultModels)
		{
			isAdminPanel = false;
			IsEmpty = false;
			this.closeParentAction = closeParentAction;
			isExcludingInactiveResults = OrganisationsDataRegistry.Instance.ExcludeInactivePotentialDuplicates.Value;
			isExcludingOtherCountries = OrganisationsDataRegistry.Instance.ExcludePotentialDuplicatesFromOtherCountries.Value;
			isShowIgnoredResults = true;
		}

		protected DeduplicationResultDetail(
			TMaster master,
			IEnumerable<TMaster> targets,
			IEnumerable<ScoringResult> scoringResults,
			IEnumerable<PatternMatchingResultModel> ptmResultModels,
			bool isExcludingInactiveResults,
			bool isExcludingOtherCountries,
			bool isShowIgnoredResults,
			Dictionary<ScoringResult, PatternMatchingResult> resultDictionary)
			: this(master, targets, scoringResults, ptmResultModels)
		{
			isAdminPanel = true;
			IsEmpty = false;
			this.isExcludingInactiveResults = isExcludingInactiveResults;
			this.isExcludingOtherCountries = isExcludingOtherCountries;
			this.isShowIgnoredResults = isShowIgnoredResults;
			this.resultDictionary = resultDictionary;
		}

		DeduplicationResultDetail(
			TMaster master,
			IEnumerable<TMaster> targets,
			IEnumerable<ScoringResult> scoringResults,
			IEnumerable<PatternMatchingResultModel> ptmResultModels)
			: base(master.Master?.Factory)
		{
			Master = master;
			Targets = targets.ToList();
			ScoringResults = scoringResults.ToList();
			PtmResultModels = ptmResultModels.ToArray();
		}

		protected Action<ZGuid> LoadInformationAction => (selectedItemPK) =>
		{
			if (!IsEmpty && Master != null)
			{
				SetViewBusy(true);

				using (this.SuspendNotifyPropertyChanges())
				{
					this.NotifyPropertyChanged(nameof(ShowIgnoreButton));
					GetOrUpdateCandidatesForMerging();
					LoadFullList();
					FilterDuplicationCandidates();
					SetCurrentSelection(selectedItemPK);
				}

				SetViewBusy(false);
			}
		};

		protected virtual Action<string, bool, IEnumerable<TCandidate>> CreateIgnoreBillingTransaction { get; }

		public virtual ZGuid SelectedCandidatePK
		{
			get => selectedCandidatePK.IsValid ? selectedCandidatePK : ZGuid.Empty;
			set
			{
				selectedCandidatePK = value;
				ShowTargetRelatedButtons = !CurrentSelectedCandidate?.IsDummy ?? false;
			}
		}
		ZGuid selectedCandidatePK;

		public IEnumerable<DuplicationModelDetailGroup> GetSelectedCandidateDetails()
		{
			if (DuplicationCandidates.Count == 0 || CurrentSelectedCandidate == null)
			{
				return Enumerable.Empty<DuplicationModelDetailGroup>();
			}

			return CurrentSelectedCandidate.IsDummy
				? DuplicationModelDetail.GetDuplicationModelDetails(DuplicationCandidates[0].DeduplicationPresenterModels)
				: DuplicationModelDetail.GetDuplicationModelDetails(CurrentSelectedCandidate.DeduplicationPresenterModels);
		}

		public DuplicationCandidateCollection<TCandidate> DuplicationCandidates { get; } = new DuplicationCandidateCollection<TCandidate>();

		public IEnumerable<TCandidate> UnfilteredResults
		{
			get => unfilteredResults;
			set
			{
				unfilteredResults = value;
				PostProcessDuplications();
			}
		}
		IEnumerable<TCandidate> unfilteredResults;

		protected virtual void PostProcessDuplications()
		{
		}

		public bool IsExcludingInactiveResults
		{
			get => isExcludingInactiveResults;
			set
			{
				if (value != isExcludingInactiveResults)
				{
					isExcludingInactiveResults = value;
					PropagateDeduplicationAction(DeduplicationAction.ExcludeInactiveFilterChanged);
					if (SelectedCandidatePK != ZGuid.Empty)
					{
						LoadInformationAction(SelectedCandidatePK);
					}
					else
					{
						LoadInformationAction(ZGuid.Empty);
					}
				}
			}
		}

		public WaterMarkType WaterMark
		{
			get
			{
				if (Master != null && DuplicationCandidates.Cast<DuplicationCandidate>().Any(d => d.IsDummy))
				{
					var hasUnfilteredResults = UnfilteredResults.Any();

					if ((IsExcludingInactiveResults || IsExcludingOtherCountries || !IsShowIgnoredResults) && UnfilteredResults != null && hasUnfilteredResults)
					{
						return WaterMarkType.TemporarilyIgnored;
					}
					else if (!hasUnfilteredResults)
					{
						return WaterMarkType.NoDeduplicates;
					}
				}

				return WaterMarkType.None;
			}
		}

		public bool IsShowIgnoredResults
		{
			get => isShowIgnoredResults;
			set
			{
				if (value != isShowIgnoredResults)
				{
					isShowIgnoredResults = value;
					PropagateDeduplicationAction(DeduplicationAction.ShowIgnoredFilterChanged);
					if (SelectedCandidatePK != ZGuid.Empty)
					{
						LoadInformationAction(SelectedCandidatePK);
					}
					else
					{
						LoadInformationAction(ZGuid.Empty);
					}
				}
			}
		}

		public bool IsExcludingOtherCountries
		{
			set
			{
				if (value != isExcludingOtherCountries)
				{
					isExcludingOtherCountries = value;
					PropagateDeduplicationAction(DeduplicationAction.ExcludeCountriesFilterChanged);
					if (SelectedCandidatePK != ZGuid.Empty)
					{
						LoadInformationAction(SelectedCandidatePK);
					}
					else
					{
						LoadInformationAction(ZGuid.Empty);
					}
				}
			}
			get
			{
				return isExcludingOtherCountries;
			}
		}

		public abstract SecurityCheckpoint ViewEntityFormCheckPoint { get; }
		public abstract SecurityCheckpoint DuplicateDetectionMergeCheckPoint { get; }
		public abstract SecurityCheckpoint IgnoreDuplicatesCheckPoint { get; }
		public virtual string MasterCode => string.Empty;
		public virtual string CandidateCode => string.Empty;
		public bool IsEmpty { get; protected set; }
		public bool IsLoadingData { get; set; }

		public int CandidatesCount
		{
			get => duplicationsCount;
			set
			{
				if (duplicationsCount != value)
				{
					duplicationsCount = value;
					this.NotifyPropertyChanged();
				}
			}
		}
		int duplicationsCount;

		protected IEnumerable<TMaster> Targets { get; set; } = Enumerable.Empty<TMaster>();
		protected List<ScoringResult> ScoringResults { get; set; } = new List<ScoringResult>();
		protected IEnumerable<PatternMatchingResultModel> PtmResultModels { get; set; }
		protected abstract TCandidate EmptyCandidate { get; }
		protected bool ignoreCountBiggerThanZero;
		protected bool isDummy;
		public bool ReloadRequired { get; set; }
		protected virtual bool TargetsReloadRequired { get; private set; }

		protected void UpdateDuplicationsCount()
		{
			CandidatesCount = DuplicationCandidates == null ? 0 : DuplicationCandidates.Count(p => !((TCandidate)p).IsDummy);
		}

		protected TCandidate CurrentSelectedCandidate
		{
			get
			{
				var selectedCandidatePK = SelectedCandidatePK;
				return (TCandidate)DuplicationCandidates.SingleOrDefault(x => ((TCandidate)x).TargetPK == selectedCandidatePK);
			}
		}

		public void LoadCandidatesAndUpdateSelectedItem()
		{
			LoadInformationAction(SelectedCandidatePK);
		}

		void FilterDuplicationCandidates()
		{
			FilterAndUpdate();
			AddEmptyCandidateAndUpdateSelectedItem();
		}

		public abstract void FilterAndUpdate();

		protected bool MeetGeneralPotentialDuplicationsCondition(TCandidate model)
		{
			return (!IsExcludingOtherCountries || model.IsSameCountryAsMaster)
				&& (!IsExcludingInactiveResults || model.IsActive)
				&& (IsShowIgnoredResults || !model.IsIgnored);
		}

		void AddEmptyCandidateAndUpdateSelectedItem()
		{
			if (DuplicationCandidates.Count != 0)
			{
				SetCurrentSelection(SelectedCandidatePK);
				return;
			}

			if (isAdminPanel)
			{
				SetDummyDuplication();
			}
		}

		protected abstract void SetDummyDuplication();

		protected abstract void IgnoreTarget(TCandidate target, string status);

		protected abstract IEnumerable<TCandidate> GetDuplicationCandidatesCore();

		protected abstract List<ScoringResult> GetScoringResultsCore();

		protected IEnumerable<DeduplicationPresenterModel> GenerateNewPresenterModels() => new DeduplicationPresenter(Master, (IEnumerable<IDeduplicationGlowObject>)Targets, ScoringResults).GeneratePresenterModels();

		#region Implementation

		void GetOrUpdateCandidatesForMerging()
		{
			var candidates = GetDuplicationCandidatesCore();
			CalculateIsSameCountryAsMasterOrNotForCandidates(candidates);
			CandidatesCount = candidates.Count();

			DuplicationCandidates.RemoveAll();
			if (CandidatesCount > 0)
			{
				DuplicationCandidates.AddRange(candidates);
			}
			else
			{
				ShowSelectAll = false;
			}
			SetInfoForDuplicationCandidates(DuplicationCandidates);
			UnfilteredResults = candidates;
		}

		protected void SetInfoForDuplicationCandidates(DuplicationCandidateCollection<TCandidate> value)
		{
			if (value != null)
			{
				foreach (var item in value.Cast<TCandidate>())
				{
					SetPotentialDuplicationModelInfo(item);
				}
			}
		}

		protected abstract void SetPotentialDuplicationModelInfo(TCandidate item);

		void CalculateIsSameCountryAsMasterOrNotForCandidates(IEnumerable<DuplicationCandidate> candidates)
		{
			foreach (var scoringResult in ScoringResults)
			{
				var targetModel = Targets.FirstOrDefault(g => g.PK == scoringResult.TargetPK);
				var candidate = candidates.FirstOrDefault(d => d.DeduplicationPresenterModels.First().TargetID == scoringResult.TargetPK);
				if (targetModel != null && candidate != null)
				{
					candidate.IsSameCountryAsMaster = targetModel.CountryCode == Master.CountryCode;
				}
			}
		}

		protected void PropagateDeduplicationAction(DeduplicationAction action)
		{
			if (Master != null)
			{
				Master.Master.PropagateDeduplicationActionOccurred(action, ScoringResults, PtmResultModels, Targets, isExcludingInactiveResults, isExcludingOtherCountries, isShowIgnoredResults);
			}
		}

		protected void RemoveSelectedDuplication()
		{
			if (SelectedCandidatePK != Guid.Empty)
			{
				DuplicationCandidates.Remove(DuplicationCandidates.Cast<DuplicationCandidate>().First(u => u.TargetPK == SelectedCandidatePK));
			}
			UpdateDuplicationsCount();

			if (!DuplicationCandidates.Any())
			{
				Master.Master.ValidateDuplicationResult(false);
				CloseParent();
			}
			else
			{
				SelectedCandidatePK = DuplicationCandidates[0].TargetPK;
			}
		}

		#endregion Implementation

		#region Button Field

		public virtual bool ShowCheckBox { get; set; }

		public bool ShowTargetRelatedButtons
		{
			get => !isDummy;
			protected set
			{
				isDummy = !value;
				this.NotifyPropertyChanged(nameof(ShowIgnoreAllButton));
				this.NotifyPropertyChanged(nameof(ShowTargetRelatedButtons));
			}
		}

		public virtual bool ShowSelectAll { get; set; }

		public virtual bool ShowMergeAllSelectedButton => false;

		public virtual bool EnableMergeAllSelectedButton
		{
			get => enableMergeAllSelectedButton;
			set
			{
				enableMergeAllSelectedButton = value;
				this.NotifyPropertyChanged();
			}
		}
		bool enableMergeAllSelectedButton;

		public virtual bool ShowIgnoreAllButton => false;

		public bool ShowIgnoreButton => (Master?.IsInDatabase ?? false) && ShowTargetRelatedButtons;

		public virtual bool ShowNotMatchButton => false;

		public virtual bool ShowRemoveAllIgnoresButton { get; set; }

		public virtual void SelectOrDeSelectAll()
		{
			throw new NotImplementedException();
		}

		public virtual void MergeAllSelected()
		{
			throw new NotImplementedException();
		}

		public virtual bool ConfirmMergeCandidateIntoMaster(IEnumerable<DuplicationModelDetailGroup> groups, Func<DuplicationModelDetailGroup, DuplicationModelDetailCollection> collectionGetter)
		{
			throw new NotImplementedException();
		}

		public virtual bool ConfirmMergeMasterIntoCandidate(IEnumerable<DuplicationModelDetailGroup> groups, Func<DuplicationModelDetailGroup, DuplicationModelDetailCollection> collectionGetter)
		{
			throw new NotImplementedException();
		}

		public virtual void IgnoreOrNotMatch(string status, bool forAll)
		{
			throw new NotImplementedException();
		}

		public virtual bool ShowRemoveIgnoresButton { get; protected set; }

		protected bool IgnoreOrNotMatchCore(string status, bool forAll, out MultilingualString message)
		{
			if (Master.Master.IsDeleted)
			{
				message = ResString.GetMultilingualString("B37D631C-44B2-47A4-8C23-9472AAB787DC", "Ignore could not be completed successfully because master record has been merged or deleted. Please close this form.");
				return false;
			}

			List<TCandidate> targets = null;
			if (forAll)
			{
				if (DuplicationCandidates != null)
				{
					targets = DuplicationCandidates.Cast<TCandidate>().ToList();
				}
			}
			else
			{
				if (SelectedCandidatePK != ZGuid.Empty)
				{
					targets = new List<TCandidate>() { CurrentSelectedCandidate };
				}
			}

			if (targets?.Count > 0)
			{
				var isLockedItems = false;

				using (var inputWithLock = targets.Select(x => x.TargetPK).ToList().ApplyAppLocks(PatternMatchingConstants.AppLockKey))
				{
					isLockedItems = inputWithLock.ItemsWithLocks.Count != targets.Count;

					foreach (var targetPk in inputWithLock.ItemsWithLocks)
					{
						var target = targets.First(x => x.TargetPK == targetPk.Item);

						IgnoreTarget(target, status);
					}
					PropagateDeduplicationAction(DeduplicationAction.Ignore);
				}

				if (isLockedItems)
				{
					message = ResString.GetMultilingualString("EA4754D9-112F-4FE6-85FF-33D28BE281E5", "Ignore could not be completed successfully because the target is being used by another process. Please try again later.");
					return false;
				}
				else
				{
					CreateIgnoreBillingTransaction?.Invoke(status, forAll, targets);
					message = null;
					return true;
				}
			}
			else
			{
				message = ResString.GetMultilingualString("41E92BC5-433C-4DC6-8C8C-75068A203995", "Ignore action could not be completed. Please try again.");
				return false;
			}
		}

		public virtual void RemoveIgnores(bool forAll)
		{
			throw new NotImplementedException();
		}

		public virtual bool ShowToggleExclusionButton => isAdminPanel && !IsEmpty && Master != null;

		public virtual bool ShowDeactivateMasterButton => false;

		public virtual bool ShowDeactivateButton => false;

		public bool ShowOpenMasterButton => isAdminPanel && !IsEmpty;

		public bool ShowOpenTargetButton => !isAdminPanel || !IsEmpty && ShowTargetRelatedButtons;

		public virtual bool ShowLinkButton => false;

		public virtual void OpenMaster()
		{
			throw new NotImplementedException();
		}

		public virtual void OpenTarget(string parameter)
		{
			throw new NotImplementedException();
		}

		public void CandidateBizOFormClosedHandler(BusinessObject bizO)
		{
			var masterBizOIsDeleted = Master.Master.IsDeleted;
			if (ReloadRequired)
			{
				if (!bizO.IsDeleted)
				{
					RegeneratePatternTables(bizO);
				}

				if (!masterBizOIsDeleted)
				{
					RefreshMaster();
				}
			}
			if (masterBizOIsDeleted)
			{
				CloseParent();
				PropagatePerformSearch(null, null);
			}
		}

		protected void RefreshMaster()
		{
			if (ReloadRequired)
			{
				var masterBizo = Master.Master;

				masterBizo.DeduplicationEnded -= OnDeduplicationEnded;
				masterBizo.DuplicationDetected -= OnDuplicationDetected;

				masterBizo.DeduplicationEnded += OnDeduplicationEnded;
				masterBizo.DuplicationDetected += OnDuplicationDetected;

				FindDuplicatesWithErrorBypass(masterBizo);
			}
		}

		protected virtual void FindDuplicatesWithErrorBypass(IDeduplicatable masterBizo)
		{
			masterBizo.FindDuplicatesBypassErrorChecking(false);
		}

		protected void OnDeduplicationEnded(object sender, IDuplicationEventArgs args)
		{
			if (!Master.Master.IsDuplicateFound)
			{
				CloseParent();
			}
		}

		protected void OnDuplicationDetected(object sender, IDuplicationEventArgs args)
		{
			ScoringResults = args.Results?.ToList();
			PtmResultModels = args.ResultsModels?.ToArray();
			Targets = ((IEnumerable<object>)args.TargetObjects).Cast<TMaster>();
			LoadInformationAction(SelectedCandidatePK);
		}

		protected BusinessObject GetBusinessObject(out string globalMessage)
		{
			BusinessObject result = null;
			globalMessage = null;
			if (Master == null)
			{
				globalMessage = Res.GetString("FF964886-85A2-4C3F-8847-7A4340289C59", "Select a master record");
			}
			else if (selectedCandidatePK == ZGuid.Empty)
			{
				globalMessage = Res.GetString("64C4B371-F35A-401E-AF4B-CADDD2A912C2", "Select a target record");
			}
			else
			{
				result = Factory.Load(Master.BizoType, SelectedCandidatePK);
			}

			return result;
		}

		protected abstract void RegeneratePatternTables(BusinessObject targetBizo);

		public virtual void ToggleExclusion()
		{
			throw new NotImplementedException();
		}

		public virtual void DeactivateMaster()
		{
			throw new NotImplementedException();
		}

		public virtual void DeactivateTarget()
		{
			throw new NotImplementedException();
		}

		public virtual void ExecuteLink()
		{
			throw new NotImplementedException();
		}

		#endregion Button

		protected void CloseParent()
		{
			closeParentAction?.Invoke();
		}

		protected bool DoesHeaderExist(OrgHeader targetOrganisation)
		{
			return targetOrganisation != null;
		}

		public void PropagatePerformSearch(object sender, EventArgs e)
		{
			PerformSearch?.Invoke(sender, e);
		}

		public void SetViewBusy(bool busy)
		{
			IsLoadingData = busy;
		}

		protected void SetCurrentSelection(ZGuid selectedItemPK)
		{
			if (DuplicationCandidates.Count > 0)
			{
				if (CurrentSelectedCandidate != null && selectedItemPK == CurrentSelectedCandidate.TargetPK)
				{
					SelectedCandidatePK = selectedItemPK;
				}
				else if (selectedItemPK.IsDefault)
				{
					SelectedCandidatePK = DuplicationCandidates[0].TargetPK;
				}
			}
			else
			{
				SelectedCandidatePK = Guid.Empty;
			}
		}

		protected void LoadFullList()
		{
			if (TargetsReloadRequired)
			{
				TargetsReloadRequired = false;
				LoadFullTargetGlows();
			}

			while (true)
			{
				var recordCount = ScoringResults.Count;
				using (Db.DisposableActionForDbConnection())
				{
					ScoringResults.AddRange(GetScoringResultsCore());
				}

				if (ScoringResults.Count > recordCount || ReloadRequired) //some records has been added
				{
					GetOrUpdateCandidatesForMerging();
					SetCurrentSelection(SelectedCandidatePK);
					ReloadRequired = false;
				}
				else
				{
					break;
				}
			}
		}

		protected virtual void LoadFullTargetGlows()
		{
			var targetGlowPKs = Targets.Select(u => u.PK).ToArray();
			var restResultModelPKs = PtmResultModels.Where(u => !targetGlowPKs.Contains(GetParentPK(u))).Select(GetParentPK).Distinct().ToArray();

			if (restResultModelPKs.Any())
			{
				var bizO = Master.Master as BusinessObject;
				if (bizO != null)
				{
					var readOnlyBusinessObjectFactory = new ReadOnlyBusinessObjectFactory();

					using (Db.DisposableActionForDbConnection())
					{
						Targets = Targets.Concat(GetRestTargetGlows(readOnlyBusinessObjectFactory, new HashSet<Guid>(restResultModelPKs), bizO)).ToArray();
					}
				}
			}
		}

		protected virtual Guid GetParentPK(PatternMatchingResultModel resultModel)
		{
			return Guid.Empty;
		}

		protected abstract IEnumerable<TMaster> GetRestTargetGlows(ReadOnlyBusinessObjectFactory readOnlyBusinessObjectFactory, HashSet<Guid> restResultModelPKs, BusinessObject bizO);
		public List<PropertyChangedNotify> NotifyPropertyChanges { get; } = new List<PropertyChangedNotify>();
	}

	public enum WaterMarkType
	{
		None,
		NoDeduplicates,
		TemporarilyIgnored,
	}
}
