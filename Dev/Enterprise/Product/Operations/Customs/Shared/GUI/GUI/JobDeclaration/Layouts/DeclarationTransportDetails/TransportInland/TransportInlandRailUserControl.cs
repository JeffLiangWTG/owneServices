using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public partial class TransportInlandRailUserControl : ZUserControl
	{
		public TransportInlandRailUserControl()
		{
			InitializeComponent();
		}

		public static class ControlNames
		{
			public const string Parent = nameof(TransportInlandRailUserControl);
			public const string TrainNationalityCodeFindBox = nameof(TransportInlandRailUserControl.TrainNationalityCodeFindBox);
			public const string TrainNumberTextBox = nameof(TransportInlandRailUserControl.TrainNumberTextBox);
			public const string WagonNumberTextBox = nameof(TransportInlandRailUserControl.WagonNumberTextBox);
			public const string WagonNationalityCodeFindBox = nameof(TransportInlandRailUserControl.WagonNationalityCodeFindBox);
		}
	}
}
