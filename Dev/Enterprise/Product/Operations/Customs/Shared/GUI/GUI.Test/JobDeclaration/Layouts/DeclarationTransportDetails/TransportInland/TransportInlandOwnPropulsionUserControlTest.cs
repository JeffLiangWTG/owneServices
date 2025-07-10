using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	sealed class TransportInlandOwnPropulsionUserControlTest : TestCase
	{
		public void TestCaptionRenderingEnabled()
		{
			AssertEquals("The layout needs the caption resource strings", true, control.CaptionRenderingEnabled);
		}

		public void TestTransportNationalityCodeFindBox()
		{
			var transportNationalityCodeFindBox = control.TransportNationalityCodeFindBox;
			CombineAssertions(() =>
			{
				AssertEquals("Changing position breaks the layout", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(287, 24, true), transportNationalityCodeFindBox.Location);
				AssertEquals("Tab", 2, transportNationalityCodeFindBox.TabIndex);
				AssertEquals("Binding", "JE_RN_NKTransportNationalityInland", transportNationalityCodeFindBox.BindTo);
			});
		}

		public void TestTransportIDTextBox()
		{
			var transportIDTextBox = control.TransportIDTextBox;
			CombineAssertions(() =>
			{
				AssertEquals("Changing position breaks the layout", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 24, true), transportIDTextBox.Location);
				AssertEquals("Tab", 1, transportIDTextBox.TabIndex);
				AssertEquals("CharacterCasing", System.Windows.Forms.CharacterCasing.Normal, transportIDTextBox.CharacterCasing);
				AssertEquals("Caption", "Transport ID", transportIDTextBox.CaptionResourceString.Caption);
				AssertEquals("ShortCaption", "ID", transportIDTextBox.CaptionResourceString.ShortCaption);
				AssertEquals("Binding", "JE_TransportIDInland", transportIDTextBox.BindTo);
			});
		}

		public void TestTypeOfIDDropEdit()
		{
			var typeOfIDDropEdit = control.TypeOfIDDropEdit;
			CombineAssertions(() =>
			{
				AssertEquals("Changing position breaks the layout", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true), typeOfIDDropEdit.Location);
				AssertEquals("Tab", 0, typeOfIDDropEdit.TabIndex);
				AssertEquals("Binding", "JE_TransportMeans", typeOfIDDropEdit.BindTo);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new TransportInlandOwnPropulsionUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
		TransportInlandOwnPropulsionUserControl control;
	}
}
