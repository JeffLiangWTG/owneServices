using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Orders.GUI.Testing
{
	public class OrderPlanningVesselVoyageAndDatesControlTest : TestCaseWithFactory
	{
		public void TestVesselsVisibleOnlyOnSeaAndEmpty()
		{
			Order order = Factory.New<Order>();
			using (ZForm form = new ZForm(order))
			{
				TestOrderPlanningVesselVoyageAndDatesControl control = new TestOrderPlanningVesselVoyageAndDatesControl();
				form.Controls.Add(control);
				form.Show();
				Application.DoEvents();
				control.SetDataBinding(order, "");

				order.JD_TransportMode = string.Empty;
				Assert("Should be visible on empty", control.VesselDescriptionLabel.Visible);
				Assert("Should be visible on empty", control.DepartureVesselFindBox.Visible);
				Assert("Should be visible on empty", control.IntermediateVesselFindBox.Visible);
				Assert("Should be visible on empty", control.ArrivalVesselFindBox.Visible);

				order.JD_TransportMode = Core.Constants.TransportModes.Air;
				Assert("Should not be visible on air", !control.VesselDescriptionLabel.Visible);
				Assert("Should not be visible on air", !control.DepartureVesselFindBox.Visible);
				Assert("Should not be visible on air", !control.IntermediateVesselFindBox.Visible);
				Assert("Should not be visible on air", !control.ArrivalVesselFindBox.Visible);

				order.JD_TransportMode = Core.Constants.TransportModes.Sea;
				Assert("Should be visible on sea", control.VesselDescriptionLabel.Visible);
				Assert("Should be visible on sea", control.DepartureVesselFindBox.Visible);
				Assert("Should be visible on sea", control.IntermediateVesselFindBox.Visible);
				Assert("Should be visible on sea", control.ArrivalVesselFindBox.Visible);

				order.JD_TransportMode = Core.Constants.TransportModes.Rail;
				Assert("Should not be visible on rail", !control.VesselDescriptionLabel.Visible);
				Assert("Should not be visible on rail", !control.DepartureVesselFindBox.Visible);
				Assert("Should not be visible on rail", !control.IntermediateVesselFindBox.Visible);
				Assert("Should not be visible on rail", !control.ArrivalVesselFindBox.Visible);
			}
		}

		[RequiresSTA]
		public void TestVesselsVisibleOnlyOnSeaAndEmptyOnFormOpen()
		{
			Order order = Factory.New<Order>();

			order.JD_TransportMode = string.Empty;
			AssertVisibleOnFormOpen(order, true);

			order.JD_TransportMode = Core.Constants.TransportModes.Air;
			AssertVisibleOnFormOpen(order, false);

			order.JD_TransportMode = Core.Constants.TransportModes.Sea;
			AssertVisibleOnFormOpen(order, true);

			order.JD_TransportMode = Core.Constants.TransportModes.Rail;
			AssertVisibleOnFormOpen(order, false);
		}

		void AssertVisibleOnFormOpen(Order order, bool expectedVisible)
		{
			using (ZForm form = new ZForm(order))
			{
				TestOrderPlanningVesselVoyageAndDatesControl control = new TestOrderPlanningVesselVoyageAndDatesControl();
				form.Controls.Add(control);
				form.Show();
				Application.DoEvents();
				control.SetDataBinding(order, "");

				AssertEquals(expectedVisible, control.VesselDescriptionLabel.Visible);
				AssertEquals(expectedVisible, control.DepartureVesselFindBox.Visible);
				AssertEquals(expectedVisible, control.IntermediateVesselFindBox.Visible);
				AssertEquals(expectedVisible, control.ArrivalVesselFindBox.Visible);
			}
		}

		public class TestOrderPlanningVesselVoyageAndDatesControl : OrderPlanningVesselVoyageAndDatesControl
		{
			public ZArchitecture.ZLabel VesselDescriptionLabel
			{
				get { return base.VesselLabel; }
			}

			public ZCodeFindBox DepartureVesselFindBox
			{
				get { return base.JD_RV_NKDepartureVesselBoundFindBox; }
			}

			public ZCodeFindBox IntermediateVesselFindBox
			{
				get { return base.JD_RV_NKIntermediateVesselBoundFindBox; }
			}

			public ZCodeFindBox ArrivalVesselFindBox
			{
				get { return base.JD_RV_NKArrivalVesselBoundFindBox; }
			}
		}
	}
}
