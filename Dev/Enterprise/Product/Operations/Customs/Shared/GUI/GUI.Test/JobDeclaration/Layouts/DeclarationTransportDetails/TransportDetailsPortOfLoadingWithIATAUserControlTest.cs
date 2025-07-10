using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	sealed class TransportDetailsPortOfLoadingWithIATAUserControlTest : TestCase
	{
		public void TestCaptionRenderingEnabled()
		{
			AssertEquals("The layout needs the caption resource strings", true, control.CaptionRenderingEnabled);
		}

		public void TestPortOfLoadingFindBox()
		{
			var portOfLoadingFindBox = control.PortOfLoadingFindBox;
			CombineAssertions(() =>
			{
				AssertEquals("Changing position breaks the layout", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true), portOfLoadingFindBox.Location);
				AssertEquals("Tab", 0, portOfLoadingFindBox.TabIndex);
				AssertEquals("Module ID", ZArchitecture.Modules.ModuleIDs.RefUNLOCO, portOfLoadingFindBox.ModuleID);
				AssertEquals("Pre Bound Max Length", 5, portOfLoadingFindBox.PreBoundMaxLength);
				AssertEquals("Binding", "JE_RL_NKPortOfLoading", portOfLoadingFindBox.BindTo);
			});
		}

		public void TestExportDateEdit()
		{
			var exportDateEdit = control.ExportDateEdit;
			CombineAssertions(() =>
			{
				AssertEquals("Changing position breaks the layout", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(134, 0, true), exportDateEdit.Location);
				AssertEquals("Tab", 1, exportDateEdit.TabIndex);
				AssertEquals("Binding", "JE_ExportDate", exportDateEdit.BindTo);
				AssertEquals("Short Caption", "Dep.", exportDateEdit.CaptionResourceString.ShortCaption);
				AssertEquals("Caption", "Departure", exportDateEdit.CaptionResourceString.Caption);
			});
		}

		public void TestIATALoadPortCodeFindBox()
		{
			var iataLoadPortCodeFindBox = control.IATALoadPortCodeFindBox;
			CombineAssertions(() =>
			{
				AssertEquals("Changing position breaks the layout", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(277, 0, true), iataLoadPortCodeFindBox.Location);
				AssertEquals("Tab", 2, iataLoadPortCodeFindBox.TabIndex);
				AssertEquals("Caption", "IATA", iataLoadPortCodeFindBox.CaptionResourceString.Caption);
				AssertEquals("Binding", "JE_IATALoadPort", iataLoadPortCodeFindBox.BindTo);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new TransportDetailsPortOfLoadingWithIATAUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
		TransportDetailsPortOfLoadingWithIATAUserControl control;
	}
}
