using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Tools.DuplicateDetector;
using CargoWise.Windows.UI;
using Enterprise.MasterData.Business;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterData.GUI
{
	public partial class DeduplicationPersonsUserControl : ZUserControl, IDeduplicationBusinessObjectUserControl
	{
		public DeduplicationPersonsUserControl()
		{
			InitializeComponent();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			BindingSource.DataSource = DeduplicationPersonCollection;
			AddFilterControl();
			EnsureFilterGridVisible();
			DeduplicationBusinessObjectFilterControl.Grid.ListManager.CurrentChanged += ListManager_CurrentChanged;

			SetupDuplicationDetailView(EmptyResultDetail);

			if (PerformSearchOnLoad)
			{
				ResetFilterStripAndPerformDefaultCodeSearchCore();
			}
		}

		#region IDeduplicationBusinessObjectUserControl Member

		FilterStripBusinessObject deduplicationPersonFilterBizo;

		public FilterStripBusinessObject DeduplicationBusinessObjectFilterBizo => deduplicationPersonFilterBizo ?? (deduplicationPersonFilterBizo = GetNewDeduplicationPersonFilterBusinessObject());

		ZFilterStripControl deduplicationPersonFilterControl;

		public ZFilterStripControl DeduplicationBusinessObjectFilterControl => deduplicationPersonFilterControl ?? (deduplicationPersonFilterControl = GetNewDeduplicationPersonFilterControl());

		public void ResetFilterStripAndPerformDefaultCodeSearch(FilterBusinessObjectDefaults defaults, bool waitforLoad = false)
		{
			//TODO: implement the logic
			throw new NotImplementedException();
		}

		#endregion

		bool deduplicationDisabledMessageHasShown;
		BusinessObjectFactory factory;
		PatternMatchingResult tempPatternMatchingResult;
		DeduplicationGlbPerson tempDeduplicationGlbPerson;
		DeduplicationPersonCollection deduplicationPersonCollection;

		public GlbPerson CurrentlySelectedPerson { get; set; }
		public bool IsExcludingOtherCountries { get; set; }
		public bool IsExcludingInactive { get; set; }
		AdministrationPanelManager Manager => ((ZForm)ParentForm).BusinessEntity as AdministrationPanelManager;
		bool PerformSearchOnLoad { get; set; }
		BusinessObjectFactory Factory => factory ?? (factory = new BusinessObjectFactory());

		DeduplicationPersonCollection DeduplicationPersonCollection => deduplicationPersonCollection ?? (deduplicationPersonCollection = new DeduplicationPersonCollection(Manager.Factory));

		DeduplicationPersonResultDetail EmptyResultDetail => emptyResultDetail ?? (emptyResultDetail = new DeduplicationPersonResultDetail());
		DeduplicationPersonResultDetail emptyResultDetail;

		PatternMatchingResult TempPatternMatchingResult
		{
			get
			{
				if (tempPatternMatchingResult == null)
				{
					tempPatternMatchingResult = Factory.New<PatternMatchingResult>();
					tempPatternMatchingResult.PMT_TargetPK = TempDeduplicationGlbPerson.PER_PK;
					tempPatternMatchingResult.PMT_Status = "NDU";
				}

				return tempPatternMatchingResult;
			}
		}

		DeduplicationGlbPerson TempDeduplicationGlbPerson
		{
			get
			{
				if (tempDeduplicationGlbPerson == null)
				{
					var tempGlbPerson = Factory.New<GlbPerson>();
					tempDeduplicationGlbPerson = tempGlbPerson.CreateIGlbPerson() as DeduplicationGlbPerson;
				}

				return tempDeduplicationGlbPerson;
			}
		}

		void ResetFilterStripAndPerformDefaultCodeSearchCore()
		{
			//TODO: implement the logic
			throw new NotImplementedException();
		}

		void AddFilterControl()
		{
			FilterControlGroupBox.Controls.Add(DeduplicationBusinessObjectFilterControl);
		}

		void EnsureFilterGridVisible()
		{
			var filterStripsPanelSearchResult = DeduplicationBusinessObjectFilterControl.Controls.Find("FilterStripsPanel", true);
			if (filterStripsPanelSearchResult.Length > 0)
			{
				var filterStripsPanel = filterStripsPanelSearchResult[0];
				var preferedHeight = filterStripsPanel.Height + ControlDpiScalingHelper.ScaleToCurrentDpiY(150);
				if (MainSplitContainer.SplitterDistance < preferedHeight)
				{
					MainSplitContainer.SplitterDistance = preferedHeight;
				}
			}
		}
		void RefreshFilterGrid()
		{
			DeduplicationBusinessObjectFilterControl.FirePerformSearch();
		}

		protected virtual FilterStripBusinessObject GetNewDeduplicationPersonFilterBusinessObject()
		{
			return new DeduplicationPersonFilterBusinessObject();
		}

		protected ZFilterStripControl GetNewDeduplicationPersonFilterControl()
		{
			return new DeduplicationPersonFilterControl(DeduplicationPersonCollection, DeduplicationBusinessObjectFilterBizo)
			{
				Name = "DeduplicationPersonFilterControl",
				Dock = DockStyle.Fill
			};
		}

		protected virtual void ListManager_CurrentChanged(object sender, EventArgs e)
		{
			var dedupPerson = DeduplicationBusinessObjectFilterControl.Grid.ListManager.GetCurrent() as DeduplicationPerson;

			if (dedupPerson != null && dedupPerson.Factory.Load<GlbPerson>(dedupPerson.PK) == null)
			{
				RefreshFilterGrid();
			}
			else if (dedupPerson != null && SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.Value)
			{
				if (CurrentlySelectedPerson?.PK != dedupPerson.PK)
				{
					SetCurrentlySelectedPersonWithEventHandler(dedupPerson);
				}
				else
				{
					return; //FindPotentialDuplicates will change dedupPerson and call this method again
				}

				dedupPerson.DuplicateSearchingFinished -= DedupPerson_DuplicateSearchingFinished;
				SetupDuplicationDetailView(EmptyResultDetail);
				SetupDuplicationDetailView(dedupPerson);
			}
			else
			{
				if (!SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.Value && !deduplicationDisabledMessageHasShown)
				{
					Globals.Message.ShowWarning(ResString.GetMultilingualString("0d04b494-4a65-4382-a0f5-77d877cc4b01", "De-duplication functionality has been disabled, please turn it on in Registry to access Person De-duplication."));
					deduplicationDisabledMessageHasShown = true;
				}

				SetCurrentlySelectedPersonWithEventHandler(null);
				SetupDuplicationDetailView(EmptyResultDetail);
			}
		}

#if DEBUG
		protected virtual
#endif
		void DedupPerson_DuplicateSearchingFinished(object sender, DuplicateSearchingFinishedEventArgs e)
		{
			var dedupPerson = sender as DeduplicationPerson;
			dedupPerson.DuplicateSearchingFinished -= DedupPerson_DuplicateSearchingFinished;

			if (dedupPerson.PK == CurrentlySelectedPerson.PK)
			{
				ApplicationDispatcher.Current.Invoke(() =>
				{
					dedupPerson.SaveDeduplicationResults(e.ScoringResults);
					SetupDuplicationDetailView(dedupPerson, e.ScoringResults);
				});
			}
		}

		internal void SetupDuplicationDetailView(DeduplicationPerson dedupPerson, IEnumerable<ScoringResult> scoringResults = null)
		{
			((IDeduplicatable)CurrentlySelectedPerson).ShouldRunDeduplication = true;

			if (dedupPerson.DPE_Status == DeduplicationHelper.StatusConstants.Processed
				|| dedupPerson.DPE_Status == DeduplicationHelper.StatusConstants.Error
				|| dedupPerson.DPE_Status == DeduplicationHelper.StatusConstants.Excluded)
			{
				var results = new Dictionary<ScoringResult, PatternMatchingResult>();
				if (scoringResults != null && scoringResults.Any())
				{
					foreach (PatternMatchingResult result in dedupPerson.MatchingResultCollection)
					{
						results[scoringResults.FirstOrDefault(r => r.MasterPK == result.PMT_MasterPK && r.TargetPK == result.PMT_TargetPK)] = result;
					}
				}
				else
				{
					foreach (PatternMatchingResult result in dedupPerson.MatchingResultCollection)
					{
						results[TargetScorerController.Score(CurrentlySelectedPerson.CreateIGlbPerson(), result.TargetPerson.CreateIGlbPerson(), true)] = result;
					}
				}
				var listOfTargetGlows = dedupPerson.MatchingResultCollection.Select(result => result.TargetPerson.CreateIGlbPerson() as DeduplicationGlbPerson).ToList();

				if (!results.Any())
				{
					results[TargetScorerController.Score(CurrentlySelectedPerson.CreateIGlbPerson(), TempDeduplicationGlbPerson, true)] = TempPatternMatchingResult;
					listOfTargetGlows.Add(TempDeduplicationGlbPerson);
				}

				var resultDetail = new DeduplicationPersonResultDetail(CurrentlySelectedPerson.CreateIGlbPerson() as DeduplicationGlbPerson, listOfTargetGlows, results.Keys.ToArray(), Enumerable.Empty<PatternMatchingResultModel>(), IsExcludingInactive, IsExcludingOtherCountries, isShowIgnoredResults: true, results);
				resultDetail.PerformSearch += (sender, e) => DeduplicationBusinessObjectFilterControl.FirePerformSearch();
				SetupDuplicationDetailView(resultDetail);
			}
			else
			{
				SetViewBusy(true);
				dedupPerson.DuplicateSearchingFinished += DedupPerson_DuplicateSearchingFinished;

				if (Globals.IsTest)
				{
					DedupPerson_DuplicateSearchingFinished(dedupPerson, null);
				}
				else
				{
					Task.Run(dedupPerson.FindPotentialDuplicates);
				}
			}
		}

		void SetupDuplicationDetailView(DeduplicationPersonResultDetail resultDetail)
		{
			ResultsViewerUserControl.SetupDataContext(resultDetail, isAdminPanel: true);
		}

		void SetViewBusy(bool busy)
		{
			ResultsViewerUserControl.SetViewBusy(busy);
		}

		void SetCurrentlySelectedPersonWithEventHandler(DeduplicationPerson dedupPerson)
		{
			if (CurrentlySelectedPerson != null)
			{
				CurrentlySelectedPerson.DeduplicationActionOccurred -= CurrentPersonDeduplicationActionOccurred;
			}

			CurrentlySelectedPerson = dedupPerson?.MasterGlbPerson;
			if (CurrentlySelectedPerson != null)
			{
				CurrentlySelectedPerson.DeduplicationActionOccurred += CurrentPersonDeduplicationActionOccurred;
			}
		}

		protected void CurrentPersonDeduplicationActionOccurred(object sender, IDuplicationEventArgs e)
		{
			if (e.InvokedAction == DeduplicationAction.ExcludeCountriesFilterChanged)
			{
				IsExcludingOtherCountries = e.IsExcludingOtherCountriesFromResults;
			}
			else if (e.InvokedAction == DeduplicationAction.ExcludeInactiveFilterChanged)
			{
				IsExcludingInactive = e.IsExcludingInactiveFromResults;
			}
			else if (e.InvokedAction == DeduplicationAction.MasterExclusionToggled)
			{
				var currentItem = DeduplicationBusinessObjectFilterControl.Grid.ListManager.GetCurrent() as DeduplicationPerson;
				currentItem.Reload();
				SetupDuplicationDetailView(currentItem);
			}
		}

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				if (CurrentlySelectedPerson != null)
				{
					CurrentlySelectedPerson.DeduplicationActionOccurred -= CurrentPersonDeduplicationActionOccurred;
				}

				components.Dispose();
			}

			base.Dispose(disposing);
		}

		#endregion
	}
}
