using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Agency.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Agency.GUI.Testing
{
	internal class BillOfLadingMainPageTest : BaseAgencyTest
	{
		public void TestSetForContainerMode()
		{
			using (BillOfLadingMainPage control = new BillOfLadingMainPage())
			{
				ZCalcDropEdit packsCalcDropEdit = GetControl<ZCalcDropEdit>(control, "JS_OuterPacksBoundZCalcDropEdit");
				ZCalcDropEdit weightCalcDeropEdit = GetControl<ZCalcDropEdit>(control, "JS_ActualWeightBoundCalcDropEdit");
				ZCalcDropEdit volumeCalcDropEdit = GetControl<ZCalcDropEdit>(control, "JS_ActualVolumeBoundZCalcDropEdit");
				control.SetForContainerMode(Core.Constants.ContainerModes.Bulk);
				Assert(packsCalcDropEdit.Visible);
				Assert(weightCalcDeropEdit.Visible);
				Assert(volumeCalcDropEdit.Visible);
				AssertEquals("", packsCalcDropEdit.Text);
				control.SetForContainerMode(Constants.ContainerModes.FCL);
				Assert(!packsCalcDropEdit.Visible);
				Assert(!weightCalcDeropEdit.Visible);
				Assert(!volumeCalcDropEdit.Visible);
				AssertEquals("", packsCalcDropEdit.Text);
				control.SetForContainerMode(Constants.ContainerModes.RollOnRollOff);
				Assert(packsCalcDropEdit.Visible);
				Assert(weightCalcDeropEdit.Visible);
				Assert(volumeCalcDropEdit.Visible);
				AssertEquals("", packsCalcDropEdit.Text);
				control.SetForContainerMode(Constants.ContainerModes.BreakBulk);
				Assert(packsCalcDropEdit.Visible);
				Assert(weightCalcDeropEdit.Visible);
				Assert(volumeCalcDropEdit.Visible);
				AssertEquals("", packsCalcDropEdit.Text);
			}
		}

		public void TestChangeCommissionedShipment()
		{
			var shipment = Factory.NewWithValidTestData<BillOfLading>();
			shipment.JS_PackingMode = "FCL";
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "GBLON";
			Factory.Save();
			using (ZForm form = new ZForm(shipment))
			{
				using (var control = new BillOfLadingMainPageForTest())
				{
					form.Controls.Add(control);
					form.Show();
					Application.DoEvents();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					shipment.JS_RL_NKOrigin = "UAIEV";
					AssertEquals(shipment.JS_RL_NKOriginInfo, control.LastConfirmedInfo);
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					shipment.JS_RL_NKDestination = "USLAX";
					AssertEquals(shipment.JS_RL_NKDestinationInfo, control.LastConfirmedInfo);
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					shipment.JS_PackingMode = "ROR";
					AssertEquals(shipment.JS_PackingModeInfo, control.LastConfirmedInfo);
				}
			}
		}

		class BillOfLadingMainPageForTest : BillOfLadingMainPage
		{
			public ZPropertyInfo LastConfirmedInfo { get; set; }

			protected override void ConfirmReversal(ZPropertyInfo info)
			{
				LastConfirmedInfo = info;
				base.ConfirmReversal(info);
			}
		}

		#region Implementation
		T GetControl<T>(BillOfLadingMainPage control, string name)
		{
			return (T)typeof(BillOfLadingMainPage).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(control);
		}
		#endregion
	}
}
