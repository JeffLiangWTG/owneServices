using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public partial class TransportDetailsPortOfLoadingWithIATAUserControl : ZUserControl
	{
		public TransportDetailsPortOfLoadingWithIATAUserControl()
		{
			InitializeComponent();
		}

		public static class ControlNames
		{
			public const string PortOfLoadingFindBox = nameof(TransportDetailsPortOfLoadingWithIATAUserControl.PortOfLoadingFindBox);
			public const string ExportDateEdit = nameof(TransportDetailsPortOfLoadingWithIATAUserControl.ExportDateEdit);
			public const string IATALoadPortCodeFindBox = nameof(TransportDetailsPortOfLoadingWithIATAUserControl.IATALoadPortCodeFindBox);
		}
	}
}
