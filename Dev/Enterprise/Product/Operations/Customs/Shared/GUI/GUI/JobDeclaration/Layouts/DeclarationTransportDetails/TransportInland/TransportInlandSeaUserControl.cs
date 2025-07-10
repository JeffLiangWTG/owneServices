using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public partial class TransportInlandSeaUserControl : ZUserControl
	{
		public TransportInlandSeaUserControl()
		{
			InitializeComponent();
		}

		public static class ControlNames
		{
			public const string Parent = nameof(TransportInlandSeaUserControl);
			public const string VesselIDCodeFindBox = nameof(TransportInlandSeaUserControl.VesselIDCodeFindBox);
			public const string TransportNationalityCodeFindBox = nameof(TransportInlandSeaUserControl.TransportNationalityCodeFindBox);
		}
	}
}
