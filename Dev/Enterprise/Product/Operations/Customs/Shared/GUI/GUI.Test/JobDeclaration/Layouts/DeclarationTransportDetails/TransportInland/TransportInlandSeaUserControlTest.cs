using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	sealed class TransportInlandSeaUserControlTest : TestCase
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
				AssertEquals("Changing position breaks the layout", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(287, 0, true), transportNationalityCodeFindBox.Location);
				AssertEquals("Tab", 1, transportNationalityCodeFindBox.TabIndex);
				AssertEquals("Binding", "JE_RN_NKTransportNationalityInland", transportNationalityCodeFindBox.BindTo);
			});
		}

		public void TestVesselIDCodeFindBox()
		{
			var vesselIDCodeFindBox = control.VesselIDCodeFindBox;
			CombineAssertions(() =>
			{
				AssertEquals("Changing position breaks the layout", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true), vesselIDCodeFindBox.Location);
				AssertEquals("Tab", 0, vesselIDCodeFindBox.TabIndex);
				AssertEquals("Character Casing", System.Windows.Forms.CharacterCasing.Normal, vesselIDCodeFindBox.CodeBox.CharacterCasing);
				AssertEquals("Caption", "Vessel ID", vesselIDCodeFindBox.CaptionResourceString.Caption);
				AssertEquals("ShortCaption", "ID", vesselIDCodeFindBox.CaptionResourceString.ShortCaption);
				AssertEquals("Binding", "JE_TransportIDInland", vesselIDCodeFindBox.BindTo);
				AssertEquals("BindToList", "Lookups.InlandVesselNamesOrLloyds", vesselIDCodeFindBox.BindToList);
				AssertEquals("CodeBox.MaximumSize", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(164, 20, true), vesselIDCodeFindBox.CodeBox.MaximumSize);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new TransportInlandSeaUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
		TransportInlandSeaUserControl control;
	}
}
