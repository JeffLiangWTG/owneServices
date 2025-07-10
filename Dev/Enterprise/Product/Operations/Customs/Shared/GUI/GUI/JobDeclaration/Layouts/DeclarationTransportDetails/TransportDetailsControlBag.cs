using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public sealed class TransportDetailsControlBag : ControlBag
	{
		protected override Control CreateTemplate() => new TransportDetailsUserControl();

		public static TransportDetailsControlBag Instance => instance ?? (instance = new TransportDetailsControlBag());

		[ThreadStatic]
		static TransportDetailsControlBag instance;

		TransportDetailsControlBag()
		{
			OverrideValuesCheckBox = RegisterControl(nameof(TransportDetailsUserControl.OverrideValuesCheckBox));
			PortOfLoadingUserControl = RegisterControl(nameof(TransportDetailsUserControl.PortOfLoadingUserControl));
			PortOfFirstArrivalUserControl = RegisterControl(nameof(TransportDetailsUserControl.PortOfFirstArrivalUserControl));
			PortOfDischargeUserControl = RegisterControl(nameof(TransportDetailsUserControl.PortOfDischargeUserControl));
			VehicleRegistrationNumberTextBox = RegisterControl(nameof(TransportDetailsUserControl.VehicleRegistrationNumberTextBox));
			MasterBillTextBox = RegisterControl(nameof(TransportDetailsUserControl.MasterBillTextBox));
			FlightUserControl = RegisterControl(nameof(TransportDetailsUserControl.FlightUserControl));
			IATALoadPortCodeFindBox = RegisterControl(nameof(TransportDetailsUserControl.IATALoadPortCodeFindBox));
			VoyageNumberTextBox = RegisterControl(nameof(TransportDetailsUserControl.VoyageNumberTextBox));
			VesselCodeFindBox = RegisterControl(nameof(TransportDetailsUserControl.VesselCodeFindBox));
			OceanBillTextBox = RegisterControl(nameof(TransportDetailsUserControl.OceanBillTextBox));
			TransportIDAndNationalityUserControl = RegisterControl(nameof(TransportDetailsUserControl.TransportIDAndNationalityUserControl));
			FlightAndNationalityUserControl = RegisterControl(nameof(TransportDetailsUserControl.FlightAndNationalityUserControl));
			InlandModeOfTransportDropEdit = RegisterControl(nameof(TransportDetailsUserControl.InlandModeOfTransportDropEdit));
			VoyageAndNationalityUserControl = RegisterControl(nameof(TransportDetailsUserControl.VoyageAndNationalityUserControl));
			TransportDetailsPortOfLoadingWithIATAUserControl = RegisterControl(nameof(TransportDetailsUserControl.TransportDetailsPortOfLoadingWithIATAUserControl));
			TransportInlandSeparatorUserControl = RegisterControl(nameof(TransportDetailsUserControl.TransportInlandSeparatorUserControl));
			TransportInlandModeAndTypeOfIdUserControl = RegisterControl(nameof(TransportDetailsUserControl.TransportInlandModeAndTypeOfIdUserControl));
			TransportInlandIDAndNationalityUserControl = RegisterControl(nameof(TransportDetailsUserControl.TransportInlandIDAndNationalityUserControl));
			TransportInlandRoadUserControl = RegisterControl(nameof(TransportDetailsUserControl.TransportInlandRoadUserControl));
			TransportInlandAirUserControl = RegisterControl(nameof(TransportDetailsUserControl.TransportInlandAirUserControl));
			TransportInlandInlandWaterwaysUserControl = RegisterControl(nameof(TransportDetailsUserControl.TransportInlandInlandWaterwaysUserControl));
			TransportInlandOwnPropulsionUserControl = RegisterControl(nameof(TransportDetailsUserControl.TransportInlandOwnPropulsionUserControl));
			TransportInlandRailUserControl = RegisterControl(nameof(TransportDetailsUserControl.TransportInlandRailUserControl));
			TransportInlandSeaUserControl = RegisterControl(nameof(TransportDetailsUserControl.TransportInlandSeaUserControl));
			UCRTextBox = RegisterControl(nameof(TransportDetailsUserControl.UCRTextBox));
			SubLocationOfGoodsTextBox = RegisterControl(nameof(TransportDetailsUserControl.SubLocationOfGoodsTextBox));
			GoodsDestinationCountryCodeFindBox = RegisterControl(nameof(TransportDetailsUserControl.GoodsDestinationCountryCodeFindBox));
			TransportMeansDropEdit = RegisterControl(nameof(TransportDetailsUserControl.TransportMeansDropEdit));
			CustomsOfficeCodeFindBox = RegisterControl(nameof(TransportDetailsUserControl.CustomsOfficeCodeFindBox));
			CustomsLoadPortCodeFindBox = RegisterControl(nameof(TransportDetailsUserControl.CustomsLoadPortCodeFindBox));
		}

		public ControlReference OverrideValuesCheckBox;
		public ControlReference MasterBillTextBox;
		public ControlReference FlightUserControl;
		public ControlReference VehicleRegistrationNumberTextBox;
		public ControlReference VoyageNumberTextBox;
		public ControlReference VesselCodeFindBox;
		public ControlReference OceanBillTextBox;
		public ControlReference PortOfLoadingUserControl;
		public ControlReference PortOfDischargeUserControl;
		public ControlReference TransportIDAndNationalityUserControl;
		public ControlReference FlightAndNationalityUserControl;
		public ControlReference IATALoadPortCodeFindBox;
		public ControlReference InlandModeOfTransportDropEdit;
		public ControlReference PortOfFirstArrivalUserControl;
		public ControlReference VoyageAndNationalityUserControl;
		public ControlReference TransportDetailsPortOfLoadingWithIATAUserControl;
		public ControlReference TransportInlandSeparatorUserControl;
		public ControlReference TransportInlandModeAndTypeOfIdUserControl;
		public ControlReference TransportInlandIDAndNationalityUserControl;
		public ControlReference TransportInlandRoadUserControl;
		public ControlReference TransportInlandAirUserControl;
		public ControlReference TransportInlandInlandWaterwaysUserControl;
		public ControlReference TransportInlandOwnPropulsionUserControl;
		public ControlReference TransportInlandRailUserControl;
		public ControlReference TransportInlandSeaUserControl;
		public ControlReference UCRTextBox;
		public ControlReference SubLocationOfGoodsTextBox;
		public ControlReference GoodsDestinationCountryCodeFindBox;
		public ControlReference TransportMeansDropEdit;
		public ControlReference CustomsOfficeCodeFindBox;
		public ControlReference CustomsLoadPortCodeFindBox;
	}
}
