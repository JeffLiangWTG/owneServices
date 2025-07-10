using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Manifest.Business
{
	public class N5101HGoodsMeasure : IN5101HGoodsMeasure
	{
		public N5101HGoodsMeasure(AsycudaBill bill)
		{
			this.bill = bill;
		}

		readonly AsycudaBill bill;

		public ZDecimal GrossVolumeMeasure => bill.ABL_Volume;

		public ZString VolumeUnitCode => bill.ABL_VolumeUQ;
	}
}
