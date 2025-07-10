using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.GUI;
using Enterprise.Freight.GUI.OnlineSailingSchedules;
using Enterprise.Freight.Integration;
using Enterprise.Freight.OnlineSailingSchedules;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.QuotedBookings.GUI
{
	[TypeDescriptionProvider(typeof(ZControlTypeDescriptionProvider))]
	public partial class ScheduleChooserControl : ZUserControl, IScheduleChooserControl, IAllowTabBackwardBetweenSomeOfMyChildren
	{
		public ScheduleChooserControl()
		{
			InitializeComponent();
			DataBindings.CollectionChanging += new CollectionChangeEventHandler(DataBindings_CollectionChanging);
			SetDataSourceBinding("TransportContainerMode", QuotedBooking.Schema.Mode);
			SetDataSourceBinding("IsDirectBooking", "Booking." + ForwardingShipment.Schema.JS_IsDirectBooking);

#if DEBUG
			TypeDescriptor.AddAttributes(ButtonSelectFromConsortium, new SuppressFormsLocalizedTestAttribute());
#endif
		}

		void DataBindings_CollectionChanging(object sender, CollectionChangeEventArgs e)
		{
			Binding binding = e.Element as Binding;
			if (binding != null &&
				(binding.PropertyName == "TransportContainerMode" || binding.PropertyName == "IsDirectBooking"))
			{
				binding.DataSourceUpdateMode = DataSourceUpdateMode.Never;
			}
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			SetupSailingCaptions();
		}

		#region Binding

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			if (dataSource != null)
			{
				if (QuotedBooking?.Booking != null)
				{
					if (QuotedBooking.Booking.JS_IsCancelled || (QuotedBooking.Booking.JS_IsBooking && QuotedBooking.Booking.JS_IsForwardRegistered))
					{
						EnableSailingButtons = false;
					}
				}

				this.SetControlVisibility(QuotedBooking);
			}
		}

		#region TransportContainerMode

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
		public ZString TransportContainerMode
		{
			get { return transportContainerMode; }
			set
			{
				if (transportContainerMode != value)
				{
					transportContainerMode = value;
					this.SetControlVisibility(QuotedBooking);
					SetupSailingCaptions();
				}
			}
		}
		ZString transportContainerMode;

		#endregion

		#region IsDirectBooking

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
		public ZString IsDirectBooking
		{
			get { return isDirectBooking; }
			set
			{
				if (isDirectBooking != value)
				{
					isDirectBooking = value;
					this.SetControlVisibility(QuotedBooking);
					SetupSailingCaptions();
				}
			}
		}
		ZString isDirectBooking;

		#endregion

		#endregion

		#region Schedule

		#region Schedule Button Clicks

		void AddNewSailingButton_Click_1(object sender, EventArgs e)
		{
			ISailingParentFindBox sailingParent = QuotedBooking.ScheduleChooser;

			if (!sailingParent.Origin.IsEmpty && !sailingParent.Destination.IsEmpty && !QuotedBooking.Booking.JS_A_BKD.IsEmpty)
			{
				VoyageFinder voyageFinder = QuotedBooking.ScheduleChooser.GetVoyageFinder();
				VesselVoyageForm voyageForm = new VesselVoyageForm(voyageFinder, sailingParent.TransportMode);
				ZFormModaliser.Show(voyageForm, ParentQuotedBookingForm);
				voyageForm.Closed += new EventHandler(VoyageForm_Closed);
			}
			else
			{
				Globals.Message.ShowError(Res.GetString("d563aef4-c6e5-42ff-96cb-dd58d299a68f", "Please enter Load and Discharge ports."), Res.GetString("30fc3e76-2cc3-45fa-b9c9-622432ad521d", "Add {0} Error", QuotedBooking.ScheduleChooser.SailingText));
			}
		}

		void ViewSailingButton_Click_1(object sender, EventArgs e)
		{
			SailingIFindBox helper = new SailingIFindBox(QuotedBooking.ScheduleChooser, ParentQuotedBookingForm);
			helper.ShowModuleFromISailingParent();
		}

		void ClearSailingButton_Click_1(object sender, EventArgs e)
		{
			QuotedBooking.Booking.JS_JX = ZGuid.Empty;
			QuotedBooking.RefreshBinding();
		}

		void ViewLoadListButton_Click(object sender, EventArgs e)
		{
			ISailingParentFindBox sailingParent = QuotedBooking.ScheduleChooser;
			BusinessObjectFactory factory = new BusinessObjectFactory();
			JobSailing loadList = factory.Load<JobSailing>(sailingParent.SailingPK);

			if (loadList != null)
			{
				loadList.ReadOnly = true;
				ZController controller = ZControllerFactory.Create(ControllerIDs.LoadList);
				controller.SetFormsModalTo(ParentQuotedBookingForm);
				controller.ShowEditForm(loadList);
			}
			else
			{
				if (sailingParent.TransportMode == Enterprise.Core.Constants.TransportModes.Sea)
				{
					Globals.Message.ShowError(Res.GetString("920ead66-9499-4697-8bdb-a992ee939ed4", "Please select a Sailing."), Res.GetString("e6889f88-e366-47a6-961a-33762fbb8645", "Load List Error"));
				}
				else if (sailingParent.TransportMode == Enterprise.Core.Constants.TransportModes.Air)
				{
					Globals.Message.ShowError(Res.GetString("5ee029da-1bc6-469f-a18d-ee70d5959fe3", "Please select a Flight."), Res.GetString("0bc9ffe0-33e1-420e-a29a-480bc0d75845", "Load List Error"));
				}
				else
				{
					Globals.Message.ShowError(Res.GetString("cce542a2-8475-42ff-a560-ec556381f91f", "Please select a Sector."), Res.GetString("e07dd451-a15e-40bd-bc55-5e6b60b75dcd", "Load List Error"));
				}
			}
		}

		void VoyageForm_Closed(object sender, EventArgs e)
		{
			if (ParentForm.IsDisposed || ParentForm == null)
			{
				return;//drop action since the ParentForm already disposed
			}
			ISailingParentFindBox sailingParent = QuotedBooking.ScheduleChooser;

			if (!QuotedBooking.Booking.JS_A_BKD.IsEmpty && !sailingParent.Destination.IsEmpty && !sailingParent.Origin.IsEmpty)
			{
				var vesselVoyageForm = (VesselVoyageForm)sender;
				if (vesselVoyageForm != null && vesselVoyageForm.NewSailingCreated && vesselVoyageForm.RequiredSailing != null)
				{
					sailingParent.SailingPK = vesselVoyageForm.RequiredSailing.PK;
				}
			}

			QuotedBooking.RefreshBindingIncludingChildren();
		}

		void JS_IsNeutral_CheckedChanged(object sender, EventArgs e)
		{
			MAWBNumberTextBox.Visible = !this.JS_IsNeutral.Checked;
			MAWBPendingAllocationTextBox.Visible = this.JS_IsNeutral.Checked;
		}

		void ImportGlobalSchedulesButton_Click(object sender, EventArgs e)
		{
			ShowGlobalSchedulesFormAndUpdateBookingIfNecessary();
		}

		void ShowGlobalSchedulesFormAndUpdateBookingIfNecessary()
		{
			var sailingParent = (ISailingParentFindBox)QuotedBooking.ScheduleChooser;
			if (sailingParent == null)
			{
				return;
			}

			var controller = ZControllerFactory.Create(ControllerIDs.OnlineSailingSchedules);
			controller.SetFormsModalTo(this.ParentForm);

			var onlineSchedulesForm = controller.ShowNewForm() as OnlineSchedulesForm;
			if (onlineSchedulesForm != null)
			{
				onlineSchedulesForm.AllowMultipleSelection = false;
				onlineSchedulesForm.SailingScheduleCreateFromJob = true;

				var filterDefaults = new OnlineSchedulesFilterStripBusinessObject.FilterDefaults()
				{
					Origin = sailingParent.Origin,
					Destination = sailingParent.Destination,
					IncludeRelatedPorts = ZBool.True
				};

				onlineSchedulesForm.SetFilterDefaults(filterDefaults);

				onlineSchedulesForm.SailingSchedulesImported += delegate(object sender, OnlineSailingSchedulesImportedEventArgs eventArgs)
				{
					var importedRoute = eventArgs.ImportedRoutes.FirstOrDefault();

					if (importedRoute == null || QuotedBooking.Booking == null)
					{
						return;
					}

					UpdateCarrierContractAndAllocationDetails(importedRoute);
				};
			}
		}

		internal void UpdateCarrierContractAndAllocationDetails(Route importedRoute)
		{
			var firstLeg = importedRoute.Legs.Cast<Leg>().FirstOrDefault();
			if (firstLeg != null)
			{
				var matchingSailing = firstLeg.FindMatchingJobSailing();
				if (matchingSailing != null)
				{
					QuotedBooking.Booking.JS_JX = matchingSailing.PK;

					var routeSelector = QuotedBooking.Factory.GetValue<IMultiAllocationRouteSelectorProvider>();
					var overrideDialog = QuotedBooking.Factory.GetValue<IOverrideAllocationRouteDialogProvider>();
					if (routeSelector == null || overrideDialog == null)
					{
						QuotedBooking.RefreshBinding();
						return;
					}

					var allocationRouteQuery = new ZQuery(RatingContractAllocationLineSchema.RCA_JX_SailingSchedule, matchingSailing.PK);
					var allocationRoutes = QuotedBooking.Factory.Load<IRatingContractAllocationLine>(allocationRouteQuery);

					if (allocationRoutes.Length == 0)
					{
						QuotedBooking.RefreshBinding();
						return;
					}

					IAllocationRouteAssignable routeAssignable = QuotedBooking;

					if (allocationRoutes.Length == 1)
					{
						var route = allocationRoutes[0];

						var currentAllocationRoute = QuotedBooking.Factory.Load<IRatingContractAllocationLine>(QuotedBooking.AllocationLinePK);
						var quotedBookingAllocationLineID = currentAllocationRoute?.RCA_AllocationLineID ?? ZString.Empty;

						if (!routeAssignable.HasCarrierOrRouteDifferentToOverride(route)
							|| overrideDialog.PromptUserForConfirmingOverride(route, QuotedBooking))
						{
							routeAssignable.UpdateCarrierContractAndAllocationDetails(route);
						}
						QuotedBooking.RefreshBinding();
						return;
					}

					var isAssignedRoute = allocationRoutes.Any(route => routeAssignable.IsAssignedAllocationRoute(route));
					
					if (!isAssignedRoute)
					{
						routeSelector.PromptUserForSelectingAllocationRoute(routeAssignable, allocationRoutes);
					}
					
					QuotedBooking.RefreshBinding();
				}
			}
		}

		#endregion

		#region ButtonSelectFromConsortium

		class OrgMenuItem : MenuItem
		{
			readonly OrgHeader fMember;

			public OrgMenuItem(OrgHeader member)
			{
				fMember = member;

				Text = string.Format(CultureInfo.InvariantCulture, "{0} ({1})", member.OH_FullNameTruncated, member.OH_Code);
			}

			public OrgHeader Member
			{
				get { return fMember; }
			}
		}

		void ButtonSelectFromConsortium_Click(object sender, EventArgs e)
		{
			if (QuotedBooking.Booking.Sailing != null && QuotedBooking.Booking.Sailing.Vessel != null && QuotedBooking.Booking.Sailing.Vessel.CarrierConsortium != null)
			{
				ContextMenu context = new ContextMenu();

				foreach (OrgHeader member in QuotedBooking.Booking.Sailing.Vessel.CarrierConsortium.OrgHeaders)
				{
					MenuItem menu = new OrgMenuItem(member);
					menu.Click += new EventHandler(Menu_Click);
					context.MenuItems.Add(menu);
				}

				context.Show(ButtonSelectFromConsortium, CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, ControlDpiScalingHelper.UnscaleFromCurrentDpiY(ButtonSelectFromConsortium.Height)));
			}
			else
			{
				Globals.Message.Show(Res.GetString("eadfcea4-689e-4876-8d73-c644e9afdf3a", "This vessel does not belong to a consortium."),
					Res.GetString("c59d4572-94d5-46b1-a4bc-e77e6eda1106", "Select Carrier From Consortium."), MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
		}

		void Menu_Click(object sender, EventArgs e)
		{
			OrgMenuItem menu = sender as OrgMenuItem;

			if (menu != null)
			{
				QuotedBooking.Booking.JS_OA_BookedShippingLineAddress = menu.Member.MainAddress.PK;
			}
		}

		#endregion

		#region Setup Sailing Panel

		void SetupSailingCaptions()
		{
			if (QuotedBooking == null || QuotedBooking.Booking == null)
			{
				return;
			}

			string journeySummary = Res.GetString("ScheduleChooserControl|SailingSummarygroupBox.JourneySummary", "Journey Summary");
			string viewSectors = Res.GetString("ScheduleChooserControl|ViewSailingButton.ViewSectors", "View Sectors");
			string addNewSector = Res.GetString("ScheduleChooserControl|AddNewSailingButton.AddNewSector", "Add New Sector");
			string clearSector = Res.GetString("ScheduleChooserControl|ClearSailingButton.ClearSector", "Clear Sector");

			VoyageNumberBoundTextBox.GetExtension<LabelCaptionRenderer>().Caption = QuotedBooking.ScheduleChooser.GetVoyageNoLabelDependingOnTransportMode();
			JS_Calc_DepotCutOffBoundReadOnlyDateEdit.GetExtension<LabelCaptionRenderer>().Caption = Res.GetString("ScheduleChooserControl|CutOffLabel.CFSCutOff", "CFS Cut Off");

			if (QuotedBooking.Booking.IsSea)
			{
				SailingSummarygroupBox.Text = Res.GetString("ScheduleChooserControl|SailingSummarygroupBox.SailingSummary", "Sailing Summary");
				((IVariableLengthCaptionRenderer)ViewSailingButton).Captions = new string[] { Res.GetString("ScheduleChooserControl|ViewSailingButton.ViewSailings", "View Sailings") };
				((IVariableLengthCaptionRenderer)AddNewSailingButton).Captions = new string[] { Res.GetString("ScheduleChooserControl|AddNewSailingButton.AddNewSailing", "Add New Sailing") };
				((IVariableLengthCaptionRenderer)ClearSailingButton).Captions = new string[] { Res.GetString("ScheduleChooserControl|ClearSailingButton.ClearSailing", "Clear Sailing") };

				SetupControlsForShortDateFormat();
			}
			else if (QuotedBooking.Booking.IsAir)
			{
				SailingSummarygroupBox.Text = Res.GetString("ScheduleChooserControl|SailingSummarygroupBox.FlightSummary", "Flight Summary");
				((IVariableLengthCaptionRenderer)ViewSailingButton).Captions = new string[] { Res.GetString("ScheduleChooserControl|ViewSailingButton.ViewFlights", "View Flights") };
				((IVariableLengthCaptionRenderer)AddNewSailingButton).Captions = new string[] { Res.GetString("ScheduleChooserControl|AddNewSailingButton.AddNewFlight", "Add New Flight") };
				((IVariableLengthCaptionRenderer)ClearSailingButton).Captions = new string[] { Res.GetString("ScheduleChooserControl|ClearSailingButton.ClearFlight", "Clear Flight") };

				SetupControlsForLongDateFormat();
			}
			else if (QuotedBooking.Booking.IsRail)
			{
				VesselBoundTextBox.GetExtension<LabelCaptionRenderer>().Caption = Res.GetString("ScheduleChooserControl|VesselLabel.Journey", "Journey");
				SailingSummarygroupBox.Text = journeySummary;
				((IVariableLengthCaptionRenderer)ViewSailingButton).Captions = new string[] { viewSectors };
				((IVariableLengthCaptionRenderer)AddNewSailingButton).Captions = new string[] { addNewSector };
				((IVariableLengthCaptionRenderer)ClearSailingButton).Captions = new string[] { clearSector };

				SetupControlsForShortDateFormat();
			}
			else if (QuotedBooking.Booking.IsRoad)
			{
				SailingSummarygroupBox.Text = journeySummary;
				((IVariableLengthCaptionRenderer)ViewSailingButton).Captions = new string[] { viewSectors };
				((IVariableLengthCaptionRenderer)AddNewSailingButton).Captions = new string[] { addNewSector };
				((IVariableLengthCaptionRenderer)ClearSailingButton).Captions = new string[] { clearSector };

				SetupControlsForLongDateFormat();
			}
		}

		void SetupControlsForShortDateFormat()
		{
			ETDZDateEdit.DateTimeFormat = ZDateTimePickerFormat.Short;
			ETAZDateEdit.DateTimeFormat = ZDateTimePickerFormat.Short;

			LoadingPortCodeFindBox.Location = ControlDpiScalingHelper.NewScaledPoint(418, 0, true);
			DischargePortCodeFindBox.Location = ControlDpiScalingHelper.NewScaledPoint(418, 22, true);
		}

		void SetupControlsForLongDateFormat()
		{
			ETDZDateEdit.DateTimeFormat = ZDateTimePickerFormat.Long;
			ETAZDateEdit.DateTimeFormat = ZDateTimePickerFormat.Long;

			LoadingPortCodeFindBox.Location = ControlDpiScalingHelper.NewScaledPoint(452, 0, true);
			DischargePortCodeFindBox.Location = ControlDpiScalingHelper.NewScaledPoint(452, 22, true);
		}

		#endregion

		#region IScheduleChooserControl members

		bool IScheduleChooserControl.ShowDirect
		{
			set { JS_IsDirect.Visible = value; }
		}

		bool IScheduleChooserControl.ShowIsNeutral
		{
			set { JS_IsNeutral.Visible = value; }
		}

		bool IScheduleChooserControl.ShowServiceLevelFields
		{
			set
			{
				JS_AWBServiceLevelLabel.Visible = value;
				JS_AWBServiceLevelDropEdit.Visible = value;
			}
		}

		bool IScheduleChooserControl.ShowMAWBFields
		{
			set
			{
				MAWBLabel.Visible = value;
				MAWBHyphenLabel.Visible = value;
				MAWBNumberTextBox.Visible = QuotedBooking.Booking.JS_IsForwardRegistered || (value && !QuotedBooking.ScheduleChooser.Parent.IsNeutralMaster);
				MAWBPrefixTextBox.Visible = value;
				MAWBPendingAllocationTextBox.Visible = value && (QuotedBooking.ScheduleChooser.Parent.IsNeutralMaster && !QuotedBooking.Booking.JS_IsForwardRegistered);
			}
		}

		bool IScheduleChooserControl.ShowMAWBSeaNumberTextBox
		{
			set { MAWBSeaNumberTextBox.Visible = value; }
		}

		bool IScheduleChooserControl.ShowHBLNumberTextBox
		{
			set { HBLNumberTextBox.Visible = value; }
		}

		bool IScheduleChooserControl.ShowCarrierDetails
		{
			set
			{
				//Carrier
				CarrierLabel.Visible = value;
				BookedShippingLineBoundOrgFindBox.Visible = value;
				ButtonSelectFromConsortium.Visible = value;

				//BookingRef
				BookingRefTextBox.Visible = value;
			}
		}

		bool IScheduleChooserControl.ShowCreditorDetails
		{
			set
			{
				//Creditor
				CreditorLabel.Visible = value;
				CreditorFindBox.Visible = value;
			}
		}

		bool IScheduleChooserControl.ShowScheduleTotalWeightAndVolume
		{
			set
			{
				SailingTotalVolumeCalcDropEdit.Visible = value;
				SailingTotalWeightCalcDropEdit.Visible = value;
				ViewLoadListButton.Visible = value;
			}
		}

		bool IScheduleChooserControl.ShowVesselBoundTextBox
		{
			set { VesselBoundTextBox.Visible = value; }
		}

		bool IScheduleChooserControl.ShowFCLCutOffReaoOnlyDateEdit
		{
			set { JS_Calc_FCLCutOffBoundReadOnlyDateEdit.Visible = value; }
		}

		bool IScheduleChooserControl.ShowImportOnlineSchedulesButton
		{
			set
			{
				ImportGlobalSchedulesButton.Visible = value && FreightDataRegistry.Instance.EnableScheduleFeedService.Value;
			}
		}

		#endregion

		#region EnableSailingButtons

		bool EnableSailingButtons
		{
			set
			{
				ViewSailingButton.Enabled = value;
				AddNewSailingButton.Enabled = value;
				ClearSailingButton.Enabled = value;
				ViewLoadListButton.Enabled = value;
				ImportGlobalSchedulesButton.Enabled = value;
			}
		}

		#endregion

		#endregion

		#region Implementaion

		QuotedBookingForm ParentQuotedBookingForm
		{
			get { return (QuotedBookingForm)ParentForm; }
		}

		QuotedBooking QuotedBooking
		{
			get { return ParentQuotedBookingForm == null ? null : ParentQuotedBookingForm.QuotedBooking; }
		}

		#endregion

		#region IAllowTabBackwardBetweenSomeOfMyChildren Members

		bool IAllowTabBackwardBetweenSomeOfMyChildren.AllowTabBackward(Control control, Control previousControl)
		{
			return control == ImportGlobalSchedulesButton && previousControl == ViewLoadListButton;
		}

		#endregion

		#region Metadata

		public static PropertyDescriptor[] GetPropertyDescriptors()
		{
			return new ControlPropertyDescriptorBuilder<ScheduleChooserControl>()
			.Property("IsDirectBooking", ZString.Empty, false)
			.Property("TransportContainerMode", ZString.Empty, false)
			.Result;
		}

		#endregion
	}
}
