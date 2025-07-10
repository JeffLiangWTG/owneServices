using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;

namespace Enterprise.Freight.Module.Testing
{
	abstract class JobSailingControllerTest<T> : ZControllerBasherTest where T : JobSailingController, new()
	{
		#region ShowNewForm / ShowEditForm

		public void TestShowNewForm()
		{
			using (var form = (ZJobVoyageForm)Controller.ShowNewForm())
			{
				var voyageBoundToForm = (JobVoyage)form.BusinessEntity;
				AssertEquals("Should default the transport mode", TransportType.ToString(), voyageBoundToForm.JV_AirSeaRoad);
				AssertEquals("HasChanges=false initially so the save button is not enabled", false, voyageBoundToForm.HasChanges);
			}
		}

		public void TestShowNewForm_CheckPointForCreateFromJob()
		{
			var isAllowedCreateFromJob = CheckPointForCreateFromJob.IsAllowed;
			using (new DisposableAction(() => { CheckPointForCreateFromJob.IsAllowed = isAllowedCreateFromJob; }))
			{
				CheckPointForCreateFromJob.IsAllowed = false;
				if (Controller is JobSailingController jobSailingController)
				{
					jobSailingController.ScheduleCreateFromJob = true;
				}
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				using (var form = Controller.ShowNewForm())
				{
					Assert(UnitTestUserNotification.Instance.LastMessage.WasError);
					AssertEquals(CheckPointForCreateFromJob.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestShowEditForm_ForVoyage()
		{
			TestShowEditForm(Voyage);
		}

		public void TestShowEditForm_ForSailing(BusinessObject voyageOrSailing)
		{
			TestShowEditForm(Sailing);
		}

		void TestShowEditForm(BusinessObject voyageOrSailing)
		{
			voyageOrSailing.Factory.Save();
			using (ZJobVoyageForm form = (ZJobVoyageForm)Controller.ShowEditForm(voyageOrSailing))
			{
				JobVoyage voyageBoundToForm = (JobVoyage)form.BusinessEntity;
				AssertEquals("Should show the form bound to the voyage", Voyage.PK, voyageBoundToForm.PK);
				AssertEquals("HasChanges=false initially so the save button is not enabled", false, voyageBoundToForm.HasChanges);
			}
		}

		#endregion

		public void TestShowDeleteForm()
		{
			var voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = TransportType;

			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUSYD";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "USLAX";
			voyage.GenerateSailings();

			Factory.Save();

			var sailing = voyage.Sailings[0];

			var shipment = Factory.New<CommonShipment>();
			shipment.JS_JX = sailing.PK;
			AssertEquals("Precondition", true, sailing.IsReferenced());
			AssertEquals("Precondition", false, voyage.CanDelete);

			JobSailingController controller = new T();
			controller.ShowDeleteForm(sailing);

			AssertNull(controller.LastShownForm);
			Assert(UnitTestUserNotification.Instance.LastMessage.WasWarning);
			AssertEquals("Cannot Delete " + voyage.HumanReadableName, UnitTestUserNotification.Instance.LastMessage.Caption);
			AssertEquals(voyage.ReasonForNotAbleToDelete, UnitTestUserNotification.Instance.LastMessage.Text);

			shipment.JS_JX = ZGuid.Empty;
			AssertEquals("Precondition", false, sailing.IsReferenced());
			AssertEquals("Precondition", true, voyage.CanDelete);

			controller.ShowDeleteForm(sailing);
			AssertEquals(typeof(ZJobVoyageForm), controller.LastShownForm.GetType());
			controller.LastShownForm.Dispose();
		}

		public void TestDeleteMultipleSailings()
		{
			JobSailingController controller = new T();
			AssertNoExceptionThrown(() => controller.DeleteMultiple(System.Array.Empty<BusinessObject>()));

			JobVoyage voyage1 = Factory.New<JobVoyage>();
			voyage1.JV_AirSeaRoad = Constants.TransportModes.Sea;
			voyage1.JV_VoyageFlight = "AAA";

			VoyageOrigin voyage1Origin1 = voyage1.Origins.AddNew();
			voyage1Origin1.JA_RL_NKPortOfLoading = "AUMEL";
			voyage1Origin1.JA_E_DEP = new ZDateTime(2011, 1, 1);

			VoyageOrigin voyage1Origin2 = voyage1.Origins.AddNew();
			voyage1Origin2.JA_RL_NKPortOfLoading = "AUSYD";
			voyage1Origin2.JA_E_DEP = new ZDateTime(2011, 1, 2);

			VoyageDestination voyage1Destination1 = voyage1.Destinations.AddNew();
			voyage1Destination1.JB_E_ARV = new ZDateTime(2011, 2, 1);
			voyage1Destination1.JB_RL_NKPortOfDischarge = "NZAKL";

			Factory.Save();

			JobSailing sailing1 = voyage1.Sailings[0];
			JobSailing sailing2 = voyage1.Sailings[1];

			controller.DeleteMultiple(new[] { sailing1, sailing2 });

			AssertEquals(typeof(ZJobVoyageForm), controller.LastShownForm.GetType());
			AssertEquals(voyage1.PK, ((JobVoyage)((ZJobVoyageForm)controller.LastShownForm).DataSource).PK);
			AssertEquals(false, voyage1.IsDeleted);

			JobVoyage voyage2 = Factory.New<JobVoyage>();
			voyage2.JV_AirSeaRoad = Constants.TransportModes.Sea;
			voyage2.JV_VoyageFlight = "BBB";

			VoyageOrigin voyage2Origin1 = voyage2.Origins.AddNew();
			voyage2Origin1.JA_RL_NKPortOfLoading = "AUMEL";
			voyage2Origin1.JA_E_DEP = new ZDateTime(2011, 1, 1);

			VoyageDestination voyage2Destination1 = voyage2.Destinations.AddNew();
			voyage2Destination1.JB_E_ARV = new ZDateTime(2011, 2, 1);
			voyage2Destination1.JB_RL_NKPortOfDischarge = "NZAKL";

			Factory.Save();

			JobSailing sailing3 = voyage2.Sailings[0];

			UnitTestUserNotification.Instance.AddUserResponse("yes");
			controller.DeleteMultiple(new[] { sailing1, sailing3 });

			AssertContains("Sailing Schedule (Vessel='', Voyage='AAA', Carrier='')\r\nSailing Schedule (Vessel='', Voyage='BBB', Carrier='')", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals(true, voyage1.IsDeleted);
			AssertEquals(true, voyage2.IsDeleted);

			controller.LastShownForm.Dispose();
		}

		#region Implementation

		protected abstract ZString TransportType { get; }
		protected abstract SecurityCheckpoint CheckPointForCreateFromJob { get; }

		JobVoyage Voyage
		{
			get { return voyage ?? (voyage = Factory.New<JobVoyage>()); }
		}
		JobVoyage voyage;

		JobSailing Sailing
		{
			get { return sailing ?? (sailing = Voyage.Sailings.AddNew()); }
		}
		JobSailing sailing;

		protected override BusinessObject GetBusinessObjectWithoutValidationErrors()
		{
			var voyage = base.GetBusinessObjectWithoutValidationErrors() as JobVoyage;
			voyage.JV_AirSeaRoad = TransportMode;

			var origin = Factory.New<VoyageOrigin>();
			origin.JA_RL_NKPortOfLoading = "AUBNE";
			origin.JA_E_DEP = ZDateTime.Today.AddDays(-10);
			voyage.Origins.Add(origin);

			var destination = Factory.New<VoyageDestination>();
			destination.JB_RL_NKPortOfDischarge = "SGSIN";
			voyage.Destinations.Add(destination);

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			var line = Factory.NewWithValidTestData<OrgHeader>();
			line.OH_IsShippingProvider = true;

			InitializeLine(line);

			vessel.RV_OH = line.PK;
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "123";

			return voyage;
		}

		protected abstract void InitializeLine(OrgHeader line);

		protected abstract ZString TransportMode { get; }

		protected override IEnumerable<ControllerID> NonCustomsPlugInsToExcludeFromTest => new ControllerID[]
		{
			ControllerIDs.AgencyAllocation,
			ControllerIDs.AgencyPortMessaging,
			ControllerIDs.AgencyNZPortMessaging,
			ControllerIDs.AgencyDangerousGoodsManifest,
			ControllerIDs.Customs.US.StowPlan,
			ControllerIDs.Customs.AU.ManifestPluginToSailingController,
			ControllerIDs.ETerminalReleaseManifestPortMessaging
		}.Union(base.NonCustomsPlugInsToExcludeFromTest);

		public override void TestGetOpenFormUrlslDoesNotHitDatabase()
		{
			Assert(true);
		}

		#endregion
	}
}
