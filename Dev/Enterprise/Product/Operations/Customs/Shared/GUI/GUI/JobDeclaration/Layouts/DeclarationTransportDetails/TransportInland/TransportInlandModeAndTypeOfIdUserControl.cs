using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public partial class TransportInlandModeAndTypeOfIdUserControl : ZUserControl
	{
		public TransportInlandModeAndTypeOfIdUserControl()
		{
			InitializeComponent();
		}

		public static class ControlNames
		{
			public const string InlandModeOfTransportDropEdit = nameof(TransportInlandModeAndTypeOfIdUserControl.InlandModeOfTransportDropEdit);
			public const string TypeOfIDDropEdit = nameof(TransportInlandModeAndTypeOfIdUserControl.TypeOfIDDropEdit);
		}
	}
}
