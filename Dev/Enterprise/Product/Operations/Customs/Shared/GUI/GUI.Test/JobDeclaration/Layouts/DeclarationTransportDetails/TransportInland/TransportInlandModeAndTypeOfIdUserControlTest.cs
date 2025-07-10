using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	sealed class TransportInlandModeAndTypeOfIdUserControlTest : TestCase
	{
		public void TestCaptionRenderingEnabled()
		{
			AssertEquals("The layout needs the caption resource strings", true, control.CaptionRenderingEnabled);
		}

		public void TestInlandModeOfTransportDropEdit()
		{
			var inlandModeOfTransportDropEdit = control.InlandModeOfTransportDropEdit;
			CombineAssertions(() =>
			{
				AssertEquals("Changing position breaks the layout", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true), inlandModeOfTransportDropEdit.Location);
				AssertEquals("Tab", 0, inlandModeOfTransportDropEdit.TabIndex);
				AssertEquals("Binding", "JE_TransportModeInland", inlandModeOfTransportDropEdit.BindTo);
			});
		}

		public void TestTypeOfIDDropEdit()
		{
			var typeOfIDDropEdit = control.TypeOfIDDropEdit;
			CombineAssertions(() =>
			{
				AssertEquals("Changing position breaks the layout", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(200, 0, true), typeOfIDDropEdit.Location);
				AssertEquals("Tab", 1, typeOfIDDropEdit.TabIndex);
				AssertEquals("Binding", "JE_TransportMeans", typeOfIDDropEdit.BindTo);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new TransportInlandModeAndTypeOfIdUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
		TransportInlandModeAndTypeOfIdUserControl control;
	}
}
