using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public partial class TransportInlandOwnPropulsionUserControl : ZUserControl
	{
		public TransportInlandOwnPropulsionUserControl()
		{
			InitializeComponent();
		}

		public static class ControlNames
		{
			public const string Parent = nameof(TransportInlandOwnPropulsionUserControl);
			public const string TransportNationalityCodeFindBox = nameof(TransportInlandOwnPropulsionUserControl.TransportNationalityCodeFindBox);
			public const string TransportIDTextBox = nameof(TransportInlandOwnPropulsionUserControl.TransportIDTextBox);
			public const string TypeOfIDDropEdit = nameof(TransportInlandOwnPropulsionUserControl.TypeOfIDDropEdit);
		}
	}
}
