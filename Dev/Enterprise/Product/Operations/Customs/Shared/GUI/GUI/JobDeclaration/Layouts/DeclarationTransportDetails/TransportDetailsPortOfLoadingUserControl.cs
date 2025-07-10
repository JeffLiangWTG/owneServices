using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public partial class TransportDetailsPortOfLoadingUserControl : ZUserControl
	{
		public TransportDetailsPortOfLoadingUserControl()
		{
			InitializeComponent();
		}

		public static class ControlNames
		{
			public const string PortOfLoadingFindBox = nameof(TransportDetailsPortOfLoadingUserControl.PortOfLoadingFindBox);
			public const string ExportDateEdit = nameof(TransportDetailsPortOfLoadingUserControl.ExportDateEdit);
		}
	}
}
