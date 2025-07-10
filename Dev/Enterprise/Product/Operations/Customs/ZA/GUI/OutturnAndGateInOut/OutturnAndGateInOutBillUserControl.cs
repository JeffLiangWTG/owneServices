using System.ComponentModel;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ZA.GUI
{
	public partial class OutturnAndGateInOutBillUserControl : ZUserControl
	{
		public OutturnAndGateInOutBillUserControl()
		{
			InitializeComponent();

#if DEBUG
			TypeDescriptor.AddAttributes(zDropEditCustomsStatus, new SuppressFormsLocalizedTestAttribute());
#endif
		}
	}
}
