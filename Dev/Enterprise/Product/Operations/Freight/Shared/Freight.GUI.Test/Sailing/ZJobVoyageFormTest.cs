using System;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.OnlineSailingSchedules;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using GSS = Enterprise.Freight.OnlineSailingSchedules.ServiceModel;

namespace Enterprise.Freight.GUI.Testing
{
	[TestedType(typeof(ZJobVoyageForm))]
	sealed class ZJobVoyageFormTest : ZFormBasherTest
	{
		public void TestSetQueryProvider()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();

			AssertEquals("ScheduleUpdateNullQueryProvider", ScheduleUpdateQueryProviderFactory.Get(Factory).GetType().Name);

			using (ZJobVoyageForm form = new ZJobVoyageForm(voyage))
			{
				AssertType(typeof(ScheduleUpdateGuiQueryProvider), ScheduleUpdateQueryProviderFactory.Get(Factory));
			}
		}

		public void TestBusyIndicatorProviderIsRegistered()
		{
			var voyage = Factory.New<JobVoyage>();
			using (var form = new ZJobVoyageForm(voyage))
			{
				AssertEquals(true, Factory.GetValue<IBusyIndicatorProvider>() is BusyIndicatorProvider);
			}
		}

		public void TestFlightScheduleColumnCaptions()
		{
			Voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Air;
			Form.Show();

			SailingsGrid jobSailingBoundGrid = FindSailingsGrid(Form);
			ZGrid jobVoyOriginBoundGrid = FindOriginGrid(Form);

			AssertEquals("CFS Receival Start", jobSailingBoundGrid.Columns[JobSailing.Schema.JX_DepotReceivalCommences].ColumnStyle.HeaderText);
			AssertEquals("CFS Cut Off", jobSailingBoundGrid.Columns[JobSailing.Schema.JX_DepotCutOff].ColumnStyle.HeaderText);
			AssertEquals("ULD Cargo Cut Off", jobVoyOriginBoundGrid.Columns[JobVoyOriginSchema.Constants.JA_CutOff].ColumnStyle.HeaderText);
			AssertEquals("ULD Cargo Rec. Start", jobVoyOriginBoundGrid.Columns[JobVoyOriginSchema.Constants.JA_ReceivalCommences].ColumnStyle.HeaderText);
			AssertEquals("CFS Storage Start", jobSailingBoundGrid.Columns[JobSailing.Schema.JX_DepotStorageDate].ColumnStyle.HeaderText);
			AssertEquals("CFS Available", jobSailingBoundGrid.Columns[JobSailing.Schema.JX_DepotAvailabilityDate].ColumnStyle.HeaderText);
		}

		public void TestSailingScheduleColumnCaptions()
		{
			Voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Sea;
			Form.Show();

			SailingsGrid jobSailingBoundGrid = FindSailingsGrid(Form);
			ZGrid jobVoyOriginBoundGrid = FindOriginGrid(Form);

			AssertEquals("CFS Receival Start", jobSailingBoundGrid.Columns[JobSailing.Schema.JX_DepotReceivalCommences].ColumnStyle.HeaderText);
			AssertEquals("CFS Cut Off", jobSailingBoundGrid.Columns[JobSailing.Schema.JX_DepotCutOff].ColumnStyle.HeaderText);
			AssertEquals("CTO Cut Off", jobVoyOriginBoundGrid.Columns[JobVoyOriginSchema.Constants.JA_CutOff].ColumnStyle.HeaderText);
			AssertEquals("CTO Receival Start", jobVoyOriginBoundGrid.Columns[JobVoyOriginSchema.Constants.JA_ReceivalCommences].ColumnStyle.HeaderText);
		}

		public void TestArrivalReportingPlugin()
		{
			Assert(Form.PlugIns.Instances.Any(plugin => plugin.Name == "Arrival Reporting"));
		}

		public void TestManifestPlugin()
		{
			Assert(Form.PlugIns.Instances.Any(plugin => plugin.Name == "Customs Manifest"));
		}

		public void TestStowPlanPlugin()
		{
			GlbStaff.CurrentUser.GS_IsDeveloper = true;
			Assert(Form.MessagingTabControl.PlugIns.Instances.Any(plugin => plugin.Name == "Stow Plan"));
		}

		public void TestZACALINFPlugin()
		{
			var voyage = Factory.New<JobVoyage>();
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				using (var tmpForm = new TestJobVoyageForm(voyage))
				{
					Assert(!tmpForm.MessagingTabControl.PlugIns.Instances.Any(plugin => plugin.Name == "ZA CALINF Messaging"));
				}
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.SouthAfrica))
			{
				using (var tmpForm = new TestJobVoyageForm(voyage))
				{
					Assert(tmpForm.MessagingTabControl.PlugIns.Instances.Any(plugin => plugin.Name == "ZA CALINF Messaging"));
				}
			}
		}

		public void TestSea_CreateSlotVoyageFromMainOne()
		{
			Func<ZJobVoyageForm, MenuItem> findMenuItem = (voyageForm) =>
				{
					return voyageForm.Menu.MenuItems.FindByText("Create Slot Sailing Schedule", true);
				};

			var airVoyage = Factory.New<JobVoyage>();
			airVoyage.JV_AirSeaRoad = Constants.TransportModes.Air;

			using (var form = new ZJobVoyageForm(airVoyage))
			{
				form.Show();
				AssertNull(findMenuItem(form));
			}

			var carrier = Factory.New<OrgHeader>();
			carrier.OH_Code = "CARRIER";
			carrier.OH_IsShippingLine = true;
			carrier.OH_IsShippingProvider = true;

			var vessel = Factory.New<RefVessel>();
			vessel.RV_Name = "Visund";

			var seaVoyage = Factory.New<JobVoyage>();
			seaVoyage.JV_AirSeaRoad = Constants.TransportModes.Sea;
			seaVoyage.JV_RV_NKVessel = vessel.RV_FK;
			seaVoyage.JV_VoyageFlight = "123";

			ZDateTime now = ZDateTime.Now.Date;

			var origin1 = seaVoyage.Origins.AddNew();
			origin1.JA_RL_NKPortOfLoading = "AUSYD";
			origin1.JA_E_DEP = now;

			var origin2 = seaVoyage.Origins.AddNew();
			origin2.JA_RL_NKPortOfLoading = "AUMEL";
			origin2.JA_E_DEP = now.AddDays(10);

			var destination1 = seaVoyage.Destinations.AddNew();
			destination1.JB_RL_NKPortOfDischarge = "AUBNE";
			destination1.JB_E_ARV = now.AddDays(5);

			var destination2 = seaVoyage.Destinations.AddNew();
			destination2.JB_RL_NKPortOfDischarge = "NZAKL";
			destination2.JB_E_ARV = now.AddDays(20);

			seaVoyage.GenerateSailings();

			using (var form = new ZJobVoyageForm(seaVoyage))
			{
				form.Show();
				seaVoyage.JV_VoyageType = Constants.VoyageType.SlotVoyage;

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				findMenuItem(form).PerformClick();

				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals("Slot Sailing Schedule can be created from the Main Sailing Schedule only.", UnitTestUserNotification.Instance.LastMessage.Text);

				seaVoyage.JV_VoyageType = Constants.VoyageType.MainVoyage;
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				findMenuItem(form).PerformClick();

				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasWarning);
				AssertEquals("Please save the voyage before proceeding.", UnitTestUserNotification.Instance.LastMessage.Text);

				Factory.Save();

				var alreadyOpenedFormHandles = Application.OpenForms.Cast<Form>().Select(f => f.Handle).ToArray();
				findMenuItem(form).PerformClick();

				var newForm = Application.OpenForms
					.Cast<Form>()
					.Single(f => !alreadyOpenedFormHandles.Contains(f.Handle));

				var slotVoyageForm = newForm as ZJobVoyageForm;
				AssertNotNull("New slot voyage form was opened", slotVoyageForm);

				var slotVoyage = slotVoyageForm.DataSource as JobVoyage;
				AssertEquals("Slot voyage created", Constants.VoyageType.SlotVoyage, slotVoyage.JV_VoyageType);
				AssertEquals("Vessel copied", "Visund", slotVoyage.JV_RV_NKVessel);
				AssertEquals("Voyage number copied", "123", slotVoyage.JV_VoyageFlight);
				AssertEquals("Carrier is empty", true, slotVoyage.JV_OH_Line.IsEmpty);

				AssertEquals("Origins copied", 2, slotVoyage.Origins.Count);

				var slotOrigin1 = slotVoyage.Origins.Cast<VoyageOrigin>().Single(o => o.JA_RL_NKPortOfLoading == "AUSYD");
				AssertEquals(origin1.JA_E_DEP, slotOrigin1.JA_E_DEP);

				var slotOrigin2 = slotVoyage.Origins.Cast<VoyageOrigin>().Single(o => o.JA_RL_NKPortOfLoading == "AUMEL");
				AssertEquals(origin2.JA_E_DEP, slotOrigin2.JA_E_DEP);

				AssertEquals("Destinations copied", 2, slotVoyage.Destinations.Count);

				var slotDestination1 = slotVoyage.Destinations.Cast<VoyageDestination>().Single(d => d.JB_RL_NKPortOfDischarge == "AUBNE");
				AssertEquals(destination1.JB_E_ARV, slotDestination1.JB_E_ARV);

				var slotDestination2 = slotVoyage.Destinations.Cast<VoyageDestination>().Single(d => d.JB_RL_NKPortOfDischarge == "NZAKL");
				AssertEquals(destination2.JB_E_ARV, slotDestination2.JB_E_ARV);

				slotVoyageForm.Dispose();
			}
		}

		public void TestCannotDeletePortHandler()
		{
			var voyage = Factory.New<JobVoyage>();

			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUSYD";

			var destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "USLAX";

			voyage.GenerateSailings();

			var sailing = voyage.Sailings[0];

			var shipment = Factory.New<CommonShipment>();
			shipment.JS_JX = sailing.PK;
			AssertEquals("Precondition", true, sailing.IsReferenced());

			using (var form = new ZJobVoyageForm(voyage))
			{
				form.Show();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

				voyage.Origins.RemoveAndDelete(origin);

				Assert(UnitTestUserNotification.Instance.LastMessage.WasWarning);
				AssertEquals("Cannot Delete Port", UnitTestUserNotification.Instance.LastMessage.Caption);
				AssertEquals(sailing.GetReferencingJobNumbers(), UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

				voyage.Destinations.RemoveAndDelete(destination);

				Assert(UnitTestUserNotification.Instance.LastMessage.WasWarning);
				AssertEquals("Cannot Delete Port", UnitTestUserNotification.Instance.LastMessage.Caption);
				AssertEquals(sailing.GetReferencingJobNumbers(), UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				shipment.JS_JX = ZGuid.Empty;
				AssertEquals("Precondition", false, sailing.IsReferenced());

				voyage.Origins.RemoveAndDelete(origin);
				voyage.Destinations.RemoveAndDelete(destination);
				Assert(UnitTestUserNotification.Instance.LastMessage.WasNone);
			}
		}

		public void TestActionsMenuItemsNotAvailableInViewMode()
		{
			ActionsMenuItemsHelperTest.AssertActionsMenuItemsNotAvailableInViewMode(Form);
		}

		static ZDateTime GssTestOriginalEtd { get; } = new ZDateTime(2020, 01, 01);
		static ZDateTime GssTestOriginalEta { get; } = new ZDateTime(2020, 01, 31);
		static ZDateTime GssTestUpdatedEtd => GssTestOriginalEtd.AddDays(1);
		static ZDateTime GssTestUpdatedEta => GssTestOriginalEta.AddDays(1);

		public void TestGlobalSailingSchedulesFillsInEtaWhenNewDestinationIsAdded()
		{
			TestGlobalSailingSchedulesDateChange((voyage, form) =>
			{
				voyage.Destinations.RemoveAndDeleteAll();

				var newDestination = voyage.Destinations.AddNew();
				newDestination.JB_RL_NKPortOfDischarge = "USLAX";

				AssertEquals("ETA update was expected without any action from the user as newly added destinations get their ETA from Global Sailing Schedules unconditionally", GssTestUpdatedEta, voyage.Destinations[0].JB_E_ARV);
			});
		}

		public void TestGlobalSailingSchedulesFillsInEtdWhenNewOriginIsAdded()
		{
			TestGlobalSailingSchedulesDateChange((voyage, form) =>
			{
				voyage.Origins.RemoveAndDeleteAll();

				var newOrigin = voyage.Origins.AddNew();
				newOrigin.JA_RL_NKPortOfLoading = "AUSYD";

				AssertEquals("ETD update was expected without any action from the user as newly added origins get their ETD from Global Sailing Schedules unconditionally", GssTestUpdatedEtd, voyage.Origins[0].JA_E_DEP);
			});
		}

		void TestGlobalSailingSchedulesDateChange(Action<JobVoyage, ZJobVoyageForm> testAction)
		{
			var voyage = Factory.New<JobVoyage>();

			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUSYD";
			origin.JA_E_DEP = GssTestOriginalEtd;

			var destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "USLAX";
			destination.JB_E_ARV = GssTestOriginalEta;

			voyage.GenerateSailings();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("US")) // Ensures IOnlineSailingSchedulesDataVendor is used, as opposed to OneStop or Dakosy
			{
				var gssRoute = new GSS.Route
				{
					Carrier = new GSS.Carrier { Code = "SCAC" },
					Legs = new[]
					{
						new GSS.Leg
						{
							LoadPort = new GSS.Port { Unloco = "AUSYD" },
							DischargePort = new GSS.Port { Unloco = "USLAX" },
							Etd = GssTestUpdatedEtd.ToDateTime(),
							Eta = GssTestUpdatedEta.ToDateTime(),
							LegType = "SEA",
							Voyage = new GSS.Voyage
							{
								Code = "001E",
								TradeLane = new GSS.TradeLane { Name = "TL1" },
								Vessel = new GSS.Vessel
								{
									ImoNumber = "1234567",
									VesselName = "Test Vessel"
								}
							},
						}
					}
				};

				var route = new Route(Factory);
				route.SetValues(gssRoute);

				var mockGssDataVendor = new TestOnlineSailingScheduleDataVendor(route);
				using (ObjectFactory.Substitute<Integration.SailingDataVendor.IOnlineSailingSchedulesDataVendor>(mockGssDataVendor))
				{
					using (var form = new ZJobVoyageForm(voyage))
					{
						form.Show();
						testAction(voyage, form);
					}
				}
			}
		}

		public void TestInitialiseShippingManagerMenu()
		{
			Voyage.JV_AirSeaRoad = Constants.TransportModes.Sea;

			using (var form = new TestJobVoyageForm(Voyage))
			{
				AssertNotNull(form.Menu.MenuItems.FindByText("Shipping Manager"));
			}

			Voyage.JV_AirSeaRoad = Constants.TransportModes.Air;

			using (var form = new TestJobVoyageForm(Voyage))
			{
				AssertNull(form.Menu.MenuItems.FindByText("Shipping Manager"));
			}
		}

		public void TestDangerousGoodsManifestPlugin()
		{
			Assert(Form.MessagingTabControl.PlugIns.Instances.Any(plugin => plugin.Name == "Dangerous Goods Manifest"));
		}

		#region Test Classes

		class TestJobVoyageForm : ZJobVoyageForm
		{
			public TestJobVoyageForm(JobVoyage voyage)
				: base(voyage)
			{
			}

			public ZTabControl MessagingTabControl
			{
				get
				{
					return base.messagingTabControl;
				}
			}

			public new ContinueWithSave ValidateAndSave()
			{
				return base.ValidateAndSave();
			}
		}

		sealed class TestOnlineSailingScheduleDataVendor : OnlineSailingSchedulesDataVendor
		{
			public Route RouteToServe { get; }

			public TestOnlineSailingScheduleDataVendor(Route routeToServe)
			{
				RouteToServe = routeToServe;
			}

			protected override Route FindBestMatchingRoute(VoyageOrigin origin, bool includeRelatedPorts = false) => RouteToServe;
			protected override Route FindBestMatchingRoute(VoyageDestination destination, bool includeRelatedPorts = false) => RouteToServe;
		}

		#endregion

		#region Implementation

		TestJobVoyageForm Form
		{
			get
			{
				if (fForm == null)
				{
					fForm = new TestJobVoyageForm(Voyage);
				}
				return fForm;
			}
		}
		TestJobVoyageForm fForm;

		JobVoyage Voyage
		{
			get
			{
				if (fVoyage == null)
				{
					var today = ZDateTime.Today;

					fVoyage = Factory.New<JobVoyage>();
					Voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Sea;
					Voyage.JV_VoyageFlight = "234";

					var vessel = RefVessel.LookupVesselByName("ARAFURA", Factory).First();
					Voyage.JV_RV_NKVessel = vessel.RV_FK;

					VoyageOrigin origin = fVoyage.Origins.AddNew();
					origin.JA_JV = Voyage.PK;
					origin.JA_E_DEP = today;
					origin.JA_RL_NKPortOfLoading = "USLAX";
					origin.JA_ReceivalCommences = today.AddDays(-10);
					origin.JA_CutOff = today.AddDays(-1);
					origin.HasChanges = false;

					VoyageDestination destination = fVoyage.Destinations.AddNew();
					destination.JB_JV = Voyage.PK;
					destination.JB_E_ARV = today.AddMonths(1);
					destination.JB_RL_NKPortOfDischarge = "AUSYD";
					destination.HasChanges = false;

					JobSailing sailing = Voyage.Sailings[0];
					sailing.JX_DepotCutOff = today.AddDays(-2);
					sailing.JX_DepotReceivalCommences = today.AddDays(-10);
					sailing.HasChanges = false;

					Voyage.HasChanges = false;
				}
				return fVoyage;
			}
		}
		JobVoyage fVoyage;

		SailingsGrid FindSailingsGrid(ZJobVoyageForm form)
		{
			VoyageDetailsControl control = FindDetailsControl(form);
			return FindControl<SailingsGrid, VoyageDetailsControl>(control, "jobSailingBoundGrid");
		}

		ZGrid FindOriginGrid(ZJobVoyageForm form)
		{
			VoyageDetailsControl control = FindDetailsControl(form);
			return FindControl<ZGrid, VoyageDetailsControl>(control, "jobVoyOriginBoundGrid");
		}

		VoyageDetailsControl FindDetailsControl(ZJobVoyageForm form)
		{
			return FindControl<VoyageDetailsControl, ZJobVoyageForm>(form, "detailsControl");
		}

		RetT FindControl<RetT, ParentT>(ParentT parent, string name)
		{
			return (RetT)typeof(ParentT).InvokeMember(name, BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.Instance, null, parent, Array.Empty<object>());
		}

		protected override Form GetFormToBashCore()
		{
			var form = new TestJobVoyageForm(Voyage);
			form.ControllerID = ControllerIDs.JobSeaSailing;
			return form;
		}

		protected override void TearDown()
		{
			base.TearDown();

			if (fForm != null)
			{
				fForm.Dispose();
			}
		}

		#endregion
	}
}
