using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Tools.DuplicateDetector;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.MasterData.Business;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Registry.Business.OrganisationsDataRegistry;

namespace Enterprise.MasterData.GUI
{
	public partial class DeduplicationOrganizationsUserControl : ZUserControl, IDeduplicationBusinessObjectUserControl
	{
		public DeduplicationOrganizationsUserControl()
		{
			InitializeComponent();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			BindingSource.DataSource = Manager.DeduplicationOrganisationCollection;
			AddFilterControl();
			EnsureFilterGridVisible();
			DeduplicationBusinessObjectFilterControl.Grid.ListManager.CurrentChanged += ListManager_CurrentChanged;

			SetupDuplicationDetailView(EmptyResultDetail);

			if (PerformSearchOnLoad)
			{
				ResetFilterStripAndPerformDefaultCodeSearchCore();
			}
		}

		bool PerformSearchOnLoad { get; set; }
		ZString DefaultCode { get; set; }

		DeduplicationOrganisation CurrentDeduplicationOrganisation => DeduplicationBusinessObjectFilterControl.Grid.ListManager.GetCurrent() as DeduplicationOrganisation;

		DeduplicationOrganisationResultDetail EmptyResultDetail => emptyResultDetail ?? (emptyResultDetail = new DeduplicationOrganisationResultDetail());
		DeduplicationOrganisationResultDetail emptyResultDetail;

		#region IDeduplicationBusinessObjectUserControl Members

		FilterStripBusinessObject deduplicationOrganisationFilterBizo;

		public FilterStripBusinessObject DeduplicationBusinessObjectFilterBizo => deduplicationOrganisationFilterBizo ?? (deduplicationOrganisationFilterBizo = GetNewDeduplicationOrganisationFilterBusinessObject());

		ZFilterStripControl deduplicationOrganisationFilterControl;

		public ZFilterStripControl DeduplicationBusinessObjectFilterControl => deduplicationOrganisationFilterControl ?? (deduplicationOrganisationFilterControl = GetNewDeduplicationOrganisationFilterControl());

		public void ResetFilterStripAndPerformDefaultCodeSearch(FilterBusinessObjectDefaults defaults, bool waitforLoad = false)
		{
			DeduplicationBusinessObjectFilterBizo.SetExternalDefaults(defaults);
			DefaultCode = (ZString)defaults["Code:Property"].Value;
			PerformSearchOnLoad = waitforLoad;

			if (!waitforLoad)
			{
				ResetFilterStripAndPerformDefaultCodeSearchCore();
			}
		}

		#endregion

		void ResetFilterStripAndPerformDefaultCodeSearchCore()
		{
			// Need to remove current set filters and manually populate default property again
			DeduplicationBusinessObjectFilterControl.ResetFilterStrips();
			(DeduplicationBusinessObjectFilterBizo.ActiveModuleFilters[0] as ModuleTextFilter).Property = DefaultCode;

			DeduplicationBusinessObjectFilterControl.FirePerformSearch();
		}

		protected void RefreshRetainedRecord()
		{
			var dedupOrg = CurrentDeduplicationOrganisation;

			if (dedupOrg != null && dedupOrg.PK == CurrentlySelectedOrg.PK)
			{
				CurrentlySelectedOrg = Factory.LoadTop1<OrgHeader>(new ZDBOnlyQuery(typeof(OrgHeader)).AddToFilter(OrgHeaderSchema.PK, SQLComparisonOperator.Equal, dedupOrg.PK));
				SetupDuplicationDetailView(dedupOrg);
			}
		}

		protected virtual void ListManager_CurrentChanged(object sender, EventArgs e)
		{
			var dedupOrg = CurrentDeduplicationOrganisation;

			if (dedupOrg != null && dedupOrg.Factory.Load<OrgHeader>(dedupOrg.PK) == null)
			{
				// Refresh filter control to remove deleted organisations
				DeduplicationBusinessObjectFilterControl.FirePerformSearch();
			}
			else if (dedupOrg != null && Instance.EnableDeduplicationFinder.Value)
			{
				if (CurrentlySelectedOrg?.PK != dedupOrg.PK)
				{
					SetCurrentlySelectedOrgWithEventHandler(dedupOrg);
				}
				else
				{
					return; //FindPotentialDuplicates will change dedupOrg and call this method again
				}
				dedupOrg.DuplicateSearchingFinished -= DedupOrg_DuplicateSearchingFinished;
				SetupDuplicationDetailView(EmptyResultDetail);
				SetupDuplicationDetailView(dedupOrg);
			}
			else
			{
				if (!Instance.EnableDeduplicationFinder.Value && !deduplicationDisabledMessageHasShown)
				{
					Globals.Message.ShowWarning(ResString.GetMultilingualString("f6c5fcca-7f43-4669-a8fa-e128fffb9846", "De-duplication functionalities has been disabled, please turn it on in Registry to access Organization De-duplication."));
					deduplicationDisabledMessageHasShown = true;
				}

				SetCurrentlySelectedOrgWithEventHandler(null);

				SetupDuplicationDetailView(EmptyResultDetail);
			}
		}

		void SetCurrentlySelectedOrgWithEventHandler(DeduplicationOrganisation org)
		{
			if (CurrentlySelectedOrg != null)
			{
				CurrentlySelectedOrg.DeduplicationActionOccurred -= CurrentOrgDeduplicationActionOccurred;
			}

			CurrentlySelectedOrg = org?.MasterOrgHeader;
			if (CurrentlySelectedOrg != null)
			{
				CurrentlySelectedOrg.DeduplicationActionOccurred += CurrentOrgDeduplicationActionOccurred;
			}
		}

		internal void SetupDuplicationDetailView(DeduplicationOrganisation dedupOrg, IEnumerable<ScoringResult> scoringResults = null, bool isReloadRequired = false)
		{
			((IDeduplicatable)CurrentlySelectedOrg).ShouldRunDeduplication = true;
			if ((dedupOrg.DOH_Status == DeduplicationHelper.StatusConstants.Processed
				|| dedupOrg.DOH_Status == DeduplicationHelper.StatusConstants.Error
				|| dedupOrg.DOH_Status == DeduplicationHelper.StatusConstants.Excluded)
				&& !isReloadRequired)
			{
				var results = new Dictionary<ScoringResult, PatternMatchingResult>();
				if (scoringResults != null && scoringResults.Any())
				{
					foreach (PatternMatchingResult result in dedupOrg.MatchingResultCollection)
					{
						results[scoringResults.FirstOrDefault(r => r.MasterPK == result.PMT_MasterPK && r.TargetPK == result.PMT_TargetPK)] = result;
					}
				}
				else
				{
					foreach (PatternMatchingResult result in dedupOrg.MatchingResultCollection)
					{
						results[TargetScorerController.Score(new DeduplicationOrgHeader(CurrentlySelectedOrg), new DeduplicationOrgHeader(result.TargetOrganisation), true)] = result;
					}
				}
				var listOfTargetGlows = dedupOrg.MatchingResultCollection.Select(result => new DeduplicationOrgHeader(result.TargetOrganisation)).ToList();

				if (!results.Any())
				{
					results[TargetScorerController.Score(new DeduplicationOrgHeader(CurrentlySelectedOrg), new DeduplicationOrgHeader(TempPatternMatchingResult.TargetOrganisation), true)] = TempPatternMatchingResult;
					listOfTargetGlows.Add(TempDeduplicationOrgHeader);
				}

				var resultDetail = new DeduplicationOrganisationResultDetail(new DeduplicationOrgHeader(CurrentlySelectedOrg), listOfTargetGlows, results.Keys.ToArray(), Enumerable.Empty<PatternMatchingResultModel>(), IsExcludingInactive, IsExcludingOtherCountries, IsShowIgnored, results);
				resultDetail.PerformSearch += (sender, e) => DeduplicationBusinessObjectFilterControl.FirePerformSearch();
				SetupDuplicationDetailView(resultDetail);
			}
			else
			{
				SetViewBusy(true);
				dedupOrg.DuplicateSearchingFinished += DedupOrg_DuplicateSearchingFinished;
				if (Globals.IsTest)
				{
					DedupOrg_DuplicateSearchingFinished(dedupOrg, null);
				}
				else
				{
					Task.Run(dedupOrg.FindPotentialDuplicates);
				}
			}
		}

		protected void CurrentOrgDeduplicationActionOccurred(object sender, IDuplicationEventArgs e)
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
				RefreshDeduplicationDetailView();
			}
			else if (e.InvokedAction == DeduplicationAction.ReloadRequired)
			{
				RefreshDeduplicationDetailView(isReloadRequired: true);
			}
			else if (e.InvokedAction == DeduplicationAction.ShowIgnoredFilterChanged)
			{
				IsShowIgnored = e.IsShowIgnoredFromResults;
			}
			else if (e.InvokedAction == DeduplicationAction.Ignore && !IsShowIgnored)
			{
				RefreshDeduplicationDetailView();
			}

			void RefreshDeduplicationDetailView(bool isReloadRequired = false)
			{
				var currentItem = CurrentDeduplicationOrganisation;
				currentItem.Reload();
				SetupDuplicationDetailView(currentItem, null, isReloadRequired);
			}
		}
		
		void SetViewBusy(bool busy)
		{
			ResultsViewerUserControl.SetViewBusy(busy);
		}

#if DEBUG
		protected virtual
#endif
	void DedupOrg_DuplicateSearchingFinished(object sender, DuplicateSearchingFinishedEventArgs e)
		{
			var dedupOrg = sender as DeduplicationOrganisation;

			dedupOrg.DuplicateSearchingFinished -= DedupOrg_DuplicateSearchingFinished;
			if (dedupOrg.PK == CurrentlySelectedOrg.PK)
			{
				ApplicationDispatcher.Current.Invoke(() =>
				{
					dedupOrg.SaveDeduplicationResults(e.ScoringResults);
					SetupDuplicationDetailView(dedupOrg, e.ScoringResults);
				});
			}
		}

		bool deduplicationDisabledMessageHasShown;

		void SetupDuplicationDetailView(DeduplicationOrganisationResultDetail resultDetail)
		{
			ResultsViewerUserControl.SetupDataContext(resultDetail, isAdminPanel: true);
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

		protected virtual FilterStripBusinessObject GetNewDeduplicationOrganisationFilterBusinessObject()
		{
			return ObjectFactory.Get<FilterStripBusinessObject>("DeduplicationOrganisationFilterBusinessObject");
		}

		protected ZFilterStripControl GetNewDeduplicationOrganisationFilterControl()
		{
			return new DeduplicationOrganisationFilterControl(Manager.DeduplicationOrganisationCollection, DeduplicationBusinessObjectFilterBizo)
			{
				Name = "DeduplicationOrganisationFilterControl",
				Dock = DockStyle.Fill
			};
		}

		void AddFilterControl()
		{
			FilterControlGroupBox.Controls.Add(DeduplicationBusinessObjectFilterControl);
		}

		public OrgHeader CurrentlySelectedOrg { get; set; }
		public bool IsExcludingOtherCountries { get; set; }
		public bool IsExcludingInactive { get; set; }

		public bool IsShowIgnored { get; set; }

		AdministrationPanelManager Manager => ((ZForm)ParentForm).BusinessEntity as AdministrationPanelManager;

		#region Dispose
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				CurrentlySelectedOrg.DeduplicationActionOccurred -= CurrentOrgDeduplicationActionOccurred;
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#endregion

		BusinessObjectFactory Factory => factory ?? (factory = new BusinessObjectFactory());
		BusinessObjectFactory factory;

		PatternMatchingResult TempPatternMatchingResult
		{
			get
			{
				if (tempPatternMatchingResult == null)
				{
					tempPatternMatchingResult = Factory.New<PatternMatchingResult>();
					tempPatternMatchingResult.PMT_TargetPK = TempDeduplicationOrgHeader.PK;
					tempPatternMatchingResult.PMT_Status = "NDU";
				}
				return tempPatternMatchingResult;
			}
		}
		PatternMatchingResult tempPatternMatchingResult;

		DeduplicationOrgHeader TempDeduplicationOrgHeader
		{
			get
			{
				if (tempDeduplicationOrgHeader == null)
				{
					var tempOrgHeader = Factory.New<OrgHeader>();
					((IDeduplicatable)tempOrgHeader).ShouldRunDeduplication = true;
					var tempAddress = tempOrgHeader.Addresses.AddNew();
					tempAddress.OA_Address1 = "A1";
					tempDeduplicationOrgHeader = new DeduplicationOrgHeader(tempOrgHeader, true);
				}
				return tempDeduplicationOrgHeader;
			}
		}
		DeduplicationOrgHeader tempDeduplicationOrgHeader;
	}
}
