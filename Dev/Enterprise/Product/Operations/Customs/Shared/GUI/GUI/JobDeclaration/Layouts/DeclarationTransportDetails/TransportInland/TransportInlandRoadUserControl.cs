using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public partial class TransportInlandRoadUserControl : ZUserControl
	{
		public TransportInlandRoadUserControl()
		{
			InitializeComponent();
		}

		public static class ControlNames
		{
			public const string Parent = nameof(TransportInlandRoadUserControl);
			public const string TransportIDTextBox = nameof(TransportInlandRoadUserControl.TransportIDTextBox);
			public const string Trailer1NationalityCodeFindBox = nameof(TransportInlandRoadUserControl.Trailer1NationalityCodeFindBox);
			public const string TransportNationalityCodeFindBox = nameof(TransportInlandRoadUserControl.TransportNationalityCodeFindBox);
			public const string Trailer1IDTextBox = nameof(TransportInlandRoadUserControl.Trailer1IDTextBox);
			public const string Trailer2IDTextBox = nameof(TransportInlandRoadUserControl.Trailer2IDTextBox);
			public const string Trailer2NationalityCodeFindBox = nameof(TransportInlandRoadUserControl.Trailer2NationalityCodeFindBox);
		}
	}
}
