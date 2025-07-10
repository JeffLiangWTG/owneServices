using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public partial class TransportDetailsPortOfDischargeUserControl : ZUserControl
	{
		public TransportDetailsPortOfDischargeUserControl()
		{
			InitializeComponent();
		}

		public static class ControlNames
		{
			public const string PortOfDischargeFindBox = nameof(TransportDetailsPortOfDischargeUserControl.PortOfDischargeFindBox);
			public const string DateOfArrivalDateEdit = nameof(TransportDetailsPortOfDischargeUserControl.DateOfArrivalDateEdit);
		}
	}
}
