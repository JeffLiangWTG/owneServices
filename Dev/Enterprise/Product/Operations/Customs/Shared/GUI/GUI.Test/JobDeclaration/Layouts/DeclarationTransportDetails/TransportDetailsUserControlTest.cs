using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	sealed class TransportDetailsUserControlTest : TestCase
	{
		public void TestCaptionRenderingEnabled()
		{
			AssertEquals(true, control.CaptionRenderingEnabled);
		}

		public void TestOverrideValuesCheckBox()
		{
			AssertType<ZCheckBox>(control.OverrideValuesCheckBox);
		}

		public void TestMasterBillTextBox()
		{
			AssertType<ZMasterBillControl>(control.MasterBillTextBox);
		}

		public void TestOceanBillTextBox()
		{
			CombineAssertions(() =>
			{
				var oceanBillTextBox = control.OceanBillTextBox;
				AssertType<ZTextBox>("Control Type", oceanBillTextBox);
				AssertEquals("Caption", "Ocean Bill", oceanBillTextBox.CaptionResourceString.Caption);
			});
		}

		public void TestVehicleRegistrationNumberTextBox()
		{
			CombineAssertions(() =>
			{
				var vehicleRegistrationNumberTextBox = control.VehicleRegistrationNumberTextBox;
				AssertType<ZTextBox>("Control Type", vehicleRegistrationNumberTextBox);
				AssertEquals("Caption", "Registration", vehicleRegistrationNumberTextBox.CaptionResourceString.Caption);
			});
		}

		public void TestVoyageNumberTextBox()
		{
			CombineAssertions(() =>
			{
				var voyageNumberTextBox = control.VoyageNumberTextBox;
				AssertType<ZTextBox>("Control Type", voyageNumberTextBox);
				AssertEquals("Caption", "Voyage", voyageNumberTextBox.CaptionResourceString.Caption);
			});
		}

		public void TestVesselCodeFindBox()
		{
			AssertType<ZCodeFindBox>(control.VesselCodeFindBox);
		}

		public void TestTransportIDUserControl()
		{
			AssertType<TransportIDAndNationalityUserControl>(control.TransportIDAndNationalityUserControl);
		}

		public void TestPortOfLoadingUserControl()
		{
			AssertType<TransportDetailsPortOfLoadingUserControl>(control.PortOfLoadingUserControl);
		}

		public void TestPortOfDischargeUserControl()
		{
			AssertType<TransportDetailsPortOfDischargeUserControl>(control.PortOfDischargeUserControl);
		}

		public void TestFlightUserControl()
		{
			AssertType<TransportDetailsFlightUserControl>(control.FlightUserControl);
		}

		public void TestIATALoadPortCodeFindBox()
		{
			AssertType<ZCodeFindBox>(control.IATALoadPortCodeFindBox);
		}

		public void TestInlandModeOfTransportDropEdit()
		{
			AssertType<ZDropEdit>(control.InlandModeOfTransportDropEdit);
		}

		public void TestPortOfFirstArrivalUserControl()
		{
			AssertType<TransportDetailsPortOfFirstArrivalUserControl>(control.PortOfFirstArrivalUserControl);
		}

		public void TestFlightAndNationalityUserControl()
		{
			AssertType<FlightAndNationalityUserControl>(control.FlightAndNationalityUserControl);
		}

		public void TestVoyageAndNationalityUserControl()
		{
			AssertType<VoyageAndNationalityUserControl>(control.VoyageAndNationalityUserControl);
		}

		public void TestTransportDetailsPortOfLoadingWithIATAUserControl()
		{
			AssertType<TransportDetailsPortOfLoadingWithIATAUserControl>(control.TransportDetailsPortOfLoadingWithIATAUserControl);
		}

		public void TestTransportInlandSeparator()
		{
			var transportInlandSeparatorUserControl = control.TransportInlandSeparatorUserControl;
			CombineAssertions(() =>
			{
				AssertType<SeparatorUserControl>("Type", transportInlandSeparatorUserControl);
				AssertEquals("Caption", "Transport Inland", transportInlandSeparatorUserControl.CaptionResourceString.Caption);
			});
		}

		public void TestTransportInlandRoadUserControl()
		{
			AssertType<TransportInlandRoadUserControl>(control.TransportInlandRoadUserControl);
		}

		public void TestTransportInlandModeAndTypeOfIdUserControl()
		{
			AssertType<TransportInlandModeAndTypeOfIdUserControl>(control.TransportInlandModeAndTypeOfIdUserControl);
		}

		public void TestTransportInlandIDAndNationalityUserControl()
		{
			AssertType<TransportInlandIDAndNationalityUserControl>(control.TransportInlandIDAndNationalityUserControl);
		}

		public void TestTransportInlandAirUserControl()
		{
			AssertType<TransportInlandAirUserControl>(control.TransportInlandAirUserControl);
		}

		public void TestTransportInlandInlandWaterwaysUserControl()
		{
			AssertType<TransportInlandInlandWaterwaysUserControl>(control.TransportInlandInlandWaterwaysUserControl);
		}

		public void TestTransportInlandOwnPropulsionUserControl()
		{
			AssertType<TransportInlandOwnPropulsionUserControl>(control.TransportInlandOwnPropulsionUserControl);
		}

		public void TestTransportInlandRailUserControl()
		{
			AssertType<TransportInlandRailUserControl>(control.TransportInlandRailUserControl);
		}

		public void TestTransportInlandSeaUserControl()
		{
			AssertType<TransportInlandSeaUserControl>(control.TransportInlandSeaUserControl);
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new TransportDetailsUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
		TransportDetailsUserControl control;
	}
}
