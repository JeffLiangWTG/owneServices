using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	sealed class TransportDetailsPortOfFirstArrivalUserControlTest : TestCase
	{
		public void TestCaptionRenderingEnabled()
		{
			AssertEquals("The layout needs the caption resource strings", true, control.CaptionRenderingEnabled);
		}

		public void TestPortOfFirstArrivalCodeFindBox()
		{
			var portOfFirstArrivalCodeFindBox = control.PortOfFirstArrivalCodeFindBox;
			CombineAssertions(() =>
			{
				AssertEquals("Changing position breaks the layout", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true), portOfFirstArrivalCodeFindBox.Location);
				AssertEquals("Before the Export Date", 0, portOfFirstArrivalCodeFindBox.TabIndex);
				AssertEquals("Module ID", ZArchitecture.Modules.ModuleIDs.RefUNLOCO, portOfFirstArrivalCodeFindBox.ModuleID);
				AssertEquals("Pre Bound Max Length", 5, portOfFirstArrivalCodeFindBox.PreBoundMaxLength);
			});
		}

		public void TestDateOfFirstArrivalBoundDateEdit()
		{
			var dateEdit = control.DateOfFirstArrivalBoundDateEdit;
			CombineAssertions(() =>
			{
				AssertEquals("Changing position breaks the layout", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(250, 0, true), dateEdit.Location);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new TransportDetailsPortOfFirstArrivalUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
		TransportDetailsPortOfFirstArrivalUserControl control;
	}
}
