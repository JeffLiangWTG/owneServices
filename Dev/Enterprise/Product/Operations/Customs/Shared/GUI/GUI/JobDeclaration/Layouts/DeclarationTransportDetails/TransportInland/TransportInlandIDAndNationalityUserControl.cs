using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public partial class TransportInlandIDAndNationalityUserControl : ZUserControl
	{
		public TransportInlandIDAndNationalityUserControl()
		{
			InitializeComponent();
		}

		public static class ControlNames
		{
			public const string Parent = nameof(TransportInlandIDAndNationalityUserControl);
			public const string TransportNationalityFindBox = nameof(TransportIDAndNationalityUserControl.TransportNationalityFindBox);
			public const string TransportIDTextBox = nameof(TransportIDAndNationalityUserControl.TransportIDTextBox);
		}
	}
}
