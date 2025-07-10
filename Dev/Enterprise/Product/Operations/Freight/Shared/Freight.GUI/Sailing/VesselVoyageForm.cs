using System;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.GUI
{
	public sealed partial class VesselVoyageForm : ZChildForm
	{
		public VesselVoyageForm(VoyageFinder businessEntity, ZString transportMode)
			: base(businessEntity)
		{
			this.TransportMode = transportMode;
		}

		public ZBool NewSailingCreated { get; private set; }
		public JobSailing RequiredSailing { get; private set; }

		public void PerformAdd()
		{
			addButton.PerformClick();
		}

		public void PerformCancel()
		{
			cancelButton.PerformClick();
		}

		#region Implementation

		public override string FormHeading
		{
			get
			{
				ZString result;

				switch (TransportMode)
				{
					case Constants.TransportModes.Air:
						result = new ZString(Res.GetString("VesselVoyageForm|FormCaption|EnterFlightInformation", "Enter Flight Information"));
						break;

					case Constants.TransportModes.Road:
					case Constants.TransportModes.Rail:
						result = Res.GetString("VesselVoyageForm|FormCaption|EnterJourneyInformation", "Enter Journey Information");
						break;

					case Constants.TransportModes.Sea:
					default:
						result = new ZString(Res.GetString("VesselVoyageForm|FormCaption|EnterVesselVoyageInformation", "Enter Vessel/Voyage Information"));
						break;
				}

				return result;
			}
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		VoyageFinder VoyageFinder
		{
			get { return (VoyageFinder)BusinessEntity; }
		}

		ZString TransportMode
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return transportMode; }
			set
			{
				Panel panel;

				if ((panel = GetPanel(transportMode)) != null)
				{
					panel.Visible = false;
				}

				transportMode = value;

				if ((panel = GetPanel(transportMode)) != null)
				{
					panel.Visible = true;
				}
			}
		}

		ZController GetController()
		{
			switch (TransportMode)
			{
				case Constants.TransportModes.Air:
					return ZControllerFactory.Create(ControllerIDs.JobAirSailing);

				case Constants.TransportModes.Road:
					return ZControllerFactory.Create(ControllerIDs.JobRoadSailing);

				case Constants.TransportModes.Rail:
					return ZControllerFactory.Create(ControllerIDs.JobRailSailing);

				case Constants.TransportModes.Sea:
				default:
					return ZControllerFactory.Create(ControllerIDs.JobSeaSailing);
			}
		}

		Panel GetPanel(string transportMode)
		{
			switch (transportMode)
			{
				case Constants.TransportModes.Sea:
					return sea_panel;

				case Constants.TransportModes.Air:
					return air_panel;

				case Constants.TransportModes.Road:
					return road_panel;

				case Constants.TransportModes.Rail:
					return rail_panel;

				default:
					return null;
			}
		}

		partial void SetLastUsedControllerForTest(ZController controller);

		void AddButton_Click(object sender, EventArgs e)
		{
			var controller = GetController();
			controller.SetFormsModalTo(this);

			if (controller is IJobSailingSchedule jobSailingSchedule)
			{
				jobSailingSchedule.ScheduleCreateFromJob = true;
			}

			SetLastUsedControllerForTest(controller);

			var matchingVoyage = VoyageFinder.GetMatchingVoyage();

			if (matchingVoyage != null)
			{
				var form = (ZForm)controller.ShowEditForm(matchingVoyage);

				if (form != null)
				{
					form.Closed += new EventHandler(VoyageForm_Closed);
					newVoyage = (JobVoyage)form.BusinessEntity;
					VoyageFinder.AddOriginAndDestination(newVoyage);
					newVoyage.ReadOnly = true;
				}
			}
			else
			{
				var form = (ZForm)controller.ShowNewForm();

				if (form != null)
				{
					form.Closed += new EventHandler(VoyageForm_Closed);
					newVoyage = (JobVoyage)form.BusinessEntity;
					VoyageFinder.SetupVoyage(newVoyage);
				}
			}

			RequiredSailing = VoyageFinder.GetSailingForShipmentFromVoyage(newVoyage);
		}

		void VoyageForm_Closed(object sender, EventArgs e)
		{
			if (!newVoyage.HasChanges && newVoyage.IsInDatabase)
			{
				NewSailingCreated = true;
			}
			this.Close();
		}

		void CancelButton_Click(object sender, EventArgs e)
		{
			SetLastUsedControllerForTest(null);
			this.Close();
		}

		ZString transportMode;
		JobVoyage newVoyage;

		#endregion
	}
}

#region Test
#if DEBUG

#region Testing Members

namespace Enterprise.Freight.GUI
{
	using Enterprise.ZArchitecture.Environment;

	partial class VesselVoyageForm
	{
		public ZController LastUsedControllerForTest { get; private set; }

		partial void SetLastUsedControllerForTest(ZController controller)
		{
			if (Globals.IsTest)
			{
				LastUsedControllerForTest = controller;
			}
		}
	}
}

#endregion

#endif
#endregion
