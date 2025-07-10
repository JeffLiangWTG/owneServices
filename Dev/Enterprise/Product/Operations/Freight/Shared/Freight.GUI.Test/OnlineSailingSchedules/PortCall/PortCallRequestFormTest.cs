using System;
using System.Windows.Forms;
using Enterprise.Freight.OnlineSailingSchedules.PortCall;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.GUI.Testing
{
	[TestedType(typeof(PortCallRequestFormForTesting))]
	sealed class PortCallRequestFormTest : ZFormBasherTest
	{
		public void TestOKButton()
		{
			using (var form = new PortCallRequestFormForTesting(new PortCallManager(Factory)))
			using (var parentForm = new ZForm())
			{
				var findBox = new DummyFindBox();
				form.ShowModal(findBox, parentForm);

				var list = (PortCallResponseCollection)form.FilterControl.FilteredGrid.List;
				var portCall1 = new PortCallItem();
				portCall1.ArrivalNumber = "Ref1";
				var portCall2 = new PortCallItem();
				portCall2.ArrivalNumber = "Ref2";

				var response1 = list.AddNew();
				response1.RequestType = PortCallRequestType.Discharge;
				response1.SetValues(portCall1);

				var response2 = list.AddNew();
				response2.RequestType = PortCallRequestType.Discharge;
				response2.SetValues(portCall2);

				form.FilterControl.FilteredGrid.Select(1);
				form.OkButton.PerformClick();

				AssertEquals("Ref2", findBox.Code);
			}
		}

		public void TestOKButton_NoRowSelected()
		{
			using (var form = new PortCallRequestFormForTesting(new PortCallManager(Factory)))
			using (var parentForm = new ZForm())
			{
				var findBox = new DummyFindBox();
				form.ShowModal(findBox, parentForm);

				form.OkButton.PerformClick();

				AssertEquals(UnitTestUserNotification.Instance.LastMessage.Text, "Please select an item from the grid.");
				UnitTestUserNotification.Instance.ClearMessages();
			}
		}

		public void TestCancelButton()
		{
			using (var form = new PortCallRequestFormForTesting(new PortCallManager(Factory)))
			using (var parentForm = new ZForm())
			{
				var findBox = new DummyFindBox();
				form.ShowModal(findBox, parentForm);

				var list = (PortCallResponseCollection)form.FilterControl.FilteredGrid.List;
				var portCall1 = new PortCallItem();
				portCall1.ArrivalNumber = "Ref1";
				var portCall2 = new PortCallItem();
				portCall2.ArrivalNumber = "Ref2";

				var response1 = list.AddNew();
				response1.SetValues(portCall1);
				response1.RequestType = PortCallRequestType.Discharge;

				var response2 = list.AddNew();
				response2.RequestType = PortCallRequestType.Discharge;
				response2.SetValues(portCall2);

				form.FilterControl.FilteredGrid.Select(1);
				form.CancelButton.PerformClick();

				AssertNull(findBox.Code);
			}
		}

		public void TestFilterdGridColumns_Load()
		{
			var manager = new PortCallManager(Factory);
			manager.Request.RequestType = PortCallRequestType.Load;
			using (var form = new PortCallRequestFormForTesting(manager))
			{
				var filterControl = form.FilterControl;
				form.Show();

				AssertEquals("ETD", filterControl.FilteredGrid.GetColumnStyle("EstimatedTime").Caption);
			}
		}

		public void TestFilterdGridColumns_Discharge()
		{
			var manager = new PortCallManager(Factory);
			manager.Request.RequestType = PortCallRequestType.Discharge;
			using (var form = new PortCallRequestFormForTesting(manager))
			{
				var filterControl = form.FilterControl;
				form.Show();

				AssertEquals("ETA", filterControl.FilteredGrid.GetColumnStyle("EstimatedTime").Caption);
			}
		}

		protected override Form GetFormToBashCore()
		{
			return new PortCallRequestForm(new PortCallManager(Factory));
		}

		public override Type FormToBashType => typeof(PortCallRequestForm);
	}
}
