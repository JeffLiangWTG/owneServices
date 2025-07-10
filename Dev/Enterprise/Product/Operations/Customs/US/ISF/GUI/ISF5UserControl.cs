using Enterprise.Customs.US.ISF.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.ISF.GUI
{
	public partial class ISF5UserControl : ZUserControl
	{
		public ISF5UserControl()
		{
			InitializeComponent();
		}

		public new CusISFHeader CurrentDataItem
		{
			get { return (CusISFHeader)base.CurrentDataItem; }
		}
	}
}
