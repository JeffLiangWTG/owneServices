using Enterprise.ZArchitecture.GUI;
using ExportAWBRateLine = Enterprise.Freight.Forwarding.Business.AWB.ExportAWBRateLine;

namespace Enterprise.Freight.Forwarding.GUI.AWB
{
	public partial class NatureAndQtyOfGoodsControl : ZUserControl
	{
		public NatureAndQtyOfGoodsControl()
		{
			InitializeComponent();
		}

		protected new ExportAWBRateLine CurrentDataItem
		{
			get { return (ExportAWBRateLine)base.CurrentDataItem; }
		}
	}
}
