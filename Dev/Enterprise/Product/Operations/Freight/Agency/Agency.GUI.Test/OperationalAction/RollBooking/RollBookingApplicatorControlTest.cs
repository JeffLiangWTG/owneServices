using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Agency.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.GUI.Testing
{
	[TestedType(typeof(RollBookingApplicator))]
	internal class RollBookingApplicatorControlTest : OperationalActionMethodApplicatorTest
	{
		public void TestSelectScheduleButton_NoSelection()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsShippingLine = true;
			carrier.OH_IsShippingProvider = true;
			CreateTransport(Core.Constants.TransportPlanningType.MainVessel, "XI FENG KOU", "9955", ZDateTime.Today.AddDays(3), ZDateTime.Today.AddDays(10), carrier, false);
			using (var form = new ZForm())
			using (var control = new RollBookingApplicatorControl())
			{
				form.Controls.Add(control);
				control.SetDataBinding(Applicator, string.Empty);
				form.Show();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				control.SelectScheduleButton.PerformClick();
				AssertEquals("Please select a linked routing leg first.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSelectScheduleButton_SelectionIsNotLinked()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsShippingLine = true;
			carrier.OH_IsShippingProvider = true;
			CreateTransport(Core.Constants.TransportPlanningType.MainVessel, "XI FENG KOU", "9955", ZDateTime.Today.AddDays(3), ZDateTime.Today.AddDays(10), carrier, false);
			using (var form = new ZForm())
			using (var control = new RollBookingApplicatorControl())
			{
				form.Controls.Add(control);
				control.SetDataBinding(Applicator, string.Empty);
				form.Show();
				control.TransportsGrid.Select(0);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				control.SelectScheduleButton.PerformClick();
				AssertEquals("Current leg is not linked.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSelectScheduleButton_MultipleSelection()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsShippingLine = true;
			carrier.OH_IsShippingProvider = true;
			CreateTransport(Core.Constants.TransportPlanningType.MainVessel, "XI FENG KOU", "9955", ZDateTime.Today.AddDays(3), ZDateTime.Today.AddDays(10), carrier, true);
			CreateTransport(Core.Constants.TransportPlanningType.Other, "MAASMOND", "5577", ZDateTime.Today.AddDays(3), ZDateTime.Today.AddDays(10), carrier, false);
			using (var form = new ZForm())
			using (var control = new RollBookingApplicatorControl())
			{
				form.Controls.Add(control);
				control.SetDataBinding(Applicator, string.Empty);
				form.Show();
				control.TransportsGrid.Select(0);
				control.TransportsGrid.Select(1);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				control.SelectScheduleButton.PerformClick();
				AssertEquals("Please select only one routing leg.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#region Implementation
		void CreateTransport(ZString transportType, ZString vessel, ZString voyageFlight, ZDateTime etd, ZDateTime eta, OrgHeader carrier, bool isLinked)
		{
			var transport = ((RollBookingApplicator)Applicator).Transports.AddNew();
			transport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport.JW_TransportType = Core.Constants.TransportPlanningType.MainVessel;
			transport.JW_Vessel = vessel;
			transport.JW_VoyageFlight = voyageFlight;
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "NZAKL";
			transport.JW_ETD = etd;
			transport.JW_ETA = eta;
			transport.CarrierPK = carrier.PK;
			transport.JW_IsLinked = isLinked;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new RollBookingApplicator(Factory);
		}
		#endregion
	}
}
