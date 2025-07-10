using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Routing.S8.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using static Enterprise.Freight.Integration.Forwarding;

namespace Enterprise.Freight.Forwarding.Routing.S8.GUI
{
	public partial class RealTimeRoutingForm : ZChildForm
	{
		public RealTimeRoutingForm(RoutingManager routingManager)
			: base(routingManager)
		{
			InitializeComponent();
			CreateFilterControl();
		}

		#region Filter Control

		void CreateFilterControl()
		{
			Filter = new RoutingRequestFilterStripBusinessObject(Manager);
			FilterControl = NewFilterControl();
			FilterPanel.Controls.Add(FilterControl);
			FilterControl.Dock = DockStyle.Fill;
			FilterControl.PerformSearch += delegate
			{
				if (Filter.AreOriginDestinationBothZone())
				{
					Globals.Message.ShowError(Res.GetString("FAACDF39-B179-4C58-978E-0C795C69B0CD", "International Zone code can be used only in one of the location fields at a time, while the other has to contain UNLOCO."));
					return;
				}

				var requests = Filter.BuildRequests();

				string errorMessage;
				using (progressForm = new ProgressFormManager())
				{
					progressFormCancelled = false;
					progressForm.IsProgressBarVisible = false;
					progressForm.Cancelled += new EventHandler(delegate
					{ progressFormCancelled = true; });
					progressForm.IsCancelButtonVisible = true;
					progressForm.InitialDelay = new TimeSpan(0, 0, 2);
					progressForm.Start();

					Manager.Requests = requests;
					errorMessage = Manager.Routings.Load(requests, UpdateProgress);
				}

				if (!string.IsNullOrEmpty(errorMessage))
				{
					Globals.Message.Show(Res.GetString("17e1acbe-a0b6-4283-847e-5b17ec44b4de", "Unable to retrieve Routings - {0}", errorMessage));
				}
			};
		}

		ProgressFormManager progressForm;
		bool progressFormCancelled;

		void UpdateProgress(string status)
		{
			progressForm?.UpdateStatus(status, 0);
			if (progressFormCancelled)
			{
				throw new OperationCanceledException("Operation was cancelled");
			}
		}

		protected virtual RealTimeRoutingFilterControl NewFilterControl()
		{
			return new RealTimeRoutingFilterControl(Manager.Routings, Filter);
		}

		public RealTimeRoutingFilterControl FilterControl { get; set; }
		public RoutingRequestFilterStripBusinessObject Filter { get; set; }

		protected bool AllowMultipleSelection { get; set; }
		public bool FlightScheduleCreateFromJob { get; set; }

		public bool OpenedFromRoutingPlugin
		{
			get { return openedFromRoutingPlugin; }
			set
			{
				openedFromRoutingPlugin = value;
				ImportAndCreateMAWBsButton.Visible = !value;
			}
		}

		bool openedFromRoutingPlugin;

		bool ImportWithMultipleSelection
		{
			get { return AllowMultipleSelection && Filter.IncludeWeeklyTimetable; }
		}

		bool ImportWithSingleSelection
		{
			get { return !AllowMultipleSelection && Filter.IncludeWeeklyTimetable; }
		}

		#endregion

		#region Implementation

		public RoutingManager Manager => (RoutingManager)BusinessEntity;

		public override IBusiness BusinessEntity => selectedRoutingManager ?? base.BusinessEntity;

		RoutingManager selectedRoutingManager;

		public override string FormVerb
		{
			get { return string.Empty; }
		}

		public override ODisplayMode DisplayMode
		{
			get { return ODisplayMode.Browse; }
		}

		#endregion

		#region Closing / Chosen Routing

		protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
		{
			if (DialogResult == DialogResult.OK)
			{
				ChosenRouting = FilterControl.FilteredGrid.SelectedElements.Length > 0 ? (RoutingResponseHeader)FilterControl.FilteredGrid.SelectedElements[0] : null;
			}
			base.OnClosing(e);
		}

		void CloseButton_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.OK;
			Close();
		}

		public RoutingResponseHeader ChosenRouting { get; private set; }

		readonly List<RoutingResponseHeader> ImportedRoutings = new List<RoutingResponseHeader>();

		public bool IsImporting => ChosenRouting != null && ImportedRoutings.Contains(ChosenRouting);

		#endregion

		#region Import Button

		void ImportButton_Click(object sender, EventArgs e)
		{
			AllowMultipleSelection = !OpenedFromRoutingPlugin;
			ImportButtonCommon(false);
		}

		void ImportAndCreateMAWBsButton_Click(object sender, EventArgs e)
		{
			AllowMultipleSelection = true;
			ImportButtonCommon(true);
		}

		void ImportButtonCommon(bool importAndCreateMawb)
		{
			var factory = GetNewFactory();

			var selectedHeaders = GetSelectedHeaders(factory);
			if (selectedHeaders == null)
			{
				return;
			}

			var multiDaysSelection = RoutingMultiDaysSelection.Create(RequestedDate, selectedHeaders, Filter.IncludeWeeklyTimetable, importAndCreateMawb, factory);

			if (importAndCreateMawb || ImportWithMultipleSelection)
			{
				DepartureDates = SelectDepartureDates(multiDaysSelection);

				if (DepartureDates == null)
				{
					return;
				}
			}
			else if (ImportWithSingleSelection)
			{
				SelectedDepartureDate = SelectDepartureDate(selectedHeaders[0]);

				if (SelectedDepartureDate == ZDateTime.Empty)
				{
					return;
				}
			}

			IEnumerable<ZDateTime> departureDates;

			if (importAndCreateMawb)
			{
				departureDates = (DepartureDates.Any()) ? DepartureDates : new List<ZDateTime>() { RequestedDate };
			}
			else
			{
				departureDates = (Filter.IncludeWeeklyTimetable && AllowMultipleSelection) ? DepartureDates : new List<ZDateTime>() { DepartureDate };
			}

			multiDaysSelection.CreateVoyagesSailings(departureDates);

			// Generate consols.
			if (importAndCreateMawb)
			{
				multiDaysSelection.Generate();
			}

			var importDialogResult = ShowSchedulesConsolsForm(multiDaysSelection);
			if (importDialogResult == DialogResult.OK)
			{
				ImportedRoutings.AddRange(multiDaysSelection.RoutingResponseHeaders.Cast<RoutingResponseHeader>());
			}

			selectedRoutingManager = null;
		}

		protected virtual BusinessObjectFactory GetNewFactory()
		{
			return new BusinessObjectFactory();
		}

		protected virtual DialogResult ShowSchedulesConsolsForm(RoutingMultiDaysSelection multiDaysSelection)
		{
			return ZFormModaliser.ShowDialogAndDispose((ZChildForm)ObjectFactory.Get<ISchedulesConsolsForm>("ISchedulesConsolsForm", multiDaysSelection));
		}

		RoutingResponseHeaderCollection GetSelectedHeaders(BusinessObjectFactory factory)
		{
			if (!Env.Security.FlightScheduleNew.IsAllowed)
			{
				Globals.Message.ShowError(Env.Security.FlightScheduleNew.ErrorMessageForNotAllowed);
				return null;
			}

			if (FlightScheduleCreateFromJob && !Env.Security.FlightScheduleCreateFromJob.IsAllowed)
			{
				Globals.Message.ShowError(Env.Security.FlightScheduleCreateFromJob.ErrorMessageForNotAllowed);
				return null;
			}

			var selectedHeadersCollection = new RoutingResponseHeaderCollection(factory);
			var selectedHeadersList = FilterControl.FilteredGrid.SelectedElements.Cast<RoutingResponseHeader>();

			foreach (RoutingResponseHeader header in selectedHeadersList)
			{
				selectedHeadersCollection.Add(header);
			}

			if (selectedHeadersCollection.Count == 0)
			{
				Globals.Message.ShowError(ResString.GetMultilingualString("3f66f448-768d-441b-bfdc-a6dc9230b92c", "Please select an item from the grid."));
				return null;
			}

			if (!AllowMultipleSelection && selectedHeadersCollection.Count > 1)
			{
				Globals.Message.Show(ResString.GetMultilingualString("7989a55e-4eb6-452e-9b65-dd6d9cc37e17", "Please select only one route."));
				return null;
			}

			if (selectedHeadersCollection.Cast<RoutingResponseHeader>().Any(header => header.AnyLineHasMultipleAircraftTypes))
			{
				Globals.Message.ShowWarning(ResString.GetMultilingualString("ac681e51-9e8c-4c30-a5ef-48fb3bec845e", "Note there are multiple aircraft types listed against this flight number, please verify aircraft type with your airline."));
			}

			return selectedHeadersCollection;
		}

		IEnumerable<ZDateTime> DepartureDates { get; set; }

		public ZDateTime DepartureDate => (Filter.IncludeWeeklyTimetable ? SelectedDepartureDate : RequestedDate);

		ZDateTime SelectedDepartureDate = ZDateTime.Empty;

		ZDateTime RequestedDate
		{
			get
			{
				var baseRoutingManager = (RoutingManager)base.BusinessEntity;
				return baseRoutingManager.DepartureDate;
			}
		}

		protected virtual ZDateTime SelectDepartureDate(RoutingResponseHeader selectedHeader)
		{
			var result = ZDateTime.Empty;

			using (var form = new SingleDaySelectionForm(RequestedDate, selectedHeader))
			{
				if (form.ShowDialog() == DialogResult.Yes)
				{
					result = form.SelectedDepartureDate;
				}
			}

			return result;
		}

		protected virtual IBulkConsolCreationForm GetBulkConsolCreationForm(RoutingMultiDaysSelection multiDaysSelection)
		{
			return ObjectFactory.Get<IBulkConsolCreationForm>("IBulkConsolCreationForm", multiDaysSelection);
		}

		protected virtual IEnumerable<ZDateTime> ShowDialogAndGetResult(RoutingMultiDaysSelection multiDaysSelection)
		{
			IEnumerable<ZDateTime> result = null;

			using (var form = (ZChildForm)ObjectFactory.Get<IBulkConsolCreationForm>("IBulkConsolCreationForm", multiDaysSelection))
			{
				if (form.ShowDialog() == DialogResult.Yes)
				{
					result = multiDaysSelection.DepartureDates;
				}
			}

			return result;
		}

		protected virtual IEnumerable<ZDateTime> SelectDepartureDates(RoutingMultiDaysSelection multiDaysSelection)
		{
			return ShowDialogAndGetResult(multiDaysSelection);
		}

		#endregion
	}
}

