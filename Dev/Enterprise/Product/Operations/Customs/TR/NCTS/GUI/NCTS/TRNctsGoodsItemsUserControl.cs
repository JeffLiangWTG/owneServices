using System;
using Enterprise.Customs.EU.NCTS.GUI;

namespace Enterprise.Customs.TR.NCTS.GUI
{
	public partial class TRNctsGoodsItemsUserControl : NctsGoodsItemsUserControl
	{
		public TRNctsGoodsItemsUserControl()
		{
			InitializeComponent();
		}

		protected override Type GetItemDetailsUserControlType()
		{
			return typeof(TRItemDetailsUserControl);
		}

		protected override Type GetPreviousDocumentsUserControlType()
		{
			return typeof(NctsPreviousDocumentsUserControl);
		}
	}
}
