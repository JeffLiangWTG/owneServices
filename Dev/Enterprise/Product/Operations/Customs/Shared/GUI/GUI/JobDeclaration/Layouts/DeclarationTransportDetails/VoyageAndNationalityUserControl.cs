using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public partial class VoyageAndNationalityUserControl : ZUserControl
	{
		public VoyageAndNationalityUserControl()
		{
			InitializeComponent();
		}

		public static class ControlNames
		{
			public const string Parent = nameof(VoyageAndNationalityUserControl);
			public const string VoyageNumberTextBox = nameof(VoyageAndNationalityUserControl.VoyageNumberTextBox);
			public const string TransportNationalityFindBox = nameof(VoyageAndNationalityUserControl.TransportNationalityFindBox);
		}
	}
}
