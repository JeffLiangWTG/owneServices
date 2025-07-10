using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public partial class TransportInlandAirUserControl : ZUserControl
	{
		public TransportInlandAirUserControl()
		{
			InitializeComponent();
		}

		public static class ControlNames
		{
			public const string Parent = nameof(TransportInlandAirUserControl);
			public const string TransportNationalityCodeFindBox = nameof(TransportInlandAirUserControl.TransportNationalityCodeFindBox);
			public const string FlightTextBox = nameof(TransportInlandAirUserControl.FlightTextBox);
			public const string AircraftIDTextBox = nameof(TransportInlandAirUserControl.AircraftIDTextBox);
		}
	}
}
