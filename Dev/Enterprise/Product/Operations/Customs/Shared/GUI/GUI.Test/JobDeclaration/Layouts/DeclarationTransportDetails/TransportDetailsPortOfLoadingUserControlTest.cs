using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	sealed class TransportDetailsPortOfLoadingUserControlTest : TestCase
	{
		public void TestCaptionRenderingEnabled()
		{
			AssertEquals("The layout needs the caption resource strings", true, control.CaptionRenderingEnabled);
		}

		public void TestPortOfDischargeFindBox()
		{
			var portOfLoadingFindBox = control.PortOfLoadingFindBox;
			CombineAssertions(() =>
			{
				AssertEquals("Changing position breaks the layout", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true), portOfLoadingFindBox.Location);
				AssertEquals("Before the Export Date", 0, portOfLoadingFindBox.TabIndex);
				AssertEquals("Module ID", ZArchitecture.Modules.ModuleIDs.RefUNLOCO, portOfLoadingFindBox.ModuleID);
				AssertEquals("Pre Bound Max Length", 5, portOfLoadingFindBox.PreBoundMaxLength);
			});
		}

		public void TestExportDateEdit()
		{
			var exportDateEdit = control.ExportDateEdit;
			CombineAssertions(() =>
			{
				AssertEquals("Changing position breaks the layout", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(250, 0, true), exportDateEdit.Location);
				AssertEquals("After the Port Of Discharge", 1, exportDateEdit.TabIndex);
				AssertEquals("Short Caption", "Dep.", exportDateEdit.CaptionResourceString.ShortCaption);
				AssertEquals("Caption", "Departure", exportDateEdit.CaptionResourceString.Caption);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new TransportDetailsPortOfLoadingUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
		TransportDetailsPortOfLoadingUserControl control;
	}
}
