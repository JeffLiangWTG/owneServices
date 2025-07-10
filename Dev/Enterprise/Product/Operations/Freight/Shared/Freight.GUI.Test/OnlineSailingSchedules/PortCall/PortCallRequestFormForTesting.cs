using System.Windows.Forms;
using Enterprise.Freight.OnlineSailingSchedules.PortCall;

namespace Enterprise.Freight.GUI.Testing
{
	sealed class PortCallRequestFormForTesting : PortCallRequestForm
	{
		public PortCallRequestFormForTesting(PortCallManager manager)
			: base(manager)
		{
		}

		public Button OkButton => okBtn;
		public new Button CancelButton => cancelBtn;
	}
}
