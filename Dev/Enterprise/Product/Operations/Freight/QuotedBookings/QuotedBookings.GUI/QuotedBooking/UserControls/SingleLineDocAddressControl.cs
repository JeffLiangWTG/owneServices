using Enterprise.MasterFiles.GUI;

namespace Enterprise.Freight.QuotedBookings.GUI
{
	public partial class SingleLineDocAddressControl : ZDocAddressControl
	{
		public SingleLineDocAddressControl()
		{
			InitializeComponent();
			CutDownSingleLineNoGroupBoxAddressDropEdit.PreBoundMaxLength = 11;
		}
	}
}
