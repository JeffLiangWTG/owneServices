using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.TransportConsignment.Business;
using Enterprise.TransportConsignment.Integration;
using Enterprise.TransportConsignment.Module;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Res = Enterprise.TransportConsignment.Module.Res;

namespace Enterprise.TransportConsignment.GUI // this is not for the modules, it is an embedded filter control on a form
{
	public partial class DtbRoutePlannerFilterControl : ZFilterStripCommonControl, IDtbRoutePlannerFilterControl
	{
		/// <summary>
		/// This determines the max confirmations, *not* the max address points (consignments are >= address points).
		/// </summary>
		const int MAXIMUM_CONFIRMATIONS = 1500;

		public DtbRoutePlannerFilterControl()
			: base(new DtbRoutePlannerFilterBusinessObject())
		{
			InitializeComponent();
			PerformSearch += DtbRoutePlannerFilterControl_PerformSearch;

			DefaultGrid.AllowOverlap(CoveringLabel);
			DefaultGrid.AllowOverlap(ToolStripPermissionsLabel);
		}
		public new DtbRoutePlannerFilterBusinessObject FilterBusinessObject
		{
			get
			{
				var filter = (DtbRoutePlannerFilterBusinessObject)base.FilterBusinessObject;
				filter.QueryObjectType = typeof(DtbConsignmentConfirmation);
				return filter;
			}
		}

		#region CanSaveColumnLayouts

		protected override bool CanSaveColumnLayouts
		{
			get { return false; }
		}

		#endregion

		#region ChildFilterControl

		public void AddChildFilterControl(IDtbFilterControl filterControl)
		{
			var childFilterControl = (DtbChildFilterControl)filterControl;
			ChildFilterControls.Add(childFilterControl);
			FilterBusinessObject.AddChildFilterBusinessObject((DtbChildFilterBusinessObject)childFilterControl.FilterBusinessObject);
		}

		List<DtbChildFilterControl> ChildFilterControls
		{
			get { return childFilterControls ?? (childFilterControls = new List<DtbChildFilterControl>()); }
		}

		List<DtbChildFilterControl> childFilterControls;

		#endregion

		#region Perform Search

		protected override ZBool ShouldPerformSearch()
		{
			// user may try to assign a consignment, if the assign fails, has changes is true 
			// and the user needs to click the refresh button to reload their data
			return Planner != null && base.ShouldPerformSearch();
		}

		void DtbRoutePlannerFilterControl_PerformSearch(object sender, EventArgs e)
		{
			PerformSearchCore();
		}

		void PerformSearchCore()
		{
			OnPerformingSearch();

			Grid.SuspendLayout(); // do not refresh on each add to the collection
			try
			{
				HasSearchBeenRun = true;
				Planners.SwapFactoryRemoveAllAndAddNew();

				var loadSuccessful = AddressPoints.LoadAndConsolidateFromConfirmations(FilterBusinessObject.Filter, MAXIMUM_CONFIRMATIONS, ViewMode);
				if (!loadSuccessful)
				{
					Globals.Message.ShowError(
						Res.GetString("DtbRoutePlannerFilterControl|MoreThanMaxConsignmentsFoundMessage", "More than {0} Consignments were found. Please refine your search criteria.", MAXIMUM_CONFIRMATIONS),
						Res.GetString("DtbRoutePlannerFilterControl|MoreThanMaxConsignmentsFoundCaption", "Search Results"));
				}
				UpdateResultCountMessage(loadSuccessful);
			}
			finally
			{
				Grid.ResumeLayout();
			}

			PerformSearchOnChildren();
			OnPerformedSearch();
		}

		public void UpdateResultCountMessage(bool loadSuccessful)
		{
			if (loadSuccessful)
			{
				ResultCountMessage.UpdateResultCountMessage(AddressPoints.Count);
			}
			else
			{
				ToolStripRecordsFoundLabel.Text = ""; // Reset number of records shown in the screen
				UpdateToolStripRecordsFoundLabelTop();
			}
		}

		public void PerformSearchOnChildren()
		{
			ChildFilterControls.ForEach(fc => fc.FirePerformSearch());
		}

		DtbAddressPointCollection AddressPoints
		{
			get { return (DtbAddressPointCollection)GridCollection; }
		}

		public bool HasSearchBeenRun
		{
			get;
			private set;
		}

		void OnPerformingSearch()
		{
			if (PerformingSearch != null)
			{
				PerformingSearch(this, EventArgs.Empty);
			}
		}

		void OnPerformedSearch()
		{
			if (PerformedSearch != null)
			{
				PerformedSearch(this, EventArgs.Empty);
			}
		}

		public event EventHandler PerformingSearch;
		public event EventHandler PerformedSearch;

		ResultCountMessage ResultCountMessage
		{
			get { return resultCountMessage ?? (resultCountMessage = new ResultCountMessage(this, MaximumAllowableQueriesPerSqlStatement, int.MaxValue)); } // To make number of record search infinite.
		}
		ResultCountMessage resultCountMessage;

		#endregion

		#region ViewMode

		public void SetViewModeWithoutRunningSearch(DtbRoutePlannerViewMode viewMode)
		{
			FilterBusinessObject.ViewMode = viewMode;
		}

		public DtbRoutePlannerViewMode ViewMode
		{
			get { return FilterBusinessObject.ViewMode; }
			set
			{
				if (FilterBusinessObject.ViewMode != value) // don't run search or change grid columns if the viewmode has not changed
				{
					FilterBusinessObject.ViewMode = value;
					UpdateViewMode();
					FirePerformSearch();
				}
			}
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);
			UpdateViewMode();
		}

		void UpdateViewMode()
		{
			if (Visible)
			{
				switch (ViewMode)
				{
					case DtbRoutePlannerViewMode.Direct:
						SwapGrid(DirectModeGrid);
						break;
					default:
						SwapGrid(DefaultGrid);
						break;
				}
			}
		}

		void SwapGrid(ZDisplayGrid gridToSwapTo)
		{
			using (Grid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				var previousGridLocation = this.grid.Location;
				this.grid.Visible = false;

				if (gridToSwapTo != null)
				{
					gridToSwapTo.Location = previousGridLocation;
					this.grid = gridToSwapTo;
				}

				this.grid.Visible = true;
			}
		}

		#endregion

		#region Dispose

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				PerformSearch -= DtbRoutePlannerFilterControl_PerformSearch;

				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#endregion

		#region Planner

		DtbRoutePlanner Planner
		{
			get { return Planners != null ? Planners.Cast<DtbRoutePlanner>().FirstOrDefault() : null; }
		}

		#endregion

		#region Planners

		DtbRoutePlannerCollection Planners
		{
			get { return (DtbRoutePlannerCollection)DataSource; }
		}

		#endregion
	}
}
