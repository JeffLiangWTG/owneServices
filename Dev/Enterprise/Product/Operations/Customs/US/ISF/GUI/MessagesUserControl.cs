using Enterprise.Customs.US.ISF.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.ISF.GUI
{
	public partial class MessagesUserControl : ZUserControl
	{
		public MessagesUserControl()
		{
			InitializeComponent();
		}

		public new CusISFHeader CurrentDataItem
		{
			get { return (CusISFHeader)base.CurrentDataItem; }
		}
	}
}
