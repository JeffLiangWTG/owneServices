using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public partial class TransportDetailsPortOfFirstArrivalUserControl : ZUserControl
	{
		public TransportDetailsPortOfFirstArrivalUserControl()
		{
			InitializeComponent();
		}

		public static class ControlNames
		{
			public const string PortOfFirstArrivalCodeFindBox = nameof(TransportDetailsPortOfFirstArrivalUserControl.PortOfFirstArrivalCodeFindBox);
			public const string DateOfFirstArrivalBoundDateEdit = nameof(TransportDetailsPortOfFirstArrivalUserControl.DateOfFirstArrivalBoundDateEdit);
		}
	}
}
