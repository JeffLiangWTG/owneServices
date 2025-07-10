using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NL.GUI
{
	public partial class ImportTransportInlandRoadUserControl : ZUserControl
	{
		public ImportTransportInlandRoadUserControl()
		{
			InitializeComponent();
		}

		public static class ControlNames
		{
			public const string Parent = nameof(ImportTransportInlandRoadUserControl);
			public const string TransportIDTextBox = nameof(ImportTransportInlandRoadUserControl.TransportIDTextBox);
			public const string TransportNationalityCodeFindBox = nameof(ImportTransportInlandRoadUserControl.TransportNationalityCodeFindBox);
		}
	}
}
