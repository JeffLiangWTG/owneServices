using System.Windows.Forms;
using CargoWise.Common;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Internal;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.GUI.Testing
{
	[TestedType(typeof(SailingModulePopup))]
	sealed class SailingModulePopupTest : ZFormBasherTest
	{
		#region Implementation

		protected override Form GetFormToBashCore()
		{
			var shipment = Factory.New<CommonShipment>();
			var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.JobSeaSailing);
			return new SailingModulePopup(shipment, module);
		}

		public void TestOKButton_Click_CheckUserHasSecurityRightsToCreateNewSailings_Sea()
		{
			AssertOKButton_Click_CheckUserHasSecurityRightsToCreateNewSailings(Constants.TransportModes.Sea, ModuleIDs.JobSeaSailing, Env.Security.SailingScheduleNew, Env.Security.SailingScheduleCreateFromJob);
		}

		public void TestOKButton_Click_CheckUserHasSecurityRightsToCreateNewSailings_Air()
		{
			AssertOKButton_Click_CheckUserHasSecurityRightsToCreateNewSailings(Constants.TransportModes.Air, ModuleIDs.JobAirSailing, Env.Security.FlightScheduleNew, Env.Security.FlightScheduleCreateFromJob);
		}

		void AssertOKButton_Click_CheckUserHasSecurityRightsToCreateNewSailings(string transportMode, ModuleIdentifier iD, SecurityCheckpoint securityCheckpointNew, SecurityCheckpoint securityCheckpointCreateFromJob)
		{
			var voyage = Factory.NewWithValidTestData<JobVoyage>();
			voyage.JV_IsChartered = false;
			voyage.JV_AirSeaRoad = transportMode;
			voyage.JV_IsActive = true;

			var origin = voyage.Origins.AddNew();
			origin.FillWithValidTestData();

			var destination = voyage.Destinations.AddNew();
			destination.FillWithValidTestData();

			Factory.Save();

			var isAllowedNew = securityCheckpointNew.IsAllowed;
			var isAllowedCreateFromJob = securityCheckpointCreateFromJob.IsAllowed;
			using (new DisposableAction(() =>
			{
				securityCheckpointNew.IsAllowed = isAllowedNew;
				securityCheckpointCreateFromJob.IsAllowed = isAllowedCreateFromJob;
			}))
			{
				AssertOKButton_Click_CheckUserHasSecurityRightsToCreateNewSailings(transportMode, iD, securityCheckpointNew);

				securityCheckpointNew.IsAllowed = true;
				AssertOKButton_Click_CheckUserHasSecurityRightsToCreateNewSailings(transportMode, iD, securityCheckpointCreateFromJob);
			}
		}

		void AssertOKButton_Click_CheckUserHasSecurityRightsToCreateNewSailings(string transportMode, ModuleIdentifier iD, SecurityCheckpoint securityCheckpoint)
		{
			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.JS_TransportMode = transportMode;

			using (var form = new ZForm())
			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(iD))
			{
				var findBox = new SailingIFindBoxForTest(shipment, form);
				var popupModuleDecisionProvider = new PopupModuleDecisionProvider(findBox);
				module.OverrideModuleDecisionProvider(popupModuleDecisionProvider);

				using (var sailingModulePopup = new SailingModulePopup(shipment, module))
				{
					findBox.Popup = sailingModulePopup;
					sailingModulePopup.ShowModal(findBox, form);
					sailingModulePopup.Module_ForTest.PerformSearch_ForTest();
					AssertGreaterThanOrEqualTo(1, sailingModulePopup.Module_ForTest.GridCollection.Count);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					securityCheckpoint.IsAllowed = false;
					sailingModulePopup.Module_ForTest.DisplayGrid.Select(0);
					sailingModulePopup.ExposedOKButtonForTesting.PerformClick();
					Assert(UnitTestUserNotification.Instance.LastMessage.WasNone);
					AssertNotEquals(securityCheckpoint.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		#endregion
	}
}
