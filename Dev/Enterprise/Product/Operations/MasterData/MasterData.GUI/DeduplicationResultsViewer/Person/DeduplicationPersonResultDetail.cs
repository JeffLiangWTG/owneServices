using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.BrandManager;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Tools.DuplicateDetector;
using CargoWise.Tools.DuplicateDetector.Standard.Common;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterData.Business;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.Recruiter.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterData.GUI
{
	public class DeduplicationPersonResultDetail : DeduplicationResultDetail<DeduplicationGlbPerson, DuplicationPersonCandidate>
	{
		public DeduplicationPersonResultDetail()
			: base()
		{
		}

		public DeduplicationPersonResultDetail(
			DeduplicationGlbPerson master,
			IEnumerable<DeduplicationGlbPerson> targets,
			IEnumerable<ScoringResult> scoringResults,
			IEnumerable<PatternMatchingResultModel> ptmResultModels,
			Action closeParentAction)
			: base(master, targets, scoringResults, ptmResultModels, closeParentAction)
		{
			ShowSelectAll = true;
		}

		public DeduplicationPersonResultDetail(
			DeduplicationGlbPerson master,
			IEnumerable<DeduplicationGlbPerson> targets,
			IEnumerable<ScoringResult> scoringResults,
			IEnumerable<PatternMatchingResultModel> ptmResultModels,
			bool isExcludingInactiveResults,
			bool isExcludingOtherCountries,
			bool isShowIgnoredResults,
			Dictionary<ScoringResult, PatternMatchingResult> resultDictionary)
			: base(master, targets, scoringResults, ptmResultModels, isExcludingInactiveResults, isExcludingOtherCountries, isShowIgnoredResults, resultDictionary)
		{
			ShowSelectAll = true;
		}

		public override SecurityCheckpoint ViewEntityFormCheckPoint => Env.Security.PersonIntelligenceView;
		public override SecurityCheckpoint DuplicateDetectionMergeCheckPoint => Env.Security.PersonIntelligenceDuplicateDetectionMerge;
		public override SecurityCheckpoint IgnoreDuplicatesCheckPoint => Env.Security.PersonIntelligenceDuplicateDetectionIgnoreDuplicatesForEveryone;

		protected override DuplicationPersonCandidate EmptyCandidate => DuplicationPersonCandidate.Empty;

		IDataProvider<IDeduplicationGlowObject> Provider => provider ?? (provider = new GlbPersonDataProvider(Master));
		IDataProvider<IDeduplicationGlowObject> provider;

		public Func<DuplicationPersonCandidate, bool> MeetFilterConditions { get; set; }

		public override void FilterAndUpdate()
		{
			if (UnfilteredResults != null && UnfilteredResults.Any())
			{
				var filteredDuplications = new DuplicationCandidateCollection<DuplicationPersonCandidate>();
				filteredDuplications.AddRange(UnfilteredResults.Where(x => MeetGeneralPotentialDuplicationsCondition(x) && (MeetFilterConditions?.Invoke(x) ?? true) || x.IsDummy));
				foreach (var duplicate in UnfilteredResults)
				{
					duplicate.IsDissolved = filteredDuplications.Contains(duplicate) && duplicate.IsDissolved;
					SetPotentialDuplicationModelInfo(duplicate);
				}
				DuplicationCandidates.RemoveAll();
				DuplicationCandidates.AddRange(filteredDuplications);
				UpdateDuplicationsCount();
			}
		}

		protected override void SetDummyDuplication()
		{
			if (!UnfilteredResults.Any(d => (MeetGeneralPotentialDuplicationsCondition(d))) && Targets.Any())
			{
				var presentModel = new DeduplicationPresenterModel
				{
					Confidence = ConfidenceRating.Undefined,
					MainScore = 0.99,
				};
				var target = Targets.First();
				var presenterModelName = GetDeduplicationPresenterModel(
					DeduplicationDisplayMode.List
					, DeduplicationProvider.Constants.Name
					, DeduplicationProvider.Constants.PersonNames
					, Master.PER_FullName
					, target.PER_PK);

				var deduplicationGlbPerson = new DeduplicationGlbPerson((GlbPerson)target.Master, true)
				{
					PER_IsActive = true,
					PER_FriendlyName = string.Empty,
					PER_BirthDate = null,
					PER_HomeAddress1 = string.Empty,
					PER_HomeAddress2 = string.Empty,
					PER_City = string.Empty,
					PER_Postcode = string.Empty,
					PER_State = string.Empty,
					PER_EmailAddress = string.Empty,
					PER_EmailAddress2 = string.Empty,
					PER_MobilePhone = string.Empty,
					PER_MobilePhone2 = string.Empty,
					PER_FaxNumber = string.Empty,
					PER_HomePhone = string.Empty,
					PER_RN_NKCountry = string.Empty,
				};

				var dummyCandidate = new DuplicationPersonCandidate(presentModel, deduplicationGlbPerson)
				{
					TargetPK = target.PER_PK,
					IsSameCountryAsMaster = true,
					DeduplicationPresenterModels = new List<DeduplicationPresenterModel> { presenterModelName }
				};
				SetPotentialDuplicationModelInfo(dummyCandidate);
				DuplicationCandidates.Add(dummyCandidate);
				SelectedCandidatePK = dummyCandidate.TargetPK;
				ShowSelectAll = false;
			}
		}

		DeduplicationPresenterModel GetDeduplicationPresenterModel(DeduplicationDisplayMode displayMode, string colName, string childGroupTypeName, string childMasterValue, Guid childTargetID)
		{
			return new DeduplicationPresenterModel
			{
				ChildComparisonResultScore = 0.99,
				ChildConfidenceRating = ConfidenceRating.Undefined,
				ChildDisplayModeForType = displayMode,
				ChildDisplayNameForMasterColumns = colName,
				ChildDisplayNameForTargetColumns = colName,
				ChildGroupNameForType = childGroupTypeName,
				ChildMasterID = Guid.Empty,
				ChildMasterValue = childMasterValue,
				ChildScore = 0.99,
				ChildScoringResultGroupRating = ConfidenceRating.Undefined,
				ChildTargetID = childTargetID,
				ChildTargetValue = string.Empty,
				GlowTargetType = typeof(DeduplicationGlbPerson),
				GroupNameForType = DeduplicationProvider.Constants.Person,
				MainScore = 0.99,
				Master = string.Empty,
				MasterType = typeof(DeduplicationGlbPerson),
				MasterValue = string.Empty,
				Target = string.Empty,
				TargetID = Guid.Empty,
				TargetValue = string.Empty
			};
		}

		protected string IgnoreTargetErrorMessage => ResString.GetMultilingualString("da3ede6e-546e-41a0-a889-b541a3e0a79b", "Ignore could not be completed successfully because target person has been merged or deleted. Please close this form.");

		protected override Guid GetParentPK(PatternMatchingResultModel resultModel)
		{
			return resultModel.PersonPK;
		}

		protected override IEnumerable<DuplicationPersonCandidate> GetDuplicationCandidatesCore() => Provider
			.GetDeduplicationDataSource(GenerateNewPresenterModels(), Targets, resultDictionary)
			.GetDuplicationCandidates()
			.Cast<DuplicationPersonCandidate>();

		protected override List<ScoringResult> GetScoringResultsCore()
		{
			return Provider.GetBusinessObjectBatcher(ScoringResults, PtmResultModels.ToArray(), Targets).ScoreResults();
		}

		public override ZGuid SelectedCandidatePK
		{
			get => base.SelectedCandidatePK;
			set
			{
				base.SelectedCandidatePK = value;
				SetPersonInfo();

				if (!isAdminPanel)
				{
					this.NotifyPropertyChanged();
					return;
				}

				if (value != ZGuid.Empty)
				{
					var newSelectCandidate = (DuplicationPersonCandidate)DuplicationCandidates.SingleOrDefault(x => ((DuplicationPersonCandidate)x).TargetPK == value);
					ShowRemoveIgnoresButton = newSelectCandidate?.IsIgnored ?? false;
					ShowTargetRelatedButtons = !newSelectCandidate?.IsDummy ?? false;
				}
				else
				{
					ShowRemoveIgnoresButton = false;
					ShowTargetRelatedButtons = false;
				}

				this.NotifyPropertyChanged();
			}
		}

		DeduplicationGlbPerson selectedPerson;
		protected void SetPersonInfo()
		{
			selectedPerson = Targets.FirstOrDefault(x => x.PK == SelectedCandidatePK);
			if (selectedPerson != null)
			{
				OpenStaffMenuItemVisible = selectedPerson.GlbStaffs.Any();
				OpenContactMenuItemVisible = selectedPerson.OrgContacts.Any();
				OpenApplicantMenuItemVisible = selectedPerson.HRJobApplicants.Any();
			}
			else
			{
				OpenStaffMenuItemVisible = false;
				OpenContactMenuItemVisible = false;
				OpenApplicantMenuItemVisible = false;
			}

			this.NotifyPropertyChanged(nameof(ShowTargetRelatedButtons));
		}

		protected override void SetPotentialDuplicationModelInfo(DuplicationPersonCandidate item)
		{
			var matchingResult = resultDictionary?.Values.FirstOrDefault(r => !r.IsDeleted && r.PMT_TargetPK == item.TargetPK);
			if (matchingResult != null)
			{
				item.SetAdminPanelProperties(matchingResult);
			}
		}

		public override void SelectOrDeSelectAll()
		{
			if (DuplicationCandidates.Count == 0)
			{
				return;
			}

			var isFirstDuplicationSelectedToBeDissolved = DuplicationCandidates[0].IsDissolved;
			if (isFirstDuplicationSelectedToBeDissolved)
			{
				foreach (var duplication in DuplicationCandidates.Cast<DuplicationPersonCandidate>())
				{
					duplication.IsDissolved = false;
				}
			}
			else
			{
				foreach (var duplication in DuplicationCandidates.Cast<DuplicationPersonCandidate>())
				{
					duplication.IsDissolved = true;
				}
			}
		}

		public override bool ShowMergeAllSelectedButton => !isAdminPanel || !IsEmpty;

		protected override void PostProcessDuplications()
		{
			foreach (var duplication in UnfilteredResults)
			{
				duplication.IsDissolvedChanged += (object sender, PropertyChangedEventArgs e) =>
				{
					EnableMergeAllSelectedButton = UnfilteredResults.Any(p => p.IsDissolved) && Master != null;
				};
			}
		}

		public override void MergeAllSelected()
		{
			if (!DuplicateDetectionMergeCheckPoint.IsAllowed)
			{
				DuplicateDetectionMergeCheckPoint.ShowError();
				return;
			}

			var potentialDuplicationToRetained = Master;
			var potentialDuplicationsToDissolved = UnfilteredResults.Where(person => person.IsDissolved).ToList();

			if (potentialDuplicationToRetained != null && potentialDuplicationsToDissolved.Any())
			{
				var personToRetained = Factory.Load<GlbPerson>(potentialDuplicationToRetained.PK);
				var personsToDissolved = new List<GlbPerson>();
				foreach (var potentialDuplicationToDissolved in potentialDuplicationsToDissolved)
				{
					var personToDissolved = Factory.Load<GlbPerson>(potentialDuplicationToDissolved.TargetPK);
					if (personToDissolved != null)
					{
						personsToDissolved.Add(personToDissolved);
					}
				}

				if (personToRetained != null && personsToDissolved.Count == potentialDuplicationsToDissolved.Count)
				{
					var pmsForm = new PersonMergeSummaryForm(personToRetained, personsToDissolved);
					DialogResult result = ZFormModaliser.ShowDialogAndDispose(pmsForm);

					OnPMSFormClosed(result, potentialDuplicationToRetained.PK, potentialDuplicationsToDissolved);

					if (result != DialogResult.Cancel)
					{
						PropagatePerformSearch(this, null);
					}
				}
				else
				{
					DisplayMergeSummaryFormFailure
					(
						ResString.GetMultilingualString("9F736C64-5783-4FE1-A0CE-6C1484EAF8E0", "At least one of the persons involved in the merge is not saved in the database."),
						ResString.GetMultilingualString("ED4AB9E0-B492-45E0-8A82-4AFA6459241A", "Restart the application, and ensure all persons involved in the merge are saved prior to merging.")
					);
				}
			}
			else
			{
				ErrorReporter.ReportOnce("Reaching ExecuteMergeAllCommand should not be allowed if minimum requirements are not satisfied.");
			}
		}

		protected virtual void OnPMSFormClosed(DialogResult result, Guid potentialDuplicationToBeRetained, List<DuplicationPersonCandidate> duplicationPersonCandidatesToBeDissolved)
		{
			if (result == DialogResult.Yes)
			{
				foreach (var duplication in duplicationPersonCandidatesToBeDissolved)
				{
					RemoveDuplication(duplication);
				}

				LoadInformationAction(potentialDuplicationToBeRetained);
			}
		}

		protected void RemoveDuplication(DuplicationPersonCandidate duplicationCandidateToRemove)
		{
			if (duplicationCandidateToRemove != null && DuplicationCandidates.Contains(duplicationCandidateToRemove))
			{
				ScoringResults = ScoringResults.Where(x => x.TargetPK != duplicationCandidateToRemove.TargetPK).ToList();

				Targets = Targets.Where(x => x.PK != duplicationCandidateToRemove.TargetPK).ToArray();

				DuplicationCandidates.Remove(duplicationCandidateToRemove);
				UpdateDuplicationsCount();

				if (!DuplicationCandidates.Any())
				{
					Master.Master.ValidateDuplicationResult(false);
					CloseParent();
					SelectedCandidatePK = Guid.Empty;
				}
				else
				{
					SelectedCandidatePK = DuplicationCandidates[0].TargetPK;
				}
			}
		}

		protected void DisplayMergeSummaryFormFailure(string exceptionDetails, string userActionsRequired)
		{
			Globals.Message.ShowError(ResString.GetMultilingualString("A3BF5942-FC93-4B4E-9EE0-035591A4F887",
				"Unable to execute merge operation.\r\n\r\n" +
				"The reason for the failure is:\r\n{0}\r\n\r\n" +
				"You can rectify this by doing the following and re-merging:\r\n{1} If the problem persists, please contact {2}.",
				exceptionDetails,
				userActionsRequired,
				BrandingFactory.Instance.ProductSupportName));
		}

		public GlbPersonDuplicationFinderProxy FinderProxy => GlbPersonDuplicationFinderProxy.GetInstance(((GlbPerson)Master.Master));

		protected override IEnumerable<DeduplicationGlbPerson> GetRestTargetGlows(ReadOnlyBusinessObjectFactory readOnlyBusinessObjectFactory, HashSet<Guid> restResultModelPKs, BusinessObject bizO)
		{
			if (isAdminPanel)
			{
				return Array.Empty<DeduplicationGlbPerson>();
			}

			var restTargetBizOs = GlbPersonDuplicationFinder.LoadPersonTargetBizOs(readOnlyBusinessObjectFactory, restResultModelPKs, Master, (GlbPerson)bizO, null);
			return restTargetBizOs.Select(GlbPersonDuplicationFinder.ConvertPersonToGlowModel).Cast<DeduplicationGlbPerson>();
		}

		public override bool ShowRemoveIgnoresButton
		{
			get => ignoreCountBiggerThanZero && isAdminPanel;
			protected set
			{
				ignoreCountBiggerThanZero = value;
				this.NotifyPropertyChanged();
			}
		}

		public override bool ShowIgnoreAllButton => isAdminPanel && !IsEmpty && ShowTargetRelatedButtons;

		public override bool ShowNotMatchButton => true;

		public override bool ShowRemoveAllIgnoresButton => isAdminPanel && ignoreCountBiggerThanZero && DuplicationCandidates.Count > 1;

		public override void IgnoreOrNotMatch(string status, bool forAll)
		{
			if (!Env.Security.PersonIntelligenceDuplicateDetectionIgnoreDuplicates.IsAllowed)
			{
				Env.Security.PersonIntelligenceDuplicateDetectionIgnoreDuplicates.ShowError();
				return;
			}

			if (!IgnoreDuplicatesCheckPoint.IsAllowed && forAll)
			{
				IgnoreDuplicatesCheckPoint.ShowError();
				return;
			}

			if (!IgnoreOrNotMatchCore(status, forAll, out var message))
			{
				Globals.Message.ShowInformation(message);
			}
			else
			{
				SetInfoForDuplicationCandidates(DuplicationCandidates);
			}
		}

		protected override void IgnoreTarget(DuplicationPersonCandidate target, string status)
		{
			var targetPerson = Factory.Load<GlbPerson>(target.TargetPK);
			if (targetPerson == null)
			{
				Globals.Message.ShowInformation(IgnoreTargetErrorMessage);
			}
			else
			{
				FinderProxy.AddIgnore(targetPerson, UserIgnoreStatus.FromString(status), GlbStaff.CurrentUser.GS_Code);
				if (!isAdminPanel)
				{
					RemoveSelectedDuplication();
					UpdateDuplicationsCount();
				}
				ShowRemoveIgnoresButton = true;
			}
		}

		public override void RemoveIgnores(bool forAll)
		{
			List<DuplicationPersonCandidate> targets = null;
			if (forAll)
			{
				if (DuplicationCandidates != null)
				{
					targets = DuplicationCandidates.Cast<DuplicationPersonCandidate>().ToList();
				}
			}
			else
			{
				if (SelectedCandidatePK != Guid.Empty)
				{
					targets = new List<DuplicationPersonCandidate>() { CurrentSelectedCandidate };
				}
			}

			foreach (var target in targets)
			{
				var personTarget = Factory.Load<GlbPerson>(target.TargetPK);
				FinderProxy.RemoveTemporaryIgnore(personTarget);
				FinderProxy.RemovePermanentIgnore(personTarget);

				Factory.Save();
			}
			SetInfoForDuplicationCandidates(DuplicationCandidates);
			SelectedCandidatePK = SelectedCandidatePK;
		}

		public bool OpenStaffMenuItemVisible { get; private set; }
		public bool OpenContactMenuItemVisible { get; private set; }
		public bool OpenApplicantMenuItemVisible { get; private set; }

		public override void ToggleExclusion()
		{
			try
			{
				var master = Master.Master;
				master.IsExcludedFromDeduplication = !master.IsExcludedFromDeduplication;
				PropagateDeduplicationAction(DeduplicationAction.MasterExclusionToggled);
				this.NotifyPropertyChanged(nameof(master.IsExcludedFromDeduplication));
			}
			catch (InvalidOperationException x)
			{
				Globals.Message.ShowInformation(x.Message);
			}
		}

		public override void OpenMaster()
		{
			if (Master != null)
			{
				var bizO = Master.Master as BusinessObject;
				this.ShowFormForMaster(bizO, Master.BizoType);
				PropagateDeduplicationAction(DeduplicationAction.OpenMaster);
			}
		}

		public override void OpenTarget(string parameter)
		{
			if (parameter == null || parameter == TextConstant.Person)
			{
				OpenPersonTypeTarget();
			}
			else
			{
				if (parameter == TextConstant.Staff)
				{
					var bizO = Factory.Load(typeof(GlbStaff), selectedPerson.GlbStaffs.First().GS_PK);
					this.ShowFormForCandidate(bizO, typeof(GlbStaff));
				}
				else if (parameter == TextConstant.Contact)
				{
					var bizO = Factory.Load(typeof(OrgContact), selectedPerson.OrgContacts.First().OC_PK);
					this.ShowFormForCandidate(bizO, typeof(OrgContact));
				}
				else
				{
					var bizO = Factory.Load(typeof(HRJobApplicant), selectedPerson.HRJobApplicants.First().HA_PK);
					this.ShowFormForCandidate(bizO, typeof(HRJobApplicant));
				}

				PropagateDeduplicationAction(DeduplicationAction.OpenTarget);
			}
		}

		void OpenPersonTypeTarget()
		{
			string message;
			var bizO = GetBusinessObject(out message);
			if (message == null)
			{
				this.ShowFormForCandidate(bizO, Master.BizoType);
				PropagateDeduplicationAction(DeduplicationAction.OpenTarget);
			}
			else
			{
				Globals.Message.ShowInformation(message);
			}
		}

		protected override void RegeneratePatternTables(BusinessObject targetBizo)
		{
			var person = GetGlbPersonFromBizO(targetBizo);
			if (person != null)
			{
				var recalculator = new PatternMatchingRecalculator<GlbPerson>(person);
				recalculator.Regenerate(ObjectFactory.Get<IMasterDataProvider>().GetDeduplicationGlbPerson(person));
			}
			else
			{
				ErrorReporter.ReportOnce("b6153396-6d5e-40df-bb43-ce4650e54826", "Failed to find GlbPerson from " + targetBizo.TableName);
			}
		}

		GlbPerson GetGlbPersonFromBizO(BusinessObject bizO)
		{
			GlbPerson glbPerson;

			switch (bizO)
			{
				case GlbPerson person:
					glbPerson = person;
					break;
				case OrgContact contact:
					glbPerson = contact.Person;
					break;
				case GlbStaff staff:
					glbPerson = staff.Person;
					break;
				case HRJobApplicant jobApplicant:
					glbPerson = jobApplicant.Person;
					break;
				default:
					glbPerson = null;
					break;
			}

			return glbPerson;
		}

		protected override void FindDuplicatesWithErrorBypass(IDeduplicatable masterBizo)
		{
			if (isAdminPanel)
			{
				masterBizo.FindDuplicatesBypassErrorChecking(true);
				return;
			}
			base.FindDuplicatesWithErrorBypass(masterBizo);
		}
	}
}
