using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.Freight.GUI.Testing
{
	sealed class VoyageIFindBoxTest : TestCaseWithFactory
	{
		public void TestInfoWithRights()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = Constants.TransportModes.Sea;
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "NLAMS";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUBNE";
			Factory.Save();

			Env.Security.SailingSchedule.IsAllowed = true;

			DummyBusinessObject dummy = Factory.New<DummyBusinessObject>();

			using (ZForm form = new ZForm(dummy))
			{
				VoyageIFindBox.SelectSeaVoyage(form, dummy.Z0_GuidInfo);

				using (EmbeddedModulePopup popup = (EmbeddedModulePopup)ZFormModaliser.ActiveForm)
				{
					AssertEquals("Should not show a dialog", "None ", UnitTestUserNotification.Instance.LastMessage.ToString());
					AssertEquals("Should be showing the popup", "Enterprise.Freight.GUI.VoyageIFindBox+VoyageModulePopup", popup == null ? null : popup.GetType().FullName);

					DoFind(popup);

					AssertEquals("should not have set the pk yet", ZGuid.Empty, dummy.Z0_Guid);

					((IFindBoxPopup)popup).SelectRowByPK(voyage.PK);
					popup.ExposedOKButtonForTesting.PerformClick();

					AssertEquals("should have set the pk now", voyage.PK, dummy.Z0_Guid);
				}
			}
		}

		public void TestPropertySetterWithRights()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = Constants.TransportModes.Sea;
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "NLAMS";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUBNE";
			Factory.Save();

			Env.Security.SailingSchedule.IsAllowed = true;

			DummyBusinessObject dummy = Factory.New<DummyBusinessObject>();

			using (ZForm form = new ZForm(dummy))
			{
				ZGuid selectedPk = ZGuid.Empty;
				bool selectedPkSet = false;

				VoyageIFindBox.SelectSeaVoyage(form, Factory, delegate(ZGuid value)
				{
					selectedPk = value;
					selectedPkSet = true;
				});

				using (EmbeddedModulePopup popup = (EmbeddedModulePopup)ZFormModaliser.ActiveForm)
				{
					AssertEquals("Should not show a dialog", "None ", UnitTestUserNotification.Instance.LastMessage.ToString());
					AssertEquals("Should be showing the popup", "Enterprise.Freight.GUI.VoyageIFindBox+VoyageModulePopup", popup == null ? null : popup.GetType().FullName);

					DoFind(popup);

					AssertEquals("should not have set the pk yet", false, selectedPkSet);
					AssertEquals("should not have set the pk yet", ZGuid.Empty, selectedPk);

					((IFindBoxPopup)popup).SelectRowByPK(voyage.PK);
					popup.ExposedOKButtonForTesting.PerformClick();

					AssertEquals("should have set the pk now", true, selectedPkSet);
					AssertEquals("should have set the pk now", voyage.PK, selectedPk);
				}
			}
		}

		public void TestWithoutRights()
		{
			Env.Security.SailingSchedule.IsAllowed = false;

			DummyBusinessObject dummy = Factory.New<DummyBusinessObject>();

			using (ZForm form = new ZForm(dummy))
			{
				ZGuid selectedPk = ZGuid.Empty;
				bool selectedPkSet = false;

				VoyageIFindBox.SelectSeaVoyage(form, Factory, delegate(ZGuid value)
				{
					selectedPk = value;
					selectedPkSet = true;
				});

				using (Form popup = ZFormModaliser.ActiveForm)
				{
					const string expectedErrorText =
						"Error You do not have the appropriate security rights to run this function.\r\n" +
						"\r\n" +
						"If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:\r\n" +
						"\r\n" +
						"Operate -> Schedules -> Sailing Schedule\r\n" +
						"";

					AssertMultilineASCIIEquals("Should show a dialog", expectedErrorText, UnitTestUserNotification.Instance.LastMessage.ToString());
					AssertEquals("Should not show the popup", null, popup == null ? null : popup.GetType().FullName);

					AssertEquals("should not have set the pk", false, selectedPkSet);
					AssertEquals("should not have set the pk", ZGuid.Empty, selectedPk);
				}
			}
		}

		#region Implementation

		void DoFind(EmbeddedModulePopup popup)
		{
			var module = popup.Module_ForTest;
			((ZFilterStripControl)module.EmbeddedControl).FirePerformSearch();
		}

		#endregion
	}
}
