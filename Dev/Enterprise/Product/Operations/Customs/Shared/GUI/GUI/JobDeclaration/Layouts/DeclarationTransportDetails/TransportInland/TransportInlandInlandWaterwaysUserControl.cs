using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public partial class TransportInlandInlandWaterwaysUserControl : ZUserControl
	{
		public TransportInlandInlandWaterwaysUserControl()
		{
			InitializeComponent();
		}

		public static class ControlNames
		{
			public const string Parent = nameof(TransportInlandInlandWaterwaysUserControl);
			public const string TransportNationalityCodeFindBox = nameof(TransportInlandInlandWaterwaysUserControl.TransportNationalityCodeFindBox);
			public const string VesselIDTextBox = nameof(TransportInlandInlandWaterwaysUserControl.VesselIDTextBox);
		}
	}
}
