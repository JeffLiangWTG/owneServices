using System.ComponentModel;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Agency.GUI
{
	public partial class CombineBookingsControl : ZUserControl
	{
		public CombineBookingsControl()
		{
			InitializeComponent();
#if DEBUG
			TypeDescriptor.AddAttributes(shipmentsGrid, new SuppressFormsLocalizedTestAttribute());
#endif
		}
	}
}
