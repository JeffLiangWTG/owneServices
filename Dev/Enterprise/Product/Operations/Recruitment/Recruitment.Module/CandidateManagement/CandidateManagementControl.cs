using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Recruitment.Common;
using Enterprise.Recruitment.Module.CandidateManagement;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Shell.Core.Public.Modules;
using Enterprise.ZArchitecture.Modules;

using static CargoWise.Windows.UI.ControlDpiScalingHelper;

namespace Enterprise.Recruitment.Module
{
	public partial class CandidateManagementControl : ZUserControl
	{
		readonly CandidateModuleBusinessObject module;
		BindingManagerBase CandidateGridBinding => BindingContext[module, "Collection"];

		[Obsolete("This is only for the designer, use the constructor that accepts arguments")]
		public CandidateManagementControl()
			=> InitializeComponent();

		int lastPosition;
		public CandidateManagementControl(BusinessObjectFactory factory, CandidateModuleBusinessObject module)
		{
			InitializeComponent();
			this.module = module;

			AddToolbarButtons();
			SetDataBinding(module, "");
			AddStripControl(factory);
			grid.MouseDoubleClick += (o, e) => Grid_MouseDoubleClick();
			pageSplitControl.SplitterMoved += (sender, e) => grid.Refresh();

			BackColor = DesignModeFinder.IsDesigning
				? SystemColors.Control
				: ObjectFactory.Get<ISystemDataRegistry>().ColorTheme.MainFormBackgroundColor;

			candidateDetailsControl.Saved += (o, e) => Refresh();

			CandidateGridBinding.PositionChanged += (o, e) => lastPosition = CandidateGridBinding.Position;
			AddCandidateChangedHandler();
		}

		void AddToolbarButtons()
		{
			var newButton = CreateToolbarButton((NoResString)"New Candidate", Res.GetData("e9b3ae18-a039-4c90-b0b7-84dc59a95c38", "New Candidate"), IconTypes.NewButtonActive, IconTypes.NewButtonRest);
			newButton.Click += (o, e) => ZControllerFactory.Create(ControllerIDs.HRJobApplication).ShowNewForm();

			moduleActionsToolbar.Items.Add(newButton);
		}

		ZToolStripButton CreateToolbarButton(string key, ResourceStringData caption, IconTypes active, IconTypes rest)
		{
			var button = new ZToolStripButton();
			button.Name = key;
			button.CaptionResourceString = caption;
			button.Image = Icons.GetImage(rest);

			button.MouseEnter += (o, e) => button.Image = Icons.GetImage(active);
			button.MouseLeave += (o, e) => button.Image = Icons.GetImage(rest);

			return button;
		}

		EventHandler CandidateChangedHandler => candidateChangedHandler ?? (candidateChangedHandler = new EventHandler(HandleCandidateChanged));

		EventHandler candidateChangedHandler;

		public void RemoveHandlers()
		{
			RemoveCandidateChangedHandler();
		}

		void RemoveCandidateChangedHandler()
		{
			CandidateGridBinding.CurrentChanged -= CandidateChangedHandler;
		}

		public void AddCandidateChangedHandler()
		{
			CandidateGridBinding.CurrentChanged += CandidateChangedHandler;
		}

		void HandleCandidateChanged(object sender, EventArgs e)
		{
			var selected = grid.ListManager.GetCurrent() as Candidate;
			if (selected != null && selected.Equals(candidateDetailsControl.CurrentDataItem))
			{
				return;
			}

			var isApplicationDeleted = candidateDetailsControl.CurrentDataItem?.Application?.IsDeleted;

			if (isApplicationDeleted == true)
			{
				Globals.Message.ShowInformation(
						Res.GetString("CandidateManagementControl|ApplicationDeletedMessage", "This application has been deleted.\r\nAny unsaved candidate changes have been discarded."),
						Res.GetString("CandidateManagementControl|ApplicationDeletedCaption", "Application Deleted"));

				RefreshCandidateDetailsBinding();

				return;
			}

			var isNull = candidateDetailsControl.CurrentDataItem?.IsNull ?? true;
			var isWarning = !isNull && candidateDetailsControl.CurrentDataItem.HasChanges;

			var isCancel = false;

			if (isWarning)
			{
				var cancelOptions = Globals.Message.Show(
						Res.GetString("CandidateManagementControl|UnsavedChangesMessage", "You have unsaved changes.\r\nWould you like to save these changes?"),
						Res.GetString("CandidateManagementControl|UnsavedChangesCaption", "Warning"),
						MessageBoxButtons.YesNoCancel,
						MessageBoxIcon.Warning);

				var candidateDetailsControl = (CandidateDetailsControl)Controls.Find("CandidateDetailsControl", true).First();

				switch (cancelOptions)
				{
					case DialogResult.Yes:
						isCancel = candidateDetailsControl.ValidateAndSave() == ContinueWithSave.No;
						break;

					case DialogResult.No:
						candidateDetailsControl.DisableSaveButton();
						break;

					case DialogResult.Cancel:
						isCancel = true;
						break;
				}
			}

			if (isCancel)
			{
				var p = lastPosition;
				_ = BeginInvoke(new Action(() => CandidateGridBinding.Position = p));
			}
			else
			{
				RefreshCandidateDetailsBinding();
			}
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			RefreshCandidateDetailsBinding();
		}

		void RefreshCandidateDetailsBinding()
		{
			candidateDetailsControl.SetDataBinding(GetCandidateForDetails(), string.Empty);
		}

		internal Candidate GetCandidateForDetails()
		{
			var selectedCandidate = grid.ListManager.GetCurrent() as Candidate;
			var candidateInDB = selectedCandidate != null && selectedCandidate.Applicant.IsInDatabase && selectedCandidate.Application.IsInDatabase;

			if (!candidateInDB)
			{
				return module.DummyForBinding;
			}

			var factory = new BusinessObjectFactory { NameForDebugging = "CandidateDetailsControl" };
			return new Candidate(factory, selectedCandidate.Application.PK);
		}

		void AddStripControl(BusinessObjectFactory factory)
		{
			stripControl = new ZFilterStripBaseControl(grid, new CandidateFilterStripBusinessObject(factory))
			{
				AllowDrop = true,
				AutoSize = true,
				BackColor = Color.Transparent,
				CaptionRenderingEnabled = true,
				Dock = DockStyle.Top,
				Location = NewScaledPoint(0, 0, true),
				Name = "stripControl",
				Size = NewScaledSize(1204, 204, true),
				ShouldRunSearchOnStripsInitialized = EnvProxy.Instance.Registry.RunSearchOnEnteringAModule,
				ShowSearchResultsMessageBox = true,
				TabIndex = 0,
			};

			FixRecordCountLabelToRightEdge(stripControl);

			stripControl.PerformSearch += StripControl_PerformSearch;
			grid.Parent.Controls.Add(stripControl);
		}

		static void FixRecordCountLabelToRightEdge(ZFilterStripBaseControl stripControl)
		{
			var label = stripControl.Controls.Find("ToolStripRecordsFoundLabel", false).Single(); // Gross
			label.MaximumSize = NewScaledSize(65, 0);
			label.Dock = DockStyle.Right;
		}

		void Grid_MouseDoubleClick()
		{
			if (grid.SelectedElements.Length == 1)
			{
				var candidate = grid.SelectedElements[0] as Candidate;
				var applicant = candidate.Applicant;

				if (applicant == null)
				{
					Globals.Message.ShowError(Res.GetString("65715614-ab41-4acb-8ad0-ef697fb696f4", "No applicant found for this Candidate."));
				}
				else
				{
					_ = ZControllerFactory.Create(ControllerIDs.HRJobApplicant).ShowEditForm(applicant);
				}
			}
		}

		void StripControl_PerformSearch(object sender, EventArgs e)
			=> StripControl_PerformSearch();

		internal void StripControl_PerformSearch()
		{
			CandidateFilterBizo.InvalidateCacheQuery();
			var factory = SearchManager.GetNewFactory();
			var collection = module.Collection;
			var query = CandidateFilterBizo.Filter;
			var result = PerformSearchResult.Success(factory, query, CandidateBusinessObjectCollection.Load(factory, query).ToArray(), permitActiveCollectionUpdates: false);
			SearchManager.PushItemsIntoCollection(collection, result, null);
		}

		internal CandidateFilterStripBusinessObject CandidateFilterBizo
			=> (CandidateFilterStripBusinessObject)stripControl.FilterBusinessObject;

		internal ZFilterStripBaseControl stripControl;

		FilteredGridLoader SearchManager => searchManager ?? (searchManager = CreateSearchManager());
		internal FilteredGridLoader searchManager;

		FilteredGridLoader CreateSearchManager()
			=> new FilteredGridLoader(
				CandidateFilterBizo,
				new ResultCountMessage(stripControl, MaxRowsToLoad, MaxRowsToLoad),
				false,
				ModuleIDs.RecruitmentCandidateManagement,
				() => new BusinessObjectFactory(),
				typeof(Candidate));

		int MaxRowsToLoad
			=> SystemDataRegistry.Instance.MaxNumberOfRecordsToShowInDisplayGrids.Value;
	}
}
