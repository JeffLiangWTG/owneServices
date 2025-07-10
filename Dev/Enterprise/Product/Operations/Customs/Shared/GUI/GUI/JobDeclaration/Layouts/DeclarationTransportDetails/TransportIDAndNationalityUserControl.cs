using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public partial class TransportIDAndNationalityUserControl : ZUserControl
	{
		public TransportIDAndNationalityUserControl()
		{
			InitializeComponent();
		}

		public static class ControlNames
		{
			public const string TransportIDTextBox = nameof(TransportIDAndNationalityUserControl.TransportIDTextBox);
			public const string TransportNationalityFindBox = nameof(TransportIDAndNationalityUserControl.TransportNationalityFindBox);
		}
	}
}
